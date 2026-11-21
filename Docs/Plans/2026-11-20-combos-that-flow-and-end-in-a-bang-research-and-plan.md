# Combos that flow and end in a bang: research and plan

20 November 2026 · Renier. Live version: https://claude.ai/artifact/X5YKsPDm3LZYzefQoofnuX (progress notes are added
there). This copy was taken on 20 November 2026 at 17:05; the progress note for step 1 at its end was added at
20:00 and corrected at 20:15. The live version has three pictures that a copy cannot hold; each is described here in a line where it stands.

## The short version

**A combo will be one swing with several hits. The follow-through of each cut becomes the wind-up of the next, and each cut has more swing behind it than the last.**

- **Why today looks like attack, stop, attack.** Every attack starts in the guard and ends in the guard. Two in a row land about 1.7 s apart.
- **What the performers in our own recordings do.** They never go back to the guard. Their hits are 0.57 s apart. Between them the blade travels 5.5 m and never drops under about 2 m/s.
- **What fighting games add.** Light first and heaviest last. Each hit must buy the time for the next. One long freeze on the last blow. A last blow that grows with the combo. A defender who can still break it.
- **What the old sword masters add.** A cut ends where the next one begins. Loops keep the blade moving and add strength.
- **The plan.** A map of where our cuts start and end. A join that keeps the blade moving. A blow on every step. Power of 0.6, then 1.0, then 1.6. Then the stab if he is floored.
- **First steps.** Draw and measure the blade's path. Then two blows as one swing, in the duel lab.
- **Not verified.** Nothing here has been run as a combo or seen in Unity. The numbers come from our code, our recordings and the research.

## Your five goals, and how each one will be measured

Each goal gets a number my headless tests can print, so I can tell a combo is right before you look at it. The targets are first guesses, to be tuned by your eye.

| Your goal | What it means in the game | The number that shows it | First target |
| --- | --- | --- | --- |
| 1. One fluid motion: BAM BAM BAM | No return to guard and no standstill between blows | The time from one hit to the next; the slowest the blade and his hips go in between | 0.6 s or less between hits to begin with, tuned toward 0.5 s; the blade never under 2 m/s; his hips never stop |
| 2. Each blow flows into the next | The next blow starts where the last one's blade ended, and the blade curves round instead of stopping dead and coming back | How far the blade is from the next blow's start when that blow takes over; the sharpest corner in the blade's path | 10 cm or less; no sharp corner |
| 3. Follow-through | The blade goes through the target and on. It does not stop at the body | How far the blade travels after the hit; its speed just after the hit against just before | 60 cm or more; at least 70% of its speed kept through a body (a shield may stop it) |
| 4. Epic and powerful | The hit shows on both men: the struck man is thrown, and the striker's blade visibly bites | How fast each hit throws the struck part and the whole man; how long the blade bites | Set per blow in the plan below |
| 5. The last blow is the strongest | The combo builds and ends in a bang | Each hit's throw against the first hit's; what the last one does to him | The last hit throws him at least twice as hard as the first, and floors him or sends him stumbling 3 m or more |

Two more numbers ride along with every test. One is how many of the blows land on the man and not on his shield. The other is the existing budgets, such as a sword passing through its bearer's own shield, which a combo must not make worse.

## Where our combat stands today

We have every part a combo needs except the join. Today each attack starts from the guard and ends in the guard, so two attacks in a row land about 1.7 seconds apart.

### What is already built

| Already in the game | What it gives a combo |
| --- | --- |
| Made attacks (duel lab and dance): each one rebuilt from a recording's parts, aimed at a body part, thrown on the move, landing on a footfall | Every blow of a combo can be aimed, stretched, sped up or slowed |
| Light, plain and heavy blows: light hits 0.6 times as hard with a short wind-up, heavy 1.6 times with a long one | The lever for "the last one is the biggest" |
| A cut that lands on a body drives on through and keeps 70% of its speed | Follow-through is already real when the blade meets flesh |
| The freeze on a hit: 0.04 to 0.16 s by the blow's weight; a finisher 0.13 to 0.17 s with twice the knock-back | The bang exists and it scales |
| One door for every impact: shoved, stumbling or floored. A stumbling man cannot guard (duel lab and dance) | The last blow can floor him with what is built |
| The pounce, and the stab for a man who is down | What comes after the combo |
| The shield bash, three kicks, fakes | Ways to open him up first |
| Blade streaks and sound hooks | The show has a place to plug in |

### What stands in the way

1. **No join.** A forehand then a rising cut, each played whole: 0.87 s of recovery, then 0.83 s of wind-up. That is about 1.7 s from hit to hit. This is worked out from the recordings' lengths, not from a test run.
2. **The old combos cannot be reused.** The system from before 11 November still runs in the fight of six. Its hits came 0.32 to 0.65 s apart (headless). It plays the middle of each recorded strike, from about a quarter of a second before the hit to about 0.2 s after it, and fades the pose across to the next. Its first version cut at the hit, and in your 4 November playtest the tail of each slash was lost, so it was then let sweep on for about 0.2 s. Two things rule it out now. It plays recordings as they are, which our rules no longer allow. And its join fades between two poses, instead of carrying the blade on to where the next cut starts.
3. **Few cuts are in use.** By itself the attack maker picks only the forehand, the rising backhand and the thrust. The chops, the lunge and the spins are thrown only when named.
4. **A blocked blow backs out.** Against a shield or a parry the blade stops and comes back. A combo has no flow there yet.
5. **The blade's speed is capped at 30 m/s**, because physical arms cannot follow anything faster. So extra power cannot come from raw blade speed alone.

### What the performers in our own recordings do

They never go back to the guard. The follow-through of one blow is the wind-up of the next.

*[Picture in the live version: a chart of the speed of the blade's point over time. Two whole attacks in a row: hits 1.70 s apart, the blade back at the guard in between. The performer's one combo: hits at +0.57 s and +1.77 s, the blade never under 2.1 m/s between the first two. From our mocap takes, the blade worked out from the hand's bones.]*

The grey line is our way today, built from the two single recordings: after the first hit the blade swings back to the guard and waits. The blue line is one performer who never goes back.

| Measured in our mocap takes | Value |
| --- | --- |
| Hit to hit in the sword and shield four-cut combo | 0.57 s, then 1.20 s (the last one is a leaping chop) |
| Hit to hit in the other combo takes | 0.27 to 1.3 s, mostly 0.4 to 0.9 s |
| How far the blade's point carries on after a hit | 2 to 9 m, which is 120 to 470 degrees of blade turn |
| The blade's slowest speed between two hits | mostly 1.6 to 5.4 m/s; in 4 joins of 19 it nearly stops |
| From that slowest moment to the next hit | 0.13 to 0.6 s, mostly 0.23 to 0.47 s |
| What the blade's path does at its slowest moment | a bend of 20 to 70 degrees (a loop), or a turn-round of 80 to 175 degrees |

19 joins between 28 blows, in 9 recorded combos. The blade is worked out from the hand's bones, so the speeds are approximate.

The model for us is the first two cuts of the four-cut combo. After the forehand lands, the blade's point carries on 5.5 m in a third of a second. It slows to 2.1 m/s, turns round, and lands again 0.23 s later.

## What fighting games do

A fighting-game combo is not just fast attacks. It is a chain in which each hit buys the time for the next one. Seven ideas hold it together.

### 1. Each hit freezes the victim long enough for the next

When a hit lands, the victim is stuck reeling for a set time. Players call it hitstun. A combo is a string where the next hit arrives before that time runs out, so it cannot be blocked ([glossary](https://en.wiktionary.org/wiki/Appendix:Glossary_of_fighting_games)). Heavier attacks stun longer. In Guilty Gear Xrd the five attack levels stun a standing man for 10, 12, 14, 17 and 19 frames, which is 0.17 to 0.32 s ([Dustloop](https://dustloop.com/w/GGXRD/Attack_Attributes)).

**For us:** every blow in a combo needs a job. It must do something to him that covers the next blow: knock his guard aside, turn him, or set him stumbling.

### 2. The recovery is skipped

An attack has three parts: the start-up, the moment it can hit, and the recovery. A "cancel" overwrites the rest of one move with the next move, so the recovery never plays ([Capcom's own lesson](https://game.capcom.com/cfn/sfv/column/132455?lang=en)). It began as a bug in Street Fighter II that the makers left in, and God of War II later had about 4,000 such branches for its hero alone ([Combat Canceled](https://www.gamedeveloper.com/game-platforms/combat-canceled-i-god-of-war-i-action-game-design)). Chivalry does it with swords: start a new attack before the last one ends, and the recovery is removed ([Chivalry wiki](https://chivalry.fandom.com/wiki/Strategy)).

**For us:** our old combos skipped it too, by fading between the poses of recorded strikes. The new ones have to skip it without cutting the swing, by the blade's own path.

### 3. The freeze on the hit

Both fighters stop for a moment on every hit. The pause sells the weight. It is also the window in which the next move is entered, which is why Capcom says heavy attacks are easier to cancel than light ones.

| Game | Freeze on a hit | In seconds |
| --- | --- | --- |
| Guilty Gear Xrd, attack levels 0 to 4 | 11 to 15 frames; up to 27 for the victim on a counter hit | 0.18 to 0.25; up to 0.45 |
| Smash Ultimate, a hit worth 15% | 15 frames; never more than 30 | 0.25; never more than 0.5 |
| Smash Melee, a hit worth 15% | 8 frames; never more than 20 | 0.13; never more than 0.33 |
| Our game today, any blow | by the blow's weight | 0.04 to 0.16 |
| Our game today, a finisher | 1.7 times longer | 0.13 to 0.17 |

Frame counts from [Dustloop](https://dustloop.com/w/GGXRD/Attack_Attributes) and [SmashWiki](https://www.ssbwiki.com/Hitlag), at 60 frames a second. Our rows are from our own code.

Sakurai, who makes Smash, gives the rules ([his column](https://sourcegaming.info/2015/11/11/thoughts-on-hitstop-sakurais-famitsu-column-vol-490-1/), [his eight techniques](https://nintendowire.com/news/2022/12/12/this-week-in-sakurai-12-5-12-11-fine-tuning-hit-stop-and-cheating-the-system/)). Stronger hits freeze longer, up to a cap. The victim shakes, and the shake fades. The attacker keeps drifting a little, so the pause does not look dead. Use less of it in a crowd, because a third fighter can strike into a long freeze.

It matters. The Monster Hunter Wilds beta came out with most of the freeze missing, players said the weapons felt weak, and the director promised to bring the weight back ([GamesRadar](https://gamesradar.com/games/monster-hunter/putting-beta-feedback-into-action-monster-hunter-wilds-devs-show-off-improved-hitstop-and-reworked-weapons-that-feel-how-you-remember-them)).

**For us:** short freezes on the linking blows keep the flow. One long freeze on the last blow is the bang. Our finisher freeze is already there, a little under what 2D fighters use.

### 4. Light, then medium, then heavy

Chains go up in weight: a light attack cancels into a stronger one ([glossary](https://en.wiktionary.org/wiki/Appendix:Glossary_of_fighting_games)). The light one is fast, so it lands first. Its stun covers the slower start of the next. The heaviest comes last, when the victim can least answer.

For Honor learned this the hard way. Its big finisher kept being interrupted by a quick jab, until the blow before it was given a longer stagger ([Ubisoft](https://www.ubisoft.com/en-au/game/for-honor/news-updates/6kUYoy99SGCl29W1kcw4i9/season-5-kensei-update)).

**For us:** a quick light cut first, the heaviest last. And the blow before the last must stagger him long enough for the big wind-up.

### 5. Opener, links, ender, and the cash-out

Killer Instinct names the parts. An opener starts the combo, linkers extend it, and an ender closes it ([Infil's guide](https://ki.infil.net/combo.html)). The ender is where the reward is paid, and it grows with the combo before it: a level 1 ender hits once, a level 4 ender hits four times ([enders](https://ki.infil.net/combo4.html)). Enders also come in kinds: more damage, a launch, a slam into the wall, a hard knockdown.

Other games use the same shape. In the Warriors games the number of light attacks before the heavy one picks which finisher comes out ([Prima](https://primagames.com/?p=44307)). In Bayonetta the last hit of a finished combo deals far more than a normal attack, and the quickest route to it was made the weakest on purpose ([PlatinumGames](https://platinumgames.com/official-blog/article/6891)).

**For us:** the last blow grows with the blows landed before it. And there can be kinds of last blow: one that floors him, one that throws him back, one that breaks a shield guard.

### 6. A combo cannot run forever, and the defender has a say

Killer Instinct fills a meter with every hit. When it is full the combo blows out, so it must be ended in time. Light attacks fill it fast, heavy ones slowly ([enders](https://ki.infil.net/combo4.html)). The defender can break a combo by reading it. For Honor's finisher can still be parried or rolled away from. Most fighting games also shrink the damage of each later hit ([glossary](https://en.wiktionary.org/wiki/Appendix:Glossary_of_fighting_games)).

**For us:** two to four blows, by the fighter's skill. Every blow stays a real attack that can be blocked, parried or struck into.

### 7. What comes after

Many combos end in a knockdown, because a downed man cannot act ([glossary](https://en.wiktionary.org/wiki/Appendix:Glossary_of_fighting_games)). That hands the attacker the next move.

**For us:** the last blow floors him, and the stab for a man who is down follows. We built that this week.

## What real sword work does

Real swordsmen solved "attack, stop, attack" centuries ago: they do not go back to the guard between cuts.

### Every cut ends where another can begin

Joachim Meyer's lessons of 1570 for the dusack, a one-handed cutting sword like ours, say it outright: "you will always move from one guard to the other" ([Meyer's dusack](https://science-and-fiction.org/swordfighting/meyer_dusack.html)). A guard is not a pose to stand in. It is the place a cut ends, and so the place the next one starts. A full cut goes right through the target and out the far side. A diagonal cut down from the high right ends low on the left, and the natural answer is a rising cut back up the same line.

His fencers were told not to stand still in a guard at all. The older saying behind it is blunt: "He who lies still is dead" ([HROARR on Meyer](https://hroarr.com/?p=6678)).

Three more of his rules matter to us:

- **Three quick cuts at different heights.** High, middle and low, so that one of them lands.
- **Not just left, right, left.** Swapping sides alone does not work. The line has to change.
- **A favourite way to finish:** two diagonal cuts down, one from each side, straight after each other.

### The loop that keeps the blade moving

Sabre masters call it the moulinet, the "little mill". The point is whirled round in a circle or a figure of eight. It is done "to gather momentum and add strength to the cut" ([moulinets](https://thewoodsmansjournalonline.blogspot.com/2025/09/the-practical-swordsmans-compendium_096458709.html?m=0)). The six moulinets are drilled joined into one motion, and the whole point of the drill is that the swordsman does not come back to a guard between them. The moulinet is also how a cut that met nothing is brought back round.

### Why flowing hits harder, not only faster

- **A backswing that flows into the swing gives its stretch back.** Muscle and sinew store energy when they are stretched, and return it if the swing follows at once. A pause throws it away: in a bench-press study about 55% was lost after a pause of 1 s ([stretch and shorten](https://ojs.ub.uni-konstanz.de/cpa/article/view/2851/2696)).
- **The body cracks like a whip.** The hips go first and each part passes its speed outward. The roll of the upper arm and forearm comes last and adds a large share of the speed at the end ([throwing and striking](https://ojs.ub.uni-konstanz.de/cpa/article/view/2228/2085)).
- **There are two kinds of cut.** Filmed at 100 frames a second, an explosive cut takes 250 to 350 ms from the first movement, about as long as a man needs to react. A full cut made with a step takes 600 ms or more, reaches further and hits harder. The hips power both. A big cut that misses takes long to recover from ([cutting mechanics](https://science-and-fiction.org/swordfighting/cuts.html)).

**For us:**

- The follow-through of one cut must become the wind-up of the next, with no pause.
- The hips lead every blow. The hip turn that ends one cut winds up the cut from the other side.
- The linking blows are the short, quick kind. The last blow is the full cut with a step.
- A thrust has no swing to pass on. It opens a combo or ends one. Chivalry has the same rule: every attack can be chained except the stab ([Chivalry wiki](https://chivalry.fandom.com/wiki/Strategy)).

## What other sword and physics games do

The sword games closest to ours each solved one piece of the problem. None of them solved it the way we must.

| Game | What it does | What we take |
| --- | --- | --- |
| [Absolver](https://blog.eu.playstation.com/2017/05/10/master-martial-arts-to-battle-foes-and-friends-in-ps4s-absolver-out-29th-august/) | Every attack starts in one of four stances and ends in one. A string can loop back or lead to another stance. The makers' motto was "Combat is a dance" | Where one attack ends decides which attacks may follow. That is your second goal, written as a rule |
| [For Honor](https://www.ubisoft.com/en-au/game/for-honor/news-updates/6kUYoy99SGCl29W1kcw4i9/season-5-kensei-update) | Chains of three, in any mix of light and heavy. The finisher is special: the top heavy cannot be blocked | A short, fixed length. The last blow gets something the others lack |
| [Chivalry](https://chivalry.fandom.com/wiki/Strategy) | Start the next swing before this one ends and the recovery is removed. The chained wind-up is slower than a fresh one, but faster than waiting | Skipping the recovery is normal in sword games. In ours it becomes the follow-through |
| [Soulcalibur](https://primagames.com/tips/soul-calibur-6-controls) | Flat cuts are the fastest and catch a man who sidesteps. Downward cuts are slower, hit harder and can be sidestepped | Flat cuts as links against a man who circles or dodges. The downward cut as the last blow |
| [Warriors games](https://primagames.com/?p=44307) | Some light attacks, then one heavy. The count picks the finisher | The last blow is chosen by what came before it |
| [Bloodfall](https://80.lv/articles/this-first-person-sword-fighting-game-s-combat-is-100-physics-based), a physics sword game | Blades are real objects that rebound, lock and glance off. The risk it names: physical can turn awkward. Its cure: sparks and sound that scale with the impact | A blocked blow should rebound into the next cut, not stop the show. Streaks and sound scaled to the blow keep it readable |
| [DReCon](https://www.ubisoft.com/en-us/studio/laforge/news/VjEIwquaIyEZZSw5RZI0V/drecon-datadriven-responsive-control-of-physicsbased-characters), Ubisoft research | A physical body follows an animated target. The target is picked by searching recordings, the method For Honor used first | The same shape as ours: a made pose, and a physical body that follows it |
| [Gears of War 4](https://gdcvault.com/play/1025165/Inertialization-High-Performance-Animation-Transitions) | Joins two moves without blending both: the change is handled as a step after the animation | A join can be made on top of the next move, without playing two moves at once |

My reading of all this: fighting games cut the animation, For Honor searches recorded motion, and physics games leave the join to the player's hand. Ours has to make the join itself, from the blade's own path, on a body with real muscles. That is the new part of this plan.

## What this means for us: the design

A combo in our game is **one swing with several hits**. The blade does not go back to the guard between hits. Each cut is carried through, turned round without stopping, and comes back as the next cut.

*[Picture in the live version: two blows on one time line, thrown three ways. Today: two whole attacks, 1.70 s between the hits. The old combos: the middles of two recordings with the poses faded between, the second hit 0.32 to 0.65 s after the first. The plan: the follow-through is the wind-up, 0.57 s between the hits.]*

Today the second hit comes 1.70 s after the first. The old combos brought that down by fading between the middles of recorded strikes. The plan lands it at the performer's own 0.57 s and keeps the whole slash.

Five rules make it work.

### Rule 1. A map of where cuts start and end

Every cut starts in one corner around the body and ends in another. A cut may follow another only if it starts where the other one ended. This is Meyer's guards and Absolver's stances. It is your second goal, written as a rule.

I measured our own recordings for it. Six corners come out, and our cuts already link them.

*[Picture in the live version: the blade map. Six corners (high right, low left, out right, left shoulder, low front, overhead) and the cuts that link them: forehand from high right to low left, rising backhand back again (the figure of eight, the first combo to build), flat backhand from low left to out right, a loop over the head from out right to overhead, the chop from overhead to low front, the hand sliding from low front to the left hip, the thrust from high right to the left shoulder, the spinning backhand from the left shoulder to out right.]*

Forehand and rising backhand run between two corners and back: that is the figure of eight. The flat backhand leads out to the right, where a loop over the head sets up the chop. No cut ends in a dead end.

| This cut, then that one | Gap between the hand's end and the next start | Verdict |
| --- | --- | --- |
| Forehand, then rising backhand | 32 cm (13 cm with the pack's quick forehand) | Joins as it is |
| Forehand, then the flat backhand | 21 cm | Joins as it is. It is how the recorded combo goes |
| Rising backhand, then forehand | 24 cm | Joins with a small loop: the point rolls over the shoulder |
| Flat backhand, then chop | The blade ends out to the right, still moving at 3 to 7 m/s | Joins with a loop over the head |
| Chop, then rising backhand | 37 to 53 cm | Joins as it is |
| Thrust, then spinning backhand | 22 to 25 cm | Joins. The thrust comes back high on the left |

A first reading from the mocap takes, with the blade worked out from the hand's bones. The quick forehand is a take we own but have not baked yet.

### Rule 2. The join

Between two hits the attack does not play its recovery. It plays its own real follow-through as far as the corner. There the blade turns, at its slowest but still moving. The next attack is entered at the turn of its own wind-up.

At the join the blade keeps its speed and its direction, and is bent onto the new path within about 0.15 s. Nothing is cut off at the target, and nothing waits.

The numbers to aim at are the performer's own: 5.5 m of blade travel after the first hit, the point never slower than about 2 m/s, the second hit 0.57 s after the first.

Each blow stays its own made attack with its own aim. So everything we built for single attacks keeps working: aiming, landing on a footfall, the freeze, what a hit does to a body. Only two things are new. An attack can end at its turn instead of in the guard. And an attack can start from the blade as the last one left it.

### Rule 3. A blow a step

The combo is thrown on the move, one blow on each footfall. Our recorded walk puts a foot down every 0.55 s. The performer's hits are 0.57 s apart. So the feet give the rhythm, the hips never stop, and the fighter walks through his man. A man who is driven back is followed.

### Rule 4. Each blow has more swing behind it than the last

This is what makes the last blow the biggest without faking it.

|  | First blow | Linking blow | Last blow |
| --- | --- | --- | --- |
| What it is | A quick light cut from the guard | A cut that flows out of the one before | The full cut, with a step or a leap |
| Where its strength comes from | A short wind-up | Its wind-up is the whole follow-through of the blow before | That follow-through, plus a loop over the head, plus the step |
| Power, against a plain blow | 0.6 | 1.0 | 1.6, and more for each blow landed before it |
| Freeze on the hit | about 0.05 s | about 0.07 s | 0.13 to 0.17 s as now; to try up to 0.20 s |
| What it does to him | Knocks his guard or his sword aside | Staggers him, but leaves him in reach | Throws him: stumbling 3 m or more, or floored |
| Streak and sound | Short, pale | Longer, a higher swish | Longest and brightest, the deep hit |

The last blow grows with what came before it. After one landed blow it sends him stumbling. After two it floors him. If it floors him, the stab for a man who is down follows.

The rhythm is da, da, DUM. The first gaps are about half a second. The last blow comes a little later, because its swing is bigger. That small wait is what tells the eye the big one is coming.

### Rule 5. Every blow is still a real attack

A combo is not a cutscene. Three things can happen to each blow, and the swing carries on through all three.

- **It cuts through him.** The blade drives on and flows into the next cut.
- **It is blocked.** The shield throws the blade back. That bounce becomes the wind-up for a cut on another line: a forehand stopped by the shield turns into a thrust over it. Today a blocked blow backs out and stops.
- **It misses.** The blade swings round in a loop and comes back.

Each next blow is aimed at what the last one opened. A cut high draws his shield up, so the next goes low.

He can break it in three ways: a clean parry that beats the blade aside, a bash, kick or blow into the big wind-up of the last blow, or stepping out of reach. Then the combo is over and the next move is his.

### What starts a combo, and how long it is

Not chance. A combo starts when a light cut lands, or when something has opened him: a bash or kick that broke his guard, a stumble, or his own heavy wind-up. Its length is a number each fighter has: two, three or four blows. The cuts are read from the map, starting from the corner the blade is in.

## The first combos

Eight combos can be built from recordings we already have. The first needs nothing new but the join.

| Combo | The blows | What it still needs |
| --- | --- | --- |
| **The figure of eight** | Forehand down, rising backhand up the same line | Only the join. Both cuts are baked. This is the first one to build |
| **Bam, bam, BAM** | Forehand, rising backhand, then a heavy forehand with a step | The join, and the loop over the shoulder |
| **The performer's own** | Forehand, flat backhand, leaping chop | The loop over the head. It is the recorded four-cut combo, so we can check ours against his |
| **The spin** | Forehand, rising backhand, then a backhand made spinning right round | The cuts are baked. His whole turn is the wind-up. He shows his back, so only after a blow that staggered the man |
| **Over the shield** | Forehand onto the shield, then a thrust over it | The bounce off a block turned into a wind-up |
| **High, low** | A cut at the head to draw the shield up, then a cut under it | Aiming each blow at what the last one opened. The low cut is not built yet |
| **Kick and cut** | A kick sets him stumbling, the pounce's blow lands, a heavy rising cut ends it | The kick and the pounce exist. The pounce's blow becomes a first blow |
| **The full arc** | Hard bash breaks his guard, figure of eight, leaping chop floors him, the stab on the ground | Everything above. The bash and the stab exist |

Three honest notes:

- **The time.** You decided on 4 November that three hits take one second. The performer's first two hits are 0.57 s apart, and our walk steps every 0.55 s. So the honest target is 1.0 to 1.15 s from the first hit to the third, and we tune down from there.
- **Missing cuts.** We have no flat forehand to pair with the flat backhand, and no low cut. Both are in the mocap packs we own. Baking them is its own step.
- **Two of our "different" cuts are the same take.** The stepping backhand and the spinning backhand are one recording: he turns 396 degrees in it. One chop is a leap with a full spin, the other a lunge of 3.6 m. So today the Medium really knows six cuts: forehand, rising backhand, thrust, spinning backhand and the two chops. The flat backhand exists only as a slice of the combo take.

## The plan, step by step

Seven steps, in the duel lab first and the dance after. Step 2 is the hard one: if the join works, the rest is building on it.

| Step | What changes | What you press in the lab | Headless pass mark |
| --- | --- | --- | --- |
| **1. The blade's path, drawn and measured** | Nobody fights differently yet. A report reads every recording: its corners, how far it carries through, and how well each pair joins. The lab can draw the blade's planned path as a line, with the hit marked | "The blade's path: drawn / hidden" | The report finds the performer's own join as the best one. Its map agrees with the one in this document |
| **2. Two blows as one swing** | The join. Forehand into rising backhand, on the move, on a dummy with no guard | "Throw two: flowing / one after the other (old)" | 0.60 s or less from hit to hit, against about 1.7 s. The point never under 2 m/s between the hits. 60 cm or more of travel after the first hit. 10 cm or less at the join. The second blow lands where it was aimed |
| **3. Three blows and the big one** | Light, plain, heavy. The loops over the shoulder and over the head. The last blow's step, its longer freeze, its bigger throw, and how it grows with the blows before it. The chop and the spin as last blows. The stab follows a man who is floored | "Combo: 2 / 3 blows" and "Last blow: forehand / chop / spin" | 1.15 s or less from the first hit to the third. The last hit throws him at least twice as hard as the first. He is floored, or stumbles 3 m or more |
| **4. Against a man who defends** | Each blow is aimed at what the last one opened. A blocked blow bounces into the next line. A clean parry, a blow into the last wind-up, or distance ends the combo. The last blow on a shield knocks the guard open | Dummy: "No guard / Shield up / Parries". "A blocked blow: flows on / backs out (old)" | Against a shield, more combos get a blow onto the body than single attacks do now. A parried combo always ends. No more of the sword passing through his own shield than today |
| **5. The dance fights with combos** | Fighters start combos by themselves: after a bash or kick that opened him, after a light blow that landed, on his stumble, into his heavy wind-up. Each named dancer gets his own habits and his own combo length | "Combos (dance): on / off", and a readout for each dancer | Over four dances of 70 s: combos started and finished, the share of blows that land, men floored. The dance's budgets no worse than today |
| **6. The show** | Streaks that grow with each blow, a swish that rises, the deep hit, a camera kick on the last blow, a short slow-motion beat when it floors him, and a spark and a ring when a blow is blocked | A switch for each | Mostly your eyes in Unity. Headless only counts that each cue fired |
| **7. More cuts** | Bake the quick forehand, a flat forehand and a low cut from the packs we own. New combos: three flat cuts, high then low, a leg sweep as a last blow | The new cuts by name in the lab | The report from step 1, run again with the new cuts |

Every step comes the usual way: a Combat Feel switch with a tooltip, old against new in the lab, headless numbers, a README entry, a note here, the briefing brought up to date, a commit and a push, the panel's "Look here" list, and a 3b2b.

Only when you say: the fight of six, where a combo can spread across several men. Later still: a parry that flows straight into the defender's own combo, Large and Small bodies, longer combos and special last blows by star level, bodies thrown into other men, and a player's hand on a fighter.

One thing is settled before step 1. You decided today that a fighter goes for a man who is down. It becomes the default in the duel lab as well as the dance. It stays out of the fight of six.

## Risks, and what I need from you

Nothing in this document has been run as a combo yet. The joins and gaps come from mocap takes with the blade worked out from the hand, not from the game, and none of it has been seen in Unity.

| Risk | How we will know | What we do then |
| --- | --- | --- |
| **The arm cannot keep up with the turn.** The blade must reverse in about a quarter of a second, and our arms have real muscles and a 30 m/s limit | Step 2 measures how far the blade trails its pose at each hit | A slower turn, about 0.65 s from hit to hit. Or the loop in place of the hard turn |
| **His own shield is in the way.** The rising backhand starts at the left hip, where the shield sits | The count of sword-through-shield crossings | The shield gives way to the sword, as our rule says. If it cannot, that pair is not offered to a man with a shield |
| **The struck man leaves.** A linking blow that throws him too far breaks the chain. Too little and it looks weak | Whether he is still in reach one step later | The knock of a linking blow is set by that measure |
| **A blow a step does not fit every pair.** Some joins want 0.45 s and some 0.7 s | The report in step 1 gives each pair its time | The upper body is fitted to the feet, as attacks on the move already are. A pair that cannot fit is left out |
| **Physics makes it awkward.** Real blades can do odd things when they meet | The drawn path in step 1 shows where body and plan part | The made pose leads and the body follows, so the plan stays readable |
| **A combo that always lands decides every duel** | Steps 4 and 5 count how often combos finish | The three ways out, and the length each fighter is allowed |
| **Two combo systems at once.** The old one stays in the fight of six | Switches named apart | It is taken out only when you say |

I will take these defaults unless you say otherwise:

1. **Rhythm:** a blow a step, so about 1.1 s for three hits, tuned down toward your one second.
2. **Length:** three blows by default. Two and four are numbers a fighter can have.
3. **The spin as a last blow:** built as a choice in step 3 for you to judge in the lab.
4. **The freeze on the last blow:** tried up to 0.20 s, against 0.13 to 0.17 s now.
5. **Slow motion when the last blow floors him:** built as a switch, off to begin with.

I need nothing from you to start step 1. The one real decision comes later: when combos go into the fight of six.

## Sources

Every page here was opened and read for this document on 20 November 2026.

**Fighting games**

- [Guilty Gear Xrd attack levels: hitstop, hitstun, blockstun](https://dustloop.com/w/GGXRD/Attack_Attributes), Dustloop Wiki
- [Hitlag: the formulas and caps in every Smash game](https://www.ssbwiki.com/Hitlag), SmashWiki
- ["Thinking About Hitstop"](https://sourcegaming.info/2015/11/11/thoughts-on-hitstop-sakurais-famitsu-column-vol-490-1/), Masahiro Sakurai's Famitsu column, translated by Source Gaming
- [Sakurai's eight hit stop techniques](https://nintendowire.com/news/2022/12/12/this-week-in-sakurai-12-5-12-11-fine-tuning-hit-stop-and-cheating-the-system/), Nintendo Wire
- [Cancels explained](https://game.capcom.com/cfn/sfv/column/132455?lang=en), Capcom's Street Fighter seminar
- [Killer Instinct: combos](https://ki.infil.net/combo.html) and [enders](https://ki.infil.net/combo4.html), Infil's guide
- [Combat Canceled: God of War and action game design](https://www.gamedeveloper.com/game-platforms/combat-canceled-i-god-of-war-i-action-game-design), Eric Williams, Game Developer
- [Glossary of fighting games](https://en.wiktionary.org/wiki/Appendix:Glossary_of_fighting_games), Wiktionary
- [Monster Hunter Wilds and its missing hitstop](https://gamesradar.com/games/monster-hunter/putting-beta-feedback-into-action-monster-hunter-wilds-devs-show-off-improved-hitstop-and-reworked-weapons-that-feel-how-you-remember-them), GamesRadar

**Sword and action games**

- [For Honor: the Kensei update](https://www.ubisoft.com/en-au/game/for-honor/news-updates/6kUYoy99SGCl29W1kcw4i9/season-5-kensei-update), Ubisoft
- [Absolver's Combat Deck](https://blog.eu.playstation.com/2017/05/10/master-martial-arts-to-battle-foes-and-friends-in-ps4s-absolver-out-29th-august/), PlayStation Blog
- [Chivalry: strategy and combos](https://chivalry.fandom.com/wiki/Strategy), Chivalry wiki
- [Soulcalibur 6 controls](https://primagames.com/tips/soul-calibur-6-controls), Prima Games
- [How Warriors combat works](https://primagames.com/?p=44307), Prima Games
- [Bayonetta's combos](https://platinumgames.com/official-blog/article/6891), PlatinumGames blog
- [Bloodfall's physics sword combat](https://80.lv/articles/this-first-person-sword-fighting-game-s-combat-is-100-physics-based), 80 Level
- [DReCon: physical characters that follow animation](https://www.ubisoft.com/en-us/studio/laforge/news/VjEIwquaIyEZZSw5RZI0V/drecon-datadriven-responsive-control-of-physicsbased-characters), Ubisoft La Forge
- [Inertialization in Gears of War](https://gdcvault.com/play/1025165/Inertialization-High-Performance-Animation-Transitions), GDC Vault

**Real sword work and the body**

- [Meyer's system for the dusack](https://science-and-fiction.org/swordfighting/meyer_dusack.html) and [cutting mechanics](https://science-and-fiction.org/swordfighting/cuts.html), science-and-fiction.org
- [Meyer on constant motion](https://hroarr.com/?p=6678), HROARR
- [Moulinets](https://thewoodsmansjournalonline.blogspot.com/2025/09/the-practical-swordsmans-compendium_096458709.html?m=0), The Practical Swordsman's Compendium
- [The stretch and shorten cycle in striking](https://ojs.ub.uni-konstanz.de/cpa/article/view/2851/2696) and [the order the body's parts move in](https://ojs.ub.uni-konstanz.de/cpa/article/view/2228/2085), two sports science conference papers

**Our own project**

- The recordings in `CombatClips.json` and the mocap takes under `Tools/Mocap/Combat/samples`.
- The code: `AttackMaker`, `AttackOnTheMove`, `StrikeHits`, `ClipFighter`.
- The earlier combo plans of 1, 4, 5 and 6 November in `Docs/Plans`, and your decisions in them.
- The measuring scripts for this document are scratch work, kept in `Tools/Mocap/Combat/flow`. Step 1 turns them into the report.

## Progress, 20 November, 20:00: step 1 is built (the blade's path, measured and drawn)

You said "Yes it sounds good. Go." at 18:42, so every default in this plan is taken. Step 1 is built and pushed. Nobody fights differently yet: it is two measuring tools.

**The report passes its mark.** It reads every recording with the rig's own blade on the recorded hand (`Tools/Headless/flow.sh`). The two places where the performer went straight on from one cut into the next come out as the best two joins of 144, with almost nothing to make up (0.02 and 0.03 s), and it finds their hits 0.57 s and 1.20 s apart: the same as the raw take.

**The map agrees with this document's, with two corrections.**

- The thrust does not come back "high on the left". Its blade comes back over his sword shoulder: high right, where it started.
- There are two kinds of chop. The leaping ones end low in front, as the map says. The lunging one ends low on his right.

**The joins table, read again with the rig's own blade.** The first reading used the hand's gap alone, from the raw takes with a guessed blade. "Bend" is how long the blade would need to get from one path onto the other; the plan allows 0.15 s.

| This cut, then that one | Bend | Hit to hit, recorded pace | Verdict now | First reading |
| --- | --- | --- | --- | --- |
| Forehand, then rising backhand (step 2's pair) | 0.11 s | 0.40 s | Joins with a bend | Joins as it is |
| Forehand, then the flat backhand | 0.07 s | 0.50 s | Joins with a bend | Joins as it is |
| Rising backhand, then forehand | 0.15 s | 0.70 s | Needs a loop: the point has to roll over | A small loop |
| Flat backhand, then leaping chop | 0.03 s | 1.20 s with the performer's own loop and leap; 0.90 s at the soonest | The performer's own | A loop over the head |
| A chop, then rising backhand | 0.21 to 0.28 s | | Needs a loop: he ends crouched, his hand at the ground | Joins as it is |
| Thrust, then spinning backhand | 0.11 to 0.12 s, entered at the spin's very start | 0.83 to 1.07 s | Slow | Joins |
| Thrust, then forehand (new) | 0.10 s | 0.40 s | Joins with a bend | |

What this changes in the plan:

- **Step 2 stands as planned.** Forehand into rising backhand is the pair to build first. The numbers it starts from: leave the forehand 0.23 s after it lands, enter the rising backhand 0.17 s before it lands, which is 0.40 s from hit to hit at the recorded pace. To make up at the join: 64 cm between the two points, 43 cm between the two hands, the blades 16 degrees apart.
- **"Bam, bam, BAM" adds up.** Forehand, rising backhand, forehand comes to 0.40 + 0.70 = 1.10 s from the first hit to the third at the recorded pace. The aim was 1.0 to 1.15 s.
- **Two joins need more than I wrote.** After a chop, a rising backhand needs a loop. After a thrust, the cut that follows best is the forehand (0.40 s), not the spinning backhand.

**In the game.** The duel lab's "Strike at" has a new row, "The blade's path: drawn / hidden" (Combat Feel "The Blade's Path Is Drawn", off by default). Drawn: the path the attack's maker planned for the sword's point in pale blue, a white bar where the blade is planned to land, a yellow cross on the spot asked for, and the sword's real path in orange. It stays until his next attack. The orange line has a dot for every step of the game's clock: dots close together mean the blade is slow. "His last blade path" under it gives the numbers, and each attack writes a `bladepath` line to your playtest log.

Headless, over 22 attacks, planted and on the move: the sword's point runs 14 cm from its planned line on average, and 0.02 s behind it. **Not seen in Unity. The drawing itself is built blind.**

**A finding for your third goal** (a blow slashes through). The game freezes a blade for 0.05 s as it lands. That is meant: it is the freeze that makes a hit feel heavy. After it the blade should sweep on. Headless, 19 blows landed on a body in the aim test, and by the game's own rule every one "drove on through" with 70% of its speed. Sixteen swept on at least 70% as far as planned; three fell well short (a forearm, a shin, an upper arm). But the sword starts again slowly after the freeze: in 7 of the 19 its point stayed under 5 m/s for a further 0.07 to 0.15 s, so to the eye the blade sticks at the body for a moment and then whips on. The cause, traced the same evening: the pose sweeps on as the rule says, but the sword is a solid thing and so is the body it struck, so the sword is held by the body (it moves at 1 to 6 m/s) until it slips past. It is not seen in Unity; it is the same code there, so I expect it shows. Step 2 starts by mending it. The drawing has a dot on the orange line for every step of the game's clock: a blade that sticks shows as dots bunched up after the white bar. Step 2 needs the blade to flow through a hit, so this is the first thing to look at.

*(Corrected at 20:15. The first version of this note said five blades were "carried only 10 to 20% as far as planned" and that it might be the headless stand-in. Both were wrong: that distance was to the blade's first slow moment, most of those blades swept on after it, and nothing in the stand-in explains it.)*

The film of this step: https://claude.ai/artifact/DicGDRVN9k8W211a5JwBj6

**Next: step 2**, two blows as one swing, when you have looked at the drawing.

**After your first look (20 November, 21:15).** You said the blue dummy kept walking into you after the first attack. Your log shows why: after your first blow the swordsman pounced by himself and then stood where the pounce ended; the target walked back to its own spot, which was now right against him (0.70 m). Every blow after that was thrown from far too close. Mended: after he goes for a man by himself, the swordsman steps back to the distance he stood at (Combat Feel "After Going For Him He Takes His Distance Again"; lab row "After he goes for him:"). Headless the next blow then starts from the right distance; not seen in Unity. Your logs also show the blade sticking at the body in Unity: 7 of 19 landed blows crawled for about a tenth of a second after the freeze, the same share as headless. A fresh log from the right distance is asked for before step 2.

**Progress, 20 November, 22:08: step 2 is built (two blows as one swing). Headless only.** In the duel lab, "Two blows: flowing / one after the other (old)" and the button "Throw two": a forehand down and a rising backhand back up, at the chest. The forehand is left at the corner where its blade turns, and the backhand is begun there in the middle of its own wind-up; the body is bent from the one onto the other over 0.12 s. Against the plan's marks: first hit to second 0.53 to 0.58 s (one of seven took 0.67 s; the old way takes 1.85 s); the point never slower than 3.1 m/s between the hits; 8 to 10 m of travel. Two marks are missed: at the join the posed point is thrown 15 to 19 cm off its course in one step (the mark is 10 cm; the cause is not found), and the second blow meets his right arm on its way to the chest in five of seven. Not done: the second blow is made as a standing blow even when he walks. With it, two mends: a sword now passes on through a body its blow cut (blades that stuck after the hit: 7 of 19 before, 0 of 17 now), and after a pounce he steps back sooner (not measured). The film of this step: https://claude.ai/artifact/T38Pqw9FVNzxGAafWvJvw7

**After your look at step 2 (the night of 20 to 21 November). Headless only.** You said the two blows feel like a single motion, and that the body jumps to a different pose between the two hits. The cause is found and mended: each recorded blow is stored the way the fighter faced as its recording began, and the first build joined the two as if neither had turned (they have, by 68 and 79 degrees). The game was handed a second blow pointing the wrong way, and turned him 43 to 50 degrees, dropped his hips 17 cm and leaned him 28 degrees in one step to fit it. Now the second blow is laid onto where the first really has him: turn 0 to 7 degrees, no drop, no lean; 0.55 to 0.58 s from hit to hit; the second blow 9 to 30 cm from its spot. One thing got worse: the blade nearly stops at the bottom corner now (0.4 to 2 m/s). The lever "Join makes up at most" at 0.25 leaves the forehand sooner and keeps the blade moving, with a bigger kink; which reads better is yours to say. Not done: the pair thrown while walking fast lands low. Step 3 waits for your look. The film of the night: https://claude.ai/artifact/32AGujgomksYWrM7JgmTXW

**After your two looks on the morning of 21 November. Headless only.** 07:38: the jump at the join was gone; the second blow missed when the first drove the dummy back. Built: he jumps in for the second blow, an attack speed slider, the dummy stays where a stumble ends, and he pounces only on a stumble of 0.6 m. 08:44: he fell 3 times after long jumps in (his physical body was left half a metre behind his root, which the game took for a man borne down); you wanted one speed for combos and one for single blows; the panel needed scrolling sideways. Built (09:49): in the jump his body is carried with him (nothing of his physics put out), two speed sliders ("Speed: single", "Speed: combos"), and the panel held to its own width with rows that wrap (built blind). Step 3 still waits for your word.

**Progress, 21 November, 11:43: step 3 is built (three blows and the big one). Headless only.** You said "Ok this all looks good to me. you can proceed" at 10:41; your log of 10:38 shows the carried jump working in Unity (jumps of up to 90 cm, his chest within 8 cm, no fall). In the duel lab, "Throw combo" now throws 2 or 3 blows as one swing: a quick light forehand, a rising backhand, and the big one, which you choose in "Last blow: forehand / chop / spin". The big one begins early in its own wind-up (that is the loop that brings the blade round, and the small wait before it), lands hardest, freezes longest (up to 0.20 s) and, after two landed blows, floors him; with only two blows he stumbles and stays up. Headless at speed 1: three hits in 1.25 s with the forehand (the pass mark is 1.15 s; met at combo speed 1.25, and 1.07 s at the 1.44 you play at), 1.48 s with the chop, 1.60 s with the spin (which misses at 1.44); the big one lands 9 to 12 times as heavy as the first; he is floored each time, and the stab on the ground follows. Not right yet: the second and third blows are planted even when he walks; the chop's leap and the forehand's throw may be too much; the spin is wild. **Next: step 4**, against a man who defends, when you have looked.

The film of this step: https://claude.ai/artifact/3cZJau7b6BLWyTdwq7sJZ1
