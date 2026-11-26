#!/usr/bin/env python3
"""A film (FIGHT_FILM, the "match" command's, or the walk lab's `locolab.json`) drawn as a strip of stick figures, to look at a walk before he does.

    python3 Tools/Headless/filmstrip.py film.json out.png [from] [to] [every] [unit name part]

Each figure is the body at one moment, seen from the side of the way he travels over the stretch (or from his own side
when he does not travel), set down where he was along that way. Left limbs are drawn dark, right limbs light; a foot the
film has down gets a mark under it. A second row shows the same stretch from above (his hips' path and both feet).
"""
import json, sys
import numpy as np
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt

N = ['Pelvis', 'Spine', 'Chest', 'Head', 'UpperArmL', 'ForearmL', 'HandL', 'UpperArmR', 'ForearmR', 'HandR', 'ThighL', 'ShinL', 'FootL',
     'ThighR', 'ShinR', 'FootR', 'HeadTop', 'HandEndL', 'HandEndR', 'ToeL', 'ToeR']
I = {n: i for i, n in enumerate(N)}
MID = [('Pelvis', 'Spine'), ('Spine', 'Chest'), ('Chest', 'Head'), ('Head', 'HeadTop')]
LEFT = [('Chest', 'UpperArmL'), ('UpperArmL', 'ForearmL'), ('ForearmL', 'HandL'), ('Pelvis', 'ThighL'), ('ThighL', 'ShinL'), ('ShinL', 'FootL'), ('FootL', 'ToeL')]
RIGHT = [('Chest', 'UpperArmR'), ('UpperArmR', 'ForearmR'), ('ForearmR', 'HandR'), ('Pelvis', 'ThighR'), ('ThighR', 'ShinR'), ('ShinR', 'FootR'), ('FootR', 'ToeR')]


def main():
    film, out = sys.argv[1], sys.argv[2]
    t0 = float(sys.argv[3]) if len(sys.argv) > 3 else None
    t1 = float(sys.argv[4]) if len(sys.argv) > 4 else None
    every = float(sys.argv[5]) if len(sys.argv) > 5 else 0.1
    who = sys.argv[6] if len(sys.argv) > 6 else None
    fr = json.load(open(film))
    if isinstance(fr, dict):
        # The walk lab's film (`locolab`): one man, his parts by name, the feet his locks hold.
        alias = {'HeadTop': 'Head', 'HandEndL': 'HandTipL', 'HandEndR': 'HandTipR'}
        fr = [{'u': 'walk lab', 't': f['t'], 'p': [c for n in N for c in f['pose'][alias.get(n, n) if n not in f['pose'] else n]],
               'd': [1 if f['locks'][0] else 0, 1 if f['locks'][1] else 0], 'root': [f['root'][0], f['root'][2], f['facing']],
               'blend': f['leg']} for f in fr['frames']]
    names = sorted(set(f['u'] for f in fr))
    name = next((n for n in names if who and who.lower() in n.lower()), names[0])
    fr = [f for f in fr if f['u'] == name and (t0 is None or f['t'] >= t0) and (t1 is None or f['t'] <= t1)]
    if len(fr) < 2:
        print('nothing in that stretch'); return
    P = np.array([f['p'] for f in fr]).reshape(len(fr), -1, 3)
    t = np.array([f['t'] for f in fr])
    pel = P[:, 0]
    way = (pel[-1] - pel[0])[[0, 2]]
    if np.linalg.norm(way) < 0.3:
        yaw = np.radians(fr[0]['root'][2]); way = np.array([np.sin(yaw), np.cos(yaw)])
    way = way / np.linalg.norm(way)
    along = lambda p: (p[..., 0] - pel[0, 0]) * way[0] + (p[..., 2] - pel[0, 2]) * way[1]
    across = lambda p: (p[..., 0] - pel[0, 0]) * way[1] - (p[..., 2] - pel[0, 2]) * way[0]
    span = max(2.0, float(along(pel).max() - along(pel).min()) + 1.5)
    fig, ax = plt.subplots(2, 1, figsize=(min(60, max(12, span * 2.4)), 9), gridspec_kw={'height_ratios': [2.2, 1]})
    pick = [0]
    for i in range(1, len(fr)):
        if t[i] - t[pick[-1]] >= every - 1e-6:
            pick.append(i)
    # Figures closer together than they are wide are spread out, so that a stand or a slow walk can be read.
    spread, last = [], None
    for k in pick:
        x = float(along(pel[k]))
        if last is not None and x - last < 0.45:
            x = last + 0.45
        spread.append(x); last = x
    for k, x in zip(pick, spread):
        shift = x - float(along(pel[k]))
        for pairs, col, lw in ((MID, '#444444', 1.6), (RIGHT, '#b0b0b0', 1.4), (LEFT, '#111111', 1.6)):
            for a, b in pairs:
                pa, pb = P[k, I[a]], P[k, I[b]]
                ax[0].plot([along(pa) + shift, along(pb) + shift], [pa[1], pb[1]], color=col, lw=lw, solid_capstyle='round')
        for foot, name_f in ((0, 'FootL'), (1, 'FootR')):
            if fr[k]['d'][foot]:
                p = P[k, I[name_f]]
                ax[0].plot([along(p) + shift - 0.05, along(p) + shift + 0.12], [-0.03, -0.03], color='#c0392b' if foot == 0 else '#e59866', lw=3)
        label = fr[k].get('blend', '').split(';')[0].split(':')[0]
        ax[0].text(x, 1.95, f"{t[k]:.1f}", ha='center', fontsize=7, color='#555')
        ax[0].text(x, 2.06, label[:14], ha='center', fontsize=5.5, color='#888', rotation=30)
    ax[0].axhline(0, color='#999', lw=0.8)
    ax[0].set_ylim(-0.1, 2.3); ax[0].set_aspect('equal'); ax[0].set_xlim(spread[0] - 0.8, spread[-1] + 0.8); ax[0].axis('off')
    ax[0].set_title(f"{name}: {t[0]:.1f} to {t[-1]:.1f} s, a figure every {every:.2f} s (left limbs dark, right light; a bar: that foot is down). Figures are spread where he is slow.", fontsize=9)
    ax[1].plot(along(pel), across(pel), color='#444', lw=1)
    for nm, col in (('FootL', '#c0392b'), ('FootR', '#e59866')):
        f = P[:, I[nm]]
        dn = np.array([x['d'][0 if nm == 'FootL' else 1] for x in fr]) > 0
        ax[1].plot(along(f), across(f), color=col, lw=0.5, alpha=0.5)
        ax[1].scatter(along(f)[dn], across(f)[dn], color=col, s=4)
    ax[1].set_aspect('equal'); ax[1].set_title('from above: hips (grey), left foot (red), right foot (orange); dots where the film has the foot down', fontsize=8)
    ax[1].tick_params(labelsize=6)
    plt.tight_layout(); plt.savefig(out, dpi=110); print(out)


if __name__ == '__main__':
    main()
