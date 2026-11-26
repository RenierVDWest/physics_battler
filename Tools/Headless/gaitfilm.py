#!/usr/bin/env python3
"""A headless film (FIGHT_FILM="file.json,from,to") read as a gait, for one unit or all:

    python3 Tools/Headless/gaitfilm.py file.json [unit name part]

While he walks (the walk's own "Cycle"): how fast his body goes, how fast the walk asked, the tempo and the stride against
the recording's, his footfalls a minute and his step's length, how much his hips rise and fall in a step and when in the
step they are lowest, how even his hips' speed over the ground is, how long both feet, one foot or none is held, and how
often what he does changes. Written 26 November 2026 for the plan "Walking that feels like a real person": the numbers
that say floating (slowed tempo, short strides, level hips, long holds) next to what people do at the same speed.
"""
import json, sys, collections
import numpy as np

P = {'Pelvis': 0, 'Chest': 2, 'Head': 3, 'FootL': 12, 'FootR': 15, 'ToeL': 19, 'ToeR': 20}


def pt(f, name):
    i = P[name] * 3
    return np.array(f['p'][i:i + 3])


def runs(mask):
    out, s = [], None
    for i, m in enumerate(mask):
        if m and s is None:
            s = i
        if not m and s is not None:
            out.append((s, i)); s = None
    if s is not None:
        out.append((s, len(mask)))
    return out


def unit(frames, name):
    fr = [f for f in frames if f['u'] == name]
    if len(fr) < 10:
        return
    t = np.array([f['t'] for f in fr]); dt = float(np.median(np.diff(t)))
    doing = [f['doing'].split()[0] for f in fr]
    walk = np.array([d == 'Cycle' for d in doing])
    pel = np.array([pt(f, 'Pelvis') for f in fr])
    root = np.array([[f['root'][0], f['root'][1]] for f in fr])
    locks = np.array([f['d'] for f in fr])
    spd = np.array([f['spd'] for f in fr]); want = np.array([f['want'] for f in fr])
    cad = np.array([f['cad'] for f in fr]); ss = np.array([f['ss'] for f in fr])
    feet = {0: np.array([pt(f, 'FootL') for f in fr]), 1: np.array([pt(f, 'FootR') for f in fr])}
    total = t[-1] - t[0]
    print(f"== {name}: {total:.0f} s filmed; walking {walk.mean()*100:.0f}% of it, standing {np.mean([d=='Idle' for d in doing])*100:.0f}%, a one-off motion {np.mean([d=='OneShot' for d in doing])*100:.0f}%")
    spells = runs(walk)
    lens = [(b - a) * dt for a, b in spells]
    changes = sum(1 for i in range(1, len(doing)) if doing[i] != doing[i - 1])
    print(f"   spells of walking: {len(spells)}, median {np.median(lens) if lens else 0:.2f} s, shortest quarter under {np.percentile(lens,25) if lens else 0:.2f} s; what he does changes {changes/total:.1f} times a second")
    if walk.sum() < 10:
        return
    v = np.linalg.norm(np.diff(root, axis=0), axis=1) / dt
    v = np.concatenate([v, v[-1:]])
    w = walk & (spd > 0.05)
    print(f"   while walking: body speed median {np.median(v[w]):.2f} m/s (middle half {np.percentile(v[w],25):.2f} to {np.percentile(v[w],75):.2f}); asked {np.median(want[w]):.2f}; under 0.9 m/s for {np.mean(v[w]<0.9)*100:.0f}% of it, under 0.6 for {np.mean(v[w]<0.6)*100:.0f}%")
    print(f"   tempo against the recording: median {np.median(cad[w]):.2f} (middle half {np.percentile(cad[w],25):.2f} to {np.percentile(cad[w],75):.2f}); the same fall then looks like gravity at {np.median(cad[w])**2*100:.0f}% (tempo squared)")
    print(f"   stride against the recording: median {np.median(ss[w]):.2f} (middle half {np.percentile(ss[w],25):.2f} to {np.percentile(ss[w],75):.2f}); under 0.8 for {np.mean(ss[w]<0.8)*100:.0f}% of the walking")
    # footfalls: a lock begins
    land = []
    for k in (0, 1):
        for i in range(1, len(fr)):
            if locks[i, k] and not locks[i - 1, k] and walk[i]:
                land.append((i, k))
    land.sort()
    steps = []
    for (i0, k0), (i1, k1) in zip(land, land[1:]):
        if k0 == k1 or (i1 - i0) * dt > 1.6 or not walk[i0:i1].all():
            continue
        T = (i1 - i0) * dt
        L = float(np.linalg.norm((feet[k1][i1] - feet[k0][i0])[[0, 2]]))
        y = pel[i0:i1, 1]
        bob = float(y.max() - y.min())
        low_at = float(np.argmin(y)) / max(1, (i1 - i0))
        hv = np.linalg.norm(np.diff(pel[i0:i1][:, [0, 2]], axis=0), axis=1) / dt
        if len(hv) > 6:
            k = np.ones(5) / 5; hv = np.convolve(hv, k, mode='valid')
        ripple = float((hv.max() - hv.min()) / max(1e-6, hv.mean())) if len(hv) else 0.0
        steps.append((T, L, bob, low_at, ripple, float(hv.mean()) if len(hv) else 0.0))
    if steps:
        s = np.array(steps)
        print(f"   {len(s)} steps from one footfall to the other foot's: {60/np.median(s[:,0]):.0f} footfalls a minute (step time median {np.median(s[:,0]):.2f} s), foot to foot {np.median(s[:,1])*100:.0f} cm apart on the ground (middle half {np.percentile(s[:,1],25)*100:.0f} to {np.percentile(s[:,1],75)*100:.0f})")
        print(f"   hips rise and fall {np.median(s[:,2])*100:.1f} cm in a step (middle half {np.percentile(s[:,2],25)*100:.1f} to {np.percentile(s[:,2],75)*100:.1f}); lowest {np.median(s[:,3])*100:.0f}% of the way through the step; their speed over the ground varies {np.median(s[:,4])*100:.0f}% of its mean within a step")
    both = (locks[:, 0] == 1) & (locks[:, 1] == 1); none = (locks[:, 0] == 0) & (locks[:, 1] == 0)
    print(f"   feet held while walking: both {both[w].mean()*100:.0f}% of the time, one {(~both[w] & ~none[w]).mean()*100:.0f}%, none {none[w].mean()*100:.0f}%")
    blends = collections.Counter()
    for f, m in zip(fr, w):
        if m:
            top = max((b.split(':') for b in f['blend'].split(';') if b), key=lambda x: float(x[1]))
            blends[top[0]] += 1
    tot = sum(blends.values())
    print("   recordings leading his walk: " + ", ".join(f"{n} {c/tot*100:.0f}%" for n, c in blends.most_common(8)))
    st = ~walk
    if st.sum() > 30:
        y = pel[st, 1]
        print(f"   standing: hips at {np.median(y)*100:.0f} cm; walking: {np.median(pel[w,1])*100:.0f} cm")


if __name__ == '__main__':
    frames = json.load(open(sys.argv[1]))
    names = sorted(set(f['u'] for f in frames))
    for n in names:
        if len(sys.argv) < 3 or sys.argv[2].lower() in n.lower():
            unit(frames, n)
