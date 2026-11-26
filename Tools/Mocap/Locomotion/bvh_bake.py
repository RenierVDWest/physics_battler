#!/usr/bin/env python3
"""Bakes BVH footage (the 100STYLE set) into the project's clip format, for the matched walk.

    python3 -I Tools/Mocap/Locomotion/bvh_bake.py <100STYLE folder> <out.json> Style[:KIND,KIND...] [Style...] [--report]

The plan "Walking that feels like a real person", step 1. 100STYLE (Mason, Starke, Komura 2022; CC BY 4.0; credit
"The 100STYLE Dataset - Ian Mason") is one actor in an inertial suit at 60 frames a second: a folder a style, in each a
file a kind (FW, BW forward and backward walk; SW sidestep walk; FR, BR, SR the same at a run; ID idle; TR1.. the
changes between them), and Frame_Cuts.csv with the frames of each file that are good.

What is written is what LocoClips.json holds, a clip a file, not looped, 30 frames a second: every library bone's place
against the root (x right, y up, z forward, in leg lengths), the root's track on the ground and its facing (degrees, a
turn to the right), and the feet's contacts (heel 1, toe 2, down 4).

- The skeleton has 23 joints; the library's 33 points are its joints, with the eyes, the knuckles and nothing else made
  up from the head's and the hands' own frames.
- BVH is right-handed (his left is +x); the game is left-handed (his right is +x): x is turned over.
- The root is his hips' place on the ground and the way his hips face, both smoothed (0.15 s and 0.2 s), so that the
  body's own sway and turn within a step stay in the pose and the root carries only where he goes.
- A foot is down when its ankle or toe is low and nearly still over the ground.
- In the walks and runs he circles the room: that slow, steady turning is taken out of the root's track.
- The suit's skeleton is not the library's (higher ankles, longer feet, the hips' point elsewhere, wider): the footage
  is rebuilt on the library's measures (`onto_library`), read from LocoClips.json beside the game.

The files it reads are downloaded data: run it with -I, and keep the data in a folder of its own.
"""
import csv, json, math, os, sys
import numpy as np

RATE_OUT = 30.0
BONES = ['Hips', 'Spine', 'Spine1', 'Spine2', 'Neck', 'Head', 'HeadTop_End', 'LeftEye', 'RightEye', 'LeftShoulder', 'LeftArm', 'LeftForeArm',
         'LeftHand', 'LeftHandIndex1', 'LeftHandMiddle1', 'LeftHandPinky1', 'RightShoulder', 'RightArm', 'RightForeArm', 'RightHand',
         'RightHandIndex1', 'RightHandMiddle1', 'RightHandPinky1', 'LeftUpLeg', 'LeftLeg', 'LeftFoot', 'LeftToeBase', 'LeftToe_End',
         'RightUpLeg', 'RightLeg', 'RightFoot', 'RightToeBase', 'RightToe_End']
JOINT = {'Hips': 'Hips', 'Spine': 'Chest', 'Spine1': 'Chest2', 'Spine2': 'Chest4', 'Neck': 'Neck', 'Head': 'Head', 'HeadTop_End': 'Head_end',
         'LeftShoulder': 'LeftCollar', 'LeftArm': 'LeftShoulder', 'LeftForeArm': 'LeftElbow', 'LeftHand': 'LeftWrist',
         'RightShoulder': 'RightCollar', 'RightArm': 'RightShoulder', 'RightForeArm': 'RightElbow', 'RightHand': 'RightWrist',
         'LeftUpLeg': 'LeftHip', 'LeftLeg': 'LeftKnee', 'LeftFoot': 'LeftAnkle', 'LeftToeBase': 'LeftToe', 'LeftToe_End': 'LeftToe_end',
         'RightUpLeg': 'RightHip', 'RightLeg': 'RightKnee', 'RightFoot': 'RightAnkle', 'RightToeBase': 'RightToe', 'RightToe_End': 'RightToe_end'}


def read_bvh(path):
    names, parents, offsets, chans, stack = [], [], [], [], []
    with open(path) as f:
        lines = f.read().split('\n')
    i = 0
    while not lines[i].startswith('MOTION'):
        w = lines[i].split()
        if w and w[0] in ('ROOT', 'JOINT'):
            names.append(w[1]); parents.append(stack[-1] if stack else -1); stack.append(len(names) - 1)
        elif w and w[0] == 'End':
            names.append(names[stack[-1]] + '_end'); parents.append(stack[-1]); stack.append(len(names) - 1); chans.append([])
        elif w and w[0] == 'OFFSET':
            offsets.append([float(x) for x in w[1:4]])
        elif w and w[0] == 'CHANNELS':
            chans.append(w[2:])
        elif w and w[0] == '}':
            stack.pop()
        i += 1
    frames = int(lines[i + 1].split()[1]); dt = float(lines[i + 2].split()[2])
    data = np.array([np.array(l.split(), float) for l in lines[i + 3:i + 3 + frames] if l.strip()])
    return names, parents, np.array(offsets), chans, data, dt


def rot(axis, deg):
    r = np.radians(deg); c, s = np.cos(r), np.sin(r); z, o = np.zeros_like(r), np.ones_like(r)
    if axis == 'X':
        m = [[o, z, z], [z, c, -s], [z, s, c]]
    elif axis == 'Y':
        m = [[c, z, s], [z, o, z], [-s, z, c]]
    else:
        m = [[c, -s, z], [s, c, z], [z, z, o]]
    return np.stack([np.stack(row, axis=-1) for row in m], axis=-2)


def world(names, parents, offsets, chans, data):
    """Every joint's place and turn in the file's own axes (cm), frame by frame."""
    n = data.shape[0]; P = np.zeros((n, len(names), 3)); R = np.zeros((n, len(names), 3, 3)); col = 0
    for j, name in enumerate(names):
        local = np.tile(np.eye(3), (n, 1, 1)); pos = np.tile(offsets[j], (n, 1))
        for ch in chans[j]:
            v = data[:, col]; col += 1
            if ch.endswith('position'):
                pos[:, 'XYZ'.index(ch[0])] = v
            else:
                local = local @ rot(ch[0], v)
        if parents[j] < 0:
            P[:, j] = pos; R[:, j] = local
        else:
            p = parents[j]
            P[:, j] = P[:, p] + np.einsum('nij,nj->ni', R[:, p], pos if not chans[j] or not any(c.endswith('position') for c in chans[j]) else pos)
            R[:, j] = R[:, p] @ local
    return P, R


def smooth(x, sigma_frames):
    r = int(math.ceil(4 * sigma_frames)); k = np.exp(-0.5 * (np.arange(-r, r + 1) / sigma_frames) ** 2); k /= k.sum()
    x = np.asarray(x, float); one = x.ndim == 1
    if one:
        x = x[:, None]
    pad = np.concatenate([np.repeat(x[:1], r, 0), x, np.repeat(x[-1:], r, 0)])
    out = np.stack([np.convolve(pad[:, j], k, mode='valid') for j in range(x.shape[1])], axis=1)
    return out[:, 0] if one else out


# ---- Onto the library's body ----------------------------------------------------------------------------------------
# The game puts every recording together on one set of measures (BodyMeasures.OfClips: the library's own skeleton). The
# suit's skeleton is another: its ankle joint stands 14% of a leg over the floor where the library's stands 10%, its foot
# is half as long again, its hips' point is level with the thighs' tops where the library's is 8% of a leg above them, and
# its hips and shoulders are wider. Baked as it is, he stood with bent knees on feet 3 cm in the air. So the footage is
# rebuilt on the library's measures: what every bone *does* (the way it points) is the suit's, how long it is and where
# it sits is the library's, and the feet meet the floor as the suit's did.

def unit(v):
    return v / np.maximum(1e-9, np.linalg.norm(v, axis=-1, keepdims=True))


def frame(up, right):
    """Axes (columns: right, up, forward) with up along `up` and right toward `right`."""
    u = unit(up); f = unit(np.cross(right, u)); r = np.cross(u, f)
    return np.stack([r, u, f], axis=-1)


def library_body(path):
    """The library's own measures, from its standing and walking clips (leg lengths)."""
    d = json.load(open(path)); B = d['bones']; ix = B.index
    clip = {c['name']: c for c in d['clips']}
    def pts_of(name):
        c = clip[name]; return np.array(c['points']).reshape(c['frames'], -1, 3)
    idle, walk = pts_of('idle'), pts_of('walk_fwd')
    q = lambda P, b: P[:, ix(b)]
    L = lambda a, b: float(np.median(np.linalg.norm(q(idle, a) - q(idle, b), axis=1)))
    body = dict(length={})
    for a, b in CHAIN:
        body['length'][(a, b)] = L(a, b)
    mid = 0.5 * (q(idle, 'LeftUpLeg') + q(idle, 'RightUpLeg'))
    pel = frame(q(idle, 'Spine1') - q(idle, 'Hips'), q(idle, 'RightUpLeg') - q(idle, 'LeftUpLeg'))
    body['hips_over_thighs'] = np.median(np.einsum('nji,nj->ni', pel, q(idle, 'Hips') - mid), axis=0)
    body['hip_width'] = L('LeftUpLeg', 'RightUpLeg')
    body['thigh'], body['shin'] = L('LeftUpLeg', 'LeftLeg'), L('LeftLeg', 'LeftFoot')
    chest = frame(q(idle, 'Neck') - q(idle, 'Spine2'), q(idle, 'RightArm') - q(idle, 'LeftArm'))
    for b in ('LeftShoulder', 'RightShoulder', 'LeftArm', 'RightArm'):
        body[b] = np.median(np.einsum('nji,nj->ni', chest, q(idle, b) - q(idle, 'Spine2')), axis=0)
    head = frame(q(idle, 'HeadTop_End') - q(idle, 'Head'), q(idle, 'RightEye') - q(idle, 'LeftEye'))
    for b in ('LeftEye', 'RightEye'):
        body[b] = np.median(np.einsum('nji,nj->ni', head, q(idle, b) - q(idle, 'Head')), axis=0)
    # The foot, in its own upright plane (forward along the foot, up), from the walk: where the ball and the toes' end
    # are against the ankle with the foot flat, and a heel under and a little behind the ankle.
    flat = q(walk, 'LeftFoot')[:, 1] < np.percentile(q(walk, 'LeftFoot')[:, 1], 10) + 0.004
    def in_plane(b):
        v = (q(walk, b) - q(walk, 'LeftFoot'))[flat]
        return np.array([float(np.median(np.linalg.norm(v[:, [0, 2]], axis=1))), float(np.median(v[:, 1]))])
    body['ankle'] = float(np.median(q(walk, 'LeftFoot')[flat, 1]))
    body['ball'], body['toes'] = in_plane('LeftToeBase'), in_plane('LeftToe_End')
    body['heel'] = np.array([-0.04, -body['ankle']])
    return body


STANDS_SHORT = 0.012
HEEL_STILL, TOE_STILL = 0.2, 0.3       # m/s over the ground
UNROLL_KINDS = ('FW', 'BW', 'SW', 'FR', 'BR', 'SR')   # the walks and runs: the takes he circled the room in
UNROLL_MOST, UNROLL_OVER = 40.0, 1.0   # degrees a second at most; the turning's average over about this many seconds is what is taken out
HEEL_UP = 5.0                          # degrees the foot is pitched down from flat when its heel no longer holds
CHAIN = [('Hips', 'Spine'), ('Spine', 'Spine1'), ('Spine1', 'Spine2'), ('Spine2', 'Neck'), ('Neck', 'Head'), ('Head', 'HeadTop_End')] + \
        [(s + a, s + b) for s in ('Left', 'Right') for a, b in (('Arm', 'ForeArm'), ('ForeArm', 'Hand'), ('Hand', 'HandMiddle1'), ('Hand', 'HandIndex1'), ('Hand', 'HandPinky1'))]


def onto_library(pts, body, feet=None, side=None):
    """The suit's points (leg lengths, the game's axes, the floor at 0) rebuilt on the library's measures."""
    ix = BONES.index; q = lambda b: pts[:, ix(b)]
    out = np.zeros_like(pts); n = pts.shape[0]
    up = np.array([0.0, 1.0, 0.0])
    notes = {}
    # The feet: each a stiff foot of the library's shape, pitched as the suit's foot is from flat, as far over the
    # floor as the suit's was (the lower of its heel and the ball of the foot), its ankle where the suit's ankle is.
    # (How the suit's foot stands when flat is one thing for the whole set: taken from the standing take and handed on.)
    if feet is None:
        ankle0 = float(np.median(np.concatenate([q('LeftFoot')[:, 1], q('RightFoot')[:, 1]])))
        ball0 = float(np.median(np.concatenate([q('LeftToeBase')[:, 1], q('RightToeBase')[:, 1]])))
        v = q('LeftToeBase') - q('LeftFoot')
        feet = dict(ankle=ankle0, ball=ball0, reach=float(np.median(np.linalg.norm(v[:, [0, 2]], axis=1))), pitch=float(np.median(np.arctan2(-v[:, 1], np.linalg.norm(v[:, [0, 2]], axis=1)))))
    ankle0, ball0, pitch0 = feet['ankle'], feet['ball'], feet['pitch']
    # The thighs' tops come down by what the suit's ankle stands too high, and by a little more: the suit stands on
    # legs locked straight, the library's stand has them 1.2% short of it, which is what leaves a leg room to reach.
    drop = ankle0 - body['ankle'] + STANDS_SHORT * (body['thigh'] + body['shin'])
    turn = lambda v, a: np.stack([v[0] * np.cos(a) + v[1] * np.sin(a), -v[0] * np.sin(a) + v[1] * np.cos(a)], axis=-1)   # pitched down by a
    their_hips = float(np.median(np.linalg.norm(q('RightUpLeg') - q('LeftUpLeg'), axis=1)))
    if side is None:
        side = unit((q('RightUpLeg') - q('LeftUpLeg')) * np.array([1.0, 0.0, 1.0]))
    for s in ('Left', 'Right'):
        A, T, E = q(s + 'Foot'), q(s + 'ToeBase'), q(s + 'Toe_End')
        v = T - A; flat_len = np.linalg.norm(v[:, [0, 2]], axis=1)
        pitch = np.arctan2(-v[:, 1], flat_len)
        d = pitch - pitch0
        heading = (E - A); heading[:, 1] = 0; heading = unit(heading)
        their_heel = A[:, 1] + turn(np.array([-0.05, -ankle0]), d)[:, 1]
        clear = np.maximum(0.0, np.minimum(T[:, 1] - ball0, their_heel))
        # (The game's foot is one stiff piece from heel to toes' end: with the heel up it stands on the end of its toes,
        # which is also about how far the suit's long foot lifts its ankle. So the sole is the line from the heel to
        # under the toes' end, and the lower of its two ends is what is `clear` over the floor.)
        ball, toes, heel = turn(body['ball'], d), turn(body['toes'], d), turn(body['heel'], d)
        tip = turn(np.array([body['toes'][0], -body['ankle']]), d)
        # (Its ankle where the suit's is, and in by what the library's hips are narrower, along the root's own side:
        # the leg keeps the way it points, and a planted foot is not swung by the hips' turning within a step.)
        # With the heel up it is the toes that stand still, and the suit's long foot carries its ankle further over them
        # than ours goes over its own: from flat on, the ankle is set from the suit's toe joint, so that the end of our
        # toes stays where it stands (held at the suit's ankle, our toes slid 3 to 6 cm forward in every push-off).
        over = np.clip(d / math.radians(3.0), 0.0, 1.0)[:, None]
        on_toes = T + heading * (body['toes'][0] - feet['reach'] - tip[:, :1])
        ankle = A * (1.0 - over) + on_toes * over
        ankle = ankle + side * ((-0.5 if s == 'Left' else 0.5) * (body['hip_width'] - their_hips))
        ankle[:, 1] = clear - np.minimum(tip[:, 1], heel[:, 1])
        out[:, ix(s + 'Foot')] = ankle
        notes[s] = (d, clear)
        out[:, ix(s + 'ToeBase')] = ankle + heading * ball[:, :1] + up * ball[:, 1:]
        out[:, ix(s + 'Toe_End')] = ankle + heading * toes[:, :1] + up * toes[:, 1:]
    # The thighs' tops: the suit's, lower by what its ankle stood too high, as far apart as the library's.
    mid = 0.5 * (q('LeftUpLeg') + q('RightUpLeg')) - up * drop
    across = unit(q('RightUpLeg') - q('LeftUpLeg'))
    out[:, ix('LeftUpLeg')] = mid - 0.5 * body['hip_width'] * across
    out[:, ix('RightUpLeg')] = mid + 0.5 * body['hip_width'] * across
    # The knees: thigh and shin of the library's lengths between the thigh's top and the ankle, bent the way the suit's knee is.
    longest = 0.998 * (body['thigh'] + body['shin']); pulled = 0
    for s in ('Left', 'Right'):
        hip, ankle = out[:, ix(s + 'UpLeg')], out[:, ix(s + 'Foot')]
        to = ankle - hip; dist = np.linalg.norm(to, axis=1)
        over = dist > longest; pulled += int(over.sum())
        if over.any():
            # (A leg that would be longer than it is: the foot comes to it, with what is rigidly on the ankle.)
            shift = (unit(to) * (longest - dist)[:, None]) * over[:, None]
            for b in ('Foot', 'ToeBase', 'Toe_End'):
                out[:, ix(s + b)] += shift
            ankle = out[:, ix(s + 'Foot')]; to = ankle - hip; dist = np.linalg.norm(to, axis=1)
        axis = unit(to)
        a = (body['thigh'] ** 2 - body['shin'] ** 2 + dist ** 2) / (2 * dist)
        h = np.sqrt(np.maximum(0.0, body['thigh'] ** 2 - a ** 2))
        k = q(s + 'Leg') - q(s + 'UpLeg'); their = unit(q(s + 'Foot') - q(s + 'UpLeg'))
        pole = k - their * np.sum(k * their, axis=1, keepdims=True)
        pole = pole - axis * np.sum(pole * axis, axis=1, keepdims=True)
        weak = np.linalg.norm(pole, axis=1) < 0.02                      # (a straight leg says little: its knee goes the way the foot points)
        fwd = out[:, ix(s + 'Toe_End')] - ankle; fwd = fwd - axis * np.sum(fwd * axis, axis=1, keepdims=True)
        pole = np.where(weak[:, None], fwd, pole)
        out[:, ix(s + 'Leg')] = hip + axis * a[:, None] + unit(pole) * h[:, None]
    notes['legs pulled in'] = pulled / (2.0 * n)
    # The hips' point: over the thighs' tops as the library's is, in the pelvis's own frame.
    pel = frame(q('Spine1') - q('Hips'), q('RightUpLeg') - q('LeftUpLeg'))
    out[:, ix('Hips')] = mid + np.einsum('nij,j->ni', pel, body['hips_over_thighs'])
    # The back, the neck and the head: each piece the library's length, pointing as the suit's does.
    grow = lambda a, b: out[:, ix(a)] + body['length'][(a, b)] * unit(q(b) - q(a))
    for a, b in CHAIN[:6]:
        out[:, ix(b)] = grow(a, b)
    # The shoulders: where the library has them on the chest, moved as the suit's move on its own chest (a shrug, a
    # reach), in proportion to how wide each body is there.
    chest = frame(q('Neck') - q('Spine2'), q('RightArm') - q('LeftArm'))
    wide = body['RightArm'][0] - body['LeftArm'][0]
    their_wide = float(np.median(np.linalg.norm(q('RightArm') - q('LeftArm'), axis=1)))
    for b in ('LeftShoulder', 'RightShoulder', 'LeftArm', 'RightArm'):
        own = np.einsum('nji,nj->ni', chest, q(b) - q('Spine2'))
        moved = (own - np.median(own, axis=0)) * (wide / their_wide)
        out[:, ix(b)] = out[:, ix('Spine2')] + np.einsum('nij,nj->ni', chest, body[b] + moved)
    for a, b in CHAIN[6:]:
        out[:, ix(b)] = grow(a, b)
    head = frame(q('HeadTop_End') - q('Head'), q('RightEye') - q('LeftEye'))
    for b in ('LeftEye', 'RightEye'):
        out[:, ix(b)] = out[:, ix('Head')] + np.einsum('nij,j->ni', head, body[b])
    notes['hips down by'] = drop
    notes['feet'] = feet
    return out, notes


def bake(path, name, first, last, about, body, feet=None, unroll=False):
    names, parents, offsets, chans, data, dt = read_bvh(path)
    data = data[first:last + 1]
    P, R = world(names, parents, offsets, chans, data)
    ix = {n: i for i, n in enumerate(names)}
    leg = (np.linalg.norm(offsets[ix['LeftKnee']]) + np.linalg.norm(offsets[ix['LeftAnkle']])) / 100.0
    P = P / 100.0
    n = P.shape[0]; hz = 1.0 / dt
    pts = np.zeros((n, len(BONES), 3))
    for b, j in JOINT.items():
        pts[:, BONES.index(b)] = P[:, ix[j]]
    # The eyes and the knuckles, from the head's and the hands' own frames (the file's axes: his left is +x, forward +z).
    head = R[:, ix['Head']]
    for b, side in (('LeftEye', 1.0), ('RightEye', -1.0)):
        pts[:, BONES.index(b)] = P[:, ix['Head']] + np.einsum('nij,j->ni', head, np.array([side * 0.032, 0.09, 0.09]))
    for s in ('Left', 'Right'):
        wrist, tip, hand = P[:, ix[s + 'Wrist']], P[:, ix[s + 'Wrist_end']], R[:, ix[s + 'Wrist']]
        mid = wrist + 0.5 * (tip - wrist)
        pts[:, BONES.index(s + 'HandMiddle1')] = mid
        pts[:, BONES.index(s + 'HandIndex1')] = mid + np.einsum('nij,j->ni', hand, np.array([0, 0, 0.025]))
        pts[:, BONES.index(s + 'HandPinky1')] = mid + np.einsum('nij,j->ni', hand, np.array([0, 0, -0.025]))
    pts[:, :, 0] *= -1.0                                       # into the game's handedness: his right is +x
    floor = np.percentile(np.minimum(pts[:, BONES.index('LeftToe_End'), 1], pts[:, BONES.index('RightToe_End'), 1]), 1.0)
    pts[:, :, 1] -= floor
    # The root: the hips over the ground and the way they face, smoothed.
    hips = pts[:, 0]; r = pts[:, BONES.index('RightUpLeg')] - pts[:, BONES.index('LeftUpLeg')]
    yaw = np.unwrap(np.arctan2(-r[:, 2], r[:, 0]))             # forward = right x up = (-r.z, 0, r.x); a turn to the right from +z
    yaw = smooth(yaw, 0.2 * hz)
    place = smooth(hips[:, [0, 2]], 0.15 * hz)
    fwd = np.stack([np.sin(yaw), np.cos(yaw)], axis=1); right = np.stack([np.cos(yaw), -np.sin(yaw)], axis=1)
    toes = (pts[:, BONES.index('LeftToe_End')] - pts[:, BONES.index('LeftFoot')])[:, [0, 2]]
    toes_ahead = float(np.mean(np.sum(toes * fwd, axis=1)))
    suit = pts
    pts, notes = onto_library(suit / leg, body, feet, np.stack([right[:, 0], np.zeros(n), right[:, 1]], axis=1))
    pts = pts * leg
    d = pts[:, :, [0, 2]] - place[:, None, :]
    local = np.stack([np.einsum('nbj,nj->nb', d, right), pts[:, :, 1], np.einsum('nbj,nj->nb', d, fwd)], axis=2)
    # Contacts: an ankle or a toe that is low and nearly still.
    contacts = np.zeros((n, 2), int)
    for f, s in enumerate(('Left', 'Right')):
        A = smooth(suit[:, BONES.index(s + 'Foot')], 0.02 * hz); T = smooth(suit[:, BONES.index(s + 'ToeBase')], 0.02 * hz)
        va = np.linalg.norm(np.gradient(A, dt, axis=0)[:, [0, 2]], axis=1); vt = np.linalg.norm(np.gradient(T, dt, axis=0)[:, [0, 2]], axis=1)
        # (The heel holds only while the foot is flat and the ankle still: with the heel coming up the ankle rolls on
        # over the toes, by 8 cm before it is 3 cm higher, and a foot held at its ankle through that is dragged back.)
        pitched, clear = notes[s]
        heel = (clear * leg < 0.015) & (pitched < math.radians(HEEL_UP)) & (va < HEEL_STILL)
        toe = (T[:, 1] < np.percentile(T[:, 1], 5) + 0.025) & (vt < TOE_STILL)
        contacts[:, f] = heel * 1 + toe * 2 + (heel | toe) * 4
    step = int(round(hz / RATE_OUT))
    sel = np.arange(0, n, step)
    m = len(sel); duration = (m - 1) / RATE_OUT
    # The room was 4.5 m across, so he walked his walks and runs round in a circle: about 28 degrees a second, with
    # hardly a second of going straight in two minutes. Asked to walk straight, the search then passed the plain walk
    # over for the stop-and-go style's few straight steps. So the slow, steady part of his turning is taken out of the
    # root's track (never more than UNROLL_MOST a second; a real turn is quicker and stays): he walks straight as
    # recorded, and a curve asked for is turned back in as he goes. The pose against the root is as it was.
    circled = 0.0
    if unroll:
        rate = np.gradient(yaw, dt)
        slow = np.clip(smooth(rate, UNROLL_OVER * hz), -math.radians(UNROLL_MOST), math.radians(UNROLL_MOST))
        circled = float(np.degrees(np.median(np.abs(slow))))
        vel = np.gradient(place, dt, axis=0)
        own = np.stack([np.sum(vel * right, axis=1), np.sum(vel * fwd, axis=1)], axis=1)
        yaw = yaw[0] + np.concatenate([[0.0], np.cumsum(0.5 * ((rate - slow)[1:] + (rate - slow)[:-1]) * dt)])
        fwd = np.stack([np.sin(yaw), np.cos(yaw)], axis=1); right = np.stack([np.cos(yaw), -np.sin(yaw)], axis=1)
        vel = right * own[:, :1] + fwd * own[:, 1:]
        place = place[0] + np.concatenate([[[0.0, 0.0]], np.cumsum(0.5 * (vel[1:] + vel[:-1]) * dt, axis=0)])
    speed = np.linalg.norm(np.gradient(place, dt, axis=0), axis=1)
    vel_local = np.stack([np.sum(np.gradient(place, dt, axis=0) * right, axis=1), np.sum(np.gradient(place, dt, axis=0) * fwd, axis=1)], axis=1)
    turn_rate = np.degrees(np.gradient(yaw, dt))
    moving = speed > 0.25
    way = np.degrees(np.arctan2(vel_local[moving, 0], vel_local[moving, 1])) if moving.any() else np.array([0.0])
    shares = {k: float(np.mean((np.abs(((way - c + 180) % 360) - 180) < 45))) for k, c in (('forward', 0), ('to his right', 90), ('backward', 180), ('to his left', -90))}
    stats = dict(name=name, seconds=duration, leg=leg, toes_ahead=toes_ahead, standing=float(np.mean(speed < 0.15)),
                 speed_moving=float(np.median(speed[moving])) if moving.any() else 0.0, speed_top=float(np.percentile(speed, 95)),
                 turn=float(np.percentile(np.abs(turn_rate), 90)), ways=shares,
                 both=float(np.mean((contacts[:, 0] > 0) & (contacts[:, 1] > 0))), none=float(np.mean((contacts[:, 0] == 0) & (contacts[:, 1] == 0))),
                 pulled=notes['legs pulled in'], circled=circled, down=notes['hips down by'] * leg, feet=notes['feet'],
                 hips=float(np.median(hips[:, 1])), starts=int(np.sum((speed[1:] >= 0.4) & (speed[:-1] < 0.4))))
    clip = dict(name=name, mirrorOf='', role='footage', loop=False, rate=RATE_OUT, frames=m, legLength=round(leg, 4), duration=round(duration, 4),
                speed=round(float(np.mean(speed)) / leg, 4), direction=0.0, turn=0.0, stride=0.0, markers=[],
                root=[round(float(v), 4) for i in sel for v in (place[i, 0] / leg, place[i, 1] / leg, math.degrees(yaw[i]))],
                points=[round(float(v), 3) for v in (local[sel] / leg).reshape(-1)],
                contacts=[int(v) for v in contacts[sel].reshape(-1)], phase=[], about=about, kind='footage', set='100style')
    return clip, stats


def main():
    args = [a for a in sys.argv[1:] if not a.startswith('--')]
    folder, out, wanted = args[0], args[1], args[2:]
    here = os.path.dirname(os.path.abspath(__file__))
    body = library_body(os.path.join(here, '..', '..', '..', 'Assets', '_Battler', 'Resources', 'Locomotion', 'LocoClips.json'))
    cuts = {r['STYLE_NAME']: r for r in csv.DictReader(open(os.path.join(folder, 'Frame_Cuts.csv')))}
    clips, lines = [], []
    # How the suit's foot stands when flat: from the plain style's standing take.
    feet = None
    stand = os.path.join(folder, 'Neutral', 'Neutral_ID.bvh')
    if os.path.exists(stand) and cuts['Neutral'].get('ID_START', 'N/A') != 'N/A':
        feet = bake(stand, 'stand', int(cuts['Neutral']['ID_START']), int(cuts['Neutral']['ID_STOP']), '', body)[1]['feet']
        print(f"the suit's foot when flat: ankle {feet['ankle']:.3f}, ball {feet['ball']:.3f} leg lengths over the floor, pitched {math.degrees(feet['pitch']):.1f} degrees")
    for w in wanted:
        style, _, kinds = w.partition(':')
        kinds = kinds.split(',') if kinds else ['FW', 'BW', 'SW', 'FR', 'BR', 'SR', 'ID', 'TR1', 'TR2', 'TR3']
        for kind in kinds:
            path = os.path.join(folder, style, f'{style}_{kind}.bvh')
            a, b = cuts[style].get(kind + '_START', 'N/A'), cuts[style].get(kind + '_STOP', 'N/A')
            if not os.path.exists(path) or a == 'N/A':
                continue
            name = f's100_{style.lower()}_{kind.lower()}'
            clip, st = bake(path, name, int(a), int(b), f'100STYLE {style} {kind} (frames {a} to {b}); CC BY 4.0, The 100STYLE Dataset - Ian Mason', body, feet, kind in UNROLL_KINDS)
            clips.append(clip)
            ways = ', '.join(f"{k} {v*100:.0f}%" for k, v in st['ways'].items() if v > 0.03)
            lines.append(f"{name:24s} {st['seconds']:6.1f} s; standing {st['standing']*100:3.0f}%; moving at {st['speed_moving']:.2f} m/s (top {st['speed_top']:.2f}); "
                         f"sets off {st['starts']:3d} times; turns up to {st['turn']:.0f} deg/s; goes {ways}; both feet down {st['both']*100:.0f}%, none {st['none']*100:.0f}%; "
                         f"hips {st['hips']*100:.0f} cm; leg {st['leg']*100:.1f} cm; toes ahead {st['toes_ahead']*100:.0f} cm; "
                         f"onto the library's body: hips down {st['down']*100:.1f} cm, a leg pulled in {st['pulled']*100:.1f}% of the time"
                         + (f"; his circling taken out: {st['circled']:.0f} deg/s" if st['circled'] > 0 else ''))
    json.dump(dict(about='Footage for the matched walk, baked by Tools/Mocap/Locomotion/bvh_bake.py from the 100STYLE dataset (Ian Mason, Sebastian Starke, Taku Komura; '
                         'CC BY 4.0: credit "The 100STYLE Dataset - Ian Mason"). Points in leg lengths against the root (x right, y up, z forward).',
                   bones=BONES, clips=clips), open(out, 'w'), separators=(',', ':'))
    print('\n'.join(lines))
    print(f"{len(clips)} clips, {sum(c['frames'] for c in clips)} frames ({sum(c['duration'] for c in clips)/60:.1f} minutes), {os.path.getsize(out)/1e6:.1f} MB -> {out}")


if __name__ == '__main__':
    main()
