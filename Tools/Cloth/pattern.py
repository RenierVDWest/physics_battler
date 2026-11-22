#!/usr/bin/env python3
"""pattern.py [out.json] [picture.png]: the shirt and trousers cut as a tailor would (the plan "Clothes cut and sewn like
real clothes", step 1, 22 November).

Each piece is a flat outline in centimetres, made of named segments; each seam names the two segments sewn together,
each run the same way. Every piece is cut into even triangles about 2 cm across, its seam edges given matching points.
Each piece is also put round the body (the arrangement), a few centimetres off it, with the arms raised 60 degrees so
the armpits are open (the drape lowers them to the rest pose, as Marvelous Designer's avatar can be posed).
Writes Assets/_Battler/Resources/Locomotion/GarmentPattern_Medium.json (positions in the hips' space, metres) and a
picture of the flat pieces with their seams. Run from the repository root: python3 -I Tools/Cloth/pattern.py"""
import json, math, os, sys
import numpy as np
from scipy.spatial import Delaunay

H = 2.0  # cm, the triangles' size

# ---- The Medium (rest pose, metres; see measure_body.py) -------------------------------------------------------------
PELVIS_Y = 0.985
SHOULDER = np.array([0.205, 1.465, 0.0])     # right shoulder joint
WRIST = np.array([0.362, 0.917, -0.017])     # right hand joint
HIP = np.array([0.095, 0.93, 0.0])           # right hip joint
ANKLE = np.array([0.095, 0.085, -0.006])
ARMS_UP = 60.0                               # degrees from straight down while sewing
# The trunk's half-width and front/back at heights (from the mesh slices; below 0.95 m both thighs), for the arrangement.
TRUNK = [(0.78, 0.170, 0.076), (0.90, 0.172, 0.077), (0.95, 0.167, 0.106), (1.00, 0.151, 0.109), (1.10, 0.130, 0.094), (1.20, 0.112, 0.082),
         (1.30, 0.128, 0.088), (1.40, 0.175, 0.111), (1.46, 0.187, 0.120), (1.50, 0.182, 0.118), (1.55, 0.079, 0.046)]

# ---- The draft (cm) ---------------------------------------------------------------------------------------------------
P = dict(
    W=30.0,        # half the front (and back) at the chest: chest 96 + 24 ease = 120
    L=75.0,        # side neck point to hem, over the shoulder (66 hung to 0.97 m on him, above the hips: 22 November)
    neckW=7.5, neckF=8.0, neckB=2.0,
    shDrop=4.5, shPt=23.5,   # (22.5 left the seam inside his shoulder ball: the sleeve top stretched 15% over it)
    ahD=21.5,      # armhole depth below the shoulder point (deeper with the wider shoulder, so the sleeve cap keeps its height)
    sleeve=58.0, bicep=44.0, cuff=28.0,   # (to his wrist; his fist is 33 cm round, which no cuff here can pass: 60 and 59 caught on it)
    band=3.0,
    hipF=24.0, hipB=26.0, extF=6.0, extB=12.0, riseF=25.0, riseB=30.0, inseam=79.0,
    hemF=21.0, hemB=23.0, kneeF=24.0, kneeB=26.0, knee=46.0,
    waistband=80.0, wbH=4.0,   # (73 cm of body 1.07 m up, 5 cm for the cloth kept 8 mm off him, 2 cm; drape.py ties the drawstring)
)


def bez(p0, p1, p2, n=40):
    t = np.linspace(0, 1, n)[:, None]
    return (1 - t) ** 2 * np.array(p0, float) + 2 * (1 - t) * t * np.array(p1, float) + t ** 2 * np.array(p2, float)


def line(a, b):
    return np.array([a, b], float)


def length(poly):
    return float(np.sum(np.linalg.norm(np.diff(poly, axis=0), axis=1)))


def body_piece(front):
    W, L, nw, sd, sp, ah = P["W"], P["L"], P["neckW"], P["shDrop"], P["shPt"], P["ahD"]
    nd = P["neckF"] if front else P["neckB"]
    yArm = L - sd - ah
    segs = [
        ("hem", line((-W, 0), (W, 0))),
        ("side_px", line((W, 0), (W, yArm))),
        ("arm_px", bez((W, yArm), (sp - 0.5, yArm + 2.0), (sp, L - sd))),
        ("shoulder_px", line((sp, L - sd), (nw, L))),
        ("neck", bez((nw, L), (nw * 0.9, L - nd), (0, L - nd), 20)[:-1].tolist() + bez((0, L - nd), (-nw * 0.9, L - nd), (-nw, L), 20).tolist()),
        ("shoulder_nx", line((-nw, L), (-sp, L - sd))),
        ("arm_nx", bez((-sp, L - sd), (-sp + 0.5, yArm + 2.0), (-W, yArm))),
        ("side_nx", line((-W, yArm), (-W, 0))),
    ]
    return [(n, np.array(s, float)) for n, s in segs]


def sleeve_piece(armhole_half):
    """A set-in sleeve: its cap's height found so each half of the cap is as long as the armhole it is sewn to."""
    b, c, ln = P["bicep"] / 2, P["cuff"] / 2, P["sleeve"]
    lo, hi = 2.0, 25.0
    for _ in range(50):
        capH = 0.5 * (lo + hi)
        half = length(bez((b, ln - capH), (b * 0.45, ln + capH * 0.05), (0, ln)))
        if half < armhole_half:
            lo = capH
        else:
            hi = capH
    segs = [
        ("cuff", line((-c, 0), (c, 0))),
        ("under_px", line((c, 0), (b, ln - capH))),
        ("capF", bez((b, ln - capH), (b * 0.45, ln + capH * 0.05), (0, ln))),
        ("capB", bez((0, ln), (-b * 0.45, ln + capH * 0.05), (-b, ln - capH))),
        ("under_nx", line((-b, ln - capH), (-c, 0))),
    ]
    return segs, capH


def band_piece(parts, h, names):
    """A strip: its bottom edge in parts of the given lengths (each sewn to its own edge), its two ends sewn together."""
    xs = np.concatenate([[0], np.cumsum(parts)])
    segs = [(names[i], line((xs[i], 0), (xs[i + 1], 0))) for i in range(len(parts))]
    segs += [("end_r", line((xs[-1], 0), (xs[-1], h))), ("top", line((xs[-1], h), (0, h))), ("end_l", line((0, h), (0, 0)))]
    return segs


def leg_piece(front):
    hip = P["hipF"] if front else P["hipB"]
    ext = P["extF"] if front else P["extB"]
    rise = P["riseF"] if front else P["riseB"]
    hem = P["hemF"] if front else P["hemB"]
    kn = P["kneeF"] if front else P["kneeB"]
    ins, top = P["inseam"], P["inseam"] + rise
    side_top = P["inseam"] + P["riseF"]   # (the back rises toward the centre back; the side seams match)
    segs = [
        ("hem", line((0, 0), (hem, 0))),
        ("inseam", np.array([(hem, 0), (kn, P["knee"]), (hip + ext, ins)], float)),
        ("crotch", bez((hip + ext, ins), (hip + 0.5, ins + 1.0), (hip, ins + rise * 0.55))[:-1].tolist() + [(hip, top)]),
        ("waist", line((hip, top), (0, side_top))),
        ("side", line((0, side_top), (0, 0))),
    ]
    return [(n, np.array(s, float)) for n, s in segs]


# ---- Pieces and seams -------------------------------------------------------------------------------------------------
pieces = {}
pieces["shirt_front"] = dict(garment="shirt", segs=body_piece(True))
pieces["shirt_back"] = dict(garment="shirt", segs=body_piece(False))
arm_half = length(dict(pieces["shirt_front"]["segs"])["arm_px"])
sleeve_segs, capH = sleeve_piece(arm_half)
pieces["sleeve_R"] = dict(garment="shirt", segs=[(n, np.array(s, float)) for n, s in sleeve_segs])
pieces["sleeve_L"] = dict(garment="shirt", segs=[(n, np.array(s, float)) for n, s in sleeve_segs])
neckF = length(dict(pieces["shirt_front"]["segs"])["neck"])
neckB = length(dict(pieces["shirt_back"]["segs"])["neck"])
pieces["neck_band"] = dict(garment="shirt", segs=[(n, np.array(s, float)) for n, s in band_piece([neckF * 0.92, neckB * 0.92], P["band"], ["bot_f", "bot_b"])])
for side in "RL":
    pieces["leg_front_" + side] = dict(garment="trousers", segs=leg_piece(True))
    pieces["leg_back_" + side] = dict(garment="trousers", segs=leg_piece(False))
wb = P["waistband"]
wF, wB = wb * P["hipF"] / (2 * (P["hipF"] + P["hipB"])), wb * P["hipB"] / (2 * (P["hipF"] + P["hipB"]))
pieces["waistband"] = dict(garment="trousers", segs=[(n, np.array(s, float)) for n, s in band_piece([wF, wF, wB, wB], P["wbH"], ["w1", "w2", "w3", "w4"])])

# A seam: (piece, segment, reversed) twice; both run the same way along the seam.
seams = [
    # shirt: the front's +x side is his right (it faces out), the back's +x side his left.
    (("shirt_front", "shoulder_px", False), ("shirt_back", "shoulder_nx", True)),
    (("shirt_front", "shoulder_nx", True), ("shirt_back", "shoulder_px", False)),
    (("shirt_front", "side_px", False), ("shirt_back", "side_nx", True)),
    (("shirt_front", "side_nx", True), ("shirt_back", "side_px", False)),
    (("shirt_front", "arm_px", False), ("sleeve_R", "capF", False)),
    (("shirt_back", "arm_nx", True), ("sleeve_R", "capB", True)),
    (("shirt_front", "arm_nx", True), ("sleeve_L", "capF", False)),
    (("shirt_back", "arm_px", False), ("sleeve_L", "capB", True)),
    (("sleeve_R", "under_px", False), ("sleeve_R", "under_nx", True)),
    (("sleeve_L", "under_px", False), ("sleeve_L", "under_nx", True)),
    (("shirt_front", "neck", False), ("neck_band", "bot_f", False)),
    (("shirt_back", "neck", False), ("neck_band", "bot_b", False)),
    (("neck_band", "end_r", False), ("neck_band", "end_l", True)),
    # trousers
    (("leg_front_R", "side", False), ("leg_back_R", "side", False)),
    (("leg_front_L", "side", False), ("leg_back_L", "side", False)),
    (("leg_front_R", "inseam", False), ("leg_back_R", "inseam", False)),
    (("leg_front_L", "inseam", False), ("leg_back_L", "inseam", False)),
    (("leg_front_R", "crotch", False), ("leg_front_L", "crotch", False)),
    (("leg_back_R", "crotch", False), ("leg_back_L", "crotch", False)),
    (("leg_front_R", "waist", True), ("waistband", "w1", False)),
    (("leg_front_L", "waist", False), ("waistband", "w2", False)),
    (("leg_back_L", "waist", True), ("waistband", "w3", False)),
    (("leg_back_R", "waist", False), ("waistband", "w4", False)),
    (("waistband", "end_r", False), ("waistband", "end_l", True)),
]


# ---- Cutting: resample the outline, seams with matching counts; a lattice inside; Delaunay --------------------------
def resample(poly, n):
    d = np.concatenate([[0], np.cumsum(np.linalg.norm(np.diff(poly, axis=0), axis=1))])
    t = np.linspace(0, d[-1], n + 1)
    return np.stack([np.interp(t, d, poly[:, 0]), np.interp(t, d, poly[:, 1])], axis=1)


counts = {}
for (pa, sa, _), (pb, sb, _) in seams:
    la, lb = length(dict(pieces[pa]["segs"])[sa]), length(dict(pieces[pb]["segs"])[sb])
    n = max(2, int(round(max(la, lb) / H)))
    counts[(pa, sa)] = counts[(pb, sb)] = n


def inside(poly, pts):
    from matplotlib.path import Path
    return Path(poly).contains_points(pts)


def seg_dist(pts, poly):
    best = np.full(len(pts), 1e9)
    for a, b in zip(poly[:-1], poly[1:]):
        ab = b - a
        t = np.clip(((pts - a) @ ab) / max(ab @ ab, 1e-12), 0, 1)
        best = np.minimum(best, np.linalg.norm(pts - (a + t[:, None] * ab), axis=1))
    return best


for name, pc in pieces.items():
    verts, segidx = [], {}
    for sname, poly in pc["segs"]:
        n = counts.get((name, sname), max(1, int(round(length(poly) / H))))
        pts = resample(poly, n)
        idx = []
        for k, p in enumerate(pts):
            if k == 0 and verts and np.linalg.norm(np.array(verts[-1]) - p) < 1e-6:
                idx.append(len(verts) - 1)
                continue
            if k == len(pts) - 1 and np.linalg.norm(np.array(verts[0]) - p) < 1e-6 and len(segidx) == len(pc["segs"]) - 1:
                idx.append(0)
                continue
            verts.append(p.tolist())
            idx.append(len(verts) - 1)
        segidx[sname] = idx
    nb = len(verts)
    outline = np.array(verts)
    lo, hi = outline.min(0), outline.max(0)
    lat = []
    dy = H * math.sqrt(3) / 2
    for j, y in enumerate(np.arange(lo[1] + dy * 0.5, hi[1], dy)):
        for x in np.arange(lo[0] + (H / 2 if j % 2 else 0), hi[0], H):
            lat.append((x, y))
    lat = np.array(lat)
    closed = np.vstack([outline, outline[:1]])
    lat = lat[inside(outline, lat)]
    lat = lat[seg_dist(lat, closed) > 0.6 * H]
    allp = np.vstack([outline, lat])
    tri = Delaunay(allp).simplices
    c = allp[tri].mean(1)
    keep = inside(outline, c)
    for e in ((0, 1), (1, 2), (2, 0)):
        keep &= inside(outline, 0.5 * (allp[tri[:, e[0]]] + allp[tri[:, e[1]]])) | (np.minimum(tri[:, e[0]], tri[:, e[1]]) < nb) & (np.abs(tri[:, e[0]] - tri[:, e[1]]) % (nb - 1) <= 1)
    tri = tri[keep]
    # (Every triangle the same way round, counter-clockwise in the flat.)
    a, b, cc = allp[tri[:, 0]], allp[tri[:, 1]], allp[tri[:, 2]]
    flip = ((b - a)[:, 0] * (cc - a)[:, 1] - (b - a)[:, 1] * (cc - a)[:, 0]) < 0
    tri[flip] = tri[flip][:, [0, 2, 1]]
    # (The boundary must be edges of the cut: count those missing.)
    edges = set()
    for t in tri:
        for e in ((t[0], t[1]), (t[1], t[2]), (t[2], t[0])):
            edges.add((min(e), max(e)))
    missing = sum(1 for i in range(nb) if (min(i, (i + 1) % nb), max(i, (i + 1) % nb)) not in edges)
    pc.update(uv=allp, tris=tri, segidx=segidx, nb=nb, missing=missing)


# ---- Arrangement round the body, arms raised -------------------------------------------------------------------------
def trunk_at(y):
    ys = [t[0] for t in TRUNK]
    return float(np.interp(y, ys, [t[1] for t in TRUNK])), float(np.interp(y, ys, [t[2] for t in TRUNK]))


def ellipse_arc(a, b, n=720):
    phi = np.linspace(-math.pi, math.pi, n + 1)
    p = np.stack([a * np.cos(phi), b * np.sin(phi)], 1)
    s = np.concatenate([[0], np.cumsum(np.linalg.norm(np.diff(p, axis=0), axis=1))])
    return phi, s


def on_ellipse(a, b, phi0, arc, sign):
    """The point an arc length from angle phi0 along an ellipse (x, z), going toward +phi if sign > 0."""
    phi, s = ellipse_arc(a, b)
    s0 = np.interp(phi0, phi, s)
    sv = (s0 + sign * arc) % s[-1]
    ph = np.interp(sv, s, phi)
    return a * np.cos(ph), b * np.sin(ph)


def arm_axis(side):
    sh = SHOULDER * np.array([side, 1, 1])
    d0 = (WRIST - SHOULDER) * np.array([side, 1, 1])
    d0 /= np.linalg.norm(d0)
    up = math.radians(ARMS_UP)
    d1 = np.array([side * math.sin(up), -math.cos(up), d0[2]])
    return sh, d1 / np.linalg.norm(d1)


def place(name, pc):
    uv = pc["uv"] / 100.0
    out = np.zeros((len(uv), 3))
    W, L = P["W"] / 100, P["L"] / 100
    if name in ("shirt_front", "shirt_back"):
        hem_y = 1.555 - L
        for i, (x, y) in enumerate(uv):
            wy = hem_y + y
            tx, tz = trunk_at(min(wy, 1.50))
            a = tx + 0.045
            perim = 2 * W * 2
            b = max(tz + 0.045, 0.05)
            # (b so the ellipse is about as long round as the shirt.)
            for _ in range(30):
                pp = math.pi * (3 * (a + b) - math.sqrt((3 * a + b) * (a + 3 * b)))
                b *= (perim / pp) ** 0.5
            b = max(b, tz + 0.045)
            if name == "shirt_front":
                ex, ez = on_ellipse(a, b, math.pi / 2, x, -1)      # +x toward his right (phi 0)
            else:
                ex, ez = on_ellipse(a, b, -math.pi / 2, x, -1)     # +x toward his left (phi pi)
            out[i] = (ex, wy, ez)
    elif name.startswith("sleeve"):
        side = 1 if name.endswith("R") else -1
        sh, ax = arm_axis(side)
        # (Round the arm: the cap's top over the shoulder, the cap's front half toward his front, the underarm seam
        # under the arm.)
        out_dir = np.cross(ax, np.array([0, 0, 1.0])) * side
        out_dir = out_dir / np.linalg.norm(out_dir)
        if out_dir[1] < 0:
            out_dir = -out_dir
        fwd = np.array([0, 0, 1.0]) - ax * ax[2]
        fwd /= np.linalg.norm(fwd)
        ln = P["sleeve"] / 100
        cap_top = sh + ax * 0.02     # the cap's top at the shirt's shoulder point, 2 cm out from the joint
        for i, (x, y) in enumerate(uv):
            wcm = np.interp(y * 100, [0, P["sleeve"] - capH], [P["cuff"], P["bicep"]]) / 100
            r = max(wcm / (2 * math.pi), 0.035)   # (its own width round: a cuff placed wider got stuck on the fist)
            th = x / (wcm / (2 * math.pi)) if y * 100 <= P["sleeve"] - capH else x / (P["bicep"] / 100 / (2 * math.pi))
            c = cap_top + ax * (ln - y)
            out[i] = c + r * (math.cos(th) * out_dir + math.sin(th) * fwd)
    elif name == "neck_band":
        tot = uv[:, 0].max()
        for i, (x, y) in enumerate(uv):
            # (From his right neck point round the front to the left, then round the back.)
            ang = -2 * math.pi * x / tot
            out[i] = (0.072 * math.cos(ang), 1.545 + y, 0.072 * -math.sin(ang) * -1 if False else 0.072 * math.sin(-ang))
    elif name.startswith("leg"):
        side = 1 if name.endswith("R") else -1
        front = "front" in name
        hem_y = 0.03
        for i, (x, y) in enumerate(uv):
            wy = hem_y + y
            wf = np.interp(y * 100, [0, P["knee"], P["inseam"], P["inseam"] + P["riseF"]], [P["hemF"], P["kneeF"], P["hipF"] + P["extF"], P["hipF"]])
            wbk = np.interp(y * 100, [0, P["knee"], P["inseam"], P["inseam"] + P["riseB"]], [P["hemB"], P["kneeB"], P["hipB"] + P["extB"], P["hipB"]])
            r_pat = (wf + wbk) / 100 / (2 * math.pi)
            leg_r = 0.075 if wy > 0.45 else 0.055
            r = max(r_pat, leg_r + 0.035)
            if wy > 0.80:
                r = max(r, 0.115)
            th = (x / ((wf + wbk) / 100)) * 2 * math.pi   # 0 at the side seam, pi at the inseam
            cx = side * HIP[0] * (1 if wy < 0.80 else max(0.4, 1 - (wy - 0.80) * 2.5))
            ox = side * math.cos(th) * r
            oz = (1 if front else -1) * math.sin(th) * r
            out[i] = (cx + ox, wy, oz)
    elif name == "waistband":
        tot = uv[:, 0].max()
        for i, (x, y) in enumerate(uv):
            # (From his right side round the front, the left side, the back, home.)
            ang = 2 * math.pi * x / tot
            out[i] = (0.165 * math.cos(ang), 1.075 + y, 0.12 * math.sin(ang))
    out[:, 1] -= PELVIS_Y
    return out


for name, pc in pieces.items():
    pc["xyz"] = place(name, pc)

# ---- Write -----------------------------------------------------------------------------------------------------------
doc = {"body": "Medium", "cm": H, "armsUp": ARMS_UP, "params": P, "capH": capH, "pieces": [], "seams": []}
for name, pc in pieces.items():
    doc["pieces"].append({
        "name": name, "garment": pc["garment"],
        "uv": np.round(pc["uv"] / 100.0, 5).ravel().tolist(),
        "xyz": np.round(pc["xyz"], 5).ravel().tolist(),
        "tris": pc["tris"].ravel().tolist(),
        "segs": {k: v for k, v in pc["segidx"].items()},
    })
for (pa, sa, ra), (pb, sb, rb) in seams:
    a = pieces[pa]["segidx"][sa][::-1] if ra else pieces[pa]["segidx"][sa]
    b = pieces[pb]["segidx"][sb][::-1] if rb else pieces[pb]["segidx"][sb]
    assert len(a) == len(b), (pa, sa, pb, sb, len(a), len(b))
    doc["seams"].append({"a": pa, "b": pb, "ai": a, "bi": b, "name": f"{pa}.{sa}+{pb}.{sb}"})
out = sys.argv[1] if len(sys.argv) > 1 else "Assets/_Battler/Resources/Locomotion/GarmentPattern_Medium.json"
json.dump(doc, open(out, "w"), separators=(",", ":"))
tot = 0
for name, pc in pieces.items():
    tot += len(pc["uv"])
    print(f"{name:12s} {len(pc['uv']):5d} points {len(pc['tris']):5d} triangles; boundary edges missing {pc['missing']}")
print(f"{tot} points in all; sleeve cap {capH:.1f} cm (armhole half {arm_half:.1f} cm); {len(seams)} seams; {out} {os.path.getsize(out) // 1024} KB")
for s in doc["seams"]:
    la = length(pieces[s['a']]["uv"][s['ai']]); lb = length(pieces[s['b']]["uv"][s['bi']])
    print(f"  {s['name']:44s} {len(s['ai']):3d} pairs  {la:6.1f} / {lb:6.1f} cm")

if len(sys.argv) > 2:
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    fig, ax = plt.subplots(1, 2, figsize=(15, 8.5), gridspec_kw={"width_ratios": [1.3, 1]})
    lay = {"shirt_front": (0, 70, 0), "shirt_back": (70, 70, 0), "sleeve_R": (-5, -5, 0), "sleeve_L": (55, -5, 0), "neck_band": (105, 15, 0),
           "leg_front_R": (0, 0, 1), "leg_back_R": (40, 0, 1), "leg_front_L": (90, 0, 1), "leg_back_L": (130, 0, 1), "waistband": (40, -12, 1)}
    cols = plt.cm.tab20(np.linspace(0, 1, len(seams)))
    for name, pc in pieces.items():
        ox, oy, k = lay[name]
        a = ax[k]
        uv = pc["uv"] + np.array([ox, oy])
        if name.endswith("_L") and name.startswith("leg"):
            uv = np.array([ox, 0]) + (pc["uv"] * np.array([-1, 1])) + np.array([P["hipB"] + P["extB"], oy])
        a.triplot(uv[:, 0], uv[:, 1], pc["tris"], lw=0.25, color=(0.6, 0.65, 0.75))
        c = uv[pc["tris"]].mean(1).mean(0)
        a.text(uv[:, 0].mean(), uv[:, 1].mean(), name.replace("_", " "), ha="center", fontsize=9, weight="bold")
        pc["_draw"] = uv
    for k, s in enumerate(doc["seams"]):
        for side in ("a", "b"):
            pc = pieces[s[side]]
            uv = pc["_draw"][s[side + "i"]]
            ax[lay[s[side]][2]].plot(uv[:, 0], uv[:, 1], lw=2.2, color=cols[k])
    ax[0].set_title("Shirt: the cut (2 cm triangles); seams sewn together share a colour", fontsize=10)
    ax[1].set_title("Trousers", fontsize=10)
    for a in ax:
        a.set_aspect("equal"); a.axis("off")
    plt.tight_layout()
    plt.savefig(sys.argv[2], dpi=100)
    print(sys.argv[2])
