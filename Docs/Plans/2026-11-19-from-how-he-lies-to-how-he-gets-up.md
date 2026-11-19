# From how he lies to how he gets up: plan

19 November 2026 · Renier. Live version: https://claude.ai/artifact/FAVSQjVZCbJ87jf6aargPa (progress notes are added there).

## Bottom line

Yes, this needs a plan first. It is not one fix: the moment a fallen fighter starts to rise is where nine tenths of
the get-up's crossings happen, and three separate things cause them.

The proposal in four sentences:

1. He **reads how he lies** and picks the get-up whose start is nearest to it.
2. He **gathers himself** while he is still the one physical body that fell: his muscles pull his arms and legs to
   that start, and because his body, the ground and his shield are solid to each other, the limbs go round what is
   in the way.
3. Only when he is close does the get-up **take over**, from where he really is.
4. The get-up's own poses are then mended so the board stays outside him all the way up.

Nothing here is built. Step 1 and 2 need no decision from you; step 4 waits on one (where the shield hand posts).

## What happens today

A fighter lies as a physical body until he has been still for about a second. Then, in one step:

- one of three get-ups is chosen by which way his hips face: from his back, from his front, or from a side;
- his body stops being a falling body and is held to a pose again;
- every part turns from how it lay to where the get-up has it, over 0.6 s (the head in 0.33 s, the arms in 0.54 s).
  It is a straight blend, part by part. It does not know that a chest, a shield or the ground is in the way;
- what he holds passes through his own body for the whole get-up.

**Your log of 19 November, 08:07** (27 get-ups, measured in Unity):

| Measure | Value |
| --- | --- |
| His shield, sword or sword arm crossing him while getting up | 7.08 s in all |
| Of that, in the first 0.75 s (the blend from how he lay) | 6.28 s, which is 89% |
| Of that, later in the get-up | 0.80 s |
| The fastest a part moved in those first moments, on average | 6.7 m/s |
| The same, the worst case | 24 m/s |
| Which part was the fastest | The shield hand or forearm in 16 of 27 |

Headless, for the same crossings, the pose itself has them: 2.02 s of 2.15 s in one run of six get-ups. So it is the
blend that puts the board through him, not the physics failing to follow.

## Why it goes wrong

Three causes, in the order they cost.

1. **The start of the get-up does not look at how he lies.** There are three get-ups, each with one fixed start. A
   man on his back with his shield arm under him and one with it thrown wide get the same start, so one of them has
   an arm that must cross his whole body to reach it.
2. **The way there is a straight blend.** Each part turns directly from where it is to where it must be. If the
   chest is between the two, the arm goes through the chest. That is the leap you see, and it is why parts reach
   6.7 m/s.
3. **His body stops being solid to itself at that moment.** While he falls and lies, his arms are stopped by his
   trunk (and, since this morning, his shield by his body). The step the get-up begins, all of that is switched off,
   because a pose that drives a board through a body and a body that refuses fight each other. So nothing objects
   to the blend.

A fourth thing is separate: the get-up's own poses, after the blend, still have the board in his hips for about a
third of a second and past his head at about 1.1 s. That is 0.80 s of the 7.08 s in your log.

## The proposal: he gathers himself, then gets up

A person on the ground does not snap into a push-up. He first pulls his arms and legs to where he can push from.
That stage is missing. The plan adds it, and lets physics do the part that is hard to plan by hand.

1. **He reads how he lies.** For each start he could use (back, front, either side, and each mirrored so either hand
   can go to the ground) the code measures how far every hand, elbow, knee and foot is from it, and whether a limb
   would have to pass his trunk or his board to get there. The nearest start with no such pass wins. Today only the
   way his hips face is looked at.
2. **He gathers himself.** He stays the physical body that fell. His muscles, which already draw a fallen body to a
   pose, are given the chosen start as that pose, at full strength. His trunk, the ground and his shield stay solid
   to his limbs, so an arm that meets his chest slides round it. Limbs that are free go first; an arm trapped under
   him comes out as his trunk rolls off it. His sword hand stays where it lies (the pin built this morning).
3. **The get-up takes over.** When every hand and foot is within a few centimetres of the start, or after a time
   limit, the get-up's poses begin from the pose he really has. What is left to blend is small, and it is blended by
   where the joints are, not by turning each part.
4. **The get-up keeps the board outside him.** The poses after the start are mended so the shield is carried round
   his body on the way up. This is the part that waits on your decision.

The gather can use the second he already lies still, so he need not be on the ground any longer than today.

It fits the rules in force: nothing recorded is played as it is, the motion is made from the body's own parts, and
what varies comes only from how he really lies.

## Other ways considered

| Way | What it is | Why not the first choice |
| --- | --- | --- |
| Planned paths | Work out, for each limb, a path round the body by geometry, and move the hands and feet along it | It has to be right for every way a body can lie. Each odd pose is a new case to write. Kept as the fallback for one limb that the gather cannot free |
| A slower or smarter blend | Keep the blend, order the limbs better, take longer | It still does not know what is in the way. It would hide the leap and keep the crossings |
| More recorded get-ups | Record a start for many lying poses | A body lies in more ways than can be recorded, and recordings are reference only |
| Leave the body solid and keep the blend | Switch nothing off when the get-up begins | Tried in effect on 18 November: stopped by his body, a sword hand was thrown at 16 m/s as the pose took the blade past his trunk. The pose has to stop asking for the crossing first |

## The steps

One at a time, each behind a Combat Feel switch with an old/new toggle in the target test, each measured headless
before you see it.

| Step | What is built | How you see it | The measure it must move |
| --- | --- | --- | --- |
| 1. The measure and a stop frame | Each get-up reports its own first 0.75 s: seconds of crossing, the fastest part, how far each hand went. No change to the motion | A button "Hold him at the start of the get-up" that freezes the moment the get-up takes over, and half-speed play | Sets the baseline: 0.23 s of crossing a get-up and 6.7 m/s in your log |
| 2. He reads how he lies | The start is chosen by the whole lying pose, mirrored where that is nearer | "Getting up, the start: nearest to how he lies / by his hips only (old)" | How far the hands travel in the first moments goes down |
| 3. He gathers himself | The physical body draws itself to the start before the get-up takes over; his shield stays solid | "Getting up, from how he lies: gathers himself / blends (old)", with "Falls on demand" from four sides and "Stumble him: Extra hard" | Crossing in the first 0.75 s under 0.05 s a get-up; no part faster than 3 m/s |
| 4. The take-over | The get-up begins from the pose he has; what is left is blended by where the joints are | Same toggle as step 3, plus a lever for the time limit | No jump in any part in the step the get-up takes over |
| 5. The board stays outside him on the way up | The get-up's poses carry the shield round his body | "Getting up, his shield: carried round him / as recorded (old)" | The 0.80 s of later crossings in your log goes to near zero |

Steps 1 to 4 apply to every loadout. Step 5 is for a shield bearer.

Each step ends as every step does: a README entry, a note here, CLAUDE.md brought up to date, a commit and a push,
and a 3b2b.

## What could go wrong

- **He cannot reach the start.** An arm pinned under him, or two bodies in a heap. The time limit ends the gather,
  and that one limb falls back to a planned path or, at worst, to today's blend. The report will say how often.
- **He cannot roll himself.** A fallen body's hips are free, and joint muscles alone do not turn a trunk over. The
  gather will need a soft hold on the hips that eases him toward the start, as a real body is turned by pushing on
  the ground. This is the part most likely to need tuning by your eye.
- **It looks like a second fall.** Muscles at full strength on a lying body can thrash. The strength comes in over a
  moment, and the tremble measure already watches for it.
- **Headless does not show it.** Headless falls are cleaner than yours: the same bash that gave 22 crossings in your
  falls gave few in mine until I ran six in a row. Every step will be measured on many falls, and your log remains
  the judge.
- **Two bodies.** Fighters now keep apart while one is down (built today), which the gather relies on: it assumes
  the only solid things near him are the ground and himself.

## Decisions for you

| # | Question | My recommendation |
| --- | --- | --- |
| 1 | Physics leads the gather (this plan), or paths worked out by geometry? | Physics leads. It cannot put a limb through him, and it is different each time for a real reason |
| 2 | May the gather make him lie longer? | No: it uses the second he lies still today. If a case needs more, at most 0.5 s more |
| 3 | Where does a hand with a shield on its arm go to the ground, and where is the board while he pushes up? | The board out to his shield side at shoulder height, his forearm flat on it; he pushes up on that forearm and the board's rim, and the board comes up edge first into cover. Step 5 waits on this |
| 4 | Build in the order above, or the gather (step 3) first because it is the one you will see? | The order above. Step 1 is half a day and is how every later step is judged; step 2 makes step 3's job smaller |
| 5 | May a get-up differ each time it is seen? | Yes. It already does a little; this makes the first second depend on how he fell |

## Progress notes

**19 November 2026.** Plan written after your notes of 08:26. Not started. Built the same morning, and needed by this
plan: a fallen fighter's shield stays solid against his own body, and fighters keep apart while one is off his feet
(both headless only; see the README's entry of 19 November, "his second look").
