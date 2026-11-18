# Arms that bend as a body does, and the shield: findings and plan

> Copy of the Claude document <https://claude.ai/artifact/PwoViLF1TYzcBSx7yQ1QrK>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 12, 2026 · @Renier

## The short version

**Now is the right time.** The shield arm problem and the "arms bend in ways a body does not" problem are the same problem, and the legs will need the same fix for knees, hips and ankles. Doing it before the legs means the legs are built on it instead of repeating it.

**What I found.** Today nothing stops an impossible bend while a unit is on its feet. The recorded animations themselves are fine: measured as a person's arm, they stay inside a person's range almost all the time. Every bad bend you see is added afterwards by our own code, when it moves a hand somewhere and lets the elbow and wrist fall wherever the maths puts them. The physical body then copies the bad pose, because its joints have no stops either.

**What I propose.** Three things, in this order:

1. One description of what an arm can do, written the way a body works (where the elbow can be, which way it can fold, how far the forearm rolls, how far the wrist bends), checked against every recorded frame.
2. One gate at the very end of the pose, after everything else has had its say. Whatever arm comes in, a possible arm goes out. No code path can go around it, so a new feature cannot bring the problem back.
3. The same limits as hard stops in the physical joints, so a blow cannot bend an arm past them either.

The shield then gets rebuilt on top of that: held the way a centre-grip round shield is held, taking a blow through the arm into the body. After that come real weapon weight and the legs.

Nothing in the game's behaviour has changed yet. This turn I built the measuring tools and wrote this plan.

## Where the limits are in the stack today

You said you do not know where the bend limits live. The answer is: almost nowhere that matters.

&#91;embedded content: where bend limits are held in the pose stack · 6 stages, 2 new\]

Read it top to bottom: a clip is recorded, arms are built from it, our code moves the hands about, and the physical body follows. Red dots are stages with no limits. The blue box and the note on stage 5 are what this plan adds.

The details, for the record:

- **The limit table** (`JointLimitTable` in `Muscles/JointMath.cs`) is only applied to the old ragdoll joints, so it only acts once a unit has fallen. While a unit stands and fights it does nothing.
- **The standing physical body** (`UpperBodyArticulation`) sets every joint to turn freely in all three directions. There are no stops in it at all.
- **The pose** places arms with a two-bone solver (`TwoBoneIK`): give it a hand position and a rough elbow direction and it returns an arm. It never asks whether a body could do that. The shield, the knocks, the strike aim and the tucked free arm all use it.
- **The one exception** is the sword arm while it parries (`ArmRig`), which has its own limits. That is the piece built earlier.

### Why the earlier attempt did not hold

Three reasons, none of them careless:

1. **It guarded one door.** The limits were put inside one stage (the parry arm). Every other stage that moves an arm went on without them, and each new feature since has added another unguarded door.
2. **The limits were written in the wrong language.** The table describes each bone's rotation as three angles in the bone's own frame. That suits a hinge. It does not describe a shoulder, and in our pose a bone's roll about its own length is arbitrary, so the same visible arm can read as legal or illegal. A check written that way gives false alarms on good arms and misses bad ones.
3. **The physical body had nothing behind it.** Even a perfect pose is not enough if a blow can then fold the physical arm any way it likes.

## What I measured

I wrote a reading of an arm that works the way a body does, from where the joints are, and ran it three ways: on the recorded clips alone, on the pose the game makes from them, and on the physical body.

&#91;embedded content: headless fights · two 60 s battles of six, measured 12 November 2026\]

What the picture does not show:

- **Which bends.** The shield arm's fault is almost all one thing: the elbow folds the wrong way, up to 90 degrees past, for half a second and more at a time. The swordsman's free arm is a wrist held bent well past its range nearly all the time. The sword arm goes out at the wrist and in the forearm's roll during a strike, and at the elbow and shoulder when it is knocked.
- **The physical body copies the pose.** Its numbers are within a point or two of the pose's in every row. It has no stops of its own.
- **Your own sessions say the same.** In the three target-test logs you pushed, the shield target's arm is past a person's range 14 to 15% of the session, the free arms 31 to 96%, the attackers' sword arms 10 to 24%.
- **The shield inside its owner.** The shield or its arm is more than 2 cm inside its own head 1 to 4% of the time (10 to 13 cm at worst), and inside its own trunk 2 to 8% (17 to 24 cm at worst).
- **The yardstick holds.** Over about 4,700 recorded frames, the walks, turns and hit reactions are never out of range, and the combat clips are out 1 to 3% of the time, nearly all of it a wrist reading. So the reading can be trusted, and its ranges will be widened a little until every recorded frame passes. The Large's club strikes are the one row where the clip itself reads high, which is that same wrist range being a little tight.
- **Legs.** A rougher check against the old table says feet and thighs are out of range 3 to 20% of the time. I have not built the proper reading for legs yet. That belongs in the legs work, on the same system.

The tools are in the repo: the fight report now prints these numbers, `Tools/Playtest/armrange.py` reads them from any playtest log, and `armrange` scans the clip library.

## The design

### 1. One arm, described the way a body works

An arm is seven numbers, and each has a range a person can reach:

| What | In plain words | Range to start from |
| --- | --- | --- |
| Where the elbow is | How high the upper arm is raised, and toward where | All the way up in front and out to the side. Behind the body and across the chest, the trunk stops it: about 55 degrees straight back, 30 to 40 across. |
| Which way the elbow folds | The upper arm's turn about its own length | About 100 degrees in toward the body, 90 out |
| How far the elbow bends | Straight to fully bent, one way only | 0 to 150 degrees |
| The forearm's roll | Palm up to palm down | About 90 degrees each way |
| The wrist | Bent toward the palm or the back of the hand, and sideways | About 80 each way, and 30 to 55 sideways |

This is read from where the joints are, so it describes what you see. It does not depend on how a bone happens to be rolled inside the rig, which is what made the old check unreliable.

The ranges are set by one rule: **every frame of every recorded clip must pass.** A real person made those movements, so they are the proof of what a body can do. Where the clips go further than my starting numbers, the numbers move.

### 2. One gate at the end of the pose

After every stage has run, each arm goes through one function. It reads the arm as the seven numbers, brings any that are out of range back to the nearest value in range, and rebuilds the arm from the result.

The point of rebuilding is the guarantee. The arm that leaves the gate is made from numbers that are in range, so it cannot be an impossible arm. It is not a check that might miss a case.

When a stage asks for a hand position no arm can reach legally, the gate gives the nearest arm that can exist and the hand stops short. The stage does not get its way.

Two things keep this from looking mechanical:

- **It is quiet when nothing is wrong.** An arm that is already in range passes through untouched, so the recorded clips and the walk are not changed.
- **It moves smoothly.** The correction grows gradually as an arm goes out of range, so nothing snaps at the edge.

### 3. Fix the worst offenders at the source

The gate is the guarantee, not the quality. If a stage asks for an impossible arm half the time, the gate will hold it, but the result will look held back. So the stages that cause most of the trouble get rewritten to ask for possible arms in the first place: the shield (below), the swordsman's tucked free arm, and the knocks to a struck arm.

### 4. Hard stops in the physical body

Once the pose's bones are built from the seven numbers, the physical joints can be given real stops that mean the same thing: the elbow becomes a true hinge that bends one way, the wrist and shoulder get their ranges. Then a blow can push an arm to its limit and no further, and what is left of the blow goes into the body, which is what should happen.

This has to come after the gate. With stops in the body and an impossible pose asking it to go past them, the body and the pose would fight each other.

### 5. The same system for the legs

Knees, hips and ankles get the same treatment in the legs work: a reading, a range checked against the clips, the gate, then stops. Nothing here needs redoing for them.

## The shield

### How this kind of shield is held

Ours is a round shield, 58 cm across, held in the fist by a handle at its centre. The recorded guard holds it exactly that way: elbow down and a little out, bent about 90 degrees, forearm pointing at the enemy, fist about 30 cm in front of the chest with the board ahead of it. So I am modelling it as a centre-grip shield, which is what the animation already is.

What fighters who use these say about them ([Tactics in Viking single combat with sword and shield](https://www.science-and-fiction.org/swordfighting/sword_and_shield.html)):

- It is held out from the body and **pushed out to meet a blow**. It is not a wall held against the chest.
- For a high blow the shield is raised; for a low one it is lowered and the fighter crouches a little. The body goes with it.
- The hand sits at the middle, so a hard blow on the flat away from the centre **turns the board in the hand**. A blow caught on the rim, or on a slanted face, slides off.
- A block is not overcommitted. A heavy shield is slow to bring back.

### What ours does today

- It flies the hand to the point where the blade will arrive, then asks the solver for whatever arm reaches there. That is where the fist above or behind the head comes from, and the elbow folded the wrong way.
- The board spins freely in the fist to face the blade. No wrist is involved, so there is no limit on it.
- When it is hit, the hand's target is shoved back and the solver again picks whatever elbow fits. That is why the absorbing looks wrong: the arm is not taking the blow, the hand is being moved and the arm follows however it can.
- Nothing keeps the board out of its owner's head or body.

### What changes

1. **Arm first.** The shield moves between a few wards, each one a real arm position: middle (the recorded guard), high (fist at brow height in front of the face, board tilted back so its top rim covers the head), low (arm lowered and pushed forward, with a crouch), inside (across toward the sword side, as far as an arm goes across the chest) and outside. Meeting a blow means moving between these so the board's face is on the blade's line. The fist never goes above the head or behind the face, because no ward has it there.
2. **The board is fixed in the fist.** It turns as far as the wrist and the forearm's roll allow, and no further. Its facing comes mainly from where the forearm points, as with a real one.
3. **It cannot enter its owner.** The board and the arm are kept outside a shell round the head and trunk. Later, real collision between the shield and its owner's body backs that up.
4. **A blow goes through the arm.** The elbow bends and the fist comes back toward the shoulder along the blow's line, the shoulder and trunk rock with it, then the arm pushes back out. A blow off the centre turns the board about the fist, within the wrist's range, and it comes back. A heavy blow drives the whole body back a step, which the heavy-weapons work already does.
5. **What the arm cannot reach, the body helps with or the block fails.** Turning the trunk, ducking and stepping come first. If that is not enough, the blow gets through. That is an honest result and it reads correctly.

## The steps

One at a time, each playtested by you before the next, as we have been working. Each ends with a README entry and a note here.

| Step | What changes | What you should see | How I check it my side |
| --- | --- | --- | --- |
| 1. The measure | Nothing in play. The reading, the clip scan and the log tool. | Nothing | Done this turn. A standing unit that nothing touches reads 0%. |
| 2. The gate | Ranges set so every clip frame passes. Every arm goes through the gate at the end of the pose. | No arm bends the wrong way, on any unit, at any moment. Some hands stop short of where they used to go. | Pose past range goes to 0 in every row of the chart. Blows landed, blocked and parried stay close to today's counts. The walk and the clips are untouched. |
| 3. The sources | The swordsman's free arm and the knocks to a struck arm ask for possible arms. | The free arm hangs and moves like an arm. A struck arm is thrown the way an arm goes. | How much the gate has to correct drops toward nothing for those arms. |
| 4. The shield | Wards, the board fixed in the fist, kept out of its owner, a blow taken through the arm. | The target test: the shield held like a shield, a blow visibly taken by the arm and the body, never in the head. | Shield inside its owner: 0. Blocks that should be made are still made. |
| 5. Stops in the body | Elbow a true hinge, wrist and shoulder ranges, as hard stops in the physical joints. | A blow can push an arm to its limit and no further. | Body past range: 0, with heavy blows landing on arms. |
| 6. Real weapon weight | The weapons get their real mass in the physics, and the hit comes from that mass. | The thing you asked to see: the weight of a blow coming from the weapon itself. | The weight numbers from the heavy-weapons step, with the scripted multipliers turned down. |
| 7. The legs | The grounded-legs plan from its step 0, with the leg ranges joining this system. | As that plan says. | As that plan says. |

Steps 2 and 5 are the two halves of what you asked for: bends that cannot happen in the pose, then bends that cannot happen in the body. Step 6 sits after step 5 on purpose. A heavy weapon swung on an arm with no stops would wrench that arm into exactly the bends we are removing.

## What will change in play

- **Some blocks and blows will fall short.** Today an arm can cheat to get a shield or a blade somewhere. After the gate it cannot. A shield asked to cover a spot no arm reaches will not get there, and a strike steered a long way off its recorded line will land a little short. I will report the landed, blocked and parried counts before and after each step, so you can see how much.
- **The clips and the walk should not change.** They are already in range, and the gate leaves an arm in range alone.
- **The sword arm's parry** keeps its own limits for now and moves onto the shared ones during step 3, so there is one set of rules, not two.

### Risks I can see

- **A nearly straight arm.** Which way a straight elbow folds cannot be read from where the joints are. The gate has to carry that over smoothly from the moment before. This is the same trap as the limb that spun, and I will test for it directly.
- **Hands held back too often.** If the gate has to correct a lot, movement will look restrained. Step 3 exists to prevent that, and the measure will show it.
- **The body's stops fighting the pose.** That is why step 5 comes only after the pose is clean.

## Decisions that are yours

1. **The order.** I recommend the order in the table: the gate, the sources, the shield, the stops, real weapon weight, the legs. If you would rather see real weapon weight sooner, it can move ahead of the shield, to straight after step 2. It should not go before step 2.
2. **When the shield cannot reach.** I recommend the block simply fails, and that we improve how the body turns and ducks to help the shield as a later step. The other choice is to build the body's help into the shield step, which makes that step larger.

If you agree with both recommendations, say go and I will start step 2.

## Progress

### Step 2, the gate: built, ready for your playtest (12 November)

You agreed to both recommendations, so the order stands and a shield that cannot reach simply fails to block.

**What is in.** Every arm now passes one gate after everything else has moved it, just before the pose goes to the physical body. On the Combat Feel object there are two new fields under "Arms that bend as a body does": **Arms Held To A Body** (untick it to compare with before) and **Arm Goes Over At** (how fast an arm swings round when it has to go the long way, 600 degrees a second).

**What I measured** (60-second fights of six, headless):

| Share of the time over 10 degrees past a body's range | Pose before | Pose with the gate | Physical body with the gate |
| --- | --- | --- | --- |
| Swordsman's free arm, in guard | 85% | 0.0% | 0.0% |
| Shield arm, just struck | 24% | 0.6% | 10% |
| Swordsman's sword arm, just struck | 16% | 0.7% | 3.6% |
| Large's club arm, striking | 13% | 0.0% | 0.4% |
| Shield arm, in guard | 4.5% | 0.0% | 1.6% |

**What to expect when you look.**

- **The pose no longer asks for impossible arms.** What is left (under 1%) is an arm in the middle of swinging over.
- **The physical body can still be knocked past the range.** A struck shield arm is still out about 10% of the time (20% for a Large target hit by Larges), down from 24%. The body has no stops of its own until step 5, so a blow can still fold it. You will still see some bad bends on struck arms; they should be fewer and shorter.
- **Blocks fall short more often.** Shields blocked 30% of blows where they blocked 37%; 47% landed where 38% did. In the target test the shield target blocked 17 of 54 blows from Mediums where it blocked 20.
- **An arm sent the wrong way round now swings over the right way.** When a stage flips an elbow to the impossible side, the arm goes round through the possible side at the pace above. That is a visible swing of a few tenths of a second where before the arm sat bent wrongly.
- **The shield still goes into its owner** as often as before. That is step 4.

**Two things I found on the way.**

- Our own stages jump an arm joint by more than 60 degrees in a single step about once a second across six units. That is separate from impossible bends, and it is the likely source of much of the snapping you see. The gate adds 2 to 5 such jumps a minute of its own, at the edge of the wrist's range. Both go on the list for step 3.
- The gate is correcting the swordsman's free arm nearly all the time, and the shield arm a third of the time after a blow. That is the gate doing its job, and it is also the measure of how much step 3 and step 4 have to fix at the source.

**Not yet seen in Unity.** Everything above is from the headless runs.

### Step 2 seen in Unity (12 November, evening)

Your words: "working pretty damn well", "have not seen true issues". Your 169-second session agrees with the headless runs. Against the game's own ranges, the swordsmen's free arms were past a body's range 24 to 94% of the time before the gate and 0.0 to 0.3% after; the shield target's arm went from 14% to 5%.

### The shield, first half: built, ready for your playtest (12 November)

Everything you flagged was about the shield, so I moved the shield ahead of the free arm and the knocks. Three changes, all on the Combat Feel object under "Shield: how it moves to meet a blow":

- **Shield Fixed In Fist.** The board never turns in the hand. In the pose the grip stays as it is, and the physical board is now one piece with the hand, so a blow cannot turn it in the fist either. The hand turns on the forearm by at most **Shield Wrist Turn** (15 degrees). To place the board, the whole arm moves: the fist goes where the board is wanted and the elbow stays down.
- **Kept out of its owner.** A board the pose brings within 6 cm of its own head or trunk is moved off, the fist with it.
- **Shield Reads From** (0.5). The shield does not start to move until half of the blow's time, from its start to its contact, has passed.

|  | Before | Now |
| --- | --- | --- |
| Board turned in the fist, on average | 71° | 0° |
| Elbow far round from its lowest: in guard, just struck | 15%, 46% of the time | 0.6%, 3.6% |
| Elbow above the shoulder: in guard, just struck | 11%, 40% | 0.7%, 6% |
| Shield first moves, through the blow | 30% of the way | 51% |
| Physical shield arm past a body's range, just struck | 10% | 2.5% |
| Blows blocked, in fights of six | 30% | 33% |

**Still to do on the shield (second half).** A blow is not yet taken through the arm into the body. The physical board can still be knocked into its owner by a heavy blow, because its own body does not stop it: a Large's shield struck by a Large is inside its own trunk about a quarter of the time just after the blow, as before. That needs the body-stops step, or real collision between the shield and its owner.

**Also done.** The battle scene's debug panel and unit labels are drawn 1.8 times the size, with "Panel size" buttons at the top of the panel.

**Not yet seen in Unity.** One thing to know: if you untick Shield Fixed In Fist while playing, the physical grip only changes at the next Reset.

### The shield, corrected: the board faces the enemy (12 November, late)

Your playtest: "the shield is now pointing the wrong way by default". That was my mistake. When I locked the board in the fist I kept the old grip's resting direction, which has the board facing along the forearm. The old code hid that by turning the board in the hand all the time.

**What the recording says.** In the recorded guard the back of the hand faces the enemy, the thumb points up, and the forearm lies across behind the board. So the board now faces the same way as the back of the hand, as you described.

**How the arm places it now.** The arm keeps the shape the guard gives it and swings about the shoulder until the fist is where the board is wanted, then bends or straightens to reach. The elbow tucks down a little more as it braces. The board goes round with the arm, so it faces out from the shoulder: up when raised, to the side when swung wide. The hand stays on the forearm as the guard has it, and the wrist bends by at most 15 degrees.

|  | Before the shield work | Now |
| --- | --- | --- |
| Board turned in the fist, on average | 71° | 0° |
| Board against the way its bearer faces, in guard (seen from above) | 13° off | 25° off |
| Elbow far round from its lowest: in guard, just struck | 15%, 46% of the time | 0.1%, 3.5% |
| Blows blocked, in fights of six | 30% | 33% |
| Blows the shield target blocked (Mediums) | 17 of 54 | 23 of 50 |
| Large's shield inside its own trunk, just struck by a Large | 23% of the time | 0.2% |

The last row replaces what I wrote above about heavy blows knocking the board into its owner's trunk: with the arm held this way that has nearly gone. The board is still knocked into its owner's head 4 to 9% of the time just after a blow.

**Not yet seen in Unity.** The grip changes when a unit is built, so press Reset after pulling.

## Progress: step 3, arms that don't snap (13 November, overnight)

Built and checked headless only. Not yet seen in Unity.

**What was wrong.** The gate kept an arm inside a body's range, but our own stages still threw it about inside that range: a parry's hand rolled over in one step, a strike's wrist flicked, an elbow straightened from 80° in one step. Each reached the physical arm as a joint turned 60 to 170° in a sixtieth of a second, 60 to 70 times a fight. About half came from the parry, a third from the strike's own arm, most of the rest from the shield's upper arm.

**What changed.** Three switches on Combat Feel, under "Arms that bend as a body does":

- **Arm Follows At Most** (1500°/s; 0 turns it off). The gate follows the arm it is sent no faster than an arm moves. A snap becomes a fast move over three or four steps. A real swing (about 900°/s) is left alone.
- **Arm Bones As A Body** (on). After the gate, each arm's two bones are set the way a body's are: the elbow a hinge, no hidden roll of a bone. Nothing of this shows by itself; it stops hidden rolls reaching the physical joints, and it is what the physical elbow needs before it can have a hinge's stops (step 5).
- **Free Hand Goes With Its Arm** (on). The swordsman's tucked hand lies in line with its forearm instead of keeping a turn meant for an arm held in front.

**Measured** (fights of six, four seeds, against the same build with the three off):

|  | Before | Now |
| --- | --- | --- |
| An arm's joint turned over 60° in a step, as sent to the body (a fight) | 59 to 73 | 0, 0, 0, 1 |
| Physical joints put where the pose flipped them (a fight) | 38 to 48 | 0, 0, 0, 1 |
| The gate turning the swordsman's free arm back, in guard | 79 to 95% of the time | 20 to 32% |
| Blows blocked, parried, landed | 33%, 22%, 47% | 39%, 19%, 36% |
| Target test, Mediums: blocked, parried | 31 of 31, 21 of 27 | 29 of 30, 23 of 26 |
| Target test, Larges: blocked, parried | 19 of 23, 13 of 26 | 16 of 25, 12 of 27 |

**To look at in Unity.** Whether a parry or a shield now looks late. Whether a strike's hand looks soft where its wrist used to flick. If either does, raise Arm Follows At Most (or set it to 0 to compare).

**Not done in this step.** A knock still poses a struck arm far outside the range and leaves the gate to bring it back (a struck sword arm a third of the time). The tucked free arm is still moved by the gate a fifth of the time: the place it is tucked to sits at the edge of where an elbow goes.

## Progress: step 5, stops in the physical arm (13 November, overnight)

Built and checked headless only. This is a softer version of the step than the plan described, and the difference matters.

**What it does.** The physical arm is now read every step against the same ranges the gate holds the animation to. A joint knocked more than 2° past its range is at its stop: it is wanted back at the end of its range, it stops giving way, and it is held there with four times its muscle's whole strength, even when a blow has let the arm's muscles go. Inside its range a struck arm gives and comes back exactly as before.

**What it is not.** These stops are made from the joints' own motors. They are not the engine's joint limits, so a blow can still knock a joint past its range for a step or two before the stop has it back. A true hinge elbow in the engine needs the articulation probed in Unity for how it counts a limited joint, and I did not want to build that blind. The pose side is ready for it (step 3 made the elbow a hinge in the pose).

**Switches** on Combat Feel: **Arms Have Stops** (on), **Arm Stops Hold With** (4).

**Measured.**

|  | Stops off | Stops on |
| --- | --- | --- |
| Target test, Larges: time a physical arm is over 10° past its range | 4.6 s (73 times) | 1.5 s (27 times) |
| The arms just struck, over 10° past | 1.34% of the time | 0.50% |
| The arms in guard: the furthest past | 77° | 28° |
| Blocked, parried (target test, Larges) | 16 of 25, 12 of 27 | 15 of 25, 11 of 25 |
| Fights of six: the arms just struck, over 10° past | 2.28%, 1.27% | 0.74%, 0.77% |

The plan's check was "body past range: 0". It is not 0. What is left are spikes of one or two steps, and stops three times as strong did no better.

**To look at in Unity.** A struck arm should stop at the end of what an arm does instead of folding past it. If a struck arm now looks stiff or bounces at the end of its range, lower Arm Stops Hold With toward 1, or turn the stops off to compare.

**Not changed.** The shield board knocked into its bearer's own head or trunk by a blow is not an arm past its range, and is as it was.

## Progress: step 6, real weapon weight (13 November, overnight)

Built and measured headless. **It does not do what this plan hoped, and the heavy part is left off.**

**What is on.** Weapons and shields now weigh in the physical body what they weigh at their bearer's size: a Large's club 3.8 kg (it was a Medium's 2), its shield 7.7 kg (it was 4). For a Medium nothing changes. Switch: **Weapons Have Their Mass**.

**What I tried and left at 1.** A dial, **Weapon Mass**, makes a weapon that many times its real weight in the physics, with the arm as much stronger so it still swings as the animation does. The plan was that the weight of a blow would then come from the blade, and the script's multiplier (Weapon Weight, 3) could come down.

| Swords, Mediums on Mediums (target test) | Blows land at | Part struck goes, 0.1 s | Hips go, 0.5 s | Parry held |
| --- | --- | --- | --- | --- |
| As before | 11.8 m/s | 28 cm | 19 cm | 23 of 27 |
| Swords 3× their weight, arm as it was | 8.5 m/s | 22 cm | 11 cm | 21 of 27 |
| 3×, arm 3× as strong | 11.0 m/s | 25 cm | 19 cm | 14 of 25 |
| 3×, arm 3×, script's multiplier down to 1.7 | 6.9 m/s | 14 cm | 2 cm | 20 of 27 |
| 6×, script's multiplier down to 1 | 11.8 m/s | 13 cm | −1 cm | 19 of 26 |

**Why.** What throws a struck body is the script acting on the pose. The physical blade meets a physical upper body that strong muscles hold to that pose, standing on legs that are the pose's own. Its push is taken back within a step or two. A heavier blade therefore adds nothing to how far the body goes, and with the script turned down the blows are simply weaker.

**What this means for the plan.** The weight of a blow can only come from the weapon once the body it lands on is carried by its legs instead of placed by the pose. That is the legs plan, at its "supported hips" step. Until then, Weapon Weight stays what makes a blow heavy. I would drop this step from this plan and pick it up there.

**To look at in Unity.** The Larges: their clubs and shields are heavier in the physics now. Headless their blows land as they did. If their arms look slower or their shields sag, turn Weapons Have Their Mass off. To see the heavy weapon for yourself, set Weapon Mass to 3.

## Correction to step 3, found at the end of the night

The numbers in the step 3 note above were measured with **Arm Follows At Most** at 1500°/s. That setting cost hits, and I only saw it when I compared the whole night against its start.

At 1500 the limit also held back the strikes' own fast joints (an elbow straightening, a forearm rolling, a wrist snapping), in nearly every strike. Over four fights the share of attacks that hit anything fell from 77% to 68%. The default is now **2400°/s**, which lets the fastest recorded strike through.

| Four fights of six | Start of the night | At 1500 | At 2400 (as left) |
| --- | --- | --- | --- |
| Attacks that hit a body, shield or blade | 77% | 68% | 81% |
| Blocked, parried, landed | 33%, 22%, 47% | 31%, 21%, 39% | 34%, 22%, 45% |
| An arm's joint turned over 60° in a step (a fight) | 59 to 73 | 0 | 4 to 15 |
| Physical joints put where the pose flipped them (a fight) | 38 to 48 | 0 | 1 to 3 |

So the snaps are cut by about five sixths, not removed, and the fights hit and block as they did. The tables in the step 5 and step 6 notes were measured at 1500; their comparisons were each made at the same setting on both sides.
