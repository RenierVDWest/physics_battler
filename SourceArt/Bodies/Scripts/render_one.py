"""Quick look at one body while tuning: front, side, three-quarter and a head close-up.

Run:  python3 render_one.py <output folder with Bodies.blend> <Small|Medium|Large> [wire]
"""

import os
import sys

import bpy
from mathutils import Vector
from PIL import Image

sys.argv = [a for a in sys.argv]
OUT = os.path.abspath(sys.argv[1])
BODY = sys.argv[2]
WIRE = len(sys.argv) > 3
X = {"Small": -1.0, "Medium": 0.0, "Large": 1.6}
BG = (46, 46, 50)

bpy.ops.wm.open_mainfile(filepath=os.path.join(OUT, "Bodies.blend"))
scene = bpy.context.scene
scene.render.engine = "BLENDER_WORKBENCH"
scene.render.film_transparent = True
sh = scene.display.shading
sh.light = "STUDIO"
sh.color_type = "MATERIAL"
sh.show_cavity = True
cam = bpy.data.objects.new("Cam", bpy.data.cameras.new("Cam"))
scene.collection.objects.link(cam)
scene.camera = cam
for o in bpy.data.objects:
    if o.type == "MESH" and o.name.startswith("SM_"):
        o.hide_render = not o.name.startswith(f"SM_{BODY}_")
        if WIRE and not o.hide_render:
            m = o.modifiers.new("W", "WIREFRAME")
            m.thickness = 0.0015
            m.use_replace = False
bpy.context.view_layer.update()
H = max((o.matrix_world @ v.co).z for o in bpy.data.objects if o.type == "MESH" and o.name.startswith(f"SM_{BODY}_Head") for v in o.data.vertices) + 0.02
x = X[BODY]


def shot(loc, tgt, ortho, size=(700, 900)):
    cam.location = Vector(loc)
    cam.rotation_euler = (Vector(tgt) - cam.location).to_track_quat("-Z", "Y").to_euler()
    cam.data.type = "ORTHO"
    cam.data.ortho_scale = ortho
    scene.render.resolution_x, scene.render.resolution_y = size
    path = os.path.join(OUT, "_shot.png")
    scene.render.filepath = path
    bpy.ops.render.render(write_still=True)
    img = Image.open(path).convert("RGBA")
    return Image.alpha_composite(Image.new("RGBA", img.size, BG + (255,)), img)


c = (x, 0, H * 0.5)
d = 4 * H
imgs = [shot((x, -d, H * 0.5), c, H * 1.12), shot((x + d, 0, H * 0.5), c, H * 1.12),
        shot((x - d * 0.6, -d * 0.8, H * 0.62), c, H * 1.12)]
hz = bpy.data.objects[f"{BODY}:Head"].matrix_world.translation.z
hc = (x, 0, hz + 0.07 * H / 1.85)
imgs.append(shot((x - 0.3 * d, -d, hz + 0.05 * H), hc, 0.42 * H / 1.85 * 1.0 + 0.12 * H, (700, 900)))
sheet = Image.new("RGB", (700 * 4, 900), BG)
for k, im in enumerate(imgs):
    sheet.paste(im.convert("RGB"), (700 * k, 0))
sheet.save(os.path.join(OUT, f"one_{BODY}.png"))
