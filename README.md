# Physics Battler

An auto battler where fights play out physically: attacks are animated toward the real position of the body part they
aim at, and whatever the weapon or a flying body actually touches takes the hit.
Unity 6.6 (6000.6.7f1), URP, PC.

Design spec (animation-first combat): https://claude.ai/artifact/HJeFzW1bbvt7n4gsLSUoUT

The first prototype (active ragdolls with muscles and a balance assist) is kept on the branch
`backup/active-ragdoll-v1`. This branch starts over from the spec above.

## Current state: milestone 1, units on the board

An 8×8 board with three units a side: Small, Medium and Large. They stand in a staggered fighting stance facing the
nearest enemy, never still: each size has its own combat idle (sway, bob, weave, idle shuffles), feet stay planted
until the body has moved or turned far enough from them, then step one at a time. Legs are animation-driven
(kinematic) so they always carry the body; torso, head and arms are physical parts pinned to the pose, ready to be
knocked about by hits in milestone 3. Nobody attacks yet; that is milestone 2.

## Opening the project

1. Open the folder in Unity Hub with **6000.6.7f1** (any 6000.6.x should do).
2. On first open the project sets itself up (Console: `[Battler Setup]`): URP, materials, the body meshes' import
   settings, the tuning assets, the three unit prefabs and the Battle scene. It opens `Assets/_Battler/Scenes/Battle`
   when done. Setup version 9 also **removes the old orc, goblin, club, attack and Sandbox assets** from your local
   project (they are on the backup branch). Run setup again any time from **Physics Battler > Setup > Run Project Setup**.
3. Press Play. Commit the `.meta` files Unity creates for the new scripts and assets.

## Battle scene controls

| Input | Does |
| --- | --- |
| R | Reset the board (respawn all six) |
| T | Turn everyone around (and back): makes them step and turn on the spot |
| P / . | Pause / step one physics frame |
| M | Mark the playtest log |
| Right-drag, middle-drag, scroll | Orbit, pan, zoom |

The panel has the same buttons plus a time scale slider and the recorder controls.

## First run: what to check

1. **Standing.** Every unit stands in a staggered stance (left foot forward), feet flat, knees soft, guard up. Nothing
   wobbles or hangs as if on strings: the legs are animated, so they can't buckle.
2. **Idle.** Small weaves and bounces and shuffles often; Medium moves like a boxer; Large barely sways and stands
   wide. Nobody stands perfectly still.
3. **Feet.** Planted feet never slide. Press **T**: each unit turns round and steps its feet round under it, one at a
   time, the weight shifting over the standing foot.
4. **Arms and head.** The guard follows the idle without lag or jitter (they are physical, pinned to the pose).

If something looks off, press **M** at the moment and mention the mark number.

## Playtest logs

Every Play session is recorded into `PlaytestLogs/<date_time>/` (next to `Assets/`, so Unity doesn't import it):
what happened and why (`events.log`: spawns, steps, marks, console warnings), a summary (`session.md`), per-step data
for every unit (pelvis height, feet, balance, idle motion, authority per body region, every part's distance and angle
from its pose) and the full pose of the physical body and the animation pose. It also saves small square pictures of
the game view (300×300, 8 per second of game time) into `frames/`. The panel shows the session being recorded;
*New log* starts a fresh folder; logs close when Play stops.

To share one: stop Play, then commit and push the session folder.
`python3 Tools/Playtest/analyze.py PlaytestLogs` analyses the newest session: a report with flags (sliding feet,
weight off the feet, pinned parts lagging, shaking), an overview and a timeline per unit, stick-figure film strips of
each unit's idle, its turns and every mark, and the pictures in captioned sheets (needs numpy, pandas, matplotlib
and Pillow).

## How it works

Each unit has two rigs built from the same null tree. The **anim rig** is posed in code every physics step; it is the
authority over where the body should be. The **physical body** (one rigidbody per part) follows it with an
**authority level** per region: *Kinematic* (moved exactly; the legs), *Pinned* (dynamic, driven by velocity toward
the pose with a strength 0–1; torso, head, arms), *Loose* and *Ragdoll* (for hits and knockdowns, later milestones).

The pose is built in order: stance (planted feet, steps) → combat idle → upper body (hip turn, lean, chest
counter-turn, level head, guard arms) → weight shift over the support point → pelvis lowered only if a foot can't be
reached → two-bone leg IK → feet flat.

| Folder (`Assets/_Battler/Scripts/Runtime`) | What lives there |
| --- | --- |
| `Rig/` | `PartId` (the 16 parts and their tree), `RigFile` (the body rig JSON), `UnitBuilder`, `UnitRig`, `AnimRig`, `BodyPart` |
| `Motion/` | `CombatStyle` (tuning), `BodyMeasure`, `Stance`, `CombatIdle`, `PosePipeline`, `TwoBoneIK` |
| `Authority/` | `AuthorityController` (levels per region, pinning) |
| `Units/` | `UnitController` (the per-step loop), `UnitProfile` |
| `Board/` | `BoardGrid` (tiles), `BoardView` (the board mesh) |
| `Sandbox/` | `BattleSandbox` (lineup, panel, keys), `OrbitCamera`, `SandboxInput` |
| `Diagnostics/` | `PlaytestRecorder`, `UnitTrack`, `FrameCapture` |
| `Muscles/` | `JointMath`, `JointConventions` (joint limit conventions), `SupportPolygon` |

Editor setup lives in `Scripts/Editor/Setup`. Tests (EditMode, NUnit) live in `Assets/_Battler/Tests/EditMode`.

## Tuning

Tune in the data assets, not on the prefabs (setup rebuilds prefabs). Changes made in Play mode persist.

- `Data/Styles/CS_Small`, `CS_Medium`, `CS_Large` (combat styles): stance width, length, crouch, lead foot, toe-out;
  hip turn, chest counter-turn, lean; guard arm angles; idle sway, bob and weave (amplitude and frequency); step
  threshold, turn threshold, step duration and height, idle shuffle interval and distance. Lengths are fractions of
  the unit's leg length, so one style fits any size.
- `Data/Units/UP_Small`, `UP_Medium`, `UP_Large` (unit profiles): which body, which style, turn speed, how strongly
  the torso and arms are pinned to the pose (`torsoPin`, `armPin`), and the rigidbody settings.

## Art

The three base bodies come from `SourceArt/Bodies/Scripts/build_bodies.py` (Blender 5.2): a simple mannequin of
lathed parts with visible ball joints. It also writes `Assets/_Battler/Data/Rigs/<body>_rig.json` (nulls, aim
points, masses, colliders). See `SourceArt/Bodies/README.md`.

## Compile check and tests without Unity

`Tools/CompileCheck` compiles every script against small UnityEngine/UnityEditor/URP stubs (with real vector maths),
runs the EditMode tests, and can dry-run the battle (six units on the board, with a tiny integrator standing in for
PhysX) to produce a playtest log for `analyze.py`:

```
cd Tools/CompileCheck
dotnet build Editor.csproj                     # runtime + editor scripts
dotnet build Runtime.csproj -p:LegacyInput=true
dotnet build Tests.csproj && dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll
dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll smoke /tmp/smoke   # dry run, writes a playtest log
```

The stubs only cover the API this project uses, and the dry run has no collisions or joints; Unity is the final word.
