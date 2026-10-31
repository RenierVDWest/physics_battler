#!/usr/bin/env python3
"""A whole fight as a video: every unit's physical body (and weapon) on the board, seen from above
and to the side, team A red, team B blue; dead units grey, cut-off limbs left lying where they fell.

    python3 Tools/Playtest/fightview.py <session folder> out.mp4 [--speed 3] [--fps 15]
"""
import argparse, glob, os, re, subprocess, tempfile
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


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('session'); ap.add_argument('out')
    ap.add_argument('--speed', type=float, default=3.0)
    ap.add_argument('--fps', type=float, default=15)
    args = ap.parse_args()
    units = []
    for fn in sorted(glob.glob(os.path.join(args.session, 'units', '*.pose.csv.gz'))):
        name = re.sub(r'^\d+_', '', os.path.basename(fn).split('.')[0]).replace('_', ' ')
        units.append((name, pd.read_csv(fn)))
    events = open(os.path.join(args.session, 'events.log')).read().splitlines()
    deaths = {}
    over = None
    for line in events:
        parts = [p.strip() for p in line.split('|')]
        if len(parts) >= 5 and parts[3] == 'death':
            deaths[parts[2]] = float(parts[0])
        if len(parts) >= 5 and 'over:' in parts[4]:
            over = (float(parts[0]), parts[4])
    end = max(u[1]['t'].iloc[-1] for u in units)
    tmp = tempfile.mkdtemp()
    fig = plt.figure(figsize=(10, 7.5))
    ax = fig.add_axes([-0.12, -0.18, 1.24, 1.3], projection='3d')
    n = int(end / args.speed * args.fps)
    for i in range(n):
        t = i * args.speed / args.fps
        ax.cla()
        for k in range(9):
            g = -6.4 + 1.6 * k
            ax.plot([g, g], [-6.4, 6.4], [0, 0], color='#ccc', lw=0.6)
            ax.plot([-6.4, 6.4], [g, g], [0, 0], color='#ccc', lw=0.6)
        for name, df in units:
            r = df.iloc[min(len(df) - 1, int(np.searchsorted(df['t'].values, t)))]
            dead = name in deaths and t >= deaths[name]
            col = '#999' if dead else ('#c0392b' if name.startswith('A') else '#1f4e79')
            for a, b in BONES:
                ax.plot([r[a + '.x'], r[b + '.x']], [r[a + '.z'], r[b + '.z']], [r[a + '.y'], r[b + '.y']], color=col, lw=2)
            ax.scatter([r['Head.x']], [r['Head.z']], [r['Head.y']], color=col, s=30)
            if not np.isnan(r.get('grip.x', np.nan)):
                ax.plot([r['grip.x'], r['tip.x']], [r['grip.z'], r['tip.z']], [r['grip.y'], r['tip.y']], color='#8a5a2b', lw=3)
            if not dead:
                ax.text(r['Pelvis.x'], r['Pelvis.z'], r['Head.y'] + 0.5, name, fontsize=8, color=col, ha='center')
        ax.set_xlim(-6.4, 6.4); ax.set_ylim(-6.4, 6.4); ax.set_zlim(0, 4)
        ax.set_box_aspect((1, 1, 0.32), zoom=1.35)
        ax.view_init(elev=38, azim=-62)
        ax.set_xticks([]); ax.set_yticks([]); ax.set_zticks([])
        title = f't = {t:5.1f} s   (x{args.speed:g})'
        if over and t >= over[0]:
            title += '   ' + over[1]
        fig.texts.clear()
        fig.text(0.02, 0.96, title, fontsize=11)
        fig.savefig(os.path.join(tmp, f'f{i:05d}.png'), dpi=70)
    subprocess.run(['ffmpeg', '-y', '-loglevel', 'error', '-framerate', str(args.fps), '-i', os.path.join(tmp, 'f%05d.png'),
                    '-vf', 'scale=trunc(iw/2)*2:trunc(ih/2)*2', '-c:v', 'libx264', '-preset', 'veryfast', '-pix_fmt', 'yuv420p', args.out], check=True)


if __name__ == '__main__':
    main()
