# Locomotion overhaul plan

> Copy of the Claude document <https://claude.ai/artifact/PRykbXtbFivnTjXWBN2ntf>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 3, 2026 · @Renier

A new walking system for the Medium, built on its own branch: the reference clips drive the whole body through our rig, the feet are locked to the ground whenever a clip says they touch it, and physics pushes come in as changes to where the body is going, answered with real steps.

## Where we are

Today's walk is a procedural stepper with a captured walk loosely on top. It steps the feet wherever the steering pushes the body, which gives the gliding and double-lift you saw. Planted feet never slide in the logs: by construction they sit on their spot. The fault is when and how the feet step.

How the current walk works:

1. Steering moves the root at a velocity, whatever the legs are doing.
2. A single forward walk clip per style supplies a cycle phase, the body's bob and the arm swing. It does not supply where the feet go.
3. The stance picks each foot's next spot ahead of the hips and steps it there with a two-bone IK leg and a fixed lift arc.
4. Below 30% walk weight (slow backing off, circling, shuffling into range), the stance's own catch-up steps take over. They are quick 0.25 s hops with no gait timing at all.

What the playtest from this morning shows, for the A and B Mediums:

| Measure | A Medium | B Medium | A real walk |
| --- | --- | --- | --- |
| Walking time with both feet in the air | 13% | 20% | 0% |
| Catch-up steps with the other foot also up for over 30% of the step | 19 of 53 | (similar) | none |
| Walking swing time per step | 0.60–0.67 s | (similar) | about 0.4 s |
| Feet's travel / body's travel while walking | 1.25 | 1.32 | 1.0 |
| Time spent walking backward | 23% | 42% | (game choice) |

The last row matters: units back off a lot (giving ground, holding range), and there was no backward walk at all, only forward steps reused in reverse and catch-up hops. Fixing the stepper would not fix the deeper issue: nothing in the body, legs or arms comes from a reference that matches the direction and speed the body is actually moving.

## The reference clips

The 14 clips are good material: one skeleton throughout (leg 0.89 m, our Medium's is 0.85, so a 0.95 scale), baked at 30 keys a second, and every loop closes to the centimetre. What they lack is slow walking, which is what our units mostly do (the Medium's walk speed is 0.7 m/s).

| Clip | What it is | Speed | Notes from the measurements |
| --- | --- | --- | --- |
| Walking | Forward loop | 1.67 m/s | Long 0.87 m steps, big arm swing (0.6 m) |
| Walking\_1 | Forward, 2.5 s | 1.08 m/s | Deep hips drop, strong arm swing |
| Brutal\_To\_Happy\_Walking | Forward loop | 1.16 m/s | A heavier, swaggering walk |
| Walking\_2 | Slow, cautious | 0.53 m/s | Drifts 27° to the left; arms nearly still |
| Walking\_Backward | Backward loop | 1.20 m/s | Clean, balanced |
| Walking\_Backwards | Backward loop | 1.13 m/s | Clean, balanced |
| Walking\_Backwards\_1 | Slow backward | 0.69 m/s | Drifts diagonally; hips turned 37° |
| Left\_Strafe\_Walking | Sideways loop | 1.73 m/s | Hips and feet turned 66–78° into the travel: a crossover side-walk, not a side-step |
| Right\_Strafe\_Walking | Sideways loop | 1.73 m/s | Mirror of the above |
| Right\_Strafe\_Walk | Sideways loop | 1.95 m/s | Hips turned 81°: almost a forward walk sideways |
| Walk\_Strafe\_Left | Slow side-step | 0.61 m/s | Hips turned 38°: the closest to a true strafe |
| Start\_Walking | Standing to walking | 0 to 1.3 m/s | 2.9 s, 2.0 m |
| Stop\_Walking | Walking to standing | 1.25 to 0 m/s | 3.0 s, 1.2 m, a settling step at the end |
| Walking\_Turn\_180 | Pivot while walking | about 0.1 m/s | A 180° turn in 1 s, on the spot |

The clips' own feet slide 1–3 cm while down (7.6 cm in the pivot turn), so contact has to be labelled from the data and then enforced, not trusted.

Missing for our game:

- **Slow walks in every direction** (0.5–0.8 m/s): forward, backward, left and right. These matter most, because combat footwork is slow.
- **True side-steps** facing forward, for circling an opponent.
- **90° turns while walking**, left and right, and turns on the spot.
- **Diagonals**, forward and backward.

Mirroring every clip left to right doubles the set for free: a right strafe gives a left strafe, and the 180° pivot both ways.

### The second set (3 November): turns, side-steps and running

The second set fills every gap the first one had except diagonals. All 31 clips are on the same skeleton, so they mix freely. A few duplicate each other (the pack's right turn is Right\_Turn\_1), and the two briefcase turns carry a briefcase in one hand, so only their legs and body are used.

| Clips | What they give | Measured |
| --- | --- | --- |
| Left\_Turn\_1, Right\_Turn, the pack's left and right turn 90, Sword\_And\_Shield\_Turn | Turns on the spot, about 90° | 90–103° in 0.9–1.0 s |
| Left\_Turn, Left\_Turn\_2, Right\_Turn\_1 (and the pack's turns) | Turns on the spot, about 180° | 123–176° in 0.9–1.6 s |
| Left\_Turn\_W\_Briefcase, Right\_Turn\_W\_\_Briefcase | Walking 90° turns | 90° over 2.2 s at 1.3 m/s |
| Walk\_Strafe\_Left, Walk\_Strafe\_Right | Slow side-steps, facing forward | 0.61 m/s, hips turned only 30–38° |
| Long\_Left\_Side\_Step, Medium\_Right\_Side\_Step | Single combat side-steps from a fighting stance | 0.5 m in 0.8 s |
| The pack's walking, strafe walking and idle | The forward walk and crossover strafes again; an 8 s idle | Forward 1.67 m/s |
| The pack's running, Left\_Strafe, Right\_Strafe | Running forward and sideways | 4.4 m/s forward, 3.6–4.5 m/s sideways, both feet off the ground 40–50% of the time |
| Running\_Turn\_180, Turn\_To\_Running | A running 180° pivot; a start that turns into a run | 180° in 0.7 s; 113° into a 1.9 m/s start |

Still missing, and covered by blending until found: diagonals, a backward run, a stop from a run.

## How games solve this

Feet slide whenever the body moves differently from what the legs in the clip imply. A planted foot has zero speed, so the body must travel exactly as the clip's hips travel over that foot. Every studio fix does three things in order:

1. Make the body's movement and the animation agree.
2. Blend only between clips whose feet are in step.
3. Lock the feet with IK as the last safety net.

Locking alone, on an animation that disagrees with the body, only moves the error into stretched legs and knee pops. That is close to what our current stepper does.

| Technique | What it solves | Where it comes from | What we take |
| --- | --- | --- | --- |
| Motion matching | Picks, every few frames, the clip frame that best fits the current pose and the path ahead | Clavet, For Honor (GDC 2016); Holden, Learned Motion Matching (2020); The Last of Us Part II | Its path prediction and feature search, but only to choose start, stop and turn clips. It needs long continuous takes; 14 short loops would leave holes. |
| Phase-locked directional blending | Forward, back and side cycles blended by the direction of travel, all sharing one foot phase so the feet never disagree | Unreal sync markers; Lyra's four-direction walk; Johansen's Unity locomotion thesis | The core of our walk. Phase advances by distance walked, not time, so steps always match ground covered. |
| Stride, orientation and speed warping | Covers speeds and angles between the clips: stretches strides, turns the lower body toward travel and counter-turns the spine | Unreal Pose Warping, Lyra | Covers the slow speeds and diagonals we have no clips for, kept within about ±20% |
| Distance matching | Starts and stops land exactly where they should, played by distance left, not time | Unreal; Paragon (Delayen) | For the start and stop clips and the 180° turn |
| Inertialization / dead blending | Switches clips by fading out the difference, never averaging two motions (crossfades are a main cause of sliding) | Bollo, Gears of War 4 (GDC 2018); Holden, Dead Blending | Every switch between clip families, about 0.2 s, at a frame with the same foot planted |
| Animation drives the root, adjusted toward steering | The body goes where the clip carries it; a small, speed-limited correction pulls it toward where the steering wants it | Holden, Code vs Data Driven Displacement | Our units are AI, so they can follow the animation closely at no cost to play |
| Trajectory springs fitted to the clips | Steering accelerates and turns at the rates the clips do, so the two never drift apart | Holden, Spring-It-On and the fitting articles | Desired velocity and facing each through a spring; the path predicted 0.33, 0.67 and 1 s ahead |
| Foot locking | Pins a foot where it lands for exactly the frames the clip says it is down; lets go if the leg would over-stretch | Holden; Unreal Foot Placement | Our Stance becomes this lock manager, with contacts labelled from the clips |
| Pinned physical body and capture-point stepping | The body reacts to hits through joint motors; when a push would tip it over, it steps to where it can stop | PuppetMaster; Just Cause 2; Pratt's capture point; DReCon | Our authority levels stay; pushes become steps chosen by the same walk |

People notice foot sliding above about 2 cm per plant (Pražák and others, 2011). Our gate is 1 cm.

## The design

The clips drive the whole body; steering only says where to go and which way to face. With 14 short clips, a phase-locked directional blend plus a few transition clips beats full motion matching, the same choice Lyra made for the same reason.

Each physics step, in order:

1. **Intent.** Steering gives a desired velocity and, separately, a desired facing. Strafing and backing off are simply a velocity that differs from the facing.
2. **Trajectory.** Both go through springs whose rates are fitted to the clips, so steering never asks for a turn or start the clips can't do. The path is predicted 0.33, 0.67 and 1 s ahead.
3. **Selection.** The travel direction relative to the facing and the speed pick the cycles to blend (forward, backward, left, right: the two nearest). A start, stop or 180° turn clip is chosen when the predicted path calls for one, by a small search over the path ahead and the feet.
4. **Phase.** One gait phase for all cycles (left foot down at 0, right foot down at 0.5), advanced by distance walked divided by the blended stride. The feet can't fall out of step, and steps always match ground covered.
5. **Warping.** Stride warping stretches or shortens steps for speeds between clips. Orientation warping turns the hips and legs toward the travel and counter-turns the spine so the chest keeps its facing. Both are clamped (±20% stride, ±60° turn) and the next cycle takes over beyond that.
6. **Root.** The body moves as the blended clip carries it. A speed-limited correction pulls it toward the steering's position and facing, and a clamp stops them drifting more than a few centimetres.
7. **Pose.** The clip's joint rotations are retargeted onto our rig: hips to pelvis, the three spine bones to spine and chest, neck and head to head, and the arm and leg bones to ours, scaled by leg length (0.95 for the Medium).
8. **Layers.** A lean from acceleration and turning (into curves, back when stopping). Later, the combat layers go on top of this (see Joining the arms).
9. **Pelvis.** The hips drop just enough for both feet to reach their targets.
10. **Feet.** The Stance becomes the lock manager. A foot locks where it lands for exactly the frames its clip says it is down, pivots about its contact point when the body turns, and lets go when its clip lifts it (or if the leg would over-stretch). Two-bone IK puts each leg on its target, and a walking rule allows only one foot in the air at a time.
11. **Physics.** The physical body follows the pose by its authority levels, as now (see Physics).

Every switch between clip families (cycle to stop, stop to start, cycle to turn) fades the difference out over about 0.2 s (inertialization), starting the new clip at a frame with the same foot down. Two motions are never averaged.

Offline, a clip tool (extending the Python FBX reader we already have) does the data work once for each clip:

- Mirroring left to right.
- A ground-level root track.
- Foot contact labels for heel and toe.
- The phase markers.
- Speed, stride and turn rate.
- Distance curves for the starts and stops.
- The spring rates.

It writes them as generated C# like the walk clips today. The clip reader also learns curve tangents, so key-reduced clips work too.

### Running

Running is a second speed tier in the same blend, not a separate system. Walk and run cycles share the gait phase (left foot down at 0, right at 0.5), so the blend from one to the other keeps the feet in step.

- **When it runs.** People switch from walking to running at about 2 m/s, a Froude number (speed squared over gravity times leg length) of about 0.5 ([walk-to-run transition](https://en.wikipedia.org/wiki/Transition_from_walking_to_running)). For the Medium's 0.85 m leg that is 2.0 m/s. The same rule gives the Small and the Large their own switch speeds from their leg lengths, so every body looks right at its own pace.
- **Hysteresis.** It switches up a little above that speed and down a little below, so it doesn't flicker between gaits.
- **Flight.** A run has both feet off the ground 40–50% of the time, so the walking rule (one foot up at most) only applies below the switch. Foot locks work the same: a running foot is locked for its short time down.
- **Lean and turns.** Running leans further into acceleration and curves, and turns tighter only by slowing. The running 180° pivot and the turn into a run are transition clips like the walking ones.
- **The course** gets a run segment: walk, speed up into a run, a running curve, slow back to a walk, and stop.

## Physics

Physics comes in after the walk is clean, as stages 5 and 6. A push changes where the body is going, and the walk answers it with real steps instead of the legs being dragged.

- **Legs stay kinematic, their targets become physics-aware.** The physical upper body follows the pose on joint motors with pin weights, as it does now. The feet keep their locks, so the physical feet sit exactly where the posed ones are.
- **A hit or shove** loosens the struck region's pin for a moment (it gives, then firms up over 0.3–0.6 s). The change in the body's centre of mass velocity goes into the trajectory spring as an impulse, so the body is carried with the push rather than fighting the locked feet.
- **Balance by capture point.** From the physical centre of mass and its velocity, the point where a foot must land to stop the body is worked out each step. While it stays under the feet, the hips and spine absorb the push. Once it leaves, the walk takes one to three corrective steps toward it with the leg that best covers that direction. These are ordinary clip-driven steps, aimed with the stride and orientation warping.
- **Bumping into another unit** is the same: a small push, absorbed or stepped.
- **Knockdown.** Beyond a set error between the physical and posed body (about a head's distance), the legs are handed to physics and today's stumble, fall and get-up take over. Getting up hands back to the walk at a matched pose.

This is the PuppetMaster and Just Cause 2 recipe with Pratt's capture point for the stepping. DReCon (a learned balance controller on top of motion matching) is the long-term upgrade if the hand-written balance falls short.

## Joining the arms and combat later

The new walk produces a complete base pose. Combat goes on top of it in layers, so merging back means swapping what feeds the bottom layer, not rewriting the arm work.

| Today's system | After the merge |
| --- | --- |
| Stance and gait move the feet; the Fight Rhythm adds hips, lean and hands | The walk gives the whole lower body, the pelvis and the base torso. The Fight Rhythm's postures become additive layers on top. |
| The arm rig poses the weapon arm, the shield arm and the off hand from the guard | Unchanged. The arms take the walk's arm swing as their rest, blended by how busy they are: a guard keeps a little of the swing, a blow none. |
| A blow's lunge, body turn and lean move the root and pose directly | A lunge becomes a trajectory request (go there, this fast), so the feet step for it from the walk's clips. The blow's own turn and lean stay as an upper-body layer. |
| The steering moves the root at a velocity | Steering gives desired velocity and facing to the walk's springs; the walk moves the root. Circling and backing off become strafes and backward walks. |
| Stumbles, falls and get-ups | Unchanged, entered and left through the walk's knockdown hand-off. |

The torso is shared: the walk drives it, combat overrides it from the spine up in graded weights (about 0.3 spine, 0.6 chest, 1.0 arms), so the walk's sway still shows under a guard. The order each step is: walk, lean, Fight Rhythm, combat torso and arms, pelvis, foot locks and IK, physics.

The Small and the Large use the same walk, retargeted by leg length. Their styles become clip choices and style blends: the Brutal\_To\_Happy clip, for instance, could seed a heavier walk for the Large.

## The walking lab

The work happens on a branch, `locomotion-lab`, in its own scene: the board and one Medium, with nothing else running. Main stays as it is until the lab's walk is good enough to bring over.

Switched off in the lab: combat, the fight loop, tempo, Fight Rhythm postures, perception and the head's look, shields, weapons and the arm rig. The Medium walks unarmed, so its arms come from the clips, as the swing that goes with each walk.

Kept: the rig, the board, the physical body and its authority levels (from stage 5), and the recorder.

The course, played in a loop:

1. Start from standing, walk straight across the board, and stop.
2. Side-step left two tiles facing forward, then right two tiles.
3. Walk straight, turn left 90°, walk straight, turn right 90°, walk straight.
4. Walk a curve to the left, then one to the right.
5. Walk backward three tiles.
6. Turn 180° while walking and walk back.
7. The same course at a slow pace (0.7 m/s, the game's) and a brisk one (1.2 m/s).

Beside the course, a free mode: click a point on the board to walk there, with a key to hold a facing (to strafe or back up while looking somewhere). Later stages add a shove button and a bump into a second unit.

A headless runner, `locolab`, plays the same course without Unity. It writes the measures below and stick-figure films of the lab's walk beside the reference clip it was matched to, so I can check every change before you see it.

## Stages and their gates

&#91;embedded content: the lab's six stages and their gates\]

Each stage ends with the measures run headless and stick films; stages 2 to 6 end with a Unity build for you to play. After the lab, the Small and the Large get the same walk, and then it merges into the game with combat on top.

## Progress, 3 November

Stages 1 to 4 are built on `locomotion-lab`. Stages 5 and 6 work in the Unity lab and are waiting on a playtest. Renier's first playtest (3 November) found the walk nearly right. It found one bug: after walking with the facing held, the walk fell out of step. That is fixed (see below).

| Stage | State | What it does now |
| --- | --- | --- |
| 1. Clip data | Done | 26 Mixamo clips (plus 5 mirrored) are baked into `Resources/Locomotion/LocoClips.json`. `locofilm.py` draws any run as a GIF. |
| 2. One walk on the rig | Done | The Medium is posed from the clip landmarks. Its feet lock on landing, roll heel to toe, and swing onto the clip's path. |
| 3. Directions and running | Done | Forward, side and backward cycles blend at a shared phase, with walk and run tiers. Slow side-steps become crossover steps at speed. Each foot's stance is aligned. |
| 4. Starts, stops, turns | Done | Start clips; stop clips matched to the distance left, so they end on the spot; turns on the spot; 180° pivots. Standing keeps the feet where they stopped. |
| 5. Physical body | Built, to playtest | Unity lab, physical mode: the legs are carried exactly and the torso, head and arms are pinned, as in the fight. Gait timing scales with leg length (the Froude number), so the Small steps quicker and the Large slower, and all three bodies walk the course. |
| 6. Pushes | Built, to playtest | A shove (G) strikes the chest, and the walk steps to catch the body. A bump (the chest knocked off its pose by more than its usual lag, for example by the ball thrown with H) pushes the walk, so the feet follow the body. Pushed faster than it can step (K), it goes down as a ragdoll and stands again where it fell. Getting up properly is the game's job once the two are joined. |
| 7. Joined to the fight | First step built | While a unit is free to walk, the walk animates the root the fight moves. Its feet lock where they land, and the pelvis, spine, chest and head follow the reference walk. The pose pipeline blends into the walk, and the Fight Rhythm, guard, look and arms go on top. A blow, knock-back or stumble hands the feet back to the stance where they stand. In headless fights, planted feet no longer slide in the pose, and fewer than 3% of plants twist more than 10°. Still to do: arm swing under a light guard, lunges as stepping requests, and the start, stop and turn clips in the fight. |

**Headless course, all legs:** planted feet moved at most 0.5 cm (0.03 cm on average), down from 3.7 cm. Released feet landed within 0.7 cm of the clip's spot (95%). Both feet were never off the ground while walking. There were 3 forced steps in 66 s.

**Still rough:**

- Fixed after the playtest: the direction the body travels relative to its facing could run past ±180°. Holding Space while walking backward and turning set this off, and it reached −720° by the end of the run. Walking forward afterwards then blended the side and backward cycles, so the walk was out of step. It is now kept within ±180°, and LOCO\_COURSE=strafing tests it.
- Feet travel matches the body now (1.01 for every body). The 1.13 was a measuring fault: the measure followed the contact point, which jumps from heel to toe as the foot rolls.
- Still rough: a side-step coming to a stand finishes its last step in place, and one knee can swing quickly there. The Large's broad hips leave a side-stepping foot 1.5 cm off its plant twice on the course.

**In Unity:** the sandbox runs the lab instead of the fight. Untick *Locomotion lab* on the BattleSandbox to get the fight back.

- C switches between the course and driving: W A S D, Shift to run, Space to keep facing the same way.
- B switches the body between physical and kinematic.
- G shoves; K shoves hard (a knockdown); H throws a heavy ball at the chest; R restarts; P pauses.
- When Play stops, the run is written to `PlaytestLogs/locolab-(time)/`: the measures in session.md and the film in locolab.json.

## Measures

"No sliding, ever" becomes a number the runner checks on every change, in the posed body and in the physical one.

| Measure | Gate |
| --- | --- |
| A planted foot's travel during one plant (posed and physical) | under 1 cm |
| A planted sole's height off the ground | within ±1 cm |
| Time with both feet in the air while walking | 0% |
| Feet's travel / body's travel, per course segment | 0.95–1.05 |
| Steps that start before the other foot has landed | none |
| Pose jump in one step beyond what the reference ever does | none |
| Body's path against the course | within 0.2 m |
| Hips height, pelvis turn and arm swing against the matched clip at the same speed | within 20% |
| Matches that jump between clips more than 3 times a second | none |

The recorder logs all of these in play too (the foot logging added today already covers planted feet; contact labels, the clip being played and its frame are added), and session.md gets a Locomotion section. The final judge is your eye: each stage ends with a video of the course.

## Questions for you

1. **More clips?** Decided 3 November: the lab walks at the clips' natural speeds (about 1.1–1.7 m/s), so no slow walks are needed. Slow, short-stepped combat footwork is a later, separate set of clips. Still useful now, exported like the first set (Overdrive 50, Arm-Space 50, Mirror and In Place off):
   - A second forward walk at the same pace as the backward and strafe clips, if you find one.
   - True side-steps left and right, facing forward (Mixamo's "Left Strafe" and "Right Strafe", rather than "Strafe Walking").
   - Walking 90° turns, left and right.
   - Turns on the spot, left and right.
   - Forward and backward diagonals, if you find them.
2. **Which walk speed for the lab?** Decided 3 November: the clips' natural pace. The game's walk speed follows the walk, not the other way round.
3. **Which forward walk is the Medium's?** Walking (brisk, long strides), Walking\_1 (deep, heavy hips) or Brutal\_To\_Happy\_Walking (a swagger). Taken as a default when the build started (3 November): the pack's walking, which matches the strafes and the backward walks in pace, with the others kept as styles.
4. **Strafing style.** Your strafe clips turn the hips and feet 40–80° toward where the body is going, a crossover walk. For circling an opponent, do you want that, or square side-steps that keep the hips facing the opponent? Taken as a default when the build started (3 November): both, by speed. Slow sideways movement uses the side-step clips (hips turned about 30°), faster uses the crossover strafes.
5. **Go-ahead.** Stages 1–4 deliver what you asked to see: the Medium walking the whole course with no sliding. Given 3 November, with running added: the whole plan proceeds, stage by stage, on the `locomotion-lab` branch.

## Sources

- Holden, [Code vs Data Driven Displacement](https://theorangeduck.com/page/code-vs-data-driven-displacement): the root adjustment, simulation bone and foot locking
- Holden, [Spring-It-On](https://theorangeduck.com/page/spring-roll-call) and [Fitting Code Driven Displacement](https://theorangeduck.com/page/fitting-code-driven-displacement): springs, path prediction, fitted rates
- Holden and others, [Learned Motion Matching](https://www.theorangeduck.com/media/uploads/other_stuff/Learned_Motion_Matching.pdf): the motion-matching features
- Holden, [Dead Blending](https://theorangeduck.com/page/dead-blending) and [Simple Two-Joint IK](https://theorangeduck.com/page/simple-two-joint)
- Unreal Engine, [Pose Warping](https://dev.epicgames.com/documentation/en-us/unreal-engine/pose-warping-in-unreal-engine), [Distance Matching](https://dev.epicgames.com/documentation/en-us/unreal-engine/distance-matching-in-unreal-engine), [Sync Groups](https://dev.epicgames.com/documentation/en-us/unreal-engine/animation-sync-groups-in-unreal-engine) and [Animation in Lyra](https://dev.epicgames.com/documentation/en-us/unreal-engine/animation-in-lyra-sample-game-in-unreal-engine)
- Johansen, [Automated Semi-Procedural Animation for Character Locomotion](https://runevision.com/thesis/rune_skovbo_johansen_thesis.pdf) (the Unity locomotion system)
- Clavet, [Motion Matching and The Road to Next-Gen Animation](https://gdcvault.com/play/1023280/Motion-Matching-and-The-Road) (GDC 2016, listing)
- Bollo, [Inertialization](https://gdcvault.com/play/1025165/Inertialization-High-Performance-Animation-Transitions) (GDC 2018, listing)
- Bergamin and others, [DReCon](https://theorangeduck.com/media/uploads/other_stuff/DReCon.pdf)
- [PuppetMaster manual](https://root-motion.com/puppetmasterdox/html/page5.html) and [BehaviourPuppet](https://root-motion.com/puppetmasterdox/html/page10.html)
- [Just Cause 2: Physics Meets Animation](https://www.sambuz.com/doc/physics-ics-meets-s-anim-imation-ion-charac-racter-ter-ppt-presentation-926923) (GDC 2010 slides)
- [Capture-point push recovery](https://arxiv.org/pdf/1705.10579), after Pratt and others (2006)
- [UnderPressure](https://ar5iv.labs.arxiv.org/html/2208.04598): foot contacts and a footskate measure
- [Pražák, Hoyet and O'Sullivan (2011)](https://unpaywall.org/10.2312%2FSCA%2FSCA11%2F287-294): when people notice foot sliding
