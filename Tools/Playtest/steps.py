# steps.py <session dir> [from s] [to s] [out.png]: each fighter's feet over time, as the legs' reading has them (planted,
# sliding, landing, low in the air, stepping), with what the walk is doing (standing, walking, a clip) and how fast the
# body goes against what the fight asked. To see the rhythm of its steps: stand, step, stand, or a walk in slow motion.
import sys, gzip, csv, glob, os
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
from matplotlib.colors import ListedColormap

d = sys.argv[1]; t0 = float(sys.argv[2]) if len(sys.argv) > 2 else 5.0; t1 = float(sys.argv[3]) if len(sys.argv) > 3 else t0 + 20.0
out = sys.argv[4] if len(sys.argv) > 4 else 'steps.png'
FOOT = ListedColormap(['#2fb457', '#e5332d', '#d9b800', '#f08a1c', '#4f8fe0'])     # planted, sliding, landing, low, stepping
MODE = ListedColormap(['#d8d4cc', '#9fb8d8', '#c9a0c9'])                            # standing, walking, a clip
# (Logs from stand and step on, the "stepping" column: standing still, stepping in the stance, a step walked with the
# guard's walk, walking as asked, walking off a shove, a clip, down.)
STEP = ListedColormap(['#d8d4cc', '#3fa7a0', '#9fb8d8', '#4a6fb0', '#e0a070', '#c9a0c9', '#ffffff'])
files = sorted(glob.glob(d + '/units/*.frames.csv.gz'))
fig, axes = plt.subplots(len(files), 1, figsize=(13, 1.9 * len(files) + 0.8), dpi=130, squeeze=False)
for n, f in enumerate(files):
    ax = axes[n][0]
    name = os.path.basename(f)[3:].split('.')[0].replace('_', ' ')
    rows = [r for r in csv.DictReader(gzip.open(f, 'rt')) if t0 <= float(r['t']) <= t1]
    if not rows or 'footL' not in rows[0]:
        ax.set_title(name + ': no legs reading in this log', fontsize=9); continue
    t = [float(r['t']) for r in rows]
    ext = [t[0], t[-1]]
    ax.imshow([[int(r['footL']) for r in rows]], aspect='auto', cmap=FOOT, vmin=0, vmax=4, extent=ext + [2.0, 3.0], interpolation='nearest')
    ax.imshow([[int(r['footR']) for r in rows]], aspect='auto', cmap=FOOT, vmin=0, vmax=4, extent=ext + [1.0, 2.0], interpolation='nearest')
    if 'stepping' in rows[0]:
        ax.imshow([[int(float(r['stepping'])) for r in rows]], aspect='auto', cmap=STEP, vmin=0, vmax=6, extent=ext + [0.55, 0.95], interpolation='nearest')
    else:
        ax.imshow([[int(float(r['clipMode'])) for r in rows]], aspect='auto', cmap=MODE, vmin=0, vmax=2, extent=ext + [0.55, 0.95], interpolation='nearest')
    ax.axhline(2.0, color='white', linewidth=1.5)
    # (The speeds in the band under the strips: 0 at its foot, 1.5 m/s at its top.)
    band = lambda v: -1.15 + min(1.6, float(v))
    ax.plot(t, [band(r['clipSpeed']) for r in rows], color='#222222', linewidth=0.9, label='the body, m/s')
    ax.plot(t, [band(r['clipWant']) for r in rows], color='#999999', linewidth=0.8, linestyle=':', label='asked, m/s')
    for v in (0.0, 0.5, 1.0):
        ax.axhline(band(v), color='#dddddd', linewidth=0.5, zorder=0)
    ax.set_ylim(-1.2, 3.0); ax.set_xlim(t[0], t[-1])
    ax.set_yticks([band(0), band(0.5), band(1.0), 0.75, 1.5, 2.5]); ax.set_yticklabels(['0', '0.5', '1 m/s', 'doing', 'right foot', 'left foot'], fontsize=7)
    if n == 0: ax.legend(loc='lower right', fontsize=7, ncol=2, frameon=False)
    ax.set_title(name, fontsize=9, loc='left'); ax.tick_params(axis='x', labelsize=7)
axes[-1][0].set_xlabel('seconds')
fig.text(0.01, 0.005, 'feet: green planted, red sliding, yellow landing, orange in the air under 9 cm, blue stepping.   doing: grey standing still, teal stepping in the stance, light blue a step walked, dark blue walking, tan walking off a shove, purple a clip, white down.', fontsize=8)
fig.tight_layout(rect=[0, 0.02, 1, 1])
fig.savefig(out)
print('wrote', out)
