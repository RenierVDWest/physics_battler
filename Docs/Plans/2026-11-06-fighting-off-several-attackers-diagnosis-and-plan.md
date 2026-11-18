# Fighting off several attackers: diagnosis and plan

> Copy of the Claude document <https://claude.ai/artifact/THdxBZEQwpknnfqRM1ZrgC>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 6, 2026 · @Renier

## What you saw

A unit fighting one enemy now holds up well, but a second or third attacker coming from the side or behind gets no response. The unit being attacked keeps facing its first opponent, takes the new blows without turning, and doesn't move to get the newcomers in front of it. The goal: a unit set on by 2 or 3 attackers should look as alert as one fighting a single enemy. It notices them, turns and moves to keep them in view, defends the blow that matters most, and gives ground or breaks away when outnumbered.

This came from your playtest of 6 November, after the mediums started flanking. Flanking works now, so the defender's blindness to it shows.

## Diagnosis

A clip fighter in the fight responds to nothing but the one enemy it chose, and even that one only by standing in its guard. Five gaps, in the order a blow meets them:

1. **It never learns of a blow coming.** In the old fight, each blow started became a threat its target could see and decide to defend (`CheckThreat`, `DecideDefence`). Clip blows never become threats. The lab's defender watches one attacker (`ClipFighter.Watch` takes a single `StrikeHits`), and in the fight no one calls it at all. That was step 6 of the fight plan, not yet built.
2. **It doesn't learn who struck it.** The old path tells a struck unit who hit it (`NoteStruck`), so an unseen attacker becomes known. Clip hits skip that, so an attacker from behind stays unknown even after it lands a blow.
3. **It faces only its own target.** The fight loop turns it toward the enemy it chose (`FacePoint`), and since 6 November it commits to that enemy while it is within 3.2 m. A second attacker doesn't turn its head or its body.
4. **Its shield and parry work only in front.** The shield is held "not behind itself", and the parry meets blades across the front. A blow from the side or behind can only be stopped by turning first.
5. **Its reaction is a front hit, whatever the side.** The reaction clips are the pack's three sword-and-shield impacts. All eight impacts in your packs (sword and shield, and greatsword) throw the head backward or to one side, none forward. So a blow from behind plays a hit from the front, rocking the body back into the attacker. The bend, lean and drive-back already follow the blow's direction; only the clip doesn't.

**Measured headless** (`fight` with the new `FIGHT_LINEUP=gang2` and `gang3`: a medium with sword and shield in the middle, swordsmen coming from in front, the side and behind):

| Fight | Ends after | Blows met by the one struck |
| --- | --- | --- |
| 1 on 2, three fights | 4 to 8 s | 8 from the front landed, 1 parried; 6 from the side landed |
| 1 on 3, three fights | 6 to 7 s | 7 from the front landed; 4 from the side landed |
| 3 v 3, two fights | 16 to 22 s | 26 from the front landed, 5 parried; 20 from the side landed, 1 parried; 3 from behind landed |

So it doesn't defend the front either. The guard is only in the way by chance, about one blow in ten. The surrounded unit dies in 4 to 8 s, from two or three head blows of about 40 of its 100 health. One fix alone won't make it look alert: it has to know of the blows, see the attackers, turn, defend, and move.

## What a good response looks like

A fighter set on by several keeps them in front of it, meets whichever blow lands first, and keeps moving so they can't all reach it at once. Teachers of fighting several opponents agree on the same few things ([Fighting Arts](https://fightingarts.com/surviving-a-multi-opponent-attack-part-4-maneuvering-for-advantage-putting-it-together/)):

- **Keep them in front.** Pivot to face the nearest threat; avoid the middle, where attackers set the timing and the direction.
- **Line them up.** Move so they come one at a time, the nearest between you and the others, used as a barrier.
- **Get to the outside.** Step out past the end of the group rather than backing straight away; each sidestep opens room behind.
- **Don't linger.** Strike or leave quickly; meet the closest first.

Fighting games add two tools for making that readable:

- **Attackers take turns.** A "battle circle": an enemy in reach asks for a slot before striking, two at once in a typical demo, one in more cinematic games. The others strafe and wait, so each blow comes with warning ([Envato Tuts+](https://code.tutsplus.com/battle-circle-ai-let-your-player-feel-like-theyre-fighting-lots-of-enemies--gamedev-13535t)).
- **Being outnumbered fills a comeback.** For Honor's Revenge fills only from blocks and parries against several attackers; when used, it knocks down those who strike ([Gamepressure](https://www.gamepressure.com/forhonor/revenge-mode/z996e5)).

**What the lab already gives us:**

- the shield that meets any blade's real path, by skill;
- the parry, with weight and giving ground;
- the read-ahead of a combo's next blow;
- reactions that bend, lean and drive back along the blow's line;
- stumbles, falls and getting up.

**What's missing** is all on the defender's side of several attackers:

- a blow known from any attacker, not one;
- a choice of which to meet;
- turning to it;
- moving to keep the attackers in front;
- a reaction that reads the side the blow came from.

## The plan

Eight steps, each built headless, then playtested by you before the next. The defence this needs is the fight plan's step 6, so it is folded in here (steps 2 and 3); the fight plan's step 7 (combos and tempo) comes after. Throughout:

- the walk, the reactions, the shield's path-reading and the skill curves stay as they are;
- a 1-on-1 fight must still look as it does now.

1. **Measure.** A headless set of one defender (sword and shield, or sword only) against 1, 2 and 3 swordsmen from in front, beside and behind, at skills 0, 0.5 and 1. For each blow it records:
   - where it came from;
   - whether the defender saw it;
   - whether it was met;
   - how long the defender took to face a new attacker;
   - how spread round it the attackers stood.

   It also records the defender's planted feet and how long it lasts.
   - Gate: the baseline numbers recorded, and the log pictures (now framed close) show the 1-on-3 fights.
2. **Know every blow and every attacker.** Each clip blow becomes a threat on whoever it is aimed at, as the old fight's blows did. The defender keeps them all, each with its read path, time to contact and whether it is seen:
   - seen within its view, about 100 degrees each way;
   - felt when right beside it.

   Struck, it knows who struck it, so an attacker from behind becomes known.
   - Gate: every blow from in front is seen; one from behind only when felt or struck; nothing changes in 1-on-1.
3. **Meet the most urgent blow.** Of the blows it sees and can reach with its shield or sword, the defender meets the one landing first. It switches only for one landing clearly sooner, held steady as the shield's plan is. A second blow coming at once counts as a combo blow: harder, by its skill curve.
   - Gate: in 1-on-1, the shield and parry rates match the lab's (at skill 0.5, about 25% of single blows and 50% of combo blows missed); against 2 or 3, the blows from in front are met at about those rates.
4. **Turn to the threat.** A seen blow from beyond its guard's reach turns it: the chest and head first (a twist over the hips), the feet after, at the walk's own turning speed. Between blows it faces:
   - the nearest attacker;
   - the middle of two, when they stand within 90 degrees of each other.

   Struck from the side by another, it gives up its committed target for the one striking it.
   - Gate: an expert faces a new attacker within 0.6 s of its first blow starting; blows from the side are met; no feet dragged by the turning.
5. **Move to keep them in front.** Against two or more, it steps out to the outside of the group, away from their middle and toward the end of their line. It puts the nearest between itself and the others, and gives ground on a slant, never straight back into the middle. All of this is walked: steps, not slides.
   - Gate: the time it stands with attackers more than 120 degrees apart round it drops by at least half; planted feet within 2 cm.
6. **Reactions by side.** A blow from the side plays a side impact: the pack's two that throw the head sideways, mirrored for the other side. A blow from behind plays no front-hit clip. It gets the bend, lean and drive-back forward, a lurch or stumble forward, and then a turn toward the attacker.
   - Gate: no front clip for a blow from behind; your playtest.
7. **Concentration** (your choice, replacing turn-taking: no limit on attackers). Being outnumbered raises a stat, concentration. It strengthens the fighter's own blocks and parries, and its blows get through its enemies' blocks and parries more easily. It rises with the number of attackers on it and eases off when it is alone again. Its rates go on Combat Feel.
   - Gate: headless, a skill-0.5 defender against 3 meets more blows than without it, and lands more through their guards; your playtest.
8. **Combos across several enemies** (added 6 November). A combo strings its blows across the nearby enemies engaged with the striker, instead of three at one; a link may turn to another enemy.
   - Gate: headless, in 1-on-2 and 1-on-3, combos that strike two enemies; your playtest.

Every new setting goes on Combat Feel with a tooltip. Each step ends with a README entry and a note here.

**Progress, 6 November** (headless; your playtest of steps 2 to 5 together):

| Step | Done | Measured |
| --- | --- | --- |
| 1. Measure | `fight` with `FIGHT_LINEUP=gang2` and `gang3`; each blow logged by side and whether seen; turn times; time surrounded | the baseline in the diagnosis above |
| 2. Know every blow | each clip blow aimed at a unit is a threat it keeps; seen in view or felt; struck, it knows who struck it | nearly every blow from in front is now seen |
| 3. Meet the most urgent | the seen blow landing first, within 100 degrees of its facing, is the one its shield or sword meets; switched only for one 0.12 s sooner | on 2: lasts 36 to 41 s (was 4 to 8), 70 to 90% of blows blocked or parried |
| 4. Turn to the threat | faces the seen blow's attacker; between blows, the middle of two close together, else the nearer; struck by another, fights them | faces a new attacker in a median 0.3 to 0.8 s; on 3, lasts 16 to 32 s (was 7 to 16) |
| 5. Keep them in front | spread more than 90 degrees round it, it steps to the side that brings them in front; more than 150, it breaks out at a run | time surrounded 24% to 11% of the time outnumbered; on 3, lasts 20 to 60 s |
| 6. Reactions by side | your nine hit clips baked (the left ones mirrored to the right); thrown sideways it plays a head, big head or rib hit for that side, thrown forward the hit from behind; from in front, the pack's own as before. The bend stays and is 1.35 times harder off the front. The clips' recovery is hurried so a reaction takes at most 1 s | every side and back blow plays its clip (the hitlook test); the outnumbered unit lasts about as long as before (48, 26, 60, 17 s; was 48, 24, 44, 27) |
| 7. Concentration | two or more on it, it builds (0.3 a second for each attacker past the first) and fades alone (0.15). Full, it adds 0.5 to its own block and parry skill and half as much again to the weight they hold, and takes 0.3 off the skill of whoever it strikes. Four settings on Combat Feel | the outnumbered unit landed 12 of 27 blows (was 8 of 29) and lasted 60, 36, 49, 17 s (was 48, 26, 60, 17) |
| 8. Combos across enemies | after each blow, the next may turn on another enemy on it within 3.2 m and 100 degrees of its facing, one not yet struck first; with two such in reach, a blow sometimes opens a combo whatever its tempo (one in four) | works, but comes up rarely: the other attackers mostly stand 3 to 7 m off while one is in, so 0 to 6 turned blows a fight |

Found on the way: headless, a shield without a model had nothing to block with; it now gets a board its size. Your side clips are long (1.3 to 1.6 s), and in a full one the unit couldn't turn or step to the next attacker; the fight lost seconds to it, so the recovery is now hurried. Still open: planted feet drag up to 22 cm under staggering blows and in the press, as before this work; that needs its own pass. All eight steps are built; next is your playtest of steps 6 to 8.

## Risks and questions

**Risks:**

- **Turning and stepping fight the walk.** Fast turns and side-steps are where planted feet have slid before (up to 43 cm in the press). Steps 4 and 5 are gated on the feet.
- **Two blows at once may not be meetable.** One shield can't be in two places, so against 3 the best outcome is "meets the first, takes the second". Turns (step 7) are what make it fair.
- **A defender this alert may make 2-on-1 hopeless or endless.** Step 7's slot count is the knob, and tuning comes after your playtests.

**Your answers (6 November):**

1. **No limit** on attackers at once. Step 7 is now concentration.
2. **Concentration**, raised by being outnumbered: better blocks and parries, and its blows get through enemy guards more easily. A better system may come later.
3. **Your nine new hit clips** (Big\_Hit\_To\_Head 1 to 3, Head\_Hit, Hit\_To\_Body 1 to 3, Rib\_Hit, Big\_Rib\_Hit) are used for the reactions. The bend you sketched stays: the part struck moves most (100), the next 80, and so on, along the blow. Blows from other enemies are reinforced by the blow's direction, so they read as contact.
4. **Health stays as it is.**

**Questions for you:**

1. **Attackers taking turns (step 7):** 1 at a time (cinematic), 2 (a fair fight), or no limit (a mob, as now)?
2. **A comeback for the outnumbered**, as For Honor's Revenge? Or just a better defence?
3. **Side impacts:** mirror the pack's two that throw the head sideways, or do you have a pack with side and back hits?
4. **Health:** two or three head blows (about 40 of 100) kill a medium now. Keep it, or slow the kill while the defence work is tested?
