# Impacts that carry weight

> Copy of the Claude document <https://claude.ai/artifact/HkqvPHoe22TWSjZwxvdW5V>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 18, 2026 · @Renier

## What Renier saw

18 November, 11:12, after trying the hard bash and its counter:

> "It seems like when the shield bash is blocked by another shield bash, all of that force just disappears, like its force going nowhere. It makes a bash, which should be a high energy event, feel weak and flimsy, like it wasn't a hard bash at all. But even the medium and small should be met with more reaction. Like every force has an equal and opposite reaction, but the shield blocks are missing this now. Their bodies barely move or respond to the collision. I want it to look like their bodies are absorbing this energy. I want it to have some unpredictable effect on them, like this should still be able to push one or both off balance (for when you do stumbles)."

The brief: look into this before anything else, research and plan before building, and watch for whether what is found belongs to the whole system or only to this shield case.

## Bottom line

You are right, and it is measurable: a hard bash carries about ten times the momentum of a sword cut, and today the bodies show less of it than they show of a cut on a shield.

- **What is wrong.** The game already has a system that makes a sword blow show in the body: the trunk rocks, the shield arm gives, the body goes soft for a moment, both freeze for a few hundredths of a second, the struck one is driven back or stumbles. The shield bash was built beside that system, not through it. A bash only changes the other's ground speed, or knocks him over. Nothing happens in either body.
- **The counter made it worse.** When I added the counter this morning I capped both speeds by hand: the defender leaves at 1.2 m/s, the basher at 1 m/s backward. About 400 of the 424 units of momentum are simply deleted. That is the "force going nowhere" you saw.
- **Local or system-wide.** Both. The cause is local (the bash skips the impact system). The cure is system-wide: one door that every impact goes through, so equal and opposite reaction is a rule of the game and not something each new move has to remember. Kicks, the pounce and combos on a shield all need the same door.
- **What I propose.** Five steps, each behind a switch: measure it; put the bash through the door; keep a momentum ledger for body-on-body collisions; show the bodies absorbing the energy over the quarter second the collision lasts; let the result tip one or both off balance, unpredictably. Details below.

Nothing is built yet. This is the research and the plan, for your word.

## What happens today at each kind of impact

A sword blow goes through a full reaction system; a bash goes round it. Read from the code (`ClipFighter.TakeHit`, `Jar`, `Bashed`, `StrikeHits`).

| Impact | The one who takes it | The one who deals it |
| --- | --- | --- |
| Sword on a body | Force on the physical body; trunk rocked about the hips; head snapped; struck arm or leg knocked; region goes soft; poise lost; driven back, stumbles, or down by what his legs can catch; a reaction clip | Freeze for 0.04 to 0.16 s; blade slowed or stopped; hand knocked back |
| Sword on a shield | Force on the shield forearm; trunk rocked (0.6 of a body hit); shield arm gives by the blow's weight, guard knocked open if heavy; poise lost; driven back | Freeze; blade wound back; hand thrown back; upper body jarred (0.3 of the blow) |
| Sword on a sword (parry) | Sword arm knocked; trunk rocked (0.4); the two blades bind and give together; driven back | Blade held in the bind, then swept off; jarred |
| **Small or medium bash on a body** | **Ground speed changed, guard open 0.3 or 0.8 s. Nothing in the body** | **Nothing** |
| **Hard bash on a body** | **Knocked over** | **Nothing; he runs on through** |
| **Hard bash, countered, held** | **Ground speed set to at most 1.2 m/s. Nothing in the body** | **Stopped, sent back at 1 m/s. Nothing in the body** |
| **Any bash on a shield held in cover** | **Same as on a bare body: the shield is not looked at** | **Nothing** |

The four bold rows are the bash. None of them rocks a trunk, gives an arm, softens a body, costs poise or freezes a frame.

### Measured on the bodies

Headless, two Medium fighters (80 kg each), read off the recorded skeletons in the 0.6 s after contact. Single events from this morning's dances, so read them as sizes, not averages.

| Impact | Momentum | Trunk lean changes | Head shifts over hips | Hips dip |
| --- | --- | --- | --- | --- |
| Light cut on a shield (5 cases) | 35 to 46 N·s | 2 to 16 degrees | 9 to 21 cm | 0 to 4 cm |
| Cut on the head (2 cases) | 20 to 32 N·s | 8 to 21 degrees | 26 to 31 cm | 1 cm |
| Small bash on a body | about 100 N·s | 3.5 degrees | 6 cm | 5 cm |
| Medium bash on a body | about 190 N·s | 8 degrees | 14 cm | 5 cm |
| Hard bash countered, the defender (3 cases) | about 424 N·s | 1 to 5 degrees | 5 to 14 cm | 0 to 3 cm |

A hard bash that is countered moves the defender's trunk and head no more than a light sword cut on his shield does, with ten times the momentum behind it. The 1 to 5 degrees it does show is his own bash motion, not a reaction.

## Where the force goes missing

In a countered hard bash about 400 of 424 N·s of momentum, and about 1,000 of 1,100 joules of energy, vanish at the moment of contact with nothing drawn for them.

The hard bash as it is today, from the lab runs (basher 80 kg arriving at 5.3 m/s; defender 80 kg braced):

|  | Before contact | After contact, as coded today |
| --- | --- | --- |
| Basher | 5.3 m/s forward = 424 N·s | 1.0 m/s backward = 80 N·s the other way |
| Defender | braced, about still | 1.2 m/s backward = 96 N·s |
| Both together | 424 N·s forward | 16 N·s forward |
| Energy of motion | about 1,120 J | about 100 J |

Two things are wrong with that, and they are different problems:

1. **The speeds are invented, not worked out.** I wrote "at most 1.2" and "back at 1" this morning to keep the two on their feet. Nothing ties them to how hard the bash was, so a hard bash and a medium one come out alike.
2. **The energy has to go somewhere you can see.** Even when the final speeds are right, a thousand joules has to be taken up by knees, hips, trunk and shield arm. Today that takes zero frames.

For scale, what a real collision allows. The feet can only pass so much to the ground: grip times weight times the time of contact. For an 80 kg man with good footing over a quarter of a second that is about 150 N·s, a third of this bash. The rest must show up as the two bodies moving. So a braced man cannot make a charging man of his own weight simply stop: either he comes forward himself with real momentum, or both of them are carried back.

## What a real collision does, and how games show it

Five things from outside the project that shape the plan. Sources are at the end.

1. **The numbers are real-sized.** In filmed rugby tackles each player carries about 500 kg·m/s into a front-on tackle, at about 5 m/s, and 1,400 to 1,900 joules. Our hard bash (424 N·s, 5.3 m/s, 1,120 J) is a rugby tackle with a board in front. Nobody watching a tackle thinks nothing happened.
2. **It takes a quarter of a second, not one frame.** Measured peak shoulder forces in tackling are 2.4 to 3.4 kN. A 424 N·s collision at that peak lasts roughly 0.25 to 0.3 s (my arithmetic, taking a force that rises and falls). A sword cut is over in a few thousandths. So a body collision needs its own shape in time: the bodies press together, fold, then come apart.
3. **Bracing starts before contact.** In the same tackling study the muscles switch on about 0.3 s before impact. A man who sees it coming sets himself first. That is the counter: it should read as a brace before the boards meet, and a man who is braced should come out of it differently from one who is not.
4. **Momentum does not decide who wins.** In the rugby films the tackler dominated 57 per cent of front-on tackles, and mass, speed and momentum did not predict which. That is the licence for "unpredictable": a draw laid over the physics is true to life.
5. **Balance has a ladder.** Studies of people and walking robots describe three ways of surviving a push, in order as it gets bigger: sway at the ankles; bend at the hips and swing the trunk; take a step. Beyond what a step can reach, you fall. There is a simple measure of where the foot must go to stop: hips' position plus hips' speed times about a third of a second (for hips 1 m high). At 1.2 m/s that is 38 cm, a shuffle. At 2.7 m/s it is 85 cm, a full step back or two. At 4.4 m/s it is 1.4 m, more than a step, which is where the game already says the legs cannot catch him.

And from how games sell a hit: the reaction starts at once from an already displaced pose with no blend in, moves fast, then settles slowly; both parties react, the attacker too, with small shakes laid over the attack he is still in; there is a short freeze at contact; and a procedural bend of the spine on a block keeps two blocks from looking the same.

## Local fix or system-wide

The cause is local; the rule that prevents it coming back is system-wide.

The rule: **every impact is an amount of momentum passed between two bodies, each gets it equal and opposite, and both show it.** Every contact in the game, held against that rule:

| Contact | Follows the rule today? | What to do |
| --- | --- | --- |
| Sword on a body | Mostly. The struck one gets all of it; the striker gets a freeze and a knocked hand | Leave. A blade that cuts through gives little back |
| Sword on a shield | Partly. The blocker gets the whole blow; the striker is jarred by 0.3 of it | Look at it in step 5: your "shield blocks are missing this" may include it. It is one lever ("Jar Scale") |
| Sword on a sword | Yes, through the bind | Leave |
| Bash on a body | No | Step 2 |
| Bash on a bash | No, and the numbers are invented | Steps 2 to 4 |
| Bash on a shield held in cover | No: the shield is ignored | Step 2: a bash on a covering shield goes into the shield arm first |
| Two fighters walking into each other | Not an impact: they are only kept apart | Leave, unless one is running (step 5) |
| A leap that lands on someone | Not handled as an impact | Step 5, with the pounce |
| Kicks (step 9b, not built) | - | Build them on the door from the start |

What this means for the roadmap: the door built here is the thing kicks, stumbles, the pounce and combos on a shield were each going to need. Building it now is not a detour from the roadmap; it is the first half of step 9c done properly.

## The plan

Five steps, in this order. Each is behind a Combat Feel switch with an old/new button in the lab, and each is seen on demand with the bash buttons that are already there (small, medium, hard, at the other).

1. **The impact meter.** No change to how anything behaves. After every bash the lab shows one line: the momentum that came in, where it went (each body's ground speed, what the feet passed to the ground, what is unaccounted for), and what each body showed (trunk rock, hips dip, shield arm give). The same line headless. Done when today's countered hard bash reads about 400 N·s unaccounted for. Every later step is judged on this line.
2. **One door for every impact.** The bash hands the other an amount of momentum at the point it meets, the board or the body, through the same door a sword blow uses. The basher gets the same amount back at his shield arm. That brings at once: the trunk rocked, the shield arm giving and the guard knocked open by weight, the body going soft, poise lost, the short freeze, force on the physical body. A bash on a shield held in cover goes into the shield arm first. Switch: "A bash lands: as an impact / as a shove (old)".
3. **A momentum ledger for body on body.** The speeds after a collision are worked out, not written in. In: both weights, both speeds along the line between them. Three levers: "Bounce" (how much they spring apart; small), "Feet take" (how much a set, low stance passes to the ground, never more than grip allows), "Varies" (a drawn share either way). Out: both ground speeds, which the meter checks add up. This replaces this morning's fixed 1.2 and 1 m/s. Worked through for the countered hard bash: the defender is carried back at about 1.7 m/s and the basher is all but stopped (0.4 m/s forward). That takes the defender driving into it at 1.2 m/s, which is 96 N·s against the basher's 424; the ground taking 150; and a bounce of 0.2. With no counter the defender leaves at over 3 m/s.
4. **The bodies absorb it.** The collision gets a shape in time, about a fifth of a second in and half a second out. In: both press into the boards, knees give and hips drop (more for more energy), the shield arm is driven back to the chest and folds, the trunk folds forward over it, the head lags. Out: the trunk rocks back, the head whips, the feet slide a hand's width before the first step, everything settles slowly. Small, medium and hard differ by size, not by kind. Levers: "Contact time", "Knees give", "Arm folds", "Settles in". This is the step you will judge by eye, so it comes with a slow-motion button.
5. **Off balance, and the rest of the game.** What the ledger leaves each of them with, plus a drawn sideways share and twist, goes to the rule the legs already have: sway, a step in the stance, stumbling back so many steps, or down. Either of them, or both. "Counter holds" keeps its meaning (he keeps his feet or not) but no longer means "nothing happens". Then the same door for the rest: how much a sword striker is jarred by a shield, a leap landing on someone, and kicks built on it from the start.

**What you get after each step.** After 2: bashes that visibly land. After 3: a hard bash that is plainly harder than a medium one, and a counter that costs both men ground. After 4: the look you asked for. After 5: the unpredictability, and the hook for stumbles.

**Targets I will hold it to** (headless, on the meter): unaccounted momentum under a tenth; on a held hard counter both trunks move at least 15 degrees and both hips drop at least 8 cm, against 1 to 5 degrees and 0 to 3 cm today; small, medium and hard come out in that order on every measure.

**Risks.**

- The door was tuned for sword blows of 20 to 50 N·s. A 400 N·s collision will hit its ceilings (the trunk's rock is capped). A body collision needs its own, longer profile rather than one huge kick: that is why step 4 is its own step.
- The physical upper body is held firmly to its pose. Large forces on it have caused arms through bodies before. The meter will count that too.
- Looks cannot be judged headless. Steps 2 and 4 need your eye before I tune further.

## Open questions for you

- [ ] **Does a counter that holds still cost the defender ground?** The ledger says yes: he keeps his feet but is carried back a step or two. The alternative is that he stands his spot and only his body takes it.
- [ ] **The basher after a held counter:** stopped dead against the board (what the arithmetic gives), or thrown back a step (what it does now)?
- [ ] **Sword on a shield:** do those blocks feel weak to you as well, or was "shield blocks" only about the bash? If they do, I look at them in step 2 rather than step 5.
- [ ] **How unpredictable:** a mild draw (the same collision comes out a fifth different each time) or a wild one (now and then the stronger man is the one who goes down)?
- [ ] **Start with steps 1 and 2 now?** They are the quick ones and they already put a visible reaction on every bash.

## Sources

- Hendricks and others, [Momentum and kinetic energy before the tackle in rugby union](https://pmc.ncbi.nlm.nih.gov/articles/PMC4126292), Journal of Sports Science and Medicine, 2014: masses, speeds, momentum and energy in 60 filmed tackles; who dominated.
- Seminati and others, [Specific tackling situations affect the biomechanical demands experienced by rugby union players](https://research.tees.ac.uk/en/publications/specific-tackling-situations-affect-the-biomechanical-demands-exp/): peak shoulder forces of 2.4 to 3.4 kN; muscles switching on about 0.3 s before impact.
- [A Model Predictive Capture Point Control Framework for Robust Humanoid Balancing via Ankle, Hip, and Stepping Strategies](https://arxiv.org/html/2307.13243v2): the three ways of surviving a push and the capture point.
- Peyman Massoudi, [The role of animations in hit effects](https://gamedeveloper.com/programming/the-role-of-animations-in-hit-effects), Game Developer, 2016: no blend into a hit reaction, fast in and slow out, shakes on the attacker, hit pauses, procedural spine on blocks.
- The project's own code and this morning's headless runs: `ClipFighter.TakeHit`, `Jar`, `Bashed`, `StrikeHits`; dances `b5`, `c12`, `b3` and the duel lab bash runs of 18 November.

The contact time of a quarter of a second, the ground's share of about 150 N·s, the worked ledger speeds and the capture point distances are my own arithmetic from those figures, not quoted results.

## Progress, 18 November (midday): steps 1 and 2 built

Steps 1 and 2 are built and pushed, headless only; where the two men end up is unchanged, which is step 3.

**Renier's answers (11:54).** A counter that holds still costs the defender ground: yes. The basher is thrown back a step for a medium and a hard bash; a light one is my call (he rocks on the spot; the ledger will work it out). Sword blocks on a shield feel weak too and are to be looked into. And no invented unpredictability: only the variation the real collision gives. So nothing in these steps is drawn by lot.

**Step 1, the meter.** After every bash that lands and every blow stopped on a shield, one line: what came in, what is left as motion, what is unaccounted for, and what each body showed. Each body gets two readings: the reaction's own share (how far it rocked his upper body and drove his arms back), and everything together (his line from hips to head, his head, his hips). In the lab: "Impact meter" under "Last bash".

**Step 2, the door.** A bash hands over its momentum at the point the board really met him: upper body rocked about the hips, body bent there, shield arm driven back and guard knocked open if it met his shield, poise lost, a short freeze for both. The basher takes the same back at his own shield arm. A bash now sees what it meets: body, shield in cover, or counter. Switch: "A bash lands: as an impact / as a shove (old)".

**Sword on a shield.** The blocker was always rocked (about 20 degrees for a 23 N·s cut). The striker was rocked 3 degrees; he now takes all of it back and is rocked 17. Switch: "A block jars: the striker by all of it / by a part (old)". In every fight.

| Bash | Reaction's own rock, both men | The one who took it, hips to head | His head over his hips |
| --- | --- | --- | --- |
| Small | 10 to 11 degrees (was 0 to 1) | 6 degrees (was 2) | 6 cm (was 2) |
| Medium, on his shield | 27 to 28 (was 0) | 18 (was 5) | 18 cm (was 5) |
| Hard, on his counter | 37 to 38 (was 0 to 1) | 18 (was 7) | 17 cm (was 6) |

**A correction to the figures above.** The first measurements in this document took the trunk's lean on the line from hips to chest, which shows about a quarter of what the spine and chest turn together. The meter uses the line from hips to head. The table here is old against new on the meter's measure; it is not to be compared with the earlier table, and the target of "15 degrees" was set on the old measure. On the meter's measure the countered hard bash went from 7 to 18 degrees.

**Not done.** The counter's speeds are still written in: the meter reads about 290 N·s unaccounted for on a countered hard bash (step 3). The hips do not drop and the feet do not slide (step 4). On a countered hard bash the one who countered has a hand or forearm inside his own body for about a tenth of a second; in the fight of six it is 4.1 s in all against 3.1 s. The fight of six goes another way with strikers jarred and has not been looked at.

## Progress, 19 November (overnight): steps 3 and 5 built, headless only

Renier, 18 November, night: stumbles first, the ledger before them if they need it. They do: the basher cannot be thrown off balance until what comes back at him is worked out.

**Step 3, the momentum ledger.** Built as planned, less the "Varies" lever (no dice). Switch: "A bash's speeds: worked out / written in (old)"; levers "Impact bounce" (0.2) and "Feet take" (1). One change from the worked example above: only a man who has set himself (a counter) passes anything to the ground, by how many of his feet are down. A man who has not is thrown by all of it, and what his feet can do about that is the stumble's business. The countered hard bash, measured: 305 N·s handed over, 78 N·s to the ground (one of his feet was down), the defender carried back at 1.8 m/s, the basher all but stopped (0.3 m/s); nothing unaccounted for, where it was 294 N·s. **A counter is no longer a draw:** he keeps his feet if his legs catch him.

**Step 5, off balance.** Built as its own model (`StumbleSteps`): a weight on legs that tips away from its feet and is stopped only by a foot put down ahead of it. Switch: "Off balance: real stumble steps / the walk played fast (old)". Either man can be thrown off balance; both go through one door (`ClipFighter.ThrowBody`), which a sword blow that throws a body back uses too and kicks are to use. No sideways share and no twist are drawn: the twist is the impulse's own lever about his hips.

| Bash | New, headless |
| --- | --- |
| Small | leaves at 0.7 m/s: one stumble step, 0.17 m |
| Medium | leaves at 2.2 m/s: two stumble steps over 1.1 m, on his feet; the basher takes two small steps |
| Hard, countered | carried back at 1.8 m/s: one stumble step, 0.42 m; the basher all but stopped |
| Hard, not countered | thrown at 3.4 m/s: no foot gets under him, down after 1.9 m |

**Step 4, bodies that absorb: not built.** What there is of it comes from the stumble: his knees give as each foot takes his weight, his hips sink while his feet are behind him, his upper body tips with his weight. The quarter second in which the two press into the boards, the arm folding and the feet sliding are still to do. The targets set above for a held hard counter (trunks 15 degrees, hips down 8 cm) are not met: the defender's hips went down 1 cm in the meter's window.

**Open, for Renier.** Whether an uncountered hard bash should put a standing Medium down every time (as now, at a foot speed of 5) or throw him back three metres on three stumble steps with his feet kept (5.5). Whether the small bash still clears his space (0.17 m against 0.46 m). Details and figures: the README's entry of 19 November.
