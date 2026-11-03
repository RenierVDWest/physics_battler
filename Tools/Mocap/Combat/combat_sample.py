#!/usr/bin/env python3
"""Step 1 of the combat clips (the stances and blows redone from reference): samples every Mixamo FBX
clip in a folder into a small sample file, as the locomotion's sampler does, so the 18 MB "with skin"
exports never need to be kept.

    python3 Tools/Mocap/Combat/combat_sample.py <folder with the FBX files> [<folder> ...]

Each clip is named from its file (the upload's hash prefix dropped, lower case: "Sword_And_Shield_Slash_1"
becomes sword_and_shield_slash_1), posed at 30 frames a second over its whole length, and the world
positions of the bones the game reads are written, in Unity's axes, in metres, to
Tools/Mocap/Combat/samples/<clip>.json (the same layout as the locomotion samples).
"""
import json, os, re, sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(HERE))
sys.path.insert(0, os.path.join(os.path.dirname(HERE), 'Locomotion'))
import mixamo_rhythm as mr
from loco_sample import BONES, RATE


def clip_name(file_name):
    stem = os.path.splitext(file_name)[0]
    stem = re.sub(r'^[0-9a-f]{8}-', '', stem)
    return re.sub(r'[^a-z0-9]+', '_', stem.lower()).strip('_')


def main(folders):
    out = os.path.join(HERE, 'samples')
    os.makedirs(out, exist_ok=True)
    taken = {}
    for folder in folders:
        for f in sorted(os.listdir(folder)):
            if not f.lower().endswith('.fbx'):
                continue
            clip = clip_name(f)
            # (Two uploads of the same name: the second is <name>_b, and so on.)
            if clip in taken:
                taken[clip] += 1
                clip = f'{clip}_{chr(ord("a") + taken[clip])}'
            else:
                taken[clip] = 0
            sk = mr.Skeleton(os.path.join(folder, f))
            n = max(2, int(round(sk.length * RATE)))
            frames = []
            for i in range(n + 1):
                w = sk.pose(min(sk.length, i / RATE))
                frames.append([round(float(c), 4) for b in BONES for c in w[b][1]])
            with open(os.path.join(out, clip + '.json'), 'w') as fh:
                json.dump(dict(clip=clip, source=f, rate=RATE, length=round(sk.length, 4), bones=BONES, frames=frames), fh, separators=(',', ':'))
            print(f'{clip}: {len(frames)} frames ({sk.length:.2f} s) from {f}')


if __name__ == '__main__':
    main(sys.argv[1:])
