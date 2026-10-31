"""Preview renders of Bodies.blend: the lineup from four sides, wireframe, colliders, and a joint
stress pose per body.

Run:  python3 render_bodies.py <output folder that holds Bodies.blend>
Writes PNG sheets into <output>/Renders/.
"""

import math
import os
import sys

import bpy
from mathutils import Euler, Vector
from PIL import Image, ImageDraw, ImageFont

OUT = os.path.abspath(sys.argv[1] if len(sys.argv) > 1 else "out")
SHOTS = os.path.join(OUT, "Renders")
TMP = os.path.join(SHOTS, "_tmp")
os.makedirs(TMP, exist_ok=True)
BG = (46, 46, 50)
BODIES = ["Small", "Medium", "Large"]

STRESS = {"Spine": (-20, 10, 18), "Chest": (-15, -8, 20), "Head": (20, 15, -40),
          "UpperArm_R": (-95, 30, 0), "Forearm_R": (-120, 0, 0), "Hand_R": (-40, 0, 30),
          "UpperArm_L": (30, -80, 0), "Forearm_L": (-110, 0, 0), "Hand_L": (40, 0, 0),
          "Thigh_R": (-85, 10, 0), "Shin_R": (110, 0, 0), "Foot_R": (-30, 0, 0),
          "Thigh_L": (35, -20, 0), "Shin_L": (60, 0, 0), "Foot_L": (35, 0, 0)}
GUARD = {"Pelvis": (0, 0, 10), "Spine": (8, 0, 6), "Chest": (6, 0, 10), "Head": (-8, 0, -12),
         "UpperArm_R": (-40, 0, -20), "Forearm_R": (-95, 0, 0), "UpperArm_L": (-55, 0, 25), "Forearm_L": (-110, 0, 0),
         "Thigh_R": (18, 0, 6), "Shin_R": (30, 0, 0), "Foot_R": (-14, 0, 0),
         "Thigh_L": (-34, 0, -6), "Shin_L": (36, 0, 0), "Foot_L": (-4, 0, 0)}


def setup():
    bpy.ops.wm.open_mainfile(filepath=os.path.join(OUT, "Bodies.blend"))
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_WORKBENCH"
    scene.render.film_transparent = True
    sh = scene.display.shading
    sh.light = "STUDIO"
    sh.color_type = "MATERIAL"
    sh.show_cavity = True
    sh.cavity_type = "WORLD"
    sh.show_object_outline = False
    cam = bpy.data.objects.new("Cam", bpy.data.cameras.new("Cam"))
    scene.collection.objects.link(cam)
    scene.camera = cam
    return scene, cam


def aim(cam, location, target, ortho=None, lens=50):
    cam.location = Vector(location)
    cam.rotation_euler = (Vector(target) - cam.location).to_track_quat("-Z", "Y").to_euler()
    cam.data.clip_start = 0.02
    if ortho:
        cam.data.type, cam.data.ortho_scale = "ORTHO", ortho
    else:
        cam.data.type, cam.data.lens = "PERSP", lens


def render(scene, name, size=(1000, 900)):
    scene.render.resolution_x, scene.render.resolution_y = size
    path = os.path.join(TMP, name + ".png")
    scene.render.filepath = path
    bpy.ops.render.render(write_still=True)
    return path


def on_bg(path):
    img = Image.open(path).convert("RGBA")
    return Image.alpha_composite(Image.new("RGBA", img.size, BG + (255,)), img)


def label(img, text, size=24):
    d = ImageDraw.Draw(img)
    try:
        font = ImageFont.truetype("DejaVuSans.ttf", size)
    except OSError:
        font = ImageFont.load_default()
    d.text((16, 12), text, fill=(225, 225, 230), font=font)
    return img


def sheet(images, cols, path):
    w, h = images[0].size
    rows = (len(images) + cols - 1) // cols
    out = Image.new("RGB", (w * cols, h * rows), BG)
    for k, img in enumerate(images):
        out.paste(img.convert("RGB"), ((k % cols) * w, (k // cols) * h))
    out.save(path)


def meshes(body=None):
    return [o for o in bpy.data.objects if o.type == "MESH" and o.name.startswith("SM_")
            and (body is None or o.name.startswith(f"SM_{body}_"))]


def pose(angles, body=None):
    for o in bpy.data.objects:
        if o.type == "EMPTY":
            o.rotation_euler = (0, 0, 0)
    for b in ([body] if body else BODIES):
        for null, a in angles.items():
            bpy.data.objects[f"{b}:{null}"].rotation_euler = Euler([math.radians(x) for x in a])
    bpy.context.view_layer.update()


def wire(on):
    for o in meshes():
        o.show_wire = on
    bpy.context.scene.display.shading.show_object_outline = False


def main():
    scene, cam = setup()
    centre = (0.3, 0, 1.25)
    imgs = []
    for name, loc in (("front", (0.3, -12, 1.25)), ("side", (12, 0, 1.25)), ("back", (0.3, 12, 1.25))):
        aim(cam, loc, centre, ortho=4.3)
        imgs.append(label(on_bg(render(scene, name, (1100, 1100))), name))
    aim(cam, (-3.9, -5.4, 2.3), (0.3, 0, 1.05), lens=50)
    imgs.append(label(on_bg(render(scene, "threeq", (1100, 1100))), "three quarter"))
    sheet([i.resize((800, 800)) for i in imgs], 2, os.path.join(SHOTS, "bodies_lineup.png"))

    # wireframe, front, close per body
    scene.display.shading.color_type = "MATERIAL"
    imgs = []
    for body, x, height in (("Small", -1.0, 0.95), ("Medium", 0.0, 1.85), ("Large", 1.6, 2.45)):
        for o in meshes():
            o.hide_render = not o.name.startswith(f"SM_{body}_")
        # workbench draws no wire in renders: overlay a wireframe pass made with a modifier
        for o in meshes(body):
            m = o.modifiers.new("WirePreview", "WIREFRAME")
            m.thickness = 0.0012 * height / 1.85
            m.use_replace = False
            m.use_even_offset = False
        aim(cam, (x - 0.9 * height, -2.4 * height, 0.62 * height), (x, 0, 0.52 * height), lens=55)
        imgs.append(label(on_bg(render(scene, f"wire_{body}", (900, 1100))), f"{body} (topology)"))
        for o in meshes(body):
            o.modifiers.remove(o.modifiers["WirePreview"])
    for o in meshes():
        o.hide_render = False
    sheet([i.resize((600, 733)) for i in imgs], 3, os.path.join(SHOTS, "bodies_wire.png"))

    # colliders
    for b in BODIES:
        coll = bpy.data.collections[f"{b}_Colliders"]
        for o in coll.objects:
            o.display_type = "SOLID"
            o.color = (0.95, 0.3, 0.5, 1.0) if any(k in o.name for k in ("_L", "Pelvis", "Chest", "Head")) \
                else (0.2, 0.6, 1.0, 1.0)
        coll.hide_render = False
    for o in meshes():
        o.hide_render = True
    scene.display.shading.color_type = "OBJECT"
    over = []
    for name, loc in (("front", (0.3, -12, 1.25)), ("side", (12, 0, 1.25))):
        aim(cam, loc, centre, ortho=4.3)
        over.append(label(on_bg(render(scene, f"cols_{name}", (1100, 1100))), f"colliders, {name}"))
    for b in BODIES:
        bpy.data.collections[f"{b}_Colliders"].hide_render = True
    for o in meshes():
        o.hide_render = False
    scene.display.shading.color_type = "MATERIAL"
    sheet([i.resize((800, 800)) for i in over], 2, os.path.join(SHOTS, "bodies_colliders.png"))

    # poses: a fighting guard and the joint stress test, all three
    imgs = []
    for pname, angles in (("guard", GUARD), ("stress", STRESS)):
        pose(angles)
        aim(cam, (-3.6, -5.2, 2.2), (0.3, 0, 0.95), lens=50)
        imgs.append(label(on_bg(render(scene, f"pose_{pname}", (1100, 1100))), f"pose: {pname}"))
        aim(cam, (4.6, 4.4, 2.0), (0.3, 0, 0.95), lens=50)
        imgs.append(label(on_bg(render(scene, f"pose_{pname}_b", (1100, 1100))), f"pose: {pname}, behind"))
    pose({})
    sheet([i.resize((800, 800)) for i in imgs], 2, os.path.join(SHOTS, "bodies_poses.png"))


if __name__ == "__main__":
    main()
