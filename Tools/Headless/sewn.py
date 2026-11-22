#!/usr/bin/env python3
"""sewn.py name [who] [zoom]: the sewn shirt and trousers of a run (SEWNPTS lines, FIGHT_CLOTHDUMP or the rack's dumps;
or a Unity log's clothdump lines) on the real body meshes placed by its PARTFRAMES lines, with true depth, from his own
front, three-quarters, side, back and behind, one row for each moment dumped, into $S/name.sewn.png. who: mover, other
or rack (default mover). zoom: neck, armpit, waist, arm or hem. Run from the repository root with python3 -I."""
import json, math, os, sys
import numpy as np

sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "Cloth"))
import garment

S = os.environ.get("S", "/tmp/pb")
name = sys.argv[1]
who = sys.argv[2] if len(sys.argv) > 2 else "mover"
zoom = sys.argv[3] if len(sys.argv) > 3 else None
doc = json.load(open("Assets/_Battler/Resources/Locomotion/Garments_Medium.json"))
tris = {g["name"]: np.array(g["tris"]).reshape(-1, 3) for g in doc["garments"]}
src = name if os.path.exists(name) else os.path.join(S, name + ".txt")
pts, frames = {}, {}
for line in open(src, errors="replace"):
    i = line.find("SEWNPTS ")
    j = line.find("PARTFRAMES ")
    if i >= 0:
        bits = line[i:].split()
        if bits[2] == who:
            pts.setdefault(bits[1], {})[bits[3]] = np.array([[float(c) for c in b.split(":")] for b in bits[4:] if b.count(":") == 2])
    elif j >= 0:
        bits = line[j:].split()
        if bits[2] == who:
            fr = {}
            for b in bits[3:]:
                k = b.split(":")
                if len(k) == 8:
                    fr[k[0]] = (np.array([float(c) for c in k[1:4]]), [float(c) for c in k[4:8]])
            frames[bits[1]] = fr
times = [t for t in sorted(pts, key=float) if t in frames]
if not times:
    sys.exit("no SEWNPTS lines for " + who)
from PIL import Image
rows = []
for t in times:
    fr = frames[t]
    layers = [(p, tris[g], "trousers" if g == "trousers" else "shirt") for g, p in pts[t].items() if g in tris and len(p) == tris[g].max() + 1]
    pel = fr["Pelvis"]
    across = fr["UpperArmR"][0] - fr["UpperArmL"][0]
    facing = math.atan2(-across[2], across[0])
    s = doc["sash"]
    sash = (s["lo"], s["hi"], s["a"], s["b"], s["cz"], pel[0], pel[1])
    out = os.path.join(S, f"_sewn_{t}.png")
    garment.draw(out, layers, title=f"{t} s, {who}", zoom=zoom, sash=sash, body_meshes_=garment.body_posed(fr),
                 centre_at=np.array([pel[0][0], pel[0][1] - 0.95 - 0.035, pel[0][2]]) if not zoom else np.array([pel[0][0], pel[0][1] - garment.PELVIS_Y, pel[0][2]]),
                 facing=facing)
    rows.append(Image.open(out))
W = max(r.width for r in rows)
sheet = Image.new("RGB", (W, sum(r.height for r in rows)), "white")
y = 0
for r in rows:
    sheet.paste(r, (0, y))
    y += r.height
outp = os.path.join(S, name.split("/")[-1].replace(".txt", "") + f".{who}" + (f".{zoom}" if zoom else "") + ".sewn.png")
sheet.save(outp)
print(outp)
