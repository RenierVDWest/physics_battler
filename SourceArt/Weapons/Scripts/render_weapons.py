"""A look at the weapons beside the medium body for scale.

Run:  python3 render_weapons.py <weapons output folder> <bodies output folder> <out.png>
"""
import os
import sys

import bpy
from mathutils import Vector
from PIL import Image

W, B, OUT = (os.path.abspath(a) for a in sys.argv[1:4])
bpy.ops.wm.open_mainfile(filepath=os.path.join(B, "Bodies.blend"))
scene = bpy.context.scene
for o in bpy.data.objects:
    if o.type == "MESH":
        o.hide_render = not o.name.startswith("SM_Medium_")
with bpy.data.libraries.load(os.path.join(W, "Weapons.blend")) as (src, dst):
    dst.objects = [n for n in src.objects if n.startswith("SM_")]
for k, o in enumerate(dst.objects):
    scene.collection.objects.link(o)
    o.location = Vector((-0.55 - 0.45 * k, 0.0, 1.0))     # beside the body, standing up
    o.rotation_euler = (-1.5708, 0, 0) if o.name != "SM_Shield" else (0, 0, 0)
scene.render.engine = "BLENDER_WORKBENCH"
scene.render.film_transparent = True
sh = scene.display.shading
sh.light = "STUDIO"
sh.color_type = "MATERIAL"
sh.show_cavity = True
cam = bpy.data.objects.new("Cam", bpy.data.cameras.new("Cam"))
scene.collection.objects.link(cam)
scene.camera = cam
cam.data.type = "ORTHO"
cam.data.ortho_scale = 2.3
cam.location = Vector((-3.0, -6.0, 1.6))
cam.rotation_euler = (Vector((-0.6, 0, 0.95)) - cam.location).to_track_quat("-Z", "Y").to_euler()
scene.render.resolution_x, scene.render.resolution_y = 1100, 900
scene.render.filepath = OUT
bpy.ops.render.render(write_still=True)
img = Image.open(OUT).convert("RGBA")
Image.alpha_composite(Image.new("RGBA", img.size, (46, 46, 50, 255)), img).convert("RGB").save(OUT)
