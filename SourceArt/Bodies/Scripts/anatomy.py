"""Sculpts a body as one smooth implicit surface (a signed distance field built from muscle
volumes blended together), then cuts it into the rigid parts of the null rig.

Why this way: a sculpted base mesh reads as one continuous anatomical body. Each part is the body
surface inside that part's region, plus a ball around each of its joint pivots (clipped to the
body surface, so it doesn't show at rest). When a joint bends, the balls fill the gap, the same
ball-joint rule as before. At rest the parts meet along the original surface, so the seams are
nearly invisible.

All coordinates are Blender's: Z up, the body faces -Y, its right side is -X. Metres.
"""

import math

import numpy as np
from skimage import measure

X = np.array([1.0, 0, 0])
Y = np.array([0, 1.0, 0])     # back
F = np.array([0, -1.0, 0])    # front
Z = np.array([0, 0, 1.0])


def V(*a):
    return np.array(a, dtype=np.float64)


def unit(v):
    v = np.asarray(v, dtype=np.float64)
    return v / np.linalg.norm(v)


def frame(axis, ref=F):
    """Rotation matrix whose columns are local x, y, z with z along axis and y as close to ref."""
    z = unit(axis)
    y = np.asarray(ref, dtype=np.float64) - z * np.dot(ref, z)
    if np.linalg.norm(y) < 1e-6:
        y = Y - z * np.dot(Y, z)
    y = unit(y)
    x = np.cross(y, z)
    return np.stack([x, y, z], axis=1)


# ------------------------------------------------------------------------------- primitives

class Ellipsoid:
    def __init__(self, c, radii, axis=Z, ref=F):
        self.c = np.asarray(c, dtype=np.float64)
        self.r = np.asarray(radii, dtype=np.float64)
        self.R = frame(axis, ref)
        self.extent = float(self.r.max())

    def sdf(self, P):
        q = (P - self.c) @ self.R
        k0 = np.linalg.norm(q / self.r, axis=1)
        k1 = np.linalg.norm(q / (self.r * self.r), axis=1)
        return k0 * (k0 - 1.0) / np.maximum(k1, 1e-9)

    def bounds(self):
        return self.c - self.extent, self.c + self.extent


class RoundCone:
    """A capsule whose radius tapers from ra at a to rb at b (exact distance, after Inigo Quilez)."""

    def __init__(self, a, b, ra, rb):
        self.a, self.b = np.asarray(a, dtype=np.float64), np.asarray(b, dtype=np.float64)
        self.ra, self.rb = float(ra), float(rb)

    def sdf(self, P):
        a, b, r1, r2 = self.a, self.b, self.ra, self.rb
        ba = b - a
        l2 = float(ba @ ba)
        rr = r1 - r2
        a2 = l2 - rr * rr
        il2 = 1.0 / l2
        pa = P - a
        y = pa @ ba
        zz = y - l2
        xv = pa * l2 - np.outer(y, ba)
        x2 = np.einsum("ij,ij->i", xv, xv)
        y2 = y * y * l2
        z2 = zz * zz * l2
        k = math.copysign(1.0, rr) * rr * rr * x2
        d3 = (np.sqrt(np.maximum(x2 * a2 * il2, 0)) + y * rr) * il2 - r1
        d1 = np.sqrt(x2 + z2) * il2 - r2
        d2 = np.sqrt(x2 + y2) * il2 - r1
        out = np.where(np.sign(zz) * a2 * z2 > k, d1, np.where(np.sign(y) * a2 * y2 < k, d2, d3))
        return out

    def bounds(self):
        r = max(self.ra, self.rb)
        return np.minimum(self.a, self.b) - r, np.maximum(self.a, self.b) + r


class Sphere(Ellipsoid):
    def __init__(self, c, r):
        super().__init__(c, (r, r, r))

    def sdf(self, P):
        return np.linalg.norm(P - self.c, axis=1) - self.r[0]


def smin(a, b, k):
    if k <= 0:
        return np.minimum(a, b)
    h = np.clip(0.5 + 0.5 * (b - a) / k, 0.0, 1.0)
    return b + (a - b) * h - k * h * (1.0 - h)


def smax(a, b, k):
    return -smin(-a, -b, k)


# ------------------------------------------------------------------------------- the field

class Field:
    """A regular grid holding the body's signed distance."""

    def __init__(self, lo, hi, voxel):
        self.voxel = voxel
        self.lo = np.asarray(lo, dtype=np.float64)
        n = np.ceil((np.asarray(hi) - self.lo) / voxel).astype(int) + 1
        self.n = n
        self.axes = [self.lo[i] + voxel * np.arange(n[i]) for i in range(3)]
        self.f = np.full(tuple(n), 1.0, dtype=np.float32)

    def _slab(self, prim, k):
        blo, bhi = prim.bounds()
        blo = blo - k - 2 * self.voxel
        bhi = bhi + k + 2 * self.voxel
        i0 = np.clip(np.floor((blo - self.lo) / self.voxel).astype(int), 0, self.n - 1)
        i1 = np.clip(np.ceil((bhi - self.lo) / self.voxel).astype(int) + 1, 0, self.n)
        sl = tuple(slice(i0[i], i1[i]) for i in range(3))
        gx, gy, gz = np.meshgrid(self.axes[0][sl[0]], self.axes[1][sl[1]], self.axes[2][sl[2]], indexing="ij")
        P = np.stack([gx.ravel(), gy.ravel(), gz.ravel()], axis=1)
        return sl, P, gx.shape

    def add(self, prim, k):
        sl, P, shape = self._slab(prim, k)
        d = prim.sdf(P).reshape(shape).astype(np.float32)
        self.f[sl] = smin(self.f[sl], d, k)

    def carve(self, prim, k):
        sl, P, shape = self._slab(prim, k)
        d = prim.sdf(P).reshape(shape).astype(np.float32)
        self.f[sl] = smax(self.f[sl], -d, k)

    def clip_below(self, z0):
        """Flat soles: nothing below z0."""
        zz = self.axes[2][None, None, :]
        self.f = np.maximum(self.f, (z0 - zz).astype(np.float32))

    def points(self):
        gx, gy, gz = np.meshgrid(*self.axes, indexing="ij")
        return gx, gy, gz

    def sample(self, p):
        """Trilinear sample of the field at a point."""
        idx = (np.asarray(p) - self.lo) / self.voxel
        i0 = np.floor(idx).astype(int)
        t = idx - i0
        acc = 0.0
        for dx in (0, 1):
            for dy in (0, 1):
                for dz in (0, 1):
                    w = (t[0] if dx else 1 - t[0]) * (t[1] if dy else 1 - t[1]) * (t[2] if dz else 1 - t[2])
                    acc += w * self.f[i0[0] + dx, i0[1] + dy, i0[2] + dz]
        return float(acc)


def surface(field, values, level=0.0):
    """Marching cubes on a part's field -> (verts, faces) in world space."""
    v, f, _, _ = measure.marching_cubes(values, level=level, spacing=(field.voxel,) * 3)
    return v + field.lo, f
