#!/usr/bin/env python3
"""strip.py name t0 t1 [frames] : stick figures of a headless run's two fighters between two times of its log
(the playtest log's pose files), from the side (above) and from over their heads (below), as one picture
$S/name.strip.png. To look at what a blow does without Unity: the bodies as the game drew them, the sword from grip
to tip. It is the log's own time (the events' first column), not the report's pace clock."""
import sys, os, gzip, csv, glob, math
from PIL import Image, ImageDraw

S = os.environ.get("S", "/tmp/pb")
name, t0, t1 = sys.argv[1], float(sys.argv[2]), float(sys.argv[3])
n = int(sys.argv[4]) if len(sys.argv) > 4 else 12
log = sorted(glob.glob(f"{S}/{name}/PlaytestLogs/*/"))[-1]
files = sorted(glob.glob(log + "units/*.pose.csv.gz"))[:2]

def load(f):
    with gzip.open(f, "rt") as h:
        return [{k: (float(v) if v not in ("", None) else 0.0) for k, v in r.items()} for r in csv.DictReader(h)]

units = [load(f) for f in files]
bones = [("Pelvis", "Spine"), ("Spine", "Chest"), ("Chest", "Head"), ("Chest", "UpperArmL"), ("UpperArmL", "ForearmL"), ("ForearmL", "HandL"),
         ("Chest", "UpperArmR"), ("UpperArmR", "ForearmR"), ("ForearmR", "HandR"), ("Pelvis", "ThighL"), ("ThighL", "ShinL"), ("ShinL", "FootL"),
         ("Pelvis", "ThighR"), ("ThighR", "ShinR"), ("ShinR", "FootR")]

def at(rows, t):
    return min(rows, key=lambda r: abs(r["t"] - t))

def p(r, part):
    return (r[part + ".x"], r[part + ".y"], r[part + ".z"])

a0, b0 = at(units[0], t0), at(units[1], t0)
ax, _, az = p(a0, "Pelvis"); bx, _, bz = p(b0, "Pelvis")
d = math.hypot(bx - ax, bz - az) or 1.0
ux, uz = (bx - ax) / d, (bz - az) / d          # along the line from the first to the second
vx, vz = -uz, ux                               # to its left
cx, cz = (ax + bx) / 2, (az + bz) / 2
W, H, sc = 330, 520, 100.0
per = 6                                        # panels a row
rows_n = (n + per - 1) // per
img = Image.new("RGB", (W * per, H * rows_n), (250, 248, 242))
dr = ImageDraw.Draw(img)
cols = [(200, 110, 30), (60, 110, 190)]
for i in range(n):
    t = t0 + (t1 - t0) * i / max(1, n - 1)
    ox, oy = (i % per) * W, (i // per) * H
    dr.line([(ox, oy), (ox, oy + H)], fill=(215, 215, 210))
    dr.line([(ox, oy + 250), (ox + W, oy + 250)], fill=(150, 150, 150))
    dr.line([(ox, oy + H - 1), (ox + W, oy + H - 1)], fill=(215, 215, 210))
    dr.text((ox + 6, oy + 4), f"{t:.2f} s", fill=(40, 40, 40))
    for u, rows in enumerate(units):
        r = at(rows, t)
        def side(q):
            along = (q[0] - cx) * ux + (q[2] - cz) * uz
            return (ox + W / 2 + along * sc, oy + 250 - q[1] * sc)
        def top(q):
            along = (q[0] - cx) * ux + (q[2] - cz) * uz
            left = (q[0] - cx) * vx + (q[2] - cz) * vz
            return (ox + W / 2 + along * sc, oy + 390 - left * sc)
        for view in (side, top):
            for a, b in bones:
                dr.line([view(p(r, a)), view(p(r, b))], fill=cols[u], width=3)
            h = view(p(r, "Head"))
            dr.ellipse([h[0] - 7, h[1] - 12, h[0] + 7, h[1] + 2], outline=cols[u], width=2)
            if abs(r.get("tip.x", 0)) + abs(r.get("tip.y", 0)) > 1e-6:
                dr.line([view(p(r, "grip")), view(p(r, "tip"))], fill=(20, 20, 20), width=3)
out = f"{S}/{name}.strip.png"
img.save(out)
print(out, log)
