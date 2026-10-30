"""Checks that no two colliders of different rigidbodies come within 2 cm of each other in the rest
pose (the club counts as part of Hand_R's body), and that the feet stand on z = 0.

Run:  python3 check_colliders.py
"""

import numpy as np

import bpy  # noqa: F401  (provides mathutils for build_orc)
import build_orc as orc

CLEARANCE = 0.02
N = 4000


def surface_points(col):
    rng = np.random.default_rng(1)
    if col["type"] == "sphere":
        v = rng.normal(size=(N, 3))
        return np.array(col["center"]) + col["radius"] * v / np.linalg.norm(v, axis=1)[:, None]
    if col["type"] == "box":
        half = np.array(col["size"]) / 2
        p = rng.uniform(-1, 1, size=(N, 3))
        axis = rng.integers(0, 3, N)
        p[np.arange(N), axis] = np.sign(p[np.arange(N), axis])
        return np.array(col["center"]) + p * half
    a, b, r = np.array(col["start"]), np.array(col["end"]), col["radius"]
    t = rng.uniform(-0.3, 1.3, N)[:, None]
    v = rng.normal(size=(N, 3))
    v /= np.linalg.norm(v, axis=1)[:, None]
    centre = a + np.clip(t, 0, 1) * (b - a)
    return centre + r * v


def sdf(col, p):
    if col["type"] == "sphere":
        return np.linalg.norm(p - np.array(col["center"]), axis=1) - col["radius"]
    if col["type"] == "box":
        q = np.abs(p - np.array(col["center"])) - np.array(col["size"]) / 2
        return np.linalg.norm(np.maximum(q, 0), axis=1) + np.minimum(q.max(axis=1), 0)
    a, b, r = np.array(col["start"]), np.array(col["end"]), col["radius"]
    ab = b - a
    t = np.clip(((p - a) @ ab) / (ab @ ab), 0, 1)[:, None]
    return np.linalg.norm(p - (a + t * ab), axis=1) - r


def main():
    cols = orc.colliders()
    bodies = {part: [cols[part]] for part in orc.BODY_PARTS}
    grip = orc.P("Slot_Weapon_R")
    bodies["Hand_R"] += [orc.offset(c, grip) for c in orc.club_colliders()]  # compound on Hand_R

    names = list(bodies)
    worst = []
    for i, a in enumerate(names):
        for b in names[i + 1:]:
            d = min(sdf(cb, surface_points(ca)).min()
                    for ca in bodies[a] for cb in bodies[b])
            worst.append((d, a, b))
    worst.sort()
    print("Closest collider pairs (different bodies):")
    for d, a, b in worst[:8]:
        print(f"  {a:>11} - {b:<11} {d * 100:6.1f} cm {'OK' if d >= CLEARANCE else 'TOO CLOSE'}")
    bad = [w for w in worst if w[0] < CLEARANCE]
    print("All pairs clear by >= 2 cm" if not bad else f"{len(bad)} pairs too close")

    feet = [cols[f"Foot_{n}"] for n in "RL"]
    print("Foot box bottoms:", [round(f["center"].z - f["size"].z / 2, 4) for f in feet])
    return 1 if bad else 0


raise SystemExit(main())
