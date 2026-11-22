#!/usr/bin/env python3
"""bake_body.py [size]: the drawn body baked for the clothes (the plan "Clothes that never go into the body", step 1).

For each body part of a size: its drawn mesh (Art/Bodies/<Size>/SM_<Size>_<Part>.fbx, X mirrored as Unity imports it)
in the part's own space, and from it a signed distance grid: how far each grid point is from the drawn surface,
negative inside. Distances from dense samples of the surface (a k-d tree), inside or outside from rays along X through
the triangles (the mesh pieces are closed). Written to Assets/_Battler/Resources/Locomotion/BodyFields_<Size>.json for
the game (Resources) and the headless tests (read straight from the file), with the triangles for the exact check.
Run from the repository root."""
import base64, json, os, sys
import numpy as np
from scipy.spatial import cKDTree

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from fbxread import read

size = sys.argv[1] if len(sys.argv) > 1 else "medium"
Size = size.capitalize()
CELL = 0.008      # metres a grid cell
MARGIN = 0.04     # metres of grid around the mesh
SAMPLE = 0.0015   # metres between surface samples
COVERED = {"Pelvis", "Spine", "Chest", "UpperArm", "Forearm", "Thigh", "Shin"}
# (The drawn pieces are separate closed meshes with a slot between them at each joint: a cloth point that slipped into
# that slot was outside every piece, and a trouser leg's top could fold in through it and show a band of skin (22
# November). So each limb part's field is joined with a ball at its joint, the size of the limb's own top, which lies
# inside the body as drawn.)
BALL = {"UpperArm", "Forearm", "Thigh", "Shin"}
BALL_TOP = (0.03, 0.09)  # metres down the limb from its joint where its top's narrowest round is taken
BALL_OF = 0.92            # the ball's radius as a share of that
rig = json.load(open(f"Assets/_Battler/Data/Rigs/{size}_rig.json"))


def mesh_of(name):
    r = read(f"Assets/_Battler/Art/Bodies/{Size}/{name}.fbx")
    v = r["Vertices"][0].reshape(-1, 3).astype(np.float64).copy()
    v[:, 0] *= -1.0
    idx = r["PolygonVertexIndex"][0]
    tris, poly = [], []
    for i in idx:
        if i < 0:
            poly.append(~i)
            for k in range(1, len(poly) - 1):
                tris.append((poly[0], poly[k], poly[k + 1]))
            poly = []
        else:
            poly.append(i)
    return v, np.array(tris, dtype=np.int64)


def samples(v, t):
    a, b, c = v[t[:, 0]], v[t[:, 1]], v[t[:, 2]]
    pts = [a, b, c]
    for i in range(len(t)):
        l = max(np.linalg.norm(b[i] - a[i]), np.linalg.norm(c[i] - a[i]), np.linalg.norm(c[i] - b[i]))
        n = int(np.ceil(l / SAMPLE)) + 1
        u, w = np.meshgrid(np.linspace(0, 1, n), np.linspace(0, 1, n))
        keep = (u + w) <= 1.0
        u, w = u[keep], w[keep]
        pts.append(a[i] + u[:, None] * (b[i] - a[i]) + w[:, None] * (c[i] - a[i]))
    return np.vstack(pts)


def inside_grid(v, t, ys, zs, xs):
    """True where a grid point is inside: rays along +X from each (y, z), crossings counted."""
    a, b, c = v[t[:, 0]], v[t[:, 1]], v[t[:, 2]]
    inside = np.zeros((len(zs), len(ys), len(xs)), dtype=bool)
    # (A tiny offset off the grid's rows keeps rays off edges and vertices.)
    eps_y, eps_z = 1.234e-5, 2.345e-5
    ay, az, by_, bz, cy, cz = a[:, 1], a[:, 2], b[:, 1], b[:, 2], c[:, 1], c[:, 2]
    for iz, z0 in enumerate(zs):
        for iy, y0 in enumerate(ys):
            y, z = y0 + eps_y, z0 + eps_z
            d = (by_ - ay) * (cz - az) - (bz - az) * (cy - ay)
            ok = np.abs(d) > 1e-14
            l1 = ((by_ - y) * (cz - z) - (bz - z) * (cy - y))
            l2 = ((cy - y) * (az - z) - (cz - z) * (ay - y))
            l3 = ((ay - y) * (bz - z) - (az - z) * (by_ - y))
            s = np.sign(d)
            hit = ok & (l1 * s >= 0) & (l2 * s >= 0) & (l3 * s >= 0)
            if not hit.any():
                continue
            w1, w2, w3 = l1[hit] / d[hit], l2[hit] / d[hit], l3[hit] / d[hit]
            xh = np.sort(w1 * a[hit, 0] + w2 * b[hit, 0] + w3 * c[hit, 0])
            count = np.searchsorted(xh, xs, side="right")
            inside[iz, iy, :] = (len(xh) - count) % 2 == 1
    return inside


out = {"body": Size, "cell": CELL, "parts": []}
report = []
for part in rig["parts"]:
    v, t = mesh_of(part["mesh"])
    # (Closed? every edge in exactly two triangles.)
    e = np.sort(np.vstack([t[:, [0, 1]], t[:, [1, 2]], t[:, [2, 0]]]), axis=1)
    _, counts = np.unique(e, axis=0, return_counts=True)
    open_edges = int((counts != 2).sum())
    lo, hi = v.min(0) - MARGIN, v.max(0) + MARGIN
    ball = 0.0
    tree = cKDTree(samples(v, t))
    if part["name"].split("_")[0] in BALL:
        # (The limb's top: at heights down from the joint, the narrowest way out from its middle to the surface.)
        way = -1.0 if np.mean(v[:, 1]) < 0 else 1.0
        best = 1.0
        for h in np.linspace(BALL_TOP[0], BALL_TOP[1], 7):
            sl = v[np.abs(v[:, 1] - way * h) < 0.03]
            if len(sl) < 3:
                continue
            mid = sl.mean(0)
            for a in np.linspace(0, 2 * np.pi, 48, endpoint=False):
                d = np.array([np.cos(a), 0.0, np.sin(a)])
                # (From outside in, to the first point inside: the limb's outer skin. Its top is a hollow socket, so
                # the middle itself may be outside.)
                rays = mid + d[None, :] * np.arange(0.2, 0.0, -0.002)[:, None]
                rays[:, 1] = way * h
                r = 0.0
                for q in rays:
                    if inside_grid(v, t, [q[1]], [q[2]], [q[0]])[0, 0, 0]:
                        r = float(np.linalg.norm((q - mid)[[0, 2]]))
                        break
                if r > 0:
                    best = min(best, r)
        ball = BALL_OF * best if best < 1.0 else 0.0
        lo, hi = np.minimum(lo, -ball - MARGIN), np.maximum(hi, ball + MARGIN)
    n = np.ceil((hi - lo) / CELL).astype(int) + 1
    xs, ys, zs = [lo[k] + CELL * np.arange(n[k]) for k in range(3)]
    Z, Y, X = np.meshgrid(zs, ys, xs, indexing="ij")
    grid = np.stack([X.ravel(), Y.ravel(), Z.ravel()], axis=1)
    dist, _ = tree.query(grid, workers=-1)
    inside = inside_grid(v, t, ys, zs, xs).ravel()
    sdf = np.where(inside, -dist, dist)
    if ball > 0:
        sdf = np.minimum(sdf, np.linalg.norm(grid, axis=1) - ball)
    q = np.clip(np.round(sdf * 10000.0), -32767, 32767).astype("<i2")  # tenths of a millimetre
    out["parts"].append({
        "name": part["name"], "min": [float(x) for x in lo], "nx": int(n[0]), "ny": int(n[1]), "nz": int(n[2]),
        "sdf": base64.b64encode(q.tobytes()).decode(),
        # (The triangles for the exact check: only the parts clothes cover.)
        "tris": base64.b64encode(v[t].astype("<f4").tobytes()).decode() if part["name"].split("_")[0] in COVERED else "",
        "ball": float(ball), "meshMin": [float(x) for x in v.min(0)], "meshMax": [float(x) for x in v.max(0)],
    })
    report.append(f"{part['name']:11s} {len(t)} triangles, open edges {open_edges}, grid {n[0]}x{n[1]}x{n[2]}, inside {100 * inside.mean():.0f}% of cells, deepest {100 * -sdf.min():.1f} cm" + (f", joint ball {100 * ball:.1f} cm" if ball else ""))

path = f"Assets/_Battler/Resources/Locomotion/BodyFields_{Size}.json"
json.dump(out, open(path, "w"), separators=(",", ":"))
print("\n".join(report))
print(path, os.path.getsize(path) // 1024, "KB")
