# Getting up: research and plan

> Copy of the Claude document <https://claude.ai/artifact/TQNgYhYhCCX9sWzAMpcqnh>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 14, 2026 · @Renier

## The short version

Today's get-up snaps the body into a tidy lying pose and then plays five mirror-image poses; the new one starts from how the body really lies and gets up the way a fighter is taught to.

- **From where it lies**, with no snap. Head first.
- **Lopsided**: one hand on the ground, the shield arm free, up through one knee.
- **Three routes**: from the back, the front, the side.
- **Toward the enemy**: it turns as it rises and ends in its stance.
- **Same length** as now, about 2 s, so the fight's balance stays where it is.

It is behind a switch, the fall buttons show it, and a toggle beside them gives you old against new.

## The get-up today

The get-up today is five poses played in a fixed order, the same for everyone, starting from a tidy lying pose the body was never in.

| Step | What the code does | Why it looks wrong |
| --- | --- | --- |
| 1 | Looks only at whether the hips face up or down, and picks one of two routes | A body on its side, curled, or half sitting gets a route made for lying flat |
| 2 | In 0.3 s, pulls the body from however it fell into a straight, tidy lying pose | A sprawled body snaps straight. This is the worst moment |
| 3 | Plays five poses about 0.4 s apart: lying, hands and knees (or sitting), a crouch, rising, standing | Both arms and both legs do the same thing at the same time. People are lopsided when they get up |
| 4 | Stands facing the way the body happened to lie | It gets up facing nowhere, then has to turn to its enemy |
| 5 | Every fighter takes the same time, to the same beat | Three fighters felled together rise in step |

What is good and stays: the poses are built in code and fit any size of body; the hands and feet are placed by the limb solver; the shield is held up toward the threat while rising; what the hands hold is kept out of the ground. The whole get-up takes about 2 s for a Medium, and that stays, so the fight's balance does not move.

## How people get up, and how fighters are taught to

There is no single way people get up, but fighters are taught one: keep three points on the ground, keep a hand free, and never bring the feet together.

**Ordinary people.** VanSant's 1988 study of adults rising from their backs found 13 different combinations of arm, trunk and leg movement; the most common was used by only about a quarter of people. A later group of 30 to 39 year olds used more lopsided movements than the younger one. So lopsided is normal, and variety between people is normal ([Green 1989, on VanSant's study](https://core.ac.uk/download/33362344.pdf)). Asked to get from lying to hands and knees, 79% of young adults pivot over one side instead of going straight ([roll to quadruped test](https://ijspt.scholasticahq.com/article/147064.pdf)).

**Fighters.** Two sources teach the same move.

1. Sit resting on one hand, the other arm free ([US Army combatives, stand up in base](https://www.armystudyguide.com/content/army_combatives/basic_groundfighting_combative_techniques/stand-up-in-base.shtml)).
2. Put the weight on that hand and the opposite foot. Two supports, so the hips can float ([technical stand-up](https://www.grapplearts.com/the-four-variations-of-the-bjj-technical-standup/)).
3. Lift the hips and swing the free leg back between the two supports, so its foot lands behind the hand.
4. Only when both feet carry weight does the hand leave the ground. The feet end wide apart, in a fighting stance, hands up.

The reasons given are the ones we care about: the fighter is never without a base while rising, and the free arm covers the head the whole time. Bringing the feet together first "is not at all stable". When there is weight on the fighter, or it is hurt, it comes up onto a knee first, or posts on an elbow instead of the hand.

**What we take:**

- Lopsided, always: one hand down, the other free; one foot planted before the other.
- The free arm is the shield arm, kept toward the enemy. The game already raises the shield while rising.
- The feet land apart, in the stance, never together.
- A hand or foot that is planted stays where it is until it is lifted.
- Not everyone does it the same way or at the same speed.

## How games get a body back on its feet

Games choose a get-up by how the body lies, and blend from the ragdoll's own pose into it without a jump.

| Source | What they do | What we take |
| --- | --- | --- |
| [Just Cause 2](https://api.sambuz.com/doc/physics-ics-meets-s-anim-imation-ion-charac-racter-ter-ppt-presentation-926923) | Once the body is slow enough, compare how it lies with a set of get-up starting poses, pull it to the nearest, and blend into that get-up | More than two starting poses, chosen by the real pose |
| [Mocap Online's ragdoll guide](https://mocaponline.com/blogs/mocap-news/ragdoll-physics-animation-guide) | Back and front get-ups are the least you need, with more for lying on a side. Start the animation from the ragdoll's captured pose and fade the physics out over 0.2 to 0.5 s | Blend from the pose it is really in. A side route |
| [A Unity write-up](https://www.moddb.com/news/blending-ragdoll-physics-and-animation-in-unity) | Face up or face down by one test of the hips; notes it should be extended to more angles | The test we have today is the bare minimum |

None of these says how to make the get-up itself look good; they use recorded clips for that. We have no clips, so that part comes from how fighters are taught, above.

## The new get-up

The new get-up starts from the pose the body is really in, goes up lopsided through one knee with a hand on the ground, and ends in the stance turned toward the enemy.

**Six rules, each answering one fault of today's:**

| Rule | What it replaces |
| --- | --- |
| **Start from where it lies.** The body moves straight from its real pose toward sitting or kneeling. The head moves first, then the arms, then the trunk and legs | The snap into a tidy lying pose |
| **Three routes, chosen by how it lies:** from the back, from the front, from the side | Two routes, by one test |
| **Lopsided.** One hand stays on the ground while the other arm is free; the front foot plants first and the back leg comes through after | Both sides moving together |
| **What is planted stays planted** until it is lifted | Hands and feet drifting |
| **Turn toward the enemy** while still on the ground, by up to 35 degrees, and look at it first | Rising to face nowhere |
| **Not all alike.** Each get-up is a little faster or slower, by up to 8% | Fighters rising in step |

Which hand goes down is settled by the stance: the hand on the side of the back foot. For sword and shield that is the sword hand, which leaves the shield arm free and toward the enemy, as fighters are taught.

**From the back** (the technical stand-up, through a knee):

1. Head up, looking at the enemy. Rolls a little toward the posting hand, which goes to the ground beside the hip.
2. Sits up onto that hand. The front foot is drawn in and planted.
3. Weight on the hand and the front foot. The hips lift, the trunk leaning toward the hand, and the back leg folds under.
4. The back knee is down under the hips: a half kneel. The hand leaves the ground.
5. Rises off the front foot; the back foot lands on its place in the stance.
6. Standing, in guard.

**From the front:**

1. Head up. Hands planted by the chest, the chest comes off the ground.
2. Hips back, onto hands and knees, the weight toward the posting hand.
3. The front foot steps through to its place between the hands. The free hand lifts.
4. Half kneel, upright. The posting hand leaves the ground.
5. Rises; the back foot lands on its place.
6. Standing, in guard.

**From the side:** rolls onto its front as it pushes up, then the route from the front.

The whole thing still takes about 2 s for a Medium, and longer for a Large, as now.

## How it is built, switched, seen and checked

It is built on the get-up the game has: the same way of authoring poses in code, the same limb solver, new poses and new rules.

- **Switch.** `getUpsAreNatural` on the Combat Feel object, on by default. Off gives the get-up as it was.
- **Seeing it.** The fall buttons in the tube test (Back, Forward, Left, Right) each end in a get-up. A toggle in the same panel switches old and new, so the same fall can be watched both ways.
- **Checking it.** I cannot watch it, so each get-up is measured and written to the playtest log as a `getup` line: which route, how far it turned toward the enemy, the fastest any part moved in the first moments (a snap shows as a high speed), the furthest a hand or foot was from where its pose wanted it (a pose the body cannot reach), and how far a planted hand or foot slid.
- **What the measures cannot tell.** Whether it looks like a person. That is yours.

When the falls keep one body (step 2 of the falls plan), the get-up drives that body instead of the old ragdoll's. The poses and rules here do not change.

## Sources

Pages I opened and took facts from.

- [Green 1989, a thesis on adults rising from supine, summarising VanSant 1988](https://core.ac.uk/download/33362344.pdf). I could not open VanSant's paper itself.
- [The 30-second roll to quadruped test](https://ijspt.scholasticahq.com/article/147064.pdf): how young adults get from their backs to hands and knees.
- [US Army combatives: stand up in base](https://www.armystudyguide.com/content/army_combatives/basic_groundfighting_combative_techniques/stand-up-in-base.shtml).
- [The four variations of the technical stand-up, Grapplearts](https://www.grapplearts.com/the-four-variations-of-the-bjj-technical-standup/).
- [Physics Meets Animation: Character Stunts in Just Cause 2](https://api.sambuz.com/doc/physics-ics-meets-s-anim-imation-ion-charac-racter-ter-ppt-presentation-926923): slides.
- [Ragdoll physics and animation guide, Mocap Online](https://mocaponline.com/blogs/mocap-news/ragdoll-physics-animation-guide).
- [Blending ragdoll physics and animation in Unity, ModDB](https://www.moddb.com/news/blending-ragdoll-physics-and-animation-in-unity).

What I looked for and did not find: how long a fit adult takes from lying to standing when hurrying. The papers I could open count movements or patterns, not seconds. The 2 s we use is the game's own figure, kept.

## Progress

**14 November: built.** All of the above is in, behind `getUpsAreNatural`, with the old/new toggle beside the fall buttons. Checked headless only; not yet seen in Unity.

The same falls both ways, headless:

| Measure | As before | New |
| --- | --- | --- |
| The fastest any part moved in the first moments | 5.9 to 9.3 m/s | 2.7 to 5.8 m/s (a Large: up to 6.5) |
| A hand or foot on the ground, furthest from where its pose wanted it | 37 cm | 0 cm from the back, 6 cm from the front |
| Planted hands and feet slid, in all | 2 to 3 cm | 3 to 7 cm |

The 37 cm was the old get-up from the back asking for a hand on the ground behind the body that the arm could not reach. One thing went wrong on the way and is fixed: blending from the lying pose toward poses that were already moving made an arm jump across the body in a single step; the blend now takes each step from the pose before it.

The slide is a little worse than before and is the first thing I would tune once you have looked.

## Progress, 14 November (later): the sword after a get-up

Seen in Unity: right after a get-up, the swordsman's sword "rotating and going a bit crazy" as he turned round. Two faults, both older than the get-ups, both fixed.

- **The sword was reaching for where he stood before the fall.** The guard remembers the sword's place as a spot in the world. While he was down nobody updated it, so once up his arm reached for a spot 1.4 m behind him, through his own back, for the whole turn. The guard is now taken up from the get-up's last pose (and afresh any time it has not been held for 0.2 s). Headless: the pose had the sword hand behind its own chest for 0.87 s before, 0.00 s now.
- **A turn round to the left went right first.** The guard has one turning-round clip and it goes right. Asked to go left it played a bit of it the wrong way (55 degrees), then all of it again (165 more). It now goes the way asked in one turn.

On demand in the tube test's panel: "A fall and then a turn" (three buttons), with a switch that puts both fixes off. Not verified in Unity.

## Progress, 14 November (later still): the knee through the torso

Seen in Unity on the swordsman getting up from its back: the leg he plants went too far back, and then the knee spun round through his torso. Measured, it was in every fighter's get-up from the back, not only his: a thigh turning at 81 to 90 rad/s, 140 degrees past what a hip bends.

- **Why.** The planted foot was put under the hip (a leg folded to nothing has no sensible knee), and the other foot was swept back through the hip while the hips were still low.
- **Now, from the back** (a technical stand-up): sit a little behind the planted foot, which is always the stance's front foot; lift the hips high on that foot and one hand; only then swing the other leg back under them; rise over the planted foot.
- **From the front:** the foot is stepped through with the hips higher and the back rounded.

Headless: from the back a thigh now turns at 10 to 15 rad/s and is at most 20 degrees past the hip's range (was 90 rad/s and 140 degrees). From the front, at most 46 degrees past (was 69). Every get-up line in the log now carries these numbers. Switch: Combat Feel "Get Up Legs Clear The Hips", and a toggle in the tube test's panel. Not verified in Unity.
