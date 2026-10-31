"""Builds the milestone 2 weapons (sword, club, round shield) in the mannequin style of the bodies,
and writes everything Unity needs.

Run:  python3 build_weapons.py <output folder>      (Blender 5.x as a Python module)
Outputs (in the output folder):
  Weapons.blend              the three weapons side by side
  FBX/SM_<Weapon>.fbx        one file per weapon, pivot at the grip, ready for Unity
  weapons.json               lengths, masses, strike points and colliders (Unity axes)

Every weapon is built for the medium body (the units scale it with their size). Unity axes, as
the bodies: the grip (where the hand's weapon slot sits) is at the origin. A sword or club points
along +Z from the grip; a sword's edges face +Y and -Y, its flats +X and -X. A shield's face
looks along +Z, with the hand on its grip just behind the middle.
"""

import json
import math
import os
import sys

import bpy
import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(HERE, "..", "..", "Bodies", "Scripts"))
import simple  # noqa: E402  (the lathe kit)

STEEL = (0.74, 0.76, 0.80)
WOOD = (0.55, 0.40, 0.27)
DARK = (0.30, 0.28, 0.27)    # grips, bindings: the bodies' joint colour, a touch darker
MATERIALS = [("M_Weapon_Steel", STEEL, 0.25), ("M_Weapon_Wood", WOOD, 0.6), ("M_Weapon_Dark", DARK, 0.5)]
STEEL_I, WOOD_I, DARK_I = 0, 1, 2

UX, UY, UZ = np.array([1.0, 0, 0]), np.array([0, 1.0, 0]), np.array([0, 0, 1.0])


class Piece:
    """Verts and faces in Unity axes; turned into Blender axes (a mirror, so faces are reversed)
    and checked to point outward when added to a mesh."""

    def __init__(self):
        self.parts = []   # (verts, faces, material)

    def add_shells(self, shells, material):
        verts, faces, _ = shells.result()
        self.parts.append((np.asarray(verts, dtype=np.float64), [tuple(f) for f in faces], material))

    def add(self, verts, faces, material):
        self.parts.append((np.asarray(verts, dtype=np.float64), [tuple(f) for f in faces], material))


def to_blender(v):
    return np.stack([-v[:, 0], -v[:, 2], v[:, 1]], axis=1)


def signed_volume(verts, faces):
    vol = 0.0
    for f in faces:
        a = verts[f[0]]
        for i in range(1, len(f) - 1):
            vol += np.dot(a, np.cross(verts[f[i]], verts[f[i + 1]])) / 6.0
    return vol


def make_object(name, piece, mats):
    all_v, all_f, all_m = [], [], []
    for verts, faces, material in piece.parts:
        bv = to_blender(verts)
        if signed_volume(bv, faces) < 0:
            faces = [tuple(reversed(f)) for f in faces]
        base = sum(len(v) for v in all_v)
        all_v.append(bv)
        all_f += [tuple(i + base for i in f) for f in faces]
        all_m += [material] * len(faces)
    verts = np.concatenate(all_v)
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata([tuple(v) for v in verts.tolist()], [], all_f)
    mesh.validate()
    for m in mats:
        mesh.materials.append(m)
    for poly, mi in zip(mesh.polygons, all_m):
        poly.material_index = mi
        poly.use_smooth = True
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.scene.collection.objects.link(obj)
    return obj


def ring_solid(rings):
    """A closed solid through rings of points (each ring the same count, or a single point at
    either end)."""
    verts, faces, idx = [], [], []
    for ring in rings:
        start = len(verts)
        verts += [np.asarray(p, dtype=np.float64) for p in ring]
        idx.append(list(range(start, len(verts))))
    n = max(len(r) for r in idx)
    for r0, r1 in zip(idx[:-1], idx[1:]):
        if len(r0) == 1:
            faces += [(r0[0], r1[i], r1[(i + 1) % n]) for i in range(n)]
        elif len(r1) == 1:
            faces += [(r0[i], r1[0], r0[(i + 1) % n]) for i in range(n)]
        else:
            faces += [(r0[i], r1[i], r1[(i + 1) % n], r0[(i + 1) % n]) for i in range(n)]
    # The side quads run the first ring backwards and the last forwards; the caps run the other way.
    if len(idx[0]) > 1:
        faces.append(tuple(idx[0]))
    if len(idx[-1]) > 1:
        faces.append(tuple(reversed(idx[-1])))
    return verts, faces


# ---- The weapons (Unity axes, metres) --------------------------------------------------------------

SWORD = dict(grip_back=-0.075, guard_z=0.075, blade_start=0.085, tip=0.86, width=0.06, thick=0.01)
CLUB = dict(back=-0.11, length=0.72, handle_r=0.019, head_r=0.058)
SHIELD = dict(radius=0.29, depth=0.05, rim=0.018, centre_z=0.06)


def lens(width, thick, z):
    """The blade's cross-section at z: a flat hexagon, edges on +-Y, flats on +-X."""
    w, t = width * 0.5, thick * 0.5
    return [(0.0, w, z), (t, 0.45 * w, z), (t, -0.45 * w, z), (0.0, -w, z), (-t, -0.45 * w, z), (-t, 0.45 * w, z)]


def build_sword():
    s = SWORD
    p = Piece()
    # Blade: straight with a slight taper, then a long point.
    rings = []
    zs = np.linspace(s["blade_start"], s["tip"] - 0.11, 8)
    for z in zs:
        f = (z - s["blade_start"]) / (s["tip"] - s["blade_start"])
        rings.append(lens(s["width"] * (1.0 - 0.18 * f), s["thick"] * (1.0 - 0.3 * f), z))
    for k, z in enumerate(np.linspace(s["tip"] - 0.11, s["tip"] - 0.008, 5)[1:]):
        f = (k + 1) / 5
        rings.append(lens(s["width"] * 0.82 * (1 - f) ** 0.8, s["thick"] * 0.7 * (1 - 0.6 * f), z))
    rings.append([(0.0, 0.0, s["tip"])])
    v, f = ring_solid(rings)
    p.add(v, f, STEEL_I)
    # Cross-guard: a bar across the edges, rounded.
    guard = simple.Shells()
    guard.capsule(np.array([0, -0.085, s["guard_z"]]), np.array([0, 0.085, s["guard_z"]]),
                  [(0, 0.011), (0.5, 0.014), (1, 0.011)], 0, n=16, ref=UZ)
    p.add_shells(guard, STEEL_I)
    # Grip, wrapped.
    grip = simple.Shells()
    grip.capsule(np.array([0, 0, s["grip_back"]]), np.array([0, 0, s["guard_z"] - 0.008]),
                 [(0, 0.0145), (0.5, 0.0165), (1, 0.0145)], 0, n=18, ref=UY)
    p.add_shells(grip, DARK_I)
    # Pommel.
    pommel = simple.Shells()
    pommel.sphere(np.array([0, 0, s["grip_back"] - 0.012]), 0.024, 0, n=18, squash=(1.0, 1.0, 0.8), axis=UZ)
    p.add_shells(pommel, STEEL_I)
    info = {
        "name": "Sword", "kind": "Sword", "lengthM": round(s["tip"] - s["grip_back"] + 0.036, 3), "massKg": 1.3,
        "points": {"tip": [0, 0, s["tip"]], "edge": [0, 0, round(s["blade_start"] + 0.62 * (s["tip"] - s["blade_start"]), 3)],
                   "head": [0, 0, round(s["blade_start"] + 0.62 * (s["tip"] - s["blade_start"]), 3)]},
        "colliders": [{"type": "box", "center": [0, 0, round((s["blade_start"] + s["tip"]) / 2, 3)],
                       "size": [0.02, round(s["width"], 3), round(s["tip"] - s["blade_start"], 3)]},
                      {"type": "box", "center": [0, 0, s["guard_z"]], "size": [0.03, 0.19, 0.03]}],
    }
    return p, info


def build_club():
    c = CLUB
    p = Piece()
    shaft = simple.Shells()
    L = c["length"] - c["back"]
    prof = [(0.0, c["handle_r"] * 1.2), (0.06 / L, c["handle_r"]), (0.32 / L, c["handle_r"] * 1.05),
            (0.55 / L, c["head_r"] * 0.65), (0.72 / L, c["head_r"]), (0.84 / L, c["head_r"] * 0.98), (1.0, c["head_r"] * 0.78)]
    shaft.capsule(np.array([0, 0, c["back"]]), np.array([0, 0, c["length"]]), prof, 0, n=22, ref=UY)
    p.add_shells(shaft, WOOD_I)
    # Bindings: a dark wrap on the grip and two bands.
    wrap = simple.Shells()
    wrap.capsule(np.array([0, 0, c["back"] + 0.025]), np.array([0, 0, 0.13]),
                 [(0, c["handle_r"] * 1.12), (1, c["handle_r"] * 1.14)], 0, n=22, ref=UY)
    p.add_shells(wrap, DARK_I)
    for z, r in ((0.40, c["head_r"] * 0.86), (0.66, c["head_r"] * 1.03)):
        band = simple.Shells()
        band.capsule(np.array([0, 0, z - 0.011]), np.array([0, 0, z + 0.011]), [(0, r), (1, r)], 0, n=22, ref=UY)
        p.add_shells(band, DARK_I)
    head_z = round(c["back"] + 0.84 * L, 3)   # the fat of the head
    info = {
        "name": "Club", "kind": "Club", "lengthM": round(L, 3), "massKg": 2.0,
        "points": {"tip": [0, 0, c["length"]], "edge": [0, 0, head_z], "head": [0, 0, head_z]},
        "colliders": [{"type": "capsule", "start": [0, 0, 0.35], "end": [0, 0, round(c["length"] - c["head_r"], 3)],
                       "radius": round(c["head_r"], 3)},
                      {"type": "capsule", "start": [0, 0, c["back"] + 0.02], "end": [0, 0, 0.35], "radius": 0.025}],
    }
    return p, info


def build_shield():
    s = SHIELD
    p = Piece()
    face = simple.Shells()
    R, d, cz = s["radius"], s["depth"], s["centre_z"]
    # A shallow dome: the back is flat-ish, the face bulges forward (+Z); the rim is rounded.
    prof = [(0.0, 0.0), (0.004, R * 0.35), (0.008, R * 0.7), (0.012, R * 0.93), (0.018, R), (0.03, R * 0.995),
            (0.036, R * 0.93), (0.042, R * 0.7), (0.047, R * 0.35), (d * 0.98, 0.0)]
    face.lathe(np.array([0, 0, cz - 0.012]), UZ, prof, 0, n=40, ref=UY)
    p.add_shells(face, WOOD_I)
    rim = simple.Shells()
    # The rim: a torus of short capsules around the edge.
    segs = 32
    for k in range(segs):
        a0, a1 = 2 * math.pi * k / segs, 2 * math.pi * (k + 1.15) / segs
        q0 = np.array([math.sin(a0) * R * 0.99, math.cos(a0) * R * 0.99, cz + 0.006])
        q1 = np.array([math.sin(a1) * R * 0.99, math.cos(a1) * R * 0.99, cz + 0.006])
        rim.capsule(q0, q1, [(0, s["rim"]), (1, s["rim"])], 0, n=10, ref=UZ)
    p.add_shells(rim, DARK_I)
    boss = simple.Shells()
    boss.sphere(np.array([0, 0, cz + 0.035]), 0.075, 0, n=24, squash=(1.0, 1.0, 0.5), axis=UZ)
    p.add_shells(boss, STEEL_I)
    # The grip the hand holds, behind the boss.
    grip = simple.Shells()
    grip.capsule(np.array([0, -0.06, 0.0]), np.array([0, 0.06, 0.0]), [(0, 0.014), (0.5, 0.016), (1, 0.014)], 0, n=14, ref=UZ)
    p.add_shells(grip, DARK_I)
    for y in (-0.07, 0.07):
        post = simple.Shells()
        post.capsule(np.array([0, y, -0.004]), np.array([0, y, cz - 0.01]), [(0, 0.011), (1, 0.011)], 0, n=12, ref=UY)
        p.add_shells(post, DARK_I)
    info = {
        "name": "Shield", "kind": "Shield", "lengthM": round(2 * R, 3), "massKg": 4.0,
        "points": {"tip": [0, R, cz], "edge": [0, R, cz], "head": [0, 0, cz + 0.05]},
        "radius": R, "faceCenter": [0, 0, round(cz + 0.04, 3)],
        "colliders": [{"type": "box", "center": [0, 0, round(cz + 0.015, 3)], "size": [round(1.8 * R, 3), round(1.8 * R, 3), 0.05]}],
    }
    return p, info


def materials():
    out = []
    for name, rgb, rough in MATERIALS:
        m = bpy.data.materials.new(name)
        m.diffuse_color = (*rgb, 1.0)
        m.use_nodes = True
        bsdf = m.node_tree.nodes.get("Principled BSDF")
        if bsdf:
            bsdf.inputs["Base Color"].default_value = (*rgb, 1.0)
            bsdf.inputs["Roughness"].default_value = rough
            bsdf.inputs["Metallic"].default_value = 0.6 if name.endswith("Steel") else 0.0
        out.append(m)
    return out


def export_fbx(path, obj):
    bpy.ops.object.select_all(action="DESELECT")
    obj.select_set(True)
    bpy.context.view_layer.objects.active = obj
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, object_types={"MESH"},
                             apply_unit_scale=True, apply_scale_options="FBX_SCALE_ALL",
                             axis_forward="-Z", axis_up="Y", bake_space_transform=True,
                             use_mesh_modifiers=True, mesh_smooth_type="FACE",
                             add_leaf_bones=False, bake_anim=False)


def main(out):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    os.makedirs(os.path.join(out, "FBX"), exist_ok=True)
    mats = materials()
    infos = []
    for k, build in enumerate((build_sword, build_club, build_shield)):
        piece, info = build()
        obj = make_object(f"SM_{info['name']}", piece, mats)
        export_fbx(os.path.join(out, "FBX", f"SM_{info['name']}.fbx"), obj)
        obj.location.x = -0.5 * k      # side by side in the .blend (Blender -X is Unity +X)
        infos.append(info)
    data = {
        "about": "Weapons for the medium body (units scale them with their size). Unity axes: the grip is at the "
                 "origin; swords and clubs point along +Z, a sword's edges face +-Y; a shield's face looks along +Z. "
                 "Points: tip (thrusts), edge (where a cut strikes), head (where a club strikes).",
        "materials": [{"name": n, "color": list(c)} for n, c, _ in MATERIALS],
        "weapons": infos,
    }
    with open(os.path.join(out, "weapons.json"), "w") as f:
        json.dump(data, f, indent=2)
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(out, "Weapons.blend"))
    print("WEAPONS", json.dumps({i["name"]: i["lengthM"] for i in infos}))


if __name__ == "__main__":
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else sys.argv[1:]
    main(os.path.abspath(argv[0] if argv else "out"))
