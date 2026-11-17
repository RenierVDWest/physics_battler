#!/usr/bin/env python3
"""Get-ups rebuilt from the recordings (the pack "Getting Up"): a recorded get-up is not played. It is read off, at a
dozen moments, into the same parts the get-up's own key poses are made of (Assets/_Battler/Scripts/Runtime/Motion/
GetUp.cs, Key): where the hips are and how they are pitched, rolled and leaned; how the spine, chest and head bend;
where each ankle and wrist is; which way each knee and elbow points; how each foot is pitched. All against where the
man ends up standing (the middle of his feet, the way he faces), in leg lengths and degrees, with his standing pose
taken off so that the last key is the game's own stand. The game makes its keys from this table and plays them as it
plays the ones it makes itself: from the body as it really lay, hands and held things kept off the ground, the feet
ending on the stance's own spots.

    python3 Tools/Mocap/Combat/getup_keys.py            writes the table into GetUp.cs between its two marks
    python3 Tools/Mocap/Combat/getup_keys.py --report   prints the keys
"""
import json, math, os, sys
import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
SAMPLES = os.path.join(HERE, 'samples')
TARGET = os.path.normpath(os.path.join(HERE, '..', '..', '..', 'Assets', '_Battler', 'Scripts', 'Runtime', 'Motion', 'GetUp.cs'))
KEYS = 12
# name: (sample, from s, to s, about). The part of the take in which he gets up.
TAKES = {
    'Back': ('getup_from_back', 2.6, 6.3, 'From lying on his back: sits up, rocks forward over his feet onto them, stands'),
    'Front': ('getup_from_front', 1.6, 6.1, 'From lying on his front: pushes up, hips up, a foot stepped through, stands'),
}


def unit(v):
    n = np.linalg.norm(v)
    return v / n if n > 1e-9 else v


def angle_about(a, b, axis):
    """The signed angle from a to b about an axis (degrees)."""
    a = unit(a - axis * np.dot(a, axis)); b = unit(b - axis * np.dot(b, axis))
    return math.degrees(math.atan2(np.dot(np.cross(a, b), axis), np.dot(a, b)))


def mx(t):
    c, s = math.cos(math.radians(t)), math.sin(math.radians(t))
    return np.array([[1, 0, 0], [0, c, -s], [0, s, c]])


def mz(t):
    c, s = math.cos(math.radians(t)), math.sin(math.radians(t))
    return np.array([[c, -s, 0], [s, c, 0], [0, 0, 1]])


def read(sample, start, end):
    d = json.load(open(os.path.join(SAMPLES, sample + '.json')))
    B = {b: i for i, b in enumerate(d['bones'])}
    F = np.array(d['frames']).reshape(len(d['frames']), -1, 3)
    rate = d['rate']
    J = lambda name, i: F[i, B[name]]
    last = len(F) - 1
    leg = np.linalg.norm(J('LeftUpLeg', last) - J('LeftLeg', last)) + np.linalg.norm(J('LeftLeg', last) - J('LeftFoot', last))
    ground = min(F[:, B['LeftToeBase'], 1].min(), F[:, B['RightToeBase'], 1].min()) - 0.02
    # Where he ends up standing: the middle of his ankles on the ground, facing as his hips do.
    middle = 0.5 * (J('LeftFoot', last) + J('RightFoot', last))
    right = J('RightUpLeg', last) - J('LeftUpLeg', last); right[1] = 0; right = unit(right)
    up = np.array([0.0, 1.0, 0.0])
    fwd = np.cross(right, up)
    if np.dot(fwd, J('LeftToeBase', last) - J('LeftFoot', last)) < 0:
        fwd = -fwd; right = -right

    axes = [right, fwd]

    def local(v):
        w = v - middle
        return np.array([np.dot(w, axes[0]), v[1] - ground, np.dot(w, axes[1])])

    def way(v):
        return np.array([np.dot(v, axes[0]), v[1], np.dot(v, axes[1])])

    def heading(i):
        """The way the body heads at a frame, on the ground: lying, along its length (toward its feet on its back, toward
        its head on its front: the way it will stand up); upright, the way its hips face; between, a mix."""
        u = unit(J('Spine1', i) - J('Hips', i))
        r = J('RightUpLeg', i) - J('LeftUpLeg', i); r = unit(r - u * np.dot(r, u))
        f = np.cross(r, u)
        if np.dot(np.cross(right, up), fwd) < 0:
            f = -f
        lying = min(1.0, max(0.0, (math.degrees(math.asin(min(1.0, math.hypot(u[0], u[2])))) - 20.0) / 40.0))
        along = np.array([u[0], 0.0, u[2]]); along = unit(along) * (1.0 if f[1] < 0 else -1.0)
        faces = unit(np.array([f[0], 0.0, f[2]]))
        return unit(lying * along + (1.0 - lying) * faces)

    def pose(i):
        # (Each frame against the way the body heads then, about where he ends up standing: he turns as he gets up, and
        # a key has no turn on the ground. The route is straightened; the pose at each moment is as it was.)
        h = heading(i)
        if turns:
            axes[1] = h
            axes[0] = np.cross(up, h)
        p = {}
        hips = local(J('Hips', i))
        # (The trunk as one piece, which is what a key has: its line from the hips to between the shoulders, and its
        # roll about that line the mean of the hips' and the shoulders'. Read at the hips alone, a man piked on his
        # hands with his back rounded reads far more upright than his shoulders are, and a hip dropped to one side
        # tips shoulders that he in fact keeps level.)
        u = unit(way(0.5 * (J('LeftArm', i) + J('RightArm', i)) - J('Hips', i)))
        r = unit(way(J('RightUpLeg', i) - J('LeftUpLeg', i))) + unit(way(J('RightArm', i) - J('LeftArm', i))); r = unit(r - u * np.dot(r, u))
        pitch = math.degrees(math.atan2(u[2], math.hypot(u[0], u[1])))
        flat = math.hypot(u[0], u[1])
        lean = math.degrees(math.atan2(u[0], u[1])) * min(1.0, flat / 0.5) if flat > 1e-3 else 0.0
        lean = max(-35.0, min(35.0, lean))
        R0 = mz(-lean) @ mx(pitch)
        r0, f0 = R0 @ np.array([1.0, 0, 0]), R0 @ np.array([0, 0, 1.0])
        roll = math.degrees(math.atan2(-np.dot(r, f0), np.dot(r, r0)))
        d0 = way(J('Spine1', i) - J('Hips', i)); d1 = way(J('Spine2', i) - J('Spine1', i)); d2 = way(J('Neck', i) - J('Spine2', i)); d3 = way(J('HeadTop_End', i) - J('Head', i))
        p.update(pelvis=hips, pitch=pitch, lean=lean, roll=roll, spine=angle_about(d0, d1, r), chest=angle_about(d1, d2, r), head=angle_about(d2, d3, r))
        for s, side in (('L', 'Left'), ('R', 'Right')):
            hip, knee, ankle, toe = (local(J(side + n, i)) for n in ('UpLeg', 'Leg', 'Foot', 'ToeBase'))
            axis = unit(ankle - hip)
            bend = (knee - hip) - axis * np.dot(knee - hip, axis)
            foot = toe - ankle
            p['ankle' + s] = ankle; p['knee' + s] = unit(bend) if np.linalg.norm(bend) > 0.02 else np.array([0, 0.3, 1.0])
            p['foot' + s] = math.degrees(math.atan2(-foot[1], foot[2]))
            sh, el, wr = (local(J(side + n, i)) for n in ('Arm', 'ForeArm', 'Hand'))
            axis = unit(wr - sh)
            bend = (el - sh) - axis * np.dot(el - sh, axis)
            p['hand' + s] = wr; p['elbow' + s] = unit(bend) if np.linalg.norm(bend) > 0.02 else np.array([0, -1.0, -0.3])
        return p

    turns = False
    stand = pose(last)
    turns = True
    times = np.linspace(start, end, KEYS)
    keys = []
    for t in times:
        i = min(last, int(round(t * rate)))
        p = pose(i)
        p['time'] = (t - start) / (end - start)
        keys.append(p)

    # Which hand is on the ground last, and which foot ends in front.
    post, posted = 0, -1.0
    for k in keys:
        for s, name in enumerate(('handL', 'handR')):
            if k[name][1] < 0.22 and k['time'] > posted and k['time'] > 0.25:
                posted, post = k['time'], s
    front = 0 if stand['ankleL'][2] > stand['ankleR'][2] else 1
    # When each foot is first where it ends (within 6 cm on the ground).
    planted = []
    for s in ('L', 'R'):
        at = 1.0
        for k in reversed(keys):
            if np.linalg.norm((k['ankle' + s] - stand['ankle' + s])[[0, 2]]) > 0.06:
                break
            at = k['time']
        planted.append(at)
    return dict(keys=keys, stand=stand, leg=leg, post=post, front=front, planted=planted, seconds=end - start)


def row(k, stand, leg, planted):
    """One key as the game's Key has it: positions in leg lengths, the standing pose taken off (so standing reads
    0 and the game puts its own in); the hips' height as a share of the standing height; each ankle given what
    is left to its standing place once it is planted (the game adds its stance's own spot)."""
    out = [k['time']]
    px, py, pz = k['pelvis']
    out += [(px - stand['pelvis'][0]) / leg, py / stand['pelvis'][1], (pz - stand['pelvis'][2]) / leg]
    out += [k['pitch'] - stand['pitch'], k['spine'] - stand['spine'], k['chest'] - stand['chest'], k['head'] - stand['head'], k['roll'] - stand['roll'], k['lean'] - stand['lean']]
    for i, s in enumerate(('L', 'R')):
        a = (k['ankle' + s] - stand['ankle' + s]) / leg
        out += [a[0], a[1], a[2]]
    for s in ('L', 'R'):
        out += list(k['knee' + s])
    for s in ('L', 'R'):
        f = k['foot' + s] - stand['foot' + s]
        out.append(max(-40.0, min(175.0, (f + 180) % 360 - 180 if abs(f) > 180 else f)))
    for s in ('L', 'R'):
        h = k['hand' + s] / leg
        out += [h[0], h[1], h[2]]
    for s in ('L', 'R'):
        out += list(k['elbow' + s])
    return out


FIELDS = 'time | pelvis x y z | pitch spine chest head roll lean | ankleL x y z | ankleR x y z | kneeL | kneeR | footL footR | handL | handR | elbowL | elbowR'


def table():
    lines = ['        // <getup-takes> (made by Tools/Mocap/Combat/getup_keys.py from the pack "Getting Up"; not written by hand)',
             '        // Each row a key: ' + FIELDS + '.',
             '        // Positions in leg lengths against where he ends up standing (x to his right, z the way he faces), his standing',
             '        // pose taken off: an ankle reads 0 0 0 on its standing place, the hips\' height is a share of his standing height.',
             '        // The pitch and lean are of the line from the hips to between the shoulders; the roll is about that line.']
    for name, (sample, start, end, about) in TAKES.items():
        t = read(sample, start, end)
        lines.append(f'        /// <summary>{about} ({sample}, {start:.1f} to {end:.1f} s of it). The hand on the ground last is his {"left" if t["post"] == 0 else "right"}; the foot that ends in front his {"left" if t["front"] == 0 else "right"}.</summary>')
        lines.append(f'        private const int Recorded{name}Post = {t["post"]}, Recorded{name}Front = {t["front"]};')
        lines.append(f'        private const float Recorded{name}Seconds = {t["seconds"]:.2f}f, Recorded{name}PlantedL = {t["planted"][0]:.2f}f, Recorded{name}PlantedR = {t["planted"][1]:.2f}f, Recorded{name}StandPitch = {t["stand"]["pitch"]:.1f}f;')
        lines.append(f'        private static readonly float[][] Recorded{name} =')
        lines.append('        {')
        for k in t['keys']:
            lines.append('            new[] { ' + ', '.join(f'{v:.3f}f' for v in row(k, t['stand'], t['leg'], t['planted'])) + ' },')
        lines.append('        };')
        lines.append('')
    lines.append('        // </getup-takes>')
    return '\n'.join(lines)


if __name__ == '__main__':
    if '--report' in sys.argv:
        for name, (sample, start, end, about) in TAKES.items():
            t = read(sample, start, end)
            print(name, about, 'post', t['post'], 'front', t['front'], 'planted', t['planted'], 'leg %.2f' % t['leg'])
            for k in t['keys']:
                r = row(k, t['stand'], t['leg'], t['planted'])
                print('  %.2f hips %+.2f %.2f %+.2f pitch %+4.0f spine %+3.0f chest %+3.0f head %+3.0f roll %+4.0f lean %+3.0f | aL %+.2f %.2f %+.2f aR %+.2f %.2f %+.2f | foot %+4.0f %+4.0f | hL %+.2f %.2f %+.2f hR %+.2f %.2f %+.2f' % tuple(r[:16] + r[22:24] + r[24:30]))
        sys.exit(0)
    src = open(TARGET, encoding='utf-8').read()
    a, b = src.find('        // <getup-takes>'), src.find('        // </getup-takes>')
    if a < 0 or b < 0:
        sys.exit('GetUp.cs has no <getup-takes> marks')
    nl = '\r\n' if '\r\n' in src else '\n'
    src = src[:a] + table().replace('\n', nl) + src[b + len('        // </getup-takes>'):]
    open(TARGET, 'w', encoding='utf-8', newline='').write(src)
    print('written into', TARGET)
