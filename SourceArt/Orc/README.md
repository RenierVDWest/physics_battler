# Placeholder orc + big club

2.0 m orc split into 15 rigid body parts, plus a 2.0 m club. Built by script in Blender 5.2.

## Files

| Path | What it is |
| --- | --- |
| `FBX/SM_Orc_<Part>.fbx` (in `Assets/_Battler/Art/Orc`) | One mesh per body part. Pivot at the origin = the joint. Drop each under the null of the same name, at zero offset. |
| `FBX/SM_Club_Big.fbx` | The club. Origin = the grip. Goes under `Slot_Weapon_R` at zero offset; points along the slot's +Z. |
| `FBX/REF_Orc_Assembled.fbx` | Every part in place, flat (no hierarchy), for viewing in Softimage or Houdini. |
| `orc_rig.json` | Null names, parents and local positions; per-part mass and collider; club markers and colliders. Unity axes. |
| `Orc_Placeholder.blend` | Null hierarchy with the meshes under it, and collider previews in a hidden `Colliders` collection. |
| `Renders/` | Turnaround, collider overlay, poses and joint close-ups. |
| `Scripts/` | `build_orc.py` rebuilds everything; `check_colliders.py` verifies collider clearance; `render_orc.py` makes the previews. |

In this repo the FBX files and `orc_rig.json` are copied into `Assets/_Battler/Art/Orc` and `Assets/_Battler/Data/Rigs`; rebuild with `python3 Scripts/build_orc.py out` and copy `out/FBX/SM_*.fbx` and `out/orc_rig.json` across, then run Physics Battler > Setup > Rebuild Prefabs.

## Conventions

- Unity: Y up, the orc faces +Z, its right side is +X. 1 unit = 1 m. Feet stand on y = 0.
- Every null has identity rotation in the rest pose. Arms hang 14° out from vertical.
- FBX export: Forward -Z, Up Y, Apply Transform on, FBX All scaling. In Unity, import at scale factor 1.
- Capsule colliders on the arms are tilted, and Unity's `CapsuleCollider` only runs along a local axis, so each capsule goes on a child object rotated to its `start -> end` direction.

## Rebuild

```
pip install bpy==5.2.3
python3 Scripts/build_orc.py out
python3 Scripts/check_colliders.py
python3 Scripts/render_orc.py out
```
