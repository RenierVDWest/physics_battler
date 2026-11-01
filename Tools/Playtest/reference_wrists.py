"""Measure the wrists in the reference attack FBXs (Mixamo): per frame, the hand's turn against the
forearm (relative to the rest pose), split into the bend about the hand's two side axes and the roll
about the forearm; also the elbow bend. Prints percentiles per clip and side.

Run: python3 wrists.py <fbx>...
"""
import math
import sys

import bpy
import numpy as np


def load(path):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=path)
    arm = next(o for o in bpy.data.objects if o.type == "ARMATURE")
    return arm


def name(arm, key):
    for b in arm.pose.bones:
        if b.name.endswith(key):
            return b.name
    return None


def measure(path):
    arm = load(path)
    scene = bpy.context.scene
    act = arm.animation_data.action if arm.animation_data else None
    f0, f1 = (int(act.frame_range[0]), int(act.frame_range[1])) if act else (scene.frame_start, scene.frame_end)
    out = {}
    for side in ("Right", "Left"):
        fa, hd = name(arm, f"{side}ForeArm"), name(arm, f"{side}Hand")
        ua = name(arm, f"{side}Arm")
        rest_f = arm.data.bones[fa].matrix_local.to_quaternion()
        rest_h = arm.data.bones[hd].matrix_local.to_quaternion()
        rest_rel = rest_f.inverted() @ rest_h
        rows = []
        for f in range(f0, f1 + 1):
            scene.frame_set(f)
            qf = (arm.matrix_world @ arm.pose.bones[fa].matrix).to_quaternion()
            qh = (arm.matrix_world @ arm.pose.bones[hd].matrix).to_quaternion()
            qu = (arm.matrix_world @ arm.pose.bones[ua].matrix).to_quaternion()
            rel = rest_rel.inverted() @ (qf.inverted() @ qh)   # the hand against the forearm, rest removed
            # Blender bones point along +Y: the forearm's line is the hand's Y at rest.
            # swing-twist about Y: twist = roll about the forearm, swing = the wrist's bend.
            w, x, y, z = rel.w, rel.x, rel.y, rel.z
            twist = 2 * math.degrees(math.atan2(y, w))
            twist = (twist + 180) % 360 - 180
            t = math.radians(twist) / 2
            tq = type(rel)((math.cos(t), 0, math.sin(t), 0))
            sw = rel @ tq.inverted()
            if sw.w < 0:
                sw = -sw
            bx = 2 * math.degrees(math.asin(max(-1, min(1, sw.x))))
            bz = 2 * math.degrees(math.asin(max(-1, min(1, sw.z))))
            bend = 2 * math.degrees(math.acos(max(-1, min(1, sw.w))))
            # elbow bend: the angle between the upper arm's and forearm's lines
            ya = qu @ type(rel)((0, 0, 1, 0)) @ qu.inverted()
            elbow = math.degrees((qu.to_matrix().col[1]).angle(qf.to_matrix().col[1]))
            rows.append((bend, bx, bz, twist, elbow))
        out[side] = np.array(rows)
    return out


for p in sys.argv[1:]:
    clip = p.rsplit("/", 1)[-1].split("-", 1)[-1].replace(".fbx", "")
    res = measure(p)
    for side, a in res.items():
        pc = lambda col: "/".join(f"{v:.0f}" for v in np.percentile(a[:, col], [5, 50, 95]))
        print(f"{clip:45s} {side:5s} wrist bend {pc(0):>11s} (about X {pc(1):>12s}, about Z {pc(2):>12s})  roll {pc(3):>12s} |roll| 95% {np.percentile(np.abs(a[:,3]),95):.0f}  elbow {pc(4):>11s}  frames {len(a)}")
