#!/usr/bin/env python3
"""Film sheet of the strikes' combo cores (the `cores <dir>` runner's cores.json).

One strip per strike: the body seen from where its target stands and from above, at the core's entry (blue),
its contact (red) and its exit (grey); the blade tip's path over the clip's window (thin) and over the core
(thick); and the tip's speed with the turnaround, entry, contact and exit marked.

    python3 Tools/Mocap/Combat/coresheet.py <dir>/cores.json coresheet.png
"""
import json
import math
import sys

from PIL import Image, ImageDraw, ImageFont

PW, PH = 300, 300          # a view panel
GW = 420                   # the speed graph
SCALE = 95                 # pixels a leg length

BONES = [(0, 1), (1, 2), (2, 3), (3, 4), (4, 5), (5, 6),
         (3, 9), (9, 10), (10, 11), (11, 12),
         (3, 16), (16, 17), (17, 18), (18, 19),
         (0, 23), (23, 24), (24, 25), (25, 26), (26, 27),
         (0, 28), (28, 29), (29, 30), (30, 31), (31, 32)]
ENTRY, CONTACT, EXIT, TURN = (50, 110, 220), (210, 60, 50), (130, 130, 130), (200, 150, 40)
INK, FAINT, BG = (40, 40, 40), (200, 200, 200), (250, 249, 246)


def frame_at(clip, t):
    return min(clip["frames"], key=lambda f: abs(f["t"] - t))


def viewer(clip, view):
    c = frame_at(clip, clip["contact"])
    hips = c["p"][0]
    # (Forward from the hips' right, the left thigh to the right one.)
    fx, fz = -c["p"][23][2] + c["p"][28][2], c["p"][28][0] - c["p"][23][0]
    n = math.hypot(fx, fz) or 1
    fx, fz = fx / n, fz / n          # forward
    rx, rz = fz, -fx                 # right

    def project(p):
        x, y, z = p[0] - hips[0], p[1], p[2] - hips[2]
        if view == "front":          # (seen from in front, where the target stands: its right on the left)
            u = -(x * rx + z * rz)
            return PW / 2 + u * SCALE, PH * 0.9 - y * SCALE
        u = x * rx + z * rz           # (from above, facing up the panel)
        v = x * fx + z * fz
        return PW / 2 + u * SCALE, PH * 0.62 - v * SCALE

    return project


def figure(d, f, project, colour, width):
    for a, b in BONES:
        d.line([project(f["p"][a]), project(f["p"][b])], fill=colour, width=width)
    d.line([project(f["p"][19]), project(f["tip"])], fill=colour, width=width + 1)
    x, y = project(f["p"][5])
    d.ellipse([x - 6, y - 6, x + 6, y + 6], outline=colour, width=width)


def view_panel(clip, view):
    img = Image.new("RGB", (PW, PH), BG)
    d = ImageDraw.Draw(img)
    project = viewer(clip, view)
    if view == "front":
        d.line([(0, PH * 0.9), (PW, PH * 0.9)], fill=FAINT)
    path = [project(f["tip"]) for f in clip["frames"]]
    d.line(path, fill=FAINT, width=2)
    core = [project(f["tip"]) for f in clip["frames"] if clip["entry"] - 1e-3 <= f["t"] <= clip["exit"] + 1e-3]
    if len(core) > 1:
        d.line(core, fill=INK, width=4)
    figure(d, frame_at(clip, clip["exit"]), project, EXIT, 2)
    figure(d, frame_at(clip, clip["entry"]), project, ENTRY, 2)
    figure(d, frame_at(clip, clip["contact"]), project, CONTACT, 3)
    d.text((6, 4), "from the target" if view == "front" else "from above", fill=INK)
    return img


def graph(clip, font):
    img = Image.new("RGB", (GW, PH), BG)
    d = ImageDraw.Draw(img)
    fr = clip["frames"]
    t0, t1 = fr[0]["t"], fr[-1]["t"]
    top = max(f["speed"] for f in fr) or 1
    left, right, y0, y1 = 30, GW - 10, PH - 40, 60

    def xy(t, v):
        return left + (t - t0) / max(1e-3, t1 - t0) * (right - left), y0 - v / top * (y0 - y1)

    ex, _ = xy(clip["entry"], 0)
    xx, _ = xy(clip["exit"], 0)
    d.rectangle([ex, y1, xx, y0], fill=(232, 236, 244))
    d.line([(left, y0), (right, y0)], fill=INK)
    d.line([xy(f["t"], f["speed"]) for f in fr], fill=INK, width=2)
    for t, colour, label in ((clip["turnaround"], TURN, "turn"), (clip["entry"], ENTRY, "entry"),
                             (clip["contact"], CONTACT, "contact"), (clip["exit"], EXIT, "exit")):
        if t0 - 1e-3 <= t <= t1 + 1e-3:
            x, _ = xy(t, 0)
            d.line([(x, y1 - 4), (x, y0)], fill=colour, width=2)
            d.text((x - 12, y0 + 4 + (12 if label in ("turn", "contact") else 0)), label, fill=colour, font=font)
    lead = clip["contact"] - clip["entry"]
    d.text((left, 8), clip["name"] + ("  (leap, from takeoff: opener only)" if clip.get("leap") else ""), fill=INK, font=font)
    d.text((left, 26), f'{clip["kind"]}: contact {clip["contact"]:.2f} s, in {lead:.2f} s before, cut {clip["exit"]:.2f} s',
           fill=INK, font=font)
    d.text((left, 42), "tip speed (shaded: the core)", fill=EXIT, font=font)
    return img


def main():
    data = json.load(open(sys.argv[1]))
    out = sys.argv[2]
    strikes = data["strikes"]
    if len(sys.argv) > 3:
        strikes = [s for s in strikes if any(k in s["name"] for k in sys.argv[3].split(","))]
    try:
        font = ImageFont.truetype("DejaVuSans.ttf", 12)
    except OSError:
        font = ImageFont.load_default()
    sheet = Image.new("RGB", (GW + 2 * PW, PH * len(strikes)), BG)
    d = ImageDraw.Draw(sheet)
    for i, clip in enumerate(strikes):
        sheet.paste(graph(clip, font), (0, i * PH))
        sheet.paste(view_panel(clip, "front"), (GW, i * PH))
        sheet.paste(view_panel(clip, "top"), (GW + PW, i * PH))
        d.line([(0, i * PH), (sheet.width, i * PH)], fill=FAINT)
    sheet.save(out)
    print(f"Wrote {out} ({len(strikes)} strikes)")


if __name__ == "__main__":
    main()
