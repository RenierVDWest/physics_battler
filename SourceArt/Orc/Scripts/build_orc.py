"""Builds the placeholder orc and its 2 m club in Blender, and writes everything Unity needs.

Run:  python3 build_orc.py <output folder>      (Blender 5.x as a Python module, or
      blender -b -P build_orc.py -- <output folder>)

Conventions (same as the design spec):
  * Blender: Z up, the orc faces -Y, its RIGHT side is on -X.  Unity: Y up, faces +Z, right is +X.
  * 1 unit = 1 m.  The orc stands on z = 0 and is about 2.0 m tall.
  * One null per body part, at the joint, with identity rotation in the rest pose.
    Each part's mesh has its origin (pivot) at that null.
  * Joints are ball joints: both parts carry a sphere centred on the shared pivot, so any rotation
    about the pivot keeps the joint closed.  These overlap zones are deliberately NOT covered by
    colliders: every collider stops short of the joints at both ends.

Outputs (in the output folder):
  Orc_Placeholder.blend            null hierarchy, meshes, collider previews (hidden collection)
  FBX/SM_Orc_<Part>.fbx            one file per body part, pivot at the origin, ready for Unity
  FBX/SM_Club_Big.fbx              the club, origin at the grip, pointing along Unity +Z
  FBX/REF_Orc_Assembled.fbx        every part in place (flat, no hierarchy) for viewing in a DCC
  orc_rig.json                     nulls, masses, colliders and club markers in Unity coordinates
"""

import json
import math
import os
import sys

import bpy  # must come first when Blender runs as a Python module: it provides bmesh/mathutils
import bmesh
from mathutils import Matrix, Vector

# ----------------------------------------------------------------------------------- layout

ARM_TILT = math.radians(14.0)   # arms hang 14 degrees out from vertical in the rest pose
UPPER_ARM = 0.40
FOREARM = 0.38
FIST = 0.095                    # wrist pivot to fist centre (the weapon slot)

# Joint heights and offsets (metres).  Everything else is derived from these.
ANKLE_Z, KNEE_Z, HIP_Z = 0.10, 0.50, 0.88
HIP_X = 0.18
PELVIS_Z, WAIST_Z, CHEST_Z = 0.92, 1.07, 1.30
SHOULDER = (0.40, 1.56)          # x, z
NECK = Vector((0, -0.05, 1.62))  # head pivot: forward of the spine for the hunch
HEAD_C = Vector((0, -0.08, 1.81))  # centre of the cranium


def arm_dir(s):
    """Unit vector down the arm. s = -1 right (-X), +1 left (+X)."""
    return Vector((s * math.sin(ARM_TILT), 0.0, -math.cos(ARM_TILT)))


def side_name(s):
    return "R" if s < 0 else "L"


# name -> (parent, world position).  Every null has identity rotation.
NULLS = {"Root": (None, Vector((0, 0, 0))),
         "Pelvis": ("Root", Vector((0, 0, PELVIS_Z))),
         "Spine": ("Pelvis", Vector((0, 0, WAIST_Z))),
         "Chest": ("Spine", Vector((0, 0, CHEST_Z))),
         "Head": ("Chest", NECK.copy()),
         "Aim_Chest": ("Chest", Vector((0, -0.23, 1.46))),
         "Aim_Head": ("Head", HEAD_C + Vector((0, -0.02, 0)))}
for _s in (-1, 1):
    _n = side_name(_s)
    _sh = Vector((_s * SHOULDER[0], 0, SHOULDER[1]))
    _el = _sh + arm_dir(_s) * UPPER_ARM
    _wr = _el + arm_dir(_s) * FOREARM
    NULLS[f"UpperArm_{_n}"] = ("Chest", _sh)
    NULLS[f"Forearm_{_n}"] = (f"UpperArm_{_n}", _el)
    NULLS[f"Hand_{_n}"] = (f"Forearm_{_n}", _wr)
    NULLS[f"Slot_Weapon_{_n}"] = (f"Hand_{_n}", _wr + arm_dir(_s) * FIST)
    NULLS[f"Thigh_{_n}"] = ("Pelvis", Vector((_s * HIP_X, 0, HIP_Z)))
    NULLS[f"Shin_{_n}"] = (f"Thigh_{_n}", Vector((_s * HIP_X, 0, KNEE_Z)))
    NULLS[f"Foot_{_n}"] = (f"Shin_{_n}", Vector((_s * HIP_X, 0, ANKLE_Z)))

BODY_PARTS = ["Pelvis", "Spine", "Chest", "Head",
              "UpperArm_R", "Forearm_R", "Hand_R", "UpperArm_L", "Forearm_L", "Hand_L",
              "Thigh_R", "Shin_R", "Foot_R", "Thigh_L", "Shin_L", "Foot_L"]

# Mass share of the 120 kg orc (design spec: hands and feet heavier than real for joint stability).
TOTAL_MASS = 120.0
MASS_PCT = {"Pelvis": 14, "Spine": 13, "Chest": 15, "Head": 7,
            "UpperArm": 4, "Forearm": 3, "Hand": 2, "Thigh": 9, "Shin": 5, "Foot": 2.5}


def P(name):
    return NULLS[name][1].copy()


# ----------------------------------------------------------------------------------- materials

COLOURS = {"M_Orc_Skin": (0.33, 0.46, 0.20), "M_Orc_SkinDark": (0.22, 0.31, 0.13),
           "M_Orc_Leather": (0.36, 0.22, 0.12), "M_Orc_LeatherDark": (0.20, 0.12, 0.07),
           "M_Orc_Iron": (0.42, 0.43, 0.45), "M_Orc_Bone": (0.88, 0.83, 0.68),
           "M_Orc_Eye": (0.95, 0.62, 0.08), "M_Orc_Hair": (0.08, 0.07, 0.06),
           "M_Club_Wood": (0.45, 0.30, 0.16), "M_Club_WoodDark": (0.30, 0.19, 0.10)}


def make_materials():
    mats = {}
    for name, rgb in COLOURS.items():
        mat = bpy.data.materials.new(name)
        mat.diffuse_color = (*rgb, 1.0)
        mat.use_nodes = True
        bsdf = mat.node_tree.nodes.get("Principled BSDF")
        if bsdf:
            bsdf.inputs["Base Color"].default_value = (*rgb, 1.0)
            bsdf.inputs["Roughness"].default_value = 0.35 if "Iron" in name else 0.8
            if "Iron" in name:
                bsdf.inputs["Metallic"].default_value = 0.8
        mats[name] = mat
    return mats


# ----------------------------------------------------------------------------------- mesh kit

def align_z(direction):
    """Rotation matrix taking +Z onto direction."""
    return Vector((0, 0, 1)).rotation_difference(direction.normalized()).to_matrix().to_4x4()


class Part:
    """Accumulates primitives in WORLD space for one rigid part."""

    def __init__(self, name):
        self.name = name
        self.bm = bmesh.new()
        self.mats = []

    def _tag(self, verts, mat):
        if mat not in self.mats:
            self.mats.append(mat)
        index = self.mats.index(mat)
        for f in {f for v in verts for f in v.link_faces}:
            f.material_index = index

    def ellipsoid(self, centre, radii, mat, u=12, v=8, rot=None):
        m = Matrix.Translation(Vector(centre)) @ (rot or Matrix.Identity(4)) @ \
            Matrix.Diagonal((*radii, 1.0))
        r = bmesh.ops.create_uvsphere(self.bm, u_segments=u, v_segments=v, radius=1.0, matrix=m)
        self._tag(r["verts"], mat)

    def ball(self, centre, radius, mat, u=12, v=8):
        self.ellipsoid(centre, (radius, radius, radius), mat, u, v)

    def frustum(self, a, b, ra, rb, mat, segs=10, squash=1.0):
        """A tapered cylinder from a (radius ra) to b (radius rb). squash scales it across X'."""
        a, b = Vector(a), Vector(b)
        axis = b - a
        m = Matrix.Translation((a + b) / 2) @ align_z(axis) @ Matrix.Diagonal((squash, 1, 1, 1))
        r = bmesh.ops.create_cone(self.bm, cap_ends=True, cap_tris=False, segments=segs,
                                  radius1=ra, radius2=rb, depth=axis.length, matrix=m)
        self._tag(r["verts"], mat)

    def limb(self, a, b, ra, rb, mat, ball_a=None, ball_b=None, segs=10):
        """Frustum plus balls centred on both joint pivots (the ball-joint rule)."""
        self.frustum(a, b, ra, rb, mat, segs)
        if ball_a:
            self.ball(a, ball_a, mat)
        if ball_b:
            self.ball(b, ball_b, mat)

    def box(self, centre, size, mat, rot=None):
        m = Matrix.Translation(Vector(centre)) @ (rot or Matrix.Identity(4)) @ \
            Matrix.Diagonal((*size, 1.0))
        r = bmesh.ops.create_cube(self.bm, size=1.0, matrix=m)
        self._tag(r["verts"], mat)

    def spike(self, base, tip, radius, mat, segs=6):
        self.frustum(base, tip, radius, 0.0005, mat, segs)

    def to_object(self, mats, pivot):
        """Mesh object whose origin is the pivot; placed at the world origin for export."""
        bmesh.ops.translate(self.bm, verts=self.bm.verts, vec=-Vector(pivot))
        bmesh.ops.recalc_face_normals(self.bm, faces=self.bm.faces)
        mesh = bpy.data.meshes.new(self.name)
        self.bm.to_mesh(mesh)
        self.bm.free()
        for m in self.mats:
            mesh.materials.append(mats[m])
        obj = bpy.data.objects.new(self.name, mesh)
        bpy.context.scene.collection.objects.link(obj)
        return obj


# ----------------------------------------------------------------------------------- the orc

def build_torso():
    parts = {}
    z = lambda x, y, zz: Vector((x, y, zz))

    p = Part("SM_Orc_Pelvis")
    p.ellipsoid((0, 0.015, PELVIS_Z + 0.02), (0.26, 0.18, 0.13), "M_Orc_Leather")      # hips / trousers
    for s in (-1, 1):
        p.ball((s * HIP_X, 0, HIP_Z), 0.125, "M_Orc_Leather")                          # hip sockets
    belt = Matrix.Diagonal((1.0, 0.75, 1.0, 1.0))
    r = bmesh.ops.create_cone(p.bm, cap_ends=True, cap_tris=False, segments=16, radius1=0.262,
                              radius2=0.262, depth=0.08,
                              matrix=Matrix.Translation((0, -0.012, WAIST_Z - 0.015)) @ belt)
    p._tag(r["verts"], "M_Orc_LeatherDark")                                       # belt hides the waist
    p.box((0, -0.205, WAIST_Z - 0.015), (0.11, 0.03, 0.085), "M_Orc_Iron")           # buckle
    p.box((0, -0.222, WAIST_Z - 0.015), (0.04, 0.01, 0.055), "M_Orc_Bone")
    parts["Pelvis"] = p

    p = Part("SM_Orc_Spine")
    p.ellipsoid((0, -0.045, WAIST_Z + 0.10), (0.27, 0.22, 0.165), "M_Orc_Skin")       # gut
    p.ellipsoid((0, 0, WAIST_Z), (0.235, 0.17, 0.07), "M_Orc_Skin")                  # waist ball (in belt)
    p.ellipsoid((0, 0, CHEST_Z), (0.25, 0.18, 0.07), "M_Orc_Skin")                   # tucks into chest
    parts["Spine"] = p

    p = Part("SM_Orc_Chest")
    sx, sz = SHOULDER
    p.ellipsoid((0, 0.0, 1.46), (0.33, 0.23, 0.245), "M_Orc_Skin", u=14, v=10)        # rib cage
    for s in (-1, 1):
        p.ellipsoid((s * 0.135, -0.14, 1.50), (0.14, 0.085, 0.10), "M_Orc_Skin")       # pecs
        p.ball((s * sx, 0, sz), 0.13, "M_Orc_Skin")                                   # shoulder sockets
    p.ellipsoid((0, 0.06, 1.61), (0.28, 0.17, 0.11), "M_Orc_SkinDark")               # hunched traps
    strap = Matrix.Rotation(math.radians(-38), 4, "Y")                               # harness strap
    p.box((0.0, -0.228, 1.44), (0.08, 0.03, 0.50), "M_Orc_LeatherDark", rot=strap)
    p.ball((0.0, -0.248, 1.44), 0.037, "M_Orc_Iron")
    parts["Chest"] = p

    p = Part("SM_Orc_Head")
    h = HEAD_C
    p.frustum(NECK + z(0, 0.05, -0.08), h + z(0, 0.0, -0.08), 0.125, 0.11, "M_Orc_Skin")  # thick neck
    p.ellipsoid(h, (0.135, 0.145, 0.135), "M_Orc_Skin", u=14, v=10)                  # cranium
    p.ellipsoid(h + z(0, -0.08, -0.095), (0.13, 0.10, 0.075), "M_Orc_Skin")           # underbite jaw
    p.ellipsoid(h + z(0, -0.115, 0.035), (0.12, 0.047, 0.034), "M_Orc_SkinDark")      # brow ridge
    p.ellipsoid(h + z(0, -0.145, -0.015), (0.037, 0.032, 0.032), "M_Orc_SkinDark")    # nose
    p.box(h + z(0, -0.165, -0.07), (0.105, 0.02, 0.013), "M_Orc_Hair")               # mouth line
    for s in (-1, 1):
        p.ball(h + z(s * 0.054, -0.132, 0.005), 0.02, "M_Orc_Eye", u=8, v=6)        # eyes
        p.spike(h + z(s * 0.068, -0.155, -0.085), h + z(s * 0.082, -0.165, 0.012), 0.022,
                "M_Orc_Bone")                                                      # tusks
        p.frustum(h + z(s * 0.115, 0.015, 0.01), h + z(s * 0.25, 0.075, 0.08), 0.047, 0.004,
                  "M_Orc_Skin", segs=6, squash=0.35)                                # ears
    p.ellipsoid(h + z(0, 0.065, 0.125), (0.047, 0.09, 0.057), "M_Orc_Hair")          # top knot
    p.frustum(h + z(0, 0.105, 0.135), h + z(0, 0.20, -0.01), 0.032, 0.012, "M_Orc_Hair", segs=6)
    parts["Head"] = p
    return parts


def build_limbs():
    parts = {}
    for s in (-1, 1):
        n = side_name(s)
        d = arm_dir(s)
        sh, el, wr = P(f"UpperArm_{n}"), P(f"Forearm_{n}"), P(f"Hand_{n}")
        fist = P(f"Slot_Weapon_{n}")

        p = Part(f"SM_Orc_UpperArm_{n}")
        p.limb(sh, el, 0.115, 0.088, "M_Orc_Skin", ball_a=0.12, ball_b=0.09)
        p.ellipsoid(sh + d * 0.17 + Vector((0, -0.03, 0)), (0.10, 0.09, 0.13), "M_Orc_Skin",
                    rot=align_z(d))                                                # biceps
        if s > 0:  # left: big iron pauldron with spikes
            top = sh + Vector((0.035, 0, 0.065))
            p.ellipsoid(top, (0.175, 0.165, 0.11), "M_Orc_Iron", u=12, v=8)
            for k, (dx, dy) in enumerate(((0.0, -0.06), (0.06, 0.03), (-0.04, 0.06))):
                base = top + Vector((dx, dy, 0.07))
                p.spike(base, base + Vector((dx * 0.6 + 0.02, dy * 0.6, 0.11)), 0.028, "M_Orc_Iron")
            p.frustum(sh + d * 0.13, sh + d * 0.16, 0.12, 0.117, "M_Orc_LeatherDark", segs=12)
        else:      # right: a leather arm band
            p.frustum(sh + d * 0.23, sh + d * 0.27, 0.11, 0.107, "M_Orc_LeatherDark", segs=12)
        parts[f"UpperArm_{n}"] = p

        p = Part(f"SM_Orc_Forearm_{n}")
        p.limb(el, wr, 0.10, 0.07, "M_Orc_Skin", ball_a=0.085, ball_b=0.068)
        p.frustum(wr - d * 0.18, wr - d * 0.02, 0.092, 0.077, "M_Orc_Leather", segs=10)  # bracer
        p.frustum(wr - d * 0.16, wr - d * 0.14, 0.097, 0.095, "M_Orc_Iron", segs=10)
        parts[f"Forearm_{n}"] = p

        p = Part(f"SM_Orc_Hand_{n}")
        p.ball(wr, 0.066, "M_Orc_Skin")                                            # wrist ball
        p.ellipsoid(fist, (0.066, 0.075, 0.075), "M_Orc_Skin", u=8, v=6)             # fist
        p.ellipsoid(fist + Vector((-s * 0.036, -0.04, -0.012)), (0.036, 0.042, 0.06),
                    "M_Orc_SkinDark", u=8, v=6)                                     # curled fingers
        p.ellipsoid(fist + Vector((-s * 0.024, -0.066, 0.036)), (0.026, 0.036, 0.026),
                    "M_Orc_Skin", u=8, v=6)                                         # thumb
        parts[f"Hand_{n}"] = p

        hip, knee, ankle = P(f"Thigh_{n}"), P(f"Shin_{n}"), P(f"Foot_{n}")
        p = Part(f"SM_Orc_Thigh_{n}")
        p.ball(hip, 0.117, "M_Orc_Leather")   # a little smaller than the pelvis socket: no z-fighting
        p.frustum(hip, hip + Vector((0, 0, -0.20)), 0.135, 0.12, "M_Orc_Leather")      # trousers
        p.frustum(hip + Vector((0, 0, -0.18)), knee, 0.112, 0.09, "M_Orc_Skin")
        p.frustum(hip + Vector((0, 0, -0.21)), hip + Vector((0, 0, -0.18)), 0.124, 0.124,
                  "M_Orc_LeatherDark", segs=12)                                    # rolled hem
        p.ball(knee, 0.095, "M_Orc_Skin")
        parts[f"Thigh_{n}"] = p

        p = Part(f"SM_Orc_Shin_{n}")
        p.ball(knee, 0.09, "M_Orc_Skin")
        p.frustum(knee, ankle, 0.095, 0.068, "M_Orc_Skin")
        p.ellipsoid(knee + Vector((0, 0.035, -0.14)), (0.092, 0.082, 0.12), "M_Orc_Skin")  # calf
        p.ellipsoid(knee + Vector((0, -0.065, 0.0)), (0.08, 0.045, 0.085), "M_Orc_Leather")  # knee pad
        p.frustum(ankle + Vector((0, 0, 0.0)), ankle + Vector((0, 0, 0.14)), 0.08, 0.082,
                  "M_Orc_LeatherDark", segs=10)                                    # foot wraps
        p.ball(ankle, 0.074, "M_Orc_LeatherDark")
        parts[f"Shin_{n}"] = p

        p = Part(f"SM_Orc_Foot_{n}")
        p.ball(ankle, 0.07, "M_Orc_LeatherDark")
        p.box((ankle.x, -0.05, 0.05), (0.17, 0.27, 0.10), "M_Orc_Skin")           # foot block
        p.ellipsoid((ankle.x, 0.045, 0.055), (0.082, 0.075, 0.055), "M_Orc_Skin")        # heel
        for k in range(3):
            tx = ankle.x + (k - 1) * 0.052
            p.ellipsoid((tx, -0.2, 0.035), (0.03, 0.045, 0.035), "M_Orc_Skin", u=8, v=6)  # toes
            p.ellipsoid((tx, -0.243, 0.028), (0.014, 0.012, 0.012), "M_Orc_Bone", u=6, v=4)  # claws
        p.box((ankle.x, -0.02, 0.078), (0.178, 0.055, 0.055), "M_Orc_LeatherDark")      # strap
        parts[f"Foot_{n}"] = p
    return parts


# ----------------------------------------------------------------------------------- the club

CLUB_LENGTH = 2.0
CLUB = {  # Blender coordinates relative to the grip; the club points along -Y (Unity +Z)
    "pommel_end": Vector((0, 0.14, 0)),
    "sweet_spot": Vector((0, -1.50, 0)),   # middle of the head, 1.5 m from the grip
    "tip": Vector((0, -1.86, 0)),
}


def build_club():
    p = Part("SM_Club_Big")
    y = lambda v: Vector((0, v, 0))
    p.ellipsoid(y(0.115), (0.052, 0.04, 0.052), "M_Orc_Iron")                        # pommel
    p.frustum(y(0.10), y(-0.13), 0.038, 0.04, "M_Orc_LeatherDark", segs=10)          # grip wrap
    p.frustum(y(-0.12), y(-1.06), 0.042, 0.08, "M_Club_Wood", segs=10)               # shaft
    p.frustum(y(-1.0), y(-1.72), 0.075, 0.148, "M_Club_Wood", segs=12)               # head
    p.ellipsoid(y(-1.72), (0.148, 0.14, 0.148), "M_Club_Wood", u=12, v=8)           # rounded end
    for yy, rr in ((-1.18, 0.098), (-1.48, 0.128)):
        p.frustum(y(yy + 0.025), y(yy - 0.025), rr, rr + 0.004, "M_Orc_Iron", segs=12)  # iron bands
    for ring, yy, rr, count in ((0, -1.33, 0.112, 5), (1, -1.62, 0.14, 7)):
        for k in range(count):
            a = 2 * math.pi * (k + 0.5 * ring) / count
            out = Vector((math.cos(a), 0, math.sin(a)))
            base = y(yy) + out * (rr - 0.01)
            p.spike(base, base + out * 0.075, 0.024, "M_Orc_Iron")                  # studs
    p.frustum(y(-0.75), y(-0.80), 0.066, 0.068, "M_Club_WoodDark", segs=10)          # notch ring
    return p


# ----------------------------------------------------------------------------------- colliders

def capsule_between(a, b, radius, gap_a, gap_b):
    """Capsule on the bone a->b whose ROUNDED ENDS stop gap_a from a and gap_b from b."""
    a, b = Vector(a), Vector(b)
    d = (b - a).normalized()
    return {"type": "capsule", "start": a + d * (gap_a + radius), "end": b - d * (gap_b + radius),
            "radius": radius}


def box(centre, size):
    return {"type": "box", "center": Vector(centre), "size": Vector(size)}


def sphere(centre, radius):
    return {"type": "sphere", "center": Vector(centre), "radius": radius}


def colliders():
    """World-space colliders (Blender axes), derived from the joint pivots.  Each stops short of
    the joints: the ball-joint zones are covered by mesh only.  check_colliders.py verifies that
    no two colliders of different bodies come closer than 2 cm."""
    gap = 0.025
    pelvis_lo, pelvis_hi = HIP_Z - 0.03, WAIST_Z - 0.04          # hip pivots sit inside the box
    spine_lo, spine_hi = pelvis_hi + gap, CHEST_Z - 0.01
    head_r = 0.15
    chest_lo, chest_hi = spine_hi + gap, HEAD_C.z - head_r - gap
    c = {"Pelvis": box((0, 0.01, (pelvis_lo + pelvis_hi) / 2), (0.44, 0.32, pelvis_hi - pelvis_lo)),
         "Spine": box((0, -0.03, (spine_lo + spine_hi) / 2), (0.48, 0.40, spine_hi - spine_lo)),
         "Chest": box((0, 0.0, (chest_lo + chest_hi) / 2), (0.60, 0.42, chest_hi - chest_lo)),
         "Head": sphere(HEAD_C, head_r)}
    for s in (-1, 1):
        n = side_name(s)
        sh, el, wr = P(f"UpperArm_{n}"), P(f"Forearm_{n}"), P(f"Hand_{n}")
        c[f"UpperArm_{n}"] = capsule_between(sh, el, 0.085, 0.07, 0.03)
        c[f"Forearm_{n}"] = capsule_between(el, wr, 0.075, 0.03, 0.03)
        c[f"Hand_{n}"] = box(P(f"Slot_Weapon_{n}"), (0.13, 0.15, 0.14))
        hip, knee, ankle = P(f"Thigh_{n}"), P(f"Shin_{n}"), P(f"Foot_{n}")
        # thigh top stays below the pelvis box; shin bottom clears the foot box
        c[f"Thigh_{n}"] = capsule_between(hip, knee, 0.11, HIP_Z - (pelvis_lo - gap), 0.03)
        c[f"Shin_{n}"] = capsule_between(knee, ankle, 0.085, 0.03, 0.05)
        c[f"Foot_{n}"] = box((ankle.x, -0.085, 0.0475), (0.18, 0.34, 0.095))  # sole exactly on z = 0
    return c


def club_colliders():
    """Grip-relative colliders (Blender axes). They join Hand_R's rigidbody as a compound."""
    return [capsule_between((0, -0.08, 0), (0, -1.06, 0), 0.055, 0.0, 0.0),
            capsule_between((0, -1.08, 0), (0, -1.86, 0), 0.142, 0.0, 0.0)]


def offset(col, delta):
    out = dict(col)
    for key in ("start", "end", "center"):
        if key in out:
            out[key] = out[key] + delta
    return out


# ----------------------------------------------------------------------------------- Unity data

def to_unity(v):
    """Blender (right-handed, Z up, faces -Y) -> Unity (left-handed, Y up, faces +Z)."""
    return [round(-v.x, 4) + 0.0, round(v.z, 4) + 0.0, round(-v.y, 4) + 0.0]  # + 0.0 drops "-0.0"


def collider_json(col, pivot):
    if col["type"] == "box":
        return {"type": "box", "center": to_unity(col["center"] - pivot),
                "size": [round(col["size"].x, 4), round(col["size"].z, 4), round(col["size"].y, 4)]}
    if col["type"] == "sphere":
        return {"type": "sphere", "center": to_unity(col["center"] - pivot),
                "radius": col["radius"]}
    length = (col["end"] - col["start"]).length
    return {"type": "capsule", "start": to_unity(col["start"] - pivot),
            "end": to_unity(col["end"] - pivot), "radius": col["radius"],
            "height": round(length + 2 * col["radius"], 4)}


def mass_of(part):
    key = part.split("_")[0]
    return round(TOTAL_MASS * MASS_PCT[key] / 100.0, 2)


def write_json(path, cols):
    nulls = []
    for name, (parent, pos) in NULLS.items():
        local = pos - (NULLS[parent][1] if parent else Vector())
        nulls.append({"name": name, "parent": parent, "localPosition": to_unity(local)})
    parts = [{"name": part, "mesh": f"SM_Orc_{part}", "massKg": mass_of(part),
              "collider": collider_json(cols[part], P(part))} for part in BODY_PARTS]
    data = {
        "about": "Placeholder orc rig. Unity axes: Y up, +Z forward, +X is the character's right. "
                 "Metres. Every null has identity rotation in the rest pose; positions are local "
                 "to the parent null. Capsules are given as the two centres of their rounded ends "
                 "(start/end) plus radius; build each on a child object aligned to start->end. "
                 "Colliders stop short of every joint, so no two touch at rest.",
        "heightM": 2.0,
        "totalMassKg": TOTAL_MASS,
        "nulls": nulls,
        "parts": parts,
        "materials": [{"name": n, "color": [round(c, 4) for c in rgb]} for n, rgb in COLOURS.items()],
        "weapon": {
            "name": "Club_Big", "mesh": "SM_Club_Big", "slot": "Slot_Weapon_R",
            "attachTo": "Hand_R",
            "note": "Origin = grip, at the slot with identity rotation. The club points along "
                    "the slot's +Z. Colliders are grip-relative and join Hand_R's rigidbody.",
            "lengthM": CLUB_LENGTH, "physicsMassKg": 6.0, "gameplayMassKg": 40.0,
            "markers": {k: to_unity(v) for k, v in CLUB.items()},
            "colliders": [collider_json(c, Vector()) for c in club_colliders()],
        },
    }
    with open(path, "w") as f:
        json.dump(data, f, indent=2)


# ----------------------------------------------------------------------------------- export

def export_fbx(path, objects):
    bpy.ops.object.select_all(action="DESELECT")
    for o in objects:
        o.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, object_types={"MESH"},
                             apply_unit_scale=True, apply_scale_options="FBX_SCALE_ALL",
                             axis_forward="-Z", axis_up="Y", bake_space_transform=True,
                             use_mesh_modifiers=True, mesh_smooth_type="FACE",
                             add_leaf_bones=False, bake_anim=False)


def collider_mesh(name, col, mat):
    """A preview mesh of a collider, in world space."""
    bm = bmesh.new()
    if col["type"] == "box":
        bmesh.ops.create_cube(bm, size=1.0, matrix=Matrix.Translation(col["center"]) @
                              Matrix.Diagonal((*col["size"], 1)))
    elif col["type"] == "sphere":
        bmesh.ops.create_uvsphere(bm, u_segments=16, v_segments=10, radius=col["radius"],
                                  matrix=Matrix.Translation(col["center"]))
    else:
        a, b, r = col["start"], col["end"], col["radius"]
        m = Matrix.Translation((a + b) / 2) @ align_z(b - a)
        bmesh.ops.create_cone(bm, cap_ends=False, segments=16, radius1=r, radius2=r,
                              depth=(b - a).length, matrix=m)
        for p in (a, b):
            bmesh.ops.create_uvsphere(bm, u_segments=16, v_segments=10, radius=r,
                                      matrix=Matrix.Translation(p))
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    mesh.materials.append(mat)
    return bpy.data.objects.new(name, mesh)


def main(out):
    os.makedirs(os.path.join(out, "FBX"), exist_ok=True)
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0
    mats = make_materials()

    recipes = {**build_torso(), **build_limbs()}
    objects = {}
    for part in BODY_PARTS:
        objects[part] = recipes[part].to_object(mats, P(part))
    club = build_club().to_object(mats, Vector())

    # 1. one FBX per part, each at the origin (its pivot)
    for part, obj in objects.items():
        export_fbx(os.path.join(out, "FBX", f"SM_Orc_{part}.fbx"), [obj])
    export_fbx(os.path.join(out, "FBX", "SM_Club_Big.fbx"), [club])

    # 2. assembled reference: every part moved to its pivot, flat
    for part, obj in objects.items():
        obj.location = P(part)
    club.location = P("Slot_Weapon_R")
    export_fbx(os.path.join(out, "FBX", "REF_Orc_Assembled.fbx"), list(objects.values()) + [club])

    # 3. the .blend: null hierarchy, meshes under their nulls at zero offset
    rig = bpy.data.collections.new("Orc_Rig")
    scene.collection.children.link(rig)
    empties = {}
    for name, (parent, pos) in NULLS.items():
        e = bpy.data.objects.new(name, None)
        e.empty_display_type = "PLAIN_AXES" if name.startswith(("Slot", "Aim", "Root")) else "SPHERE"
        e.empty_display_size = 0.04
        rig.objects.link(e)
        if parent:
            e.parent = empties[parent]
            e.location = pos - NULLS[parent][1]
        else:
            e.location = pos
        empties[name] = e
    for part, obj in list(objects.items()) + [("Slot_Weapon_R", club)]:
        scene.collection.objects.unlink(obj)
        rig.objects.link(obj)
        obj.parent = empties[part]
        obj.location = (0, 0, 0)

    # 4. collider previews, parented to their parts, in a hidden collection
    cols = colliders()
    col_coll = bpy.data.collections.new("Colliders")
    scene.collection.children.link(col_coll)
    cmat = bpy.data.materials.new("M_Collider")
    cmat.diffuse_color = (1.0, 0.2, 0.6, 0.6)
    grip = P("Slot_Weapon_R")
    previews = [(f"COL_{p}", cols[p], p) for p in BODY_PARTS]
    previews += [(f"COL_Club_{i}", offset(c, grip), "Slot_Weapon_R")
                 for i, c in enumerate(club_colliders())]
    for name, col, owner in previews:
        o = collider_mesh(name, col, cmat)
        col_coll.objects.link(o)
        o.display_type = "WIRE"
        bpy.context.view_layer.update()
        o.parent = empties[owner]
        o.matrix_parent_inverse = empties[owner].matrix_world.inverted()
    col_coll.hide_render = True

    write_json(os.path.join(out, "orc_rig.json"), cols)
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(out, "Orc_Placeholder.blend"))

    tris = {o.name: sum(len(p.vertices) - 2 for p in o.data.polygons)
            for o in list(objects.values()) + [club]}
    print("TRIS", json.dumps(tris), "TOTAL", sum(tris.values()))


if __name__ == "__main__":
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else sys.argv[1:]
    main(os.path.abspath(argv[0] if argv else "out"))
