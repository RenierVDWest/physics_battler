"""Builds the small, medium and large base bodies and writes everything Unity needs.

Run:  python3 build_bodies.py <output folder>      (Blender 5.x as a Python module)

Outputs (in the output folder):
  Bodies.blend                       the three null rigs side by side, meshes under their nulls,
                                     collider previews in a hidden collection per body
  FBX/<Body>/SM_<Body>_<Part>.fbx    one file per body part, pivot at the origin, ready for Unity
  FBX/REF_<Body>_Assembled.fbx       every part in place (flat), for viewing in Softimage or Houdini
  <body>_rig.json                    nulls, masses, colliders, aim points (Unity axes)
"""

import json
import os
import sys

import bpy
from mathutils import Vector

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import bodies as B  # noqa: E402
import sculpt  # noqa: E402

SKIN = (0.62, 0.62, 0.64)
SPACING = {"Small": -1.0, "Medium": 0.0, "Large": 1.6}   # x offsets in the .blend (Blender -X = right)


def make_material():
    mat = bpy.data.materials.new("M_Body_Clay")
    mat.diffuse_color = (*SKIN, 1.0)
    mat.use_nodes = True
    bsdf = mat.node_tree.nodes.get("Principled BSDF")
    if bsdf:
        bsdf.inputs["Base Color"].default_value = (*SKIN, 1.0)
        bsdf.inputs["Roughness"].default_value = 0.7
    return mat


# Quad budget per part after remeshing (about 9,000 quads, 18,000 triangles a body).
QUADS = {"Pelvis": 900, "Spine": 700, "Chest": 1300, "Head": 1400, "UpperArm": 500, "Forearm": 350,
         "Hand": 300, "Thigh": 550, "Shin": 400, "Foot": 300}


def vec3(v):
    import numpy as np
    return np.array([v.x, v.y, v.z])


def make_part(name, verts, faces, mat, quads):
    """A mesh object from the marching-cubes surface, remeshed to clean quads (Quadriflow; a
    decimate if that fails) and smooth shaded."""
    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata([tuple(v) for v in verts.tolist()], [], [tuple(f) for f in faces.tolist()])
    mesh.validate()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.scene.collection.objects.link(obj)
    bpy.context.view_layer.objects.active = obj
    for other in bpy.context.view_layer.objects:
        other.select_set(other == obj)
    # Quadriflow is happier with a lighter mesh: decimate the marching-cubes surface first.
    dec = obj.modifiers.new("Pre", "DECIMATE")
    dec.ratio = min(1.0, 5 * quads / max(1, len(faces)))
    dg = bpy.context.evaluated_depsgraph_get()
    light = bpy.data.meshes.new_from_object(obj.evaluated_get(dg))
    obj.modifiers.clear()
    obj.data = light
    bpy.data.meshes.remove(mesh)
    faces_light = len(light.polygons)
    done = False
    try:
        done = bpy.ops.object.quadriflow_remesh(target_faces=quads, use_mesh_symmetry=False,
                                                use_preserve_sharp=False, smooth_normals=False) == {"FINISHED"}
        done = done and len(obj.data.polygons) > quads * 0.5
    except RuntimeError as error:
        print(f"[bodies] {name}: {error}")
        done = False
    if not done:
        print(f"[bodies] {name}: Quadriflow failed, decimating instead")
        dec = obj.modifiers.new("Decimate", "DECIMATE")
        dec.ratio = min(1.0, 2 * quads / max(1, faces_light))
        dg = bpy.context.evaluated_depsgraph_get()
        reduced = bpy.data.meshes.new_from_object(obj.evaluated_get(dg))
        obj.modifiers.clear()
        old = obj.data
        obj.data = reduced
        bpy.data.meshes.remove(old)
    obj.data.name = name
    obj.data.materials.clear()
    obj.data.materials.append(mat)
    for p in obj.data.polygons:
        p.use_smooth = True
    obj.select_set(False)
    return obj


def to_unity(v):
    return [round(-v.x, 4) + 0.0, round(v.z, 4) + 0.0, round(-v.y, 4) + 0.0]


def collider_json(col, pivot):
    if col["type"] == "box":
        return {"type": "box", "center": to_unity(col["center"] - pivot),
                "size": [round(col["size"].x, 4), round(col["size"].z, 4), round(col["size"].y, 4)]}
    if col["type"] == "sphere":
        return {"type": "sphere", "center": to_unity(col["center"] - pivot), "radius": round(col["radius"], 4)}
    length = (col["end"] - col["start"]).length
    return {"type": "capsule", "start": to_unity(col["start"] - pivot), "end": to_unity(col["end"] - pivot),
            "radius": round(col["radius"], 4), "height": round(length + 2 * col["radius"], 4)}


def write_json(path, body, cols):
    nulls = []
    for name, (parent, pos) in body.nulls.items():
        local = pos - (body.nulls[parent][1] if parent else Vector())
        nulls.append({"name": name, "parent": parent, "localPosition": to_unity(local)})
    name = body.spec.name
    parts = [{"name": part, "mesh": f"SM_{name}_{part}", "massKg": body.mass_of(part),
              "collider": collider_json(cols[part], body.P(part))} for part in B.BODY_PARTS]
    data = {
        "about": f"{name} base body. Unity axes: Y up, +Z forward, +X is the character's right. Metres. "
                 "Every null has identity rotation in the rest pose; positions are local to the parent "
                 "null. Aim_* nulls are points other units aim blows at. Capsules are given as the two "
                 "centres of their rounded ends plus radius. Colliders stop short of every joint.",
        "body": name,
        "heightM": round(body.height, 3),
        "totalMassKg": body.spec.mass,
        "nulls": nulls,
        "parts": parts,
        "materials": [{"name": "M_Body_Clay", "color": list(SKIN)}],
    }
    with open(path, "w") as f:
        json.dump(data, f, indent=2)


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
    import bmesh
    from mathutils import Matrix
    bm = bmesh.new()
    if col["type"] == "box":
        bmesh.ops.create_cube(bm, size=1.0, matrix=Matrix.Translation(col["center"]) @
                              Matrix.Diagonal((*col["size"], 1)))
    elif col["type"] == "sphere":
        bmesh.ops.create_uvsphere(bm, u_segments=16, v_segments=10, radius=col["radius"],
                                  matrix=Matrix.Translation(col["center"]))
    else:
        a, b, r = col["start"], col["end"], col["radius"]
        m = Matrix.Translation((a + b) / 2) @ B.align_z(b - a)
        bmesh.ops.create_cone(bm, cap_ends=False, segments=16, radius1=r, radius2=r,
                              depth=(b - a).length, matrix=m)
        for p in (a, b):
            bmesh.ops.create_uvsphere(bm, u_segments=16, v_segments=10, radius=r, matrix=Matrix.Translation(p))
    mesh = bpy.data.meshes.new(name)
    bm.to_mesh(mesh)
    bm.free()
    mesh.materials.append(mat)
    return bpy.data.objects.new(name, mesh)


def main(out):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    mat = make_material()
    cmat = bpy.data.materials.new("M_Collider")
    cmat.diffuse_color = (1.0, 0.2, 0.6, 0.6)
    report = {}

    for spec in B.SPECS:
        body = B.Body(spec)
        name = spec.name
        os.makedirs(os.path.join(out, "FBX", name), exist_ok=True)
        field = sculpt.sculpt(body)
        surfaces = sculpt.cut(body, field, B.BODY_PARTS)
        objects = {}
        for part in B.BODY_PARTS:
            verts, faces = surfaces[part]
            objects[part] = make_part(f"SM_{name}_{part}", verts - vec3(body.P(part)), faces, mat,
                                      QUADS[part.split("_")[0]])
        for part, obj in objects.items():
            export_fbx(os.path.join(out, "FBX", name, f"SM_{name}_{part}.fbx"), [obj])
        for part, obj in objects.items():
            obj.location = body.P(part)
        export_fbx(os.path.join(out, "FBX", f"REF_{name}_Assembled.fbx"), list(objects.values()))

        # null rig in the .blend, offset sideways so the three stand in a row
        rig = bpy.data.collections.new(f"{name}_Rig")
        scene.collection.children.link(rig)
        empties = {}
        for nname, (parent, pos) in body.nulls.items():
            e = bpy.data.objects.new(f"{name}:{nname}", None)
            e.empty_display_type = "PLAIN_AXES" if nname.startswith(("Slot", "Aim", "Root")) else "SPHERE"
            e.empty_display_size = 0.03 * spec.hs
            rig.objects.link(e)
            if parent:
                e.parent = empties[parent]
                e.location = pos - body.nulls[parent][1]
            else:
                e.location = pos + Vector((SPACING[name], 0, 0))
            empties[nname] = e
        for part, obj in objects.items():
            scene.collection.objects.unlink(obj)
            rig.objects.link(obj)
            obj.parent = empties[part]
            obj.location = (0, 0, 0)

        cols = sculpt.fit_colliders(body, surfaces)
        worst = sculpt.collider_clearance(body, cols, B.BODY_PARTS)
        report_cols = [f"{a}-{b} {d * 100:.1f}cm" for d, a, b in worst[:4]]
        col_coll = bpy.data.collections.new(f"{name}_Colliders")
        scene.collection.children.link(col_coll)
        bpy.context.view_layer.update()
        for part in B.BODY_PARTS:
            o = collider_mesh(f"COL_{name}_{part}", cols[part], cmat)
            col_coll.objects.link(o)
            o.display_type = "WIRE"
            o.parent = empties[part]
            o.location = -body.P(part)  # collider meshes are in body space; parent sits at the pivot
            o.location = o.location     # (no rotation in the rest pose)
        col_coll.hide_render = True

        write_json(os.path.join(out, f"{name.lower()}_rig.json"), body, cols)
        dg = bpy.context.evaluated_depsgraph_get()
        tris = 0
        for obj in objects.values():
            ev = obj.evaluated_get(dg)
            tris += sum(len(p.vertices) - 2 for p in ev.data.polygons)
        report[name] = {"height": round(body.height, 3), "mass": spec.mass, "tris": tris,
                        "shoulder_width": round(2 * body.shoulder[0], 3), "closest_colliders": report_cols}

    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(out, "Bodies.blend"))
    print("BODIES", json.dumps(report))


if __name__ == "__main__":
    argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else sys.argv[1:]
    main(os.path.abspath(argv[0] if argv else "out"))
