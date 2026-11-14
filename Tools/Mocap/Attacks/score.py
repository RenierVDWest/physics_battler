"""
A strike written as a score: each of the body's tracks (body.py) as a few keys, with the curve between them.

A track's keys are found by putting one where the curve through the keys so far is furthest from the mocap, until it
is nowhere further than the track's tolerance. Between keys the curve is a monotone cubic (it never overshoots a key:
a planted foot stays planted). The score is what the game's attack is built from; python3 score.py prints how many
keys each strike needs and how far the body built from them is from the mocap.
"""
import sys
import numpy as np
from scipy.interpolate import PchipInterpolator
from attack_lib import Clip, STRIKES
import body

# How far a track's curve may be from the mocap: places in metres, turns in degrees, ways and hand turns as the length of the difference of unit vectors.
TOLERANCE = {'pelvis': 0.006, 'pelvis_turn': 1.5, 'chest_turn': 1.5, 'head_turn': 2.5, 'clav': 0.05, 'upper': 0.03, 'fore': 0.03, 'hand': 0.03, 'ankle': 0.006, 'knee': 0.06, 'foot': 0.05, 'toe': 0.08}
TRACKS = ['pelvis', 'pelvis_turn', 'chest_turn', 'head_turn'] + [s + '_' + k for s in ('l', 'r') for k in ('clav', 'upper', 'fore', 'hand', 'ankle', 'knee', 'foot', 'toe')]


def tolerance(track):
    return TOLERANCE[track] if track in TOLERANCE else TOLERANCE[track.split('_', 1)[1]]


def curve(tk, xk, t):
    """The track through its keys, at the times asked."""
    if len(tk) == 1:
        return np.repeat(xk[:1], len(t), axis=0)
    return PchipInterpolator(tk, xk, axis=0)(np.clip(t, tk[0], tk[-1]))


def keys_of(t, x, tol, must=()):
    """The frames to key a track at."""
    n = len(t)
    keys = sorted(set([0, n - 1] + [int(k) for k in must if 0 <= k < n]))
    while True:
        y = curve(t[keys], x[keys], t)
        e = np.linalg.norm(y - x, axis=1) if x.ndim == 2 else np.abs(y - x)
        worst = int(np.argmax(e))
        if e[worst] <= tol or len(keys) >= n:
            return keys
        keys = sorted(set(keys + [worst]))


def write(name, tolerance_scale=1.0):
    """A clip's score: {track: (times against contact, values)} and its marks."""
    c = Clip(name)
    T = body.decompose(c)
    t = T['t']
    score = {}
    for track in TRACKS:
        x = T[track]
        k = keys_of(t, x, tolerance(track) * tolerance_scale, must=(c.contact,))
        score[track] = (t[k].copy(), x[k].copy())
    return c, T, score


def play(score, t):
    """Tracks from a score, at the times asked."""
    T = {'t': t}
    for track, (tk, xk) in score.items():
        T[track] = curve(tk, xk, t)
    return T


if __name__ == '__main__':
    m, _ = body.measures()
    scale = float(sys.argv[1]) if len(sys.argv) > 1 else 1.0
    print(f"{'':20} frames  keys in all  a track's keys (least, middle, most)  | the body from the score against the mocap (cm): mean, worst frame, the blade's striking part mean and worst")
    for name in STRIKES:
        c, T, score = write(name, scale)
        W = body.compose(play(score, T['t']), m)
        e = np.linalg.norm(W - c.W, axis=2) * 100
        counts = sorted(len(v[0]) for v in score.values())
        ix = {b: i for i, b in enumerate(body.BONES)}
        def strike(P):
            wr = P[:, ix['RightHand']]
            across = P[:, ix['RightHandIndex1']] - P[:, ix['RightHandPinky1']]
            along = P[:, ix['RightHandMiddle1']] - wr
            b = body.unit(body.unit(across) * np.cos(np.radians(20)) + body.unit(along) * np.sin(np.radians(20)))
            return wr + b * 0.63
        se = np.linalg.norm(strike(W) - strike(c.W), axis=1) * 100
        print(f"{name:20} {c.n:5}  {sum(counts):6}       {counts[0]:3} {counts[len(counts) // 2]:3} {counts[-1]:3}                         | {e.mean():5.2f} {e.mean(axis=1).max():5.2f}   {se.mean():5.2f} {se.max():5.2f}")
