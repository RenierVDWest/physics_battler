#!/usr/bin/env python3
"""coverage.py name [who]: how much of the drawn body the shirt and trousers leave bare, from a run's dumps (OUTFITTUBE,
PARTFRAMES). For every vertex of the covered parts' mesh files, placed as drawn: hidden if it is inside another drawn
part (the pieces overlap at the joints); covered if a ray from it straight out (along the mesh's normal) meets a cloth
face within 20 cm; else bare (seen: not covered by design, as at the neck, wrists and ankles, or come through).
By part, with where the bare places are in the part's own space. Run from the repository root."""
import base64, json, os, sys
import numpy as np
from scipy.interpolate import RegularGridInterpolator
from scipy.spatial import cKDTree

sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "Cloth"))
from fbxread import read

S = os.environ.get("S", "/tmp/pb")
name = sys.argv[1]
who = sys.argv[2] if len(sys.argv) > 2 else "mover"
COVER = {"Pelvis": "Pelvis", "Spine": "Spine", "Chest": "Chest", "UpperArmL": "UpperArm_L", "ForearmL": "Forearm_L",
         "UpperArmR": "UpperArm_R", "ForearmR": "Forearm_R", "ThighL": "Thigh_L", "ShinL": "Shin_L", "ThighR": "Thigh_R", "ShinR": "Shin_R"}
fields = json.load(open("Assets/_Battler/Resources/Locomotion/BodyFields_Medium.json"))
interp = {}
for p in fields["parts"]:
    g = np.frombuffer(base64.b64decode(p["sdf"]), dtype="<i2").astype(float).reshape(p["nz"], p["ny"], p["nx"]) / 10000.0
    c = fields["cell"]
    axes = [p["min"][2] + c * np.arange(p["nz"]), p["min"][1] + c * np.arange(p["ny"]), p["min"][0] + c * np.arange(p["nx"])]
    interp[p["name"].replace("_", "")] = RegularGridInterpolator(axes, g, bounds_error=False, fill_value=1.0)
verts, normals = {}, {}
for pid, file in COVER.items():
    r = read(f"Assets/_Battler/Art/Bodies/Medium/SM_Medium_{file}.fbx")
    v = r["Vertices"][0].reshape(-1, 3).astype(float).copy()
    v[:, 0] *= -1
    tris, poly = [], []
    for i in r["PolygonVertexIndex"][0]:
        if i < 0:
            poly.append(~i)
            for k in range(1, len(poly) - 1):
                tris.append((poly[0], poly[k], poly[k + 1]))
            poly = []
        else:
            poly.append(i)
    t = np.array(tris)
    fn = np.cross(v[t[:, 1]] - v[t[:, 0]], v[t[:, 2]] - v[t[:, 0]])
    n = np.zeros_like(v)
    for k in range(3):
        np.add.at(n, t[:, k], fn)
    n /= np.linalg.norm(n, axis=1, keepdims=True) + 1e-12
    # (Mirrored X turns the faces' winding over: make the normals point away from the part's middle.)
    if np.mean(np.sum((v - v.mean(0)) * n, axis=1)) < 0:
        n = -n
    verts[pid], normals[pid] = v, n


def rot(q, v):
    u = q[:3]
    return v + 2.0 * np.cross(u, np.cross(u, v) + q[3] * v)


def unrot(q, v):
    return rot(np.array([-q[0], -q[1], -q[2], q[3]]), v)


tubes, frames = {}, {}
for line in open(os.path.join(S, name + ".txt"), errors="replace"):
    bits = line.split()
    if line.startswith("OUTFITTUBE ") and bits[2] == who:
        rings, cols = int(bits[4]), int(bits[5])
        pts = np.array([[float(c) for c in b.split(":")] for b in bits[6:6 + rings * cols]])
        tubes.setdefault(bits[1], []).append((bits[3], rings, cols, pts))
    if line.startswith("PARTFRAMES ") and bits[2] == who:
        fr = {}
        for b in bits[3:]:
            k = b.split(":")
            fr[k[0]] = (np.array([float(k[1]), float(k[2]), float(k[3])]), np.array([float(c) for c in k[4:8]]))
        frames[bits[1]] = fr
def ray_hits(o, dvec, A, B, C):
    """Moller-Trumbore, many triangles at once: True where the ray o + s d (0 < s) meets the triangle."""
    e1, e2 = B - A, C - A
    pv = np.cross(dvec, e2)
    det = np.einsum("ij,ij->i", e1, pv)
    ok = np.abs(det) > 1e-12
    inv = np.where(ok, 1.0 / np.where(ok, det, 1.0), 0.0)
    tv = o - A
    u = np.einsum("ij,ij->i", tv, pv) * inv
    qv = np.cross(tv, e1)
    v = (qv @ dvec) * inv
    s = np.einsum("ij,ij->i", e2, qv) * inv
    return ok & (u >= 0) & (v >= 0) & (u + v <= 1) & (s > 0) & (s < 0.2)


for t in sorted(tubes, key=float):
    fr = frames[t]
    TA, TB, TC = [], [], []
    for _, rings, cols, pts in tubes[t]:
        for r in range(rings - 1):
            for c in range(cols):
                a, b = r * cols + c, r * cols + (c + 1) % cols
                TA += [pts[a], pts[a]]; TB += [pts[b], pts[b + cols]]; TC += [pts[b + cols], pts[a + cols]]
    TA, TB, TC = np.array(TA), np.array(TB), np.array(TC)
    tree = cKDTree((TA + TB + TC) / 3)
    lines, total_bare, total = [], 0, 0
    for pid, v in verts.items():
        p, q = fr[pid]
        world = np.array([p + rot(q, vv) for vv in v])
        hidden = np.zeros(len(v), bool)
        for other in COVER:
            if other == pid:
                continue
            po, qo = fr[other]
            local = np.array([unrot(qo, w - po) for w in world])
            hidden |= interp[other](local[:, ::-1]) < -0.003
        nworld = np.array([rot(q, nn) for nn in normals[pid]])
        bare = np.zeros(len(v), bool)
        for k in np.where(~hidden)[0]:
            near = tree.query_ball_point(world[k] + nworld[k] * 0.05, 0.16)
            if not near:
                bare[k] = True
                continue
            near = np.array(near)
            bare[k] = not ray_hits(world[k] + nworld[k] * 0.001, nworld[k], TA[near], TB[near], TC[near]).any()
        total += int((~hidden).sum())
        total_bare += int(bare.sum())
        if bare.any():
            c = v[bare].mean(0)
            lines.append(f"  {pid:10s} bare {bare.sum():4d} of {(~hidden).sum():4d} seen; round about ({c[0]:+.2f}, {c[1]:+.2f}, {c[2]:+.2f}) in its own space")
    print(f"{t} s, {who}: bare {total_bare} of {total} seen body points ({100.0 * total_bare / max(1, total):.1f}%)")
    print("\n".join(lines))
