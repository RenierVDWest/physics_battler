"""
What a strike clip is made of, part by part (the plan "Aimed attacks", the research of 14-15 November).

Reads the baked combat clips (Assets/_Battler/Resources/Locomotion/CombatClips.json) and gives each clip as
"channels": the body's own turns and travels, frame by frame, in metres and degrees, in the frame the fighter
faced as the clip began (x to its right, y up, z the way it faced).

    pelvis      where the hips are, and how they are turned (yaw: + to its right), tipped forward (pitch) and to a side
    chest       the same for the ribcage, and its turn against the pelvis (the "twist")
    arms        the shoulder's place on the chest, the upper arm's way, the elbow's bend, the forearm, the blade
    legs        each ankle and toe, the knee's bend, whether heel and toe are down
"""
import json, math, os
import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
CLIPS = os.path.join(HERE, '..', '..', '..', 'Assets', '_Battler', 'Resources', 'Locomotion', 'CombatClips.json')
BLADE = 0.9          # the sword's length (m) and where along it it strikes
SWEET = 0.7

_file = None


def clips():
    global _file
    if _file is None:
        _file = json.load(open(CLIPS))
    return _file


def unit(v):
    n = np.linalg.norm(v, axis=-1, keepdims=True)
    return v / np.maximum(n, 1e-9)


def yaw_of(v):
    """A flat way's turn from forward (degrees, + to the right)."""
    return np.degrees(np.arctan2(v[..., 0], v[..., 2]))


def unwrap_deg(a):
    return np.degrees(np.unwrap(np.radians(a)))


def smooth(x, rate, seconds):
    """A running mean over so long (the same length back)."""
    n = max(1, int(round(seconds * rate)) | 1)
    if n <= 1:
        return x
    k = np.ones(n) / n
    pad = n // 2
    if x.ndim == 1:
        xp = np.concatenate([np.full(pad, x[0]), x, np.full(pad, x[-1])])
        return np.convolve(xp, k, mode='valid')
    return np.stack([smooth(x[:, i], rate, seconds) for i in range(x.shape[1])], axis=1)


class Clip:
    def __init__(self, name):
        f = clips()
        d = next(c for c in f['clips'] if c['name'] == name)
        self.name = name
        self.bones = f['bones']
        self.ix = {b: i for i, b in enumerate(self.bones)}
        self.rate = d['rate']
        self.n = d['frames']
        self.leg = d['legLength']
        self.kind = d.get('kind') or ''
        self.about = d.get('about') or ''
        m = d.get('markers') or []
        self.contact = m[0] if len(m) == 2 and d['role'] == 'strike' else -1
        self.link = m[1] if len(m) == 2 and d['role'] == 'strike' else -1
        P = np.array(d['points'], float).reshape(self.n, len(self.bones), 3) * self.leg
        R = np.array(d['root'], float).reshape(self.n, 3)
        self.root = np.stack([R[:, 0] * self.leg, np.zeros(self.n), R[:, 1] * self.leg], axis=1)
        self.facing = unwrap_deg(R[:, 2])
        self.local = P                                    # against the root, as baked (metres)
        self.contacts = np.array(d['contacts'], int).reshape(self.n, 2)
        # The world, in the frame it faced as the clip began.
        W = np.zeros_like(P)
        th = np.radians(self.facing - self.facing[0])
        c, s = np.cos(th)[:, None], np.sin(th)[:, None]
        W[:, :, 0] = c * P[:, :, 0] + s * P[:, :, 2]
        W[:, :, 1] = P[:, :, 1]
        W[:, :, 2] = -s * P[:, :, 0] + c * P[:, :, 2]
        th0 = math.radians(-self.facing[0])
        r = self.root - self.root[0]
        rx = math.cos(th0) * r[:, 0] + math.sin(th0) * r[:, 2]
        rz = -math.sin(th0) * r[:, 0] + math.cos(th0) * r[:, 2]
        self.rootw = np.stack([rx, np.zeros(self.n), rz], axis=1)
        self.W = W + self.rootw[:, None, :]
        self.t = np.arange(self.n) / self.rate

    def p(self, bone):
        return self.W[:, self.ix[bone]]

    def tc(self):
        """Seconds against the contact (under 0 before it)."""
        return self.t - self.contact / self.rate


def frame_of(right, up):
    """A body part's own axes from its right and its up (rows: right, up, forward), each frame."""
    r = unit(right)
    f = unit(np.cross(r, up))
    u = np.cross(f, r)
    return np.stack([r, u, f], axis=1)        # (n, 3, 3): rows


def angles_of(F):
    """A frame's yaw (+ to the right), pitch (+ tipped forward) and roll (+ its right side down), degrees."""
    f, r = F[:, 2], F[:, 0]
    yaw = unwrap_deg(yaw_of(f))
    pitch = -np.degrees(np.arcsin(np.clip(F[:, 1, 2], -1, 1)))       # the up axis leaning forward: its z part
    pitch = np.degrees(np.arcsin(np.clip(F[:, 1][:, 2] * np.cos(np.radians(yaw)) + F[:, 1][:, 0] * np.sin(np.radians(yaw)), -1, 1)))
    roll = -np.degrees(np.arcsin(np.clip(r[:, 1], -1, 1)))
    return yaw, pitch, roll


def to_frame(F, v):
    """A world way in a part's own axes (x right, y up, z forward)."""
    return np.einsum('nij,nj->ni', F, v)


def angle(a, b):
    return np.degrees(np.arccos(np.clip(np.sum(unit(a) * unit(b), axis=-1), -1, 1)))


def channels(c):
    """Every channel of a clip, each an array over its frames."""
    ch = {}
    hips, spine, spine2, neck, head = c.p('Hips'), c.p('Spine'), c.p('Spine2'), c.p('Neck'), c.p('Head')
    lhip, rhip = c.p('LeftUpLeg'), c.p('RightUpLeg')
    lsh, rsh = c.p('LeftArm'), c.p('RightArm')
    pel = frame_of(rhip - lhip, spine - hips)
    che = frame_of(rsh - lsh, neck - spine2)
    ch['pelvis'] = hips
    ch['pelvis_yaw'], ch['pelvis_pitch'], ch['pelvis_roll'] = angles_of(pel)
    ch['chest_yaw'], ch['chest_pitch'], ch['chest_roll'] = angles_of(che)
    ch['twist'] = ch['chest_yaw'] - ch['pelvis_yaw']
    ch['chest'] = spine2
    ch['pel'], ch['che'] = pel, che
    for side, S in (('r', 'Right'), ('l', 'Left')):
        sh, el, wr = c.p(S + 'Arm'), c.p(S + 'ForeArm'), c.p(S + 'Hand')
        ch[side + '_shoulder'] = to_frame(che, sh - spine2)             # the shoulder's place on the chest
        up = to_frame(che, el - sh)
        ch[side + '_upper'] = unit(up)
        ch[side + '_raised'] = np.degrees(np.arccos(np.clip(-unit(up)[:, 1], -1, 1)))     # 0 hanging, 90 level, 180 overhead
        ch[side + '_toward'] = np.degrees(np.arctan2(unit(up)[:, 0] * (1 if side == 'r' else -1), unit(up)[:, 2]))   # 0 forward, 90 out, - across
        ch[side + '_elbow'] = angle(el - sh, wr - el)                   # 0 straight
        ch[side + '_fore'] = unit(to_frame(che, wr - el))
        ch[side + '_hand'] = wr
        ch[side + '_hand_chest'] = to_frame(che, wr - spine2)
        along = unit(c.p(S + 'HandMiddle1') - wr)
        across = unit(c.p(S + 'HandIndex1') - c.p(S + 'HandPinky1'))
        ch[side + '_along'], ch[side + '_across'] = along, across
    blade = unit(ch['r_across'] * math.cos(math.radians(20)) + ch['r_along'] * math.sin(math.radians(20)))
    ch['blade'] = blade
    ch['tip'] = ch['r_hand'] + blade * BLADE
    ch['strike'] = ch['r_hand'] + blade * BLADE * SWEET
    ch['cock'] = angle(c.p('RightHand') - c.p('RightForeArm'), blade)     # the blade against the forearm: 0 in line, 90 square
    hr = frame_of(c.p('RightEye') - c.p('LeftEye'), c.p('HeadTop_End') - head)
    ch['head_yaw'], ch['head_pitch'], _ = angles_of(hr)
    for side, S, k in (('l', 'Left', 0), ('r', 'Right', 1)):
        hip, kn, an, toe, end = c.p(S + 'UpLeg'), c.p(S + 'Leg'), c.p(S + 'Foot'), c.p(S + 'ToeBase'), c.p(S + 'Toe_End')
        ch[side + '_ankle'], ch[side + '_toe'], ch[side + '_knee_at'] = an, toe, kn
        ch[side + '_knee'] = angle(kn - hip, an - kn)                   # 0 straight
        ch[side + '_foot_yaw'] = unwrap_deg(yaw_of(toe - an))
        ch[side + '_heel_down'] = (c.contacts[:, k] & 1) > 0
        ch[side + '_toe_down'] = (c.contacts[:, k] & 2) > 0
        ch[side + '_down'] = (c.contacts[:, k] & 4) > 0
        ch[side + '_ankle_bend'] = angle(kn - an, toe - an)             # the shin against the foot: smaller, the toes pointed
    # The body's weight, roughly: the trunk's and the limbs' middles by a body's shares.
    def mid(a, b):
        return 0.5 * (c.p(a) + c.p(b))
    com = 0.14 * mid('Hips', 'Spine') + 0.36 * mid('Spine', 'Neck') + 0.08 * mid('Head', 'HeadTop_End')
    for S in ('Left', 'Right'):
        com = com + 0.028 * mid(S + 'Arm', S + 'ForeArm') + 0.022 * mid(S + 'ForeArm', S + 'Hand')
        com = com + 0.10 * mid(S + 'UpLeg', S + 'Leg') + 0.061 * mid(S + 'Leg', S + 'Foot')
    ch['com'] = com
    return ch


def speed(x, rate, smooth_s=0.0):
    """How fast a place moves (m/s) or an angle turns (degrees a second)."""
    v = np.gradient(x, axis=0) * rate
    v = np.linalg.norm(v, axis=1) if v.ndim == 2 else v
    return smooth(v, rate, smooth_s) if smooth_s > 0 else v


STRIKES = ['sns_forehand', 'sns_combo_forehand', 'sns_backhand_lunge', 'sns_combo_backhand', 'sns_rising', 'sns_thrust', 'sns_chop_lunge', 'sns_chop_side', 'sns_combo_chop']
