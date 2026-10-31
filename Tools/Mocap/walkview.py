#!/usr/bin/env python3
"""Side by side: a captured walk (in place) and a unit walking with it (a `walks` runner CSV).

    python3 Tools/Mocap/walkview.py Tools/Mocap/LargeWalk.fbx large_walk.csv out.mp4

The capture is played at the unit's own phase round the cycle, so the two step together. Both are
seen from the game camera's angle and follow their hips; each is scaled to its own leg.
"""
import argparse, os, subprocess, sys, tempfile
import numpy as np
import pandas as pd
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from mixamo_rhythm import Skeleton
from compare import FBX_BONES, UNIT_BONES, side


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('fbx'); ap.add_argument('csv'); ap.add_argument('out')
    ap.add_argument('--fps', type=float, default=20)
    args = ap.parse_args()
    sk = Skeleton(args.fbx)
    df = pd.read_csv(args.csv)
    w0 = sk.pose(0)
    fleg = np.linalg.norm(w0['LeftUpLeg'][1] - w0['LeftLeg'][1]) + np.linalg.norm(w0['LeftLeg'][1] - w0['LeftFoot'][1]) + w0['LeftFoot'][1][1]
    end = sk.pose(sk.length)
    travel = 0.5 * (end['LeftUpLeg'][1] + end['RightUpLeg'][1]) - 0.5 * (w0['LeftUpLeg'][1] + w0['RightUpLeg'][1])
    travel[1] = 0
    uleg = float(df['leg'].iloc[0])

    def draw(ax, pts, bones, title, extra=None):
        ax.cla()
        for a, b in bones:
            if a in pts and b in pts:
                p, q = pts[a], pts[b]
                ax.plot([p[0], q[0]], [p[2], q[2]], [p[1], q[1]], color=side(a, b), lw=3, solid_capstyle='round')
        if extra:
            extra(ax)
        ax.set_xlim(-0.9, 0.9); ax.set_ylim(-0.9, 0.9); ax.set_zlim(0, 1.8)
        ax.set_box_aspect((1, 1, 1))
        ax.view_init(elev=22, azim=-150)
        ax.set_xticks([]); ax.set_yticks([]); ax.set_zticks([])
        ax.set_title(title, fontsize=10)

    tmp = tempfile.mkdtemp()
    fig = plt.figure(figsize=(10, 5.4))
    a1 = fig.add_subplot(1, 2, 1, projection='3d'); a2 = fig.add_subplot(1, 2, 2, projection='3d')
    step = max(1, int(round(60 / args.fps)))
    rows = list(range(0, len(df), step))
    for n, i in enumerate(rows):
        r = df.iloc[i]
        ph = float(r['phase'])
        t = ph * sk.length
        w = sk.pose(t)
        c = 0.5 * (w['LeftUpLeg'][1] + w['RightUpLeg'][1]); c[1] = 0
        # (in place: against an evenly moving root, so the feet slide back under it)
        root = 0.5 * (w0['LeftUpLeg'][1] + w0['RightUpLeg'][1]) + travel * (t / sk.length); root[1] = 0
        fp = {k: (v[1] - root) / fleg for k, v in w.items()}
        rc = np.array([r['root.x'], 0, r['root.z']])
        up = {k[:-2]: (np.array([r[k[:-2] + '.x'], r[k[:-2] + '.y'], r[k[:-2] + '.z']]) - rc) / uleg for k in df.columns if k.endswith('.x') and k[:-2] not in ('root',)}

        def ground(ax, pts, feet):
            for f in feet:
                p = pts[f]
                ax.scatter([p[0]], [p[2]], [0], s=12, color='#999')

        def club(ax):
            g, tip = up['grip'], up['tip']
            ax.plot([g[0], tip[0]], [g[2], tip[2]], [g[1], tip[1]], color='#8a5a2b', lw=4)
            hd = up['Head']
            ax.scatter([hd[0]], [hd[2]], [hd[1]], s=200, color='#e0b080', edgecolor='k')

        draw(a1, fp, FBX_BONES, f'capture ({os.path.basename(args.fbx)}), in place')
        draw(a2, up, UNIT_BONES, f'unit t={r["t"]:.2f}s  walk {r["weight"]:.2f}', club)
        fig.savefig(os.path.join(tmp, f'f{n:04d}.png'), dpi=75)
    subprocess.run(['ffmpeg', '-y', '-loglevel', 'error', '-framerate', str(args.fps), '-i', os.path.join(tmp, 'f%04d.png'),
                    '-vf', 'scale=trunc(iw/2)*2:trunc(ih/2)*2', '-c:v', 'libx264', '-preset', 'veryfast', '-pix_fmt', 'yuv420p', args.out], check=True)


if __name__ == '__main__':
    main()
