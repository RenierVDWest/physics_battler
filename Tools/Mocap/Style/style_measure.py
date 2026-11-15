#!/usr/bin/env python3
"""Measures of a movement style, read off sampled recordings (the plan "Movement Styles as Offshoots of the
Standard Set"): what a body does in a clip, in numbers that can be set beside the same numbers for another style.

    python3 Tools/Mocap/Style/style_measure.py <sample.json> [<sample.json> ...] [--feet]

The samples are those of Tools/Mocap/Combat/combat_sample.py and the locomotion's sampler (world positions at 30
frames a second, metres, Unity's axes). Every clip is read in its own frame: where it faces on average is forward.

For each clip, three kinds of measure.

Carriage (how the body is held, the average over the clip): the hips' height against the leg (1.00 standing
straight); how far apart the feet stand, across and along; how far the hips are turned off the way he faces and
the chest off the hips; the chest's lean forward; where each hand is held against the chest (right, up, forward).

Motion (how much it moves, about that average): how far the hips range side to side, back and forth and up and
down; how fast they go; how far the hips and the chest turn; how fast each hand goes; the beat (the strongest
period in the hips' motion).

Footwork (what the feet do): steps a second; how long a foot stands; the share of the time on both feet, on
one, in the air; how far a step goes; and with --feet, where each foot stands each time it is put down (against
the middle of the clip's hips, the frame above), so the pattern of the steps can be read.
"""
import json, math, sys
import numpy as np


def load(path):
    d = json.load(open(path))
    B = d['bones']
    F = np.array(d['frames'], float).reshape(len(d['frames']), len(B), 3)
    return d.get('clip', path.split('/')[-1].replace('.json', '')), float(d.get('rate', 30)), {b: i for i, b in enumerate(B)}, F


def yaw_of(v):
    return np.degrees(np.arctan2(v[..., 0], v[..., 2]))


def unwrap_deg(a):
    return np.degrees(np.unwrap(np.radians(a)))


def planted(F, ix, side, rate):
    """A foot is down while it is low and nearly still (its toe and its ankle both)."""
    toe, ankle = F[:, ix[side + 'ToeBase']], F[:, ix[side + 'Foot']]
    low = np.minimum(toe[:, 1], ankle[:, 1] - 0.06)
    rest = np.percentile(low, 5)
    speed = np.concatenate([[0], np.linalg.norm(np.diff(toe[:, [0, 2]], axis=0), axis=1) * rate])
    down = (low < rest + 0.04) & (speed < 0.5)
    # (Gaps and blips of under a tenth of a second are closed.)
    n, least = len(down), max(1, int(0.1 * rate))
    out = down.copy()
    i = 0
    while i < n:
        j = i
        while j < n and out[j] == out[i]:
            j += 1
        if j - i < least and i > 0 and j < n:
            out[i:j] = ~out[i]
        i = j
    return out


def measure(path, feet=False):
    name, rate, ix, F = load(path)
    n = len(F)
    hips = F[:, ix['Hips']]
    across = F[:, ix['RightUpLeg']] - F[:, ix['LeftUpLeg']]
    hips_yaw = unwrap_deg(yaw_of(np.stack([-across[:, 2], np.zeros(n), across[:, 0]], axis=1)))
    shoulders = F[:, ix['RightArm']] - F[:, ix['LeftArm']]
    chest_yaw = unwrap_deg(yaw_of(np.stack([-shoulders[:, 2], np.zeros(n), shoulders[:, 0]], axis=1)))
    # The clip's own frame: where the chest faces on average is forward; the travel is taken out (a straight line from start to end).
    facing = math.radians(float(np.mean(chest_yaw)))
    fwd = np.array([math.sin(facing), 0, math.cos(facing)])
    right = np.array([math.cos(facing), 0, -math.sin(facing)])
    drift = np.linspace(0, 1, n)[:, None] * (hips[-1] - hips[0]) * np.array([1, 0, 1])
    travel = float(np.linalg.norm((hips[-1] - hips[0])[[0, 2]])) / max(1e-3, (n - 1) / rate)
    def own(p):
        q = p - drift - (hips - drift).mean(axis=0) * np.array([1, 0, 1])
        return np.stack([q @ right, q[:, 1], q @ fwd], axis=1)
    H = own(hips)
    leg = float(np.max(np.linalg.norm(F[:, ix['LeftUpLeg']] - F[:, ix['LeftLeg']], axis=1) + np.linalg.norm(F[:, ix['LeftLeg']] - F[:, ix['LeftFoot']], axis=1)))
    ankle_rest = float(np.percentile(F[:, ix['LeftFoot'], 1], 5))
    standing = leg + ankle_rest + float(np.mean(hips[:, 1] - F[:, ix['LeftUpLeg'], 1]))
    L, R = own(F[:, ix['LeftFoot']]), own(F[:, ix['RightFoot']])
    dl, dr = planted(F, ix, 'Left', rate), planted(F, ix, 'Right', rate)
    both = dl & dr
    chest = F[:, ix['Spine2']]
    spine = chest - hips
    lean = np.degrees(np.arctan2(spine @ fwd, spine[:, 1]))
    def hand(side):
        h = F[:, ix[side + 'Hand']] - chest
        o = np.stack([h @ right, h[:, 1], h @ fwd], axis=1)
        chest_frame = np.radians(chest_yaw - math.degrees(facing))
        # (Against the chest as it is turned at each moment.)
        x = o[:, 0] * np.cos(chest_frame) - o[:, 2] * np.sin(chest_frame)
        z = o[:, 0] * np.sin(chest_frame) + o[:, 2] * np.cos(chest_frame)
        speed = np.linalg.norm(np.diff(h, axis=0), axis=1) * rate
        return np.array([x.mean(), o[:, 1].mean(), z.mean()]), float(np.median(speed)), float(np.percentile(speed, 95))
    hl, hls, hlp = hand('Left')
    hr, hrs, hrp = hand('Right')
    hs = np.linalg.norm(np.diff(H[:, [0, 2]], axis=0), axis=1) * rate
    # The beat: the strongest period of the hips' side-to-side motion (its autocorrelation's first peak past a quarter second).
    x = H[:, 0] - H[:, 0].mean()
    beat = 0.0
    if x.std() > 0.01 and n > rate:
        ac = np.correlate(x, x, 'full')[n - 1:]
        ac /= ac[0]
        lo = int(0.25 * rate)
        peaks = [i for i in range(lo + 1, len(ac) - 1) if ac[i] > ac[i - 1] and ac[i] >= ac[i + 1] and ac[i] > 0.2]
        beat = peaks[0] / rate if peaks else 0.0
    # Steps: each time a foot is put down; how far from where it last stood; how long it then stands.
    steps, stands, places = [], [], []
    for side, P, d in (('L', L, dl), ('R', R, dr)):
        last, start = None, None
        for f in range(n):
            if d[f] and (f == 0 or not d[f - 1]):
                start = f
            if d[f] and (f == n - 1 or not d[f + 1]) and start is not None:
                at = P[start:f + 1].mean(axis=0)
                if f - start + 1 >= int(0.1 * rate):
                    if last is not None:
                        steps.append(float(np.linalg.norm((at - last)[[0, 2]])))
                    stands.append((f - start + 1) / rate)
                    places.append((side, start / rate, (f - start + 1) / rate, at[0], at[2]))
                    last = at
    seconds = (n - 1) / rate
    m = dict(name=name, seconds=seconds, travel=travel,
             hips_h=(float(H[:, 1].min() / standing), float(H[:, 1].mean() / standing), float(H[:, 1].max() / standing)),
             width=float(np.mean(np.abs(R[both, 0] - L[both, 0]))) if both.any() else float('nan'),
             along=float(np.mean(np.abs(R[both, 2] - L[both, 2]))) if both.any() else float('nan'),
             hips_yaw=float(np.mean(hips_yaw) - math.degrees(facing)), hips_turn=float(np.ptp(hips_yaw)),
             twist=float(np.mean(chest_yaw - hips_yaw)), twist_range=float(np.ptp(chest_yaw - hips_yaw)), chest_turn=float(np.ptp(chest_yaw)),
             lean=float(lean.mean()), lean_range=float(np.ptp(lean)),
             sway=(float(np.ptp(H[:, 0])), float(np.ptp(H[:, 2])), float(np.ptp(H[:, 1]))), hips_speed=(float(np.median(hs)), float(np.percentile(hs, 95))),
             hand_l=hl, hand_r=hr, hand_speed=(hls, hlp, hrs, hrp), beat=beat,
             steps_s=len(steps) / max(0.1, seconds), step=float(np.median(steps)) if steps else 0.0, step_most=float(max(steps)) if steps else 0.0,
             stand=float(np.median(stands)) if stands else seconds,
             both=float(np.mean(both)), one=float(np.mean(dl ^ dr)), air=float(np.mean(~dl & ~dr)), places=sorted(places, key=lambda p: p[1]))
    return m


def show(ms, feet):
    print("CARRIAGE (the average over the clip)")
    print(f"{'clip':30} {'hips':>5} {'feet apart':>11} {'hips off':>8} {'twist':>6} {'lean':>5}   left hand (r,u,f)    right hand (r,u,f)")
    print(f"{'':30} {'of leg':>5} {'across/along':>11} {'facing':>8} {'':>6} {'':>5}   against the chest, m")
    for m in ms:
        print(f"{m['name'][:30]:30} {m['hips_h'][1]:5.2f} {m['width']:5.2f}/{m['along']:4.2f} {m['hips_yaw']:8.0f} {m['twist']:6.0f} {m['lean']:5.0f}   "
              f"{m['hand_l'][0]:5.2f} {m['hand_l'][1]:5.2f} {m['hand_l'][2]:5.2f}   {m['hand_r'][0]:5.2f} {m['hand_r'][1]:5.2f} {m['hand_r'][2]:5.2f}")
    print("\nMOTION (about that average)")
    print(f"{'clip':30} {'travel':>6} {'hips range, m':>18} {'hips m/s':>10} {'hips':>5} {'chest':>5} {'lean':>5} {'hands m/s (mid, 95%)':>24} {'beat':>5}")
    print(f"{'':30} {'m/s':>6} {'side  fore   up':>18} {'mid  95%':>10} {'turn':>5} {'turn':>5} {'range':>5} {'left        right':>24} {'s':>5}")
    for m in ms:
        print(f"{m['name'][:30]:30} {m['travel']:6.2f} {m['sway'][0]:5.2f} {m['sway'][1]:5.2f} {m['sway'][2]:5.2f}  {m['hips_speed'][0]:4.2f} {m['hips_speed'][1]:4.2f} {m['hips_turn']:5.0f} {m['chest_turn']:5.0f} {m['lean_range']:5.0f}   "
              f"{m['hand_speed'][0]:4.2f} {m['hand_speed'][1]:4.2f}   {m['hand_speed'][2]:4.2f} {m['hand_speed'][3]:4.2f} {m['beat']:5.2f}")
    print("\nFOOTWORK")
    print(f"{'clip':30} {'steps/s':>7} {'step m (mid, most)':>18} {'stands s':>8} {'both':>5} {'one':>5} {'air':>5} {'hips low..high':>15}")
    for m in ms:
        print(f"{m['name'][:30]:30} {m['steps_s']:7.2f} {m['step']:8.2f} {m['step_most']:8.2f} {m['stand']:8.2f} {100 * m['both']:4.0f}% {100 * m['one']:4.0f}% {100 * m['air']:4.0f}% {m['hips_h'][0]:6.2f}..{m['hips_h'][2]:4.2f}")
    if feet:
        for m in ms:
            print(f"\n{m['name']}: where each foot is put down (s, for how long; right and forward of the hips' middle, m)")
            for side, at, long, x, z in m['places']:
                print(f"  {at:5.2f} s  {side}  stands {long:4.2f} s  at {x:5.2f} {z:5.2f}")


if __name__ == '__main__':
    args = [a for a in sys.argv[1:] if not a.startswith('--')]
    show([measure(a) for a in args], '--feet' in sys.argv)
