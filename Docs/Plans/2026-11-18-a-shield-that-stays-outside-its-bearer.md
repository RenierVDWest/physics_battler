# A shield that stays outside its bearer

> Copy of the Claude document <https://claude.ai/artifact/9UZ1xrAueNhCM6Yn4wg2NN>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 18, 2026 · @Renier

## What Renier saw

18 November, 14:22, with four screenshots from Unity:

> "For the new get up animations the shield now goes through the floor. You at some point did a good update for your get up animations where you fixed this. Please do this for the new get up animations."
>
> "I still see a lot of shield intersections with the shield user's own body. Here he is slashing through his own shield. Shield going through the body. This happens all the time. Please do some research and try to come up with a plan that fixes this once and for all."
>
> "And while you're at it, try to make it so that there is a little dampening for physics objects that collide with the player bodies, but let's also try it for weapon on shields. Currently everything is very bouncy and shaky when they hit things. I want there to be some absorption of impacts so that it does not look so much like toys fighting."

The screenshots show: a swordsman's sword hand coming out through the face of his own shield in a thrust; a blade passing through the board of his own shield in a cut; and a fighter getting up with his shield through his hips and thigh, and his sword arm through the board.

Three jobs, then: the floor fix (built today, below), this plan for the shield and its bearer, and a first try at damping (built today behind a switch, below).

## Bottom line

**Three things were asked. One is mended, one is a plan with a counter behind it, one is a first trial.**

1. **The shield through the floor in the get-ups: mended and pushed.** The get-up's shield handling had not been running at all since 11 November: it was never told what the fighter holds. It is told now. Headless, the rim went from 32 to 36 cm under the floor for about a second, to never more than 2 cm. It leaves one thing: as he starts to rise he lies on his board, so the board is in his own hips for a few tenths of a second.
2. **Shield and sword through his own body: nothing fixed yet, on purpose.** You asked for research and a plan. I built a counter first. It says a shield bearer in a real fight shows a crossing about every three seconds, two thirds of them while striking. There are seven guards in the code already and each covers one situation; on his feet none knows the thighs, and none checks the sword against the shield at all. The plan is one rule in one place at the end of the pose, then attacks checked when they are built, then real collisions as a backstop, then the counter kept as a gate so it cannot come back unnoticed.
3. **Toy-like impacts: a first trial is in, behind a switch.** The bounce was not in the physics materials. It was in how struck bodies come back: on a spring built to swing past, and a blade built to bounce off a shield. The trial stops the swing back (3 degrees past upright became 0.1) and keeps the blade on the shield, without changing how far anything goes. A tremble of the hands after a hit is still there and I have not found its cause.

**What I need from you:** look at the get-up and the absorbed impacts in Unity (both have old/new toggles), and answer the open questions at the end, the first above all: when sword and shield want the same space, which one gives way?

Nothing here has been seen in Unity.

## Measured

All of this is headless. None of it has been seen in Unity. Single runs: they show that it happens and roughly where, not exact rates.

**How it is counted.** The body as it is seen (the physical one). His blade and his sword forearm are lines; his board is a thin disc; his body is its own collision shapes. A crossing is a line through the disc, or the disc's rim or face inside a body part. This is strict: the real meshes are thicker than lines, so what you see on screen is more than these numbers.

### How often, as the game was this morning

| Run | Watched | Blade through own shield | Sword arm through own shield | Shield in own body | Together | One crossing every |
| --- | --- | --- | --- | --- | --- | --- |
| Fight of six (2 shield bearers, 40 s) | 80 s | 1.5 s | 1.2 s | 1.3 s | 5% of the time | 2.8 s per bearer |
| The dance (2 fighters, 60 s) | 140 s | 0.5 s | 0.4 s | 0.9 s | 1.2% of the time | 8 s per fighter |

So "all the time" is right for a real fight: a shield bearer shows one about every three seconds.

### What he was doing (fight of six)

| Doing | Watched | Blade through shield | Sword arm through shield | Shield in body |
| --- | --- | --- | --- | --- |
| Striking | 24.4 s | 0.92 s (4%) | 1.07 s (4%) | 0.82 s (3%) |
| Moving | 19.5 s | 0.27 s | 0 | 0.05 s |
| Standing | 12.2 s | 0.22 s | 0 | 0 |
| Just struck | 5.5 s | 0.02 s | 0 | 0.17 s (3%) |
| Bashing | 5.3 s | 0.10 s | 0.08 s | 0.05 s |
| Stumbling | 0.7 s | 0 | 0 | 0.23 s (33%) |

**Striking is two thirds of it.** Stumbling is rare but bad: a third of that time has the shield in his body.

### Which attacks

| Attack | Time striking | Blade through shield | Sword arm through shield | Shield in body |
| --- | --- | --- | --- | --- |
| Lunging chop | 9.5 s | 0.65 s | 0.47 s | 0.03 s |
| Combo | 4.8 s | 0.17 s | 0.22 s | 0.63 s |
| Rising cut | 3.4 s | 0 | 0.27 s | 0 |
| Forehand | 1.6 s | 0.05 s | 0.12 s | 0 |
| Thrust | 2.3 s | 0.02 s | 0 | 0.02 s |

The lunging chop is the slash through his own shield. The rising cut puts the sword arm through the board. The combo puts the board in his body.

### Which part of his body the shield is in

Shield-side thigh 0.63 s, hips 0.33 s, chest 0.22 s, belly 0.18 s, head 0.10 s. **The thigh is first, and the thigh is in no guard.**

### Is it the pose, or the body being knocked off its pose?

| Crossing | Seen | The pose had it too |
| --- | --- | --- |
| Blade through shield | 1.53 s | 1.13 s (three quarters) |
| Sword arm through shield | 1.15 s | 1.03 s (nine tenths) |
| Shield in body | 1.32 s | 0.48 s (a third) |

Two different problems. Sword against shield is **asked for by the pose**: a rule on the pose fixes it. Shield in body is mostly **not** in the pose (the guard that exists works); the seen body was pushed there, and only a real collision stops that.

### Getting up: the floor (mended today)

The get-up has had its own shield care for a long time: board flat under the forearm while the hands push, held up to the threat, kept out of the ground. It reads the shield's size from the fighter's kit, and since the old pose pipeline was removed on 11 November the get-up was never handed the kit. So none of it ran. The recorded get-ups put the hands on the ground, which is why it showed now.

| Get-up of a shield bearer | Shield's rim under the floor, at worst | For how long |
| --- | --- | --- |
| From his back, before | 32 cm under | 0.85 s |
| From his back, now | 6 cm above | never |
| From his front, before | 36 cm under | 1.13 s |
| From his front, now | 2 cm under, as it starts | never more than 2 cm |

The same mend keeps the sword out of the floor (in the film of the old front get-up the blade dips under the ground once; in the new one it does not).

**What it leaves, honestly.** As a get-up begins he is lying on his board, so the board is in his hips for 0.05 to 0.4 s at floor level, and for about a tenth of a second around 1.1 s in the raised board passes his head. In two fights of six that came to 0.6 s of 4.7 s of getting up, and 0.4 s of 1.7 s. Before, there was none of this (and a shield through the floor instead). That is Step 4 of the plan. It is a trade I think is right for today (a board hidden under a lying body, against a board sunk a foot into the floor), but it is a crossing and you should know it is there.

## What already exists, and why each guard misses

There are seven guards in the code today. Each was added for one situation. None of them owns the whole problem.

| Guard | What it does | Where it runs | Why crossings still get through |
| --- | --- | --- | --- |
| Board kept off the body (`KeepShieldOff`) | Moves the posed board 6 cm clear of head, chest, belly and hips | Every step, on a fighter on his feet | It does not know the thighs (the part most often crossed). It only moves the hand: it cannot turn the board. It gives up after four tries (in a fight of six it still left the board in the body on 13 steps, up to 6.6 cm deep). It works on the pose, and the body seen is the physical one |
| Blade clear of himself (`BladeFromSelf`) | Stops a feint short of his own body | Only when a feint is made | Only feints. Not real attacks, not the shield |
| Sword arm clear of the trunk (`BodyClearance`) | The arm solver keeps the sword arm and blade out of his own head, chest, belly and hips | The parry and the built blows | Trunk only: no legs, and the shield's board is not a thing it knows. The recorded attack clips do not go through it at all |
| Arms stopped by the body | Real collisions between his arms and his trunk | Only while he falls and lies | Switched off again the moment he starts getting up or stands |
| Held things are solid | Shield and sword collide with his own body | Only while he falls and lies | Same: off on his feet |
| Shield kept before the chest | Fall pose keeps the shield arm in front | Falling | Falling only |
| The get-up's own shield care | Board flat under the forearm, held up to the threat, kept out of body and ground | Getting up | It was dead from 11 to 18 November: the get-up was never told what the hands hold. Mended today (see the measured section) |

**Nothing anywhere checks the sword against the shield.** The sword arm is posed by the attack. The shield arm is posed by the cover, the bash or a knock. The two are solved one after the other and neither looks at the other. That is the slash through his own shield.

### The five root causes

1. **No owner.** Two arms, posed by different systems, and nobody is responsible for the pair.
2. **Guards per situation.** Each guard sits inside one action. Every new action arrives unguarded: the bash, the fakes, the counter, the recorded get-ups and the new knock-backs all did.
3. **Missing parts.** The legs are in no guard, and the sword is in none.
4. **Recorded attacks were not recorded with this shield.** The man in the recording had no board of this size on his arm, so his sword arm passes where our board is (the lunge and the rising cut most of all).
5. **No physical backstop on his feet.** While he stands, his own shield, sword and body pass through each other freely. Whatever the pose guard misses, or a blow pushes in, nothing stops.

## How others solve it

Three families of answer turned up. They are not rivals: the good solutions use all three, in this order.

### 1. Keep the pose itself clear (a distance rule at the end)

Robots cannot afford to hit themselves, so robotics has the cleanest version. The body is wrapped in a few simple shapes (in one walking-robot controller: 4 balls and 14 capsules, which give 15 pairs worth checking). For each pair the rule is one line: **the distance between the two shapes may not go below zero.** That rule is checked last, after everything else has had its say, and when a wanted motion would break it the limb departs from the wanted motion just enough. It ran in about 0.2 ms a step in their tests (simulation only).

What we take from it: a small fixed list of pairs, one distance rule, applied in one place at the very end, for every motion alike. We already have the shapes (the body's own capsules) and one such rule (the board against the trunk). We lack the list of pairs and the single place.

### 2. Fix the motion before it is played (clips made for another body)

Animation research has the same problem whenever a recorded motion is put on a body it was not recorded on. One well-known method (Villegas and others, 2021) notes that plain retargeting leaves limbs passing through the body, and solves it by reading the target body's real shape and adjusting the motion so that touches stay touches and nothing passes through.

What we take from it: our recorded attacks were made by a man without our shield. The crossing is in the clip, so it can be found and repaired once, when the attack is built, instead of being patched every step in the fight. We already measure each attack once (where its wind-up goes, for the fakes); how long it crosses the board is one more measure.

### 3. Let real collisions be the backstop

The third answer is the physics engine's own: make the held things solid against the holder's body, so nothing can pass through whatever the animation asks. (I looked for an account of how the full-physics sword games do this and could not open one, so this rests on our own engine and on the discussion below, not on what those games say.) A games-programming discussion of the same problem lands on the practical point: joint limits alone do not stop a limb reaching the trunk, and the working answers are simple capsule checks on the pose, with collisions behind them.

What we take from it: we already do this, but only while a fighter falls and lies (his shield, sword and arms are solid against his own body then). On his feet it is switched off. The risk of switching it on is a fight between the pose (which wants an overlap) and the collision (which forbids it), and that fight shows as shaking. So it can only come **after** the pose is kept clear, when a collision is the exception and not the rule.

### What none of them offer

Nobody had a ready rule for **which of the two gives way**, sword or shield. That is a fighting question, not an engineering one, and it is one of the open questions below.

## The plan in steps

The idea in one line: **one rule, kept in one place, for everything he does, with a counter that says when it is broken.** Each step is behind its own switch with an old/new toggle in the lab, and each is judged on the counter built today.

### Step 0. The counter (built today)

It counts three crossings on the body as it is seen: his blade through his own board, his sword arm through it, and the board in his own body. It sorts them by what he was doing. It runs in the headless reports, and in Unity it writes each crossing into the playtest log (`cross`), so your own playtests are counted too. It changes nothing in the fight.

### Step 1. One rule in one place: what he holds keeps clear of him

A single last pass over the finished pose, every step, whatever he is doing: standing, striking, bashing, struck, stumbling, getting up. It replaces the board guard that exists and takes over its job.

- **A fixed list of pairs.** Board against head, chest, belly, hips and both thighs (the shins too while he is low). Board against his sword forearm and hand. Board against his blade.
- **Who gives way.** Against his body, the board always gives. Between sword and shield, the one that is working keeps its line and the other gives: while he strikes, the shield opens a lane for the sword; while he covers a blow or bashes, the sword waits outside the rim. (This is the first open question: it is your call.)
- **How it gives.** Not only by sliding the hand, as now: also by turning the board, which is what a real shield arm does to let a cut past. And eased in over time, so nothing pops.

What it should remove: the crossings the pose itself has. Measured today, that is about three quarters of the blade-through-shield time and nine tenths of the sword-arm-through-shield time.

### Step 2. Attacks checked when they are built

Step 1 corrects in the moment. A correction that arrives as the blade does looks like a flinch. So each attack, recorded or made, is measured once when it is built: how long does its sword arm or blade cross the board held in guard? Then the shield arm gets its own part for that attack: where the board goes so the lane is already open when the blade comes through, starting during the wind-up. It is worked out by running the Step 1 rule along the attack ahead of time and smoothing the answer. Nothing recorded is played as it is: this is the same "rebuilt from parts" rule the attacks follow now.

Worst first, by today's count: the lunging chop, the rising cut, and the combo.

### Step 3. Real collisions as the backstop on his feet

The board becomes solid against his own trunk and thighs while he stands, and the blade against the board: the same thing that already happens while he falls. It catches what no pose rule can: a blow that drives his shield arm back into his chest. Measured today, most of the shield-in-body time is of this kind (the pose was clear; the body seen was not).

It also serves the impacts plan: a shield driven into the chest then pushes the chest, so the force goes into the body instead of through it.

It must come after Step 1. If the pose still asks for an overlap, pose and collision fight each other and he shakes.

### Step 4. Getting up and lying

Today's mend takes the shield out of the floor. What it leaves is listed in the measured section: a board that lies under him as the get-up starts, and where a hand with a shield on its arm should post. These get their own step because a body on the ground has a third thing to keep clear of: the floor.

### Step 5. Lock it

The counter joins the regular check runs (the fight of six, the dance, the falls, the bashes) with a budget. Proposed: no single crossing longer than a tenth of a second, and under half a per cent of fighting time in all. Every new action has to pass before it is handed over. This step is what makes it "once and for all": every guard in the table above was added after something had already gone through, and the bash, the fakes and the recorded get-ups each arrived with no guard at all.

### What I am not promising

Zero. A shield arm that is hit hard goes somewhere, and for a few hundredths of a second something may touch. The promise is one owner, one rule, and a number that goes red when it is broken.

## Impacts that look like toys

You asked for "a little dampening" and "some absorption of impacts so that it does not look so much like toys fighting". I looked for what makes a hit bounce or shake before changing anything.

### What I found

| Suspect | Finding |
| --- | --- |
| Bouncy physics materials | **Not it.** Every material in the game already has zero bounce |
| How a struck trunk and head come back | **Yes.** They come back on a spring that is deliberately under-damped (0.55 of what stops a spring dead). It swings back past upright and returns. Measured on a medium bash: 27 degrees out, then 3 degrees past upright the other way |
| The striker's blade off a shield | **Yes, and on purpose.** The setting reads: the least the attacker's hand is thrown back off a block, "so even a light blow visibly bounces". It was built to bounce |
| How fast a knocked arm or leg comes back | **Partly.** As fast as it went out, like something on elastic |
| The physical body trembling round its pose | **Yes, and not solved.** After a hit the hands and head ripple about a centimetre round where the pose wants them, changing direction 5 to 11 times in the second after. Doubling and quadrupling the joints' damping changed nothing, so it is not a weak damper. I do not know the cause yet |
| Loose things hitting a body | A 5 kg tube thrown at 4 m/s stops dead on a trunk, glances off an arm, and comes back off a shield at 0.8 m/s. Not very bouncy; left alone |

So the toy look is mostly in **how things come back**, not in how far they go.

### What the trial changes

One switch, "Impacts: absorbed / as before (old)", and one lever, "Absorbed how much" (0 is as before, 1 the most; it starts at 0.6). It applies to every impact, a blade's or a bash's, on every fighter.

- **Trunk and head settle without swinging back.** They go exactly as far as before (the kick is made bigger to pay for the heavier damping, so the sizes you approved this morning stand), and then come to rest.
- **Trunk, head and legs come back slower than they went** (at 0.6: seven tenths of the speed). Out fast, back slow is what taking a hit looks like.
- **A blade stopped by a shield or a parry stays on it.** It keeps about half its spring back at 0.6, a quarter at 1, and backs out less.

### What I tried and took out again

I also made knocked **arms** come back slower. It made your second complaint worse: a shield knocked back onto his own sword stayed there. In a fight of six his own blade was through his own board for 4.2 s instead of 1.5 s. So arms come back as fast as before, and that part waits for Step 1 of the plan above. The two requests touch each other here.

### Measured

|  | Before | Absorbed (0.6) |
| --- | --- | --- |
| Medium bash: trunk rocks | 27 to 28 degrees | 27 to 28 degrees |
| Medium bash: then swings back past upright | 3 degrees | 0.1 degrees |
| Sword on shield in the dance: swing back, on average | 1.7 degrees (worst 2.7) | under 0.1 degrees |
| Sword on shield: striker's sword hand knocked off its pose | 19 cm | 14 cm |
| Hands' changes of direction in the second after | 5 to 9 | 5 to 9 (no change) |

The dance rows are from two different 45-second dances (7 blows on shields in each; the absorbed one was run before the kick was matched exactly, so its rocks were about a seventh smaller), so read them as a direction, not a rate. The last row is the honest one: the tremble is untouched.

### What comes next for this, if you like the direction

1. Find the tremble's cause. It is the part most likely to read as "shaky".
2. The slow arm, once the shield and sword keep clear of each other.
3. The rest is the impacts plan's Step 4 (knees give, hips drop, feet slide), which is the real absorbing and is still waiting on your word.

## Open questions

1. **Who gives way, sword or shield?** My proposal: the one that is working keeps its line. While he strikes, the shield swings open to let the sword through and comes back after. While he covers a blow or bashes, the sword waits outside the rim. The other choice is that the shield never leaves its guard and every attack is bent round it. The first looks more like a fighter; the second keeps him better covered while he attacks. Which do you want?
2. **May an attack be bent?** Step 2 changes the shield arm's part in each attack. If a lane cannot be opened by the shield alone (the rising cut may be such a case), may the sword arm's own path be bent a little too, or must the recorded cut stay as it is?
3. **How strict is the budget?** I proposed no crossing longer than a tenth of a second and under half a per cent of fighting time. Tighter costs more of the shield arm's freedom (it will stand further off his body).
4. **Start now?** Step 1 is the big one. Say the word and I start with it, behind a switch, judged on the counter.
5. **The absorbed impacts: right direction?** It is a first guess at one number (0.6). Flip it old and new in the lab and tell me whether it is nearer or further from what you want before I take it further.

## Sources

- [Humanoid Self-Collision Avoidance Using Whole-Body Control with Control Barrier Functions](https://alphaxiv.org/abs/2207.00692): the body as 4 balls and 14 capsules, 15 pairs, one distance rule per pair, about 0.2 ms a step; simulation only.
- [Contact-Aware Retargeting of Skinned Motion](https://openaccess.thecvf.com/content/ICCV2021/html/Villegas_Contact-Aware_Retargeting_of_Skinned_Motion_ICCV_2021_paper.html) (ICCV 2021): recorded motion put on another body leaves limbs passing through it; fixed by adjusting the motion against the target body's real shape.
- [Inverse kinematics which avoids self-collisions](https://rsn.gamedev.net/forums/topic/713024-inverse-kinematics-which-avoids-self-collisions/5450935) (GameDev.net): joint limits alone do not keep an arm out of the trunk; capsule checks on the pose are the practical answer.
- [Unity 6000.6 Physics settings reference](https://docs.unity.com/en-us/engine/6000.6/manual/unity-editor/editor-settings-reference/comp-manager-group/class-physics-manager.md): bounce threshold, depenetration speed and solver settings, checked while looking for what makes hits bounce.
- I tried to read Epic's piece on how Half Sword makes its combat and could not open it, so nothing here rests on it.
- Everything under "Measured" comes from this project's own headless runs of 18 November.

## Built since: 18 November, evening

After reading this, Renier asked for the collisions first (Step 3), for the sword as well as the shield, and that the contact be smooth and damped and not shaky. This is what was built. All of it is behind switches with old/new toggles in the lab, beside the bash settings. None of it has been seen in Unity.

### Where the shield's shaking came from

I measured it before changing anything. A "tremble" here is what is left of the board's movement once its own smooth motion is taken away.

- It was **not contacts**. With the physical bodies touching nothing at all, the tremble was the same.
- It was **not weak joint dampers** (tried earlier today).
- It was **the pose itself**. The pose moved a standing or walking fighter's board by 2 to 12 cm in a single step: when it was raised to a blow, when it was knocked, when he started walking from standing still, and when his trunk was rocked or turned at once. The physical arm then chased it.

### What was built

1. **What he holds moves smoothly.** The very last thing done to the pose, whatever came before it: the shield hand, and the sword hand while it is not striking, follow where the pose wants them, speeding up and settling in, instead of being put there each step. It is quicker while the shield has somewhere to be (raised to a blow, or in a bash). This is the "one place at the end" of Step 1, for smoothness.
2. **Shield and sword are solid against his own body on his feet.** The physics engine stops the board and the blade at his trunk, head, thighs and shins, as it did only while he fell. Something already inside him is not thrown out: it is stopped from when it is next clear. It is off while he gets up.
3. **The pose keeps the board out of his thighs too.** The thigh was the part it was in most, and no guard knew it.
4. **Met softly.** While the shield or sword is against his own body, that arm gives way at once and presses with a third of its strength, so it rests there and comes away with the pose.
5. **Less jolt on the shield arm.** The force a blow puts on the physical shield arm at once is reduced with "Impacts: absorbed". That force threw the hand off its pose and back within three steps, on the very thing you were watching.

### Measured (headless, old against new, one run each)

|  | Before | Now |
| --- | --- | --- |
| Fight of six: shield's board trembling more than 5 mm | 16% of the time | 6% |
| ... while walking | 9% | 4% |
| ... while standing | 10% | 2% |
| ... just after being struck | 23% | 12% |
| Fight of six: shield in his own body | 2.7 s of 80 | none of 51 |
| Fight of six: a blade in its own bearer's body (all six fighters) | 135 times, 9.3 s | 25 times, 1.2 s |
| Dance: board trembling more than 5 mm | 34% of the time | 25% |
| Dance: the board's usual tremble | 15.5 mm | 8.2 mm |
| Dance: shield in his own body | 1.8 s of 80 | 0.3 s |
| After a bash: hands change direction in the second after | 5 to 11 times | 3 to 7 times |

Blocks still work: in the new dance 6 blows met the shield and none landed.

### Your own playtest of 16:41 (the build before this one)

The counter built this afternoon was in that build, so your log counted for itself: 62 crossings, 7 seconds in all, over about 112 seconds of two sword-and-shield fighters. The shield in his own body 36 times (the shield-side thigh in 20 of those, and 14 of them just after he was struck), his blade through his own shield 16 times, his sword arm through it 10 times. The thigh is exactly what points 2 and 3 above are for.

### What is not better

- **The blade's tip trembles as much as before.** Most of it is the blows themselves. Standing, it is the way he faces being turned from step to step, which carries a tip a metre out.
- **His own blade and sword arm through his own shield are not stopped by anything yet.** No collision was asked between them, and Steps 1 and 2 of the plan (who gives way; attacks checked when built) are not built.
- **While the shield arm is meeting a blow it is held exactly to its pose**, and there nothing stops it. That is the 0.3 s left in the dance.
- In the tube test the shield bearer now ends sitting where he ended on his back, because the solid board takes the tube against his chest.

### Where to look

In the duel lab, with the bash settings (or type "body" or "holds" in the search box): "What he holds:", "His shield:", "His sword:", "Against his body:". Flip each old and new during a dance. Two lines there read live while a playtest is recorded: "His own crossings" and "What he holds trembles".

## Built since: 18 November, night. The shield gives way to the sword

Renier's answer to the first open question: "Proceed with the sword and shield where the shield gives way to the sword." This is Step 1's sword-against-shield half. It is behind a switch with an old/new toggle. Not seen in Unity.

### Why his sword went through his own shield

His two arms were each posed alone. The sword arm is posed by the attack. The shield is held before his chest right through his own blows, with its rim a hand's breadth past his breastbone on the sword's side. Nothing looked at the pair.

### The rule

In the same last place the pose is finished in:

1. Take his sword's two lines: his forearm, and his blade. Take them both where the pose has them and where they are actually seen (a blade stopped on the other's shield was up to 57 cm from where the pose had it, and it is the seen blade that goes through the seen board).
2. Look where each is going over the next 0.3 seconds, at the speed it is moving now. A sword at rest is not looked ahead for.
3. Wherever one of them passes through the board's face in that time, the board steps aside in its own plane: the least way, of sixteen, that leaves the sword a lane 6 cm clear of the rim. Across his own chest and up before his face cost more, so it prefers outward and down.
4. When the path is clear the board comes back over a quarter of a second.
5. The move is followed smoothly, like everything his shield hand does since this evening. His body's own guard and the collision with his body come after it and win.

### Measured (headless)

|  | Rule off | Rule on |
| --- | --- | --- |
| Aim test (19 blows), same build: blade through his own shield | 1.30 s | 0.25 s |
| Aim test: sword arm through his own shield | 1.00 s | 0.10 s |
| Aim test: crossings of a twentieth of a second or more | 13 | 5 |
| Second fight of six, same build: blade and sword arm through his own shield | 2.4 s | 0.2 s |

In the main fight of six, with both shield bearers on their feet for all 40 seconds: blade through his own shield 0.13 s, sword arm 0.07 s, shield in his own body 0.07 s. That is 0.3 s of 80, where this morning's game had 4.0 s. His blows land as before (in the aim test 14 met the other's shield, against 16).

### Your own playtest of 19:37 (this evening's build, before the shield gave way)

About 190 seconds of two sword-and-shield fighters. The shield in his own body: 7 times, and only 2 of them on his feet (at 16:41 it was 36 times). His blade through his own shield 18 times and his sword arm 15 times, 13 of those in his own blows: that is what tonight's rule is for.

### What it costs, and what is left

- **The board moves in his own blows now**, up to about half a metre aside at the furthest. For that moment it is not before his chest. Whether that reads as a shield opened for the cut or as a shield flung aside is the thing to look at.
- **His sword upper arm is not covered.** I tried it as a third line and took it out again: nothing measurably improved.
- **Falling, down and getting up are not covered.** The rule is for a fighter on his feet. That is Step 4.
- **A correction to this evening's note.** "Met softly" also made the arm give way at once while the shield was against his body. That turned out to hold the board against him when the pose drew it away (in one fight a board sat in its bearer's belly for 3 seconds). That part is taken out: the arm now only presses with a third of its strength.

### Where to look

Duel lab, with the bash settings (or type "sword" in the search box): "His shield and his sword: the shield gives way / pass through each other (old)", with the levers "Sword's lane" and "Sword seen ahead". Throw a rising cut and a forehand, and run a dance, with it on and off.
