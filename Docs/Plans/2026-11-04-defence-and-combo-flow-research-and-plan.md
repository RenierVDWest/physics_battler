# Defence and combo flow: research and plan

> Copy of the Claude document <https://claude.ai/artifact/DxsdmX2iVBMGQQWAdJSwJu>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 4, 2026 · @Renier

## What the playtest logs show

The combos lose their rhythm for two reasons: a Medium's blow is stopped by a Medium's body and backs out to the guard, and a Large's blow throws the dummy out of reach.

- **The Large's second strike misses.** In all three of its combos in the 09:24 session, the forehand cut through the chest and drove the dummy back at 2.2 m/s, about a metre. The backhand lunge was then chosen because it would land, and it missed by 30–34 cm each time. The plan leads a moving target only a little. The dummy also starts walking back to its spot after a second. The lunge was at its limit (1.4×) and the arm's pull made up only 0.2–0.26 m.
- **Combos are slow.** Every Medium blow on a Medium is stopped by the body ("stopped in the thigh"). The stopped strike backs out into the guard. The next one waits for the feet to settle, sometimes turns on the spot, then plays its whole wind-up. Headless, the blows land 0.9–2.7 s apart.
- **The block hold looks odd.** Blocking is a separate stance. It holds the pack's block-idle pose with the shield high by the face, then turns the shield toward the striker. The guard itself only half-meets blows.
- **Leg hits.** The backhand lunge aimed at the chest struck the right thigh, because the blade passes the thigh first. A leg hit now does what any body hit does: a reaction clip and a push back.

| Time between blows | Seconds |
| --- | --- |
| Our combos now, Medium (headless) | 0.9–2.7, most 1.5–2 |
| Our combos now, Large | 1.7–5.2 |
| The 4-cut sword and shield combo (mocap) | 0.57, then 1.2 (a leap) |
| Standing melee combos 2 and 3 (mocap) | 0.7–0.9 |
| Dual weapon combo (mocap) | 0.43–0.63 |
| Large one-handed club combo (mocap) | 0.7–0.86 |

## Combo flow: chaining strikes Bam! Bam! Bam!

The target is a Medium's blows landing 0.5–0.7 s apart (a Large's 0.7–0.9 s), as the mocap combos do. Each strike's follow-through becomes the next strike's wind-up, and a blow that is stopped never goes back to the guard mid-combo.

Real combinations work this way. A cut's end is the next cut's start (the figure-eight of a forehand into a backhand). A fighter shortens the chamber of the next blow and steps in with it. The plan:

1. **Time is part of a link's cost.** A link is scored on the time from the first blow's contact to the next blow's contact, beside how well the poses meet. Time beyond 0.55 s costs heavily. Links may leave the first strike from 0.1 s after its contact, once the blade is through, not a third of the way into its recovery. They may enter the next strike as late as 0.25 s before its contact. Most of the next wind-up is then skipped.
2. **A wider bridge between poses.** Links that meet less well (cost up to about 2, against 1.2 now) are bridged by a slightly longer fade (0.15–0.25 s). It moves the hand along the way both clips are moving it, at most 12 m/s. The arm-whip guards built for steps 3–5 stay on (the faded reach, the steady elbow).
3. **A quicker chamber.** A follow-on's wind-up plays up to 1.3× faster, back to normal speed by its contact. The blow itself is not sped up.
4. **A stopped blow rebounds into the next.** A blade stopped by a body or a shield backs out a little, as now. The next strike is then taken up from that pose, through the bridge above, not from the guard. The feet are not waited on. The locks step them during the bridge.
5. **The combo follows its target.** Each follow-on is planned against where the target will be at its contact. The knock a blow gives is known when it lands, and so is how the walk slows it. A follow-on may lunge up to 1.7× (1.4 now). During a follow-through, the root drifts up to 0.3 m toward the target, the attacker pressing in. Stepping in between strikes (pressing) is kept only for a target more than about a metre beyond reach.
6. **The authored combos lead.** Your packs and uploads hold more combos than we use: the standing melee combos 1–3, the dual weapon combo, the Pro pack's slash 5 and attack 2, and the great sword combos for the Large. Cut into strikes, they link to each other at almost no cost with their own timing. The chooser takes the authored next strike when it lands. I check each by eye first (one-handed, sword hand) before baking it.
7. **Measured headless.** The combos runner reports the time between blows, the link costs, the hand and foot speeds about each switch, and the planted feet. Gates: Medium median 0.7 s or less (Large 0.85 s); every strike lands on the standing dummy and 9 in 10 on the driven one; hands at most 12 m/s about a switch; planted feet under 1 cm. The lab panel shows each combo's pace.

## Shield blocking as a skill

The guard becomes the block. The dummy stands in its normal guard and moves its shield only when a blow comes. How well it does so is set by one stat, shield skill, from 0 to 1. At 1 it meets every blow it can reach in time.

The separate blocking stance and its block-idle pose go. The pose holds the shield high by the face and looks wrong beside the guard. The panel picks the dummy's setup: open (it doesn't defend, so every blow lands), shield (sword and shield, blocking with its shield skill), or sword (sword only, parrying with its parry skill; see below). Each skill has its own control. The two stay separate stats: a unit whose shield breaks or is lost must parry with its sword, and then its parry skill is what counts.

The arm that moves the shield is solved by the fight's arm rig (`ArmRig.Solve`) instead of the plain two-bone solver used now. The rig keeps the elbow on a natural null, the wrist within its limits and the arm clear of the torso.

| Lever | Skill 0 | Skill 1 |
| --- | --- | --- |
| Starts moving after the blow starts | 0.35 s | 0.12 s, reading the wind-up |
| Error in where it reads the blow will land | up to 25 cm | none |
| Error in the line it reads the blow coming on | up to 30° | none |
| Shield speed (accelerating, not jumping) | 2 m/s | 6 m/s |
| Corrects its read as the blade comes | no: commits to the first read | every step |
| Covers low cuts (legs) | rarely: holds the shield at the chest | yes, and steps back from a low cut it can't reach |
| Angle on the blow | square on, takes all of it | slanted so a cut glances off |
| Back in the guard after a block | 0.6 s | 0.25 s |

Skill shows in failures you can watch, not dice rolls. A low-skill dummy is late, guesses the wrong side, chases the blade or leaves its legs open. Between 0 and 1 each lever is blended. A small random spread around the read error keeps blocks from looking scripted.

Gate (headless, the dummyhits runner across every strike and height): skill 1 blocks every strike whose blow takes 0.3 s or more from its start to contact; skill 0.5 blocks 50–70%; skill 0 blocks 15–30%. The shield arm stays within the rig's limits throughout.

**Built 4 Nov.** Shield skill is set in the Defender section, in quarters. Besides the table's levers, two turned out to decide most blocks. The first is how soon before contact it moves: a block is won or lost between 0.05 and 0.2 s. The second is whether it closes the line or chases the blade. The shield's turn is speed-limited too: a novice's shield whipping round at the last moment caught almost everything. Of a Medium's 27 strikes it blocks 23 at skill 1, 16 at 0.5 (59%, in the gate), and 9 at 0 (33%). The guard alone stops 8, so the novice sits just above the gate's 30%. The expert misses only on geometry: an overhead chop aimed at the hips passes the head above the shield, and a thrust takes the shield arm. Combos are blocked more than single strikes (25, 26 and 16 of 27), because a shield still up from the last blow meets the next. Not yet: stepping back from a low cut it can't reach.

## Sword parries: what the research says

A one-handed sword parries in a small set of fixed positions, each closing one line of attack. The defender takes the attacker's blade on the strong third of its own (the forte, nearest the hilt), crossing it at close to a right angle. The 19th-century sabre and broadsword manuals (Hutton, Burton, Castle, Angelo, Radaelli) agree on these. The older treatises (Silver, Meyer, Ringeck, Marozzo) add the hanging parry and the parry that is itself a cut.

**The positions** (right-handed defender; inside is its left, the chest side):

| Parry | Closes | Hand | Blade | Wrist |
| --- | --- | --- | --- | --- |
| Quinte / St George | the head | above and right of the head, upper arm out at shoulder height, elbow under the hilt | across above the head, point to the inside front | knuckles up |
| Hanging (high prime) | the head and the inside | above the head, a little forward | slopes down across the body, point toward its own left knee | knuckles up |
| High tierce | the head, the right cheek | head height, right | 45° up, point to the left over the head | knuckles up |
| Tierce | the right shoulder, arm, flank | waist to lower chest, just outside the right flank | point up and forward | knuckles up and out |
| Quarte | the chest, left side | across the body at waist to lower chest, elbow nearly at the belt | point up and forward | nails up |
| High quarte | the left cheek | left of the left eye | across the face at 45°, point to the right | nails up |
| Prime | the left shoulder | across the body at shoulder height | near upright, point down | knuckles up |
| Seconde | the right hip and leg | outside, waist height | point down to the right front | knuckles up |
| Low quarte | the belly | at the right hip, about 20 cm out | point toward the left shoulder | nails up |
| Septime / octave | the left / right thigh and knee | low and forward | point down toward the ankle / below the knee | nails up |

**How the blades meet.**

- Forte against foible: the attacker's weak outer third on the defender's strong inner third. Taken near the hilt, a blow can't force the guard in. Taken out on the defender's own foible, it does: a classic failure.
- About 90°: a cross near square stops the blow fully. A slanted cross lets it slide off and frees the defender's point to answer.
- A hard cut is met with the edge. Meeting it with the flat, square on, is the case where the defending blade is cut through or knocked aside. The flat is used when hanging or deflecting.

**Kinds of parry.**

- Stop (opposition): meet and hold, the arm drawn in and the grip firmed at impact. "No strength is necessary" (Burton). The default against a committed blow.
- Beat: strike the incoming blade aside, against a blow still developing.
- Hanging (yielding): catch the blow on a sloped blade and let it slide off, then answer to the opening.
- Counter-cut: the parry is itself a cut that takes the line, as with Meyer's and Liechtenauer's Versetzen. This comes later, when the dummy fights back.

**Timing.** A fast cut takes 0.25–0.35 s from first motion; an ordinary one about 0.6 s. A fencer's simple reaction time is about 0.26 s at every skill level. Elite fencers take about 0.35 s for a whole defensive action. Experts win by reading the wind-up and the upper body before the blade moves: they pick where an attack will land better than chance even 0.12 s before impact. So a parry against a fast blow only works if it starts on the wind-up. Skill shows in anticipation and in how cleanly the action is made, not in raw reaction.

**How novices fail**, the levers for a parry skill: late (reacting to the blade, not the wind-up), early (showing the parry so it is deceived), too wide, chasing the blade instead of closing the line, taking the blow on its own foible, flat square to a hard cut, too shallow a cross so the blow glances into the hand, the arm stretched out in a stop parry, the blade not closing the line (a hanging guard that leaves the flank), freezing in the parry, moving the feet before the hand, and no second parry after a feint.

**Footwork.** The defender retires a small step (back foot first) when the parry would be late, or when the attacker's step would bring the blow inside its forte. Against a cut at the leg, it slips the leg back and parries high, as the broadsword manuals pair them. Meyer steps away from the cut, offline, and a one-handed fighter does better going to the side than straight back.

**The arm.** The shoulder raises to 160–180°, the elbow bends 0–140°, the forearm turns 80° each way, and the wrist bends about 60° up or down but only 20° toward the thumb. The wrist moves most freely on a slant, from up-and-thumbward to down-and-pinkyward: the cutting plane. The elbow stays down by the flank in quarte and tierce, rises out to the right in St George, and lifts up and forward in prime and hanging. Only prime and the hanging parries carry the forearm across the body; the elbow rises to clear the chest there.

Sources are listed at the end.

## Sword parries: the design

The sword dummy reads each blow from the attacker's wind-up, as the shield dummy does. It picks the manual's parry for where the blow will land and moves its sword there with the fight's arm rig, the elbow where the manual puts it. Its forte crosses the incoming blade near square at the point the blade will pass. At skill 1 it meets every blow; lower skills fail in the ways novices do.

**A reach test, done today.** A new headless runner (`parryreach`) stands a Medium in its guard and asks the arm rig to hold each manual position, with whichever edge it takes better.

| Parry | Wrist short of the turn | Where the elbow went | Hand travel from today's guard |
| --- | --- | --- | --- |
| Quinte / St George | 0° | out right and up, as Castle has it | 0.85 m |
| Hanging | 0° | up and forward | 0.89 m |
| High tierce | 0° | out right, under the hilt | 0.67 m |
| Quarte | 2° | down by the flank, as Burton has it | 0.69 m |
| Prime | 5° | up, across the chest | 0.88 m |
| Low quarte | 0° | down | 0.52 m |
| Octave | 2° | down and forward | 0.58 m |
| High quarte | 18° | forward | 0.91 m |
| Seconde | 19° | down and forward | 0.46 m |
| Tierce | 31° | down | 0.47 m |
| Septime | 46° | down and forward | 0.65 m |
| Low tierce | 48° | down | 0.46 m |

Three things follow from it:

- **The arm rig can do this.** Eight of the twelve sit within its limits, and the elbows fall where the manuals put them. No position needed the shoulder past its range, and none put the arm into the torso.
- **The trunk takes part in every parry.** A parry is made with the shoulder and trunk as well as the arm. The chest turns toward the side the sword goes and leans with high and low parries: a few degrees in most, up to 25° of turn and 10° of lean in tierce, low tierce and septime, which the arm can't hold alone. Sharing the work also takes the wrist off its full twist and bend, where several poses now sit and look strained. Each pose stays inside 80% of the wrist's range.
- **It needs its own guard.** The shield guard carries the sword low and back by the right hip, so each parry is a 0.5–0.9 m reach. The manuals parry 15–45 cm from a medium guard. The sword dummy gets that guard: hand in front of the belly, point at the opponent's face, edge down. It is the walk's guard clip with the sword arm set there by the rig, the free left hand at the chest.

**How a parry is made.**

1. **Read** the blow: where it will land and the line it comes in on, as the shield dummy reads it, plus the incoming blade's path from the attacker's clip.
2. **Choose** the parry whose line holds the landing point, preferring the one nearest the current pose. Head cuts go to St George or hanging; cuts to the inside to quarte, high quarte or prime; to the outside to tierce or high tierce; low to seconde, septime or octave; thrusts to quarte or tierce by their side.
3. **Place** the crossing: the parry's pose is the start, then the grip is solved so its forte lies on the incoming blade's path, a forearm's length or more in front of the body, crossing it at 70–90°. The parry's elbow null and edge are kept.
4. **Move** there naturally:
   - The hand follows a smooth eased path (0.1–0.2 s), never a straight blend of joint turns. The blade turns the short way round. The elbow swivels smoothly round its circle, never flipping.
   - Limits: the hand at most 6 m/s, the blade turning at most 1,200°/s (a cut's own peak is 1,000–2,000°/s), the elbow swivel at most 540°/s.
   - Between parries the arm takes the manuals' way: quarte to tierce passes in front of the chest, not over the head.
   - The trunk turns and leans with every parry, most with those the arm can't hold alone. The torso clearance keeps the arm and blade off the body and head.
5. **Hold or answer.** A stop parry draws the arm in a little and firms at the contact. A blow still developing may be beaten aside. A cut from above to the head or inside may be hung, so it slides off the sloped blade.

**What contact does.** A new weapon-on-weapon sweep tests the attacker's blade against the dummy's blade capsule, beside the body and shield sweep. The outcome depends on the cross:

- 45° or more, on the forte: stopped. The hit stop, the blade rebounds (as on a shield), and a block's share of poise comes through.
- 20–45°: deflected. The blow slides off along the defender's blade, weakened, and lands only if the slide carries it into the body.
- Shallower, or on the defender's foible: forced through. The defending blade is driven in and the blow lands weakened.

The blow's impulse knocks the parrying arm, as the shield arm is knocked now.

**Footwork.** The dummy retires a small step, back foot first, when its parry would be late: the time to contact is less than its reaction plus the hand's travel. It also retires when the attacker's step would bring the blow inside its forte, and after a deceived first parry. Against a cut at a leg it slips that leg back and parries high, as the manuals pair them. Stepping offline comes later.

**Parry skill**, 0 to 1:

| Lever | Skill 0 | Skill 1 |
| --- | --- | --- |
| Starts after the blow starts | 0.4 s, on the blade | on the wind-up, about 0.1 s |
| Error in where it reads the blow will land | up to 25 cm | none |
| Picks the wrong parry | 30% of the time | never |
| Hand speed | 2.5 m/s | 6 m/s |
| Corrects its read as the blade comes | no | every step |
| Cross | shallow, often on its own foible | 80–90° on its forte |
| Arm in a stop parry | stretched out | drawn in |
| Gives ground when it would be late | no | yes |
| Second parry after a deceived first | no | yes |

**Checked headless.** A `dummyparries` runner plays every strike at the sword dummy, at three heights and skills 0, 0.5 and 1. It reports the share parried, the crossing angle and where on each blade they met. It also reports the arm's margins to its limits, the fastest hand, the fastest elbow swivel and the torso clearance, and writes films of both bodies. Gates:

- At skill 1 it parries every strike with 0.3 s or more from start to contact, and 95% overall.
- The arm never passes its limits, the elbow never flips (no swivel jump over 60° in a step), and the blade never passes through the body.
- You judge the films and the lab before it moves on.

**The Large's great sword** parries later and differently: both hands stay on the grip, so the turn comes from the trunk and the hilt (Ochs, Hengen, the crown), with more stops and binds than beats. The great sword pack's three blocking clips are its references.

## Leg hits that take the legs out

A hard blow to a leg can take that leg out from under the body and drop it. How easily depends on how much weight the leg carries. The fall and the getting up reuse the fight's own knock-down and get-up (`UnitBalance`, `GetUp`), which already work for every body.

1. **The struck leg is swept.** A hit on a thigh, shin or foot pushes that leg along the blow. A foot in the air, or one about to lift, is knocked away by almost any real blow. A planted foot carrying the weight needs a blow strong for the body's mass. Its lock lets go and the foot is thrown along the cut (sideways for a horizontal cut, back for a rising one).
2. **Stumble, or fall.** With the other foot still planted under the body, it stumbles: a catch step with the swept leg and the body-hit reaction. With the weight left over nothing, or a heavy blow (a Large's), it falls. The upper body is thrown the way the leg was swept, the body goes to ragdoll, and the fight's get-up stands it after 1–2 s.
3. **The thresholds start from the fight's.** The fight stumbles a unit from a 0.8 m/s knock and knocks it down from 2.2 m/s. A leg sweep counts the blow on the leg against how much weight that leg carries, so a sweep drops a body at a lower blow than a push does. These are tuned in the lab.
4. **References.** The packs have death falls (sword and shield deaths 1–2, two-handed deaths 1–2). They show how the knees go in the first moments of a fall, before the ragdoll takes over. They have no get-up or trip clips, so the get-up stays the fight's own key poses.

Gate: headless, the Large's low cuts drop a Medium from a planted stance, and a Medium's low cut on a stepping Medium drops it. A cut on a Medium's planted leg only makes it stumble. In the lab, you judge whether falls read as the legs going out.

**Result (built; first playtest fixed).** The first playtest found three problems. Every Medium thigh cut dropped the dummy, because a foot was read as up and the leg was swept at 0.3 m/s. Hits on the fallen dummy played standing reactions that pinned the ragdoll up into the air, then dropped it. And a Large's combo magnet snapped it 1.5 m in one frame, knocking it down and sending it sliding across the board. Now: on both feet the dummy stumbles from 0.3 m/s on the struck leg and falls from 2. With its weight all on the struck leg it stumbles from 0.25 and falls from 1.2. With the struck foot in the air it stumbles from 0.25 and falls from 1.8. A Medium's sword makes it stumble; only a Large's solid cut (2.5–3.2 m/s in the lab) drops it. Down or getting up, a blow only knocks the part it struck. The magnet carries the body at most 0.6 m per blow and 0.12 m per step, and a knocked-down body keeps no more than the knockdown speed. Combos still land (Medium 42/42, Large 9/9). In the lab, U's fourth aim (legs) sends every blow at the knees. The fall and the get-up can only be judged in Unity.

## Lab panel: bigger and organised

The panel scales up 1.5×, with a size control (− / +) kept between runs. Its 44 controls are grouped under five headed, collapsible sections, each a line of buttons and then its readouts:

1. **Body**: size (Small, Medium, Large), physical or kinematic, carrying, restart, pause, watch the fight.
2. **Walk**: drive or course, pace, guard up, shove, hard shove, ball, and the walk's measures.
3. **Attacker**: the picked strike (← J →), what it is and where it reaches, combo (I), sparring (X), aim height (U), in reach (Y), and the last strike's result in colour (green landed, blue blocked, red missed).
4. **Defender**: open, shield or sword (O), the defence skill (with presets 0, 0.5 and 1), anchored or not, and how it took the last blow.
5. **Log**: the last ten events, newest first. The full log stays in session.md.

The key for each control stays in its label. A short line under each heading says what the section's numbers mean.

## Order of work, and what was decided

The sword parry goes first: it is the hardest part and the most important now. Each step is playtested by you before the next begins.

1. **The sword dummy, stage 1**: its medium guard, and the parries held at full skill against single strikes, the trunk in each. With it, the lab panel, since the dummy's setup (open, shield, sword) and the two skill controls live there. You judge the arm.
2. **Stage 2**: what contact does (stop, deflect, forced through), the knock on the arm, and the footwork.
3. **Stage 3**: the parry skill levels, and combos against it.
4. **Combo flow**, points 1–7 above, aiming at 0.5 s between a Medium's blows and 0.7 s for a Large's (the short end), tuned once you've seen it.
5. **Shield blocking as a skill.**
6. **Leg hits and falls.**

**Decided 4 Nov:**

- The dummy's setups are open (every blow lands), shield (sword and shield) and sword (sword only).
- Shield skill and parry skill are two stats.
- Combo rhythm: start short (0.5 s and 0.7 s) and tune after you see it.
- The extra combos get baked: turned from mocap samples into the game's clips, as the current strikes were. Each strike is cut out, its contact moment marked, and it is fitted to our bodies, after a check by eye.
- The sword parry comes first.

**Stage 1 built (4 Nov), for your playtest.** Pick "Sword only" in the panel's Defender section.

- Headless, against a Medium striking at the hips, chest and head, the sword dummy parries 21 of 27 blows. The blades cross at 42–90°, the hand moves at most 11 m/s, and the elbow turns at most 9° a step.
- Not parried yet:
  - The combo's quick backhand: 0.18 s from its start to contact.
  - A thrust at the face.
  - The Small's low blows: only 12 of 27 parried. They reach the dummy's sword hand and forearm.
- The Large: 7 of 9 parried.
- The open dummy takes every blow.
- The shield dummy blocks 20 of 27 from its normal guard. The held block pose blocked a little more at chest height. That comes back in the shield step.
- New since the plan: the dummy works out where the blade will first reach its body (torso, head or either leg), from the paths of the blade's striking point and its tip. It moves to parry only about 0.3 s before contact. Up earlier, it would show the attacker where to cut.

**Stage 2 built (4 Nov), for your playtest.** Same panel setting, "Sword only".

- **The read was wrong, and that was most of the misses.** The path the dummy parried against was worked out from the clip and pinned to where the strike was aimed. With the lunge cut to keep the distance, the real blade went up to a metre from it. Now the blade is played on from where it is: the clip's arm, carried as the lunge and the aimed turn will carry the body, with the aim's pull and the wrist's steer. It matches the real blade to a few centimetres until the last tenth of a second.
- **Each parry is tried before it is taken.** The dummy plays the blow against its sword, arm and body for each parry, and takes the one that meets the blade first. It keeps the parry it holds while that still works, and never changes in the last 0.15 s.
- **Contact:** a clean stop on the forte knocks the attacker's hand back and the dummy's arm and body take the rest; a glance (foible, or a flat crossing) goes on at half speed past the sword arm. A weapon touching a body outside a blow pushes it, and a struck leg twists.
- **Spacing:** 1 m root to root for two Mediums. The lunge is cut there, pressing stops there, the aim never pulls the hand into the other body, and the dummy's sword is kept out of the attacker's body and shield.
- **The arm:** the upper arm turns at most 900° a second at the shoulder (it was jumping 30–80° in one frame on fast parries). The hand now moves at most about 10 m/s.
- **Headless:** Medium 25 of 27 parried (was 21), Large 9 of 9, Small 14 of 27. Walk unchanged. 25 of 27 combo blows land.
- **Still open:** stepping back from a blow that comes too close (the side chop that meets the upper arm), the Small's quick low blows, and a rare one-frame elbow flip in the folded guard after a Small's blow. The close pair combo backhand into combo chop can't land from 1 m, so after it the combo now takes any strike up from the guard.

**After your 12:23 playtest (4 Nov).**

- They now stand 1.4 m apart between blows, for two Mediums. The attacker strikes from there and only steps in when a strike can't reach. The dummy steps back to that distance after each blow, and no blow brings them closer than 1 m.
- The dummy's sword stays out of the attacker's shield. If the shield swings into the guard's point anyway, the blade is pushed off straight away.
- A blow at the head is met at least 0.3 m in front of the face. A parry only counts as clean if it takes the attacker's blade before that blade reaches the head or the arm, and not right at its hilt.

**Parry parked (4 Nov), to be tied to the parry skill stat in stage 3.** The buzzing guard sword is fixed: the guard draws back smoothly and keeps off the attacker's blade too, and a resting weapon no longer knocks it. A jumping strike stopped in the air now comes down with its own landing.

**Combo flow, step 1 built (4 Nov).** Points 1, 2 and 4 are in, and part of 6:

- **The beat counts in each link.** The time from one contact to the next is part of the cost.
- **Earlier links.** A strike can be left 0.1 s after its contact, and the next one taken up as late as 0.25 s before its own.
- **Quicker chamber.** The next blow's wind-up plays up to 1.2× faster.
- **Following the target.** A follow-on can lunge up to 1.7×, and a strike with no quick follow-on carries on from the guard.

Headless, a Medium's blows now land 0.63 s apart (the median, down from 1.58 s), and 24 of 27 land. Still to do: the Large, which has only three strikes and needs its great sword combos baked (point 7); follow-ons that need a 60° turn onto the target; the stopped blow's rebound (point 5); and the authored combos.

**Stage 3 built, 4 Nov.** Parry skill is a lever from 0 (novice) to 1 (expert), set in the Defender section in quarters. At skill 0 it moves late, misreads where the blow lands by up to 25 cm and rarely reads it again, takes the wrong parry 3 times in 10, goes where it read the blow instead of playing on against the blade, moves its hand at 2.5 m/s, and meets the blade shallow and out on its foible, so blows glance through. Headless, of a Medium's 27 single strikes it parries 24 at skill 1, 17 at 0.5 and 10 at 0. Of 42 combo blows it parries 17 at skill 1, 16 at 0.5 and 9 at 0: combos beat even an expert on the second and third blows. Not yet: giving ground when late, and a second parry after a feint.

## Sources

- [Hutton's sabre parries (Classical Academy)](https://classicalacademy.blogspot.com/2019/02/s7-huttons-sabre-parries.html)
- [Burton, A New System of Sword Exercise for Infantry (1876)](https://gutenberg.org/cache/epub/59336/pg59336-images.html)
- [Castle's sabre text](https://www.props.eric-hart.com/wp-content/uploads/2009/10/cold-steel.pdf)
- [Angelo and Roworth's broadsword guards (swordfight.uk)](https://swordfight.uk/?p=1225)
- [Radaellian sabre parries](https://radaellianscholar.blogspot.com/search/label/parries)
- [Parry (fencing), Wikipedia](<https://en.wikipedia.org/wiki/Parry_(fencing)>)
- [Parries handout (fencer.com)](https://fencer.com/wp-content/uploads/2021/02/Parries.pdf)
- [Should two edges meet? (Academie Duello)](https://academieduello.com/blog/should-two-edges-meet)
- [Edge on edge contact (Hugh Knight)](https://talhoffer.blogspot.com/2009/05/edge-on-edge-contact.html)
- [Marozzo: the edges of the sword](https://marozzo.com/fundamentals-part-1-the-edges-of-the-sword)
- [Ringeck (ARMA)](https://www.thearma.org/Manuals/Ringeck.htm)
- [Hengen (Wiktenauer)](https://wiktenauer.com/wiki/Hengen)
- [Meyer's dussack, Garber translation (Wiktenauer)](https://wiktenauer.com/wiki/Joachim_Meyer/Rebecca_L._R._Garber_Dusack_Ch4_2023)
- [Meyer's dussack](https://science-and-fiction.org/swordfighting/meyer_dusack.html), [cuts and their timing](https://science-and-fiction.org/swordfighting/cuts.html), [one-handed vs long sword](https://science-and-fiction.org/swordfighting/one_vs_long.html) (science-and-fiction.org)
- [Silver, Paradoxes of Defence](https://uow.edu.au/~aden/gsparado.html)
- [Crossing swords (Windsor, on Fiore)](https://www.guywindsor.net/wp-content/uploads/2024/01/Crossing-Swords-article.pdf)
- [Balkó et al. 2016, fencers' reaction times](https://periodicos.ufsc.br/index.php/rbcdh/article/download/1980-0037.2016v18n4p391/32458)
- [Elite fencers' defensive action time](https://lida.sport-iat.de/twm/Record/4041897?lng=en)
- [Hagemann et al. 2010, anticipation in fencing](https://link.springer.com/content/pdf/10.3758%2FBF03196695.pdf)
- [Borysiuk 2024, fencers' perception](https://www.frontiersin.org/journals/sports-and-active-living/articles/10.3389/fspor.2024.1387013/pdf)
- [Joint ranges of motion (WikEM)](<https://wikem.org/wiki/Joint_ROM_(Table)>)
- [Crisco et al. 2011, wrist motion](https://pmc.ncbi.nlm.nih.gov/articles/PMC3016043)
