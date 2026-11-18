# Fight Rhythm: Plan for Idle Behaviour, Postures and Styles

> Copy of the Claude document <https://claude.ai/artifact/Qgp6enacNw3DsUQxw7T99W>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Oct 31, 2026 · @Renier

## Summary

Before milestone 5, every unit gets a **Rhythm**. That is the name I propose for the movement a fighter keeps up between choosing to strike and being struck. A Rhythm is a base loop plus three to five **postures**, mixed by a **mood** that changes over the fight.

- **Large: boxing.** A low bounce behind a high guard. Its postures are a club feint, a bob and weave, a level change and a stance shift. It tends to feint just before it really swings.
- **Medium: round-shield and sword.** This is early medieval sword-and-shield fencing. The shield is held out at arm's length and the sword pulled back, and the fighter circles to the opponent's shield side. Its postures are a high feint, a shield press, a guard change and a peek over the shield.
- **Small: Filipino stick fighting (Eskrima/Kali).** The stick never stops: wrist fans, angled triangle steps, and a twirl. Its postures are an abanico flurry, a triangle step, a redonda twirl and a baiting retreat.

Each posture can mark **openings**, windows such as "0.4 to 0.55 s, head and lead arm exposed", that only some fighters know to read. They are recorded and shown, but nothing acts on them yet. Deciding when to attack is pinned for later, in its own section below.

The postures are built from key poses the way the get-ups are, so your own clips can replace them in milestone 8.

## The name: Rhythm

I suggest **Rhythm**, because that is the word boxing coaches use for this. It means the tempo and flow a fighter moves in, which they establish, vary and break to make openings. Feints, footwork and combinations are all described as parts of it ([Evolve on rhythm](https://evolve-university.com/blog/finding-your-beat-the-importance-of-rhythm-in-boxing/)). The word also covers the two things you want from the system: a style's characteristic beat, and that beat changing with mood and fatigue. Other names I considered: *Bearing*, *Carriage*, and the Japanese *kamae* (posture or ready stance).

| Term | What it means here |
| --- | --- |
| Rhythm | The whole system: one per fighting style, tweakable per unit |
| Base | The loop that plays when nothing else is mixed in (bounce, sway, guard, weapon motion) |
| Posture | A short burst of characteristic movement, 0.3–1.2 s, upper body and feet: a feint, a footwork trick, a guard change, a flourish |
| Phrase | A posture followed by what usually comes after it, such as a feint, then the real blow. Phrases are the pattern that keeps the mix from being random |
| Mood | What sets the mix: energy (falls with effort) and aggression (rises with success). Fresh fighters posture often and bounce high; tired ones barely do, and their guard sags |
| Opening | A window inside a posture or the base when a body part is exposed (times and parts are data) |
| Read | A unit's knowledge of another style's openings. Who knows what is data; units may learn reads in play |
| Tell | A posture that gives away what comes next, such as a boxer's feint before a real swing. Tells are what reads key on |

In code, the current `CombatIdle` (sway, bob, weave) becomes the base layer of a `Rhythm`. The style asset gains a Rhythm section, and each posture is its own asset like a move.

## What the research says

Fighters fill the time between strikes on purpose, with three kinds of movement. They keep a base that stays light and ready, they throw fakes to draw reactions, and they change angle with their feet. Animators build idles the same way: a quiet base with distinct breaks played over it.

**How fighters move between strikes**

- **The base is small and low.** A boxer's bounce keeps the hips down, the feet barely leaving the floor, and the weight about 55/45 between the legs, so they can change direction or punch at once ([Expert Boxing](https://expertboxing.com/boxing-bounce-step-footwork-technique)).
- **Feints draw a reaction, then punish it.** A half-thrown jab makes the opponent parry, which opens the other side. A body feint drops the guard for a shot to the head. Half-speed punches mistime the defence ([Evolve: 10 feints](https://evolve-mma.com/blog/10-essential-boxing-feints-and-how-to-use-them/)). Feints come right before the real blow, which is your feint-before-hit pattern.
- **Head movement and level changes are postures too.** Tyson's peek-a-boo kept the gloves at the cheekbones, bent the knees to change level, and bobbed and weaved constantly. His *shift* switched stance mid-move to attack from a new angle ([Evolve on Tyson](https://evolve-mma.com/blog/breaking-down-mike-tysons-style-of-boxing/)).
- **With a shield, feint with the sword.** The shield is too heavy to change line in time. A feinted high cut draws the shield up for a cut to the wrist. A shield press pins the other shield for a thrust underneath. After attacking: strike, raise the shield, step back. Lowering the shield to look again after a high block is the moment to strike ([sword and shield tactics](https://www.science-and-fiction.org/swordfighting/sword_and_shield.html)).
- **Stick fighting never stops moving.** Eskrima flows from one technique to the next with the stick in constant motion. The *abanico* (fan) is a whipping side-to-side wrist motion driven from the hips. Footwork follows triangle patterns that step off the line to a new angle ([FMA glossary](https://pdfcoffee.com/fma-glossary-pdf-free.html), [abanico mechanics](https://www.martialtalk.com/threads/abanico-striking.11496), [male triangle](https://maelstromcore.com/?p=6468), [Inosanto notes](https://lakasfma.com/2013/08/12/08-11-13-inosanto-fma-notes/)).

**What gives a fighter away (the raw material for openings and reads)**

- Tells before a strike: a dropped rear hand before a cross, leaning back before a kick, and feet stepping off the centre line before a power shot. "A fighter's feet are often their loudest tell." Habits are tells too: a favourite combination, backing straight up after exchanges, dropping a hand when hooking ([Evolve on reading opponents](https://evolve-mma.com/blog/reading-your-opponent-how-to-spot-subtle-tells-and-patterns-during-a-fight/)).
- Fatigue shows in breathing before it shows in output, and tired fighters feint less and drop their guard. That is your boxer who postures less as he tires.
- Fighting games formalise openings as frame data: every move has startup, active and recovery frames, and being hit during recovery is being punished ([Dashfight](https://dashfight.com/news/fighting-games-essentials-frame-data-3156)). A posture's opening at 0.4 s is the same idea applied to a feint.

**How animators build idles**

- One neutral base loop that everything blends to and from, with nothing distinctive in it, because repetition gives quirks away. Distinctive moves go in breaks that play now and then. Additive layers and blends, such as relaxed against out of breath, replace many separate clips ([GamesRadar on idles](https://gamesradar.com/deceptively-complex-art-doing-nothing-standing-around)).
- Overgrowth built its fighters from a few key poses with procedural blending ([GDC: an indie approach to procedural animation](https://gdcvault.com/play/1020049/Animation-Bootcamp-An-Indie-Approach)). The swings and get-ups here already work this way, so postures use the same approach.

## The three styles

Each style has one base and four postures. Each posture has a characteristic follow-up, so the mix has a pattern. Lengths and openings are first guesses to tune by eye. The weapon stays in the right hand throughout, and the off hand does what a lead hand or shield does in that style.

### Large: boxing, with a club for a right hand

The large unit is a pressure boxer in Tyson's mould: heavy, low and always moving its head. The club is its power hand.

**Base:** a low bounce on the balls of the feet (about 2 a second, feet barely leaving the ground), knees bent, chin down. The off hand is held high at the cheek. The club is cocked at the shoulder and pumps slightly with the bounce. A slow side-to-side head sway rides on top.

| Posture | What moves | Length | Opening | Usually followed by |
| --- | --- | --- | --- | --- |
| Club feint | The club snaps back as if winding up and stops, with a shoulder dip and the weight onto the rear foot. Then it settles | 0.45 s | 0.15–0.3 s: head, as the off hand drops with the shoulder | The real swing (lead-in), or a second feint |
| Jab feint | The off hand flicks half out at the face and back, with a small step-in | 0.35 s | 0.1–0.25 s: the off-hand side of the head | Club swing to the other side |
| Bob and weave | Dips into a U under an imagined hook, knees bending about 25%, and rises on the other side | 0.7 s | 0.3–0.45 s at the bottom: head low, open to a rising blow | A rising club from the low side |
| Shift | The rear foot steps through to lead, then steps back (the stance switch you saw), with the body turning with it | 0.6 s | 0.25–0.4 s: feet square, off balance (a small push makes it stumble) | Pressing forward a step |

**Mood:** fresh, it feints every 1–2 s and bounces high. Tired, the bounce flattens, it feints rarely, and the off hand sags (an opening in the base). Aggression raises the share of club and jab feints over head movement.

### Medium: round-shield and sword

The medium unit fights the way sword and round-shield fencing is reconstructed from early medieval sources. The shield leads and does the defending, the sword threatens from behind it, and the fighter circles to the side the opponent's shield can't cover.

**Base:** the shield is held out at arm's length, rim forward, angled across the high and low left. The sword is pulled back near the hip, point forward. The stance is low and the fighter takes slow side steps around to the right, toward the opponent's shield side. The sword tip makes a small, slow circle.

| Posture | What moves | Length | Opening | Usually followed by |
| --- | --- | --- | --- | --- |
| High feint | The sword rises sharply over the shield as if for a high cut, the line breaks off, and it returns | 0.5 s | 0.2–0.35 s: the sword arm, raised and out from the shield | A low cut or thrust (second intention) |
| Shield press | The shield punches forward 20–30 cm with a half step, then pulls back | 0.6 s | 0.25–0.45 s: the legs and the sword side, while the shield is out | A thrust under the shield |
| Guard change | The sword moves between guards: over the head (hanging), on the shoulder, back at the hip. The shield drops to the knee or rises to the shoulder | 0.8 s | 0.3–0.6 s: wherever the shield isn't, while it moves | Nothing in particular; it keeps the opponent guessing |
| Peek | The shield dips to look over it, then comes back up | 0.4 s | 0.1–0.3 s: the head, by design (the classic moment to strike) | A step back if the opponent moves |

After its own blow it raises the shield and steps back (attack, raise, retreat). **Mood:** aggression adds presses and feints. A tired fighter peeks more often and holds the shield lower.

### Small: Filipino stick fighting (Eskrima/Kali)

The small unit's club becomes a single stick, the *solo baston*. Eskrima suits a small, quick fighter: constant stick motion, a live off hand, and footwork that steps off the line to a new angle. It has the restless hand motion you liked in kung fu, with a weapon it already carries.

**Base:** a light, upright stance with small constant steps. The stick traces a slow figure-eight at chest height from the wrist. The off hand (the "alive hand") is held in front of the chest, ready to check.

| Posture | What moves | Length | Opening | Usually followed by |
| --- | --- | --- | --- | --- |
| Abanico flurry | Three or four fast fan flicks of the stick, side to side from the wrist, driven by the hips | 0.6 s | 0.2–0.5 s: the legs and body, while the eyes and stick are high | A strike from the angle the flurry ends on |
| Triangle step | Steps forward-diagonal to the tip of a triangle, a quick foot exchange, out to the other side | 0.5 s | 0.2–0.3 s: mid-exchange, both feet light | A strike from the new angle |
| Redonda twirl | One big circle of the stick overhead and around: a flourish, and a wind-up in disguise | 0.8 s | 0.35–0.6 s: the whole body, while the stick is behind | An overhead strike at the end of the circle |
| Bait retreat | Steps back with the stick pulled back and the alive hand out, inviting an attack | 0.7 s | 0.1–0.7 s: apparently everything (it's a trap) | A counter when the opponent swings (needs milestone 7) |

**Mood:** this style postures almost constantly when fresh. Tired, the stick slows and the steps shorten. Aggression trades bait retreats for flurries and triangle steps.

Kung fu, such as the snake-hand forms you mentioned, was the other candidate for the small unit. It fits empty hands better than a club. It can be a fourth Rhythm later; the system doesn't depend on the choice.

## How it works

A Rhythm is data played by a small per-unit director onto the pose pipeline, between the idle and the weight shift. The director decides when to posture and which posture to play. Feet go through the stance's own steps, so balance and foot planting stay as they are.

&#91;embedded content: Rhythm architecture · data, director, pose pipeline\]

The director is the only new decision-maker. Everything it plays goes through the pose pipeline that already keeps the units balanced.

**The data (assets you can tune live, like moves)**

- `RhythmDefinition` holds the base settings (bounce rate and height, sway, guard variant, weapon loop path and speed), the posture list with base weights, the phrases ("after A, B is X times more likely"; "before a blow, play a lead-in from this set with probability p"), and the mood settings. Each `CombatStyle` points at one; the three styles get one each.
- `PostureDefinition` is a timeline of key poses on channels relative to the base. Body: hip shift, crouch, lean, turn, head. Each hand: grip offset from its guard, or *part of a move* (the club feint is the club swing's wind-up played to 35% and back). Feet: step events (foot, time, target in stance space, or "swap lead"). It also holds a blend in and out, opening windows (from, to, exposed aim parts), and a tell flag. The postures are a handful of numbers like the big swings, interpolated like the get-ups, and can later be replaced by your clips.

**At run time**

- **Base.** `CombatIdle` grows into the base layer. The sway, bob and weave stay, and each style adds its own: the boxer's bounce at a set beat, the stick's figure-eight, the shield fighter's slow circling steps, guard height falling with energy.
- **Postures.** A posture's channels are added on top of the base, weighted by its blend envelope. Arm channels move the guard grip, so blows and blocks still take over from the guard as they do now. A posture never plays during a blow, a block, a stumble or while down. A blow starting cuts the posture short, fading it out over 0.15 s.
- **The director.** After each posture it waits a gap drawn from the style's rate, scaled by energy. Then it picks the next posture by weight × mood × phrase bias, never the same twice running. When the fight logic decides to attack, the director first plays a lead-in with the style's probability (the boxer's feint before the swing), then starts the blow. Being swung at or struck biases it toward defensive postures, such as bob and weave or peek. This is a hook for milestone 7.
- **Mood.** Energy starts at 1. Postures, blows and hits taken lower it, and standing quietly restores it slowly. Aggression starts at the style's value, rises with hits landed and falls with hits taken. Mood sets the posture rate, the feint share, bounce height and guard height, so a tired boxer visibly feints less and drops his hand.
- **Openings.** While a posture (or a sagging guard) has an opening, the unit publishes it: which parts are exposed and until when. For now it is only logged and drawn in the sandbox (exposed parts outlined), ready for the attack decision later.

**What changes for the existing fight code:** the sandbox's attack timer stays as it is, but every blow it starts goes through the director, which adds the lead-in. Stumbles, knock-downs, get-ups and blocks pause the Rhythm, and it resumes from the base.

## Pinned: what an opening is, and when to attack

This is parked until the fight loop (milestone 6), where choosing a target and a body part lives. I'll remind you there, or sooner if the attack timer starts to look wrong next to the Rhythms. For now the Rhythm records openings without acting on them.

The candidates for "my blow will get through now", from the research:

1. **A posture's window**, such as the boxer's bob and weave at its lowest. Only units that have a read on that style see it, and reads are what you'd give units or let them learn.
2. **The opponent's recovery.** Right after their blow, while it's still finishing, they can be punished, like recovery frames in fighting games.
3. **Off balance or down:** stumbling, getting up, or a mid-shift square stance.
4. **A guard dropped from fatigue.** It's open to everyone, and grows as energy falls.
5. **Their reaction to my feint.** The shield goes up, so the legs are open. This is the boxer's feint-then-hit pattern done for a reason rather than by habit.
6. **Tells.** A posture that announces the next move gives anyone with the read time to get there first.

The open questions for then are how a unit perceives these (instantly, or with a reaction time), how sure it must be before committing, and whether reads are learned per opponent or per style.

## Plan of action

Seven steps, each verified before the next. The gate is that with the labels turned off, you can tell the three units' styles apart, the boxer feints before most of his swings, and everyone postures less as the fight wears on.

1. **Skeleton.** `RhythmDefinition`, `PostureDefinition`, the director and the mood, wired into the pose pipeline with an empty Rhythm. Nothing visible changes, and every existing test still passes.
2. **Bases.** The three base loops: the bounce, the circling shield and the stick figure-eight. Check: the balance and foot tests at every size, plus a side-by-side review sheet of a second of each base.
3. **Postures.** The body, arm, part-of-a-move and footwork channels, then the twelve postures. Check: each stays balanced over its feet, the weapon and arms stay clear of the unit's own body (the same check the planner uses), feet plant without sliding, and nothing pops at the blends. There are review sheets per posture, like the swings.
4. **Director and phrases.** Rates, weights, no repeats, lead-ins before blows, defensive bias after being swung at. Check by numbers over a long dry run: at least 60% of the boxer's blows follow a feint, and posture mixes match the weights.
5. **Mood.** Energy and aggression and what they drive. Check: over a two-minute fight, posture rate falls by at least half and the guard sags measurably.
6. **Openings.** Windows published, logged and drawn in the sandbox (exposed parts outlined), with a per-unit timeline in the analyser.
7. **Tools and finish.** A Rhythm gallery key: each style's base, then each posture slowed and at speed. Panel controls to force a posture and set mood. Log events and analyser charts (posture timeline, mood curves). README, then a playtest for you.

**Questions for you (I'll go with these defaults unless you say otherwise)**

- Styles: boxing for the large unit, round-shield and sword for the medium, Eskrima for the small. Kung fu can come later as an extra style.
- Posture count: four each rather than three, so the mix has room for patterns.
- Name: Rhythm, and its parts as in the vocabulary table.
- The attack timer stays until milestone 6; only the lead-ins change when blows start.

Your ideas for blocking with the weapon, dodging, rolling and backflips fit milestone 7 (defence). A Rhythm's defensive postures are where those per-style preferences will plug in.
