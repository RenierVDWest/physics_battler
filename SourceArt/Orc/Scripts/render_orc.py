"""Preview renders of Orc_Placeholder.blend: turnaround, collider overlay, poses and joint close-ups.

Run:  python3 render_orc.py <output folder that holds Orc_Placeholder.blend>
Writes PNG sheets into <output>/renders/.
"""

import math
import os
import sys

import bpy
from mathutils import Euler, Vector
from PIL import Image, ImageDraw, ImageFont

OUT = os.path.abspath(sys.argv[1] if len(sys.argv) > 1 else "out")
SHOTS = os.path.join(OUT, "renders")
TMP = os.path.join(SHOTS, "_tmp")
os.makedirs(TMP, exist_ok=True)
BG = (214, 216, 220)

POSES = {
    # Club wind-up from the spec: torso coiled, club trailing low behind the right side.
    "windup": {"Pelvis": (0, 0, -8), "Spine": (6, 0, -12), "Chest": (8, 0, -22),
               "Head": (-5, 0, 32),
               "UpperArm_R": (40, 8, 0), "Forearm_R": (-25, 0, 0), "Hand_R": (120, 0, 8),
               "UpperArm_L": (-45, -25, 0), "Forearm_L": (-60, 0, 0),
               "Thigh_R": (12, 4, 0), "Shin_R": (14, 0, 0), "Foot_R": (-10, 0, 0),
               "Thigh_L": (-28, -6, 0), "Shin_L": (30, 0, 0), "Foot_L": (-4, 0, 0)},
    # Joint stress test: big bends everywhere, to check that no joint opens a gap.
    "stress": {"Spine": (-22, 10, 18), "Chest": (-15, -8, 20), "Head": (20, 15, -45),
               "UpperArm_R": (-95, 30, 0), "Forearm_R": (-125, 0, 0), "Hand_R": (-40, 0, 30),
               "UpperArm_L": (30, -85, 0), "Forearm_L": (-110, 0, 0), "Hand_L": (40, 0, 0),
               "Thigh_R": (-85, 10, 0), "Shin_R": (115, 0, 0), "Foot_R": (-30, 0, 0),
               "Thigh_L": (35, -20, 0), "Shin_L": (60, 0, 0), "Foot_L": (35, 0, 0)},
}

# joint close-ups in the stress pose: (label, null at the joint, camera direction from the joint)
CLOSEUPS = [("Shoulder L, arm raised 85°", "UpperArm_L", (1.0, -0.6, 0.3)),
            ("Elbow L, 110°", "Forearm_L", (0.5, -1.0, 0.2)),
            ("Elbow R, 125°", "Forearm_R", (-1.0, -0.4, 0.1)),
            ("Hip R, 85° forward", "Thigh_R", (-1.0, -0.3, 0.2)),
            ("Knee R, 115°", "Shin_R", (-1.0, 0.1, 0.25)),
            ("Waist + chest twisted and bent", "Spine", (0.3, -1.0, 0.4)),
            ("Neck, head turned 45°", "Head", (0.7, -0.6, 0.5)),
            ("Ankle L, 35°", "Foot_L", (1.0, -0.3, 0.2))]

COLLIDER_COLOURS = [(0.95, 0.25, 0.45), (0.15, 0.55, 0.95), (1.0, 0.65, 0.1), (0.2, 0.75, 0.45)]


def setup():
    bpy.ops.wm.open_mainfile(filepath=os.path.join(OUT, "Orc_Placeholder.blend"))
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_WORKBENCH"
    scene.render.film_transparent = True
    sh = scene.display.shading
    sh.light = "STUDIO"
    sh.color_type = "MATERIAL"
    sh.show_cavity = True
    sh.cavity_type = "WORLD"
    sh.show_object_outline = True
    sh.object_outline_color = (0.05, 0.05, 0.05)
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


def render(scene, name, size=(900, 1100)):
    scene.render.resolution_x, scene.render.resolution_y = size
    path = os.path.join(TMP, name + ".png")
    scene.render.filepath = path
    bpy.ops.render.render(write_still=True)
    return path


def on_bg(path, ground_y=None):
    img = Image.open(path).convert("RGBA")
    bg = Image.new("RGBA", img.size, BG + (255,))
    if ground_y is not None:
        ImageDraw.Draw(bg).rectangle([0, ground_y, img.width, img.height], fill=(196, 198, 203, 255))
    return Image.alpha_composite(bg, img)


def pose(name):
    for o in bpy.data.objects:
        if o.type == "EMPTY":
            o.rotation_euler = (0, 0, 0)
    for null, angles in POSES.get(name, {}).items():
        bpy.data.objects[null].rotation_euler = Euler([math.radians(a) for a in angles])
    bpy.context.view_layer.update()


def body_objects():
    return [o for o in bpy.data.collections["Orc_Rig"].objects if o.type == "MESH"]


def label(img, text):
    d = ImageDraw.Draw(img)
    try:
        font = ImageFont.truetype("DejaVuSans.ttf", 22)
    except OSError:
        font = ImageFont.load_default()
    box = d.textbbox((14, 10), text, font=font)
    d.rectangle([box[0] - 8, box[1] - 6, box[2] + 8, box[3] + 6], fill=(245, 245, 247))
    d.text((14, 10), text, fill=(30, 30, 34), font=font)
    return img


def sheet(images, cols, path):
    w, h = images[0].size
    rows = (len(images) + cols - 1) // cols
    out = Image.new("RGB", (w * cols, h * rows), BG)
    for k, img in enumerate(images):
        out.paste(img.convert("RGB"), ((k % cols) * w, (k // cols) * h))
    out.save(path)


def main():
    scene, cam = setup()
    centre = (0, 0, 1.0)
    ortho_ground = int(1100 * (0.5 + (1.05 - 0.0) / 2.9))  # pixel row of z = 0 in the ortho shots

    # 1. turnaround with the club
    views = [("front", (0, -9, 1.05), centre, 2.9), ("side", (9, 0, 1.05), centre, 2.9),
             ("three_quarter", (-4.2, -5.6, 2.4), (-0.2, -0.5, 0.95), None),
             ("back", (3.4, 5.0, 2.2), centre, None)]
    imgs = []
    for name, loc, tgt, ortho in views:
        aim(cam, loc, tgt, ortho=ortho, lens=55)
        imgs.append(on_bg(render(scene, name), ortho_ground if ortho else None))
    sheet([i.resize((450, 550)) for i in imgs], 4, os.path.join(SHOTS, "orc_turnaround.png"))

    aim(cam, (-0.9, -1.6, 2.0), (0, -0.08, 1.78), lens=60)
    on_bg(render(scene, "head", (700, 700))).convert("RGB").save(os.path.join(SHOTS, "orc_head.png"))

    # 2. colliders over a faded body (two passes, composited)
    coll = bpy.data.collections["Colliders"]
    for k, o in enumerate(sorted(coll.objects, key=lambda o: o.name)):
        o.display_type = "SOLID"
        o.color = (*COLLIDER_COLOURS[k % len(COLLIDER_COLOURS)], 1.0)
    over = []
    for name, loc in (("front", (0, -9, 1.05)), ("side", (9, 0, 1.05))):
        aim(cam, loc, centre, ortho=2.9)
        coll.hide_render = True
        body = on_bg(render(scene, f"cbody_{name}"), ortho_ground)
        body = Image.blend(body, Image.new("RGBA", body.size, (255, 255, 255, 255)), 0.6)
        coll.hide_render = False
        for o in body_objects():
            o.hide_render = True
        scene.display.shading.color_type = "OBJECT"
        cols = Image.open(render(scene, f"cols_{name}")).convert("RGBA")
        scene.display.shading.color_type = "MATERIAL"
        for o in body_objects():
            o.hide_render = False
        coll.hide_render = True
        over.append(label(Image.alpha_composite(body, cols), f"Colliders, {name}"))
    sheet([i.resize((450, 550)) for i in over], 2, os.path.join(SHOTS, "orc_colliders.png"))

    # 3. poses
    imgs = []
    for name in POSES:
        pose(name)
        aim(cam, (-4.6, -5.0, 2.2), (0, 0, 0.95), lens=68)
        imgs.append(label(on_bg(render(scene, f"pose_{name}")), f"Pose: {name}"))
        aim(cam, (4.6, 3.6, 2.0), (-0.3, 0.3, 0.9), lens=58)
        imgs.append(on_bg(render(scene, f"pose_{name}_back")))
    sheet([i.resize((450, 550)) for i in imgs], 4, os.path.join(SHOTS, "orc_poses.png"))

    # 4. joint close-ups in the stress pose
    pose("stress")
    bpy.data.objects["SM_Club_Big"].hide_render = True
    imgs = []
    for k, (text, null, direction) in enumerate(CLOSEUPS):
        joint = bpy.data.objects[null].matrix_world.translation.copy()
        d = Vector(direction).normalized()
        aim(cam, joint + d * 1.35, joint, lens=50)
        imgs.append(label(on_bg(render(scene, f"close_{k}", (500, 500))), text))
    sheet(imgs, 4, os.path.join(SHOTS, "orc_joint_closeups.png"))
    pose("")


main()
