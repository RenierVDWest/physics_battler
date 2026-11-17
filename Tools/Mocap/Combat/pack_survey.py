#!/usr/bin/env python3
"""A survey of animation packs for what their LEGS do (the plan "Convincing walking in the dance"): every FBX take in
every pack folder is sampled (as combat_sample.py does, into a cache folder outside the project) and its lower body
measured, so takes can be picked for the standard set by how near they are to the guard's own stance. Nothing here
is used as it is: a take picked is rebuilt (its legs under the guard's own chest and arms, on the guard's carriage).

    python3 Tools/Mocap/Combat/pack_survey.py sample <packs folder> <cache folder> [part k of n]
    python3 Tools/Mocap/Combat/pack_survey.py measure <cache folder> <out.csv>

<packs folder> holds one folder a pack. Measures are at a Medium's size (legs of 0.85 m), in the take's own frame
(it faces +z at its start).
"""
import csv, json, math, os, re, sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(HERE))
sys.path.insert(0, os.path.join(os.path.dirname(HERE), 'Locomotion'))


def clip_name(file_name):
    stem = os.path.splitext(file_name)[0]
    stem = re.sub(r'^[0-9a-f]{8}-', '', stem)
    return re.sub(r'[^a-z0-9]+', '_', stem.lower()).strip('_')


def sample(packs, cache, part=0, parts=1):
    import mixamo_rhythm as mr
    from loco_sample import BONES, RATE
    import numpy as np
    jobs = []
    for pack in sorted(os.listdir(packs)):
        folder = os.path.join(packs, pack)
        if not os.path.isdir(folder):
            continue
        for root, _, files in os.walk(folder):
            for f in sorted(files):
                if f.lower().endswith('.fbx'):
                    jobs.append((pack, os.path.join(root, f), f))
    for k, (pack, path, f) in enumerate(jobs):
        if k % parts != part:
            continue
        out = os.path.join(cache, pack)
        os.makedirs(out, exist_ok=True)
        dest = os.path.join(out, clip_name(f) + '.json')
        if os.path.exists(dest):
            continue
        try:
            sk = mr.Skeleton(path)
            if sk.length < 0.05:
                print(f'{pack}/{f}: no motion (a character or a pose)')
                json.dump(dict(clip=clip_name(f), source=f, skip='no motion'), open(dest, 'w'))
                continue
            n = max(2, int(round(sk.length * RATE)))
            frames = []
            for i in range(n + 1):
                w = sk.pose(min(sk.length, i / RATE))
                if 'LeftEye' not in w:
                    head, top = np.array(w['Head'][1]), np.array(w['HeadTop_End'][1])
                    right = np.array(w['RightArm'][1]) - np.array(w['LeftArm'][1])
                    right[1] = 0.0
                    right /= max(1e-6, np.linalg.norm(right))
                    fwd = np.cross(right, [0.0, 1.0, 0.0])
                    mid = head + 0.5 * (top - head) + 0.08 * fwd
                    w = dict(w, LeftEye=(None, mid - 0.03 * right), RightEye=(None, mid + 0.03 * right))
                frames.append([round(float(c), 4) for b in BONES for c in w[b][1]])
            json.dump(dict(clip=clip_name(f), source=f, rate=RATE, length=round(sk.length, 4), bones=BONES, frames=frames), open(dest, 'w'), separators=(',', ':'))
            print(f'{pack}/{clip_name(f)}: {len(frames)} frames ({sk.length:.2f} s)')
        except Exception as e:  # (a file the reader cannot take: said, and gone on from)
            print(f'{pack}/{f}: could not be read: {type(e).__name__}: {e}')
            json.dump(dict(clip=clip_name(f), source=f, skip=f'{type(e).__name__}: {e}'), open(dest, 'w'))


def measure_take(d):
    import numpy as np
    b = {x: i for i, x in enumerate(d['bones'])}
    f = np.array(d['frames']).reshape(len(d['frames']), -1, 3)
    n = len(f)
    rate = d['rate']
    leg = np.linalg.norm(f[0, b['LeftUpLeg']] - f[0, b['LeftLeg']]) + np.linalg.norm(f[0, b['LeftLeg']] - f[0, b['LeftFoot']])
    k = 0.85 / max(1e-6, leg)
    f = f * k
    hips = f[:, b['Hips']]
    ground = min(f[:, b['LeftToeBase'], 1].min(), f[:, b['RightToeBase'], 1].min())
    move = hips[-1] - hips[0]
    dist = float(np.hypot(move[0], move[2]))
    way = float(np.degrees(np.arctan2(move[0], move[2]))) if dist > 0.15 else 0.0
    # (The path's length: a take that goes out and comes back travels though it ends where it began.)
    step = np.diff(hips[:, [0, 2]], axis=0)
    path = float(np.linalg.norm(step, axis=1).sum())
    across = f[:, b['RightUpLeg']] - f[:, b['LeftUpLeg']]
    yaw = np.unwrap(np.arctan2(-across[:, 2], across[:, 0]))
    turn = float(np.degrees(yaw[-1] - yaw[0]))
    hips_yaw = float(np.degrees(np.mean(yaw)))
    sh = f[:, b['RightArm']] - f[:, b['LeftArm']]
    chest_yaw = float(np.degrees(np.arctan2(-sh[:, 2], sh[:, 0]).mean()))
    up = f[:, b['Spine2']] - hips
    lean = float(np.degrees(np.arctan2(np.hypot(up[:, 0], up[:, 2]), up[:, 1])).mean())
    l, r = f[:, b['LeftFoot']], f[:, b['RightFoot']]
    sep = np.hypot(l[:, 0] - r[:, 0], l[:, 2] - r[:, 2])
    # (Along and across the way it travels; a take that goes nowhere: along its facing.)
    wr = math.radians(way)
    along = np.array([math.sin(wr), 0.0, math.cos(wr)])
    side = np.array([math.cos(wr), 0.0, -math.sin(wr)])
    lead = (l - r) @ along
    wide = (l - r) @ side
    # (In the hips' own frame, each moment: where the left foot is from the right (ahead, to the right), and the way
    # it travels against where the hips face. A guard stands turned from the way it looks, so what its legs do is
    # read against its hips, not against the take's front.)
    hy = yaw
    fwd_x, fwd_z = np.sin(hy), np.cos(hy)
    rgt_x, rgt_z = np.cos(hy), -np.sin(hy)
    dlr = l - r
    feet_fwd = float((dlr[:, 0] * fwd_x + dlr[:, 2] * fwd_z).mean())
    feet_side = float((dlr[:, 0] * rgt_x + dlr[:, 2] * rgt_z).mean())
    rel = (way - hips_yaw + 180.0) % 360.0 - 180.0 if dist > 0.15 else 0.0
    # (Do the feet keep their order: along the way (one foot stays ahead: a guard's step) and across it (they never cross).)
    keeps_along = float(max((lead > 0.02).mean(), (lead < -0.02).mean()))
    keeps_across = float(max((wide > 0.02).mean(), (wide < -0.02).mean()))
    # (Footfalls: a foot is down while it is low and slow.)
    def falls(foot, toe):
        h = np.minimum(foot[:, 1], toe[:, 1] + 0.05) - ground
        v = np.linalg.norm(np.diff(foot[:, [0, 2]], axis=0), axis=1) * rate
        v = np.append(v, v[-1])
        down = (v < 0.35) & (h < 0.16)
        return int(((~down[:-1]) & down[1:]).sum()), float(down.mean())
    fl, dl = falls(l, f[:, b['LeftToeBase']])
    fr, dr = falls(r, f[:, b['RightToeBase']])
    # (Does it loop: the body against its hips at the end as at the start.)
    rel0, rel1 = f[0] - hips[0], f[-1] - hips[-1]
    loop = float(np.linalg.norm(rel1 - rel0, axis=1).max())
    # (How busy the upper body is: the hands' speed against the chest.)
    chest = f[:, b['Spine2']]
    hands = 0.5 * (np.linalg.norm(np.diff(f[:, b['LeftHand']] - chest, axis=0), axis=1) + np.linalg.norm(np.diff(f[:, b['RightHand']] - chest, axis=0), axis=1)) * rate
    length = d['length']
    return dict(length=round(length, 2), travel=round(dist, 2), path=round(path, 2), way=round(way), pace=round(path / max(1e-3, length), 2), turn=round(turn),
                hips=round(float((hips[:, 1] - ground).mean()), 3), hips_low=round(float((hips[:, 1] - ground).min()), 3), hips_high=round(float((hips[:, 1] - ground).max()), 3),
                feet_apart=round(float(sep.mean()), 2), lead=round(float(lead.mean()), 2), wide=round(float(np.abs(wide).mean()), 2),
                rel=round(rel), feet_fwd=round(feet_fwd, 2), feet_side=round(feet_side, 2), keeps_along=round(keeps_along, 2), keeps_across=round(keeps_across, 2), hips_yaw=round(hips_yaw), chest_yaw=round(chest_yaw), lean=round(lean),
                footfalls=fl + fr, down=round(0.5 * (dl + dr), 2), loop=round(loop, 2), hands=round(float(hands.mean()), 2), hands_most=round(float(hands.max()), 1))


def measure(cache, out):
    rows = []
    for pack in sorted(os.listdir(cache)):
        folder = os.path.join(cache, pack)
        if not os.path.isdir(folder):
            continue
        for name in sorted(os.listdir(folder)):
            if not name.endswith('.json'):
                continue
            d = json.load(open(os.path.join(folder, name)))
            if 'skip' in d:
                rows.append(dict(pack=pack, take=d['clip'], skip=d['skip']))
                continue
            try:
                rows.append(dict(pack=pack, take=d['clip'], skip='', **measure_take(d)))
            except Exception as e:
                rows.append(dict(pack=pack, take=d['clip'], skip=f'{type(e).__name__}: {e}'))
    keys = ['pack', 'take', 'skip', 'length', 'travel', 'path', 'way', 'pace', 'turn', 'hips', 'hips_low', 'hips_high', 'feet_apart', 'lead', 'wide', 'rel', 'feet_fwd', 'feet_side', 'keeps_along', 'keeps_across', 'hips_yaw', 'chest_yaw', 'lean', 'footfalls', 'down', 'loop', 'hands', 'hands_most']
    with open(out, 'w', newline='') as fh:
        w = csv.DictWriter(fh, fieldnames=keys)
        w.writeheader()
        for r in rows:
            w.writerow(r)
    print(f'{len(rows)} takes measured into {out}; {sum(1 for r in rows if r.get("skip"))} could not be')


if __name__ == '__main__':
    if len(sys.argv) >= 4 and sys.argv[1] == 'sample':
        part, parts = (int(sys.argv[4]), int(sys.argv[5])) if len(sys.argv) >= 6 else (0, 1)
        sample(sys.argv[2], sys.argv[3], part, parts)
    elif len(sys.argv) >= 4 and sys.argv[1] == 'measure':
        measure(sys.argv[2], sys.argv[3])
    else:
        print(__doc__)
