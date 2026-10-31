#!/usr/bin/env python3
"""Reads a Physics Battler playtest log (a folder under PlaytestLogs/) and writes a report with plots.

    python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_07-30-12 [--out DIR] [--frames 8]

Writes into DIR (default: <session>/analysis/, which git ignores):
    report.md                         the story of the session: states, swings, falls, tracking, flags
    timeline_<unit>.png               state, stability, pelvis height, tilt, balance error, assist over time
    swing_<unit>_<n>.png              one swing: ghost vs club angle, tip speed, distance to target, arm joints
    swing_<unit>_<n>_film.png         stick figures through the swing (side and top views)
    fall_<unit>_<n>.png               one fall and get-up: pelvis height vs ghost, tilt, lift force, ground contacts
    fall_<unit>_<n>_film.png          stick figures through the fall and get-up
    mark_<n>_film.png                 stick figures around each mark
    pictures_<nn>.jpg                 the recorded pictures of the game view, 24 per sheet (3 s at 8 per second),
                                      each captioned with its time and what every unit was doing
    swing_<unit>_<n>_pictures.jpg     the pictures through one swing (and fall_…, mark_… likewise)

Needs numpy, pandas, matplotlib and Pillow (for the pictures). The recorder's format is described in
Assets/_Battler/Scripts/Runtime/Diagnostics/PlaytestRecorder.cs.
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

STATE_COLORS = {
    "Steady": "#9bd19b",
    "Flinch": "#f2e394",
    "Stagger": "#f2b36b",
    "Knockdown": "#e36b6b",
    "GettingUp": "#8fb6e8",
    "Dead": "#555555",
}
PHASE_COLORS = {
    "WindUp": "#c9b6e8",
    "Load": "#a58bd9",
    "Strike": "#e36b6b",
    "Follow": "#f2b36b",
    "Recover": "#d8d8d8",
}
ARM_PARTS = ["UpperArmR", "ForearmR", "HandR"]
LEG_PARTS = ["ThighL", "ShinL", "FootL", "ThighR", "ShinR", "FootR"]


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


# ---- Episodes ---------------------------------------------------------------------------------------


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


def swings(unit):
    """Contiguous runs where the planner is not Idle."""
    f = unit.frames
    if f.empty or "phase" not in f:
        return []
    active = (f["phase"] != "Idle").to_numpy()
    out = []
    i = 0
    n = len(f)
    while i < n:
        if active[i]:
            j = i
            while j + 1 < n and active[j + 1]:
                j += 1
            out.append((i, j))
            i = j + 1
        else:
            i += 1
    return out


def falls(unit):
    """From each Knockdown entry to 2.5 s after the following get-up (or the next knockdown / the end)."""
    f = unit.frames
    if f.empty:
        return []
    state = f["state"].to_numpy()
    t = f["t"].to_numpy()
    starts = [i for i in range(len(state)) if state[i] == "Knockdown" and (i == 0 or state[i - 1] != "Knockdown")]
    out = []
    for k, s in enumerate(starts):
        limit = starts[k + 1] - 1 if k + 1 < len(starts) else len(state) - 1
        end = limit
        # The get-up ends when a standing state follows GettingUp; keep 2.5 s after that.
        for i in range(s, limit + 1):
            if state[i] == "GettingUp" and i + 1 <= limit and state[i + 1] != "GettingUp":
                stop_t = t[i] + 2.5
                end = min(limit, int(np.searchsorted(t, stop_t)))
                break
        out.append((max(0, s - 30), end, s))
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
    # Club.
    if "grip.x" in row and not pd.isna(row["grip.x"]):
        grip = np.array([row["grip.x"], row["grip.y"], row["grip.z"]])
        tip = np.array([row["tip.x"], row["tip.y"], row["tip.z"]])
        seg(grip, tip, "#8a5a2b", lw * 1.8, z=3)
        if show_ghost and not pd.isna(row.get("ghostGrip.x", np.nan)):
            g0 = np.array([row["ghostGrip.x"], row["ghostGrip.y"], row["ghostGrip.z"]])
            g1 = np.array([row["ghostTip.x"], row["ghostTip.y"], row["ghostTip.z"]])
            seg(g0, g1, "#c58bff", 1.4, "--", 0.9, 1)
    if "target.x" in row and not pd.isna(row["target.x"]):
        p = axes_fn(np.array([row["target.x"], row["target.y"], row["target.z"]]))
        ax.plot(p[0], p[1], "x", color="red", ms=8, mew=2, zorder=4)
    if "capture.x" in row and not pd.isna(row["capture.x"]):
        p = axes_fn(np.array([row["capture.x"], 0.0, row["capture.z"]]))
        ax.plot(p[0], p[1], "o", color="#e0b000", ms=4, zorder=4)


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


def filmstrip(session, times, forward, title, path, focus_units=None, subtitle_fn=None):
    side, top = view_axes(forward)
    units = [u for u in session.units if not u.pose.empty]
    if focus_units:
        units = [u for u in units if u.label in focus_units] + [u for u in units if u.label not in focus_units]
    n = len(times)
    fig, axs = plt.subplots(2, n, figsize=(2.3 * n, 6.2), squeeze=False)
    palette = ["#202020", "#7a3b9a", "#1f6f3f", "#9a5b00"]
    # Shared limits from everything drawn.
    xs, ys, zs = [], [], []
    for t in times:
        for u in units:
            r = nearest_row(u.pose, t)
            if r is None or abs(r["t"] - t) > 0.1:
                continue
            for pt in pose_points(session, u, r).values():
                s, tp = side(pt), top(pt)
                xs.append(s[0]); ys.append(s[1]); zs.append(tp[1])
            if "tip.x" in r and not pd.isna(r["tip.x"]):
                tip = np.array([r["tip.x"], r["tip.y"], r["tip.z"]])
                xs.append(side(tip)[0]); ys.append(side(tip)[1]); zs.append(top(tip)[1])
    if not xs:
        plt.close(fig)
        return False
    x0, x1 = min(xs) - 0.4, max(xs) + 0.4
    y1 = max(max(ys) + 0.3, 2.2)
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
                draw_figure(ax, session, u, r, fn, palette[ui % len(palette)])
            if row_i == 0:
                label = f"{t:.2f} s"
                if subtitle_fn:
                    label += "\n" + subtitle_fn(t)
                ax.set_title(label, fontsize=7.5)
    axs[0][0].set_ylabel("side", fontsize=9)
    axs[1][0].set_ylabel("top", fontsize=9)
    fig.suptitle(title + "   (solid: physical body, dashed cyan: ghost, brown: club, dashed purple: ghost club, x: target)", fontsize=9)
    fig.tight_layout(rect=(0, 0, 1, 0.95))
    fig.savefig(path, dpi=110)
    plt.close(fig)
    return True


def state_band(ax, f, column="state", colors=STATE_COLORS):
    t = f["t"].to_numpy()
    for value, t0, t1, _, _ in segments(f[column], t):
        ax.axvspan(t0, t1 + 1 / 60, color=colors.get(value, "#eeeeee"), alpha=0.35, lw=0)


def mark_lines(ax, session, t0, t1):
    for m in session.meta.get("marks", []):
        if t0 <= m["time"] <= t1:
            ax.axvline(m["time"], color="black", lw=1, ls=":")
            ax.text(m["time"], ax.get_ylim()[1], f" M{m['n']}", fontsize=7, va="top")


def legend_states(ax, colors):
    from matplotlib.patches import Patch
    ax.legend(handles=[Patch(color=c, alpha=0.5, label=k) for k, c in colors.items()], fontsize=6, ncol=len(colors), loc="upper left")


def plot_timeline(session, unit, path):
    f = unit.frames
    if f.empty:
        return False
    t = f["t"].to_numpy()
    rest = unit.meta.get("restPelvisHeight", 1.0)
    fig, axs = plt.subplots(5, 1, figsize=(13, 10), sharex=True)
    for ax in axs:
        state_band(ax, f)
    axs[0].plot(t, f["stability"], color="black", lw=1.2, label="stability")
    axs[0].plot(t, f["health"], color="#c00000", lw=1.2, label="health")
    axs[0].set_ylabel("stability / health")
    axs[0].legend(fontsize=7, loc="lower left")
    axs[1].plot(t, f["pelvisH"], color="black", lw=1.2, label="pelvis (physical)")
    axs[1].plot(t, f["ghostPelvisH"], color="#18b6d8", lw=1, ls="--", label="pelvis (ghost)")
    axs[1].axhline(rest * 0.55, color="#c00000", lw=0.8, ls=":", label="tipped below 55% rest")
    axs[1].set_ylabel("height (m)")
    axs[1].legend(fontsize=7, loc="lower left")
    axs[2].plot(t, f["chestTilt"], color="black", lw=1.2, label="chest tilt")
    axs[2].plot(t, f["pelvisTilt"], color="#777", lw=1, label="pelvis tilt")
    axs[2].set_ylabel("tilt (°)")
    axs[2].legend(fontsize=7, loc="upper left")
    axs[3].plot(t, f["balanceError"], color="black", lw=1.2, label="balance error (capture point to feet)")
    steps = f["stepping"].to_numpy().astype(bool)
    axs[3].fill_between(t, 0, steps * f["balanceError"].max() * 0.15, color="#2a7", alpha=0.6, step="mid", label="stepping")
    axs[3].set_ylabel("m")
    axs[3].legend(fontsize=7, loc="upper left")
    axs[4].plot(t, f["assist"], color="black", lw=1.2, label="balance assist")
    axs[4].plot(t, f["tone"], color="#a05", lw=1, label="muscle tone")
    if "str_HandR" in f:
        axs[4].plot(t, f["str_UpperArmR"], color="#e80", lw=0.8, label="right arm strength")
    axs[4].set_ylabel("0..1+")
    axs[4].set_xlabel("time (s)")
    axs[4].legend(fontsize=7, loc="upper left")
    # Swing phases on the top axis.
    for a0, a1 in swings(unit):
        sub = f.iloc[a0:a1 + 1]
        for value, s0, s1, _, _ in segments(sub["phase"], sub["t"].to_numpy()):
            axs[0].axvspan(s0, s1 + 1 / 60, ymin=0.92, ymax=1.0, color=PHASE_COLORS.get(value, "#ccc"), lw=0)
    for e in unit.events:
        if e.cat == "HIT":
            axs[0].axvline(e.t, color="#c00000", lw=0.8)
    for e in session.events:
        if e.cat == "HIT" and f"→ {unit.label} " in e.text:
            axs[0].axvline(e.t, color="#c00000", lw=1.6, ls="--")
    for ax in axs:
        mark_lines(ax, session, t[0], t[-1])
    legend_states(axs[4], STATE_COLORS)
    fig.suptitle(f"{unit.label}: timeline (colour bands: state; top strip: swing phases; red lines: hits dealt, dashed: hits taken)", fontsize=10)
    fig.tight_layout(rect=(0, 0, 1, 0.97))
    fig.savefig(path, dpi=100)
    plt.close(fig)
    return True


def unwrapped_angles(f):
    """Ghost and club swing angles without the ±180° jumps: the ghost unwrapped (ending where the
    recorder says, so the strike reads negative-to-positive), the club as ghost minus lag."""
    theta = f["theta"].to_numpy(dtype=float)
    lag = f["lag"].to_numpy(dtype=float)
    ghost = np.full_like(theta, np.nan)
    ok = ~np.isnan(theta)
    if ok.any():
        unwrapped = np.degrees(np.unwrap(np.radians(theta[ok])))
        unwrapped += theta[ok][-1] - unwrapped[-1]
        ghost[ok] = unwrapped
    return ghost, ghost - lag


def plot_swing(session, unit, i0, i1, n, path):
    f = unit.frames.iloc[max(0, i0 - 10):i1 + 15]
    t = f["t"].to_numpy()
    fig, axs = plt.subplots(4, 1, figsize=(10, 9.5), sharex=True)
    for ax in axs:
        state_band(ax, f, "phase", PHASE_COLORS)
    ghost, club = unwrapped_angles(f)
    axs[0].plot(t, ghost, color="#18b6d8", lw=1.6, ls="--", label="ghost θ (where the swing wants the club)")
    axs[0].plot(t, club, color="#8a5a2b", lw=1.8, label="club θ (where the club is)")
    axs[0].axhline(0, color="red", lw=0.8, ls=":", label="θ 0 = contact direction")
    axs[0].set_ylabel("swing angle (°)")
    axs[0].legend(fontsize=7, loc="best")
    axs[1].plot(t, f["tipSpeed"], color="black", lw=1.4, label="club tip speed")
    axs[1].set_ylabel("m/s")
    ax2 = axs[1].twinx()
    ax2.plot(t, f["tipH"], color="#777", lw=1, label="tip height")
    ax2.axhline(0, color="#777", lw=0.6)
    ax2.set_ylabel("tip height (m)", color="#777")
    axs[1].legend(fontsize=7, loc="upper left")
    axs[2].plot(t, f["clubToTarget"], color="red", lw=1.4, label="club to the swing's target point")
    axs[2].plot(t, f["clubToVictim"], color="#a05", lw=1, ls="--", label="club to the victim's chest now")
    axs[2].plot(t, f["gripError"], color="#18b6d8", lw=1, label="grip: physical vs ghost")
    axs[2].set_ylabel("m")
    axs[2].legend(fontsize=7, loc="best")
    for p, c in zip(ARM_PARTS + ["Chest", "Spine"], ["#d62728", "#ff7f0e", "#bcbd22", "#1f77b4", "#17becf"]):
        axs[3].plot(t, f[f"err_{p}"], color=c, lw=1.2, label=f"{p} error")
    axs[3].set_ylabel("joint error vs ghost (°)")
    ax3 = axs[3].twinx()
    ax3.plot(t, f["str_UpperArmR"], color="#555", lw=0.9, ls=":", label="arm strength")
    ax3.set_ylabel("arm strength", color="#555")
    axs[3].legend(fontsize=7, loc="upper left", ncol=3)
    axs[3].set_xlabel("time (s)")
    for e in unit.events:
        if e.cat in ("club", "HIT", "hit") and t[0] <= e.t <= t[-1]:
            for ax in axs:
                ax.axvline(e.t, color="#c00000" if e.cat == "HIT" else "#8a5a2b", lw=0.9, ls="-" if e.cat == "HIT" else "--")
    fig.suptitle(f"{unit.label}: swing #{n} (bands: phases — wind-up, load, strike, follow, recover; lines: club contacts / hits)", fontsize=10)
    fig.tight_layout(rect=(0, 0, 1, 0.97))
    fig.savefig(path, dpi=100)
    plt.close(fig)


def plot_fall(session, unit, i0, i1, n, path):
    f = unit.frames.iloc[i0:i1 + 1]
    t = f["t"].to_numpy()
    rest = unit.meta.get("restPelvisHeight", 1.0)
    weight = unit.meta.get("mass", 100) * 9.81
    fig, axs = plt.subplots(4, 1, figsize=(10, 9.5), sharex=True)
    for ax in axs:
        state_band(ax, f)
    axs[0].plot(t, f["pelvisH"], color="black", lw=1.4, label="pelvis (physical)")
    axs[0].plot(t, f["ghostPelvisH"], color="#18b6d8", lw=1.2, ls="--", label="pelvis (ghost wants)")
    axs[0].plot(t, f["comY"], color="#777", lw=1, label="centre of mass")
    axs[0].axhline(rest * 0.55, color="#c00000", lw=0.8, ls=":", label="tipped below this")
    axs[0].set_ylabel("height (m)")
    axs[0].legend(fontsize=7, loc="best")
    axs[1].plot(t, f["chestTilt"], color="black", lw=1.4, label="chest tilt")
    axs[1].plot(t, f["pelvisTilt"], color="#777", lw=1, label="pelvis tilt")
    axs[1].plot(t, (f["chestFrontUp"] * 90), color="#a05", lw=0.8, ls=":", label="chest front ·up ×90 (+ on back, − face down)")
    axs[1].set_ylabel("°")
    axs[1].legend(fontsize=7, loc="best")
    axs[2].plot(t, f["forceVertical"], color="black", lw=1.2, label="assist lift force (N)")
    axs[2].axhline(weight, color="#777", lw=0.8, ls=":", label="body weight")
    axs[2].set_ylabel("N")
    ax2 = axs[2].twinx()
    ax2.plot(t, f["assist"], color="#1f77b4", lw=1, label="assist")
    ax2.plot(t, f["tone"], color="#a05", lw=1, label="tone")
    ax2.set_ylim(0, 1.1)
    ax2.set_ylabel("assist / tone")
    axs[2].legend(fontsize=7, loc="upper left")
    ax2.legend(fontsize=7, loc="upper right")
    # Ground contact raster.
    masks = f["groundMask"].to_numpy().astype(int)
    for k, p in enumerate(session.parts):
        bit = 1 << k
        on = (masks & bit) != 0
        axs[3].fill_between(t, k - 0.4, k + 0.4, where=on, color="#444", step="mid", lw=0)
    axs[3].set_yticks(range(len(session.parts)))
    axs[3].set_yticklabels(session.parts, fontsize=6)
    axs[3].set_ylabel("touching the ground")
    axs[3].set_xlabel("time (s)")
    for ax in axs:
        mark_lines(ax, session, t[0], t[-1])
    fig.suptitle(f"{unit.label}: fall #{n} and get-up (bands: states)", fontsize=10)
    fig.tight_layout(rect=(0, 0, 1, 0.97))
    fig.savefig(path, dpi=100)
    plt.close(fig)


# ---- Report ---------------------------------------------------------------------------------------


def fmt(x, digits=2):
    if x is None or (isinstance(x, float) and (math.isnan(x) or math.isinf(x))):
        return "-"
    return f"{x:.{digits}f}"


def swing_metrics(unit, i0, i1):
    f = unit.frames.iloc[i0:i1 + 1]
    out = {"start": float(f["t"].iloc[0]), "end": float(f["t"].iloc[-1])}
    phases = {}
    for value, t0, t1, a, b in segments(f["phase"], f["t"].to_numpy()):
        phases.setdefault(value, []).append((t0, t1, a, b))
    out["phases"] = phases
    out["reached"] = [p for p in ("WindUp", "Load", "Strike", "Follow", "Recover") if p in phases]
    def at_end(phase):
        if phase not in phases:
            return None
        a, b = phases[phase][-1][2], phases[phase][-1][3]
        r = f.iloc[b]
        return {"theta": r["theta"], "club": r["clubTheta"], "lag": r["lag"], "tip": r["tipSpeed"], "toTarget": r["clubToTarget"]}
    out["ends"] = {p: at_end(p) for p in ("WindUp", "Load", "Strike", "Follow")}
    strike = f[f["phase"] == "Strike"]
    out["strikeMaxLag"] = float(strike["lag"].abs().max()) if len(strike) else float("nan")
    out["maxTip"] = float(f["tipSpeed"].max())
    late = f[f["phase"].isin(["Strike", "Follow"])]
    if len(late) and late["clubToTarget"].notna().any():
        k = late["clubToTarget"].idxmin()
        out["closest"] = float(late.loc[k, "clubToTarget"])
        out["closestAt"] = float(late.loc[k, "t"])
        out["closestTheta"] = float(late.loc[k, "theta"])
        out["closestClub"] = float(late.loc[k, "clubTheta"])
    out["minTipH"] = float(f["tipH"].min()) if f["tipH"].notna().any() else float("nan")
    out["armErr"] = {p: float(f[f"err_{p}"].max()) for p in ARM_PARTS}
    out["armErrStrikeMean"] = {p: float(strike[f"err_{p}"].mean()) if len(strike) else float("nan") for p in ARM_PARTS}
    out["minClubTheta"] = float(f["clubTheta"].min()) if f["clubTheta"].notna().any() else float("nan")
    out["minGhostTheta"] = float(f["theta"].min()) if f["theta"].notna().any() else float("nan")
    out["pelvisDrop"] = float(f["pelvisH"].iloc[0] - f["pelvisH"].min())
    out["maxTilt"] = float(f["chestTilt"].max())
    out["groundTouch"] = int(((f["groundMask"].astype(int) & ~((1 << 12) | (1 << 15))) != 0).sum())
    return out


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


# ---- Pictures of the game view -------------------------------------------------------------------


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


def picture_caption(session, t):
    parts = [f"{t:.2f}s"]
    for unit in session.units:
        f = unit.frames
        if f.empty or t < f["t"].iloc[0] - 0.05 or t > f["t"].iloc[-1] + 0.05:
            continue
        r = nearest_row(f, t)
        short = unit.label.split(" (")[0].replace("Orc ", "")
        what = r["state"] if r["phase"] == "Idle" else f"{r['phase']}"
        parts.append(f"{short}:{what}")
    return " ".join(parts)


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


def write_report(session, out, args):
    lines = []
    w = lines.append
    m = session.meta
    duration = (m.get("endTime") or (session.events[-1].t if session.events else 0)) - m.get("startTime", 0)
    w(f"# Playtest {m['session']} — analysis")
    w("")
    w(f"Unity {m.get('unity')}, physics {1 / session.dt:.0f} Hz, {duration:.1f} s recorded, {len(session.events)} events, "
      f"{len(m.get('marks', []))} marks{'' if m.get('endTime') else ' (the session did not end cleanly; data may be cut short)'}.")
    w("")
    problems = [e for e in session.events if e.cat == "console" and (e.text.startswith("ERROR") or e.text.startswith("EXCEPTION") or e.text.startswith("warning"))]
    if problems:
        w(f"**Console:** {len(problems)} warnings/errors. First: `{problems[0].text[:200]}`")
        w("")

    for unit in session.units:
        f = unit.frames
        w(f"## {unit.label}")
        w("")
        if f.empty:
            w("No frames recorded.")
            continue
        t = f["t"].to_numpy()
        w(f"{len(f)} frames, {t[0]:.2f}–{t[-1]:.2f} s. Mass {unit.meta.get('mass', '?')} kg, rest pelvis height {unit.meta.get('restPelvisHeight', '?')} m.")
        w("")
        w("**States**")
        w("")
        state_events = [e for e in unit.events if e.cat == "state"]
        for e in state_events:
            w(f"- {e.t:7.2f} s  {e.text}")
        w("")

        sw = swings(unit)
        if sw:
            w("**Swings**")
            w("")
            w("| # | start | phases reached | ghost θ min / club θ min | lag at end of load / strike | worst strike lag | fastest tip | closest to target | tip lowest | outcome |")
            w("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |")
            results = [e for e in unit.events if e.cat == "attack" and "result:" in e.text]
            for n, (i0, i1) in enumerate(sw, 1):
                mtr = swing_metrics(unit, i0, i1)
                ends = mtr["ends"]
                outcome = next((e.text.split("result: ", 1)[1].split(";")[0] for e in results if mtr["start"] - 0.05 <= e.t <= mtr["end"] + 0.5), "?")
                closest = f"{fmt(mtr.get('closest'))} m (ghost θ {fmt(mtr.get('closestTheta'), 0)}°, club θ {fmt(mtr.get('closestClub'), 0)}°)" if "closest" in mtr else "-"
                w(f"| {n} | {mtr['start']:.2f} | {'→'.join(mtr['reached'])} | {fmt(mtr['minGhostTheta'], 0)}° / {fmt(mtr['minClubTheta'], 0)}° | "
                  f"{fmt(ends['Load']['lag'] if ends['Load'] else None, 0)}° / {fmt(ends['Strike']['lag'] if ends['Strike'] else None, 0)}° | "
                  f"{fmt(mtr['strikeMaxLag'], 0)}° | {fmt(mtr['maxTip'], 1)} m/s | {closest} | {fmt(mtr['minTipH'])} m | {outcome} |")
            w("")
            w("Arm tracking during strikes (mean error °, physical vs ghost): " + "; ".join(
                f"#{n}: " + ", ".join(f"{p} {fmt(swing_metrics(unit, i0, i1)['armErrStrikeMean'][p], 0)}" for p in ARM_PARTS)
                for n, (i0, i1) in enumerate(sw, 1)))
            w("")

        fl = falls(unit)
        if fl:
            w("**Falls and get-ups**")
            w("")
            for n, (i0, i1, s) in enumerate(fl, 1):
                sub = f.iloc[s:i1 + 1]
                getting = sub[sub["state"] == "GettingUp"]
                text = [f"fall #{n} at {f['t'].iloc[s]:.2f} s"]
                ev = events_between(unit, f["t"].iloc[s] - 0.05, f["t"].iloc[i1], {"state", "getup", "contact"})
                if len(getting):
                    text.append(f"get-up {getting['t'].iloc[0]:.2f}–{getting['t'].iloc[-1]:.2f} s: pelvis {getting['pelvisH'].iloc[0]:.2f} → max {getting['pelvisH'].max():.2f} → "
                                f"{getting['pelvisH'].iloc[-1]:.2f} m (ghost ends at {getting['ghostPelvisH'].iloc[-1]:.2f}), chest tilt {getting['chestTilt'].iloc[0]:.0f}° → {getting['chestTilt'].iloc[-1]:.0f}°, "
                                f"lift force mean {getting['forceVertical'].mean():.0f} N vs weight {unit.meta.get('mass', 100) * 9.81:.0f} N")
                else:
                    text.append("never started getting up in this window")
                w("- " + "; ".join(text))
                for e in ev:
                    w(f"    - {e.t:7.2f} s  [{e.cat}] {e.text}")
            w("")

        # Shaking: angular velocity flipping back and forth every physics step.
        idle_mask = (f["state"] == "Steady") & (f["phase"] == "Idle")
        jit = jitter(session, unit, idle_mask)
        if jit:
            shaking = [p for p, (rms, alt) in jit.items() if alt > 0.75 and rms > 1.5]
            w("**Shaking while standing idle** (rms angular speed rad/s, share of steps reversing): "
              + ", ".join(f"{p} {rms:.1f}/{alt:.2f}" for p, (rms, alt) in jit.items()))
            w("")
            w(f"→ {'SHAKING at the physics rate: ' + ', '.join(shaking) if shaking else 'no 30 Hz shaking'}")
            w("")

        # Tracking error table.
        ctx_rows = []
        for ctx, mask in (("idle", (f["state"] == "Steady") & (f["phase"] == "Idle")),
                          ("swinging", f["phase"] != "Idle"),
                          ("stagger", f["state"] == "Stagger"),
                          ("getting up", f["state"] == "GettingUp")):
            if mask.sum() >= 12:
                ctx_rows.append((ctx, f[mask]))
        if ctx_rows:
            w("**Joint tracking, physical vs ghost** (mean / 95th percentile, degrees)")
            w("")
            w("| part | " + " | ".join(f"{c} ({len(d) * session.dt:.0f} s)" for c, d in ctx_rows) + " |")
            w("| --- |" + " --- |" * len(ctx_rows))
            for p in session.parts:
                w(f"| {p} | " + " | ".join(f"{d[f'err_{p}'].mean():.0f} / {d[f'err_{p}'].quantile(0.95):.0f}" for _, d in ctx_rows) + " |")
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
        w(f"{len(pics)} pictures, {pics['t'].iloc[0]:.2f}–{pics['t'].iloc[-1]:.2f} s. Sheets of 24 (`pictures_NN.jpg`), and for each swing, fall and mark "
          "(`*_pictures.jpg`). Captions: time, then each unit's phase (or state when idle).")
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

    for unit in session.units:
        if unit.frames.empty:
            continue
        plot_timeline(session, unit, os.path.join(out, f"timeline_{unit.stem}.png"))
        for n, (i0, i1) in enumerate(swings(unit), 1):
            plot_swing(session, unit, i0, i1, n, os.path.join(out, f"swing_{unit.stem}_{n}.png"))
            f = unit.frames
            t0, t1 = f["t"].iloc[i0], f["t"].iloc[i1]
            # Film: evenly through wind-up/load, denser through the strike.
            strike = f.iloc[i0:i1 + 1]
            strike = strike[strike["phase"].isin(["Strike", "Follow"])]
            k = args.frames
            if len(strike):
                s0, s1 = strike["t"].iloc[0], strike["t"].iloc[-1]
                times = list(np.linspace(t0, s0, max(2, k // 3), endpoint=False)) + list(np.linspace(s0, s1, k - max(2, k // 3)))
            else:
                times = list(np.linspace(t0, t1, k))
            row = f.iloc[i0]
            target = None
            pose_row = nearest_row(unit.pose, t0 + 0.05)
            if pose_row is not None and not pd.isna(pose_row.get("target.x", np.nan)):
                target = np.array([pose_row["target.x"], 0, pose_row["target.z"]]) - np.array([pose_row["Pelvis.x"], 0, pose_row["Pelvis.z"]])
            yaw = math.radians(row["rootYaw"])
            forward = target if target is not None else np.array([math.sin(yaw), 0, math.cos(yaw)])

            def sub(tt, f=f):
                r = nearest_row(f, tt)
                return f"{r['phase']} θ {fmt(r['theta'], 0)}°/{fmt(r['clubTheta'], 0)}°"
            filmstrip(session, times, forward, f"{unit.label} swing #{n}", os.path.join(out, f"swing_{unit.stem}_{n}_film.png"),
                      focus_units=[unit.label], subtitle_fn=sub)
            picture_sheet(session, pictures_between(pics, t0 - 0.15, t1 + 0.3), os.path.join(out, f"swing_{unit.stem}_{n}_pictures.jpg"),
                          f"{unit.label} swing #{n}, {t0:.2f}-{t1:.2f} s", flip=args.flip_pictures)
        for n, (i0, i1, s) in enumerate(falls(unit), 1):
            plot_fall(session, unit, i0, i1, n, os.path.join(out, f"fall_{unit.stem}_{n}.png"))
            f = unit.frames
            times = list(np.linspace(f["t"].iloc[s], f["t"].iloc[i1], args.frames + 2))
            yaw = math.radians(f["rootYaw"].iloc[s])
            forward = np.array([math.sin(yaw), 0, math.cos(yaw)])

            def sub(tt, f=f):
                r = nearest_row(f, tt)
                return f"{r['state']} p{r['pelvisH']:.2f} t{r['chestTilt']:.0f}°"
            filmstrip(session, times, forward, f"{unit.label} fall #{n}", os.path.join(out, f"fall_{unit.stem}_{n}_film.png"),
                      focus_units=[unit.label], subtitle_fn=sub)
            picture_sheet(session, pictures_between(pics, f["t"].iloc[s] - 0.5, f["t"].iloc[i1]), os.path.join(out, f"fall_{unit.stem}_{n}_pictures.jpg"),
                          f"{unit.label} fall #{n}", flip=args.flip_pictures)

    for mk in session.meta.get("marks", []):
        times = list(np.linspace(mk["time"] - 1.0, mk["time"] + 0.5, args.frames))
        forward = np.array([1.0, 0, 0])
        filmstrip(session, times, forward, f"mark {mk['n']} (1 s before to 0.5 s after)", os.path.join(out, f"mark_{mk['n']}_film.png"))
        picture_sheet(session, pictures_between(pics, mk["time"] - 2.0, mk["time"] + 0.5), os.path.join(out, f"mark_{mk['n']}_pictures.jpg"),
                      f"mark {mk['n']} at {mk['time']:.2f} s (2 s before to 0.5 s after)", flip=args.flip_pictures)

    write_report(session, out, args)
    print(os.path.join(out, "report.md"))


if __name__ == "__main__":
    main()
