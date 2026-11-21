


### 21 November, 11:43: combos, step 3: three blows and the big one

**What he said (10:41):** "Ok this all looks good to me. you can proceed", with two logs.

**His logs** (`PlaytestLogs/2026-11-21_10-34-53` and `_10-38-09`; frames looked at and removed; **seen in Unity**).
The second, with the dummy unarmed: 14 pairs thrown, the second blow landed in 12, 0.47 to 0.62 s after the first.
He jumped in 10 times, by 21 to 90 cm. **The swordsman did not fall once, and in every jump his chest stayed within
4 to 8 cm of its pose, the 90 cm one too** (52 cm in the log of 08:33, the old way). So the carried jump of 09:49
does in Unity what it did headless. In the first log five of eight pairs ended at the first blow: the target there
has a sword and shield and blocked it; that is step 4's. **The speeds he ended on: a single blow at 1.15, a combo's
blows at 1.44** (his own Combat Feel; the code's defaults are still 1 and 1).

**What is built** (the plan's step 3; every number below headless, dummy with no guard, combos' speed 1):

**1. A combo is a chain** (`ClipFighter.FlowCombo`, `FlowBlow`, the roll in `StepFlow`; `FlowCombos` in
`AttackMaker.cs` says what it is made of; `DuelLab.ThrowCombo`). The pair of step 2 is kept as it was: a blow, and
the next entered out of its carry-through. A combo is pairs end to end: as the second blow lands (or passes where it
should have), the pair's account is closed and a new pair is opened on that same blow, which is now "the first".
`AttackScore` remembers which recording a joined blow is and from which frame (`Raw`, `EnteredAt`), so the third
blow is joined by the recording's own map. A joined blow's key (`JoinKey`) says all it was made from: the same join
is the same score, and a third blow never gets a stale second.

**2. Light, plain, heavy** (Combat Feel "A Combos Blows Grow"; lab row "A combo's blows: light, plain, heavy / all
plain (old)"). The first blow is a quick light cut (`FlowCombos.First`: wound up 0.6 as far and in 0.7 of the time;
after its landing it is as recorded, because its carry-through is the next blow's wind-up) and lands at 0.6 of a
plain blow. A linking blow is plain. **The last is the big one:**
- it is entered early in its own wind-up, at least 0.3 s before it lands (`ClipFighter.FlowLastWindsUp`,
  `BladeFlow.Join`'s new `leastBefore`): that wind-up is the loop that brings the blade round, over the shoulder
  for a forehand, over the head for a chop, and the wait that says the big one is coming;
- it lands at 1.6 after one landed blow, a quarter more after two (2.0), a quarter less after none
  (`FlowCombos.LastPower`, `ClipFighter.FlowLastGrows`);
- it is the showstopper (`StrikeHits.Finishing`, the old combos' own): its freeze on the hit is longer, up to 0.20 s
  (`FlowLastStop`), and where two blows landed before it its knock is doubled (`StrikeHits.FinisherThrows`);
- where two landed before it, it floors him whatever it threw him at (`FlowLastFloors`; a chop onto the head drives
  him down, not back, and left him standing).
The plan's words for it: "After one landed blow it sends him stumbling. After two it floors him."

**3. Three last blows to choose from** (Combat Feel "A Combos Last Blow Is"; lab row "Last blow: forehand / chop /
spin"): the forehand (`sns_forehand`), the leaping chop (`sns_chop_lunge`), and a backhand with a spin behind it
(`spin_backhand_mid`). Two other spins were tried and met nothing: `spin_backhand_high` (the maker itself planned a
miss of 92 cm) and `sns_backhand_lunge` (117 cm).

**4. Two blows** (lab "Combo: 2 blows"): the forehand, light, and the rising backhand as the big one.

**Measured** (one throw each unless said; `FIGHT_COMBO`):

| combo | first hit to last | the big one against the first | what it did to him | the last join |
|---|---|---|---|---|
| 3, forehand | 1.25 s | 11 times as heavy | thrown at 9.4 m/s, more than his legs catch (4.4): down, 2.3 m off | left 0.10 s after the landing, entered 0.40 s before; kink 25 cm |
| 3, chop | 1.48 s | 12 times | on the head, thrown back at 2.8 m/s, floored by the rule; 0.8 m off | left 0.17, entered 0.60; kink 17 cm |
| 3, spin | 1.60 s | 9 times | on the forearm, floored by the rule; 2.3 m off | left 0.33, entered 0.30; kink 23 cm |
| 2 blows | 0.70 s | 4.8 times | thrown back stumbling at 1.3 m/s, on his feet, 0.6 m off | left 0.20, entered 0.30 |
| 3, forehand, all plain (old) | 1.47 s | 1.0 times | a head reaction, driven back at 1.0 m/s, on his feet where he stood | the soonest within 0.15 s |
| 3, forehand, combos' speed 1.25 | 1.15 s (measured before the 12:15 changes below) | | down | |
| 3, forehand, his speeds (combos 1.44, single 1.15) | 1.07 s | 7 times | thrown at 6.1 m/s: down | |
| 3, chop, his speeds | 1.25 s (before the 12:15 changes) | | floored by the rule | |
| 3, spin, his speeds | **met nothing** | | | |

The freeze on the big one: 0.16 to 0.19 s (0.04 and 0.05 on the first two). The second hit comes 0.52 to 0.55 s after
the first; the third 0.70 s after the second (forehand): da, da ... DUM.

**Against the plan's pass marks:** "1.15 s or less from the first hit to the third": **1.25 s at speed 1, not met;
1.15 s at speed 1.25 and 1.07 s at the 1.44 he plays at.** "The last hit throws him at least twice as hard as the first": met,
9 to 12 times at speed 1, 7 at his. "He is floored, or stumbles 3 m or more": floored by all three last blows of a combo of three.

**Looked at, not only counted:** `Tools/Headless/strip.py name t0 t1` draws a run's two fighters as stick figures
from its log, from the side and from above (new today; `$S/name.strip.png`). In them: the forehand goes up and back
over his shoulder and comes down with a deep lunge; in the chop he leaps (his hips rise 0.76 m, the recording's own
leap) and the freeze on the hit catches him in the air; in the spin he crouches, turns and reaches out low. Nothing
folded or flipped. **The sword is not in those drawings (headless does not record it), and none of this is seen in
Unity.**

**Also:** the stab at a floored man follows by itself (headless with `FIGHT_DOWNED=1`: floored at 4.8 s, struck where
he lay at 7.5 s). He does not pounce out of the middle of a combo (`RunIn.StepPounce`: not while a flow is on).
Against a shield the combo ends at the first blow, which is blocked: that is step 4.

**Not done, or not right yet:** the last blow's step is the forehand's own lunge and the jump in, no new step. The
second and third blows are made planted even when he walks (step 2's limit). The chop's leap may be too much. The
spin throws the sword's point 58 cm off its planned path on average (it lands all the same). The big forehand throws
him at 9 to 10.6 m/s, which may be too far. No sounds, streaks or camera yet (step 6).

**Lab:** rows "Combo: 2 blows / 3 blows / Throw combo", "Last blow:", "A combo's blows:", levers "Last blow lands
with", "Grows, a blow landed", "Last blow's freeze", "Last blow winds up", "Last join makes up", row "After two
landed, the last: floors him / only if thrown", readout "His last combo". "Throw two" stays, and is the combo of two.
**Headless:** `FIGHT_COMBO="3,chop"` with `FIGHT_THROWTWO`, `FIGHT_COMBOGROWS=0`, `FIGHT_COMBOLAST="1.6,0.25,0.2,0.3,0.2"`,
`FIGHT_COMBOFLOORS=0`, `FIGHT_COMBOSPIN=name`; a `COMBO` line as each ends and a `FLOW` line for each pair. Without
`FIGHT_COMBO`, `FIGHT_THROWTWO` throws two plain blows as before. `Tools/Headless/wait.sh name...` waits for runs.

Checks: 0 errors; the panel's look list 10 of 10; `flow.sh` OK; the fight of six (40 s, seed 5) at its baseline of this morning: crossings 0.07% (budget 0.5%), tremble 4.8% (budget 8%); no exception in any run.

**Two changes after the first push (12:15; the table above is as it stands now; the film's numbers are from before
them: 1.22 s, 1.57 s, 0.68 s):**
- **His man is led for no longer than 0.13 s** (`ClipFighter.FlowLeadsAtMost`; `FIGHT_FLOWLEADSMOST=9` for before),
  and the time to the landing is counted at the combos' pace. As a blow is begun, the jump in aims at where his man
  will be when it lands, by how he is going. For the big one that was his speed times the whole of its long
  wind-up, and a man thrown back stumbling has stopped long before. At his combo speed of 1.44 (the second blow
  throws the dummy back at 2.4 m/s there, 0.9 at speed 1) the jump put him chest to chest with a man standing still
  (seen in the stick figures) and the big forehand met him at 6.3 m/s, its arm still bent. Now: 10.0 m/s, on the
  chest, thrown at 6.1 m/s, down. At speed 1 it moved the forehand from 1.22 to 1.25 s.
- **A last blow takes its smoothest join where that is also the sooner one** (`BladeFlow.Join`). With two blows, the
  rising backhand's soonest join within the allowance was entered 0.70 s before its landing (1.05 s hit to hit);
  its smoothest is entered at 0.30 s: 0.70 s hit to hit.
**The spin still meets nothing at combo speed 1.44** (it lands at 1). Not looked into.

**Its film** (21 November, 12:02; 3b2b, made by Opus at medium effort; 6 scenes, 26 beats, about 3 minutes 30): https://claude.ai/artifact/3cZJau7b6BLWyTdwq7sJZ1
