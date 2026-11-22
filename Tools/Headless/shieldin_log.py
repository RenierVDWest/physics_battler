#!/usr/bin/env python3
"""The other man in a shield, read from a playtest log that has no `shieldin` lines (22 November).

His playtest logs hold every part's place and turn each physics step (units/*.pose.csv.gz) but not the shield. A
headless report's SOLIDS lines give each part's solid in the part's own space and the board in the shield hand's
space; with them the board and every part are put back each step, as the game's own measure does
(ClipFighter.MeasureOthersInShield): a part counts when the board's disc reaches more than 2 cm into it.

    python3 Tools/Headless/shieldin_log.py PlaytestLogs/<date_time> <report with SOLIDS lines> [shielded unit names...]

Approximate: the board is taken as held fixed in the hand (it hangs on joints in the game), and the parts' solids are
a Medium's.
"""
import gzip, csv, sys, glob, os
import numpy as np

PARTS = ["Pelvis", "Spine", "Chest", "Head", "UpperArmL", "ForearmL", "HandL", "UpperArmR", "ForearmR", "HandR",
         "ThighL", "ShinL", "FootL", "ThighR", "ShinR", "FootR"]
DEEP, LONG = 0.02, 0.1


def rot(q, v):
    x, y, z, w = q
    u = np.array([x, y, z])
    return v + 2.0 * np.cross(u, np.cross(u, v) + w * v)


def solids(path):
    for line in open(path):
        if line.startswith("SOLIDS ") and "Board=" in line:
            out = {}
            for tok in line.split():
                if "=" in tok:
                    k, vals = tok.split("=")
                    out[k] = [float(x) for x in vals.split(",")]
            return out
    sys.exit("no SOLIDS line with a board in " + path)


def disc_gap(p, c, n, r):
    d = p - c
    over = d @ n
    flat = d - n * over
    f = np.linalg.norm(flat)
    nearest = c + (flat / f * r if f > r else flat)
    return np.linalg.norm(nearest - p)


def load(path):
    rows = {}
    with gzip.open(path, "rt") as f:
        rd = csv.reader(f)
        head = next(rd)
        col = {h: i for i, h in enumerate(head)}
        for row in rd:
            try:
                t = round(float(row[0]), 4)
                parts = {}
                for p in PARTS:
                    i = col[p + ".x"]
                    if row[i] == "":
                        continue
                    pos = np.array([float(row[i]), float(row[i + 1]), float(row[i + 2])])
                    q = np.array([float(row[i + 3]), float(row[i + 4]), float(row[i + 5]), float(row[i + 6])])
                    parts[p] = (pos, q)
                rows[t] = parts
            except (ValueError, KeyError, IndexError):
                pass
    return rows


def main():
    logdir, report = sys.argv[1], sys.argv[2]
    shielded = set(sys.argv[3:])
    so = solids(report)
    lc, ln, radius = np.array(so["Board"][:3]), np.array(so["Board"][3:6]), so["Board"][6]
    units = {os.path.basename(p).split(".")[0]: load(p) for p in sorted(glob.glob(os.path.join(logdir, "units", "*.pose.csv.gz")))}
    names = list(units)
    print(f"{logdir}: units {', '.join(names)}; the board {100 * radius:.0f} cm in radius")
    for a in names:
        if shielded and a not in shielded:
            continue
        for b in names:
            if a == b:
                continue
            A, B = units[a], units[b]
            times = sorted(set(A) & set(B))
            if len(times) < 2:
                continue
            dt = np.median(np.diff(times))
            watched = inside = 0.0
            run, deep_run, what = 0.0, 0.0, set()
            stretches = []
            by_part = {}
            for t in times:
                pa, pb = A[t], B[t]
                if "HandL" not in pa or "Pelvis" not in pa or "Pelvis" not in pb:
                    continue
                if np.linalg.norm(pa["Pelvis"][0] - pb["Pelvis"][0]) > 2.0:
                    continue
                watched += dt
                hp, hq = pa["HandL"]
                c, n = hp + rot(hq, lc), rot(hq, ln)
                n = n / np.linalg.norm(n)
                deepest, parts_in = 0.0, []
                for p, (pos, q) in pb.items():
                    if p not in so:
                        continue
                    s = so[p]
                    e0, e1, r = pos + rot(q, np.array(s[:3])), pos + rot(q, np.array(s[3:6])), s[6]
                    gap = min(disc_gap(e0 + (e1 - e0) * k / 8.0, c, n, radius) for k in range(9))
                    depth = r - gap
                    if depth > DEEP:
                        parts_in.append(p)
                        by_part[p] = by_part.get(p, 0.0) + dt
                    deepest = max(deepest, depth)
                if deepest > DEEP:
                    inside += dt
                    if run == 0.0:
                        start = t
                    run += dt
                    deep_run = max(deep_run, deepest)
                    what.update(parts_in)
                elif run > 0.0:
                    stretches.append((start, run, deep_run, sorted(what)))
                    run, deep_run, what = 0.0, 0.0, set()
            if run > 0.0:
                stretches.append((start, run, deep_run, sorted(what)))
            if watched <= 0.0:
                continue
            long_ = [s for s in stretches if s[1] >= LONG - 1e-4]
            print(f"  {b} in {a}'s shield: {inside:.2f} s of {watched:.0f} s near ({100 * inside / watched:.2f}%), {len(stretches)} stretches, "
                  f"{len(long_)} of {LONG} s or more, the longest {max([s[1] for s in stretches], default=0):.2f} s, "
                  f"the deepest {100 * max([s[2] for s in stretches], default=0):.1f} cm")
            if by_part:
                print("    by part: " + ", ".join(f"{p} {v:.2f} s" for p, v in sorted(by_part.items(), key=lambda kv: -kv[1])))
            for s in sorted(long_, key=lambda s: -s[1])[:12]:
                print(f"    at {s[0]:.2f} s for {s[1]:.2f} s, {100 * s[2]:.1f} cm deep: {', '.join(s[3])}")


if __name__ == "__main__":
    main()
