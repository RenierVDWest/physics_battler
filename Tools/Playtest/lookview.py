#!/usr/bin/env python3
"""Frames from the look runner (milestone 9), seen from above: the unit at the middle (blue), its
view drawn from its head (clear view dark, the edge of its view light, the felt circle dashed), the
point it attends to (a cross) and the two enemies walking round it, coloured by how it perceives
them (black: clear, orange: the edge of its view, grey: felt, red outline: not at all; a thick ring
while one swings at it).

    python3 Tools/Playtest/lookview.py <look.csv> out.png [--frames 24] [--from 0] [--to 12]
"""
import argparse
import numpy as np
import pandas as pd
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
from matplotlib.patches import Wedge, Circle

BONES = [('Pelvis', 'Spine'), ('Spine', 'Chest'), ('Chest', 'Head'),
         ('Chest', 'UpperArmL'), ('UpperArmL', 'ForearmL'), ('ForearmL', 'HandL'),
         ('Chest', 'UpperArmR'), ('UpperArmR', 'ForearmR'), ('ForearmR', 'HandR'),
         ('Pelvis', 'ThighL'), ('ThighL', 'ShinL'), ('ShinL', 'FootL'),
         ('Pelvis', 'ThighR'), ('ThighR', 'ShinR'), ('ShinR', 'FootR')]
COLOUR = {'Clear': '#111', 'Edge': '#e67e22', 'Felt': '#999', 'Unseen': 'none'}


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('csv'); ap.add_argument('out')
    ap.add_argument('--frames', type=int, default=24)
    ap.add_argument('--from', dest='t0', type=float, default=0.0)
    ap.add_argument('--to', dest='t1', type=float, default=12.0)
    args = ap.parse_args()
    df = pd.read_csv(args.csv)
    df = df[(df.t >= args.t0) & (df.t <= args.t1)]
    idx = np.linspace(0, len(df) - 1, args.frames).astype(int)
    cols = 6
    rows = (len(idx) + cols - 1) // cols
    fig, axes = plt.subplots(rows, cols, figsize=(2.6 * cols, 2.7 * rows))
    for ax, i in zip(axes.flat, idx):
        r = df.iloc[i]
        # The view, from the head: x right, z up in the picture.
        heading = np.degrees(np.arctan2(r['fwd.x'], r['fwd.z']))   # clockwise from +z
        centre = (r['eye.x'], r['eye.z'])
        theta = 90 - heading   # matplotlib: counter-clockwise from +x
        ax.add_patch(Wedge(centre, 4, theta - 100, theta + 100, color='#d6eaf8', zorder=0))
        ax.add_patch(Wedge(centre, 4, theta - 30, theta + 30, color='#85c1e9', zorder=0))
        ax.add_patch(Circle(centre, 0.9 + 0.33, fill=False, ls='--', color='#aaa', lw=0.8))
        for a, b in BONES:
            ax.plot([r[a + '.x'], r[b + '.x']], [r[a + '.z'], r[b + '.z']], color='#1f4e79', lw=1.2)
        ax.plot([r['eye.x'], r['eye.x'] + 0.7 * r['fwd.x']], [r['eye.z'], r['eye.z'] + 0.7 * r['fwd.z']], color='#1f4e79', lw=2)
        ax.plot([r['point.x']], [r['point.z']], marker='x', color='#c0392b', ms=7, mew=2)
        for who in ('near', 'far'):
            s = r[who + '.sight']
            face = COLOUR.get(s, 'none')
            edge = '#c0392b' if s == 'Unseen' else face
            ax.add_patch(Circle((r[who + '.x'], r[who + '.z']), 0.22, facecolor=face, edgecolor=edge, lw=3 if r[who + '.swing'] else 1.2))
        ax.set_xlim(-3.6, 3.6); ax.set_ylim(-3.6, 3.6)
        ax.set_aspect('equal'); ax.set_xticks([]); ax.set_yticks([])
        ax.set_title(f"{r['t']:.1f} s: {r['kind']}", fontsize=8)
    for ax in list(axes.flat)[len(idx):]:
        ax.axis('off')
    fig.tight_layout()
    fig.savefig(args.out, dpi=90)


if __name__ == '__main__':
    main()
