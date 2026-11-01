#!/usr/bin/env python3
"""Milestone 9b: bodies on the ground. For each unit while it lies down (knocked down or dead):
how often each joint of its physical body is more than 10 degrees past its limits, and how fast its
fastest part moves with a standing unit's legs near it and with none. Works on logs from before the
milestone too (it measures from the logged part rotations), so a playtest can be set against older ones.

    python3 Tools/Playtest/ragdoll.py <session folder>
analyze.py prints the same section.
"""
import sys
import numpy as np
import pandas as pd

# The joint limits (Muscles/JointMath.cs, JointLimitTable.For): pitch min, pitch max, about Y, about Z.
# Logs from before and after the milestone are both measured against these (the body's own limits).
LIMITS = {
    'Spine': (-25, 40, 30, 25), 'Chest': (-20, 30, 30, 20), 'Head': (-50, 60, 80, 30),
    'UpperArmL': (-160, 50, 90, 130), 'UpperArmR': (-160, 50, 90, 130),
    'ForearmL': (-145, 5, 60, 15), 'ForearmR': (-145, 5, 60, 15),
    'HandL': (-15, 30, 80, 40), 'HandR': (-15, 30, 80, 40),
    'ThighL': (-110, 35, 45, 45), 'ThighR': (-110, 35, 45, 45),
    'ShinL': (-5, 145, 15, 15), 'ShinR': (-5, 145, 15, 15),
    'FootL': (-40, 45, 25, 25), 'FootR': (-40, 45, 25, 25),
}
# The table before milestone 9b (the wrist's roll then sat in the forearm).
LIMITS_BEFORE_9B = {
    'Spine': (-25, 40, 30, 25), 'Chest': (-20, 30, 30, 20), 'Head': (-40, 45, 70, 30),
    'UpperArmL': (-160, 50, 80, 100), 'UpperArmR': (-160, 50, 80, 100),
    'ForearmL': (-145, 5, 60, 10), 'ForearmR': (-145, 5, 60, 10),
    'HandL': (-70, 70, 40, 45), 'HandR': (-70, 70, 40, 45),
    'ThighL': (-110, 35, 40, 45), 'ThighR': (-110, 35, 40, 45),
    'ShinL': (-5, 145, 8, 8), 'ShinR': (-5, 145, 8, 8),
    'FootL': (-40, 45, 15, 25), 'FootR': (-40, 45, 15, 25),
}
PARENT = {'Spine': 'Pelvis', 'Chest': 'Spine', 'Head': 'Chest', 'UpperArmL': 'Chest', 'ForearmL': 'UpperArmL', 'HandL': 'ForearmL',
          'UpperArmR': 'Chest', 'ForearmR': 'UpperArmR', 'HandR': 'ForearmR', 'ThighL': 'Pelvis', 'ShinL': 'ThighL', 'FootL': 'ShinL',
          'ThighR': 'Pelvis', 'ShinR': 'ThighR', 'FootR': 'ShinR'}
JOINT = {'Spine': 'spine', 'Chest': 'chest', 'Head': 'neck', 'UpperArmL': 'shoulder', 'UpperArmR': 'shoulder', 'ForearmL': 'elbow',
         'ForearmR': 'elbow', 'HandL': 'wrist', 'HandR': 'wrist', 'ThighL': 'hip', 'ThighR': 'hip', 'ShinL': 'knee', 'ShinR': 'knee',
         'FootL': 'ankle', 'FootR': 'ankle'}
LEGS = ['ThighL', 'ShinL', 'FootL', 'ThighR', 'ShinR', 'FootR']
PARTS = ['Pelvis', 'Spine', 'Chest', 'Head', 'UpperArmL', 'ForearmL', 'HandL', 'UpperArmR', 'ForearmR', 'HandR'] + LEGS


def _q(df, p):
    return df[[p + '.qx', p + '.qy', p + '.qz', p + '.qw']].to_numpy(dtype=float)


def _mul(a, b):
    x1, y1, z1, w1 = a.T
    x2, y2, z2, w2 = b.T
    return np.stack([w1 * x2 + x1 * w2 + y1 * z2 - z1 * y2, w1 * y2 - x1 * z2 + y1 * w2 + z1 * x2,
                     w1 * z2 + x1 * y2 - y1 * x2 + z1 * w2, w1 * w2 - x1 * x2 - y1 * y2 - z1 * z2], 1)


def past_limits(pose, part, limits):
    """Degrees past the limits per row (0 inside), split as PhysX does: the bend about X, then the swing about Y and Z."""
    lo, hi, ty, tz = limits
    r = _mul(_q(pose, PARENT[part]) * np.array([-1, -1, -1, 1]), _q(pose, part))
    r[r[:, 3] < 0] *= -1
    x, w = r[:, 0], r[:, 3]
    pitch = np.degrees(2 * np.arctan2(x, w))
    h = np.radians(pitch) / 2
    twist = np.stack([np.sin(h), 0 * h, 0 * h, np.cos(h)], 1)
    s = _mul(r, twist * np.array([-1, -1, -1, 1]))
    s[s[:, 3] < 0] *= -1
    ay = np.degrees(2 * np.arcsin(np.clip(s[:, 1], -1, 1)))
    az = np.degrees(2 * np.arcsin(np.clip(s[:, 2], -1, 1)))
    return np.maximum.reduce([lo - pitch, pitch - hi, np.abs(ay) - ty, np.abs(az) - tz, np.zeros_like(pitch)])


def _states(unit, n):
    s = unit.frames['state'].to_numpy()[:n] if 'state' in unit.frames else np.array(['?'] * n)
    alive = unit.frames['alive'].to_numpy()[:n] if 'alive' in unit.frames else np.ones(n, bool)
    return s, alive


def measure(session, limits=None):
    """Per unit: joints past their limits while lying, and part speeds with and without standing legs near."""
    limits = limits or LIMITS
    units = [u for u in session.units if not u.pose.empty and not u.frames.empty]
    poses = {}
    for u in units:
        n = min(len(u.pose), len(u.frames))
        p = u.pose.iloc[:n].copy()
        s, alive = _states(u, n)
        p['state'] = s
        p['lying'] = (s == 'Down') | (s == 'Dead') | (alive.astype(str) == 'False') | (alive.astype(str) == '0')
        poses[u.label] = p.set_index(np.round(p['t'].to_numpy(), 3))
    rows = []
    for u in units:
        p = poses[u.label]
        lying = p[p['lying']]
        if len(lying) < 30:
            continue
        r = {'unit': u.label, 'lying_s': len(lying) * session.dt}
        joints = {}
        for part, lim in limits.items():
            if part + '.qx' not in lying:
                continue
            past = past_limits(lying, part, lim)
            j = JOINT[part]
            joints.setdefault(j, []).append(past)
        r['joints'] = joints   # (a list per joint: left and right apart)
        allpast = np.max(np.stack([past_limits(lying, part, lim) for part, lim in limits.items() if part + '.qx' in lying], 1), 1)
        r['past_share'] = float((allpast > 10).mean())
        r['past_worst'] = float(allpast.max())
        # Speeds: from positions, 1.5 s or more after it went down (by then it has settled).
        t = lying.index.to_numpy()
        P = np.stack([lying[[x + '.x', x + '.y', x + '.z']].to_numpy(dtype=float) for x in PARTS if x + '.x' in lying], 1)
        dt = np.diff(t)
        ok = (dt > 0) & (dt < 0.05)
        v = np.linalg.norm(np.diff(P, axis=0), axis=2) / np.maximum(dt, 1e-4)[:, None]
        fastest = v.max(1)
        near = np.full(len(t) - 1, 9.0)
        for other in units:
            if other is u:
                continue
            o = poses[other.label].reindex(t[1:])
            standing = (o['state'] == 'Standing').to_numpy()
            L = np.stack([o[[x + '.x', x + '.y', x + '.z']].to_numpy(dtype=float) for x in LEGS], 1)
            d = np.linalg.norm(P[1:, :, None, :] - L[:, None, :, :], axis=3).min((1, 2))
            d = np.where(standing, np.nan_to_num(d, nan=9.0), 9.0)
            near = np.minimum(near, d)
        start = np.r_[True, np.diff(t) > 0.05]  # (a new stretch on the ground: down again)
        since = t - np.maximum.accumulate(np.where(start, t, -np.inf))
        settled = (since[1:] >= 1.5) & ok
        r['near'] = fastest[settled & (near < 0.5)]
        r['alone'] = fastest[settled & (near >= 1.0)]
        rows.append(r)
    return rows


def _pct(a, q):
    return f"{np.percentile(a, q):.1f}" if len(a) else "-"


def section(session):
    rows = measure(session)
    lines = ["## Bodies on the ground", ""]
    if not rows:
        return [lines + ["No unit lay on the ground long enough to measure.", ""]]
    lines.append("Each joint while lying (the worse side): the share of the time it was more than 10° past its limits, and the worst. "
                 "Speeds: the fastest part of the body, from 1.5 s after it went down, with a standing unit's leg within 0.5 m and with none within 1 m.")
    lines.append("")
    names = ['elbow', 'knee', 'ankle', 'wrist', 'shoulder', 'neck', 'hip', 'spine', 'chest']
    lines.append("| unit | on the ground | any joint past 10° | worst | " + " | ".join(names) + " | fastest part, leg near (median / 95%) | alone (median) |")
    lines.append("| --- | --- | --- | --- | " + " | ".join("---" for _ in names) + " | --- | --- |")
    for r in rows:
        cells = []
        for j in names:
            sides = r['joints'].get(j)
            if not sides:
                cells.append("-")
                continue
            share = max((a > 10).mean() for a in sides)   # (the worse side)
            cells.append(f"{share * 100:.0f}% / {max(a.max() for a in sides):.0f}°")
        lines.append(f"| {r['unit']} | {r['lying_s']:.0f} s | {r['past_share'] * 100:.0f}% | {r['past_worst']:.0f}° | " + " | ".join(cells)
                     + f" | {_pct(r['near'], 50)} / {_pct(r['near'], 95)} m/s ({len(r['near'])} steps) | {_pct(r['alone'], 50)} m/s |")
    lines.append("")
    return [lines]


if __name__ == '__main__':
    sys.path.insert(0, __file__.rsplit('/', 1)[0])
    import analyze
    for line in section(analyze.Session(sys.argv[1]))[0]:
        print(line)
