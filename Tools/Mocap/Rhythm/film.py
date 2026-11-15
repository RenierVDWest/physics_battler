#!/usr/bin/env python3
"""Film strips of the fight's bodies as posed (the plan "Fight Rhythm from Recordings"): what a fighter does in a
stretch of a headless fight, drawn so it can be looked at.

The fight runner writes the posed body of the fighters named, each step, from one second to another:

    FIGHT_FILM="film.json,3.0,4.2,swordsman,target" dotnet Tools/CompileCheck/bin/Tests/Debug/PhysicsBattler.Tests.dll fight ...

and this draws them as a strip of stick figures at even moments: from the side (across the line between the first
frame's two fighters, or across the first fighter's facing) and from above. The first fighter named is drawn in
colour (blue its left side, red its right), the others grey. A foot held down has a bar under it; the sword is
the dark line. The first fighter's start is kept at the same place in every cell, so its travel shows.

    python3 film.py film.json out.png [--moments 10] [--who swordsman] [--from 3.0 --to 4.2] [--title "..."]
"""
import argparse
import json
import math

from PIL import Image, ImageDraw

PARTS = "Pelvis Spine Chest Head UpperArmL ForearmL HandL UpperArmR ForearmR HandR ThighL ShinL FootL ThighR ShinR FootR HeadTop HandLEnd HandREnd ToeL ToeR".split()
IX = {n: i for i, n in enumerate(PARTS)}
BONES = [("Pelvis", "Spine"), ("Spine", "Chest"), ("Chest", "Head"), ("Head", "HeadTop"),
         ("Chest", "UpperArmL"), ("UpperArmL", "ForearmL"), ("ForearmL", "HandL"), ("HandL", "HandLEnd"),
         ("Chest", "UpperArmR"), ("UpperArmR", "ForearmR"), ("ForearmR", "HandR"), ("HandR", "HandREnd"),
         ("Pelvis", "ThighL"), ("ThighL", "ShinL"), ("ShinL", "FootL"), ("FootL", "ToeL"),
         ("Pelvis", "ThighR"), ("ThighR", "ShinR"), ("ShinR", "FootR"), ("FootR", "ToeR")]
LEFT, RIGHT, MID, SWORD, GREY = (60, 120, 220), (215, 80, 60), (70, 70, 70), (20, 20, 20), (175, 175, 170)
CELL_W, SIDE_H, TOP_H = 230, 250, 230
SCALE = 100.0     # pixels a metre


def pts(frame):
    p = frame["p"]
    return [(p[3 * i], p[3 * i + 1], p[3 * i + 2]) for i in range(len(PARTS))]


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("film")
    ap.add_argument("out")
    ap.add_argument("--moments", type=int, default=10)
    ap.add_argument("--who", default="")
    ap.add_argument("--from", dest="t0", type=float, default=None)
    ap.add_argument("--to", dest="t1", type=float, default=None)
    ap.add_argument("--title", default="")
    args = ap.parse_args()
    frames = json.load(open(args.film))
    names = []
    for f in frames:
        if f["u"] not in names:
            names.append(f["u"])
    who = next((n for n in names if args.who.lower() in n.lower()), names[0])
    mine = [f for f in frames if f["u"] == who and (args.t0 is None or f["t"] >= args.t0) and (args.t1 is None or f["t"] <= args.t1)]
    others = {n: {round(f["t"], 3): f for f in frames if f["u"] == n} for n in names if n != who}
    n = len(mine)
    picks = sorted(set(round(i * (n - 1) / max(1, args.moments - 1)) for i in range(args.moments)))
    first = mine[0]
    ground = min(min(q[1] for q in pts(f)) for f in mine)
    x0, z0 = first["root"][0], first["root"][1]
    # Forward: toward the other fighter as the film begins, or the way it faces.
    fwd = None
    for o in others.values():
        g = o.get(round(first["t"], 3))
        if g:
            dx, dz = g["root"][0] - x0, g["root"][1] - z0
            d = math.hypot(dx, dz)
            if d > 0.2:
                fwd = (dx / d, dz / d)
    if fwd is None:
        a = math.radians(first["root"][2])
        fwd = (math.sin(a), math.cos(a))
    right = (fwd[1], -fwd[0])

    def own(q):
        dx, dz = q[0] - x0, q[2] - z0
        return dx * right[0] + dz * right[1], q[1] - ground, dx * fwd[0] + dz * fwd[1]

    img = Image.new("RGB", (CELL_W * len(picks), 34 + SIDE_H + TOP_H), (250, 250, 247))
    d = ImageDraw.Draw(img)
    d.text((8, 4), args.title or f"{who}: {mine[0]['t']:.2f} to {mine[-1]['t']:.2f} s", fill=(0, 0, 0))
    for k, i in enumerate(picks):
        f = mine[i]
        ox = k * CELL_W

        def side(q):
            return ox + 50 + q[2] * SCALE * 0.75, 34 + SIDE_H - 14 - q[1] * SCALE

        def top(q):
            return ox + CELL_W / 2 + q[0] * SCALE * 0.75, 34 + SIDE_H + TOP_H - 30 - q[2] * SCALE * 0.75

        d.text((ox + 8, 19), f"{f['t']:.2f} s  {f['doing'][:26]}", fill=(90, 90, 90))
        d.line([(ox, 34 + SIDE_H - 14), (ox + CELL_W, 34 + SIDE_H - 14)], fill=(200, 200, 195))
        d.line([(ox, 34), (ox, 34 + SIDE_H + TOP_H)], fill=(225, 225, 220))
        figures = [(o.get(round(f["t"], 3)), False) for o in others.values()] + [(f, True)]
        for g, colour in figures:
            if g is None:
                continue
            p = [own(q) for q in pts(g)]
            for view in (side, top):
                for a, b in sorted(BONES, key=lambda ab: 0 if ab[1].endswith("L") or "L" in ab[1][-4:] and "R" not in ab[1][-4:] else 1):
                    left = b.endswith("L") or b in ("HandLEnd",)
                    rightside = b.endswith("R") or b in ("HandREnd",)
                    c = GREY if not colour else LEFT if left else RIGHT if rightside else MID
                    d.line([view(p[IX[a]]), view(p[IX[b]])], fill=c, width=3 if colour else 2)
                hx, hy = view(p[IX["Head"]])
                d.ellipse([hx - 6, hy - 6, hx + 6, hy + 6], outline=MID if colour else GREY, width=2)
                s = g["s"]
                d.line([view(own(s[0:3])), view(own(s[3:6]))], fill=SWORD if colour else GREY, width=3 if colour else 2)
            if colour:
                for j, foot in enumerate(("FootL", "FootR")):
                    if g["d"][j]:
                        fx, _ = side(p[IX[foot]])
                        d.line([(fx - 9, 34 + SIDE_H - 9 + 4 * j), (fx + 9, 34 + SIDE_H - 9 + 4 * j)], fill=LEFT if j == 0 else RIGHT, width=3)
    img.save(args.out)
    print(args.out)


if __name__ == "__main__":
    main()
