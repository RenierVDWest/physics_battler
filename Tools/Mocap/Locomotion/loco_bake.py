#!/usr/bin/env python3
"""Step 2 of the locomotion clips: turns the sample files (loco_sample.py) into the game's clip
library, Assets/_Battler/Resources/Locomotion/LocoClips.json.

    python3 Tools/Mocap/Locomotion/loco_bake.py [--report]

For each clip it works out, from the bones alone:
  - the root: a point on the ground under the body and the way the body faces. A loop's root
    goes straight at the loop's own speed and faces one way (where the head looks, on average);
    a one-shot's follows the hips' ground track and the head's facing, smoothed over 0.3 s;
  - every bone's place against that root (x right, y up, z forward), in leg lengths, so any body
    can be posed from it by its own leg length;
  - when each foot is down, heel and toe apart: low (within a few centimetres of where it rests)
    and nearly still, held at least 0.1 s;
  - for a loop, its gait phase: the left foot lands at 0, the right at 0.5, evened out between;
  - its speed, the way it travels against the way it faces, its stride and how far it turns.
Some one-shots are mirrored left to right too (the other foot first, the other way round).
"""
import json, math, os, sys
import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
ROOT = os.path.abspath(os.path.join(HERE, '..', '..', '..'))
OUT = os.path.join(ROOT, 'Assets', '_Battler', 'Resources', 'Locomotion', 'LocoClips.json')

LOOPS = {'idle', 'walk_fwd', 'walk_fwd_heavy', 'walk_fwd_swagger', 'walk_back', 'walk_back_2', 'walk_left_cross', 'walk_right_cross',
         'walk_left_step', 'walk_right_step', 'run_fwd', 'run_left', 'run_right'}
MIRROR = ['start_walk', 'stop_walk', 'turn180_walk', 'turn180_run', 'turn_to_run']
ROLE = {
    'idle': 'idle', 'start_walk': 'start', 'stop_walk': 'stop', 'turn180_walk': 'pivot', 'turn180_run': 'pivot',
    'turn90_left': 'turn', 'turn90_right': 'turn', 'turn180_left': 'turn', 'turn180_right': 'turn',
    'walkturn90_left': 'walkturn', 'walkturn90_right': 'walkturn', 'turn_to_run': 'start', 'sidestep_left': 'step', 'sidestep_right': 'step',
}

# Contacts: a heel (the ankle bone) or a toe (the ball of the foot) is down when it is within this
# much of its resting height and moving slower than this, for at least MIN_DOWN seconds.
HEEL_REST, TOE_REST = 0.087, 0.0
HEEL_UP, TOE_UP = 0.035, 0.03
STILL = 0.4
MIN_DOWN = 0.1


def yaw_of(v):
    return math.atan2(v[0], v[2])


def smooth(x, rate, seconds, loop=False):
    n = len(x)
    s = max(1e-6, seconds * rate / 2.0)
    k = np.arange(-int(3 * s), int(3 * s) + 1)
    w = np.exp(-0.5 * (k / s) ** 2)
    w /= w.sum()
    out = np.zeros_like(x)
    for i in range(n):
        idx = (i + k) % n if loop else np.clip(i + k, 0, n - 1)
        out[i] = np.tensordot(w, x[idx], axes=(0, 0))
    return out


def load(clip):
    with open(os.path.join(HERE, 'samples', clip + '.json')) as f:
        d = json.load(f)
    B = d['bones']
    F = np.array(d['frames'], float).reshape(len(d['frames']), len(B), 3)
    return d, B, F


def mirror(B, F):
    G = F.copy()
    G[:, :, 0] *= -1
    swap = {b: (b.replace('Left', 'Right') if b.startswith('Left') else b.replace('Right', 'Left')) for b in B}
    return np.stack([G[:, B.index(swap[b])] for b in B], axis=1)


def runs(flags, loop):
    """The (start, end) frame ranges where flags hold (end exclusive); a loop's run may wrap."""
    n = len(flags)
    out = []
    if loop and flags.all():
        return [(0, n)]
    start = None
    for i in range(n):
        if flags[i] and start is None:
            start = i
        if not flags[i] and start is not None:
            out.append((start, i))
            start = None
    if start is not None:
        if loop and out and out[0][0] == 0:
            first = out.pop(0)
            out.append((start, n + first[1]))
        else:
            out.append((start, n))
    return out


def clean(flags, rate, loop):
    """Contacts held at least MIN_DOWN; gaps shorter than that closed."""
    n = len(flags)
    f = flags.copy()
    least = max(1, int(round(MIN_DOWN * rate)))
    for a, b in runs(~f, loop):
        if b - a < least and not (not loop and (a == 0 or b >= n)):
            for i in range(a, b):
                f[i % n] = True
    for a, b in runs(f, loop):
        if b - a < least:
            for i in range(a, b):
                f[i % n] = False
    return f


def bake(clip, d, B, F, mirrored_of=None, yaw_from='head'):
    rate = d['rate']
    loop = clip in LOOPS
    ix = {b: i for i, b in enumerate(B)}
    if loop:
        F = F[:-1]          # (the last frame is the first again, a loop on)
    n = len(F)
    leg = float(np.mean(np.linalg.norm(F[:, ix['LeftUpLeg']] - F[:, ix['LeftLeg']], axis=1) + np.linalg.norm(F[:, ix['LeftLeg']] - F[:, ix['LeftFoot']], axis=1)))

    # Facing: where the head looks (eyes' line, square to it), flat.
    right = F[:, ix['RightEye']] - F[:, ix['LeftEye']]
    up = F[:, ix['HeadTop_End']] - F[:, ix['Head']]
    fwd = np.cross(right, up)
    head_yaw = np.unwrap(np.array([yaw_of(v) for v in fwd]))
    hips = F[:, ix['Hips']].copy()
    hips[:, 1] = 0.0
    if loop:
        c = math.atan2(np.mean(np.sin(head_yaw)), np.mean(np.cos(head_yaw)))
        yaw = np.full(n, c)
        # (A loop's root goes straight, at the loop's own speed: from its mean place, by its drift per loop.)
        drift = (F[-1, ix['Hips']] - F[0, ix['Hips']]) * n / max(1, n - 1)
        drift[1] = 0.0
        t = np.arange(n) / n
        mean_hips = np.mean(hips - np.outer(t, drift), axis=0)
        root = mean_hips + np.outer(t, drift)
    else:
        if yaw_from == 'hips':
            # (A strike's head looks about wildly; the body's turn is the hips' line's, kept at the
            # head's facing at the start: a side-on guard's hips stay turned against it.)
            hr = F[:, ix['RightUpLeg']] - F[:, ix['LeftUpLeg']]
            hips_yaw = np.unwrap(np.array([yaw_of(np.array([-v[2], 0.0, v[0]])) for v in hr]))
            head0 = math.atan2(np.mean(np.sin(head_yaw[:5])), np.mean(np.cos(head_yaw[:5])))
            head_yaw = hips_yaw + (head0 - hips_yaw[0])
        yaw = smooth(head_yaw, rate, 0.3)
        root = smooth(hips, rate, 0.4)
        drift = root[-1] - root[0]

    # Bones against the root, in leg lengths.
    P = np.zeros_like(F)
    for i in range(n):
        c, s = math.cos(-yaw[i]), math.sin(-yaw[i])
        rel = F[i] - root[i]
        P[i, :, 0] = (c * rel[:, 0] + s * rel[:, 2]) / leg
        P[i, :, 1] = F[i, :, 1] / leg
        P[i, :, 2] = (-s * rel[:, 0] + c * rel[:, 2]) / leg

    # Contacts.
    def speed(b):
        p = F[:, ix[b]]
        if loop:
            q = np.roll(p, -1, axis=0)
            q[-1] += drift
        else:
            q = np.vstack([p[1:], p[-1:]])
        v = q - p
        return np.linalg.norm(v[:, [0, 2]], axis=1) * rate

    contacts = np.zeros((n, 2), int)
    for k, side in enumerate(('Left', 'Right')):
        heel = clean((F[:, ix[side + 'Foot'], 1] < HEEL_REST + HEEL_UP) & (speed(side + 'Foot') < STILL), rate, loop)
        toe = clean((F[:, ix[side + 'ToeBase'], 1] < TOE_REST + TOE_UP) & (speed(side + 'ToeBase') < STILL), rate, loop)
        down = clean(heel | toe, rate, loop)
        contacts[:, k] = heel.astype(int) * 1 + toe.astype(int) * 2 + down.astype(int) * 4

    # The loop's phase: the left foot lands at 0, the right at 0.5.
    phase = None
    markers = []
    if loop and clip != 'idle':
        land = {}
        for k in (0, 1):
            down = (contacts[:, k] & 4) > 0
            land[k] = [a % n for a, b in runs(down, True)]
        if len(land[0]) == 1 and len(land[1]) == 1:
            l0, r0 = land[0][0], land[1][0]
            markers = [l0, r0]
            phase = np.zeros(n)
            span_lr = (r0 - l0) % n or n
            span_rl = (l0 - r0) % n or n
            for i in range(n):
                a = (i - l0) % n
                phase[i] = 0.5 * a / span_lr if a < span_lr else 0.5 + 0.5 * (a - span_lr) / span_rl
        else:
            print(f'  {clip}: {len(land[0])} left and {len(land[1])} right landings in the loop: no phase')

    duration = n / rate if loop else (n - 1) / rate
    travel = np.array([drift[0], drift[2]])
    c, s = math.cos(-yaw[0]), math.sin(-yaw[0])
    local = np.array([c * travel[0] + s * travel[1], -s * travel[0] + c * travel[1]])
    spd = float(np.linalg.norm(travel) / duration)
    direction = math.degrees(math.atan2(local[0], local[1])) if np.linalg.norm(travel) > 0.05 else 0.0
    turn = math.degrees(yaw[-1] - yaw[0])
    return dict(
        name=clip if mirrored_of is None else clip,
        mirrorOf=mirrored_of or '',
        role='cycle' if loop and clip != 'idle' else ROLE.get(clip.replace('_m', ''), 'other'),
        loop=loop, rate=float(rate), frames=n, legLength=round(leg, 4), duration=round(duration, 4),
        speed=round(spd / leg, 4), direction=round(direction, 2), turn=round(turn, 2),
        stride=round(spd * duration / leg, 4) if loop else 0.0,
        markers=markers,
        root=[round(float(v), 4) for i in range(n) for v in (root[i, 0] / leg, root[i, 2] / leg, math.degrees(yaw[i]))],
        points=[round(float(v), 4) for v in P.reshape(-1)],
        contacts=[int(v) for v in contacts.reshape(-1)],
        phase=[round(float(v), 4) for v in phase] if phase is not None else [],
        about=d['about'] + (' (mirrored)' if mirrored_of else ''),
    )


def main(report):
    clips = []
    names = sorted(f[:-5] for f in os.listdir(os.path.join(HERE, 'samples')) if f.endswith('.json'))
    bones = None
    for clip in names:
        d, B, F = load(clip)
        bones = B
        clips.append(bake(clip, d, B, F))
        if clip in MIRROR:
            clips.append(bake(clip + '_m', d, B, mirror(B, F), mirrored_of=clip))
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    with open(OUT, 'w') as f:
        json.dump(dict(about='Locomotion clips baked by Tools/Mocap/Locomotion/loco_bake.py; points in leg lengths against the root (x right, y up, z forward).',
                       bones=bones, clips=clips), f, separators=(',', ':'))
    print(f'{len(clips)} clips, {sum(c["frames"] for c in clips)} frames, {os.path.getsize(OUT) / 1e6:.2f} MB -> {OUT}')
    if report:
        for c in clips:
            con = np.array(c['contacts']).reshape(-1, 2)
            down = (con & 4) > 0
            print(f"{c['name']:18s} {c['role']:8s} {c['duration']:5.2f}s speed {c['speed'] * c['legLength']:4.2f} m/s dir {c['direction']:+6.1f} turn {c['turn']:+6.1f} "
                  f"stride {c['stride'] * c['legLength']:4.2f} m; down L {down[:, 0].mean() * 100:3.0f}% R {down[:, 1].mean() * 100:3.0f}% both {np.mean(down[:, 0] & down[:, 1]) * 100:3.0f}% none {np.mean(~down[:, 0] & ~down[:, 1]) * 100:3.0f}%; markers {c['markers']}")


if __name__ == '__main__':
    main('--report' in sys.argv)
