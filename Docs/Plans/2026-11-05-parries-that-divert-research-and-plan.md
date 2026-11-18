# Parries that divert: research and plan

> Copy of the Claude document <https://claude.ai/artifact/SptPk1KBhpW1PyrQSVyjRZ>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 5, 2026 · @Renier

## What goes wrong

The parry decides whether a blow is stopped, but nothing on screen makes the two weapons meet. So the club passes through the sword, and low blows pass under it.

- **Through the blade.** In the playtest screenshots, the Large's club sweeps straight through the dummy's sword and into its head. The sword is in the right place; the club simply isn't kept on its side of it.
- **Glances go on through.** A blow the parry only half meets is marked "glanced off the sword: on, slowed", and the club carries on at half speed, through the blade, into the body behind. In the sword test, a forehand glanced, then swept through the head. In the club test, a backhand glanced in seconde, then cut through the pelvis and spine.
- **Even a full parry doesn't show contact.** A parried blow stops the attacker's strike and throws its hand back at most 0.18 m. The blades are never held together, and the defender's sword never moves the other weapon.
- **Low blows get under every parry.** The club's low backhand cut through the right shin, then both feet, and later both thighs. The lowest parries keep the hand at hip height with the blade angled forward, so nothing covers below the knee.

## Research: how a parry diverts a blow

A good parry doesn't stop a blow. It meets the blow with the strong of the blade, stays in contact, and pushes it off its line. That is the motion you asked for.

- **Deflect, don't block.** "Voiding, deflecting and flat-of-the-blade parries are always preferable to direct blocking"; "rigidly blocking to 'absorb momentum' is incorrect" ([ARMA, on parrying](https://www.thearma.org/essays/parrying.htm)). A heavy blow taken square on is the worst case, which is why a club should be turned, not stopped.
- **Meet it with motion.** Students are taught to "never just hold their weapon still and ready to receive a blow" but to "deflect or smack an attack (i.e., to displace it)" ([ARMA](https://www.thearma.org/essays/parrying.htm)). In Liechtenauer's sweeping parries the defender's blade travels through an arc, and "the opponent's strike will be knocked away by the motion of the sweep" ([Sword and Pen](https://swordandpen.substack.com/p/liechtenauers-sweeping-parries)).
- **Strong against weak, then press.** "The strong of the blade is usually the best for parrying, as it has the best leverage". After contact the defender stays on the blade: "remain strongly on it and press firmly" (same source). Setting aside, Absetzen, "winds against the incoming cut" ([Wiktenauer](https://wiktenauer.com/wiki/Absetzen)).
- **Low parries point down.** In seconde the blade goes "down and to the outside", in prime "down and to the inside"; in sabre's octave "the blade points straight down" ([Wikipedia, Parry (fencing)](<https://en.wikipedia.org/wiki/Parry_(fencing)>)). That is the hanging, point-down blade you sketched for the club at the legs.
- **How a game keeps blades from passing.** Blade Ballad moves its characters kinematically, gives the blade only a little physics, and, in a bind, "the weapon aligns to the blade it's engaged with and maintains enough pressure to stay in contact". That was its fix for blades tunnelling through each other ([Blade Ballad devlog](https://khiemgluong.itch.io/bladeballad/devlog/946787/binds-and-spears)).

For us this means: when the blades meet, hold the contact point on both blades for a moment. Then let the defender's blade sweep, and carry the attacker's weapon with it. No physics solver is needed: both weapons are already posed exactly, so the contact can be a pose rule.

## How the parry works today

Today's parry is a placement plus a yes/no test. The defender puts its sword on the blow's predicted path, the hit finder checks whether the two blades came within touching distance, and the blow is then stopped or let through. Nothing in between holds the blades together.

| Part | What it does now | Why the club goes through |
| --- | --- | --- |
| Placement (`SwordParry`) | Picks one of 12 parries by where the blow will land, puts the forte (the strong near the hilt) on the blow's path, and holds it there | Fine as a start. The lowest parries keep the hand at 0.95 m and the blade angled forward, so a blow at the shins passes under the point. |
| Meeting test (`StrikeHits`) | Each step, the attacker's blade is checked at up to 10 sample points against the defender's blade as it stands; they meet if they come within 0.035 m plus 0.02 m (at a Medium's size) | A club at 16 m/s moves 0.27 m a step. If it crosses the thin blade between samples, it can be missed outright. |
| A clean parry | Taken within 0.65 of the blade from the hilt and crossing at 40° or more: the strike winds back to the moment of contact, the hand is thrown back up to 0.18 m, then a hit stop of 0.04 to 0.1 s | Contact is shown for one frame, then the weapons part. The defender's sword never moves the other weapon. |
| A glance | Anything else: the blow goes on at half its momentum, and the sword is ignored for the rest of that blow | The club carries on through the sword, into the head or the pelvis behind it. That is the screenshot. |
| The defender's arm | The blow's knock pushes the sword arm back up to 0.12 m | It is pushed, but it doesn't push back. |

## The design: meet, bind, set aside

When the blades meet they stay together, the defender's blade sweeps outward, and the attacker's weapon is carried with it until it is past the body. Both weapons are already posed exactly (each weapon arm is held kinematic in a blow), so all of this is pose rules, not physics. It takes three new pieces:

1. **A meeting that can't be skipped.** The test covers the whole area the attacker's blade swept since the last step, as two triangles, against the defender's blade. A club crossing the sword between two samples is still caught. From contact on, the blow never passes through the sword in any outcome.
2. **The bind: one shared contact point.** At contact, the hit finder records where on each blade they met. Each step after that, the attacker's hand is offset so its met point stays on the defender's blade. The offset works like the arm's aim pull, which already moves the hand of a posed strike. The attacker's clip plays on, slowed (to 0.4, a heavy club 0.6), so its body keeps driving the blow while its weapon is held.
3. **The set-aside: the defender sweeps.** The parry's forte travels outward, away from its body's middle line, in the plane the blow came in. For a blow on its right that is out to the right; for a low blow, down and out. At full skill it sweeps 0.3 m (at a Medium's size) over about 0.18 s, the wrist turning the point outward with it. The contact slides a little toward the hilt, the strong pressing on the weak. The attacker's weapon goes where the contact goes.

&#91;embedded content: a parry's four phases, contact to release\]

The weapons are never apart between contact and release; the sweep is what moves the blow off line.

**Release.** The bind ends when the attacker's met point is carried past the side of the defender's body (half its width plus 0.15 m), or after 0.25 s. The attacker's strike then plays on from where it was set aside, its hand offset easing back over 0.2 s. That blow can no longer land on this defender. A light blade rebounds as now; a heavy club drags on past, off line. A combo's next blow starts from where the weapon ended up.

**Three outcomes**, from today's tests (where on the blade, the crossing angle) and, later, the blow's weight:

| Outcome | When | What is seen |
| --- | --- | --- |
| Set aside | Clean: on the strong, crossing at 40° or more | The full sweep; the blow is carried past the body |
| Slides off | Today's glance: on the weak, or a shallow crossing | A short bind; the attacker's weapon slides along the sword and off its point, then goes on slowed beside the blade, never through it |
| Beaten | Weight plan step 4: a much heavier blow taken square on | The sword is driven back toward the body, still in contact, until the arm gives; then the club slides off its end, slowed |

The impact keeps a short freeze (0.04 s), so the contact reads as a hit. Then the bind runs live.

## Low blows: point-down parries

Blows at the legs get two new parries that hang the blade point-down along the leg, as in your sketch, so a full-skill defender covers everything below the hips.

- **Low seconde (outside).** The hand at the right hip, about 0.9 m up and 0.3 m forward at a Medium's size. The blade hangs nearly straight down with the point a little forward. A 0.9 m sword then reaches to about 0.1 m off the ground and covers the right leg.
- **Low prime (inside).** The hand across in front of the left hip, the blade hanging the same way. It covers the left leg, which is the lead leg in the sword-only guard and the one the club met first in the playtest.
- **When they are taken.** For blows that will land below the hips, a new leg band beside the existing low line. Seconde and octave stay for the hips.
- **The body goes with the arm.** The knees bend a little (the pelvis about 0.08 m lower) and the trunk leans forward a few degrees, so the hand reaches without the arm going straight.
- **The sweep.** The set-aside carries the club out and a little down, away from the legs. A club pushed down hits the ground short of the feet, not the shins.

Today's headless runs aim only at the hips, chest and head, so leg blows were never measured. The plan adds leg-aimed runs, so the expert's leg parries are gated like the rest.

## The steps

Five steps in the lab, each gated headless and then by your playtest. They take over the parry half of the weight plan's step 4; its other steps follow afterwards.

1. **Measure it.** The headless runs count every step where the attacker's weapon passes through the defender's blade, and leg-aimed runs are added (the Large's club and the Medium's sword at the knees and shins).
   - Gate (headless): today's numbers written down: passes through the blade, and leg blows parried, per body and skill.
2. **Never through the blade.** The swept meeting test and the bind's shared contact point. A glance slides along the sword and off its point instead of going through it.
   - Gate (headless): 0 passes through the blade at every skill, against the Medium's sword and the Large's club; combos still land as now on the open dummy.
   - Gate (you): the Large's combo at the sword dummy, looking for the club passing the sword.
3. **The set-aside.** The defender's sweep carries the attacker's weapon past its body, and the attacker plays on from where it was set aside, the combo's next blow included.
   - Gate (headless): a set-aside blow never reaches the defender's body; the attacker's hand never jumps (under 22 m/s, the swing's own speed); the expert's parry rates hold or improve.
   - Gate (you): the sweep reads as one weapon turning the other aside.
4. **Point-down low parries.** Low seconde and low prime, the leg band, the knee bend.
   - Gate (headless): at full skill, every leg blow from the Large's club and the Medium's sword is parried; none cuts a leg.
   - Gate (you): the club at the legs.
5. **Heavy blows beat a square parry.** The beaten outcome, with the weight plan's figures, still never through the blade.
   - Gate (headless): an expert at a slant sets the club aside; a novice taking it square on is beaten.

**Progress (5 November): steps 1 to 4 done, headless gates passed; your playtest next.**

| Measure (18 runs: 3 attackers, 3 skills, aimed at the body and the legs) | Before | Now |
| --- | --- | --- |
| Steps the blade went through the sword unmet | 97 | 4 |
| The Large's club: blows through, all skills | 27 | 0 |
| The expert against the Medium: blows through | 9 of 42 | 4 of 25 |
| The expert against the Small: blows through | 23 of 41 | 3 of 14 |
| The expert against leg-aimed Medium blows: legs cut | 3 | 0 |

What changed on the way:

- **A glance backs out too.** Played on from beside the sword, its swing carried it straight back across the blade. Now it slides past the point, then backs out like a stopped blow; its combo goes on.
- **The defender reads the next blow sooner.** A met blow takes up its combo's next blow at once as it backs out, so the read-ahead counts it as starting at once.
- **The combos are shorter.** A clean parry ends them, as you chose, so there are fewer blows in each run.

Not done yet: the knee bend for the low parries, and the Small's own low blows, which still reach the left thigh when aimed at the legs (15 of 22 at full skill). Step 5, heavy blows beating a square parry, comes with the weight plan.

## Questions

**Renier's answers (5 November).** A clean set-aside ends the attacker's combo. The sweep moves at the speed the attacking blow came in: a 16 m/s slash is turned aside at 16 m/s, so the bind lasts as long as the distance takes at that speed (0.3 m at 16 m/s is about 0.02 s, shown over at least a few frames). Steps 1 to 4 go now, before the rest of the weight plan.

1. **Does a clean set-aside end the attacker's combo?** Recommended: no for now. The next blow comes from where the weapon was set aside, a little later. Breaking the combo, which opens the attacker for a riposte, is a fight rule worth adding once ripostes exist.
2. **How long should the bind show?** Recommended: quick, about 0.18 s for the sweep, so it reads as one motion and a combo keeps its pace. A longer, visible slide (about 0.3 s) reads more clearly but slows the fight.
3. **Order.** Recommended: steps 1 to 4 before the rest of the weight plan, since a parry that holds contact is what the weight will be felt through. Step 5 joins the weight plan's step 4.

## Sources

- [On parrying](https://www.thearma.org/essays/parrying.htm) (ARMA)
- [Liechtenauer's sweeping parries](https://swordandpen.substack.com/p/liechtenauers-sweeping-parries) (Sword and Pen)
- [Absetzen](https://wiktenauer.com/wiki/Absetzen) (Wiktenauer)
- [Parry (fencing)](<https://en.wikipedia.org/wiki/Parry_(fencing)>) (Wikipedia)
- [Binds and spears](https://khiemgluong.itch.io/bladeballad/devlog/946787/binds-and-spears) (Blade Ballad devlog)
