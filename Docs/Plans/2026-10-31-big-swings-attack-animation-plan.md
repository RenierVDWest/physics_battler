# Big Swings: Attack Animation Plan

> Copy of the Claude document <https://claude.ai/artifact/LLsWvHkn4AA69bqFVzhEwn>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Oct 31, 2026 · @Renier

## Summary

The attacks are pokes because each one is a short point path that stops at the target's skin. The arm is bent at contact, the body barely turns, and the weapon reaches only 5–11 m/s.

The fix is to stop describing a blow as "a path to a point" and describe it as **a swing**: the whole body rotating a weapon held at full reach, through a big arc, along a swing plane. The swing is aimed so the aim point sits partway along the weapon's travel, not at its end, so it carries on deep into the target's space. Hips lead, then chest, then arm, then wrist, each joint peaking a moment after the one before. The body steps into every blow. A long, tense wind-up and a fast strike carry the weapon at 14–20 m/s through contact, into a follow-through that wraps around the body.

On the hit side, big swings can **cleave**. A body that is light against the swing's momentum is shoved out of the arc and the swing carries on, slowed. A heavy body or a shield stops it.

We go overboard first (arcs of 250° and more, thrusts that drive 60 cm past the aim point) and rein in later. Before Unity, you review each new move as film strips and speed graphs that I render here.

## Why the attacks are pokes now

Every move is built to arrive at the aim point and stop there. Five things in the move data and your 16:47 log make that read as a poke:

1. **The path ends at the skin.** Paths are written in an aim frame whose origin is the body part aimed at. After contact, the cut drifts sideways and comes back toward the attacker. It goes at most 4 cm deeper than the aim point, and the thrust 6 cm. The smash bounces 12 cm back up. Nothing travels through the target's space.
2. **The arm is bent at contact.** `reach` puts the aim 0.95–1.22 m from the chest. A medium unit's arm plus weapon reaches about 1.6 m from the shoulder, so the elbow is still bent when the blow lands. That is the look of a jab, not a swing.
3. **The arc is small and centred on the chest.** The striking part travels about 1.5 m in all. The blade points out from the chest rather than swinging about the shoulder and hips. Wind-ups sit 0.55–0.8 m from the aim and never go behind the body.
4. **The body barely takes part.** Hips and chest turn 35–55° each way, the lean is 10–20°, and the lunge's step is a shuffle, not a step through. No joint leads or lags another, so everything starts and stops together.
5. **It's slow where it should be fast.** Measured speed at contact, by move:

| Move | Speed at contact (median of hits) | Into the target past the aim |
| --- | --- | --- |
| ClubSwing | 9.7 m/s | 0 cm |
| SwordCut | 6.9 m/s | 4 cm |
| ClubSmash | 5.9 m/s | 0 cm (bounces back) |
| SwordBackhand | 5.8 m/s | 4 cm |
| SwordThrust | 3.0 m/s | 6 cm |

A real full cut turns the sword at 1000–2000°/s through impact ([science-and-fiction.org](https://science-and-fiction.org/swordfighting/cuts.html)), which is 15–30 m/s at the striking part. A trained batter's hands reach 1300–1500°/s ([Rockland Peak Performance](https://rocklandpeakperformance.com/3-questions-to-ask-when-analyzing-a-kinematic-sequence)). We are at a third to a half of that.

The rule from the last round, *every body stops the blow*, makes this worse: a short blow that stops on contact never shows its arc at all.

## What makes a blow look devastating

Animators, swordsmanship research and sports biomechanics agree: power reads from a loaded wind-up, a very short fast strike carried *through* the target, and the whole body turning in sequence behind it.

| Principle | What the sources say | What it means for us |
| --- | --- | --- |
| Anticipation | Wind up and "tense the body for a few frames"; too subtle and the attack feels weak ([Wayline](https://www.wayline.io/blog/level-up-indie-game-melee-combat), [Aztez](https://aztez.com/?p=8443)) | A deep loaded pose with the weapon behind the body and a short hold before release |
| A short strike | The swing or thrust itself is about 6 frames ([Aztez](https://aztez.com/?p=8443)), roughly 0.1 s | Wind-up 0.35–0.5 s, strike 0.1–0.15 s through contact, follow-through 0.25–0.35 s |
| Through, not to | A cut that stops just past impact is 10–20% slower; a through cut carries full speed and energy into the target ([science-and-fiction.org](https://science-and-fiction.org/swordfighting/cuts.html)) | Aim at a point inside the target's body, never its surface |
| The shape of a swing | First the body moves while the blade barely does; then 30–40° of acceleration; then a constant-speed arc to impact; then a deceleration arc ([science-and-fiction.org](https://science-and-fiction.org/swordfighting/cuts.html)) | Speed profile per move: slow body start, sharp acceleration, flat-out arc through contact, long braking arc |
| Pivot inside the body | A full cut pivots inside the body and is powered by the hips; a step must turn the hips with it ([science-and-fiction.org](https://science-and-fiction.org/swordfighting/cuts.html)) | Swing about the hips and shoulder, not the chest; footwork turns the hips |
| Kinematic sequence | Pelvis, then torso, then lead arm, then hand, each faster than the last: pelvis about 650°/s, torso about 800°/s, hand 1300–2100°/s ([Rockland Peak Performance](https://rocklandpeakperformance.com/3-questions-to-ask-when-analyzing-a-kinematic-sequence)) | Each joint starts later and peaks later than the one below it; the weapon trails the hand, then whips past it at contact |
| Exaggeration | Exaggerate the key poses; avoid "pillow fighting"; squash the receiver and stretch the attacker at impact ([Wayline](https://www.wayline.io/blog/level-up-indie-game-melee-combat)) | Wind-up and contact poses pushed past real life; arm and body at full stretch at contact |
| Contact | Both attacker and defender freeze a few frames at the hit; reactions are fast and over the top ([Aztez](https://aztez.com/?p=8443)) | Keep the hit stop; scale the recoil with the new impulses |
| Weapons that collide | Physics-driven sword games make weapons collide, rebound and lock rather than pass through ([80.lv on Bloodfall](https://80.lv/articles/this-first-person-sword-fighting-game-s-combat-is-100-physics-based)) | Keep "never through a body", but let a heavy swing shove light bodies out of its way |
| Smear and arc | A large, bright, fast swing trail sells the motion ([Aztez](https://aztez.com/?p=8443)) | A weapon trail on the striking part during the strike, so the arc reads even at a distance |

## The new swing model

A move becomes a handful of swing numbers instead of a hand-drawn point path. The code turns those numbers into the path, the weapon's angle, the body's turn and the footwork, so every move is a real swing by construction.

1. **Swing plane.** Each blow swings in a plane through the aim point, tilted by an angle: 0° is a flat horizontal swing, 45° a diagonal cut down, 90° straight down overhead, and negative angles rise. The plane sits outside the attacker's weapon shoulder, so the arc wraps around the body without passing through it.
2. **Pivot and radius.** The weapon turns about a pivot between the weapon shoulder and the hips. The radius is the arm at full stretch plus the grip-to-striking-part length: about 1.2 m for a medium sword, scaled by size. The blade points out along the radius, so the weapon is always extended, never dragged.
3. **Arc angles.** The swing starts 150–220° back (behind the shoulder or head) and finishes 100–160° past contact. Total arc: 250–380° of travel for the big swings.
4. **Through depth.** Contact is placed 30–45° before the arc's deepest point, so after hitting the aim point the striking part still drives 0.4–0.6 m (medium size) into the target's space. Thrusts aim 0.6 m past the aim point.
5. **Kinematic chain.** One swing angle drives four joints with offsets. Hips lead, the chest follows about 40 ms later, the shoulder about 80 ms later, and the wrist snaps last, at contact. The hips turn 70–110° away in the wind-up and come through 40–70° past square. The weapon trails the hand by up to 35° while accelerating, lines up at contact and leads in the follow-through.
6. **Footwork.** Every blow steps.
   - *Step-in*: the lead foot steps toward the target for cuts.
   - *Passing step*: the rear foot comes through and becomes the lead, for the biggest swings and the thrust.
   - *Pivot*: both feet turn with the hips.

   The step lands just before contact, so the weight arrives with the blow.
7. **Body shape.** In the wind-up the unit crouches, turns away and loads onto the back foot. At contact it rises onto the front foot and leans 20–35° into the blow, at full stretch. The off arm counter-swings (the shield arm pulls in and back) for balance.
8. **Timing in phases.**
   - Wind-up: 0.3–0.45 s, easing in.
   - Hold: 0.05–0.12 s, the tense moment.
   - Strike: 0.1–0.15 s from release to contact.
   - Follow-through: 0.2–0.3 s, braking.
   - Recovery: 0.3–0.4 s back to guard.

   Bigger units are slower, as now.

&#91;embedded content: planned swing timing · phases from this plan, peak speeds from Rockland Peak Performance\]

The weapon is slowest while the body loads and fastest at contact, because each joint hands its speed on to the next.

What stays: the aim point is still locked at the start and the blow still has to pass within 3 cm of it. The planner still chooses the lunge and refuses blows that pass through the attacker's own body. The weapon arm is still posed and kinematic in the blow. The Move Preview window still works, now showing the arc and the swing plane.

## The new move set

There are seven moves, up from five: the five existing ones are rebuilt as big swings, and two new ones are added (a flat sword cleave and a rising club blow). These are the overboard starting numbers, at medium size.

| Move | Swing plane | Arc: wind-up / past contact | Into the target | Footwork | Speed at contact | Length |
| --- | --- | --- | --- | --- | --- | --- |
| Sword forehand cut | 40° down, from high right | 190° / 130° | 0.45 m | Step-in | 16–20 m/s | 1.0 s |
| Sword backhand cut | 25° down, from high left across the body | 160° / 120° | 0.4 m | Pivot | 14–18 m/s | 0.95 s |
| Sword cleave (new) | Flat, at waist to chest height | 210° / 150° | 0.5 m | Passing step | 16–20 m/s | 1.1 s |
| Sword run-through thrust | Straight line, point first | Drawn back 0.5 m past the hip / driven 0.6 m past the aim | 0.6 m | Passing step, full lunge up to 1.4 m | 8–10 m/s at the point | 0.9 s |
| Club haymaker | 10° down, from far back right | 220° / 160° | 0.5 m | Passing step | 14–18 m/s | 1.15 s |
| Club overhead smash | 85°, from behind the back, over the head | 210° / 110° (down toward the ground past the target) | 0.45 m | Step-in, deep crouch | 14–17 m/s | 1.2 s |
| Club rising blow (new) | 50° up, from low behind | 160° / 140° | 0.4 m | Step-in | 12–15 m/s | 1.05 s |

The thrust is the one straight-line move. It draws the sword back past the hip with the body coiled, then drives the point and the whole body forward together, arm locked at full extension. Its force comes from the body's momentum behind the point as much as from the arm.

Targets: cuts and swings can be aimed at any part. The smash takes head, neck, chest and elbows. The rising blow takes knees, belly and chest. The thrust takes the body.

## Hits with big swings

A big arc meets more bodies and carries 2–3 times the speed, so the hit rules change in four places.

1. **Cleaving.** A swing keeps a share of its speed after each body it meets: 1 − struck mass ÷ (struck mass + attacker mass).
   - If that share is above 0.55, the swing carries on, slowed by that share.
   - Below 0.55 it stops and bounces back, as it does now. A shield always stops it.
   - With our masses, a large unit cleaves through small (keeps 88%) and medium units (78%) but stops on another large one. A medium cleaves through a small unit (68%) but stops on a medium. A small unit stops on everything.
2. **Never through a body.** A cleaved body is shoved ahead of the weapon, not passed through. It gets a knock-back of about 60% of the weapon's speed along the swing for a moment, plus the full stagger recoil, so it is thrown out of the arc as the weapon goes by. This is what knocks people over. A true knockdown (falling, lying, getting up) is milestone 4; until then a cleaved unit stumbles hard and keeps its feet.
3. **Damage and force retuned to the new speeds.** Weapon damage is now set at 15 m/s instead of 6, so health totals keep their meaning. Impulses stay physical: a club haymaker at 16 m/s lands around 120–200 N·s. The knock-back and recoil caps go up so a large unit's swing can throw a small one 1–2 m.
4. **Accurate sweeps at speed.** At 20 m/s the weapon moves 33 cm per physics step along a curve. A straight sweep between steps would cut the corner and could miss an arm. The sweep samples the posed arc 4–8 times per step instead.

Also: a weapon trail on the striking part during the strike. Optionally, a small camera shake on hits above about 100 N·s. Friendly fire stays on: a wide arc that clips an ally is part of what you want to test.

## How we'll know it worked

Every move is measured on every size pairing, in the tests and in playtest logs, against these targets:

| Measure | Target (medium size) | Where it's checked |
| --- | --- | --- |
| Arc travelled by the weapon | at least 250° for swings and cuts | Tests, film strips |
| Speed of the striking part at contact | Cuts 14–20 m/s, club 12–18 m/s, thrust 8–10 m/s | Tests, playtest log's blow table |
| Depth carried past the aim point | at least 0.4 m (thrust 0.6 m) | Tests |
| Body turn, hips through the blow | at least 110° | Tests, film strips |
| Feet | every blow steps; the step lands before contact | Tests, film strips |
| Still lands | within 3 cm of the aim point, as now | Tests (the milestone 2 gate stays) |
| Clear of itself | weapon and arm never inside the wielder's head or torso | Tests (as now) |
| Hips | no drop over 12% of the leg | Tests (as now) |

The review comes in two rounds before you spend time in Unity:

1. **Film strips here.** For each move I render side, top and three-quarter stick-figure strips against each target size, with the weapon's arc traced and a speed-over-time graph. I send them to you first, so you can judge the shapes and say "bigger" or "less" before anything is tuned in Unity.
2. **In Unity.** A new key, **G**, plays every move in turn against a dummy, slowly and then at full speed. The Move Preview window shows the swing plane and arc. Then a normal brawl, with the log's blow table reporting arc, speed and depth for every blow.

## Build order, risks and open questions

Six steps, with your review after step 3 and again after step 6:

1. **Swing generator.** Code that turns a move's swing numbers into its path, weapon angle, joint timing and steps, with tests for every target in the table above.
2. **The seven moves.** Rebuild the five and add the cleave and the rising blow. Setup 14 refreshes the move assets you haven't changed and adds the new moves to the weapons.
3. **Film strips to you.** Every move against every size, with arcs and speed graphs. You say bigger, smaller, slower or faster.
4. **Hit changes.** Cleaving, the shove out of the arc, damage set at 15 m/s, swept arcs.
5. **Presentation and tools.** Weapon trails, the G gallery key, the arc in Move Preview, and arc, speed and depth in the playtest log.
6. **Playtest in Unity.**

**Risks**

- *Hitting itself.* Wind-ups behind the head and back can pass through the wielder. The swing plane sits outside the shoulder and the clearance check stays, but some moves may be refused at some size pairings (a large overhead on a small target, say). The planner then picks another move.
- *Fights get wider.* Contact at full stretch means units strike from further away, so the engage distance grows by about 0.3–0.5 m and crowds spread out.
- *The body may lag.* Big fast twists may outrun the pinned torso in Unity. If the film strips look right but Unity lags, the torso pin goes up during blows.
- *Lead foot swaps.* Passing steps swap the lead foot. The stance has to accept the new lead instead of stepping back.
- *Lots of friendly fire.* Wide arcs will clip allies often. That is wanted for this test; making units avoid it comes later.

**Open questions for you**

- [ ] Should the large unit swing its club two-handed? It would look heavier but needs a two-handed grip pose.
- [ ] Should a simple knockdown come forward from milestone 4 now, so a cleaved light unit actually falls instead of stumbling?
- [ ] Camera shake on big hits: yes or no?

## Sources

- [Aztez: Anatomy of a successful attack](https://aztez.com/?p=8443)
- [science-and-fiction.org: Cutting mechanics](https://science-and-fiction.org/swordfighting/cuts.html)
- [Rockland Peak Performance: Analyzing a kinematic sequence](https://rocklandpeakperformance.com/3-questions-to-ask-when-analyzing-a-kinematic-sequence)
- [Wayline: Level up indie game melee combat](https://www.wayline.io/blog/level-up-indie-game-melee-combat)
- [80.lv: Bloodfall's physics-based sword combat](https://80.lv/articles/this-first-person-sword-fighting-game-s-combat-is-100-physics-based)
