#!/usr/bin/env python3
"""A survey of movement clips for the fight rhythm (the plan "Fight Rhythm from Recordings"): every Mixamo FBX in
a folder is sampled at 30 frames a second and measured, so a pack can be read before anything is built from it.

    python3 Tools/Mocap/Rhythm/rhythm_survey.py <folder with the FBX files> [--save <folder for the samples>]

For each clip: its length; whether it loops (how far its last pose is from its first, the travel taken out); how
far and which way the hips travel over the ground against the way the body faces (forward, right; metres) and the
speed that is; how far it turns; how high the hips are (lowest, middle, highest; the standing height is the
leg); how much of the time each foot is on the ground, both, neither (in the air); how many times each foot lands;
whether a hand is on the ground; and how far the hands move against the chest (how busy the arms are).
"""
import json, math, os, sys
import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(HERE))
import mixamo_rhythm as mr

RATE = 30
BONES = ['Hips', 'Spine2', 'Head', 'LeftHand', 'RightHand', 'LeftUpLeg', 'RightUpLeg', 'LeftFoot', 'RightFoot',
         'LeftToeBase', 'RightToeBase', 'LeftArm', 'RightArm']


def sample(path):
    sk = mr.Skeleton(path)
    n = max(2, int(round(sk.length * RATE)) + 1)
    frames = np.zeros((n, len(BONES), 3))
    for f in range(n):
        pose = sk.pose(min(sk.length, f / RATE))
        for i, b in enumerate(BONES):
            key = b if b in pose else 'mixamorig:' + b
            frames[f, i] = pose[key][1]
    return sk.length, frames


def downs(y, speed, rest):
    """A foot is down while it is low (within 4 cm of where it rests) and nearly still."""
    return (y < rest + 0.04) & (speed < 0.6)


def measure(name, length, F):
    b = {n: i for i, n in enumerate(BONES)}
    n = len(F)
    hips = F[:, b['Hips']]
    across = F[:, b['RightUpLeg']] - F[:, b['LeftUpLeg']]
    yaw = np.degrees(np.unwrap(np.arctan2(-across[:, 2], across[:, 0])))
    y0 = math.radians(yaw[0])
    fwd, right = np.array([math.sin(y0), 0, math.cos(y0)]), np.array([math.cos(y0), 0, -math.sin(y0)])
    went = hips[-1] - hips[0]
    path = float(np.sum(np.linalg.norm(np.diff(hips[:, [0, 2]], axis=0), axis=1)))
    leg = float(np.median(F[:, b['LeftUpLeg'], 1]))
    # (How far the last pose is from the first, the hips' travel taken out: under about 6 cm a clip loops.)
    loop = float(np.mean(np.linalg.norm((F[-1] - hips[-1]) - (F[0] - hips[0]), axis=1)))
    feet = {}
    for side in ('Left', 'Right'):
        toe = F[:, b[side + 'ToeBase']]
        heel = F[:, b[side + 'Foot']]
        speed = np.concatenate([[0], np.linalg.norm(np.diff(toe, axis=0), axis=1) * RATE])
        feet[side] = downs(np.minimum(toe[:, 1], heel[:, 1] - 0.06), speed, float(np.percentile(np.minimum(toe[:, 1], heel[:, 1] - 0.06), 5)))
    l, r = feet['Left'], feet['Right']
    lands = lambda d: int(np.sum(d[1:] & ~d[:-1]))
    hand_low = float(min(F[:, b['LeftHand'], 1].min(), F[:, b['RightHand'], 1].min()))
    chest = F[:, b['Spine2']]
    busy = max(float(np.sum(np.linalg.norm(np.diff(F[:, b[h]] - chest, axis=0), axis=1))) / max(0.1, length) for h in ('LeftHand', 'RightHand'))
    return dict(name=name, length=length, loop=loop, forward=float(went @ fwd), right=float(went @ right), path=path,
                speed=path / max(0.1, length), turn=float(yaw[-1] - yaw[0]), turn_most=float(np.abs(yaw - yaw[0]).max()),
                hips=(float(hips[:, 1].min()), float(np.median(hips[:, 1])), float(hips[:, 1].max())), leg=leg,
                both=float(np.mean(l & r)), one=float(np.mean(l ^ r)), air=float(np.mean(~l & ~r)), lands=(lands(l), lands(r)),
                hand_low=hand_low, head_low=float(F[:, b['Head'], 1].min()), busy=busy)


def main(folder, save):
    rows = []
    for root, _, files in os.walk(folder):
        for fn in sorted(files):
            if not fn.lower().endswith('.fbx'):
                continue
            try:
                length, F = sample(os.path.join(root, fn))
            except Exception as e:      # a character file with no animation in it, say
                print(f'{fn}: not read ({e})')
                continue
            if length < 0.05:
                print(f'{fn}: no animation')
                continue
            name = os.path.splitext(fn)[0]
            rows.append(measure(name, length, F))
            if save:
                os.makedirs(save, exist_ok=True)
                with open(os.path.join(save, name.replace(' ', '_') + '.json'), 'w') as f:
                    json.dump(dict(clip=name, rate=RATE, length=round(length, 4), bones=BONES, frames=np.round(F, 4).reshape(len(F), -1).tolist()), f, separators=(',', ':'))
    print(f"{'clip':30} {'s':>5} {'loop':>5} {'fwd':>6} {'right':>6} {'m/s':>5} {'turn':>5} {'most':>5}  hips low/mid/high   both  one  air  lands  hand  head  arms")
    for m in rows:
        print(f"{m['name'][:30]:30} {m['length']:5.2f} {100 * m['loop']:4.0f}c {m['forward']:6.2f} {m['right']:6.2f} {m['speed']:5.2f} {m['turn']:5.0f} {m['turn_most']:5.0f}  "
              f"{m['hips'][0]:.2f} {m['hips'][1]:.2f} {m['hips'][2]:.2f}  {100 * m['both']:4.0f}% {100 * m['one']:3.0f}% {100 * m['air']:3.0f}%  {m['lands'][0]}+{m['lands'][1]}  "
              f"{m['hand_low']:5.2f} {m['head_low']:5.2f} {m['busy']:5.2f}")


if __name__ == '__main__':
    args = sys.argv[1:]
    save = None
    if '--save' in args:
        i = args.index('--save')
        save = args[i + 1]
        del args[i:i + 2]
    main(args[0], save)
