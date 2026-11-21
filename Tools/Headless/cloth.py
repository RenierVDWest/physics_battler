#!/usr/bin/env python3
"""cloth.py name [who]: the clothes' points (CLOTHPTS) and the body's joints (BODYPTS) of a run, drawn from the front and
the side at each moment dumped (FIGHT_CLOTHDUMP), into $S/name.cloth.png. who: mover or other (default other)."""
import os, sys
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt

S = os.environ.get("S", "/tmp/pb")
name = sys.argv[1]
who = sys.argv[2] if len(sys.argv) > 2 else "other"
cloth, body = {}, {}
for line in open(os.path.join(S, name + ".txt"), errors="replace"):
    for tag, into in (("CLOTHPTS ", cloth), ("BODYPTS ", body)):
        if line.startswith(tag):
            bits = line.split()
            t, w = bits[1], bits[2]
            if w != who:
                continue
            pts = []
            for b in bits[3:]:
                k, x, y, z = b.split(":")
                pts.append((k, float(x), float(y), float(z)))
            into[t] = pts
times = sorted(cloth, key=float)
if not times:
    sys.exit("no CLOTHPTS lines for " + who)
links = [("Pelvis", "Spine"), ("Spine", "Chest"), ("Chest", "Head"), ("Chest", "UpperArmL"), ("UpperArmL", "ForearmL"), ("ForearmL", "HandL"),
         ("Chest", "UpperArmR"), ("UpperArmR", "ForearmR"), ("ForearmR", "HandR"), ("Pelvis", "ThighL"), ("ThighL", "ShinL"), ("ShinL", "FootL"),
         ("Pelvis", "ThighR"), ("ThighR", "ShinR"), ("ShinR", "FootR")]
fig, axes = plt.subplots(2, len(times), figsize=(3.2 * len(times), 7.5), squeeze=False)
for i, t in enumerate(times):
    j = {k: (x, y, z) for k, x, y, z in body.get(t, [])}
    cx = j["Pelvis"][0] if "Pelvis" in j else 0
    cz = j["Pelvis"][2] if "Pelvis" in j else 0
    for row, (a, label) in enumerate(((0, "x"), (2, "z"))):
        ax = axes[row][i]
        c0 = cx if a == 0 else cz
        for g, col in (("h", "#2a3a6a"), ("l", "#a33"), ("r", "#3a3")):
            xs = [p[1 + a] - c0 for p in cloth[t] if p[0] == g]
            ys = [p[2] for p in cloth[t] if p[0] == g]
            ax.scatter(xs, ys, s=3, c=col)
        for u, v in links:
            if u in j and v in j:
                ax.plot([j[u][a] - c0, j[v][a] - c0], [j[u][1], j[v][1]], "k-", lw=2)
        ax.set_xlim(-0.8, 0.8)
        ax.set_ylim(-0.05, 1.9)
        ax.set_aspect("equal")
        ax.set_title(f"{t} s, from the {'front' if a == 0 else 'side'}", fontsize=9)
plt.tight_layout()
out = os.path.join(S, name + ".cloth.png")
plt.savefig(out, dpi=70)
print(out)
