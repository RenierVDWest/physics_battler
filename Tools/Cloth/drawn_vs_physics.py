#!/usr/bin/env python3
"""drawn_vs_physics.py [size]: how far each body part's drawn mesh (Art/Bodies/<Size>/SM_<Size>_*.fbx) stands outside
its physics shape (Data/Rigs/<size>_rig.json), in the part's own space, X mirrored as Unity imports it.
Written 22 November 2026 for the plan "Clothes that never go into the body". Run from the repository root."""
import json, os, sys
import numpy as np
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from fbxread import read

size = sys.argv[1] if len(sys.argv) > 1 else "medium"
Size = size.capitalize()
rig = json.load(open(f"Assets/_Battler/Data/Rigs/{size}_rig.json"))


def outside(c, p):
    if c["type"] == "box":
        q = np.abs(p - np.array(c["center"])) - np.array(c["size"]) / 2
        return np.linalg.norm(np.maximum(q, 0), axis=1) + np.minimum(q.max(1), 0)
    if c["type"] == "sphere":
        return np.linalg.norm(p - np.array(c["center"]), axis=1) - c["radius"]
    a, b = np.array(c["start"]), np.array(c["end"])
    ab = b - a
    t = np.clip(((p - a) @ ab) / (ab @ ab), 0, 1)
    return np.linalg.norm(p - (a + t[:, None] * ab), axis=1) - c["radius"]


for part in rig["parts"]:
    v = read(f"Assets/_Battler/Art/Bodies/{Size}/{part['mesh']}.fbx")["Vertices"][0].reshape(-1, 3).copy()
    v[:, 0] *= -1  # Unity's FBX import mirrors X
    s = outside(part["collider"], v)
    print(f"{part['name']:11s} {100 * (s > 0.005).mean():3.0f}% of the drawn points more than 5 mm outside; "
          f"median {100 * np.median(s):5.1f} cm, 90th {100 * np.percentile(s, 90):5.1f} cm, most {100 * s.max():5.1f} cm")
