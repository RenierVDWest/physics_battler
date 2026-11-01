#!/usr/bin/env python3
"""Film strips of the shield runner (milestone 9): for chosen trials, frames through the blow seen
from above and from the side. The defender in blue with its shield disc (its face's normal as a
short line), its head's look as a dashed line; the attacker's sword in brown.

    python3 Tools/Playtest/shieldview.py <shield.csv> out.png [--trials 1,5,9] [--frames 8]
"""
import argparse
import numpy as np
import pandas as pd
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt

BONES = [('Pelvis', 'Spine'), ('Spine', 'Chest'), ('Chest', 'Head'),
         ('Chest', 'UpperArmL'), ('UpperArmL', 'ForearmL'), ('ForearmL', 'HandL'),
         ('Chest', 'UpperArmR'), ('UpperArmR', 'ForearmR'), ('ForearmR', 'HandR'),
         ('Pelvis', 'ThighL'), ('ThighL', 'ShinL'), ('ShinL', 'FootL'),
         ('Pelvis', 'ThighR'), ('ThighR', 'ShinR'), ('ShinR', 'FootR')]


def disc(r, n=24):
    c = np.array([r['sc.x'], r['sc.y'], r['sc.z']])
    f = np.array([r['sf.x'], r['sf.y'], r['sf.z']])
    f = f / (np.linalg.norm(f) + 1e-9)
    up = np.array([0, 1.0, 0])
    u = np.cross(up, f)
    if np.linalg.norm(u) < 1e-6:
        u = np.array([1.0, 0, 0])
    u /= np.linalg.norm(u)
    v = np.cross(f, u)
    a = np.linspace(0, 2 * np.pi, n + 1)
    pts = c[None, :] + r['sr'] * (np.cos(a)[:, None] * u[None, :] + np.sin(a)[:, None] * v[None, :])
    return c, f, pts


def draw(ax, r, view):
    # view: 'top' (x, z) or 'side' (z, y) seen from the defender's right
    def P(p):
        return (p[0], p[2]) if view == 'top' else (p[2], p[1])
    for a, b in BONES:
        pa = (r[a + '.x'], r[a + '.y'], r[a + '.z'])
        pb = (r[b + '.x'], r[b + '.y'], r[b + '.z'])
        ax.plot(*zip(P(pa), P(pb)), color='#1f4e79', lw=1.5)
    if r['sr'] > 0:
        c, f, pts = disc(r)
        xy = np.array([P(p) for p in pts])
        ax.plot(xy[:, 0], xy[:, 1], color='#2e86c1', lw=1.2)
        ax.plot(*zip(P(c), P(c + 0.25 * f)), color='#2e86c1', lw=1)
    g = (r['grip.x'], r['grip.y'], r['grip.z'])
    t = (r['tip.x'], r['tip.y'], r['tip.z'])
    ax.plot(*zip(P(g), P(t)), color='#8a5a2b', lw=2.5)
    e = np.array([r['eye.x'], r['eye.y'], r['eye.z']])
    l = np.array([r['look.x'], r['look.y'], r['look.z']])
    ax.plot(*zip(P(e), P(e + 0.6 * l)), color='#555', lw=1, ls='--')


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('csv'); ap.add_argument('out')
    ap.add_argument('--trials', default='')
    ap.add_argument('--frames', type=int, default=8)
    args = ap.parse_args()
    df = pd.read_csv(args.csv)
    trials = [int(x) for x in args.trials.split(',')] if args.trials else sorted(df['trial'].unique())[:6]
    n = args.frames
    fig, axes = plt.subplots(2 * len(trials), n, figsize=(2.0 * n, 4.2 * len(trials)))
    axes = np.atleast_2d(axes)
    for k, tr in enumerate(trials):
        d = df[df['trial'] == tr]
        if len(d) == 0:
            continue
        idx = np.linspace(0, len(d) - 1, n).astype(int)
        for j, i in enumerate(idx):
            r = d.iloc[i]
            for v, view in enumerate(('top', 'side')):
                ax = axes[2 * k + v, j]
                draw(ax, r, view)
                ax.set_aspect('equal')
                if view == 'top':
                    ax.set_xlim(-1.6, 1.6); ax.set_ylim(-1.6, 1.6)
                else:
                    ax.set_xlim(-1.6, 1.6); ax.set_ylim(0, 2.2)
                ax.set_xticks([]); ax.set_yticks([])
                if v == 0:
                    ax.set_title(f"#{tr} {int(r['dir'])}° {r['move']}\nblow {r['blowT']:.2f} {r['mode']}", fontsize=7)
    fig.tight_layout()
    fig.savefig(args.out, dpi=90)


if __name__ == '__main__':
    main()
