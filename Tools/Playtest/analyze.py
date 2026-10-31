#!/usr/bin/env python3
"""Reads a Physics Battler playtest log (a folder under PlaytestLogs/) and writes a report with plots.

    python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_07-30-12 [--out DIR] [--frames 8]

Milestones 1 and 2 (animation-first rebuild): units stand in their combat stance on the board,
idle, step, turn, close in and strike at body parts. The questions this answers:
    - do the feet stay planted (no sliding) and step when they should?
    - is the weight over the feet, are the hips at the right height?
    - do the pinned parts (torso, head, arms) follow the animation pose, without shaking?
    - does each body size move in its own style (sway, bob, shuffles)?
    - does every blow reach the point it was aimed at, on the pose and with the physical weapon?
      What was actually there when it arrived (the target sways and steps)?

Writes into DIR (default: <session>/analysis/, which git ignores):
    report.md                         numbers per unit, flags, events, marks
    overview.png                      every unit: tracking error, weight offset, steps over time
    timeline_<unit>.png               one unit: pelvis height, feet (lift, slide, steps), balance, idle motion,
                                      tracking error per region, facing
    idle_<unit>_film.png              stick figures over the first seconds (side and top views)
    turn_<unit>_<n>_film.png          stick figures through each big turn
    blow_<unit>_<n>_film.png          stick figures of attacker and target through a blow, seen from its side
    blow_<unit>_<n>_pictures.jpg      the pictures through that blow
    mark_<n>_film.png                 stick figures around each mark (every unit)
    pictures_<nn>.jpg                 the recorded pictures of the game view, 24 per sheet (3 s at 8 per second),
                                      each captioned with its time and what every unit's feet were doing
    turn_…_pictures.jpg, mark_…_pictures.jpg   the pictures around those moments

Needs numpy, pandas, matplotlib and Pillow (for the pictures). The recorder's format is described in
Assets/_Battler/Scripts/Runtime/Diagnostics/PlaytestRecorder.cs and UnitTrack.cs.
"""

import argparse
import gzip
import io
import json
import math
import os
import re
import sys
import zlib
from dataclasses import dataclass, field

import numpy as np
import pandas as pd

import matplotlib

matplotlib.use("Agg")
import matplotlib.pyplot as plt  # noqa: E402

REGIONS = ["Legs", "Torso", "Head", "ArmL", "ArmR"]
LEVELS = {0: "Kinematic", 1: "Pinned", 2: "Loose", 3: "Ragdoll"}
REGION_PARTS = {
    "Legs": ["Pelvis", "ThighL", "ShinL", "FootL", "ThighR", "ShinR", "FootR"],
    "Torso": ["Spine", "Chest"],
    "Head": ["Head"],
    "ArmL": ["UpperArmL", "ForearmL", "HandL"],
    "ArmR": ["UpperArmR", "ForearmR", "HandR"],
}
REGION_COLORS = {"Legs": "#555555", "Torso": "#d1495b", "Head": "#edae49", "ArmL": "#00798c", "ArmR": "#30638e"}
TEAM_COLORS = ["#b3743a", "#3a6fb3"]

# Flags: what counts as a problem in milestone 1.
SLIDE_LIMIT = 0.01          # m a planted foot may slide
PINNED_ERROR_LIMIT = 0.05   # m a pinned part may lag its pose (95th percentile)
COM_LIMIT = 0.08            # share of leg length the weight may be off the support point
SHAKE_ALTERNATION = 0.75    # share of steps reversing the angular acceleration
SHAKE_RMS = 1.5             # rad/s


# ---- Reading --------------------------------------------------------------------------------------


def read_gz_csv(path):
    """Reads a gzipped CSV, tolerating a file cut short (Unity closed without finishing it)."""
    if not os.path.exists(path) or os.path.getsize(path) == 0:
        return pd.DataFrame()
    raw = open(path, "rb").read()
    try:
        text = gzip.decompress(raw)
    except (EOFError, OSError, zlib.error):
        d = zlib.decompressobj(16 + zlib.MAX_WBITS)
        text = d.decompress(raw)
        text = text[: text.rfind(b"\n") + 1]   # drop a half-written last line
    if not text.strip():
        return pd.DataFrame()
    return pd.read_csv(io.BytesIO(text), low_memory=False)


@dataclass
class Event:
    t: float
    step: int
    who: str
    cat: str
    text: str


def read_events(path):
    events = []
    if not os.path.exists(path):
        return events
    for line in open(path, encoding="utf-8", errors="replace"):
        if line.startswith("#") or not line.strip():
            continue
        parts = [p.strip() for p in line.rstrip("\n").split(" | ", 4)]
        if len(parts) < 5:
            continue
        try:
            events.append(Event(float(parts[0]), int(parts[1]), parts[2], parts[3], parts[4]))
        except ValueError:
            continue
    return events


@dataclass
class Unit:
    meta: dict
    label: str
    stem: str
    frames: pd.DataFrame
    pose: pd.DataFrame
    events: list = field(default_factory=list)


class Session:
    def __init__(self, path):
        self.path = path
        with open(os.path.join(path, "meta.json"), encoding="utf-8") as f:
            text = f.read()
        # Logs from before the recorder forced invariant number formatting could write "5,507" in marks.
        text = re.sub(r'"time": (\d+),(\d+)', r'"time": \1.\2', text)
        self.meta = json.loads(text)
        self.events = read_events(os.path.join(path, "events.log"))
        self.parts = [p["name"] for p in self.meta["parts"]]
        self.parent = {p["name"]: p["parent"] for p in self.meta["parts"]}
        self.units = []
        for u in self.meta["units"]:
            frames = read_gz_csv(os.path.join(path, u["frames"]))
            pose = read_gz_csv(os.path.join(path, u["pose"]))
            unit = Unit(u, u["label"], u["stem"], frames, pose)
            unit.events = [e for e in self.events if e.who == u["label"]]
            self.units.append(unit)
        self.world = read_gz_csv(os.path.join(path, "world.csv.gz"))
        self.dt = self.meta.get("fixedDeltaTime", 1 / 60)

    def unit(self, label):
        for u in self.units:
            if u.label == label:
                return u
        return None


def segments(series, times):
    """Runs of equal values: [(value, t0, t1, i0, i1)]."""
    out = []
    if len(series) == 0:
        return out
    values = series.to_numpy()
    start = 0
    for i in range(1, len(values) + 1):
        if i == len(values) or values[i] != values[start]:
            out.append((values[start], float(times[start]), float(times[i - 1]), start, i - 1))
            start = i
    return out


# ---- Geometry ---------------------------------------------------------------------------------------


def quat_rotate(q, v):
    """Rotates vectors v (N×3) by quaternions q (N×4, x y z w), Unity convention."""
    x, y, z, w = q[:, 0], q[:, 1], q[:, 2], q[:, 3]
    u = np.stack([x, y, z], axis=1)
    uv = np.cross(u, v)
    uuv = np.cross(u, uv)
    return v + 2.0 * (uv * w[:, None] + uuv)


def pose_points(session, unit, row, ghost=False):
    """{part: xyz} for one pose row (a pandas Series)."""
    pts = {}
    for p in session.parts:
        keys = (f"{p}.gx", f"{p}.gy", f"{p}.gz") if ghost else (f"{p}.x", f"{p}.y", f"{p}.z")
        if keys[0] in row and not pd.isna(row[keys[0]]):
            pts[p] = np.array([row[keys[0]], row[keys[1]], row[keys[2]]], dtype=float)
    return pts


def extremities(row, part, offset):
    q = np.array([[row[f"{part}.qx"], row[f"{part}.qy"], row[f"{part}.qz"], row[f"{part}.qw"]]], dtype=float)
    p = np.array([row[f"{part}.x"], row[f"{part}.y"], row[f"{part}.z"]], dtype=float)
    return p + quat_rotate(q, np.array([offset], dtype=float))[0]


def nearest_row(df, t):
    if df.empty:
        return None
    i = int(np.clip(np.searchsorted(df["t"].to_numpy(), t), 0, len(df) - 1))
    return df.iloc[i]


# ---- Drawing --------------------------------------------------------------------------------------


def draw_figure(ax, session, unit, row, axes_fn, color, ghost_color="#18b6d8", show_ghost=True, lw=2.2):
    pts = pose_points(session, unit, row)
    if not pts:
        return
    ghost = pose_points(session, unit, row, ghost=True) if show_ghost else {}

    def seg(a, b, c, w, style="-", alpha=1.0, z=2):
        pa, pb = axes_fn(a), axes_fn(b)
        ax.plot([pa[0], pb[0]], [pa[1], pb[1]], style, color=c, lw=w, alpha=alpha, zorder=z, solid_capstyle="round")

    for p, parent in session.parent.items():
        if parent and p in ghost and parent in ghost:
            seg(ghost[parent], ghost[p], ghost_color, 1.4, "--", 0.9, 1)
    for p, parent in session.parent.items():
        if parent and p in pts and parent in pts:
            seg(pts[parent], pts[p], color, lw)
    # Head, toes: from the parts' rotations.
    try:
        seg(pts["Head"], extremities(row, "Head", (0, 0.3, 0)), color, lw * 2.4)
        for foot in ("FootL", "FootR"):
            seg(pts[foot], extremities(row, foot, (0, -0.06, 0.2)), color, lw)
    except KeyError:
        pass
    # The weapon: physical (brown) and posed (dashed purple), and the locked aim point (red x).
    if "grip.x" in row and not pd.isna(row["grip.x"]):
        grip = np.array([row["grip.x"], row["grip.y"], row["grip.z"]], dtype=float)
        tip = np.array([row["tip.x"], row["tip.y"], row["tip.z"]], dtype=float)
        seg(grip, tip, "#8a5a2b", lw * 1.5, z=3)
        if show_ghost and not pd.isna(row.get("ghostGrip.x", np.nan)):
            g0 = np.array([row["ghostGrip.x"], row["ghostGrip.y"], row["ghostGrip.z"]], dtype=float)
            g1 = np.array([row["ghostTip.x"], row["ghostTip.y"], row["ghostTip.z"]], dtype=float)
            seg(g0, g1, "#c58bff", 1.2, "--", 0.9, 1)
    if "target.x" in row and not pd.isna(row["target.x"]):
        p = axes_fn(np.array([row["target.x"], row["target.y"], row["target.z"]], dtype=float))
        ax.plot(p[0], p[1], "x", color="red", ms=8, mew=2, zorder=5)
    # Where the stance has planted each foot (the anim pose's ground spots).
    for side in ("L", "R"):
        if f"plant{side}.x" in row and not pd.isna(row[f"plant{side}.x"]):
            p = axes_fn(np.array([row[f"plant{side}.x"], 0.0, row[f"plant{side}.z"]]))
            ax.plot(p[0], p[1], "s", color="#e0b000", ms=3.5, zorder=4)


def view_axes(forward):
    """(side view, top view) projection functions for a horizontal 'forward' direction."""
    f = np.array([forward[0], 0.0, forward[2]], dtype=float)
    n = np.linalg.norm(f)
    f = f / n if n > 1e-6 else np.array([1.0, 0.0, 0.0])
    right = np.array([f[2], 0.0, -f[0]])   # Unity: right = up × forward... for +Z forward, right is +X

    def side(p):
        return np.array([np.dot(p, f), p[1]])

    def top(p):
        return np.array([np.dot(p, f), -np.dot(p, right)])

    return side, top


def filmstrip(session, times, forward, title, path, focus_units=None, subtitle_fn=None, only=False):
    side, top = view_axes(forward)
    units = [u for u in session.units if not u.pose.empty]
    if focus_units:
        units = [u for u in units if u.label in focus_units] + ([] if only else [u for u in units if u.label not in focus_units])
    n = len(times)
    fig, axs = plt.subplots(2, n, figsize=(2.3 * n, 6.2), squeeze=False)
    palette = ["#202020", "#1f6f3f", "#9a5b00", "#7a3b9a"]
    # Shared limits from everything drawn.
    xs, ys, zs = [], [], []
    for t in times:
        for u in units:
            r = nearest_row(u.pose, t)
            if r is None or abs(r["t"] - t) > 0.1:
                continue
            pts = list(pose_points(session, u, r).values())
            if "tip.x" in r and not pd.isna(r["tip.x"]):
                pts.append(np.array([r["tip.x"], r["tip.y"], r["tip.z"]], dtype=float))
            for pt in pts:
                s, tp = side(pt), top(pt)
                xs.append(s[0]); ys.append(s[1]); zs.append(tp[1])
    if not xs:
        plt.close(fig)
        return False
    x0, x1 = min(xs) - 0.4, max(xs) + 0.4
    y1 = max(ys) + 0.3
    z0, z1 = min(zs) - 0.4, max(zs) + 0.4
    for k, t in enumerate(times):
        for row_i, fn in enumerate((side, top)):
            ax = axs[row_i][k]
            ax.set_xlim(x0, x1)
            if row_i == 0:
                ax.set_ylim(-0.05, y1)
                ax.axhline(0, color="#999", lw=1)
            else:
                ax.set_ylim(z0, z1)
            ax.set_aspect("equal")
            ax.set_xticks([]); ax.set_yticks([])
            for ui, u in enumerate(units):
                r = nearest_row(u.pose, t)
                if r is None or abs(r["t"] - t) > 0.1:
                    continue
                draw_figure(ax, session, u, r, fn, TEAM_COLORS[int(u.meta.get("team", ui)) % 2] if not only else palette[ui % len(palette)])
            if row_i == 0:
                label = f"{t:.2f} s"
                if subtitle_fn:
                    label += "\n" + subtitle_fn(t)
                ax.set_title(label, fontsize=7.5)
    axs[0][0].set_ylabel("side", fontsize=9)
    axs[1][0].set_ylabel("top", fontsize=9)
    fig.suptitle(title + "   (solid: physical body, dashed cyan: animation pose, brown: weapon, dashed purple: posed weapon, red x: aim point, yellow squares: planted feet)", fontsize=9)
    fig.tight_layout(rect=(0, 0, 1, 0.95))
    fig.savefig(path, dpi=110)
    plt.close(fig)
    return True


def mark_lines(ax, session, t0, t1):
    for m in session.meta.get("marks", []):
        if t0 <= m["time"] <= t1:
            ax.axvline(m["time"], color="black", lw=1, ls=":")
            ax.text(m["time"], ax.get_ylim()[1], f" M{m['n']}", fontsize=7, va="top")


def _qmul(a, b):
    x1, y1, z1, w1 = a.T
    x2, y2, z2, w2 = b.T
    return np.stack([w1 * x2 + x1 * w2 + y1 * z2 - z1 * y2, w1 * y2 - x1 * z2 + y1 * w2 + z1 * x2,
                     w1 * z2 + x1 * y2 - y1 * x2 + z1 * w2, w1 * w2 - x1 * x2 - y1 * y2 - z1 * z2], 1)


def jitter(session, unit, mask):
    """Per part, over the masked pose rows: rms angular speed (rad/s), and the share of steps whose change in
    angular velocity reverses the previous one. Near 1.0 means shaking at the physics rate (30 Hz)."""
    p = unit.pose
    if p.empty or unit.frames.empty:
        return {}
    m = p.merge(unit.frames[["t"]].assign(_keep=mask.to_numpy()), on="t")
    m = m[m["_keep"]]
    out = {}
    if len(m) < 12:
        return out
    t = m["t"].to_numpy()
    contiguous = np.diff(t) < session.dt * 1.5
    for part in session.parts:
        q = m[[f"{part}.qx", f"{part}.qy", f"{part}.qz", f"{part}.qw"]].to_numpy(dtype=float)
        inv = q[:-1].copy()
        inv[:, :3] *= -1
        d = _qmul(inv, q[1:])
        d[d[:, 3] < 0] *= -1
        v = d[:, :3]
        s = np.linalg.norm(v, axis=1)
        ang = 2 * np.arctan2(s, d[:, 3])
        with np.errstate(invalid="ignore", divide="ignore"):
            w = np.where(s[:, None] > 1e-9, v / s[:, None], 0) * (ang / session.dt)[:, None]
        w = w[contiguous]
        if len(w) < 4:
            continue
        dw = np.diff(w, axis=0)
        alt = float((np.einsum("ij,ij->i", dw[:-1], dw[1:]) < 0).mean())
        out[part] = (float(np.sqrt((w ** 2).sum(1).mean())), alt)
    return out


def events_between(unit, t0, t1, cats=None):
    return [e for e in unit.events if t0 - 1e-6 <= e.t <= t1 + 1e-6 and (cats is None or e.cat in cats)]


def read_pictures(session):
    path = os.path.join(session.path, "frames", "frames.csv")
    if not os.path.exists(path):
        return pd.DataFrame()
    try:
        df = pd.read_csv(path)
    except Exception:
        return pd.DataFrame()
    df = df[[os.path.exists(os.path.join(session.path, "frames", f)) for f in df["file"]]]
    return df.sort_values("t").reset_index(drop=True)


def picture_sheet(session, pics, path, title, cols=6, flip=False, max_count=24):
    from PIL import Image, ImageDraw, ImageFont
    if pics.empty:
        return False
    if len(pics) > max_count:
        idx = np.linspace(0, len(pics) - 1, max_count).round().astype(int)
        pics = pics.iloc[sorted(set(idx))]
    first = Image.open(os.path.join(session.path, "frames", pics["file"].iloc[0]))
    w, h = first.size
    cap = 16
    rows = int(math.ceil(len(pics) / cols))
    head = 22
    sheet = Image.new("RGB", (cols * w, head + rows * (h + cap)), (24, 24, 24))
    draw = ImageDraw.Draw(sheet)
    try:
        font = ImageFont.load_default(size=12)
        big = ImageFont.load_default(size=15)
    except TypeError:
        font = big = ImageFont.load_default()
    draw.text((6, 3), title, fill=(240, 240, 240), font=big)
    for k, (_, row) in enumerate(pics.iterrows()):
        img = Image.open(os.path.join(session.path, "frames", row["file"])).convert("RGB")
        if flip:
            img = img.transpose(Image.FLIP_TOP_BOTTOM)
        x, y = (k % cols) * w, head + (k // cols) * (h + cap)
        sheet.paste(img, (x, y))
        draw.text((x + 3, y + h + 1), picture_caption(session, row["t"]), fill=(230, 230, 120), font=font)
    sheet.save(path, quality=85)
    return True


def picture_sheets(session, out, args):
    pics = read_pictures(session)
    made = []
    if pics.empty:
        return pics, made
    per = 24
    for n, start in enumerate(range(0, len(pics), per), 1):
        chunk = pics.iloc[start:start + per]
        name = f"pictures_{n:02d}.jpg"
        if picture_sheet(session, chunk, os.path.join(out, name), f"pictures {chunk['t'].iloc[0]:.2f}-{chunk['t'].iloc[-1]:.2f} s", flip=args.flip_pictures):
            made.append((name, chunk["t"].iloc[0], chunk["t"].iloc[-1]))
    return pics, made


def pictures_between(pics, t0, t1):
    if pics.empty:
        return pics
    return pics[(pics["t"] >= t0) & (pics["t"] <= t1)]


# ---- Helpers --------------------------------------------------------------------------------------


def fmt(x, digits=2):
    if x is None or (isinstance(x, float) and (math.isnan(x) or math.isinf(x))):
        return "-"
    return f"{x:.{digits}f}"


def col(f, name, default=np.nan):
    return f[name].astype(float) if name in f else pd.Series(default, index=f.index, dtype=float)


def leg_length(unit):
    return float(unit.meta.get("legLength") or 0.845)


def region_error(f, region):
    """Per frame, the largest distance (m) of the region's parts from their pose."""
    cols = [f"err_{p}" for p in REGION_PARTS[region] if f"err_{p}" in f]
    if not cols:
        return pd.Series(np.nan, index=f.index)
    return f[cols].astype(float).max(axis=1)


def region_angle(f, region):
    cols = [f"aerr_{p}" for p in REGION_PARTS[region] if f"aerr_{p}" in f]
    if not cols:
        return pd.Series(np.nan, index=f.index)
    return f[cols].astype(float).max(axis=1)


def stepping_mask(f):
    return (col(f, "stepL", 0) > 0.5) | (col(f, "stepR", 0) > 0.5)


def turns(unit, min_degrees=45.0, gap=0.4):
    """Spans where the unit turned a lot: [(i0, i1, degrees)]."""
    f = unit.frames
    if f.empty or "rootYaw" not in f:
        return []
    yaw = np.unwrap(np.radians(f["rootYaw"].astype(float).to_numpy()))
    rate = np.abs(np.gradient(yaw)) / max(1e-6, float(np.median(np.diff(f["t"].to_numpy())) or 1 / 60))
    moving = rate > math.radians(20)
    out = []
    i = 0
    n = len(moving)
    while i < n:
        if not moving[i]:
            i += 1
            continue
        j = i
        last = i
        while j < n and (moving[j] or f["t"].iloc[j] - f["t"].iloc[last] < gap):
            if moving[j]:
                last = j
            j += 1
        degrees = math.degrees(abs(yaw[last] - yaw[i]))
        if degrees >= min_degrees:
            out.append((i, last, degrees))
        i = j
    return out


BLOW_START = re.compile(r"^(\w+) at (.+)'s (\w+): aim locked at \(([-\d.]+), ([-\d.]+), ([-\d.]+)\), lunge ([-\d.]+) m, ([\d.]+) s, contact at ([\d.]+) s")
BLOW_CONTACT = re.compile(r"^(\w+) at (.+)'s (\w+): passed ([\d.]+) cm from the aim point; (?:nearest part (.+)'s (\w+) \(([\d.]+) cm\)|nothing near); the \w+ had moved ([\d.]+) cm")
BLOW_DONE = re.compile(r"^(\w+) done: (?:the physical weapon passed ([\d.]+) cm|no physical weapon)")


def blows(unit):
    """The unit's blows, from its events: dicts with t, move, target, part, lunge, duration, contact time, and what happened."""
    out = []
    current = None
    for e in unit.events:
        if e.cat == "attack":
            m = BLOW_START.match(e.text)
            if m:
                current = dict(t=e.t, move=m.group(1), target=m.group(2), part=m.group(3),
                               aim=np.array([float(m.group(4)), float(m.group(5)), float(m.group(6))]),
                               lunge=float(m.group(7)), duration=float(m.group(8)), tc=float(m.group(9)),
                               err=None, struck=None, struckPart=None, struckCm=None, moved=None, physical=None, touches=[])
                out.append(current)
                continue
            m = BLOW_DONE.match(e.text)
            if m and current is not None and current["move"] == m.group(1):
                current["physical"] = float(m.group(2)) if m.group(2) else None
                current = None
        elif e.cat == "contact" and current is not None:
            m = BLOW_CONTACT.match(e.text)
            if m:
                current["err"] = float(m.group(4))
                current["struck"] = m.group(5)
                current["struckPart"] = m.group(6)
                current["struckCm"] = float(m.group(7)) if m.group(7) else None
                current["moved"] = float(m.group(8))
        elif e.cat == "touch" and current is not None:
            current["touches"].append(e.text)
    return out


PART_OF = {"Head": "Head", "Neck": "Head", "Chest": "Chest", "Belly": "Spine", "ElbowL": "ForearmL", "ElbowR": "ForearmR",
           "KneeL": "ShinL", "KneeR": "ShinR"}


def short_label(unit):
    return unit.label.replace(" ", "")


def picture_caption(session, t):
    parts = [f"{t:.2f}s"]
    for unit in session.units:
        f = unit.frames
        if f.empty or t < f["t"].iloc[0] - 0.05 or t > f["t"].iloc[-1] + 0.05:
            continue
        r = nearest_row(f, t)
        stepping = (float(r.get("stepL", 0)) > 0.5) or (float(r.get("stepR", 0)) > 0.5)
        move = r.get("move", "")
        what = f"{move}" if isinstance(move, str) and move else ("step" if stepping else "-")
        parts.append(f"{short_label(unit)}:{what}")
    return " ".join(parts)


def step_spans(ax, f):
    t = f["t"].to_numpy()
    for side, color in (("L", "#8fb6e8"), ("R", "#f2b36b")):
        if f"step{side}" not in f:
            continue
        for value, t0, t1, _, _ in segments(f[f"step{side}"].astype(float) > 0.5, t):
            if value:
                ax.axvspan(t0, t1 + 1 / 60, color=color, alpha=0.25, lw=0)


def plot_timeline(session, unit, path):
    f = unit.frames
    t = f["t"].to_numpy()
    leg = leg_length(unit)
    fig, axs = plt.subplots(7, 1, figsize=(12, 17), sharex=True)

    ax = axs[0]
    ax.plot(t, col(f, "pelvisH"), color="black", lw=1.4, label="pelvis height (physical)")
    ax.plot(t, col(f, "animPelvisH"), color="#18b6d8", lw=1, ls="--", label="pelvis height (pose)")
    ax.plot(t, unit.meta.get("restPelvisHeight", np.nan) - col(f, "pelvisDrop"), color="#999", lw=0.8, ls=":", label="rest height (crouch and bob lower it)")
    ax.set_ylabel("m")
    ax.legend(fontsize=7, loc="lower left")
    ax.set_title(f"{unit.label}: pelvis", fontsize=9)

    ax = axs[1]
    step_spans(ax, f)
    ax.plot(t, col(f, "liftL") * 100, color="#30638e", lw=1, label="left foot lift")
    ax.plot(t, col(f, "liftR") * 100, color="#b3743a", lw=1, label="right foot lift")
    ax.plot(t, col(f, "slideL") * 100, color="#30638e", lw=1, ls=":", label="left slide while planted")
    ax.plot(t, col(f, "slideR") * 100, color="#b3743a", lw=1, ls=":", label="right slide while planted")
    ax.axhline(SLIDE_LIMIT * 100, color="red", lw=0.6, ls="--")
    ax.set_ylabel("cm")
    ax.legend(fontsize=7, loc="upper left", ncol=2)
    ax.set_title("feet (shaded: stepping, blue left / orange right)", fontsize=9)

    ax = axs[2]
    ax.plot(t, col(f, "comOffset") * 100, color="black", lw=1.2, label="pose weight off the support point")
    ax.plot(t, col(f, "physCom") * 100, color="#d1495b", lw=1, label="physical weight off the support point")
    ax.plot(t, col(f, "weightShift") * 100, color="#999", lw=0.8, ls="--", label="weight shift applied")
    ax.axhline(COM_LIMIT * leg * 100, color="red", lw=0.6, ls="--")
    ax.set_ylabel("cm")
    ax.legend(fontsize=7, loc="upper left")
    ax.set_title("balance", fontsize=9)

    ax = axs[3]
    ax.plot(t, col(f, "swayX") * 100, lw=1, label="sway side")
    ax.plot(t, col(f, "swayZ") * 100, lw=1, label="sway front/back")
    ax.plot(t, col(f, "bob") * 100, lw=1, label="bob")
    ax.set_ylabel("cm")
    ax.legend(fontsize=7, loc="upper left", ncol=3)
    ax.set_title("combat idle", fontsize=9)

    ax = axs[4]
    for region in REGIONS:
        ax.plot(t, region_error(f, region) * 100, color=REGION_COLORS[region], lw=1, label=region)
    ax.axhline(PINNED_ERROR_LIMIT * 100, color="red", lw=0.6, ls="--")
    ax.set_ylabel("cm")
    ax.legend(fontsize=7, loc="upper left", ncol=5)
    ax.set_title("physical parts against the pose (largest per region)", fontsize=9)

    ax = axs[5]
    if "move" in f:
        moving = f["move"].fillna("").astype(str)
        for value, t0, t1, _, _ in segments(moving, t):
            if value:
                ax.axvspan(t0, t1 + 1 / 60, color="#f2b36b", alpha=0.25, lw=0)
                ax.text(t0, 0.98, value, fontsize=6, va="top", transform=ax.get_xaxis_transform())
    ax.plot(t, col(f, "aimErr") * 100, color="#d1495b", lw=1, label="posed striking part to aim point")
    ax.plot(t, col(f, "blend") * 10, color="#999", lw=0.8, ls="--", label="blend x10")
    ax.set_ylabel("cm")
    ax.set_ylim(bottom=0)
    ax.legend(fontsize=7, loc="upper right")
    ax.set_title("blows (shaded): the posed striking part's distance from the locked aim point", fontsize=9)

    ax = axs[6]
    ax.plot(t, np.degrees(np.unwrap(np.radians(col(f, "rootYaw").to_numpy()))), color="black", lw=1)
    ax.set_ylabel("facing °")
    ax.set_xlabel("time (s)")
    ax2 = ax.twinx()
    ax2.plot(t, col(f, "steps"), color="#30638e", lw=1, label="steps")
    ax2.plot(t, col(f, "shuffles"), color="#b3743a", lw=1, label="idle shuffles")
    ax2.legend(fontsize=7, loc="lower right")
    ax.set_title("facing, steps", fontsize=9)
    for a in axs:
        mark_lines(a, session, t[0], t[-1])
        a.grid(alpha=0.25)
    fig.tight_layout()
    fig.savefig(path, dpi=100)
    plt.close(fig)


def plot_overview(session, path):
    units = [u for u in session.units if not u.frames.empty]
    if not units:
        return
    fig, axs = plt.subplots(3, 1, figsize=(12, 9), sharex=True)
    for k, u in enumerate(units):
        f = u.frames
        t = f["t"].to_numpy()
        color = TEAM_COLORS[int(u.meta.get("team", 0)) % 2]
        ls = ["-", "--", ":"][k % 3]
        pinned = pd.concat([region_error(f, r) for r in ("Torso", "Head", "ArmL", "ArmR")], axis=1).max(axis=1)
        axs[0].plot(t, pinned * 100, color=color, ls=ls, lw=1, label=u.label)
        axs[1].plot(t, col(f, "comOffset") / leg_length(u) * 100, color=color, ls=ls, lw=1, label=u.label)
        axs[2].plot(t, col(f, "steps") + col(f, "shuffles"), color=color, ls=ls, lw=1, label=u.label)
    axs[0].set_ylabel("cm"); axs[0].set_title("largest pinned-part error", fontsize=9)
    axs[1].set_ylabel("% of leg"); axs[1].set_title("pose weight off the support point", fontsize=9)
    axs[2].set_ylabel("count"); axs[2].set_title("steps + idle shuffles", fontsize=9)
    axs[2].set_xlabel("time (s)")
    axs[0].legend(fontsize=7, ncol=3)
    for a in axs:
        mark_lines(a, session, 0, 1e9)
        a.grid(alpha=0.25)
    fig.tight_layout()
    fig.savefig(path, dpi=100)
    plt.close(fig)


def facing(unit, t):
    r = nearest_row(unit.frames, t)
    yaw = math.radians(float(r["rootYaw"])) if r is not None else 0.0
    return np.array([math.sin(yaw), 0, math.cos(yaw)])


def side_view(unit, t):
    """The film strips' horizontal axis for a view from the unit's side: its facing, so the stagger shows."""
    return facing(unit, t)


# ---- Report ---------------------------------------------------------------------------------------


def unit_numbers(session, unit):
    f = unit.frames
    leg = leg_length(unit)
    n = {}
    n["steps"] = int(col(f, "steps").max()) if "steps" in f else 0
    n["shuffles"] = int(col(f, "shuffles").max()) if "shuffles" in f else 0
    n["slide"] = float(max(col(f, "slideL").max(), col(f, "slideR").max()))
    idle = f["move"].fillna("").astype(str) == "" if "move" in f else pd.Series(True, index=f.index)
    n["com"] = float(col(f, "comOffset")[idle].max())
    n["com95"] = float(col(f, "comOffset")[idle].quantile(0.95))
    n["physCom95"] = float(col(f, "physCom")[idle].quantile(0.95))
    n["drop"] = float(col(f, "pelvisDrop").max())
    n["dropIdle"] = float(col(f, "pelvisDrop")[idle].max())
    n["pelvisGap"] = float((col(f, "pelvisH") - col(f, "animPelvisH")).abs().max())
    n["sway"] = float(np.hypot(col(f, "swayX"), col(f, "swayZ")).max())
    n["bob"] = float(-col(f, "bob").min())
    n["leg"] = leg
    stepping = stepping_mask(f)
    n["stepShare"] = float(stepping.mean())
    regions = {}
    for r in REGIONS:
        e = region_error(f, r)
        a = region_angle(f, r)
        lvl = col(f, f"lvl_{r}")
        regions[r] = dict(level=LEVELS.get(int(lvl.mode().iloc[0]) if lvl.notna().any() else -1, "?"),
                          pin=float(col(f, f"pin_{r}").median()),
                          mean=float(e.mean()), p95=float(e.quantile(0.95)), max=float(e.max()),
                          amean=float(a.mean()), ap95=float(a.quantile(0.95)),
                          step95=float(e[stepping].quantile(0.95)) if stepping.any() else np.nan)
    n["regions"] = regions
    still = ~stepping & (f["t"] > f["t"].iloc[0] + 0.5)   # not the spawn and equip
    if "move" in f:
        still &= f["move"].fillna("").astype(str) == ""
    n["jitter"] = jitter(session, unit, still) if still.sum() >= 12 else {}
    n["blows"] = blows(unit)
    return n


def flags_for(unit, n):
    out = []
    leg = n["leg"]
    if n["slide"] > SLIDE_LIMIT:
        out.append(f"a planted foot slid {n['slide'] * 100:.1f} cm")
    if n["com95"] > COM_LIMIT * leg:
        out.append(f"the pose's weight was {n['com95'] * 100:.0f} cm off the support point (95th pct; limit {COM_LIMIT * leg * 100:.0f})")
    if n["physCom95"] > 1.5 * COM_LIMIT * leg:
        out.append(f"the physical body's weight was {n['physCom95'] * 100:.0f} cm off the support point (95th pct)")
    if n["dropIdle"] > 0.05 * leg:
        out.append(f"hips dropped {n['dropIdle'] * 100:.0f} cm to reach the feet out of blows (stance too wide or long for the legs?)")
    for r, d in n["regions"].items():
        if d["level"] == "Kinematic":
            if d["max"] > 0.005:
                out.append(f"kinematic {r} missed its pose by up to {d['max'] * 100:.1f} cm")
        elif d["level"] == "Pinned" and d["p95"] > PINNED_ERROR_LIMIT:
            out.append(f"pinned {r} lagged its pose: {d['p95'] * 100:.1f} cm (95th pct), {d['ap95']:.0f}°")
    bl = n.get("blows", [])
    missed = [b for b in bl if b["err"] is not None and b["err"] > 3.0]
    if missed:
        out.append(f"{len(missed)} blows passed more than 3 cm from their aim point on the pose")
    unreached = [b for b in bl if b["err"] is None]
    if unreached:
        out.append(f"{len(unreached)} blows never reached their contact moment")
    pulled = [b for b in bl if b["physical"] is not None and b["physical"] > 8.0]
    if pulled:
        out.append(f"{len(pulled)} blows where the physical weapon passed more than 8 cm from the aim point (physics pulled it off)")
    shaking = [p for p, (rms, alt) in n["jitter"].items() if alt > SHAKE_ALTERNATION and rms > SHAKE_RMS]
    if shaking:
        out.append("shaking at the physics rate: " + ", ".join(shaking))
    if n["steps"] + n["shuffles"] == 0 and len(unit.frames) > 600:
        out.append("never stepped or shuffled (units should never stand still)")
    return out


def struck_aimed(b):
    return b["struck"] == b["target"] and b["struckPart"] == PART_OF.get(b["part"]) and (b["struckCm"] or 99) < 5.0


def blow_summary(bl):
    if not bl:
        return "-"
    landed = sum(1 for b in bl if b["err"] is not None and b["err"] <= 3.0)
    aimed = sum(1 for b in bl if struck_aimed(b))
    return f"{len(bl)} ({landed} / {aimed})"


def write_report(session, out, args, films):
    lines = []
    w = lines.append
    m = session.meta
    duration = (m.get("endTime") or (session.events[-1].t if session.events else 0)) - m.get("startTime", 0)
    w(f"# Playtest {m['session']} — analysis")
    w("")
    w(f"Unity {m.get('unity')}, physics {1 / session.dt:.0f} Hz, {duration:.1f} s recorded, {len(session.events)} events, "
      f"{len(m.get('marks', []))} marks{'' if m.get('endTime') else ' (the session did not end cleanly; data may be cut short)'}.")
    w("")
    problems = [e for e in session.events if e.cat == "console" and (e.text.startswith("ERROR") or e.text.startswith("EXCEPTION") or e.text.lower().startswith("warning"))]
    if problems:
        w(f"**Console:** {len(problems)} warnings/errors. First: `{problems[0].text[:200]}`")
        w("")

    numbers = {}
    w("## Summary")
    w("")
    w("| unit | leg | steps | shuffles | stepping | worst slide | weight off support out of blows (pose 95% / physical 95%) | hip drop | sway / bob | pinned error 95% (torso, head, arms) | blows (landed on the pose / struck the part aimed at) | flags |")
    w("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |")
    for unit in session.units:
        if unit.frames.empty:
            continue
        n = numbers[unit.label] = unit_numbers(session, unit)
        fl = flags_for(unit, n)
        rg = n["regions"]
        pinned = ", ".join(f"{rg[r]['p95'] * 100:.1f}" for r in ("Torso", "Head", "ArmL", "ArmR"))
        w(f"| {unit.label} | {n['leg']:.2f} m | {n['steps']} | {n['shuffles']} | {n['stepShare'] * 100:.0f}% | {n['slide'] * 100:.1f} cm | "
          f"{n['com95'] * 100:.1f} / {n['physCom95'] * 100:.1f} cm | {n['drop'] * 100:.1f} cm | {n['sway'] * 100:.1f} / {n['bob'] * 100:.1f} cm | "
          f"{pinned} cm | {blow_summary(n['blows'])} | {len(fl) or 'none'} |")
    w("")

    for unit in session.units:
        f = unit.frames
        w(f"## {unit.label}")
        w("")
        if f.empty:
            w("No frames recorded.")
            w("")
            continue
        n = numbers[unit.label]
        t = f["t"].to_numpy()
        w(f"{len(f)} frames, {t[0]:.2f}–{t[-1]:.2f} s. Mass {fmt(unit.meta.get('mass'), 0)} kg, leg {n['leg']:.2f} m, "
          f"rest pelvis height {fmt(unit.meta.get('restPelvisHeight'))} m, profile {unit.meta.get('profile', '?')}.")
        w("")
        fl = flags_for(unit, n)
        w("**Flags:** " + ("; ".join(fl) if fl else "none."))
        w("")
        w("| region | authority | distance from pose mean / 95% / max (cm) | while stepping 95% | angle mean / 95% (°) |")
        w("| --- | --- | --- | --- | --- |")
        for r, d in n["regions"].items():
            level = d["level"] + (f" {d['pin']:.2f}" if d["level"] == "Pinned" else "")
            w(f"| {r} | {level} | {d['mean'] * 100:.1f} / {d['p95'] * 100:.1f} / {d['max'] * 100:.1f} | {fmt(d['step95'] * 100, 1)} | {d['amean']:.0f} / {d['ap95']:.0f} |")
        w("")
        if n["jitter"]:
            w("**Shaking while planted** (rms angular speed rad/s / share of steps reversing; above "
              f"{SHAKE_ALTERNATION} with rms above {SHAKE_RMS} is shaking): "
              + ", ".join(f"{p} {rms:.1f}/{alt:.2f}" for p, (rms, alt) in n["jitter"].items()))
            w("")
        tr = turns(unit)
        if tr:
            w("**Turns:** " + "; ".join(f"{f['t'].iloc[i0]:.2f}–{f['t'].iloc[i1]:.2f} s, {deg:.0f}°, "
                                        f"{len(events_between(unit, f['t'].iloc[i0], f['t'].iloc[i1] + 0.5, {'step'}))} steps" for i0, i1, deg in tr))
            w("")
        if n["blows"]:
            w("**Blows** (aim error: the posed striking part's closest pass to the locked aim point at contact; physical: the weapon's "
              "closest pass in the active window; moved: how far the aimed part had moved since the aim was locked; struck: the "
              "body part nearest the striking part at contact)")
            w("")
            w("| t | move | at | lunge | aim error | physical | moved | struck | touched |")
            w("| --- | --- | --- | --- | --- | --- | --- | --- | --- |")
            for b in n["blows"]:
                struck = f"{b['struck']}'s {b['struckPart']} ({fmt(b['struckCm'], 1)} cm)" if b["struck"] else "-"
                mark = " ✓" if struck_aimed(b) else ""
                touched = "; ".join(t.split(" touched ", 1)[-1] for t in b["touches"][:3]) or "-"
                w(f"| {b['t']:.2f} | {b['move']} | {b['target']}'s {b['part']} | {b['lunge']:.2f} m | {fmt(b['err'], 1)} cm | "
                  f"{fmt(b['physical'], 1)} cm | {fmt(b['moved'], 1)} cm | {struck}{mark} | {touched} |")
            w("")

        steps = [e for e in unit.events if e.cat == "step"]
        if steps:
            far = [e for e in steps if re.search(r"(\d+) cm from its spot", e.text) and int(re.search(r"(\d+) cm from its spot", e.text).group(1)) > 10]
            w(f"**Steps:** {len(steps)} logged; {len(far)} landed more than 10 cm from where the stance wants the foot now "
              "(normal while turning: the spot moves during the step).")
            w("")
        other = [e for e in unit.events if e.cat not in ("step", "spawn", "attack", "contact", "touch")]
        if other:
            w("**Other events**")
            w("")
            for e in other[:40]:
                w(f"- {e.t:7.2f} s  [{e.cat}] {e.text}")
            w("")

    if session.meta.get("marks"):
        w("## Marks")
        w("")
        for mk in session.meta["marks"]:
            w(f"- **mark {mk['n']}** at {mk['time']:.2f} s {mk.get('note', '')}")
            for e in session.events:
                if e.cat == "mark" and abs(e.t - mk["time"]) < 0.02:
                    w(f"    - {e.who}: {e.text}")
        w("")

    pics = read_pictures(session)
    if not pics.empty:
        w("## Pictures of the game view")
        w("")
        w(f"{len(pics)} pictures, {pics['t'].iloc[0]:.2f}–{pics['t'].iloc[-1]:.2f} s. Sheets of 24 (`pictures_NN.jpg`), and around each turn "
          "and mark (`*_pictures.jpg`). Captions: time, then for each unit `step` when a foot is in the air.")
        w("")
    w("## Images")
    w("")
    for name in sorted(os.listdir(out)):
        if name.endswith(".png") or name.endswith(".jpg"):
            w(f"- `{name}`")
    with open(os.path.join(out, "report.md"), "w", encoding="utf-8") as fh:
        fh.write("\n".join(lines) + "\n")


# ---- Main -----------------------------------------------------------------------------------------


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("session", help="a folder under PlaytestLogs/ (or PlaytestLogs/ itself for the newest)")
    ap.add_argument("--out", help="output folder (default <session>/analysis)")
    ap.add_argument("--frames", type=int, default=8, help="stick figures per film strip")
    ap.add_argument("--max-turns", type=int, default=3, help="film strips of at most this many turns per unit")
    ap.add_argument("--max-blows", type=int, default=4, help="film strips of at most this many blows per unit")
    ap.add_argument("--flip-pictures", action="store_true", help="turn the recorded pictures upside down (if they were read back flipped)")
    args = ap.parse_args()

    path = args.session.rstrip("/\\")
    if not os.path.exists(os.path.join(path, "meta.json")):
        subs = sorted(d for d in os.listdir(path) if os.path.exists(os.path.join(path, d, "meta.json")))
        if not subs:
            sys.exit(f"No playtest session in {path}")
        path = os.path.join(path, subs[-1])
    session = Session(path)
    out = args.out or os.path.join(path, "analysis")
    os.makedirs(out, exist_ok=True)
    pics, _ = picture_sheets(session, out, args)
    plot_overview(session, os.path.join(out, "overview.png"))
    films = []

    for unit in session.units:
        f = unit.frames
        if f.empty:
            continue
        plot_timeline(session, unit, os.path.join(out, f"timeline_{unit.stem}.png"))

        def sub(tt, f=f):
            r = nearest_row(f, tt)
            feet = "".join(s for s in ("L", "R") if float(r.get(f"step{s}", 0)) > 0.5) or "-"
            return f"step {feet} yaw {float(r['rootYaw']):.0f}°"

        t0 = float(f["t"].iloc[0])
        times = list(np.linspace(t0 + 0.1, min(t0 + 3.0, float(f["t"].iloc[-1])), args.frames))
        if filmstrip(session, times, side_view(unit, t0 + 0.1), f"{unit.label}: idle, first 3 s (seen from its right)",
                     os.path.join(out, f"idle_{unit.stem}_film.png"), focus_units=[unit.label], only=True, subtitle_fn=sub):
            films.append(f"idle_{unit.stem}_film.png")
        for n, (i0, i1, deg) in enumerate(turns(unit)[: args.max_turns], 1):
            a, b = float(f["t"].iloc[i0]), float(f["t"].iloc[i1]) + 0.4
            times = list(np.linspace(a - 0.1, b, args.frames))
            name = f"turn_{unit.stem}_{n}"
            filmstrip(session, times, side_view(unit, a), f"{unit.label}: turn #{n}, {deg:.0f}° (seen from its starting right)",
                      os.path.join(out, name + "_film.png"), focus_units=[unit.label], only=True, subtitle_fn=sub)
            picture_sheet(session, pictures_between(pics, a - 0.3, b + 0.2), os.path.join(out, name + "_pictures.jpg"),
                          f"{unit.label} turn #{n}, {a:.2f}-{b:.2f} s", flip=args.flip_pictures)

        for n, b in enumerate(blows(unit)[: args.max_blows], 1):
            a = b["t"]
            contact = a + b["tc"]
            end = a + b["duration"]
            k = args.frames
            times = list(np.linspace(a, contact, max(3, k // 2), endpoint=False)) + list(np.linspace(contact, end, k - max(3, k // 2)))
            to = b["aim"] - np.array([float(nearest_row(f, a)["rootX"]), 0.0, float(nearest_row(f, a)["rootZ"])])
            to[1] = 0.0
            view = to if np.linalg.norm(to) > 1e-3 else facing(unit, a)
            name = f"blow_{unit.stem}_{n}"

            def sub(tt, f=f, contact=contact):
                r = nearest_row(f, tt)
                marker = " contact" if abs(tt - contact) < 0.02 else ""
                return f"{tt - a:.2f} s{marker}\nerr {float(r.get('aimErr', 0)) * 100:.0f} cm"
            filmstrip(session, times, view, f"{unit.label}: {b['move']} at {b['target']}'s {b['part']} (#{n}, seen from the side of the blow)",
                      os.path.join(out, name + "_film.png"), focus_units=[unit.label, b["target"]], only=True, subtitle_fn=sub)
            picture_sheet(session, pictures_between(pics, a - 0.1, end + 0.1), os.path.join(out, name + "_pictures.jpg"),
                          f"{unit.label} {b['move']} at {b['target']}'s {b['part']}, {a:.2f}-{end:.2f} s", flip=args.flip_pictures)

    for mk in session.meta.get("marks", []):
        times = list(np.linspace(mk["time"] - 1.0, mk["time"] + 0.5, args.frames))
        filmstrip(session, times, np.array([1.0, 0, 0]), f"mark {mk['n']} (1 s before to 0.5 s after)", os.path.join(out, f"mark_{mk['n']}_film.png"))
        picture_sheet(session, pictures_between(pics, mk["time"] - 2.0, mk["time"] + 0.5), os.path.join(out, f"mark_{mk['n']}_pictures.jpg"),
                      f"mark {mk['n']} at {mk['time']:.2f} s (2 s before to 0.5 s after)", flip=args.flip_pictures)

    write_report(session, out, args, films)
    print(os.path.join(out, "report.md"))


if __name__ == "__main__":
    main()
