#!/usr/bin/env python3
"""Each walking, jogging and running loop in the two clip libraries measured as a gait, at a Medium's size
(26 November 2026, for the plan "Walking that feels like a real person"):

    python3 Tools/Mocap/Locomotion/gait_survey.py [csv]

speed, steps a minute, step length, the walk ratio (step length over steps a minute), how far the hips rise and
fall and sway in a stride, how long both feet are down, how long none is, and the Froude number (speed squared
over gravity times leg length: bodies moving alike have the same). Read against what people do at that speed, it
shows which loops are one person's ordinary walk and which are not, and how far the set is from one body.
"""
import json, math, os, sys
import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, '..', '..', '..'))
RES = os.path.join(ROOT, 'Assets', '_Battler', 'Resources', 'Locomotion')
LEG = 0.845  # a Medium's leg length, metres (the playtest logs' meta.json)
G = 9.81


def survey():
    rows = []
    for fn in ('LocoClips.json', 'CombatClips.json'):
        d = json.load(open(os.path.join(RES, fn)))
        bones = d['bones']
        hips = bones.index('Hips')
        nb = len(bones)
        for c in d['clips']:
            if c.get('role') != 'cycle' or not c.get('loop'):
                continue
            n = c['frames']
            T = c['duration']
            p = np.array(c['points']).reshape(n, nb, 3) * LEG
            con = np.array(c['contacts']).reshape(n, 2)
            down = con > 0
            # landings of each foot: up -> down
            def landings(k):
                return int(sum(1 for i in range(n) if down[i, k] and not down[i - 1, k]))
            steps = landings(0) + landings(1)
            steps = steps if steps > 0 else 2
            v = c['speed'] * LEG
            cadence = steps / T * 60.0
            step_len = v * T / steps
            hy = p[:, hips, 1]
            hx = p[:, hips, 0]
            both = float(np.mean(down[:, 0] & down[:, 1]))
            none = float(np.mean(~down[:, 0] & ~down[:, 1]))
            rows.append(dict(lib=fn.replace('Clips.json', ''), name=c['name'], speed=v, direction=c.get('direction', 0.0), T=T, steps=steps, cadence=cadence,
                             step=step_len, ratio=step_len / cadence * 1000.0, bob=(hy.max() - hy.min()) * 100, sway=(hx.max() - hx.min()) * 100,
                             hips=float(hy.mean()) * 100, both=both * 100, none=none * 100, froude=v * v / (G * LEG), about=c.get('about', '')))
    return rows


if __name__ == '__main__':
    rows = survey()
    print(f"{'clip':24s} {'m/s':>5s} {'dir':>5s} {'T s':>5s} {'steps':>5s} {'st/min':>6s} {'step m':>6s} {'ratio':>6s} {'bob cm':>6s} {'sway':>5s} {'hips':>5s} {'both%':>5s} {'air%':>5s} {'Fr':>5s}")
    for r in rows:
        print(f"{r['name']:24s} {r['speed']:5.2f} {r['direction']:5.0f} {r['T']:5.2f} {r['steps']:5d} {r['cadence']:6.0f} {r['step']:6.2f} {r['ratio']:6.1f} {r['bob']:6.1f} {r['sway']:5.1f} {r['hips']:5.0f} {r['both']:5.0f} {r['none']:5.0f} {r['froude']:5.2f}")
    if len(sys.argv) > 1:
        import csv
        with open(sys.argv[1], 'w', newline='') as f:
            w = csv.DictWriter(f, fieldnames=list(rows[0].keys()))
            w.writeheader()
            w.writerows(rows)
