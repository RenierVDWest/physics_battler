# Weapon arm rebuild plan

> Copy of the Claude document <https://claude.ai/artifact/RGb9d1i935pRxtbLFXvS8o>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 2, 2026 · @Renier

## Where we are

The weapon arm has had eight fixes in two days and still twists, because nothing in it decides where the elbow goes. The arm's shape is whatever a numerical solver lands on for the hand and weapon it is given. The physical arm then follows each part separately, so the joints only get in the way.

What the playtests show:

- The Large's club hand ends up beside its head with the wrist cocked hard, the club pointing forward or down, or the forearm upright in front of the face with the club standing straight up (screenshots of 2 November, 06:22 to 07:49).
- The physical guard arm sat 20 to 30 degrees off its pose for seconds at a time, in every playtest from the night held arms got joint limits; it had been 2 degrees before.
- Shield arms getting up pointed the shield at the enemy at arm's length, through the body.

The fixes so far, each aimed at a symptom:

| When | Fix | What it treated |
| --- | --- | --- |
| 1 Nov 17:20 | The arm carries on from the last step; the weapon arm is posed exactly in blows | Shaking swings |
| 1 Nov 19:33 | The arm solved in joint angles, limits as hard edges | Unnatural weapon arms |
| 1 Nov 22:02 | A fitted shield arm with an elbow-direction term | Shield through the body getting up |
| 1 Nov 22:12 | Held arms keep their joint limits | Wrists twisted after blows |
| 2 Nov 06:44 | The weapon arm held on its pose 0.4 s after each blow | Arms trailing their return to guard |
| 2 Nov 07:20 | Held joint limits widened to the shape PhysX holds | Guard arm stuck off its pose |
| 2 Nov 07:40 | Held arms' joints free again | The same, still stuck |

Why they kept missing:

1. **No elbow control.** A hand position and a weapon direction leave the elbow free to swing round the shoulder-to-hand line. The solver picks a point on that circle from its last answer, its limits and its cost terms, so the same blow can come out with different elbows, and the wrist takes whatever twist is left over.
2. **The weapon dictates the whole hand.** The hand's rotation is set by the weapon's direction and roll. Nothing lets the weapon roll a little, or moves the elbow instead, when that would break the wrist.
3. **The physical arm is pulled part by part in world space.** Each of the upper arm, forearm and hand is servoed toward its own posed position. The joints between them are only constraints, so joint limits fight the servos, and free joints let parts twist past what an arm can do.
4. **We test without PhysX joints.** The test harness moves bodies freely, so problems in how the physical arm follows its pose only show up in playtests.

## Progress, 2 November: the rig works, blows need re-authoring

The arm rig is built and sound, and guards on it are clean. Blows are not right yet, because they are defined weapon-first. The decision below is yours before blows go further.

**Done** (on main: the rig alone; on the `arm-rebuild` branch: everything wired in):

- **The arm rig.** One exact solve from the controls, with no memory. It passes its tests:
  - an arm made inside its limits is found again within 2 mm and 0.5 degrees, elbow included;
  - no goal takes a joint past its limits, and the elbow only bends about its hinge;
  - the same goal gives the same arm, whatever came before;
  - smooth goals give smooth arms, nearly straight arms included. It takes 26 microseconds a natural goal, 0.7 ms an impossible one.
- **The elbow null is an angle round the arm.** 0 is hanging relaxed, +90 out, 180 up, −90 across. As a point, the null flipped the elbow whenever the arm pointed through it, which happened in every wind-up.
- **Moves key the elbow and the wrist.** The wrist keys are forearm turn, flexion and deviation, at wind-up, contact and follow-through. Each style has a guard elbow, and each weapon a grip roll. Setup 22 fills them.
- **Guards on the rig** (all three bodies): the wrist is inside its limits in every step, the elbow at its null, the arm clear of the torso.

**Found:**

- **The paths ask for impossible wrists.** At the null arm, the weapon paths asked the wrist for 95–116° of flexion, up to 126° of deviation and up to 174° of forearm twist. A wrist does about 60°, 20–35° and 80°. The old solver hid this by quietly pointing the weapon elsewhere, which is what you saw as twisted wrists.
- **Keyed wrists fix the wrist, not the reach.** They bring blows inside the wrist's limits. But putting the striking part on the weapon path still asks for arms that are out of reach, up to 45° past the shoulder or up to 13 cm into the torso. The arm also jumps where no elbow place matches the key: a whole forearm, hand and club is a long second bone, and its swivel doesn't map cleanly.

**Proposed: blows authored hand-first, as an animator rigs them.**

- Each blow keys the **hand's path** (the IK effector) at wind-up, contact and follow-through, with the **elbow null** and the **wrist** keyed at the same moments. The weapon simply follows the hand.
- The **contact hand** is solved once, when the blow is planned: the arm pose that puts the striking part on the aim with the contact wrist and elbow. The hand's path runs through it, so blows still land exactly.
- The arm each step is a plain two-bone solve to the wrist. It is smooth by construction, with nothing to search and nothing to flip.
- **What stays:** aim, lunge, body turn and timing. The weapon's arc becomes a result, not a target, except at contact.

* [x] **Your call:** go hand-first for blows (recommended), or something else?

## Progress, 2 November (later): hand-first is in, blows need keying

You chose hand-first. It is built on the `arm-rebuild` branch (commits cba23a73 and 88f6b514). 131 of 151 tests pass.

**Done:**

- **Hand-first blows.** The hand moves through its keys on arcs round the shoulder; the thrust drives in a straight line. There is no corner at contact, and the wrist and elbow keys are timed with the hand.
- **Arms inside their limits.** In every blow, at every body size, the wrist and shoulder stay inside their limits on every step, and the elbow sits on its keyed null.
- **One limit model.** Tests and the playtest logs measure the arms with the rig's own limits.
- **The arm keeps off its own body.** The hand is moved out until the arm and blade clear the torso. A blow that could only land through the attacker's own body misses in its dry run, and the planner drops it.
- **Blows land.** Near contact, the hand is shifted by however far the contact pose would now miss the aim. Planning reaches no further than the arm can carry the weapon with the contact wrist. The thrust chambers by the hip and drives in a line, and the backhands wind up in front of the chest.
- **The arm bench.** Physics Battler > Move Preview > Arm keys: jump to a blow's wind-up, contact or follow-through and set the hand (drag it in the Scene view), the wrist (three sliders within its limits) and the elbow null. The Scene view draws the arm, the elbow's circle and null, the hand's path and its keys.

**Found:**

- **The default keys are too small.** On a sword cut, the striking part at the end of the wind-up is only about 8° from where it strikes, so swings sweep 130–175° where the big-swing tests want 280°.
- **The old paths can't be matched.** A key search against the old paths shows several of them can't be reached with a wrist inside its limits. The keys need authoring by eye, not copying from the paths.

**Remaining:**

- [ ] Key the swings on the arm bench (yours, or a first pass by me)
- [ ] Re-check the swing-arc, shield-block, knock-down and sever tests against the new keys
- [ ] Steps 5–7: shield and off hand on the rig, joint motors and frames in PhysX, cleanup

## Progress, 2 November (afternoon): the swings keyed

You asked me to key the swings myself and carry on. Everything is on the `arm-rebuild` branch.

**How the keys were made.** For each swing I designed the pose at the wind-up and at the follow-through (where the hand, the elbow and the blade are). For the cut, the blade is drawn back high behind the shoulder; for the cleave and smash, it hangs behind the head; for the backhands, it is wound across the front; for the rising blow, it starts low behind the hip. A fitter (`keyfit`) then adjusts the keys near those poses for arc, landing, clearance, wrist limits and speed through contact. I checked every blow in stick-figure films (`armfilm`).

**Fixed on the way:**

- **Guard pops:** the guard no longer pops. The weapon's roll in the hand is now continuous, the torso is measured exactly near the grip room, and the elbow never folds past 125°.
- **Overhead flips:** the elbow's swivel reference flipped on overhead wind-ups. Its singular point now sits low behind the far hip, where no arm goes.
- **Contact correction:** it now moves the contact key itself, so the hand's path carries it with no kink. The keys are splined against the blow's own pace.
- **Combo hand-overs:** each link starts from where the arm really was.
- **Shield prediction:** the shield reads the blow as it will be played. The path is the blow's posed way, laid out when it starts.
- **Swing timing:** swings now strike 30% quicker and wind up 30% slower (setup 23).

**Still open (test failures to look at after a playtest):**

- [ ] Forehand arcs measure 240–260° where the big-swing test asks 280°
- [ ] The club smash lands with the club angled up from a low hand; it wants keying by eye on the arm bench
- [ ] Bystander and heavy-swing hit tests: the new blows come in differently (a horizontal cut reaches the target's arm before a body set in its way)
- [ ] A few landings in long sequences of blows miss by 4–8 cm
- [ ] Steps 5–7: shield and off hand on the rig, joint motors and frames in PhysX, cleanup

## Progress, 2 November (evening): bodies on the ground, steps 1 and 6

**The 16:44 playtest.** The arm looked right. The weapon arm's upper arm and forearm sat 1–2° off the pose (median) in guard and 0° in blows. The hand was the weak spot: 8° median, 32° at the 90th percentile.

Two bodies on the ground went wild:

- A downed Large was thrown 7.8 m up. A Medium got up beside it, and its foot came down on the Large's cut-off shin.
- A downed Medium shook for a second against a standing Large's legs.

Both had elbows 100°+ past their limits. With the arm joints weighting the light limb toward the heavy body, a joint held past its limit makes momentum from nothing: the Large kept accelerating upward for half a second.

**Fixes for bodies on the ground:**

- On the ground, the joints weigh both sides as they are.
- After 1 s on the ground, no part goes faster than 4 m/s or rises faster than 2 m/s unless a blow lands. A part already faster only slows.
- Cut-off limbs count as bodies on the ground: a standing foot nudges them, never pins them.

**Step 1, one limit model.** The elbow and wrist joints get the arm rig's own frames, set up when a unit starts:

- The elbow is a hinge about the rig's elbow axis. Before, PhysX bent it about the part's X axis, 16–24° off the hinge the pose uses, because the arms rest in an A-pose. On the ground, with the side axes locked, that mismatch alone forced the elbow sideways.
- The wrist twists about the forearm and bends in a round 60° ellipse. This contains the rig's 60×35° ellipse whichever way PhysX turns it.
- The shoulder stays as it was. Its cone around the relaxed reach fits no PhysX ellipse.
- A test checks every elbow and wrist the rig can pose against the joint's limits, for all three bodies.

**Step 6, joint motors.** The weapon arm, out of blows, is held by each joint's slerp drive aimed at the pose's turn in the joint's frame, with no world pull:

- Its limits stay on.
- Its weight is carried.
- Each motor is strengthened for what hangs from it.
- The pose's turning is fed forward.

Blows stay kinematic. A sandbox switch puts the arm back on the world servo to compare, and the log has each arm joint's error in joint space (jerr\_\*). The off arm stays on the servo until step 5.

**For the next playtest:**

- [ ] The weapon arm in guard with the motors (jerr\_UpperArmR, ForearmR, HandR; target under 5° median), and against the switch turned off
- [ ] Bodies on the ground: no launches, no shaking
- [ ] Elbows on the ground bend like hinges

## Progress, 2 November (late): the 17:26 playtests, steps 5 and 7

**The 17:26–17:29 playtests.** No body on the ground flew off after its first second. Two things were still wrong:

- **A launched Small was thrown 2.7 m up by its own joints.** It happened a fifth of a second after a club launched it at 4.8 m/s, inside the free second. Fixed: no part rises faster than it was sent up.
- **The arm motors were too soft.** The arm settled at about 2 Hz where 7 was asked. In guard the joints sat 5–9° off their pose (joint space), against 1–3° on the world servo. The motors are now asked three times quicker.

**Step 5.** These now go through ArmRig:

- **The shield arm:** the shield's turn is kept, the elbow moves for it from its null, and the shield rolls about its face last.
- **The bare off hand:** a keyed straight wrist.
- **Get-ups:** the off arm's keys and the shield arm. Raised off the ground, the same arm is moved up.

`ArmRig.SwivelToward` turns an elbow pole into a null. It keeps the elbow where it is as the pole runs along the arm, which is where get-up elbows used to flip.

The weapon arm's get-up keys stay on the two-bone solve. Its weapon is laid out by the hand, up to 100° past the wrist's range, so on the rig the weapon either pointed into the ground or the elbow flipped. Those keys want reworking so the arm lays the weapon out, not the wrist.

**Step 7.** `ArmFit` and `WeaponArmSolve` are deleted, along with PosePipeline's `NaturalArm`, `SmoothWeaponArm`, `MovePose` and both `ClampJoint`s. The README's current state is now the arm rebuild.

The test suite is unchanged: 137 pass, and the same 15 fail as before.

**For the next playtest:**

- [ ] The shield arm and the bare off hand in guard and in blocks
- [ ] Get-ups, face up and face down, with and without a shield
- [ ] The weapon arm on the stiffer motors, and with the switch off for comparison

## Progress, 2 November (night): planning spread out, three close blows

Done overnight, with no playtest yet:

- **Planning spread over steps.** A new blow is planned a few poses at a time, about 4 ms of each physics step, not all at once (`MovePlanner.Job`). The playtest recorder writes one file per step. Combo links are still planned in one go; that is the one hitch source left that I know of.
- **The shield bash** (Medium, `MoveStyle.ShieldBash`). The shield's face goes straight into the target and back on the off arm, as the jab does. It shoves more than it hurts.
- **The butt strike, or punyo** (Small with a club, `MoveStyle.ClubPunyo`). The butt end below the fist rises to a chamber above the aim, then hammers down onto the face, neck or collarbone, on the weapon arm. It is made in code for every club and never cuts. It only reaches targets about as tall as the Small.
- **Tests.** There is a new test for each blow. The full suite has 140 passing and the same 15 known failures.
- **Headless fights.** In three fights the jab came up 9 times and the bash and butt strike not at all. The fight's spacing doesn't yet seek the close range these blows need.

**Left for a playtest:**

- The weapon arm's get-up keys on the rig, which need keying by eye.
- Whether the stutters are gone.
- How the bash and the butt strike look when they come up.

## How it's done properly

The established answer has three parts: an analytic arm solve with an explicit elbow control, a hand rotation split into forearm twist and wrist bend with clear limits, and a physical body driven joint by joint, not part by part in world space. All three are standard. None of them is in our arm today.

### The arm: two-bone IK with an elbow pole

- **Reach sets the elbow bend.** The elbow angle depends only on the distance from shoulder to wrist (law of cosines). [Juckett](https://ryanjuckett.com/analytic-two-bone-ik-in-2d); [Soechting et al. 1995](https://www.ece.uvic.ca/~bctill/papers/mocap/Soechting_etal_1995.pdf): "the elbow angle depends only on the distance of the target from the shoulder."
- **The elbow swings on a circle.** With shoulder and wrist fixed, the elbow can only move round a circle about the shoulder-to-wrist line. One angle, the swivel, picks the point ([Tolani, Goswami and Badler 2000](https://www.ece.uvic.ca/~bctill/papers/mocap/Tolani_etal_2000.pdf)). That angle is the arm's whole remaining freedom.
- **The "null" is the swivel control.** Softimage's [up-vector constraint](https://download.autodesk.com/global/docs/softimage2014/en_us/files/ik_ConstrainingtheChainxd5sUpVectorDirection.htm) and Maya's [pole vector](https://help.autodesk.com/cloudhelp/2023/ENU/Maya-CharacterAnimation/files/GUID-9942FFB5-65C2-46E2-B5A3-297667A9FB5D.htm) set it with an animated null. Unity's [TwoBoneIK hint](https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.0/manual/constraints/TwoBoneIKConstraint.html), Final IK's [bend goal and swivel offset](https://root-motion.com/finalikdox/html/page2.html) and Unreal's [pole vector](https://dev.epicgames.com/documentation/en-us/unreal-engine/node-reference/ControlRig/BasicIK) do the same.
- **Known pitfalls, known fixes.** The chain flips when the target crosses the pole line, when the pole sits near the shoulder-to-wrist line, or when the arm is nearly straight. The fixes:
  - Place the pole in the chest's frame, not the world.
  - Keep it as an angle and smooth that angle.
  - Hold the last angle when the arm is near straight.
  - Ease the reach near full length ("soft IK").
- **Natural elbows hang low.** Arm-motion studies measure the swivel from "the elbow at its lowest possible point" ([Rosen group 2014](https://users.wpi.edu/~zli11/papers/C2014_EMBC_Rosen_TaskRelevance.pdf)). People turn the forearm first and swing the elbow out only when the forearm runs out of turn. A model built on this predicts real arms within 5 degrees ([Kim et al. 2012](https://users.wpi.edu/~zli11/papers/C2012_ICRA_Rosen_KinematicRedundancy.pdf)).
- **Joint limits become a range of swivel.** Each shoulder and wrist angle varies smoothly with the swivel, so the limits mark out a range of valid swivel angles. The solver picks the valid angle nearest the one asked for (Tolani). Limits are kept by moving the elbow, not by breaking the wrist.

### The hand and wrist on a held weapon

- The hand's rotation comes from the weapon (grip line plus edge direction). Relative to the forearm it splits into a **twist** about the forearm (turning the palm over) and a **swing** (bending the wrist), the same split PhysX uses ([PhysX joints](https://nvidia-omniverse.github.io/PhysX/physx/5.4.1/docs/Joints.html)).
- Human ranges, which the arm should keep well inside:

| Motion | Range | Source |
| --- | --- | --- |
| Forearm twist (palm down / palm up) | 80° each way | [Physiopedia](https://www.physio-pedia.com/Range_of_Motion_Normative_Values) |
| Wrist flexion / extension | 60–65° each way | Physiopedia; [Crisco 2011](https://pmc.ncbi.nlm.nih.gov/articles/PMC3016043) |
| Toward the thumb (radial) | about 20° | Physiopedia; Crisco 2011 |
| Toward the little finger (ulnar) | 30–40° | Crisco 2011 |

- What a wrist can't take goes elsewhere, in a fixed order: forearm twist first, then the elbow's swivel, and last the weapon's own roll about its grip. That order follows Rosen and Tolani above; no game source documents it, so it is our design choice.

### The physical body: muscles in joint space, pins only where needed

- **The standard rig splits muscles from pins.** [PuppetMaster](http://root-motion.com/puppetmasterdox/html/pages.html), the standard Unity tool, drives each joint toward the animation's local rotation with the joint's own motor (a "muscle"). It adds a world-space "pin" force only where needed, usually only at the hips, and calls that pin "an unnatural force." Unreal's [physical animation](https://dev.epicgames.com/documentation/en-us/unreal-engine/API/Runtime/Engine/FPhysicalAnimationData) has the same split.
- **Joint-space motors can't fight each other.** Each one only turns its own joint. Our servo pushes every part toward its own spot in the world, so in a chain the servos and joint limits work against each other. Unity's manual warns that chains of joints "might encounter unsolvable scenarios" and settle off target. That matches the 20 to 30 degree stalls; the link is our reading of it, not a quote.
- **Unity has a chain made for this.** [ArticulationBody](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/articulationbody) solves a chain as a whole (reduced coordinates): no stretch at the joints, and motors and limits per joint axis in joint space.
- **The pose must respect the physical limits.** Shape and pose must use the same limit model as PhysX: twist about the joint's X axis, swing limits as an ellipse ([CmConeLimitHelper](https://raw.githubusercontent.com/NVIDIA-Omniverse/PhysX/main/physx/source/common/src/CmConeLimitHelper.h)). Then every pose is reachable and limits can stay on.
- **Long props need their own pin.** PuppetMaster notes that a weapon at the end of a long chain is "less accurate and less stable." It gives props an extra pin and keeps hands on a stronger hold.

## The current weapon arm, audited

The twisted arm in the screenshots is in the pose itself, not only in the physics. After a blow, the solver wedges the Large's arm against five joint limits at once and stays there for up to three seconds. It is freed only when the next blow starts. The weapon path that drives the arm is sound; the arm solve and the layers bolted onto it are what need replacing.

### How the arm is posed now

- **The guard is a weapon target, not an arm pose.** The weapon's guard grip, the fight rhythm's guard offset, a captured hand loop, posture keys and mood are summed into a grip position and weapon rotation. The same solver as in blows then reaches it. Two sources set the guard, and the rhythm's offsets are documented in arm lengths but added in metres.
- **A blow is a weapon path.** It is a swing plane through a pivot near the chest and the aim point, tilted per move, run through wind-up, hold, strike, follow-through and recovery. The weapon points out from the pivot through the striking point, lagged by `wristLag`. This part is sound.
- **The arm is a numerical fit in 9 joint angles.** A damped least-squares solver balances seven weighted wishes: reach the grip, point the weapon, keep the wrist easy, a pole, body clearance, stillness, and the planned contact shape. Joint limits are hard walls. The solve starts from last step's answer and may move each joint at most 25 degrees a step in the guard.
- **The elbow has no real control.** One fixed pole, out, down and slightly back, is the same for every move and phase. At 0.04 per degree it is the weakest of the seven wishes, so the other wishes decide where the elbow goes.
- **Nine layers edit the arm after the solve:**
  - spine pre-bend and lean-after re-solves;
  - a grip push-out from the torso, and a spring that moves a blade lying against the head;
  - a 12 Hz smoothing filter;
  - the ground clamp;
  - the combo link blend, and the hand-over smoothing that re-turns every upper-body part after the pose is composed;
  - knock offsets;
  - the rewind of stopped blows.
- **Three arm solvers in parallel.** The weapon arm, the shield arm (an elbow sweep for the straightest wrist) and the bare off hand and get-ups (a second least-squares fit) are solved three different ways.

### What the pose does (stub, three bodies, several fights)

|  | Large (club) | Medium (sword) | Small (club) |
| --- | --- | --- | --- |
| Guard elbow points | down and out | straight out sideways ("chicken wing"); up in 25% of steps | out sideways |
| Guard steps with a joint at a limit | 43% | 29% | 23% |
| Elbow bent sideways to its 15° end | 22–70% of steps, by body and phase |  |  |
| Wrist | bend 18° | bend 22°, at the edge of its easy range; roll 77° of 80 | shoulder rolled 76° |
| Wedged after a blow | 0 to 6 times a fight, up to 7 s in all | short ones | none seen |

- **The wedge.** The shoulder sits at -160 / -90 / 73, the elbow fully bent with the forearm rolled to its 60-degree end, and the wrist bent 40. The hand is beside the head with the forearm across the face, and the club points forward instead of up and back. That is the first screenshot.
- **The backhand is the worst move.** Its wind-up raises a nearly straight arm to the shoulder's limit beside the head. In the strike the elbow flips up, and the wrist bends 51–55 degrees with its roll pinned at its end.
- **The pose jumps.** In the guard, the elbow can swing up to 154 degrees round its circle in one step. In blows, parts turn up to 75–119 degrees a step.

### How the physical arm follows

- **In blows it is kinematic.** It is moved exactly onto the pose (capped at 15 m/s), and held so for 0.4 s after each blow.
- **Otherwise each part is pulled in world space.** Upper arm, forearm and hand are each servoed toward their own posed spot. Pins are 0.6 (Large), 0.7 (Medium) and 0.75 (Small).
- **Joints are free while held.** The physical arm has no limits of its own, so it can only be as good as the pose.
- **No collisions with itself or its weapon.** The main weapon is a trigger with no mass while standing.

### Keep, replace

| Keep | Replace |
| --- | --- |
| The move data, aim frame, swing path and five-phase timeline | The 9-angle least-squares arm solve, its weights and its walls |
| The weapon's direction from the path (with `wristLag`) | The fixed, near-weightless pole |
| The planner's dry tries and clearance checks | Warm start, step caps and the near-strike-only re-solve (the wedge) |
| Kinematic blows with speed caps | The nine layers that edit the arm after the solve |
| The joint limit table, as data | The two guard sources (weapon guard + rhythm offset) |
| Body clearance, as a measurement | Three separate arm solvers |
|  | The per-part world-space servo and free joints while held |

## The new design

The arm becomes a rig, as an animator would build it. Three controls go in: where the wrist goes, how the hand turns, and an elbow null. One exact solve turns them into joint rotations. Everything that changes the arm changes the controls, and nothing edits the joints after the solve. The physical arm follows with joint motors, inside the same joint limits the solve uses.

&#91;embedded content: the arm rig · inputs, three controls, one solve, the physical arm\]

The elbow null is the control that was missing: today the elbow goes wherever a weighted fit happens to land.

### The controls

1. **The wrist target.** This is the grip point from the weapon (the weapon path in a blow, the guard in between), moved back to the wrist through the hand.
2. **The hand's turn.** It comes from the weapon: the grip line must hold, and the edge's direction is wanted. Each weapon says how far its roll about the grip may give: a club's roll hardly shows, a sword's edge does.
3. **The elbow null.** This is the swivel angle: where the elbow sits on its circle round the shoulder-to-wrist line, in the chest's frame. 0 is elbow hanging lowest, + is out to the side, 90 straight out, 180 up, and − is in across the body. It is a curve over each move's time (keys at guard, wind-up, hold, contact, follow-through), with a guard value per fighting style. An angle can't flip the way a point near the arm's line can.

### The solve: exact, in order, no memory

1. **Reach sets the elbow bend** (law of cosines), eased near full stretch so a straightening arm doesn't pop.
2. **The null places the elbow** on its circle.
3. **The elbow is a hinge.** Upper arm and forearm are aimed down their bones, with the elbow bending only in the plane of the arm. Today's arm bends the elbow 15 degrees sideways in up to 70% of steps; the new arm never does.
4. **The hand's turn splits into forearm twist and wrist bend.** The twist (palm over) and the bend (flexion, toward thumb or little finger) are each clamped to the human range in the research table.
5. **What the wrist can't take moves the elbow.** The solver looks along the elbow's circle for the nearest swivel where shoulder, forearm and wrist all fit (Tolani's method: about 72 tries per arm, cheap). Only if none fits does the weapon roll about its grip, within that weapon's allowance.
6. **What still doesn't fit is reported, not hidden.** The planner's dry tries see it and drop or adjust a blow whose arm can't be made, before it starts.

The same controls always give the same arm: there is no warm start, step cap or carried state, so there is nothing to wedge. Smooth controls give a smooth arm, and the step-to-step turn is limited only by how fast the controls move.

### Everything else becomes an input

| Today it edits the arm after the solve | In the new design it moves |
| --- | --- |
| Ground clamp turns wrist and shoulder | the weapon target, lifted before the solve |
| Knock offsets added to the target | the wrist target and the null, as springs |
| Combo link blend and hand-over smoothing re-turn the joints | the three controls, blended from link to link |
| 12 Hz smoothing filter | nothing: the controls are smooth already |
| Spine pre-bend and lean-after re-solves | the body's pose, set once before the arm |
| Grip push-out and the blade-at-the-head spring | the guard's wrist target, authored clear of the body |
| Two guard sources (weapon guard + rhythm offset) | one guard per style: wrist target, hand turn, null; the rhythm moves those, in the same units |

### One limit model, the same in the pose and in PhysX

- **Each joint gets a frame that suits it.** The shoulder twists about the upper arm, with its swing cone centred on the arm's middle range. The elbow is a hinge. The wrist twists about the forearm, with its range toward the thumb and little finger centred (Unity's sideways limits are always symmetric).
- **The solve clamps with the exact shape PhysX holds** (twist range, swing ellipse). Every pose can then be reached, and the limits stay on without fighting it.
- **The unit prefabs are rebuilt with the new joint frames** (a Setup step, as at Setup 12 and 19). Bodies on the ground use the same frames, so ragdolls must be rechecked.

### The physical arm: joint motors, not world pins

- **Muscles, not pins.** Each arm joint is driven toward its posed rotation by its own motor (the joint's slerp drive, aimed at the pose's local rotation), as PuppetMaster does. Motors can't fight each other or the limits, so the 20 to 30 degree stalls can't happen.
- **No world pins on the arm.** The world-space servo comes off arm parts. The chest stays held as now, and the arm hangs from it.
- **Blows stay kinematic for now.** Hits are judged from the weapon as PhysX moves it, so the weapon arm keeps being placed exactly in blows. A muscled blow is a later option, not part of this rebuild. If the muscles track well, the 0.4 s hold after blows comes off.
- **ArticulationBody is the fallback.** It solves a whole chain at once with joint-space drives, but changes every unit. We switch only if the muscled ConfigurableJoint arm still drifts.

### One arm solver for every arm

The weapon arm, the shield arm, the bare off hand, the arms in get-ups and the off-hand blows still to come in milestone 10 (jab, butt strike, shield bash) all use the same rig, with their own controls. The shield arm's "straightest wrist" sweep and the second least-squares fit go.

### The null you can see and move

- **A gizmo in the Scene view.** Each arm shows its elbow null, the elbow's circle, and the shoulder cone and wrist range.
- **Keys in the move asset.** Each move's elbow curve sits in its inspector, and changes show on the next blow, like the other move fields today.
- **An arm bench scene.** One unit on a stand plays its guard and each move slowly, on a loop, so the arms can be judged and keyed by eye.

### Starting values for the elbow

About 25 degrees out for the guards, nothing higher. Up and out in the wind-up of overhead blows and forehand swings; lower and in across the body through and after contact. For the backhand, the elbow forward and in across the chest in the wind-up, opening out through contact. These are a first pass from technique for you to correct; no source measures elbows in striking.

## Steps

Seven steps, each checked before the next. Three of them stop for your playtest, because the harness has no PhysX joints. No more milestone 10 moves until step 7 is done; the off-hand blows then come almost for free on the new rig.

| # | Step | Checked in the harness | Stops for your playtest |
| --- | --- | --- | --- |
| 1 | **Joint frames and one limit model.** Each arm joint gets its new frame and centred limits, shared by the solve and PhysX; Setup rebuilds the unit prefabs. | Every pose inside the limits is inside PhysX's shape (by construction, tested across the whole range). Ragdoll tests still pass. | Yes: the arm bench and a joint probe show the limits where the gizmo draws them, and bodies on the ground still settle. |
| 2 | **The arm rig.** The analytic solve from the three controls, with the gizmo. | Reaches the wrist exactly when it can. The elbow sits on the asked swivel unless the wrist forces it off. No joint past its limits, ever. No elbow bent sideways. The same controls give the same arm. Smooth controls give a smooth arm. No flip near a straight arm. | No |
| 3 | **Elbow curves per move, one guard per style.** Fields in the move and style assets with the starting values; Setup fills them. | Every move's arm, at every 1% of its time, for all three bodies: no limit reached except where the weapon roll gives way, which is reported. | No |
| 4 | **The weapon arm on the rig.** Guard and blows; the least-squares solver and the nine layers after it go; the ground clamp, knocks and combo blending move onto the controls. | The `arms`, `aim`, `swings`, `combos` and fight runners at least as good as today, with no wedge (the audit's wedges: Large 0 to 6 a fight, now 0). Wrist bend within its range in every step. | Yes: how the arms look, guard and every move. You correct the elbow keys on the arm bench. |
| 5 | **Shield arm, off hand and get-ups on the rig.** | The shield and get-up tests, unchanged in what they ask. | No |
| 6 | **Joint motors on the physical arm.** Muscles to the posed rotations, world pins off the arm, limits on. | Only the maths (the drive's target formula against Unity's convention probe). | Yes: the playtest log measures each arm joint's error in joint space, and how often a joint sits on its limit. Targets: guard under 5 degrees median, no stalls. |
| 7 | **Clean-up.** The old solvers and dead code removed; the README's arm section rewritten. | The full suite. | No |

## Open questions and risks

### For you

- [ ] **How should the elbow null be authored?** A curve per move (keyed like an animated null), or the five phase values only (guard, wind-up, hold, contact, follow-through)? The plan assumes a curve with those five keys to start.
- [ ] **How much may each weapon roll about its grip?** The plan assumes a club about 30 degrees and a sword about 10, since its edge must land.
- [ ] **Should the elbow keys be authored per body as well as per move?** The Large's boxing guard and the Small's eskrima may want different elbows for the same blow.

### Risks

- **The shoulder cone is a simplification.** A real shoulder's range changes with how far the arm is rolled; one ellipse can't follow that. The elbow keys keep arms in the middle of the range, and the arm bench shows where it pinches.
- **New joint frames change ragdolls.** Bodies on the ground use the same joints, so falls and get-ups need a look in the step 1 playtest.
- **Muscle strength against accuracy.** The guard is held by motors, not placed. Too soft and it sags; too stiff and hits look wooden. A strength per body is tuned in the step 6 playtest. Blows stay placed, so hits don't depend on it.
- **Unreachable blows show up.** Some of today's moves may ask for an arm no person can make, as the backhand does. The planner drops or reshapes those; a move that keeps failing is reworked, not forced.
- **Time.** This is several sessions of work. Milestones 11 to 13 wait until it is done.
