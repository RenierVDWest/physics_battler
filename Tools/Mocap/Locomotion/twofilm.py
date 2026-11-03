"""Draw two lab films together (a striker and the dummy, from `dummyhits`) as a contact sheet, square on from the side.

    python3 twofilm.py striker.json dummy.json out.png [--times 0.2,0.4,...]
"""
import argparse
import json
import math

from PIL import Image, ImageDraw

BONES = [
    ("Pelvis", "Spine"), ("Spine", "Chest"), ("Chest", "Head"),
    ("Chest", "UpperArmL"), ("UpperArmL", "ForearmL"), ("ForearmL", "HandL"),
    ("Chest", "UpperArmR"), ("UpperArmR", "ForearmR"), ("ForearmR", "HandR"),
    ("Pelvis", "ThighL"), ("ThighL", "ShinL"), ("ShinL", "FootL"), ("FootL", "ToeL"),
    ("Pelvis", "ThighR"), ("ThighR", "ShinR"), ("ShinR", "FootR"), ("FootR", "ToeR"),
]
W, H = 360, 330


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("a")
    ap.add_argument("b")
    ap.add_argument("out")
    ap.add_argument("--times", default="")
    ap.add_argument("--yaw", type=float, default=-90.0, help="the camera's way (degrees; -90: from the striker's right)")
    a = ap.parse_args()
    fa, fb = json.load(open(a.a))["frames"], json.load(open(a.b))["frames"]
    times = [float(t) for t in a.times.split(",")] if a.times else [fa[int(i * (len(fa) - 1) / 7)]["t"] for i in range(8)]
    sheet = Image.new("RGB", (W * 4, H * ((len(times) + 3) // 4)), (246, 244, 238))
    r = math.radians(-a.yaw)
    c, s = math.cos(r), math.sin(r)

    def P(p, mid):
        x, z = p[0] - mid[0], p[2] - mid[2]
        lx = c * x + s * z
        return W / 2 + lx * 110, H * 0.88 - p[1] * 110

    for n, t in enumerate(times):
        x = min(fa, key=lambda f: abs(f["t"] - t))
        y = min(fb, key=lambda f: abs(f["t"] - t))
        img = Image.new("RGB", (W, H), (246, 244, 238))
        d = ImageDraw.Draw(img)
        mid = [(x["root"][i] + y["root"][i]) / 2 for i in range(3)]
        d.line([(0, H * 0.88), (W, H * 0.88)], fill=(210, 205, 195))
        for f, col in ((y, (120, 120, 120)), (x, (200, 80, 60))):
            p = f["pose"]
            for u, v in BONES:
                if u in p and v in p:
                    d.line([P(p[u], mid), P(p[v], mid)], fill=col, width=3)
            hx, hy = P([p["Head"][0], p["Head"][1] + 0.1, p["Head"][2]], mid)
            d.ellipse([hx - 9, hy - 9, hx + 9, hy + 9], outline=col, width=2)
            if "BladeTipR" in p:
                d.line([P(p["HandTipR"], mid), P(p["BladeTipR"], mid)], fill=(60, 60, 90), width=3)
            if "ShieldC" in p:
                sc, sn = p["ShieldC"], p["ShieldN"]
                u = [-sn[2], 0.0, sn[0]]
                ul = math.sqrt(u[0] ** 2 + u[2] ** 2) or 1.0
                u = [k / ul for k in u]
                v = [sn[1] * u[2] - sn[2] * u[1], sn[2] * u[0] - sn[0] * u[2], sn[0] * u[1] - sn[1] * u[0]]
                ring = [P([sc[i] + 0.3 * (math.cos(k / 12 * 6.283) * u[i] + math.sin(k / 12 * 6.283) * v[i]) for i in range(3)], mid) for k in range(13)]
                d.line(ring, fill=(140, 100, 60), width=2)
        d.text((6, 6), f'{x["t"]:.2f} s  {x["mode"]} {x["shot"]}', fill=(30, 30, 30))
        sheet.paste(img, ((n % 4) * W, (n // 4) * H))
    sheet.save(a.out)


if __name__ == "__main__":
    main()
