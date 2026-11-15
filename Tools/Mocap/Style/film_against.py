#!/usr/bin/env python3
"""A made motion set beside the recording it is made of (the plan "Movement Styles as Offshoots of the Standard Set":
the check that was missing): what the fighter looks like in a headless fight against what the recording looks like
at the same moment, as numbers and as a picture to look at.

A fighter standing in a ginga made of its takes writes, with each frame of the fight's film, which take he is in and
the moment of it (FIGHT_FILM; Tools/Mocap/Rhythm/film.py says how a film is written). This reads the film and the
clip library and, for every such frame, sets the fighter's body (each part against his hips, in the frame he faces)
beside the take's at that moment:

    python3 Tools/Mocap/Style/film_against.py film.json [--from 2 --to 20] [--strip out.png --moments 10]

It prints, part by part, how far the fighter is from the recording (cm: the usual and the worst, and when the worst
is; what is off the whole time is told apart as his build, which is not the recorded man's), with the legs apart
from the rest (the recording's feet are first made as much smaller as the ginga is, which the film says;
the trunk, the head and the arms are not made smaller), and how the trunk leans and how high the hips are in each. With --strip it draws both, the
recording above and the fighter below, from the front, from his right side and from above, at even moments.
"""
import json, math, os, sys
import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, '..', '..', '..'))
PARTS = "Pelvis Spine Chest Head UpperArmL ForearmL HandL UpperArmR ForearmR HandR ThighL ShinL FootL ThighR ShinR FootR HeadTop HandLEnd HandREnd ToeL ToeR".split()
IX = {n: i for i, n in enumerate(PARTS)}
# A fighter's part and the recording's bone that is the same joint.
SAME = [('Head', 'Head'), ('UpperArmL', 'LeftArm'), ('ForearmL', 'LeftForeArm'), ('HandL', 'LeftHand'), ('UpperArmR', 'RightArm'), ('ForearmR', 'RightForeArm'), ('HandR', 'RightHand'),
        ('ThighL', 'LeftUpLeg'), ('ShinL', 'LeftLeg'), ('FootL', 'LeftFoot'), ('ThighR', 'RightUpLeg'), ('ShinR', 'RightLeg'), ('FootR', 'RightFoot')]
GROUPS = [('head', ['Head']), ('shoulders', ['UpperArmL', 'UpperArmR']), ('elbows', ['ForearmL', 'ForearmR']), ('hands', ['HandL', 'HandR']),
          ('knees', ['ShinL', 'ShinR']), ('feet', ['FootL', 'FootR'])]
RIG = [('Pelvis', 'Head'), ('Head', 'HeadTop'), ('UpperArmL', 'UpperArmR'), ('UpperArmL', 'ForearmL'), ('ForearmL', 'HandL'), ('UpperArmR', 'ForearmR'), ('ForearmR', 'HandR'),
       ('ThighL', 'ThighR'), ('ThighL', 'ShinL'), ('ShinL', 'FootL'), ('FootL', 'ToeL'), ('ThighR', 'ShinR'), ('ShinR', 'FootR'), ('FootR', 'ToeR')]
REC = [('Hips', 'Head'), ('Head', 'HeadTop_End'), ('LeftArm', 'RightArm'), ('LeftArm', 'LeftForeArm'), ('LeftForeArm', 'LeftHand'), ('RightArm', 'RightForeArm'), ('RightForeArm', 'RightHand'),
       ('LeftUpLeg', 'RightUpLeg'), ('LeftUpLeg', 'LeftLeg'), ('LeftLeg', 'LeftFoot'), ('LeftFoot', 'LeftToeBase'), ('RightUpLeg', 'RightLeg'), ('RightLeg', 'RightFoot'), ('RightFoot', 'RightToeBase')]
LEFT, RIGHT, MID = (60, 120, 220), (215, 80, 60), (70, 70, 70)


def clips():
    out, bones = {}, None
    for path in (os.path.join(ROOT, 'Assets', '_Battler', 'Resources', 'Locomotion', 'CombatClips.json'), os.path.join(HERE, 'RulerClips.json')):
        if os.path.exists(path):
            d = json.load(open(path))
            bones = d['bones']
            for c in d['clips']:
                out[c['name']] = c
    return out, {b: i for i, b in enumerate(bones)}


def recorded(clip, time, n):
    """The recording's body at a time of it: points against its root (metres), the loop gone round."""
    P = np.array(clip['points'], float).reshape(clip['frames'], n, 3) * clip['legLength']
    frames = clip['frames'] - 1
    f = (time * clip['rate']) % frames
    a = int(math.floor(f))
    b = (a + 1) % frames
    return P[a] + (P[b] - P[a]) * (f - a)


def main():
    args = sys.argv[1:]
    path = args[0]
    def opt(name, otherwise):
        return type(otherwise)(args[args.index(name) + 1]) if name in args else otherwise
    a, b, strip, moments = opt('--from', 0.0), opt('--to', 1e9), opt('--strip', ''), opt('--moments', 10)
    lib, bone = clips()
    frames = json.load(open(path))
    who = frames[0]['u']
    mine = [f for f in frames if f['u'] == who and a <= f['t'] <= b and f.get('ref') in lib]
    if not mine:
        print(f'{path}: no frames that name a take (the fighter was not standing in a ginga made of its takes)')
        return 1
    rig_leg, pairs = 0.845, []
    for f in mine:
        p = np.array(f['p'], float).reshape(len(PARTS), 3)
        yaw = math.radians(f['root'][2])
        c, s = math.cos(yaw), math.sin(yaw)
        # (Against his hips, in the frame he faces: x to his right, z ahead; made the recording's size.)
        rel = p - p[IX['Pelvis']] * np.array([1, 0, 1])
        own = np.stack([rel[:, 0] * c - rel[:, 2] * s, rel[:, 1], rel[:, 0] * s + rel[:, 2] * c], axis=1) * (0.889 / rig_leg)
        r = recorded(lib[f['ref']], f['reft'], len(bone))
        r = r - r[bone['Hips']] * np.array([1, 0, 1])
        # (A ginga made smaller than its takes has its feet that much nearer under the hips, and the knees go with them.)
        size = f.get('size', 1.0)
        for side in ('Left', 'Right'):
            r[bone[side + 'Foot']] *= np.array([size, 1, size])
            r[bone[side + 'ToeBase']] *= np.array([size, 1, size])
            r[bone[side + 'Leg']] = r[bone[side + 'UpLeg']] + (r[bone[side + 'Leg']] - r[bone[side + 'UpLeg']]) * np.array([0.5 + 0.5 * size, 1, 0.5 + 0.5 * size])
        pairs.append((f, own, r))
    print(f"{os.path.basename(path)} ({who}): {len(pairs)} frames from {pairs[0][0]['t']:.1f} to {pairs[-1][0]['t']:.1f} s, in the takes {sorted(set(f['ref'] for f, _, _ in pairs))}")
    for name, parts in GROUPS:
        off = np.array([[own[IX[p]] - r[bone[dict(SAME)[p]]] for p in parts] for _, own, r in pairs])
        # (What is off the whole time is his build, which is not the recorded man's; what is left is the motion.)
        build = off.mean(axis=0)
        d = np.linalg.norm(off - build, axis=2)
        worst = np.unravel_index(d.argmax(), d.shape)
        print(f"  {name:9s} {100 * np.sqrt((d ** 2).mean()):5.1f} cm off the recording's motion (worst {100 * d.max():5.1f} at {pairs[worst[0]][0]['t']:.2f} s, {parts[worst[1]]}, in {pairs[worst[0]][0]['ref']} at {pairs[worst[0]][0]['reft']:.2f} s); his build puts them {100 * np.linalg.norm(build, axis=1).mean():4.1f} cm off throughout")
    def lean(up):
        return math.degrees(math.atan2(math.hypot(up[0], up[2]), up[1]))
    mine_lean = [lean(own[IX['Head']] - own[IX['Pelvis']]) for _, own, _ in pairs]
    rec_lean = [lean(r[bone['Head']] - r[bone['Hips']]) for _, _, r in pairs]
    print(f"  hips to head leans {np.mean(mine_lean):.0f}° ({min(mine_lean):.0f} to {max(mine_lean):.0f}); the recording's {np.mean(rec_lean):.0f}° ({min(rec_lean):.0f} to {max(rec_lean):.0f})")
    mh = [own[IX['Pelvis']][1] for _, own, _ in pairs]; rh = [r[bone['Hips']][1] for _, _, r in pairs]
    print(f"  hips {np.mean(mh):.2f} m up ({min(mh):.2f} to {max(mh):.2f}); the recording's {np.mean(rh):.2f} ({min(rh):.2f} to {max(rh):.2f})")
    hand = lambda own, k: np.linalg.norm(own[IX[k]] - own[IX['Head']])
    if strip:
        from PIL import Image, ImageDraw
        take = [pairs[int(round(i * (len(pairs) - 1) / max(1, moments - 1)))] for i in range(moments)]
        W, H, k = 170, 150, 62.0
        img = Image.new('RGB', (W * moments, H * 6 + 24), (250, 250, 247))
        dr = ImageDraw.Draw(img)
        views = [('front', lambda q: (q[0], q[1])), ('his right side', lambda q: (q[2], q[1])), ('above', lambda q: (q[0], -q[2] + 0.9))]
        dr.text((4, 4), f"{os.path.basename(path)}: in each pair of rows the recording is above and the fighter below; from the front, from his right side (ahead to the right), from above (ahead up). Blue: his left.", fill=(40, 40, 40))
        for col, (f, own, r) in enumerate(take):
            for v, (_, proj) in enumerate(views):
                for row, (pts, chain, index) in enumerate(((r, REC, bone), (own, RIG, IX))):
                    x0, y0 = col * W + W / 2, 24 + (v * 2 + row) * H + H - 18
                    dr.line([(col * W, y0), (col * W + W, y0)], fill=(225, 225, 220))
                    for a_, b_ in chain:
                        pa, pb = proj(pts[index[a_]]), proj(pts[index[b_]])
                        colour = LEFT if 'L' in a_.replace('Leg', '') and ('Left' in a_ or a_.endswith('L')) or 'Left' in b_ or b_.endswith('L') else RIGHT if 'Right' in a_ or a_.endswith('R') or 'Right' in b_ or b_.endswith('R') else MID
                        if (a_, b_) in (('UpperArmL', 'UpperArmR'), ('LeftArm', 'RightArm'), ('ThighL', 'ThighR'), ('LeftUpLeg', 'RightUpLeg'), ('Pelvis', 'Head'), ('Hips', 'Head'), ('Head', 'HeadTop'), ('Head', 'HeadTop_End')):
                            colour = MID
                        dr.line([(x0 + k * pa[0], y0 - k * pa[1]), (x0 + k * pb[0], y0 - k * pb[1])], fill=colour, width=2)
            dr.text((col * W + 4, 24 + 2), f"{f['t']:.2f} s: {f['ref'].replace('cap_', '')} {f['reft']:.2f}{' rest' if f.get('resting') else ''}", fill=(90, 90, 90))
        for v, (name, _) in enumerate(views):
            dr.text((4, 24 + v * 2 * H + 14), name, fill=(120, 120, 120))
        img.save(strip)
        print(f'  drawn: {strip}')
    return 0


if __name__ == '__main__':
    sys.exit(main())
