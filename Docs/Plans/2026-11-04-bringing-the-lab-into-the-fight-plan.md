# Bringing the lab into the fight: plan

> Copy of the Claude document <https://claude.ai/artifact/9ixuGXg3bwZiJd1VXVCqFR>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 4, 2026 · @Renier

## Where we are

The lab now has every piece of a fighter, but only for one attacker against one dummy. The fight still poses everything by hand. The two can't be mixed part by part: each builds the whole pose its own way.

**The lab (clip-driven).** One unit walks, guards, steps and turns from the mocap clips. It strikes at a dummy, aimed by turning, lunging and the arm's pull, and chains combos (Bam! Bam! BAM!, the magnet). The dummy blocks with its shield or parries with its sword at a skill from novice to expert. Hits are found by sweeping the blade, with reactions, being driven back, and sounds. All three bodies work.

**The fight (hand-built).** Six units on the board. The fight loop picks targets, walks units in and away, and decides when to strike and when to combo, gated by tempo and energy. The move planner shapes each blow as a hand-made path. Damage, poise, staggers, severing, death, knock-downs, ragdolls and get-ups all work, and so do perception, guard breaks, pressure and the recorder's logs.

**Why they can't be mixed.** In the lab, the locomotor and the clip pose own the whole body and move the root with the clips' own travel. In the fight, the fight loop writes the root directly. The pose pipeline then builds the pose from a stance, Fight Rhythm offsets and the planner's paths. The first walk join blended the two and looked wrong, and was switched off. The lab's pieces also each assume one opponent: the hit finder takes one dummy, the dummy watches one striker, and the magnet and the keep-distance know one body.

## The approach

Every unit, in the lab and in the fight, becomes the same **clip fighter**: one body that both attacks and defends, built from the lab's pieces. The fight keeps its decisions and its rules and hands the motion to the clip fighter. The lab stays the test bed: since both run the same code, what is tuned in the lab is what the fight plays.

**One clip fighter, two halves:**

- **Attacking** (taken out of the lab's walker code): the locomotor and clip pose, aiming a strike at a body part, the combo player and magnet, keeping the fighting distance, holding the weapon arm through a blow, being knocked off its feet by a hard bump.
- **Defending** (taken out of the dummy): watching every blow coming at it from anyone, the shield and sword parry at its skill, hit reactions (the lean, head snap, arm knock and reaction clips), and being driven back by its steps.
- The hit finder sweeps the blade against **every** body on the board (bystanders and friends too, as the fight does now), each struck once per blow.

**What comes from where:**

| Part of a fight | Comes from | How |
| --- | --- | --- |
| Choosing targets, walking in, circling, backing off | Fight | The fight loop asks for a velocity and a facing instead of moving the root itself. The locomotor walks it with the guard clips. |
| When to strike and when to combo (tempo, energy, openings, punish, counter, press) | Fight | Unchanged. |
| Which strike, aimed where | Lab | Of the body's strikes, one that lands from where the unit stands (the lab's reach test), at the part the fight picked. This replaces the move planner. |
| The strike and combo motion | Lab | Clips, aim, cores, the magnet. |
| Finding hits | Lab | The blade sweep, the clean or glancing parry, the shield. |
| Damage, poise, stagger, flinch, severing, death | Fight | The lab's hit goes through the fight's damage rules into a hit report, so the fight's events reach the sound, recorder and camera. |
| Reactions and being driven back | Lab | The dummy's reactions, now on every unit. |
| Stumbles, knock-downs, ragdoll, get-up | Fight | Its balance rules and get-up, handing back to the locomotor once the unit is up. Leg falls stay off. |
| Blocking and parrying | Lab | Physical blocks and parries at the unit's skill instead of a rolled chance. Perception still decides when it has seen the blow. |
| Guard breaks, pressure, shell | Fight | Unchanged rules; the shell is the guard clip with the shield held high. |
| Logs | Both | The recorder gains each unit's clip, combo and defence state. |

Each step adds one switch, off until you pass it: `UnitController.ClipMotion`, with `CLIP_FIGHT=1` headless. With the switch off the fight is exactly as it is today.

## The steps

Seven steps: three in the lab, then four in the fight. Each is gated by the headless runs first, then your playtest, before the next begins.

1. **One clip fighter (no visible change).** The lab's walker and dummy are rebuilt as two clip fighters. The dummy keeps only its lab extras: its spot, being anchored, and the O stances. The hit finder takes any number of bodies.
   - Gate (headless): every lab number unchanged. Combos land 42/42 (Medium), 9/9 (Large) and 41/42 (Small); the shield and parry skill tables match; the walk's measures match.
   - Gate (you): a quick look that the lab plays exactly as before.
2. **Both fight back, in the lab.** The dummy can strike the walker (K, or a sparring mode that trades blows). The walker blocks with its own shield or sword at its skill, takes hits, reacts and is driven back.
   - Gate (you): exchanges read right both ways, with no tangles, pops or blades through bodies.
3. **Footwork against an opponent, in the lab** (the old plan's step 7). A fight-style mode where both fighters steer themselves, using the fight loop's own choices: walk in to their strikes' reach, circle by side-stepping in the guard, back off, turn to face. No blows yet.
   - Gate (headless): feet stay planted, and the walk's measures hold for the guard footwork.
   - Gate (you): the footwork looks right at fighting distance, turns included.
4. **The fight, moving only.** With `ClipMotion` on, fight units are clip fighters. The fight loop's steering becomes a velocity and facing. Every other place that moves the root goes through the locomotor: separation, knock-back, the combo's bank and stumbles. Blows are held off in this step, and the recorder logs each unit's clips.
   - Gate (headless): in 3 v 3 runs, every unit reaches and rings its target, no two bodies overlap, nothing snaps, and the feet stay planted.
   - Gate (you): the board in Unity.
5. **The fight, strikes and hits.** The fight loop starts a clip strike in place of the planner. The hit finder sweeps every body through the fight's damage rules: health, poise, severing, death and the events. Struck units react as the dummy does. Big pushes stumble or floor them with the fight's balance rules, and they get up into the walk. Defence is just the held guard in this step.
   - Gate (headless): fights end with a winner, with hits, misses and blocks counted per body, and no unit floored by its own motion.
   - Gate (you): playtests.
6. **Defence.** Every unit meets the most urgent blow coming at it, from any attacker, with its shield or sword at its own shield skill and parry skill. It only defends blows it has seen. Guard breaks and pressure work as now.
   - Gate (headless): block and parry rates per skill near the lab's.
   - Gate (you): playtests.
7. **Combos and tempo.** The fight's combo choices (punish, counter, press) play the lab's combos. Press, shell and gather shape the distance and the timing. Then `ClipMotion` is switched on by default. The old pose path stays behind the switch until you say it can go.
   - Gate (you): playtests.

**Progress, 4 November:**

- **Step 1 is built and waiting for your look in the lab.** The walker and the dummy are both clip fighters, and the hit finder sweeps any number of bodies. All 28 headless runs are identical to the baseline taken before the change: combos and single strikes for the three bodies against the open, shield and sword dummy, the parry and shield skills, and the walk. A fresh review of the lab's Unity-only path found that a walker knocked down by a bump would have had its arms held again while down; that is fixed. The one intended difference is that the dummy's physical body now has the same ground and body friction as the walker.

* **Step 1 passed** (Renier, 5 November); its logs were clean.
* **Step 2 is built and waiting for a playtest.** The dummy strikes back: Z for a strike, Shift+Z for a combo, and with "Fights back" on it answers each of the walker's blows. The walker defends by its loadout at its own skill. A sword fighter's parry kept posing the arm through its own strikes, which snapped the hands at 40–51 m/s; it now stands down while the fighter strikes and eases back after. Headless, two Mediums trading blows land, block and parry both ways, and no hand moves faster than a swing. The one-way gate is unchanged: all 28 runs are identical. For step 3: in a lunging combo the bodies come within 0.95 m, against a fighting distance of 1.2 m.

- **Step 2 passed** (Renier, 5 November). Fixed from its logs: the dummy now waits until the walker's blows are over before it answers, and its blows that meet nothing are logged. Also fixed: planted feet dragged by switches between clips, up to 26 cm. Sword-only fighters now guard left foot forward, Renier's choice, since every strike and reaction clip is left-lead. A blow during a fighter's own strike, or on one still reeling, no longer starts a reaction clip. Two bodies never come nearer than 0.85 of their fighting distance.
- **Step 3 is built and waiting for a playtest.** The lab's Footwork section has both fighters steering themselves with the fight loop's movement choices, each with a tempo. Headless: planted feet stay within 0.2 cm; the pair settles at 1.3–1.5 m by tempo; no turn clips are needed. With blows crossing, feet slide 6–12 cm at most (was 26). The one-way gate changed only where intended: combos land as before, the parry holds from the left-lead guard, and its arm moves far better (the Small's worst elbow jump went from 159 to 15 degrees a step).

* **Step 3 passed** (Renier, 5 November). Before step 4, a tangent: the parry missed the Large's combo follow-ons, and a blow's weight should be felt in every hit, block and parry. Plan: [Weight and momentum: research and plan](https://claude.ai/artifact/CvCKHzT7jXkHXDSntCJaHa).
* **The tangents are done** (Renier, 6 November). The weight plan's five steps are done and playtested, and so is the plan [Shield and blade contact](https://claude.ai/artifact/S2QGX9iFCKHkookdZewUU2). Blow numbers now count across all attackers, so two fighters can't confuse each other's blows. Step 4 is next.
* **Step 4 is built and waiting for your look on the board** (6 November, on `main`). Turn on the sandbox's "Clip motion" (on by default) and each fight unit becomes the lab's clip fighter. It keeps its health, perception, tempo and rhythm, and the fight loop steers it as a walk and a facing. Clip fighters keep 0.2 m more room from other bodies, because their walk answers a push late. Headless, 3 v 3 over 60 s:
  - every unit reaches its target in 5.6 to 7.2 s;
  - no root jumps;
  - planted feet stay within 0.2 cm, except one plant at 2.2 cm;
  - bodies overlap on 12 of 3,600 steps, never nearer than 0.49 m.

  With the switch off, the old path is unchanged.
* **Step 4 passed** (Renier, 6 November): the units walk fine, and nothing intersects. **Step 5 is built and waiting for a playtest.** The fight loop still chooses when to strike and at whom, and a clip fighter strikes with the lab's strikes and combos. Each body a blade meets takes the fight's damage and poise and reacts as the lab's dummy does. When its health runs out it dies: it falls and doesn't get up. A blow stops in an ally instead of sweeping on, and a fighter doesn't swing with an ally in the way. Bodies are kept apart as the feet allow. Headless, three 3 v 3 fights end with a winner in 15 to 42 s. No unit falls except when killed, and bodies never overlap. Not yet: defence beyond the held guard (step 6) and severing. Left over: planted feet still drag up to 43 cm in the press of blows (the lab's spar had 21 cm).
* **Steps 5 and 6 passed, step 7 and severing built** (6 November, on `main`). Step 6, defence, was done as part of [Fighting off several attackers](https://claude.ai/artifact/THdxBZEQwpknnfqRM1ZrgC). Step 7: a clip fighter's blows now reach its tempo and rhythm. Each blow costs energy and counts in a press's burst; before this its energy never fell and its counter never knew it had answered. Combos are as long as the tempo makes them: a counter's two blows, a press's, punish's or full gather's three. Headless over three 3 v 3 fights, 20 to 24% of blows came in combos (before, none were counted). Clip motion is already the default in Unity; the old pose path stays behind the switch. Severing: an edge across a joint cuts the limb off, by the fight's own rules. Losing the weapon arm takes the weapon, the shield arm the shield, and a leg the footing: the unit stays down and bleeds out. Headless, five limbs came off across four fights. Still open: planted feet drag in the press.

## Not coming yet, and risks

**Left out under the clip fighter, for now:** the Fight Rhythm's hand-made postures, sways, feints and lead-ins; shield bashes and off-hand jabs (no clips); the Small's dodge (M12); kicks; leg falls. The old fight keeps all of them behind the switch.

**Risks:**

- **Six bodies at once.** Clip posing, IK and the parry's solves now run six times. Each fight step's cost is measured from step 4 on.
- **Crowding.** The lab only ever had two bodies, one of them standing still. With six, blades and bodies cross. Keeping two units apart and choosing a strike that lands both have to work against more than one neighbour.
- **Reach and the board.** The lab's strikes each have one natural distance. The fight loop's engage distance has to come from those reaches, or units will stand where nothing lands.
- **The logs.** Today's fight logs describe the hand-built blows. Until step 4 adds the clip state, the fight's logs say little about the new motion.

## Questions

**Renier's decisions (4 November): the plan is approved, starting with step 1.**

- **Aim: locked at each blow's start.** Follow-ons are each aimed afresh, and the magnet only follows a target the unit's own blow drove back.
- **Defence: physical**, with a shield skill and a parry skill per unit, 0.5 for every body to start.
- **The Fight Rhythm's postures and feints are left out for now**, to come back later as clips.

The questions as asked:

1. **Does a blow follow its target, or is its aim locked when it starts?** The pillar says a blow locks the target part's position at its start, so a unit that moves can make it miss. In the lab, a strike's hand follows the target within the arm's 0.4 m pull, and the combo's magnet snaps the body after it, so nearly everything lands unless it is blocked.
   - Recommended: each blow locks its aim at its own start, per the pillar. A combo's follow-ons are each aimed afresh as they start. The magnet only follows a target its own last blow drove back.
   - Or: keep the lab's following, so dodging and footwork matter less.
2. **Defence: physical skill instead of a rolled chance?** Today the fight rolls a block or parry chance, then poses it. The lab blocks or parries physically, and skill sets how well. The plan gives each unit two stats, shield skill and parry skill, as you asked earlier.
   - Recommended start: 0.5 for every body, set per unit on its profile.
3. **Is it all right to leave out the Fight Rhythm's postures and feints for now?** Under the clip fighter, between blows a unit shows the guard clips and their footwork, nothing hand-made. The tempo still decides distance and timing. Clip-made feints and sways can come back as a later step.
