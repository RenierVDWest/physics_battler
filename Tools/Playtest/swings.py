#!/usr/bin/env python3
"""Review sheets for blows: what each swing looks like and how fast it goes.

    python3 Tools/Playtest/swings.py PlaytestLogs/<session> [--out DIR]

For every blow in the session, one image (swing_<nn>_<unit>_<move>.png):
    - a three-quarter view and a top view: the attacker's animation pose at a few moments
      (faded early, solid at contact), the target at contact, and the weapon at every step,
      drawn as a smear coloured by its speed;
    - the striking part's speed over the blow, with the contact marked.
And swings_sheet.png: every blow's three-quarter view side by side.

Made for the gallery that `dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll swings <folder>`
records (each move once), but works on any session. Needs numpy, pandas, matplotlib.
"""
import argparse
import os
import sys

import numpy as np
import matplotlib

matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.collections import LineCollection

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import analyze  # noqa: E402

SPEED_MAX = 25.0  # m/s at the top of the colour scale


def project(points, forward, yaw_deg, pitch_deg):
    """Screen x, y of world points seen from yaw degrees round from behind the attacker (to its right), pitch above."""
    f = np.array([forward[0], 0.0, forward[2]])
    f /= max(np.linalg.norm(f), 1e-6)
    right = np.array([f[2], 0.0, -f[0]])
    yaw, pitch = np.radians(yaw_deg), np.radians(pitch_deg)
    # The camera looks along d (from its position toward the scene).
    d = np.cos(pitch) * (np.cos(yaw) * f + np.sin(yaw) * -right) + np.sin(pitch) * np.array([0.0, -1.0, 0.0])
    cam_right = np.cross(d, [0.0, 1.0, 0.0])
    cam_right /= max(np.linalg.norm(cam_right), 1e-6)
    cam_up = np.cross(cam_right, d)
    pts = np.atleast_2d(points)
    return np.stack([pts @ cam_right, pts @ cam_up], axis=1)


def bones(session, pts):
    for part, parent in session.parent.items():
        if parent and part in pts and parent in pts:
            yield pts[parent], pts[part]


def skeleton(ax, session, row, forward, view, color, alpha, lw, ghost=True):
    pts = analyze.pose_points(session, None, row, ghost=ghost)
    if not pts:
        pts = analyze.pose_points(session, None, row, ghost=False)
    for a, b in bones(session, pts):
        s = project(np.array([a, b]), forward, *view)
        ax.plot(s[:, 0], s[:, 1], color=color, alpha=alpha, lw=lw, solid_capstyle="round")
    if "Head" in pts:
        s = project(pts["Head"], forward, *view)
        ax.plot(s[0, 0], s[0, 1], "o", color=color, alpha=alpha, ms=lw * 3)


def weapon_at(row, ghost=True):
    keys = ("ghostGrip", "ghostTip") if ghost and "ghostGrip.x" in row and not np.isnan(row["ghostGrip.x"]) else ("grip", "tip")
    g = np.array([row[f"{keys[0]}.x"], row[f"{keys[0]}.y"], row[f"{keys[0]}.z"]], dtype=float)
    t = np.array([row[f"{keys[1]}.x"], row[f"{keys[1]}.y"], row[f"{keys[1]}.z"]], dtype=float)
    return g, t


def draw_view(ax, session, unit, target, blow, frames, view, title):
    pose = unit.pose
    t0, t1 = blow["t"], blow["t"] + blow["duration"]
    contact = blow["t"] + blow["tc"]
    rows = pose[(pose["t"] >= t0) & (pose["t"] <= t1)]
    if rows.empty:
        return
    aim = blow["aim"]
    first = rows.iloc[0]
    forward = aim - np.array([first["Pelvis.x"], 0.0, first["Pelvis.z"]])
    forward[1] = 0.0
    # The weapon at every step: a smear coloured by the speed of its tip.
    segs, speeds = [], []
    last = None
    for _, r in rows.iterrows():
        g, tip = weapon_at(r)
        if np.isnan(g).any() or np.isnan(tip).any():
            continue
        s = project(np.array([g, tip]), forward, *view)
        speed = 0.0 if last is None else np.linalg.norm(tip - last) / session.dt
        segs.append(s)
        speeds.append(speed)
        last = tip
    if segs:
        cmap = plt.get_cmap("plasma")
        lc = LineCollection(segs, colors=[cmap(min(1.0, v / SPEED_MAX)) for v in speeds], linewidths=1.2, alpha=0.55)
        ax.add_collection(lc)
    # The target at contact, the attacker at a few moments (solid at contact).
    if target is not None and not target.pose.empty:
        tr = analyze.nearest_row(target.pose, contact)
        if tr is not None:
            skeleton(ax, session, tr, forward, view, "#1f6f3f", 0.9, 1.6, ghost=False)
    moments = [t0 + 0.02, t0 + 0.6 * blow["tc"], t0 + 0.85 * blow["tc"], contact, contact + 0.35 * (t1 - contact), contact + 0.6 * (t1 - contact)]
    for i, m in enumerate(moments):
        r = analyze.nearest_row(pose, m)
        if r is None:
            continue
        at_contact = abs(m - contact) < 1e-6
        skeleton(ax, session, r, forward, view, "#202020" if at_contact else "#7a7a7a", 1.0 if at_contact else 0.25 + 0.08 * i,
                 2.2 if at_contact else 1.1)
        g, tip = weapon_at(r)
        s = project(np.array([g, tip]), forward, *view)
        ax.plot(s[:, 0], s[:, 1], color="#8a5a2b", lw=3.2 if at_contact else 1.6, alpha=1.0 if at_contact else 0.45, solid_capstyle="round")
    a = project(aim, forward, *view)
    ax.plot(a[0, 0], a[0, 1], "x", color="red", ms=9, mew=2.2)
    ax.set_aspect("equal")
    ax.autoscale_view()
    ax.set_xticks([])
    ax.set_yticks([])
    ax.set_title(title, fontsize=9)


def speed_panel(ax, session, unit, blow):
    f = unit.frames
    t0, t1 = blow["t"], blow["t"] + blow["duration"]
    g = f[(f["t"] >= t0) & (f["t"] <= t1 + 0.02)]
    if g.empty or "strikeX" not in g:
        return
    p = g[["strikeX", "strikeY", "strikeZ"]].to_numpy()
    t = g["t"].to_numpy() - t0
    v = np.r_[np.nan, np.linalg.norm(np.diff(p, axis=0), axis=1) / np.maximum(np.diff(t), 1e-6)]
    blank = np.r_[True, np.linalg.norm(p[:-1], axis=1) < 1e-6] | (np.linalg.norm(p, axis=1) < 1e-6)
    v[blank] = np.nan   # (no striking part recorded before the blow starts)
    pose = unit.pose
    pr = pose[(pose["t"] >= t0) & (pose["t"] <= t1 + 0.02)]
    tips = pr[["tip.x", "tip.y", "tip.z"]].to_numpy()
    tt = pr["t"].to_numpy() - t0
    tv = np.r_[np.nan, np.linalg.norm(np.diff(tips, axis=0), axis=1) / np.maximum(np.diff(tt), 1e-6)]
    ax.plot(t, v, color="#d1495b", lw=1.6, label="striking part")
    ax.plot(tt, tv, color="#999", lw=1, ls="--", label="tip")
    ax.axvline(blow["tc"], color="#30638e", ls=":", lw=1.2)
    ax.text(blow["tc"], 0.97, " contact", transform=ax.get_xaxis_transform(), fontsize=7, va="top", color="#30638e")
    ax.axhspan(12, 20, color="#2a9d8f", alpha=0.08, lw=0)
    ax.set_xlabel("s into the blow", fontsize=8)
    ax.set_ylabel("m/s", fontsize=8)
    ax.set_ylim(0, max(25.0, np.nanmax(tv) * 1.05 if np.isfinite(np.nanmax(tv)) else 25.0))
    ax.legend(fontsize=7, loc="upper left")
    ax.tick_params(labelsize=7)
    ax.grid(alpha=0.25)
    at = np.argmin(np.abs(t - blow["tc"]))
    ax.set_title(f"speed: {v[at]:.1f} m/s at contact, peak {np.nanmax(v):.1f}", fontsize=9)


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("session")
    ap.add_argument("--out")
    args = ap.parse_args()
    path = args.session.rstrip("/\\")
    if not os.path.exists(os.path.join(path, "meta.json")):
        subs = sorted(d for d in os.listdir(path) if os.path.exists(os.path.join(path, d, "meta.json")))
        path = os.path.join(path, subs[-1])
    session = analyze.Session(path)
    out = args.out or os.path.join(path, "swings")
    os.makedirs(out, exist_ok=True)
    sheet = []
    n = 0
    for unit in session.units:
        for blow in analyze.blows(unit):
            target = session.unit(blow["target"])
            n += 1
            name = f"swing_{n:02d}_{unit.stem}_{blow['move']}"
            fig = plt.figure(figsize=(15, 5.2))
            ax1 = fig.add_subplot(1, 3, 1)
            ax2 = fig.add_subplot(1, 3, 2)
            ax3 = fig.add_subplot(1, 3, 3)
            label = f"{unit.label}: {blow['move']} at {blow['target']}'s {blow['part']}"
            draw_view(ax1, session, unit, target, blow, None, (55, 18), "three-quarter view (from the attacker's right, behind)")
            draw_view(ax2, session, unit, target, blow, None, (0, 89), "from above (attacker at the left of the target)")
            speed_panel(ax3, session, unit, blow)
            fig.suptitle(label + "   (grey: the attacker through the blow, black: at contact; green: the target; "
                                 "the weapon at every step, coloured by speed, yellow fast)", fontsize=9)
            fig.tight_layout()
            fig.savefig(os.path.join(out, name + ".png"), dpi=90)
            plt.close(fig)
            sheet.append((unit, target, blow, label))
    if sheet:
        cols = min(5, len(sheet))
        rows = (len(sheet) + cols - 1) // cols
        fig, axs = plt.subplots(rows, cols, figsize=(4.2 * cols, 4.4 * rows), squeeze=False)
        for ax in axs.ravel():
            ax.axis("off")
        for ax, (unit, target, blow, label) in zip(axs.ravel(), sheet):
            ax.axis("on")
            draw_view(ax, session, unit, target, blow, None, (55, 18), label.replace(": ", "\n", 1))
        fig.tight_layout()
        fig.savefig(os.path.join(out, "swings_sheet.png"), dpi=80)
        plt.close(fig)
    print(out)


if __name__ == "__main__":
    main()
