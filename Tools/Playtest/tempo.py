#!/usr/bin/env python3
"""The Fight Rhythm and tempo of a fight (milestone 7a): what the units do with their time, whether
their postures can be seen, how their blows are spaced, and whether they take turns.

    python3 Tools/Playtest/tempo.py SESSION [SESSION ...] [--csv OUT.csv]

With several sessions, the figures are pooled by body. analyze.py writes the same section into
its report. What it measures, per unit (on its feet only):
    - the share of time walking, in a blow, in a posture, blocking, and on the idle loop alone, and
      in each tempo state once the units have one (a `tempo` column; milestone 8);
    - postures a minute, and each posture's extra motion: how far it takes the hands, the head and
      the hips (their animation pose, against the unit's root) beyond the idle loop's own reach, in
      cm. A posture under 10 cm is flagged: it can't be seen;
    - energy: its range and how many charge-and-spend cycles it ran (a rise of 0.2 then a fall of 0.2);
    - blows a minute, the gaps between its own blows, and the share of blows in bursts (under 1 s
      after its last one);
    - blocks: blows that came at it, shields raised.
And per fight: within each pair, how often the next blow is the other unit's (alternation; 50% is
even chances), and the share of blows thrown while the target was itself mid-blow.
"""

import argparse
import os
import re
import sys

import numpy as np
import pandas as pd

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import analyze  # noqa: E402

PARTS = ["HandL", "HandR", "Head", "Pelvis"]
VISIBLE_CM = 10.0
BURST_GAP = 1.0


def _local_goals(unit):
    """Each part's animation pose against the root, in the root's facing (x right, z forward), metres: {part: (n, 3)}."""
    f, p = unit.frames, unit.pose
    n = min(len(f), len(p))
    yaw = np.radians(f["rootYaw"].astype(float).values[:n])
    rx, rz = f["rootX"].astype(float).values[:n], f["rootZ"].astype(float).values[:n]
    c, s = np.cos(yaw), np.sin(yaw)
    out = {}
    for part in PARTS:
        if part + ".gx" not in p:
            continue
        dx = p[part + ".gx"].astype(float).values[:n] - rx
        dz = p[part + ".gz"].astype(float).values[:n] - rz
        y = p[part + ".gy"].astype(float).values[:n]
        out[part] = np.stack([c * dx - s * dz, y, s * dx + c * dz], axis=1)
    return out, n


def _flag(f, name):
    if name not in f:
        return pd.Series(False, index=f.index)
    v = f[name]
    if v.dtype == object:
        return v.fillna("").astype(str).str.lower().isin(["1", "true"])
    return v.fillna(0).astype(float) > 0.5


def unit_tempo(session, unit):
    f = unit.frames
    if f.empty:
        return None
    goals, n = _local_goals(unit)
    f = f.iloc[:n]
    t = f["t"].astype(float).values
    dt = np.diff(t, prepend=t[0])
    alive = _flag(f, "alive") if "alive" in f else pd.Series(True, index=f.index)
    live = (alive & analyze.standing_mask(f)).values
    walk = (f["walk"].fillna(0).astype(float) > 0.5).values if "walk" in f else np.zeros(n, bool)
    move = (f["move"].fillna("").astype(str) != "").values if "move" in f else np.zeros(n, bool)
    posture = f["posture"].fillna("").astype(str).values if "posture" in f else np.array([""] * n)
    in_posture = posture != ""
    block = (f["block"].fillna(0).astype(float) > 0.05).values if "block" in f else np.zeros(n, bool)
    base = live & ~walk & ~move & ~in_posture & ~block
    # For the idle loop's reach: well clear of anything else that moves the body (the walk fading,
    # a blow and its fade, a hit's recoil, a raised shield).
    gait = (f["walk"].fillna(0).astype(float) > 0.3).values if "walk" in f else np.zeros(n, bool)
    busy = move.copy()
    for e in unit.events:
        if e.cat == "struck":
            busy |= (t >= e.t) & (t < e.t + 0.8)
    for end in t[1:][move[:-1] & ~move[1:]]:
        busy |= (t >= end) & (t < end + 0.8)
    quiet = live & ~gait & ~busy & ~block
    standing = float(dt[live].sum())
    if standing < 1.0:
        return None

    def share(mask):
        return float(dt[live & mask].sum()) / standing

    out = dict(unit=unit.label, body=unit.meta.get("body") or unit.label.split()[-1], standing=standing,
               walk=share(walk), attack=share(move), posture=share(in_posture & ~move), block=share(block & ~move), base=share(base))
    if "tempo" in f:
        tempo = f["tempo"].fillna("").astype(str).values
        for state in sorted(set(tempo[live]) - {""}):
            out["tempo_" + state] = share(tempo == state)

    # Postures: each run of one posture, its extra motion beyond the idle loop's reach (the
    # 90th percentile of how far each part wanders from its middle on the idle loop alone).
    reach, mid = {}, {}
    idle = quiet & ~in_posture
    for part, g in goals.items():
        if idle.sum() < 30:
            continue
        mid[part] = np.median(g[idle], axis=0)
        reach[part] = np.percentile(np.linalg.norm(g[idle] - mid[part], axis=1), 90)
    runs = []
    for value, t0, t1, i0, i1 in analyze.segments(pd.Series(posture), t):
        if value == "" or not live[i0:i1 + 1].any():
            continue
        sel = np.zeros(n, bool)
        sel[i0:i1 + 1] = True
        sel &= quiet
        if not mid or sel.sum() < 3:
            runs.append((value, float("nan")))
            continue
        extra = max(float(np.percentile(np.linalg.norm(goals[p][sel] - mid[p], axis=1), 95) - reach[p]) for p in mid)
        runs.append((value, max(0.0, extra) * 100.0))
    out["postures"] = len(runs)
    out["postures_min"] = len(runs) / standing * 60.0
    by_name = {}
    for value, cm in runs:
        by_name.setdefault(value, []).append(cm)
    out["posture_cm"] = {k: float(np.nanmedian(v)) if np.isfinite(v).any() else float("nan") for k, v in by_name.items()}
    out["posture_n"] = {k: len(v) for k, v in by_name.items()}
    out["hidden"] = sorted(k for k, v in out["posture_cm"].items() if np.isfinite(v) and v < VISIBLE_CM)
    out["base_reach_cm"] = float(np.mean(list(reach.values())) * 100.0) if reach else float("nan")

    # Energy cycles.
    if "energy" in f:
        e = f["energy"].astype(float).values[live]
        out["energy_min"], out["energy_max"] = float(e.min()), float(e.max())
        cycles, low, high, rising = 0, e[0], e[0], True
        for x in e:
            if rising:
                high = max(high, x)
                if high - x >= 0.2:
                    rising, low = False, x
            else:
                low = min(low, x)
                if x - low >= 0.2:
                    cycles += 1
                    rising, high = True, x
        out["energy_cycles"] = cycles

    # Blows.
    bl = analyze.blows(unit)
    starts = np.array([b["t"] for b in bl])
    out["blows"] = len(bl)
    out["blows_min"] = len(bl) / standing * 60.0
    gaps = np.diff(starts) if len(starts) > 1 else np.array([])
    out["gaps"] = gaps
    out["burst_share"] = float((gaps < BURST_GAP).mean()) if len(gaps) else 0.0

    # Defence.
    d = [e for e in unit.events if e.cat == "defence"]
    out["incoming"] = len(d)
    out["shields"] = sum(1 for e in d if e.text.startswith("raises the shield"))
    return out


def fight_tempo(session):
    """Alternation within each pair, and blows thrown into a target's own blow."""
    events = []
    for u in session.units:
        for b in analyze.blows(u):
            events.append((b["t"], u.label, b["target"], b["duration"]))
    events.sort()
    last, busy = {}, {}
    alt = same = into = 0
    for t, a, b, dur in events:
        key = frozenset((a, b))
        if key in last:
            if last[key] == a:
                same += 1
            else:
                alt += 1
        last[key] = a
        if busy.get(b, -1.0) > t:
            into += 1
        busy[a] = t + dur
    return dict(blows=len(events), alternate=alt, same=same,
                alternation=alt / max(1, alt + same), into=into / max(1, len(events)))


def section(session):
    """Markdown lines for analyze.py's report."""
    rows = [r for r in (unit_tempo(session, u) for u in session.units) if r]
    fight = fight_tempo(session)
    lines = ["## Fight Rhythm and tempo", ""]
    lines.append(f"Within each pair, the next blow was the other unit's {fight['alternation'] * 100:.0f}% of the time "
                 f"({fight['alternate']} of {fight['alternate'] + fight['same']}; 50% is even chances, over 55% reads as taking turns). "
                 f"{fight['into'] * 100:.0f}% of the {fight['blows']} blows were thrown while the target was itself mid-blow.")
    lines.append("")
    lines.append("| unit | on its feet | walking | in a blow | posture | blocking | idle loop | postures a min | blows a min | gap between blows (median, 10–90%) | in bursts (< 1 s) | energy (range, cycles) | blows at it / shields raised |")
    lines.append("| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |")
    for r in rows:
        g = r["gaps"]
        gaps = f"{np.median(g):.1f} s ({np.percentile(g, 10):.1f}–{np.percentile(g, 90):.1f})" if len(g) else "-"
        energy = f"{r.get('energy_min', float('nan')):.2f}–{r.get('energy_max', float('nan')):.2f}, {r.get('energy_cycles', 0)}"
        lines.append(f"| {r['unit']} | {r['standing']:.0f} s | {r['walk'] * 100:.0f}% | {r['attack'] * 100:.0f}% | {r['posture'] * 100:.0f}% | "
                     f"{r['block'] * 100:.0f}% | {r['base'] * 100:.0f}% | {r['postures_min']:.1f} | {r['blows_min']:.1f} | {gaps} | "
                     f"{r['burst_share'] * 100:.0f}% | {energy} | {r['incoming']} / {r['shields']} |")
    lines.append("")
    tempo_cols = sorted({k for r in rows for k in r if k.startswith("tempo_")})
    if tempo_cols:
        lines.append("| unit | " + " | ".join(c[6:] for c in tempo_cols) + " |")
        lines.append("| --- | " + " | ".join("---" for _ in tempo_cols) + " |")
        for r in rows:
            lines.append(f"| {r['unit']} | " + " | ".join(f"{r.get(c, 0) * 100:.0f}%" for c in tempo_cols) + " |")
        lines.append("")
    lines.append(f"**Posture extra motion** (median per posture: how far it takes the hands, head or hips beyond the idle loop's reach; "
                 f"under {VISIBLE_CM:.0f} cm can't be seen):")
    lines.append("")
    for r in rows:
        if not r["posture_n"]:
            continue
        names = ", ".join(f"{k} {r['posture_cm'][k]:.0f} cm ×{r['posture_n'][k]}" for k in sorted(r["posture_cm"], key=lambda k: -r["posture_n"][k]))
        hidden = f" **Can't be seen:** {', '.join(r['hidden'])}." if r["hidden"] else ""
        lines.append(f"- {r['unit']} (idle loop reach {r['base_reach_cm']:.0f} cm): {names}.{hidden}")
    lines.append("")
    return lines, rows, fight


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("sessions", nargs="+")
    ap.add_argument("--csv")
    args = ap.parse_args()
    pooled, fights = [], []
    for path in args.sessions:
        session = analyze.Session(path)
        lines, rows, fight = section(session)
        if len(args.sessions) == 1:
            print("\n".join(lines))
        pooled += rows
        fights.append(fight)
    if len(args.sessions) > 1:
        df = pd.DataFrame([{k: v for k, v in r.items() if not isinstance(v, (dict, list, np.ndarray))} for r in pooled])
        g = df.groupby("body")
        wsum = lambda c: g.apply(lambda x: (x[c] * x.standing).sum() / x.standing.sum())
        table = pd.DataFrame({"standing s": g.standing.sum().round(0), "walk": wsum("walk"), "blow": wsum("attack"), "posture": wsum("posture"),
                              "block": wsum("block"), "idle": wsum("base"),
                              "postures/min": g.postures.sum() / g.standing.sum() * 60, "blows/min": g.blows.sum() / g.standing.sum() * 60})
        print(table.round(2).to_string())
        gaps = np.concatenate([r["gaps"] for r in pooled if len(r["gaps"])])
        alt = sum(f["alternate"] for f in fights)
        same = sum(f["same"] for f in fights)
        blows = sum(f["blows"] for f in fights)
        print(f"gaps between a unit's own blows: median {np.median(gaps):.2f} s (10% {np.percentile(gaps, 10):.2f}, 90% {np.percentile(gaps, 90):.2f}); "
              f"in bursts (< 1 s): {(gaps < BURST_GAP).mean() * 100:.0f}%")
        print(f"alternation within pairs: {alt / max(1, alt + same) * 100:.0f}% ({alt} of {alt + same}); "
              f"blows into the target's own blow: {sum(f['into'] * f['blows'] for f in fights) / max(1, blows) * 100:.0f}%")
        cms = {}
        for r in pooled:
            for k, v in r["posture_cm"].items():
                cms.setdefault(k, []).append(v)
        print("posture extra motion (median cm): " + ", ".join(f"{k} {np.median(v):.0f}" for k, v in sorted(cms.items())))
        if args.csv:
            df.to_csv(args.csv, index=False)


if __name__ == "__main__":
    main()
