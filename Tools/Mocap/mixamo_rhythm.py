#!/usr/bin/env python3
"""Turns a looping Mixamo idle (binary FBX, skeleton animation) into a Fight Rhythm base clip.

    python3 Tools/Mocap/mixamo_rhythm.py Tools/Mocap/Boxing.fbx --name Boxing \
        --out-cs Assets/_Battler/Scripts/Runtime/Motion/RhythmClips.Boxing.g.cs [--rig large_Base.csv]

It reads the skeleton's animation, poses it at 20 keys a second over the loop and measures, in
Unity's axes (the FBX's x mirrored) and the body's own facing:
  - the stance: feet apart side to side and front to back, how low the hips sit, how far the hips
    and the chest are turned, how far the spine leans (leg lengths, degrees);
  - the loop, as offsets from the loop's mean: the hips (leg lengths; the drop down from the
    loop's highest point), the pelvis and chest turns, the lean and side roll, the head's nod and
    turn against the chest, and both hands against the chest (in the chest's facing);
  - the hands' mean places, measured against the shoulder in arm lengths, so they can be put on
    a body with other proportions (--rig: a `rhythms` runner CSV of the unit, for its shoulders).
It writes the clip as C# (Motion/RhythmClips.g.cs) for RhythmClips, and prints the stance.
"""
import argparse, math, os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import fbx_binary

TICK = 46186158000
MEDIUM_LEG = 0.85      # metres: "size units" are metres on the medium body


def euler_xyz(r):
    x, y, z = [math.radians(a) for a in r]
    rx = np.array([[1, 0, 0], [0, math.cos(x), -math.sin(x)], [0, math.sin(x), math.cos(x)]])
    ry = np.array([[math.cos(y), 0, math.sin(y)], [0, 1, 0], [-math.sin(y), 0, math.cos(y)]])
    rz = np.array([[math.cos(z), -math.sin(z), 0], [math.sin(z), math.cos(z), 0], [0, 0, 1]])
    return rz @ ry @ rx


class Skeleton:
    def __init__(self, path):
        _, root = fbx_binary.read(path)
        objs = root.first('Objects')
        byid = {o.props[0]: o for o in objs.children}
        self.models = {i: o for i, o in byid.items() if o.name == 'Model'}
        self.parent, cnodes, curves = {}, {}, {}
        for c in root.first('Connections').children:
            src, dst = c.props[1], c.props[2]
            s, d = byid.get(src), byid.get(dst)
            if s is None or d is None:
                continue
            if s.name == 'Model' and d.name == 'Model':
                self.parent[src] = dst
            elif s.name == 'AnimationCurveNode' and d.name == 'Model':
                cnodes[(dst, c.props[3])] = src
            elif s.name == 'AnimationCurve' and d.name == 'AnimationCurveNode':
                curves[(dst, c.props[3])] = src
        self.info, self.anim, self.length = {}, {}, 0.0
        for i, m in self.models.items():
            p = {q.props[0]: q.props[4:] for q in m.first('Properties70').children} if m.first('Properties70') else {}
            if int(p.get('RotationOrder', [0])[0]) != 0:
                raise SystemExit('only XYZ rotation order is supported')
            name = m.props[1].split('\x00')[0].split(':')[-1]
            self.info[i] = dict(name=name, T=np.array(p.get('Lcl Translation', [0, 0, 0]), float),
                                R=np.array(p.get('Lcl Rotation', [0, 0, 0]), float),
                                pre=euler_xyz(p.get('PreRotation', [0, 0, 0])), post=euler_xyz(p.get('PostRotation', [0, 0, 0])))
        for (mid, prop), cn in cnodes.items():
            d = {}
            for ax in 'XYZ':
                k = curves.get((cn, 'd|' + ax))
                if k:
                    c = byid[k]
                    t = np.array(c.first('KeyTime').props[0], float) / TICK
                    d[ax] = (t, np.array(c.first('KeyValueFloat').props[0], float))
                    self.length = max(self.length, t[-1])
            self.anim[(mid, prop)] = d

    def _value(self, mid, prop, base, t):
        d = self.anim.get((mid, prop))
        out = base.copy()
        for i, ax in enumerate('XYZ'):
            if d and ax in d:
                out[i] = np.interp(t, *d[ax])
        return out

    def pose(self, t):
        """World (rotation, position) of every bone at time t, in Unity's axes (x mirrored), metres."""
        world = {}

        def get(mid):
            if mid not in world:
                inf = self.info[mid]
                tr = self._value(mid, 'Lcl Translation', inf['T'], t)
                rot = inf['pre'] @ euler_xyz(self._value(mid, 'Lcl Rotation', inf['R'], t)) @ inf['post'].T
                if mid in self.parent:
                    pr, pt = get(self.parent[mid])
                    world[mid] = (pr @ rot, pt + pr @ tr)
                else:
                    world[mid] = (rot, tr)
            return world[mid]

        s = np.diag([-1.0, 1.0, 1.0])
        out = {}
        for mid in self.info:
            r, p = get(mid)
            out[self.info[mid]['name']] = (s @ r @ s, s @ p / 100.0)
        return out


def yaw_of(v):
    return math.degrees(math.atan2(v[0], v[2]))


def turn_y(deg, v):
    a = math.radians(deg)
    c, s = math.cos(a), math.sin(a)
    # Unity's Euler(0, deg, 0): forward (0,0,1) -> (sin, 0, cos)
    return np.array([c * v[0] + s * v[2], v[1], -s * v[0] + c * v[2]])


def measure(sk, rate):
    n = int(round(sk.length * rate))
    times = np.arange(n) * sk.length / n
    rows = []
    for t in times:
        w = sk.pose(t)
        P = lambda b: w[b][1]
        fwd = lambda b: w[b][0] @ np.array([0, 0, 1.0])
        hip = 0.5 * (P('LeftUpLeg') + P('RightUpLeg'))
        pelvis_yaw = yaw_of(fwd('Hips'))
        chest_yaw = yaw_of(fwd('Spine2'))
        spine = P('Spine2') - P('Hips')
        lean = math.degrees(math.atan2(spine[2], spine[1]))
        roll = math.degrees(math.atan2(spine[0], spine[1]))       # + = toward the right
        hf, cf = fwd('Head'), fwd('Spine2')
        head_pitch = -math.degrees(math.asin(np.clip(hf[1], -1, 1))) + math.degrees(math.asin(np.clip(cf[1], -1, 1)))
        head_yaw = yaw_of(hf) - chest_yaw
        hands = {}
        for side in ('Left', 'Right'):
            # against the chest, in the chest's facing (x right, y up, z forward)
            hands[side] = turn_y(-chest_yaw, P(side + 'Hand') - P('Spine2'))
            hands[side + 'Sh'] = turn_y(-chest_yaw, P(side + 'Hand') - P(side + 'Arm'))
        rows.append(dict(t=t, hip=hip, pelvis_yaw=pelvis_yaw, chest_yaw=chest_yaw, lean=lean, roll=roll,
                         head=(head_pitch, head_yaw), hands=hands,
                         feet=(P('LeftFoot'), P('RightFoot')), w=w))
    return times, rows


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('fbx')
    ap.add_argument('--name', default='Clip')
    ap.add_argument('--rate', type=float, default=20.0)
    ap.add_argument('--rig', help='a `rhythms` runner CSV of the unit (its first frame gives its shoulders)')
    ap.add_argument('--out-cs')
    args = ap.parse_args()

    sk = Skeleton(args.fbx)
    times, rows = measure(sk, args.rate)
    w0 = rows[0]['w']
    L = lambda a, b: np.linalg.norm(w0[a][1] - w0[b][1])
    leg = L('LeftUpLeg', 'LeftLeg') + L('LeftLeg', 'LeftFoot') + w0['LeftFoot'][1][1]
    arm = L('LeftArm', 'LeftForeArm') + L('LeftForeArm', 'LeftHand')

    hip = np.array([r['hip'] for r in rows])
    lf = np.array([r['feet'][0] for r in rows]).mean(0)
    rf = np.array([r['feet'][1] for r in rows]).mean(0)
    pel = np.array([r['pelvis_yaw'] for r in rows]); che = np.array([r['chest_yaw'] for r in rows])
    lean = np.array([r['lean'] for r in rows]); roll = np.array([r['roll'] for r in rows])
    head = np.array([r['head'] for r in rows])
    hand = {s: np.array([r['hands'][s] for r in rows]) for s in ('Left', 'Right', 'LeftSh', 'RightSh')}

    print(f'{args.fbx}: {sk.length:.2f} s loop, {len(rows)} keys; leg {leg:.3f} m, arm {arm:.3f} m')
    print(f'stance: feet {abs(lf[0] - rf[0]) / leg:.2f} legs apart side to side, {abs(lf[2] - rf[2]) / leg:.2f} front to back '
          f'({"left" if lf[2] > rf[2] else "right"} foot leads); hips {(leg - hip[:, 1].max()) / leg:.3f} (highest) to '
          f'{(leg - hip[:, 1].min()) / leg:.3f} legs below straight')
    print(f'turns: pelvis {pel.mean():.1f}°, chest {che.mean():.1f}° (+ = to the right); spine lean {lean.mean():.1f}°, '
          f'head {head[:, 0].mean():.1f}° down against the chest')
    for s in ('Left', 'Right'):
        print(f'{s} hand against the chest {np.round(hand[s].mean(0) / leg, 3)} legs; against its shoulder '
              f'{np.round(hand[s + "Sh"].mean(0) / arm, 3)} arms')

    # The loop: offsets from the mean (the hips' drop from the loop's highest point).
    mid = 0.5 * (lf + rf)
    keys = []
    for i, r in enumerate(rows):
        d = (r['hip'] - hip.mean(0)) / leg
        k = dict(hips=(d[0], 0.0, d[2]), drop=(hip[:, 1].max() - r['hip'][1]) / leg,
                 turn=che[i] - che.mean(), hipTurn=(pel[i] - pel.mean()) - 0.45 * (che[i] - che.mean()),
                 lean=lean[i] - lean.mean(), roll=roll[i] - roll.mean(),
                 head=tuple(head[i] - head.mean(0)),
                 main=tuple((hand['Right'][i] - hand['Right'].mean(0)) / leg * MEDIUM_LEG),
                 off=tuple((hand['Left'][i] - hand['Left'].mean(0)) / leg * MEDIUM_LEG))
        keys.append(k)
    rng = lambda f: max(abs(f(k)) for k in keys)
    print(f'loop: hips ±{rng(lambda k: k["hips"][0]):.3f} side, ±{rng(lambda k: k["hips"][2]):.3f} fore and aft, '
          f'drop up to {rng(lambda k: k["drop"]):.3f} legs; turn ±{rng(lambda k: k["turn"]):.1f}°, lean ±{rng(lambda k: k["lean"]):.1f}°')

    off_guard = None
    if args.rig:
        import csv
        row = next(csv.DictReader(open(args.rig)))
        v = lambda p: np.array([float(row[p + '.' + a]) for a in 'xyz'])
        across = v('UpperArmL') - v('UpperArmR')
        cyaw = math.degrees(math.atan2(across[2], -across[0]))
        sh = turn_y(-cyaw, v('UpperArmL') - v('Chest'))
        rig_arm = np.linalg.norm(v('ForearmL') - v('UpperArmL')) + np.linalg.norm(v('HandL') - v('ForearmL'))
        size = float(os.environ.get('RIG_SIZE', '1.235'))
        target = sh + hand['LeftSh'].mean(0) / arm * rig_arm
        off_guard = target / size
        print(f'rig: shoulder {np.round(sh, 3)} against its chest, arm {rig_arm:.3f} m -> bare hand at {np.round(target, 3)} m '
              f'(offGuard {np.round(off_guard, 3)} at medium size)')

    if args.out_cs:
        f = lambda x: f'{x:.4f}f'
        v3 = lambda t: f'new Vector3({f(t[0])}, {f(t[1])}, {f(t[2])})'
        lines = []
        for k in keys:
            lines.append(f'                K({v3(k["hips"])}, {f(k["drop"])}, {f(k["turn"])}, {f(k["hipTurn"])}, {f(k["lean"])}, {f(k["roll"])}, '
                         f'new Vector2({f(k["head"][0])}, {f(k["head"][1])}), {v3(k["main"])}, {v3(k["off"])}),')
        length = sk.length * math.sqrt(MEDIUM_LEG / leg)
        src = f'''// Generated by Tools/Mocap/mixamo_rhythm.py from {os.path.basename(args.fbx)}: do not edit by hand.
using UnityEngine;

namespace PhysicsBattler.Motion
{{
    public static partial class RhythmClips
    {{
        /// <summary>{args.name}: {sk.length:.2f} s loop on a {leg:.2f} m leg ({length:.2f} s on the medium body), {len(keys)} keys.</summary>
        public static RhythmClip {args.name}() => new RhythmClip
        {{
            source = "{os.path.basename(args.fbx)}",
            length = {f(length)},
            keys = new[]
            {{
{chr(10).join(lines)}
            }},
        }};
    }}
}}
'''
        open(args.out_cs, 'w').write(src)
        print('wrote', args.out_cs)


if __name__ == '__main__':
    main()
