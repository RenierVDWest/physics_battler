# Physics Battler: walking that feels like a real person (research and plan)

26 November 2026, 16:50. Live version: https://claude.ai/artifact/BgRzbPiWNQerLqk6kmdaXs (progress notes go there and
here). Written after his words of 16:09: "the units walking around just does not feel very concincing. They walk clumsily,
floating times. They do not seem like they have real weight", and his ask for a deep dive into a ground-breaking
walking system and where its animations can come from. Nothing in it is built. Six questions wait for him.
Its film: FILM_LINK

## The short answer

You are right, and the cause is where you looked: the footage. Measured today, our fighter walks at 0.75 m/s on recordings made at 0.9 to 1.2 m/s. So each recording is slowed to 0.85 of its pace and its stride is cut to 0.78. Slowed motion looks like weak gravity: that is the floating. He walks in spells of two or three steps, and his guard has no recorded start or stop. His steps forward are one performer's legs under another performer's chest.

Sliders cannot repair that. What can is what the best-walking games do.

1. **One coherent set of footage.** One performer, in the guard, walking every way, with the starts, stops, turns and changes of speed.
2. **Motion matching.** Several times a second a search picks the recorded moment that fits his pose and goes his way.
3. **The recording moves the body, not the code.** Your rebuild from parts and your sliders stay on top.

Can the footage be sourced online? Yes.

- **Epic did share such a pack:** the Game Animation Sample, free, over 500 animations and growing. Its licence is for Unreal Engine only, so we may study it and not use it.
- **Bought packs made for this exist.** My pick is MoCap Central's Dungeon Life and Core Motion, about $185 together.
- **A free set we may use exists too:** 100STYLE. It is enough to build and prove the system while you choose.

The plan has seven steps. The first three need neither Unity nor money. Your first look is step 3: one man walking alone, old against new. Nothing is built yet, and every number here comes from runs without a screen.

**Six questions wait for you at the end.** Two of them start the work: may I begin on the free set, and will you watch the two packs' previews?

## What is wrong today, measured

Your eye is right, and the numbers say why. The fighter is slid along at a speed nobody was recorded walking at, and a faster walk is slowed and shrunk to fit. I ran the game's own code without a screen and read his walk the way a gait lab would. Every number in this section is headless: I have not seen the walk in Unity.

*(A chart in the live document: the hips' rise and fall in a step against walking speed, for the 35 walking recordings as performed, for people (Orendurff and others 2004, centre of mass: 2.74 cm at 0.7 m/s, 4.83 cm at 1.6 m/s) and for our fighter (0.75 m/s and 6.3 cm on the course; 0.71 and 6.1, 0.74 and 7.9 in the dance). Headless, 26 November 2026.)*

| What was measured | Our fighter | The recordings he is made from | People |
| --- | --- | --- | --- |
| Speed while walking | 0.71 to 0.75 m/s | 0.9 to 1.2 m/s | 1.2 to 1.4 m/s when they choose |
| Pace the recording is played at | 0.85 of its own | 1 |  |
| Length of his stride | 0.78 of the recording's | 1 |  |
| Hips rise and fall in a step | 6.1 to 7.9 cm | 5.2 to 8.0 cm | about 2.7 cm at 0.7 m/s (centre of mass) |
| Hips' speed swings within a step by | 67 to 86% | 29 to 48% |  |
| One spell of walking lasts | 1.7 to 2.0 s: two or three steps | every recording is steady walking | steady walking begins after three to five steps |
| From standing to 80% of his pace | 0.3 s, with no shift of weight first | no start in his guard was recorded | a shift of weight, then about 90% by the end of the first step |

"Our fighter" is the swordsman on the footwork course (25 s) and both men of a dance (54 s). "The recordings" are the four steps his guard walks on, as their performers did them.

### Six causes

**1. He moves at a speed nobody was recorded at.** He walks at about 0.75 m/s. The library holds 35 different walking loops. Only two were performed under 0.9 m/s, and those are a relaxed side-walk, not his guard. The four steps of his guard were performed at 0.9 to 1.2 m/s. So the game plays them at 0.85 of their pace and cuts their strides to 0.78. He sits on the slowest pace the walk's own rule allows.

**2. Slowed motion looks like weak gravity.** A body drops onto its front foot at a pace set by gravity and the length of its legs. Play that drop at 0.85 of its pace and it takes longer than gravity allows: to the eye, gravity at 72% (0.85 times 0.85). That is the floating. [Retiming a motion changes the physics it implies](https://www.ri.cmu.edu/publications/physics-based-motion-retiming), and animation research treats that as a problem of its own.

**3. It is a fast walk made small, not a slow walk.** People walking slowly look different from people walking fast. At 0.7 m/s their weight rises and falls about 2.7 cm and shifts about 7 cm from side to side. At 1.6 m/s it rises and falls 4.8 cm and shifts under 4 cm (Orendurff and others, 2004). Our fighter goes at 0.7 m/s with the 6 to 8 cm rise and fall of the faster walks he is cut from. Our own two slow recordings agree with the people: 3.6 cm at 0.58 m/s.

**4. He never gets going.** One spell of walking lasts 1.7 to 2.0 seconds: two or three steps. A person spends the first step getting up to about 90% of his pace, and walks steadily only [after three to five steps](https://pmc.ncbi.nlm.nih.gov/articles/PMC5378148). So nearly all of a fighter's walking is starting and stopping. His guard has no recording of either. The library holds one relaxed start and one stop, forward only, and the guard cannot use them. Each spell is cut from the middle of a steady stride and switched on. He is at 80% of his pace 0.3 seconds after standing still, with no shift of weight first.

**5. The code slides the body, and the legs keep up.** The game decides where the body goes and then fits legs under it. In a real walk it is the other way round: the legs push, and the body answers. His hips surge and stall within each step by 67 to 86% of their speed, where the performers' hips swing 29 to 48%. His speed also jumps. In the dance each man's hips change speed by more than 0.5 m/s between two physics steps 46 to 60 times a minute. Some of those are blows landing, which is meant. Others are joins: from his stand into a walk, or from a walk into a leap. At one such leap the carried body went from 0.83 m/s to nothing in one step.

**6. It is a Frankenstein, inside a single frame too.** The 35 walking loops come from nine Mixamo packs or more. For one body the hips ride anywhere from 69 to 95 cm, and the cadence runs from 82 to 120 steps a minute. His guard's four steps come from two performers: forward and back are an archer's legs under the sword-and-shield man's chest and arms. Every loop is a single stride of about a second, so every step he takes is the same step. And his hips are carried by the pose with his legs hung from them, so physics cannot add the weight the recordings lack.

### What the plan of 17 November did, and could not

That plan stopped the body jumping between unlike recordings: the hips' band went from 24 cm to 6 cm, measured then. It did so by carrying every step on the guard's own stance and taking whole steps. It said plainly that it was not motion matching, because that "needs minutes of footage shot for the purpose". That footage is what this plan is about.

### How this was measured

Two new tools did the measuring. `Tools/Mocap/Locomotion/gait_survey.py` reads every walking recording as a gait. `Tools/Headless/gaitfilm.py` reads a headless run of the game the same way. The runs were the footwork course and a dance, on the release build, one run each. Fights are chaotic, so a second run will differ a little. The hips are not the centre of mass, so the comparison with people is rough. Unity has the last word.

## What makes a walk feel weighted

A walk reads as heavy when gravity sets its timing and the body travels over its feet. Nobody sees weight itself. The eye reads about a dozen small things, and a slowed or patched walk loses most of them. This is the list a new system has to deliver, with where our walk stands on each.

| What the eye reads | In a real walk | Our walk today |
| --- | --- | --- |
| Timing set by gravity | The swinging leg and the drop onto the front foot have a pace of their own. Stretch it and gravity looks weak. | Played at 0.85 of its pace. |
| The shift of weight | The hips move over the standing foot before the other foot lifts. Slow walkers shift more: about 7 cm at 0.7 m/s, under 4 cm at 1.6 m/s. | A faster walk's shift at a slow speed. |
| Rise and fall | The body is lowest while both feet are down and highest over the standing leg. It grows with speed: 2.7 cm at 0.7 m/s, 4.8 cm at 1.6 m/s. | 6 to 8 cm at 0.7 m/s. |
| Both feet down | At a chosen pace both feet are on the ground for [about a fifth of each stride](https://pmc.ncbi.nlm.nih.gov/articles/PMC8217618/). At the very slowest speeds it [passes half](https://www.nature.com/articles/s41598-019-54271-2). A run has none: both feet leave the ground. | Not measured reliably yet. |
| The planted foot | It lands on the heel, rolls flat and leaves from the toes. It does not slide. | Feet are locked to the ground once down. This part is good. |
| Hips against shoulders | The hips turn with the legs and the shoulders turn against them. In our recordings that twist runs from 11 to 47 degrees, depending on the performer. | Forward and back, the legs and the chest are two performers'. |
| Starting | The weight first goes [back and toward the foot that will step](https://pmc.ncbi.nlm.nih.gov/articles/PMC5015477), so the body falls forward. Then the first step. | No start in the guard. He is at pace in 0.3 s. |
| Stopping and turning | A braking step, a planted foot, a push off the outside leg. | Turns on the spot are recorded. Stops and pivots in the guard are not. |
| Leaning into speed | The body leans into a start, back from a stop and into a curve. | A lean lever exists. I found nothing that ties it to how he speeds up. |
| No two steps alike | Small differences in length, timing and where the head goes. | One stride of about a second, repeated. |
| The head leads | He looks where he will go, or at his man: eyes and head first, then the chest, then the feet. | A head-look from an earlier milestone is in the code (`HeadLook`). Whether today's fighters still run it is to be checked. |
| What you hear | A footfall at the moment the foot lands. | Step sounds exist. To be checked against the new footfalls. |

### Three rules that follow

1. **Never stretch time by more than a tenth.** If he must go slower, he needs a recording of someone going slower. At 0.9 of its pace gravity still reads as 81%; at 0.85 it is 72%.
2. **One performer, one moment.** The hips, legs and spine of a step come from the same person at the same instant. That is where the shift of weight and the twist live.
3. **Record the changes, not only the walking.** A fighter's walking is mostly starting, stopping, turning and single steps. Those need recordings of their own.

## How the best games do it

The games whose walking looks real do the same three things. They record a lot of one performer. They let the recording move the body. And they choose the next moment of footage several times a second. The method is called motion matching.

### Motion matching in plain words

A performer is recorded for minutes at a time doing everything the character must do: walking each way, starting, stopping, turning, circling. Nothing is cut into loops. The game then keeps asking one question. Of all the recorded moments, which looks most like my pose now, and goes where I want to be in a second? It jumps there and plays on. A short blend hides the jump.

- **What is compared:** where the feet are and how fast they move, how fast the hips move, and the path of the body over the next second. One open implementation uses [59 numbers a moment and searches about five times a second](https://docs.o3de.org/blog/posts/blog-motionmatching/).
- **It is cheap.** By the same source, an hour of footage is 108,000 moments and about 24 MB of numbers. We need minutes, not hours.
- **Where it comes from:** Ubisoft's For Honor, shown by Simon Clavet at [GDC 2016](https://www.gdcvault.com/play/1022985/Motion-Matching-and-The-Road). Naughty Dog used it in [The Last of Us Part II](https://gdcvault.com/play/1027118/Motion-Matching-in-The-Last). Epic built it into Unreal Engine 5.
- **Its closest relative to us is For Honor:** two armed fighters who face each other and circle.

### What their recordings contain

The list of moves to record is called a dance card. This is the card for a fighter who faces his man, beside what our guard has today.

| Kind of recording | What it gives | In our guard today |
| --- | --- | --- |
| Standing, with shifts of weight | A stand that is alive | Three idles |
| Steady walking in eight directions, at two or three speeds | The body of the walk | Four directions, one speed each, from two performers |
| Starts in each direction | The shift of weight and the first step | None |
| Stops from each direction, off either foot | The braking step and the settle | None |
| Single steps and small adjustments | Moving half a metre without walking | None |
| Changes of direction while moving | The planted foot and the push | One, at a run |
| Turns on the spot | Squaring up when his man moves round him | Yes, several |
| Circling a point, each way | Walking round a man while facing him | None |
| Changes of speed: walk, jog, run, sprint and back | Gaits that flow into each other | None |
| Jog, run and sprint with their own starts, stops and turns | Covering ground | Loops only |

### Small corrections, never big ones

A recording rarely fits exactly. Three tools close the gap. The stride is made a little longer or shorter. The legs are turned a little off the recorded direction. The planted foot is held where it landed. Epic calls them [stride warping, orientation warping and foot locking](https://dev.epicgames.com/documentation/unreal-engine/pose-warping-in-unreal-engine). We already have all three. The difference is how much we ask of them: a good system corrects by a tenth, and ours stretches by a fifth or more.

### Who moves the body: the code or the recording

Daniel Holden's [Code vs Data Driven Displacement](https://theorangeduck.com/page/code-vs-data-driven-displacement) sets out the choice. If code moves the body, the position is exact and the feet have to be made to fit. If the recording moves the body, the feet are true and the position is less exact. Systems that look real let the recording lead, and pull it gently toward where the code wants it.

Here we have an advantage, and this is my own reading, not a source's. A player holding a stick feels every delay, so those games fight to stay responsive. An auto battler has no stick. Our fighters can let the recording lead.

### Facing a man while walking round him

Three things work together.

1. The performer is recorded facing forward while his legs go in each of eight directions. Between those eight, the legs are turned a little.
2. A layer on top turns the eyes, the head and the chest toward the other man, each by its share and at its own pace.
3. When he has turned further than that layer can reach, a recorded turn on the spot squares his feet again.

Epic's sample added a look-at layer and a search across two characters at once in its [August 2026 update](https://www.unrealengine.com/tech-blog/download-the-latest-game-animation-sample-project-now-updated-for-ue-5-8).

### The frontier: a body that carries its own weight

In everything above the body is still posed, not simulated. The next step is known from research. In Ubisoft's [DReCon](https://montreal.ubisoft.com/en/drecon-data-driven-responsive-control-of-physics-based-characters/), motion matching chooses the motion, and a simulated body that was trained by trial and error follows it with its own muscles. A shove then moves him for real, and he keeps his feet because he has to. For a physics battler, that is where ground-breaking lies. It comes second, because a physical body can only follow a good walk once a good walk exists.

### What Unity gives us

Unity ships no motion matching of its own. Free ones exist: [MxM](https://github.com/Frost-Blade-Studios/Motion-Matching-for-Unity), now under the MIT licence, and [an open research package](https://github.com/JLPM22/MotionMatching). Our fighters are posed in code from baked recordings and tested without Unity, so we would write our own. The search itself is small.

## Where the animations can come from

Yes, Epic shared exactly such a pack, and no, we may not use it: its licence is for Unreal Engine only. The practical source is a bought pack made for motion matching, for about 50 to 190 dollars. A free research set can carry the work until you have chosen one. None of this is legal advice: I read the licence pages, and I am not a lawyer.

### Epic's pack

Epic's [Game Animation Sample Project](https://www.unrealengine.com/en-US/blog/game-animation-sample) came out free in June 2024, with over 500 captured animations and a complete motion matching setup. Later updates added hundreds more, among them a new walking set [in December 2025](https://www.unrealengine.com/en-US/tech-blog/explore-the-updates-to-the-game-animation-sample-project-in-ue-5-7) that aims to need less data. The August 2026 update added physics, a look-at layer and matching across two characters. It is the best public example of the system you describe.

Its licence label reads "UE-Only Content - Licensed for Use Only with Unreal Engine-based Products" (as quoted on [Epic's forum](https://forums.unrealengine.com/t/what-can-i-do-with-game-animation-sample-that-recently-was-at-100-off/1903980); [Epic's content licence](https://www.unrealengine.com/en-US/eula/content) defines the term). So it cannot go into a Unity game. It is still worth an hour of your time. You can open it in Unreal for free and see the target with your own eyes, and its list of animations is a ready-made dance card.

### The sources side by side

| Source | What it is | Cost | May it go in a Unity game? | Fit for us |
| --- | --- | --- | --- | --- |
| Epic's Game Animation Sample | Walking, running and climbing captured for motion matching, with a full working example | Free | No: Unreal only | A reference to study |
| [MoCap Central: Dungeon Life](https://www.mocapcentral.com/resources/dungeon-life) | 825 animations (544 unique). A sword-and-shield set; walks and jogs in each direction with starts, pivots and stops | $134.99 | Sold for games; licence text still to be read | The closest to our guard |
| [MoCap Central: Core Motion](https://mocapcentral.com/products/core-motion-pack) | 659 animations. Unarmed walking in eight directions with starts, turns, stops and pivots | $49.99 | Yes: also sold on the Unity Asset Store | Moving outside the fight |
| [MoCap Central: Longsword Vol. 2](https://mocapcentral.com/products/animation-pack-longsword-vol-2) | 423 animations. Eight-way walks with starts, stops and pivots, in two sword stances | $139.99 | As Dungeon Life | The two-handed style, later |
| [MoCap Online: Mobility Pro](https://mocaponline.com/products/mobility) | 223 animations and more: walk, jog, run, eight-way strafes, pivots, changes | $99.99 | Yes: its [standard licence](https://mocaponline.com/pages/licensing) covers an indie game | An alternative for unarmed movement |
| Unity Asset Store packs | For one, a [sword-and-shield set](https://marketplace.unity.com/packages/3d/animations/sword-and-shield-animation-set-advanced-158412) with 180 movement clips at 60 frames a second | $49 and up | Yes | To judge by eye; starts and stops not listed |
| [100STYLE](https://zenodo.org/record/8127870) | Over 4 million frames, about 19 hours, of one actor: walks and runs forward, back and sideways, idles and the changes between them, in 100 styles | Free | Yes, with credit (CC BY 4.0) | To build and prove the system. A research capture, with no guard |
| [CMU motion capture database](https://mocap.cs.cmu.edu/) | 2,605 trials of many subjects, from the early 2000s | Free | Yes; it may not be resold | Old, noisy at the toes, not one coherent set |
| LAFAN1 (Ubisoft), Bandai Namco's sets | Good research sets | Free | No: non-commercial | Not usable |
| Mixamo | What we have | Free | Yes, but the raw files may not be handed out | The Frankenstein |
| AI generators, such as [Motorica](https://www.gamedeveloper.com/business/genai-animation-startup-motorica-raises-5-million-pounds-in-investment) | Walking generated along a path in a chosen style | A free tier; the rest on request | Terms still to be read | Promising, unproven for us |
| Our own capture | Exactly our stances and our dance card | A camera app is free or a few dollars a month; a suit is about $2,500 to 3,000 | Yes: it is ours | The only way to get exactly our guard |

I have read these product pages. I have not seen any of the motion, and I have not read MoCap Central's licence text.

### What a licence has to allow

1. **A commercial game in any engine.** Epic's sample fails this, and so do the research sets marked non-commercial.
2. **Where the files are kept.** Bought packs forbid handing out the raw files, and [Adobe's Mixamo FAQ](https://community.adobe.com/t5/mixamo-discussions/mixamo-faq-licensing-royalties-ownership-eula-and-tos/m-p/13234775) says the same of Mixamo's. Our repository is public and holds Mixamo files today. Before a bought pack goes in, the repository has to be private.
3. **Machine learning.** MoCap Online's standard licence does not allow training on its data without a paid permit. That matters only if we later train a physical body on the recordings.

### What I recommend

Buy one family of packs from one studio, so that the stand, the walk and the run share a skeleton and a way of moving. My pick is MoCap Central's Dungeon Life for the guard and Core Motion for moving outside the fight: about $185 together. Watch their preview videos before paying. You are the one who can judge whether that guard is the guard you want.

While you look, I can build the system on 100STYLE. It is free and may sit in a public repository. It has what a matcher needs to be proven: one actor, every direction, runs, and the changes between them.

Our own capture comes last. It is the only way to get precisely our guard, but cheap capture is weakest at the feet, and feet are what walking is.

## What fits this project

Keep everything you like: the rebuild from parts, the sliders, the physical body. Change two things underneath them: what the walk is made from, and who moves the body.

*(A drawing in the live document. Today: the fight decides where he should go; code moves his body (step, stand, step, at about 0.75 m/s); four loops are blended and stretched (slowed to 0.85, strides cut to 0.78); he is rebuilt from his parts with the sliders; feet locked, then the physical body (hips carried by the pose, legs hung from them). Proposed: the same steering; a search picks the next recorded moment; the recording moves his body, pulled gently onto the path; the same rebuild and sliders; feet locked, a look layer, the physical body, with small corrections only.)*

### Three ways to go

|  | A. New footage, same system | B. Motion matching, the footage leads | C. A physical body that walks |
| --- | --- | --- | --- |
| What changes | One coherent set replaces the loops. Starts and stops are added by hand-written rules. | A search picks the next recorded moment. The body travels as the performer's did. | A simulated body follows B with its own legs. |
| What it cures | The mixed bodies and the slowed walk | Five of the six causes | The sixth: weight through the legs |
| What it costs | Little | A new heart for the walk; the parts around it stay | Research and machine learning; months |
| Where it can fail | A rule for every start, stop and turn, in every direction | A gap in the footage shows as an odd choice | It may not work out |
| Verdict | Not enough | Do this | Later, on top of B, as its own plan |

### How B keeps your rules

- **No recording is played as it is.** The search only chooses which recorded moment comes next. That moment is still taken apart and rebuilt, part against part. Your sliders still work on it: bounce, sway, hip turn, chest turn, arm swing, step lift, lean, crouch.
- **The guard's arms stay the game's.** The shield is held in cover and the sword arm makes the gestures, as now. The walk supplies the legs, the hips and the spine.
- **Blows on the move still ride the walk.** They get truer footfalls to ride on.
- **The physical body stays.** It follows a pose that now moves like a body.
- **One step at a time.** Each step sits behind a Combat Feel switch, with an old and new toggle in a lab.
- **It can be tested without Unity.** The search is plain code, and the walk's report card in the plan below reads its result.

### What retires and what stays

The stepping pacer, the blend of four loops and the floor on tempo retire from the matched walk. The inertial blend, the foot locks, the small corrections to stride and leg direction, the turns on the spot, the levers and the stumble steps all stay. Nothing is deleted: the old walk remains behind its switch.

### How your asks come out of it

- **Walking round each other, looking at each other.** The footage is recorded facing forward while the legs go in any direction. Where his man stands becomes part of what the search asks for. A look layer turns eyes, head and chest. A recorded turn on the spot squares his feet.
- **Every step weighted.** The footage plays at its own pace, on its own stride, from one performer, with recorded starts and stops. That removes the first five causes.
- **Slow walk, strut, run, sprint, without a seam.** Each gait is footage in the same library, and he moves between them through recorded changes of speed. A style such as a strut is footage of its own, or your sliders on the plain walk. 100STYLE holds a hundred styles to try that with.
- **Nuance.** With minutes of footage in place of one stride, his steps differ as the performer's did.

## The plan: seven steps

The first three steps need neither Unity nor money, and I can start them on your go. You look for the first time at step 3. Each step sits behind a Combat Feel switch, with an old and new toggle in a lab. Each ends with a build-log entry, a push and a film. Nothing here is built yet.

| Step | What is built | What you see | Passes when (headless) |
| --- | --- | --- | --- |
| 0. The report card | The measures of this document in every headless report and in your playtest log: one `gait` line a fighter. A strip of stick figures, so that I look before you do. | Nothing new in the game. A lab button "Walk beside the recording" shows a raw recording next to the fighter. | The card repeats today's numbers on today's walk. |
| 1. The footage in | A baker for the chosen set. A new library beside the old ones. Every moment tagged with which foot is down. A catalogue of what the dance card has and lacks. | Nothing yet. You get the catalogue. | One performer: hips within 3 cm across the walks of one stance. Every gap is listed. |
| 2. The search | `MotionMatch.cs`: what is compared, the search, the jump through the inertial blend we already have, and the body moved by the recording. A bare skeleton walks the footwork course. | Nothing yet. You get pictures and the card. | The report card below, on a bare walker. |
| 3. One man walking | Combat Feel "He Walks By Matched Footage". Walk lab: "His walk: matched (new) / blended loops (old)", "Send him: stroll, walk, jog, run, sprint" and "Stop him". No guard and no fight. | **Your first look.** Does one man, alone, walk like a person? | The card, in your Unity log too. |
| 4. Round his man | Where his man stands joins the search. The look layer for eyes, head and chest. A recorded turn on the spot when his feet are more than 45 degrees off. Duel lab: "Circle him". | **Second look.** Two men walking round each other, facing. | Chest within 10 degrees of his man for 95% of the circling. |
| 5. Into the fight | The guard's arms on the matched legs. Blows on the move ride the new footfalls. Speed is carried across every join: stumbles, get-ups, leaps. Dance: "Walk (dance): matched / loops". | **Third look.** The dance. | Both budgets kept. Speed jumps only at blows. Blows land as often as before, five throws each way. |
| 6. Gaits and styles | Slow walk, strut, jog, run and sprint, and the recorded changes between them. The run at a man who is down and the pounce use recorded speeding up and a recorded stop. | **Fourth look.** | No pace outside 0.9 to 1.1 at any speed. |
| 7. A body that carries itself | Research: a simulated body with working legs follows the matched walk. Its own plan, once the rest is solid. | Later. | Later. |

Two small things ride along: step sounds on the true footfalls, and a lean into each speeding up and slowing down. The fight of six stays on the old walk until you say.

### The walk's report card

These are the pass marks. The middle column is today's walk.

| Measure | Today | Passes at |
| --- | --- | --- |
| Pace against the footage | 0.85 | 0.9 to 1.1 for 95% of his walking |
| Stride against the footage | 0.78, and under 0.8 for half his walking | 0.9 to 1.1 for 90% |
| Rise and fall of the hips at his speed | 6 to 8 cm at 0.7 m/s | Within 1.5 cm of the same performer's at that speed |
| Hips' speed swing within a step | 67 to 86% | No more than the footage's own |
| Starts and stops that are recorded ones | None | Nine in ten |
| Speed jumps over 0.5 m/s in one physics step, away from blows | 5 a minute on the course | None |
| Foot slide before the lock takes hold | Not measured yet | Under 2 cm a step |
| Where he ends | Exact: the code puts him there | Within 15 cm and 10 degrees of where he was sent |
| The two budgets, crossings and tremble | 0.07% and 4.8% in the fight of six | Kept |
| Cost of one search |  | Under half a millisecond, a few times a second |

### What I need from you, and when

- **Now:** your answers to the questions below.
- **Before step 4:** the bought guard set, in a private repository, if you choose to buy. Without it step 4 runs on the nearest footage in the free set and waits for the real one.
- **At each look:** the panel frames what to press, as always.

## Risks, and what is not known

The biggest unknown is that I have seen none of this. The diagnosis is a set of numbers that agree with your eye, and the packs are product pages I have read.

| Risk | Why it matters | What guards against it |
| --- | --- | --- |
| The diagnosis is headless | Unity has the last word. | Step 0 puts the same measures in your playtest log. |
| I have not seen the packs move | A product page flatters. Their guard may not be the guard you want. | You watch the previews before paying. Each pack's list of clips is checked against the dance card first. |
| The stance join | Our attacks come from Mixamo's sword-and-shield performer. A new performer's walk under them needs a join. | We already join walk to blow by an inertial blend. Later the attacks can move to the new performer too. |
| Positions become less exact | He lands within centimetres of where he was sent, not on the spot. Reach, the jump in and "he takes his distance again" were tuned on exact travel. | A pass mark of 15 cm, and a gentle pull onto the path. Step 5 measures blows landed. |
| Gaps in the footage | A missing move shows as an odd choice or a slide. | The catalogue of step 1 and the report card. A gap is filled by a second pack or a capture. |
| The repository is public | Bought files may not go into it. Mixamo's raw files are in it today. | Make it private before buying. Your call; I am not a lawyer. |
| The free set is a research capture | It was made in a suit, in a small room. Its feet will slide more than a studio's. | It proves the system. It is not the final look. |
| The physical upper body | A livelier spine may shake what the arms hold. | The two budgets are checked at every step. |
| Large and Small | A bigger body should walk at a slower pace. That is not designed here. | The footage is stored in leg lengths, so the rule can come later. |
| Step 7 may never pay | A learned physical walker is research. | It is last, optional and its own plan. |

Three more things you should know.

- Three web pages could not be opened without your approval: two pages about 100STYLE and an article on For Honor. I used the dataset's archive page and its paper instead.
- A physics check exists but is not trusted yet. `Tools/Headless/weight.py` asks whether the push from the ground could come from under his feet. It reads 0% outside for a standing recording, but 14 to 72% for walking recordings as performed. It needs calibrating before it can be a pass mark.
- The combos, the clothes and the shock wave are untouched by this plan. Their open items wait as they were.

## Questions for you

Six answers start the work. My recommendation is given with each.

1. **The direction.** Motion matching on one coherent set, with the recording moving the body (way B)? *Recommended: yes.*
2. **Start now on the free set?** Steps 0 to 2 need neither Unity nor money. *Recommended: yes.*
3. **The packs.** Will you watch the previews of [Dungeon Life](https://www.mocapcentral.com/resources/dungeon-life) and [Core Motion](https://mocapcentral.com/products/core-motion-pack), and say whether that guard and that walk are what you want? If they are, buy them, and make the repository private first. *Recommended: yes, about $185.*
4. **May the recording lead?** He would end within about 15 cm of where the code sent him, not exactly on the spot. *Recommended: yes.*
5. **Whose guard?** Keep our guard and carry the new steps on it, or take the pack's guard as his stance. *Recommended: decide once you have seen the previews. I lean to the pack's, so that his stand and his step are one performer's.*
6. **The body that carries itself (way C).** Keep it on the roadmap as research after the rest? *Recommended: yes, as its own plan, not now.*

One thing that would help and is optional: open Epic's sample in Unreal for an hour. It is free, and it shows the target better than any description.

## Sources

**Measured here**

- The game's own code, run without Unity on 26 November 2026: the footwork course (25 s) and a dance (54 s), release build, read with `Tools/Headless/gaitfilm.py`.
- Every walking loop in `LocoClips.json` and `CombatClips.json`, read with `Tools/Mocap/Locomotion/gait_survey.py`.
- The plan of 17 November, "Convincing walking in the dance", and the walk's own code: `Locomotor.cs`, `StepPacer.cs`, `MoveScore.cs`, `UpperBodyArticulation.cs`, `CombatFeel.cs`.

**How people walk**

- Orendurff and others, Journal of Rehabilitation Research and Development 41(6), 2004, pages 829 to 834: how far the centre of mass moves at each walking speed.
- [A table of typical adult walking values](https://pmc.ncbi.nlm.nih.gov/articles/PMC8217618/): speed, step length, time on both feet.
- Wu and others, 2019, Scientific Reports: [walking at very slow speeds](https://www.nature.com/articles/s41598-019-54271-2).
- Anand, Seipel and Rietdyk, 2017: [how walking starts](https://pmc.ncbi.nlm.nih.gov/articles/PMC5378148), and [a study of the shift of weight before the first step](https://pmc.ncbi.nlm.nih.gov/articles/PMC5015477).
- McCann, Pollard and Srinivasa, 2006: [Physics-Based Motion Retiming](https://www.ri.cmu.edu/publications/physics-based-motion-retiming).

**How games do it**

- Simon Clavet, [Motion Matching and The Road to Next-Gen Animation](https://www.gdcvault.com/play/1022985/Motion-Matching-and-The-Road), GDC 2016. Cited from its listing; not watched.
- Naughty Dog, [Motion Matching in The Last of Us Part II](https://gdcvault.com/play/1027118/Motion-Matching-in-The-Last), GDC 2021. Cited from its listing.
- O3DE, [Motion Matching](https://docs.o3de.org/blog/posts/blog-motionmatching/): what is compared, how often, and how much memory.
- Epic Games, [Pose Warping in Unreal Engine](https://dev.epicgames.com/documentation/unreal-engine/pose-warping-in-unreal-engine).
- Daniel Holden, [Code vs Data Driven Displacement](https://theorangeduck.com/page/code-vs-data-driven-displacement).
- Ubisoft La Forge, [DReCon](https://montreal.ubisoft.com/en/drecon-data-driven-responsive-control-of-physics-based-characters/), 2019.
- [MxM, Motion Matching for Unity](https://github.com/Frost-Blade-Studios/Motion-Matching-for-Unity) and [JLPM22's MotionMatching](https://github.com/JLPM22/MotionMatching).

**Where animations come from**

- Epic: [the sample's announcement](https://www.unrealengine.com/en-US/blog/game-animation-sample), its [December 2025](https://www.unrealengine.com/en-US/tech-blog/explore-the-updates-to-the-game-animation-sample-project-in-ue-5-7) and [August 2026](https://www.unrealengine.com/tech-blog/download-the-latest-game-animation-sample-project-now-updated-for-ue-5-8) updates, [the forum thread that quotes its licence label](https://forums.unrealengine.com/t/what-can-i-do-with-game-animation-sample-that-recently-was-at-100-off/1903980) and [Epic's content licence](https://www.unrealengine.com/en-US/eula/content).
- MoCap Central: [Dungeon Life](https://www.mocapcentral.com/resources/dungeon-life), [Core Motion](https://mocapcentral.com/products/core-motion-pack), [Longsword Vol. 2](https://mocapcentral.com/products/animation-pack-longsword-vol-2), and [Core Motion on the Unity Asset Store](https://assetstore.unity.com/packages/3d/animations/mc-core-motion-believable-3d-animations-by-mocap-central-289267).
- MoCap Online: [Mobility Pro](https://mocaponline.com/products/mobility) and [its licences](https://mocaponline.com/pages/licensing).
- Unity Asset Store: [Sword and shield animation set ADVANCED](https://marketplace.unity.com/packages/3d/animations/sword-and-shield-animation-set-advanced-158412) and [the store's terms](https://unity.com/legal/as-terms).
- [100STYLE on Zenodo](https://zenodo.org/record/8127870) and [its paper](https://arxiv.org/abs/2201.04439).
- [CMU Graphics Lab Motion Capture Database](https://mocap.cs.cmu.edu/).
- Adobe, [Mixamo FAQ](https://community.adobe.com/t5/mixamo-discussions/mixamo-faq-licensing-royalties-ownership-eula-and-tos/m-p/13234775).
- Motorica: [Game Developer's report](https://www.gamedeveloper.com/business/genai-animation-startup-motorica-raises-5-million-pounds-in-investment) and [a September 2026 guide to AI animation tools](https://app.cinevva.com/guides/ai-animation-generators-3d).
- Capture prices come from 2026 buyer's guides read for this document. Check them before buying.
