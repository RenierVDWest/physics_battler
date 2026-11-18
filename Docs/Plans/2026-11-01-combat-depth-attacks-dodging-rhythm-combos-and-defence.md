# Combat Depth: Attacks, Dodging, Rhythm, Combos and Defence

> Copy of the Claude document <https://claude.ai/artifact/4BijSWun94DiWFcjcYCRJp>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 1, 2026 · @Renier

## Summary

The fights feel like turn-taking because a fixed clock times the blows, and mood barely touches it. After each blow a unit waits for the blow to finish plus another 1.6–3 s. So no unit ever throws two blows within 1 s of each other. In a pair, the next blow comes from the other side 63% of the time. The postures do play, about 16 a minute, but they move the body no more than the idle loop does, so nobody sees them.

One new idea sits at the centre of every plan: a unit's **tempo**. Energy builds while a unit holds back and gets spent in bursts. Aggression decides what it is spent on. Pressure from the other side's blows pushes a unit into defence. Combos, signature attacks, dodges and the defensive shell all hang off that.

| Plan | What it adds | Built on |
| --- | --- | --- |
| 1 Signature attacks | Two big mocap-led attacks per style: a leaping smash, a spinning advance, a low spin, and more | The mocap pipeline, extended to one-shot attack clips; the aimed weapon stays |
| 2 Dodging | Reactive slips, ducks and sway-backs. The hit test decides whether they work, not a dice roll. Skill means reaction, timing and picking the right dodge | Dodging.fbx, postures, the locked aim |
| 3 Fight Rhythm | Logging first. Then tempo states (gather, probe, press, shell, counter), mood you can see, and postures about three times bigger | RhythmPlayer, FightLoop |
| 4 Combos | Two strings per style, made of chained blows that skip their own wind-up | MoveState, MovePlanner |
| 5 Defensive posture | A pressure meter, a shell guard for each style, giving ground, guard breaks and counters | Blocks, poise |

The build order is M7 measure and free roam, M8 tempo, M9 combos, M10 defence, M11 dodging, M12 signature attacks. The plans were agreed on 1 November 2026, and M7 is under way.

## What the logs say now

The postures do play, but they are too small to see, and the blows come on a clock. These numbers come from four headless 3 v 3 fights (seeds 1 to 4, 55–90 s each, 1,225 unit-seconds on their feet), measured from the playtest logs.

| Body (style) | Walking | In a blow | In a posture | Idle loop only | Postures a minute | Blows a minute |
| --- | --- | --- | --- | --- | --- | --- |
| Large (boxer, club) | 18% | 28% | 19% | 36% | 20.0 | 11.0 |
| Medium (sword and shield) | 45% | 14% | 13% | 29% | 13.7 | 9.6 |
| Small (eskrima, stick) | 57% | 9% | 10% | 24% | 13.6 | 6.8 |

Shares are of the time standing. The rest of the time goes on hit reactions and stumbles.

- **The postures can't be seen.** During a posture, the hand moves against the chest by 5–7 cm (standard deviation). That is the same as the idle loop alone (4–10 cm), against 9–28 cm in a blow. Hip height varies by 1–5 cm in both. The jab feint is the one you notice, and it is the only posture that pushes a hand 30 cm.
- **Walking hides them.** Small and medium units walk for 45–57% of their standing time, and walking blocks postures.
- **Mood changes nothing you can see.** Energy only drains, from 1.0 to about 0.5 over a fight, and it comes back at 0.0015 a second. Aggression wanders between 0.3 and 0.9. All they do is nudge the chance of a lead-in posture, which can hold a blow back by the posture's length (0.4–1 s). Neither one ever makes a unit attack sooner or chain blows.
- **The blows run on a timer.** A unit's own blows come a median of 4.8 s apart (10th percentile 3.5 s, 90th 11 s), and none come within 1 s, so there are no combos. A blow takes 1.1–2.1 s, and then the FightLoop waits a random 1.6–3 s. Every style uses that same interval.
- **They take turns.** Within a pair, the next blow is the other unit's 63% of the time; even chances would give 50%. 30% of blows are thrown while the target is itself mid-swing.
- **Only the shield blocks.** The medium unit blocks with a 0.55 chance after 0.22 s. The other two never defend. Blocks aren't logged as events of their own, so they show up only as "stopped" blows: 96 of 194 blows were stopped by the target.

## Research

Good fighting games treat tempo as a shared resource. Offence spends it, defence has a cost, and reactions have a human-sized limit. Each finding below shaped a choice in the plans.

| Source | What it shows | What we take |
| --- | --- | --- |
| [On Reaction Times](https://ki.infil.net/reaction.html) (Killer Instinct guide) | A typical reaction to one clear cue takes about 265 ms (16 frames). Faster moves need a read. With several possible threats, even slow moves get hard to answer. Breaking a light combo link usually needs a guess. | Reaction times of 0.18–0.35 s depending on skill. A blow whose wind-up is shorter than the reaction plus the dodge can't be dodged on sight. The later links of a combo are harder to answer. |
| [SF6 Drive system](https://words.infil.net/w04-sf6review-p2.html) | One gauge feeds both offence and defence. Blocking drains it. Running out (burnout) leaves the fighter open for about 20 s. Players call it a second health bar. | Energy pays for blows, combos, dodges and blocks. Running dry means slower blows and a sagging guard. |
| [Sekiro posture](https://samurai-gamers.com/sekiro-shadows-die-twice/posture-system/) | Blocked blows fill a posture bar, and it recovers more slowly when hurt. When it breaks, the fighter is wide open. | A shell guard can be broken by a long barrage, so soaking costs something. |
| [For Honor terms](https://www.ubisoft.com/es-mx/game/for-honor/news-updates/6XEM2qUiwcokp753uCzgkz/for-honor-terminology-guide) | A well-timed dodge avoids a blow entirely. Revenge builds when a lone fighter is attacked. Out of stamina, attacks come out much slower. | Dodges succeed or fail on their timing. Being pressed builds toward a counter. |
| [Sword and round shield tactics](https://www.science-and-fiction.org/swordfighting/sword_and_shield.html) | The main sequence is threat, pin, thrust, retreat: cut high so the shield rises, pin it with your own, thrust low, step back with the shield high. Also the second-intention attack, and stepping to the shield side. | The medium unit's combos and its shield bash. |
| [FMA glossary](https://pdfcoffee.com/fma-glossary-pdf-free.html) | *Abanico* is the fan strike. *Banda y banda* is side to side. *Cardena* links techniques. *Doble* is two quick strikes. *Sero-sero* is wide circling strikes while advancing. *Rompida* is an X of up and down. | The small unit's combos, named for real eskrima strings. |
| [Boxing combinations](https://evolve-mma.com/blog/15-basic-boxing-combinations-you-should-master-first/) | 1-2, 1-2-3, and 2-3-2 (angles that are hard to block). 1-6-3: the uppercut drops the guard for the hook. | The large unit's combos: the off-hand jab as the 1, the club as the power hand. |
| [Hellish Quart review](https://pcgamer.com/hellish-quart-is-the-most-authentic-sword-fighting-game-in-years) | In a physics sword duel, blocks are automatic, but blocking too much lets a cut through. | Blocks and the shell aren't walls; physics decides what gets through. |
| [Bannerlord combat AI](https://www.gamebanshee.com/3ua66) | Better bots understand timings, decide faster and vary slightly. Defensive orders make them careful, but they still take openings. | Skill is a set of levers (reaction, timing, reading) on each unit. Temperament stays with the style. |
| [Motion Warping](https://dev.epicgames.com/documentation/en-us/unreal-engine/motion-warping-in-unreal-engine) (Unreal) | Root motion is warped inside a window of the clip, so a canned attack lands on a target. | Signature attacks keep the clip's body and bend its travel and the weapon's path onto the aim point. |

## The reference clips

The twelve clips give us three things: body motion for the big attacks, beat timings for the combos, and one clean dodge to build the evasions on. The two Dodging files are byte-identical. Everything was read with the mocap tools at 30 keys a second, in leg lengths (the Mixamo leg is 0.89 m). A "strike" is a hand moving faster than 8 legs a second.

| Clip | Length | What the body does | Strikes (s) | Used for |
| --- | --- | --- | --- | --- |
| Mutant Jump Attack | 3.7 s | Crouches for 0.4 s, leaps 2 legs forward with the hips 1.9 legs up, smashes two-handed overhead on landing, then crouches to recover until about 3.0 s | 1.55 (both hands, down) | Large: Leaping Smash |
| Standing Melee Attack Horizontal | 2.4 s | A 140° chest wind-up, then one flat swing; the feet barely move | 0.93 | Large: Haymaker |
| Standing Melee Attack 360 Low | 2.3 s | A full 360° pivot on the spot, swinging low at the knees on the way round | 0.90 | Small: Spinning Low Redonda |
| Sword And Shield Attack | 1.7 s | Spins 405° while advancing 2.7 legs, hips dropping 0.3 legs, and thrusts at the end of the turn | 0.73 | Medium: Spinning Advance |
| Standing Melee Attack Backhand | 3.2 s | A big step in (1.4 legs), a 230° chest turn into a rising backhand, then a step back out | 0.80, 1.03 | Medium: Backhand Riposte |
| Standing Torch Melee Attack 05 | 3.2 s | Two driving steps forward (1.8 legs), a falling cut, then a rising thrust | 0.73, 1.63 | Small: Lunging Doble |
| Dual Weapon Combo | 3.6 s | Six strikes from alternating hands with a long side step, 0.15–0.7 s apart | 0.70, 1.07, 1.77, 2.17, 2.53, 2.67 | Small: flurry timing |
| Two Hand Sword Combo | 3.3 s | Three heavy blows (across, down, down), 0.7–0.9 s apart, circling 2 legs | 0.55, 1.25, 2.16 | Medium: three-beat combo timing |
| Melee Combo Ver. 1 | 4.7 s | A 1.7 s slow build-up, then three heavy blows advancing 1.6 legs | 1.95, 2.12, 2.95 | Large: pressure-chain timing |
| Melee Combo Ver. 2 | 4.2 s | Four blows with a step in and a step back out | 0.91, 1.92, 2.45, 2.62 | Small and medium: in-and-out strings |
| Melee Combo Ver. 3 | 2.7 s | Two quick thrusts with a turning backhand between them, no travel | 1.00, 1.73 | Medium: double thrust |
| Dodging | 1.1 s | Slips right. The right foot steps 0.29 legs out (0.13–0.34 s), and the head moves 0.49 legs sideways and 0.27 legs down, furthest at 0.54 s. Back in place by 1.0 s | none | Every dodge; the timing model |

The dodge sets the timing for Plan 2. The head is 0.15 legs (13 cm) off line after 0.27 s and 0.38 legs (34 cm) off after 0.40 s. So a dodge has to start about 0.3 s before contact to clear a head-sized target.

## Plan 1: signature attacks

Each style gets two signature attacks. The body comes from a mocap clip: steps, spins, leaps, two strikes in one move. The weapon is still aimed, and the sweep test still decides what it hits. Today every blow is one programmatic arc, a wind-up and a swing with some lunge. That is why they all look alike.

| Style | Attack | From clip | What it does | Risk it carries |
| --- | --- | --- | --- | --- |
| Large | Leaping Smash | Mutant Jump Attack | Crouches, hops forward 1.5–2.5 m, and smashes two-handed overhead on landing. Hits hard enough for a knockdown | 0.5 s open on landing; the crouch is a clear tell |
| Large | Haymaker | Melee Attack Horizontal | A 0.9 s wind-up with the chest turned 140° away, then a flat swing at head height that knocks back | The long wind-up can be ducked |
| Medium | Spinning Advance | Sword And Shield Attack | Closes about 2 legs while spinning, the shield leading, and thrusts at the end of the turn | Its back is turned for about 0.2 s mid-spin |
| Medium | Backhand Riposte | Melee Attack Backhand | Steps in with a rising backhand and steps back out. A punish after a block or a dodge | Overreaches if the target has moved |
| Small | Lunging Doble | Torch Melee Attack 05 | Two driving steps, a falling cut, then a rising thrust: two strikes in one move | 3.2 s long; a dodge of the first strike leaves it stranded |
| Small | Spinning Low Redonda | Melee Attack 360 Low | A full spin, sweeping the knees on the way round. Staggers the legs | Back turned, and low to the ground |

### How they are built

1. **Attack clips from mocap.** The extraction tool gains an `--attack` mode beside `--gait`. It keeps the root's travel and turn, which spins and leaps need. It also writes the hips, turns, lean, head, both hands, each foot's lift and land, and the strike windows (hand speed peaks, editable). The output is a one-shot `AttackClip`, written as generated C# like the loops.
2. **A new move shape, `Clip`.** The move's body layer plays the clip in place of the made-up lunge and twist. Inside each strike window the weapon blends out of the clip's hand path and into an aimed swing or thrust (the existing maths), so `TryOut`, `Plan` and `Acceptable` keep working.
3. **Warping onto the target.** As in Unreal's Motion Warping, the clip's travel is scaled (0.6–1.3×) so that the strike lands at the target's distance. Its turn is bent so that it faces the target at contact. Outside the warp windows the clip plays as captured.
4. **Real steps.** The clip's foot lifts and lands become `Stance.StepTo` calls, as they do for the walk, so travel is stepped and never slid.
5. **Several contacts in one move.** `MoveState` gets a list of contacts, each with its own aim part and its own sweep. The Lunging Doble aims its cut and its thrust separately.
6. **Commitment.** Each signature attack carries opening windows (posture-style data), a long recovery and a big energy cost (Plan 3). Once its drive has started it can't be cancelled.

### When they come out

As gap closers from 1.5–2.5 m, where today the FightLoop only walks in. As combo finishers (Plan 4). As punishes when the target is staggered or has just whiffed. And as the way to spend a full energy bar during a press.

### Checks

Unit tests: clip sampling, warping maths, and the contact list. The aim matrix runs every signature attack against a dummy at its range, aiming for no misses and no swings through its own body. The balance runner checks that the pelvis never drops more than 0.25 legs and that nothing falls. A video of each attack on each body goes to you before tuning.

### Risks

- **Room to land.** A spinning advance or a leap travels 2–2.7 legs. With free roam (M7) there is no tile to leave, but the landing spot must be clear of bodies and inside the board. If it isn't, the travel is warped short.
- **The leap.** The clip lifts the hips 1.9 legs, and the physical rig would have to come off its pins and land. The plan is a long, low hop: about 0.35 legs high but still 1.5–2.5 m long. It is tried first as a spike behind its own gate. If the hop fails, the fallback is a two-step charge into the same overhead smash.
- **The spins.** A 360° root turn goes through the unit's facing logic, so facing must follow the clip during the move, not the target.
- **Commit points.** Clip moves have no drive start today. Each one gets a commit point taken from its strike window (Plan 2 needs it).
- **Mapping the hands onto our rig.** The same arm-length mapping as the idles (`--rig`) should hold, but each attack gets checked on all three bodies.
- **A spec decision.** The spec says "clips only for idle, locomotion and get-ups". Clip-led attacks keep the weapon aimed but change that decision, so they need your OK.

## Plan 2: dodging as a skill

A dodge is a real movement of the body, started in reaction to a blow. The sweep test decides whether it worked; no dice roll does. Skill sets how fast a unit sees the blow, how evenly it times the dodge, and how well it picks the right one. A clean dodge leaves the attacker over-committed and open, and that turns the dodge into a counter. That is what makes it count in a fight.

### When a unit may try

- A blow is coming at it, and it isn't in a blow of its own (your rule).
- It is on its feet and not stumbling, knocked or in hit stop. The blow starts within 60° of where it faces; a blow it can't see, it can't dodge.
- It has the energy (about 0.12) and room around it, clear of bodies and the board's edge, and it hasn't dodged twice in the last 3 s.
- Then it tries with a chance: skill × style × how fresh it is × its tempo state. The shell from Plan 5 makes dodges likelier, and so does a read of the blow. Overall, at most about a quarter of incoming blows should draw a dodge, so it doesn't happen every time.

### The timing model

Today the aim locks when the blow starts. If the lock simply moved to the start of the drive, the window would be close to zero. Measured from the move assets on the medium body, the drive starts 0.20 s (thrust) to 0.37 s (smash) before contact, and a dodge needs about 0.3 s to clear the head. So the plan has three parts:

- **Commit earlier.** Each move gets a **commit point**: the end of its visible tell, set about 0.55–0.75 s before contact (later for the jab, earlier for the Haymaker and the Leaping Smash). Until then the attacker **tracks** the target. This tracking is cheap: the locked path is shifted by how far the aimed part has moved, the way warping shifts a clip. There is no re-plan every frame, and the lunge's feet are placed at the commit point, not before it.
- **A snappier slip.** The clip moves the head 13 cm in 0.27 s. Played 1.3× faster for the first half, it clears a head-sized target in about 0.2 s (×√size on bigger bodies).
- **The window.** It runs from the commit point to contact minus the clear time, which comes to about 0.25–0.5 s for most blows. Started too early, the dodge gets tracked and the blow lands anyway. On time, the part is off line and the blow whiffs. Too late, the blow lands, often on another part, such as the shoulder instead of the head.

The dodge starts at the blow's start, plus the unit's reaction time, plus a timing error drawn from its skill. Skilled units wait for the commit, and poor ones go as soon as they react. Quick blows like the jab leave almost no window and can only be dodged on a read; telegraphed blows leave a wide one. M7 measures every move's commit-to-contact time, so the numbers above get checked before any dodge is built. Feints get a hook of their own (a threat event like `OnIncoming`), so a feint can bait an early dodge. This changes a decision in the spec ("the aim is locked where the part was when the attack started"), and that needs your OK.

### The dodges

All four are built from Dodging.fbx, reshaped and mirrored. They play as an **evasion layer** of their own, not as director postures. Postures can't start while a unit blocks, is in hit stop or has a blow fading, and they fade out when the unit stops being free. A dodge has to work in exactly those moments. M11 builds the slip and the sway back first. The duck and the side step follow once those two pass their gate.

| Dodge | Shape (from the clip) | Beats | Loses to | Styles |
| --- | --- | --- | --- | --- |
| Slip (left or right) | The head 0.3–0.45 legs sideways and a little down; the near foot steps out | Thrusts and vertical blows (smash, cleave) | Flat swings | Large, Small |
| Duck | Crouch 0.25 legs and lean in (a deeper bob) | Flat swings at head height (cut, Haymaker) | Overheads and low blows | Large |
| Sway back | The rear foot steps back 0.3 legs and the body leans back 15° | Blows at the edge of reach | Lunging thrusts, the Leaping Smash | All three |
| Side step | A triangle step out of line | Thrusts and smashes | Spins and wide sweeps | Small |

The blow's plane and height decide which dodge is right. A unit's read skill is its chance of picking that one; otherwise it picks one of its style's dodges at random. A wrong pick usually still gets hit, and the physics shows why.

### What a dodge leads to

- **Clean**: the attacker whiffs. Its recovery stretches 30% and it opens (head and chest for 0.4 s), so a dodger that reads openings counters at once, with a Backhand Riposte for the medium unit. A whiff also breaks a combo (Plan 4). Aggression rises for the dodger and falls for the attacker.
- **Glancing**: the blow lands on another part, usually one that takes less damage than the head. Hit speed today is the weapon's own speed, not its speed against the body, so a dodge moving away doesn't soften the blow by itself. M11 can switch damage to the relative speed if glancing hits should count for less.
- **Caught mid-dodge**: with a foot in the air the unit is easier to unbalance, so poise damage is ×1.3.
- **The cost**: energy, plus no other dodge for 0.6 s.

### Skill on the unit, flavour on the style

Skill moves to a new `CombatSkill` block on each `UnitProfile`, so two boxers can differ. The style keeps the flavour: which dodges, the base willingness, block or dodge.

| Preset | Reaction | Timing spread | Read | Dodge willingness | Block |
| --- | --- | --- | --- | --- | --- |
| Rookie | 0.35 s | ±0.12 s | 0.3 | 0.3 | 0.35 |
| Fighter | 0.27 s | ±0.08 s | 0.5 | 0.5 | 0.55 |
| Veteran | 0.22 s | ±0.05 s | 0.7 | 0.7 | 0.7 |
| Master | 0.18 s | ±0.03 s | 0.9 | 0.85 | 0.85 |

The existing `blockChance`, `reactionTime` and `readsOpenings` move into this block (data migration 18). Composure, used in Plan 5, joins them.

### Checks

- **A `dodge` runner.** A dummy attacker throws each move 50 times at a dodger of each preset, giving a table of clean, glancing and hit. The target bands are set once M7 has measured the real windows. The first guess is 15–25% clean for a Rookie's attempts and 50–65% for a Master's. Early dodges must get tracked, and no dodge may run into another body.
- **Unit tests** for the window maths and the choice table.
- **Logged events**, for example: "dodge: slip right vs ClubSwing at the Head; started 0.42 s in (lock 0.38, contact 0.72); missed by 14 cm; clean; counter opened".

## Plan 3: making the Fight Rhythm show

The fix has three parts. First, measure what shows. Then put the blows under a tempo that mood drives: gather, probe, press, shell, counter. Last, make every state and posture big enough to read from across the board. The fixed attack clock goes. A unit attacks when its tempo says so.

### 1. Measure first (M7)

The playtest logs and `analyze.py` gain a rhythm section for each unit:

- the share of time in each tempo state, and a timeline with state bands under the energy and aggression curves;
- the postures played, by name, each with its **extra motion**: peak displacement of the hands, head and hips beyond the idle loop, in cm. A flag goes up under 10 cm;
- the energy cycles (charge and spend), the bursts (blows under 1 s apart), the gaps between blows, and the alternation rate within each pair;
- blocks and dodges as events of their own, with their outcome.

The `fight` runner prints the headline figures, and the fight video draws an energy bar and the state over each unit. The numbers in *What the logs say now* become the baseline.

### 2. Tempo states

&#91;embedded content: tempo states · five states and what moves a unit between them\]

| State | What the unit does | Enters when | Leaves when |
| --- | --- | --- | --- |
| Gather | Holds just outside reach (engage distance + 0.3 × size), circles, settles its guard. Recharges | It has just spent a burst, it's hurt and losing, or it's out of energy | Energy reaches its probe level, or an opening shows |
| Probe | Single blows and feints. The tempo decides each moment whether to throw; today's wait (the blow's length plus 1.6–3 s) is gone, or turn-taking would come straight back | Energy is moderate | Energy and aggression reach the press level, or energy runs low |
| Press | Closes in and chains blows, combos (Plan 4) and signature attacks (Plan 1), with gaps of 0.15–0.5 s | Energy ≥ the style's press level and aggression ≥ 0.5, or the target is staggered or badly hurt | Energy < 0.2, or the burst is broken (blocked twice, dodged, or it takes a heavy hit) |
| Shell | Defends (Plan 5) | Pressure received ≥ its composure | The barrage stops |
| Counter | One quick blow or a riposte at once | It blocked, dodged or soaked a burst that has just ended, and has ≥ 0.25 energy | After one blow or string, back to probe or press |

While one unit gathers and the other presses, the fight has a shape: a burst, a soak, a lull, an answer. When both gather, they circle, and that tension is the point. Turn-taking fades because the next blow belongs to whoever has the energy and the mood, not to whoever waited longest.

### 3. Energy economy

Energy now really cycles. It charges quickly when a unit holds back and gets spent in bursts. These starting numbers will be tuned in M8.

| Event | Energy |
| --- | --- |
| Gathering: standing or circling at range | +0.20 a second |
| Walking to a target | +0.05 a second |
| In the shell, under pressure | +0.02 a second |
| A probe blow | −0.08 |
| A press blow or a heavy combo link | −0.12 |
| A light link (jab, fan strike, punyo) | −0.08 |
| A signature attack | −0.35 |
| A dodge | −0.12 |
| A block or parry | −0.04 |
| A hit taken | −0.05 to −0.20 (by poise damage) |

A press ends when energy falls under 0.25, so the floor (`minEnergy`, 0.15 today) drops to 0.05 and is reached only by soaking or taking hits. Gathering from 0.25 back to full takes about 3.75 s. That refill pays for four heavy links (0.12 each, down to 0.25) or six light ones, and each style's burst cap sits about there. Below 0.3 energy, blows are 15% slower, the guard sags (as it does now), and the dodge timing spread grows 1.5×.

Aggression rises when a unit lands blows, when the target is hurt, and when it has more energy than the target. It falls when it is hit and when its health is low. It drifts back to the style's temperament. Aggression lowers the press level and lengthens bursts.

### 4. Mood you can see

Each state gets a continuous stance layer, blended over about 0.3 s, plus its own base-loop speed and size. A one-off posture can't carry mood; a stance held for seconds can. Today the rhythm layer is weighted by 1 minus the blow's blend, so a press stance would vanish for exactly the blows it is meant to colour. The stance layer therefore sits under the move and stays at full weight through blows.

| State | Body | Idle loop | Postures allowed |
| --- | --- | --- | --- |
| Gather | Sits 0.05 legs lower, weight back, guard tucked in, chin down | 0.8× speed, deeper breathing | Settle, guard change, peek, measuring steps |
| Probe | The stance as now | 1× | Feints, footwork |
| Press | Leans in 6°, weight forward, hands higher and further out, chest squared | 1.25× speed, bigger bounce | Lead-in feints, a stalking step-in |
| Shell | Crouches 0.08 legs, guard high for its style, leans in behind it | Small and quick | Defence only |
| Counter | Snaps out of the shell, weight forward | none | none |

### 5. Postures that show

- **An amplitude pass.** Every posture must move a hand at least 12 cm, the hips at least 0.06 legs, or the turn at least 12° beyond the idle loop. The new logging checks this. Most of today's postures roughly double or triple.
- **Over walking.** Upper-body postures (feints, guard changes, peeks) layer over the walk at close range. Only footwork postures wait for the feet.
- **Postures per state.** Gather gets settle and measure postures. Press gets the stalking step-in and uses feints as lead-ins. Shell gets the defensive ones.

### 6. Temperament per style

| Style | Gathers for | Presses at energy (from 0.25 at 0.2 a second) | Press needs aggression | Burst | Shells at pressure | Character |
| --- | --- | --- | --- | --- | --- | --- |
| Large (boxer) | 1.5–3 s | 0.55 | 0.4 (its base is 0.65) | 3–4 heavy links, walking forward | 0.7 (hard to rattle) | Pressure fighter; stalks |
| Medium (sword and shield) | 3–4 s | 0.85 | 0.45 (its base is 0.5) | 2–3 heavy links | 0.4 (shells early) | Patient; counters off blocks |
| Small (eskrima) | 1.5–2.5 s, circling | 0.6 | 0.45 (its base is 0.55) | 3–6 light links | 0.5 (would rather dodge) | Timing; in and out |

### Checks (the M8 gate)

Alternation within a pair ≤ 55%. At least 25% of blows land in bursts (under 1 s apart). Each unit runs at least two charge-and-spend cycles per minute of fighting. Gather spells of 2–5 s can be seen in the video. Every posture shows at least twice the idle loop's extra motion. Tempo maths and transitions get unit tests.

## Plan 4: combos

A combo is a string of chained blows. Each link starts from the last one's follow-through and skips its own wind-up, so the links come 0.4–0.7 s apart instead of the 3.5 s or more between blows today. Combos come out when a unit has the energy and the mood: during a press, as a counter, or to punish a stagger. A unit that gathered to full energy gets its longest string and a finisher.

### How a link chains

1. **One combo state owns the pose.** Today `Pose.Move` holds one blow, a fading blow is dropped, and `StartMove` refuses while a blow is under way. A `ComboState` takes the move slot and hands it from link to link. For about 0.1 s it blends the outgoing link's arm, turn and lean into the incoming link's.
2. **The lunge is banked at each link.** A lunge moves only the pelvis; the root stays put. Cutting a link at contact would snap up to 1.4 m of lunge back. Instead, at each link-out the root (and the stance's home) moves to where the pelvis has gone, and the feet have already stepped there. The next link plans from the new root.
3. **Turns carry over.** The next link's twist starts from the outgoing link's end turn and not from its own wind-up angle, so a club swing into a backhand doesn't jump 150°.
4. **Link-out and link-in.** Each move gets a link-out point just after contact, where a chained move is cut and skips its recovery. It also gets a link-in point, where a chained move starts. Today a swing's blend is complete a third of the way into its wind-up, so link-in is defined on the swing's progress, not on the blend.
5. **The next link is planned during the current one,** aimed when it starts. If the target has given ground by up to 0.5 m, the link adds a step in. If it went further, the combo ends.
6. **Only the last link has a full recovery,** so a whole combo carries the commitment of one blow.

### The new moves combos need

Four of the nine strings below rest on moves that don't exist yet. So M9 comes in two steps. **M9a** builds chaining and one string per style from today's moves: body then head (Large), double thrust (Medium), and rompida (Small, built from the rising and swinging club blows). **M9b** adds the new moves and the strings that need them.

| New move | Why it is new | Needed by |
| --- | --- | --- |
| Club backhand | The club has only a swing, a smash and a rising blow | Large 1-2-3 and 2-3-2, small banda y banda |
| Fan strike (abanico) | A short wrist-led arc, about 0.25 s from link-in to contact. Today's club moves need 0.5–0.7 s | Small flurry |
| Off-hand jab | The sweep checks only the main weapon, and the swing path is right-arm only | Large 1-2 and 1-2-3 |
| Punyo (butt strike) | `StrikeWith` has no butt end | Small flurry finisher |
| Shield bash | The shield has no moves and does no damage | Medium threat, pin, thrust |

Off-hand and shield strikes need a second sweep, on the off hand. That is the largest single job in M9.

### Combo data

A `ComboDefinition` holds an ordered list of links. Each link sets its move, the parts it prefers, its gap, and what happens if it is blocked: continue, switch branch, or stop. It also holds the energy needed and an optional finisher. It belongs to the style, beside its Fight Rhythm.

### The strings

| Style | Combo | Links | If blocked | Timing reference |
| --- | --- | --- | --- | --- |
| Large | 1-2 | Off-hand jab to the head (new quick move: today's jab feint made real) → club swing at the head | Keeps hammering | Boxing 1-2 |
| Large | 1-2-3 to the Smash | Jab → club hook → backhand club return → Club Smash or Haymaker | Switches angle (2-3-2) | Melee Combo Ver. 1 |
| Large | Body, then head | Rising club to the belly or knee → club swing at the head | Stops | Boxing 1-6-3 |
| Medium | Threat, pin, thrust | High cut at the head → shield bash (new move: pins the shield and arm, costs poise) → low thrust at the belly → step back, shield high | Moves on to the pin; the high cut is meant to be blocked | Sword and shield tactics |
| Medium | Second intention | High cut → a small step back → thrust at the head once the guard drops (a 0.6 s wait that reads the opening) | Waits for the opening | Sword and shield tactics |
| Medium | Double thrust | Thrust → turning backhand → thrust | Stops | Melee Combo Ver. 3 |
| Small | Abanico flurry | 4–6 fan strikes, side to side, 0.4–0.5 s apart, alternating head and elbows → a punyo butt strike (new quick move) or a redonda | Keeps flowing, since later links are harder to answer | Dual Weapon Combo |
| Small | Rompida | Rising diagonal → falling diagonal (an X) | Stops | FMA glossary |
| Small | Sero-sero | Wide circling strikes while stepping in and out → Lunging Doble finisher | Steps out | Melee Combo Ver. 2 |

### When combos come out

- **Press**: when energy covers at least two links. The main source.
- **Counter**: after a clean dodge or block, a short two-link string.
- **Punish**: when the target is staggered or knocked, a long string with a finisher.
- **After a full gather**: the longest string the style has, with its signature finisher. This is the energy-then-explosion you asked for.

Length = the least of the style's maximum, the energy available ÷ cost per link, and an aggression-scaled count. A new skill lever, link timing, sets how tight the gaps are and how often a link is fumbled.

### How combos break

A dodged link (a whiff) ends the combo and leaves the attacker open; that is the dodge payoff. A blocked link follows the combo's rule. A counter that lands ends it. Running out of energy ends it with the current link.

### Checks

- **Unit tests**: link timing (the next link starts at the last one's link-out, skipping its wind-up) and branching on each outcome.
- **A `combos` runner**: each combo against a standing dummy, a shield-blocking dummy and a dodging one. It reports links landed, time per link, the balance figures and own-body passes.
- **The M9 gate**: in fights, 25–40% of blows come inside combos, the median gap between links is 0.4–0.7 s, and no link swings through its own body.

## Plan 5: defensive posture

A unit on the receiving end of a press builds up **pressure**. Past its composure it goes into its style's shell: guard high, crouched, giving ground, blocking and dodging more. It soaks the barrage and waits for the attacker to run dry, then counters. The shell is not a wall. A long barrage can break it, and angles and low links get past it.

### Pressure

- **Builds**: +0.25 for each incoming blow and +0.1 more for each later combo link. A hit taken adds its poise damage over the unit's maximum poise.
- **Decays**: 0.35 a second once 0.5 s pass without a blow.
- **Shell**: the unit shells when pressure reaches its composure, a new skill lever (the style sets the base; see the temperament table in Plan 3). It stays in the shell until pressure falls back under half of that.

### The shell, by style

| Style | Shell | Defends with | Gives ground |
| --- | --- | --- | --- |
| Large (boxer) | A high shell: both forearms up beside the head, elbows tight over the ribs, chin tucked, crouched and leaning in, the club upright in front | The arms physically cover the head. A blow aimed at the head lands on a forearm instead, so damage drops from ×1.6 (head) to ×0.5 (forearm). Poise ignores the part today, so a guarding forearm gets a block-like poise rule (×0.30 of the blow's momentum instead of ×0.45); otherwise the shell would flinch and stagger. Ducks and slips get likelier | Little. It walks into the pressure |
| Medium (sword and shield) | Shield high and forward, crouched behind it, the sword drawn back ready | Block chance +0.25. After the first link it anticipates, so reaction time is 30% faster on later links | A step back each link |
| Small (eskrima) | Low, light on the feet, the stick across the body | Dodge chance ×1.5, retreating footwork (bait-retreat), and the stick deflects as a new weapon parry | Steps back and sideways, and backs off further when there is room behind |

The large and small units get a **weapon parry**: today only shields block. The club or stick meets the incoming weapon. It has a lower chance than a shield, and more of the blow's poise comes through. Today the sweep skips weapons entirely (the code leaves clashes for later), so a parry is a new kind of contact: weapon against weapon. Together with the guard poise rule, this is the largest job in M10.

### What soaking costs

Each block costs energy (Plan 3), and the shell recharges slowly. If pressure stays above 1.0 while poise is under 30%, the **guard breaks**, as in Sekiro: the unit staggers with its guard flung open, head and chest open for 0.6 s. That is the reward for a long, well-fed barrage. Low links under a high shell, a 2-3-2 change of angle, and the shield bash pin all get past it.

### Giving ground and the counter

Shelling units step back 0.2–0.4 legs per link, and the small unit backs off further when there is room behind. With free roam (M7), giving ground is just steering backwards, checked against bodies and the board's edge, and the presser follows. A press can now push the fight across the board, which is pressure you can see. When the burst ends (finished, whiffed, or the attacker's energy drops under 0.25) and the defender has 0.25 energy left, it enters **counter** (Plan 3). A composed unit can counter in the middle of a combo, between two slow links.

### Checks (the M10 gate)

- While its opponent presses, the defender spends at least 70% of the time in the shell.
- It blocks, parries or dodges 30–60% of combo links, depending on skill.
- Guard breaks happen, but at most about one a minute.
- About 30–50% of ended bursts get a counter.
- **A `pressure` runner**: each combo against a defender of each style and skill preset.
- **Logging**: the pressure curve, shell spells, blocks, parries and dodges within each spell, guard breaks and counters.

## Plan 6: free roam

Once a fight starts, units leave the tiles behind and move freely. Placement before the fight stays on the grid. This comes first, inside M7, because tiles get in the way of gathering at range, circling, gap closers, dodges and giving ground. Building tile-aware versions of those first and removing them later would mean doing the work twice.

The tiles quietly did four jobs, and each gets a replacement:

| Job | Today | With free roam |
| --- | --- | --- |
| Spacing | Tile claims, resettling, the 0.35-tile clamp | Steering: seek an engage point, push away from nearby bodies, stay inside the board. A stuck timer stays as a safety net. The Dijkstra pathing goes, since the board has no obstacles |
| Crowding | 8 neighbouring tiles cap the attackers on one unit | A ring of attack slots around each target at engage distance, sized by the target's body. A full ring sends the next attacker to another target or to wait just outside |
| Bumping | Bodies rarely touch | Separation keeps physical contact between units rare, so the balance system isn't reacting to it all the time |
| Jitter | Tile centres are fixed targets | The steering velocity is smoothed with small dead zones, so the gait doesn't take nervous little steps |

The walk and gait already run off a continuous velocity, so the animation barely changes. The FightLoop tests get rewritten around the new rules: no two bodies overlap, nobody is stuck for more than 2 s, everyone engages, fights end, and mirrored runs stay even.

## Milestones, tests and open questions

There are six milestones, M7 to M12, and each ends at a gate you can check in the logs and in a video. M7 comes in two steps. **M7a** is the logging, run first on today's fights so we have a before picture. **M7b** is free roam, judged by the same logs. M7a also measures every move's commit-to-contact time, because the dodge design depends on it. The tempo (M8) comes next because it is what is most missing; combos, defence, dodges and signature attacks all plug into it. Defence follows combos because it needs a barrage to soak. Signature attacks come last: they are the biggest pipeline job, and they slot into combos as finishers.

A code review against the plan's first draft changed five things. The dodge now needs an earlier commit point (the old lock left a window of about zero). Combos need a combo state that banks the lunge, plus five new moves. The energy numbers were redone so that they add up. The mood stance sits under blows. And shell guards get a poise rule, so they don't stagger.

&#91;embedded content: build order · six milestones, each with its gate\]

M8 is the core. The other four milestones each add one way to spend or answer the energy it introduces.

Every milestone ends the same way: the compile check, all tests, the smoke, aim, swings and balance runners, four fight seeds plus mirrored runs, a video for you, the README and spec doc updated, then commit and push.

### Progress

**M7 is done** (1 November 2026, commit `aaedc78b`). Its numbers come from 12 headless fights: seeds 1–8, plus 4 mirrored.

- **Free roam works.** Bodies never overlapped, fights ended in 27–92 s, and the lineup decides who wins, not the side.
- **Smaller units fight more.** Medium units now throw 14.1 blows a minute (from 9.6) and small ones 9.6 (from 6.8). A longer reach no longer keeps backing away from a shorter one.
- **The tempo problem is now measured, and it got worse.** In a pair, the next blow is the other unit's 76% of the time (63% before), and there are still no blows within 1 s of each other. That fits: units now meet one on one more often, and the attack clock is unchanged. M8 replaces the clock.
- **The postures, as a baseline.** The jab feint (20 cm) and club feint (22 cm) show. The shield press (0 cm), peek (0), shift (3), abanico (5) and triangle step (4) can't be seen.
- **The move windows (`windows` runner).** Drive to contact is 0.15–0.40 s, and wind-up end to contact is 0.16–0.55 s, across the three bodies. This confirms the dodge needs commit points.
- **Still open: the small units.** They spend 63% of their standing time walking and die early, after 12–40 s on their feet, because they arrive last and get ganged. This belongs with temperament in M8.

**M8 is done** (1 November 2026, commit `9ea52030`). Its numbers come from 6 headless fights plus their 6 mirrored runs.

- **Turn-taking is down.** In a pair, the next blow is the other unit's 45% of the time (76% after M7, 63% before).
- **Bursts happen.** 35% of blows start within 0.6 s of the unit's last blow ending; before, none did. A burst is now measured from a blow's end, because blows still last 1.2–2.1 s each. Chained combos (M9) will overlap.
- **Every posture shows.** Each moves the hands, head or hips at least 10 cm beyond what they did just before, scaled by body. Peek went from 8 to 18 cm, the shield press from 10 to 4–24 cm, and the triangle step from 8 to 10 cm on a small body. The abanico is at 16 cm and the bob and weave at 38 cm.
- **Time on their feet, by state:** gather about 20%, press about 38%, shell about 23%, probe about 10%, counter about 7%.
- **Missed: energy cycles.** Energy runs 1.6 charge-and-spend cycles a minute, against a gate of 2. Fights now end in 29–72 s, and most units fall within 40 s, before they complete more cycles.
- **Still open, outside the tempo.** A Large lost a leg to a 12-damage backhand at the hip (the M5 sever thresholds), and small units still spend 67% of their time walking. Both are worth a look before or during M10.

The defensive posture is built (2 November 2026, commits 14128dab and f3775c34). It is this plan's "M10 defence", now numbered M11 because the shield, hands and head work became M9. Its numbers come from four headless fights (seeds 1, 3, 7 and 11).

What was built, against Plan 5:

- **Weapon parries.** A club or stick is set across where the incoming blade will be. The sweep now has weapon-on-weapon contact.
- **Defence skill on each unit.** It scales the chance to block or parry and the reaction time.
- **Shell guards.** The Large's high shell uses its forearms, the shield is raised to the eyes, and the Small holds its stick across its body. The shell gives +0.25 to block or parry, and later links are met 30% sooner.
- **Guard breaks.** They happen only under a barrage and at most once every 40 s.
- **Giving ground and costs.** A shelled unit gives 0.2–0.4 legs of ground per blow, and each block or parry costs 0.04 energy.
- **Ripostes.** A unit can counter between a barrage's links.
- **Pressure.** It now builds while an enemy presses. Before, it built only from blows and could run away.

**Results by unit:**

- **Medium.** Shell while pressed 75–92% (gate 70%). Links defended 17–100%, mostly 30–60% (gate 30–60%). Guard breaks up to 2 a minute (gate about 1). Counters 50–65% of bursts (gate 30–50%).
- **Large.** Shell 13–80%, links defended 0–44%, guard breaks 0–0.5 a minute, counters 10–49%.
- **Small.** It is pressed little, so shell time is 9–66% and links defended 0–17%. Its defence is the dodge, which comes in the next milestone.

**What else showed up:**

- **Rare ripostes.** There is about one riposte per fight.
- **Lab test.** Blow by blow with the defender always trying, about 60% of blows are parried or blocked.
- **Medium shield.** The Medium's shield misses some big forehand swings. This is the same shield-cone limit as the failing perception test.
- **Headless weapon colliders.** Before this, the headless fights' weapons had no colliders, so blocks never happened there. They do now.

**For the next playtest:** how the parries and the high shell look, and whether guard breaks feel rare enough.

### Decisions (1 November 2026)

1. **Order:** left to Claude. Measure and free roam first, then the order above.
2. **Skill:** on each unit. Many units will share a style but need their own skill levels.
3. **Parries:** the club and the stick parry too, not only the shield.
4. **The leap:** a long, low hop is fine.
5. **Showing the mood:** the body alone shows it, with no bars or labels.
6. **Dodging while walking:** allowed.
7. **Spec changes:** aim tracking until a commit point, and clip-led bodies for signature attacks, are both approved.
8. **Free roam:** units roam freely once the fight starts (Plan 6), built in M7.
