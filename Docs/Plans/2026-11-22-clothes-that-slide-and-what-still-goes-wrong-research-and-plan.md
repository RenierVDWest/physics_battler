# Physics Battler: clothes that slide, and what still goes wrong (research and plan)

22 November 2026, 20:15. Live version: https://claude.ai/artifact/QruYXVgHVVNgukoVt3jyUs (progress notes go there and
here). Written after his look at 20:11; nothing in it is built yet. Its film: https://claude.ai/artifact/LeWJakPgY8rMBwph6wZBHk

## Bottom line

The speed is fixed and limbs no longer come through the cloth. Five things are left. Four are the cloth: spikes when a
man falls, a ridge along the shoulder seam, the elbow coming through with spikes round it, and cloth that is dragged and
pinched by a turning arm instead of sliding over it. The fifth is not the cloth at all: a head, an arm or a sword stuck
inside the other man's shield. The plan below fixes them in six steps, each behind a switch with an old/new toggle in
the lab. The step that matters most is the fourth: let the cloth slide over the body instead of being glued to it.

**What he said at 20:11:** "OK this is much better now. I have not seen limbs coming though the cloths anymore. And the
spikes are much reduced, but not perfect." He showed seven pictures and asked for a plan before any fixing.

| # | What he saw | His picture | Measured |
| --- | --- | --- | --- |
| 1 | Spikes on the shirt when a man falls (not every fall) | The target lying on his side, spikes at the shield shoulder | His log of 19:55: the longest thread is 4.7 times its length in half of all moments, 10 times in one in ten, 24 times at worst. This counts threads hidden inside the body too, so it overstates what shows. |
| 2 | A head, an arm or a sword stuck inside the other man's shield | Two pictures, close in | Not measured: no line in his log counts it yet. |
| 3 | A ridge along the shoulder seam | The swordsman's left shoulder | Not measured. |
| 4 | Spikes round the bent elbow, and the elbow coming through | Three pictures, both men | Not measured on its own; part of row 1. |
| 5 | The cloth pulled and pinched as the arm lifts and turns, instead of sliding over it | (what he saw while watching) | Not measured. |

**His log of 19:55 (Unity, a dance of 228 s):** the speed fix works. A frame takes 25.7 ms (median), against 40.1 ms
before the fix and 24.7 ms at step 3. The clothes take 8.2 ms of it, against 18.3 ms before. Each man's cloth costs 4.4
ms of main-thread time (median). His own sword and shield crossed his body 38 times, each 0.05 to 0.45 s: the old
measure, not yet compared with earlier logs.

## Why each one happens

The root of three of the five is one design choice: each cloth point is glued to the body part that carries it,
turning with it, and only swings a little around that glued place. Real cloth is not glued; it slides. Where the trace
is firm I say so; where it is still a guess I say that too.

**1. Spikes when a man falls (traced headless, 17 s of "Shake him").** A man on the ground has his knees and arms folded
against himself. A cloth point carried by the shin can end up deep inside the thigh lying on it. Pushed out of the
thigh, it goes out the far side, 16 to 23 cm from where it belongs, and its threads become spikes. The last fix limits
that push to 3 cm, but the moving step itself can still throw a point up to its swing limit (10 cm at a hem) when the
body stops hard against the ground. One more gap: the floor is not in the cloth's world at all, so cloth under a lying
man hangs through it (hidden, but it pulls its neighbours).

**2. A head, arm or sword stuck in the other man's shield (not the cloth; cause not found).** The cloth never touches
the physics, so it cannot cause this. It is the bodies and shields themselves, close in (bash, pounce, "puts the other
before him" in "Shake him"). Whether it is new or only newly noticed is not known: no line in his log counts it. First
job: measure it in his logs and headless, and compare a build from before today's cloth work.

**3. The ridge along the shoulder seam (from the code; not yet measured).** The sleeve and the shirt's body choose their
carrying parts by different rules. Sleeve points ride the upper arm, and the chest only within 10 cm of the shoulder.
Body points ride the chest, and the upper arm only within 8 cm. So the two sides of the seam are carried differently.
When the arm lifts they move apart or press together, and the seam stands up as a ridge.

**4. The elbow through, and spikes round it (partly traced).** Three causes add up. First, sleeve points near the elbow
are carried round the joint at a fixed distance from its centre, but the point of a bent elbow sticks out further than
that. Second, the baked body puts a 2.9 cm ball at the top of the forearm; the grey elbow ball on his model may be
bigger, or a separate mesh the cloth does not know about (to be checked in the model file). Third, where a point is
pushed out of the elbow and its neighbour stays in the crease, the thread between them becomes a spike.

**5. Cloth pinched and dragged instead of sliding (from the code).** There is no friction in the collision now: a push
moves a point straight out of the body and nothing holds it sideways. The pinching comes from the gluing. Each point
turns with its part's whole rotation, twist included, so when the arm rolls, the sleeve rolls with it and the cloth
between arm and chest is wrung. Then three things pull the cloth back to its glued place: the 3% pull each picture
toward the drape, the swing limit (at most 10 cm from the glued place) and the backstop. So the cloth cannot slide even
where nothing stops it.

## How games and cloth tools solve these

Game cloth uses three limits instead of gluing. Each point has a sphere it may not leave (max distance), a sphere it may
not enter (backstop), and a leash to where the garment is held that only pulls when the cloth is stretched. Inside those
it moves freely. Our cloth already has the first two; it lacks the leash, and it glues where games let go.

| Idea | Where it comes from | What it fixes here |
| --- | --- | --- |
| **Long-range leash (tether).** Each point may be no further from the held part (collar, shoulders, waistband) than its distance along the cloth; inside that it moves freely. The paper reports 5 solver sweeps with leashes looking better than 15 without. | Kim, Chentanez, Müller, [Long Range Attachments](https://matthias-research.github.io/pages/publications/sca2012cloth.pdf), 2012 (NVIDIA) | Lets cloth slide without falling off or stretching: the replacement for the glue (5) and for the stretch that makes spikes (1, 4). |
| **Max distance and backstop**, painted per point. Max distance is how far a point may move from its animated place; the backstop is a sphere behind it that it may not enter. | Unreal's [clothing tool](https://dev.epicgames.com/documentation/unreal-engine/clothing-tool-in-unreal-engine) and [backstop settings](https://dev.epicgames.com/documentation/unreal-engine/node-reference/Dataflow/SimulationBackstopConfig) | Ours since 19:45. To change: measure the backstop from the body's surface, not from the glued place, so sliding cloth keeps it. |
| **Friction as a number**, cloth against body, 0 to 1. | Chaos Cloth's [friction coefficient](https://dev.epicgames.com/documentation/unreal-engine/API/Plugins/ChaosCloth/UChaosClothConfig) | His ask. Ours is 0 in the collision already; the sticking is the glue. A friction lever is still useful once the glue is loosened (some grip on the shoulders, little on the arms). |
| **Twist taken out of the carry.** A rotation splits into a swing (where the bone points) and a twist (its roll about itself). | [Swing-twist decomposition](https://arxiv.org/pdf/1506.05481) (Dobrowolski); Unreal's [ToSwingTwist](https://dev.epicgames.com/documentation/unreal-engine/API/Runtime/Core/TQuat/ToSwingTwist) | Sleeves follow where the arm points, not its roll: no wringing (5). |
| **Weights smoothed over the surface**, so neighbours are carried alike. | Baran and Popović, [Automatic Rigging](https://people.csail.mit.edu/ibaran/papers/2007-SIGGRAPH-Pinocchio.pdf), 2007: bone weights by proximity, smoothed by diffusion over the surface | The shoulder seam ridge (3). |
| **Small steps**: many short steps with one sweep each beat a few steps with many sweeps. | Macklin and others, [Small Steps in Physics Simulation](https://diglib.eg.org/bitstreams/5ccd0aa4-edd9-48a1-8ed5-1c50d227a37a/download), 2019 (NVIDIA) | Fast limbs and hard landings (1, 4) at the same cost. |
| **Collision with friction and contact in the same loop**, cloth against particles and meshes. | Macklin and others, [Unified Particle Physics](https://matthias-research.github.io/pages/publications/flex.pdf), 2014 (NVIDIA Flex) | How to add friction cleanly if wanted. |

## The plan: six steps

Measure first, then the shield, then loosen the glue, then the three local faults. Each step gets a Combat Feel switch
with a tooltip, an old/new row in the duel lab under "Clothes", a headless measure, the panel's yellow frames, a README
entry, a push and a film.

1. **Measures that see what he sees** (no change to how anything moves).
    - Visible spikes: threads more than 2 times their length with both ends outside the body. Threads hidden inside the
      body stop counting, so the number matches what shows.
    - A body in the other man's shield: how deep and how long, in his log and headless.
    - The shoulder seam: the bend across it, against the drape.
    - Pinch: cloth squashed below 60% of its length, and how far the sleeve rolls when the arm rolls.
    - Bare elbow: rays from the drawn elbow that reach no cloth.
    - Also check his model file: is the grey elbow ball part of the forearm mesh the cloth knows, or a mesh of its own?
    - Pass: each measure reads the same thing in his Unity log as headless, on the same moves.
2. **The shield** (bodies and shields, not cloth).
    - Count it in his 19:55 log and in yesterday's dance logs, and headless on today's build and on this morning's. That
      tells whether it is new.
    - Then trace one case step by step and fix its cause, behind its own switch.
    - Pass: no part of a man more than 2 cm inside the other man's shield for longer than 0.1 s in "Shake him" and in
      four dances.
3. **Sleeves and trouser legs stop rolling with the limb.** Carry them by where the limb points (its swing), not its roll
   about itself (its twist).
    - Lab: "The limb's roll:" ignored (new) / carried (old).
    - Pass: when the forearm rolls 90 degrees, the sleeve turns less than 20 degrees with it.
4. **The cloth slides over him (the big one).**
    - A leash from each point to where the garment is held, as long as the cloth between them. It only pulls when
      stretched, so the cloth cannot fall off or stretch.
    - The glued place becomes a soft guide: a weak pull, and a swing limit along the body larger than the one away
      from it.
    - The backstop is taken from his skin, not from the glued place.
    - A friction lever, 0 to 1: low on the arms and legs, more on the shoulders and waist.
    - Lab: "Cloth on him:" slides (new) / glued (old), and "Friction".
    - Pass: pinched threads (under 60% of length) at the armpit and elbow halved in "Shake him"; visible spikes no more
      than now; limbs through no more than now (0.12% of checks).
5. **The shoulder seam.** Smooth the carrying weights over the cloth, so neighbours on both sides of a seam are carried
   alike. The same at the elbows and knees.
    - Lab: "Seams carried:" smoothly (new) / by side (old).
    - Pass: the bend across the shoulder seam within 30 degrees of the drape's with the arm raised 90 degrees.
6. **Elbows, knees and falls.**
    - Carry cloth round a bent elbow or knee at the drawn joint's real reach, and put the elbow ball in the baked body
      if it is a mesh of its own.
    - Add the floor to the cloth's world.
    - Soften the jolt of a hard landing (more small steps when the body moves fast; a cap on what one picture's stop
      may throw).
    - Pull a lone spiked point back among its neighbours.
    - Lab: one switch each, and "Kick him down" to make a fall on demand.
    - Pass: in 20 falls and "Shake him", at most 1 picture in 1,000 with a visible spike, none over 3 times; no bare
      elbow skin.

The cost must stay at or under his 19:55 log: 8.2 ms of clothes in a 25.7 ms frame. If a step costs more, its cheaper
setting comes first.

## How we prove it

Every step is measured headless before it is pushed, and checked in his Unity log after. Pictures are drawn of the same
moments with the switch off and on, because today the counts missed knees poking through that the pictures showed.

| What | Headless | His Unity |
| --- | --- | --- |
| Visible spikes | "Shake him", 20 falls, the rack of 1,000 poses | A new count on each `cloth` line in his log |
| Body in the other man's shield | "Shake him", four dances | A new `shield` line in his log |
| Shoulder seam, pinch, sleeve roll | "Shake him"; arm raised and rolled on demand | His eye; his pictures |
| Bare elbow | Rays from the drawn body (`coverage.py`) | His eye |
| Limbs through, cloth inside | As today | As today |
| Cost | Time per part of the step | `ms_Clothes` and the frame time |

Only his Unity can tell: how it looks in motion, and what it costs with Burst on many cores. The headless runs use one
core and no Burst.

## Questions for him

My recommendation comes first in each; "go" takes them all.

1. **The shield first?** It is not the cloth, but it may be what shows most. Recommended: yes, step 2 before the cloth
   steps, but measure first and report what was found before fixing.
2. **How much grip?** Recommended: friction 0.1 on the arms and legs, 0.3 on the shoulders and waist, as a lever to tune
   by eye.
3. **How far may cloth slide?** Recommended: as far as the leash allows along the body (a sleeve can ride up the forearm
   a few centimetres), at most 3 cm away from the body except at the hems.
4. **The order?** Recommended as written: measures, shield, roll, slide, seam, elbows and falls; one film after each step
   that needs his eye.

## Sources

- Kim, Chentanez, Müller-Fischer: [Long Range Attachments: A Method to Simulate Inextensible Clothing in Computer Games](https://matthias-research.github.io/pages/publications/sca2012cloth.pdf), SCA 2012
- Epic Games: [Clothing tool in Unreal Engine](https://dev.epicgames.com/documentation/unreal-engine/clothing-tool-in-unreal-engine) (max distance, backstop); [Simulation backstop config](https://dev.epicgames.com/documentation/unreal-engine/node-reference/Dataflow/SimulationBackstopConfig); [UChaosClothConfig](https://dev.epicgames.com/documentation/unreal-engine/API/Plugins/ChaosCloth/UChaosClothConfig) (friction coefficient); [TQuat::ToSwingTwist](https://dev.epicgames.com/documentation/unreal-engine/API/Runtime/Core/TQuat/ToSwingTwist)
- Dobrowolski: [Swing-twist decomposition in Clifford algebra](https://arxiv.org/pdf/1506.05481), 2015
- Baran, Popović: [Automatic Rigging and Animation of 3D Characters](https://people.csail.mit.edu/ibaran/papers/2007-SIGGRAPH-Pinocchio.pdf), SIGGRAPH 2007
- Macklin and others: [Small Steps in Physics Simulation](https://diglib.eg.org/bitstreams/5ccd0aa4-edd9-48a1-8ed5-1c50d227a37a/download), SCA 2019
- Macklin, Müller, Chentanez, Kim: [Unified Particle Physics for Real-Time Applications](https://matthias-research.github.io/pages/publications/flex.pdf), SIGGRAPH 2014
- His playtest log `PlaytestLogs/2026-11-22_19-55-41` and seven pictures of 20:11; this project's README entries of 18:20 and 19:45

## Progress

**22 November, 22:40: he said "go" at 20:48; all six steps built (headless only; the Burst job's new fields built blind).**
The README's entry of 22:40 has the tables.

- Step 1: the measures are in his log (`shieldin`, `shieldletgo`, and two more figures on each `cloth` line) and headless
  (`SHIELDIN`, `SEWNLOOK`). The grey elbow ball is the forearm mesh's own top: the cloth already knows it.
- Step 2: in his log of 19:55 the board stayed 9 cm in the other man's chest for 1.1 s after a small bash, carried along
  as he stumbled back. Not reproducible headless (the engine pushes it out in 0.03 s). Built: a shield lets go of a man
  stuck in it for a tenth of a second ("A man stuck in a shield:", "Bash him close").
- Step 3: sleeves follow where the limb points, not its roll ("The limb's roll:"). Alone the best of the six.
- Step 4: the cloth slides, with a leash and a grip ("Cloth on him:", "Grip on his limbs" 0.1, "Grip on his trunk" 0.3).
- Step 5: the shoulder seams carried smoothly ("The shoulder seams:").
- Step 6: the floor, a cap on a hard stop's throw, lone spikes put back; the joint's real reach built and left off (it
  more than doubled the spikes).
- Headless, "Shake him", old against new: spikes over 3 times in 19.1% of pictures to 13.0%; armpit pinch 2.5% to 1.0%
  (halved); elbow pinch 4.3% to 4.4% (not halved); the seam's worst ridge 16.6 to 6.0 cm; points through 31,077 to
  20,530. Three falls: spikes over 3 times 22.4% to 18.2%. The deepest point inside got worse (2.4 to 5.9 cm).
- Not met: the elbow pinch halved; the falls' 1 in 1,000. Not known: how it looks or costs in Unity.
- Its film: https://claude.ai/artifact/CsxNWHwbksFJax1iWgxf25
