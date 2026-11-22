# Cloth that moves: samurai clothes on the fighters, research and plan

21 November 2026. The live document: https://claude.ai/artifact/3Jev6uXAQk3Nsncq7GCtAh (this is its copy in the
repository; where the two differ, the live one is newer). **Nothing here is built: this is the research and the plan.**

Its film (3b2b, about five minutes): https://claude.ai/artifact/TtEraxUG6Bk2VKuR9n6uiu

## Bottom line

It can be done, and I recommend we write our own small cloth solver instead of using Unity's or buying one.

- **Why our own.** Our fighters are made of a few simple solid shapes, which is the easy case for cloth. And our own
  solver is the only kind I can run and measure without Unity, which is how everything else in this project gets made
  safely. Unity's cloth and the bought ones I would be building completely blind.
- **How it stays out of the body.** Six safeguards stacked on each other, not one. The main ones: the cloth is pushed
  out of each body part's own shape, it is worked out in many small steps so a fast arm cannot jump through it, and
  every point of cloth is on a short leash to its place on the body.
- **The first outfit is deliberately simple.** The pleated trousers as one wide pleated skirt (a real kind of hakama),
  sleeves tied back short, the chest covered by a snug shell that does not simulate. Then it grows towards the picture.
- **I make the clothes by code**, from flat rectangles, the way the real garments are cut. No modelling program, no
  files to import.
- **Six steps.** The first is one cloth skirt on the standing dummy in the lab, with numbers for how often it touches
  the body, how much it stretches, and what it costs.

## What you asked for

Your words, 21 November, 20:47 (with a picture of a mannequin in a kimono top, hakama and sash): "I want them to wear
clothes that are actual dynamic cloth. So you will need to model the clothes in a way that will work well in Unity. And
then I want you to implement them in Unity. This sounds like it can be a very tricky thing to get right because we cant
have the clothes intersecting with the bodies. Though you may want to disable the rendering on the body parts that are
completely covered in clothes. The clothes need to swing and sway realistically. The cloths itself can be based on
traditional samurai attire like this. It does not need to actually be totally faithful to that since the first version
should have a pretty conservative mesh to test it out." And: "before we do this [the shock waves] I want you to do
another research and planning document".

How I read it: real moving cloth; it must not go through the body (the hard part); covered body parts may be hidden;
samurai clothes, loosely, a simple safe first version; I make the garments and the code.

**The picture:** a patterned top with a crossed collar and very wide, square, hanging sleeves; wide pleated trousers
falling straight to the ankle, tied high at the waist; a sash knotted in front. The sleeves and the pleated legs are the
parts that swing; the chest and waist are held snug by the sash.

## The outfit

| Garment | What it is | What that means for us | Version 1 |
|---|---|---|---|
| The top (kimono, or a short haori jacket) | A wrapped-front garment with square sleeves and a rectangular body, cut from a long narrow bolt of cloth | It is rectangles. The sleeves are flat square bags hanging from the arm | A snug shell on the chest that does not simulate; two sleeves that do |
| The pleated trousers (hakama) | Seven deep pleats, five in front and two behind. Two kinds: divided like trousers (umanori), undivided like a long skirt (andon bakama). The two look alike | The undivided kind is one wide tube with the legs inside it | The undivided kind: one pleated skirt from the waist to the ankle |
| The sash and ties (obi, himo) | Tied round the waist; the hakama's straps wrap the body and knot in front | It is where the cloth is fastened to the body | A rigid ring on the hips. The top of the skirt is pinned to it |

These facts are from search summaries of the Wikipedia pages on the kimono and the hakama; **those pages could not be
opened** (the request needed his approval and he was away).

Two choices recommended for version 1: **the skirt kind of hakama** (with divided legs, cloth runs between the thighs
and every step and kick drags it), and **sleeves tied back** (with a cord; as far as I know a tasuki, from my own
knowledge, not looked up), which gives a short sleeve to the elbow, clear of the sword and the shield.

## Three ways to do cloth in Unity

| | Unity's own Cloth | A bought solver (Magica Cloth 2, Obi Cloth) | Our own solver |
|---|---|---|---|
| What it is | Built into Unity | Add-ons from the Asset Store | A few hundred lines of our own code |
| Works on | Only a Skinned Mesh Renderer. Our fighters are rigid parts, not a skinned mesh | Mesh cloth and bone cloth | Whatever we give it |
| Collides with | Only spheres, capsules and cone-capsules listed by hand. It does not push back | Spheres and capsules (Magica); all standard Unity colliders (Obi) | Our body parts' own shapes: capsules, boxes, spheres |
| Cloth against itself | Yes, by points; the manual warns it can take a large share of the time | Yes | Not in the first version |
| Set up by | Painting limits on the points in the editor | Editor tools | Code |
| Needs | Nothing | Unity's Burst, Jobs, Collections and Mathematics packages; money | Nothing |
| Can I run and measure it without Unity? | **No** | **No** | **Yes** |
| Can I see what it is doing when it goes wrong? | No | Source is included (Magica) | Yes |

Unity's column is from Unity's manual (opened). The bought solvers' column is from search summaries (not opened).

**Recommendation: our own.** I can test it; our bodies are the easy case (about fifteen rigid shapes); we control every
safeguard. **What we give up:** cloth against itself, so pleats may pass through each other. **Fallback** if the look
falls short after step 3: Magica Cloth 2 with the same garments.

## How cloth simulation works, and what keeps it out of the body

A piece of cloth is a net of points joined by threads. Each step, gravity and the cloth's own speed move every point a
little. Then the rules are put right, one at a time, several times over: a thread that got too long pulls its points
together, a point inside the body is pushed out, a pinned point is put back on its pin. (Position based dynamics; the
modern form is XPBD.)

**Six safeguards, stacked:**
1. **Pushed out of each part's own shape** (a capsule, a box or a sphere), to its surface plus a small gap. Exact.
2. **Many small steps** each picture, so a fast arm moves only a little per step (Müller recommends this over catching
   fast movers afterwards).
3. **A speed limit:** no point moves further than the cloth's thickness in one small step. Müller's example: 1 cm, 20
   steps, 30 pictures a second allows 6 m/s. Ours (60 pictures a second, 2 cm, 8 steps): about 10 m/s.
4. **A leash:** every point has a home place on the body and may not stray further than a set distance (Unity's cloth
   calls it "Max Distance").
5. **It remembers its side** of a limb at the start of each step and is kept there (the idea in Chris Lewin's paper on
   EA's Frostbite site).
6. **Covered parts are hidden**, so a slip of a centimetre under the cloth cannot be seen.
And a net under all of them: a garment that goes wrong is quietly put back to its hanging shape. It must never explode.

The points must be closer together than the thinnest part they cover: about 4 cm.

## What makes our fighters a hard case

| What our fighters do | Why it is hard for cloth | The answer |
|---|---|---|
| Swing an arm so the blade does 16 m/s | Fast cloth can pass through a body part | Small steps, the speed limit, the leash. Sleeves tied back in version 1 |
| Kick, stumble, jump in | A leg lifts inside the skirt and drags it | The skirt kind of hakama; enough cloth at the hem |
| Fall, lie on the ground, get up | Cloth trapped between body and floor | The floor is one more thing to be pushed out of; put back to hanging if badly caught, and at the start of a get-up |
| Jump from place to place in one step | The body is suddenly metres away | The cloth is moved with it and settled |
| Lose a hand or an arm | The sleeve's arm is gone | The sleeve hangs empty. Cutting cloth is not in this plan |
| Hold a sword and a shield | Blade and board pass close to his own sleeves | Short sleeves stay clear; cloth ignores weapons at first |
| Fight in a clinch | Two men's clothes meet | Clothes ignore the other man in version 1 |
| Freeze on a hit, or go slow | The body stops dead | The cloth keeps moving through the freeze |
| The planned shock wave | The body shudders fast | The cloth follows the body as drawn, so it picks the wave up for free |
| Six fighters at once | Six sets of clothes every step | A budget, measured from step 1 (Lewin's example character: 0.4 ms a step on one thread) |

**The cost, estimated (not measured):** about 600 points a fighter (skirt about 400, sleeves about 200), 8 small steps;
3,600 points for six. If plain C# is too slow, Unity's Burst compiler is the usual answer; adding it is his decision.

## How the clothes get made

By code, from flat pieces, as the fighter is set out: **cut** (each piece a flat grid of points about 4 cm apart),
**wrap** (the skirt's strip into a tube round the hips, cut wider than the waist, the extra folded into pleats where it
is pinned: five in front, two behind), **pin** (the skirt's top edge to the sash ring; each sleeve's top round the upper
arm), **drape** (the solver runs a second or two before anything is shown), **draw** (the points are the mesh's corners,
both sides visible, re-lit every picture). The body's size comes from the rig, so Large and Small fit with no extra work.

| Part | Version 1 |
|---|---|
| Hips, thighs | Hidden, under the skirt |
| Shins | Hidden when the hem is at the ankle |
| Spine, chest | Covered by the snug shell (fixed, not simulated) |
| Upper arms | Hidden, under the sleeves |
| Forearms, hands, feet, head | Drawn |

The first clothes: plain cloth in one or two colours, a single sheet, soft folds. Pattern, knot, thickness and crisp
pleats are step 4.

## The sliders and switches (a new lab section "Clothes")

| Slider | What it does | First value |
|---|---|---|
| Weight | Heavy hangs straight and swings slowly; light flutters | Middling, like cotton |
| Stiffness | How easily it folds | Fairly stiff |
| Stretch | How far a thread may stretch | 3% |
| Air | How fast a swing dies away | About a second |
| How much of his movement it feels | 1: thrown about by everything he does; lower: calmer | 0.7 |
| Wind | A light breeze | A little |
| Gap | How far off the body the cloth is held | 1.5 cm |
| Leash | How far a point may stray from its place | 25 cm on a sleeve, more down the skirt |
| Small steps | More is safer and costs more | 8 |

Switches: clothes none (old) / skirt / skirt and sleeves / all; covered parts hidden / drawn; show the cloth's points.
And a button **"Shake him"**: the dummy goes through a fixed set of moves (a turn, a jump, an attack, a kick, a fall, a
get-up) so the cloth can be judged on demand.

## The steps

| Step | What you get | In the lab | What I can measure without Unity |
|---|---|---|---|
| **1. A skirt on a standing man** | The solver, and the pleated skirt hanging from the sash on the duel lab's dummy; he stands, walks, turns; covered parts hidden | "Clothes: none (old) / skirt". "Shake him". Levers for weight, stiffness, air, wind | How deep and how often a point is inside the body; stretch; stillness at rest; cost per fighter; pictures of the cloth's shape drawn from the numbers |
| **2. Through the hard moves** | The skirt through attacks, combos, a jump in, a kick, a stumble, a fall, lying down, a get-up | "Shake him" runs them all | The same per move. Target: no point deeper than the gap in more than 1 picture in 1,000, never on a drawn part |
| **3. Sleeves** | Tied-back sleeves, then the full wide sleeves, sword and shield in hand | "Clothes: skirt and sleeves". "Sleeves: tied back / full" | The same for the arms at fighting speed; how often a full sleeve meets his own blade or board |
| **4. The look** | Chest shell, collar, sash knot; colours for the two sides; pressed pleats; a pattern; thickness | "Clothes: all" | Nothing that matters: his eyes |
| **5. Everyone** | Large and Small, a lost arm, six fighters, the dance | "Dance" | The cost for six; the dance's checks no worse |
| **6. If wanted** | Two-legged trousers; cloth against the other man and his blade; cloth against itself; cloth that can be cut | to be planned | to be planned |

**The order:** clothes first, then the shock waves (he said "before we do this"), then the combos plan's step 7.

## Risks, and questions

Risks: I cannot see it (I can measure and draw its shape, not judge it); a ceiling on quality without cloth against
itself; lying down and getting up; cost for six in plain C#; clothes will show every small tremble the rigid body hides.

Questions:
1. Our own solver, and not a bought one? Recommended: our own, Magica Cloth 2 as fallback.
2. The skirt kind of hakama first? Recommended: yes.
3. Sleeves tied back first? Recommended: yes, full sleeves in the same step.
4. Who wears them: all the same, or a colour for each side? Recommended: one outfit, two colours.
5. May Unity's Burst package be added later if speed needs it?
6. Clothes before shock waves? That is how "before we do this" is read.

## Sources

Opened and read: Unity 6.6 manual, "Cloth"
(https://docs.unity.com/en-us/engine/6000.6/manual/physics-section/physics-overview/class-cloth). Matthias Müller, Ten
Minute Physics, lecture 15, "Cloth Self-Collision Handling"
(https://matthias-research.github.io/pages/tenMinutePhysics/15-selfCollision.pdf). Chris Lewin, "Cloth Self Collision
with Predictive Contacts", on EA's Frostbite site
(https://media.contentapi.ea.com/content/dam/eacom/frostbite/files/gdc2018-chrislewin-clothselfcollisionwithpredictivecontacts.pdf).
Not opened (approval could not be asked for while he was away; search summaries only): Wikipedia "Hakama" and "Kimono";
Magica Cloth 2 (https://magicasoft.jp/en/magica-cloth-2-2/); Obi Cloth (Unity Asset Store); Ten Minute Physics lecture
14. From my own knowledge: the tasuki; that Unity's and Unreal's cloth are built on position based dynamics; that Burst
makes such loops several times faster. From the game's code: `Rig/UnitBuilder.cs` (each part a box, a sphere or a
capsule with its own mesh), `Rig/PartId.cs`, the package list (no Burst today).

## Progress

**21 November, night: steps 1 to 3 built, and a first piece of step 4. Headless only, built blind.** His answers
(21:33): as recommended, after the shock waves. Our own solver (`ClothNet.cs`) with all six safeguards and the net; the
hakama (skirt kind, 640 points, pinned at a sash), two tied-back sleeves, and a top over the chest that bends with him
but is not simulated; hips, thighs, spine, chest and upper arms hidden. Duel lab: heading "Clothes", "Clothes:" none
(old) / skirt / skirt and top, "Shake him", the sliders. Labs and dance only. Measured: a point of cloth inside a drawn
part in 0.4% of pictures through "Shake him" and 1.2% in a dance (the plan's mark was 0.1%: missed, mostly the skirt
against the shins); nothing put back to hanging; about 4 ms a fighter a picture headless. The build log's entry
"clothes that move" has the table. Not done: steps 5 and 6, crisp pleats, pattern, two-legged trousers, full sleeves.

The build's film (3b2b, about three minutes): https://claude.ai/artifact/GA5FrDmgSqaxDYsk5eLmMq

## Progress, 22 November, 08:20: the open robe, on other cores (headless only)

His look at the first build: "Victorian dresses", stiff "like paper or thick rubber", legs through it, and it tanked the
frame rate. His drawing: one robe from the shoulders to the ankles, open down the front, a sash. His answers: no sleeves
yet; Burst now. Built: one sheet of 864 points held at the collar (chest) and pinned by the sash (hips), open in front,
armholes at the sides, stiffness 0.25; the threads kept out of the limbs as threads (a leg cannot pass between two
points); two guards against tearing; the sums moved into a Burst job, one picture behind his body (`ClothSolve.cs`,
`ClothJob.cs`). Headless through "Shake him": a point inside his body 0.05% of the time (the deepest 4.9 cm), never put
back to hanging, a thread at twice its length in 2.4% of pictures; on the main thread, without the sums, 0.9 ms a fighter.
**The Burst job is first compiled in his Unity; the frame rate is not known until he runs it.** The build log's entry of
22 November, 08:20 has the tables.

The film of this build (3b2b): https://claude.ai/artifact/4dFNQateNkfj5hViL6oZda
