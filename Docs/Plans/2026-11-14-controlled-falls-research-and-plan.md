# Controlled falls: research and plan

> Copy of the Claude document <https://claude.ai/artifact/L1KorNskCqjo8dT32DSguE>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 14, 2026 · @Renier

## The short version

A fighter that is knocked down should fight the fall all the way to the ground, the way a person does, and today it does not: it turns into a limp ragdoll in one step.

- **What people do.** They have about 0.7 s. They bend their legs to get low, throw their hands out with soft elbows, turn to land on something that can take it, and keep the head off the ground. Each of those cuts the impact by a fifth to a half.
- **How good games do it.** The body stays physical and its joints keep pulling toward a protective pose. Nobody who gets this right uses a limp ragdoll for the living.
- **What we build.** One body from the blow to the get-up. A small fall director picks the pose from the state of the body (which way it is going, how near the ground). Muscles are strong on the way down, give at the landing, and go slack only for the dead.
- **When.** A fall test with buttons and measures can be built now. The rest needs the hips to be let go, which is step 7 of the legs plan, so it follows your hit force and the staggers, as we agreed.
- **How you judge it.** Every step has a button that makes the fall happen, with an old/new toggle.

Five things are yours to decide; they are at the end, with my recommendation for each.

## What a fall is in the game today

A fall today is a ragdoll: the step the game decides a fighter goes down, every muscle is switched off and the body drops as dead weight.

| Stage | What the code does now |
| --- | --- |
| The decision | The legs decide. A blow that throws the body faster than 4.4 m/s (for a Medium) is a knock-down; slower is a stumble or a step. A leg swept, a body landing on it, or death also drop it. |
| Going down | There is no such stage. The physical body is taken away in one step and the older ragdoll parts take over, unheld, each given a starting speed along the blow (0.9 m/s for the body, 3 m/s for a swept leg). |
| On the ground | It lies until it is still: at least 1 s, at most 2.5 s. |
| Getting up | Built by code from key poses, one set for face up and one for face down. |

Three things are missing, and they are the whole of this plan:

1. **Nothing between standing and ragdoll.** A person fights the fall all the way down. Ours gives up in one step.
2. **No protective moves.** No hand goes out, the head is not kept off the ground, the body does not turn to land on something soft.
3. **Two bodies.** The fighter swaps from its physical body to the ragdoll at the worst moment, so whatever the blow was doing to it is thrown away.

The animation packs do not help here: of the 66 combat clips, 13 are hit reactions and none is a fall, a knock-down or a get-up. Whatever we build has to make the motion itself.

Your tube test this morning showed the starting point. At 600 N·s, physics only, the head was pushed up to 38 cm off its pose and the spine bent to its 20 degree limit, then everything sprang back. No hit of any size can fell a fighter by touching it, because the hips are carried by the animation.

## What real people do when they fall

A person knocked off their feet has about 0.7 seconds before the ground, and spends it getting low, getting their hands out and keeping their head up.

**The time they have.** From losing balance to hitting the ground takes about 0.7 to 0.75 s from standing. The first 0.2 to 0.3 s is passive: the body tips like a pole, and the only thing that happens is a startle, the arms jerking up and out within about 0.1 s. Deliberate protective movement starts near 0.3 s, which leaves about 0.4 s to do everything ([Le Goic 2018](https://www.frontiersin.org/article/10.3389/fneur.2018.00986/full)).

**They step first.** In the same study people stepped in 92% of trials, and a fall with no corrective step was the mark of someone who was going down. Young adults catch a sideways shove with one big cross-over step; older adults need several smaller ones ([Maki and McIlroy](https://scispace.com/pdf/control-of-rapid-limb-movements-for-balance-recovery-age-20q4l5sphs.pdf)). A fall is what is left when stepping has failed.

**What they do on the way down, and what it buys them:**

| What the body does | Fall | Measured effect |
| --- | --- | --- |
| Squats: bends knees and hips so the legs soak up the drop | Backward | Hip hits 18% slower, with 44% less energy ([Moon and Sosnoff 2017](https://www.gwern.net/doc/longevity/2016-moon.pdf)) |
| Lands on outstretched hands, and knees too when going forward or sideways | All | Keeps the head and the pelvis from being what hits first ([Sandler and Robinovitch 2001](https://core.ac.uk/download/357353563.pdf), on Hsiao's 1998 falls) |
| Keeps the elbows a little bent, not locked | Forward | 40% less force at the elbow, 26% less at shoulder and wrist ([Moon and Sosnoff 2017](https://www.gwern.net/doc/longevity/2016-moon.pdf)) |
| Turns to face the ground and lands on hands and knees | Sideways | 34% less energy at the hip ([Moon and Sosnoff 2017](https://www.gwern.net/doc/longevity/2016-moon.pdf)) |
| Rolls, as in judo: lands on the side and keeps turning | Sideways | 25% less force at the hip; 17% for beginners after 30 minutes of practice, who turn the trunk less ([Weerdesteyn 2008](https://pesquisa.bvsalud.org/bvsvs/resource/pt/mdl-17764975)) |
| Holds the head up with the neck muscles | Backward | Head arrives at 1.7 m/s instead of 3.2 m/s; with the hardest neck effort it does not touch at all ([2017 neck study](https://www.pubmed.gov/28843128/)) |

**What it looks like when it fails.** In 227 filmed falls of old people in care homes, the head hit something in 37% of them, even though the hands reached the ground in 74%. The arms got there but were too weak or too late to hold the body up ([Schonnop 2013, reported here](https://www.sciencedaily.com/releases/2013/10/131007122542.htm)). That is a useful picture for a fighter who is badly hurt.

**The five rules we take from this:**

1. Step first. Fall only when the steps have failed.
2. Get low before landing. Bent legs absorb the drop.
3. Land on many parts one after another (hands, knees, bottom, then the back or a shoulder), never on one part all at once.
4. Arms out early, elbows soft.
5. The head does not touch the ground. The neck works harder than anything else.

## How games and research have built it

Every version that looks good does the same thing: the body stays physical all the way down, and its joints keep pulling toward a pose that says "protect yourself". None of them drops to a limp ragdoll.

| Who | What they do | What we take |
| --- | --- | --- |
| [Euphoria](https://gta.wiki/w/Euphoria) (GTA IV, Red Dead, Max Payne 3) | Simulates the body, its muscles and a simple nervous system. Characters stumble and grab for support, reach out with their hands to break a fall, and cover their heads while rolling. | The list of behaviours. Also a warning about cost: GTA V kept it to about three characters at a time. |
| [Just Cause 2](https://api.sambuz.com/doc/physics-ics-meets-s-anim-imation-ion-charac-racter-ter-ppt-presentation-926923) | The ragdoll is pulled toward an animation pose by joint motors, so it still reacts to what it hits. Once it is slow enough, the game compares how it lies with a set of get-up starting poses, pulls it to the nearest and blends in. | Falls driven by a pose. Choosing the get-up by how the body actually lies. |
| [Jedi: Fallen Order](https://gamedeveloper.com/programming/leveraging-physical-animation-to-sell-force-powers-in-i-jedi-fallen-order-i-) | Joint motors follow the animation, and the hips are tied to the animation by a constraint. When the hips cannot reach where the animation wants them, the game lets go and the body falls. | This is our body already: carried hips, driven joints. Their "let go when the hips cannot be held" is how a fall should start for us. |
| [Zordan 2005](https://graphics.cs.ucr.edu/projects/dynResp/mocsim2.html) | On a hit the character becomes a simulation with working muscles, while a search finds the best moment of recorded motion to rejoin; the muscles steer it there. | A muscled passage between one animation and the next. |
| [Faloutsos, "virtual stuntman"](https://www.eecs.yorku.ca/~pfal/Petros_Faloutsos/Publications_files/sig03-complex-sketch.pdf) | Separate small controllers hand over to each other according to the state of the body. The falling one puts the hands down to cushion the landing. Speeds are matched at every handover. | Stages that start when the body is ready for them. No jump in speed between stages. |
| [Ha, Ye and Liu 2012](https://faculty.cc.gatech.edu/~sha9/projects/ha2012flm/index.html) | A landing controller with no recorded motion: shape the body in the air to control its turning, meet the ground, roll so the impact is spread over many parts, get up. | The roll, for fighters thrown hard. |
| [UKEMI robot, 2002](https://scispace.com/pdf/ukemi-falling-motion-control-to-minimize-damage-to-biped-4i3qpghsoe.pdf) | When the fall begins the knees bend fast to lower the body. It lands on a padded hip first, then the arms and lower back share the rest. It turns first if it is falling a bad way. The landing posture must be ready before the predicted landing. | The order the parts land in. Be in the landing pose early. |
| [A learned controller, 2025](https://scitepress.org/Papers/2025/132592/132592.pdf) | A network trained by trial and error found its own protective moves: a crouch with the hands down, a roll with an arm behind the head. But 22 of 25 viewers preferred recorded human falls; the learned ones looked twitchy and over-curled. | Not this road. It needs a training rig we do not have, and today it looks worse. |

Three lessons run through all of them:

1. **Motors on the joints toward a pose, not forces pushing the body around in the world.** This is what our physical body already does.
2. **Stages follow the state of the body, not a clock.** How far it has tipped and how near the ground it is decide what it does next. A clip that expects the ground at 0.6 s is wrong for a fighter thrown twice as hard.
3. **Where the pose comes from matters more than clever control.** Viewers prefer human-looking poses, so ours are authored the way a person falls, and physics supplies the timing and the mess.

## The design: what a fall will look like

A fall becomes five stages, and the fighter is doing something in every one of them.

&#91;embedded content: the five stages of a fall · what each part of the body does\]

Read it left to right: each column is a stage, each row a part of the body. The bar in the last row is how hard the muscles are working.

1. **Stagger.** The legs step to catch the blow. This exists today and gets better in the staggers work. It ends when the legs cannot catch it, by the rule the game already uses.
2. **Losing it.** About the first quarter of a second. The hips are let go and the body tips where the blow sends it. The arms jerk out, and nothing clever happens yet, as in people. This is where the force of the blow shows.
3. **Going down.** The body takes the protective pose for the way it is falling: knees fold, the head is pulled away from the ground, the hands go to where the ground will be. It ends when the first part is about to touch.
4. **Landing.** Parts land one after another. Each part that lands is held where it landed with a limited strength, as a planted foot is now, and elbows and knees give under the weight. The muscles ease off as the speed is spent.
5. **On the ground.** The body settles into a pose a person would end in, lying or sitting, and gets up from that pose.

**By the way it falls:**

| Falling | Legs | Trunk | Head | Arms | Lands on, in order |
| --- | --- | --- | --- | --- | --- |
| Backward | Knees and hips fold, as if sitting down | Curls forward, back rounded | Chin to chest | Swing back and out to the sides, elbows soft | Bottom, hands, then a roll onto the rounded back |
| Forward | One last step, then the knees bend | Stays long, chest held up | Lifted and turned to one side | Both out in front at shoulder width, elbows soft | Knees if it is slow, hands, forearms, chest |
| Sideways | The near leg folds | Turns to face the ground, or keeps turning into a roll | Tilted away from the ground | The lower arm reaches for the ground, the other follows across | Side of the thigh, hand and forearm, hip, shoulder |
| Spinning | As whichever of the three it is turned toward when it nears the ground |  |  |  |  |

**By how hard it was hit:**

| The blow | What happens | Time down |
| --- | --- | --- |
| Just more than the legs can catch | **A caught fall** (new): down to a hand and a knee, or a sit, and straight back up | Under a second |
| Clearly more | The full fall above; it lies, then gets up | As now, 1 to 2.5 s |
| Thrown (a Large's club) | In the air it tucks: arms in, chin down. It lands and tumbles, and physics leads while it holds the tuck | Longer |
| Killed | The startle, then every muscle fades to nothing in about 0.3 s. No bracing | Stays |
| A leg cut off | The full fall with bracing | Stays |

The dead falling differently from the living is on purpose: you can read who is finished from how they go down.

**Weapons and size.** A fighter keeps hold of what it carries. The sword hand lands on its knuckles and forearm, and the shield arm lands on the shield. A Large falls more slowly than a Medium because it is taller; physics gives that for free.

The times and limits in this section are starting guesses. They get tuned in the fall test.

## How it fits our fighter

The fall is built from parts the fighter already has. One new piece, a fall director, decides the pose, and the physical body does the rest.

**What changes:**

| Today | With this plan |
| --- | --- |
| The hips are carried by the animation and never let go | The hips are held with a limited strength (step 7 of the legs plan). In a fall that strength goes to zero and the body is free |
| On a fall the physical body is thrown away and the old ragdoll takes over | One body from the blow to the get-up. The old ragdoll stays behind a switch as the fallback, and for limbs that are cut off |
| Every joint is driven back to its pose as hard as it is allowed | Muscle strength is a dial, by body region and by stage: strong going down, giving at the landing, easing on the ground, nothing for the dead |
| The pose always comes from a clip | During a fall the pose comes from the fall director |

**The fall director.** Each step it reads the state of the body: which way it is tipping compared with the way it faces, how fast, how high the hips and the head are, how soon the ground comes. From that it picks the stage, blends a handful of authored poses (about ten: tucked, sitting, reaching forward, reaching sideways, rolled, lying), aims the hands at the ground, and sets the muscle dial. It never moves the body itself. Physics does that.

**What we reuse:**

- The physical body with its legs (step 6).
- The soft pin that holds a planted foot with a limited force. A hand or knee that lands gets the same pin.
- The way the get-up is built from key poses. The fall's poses are authored the same way.
- The arm solver that puts a hand on a point.
- The tube test, to make any fall happen on demand.

**Why not fall clips.** The packs have none, and a clip runs on a clock while a fall must follow the body. But the director takes its poses from anywhere, so a bought pack of falls can be plugged in later as a source of poses if the authored ones look stiff.

**What stays animation.** The stagger's steps, the hit reactions and the get-up.

**How I check it without seeing it.** I cannot watch a fall, so each one is measured, headless and in your logs:

- Did the head touch the ground, and how fast. Aim: fewer than 1 in 10 for the living.
- Did a hand or a knee land before the trunk. This one turned out to be a weak measure: a limp ragdoll's dangling hand often touches first by accident. It is kept, but the head and the hips are what count.
- How fast the hips landed. Aim: clearly slower than today.
- How long from the blow to lying still, and how far it slid.
- How far the limbs are from the pose asked for, which tells limp from held.

Today's numbers, from the first run of the fall test (12 falls, three each backward, forward, left and right, headless): the head reached the ground in 12 of 12, at 3.7 m/s on average and 7.0 at most. The hips landed at 2.3 m/s on average. The body was on the ground about 0.45 s after the blow, where a person takes about 0.7 s, because the legs give nothing. These measures say whether the fighter protects itself. Whether it looks good is yours to judge, and every step has a button for that.

## The steps, in order

Eleven small steps. The first can be done now; the rest wait for the hips to be let go, which is step 7 of the legs plan.

**Where this sits in the order we agreed:** the fall test (step 1 below) now, then supported hips with your hit force, then staggers on real legs, then steps 2 to 11.

| Step | What it is | What you will see | Combat Feel switch |
| --- | --- | --- | --- |
| 1 | **The fall test and today's numbers.** The tube test gets fall buttons: back, front, left, right, thrown. Every fall is measured | Nothing changes in how they fall. You get falls on demand, and we get the numbers to beat | none |
| 2 | **One body through the fall.** The physical body is kept, the hips are let go, the joints go slack | About what you see today, but with no swap of bodies. It proves the free body is stable | `fallsKeepTheBody` |
| 3 | **Muscle.** Joints hold a braced pose with limited strength going down and ease off on the ground. The dead go limp | The first real gain: limbs stop flailing, and the dead and the living fall differently | `fallsHaveMuscle` |
| 4 | **The head.** The neck pulls the head away from the ground, whichever way it falls | Heads stop bouncing off the floor | `fallsProtectTheHead` |
| 5 | **Backward.** Sit down, round the back, arms back; a hand that lands stays put | The commonest fall looks like a person sitting down hard and rocking back | `fallsBackwardBraced` |
| 6 | **Forward.** Hands out, elbows give, knees first when slow | Caught on the hands, chest lowered | `fallsForwardBraced` |
| 7 | **Sideways and spinning.** Turn to face the ground, or roll | No more falling flat on a hip | `fallsSidewaysBraced` |
| 8 | **The caught fall.** A hand and a knee, then straight back up | A new thing between a stagger and a fall | `fallsCanBeCaught` |
| 9 | **Thrown.** Tuck in the air, tumble on landing | A Large's club sends a body that still looks alive | `thrownBodiesTuck` |
| 10 | **On the ground.** Settling poses; get-ups from the side and from sitting, chosen by how the body lies | Getting up starts from where it really ended | `getUpFromHowItLies` |
| 11 | **Weapons, shields, sizes, cost.** Landing on the shield, the sword hand; a tuning pass; a check with many units | Polish | as needed |

Every step is done the same way:

- It sits behind its switch, with a tooltip, on the Combat Feel object.
- The fall buttons get an old/new toggle for it, so you can hit the same fall both ways.
- I report the measures before and after, and say plainly what I could not check.
- It ends with a README entry and a note in the plan.

Steps 3 and 4 give the most for the least work, so they come before any one direction is finished. Backward comes first of the directions because a fighter is mostly struck from the front.

## Risks, and what is yours to decide

The biggest risk is the free body: a fighter with driven joints and nothing holding its hips has never been run in this project.

| Risk | What I do about it |
| --- | --- |
| The free body is unstable in the engine | Step 2 exists to find this out early. The old ragdoll stays behind its switch as the way back |
| Joints are driven like a wall, and a braced arm must give | Joints get a real strength limit, tuned with the tubes. This morning at 600 N·s the spine bent to its stop and sprang straight back |
| A fighter's own parts do not collide with each other, so a reaching arm can pass through its body | The poses are authored to keep the arms clear |
| Authored poses look stiff | More in-between poses first; a bought pack of falls as the pose source if that is not enough |
| Many fallen bodies cost too much | Measured from step 2 on. Ours is far simpler than Euphoria, which GTA V held to about three characters |
| I cannot see it | The measures say whether it protects itself. Your eye says whether it looks right, at every step |
| The fight's balance shifts | A caught fall is shorter than a fall. Checked with the many-seeds test before it is on by default |

**Yours to decide.** None of these holds up step 1.

- [ ] **Where the poses come from.** I author them in code, as the get-up is (my recommendation: no cost, and I can start), or you buy a falls and knock-downs pack to use as the source.
- [ ] **Weapons.** Fighters keep hold of sword and shield when they fall (my recommendation), or drop them on a hard fall.
- [ ] **The dead.** They fall limp while the living brace (my recommendation), or everyone braces.
- [ ] **The caught fall.** A new outcome between a stagger and a fall (my recommendation: yes, behind its switch).
- [ ] **How often they fall.** I leave today's limit alone until the hit force and the staggers are in, then we set it with the tubes.

## Sources

Pages I opened and took facts from. Where I could only read a summary of a paper and not the paper itself, the list says so.

**How people fall**

- [Moon and Sosnoff 2017, Safe landing strategies during a fall](https://www.gwern.net/doc/longevity/2016-moon.pdf): a review of 13 studies; the source of most percentages here.
- [Le Goic and others 2018, An initial passive phase that limits the time to recover](https://www.frontiersin.org/article/10.3389/fneur.2018.00986/full): the timing of a fall and of the first reactions.
- [Sandler and Robinovitch 2001, lower extremity strength and impact severity in a backward fall](https://core.ac.uk/download/357353563.pdf): also my source for Hsiao and Robinovitch's 1998 study of unexpected falls, which I could not open directly.
- [Weerdesteyn and others 2008, martial arts fall techniques](https://pesquisa.bvsalud.org/bvsvs/resource/pt/mdl-17764975): abstract only.
- [Neck muscles and head impact in backward falls, 2017](https://www.pubmed.gov/28843128/): abstract only.
- [Head impact in filmed falls in care homes (Schonnop 2013), as reported by ScienceDaily](https://www.sciencedaily.com/releases/2013/10/131007122542.htm): a news report of the paper.
- [Maki and McIlroy, Control of rapid limb movements for balance recovery](https://scispace.com/pdf/control-of-rapid-limb-movements-for-balance-recovery-age-20q4l5sphs.pdf): stepping and grasping to stay up.

**How it has been built**

- [Euphoria, GTA wiki](https://gta.wiki/w/Euphoria) and [Euphoria (software), Wikipedia](<https://en.wikipedia.org/wiki/Euphoria_(software)>): behaviours and games; neither explains the internals.
- [Physics Meets Animation: Character Stunts in Just Cause 2](https://api.sambuz.com/doc/physics-ics-meets-s-anim-imation-ion-charac-racter-ter-ppt-presentation-926923): slides of the talk.
- [Physical animation in Jedi: Fallen Order, Game Developer](https://gamedeveloper.com/programming/leveraging-physical-animation-to-sell-force-powers-in-i-jedi-fallen-order-i-): a write-up of the talk.
- [Zordan and others 2005, Dynamic response for motion capture animation](https://graphics.cs.ucr.edu/projects/dynResp/mocsim2.html): project page.
- [Faloutsos, the virtual stuntman](https://www.eecs.yorku.ca/~pfal/Petros_Faloutsos/Publications_files/sig03-complex-sketch.pdf): a short sketch, not the full paper.
- [Ha, Ye and Liu 2012, Falling and landing motion control](https://faculty.cc.gatech.edu/~sha9/projects/ha2012flm/index.html): project page.
- [Fujiwara and others 2002, UKEMI: falling motion control for a humanoid robot](https://scispace.com/pdf/ukemi-falling-motion-control-to-minimize-damage-to-biped-4i3qpghsoe.pdf).
- [Beyond the Ragdoll: a safe-falling controller, 2025](https://scitepress.org/Papers/2025/132592/132592.pdf).

What I looked for and did not find: a technical account of how Euphoria's fall behaviours work inside. The public pages only list what they do.

## Progress

**14 November: step 1 is built.** The tube test's panel has Back, Forward, Left, Right and Thrown back; each sends the targets down that way. Every fall, in the test or in a fight, writes a `fall` line to the playtest log. Checked headless only; the buttons and the log lines are not yet seen in Unity.

Today's fall over 16 measured falls: the head reached the ground in all 16, at 4.1 m/s on average. One more thing came out of it: the fall forgets the blow. A fighter thrown at 13.9 m/s and one thrown at 5.1 m/s go down the same way, on the spot, because every fall starts at the same 1.4 m/s. That belongs with the hit force work.

**14 November: decided, and step 1 seen in Unity.** All five decisions went as recommended: the poses are authored in code; fighters keep hold of what they carry; the dead fall limp while the living brace; the caught fall is built; how often they fall is left alone until the hit force and the staggers are in. In Unity the fall buttons worked: 36 falls, all four ways, and the head reached the ground in all 36.

**14 November: the get-up has its own plan and is rebuilt.** Step 10 of this plan (getting up from how the body lies) grew into [Getting up: research and plan](https://claude.ai/artifact/TQNgYhYhCCX9sWzAMpcqnh) and is built ahead of the other steps, since it does not need the hips let go.

**14 November: steps 2, 3 and 4 are built (one body, muscle, the head).** The hips can be let go now (step 7 of the legs plan is in), so a fall keeps the one physical body. It is let go, gets its weight, lands on the ground, and its joints are pulled toward a pose with about a person's strength. The pose is made each step from how the body is: it gets low, throws the arms out and then reaches for the ground, and keeps the head away from the ground. Planted feet stay put until the body has gone over. The dead get a start and then go limp. Getting up takes the same body up where it lies.

Headless, over 21 living falls, the head reached the ground in 9 (it was 16 of 16). No body was lost by the engine in the tests or in two fights of a minute. Not yet seen in Unity.

What is not right yet: a 200 N·s blow throws the free body flat in a quarter of a second, and the hips land at 2 to 5 m/s; the arms are sometimes half a turn from where the pose wants them; the legs fold by one rule for every fall. Steps 5 to 9 (each direction, hands held where they land, the caught fall, the thrown body) are where those get fixed.

**14 November: seen in Unity, two faults fixed, and step 8 (the caught fall) built.** The falls in the one body held in Unity over 55 falls and were liked. The log showed two things. Every get-up carried the body to its feet for one step and back; fixed. And the head still reached the ground in 51 of 55.

The cause of the second was the hit force. A blow that fells was putting its whole extra force into a body that had just been let go, which threw it flat in a fifth of a second. A felling blow now keeps 0.3 of that force. The legs now fold as the hips come down instead of on a clock. Headless, the fall test's 12 falls: the head reaches the ground in 2 (it was 6 with the first version, 12 as a ragdoll).

The caught fall is in: a fighter thrown only a little faster than its legs can catch goes down on legs that hold, and rises from a knee or from sitting in about a second. It happened once in two fights of a minute. Not yet seen in Unity.

Still to do from this plan: a hand that lands held where it landed (steps 5 to 7), the thrown body's tuck (step 9), weapons and sizes (step 11).

## Progress, 14 November (afternoon): hands stay put, bodies settle (steps 5 to 7)

Built and checked headless; not yet seen in Unity.

- **A hand that lands stays where it landed.** It is held there with about a palm's grip (0.35 of the body's weight a hand), and the arm bends under the body as it comes down. Before, hands skated 2 to 4.5 m over the ground in one fall.
- **A body that is down settles.** A quarter of a second after its hips are down, its muscles stop reaching and only hold the pose it lies in; the head is still held up. Before, a body on its side rocked from front to back for as long as it was down.
- **Time on the ground is kept.** A settled body is still at once, so it would be up after exactly one second every time. It now stays down 1.4 s give or take 0.3, which is what the average was.

| The fall buttons (200 N·s) | Backward | Forward | To its left | To its right |
| --- | --- | --- | --- | --- |
| Hands' way over the ground | 227 cm, now 23 | 218, now 22 | 452, now 16 | 216, now 15 |
| Hips' way over the ground | 64 cm, now 26 | 9, now 0 | 109, now 20 | 39, now 5 |

Falling forward, a fighter now ends on hands and knees. Switches on Combat Feel: "Falls Hands Stay Planted", "Fall Hand Hold", "Fallen Bodies Settle"; all three are in the panel's "Falls and getting up" section.

Not done of steps 5 to 7: knees are not pinned, and a body falling sideways does not turn to face the ground. Still to do after that: the thrown body's tuck (step 9), weapons and sizes (step 11).

## Progress, 14 November (late afternoon): the thrown body tucks (step 9)

Built and checked headless; not yet seen in Unity.

A fighter thrown at twice what its legs can catch (the open-handed target under "Thrown back" and "Club"; in fights, a Large's club at its hardest) is now thrown, not knocked over:

- **The whole body goes, and a little upward.** Before, the legs were left behind and the body was slammed flat in a quarter of a second.
- **It tucks:** back rounded, chin held down, knees half drawn up, arms in before the chest. An arm holding a weapon stays out to the side.
- **It tumbles in the tuck**, then opens out about half a second after it first touches the ground, and its hands stop it. It stays down half a second longer than a fighter who was only knocked over.

| Open-handed target, thrown from front and back, 450 and 600 N·s | Before | Tucked |
| --- | --- | --- |
| Head came down at | 0.2, 5.7, 5.3, 1.7 m/s | 1.8, never, 2.2, never |
| Distance over the ground once down | 0.2 to 0.8 m | 0.1 to 3.8 m |
| Time down | 1.1 to 1.7 s | 1.8 to 3.5 s |

Thrown backward he turns right over backward. Whether that reads as a tumble or a stunt needs an eye. In one headless fight a swordsman thrown extremely hard (24.8 m/s) still came down on his head at 4.3 m/s.

Switch: Combat Feel "Thrown Bodies Tuck", and "A thrown body tucks" in the panel. Still to do: weapons, shields and sizes in falls, and a cost check with many fighters (step 11).

## Progress, 14 November (evening): weapons, shields and sizes in falls, and the cost (step 11)

Built and checked headless; not yet seen in Unity.

**Measured first.** Each fall's log line now says how long an arm, the weapon and the shield were inside the fighter's own body, how low the weapon and the shield came against the ground, how fast the weapon's end went, and whether the fall was a ragdoll's.

**What it showed, and what changed:**

- **Weapons in an ordinary fall were fine.** The ground stops them and the fall's pose keeps the blade out to the side. Nothing changed.
- **Arms lay inside the body** (two seconds after a tucked throw). Now, while a fighter is down, its forearms, hands and shield are stopped by its own trunk, and the fall's pose keeps the hands beside or in front of the trunk, never through it.
- **A shield caught on the ground** and held the body up, which then rolled off it sideways. Now the shield is kept in front of the chest: going forward the fighter lands on it, going backward it lies over him.
- **A fighter with a limb cut off falls as a ragdoll** (half the falls in a test fight, and the dead of them stay on the field). Its sword hung through the ground and its arm lay in its body. Now what a ragdoll holds is stopped by the ground, and its arms by its body.

|  | Before | Now |
| --- | --- | --- |
| Thrown and tucked: an arm in its own body | 1.95 s | 0.07 s |
| Shield arm away from its pose, falling forward (Medium, Large) | 43, 117 cm | 6, 6 cm |
| A Large with a shield knocked forward ends | on its back | on its front, on its shield |
| Ragdoll: sword's end against the ground | 84 cm under it | 1 cm over it |
| Ragdoll: an arm in its own body, of 3 s | 2.4 to 3.0 s | 0 s |

**Sizes.** A Large falls by the same rules as a Medium, slower. Knocked forward it stays on its hands and knees until it gets up, where a Medium's trunk comes down.

**Cost.** A fall costs nothing over standing (from the Unity logs, three fighters: 4.4 ms a physics step standing, 4.2 falling, 4.7 getting up). What costs is the fight itself: about 1.3 ms a fighter a step in the editor, so six take 8 ms of the 16.7 a step has, and twelve would take all of it.

**Found on the way, not done:**

- Getting up from its front, a kneeling knee is 2 to 12 cm under the ground for up to 0.65 s.
- Thrown to its right at 600 N·s (not tucked), a fighter's head comes down on the ground at 2.5 to 3 m/s.
- With its shield before it, a Medium knocked backward rolls further onto its shoulders (it has one hand to brake with, not two).

Switches (Combat Feel, and ticks in the panel): "Falls Arms Stay Out Of The Body", "Falls Shield Kept Before The Chest", "Ragdolls Hold Solid Things". Next in the agreed order: staggers on real legs.
