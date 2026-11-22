"""zbuf.py: a true depth-tested picture of triangles (matplotlib's 3D sorts whole faces and paints skin over cloth
wrongly), shared by the cloth pictures. The same drawing as Tools/Headless/outfit.py."""
import numpy as np

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
                    for c_ in range(3):
                        img[y, x, c_] = w0 * col[k, 0, c_] + w1 * col[k, 1, c_] + w2 * col[k, 2, c_]


def zrender(tri, col, lo_u, hi_u, lo_v, hi_v, px=420):
    """tri: (n, 3, 3) in view space (x across, y depth away from the eye, z up); col: (n, 3) a colour a triangle, or
    (n, 3, 3) a colour a corner (blended across the triangle)."""
    tri = np.ascontiguousarray(tri, dtype=np.float64)
    col = np.asarray(col, dtype=np.float64)
    if col.ndim == 2:
        col = np.repeat(col[:, None, :], 3, axis=1)
    col = np.ascontiguousarray(col)
    scale = px / (hi_u - lo_u)
    W = px
    H = int((hi_v - lo_v) * scale)
    img = np.ones((H, W, 3))
    depth = np.full((H, W), 1e9)
    _raster(tri, col, W, H, lo_u, lo_v, scale, img, depth)
    return img
