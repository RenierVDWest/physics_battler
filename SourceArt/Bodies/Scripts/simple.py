"""A simple, elegant body in the spirit of an artist's mannequin: every part is a smooth turned
form (a lathe around its bone), with a visible ball at each joint. No muscles, no face.

build(body) -> {part: (verts, faces, material_per_face)} in world space (Blender axes: Z up,
facing -Y, the right side on -X). Material 0 is the body, 1 the joint balls.
"""

import math

import numpy as np

F = np.array([0, -1.0, 0])
Z = np.array([0, 0, 1.0])
X = np.array([1.0, 0, 0])
BODY, JOINT = 0, 1


def unit(v):
    v = np.asarray(v, dtype=np.float64)
    return v / np.linalg.norm(v)


def vec(v):
    return np.array([v.x, v.y, v.z], dtype=np.float64)


class Shells:
    def __init__(self):
        self.verts, self.faces, self.mats = [], [], []

    def lathe(self, a, axis, profile, mat=BODY, n=28, squash=(1.0, 1.0), ref=F, offsets=None):
        """profile: [(t, r)] along the axis from a, first and last r = 0 (poles).
        squash scales the ring across (x: the side, y: front-back). offsets: [(t, vector)] shifts."""
        axis = unit(axis)
        y = np.asarray(ref, dtype=np.float64) - axis * np.dot(ref, axis)
        if np.linalg.norm(y) < 1e-6:
            y = Z - axis * np.dot(Z, axis)
        y = unit(y)
        x = np.cross(y, axis)
        base = len(self.verts)
        rings = []
        for k, (t, r) in enumerate(profile):
            c = np.asarray(a) + axis * t
            if offsets is not None:
                c = c + offsets[k]
            if r <= 1e-9:
                self.verts.append(c)
                rings.append([len(self.verts) - 1])
                continue
            ring = []
            for i in range(n):
                th = 2 * math.pi * i / n
                self.verts.append(c + x * math.sin(th) * r * squash[0] + y * math.cos(th) * r * squash[1])
                ring.append(len(self.verts) - 1)
            rings.append(ring)
        for r0, r1 in zip(rings[:-1], rings[1:]):
            if len(r0) == 1:
                for i in range(n):
                    self._face((r0[0], r1[(i + 1) % n], r1[i]), mat)
            elif len(r1) == 1:
                for i in range(n):
                    self._face((r0[i], r0[(i + 1) % n], r1[0]), mat)
            else:
                for i in range(n):
                    self._face((r0[i], r0[(i + 1) % n], r1[(i + 1) % n], r1[i]), mat)
        return base

    def _face(self, f, mat):
        # The ring/pole orders above run clockwise seen from outside; reversed, every face points outward
        # (Unity culls back faces, so inward faces show the inside of each shell).
        self.faces.append(tuple(reversed(f)))
        self.mats.append(mat)

    def sphere(self, c, r, mat=JOINT, n=24, squash=(1.0, 1.0, 1.0), axis=Z):
        prof = [(-r * squash[2] * math.cos(math.pi * k / 12), r * math.sin(math.pi * k / 12)) for k in range(13)]
        self.lathe(c, axis, prof, mat, n, squash=(squash[0], squash[1]))

    def capsule(self, a, b, radii, mat=BODY, n=28, squash=(1.0, 1.0), ref=F):
        """A smooth tapered form from a to b. radii: [(fraction of the length, radius)], the first
        at 0 and the last at 1; both ends close with round domes."""
        a, b = np.asarray(a, dtype=np.float64), np.asarray(b, dtype=np.float64)
        L = np.linalg.norm(b - a)
        r0, r1 = radii[0][1], radii[-1][1]
        prof = [(-r0 * math.cos(math.pi / 2 * k / 6), r0 * math.sin(math.pi / 2 * k / 6)) for k in range(6)]
        pts = [(f * L, r) for f, r in radii]
        # smooth the interior with a few in-between samples (Catmull-Rom)
        dense = []
        for i in range(len(pts) - 1):
            p0, p1 = pts[max(i - 1, 0)], pts[i]
            p2, p3 = pts[i + 1], pts[min(i + 2, len(pts) - 1)]
            for s in range(4):
                u = s / 4
                t = p1[0] + (p2[0] - p1[0]) * u
                r = 0.5 * ((2 * p1[1]) + (-p0[1] + p2[1]) * u + (2 * p0[1] - 5 * p1[1] + 4 * p2[1] - p3[1]) * u * u
                           + (-p0[1] + 3 * p1[1] - 3 * p2[1] + p3[1]) * u ** 3)
                dense.append((t, r))
        dense.append(pts[-1])
        prof += dense
        prof += [(L + r1 * math.sin(math.pi / 2 * k / 6), r1 * math.cos(math.pi / 2 * k / 6)) for k in range(1, 7)]
        self.lathe(a, b - a, prof, mat, n, squash=squash, ref=ref)

    def tube(self, points, r, ref, mat=BODY, n=12):
        """A round tube of radius r along a path (a curled finger), closed with round ends; ref is
        a direction square to the whole path (the rings' orientation)."""
        pts = [np.asarray(p, dtype=np.float64) for p in points]
        tangents = []
        for i in range(len(pts)):
            a, b = pts[max(i - 1, 0)], pts[min(i + 1, len(pts) - 1)]
            tangents.append(unit(b - a))
        y = unit(ref)
        rings = []
        # the round ends: a few rings back from the first point and on from the last
        caps = 3
        for k in range(caps, 0, -1):
            t = math.pi / 2 * k / caps
            rings.append((pts[0] - tangents[0] * r * math.sin(t), r * math.cos(t), tangents[0]))
        for p, t in zip(pts, tangents):
            rings.append((p, r, t))
        for k in range(1, caps + 1):
            t = math.pi / 2 * k / caps
            rings.append((pts[-1] + tangents[-1] * r * math.sin(t), r * math.cos(t), tangents[-1]))
        ids = []
        for c, rr, t in rings:
            if rr < 1e-6:
                self.verts.append(c)
                ids.append([len(self.verts) - 1])
                continue
            yy = unit(y - t * np.dot(y, t))
            xx = np.cross(yy, t)
            ring = []
            for i in range(n):
                th = 2 * math.pi * i / n
                self.verts.append(c + xx * math.sin(th) * rr + yy * math.cos(th) * rr)
                ring.append(len(self.verts) - 1)
            ids.append(ring)
        for r0, r1 in zip(ids[:-1], ids[1:]):
            if len(r0) == 1:
                for i in range(n):
                    self._face((r0[0], r1[(i + 1) % n], r1[i]), mat)
            elif len(r1) == 1:
                for i in range(n):
                    self._face((r0[i], r0[(i + 1) % n], r1[0]), mat)
            else:
                for i in range(n):
                    self._face((r0[i], r0[(i + 1) % n], r1[(i + 1) % n], r1[i]), mat)

    def result(self):
        return np.array(self.verts), self.faces, self.mats


def smooth_profile(prof, offs, steps=4):
    """Catmull-Rom in-betweens for a lathe profile (and its offsets)."""
    out_p, out_o = [], []
    n = len(prof)
    for i in range(n - 1):
        p0, p1, p2, p3 = prof[max(i - 1, 0)], prof[i], prof[i + 1], prof[min(i + 2, n - 1)]
        for k in range(steps):
            u = k / steps
            def cr(a, b, c, d):
                return 0.5 * (2 * b + (-a + c) * u + (2 * a - 5 * b + 4 * c - d) * u * u + (-a + 3 * b - 3 * c + d) * u ** 3)
            t = cr(p0[0], p1[0], p2[0], p3[0])
            r = max(cr(p0[1], p1[1], p2[1], p3[1]), 0.0) if (i > 0 or k > 0) else 0.0
            out_p.append((t, r))
            out_o.append(offs[i] + (offs[i + 1] - offs[i]) * u)
    out_p.append(prof[-1])
    out_o.append(offs[-1])
    return out_p, out_o


# The channel through a closed fist (metres at medium size; weapons scale with the body): a little
# wider than the thickest grip (the club's binding, 2.1 cm), so a handle passes through it.
GRIP_CHANNEL = 0.022


def fist(s, W, G, out, h, hu):
    """A closed fist round the weapon slot G (milestone 9b): the grip runs through it along the
    knuckles (the slot's forward, the body's front at rest), the back of the hand on the outside,
    the palm on the inside, four fingers curled round the far side of the grip into the palm, the
    thumb over the first two. Built of plain turned forms, as the rest of the mannequin."""
    d = unit(G - W)                       # along the hand, from the wrist
    f = unit(F - d * np.dot(F, d))         # the grip's line (index at the front)
    o = unit(np.cross(d, f))
    if np.dot(o, out) < 0:
        o = -o                             # the back of the hand, outward
    L = np.linalg.norm(G - W)
    rg = GRIP_CHANNEL * h
    ft = 0.0105 * hu                       # a finger's radius
    rc = rg + ft                           # the fingers' line round the grip

    def arc(theta, r=rc):
        t = math.radians(theta)
        return G + r * (math.cos(t) * o + math.sin(t) * d)

    # The palm and the back of the hand, one block from the wrist to the grip (stopping short of
    # the channel), and the back on round the outside to the knuckles.
    reach = L - rg - 0.003 * hu
    s.sphere(W + d * 0.5 * reach + o * 0.006 * hu, 1.0, mat=BODY, n=20,
             squash=(0.033 * hu, 0.046 * hu, 0.5 * reach + 0.004 * hu), axis=d)
    K = arc(20.0, rc + 0.2 * ft)
    back0 = W + d * 0.02 * hu + o * 0.012 * hu
    s.sphere((back0 + K) / 2, 1.0, mat=BODY, n=20,
             squash=(0.017 * hu, 0.045 * hu, 0.5 * np.linalg.norm(K - back0) + 0.006 * hu), axis=unit(K - back0))
    # The ball of the thumb, on the inside at the front.
    s.sphere(W + d * 0.045 * hu + f * 0.028 * hu - o * 0.018 * hu, 1.0, mat=BODY, n=16,
             squash=(0.016 * hu, 0.019 * hu, 0.03 * hu), axis=d)
    # Four fingers, index (front) to little finger, curled from the knuckles round the far side.
    spacing = 0.0215 * hu
    for k, (w, size, tuck) in enumerate([(1.5, 1.0, 215.0), (0.5, 1.05, 220.0), (-0.5, 1.0, 215.0), (-1.5, 0.85, 205.0)]):
        r = ft * size
        across = f * (w * spacing)
        s.tube([arc(a) + across for a in np.linspace(-5.0, tuck, 12)], r, f)
        s.sphere(arc(15.0, rc + 0.25 * ft) + across, r * 1.18, mat=BODY, n=10)   # (the knuckle)
    # The thumb: from the heel of the palm at the front, over the first two fingers on the inside.
    T0 = W + d * 0.04 * hu + f * 0.034 * hu - o * 0.02 * hu
    T1 = arc(195.0, rc + 0.7 * ft) + f * 0.042 * hu
    T2 = arc(172.0, rc + 1.7 * ft) + f * 0.004 * hu
    s.capsule(T0, T1, [(0, 0.0135 * hu), (1, 0.0115 * hu)], n=12, ref=d)
    s.capsule(T1, T2, [(0, 0.0115 * hu), (1, 0.0098 * hu)], n=12, ref=d)


def build(body):
    """Every girth is the medium body's, times the height scale and the spec's factor for that
    region (bodies.SPECS), so small and large keep the style with their own proportions."""
    sp, h = body.spec, body.spec.hs
    parts = {}
    hz, pz, wz, cz, nz = body.hip_z, body.pelvis_z, body.waist_z, body.chest_z, body.neck_z
    sx = body.shoulder[0]
    hi = h * sp.hips * sp.hip_w ** 0.5
    wa = h * sp.waist
    ch = h * sp.chest
    nk = h * sp.neck
    hd = h * sp.head
    ua, fa, hu = h * sp.upper_arm, h * sp.forearm, h * sp.hand
    th, sn, fu = h * sp.thigh, h * sp.shin, h * sp.foot

    # pelvis: a smooth rounded block, hips widest
    s = Shells()
    s.capsule((0, 0.006 * h, hz - 0.055 * h), (0, 0.006 * h, wz + 0.01 * h),
              [(0, 0.075 * hi), (0.25, 0.14 * hi), (0.55, 0.15 * hi), (0.85, 0.13 * wa), (1, 0.115 * wa)],
              squash=(1.0, 0.68))
    parts["Pelvis"] = s
    # spine: a narrow waist
    s = Shells()
    s.capsule((0, 0.004 * h, wz - 0.01 * h), (0, 0.004 * h, cz + 0.02 * h),
              [(0, 0.11 * wa), (0.5, 0.112 * wa), (1, 0.122 * ch)], squash=(1.0, 0.7))
    parts["Spine"] = s
    # chest: an inverted egg, broad at the shoulders, tapering to the waist
    s = Shells()
    s.capsule((0, 0.0, cz - 0.01 * h), (0, 0.006 * h, nz - 0.005 * h),
              [(0, 0.12 * ch), (0.3, 0.16 * ch), (0.62, 0.9 * sx), (0.8, 0.89 * sx), (0.92, 0.73 * sx),
               (1, 0.065 * nk / sp.neck ** 0.3)],
              squash=(1.0, 0.62 * sp.chest ** 0.15))
    parts["Chest"] = s
    # head: a neck and an egg, the narrow end at the chin
    s = Shells()
    s.capsule((0, 0.004 * h, nz - 0.02 * h), (0, 0.0, body.neck_top + 0.03 * hd),
              [(0, 0.044 * nk), (1, 0.04 * nk)], mat=JOINT, n=20)
    hc = vec(body.head_c)
    chin = hc[2] - 0.112 * hd
    crown = hc[2] + 0.118 * hd
    L = crown - chin
    prof = [(0, 0.0), (0.02 * L, 0.03 * hd), (0.08 * L, 0.052 * hd), (0.2 * L, 0.07 * hd), (0.38 * L, 0.082 * hd),
            (0.55 * L, 0.088 * hd), (0.72 * L, 0.084 * hd), (0.86 * L, 0.068 * hd), (0.95 * L, 0.042 * hd), (L, 0.0)]
    offs = [np.array([0, -0.012 * hd * (1 - f / L) ** 2, 0]) for f, _ in prof]   # chin a little forward
    prof, offs = smooth_profile(prof, offs)
    s.lathe(np.array([0, hc[1], chin]), Z, prof, BODY, n=36, squash=(1.0, 1.14), offsets=offs)
    parts["Head"] = s

    for side in (-1, 1):
        n = "R" if side < 0 else "L"
        S, E, W = vec(body.P(f"UpperArm_{n}")), vec(body.P(f"Forearm_{n}")), vec(body.P(f"Hand_{n}"))
        d = unit(W - E)
        out = side * X
        s = Shells()
        s.sphere(S, 0.058 * ua * sp.deltoid ** 0.5)
        s.capsule(S + unit(E - S) * 0.03 * h, E, [(0, 0.05 * ua), (0.3, 0.049 * ua), (1, 0.038 * ua)])
        parts[f"UpperArm_{n}"] = s
        s = Shells()
        s.sphere(E, 0.043 * max(ua, fa))
        s.capsule(E + d * 0.02 * h, W, [(0, 0.04 * fa), (0.25, 0.042 * fa), (1, 0.028 * fa)])
        parts[f"Forearm_{n}"] = s
        s = Shells()
        s.sphere(W, 0.03 * max(fa, hu * 0.8))
        fist(s, W, vec(body.P(f"Slot_Weapon_{n}")), out, h, hu)
        parts[f"Hand_{n}"] = s

        H, K, A = vec(body.P(f"Thigh_{n}")), vec(body.P(f"Shin_{n}")), vec(body.P(f"Foot_{n}"))
        s = Shells()
        s.sphere(H, 0.075 * th)
        s.capsule(H + unit(K - H) * 0.04 * h, K - unit(K - H) * 0.012 * h,
                  [(0, 0.078 * th), (0.25, 0.076 * th), (1, 0.049 * max(th * 0.95, sn))])
        parts[f"Thigh_{n}"] = s
        s = Shells()
        s.sphere(K, 0.053 * max(th * 0.95, sn))
        s.capsule(K + unit(A - K) * 0.03 * h, A, [(0, 0.047 * sn), (0.25, 0.05 * sn), (0.8, 0.033 * sn), (1, 0.029 * sn)])
        parts[f"Shin_{n}"] = s
        s = Shells()
        s.sphere(A, 0.034 * max(sn, fu * 0.8))
        heel = np.array([A[0], A[1] + 0.045 * fu, 0.04 * fu])
        toe = np.array([A[0] + side * 0.01 * fu, A[1] - 0.17 * fu, 0.03 * fu])
        s.capsule(heel, toe, [(0, 0.038 * fu), (0.35, 0.045 * fu), (0.75, 0.046 * fu), (1, 0.034 * fu)],
                  squash=(1.0, 0.75), ref=Z)
        v, f, m = s.result()
        v[:, 2] = np.maximum(v[:, 2], 0.0)      # flat sole on the ground
        s.verts = list(v)
        parts[f"Foot_{n}"] = s
    return {p: s.result() for p, s in parts.items()}
