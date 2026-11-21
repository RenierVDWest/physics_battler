


### 21 November, 13:57: after his look at step 3: the big one made big, a new spin from his recording, the first blows keep him in reach

**What he said (12:45, with a recording, Mixamo's "Standing Melee Attack 360 Low"):** "Ok that looks good. The spin
does not look good, does not spin all the way around and the slash does not seem impactful. See if referencing this
anim can be helpful. For the forehand combo end I think that slash needs to look a lot more impactful, feels weak
atm. Do this and proceed".

**His logs** (`PlaytestLogs/2026-11-21_12-32-15` and `_12-35-52`; frames looked at and removed; **seen in Unity**):
36 combos, **thrown at combo speed 1.88** (single 1.15). What they show of "feels weak":
- **The big one's sword reached him slowly.** Over the combos of three with a forehand: 3.9 to 16 m/s, mostly 7 to
  11, where the light first blow reached him at 16.
- **The first blow often threw him out of the combo.** At 1.88 the light forehand met him at 16 m/s (8.9 headless at
  speed 1) and threw him back stumbling at 2 m/s in 14 combos, or floored him outright in 7 ("thrown at 5"). The
  blows after it then met a man lying down, or one going away, at 4 to 9 m/s, or met nothing.
- One fall of the swordsman, in the part with an armed target ("borne down, 39 cm"), not after a jump in.

**Why the big one was weak:** at 1.88 its wind-up and cut together last a quarter of a second: there is no wait
before it, and no time for a big swing. And in Unity the man it was meant for had often been thrown out of reach or
onto the ground by the first blow.

**1. The big one has its own speed** (`AttackMaker.BigPace`, `ClipFighter.ThrowsTheBigOne`; Combat Feel "A Combos Big
Blow Is Thrown At", 1; lab lever "Speed: big one"; `FIGHT_BIGPACE`). The first blows go at the combos' speed; the
last keeps its own time: its wind-up is the wait before it. At combos 1.88: the second hit 0.47 to 0.52 s after the
first, the big one 0.6 to 0.7 s after that.

**2. The big one is swung big** (`FlowCombos.LastBig`, `BigSwing`, `BigCarries`; Combat Feel "The Big One Swings
Big"; lab row "The big one's swing: big / as recorded (old)"; `FIGHT_BIGSWING=0`, `FIGHT_BIGSTYLE=...`): wound up a
quarter further, the arm drawn further back and carried further through, the hips driving and sinking more, carried
on 1.4 times as far after the landing; and it drives on through him at 0.95 of its pace, not 0.7
(`StrikeHits.FinisherCarry`). Its times are the recording's.

**3. The first blows leave him in reach** (`StrikeHits.Linking`, `LinkKeeps` 0.8; `ClipFighter.FlowLinksKeepHim`;
Combat Feel "A Combos Earlier Blows Leave Him In Reach"; lab row "Before the big one, a blow: leaves him in reach /
throws him (old)"; `FIGHT_COMBOKEEPS=0`). A combo's blow before its last drives him back no harder than 0.8 of what
would throw him back stumbling: never stumbling, never down. The plan's rule 4: a first blow "knocks his guard or
his sword aside", a linking blow "staggers him, but leaves him in reach".

**Measured, the forehand combo at his speeds (headless; combos 1.88, single 1.15; five throws in one sitting each):**

| | combos whose big one landed | the blade as it first touched him (m/s; 16 is the most counted) | what the big one did |
|---|---|---|---|
| as at 11:43 | 4 of 5 (one combo met nothing at all) | 16, 16, 12.6, 10.4 | thrown at 5.7 to 11.4 m/s: down |
| now, swung as recorded | 5 of 5 | 14.0, 16, 9.3, 10.5, 11.7 | down 4 of 5 |
| now, swung big | 5 of 5 | 11.4, 14.9, 16, 13.9, 12.0 | thrown at 7.7 to 11.0 m/s: down 5 of 5 |
| swung big, hips ahead of the arm and a quicker cut (tried) | 5 of 5 | 11.7, 8.9, 12.8, 16, 10.7 | down 5 of 5 |

**Read honestly: headless, the blade reaches him about as fast as before.** What is different is that the big one
now lands every time, that there is a wait before it, that the pose is bigger and that it goes through him. And
headless never showed his real trouble: there the light first blow reaches the dummy at 4.5 to 11.6 m/s and leaves
him standing, where in his Unity logs it reached him at 16 and threw him. So what rule 3 does in Unity is not
measured at all. The first blows headless, now: "driven back at 0.4 to 1.0 m/s", never stumbling.

**A mistake on the way, and what it cost:** the first numbers for all of this (a blade at "up to 40 m/s") were taken
between baking the new recording and finding that it had moved the body's measures (below). They were wrong and are
not used.

**The new recording moved every blow in the game, and the six caught it.** `BodyMeasures.OfClips` takes the body's
measures (what every made attack is put together with) from every strike in the library. One more strike, of
another pack's performer, changed them: the fight of six went from 0.07% and 4.8% to **0.77% and 12.6%, both over
budget**. `AttackScore.OtherBodies` now lists recordings that are thrown but not measured (`spin_low`); the six is
back at 0.07% and 4.8% exactly. **Every recording baked from now on goes on that list.**

**4. A new spin, from his recording** (`spin_low`, baked by `Tools/Mocap/Combat/combat_bake.py` from
`samples/standing_melee_attack_360_low_1.json`; `FlowCombos.Lasts[2]`, `SpinWindsUp` 0.8, `SpinTurnsAtMost` 120,
`SpinGoesLow`, `FlowCombos.Spin`; `FlowBlow.WindsUp`, `MostTurn`, `AsRecorded`; `AttackAim.AsRecorded`). In the
recording he turns right round on the spot: 250 degrees before the cut, his hand over his head through the turn,
then down and across low, left to right; the cut lands 0.93 s in at up to 25 m/s; the rest of the turn after it.
Why the old one "did not spin all the way around": every last blow was entered 0.3 s before its landing, and by
then most of a spin is over. Now:
- the spin is entered at its beginning (0.8 s before it lands), so the whole turn is seen;
- the maker may turn him up to 120 degrees more to bring the cut onto his man (he stands turned 70 degrees away
  after the rising backhand), and that is taken up in the spin itself;
- the cut lands at the moment the recording has it (left to choose, the maker took the tail of the sweep, out to
  his side, where the blade was slow: it met nothing);
- it is aimed at the hips, and made as recorded (the big style would drive his hips further round still).
**Headless: he turns 388 to 412 degrees before the hit** (about 500 in all); the cut landed in 4 of 4 throws, at
combos 1 and at combos 1.88 (the old spin missed at 1.44), at 10.6 to 16 m/s, the point up to 21 to 37 m/s; thrown at
8.9 to 11.7 m/s. **Not right:** it lands 50 cm above where it was aimed, on the chest; the sword's point strays 47 to 69 cm
from its planned path; and in the stick figures he ends chest to chest with the dummy, their legs crossing: the cut
is a short one (its own reach 0.91 m) and he jumps in 51 cm for it.

**Also:** `FLOWMADE` says the blow's own reach against how far his man is.

Checks: 0 errors; the panel's look list 9 of 9; `flow.sh` OK with the new cut in the map; the fight of six (40 s, seed 5) at its baseline: crossings 0.07% (budget 0.5%), tremble 4.8% (budget 8%), after the mend above; no exception in any run. The chop at combos 1.88: 2 of 2, on the head at 16 m/s, floored; the forehand at combos 1: 2 of 2, down, 1.25 s first hit to last.
