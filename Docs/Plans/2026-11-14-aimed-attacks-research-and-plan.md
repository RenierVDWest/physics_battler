# Aimed attacks: research and plan

> Copy of the Claude document <https://claude.ai/artifact/GpqpU4z5KUCuK9M3EBrEBY>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 14, 2026 · @Renier

## The short version

Today a fighter picks a recorded swing and bends it toward the enemy's middle. The new attack is built in code for one body part and one moment.

- **Today:** nine borrowed sword clips. Each can only be aimed at the hips, the chest or the face, and lands 0.5 to 1.4 s after it starts.
- **In the battle you watched:** of 82 attacks, 36 touched nothing. Of the 53 that touched something, 36 met a shield or a sword. The 17 that reached a body hit whatever was in the way.
- **New:** the fight asks for a body part and a landing time. The attack picks a cutting line that is open, a size of cut that fits the time, and a step that reaches. The blade is drawn along that line and the body is built behind it.
- **Built like the get-ups and the sword parry:** in code, on the body you have now. The physical body, the legs and the hits stay as they are.
- **Two tests, in your order:** every body part on a standing target, tinted red when hit; then the shield that opens for 400 ms.
- **When:** I would do it next, before staggers. The reasons are under "When to build it".

Nothing here is built yet. This is the research and the plan.

## What an attack is in the game today

An attack today is a recorded swing, played as it was recorded, with three small corrections laid over it.

| Step | What the code does | What it means |
| --- | --- | --- |
| 1 | Picks, by chance, one of the clips that can land from where the fighter stands | The swing is chosen first. The target follows from it |
| 2 | Aims at the hips, the middle of the chest or the face, whichever is nearest the height that clip swings at | Three targets. Never an arm, a leg, the neck or a shoulder |
| 3 | Turns the body up to 60 degrees, stretches the lunge 0.4 to 1.4 times, pulls the hand up to 40 cm toward the aim point | Small corrections on a fixed motion |
| 4 | Plays the clip at its own speed | It lands when the clip lands |

The nine sword clips, measured from their own tracks. The Large swings the same nine with its club.

| Clip | Lands after | Of that, the cut itself |
| --- | --- | --- |
| Thrust over the shield | 0.47 s | 0.38 s |
| Forehand cut | 0.63 s | 0.13 s |
| Forehand from the four-cut combo | 0.67 s | 0.12 s |
| Stepping backhand | 0.77 s | 0.08 s |
| Rising backhand | 0.83 s | 0.30 s |
| Leaping spin chop | 1.10 s | 0.32 s |
| Overhead chop with a lunge | 1.17 s | 0.21 s |
| Overhead chop with a side step | 1.40 s | 0.21 s |
| Short backhand (a combo link only) | 0.17 s | 0.13 s |

So the blade's own cut is quick, 0.1 to 0.3 s. Most of each clip is the wind-up, 0.5 to 1.2 s, and it cannot be skipped or held.

**What works and stays.** A hit is what the physical blade touches. The damage rules, the hit stop, the blade that rebounds off a shield, and the defender's reading of the blade's path all stay.

**What the game had before the clips.** The first build had blows made in code and aimed at body parts (milestone 2, `SwingPath` and `MovePlanner`). The combat overhaul replaced them with the clips. That code is still in the project. Its swing was one arc about one point in the chest, with the body posed by fixed curves on legs that were not physical. The plan below takes its good idea, planning a blow so that it lands, and not its swing.

## What goes wrong in the battle

In the battle you watched this evening, fewer than one attack in four reached a body, and none of those was a chosen body part.

From your log of 19:19 (six fighters, the last 55 s of the session):

| What happened to the 82 attacks | Count |
| --- | --- |
| Touched nothing at all | 36 |
| Met a shield | 22 hits |
| Met a sword that parried | 14 hits |
| Reached a body | 17 hits |

(46 attacks made the 53 hits; a combo can hit more than once.)

Where the 17 body hits landed: right hand 4, thighs 4, chest 3, head 2, hips 2, a shin 1, a forearm 1. Every one of those blows was aimed at the hips, the chest or the face. Ten of the 17 landed on something else that was in the way.

From the swing starting to the first touch took 0.78 s at the middle, and up to 1.28 s.

Four faults follow from how an attack is made today:

1. **No real target.** The blade goes toward the middle of the body. What it meets on the way is chance.
2. **No timing.** A blow lands when its clip lands, half a second to a second and a half after it starts. An opening of 0.4 s is over before the fastest clip arrives.
3. **No opening.** The clip is picked by chance from those that reach. Nothing asks whether the line is covered, so of the blows that touch anything, two in three end on a shield or a sword.
4. **One size.** Every clip is a whole-body swing with a long wind-up. There is no small quick cut for a short chance.

These are what the numbers show. What your eye sees as wrong in the motion itself, I cannot measure, and I would like to hear it.

## What a sword attack is made of

A cut is the sword turning about a point in the body: the hand starts it, the hips drive it, the wrist finishes it, and the foot lands last.

Seven parts make a cut. Each is something the code can set.

| Part | What the sources say |
| --- | --- |
| **The line** | A cut travels in a flat plane through the target. Meyer draws eight lines through one point: straight down, two diagonals down, two level, two diagonals up, and each but the first from either side ([Meyer's system](https://science-and-fiction.org/swordfighting/meyer_dusack.html)). The sabre manuals number the same six diagonals and levels ([the six cuts](https://swordfight.uk/the-six-cuts/)). The thrust is the ninth: straight in |
| **The pivot** | "The main motion during a cut is that the sword rotates around a pivot point", inside the body for a full cut ([cutting mechanics](https://science-and-fiction.org/swordfighting/cuts.html)). A cut from the wrist is quicker and weaker than one from the shoulder ([ARMA](https://thearma.org/essays/howacutworks.htm)); the sabre manuals want cuts "as much as possible from the wrist" so the elbow does not bend and shorten the reach ([the six cuts](https://swordfight.uk/the-six-cuts/)) |
| **The order of the body** | First the body moves and the blade hardly at all. Then the blade speeds up over 30 to 40 degrees, runs at a steady speed for the last 50 ms into the target, and slows after it ([cutting mechanics](https://science-and-fiction.org/swordfighting/cuts.html)). In a fencer's lunge the joints reach their top speed one after the other, each later than the last ([lunge study](https://ojs.ub.uni-konstanz.de/cpa/article/view/6606/5967)) |
| **Hand before foot** | The sword goes first and the foot follows. Waite: "never let the foot touch the ground before the sword reaches its destination". Del Frate: a lunge after the blow is wasted, a lunge before it exposes the arm ([step and cut](https://www.bookandsword.com/2023/03/04/fencing-problems-coordinating-a-step-and-a-cut/)) |
| **The edge and the striking point** | The edge has to lie in the plane the blade travels in, and the blow lands "with a good portion of the edge", not with the point thrown forward ([ARMA](https://thearma.org/essays/howacutworks.htm)). A cut is aimed through the target, to stop a little past it, not on it ([the six cuts](https://swordfight.uk/the-six-cuts/)) |
| **The end** | "In all cuts and techniques you will always move from one guard to the other" ([Meyer's system](https://science-and-fiction.org/swordfighting/meyer_dusack.html)). A full cut ends in the guard on the far side. A half cut stops with the point forward |
| **The way back** | Head first, then the body, then the foot, the same order as going in, reversed ([step and cut](https://www.bookandsword.com/2023/03/04/fencing-problems-coordinating-a-step-and-a-cut/)). With a shield, "shield goes up and covers the retreat" ([sword and shield](https://science-and-fiction.org/swordfighting/sword_and_shield.html)) |

The numbers I will start from:

| Measure | Value | From |
| --- | --- | --- |
| A quick cut, no step | 0.25 to 0.35 s. It cannot be made in time with a step, and it loses reach | [cutting mechanics](https://science-and-fiction.org/swordfighting/cuts.html) |
| A standard cut with its step | 0.6 s or more, with 10 to 20 cm more reach | the same |
| The blade's fastest turning | 1,000 to 2,000 degrees a second | the same |
| A fencer's lunge, start to hit | 0.70 s | [lunge study](https://ojs.ub.uni-konstanz.de/cpa/article/view/6606/5967) |
| A practised thrust at close range | under 0.2 s | [sword and shield](https://science-and-fiction.org/swordfighting/sword_and_shield.html) |

Our own clips agree: their cuts take 0.1 to 0.3 s once the wind-up is over. And 2,000 degrees a second at a sword's length is about the 30 m/s the game already caps a blade at.

One thing no source states as a rule, and I take as the design's backbone: **a cut comes in sizes.** A small one from the wrist, a middle one from the elbow, a full one from the shoulder with the hips and a step. Bigger is slower, reaches further and hits harder. The size is how a fighter fits a blow into the time he has.

## Openings and timing

A fighter attacks a part that is uncovered, at a moment when the enemy cannot cover it in time. The fencing books say where and when, and they agree with you.

**Where: the opening.**

- The old German school cuts the enemy into four openings, upper and lower, left and right. Every guard covers some and leaves others ([four openings](https://swordfight.uk/four-openings/)).
- "The best time to attack an opening is when the opponent's weapon is just moving away from it" ([Meyer's system](https://science-and-fiction.org/swordfighting/meyer_dusack.html)). After his blow, the opening furthest from where his blade ended.
- With a shield: "the shield nearly covers two openings and the sword takes care of a third one". What is left is the head when the shield is low, the sword hand and wrist, the upper leg, and the leg under the shield, which needs a crouch and is slow. A shield raised to cover the head blinds the man behind it ([sword and shield](https://science-and-fiction.org/swordfighting/sword_and_shield.html)).
- The bones agree. At Visby (1361) most of the 456 cut wounds were in the lower half of the body, which the authors put down to shields covering the upper half. At Towton (1461) forearms were the commonest body wound ([battle injuries](https://www.medievalists.net/2024/03/medieval-battle-injuries/)).

**When: the tempo.** The Italian school lists five moments to strike ([tempo](https://www.chicagoswordplayguild.com/defining-tempo-in-italian-swordplay-its-tactical-implications)):

1. As he lifts a foot to step.
2. As he changes from one guard to another.
3. As he raises his hand to strike.
4. After his blow has gone past you.
5. After you have parried it.

The rule under all five: your movement has to be shorter than the movement he needs to answer it.

**How much time there is.** Fencers answer a single signal in about 0.26 s, and take 0.36 to 0.42 s when they must choose between two or three ([reaction study](https://scielo.br/j/rbcdh/a/YY7fdDMcB3yq9zjvCVsJszg/?lang=en)). The makers of For Honor say players who answer in under 0.2 s are "exceedingly rare" ([For Honor](https://www.ubisoft.com/en-us/game/for-honor/news-updates/9OPowoU28h2YRO8zSvw5z/testing-grounds-how-attacks-are-changing)). So for your 400 ms opening:

| The attacker | Time to see it | Time left for the blow | What fits |
| --- | --- | --- | --- |
| Knows it is coming (the best case, for testing) | 0 s | 0.40 s | A wrist or elbow cut, or a thrust |
| Sees it open, and was waiting for that one part | about 0.25 s | about 0.15 s | Only a wrist cut from a sword already pointed there |
| Has to pick among several parts | about 0.4 s | none | Nothing. He must have guessed |

Three things follow for the design:

1. **The ready position is half the attack.** A sword waiting at the start of its cut needs only the cut. This is what a guard is.
2. **The size of the cut is the timing lever.** A short opening takes a small cut; a long one allows a full one.
3. **Reaction, timing and aim are the three stats** you asked for. At their best they are all zero, which is the first row of the table.

## How games and research build attacks

There are four ways to make an attack reach a chosen place at a chosen time, and only one of them works with what we have.

| Way | Who does it | What it gives | Why it does or does not fit us |
| --- | --- | --- | --- |
| **Bend a recording** | [Unreal's motion warping](https://dev.epicgames.com/documentation/en-us/unreal-engine/motion-warping-in-unreal-engine) moves and turns a recorded attack's root so it lines up with a target. [Witkin and Popović](https://www.ri.cmu.edu/publications/motion-warping) bend a recording's curves to meet a few set poses and keep its detail | A recorded blow that ends in the right place | This is what the game does today. It fixes where the body goes, not which part the blade meets or when |
| **Blend many recordings** | [Verbs and adverbs](https://eecs.vuse.vanderbilt.edu/People/bobbyb/pubs/VandAdv98.html): record the same action many ways and blend between them by a few dials | Smooth, natural motion anywhere between the examples | It needs a recording for each corner of what you want: every line, height, size and distance. We have nine clips |
| **Learn it** | [Won and others](https://ai.meta.com/research/publications/control-strategies-for-physically-simulated-characters-performing-two-player-competitive-sports/) trained physical boxers and fencers by trial and error, first the skills, then the tactics | Physical fighters that find their own blows | A research project with its own training rig. Not for now |
| **Build it from parts** | [Kingdom Come](https://www.pcgamer.com/kingdom-come-deliverance-lets-you-attack-36-body-part-zones) built a system that "uses procedural animations" because doing it by hand would have taken thousands of them. [Overgrowth](https://gdcvault.com/play/1020049/Animation-Bootcamp-An-Indie-Approach) makes fluid motion from "very few key frames" and simple rules between them | Any target, any moment, any body size | It is how the get-ups and the sword parry are made here. The risk is that it looks mechanical, and the answer is in how much of a real cut's order goes into it |

Three more games worth knowing, for what they decide by physics:

- [Hellish Quart](https://kubold.com/blog/2020/7/24/hellish-quart-a-game-about-historical-fencing) plays recorded fencing on physical bodies: "the blades really block each other". Four buttons give four attack angles. It cannot aim at a part.
- [Exanima](https://cdn.akamai.steamstatic.com/steam/apps/362490/manuals/Exanima_Quickstart_Guide.pdf) lets the player set a swing's side and timing and lets the physics decide what it does. Its guide says the computer's fighters are given a person's reaction time.
- [Mordhau](https://mordhau.fandom.com/wiki/Combat?oldid=3196) splits an attack into a wind-up and a release, and players bend a swing to land early or late so the parry misses it. That is timing used as a weapon, and it is the lever you asked for.

And one number: [For Honor](https://www.ubisoft.com/en-us/game/for-honor/news-updates/9OPowoU28h2YRO8zSvw5z/testing-grounds-how-attacks-are-changing)'s quick attacks take 0.4 to 0.5 s. Its makers shortened what the defender sees of them to 0.3 to 0.4 s, so that blocking them is a guess and not a reflex.

## The design: an attack built from its parts

The fight gives an order, "this part, at this moment". The attack works backward from the blade meeting that part to a motion of the whole body.

### The order

Four things, and nothing else, come from the fight:

- **Who.**
- **Which part.** The 16 parts of the body, plus the neck and the two shoulders: 19 targets.
- **When.** A moment to land, or "as soon as you can".
- **How hard.** A quick touch, or a blow he commits to.

What each target usually takes. This is my reading of the sources, set out so you can correct it; the tests will correct it too.

| Target | The blows that cross it | What it asks of the attacker |
| --- | --- | --- |
| Head | Straight down, the two diagonals down, a level cut to its side, a thrust to the face | Little. It is high and near |
| Neck and shoulders | The diagonals down; a level cut to the neck | The blade has to come over or round his guard |
| Upper arms | A diagonal down or a level cut from that arm's own side | It can only be reached from outside. From inside, his chest is in the way |
| Forearms and hands | Down onto one held level, up under one held high. The small cuts | The quickest target: it is nearest the sword. It also moves the most |
| Chest and belly | The diagonals, the level cuts, the thrust | A way past both his arms and what they hold |
| Hips and thighs | A diagonal down, a low level cut | Bent knees and a lean in |
| Shins and feet | A low level cut, a rising diagonal | A deep bend or a lunge. It is slow, and it bares the attacker's own head |

### Step one: choose the blow

Before anything moves, the attack tries every blow it knows against the target as he stands, and keeps the ones that would work.

A blow is a **line** (the eight cuts and the thrust), a **size** (below) and a **step** (none, a short one, a lunge). For each, the fighter is posed as he would be at the moment the blade meets the part, and five questions are asked:

1. **Does it reach?** Arm, blade and step together.
2. **Is the way open?** The blade is swept along its path ahead of time. If it meets his shield, his sword, another part of him, my own shield or a friend before it meets the target, the line is closed.
3. **Does it cross the part?** A cut along a limb glances off. A forearm held level takes a cut from above; a shin takes one from the side.
4. **Does it fit the time?**
5. **Can the feet hold it?** The body's weight has to stay over the feet through the blow.

Of the blows left, it takes the quickest. If none is left there is no attack, and the log says why: too far, covered, or no time. A fighter who does not strike at a covered part is the behaviour you asked for.

### The four sizes

| Size | What leads it | The blade turns | Lands in | Force | Good for |
| --- | --- | --- | --- | --- | --- |
| Wrist cut | The wrist; the forearm rolls | 60 to 90 degrees | 0.2 to 0.3 s | Light | A short opening; a hand, a forearm |
| Elbow cut | The forearm swings | 90 to 150 degrees | 0.3 to 0.45 s | Middle | Most blows |
| Full cut | The shoulder, the hips and a step | 150 to 270 degrees | 0.6 s and more | Heavy | A long opening; a leg; the blow that ends it |
| Thrust | The arm straightens, the body follows | It does not turn | 0.2 to 0.5 s | A point | The gap beside a shield; the face |

The times are starting values from the sources above. Each will be a number on the Combat Feel object, and later a stat.

### One cut in time

Every blow has the same five phases: wind up, cut, contact, carry through, come back. A small cut has a short wind-up or none.

&#91;embedded content: one cut in time · eight parts of the body, four phases\]

Read it left to right. The coloured bars are the cut itself: each part starts a little after the one above it, and they all arrive together. The foot comes down just after the blade does.

### How the motion is made: three poses, worked out backward

1. **At the target.** The blade is placed first: its striking part on the body part, its edge square to the line, aimed to stop 10 to 20 cm past the surface. Then the hand that holds a blade there. Then the arm, with the arm solver the sword parry already uses. Then the chest and hips, turned and leaned so the shoulder is where that arm needs it. Then the knees, bent for a low target. Then the feet.
2. **Wound up.** The same pose unwound: the blade turned back along its line by the size's angle, the chest turned away, the weight on the back foot.
3. **Carried through.** The blade taken on past the target into the guard on the far side.

Between the poses each part of the body moves on its own clock, in a real cut's order: hand first, hips, chest, elbow, wrist, and the foot landing with the blow. For the last 50 ms into the target the blade is held exactly on its line at a steady speed, so the edge arrives true.

This is how the get-ups are made: a few poses built in code from where the body is, with each limb on its own timing between them. Every length in it is the fighter's own (his arm, his sword, his legs), so one blow serves a Medium and a Large.

### Landing at a moment

- Each phase has a shortest and a longest time.
- **Early?** The fighter winds up and waits there. A wound-up sword is a guard, and from it only the cut is left.
- **A little off?** A cut in flight can be slowed or hurried by about a fifth.
- **Too late for this size?** A smaller size. If the smallest is too slow, no attack.
- **The point of no return** is about 0.1 s before contact. After it nothing changes.

### A target that moves

Until the point of no return, the attack reads where the part is each step and bends the cut toward it, no faster than a hand can change course. It also reads where its own physical blade is against where it planned it, and corrects for the difference.

### After contact

- **It cut through, or missed:** the blade carries on into the far guard, and the fighter comes back: head, body, foot.
- **It was stopped** by a shield, a sword or a heavy body: the blade backs out, as it does today.

### Made to hit hard

A blow drawn exactly as a fencing book teaches is small and tidy, because a real fighter hides what he is about to do. On a screen that reads as weak. So each blow is pushed past the book, by one dial, in the places an animator would push it:

| Where | Real | Pushed |
| --- | --- | --- |
| The wind-up | As small as the cut allows | Wound further back, the chest turned further away, the weight sunk onto the back leg |
| The start of the cut | Speeds up smoothly | A short stillness at the top, then it goes all at once |
| The body in the cut | Only as much as the reach needs | The hips and chest thrown in, the body dropping into the blow |
| The carry-through | Stopped early, to be safe | Carried well past the target, the body following the sword |
| The step | Lands with the blow | Lands hard with it |

The dial is one number on the Combat Feel object: at 0 the blow is the book's, at 1 it is the pushed one, and it starts pushed. The moment of contact does not move with it, so a pushed blow still lands on its part at its time. A small cut stays small, or it would stop fitting a short opening; what it gets is the snap.

What already sells a hit stays: the hit stop, the blade stopped by what it meets, the body knocked by the blow's real force.

### The levers

All of these are perfect in the tests, and become stats later:

| Lever | What it changes |
| --- | --- |
| Reaction time | How long after an opening appears the fighter can start |
| Timing error | How far from the asked moment the blow lands |
| Aim error | How far from the asked point the blade is steered |
| Cut speed | The time each size takes |
| Point of no return | How late he can still change his mind |
| Reach judgement | Whether he tries blows that fall short |

### What the Mixamo clips are still for

They are measured, not played. Their wind-up and cut times, how far the body turns, and how far it travels are the numbers the sizes start from. Headless, I can film a clip and a built cut side by side as a check.

## How it fits the fighter we have

The new attack writes the same thing a clip writes today, a pose for each step. Everything after the pose stays as it is.

| Piece | Today | With aimed attacks |
| --- | --- | --- |
| Where the pose comes from in a strike | A strike clip, turned, stretched and pulled (`LocoBody.PlayStrike`) | The built blow, starting from the pose the body is in |
| The sword arm | The clip's arm, with the hand pulled | The arm solver (`ArmRig.Solve`) that the sword parry uses, with its joint limits |
| The trunk | The clip's | Built from the blow, on the same joints |
| The feet | Where the clip's root and feet go | The stance's own steps (`StepsInStance`), with one step planned for the blow |
| Balance | Not checked | The legs' opinion (`Grounding`): the weight over the feet through the blow |
| The physical body | Follows the pose | The same. Hips held, legs physical, arms on their motors |
| What counts as a hit | What the physical blade touches near the clip's contact (`StrikeHits`) | The same, with the blade live from the start of the cut to the end of the carry-through |
| Damage, hit stop, a stopped blade | `DamageRules`, the rebound | The same |
| A blow's weight | Measured from the clip's sweep and speed | Worked out from the built blow's size and speed |
| What the defender reads | The blade's coming path, from the clip (`BlowRead`) | The same, from the plan, which knows the path exactly |
| Who and when | The fight's timer, then a clip by chance (`StrikeThatLands`) | The fight gives the part and the moment. Its notion of an open part (`Openings`) already exists |
| Combos | Clip pieces linked | Later: one blow's end is the next one's wind-up |
| The Large's club | The sword clips, one hand | Later: the same blows with two hands on the handle |

Three things that are new and have no clip to lean on:

- **The fighter's guard is where a blow starts.** Between blows the body stands in the guard's clip, as now. A blow starts from wherever the body is, as a get-up does.
- **The shield arm in an attack.** It has to stay between the two fighters and out of its own sword's way. The shield's own code keeps doing the first; the plan checks the second.
- **A puppet shield for the second test.** The target's shield is moved by the test, not by its own judgement.

## The two tests

Both are places in the debug panel's place bar, beside the lab, the battle, the targets and the tubes. Each has a headless twin, so I can run every case before you look.

### Test 1: every body part, on a target that stands still

One swordsman and one target. You pick a part and press Strike.

| In the panel | What it does |
| --- | --- |
| A body map of 19 buttons | Picks the part to attack |
| Strike | One blow at that part, the best the attack can find |
| Strike them all | Each part in turn, a second apart |
| Line and size | Left on "choose for me", or set by hand to see one blow |
| Target: open hands, sword, sword and shield | What stands in the way |
| Distance and facing | The target nearer, further, or turned |
| Slow motion | The speed slider that is there now |

**The red tint.** A part the blade touches turns red at once and fades back over one second. It is the part that was really hit, not the part aimed at, so a miss shows as the wrong part turning red.

**What is written for each blow** (on screen and in the log):

| Measure | Good is |
| --- | --- |
| Part aimed at, part hit | The same |
| How far the blade's striking part passed from the aim point | Under 3 cm |
| The edge against the line of travel | Under 15 degrees |
| Speed at contact | In the size's range |
| Weight over the feet through the blow | Yes |
| Any arm joint past its range | None |
| The physical blade against the planned blade at contact | Under 3 cm |

### Test 2: the shield that opens

The target stands with its shield over the part you picked. On a timer, or on a button, it takes the shield away for a set time and puts it back.

| In the panel | What it does |
| --- | --- |
| The body map | The part the shield covers and the sword goes for |
| Open for | 200 to 1,000 ms; 400 to start |
| Open now, or every few seconds at random | When the opening comes |
| Reaction time | 0 to 300 ms after the shield starts to move. He has to see it move; at 0 he starts the instant it does |
| Timing and aim error | 0 to start |

**What is written for each opening:** did the blow land inside it, how many ms after it opened, which part it hit, or what stopped it. The red tint shows the hit; a blow on the shield shows no tint.

A pass, at its best settings: the right part, inside the window, every time the window is long enough for the smallest cut. And no blow thrown at all when it is not.

## The steps, in order

Ten steps. The first builds the test and measures today's attacks, so every later step has a number to beat. Your first test is whole at step 5, your second at step 6. The battle keeps its clips until step 8, so it gets no worse while this is built.

| Step | What is built | What you will see | What I measure |
| --- | --- | --- | --- |
| 0 | **The aim test and today's numbers.** The place, the body map, the red tint, the readouts. Today's clips are asked for each part | The tint working, and how seldom today's blows hit the part asked for | Part hit against part asked, for all 19 |
| 1 | **The blade and the arm.** Wrist and elbow cuts on all eight lines, at a part within reach, the body standing still | A standing swordsman cutting at a head, a shoulder, an arm, with his arm alone | Miss in cm; the edge; arm joints in range |
| 2 | **The trunk.** The hips and chest turn and lean behind the cut, the hips ahead of the chest. The knees bend for a low part | The same cuts with the body in them. A thigh and a hip in reach | The order the joints peak in; reach |
| 3 | **The step.** The front foot goes with the blow and lands as it lands. The weight shifts onto it. The step back | Blows from fighting distance. Shins and feet in reach | Foot landing against contact, in ms; weight over the feet |
| 4 | **Full cuts and the thrust.** The wind-up, the carry-through into the far guard, the way back | Heavy blows that look thrown, not placed | The blade's speed along the cut; time to land |
| 5 | **Choosing the blow.** Line, size and step picked for the part; the path checked for his shield, his sword, his body, my shield | **Test 1 whole:** any of the 19 parts, on an open, armed or shielded target | Hit rate over 19 parts, three targets, three distances |
| 6 | **Landing at a moment.** Waiting wound up; hurrying or slowing a cut; the point of no return; refusing a blow there is no time for. The puppet shield | **Test 2 whole:** the shield opens for 400 ms and the blow is there | Lands inside the window, yes or no; ms after it opened |
| 7 | **A target that moves.** The cut bent toward a part that sways or steps. An attacker with a shield of his own | Hits on a target walking and turning | Hit rate on a moving target |
| 8 | **Into the fight.** The fight orders a part and a moment. The defender reads the blow. Combos as chains of blows. The Large's club. The strike clips go | The battle | Tonight's numbers again: attacks that touch nothing, hits on a chosen part |
| 9 | **The levers as stats.** Reaction, timing error and aim error for each fighter | Fighters who differ, and miss like people | Hit rates against the stats |

Every step is done the way the falls were:

- It sits behind a switch on the Combat Feel object, with a tooltip, and the panel has a tick for old against new.
- It has a button that makes it happen on demand.
- I report the numbers before and after, and say plainly what I could not check.
- It ends with a README entry and a note in this plan.
- You look before the next step starts. Steps 1 and 2 most of all: that is where it is decided whether a built cut looks like a cut.

## Risks, and what was decided

The biggest risk is the one this game has met before: a blow made in code that reaches its target and does not look like a blow.

| Risk | What I do about it |
| --- | --- |
| **It looks mechanical.** The first coded blows were replaced by clips | The body's order is built in from the start: hand, hips, chest, elbow, wrist, foot. The physical body softens it, and each blow is pushed past the fencing book ("Made to hit hard"). You judge it at steps 1 and 2, before anything rests on it. If it fails there, the fallback is a coded arm and blade on top of a clip's trunk and legs |
| **The physical blade misses thin parts.** A forearm is 6 cm across, a shin 7. In a strike today the physical sword hand is within 2.4 cm of its pose half the time, and more than 21 cm off one step in twenty | Measured at contact from step 1. The attack corrects for where its real blade is. If that is not enough, the sword arm is held tighter to its pose for the last tenth of a second |
| **The legs cannot hold a lunge** on the new physical legs | The plan asks the legs' opinion before it commits, and takes a shorter step or a smaller cut |
| **Perfect aim makes fights short and deadly** | That is what the levers are for. Before aimed blows go on in the battle, the many-seeds check is run for who wins and how fast |
| **The defender stops everything.** He reads a planned blow exactly | Then nothing lands except into an opening, which is the game you describe. If it is too much, his reading gets its own error |
| **Choosing a blow costs time** each attack | It is one pose and one sweep for each candidate, a few dozen, once per attack. Measured at step 5 |
| **I cannot see it** | The measures say whether it hits, on time, in balance. Your eye says whether it is a sword fighter. Every step waits for it |

**Decided with Renier on 14 November.**

1. **What is scrapped.** In the end, every Mixamo animation, in stages: building each one for this system is worth more than borrowing it. Attacks are the first stage. The guard, the walk, the turns, the block and the hit reactions stay on their clips while attacks are built.
2. **The first attacker.** A swordsman with no shield, then sword and shield, then the Large's club.
3. **The targets.** The 16 parts plus the neck and the two shoulders: 19.
4. **Thrusts.** In, with the cuts.
5. **A covered part.** The attacker holds back. The shield covering a part is there so the attacker's skill at using openings can be tested and, later, so a skilled swordsman can take an opponent apart through them.
6. **The second test's attacker.** He has to see the shield move. He is never told the moment in advance.
7. **Combos.** Today's combos go with the clips they are made of. They are rebuilt later from aimed blows.
8. **How it must look.** Not mechanical. The blows are exaggerated so that they feel like they hit hard.

## When to build it

Next, before staggers. Three reasons.

- **It is what looks wrong now.** The falls and get-ups hold up in the tube test. The battle does not, and the attacks are why.
- **Staggers and fall tuning depend on it.** Both are tuned against what blows land, where, and how hard. Aimed blows change all three. Done first, staggers would be tuned against swings that are about to go, and tuned again.
- **It makes staggers easier to build.** A stagger needs a blow on a leg or a shoulder on demand. Today only the tube gives that. An aimed blow gives the real thing.

The order I propose:

1. **Aimed attacks, steps 0 to 6.** Both of your tests. The battle is untouched.
2. **Staggers on real legs,** driven by aimed blows in the test.
3. **Aimed attacks, steps 7 to 9.** Into the battle, then the stats.
4. **How often fighters fall,** and the many-seeds check of who wins.

The loose ends I listed (the knee that dips into the ground getting up, the head in a hard sideways fall, the lab's old panel) fit between these whenever one bothers you.

The other order, staggers first, costs nothing but the second tuning. If you would rather finish what was agreed before starting this, say so and I will.

## Sources

Pages I opened and took facts from.

**How a cut is made**

- [Cutting mechanics](https://science-and-fiction.org/swordfighting/cuts.html), Science and Fiction: the pivot, the phases of the arc, cut times and turning speeds, measured by a practitioner.
- [What makes an effective sword cut?](https://thearma.org/essays/howacutworks.htm), ARMA: the edge, speed, shoulder against wrist.
- [Coordinating a step and a cut](https://www.bookandsword.com/2023/03/04/fencing-problems-coordinating-a-step-and-a-cut/), Book and Sword: hand before foot, with Waite, del Frate, Arlow, Giganti and the Liechtenauer glosses.
- [The six cuts](https://swordfight.uk/the-six-cuts/), Swordfight.uk: the sabre's cuts, from the wrist, through the target.
- [Meyer's system for the dusack](https://science-and-fiction.org/swordfighting/meyer_dusack.html), Science and Fiction: the eight lines, guard to guard, when to attack an opening.
- [Use of the kinematic chain in the fencing attacking lunge](https://ojs.ub.uni-konstanz.de/cpa/article/view/6606/5967): the order the joints peak in, 0.70 s start to hit. Ten fencers, so a pointer more than a proof.
- [Biomechanical factors to shorten the men strike in kendo](https://www.jstage.jst.go.jp/article/ijshs/15/0/15_201611/_article/-char/en): the front foot lifts as the sword rises. Abstract only.

**Openings and timing**

- [Four openings](https://swordfight.uk/four-openings/), Swordfight.uk.
- [Defining tempo in Italian swordplay](https://www.chicagoswordplayguild.com/defining-tempo-in-italian-swordplay-its-tactical-implications), Chicago Swordplay Guild: dall'Agocchie's five moments.
- [Tactics in Viking single combat with sword and shield](https://science-and-fiction.org/swordfighting/sword_and_shield.html), Science and Fiction: what a shield leaves open.
- [Medieval battle injuries](https://www.medievalists.net/2024/03/medieval-battle-injuries/), Medievalists.net: Visby and Towton.
- [Importance of the full leg target](https://thearma.org/essays/fullleg.htm), ARMA: the low line, and what a leg attack exposes.
- [Simple and choice reaction time of fencers](https://scielo.br/j/rbcdh/a/YY7fdDMcB3yq9zjvCVsJszg/?lang=en): 0.26 s to one signal, 0.36 to 0.42 s to a choice.

**Games and methods**

- [For Honor: how attacks are changing](https://www.ubisoft.com/en-us/game/for-honor/news-updates/9OPowoU28h2YRO8zSvw5z/testing-grounds-how-attacks-are-changing), Ubisoft.
- [Mordhau wiki: combat](https://mordhau.fandom.com/wiki/Combat?oldid=3196).
- [Kingdom Come: Deliverance sword fighting](https://www.pcgamer.com/kingdom-come-deliverance-lets-you-attack-36-body-part-zones), PC Gamer.
- [Hellish Quart](https://kubold.com/blog/2020/7/24/hellish-quart-a-game-about-historical-fencing), Kubold.
- [Exanima quick start guide](https://cdn.akamai.steamstatic.com/steam/apps/362490/manuals/Exanima_Quickstart_Guide.pdf), Bare Mettle.
- [Motion warping in Unreal Engine](https://dev.epicgames.com/documentation/en-us/unreal-engine/motion-warping-in-unreal-engine), Epic.
- [Motion warping](https://www.ri.cmu.edu/publications/motion-warping), Witkin and Popović, 1995.
- [Verbs and adverbs](https://eecs.vuse.vanderbilt.edu/People/bobbyb/pubs/VandAdv98.html), Rose, Cohen and Bodenheimer, 1998.
- [An indie approach to procedural animation](https://gdcvault.com/play/1020049/Animation-Bootcamp-An-Indie-Approach), David Rosen, GDC 2014. The description only; I could not watch the talk.
- [Control strategies for physically simulated characters](https://ai.meta.com/research/publications/control-strategies-for-physically-simulated-characters-performing-two-player-competitive-sports/), Won, Gopinath and Hodgins, 2021. Abstract only.

**From the project**

- Your playtest log of 14 November, 19:19: the battle's attacks and hits.
- The baked clips (`CombatClips.json`) and their measured cores: the clip timings.
- The code that makes an attack today: `LocoBody.PlayStrike`, `ClipFighter.AimAt` and `StrikeThatLands`, `StrikeHits`; and the first coded blows, `SwingPath` and `MovePlanner`.

**What I looked for and did not find.** A laboratory measurement of how long a one-handed sword cut takes by its size; the cut times here are one practitioner's own measurements. A page that sets out cuts from the wrist, the elbow and the shoulder side by side; the one I found would not open. The share of Visby's wounds that were on the lower legs; the page I could open says only "most".

## Progress

**14 November: decisions taken** (the list under "Risks"). Attacks come before staggers. Step 0, the aim test and today's numbers, is started.

### Step 0 done: the aim test, the red tint, today's numbers (14 November)

Nothing about how anyone fights changed. Built headless; not yet looked at in Unity.

**What is there.** A new place, **Aim** (key 7): one swordsman, one target that only stands there. A map of the body with the 19 spots; pressing one asks for a blow at it. The target can hold nothing, sword and shield, or a sword, stand nearer or further, and be turned. Any part a blow lands on turns red and fades over a second, in the battle too (Combat Feel "Struck Parts Flash Red"). For now a blow is whichever of today's nine clips reaches nearest the spot.

**What each blow is measured by.** What the blade met first, when, how fast, how far from the spot. How near the blade's striking part came to the spot before it met anything. How far the edge was turned off the way the blade was going. How far the blade as seen was from its pose, and from the path its pose took. How far the swordsman's weight was outside his feet as it landed. These are the numbers every later step is judged by.

**Today's numbers** (19 spots once each, from 1.4 m):

| The target | On the spot | Another part | Shield or sword | Nothing |
| --- | --- | --- | --- | --- |
| Open hands, facing | 5 | 13 | 0 | 1 |
| Open hands, side on | 2 | 17 | 0 | 0 |
| Sword and shield | 0 | 5 | 12 | 2 |
| A sword | 0 | 5 | 14 | 0 |

- **Aim:** one blow in four lands on the part asked for, against a target that does nothing. Against any guard, none.
- **Timing:** contact comes 0.18 to 1.13 s after the blow is asked (0.7 s at the middle), set by the clip.
- **Edge:** 22 to 26 degrees off the blade's way at the middle.
- **Balance:** the weight is 34 to 38 cm outside the feet as the blow lands. The stretched lunges land before the front foot does.

**One finding that changes the plan a little.** The physical blade follows the path its pose takes (11 to 14 cm off it at the middle) but late: at the worst of a blow it is about 50 cm behind where the pose has it. So the body carries out the line it is given, a few hundredths of a second after. Step 1 should measure that delay exactly, and step 6 (landing at a moment) has to plan for it, not find it out then.

**Not verified:** everything seen in Unity: the place, the body map, the walk back to the spots, and the tint itself (it cannot be seen headless).

**Next:** step 1, the blade and the arm: wrist and elbow cuts along the eight lines, from a standing body.

### Steps 1 and 2 done together: the sword arm and the trunk (14 November)

Built blows are in the aim test, behind three Combat Feel switches. The battle still uses the strike clips. Built headless; how it looks is not yet seen by anyone.

**Why the two steps became one.** The arm alone reaches nothing from where fighters stand. A swordsman stands with his sword shoulder back, so from 1.05 m apart his shoulder is 1.24 m from the other's head, and an arm and a blade reach 1.1 m. Brought nearer, his guard's point is already in the other's body. With the trunk turning in and leaning, the reach is about 1.25 m, and a standing blow can be shown from 1.05 m.

**How the blow is made** (this replaces "three poses of the blade" in the design above):

- The first version placed the blade and asked the arm to follow. The wrist was 70 to 110 degrees past its range. A blade's path is not something an arm can always make.
- The version that works is made of the joints' own turns. The upper arm, forearm and blade swing in one plane about the elbow's hinge, like a hammer: the trunk turns, then the shoulder, the elbow, the wrist. Every joint is in range by construction.
- The arm at the target comes from the arm rig. The whole arm is then turned about the shoulder-to-target line until its swing comes in along the line asked for.

**What it does**, 19 spots once each from 1.05 m:

| The target | On the spot | Another part | Shield or sword | Nothing |
| --- | --- | --- | --- | --- |
| Open hands | 5 | 13 | 0 | 1 |
| Sword and shield | 1 | 2 | 15 | 1 |
| A sword | 1 | 6 | 12 | 0 |

- **Timing:** an elbow cut lands 0.40 s after it is asked against 0.38 planned; a wrist cut 0.27 to 0.28 s against 0.26.
- **Edge:** 13 degrees off the blade's way (the clips: 22 to 26).
- **Arm:** in range throughout.
- **Reach:** 12 of the 19 spots are in reach from standing feet. 5 are hit, 2 land 12 cm off, and 5 are stopped by the target's own arm in the way, which nothing checks yet (step 5).
- **Defence:** a shield or a sword reads a built blow and meets it, as it does a clip's.

**Changes to the plan from here.**

- **Level cuts from the right** cannot be made by a hammer's arm: asked for, they come in 50 to 87 degrees off line. They need the upper arm rolling and the trunk turning, palm up. That is added to step 4 (full cuts).
- **The knees** for low parts move from step 2 to step 3, with the step.
- **Step 3 matters more than planned.** Shins, feet and the far arm are 11 to 54 cm out of reach, and the swordsman's weight is 27 cm outside his feet as a blow lands.
- **The delay between pose and body** is still about 50 cm of blade at the worst of a blow; the blade lands 0.01 to 0.02 s late. Small enough to plan for in step 6.

**Next:** Renier looks at the blows in Unity. Then step 3, the step.

## Back to the drawing board: attacks made from the recordings (night of 14 to 15 November)

**Bottom line.** The built blows (steps 1 and 2) are set aside: an arm and a trunk on still feet look mechanical. An attack is now made from a recorded strike. The recording is taken apart into each body part's own track, the tracks are changed (by a style, and so the blade lands on the part asked for), and the body is put together again. The whole body moves as the recording's does, legs included. Fourteen levers change how it is thrown. It is in the aim test (key 7); the battle still plays the strike clips.

### What a sword blow is made of

Read off the recorded forehand cut, frame by frame, each joint. Times are against the moment the blade lands.

| Phase | Legs | Hips | Chest | Sword arm |
| --- | --- | --- | --- | --- |
| Guard | Side-on. Left foot 0.34 m ahead, right 0.41 m behind, knees bent 41 and 46 degrees | 0.88 m up, turned 38 degrees right | A little further right | Folded, blade up |
| Wind-up (first 0.46 s) | Front foot lifts at -0.53 s, steps 0.20 m, lands at -0.27 s: before the cut | Wind back a few degrees | Winds back to 62 degrees right | Drawn back: elbow 100 degrees, blade cocked 108 degrees off the forearm |
| Drive (last 0.17 s) | Back knee straightens 46 to 21 degrees: the push. Back heel up. Front knee bends 41 to 81 degrees and takes the weight | Travel 0.46 m forward, sink 12 cm. Turn fastest at -0.17 s (353 degrees a second) | Turns fastest at -0.13 s (550) | Upper arm, elbow and wrist all peak in the last 0.03 s (600 to 740). Point at 36 m/s |
| Landing | Back foot dragging forward on its toe | Turned 75 degrees from the guard | Turned 136 degrees | Nearly straight (elbow 19 degrees) |
| Carried through (0.2 s) | Back foot still off the ground | 12 degrees more | 38 degrees more | Blade down and across |
| Back (to +0.87 s) | Front foot steps back at +0.67 s | Return | Return | Return |

Three things stand out, and all three match what sport science measures:

- **The back leg pushes, the front leg brakes.** The back knee and ankle straighten to drive the hips forward; the front leg lands first, bends and takes the weight, which is what turns forward travel into a turning body. A fencing lunge study measured the same: the rear leg's push, then 1,400 to 1,600 N on the front leg. In a bat swing the front leg takes 159% of body weight.
- **The body cracks like a whip.** Each part reaches its top speed a little after the one below it: hips, chest, then the arm. The papers call it the kinematic sequence.
- **The step comes before the cut.** The front foot is down a quarter of a second before the blade lands. The blow is thrown off a planted foot.

The other two plain recordings: the **rising backhand** does it on a passing step (the back foot goes 1.1 m past the front one, then the cut, hips rising); the **thrust** does it standing, on the ball of the back foot, hips turning 60 degrees. The other six recordings are leaps, spins and a fragment.

Sources: [fencing lunge kinematics and forces](https://pmc.ncbi.nlm.nih.gov/articles/PMC10900521/), [the kinematic sequence](https://www.frontiersin.org/articles/10.3389/fspor.2021.699251/pdf), [lead-leg forces and bat speed](https://scholars.mssm.edu/en/publications/lower-extremity-kinematic-and-kinetic-factors-associated-with-bat/).

### How an attack is made

1. **The score.** A recording becomes twenty tracks: the hips' place and turn, the chest's turn on the hips, the head, each arm's bones, each hand, each ankle, knee and foot. Put together again it is the recording to within about 1 cm.
2. **The style** changes how far each part goes in each phase, and when (the levers below).
3. **The aim.** Every moment of the cut at which the blade is still fast is tried as the moment to land. For each: hips lowered and chest leaned for the target's height, lunge stretched or shortened for its distance, body turned for its line, the wrist steering the last of it. Each costs something. The cheapest moment of the cheapest recording is taken.
4. **The legs can be another recording's.** The thrust was recorded standing; on the forehand's legs it is a lunging thrust. The forehand on the thrust's legs is a cut from where it stands.
5. **The moment.** Asked to land at a set time, the wind-up is stretched or shortened.
6. What comes out is an ordinary clip, so the walk, the feet, the hits, the defender's read and the physical body take it as they take a recording.

### The style levers

Each is 1 for the recording's own. They are Combat Feel fields (Attack ...) and sliders in the aim test.

| Lever | What it changes |
| --- | --- |
| Wind-up | How far hips and chest wind back before the drive |
| Arm swing | How far the sword arm is drawn back and carried through |
| Hip drive | How much of the turn comes from the hips rather than the spine |
| Twist | How far the shoulders come round on the hips |
| Lean | How far the chest leans |
| Lunge | How far the body travels (an aimed attack sets its own travel to reach) |
| Drop | How far the hips sink into the front knee |
| Crouch | How low the whole attack is carried |
| Carry-through | How far the body goes on after the landing |
| Wind-up time, Drive time, Back time | How long each phase takes |
| Hips lead, Arm lags | The whip: hips sooner, arm later |

Six styles to start from: Plain, Heavy, Quick, Brawler, Low, Whip.

### A bug found on the way

Every strike's step went the wrong way. A clip's travel was turned into its own frame back to front, so a clip that turns as it travels went off to a side by twice its turn. The rising cut's 0.42 m step forward went 0.33 m to the left; the lunging backhand's 1.3 m forward went a little backward. This is why lunges never looked like they drove at the target. Fixed, behind Combat Feel "Clips Travel As Recorded" (off: the old fight, blow for blow). It changes the battle too.

### Measured (headless)

- As posed, the blade's striking part passes **4 cm** from the spot asked for at the middle, 17 at the worst (19 spots, twice, from 1.5 m).
- On an open target **21 of 38** land on the spot asked for. The strike clips managed 5 of 19, the built blows 5 of 19. The misses land on a neighbour: the blade as seen trails its pose and meets a nearer part, or the target's own arm is in the way.
- Asked to land at 0.5 s, 0.9 s and 1.2 s, blows landed at 0.52, about 0.92 and 1.20 s at the middle.
- The lunging thrust reaches from 2.0 m (standing: 1.3 m).

### Not done, not verified

- **Not seen in motion.** I have only looked at stills of the made poses. How it looks in Unity is the question.
- Three plain recordings to make from. There is no plain low cut, overhead chop or descending backhand: low parts are reached by crouching and leaning a forehand or a rising cut.
- Nothing yet picks the open line against a shield or a sword: 27 of 38 are stopped.
- A body turned a long way to reach a line (the most is 50 degrees) pivots on its planted foot; the foot itself does not turn.

### Next

Your look at it. Then, in this order: the line that is open (step 4), Test 2 (the shield opens for 0.4 s), into the fight, levers as stats.
