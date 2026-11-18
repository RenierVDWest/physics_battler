# Ragdolls: Plan

> Copy of the Claude document <https://claude.ai/artifact/AsWxyew3t7oX8YFtrSVYFg>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 1, 2026 · @Renier

## Summary

Your two complaints have causes your three logs from this morning bear out, and the second makes the first much worse.

- **Limbs past their limits.** On bodies lying on the ground, elbows and knees sit past their limits a third to half of the time, by up to 170 degrees. The limits are set, but four things keep them from holding:
  - the elbow and knee side limits are too narrow for PhysX to hold steadily;
  - joints switch from free to limited while already bent too far;
  - the upper arm is nearly seven times lighter than the chest it hangs from;
  - a unit's own limbs pass through its own body.
- **Shaking and being thrown about.** A lying body's parts are mostly at rest until a standing unit's leg comes within half a metre. Then they often move at 3–7.5 m/s, and up to 28 m/s. Standing legs are kinematic: to PhysX they are infinitely heavy, so they shove whatever they touch out of the way at any cost. That also forces the joints past their limits.

**The fix, in short:**

- **Joints:** limits PhysX can hold, brought back into range before they lock, and the light arms weighted more heavily in the joint solve.
- **Weight:** a cap on how hard a walking leg can push a lying body, more weight and damping once a body settles, its own limbs colliding with it, and units walking round bodies rather than through them.
- **Measures** in the logs, and a sandbox key that makes the situation on demand.

**When:** in the next update, together with the hands (9b). The wrist limits there come from the same table, and a pose that stays within its limits is what keeps the switch to ragdoll clean. This work can't be tested here: there is no PhysX outside Unity, so its checks come from your playtest logs.

## What your logs show

I measured every physical joint against its limits, and every lying body's speed against how close a standing unit's legs were. This covers your three playtests from 09:00, 09:07 and 09:08 this morning.

### Joints past their limits

Shares of the time a joint is more than 10 degrees past its limit, and the worst case. Down means knocked down and alive; dead means a ragdoll that stays down.

| Joint | Down: past by >10° | Down: worst | Dead: past by >10° | Dead: worst |
| --- | --- | --- | --- | --- |
| Elbows | 40–58% | 173° | 0–97% | 174° |
| Knees | 15–38% | 175° | 1–3% | 170° |
| Ankles | 11–29% | 134° | 0–3% | 135° |
| Wrists | 8–17% | 130° | 0–1% | 106° |
| Shoulders | 3–10% | 124° | 0–22% | 130° |
| Neck | 7–15% | 56° | 1–15% | 93° |
| Hips, spine, chest | 1–6% | 76° | under 1% | 51° |

Elbows and knees are the worst. Those are the two joints whose side limits are the narrowest: 8–10 degrees. Elbows stay bent too far even on the dead, often by a steady 50 degrees or more. The other joints are worse in a body that just went down than in one that has lain dead a while, so the moment of the switch matters too.

### Bodies on the ground, and the legs around them

The fastest part of a lying body, 1.5 s or more after it went down (by then it has settled), against how close the nearest standing unit's leg is:

| Nearest standing leg | Fastest part, median | Fastest part, 95th percentile |
| --- | --- | --- |
| Over 1 m | under 0.2 m/s for 5 of 6 bodies | up to 6.8 m/s |
| 0.5–1 m | 0–11 m/s | up to 12.8 m/s |
| 0.25–0.5 m | 0–7.5 m/s, most over 3 | up to 10.2 m/s |
| Under 0.25 m | 0–9.8 m/s, most over 1.5 | up to 28 m/s |

Left alone, a body mostly lies still: PhysX puts it to sleep. When a unit walks into it, it is often thrown about at the speed of a sword swing. (Small bodies stayed still more often, mostly because few units walked over them.)

## Why it happens

1. **Walking legs are infinitely heavy to PhysX.** Standing units' legs and pelvis are kinematic: the pose moves them, and PhysX gives them unlimited mass. A leg that steps into a lying arm moves it out of its way however hard that has to be. The arm's joints can't hold against that, so it is bent past its limits, and the body jolts each step the leg pushes. Nothing steers units round bodies on the ground, either: the push that keeps bodies apart skips units lying down.
2. **Side limits too narrow to hold.** The elbows allow 10 degrees to the side and the knees 8 degrees each way. Unity's ragdoll guide says side limits under about 5–15 degrees aren't stable. Those are exactly the two worst joints.
3. **Joints locked while already too far.** While a unit stands, its joints swing free (a pinned limit would only fight the pose), and the pose doesn't respect the limits: the wrists and some guards sit past them. At the moment it goes down, the limits switch on with the joint already outside. PhysX then has to drag it back, which throws the limb, or never quite does.
4. **Light limbs on a heavy body.** The upper arm (2.4 kg on a medium) hangs from the chest (16.3 kg): 6.7 to 1. Unity says 2 to 1 is fine and 10 to 1 is jittery. The shoulders are near the edge, so a push on the arm moves it more than the joint can manage.
5. **Its own limbs pass through it.** A unit's own parts never collide with each other: while it stands, the pose keeps them apart. On the ground nothing does, so an arm folds through the chest or a leg through the other.
6. **Nothing slows a body that has fallen.** It keeps the damping of a standing body (0.05 linear, 0.1 angular). Once a push gets it moving, it slides and rolls freely, and it reads as light.

## Research

PhysX can hold a ragdoll's limits if four things are true. The side limits mustn't be tiny, joined parts mustn't differ much in weight, the joints must start inside their limits, and nothing infinitely heavy may press on a limb. Unity can also cap how hard any single contact pushes. That is the tool for walking legs.

| Source | What it says | What we take |
| --- | --- | --- |
| [Unity: ragdoll stability](https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/ragdoll/ragdoll-stability) | Avoid small Y and Z (side) limits: the least stable angle is about 5–15°. A 2× mass difference between joined bodies is fine, 10× is jittery. Raise solver iterations to 10–20 for jitter, velocity iterations for bounces. Turn off Enable Preprocessing. Lower the depenetration speed where bodies overlap. | Side limits of at least 15°. Heavier arms in the joint solve. 20 position and 8 velocity iterations while lying. Preprocessing is already off. A gentler depenetration while lying. |
| [Unity: ConfigurableJoint](https://docs.unity3d.com/ScriptReference/ConfigurableJoint.html) | Projection corrects only locked axes, and it isn't physical. Mass scale and connected mass scale scale the bodies' inverse mass and inertia in the joint solve. | No projection: our axes are limited, not locked. Mass scale on the shoulders and hips. |
| [Unity: contact modification](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/physics/contactmodifyevent) | Colliders with modifiable contacts pass their contact pairs to a callback, on any physics thread, before the solver. There, each contact's [maximum impulse](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/modifiablecontactpair.md) can be capped, or the contact ignored. | Cap how hard a walking leg can push a lying body. It still pushes, but it can't fling. |
| [Unity: ArticulationBody](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/articulationbody) | Articulations are solved in reduced coordinates by a Featherstone solver: each body moves only along its joint's free axes, so joints can't stretch. | The most robust ragdoll there is, but it means rebuilding how bodies are posed and pinned. Not now; kept in mind if R1 falls short. |
| [PhysX D6 joint](https://nvidia-omniverse.github.io/PhysX/physx/5.4.1/_api_build/classPxD6Joint.html) | The twist limit can be asymmetric. The two swing axes share a symmetric cone (or a pyramid). | Our layout already matches: the main bend (elbow, knee, hip, shoulder raise) is the asymmetric twist axis, and the cone takes the rest. |

## Plan R1: joints that hold their limits

A body on the ground keeps every joint within a human range. It gets there by having limits PhysX can hold and by starting inside them. Arms that are too light are made heavier in the joint solve, and its own limbs no longer pass through it.

1. **Limits PhysX can hold.** No side limit under 15 degrees. The elbow's side limit goes from 10 to 15 degrees, and the knee's twist and side from 8 to 15. The elbow still bends one way only, and so does the knee. That way is the main axis, which can be one-sided.
2. **The pose kept within the limits.** The pose limits every joint, not only the wrist (Plan D of milestone 9). Anything a joint can't take moves to the next joint up, as the wrist plan does. A unit then goes down already inside its limits. One limit needs checking first: the left hip sits past its 40-degree twist two-thirds of the time standing, in the stances. Either the stance turns the foot too far, or the limit is tighter than a person's (about 45 degrees each way). I'll measure which and fix it.
3. **No snap when the limits come on.** When a joint switches from free to limited, its current angle is checked. If it is outside, the limit starts where the joint is and closes to the anatomical limit over 0.3 s. PhysX then brings it back smoothly, instead of throwing the limb or leaving it stuck outside.
4. **Light arms weighted more in the joint solve.** The shoulder joint scales the chest's weight down in its solve (connected mass scale about 2.2), so the arm and chest meet at about 3 to 1 instead of 6.7 to 1. The neck and hips are already under 3 to 1.
5. **Its own limbs collide with it.** When a unit goes down, the parts that don't share a joint collide with each other: forearms and hands with the chest, belly, head and thighs, and each leg with the other. When it starts to get up, they go back to ignoring each other, as the pose keeps them apart.
6. **More solver work while lying.** 20 position and 8 velocity iterations while lying (12 and 4 standing).

### Checks (from your playtest logs)

- While lying: every joint more than 10 degrees past its limit under 2% of the time. The worst case is under 25 degrees. Elbows and knees are counted on their own.
- At the switch to ragdoll: no part speeds up by more than the blow gave it.
- Limbs inside the body while lying: none deeper than 3 cm.

## Plan R2: heavy, solid bodies on the ground

A body on the ground still gives to someone walking into it, but only as a heavy body would: it is nudged and rolled a little, never flung. Units mostly walk round bodies rather than through them, and once a body has settled it stays settled.

1. **A cap on what a walking leg can do.** Contact modification is switched on for the colliders of every body lying on the ground. Where a standing unit's leg (kinematic) touches one of its parts, the contact's push is capped at about 0.3 m/s worth of that part's weight each step. The leg still moves the limb, but slowly, as if the limb were heavy. The list of who is lying and who is standing is copied for the physics threads each step, since the callback runs off the main thread. Contacts between lying bodies and the floor, weapons or other lying bodies are left alone.
2. **Walking round bodies.** The steering in the fight loop treats each body on the ground as an obstacle, a gentle push away from its torso and legs. A unit walks round one when it can and steps through only when it has no other way. (Bodies are skipped there today.)
3. **Heavier once settled.** A body that has fallen and slowed (its hips under 0.5 m/s for 0.3 s) has its damping raised over half a second: linear from 0.05 to about 1, angular from 0.1 to about 4. Its friction on the floor goes from 0.6 to 0.9, and it separates from overlaps at 1 m/s instead of 3. A body that is struck again (a blow, a cleave, a thrown body) gets its light damping back for a moment, so hits still throw it.
4. **A little muscle tone.** While lying, each joint has a weak damper on its drive (no spring), scaled by the parts' weight. Limbs fold down and settle rather than flop and bounce. A unit that is only down keeps a bit more tone than a dead one.

### Checks (from your playtest logs)

- With a standing unit's leg within 0.5 m: a lying body's fastest part has a median under 0.5 m/s and a 95th percentile under 1.5 m/s. Today those are 3–7.5 m/s and up to 28 m/s.
- Bodies still give: a lying arm walked into moves a few centimetres, not none.
- Units walking through a lying body's torso: under 10% of the times they pass within a metre of one.
- A knocked-down unit still falls as freely as now, judged by the speed of its first half second down.

## Plan R3: measuring it

I can't run PhysX here: the test runner's stand-in physics has no joints. So this work is judged on your playtest logs, and the logs need to measure it.

- **Log columns.** Two new columns each step. One gives how far the worst joint is past its limit, and which joint that is. The other gives the fastest part of a lying body and how close the nearest standing leg is.
- **Summary.** A Ragdoll line in each unit's summary: time past the limits, the worst joint, and part speeds with and without a leg near.
- **Analysis.** A ragdoll section in analyze.py, from the two scripts behind the tables above, so every playtest prints the before-and-after.
- **A sandbox key for the situation.** A key that knocks two units down in the middle of the board, kills one, and makes the others walk back and forth across them for 20 seconds. You get the hard case on demand, rather than waiting for it in a fight.
- **Tests here.** The parts that are pure maths: limits closing in over 0.3 s, the push cap, and the steering round bodies. The PhysX side is judged on your logs.

## Schedule

All of it goes into the next update, which becomes **M9b: hands, wrists and ragdolls**. The joint limits are one job: the same table and the same pose clamp keep a wrist from looking broken while standing and a ragdoll's elbow in range on the ground. Today's logs already give the before-numbers, so a single playtest of M9b shows the after.

The order inside M9b:

1. R3, the measures and the sandbox key, so everything after it is measured.
2. R1, the joints: limits, the pose clamp for every joint (the wrists among them), the eased switch, the shoulder mass scale, self-collision.
3. R2, the weight: the push cap, steering round bodies, settling, muscle tone.
4. The hands from milestone 9's Plan D: the closed-fist meshes and the grips.
5. The shield against its own arm and hand (your screenshots): the shield's body check also covers the shield arm's upper arm and forearm. The grips fix the hand.

M9c (solid weapons and the shield get-ups) follows as planned. Say if you want anything changed before I start M9b.
