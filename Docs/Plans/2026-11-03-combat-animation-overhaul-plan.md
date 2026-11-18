# Combat animation overhaul plan

> Copy of the Claude document <https://claude.ai/artifact/A9Jq1fRJm1AqfT62sCH49b>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 3, 2026 · @Renier

## Where we are

The new walk works, and the stances and every blow will be rebuilt the same way: from motion-capture clips, posed on our rig, with the walk's feet and rhythm kept underneath. Nothing goes into the fight until each step has passed in the lab.

**What works now (branch `locomotion-lab`).** One unit walks, runs, side-steps, turns, starts and stops from the Mixamo clips. Its feet are locked where they land; planted feet move less than half a centimetre. It carries a sword and shield with the arms still swinging and the wrists steadying them. All three bodies use it.

**What the fight does today.** Every pose is built by hand in code:

- The feet come from a stance that plants and steps them itself.
- The idle sway, crouch and Fight Rhythm postures are offsets added to the hips, spine and chest.
- Each blow is a hand-shaped path for the weapon tip (the `MD_` move definitions). The arm rig bends the arm to follow it, and the lunge moves the hips.

**Why it is being redone.** Those poses were tuned by eye from a few captures. They do not share the walk's motion, so joining the two looked wrong: squatting, crossed legs, bodies hanging over their feet. The new clips give each body a real stance, real footwork and real strikes to start from.

**What stays.** The gameplay around the poses is kept:

- seeing and choosing targets;
- the tempo and the Fight Rhythm's choices;
- combo choices;
- damage, poise and severing;
- hits as physical contacts, with the physical body held by pins.

The pillar stays too: a blow locks the target body part when it starts, and the strike is aimed there. Only where the poses come from changes.

## The reference clips

The 37 clips give the Medium a full guard: stance, footwork, turns, a block and about ten single strikes. The Large has a two-handed guard, footwork and about eight strikes. The Small borrows from both. Measurements are the Mixamo actor's (leg 0.89 m) and are scaled to each body.

**The guards.**

| Body | Guard clip | Feet | Hips | Hands |
| --- | --- | --- | --- | --- |
| Medium | Sword And Shield Idle (three takes: 2.5, 3.7 and 8.7 s) | Left foot 0.36 m ahead, right 0.40 m behind, 0.45 m apart | Turned 40°, 12 cm lower than standing | Shield hand forward at chest height; sword hand back by the right shoulder, sword up |
| Large | Large\_Great\_Sword\_Idle (three takes) | Left 0.18 m ahead, right 0.17 m behind, 0.31 m apart | Turned 15°, upright | Both hands together on the weapon, in front of the chest |

**The footwork.**

| Body | Clips | What they are |
| --- | --- | --- |
| Medium | Sword And Shield Walk, Walk Backwards 02 | Walking with the weapons up, forward 1.5 m/s and back 1.1 m/s |
| Medium | Strafe 1, Strafe 2 | Wide guard side-steps, left and right, about 1.1 m/s, feet 0.8 m apart |
| Medium | Strafe, Strafe Reverse | Fast crossover runs on a diagonal |
| Medium | Run, Run Backwards | Running with the weapons, 4.1 m/s forward and back |
| Medium | Turn (two, left and right), 180 Turn (two) | Turning in the guard, 99° and 180° |
| Large | Great Sword Walk, Strafe, Strafe 02 | Walking forward and side-stepping both ways, weapon in both hands |

**The strikes.** Each strike's peak hand speed marks its contact moment.

| Body | Clip | Strikes in it | Use |
| --- | --- | --- | --- |
| Medium | Slash 3 | One forehand cut, diagonal down, 13 m/s, in place | Single strike |
| Medium | Slash 1 | One rising backhand cut, 10 m/s, in place | Single strike |
| Medium | Attack 2 | One high thrust over the shield, 9 m/s, in place | Single strike |
| Medium | Attack 1 | One stepping backhand, 11 m/s, a lunge of about 2 m | Single strike with a lunge |
| Medium | Slash 2 | One overhead chop with a side step, 10 m/s | Single strike |
| Medium | Attack 3 | A rising cut, then an overhead chop, with a lunge of 3.4 m | Cut into 2 strikes |
| Medium | Slash | Four cuts while advancing 1.4 m and stepping 2 m aside | Cut into 3 or 4 strikes |
| Medium | Kick | A front kick | A new attack (a question below) |
| Medium | Block | Shield brought across, 0.6 s | The shield block |
| Large | Great Sword Slash 02 | Two heavy two-handed swings, in place | Cut into 2 strikes |
| Large | Great Sword Slash | Three two-handed strikes while advancing 3 m | Cut into 3 strikes |
| Large | Two Hand Club Combo | Five heavy swings, 4 to 5 m/s | Cut into single strikes |
| Large | One Hand Club Combo | A jab, then two downward smashes, 7 to 10 m/s | Cut into 3 strikes |
| Large | Great Sword Kick | A kick from the two-handed guard | A new attack (a question below) |

**Duplicates.** Attack and Attack 1 are the same clip, and so are Turn and Turn 1. One of each is kept.

**The Small's picks.** For the Small's one-handed club, light and quick:

- Guard: the Medium's idle, with the free hand forward where the shield hand is (a stick fighter's checking hand).
- Footwork: the Medium's guard side-steps, walks and turns.
- Strikes: Slash 3, Slash 1 and Attack 2 from the Medium, plus the jab and two smashes from One Hand Club Combo.

**Gaps.** Nothing is missing that blocks the plan. These are filled by blending and would only be nicer with a clip:

- a walk settling into the guard, and the guard setting off into a walk;
- single guard steps forward and back;
- turns and a backward walk for the Large;
- a block for the Large.

## Cutting single strikes from the clips

Every strike becomes its own short clip that starts and ends in that body's guard. Combo clips are only material: they are cut apart, and our combos chain single strikes.

**Where the cuts go.** Each strike is found by its weapon hand's speed:

1. **Wind-up:** from the last moment the hand is nearly still before the peak.
2. **Strike:** the peak, which is the contact moment.
3. **Recovery:** until the hand is nearly still again.

The automatic cuts are a first guess; I'll check each one in the film and move it by hand where needed. A cut in the middle of a combo usually starts from the previous strike's recovery rather than from the guard. Those strikes get a short blend from the guard at the start, and back to the guard at the end, the same way the walk blends between clips.

**What each strike records** (baked like the walk clips, so the game reads them the same way):

- the whole body's pose every frame, against the clip's own root;
- the root's travel and turn: the lunge and any step, so the feet step for real and stay planted;
- each foot's contacts;
- its marks: wind-up start, contact, recovery end, and the window when a next strike can follow;
- its kind and height: forehand, backhand, rising, downward or thrust; high, middle or low. The planner uses these to pick a strike that suits the target;
- the weapon's line at contact. The clip has no weapon in it, so this comes from the hand's turn and our grip, with our weapon's own length.

**The library, per body.** First count from the cuts:

| Body | Forehand | Backhand | Rising | Downward | Thrust | Not yet sorted | Kick | Block |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Medium | 2 | 2 | 2 | 3 | 1 | 0 | 1 | 1 |
| Large | 0 | 1 | 0 | 2 | 3 | 7 slower two-handed swings | 1 | 0 |
| Small | 1 | 0 | 1 | 2 | 2 | 0 | 0 | 0 |

The Small's strikes are clips it shares with the other two; each is played at its own size and speed.

## The stance on top of the walk

The guard is a second set of clips for the same walk system, not a separate pose builder. Everything the walk does now (locked feet, blends at a shared phase, starts, stops, turns, steadied hands) works the same in the guard.

**Two sets, one walker.** The walker gets a relaxed set (today's clips, with the carried weapons) and a guard set per body:

|  | Relaxed (now) | Guard (Medium) | Guard (Large) |
| --- | --- | --- | --- |
| Standing | Idle | Sword And Shield Idle, three takes | Great Sword Idle, three takes |
| Forward and back | Walks, run | Sword And Shield Walk, Walk Backwards 02, Run, Run Backwards | Great Sword Walk; backward from the relaxed walk with the guard's arms |
| Sideways | Side-steps, crossovers | Strafe 1 and 2 (wide guard steps) | Great Sword Strafe and 02 |
| Turning | Turns on the spot, pivots | Turn left and right, 180 Turn | The relaxed turns with the guard's arms |

**Going into the guard and out of it.** A guard weight moves from 0 to 1 over about half a second. Both sets play at the same gait phase, so a step that is under way finishes as one step. Standing, the feet stay planted, and the walker steps them into the guard's wider stance with one or two short steps; it does not slide them. The arms blend from the relaxed carry into the guard's hands.

**The guard's arms come from the clips.** The hands follow the mocap actor's hands, which are already holding a sword and shield. The weapon's angle is the clip hand's turn with our grip. If any wrist bends as it did in step 1, I'll straighten it the same way: the wrist steadies the weapon and does not bend it.

**The guard faces the enemy.** The guard clips stand side-on: the Medium's hips are turned 40° with the left foot leading. The walker's facing is the head's, so the head and the shield face the enemy while the hips stay turned, as in the clip.

**What happens to the Fight Rhythm.** The Fight Rhythm keeps deciding what to do: settle, circle, advance, back off, feint. Its moves become requests to the walker: a velocity, a facing and which guard clip to play. Its hand-made postures (hip shifts, lean, bounce) are switched off at first, because the clips have their own sway. They come back only as small layers if the guard looks too still.

## Aiming a clip strike at the real target

A strike plays its clip, and three small, measured corrections bend it so the weapon passes through the body part locked when the blow began. The clip keeps the motion; the corrections only aim it.

**At the start**, the planner picks a strike whose kind and height suit the target, and works out where the clip's weapon would be at contact. It then sets three corrections:

1. **Facing:** the body turns by the angle between the clip's contact point and the target, at most about 30°, spread over the wind-up. The feet step the turn as the walk's turns do.
2. **Distance:** the lunge (the clip's root travel) is stretched or shortened so contact happens at the target's distance, between about 0.6 and 1.4 times its own length, as the walk's stops are. Beyond that range a different strike is picked, or the unit steps in first.
3. **Height and the last few centimetres:** the weapon hand is pulled toward the line that passes the weapon through the target. The pull rises through the wind-up, is full at contact, and is gone by the end of the recovery. The spine takes a little of a big change in height, as a crouch or a reach.

**Timing.** Contact happens at the clip's own contact frame. The tempo can play a strike a little faster or slower, as it does now.

**Feet.** The lunge's steps are the clip's own, so planted feet stay planted. The walker hands the feet to the strike and takes them back afterwards, as it does with its start and stop clips.

**Physics is unchanged.** The physical body is held to this pose by the same pins:

- what the weapon touches takes the hit;
- a blocked or stopped swing cuts the strike short and blends back to the guard;
- recoil, stumbles and knockdowns work as they do now;
- the target body part locked at the start is still what the blow aims at, so a dodge still makes it miss.

## Combos from single strikes

A combo is a chain of single strikes. Each next strike starts from the end of the last one, so no combo clip is played as it is.

**Which strike can follow which** is worked out once, when the clips are baked:

- For every pair of strikes, a cost compares where the first strike's recovery leaves the body with where the second strike's wind-up starts. It looks at the hands, the weapon's line, the hips and the feet.
- A low cost is a natural follow-on: a forehand into a backhand, or a rising cut into a downward chop.
- The pairs the mocap actor did in a row (in the combo clips) are kept as good links, whatever the cost says.

**Playing a link.** The first strike is cut short in its link window, after contact and before its recovery ends. The next strike then starts from about where its wind-up begins. The walker's switch blend covers the small difference, as it does between walk clips. The feet stay planted unless the next strike steps.

**Choosing the links stays the combo system's job.** It already decides when to chain, against which target, and when to stop: blocked, out of reach, out of energy. From the strikes that can follow, it picks one that suits the target's height and side.

## Being struck, and blocking (added 3 Nov, night)

The playtest after step 5 showed four things. The dummy barely reacts, even to the Large. It never gets driven back. Blocked blows feel like nothing. Many strikes miss in Unity that land in the headless runs.

**Why strikes miss in Unity but not headless.** In the lab, the striker's weapon arm is held by pins, so the physical arm and the sword seen on it trail a 16 m/s swing. The weapon is swept along that lagging path. The log's aim check uses the posed arm, and says the blade passed 0–6 cm from the target while nothing was struck. The fight already deals with this: it makes the weapon arm kinematic during a blow and for 0.4 s after. The lab will do the same, for the striker's weapon arm and for the dummy's arm when it raises its shield.

**The dummy is an enemy unit that doesn't fight back.** It reacts by the fight's own rules, with nothing dumbed down:

- **Driven back.** What the struck part doesn't take moves the whole body. Heavy blows step it back, and a Large's blow staggers a Medium back a few steps.
- **Reaction clips** from the sword and shield pack, chosen by the reaction the damage rules give. A light hit plays the light flinch (impact). A flinch to the body plays the doubling-over (impact 2). A flinch to the head, or any stagger, plays the heavy recoil (impact 3). A hit strong enough to step it back plays no clip: the walk's own steps do the reaction. The pose's recoil springs go on top of either.
- **Blocked blows connect.** The shield arm is knocked back hard, the body rocks, and the rest of the blow pushes it back, by the fight's block rule. A Large's blocked blow drives a Medium back a step.

**Blocking means meeting the blow.** A blocking dummy raises its shield with the pack's block clip and holds it with the block idle. From the moment the striker's blow starts, it tracks the weapon. With a reaction delay and a limited shield speed, it puts the shield between the blade and the part the blow is aimed at, face toward the blade.

**Combos press.** A combo's strike that drives the dummy back would leave the next strike out of reach. When no follow-on would land from where the striker stands, it takes a guard step toward the dummy until one would, for at most a second, then goes on. That is the start of step 7's footwork.

**Which way a follow-on faces.** Some strikes turn the body a long way: the combo forehand turns it 120° by its end, the backhand lunge 80° by contact. The next strike then needs a large turn. A follow-on that needs less turning is now preferred among those that land.

**Done overnight (4 Nov), checked headless, to be playtested.**

- The weapon arm is held kinematic while a strike plays and for 0.4 s after, in the lab as in the fight.
- Reactions: the pack's impact clips by how hard the blow was. Driven back with steps after the reaction shows. The Large drives a Medium back 0.3–0.9 m.
- Blocking: the dummy reads where each blow will land and the way it will come in from the striker's wind-up, and meets it with its shield face-on. It blocks all 18 Medium strikes but one (the 0.18 s combo backhand), and all 6 of the Large's.
- The 90° follow-on had a different cause than the turn. A strike stopped by the body ended early, but its follow-on was linked as if it had played out, so the next strike was turned 60–90° off the dummy. Now a stopped strike's follow-on starts from where it stopped, or from the guard.
- Combos press after a driven-back dummy, and each strike's hand follows its target: the Medium and Small land 27 of 27, the Large 7 of 8.
- The walk is unchanged: 104 plants, 0.3 cm.

## The steps

Eight steps, each shown to you in the lab and passed by you before the next begins. The Medium goes first; the fight is last.

1. **Clips baked and checked.** The guard clips and every cut strike are baked onto our Medium, and I send films of each.
   - Gate: each guard and strike looks like its clip on our body, with no wrist or knee problems; you agree with the cuts.
2. **The guard in the lab (Medium).** The lab's unit goes into the guard and out of it (a key), stands, side-steps, walks back and turns in it, driven or on a course.
   - Gate: the walk's quality is unchanged; feet stay planted; going into and out of the guard looks natural.
3. **Single strikes at a dummy (Medium).** A dummy stands in the lab with target points at different heights and distances. A key plays each strike, aimed at the chosen point.
   - Gate: each strike lands where it aims; lunges step and stay planted; the clip's motion is still recognisable.
4. **Hits at the dummy.** The dummy's body takes physical hits, and a blocked swing cuts short and blends back.
   - Gate: contacts land at the contact moment; nothing pops on a cut-short swing.
5. **Combos at the dummy (Medium).** Chains of single strikes, chosen by the follow-on costs.
   - Gate: each link flows; no pops; feet planted.
6. **The Large, then the Small.** Steps 1 to 5 again for each, from their own clips and the Small's picks.
   - Gate: the same, per body.
7. **Moving in the guard against an opponent (no blows).** Two units circle, approach and back off using the fight's own movement choices.
   - Gate: footwork looks right at fight distances, including the turns.
8. **The fight.** The fight's pose sources are swapped one at a time (guard first, then strikes, then combos), with a switch to the old ones until each has passed.
   - Gate: your playtests.

**Progress, 3 November:**

- **Step 1 passed.** Renier checked the clips in Unity. The issues he found are fixed: a pose left stuck after a clip, a lifted foot left in the air, the club jumping when the grip changed, and the body sagging while paused.
- **Step 2 passed** the playtest. T raises and lowers the guard in the lab.
- **Step 3 is built and waiting for a playtest.** A strike is aimed at a training dummy four ways: the body turns into it (up to 35°), the lunge stretches or shortens (0.6 to 1.4 times), the arm reaches up to 0.4 m, and about contact the wrist turns the blade onto it (a thrust's all the way, since the clips' thrusting hands don't show where the blade points). Each strike's reach on each body is measured into StrikeReach.json. For targets near a strike's own reach, the blade passes within 5 cm in 228 of 243 aims for the Medium, 193 of 243 for the Small and 295 of 297 for the Large. Each strike has a natural height and distance, so choosing the strike that fits the target is left to step 5 (combos) and step 7 (moving in the guard).
- **Step 3 passed** the playtest. The logs showed four problems, now fixed: the combo's backhand was cut at the top of a leap (re-cut where both feet are down); a foot snapped when the guard came to a stand; arms whipped after overhead strikes; and a small pull back while stopping counted as reversing.
- **Step 4 is built and waiting for a playtest.** The dummy is now a real body (a Medium with its own loadout), physical like the walker. It stands open, in its guard, or blocking (O). The weapon is checked every step around contact, and the first part or shield it meets is struck by the fight's own damage rules. Both fighters freeze briefly for the hit stop, the struck part is knocked and sways back, and the dummy steps back. A blow that cuts through slows down. A stopped blow is shown where it met, backs out a little and fades into the guard. Headless against a dummy at each strike's reach, the Medium lands 18 of 36 strikes and has 18 blocked.
- **Step 4 playtest:** the dummy now reacts in its pose: the upper body is thrown back, the head snaps, a struck arm or the shield is knocked aside, and it steps back. Pressing J no longer restarts a strike that is already playing. The Large's own two-handed clips looked wrong, so for now **the Large uses the Medium's clips**, with its club in the sword hand and the shield hand empty. One switch (`LocoLibrary.LargeOwnClips`) brings its own clips back.
- **Step 4 passed.** The Large uses three of the Medium's strikes: the backhand lunge, the combo forehand and the forehand. Its long lunge no longer knocks it down.
- **Step 5 is built and waiting for a playtest.** Every pair of strikes has a follow-on cost, with the hips lined up before the poses are compared. A combo chains up to three strikes, each the best follow-on of the last. At each link the root turns and moves so the next clip's hips land on the last one's. The clips' recoveries return to the guard and their wind-ups leave it, so the best links are end-to-start, and combos keep the actor's pace. Headless, the Medium lands 24 of 27 strikes in combos. The misses mostly come from the fighter ending up too close or too far, which step 7's footwork handles.

## Questions

Renier answered on 3 November. The decisions are:

1. **The Large's grip: both.** The guard is two-handed, as the great sword clips are. The one-handed strikes (the jab and the smashes) let go with the off hand and take it back at the end.
2. **Kicks: later.** They are pinned to come back to after the strikes, because they change the gameplay.
3. **The Fight Rhythm's postures:** use the new clips wherever they can be used.
4. **Raising the guard:** within 3 m of an enemy.
5. **The Small's picks:** as listed, for now.
6. **Lead foot:** as the clips have it (the Medium leads with the left).
