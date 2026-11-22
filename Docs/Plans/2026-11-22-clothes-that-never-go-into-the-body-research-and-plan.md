# Clothes that never go into the body: research and plan

22 November 2026, 08:55. Live version (with its picture and the sources as links): https://claude.ai/artifact/NcFgFLd75rj2ZDECPKUVp1
This copy is refreshed when the plan changes a lot; progress notes go to both.

His words (08:46, with a picture of the robe): "the cloth is still not working. ... instead of doing a robe, do long
sleave shirt and pants that go to the feet. Then I want you to go back to the drawing board and make updated plan ...
You will need to do research on how other solutions have approached this. ... Cloth that does not intersect with the
body ever. Spare no expense in finding the righ solution to have a robust cloth that never intersects with the limbs.
... you can hide the body parts that are occluded eventually, but for how I want you to come up with a solution so
robust that we dont even need to do that".

## Bottom line

**What went wrong:** the cloth was kept out of the wrong body. It was pushed out of the fighter's *physics* shapes (the
boxes and capsules the game collides with), but what you *see* is the drawn body, and the drawn body is much fatter. I
measured the 16 body meshes against their physics shapes: on every part except the hands and feet, 80 to 90% of the
drawn surface stands outside its physics shape, by 2.4 to 4.8 cm in the middle of the range and up to 12 to 16 cm at
the far ends. The cloth sat 2 cm off the physics shapes, so it was inside the drawn thigh, chest and arms most of the
time. That is the knee and the collar in your picture. Two smaller causes make it worse: the cloth is worked out one
picture behind the body, so a fast limb runs into it; and it collides as loose points that a limb can slip between.

**The fix, in one idea:** stop *chasing* the body and *carry* the cloth on it. Every point of cloth gets a home on a
body part, a set distance outside the *drawn* surface of that part, and moves with the part exactly, the same picture.
The simulation only adds a small wobble on top, and that wobble is fenced: it may not go further than a set distance
from home, and never inward past the surface. Then a last pass, on the body exactly as it is drawn this picture,
pushes any point and any edge that is still inside back out. This is how the big game engines keep clothes out of
bodies (Unity, Unreal, NVIDIA's cloth all have it), plus a measured guarantee on top that they leave to the artist.

**What you get:** a long-sleeved shirt to the wrists and trousers to the ankles, cut by code around this exact body.
Snug where the body is broad (shoulders, chest, thighs), looser at the cuffs and the lower legs, so they still move.
**The goal is zero cloth inside the drawn body, measured against the real meshes, in every test.** Hiding body parts is
not needed for it; it stays as a fallback switch only.

**Nothing in this plan is built.** The measurements in it are from the mesh files and the code, not from Unity.

## What your picture shows, and why

In your picture of 08:46: the swordsman's knee and thigh come through the front of his robe, his collar sits low
across his chest like a strapless dress, and the target's robe has slid round so one front panel hangs off his
shoulder like a strap.

**Cause 1, the big one: the cloth was kept out of the physics body, not the drawn one.** Each body part has two
shapes: the drawn mesh (`Art/Bodies/Medium/SM_Medium_*.fbx`) and the physics shape (a box or capsule in
`medium_rig.json`) that the cloth was pushed out of. I read the 16 mesh files and measured every drawn point against
its own physics shape (in the part's own space, with X mirrored as Unity imports it):

| Part | Drawn surface outside its physics shape | Middle of the range | Far end |
|---|---|---|---|
| Pelvis | 80% | 3.1 cm | 15.5 cm |
| Spine | 86% | 4.8 cm | 15.2 cm |
| Chest | 80% | 3.2 cm | 14.0 cm |
| Upper arm | 90% | 3.9 cm | 8.8 cm |
| Forearm | 90% | 2.6 cm | 6.3 cm |
| Thigh | 90% | 4.6 cm | 12.5 cm |
| Shin | 85% | 2.4 cm | 7.3 cm |
| Hand | 25% | 0 | 4.6 cm |
| Foot | 52% | 0.6 cm | 6.5 cm |

For example the drawn thigh is about 8 cm thick from its middle, the physics capsule 5.6 cm; the drawn chest reaches
31 cm up to the base of the neck, the physics box stops at 19. The cloth was held 2 cm off the physics shapes, so on
most of the body it was 1 to 3 cm *inside* what you see. The far ends are the round joint pieces and the ends of the
meshes, which overlap the next part. (Read headless, without Unity: if Unity's import adds a turn I have not accounted
for, the numbers shift a little, not the conclusion.)

**Cause 2: one picture late.** Since this morning the cloth is worked out on another core and taken up a picture
later. A fist or a knee at 5 to 10 m/s moves 8 to 17 cm in one picture at 60 frames a second.

**Cause 3: loose points and gaps.** 864 points about 3 to 5 cm apart. Points were kept out of the body, threads out of
capsules but not boxes (chest, spine, pelvis, hands, feet), by pushing that fights the pins at the collar and the sash.
Where they disagree, this morning's guards let the cloth stay in the body rather than tear.

**Cause 4: the cut.** A robe open at the front and at both armholes is three loose panels held only at the collar and
the sash; nothing holds a front panel in front of him, so it slides round (the target).

None of these is a tuning problem. Causes 1 and 2 mean the old cloth could not have stayed out of the drawn body
whatever the sliders said.

## How others do it

**Games: tight clothes are carried, not simulated.** Small and tight garments are usually not simulated at all; only
big loose pieces such as capes are (80.lv). Tight garments move mostly with the body (the 2024 loose-clothing paper).
Even a simulated jacket is skinned at its top row (Alan Wake, Remedy).

**Engines: "max distance" and "backstop".** The cloth mesh is skinned to the skeleton like the body; each point may
move only within a sphere around its skinned place (max distance), and a second sphere set back inside the body along
the normal is a place it may never enter (the backstop): NVIDIA APEX/PhysX clothing, Unity's Cloth (max distance 0 =
not simulated; "surface penetration" / `collisionSphereDistance` = backstop), Unreal Chaos Cloth (max distance,
backstop distance and radius), Open 3D Engine (the backstop exists because simple collider shapes are not enough to
keep the cloth out of the body meshes: **exactly our cause 1**). None of them *proves* the cloth never enters the body:
the artist paints the distances by eye. We will measure it.

**Research: guaranteed non-penetration.** Bridson, Fedkiw and Anderson (2002): the first guarantee of no cloth
self-interference. IPC (2020) and Codimensional IPC (2021): intersection-free whatever the step or speed, but an
optimisation solve each step, far too costly here. **Offset Geometric Contact (OGC, SIGGRAPH 2025):** the same
guarantee cheaply, by giving each point its own bound on how far it may move in a step; 246,000 points in 6 to 12 ms
on a top GPU. **We borrow its central idea:** no point moves further in one small step than its distance to the body.
Distance fields (SDF) are the usual body test; for our rigid mannequin pieces each piece's exact field can be baked
once from its mesh.

**Skinning across a joint.** Linear blend skinning collapses at bent and twisted joints; dual quaternions remove that
but bulge. Our joints are round balls, so cloth over a joint is turned round the ball's centre at a fixed radius.

**Learned cloth** (EA's Swish, learned body fields): shipped, but needs data and a pipeline we do not have.

## The approach: a carried shell, a fenced wobble, and a last word

Each picture: the body as drawn (this picture's parts, shock wave included) → homes on a shell (each point fixed to a
part, 1.2 cm off its drawn skin) → a fenced wobble (other cores; never past its max distance or inward) → the last
word (points, edges, faces out of the drawn body). The wobble may be worked out late on another core; the homes and
the last word always use this picture's body.

1. **The drawn body, baked once.** Each rigid piece's distance field from its mesh, in its own space, about 2 mm
   accurate, in a data file read by the game and the headless tests. The physics shapes are no longer used for cloth.
2. **Homes on a shell.** Every cloth point has a part and a place in its space 1.2 cm outside its drawn surface. Over a
   joint the home is on the joint ball: fixed distance from its centre, direction turned partway between the pieces.
3. **A fenced wobble.** A displacement in the part's own space. Max distance: 0 at the collar, shoulders, waistband;
   1 to 2 cm on chest and thighs; 5 to 8 cm at cuffs and trouser bottoms. Backstop: never inward past home, never
   closer than 1.2 cm to the drawn surface. Small steps on other cores; no step longer than the distance to the body.
4. **The last word.** Every point on this picture's homes, checked against the union of all covered parts' fields;
   pushed out to 1.2 cm. Then thread middles and triangle centres; if one is inside, its corners pushed out together.
   On the main thread or a short job finished before the picture is drawn.

**Why "never" holds.** After the last word every point is at least 1.2 cm outside. A thread of length L over a surface
of radius r sags at most r − √(r² − L²/4): on the forearm near the wrist (radius about 3 cm) a 2.5 cm thread sags
2.7 mm. Threads ≤ 2.5 cm on the limbs; the middle-and-centre check catches the hollows where pieces meet. The measure
counts every point, thread middle and triangle centre inside the drawn body, every picture; the target is zero.

**What it does not promise.** The drawn body can overlap itself (a hand pressed into the chest, the pieces at a
joint). The cloth stays over the outside of the parts it covers; a bare hand pushed into the chest shows through the
shirt as it shows through the bare chest now. Others (the other fighter, weapons, bare hands, feet, head) push the
cloth only as far as its own body allows.

## The garments: a shirt and trousers, cut by code

**Shirt:** a body tube from the base of the neck to the hips, sleeves to the wrists, joined by a ring round each
shoulder ball; hem a little below the waist over the trousers. About 1,600 points.
**Trousers:** a waistband, a seat, leg tubes to the ankles, joined at the crotch over the hip balls; snug at the waist
and thighs, widening from the knee. About 1,500 points.
**Cut:** rings round each part's axis at stations a few cm apart, on rays to the baked surface, 1.2 cm beyond it plus
slack where loose. Threads across, down, diagonals, every other point for bending; ≤ 2.5 cm on the limbs.
**Looseness:** per point max distance as above (the sliders). **Colour:** shirt per side, darker trousers, the sash.
The robe stays as "robe (old)" until the new clothes pass.

## How we prove it never goes in

The old measure counted cloth inside the physics shapes (0.05% while he saw a thigh through it). The new measures are
against the drawn body. **Budget: zero** points, thread middles or triangle centres inside a covered part, any
picture, any test. Two independent checks: the fast one every picture (the baked fields), the exact one on sampled
pictures (inside-or-outside against the mesh triangles, not using the bake). Tests: the torture rack (every joint to
its limits alone and in pairs, 10,000 random poses, the same snapped in one picture), "Shake him", rings, kicks,
get-ups, combos, four dances of 70 s. A headless renderer drawing the real body meshes with the cloth over them. In
Unity: "Cloth inside the body: 0 this scene", an x-ray switch (red dots), `clothin` log lines, the cost on the panel.
Counted apart: the drawn body overlapping itself.

## Steps

| Step | What | In the lab | Pass mark |
|---|---|---|---|
| 1 | Bake the drawn body; both checks; the headless renderer; measure today's robe. | X-ray switch; "Cloth inside the body"; a start-up check in Unity that the bake matches the loaded meshes. | Robe clearly above zero; Unity check within 2 mm. |
| 2 | Shirt and trousers, carried only (homes, joint balls, last word). | "Clothes:" shirt and trousers / robe (old) / none; "Shake him". | Zero, both checks, all tests. |
| 3 | Never late: homes and last word on this picture's body; job done before drawing. | Frame rate; cost. | Zero with snapped poses; main thread under 1 ms a fighter. |
| 4 | The fenced wobble. | Looseness, weight, air, wind; wobble on/off. | Zero; cuffs and trouser bottoms swing (cm). |
| 5 | Pushed by others, never past his body. | Strike, combo, dance. | Zero. |
| 6 | The look: fold shading, sash, colours, fewer points far away. | His eye. | His word. |

## Risks, cost, questions

Risks: Unity's import placement of the meshes (step 1's check); big overlaps (pelvis over thighs, chest over spine)
make the seat and middle a little fuller; cloth against cloth at the armpit not mended; carried cloth reads fitted.
Cost (estimate): about 3,100 points a fighter, about 30,000 field lookups a picture; expected well under 1 ms with
Burst; measured in step 3.

Questions: (1) "never" as defined, body self-overlap excepted? (recommended yes); (2) the fit: snug top, loosening to
cuffs, widening from the knee (recommended), tight, or loose; (3) keep the sash? (recommended yes); (4) steps 1 to 3
together before he looks (recommended), then 4 to 6 one at a time.

## Sources

NVIDIA APEX Clothing docs; Unity ClothSkinningCoefficient and Cloth manual; Unreal Chaos Cloth backstop and max
distance; Open 3D Engine NVIDIA Cloth constraints; 80.lv on cloth in games; Remedy, Alan Wake cloth; the 2024
loose-fitting clothing paper (arXiv 2404.16896); Bridson, Fedkiw, Anderson 2002; Li et al. IPC 2020 and C-IPC 2021;
Chen et al. OGC 2025; Macklin et al. Small Steps 2019; Akar et al. shallow SDFs 2024; Kavan et al. dual quaternions;
Disney Research optimized centers of rotation; EA SEED Swish. Links in the live document.
