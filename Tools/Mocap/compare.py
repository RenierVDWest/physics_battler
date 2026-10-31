#!/usr/bin/env python3
"""Side by side: a captured loop (the FBX skeleton) and a unit playing it (a `rhythms` runner CSV).

    python3 Tools/Mocap/compare.py Boxing.fbx large_Base.csv out.mp4 [--still out.png]

Both are drawn from the game camera's angle (above and behind), each scaled to its own leg
length, and lined up in time (the unit's loop is a little slower on a bigger body).
"""
import argparse, os, subprocess, sys, tempfile
import numpy as np
import pandas as pd
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from mixamo_rhythm import Skeleton

FBX_BONES = [('Hips', 'Spine'), ('Spine', 'Spine1'), ('Spine1', 'Spine2'), ('Spine2', 'Neck'), ('Neck', 'Head'),
             ('Spine2', 'LeftShoulder'), ('LeftShoulder', 'LeftArm'), ('LeftArm', 'LeftForeArm'), ('LeftForeArm', 'LeftHand'), ('LeftHand', 'LeftHandMiddle1'),
             ('Spine2', 'RightShoulder'), ('RightShoulder', 'RightArm'), ('RightArm', 'RightForeArm'), ('RightForeArm', 'RightHand'), ('RightHand', 'RightHandMiddle1'),
             ('Hips', 'LeftUpLeg'), ('LeftUpLeg', 'LeftLeg'), ('LeftLeg', 'LeftFoot'), ('LeftFoot', 'LeftToeBase'),
             ('Hips', 'RightUpLeg'), ('RightUpLeg', 'RightLeg'), ('RightLeg', 'RightFoot'), ('RightFoot', 'RightToeBase'),
             ('Head', 'HeadTop_End')]
UNIT_BONES = [('Pelvis', 'Spine'), ('Spine', 'Chest'), ('Chest', 'Head'),
              ('Chest', 'UpperArmL'), ('UpperArmL', 'ForearmL'), ('ForearmL', 'HandL'),
              ('Chest', 'UpperArmR'), ('UpperArmR', 'ForearmR'), ('ForearmR', 'HandR'),
              ('Pelvis', 'ThighL'), ('ThighL', 'ShinL'), ('ShinL', 'FootL'),
              ('Pelvis', 'ThighR'), ('ThighR', 'ShinR'), ('ShinR', 'FootR')]


def side(a, b):
    return '#c0392b' if 'Left' in a + b or a.endswith('L') or b.endswith('L') else '#1f4e79'


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('fbx'); ap.add_argument('csv'); ap.add_argument('out')
    ap.add_argument('--still')
    ap.add_argument('--fps', type=float, default=20)
    ap.add_argument('--loops', type=float, default=2)
    args = ap.parse_args()

    sk = Skeleton(args.fbx)
    df = pd.read_csv(args.csv)
    # The unit's loop length: from its pelvis's fore-aft rock (autocorrelation peak near the capture's length).
    pz = df['Pelvis.z'].values - 0.5 * (df['FootL.z'].values + df['FootR.z'].values)
    t = df['t'].values
    dt = t[1] - t[0]
    best, best_lag = -1, None
    for lag in range(int(0.6 * sk.length / dt), int(1.6 * sk.length / dt)):
        c = np.corrcoef(pz[:-lag], pz[lag:])[0, 1]
        if c > best:
            best, best_lag = c, lag
    period = best_lag * dt
    # Phase: line the capture's hips up with the unit's.
    fbx_times = np.linspace(0, sk.length, 60, endpoint=False)
    fz = []
    for ft in fbx_times:
        w = sk.pose(ft)
        hip = 0.5 * (w['LeftUpLeg'][1] + w['RightUpLeg'][1])
        fz.append(hip[2] - 0.5 * (w['LeftFoot'][1][2] + w['RightFoot'][1][2]))
    fz = np.array(fz)
    start = int(1.0 / dt)
    seg_t = t[start:] - t[start]
    best_c, offset = -2, 0.0
    for off in np.linspace(0, 1, 60, endpoint=False):
        ph = (seg_t / period + off) % 1.0
        model = np.interp(ph * sk.length, fbx_times, fz, period=sk.length)
        n = min(len(model), int(2 * period / dt))
        c = np.corrcoef(model[:n], pz[start:start + n])[0, 1]
        if c > best_c:
            best_c, offset = c, off
    print(f'unit loop {period:.2f} s (capture {sk.length:.2f} s), hips rock correlation {best_c:.2f}')

    w0 = sk.pose(0)
    fleg = np.linalg.norm(w0['LeftUpLeg'][1] - w0['LeftLeg'][1]) + np.linalg.norm(w0['LeftLeg'][1] - w0['LeftFoot'][1]) + w0['LeftFoot'][1][1]
    r0 = df.iloc[0]
    uleg = float(r0['Pelvis.y'])   # (pelvis height standing: near enough to scale by)

    def frame(ax, pts, bones, title, extra=None):
        ax.cla()
        for a, b in bones:
            if a in pts and b in pts:
                p, q = pts[a], pts[b]
                ax.plot([p[0], q[0]], [p[2], q[2]], [p[1], q[1]], color=side(a, b), lw=3, solid_capstyle='round')
        if extra:
            extra(ax)
        ax.set_xlim(-0.7, 0.7); ax.set_ylim(-0.7, 0.7); ax.set_zlim(0, 1.6)
        ax.set_box_aspect((1, 1, 1.15))
        ax.view_init(elev=30, azim=-125)
        ax.set_xticks([]); ax.set_yticks([]); ax.set_zticks([])
        xx, yy = np.meshgrid([-0.7, 0.7], [-0.7, 0.7])
        ax.plot_surface(xx, yy, np.zeros_like(xx), color='#ddd', alpha=0.3)
        ax.set_title(title, fontsize=10)

    n = int(args.loops * period * args.fps)
    tmp = tempfile.mkdtemp()
    fig = plt.figure(figsize=(10, 5.6))
    a1 = fig.add_subplot(1, 2, 1, projection='3d'); a2 = fig.add_subplot(1, 2, 2, projection='3d')
    for i in range(n):
        tt = start * dt + i / args.fps
        ph = ((tt - start * dt) / period + offset) % 1.0
        w = sk.pose(ph * sk.length)
        c = 0.5 * (w['LeftFoot'][1] + w['RightFoot'][1]); c[1] = 0
        fp = {k: (v[1] - c) / fleg for k, v in w.items()}
        r = df.iloc[min(len(df) - 1, int(round(tt / dt)) - 1)]
        uc = np.array([0.5 * (r['FootL.x'] + r['FootR.x']), 0, 0.5 * (r['FootL.z'] + r['FootR.z'])])
        up = {k.split('.')[0]: (np.array([r[k.split('.')[0] + '.x'], r[k.split('.')[0] + '.y'], r[k.split('.')[0] + '.z']]) - uc) / uleg
              for k in df.columns if k.endswith('.x')}

        def club(ax):
            g, tip = up['grip'], up['tip']
            ax.plot([g[0], tip[0]], [g[2], tip[2]], [g[1], tip[1]], color='#8a5a2b', lw=5)
            hd = up['Head']
            ax.scatter([hd[0]], [hd[2]], [hd[1]], s=260, color='#e0b080', edgecolor='k')

        def fhead(ax):
            hd = 0.5 * (fp['Head'] + fp['HeadTop_End'])
            ax.scatter([hd[0]], [hd[2]], [hd[1]], s=180, color='#e0b080', edgecolor='k')

        frame(a1, fp, FBX_BONES, f'capture ({os.path.basename(args.fbx)})', fhead)
        frame(a2, up, UNIT_BONES, 'the large unit, Fight Rhythm base', club)
        fig.savefig(os.path.join(tmp, f'f{i:04d}.png'), dpi=80)
        if args.still and i == 0:
            pass
    subprocess.run(['ffmpeg', '-y', '-loglevel', 'error', '-framerate', str(args.fps), '-i', os.path.join(tmp, 'f%04d.png'),
                    '-vf', 'scale=trunc(iw/2)*2:trunc(ih/2)*2', '-c:v', 'libx264', '-preset', 'veryfast', '-pix_fmt', 'yuv420p', args.out], check=True)
    if args.still:
        # A strip of moments through one loop.
        k = 6
        fig2 = plt.figure(figsize=(3 * k, 6.4))
        for j in range(k):
            tt = start * dt + j * period / k
            ph = ((tt - start * dt) / period + offset) % 1.0
            w = sk.pose(ph * sk.length)
            c = 0.5 * (w['LeftFoot'][1] + w['RightFoot'][1]); c[1] = 0
            fp = {kk: (v[1] - c) / fleg for kk, v in w.items()}
            r = df.iloc[min(len(df) - 1, int(round(tt / dt)) - 1)]
            uc = np.array([0.5 * (r['FootL.x'] + r['FootR.x']), 0, 0.5 * (r['FootL.z'] + r['FootR.z'])])
            up = {kk.split('.')[0]: (np.array([r[kk.split('.')[0] + '.x'], r[kk.split('.')[0] + '.y'], r[kk.split('.')[0] + '.z']]) - uc) / uleg
                  for kk in df.columns if kk.endswith('.x')}
            b1 = fig2.add_subplot(2, k, j + 1, projection='3d'); b2 = fig2.add_subplot(2, k, k + j + 1, projection='3d')
            frame(b1, fp, FBX_BONES, f'capture {ph:.2f}')
            frame(b2, up, UNIT_BONES, f'unit {ph:.2f}', lambda ax: ax.plot([up['grip'][0], up['tip'][0]], [up['grip'][2], up['tip'][2]], [up['grip'][1], up['tip'][1]], color='#8a5a2b', lw=4))
        fig2.tight_layout()
        fig2.savefig(args.still, dpi=70)


if __name__ == '__main__':
    main()
