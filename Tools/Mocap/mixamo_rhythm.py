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


def foot_phases(feet, length, travel, leg):
    """Lift and land phases (0 to 1) of a foot over the loop, and its step height (legs)."""
    pos = np.array(feet)
    n = len(pos)
    dt = length / n
    nxt = np.vstack([pos[1:], pos[:1] + travel])
    speed = np.linalg.norm((nxt - pos)[:, [0, 2]], axis=1) / dt
    v = np.linalg.norm(travel) / length
    # In the air: off the ground (above its lowest by a fifth of its lift) or sliding along fast.
    lifted = pos[:, 1] - pos[:, 1].min() > 0.2 * (pos[:, 1].max() - pos[:, 1].min())
    moving = lifted & (speed > 0.25 * v)
    lift = land = None
    for i in range(n):
        if moving[i] and not moving[i - 1]:
            lift = i / n
        if not moving[i] and moving[i - 1]:
            land = i / n
    height = (pos[:, 1].max() - pos[:, 1].min()) / leg
    return lift, land, height


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('fbx')
    ap.add_argument('--name', default='Clip')
    ap.add_argument('--rate', type=float, default=20.0)
    ap.add_argument('--gait', action='store_true', help='a walk (with root motion): writes a GaitClip')
    ap.add_argument('--rig', help='a `rhythms` runner CSV of the unit: where to put its hands (needs --size)')
    ap.add_argument('--size', type=float, default=1.0, help="the rig's size factor")
    ap.add_argument('--out-cs')
    args = ap.parse_args()

    sk = Skeleton(args.fbx)
    times, rows = measure(sk, args.rate)
    w0 = rows[0]['w']
    L = lambda a, b: np.linalg.norm(w0[a][1] - w0[b][1])
    leg = L('LeftUpLeg', 'LeftLeg') + L('LeftLeg', 'LeftFoot') + w0['LeftFoot'][1][1]
    arm = L('LeftArm', 'LeftForeArm') + L('LeftForeArm', 'LeftHand')

    hip = np.array([r['hip'] for r in rows])
    # A walk travels: the hips are measured against a root moving evenly along the way.
    travel = np.zeros(3)
    if args.gait:
        end = sk.pose(sk.length)
        travel = 0.5 * (end['LeftUpLeg'][1] + end['RightUpLeg'][1]) - hip[0]
        travel[1] = 0.0
    v = np.linalg.norm(travel) / sk.length
    root = np.array([travel * (t / sk.length) for t in times])
    root += (hip - root).mean(0)
    root[:, 1] = 0.0
    lf_all = [r['feet'][0] for r in rows]
    rf_all = [r['feet'][1] for r in rows]
    lf = (np.array(lf_all) - root).mean(0)
    rf = (np.array(rf_all) - root).mean(0)
    pel = np.array([r['pelvis_yaw'] for r in rows]); che = np.array([r['chest_yaw'] for r in rows])
    lean = np.array([r['lean'] for r in rows]); roll = np.array([r['roll'] for r in rows])
    head = np.array([r['head'] for r in rows])
    hand = {s: np.array([r['hands'][s] for r in rows]) for s in ('Left', 'Right', 'LeftSh', 'RightSh')}

    print(f'{args.fbx}: {sk.length:.2f} s loop, {len(rows)} keys; leg {leg:.3f} m, arm {arm:.3f} m')
    if args.gait:
        print(f'walk: {np.linalg.norm(travel):.2f} m a loop ({np.linalg.norm(travel) / leg:.2f} legs), {v:.2f} m/s ({v / leg:.2f} legs/s)')
    print(f'stance: feet {abs(lf[0] - rf[0]) / leg:.2f} legs apart side to side, {abs(lf[2] - rf[2]) / leg:.2f} front to back '
          f'({"left" if lf[2] > rf[2] else "right"} foot leads); hips {(leg - hip[:, 1].max()) / leg:.3f} (highest) to '
          f'{(leg - hip[:, 1].min()) / leg:.3f} legs below straight')
    lead_weight = None
    if not args.gait:
        lead, rear = (lf, rf) if lf[2] > rf[2] else (rf, lf)
        ax = lead - rear; ax[1] = 0
        lead_weight = float(np.dot((hip - root).mean(0) - rear, ax) / np.dot(ax, ax))
        print(f'weight {lead_weight:.2f} on the lead foot')
    print(f'turns: pelvis {pel.mean():.1f}°, chest {che.mean():.1f}° (+ = to the right); spine lean {lean.mean():.1f}°, '
          f'head {head[:, 0].mean():.1f}° down against the chest')
    for s in ('Left', 'Right'):
        print(f'{s} hand against the chest {np.round(hand[s].mean(0) / leg, 3)} legs; against its shoulder '
              f'{np.round(hand[s + "Sh"].mean(0) / arm, 3)} arms')

    # The loop: offsets from the mean (the hips' drop from the loop's highest point).
    hrel = hip - root
    keys = []
    for i, r in enumerate(rows):
        d = (hrel[i] - hrel.mean(0)) / leg
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

    gait = None
    if args.gait:
        ll, ld, lh = foot_phases(lf_all, sk.length, travel, leg)
        rl, rd, rh = foot_phases(rf_all, sk.length, travel, leg)
        gait = dict(distance=np.linalg.norm(travel) / leg, width=abs(lf[0] - rf[0]) / leg, height=0.5 * (lh + rh),
                    lean=lean.mean(), crouch=(leg - hip[:, 1].max()) / leg, pelvis=pel.mean(), chest=che.mean(),
                    liftL=ll, landL=ld, liftR=rl, landR=rd)
        print('feet: left lifts at {liftL:.2f} and lands at {landL:.2f}, right {liftR:.2f} / {landR:.2f}; steps {height:.3f} legs high, '
              '{width:.2f} legs apart'.format(**gait))

    if args.rig:
        # Where the capture's hands go on this body: against each shoulder, in arm lengths, in the
        # chest's facing; and how far that is from where the unit's hands are now (its facing).
        import pandas as pd
        df = pd.read_csv(args.rig)
        m = df.mean(numeric_only=True)
        vv = lambda p: np.array([m[p + '.x'], m[p + '.y'], m[p + '.z']])
        across = vv('UpperArmL') - vv('UpperArmR')
        cyaw = math.degrees(math.atan2(across[2], -across[0]))
        print(f'rig: chest turned {cyaw:.1f}°')
        for s, tag in (('Left', 'L'), ('Right', 'R')):
            sh = turn_y(-cyaw, vv('UpperArm' + tag) - vv('Chest'))
            rig_arm = np.linalg.norm(vv('Forearm' + tag) - vv('UpperArm' + tag)) + np.linalg.norm(vv('Hand' + tag) - vv('Forearm' + tag))
            target = sh + hand[s + 'Sh'].mean(0) / arm * rig_arm
            now = turn_y(-cyaw, vv('Hand' + tag) - vv('Chest'))
            delta = turn_y(cyaw, target - now)
            print(f'  {s} hand: target {np.round(target, 3)} m against the chest (now {np.round(now, 3)}); move it by '
                  f'{np.round(delta, 3)} m in the unit\'s facing = {np.round(delta / args.size, 3)} at medium size')

    if args.out_cs:
        f = lambda x: f'{x:.4f}f'
        v3 = lambda t: f'new Vector3({f(t[0])}, {f(t[1])}, {f(t[2])})'
        lines = []
        for k in keys:
            lines.append(f'                K({v3(k["hips"])}, {f(k["drop"])}, {f(k["turn"])}, {f(k["hipTurn"])}, {f(k["lean"])}, {f(k["roll"])}, '
                         f'new Vector2({f(k["head"][0])}, {f(k["head"][1])}), {v3(k["main"])}, {v3(k["off"])}),')
        length = sk.length * math.sqrt(MEDIUM_LEG / leg)
        src = os.path.basename(args.fbx)
        if gait:
            body = f'''new RhythmClip
            {{
                source = "{src}",
                length = {f(length)},
                keys = new[]
                {{
{chr(10).join("    " + l for l in lines)}
                }},
            }}'''
            g = gait
            text = f'''// Generated by Tools/Mocap/mixamo_rhythm.py --gait from {src}: do not edit by hand.
using UnityEngine;

namespace PhysicsBattler.Motion
{{
    public static partial class RhythmClips
    {{
        /// <summary>{args.name}: a {sk.length:.2f} s walk cycle on a {leg:.2f} m leg ({length:.2f} s on the medium body), {g["distance"]:.2f} legs a cycle, {len(keys)} keys.</summary>
        public static GaitClip {args.name}() => new GaitClip
        {{
            source = "{src}",
            length = {f(length)},
            distance = {f(g["distance"])},
            width = {f(g["width"])},
            stepHeight = {f(g["height"])},
            lean = {f(g["lean"])},
            crouch = {f(g["crouch"])},
            pelvisTurn = {f(g["pelvis"])},
            chestTurn = {f(g["chest"])},
            liftL = {f(g["liftL"])},
            landL = {f(g["landL"])},
            liftR = {f(g["liftR"])},
            landR = {f(g["landR"])},
            body = {body},
        }};
    }}
}}
'''
        else:
            text = f'''// Generated by Tools/Mocap/mixamo_rhythm.py from {src}: do not edit by hand.
using UnityEngine;

namespace PhysicsBattler.Motion
{{
    public static partial class RhythmClips
    {{
        /// <summary>{args.name}: {sk.length:.2f} s loop on a {leg:.2f} m leg ({length:.2f} s on the medium body), {len(keys)} keys.</summary>
        public static RhythmClip {args.name}() => new RhythmClip
        {{
            source = "{src}",
            length = {f(length)},
            keys = new[]
            {{
{chr(10).join(lines)}
            }},
        }};
    }}
}}
'''
        open(args.out_cs, 'w').write(text)
        print('wrote', args.out_cs)


if __name__ == '__main__':
    main()
