#!/usr/bin/env python3
"""The steps a fighter takes in a headless fight's film (the plan "Movement Styles as Offshoots of the Standard Set",
tests of steps S3 and S4): each swing of a foot, from its leaving the ground to its landing, in the measures the
recordings' long steps are given in (Tools/CompileCheck: "style steps"), so a made ginga can be set beside the pack's.

    python3 Tools/Mocap/Style/film_steps.py film.json [--from 1 --to 30] [--each]

Prints how many steps, how far and how long they are, how high the ankle rises, how far the hips range from side to
side, how far a standing foot slides while the other swings, and how different the steps are from one another; with
--each, every step.
"""
import json, sys
import numpy as np

PARTS = "Pelvis Spine Chest Head UpperArmL ForearmL HandL UpperArmR ForearmR HandR ThighL ShinL FootL ThighR ShinR FootR HeadTop HandLEnd HandREnd ToeL ToeR".split()
IX = {n: i for i, n in enumerate(PARTS)}


def main():
    args = sys.argv[1:]
    path = args[0]
    a = float(args[args.index('--from') + 1]) if '--from' in args else 0.0
    b = float(args[args.index('--to') + 1]) if '--to' in args else 1e9
    frames = json.load(open(path))
    who = frames[0]['u']
    mine = [f for f in frames if f['u'] == who and a <= f['t'] <= b]
    t = np.array([f['t'] for f in mine])
    p = np.array([f['p'] for f in mine], float).reshape(len(mine), len(PARTS), 3)
    d = np.array([f['d'] for f in mine], bool)
    steps = []
    for foot, name, other in ((0, 'FootL', 'FootR'), (1, 'FootR', 'FootL')):
        start = None
        for i in range(1, len(t)):
            if not d[i, foot] and d[i - 1, foot]:
                start = i - 1
            elif d[i, foot] and not d[i - 1, foot] and start is not None:
                A, B = p[start, IX[name]], p[i, IX[name]]
                far = float(np.linalg.norm((B - A)[[0, 2]]))
                lift = float(p[start:i + 1, IX[name], 1].max() - min(A[1], B[1]))
                slide = float(np.linalg.norm((p[i, IX[other]] - p[start, IX[other]])[[0, 2]]))
                steps.append(dict(foot='LR'[foot], at=float(t[start]), time=float(t[i] - t[start]), far=far, lift=lift, slide=slide, to=B[[0, 2]].copy()))
                start = None
    steps.sort(key=lambda s: s['at'])
    long = [s for s in steps if s['far'] >= 0.25]
    print(f"{path.split('/')[-1]} ({who}), {t[0]:.1f} to {t[-1]:.1f} s: {len(steps)} steps, {len(long)} of 25 cm or more")
    if long:
        far = np.array([s['far'] for s in long]); time = np.array([s['time'] for s in long]); lift = np.array([s['lift'] for s in long]); slide = np.array([s['slide'] for s in long])
        print(f"  they go {far.min():.2f} to {far.max():.2f} m (middle {np.median(far):.2f}), take {time.min():.2f} to {time.max():.2f} s (middle {np.median(time):.2f}; {np.median(time / far):.2f} s a metre), "
              f"the ankle rises {100 * lift.min():.0f} to {100 * lift.max():.0f} cm; the standing foot slides {100 * slide.mean():.1f} cm a step, {100 * slide.max():.1f} at most")
        gaps = np.diff([s['at'] for s in long])
        print(f"  from one step's start to the next: {gaps.min():.2f} to {gaps.max():.2f} s; no two steps alike: the nearest two differ by {100 * min(abs(x - y) for i, x in enumerate(far) for y in far[i + 1:]):.1f} cm in length" if len(long) > 2 else "")
    hips = p[:, IX['Pelvis']]
    print(f"  hips range {np.ptp(hips[:, 0]):.2f} by {np.ptp(hips[:, 2]):.2f} m over the ground, {hips[:, 1].min():.2f} to {hips[:, 1].max():.2f} m up")
    if '--each' in args:
        for s in steps:
            print(f"    {s['at']:6.2f} {s['foot']} {s['time']:.2f} s, {s['far']:.2f} m, rises {100 * s['lift']:.0f} cm, lands ({s['to'][0]:.2f}, {s['to'][1]:.2f}); the other foot slides {100 * s['slide']:.1f} cm")


if __name__ == '__main__':
    main()
