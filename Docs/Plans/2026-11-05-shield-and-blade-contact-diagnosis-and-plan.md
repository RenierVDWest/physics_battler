# Shield and blade contact: diagnosis and plan

> Copy of the Claude document <https://claude.ai/artifact/S2QGX9iFCKHkookdZewUU2>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 5, 2026 · @Renier

## What you saw

Two separate faults show in the pictures: blades pass through bodies, and the shield guards the wrong place. Your session of 5 November (21:03, the walker with sword and shield against the sword dummy) shows both.

- **Blades through bodies (pictures 1 to 4).** Four of the dummy's blows that landed went on through two to four more parts after the first. At 28.1 s its forehand went through the left upper arm, then the spine, then the pelvis. At 43.8 s its chop went through the head, the chest and the upper arm, then reached the shield on the far side. Pictures 2 and 4 show a blade resting inside a leg outside any blow, while the dummy crouches in its reaction.
- **The shield in the wrong place (pictures 5 and 6).** Against single blows the shield did well: 7 of the dummy's 8 were blocked. The one that wasn't was an overhead chop, which landed in the head at 20.3 s while the shield stayed at chest height. Against combos it failed: 1 of 8 combo blows blocked. Headless, an expert shield on the training post shows the same faults: a chop aimed at the hips lands in the head on its way down, and thrusts at the hips and chest land in the shield arm.

One thing that makes the shield look worse in the lab: the walker's shield skill starts at 0.5. On the skill curve that is three quarters of the way to a novice. It misreads where a blow lands by up to 19 cm and the line it comes on by up to 22°, and it starts to move only about 0.1 s before contact. So part of what you saw is the skill working as designed. The rest is real faults, which an expert has too.

## Why it happens

The code was read for both faults. There are three causes of blades in bodies and four of the shield guarding the wrong place.

**Blades through bodies**

1. **Combo blows sweep on through.** A combo's edge blow is deliberately let through the body it lands on, so the next blow follows on ("Bam and on"). Each part after the first logs a hit of 0 N·s: the blade is drawn through the body with nothing stopping it. Clubs already stop since step 2 of the weight plan; swords don't.
2. **The old fight's cleave.** A blow from a heavier attacker cuts on through a lighter body ("cut through the head"), the Large through a Medium, a Medium through a Small.
3. **Nothing stops a blade outside a blow.** In the wind-up, the recovery, a reaction or the guard, a blade that meets a body or a shield only pushes it. The blade itself goes on into it, as in pictures 2 and 4.

**The shield guards the wrong place**

1. **It guards the aim point, not the blade's path.** The shield goes to a point a forearm's length out from where the blow will land, toward where the blade is now. It never looks at the path the blade will take. A chop aimed at the hips passes the head first, and the shield waits at the hips.
2. **It follows one point, not the whole blade.** Only the striking point is read, not the length of the blade from grip to tip. The blade can pass beside the board with its striking point well covered.
3. **It faces the blade's position, not its motion.** The face turns toward where the blade is, not square against the way it moves. A blow coming down from above meets the board edge-on.
4. **It can't reach high, and moves late in combos.** The arm's reach and elbow keep the board at shoulder height or below, so an overhead chop or a jumping strike goes over it. A combo's next blow comes into view about 0.15 s before contact, the same fault the parry had. The parry was given a read-ahead of the next blow; the shield never got it.

The parry had all four of these faults before "Parries that divert", and the cure was the same: read the blade's real path, meet it where it will be, and test that the meeting will happen.

## The approach

Five rules, each checked headless by a number before you playtest it.

1. **A slash carries through; nothing else goes into a body or a shield.** A sword's slash sweeps on through the body, as you want: the body's reaction shows that it landed. Outside a blow, the weapon rests against what it touches, its hand held off by the depth it would go in. A shield is never passed through.
2. **The shield meets the blade on the blade's real path.** The path from the blow's clip, the one the parry already reads, says where the whole blade (grip to tip) will be at each moment. The shield goes where that path first crosses the reach of the shield arm, in front of what it would hit first.
3. **The face is square to the blade's motion there.** An expert then slants it, so a cut glances off.
4. **A test before contact says whether it will meet.** It checks whether the planned board will lie across the blade's path. Every blow is logged with what was planned and what happened, as the parry's blows are now.
5. **Skill changes only the read, never the geometry.** Skill sets how early it moves, how far off it reads, and how fast the arm is. An expert's shield is always on the path, and a novice's is late or off, but never facing the wrong way.

## The steps

Seven steps, one at a time, each committed with its numbers and handed to you to playtest before the next. The shield comes first, as you chose. Every step's gate also checks that the reactions are unchanged: the bend, the hit stop, the drive-back and the stumble.

1. **Measure it first.** No behaviour changes.
   - New per-blow shield log, in the lab's session log as the parry's is: when it read the blow, where it planned the board, and whether the board lay across the real path. Then where the blade met (on the face, centre to rim, or on the arm or body) and the angle between the face and the blade's motion.
   - New counter: how deep any blade goes into another body or a shield, every step, outside a blow (wind-up, recovery, reaction, guard).
   - The headless set gains an overhead chop, a jumping strike and combos, at skills 0, 0.5 and 1, against the Medium and the Large.
   - Gate: today's numbers recorded, to measure every later step against.
   - **Done (5 November).** Before the change, at skill 1 on the training post: 23 of 27 single blows met on the board, the blade a median 0.99 of the board's radius from its centre, 13 on the rim. The face was 39° off square (median), up to 90°, edge on. The Medium's combos: 42 of 42 on the board, but 30 on the rim.
2. **The shield meets the blade on its path.** The shield's target and face come from the blade's path, as the parry's do (rules 2 to 4), at skill 1 first.
   - Gate: the expert on the training post blocks all 27 single blows, including the chop at the hips and the thrusts. Every block lands on the board's face, none on the arm, with the face within 20° of square before its slant.
   - **Done (5 November), headless gate passed; your playtest next, at shield skill 1.** At skill 1: 27 of 27 single blows on the board, the blade a median 0.43 of the radius from its centre, 3 on the rim. The Medium's combos: 42 of 42, 0.30 from the centre, none on the rim. In the spar the rim went from 15 of 27 blocks to 2 of 26. The face's median 33° off square is mostly the expert's 30° slant. The three left on the rim are a combo's follow-on played as a single blow, which starts too close to contact for the shield to see. High blows still meet the board only as high as the arm reached before; that is step 3. Skill 0.5 is no better yet; that is step 5. The reactions and the parry are unchanged.
   - **Steadied (5 November), after your playtest: the shield moved and turned back and forth as the blade came.** Three causes were fixed. The hand was placed from where the physical board was, so it flipped between two places every step. The face began its turn from straight ahead, not from the guard, a 35° snap. And it chased every read of the wind-up. The plan is now made once, held in the body's frame, moved only for a change over 10 cm or 25° (eased in), and the shield starts no sooner than 0.6 s before contact. Single blows at skill 1: the board travels 0.82 m to get 0.54 m (was 1.23), reverses at most 6 times (was 19), and the face turns 143° to turn 106° (was 212°). Still 26 of 27 on the board. Combo blows met late are step 4's work.
   - **Steadied further (5 November), after your playtest: no difference seen.** The hand now moves like an arm, speeding up and settling in over 0.08 s, held in the body's frame so it doesn't trail as the body moves. The face eases round over 0.1 s. A later read moves the plan only for a change over 20 cm or 40°. In the spar at skill 1, the board goes back on itself at most once per blow (was 6) and the face turns 79° to turn 59° (was 127°). The cost: blades meet it a little further from the centre, 6 of 25 on the rim (was 2 of 26). All five settings are on Combat Feel, under "Shield".
3. **High and low guards.** The arm lifts the board above the head, elbow out, for an overhead chop or a jumping strike, and drops it for a low cut. The board goes as far as the arm reaches from the shoulder; the elbow's direction follows the height.
   - Gate: at skill 1, the overhead chop, the jumping strike and a low cut are each met on the face.
   - **Done (5 November), headless gate passed; your playtest next.** At skill 1, high blows were already all met on the board after step 2 (13 of 13). Where the blade comes in out of the arm's reach, the board now meets it further along its way in, at the first place the arm reaches. Added at your request: the shield arm stays behind its own board. The elbow bends behind the board and out to the side, never into the chest, and the face turns no further than keeps the arm behind it. The arm through the board went from 270 steps to 0 in single blows, and from 241 to 0 in combos. Low cuts are rare in the set; only one was met, on the rim.
   - **Playtested (6 November): looks good.**
4. **Combos.** The shield takes the read-ahead of a combo's next blow, as the parry does. It plans to arrive by the time the blade does, and goes from the last block straight to the next meeting point.
   - Gate: the expert shield blocks all 42 blows of the Medium's combos and all 9 of the Large's, on the face.
   - **Begun (5 November), no change in play yet.** Measured first: the blade comes a median 0.14 m from where the shield planned to meet it in the Medium's combos, and 0.2 to 0.3 m in the Large's; the board's radius is 0.27 m. The first read of a combo's blow is off by up to 0.78 m. So the fault is the read of the blade's path, not the shield's moving. Three fixes were tried and left off, because none helped reliably: planning on a finer path about contact, one late correction, and a smaller slant. Next: measure the path read itself against the blade's real path, blow by blow, and fix the read where it is off.
   - **Read fixed (6 November), headless; your playtest next, at shield skill 1.** Measured blow by blow, the read of a combo's next blow had the body's place and facing about right (0.1 m, a few degrees). The blade itself was badly off: it was taken from the clip's hand bones, without our grip or the wrist's steer at the target. A chop's blade came out 0.6 m low, and a thrust's point over the head. The read now fits the blade at contact to where the strike's own plan will put it. At skill 1, the first read's error at contact for the Medium's combos fell from a median 0.81 m to 0.29 m, and the worst from 1.49 m to 0.45 m. All 42 of the Medium's combo blows now meet the board (was 39), though further from its centre (median 0.64 of the radius, was 0.47). The Large's forehand stays about 0.9 m off, because that blow falls short of where it is aimed. Skill 0.5 is unchanged; a middling shield waits to see a blow rather than reading it ahead. The gate is met for the Medium. The Large's set now plays only 3 combos (6 blows met), so its 9 are not tested. Also fixed from the 6 November logs: blow numbers were counted per attacker. When the dummy was set up again, its new thrust was taken for an old blow, and the walker's shield kept a 5 s old plan.
5. **Skill on the new geometry.** Lateness, misread and arm speed are tuned again, so a novice's misses look late, not wrong-facing.
   - Gate: at skill 0.5 the shield misses about 25% of single blows and 50% of combo blows (your numbers). Skill 1 misses none; skill 0 misses most. Then your playtest at each.
   - **Done (6 November), headless; your playtest next.** A blow read within 0.6 s of the last one is a combo's next blow. It is harder to read, and each kind has its own skill curve on Combat Feel. A blow is planned for only once the shield sees it, and a poor shield lets its guard drop between blows. The board now counts as round: blades past its rim, in the corners of its square collider, go by. Misses at skill 0.5: 26% of single blows (33% against the Large), 40% of combo blows (50% against the Large). At skill 0: 70% and 55%. At skill 1: 4% and 7%.
6. **Weapons rest against what they touch outside a blow.** The weapon hand is held off by the depth the blade would go in, eased in and out, the way the parry's bind holds a blade now. This covers both fighters, against the other's body and shield. Slashes in a blow still carry through.
   - Gate: no blade more than 2 cm into a body or a shield outside a blow. No new weapon jitter (the spar's weapon flick count).
   - Done (headless; needs a playtest). Outside a blow, a blade sitting in the other fighter's body or shield pushes its hand out along the shortest way out, eased in over 0.01 s and let go over 0.08 s. Steps more than 2 cm in fell from 165 to 89 against the Medium, 105 to 41 against the Small, 25 to 11 against the Large and 78 to 57 in the Medium's combos. The longest run fell from 12 steps to 5. The gate's "none" is not met yet: the blade goes back in during some recoveries, and in some the push grows while the blade stays in; why is not pinned down yet. The spar's weapon flicks are unchanged at 3. Hits, hit stops and drive-backs are as before.
7. **Weight in a block, again.** Weight step 4 (the shield arm knocked back, the guard knocked open, the attacker's rebound) is looked at again on the new shield. It is kept if it shows, and redone if it doesn't.
   - **Kept (6 November).** Your playtest: it looks fine on the new shield, so weight step 4 stays as it is.
   - Gate: your playtest.

Weight step 5 (giving ground) waits until this is done; its step back will use the shield's new meeting point.

## Risks and what stays

- **The reactions could be lost.** The bend, the hit stop, the drive-back, the stumble and the reaction clips must look as they do now. Each step's gate compares the dummy's bend, lean, hit stop and distance moved per blow against today's.
- **A held-off weapon can look stuck.** Resting the weapon against a body outside a blow could look as if it snags. It eases in and out, and is measured by the spar's weapon jitter count.
- **The arm can't always reach the path.** Some blows will cross the body where no shield position can meet them. Then it takes the nearest point it can reach in time, and the log says it was short. Later, weight step 5's step back gives it room.
- **Unchanged:** the parry, the slashes that carry through, the weight rules (hit stop, drive-back, stumble, beaten parries) and the walk. The fight's own severing is unchanged too.

## Decided (5 November)

1. **Slashes carry through.** A sword's slash keeps its sweep through the body; the reactions show the hit.
2. **Skill 0.5 misses 25% of single blows and 50% of combo blows.**
3. **The shield first.** The weapon at rest against bodies comes after it.
