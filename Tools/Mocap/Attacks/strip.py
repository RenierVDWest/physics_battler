"""Film strips of made attacks (the plan "Aimed attacks"): what the attack maker makes, drawn so it can be looked at.

The fight runner writes the made attacks as points:

    ATTACK_DUMP=made.json dotnet Tools/CompileCheck/bin/Tests/Debug/PhysicsBattler.Tests.dll attacks

and this draws each as a strip of stick figures at its key moments (the start, the wind-up, the drive, the landing,
carried through, the end): from its right side (it faces right; forward is to the right of the picture) and from
above (forward is up). The sword is the dark line, the target the red dot, a foot on the ground has a bar under it.
Blue is its left side, red its right.

    python3 strip.py made.json out_dir [--only forehand] [--moments 9]
    python3 strip.py made.json out_dir --feet     the feet's slide while they are down, for each attack (cm)
"""
import argparse
import json
import math
import os

from PIL import Image, ImageDraw

NAMES = ("Hips Spine Spine1 Spine2 Neck Head HeadTop LeftEye RightEye "
         "LeftShoulder LeftArm LeftForeArm LeftHand LeftHandIndex LeftHandMiddle LeftHandPinky "
         "RightShoulder RightArm RightForeArm RightHand RightHandIndex RightHandMiddle RightHandPinky "
         "LeftUpLeg LeftLeg LeftFoot LeftToeBase LeftToeEnd "
         "RightUpLeg RightLeg RightFoot RightToeBase RightToeEnd").split()
IX = {n: i for i, n in enumerate(NAMES)}
BONES = [("Hips", "Spine"), ("Spine", "Spine1"), ("Spine1", "Spine2"), ("Spine2", "Neck"), ("Neck", "Head"), ("Head", "HeadTop"),
         ("Spine2", "LeftShoulder"), ("LeftShoulder", "LeftArm"), ("LeftArm", "LeftForeArm"), ("LeftForeArm", "LeftHand"), ("LeftHand", "LeftHandMiddle"),
         ("Spine2", "RightShoulder"), ("RightShoulder", "RightArm"), ("RightArm", "RightForeArm"), ("RightForeArm", "RightHand"), ("RightHand", "RightHandMiddle"),
         ("Hips", "LeftUpLeg"), ("LeftUpLeg", "LeftLeg"), ("LeftLeg", "LeftFoot"), ("LeftFoot", "LeftToeBase"), ("LeftToeBase", "LeftToeEnd"),
         ("Hips", "RightUpLeg"), ("RightUpLeg", "RightLeg"), ("RightLeg", "RightFoot"), ("RightFoot", "RightToeBase"), ("RightToeBase", "RightToeEnd")]
LEFT, RIGHT, MID, SWORD, TARGET = (60, 120, 220), (215, 80, 60), (70, 70, 70), (20, 20, 20), (230, 30, 30)
CELL_W, SIDE_H, TOP_H = 250, 250, 240
SCALE = 95.0     # pixels a metre


def colour(a, b):
    return LEFT if "Left" in b else RIGHT if "Right" in b else MID


def points(frame):
    p = frame["p"]
    return [(p[3 * i], p[3 * i + 1], p[3 * i + 2]) for i in range(len(NAMES))]


def moments(clip, count):
    """The frames to draw: evenly through the wind-up, the landing itself, evenly after it."""
    n, c = len(clip["frames"]), clip["contact"]
    before = max(2, count // 2)
    after = max(2, count - before - 1)
    picks = [round(c * i / before) for i in range(before)] + [c] + [round(c + (n - 1 - c) * (i + 1) / after) for i in range(after)]
    return sorted(set(min(n - 1, max(0, f)) for f in picks))


def draw(clip, count):
    frames = clip["frames"]
    picks = moments(clip, count)
    w = CELL_W * len(picks)
    img = Image.new("RGB", (w, 34 + SIDE_H + TOP_H), (250, 250, 247))
    d = ImageDraw.Draw(img)
    d.text((8, 4), clip["name"] + ("   |   " + clip["fit"] if clip.get("aimed") else ""), fill=(0, 0, 0))
    start = points(frames[0])
    x0, z0 = start[IX["Hips"]][0], start[IX["Hips"]][2]
    for k, f in enumerate(picks):
        p = points(frames[f])
        ox = k * CELL_W
        label = f"{f / clip['rate']:.2f} s" + ("  LANDS" if f == clip["contact"] else "")
        d.text((ox + 8, 19), label, fill=(170, 30, 30) if f == clip["contact"] else (90, 90, 90))
        # From its right side: forward (z) to the right, up (y) up. The body's start is kept at the same place in every cell,
        # so its travel shows.
        def side(q):
            return ox + 60 + (q[2] - z0) * SCALE, 34 + SIDE_H - 14 - q[1] * SCALE

        def top(q):
            return ox + CELL_W / 2 + (q[0] - x0) * SCALE, 34 + SIDE_H + TOP_H - 40 - (q[2] - z0) * SCALE

        d.line([(ox, 34 + SIDE_H - 14), (ox + CELL_W, 34 + SIDE_H - 14)], fill=(200, 200, 195))
        d.line([(ox, 34), (ox, 34 + SIDE_H + TOP_H)], fill=(225, 225, 220))
        for view in (side, top):
            # (The far side first, so the near side is drawn over it.)
            for a, b in sorted(BONES, key=lambda ab: 0 if "Left" in ab[1] else 2 if "Right" in ab[1] else 1):
                d.line([view(p[IX[a]]), view(p[IX[b]])], fill=colour(a, b), width=3)
            hx, hy = view(p[IX["Head"]])
            d.ellipse([hx - 7, hy - 7, hx + 7, hy + 7], outline=MID, width=2)
            s = frames[f]["s"]
            d.line([view(s[0:3]), view(s[3:6])], fill=SWORD, width=3)
            if clip.get("aimed"):
                tx, ty = view(clip["target"])
                d.ellipse([tx - 4, ty - 4, tx + 4, ty + 4], fill=TARGET)
        for i, foot in enumerate(("LeftToeBase", "RightToeBase")):
            if frames[f]["d"][i] & 4:
                fx, fy = side(p[IX[foot]])
                d.line([(fx - 9, 34 + SIDE_H - 10), (fx + 9, 34 + SIDE_H - 10)], fill=LEFT if i == 0 else RIGHT, width=3)
    return img


def feet(clip):
    """How far each foot slides over the ground while it is down (cm, the most between one frame and the next summed)."""
    out = []
    for i, ankle in enumerate(("LeftFoot", "RightFoot")):
        slid, down = 0.0, 0
        last = None
        for fr in clip["frames"]:
            p = points(fr)[IX[ankle]]
            if fr["d"][i] & 4:
                down += 1
                if last is not None:
                    slid += math.hypot(p[0] - last[0], p[2] - last[2])
                last = p
            else:
                last = None
        out.append((100 * slid, down / clip["rate"]))
    return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("made")
    ap.add_argument("out")
    ap.add_argument("--only", default="")
    ap.add_argument("--moments", type=int, default=9)
    ap.add_argument("--feet", action="store_true")
    args = ap.parse_args()
    clips = [c for c in json.load(open(args.made)) if args.only in c["name"]]
    if args.feet:
        for c in clips:
            (l, lt), (r, rt) = feet(c)
            print(f"{c['name']:62} left foot slides {l:5.1f} cm in {lt:.2f} s down, right {r:5.1f} cm in {rt:.2f} s")
        return
    os.makedirs(args.out, exist_ok=True)
    for i, c in enumerate(clips):
        name = f"{i:02d}_" + "".join(ch if ch.isalnum() else "_" for ch in c["name"])[:60] + ".png"
        draw(c, args.moments).save(os.path.join(args.out, name))
        print(name)


if __name__ == "__main__":
    main()
