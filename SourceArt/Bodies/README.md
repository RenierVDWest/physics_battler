# Base bodies: small, medium, large

Three unthemed base bodies for the animation-first build, each split into 16 rigid body parts on the
same null rig as the orc. Built by script in Blender 5.2.

| Body | Height | Mass | Build |
| --- | --- | --- | --- |
| Small | 0.90 m | 15 kg | Half human size: a big head, big hands and feet, wiry limbs, a little belly |
| Medium | 1.83 m | 80 kg | Heroic human: broad shoulders, narrow waist |
| Large | 2.37 m | 280 kg | Taller and much broader: huge traps, shoulders and arms, thick legs, a small head for its size |

![Lineup](Renders/bodies_lineup.png)

## How they're made

1. **Sculpted as one surface.** Each body is a signed distance field built from muscle volumes
   (rib cage, pecs, lats, traps, deltoids, biceps, quads, calves...) blended smoothly together
   (`Scripts/sculpt.py`, `Scripts/anatomy.py`). Proportions per body are in `Scripts/bodies.py`
   (`SPECS`): one set of factors per region, so a new body type is a new line there.
2. **Cut into parts.** Each part is the body surface inside its region (trunk split at the waist,
   chest and neck; limbs split at their joints), plus a ball around each of its pivots, clipped to the
   body surface. At rest the parts meet along the original surface; when a joint bends, the balls
   fill the gap.
3. **Remeshed to quads** (Quadriflow), about 9,000 quads (18,000 triangles) a body.
4. **Colliders fitted** to the parts: boxes for the trunk, a sphere for the head, capsules for the
   limbs, boxes for hands and feet. Every pair of different parts is at least 2 cm apart at rest
   (scaled with the body).

## Files

| Path | What it is |
| --- | --- |
| `Assets/_Battler/Art/Bodies/<Body>/SM_<Body>_<Part>.fbx` | One mesh per part. Pivot at the origin = the joint. Drop each under the null of the same name, at zero offset. |
| `Assets/_Battler/Data/Rigs/<body>_rig.json` | Nulls (with `Aim_Head`, `Aim_Neck`, `Aim_Chest`, `Aim_Belly` points to strike at), masses, colliders. Unity axes. |
| `FBX/REF_<Body>_Assembled.fbx` | Every part in place (flat), for viewing in Softimage or Houdini. |
| `Bodies.blend` | The three null rigs side by side with the meshes under them, colliders in hidden collections. |
| `Renders/` | Lineup, each body with its topology, colliders, and a guard pose and joint stress test. |

## Conventions

Same as the orc: Unity Y up, facing +Z, the body's right is +X; 1 unit = 1 m; feet on y = 0. Every
null has identity rotation in the rest pose; arms hang 16° (small 18°, large 24°) out from vertical.
FBX: Forward -Z, Up Y, Apply Transform, FBX All scaling; import at scale factor 1.

## Rebuild

```
pip install bpy==5.2.3 scikit-image
python3 Scripts/build_bodies.py out      # meshes, rig files, .blend (about a minute)
python3 Scripts/render_bodies.py out     # Renders/
python3 Scripts/render_one.py out Medium wire   # one body close up, while tuning
```
