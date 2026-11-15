#!/usr/bin/env python3
"""The scorecard for a style as the fight poses it (the plan "Movement Styles as Offshoots of the Standard Set",
tests T1 and T4): what the first fighter of a headless fight's film does, in the same plain measures the
recordings are measured in, so a style laid over the standard set can be set beside the pack it came from.

    FIGHT_FILM="film.json,0.3,11.8" ... fight ...      (Tools/Mocap/Rhythm/film.py says how a film is written)
    python3 Tools/Mocap/Style/film_measure.py film.json [film2.json ...] [--windows 0.5-2.0,2.6-5.0]

For each film and each window of time: how far the trunk leans from upright (degrees), how high the hips are
(metres), how far each planted foot slides (cm a second, the mean and the worst), the least the knees are bent
(degrees: 0 is a straight leg; below 0 it bends backwards), and how fast the head goes at its fastest (m/s: a
pop shows as a spike).
"""
import json, math, sys
import numpy as np

PARTS = "Pelvis Spine Chest Head UpperArmL ForearmL HandL UpperArmR ForearmR HandR ThighL ShinL FootL ThighR ShinR FootR HeadTop HandLEnd HandREnd ToeL ToeR".split()
IX = {n: i for i, n in enumerate(PARTS)}


def load(path):
    frames = json.load(open(path))
    who = frames[0]['u']
    mine = [f for f in frames if f['u'] == who]
    t = np.array([f['t'] for f in mine])
    p = np.array([f['p'] for f in mine], float).reshape(len(mine), len(PARTS), 3)
    d = np.array([f['d'] for f in mine], bool)
    return who, t, p, d


def measure(t, p, d, a, b):
    k = (t >= a) & (t <= b)
    if k.sum() < 3:
        return None
    tt, pp, dd = t[k], p[k], d[k]
    dt = np.diff(tt)
    trunk = pp[:, IX['Chest']] - pp[:, IX['Pelvis']]
    lean = np.degrees(np.arctan2(np.linalg.norm(trunk[:, [0, 2]], axis=1), trunk[:, 1]))
    slides = []
    for foot, name in ((0, 'FootL'), (1, 'FootR')):
        v = np.linalg.norm(np.diff(pp[:, IX[name]][:, [0, 2]], axis=0), axis=1) / np.maximum(dt, 1e-4)
        held = dd[1:, foot] & dd[:-1, foot]
        slides += list(v[held])
    bends = []
    for hip, knee, ankle in (('ThighL', 'ShinL', 'FootL'), ('ThighR', 'ShinR', 'FootR')):
        u, w = pp[:, IX[knee]] - pp[:, IX[hip]], pp[:, IX[ankle]] - pp[:, IX[knee]]
        cos = np.sum(u * w, axis=1) / (np.linalg.norm(u, axis=1) * np.linalg.norm(w, axis=1))
        bends += list(np.degrees(np.arccos(np.clip(cos, -1, 1))))
    head = np.linalg.norm(np.diff(pp[:, IX['Head']], axis=0), axis=1) / np.maximum(dt, 1e-4)
    hips = np.linalg.norm(np.diff(pp[:, IX['Pelvis']], axis=0), axis=1) / np.maximum(dt, 1e-4)
    # (The head against the hips: what the body does on top of its travel.)
    rel = np.linalg.norm(np.diff(pp[:, IX['Head']] - pp[:, IX['Pelvis']], axis=0), axis=1) / np.maximum(dt, 1e-4)
    return dict(lean=(lean.mean(), lean.min(), lean.max()), hips=pp[:, IX['Pelvis'], 1].mean(),
                slide=(100 * np.mean(slides) if slides else 0.0, 100 * np.max(slides) if slides else 0.0),
                bend=(min(bends), max(bends)), head=head.max(), rel=(np.percentile(rel, 50), rel.max()), hipsfast=hips.max())


def main():
    args = [a for a in sys.argv[1:] if not a.startswith('--')]
    windows = [(0.5, 2.0, 'standing'), (2.6, 5.0, 'walking in'), (6.0, 8.3, 'circling'), (9.0, 10.5, 'backing away')]
    for i, a in enumerate(sys.argv):
        if a == '--windows':
            windows = [(float(w.split('-')[0]), float(w.split('-')[1]), w) for w in sys.argv[i + 1].split(',')]
            args.remove(sys.argv[i + 1])
    for path in args:
        who, t, p, d = load(path)
        print(f'{path.split("/")[-1]} ({who}):')
        for a, b, name in windows:
            m = measure(t, p, d, a, b)
            if m:
                print(f"  {name:13s} lean {m['lean'][0]:4.1f}° ({m['lean'][1]:4.1f} to {m['lean'][2]:4.1f}) | hips {m['hips']:.3f} m up | planted feet slide {m['slide'][0]:4.2f} cm/s, worst {m['slide'][1]:5.1f} | "
                      f"knees bent {m['bend'][0]:5.1f}° to {m['bend'][1]:5.1f}° | head on the hips goes {m['rel'][0]:.2f} m/s, fastest {m['rel'][1]:.2f}; hips fastest {m['hipsfast']:.2f}")


if __name__ == '__main__':
    main()
