#!/usr/bin/env python3
"""drape.py [pattern.json] [out.json]: step 2 of the plan "Clothes cut and sewn like real clothes" (22 November): the
flat pieces sewn and draped on the drawn body, once, as Marvelous Designer does.

In: GarmentPattern_<Size>.json (pattern.py: the pieces flat, their triangles, the seams, and each piece put round the
body with the arms raised 60 degrees) and BodyFields_<Size>.json (the drawn body as distance grids, bake_body.py).

1. The trousers: the seams pull their pairs of points together with gravity off, then gravity, then the seams are
   welded (each pair made one point) and it settles.
2. The shirt the same, arms raised; then the arms are lowered to the rest pose over a second; the sash gathers it at
   the waist; it is kept outside the trousers (now still) and off itself.

The cloth (XPBD in small steps): each thread kept to its length on the flat pattern, hard against stretching and soft
against squashing (so spare cloth buckles into folds); a soft bend (the distance across each pair of triangles, kept
no shorter than laid flat); the body pushing it out to 5 mm, with friction. Every point weighs the same.

Out: Garments_<Size>.json (points in the hips' space, triangles, threads and bends with their lengths, each point's
piece, flat place and distance along the cloth from where it is held) and the measures, printed and in the file.
Levers: DRAPE_<NAME>=value (see LEVERS). DRAPE_SNAP=folder draws each stage there (garment.py); DRAPE_DEBUG=1 prints the
widest seams, longest threads and the hems after each stage. About 75 s. Run from the repository root with python3 -I."""
import base64, json, math, os, sys, time
import numpy as np
from numba import njit
from scipy.sparse import coo_matrix
from scipy.sparse.csgraph import dijkstra

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

SIZE = "Medium"
PELVIS_Y = 0.985
PAT = sys.argv[1] if len(sys.argv) > 1 else f"Assets/_Battler/Resources/Locomotion/GarmentPattern_{SIZE}.json"
OUT = sys.argv[2] if len(sys.argv) > 2 else f"Assets/_Battler/Resources/Locomotion/Garments_{SIZE}.json"

LEVERS = dict(
    SUB=16,            # small steps a picture (60 pictures a second)
    ITER=12,           # times the threads, bends and seams are gone over in a small step
    SQUASH=1e-4,       # give against squashing (XPBD compliance, every point weighing 1)
    BEND=3e-4,         # give against bending
    BANDBEND=3e-5,     # the neck band's and waistband's (a stiffer fabric)
    SEWPULL=1e-6,      # give of a seam while sewing
    SEWSTEP=0.0006,    # metres a seam pair may close in a small step at most
    THICK=0.008,       # metres the body keeps the cloth off itself (5 mm let the drawn body show through at sharp places: its 8 mm grid rounds them)
    FRICTION=0.15,     # share of the sliding along the body taken away in a small step
    OVER=0.008,        # metres the trousers keep the shirt off them
    SELF=0.012,        # metres two points of the shirt keep apart (not near each other along the cloth)
    SASH_LO=1.04, SASH_HI=1.12,   # metres up the sash
    SASHGAP=0.012,     # metres the sash sits outside the waistband
    DRAWGAP=0.018,     # metres the tied drawstring lets the waistband stand off his waist
    DRAW_LO=1.065,     # metres up the waistband's lowest points stay
)
for k in LEVERS:
    if "DRAPE_" + k in os.environ:
        LEVERS[k] = float(os.environ["DRAPE_" + k])
L = LEVERS
HZ = 60.0

# ---- The drawn body ---------------------------------------------------------------------------------------------------
rig = json.load(open(f"Assets/_Battler/Data/Rigs/{SIZE.lower()}_rig.json"))
nulls = {n["name"]: n for n in rig["nulls"]}


def world(name):
    p = np.zeros(3)
    n = nulls[name]
    while n:
        p += np.array(n["localPosition"])
        n = nulls.get(n["parent"]) if n["parent"] else None
    return p


bf = json.load(open(f"Assets/_Battler/Resources/Locomotion/BodyFields_{SIZE}.json"))
CELL = float(bf["cell"])
meta = np.zeros((len(bf["parts"]), 11))
grids = []
off = 0
for k, p in enumerate(bf["parts"]):
    q = np.frombuffer(base64.b64decode(p["sdf"]), dtype="<i2").astype(np.float64) / 10000.0
    w = world(p["name"])
    base = p["name"].split("_")[0]
    side = 0.0
    if base in ("UpperArm", "Forearm", "Hand"):
        side = 1.0 if p["name"].endswith("_R") else -1.0
    meta[k] = (*p["min"], p["nx"], p["ny"], p["nz"], off, *w, side)
    grids.append(q)
    off += len(q)
SDF = np.concatenate(grids)
PART_NAMES = [p["name"] for p in bf["parts"]]
SH = world("UpperArm_R")
rest_arm = world("Hand_R") - world("UpperArm_R")
REST_DEG = math.degrees(math.atan2(rest_arm[0], -rest_arm[1]))


@njit(cache=True, fastmath=True)
def body_dist(px, py, pz, meta, sdf, cell, ca, sa, shx, shy):
    """The drawn body's signed distance at a point (negative inside) and its way out, with the arms raised by the turn
    whose cosine and sine are ca, sa (about the shoulder, out to the side)."""
    best = 1e3
    gx = 0.0
    gy = 1.0
    gz = 0.0
    for k in range(meta.shape[0]):
        side = meta[k, 10]
        x = px
        y = py
        z = pz
        if side != 0.0:
            dx = x - side * shx
            dy = y - shy
            s = -side * sa
            x = side * shx + dx * ca - dy * s
            y = shy + dx * s + dy * ca
        fx = (x - meta[k, 7] - meta[k, 0]) / cell
        fy = (y - meta[k, 8] - meta[k, 1]) / cell
        fz = (z - meta[k, 9] - meta[k, 2]) / cell
        nx = int(meta[k, 3])
        ny = int(meta[k, 4])
        nz = int(meta[k, 5])
        if fx < 0.0 or fy < 0.0 or fz < 0.0 or fx >= nx - 1 or fy >= ny - 1 or fz >= nz - 1:
            continue
        ix = int(fx)
        iy = int(fy)
        iz = int(fz)
        tx = fx - ix
        ty = fy - iy
        tz = fz - iz
        i0 = int(meta[k, 6]) + (iz * ny + iy) * nx + ix
        c000 = sdf[i0]
        c100 = sdf[i0 + 1]
        c010 = sdf[i0 + nx]
        c110 = sdf[i0 + nx + 1]
        i1 = i0 + nx * ny
        c001 = sdf[i1]
        c101 = sdf[i1 + 1]
        c011 = sdf[i1 + nx]
        c111 = sdf[i1 + nx + 1]
        c00 = c000 + (c100 - c000) * tx
        c10 = c010 + (c110 - c010) * tx
        c01 = c001 + (c101 - c001) * tx
        c11 = c011 + (c111 - c011) * tx
        c0 = c00 + (c10 - c00) * ty
        c1 = c01 + (c11 - c01) * ty
        d = c0 + (c1 - c0) * tz
        if d < best:
            best = d
            ux = ((c100 - c000) * (1 - ty) * (1 - tz) + (c110 - c010) * ty * (1 - tz) + (c101 - c001) * (1 - ty) * tz + (c111 - c011) * ty * tz)
            uy = (c10 - c00) * (1 - tz) + (c11 - c01) * tz
            uz = c1 - c0
            if side != 0.0:
                s2 = side * sa
                gx = ux * ca - uy * s2
                gy = ux * s2 + uy * ca
            else:
                gx = ux
                gy = uy
            gz = uz
    n = math.sqrt(gx * gx + gy * gy + gz * gz)
    if n > 1e-12:
        gx /= n
        gy /= n
        gz /= n
    return best, gx, gy, gz


# ---- One picture of cloth ---------------------------------------------------------------------------------------------
@njit(cache=True, fastmath=True)
def _closest(px, py, pz, t):
    """The closest point to p on triangle t (3x3)."""
    ax, ay, az = t[0, 0], t[0, 1], t[0, 2]
    abx, aby, abz = t[1, 0] - ax, t[1, 1] - ay, t[1, 2] - az
    acx, acy, acz = t[2, 0] - ax, t[2, 1] - ay, t[2, 2] - az
    apx, apy, apz = px - ax, py - ay, pz - az
    d1 = abx * apx + aby * apy + abz * apz
    d2 = acx * apx + acy * apy + acz * apz
    if d1 <= 0 and d2 <= 0:
        return ax, ay, az
    bpx, bpy, bpz = px - t[1, 0], py - t[1, 1], pz - t[1, 2]
    d3 = abx * bpx + aby * bpy + abz * bpz
    d4 = acx * bpx + acy * bpy + acz * bpz
    if d3 >= 0 and d4 <= d3:
        return t[1, 0], t[1, 1], t[1, 2]
    vc = d1 * d4 - d3 * d2
    if vc <= 0 and d1 >= 0 and d3 <= 0:
        v = d1 / (d1 - d3)
        return ax + v * abx, ay + v * aby, az + v * abz
    cpx, cpy, cpz = px - t[2, 0], py - t[2, 1], pz - t[2, 2]
    d5 = abx * cpx + aby * cpy + abz * cpz
    d6 = acx * cpx + acy * cpy + acz * cpz
    if d6 >= 0 and d5 <= d6:
        return t[2, 0], t[2, 1], t[2, 2]
    vb = d5 * d2 - d1 * d6
    if vb <= 0 and d2 >= 0 and d6 <= 0:
        w = d2 / (d2 - d6)
        return ax + w * acx, ay + w * acy, az + w * acz
    va = d3 * d6 - d5 * d4
    if va <= 0 and (d4 - d3) >= 0 and (d5 - d6) >= 0:
        w = (d4 - d3) / ((d4 - d3) + (d5 - d6))
        return t[1, 0] + w * (t[2, 0] - t[1, 0]), t[1, 1] + w * (t[2, 1] - t[1, 1]), t[1, 2] + w * (t[2, 2] - t[1, 2])
    den = 1.0 / (va + vb + vc)
    v = vb * den
    w = vc * den
    return ax + abx * v + acx * w, ay + aby * v + acy * w, az + abz * v + acz * w


@njit(cache=True, fastmath=True)
def _excluded(p, q, ex_start, ex):
    lo = ex_start[p]
    hi = ex_start[p + 1] - 1
    while lo <= hi:
        m = (lo + hi) >> 1
        if ex[m] == q:
            return True
        if ex[m] < q:
            lo = m + 1
        else:
            hi = m - 1
    return False


@njit(cache=True, fastmath=True)
def picture(x, prev, nsub, g, keep, E, EL, ct, cc, B, BL, BC, S, sc, smax,
            meta, sdf, cell, ang_a, ang_b, shx, shy, thick, mu, floor_y,
            rings, ring_mask, otri, on, ostart, oitems, olo, ocell, odims, othick,
            self_on, srad, ex_start, ex, iters, piece):
    """One picture (1/60 s) of nsub small steps. x, prev: (n, 3). The arms' extra raise goes from ang_a to ang_b
    (radians) over the picture."""
    n = x.shape[0]
    h = 1.0 / 60.0 / nsub
    kp = keep ** (1.0 / nsub)
    gh = g * h * h
    start = np.empty_like(x)
    contact = np.zeros(n, dtype=np.int8)
    nrm = np.zeros((n, 3))
    tsize = 1 << 16
    cstart = np.zeros(tsize + 1, dtype=np.int64)
    citems = np.empty(n, dtype=np.int64)
    keys = np.empty(n, dtype=np.int64)
    for s in range(nsub):
        ang = ang_a + (ang_b - ang_a) * (s + 1.0) / nsub
        ca = math.cos(ang)
        sa = math.sin(ang)
        for i in range(n):
            start[i, 0] = x[i, 0]
            start[i, 1] = x[i, 1]
            start[i, 2] = x[i, 2]
            vx = (x[i, 0] - prev[i, 0]) * kp
            vy = (x[i, 1] - prev[i, 1]) * kp
            vz = (x[i, 2] - prev[i, 2]) * kp
            prev[i, 0] = x[i, 0]
            prev[i, 1] = x[i, 1]
            prev[i, 2] = x[i, 2]
            x[i, 0] += vx
            x[i, 1] += vy - gh
            x[i, 2] += vz
        # Threads: hard against stretching, soft against squashing.
        at = ct / (h * h)
        ac = cc / (h * h)
        for it in range(iters):   # (Gauss-Seidel sweeps: one alone let the waistband stretch 25% under the trousers' weight)
            for e in range(E.shape[0]):
                i = E[e, 0]
                j = E[e, 1]
                dx = x[j, 0] - x[i, 0]
                dy = x[j, 1] - x[i, 1]
                dz = x[j, 2] - x[i, 2]
                l = math.sqrt(dx * dx + dy * dy + dz * dz)
                if l < 1e-9:
                    continue
                c = l - EL[e]
                a = at if c > 0 else ac
                dl = -c / (2.0 + a) / l
                x[i, 0] -= dl * dx
                x[i, 1] -= dl * dy
                x[i, 2] -= dl * dz
                x[j, 0] += dl * dx
                x[j, 1] += dl * dy
                x[j, 2] += dl * dz
            # Bends: never shorter than laid flat, softly.
            for b in range(B.shape[0]):
                i = B[b, 0]
                j = B[b, 1]
                dx = x[j, 0] - x[i, 0]
                dy = x[j, 1] - x[i, 1]
                dz = x[j, 2] - x[i, 2]
                l = math.sqrt(dx * dx + dy * dy + dz * dz)
                if l < 1e-9:
                    continue
                c = l - BL[b]
                if c > 0:
                    continue
                a = BC[b] / (h * h)
                dl = -c / (2.0 + a) / l
                x[i, 0] -= dl * dx
                x[i, 1] -= dl * dy
                x[i, 2] -= dl * dz
                x[j, 0] += dl * dx
                x[j, 1] += dl * dy
                x[j, 2] += dl * dz
            # Seams: their pairs pulled together, a little a small step.
            a = sc / (h * h)
            for k in range(S.shape[0]):
                i = S[k, 0]
                j = S[k, 1]
                dx = x[j, 0] - x[i, 0]
                dy = x[j, 1] - x[i, 1]
                dz = x[j, 2] - x[i, 2]
                l = math.sqrt(dx * dx + dy * dy + dz * dz)
                if l < 1e-9:
                    continue
                m = l / (2.0 + a)
                if m > smax:
                    m = smax
                f = m / l
                x[i, 0] += f * dx
                x[i, 1] += f * dy
                x[i, 2] += f * dz
                x[j, 0] -= f * dx
                x[j, 1] -= f * dy
                x[j, 2] -= f * dz
        # Rings: the sash round the waist the shirt is gathered into, the trousers' drawstring tied. Ring k holds the
        # points marked k + 1 that are between its heights inside its ellipse (and, if it holds, no lower).
        for k in range(rings.shape[0]):
            if rings[k, 0] <= 0:
                continue
            for i in range(n):
                if ring_mask[i] != k + 1 or x[i, 1] > rings[k, 2]:
                    continue
                if x[i, 1] < rings[k, 1]:
                    if rings[k, 7] <= 0:
                        continue
                    x[i, 1] = rings[k, 1]   # (a ring that holds its points up: the drawstring on his hips)
                ex_ = (x[i, 0] - rings[k, 5]) / rings[k, 3]
                ez_ = (x[i, 2] - rings[k, 6]) / rings[k, 4]
                r2 = ex_ * ex_ + ez_ * ez_
                if r2 > 1.0:
                    f = 1.0 / math.sqrt(r2)
                    x[i, 0] = rings[k, 5] + (x[i, 0] - rings[k, 5]) * f
                    x[i, 2] = rings[k, 6] + (x[i, 2] - rings[k, 6]) * f
        # Off itself: points of two pieces (a sleeve against the shirt's side) that are not near each other along the
        # cloth keep srad apart. (Within a piece it lifted the front's hem 10 cm: its own folds pushing apart.)
        if self_on > 0:
            for t in range(tsize + 1):
                cstart[t] = 0
            for i in range(n):
                cx = int(math.floor(x[i, 0] / srad))
                cy = int(math.floor(x[i, 1] / srad))
                cz = int(math.floor(x[i, 2] / srad))
                kk = ((cx * 73856093) ^ (cy * 19349663) ^ (cz * 83492791)) & (tsize - 1)
                keys[i] = kk
                cstart[kk + 1] += 1
            for t in range(tsize):
                cstart[t + 1] += cstart[t]
            fill = cstart.copy()
            for i in range(n):
                citems[fill[keys[i]]] = i
                fill[keys[i]] += 1
            r2 = srad * srad
            for i in range(n):
                cx = int(math.floor(x[i, 0] / srad))
                cy = int(math.floor(x[i, 1] / srad))
                cz = int(math.floor(x[i, 2] / srad))
                for ox in range(-1, 2):
                    for oy in range(-1, 2):
                        for oz in range(-1, 2):
                            kk = (((cx + ox) * 73856093) ^ ((cy + oy) * 19349663) ^ ((cz + oz) * 83492791)) & (tsize - 1)
                            for t in range(cstart[kk], cstart[kk + 1]):
                                j = citems[t]
                                if j <= i or piece[j] == piece[i]:
                                    continue
                                dx = x[j, 0] - x[i, 0]
                                dy = x[j, 1] - x[i, 1]
                                dz = x[j, 2] - x[i, 2]
                                d2 = dx * dx + dy * dy + dz * dz
                                if d2 >= r2 or d2 < 1e-14:
                                    continue
                                if _excluded(i, j, ex_start, ex):
                                    continue
                                d = math.sqrt(d2)
                                f = 0.5 * (srad - d) / d
                                x[i, 0] -= f * dx
                                x[i, 1] -= f * dy
                                x[i, 2] -= f * dz
                                x[j, 0] += f * dx
                                x[j, 1] += f * dy
                                x[j, 2] += f * dz
        # Outside the trousers (still): each point off the nearest of their triangles, on the side away from him.
        if otri.shape[0] > 0:
            for i in range(n):
                ix = int((x[i, 0] - olo[0]) / ocell)
                iy = int((x[i, 1] - olo[1]) / ocell)
                iz = int((x[i, 2] - olo[2]) / ocell)
                if ix < 0 or iy < 0 or iz < 0 or ix >= odims[0] or iy >= odims[1] or iz >= odims[2]:
                    continue
                c = (iz * odims[1] + iy) * odims[0] + ix
                best = 1e9
                bs = 0.0
                bk = -1
                for t in range(ostart[c], ostart[c + 1]):
                    k = oitems[t]
                    qx, qy, qz = _closest(x[i, 0], x[i, 1], x[i, 2], otri[k])
                    dx = x[i, 0] - qx
                    dy = x[i, 1] - qy
                    dz = x[i, 2] - qz
                    d2 = dx * dx + dy * dy + dz * dz
                    if d2 < best:
                        best = d2
                        bk = k
                        bs = dx * on[k, 0] + dy * on[k, 1] + dz * on[k, 2]
                if bk >= 0 and best < 0.025 * 0.025 and bs < othick:
                    m = othick - bs
                    x[i, 0] += on[bk, 0] * m
                    x[i, 1] += on[bk, 1] * m
                    x[i, 2] += on[bk, 2] * m
        # Out of the drawn body, with friction; and off the floor.
        for i in range(n):
            contact[i] = 0
            for rep in range(2):
                d, gx, gy, gz = body_dist(x[i, 0], x[i, 1], x[i, 2], meta, sdf, cell, ca, sa, shx, shy)
                if d >= thick:
                    break
                m = thick - d
                x[i, 0] += gx * m
                x[i, 1] += gy * m
                x[i, 2] += gz * m
                contact[i] = 1
                nrm[i, 0] = gx
                nrm[i, 1] = gy
                nrm[i, 2] = gz
            if x[i, 1] < floor_y:
                x[i, 1] = floor_y
                contact[i] = 1
                nrm[i, 0] = 0.0
                nrm[i, 1] = 1.0
                nrm[i, 2] = 0.0
            if contact[i] == 1 and mu > 0:
                dx = x[i, 0] - start[i, 0]
                dy = x[i, 1] - start[i, 1]
                dz = x[i, 2] - start[i, 2]
                dn = dx * nrm[i, 0] + dy * nrm[i, 1] + dz * nrm[i, 2]
                tx = dx - dn * nrm[i, 0]
                ty = dy - dn * nrm[i, 1]
                tz = dz - dn * nrm[i, 2]
                x[i, 0] -= mu * tx
                x[i, 1] -= mu * ty
                x[i, 2] -= mu * tz


@njit(cache=True, fastmath=True)
def all_dist(x, meta, sdf, cell, ca, sa, shx, shy):
    out = np.empty(x.shape[0])
    for i in range(x.shape[0]):
        d, _, _, _ = body_dist(x[i, 0], x[i, 1], x[i, 2], meta, sdf, cell, ca, sa, shx, shy)
        out[i] = d
    return out


# ---- The garments -----------------------------------------------------------------------------------------------------
pat = json.load(open(PAT))
STIFF = ("neck_band", "waistband")


class Garment:
    def __init__(self, name):
        self.name = name
        self.pieces = [p for p in pat["pieces"] if p["garment"] == name]
        self.offs = {}
        uv, xyz, tris, piece = [], [], [], []
        n = 0
        for k, p in enumerate(self.pieces):
            self.offs[p["name"]] = n
            u = np.array(p["uv"]).reshape(-1, 2)
            uv.append(u)
            xyz.append(np.array(p["xyz"]).reshape(-1, 3) + [0, PELVIS_Y, 0])
            tris.append(np.array(p["tris"]).reshape(-1, 3) + n)
            piece += [k] * len(u)
            n += len(u)
        self.uv = np.vstack(uv)
        self.x0 = np.vstack(xyz)
        self.tris0 = np.vstack(tris)
        self.piece0 = np.array(piece)
        self.stiff0 = np.array([self.pieces[k]["name"] in STIFF for k in piece])
        pairs = []
        self.seam_names = []
        for s in pat["seams"]:
            if s["a"] in self.offs and s["b"] in self.offs:
                for a, b in zip(s["ai"], s["bi"]):
                    pairs.append((self.offs[s["a"]] + a, self.offs[s["b"]] + b))
                    self.seam_names.append(s["name"])
        self.seams0 = np.array(pairs, dtype=np.int64)
        self.n0 = n
        self.rep = np.arange(n)          # each original point's welded point (before the weld, itself)
        self.x = self.x0.copy()
        self.prev = self.x.copy()
        self.topology()

    def topology(self):
        """Threads, bends and seams of the points as they are now joined."""
        uniq, idx = np.unique(self.rep, return_inverse=True)
        self.idx = idx                   # original point -> point
        self.n = len(uniq)
        T = idx[self.tris0]
        self.tris = T
        rest = {}
        for t0, t in zip(self.tris0, T):
            for a, b in ((0, 1), (1, 2), (2, 0)):
                i, j = t[a], t[b]
                key = (min(i, j), max(i, j))
                l = float(np.linalg.norm(self.uv[t0[a]] - self.uv[t0[b]]))
                # (A thread sewn from two pieces of different lengths, a gather: the shorter side's length.)
                rest[key] = min(rest.get(key, 1e9), l)
        self.E = np.array(list(rest.keys()), dtype=np.int64)
        self.EL = np.array(list(rest.values()))
        opp = {}
        for ti, t in enumerate(T):
            for a, b, c in ((0, 1, 2), (1, 2, 0), (2, 0, 1)):
                key = (min(t[a], t[b]), max(t[a], t[b]))
                opp.setdefault(key, []).append(t[c])
        B, BL = [], []
        self.nonmanifold = 0
        for (i, j), o in opp.items():
            if len(o) > 2:
                self.nonmanifold += 1
            if len(o) != 2:
                continue
            k, l = o
            Lij = rest[(i, j)]
            a1, b1 = rest[(min(i, k), max(i, k))], rest[(min(j, k), max(j, k))]
            a2, b2 = rest[(min(i, l), max(i, l))], rest[(min(j, l), max(j, l))]
            x1 = (a1 * a1 - b1 * b1 + Lij * Lij) / (2 * Lij)
            y1 = math.sqrt(max(a1 * a1 - x1 * x1, 0.0))
            x2 = (a2 * a2 - b2 * b2 + Lij * Lij) / (2 * Lij)
            y2 = -math.sqrt(max(a2 * a2 - x2 * x2, 0.0))
            B.append((k, l))
            BL.append(math.hypot(x1 - x2, y1 - y2))
        self.B = np.array(B, dtype=np.int64)
        self.BL = np.array(BL)
        stiff = np.zeros(self.n, bool)
        stiff[idx[self.stiff0]] = True
        self.stiff = stiff
        self.BC = np.where(stiff[self.B[:, 0]] & stiff[self.B[:, 1]], L["BANDBEND"], L["BEND"])
        S = idx[self.seams0] if len(self.seams0) else np.zeros((0, 2), np.int64)
        self.S = S[S[:, 0] != S[:, 1]] if len(S) else S
        # (Pairs near each other along the cloth never push apart: two steps along threads or seams.)
        links = np.vstack([self.E, self.S]) if len(self.S) else self.E
        A = coo_matrix((np.ones(len(links)), (links[:, 0], links[:, 1])), shape=(self.n, self.n)).tocsr()
        A = ((A + A.T) > 0).astype(np.int32)
        A2 = ((A + A @ A) > 0).tocsr()
        A2.sort_indices()
        self.ex_start = A2.indptr.astype(np.int64)
        self.ex = A2.indices.astype(np.int64)
        piece = np.zeros(self.n, np.int64)
        piece[idx] = self.piece0
        self.piece = piece

    def weld(self):
        """Each sewn pair made one point (union-find), at the middle of the points it joins."""
        parent = np.arange(self.n0)

        def find(a):
            while parent[a] != a:
                parent[a] = parent[parent[a]]
                a = parent[a]
            return a
        for a, b in self.seams0:
            ra, rb = find(a), find(b)
            if ra != rb:
                parent[max(ra, rb)] = min(ra, rb)
        rep = np.array([find(a) for a in range(self.n0)])
        # Gap across each pair just before.
        xo = self.x[self.idx]
        gap = np.linalg.norm(xo[self.seams0[:, 0]] - xo[self.seams0[:, 1]], axis=1)
        po = self.prev[self.idx]
        self.rep = rep
        self.topology()
        cnt = np.bincount(self.idx, minlength=self.n)[:, None]
        x = np.zeros((self.n, 3))
        pv = np.zeros((self.n, 3))
        np.add.at(x, self.idx, xo)
        np.add.at(pv, self.idx, po)
        self.x = x / cnt
        self.prev = pv / cnt
        return gap


trousers = Garment("trousers")
shirt = Garment("shirt")
EMPTY_T = np.zeros((0, 3, 3))
EMPTY_N = np.zeros((0, 3))
EMPTY_I = np.zeros(1, np.int64)
NO_SASH = np.zeros(8)
# The drawstring, tied: the waistband kept inside his waist at 1.085 m (half-widths 13.5 and 9.75 cm, the middle 0.6 cm
# behind) and a little, and no lower than DRAW_LO (as his hip bones hold it). (Its weight alone stretched a 77 cm band 18% and an 80 cm one slid off his hips.)
DRAW = np.array([1.0, L["DRAW_LO"], 9.0, 0.135 + L["DRAWGAP"], 0.0975 + L["DRAWGAP"], 0.0, -0.006, 1.0])


def run(gm, frames, gravity, keep, ang=(None, None), sew=True, sash=NO_SASH, obstacle=None, self_on=False, label=""):
    t0 = time.time()
    raise_full = math.radians(pat["armsUp"] - REST_DEG)
    a0 = raise_full if ang[0] is None else ang[0]
    a1 = a0 if ang[1] is None else ang[1]
    ob = obstacle or (EMPTY_T, EMPTY_N, EMPTY_I, EMPTY_I, np.zeros(3), 1.0, np.ones(3, np.int64))
    # (The sash gathers the shirt's body only: a sleeve hangs past it. The drawstring holds the waistband.)
    names = [p["name"] for p in gm.pieces]
    ring_mask = np.isin(gm.piece, [names.index(n) for n in ("shirt_front", "shirt_back") if n in names]).astype(np.int8)
    if "waistband" in names:
        ring_mask[gm.piece == names.index("waistband")] = 2
    rings = np.vstack([sash, DRAW])
    for f in range(frames):
        aa = a0 + (a1 - a0) * f / frames
        ab = a0 + (a1 - a0) * (f + 1) / frames
        picture(gm.x, gm.prev, int(L["SUB"]), 9.81 * gravity, keep, gm.E, gm.EL, 0.0, L["SQUASH"],
                gm.B, gm.BL, gm.BC, gm.S if sew else np.zeros((0, 2), np.int64), L["SEWPULL"], L["SEWSTEP"],
                meta, SDF, CELL, aa, ab, SH[0], SH[1], L["THICK"], L["FRICTION"], 0.004,
                rings, ring_mask, ob[0], ob[1], ob[2], ob[3], ob[4], ob[5], ob[6], L["OVER"],
                1 if self_on else 0, L["SELF"], gm.ex_start, gm.ex, int(L["ITER"]), gm.piece)
    gap = 0.0
    if len(gm.S):
        gap = float(np.linalg.norm(gm.x[gm.S[:, 0]] - gm.x[gm.S[:, 1]], axis=1).max())
    st = stretch(gm)
    if os.environ.get("DRAPE_DEBUG"):
        if len(gm.S):
            g_ = np.linalg.norm(gm.x[gm.S[:, 0]] - gm.x[gm.S[:, 1]], axis=1)
            worst = {}
            for (a, b), nm, gg in zip(gm.seams0, gm.seam_names, np.linalg.norm(gm.x[gm.idx[gm.seams0[:, 0]]] - gm.x[gm.idx[gm.seams0[:, 1]]], axis=1)):
                worst[nm] = max(worst.get(nm, 0), gg)
            print("     seams:", ", ".join(f"{k.split('+')[0]}~{v * 100:.1f}" for k, v in sorted(worst.items(), key=lambda kv: -kv[1])[:6]))
        for p_ in gm.pieces:
            if p_["name"] in ("shirt_front", "shirt_back", "sleeve_R", "neck_band"):
                for sg in ("hem", "cuff", "top"):
                    if sg in p_["segs"]:
                        ii = gm.idx[np.array(p_["segs"][sg]) + gm.offs[p_["name"]]]
                        print(f"     {p_['name']}.{sg}: y {gm.x[ii, 1].min():.3f} to {gm.x[ii, 1].max():.3f}, middle {np.median(gm.x[ii], 0).round(3)}")
        l_ = np.linalg.norm(gm.x[gm.E[:, 0]] - gm.x[gm.E[:, 1]], axis=1) / gm.EL
        for e in np.argsort(-l_)[:5]:
            i, j = gm.E[e]
            print(f"     thread {l_[e]:.2f}x rest {100 * gm.EL[e]:.1f} cm, {gm.pieces[gm.piece[i]]['name']}/{gm.pieces[gm.piece[j]]['name']} at {np.round(gm.x[i], 3)}")
    if os.environ.get("DRAPE_SNAP"):
        import garment
        SNAPS.append(label)
        lay = [(g.x, g.tris, g.name) for g in DRAWN]
        garment.draw(os.path.join(os.environ["DRAPE_SNAP"], f"{len(SNAPS):02d}.png"), lay, a1, f"{len(SNAPS)}: {gm.name}, {label}")
    print(f"  {label:34s} {frames / HZ:4.1f} s  widest seam gap {100 * gap:5.1f} cm  longest thread {100 * st:5.1f}%  ({time.time() - t0:.1f} s)", flush=True)


def stretch(gm):
    l = np.linalg.norm(gm.x[gm.E[:, 0]] - gm.x[gm.E[:, 1]], axis=1)
    return float((l / gm.EL).max())


def obstacle_of(gm):
    """The trousers as triangles to keep the shirt off, each facing away from the body, filed in a grid."""
    tri = gm.x[gm.tris]
    nrm = np.cross(tri[:, 1] - tri[:, 0], tri[:, 2] - tri[:, 0])
    nrm /= np.linalg.norm(nrm, axis=1, keepdims=True) + 1e-12
    cen = tri.mean(1)
    for k in range(len(tri)):
        d, gx, gy, gz = body_dist(cen[k, 0], cen[k, 1], cen[k, 2], meta, SDF, CELL, 1.0, 0.0, SH[0], SH[1])
        if nrm[k] @ np.array([gx, gy, gz]) < 0:
            nrm[k] = -nrm[k]
    cell = 0.03
    lo = tri.reshape(-1, 3).min(0) - 0.05
    hi = tri.reshape(-1, 3).max(0) + 0.05
    dims = np.ceil((hi - lo) / cell).astype(np.int64)
    lists = [[] for _ in range(int(np.prod(dims)))]
    for k in range(len(tri)):
        a = np.floor((tri[k].min(0) - 0.026 - lo) / cell).astype(int)
        b = np.floor((tri[k].max(0) + 0.026 - lo) / cell).astype(int)
        for iz in range(a[2], b[2] + 1):
            for iy in range(a[1], b[1] + 1):
                for ix in range(a[0], b[0] + 1):
                    lists[(iz * dims[1] + iy) * dims[0] + ix].append(k)
    start = np.zeros(len(lists) + 1, np.int64)
    start[1:] = np.cumsum([len(l) for l in lists])
    items = np.array([k for l in lists for k in l], np.int64)
    return (np.ascontiguousarray(tri), nrm, start, items, lo, cell, dims)


# ---- The drape --------------------------------------------------------------------------------------------------------
print(f"drape: {trousers.n0} trouser points, {shirt.n0} shirt points; arms raised {pat['armsUp']:.0f} degrees (rest {REST_DEG:.0f})")
t_all = time.time()
d0 = all_dist(np.vstack([trousers.x, shirt.x]), meta, SDF, CELL, math.cos(math.radians(pat["armsUp"] - REST_DEG)),
              math.sin(math.radians(pat["armsUp"] - REST_DEG)), SH[0], SH[1])
print(f"  placed: {int((d0 < 0).sum())} points inside the body, nearest {100 * d0.min():.1f} cm")
for gm, dd in ((trousers, d0[:trousers.n0]), (shirt, d0[trousers.n0:])):
    for k, p in enumerate(gm.pieces):
        s_ = dd[gm.piece0 == k]
        if (s_ < 0).any():
            print(f"    {p['name']}: {int((s_ < 0).sum())} inside, deepest {100 * s_.min():.1f} cm")
SNAPS = []
DRAWN = [trousers]


def place_clear(gm, ca, sa, clear=0.02):
    """Points placed in or near the body moved straight out from the piece's middle line until 2 cm clear."""
    moved = 0
    for i in range(gm.n):
        x = gm.x[i]
        d, _, _, _ = body_dist(x[0], x[1], x[2], meta, SDF, CELL, ca, sa, SH[0], SH[1])
        if d >= clear:
            continue
        nm = gm.pieces[gm.piece[i]]["name"]
        cx = 0.0
        if nm.startswith("leg") and x[1] < 0.86:
            cx = 0.095 if nm.endswith("R") else -0.095
        u = np.array([x[0] - cx, 0.0, x[2]])
        if nm.startswith("sleeve"):
            # (Straight out from the raised arm's line.)
            side = 1.0 if nm.endswith("R") else -1.0
            piv = SH * [side, 1, 1]
            r0 = rest_arm * [side, 1, 1]
            a = side * math.atan2(sa, ca)
            ax_ = np.array([r0[0] * math.cos(a) - r0[1] * math.sin(a), r0[0] * math.sin(a) + r0[1] * math.cos(a), r0[2]])
            ax_ /= np.linalg.norm(ax_)
            u = (x - piv) - ((x - piv) @ ax_) * ax_
        u /= np.linalg.norm(u) + 1e-12
        for _ in range(60):
            x += u * 0.005
            d, _, _, _ = body_dist(x[0], x[1], x[2], meta, SDF, CELL, ca, sa, SH[0], SH[1])
            if d >= clear:
                break
        moved += 1
    gm.prev = gm.x.copy()
    return moved


_up = math.radians(pat["armsUp"] - REST_DEG)
print(f"  moved clear of the body to start: trousers {place_clear(trousers, math.cos(_up), math.sin(_up))}, shirt {place_clear(shirt, math.cos(_up), math.sin(_up))}")

print("trousers")
run(trousers, 60, 0.0, 0.80, label="sewn, no gravity")
run(trousers, 60, 1.0, 0.85, label="sewn, gravity")
gap_t = trousers.weld()
print(f"  welded: {trousers.n} points, widest pair gap before {100 * gap_t.max():.2f} cm, non-manifold edges {trousers.nonmanifold}")
run(trousers, 120, 1.0, 0.92, label="settled")

# The sash: round the waistband, a little out.
xt = trousers.x
band = xt[(xt[:, 1] > L["SASH_LO"] - 0.01) & (xt[:, 1] < L["SASH_HI"] + 0.01)]
cz = 0.5 * (band[:, 2].max() + band[:, 2].min())
SASH = np.array([1.0, L["SASH_LO"], L["SASH_HI"], np.abs(band[:, 0]).max() + L["SASHGAP"],
                 0.5 * (band[:, 2].max() - band[:, 2].min()) + L["SASHGAP"], 0.0, cz, 0.0])
print(f"  sash: {100 * SASH[3]:.1f} by {100 * SASH[4]:.1f} cm half-widths at {L['SASH_LO']:.2f} to {L['SASH_HI']:.2f} m")
OB = obstacle_of(trousers)

# The shirt starts outside the trousers: below 1.2 m each of its points is moved out to the trousers' widest way at
# its height and angle, plus 2 cm.
ang_t = np.arctan2(xt[:, 2] - cz, xt[:, 0])
rad_t = np.hypot(xt[:, 0], xt[:, 2] - cz)
prof = np.zeros((80, 48))
yb = np.clip(((xt[:, 1] - 0.6) / 0.01).astype(int), 0, 79)
ab = ((ang_t + math.pi) / (2 * math.pi) * 48).astype(int) % 48
np.maximum.at(prof, (yb, ab), rad_t)
xs = shirt.x
moved = 0
for i in range(len(xs)):
    if xs[i, 1] > 1.2:
        continue
    a = math.atan2(xs[i, 2] - cz, xs[i, 0])
    r = math.hypot(xs[i, 0], xs[i, 2] - cz)
    y0 = int(np.clip((xs[i, 1] - 0.6) / 0.01, 0, 79))
    a0 = int((a + math.pi) / (2 * math.pi) * 48) % 48
    need = max(prof[max(y0 - 2, 0):y0 + 3, [(a0 - 1) % 48, a0, (a0 + 1) % 48]].max(), 0) + 0.02
    if r < need:
        xs[i, 0] *= need / r
        xs[i, 2] = cz + (xs[i, 2] - cz) * need / r
        moved += 1
shirt.prev = shirt.x.copy()
DRAWN = [trousers, shirt]
print(f"shirt ({moved} points moved out past the trousers to start)")
full = math.radians(pat["armsUp"] - REST_DEG)
run(shirt, 60, 0.0, 0.80, ang=(full, full), obstacle=OB, label="sewn, no gravity, arms up")
run(shirt, 45, 1.0, 0.85, ang=(full, full), obstacle=OB, self_on=True, label="sewn, gravity, arms up")
run(shirt, 75, 1.0, 0.85, ang=(full, 0.0), obstacle=OB, self_on=True, label="arms lowered to rest")
run(shirt, 60, 1.0, 0.90, ang=(0.0, 0.0), obstacle=OB, self_on=True, label="hung from the shoulders")
SASH_IN = SASH.copy()
for k in range(6):
    s = SASH.copy()
    s[3:5] *= 1.0 + 0.12 * (5 - k) / 5
    run(shirt, 10, 1.0, 0.88, ang=(0.0, 0.0), sash=s, obstacle=OB, self_on=True, label=f"the sash drawn in ({k + 1} of 6)")
gap_s = shirt.weld()
print(f"  welded: {shirt.n} points, widest pair gap before {100 * gap_s.max():.2f} cm, non-manifold edges {shirt.nonmanifold}")
run(shirt, 120, 1.0, 0.93, ang=(0.0, 0.0), sash=SASH, obstacle=OB, self_on=True, label="settled")
run(shirt, 60, 1.0, 0.97, ang=(0.0, 0.0), sash=SASH, obstacle=OB, self_on=True, label="settled, light damping")
print(f"drape done in {time.time() - t_all:.0f} s")


# ---- Measures ---------------------------------------------------------------------------------------------------------
def body_d(p):
    return all_dist(np.ascontiguousarray(p), meta, SDF, CELL, 1.0, 0.0, SH[0], SH[1])


M = {}
for gm in (trousers, shirt):
    pts = gm.x
    mids = 0.5 * (pts[gm.E[:, 0]] + pts[gm.E[:, 1]])
    faces = pts[gm.tris].mean(1)
    dp, dm, df = body_d(pts), body_d(mids), body_d(faces)
    l = np.linalg.norm(pts[gm.E[:, 0]] - pts[gm.E[:, 1]], axis=1) / gm.EL
    sm = pts.copy()
    nb = [[] for _ in range(gm.n)]
    for i, j in gm.E:
        nb[i].append(j)
        nb[j].append(i)
    for _ in range(8):
        sm = np.array([sm[nb[i]].mean(0) if nb[i] else sm[i] for i in range(gm.n)])
    # (Fold depth: how far the cloth is from a copy of itself smoothed over about 6 cm.)
    fold = np.linalg.norm(pts - sm, axis=1)
    names_ = [p["name"] for p in gm.pieces]
    region = {}
    if gm.name == "shirt":
        bod = np.isin(gm.piece, [names_.index("shirt_front"), names_.index("shirt_back")])
        slv = np.isin(gm.piece, [names_.index("sleeve_R"), names_.index("sleeve_L")])
        region = {"belowTheSash": bod & (pts[:, 1] < L["SASH_LO"]), "sleeves": slv, "underTheArms": slv & (pts[:, 1] > 1.2) & (np.abs(pts[:, 0]) < 0.26)}
    else:
        region = {"knees": (pts[:, 1] > 0.4) & (pts[:, 1] < 0.6), "hems": pts[:, 1] < 0.25}
    M[gm.name] = dict(points=int(gm.n), triangles=int(len(gm.tris)),
                      pointsInside=int((dp < 0).sum()), middlesInside=int((dm < 0).sum()), facesInside=int((df < 0).sum()),
                      nearestCm=round(100 * float(min(dp.min(), dm.min(), df.min())), 2),
                      longestThread=round(float(l.max()), 3), threadsOver105=int((l > 1.05).sum()),
                      shortestThread=round(float(l.min()), 3),
                      foldDepthCm={k: dict(p50=round(100 * float(np.percentile(fold[m], 50)), 2), p90=round(100 * float(np.percentile(fold[m], 90)), 2),
                                           most=round(100 * float(fold[m].max()), 2)) for k, m in dict(all=np.ones(gm.n, bool), **region).items()},
                      nonManifoldEdges=int(gm.nonmanifold))
M["trousers"]["seamGapBeforeWeldCm"] = round(100 * float(gap_t.max()), 2)
M["shirt"]["seamGapBeforeWeldCm"] = round(100 * float(gap_s.max()), 2)

# Hangs, not hugs: the shirt's outline in four ways between the chest and the sash, against the straight line.
xs = shirt.x
dirs = {"front": (0, 1), "back": (0, -1), "right": (1, 0), "left": (-1, 0)}
hang = {}
for nm, (ux, uz) in dirs.items():
    ang = np.arctan2(xs[:, 2] - cz, xs[:, 0])
    want = math.atan2(uz, ux)
    sel = np.abs((ang - want + math.pi) % (2 * math.pi) - math.pi) < math.radians(12)
    r = np.hypot(xs[:, 0], xs[:, 2] - cz)
    ys = np.arange(L["SASH_HI"], 1.36, 0.02)
    prof_ = []
    for y in ys:
        s = sel & (np.abs(xs[:, 1] - y) < 0.012)
        prof_.append(r[s].max() if s.any() else np.nan)
    prof_ = np.array(prof_)
    line = prof_[0] + (prof_[-1] - prof_[0]) * (ys - ys[0]) / (ys[-1] - ys[0])
    hang[nm] = round(100 * float(np.nanmax(line - prof_)), 2)
M["shirt"]["inFromChestToSashLineCm"] = hang


def girth(p):
    from scipy.spatial import ConvexHull
    h = ConvexHull(p[:, [0, 2]])
    v = p[h.vertices][:, [0, 2]]
    return float(np.sum(np.linalg.norm(v - np.roll(v, 1, axis=0), axis=1)))


body_pts = []
from fbxread import read  # noqa: E402  (Tools/Cloth on the path: this file's folder)
for nm in ("Pelvis", "Thigh_R", "Thigh_L"):
    r = read(f"Assets/_Battler/Art/Bodies/{SIZE}/SM_{SIZE}_{nm}.fbx")
    v = r["Vertices"][0].reshape(-1, 3).astype(float).copy()
    v[:, 0] *= -1
    body_pts.append(v + world(nm))
body_pts = np.vstack(body_pts)
yh = 0.97
bodice = np.isin(shirt.piece, [[p["name"] for p in shirt.pieces].index(n) for n in ("shirt_front", "shirt_back")])
M["shirt"]["girthBelowSashCm"] = round(100 * girth(xs[(np.abs(xs[:, 1] - yh) < 0.015) & bodice]), 1)
M["shirt"]["hipsGirthCm"] = round(100 * girth(body_pts[np.abs(body_pts[:, 1] - yh) < 0.01]), 1)
nb = shirt.piece == [p["name"] for p in shirt.pieces].index("neck_band")
M["shirt"]["neckBandTopCm"] = round(100 * float(xs[nb, 1].max()), 1)
M["shirt"]["neckBandFrontLowCm"] = round(100 * float(xs[nb & (xs[:, 2] > 0.03), 1].min()), 1)
M["shirt"]["neckBandOffSkinCm"] = round(100 * float(np.median(body_d(xs[nb]))), 2)
M["trousers"]["waistbandAtCm"] = round(100 * float(trousers.x[trousers.piece == [p["name"] for p in trousers.pieces].index("waistband"), 1].mean()), 1)
M["trousers"]["hemsLowestCm"] = round(100 * float(trousers.x[:, 1].min()), 1)


# Bare skin: every vertex of the covered parts' drawn meshes, a ray straight out of the body; covered if it meets cloth
# within 20 cm, hidden if just outside it is still inside the body (another piece over it at a joint), else bare.
@njit(cache=True, fastmath=True)
def rays_hit(o, d, T, lo, hi, near):
    """The nearest distance along each ray (from o, way d) to a triangle of T, past `near` and within 20 cm (else 9)."""
    hit = np.full(o.shape[0], 9.0)
    for i in range(o.shape[0]):
        for k in range(T.shape[0]):
            # (A box round the ray's 20 cm first.)
            ex = o[i, 0] + 0.2 * d[i, 0]
            ey = o[i, 1] + 0.2 * d[i, 1]
            ez = o[i, 2] + 0.2 * d[i, 2]
            if max(o[i, 0], ex) < lo[k, 0] or min(o[i, 0], ex) > hi[k, 0] or max(o[i, 1], ey) < lo[k, 1] or min(o[i, 1], ey) > hi[k, 1] or max(o[i, 2], ez) < lo[k, 2] or min(o[i, 2], ez) > hi[k, 2]:
                continue
            e1x = T[k, 1, 0] - T[k, 0, 0]; e1y = T[k, 1, 1] - T[k, 0, 1]; e1z = T[k, 1, 2] - T[k, 0, 2]
            e2x = T[k, 2, 0] - T[k, 0, 0]; e2y = T[k, 2, 1] - T[k, 0, 1]; e2z = T[k, 2, 2] - T[k, 0, 2]
            px = d[i, 1] * e2z - d[i, 2] * e2y; py = d[i, 2] * e2x - d[i, 0] * e2z; pz = d[i, 0] * e2y - d[i, 1] * e2x
            det = e1x * px + e1y * py + e1z * pz
            if abs(det) < 1e-12:
                continue
            inv = 1.0 / det
            tx = o[i, 0] - T[k, 0, 0]; ty = o[i, 1] - T[k, 0, 1]; tz = o[i, 2] - T[k, 0, 2]
            u = (tx * px + ty * py + tz * pz) * inv
            if u < 0 or u > 1:
                continue
            qx = ty * e1z - tz * e1y; qy = tz * e1x - tx * e1z; qz = tx * e1y - ty * e1x
            v = (d[i, 0] * qx + d[i, 1] * qy + d[i, 2] * qz) * inv
            if v < 0 or u + v > 1:
                continue
            t = (e2x * qx + e2y * qy + e2z * qz) * inv
            if t >= near and t <= 0.2 and t < hit[i]:
                hit[i] = t
    return hit


COVERED = ("Pelvis", "Spine", "Chest", "UpperArm", "Forearm", "Thigh", "Shin")
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import garment as _garment  # noqa: E402
BODY_T = np.ascontiguousarray(_garment.body_tris(0.0))
bare_where = {}
cloth_T = np.concatenate([trousers.x[trousers.tris], shirt.x[shirt.tris]])
bare = {}
bare_pts = []
for part in rig["parts"]:
    if part["name"].split("_")[0] not in COVERED:
        continue
    r = read(f"Assets/_Battler/Art/Bodies/{SIZE}/{part['mesh']}.fbx")
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
    nv = np.zeros_like(v)
    for k in range(3):
        np.add.at(nv, t[:, k], fn)
    nv /= np.linalg.norm(nv, axis=1, keepdims=True) + 1e-12
    if np.mean(np.sum((v - v.mean(0)) * nv, axis=1)) < 0:
        nv = -nv
    v = v + world(part["name"])
    hidden = body_d(v + nv * 0.003) < 0
    o = np.ascontiguousarray(v[~hidden])
    dv = np.ascontiguousarray(nv[~hidden])
    tc = rays_hit(o, dv, cloth_T, cloth_T.min(1), cloth_T.max(1), -0.002)
    # (A ray that meets the body before any cloth starts in a crease, as under the arm: not seen either.)
    tb = rays_hit(o, dv, BODY_T, BODY_T.min(1), BODY_T.max(1), 0.004)
    crease = (tb < tc) & (tc > 0.2)
    b = o[(tc > 0.2) & ~crease]
    bare[part["name"]] = [int(len(b)), int(len(o)), int(crease.sum())]
    if len(b):
        bare_where[part["name"]] = [round(float(b[:, 1].min()), 3), round(float(b[:, 1].max()), 3)]
    bare_pts.append(b)
bare_pts = np.vstack(bare_pts)
neck_y = float(shirt.x[nb, 1].min())
M["bareSkin"] = {"byPart (bare, seen, in a crease)": bare, "bareHeights": bare_where,
                 "aboveTheNeckBand": int((bare_pts[:, 1] > neck_y).sum()),
                 "elsewhere": int((bare_pts[:, 1] <= neck_y).sum())}
for k, v in M.items():
    print(k, json.dumps(v))


# ---- Save -------------------------------------------------------------------------------------------------------------
def held_from(gm, names):
    """Metres along the cloth from the nearest point of the named pieces or seams (where it is held)."""
    srcs = set()
    for nm in names:
        if nm in [p["name"] for p in gm.pieces]:
            srcs |= set(np.nonzero(gm.piece == [p["name"] for p in gm.pieces].index(nm))[0].tolist())
        for (a, b), sn in zip(gm.seams0, gm.seam_names):
            if nm in sn:
                srcs.add(int(gm.idx[a]))
    l = np.linalg.norm(gm.x[gm.E[:, 0]] - gm.x[gm.E[:, 1]], axis=1)
    A = coo_matrix((np.r_[l, l], (np.r_[gm.E[:, 0], gm.E[:, 1]], np.r_[gm.E[:, 1], gm.E[:, 0]])), shape=(gm.n, gm.n)).tocsr()
    return dijkstra(A, indices=sorted(srcs), min_only=True)


doc = {"body": SIZE, "pelvisY": PELVIS_Y, "from": os.path.basename(PAT), "levers": L, "measures": M,
       "sash": {"lo": L["SASH_LO"] - PELVIS_Y, "hi": L["SASH_HI"] - PELVIS_Y, "a": float(SASH[3]), "b": float(SASH[4]), "cz": float(SASH[6])},
       "garments": []}
for gm, held in ((trousers, ["waistband"]), (shirt, ["neck_band", "shoulder"])):
    uvp = np.zeros((gm.n, 2))
    uvp[gm.idx] = gm.uv
    doc["garments"].append({
        "name": gm.name,
        "pieces": [p["name"] for p in gm.pieces],
        "x": np.round((gm.x - [0, PELVIS_Y, 0]).ravel(), 5).tolist(),
        "piece": gm.piece.tolist(),
        "uv": np.round(uvp.ravel(), 5).tolist(),
        "tris": gm.tris.ravel().tolist(),
        "threads": gm.E.ravel().tolist(), "threadLength": np.round(gm.EL, 5).tolist(),
        "bends": gm.B.ravel().tolist(), "bendLength": np.round(gm.BL, 5).tolist(),
        "stiff": gm.stiff.astype(int).tolist(),
        "heldFrom": np.round(held_from(gm, held), 4).tolist(),
    })
json.dump(doc, open(OUT, "w"), separators=(",", ":"))
print(OUT, os.path.getsize(OUT) // 1024, "KB")
