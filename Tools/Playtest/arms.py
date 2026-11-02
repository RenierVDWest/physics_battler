#!/usr/bin/env python3
"""Milestone 9c: arms, wrists and the shield, measured on the physical body of each standing unit.

    python3 Tools/Playtest/arms.py <session folder>
analyze.py prints the same section. Works on older logs too, so each playtest can be set against
the last.

What it measures (the plan: https://claude.ai/artifact/UJNwdKChsM7vidDhArKF9W):
    - wrists: the hand's turn against the forearm, split into the bend (swing) and the roll about
      the forearm's line, as Tools/Playtest/reference_wrists.py measures the reference attack FBXs
      (median 23 degrees, 95th 45, without the roll);
    - the shield: how fast its face turns with no hit in the last second, how far the shield hand
      is from the chest;
    - after a hit: how many times the struck part turns back in the next second (a buzz), how far
      it is off its pose and how fast it spins;
    - shield contacts: each time a shield overlapped another unit's body, how deep and how long
      (target: under 2 cm in 95%, none longer than 0.2 s);
    - after a blow stopped by a body or a shield: how far the attacker's weapon hand is from its pose;
    - joints past their limits while standing, on the pose and on the physical body;
    - the swing's smoothness: the weapon tip's acceleration in blows and how often it turns back on
      itself, physical and posed, and the physical tip off the pose.
"""
import re
import sys

import numpy as np
import pandas as pd

# The reference attacks' wrists (Tools/Playtest/reference_wrists.py over the ten attack FBXs).
REFERENCE_BEND = (23.0, 45.0)


def _q(df, p):
    return df[[p + '.qx', p + '.qy', p + '.qz', p + '.qw']].to_numpy(dtype=float)


def _p(df, p):
    return df[[p + '.x', p + '.y', p + '.z']].to_numpy(dtype=float)


def _mul(a, b):
    x1, y1, z1, w1 = a.T
    x2, y2, z2, w2 = b.T
    return np.stack([w1 * x2 + x1 * w2 + y1 * z2 - z1 * y2, w1 * y2 - x1 * z2 + y1 * w2 + z1 * x2,
                     w1 * z2 + x1 * y2 - y1 * x2 + z1 * w2, w1 * w2 - x1 * x2 - y1 * y2 - z1 * z2], 1)


def _conj(a):
    return a * np.array([-1, -1, -1, 1])


def _rotate(q, v):
    v = np.broadcast_to(v, (len(q), 3))
    qv = np.concatenate([v, np.zeros((len(q), 1))], 1)
    return _mul(_mul(q, qv), _conj(q))[:, :3]


def wrist(pose, side):
    """Per row: the wrist's bend (degrees, roll excluded) and its roll about the forearm's line."""
    qf, qh = _q(pose, 'Forearm' + side), _q(pose, 'Hand' + side)
    rel = _mul(_conj(qf), qh)
    rel[rel[:, 3] < 0] *= -1
    # The forearm's line in its own frame (every null has identity rotation at rest, so the
    # hand's position from the elbow, in the forearm's frame, is the same axis every row).
    ax = _rotate(_conj(qf), _p(pose, 'Hand' + side) - _p(pose, 'Forearm' + side))
    ax = np.median(ax / np.maximum(np.linalg.norm(ax, axis=1, keepdims=True), 1e-9), 0)
    ax /= np.linalg.norm(ax)
    along = rel[:, :3] @ ax
    roll = np.degrees(2 * np.arctan2(along, rel[:, 3]))
    roll = (roll + 180) % 360 - 180
    twist = np.concatenate([along[:, None] * ax, rel[:, 3:4]], 1)
    twist /= np.maximum(np.linalg.norm(twist, axis=1, keepdims=True), 1e-9)
    swing = _mul(rel, _conj(twist))
    bend = np.degrees(2 * np.arccos(np.clip(np.abs(swing[:, 3]), 0, 1)))
    return bend, roll


def _units(session):
    out = []
    for u in session.units:
        if u.pose.empty or u.frames.empty:
            continue
        n = min(len(u.pose), len(u.frames))
        out.append((u, u.pose.iloc[:n].reset_index(drop=True), u.frames.iloc[:n].reset_index(drop=True)))
    return out


def _standing(fr):
    state = fr['state'].astype(str).to_numpy() if 'state' in fr else np.array(['Standing'] * len(fr))
    alive = fr['alive'].astype(str).isin(['True', '1', '1.0']).to_numpy() if 'alive' in fr else np.ones(len(fr), bool)
    return (state == 'Standing') & alive


def _in_blow(fr):
    if 'move' not in fr:
        return np.zeros(len(fr), bool)
    m = fr['move']
    return (m.notna() & ~m.astype(str).str.strip().isin(['', 'nan', 'None', '<NA>'])).to_numpy()


HIT = re.compile(r"^(\w+) struck (\w \w+)'s (\w+) at ([\d.]+) m/s: (.*)$")


def _hits(session):
    for e in session.events:
        if e.cat != 'hit':
            continue
        m = HIT.match(e.text)
        if m:
            move, target, part, speed, rest = m.groups()
            yield e.t, e.who, move, target, part, float(speed), rest


SHIELD = re.compile(r"^(.*)'s shield in (.*) (\w+) for ([\d.]+) s, ([\d.]+) cm deep at most$")


def shield_contacts(session):
    """Milestone 9c: each time a shield overlapped another unit's body (the recorder's "shield" events)."""
    rows = []
    for e in session.events:
        if e.cat != 'shield':
            continue
        m = SHIELD.match(e.text)
        if m:
            unit, other, part, dur, depth = m.groups()
            rows.append((e.t, unit, other, part, float(dur), float(depth) / 100.0))
    return pd.DataFrame(rows, columns=['t', 'unit', 'other', 'part', 'duration', 'depth'])


def measure(session):
    units = _units(session)
    by_label = {u.label: (u, pose, fr) for u, pose, fr in units}
    hits = list(_hits(session))
    hit_times = np.array([h[0] for h in hits]) if hits else np.zeros(0)
    r = {'wrists': [], 'shield': [], 'after_hit': [], 'stopped': [], 'limits': [], 'swing': []}

    for u, pose, fr in units:
        stand = _standing(fr)
        blow = _in_blow(fr)
        for side, what in (('R', 'weapon'), ('L', 'off hand')):
            if 'Hand' + side + '.qx' not in pose:
                continue
            bend, roll = wrist(pose, side)
            for label, m in (('guard', stand & ~blow), ('blows', stand & blow)):
                if m.sum() < 30:
                    continue
                r['wrists'].append((u.label, what, label, np.percentile(bend[m], 50), np.percentile(bend[m], 95),
                                    np.percentile(np.abs(roll[m]), 95), int(m.sum())))

        # The swing's smoothness: the weapon tip in blows (steps inside one blow, standing), physical
        # and posed. Acceleration and how often it turns back on itself; the physical tip off the pose.
        if 'tip.x' in pose and 'ghostTip.x' in pose:
            moves = fr['move'].astype(str).to_numpy() if 'move' in fr else np.array([''] * len(fr))
            n = min(len(pose), len(fr))
            same = np.zeros(n, bool)
            same[1:-1] = (moves[1:n - 1] == moves[:n - 2]) & (moves[1:n - 1] == moves[2:n])
            ok = same & blow[:n] & stand[:n]
            row = [u.label]
            for kind in ('tip', 'ghostTip'):
                tip = pose[[kind + '.x', kind + '.y', kind + '.z']].to_numpy(dtype=float)[:n]
                v = np.diff(tip, axis=0) * 60.0
                acc = np.zeros(n)
                acc[1:-1] = np.linalg.norm(np.diff(v, axis=0), axis=1) * 60.0
                back = np.zeros(n, bool)
                fast = np.linalg.norm(v, axis=1) > 1.0
                back[1:-1] = fast[:-1] & fast[1:] & ((v[:-1] * v[1:]).sum(1) < 0)
                good = ok & np.isfinite(acc)
                row += [np.median(acc[good]) if good.any() else np.nan, np.percentile(acc[good], 95) if good.any() else np.nan,
                        int(back[good].sum())]
            off = np.linalg.norm(pose[['tip.x', 'tip.y', 'tip.z']].to_numpy(dtype=float)[:n]
                                 - pose[['ghostTip.x', 'ghostTip.y', 'ghostTip.z']].to_numpy(dtype=float)[:n], axis=1)
            good = ok & np.isfinite(off)
            if good.sum() >= 30:
                row += [np.percentile(off[good], 95), int(good.sum())]
                r['swing'].append(tuple(row))

        # The shield: its face is the shield hand's down (the centre grip, before and after 9b).
        if 'shield' in fr and fr['shield'].astype(str).isin(['Guard', 'Track', 'Block', 'High']).any():
            t = pose['t'].to_numpy()
            face = _rotate(_q(pose, 'HandL'), np.array([0.0, -1.0, 0.0]))
            dt = np.maximum(np.diff(t), 1e-4)
            turn = np.degrees(np.arccos(np.clip((face[1:] * face[:-1]).sum(1), -1, 1))) / dt
            quiet = stand[1:] & (dt < 0.05)
            if len(hit_times):
                quiet &= np.array([np.all(np.abs(hit_times - x) > 1.0) for x in t[1:]])
            out = np.linalg.norm(_p(pose, 'HandL') - _p(pose, 'Chest'), axis=1)
            if quiet.sum() > 30:
                r['shield'].append((u.label, np.percentile(turn[quiet], 50), np.percentile(turn[quiet], 99),
                                    np.median(out[1:][quiet]), int(quiet.sum())))

        # (Within a second of a blow ending, apart: the arm handed back from the blow.)
        t_fr = fr['t'].to_numpy(dtype=float)
        since = np.full(len(fr), 99.0)
        last = -99.0
        for i in range(len(fr)):
            if blow[i]:
                last = t_fr[i]
            since[i] = t_fr[i] - last
        after = stand & ~blow & (since < 1.0)
        for col, label in (('posePast', 'pose'), ('jointPast', 'physical')):
            if col not in fr:
                continue
            for name, m in (('guard', stand & ~blow & ~after), ('after blows', after), ('blows', stand & blow)):
                if m.sum() < 30:
                    continue
                past = fr[col].to_numpy(dtype=float)[m]
                which = fr[('poseJoint' if label == 'pose' else 'joint')].astype(str).to_numpy()[m][past > 10]
                worst = pd.Series(which).value_counts().index[0] if len(which) else '-'
                r['limits'].append((u.label, label, name, float((past > 10).mean()), worst))

    for t, attacker, move, target, part, speed, rest in hits:
        blocked = 'shield' in rest
        if target in by_label:
            _, pose, fr = by_label[target]
            seg = pose[(pose['t'] >= t - 0.02) & (pose['t'] <= t + 1.0)]
            if len(seg) >= 30 and part + '.qx' in seg:
                qa = _q(seg, part)
                rel = _mul(qa[1:], _conj(qa[:-1]))
                rel[rel[:, 3] < 0] *= -1
                ang = 2 * np.arccos(np.clip(rel[:, 3], -1, 1))
                axis = rel[:, :3] / np.maximum(np.linalg.norm(rel[:, :3], axis=1, keepdims=True), 1e-9)
                dt = np.maximum(np.diff(seg['t'].to_numpy()), 1e-4)
                w = axis * (ang / dt)[:, None]
                main = np.linalg.svd(w, full_matrices=False)[2][0]
                s = w @ main
                sgn = np.sign(s[np.abs(s) > 2.0])
                fs = fr[(fr['t'] >= t - 0.02) & (fr['t'] <= t + 1.0)]
                off = float(np.nanmax(fs['aerr_' + part])) if 'aerr_' + part in fs else np.nan
                r['after_hit'].append(('shield' if blocked else 'body', part, int((np.diff(sgn) != 0).sum()), off,
                                       float(np.abs(s).max())))
        if 'swing stopped' in rest and attacker in by_label:
            _, _, fr = by_label[attacker]
            fs = fr[(fr['t'] >= t) & (fr['t'] <= t + 1.0)]
            if len(fs) and 'err_HandR' in fs:
                r['stopped'].append(('shield' if blocked else 'body', float(fs['err_HandR'].max()),
                                     float(fs['aerr_HandR'].max())))
    return r


def _pct(a, q):
    return f"{np.percentile(a, q):.0f}" if len(a) else "-"


def section(session):
    r = measure(session)
    L = ["## Arms, wrists and the shield", ""]
    L.append(f"Wrists: the bend without the roll, median / 95th (the reference attacks: {REFERENCE_BEND[0]:.0f}° / {REFERENCE_BEND[1]:.0f}°), "
             "and the roll's 95th.")
    L.append("")
    if r['wrists']:
        L.append("| unit | hand | when | bend median | bend 95th | roll 95th | steps |")
        L.append("| --- | --- | --- | --- | --- | --- | --- |")
        for unit, what, when, med, p95, roll, n in r['wrists']:
            flag = "  **bent**" if med > 25 or p95 > 45 else ""
            L.append(f"| {unit} | {what} | {when} | {med:.0f}°{flag} | {p95:.0f}° | {roll:.0f}° | {n} |")
        L.append("")
    if r['swing']:
        L.append("The swing's smoothness: the weapon tip in blows, physical and posed. Its acceleration (median / 95th; "
                 "push A ~80 / ~850 m/s²), how many steps it turned back on itself, and the physical tip off the posed one "
                 "(95th; in blows the weapon arm is posed exactly, so ~0). The first try at straight wrists: 800 / 7000 m/s², "
                 "~450 turns back, 1.6 m off.")
        L.append("")
        L.append("| unit | physical accel | physical turns back | posed accel | posed turns back | physical off the pose | steps |")
        L.append("| --- | --- | --- | --- | --- | --- | --- |")
        for unit, pm, p95, pb, gm, g95, gb, off, n in r['swing']:
            flag = "  **shaking**" if pm > 3 * max(gm, 1.0) or off > 0.1 else ""
            L.append(f"| {unit} | {pm:.0f} / {p95:.0f} m/s²{flag} | {pb} | {gm:.0f} / {g95:.0f} m/s² | {gb} | {off:.2f} m | {n} |")
        L.append("")
    if r['shield']:
        L.append("The shield with no hit in the last second: how fast its face turns (median / 99th; target ≤ 20 / ≤ 250°/s), "
                 "and how far the shield hand is from the chest (median; target 0.30–0.40 m at medium size).")
        L.append("")
        L.append("| unit | turning median | turning 99th | hand to chest | steps |")
        L.append("| --- | --- | --- | --- | --- |")
        for unit, med, p99, out, n in r['shield']:
            L.append(f"| {unit} | {med:.0f}°/s | {p99:.0f}°/s | {out:.2f} m | {n} |")
        L.append("")
    sc = shield_contacts(session)
    if len(sc):
        p95 = np.percentile(sc.depth, 95) * 100
        under = (sc.depth < 0.02).mean() * 100
        flag = "  **too deep**" if under < 95 else ""
        flag2 = "  **too long**" if sc.duration.max() > 0.2 else ""
        L.append(f"Shield contacts with other bodies: {len(sc)}; depth 95th {p95:.1f} cm, {under:.0f}% under 2 cm (target 95%){flag}, "
                 f"deepest {sc.depth.max() * 100:.1f} cm; longest {sc.duration.max():.2f} s (target under 0.2 s){flag2}.")
        L.append("")
        L.append("| shield of | contacts | depth 95th | deepest | longest | most often against |")
        L.append("| --- | --- | --- | --- | --- | --- |")
        for unit, g in sc.groupby('unit'):
            L.append(f"| {unit} | {len(g)} | {np.percentile(g.depth, 95) * 100:.1f} cm | {g.depth.max() * 100:.1f} cm | {g.duration.max():.2f} s "
                     f"| {g.part.mode().iloc[0]} |")
        L.append("")
    if r['after_hit']:
        d = pd.DataFrame(r['after_hit'], columns=['on', 'part', 'reversals', 'off', 'spin'])
        L.append("In the second after a hit, the struck part (target: turns back at most once, ≤ 30° off its pose, ≤ 15 rad/s):")
        L.append("")
        L.append("| struck | hits | turns back (median / max) | off its pose (median / max) | spin (median / max) |")
        L.append("| --- | --- | --- | --- | --- |")
        for on, g in d.groupby('on'):
            L.append(f"| {on} | {len(g)} | {_pct(g.reversals, 50)} / {g.reversals.max()} | {_pct(g.off.dropna(), 50)}° / {g.off.max():.0f}° "
                     f"| {_pct(g.spin, 50)} / {g.spin.max():.0f} rad/s |")
        L.append("")
    if r['stopped']:
        d = pd.DataFrame(r['stopped'], columns=['by', 'err', 'aerr'])
        L.append("In the second after a blow was stopped, the attacker's weapon hand off its pose (target ≤ 0.2 m, ≤ 40°):")
        L.append("")
        L.append("| stopped by | blows | distance (median / max) | turn (median / max) |")
        L.append("| --- | --- | --- | --- |")
        for by, g in d.groupby('by'):
            L.append(f"| {by} | {len(g)} | {g.err.median():.2f} / {g.err.max():.2f} m | {g.aerr.median():.0f}° / {g.aerr.max():.0f}° |")
        L.append("")
    held = []
    for u, pose, fr in _units(session):
        if 'lvl_ArmR' not in fr:
            continue
        m = ((fr['state'] == 'Standing') & (fr['lvl_ArmR'] == 1) & fr['move'].isna()).to_numpy()
        if m.sum() >= 30:
            up = fr['aerr_UpperArmR'].to_numpy(float)[m]
            hand = fr['aerr_HandR'].to_numpy(float)[m]
            held.append((u.label, np.median(up), np.percentile(up, 95), np.median(hand), np.percentile(hand, 95), int(m.sum())))
    if held:
        # (Milestone 10: the held arms' joint limits fought the guard's pose; the arm stuck 30-40° off it.
        # The 21:30 playtest, before them: about 2° / 9° at the shoulder.)
        L.append("The weapon arm held in the guard (pinned, no blow): its turn off the pose (median / 95th; target ≤ 5° / ≤ 15° at the shoulder).")
        L.append("")
        L.append("| unit | upper arm | hand | steps |")
        L.append("| --- | --- | --- | --- |")
        for unit, um, u95, hm, h95, n in held:
            flag = "  **stuck off its pose**" if um > 10 else ""
            L.append(f"| {unit} | {um:.0f}° / {u95:.0f}°{flag} | {hm:.0f}° / {h95:.0f}° | {n} |")
        L.append("")
    if r['limits']:
        L.append("Standing: the share of steps with a joint more than 10° past its limits (target: none), and the joint most often.")
        L.append("")
        L.append("| unit | body | when | past 10° | most often |")
        L.append("| --- | --- | --- | --- | --- |")
        for unit, body, when, share, worst in r['limits']:
            L.append(f"| {unit} | {body} | {when} | {share * 100:.0f}% | {worst} |")
        L.append("")
    return [L]


if __name__ == '__main__':
    sys.path.insert(0, __file__.rsplit('/', 1)[0])
    import analyze
    for line in section(analyze.Session(sys.argv[1]))[0]:
        print(line)
