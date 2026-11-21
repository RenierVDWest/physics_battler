# Shock waves through the body on a hit: research and plan

21 November 2026. The live document: https://claude.ai/artifact/FED9yrpFr7tz59LvbJLnd6 (this is its copy in the
repository; where the two differ, the live one is newer). **Nothing here is built: this is the research and the plan.**
Its film (3b2b, about five minutes): https://claude.ai/artifact/5qRcjDgh6jbCCJivN8R1pK

## Bottom line

You are right: fake it. A hit should start a wave at the part that was struck. The wave travels out through the body,
part by part, and each part shudders as it passes, then settles. It is drawn on the pose only. It changes no physics,
no hit and no gameplay.

- **It is one small formula per body part**, not a simulation: when the wave arrives, how far it swings, how fast it
  rings, how fast it dies.
- **The game already has half of it.** A hit bends the whole body round the blow for a moment. What is missing is the
  travel (far parts move later) and the ring (peaks and valleys).
- **Your three sliders are in**: frequency, how fast it dies, strength. I add two: how fast the wave travels, and how
  much each joint passes on.
- **Real bodies do this**, at about 15 shudders a second and 1 to 4 cm. You asked (20:04) for it to be over the top,
  more than other games: so the first values are about three times as far as a real body moves, slower so the eye can
  follow it, and it rings longer.
- **Six steps**, the first in the duel lab on one arm chain.

## What you asked for

Your words, 21 November, 20:01: "add shock waves to the impacts ... to get some really juicy and impactful reactions
... this does not need to have gameplay or physics implications ... the only way to do this is to not expect the
physics simulation to cause this, but to fake it. So that you can have full control of how the parts vibrate as the
shock waves are sent through the body." And at 20:04: "I want this to be a bit over the top, more so than you will
see in other games."

How I read it:
- **A wave, not a shake.** "If the chest is hit it will ... move back, dragging the upper arms, then lower arms etc as
  a wave travels through the body with its peaks and valleys." The struck part moves first, the others follow in turn.
- **It starts where the blow lands.** "If the upper arm is hit then that wave will go into the chest and to the lower
  arm, hand." It travels both ways from the struck part.
- **Sliders.** "For the frequency of the wave, how fast it dissipates. The wave strength as in how much\far it will
  move the body parts."
- **Looks only.** Nothing in the fight changes: not who is hit, not how hard, not who falls.

## What the game does on a hit today

A hit already moves the pose in four ways. None of them travels, and only the slow ones ring.

| What | Where in the code | How it moves | What is missing |
|---|---|---|---|
| The bend | `ClipFighter.AddImpact`, `Bend` | Every part is pushed along the blow, the struck part most, less with distance (out to 1.2 m). Out in 0.08 s and straight back | All parts move at the same moment. It does not ring |
| The lean | `ClipFighter.Recoil` | The trunk rocks about the hips on a spring at 1.6 swings a second | One slow rock of the whole trunk |
| The head snap | `Recoil` | The head snaps on its own spring at 2.4 a second, only when the head is hit | Only the head |
| The arm knock | `KnockArm`, `KnockArmBy` | A struck arm or shield is knocked aside and comes back at 2.5 a second | Only that arm |
| Knees and trunk give | the impacts plan's step 4 | The hips sink and the trunk folds under a heavy blow | A slow give, no shudder |

All of these are "a pose only, no physics", in the code's own words. The physical upper body then follows the pose
through its motors. The shock wave is the bend with two things added: **a delay that grows with distance along the
body, and a ring instead of a single push.**

## What a real body does when it is hit

| Fact | Number | Where from |
|---|---|---|
| Flesh on a limb swings on after an impact ("wobbling mass") | 3 to 55 swings a second; 3 mm to 4 cm against the bone; turns 2 to 11 degrees | Schmitt and Günther, 2011 |
| A forearm struck on a surface | Main swings near 14 and 20 a second; skin markers moved up to 1.7 cm; it overshoots, then two small swings, then still | Pain and Challis, 2002 |
| A tensed arm | About half the movement of a relaxed one | Pain and Challis, 2002 |
| Relaxed flesh in general | About 15 swings a second; bone rings at 200 to 900 | Play and others, 2022 |
| How fast a ripple travels through muscle | A few metres a second (about 2.6 relaxed, up to about 8 tensed). **Not checked on the page**: from its search summary | Chernak and others, 2013 |

So the real ripple is short (about three visible swings in a fifth of a second), small (1 to 4 cm), fast (15 a second
is four pictures a swing at 60 pictures a second), and it travels (chest to hand in about a quarter of a second).
Animators have drawn it by hand for ninety years: Art Babbitt's "successive breaking of joints", the motion unfolding
from the chest through the shoulder and elbow to the fingertips.

## How games fake it

Nobody I found sends a travelling wave through the body. They do one of three simpler things.

| Technique | Who | What it does | What we take |
|---|---|---|---|
| A push that falls off with distance and springs back | Easy Procedural Hit Reactions, an Unreal add-on | On top of the animation, no physics; the hit's place, direction, bone, strength; an influence radius and falloff; sliders for strength, radius, damping, stiffness, mass, a limit per bone. No travel, no delay between bones | The inputs and the limits per part. Our bend is already this |
| The whole figure vibrates during the freeze | Super Smash Bros (Sakurai's column on hitstop) | The struck character shakes while both are frozen, large at first and shrinking; the attacker a little too; bigger when the camera is far; **the hit boxes stay still while the picture shakes** | Run the wave through our freeze. Shake the attacker a little. Keep everything that decides hits still |
| Springs you can set by feel | Daniel Holden, "Spring-It-On" | An exact formula, stable at any step; set by a half-life and a frequency | Our sliders are exactly these two |

The other road is to let the physics do it. You ruled that out, and I agree: our physical upper body's motors are
there to hold it to the pose, and its checks (the shield's tremble, a sword through his own shield) are already at or
over their limits in the dance. **Why ours can go further:** a wave that visibly travels, joint after joint, with the
far ends whipping late, is the hand-animator's trick done by code on every hit.

## The model: one faked wave through the body's parts

1. **Where it starts.** The part that was struck, and the way the blow was going.
2. **How far along the body each part is.** Along the bones, not through the air: chest to right hand is about 0.8 m.
3. **When it arrives.** Distance divided by the wave's speed. Until then the part does not move: that is the "dragging".
4. **How it swings.** Along the blow's way and back, as a spring dying away.
5. **How big.** The struck part gets the full strength; each joint passes on a share; light ends are loose.

    u_p(t) = A · k^(j_p) · m_p · exp(−(t − t_p)·ln2 / h) · sin(2π f (t − t_p)),   t_p = d_p / c

The movement of part p is the strength A, times the pass-on k once for each of the j joints crossed, times the part's
looseness m, times a fade with half-life h, times a swing at f swings a second, from its own arrival time: its
distance d along the body divided by the wave's speed c.

**A worked example with the first values** (a heavy blow on the chest; planned, not built or measured):

| Part | Along the body from the chest | The wave arrives after | Its first swing |
|---|---|---|---|
| Chest | 0 m | 0 s | 9 cm back |
| Head | 0.3 m | 0.06 s | 9 cm (loose) |
| Upper arms | 0.25 m | 0.05 s | 7 cm |
| Forearms | 0.55 m | 0.11 s | 5 cm |
| Hands, sword, shield | 0.8 m | 0.16 s | 5 cm (loose) |
| Hips | 0.45 m | 0.09 s | 5 cm |
| Thighs | 0.65 m | 0.13 s | 4 cm |
| Shins | 1.1 m | 0.22 s | 3 cm, feet stay planted |

Each part also **tilts** as the wave passes through it. A real body moves 1 to 4 cm; nine at the chest is about three
times real, on purpose.

## The sliders

| Slider | What it does | First value | Range | A real body |
|---|---|---|---|---|
| **Frequency** (yours) | Swings a second as the wave passes | 7 | 3 to 20 | About 15 |
| **Dies away in** (yours) | Half-life: the time for the swing to halve | 0.15 s | 0.03 to 0.6 s | About three swings in a fifth of a second |
| **Strength** (yours) | How far a heavy blow moves the struck part; lighter blows less | 9 cm, never more than 15 cm | 0 to 15 cm | 1 to 4 cm |
| **Wave speed** | How fast it travels along the body | 5 m/s (chest to hand in 0.16 s) | 1 to 20 m/s | A few m/s |
| **Each joint passes on** | The share of the wave that gets through a joint | 0.75 | 0.3 to 1 | Not known |
| **Loose ends** | How much further the hands, head, sword and shield swing | 1.3 times | 1 to 2 | Not known |
| **Tilt** | How much a part turns as the wave runs through it | 1.5 degrees for each cm | 0 to 3 | 2 to 11 degrees in all |
| **How over the top** | One dial on the strength and the dying away together | 1 | 0 (off) to 2 | About 0.3 |

Two switches: the wave on or off (old), and the wave runs through the freeze on a hit.

## Where it plugs in, and what it must not touch

- **Where.** In `ClipFighter`, next to the bend (`AddImpact`, `Bend`): the same call starts a wave. Its offsets are
  added to the pose as the last thing before the body is shown.
- **The body as a chain.** Hips, spine, chest, head; each upper arm, forearm, hand and what it holds; each thigh,
  shin, foot. Link lengths from the rig, so each body size gets its own timing.
- **What starts a wave.** A blade or club on the body; a blow on a shield (from the shield hand up that arm); a bash, a
  kick, a thrown body. Later, small: the attacker's own sword arm when his blow is stopped.
- **Must not touch:** anything that decides a hit (colliders, the blade's live path, the shield's place read the body
  without the wave); the feet (the wave fades at the shins); our own checks (crossings and tremble must read the body
  under the wave); the fight of six, until you say.
- **To find out first.** The seen upper body is physical and follows the pose through motors: a fast swing on the pose
  may come out smaller. Step 1 measures it. If too little shows, the wave is added to the drawn body after the physics.

## The steps

| Step | What you get | In the lab | What headless can measure |
|---|---|---|---|
| **1. One wave down one arm** | A blow on the chest sends a wave through the shoulder, upper arm, forearm and hand of each arm. All eight sliders work | "Duel" > "Strike at": a button "Ring him: chest" that starts a wave with no blow. "The wave: on / off (old)" | For each part: when it first moved, its first swing, how many swings, when it was still, against the formula. How much of the posed wave the drawn body shows |
| **2. The whole body, from any part** | A hit anywhere starts it there. A blow on a shield runs up the shield arm | "Ring him: head / arm / shield / hips / shin" | The same table for every starting part. The feet do not move |
| **3. The whip** | Parts tilt as the wave runs through them. The sword and the shield whip at the end of the chain | Levers for the tilt and the loose ends | The sword tip's and shield rim's swing against the hand's |
| **4. Living with the rest** | It runs through the freeze. It sits on the bend, the lean and the head snap without doubling them. A combo's big one rings most. The attacker's own arm shudders when he is stopped | "The wave in the freeze: on / off". "The attacker shudders: on / off" | Strength by the blow's weight. The same blows land with it on and off |
| **5. The checks see through it** | Our crossing and tremble checks read the body under the wave | none | The checks read as they do with it off |
| **6. The dance** | The dancers ring each other on every hit and block | "Dance" | Waves started in four dances of 70 s; the checks no worse |

Only when you say: the fight of six.

## Risks, and questions for you

Risks: it may read as jelly (the master dial is for this); a swinging hand takes the sword and shield with it; the
drawn body may smooth it out; many hits at once add up (a cap on the total); a wave that waits for the freeze loses
its first, biggest swing.

Questions:
1. Should the wave run during the freeze on a hit? I recommend yes.
2. Should the legs take part? I recommend it fades through the thighs and stops at the shins.
3. Should the attacker's own arm shudder when his blow is stopped? I recommend yes, small.
4. Keep the bend we have, under the wave? I recommend yes: the bend is the push, the wave is the ring after it.
5. How over the top? The first values are about three times a real body. Say so if you want to begin wilder.

## Sources

Opened and read: Schmitt and Günther, "Human leg impact: energy dissipation of wobbling masses", Archive of Applied
Mechanics, 2011 (https://link.springer.com/article/10.1007/s00419-010-0458-z). Pain and Challis, "Soft tissue motion
during impacts", Journal of Applied Biomechanics, 2002 (https://core.ac.uk/download/288387862.pdf). Play, Trama, Millet
and others, "Soft Tissue Vibrations in Running: A Narrative Review", 2022
(https://link.springer.com/article/10.1186/s40798-022-00524-w). Sakurai's Famitsu column on hitstop, fan translation,
Source Gaming, 2015 (https://sourcegaming.info/2015/11/11/thoughts-on-hitstop-sakurais-famitsu-column-vol-490-1/).
Easy Procedural Hit Reactions, Unreal Engine forums
(https://forums.unrealengine.com/t/kettunen-easy-procedural-hit-reactions/2741781). Daniel Holden, "Spring-It-On",
2021 (https://theorangeduck.com/page/spring-roll-call). "Art Babbitt and Successive Breaking of Joints", Animation
Apprentice, 2019 (https://animationapprentice.blogspot.com/2019/04/what-is-successive-breaking-of-joints.html).
Not opened (its numbers are from the search summary): Chernak, DeWall, Lee and Thelen, 2013
(https://pmc.ncbi.nlm.nih.gov/articles/PMC3709597).

## Progress

**21 November, 22:50: steps 1 to 6 built in one go, headless only, not seen in Unity.** His answers (21:33): seen in the
freeze and on every hit and impact; legs as suggested; the attacker's arm yes; keep the bend; "3 times" read as the
first values (about three times a real body), the dial going to 3 for wilder. A reading taken: the wave is put on the
drawn body (the meshes), not the pose, so no physics, hit or check sees it and step 5 holds by construction. Drawn as a
skeleton from the hips out, so the body stays joined (its joints within 3 to 5 cm of the formula). Lab: "The shock
wave" in the duel lab, "Ring him:" six buttons, the eight sliders, three switches. Labs and dance only. Measured: a ring
of the chest reaches the hands at 0.18 s (3.2 cm) and is still at 0.8 s; the sword's point swings 25 cm. One dance: 57
waves, 17 attackers' shudders. The build log's entry of 22:50 has the tables. The build's film: https://claude.ai/artifact/NxqQKD4EN8eZmRxQkkz9pZ
