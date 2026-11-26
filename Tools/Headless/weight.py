#!/usr/bin/env python3
"""Is his weight over his feet? A walk's physics checked from its poses alone.

    python3 Tools/Headless/weight.py film.json [unit name part]      a headless film (FIGHT_FILM="file.json,from,to")
    python3 Tools/Headless/weight.py --clips [tempo] [name part ...]  the baked recordings, as performed or slowed

A body can only be pushed by the ground under its feet. From where his centre of mass is and how it speeds up and slows
down, there is one spot on the ground the push must come from (the "zero moment point": the centre of mass's place on
the ground, moved back by height / gravity times its acceleration). If that spot is under a foot that is on the ground,
the motion is one a real body could make. If it is beside his feet, nothing holds him up there: a real man would tip
over, and to the eye he floats. Written 26 November 2026 for the plan "Walking that feels like a real person".

What it prints for each walk: how much of the time the spot is outside his feet (by more than 5 cm, and by more than
10), how far outside it is on average when it is, and how much of the time no foot is on the ground at all.

NOT CALIBRATED (26 November 2026): it reads 0% outside for the standing recording, but 14 to 72% for walking recordings
as performed and 69 to 79% for the game's walk, and a slowed recording reads better, not worse. Do not quote its numbers
until it passes on footage known to be clean; it is here as the start of the plan's step 0.

Rough on purpose: the centre of mass is worked out from the joints with a textbook's shares of weight (no sword, no
shield), a foot is "on the ground" when its ankle or toe is low and nearly still, and its print is a box from 7 cm
behind the ankle to the toe, 10 cm wide. The same rule is used for the recordings and for the game, so the two can be
compared; standing still is printed as a check of the method (it should be inside nearly always).
"""
import json, os, sys
import numpy as np

G = 9.81
LEG = 0.845          # a Medium's leg length (m): the recordings are in leg lengths
HZ = 60.0
CUT = 5.0            # low-pass (Hz) before speeds and accelerations are taken

# (from joint, to joint, share of body weight, how far along from the first joint its weight sits)
FILM = {'Pelvis': 0, 'Spine': 1, 'Chest': 2, 'Head': 3, 'UpperArmL': 4, 'ForearmL': 5, 'HandL': 6, 'UpperArmR': 7,
        'ForearmR': 8, 'HandR': 9, 'ThighL': 10, 'ShinL': 11, 'FootL': 12, 'ThighR': 13, 'ShinR': 14, 'FootR': 15,
        'HeadTop': 16, 'HandEndL': 17, 'HandEndR': 18, 'ToeL': 19, 'ToeR': 20}
FILM_SEGS = [('Pelvis', 'Spine', 0.142, 0.3), ('Spine', 'Chest', 0.139, 0.5), ('Chest', 'Head', 0.216, 0.5),
             ('Head', 'HeadTop', 0.081, 0.5),
             ('UpperArmL', 'ForearmL', 0.028, 0.436), ('ForearmL', 'HandL', 0.016, 0.43), ('HandL', 'HandEndL', 0.006, 0.5),
             ('UpperArmR', 'ForearmR', 0.028, 0.436), ('ForearmR', 'HandR', 0.016, 0.43), ('HandR', 'HandEndR', 0.006, 0.5),
             ('ThighL', 'ShinL', 0.100, 0.433), ('ShinL', 'FootL', 0.0465, 0.433), ('FootL', 'ToeL', 0.0145, 0.5),
             ('ThighR', 'ShinR', 0.100, 0.433), ('ShinR', 'FootR', 0.0465, 0.433), ('FootR', 'ToeR', 0.0145, 0.5)]
CLIP_SEGS = [('Hips', 'Spine1', 0.142, 0.3), ('Spine1', 'Spine2', 0.139, 0.5), ('Spine2', 'Head', 0.216, 0.5),
             ('Head', 'HeadTop_End', 0.081, 0.5),
             ('LeftArm', 'LeftForeArm', 0.028, 0.436), ('LeftForeArm', 'LeftHand', 0.016, 0.43), ('LeftHand', 'LeftHandMiddle1', 0.006, 0.5),
             ('RightArm', 'RightForeArm', 0.028, 0.436), ('RightForeArm', 'RightHand', 0.016, 0.43), ('RightHand', 'RightHandMiddle1', 0.006, 0.5),
             ('LeftUpLeg', 'LeftLeg', 0.100, 0.433), ('LeftLeg', 'LeftFoot', 0.0465, 0.433), ('LeftFoot', 'LeftToe_End', 0.0145, 0.5),
             ('RightUpLeg', 'RightLeg', 0.100, 0.433), ('RightLeg', 'RightFoot', 0.0465, 0.433), ('RightFoot', 'RightToe_End', 0.0145, 0.5)]


def lowpass(x, hz=HZ, cut=CUT):
    """A zero-phase low-pass down each column (a Gaussian whose half-power point is the cut)."""
    sigma = 0.1325 / cut * hz
    r = int(np.ceil(4 * sigma)); k = np.exp(-0.5 * (np.arange(-r, r + 1) / sigma) ** 2); k /= k.sum()
    x = np.asarray(x, float)
    pad = np.concatenate([np.repeat(x[:1], r, 0), x, np.repeat(x[-1:], r, 0)])
    return np.stack([np.convolve(pad[:, j], k, mode='valid') for j in range(x.shape[1])], axis=1)


def com(joints, index, segs):
    c = np.zeros((joints.shape[0], 3)); m = 0.0
    for a, b, share, along in segs:
        pa, pb = joints[:, index[a]], joints[:, index[b]]
        c += share * (pa + along * (pb - pa)); m += share
    return c / m


def hull(pts):
    pts = sorted(set(map(tuple, np.round(pts, 5))))
    if len(pts) <= 2:
        return pts
    def cross(o, a, b):
        return (a[0] - o[0]) * (b[1] - o[1]) - (a[1] - o[1]) * (b[0] - o[0])
    lo, up = [], []
    for p in pts:
        while len(lo) >= 2 and cross(lo[-2], lo[-1], p) <= 0:
            lo.pop()
        lo.append(p)
    for p in reversed(pts):
        while len(up) >= 2 and cross(up[-2], up[-1], p) <= 0:
            up.pop()
        up.append(p)
    return lo[:-1] + up[:-1]


def outside(poly, q):
    """How far a point is outside a convex shape (0 inside)."""
    if len(poly) == 0:
        return float('nan')
    n = len(poly); inside = n >= 3; best = 1e9
    for i in range(n):
        a = np.array(poly[i]); b = np.array(poly[(i + 1) % n]); ab = b - a
        t = 0.0 if ab @ ab < 1e-12 else float(np.clip((q - a) @ ab / (ab @ ab), 0, 1))
        best = min(best, float(np.linalg.norm(q - (a + t * ab))))
        if n >= 3 and (ab[0] * (q[1] - a[1]) - ab[1] * (q[0] - a[0])) < 0:
            inside = False
    return 0.0 if inside else best


def footprint(ankle, toe, heel_low, toe_low):
    u = (toe - ankle)[[0, 2]]; L = np.linalg.norm(u); u = u / max(L, 1e-6); s = np.array([-u[1], u[0]]) * 0.05
    a = ankle[[0, 2]]; t = toe[[0, 2]]
    back = a - 0.07 * u if heel_low else t - 0.09 * u       # only the ball of the foot once the heel is up
    front = t + 0.01 * u if toe_low else a + 0.08 * u       # only the heel while the toe is still up
    return [back + s, back - s, front + s, front - s]


def weigh(joints, index, segs, feet, hz=HZ, mask=None):
    """joints: frames x joints x 3 in the world (m), at hz. feet: ((ankle, toe) left, (ankle, toe) right) joint names.
    Returns per frame: how far the spot is outside his feet (m; nan with no foot down), and how many feet are down."""
    n = joints.shape[0]
    c = lowpass(com(joints, index, segs), hz)
    v = np.gradient(c, 1.0 / hz, axis=0); a = np.gradient(v, 1.0 / hz, axis=0)
    spot = np.stack([c[:, 0] - c[:, 1] * a[:, 0] / (a[:, 1] + G), c[:, 2] - c[:, 1] * a[:, 2] / (a[:, 1] + G)], axis=1)
    out = np.full(n, np.nan); down = np.zeros(n, int)
    f = []
    for ankle, toe in feet:
        A = lowpass(joints[:, index[ankle]], hz); T = lowpass(joints[:, index[toe]], hz)
        va = np.linalg.norm(np.gradient(A, 1.0 / hz, axis=0)[:, [0, 2]], axis=1)
        vt = np.linalg.norm(np.gradient(T, 1.0 / hz, axis=0)[:, [0, 2]], axis=1)
        heel_low = (A[:, 1] < np.percentile(A[:, 1], 2) + 0.04) & (va < 0.5)
        toe_low = (T[:, 1] < np.percentile(T[:, 1], 2) + 0.03) & (vt < 0.5)
        f.append((A, T, heel_low, toe_low))
    for i in range(n):
        pts = []
        for A, T, hl, tl in f:
            if hl[i] or tl[i]:
                pts += footprint(A[i], T[i], hl[i], tl[i]); down[i] += 1
        if pts:
            out[i] = outside(hull(np.array(pts)), spot[i])
    return out, down, c, a


def say(label, out, down, sel):
    o = out[sel]; d = down[sel]
    on = ~np.isnan(o)
    if on.sum() < 10:
        print(f"{label}: too little"); return None
    over5 = np.mean(o[on] > 0.05) * 100; over10 = np.mean(o[on] > 0.10) * 100
    far = np.mean(o[on][o[on] > 0.05]) * 100 if (o[on] > 0.05).any() else 0.0
    res = dict(over5=over5, over10=over10, far=far, none=np.mean(d == 0) * 100, both=np.mean(d == 2) * 100, one=np.mean(d == 1) * 100)
    print(f"{label}: outside his feet by over 5 cm {over5:3.0f}% of the time, by over 10 cm {over10:3.0f}% (then {far:.0f} cm out on average); "
          f"feet down: both {res['both']:.0f}%, one {res['one']:.0f}%, none {res['none']:.0f}%")
    return res


def film(path, who=None):
    frames = json.load(open(path))
    for name in sorted(set(f['u'] for f in frames)):
        if who and who.lower() not in name.lower():
            continue
        fr = [f for f in frames if f['u'] == name]
        if len(fr) < 60:
            continue
        t = np.array([f['t'] for f in fr]); hz = 1.0 / float(np.median(np.diff(t)))
        J = np.array([f['p'] for f in fr]).reshape(len(fr), -1, 3)
        out, down, c, a = weigh(J, FILM, FILM_SEGS, (('FootL', 'ToeL'), ('FootR', 'ToeR')), hz)
        doing = np.array([f['doing'].split()[0] for f in fr])
        print(f"== {name} ({len(fr)/hz:.0f} s at {hz:.0f} a second)")
        say("   walking ", out, down, doing == 'Cycle')
        say("   standing", out, down, doing == 'Idle')
        w = doing == 'Cycle'
        ah = np.linalg.norm(a[w][:, [0, 2]], axis=1)
        print(f"   walking: his centre of mass is sped up and slowed by {np.median(ah):.2f} m/s2 (median), {np.percentile(ah, 90):.2f} at the top tenth; "
              f"up and down by {np.percentile(np.abs(a[w][:, 1]), 90):.2f}")


def clip_world(c, bones, tempo=1.0, loops=3):
    n = c['frames']; nb = len(bones)
    p = np.array(c['points']).reshape(n, nb, 3) * LEG
    r = np.array(c['root']).reshape(n, 3); place = r[:, :2] * LEG; yaw = np.radians(r[:, 2])
    drift = (place[-1] - place[0]) * n / (n - 1) if c.get('loop') else np.zeros(2)
    reps = loops if c.get('loop') else 1
    W = []
    for k in range(reps):
        pl = place + k * drift
        right = np.stack([np.cos(yaw), -np.sin(yaw)], axis=1); fwd = np.stack([np.sin(yaw), np.cos(yaw)], axis=1)
        x = pl[:, None, 0] + p[:, :, 0] * right[:, None, 0] + p[:, :, 2] * fwd[:, None, 0]
        z = pl[:, None, 1] + p[:, :, 0] * right[:, None, 1] + p[:, :, 2] * fwd[:, None, 1]
        W.append(np.stack([x, p[:, :, 1], z], axis=2))
    W = np.concatenate(W, axis=0)
    # to 60 a second, played at the tempo (slower than recorded when under 1)
    t_src = np.arange(W.shape[0]) / (c['rate'] * tempo)
    t = np.arange(0, t_src[-1], 1.0 / HZ)
    flat = W.reshape(W.shape[0], -1)
    out = np.stack([np.interp(t, t_src, flat[:, j]) for j in range(flat.shape[1])], axis=1).reshape(len(t), nb, 3)
    mid = (t >= t_src[-1] / 3) & (t < 2 * t_src[-1] / 3) if reps == 3 else np.ones(len(t), bool)
    return out, mid


def clips(tempo, wanted):
    here = os.path.dirname(os.path.abspath(__file__))
    res = os.path.join(here, '..', '..', 'Assets', '_Battler', 'Resources', 'Locomotion')
    rows = []
    for fn in ('LocoClips.json', 'CombatClips.json'):
        d = json.load(open(os.path.join(res, fn))); bones = d['bones']; index = {b: i for i, b in enumerate(bones)}
        for c in d['clips']:
            if wanted and not any(w in c['name'] for w in wanted):
                continue
            if not wanted and not (c.get('loop') and c.get('role') in ('cycle', 'idle')):
                continue
            J, mid = clip_world(c, bones, tempo)
            out, down, cm, a = weigh(J, index, CLIP_SEGS, (('LeftFoot', 'LeftToe_End'), ('RightFoot', 'RightToe_End')))
            r = say(f"{c['name']:22s} {c['speed']*LEG*tempo:4.2f} m/s at tempo {tempo:.2f}", out, down, mid)
            if r:
                rows.append((c['name'], c['speed'] * LEG * tempo, r))
    return rows


if __name__ == '__main__':
    if len(sys.argv) > 1 and sys.argv[1] == '--clips':
        rest = sys.argv[2:]
        tempo = 1.0
        if rest:
            try:
                tempo = float(rest[0]); rest = rest[1:]
            except ValueError:
                pass
        clips(tempo, rest)
    else:
        film(sys.argv[1], sys.argv[2] if len(sys.argv) > 2 else None)
