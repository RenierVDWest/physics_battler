"""The fists (milestone 9b): each body's right hand, closed round a grip (a dark rod through the
weapon slot, as thick as the club's binding), from four sides: the back of the hand, the palm and
thumb, down the grip (the channel), and three-quarter. The forearm is shown for where it joins.

Run:  python3 render_fists.py <output folder with Bodies.blend>      -> fists.png
"""

import math
import os
import sys

import bpy
from mathutils import Vector
from PIL import Image, ImageDraw

OUT = os.path.abspath(sys.argv[1])
BG = (46, 46, 50)
BODIES = ["Small", "Medium", "Large"]
SCALE = {"Small": 0.7, "Medium": 1.0, "Large": 1.38}
CELL = 360

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
rod_mat = bpy.data.materials.new("M_Rod")
rod_mat.diffuse_color = (0.12, 0.11, 0.1, 1.0)


def show(body):
    for o in bpy.data.objects:
        if o.type == "MESH":
            o.hide_render = not (o.name in (f"SM_{body}_Hand_R", f"SM_{body}_Forearm_R") or o.name == f"ROD_{body}")


def shot(loc, tgt, ortho):
    cam.location = Vector(loc)
    cam.rotation_euler = (Vector(tgt) - cam.location).to_track_quat("-Z", "Y").to_euler()
    cam.data.type = "ORTHO"
    cam.data.ortho_scale = ortho
    scene.render.resolution_x = scene.render.resolution_y = CELL
    path = os.path.join(OUT, "_fist.png")
    scene.render.filepath = path
    bpy.ops.render.render(write_still=True)
    img = Image.open(path).convert("RGBA")
    return Image.alpha_composite(Image.new("RGBA", img.size, BG + (255,)), img).convert("RGB")


rows = []
for body in BODIES:
    slot = bpy.data.objects[f"{body}:Slot_Weapon_R"].matrix_world.translation.copy()
    wrist = bpy.data.objects[f"{body}:Hand_R"].matrix_world.translation.copy()
    k = SCALE[body]
    # The grip: along the slot's forward (the body's front, Blender -Y), 0.021 m (club binding) at medium size.
    bpy.ops.mesh.primitive_cylinder_add(radius=0.021 * k, depth=0.24 * k, location=slot, rotation=(math.pi / 2, 0, 0))
    rod = bpy.context.active_object
    rod.name = f"ROD_{body}"
    rod.data.materials.append(rod_mat)
    show(body)
    bpy.context.view_layer.update()
    c = (wrist + slot) / 2
    d = 1.0
    ortho = 0.3 * k
    along = (slot - wrist).normalized()
    out = Vector((-1, 0, 0))   # the right hand's outside (Blender -X)
    views = [c + out * d,                                   # the back of the hand
             c - out * d,                                   # the palm and thumb
             c + Vector((0, -d, 0)),                        # down the grip, from the front
             c + (out * 0.6 + Vector((0, -0.6, 0.35))).normalized() * d]
    rows.append([shot(v, c, ortho) for v in views])

labels = ["back of the hand", "palm and thumb", "down the grip", "three-quarter"]
sheet = Image.new("RGB", (CELL * 4, CELL * len(BODIES) + 24), BG)
draw = ImageDraw.Draw(sheet)
for j, text in enumerate(labels):
    draw.text((CELL * j + 8, 6), text, fill=(200, 200, 200))
for i, row in enumerate(rows):
    for j, img in enumerate(row):
        sheet.paste(img, (CELL * j, 24 + CELL * i))
    draw.text((8, 24 + CELL * i + 6), BODIES[i], fill=(220, 220, 220))
sheet.save(os.path.join(OUT, "fists.png"))
print("FISTS", os.path.join(OUT, "fists.png"))
