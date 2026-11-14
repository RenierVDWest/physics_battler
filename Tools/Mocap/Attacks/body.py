"""
A body taken apart into its parts' own turns and travels, and put together again (the plan "Aimed attacks").

decompose(clip) gives a clip as "tracks": for every frame, where the pelvis is and how it is turned, how the chest is
turned on the pelvis and the head on the chest, which way each bone of each arm points on the chest and how the hand
is turned, where each ankle is, which way the knee points and how the foot lies. compose(tracks) builds the 33 points
again from the tracks and the body's own measures (its bone lengths, where its joints sit on the pelvis and the chest).
A clip taken apart and put together comes out as it went in, to a few millimetres: so the tracks hold all of the motion,
and a motion can be made, or changed, by writing tracks.

Axes: x to the body's right, y up, z forward. Turns as Unity has them: yaw about y (+ to the right), then pitch about x
(+ tipped forward), then roll about z (+ the right side up); a part's turn is Ry(yaw) Rx(pitch) Rz(roll).
"""
import math
import numpy as np
from attack_lib import Clip, unit, unwrap_deg, STRIKES


def rx(a):
    c, s = np.cos(a), np.sin(a); z, o = np.zeros_like(a), np.ones_like(a)
    return np.stack([np.stack([o, z, z], -1), np.stack([z, c, -s], -1), np.stack([z, s, c], -1)], -2)


def ry(a):
    c, s = np.cos(a), np.sin(a); z, o = np.zeros_like(a), np.ones_like(a)
    return np.stack([np.stack([c, z, s], -1), np.stack([z, o, z], -1), np.stack([-s, z, c], -1)], -2)


def rz(a):
    c, s = np.cos(a), np.sin(a); z, o = np.zeros_like(a), np.ones_like(a)
    return np.stack([np.stack([c, -s, z], -1), np.stack([s, c, z], -1), np.stack([z, z, o], -1)], -2)


def turn(ypr):
    """The turn of yaw, pitch, roll (degrees; (n, 3) -> (n, 3, 3))."""
    a = np.radians(ypr)
    return ry(a[..., 0]) @ rx(a[..., 1]) @ rz(a[..., 2])


def ypr_of(R):
    """Yaw, pitch, roll (degrees) of a turn; the yaw carried on round, never jumping."""
    yaw = np.degrees(np.arctan2(R[..., 0, 2], R[..., 2, 2]))
    pitch = -np.degrees(np.arcsin(np.clip(R[..., 1, 2], -1, 1)))
    roll = np.degrees(np.arctan2(R[..., 1, 0], R[..., 1, 1]))
    return np.stack([unwrap_deg(yaw) if yaw.ndim else yaw, pitch, unwrap_deg(roll) if roll.ndim else roll], -1)


def axes(right, up):
    """A part's turn from its right and (about) its up: columns its right, up, forward."""
    x = unit(right)
    z = unit(np.cross(x, up))
    y = np.cross(z, x)
    return np.stack([x, y, z], -1)


def quat_of(R):
    """Turns as quaternions (x, y, z, w), each the same side as the one before it."""
    n = R.shape[0]
    q = np.zeros((n, 4))
    for i in range(n):
        m = R[i]; t = m[0, 0] + m[1, 1] + m[2, 2]
        if t > 0:
            s = math.sqrt(t + 1) * 2; q[i] = [(m[2, 1] - m[1, 2]) / s, (m[0, 2] - m[2, 0]) / s, (m[1, 0] - m[0, 1]) / s, 0.25 * s]
        elif m[0, 0] > m[1, 1] and m[0, 0] > m[2, 2]:
            s = math.sqrt(1 + m[0, 0] - m[1, 1] - m[2, 2]) * 2; q[i] = [0.25 * s, (m[0, 1] + m[1, 0]) / s, (m[0, 2] + m[2, 0]) / s, (m[2, 1] - m[1, 2]) / s]
        elif m[1, 1] > m[2, 2]:
            s = math.sqrt(1 + m[1, 1] - m[0, 0] - m[2, 2]) * 2; q[i] = [(m[0, 1] + m[1, 0]) / s, 0.25 * s, (m[1, 2] + m[2, 1]) / s, (m[0, 2] - m[2, 0]) / s]
        else:
            s = math.sqrt(1 + m[2, 2] - m[0, 0] - m[1, 1]) * 2; q[i] = [(m[0, 2] + m[2, 0]) / s, (m[1, 2] + m[2, 1]) / s, 0.25 * s, (m[1, 0] - m[0, 1]) / s]
        if i > 0 and np.dot(q[i], q[i - 1]) < 0:
            q[i] = -q[i]
    return q


def turn_of_quat(q):
    q = unit(q)
    x, y, z, w = q[..., 0], q[..., 1], q[..., 2], q[..., 3]
    return np.stack([np.stack([1 - 2 * (y * y + z * z), 2 * (x * y - z * w), 2 * (x * z + y * w)], -1),
                     np.stack([2 * (x * y + z * w), 1 - 2 * (x * x + z * z), 2 * (y * z - x * w)], -1),
                     np.stack([2 * (x * z - y * w), 2 * (y * z + x * w), 1 - 2 * (x * x + y * y)], -1)], -2)


def into(R, v):
    """A world way in a part's own axes."""
    return np.einsum('nji,nj->ni', R, v)


def out(R, v):
    """A way in a part's own axes, in the world."""
    return np.einsum('nij,nj->ni', R, v) if v.ndim == 2 else np.einsum('nij,j->ni', R, v)


SIDES = (('l', 'Left'), ('r', 'Right'))
SPINE_SHARES = (0.35, 0.7)        # how much of the chest's turn on the pelvis the two spine bones have taken


def frames_of(c):
    """The pelvis's, the chest's, the head's and the hands' turns from a clip's points."""
    p = c.p
    pel = axes(p('RightUpLeg') - p('LeftUpLeg'), p('Spine') - p('Hips'))
    che = axes(p('RightShoulder') - p('LeftShoulder'), p('Neck') - p('Spine2'))
    head = axes(p('RightEye') - p('LeftEye'), p('HeadTop_End') - p('Head'))
    hands = {}
    for s, S in SIDES:
        along = p(S + 'HandMiddle1') - p(S + 'Hand')
        across = p(S + 'HandIndex1') - p(S + 'HandPinky1')
        x = unit(across)
        z = unit(along - x * np.sum(along * x, axis=1, keepdims=True))
        hands[s] = np.stack([x, np.cross(z, x), z], -1)
    return pel, che, head, hands


def slerp_turn(A, B, share):
    """A turn part of the way from one to another (each frame)."""
    qa, qb = quat_of(A), quat_of(B)
    d = np.sum(qa * qb, axis=1, keepdims=True)
    qb = np.where(d < 0, -qb, qb); d = np.abs(d)
    th = np.arccos(np.clip(d, -1, 1))
    s = np.sin(th)
    wa = np.where(s > 1e-6, np.sin((1 - share) * th) / np.maximum(s, 1e-9), 1 - share)
    wb = np.where(s > 1e-6, np.sin(share * th) / np.maximum(s, 1e-9), share)
    return turn_of_quat(wa * qa + wb * qb)


def measures(names=None):
    """The body's own measures, the same in every clip: where its joints sit on the pelvis, the chest, the head and the hand, and its bones' lengths."""
    acc = {}
    def add(k, v):
        acc.setdefault(k, []).append(v)
    for name in (names or ['sns_idle'] + STRIKES):
        c = Clip(name); p = c.p
        pel, che, head, hands = frames_of(c)
        add('spine', into(pel, p('Spine') - p('Hips')))
        s1 = slerp_turn(pel, che, SPINE_SHARES[0]); s2 = slerp_turn(pel, che, SPINE_SHARES[1])
        add('spine1', into(s1, p('Spine1') - p('Spine')))
        add('spine2', into(s2, p('Spine2') - p('Spine1')))
        add('neck', into(che, p('Neck') - p('Spine2')))
        nh = slerp_turn(che, head, 0.5)
        add('head', into(nh, p('Head') - p('Neck')))
        add('headtop', into(head, p('HeadTop_End') - p('Head')))
        for s, S in SIDES:
            add(s + '_eye', into(head, p(S + 'Eye') - p('Head')))
            add(s + '_hip', into(pel, p(S + 'UpLeg') - p('Hips')))
            add(s + '_clav', into(che, p(S + 'Shoulder') - p('Spine2')))
            for k, a, b in (('_clav_len', 'Shoulder', 'Arm'), ('_upper_len', 'Arm', 'ForeArm'), ('_fore_len', 'ForeArm', 'Hand'), ('_thigh_len', 'UpLeg', 'Leg'), ('_shin_len', 'Leg', 'Foot'),
                            ('_foot_len', 'Foot', 'ToeBase'), ('_toe_len', 'ToeBase', 'Toe_End')):
                add(s + k, np.linalg.norm(p(S + b) - p(S + a), axis=1))
            for f in ('Index1', 'Middle1', 'Pinky1'):
                add(s + '_' + f.lower(), into(hands[s], p(S + 'Hand' + f) - p(S + 'Hand')))
    m = {k: np.mean(np.concatenate(v, axis=0), axis=0) for k, v in acc.items()}
    spread = {k: float(np.max(np.std(np.concatenate(v, axis=0), axis=0))) for k, v in acc.items()}
    return m, spread


def decompose(c):
    """A clip's tracks (see the top of the file). Angles in degrees, places in metres, ways as unit vectors."""
    p = c.p
    pel, che, head, hands = frames_of(c)
    T = {'t': c.tc(), 'rate': c.rate}
    T['pelvis'] = p('Hips').copy()
    T['pelvis_turn'] = ypr_of(pel)
    T['chest_turn'] = ypr_of(np.einsum('nji,njk->nik', pel, che))          # the chest on the pelvis
    T['head_turn'] = ypr_of(np.einsum('nji,njk->nik', che, head))          # the head on the chest
    for s, S in SIDES:
        T[s + '_clav'] = unit(into(che, p(S + 'Arm') - p(S + 'Shoulder')))
        T[s + '_upper'] = unit(into(che, p(S + 'ForeArm') - p(S + 'Arm')))
        T[s + '_fore'] = unit(into(che, p(S + 'Hand') - p(S + 'ForeArm')))
        T[s + '_hand'] = quat_of(np.einsum('nji,njk->nik', che, hands[s]))  # the hand's turn on the chest
        hip, knee, ankle = p(S + 'UpLeg'), p(S + 'Leg'), p(S + 'Foot')
        T[s + '_ankle'] = ankle.copy()
        axis = unit(ankle - hip)
        way = (knee - hip) - axis * np.sum((knee - hip) * axis, axis=1, keepdims=True)
        T[s + '_knee'] = unit(into(pel, way))                               # which way the knee points, off the line from hip to ankle
        T[s + '_foot'] = unit(p(S + 'ToeBase') - ankle)
        T[s + '_toe'] = unit(p(S + 'Toe_End') - p(S + 'ToeBase'))
    return T


BONES = ['Hips', 'Spine', 'Spine1', 'Spine2', 'Neck', 'Head', 'HeadTop_End', 'LeftEye', 'RightEye', 'LeftShoulder', 'LeftArm', 'LeftForeArm', 'LeftHand', 'LeftHandIndex1', 'LeftHandMiddle1',
         'LeftHandPinky1', 'RightShoulder', 'RightArm', 'RightForeArm', 'RightHand', 'RightHandIndex1', 'RightHandMiddle1', 'RightHandPinky1', 'LeftUpLeg', 'LeftLeg', 'LeftFoot',
         'LeftToeBase', 'LeftToe_End', 'RightUpLeg', 'RightLeg', 'RightFoot', 'RightToeBase', 'RightToe_End']


def compose(T, m):
    """The 33 points (world, metres) from tracks and the body's measures."""
    n = len(T['pelvis'])
    P = {}
    pel = turn(T['pelvis_turn'])
    che = pel @ turn(T['chest_turn'])
    head = che @ turn(T['head_turn'])
    P['Hips'] = T['pelvis']
    P['Spine'] = P['Hips'] + out(pel, m['spine'])
    P['Spine1'] = P['Spine'] + out(slerp_turn(pel, che, SPINE_SHARES[0]), m['spine1'])
    P['Spine2'] = P['Spine1'] + out(slerp_turn(pel, che, SPINE_SHARES[1]), m['spine2'])
    P['Neck'] = P['Spine2'] + out(che, m['neck'])
    P['Head'] = P['Neck'] + out(slerp_turn(che, head, 0.5), m['head'])
    P['HeadTop_End'] = P['Head'] + out(head, m['headtop'])
    for s, S in SIDES:
        P[S + 'Eye'] = P['Head'] + out(head, m[s + '_eye'])
        P[S + 'Shoulder'] = P['Spine2'] + out(che, m[s + '_clav'])
        P[S + 'Arm'] = P[S + 'Shoulder'] + out(che, unit(T[s + '_clav'])) * m[s + '_clav_len']
        P[S + 'ForeArm'] = P[S + 'Arm'] + out(che, unit(T[s + '_upper'])) * m[s + '_upper_len']
        P[S + 'Hand'] = P[S + 'ForeArm'] + out(che, unit(T[s + '_fore'])) * m[s + '_fore_len']
        hand = che @ turn_of_quat(T[s + '_hand'])
        for f in ('Index1', 'Middle1', 'Pinky1'):
            P[S + 'Hand' + f] = P[S + 'Hand'] + out(hand, m[s + '_' + f.lower()])
        hip = P['Hips'] + out(pel, m[s + '_hip'])
        ankle = T[s + '_ankle']
        a, b = m[s + '_thigh_len'], m[s + '_shin_len']
        to = ankle - hip
        d = np.linalg.norm(to, axis=1)
        # (A leg asked longer than it is: the ankle comes in to where the leg reaches.)
        reach = np.minimum(d, 0.999 * (a + b))
        axis = unit(to)
        ankle = hip + axis * reach[:, None]
        along = (reach * reach + a * a - b * b) / (2 * reach)
        off = np.sqrt(np.maximum(0, a * a - along * along))
        way = out(pel, unit(T[s + '_knee']))
        way = unit(way - axis * np.sum(way * axis, axis=1, keepdims=True))
        P[S + 'UpLeg'] = hip
        P[S + 'Leg'] = hip + axis * along[:, None] + way * off[:, None]
        P[S + 'Foot'] = ankle
        P[S + 'ToeBase'] = ankle + unit(T[s + '_foot']) * m[s + '_foot_len']
        P[S + 'Toe_End'] = P[S + 'ToeBase'] + unit(T[s + '_toe']) * m[s + '_toe_len']
    return np.stack([P[b] for b in BONES], axis=1)


if __name__ == '__main__':
    m, spread = measures()
    print('the body\'s measures (m), and how much each varies over the clips (the most of its parts, m):')
    for k in sorted(m):
        v = m[k]
        print(f"  {k:14} {np.array2string(np.atleast_1d(v), precision=3, suppress_small=True):32} spread {spread[k]:.4f}")
    print('\ntaken apart and put together: how far each clip\'s points come out from where they went in (cm)')
    for name in ['sns_idle'] + STRIKES:
        c = Clip(name)
        W = compose(decompose(c), m)
        e = np.linalg.norm(W - c.W, axis=2) * 100
        worst = int(np.argmax(e.mean(axis=0)))
        print(f"  {name:20} mean {e.mean():.2f}  worst frame {e.mean(axis=1).max():.2f}  worst point {BONES[worst]} {e.mean(axis=0)[worst]:.2f} (most {e[:, worst].max():.2f})")
