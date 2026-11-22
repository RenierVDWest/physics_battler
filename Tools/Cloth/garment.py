#!/usr/bin/env python3
"""garment.py [Garments.json] [out.png] [zoom]: the draped garments (drape.py) on the drawn body in its rest pose,
from his front, three-quarters, side, back and three-quarters behind, drawn with true depth (zbuf.py). zoom: "neck",
"waist", "arm" or "hem" for a close look. Also used by drape.py to draw a moment of the drape (DRAPE_SNAP).
Run from the repository root with python3 -I."""
import json, math, os, sys
import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from fbxread import read
from zbuf import zrender

SIZE = "Medium"
PELVIS_Y = 0.985
COLOURS = {"shirt": (0.62, 0.66, 0.78), "trousers": (0.24, 0.27, 0.40), "sash": (0.12, 0.10, 0.09), "skin": (0.93, 0.74, 0.56)}
VIEWS = (("front", 0), ("three-quarters", 40), ("side", 90), ("back", 180), ("behind, left", 230))
ZOOM = {"neck": (0.0, 1.45, 0.22), "waist": (0.0, 1.05, 0.30), "arm": (0.25, 1.25, 0.30), "hem": (0.0, 0.25, 0.35),
        "armpit": (0.2, 1.32, 0.16)}

_rig = json.load(open(f"Assets/_Battler/Data/Rigs/{SIZE.lower()}_rig.json"))
_nulls = {n["name"]: n for n in _rig["nulls"]}


def world(name):
    p = np.zeros(3)
    n = _nulls[name]
    while n:
        p += np.array(n["localPosition"])
        n = _nulls.get(n["parent"]) if n["parent"] else None
    return p


_body = None


def body():
    global _body
    if _body is None:
        _body = []
        for part in _rig["parts"]:
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
            base = part["name"].split("_")[0]
            side = 0.0
            if base in ("UpperArm", "Forearm", "Hand"):
                side = 1.0 if part["name"].endswith("_R") else -1.0
            _body.append((v + world(part["name"]), np.array(tris), side))
    return _body


def body_meshes(arm_raise=0.0):
    """The body's parts as (points, triangles), the arms raised by arm_raise radians out to the side."""
    sh = world("UpperArm_R")
    out = []
    for v, t, side in body():
        w = v.copy()
        if side != 0.0 and arm_raise != 0.0:
            a = side * arm_raise
            dx = w[:, 0] - side * sh[0]
            dy = w[:, 1] - sh[1]
            w[:, 0] = side * sh[0] + dx * math.cos(a) - dy * math.sin(a)
            w[:, 1] = sh[1] + dx * math.sin(a) + dy * math.cos(a)
        out.append((w, t))
    return out


def body_tris(arm_raise=0.0):
    """The body's triangles (n, 3, 3), the arms raised by arm_raise radians out to the side."""
    sh = world("UpperArm_R")
    out = []
    for v, t, side in body():
        w = v.copy()
        if side != 0.0 and arm_raise != 0.0:
            a = side * arm_raise
            dx = w[:, 0] - side * sh[0]
            dy = w[:, 1] - sh[1]
            w[:, 0] = side * sh[0] + dx * math.cos(a) - dy * math.sin(a)
            w[:, 1] = sh[1] + dx * math.sin(a) + dy * math.cos(a)
        out.append(w[t])
    return np.concatenate(out)


def draw(out, layers, arm_raise=0.0, title="", zoom=None, sash=None, px=420, views=VIEWS):
    """layers: [(points (n, 3) world, triangles (m, 3), kind)]. Shaded smoothly: each corner takes the mean light of
    the faces round its point (lit from both sides, so a face's winding does not matter)."""
    import matplotlib
    matplotlib.use("Agg")
    import matplotlib.pyplot as plt
    meshes = [(w, t, "skin") for w, t in body_meshes(arm_raise)] + [(x, t, k) for x, t, k in layers]
    if sash is not None:
        lo, hi, a, b, cz = sash
        a += 0.006
        b += 0.006
        k = 72
        ang = 2 * math.pi * np.arange(k) / k
        pts = np.vstack([np.stack([a * np.cos(ang), np.full(k, y), cz + b * np.sin(ang)], 1) for y in (lo, hi)])
        tri = []
        for j in range(k):
            j1 = (j + 1) % k
            tri += [(j, j1, k + j1), (j, k + j1, k + j)]
        meshes.append((pts, np.array(tri), "sash"))
    light = np.array([0.35, -0.6, 0.72])
    light /= np.linalg.norm(light)
    fig = plt.figure(figsize=(3.3 * len(views) * px / 420, (5.0 if not zoom else 3.6) * px / 420))
    for vi, (label, turn) in enumerate(views):
        a = math.radians(turn)
        right = np.array([math.cos(a), 0, -math.sin(a)])
        ahead = np.array([math.sin(a), 0, math.cos(a)])
        if zoom:
            cx, cy, span = ZOOM[zoom]
            centre = np.array([cx, cy, 0.0])
        else:
            centre = np.array([0.0, 0.95, 0.0])
        tris, cols = [], []
        for x, t, kind in meshes:
            d = x - centre
            V = np.stack([-(d @ right), -(d @ ahead), d[:, 1]], axis=-1)
            T = V[t]
            n = np.cross(T[:, 1] - T[:, 0], T[:, 2] - T[:, 0])
            n /= np.linalg.norm(n, axis=1, keepdims=True) + 1e-12
            fs = np.abs(n @ light)
            vs = np.zeros(len(x))
            cnt = np.zeros(len(x))
            for c in range(3):
                np.add.at(vs, t[:, c], fs)
                np.add.at(cnt, t[:, c], 1)
            vs /= np.maximum(cnt, 1)
            shade = 0.38 + 0.62 * vs[t]
            tris.append(T)
            cols.append(np.array(COLOURS[kind])[None, None, :] * shade[:, :, None])
        tris = np.concatenate(tris)
        cols = np.concatenate(cols)
        if zoom:
            img = zrender(tris, cols, -span, span, -span, span, px=px)
        else:
            img = zrender(tris, cols, -0.62, 0.62, -0.98, 0.95, px=px)
        ax = fig.add_subplot(1, len(views), vi + 1)
        ax.imshow(img, interpolation="bilinear")
        ax.set_axis_off()
        ax.set_title(label, fontsize=10)
    if title:
        fig.suptitle(title, fontsize=11)
    plt.tight_layout()
    plt.savefig(out, dpi=80)
    plt.close(fig)
    return out


if __name__ == "__main__":
    src = sys.argv[1] if len(sys.argv) > 1 else f"Assets/_Battler/Resources/Locomotion/Garments_{SIZE}.json"
    out = sys.argv[2] if len(sys.argv) > 2 else "/tmp/garments.png"
    zoom = sys.argv[3] if len(sys.argv) > 3 else None
    doc = json.load(open(src))
    layers = []
    for g in doc["garments"]:
        if g["name"] in os.environ.get("HIDE", "").split(","):
            continue
        x = np.array(g["x"]).reshape(-1, 3) + [0, doc["pelvisY"], 0]
        layers.append((x, np.array(g["tris"]).reshape(-1, 3), g["name"]))
    s = doc.get("sash")
    sash = (s["lo"] + doc["pelvisY"], s["hi"] + doc["pelvisY"], s["a"], s["b"], s["cz"]) if s and "sash" not in os.environ.get("HIDE", "") else None
    print(draw(out, layers, 0.0, os.environ.get("TITLE", ""), zoom, sash, px=int(os.environ.get("PX", "420"))))
