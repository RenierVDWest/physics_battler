# Combo feel: Bam! Bam! Bam! in one second

> Copy of the Claude document <https://claude.ai/artifact/LZCLUsz9T85Zv1zzD2t12R>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 4, 2026 · @Renier

## The goal

Three blows from one unit should land inside about a second: Bam! Bam! Bam!. That means about 0.35–0.45 s from one contact to the next, where today the lab shows something closer to a whole strike each time.

Your reading of the lab is right. Each follow-on is taken up early in its own wind-up, often from a pose near the guard. So the unit looks as if it goes back to its default pose and starts the next attack from scratch, even when the clock between contacts is shorter than it was. Your proposal is the method this plan builds on:

- **Cut each blow right after contact,** a short follow-through and no recovery.
- **Blend straight into the next blow** at a point 200–300 ms before its own contact, sped up where needed.
- **The same again for the third.**

Blade streaks, ported from Hexamus, come with it, so the speed reads on screen.

## What the research says

Fast action games chain blows by cancelling: the next attack starts in the frames just after the hit, and only its last fraction before contact is ever seen. That gives about 0.3–0.4 s from hit to hit, the same as a real fencer's explosive cut.

| Source | What it shows | Number |
| --- | --- | --- |
| [Cutting mechanics](https://science-and-fiction.org/swordfighting/cuts.html) (filmed at 100 fps) | An explosive cut, first motion to impact | 250–350 ms (a standard cut 600 ms or more) |
| Same | The fast arc just before impact | under 50 ms, 1000–2000°/s |
| [Combat Canceled: God of War](https://www.gamedeveloper.com/game-platforms/combat-canceled-i-god-of-war-i-action-game-design) | An attack's example: 60 frames, hits on frames 15–20; the next attack cancels in from frame 21, its input buffered from frame 0 | about 0.3 s hit to hit at 60 fps |
| Same | Kratos's attack tree in God of War II | about 4,000 cancel branches |
| Same | Cancel windows: pre-hit (before contact) and post-hit (after it) | Devil May Cry melee: post-hit only |
| [Dark Souls attacks per second](https://axebps-redlib.hf.space/r/darksouls/comments/bqk3mn/comprehensive_list_of_attackspersecond) | A deliberately weighty game's light-attack chains | fast straight sword 0.66 s per blow, fast dagger 0.46 s |
| [Inertialization, Gears of War](https://gamedeveloper.com/art/video-high-performance-animation-transitions-in-i-gears-of-war-i-) | Transitions done as a post-process on the pose, not a cross-fade of two clips | already how our walk switches clips |
| Your mocap combos (measured earlier) | Blows a real actor chained | 0.43–0.9 s apart |

Three things follow for us:

1. **Cut at contact.** Nothing after a blow's follow-through is ever shown in a combo.
2. **Enter late.** A follow-on starts 0.2–0.3 s before its own contact. Its wind-up up to there is skipped, never played or sped through.
3. **Bridge the pose.** The arm gets from where one blow ended to where the next is entered in about 0.1 s, the way inertialization already fades our clip switches.

Our own mocap combos are slower than this. The actor fought at a training pace, so they are a source of pose pairs that flow, not of timing.

## Why ours are slow

Most of each gap is the follow-on's own wind-up, played from near its start. Headless, against the sword dummy, a Medium's combos show it:

| Combo (Medium vs sword dummy) | Contacts at (s) | Hit to hit (s) | Where the follow-ons were taken up |
| --- | --- | --- | --- |
| thrust > forehand > rising | 0.43, 0.73, 1.25 | 0.30, 0.52 | forehand 0.26 s before contact, rising 0.49 s before |
| forehand > thrust > combo forehand | 0.58, 1.00, 1.55 | 0.42, 0.55 | thrust 0.37 s before contact, combo forehand 0.50 s before |
| combo forehand > (pressed in) thrust > rising | 0.67, 2.60, 3.13 | 1.93, 0.53 | thrust from the guard after stepping in |

Where the time goes:

- **The follow-on's wind-up, 0.3–0.5 s.** The chooser takes up the next strike wherever its pose best meets the last one's, and that is usually early, near the guard. That is the "back to default, then the whole next attack" you saw. Shown at 1.2×, it still takes 0.3–0.4 s.
- **The hit stop, 0.06–0.1 s a blow.** A whole blow's worth over three blows.
- **Pressing, 1.5–2.5 s.** When the dummy is driven back out of reach, the attacker steps after it in its guard and starts the next strike from the guard.
- **Long strikes.** The side chop (contact at 1.40 s), the jump chop (1.17 s) and the combo chop (1.10 s) can't be fast follow-ons at all. As openers they put a second or more before the first blow.
- **Big turns.** Some follow-ons now turn the body 75–85° in a short wind-up, because each clip's root faces differently where the two poses meet.

The blending itself is not the problem. The clip switch fades the pose over about 0.1 s without pops. What is missing is a rule for where to enter the next strike, and a bridge that carries the blade there quickly.

## The design

Each strike gets a **combo core**: the short stretch of it a combo ever shows. A combo plays the first strike whole up to its contact, then only cores, joined by quick bridges. Budget for one beat, contact to contact: about 0.4 s.

| Part of the beat | Time | What happens |
| --- | --- | --- |
| Follow-through | 0.06 s | The blade carries on past contact (or rebounds off a parry or shield), then the strike is cut |
| Hit stop | 0.06–0.1 s | Kept as for a single blow, so each hit still lands hard (decided 4 Nov) |
| Bridge | 0.10–0.12 s | The pose is carried from the cut to the next core's start |
| Chamber | 0.20–0.25 s | The next strike from its core's start to its contact, at its own pace or up to 1.2× |

Three blows: the opener's own wind-up, then about 0.8 s for the next two.

&#91;embedded content: one combo today and with cores, to scale\]

Today's case is a combo whose second blow was pressed in after a parry: 1.9 s, then 0.53 s. With cores the same three blows land at 0.67, 1.11 and 1.55 s.

### 1. Cores, found once per strike

- **Entry:** where the core starts, 0.18–0.30 s before the strike's contact. It is placed where the blade is already moving into the blow, and the body is turned and stepping into it.
- **Exit:** where it is cut, 0.06 s after contact, before the recovery begins.
- Worked out headless from each clip and written to the clips' data, as the contact marks are. A film sheet of every core lets you check them by eye.
- A strike whose blow comes from a long wind-up (side chop, jump chop, combo chop) keeps a core too. It may then follow on, entered late, where today it can't.

### 2. The cut and the bridge

- At the exit the strike is left, whatever the blow did: cut through, parried, blocked or stopped. There is no backing out to the guard in a combo.
- The bridge is the clip switch's fade, fixed at about 0.1 s for combos. The hand is never moved faster than about 15 m/s by it. A pair whose poses are too far apart for that is not chosen.
- The weapon hand keeps the way it was moving where the next blow allows it: a forehand that ends low on the left goes into a backhand chambered there. This is the figure-eight of real combinations.

### 3. Choosing the next blow

- Every strike's core is a candidate. The score is the bridge's length (hand distance and turn of the blade), the body's turn onto the target, and the flow of the hand from the last blow.
- Time is fixed by the core, so the chooser no longer trades rhythm against pose fit.
- **Pairs from your mocap combos** (the standing melee combos, the dual weapon combo, the great sword combos) are preferred where they land. The actor's own transitions are the most natural bridges we have, played at our pace.

### 4. Reach and turns

- Each core's own step and lunge carry the body in, stretched up to 1.7×. The 1.2 m nearest distance still holds.
- A dummy driven back is followed, not given up on: the next core takes one longer step in and is aimed where the dummy will be, so every blow of a combo lands. There is no walking after it in the guard mid-combo, which is what made 2-second gaps.
- The body turns at most about 45° onto the target in a core. More than that, the blow isn't chosen.

### 5. The defender

- At 0.4 s beats the dummy sees each follow-on for about 0.25 s before contact, inside its 0.26 s reaction time. A full-skill parry will miss some third blows. That is what a combination is for, and the parry skill stat (stage 3) decides how many.

## Blade streaks

The streaks port Hexamus's `MotionTrail`: a band swept by the blade's edge, strongest at the tip and gone within a fifth of a second.

- **What it draws:** the band between two points along the blade, a quarter of the way up and the tip, sampled each frame and smoothed into a curve. A fast swing reads as an arc, not straight pieces.
- **When:** only while the blade turns fast. Walking or stepping with a sword held still leaves nothing, so the guard never trails.
- **Settings, Hexamus's to start:** it lasts 0.16 s, starts at 3.5 m/s at the tip and is full at 7 m/s. A pale grey near the steel's own at 55% at the newest end, fading to a darker grey and nothing at the oldest.
- **On whom:** every sword and club in the lab, the attacker's and the dummy's, so parries show both blades' arcs. Shields have none.
- It lives in world space, so a streak finishes fading even after a hit stop or a switch.
- The lab panel gets a switch to turn streaks on and off, and their length, for tuning while you watch.

## Steps

Five steps, each one playtested by you before the next. The gates are checked headless first, against both the open and the sword dummy.

1. **Streaks and a beat readout.** The blade streaks go in. The lab panel and log show each combo's hit-to-hit times, and the time from the first contact to the third.
   - You check: the streaks' look and length. Done 4 Nov, ready to play (the panel's Attacker section switches them and sets their length, and shows the last combo's beat).
2. **Cores found.** Each strike's entry and exit are worked out and drawn on a film sheet, one strip per strike. Nothing plays differently yet.
   - You check: that each core starts where the blow is already on its way, not in the guard. Done 4 Nov: the film sheets are in the chat, and the README has the commands.
3. **Combos made of cores.** The cut at exit, the 0.1 s bridge, entry into cores, the hit stop kept, and a driven-back dummy followed with a longer step in rather than walked after.
   - Gate: a Medium's hit to hit 0.42 s or less (median), the third blow within 0.9 s of the first contact.
   - Gate: hands under 15 m/s outside the blow itself, planted feet still under 1 cm.
   - Gate: every follow-on lands on the open dummy, and every blow stays 1.2 m or further away.
   - You check: Bam! Bam! Bam!.
4. **The flow chooser.** The figure-eight preference, body turns of 45° or less, and the mocap pairs preferred.
   - Gate: no follow-on turns the body more than 45°.
   - Done 4 Nov, ready to play. For the Medium against the open dummy, 30 of 30 blows land, and the follow-ons turn the body 18° (median), 45° at most. Each strike's combos within reach of its best are gone round in turn: the panel shows "n of m". The Small's low blows still fall short at the fighting distance (step 5), and the Medium's only showstopper that can be led into is the jump chop.
   - You check: whether the chains look like one fighter's combination rather than three separate strikes.
5. **The Large and the Small.** Cores for the Large's great sword strikes, with its pack combos baked so it has more than three strikes to chain. The Small's cores, at its shorter reach.
   - Gate: a Large's hit to hit 0.55 s or less; a Small's 0.35 s or less, with 8 in 10 follow-ons landing.

Tuning after you've played step 3 sets the last numbers: the hit stop, the chamber speed, the bridge time and the streaks' settings.

## Decisions for you

1. **What "in one second" counts.** From the first contact to the third (the plan's reading: the opener keeps its own wind-up, a real anticipation), or from the first move of the opener to the third contact? The second needs a shortened opener too.
2. **The hit stop in a combo.** 0.04–0.05 s keeps the rhythm; 0.08–0.1 s, as now, gives each blow more weight but costs a tenth of a second per blow.
3. **When the dummy is driven out of reach.** End the combo there (the plan), or let the next core take one longer step in after it?
4. **The long chops.** Keep them as openers only, or let their late cores follow on (entered 0.25 s before contact, their big wind-ups skipped)?
5. **Streak colour.** Hexamus's white to pale blue, or something per unit or per weapon?

**Decided 4 Nov:**

1. One second counts from the first contact to the third. The opener keeps its wind-up.
2. The hit stop keeps its weight (0.06–0.1 s). What matters is that the blows read as one combo, and each still lands hard.
3. A dummy driven back is followed: the next core takes a longer step in and is aimed where the dummy will be. In this test state every blow of a combo lands. Breaking, dodging or parrying a combo comes later, as deliberate design, never because the attacker's own blow pushed the dummy away.
4. The long chops are leaps about a leg length high, taking off 0.6–0.7 s before contact. Since the playtest on 4 Nov they are the showstoppers: they end a combo, taken up at takeoff with the takeoff quickened. The two spinning chops land 120–160° off the way they face at takeoff, so no blow can lead into them; picked, they open a combo instead, taken up from before their spin.
5. Streaks are a pale grey close to the blade's own steel, not blue.

**From the playtest, 4 Nov:**

1. Bam! Bam! BAM!: every combo ends on its biggest blow, the showstopper. It lands harder and its hit stop is longer (0.13–0.17 s). A showstopper picked in the lab ends its combo, with two quick blows before it.
2. No lopsided rhythm. The opener is clipped too: it is taken up 0.4 s before its contact, from the guard, so each blow takes about the same time. The showstopper's beat is a little longer (about 0.5 s against 0.3–0.4 s), the wind-up of a big blow.
3. A third blow must never be held up. Pressing is gone: a follow-on takes a longer step in after a dummy its last blow drove back (up to 0.7 m), planned on the blade's reach before the arm's pull.

## Sources

- [Cutting mechanics](https://science-and-fiction.org/swordfighting/cuts.html), science-and-fiction.org
- [Combat Canceled: God of War & Action Game Design](https://www.gamedeveloper.com/game-platforms/combat-canceled-i-god-of-war-i-action-game-design), Game Developer
- [Comprehensive list of attacks per second](https://axebps-redlib.hf.space/r/darksouls/comments/bqk3mn/comprehensive_list_of_attackspersecond), r/darksouls
- [High-performance animation transitions in Gears of War](https://gamedeveloper.com/art/video-high-performance-animation-transitions-in-i-gears-of-war-i-), Game Developer
