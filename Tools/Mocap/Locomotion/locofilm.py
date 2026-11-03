"""Draw the locomotion lab's film (locolab.json, written by `locolab`) as a GIF.

Left: the body as a stick figure from a camera that follows it (a three-quarter view from
the side it is facing, slowly turning with it), its planted feet marked. Right: the board
from above, with the path so far and every footprint left by a planted foot.

    python3 locofilm.py <dir>/locolab.json out.gif [--from 0] [--to 30] [--every 2] [--scale 1]
"""
import argparse
import json
import math

from PIL import Image, ImageDraw

W, H = 420, 420           # each panel
BOARD = 12.8              # metres, the board's width (8 tiles of 1.6 m)

BONES = [
    ("Pelvis", "Spine"), ("Spine", "Chest"), ("Chest", "Head"),
    ("Chest", "UpperArmL"), ("UpperArmL", "ForearmL"), ("ForearmL", "HandL"), ("HandL", "HandTipL"),
    ("Chest", "UpperArmR"), ("UpperArmR", "ForearmR"), ("ForearmR", "HandR"), ("HandR", "HandTipR"),
    ("Pelvis", "ThighL"), ("ThighL", "ShinL"), ("ShinL", "FootL"), ("FootL", "ToeL"),
    ("Pelvis", "ThighR"), ("ThighR", "ShinR"), ("ShinR", "FootR"), ("FootR", "ToeR"),
]
LEFT = (70, 130, 220)
RIGHT = (220, 90, 70)
MID = (60, 60, 60)


def side(a, b):
    name = a + b
    if name.endswith("L") or b.endswith("L"):
        return LEFT
    if name.endswith("R") or b.endswith("R"):
        return RIGHT
    return MID


def project(p, centre, yaw, pitch, scale):
    """A point (world, metres) onto the follow panel: the camera looks along `yaw` (degrees), tilted down by `pitch`."""
    x, y, z = p[0] - centre[0], p[1], p[2] - centre[2]
    r = math.radians(-yaw)
    c, s = math.cos(r), math.sin(r)
    lx, lz = c * x + s * z, -s * x + c * z
    t = math.radians(pitch)
    sy = y * math.cos(t) + lz * math.sin(t)
    return W / 2 + lx * scale, H * 0.78 - sy * scale


def board_xy(p, size):
    return (p[0] / BOARD + 0.5) * size, (0.5 - p[2] / BOARD) * size


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("film")
    ap.add_argument("out")
    ap.add_argument("--from", dest="t0", type=float, default=0.0)
    ap.add_argument("--to", dest="t1", type=float, default=1e9)
    ap.add_argument("--every", type=int, default=2, help="draw every Nth film frame")
    ap.add_argument("--scale", type=float, default=1.0)
    ap.add_argument("--view", default="follow", choices=["follow", "side", "front", "left"], help="the camera: following from the right-front, or square on from the right side, the front or the left side")
    ap.add_argument("--shield", type=float, default=1.0, help="the shield's size (1: a Medium's)")
    a = ap.parse_args()

    film = json.load(open(a.film))
    frames = [f for f in film["frames"] if a.t0 <= f["t"] <= a.t1]
    dt = film.get("dt", 0.04)
    scale = 150 * a.scale

    prints = []        # (x, z, foot) of every plant
    path = []
    was = [0, 0]
    cam_yaw = None
    images = []
    for i, f in enumerate(frames):
        pose = f["pose"]
        root = f["root"]
        path.append((root[0], root[2]))
        for k, toe in ((0, "ToeL"), (1, "ToeR")):
            if f["locks"][k] and not was[k]:
                foot = pose["FootL" if k == 0 else "FootR"]
                prints.append((foot, pose[toe], k))
            was[k] = f["locks"][k]

        # (The camera: from the body's right-front, turning slowly after it.)
        want = f["facing"] + {"follow": -60.0, "side": -90.0, "front": 180.0, "left": 90.0}[a.view]
        cam_yaw = want if cam_yaw is None or a.view != "follow" else cam_yaw + ((want - cam_yaw + 180) % 360 - 180) * 0.03
        if i % a.every:
            continue

        img = Image.new("RGB", (2 * W, H), (246, 244, 238))
        d = ImageDraw.Draw(img)

        # Follow panel: a ground grid of tiles, the footprints near, the body.
        centre = root
        tile = 1.6
        gx0 = math.floor((centre[0] - 4) / tile) * tile
        gz0 = math.floor((centre[2] - 4) / tile) * tile
        for n in range(8):
            for line in (((gx0 + n * tile, gz0), (gx0 + n * tile, gz0 + 8 * tile)),
                         ((gx0, gz0 + n * tile), (gx0 + 8 * tile, gz0 + n * tile))):
                p0 = project((line[0][0], 0, line[0][1]), centre, cam_yaw, 25, scale)
                p1 = project((line[1][0], 0, line[1][1]), centre, cam_yaw, 25, scale)
                d.line([p0, p1], fill=(225, 222, 212), width=1)
        for heel, toe, k in prints[-12:]:
            p0 = project((heel[0], 0, heel[2]), centre, cam_yaw, 25, scale)
            p1 = project((toe[0], 0, toe[2]), centre, cam_yaw, 25, scale)
            d.line([p0, p1], fill=(180, 200, 235) if k == 0 else (235, 190, 180), width=5)

        for a_, b_ in BONES:
            if a_ in pose and b_ in pose:
                d.line([project(pose[a_], centre, cam_yaw, 25, scale), project(pose[b_], centre, cam_yaw, 25, scale)],
                       fill=side(a_, b_), width=4)
        if "BladeTipR" in pose:
            d.line([project(pose["HandTipR"], centre, cam_yaw, 25, scale), project(pose["BladeTipR"], centre, cam_yaw, 25, scale)],
                   fill=(120, 120, 135), width=3)
        if "ShieldC" in pose:
            c, n = pose["ShieldC"], pose["ShieldN"]
            u = [n[1], -n[0], 0.0] if abs(n[2]) > 0.9 else [-n[2], 0.0, n[0]]
            ul = math.sqrt(sum(x * x for x in u)) or 1.0
            u = [x / ul for x in u]
            v = [n[1] * u[2] - n[2] * u[1], n[2] * u[0] - n[0] * u[2], n[0] * u[1] - n[1] * u[0]]
            rad = 0.3 * a.shield
            ring = [project([c[i] + rad * (math.cos(k / 12 * 2 * math.pi) * u[i] + math.sin(k / 12 * 2 * math.pi) * v[i]) for i in range(3)],
                            centre, cam_yaw, 25, scale) for k in range(13)]
            d.polygon(ring, outline=(110, 80, 50), fill=(170, 130, 90))
        hx, hy = project([pose["Head"][0], pose["Head"][1] + 0.12, pose["Head"][2]], centre, cam_yaw, 25, scale)
        d.ellipse([hx - 13, hy - 13, hx + 13, hy + 13], outline=MID, width=3)
        for k, name in ((0, "FootL"), (1, "FootR")):
            if f["locks"][k]:
                x, y = project(pose[name], centre, cam_yaw, 25, scale)
                d.ellipse([x - 5, y - 5, x + 5, y + 5], fill=LEFT if k == 0 else RIGHT)

        # Board panel.
        ox = W
        size = H
        for n in range(9):
            v = n * size / 8
            d.line([(ox + v, 0), (ox + v, size)], fill=(225, 222, 212))
            d.line([(ox, v), (ox + size, v)], fill=(225, 222, 212))
        for heel, toe, k in prints:
            x0, y0 = board_xy(heel, size)
            x1, y1 = board_xy(toe, size)
            d.line([(ox + x0, y0), (ox + x1, y1)], fill=LEFT if k == 0 else RIGHT, width=3)
        if len(path) > 1:
            d.line([(ox + board_xy((x, 0, z), size)[0], board_xy((x, 0, z), size)[1]) for x, z in path[::3]],
                   fill=(150, 150, 150), width=1)
        bx, by = board_xy(root, size)
        r = math.radians(f["facing"])
        d.ellipse([ox + bx - 5, by - 5, ox + bx + 5, by + 5], fill=MID)
        d.line([(ox + bx, by), (ox + bx + 14 * math.sin(r), by - 14 * math.cos(r))], fill=MID, width=2)

        d.text((10, 8), f'{f["t"]:6.2f} s  {f["leg"]}', fill=(30, 30, 30))
        d.text((10, 22), f'{f["mode"]}  {f["speed"]:.2f} m/s  run {f["run"]:.2f}  phase {f["phase"]:.2f}', fill=(90, 90, 90))
        images.append(img)

    if not images:
        raise SystemExit("no frames in that time")
    images[0].save(a.out, save_all=True, append_images=images[1:], duration=int(1000 * dt * a.every), loop=0, optimize=True)
    print(f"{a.out}: {len(images)} frames")


if __name__ == "__main__":
    main()
