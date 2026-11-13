# legsview.py <session dir> <time s> [out.png] [unit filter]: the legs' opinion at one moment, seen from above, as the game's
# "Show Legs Opinion" draws it on the ground: each sole in the colour of what its foot is doing, the support's outline, a dot
# under the body's weight, a cross where a foot is needed. From each unit's frames (footL, footR, weightX..., written by the
# game) and its pose.
import sys, gzip, csv, glob, os, math
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt

d = sys.argv[1]; at = float(sys.argv[2]); out = sys.argv[3] if len(sys.argv) > 3 else 'legsview.png'; only = sys.argv[4] if len(sys.argv) > 4 else ''
STATE = {0: ('planted', '#2fb457'), 1: ('sliding', '#e5332d'), 2: ('landing', '#d9b800'), 3: ('low in the air', '#f08a1c'), 4: ('stepping', '#4f8fe0')}

def rot(q, v):
    x, y, z, w = q; vx, vy, vz = v
    tx = 2 * (y * vz - z * vy); ty = 2 * (z * vx - x * vz); tz = 2 * (x * vy - y * vx)
    return (vx + w * tx + (y * tz - z * ty), vy + w * ty + (z * tx - x * tz), vz + w * tz + (x * ty - y * tx))

def hull(pts):
    pts = sorted(set(pts))
    if len(pts) < 3: return pts
    def cross(o, a, b): return (a[0] - o[0]) * (b[1] - o[1]) - (a[1] - o[1]) * (b[0] - o[0])
    lo = []
    for p in pts:
        while len(lo) >= 2 and cross(lo[-2], lo[-1], p) <= 0: lo.pop()
        lo.append(p)
    up = []
    for p in reversed(pts):
        while len(up) >= 2 and cross(up[-2], up[-1], p) <= 0: up.pop()
        up.append(p)
    return lo[:-1] + up[:-1]

files = [f for f in sorted(glob.glob(d + '/units/*.pose.csv.gz')) if not only or only in os.path.basename(f)[3:].split('.')[0].replace('_', ' ')]
cols = min(3, max(1, len(files))); rows = max(1, math.ceil(len(files) / cols))
fig, axes = plt.subplots(rows, cols, figsize=(4.2 * cols, 4.4 * rows), dpi=140, squeeze=False)
seen = 0
for n, f in enumerate(files):
    ax = axes[n // cols][n % cols]
    ax.set_facecolor('#f4f2ee'); ax.set_aspect('equal'); ax.set_xticks([]); ax.set_yticks([])
    name = os.path.basename(f)[3:].split('.')[0].replace('_', ' ')
    frames = list(csv.DictReader(gzip.open(f.replace('.pose.', '.frames.'), 'rt')))
    if not frames or 'footL' not in frames[0]:
        ax.set_title(name + ': no legs reading in this log', fontsize=9); continue
    # (The reading in a frame's row was taken a step before the pose beside it: the row after is the pose's own.)
    i = min(range(len(frames)), key=lambda k: abs(float(frames[k]['t']) - at))
    row = frames[min(i + 1, len(frames) - 1)]
    pose = {r['t']: r for r in csv.DictReader(gzip.open(f, 'rt'))}
    p = pose.get(frames[i]['t'])
    if p is None or p['Pelvis.y'] in ('', 'NaN'):
        ax.set_title(name + ': not there', fontsize=9); continue
    P = lambda part: (float(p[part + '.x']), float(p[part + '.y']), float(p[part + '.z']))
    Q = lambda part: (float(p[part + '.qx']), float(p[part + '.qy']), float(p[part + '.qz']), float(p[part + '.qw']))
    ups = sorted(float(r['FootL.y']) for r in pose.values() if r['Pelvis.y'] not in ('', 'NaN') and float(r['Pelvis.y']) > 0.62)
    stand = ups[len(ups) // 20] if ups else 0.083
    # (Its size as the game has it: its leg against a Medium's.)
    size = (math.dist(P('ThighL'), P('ShinL')) + math.dist(P('ShinL'), P('FootL'))) / 0.845
    if float(p['Pelvis.y']) < 0.62 * size:
        ax.set_title(name + ': down', fontsize=9); continue
    pts = []; doing = []
    for s, col in (('L', 'footL'), ('R', 'footR')):
        a = P('Foot' + s); q = Q('Foot' + s); down = -(stand - 0.015 * size); w = 0.05 * size
        cs = [tuple(x + y for x, y in zip(a, rot(q, c))) for c in ((-w, down, -0.02 * size), (w, down, -0.02 * size), (w, down, 0.10 * size), (-w, down, 0.10 * size))]
        label, colour = STATE.get(int(row[col]), ('?', '#888888'))
        doing.append(('left ' if s == 'L' else 'right ') + label)
        ax.fill([c[0] for c in cs], [c[2] for c in cs], facecolor=colour, alpha=0.3, edgecolor=colour, linewidth=2.4, zorder=3)
        # (An end of the sole is down by its middle, as the game has it.)
        heel = 0.5 * (cs[0][1] + cs[1][1]) < 0.02 * size; toe = 0.5 * (cs[2][1] + cs[3][1]) < 0.02 * size
        if heel: pts += [(cs[0][0], cs[0][2]), (cs[1][0], cs[1][2])]
        if toe: pts += [(cs[2][0], cs[2][2]), (cs[3][0], cs[3][2])]
    h = hull(pts)
    if len(h) >= 2:
        ax.plot([c[0] for c in h] + [h[0][0]], [c[1] for c in h] + [h[0][1]], color='#444444', linewidth=1.1, linestyle='--', zorder=2)
    wx, wz, nx, nz = float(row['weightX']), float(row['weightZ']), float(row['neededX']), float(row['neededZ'])
    wo, no = float(row['weightOut']), float(row['neededOut'])
    wanted = no < 0 or no > 10 * size
    ax.plot([wx, nx], [wz, nz], color='#f06a00' if wanted else '#999999', linewidth=1.0, zorder=4)
    ax.plot([nx], [nz], marker='x', markersize=13 if wanted else 9, markeredgewidth=3 if wanted else 1.6, color='#f06a00' if wanted else '#999999', zorder=5)
    ax.plot([wx], [wz], marker='o', markersize=10, color='#2fb457' if wo == 0 else '#e5332d', markeredgecolor='white', zorder=6)
    # (The body, faintly: its shoulders' line, so which way it faces can be seen.)
    sl, sr = P('UpperArmL'), P('UpperArmR')
    ax.plot([sl[0], sr[0]], [sl[2], sr[2]], color='#333333', alpha=0.2, linewidth=7, solid_capstyle='round', zorder=1)
    half = 0.75 * size
    ax.set_xlim(wx - half, wx + half); ax.set_ylim(wz - half, wz + half)
    ax.plot([wx - half + 0.05, wx - half + 0.25], [wz - half + 0.06] * 2, color='#222222', linewidth=2); ax.text(wx - half + 0.15, wz - half + 0.09, '20 cm', ha='center', fontsize=7)
    weight = 'both feet up' if wo < 0 else 'weight over the support' if wo == 0 else f'weight {wo:.0f} cm outside'
    ax.set_title(f"{name}\n{', '.join(doing)}; {weight}", fontsize=8.5)
    seen += 1
for n in range(len(files), rows * cols):
    axes[n // cols][n % cols].axis('off')
ax = fig.add_axes([0.0, -0.02, 1.0, 0.02]); ax.axis('off')
for k, (label, colour) in STATE.items():
    ax.plot([], [], marker='s', linestyle='', markersize=9, markerfacecolor=colour, alpha=0.6, markeredgecolor=colour, label='a foot ' + label)
ax.plot([], [], color='#555555', linestyle='--', label='the support')
ax.plot([], [], marker='o', linestyle='', color='#2fb457', markeredgecolor='white', label='the weight, over the support')
ax.plot([], [], marker='o', linestyle='', color='#e5332d', markeredgecolor='white', label='the weight, outside it')
ax.plot([], [], marker='x', linestyle='', color='#999999', label='where a foot is needed (on the support)')
ax.plot([], [], marker='x', linestyle='', markeredgewidth=3, color='#f06a00', label='a foot needed over 10 cm outside it')
ax.legend(loc='upper center', ncol=5, fontsize=8, frameon=False)
fig.suptitle(f"The legs' opinion at {at:.2f} s, each fighter from above ({os.path.basename(os.path.normpath(d))})", fontsize=11)
fig.savefig(out, bbox_inches='tight')
print('wrote', out, 'with', seen, 'units')
