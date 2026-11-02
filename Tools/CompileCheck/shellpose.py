"""Draws `shells` output: each body's plain guard, shell, broken guard and parry or block, from the front
(behind the opponent, looking at the unit) and from the side. Usage: python3 shellpose.py shells.json out.png"""
import json, sys
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from matplotlib.patches import Circle

BONES = [("Pelvis", "Spine"), ("Spine", "Chest"), ("Chest", "Head"),
         ("Pelvis", "ThighL"), ("ThighL", "ShinL"), ("ShinL", "FootL"),
         ("Pelvis", "ThighR"), ("ThighR", "ShinR"), ("ShinR", "FootR")]
LEFT = [("Chest", "UpperArmL"), ("UpperArmL", "ForearmL"), ("ForearmL", "HandL")]
RIGHT = [("Chest", "UpperArmR"), ("UpperArmR", "ForearmR"), ("ForearmR", "HandR")]
shots = json.load(open(sys.argv[1]))
bodies = []
for s in shots:
    if s["body"] not in bodies:
        bodies.append(s["body"])
cols = max(sum(1 for s in shots if s["body"] == b) for b in bodies)
fig, axes = plt.subplots(len(bodies) * 2, cols, figsize=(3.2 * cols, 3.4 * len(bodies) * 2))
for r, b in enumerate(bodies):
    row = [s for s in shots if s["body"] == b]
    for c in range(cols):
        for v in range(2):
            ax = axes[2 * r + v][c]
            ax.set_aspect("equal")
            ax.set_xticks([]); ax.set_yticks([])
            if c >= len(row):
                ax.axis("off")
                continue
            s = row[c]
            p = s["pose"]
            # front: x across (mirrored: seen from the opponent), y up; side: z forward, y up
            X = (lambda q: -q[0]) if v == 0 else (lambda q: q[2])
            Y = lambda q: q[1]
            for a, bb in BONES:
                ax.plot([X(p[a]), X(p[bb])], [Y(p[a]), Y(p[bb])], color="#4a6fd0", lw=2)
            for a, bb in LEFT:
                ax.plot([X(p[a]), X(p[bb])], [Y(p[a]), Y(p[bb])], color="#2a9d4a", lw=3)
            for a, bb in RIGHT:
                ax.plot([X(p[a]), X(p[bb])], [Y(p[a]), Y(p[bb])], color="#d03a2a", lw=3)
            ax.plot([X(s["grip"]), X(s["tip"])], [Y(s["grip"]), Y(s["tip"])], color="black", lw=3)
            ax.add_patch(Circle((X(p["Head"]), Y(p["Head"]) + 0.6 * s["head"]), s["head"], fill=False, color="#4a6fd0", lw=1.5))
            if "shield" in s:
                ax.scatter([X(s["shield"])], [Y(s["shield"])], color="#2a9d4a", s=200, alpha=0.4)
            ax.set_title(f"{b} {s['name']} ({'front' if v == 0 else 'side'})\n{s['clear']}", fontsize=8)
            ax.set_xlim(X(p["Pelvis"]) - 1.0, X(p["Pelvis"]) + 1.0) if v == 0 else ax.set_xlim(p["Pelvis"][2] - 0.8, p["Pelvis"][2] + 1.2)
            ax.set_ylim(-0.05, Y(p["Head"]) + 0.6)
plt.tight_layout()
plt.savefig(sys.argv[2], dpi=80)
