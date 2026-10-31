"""One body in a fighting guard and a joint stress test, front three-quarter and from behind.

Run:  python3 render_pose.py <output folder with Bodies.blend> <Body>
"""

import math
import os
import sys

import bpy
from mathutils import Euler, Vector
from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from render_bodies import GUARD, STRESS  # noqa: E402

OUT, BODY = os.path.abspath(sys.argv[1]), sys.argv[2]
BG = (46, 46, 50)
bpy.ops.wm.open_mainfile(filepath=os.path.join(OUT, "Bodies.blend"))
scene = bpy.context.scene
scene.render.engine = "BLENDER_WORKBENCH"
scene.render.film_transparent = True
scene.display.shading.light = "STUDIO"
scene.display.shading.color_type = "MATERIAL"
scene.display.shading.show_cavity = True
cam = bpy.data.objects.new("Cam", bpy.data.cameras.new("Cam"))
scene.collection.objects.link(cam)
scene.camera = cam
root = bpy.data.objects[f"{BODY}:Root"].matrix_world.translation
for o in bpy.data.objects:
    if o.type == "MESH" and o.name.startswith("SM_"):
        o.hide_render = not o.name.startswith(f"SM_{BODY}_")
H = 1.85 * (0.5 if BODY == "Small" else 1.0 if BODY == "Medium" else 1.38)


def pose(angles):
    for o in bpy.data.objects:
        if o.type == "EMPTY" and o.name.startswith(f"{BODY}:"):
            o.rotation_euler = (0, 0, 0)
    for null, a in angles.items():
        bpy.data.objects[f"{BODY}:{null}"].rotation_euler = Euler([math.radians(x) for x in a])
    bpy.context.view_layer.update()


def shot(direction):
    tgt = root + Vector((0, 0, 0.5 * H))
    cam.location = tgt + Vector(direction).normalized() * 1.75 * H
    cam.rotation_euler = (tgt - cam.location).to_track_quat("-Z", "Y").to_euler()
    cam.data.lens = 50
    scene.render.resolution_x, scene.render.resolution_y = 700, 900
    path = os.path.join(OUT, "_pose.png")
    scene.render.filepath = path
    bpy.ops.render.render(write_still=True)
    img = Image.open(path).convert("RGBA")
    return Image.alpha_composite(Image.new("RGBA", img.size, BG + (255,)), img)


imgs = []
for angles in (GUARD, STRESS):
    pose(angles)
    imgs.append(shot((-0.55, -0.8, 0.25)))
    imgs.append(shot((0.6, 0.7, 0.2)))
sheet = Image.new("RGB", (700 * 4, 900), BG)
for k, im in enumerate(imgs):
    sheet.paste(im.convert("RGB"), (700 * k, 0))
sheet.save(os.path.join(OUT, f"pose_{BODY}.png"))
