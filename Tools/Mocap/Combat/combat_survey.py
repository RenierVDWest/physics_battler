#!/usr/bin/env python3
"""A survey of the sampled combat clips (Tools/Mocap/Combat/samples): for each, its length, how far
and which way the hips travel, how far it turns, whether it loops, its steps (each foot's landings),
whether the hands hold one thing together (two-handed), and its blows: the moments the right (and
left) hand peaks in speed, with that speed and where the hand is then against the body.

    python3 Tools/Mocap/Combat/combat_survey.py [name filter]
"""
import json, math, os, sys
import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
SAMPLES = os.path.join(HERE, 'samples')


def load(path):
    d = json.load(open(path))
    b = {n: i for i, n in enumerate(d['bones'])}
    f = np.array(d['frames']).reshape(len(d['frames']), len(d['bones']), 3)
    return d, b, f


def yaw_of(f, b):
    r = f[:, b['RightUpLeg']] - f[:, b['LeftUpLeg']]
    return np.degrees(np.arctan2(-r[:, 2], r[:, 0]))   # facing: right is +x at yaw 0, forward +z


def main(filt):
    for name in sorted(os.listdir(SAMPLES)):
        if filt and filt not in name:
            continue
        d, b, f = load(os.path.join(SAMPLES, name))
        rate = d['rate']
        n = len(f)
        hips = f[:, b['Hips']]
        yaw = yaw_of(f, b)
        turn = (yaw[-1] - yaw[0] + 180) % 360 - 180
        y0 = math.radians(yaw[0])
        move = hips[-1] - hips[0]
        fwd = move[0] * math.sin(y0) + move[2] * math.cos(y0)
        side = move[0] * math.cos(y0) - move[2] * math.sin(y0)
        loop = np.linalg.norm((f[-1] - f[-1, b['Hips']]) - (f[0] - f[0, b['Hips']]), axis=1).max()
        steps = []
        for side_name in ('Left', 'Right'):
            foot = f[:, b[side_name + 'Foot']]
            h = foot[:, 1]
            down = h < h.min() + 0.035
            v = np.r_[0, np.linalg.norm(np.diff(foot[:, [0, 2]], axis=0), axis=1) * rate]
            planted = down & (v < 0.4)
            lands = int(np.sum(planted[1:] & ~planted[:-1]))
            steps.append(lands)
        gap = np.linalg.norm(f[:, b['RightHand']] - f[:, b['LeftHand']], axis=1)
        two = float(np.median(gap)) < 0.22
        blows = []
        for hand in ('RightHand', 'LeftHand'):
            p = f[:, b[hand]]
            sp = np.r_[0, np.linalg.norm(np.diff(p, axis=0), axis=1) * rate]
            sp = np.convolve(sp, np.ones(3) / 3, mode='same')
            for i in range(2, n - 2):
                if sp[i] > 3.0 and sp[i] == sp[max(0, i - 6):i + 7].max():
                    rel = p[i] - hips[i]
                    yy = math.radians(yaw[i])
                    rf = rel[0] * math.sin(yy) + rel[2] * math.cos(yy)
                    rs = rel[0] * math.cos(yy) - rel[2] * math.sin(yy)
                    blows.append(f"{hand[0]} {i / rate:.2f}s {sp[i]:.1f}m/s @({rs:+.2f},{rel[1]:+.2f},{rf:+.2f})")
        print(f"{name[:-5]:36s} {d['length']:5.2f}s  hips fwd {fwd:+.2f} side {side:+.2f} m  turn {turn:+5.0f}  "
              f"{'loops' if loop < 0.08 else 'ends ' + format(loop, '.2f')}  steps L{steps[0]} R{steps[1]}  {'two-handed' if two else ''}")
        for bl in blows:
            print('      blow', bl)


if __name__ == '__main__':
    main(sys.argv[1] if len(sys.argv) > 1 else None)
