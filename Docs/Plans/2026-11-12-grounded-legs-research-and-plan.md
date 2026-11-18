# Grounded legs: research and plan

> Copy of the Claude document <https://claude.ai/artifact/P8Du2bu8f7hN8xMaVSY5xP>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 12, 2026 · @Renier

## Summary

The legs look ungrounded because the body is moved first and the feet follow it; the plan turns that round, so the feet are placed where they hold the body up and the body moves over them. Nothing here is built yet. This is the research and the plan for your approval.

- **What the logs show.** In your last fight a foot was planted only 30% of the time and both feet were down 6% of the time. Fighters stood still 2% of the time. On one foot, the centre of gravity was a median 13 cm outside that foot.
- **The main cause.** The walk clips are played in slow motion at fight speeds, so a foot floats for over 0.6 s in 40% of its swings. Sliding along the ground is real but smaller: 8% of the time.
- **Part 1, grounded leg animation (steps 0 to 5).** A new layer gives the legs an opinion at every step: where the weight is, what supports it, and where a foot is needed. Feet are planted or stepping, never anything between. The clips keep deciding the motion and its style.
- **Part 2, physical legs (steps 6 and 7).** First the legs join the physical body under hips the animation still carries. Then the hips are held by a limited support, so a shove moves the whole fighter and the legs catch it. A body that balances with no support at all is not planned.
- **Order.** Part 1 comes first because physical legs need a grounded pose to follow. Steps 0 to 2 are changes to existing code and should remove most of what you see.

Four questions at the end are yours to answer before work starts.

## How the lower body works today

The legs are placed by the animation alone, and nothing in the game knows where the body's weight is. `UnitRig.CenterOfMass()` exists in the code and is never called.

1. **Steering.** Each step the fight decides a velocity and a facing for the unit: walk in, hold distance, circle, back off (`Footwork`).
2. **Walking.** The walk is several recorded walk cycles (forward, sideways, backward; relaxed and guard) mixed together at one shared step rhythm. The rhythm advances by the ground covered, so the slower the body moves, the slower the cycle plays. Strides are stretched or shrunk to fit the speed, from 0.3 to 1.3 times the recorded stride (`Locomotor`).
3. **Clips.** Strikes, reactions and turns play as recorded. The clip's own motion moves the body, scaled 0.6 to 1.4 times so a lunge reaches its target. Changing from one motion to another fades the difference out instead of averaging the two.
4. **Foot locks.** A foot is held where it lands for as long as its clip says it is down, and the hips drop so both legs reach. A foot whose place moves faster than 0.4 m/s is lifted, up to 7 cm, so it is never dragged (`LocoBody`).
5. **Pushes.** A blow changes the walk's velocity and the walk cycles do the stepping. Two bodies that overlap are moved apart at once, and ground given to a blow moves the body at once.
6. **Standing.** The feet are held where the body stopped. If they are more than a tenth of a leg length from the stance, they step back to it one at a time.
7. **The parts.** The leg parts are then put exactly where this pose has them. They are solid but cannot be pushed.

Where a foot lands is decided by the clip and its rhythm. It is never decided by where the body needs a foot.

## What the playtest logs show

Your eye is right, and the cause is mostly not sliding along the ground: the feet are hardly ever on it. Measured on your last fight (12 November, 09:38, four Mediums, 112 s, 296 unit-seconds on their feet):

| Measure | Now | What grounded would be |
| --- | --- | --- |
| Time a fighter is standing (not walking, not in a clip) | 2% | Most of the time between blows |
| Time a foot is planted (on the ground, not moving) | 30% | Over 70% at these speeds |
| Time both feet are on the ground | 6% | Most of the time when the body moves under 0.3 m/s |
| While walking: time with a foot in the air | 85% | Under 40% |
| Swings lasting over 0.6 s | 40% of swings, 76% of all time in the air | None: a step takes 0.25 to 0.45 s |
| In those long swings | the foot travels 60 cm while the body travels 34 cm |  |
| A foot on the ground and moving over 10 cm/s | 8% of the time, 2.8 m a minute per unit | 0 |
| Slides over 5 cm | 73 (15 a minute per unit), worst 42 cm | 0 |
| On one foot, body under 0.3 m/s: centre of gravity outside that foot | by 13 cm (median), a quarter of the time over 28 cm | Inside it |
| Standing still on both feet: centre of gravity outside the support | 38% of the time, worst 13 cm (from only 5 unit-seconds of standing) | Under 5% |
| Both feet down in a clip: the point a foot would need to be on to stop the body is over 10 cm outside the support | 40% of the time | Rare, and followed by a step |

How these were measured: from the recorded position and turn of every part, 60 times a second. The sole is the foot part's heel and ball points; "on the ground" is within 2 cm of it. The centre of gravity is the parts' masses at their middles, without the sword and shield, so it is good to about 3 cm.

Two cautions. The strike clips are motion capture with real hops and leaps in them, so some time with both feet up is correct. And the project's own headless slide figure (2 to 3 cm worst) counted a foot only while its ankle was at standing height, so it missed most of this.

## Why it slides and looks ungrounded

The body is moved first and the feet are made to follow it. A grounded body works the other way round: the feet are placed, and the body moves over them. Seven causes follow from that, in order of how much of the picture they explain.

1. **Walk cycles played in slow motion.** The walk's rhythm advances by ground covered. While walking, the wanted speed is between 0.12 and 0.8 m/s 72% of the time, and the guard's walk clips were recorded at about 1.0 to 1.4 m/s. So a foot hangs in the air for over 0.6 s in 40% of swings, and over 1.3 s in a tenth of them, while the other carries a body whose weight is not over it.
2. **Fighters never stand.** Steering asks for a small velocity nearly all the time (holding distance, circling at 0.35 m/s), and a walk only comes to a stand when the speed falls under 0.12 m/s. Standing is 2% of the fight.
3. **Steps land where the clip's rhythm puts them.** No step is taken because the body needs one, and none is placed where it would hold the body up. A shove changes the walk's speed, and the feet then land wherever the faster cycle has them.
4. **Mixed cycles.** Going diagonally is a mix of a forward and a sideways cycle. Averaging two foot paths gives a path neither clip has, and the foot lock then has to correct the foot back to where the body is.
5. **The body moved at once.** Two bodies kept apart, ground given to a blow, and a combo's pull all move the body without a step. The feet are then lifted up to 7 cm so they hover over instead of dragging.
6. **Clips stretched over planted feet.** A lunge is scaled 0.6 to 1.4 times and turned up to 35° to reach its target while its feet stay where the clip planted them, so the body ends up beside its support.
7. **The upper body moves and the legs do not answer.** The physical torso and arms now lean, give way and carry a sword and shield. The legs only hear about it when the chest is over 4 cm off its pose.

## What others do

Six ideas from games and research bear on this. None needs machine learning except the last, which is why it is not recommended.

| Idea | What it is | Where it is used | Use here |
| --- | --- | --- | --- |
| Capture point | The spot on the ground a foot must reach to bring the body to a stop: the centre of gravity plus its velocity times √(height / g). A push that moves it outside the feet means a step is needed, and says where. | Humanoid robots; [Pratt and others, 2006](https://scispace.com/papers/capture-point-a-step-toward-humanoid-push-recovery-12jssfk8t5) | The core of the legs' "opinion": when to step and where |
| Foot placement in human walking | People put each foot a fixed distance behind and to the side of that same point (there called the extrapolated centre of mass). | Gait research; [Curtze, Buurke and McCrum, 2023](https://cris.maastrichtuniversity.nl/files/177130543/Notes_on_the_margin_of_stability_2023_Curtze_Buurke_McCrum_Preprint_v2.pdf), on Hof's work | The offset that turns "stop" into "keep walking at this speed" |
| Stride warping | Step length is scaled to the body's speed (speed over the clip's speed) instead of slowing the clip down. | [Unreal Engine, Pose Warping](https://dev.epicgames.com/documentation/en-us/unreal-engine/pose-warping-in-unreal-engine) | Already here in part (0.3 to 1.3). The missing half is a floor on the rhythm, so slow movement is short quick steps |
| Procedural stepping over few poses | Steps, weight shift and lean are computed; the animator supplies only a few key poses for style. | Overgrowth; [Rosen, GDC 2014](https://gdcvault.com/play/1020583) | The model for "clips guide, the legs decide" |
| Physical body held up by a helper force | A standing force pushes the torso up, and a balance script pushes the legs to stay under the body; a step handler decides when a step ends and the next may start. | Landfall's games, Totally Accurate Battle Simulator among them; [Landfall, on Game Developer](https://gamedeveloper.com/design/how-landfall-games-finds-the-fun-in-physics-engines) | The honest way to make legs physical without the body having to balance itself |
| Physical body that balances itself | Joint muscles follow an animation, with corrections so it stays up. Early work was hand-tuned and survives pushes on a walk ([SIMBICON, 2007](https://www.cs.ubc.ca/~van/papers/Simbicon.htm)); current work trains a neural network for it ([DReCon, Ubisoft 2019](https://www.ubisoft.com/en-us/studio/laforge/news/VjEIwquaIyEZZSw5RZI0V/drecon-datadriven-responsive-control-of-physicsbased-characters)). | Research, and a few shipped games | Not planned. It needs a training pipeline this project does not have, and it trades away control of the animation |

The project already uses the standard fix for mixing motions (fading the difference out instead of averaging) and foot locking. Those are sound and stay.

## Part 1 design: legs that ground the unit

The order is turned round: the feet are decided first, and the body is then placed over them. The clips still say what the fighter is doing and how it looks doing it. A new layer between the clips and the body decides where each foot is.

&#91;embedded content: how the parts fit · 5 stages, 1 loop back\]

Read it top down. The loop on the right is what makes the legs answer a shove: what really happened to the body goes back into the opinion.

**What the legs know at every step (their "opinion")**

- **Where the weight is.** The centre of gravity and its velocity, from every part's mass, including the physical upper body, the sword and the shield.
- **What holds it up.** The support: the area under and between the planted feet.
- **Where a foot is needed.** The capture point, which is the centre of gravity moved along its velocity by about a third of a second's travel. If that point is inside the support, the body can stop where it stands. If it is outside, a foot has to go there.

**The rules**

1. **A foot is planted or stepping, and nothing else.** Planted means fixed to its spot on the ground, position and turn, sole down or on its toe. It moves only by a step: a planned arc from where it is to where it is going, with a set duration and height. The hover lift and the fades that pull a foot after the body go away.
2. **A step is taken for a reason.** Either the clip says to step and the legs agree it is safe, or the legs need one: the capture point is leaving the support, a planted leg is stretched to its limit, the stance has become too narrow or crossed, or the body has turned too far over a foot.
3. **A step lands where it holds the body up.** The clip proposes where (its own stance shape, its stride). The legs move that spot toward the capture point as far as balance needs, then limit it to what the leg can reach and to ground that is free of other fighters' feet.
4. **The weight shifts before a foot lifts.** The hips move over the foot that will carry the body, then the other foot leaves the ground.
5. **The hips follow the feet.** After the feet are decided, the hips are placed so the centre of gravity is over the support, lowered so both legs reach. The body's speed over the ground is whatever its steps give it. The steering asks for a velocity, and the legs deliver it in steps.
6. **Slow movement is short quick steps with standing between.** A step takes 0.25 to 0.45 s whatever the body's speed. Moving at 0.3 m/s is one 25 cm step every second or so, with both feet planted the rest of the time.
7. **A shove is answered by the legs.** A blow changes the body's velocity, the capture point jumps, and a foot goes to it: one step for a light shove, two or three for a hard one. If no foot can reach in time, the fighter stumbles or falls. That replaces today's fixed thresholds with what the legs can actually catch.
8. **Nothing moves the body without a step.** Bodies kept apart, ground given to a blow and a combo's pull all become pushes that the legs step for.

**How the clips still guide**

| The clip gives | The legs decide |
| --- | --- |
| Which motion: guard, walk, strike, flinch | Whether each foot is planted or stepping |
| The stance's shape: how wide, which foot forward | Exactly where a foot lands |
| When the clip's own feet step, and the arc and timing of its swings | When a step is needed that the clip did not ask for, and when a clip's step is skipped |
| The hips' height, tilt and style; the knees' direction; the feet's turn | Where the hips sit over the feet |
| Leaps and hops in strike clips (left as recorded) | Where the landing foot comes down |

**In the code.** A new class, between `Locomotor` (which keeps producing the clip pose, the contacts and the proposed foot places) and `LocoBody` (which keeps posing the legs to reach their feet). It replaces the walk's distance-driven rhythm at low speed, `LiftMoving`, the unlock fades, `Push` and `Nudge`'s reliance on the cycle's rhythm, and the instant root moves. `Footwork` changes to ask for a place to be, with a dead zone, so a fighter holding its distance stands.

## Part 2 design: physical legs

Recommended: make the legs physical in two stages, A then B, and do not attempt C. "Like the upper body" can mean three different things, and they differ in what holds the body up.

|  | A. Physical legs under carried hips | B. Hips held by a limited support | C. Standing on its own legs |
| --- | --- | --- | --- |
| What holds the body up | The animation carries the hips, as now | A support force with a strength limit pulls the hips toward where Part 1 has them | Only the leg muscles and the ground |
| What a blow to a leg does | The leg gives way and comes back, like an arm | The same | The same |
| What a hard shove to the body does | The upper body leans; Part 1's legs step for it | The whole body is moved, hips included, and the legs step under where it really is | The same, or it falls |
| Can it fall over by itself | No. Falls are decided, as now | Only when the legs' opinion says no step can catch it; then the support lets go | Yes, at any time |
| What it needs | The step 5 machinery, six more parts per fighter | The same, plus the animation following the physical hips | A trained or heavily tuned balance controller |
| Risk | Low | Medium | High |

**A. Physical legs under carried hips.** The thighs, shins and feet join the same physical body as the torso and arms, with the same muscles, slack and limits. They follow the grounded pose from Part 1.

- A planted foot is pinned to its spot on the ground by a joint with a force limit. A blow strong enough to break the pin knocks the foot away, and the legs then need a new step.
- A foot in a step is free and collides with the ground and with other fighters.
- Legs stop passing through each other and through shields and blades. Today a shield pressed on a leg cannot move it.
- A leg knocked aside is reported to Part 1 the way the chest already is, so the legs step for it.
- The pin and the carried hips close a loop through the leg. The pose already fits the leg's length, and the pin is made slightly soft so the two never fight.

**B. Hips held by a limited support.** The hips stop being carried exactly. A support with a strength limit and slack, like a muscle, holds them toward where Part 1 places them. This is the "helper force" of the research table, made honest: it has a limit, and a blow can beat it.

- A shove moves the real body. The legs' opinion reads the real centre of gravity, so the recovery steps answer what actually happened.
- The animation's position follows the physical hips, as it already does for a fallen fighter.
- A fall happens when the capture point cannot be reached in two or three steps. The support then lets go and the existing ragdoll takes over.

**C. Standing on its own legs.** Not planned. Hand-tuned controllers of this kind walk and survive pushes but do not perform a library of fencing strikes. The versions that do are trained neural networks. Either would replace the animation-led design you asked for.

**Unity cautions, from step 5.** Three times in step 5, Unity showed a fault that the headless fights had not. Every new engine feature used here (a pin joint on a foot, foot against ground friction, the hips' support) gets a start-up measurement in Unity and its own lines in the playtest log before anything depends on it.

## The steps in order

Eight steps, each one playtested by you before the next starts, each behind a Combat Feel switch so old and new can be compared in the same scene. Steps 0 to 5 are Part 1; steps 6 and 7 are Part 2.

| Step | What changes | What you would see in Unity | How it is measured headless |
| --- | --- | --- | --- |
| 0. See it | No behaviour change. The fight report and the playtest log record each foot's state, the centre of gravity and the capture point. A Combat Feel toggle draws them in the scene: a dot for the weight, an outline for the support, a cross where a foot is needed. | The legs' opinion drawn under each fighter, on today's animation. You can see where it is unbalanced. | The table in "What the playtest logs show", produced by the report |
| 1. Stand and step | Slow movement becomes short quick steps with standing between. Steering gets a dead zone so a fighter holding its distance stands. | Fighters stand planted in guard and adjust with decisive small steps. No slow floating swings. | Standing time and both-feet time up; no swing over 0.45 s; blows met within 10% of before |
| 2. Planted or stepping | A planted foot is fixed. The hover lift and the fades go. Bodies kept apart, ground given and a combo's pull become pushes. | No foot drags or hovers. A fighter pushed off another steps away. | No planted foot moves over 1 cm; no foot low and moving |
| 3. Weight over the feet | The centre of gravity is computed. The hips are placed over the support. The weight shifts before a foot lifts. A lean the feet cannot hold triggers a step. | The body sits between its feet and visibly shifts its weight before stepping. | Standing: centre of gravity inside the support over 95% of the time. On one foot: over that foot |
| 4. Steps where they are needed | Step targets come from the capture point. A shove is answered by one to three recovery steps, or a stumble or fall when none can reach. | Shoved back, a fighter catches itself with steps in the shove's direction, more for a harder shove. | After a push: the capture point back inside the support within three steps, or a fall |
| 5. The clips' feet | A lunge that was scaled or turned to reach its target re-plans where its feet land. Turns pivot on a planted foot up to a limit, then step. | The front foot of a lunge lands under the body, however far the lunge was stretched. | In clips with both feet down: capture point over 10 cm outside the support under 10% of the time (40% now) |
| 6. Physical legs (A) | Thighs, shins and feet join the physical body. Planted feet are pinned. Behind its own switch. | A blow to a leg knocks it aside and it comes back. Nothing passes through a leg. | Legs inside other bodies: none over 2 cm; planted feet hold within 1 cm; no body put back on its pose |
| 7. Supported hips (B) | The hips are held by a limited support. The animation follows the physical hips. Falls come from what the legs can catch. | A hard shove moves the whole fighter and the legs catch it or it goes down. | Falls only when the opinion predicted one; hips within a few cm of the grounded pose otherwise |

After step 7 comes a clean-up step: remove what the new layer replaced, a performance pass with six fighters, and the README.

Steps 1 and 2 remove most of what the measurements show, and both are changes to existing code. Steps 3 to 5 are the new "opinion". If only one part were built, steps 0 to 2 give the most for the least.

## Risks and open questions

**Risks**

- **The fight's timing changes.** Reach, lunge distance and who gets the first blow all depend on how the body moves. Each step keeps "blows met" and the hit and block counts beside the old ones, and a step that shifts them by more than a tenth is tuned before it reaches you.
- **Computed steps can look mechanical.** The clips' own swing arcs, timing and stance shapes are used for every step, and the guard pack's side-step clips are the model for the small ones. This is the part most in need of your eye.
- **Step chatter.** The centre of gravity now includes a physical upper body that sways. A step decision needs a margin and a minimum time between steps, or the feet will fidget.
- **Crowding.** Six fighters need free ground for their feet. Step targets have to avoid other fighters' feet and legs, and a foot may find no good place.
- **Unity against headless.** Three times in step 5, Unity showed a fault that the headless fights had not. Pins, ground friction and the hips' support are all new engine features for this project.
- **Closed loops.** A carried hip and a pinned foot fix both ends of a leg. If the pose and the physical leg disagree by a few millimetres, stiff joints fight. The pin is soft for that reason, and step 6 measures it first.
- **Falls, getting up, severed limbs.** These already hand the body to the ragdoll and back. Each step re-checks them.
- **Cost.** Part 2 adds six parts per fighter and ground contacts. The last fight had no slow frames after start-up, so there is room, but it is measured at step 6.

**Open questions for you**

- [x] Is it right that fighters stand planted between actions and move in distinct steps? It changes the look of circling from a slow continuous walk to step, step, stand.
- [x] The strike clips contain real leaps and hops. Keep them as recorded, with the legs only choosing the landing spot?
- [x] Part 2: build A, look at it, and then decide on B? Or is B the goal from the start?
- [x] The earlier list (straight-arm roll, Larges back in, retiring the old blocked-arm rule, skill, hits from what touches): the first two are small and could go first. Should the rest wait until after this?

**Decided on 12 November 2026**

- Standing planted and moving in distinct steps is the look wanted. It is better than a slowed walk.
- The clips' leaps and hops stay as recorded for now, with the legs choosing only the landing spot. To be looked at again once it is on screen.
- Part 2: B is wanted, but A is built first because it is on the way to B. Whether to go on to B is decided after A has been playtested.
- The straight-arm roll and bringing the Larges back go first. The rest of the earlier list waits until after the legs.

## Sources

Opened on 12 November 2026.

- Pratt, Carff, Drakunov and Goswami, [Capture Point: A Step toward Humanoid Push Recovery](https://scispace.com/papers/capture-point-a-step-toward-humanoid-push-recovery-12jssfk8t5), IEEE-RAS Humanoids 2006 (abstract page).
- Curtze, Buurke and McCrum, [Notes on the margin of stability](https://cris.maastrichtuniversity.nl/files/177130543/Notes_on_the_margin_of_stability_2023_Curtze_Buurke_McCrum_Preprint_v2.pdf), 2023 preprint. The formula for the extrapolated centre of mass, and Hof's findings on foot placement.
- Epic Games, [Pose Warping in Unreal Engine](https://dev.epicgames.com/documentation/en-us/unreal-engine/pose-warping-in-unreal-engine). Stride warping.
- David Rosen, [Animation Bootcamp: An Indie Approach to Procedural Animation](https://gdcvault.com/play/1020583), GDC 2014 (session page; the talk itself is behind a login and was not watched for this).
- [How Landfall Games finds the fun in physics engines](https://gamedeveloper.com/design/how-landfall-games-finds-the-fun-in-physics-engines), Game Developer. The standing handler, balance script and step handler.
- Yin, Loken and van de Panne, [SIMBICON: Simple Biped Locomotion Control](https://www.cs.ubc.ca/~van/papers/Simbicon.htm), SIGGRAPH 2007.
- Bergamin, Clavet, Holden and Forbes, [DReCon: Data-Driven Responsive Control of Physics-Based Characters](https://www.ubisoft.com/en-us/studio/laforge/news/VjEIwquaIyEZZSw5RZI0V/drecon-datadriven-responsive-control-of-physicsbased-characters), SIGGRAPH Asia 2019.
- The project's own code (`Locomotor`, `LocoBody`, `Footwork`, `UnitRig`) and the playtest log `PlaytestLogs/2026-11-12_09-38-43`.

## Progress: step 0, see it (13 November, overnight)

Built. Nothing in play changes: no foot, hip or step is moved by any of it.

**What is there.**

- Each fighter has a reading of what its legs make of its body, taken once a step: what each foot is doing, where the weight is, the patch of ground the feet hold, and where a foot is needed to stop the body.
- **Show Legs Opinion** on Combat Feel (off by default) draws it on the ground under each fighter. Soles: green planted, red sliding, yellow landing, orange in the air but too low to be a step, blue stepping. White outline: the support. Dot: the weight, green over the support and red outside it. Cross: where a foot is needed, orange when that is more than 10 cm outside the support.
- The playtest log records it for every unit, every step.
- The fight report prints the table from "What the playtest logs show".

**The table, made by the report** (headless fights of six, two seeds, against the Unity logs this plan measured):

| Measure | Unity logs (this plan) | Headless now |
| --- | --- | --- |
| Standing | 2% | 1%, 1% |
| A foot planted | 30% | 28%, 29% |
| Both feet on the ground | 6% | 8%, 6% |
| Walking: a foot in the air | 85% | 93%, 94% |
| Swings over 0.6 s | 40% of swings | 44%, 48% |
| A foot on the ground and moving | 8% | 11%, 10% |
| Still on both feet: weight outside the support | 38% | 32%, 35% |
| Both feet down in a clip: a foot needed over 10 cm outside | 40% | 34%, 29% |

The reading matches the log tool: on the same fight of Mediums the two give a foot planted 38% and 39% of the time, sliding 7% and 8%.

**Also measured: the legs as a body's legs.** The same kind of reading the arms have, with ranges set from the recorded clips. The clips' own legs are never past them. The posed legs are past them 1 to 2% of the time in a strike or reaction: a knee folding forward for a few frames of a combo, and ankles bent up to 70° where the foot is held level under a swinging shin. Nothing is held to these ranges yet.

**Not verified.** I have not seen the view on a screen. Its numbers are checked against the log. If it draws nothing or in the wrong place, the log and the report still hold, and I will fix the drawing from a screenshot.

**Update, later the same night.** The reading now learns how high each fighter's ankles stand during the fight, the way the log tool does, instead of taking it from the rig. With that the report's table is closer to the Unity logs than the one above: a foot planted 27% and 28% (30%), on the ground and moving 8% and 7% (8%), slides over 5 cm 14 and 10 a minute (15), both feet down in a clip with a foot needed over 10 cm outside the support 42% and 37% (40%).

**Found by this, not changed.** By the log, the Larges' ankles stand at 10.3 cm in a fight, where their rig has them at 12.8 cm at rest and the foot's box reaches 12.8 cm below the ankle. That would put a Large's feet about 2.5 cm into the ground. Worth a look in Unity before the legs work starts.

`python3 Tools/Playtest/legsview.py PlaytestLogs/<session> <time> out.png` draws one moment of a session from above, as the view draws it.

## Progress: step 1, stand and step (13 November)

Built, behind the Combat Feel switch `standAndStep`; off, the same seeds give the same fights as before. Not yet seen in Unity.

**What was built.** In its guard a fighter stands until what the fight asks adds up to a step's worth (22 cm), steps it at a step's pace (0.75 m/s), and stands again. A velocity as fast as a walk is walked as before, and so is a shove.

**What changed from the plan.** The plan had the slow step taken with the guard's walking animation, quicker. That did not work, for a reason the plan had missed: the guard's standing clip is a wide, deep stance (a Medium's feet 73 cm fore and aft), and the guard's walking clips are upright walks with the feet under the hips. Walked, every step drew the stance in and every stop stepped out into it again. So a step is now taken in the stance: the body goes at the guard's height and the feet step after it one at a time to where the stance has them, a quarter of a second each, one foot always down. The walked version is kept behind a second switch (`stepsInTheStance` off) to compare.

**Measured** (four fights of six, 60 s, the same seeds off and on):

|  | Off | On |
| --- | --- | --- |
| Standing still | 0 to 1% | 9 to 15% |
| Stepping in the stance | none | 23 to 33% |
| Walking (the walk's animation) | 61 to 67% | 17 to 23% |
| Both feet on the ground | 3 to 4% | 9 to 15% |
| A swing, at the middle | 0.62 to 0.72 s | 0.27 s |
| Swings over 0.45 s, walking or standing | 76 to 79% | 28 to 33% |
| A foot on the ground and sliding | 7 to 8% | 3 to 4% |
| Blows started; blows that met something | 256; 259 | 268; 242 |

**Against this step's measures.** Standing time and both-feet time: up. No swing over 0.45 s: not met. The steps in the stance are a quarter of a second, but a fifth of the time is still the walk's own animation at its recorded 0.5 to 0.7 s a swing: walking off a shove (11 to 13% of the time) and walking as fast as asked (5 to 9%). Those belong to steps 2 and 4. Blows met within 10%: met (7% fewer), but one blow in ten now meets nothing, where nearly every one met a body, a shield or a blade. Cause not found; the likeliest is that a fighter backing off now gets away in a quick step.

**Two findings for later steps.**

- The fighters still step most of their guard time, because the fight asks it. Near its distance a fighter is closing half the time and backing off a third of the time; it is asked nothing a seventh to a quarter of the time. The dead zone this step added (a fighter holds its distance until it is 18 cm off) changed little. More standing would need the tempo's distances and the circling to ask less. That is a fight design question, not a legs one.
- On one foot the weight is 27 to 31 cm outside that foot (14 to 19 before): the stance is wide and the hips stay in its middle while a foot steps. Step 3 is where that is put right.

Not verified: how any of it looks.

## Progress: step 1 seen in Unity; step 2, first part (13 November)

**Step 1 in Unity.** You looked and called the small steps natural. The log of that session reads as the headless fights do (a swing 0.27 s at the middle in both, a foot sliding 4% of the time, standing 42%).

**Step 2 is being done in parts.** It is four changes, and they change the look in different ways, so each gets its own look. The first part is the last line of the step: a fighter pushed off another steps away.

**Built: a shove is stepped in the stance** (Combat Feel `shovesSteppedInTheStance`). After step 1 a fighter in guard still stood up into the upright walk at every push (kept off another body, driven back by a blow) and stepped out into its stance again after: 11 to 13% of the time, a third of the time for a struck target. Now it keeps its stance and its feet step after it. A push faster than the feet can follow one at a time (1 m/s) is still walked until it slows to that.

Two smaller things with it:

- Standing, the feet follow how fast the body really goes, so they also step promptly after ground given to a blow and bodies moved apart.
- A step to its place is one arc: lift, over, straight down. Before, the foot landed a few centimetres in the air and sank. Two new dials: how long a step takes (0.22 s) and how high it lifts (6 cm).

**Measured** (the same four fights, step 1 against now):

|  | Step 1 | Now |
| --- | --- | --- |
| Walking (the walk's animation) | 17 to 23% | 11 to 13% |
| Stepping in the stance | 23 to 33% | 29 to 39% |
| Both feet on the ground | 9 to 15% | 13 to 18% |
| Swings over 0.45 s, walking or standing | 28 to 33% | 16 to 21% |
| A foot on the ground and sliding | 3 to 4% | 3% |
| On one foot: the weight outside that foot | 27 to 31 cm | 37 to 40 cm |
| Blows started; blows that met something | 268; 242 | 256; 239 |

**A fault found and fixed on the way.** A foot held over its landing place while a walk began under it was left behind until its leg was past its reach, and the engine then threw every physical body in the fight (one fight in four, five seconds in). The foot now lets go of its landing place after a twentieth of a second or at its leg's reach. The engine has no guard of its own against a leg stretched past its reach.

**What is left of step 2, by the measures now in the report.**

- Standing, a planted foot's place is already fixed (over 1 cm in 2 to 5% of holds).
- In strikes and reactions a planted foot is let go 20 to 25 times a minute for each fighter because the body has gone 21 to 25 cm from it: the strikes are scaled and snapped onto their targets over feet their clips planted. That, and feet turning with the body past 35 degrees, is step 5's subject more than step 2's.
- The hover lift still acts 20 to 23% of the time while walking and 12 to 16% in a clip.
- Ground given to a blow still moves the body at 4 m/s; in the stance the feet now hop after it.

**The one-foot row is worse and will keep getting worse until step 3**: the more a fighter steps in its wide stance, the more time it spends on one foot with its weight in the middle.

Not verified: how it looks, and Unity.

## Progress: step 3, the weight over the feet (13 November)

**Step 2's first part in Unity.** Your log reads as headless: a step in the stance 0.22 s, the armed targets stepping in their stance 55 to 58% of the time and walking off a shove 9 to 10% (a third before).

**Built** (Combat Feel `weightShifts`; off is step 2 as it was):

- The hips go toward the foot that carries the body while the other steps, and come back when both are down.
- A step taken from standing waits 0.14 s for the weight to go over before its foot lifts.
- Standing on both feet with the weight off what they hold, the hips go under it.

**This step's measure was wrong, and is not met.** The plan asked that on one foot the weight be over that foot. In this stance the feet are 85 cm apart, so that is a 35 cm swing of the hips there and back for every step, and a step is a fifth of a second. A body does not need it: lifted for a fifth of a second it falls 9 cm toward the lifted foot, and that foot coming down catches it. So the full shift (17 cm at the default) is for a step from standing, and a fighter stepping at pace sways 5 cm. The check that matters for a quick step is where the body would need a foot to stop itself as the stepping foot lands. The report has that now.

**Measured** (the same four fights, step 2 against now):

|  | Step 2 | Now |
| --- | --- | --- |
| Still on both feet: the weight off what they hold | 11 to 19% of the time | 4 to 11% |
| On one foot, the body slow: the weight outside that foot | 37 to 38 cm | 26 to 35 cm |
| On one foot, the body going: where a foot is needed, outside that foot | 45 to 46 cm | 26 to 30 cm |
| As a stepping foot lands: a foot needed over 10 cm outside the two | 19 to 20% | 15 to 22% |
| Blows started; blows that met something | 256; 239 | 271; 268 |

Standing with the weight inside the support over 95% of the time: met in three fights of four (89, 94, 96, 96%).

**For step 4.** One landing in six still leaves the body needing a foot over 10 cm from where its two are. Two feet in a stance hold a strip of ground about 10 cm wide, and a body crossing it at a step's pace is ahead of it. Placing the step where it holds the body up is step 4. So is "a lean the feet cannot hold triggers a step", which I left out of this one.

Not verified: how it looks, and Unity.

## Progress: step 4, steps where they are needed (13 November)

Measured first with a new test (`shoves`: one Medium in its guard, shoved at 0.3 to 2 m/s from four sides). Most of step 4 was already so after steps 1 to 3; what was built is the part that was not.

- **Built: a harder shove is stepped in the stance** (Combat Feel `shoveSteppedUpTo`, 1.6 m/s for a Medium). A shove at 1.3 m/s is at rest in two or three steps and 1.1 s, none of it walked; before, it stood the fighter up into the upright walk for a fifth of a second.
- **Tried and taken out: stepping straight to where the body will stop.** No fewer steps, and both feet in the air more often.
- **The hips are brought 2 cm inside what the feet hold**, not to its edge: the weight is off the support 3 to 7% of standing time in four fights (the plan asked under 5%).
- **The plan's measure (back on the support within three steps) cannot be read off a fight**: only one shove in four plays out before the next shove or a strike begins. The `shoves` test is the measure: at rest in one to three steps up to 1.3 m/s, four or five at 2 m/s.

In Unity (target test, 112 s): as headless; weight off the support 3% of standing time.

## Progress: the legs decide a stumble or a fall (13 November)

The rest of step 4 (Combat Feel `legsDecideAFall`, `legsCatchUpTo`). The speed a blow would throw the body at, on top of how fast it is going, is set against what its legs can do: up to 1.6 m/s its stance catches it; up to 4.4 m/s it is thrown back stumbling as before; faster, it goes down and gets up. 4.4 m/s is how fast the backward run in the animation set carries a body.

- Headless, four fights of six: 1, 1, 2 and 3 knock-downs a fight, all from a Large's club.
- In Unity (two target tests, 38 s and 60 s): one and four knock-downs, all a Medium's sword on a Medium (4.9 to 9.0 m/s). More than the headless target test said (none): its blows throw a target far more lightly, not looked into. Each fall and getting up went through without fault.

## Found in Unity: a leg can throw a body out of the world (13 November)

The first of those tests broke at 36.4 s: all six fighters gone at once. A spinning jump's leg (it goes where its pose puts it, and nothing stops it) hooked a standing fighter's sword arm and drew it out to its full length; the engine could not hold that body together, and two steps later it was thrown 10^12 m, the five others with it in the same step. Not caused by the knock-downs; possible in any test since the upper bodies became physical.

- **Built: a net** (Combat Feel `lostBodyPutBackAtOnce`). After every physics step, a body whose pelvis is more than 3 cm off what carries it (it is never more than 2 mm off otherwise), or with a part more than 2.5 m from its pose, is put back on its pose at once and built anew before the next step.
- Headless, with one fighter's sword hand pulled as a leg would pull it: before, the program stopped; now that body is put back and the fight goes on. Never used in fights with nothing pulled, which are blow for blow what they were.
- Not verified in Unity against the leg that did it.
- **This is a net, not the cure.** A leg still cannot be stopped by what it hits. That is a reason for step 6 (physical legs) that the plan did not have when it was written.

Correction to the last point above, after all four fights were run: three of the four never use the net and are blow for blow what they were. The fourth has a real case of it 5 s in (a fighter struck in the air comes down with its leg through another's upper body); the net acts there twice, and that fight differs from there on (91 blows where it had 93).

## Progress: step 2, ground given in the time there is (13 November)

In Unity first (96 s, target test): the net for a lost body was never used; six knock-downs, all clean.

Step 2 has "what moved the body at once becomes a push". The largest such move is ground given to a blow, which was snapped in at 4 m/s. It now takes the time there is before the blow (Combat Feel `groundGivenInTime`).

**It does less than hoped.** A step back takes 0.14 s where it took 0.07 s, half of them under 2 m/s, and as many blows land as before. But both feet still leave the ground on nearly every step back (97% against 94%):

- most ground is given by a sword arm late to its parry, read with a tenth of a second left, so half the steps back are still snapped;
- a foot's step takes 0.22 s, in which even the slower ones carry the body 30 cm, further than the other foot can stay behind.

A third of a metre in a seventh of a second is a hop for a person too. **The plan's wording is wrong here.** What is left is a choice: make it a hop on purpose (both feet off and down together, the body dipping to land), or give ground earlier, which changes the fight.

Left of step 2: the lift of a foot that moves over the ground with its sole down (20 to 23% of walking time). It hides a foot the walk's clip swings too low; it wants the swing's height mended first.

## Progress: a step back is a hop, and a button to see it (13 November)

Renier's choice of the three above: make it a hop on purpose (Combat Feel `stepBackIsAHop`).

- A fighter standing in its guard that gives 12 cm of ground or more hops: both feet leave the ground as the body goes, each goes in one arc to its place under the stance where the body arrives, they come down together, and the hips rise 2 cm in the air and sink 4.5 cm as the legs take the weight.
- Where the body goes and when is unchanged. A step back begun in a reaction clip, or by a fighter already thrown, is as it was.
- Headless: the feet come down together in 51% of a fight's steps back (1% before) and 68% of the button's (11%); as many blows land. Not yet seen in Unity.

**A standing rule from now on, at Renier's asking:** each change comes with a way to see it on demand. Here: the debug panel's **Step back now (2)** button makes every fighter standing in its guard step back at once, and a toggle under it switches the hop on and off.

## Progress: the hop back is off; step 5, the clips' feet (night of 13 to 14 November)

**The hop.** Renier looked at the step back both ways with the debug button and preferred it without the hop. It is off by default (`stepBackHops`); the code and the button stay.

**Step 5, measured first.** The plan's step 5 has two halves: a lunge re-plans its feet, and a turn pivots on a planted foot. The report now says what a sliding foot's hold was doing, in which clip, and how far through it. What it shows:

- **The sliding is in the turns, not the lunges.** Target test, 60 s: 5.8 m of sliding in clips for six fighters, 2.9 m of it in one turn clip; each strike's clip 0.1 to 0.5 m. A strike's pose is built on its root, so however a lunge is stretched or turned the front foot already lands under the body. That half of step 5 is not built, because it is already so.
- **What a turn did.** A planted foot keeps its direction up to 35 degrees, then has to go round with the body. It went round about its ankle, flat, its toes sweeping over the ground.

**Built: a planted foot pivots on the ball of the foot** (Combat Feel `feetPivotOnTheBall`). Toes held, heel going round.

- Target test: sliding in clips 5.8 to 3.6 m; the turn clip 2.9 to 0.5 m; a foot's sliding in all 1.3 to 0.8 m a minute; blocks, parries and hits as they were.
- Four fights of six: sliding in clips 20.7 to 15.5 m, in the turn clips 13.9 to 6.4 m.
- It does not change the fighting: fourteen short fights run both ways have 96 against 108 blows landed, not distinguishable from no difference. (Four long fights alone looked like a large difference; they were chance.)

**A fault in the measuring, fixed.** Every knock-down counted as two to four metres of foot sliding in the clip the fighter got up into. Slide figures in this plan from the day the knock-downs came in are too high by that.

**To see it:** the debug panel's **Turn now (3)** button and the toggle under it. Not yet seen in Unity.

## Progress: step 6, physical legs: built, and switched off (night of 13 to 14 November)

Built behind Combat Feel `physicalLegs`, **off by default** because nobody has seen it. The debug panel has a toggle for it.

**What it is.** The thighs, shins and feet join the physical body: six more parts hung from the carried hips, each driven to the pose by a muscle, held to the pose as the trunk is. A planted foot is pinned where it is planted by a spring (120 kN/m, giving past 1500 N), not a lock, as this plan's design asked.

**Measured headless** (target test and four fights of six, legs off against physical legs):

- Bodies inside other bodies by over 2 cm: 42 times in the four fights (mostly a foot or a shin, 8 to 12 cm deep) against 5 (a hip in a hand, in one fight). In the target test: 8 times against never.
- A planted foot is within 0.3 to 0.6 cm of its place 95% of the time, 0.8 to 2.4 cm 99%.
- The feet's sliding, and blocks, parries and hits, are as they were. No body lost. Eight knock-downs went through.
- About half as much again to compute, headless.

**Against this plan's measures for step 6:** legs inside other bodies none over 2 cm: met. Planted feet within 1 cm: 95% of the time, not 99%. No body put back on its pose: met.

**Not done: a blow to a leg hardly moves it.** The plan has "a blow to a leg knocks it aside and it comes back". A struck leg is let go and kicked, and moves 1 to 8 cm for a tenth of a second: the joints are driven by speed, and a kick is undone at once. It would have to go into the pose, as an arm's knock does.

**Not verified in Unity at all.** The foot's pin is a new use of the engine for this project (a sprung joint from a carried body to a physical part), which this plan says to measure in Unity before anything depends on it. Cost in Unity is not measured.

**Left of step 2 and not done:** the lift of a foot that skims the ground. It hides a walk's swing that the animation has low; mending it changes how the walk looks. Left as it is.

**Step 7 (supported hips)** waits on Renier's look at step 6.

## Progress: physical legs on, the tube test, and what comes after (14 November)

**Physical legs seen in Unity and switched on.** A 74 s target test with them on: no fault, no body lost, 83 frames a second. A planted foot holds within 1.4 cm 95% of the time there (0.3 to 0.6 cm headless): looser, not yet looked into. On by default (`legsArePhysical`).

**The tube test** (debug panel: Tube test (5), Hit (6), R to reset). The three targets with no attackers, each struck by a tube of a set momentum and speed, with sliders for how hard, how fast, how high, nearer or further, and the targets turned so it meets the front, a side or the back. A toggle takes the tube as a blow of the fight's as well, or as the physics only. Not yet seen in Unity.

**What it shows, headless.** 60 N·s on a chest (more than a sword cut), the physics only: the tube stops dead and the chest is not a millimetre off its pose. Taken as a blow, the same hit drives the fighter back 60 cm; 300 N·s knocks it down. A blow's weight does nothing to the physical body by touching it: the hips are carried and each joint is driven straight back to its pose. Everything a blow does today is done by the fight on top.

**Agreed with Renier, the order from here:**

1. The tube test (done).
2. Research and a plan for controlled falls (bracing with the hands, guarding the head; not a rag doll).
3. Step 7, supported hips, with Renier's idea of a hit force put on the struck part and shared to the parts around it, tuned with the tubes.
4. Staggers on real legs, then the falls built from the plan.

**Progress, 14 November (the tube test's first try in Unity).** Hit (6) sent three tubes and nothing moved. Not the tubes: any fresh set of fighters during play (R, the tube test) stopped the engine for the rest of that play. A destroyed fighter was still told of each physics step, threw reaching for its body, and the engine was never stepped again (5,432 exceptions in 91 s of the log). Fixed: a destroyed unit takes its fighter off the engine's steps, and one that throws is taken off and logged while the engine goes on. Headless check: 3 left told before, 0 after. Not yet verified in Unity. Next: the tube test looked at in Unity, then the research and plan for controlled falls.

**Progress, 14 November (the tube test works in Unity; falls planned).** The tube test ran clean in Unity: 27 hits from 60 to 600 N·s, no error; at 600 N·s the head went 38 cm off its pose and sprang back. The falls now have their own plan, [Controlled falls: research and plan](https://claude.ai/artifact/L1KorNskCqjo8dT32DSguE). Its step 1 is built: falls on demand from the tube test's panel, and every fall measured. Today's fall: the head reaches the ground in 16 of 16. Its steps 2 to 11 wait for this plan's step 7 (hips held with a limited strength), which is next, together with the hit force.

**Progress, 14 November (step 7: the hips held, not locked; a blow's force).** The physical body's hips are now held where the animation has them by a spring of limited strength (5 swings a second, at most 3 times the body's weight) instead of being locked there, behind `hipsAreHeldNotLocked`. Tubes, physics only, headless: 60 N·s moves the hips 0.6 cm, 600 N·s 3 cm, and they come back. With it comes the hit force (`hitForce`, 1): the blow's momentum again on the part struck, half on the parts joined to it, a quarter on the next. It shoves the body through the hips' hold and turns those joints aside (11 degrees at most for a 60 N·s blow, 28 at hit force 3). Two fights of 45 s ran clean: no body lost, the hips 0.3 to 0.7 cm from the pose on average, 4 to 22 cm at the hardest blows. Not yet seen in Unity. Not done: the hips let go for a fall, and the hips turning under a blow. Next: staggers on real legs, then the falls plan from its step 2.
