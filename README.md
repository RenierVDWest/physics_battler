# Physics Battler

An auto battler where units fight with physics-driven, procedural animation: active ragdolls instead of keyframes.
Unity 6.6 (6000.6.7f1), URP, PC.

Design spec: https://claude.ai/code/artifact/b1e9f331-bdae-4fc5-ad71-c1ea73402024

## Current state: first prototype (spec milestones 1–6 in code, untuned)

Two placeholder orcs in a sandbox. One swings a 2 m club low along his side and into the other's chest; the victim reels,
takes catch steps and tries to stay on his feet. Everything is in code, but none of it has been run in Unity yet, so the
first sessions are about checking each layer works and tuning it (see **First run** below).

## Opening the project

1. Open the folder in Unity Hub with **6000.6.7f1** (any 6000.6.x should do).
2. On first open the project sets itself up (Console: `[Battler Setup]`): URP, materials, the orc and club models' import
   settings, the tuning assets, the prefabs and the Sandbox scene. It opens `Assets/_Battler/Scenes/Sandbox` when done.
   Run it again any time from **Physics Battler > Setup > Run Project Setup**; **Rebuild Prefabs** rebuilds just the prefabs.
3. Press Play. Turn **Gizmos** on in the Game view to see the ghost rig (cyan) over the physical body (white), the
   swing's arc (magenta, red once committed), and the capture point (yellow) and support centre (green).

## Sandbox controls

| Input | Does |
| --- | --- |
| Space | Orc A swings at Orc B |
| R | Respawn both (applies the distance slider) |
| H | Hit the attacker from the side |
| P / . | Pause / step one physics frame |
| G | Ghost gizmos on or off |
| M | Mark the playtest log |
| Left-click | Push the body part under the mouse |
| Right-drag, middle-drag, scroll | Orbit, pan, zoom |

The panel also has: attack every 4 s, goblin / barrel into the arc, drop a crate between them, kill Orc B, push strength,
allies-take-damage, and a time scale slider. Each unit shows its state, stability (white) and health (red).

## First run: what to check, in order

Each layer depends on the one before it, so check them in this order.

1. **The joint probe.** The panel and Console show `Joint probe: drive standard ..., X limit sign +1 ...`. It measures
   ConfigurableJoint's sign conventions at start, and the joints use what it found. If it reports something odd, send it to Claude.
2. **Standing.** Both orcs should stand with little wobble. If they sag, raise `kpVertical` or `supportFeedForward` on
   `Data/Units/UP_Orc`; if they wobble, raise `dampingRatio` or lower the muscle Hz.
3. **Pushing.** Click a chest with a small push: the orc should wobble and recover. A big push should make it stagger,
   take catch steps, and then fall and get back up, but never go limp.
4. **The swing.** Orc A holds the club in both hands, low at his right side and pointing back. Space: he pulls it back
   and up behind him and brings it to a stop (weight on the back foot); the moment the pull-back has stopped he swings
   it down round his side and forward and up into the chest (weight onto the front foot), brakes it in the
   follow-through, and brings it back to the carry.
   If the club lags far behind the ghost, raise `armHz` or the attack's `strikeArmBoost`.
5. **The hit.** Orc B should take damage, stagger and step back. The hit log shows the speed. If swings never register,
   lower `minHitSpeed` on `Data/Weapons/WD_ClubBig`.
6. **Interference.** Goblin into the arc, barrel into the arc, crate between them, side hit mid-swing.

## Playtest logs

Every Play session is recorded into `PlaytestLogs/<date_time>/` (next to `Assets/`, so Unity doesn't import it):
what happened and why (`events.log`), a summary (`session.md`), and per-step data for every unit (state, balance, swing,
each joint's error against the ghost, and the full pose of the physical body and the ghost). The panel shows the session
being recorded; **M** (or *Mark*) drops a numbered marker with a snapshot of every unit — press it when you see
something odd and mention the mark number. *New log* starts a fresh folder; logs close when Play stops.
It also saves small square pictures of the game view (300×300, 8 per second of game time, from the main camera, without
the UI) into `frames/`, with `frames/frames.csv` saying when each was taken; the recorder's inspector sets the size,
rate and limit.

To share one: stop Play, then commit and push the session folder (a minute of play is about 1–2 MB of data plus about
7 MB of pictures).
`python3 Tools/Playtest/analyze.py PlaytestLogs` analyses the newest session: a report plus timelines, swing plots,
stick-figure film strips of every swing, fall and mark, and the pictures laid out in captioned sheets, overall and per
swing, fall and mark (needs numpy, pandas, matplotlib and Pillow).

## How it works (one paragraph per layer)

Each unit is two rigs. The **ghost** (an invisible copy of the null tree) is posed procedurally every physics step. The
**physical body** (rigidbody parts joined by ConfigurableJoints) chases the ghost with acceleration-based joint springs, the
**muscles**. A tunable **balance assist** holds the pelvis up and the torso upright, and hits take that support away
through **stability**. Damage comes from what the weapon physically touches, never from who it was aimed at. The full
reasoning is in the design spec.

| Folder (`Assets/_Battler/Scripts/Runtime`) | What lives there |
| --- | --- |
| `Rig/` | `PartId` (the 15 parts and their tree), `RigFile` (orc_rig.json), `UnitBuilder`, `UnitRig`, `BodyPart`, `GhostRig` |
| `Muscles/` | `MuscleController` (joint drives, hit dips), `BalanceController` (hand of god, re-anchoring, catch steps), `JointMath`, `JointConventions` (the probe) |
| `Motion/` | `PoseComposer` (stance, legs and arm IK, look-at), `TwoBoneIK`, `SwingSolver`, `AttackPlanner`, `AttackDefinition`, `Stance` |
| `Combat/` | `Weapon`, `WeaponDefinition`, `ImpactSystem` (hits, shoves, `HitEvent`) |
| `Units/` | `UnitController` (the per-step loop), `UnitState` (stability and states), `UnitProfile` |
| `Sandbox/` | `SandboxController` (panel, scenarios, push tool), `OrbitCamera`, `SandboxInput` |

Editor setup lives in `Scripts/Editor/Setup`. Tests (EditMode, NUnit) live in `Assets/_Battler/Tests/EditMode`.

## Tuning

Tune in the data assets, not on the prefabs (the setup rebuilds prefabs). Changes made in Play mode persist.

- `Data/Units/UP_Orc`, `UP_Goblin`: stance (`stanceWidth`, `stanceLength`, `leadWithLeftFoot`, `stanceToeOut`,
  `stanceCrouch`), muscles, balance assist, stability, catch steps, getting up. Balance is measured against the area
  the feet support: `stepThreshold` is how far outside it the weight may go before a catch step. The knobs that
  matter most right now: `maxSubtreeBoost` (how much the arm may stiffen to carry the club; the log's `muscles` line
  shows what each joint needs), `minPartInertia`, `gravityCompensation`, `legSupport` (planted legs carry the body),
  `velocityFeedForward`, `uprightBodyShare` and `supportFeedForward` (how hard the balance assist holds the unit up:
  lower is heavier and less puppet-like), `steadyStepFactor`, and the get-up's `getUpDuration`, `getUpGrace`,
  `standMaxTilt`. Any of `gravityCompensation`, `legSupport`, `velocityFeedForward` can be set to 0 in Play mode to
  switch that part off and see what it does.
- `Data/Weapons/WD_ClubBig`: `size` (the club is built at half its modelled size, 0.95 m, one-handed, 2 kg), physics
  mass vs gameplay mass, damage, knockback, stability loss; optionally two-handed (`twoHanded`, `secondGrip`, and the
  low carry `carryGrip`, `carryDirection`). The physics mass (and
  where the club's colliders put its weight) is what the wielder has to move: with `strength` on the unit profile it
  sets how fast a swing can go. The log's `equip` line says how the two measure up.
- `Data/Attacks/AD_ClubLowSwing`: no timings any more: the swing's pace comes from the unit's `strength` against the
  weapon's real weight. Here: the wound-up angle and the follow-through room (`followAngle`, which caps the strike's
  speed to what can be braked), each phase's effort (share of strength), how settled the club must be before the
  strike, plane roll, body twist and weight shift.

## Art

The placeholder orc and club come from `SourceArt/Orc/Scripts/build_orc.py` (Blender 5.2), which also writes
`Assets/_Battler/Data/Rigs/orc_rig.json` (nulls, masses, colliders and club markers). See `SourceArt/Orc/README.md`.

## Compile check and tests without Unity

`Tools/CompileCheck` compiles every script against small UnityEngine/UnityEditor/URP stubs (with real vector maths) and
runs the EditMode tests:

```
cd Tools/CompileCheck
dotnet build Editor.csproj                     # runtime + editor scripts
dotnet build Runtime.csproj -p:LegacyInput=true
dotnet build Tests.csproj && dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll
```

The stubs only cover the API this project uses; a stub that is wrong can hide a real compile error, so Unity's own
compile is still the final word.
