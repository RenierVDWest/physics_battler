# Movement Styles as Offshoots of the Standard Set

> Copy of the Claude document <https://claude.ai/artifact/AUmVKUKxhELrbbn3ZL8PJ4>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 15, 2026 · @Renier

## Bottom line

Yes, building styles as offshoots of the standard set is the right approach, with one amendment: only part of a style can be morphed. The rest has to be built.

A style is three things, and each needs its own treatment:

| Part of a style | What it is | Capoeira example | How it is made |
| --- | --- | --- | --- |
| **Manner** | How the body is carried and how much it moves, whatever it is doing | Chest leaned 30 to 40 degrees forward, hands up by the face, hips low | **Morphed**: measured as a difference from the standard set and laid over any standard motion |
| **Vocabulary** | Moves only this style has | The ginga's step pattern, the esquiva ducks | **Built**: as a pattern of the standard set's own steps and weight shifts, with sizes and timings measured from the recordings |
| **Rhythm** | The beat, how regular it is, how restless | A step every 0.6 to 0.9 s, never still | **Dialled**: measured ranges, with a restlessness dial and variation so no two cycles are alike |

The first pass failed because it did none of these. It played one recorded loop as the idle. That is why it repeats, why it moves too much, and why it has nothing to do with the standard set.

The measurements below support the approach. The sword and shield guard we already have turns out to be exactly "the plain walk plus a change of carriage". The capoeira carriage is the same across every clip in the pack. And all four recorded gingas fit one step pattern with a handful of numbers.

What you would get: a **style amount** slider in the duel lab that takes the fighter from Standard (0) to Capoeira (1), and a ginga that is generated step by step instead of replayed.

## What went wrong in the first pass

The first pass treated the ginga as an idle animation: one 3.4 s recording, played on a loop at two thirds of its size. Both things you saw follow directly from that.

- **It repeats** because it is literally the same 3.4 s played again and again. A recording has no way to vary.
- **It moves too much** because the recording is a performance. The dancer's hips travel 1.5 m from side to side at about 1 m/s, as fast as a walk. At two thirds that is still a metre of travel and 0.6 m/s, without ever going anywhere.
- **It is not connected to the standard set.** Nothing of the standard guard is in it, so nothing we tuned there (the planted feet, the steps, the weight over the feet) carries over. To walk, he drops the style entirely and becomes the standard fighter.
- **The pack cannot fill the gaps.** It has no walking, circling or backing-away loops in the style. Its travelling clips are single steps with turns in them. So "play the recordings" has no answer for most of what a fighter does.

The lesson: a recording is good **evidence** of a style and poor **material** for one. We should measure it, not play it.

## What is known

The idea of a style as a change applied to a neutral motion is old and well tested, and the research also says clearly where it stops working. Both halves matter for us.

**Where it works: the same action, done differently.**

| Work | What it showed |
| --- | --- |
| [Emotion from Motion](https://graphicsinterface.org/proceedings/gi1996/gi1996-26) (Amaya, Bruderlin, Calvert, 1996) | An "emotional" motion differs from a neutral one mainly in two things: timing and range. Measure those two on one movement and you can apply them to others. |
| [Fourier Principles for Emotion-based Human Figure Animation](https://history.siggraph.org/?p=118723) (Unuma, Anjyo, Takeuchi, 1995) | A quality like "briskness" was taken from the difference between a normal and a brisk walk and then applied to a run. |
| [Style Translation for Human Motion](https://people.csail.mit.edu/ehsu/work/sig05stf) (Hsu, Pulli, Popović, 2005) | Learns a style from pairs of performances "of the same content" in two styles, then translates new motion with simple operations each frame. It needs the paired examples. |
| [Real-Time Style Modelling of Human Locomotion](https://arxiv.org/abs/2201.04439) (Mason, Starke, Komura, 2022) | 100 styles, over 4 million frames, all of it locomotion. Even the largest modern style data is "the same walk, a hundred ways". |

**Where it stops: when the style changes what the feet do.**

| Work | What it admits |
| --- | --- |
| [Unpaired Motion Style Transfer from Video to Animation](https://ar5iv.labs.arxiv.org/html/2005.05751) (Aberman and others, 2020) | The method "assumes that the foot contact timing is part of the content, and not of the style", and says this "is not always true", giving a zombie dragging a foot as the example. |
| [Spectral Style Transfer between Independent Actions](https://geometry.cs.ucl.ac.uk/projects/2016/spectral-style-transfer) (Yumer and Mitra, 2016) | Shows an earlier method turning kicks into something like walks when its data had no kicks in that style. A move the style data does not cover cannot be conjured from the style alone. |

The ginga is exactly the second case. It is not the standard stand done differently: it is a different pattern of steps. That is why this plan splits a style into manner (morphed) and vocabulary (built).

**What the ginga is, in capoeira's own terms.** It is a "constant triangular" footwork whose purpose is to keep the player moving, hard to hit and ready to dodge, feint or attack, and its speed follows the music. Players are told "no one has the same ginga". ([Ginga, Wikipedia](<https://en.wikipedia.org/wiki/Ginga_(capoeira)>)) So variation and tempo are part of the style itself, not extras.

**Game practice.** Small teams get fluid, responsive characters from "very few key frames" plus simple procedural rules, rather than from large libraries of clips ([An Indie Approach to Procedural Animation](https://gdcvault.com/play/1020049/Animation-Bootcamp-An-Indie-Approach), David Rosen, GDC 2014). That is the same bet as ours: a small solid base, and rules on top.

The modern machine-learning methods are not an option for us: they need thousands of clips per style and give no dials. We have 39 clips and want dials.

## What our own recordings say

I measured the standard set and the capoeira pack with the same ruler (a new tool, `Tools/Mocap/Style/style_measure.py`). Three findings, each of which the plan rests on.

### Finding 1: the guard we already have is "the plain walk plus a change of carriage"

This is the approach working, on a style we already own. The sword and shield walk and the plain walk have the same legs. Only the way the body is carried differs.

| Measure | Plain walk | Sword and shield walk |
| --- | --- | --- |
| Hips' height (1.00 = standing straight) | 0.94 | 0.94 |
| Hips' range side to side / up and down (m) | 0.06 / 0.08 | 0.06 / 0.07 |
| Chest leaned forward (degrees) | 1 | 9 |
| Hips turned off the way he faces (degrees) | 8 | 22 |
| Hands below the chest (m) | 0.41 | 0.13 to 0.20 |
| Hands' speed (m/s) | 1.4 to 1.5 | 0.3 to 0.4 |

The legs match to the centimetre. The upper body is leaned, turned and held up instead of swinging. That is manner, and nothing else.

### Finding 2: the capoeira manner is a stable signature

Across six different capoeira clips (four gingas on the spot, one travelling forward, one back) the carriage barely changes. It can be measured from a few clips and trusted.

| Measure | Standard guard | Capoeira (six clips) |
| --- | --- | --- |
| Chest leaned forward (degrees) | 7 to 14 | 30 to 40 |
| Hips' height (1.00 = standing straight) | 0.82 to 0.94 | 0.75 to 0.83 |
| Hands below the chest (m) | 0.13 to 0.25 | 0.06 to 0.22 |
| Hands' speed, at the middle (m/s) | 0.05 standing, 0.3 walking | 0.8 to 2.6 |
| Hips' speed standing (m/s) | 0.01 | 0.9 |

So the capoeira manner, in plain words: lean well forward, sit a little lower, keep the hands up and busy.

### Finding 3: all four recorded gingas are one step pattern

Each take puts its feet down in the same four places, in the same order. Only the sizes and the timing differ. The picture below shows the pattern; the table gives each take's numbers.

&#91;embedded content: the ginga seen from above · four places, four steps (plain take, to scale)\]

| Take | Loop (s) | Front feet apart (m) | Back step: behind / outside the other front foot (m) | Hips' sway (m) | Hips lowest to highest | Lean (degrees) |
| --- | --- | --- | --- | --- | --- | --- |
| Plain ("capoeira") | 3.4 | 1.34 | 0.93 / 0.36 | 1.5 | 0.74 to 0.90 | 30 |
| Quieter (variation 3) | 2.9 | 0.91 | 0.96 / 0.22 | 1.0 | 0.76 to 0.86 | 34 |
| Lower, turning (variation 1) | 3.5 | 1.13 | 0.91 / 0.41 | 1.4 | 0.64 to 0.85 | 39 |
| Lower (variation 2) | 3.8 | 1.11 | 1.03 / 0.26 | 1.4 | 0.66 to 0.84 | 40 |

The ginga is therefore not an animation to be played. It is a rule: "stand on two front places; step one foot back and across behind the other, shift over it, bring it forward again; then the other side". Seven or eight numbers describe a take, and the spread between takes tells us how much it may vary.

### What the pack holds

Of its 39 clips, only 11 are the base (6 gingas on the spot, 5 travelling). 6 are evasions, 19 are kicks and sweeps, 3 are acrobatics. So the style's manner has to be read off about a quarter of the pack, and everything a fighter does most (walking, circling, backing away, striking with a sword) is not in the pack at all. It has to come from the standard set.

## The proposed method

A style becomes a small profile of numbers and a few rules, applied to the standard machinery. Nothing of the style is played from a recording.

&#91;embedded content: how a style sits on the standard set · profile, pattern runner, manner layer\]

### The manner layer (morphed)

The standard set poses the body as it does today. Then one extra layer changes how that body is carried.

- It works on the same body parts we already split attacks into: hips, chest, head, each arm. The feet stay exactly where the standard put them, so planted feet stay planted and the knees bend to follow the hips.
- **Carriage** is added: lean the chest this much, sit the hips this much lower, hold the hands here.
- **Motion** is scaled: how far the hips sway with each step, how busy the arms are.
- One **style amount** runs through all of it. 0 is the standard fighter, 1 is the full style, and anything between is a blend. This is the "morphing" you described, as a slider.

Because it sits on top of whatever the standard is doing, the fighter keeps the style while walking, jogging, circling, leaping and striking on the move. The first pass lost the style the moment he took a step.

### The vocabulary (built)

Moves that only the style has are written as patterns that drive the standard machinery, not as clips.

- **The ginga** becomes a step rule over four places: stand on the two front places, step one foot back and across, shift the weight over it, bring it forward, change sides. The standard stepping system takes the steps, so the feet plant and the weight moves as they already do.
- **It needs one new standard part: the long step.** Today the standard set has short steps (0.3 to 0.45 m) and leaps (both feet off the ground). The ginga's steps are long sweeps of 0.9 to 1.8 m with one foot planted. That step will be made the way the leap was, from recordings, and added to the standard set, so every later style can use it.
- **Moves with no standard equivalent** (the deep esquiva ducks, hips down to 0.4 of standing) are made from their recording as a difference from the standard guard, the same way attacks are made.

### The rhythm (dialled)

- **Tempo**: the beat of the steps, measured from the pack (a step every 0.6 to 0.9 s) and adjustable.
- **Restlessness**: how much of the time he is stepping rather than settled. At 0 he stands in the style's stance and only sways. At 1 he never stops, as in the recordings. This is the dial for "moving too much".
- **Variation**: each step's size and timing is drawn from the range measured across the four takes, so no two cycles are alike. This is the fix for "repeating the same thing".
- **It answers the fight**: the four places move with where he wants to go, so the ginga itself travels and circles. Later the dance can raise the restlessness in range and lower it out of range.

### Why the standard set is the right base

The standard guard's feet stand 0.80 to 0.86 m apart. The quieter ginga's two front places are 0.91 m apart. So the ginga's resting stance is very nearly the standard stance, squared up and leaned forward. The style really is an offshoot: the same stance, a different carriage, plus one stepping habit.

## How a style is learned from a pack

This is the repeatable part, and the part that becomes the skill. Each stage ends in something that can be checked before the next one starts.

| Stage | What happens | What comes out |
| --- | --- | --- |
| 1. Survey | Open every clip in the pack and sort it: standing base, travelling base, evasions, strikes, acrobatics. | An inventory. For capoeira: 11 base clips, 6 evasions, 19 kicks and sweeps, 3 acrobatics. |
| 2. Pair | For each base clip, name the standard clip that does the same job (stand, walk forward, walk back, circle). | Pairs to compare. Clips with no partner are vocabulary. |
| 3. Measure the manner | Run the measuring tool on each pair. Take the difference: lean, hips height, hand places, how much the hips and hands move. | The carriage and motion numbers, with the spread across clips. |
| 4. Find the patterns | For the vocabulary clips, pull out the foot places, the order the feet visit them, the timing, and how the hips travel between them. Check that several takes fit the same pattern. | A step rule with ranges. For the ginga: four places, four steps a cycle, sizes and timings from four takes. |
| 5. Write the profile | Put stages 3 and 4 into one small data file: the style's numbers and the names of its patterns. | The style profile. Readable, hand-tunable, a page long. |
| 6. Build | Feed the profile to the manner layer and the pattern runner. No new code if the style only needs parts that exist; one new standard part if it needs a new kind of step. | The style, running in the duel lab behind a switch. |
| 7. Score | Measure the generated motion with the same tool and lay it next to the recordings. | A scorecard: where it matches, where it does not. |
| 8. Look | You watch it in the lab, against the recording, with the dials. | Your verdict. Numbers cannot replace this one. |

Stages 1 to 4 are already done once for capoeira, by hand, while writing this plan. The measuring tool exists (`Tools/Mocap/Style/style_measure.py`). Stages 5 to 7 are what the steps below build.

**What goes into a profile.** Roughly thirty numbers and a short list:

- Carriage: chest lean, hips height, hips turned off the facing, where each hand is held against the chest, head tilt.
- Motion: how far the hips sway and bob per step, how much the chest counter-turns, how busy the hands are.
- Rhythm: steps per second, how long a foot stays down, restlessness, variation.
- Vocabulary: the named patterns (the ginga's four places with their sizes) and the named made moves (the esquivas).
- Limits: how low the hips may go over standard legs, how far the lean may go with a sword in hand.

## How we will know it works

The plan stands or falls on one claim: a style can be taken off a recording as numbers and put back onto the standard set. That claim can be tested before any capoeira is built.

| Test | What it does | Passes when |
| --- | --- | --- |
| **T0. Round trip on a style we already own** | Take the plain walk. Lay the sword-and-shield guard's manner over it. Compare with the recorded sword-and-shield walk, which the layer never saw. | Carriage numbers within the spread between recorded takes. Feet do not slide. You cannot easily tell which is which in the lab. |
| **T1. Capoeira manner over the standard set** | Stand, walk, jog and circle with the style amount at 1. | Lean, hips height and hand places land in the measured capoeira ranges. Planted feet stay planted. Knees never lock or fold wrong. |
| **T2. The generated ginga** | Run the ginga pattern for a minute and measure it like a recording. | Foot places, step lengths, timing and hips travel fall inside the ranges of the four takes. No two cycles are the same. |
| **T3. Held-out check** | Make the ginga travel forward and back. Compare with the pack's ginga forward and backward clips, which were not used to fit anything. | Pace and step pattern in the same range as the recordings (forward is 1.44 m/s). |
| **T4. The slider** | Sweep the style amount from 0 to 1 while he stands, walks and fights. | No pops, no foot slide at any setting. 0 is exactly today's standard fighter. |
| **T5. Nothing else moved** | The standing regression fights. | 78 and 84 blows, as today, with the style off. |

**Why T0 matters most.** It is the cheapest honest test of the whole idea. We have both ends: the plain walk and the guarded walk were recorded separately, and the measurements above already suggest one is the other plus a carriage. If the round trip fails there, where the two styles are close, it will not work for capoeira, and we will know after one step instead of five.

**What only you can judge.** Numbers can say the hips are at the right height and the feet land in the right places. They cannot say it reads as capoeira, or that it has stopped looking like a loop. Every step therefore ends with a lab scene: the recording on one side, the generated version on the other, and the dials.

## Alternatives considered, risks and limits

### Other ways to do it

| Approach | Why not |
| --- | --- |
| Play the pack's recordings (the first pass) | The style lives in the clip, so it is lost on the first step, repeats exactly, and cannot be dialled. This is what you rejected. |
| Play all four ginga takes at random | Hides the repeat for a while. Still too busy, still cannot travel or turn, still nothing while walking. |
| Learned style transfer (the research systems) | They need thousands of clips per style and a trained network. We have 39 clips and one performer. They also give no dials, and their output still needs foot cleanup. |
| Motion matching inside the pack | Needs minutes of continuous varied movement to search through. The pack is short separate takes. |
| Write the style by hand with no reference | Possible, but then nothing says when it is right. The recordings are the ruler. |
| **Standard base + measured manner + built vocabulary (this plan)** | Uses what the project already does well: body tracks, layering, made steps, foot locks. Every number comes from a recording and every number is a dial. |

### Risks, stated plainly

- **It may look like a standard fighter with a filter on.** Carriage alone will not sell capoeira. The research says the same: adding an offset moves the pose but not the timing. That is why the ginga is built as vocabulary and the rhythm is its own part. If the manner layer alone looks thin, that is expected, not a failure.
- **Deep lean and crouch over standard legs.** Capoeira leans 30 to 40 degrees and sits 10 to 20 percent lower. Standard steps under that body may reach or stub. The knees follow the hips already, but long legs at a low hip height is new ground. The slider lets us find where it breaks.
- **The long step is new machinery.** The ginga cannot be built from today's short steps. This is the largest single piece of work in the plan, and the one most likely to take more than one pass.
- **The arms were recorded empty-handed.** The pack's arms swing freely across the body. Ours hold a sword. The first pass already needed a wrist rule for this. The arms will be a style-inspired guard, not a copy.
- **The recordings are a performance.** They are big and constant because the performer was showing the style. A fighter waiting for an opening would be quieter. The restlessness dial exists for this, and its default is a judgement call that is yours.
- **One performer.** The numbers describe one person's ginga. The spread across four takes is real but narrow.
- **Some moves will not transfer.** Cartwheels, handstands and floor moves have no standard equivalent and are out of scope for now. They could later be played as recordings, entered and left through the existing blending, as the leaps are.

### What would make me stop and rethink

- T0 fails: the guard cannot be rebuilt from the plain walk. Then the manner is not separable in our track space, and the layer needs a different design before anything else.
- The generated ginga passes its numbers but you still read it as mechanical after the variation and restlessness dials. Then the missing ingredient is in the timing between body parts, and the next thing to try is taking that timing from the recordings rather than from rules.

## The steps

One step at a time, each behind its own Combat Feel switch with controls in the duel lab, as before. Each ends with something you can look at, and a number that says whether it is done.

| Step | What is built | What you will see in the lab | Done when |
| --- | --- | --- | --- |
| **S0. Measure and profile** | The measuring tool finished into a scorecard. The style profile file, with the standard guard and capoeira written out. | Nothing moves yet. A printed comparison of any generated motion against the pack. | The profile for the guard reproduces the numbers in this document. |
| **S1. The manner layer, proven on the guard** | The layer that lays a carriage over a standard pose. Test T0. | A toggle on the walking fighter: recorded sword-and-shield walk / plain walk with the guard's manner laid on. | T0 passes, and you find the two hard to tell apart. |
| **S2. Capoeira's manner over the standard set** | The capoeira profile's carriage and motion, with the style amount slider. | A slider from Standard to Capoeira while he stands, walks, jogs and circles. He keeps the style on the move. | T1 and T4 pass. |
| **S3. The long step** | A new standard part: a long sweeping step with one foot planted, made from recordings as the leap was. | A button: take a long step back-across, forward-across, to the side. | Foot lands within a few centimetres of the asked place; planted foot does not slide. |
| **S4. The ginga as a pattern** | The four-place step rule, with tempo, restlessness and variation dials. | Old / new toggle: the recorded loop against the generated ginga. Dials for width, tempo, restlessness. | T2 passes, and you no longer read it as a loop or as too busy. |
| **S5. The ginga travels and turns** | The four places move with where he wants to go. | Orders in the lab: advance, retreat, circle, all while in the ginga. | T3 passes. |
| **S6. Arms and sword in the style** | The style's arm carriage with a sword in hand, kept clear of the body. | The arm dial; sword never through the body or the ground. | Blade clearances hold through a minute of ginga. |
| **S7. Attacks out of the ginga** | The made attacks thrown from the ginga, using the on-the-move layering. | The strike buttons while he is in the ginga. | Blade lands as close as on the standard walk today. |
| **S8. The esquivas** | The ducks as made dodges, as differences from the stance. | A dodge button with an incoming blow. | The head clears the blade line by a set margin. |
| **S9. The skill, proven on a second style** | The procedure written down, then run from scratch on another pack. | A second style in the lab, built by following the skill. | The second style reaches its lab scene without new machinery. |

**Order and risk.** S1 is first because it tests the idea cheaply. S3 is the biggest unknown and comes before the ginga depends on it. S2 gives you something capoeira-like to look at early, before the hard part.

**What stays.** The first-pass ginga remains in the lab as the "old" side of the toggle until S4 is accepted, then it is removed. Attacks on the move stay the default for moving fighters throughout.

## The skill

You asked for this to be written down as a skill, so a later style can be built the same way. The skill is the pipeline above, in a form that can be followed cold.

**What it will hold**

- **When to use it:** a new animation pack and the wish to turn it into a fighting style on top of the standard set.
- **What it needs:** the pack, baked into our sample format; a working standard set; the measuring tools.
- **The procedure:** the eight stages, each with the exact command, what to look at in its output, and what counts as good enough to move on.
- **The profile format:** every field, what it means in plain words, and the range that is sensible.
- **The tests:** the round trip, the scorecard, the slider sweep, the regression fights, and the lab scene for the user's eye.
- **The pitfalls:** what went wrong on capoeira and how it showed. The first entry is already known: do not play the pack's loop as the idle.
- **What it cannot do:** styles whose base is not on two feet, and moves with no standard equivalent.

**When it gets written.** In two passes. A first draft after S4, when the ginga is accepted and the pipeline has worked once end to end. The final version after S9, corrected by whatever the second style exposes. A skill written before the method has worked would be a guess; one written after a single style would be capoeira-shaped. The second style is what makes it general.

**A good second style.** One that differs from capoeira in kind, so the skill is tested rather than repeated. A heavy two-handed style would do it: low tempo, big weight shifts, almost no restlessness. That is the opposite corner from capoeira on every dial.

## Decisions for you

Nothing is built until you have read this. These are the choices that are yours; each has my recommendation first.

1. **The split.** Morph the manner, build the vocabulary, dial the rhythm, rather than morphing everything. *Recommended: yes.* It is your idea with one amendment: the ginga's step pattern is built from standard steps, because a morph cannot invent steps the standard does not take.
2. **A generated ginga instead of the recorded one.** The recordings become the ruler, not the animation. *Recommended: yes.* It is the only route that fixes both of your complaints.
3. **Prove it on the guard first (S1).** One step spent on a style we already have before any capoeira. *Recommended: yes.* If the idea is wrong, this is where it shows, cheaply.
4. **How restless the ginga is by default.** The recordings never stop. *Recommended: start well below the recordings, around a third, and let you set it in the lab.* Later the dance can raise it in range.
5. **The long step joins the standard set.** *Recommended: yes.* Other styles will want it, and the standard fighter could use it to close distance.
6. **Acrobatics.** Cartwheels and floor moves. *Recommended: leave out for now.*
7. **The second style for the skill.** *Recommended: a heavy two-handed style.* If you have a pack in mind, name it and I will plan around it.

If you agree with all seven, say so and I start with S0 and S1.

## Progress: 15 November, the plan agreed; S0 and S1 built

**Decisions.** All seven agreed as recommended. Two rules were added by you and now stand over the whole plan:

- Nothing is played as a plain recording any more. Everything is rebuilt the way the attacks were.
- Making the standard set better always comes first, because everything else is built on it. Whatever can be rolled into the standard set is.

The second style (S9) will be the Great Sword pack you sent (51 clips). The two spinning attacks are out of the lab's list for now.

**S0, the profile: done.** A style's manner is now a file of numbers (`StyleProfiles.json`), written by the measuring tool and read by the game. It holds six styles: plain, guard, sword, heavy, swagger and capoeira's stand. Its numbers agree with the tables above.

**S1, the manner layer and the round trip: built, and it passes in numbers.** The plain walk with the guard's manner laid on, against the guard's own recording, at the same moments of the stride (cm apart):

| Walking forward | Hips | Chest | Head | Elbows | Hands | Feet |
| --- | --- | --- | --- | --- | --- | --- |
| The plain walk as it is | 1.9 | 4.7 | 11.6 | 20.9 | 41.2 | 7.0 |
| With the guard's carriage | 1.8 | 2.5 | 4.8 | 9.1 | 15.2 | 7.0 |
| With its carriage and its motion | 1.7 | 1.7 | 2.6 | 2.9 | 4.1 | 7.0 |

Walking back, the sword-alone walk and the swagger come out the same way (hands 3.6 to 5.0 cm). So the upper body of a style we own can be made again from the plain walk and about sixty numbers, to within 2 to 5 cm.

**What is waiting for your eye.** In the duel lab (standard style): "His walk: recorded / rebuilt", with levers for carriage and motion. Walk him to the other and back away, and switch as he goes. The lab also prints which clip he is walking on, so the two cannot be mistaken.

**Corrections to this plan, from doing it**

- **A correction to Finding 1.** I wrote that the legs match to the centimetre. The hips' height and range do. The feet are 7 cm apart on average: the guard's stride is 4 cm longer and timed a little differently. The layer leaves the legs alone, so that gap stays.
- **The spine's bend is part of a carriage.** The tracks say how the hips, chest and head are turned, but not how the back curves between them. Without it the chest was 3 cm off and the head 5. It is now in the profile. The ginga's back is bent about twice as far as the guard's, so capoeira needs it more.
- **A carriage belongs to its job.** The guard stands very differently from how it walks (hips turned 41 degrees and sitting low, against upright). Its standing carriage laid on the walk is 14 cm off at the hips. So each job is measured on its own recording, and a pack that lacks a job gets the nearest one, which is a real limit. For capoeira this means the travelling gingas must be measured for the walking jobs, not only the gingas on the spot (S2).
- **Where the feet stand is not manner.** The guard's step to the side is a different step from the plain walk's (feet 21 to 28 cm apart from it). So stance and steps sit with the vocabulary, as the plan already says for the ginga. Only the walk forward and back are rebuilt in the test.
- **The run's arms are not got this way** (18 cm off): the guard pumps one arm and holds the other still, which one number per arm cannot say. Noted for later; the jog does not use them.
- **"About thirty numbers" was low.** A carriage is about sixty per job.

**Next:** S2, capoeira's manner over the standard set with the style amount slider, once you have looked at S1.

## Progress: 15 November, S1 seen in Unity; S2 built

**S1 in Unity.** You could not tell the recorded walk from the rebuilt one. One limit of that look, found afterwards: a fighter holding a sword or shield has his arms placed by the sword's and shield's own guard, on top of whatever the walk does. So the arms you saw were the same arms in both. Your look confirmed the trunk, head and legs. The arms are confirmed in numbers only.

**S2, capoeira's manner over the standard set: built.** One lever, the style amount, from 0 (the standard fighter, untouched) to 1. He does everything the standard does and is carried as capoeira carries the body.

|  | Standard | Amount 1 | The capoeira recordings |
| --- | --- | --- | --- |
| Lean (degrees) | 11 to 14 | 35 to 38 | 30 to 41 |
| Hips, standing / walking (m) | 0.83 / 0.88 | 0.76 / 0.78 | in the pack's range |
| Planted feet | as placed | exactly the same |  |

Tests T1 and T4 pass in numbers: the feet do not move, the knees never straighten or fold wrong, and sweeping the lever through a duel and through a fight makes nothing jump. With the lever at 0 the regression fights are unchanged.

**Blows with the manner on** land about as well as the standard's. From a stand the blade met the chest four times in six (the standard: two in six). On the move it is much the same as the standard.

**What is waiting for your eye.** Duel lab, standard style: the lever "Capoeira manner", with "Squares up", "Livelier", "Kept in blows" and "Eyes up" under it. Slide it while he stands, walks, circles, jogs, leaps and strikes.

**What it will not show yet**

- His arms. They stay the standard guard's, for the reason above. That is step S6.
- His stance. He stands in the guard's stance, one foot forward. Capoeira stands square and steps. That is the ginga, S3 and S4.

**Rules the layer needed** (each found by something going wrong)

- The hips are never turned by a manner. How they sit on the feet belongs to the stance.
- The lean goes toward where he faces, not where his hips point. Otherwise he leaned 41 degrees off his opponent.
- The carriage follows the job the legs are doing, switched at the same moment the walk switches. Lagging behind made the head jump.
- A blow from a stand is fitted for the body as the manner will carry it. Without that the blade was 25 to 33 cm off.
- He comes half way up to strike. Fully leaned, a blow at the chest has to go upward and gets stopped by the opponent's sword hand.

**Corrections**

- In the last note I wrote that the ginga's back is bent about twice as far as the guard's. That was wrong. On average it is bent about the same; it moves more.
- The capoeira profile now also holds the carriage travelling forward and back, measured on two recordings that are never played. It is the same as standing, as Finding 2 said.

**Next:** S3, the long step, once you have looked at S2.

## Progress: 15 November, S2 seen in Unity; S3 and S4 built

**S2 in Unity.** No bugs seen, and it looks bad. That was to be expected of a posture alone on the standard stance and arms. The look is to be judged once the footwork and the arms are there with it.

**S3 and S4, the long step and the ginga made of its pattern: built.** Nothing of a recording is played any more.

- He rests with one foot back, the hips over the front foot.
- To change sides the back foot sweeps forward and the other sweeps back behind it. Those are two long steps, and the standing foot is not moved at all.
- The hips shuttle across under him, then he rests and changes back.
- No two changes are quite alike: where the feet land, how long he rests and how quick the steps are all vary.
- He walks, jogs and strikes as the standard does, carried in capoeira's manner.
- The sword stays in its guard throughout. Only the free arm does the ginga's arm work.

|  | As set | At the recordings' settings | The recordings |
| --- | --- | --- | --- |
| A step goes (m) | 1.05 to 1.16 | 1.34 to 1.49 | 1.41 to 2.16 |
| and takes (s) | 0.42 to 0.50 | 0.52 to 0.63 | 0.50 to 0.83 |
| Hips, side to side (m) | 0.88 | 1.11 | 1.0 to 1.5 |
| Standing foot slides (cm a step) | 0.0 | 0.0 | 0 to 20 |

Test T2 passes in numbers. The first pass's looped recording slid its standing foot 2.8 cm a step.

**What is waiting for your eye.** Duel lab, style "Capoeira". "Ginga: made / recorded" switches between the new one and the first pass. Under it are four levers (Restless, Size, Tempo, Varies) and a button, "Change sides now". Turn Restless right down and press the button to look at one change at a time.

**A change to the plan.** S3 promised its own lab buttons for a long step by the standard fighter. I did not build them. A single long passing step leaves the standard fighter with the wrong foot forward, and the guard has no stance for that. The long step exists as a standard part and the ginga uses it. A use for it in the standard set (closing distance) is better decided when we get to the dance.

**Known and not done**

- Every change of sides is the same two steps. Only size and timing vary.
- The back foot stands flat. In the recordings it is on its ball.
- Asked to come closer slowly, he re-lays the pattern with short quick steps. A ginga that travels properly is S5.
- The sword arm does not sway with the ginga. That is S6.

**Next:** S5, the ginga travels and turns, once you have looked at this.

## Progress: S4 again, a change of approach (15 November)

The first way of S4 (a ginga generated from two rest poses and measured numbers) was rejected in Unity: it did not look like the recordings, the sword arm went through his body, and no check I had could have told me. Two changes, agreed with Renier.

**1. Nothing goes to Unity before it has been set beside the recording.** A new tool, `Tools/Mocap/Style/film_against.py`, takes a headless fight's film and, for every frame, the recording at the same moment, and gives a table (how far each part is from the recording's, his build taken out) and a strip of pictures (recording above, fighter below; front, side, above). This is now a standing part of the procedure and belongs in the skill.

**2. The ginga is made the way the attacks are made: from the recorded takes' own motion, edited.** Every part of the body at every moment is the recording's. Only these are changed: size, pace, the rests (he slows, is held and rocks a little at the moment the recorded man is stillest), which of the four takes he is in, and the feet (planted feet held; a swinging foot bent to leave from where it stood and land on its place).

**What the check found at once.** The body's own rules were being laid over the recording: the weight shift toward the standing foot put the hips up to 13 cm off the recording and sent them across at 3 m/s; the lift of a low moving foot raised the swinging foot a second time. Both are left out inside the made ginga. A rule for the skill: *a style's recorded motion already has its own weight shift and footwork; the standard body's helpers for these must be off wherever the recording is played.*

**Measured, empty hands** (cm from the recording's motion at the same moment): as recorded, head 1.0, hands 4.6, knees 3.2, feet 4.6, standing foot slides 0.0. As set (size 0.8, rests, takes changing): head 2.3, hands 4.9, knees 6.5, feet 6.8, slides 2.2 a step. Regression fights unchanged (78 and 84 blows).

**Open, in order.** The look in Unity (empty hands first). Then the sword arm in the ginga, as its own step with its own check (the blade must never be in his own body). Then S5: the ginga keeps its distance, travels and turns, and walking off from it starts from where the ginga has him (now a foot is carried up to 0.5 m as he leaves). Smaller: for one step after going over to another take his feet are still the old take's; the widest take at full size is wider than his legs reach.

## Shelved (16 November)

Renier has shelved the capoeira style and this plan with it. The last pass of the ginga played the recorded takes back, which is not the agreed method (motion is rebuilt from the recordings' parts, never played), and the pack asks more than the setup is ready for. Nothing here is deleted: the style's code, its profile file and the measuring tools stay in the project, unused, behind Combat Feel "The Capoeira Style Is Shelved".

What carried over to the standard style: taking a movement recording apart into its parts and putting the body together from them, which is now how the standard fighter stands, walks and jogs (the plan "Fight Rhythm from Recordings", step A), and the check that sets a fighter beside a recording.

The skill for making a style from a pack is not written. It waits until a style has been made that Renier accepts.
