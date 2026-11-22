#!/usr/bin/env python3
"""outfit.py name [who] [zoom]: the shirt and trousers (OUTFITTUBE lines) over the real body meshes (the mesh files, placed
by the PARTFRAMES lines), seen from his own front, three-quarters, side and back, at each moment dumped (FIGHT_CLOTHDUMP),
into $S/name.outfit.png. who: mover or other (default mover). zoom: a part name to look closely at (e.g. ThighR).
Bare body shows where it is not covered or where it comes through. A picture, not a measure. Run from the repository root."""
import os, sys
import numpy as np
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d.art3d import Poly3DCollection

sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "Cloth"))
from fbxread import read

S = os.environ.get("S", "/tmp/pb")
name = sys.argv[1]
who = sys.argv[2] if len(sys.argv) > 2 else "mover"
zoom = sys.argv[3] if len(sys.argv) > 3 else None
MESH = {"Pelvis": "Pelvis", "Spine": "Spine", "Chest": "Chest", "Head": "Head", "UpperArmL": "UpperArm_L", "ForearmL": "Forearm_L",
        "HandL": "Hand_L", "UpperArmR": "UpperArm_R", "ForearmR": "Forearm_R", "HandR": "Hand_R", "ThighL": "Thigh_L", "ShinL": "Shin_L",
        "FootL": "Foot_L", "ThighR": "Thigh_R", "ShinR": "Shin_R", "FootR": "Foot_R"}
meshes = {}
for pid, file in MESH.items():
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
    if len(t) > 1800:  # (the hands and head: fewer faces, it is only a picture)
        t = t[:: int(np.ceil(len(t) / 1800))]
    meshes[pid] = (v, t)


def rot(q, v):
    x, y, z, w = q
    u = np.array([x, y, z])
    return v + 2.0 * np.cross(u, np.cross(u, v) + w * v)


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

try:
    import numba
    _jit = numba.njit(cache=False)
except ImportError:  # (slow without it, but right)
    def _jit(f):
        return f


@_jit
def _raster(tri, col, W, H, lo_u, lo_v, scale, img, depth):
    for k in range(tri.shape[0]):
        u0 = (tri[k, 0, 0] - lo_u) * scale; v0 = H - 1 - (tri[k, 0, 2] - lo_v) * scale; z0 = tri[k, 0, 1]
        u1 = (tri[k, 1, 0] - lo_u) * scale; v1 = H - 1 - (tri[k, 1, 2] - lo_v) * scale; z1 = tri[k, 1, 1]
        u2 = (tri[k, 2, 0] - lo_u) * scale; v2 = H - 1 - (tri[k, 2, 2] - lo_v) * scale; z2 = tri[k, 2, 1]
        area = (u1 - u0) * (v2 - v0) - (u2 - u0) * (v1 - v0)
        if abs(area) < 1e-9:
            continue
        a = int(max(0.0, min(u0, u1, u2))); b = int(min(W - 1.0, max(u0, u1, u2)) + 1)
        c = int(max(0.0, min(v0, v1, v2))); d = int(min(H - 1.0, max(v0, v1, v2)) + 1)
        for y in range(c, d):
            for x in range(a, b):
                px = x + 0.5; py = y + 0.5
                w0 = ((u1 - px) * (v2 - py) - (u2 - px) * (v1 - py)) / area
                w1 = ((u2 - px) * (v0 - py) - (u0 - px) * (v2 - py)) / area
                w2 = 1.0 - w0 - w1
                if w0 < 0 or w1 < 0 or w2 < 0:
                    continue
                z = w0 * z0 + w1 * z1 + w2 * z2
                if z < depth[y, x]:
                    depth[y, x] = z
                    img[y, x, 0] = col[k, 0]; img[y, x, 1] = col[k, 1]; img[y, x, 2] = col[k, 2]


def zrender(polys, cols, lo_u, hi_u, lo_v, hi_v, px=420):
    """A true depth-tested picture (matplotlib's 3D sorts whole faces and paints skin over cloth wrongly)."""
    tris, tc = [], []
    for P, c in zip(polys, cols):
        for j in range(1, len(P) - 1):
            tris.append([P[0], P[j], P[j + 1]])
            tc.append(c[:3])
    tri = np.array(tris, dtype=np.float64); col = np.array(tc, dtype=np.float64)
    scale = px / (hi_u - lo_u)
    W = px; H = int((hi_v - lo_v) * scale)
    img = np.ones((H, W, 3)); depth = np.full((H, W), 1e9)
    _raster(tri, col, W, H, lo_u, lo_v, scale, img, depth)
    return img

times = sorted(tubes, key=float)
if not times:
    sys.exit("no OUTFITTUBE lines for " + who)
colours = {"shirt": (0.55, 0.6, 0.72), "trousers": (0.2, 0.23, 0.35), "sash": (0.1, 0.09, 0.08), "skin": (0.95, 0.72, 0.5)}
views = (("front", 0), ("three-quarters", 40), ("side", 90), ("back", 180))
light = np.array([0.3, 0.6, 0.75])
light /= np.linalg.norm(light)
fig = plt.figure(figsize=(3.6 * len(views), 4.6 * len(times)))
for i, t in enumerate(times):
    fr = frames[t]
    pel = fr["Pelvis"][0]
    across = fr["UpperArmR"][0] - fr["UpperArmL"][0]
    across[1] = 0
    across /= np.linalg.norm(across) + 1e-9
    fwd = np.cross(across, [0, 1, 0])
    centre = fr[zoom][0] if zoom else pel
    span = 0.35 if zoom else 1.0
    world_tris, kinds = [], []
    for pid, (v, tt) in meshes.items():
        p, q = fr[pid]
        wv = np.array([p + rot(q, vv) for vv in v])
        for tri in tt:
            world_tris.append(wv[tri])
            kinds.append("skin")
    for tname, rings, cols, pts in tubes[t]:
        if any(h and h in tname for h in os.environ.get("HIDE", "").split(",")):
            continue
        kind = "sash" if tname == "sash" else "trousers" if "trouser" in tname else "shirt"
        for r in range(rings - 1):
            for c in range(cols):
                a, b = r * cols + c, r * cols + (c + 1) % cols
                world_tris.append(np.array([pts[a], pts[b], pts[b + cols], pts[a + cols]]))
                kinds.append(kind)
    for v_i, (label, turn) in enumerate(views):
        a = np.radians(turn)
        right = np.cos(a) * across - np.sin(a) * fwd
        ahead = np.cos(a) * fwd + np.sin(a) * across

        def to(pnt):
            d = pnt - centre
            return np.array([-(d @ right), -(d @ ahead), d[1]])
        polys, cols_ = [], []
        for poly, kind in zip(world_tris, kinds):
            P = np.array([to(pp) for pp in poly])
            n = np.cross(P[1] - P[0], P[2] - P[0])
            n /= np.linalg.norm(n) + 1e-12
            shade = 0.35 + 0.65 * abs(n @ np.array([light[0], -light[2], light[1]]))
            col = np.array(colours[kind]) * shade
            polys.append(P)
            cols_.append((*col, 1.0))
        ax = fig.add_subplot(len(times), len(views), i * len(views) + v_i + 1)
        if zoom:
            img = zrender(polys, cols_, -span, span, -span, span)
        else:
            img = zrender(polys, cols_, -0.6, 0.6, -1.0, 0.9)
        ax.imshow(img, interpolation="bilinear")
        ax.set_axis_off()
        ax.set_title(f"{float(t):.2f} s, {label}", fontsize=9)
plt.tight_layout()
out = os.path.join(S, name + (".zoom" if zoom else "") + ".outfit.png")
plt.savefig(out, dpi=80)
print(out)
