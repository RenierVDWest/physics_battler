"""The anatomy of the three bodies, as muscle volumes on the joint layout from bodies.Body, and the
cut into rigid parts.

sculpt(body) -> Field          the whole body's signed distance
cut(body, field) -> {part: (verts, faces)}   one closed surface per part, world space
"""

import numpy as np

from anatomy import F, X, Y, Z, Ellipsoid, Field, RoundCone, Sphere, V, surface, unit


def vec(v):
    return np.array([v.x, v.y, v.z], dtype=np.float64)


def v32(v):
    return np.asarray(v, dtype=np.float32)


def sculpt(body):
    sp, h = body.spec, body.spec.hs
    xmax = max(abs(p.x) for _, p in body.nulls.values()) + 0.11 * h * max(sp.hand, sp.upper_arm, sp.thigh)
    lo = np.array([-xmax, -0.24 * h * max(sp.foot, sp.chest, sp.belly), -0.01])
    hi = np.array([xmax, 0.2 * h * max(sp.glutes, sp.traps, sp.chest), body.head_c.z + body.head_size * 0.62])
    fld = Field(lo, hi, 0.0055 * h)

    def add(prim, k):
        fld.add(prim, k * h)

    hz, pz, wz, cz, nz = body.hip_z, body.pelvis_z, body.waist_z, body.chest_z, body.neck_z
    sz = body.shoulder[1]
    ch, wa, hi_ = sp.chest * h, sp.waist * h, sp.hips * h
    pe, la, tr, gl, be = sp.pecs, sp.lats, sp.traps, sp.glutes, sp.belly
    sw, hw = sp.shoulder_w, sp.hip_w
    nk = sp.neck * h

    # ---- torso
    add(Ellipsoid(V(0, 0.0, cz + 0.11 * h * sp.torso_len), (0.152 * ch, 0.118 * ch, 0.2 * h * sp.torso_len)), 0.04)
    add(Ellipsoid(V(0, 0.012 * h, sz - 0.005 * h), (0.19 * ch * sw, 0.092 * ch, 0.065 * h)), 0.04)
    add(RoundCone(V(0, 0.07 * h, wz - 0.06 * h), V(0, 0.06 * h, sz - 0.04 * h), 0.068 * wa, 0.08 * ch), 0.04)
    add(Ellipsoid(V(0, -0.045 * wa * be, (wz + cz) / 2 + 0.01 * h), (0.105 * wa, 0.072 * wa * be, 0.15 * h)), 0.035)
    add(Ellipsoid(V(0, 0.006 * h, wz + 0.01 * h), (0.136 * wa, 0.1 * wa, 0.15 * h)), 0.05)
    add(Ellipsoid(V(0, 0.004 * h, pz - 0.0 * h), (0.138 * hi_ * hw, 0.1 * hi_, 0.1 * h)), 0.05)
    add(RoundCone(V(0, -0.01 * h, pz - 0.02 * h), V(0, 0.005 * h, hz - 0.065 * h), 0.08 * hi_, 0.035 * hi_), 0.03)
    for s in (-1, 1):
        add(Ellipsoid(V(s * 0.08 * ch, -0.062 * ch, sz - 0.06 * h), (0.095 * ch * pe ** .5, 0.04 * ch * pe ** .8, 0.055 * h * pe ** .5),
                      axis=unit(V(s * 0.45, 0, 1))), 0.03)
        add(Ellipsoid(V(s * 0.118 * ch * la, 0.035 * h, cz + 0.09 * h), (0.06 * ch * la, 0.072 * ch, 0.15 * h),
                      axis=unit(V(s * 0.35, 0, 1))), 0.035)
        add(RoundCone(V(s * 0.03 * h, 0.03 * h, nz + 0.03 * h * tr ** .4), V(s * 0.165 * ch * sw, 0.018 * h, sz + 0.028 * h),
                      0.045 * h * tr ** .6, 0.032 * h * tr ** .5), 0.035)
        add(Ellipsoid(V(s * 0.075 * hi_ * hw, 0.065 * h * gl, hz + 0.005 * h), (0.085 * hi_ * gl ** .5, 0.07 * hi_ * gl, 0.095 * h)), 0.03)
    add(Ellipsoid(V(0, 0.07 * h, sz - 0.06 * h), (0.11 * h * tr ** .5, 0.04 * h * tr ** .6, 0.12 * h)), 0.035)

    # ---- neck and head
    hc = vec(body.head_c)
    u = body.head_size / 0.235
    nt = body.neck_top
    add(RoundCone(V(0, 0.012 * h, nz - 0.03 * h), V(0, hc[1] + 0.012 * u, nt + 0.03 * u), 0.062 * nk, 0.052 * nk), 0.03)
    for s in (-1, 1):   # sternomastoids, ear to collarbone
        add(RoundCone(hc + V(s * 0.045 * u, 0.0, -0.05 * u), V(s * 0.02 * h, -0.045 * h * sp.neck ** .5, nz + 0.005 * h),
                      0.016 * nk, 0.014 * nk), 0.02)

    def hp(dx, dy, dz):
        return hc + V(dx * u, dy * u, dz * u)

    add(Ellipsoid(hp(0, 0.012, 0.03), (0.079 * u, 0.098 * u, 0.098 * u)), 0.015 * u / h)
    add(Ellipsoid(hp(0, -0.038, -0.035), (0.067 * u, 0.06 * u, 0.085 * u)), 0.022 * u / h)
    for s in (-1, 1):
        add(RoundCone(hp(s * 0.058, 0.005, -0.045), hp(s * 0.02, -0.07, -0.098), 0.022 * u, 0.018 * u), 0.02 * u / h)
        add(Ellipsoid(hp(s * 0.047, -0.06, -0.005), (0.022 * u, 0.018 * u, 0.014 * u)), 0.015 * u / h)
        add(Ellipsoid(hp(s * 0.08, 0.01, -0.005), (0.01 * u, 0.02 * u, 0.03 * u)), 0.01 * u / h)
    add(Sphere(hp(0, -0.074, -0.1), 0.024 * u), 0.015 * u / h)
    add(Ellipsoid(hp(0, -0.074, 0.03), (0.06 * u, 0.02 * u, 0.014 * u)), 0.015 * u / h)
    add(RoundCone(hp(0, -0.088, 0.02), hp(0, -0.106, -0.028), 0.007 * u, 0.012 * u), 0.008 * u / h)
    add(Ellipsoid(hp(0, -0.09, -0.056), (0.028 * u, 0.012 * u, 0.01 * u)), 0.006 * u / h)
    for s in (-1, 1):
        fld.carve(Sphere(hp(s * 0.032, -0.094, 0.008), 0.0145 * u), 0.008 * u)

    # ---- arms
    for s in (-1, 1):
        n = "R" if s < 0 else "L"
        S, E, W = vec(body.P(f"UpperArm_{n}")), vec(body.P(f"Forearm_{n}")), vec(body.P(f"Hand_{n}"))
        d = unit(E - S)
        dF = unit(W - E)
        out = s * X
        ua = 1.12 * sp.upper_arm * h
        fa = 1.1 * sp.forearm * h
        dl, bi = sp.deltoid, sp.biceps
        L = np.linalg.norm(E - S)
        add(RoundCone(S, E, 0.045 * ua, 0.036 * ua), 0.02)
        add(Ellipsoid(S + d * 0.15 * L + out * 0.018 * ua, (0.058 * ua * dl, 0.066 * ua * dl, 0.3 * L * dl ** .4), axis=d), 0.02)
        add(Ellipsoid(S + d * 0.55 * L + F * 0.022 * ua, (0.036 * ua * bi, 0.038 * ua * bi, 0.28 * L), axis=d), 0.02)
        add(Ellipsoid(S + d * 0.47 * L - F * 0.028 * ua, (0.04 * ua, 0.042 * ua, 0.35 * L), axis=d), 0.02)
        add(Sphere(E, 0.037 * ua), 0.015)
        Lf = np.linalg.norm(W - E)
        add(RoundCone(E, W, 0.04 * fa, 0.025 * fa), 0.02)
        add(Ellipsoid(E + dF * 0.28 * Lf + out * 0.012 * fa + F * 0.008 * fa, (0.036 * fa, 0.034 * fa, 0.32 * Lf), axis=dF), 0.02)
        add(Ellipsoid(E + dF * 0.26 * Lf - out * 0.015 * fa, (0.033 * fa, 0.032 * fa, 0.3 * Lf), axis=dF), 0.02)
        add(Ellipsoid(W, (0.021 * fa, 0.03 * fa, 0.025 * fa), axis=dF), 0.015)
        hu = sp.hand * h
        add(Ellipsoid(W + dF * 0.045 * hu, (0.02 * hu, 0.042 * hu, 0.05 * hu), axis=dF), 0.01)
        add(Ellipsoid(W + dF * 0.09 * hu - out * 0.01 * hu, (0.032 * hu, 0.044 * hu, 0.035 * hu), axis=dF), 0.012)
        add(RoundCone(W + dF * 0.025 * hu + F * 0.03 * hu, W + dF * 0.075 * hu + F * 0.045 * hu - out * 0.02 * hu,
                      0.015 * hu, 0.012 * hu), 0.008)

    # ---- legs
    for s in (-1, 1):
        n = "R" if s < 0 else "L"
        H, K, A = vec(body.P(f"Thigh_{n}")), vec(body.P(f"Shin_{n}")), vec(body.P(f"Foot_{n}"))
        out = s * X
        th = 1.12 * sp.thigh * h
        sn = sp.shin * h
        ca = sp.calf
        dT = unit(K - H)
        dS = unit(A - K)
        Lt, Ls = np.linalg.norm(K - H), np.linalg.norm(A - K)
        add(RoundCone(H, K, 0.075 * th, 0.047 * th), 0.03)
        add(Ellipsoid(H + dT * 0.45 * Lt + F * 0.028 * th + out * 0.008 * th, (0.068 * th, 0.06 * th, 0.45 * Lt), axis=dT), 0.03)
        add(Ellipsoid(K - dT * 0.24 * Lt - out * 0.03 * th + F * 0.02 * th, (0.042 * th, 0.04 * th, 0.15 * Lt), axis=dT), 0.02)
        add(Ellipsoid(H + dT * 0.5 * Lt + Y * 0.035 * th, (0.06 * th, 0.055 * th, 0.42 * Lt), axis=dT), 0.03)
        add(Ellipsoid(H + dT * 0.22 * Lt - out * 0.04 * th, (0.05 * th, 0.055 * th, 0.3 * Lt), axis=dT), 0.03)
        add(Ellipsoid(H + out * 0.03 * th + dT * 0.05 * Lt, (0.045 * th, 0.055 * th, 0.18 * Lt), axis=dT), 0.03)
        add(Sphere(K, 0.048 * th), 0.02)
        add(Ellipsoid(K + F * 0.042 * th + Z * 0.01 * h, (0.028 * th, 0.016 * th, 0.03 * th)), 0.012)
        add(RoundCone(K, A, 0.045 * sn, 0.027 * sn), 0.02)
        add(Ellipsoid(K + dS * 0.3 * Ls + Y * 0.04 * sn + out * 0.008 * sn, (0.046 * sn * ca, 0.045 * sn * ca, 0.27 * Ls), axis=dS), 0.03)
        add(Ellipsoid(K + dS * 0.34 * Ls + Y * 0.036 * sn - out * 0.018 * sn, (0.036 * sn * ca, 0.036 * sn * ca, 0.22 * Ls), axis=dS), 0.03)
        add(Ellipsoid(K + dS * 0.36 * Ls + F * 0.012 * sn + out * 0.012 * sn, (0.026 * sn, 0.026 * sn, 0.3 * Ls), axis=dS), 0.02)
        add(RoundCone(A + Y * 0.024 * sn + Z * 0.07 * h, A + Y * 0.034 * sn - Z * 0.035 * h, 0.016 * sn, 0.019 * sn), 0.015)
        add(Ellipsoid(A, (0.036 * sn, 0.03 * sn, 0.03 * sn)), 0.015)
        fu = sp.foot * h
        add(Ellipsoid(V(A[0], A[1] + 0.03 * fu, 0.036 * fu), (0.032 * fu, 0.042 * fu, 0.042 * fu)), 0.015)
        add(RoundCone(V(A[0], A[1], A[2] - 0.015 * h), V(A[0] + s * 0.008 * fu, A[1] - 0.12 * fu, 0.026 * fu), 0.036 * fu, 0.03 * fu), 0.02)
        add(Ellipsoid(V(A[0] + s * 0.01 * fu, A[1] - 0.125 * fu, 0.023 * fu), (0.045 * fu, 0.036 * fu, 0.023 * fu)), 0.015)
        add(Ellipsoid(V(A[0] + s * 0.008 * fu, A[1] - 0.172 * fu, 0.017 * fu), (0.04 * fu, 0.03 * fu, 0.017 * fu)), 0.012)
    fld.clip_below(0.0)
    return fld


# ------------------------------------------------------------------------------- the cut

def seg_dist(P, a, b):
    ab = b - a
    t = np.clip(((P - a) @ ab) / (ab @ ab), 0.0, 1.0)
    return np.linalg.norm(P - (a + t[:, None] * ab), axis=1)


def chain_dist(P, pts):
    return np.min([seg_dist(P, a, b) for a, b in zip(pts[:-1], pts[1:])], axis=0)


JOINTS = [("Spine", "Pelvis", "Spine"), ("Chest", "Spine", "Chest"), ("Head", "Chest", "Head"),
          ("UpperArm_R", "Chest", "UpperArm_R"), ("Forearm_R", "UpperArm_R", "Forearm_R"),
          ("Hand_R", "Forearm_R", "Hand_R"), ("UpperArm_L", "Chest", "UpperArm_L"),
          ("Forearm_L", "UpperArm_L", "Forearm_L"), ("Hand_L", "Forearm_L", "Hand_L"),
          ("Thigh_R", "Pelvis", "Thigh_R"), ("Shin_R", "Thigh_R", "Shin_R"), ("Foot_R", "Shin_R", "Foot_R"),
          ("Thigh_L", "Pelvis", "Thigh_L"), ("Shin_L", "Thigh_L", "Shin_L"), ("Foot_L", "Shin_L", "Foot_L")]


def cut(body, fld, parts):
    sp, h = body.spec, body.spec.hs
    gx, gy, gz = fld.points()
    shape = gx.shape
    P = np.stack([gx.ravel(), gy.ravel(), gz.ravel()], axis=1).astype(np.float32)
    del gx, gy, gz
    zf = P[:, 2]
    f = fld.f.ravel()

    def pv(name):
        return v32(vec(body.P(name)))

    region = {}
    arm, leg = {}, {}
    dA, dL = {}, {}
    for s in (-1, 1):
        n = "R" if s < 0 else "L"
        S, E, W, slot = pv(f"UpperArm_{n}"), pv(f"Forearm_{n}"), pv(f"Hand_{n}"), pv(f"Slot_Weapon_{n}")
        dA[s] = chain_dist(P, [S, E, W, slot + (slot - W) * 0.5])
        H, K, A = pv(f"Thigh_{n}"), pv(f"Shin_{n}"), pv(f"Foot_{n}")
        toe = V(A[0], A[1] - 0.16 * sp.foot * h, 0.02 * h)
        dL[s] = chain_dist(P, [H, K, A, toe])
    head_c = v32(vec(body.head_c))
    trunk_axis = chain_dist(P, [v32(V(0, 0.01 * h, body.hip_z - 0.06 * h)), v32(V(0, 0.01 * h, body.neck_z))])
    rT = 0.13 * sp.chest * h
    for s in (-1, 1):
        n = "R" if s < 0 else "L"
        S = pv(f"UpperArm_{n}")
        nS = v32(unit(s * X + 0.3 * Z))
        shoulder = (P - (S - s * X * 0.022 * h).astype(np.float32)) @ nS
        rA = 0.045 * 1.12 * sp.upper_arm * h
        below = (trunk_axis - rT) - (dA[s] - rA)
        w = np.clip((P[:, 2] - (S[2] - 0.12 * h)) / (0.1 * h), 0.0, 1.0)
        w = w * w * (3 - 2 * w)
        armness = below + (shoulder - below) * w
        others = np.minimum(np.minimum(dL[-1], dL[1]), dA[-s])
        head_zone = np.linalg.norm(P - head_c, axis=1) - 0.62 * body.head_size
        arm[s] = np.maximum(np.maximum(-armness, dA[s] - others), -head_zone)
        H = pv(f"Thigh_{n}")
        nH = unit(-Z + s * 0.55 * X)
        hip = (P - H) @ nH
        others = np.minimum(np.minimum(dA[-1], dA[1]), dL[-s])
        leg[s] = np.maximum(-hip, dL[s] - others)
    trunk = -np.minimum(np.minimum(arm[-1], arm[1]), np.minimum(leg[-1], leg[1]))
    del dA, dL

    nz, wz, cz = body.neck_z, body.waist_z, body.chest_z
    rho = np.hypot(P[:, 0], P[:, 1] - 0.01 * h)
    cone = (nz + 0.9 * np.maximum(0.0, rho - 0.05 * sp.neck * h)) - zf   # > 0 below the neck cut
    region["Pelvis"] = np.maximum(trunk, zf - wz)
    region["Spine"] = np.maximum(trunk, np.maximum(wz - zf, zf - cz))
    u = body.head_size / 0.235
    face_c = head_c + v32(V(0, -0.03 * u, -0.03 * u))
    skull = np.linalg.norm(P - face_c, axis=1) - 0.45 * body.head_size   # the whole head, jaw included
    head = np.minimum(cone, skull)
    region["Chest"] = np.maximum(trunk, np.maximum(cz - zf, -head))
    region["Head"] = np.maximum(trunk, head)
    del trunk, cone, rho, skull, head
    for s in (-1, 1):
        n = "R" if s < 0 else "L"
        S, E, W = pv(f"UpperArm_{n}"), pv(f"Forearm_{n}"), pv(f"Hand_{n}")
        dU, dF = unit(E - S), unit(W - E)
        nE = unit(dU + dF)
        pe = (P - E) @ nE
        pw = (P - W) @ dF
        region[f"UpperArm_{n}"] = np.maximum(arm[s], pe)
        region[f"Forearm_{n}"] = np.maximum(arm[s], np.maximum(-pe, pw))
        region[f"Hand_{n}"] = np.maximum(arm[s], -pw)
        H, K, A = pv(f"Thigh_{n}"), pv(f"Shin_{n}"), pv(f"Foot_{n}")
        nK = unit(unit(K - H) + unit(A - K))
        nA = unit(unit(A - K) + F)
        pk = (P - K) @ nK
        pa = (P - A) @ nA
        region[f"Thigh_{n}"] = np.maximum(leg[s], pk)
        region[f"Shin_{n}"] = np.maximum(leg[s], np.maximum(-pk, pa))
        region[f"Foot_{n}"] = np.maximum(leg[s], -pa)
    del arm, leg

    # ball joints: each part carries a ball at each of its pivots, clipped to the body surface
    balls = {p: [] for p in parts}
    for pivot, parent, child in JOINTS:
        c = pv(pivot)
        depth = -fld.sample(c)
        r = max(depth * 0.92, 0.01 * h)
        balls[parent].append((c, r * 0.985))
        balls[child].append((c, r * 0.95))

    out = {}
    for part in parts:
        val = np.maximum(f, region[part])
        for c, r in balls[part]:
            ball = np.linalg.norm(P - c, axis=1) - r
            val = np.minimum(val, np.maximum(ball, f))
        vol = val.reshape(shape)
        inside = np.argwhere(vol < 0)
        if len(inside) == 0:
            raise RuntimeError(f"{part}: empty region")
        i0 = np.maximum(inside.min(axis=0) - 2, 0)
        i1 = np.minimum(inside.max(axis=0) + 3, np.array(shape))
        sub = vol[i0[0]:i1[0], i0[1]:i1[1], i0[2]:i1[2]]
        sub = np.pad(sub, 1, constant_values=1.0)
        verts, faces = surface(fld, sub)
        verts += (i0 - 1) * fld.voxel
        out[part] = (verts, faces)
    return out


# ------------------------------------------------------------------------------- colliders

def fit_colliders(body, surfaces):
    """Colliders fitted to the sculpted parts: boxes for the trunk, a sphere for the head,
    capsules along the limb bones, boxes for hands and feet. Each stops short of its joints, so
    no two touch at rest (check_bodies.py verifies the clearance)."""
    from mathutils import Vector
    sp, h = body.spec, body.spec.hs
    gap = 0.02 * h

    def pv(name):
        return vec(body.P(name))

    def Vv(a):
        return Vector(tuple(float(x) for x in a))

    def box(lo, hi):
        lo, hi = np.asarray(lo), np.asarray(hi)
        return {"type": "box", "center": Vv((lo + hi) / 2), "size": Vv(hi - lo)}

    c = {}
    # trunk boxes, between the joint heights, as wide as the part there (a little inside it)
    bands = {"Pelvis": (body.hip_z + 0.025 * h, body.waist_z - gap / 2),
             "Spine": (body.waist_z + gap / 2, body.chest_z - gap / 2),
             "Chest": (body.chest_z + gap / 2, body.shoulder[1] + 0.02 * h)}
    for part, (z0, z1) in bands.items():
        v = surfaces[part][0]
        v = v[(v[:, 2] > z0) & (v[:, 2] < z1)]
        half_x = np.percentile(np.abs(v[:, 0]), 92) * 0.86
        y0, y1 = np.percentile(v[:, 1], [6, 94])
        yc, hy = (y0 + y1) / 2, (y1 - y0) / 2 * 0.86
        c[part] = box((-half_x, yc - hy, z0), (half_x, yc + hy, z1))
    v = surfaces["Head"][0]
    v = v[v[:, 2] > body.neck_top + 0.02 * h]
    centre = (v.min(axis=0) + v.max(axis=0)) / 2
    r = np.median(np.linalg.norm(v - centre, axis=1)) * 0.84
    c["Head"] = {"type": "sphere", "center": Vv(centre), "radius": float(r)}

    def capsule(part, a, b, gap_a, gap_b, k=0.82):
        v = surfaces[part][0]
        ab = b - a
        L = np.linalg.norm(ab)
        d = ab / L
        t = (v - a) @ d
        mid = v[(t > 0.25 * L) & (t < 0.75 * L)]
        radial = np.linalg.norm((mid - a) - np.outer((mid - a) @ d, d), axis=1)
        r = float(np.percentile(radial, 25) * k)
        return {"type": "capsule", "start": Vv(a + d * (gap_a + r)), "end": Vv(b - d * (gap_b + r)), "radius": r}

    for s in (-1, 1):
        n = "R" if s < 0 else "L"
        S, E, W, slot = pv(f"UpperArm_{n}"), pv(f"Forearm_{n}"), pv(f"Hand_{n}"), pv(f"Slot_Weapon_{n}")
        c[f"UpperArm_{n}"] = capsule(f"UpperArm_{n}", S, E, 0.03 * h * sp.deltoid, gap)
        c[f"Forearm_{n}"] = capsule(f"Forearm_{n}", E, W, gap, 0.012 * h)
        v = surfaces[f"Hand_{n}"][0]
        v = v[(v - W) @ unit(slot - W) > 0.012 * h]
        lo, hi = v.min(axis=0), v.max(axis=0)
        mid, half = (lo + hi) / 2, (hi - lo) / 2 * 0.78
        c[f"Hand_{n}"] = box(mid - half, mid + half)
        H, K, A = pv(f"Thigh_{n}"), pv(f"Shin_{n}"), pv(f"Foot_{n}")
        c[f"Thigh_{n}"] = capsule(f"Thigh_{n}", H, K, body.hip_z - body.hip_z + 0.05 * h, gap)
        c[f"Shin_{n}"] = capsule(f"Shin_{n}", K, A, gap, 0.03 * h * sp.foot ** .5)
        v = surfaces[f"Foot_{n}"][0]
        v = v[v[:, 2] < A[2] * 0.75]
        lo, hi = v.min(axis=0), v.max(axis=0)
        mid, half = (lo + hi) / 2, (hi - lo) / 2
        c[f"Foot_{n}"] = box((mid[0] - half[0] * 0.8, mid[1] - half[1] * 0.9, 0.0),
                             (mid[0] + half[0] * 0.8, mid[1] + half[1] * 0.9, hi[2] * 0.85))
    # keep the chest box clear of the upper arm capsules
    arm = c["UpperArm_L"]
    limit = min(arm["start"].x, arm["end"].x) - arm["radius"] - gap
    chest = c["Chest"]
    if chest["size"].x / 2 > limit:
        chest["size"].x = 2 * limit
    return c


def collider_clearance(body, cols, parts):
    """Smallest gap between colliders of different parts (sampled surfaces), worst pairs first."""
    rng = np.random.default_rng(1)
    N = 3000

    def pts(col):
        if col["type"] == "sphere":
            v = rng.normal(size=(N, 3))
            return np.array(col["center"]) + col["radius"] * v / np.linalg.norm(v, axis=1)[:, None]
        if col["type"] == "box":
            half = np.array(col["size"]) / 2
            p = rng.uniform(-1, 1, size=(N, 3))
            ax = rng.integers(0, 3, N)
            p[np.arange(N), ax] = np.sign(p[np.arange(N), ax])
            return np.array(col["center"]) + p * half
        a, b, r = np.array(col["start"]), np.array(col["end"]), col["radius"]
        t = rng.uniform(-0.3, 1.3, N)[:, None]
        v = rng.normal(size=(N, 3))
        v /= np.linalg.norm(v, axis=1)[:, None]
        return a + np.clip(t, 0, 1) * (b - a) + r * v

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

    out = []
    for i, a in enumerate(parts):
        for b in parts[i + 1:]:
            out.append((float(min(sdf(cols[b], pts(cols[a])).min(), sdf(cols[a], pts(cols[b])).min())), a, b))
    return sorted(out)
