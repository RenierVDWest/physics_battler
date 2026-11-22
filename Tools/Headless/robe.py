#!/usr/bin/env python3
"""robe.py name [who]: the robe (CLOTHPTS, 36 columns by 24 rows, the sash at row 6) drawn as shaded cloth over the body's
joints (BODYPTS), seen from his own front, three-quarters and side, at each moment dumped (FIGHT_CLOTHDUMP), into
$S/name.robe.png. who: mover or other (default mover). A look at the shape, not a measure."""
import os, sys
import numpy as np
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d.art3d import Poly3DCollection

S = os.environ.get("S", "/tmp/pb")
name = sys.argv[1]
who = sys.argv[2] if len(sys.argv) > 2 else "mover"
C, R, SASH, PANEL = 36, 24, 6, 9
cloth, body = {}, {}
for line in open(os.path.join(S, name + ".txt"), errors="replace"):
    for tag, into in (("CLOTHPTS ", cloth), ("BODYPTS ", body)):
        if line.startswith(tag):
            bits = line.split()
            if bits[2] != who:
                continue
            pts = []
            for b in bits[3:]:
                k, x, y, z = b.split(":")
                pts.append((k, float(x), float(y), float(z)))
            into[bits[1]] = pts
times = sorted(cloth, key=float)
if not times:
    sys.exit("no CLOTHPTS lines for " + who)
links = [("Pelvis", "Spine"), ("Spine", "Chest"), ("Chest", "Head"), ("Chest", "UpperArmL"), ("UpperArmL", "ForearmL"), ("ForearmL", "HandL"),
         ("Chest", "UpperArmR"), ("UpperArmR", "ForearmR"), ("ForearmR", "HandR"), ("Pelvis", "ThighL"), ("ThighL", "ShinL"), ("ShinL", "FootL"),
         ("Pelvis", "ThighR"), ("ThighR", "ShinR"), ("ShinR", "FootR")]


def armhole(r, c):
    return r < SASH and (c == PANEL - 1 or c == C - PANEL - 1)


views = (("front", 0), ("three-quarters", 40), ("side", 90))
fig = plt.figure(figsize=(3.4 * len(views), 3.9 * len(times)))
light = np.array([0.4, 0.8, 0.45]); light /= np.linalg.norm(light)
for i, t in enumerate(times):
    P = np.array([(x, y, z) for _, x, y, z in cloth[t]])
    if len(P) != C * R:
        continue
    j = {k: np.array((x, y, z)) for k, x, y, z in body.get(t, [])}
    pel = j.get("Pelvis", P.mean(0))
    # (His facing: the shoulders' line turned a quarter about the up axis.)
    across = j["UpperArmR"] - j["UpperArmL"] if "UpperArmR" in j else np.array([1.0, 0, 0])
    across[1] = 0
    across /= np.linalg.norm(across) + 1e-9
    fwd = np.cross(across, [0, 1, 0])
    for v, (label, turn) in enumerate(views):
        a = np.radians(turn)
        rx = np.cos(a) * across - np.sin(a) * fwd  # screen right
        def to(p):
            d = p - pel
            # (Seen from in front of him: his right on the left of the picture, as a man facing you.)
            return np.array([-(d @ rx), -(d @ (np.cos(a) * fwd + np.sin(a) * across)), p[1]])
        Q = np.array([to(p) for p in P])
        polys, shades = [], []
        for r in range(R - 1):
            for c in range(C - 1):
                if armhole(r, c):
                    continue
                quad = [Q[r * C + c], Q[r * C + c + 1], Q[(r + 1) * C + c + 1], Q[(r + 1) * C + c]]
                n = np.cross(quad[1] - quad[0], quad[3] - quad[0])
                n /= np.linalg.norm(n) + 1e-9
                # (Its outward side: away from his middle. Seen from inside (the camera is at -y), it is the lining.)
                mid = sum(quad) / 4 - to(pel)
                if n[0] * mid[0] + n[1] * mid[1] < 0:
                    n = -n
                polys.append(quad)
                shades.append((0.35 + 0.65 * abs(n @ light), n[1] > 0))
        ax = fig.add_subplot(len(times), len(views), i * len(views) + v + 1, projection="3d")
        cols = [((0.75 * s, 0.72 * s, 0.6 * s, 0.95) if inside else (0.2 * s, 0.26 * s, 0.55 * s, 0.95)) for s, inside in shades]
        ax.add_collection3d(Poly3DCollection(polys, facecolors=cols, edgecolors="none"))
        for p, q in links:
            if p in j and q in j:
                A, B = to(j[p]), to(j[q])
                ax.plot([A[0], B[0]], [A[1], B[1]], [A[2], B[2]], color="#d9a066", lw=4, zorder=0)
        if "Head" in j:
            H = to(j["Head"])
            ax.scatter([H[0]], [H[1]], [H[2] + 0.1], s=250, color="#d9a066")
        ax.view_init(elev=8, azim=-90)
        ax.set_xlim(-0.6, 0.6); ax.set_ylim(-0.6, 0.6); ax.set_zlim(0, 1.8)
        ax.set_box_aspect((1.2, 1.2, 1.8))
        ax.set_axis_off()
        ax.set_title(f"{float(t):.2f} s, {label}", fontsize=9)
plt.tight_layout()
out = os.path.join(S, name + ".robe.png")
plt.savefig(out, dpi=80)
print(out)
