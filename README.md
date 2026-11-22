# Physics Battler

An auto battler where fights play out physically: attacks are animated toward the real position of the body part they
aim at, and whatever the weapon or a flying body actually touches takes the hit.
Unity 6.6 (6000.6.7f1), URP, PC.

**New to this project, or a new chat? Read `CLAUDE.md` first**: the short briefing (how the work is done, the
design rules, where things stand, the roadmap). This README is the full build log: search it, do not read it whole.

Design spec (animation-first combat): https://claude.ai/artifact/HJeFzW1bbvt7n4gsLSUoUT
The main plan in hand, the Fight Rhythm plan: https://claude.ai/artifact/GmgWWcXjaoQvCj5n5U5wLh

The first prototype (active ragdolls with muscles and a balance assist) is kept on the branch
`backup/active-ragdoll-v1`. This branch starts over from the spec above.

## Branch locomotion-lab: the walking overhaul

Plan and progress: https://claude.ai/artifact/PRykbXtbFivnTjXWBN2ntf

On this branch the sandbox runs the **locomotion lab** instead of the fight; untick *Locomotion lab* on the
BattleSandbox to get the fight back. In the lab, one Medium walks using the new locomotion
(`Scripts/Runtime/Locomotion/`). The walk is built from the Mixamo reference clips, which are baked into
`Resources/Locomotion/LocoClips.json` by `Tools/Mocap/Locomotion/loco_sample.py` and `loco_bake.py`.

- **What it does:**
  - Forward, side, backward and running cycles, blended at a shared gait phase.
  - Start and stop clips. A stop is matched to the distance left, so it ends on the spot.
  - Turns on the spot and 180° pivots.
  - Feet lock when they land and roll heel to toe. Planted feet move at most 0.5 cm over the whole course.
- **Keys:**
  - C switches between the course and driving: W A S D (relative to the camera), Shift to run, Space to keep facing the same way, V to change walking speed.
  - B switches the body between physical (legs carried exactly; torso, head and arms pinned) and kinematic.
  - G shoves the chest and the walk steps to catch the body; K shoves hard enough to knock it down (a ragdoll, up
    again after 3 s); H throws a heavy ball at the chest, and the knock pushes the walk.
  - L changes what it carries: its own loadout (as its marker in the scene holds it), bare hands, or any other
    marker's. A carrying arm keeps the walk's swing, smaller (a weapon about 3/4, a shield about 2/5), about where
    it holds the thing: the weapon low at the side, point forward and a little down (lifting as the hand swings
    forward), the shield in front of the belly facing forward. N changes the body.
  - The combat clips (the combat overhaul's step 1): `<` and `>` in the panel (or the , and . keys) pick one of the body's guard and
    strike clips (the Medium's, for every body for now), J plays it as it is: its root moves the
    body, its feet lock where they land, its own hands hold the weapons. Baked by `Tools/Mocap/Combat/combat_bake.py`
    into `Resources/Locomotion/CombatClips.json`; filmed headless with `combatfilm <dir> [Medium|Large] [clips]`.
  - The guard (the combat overhaul's step 2): T raises it and lowers it again (over half a second). In the guard
    the walker plays the guard's clips (its standing, walking forward and back, side-steps, running, turning on the
    spot) blended in at the same gait phase, the clips' own hands holding the weapons; standing, the feet step into
    the guard's wider stance and back out. Driving in the guard, it faces where the camera looks and steps whichever
    way it is driven. Headless: `LOCO_COURSE=guard` (with `LOCO_ARMS=sword`).
  - Aimed strikes (the combat overhaul's step 3): driving (not on the course) a second Medium stands ahead as a
    training dummy, holding a Medium's own loadout. In the guard the walker faces it; J plays a strike clip aimed at
    it: the body turns into it (up to 35°), the lunge stretches or shortens (0.6 to 1.4 times the clip's), the arm
    reaches up to 0.4 m toward it, and about contact the wrist turns the blade onto it (a thrust's all the way). U
    picks where: low (its hips), middle (its chest) or high (its face); Y puts it straight ahead as far as the
    picked strike reaches; the panel shows each strike's reach. Where each strike's weapon is at contact on each
    body is measured headless into `Resources/Locomotion/StrikeReach.json` (`strikes <dir> <Small|Medium|Large>
    --measure`); `strikes <dir> <body> [clips]` aims each at targets (`AIM_REACH=1`: about its own reach).
  - The Large uses the Medium's sword and shield clips for now (its club in the sword hand, the shield hand empty):
    `LocoLibrary.LargeOwnClips` switches back to its own two-handed clips (lg_, with the left hand on the handle).
    A strike plays out before J starts another.
  - Hits (step 4): the weapon is swept from step to step about each strike's contact, and the first part or shield
    of the dummy it meets is struck by the fight's own damage rules. Both stand still a moment (the hit stop). A
    blow that cuts through slows; one that is stopped (a shield, a body too heavy for it) is shown where it met,
    backs out a little and fades into the guard. While a strike plays, the weapon arm is held exactly to its pose
    (kinematic, as the fight holds it in a blow): pinned, the physical sword trailed a 16 m/s swing and missed what
    the pose struck, which is why so many hits went through the dummy in the lab and not headless.
  - The dummy is an enemy unit that doesn't strike back (a `ClipFighter`, as the walker is): a Medium in the Medium's guard, nothing
    dumbed down. A blow throws its upper body back by the blow's lever, snaps its head, knocks a struck arm or the
    shield aside, and plays the pack's impact clips by how hard it was (impulse per kilogram): a light flinch, doubled
    over for one to the body, thrown back for one to the head, the chest's top, a heavy blow or a broken poise; an
    arm struck only flinches. What the struck part doesn't take drives it back with the walk's own steps, after the
    reaction has shown (the fight's rule, `KnockScale`): the Large's blows drive a Medium back 0.3 to 0.9 m.
    `Anchored` makes it a training post again. It steps back onto its spot a second after it was last struck.
  - The dummy's setup (the panel's Defender section, or O to cycle): **open** (its guard down, a sword only, no
    defence: every blow lands), **sword and shield** (it meets each blow with its shield, read from the striker's
    wind-up, `StrikeHits.Read`), or **sword only** (it parries with its sword, `SwordParry`). Shield skill and
    parry skill are separate stats (levels come later; for now both are full).
  - The sword parry (stage 1 of the defence plan): from a medium guard (the hand before the belly, the point at the
    opponent's face, the free hand behind the left hip), the dummy reads each blow's path from the striker's clip
    (where the blade first reaches the body: torso, head or either leg; the striking point's and the tip's path),
    and about 0.3 s before contact takes the sabre manuals' parry for it (St George or hanging for the head; tierce,
    high tierce, quarte, high quarte, prime for the body; low tierce, low quarte, seconde, septime, octave low). The
    forte goes onto the incoming blade's path, the blade square to the plane it sweeps, the trunk turned and leaned
    with every parry. The arm is the fight's arm rig: the elbow on the parry's null and swivelling at most 540
    degrees a second, the hand turning at most 1,200 degrees a second, the wrist within its limits, the arm clear
    of the torso. Blade meets blade as two segments' nearest points: a parried blow is stopped (the hit stop, it
    backs out) and knocks the sword arm and drives the dummy back. `parryreach <body>` tests each manual position
    on the arm rig.
  - Stage 2: the blow is read as it will really come. `StrikeHits.Read` plays the blade on from where it is now:
    the clip's arm carried by the root as the lunge will move it (cut short or stretched, the aimed turn spread
    over the wind-up: `Locomotor.ShotRootAhead`), the aim's pull as it will be then (`LocoBody.PullAhead`) and the
    wrist's steer (`LocoBody.SteerAhead`), from 0.28 s before contact to the end of its live moments. (Before, the
    path was hung on where the strike was aimed, and missed where it went by up to a metre.) Each parry is then
    tried against that path with the sword, the arm and the body as they would be (`SwordParry.Meets`): the one
    held is kept while it meets the blade first, else the one read, else whichever does, nearest to hand; within
    0.15 s of the blow landing it is no longer changed, only placed. Outcomes: a clean stop on the forte (the
    attacker's hand knocked back, `LocoBody.KnockHand`; the dummy's arm and body take the rest) or a glance off
    the foible or a flat crossing, which goes on at half its speed past the sword arm. A parry is clean only when it
    takes the attacker's blade a quarter of the way along or more (not a jam at the hilt) and before that blade is
    at the head or the arm; a blow at the head is met at least 0.3 m before the face. Any weapon touching a body
    outside a blow pushes it, and a leg struck twists at the thigh. The upper arm turns at the shoulder at most 900
    degrees a second (`UpperArmSpeed`): the hand's step is held back to it.
  - Distance: the two stand 1.6 m apart between blows (root to root for two Mediums, `ClipFighter.StandingDistance`);
    the attacker strikes from there, its lunge taking it in, and steps in only for a strike that can't reach from
    there (the lab's `SparSpot`). No blow takes them nearer than 1.2 m (`FightingDistance`: about half a metre
    between the shield and the other body): the lunge is cut short there, pressing stops there, the aim's pull
    never takes the hand into the other body. After a blow the dummy steps back to 1.6 m. From there every Medium
    strike lands but the quick combo backhand (its reach is 0.7 m). Pressed on (by a shield or a body too close),
    the sword is drawn out to its side and raised toward upright, and the forearm is kept off the shield too. Its sword is kept out of the attacker's body and shield (`OpponentShape`), and where the
    shield swings into its guard's point anyway, the sword is pushed off, turned at the wrist up to 25 degrees a
    step and eased back at 120 degrees a second while it stays clear (the guard keeps 0.15 m off it to begin with).
    Back to the guard after a parry at half the parry's speeds. The parry's grip stays within the arm's reach.
  - A jumping strike stopped in the air (blocked, parried, or stopped in the body: `Locomotor.ShotAirborne`) no
    longer backs out of the clip, which had it float back up and drop into the stand: it comes down where it is,
    its own landing played on (`StopShotTravel`), the weapon arm held off the follow-through and brought back to
    the clip's closing guard over 0.4 s (`LocoBody.HoldWeaponArm`). The pack's Falling_To_Landing is sampled
    (`samples/falling_to_landing.json`) if a jump ever needs a landing of its own.
  - Headless (the attacker carrying its shield), the sword dummy parries 23 of 27 of a Medium's strikes, 9 of 9 of
    a Large's, 15 of 27 of a Small's. Not yet: footwork (stepping back from a blow too close), the Small's quick
    low blows, and a rare elbow flip of the arm rig in the folded guard. Combos: pressing in after a strike stops
    at the fighting distance and may take up any strike from the guard there (26 of 27 land); the close pair
    combo_backhand > combo_chop no longer links at that distance. M puts the lab's panel back at the top left.
  - Combo flow (the Bam! Bam! Bam! plan): the time from one blow's contact to the next's is part of a link's cost
    (`StrikeCombos.TimeCost`: 0.2 a second, and 2.5 a second more past 0.55 s); a strike may be left 0.1 s after
    its contact and the next taken up as late as 0.25 s before its own; the follow-on's chamber (its wind-up) plays
    up to 1.2 times as fast, easing back to its own pace by contact (`Locomotor.Chamber`). A blow stopped by what
    it met (parried, blocked, stopped in the body) goes straight into the next from where it stopped, without
    backing out to its guard (links up to cost 3.2 there: the switch's fade carries the difference). A follow-on
    may lunge to 1.7 times and turn the body up to 90 degrees onto a target that was driven back, aimed 0.9 of
    the way to where it will be. With no quick follow-on, the strike's recovery is cut 0.3 s after its contact and
    the next is pressed in: stepped after at 2.2 m/s and taken up 0.45 s before its contact, chamber quickened.
    The dummy waits 0.8 s after a blow before it steps back. Headless (`combos <dir> <body>`, `COMBO_STANCE=sword`
    against the parrying dummy): a Medium's blows land 0.52 s apart (median) on the open dummy and 0.53 s on the
    sword dummy (1.58 s before); a Small's 0.4 s. Not yet: the Large (three strikes, 1.7-2.7 s), the long chops
    as follow-ons, the authored combos.
  - Combo feel, step 1 (the plan doc "Combo feel: Bam! Bam! Bam! in one second"): blade streaks and the beat.
    Each sword and club leaves a pale grey streak (`BladeStreak`, from Hexamus's MotionTrail): the band from a
    quarter of the way up the blade to its tip (a club from just under half), drawn only while the tip turns
    round the base faster than 3.5 m/s (full at 7), fading over 0.16 s and from the tip down to the base, in world
    space so it finishes fading through a hit stop or a switch. The attacker's panel switches them off and on and
    sets how long they last; it also shows the last combo's beat (the time from each contact to the next, and from
    the first contact to the last; also in the log), the measure the next steps are gated on.
  - Combo feel, step 2: each strike's combo core (`StrikeCores`, worked out from the clip's own tracks; nothing
    plays differently yet). Its entry is 0.03 s after the chamber's turnaround (the blade tip, turning round the
    shoulder, last slower than a fifth of its contact speed), kept 0.18 to 0.30 s before contact; its exit 0.06 s
    after contact. A strike in the air at its entry is entered at its takeoff instead: the three long chops leap a
    leg length high 0.6 to 0.7 s before contact, so their cores are leaps, which open combos but never follow on
    (entering one in the air would lift the body a leg length in the bridge's 0.1 s). Headless: `cores <dir>`
    prints the table and writes cores.json; `python3 Tools/Mocap/Combat/coresheet.py <dir>/cores.json sheet.png
    [sns_,lg_]` draws the film sheet (tip speed with the marks; the body from the target and from above at the
    entry, contact and exit; the tip's path, thick over the core).
  - Combo feel, step 3: combos of cores, Bam! Bam! BAM! (`ComboPlayer.Cores`, on for the Small and the Medium; the
    Large keeps the earlier chooser until step 5). A combo is planned up front (`StrikeCombos.CoreChain`) and ends
    on its showstopper: a leap (the jump chop, the side chop, the combo chop), or with none the heaviest blow
    (`StrikeCore.Weight`). A showstopper picked ends its combo, two quick blows before it; any other strike picked
    opens it. The quick blows are the ones whose cores meet best: the hips lined up at the link, the root's turn
    there and the aim's turn the next blow needs (`StrikeLink.Aim`: no link whose blow lands more than 50 degrees
    off the way the last one did, so the spinning chops, which land 120 to 160 degrees off where they take off,
    never end a combo; picked, they open it, taken up from before their spin). Each blow is cut 0.06 s after
    its contact (or where it was stopped), and the next taken up at once at its core's entry, its chamber
    quickened to take about 0.42 s (`StrikeCombos.CoreChamberSpeed`, up to 2 times for a leap's takeoff). The
    opener is taken up 0.4 s before its contact, from the guard. A follow-on aimed at a target its last blow
    drove back takes a longer step in on top of its own travel (`ComboPlayer.StepIn`, up to 0.7 m, eased over its
    wind-up, never nearer than the fighting distance): no pressing, no combo given up. The showstopper lands 1.5
    times as hard, with a longer hit stop (0.13 to 0.17 s; `StrikeHits.Finishing`). Fixed on the way: a link's
    root turn snapped the shield hand round for a frame (`LocoBody.SmoothRootTurn`; the clip's own pose is now
    turned with the root too), and a jump strike stopped in the air dropped its held arm in a frame at the next
    blow (now faded out). Headless (`combos <dir> <body>`; `COMBO_OLD=1` the earlier chooser, `COMBO_AIM=1` where
    each blade was at contact, `COMBO_HAND=<strike>:<from>-<to>` the weapon hand each frame): the Medium lands 27
    of 27 blows on the open dummy and the sword dummy, 0.25 to 0.38 s between the quick blows and 0.47 to 0.55 s
    into the showstopper, the third 0.73 to 0.93 s after the first; the Small 26 of 27, 0.22 to 0.37 s; the hands
    at most 15.6 m/s about the switches.
  - Combo feel, step 4: the flow chooser. A link's cost now also counts the figure-eight (`StrikeLink.Flow`: the
    weapon hand going on the way it was moving into the next blow's start, not back the way it came) and takes off
    a pair the mocap actor did in a row (`StrikeCombos.MocapPairs`: the four-cut combo's forehand into its
    backhand, the Large's three-strike sets); no planned link turns the body more than 45 degrees onto its
    target. Each strike's combos within 0.5 of its best are gone round in turn (`StrikeCombos.CoreChains`; the
    panel shows "n of m", each I plays the next). As it goes, a follow-on that would turn the body more than 45
    degrees from where the last blow actually left it (a blow stopped early has turned less than its plan) is
    swapped for the unused one of its part that turns least (`ComboPlayer.Reconsider`; never the strike picked,
    `ComboPlayer.Keep`). Headless, the Medium against the open dummy: 30 of 30 blows land, the follow-ons turn the
    body 18 degrees (median), 45 at most; against the sword dummy 16 of 20 within 45 (the jump chop, its only
    showstopper that can be led into, is kept). The Small lands 25 of 30: its low blows don't reach the dummy's
    hips from the fighting distance (step 5). Not yet: a second showstopper for the Medium (its other two leaps
    are spinning chops, which can't end a combo).
  - Combo feel, from the playtest after step 4: hard left, right, left. Each cut sweeps back the way the last one
    came (`StrikeCombos.Side`: forehand 1, backhand and rising backhand -1, chops and thrusts 0), the middle blow
    always a cut (never the thrust), the showstopper back across again or straight down. The showstoppers are now
    the jump chop and the heaviest cut each way (the diagonal forehand, the rising backhand), and going round a
    strike's combos (I) gives each showstopper's best in turn. A strike that leads well into nothing may go in a
    combo's middle. The swap as it goes keeps the sweep. Blows before the showstopper drive the target back half
    as far (`StrikeHits.ComboKnock`), so the next still reaches it; the showstopper 1.5 times. A strike taken up
    late in its wind-up lets go of the last motion's hand turn by 0.08 s before contact (`LocoBody.HandFadeGoneBefore`):
    the fade alone left the Large's club pointing up to a metre off as its follow-ons landed. The Large plays
    combos of cores too (combo forehand > backhand lunge > forehand: 9 of 9 land, 0.55 s and 0.35 s apart, the
    third 0.9 s after the first). The lab's sparring spot is where the combo's opener reaches from, taken up
    where the combo takes it. Headless: the Medium lands 32 of 33 (open dummy) and 33 of 33 (sword dummy), 0.35 s
    hit to hit; the Small 32 of 33, 0.23 s; single strikes, the Medium 27 of 27.
  - Combo feel, the whole slash (playtest: cut at contact, the slash's tail was lost, and every combo read as
    short blows). A core now runs on through its contact until the slash has swept out (`StrikeCores.Exit`: the
    blade's tip slower than 12% of its speed about contact, 0.18 to 0.35 s after it), the follow-through played
    1.8 times as fast (`ComboPlayer.FollowSpeed`; the showstopper's at its own pace). In a combo a blow that lands
    on the body sweeps on through it, slowed to no less than 0.9 (`StrikeHits.ComboCarry`), and pauses once a blow,
    on its first contact. The blows knock harder: 1.5 times a single blow's, the showstopper 2 times
    (`StrikeHits.ComboKnock`, `FinisherKnock`). The magnet (`ComboPlayer.Magnetic`): the follow-on steps in as far
    as its blow needs (up to 2 m), and through each wind-up the body is carried after its target as it is driven
    off from where the blow was aimed, the gap closed by contact, never nearer than the fighting distance
    (`LocoBody.CarriedBy` keeps the arm's pull from asking for it again). Headless: the Medium lands 42 of 42 on the
    open dummy, 0.47 s hit to hit, the third about 1 s after the first; the Large 9 of 9, 0.92 s; the Small 41 of
    42, 0.72 s. Against the sword dummy 33 of 42: the backhands' cores don't reach its hips from the fighting
    distance when it parries.
  - The magnet snaps (playtest: a second or third blow still missed now and then, and the pull should snap, not
    slide). Through each blow's wind-up the body is snapped across the gap between where the swing itself lands
    the blade (without the arm's pull) and where the target is (`ComboPlayer.SnapTime` 0.03 s, the e-folding),
    led only by the snap's own lag, never nearer than the fighting distance; the arm's pull needs that much less
    (`LocoBody.SnappedBy`). The step in at a blow's start is taken in its first 0.08 s (`Locomotor.StepInOver`),
    and steps in until the swing lands within 3 cm. Headless: 42 of 42 blows land for the Medium, the Small and
    (9 of 9) the Large; the second blows pass within about 10 cm of their aim (15 to 30 before).
  - Parry skill (the defence plan's stage 3; `SwordParry.Skill`, the dummy's `ParrySkill`; the Defender section's
    − and + in quarters, novice to expert). Its levers at skill 0 (all off at 1): it moves only 0.4 s into a blow
    and 0.12 s before contact (0.1 s and 0.32 s), reads where the blow lands up to 25 cm off and reads it again only
    every 0.3 s (every step), takes the wrong parry 3 times in 10, and goes where it read the blow rather than
    playing its parry on against the blade it sees (an expert always does), its hand at 2.5 m/s (6), its blade
    hardly squared to the blow (a shallow cross) and met out toward its own foible. The contact's rules (stage 2)
    then let the blow glance through. Each blow's slips are drawn as it starts, the same each run; on a curve, so
    skill 0.5 is about midway. Headless (`PARRY_SKILL=<0..1>` with `dummyhits` and `DUMMY_STANCE=sword`, or with
    `combos` and `COMBO_STANCE=sword`): of a Medium's 27 single strikes it parries 24 at skill 1, 25 at 0.75, 17
    at 0.5, 12 at 0.25, 10 at 0 (glancing 0, 0, 3, 6, 7); of 42 combo blows 17 at skill 1, 16 at 0.5, 9 at 0,
    most of the expert's on the opener. Not yet: giving ground when late, and a second parry after a feint (there
    are no feints yet).
  - Shield skill (the defence plan's shield table; `ClipFighter.ShieldSkill`, the Defender section's − and + in quarters).
    At skill 0 (all off at 1): it moves its shield only 0.05 s before contact (an expert as soon as it reads the
    wind-up; between them most of the scale is spent on 0.05 to 0.2 s, where a block is won or lost), 0.35 s into
    the blow at the soonest (0.12 s), chases the blade where it is instead of closing the line it will come in on,
    reads where it lands up to 25 cm off to one side of the line and the line up to 30 degrees off, keeps to its first
    read (every step), its shield comes at 2 m/s building up to it (6 m/s) and turns at 200 degrees a second (720:
    no whipping round at the last moment), it holds the shield at the chest against a low cut, takes a blow square
    on (an expert's shield is slanted 30 degrees and turns half the knock aside: "blocked by the shield, turned aside"),
    and stays up 0.6 s after (0.25 s). Headless (`SHIELD_SKILL=<0..1>` with `dummyhits` and `DUMMY_STANCE=shield`,
    or `combos` and `COMBO_STANCE=shield`): of a Medium's 27 strikes it blocks 23 at skill 1, 24 at 0.75, 16 at 0.5,
    10 at 0.25, 9 at 0 (the guard alone, never moved, 8); of a Large's 9, 9, 8, 3; of 27 combo blows 25 at skill 1,
    26 at 0.5, 16 at 0 (a shield still up from the last blow meets the next). The expert's 4 are its geometry:
    an overhead chop aimed at the hips passes the head above the shield, and a thrust takes the shield arm.
  - Leg hits and falls (the defence plan's leg hits; `ClipFighter.SweepLeg`). A blow on a thigh, shin or foot is weighed
    against the weight that leg carries (the blow's knock over the body's mass; its parts' knocks within 0.2 s added,
    one blow sweeping through both legs) — shelved, off (`ClipFighter.LegFalls`; `LEG_FALLS=1` headless): the falls
    and the get-up didn't read well in the playtests, so a leg hit is a body hit with the leg twisted. On: on both feet it stumbles from 0.3 m/s on that leg (a catch step, the swept
    foot let go) and falls from 2 (under the fight's 2.2 for a push); with the other foot off the ground its weight is
    all on the leg struck: it stumbles from 0.25 and is swept from under it from 1.2; a foot in the air is knocked away
    from 0.25 (a stumble) and only a blow over 1.8 drops it. A Medium's sword makes it stumble; only a Large's solid
    cut drops it (the playtest: every thigh cut dropped it). Down or getting up, a blow only knocks the part it struck
    (no reaction clip or softened hold, which hauled the ragdoll up to a standing pose). Down, the body goes to physics, the swept
    leg thrown along the blow and the body over it; it lies at least 1 s and until still (2.5 s at most), then gets up
    with the fight's own get-up (`Motion/GetUp`, face up or face down, its guard taking over at the end). Not when
    anchored. Headless (`DUMMY_LOW=1` with `dummyhits`: the low blows at its knees): a Medium's thigh cuts on a
    planted Medium make it stumble; the Large's shin cuts there (1 to 1.6 m/s) make it stumble, its solid thigh cuts in
    the lab (2.5 to 3.2 m/s) drop it; struck anywhere else it never falls. A combo's magnet carries the body at most
    0.6 m a blow and 0.12 m a step (a Medium's; the playtest: a Large snapped 1.5 m in a frame, went down and slid off
    across the board), and a body knocked down keeps no more than the knockdown speed. (Headless has no body
    to fall: the fall and the get-up are seen in the lab: U's fourth aim, legs, sends every strike and combo blow at
    its knees.)
  - Clip fighters (the plan "Bringing the lab into the fight", step 1; `Locomotion/ClipFighter.cs`): the walker and
    the dummy are now the same kind of body, a clip fighter that walks, guards, strikes and defends from the clips.
    A step is `Move` (the walk, the strike or combo, the pose, the defence) then `Apply` (the arms' holds, the
    physical body driven to the pose); the dummy steers itself with `Step`. `EnableStrikes` gives a fighter its
    blade's hits, swept against any number of bodies (`StrikeHits.Step(dt, bodies)`: the first part, shield or
    sword of anyone's the blade meets, each body's parts struck once a blow, its own never). The walker doesn't
    defend yet (`Defends`), since nothing strikes it; the dummy keeps its lab-only spot and training-post switch.
    Nothing plays differently: headless, every combo, strike, skill and walk figure is unchanged.
  - Both fight back (step 2). The dummy strikes the walker: Z a strike, Shift+Z a combo, and with "Fights back"
    (the Defender section, on by default) it answers each of the walker's strikes or combos 0.45 s after it ends,
    every third answer a combo. It picks one of its strikes that lands from where it stands
    (`ClipFighter.StrikeThatLands`: the blade within 10 cm of its aim, the turn, lunge and arm's pull taken), aimed
    at the walker's part nearest the strike's height (`AimAt`), and plays it (`StrikeAt`, `ComboAt`). The walker now
    defends as its loadout has it, at its own skill (the Attacker section's − and +, 0.5 to start): the shield
    blocks, a sword alone parries, a club or bare hands take the blows. It reacts and is driven back as the dummy
    is; a blow that lands on it is not a bump (no knock-down from it). Either one's hit stop holds both. A sword
    fighter's parry stands down while it strikes with that sword (it had kept posing the arm, and fought the
    strike's), and takes the arm up again from where the strike leaves it (`SwordParry.Resync`); its arms and trunk
    are faded into the strike's over 0.2 s (`ClipFighter.HandOffOver`), the free arm back to its tuck over 0.3 s.
    Headless: `spar <dir> [s]` (two Mediums trade blows, `SPAR_A`/`SPAR_B` open, shield or sword, `SPAR_SKILL`):
    blows land, are blocked and parried both ways; no hand faster than a swing's 22 m/s.
  - Footwork (step 3; `Locomotion/Footwork.cs`, the panel's Footwork section): both steer themselves, as the fight's
    units do (`FightLoop.Approach` and the Fight Rhythm's circling, as a velocity and a facing the guard's clips walk):
    each walks in to its striking distance (the middle of its strikes' reaches, never inside the fighting distance),
    slowing over the last 0.4 m; holds it; backs off only when crowded or holding back; near, goes round the other
    sideways in spells (1.5 to 3.5 s, then standing 0.8 to 2.2 s, either way round; the two go round the same way);
    and faces it. Its tempo moves the distance: Probe its striking distance, Gather just out of the other's reach,
    Press closer in, Shell giving ground. Two bodies never come nearer than 0.85 of their fighting distance
    (`ClipFighter.KeepApart`: each pushed straight off the other at once, the lighter further; two lunges at once
    passed through each other). Feet, from the step 2 playtest: a sword-only fighter now guards left foot forward as
    every strike and hit reaction clip is (the fencer's right-lead guard dragged a planted foot through the switch:
    up to 19 cm); a blow on a fighter in its own strike doesn't start a reaction clip (a flinch or a stagger cuts the
    strike short, a lighter blow lets it carry on: the fight's rule), nor on one still reeling from the last (each
    switch dragged a planted foot up to 26 cm). Headless: `footwork <dir> [s]` (`FOOT_A`/`FOOT_B` probe, gather, press,
    shell): planted feet within 0.2 cm, 1.30 to 1.49 m apart by tempo, no turn clips needed; `spar` with
    `SPAR_CLASH=1` (blows that cross): planted feet within 6 to 12 cm (was 26), the bodies no nearer than 1.02 m.
  - Every combo blow met (the weight plan "Weight and momentum", step 1): a parry reads each blow afresh by its serial
    (`BlowRead.Serial`), so the last blow's parry is no longer held for the next; a combo's next blow is read ahead
    out of the last one's follow-through (`ComboPlayer.Upcoming`, `StrikeHits.ReadUpcoming`: planned as the strike will
    be, the link, the step in, the aimed turn and lunge, the magnet, the capped pull), about 0.25 s before contact
    instead of 0.15; a parry taken on that read is looked at once more as the blow comes into view; one held from the
    last blow is kept only where it meets the blade. The blow's knock goes into the parry (the sword arm is moved by
    it, up to 0.12 m at full knock). Giving ground (`ClipFighter.GiveGround`) is a last resort for an arm 0.05 s late:
    given at any lateness it stepped away from where the arm put the sword. Headless (`combos`, expert sword): the
    Medium's 42 blows 26 parried, 7 glanced, 9 through (was 20, 8, 13); the Large's club 9 of 9 parried.
  - Parries that divert (the plan "Parries that divert: research and plan", steps 1 to 4): the blades meet and stay
    together, and the parry turns the blow aside; nothing passes through the sword. The meeting covers the whole
    area the attacker's blade swept in a step, so a fast club can't cross the sword between samples. At contact
    the strike is put back to where the blades met and held there (`StrikeHits` bind; `LocoBody.Bind` holds the
    weapon hand off its clip). A clean parry then sweeps it out off the body at the blow's own speed
    (`SwordParry.Sweep`), the two blades together, out to 0.4 m beyond the body's middle; then it backs out, and a
    clean parry ends the combo. A glance carries it out and along the sword past its point, then it backs out; its
    combo goes on, and the blow is spent on that body. Two point-down parries for blows below the knees (under
    0.6 m at a Medium's size): low prime over the left leg, low seconde over the right. The read-ahead counts a met
    blow's next one as starting at once (a met blow takes up the next as it backs out). Headless (`combos`,
    `COMBO_TARGET=legs` for leg-aimed runs; each run reports the steps the blade went through the sword unmet):
    such steps 97 to 4 in 18 runs; the Large's club met by every skill; the expert lets 4 of the Medium's 25
    blows through (was 9 of 42) and 3 of the Small's 14 (was 23 of 41). A clean parry holds the blow on the middle
    of the blade (`SwordParry.HoldAt`): met wherever it first touches (the Large's long club near the hilt, over
    the hand), the sword slides along itself at once so the blow sits on its middle, then sweeps. The lab writes
    the parry's read of each blow into the session log ("the parry: blow N: ..."); `combos` takes
    `COMBO_PLACE=x,z` to start the dummy off to the side and further away. A fighter with one weapon alone, a club
    as much as a sword, takes the sword-only stance: its free hand tucked out of the way and its weapon parrying
    (the playtest: the Large's club alone guarded as if behind an unseen shield); the logs name the weapon
    ("parried by the club"). A club is held as a brute holds one (`SwordParry.AsClub`): its guard upright,
    leaning forward and a little out (not pointed at the other like a sword); its free arm is left as its guard
    clip holds it (at its side or behind its back looked wrong on the Large), and the parry keeps blows off that arm
    too (without, the expert took 16 hits in 8 of the Medium's blows; with, 3). Headless: `spar` with
    `SPAR_A=club` (a Large with its club; `SPAR_TRACE=from,to` prints the parries' reads): the expert parries 8 of
    the Medium's 8 blows with its club; the club flicks back and forth 21 times in 40 s out of its strikes (45 with
    the pointed guard; a Medium's sword 51).
  - The blow's weight (the plan "Weight and momentum", step 2): a blunt blow (a club's head, a fist, a shield's
    face) never cleaves through a body: it stops in the first body it meets and its weight goes into it, in a combo
    too (the combo's next blow is taken up from there); a blunt blow stopped in a shield arm with the board just
    behind it is the shield's. Edges cleave as before; the fight's own swing still cleaves until it takes these
    rules up. Every contact logs its weight (`HitResult.Weight`: the momentum the struck body took over its mass,
    m/s). Headless (`dummyhits`, `DUMMY_WEAPON=club|sword`; on the open Medium, single blows): a Medium's sword
    0.37 m/s (median), its club 0.65, the Large's club 1.24; the Large's club through a body 0 times (was 10).
    The Large's combos on the open dummy now land 2 of 3: its backhand, stopped, hasn't turned the body as its
    sweep through did, and the forehand after it needs a 67 degree turn (60 at most).
  - The impact's bend (the playtest): every blow on the body bends it for a moment, a pose only, no physics. Each
    part is pushed along the blow, the part struck the most and the rest less the further they are from where it
    landed (nothing 1.2 m away at a Medium's size), so the body bends round the blow; out to its peak in 0.07 s and
    back, mostly gone in 0.35 s (`ClipFighter.Bend`). The hips move by theirs as far as both legs still reach their
    planted feet (the knees take it), then the spine, the chest, the head and the arms each turn so the joint they
    carry goes where its share puts it. How far by the blow's weight (0.35 m per m/s, at most 0.4 m): single blows
    on a Medium bend it 0.13 m from a Medium's sword, 0.25 from its club, 0.4 from the Large's club (`dummyhits`
    prints "bent ... at its peak").
  - Combat Feel (`Locomotion/CombatFeel.cs`): the scene's "Combat Feel" object holds the fight's visual feedback to
    tweak live, in play mode or out of it: the hit bend (its strength per m/s of weight, its most, its reach, how
    soon it peaks) and the older lean and head snap (as multipliers). Added by itself to any scene with the lab in
    it (`Editor/CombatFeelInScene.cs`; also Physics Battler > Add Combat Feel): save the scene once to keep it.
    Headless, its defaults hold. More of the fight's feel joins it as it is made.
  - Weight in a clean hit (the plan "Weight and momentum", step 3): the hit stop, the drive back and the stumble go
    by the blow's weight. The hit stop is 0.03 s plus 0.05 s per m/s (0.04 to 0.16 s); the drive back is stronger
    (3.5, was 2.5) and caps at 2.2 m/s; from 1 m/s of weight the body skips its reaction clip and is thrown back
    stumbling at once (1.3 times the drive, at most 2.8 m/s), its walk taking the catch steps, and can't strike for
    0.7 s while it finds its feet. All of these are on Combat Feel ("Hit stop", "Driven back"). Headless (single
    blows on the open Medium): a Medium's sword stops it 0.05 s and moves it 0.22 m; its club 0.07 s, 0.44 m; the
    Large's club 0.10 s, 0.97 m, every blow a stumble.
  - Weight in a block and a parry (the plan "Weight and momentum", step 4): a held parry first gives along the blow
    by its weight, the attacking weapon on the blade as it goes (0.1 m per m/s, at most 0.2, never more than half the
    way to the body: a Medium's sword 0.05 m, its club 0.06, the Large's club up to 0.12), then sweeps it aside. A
    square parry holds a blow up to 0.8 m/s of weight for a novice, 2 for an expert (a combo's last blow counts
    double); a heavier one beats it: the blade is crushed back into the body at the blow's pace and 0.6 of the blow
    lands on what it is crushed into, and the combo goes on. A glance is never beaten. The shield arm is knocked back
    by the blow's weight (0.12 m per m/s, at most 0.25); from 1 m/s on the shield the guard is knocked open: the arm
    thrown wide and slow to come back, the shield meeting nothing for 0.5 s. Stopped on a shield or a blade, the
    attacker's hand is thrown back by the weight (2.4 m/s per m/s, 0.8 to 3.5) and its upper body jarred. All on
    Combat Feel ("Block and parry", "The attacker's rebound"), every value with a tooltip. Headless: the parry and
    shield rates are as they were except against the Large: the novice's parries of its combos are beaten (3 of 3),
    a mid-skill shield's guard is knocked open by each of its single blows; the parrying hand's fastest fell from
    20 to 15 m/s.
  - The shield's meeting, measured (the plan "Shield and blade contact", step 1): every blow's first contact with a
    shield-bearer logs how near the blade passed the board's centre (in shares of its radius: past 0.8 is the rim,
    over 1 off it) and how far the face was off square to the blade's motion; `dummyhits` and `combos` sum it up
    ("The shield met: ..."), and count the steps a blade rests in a body or a shield outside its blows. Today, at
    skill 1: single blows 23 of 27 on the board, the blade a median 0.99 of the radius from the centre (13 on the
    rim), the face 39 deg off square (most 90: edge on); the four off it are two chops to the head on their way to
    the hips and two thrusts into the shield arm.
  - The shield meets the blade on its path (the plan "Shield and blade contact", step 2): the board goes where the
    blow's blade, read along its path from grip to tip (the parry's read), first comes within 0.42 m of the body's
    line (hips to the top of the head), not out from where the blow is aimed; its face square to the blade's motion
    there, an expert's slanted about the blade's own line so a cut glances off outward. Skill still misreads and
    holds its first guess as before. A path that never comes so near is met as before. Each blow's plan goes into
    the lab's session log ("the shield: blow N ..."). Headless at skill 1: single blows 27 of 27 on the board (was
    23), the blade a median 0.43 of the radius from its centre (was 0.99), 3 on the rim (was 13); the Medium's
    combos 42 of 42, 0.30 from the centre, none on the rim (was 30); in the spar the rim went from 15 of 27 to 2 of
    26. Skill 0.5 is no better yet (step 5 tunes it); high blows are still met only as high as the arm reached
    before (step 3).
  - The shield held steady (the playtest: it moved and turned back and forth as the blade came). Three causes: the
    hand was placed from the physical board's offset as it was, so it went back and forth between two places step to
    step; the face started its turn from straight ahead, not from the guard, a 35 deg snap as it began to move; and
    it chased every read of the wind-up, whose path swings about. Now the hand is placed from the board's fixed place
    in the fist; the face turns from where the guard holds it; the plan is made once, kept in the body's own frame,
    and moved only when a later read moves it more than 10 cm or turns it more than 25 deg, easing over (in 0.06 s,
    quicker as contact nears) and never once the blade is inside the guard; and the shield moves no sooner than 0.6
    s before contact. Headless at skill 1, single blows, from first read to contact: the board moves a median 0.82
    m to get 0.54 m (was 1.23 to 0.53), the most 1.6 m (was 5.0), going back on itself at most 6 times (was 19); the
    face turns 143 deg to turn 106 (was 212 to 101), the most 269 (was 512). Still 26 of 27 on the board, a median
    0.37 of the radius from its centre. `dummyhits` and `combos` print "The shield's steadiness".
  - The shield held steadier still (the playtest: no difference seen). The hand now goes where the shield is going
    as an arm moves, speeding up and settling in (a critically damped spring, settling in 0.08 s), held in the
    body's own frame so it doesn't trail behind as the body steps and turns; the face eases round the same way (0.1
    s). A later read of the blow moves the plan only if it changes by more than 20 cm or 40 deg: the shield commits
    to its first read. All five on Combat Feel ("Shield"). In the spar at skill 1 (`spar` now prints the shield's
    steadiness): the board moves 0.69 m to get 0.67 m (was 1.09 to 0.60 before the first steadying), never going
    back on itself more than once (was 6 times); the face turns 79 deg to turn 59 (was 127 to 57). The cost: the
    blade meets it a little further from its centre, a median 0.57 of the radius (was 0.40), 6 of 25 on the rim
    (was 2 of 26); a smaller settle time or replan distance on Combat Feel trades back.
  - High and low guards, and the shield arm kept behind its board (the plan "Shield and blade contact", step 3; the
    playtest: the raised board cut through its own forearm). Where the blade first comes within the guard out of the
    arm's reach (a chop from high above, a cut at the legs), the board meets it further along its way in, at the
    first place the arm reaches. The elbow bends behind the board, away from its face, and out to the side, never
    into its own chest (it is solved again turned further out if it comes within 0.16 m of the body's line); and the
    face turns no further than keeps the forearm and the upper arm going into the board's back, eased into like any
    other turn. Headless: the shield arm through its own board 0 steps everywhere (was 270 in the expert's single
    blows, 241 in its combos, 67 in the spar); the elbow at least 0.16 m from the body's line (was 0.07). At skill 1
    the high blows (the blade over 0.25 m above the chest) were already all met on the board, 13 of 13; single blows
    26 of 27, the Medium's combos 40 of 42. `dummyhits`, `combos` and `spar` print the arm through the board.
  - Combo blows on the shield, measured (the plan "Shield and blade contact", step 4, begun): each blow met now logs
    how far the blade came from the shield's first plan for it and from the plan it held ("planned ..."). At skill
    1 the blade came a median 0.14 m from the plan held against the Medium's combos, 0.2 to 0.3 m against the
    Large's (the board's radius is 0.27): the shield's read of the blade's path is what is off, the first read of a
    combo's blow most (up to 0.78 m). Tried and left off: planning on a finer path about contact (`SHIELD_FINE`; met
    the blade further from the board's centre), one late correction, a smaller slant. Nothing changes in play yet.
  - Skill on the shield (the plan "Shield and blade contact", step 5): a blow read within 0.6 s of the last one
    passing is a combo's next, harder to read than one on its own; each has its own curve of the skill on Combat Feel
    (skill to the power of "shield single curve", 1, and "shield combo curve", 4). A blow is planned for only once it
    is seen: a shield still up from the last block stays where that was, and a poor one (more than half way to a
    novice) lets it fall back to its guard, never tracking the next blow unseen. And the board is round: a blade past
    its rim, in the corners of its square collider, goes on by (it blocked blades well off the board). Headless,
    single blows missed (blade not on the board): skill 0.5 26% (Medium attacker; the Large 33%), skill 0 70% (67%),
    skill 1 4% (0%); combo blows: skill 0.5 40% (the Large 50%), skill 0 55%, skill 1 7% (0%). The aim was a
    quarter of single blows and half of a combo's at skill 0.5.
  - The Large thrown off the field (the playtest of 6 November): starting a combo as the dummy started its own, its
    pose jumped 1.3 m in one step (a combo's step in, taken in 0.08 s), and its physical body, the kinematic legs
    swept after the pose into the dummy's, was knocked down and flung 32 m. A combo's step in now carries the body no
    faster than 9 m/s (at a Medium's size), the rest taken in the steps after; and a carry moves every physical part,
    the kinematic legs too, instead of sweeping them there. In the spar the biggest root jump in a step went from
    0.62 m to 0.30 m (`spar` prints it). Combos land as before but for the Small's against the sword, now parried a
    blow later.
  - Weapons rest against bodies and shields outside their blows (the plan "Shield and blade contact", step 6). Out
    of a blow (winding up, recovering, on guard) a blade sitting in the other fighter's body or shield pushes its
    hand out, along the shortest way out of what it is in, taken in quickly (0.01 s) and let go slowly (0.08 s), the
    arm solved again to reach it; in a blow nothing changes, so slashes still carry through. Headless, steps the
    blade was over 2 cm into a body or shield outside its blows: against the Medium 165 to 89, the Small 105 to 41,
    the Large 25 to 11, the Medium's combos 78 to 57; the longest such run 12 steps to 5 (the Small), 6 to 2 (the
    Large). What lands, is blocked or parried, the hit stops and drive-backs are as before; the spar's weapon flicks
    the same (3). `NO_REST=1` turns it off headless; `dummyhits` and `combos` print the blade outside its blows.
  - A new blow taken for an old one (the playtest of 6 November): each attacker counted its own blows, so a dummy
    set up again (or the spar's other fighter) began again at blow 1, and a shield that had met a blow 1 kept its plan
    for that one, 5 s old: the dummy's thrust went in at the walker's head, off the board. Blows are now counted
    across every attacker. The lab's shield lines say whose shield ("the walker", "the dummy"). In the spar A throws
    9 blows (was 8); `dummyhits` and `combos` as before.
  - A combo's next blow read where its blade will really be (the plan "Shield and blade contact", step 4). Read
    coming out of the last blow, the next one's blade was taken from the clip's hand bones alone, which hold it nothing
    like our grip does and leave out the wrist's steer at the target: a chop's blade came out 0.6 m low, a thrust's
    point over the head. The read is now fitted to where the strike's own plan will have the blade at contact (the
    grip as measured for each clip on each body, `StrikeReach`, the arm's pull, the wrist's steer). Headless at skill
    1, the blade at contact from the first read: the Medium's combos median 0.81 to 0.29 m, the worst 1.49 to 0.45 m;
    the Small's the worst 0.77 to 0.38 m (median 0.23 to 0.35). The Medium's combos all on the board, 42 of 42 (was 39),
    if further from its centre (median 0.64 of the radius, was 0.47). The Large's forehand stays 0.9 m off: it falls
    short of where it is aimed. At skill 0.5 nothing changes (a middling shield waits to see a blow, it doesn't read
    it ahead): combos still 40% off the board. `PRED_TRACE=1 combos` prints each read against the blow;
    `NO_FIT_AHEAD=1` reads as before.
  - Giving ground to a heavy blow (the plan "Weight and momentum", step 5). A blow's read carries the momentum a block
    of it would take (a combo's blow 1.5 times, its showstopper twice); over the defender's mass, the weight it reads
    coming. A shield steps back from one at least 0.9 m/s heavy, about 0.22 s before contact, 0.25 to 0.5 m by how heavy
    (an expert always, a novice now and then, later and shorter), its feet stepping after it, and meets the blow
    further out, lighter by up to 30% (its shield arm knocked back less, the guard knocked open less often). Not in a
    clip of its own (a reaction, a strike), where its feet can't step. A parry steps back only from a blow heavier than
    its skill holds (it would beat it), and still when its arm would be late: stepped back from lighter ones, or from
    blows coming in deep, the body left where its arm was putting the sword (the expert let 2 more of the Large's
    single blows through). Stepping back from deep blows stays on Combat Feel, off. All six on Combat Feel ("Giving
    ground"). Headless at skill 1: the shield steps back from every Large blow (weight 1.85 m/s in combos; single
    blows met at 0.44 to 0.49 m/s, were 0.62) and the Medium's combo blows (1.07 m/s), never the Medium's single blows
    (0.8) or the Small's (0.13). The Large's combos blocked 9 of 9 (were 6, 3 missed); everything else, the sword's
    parries and the spar as before. The dummy's planted feet within 0.2 cm in every combo (`combos` prints them, and
    the weights read). `GROUND_TRACE=1` prints each step back; the lab's log has them; `NO_GROUND_STEP=1` steps back
    only for a late arm, as before.
  - Giving ground, smaller and followed (the playtest of 6 November: far too far, the blow no longer landing). The step
    back to a heavy blow is 0.1 to 0.2 m (was 0.25 to 0.5), and the attacker follows it: the blow still meets the
    shield, further out (only a late arm's step is a void the blow is still aimed past). The Large's blows met at 0.14
    m back, all on the shield (9 of 9 single, 9 of 9 in combos), at 0.49 m/s (were 0.62).
  - A jump struck in the air comes down (the playtest: drifting to the floor). A fighter struck mid-jump (the dummy's
    jumping chop met by the walker's blow) had its strike cut short there, backing out and fading to its walk in the
    air: its pelvis sank from 1.55 m over half a second. It now comes down through its own clip's fall and landing,
    as a jump stopped by what it struck does (the weapon arm held off the follow-through), and the blow drives it
    back only once a foot is down. Headless (`JUMP_HIT=0.7 JUMP_TRACE=1 dummyhits <dir> Medium sns_chop_lunge` strikes
    the attacker 0.7 s into its jump): it falls from 1.71 m to its touchdown at 1.2 s, then steps back.
  - No step back over planted feet (the playtest of 6 November: the walker's planted foot dragged 17, 30 and 34 cm).
    Still in a reaction clip from one blow, a late arm's last-resort step back to the next moved the body while the
    clip kept the feet planted. It now gives the reaction up for the walk's steps; in its own strike it doesn't step
    back. A scene's Combat Feel keeps the giving-ground values it first saved (0.25 and 0.5 m): the new defaults, 0.1
    and 0.2, show only once set there. Left: a Small's combos at parry skill 0.5 still drag a planted foot 14 cm as
    the dummy is driven back (as before).
  - The fight on clip fighters, moving only (the plan "Bringing the lab into the fight", step 4; now on `main`). With
    the sandbox's "Clip motion" on (headless `CLIP_FIGHT=1`), each fight unit is the lab's clip fighter: the lab's
    walk, guard and body, its weapons held as the lab holds them (`Units/UnitClips.cs`). It keeps what the fight reads
    of it (health and poise, team, perception, tempo, rhythm); the old pose pipeline, gait, balance and stance are not
    built. The fight loop steers it as a walk and a facing instead of moving its root, its root following the walk;
    once near its target it circles it in spells, as the lab's footwork does, giving way when crowded. Clip fighters
    keep 0.2 m more room from other bodies (their walk answers a push late, the feet taking the steps). Knocked off
    its feet, the fight sees it down until its get-up. No blows yet (step 5). Headless 3 v 3, 60 s: every unit
    reaches its target in 5.6 to 7.2 s and holds near its striking distance; the root never jumps (0.01 m a step at
    most); planted feet within 0.2 cm but one plant of 2.2 cm; bodies overlapping 12 steps of 3600, never nearer than
    0.49 m (none with the teams swapped). The old path (the switch off) as it was. `fight` prints each clip fighter's
    approach, feet and root (`FIGHT_SECONDS`, `FIGHT_OVERLAP_TRACE`).
  - The fight on clip fighters, striking (the plan "Bringing the lab into the fight", step 5). The fight loop still
    chooses when to strike and at whom (its tempo, its delays, the openings it reads, when to throw a combo); a clip
    fighter strikes with the lab's: a strike of its own that lands from where it stands, or the lab's combo it opens.
    Its blade sweeps every other body on the board; each body it meets takes the fight's damage rules (health, poise;
    the combo's knock in them), its reaction the lab's (bend, hit stop, drive-back, stumble), and dies when its health
    is gone: it falls as a body knocked down does, and never gets up. The fight hears every hit (`UnitController.Hit`,
    `Died`, and `ClipStrikeStarted` for each blow). A blow stops in an ally instead of sweeping on through, and a fighter
    doesn't swing with an ally in the way (the lab's combos hit one in four on allies). Bodies are kept from passing
    through each other as the feet allow: pushed off with catch steps, a lunge into a body cut short, only one deep in
    another moved straight out. No defence yet but the held guard (step 6), no severing yet. Headless 3 v 3, three
    fights: each ends with a winner in 15 to 42 s; no unit floored but by its death; bodies never overlapping;
    friendly fire 3 to 10 of 25 to 40 hits (the old fight: 2 or 3 of 65). Left: planted feet still drag in the press,
    up to 43 cm (in reaction and strike clips; the lab's spar had 21 cm). `CLIP_NO_BLOWS=1` moves only; `fight`
    prints each clip fighter's blows and hits (`FIGHT_HITS` each hit, `FIGHT_FEET` each foot dragged).
  - Clip fighters pair off and spread out (the playtest of 6 November: they clumped up, four on one spot, the
    Small stuck in its own Large's back). Choosing a target, one already taken by a teammate counts 2.5 tiles
    further, so they spread over the enemies; two on one target go round to sides of it at least 100 degrees apart
    (flanking) instead of queuing; and a clip fighter keeps 0.5 m more room from any teammate and 0.7 m from an
    enemy fighting someone else (the next pair over). Headless, the share of time three or more stood in one knot
    went from 29, 12 and 6% to 12, 17 and 8% (four or more: 13, 1, 0% to 7, 5, 6%); the playtest's sessions had
    40, 8 and 52%. `python3 Tools/Playtest/clump.py PlaytestLogs/<session>` measures a session's knots.
  - Roles on the way in, and the smalls out (Renier, 6 November). The default lineup's small units leave the battle:
    setup 24 turns each small spawn into a medium with a sword only ("A Medium (sword)"); each side has a large with a
    club, a medium with sword and shield, a medium with a sword (headless: "Swordsman"; `FIGHT_SMALLS=1` has the smalls
    back). Clip fighters come in by role: a large charges the nearest enemy, mostly walking, a run now and then; a
    medium flanks, heading first for a place 2.8 m out to the side of its target and a little past it, on its own side
    of the fight (a team's two flankers go round opposite sides), running or walking in spells of 1.5 to 3.5 s (half
    of them running). Close to any enemy (2.2 m; a flanker on its way round, 1.5 m but for its own target) it commits
    to fighting it, and stays on it until it is 3.2 m off. Headless, four fights: three or more in one knot at most
    10% of the time (was up to 17%), the mediums running 5 to 54% of their way in, allies hit 0 to 4 times a fight.
    `fight` prints each unit's running and the side it came at its first target from.
  - Fighting off several attackers, steps 2 and 3 (the plan "Fighting off several attackers"; the fight plan's step 6
    folded in). A clip fighter knows every blow coming at it: each enemy's strike aimed at it is a threat it keeps,
    with its read path and time to contact, seen once it is in its view (or felt beside it), as the old fight's blows
    were; struck, it knows who struck it. Of the blows it has seen and its guard can reach (100 degrees each way of
    where it faces), it meets the one landing first with its shield or sword (`ClipFighter.Watch` given that read),
    given up only for one landing 0.12 s sooner; its parry keeps off that attacker's body, shield and blade. A shield
    with no model of its own (headless) gets a board its size. Headless (`FIGHT_LINEUP=gang2`, `gang3`: a medium
    with sword and shield in the middle on 2 or 3 swordsmen): on 2 it lasts 36 to 41 s (was 4 to 8), blocking or
    parrying 70 to 90% of the blows that reach it; on 3, 7 to 16 s, the blows from beside and behind it never sees
    landing (steps 4 and 5). `fight` prints every blow met by where it came from and whether it was seen.
  - Fighting off several attackers, step 4: turning to the threat. A clip fighter faces the attacker of the seen
    blow landing first; between blows, with two or more enemies on it within 3.2 m, the middle of the two nearest
    when they stand within 90 degrees of each other as it sees them, else the nearer; only then where the fight loop
    points it. Struck by another than the one it fights, it turns its fight to them for 1.5 s (committed). Headless,
    on 3 (`FIGHT_LINEUP=gang3`) it lasts 16 to 32 s (was 7 to 16), most blows from beside it now blocked; it faces an
    attacker starting a blow from beside or behind in a median 0.3 to 0.8 s (90%: 1.1 to 2.3 s). Planted feet as
    before (the drive-back's slides under staggering blows, 14 to 22 cm in two fights, were there before).
  - Fighting off several attackers, step 5: keeping them in front. Set on by two or more spread round it more than 90
    degrees, a clip fighter looks a step (1 m) out each of 16 ways and walks the one from which they stand in the
    narrowest arc (the outside of the group, the nearest between it and the others), on the board and not into any of
    them; spread more than 150 degrees, it breaks out at a run. Headless over seven fights (1 on 2, 1 on 3, 3 v 3), the
    time it stood with its attackers more than 120 degrees apart round it went from 24 to 11% of the time it was
    outnumbered; on 3 it lasted 20 to 60 s (was 16 to 32). `fight` prints it ("outnumbered"). Left: planted feet still
    drag up to 22 cm in the press (reaction and strike clips, the drive-back), as before this work.
  - Fighting off several attackers, step 6: reactions by side. A blow that throws the body sideways or forward (a
    second or third attacker's, from its side or behind) plays Renier's new hit clips (`Tools/Mocap/Combat/samples`,
    baked by `combat_bake.py`, each `hit_left_` one mirrored into a `hit_right_` one): thrown to its right, a blow from
    its left (`hit_left_head`, `_head_big`, `_head_big_b` for a heavy one to the head, `_rib` to the body); thrown
    forward, `hit_back_head`. From in front it keeps the pack's own reactions. The bend (the struck part 100, its
    neighbours less, along the blow) stays, and off the front it bends and throws the upper body harder (Combat Feel's
    Off Front Bend, 1.35). The clips are long (1.3 to 1.6 s), so after the blow lands (0.4 s) their recovery is
    hurried to end within Combat Feel's Side Hit Time (1 s): in a full one the unit couldn't turn or step to the next
    attacker. `hitlook` strikes a standing Medium eight ways and prints what it played; `NO_SIDE_HITS=1` plays the
    front reactions whatever the side, as before. Headless over four fights (1 on 2, 1 on 3) the outnumbered unit
    lasted about as long as before (48, 26, 60, 17 s; was 48, 24, 44, 27).
  - Fighting off several attackers, step 7: concentration. With two or more enemies on it a unit's concentration builds
    (0 to 1; Combat Feel's Concentration Rise, 0.3 a second for each attacker past the first) and with one or none it
    fades (Concentration Fall, 0.15). It adds to its own shield and parry skill (Concentration Guard, 0.5 at full) and
    as much again to the weight its parry holds and its guard takes before it is knocked open (its shield knocked back
    less); and it takes off the block and parry skill of whoever it strikes (Concentration Accuracy, 0.3 at full: an
    expert defends as 0.7). The units in the fight are experts (skill 1), so the guard's gain shows in the weight it
    holds and against a concentrated attacker. Headless over four fights (1 on 2, 1 on 3), the outnumbered unit landed
    12 of 27 blows (was 8 of 29) and lasted 60, 36, 49, 17 s (was 48, 26, 60, 17). `fight` prints each unit's highest;
    the playtest logs record it each frame (`concentration`). `NO_CONCENTRATION=1` keeps it at nothing.
  - Fighting off several attackers, step 8: combos across several enemies. Once a combo's blow has passed its contact
    (or been met short of it), the next may turn on another enemy fighting the unit, standing within Combat Feel's
    Combo Spread Reach (3.2 m) and Turn (100 degrees) of where it faces, with no friend in the swing's way: one the
    combo hasn't struck at yet first, then the one it turns least to (two on it: the first, the second, the first
    again). The blow playing stays on whom it was at; the new one becomes its target, so its threat board sees the
    next blow coming. With two or more such enemies, a blow now and then opens a combo whatever its tempo says (Combo
    Sweep Chance, 0.25). `fight` prints how many blows each unit turned; `COMBO_TRACE=1` prints each choice and who
    stood where; `CLIP_COMBO_ONE=1` keeps every blow on one, as before. Headless the turn comes up less than hoped: the
    second and third attacker mostly stand off (3 to 7 m, often behind) while one is in, so over five 1-on-3 fights
    the outnumbered unit turned 0 to 6 blows a fight; in the 3 v 3 fights, 0 to 3 a unit.
  - From the playtest of 6 November (13:14): a combo whose target fell went on striking the body (four blows over a
    second, logged as hits on a dead unit); its blow that killed is now its last unless it can turn on another, and a
    blade through a body already dead is no hit. The playtest log now says what a clip fighter did: each strike and
    combo it started (`attack`, with its concentration), each combo blow turned on another enemy, and in each `struck`
    line the reaction it played (`hit_left_rib reaction`, `parried with ...`) and its concentration.
  - Bringing the lab into the fight, step 7: combos and tempo. A clip fighter's blows now reach its tempo and its
    rhythm as the old fight's do: each it starts, a combo's next ones too, costs its energy and counts in its press's
    burst, and each end is noted. Until now they never heard of them: its energy never fell, its press never ended on a
    burst, its counter never knew it had answered (it waited 1.2 s and gave up). Its combos are as long as the tempo
    makes them (a counter's two blows, an opener and a showstopper; a press's, a punish's and a full gather's three),
    and the log says why (`a combo (full gather): ...`). Headless over three 3 v 3 fights, 20 to 24% of blows came in
    combos (was none counted), for full gathers, counters, punishes and presses; the tempo spends more time probing
    and less pressing than before (it tires now), with about the same damage dealt. `fight` prints the combos' reasons.
    Clip motion was already the fight's default in Unity (the sandbox's Clip Motion, on); headless it stays behind
    `CLIP_FIGHT=1`, the old pose path's tests running without it.
  - Severing for clip fighters (milestone 5's rules, which their blows had skipped). A clip fighter's edge crossing a
    joint, fast and hard enough, cuts the limb off there, judged on the pose where the blow met it, the blade the
    strike swept that step. The limb flies off with what it held; the wound costs health and bleeds. The head is its
    life; the weapon arm takes its weapon, strikes and parry (a shield still guards); the shield arm its shield (a
    one-handed sword left parries now); a leg its footing: it falls and stays down, bleeding out (`StaysDown`, no
    get-up). Headless over four 3 v 3 fights, five limbs came off in two (knees, elbows, a hip), none in the other
    two; the log says why a cut didn't go through (`no sever: ...`). `CLIP_NO_SEVER=1` keeps every limb on.
  - The playtest logs can be committed while Unity is open (Windows: "Permission denied" on `perf.csv.gz` until Unity
    was closed). The recorder's tables (`perf.csv.gz`, each unit's frames and poses) were opened sharing nothing; they
    are now readable while written (a session still recording commits as far as it has got). And stopping a session
    closes every file even if its summary fails. A clip fighter's track records its walk (mode, speed, the speed
    asked, the feet held) instead of the old pose's.
  - Sound (Hexamus's tool as it is; `Docs/Sound.md`): Physics Battler > Audio > Sound Effects makes the sounds with
    ElevenLabs (the key is the one entered for Hexamus on this computer), takes heard and re-rolled in the window.
    Every place two things meet plays its sound (`Audio/SoundHooks.cs`): steps and landings, swings, blades and
    clubs on bodies, on blades and on shields, glances, taps, shields meeting, bodies colliding, severing, bodies
    and dropped weapons on the ground, the showstopper's boom.
  - A small ball marks where the last strike was aimed: green it landed, blue it was blocked or parried, red it
    missed. The log says what each blow did and how the dummy took it. Headless: `dummyhits <dir> <body> [clips]`
    strikes the dummy in each setup at its hips, chest and head (`DUMMY_STANCE=open|shield|sword` one setup;
    `SHIELD_TRACE=1` the blade and the shield each step, `PARRY_TRACE=1` the parry's arm each step), and writes
    both bodies' films (`Tools/Mocap/Locomotion/twofilm.py` draws them).
  - The panel: 1.5x by default (its − and + buttons, kept between runs), in five sections that open and close:
    body, walk, attacker, defender, log.
  - Sparring (X, on by default when driving): the walker keeps its guard up, faces the dummy and walks itself to
    where the picked strike reaches it (the panel says when it is in position), so J or I strike from a meaningful
    spot; W A S D still move it. The dummy steps back onto its spot a second after it was last struck.
  - Combos (step 5): I plays the picked strike's combo at the dummy (the panel shows it): up to three strikes,
    each the best follow-on of the last (`StrikeCombos`: for every pair, where the first is left and the second
    taken up, by how well the hands, the weapon's line, the hips and the feet meet there, the hips lined up), each
    aimed at the dummy's part nearest the height it reaches. As each strike goes on, its follow-on is chosen
    there and then, as the old move planner chose a move only if it could land: of the follow-ons that land from
    where the link leaves the body (within 5 cm, with the turn up to 60°, the lunge 0.4 to 1.4 times and the arm's
    pull), the cheapest, a turn of the body onto the target counting against it (`ComboPlayer.TurnWeight`). The
    root turns and moves at each link so the next clip's hips land on the last one's. Each strike's hand follows
    its target as it moves (`LocoBody.Retarget`, within the pull), and a follow-on is aimed where a driven-back
    dummy will be. A strike stopped by what it met (most of a Medium's, against a Medium) is over where it stopped,
    not at its link mark: its follow-on is taken up from there if their poses meet, and otherwise from the guard
    (linking as if it had played out turned the next strike 60 to 90 degrees off the dummy). None lands from where
    the body is (the dummy driven back), the combo presses: it steps in after the dummy in its guard, turning to it
    if it is well off to the side, stands into its guard, and goes on (for at most 3 s). Headless: `combos <dir>
    <body>` (the cost table, each combo played at the dummy, now driven back, films of both bodies;
    `COMBO_WHY=1` the follow-ons weighed; `COMBO_CURVE=a>b` the cost through the first's recovery): the Medium's
    and the Small's land 27 of 27 strikes, the Large's 7 of 8.
  - R restarts, P pauses (the physical body is still held to the pose while paused).
- **Logs:** each run writes `PlaytestLogs/locolab-(time)/`, with the measures in session.md and the film in
  locolab.json. To draw the film: `python3 Tools/Mocap/Locomotion/locofilm.py <that>/locolab.json film.gif`.
- **In the fight (first join):** untick *Locomotion lab* and the fight's units walk with the new locomotion
  (`Units/UnitWalk.cs`): while a unit is free to walk, the walk animates the root the fight moves (its feet locked
  where they land, the pelvis, spine, chest and head as the reference walk has them); the pose pipeline blends its
  own lower body and torso into the walk's, and the Fight Rhythm, guard, look and arms go on top. A blow, a
  knock-back or a stumble hands the feet back to the stance where the walk left them. `UnitController.NewWalk`
  switches it off (`OLD_WALK=1` headless).
- **Headless:** `dotnet Tools/CompileCheck/bin/Tests/Debug/PhysicsBattler.Tests.dll locolab <dir> 70` runs the
  same course and prints the measures. `LOCO_MODES=1` traces the clip switches;
  `LOCO_SHOVE="time,yaw,m/s;..."` adds shoves; `LOCO_BODY=Small|Large` walks another body;
  `LOCO_COURSE=strafing` walks the facing-held test course; `LOCO_ARMS=sword` carries a weapon and a shield.
  `locofilm.py --view side|front|left|follow` picks the camera.

## Current state: milestone 11, the defensive posture

The pressure meter, the shell and the counter came in milestone 8; milestone 11 makes the shell defend.

- **Weapon parries** (`Units/UnitParry.cs`): a unit without a shield meets a blow it has seen with its
  weapon. After its reaction time the club or stick is set across where the incoming blade will be as
  its striking part comes within reach (a swing's blade back from its pivot, a thrust's along its
  line), square to that blade and in front of the chest; the weapon arm is posed exactly meanwhile
  (`PosePipeline.Parry`). A blow whose blade meets it is stopped there, a new weapon-on-weapon
  contact in the sweep: no damage, 0.85 of a block's poise coming through. `CombatStyle.parryChance`
  is 0.4 (a shield's block chance is 0.55).
- **Defence skill on each unit** (`UnitProfile.defenceSkill`, 0.5 is each style's own figures): it
  scales the chance to block or parry (×0.6 to ×1.4) and the reaction time (×1.3 to ×0.75).
- **Shell guards** (`Temperament.shellGuard`): the boxer's high shell, fists by the temples, the club
  upright, the arms folded tight (a blow there lands on a guarding arm, as a forearm's damage and a
  block's poise, 0.3 of its momentum); the shield raised to the eyes; the eskrimador's stick across its
  body. In the shell, a block or parry is 0.25 likelier, and a combo's later links are met 30% sooner.
- **Pressure**: an enemy pressing it, close in, builds it (0.35 a second, up to just past its
  composure); blows take it further, to 3 at most.
- **Guard breaks** (`Units/UnitGuard.cs`): a blow from a barrage (a combo link, or one pressing it)
  taken on its guard while shelled, with the pressure at 1.6 or more and its poise under a quarter,
  breaks it: it staggers, its weapon and shield or hands flung open, and can't defend for 0.6 s. At
  most one every 40 s.
- **Soaking costs**: each block or parry costs 0.04 energy. Shelled up, it gives 0.2–0.4 of a leg of
  ground under each blow (the Small half as much again), and the presser follows.
- **Ripostes**: a composed unit may go straight to its counter off a blow it took on its guard,
  between a barrage's links.
- **Measured**: session.md has a Defence section (time in the shell while pressed, combo links
  defended, blows met on the guard, guard breaks a minute, bursts answered with a counter); the frames
  have `shellGuard`, `parry` and `guardOpen`; guard breaks are "defence" events. The `parries` runner
  plays every blow at every defender (`PARRY_SKILL=presets`: shelled up, three skill levels); `shells`
  draws the guards (`shellpose.py`). The headless fights now give their code-made weapons colliders,
  so blocks and parries happen there too.
- In four headless fights: the Medium spends 75–92% of a press at it in the shell and defends 17–100%
  of the links (most fights 30–60%); the Large 13–80% and 0–44%; the Small, pressed little, 9–66%
  and 0–17% (its defence is the dodge, milestone 12). Guard breaks: 0–0.5 a minute for the Large, up
  to 2 for the Medium. 10–65% of ended bursts get a counter. Ripostes are rare (about one a fight).

## Milestone 10, the arm rebuild (every arm on one rig, one limit model, joint motors)

Plan: https://claude.ai/artifact/RGb9d1i935pRxtbLFXvS8o

### The arm rebuild

Every arm is posed by one rig (`Motion/ArmRig.cs`), an animator's controls solved exactly with no
memory: a point of the hand goes somewhere (`ArmGoal.point`, `pointInHand`), the hand is turned a way
(`handTurn`, or a keyed wrist: `wristGiven`), and the elbow sits on its circle round the shoulder-to-
hand line at a null (`elbowSwivel`, an angle from a reference carried smoothly with the arm). The
elbow is a hinge; the wrist twists about the forearm and bends inside an ellipse; what the wrist can't
take moves the elbow round its circle (as far as `turnWeight` asks; `nearNull` keeps it to the nearest
place), then rolls the hand about an allowed line (`rollAxisInHand`). `ArmRig.SwivelToward` turns an
elbow pole into a null. The same goal gives the same arm.

- **The weapon arm** (`PosePipeline.PoseWeaponArm`): blows are hand-first: the hand moves through its
  keys (wind-up, the contact place solved when the blow was planned, follow-through) on arcs round the
  shoulder, with the wrist and elbow keyed at the same moments (`MoveDefinition.DefaultHands`, `Wrists`,
  `Elbows`; fitted by the `keyfit` runner), and the weapon follows the hand.
- **The shield arm, the bare off hand and get-ups' off arm** (`PlaceShieldArm`, the bare guard,
  `GetUp.KeyArm`, `GetUp.PlaceShieldArm`) on the same rig. The weapon arm's get-up keys still use the
  two-bone solve: its weapon is laid out by the hand, past the wrist's range.
- **One limit model** (`Muscles/JointMath.cs`: `JointFrames`, `JointLimitTable.Physical`): the elbow
  and wrist joints get the rig's own frames at spawn (the elbow a hinge about the rig's elbow axis, the
  wrist twisting about the forearm), so every arm the rig poses is one PhysX holds. On the ground the
  elbow is a hinge (the forearm rolls at the wrist). `ArmRig.PastLimits` measures any arm, posed or
  physical, the log's `jointPast` included.
- **Joint motors** (`AuthorityLevel.Muscled`): out of blows, the weapon arm is held by each joint's
  slerp drive aimed at the pose's turn in the joint's frame, its limits on, its weight carried, each
  motor strengthened for what hangs from it, the pose's turning fed forward; no pull in the world.
  Blows stay kinematic. The sandbox's "Weapon arm on joint motors" switch puts it back on the world
  servo; the log has each arm joint's error in joint space (`jerr_*`). The off arm stays on the servo.
- **Bodies on the ground** never fly off by themselves (`UnitRagdoll.HoldLyingSpeed`): no part rises
  faster than it was sent up, and a second after it falls or is struck no part speeds up; the joints
  aren't mass-weighted on the ground; cut-off limbs count as bodies on the ground.

### Milestone 10's close blows: the jab, the shield bash, the butt strike

- **The jab** (`MoveStyle.OffJab`, the large boxer's): the bare lead fist snaps straight from its
  guard to the aim point, a little through, and straight back, palm turning down, the lead shoulder
  turned in behind it and a half step in. The weapon arm keeps its guard (and its rhythm) meanwhile.
- **Off-hand blows** (`MoveDefinition.hand = Off`): thrown with what the off hand has
  (`Loadout.OffHand`: its shield, or `WeaponDefinition.BareFist`, a fist made in code with the bare
  hand's blows). `Combat/OffHandPath` locks the aim and works out the step in; `PosePipeline.PoseJab`
  poses the off arm on the rig; the off arm is posed exactly through the blow (as the weapon arm is in
  its own); the sweep follows the fist from the wrist to the knuckles (`UnitCombat.StrikingPart`); a
  stopped jab springs the off arm back. The planner offers it beside the weapon's blows: it reaches
  only from close in.
- **The shield bash** (`MoveStyle.ShieldBash`, the Medium shield-bearer's): the shield's face driven
  straight from its guard into the target and back, as the jab, the face turned to meet the way it
  goes. Any shield without blows of its own gets it (`WeaponDefinition.MovesOf`). It shoves more than
  it hurts (`DamageRules.ShieldBashDamage`); the sweep covers most of the face.
- **The butt strike** (`MoveStyle.ClubPunyo`, the Small's with a club): the butt end below the fist
  goes up to a chamber above the aim, then hammers straight down onto the face, the neck or the
  collarbone and a little through, the stick standing up behind the fist (`PosePipeline.PoseButtStrike`).
  A close blow on the weapon arm (`MoveDefinition.Close`; `OffHandPath` prepares either arm). Made in
  code for every club (`WeaponDefinition.ExtraMoves`), so no asset changes. It never cuts. It reaches
  only a target about as tall as the Small (a Medium's chest is above its shoulder).
- The fight's spacing doesn't yet seek the close range these need, so the bash and the butt strike
  come up only when a fight happens to bring two units together (in three headless fights the jab
  came up nine times, the bash and the butt strike not at all).
- Still to come: the weapon arm's get-up keys on the rig (they need keying by eye).

### Performance: planning spread over steps

- `Diagnostics/Perf` and `PerfTrack` time the game's own work per frame (perf.csv.gz, perf_counters.txt,
  hitches over 40 ms logged as "perf" events, a Performance section in session.md).
- A new blow is planned a few poses at a time (`MovePlanner.Job`, about 4 ms of each physics step),
  not all in one frame; the playtest recorder writes one file a step. Combo links are still planned
  in one go (the one hitch source left that I know of). The planner's dry poses skip the off arm, its
  clearance check stops at the first pose that fails, and the paths are kept relative to the root.

## Before the arm rebuild: milestone 10, combos (chained links; the club's backhand and fan strikes)

Plan for milestones 7 to 13 (free roam, tempo, combos, defence, dodging, signature attacks):
https://claude.ai/artifact/4BijSWun94DiWFcjcYCRJp. Milestone 9 (the shield, hands and head, before
combos; the combat plan's milestones move back by one): https://claude.ai/artifact/HCsdNXmL5zVwV1ep1ta5Gj

### Milestone 10, second part: the club's backhand and fan strikes; fixes from the combo playtest

**New moves** (`MoveDefinition`, Setup 20 makes the assets and puts them on the club): the **club
backhand** (`ClubBackhand`, the hook's return: wound across to the left, swung flat out through the
target), and the **fan strike** (abanico, `ClubFan` and `ClubFanBackhand`): a short arc led by the
wrist from close by the shoulder, little wind-up and little body, at the head, neck and hands, about
0.45 s from one to the next in a string.

**New strings:** the Large's "hook, backhand, smash" (finisher, Melee Combo Ver. 1) and "2-3-2"
(keeps hammering); the Small's "banda y banda" (fan forehand and backhand, keeps hammering) and
"abanico flurry" (four fans side to side and the smash; finisher). Still to come in milestone 10: the
off-hand jab, the butt strike (punyo) and the shield bash, which need a sweep on the off hand.

**Bodies launched by small movements** (the 06:22 playtest: a body thrown off the board at 100 m/s). A
Large's smash in a combo drove the club into the floor; the ground clamp lifted it out at once and the
posed hand jumped 1.3 m in one step; kinematic, the hand slammed the body lying beside it. Now:
- A kinematic part moves at most 15 m/s and turns at most 1800 degrees a second toward its pose
  (`AuthorityController.KinematicMostSpeed`; a pose 2.5 m away is a placing, done at once), and a held
  part's velocity is capped the same. Test: `ArmTests.AKinematicPartNeverSlamsAtThePosesJump`.
- The ground clamp's lift changes by at most 10 degrees a step (`GroundClamp.Memory`): a buried blow
  is lifted out over a few steps. The `arms` runner prints the posed hand's travel in a step: the
  139 cm jump is gone (at most 61-67 cm, a strike's own speed, which the cap above keeps off the body).

**Twisted arms after blows** (the Large's club arm in the playtest): the physical weapon arm trailed its
fast return to guard by 30 to 80 degrees, its wrist past its limits. It stays posed exactly for 0.4 s
after a blow (`UnitCombat.WeaponArmHeldAfter`).

**The shield getting up** is held as a guard holds it: the elbow down and out by the ribs, the forearm
pointing at the threat, the wrist straight; not pointed out at arm's length. The arm fit keeps the elbow
to its way (`ArmFit`'s elbow term: it had gone up over the shield).

**The arm "gimping out" in the guard** (the 07:02 and 07:22 playtests): every playtest since the night
held arms were given their anatomical joint limits (milestone 9c) had the guard arm 20 to 30 degrees off
its pose for seconds at a time (the Large's shoulder 25 median; 2 the evening before, for every body).
The limits trapped it. A held part is turned straight toward its pose, and that way can run a joint into
a limit even when the pose is well inside it: the wrist's side-to-side range is 55 degrees, beside a
roll of up to 127, and the stuck wrists sat on its ends (15 to 48% of the stuck steps; never while the
arm tracked). A first fix (`JointMath.ConeAround`: PhysX holds a joint's two swing limits together, as
an ellipse, not one at a time as the table has them) was right but not enough. Held arms' joints swing
free again, as they did before; the twists after blows the limits were for are kept off by the 0.4 s
the weapon arm stays posed after each blow. Any joint held with its limits (`HeldLimits`) gets the
ellipse that takes in its whole table. `guard` runner: the arm held in the guard against its pose with
free bodies (within a degree: the pose and the servo are sound); arms.py: the same in a playtest
(target at most 5 degrees median at the shoulder).

**Moves say which bodies throw them** (`MoveDefinition.bodies`, Setup 21 sets it on the move assets):
the fan strikes are the small body's; the Large threw them as single blows in the playtest.

**Measures** (stub): 147 tests pass. `aim`: no misses (32 blows turned down for their own body). Four
fights: 32-44% of blows in combos, from one striking moment to the next 0.57-0.82 s median, no standing
bodies overlapping. `combos`: the fan strings throw every link, 0.42-0.45 s apart; the Large's three-link
strings stop after two (the smash doesn't reach from the backhand's end).

### Milestone 10: combos

Plan 4 of the combat plan (https://claude.ai/artifact/4BijSWun94DiWFcjcYCRJp; there it is M9a, the
milestones having moved back by one). A combo is a string of chained blows.

**Chaining** (`Units/UnitCombo.cs`, `ComboRun`). One combo owns the move slot and hands it from link to
link. A link is cut at its **link-out**, a little into its follow-through (`ComboDefinition.LinkOut`:
15% of the way from contact to the recovery), skipping its recovery; the next starts at its **link-in**,
three quarters of the way through its own wind-up (`LinkIn`), slowly at first (`LinkStartPace` 0.5,
picking up to full in a quarter of a second). Only the last link plays its recovery, so a whole combo
carries the commitment of one blow.
- **The lunge is banked** at each hand-over: the root moves to where the hips have gone (the lunge's steps
  have put the feet there) and the pose is kept where it stands, so nothing snaps back; the next link is
  planned, and the last one steps home, from there.
- **The next link grows out of the last.** The outgoing link plays on through its follow-through as
  `Pose.LinkFrom`, and over the rest of the new link's wind-up its share of the pose eases from all to
  nothing: the hips' and chest's turns, the lean, the sink, the lunge, the reach bend and the weapon's way
  all carry over (a club swing into a backhand doesn't jump 150 degrees). The weapon arm's pull toward its
  planned striking shape and its rush near the strike come in as the last link's share goes. Whatever
  still differs between the last step of one link and the first of the next (the arm's solve, mostly) is
  carried on as the last link was moving and decays over 0.2 s (`SmoothHandOver`), the feet kept planted.
- **Planned during the combo**, from where the body is at the link-out: the link's preferred parts first.
  If the string's move doesn't land from there, another of the weapon's blows that does takes its place
  (a fighter improvises). If nothing reaches (the target gave ground), the combo ends with that link's
  recovery.
- **How combos break.** A link that lands and is stopped in the body carries on; a link stopped on a
  shield ends the combo, or, for strings that keep hammering (`ComboBlocked.Continue`), the next link comes
  as it rebounds. Knocked off its feet, flinching or staggered, or out of energy (each link costs the
  tempo's blow cost), the combo ends.
- The planner's dry poses no longer carry the moment's knocks and recoils (they fade before contact).

**The strings** (`Combat/ComboDefinition.cs`), from the moves each weapon has today (the plan's M9b moves,
the club backhand, fan strike, off-hand jab, butt strike and shield bash, come next):

| body | string | links | if blocked |
| --- | --- | --- | --- |
| Large | body then head (Boxing 1-6-3) | rising club to the belly or knee, club swing at the head | stops |
| Large | hammer (Boxing 1-2) | club swing at the head or chest, smash at the head | keeps hammering |
| Large | body, head, smash (finisher) | rising club, swing at the head, smash | stops |
| Medium | double thrust (Melee Combo Ver. 3) | thrust, backhand, thrust | stops |
| Medium | forehand, backhand | cut at the head or neck, backhand at the body | keeps hammering |
| Medium | cut, backhand, cleave (finisher) | cut, backhand, cleave | stops |
| Small | rompida (FMA) | rising diagonal, falling diagonal | stops |
| Small | flowing | swing, rising, swing at the elbows and head | keeps hammering |
| Small | sero-sero (finisher) | swing, rising, swing, smash | stops |

**When combos come out** (`FightLoop.ChooseCombo`). Pressing with the energy for two links (9% of the
blows it decides on), countering (12%, two links), a target reeling (25%, a finisher string), and the first
blow fresh out of a full gather (60%, its finisher). Length: the least of the string's links, the energy over a link's cost, and
two plus the aggression's share of two more.

**Logging.** `events.log` has a `combo` line as each starts (string, why, links, target), links (move and
part) and ends (why, after how many links). The `fight` runner prints the combos, the share of blows inside
them, the time from one striking moment to the next and from one link's start to the next, and how they
ended. The `combos` runner throws every body's strings at a standing opponent.

**Measures** (stub). Tests (`ComboTests`): each later link starts at its link-in, its start 0.3 to 1 s
after the last link's; no part of the pose pops at a hand-over more than a single blow's start does
(5 cm more in a step than the step before, by size; after a stopped link, from its rebound, 12); a link
that no longer reaches ends the combo; every body has three strings and a finisher. Four fights (seeds 7,
11, 3, 5): 25-32% of blows inside combos (target 25-40%), 5 to 8 combos a fight; from one striking moment to
the next, median 0.72-0.83 s where three or more were measured (target 0.4-0.7: a little slow; the hit stop
of a landed link and the slow start of the next count in it); no standing bodies overlapping (the banked
root keeps a body's room from the target, `BankRoom`); combos mostly end stopped in a body, knocked off their
feet or with the target down. The `combos` runner: every string throws all its links at a standing opponent
(the Large's finisher stops after two: the smash doesn't reach from there), links 0.5-1 s apart start to
start. Arms, aim and the get-ups as before (no misses, 27 blows turned down; the weapon arm inside its
limits).

**Before the combos, from the M9c playtest:**
- *The shield get-up* (`GetUp.HoldShieldUp`, `Motion/ArmFit.cs`): the arm reached straight across the head
  and chest to a threat on the sword side, its joints up to 120 degrees past their limits. It now reaches on
  its own side (a little across the chest at most), the shield turning toward the threat from there, the arm
  fitted inside its joints' limits (the shoulder and elbow solved in their angles, the wrist turned as far as
  it goes, carrying on from the last step), pushed out of its own head and body, the shield's roll chosen to
  suit the wrist. Tested: no more than 3 cm into its own body, no joint more than 10 degrees past its limits.
- *Twisted arms after blows*: the playtests had physical sword wrists 100 to 150 degrees past their limits
  for up to half a second after a blow (a held arm's joints swung free, so the servo could pull hand and
  forearm round separately). Held arms keep their anatomical limits now (their poses are solved inside
  them). The Large's bare off hand, up to 28 degrees past its elbow's limits in the pose, is fitted inside
  them. `arms.py` shows the joints within a second after a blow apart.
- *Shield contacts* (the playtest: 62-80% under 2 cm, up to 15 cm and 0.7 s): most of the deep ones were in a
  kinematic weapon arm or a standing unit's kinematic hips and legs, which no push moves, so the shield's cap
  only kept the shield in. Contacts with kinematic bodies aren't capped now (`GroundContacts.SetKinematic`).

### Milestone 9c, last part: weapons kept out of the ground, getting up behind the shield, dropped weapons

**What the hands hold stays out of the ground** (`Motion/GroundClamp.cs`, `PosePipeline.KeepWeaponsOffGround`).
After the arms are solved, every pose (standing, in a blow, getting up) measures the held weapon (points
from pommel to tip, each with the weapon's cross-section: a blade is wide across its edges and thin
through, the crossguard wider) and the shield (the lowest point of its rim, its back when it faces up)
against the ground. Anything lower than 2 cm (by size) is lifted: first by turning the wrist (at most
60 degrees, never further past the wrist's limits than the pose had it), then by turning the whole arm
up at the shoulder, the elbow keeping its bend. The floor is soft: the lift starts a few centimetres
above the margin and eases in (`Soft`), and it is measured where the low part of the weapon is (the low
points weighed together), so the arm never jerks when the floor takes over or when a different point
becomes the lowest. A shield lying nearly flat, or a low part hanging straight under the wrist, is lifted
by the arm, not the wrist.

**Getting up with a shield** (`GetUp`). Face up, the unit rolls 18 degrees onto its sword side and pushes
on the sword fist, the blade flat along the ground pointing back and away; the shield is held up toward
the threat (the enemy it knows of, or up and forward without one) at arm's length, swung round the
shoulder, pushed out of its own body along its face and lifted where its rim would dig in. Face down, the
push is as before, with the shield flat under the forearm, its rim on the ground and the hand a little
wider, and it swings up toward the threat once the body is off its hands. As the standing pose takes over,
its shield takes over too. The body as it lay is brought out of the floor once at the start (the clamp may
go further there), and the clamp comes in over the blend from it.

**The standing pose a get-up blends into keeps its arm** (`PosePipeline.Compose(..., armPlays)`). That pose
is composed each step without moving the feet, which made the weapon arm solve afresh every step since the
natural-arms push: at the end of a large unit's get-up the hand flicked 4 cm and back (`EveryUnitGetsUp`
failed in some test orders, at the last push too). Its arm now carries on from step to step.

**Dropped weapons** (`Weapon.Release`, `UnitSever`). A weapon or shield whose hand is cut off falls from it:
out of the hand, solid, with a body of its own (its mass, the hand's speed and the limb's throw, continuous
collision), to land and come to rest on the floor. A dropped shield's weight leaves the hand. Dropped
weapons are cleared away with their unit. A held weapon is solid while its unit lies on the ground, alive or
dead (`Weapon.SetLying`, from `UnitRagdoll`), and registered with `GroundContacts` as lying, so a standing
unit stepping on it only nudges it; standing again, it only reports what it touches. All weapon colliders
are on the **Weapon** layer (set when equipped; Setup builds new prefabs on it).

**Shield contacts in the log** (`Diagnostics/ShieldContacts.cs`, `PlaytestRecorder`). Each physics step,
each shield's overlaps with other units' parts are measured where the simulation has them (how deep, by
`Physics.ComputePenetration`), followed from the step a contact starts to the step it ends. Each one is an
event (`shield`: who, which part, how long, how deep); `session.md` has a summary, and `arms.py` (and
`analyze.py`) a table per shield. Targets: under 2 cm deep in 95% of contacts, none longer than 0.2 s.

**Measures** (stub). Tests: `BalanceTests.AShieldBearerGetsUpBehindItsShield` (face up and face down: it
gets up, no part of the pose pops, nothing it holds more than 1 cm into the ground after the blend from the
body as it lay, the shield at most 3 cm into its own body, facing within 45 degrees of the threat in at
least 80% of the second half), `EveryUnitGetsUp` (now with the sword out of the ground and where a pop is),
`SeverTests.LosingTheWeaponArmLosesTheWeapon` (it falls on its own, solid, thrown with the limb),
`SeverTests.AHeldWeaponIsSolidOnlyWhileItsUnitLies`, `ArmTests.ShieldContactsAreFollowedFromStartToEnd`.
`arms 7` and `arms 11` (whole fights): standing, nothing held ever lower than 2 cm over the ground (16,236 and
7,537 steps); getting up after the blend, never more than 1 cm below (lowest 0.2 cm, 1,443 steps). During the
blend from the body as it lay the stubs can be far into the floor (to 56 cm): the stubs' ragdolls have no
floor and their weapons lie anywhere; in play the held weapon is solid on the floor while the unit lies.
Arms, aim, shield and balance runners as before (the weapon wrist 19 / 23 in the guard, 32 / 37 in blows;
no misses, 29 blows turned down; the shield faces its threat 87% of the time in blows). **Only Unity can
show** the dropped and lying weapons resting on the floor, and the shield contacts (the stubs have no
collisions): look at a playtest's `## Shield contacts` and `arms.py`.

### Milestone 9c, second part: the arm points the weapon, stopped blows rebound, a solid shield

**The arm points the weapon, the wrist stays straight, and the arm keeps a person's shape**
(`Motion/WeaponArmSolve.cs`, `PosePipeline.PoseWeaponArm`). The weapon used to be put where the move
wanted it and the wrist bent to suit, up to 160 degrees past its limits. Now:
- The grip leans the weapon 20 degrees forward of the fist (`GripLean`, on both the animated and the
  physical weapon slot), as a hand holds a sword; a thrust leans it 35 degrees more as it drives in
  (`ThrustLean`), so the blade runs along the forearm.
- The arm is solved in its joints' own angles: the shoulder, the elbow and forearm, the wrist, each
  three angles as the joint table measures them (pitch, then the turns about Y and Z, their swing kept
  inside a cone). **Every angle stays inside the table's range**, so no shape an arm can't make is
  ever chosen: no elbow bent backwards, no shoulder twisted over, no hand behind the head.
- Within those ranges a few damped least-squares steps a pose weigh up, together: the striking part on
  its target (most at the striking moment, `ArmAimWeight`, much less in the wind-up and follow-through,
  `ArmAimWeightAway`, where a natural arm comes first), the weapon pointing the asked way, the wrist
  inside its easy range (22 degrees in the guard, 35 in a blow, roll 75), the elbow toward its pole
  (down and out), and the arm and blade out of its own body (`ArmBodyWeight`; the upper arm, spine and
  pelvis are now measured too, in `BodyClearance`, for the planner as well).
- A playing arm starts from last step's shape, carried on as it was moving, and no joint turns more
  than 25 degrees a step (90 at the striking moment, `ArmAngleStepCalm`, `ArmAngleStepStrike`): it never
  jumps. Away from the striking moment it holds its shape more firmly (`ArmStillCalm`).
- The planner's try at the striking moment keeps the arm's shape on the blow (`MoveState.ContactArm`);
  the playing arm is drawn toward it from 60% of the way before (`ArmPlanWeight`), so it arrives in that
  shape. Near the striking moment, an arm that has lost its way (2 cm off, or in its own body) also
  solves afresh and heads there, as fast as it may move.
- Short of a point ahead near the striking moment, the body leans after it (up to 50 degrees). Out of a
  blow the arm's shape eases (12 Hz). A guard that would put the blade into its own body moves out.
- `MovePlanner.InTheWay` (the bystander tests and the sandbox) uses the blade as the arm poses it.

**How it got here** (two fixes after playtests of this part). The first version turned a rigid
forearm-and-weapon piece round the shoulder, choosing the elbow's place afresh every step: where the
best choice flipped the arm turned over in a step (the shaking swings). Carrying the shape on from the
last step fixed that, but nothing in that solver knew the shoulder's limits: the shoulder was more than
10 degrees past them in 27% of blow steps (up to 130), the arm inside its own body in 12%. Solving in
the joints' angles makes the limits the edges of what can be chosen.

**The weapon arm is posed exactly in blows again** (`UnitCombat.SetAuthority`). The first version held it
by the servo in blows, with the joint limits on, to soften stopped blows. The playtest showed the
physical arm can't follow a fast swing's turns (rigid bodies turn at most 30 radians a second, and the
limits stopped it where the pose went past them): the sword hand ended 120 to 170 degrees off its pose,
the blade up to 1.9 m from it, and the tip's acceleration in blows was 10 times push A's. The weapon arm is
kinematic in blows and while a stopped one fades, as before, and a held arm's limits are as in push A.
The servo still caps how far it pulls a part round in one go (120 degrees) and ignores a pose that jumps
more than that in one step.

**A stopped blow rebounds** (`UnitCombat.StopSwing`, `Rebound`). A blow stopped by a shield, weapon or
body now rewinds at most 12% of its length (`RewindMost`, `MoveState.RewindFloor`) and the weapon arm's
pose takes a rebound knock back along the blow (a light stop 0.15, a hard one 0.25 of the arm's knock),
then comes back once.

**A solid, weighted shield** (`Weapon.Equip`, `GroundContacts`). A physical unit's shield collider is
solid, not a trigger: it stops against bodies and weapons instead of passing into them. Its contacts
are capped at 2 N·s a step (`ShieldPush`), so it shoves rather than launches. Its mass is added to the
hand's body and taken off again when it is unequipped.

**Measures** (stub, the pose; `arms 7` and `arms 11`, a whole fight each, against push A and the
first playtested version). The weapon arm's joints more than 10 degrees past their limits in blows: the
shoulder 0% (push A 6.5%, then 27%), the elbow 0% (push A 8.6%), the wrist 0% (push A 84%). The arm
inside its own torso more than 2 cm: 0% in the guard and in blows (push A 3.6% and 1.7%, then 10% and
13%). The wrist's bend in the guard 18-22 median / 23-30 95th, in blows 31-33 / 37 (the reference
attack FBXs: 23 / 45). Smoothness, three fights: the arm's turning speed changes from step to step at
most 420-590 degrees a second at the 95th percentile (push A 720-1,930), the blade's tip turns back on
itself 19-23 times (push A 36-46). `aim`: no misses; 29 blows turned down for passing through the
attacker's own body (push A 5: the body check now counts the upper arm, spine and pelvis), most of them
a large unit's. Tests: `SwingTests.TheWeaponArmKeepsAPersonsShapeInABlow` (fails on the playtested
version: the shoulder 129 degrees past its limits), `SwingTests.TheWeaponArmNeverJumpsInABlow`,
`ArmTests.InTheGuardTheWeaponWristStaysNearlyStraight`, `ArmTests.TheShieldIsSolidAndWeighsOnTheHand`.
The bystander tests allow a cut that severs the bystander's guard arm first (the blade comes in on a
slightly different line now; a severing cut carries on). Playtest measure of the shaking: the swing
section of `python3 Tools/Playtest/arms.py`; of the arm's shape: the `posePast` / `poseJoint` columns.

### Milestone 9c, first part: hits that land softly, a calm and close shield

Plan (straight wrists, a close shield, hits that land softly): https://claude.ai/artifact/UJNwdKChsM7vidDhArKF9W.
This was its first push; the second (above) brought the straight wrists, the joint limits kept while
standing, a designed rebound for stopped blows and a solid, weighted shield.

**A servo instead of velocity pinning** (`AuthorityController`). A held part used to get, each step,
the velocity that would land it on its pose, blended with its own by the pin. That rings (about 9 Hz
at the arms' 0.7, slower as the pin drops), which is the antenna buzz after a hit. Now each part moves
with its pose (the pose's own velocity over the last step) and a critically damped spring closes what
is left, solved implicitly: pushed off its pose it comes back once, smoothly, at any stiffness. The pin
still sets how firmly (`HoldFrequency`: 0.7 is about 7 Hz, 0.3 about 2.3 Hz); 1 still lands exactly.

**A hit knocks an arm aside once** (`UnitCombat.KnockArm`). A hit on an arm, or on the shield it
holds, kicks a critically damped spring in that arm's pose (2.5 Hz): the hand goes back along the blow
and turns with its lever, up to 12 cm and 25 degrees (saturating with the blow), and comes straight
back. The physical arm takes the blow as one, hand, forearm and upper arm together, with the shield's
weight counted, at most 2 m/s; it softens to about 2 Hz and firms up over 0.4 s.

**The shield close and calm** (`PosePipeline.ControlledShield`, `UnitShield`). The guard is built
from the arm: the elbow hangs by the ribs, the forearm points toward the threat 15 degrees in across
the body, and the shield faces where the forearm points (the centre grip, wrist straight). Raised, the
elbow comes up; in a block it opens toward the blow. Its aim holds still until the threat has moved
12 degrees, then holds again for 0.3 s; its state holds 0.25 s; it eases in critically damped. A seen
wind-up turns it once toward where the blow will come from (it no longer chases the weapon round), and
only from a clearly seen attacker within reach. It turns at most 250 degrees a second (720 in a block),
never flips across the back as the chest turns away, and takes a third of the Fight Rhythm's sway.

**Sandbox.** **J**: the shield test. A shield-holder in the middle; a sword unit and a club unit take
turns striking at it from in front (two blows each). Watch the knock and the return.

**Measures.** `arms [seed]` (runner): a whole fight, measuring the pose: the weapon wrist's bend in the
guard and in blows (as the reference attack FBXs are measured: median 23, 95th 45 degrees), the shield's
turning by state with no hit in the last second, and the shield hand's distance from the chest.
`python3 Tools/Playtest/arms.py <session>` (also a section in `analyze.py`): the same on the physical
body of a playtest, plus the struck part's turns back, offset and spin in the second after a hit, the
attacker's weapon hand after a stopped blow, and joints past their limits. `Tools/Playtest/reference_wrists.py`
measures the reference FBXs (Blender as a module).

### Milestone 9b: hands, wrists and bodies on the ground

Plan for the bodies on the ground: https://claude.ai/artifact/AsWxyew3t7oX8YFtrSVYFg

**Joints that hold their limits** (`Muscles/JointMath.cs`, `Authority/AuthorityController.cs`). The
limit table is reworked from measured poses: the forearm's roll now shows at the wrist (80 degrees
each way), the elbow and knee are hinges (15 degrees of side play), the shoulder goes out to 130 for a
high guard. When a unit goes down its joints don't snap to their limits: each starts wide enough to
hold the joint where it is and closes to its ragdoll limits over 0.3 s; the elbow's and knee's side
axes lock once they are within 8 degrees. While it lies, each joint has a little muscle tone (a
damper, more for a downed unit than a dead one), its parts collide with each other (not with their
neighbours) and the solver works harder on it (20 and 8 iterations). A light limb on a heavy body is
weighted at most 3 to 1 in the joint solve (`connectedMassScale`), so a push on it doesn't tear the
joint.

**Heavy on the ground** (`Units/UnitRagdoll.cs`, `Units/GroundContacts.cs`). A standing unit's legs are
kinematic and its body driven hard to its pose, so to PhysX they weigh nothing less than a wall: a
contact between a standing unit's part and a lying body's part is capped so it can change that part's
speed by at most 0.2 m/s a step. Bodies are nudged and rolled, never flung. Once a body has settled
(hips under 0.5 m/s for 0.3 s) it grows heavy over 0.5 s: more damping, more friction, a slower push
apart when parts overlap. A blow, or being thrown along faster than 1.5 m/s, makes it light again for
a moment. Walking units keep round a body on the ground (`FightLoop.AroundBody`: its head, hips and
legs, with 12 cm to spare) instead of walking through it.

**Natural arms and fists** (`PosePipeline.NaturalArm`, `AlignHinge`). An arm holding a weapon or shield
turns its elbow round the shoulder-to-hand line to where the wrist is straightest (eased at 300
degrees a second, never jumping); the elbow and knee bend about their hinge; what the wrist still
can't take is held at its limits (not during a blow: the blow's weapon turn wins). Knees point within
40 degrees of where the hips face. The hands are now closed fists round the weapon slot
(`SourceArt/Bodies/Renders/fists.png`): the grip runs through the fist along the knuckles, thumb on
top. The shield is held by its centre grip, the bar upright through the fist and the forearm pointing
at the boss (the wrist stays straight); it is held out from the shoulder and pushed out along its face
where its rim would cut its own arm.

**Sandbox.** **D**: the ground test. The two lightest units go down side by side across the middle
(one knocked down, one killed); the others walk back and forth straight across them for 20 s. Watch
the bodies: they should stay in one piece within their limits, get nudged and rolled, not shaken or
thrown. The setup runs again on opening (version 19) to rebuild the unit prefabs with the fists.

**Logs.** New columns: `jointPast` and `joint` (the physical body's joint furthest past its limits, in
degrees), `posePast` and `poseJoint` (the same for the pose), `lyingSpeed` (its fastest part while it
lies) and `nearLeg` (how close the nearest standing unit's legs are). The summary has "Pose against
the joint limits" and "Ragdoll" lines; `analyze.py` has a "Bodies on the ground" section (also
`python3 Tools/Playtest/ragdoll.py <session>`), which measures older logs too.

**Runners.** `poselimits`: each body squared up with its rhythm and tempo running for 10 s, each joint
of the pose against its limits.

### Milestone 9a: seeing, the head's look and the shield under control

**Perception** (`Combat/Perception.cs`, `Units/UnitPerception.cs`). A unit sees from its head: clearly
within 30 degrees of where it points, movement out to 100 degrees each way (70 up and down), and it
feels what is within about a body length. It remembers each unit it has perceived (where, when, how it
moved) for 3 s after losing sight of it. These are per unit, on the profile (`perception`). A blow is
reacted to only once the unit sees it: at the edge of its view it reacts 1.5 times slower; one it
never sees lands unopposed, and the attacker is known from then on. Pressure (the tempo) counts seen
blows, and unseen ones only when they land. The fight loop picks targets only among enemies a unit
knows of (with none, it turns about to find one) and reads openings only in clear view.

**The head's look** (`Motion/HeadLook.cs`, `Units/UnitLook.cs`). An attention score picks what to look
at: its own blow's aim, a blow it has seen coming, the weapon of the enemy it minds as it winds up,
someone new at the edge of its view, a glance now and then at whoever is nearest, and otherwise that
enemy's face. A new point must clearly beat the current one, which is held at least 0.4 s (a much
stronger one, like a blow, cuts in), and a new stimulus is answered after 0.1–0.2 s. The look turns
eased, a big one in about a third of a second, never over 350 degrees a second. The chest takes a
fifth of it (and what the neck can't, up to 30 degrees; not while a blow owns the chest), the head the
rest within its limits (80 degrees round from the chest, 60 down, 50 up); the eyes take the last few
degrees, so the head doesn't point dead on.

**The shield** (`Combat/ShieldCone.cs`, `Units/UnitShield.cs`, `PosePipeline.ControlledShield`). It
always points at the threat the unit knows of: the enemy's chest (**Guard**), its weapon as it winds
up (**Track**), a seen blow's path where it comes in (**Block**, the face set against the weapon's
travel), raised to the eyes when shelled up (**High**). It moves there eased, at most 300 degrees a
second toward a blow and 150 back. It stays inside the cone a shield arm covers from the chest (40
degrees across the body, 90 out to its side, 35 down, 55 up), never behind the chest, held out as far
as the arm reaches with the elbow a little bent, and out of its own body (its rim and face are checked
against the torso, head and thighs; it slides out round the shoulder, or turns edge-on). A seen blow
coming in from outside the cone turns the unit to face it, and it blocks once the blow is inside, if
there is time. Tests and other measurements can switch the look off (`LookOn`).

**Logs.** New columns: `look` (what it attends to), `headTurn`, `headPitch`, `lookOff` (how far its
gaze is from it), `known`, `shield` (its state), `shieldOff`, `shieldClear`. New `sight` events (noticed,
lost track of). The `defence` events say how each blow was seen (or "never saw it"). The summary has
Defence, Look and Shield lines.

**Runners.** `shield [dir]`: a sword-and-shield unit, busy with an enemy in front of it, is attacked
from 12 directions with each swing; prints how it saw each blow, what it did, and what was struck;
with a folder, `Tools/Playtest/shieldview.py <dir>/shield.csv out.png --trials 1,13,33` draws film
strips. `look [dir]`: two enemies walk round a unit; `Tools/Playtest/lookview.py <dir>/look.csv out.png`
draws its view zones, what it sees and where it looks.

### Milestone 8: the tempo

The blows no longer come on a fixed clock. Each unit has a **tempo** (`Motion/Tempo.cs`,
`Units/UnitTempo.cs`), and its mood decides when it attacks:
- **Gather.** Holding back just out of the target's reach, circling, settling its guard. Energy comes
  back fast (0.25 a second). Sometimes it only rests until it can probe; sometimes (`holdChance`) it
  holds on to a full charge and goes straight into a press.
- **Probe.** Single blows and feints now and then, energy coming back slowly.
- **Press.** Charged and fired up (energy and aggression over the style's levels), it closes right in
  and throws blow after blow, 0.1–0.45 s apart (combos come in milestone 9), until it is spent, its
  burst is done, or the press is broken (two blocked blows, a heavy hit).
- **Shell.** Under a barrage (pressure: each blow at it adds 0.15, 0.4 from a pressing unit; it fades
  once 1.5 s pass without one), it shells up: crouched behind its guard, giving ground, raising the
  shield more readily (milestone 10 makes it defend harder).
- **Counter.** When the barrage stops, one quick answer at once.

Each state holds a **stance** on top of the Fight Rhythm, eased in over 0.4 s, and it stays on through a
blow: gathering sits lower with the weight back, the chin tucked and the guard in, the base loop slow and
deep; pressing leans in on the front foot, hands up and out, the loop quick; the shell is each style's
own (the boxer's high shell, the shield raised to the eyes, the stick fighter low with the stick across).
Each style's **temperament** (on its Fight Rhythm asset) sets its levels, burst, composure and stances:
the boxer presses early and long and is hard to rattle; the swordsman gathers long, presses short and
shells early; the stick fighter flurries in and out.

The postures were made two to three times bigger (each now moves the hands, head or hips at least 10 cm,
scaled by the body, beyond what they did just before), each plays only in the tempo states it suits, the
ones that don't move the feet play while walking, and two new ones per style: a settle while gathering
and a stalking step in while pressing. Setup 18 rebuilds the Fight Rhythm assets from the code (tuning
done in them is lost).

The `fight` runner prints the share of time in each state; the playtest log has a `tempo` and a
`pressure` column and a `tempo` event at each change, with the reason.

### Milestone 7: measuring the rhythm, and free roam

**7a, the measurements.** Every playtest log now says how a fight *moves*, not just what hit what
(`Tools/Playtest/tempo.py`, also written into `analyze.py`'s report):
- each unit's time on its feet split into walking, in a blow, in a posture, blocking and the idle loop
  alone (and into tempo states once they exist, milestone 8);
- each posture's **extra motion**: how far it takes the hands, head or hips beyond the idle loop's own
  reach, in cm. Under 10 cm it is flagged: it can't be seen;
- energy's range and its charge-and-spend cycles; the gaps between a unit's own blows and the share in
  bursts (under 1 s apart); within each pair, how often the next blow is the other unit's (over 55% reads
  as taking turns); blows thrown into the target's own blow;
- **defence** events: every blow coming at a unit and what it did (raised the shield, didn't, couldn't:
  mid-blow, off its feet, nothing to defend with).

`python3 Tools/Playtest/tempo.py <session> [<session> ...]` pools several fights by body. The `fight`
runner prints the headline (gaps, bursts, alternation), and `windows` lists every move's timing as a
defender sees it (the wind-up's end, the drive, contact) on each body, for the dodge design.

**7b, free roam.** Once the fight starts, units leave the tiles behind (`Battle/FightLoop.cs`); the tiles
are for placing units. Each unit walks straight to a ring round its target at its striking distance,
steered by a smoothed velocity: what it wants, plus a push away from every body too close (standing or
lying, either team; more room between two of a team on one target, to swing in) and back from the board's
edge. Small wants are dropped, so the feet don't take nervous little steps. A target with three attackers
already (`RingSize`) is passed over for another. No headway for 2 s and it goes round, to one side for a
moment. On the ring it keeps its distance, its Fight Rhythm circles it, and it strikes as before.

### Milestone 6: the fight loop

Press Play and the two teams fight it out with no help (`Battle/FightLoop.cs`):
- **Targets.** Each unit picks the nearest enemy that can still fight. It keeps
  the target it has, and favours one that is already hurt or already fighting it. Someone on the ground is a
  target only when no one is standing.
- **Tile to tile** (replaced by free roam in milestone 7). It walked across the board from tile centre to tile centre (8 ways, round others, never
  cutting their corners) to a free tile next to its target, playing its captured walk at a speed that goes
  with the square root of its size. The tiles a unit stands on and is heading for are held, so no two share
  one, and a body on the ground holds the tile it lies on. A unit knocked off its tile, or onto someone
  else's, walks to the nearest free one. After getting up it goes back onto the board.
- **On its tile** (now: on its ring) it closes in or gives ground a little (never off the tile). When its timer comes round,
  it strikes its target or any other enemy in reach in front of it, at a body part one of its moves can
  land on.
- **Openings** (the question pinned in the Fight Rhythm plan). An opening is a body part the target leaves
  open right now: a posture's window (the bob and weave at its lowest, a peek over the shield, an abanico's
  wide flicks), or the head once its guard has sagged with tiredness. Each style reads them some of the time
  (`readsOpenings`: the small stick fighter 75%, the swordsman 55%, the boxer 35%). When it reads one, it
  aims at the open part (four times likelier), and strikes up to `openingLead` (0.8 s) early to catch the
  window. It skips its lead-in, so the opening isn't wasted. The log notes each one it read.
- **The end.** The fight is over when one team has no one left who can fight: dead, unarmed or legless. A
  time limit (240 s) ends it on who is left. The result is shown on screen and logged; R starts another.

### Milestone 5: severing

An 8×8 board with three units a side: Small, Medium and Large. Small and large carry clubs, the
medium ones a sword and shield. They close in on the nearest enemy and strike at its body parts:
head, neck, chest, belly, elbows or knees. What the weapon meets takes the hit, and a hard hit
throws the body: off balance, off its feet, or through the air into whoever is standing there.

### Milestone 5: severing

A sword's edge can cut a limb off at a joint: the neck, a shoulder, an elbow, a wrist, a hip, a knee or an ankle.
It cuts through when all of these hold:
- the edge passes through the limb within about 5 cm of the joint (scaled by size), where the blade actually met
  the body (a cut into the top of the chest doesn't take the head off; the neck goes only to a blade that met the
  head or neck);
- it moves across the bone, not along it;
- it is faster than the weapon's `severSpeed` (7 m/s for the sword);
- the blow does enough damage for that joint (`DamageRules.SeverDamage`, times the struck unit's size: a wrist is
  easiest, a hip hardest).

A slower or lighter cut only wounds. Clubs and thrusts never sever (for now). The blade carries on through a cut
limb.

The limb leaves the unit as a loose physics object, with its mesh, colliders and anything its hand held. It isn't
posed, counted or struck as the unit any more, and it goes when the unit is removed. The unit loses what the limb
did:
- **the weapon arm:** its weapon goes with the hand and it can't strike;
- **the shield arm:** its shield goes and it can't block;
- **a leg:** it goes down and stays down;
- **the head:** it dies.

Every cut also costs a share of health (`DamageRules.SeverLoss`) and bleeds (`BleedRate`): a lost leg kills in
about 20 s, a hand in about a minute. Moves never aim at a limb that is gone. Cuts at an elbow, knee or neck are
deliberate sever attempts: the aim point is the joint itself. Unit labels show what was cut and the bleeding. The
log records each sever, and on near misses, why the cut didn't sever (too slow, not hard enough, along the bone,
too far from the joint). **K** is the sever test: a sword unit cuts at an enemy's elbow.

How a maimed unit fights on (switching hands, hopping, crawling) is still open in the spec, to design through play.
For now an unarmed unit stands and guards while it bleeds, and a legless one lies where it fell.

### Fight Rhythms: how each style moves between blows

The plan is in the "Fight Rhythm" doc. Every unit now has a **Fight Rhythm**: a base loop it
always plays, and four **postures** (short bursts of its style's characteristic movement) mixed in
over it. A **mood** changes the mix over the fight.

| Unit | Style | Base | Postures |
| --- | --- | --- | --- |
| Large | Boxing (the club for a right hand) | A captured boxing idle (Boxing.fbx): bladed and long, rocking forward and down onto the lead leg and back up, the body turning with it, the lead fist up in front of the shoulder, the club laid back over the right shoulder | Club feint (the real swing's wind-up, a third of the way, and back), jab feint (lead hand half out with a small step in), bob and weave, shift (the rear foot steps through to lead and back) |
| Medium | Round-shield and sword | A captured idle (Medium_Idle.fbx): side-on, the feet nearly in line, sitting low and back, the shield arm out toward the opponent, the sword hand in front of the chest, the point circling a little, circling to the side | High feint (the cut's wind-up over the shield), shield press (with a half step), guard change (sword overhead, then on the shoulder; shield down, then up), peek (the shield dips) |
| Small | Eskrima (the club as a single stick) | A captured idle (Small_Idle.fbx): side-on and low, the hands low, the alive hand out in front, the stick held low pointing forward and making a figure-eight from the wrist | Abanico flurry (fan flicks), triangle step (off the line to a new angle), redonda twirl (a circle of the stick overhead), bait retreat |

- **Patterns, not dice.** Each posture has a usual follow-up, which is made likelier after it. The
  same posture never plays twice in a row. Before a blow, the style may play a **lead-in** first;
  the boxer does this most often, and the blow comes as the feint ends.
- **Mood.** Energy falls a little with every posture, blow and hit taken, and comes back slowly.
  As it falls, postures thin out (half as many at half energy), the bounce flattens and the guard
  sags. Below 35% the head is open. Aggression rises with hits landed and falls with hits taken.
  It favours feints over defensive postures.
- **Openings.** Every posture marks windows where body parts are open, such as the bob and weave at
  its lowest. The unit's label shows them as they happen, and the log records them. Nothing acts on
  them yet; that waits for the fight loop.
- Weapon feints play the unit's own blow's wind-up and never strike. Postures pause for blows,
  blocks and stumbles, and a blow takes over from the rhythm as it blends in.
- **H** is the rhythm gallery: each style's base, then every posture slowed and at full speed.
  The panel can make everyone tired, to see the difference.
- **Captured loops.** A rhythm's base can be a loop from motion capture instead of the made-up
  bounce and sway (its `clip`): every body's is a Mixamo idle (Boxing, Medium_Idle, Small_Idle; the
  FBX files are in Tools/Mocap). `Tools/Mocap/mixamo_rhythm.py`
  reads a looping Mixamo FBX (binary, skeleton animation) and writes it as a clip: the hips' rock and
  bounce, the pelvis and chest turns, the lean, the head, and both hands, as offsets from the loop's
  mean pose. It also prints the stance (feet apart, how low, how turned) and where the hands sit, which
  belong in the combat style and the rhythm's guards (with `--rig`, a `rhythms` CSV of the unit, it works out
  how far to move each hand). `Tools/Mocap/compare.py` renders the capture and the unit side by side (a video).
- **Captured walks.** Each body walks with a Mixamo walk (Big_Walking, Med_Walking, Small_Walking: the combat
  style's `walk`). When the game walks a unit, the walk fades in with the speed: the feet step to the capture's
  cadence, lift and width (each lands about half a stance ahead of where the hips will be, never across the other
  foot; a foot left behind or going the wrong way after a sharp turn steps again at once), the hips go along with
  the root, and the body takes the walk's lean, crouch, turns, head and arm swing round the guards. The cadence
  slows with a slower walk (the steps shorten), but never so much the feet can't keep under the hips. Standing
  still, the walk fades and the stance takes the feet home. A blow, a block or a knock-back stops it.
  `mixamo_rhythm.py --gait` reads a walk (with its root motion), `Tools/Mocap/walkview.py` shows a unit walking
  next to it. A side-on stance (the medium and small ones) squares up into a blow, so the swing clears the body.

### Milestone 4: stumbles, ragdolls, get-ups

Every blow's push (the impulse its struck part didn't take, over the unit's mass, half as much
again when its poise breaks; plus the shove of a swing cleaving through it) decides what happens,
against the unit's size:

| Push (medium unit; × √size) | What happens |
| --- | --- |
| under 0.8 m/s | Recoil as in milestone 3: the body is thrown back, the root slides, the feet keep up. |
| 0.8–2.2 m/s, or its poise breaks | **Stumble** (the stagger you asked for): knocked off balance in the blow's direction. The upper body leans with the fall (14° per m/s, up to 30°), the hips go ahead of the feet, the arms go out loosely, and the feet take quick catch steps after the body. Each catch step takes 45% of the speed. It ends up 0.3–0.9 m from where it was struck and stands again. It can't attack or block meanwhile. Stumbling into someone knocks them by their share. |
| 2.2–4 m/s | **Knocked down**: a full ragdoll, the upper body thrown hardest so it topples over its feet. |
| 4 m/s and over | **Launched**: the whole body thrown, up as well as along (at most 6.5 m/s). |

- **Bodies hurt.** A body down that is moving fast (over 1.5 m/s) into another unit's body strikes
  it. The impulse is half the closing speed times the two units' reduced mass. It does 0.15 damage
  per N·s (by body part) and knocks the unit struck the same way, so it can stumble, fall or be
  launched in turn. The thrown body takes half the damage. It never hits the one who knocked it
  down: their swing is carrying it.
- **Getting up**, as the spec has it. Once the pelvis has been still for 0.3 s (after at least
  1.1 s down, at most 4 s), the unit gets up face down or face up, by which way its pelvis faces.
  The root goes under the body facing along it. Every part blends from how the ragdoll lies to
  the first key pose in 0.3 s (legs first, then the legs are kinematic again). The key poses then
  play: face down, push up, hands and knees, one knee, rise; face up, sit up on the hands, push
  into a crouch, rise. Then it blends into the standing pose. About 2 s, longer for bigger bodies.
  The key poses are made in code from a few numbers in proportion to the body, so one set fits
  every size. Poise is restored.
- **Back onto the board.** Once up, it walks to the middle of the nearest free tile, then fights on.
- What sends whom where with today's weapons: a large club knocks a medium unit down and launches a
  small one; a medium sword cleaving through a small unit, and any blow that breaks poise, makes it
  stumble; equal sizes mostly recoil.
- **L** is the launch test: the heaviest armed unit swings at the lightest enemy, with a third unit
  standing where the blow will throw it.

### Big swings (before milestone 4)

The attacks were pokes: short point paths that stopped at the target's skin, the arm bent at
contact, 3–10 m/s. They are now swings, built from a handful of numbers per move
(`MoveShape.Swing` and `Thrust`; the plan is in the "Big Swings" doc):

- **An arc about a pivot in the body**, in a tilted swing plane through the aim point: 160–220° of
  wind-up behind the shoulder or head, 110–160° of follow-through past the target, 280–380° in
  all. The weapon points out from the pivot at full reach, trailing the hand as it accelerates and
  whipping through at contact.
- **Through the target, not to it.** The unit steps in until the aim point is closer than its full
  reach, so the striking part arrives still reaching out and carries on past the aim point.
- **The whole body behind it.** The hips turn ahead of the weapon and the chest between them
  (200° or more of chest turn in the big swings). The body crouches and rocks back in the wind-up,
  then rises and leans 20–40° into the blow. Every blow steps into it, with a passing step (the rear
  foot through to the front) for the cleave, the haymaker and the thrust.
- **Timed like a real cut.** A slow wind-up, a tense hold, a strike that accelerates and then runs
  flat out through contact at 12–19 m/s, a braking follow-through, and a calm recovery that comes
  back round the front. The strike is much the fastest part.
- **Seven moves.** Sword: forehand cut, backhand cut, a flat cleave (new), and a run-through thrust
  that drives the point 60 cm past the aim point. Club: the haymaker, an overhead smash from
  behind the back, and a rising blow from low behind (new).
- **Cleaving.** A swing keeps 1 − struck mass ÷ (struck mass + attacker mass) of its speed through
  a body. If that is at least 55% it carries on, slowed, and the body is shoved out of the arc. So
  a large unit cleaves through small and medium units, and a medium through a small one; equal
  weights and shields stop the swing.
- **G** plays every move in turn against a dummy, at a third of the speed and then at full speed.
  Heavy hits shake the camera a little (panel toggle). Swings leave a short trail.

### Milestone 3: hits

- **Every weapon contact is a hit.** From the start of a blow's active window to the end of its
  follow-through, the weapon is swept from where it was last physics step to where it is now. The
  first body part or shield it meets is struck, whoever it belongs to: the part aimed at, another
  part of the target, an ally (friendly fire does full damage) or someone else standing in the
  way. A touch slower than the weapon's *min hit speed* (2 m/s at the striking part) is not a hit.
- **The weapon never passes through a body.** A body too heavy to cleave through stops the blow: it
  is wound back to the moment it met the body, held there for the hit stop, then bounces back
  along its own path as it fades out. A lighter body is shoved out of the arc and the swing goes
  on. The weapon arm stays posed throughout (nothing goes loose, so the weapon can't spin).
- **Weight matters.** Damage goes with the weapon's speed and the part struck (the head hurts most,
  hands least). The impulse is the weapon's mass at the wielder's size, plus some arm, times its
  speed. A club's weight lands; an edge or point cuts in and passes on less. The same blow throws
  a light body much further than a heavy one.
- **Struck bodies look struck.** The pose recoils: the hips are pushed with the blow and the knees
  give, the upper body is thrown back and twisted the way the blow levers it, and a blow to the
  head snaps it round. It springs back with a little overshoot. The struck part and the part it
  hangs from also take the impulse physically and are held loosely for a moment. The rest of the
  blow knocks the whole unit back, and its feet stumble after it. The attacker jolts too.
- **Flinch and stagger.** A hard blow is a **flinch**: a bigger recoil, and the unit's own blow is
  cancelled. Losing all poise is a **stagger**: the biggest recoil, knocked back further, blow
  cancelled, then poise comes back. At zero health the unit goes limp (getting up, being knocked
  down or launched come in milestone 4).
- **Bodies don't share space.** Each unit takes up a circle around its hips. Units that come closer
  than that are pushed apart, the lighter one more, and their feet step after.
- **Hit stop.** At each hit the attacker and the struck freeze for 40–100 ms, longer for heavier
  blows.
- **Shield blocks.** A unit with a shield that sees a blow coming may raise it across the blow
  after its reaction time (`blockChance`, `reactionTime` in its combat style). A blow that meets
  the shield does no damage, costs some poise and stops the swing.
- **Cut-short blows let go smoothly.** A stopped, flinched or staggered blow fades out of the pose
  over 0.35 s from where it was, and feet out in the lunge step back.
- **Health and poise per size:** small 60/25, medium 100/45, large 220/120. Poise comes back at 15
  a second. The labels show them.
- **The gate test:** *a bystander in the way takes the blow* (`HitTests`). A third unit stands
  across a cut aimed at another; it is struck first, the blow stops on it, and the target is
  untouched. Press **B** in
  the Battle scene to see it: fighting stops, a sword unit strikes at an enemy, and a third unit
  is stood in the way.

### Milestone 2: aimed blows

- **Aimed blows.** When a blow starts, it locks the aim point where the chosen body part is at that
  moment. The move then lunges and steers the weapon so its edge, point or club head arrives
  there. A target that sways, steps or lunges meanwhile can be struck somewhere else, or missed.
  This is what the combat idle is for.
- **Planned to land, clear of the wielder.** A blow is only chosen if it can land and stays clear of
  the attacker's own body. The planner poses the attacker at the moment of contact and checks that
  the striking part reaches the aim point within 3 cm. It also poses the whole blow and checks that
  the weapon and weapon arm never pass through the attacker's own head or torso. In the tests every
  planned blow lands and stays clear, for every size against every other.
- **Aimed along the line of the blow.** The move's path is laid out along the line from the
  attacker's chest to the aim point, so a large unit striking a small one winds up above and beside
  its shoulder and comes down onto it. Sideways distances scale with the attacker's breadth.
- **Bodies adapt to the target.** Units sink and bend toward low targets and reach up to high ones.
  The lunge's steps go with the drive forward after the wind-up. Long lunges are advance-lunges:
  the lead foot steps out and the rear foot follows, so the hips stay up.
- **Stance and idle (milestone 1).** Each size has its own stance and combat idle. Feet stay
  planted and step one at a time. Legs are animation-driven (kinematic), so they always carry the
  body; torso, head and arms are physical parts pinned to the pose.
- **Fighting anyone in reach.** Units close in on the nearest enemy but strike at any enemy in reach
  in front of them. If nothing lands cleanly they close in, or give ground when they are too close
  to swing.

The lineup is laid out in the Battle scene as **unit markers** (Lineup > A Small, ...). Each one
shows its unit standing on its tile, armed and in its team colour, while you edit. Move a marker
and it snaps to a tile. Change its body, team or weapons in the inspector. In Play the previews go
and the real units spawn on the markers.

## Opening the project

1. Open the folder in Unity Hub with **6000.6.7f1** (any 6000.6.x should do).
2. On first open the project sets itself up (Console: `[Battler Setup]`): URP, materials, the body meshes' import
   settings, the tuning assets, the three unit prefabs, the weapons and their moves, and the Battle scene. It opens
   `Assets/_Battler/Scenes/Battle` when done. Setup version 10 adds the weapons and a lineup of unit markers to your
   Battle scene (only if it has none). Version 11 refreshes the ready-made moves and, if your lineup is still the
   default one, rearranges team B so each unit faces a different size. Version 13 gives the small and large
   profiles their health and poise and each weapon its damage and hit radius (unless you changed them). Version 14
   rebuilds the ready-made moves as big swings (moves still on the old hand-drawn path) and adds the sword cleave and
   the club's rising blow to the weapons. Version 15 adds the Fight Rhythms (Data/Rhythms) and points each body's
   combat style at its own. Version 16 gives the boxer the captured boxing idle: the large style's stance and
   `RD_Boxing`'s clip and guards (unless you changed them). Version 17 does the same for the medium and small
   units' captured idles, and gives every combat style its captured walk. Run setup again any time from **Physics Battler > Setup > Run Project Setup**.
3. Press Play. Commit the `.meta` files Unity creates for the new scripts and assets.

## Battle scene controls

| Input | Does |
| --- | --- |
| R | Reset the board (respawn from the markers) |
| F | Fighting on or off (off: they only stand and face each other) |
| 1 | Target test: instead of the battle, three attackers each on a target that only stands there (from the locomotion lab, 1 instead of F goes straight to it) |
| B | Bystander test: fighting stops, a sword unit strikes at an enemy with a third unit stood in the way |
| G | Move gallery: every move in turn against a dummy, at a third of the speed and then at full speed (G again stops) |
| L | Launch test: the heaviest armed unit swings at the lightest enemy, with a third unit standing where it will be thrown |
| H | Rhythm gallery: each Fight Rhythm's base, then every posture slowed to 40% and at full speed (H again stops) |
| K | Sever test: a sword unit cuts at an enemy's elbow; the arm should come off there |
| D | Ground test: the two lightest go down (one dead), the others walk back and forth across them for 20 s |
| J | Shield test: a sword unit and a club unit take turns striking a shield-holder's shield |
| T | Turn everyone around (and back): makes them step and turn on the spot |
| P / . | Pause / step one physics frame |
| M | Mark the playtest log |
| Right-drag, middle-drag, scroll | Orbit, pan, zoom |

**The target test** (1, or the panel's "Target test"; from the locomotion lab, 1 or its "Target test" button
instead of F). Three pairs side by side, 4 m apart, for looking at what a blow does without a battle around it.
From left to right the targets are one with nothing in its hands (it does not block: every blow lands), one with
sword and shield (it blocks), one with a sword alone (it parries). A target never strikes, faces its attacker,
and walks back to its spot when a blow has driven it off. Each attacker fights its own target and no one else.
The panel then has "Attackers: Mediums / Larges" (a Medium attacks with a sword, a Large with its club),
"Targets: Mediums / Larges" (the same three, as Larges), and "Targets take harm": off, as it starts, a target
loses no health and no limb, so it can be struck again and again (it still reacts, and its poise still breaks).
R resets the test; "Back to the battle" returns to the units the scene's markers lay out. Headless:
`FIGHT_LINEUP=targets` with `FIGHT_TARGETS="LM"` (the attackers' body, then the targets'; M or L) and
`FIGHT_TARGETS_HARMED=1`; the fight report then also says how far each target was driven off its spot.

**The panel** (laid out afresh on 14 November: it had grown past the bottom of the screen, every control with
its whole explanation beside it). Always there at the top: a bar of the five places (Walk lab, Battle, Targets, Tubes, Aim: where one is, and
a button to each of the others; the locomotion lab's panel has the same bar, so any of them is one press from
any other, and the battle can go back to the lab, which it could not), pause, step, reset, the panel's size
(− and +), the speed, and the recording.
Always there at the bottom: what the last thing pressed did, and a box that says what the thing under the mouse
does. Every control now shows only its name and its value; what it means is its tooltip, read in that box.
Between them the sections, each folded shut or open by pressing its heading (kept between sessions): Fight and
tests; Tube test (only in the tube test: falls on demand, a fall then a turn, blows on demand, the tube's own
settings); Aim test (only in the aim test, 7: built blows or the strike clips, a blow's line and size, a map of the body to ask a blow at each part, and what the last blow did); Hits and hips; Falls and getting up; Moves on demand; Legs and feet; Other switches; Keys and camera (the list of keys). From the lab, 5 goes straight to the tube test as 1 does to
the target test. The sections scroll when they are taller than the screen, and the
panel is never taller than the screen; over the panel the mouse's wheel scrolls it and does not zoom the camera. Not verified in Unity: the headless build only checks that it compiles.

**What is tuned in the panel is kept** (14 November). The panel's sliders and switches change the scene's Combat
Feel object while the game plays, and Unity puts a scene's objects back as they were when play stops: everything
tuned was lost. Now, when play stops, every Combat Feel setting that was changed is put on the scene's object
(`Editor/CombatFeelKeep`; one undo step; the Console says which, from what to what), and the scene is saved for it
if it had no other unsaved changes when play began (if it had, it is left marked changed, to be saved by hand).
At the bottom of the panel: **Keep what I tune** (on; kept between sessions; it says how many settings differ
from the saved scene) and **Back to saved** (every setting as it was when play began). The tube's own sliders are
not the game's, so they are remembered apart, between sessions, as they were last set by hand. Not kept: the
switches that are not on the Combat Feel (the sword taken up where it is, everyone tired, the camera's shake, the
weapon arm on joint motors), the speed, the sizes of attackers and targets. Not run in Unity: the editor's part
is only compiled here.

Each unit's label
shows its health and poise, the blow it is making and the body part it is aimed at, or that it is
blocking, stumbling, down or getting up, or the posture it is playing and what that leaves open.

**Physics Battler > Move Preview** plays any move against a dummy target. You pick the sizes, the
distance and the body part, then scrub through the move. The Scene view draws the striking part's
path and the aim point, and the window says whether the planner would choose the blow and how
close it passes. Edit a move asset and press Rebuild to see the change.

## First run: what to check

1. **The lineup in the scene.** Before pressing Play, the six units stand on their tiles in the
   Scene and Game views, armed and coloured. Drag a marker to another tile, or change its weapons.
2. **Closing in.** In Play, units walk toward the nearest enemy, feet stepping under them, and stop
   at striking distance (closer for small units).
3. **Blows.** Each blow winds up, lunges (the lead foot steps out, and the rear one follows on a
   long lunge) and comes down on the body part in its label, then recovers back to the tile.
   Low targets: the attacker sinks and bends. High targets: it reaches up.
4. **Missing on purpose.** A blow at a unit that is itself lunging or swaying can land elsewhere or
   miss. That's the design. The log says what was there at contact.
5. **The guard.** The sword points forward and up, and the shield is held in front on the left.
   Both move with the idle without jitter.

Milestone 3:

6. **Hits.** The weapon stops where it meets the body, both units freeze for a few frames, then the
   weapon bounces back. The struck body is thrown back from the blow (hips pushed, upper body bent
   and twisted, the head snapped if struck), springs back, and stumbles a step or two. Health drops
   in the label.
7. **What gets hit.** Arms held in guard often take blows meant for the head or chest: that's the
   guard working. A unit standing beside or in front of the target can take the blow instead.
8. **Heavy against light.** A large unit's club staggers a small one (knocked well back, its own
   blow cancelled); a small unit's club stops dead on a large one, which barely moves.
9. **Blocks.** The medium units raise their shields at blows coming at them about half the time;
   a blocked blow stops on the shield.
10. **Deaths.** At zero health a unit goes limp and lies there; the others stop fighting it.
11. **No sharing space.** Units crowding one target stand around it, never inside each other.
12. **Press B** for the bystander test.

Milestone 4:

13. **Stumbles.** A blow that breaks poise, or a heavy one, sends the unit stumbling back along the
    blow, leaning with it and arms out, catching itself with two to four quick steps. It ends up
    somewhere else and turns back to the fight.
14. **Knock-downs.** A large unit's club knocks medium units down and launches small ones. The body
    goes limp with the blow's speed and topples or flies, then lies still.
15. **Bodies hurt.** Press **L**: the small unit should fly into the unit beside it, which loses
    health and stumbles or falls in turn (the log's `impact` lines).
16. **Get-ups.** Every unit down gets up after a second or two: face down by pushing up onto hands
    and knees and one knee, face up by sitting up and pushing into a crouch. It should start from
    how it lay without a pop, and end in its stance. Then it walks to the middle of a free tile.
17. **Fight Rhythms.** Press **H**: each unit plays its captured idle (the large one rocks in a bladed boxing
    stance, fists up, the bare hand's wrist straight; the medium one stands side-on behind its shield; the small
    one low, hands low) and its postures: the large one feints like a boxer, the medium one
    circles behind its shield, the small one fans its stick and steps to angles. In a fight, the
    boxer feints just before most of its swings. Tick "Everyone tired" in the panel: the postures
    thin out and the guards drop.
18. **Captured walks.** Watch units walk in to fight and back onto a tile after getting up: the big one lumbers
    with its arms swinging round the guard, the medium one walks upright, the small one sneaks low. The feet
    shouldn't slide or cross, and when they stop they settle back into their stance.
19. **Severing.** Press **K**: the medium swordsman cuts at an enemy's elbow, and the forearm (with whatever its
    hand held) should fall away from the elbow, the label showing the cut and the bleeding. In a fight, watch for
    arms and lower legs coming off. A unit that loses its weapon arm stops attacking, one that loses a leg stays
    down, and both bleed out.
20. **The fight loop.** Press Play and watch: each unit picks a target, walks tile to tile to stand next to it
    (never two on a tile), fights, and the fight ends on its own with a result on screen. Units lying on the
    board block their tile; units knocked off their tile walk back onto a free one.

If something looks off, press **M** at the moment and mention the mark number.

## Playtest logs

Code review of 11 November 2026 (the puppet, the stand-in physics, and a plan for a physical upper body):
https://claude.ai/artifact/5YY9GpcDGQRLVyZPRJjcsP. Its figures come from `python3 Tools/Playtest/puppet.py
PlaytestLogs/<session>`: how far each physical part is from its pose, and how often two living bodies overlap,
measured on a real Unity log.

Every Play session is recorded into `PlaytestLogs/<date_time>/` (next to `Assets/`, so Unity doesn't import it):
what happened and why (`events.log`: spawns, steps, blows (where each was aimed, how close it passed and what was
there), hits (who struck whom where, how hard, the reaction, whether the swing stopped), blows cut short, deaths,
marks, console warnings), a summary (`session.md`), per-step data for every unit
(pelvis height, feet, balance, idle motion, authority per body region, every part's distance and angle from its pose,
the blow under way with its aim point and striking part) and the full pose of the physical body, the animation pose
and the weapon. It also saves small square pictures of
the game view (300×300, 8 per second of game time) into `frames/`. The panel shows the session being recorded;
*New log* starts a fresh folder; logs close when Play stops.

To share one: stop Play, then commit and push the session folder.
`python3 Tools/Playtest/ground.py PlaytestLogs/<session>` and `feet.py` measure how the feet meet the ground in a
session and whether the body is balanced over them (a foot planted, sliding, low in the air or stepping; how long
swings last; the centre of gravity against the feet's support). They are the measures of the plan "Grounded legs"
(https://claude.ai/artifact/P8Du2bu8f7hN8xMaVSY5xP, 12 November 2026: research and plan only, nothing built).

`python3 Tools/Playtest/rolls.py PlaytestLogs/<session>/units` lists each time an upper arm kept turning fast and
how much of it was about the arm's own length (a straight arm rolling); `elbows.py`, each step an elbow jumped with
its hand hardly moving; `shake.py`, how long each part shook (its place going back and forth step by step), body and pose;
`armrange.py`, each arm read as a body's arm and how long it was past a person's range.

`python3 Tools/Playtest/analyze.py PlaytestLogs` analyses the newest session: a report with flags (sliding feet,
weight off the feet, pinned parts lagging, shaking, blows that miss, friendly fire), a table of every blow and every
hit, each unit's stumbles, knock-downs and get-ups (flagging any that never got up), an overview and a timeline per unit (with health and poise; stumbling, down and getting up shaded), film strips of hits, stick-figure film strips of each unit's idle, its turns, its blows (attacker and target, seen from
the side) and every mark, and the pictures in captioned sheets (needs numpy, pandas, matplotlib
and Pillow).

## How it works

Each unit has two rigs built from the same null tree. The **anim rig** is posed in code every physics step; it is the
authority over where the body should be. The **physical body** (one rigidbody per part) follows it with an
**authority level** per region: *Kinematic* (moved exactly; the legs), *Pinned* (dynamic, driven by velocity toward
the pose with a strength 0–1; torso, head, arms), *Loose* and *Ragdoll* (knocked down, launched, dead).

The pose is built in order: stance (planted feet, steps) → combat idle → upper body (hip turn, lean, chest
counter-turn, level head, guard) → the blow's body (twist, lean, sink toward low targets) → weight shift over the
support point, or the blow's lunge → pelvis lowered only if a foot can't be reached → two-bone leg IK → feet flat →
arms: the weapon in guard, or on the blow's path (two-bone arm IK, the spine bending if the arm can't reach), the
shield in guard → the head looks at the aim point.

A **move** (`MoveDefinition`) is a swing or a thrust described by a few numbers: the swing plane's tilt, the arc's
wind-up and follow-through angles, how far it carries past the aim point, the pivot, the phase timings, and the
body's turn, lean, crouch and footwork (`MoveTimeline` works out the phases). For each blow, `SwingPath` poses the
attacker at contact to find the pivot, then lays the striking part's path out in the world (the arc, or the thrust's
line) through the aim point, the pivot moving with the body. The old hand-drawn path in an aim frame is kept as the
Path shape for custom moves. The **planner** (`MovePlanner`) locks the aim, works out the lunge and keeps only blows
that land and stay clear of the attacker's own body.

**Hits** (`UnitCombat`, `DamageRules`): each physics step of a blow's active window, the striking length of the
physical weapon (most of a blade, a club's head end) is swept from last step's position as a capsule. Body parts and
shields it overlaps are struck in the order the blade meets them (nearest the hand first, since the blade turns about
the hand); between steps a swing's weapon is swept round its arc, not along the chord. A body too heavy to cleave
stops the blow, which is wound back to the start of that stretch; a lighter one is shoved aside and the swing slows. `DamageRules.Resolve`
turns a contact into damage, poise damage, an impulse and a reaction. The struck unit's recoil springs (hips, upper
body, head) are kicked by the blow and feed the pose (`PosePipeline.React*`). The struck part and the part it hangs
from take some impulse physically (at most 5 m/s on their own) while held loosely, and the rest becomes knock-back
that slides the unit's root while its feet step after it. Units keep apart by pushing their roots out of each other's
body radius, split by mass.

**Balance** (`UnitBalance`, milestone 4): a unit is `Standing`, `Stumbling`, `Down` or `GettingUp`. `Knock` turns a
push into a slide, a stumble, a knock-down or a launch. Stumbling, the root is carried at the stumble's speed and
the stance's feet hurry after it with urgent short steps (`Stance.Drift`, `Stance.Urgency`), while the pose leans
with it and throws the arms out (`PosePipeline.Topple`, `Flail`). Down, every region is ragdoll and the root follows
the pelvis; fast parts are checked against other units' parts for body impacts. Getting up, `GetUp` poses the anim
rig (key poses by IK, blended from the captured ragdoll pose and into the standing pose; limbs are blended by where
their joints are and solved again so an elbow never swings round the wrong way) while the pins come back up.

| Folder (`Assets/_Battler/Scripts/Runtime`) | What lives there |
| --- | --- |
| `Rig/` | `PartId` (the 16 parts and their tree), `RigFile` (the body rig JSON), `UnitBuilder`, `UnitRig`, `AnimRig`, `BodyPart` |
| `Motion/` | `CombatStyle` (tuning), `BodyMeasure`, `Stance`, `CombatIdle`, `PosePipeline`, `ArmRig` (every arm), `GroundClamp`, `TwoBoneIK` (legs), `GetUp` (get-up key poses, `PoseBuffer`), `RhythmDefinition`, `PostureDefinition`, `RhythmPlayer` (Fight Rhythms) |
| `Authority/` | `AuthorityController` (levels per region, pinning, the weapon arm's joint motors) |
| `Combat/` | `WeaponDefinition`, `MoveDefinition`, `MoveState`, `MovePlanner`, `ComboDefinition` (the strings), `Weapon` (the weapon in the hand), `AimPart`, `DamageRules`, `BodyClearance` |
| `Units/` | `UnitController` (the per-step loop, blows, the lunge's steps), `UnitCombo` (combos), `UnitCombat` (health, poise, the weapon sweep, reactions, blocks), `UnitBalance` (stumbles, knock-downs, launches, body impacts, get-ups), `UnitRhythm` (the Fight Rhythm on the unit: steps, travel, feints, lead-ins), `UnitProfile` |
| `Board/` | `BoardGrid` (tiles), `BoardView` (the board mesh) |
| `Sandbox/` | `BattleSandbox` (spawning, panel, keys), `UnitSpawn` (the scene's markers), `Brawl` (close in and strike), `OrbitCamera`, `SandboxInput` |
| `Diagnostics/` | `PlaytestRecorder`, `UnitTrack`, `FrameCapture`, `ShieldContacts` |
| `Muscles/` | `JointMath`, `JointConventions` (joint limit conventions), `SupportPolygon` |

Editor setup lives in `Scripts/Editor/Setup`. Tests (EditMode, NUnit) live in `Assets/_Battler/Tests/EditMode`.

## Tuning

Tune in the data assets, not on the prefabs (setup rebuilds prefabs). Changes made in Play mode persist.

- `Data/Styles/CS_Small`, `CS_Medium`, `CS_Large` (combat styles): stance width, length, crouch, lead foot, toe-out;
  hip turn, chest counter-turn, lean; guard arm angles; idle sway, bob and weave (amplitude and frequency); step
  threshold, turn threshold, step duration and height, idle shuffle interval and distance. Lengths are fractions of
  the unit's leg length, so one style fits any size.
- The combat styles also set the sandbox fighting: `attackInterval`, `engageDistance` (chest to chest at medium
  size) and `walkSpeed`; and blocking: `blockChance` and `reactionTime`.
- `Data/Units/UP_*` (unit profiles): `maxHealth`, `maxPoise`, `poiseRegen`.
- `Data/Weapons/WD_*` (weapons): `damage` (at 15 m/s to the chest), `minHitSpeed`, `sweepRadius` (the striking part's
  thickness for hit detection). A move's `hitStop` sets the base hit stop.
- `Data/Moves/MD_*` (moves, Swing and Thrust shapes): the swing plane's tilt, forehand or backhand, the wind-up and
  follow-through angles, `through` (how far past the aim point it carries), the pivot, the wind-up radius and the
  wrist's lag; the timing (wind-up, hold, strike, recovery; the follow-through follows from the speed); the body's
  turn back and through, lean, crouch, rock-back, passing step and drive; `maxLunge`, which body parts it may aim at.
  Edit while the game runs; the next blow uses the changes. Press G to see them all.
- `Data/Weapons/WD_*` (weapons): the guard (where the grip is held and which way it points) and the weapon's moves.
  Its shape (length, mass, strike points) comes from `weapons.json` and is refreshed by setup.
- `Data/Units/UP_Small`, `UP_Medium`, `UP_Large` (unit profiles): which body, which style, turn speed, how strongly
  the torso and arms are pinned to the pose (`torsoPin`, `armPin`), and the rigidbody settings.
- `Data/Rhythms/RD_Boxing`, `RD_ShieldAndSword`, `RD_Eskrima` (Fight Rhythms): the base (a captured `clip`, with
  `strength` for how much of its motion plays; or the bounce rate and
  height, sway, the weapon's loop, both hands' guards, circling), the postures and how often they come
  (`gap`), the lead-in chance, and the mood (starting aggression, what each posture, blow and hit costs in
  energy, regeneration, how far the guard sags). `Data/Rhythms/Postures/PD_*`: each posture's length, weight,
  key poses (hips, crouch, lean, roll, turn, head, both hands and how the weapon turns, travel), planned
  steps, a weapon feint (a move's name and how far into its wind-up), openings, and its usual follow-up.
  Edit while the game runs; H shows them. A combat style's `rhythm` picks which one a body uses.
- The fight loop (milestone 6): each combat style's `readsOpenings` and `openingLead` (and its `walkSpeed`,
  `attackInterval`, `engageDistance`); the time limit is `FightLoop.TimeLimit`.
- Severing (milestone 5): each weapon's `severSpeed`. In code at the bottom of `Combat/DamageRules.cs`: the
  damage each joint takes to cut (`SeverDamage`), the wound (`SeverLoss`) and the bleeding (`BleedRate`). In
  `Units/UnitSever.cs`: how close to the joint (`SeverReach`) and how square to the bone (`SeverAlong`).
- Balance (milestone 4) is tuned in code for now, at the top of `Units/UnitBalance.cs`: the pushes that make a unit
  stumble, fall or fly (`StumbleAt`, `KnockdownAt`, `LaunchAt`), how much each catch step takes (`CatchStep`), the
  stumble's lean, the launch's speed and lift, how long a body stays down, and body impacts (`ImpactSpeed`,
  `ImpactDamage`). `UnitCombat.CleaveShove` sets how hard a swing that cleaves through a body throws it. The get-up's
  key poses are in `Motion/GetUp.cs`.

## Art

The three base bodies come from `SourceArt/Bodies/Scripts/build_bodies.py` (Blender 5.2): a simple mannequin of
lathed parts with visible ball joints. It also writes `Assets/_Battler/Data/Rigs/<body>_rig.json` (nulls, aim
points, masses, colliders). See `SourceArt/Bodies/README.md`. The sword, club and shield come from
`SourceArt/Weapons/Scripts/build_weapons.py`, which writes `weapons.json` (strike points, colliders); see
`SourceArt/Weapons/README.md`.

## Compile check and tests without Unity

**The real engine, headless (branch `physical-body`, 11 November 2026).** `Tools/CompileCheck/get-physx.sh` fetches
Cysharp's MagicPhysX (.NET bindings for NVIDIA PhysX 5, MIT; kept out of the repository in `ThirdParty/`). With it
there, `Tests.csproj` builds `Runner/PhysX/PhysXWorld.cs`: every stub Rigidbody, Collider and ConfigurableJoint
mirrored into PhysX as Unity sets them up (a ConfigurableJoint is a PhysX D6 joint, its own body first), each game
step simulated in one or more physics steps, the result written back, and collisions told to the scripts
(`OnCollisionEnter`, `OnCollisionStay`). Without it everything builds and runs as before, on the dry run.

```
./get-physx.sh && dotnet build Tests.csproj
dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll physx                 # the engine's checks (pass/fail)
PHYSX=1 FIGHT_LINEUP=mediums dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll fight /tmp/fight 1
python3 ../Playtest/puppet.py /tmp/fight/PlaytestLogs/<session>       # the same measures as on a Unity log
```

`PHYSX_SUBSTEPS=4` runs four physics steps in each game step (240 a second). `FIGHT_LINEUP=mediums` is the new
plan's battle: six Mediums. How well it stands in for Unity, measured:

- The game's own joint probe reads on it what it reads in Unity: "drive standard (asked +35, got 35.0), X limit sign
  -1 (pushed +, stopped at 5.0), velocity signs -1 -1 -1" (the turn 24 to 25 degrees; Unity 25).
- A 60 s fight of physical clip fighters against the playtest of 6 November 14:16, how far each physical part is from
  its pose (cm; median, 95%, 99%): torso and head 0.14, 1.05, 2.33 (Unity 0.14, 1.12, 2.87); shield arm 0.51, 7.81,
  15.8 (Unity 0.62, 9.70, 17.1); weapon arm 0.00, 2.48, 5.34 (Unity 0.00, 1.97, 6.22); legs exactly on it in both.
- A ragdoll's joints come 2 cm apart at worst in a fall; Unity's dead came 2 to 12 cm apart in the playtests.
- The same seed repeats exactly, with the engine and without it.

Not yet in it: contact modification (`GroundContacts`), raycasts, limit springs, colliders' own materials. Unity
stays the final word.

**What passes through what (step 1's baseline).** With `PHYSX=1`, `fight` prints, for the living units on their feet,
how often and how deep a blade is in another's body, in a shield, through another blade or in its own body, a shield
in a body and a body in a body (the engine's own overlap query each step; headless a sword is a thin capsule its
length, a shield its board). Today's build, 60 s of six Mediums (three seeds): a blade over 2 cm into another's body
90 to 116 times (3 to 4 s, deepest 11 to 13 cm); over 1 cm into a shield 77 to 107 times (deepest 19 to 26 cm: through
the board); into its own body 43 to 66 times (deepest 10 cm); a shield into a body about 90 to 100 times (deepest 13 to
17 cm). `FIGHT_LINEUP=duel`: one on one.

**One physical sword arm (step 2, the experiment so far).** `physx-arm`: the Medium's strikes with the weapon arm held
several ways, swung at nothing, at a fixed post and at a free 20 kg ball (`ARM_SETUP`, `ARM_ONLY`, `ARM_CLIPS`,
`ARM_TRACE=1`, `ARM_HZ`, `ARM_FORCE`). `ClipFighter.StrikeArm` and `TorsoHold` set how the arm and the torso are held
(as they have been by default); `AuthorityController.MusclesByInertia` sizes each joint's motor for what hangs from it.
What it found:

- The clips' blades are faster than an arm: about contact the posed point moves at up to 67 m/s for two frames (the
  hand at 17), three times a real cut. Today's kinematic arm, held to 15 m/s and 1800 degrees a second, is itself 64 to
  100 cm behind its pose for those frames.
- The motors as they were (accelerations, boosted by the load) can't hold a sword: PhysX sizes them for the lighter of
  a joint's two bodies, the forearm, a sixtieth of what a hand with a sword takes. Sized by inertia they follow a slow
  pose within 5 to 15 cm at the point and are back on it within 7 cm after a blow; about contact they are 40 to 50 cm
  behind.
- A motor on the arm turns a torso held by its pins (up to 84 degrees): the arm needs something that can't be turned
  to push against.
- Contact is right at 240 physics steps a second: the blade never goes more than 0.5 cm into the post (5 cm at 60 Hz;
  today's passes through it), and leaves the ball at 0.2 to 0.7 m/s where today's kinematic arm launches it at 9.
- No blow-ups in any of it.

**The sword arm as an articulation, and a limit on a blade's speed (step 2, the second build).** The same experiment
with the arm built the other way the plan named: `Runner/PhysX/ArticulatedArm.cs`, a PhysX articulation (reduced
coordinates: the joints can't come apart, and its motors are solved with the chain, not after it). Chest, upper arm,
forearm, hand with a solid 1.2 kg sword (balanced 18 cm past the grip, as a sword is, not halfway along its blade);
ball joints, each motor an acceleration drive toward the posed joint's turn (its turn vector) with the pose's speed fed
forward; the chest locked to a body the pose carries. `physx-arm` now ends with it (`ART_HZ` the motors' frequency,
default 80; `ART_FORCE` the shoulder's most torque, default 600 N m, the elbow 0.7 and the wrist 0.25 of it).

What had to be found out to build it (for the Unity side later):

- A ball joint's position is the child's turn against its parent as a turn vector (axis times angle, radians).
- A part's inertia must be given in the part's own frame. PhysX works out a capsule's in a frame turned along the
  capsule; with that, a ball joint settles up to 18 degrees off its target. (`ART_TURNED_MASS=1` shows it.)
- Each joint needs a little armature (0.02 kg m2): without it a blade, which has next to no inertia about its own
  length, rings on a stiff motor (the hand at 1 to 4 m/s standing still).
- A motor's limit is an impulse a step unless the articulation is told its limits are forces
  (`DriveLimitsAreForces`), as Unity's are.

Where the 67 m/s comes from: the clips themselves. In `sns_forehand` the hand turns 83 degrees between two frames
(2500 degrees a second) with the wrist at 13 m/s; the wrist's steer onto the target adds the rest. To follow that with
this sword a wrist needs some hundreds of N m; a person's turns with about 25.

So a strike's blade is given a speed limit: Combat Feel > A blow's speed > `mostBladeSpeed` (m/s; 0, the default:
off, the clips at their own pace, and the fight's log is the same line for line). With a limit set, `LocoBody` turns
the weapon hand back where the pose would swing the blade's point further than the limit allows in a step
(`LimitBlade`), and slows the strike's clip by what the pose wanted (`BladePace`, through `Locomotor.ShotPace`), so
the body waits for its blade. At 30 m/s a blow lands 0.05 s later than its clip's (0.07 at most); the posed point
never goes faster than 30.

The eight strikes, each at nothing, at a post and at a 20 kg ball, 240 physics steps a second (tip: how far the
physical blade's point is from the posed one):

| | tip in the wind-up, at most | tip through the blow, at most (median) | tip at contact | deepest in the post | ball left at |
|---|---|---|---|---|---|
| joints with motors, limit 30 | 81 cm | 77 cm (65) | 53 cm | 0.2 cm | 0.3 m/s |
| articulation 600 N m, no limit | 8 cm | 91 cm (55) | 36 cm | 7.2 cm | 0.9 m/s |
| articulation 600 N m, limit 30 | 8 cm | 44 cm (28) | 10 cm | 0.1 cm | 0.9 m/s |
| articulation, motors without limit, limit 30 | 7 cm | 19 cm (15) | 9 cm | 0.1 cm | 2.9 m/s |
| articulation 600 N m, limit 30, 120 steps a second | 14 cm | 45 cm (32) | 13 cm | 7.7 cm | 0.7 m/s |

The verdict: the articulation, with the limit. It holds a slow pose within 8 cm at the point of a 90 cm blade, meets
its target within 10 cm, stops on the post and gives the ball a real blow's push, with no blow-ups; the joints with
motors are 40 to 80 cm off even in the wind-up. For the two or three steps of a blow's fastest part the blade is up to
44 cm behind its pose: an arm of that strength can't be there sooner, and it is back on the pose after. Without the
limit the blade also goes too fast for the engine to catch (7 cm into the post). 240 steps a second are needed (at 120:
8 cm into the post). Motors without a limit follow closest, but fight what they meet (the hand at 37 m/s after the
post, the ball launched at up to 7 m/s): strength is what makes contact right.

Tried and dropped: slowing the clip by how far the physical blade is behind its pose (a closed loop). The pose jumps
a metre in a step before the arm has fallen behind, and the arm then overshoots: worse than no limit.

With `FIGHT_BLADE=30` the headless clip fight (posed, the old way) plays with the limit: 97 blows where there were
109, parries 12 where there were 27. The defender's reading of when a blow lands doesn't know about the slowed clip
yet; that is for the step that brings the arm into the fight.

**A hit on a physical body (step 3, the experiment).** `physx-hit`: a Medium stands in its guard, its legs the
pose's, its upper body (pelvis to head, both arms) one articulation on muscles (`Runner/PhysX/ArticulatedBody.cs`),
and is struck by a ball of a known weight at the chest, the head, the weapon arm and the back (`HIT_SETUP`, `HIT_ONLY`,
`HIT_TRACE=1`; `HIT_TORSO`, `HIT_NECK`, `HIT_ARMS`, `HIT_STRENGTH`, `HIT_LOOSE`, `HIT_LEGS=1` with "as set"). A blade's
blow gave the 20 kg ball of step 2 about 18 N s, so 3 kg at 8 m/s (24 N s) stands for a sword's blow, 6 kg at 10 m/s
for a heavy one and 20 kg at 8 m/s (150 N s) for a Large's club. `HIT_FILM=<folder>` writes where every part is each
step, and `python3 Tools/Playtest/hitfilm.py <folder> out.mp4 <blow> [setup ...]` draws it as a film, a quarter speed.

What it found:

- The engine's own "acceleration" motors size themselves for the one part below a joint, not for all that hangs from
  it: a spine asked for 6 Hz let the body sag 63 degrees under its own weight. `ArticulatedBody` sizes each motor by
  the inertia of everything that hangs from its joint (the spine's 5 kg m2, a hundred times the spine's own): the
  body then stands within half a degree and 1 cm of its pose at 6 Hz. (So the sword arm's "80 Hz" in step 2 was a
  name, not what it swung at: its motors were softer than their numbers. Its measures stand as measured.)
- Today's body swallows the blow. A sword's blow moves the chest 1.5 cm for four frames; the club's 150 N s moves it
  9 cm and it is back in a quarter of a second, its feet where they were. The pins write the parts back onto the pose
  and the blow's momentum is gone.
- The upper body on muscles is steady: no blow-up up to 150 N s, no swinging back and forth (the motors are damped
  just enough not to), back on its pose within 0.1 to 0.2 s. And it is stiff, as a braced body is: a sword's blow
  moves the struck part about 1 cm, and 16 of its 18 N s arrive at the pelvis. A torso soft enough to fold for that
  blow (under half a hertz) could not follow its own animation.
- So what a blow does to a body is mostly what it does to its feet. Given to the walk as a change of speed (the
  blow's weight over the body's mass: `Locomotor.Push`, which the walk already answers with steps), a sword's blow
  takes the body 11 cm back at 0.22 m/s, a heavy one 29 cm at 0.57 m/s, the club 90 cm at 1.5 m/s (in the fight:
  off its feet).
- Muscles let go to a quarter of their quickness for 0.12 s when struck: the arms and the head are thrown two to
  three times as far (a blow to the head: the head 2.5 cm, the hands 10 cm), still without swinging back.
- The struck part goes the blow's way (73 to 99% of its move in eight blows of nine), but it is not the part that
  moves most: the free ends are, the hands.

| 3 kg at 8 m/s at the chest | head moved | hands moved | back on its pose | its feet took it |
|---|---|---|---|---|
| today's body | 2.3 cm | 1.5 cm | 0.07 s | 0 |
| muscles, torso 6 Hz | 0.5 cm | 1.5 cm | at once | 0 |
| the same, and the legs take the blow | 1.1 cm | 4.1 cm | 1.4 s (walking) | 11 cm |
| the same, muscles let go when struck | 1.7 cm | 5.2 cm | 1.4 s (walking) | 11 cm |

The verdict: the physical upper body is sound, and it shows that a real-sized reaction to a sword's blow is small.
What makes a hit read is the legs taking it (step 4), the muscles letting go for a moment, and what is layered on
top as today (hit stop, the hit clips), with one honest dial if more is wanted: a blow's weight scaled up. Not in
this yet: the blow from the physical sword itself (the ball stands in), the shield, the body's own parts stopping
each other, a blow to the legs, and anything in Unity.

**Feet that don't slide (step 4).** `fight` now measures it: each foot's ankle while its motion says it is down and
it is at its standing height, and how far it moves there ("feet sliding on the ground": centimetres a second, the
worst single slide; `FIGHT_SLIDES=1` lists each slide over 5 cm with what was playing, `FIGHT_SLIDE_TRACE=1` every
sliding step), and how far roots were moved at once instead of walked ("roots moved at once": two bodies kept apart,
ground given to a blow, a combo's strike snapped onto its target). Six Mediums, three fights of 60 s:

| | worst single slide | bodies carried in by combos' strikes | blows met |
|---|---|---|---|
| before | 18, 10, 55 cm | 24 m | 252 |
| now | 2, 2, 3 cm | 24 m | 252 |

What the slides were: a combo's strike carried its body in after its target at up to 7.2 m/s (12 cm a step); a
planted foot was let go when its leg could no longer reach, and then followed its place under the body along the
ground, 55 cm in a sixth of a second at the worst. Two changes, both Combat Feel fields under "Feet":

- `comboPullSpeed` (m/s; 3, it was 7.2): how fast a combo's strike may carry the body in. At 3 the body is carried
  as far in all and as many blows are met; at 2 it is carried 40% less far.
- `movingFootLift` (cm; 7; 0: off): a foot moving over the ground is lifted, as a stepping foot is, from 0.4 m/s
  up (`LocoBody.LiftMoving`). A foot is then either held where it stands or in the air.

On the real engine (`PHYSX=1`, 30 s) the worst was 10 cm, once, in a walk; the rest 1 to 2 cm. The slide measure
counts a foot within 1.5 cm of its standing height: a foot lifted over that is a step, however quick. Whether those
quick steps read well is for the eye. The fights are no longer the same line for line as before this step (a
foot's height moves the hips, and with them everything).

Still as they were: ground given to a blow (the body moved 4 m/s for a tenth of a second, then pushed into a step)
and two bodies kept apart (moved off each other at once). Together about 1 m a minute over six units, and with the
lift they no longer drag a foot. A blow's own push already goes to the walk as a change of speed (`Locomotor.Push`),
as step 3 found it should.

**The old pose pipeline goes (branch `physical-body`, begun 11 November 2026).** Every unit is now a clip fighter:
`UnitController` always builds one, and the `ClipMotion` switch, the sandbox's "Clip motion" toggle and `CLIP_FIGHT`
are gone. What went with them so far:

- The old path's stepping, equipping and teleporting, and the 30 places that chose between it and the clip fighter
  (`FightLoop`, `UnitSever`, `UnitTrack`, the recorder, the sounds).
- The 18 EditMode test files (157 tests, all of the old path) and the 26 headless reports that drove only the old
  path (`smoke`, `aim`, `swings`, `walks`, `rhythms`, `balance`, `parries`, `shield`, `arms`, `guard`, ...). There are
  no unit tests at the moment; the checks are the ones listed below. (Unity will note that the test assembly in
  `Assets/_Battler/Tests/EditMode` has no scripts: the next tests go there.)
- The Move Preview window, and the spawn marker's posed preview: a marker now shows its body at rest in its team's
  colour, unarmed (the stance and the weapons in the hands were the old pipeline's pose).
- About 280 members nothing reaches any more, found by the compiler: everything removable in `Units/`, `Motion/` and
  `Combat/` was taken out at once, and only what the three builds then missed was put back.

The game's runtime code is 41,900 lines where it was 46,400, and about 10,700 lines of tests and reports are gone.
After each stage the clip fight's event logs were compared with the ones from before, on eight fights (three seeds,
two line-ups, posed and on the real engine): the same line for line.

What is left of the old pipeline is still reached by live code, so it can't be taken out by rule. Where it hangs
(names of the old types counted in the live code): `Stance` (the clip fighter's ground height and the logs),
`MoveState` and `AimPart` (the sandbox's scenarios, the recorder), `Tempo` (the fight loop), `Perception`, `GetUp`
(the clip fighter's get-up uses it), `MovePlanner` (the sandbox), `DamageRules`, `Weapon` and `WeaponDefinition`
(live: kept), and `MoveDefinition`, `RhythmDefinition`, `PostureDefinition`, `CombatStyle` with their 37 assets and the
editor's set-up menus that build them. `PosePipeline` (2,300 lines) hangs by two lines in `UnitController`. Each
of these is a cut to make by hand, with the logs compared after it.

**The physical upper body in the fight (step 5, behind a switch).** With Combat Feel > The physical upper body >
`physicalUpperBody` on (off by default; it takes effect at the next Spawn), a clip fighter's hips, torso, head and
both arms are one jointed body (`UpperBodyArticulation`, on Unity's `ArticulationBody`), on legs the animation still
moves:

- Its hips are carried where the pose's are (locked to a body that is moved there); every other joint has a motor
  driven to the pose's turn, sized by what hangs from it (a quickness in Hz, so one setting holds a head and a whole
  arm alike): torso, neck and shield arm 30 Hz, the weapon arm 40.
- Each muscle has a strength, what it holds against the world with: 600 N m at a Medium's shoulders and spine
  (`muscleStrength`), 0.7 of it at an elbow, a quarter at a wrist and the neck, and as much more for a body as it
  is heavier to turn (its weight times its size squared: a Large has 5.4 times a Medium's). On top of it each is
  allowed what the pose's own motion takes (`Allow`): the torque that turns what hangs from the joint as fast as
  the pose turns it, that carries it as the hips are carried and turned, and what the joints further out are
  allowed. So the strength sets how hard a stopped blade pushes and how firmly a body stands, not how fast a sword
  can be swung.
- A joint's motor is a damper alone, asked for a speed and no place (`UpperBodyArticulation.Ask`): the pose's own
  speed of turn, and on top of it what brings the joint to where it is wanted in a step. It takes off whatever
  speed the joint has beyond that with all the muscle has and no more. There is no spring in it anywhere.
- The arms have slack. Where a joint is wanted is its pose and its slack, how far aside of the pose it has been
  left. Pushed more than 8° from there (`giveWayAngle`; a little more for a joint the pose is turning fast) it
  gives way: where it is becomes where it is wanted. So a blow that turns an arm 15° leaves it 15° aside, doing
  what the animation does that much off. The slack comes back out over a second while it is under 15°
  (`slackAngle`, `slackTime`), quicker the further aside, in 0.3 s from 40° on (`tautAngle`, `tautTime`), and is
  never more than 50°: past that the joint is held with all its muscle has. The spine, chest and head give way
  too but are held (`tautTime` at any angle, 20° at most). An arm is held as well while its slack would leave
  its forearm, hand, sword or shield 3 cm inside its own trunk or head, or a joint 8° further past its range than
  the animation has it (`ClipFighter.HoldOrSlack`). A wrist with a shield strapped on gives way to nothing.
- No joint may turn faster than 90 rad/s (the fastest clip turns one at 44), a part found inside another is pushed
  out of it at 3 m/s at most, and the parts weigh nothing on their joints (a muscle holds its limb up unasked).
- The sword's blade is a solid on the hand. The shield is a part of its own on a ball joint at the grip, driven to
  where the pose turns it in the hand.
- The game steps the physics itself, four times for each of its own steps (`PhysicsSteps`; in Unity the
  `PhysicsStepper` the sandbox adds), each motor's target moved a quarter of the way each time and named the way
  nearest where its joint is. A strike's blade is limited to 30 m/s unless `mostBladeSpeed` says otherwise.
- The rig's own upper parts follow it as shadows (triggers, moved onto it after each step), so everything that
  reads a part's place, and everything drawn, follows the physical body. Hits are still decided as before, by the
  pose's swept blade: that is step 6.
- A part cut off, or a change of what is held, builds the body again without it; a body that falls lets it go and
  falls as the rig's own parts; it is built again when the unit is back on its feet.

What passes through what, three fights of 60 s on the real engine (`PHYSX=1`, and `FIGHT_PHYSICAL=1` for the physical
body), living units on their feet, the three fights together. Six Mediums (four with shields):

| | held to the pose (as today) | physical upper body |
|---|---|---|
| a blade in another's body (over 2 cm) | 195 times, 7.2 s, to 12.7 cm | 16 times, 0.4 s, to 12.4 cm |
| a blade in a shield (over 1 cm) | 177 times, 9.1 s, to 25 cm | 15 times, 0.4 s, to 12.4 cm |
| a blade through a blade (over 0.5 cm) | 33 times, 0.7 s, to 2.4 cm | twice, 0.1 s, to 1.6 cm |
| a shield in another's body (over 2 cm) | 172 times, 12.9 s, to 16.6 cm | twice, 0.1 s, to 4.4 cm |
| a body in another's body (over 2 cm) | 24 times, 1.2 s, to 7.3 cm | 16 times, 0.9 s, to 7.7 cm |

The scene's own line-up, a Swordsman, a Medium with a shield and a Large with a club a side:

| | held to the pose (as today) | physical upper body |
|---|---|---|
| a blade in another's body | 456 times, 16.0 s, to 20.3 cm | 6 times, 0.1 s, to 8.8 cm |
| a blade in a shield | 160 times, 6.4 s, to 21.7 cm | 6 times, 0.4 s, to 4.0 cm |
| a blade through a blade | 63 times, 1.4 s, to 2.6 cm | once, under 0.05 s, 0.5 cm |
| a shield in another's body | 175 times, 10.5 s, to 18.2 cm | 3 times, under 0.1 s, to 6.1 cm |
| a body in another's body | 84 times, 1.9 s, to 12.4 cm | 33 times, 2.7 s, to 12.3 cm |

(The physical columns are from after the third look in Unity, below, with the arms' slack. 32 of the 33 bodies in
bodies are one fight's legs, a Medium's foot in a Large's thigh: the legs are the animation's still.)

How far the physical body is from the pose (units on their feet, both line-ups): the head within 1.2 to 1.8 cm
95% of the time and 3 to 7 cm 99% of it; the weapon hand in a strike within 1.5 to 1.9 cm half the time and 9 to
17 cm 95% of it; outside a strike within 3 to 6 cm 95% of the time; the shield hand within 6 to 13 cm 95% of the
time; the shield in its hand within 5° to 7° of the pose 95% of the time. These are no longer how well a muscle
keeps up: an arm that has been knocked aside is meant to be aside. Touching nothing (`UPPER_GHOST=1`) the weapon
hand is within 1.4 cm half the time in a strike and 7.5 cm 95% of it, the head within 1 cm 95% of the time. At
the worst, for moments: a head 17 to 83 cm off, a hand 0.5 to 1.5 m. In a minute's fight of six a joint gave way
170 to 1,900 times, an arm was held for being in its own body 700 to 2,500 steps and for a joint past its range
700 to 1,400. No body was borne down or had to be put back on its pose in these six fights.

**The second look in Unity** (11 November, evening: "still happens, even between two Mediums"). With all of the
above in, the bodies were calm in Unity until something met them (a shield pressed on a leg, a parry, a blow) and
then whirled, joints at 150 to 185 rad/s for seconds. Headless the same fights stayed whole, so it was looked for
in Unity's log, and then in the tails of the headless figures, where it had been all along in small: a shield hand
2 m from its pose for a moment, a wrist swinging 70° to and fro about its forearm for a quarter of a second with
nothing touching it.

- *A motor at its limit does not damp, and that was the whirl.* A Medium's shoulder has a spring of 96,500 N m a
  radian and a strength of 600 N m: it is at its limit a third of a degree from its pose. Past that the motor gives
  its strength toward the pose and no more, whatever the damper asked, so a joint knocked off its pose is thrown
  at it, passes it, and is thrown back. Unity's log shows it to the number: a head knocked 20° out by a parry
  (A Medium 2, 7.13 s) nods 20°, 25°, back 20°, 38°, forward 37°, 44°, a step each, ten times a second, which is
  what a neck of 150 N m does with a head when nothing damps it; a quarter of a second on it goes right round, 100°
  and more a step, and takes the arms and the spine with it. And a joint that has once gone round goes on: its
  target is named the way nearest where it is, so it is braked for half of each turn and driven for the other.
  The allowance above had mended this only for what the pose itself does. Now a joint off its pose is asked for a
  speed, as said above (`Ask`), and the damper works whatever the spring is doing. Tested by throwing the bodies
  out of hand (`FIGHT_KICK="3,150"`: every joint of every body set turning at 150 rad/s): before, the fastest
  joint was still turning at 360 rad/s in the second tenth of a second after and 118 in the third, and one body had
  to be put back on its pose; now 26 rad/s in the second tenth.
  Why Unity's engine kept the swing growing where the headless one let it die in a third of a second was not
  found then. It was on the third look, below: the two engines do not write a joint's turn alike.
- *How fast the pose turns a joint* was taken from the change of its turn's three numbers. Kept continuous, a
  shoulder's turn came to be written as 310° about one axis where it was 50° about the other way, and numbers
  that far out move several times as fast as the turn does, and another way: the speed its motor was told, and
  what it was allowed over its strength, were five times too much. Now from the turn itself (`TurnBetween`).
- *The turn back to a pose is a turn too.* The first build of the above went after the difference of the numbers:
  a shield turned half round in its hand then went round its pose and not to it, 180° off for a third of a second
  (found headless, in the 1% tail of "the shield in its hand"). And a joint's speed is counted on the part's own
  axes, not its parent's: given on the parent's, the weapon hand was 20 cm from its pose half the time.
- *Limits said outright.* No joint turns faster than 90 rad/s (it was 200, and the whirl ran at it). A part found
  inside another is pushed out at 3 m/s at most (`MostPushOut`): it was left to the engine, and in Unity's log
  the legs were met by a shield at 24, 37 and 98 m/s as it was thrown out from between them.
- *A body out of hand is put back sooner.* As well as a hand or the head far off for half a second (counted down
  now when it is back, not dropped: a body thrown about passes its pose again and again), any joint turning
  faster than 60 rad/s for a fifth of a second in all; and it is put back still, not turning as it was.
- *More is measured at start-up* (`ArticulationConventions`, in the log's first lines): that a motor's target
  speed is taken in degrees a second, that its damping is for radians a second, and that a speed is counted on the
  part's own axes. Where Unity reads otherwise the body is given what Unity takes.
- *The log says more.* A body out of hand (a joint over 60 rad/s, the head 25 cm or a hand 60 cm off its pose) is
  written to the playtest log as it stands, a few times a second at most: `[Upper]` lines, each joint's target,
  where it is, how fast it turns, its spring and its limit. And the scene's Combat Feel is saved with the session
  (`config/Combat_Feel.json`), as the profiles and styles are.

The two Larges' spawn markers in `Battle.unity` are switched off (inactive, not removed): four Mediums fight.

**The third look in Unity** (12 November: "much better", the bodies no longer went wild, but "a little spinning of
limbs at points", and the arms "feel like springs": they wobble after a blow where they should stay knocked aside
a while and come back slowly). The log (75 s, four Mediums) has both. 161 times a joint kept turning more than 25°
a step for four steps or more with its pose near still, the sword wrists most (B Medium's 59 times); from 13.0 s
B Medium's sword hand swung 80° to and fro, four times a second, for three seconds; and the bodies were put back
on their poses 1 to 4 times each.

- *Unity's engine writes a turn otherwise, and its spring then pushes the wrong way.* The new `[Upper]` lines give
  each joint's position as the engine has it. The headless engine keeps a ball joint's turn under 180°; Unity's
  writes it anywhere from 0 to 360 (the same turn, 300° one way where it is 60° the other): 67 of 159 lines for
  the sword wrist, 30 for the shield shoulder. The parts' recorded turns agree with those numbers (2° to 3°), so
  they are read right. But a turn's three numbers past 180° move the other way to the turn itself, sideways of
  its axis, five times as far at 300°; and the engine's spring pulls each number toward its target's. It pulled
  the joint away from where it was sent. Of the 146 lines with a joint over 60 rad/s, that joint was written past
  180° in 85. This is what the headless fights never showed, and why the first build whirled in Unity alone.
  The motors have no spring now (stiffness 0, the place sent is the place the joint is): nothing works on the
  engine's numbers any more, only on speeds, which the probe measures in Unity as they are headless.
- *Slack*, as asked for: see the list above. A blow leaves an arm aside; it comes back over a second or two, with
  nothing to swing it past.
- *A speed alone does not hold a pose.* The first build of this asked each joint for its pose's speed and a slow
  drift back, and no more. Touching nothing, the sword hand was then 8 cm from its pose half the time in a
  strike: the engine does not give a chain of joints quite the speeds they are asked (an upper arm slipped 15°
  about its own length in each windup; four times the solver's steps took it to 4°, a hundred times the muscle's
  strength changed nothing). So a joint is also brought to where it is wanted, in a step, by the speed it is
  asked: 1.4 cm.
- *Measured against where the pose has it now.* A joint was first measured against where the pose has it by the
  physics step's end: one the pose turns 40° in a step was 10° "behind" at each quarter of it and gave way.
- *A wrist with a shield on it.* The shield's grip has the arm's whole strength, four times a wrist's, and pushes
  back on the hand with it each time the pose turns the board in the fist: the shield wrists gave way 257 times
  in 20 s with nothing touching them. A wrist bears its grip's strength now and gives way to nothing.
- *The damper is as strong about every axis as about the heaviest*, and three times what a spring of the motor's
  quickness would have: it has no spring to be matched to.
- *Not done:* the legs are the animation's, so an arm's slack is the only give in a body. The old give of a
  stopped arm (`blockedArmStrength`) is still on beside it.

**The limbs that spun** (12 November: the third look's "a little spinning of limbs at points" was still in the
look after it, the slack's, "for sure an improvement"). That last log (09:38, four Mediums) has two kinds, and
both are the pose's: the physical arm only goes where the pose puts it.

- *A straight arm rolled about its own length.* 7 times an arm with its elbow near straight turned 240° to 550°
  about its own length in 4 to 11 steps, its hand turning as far the other way
  (`python3 Tools/Playtest/rolls.py PlaytestLogs/<session>/units`). Which way a straight arm is bent is read from
  a few millimetres of elbow, and two things moved those. A weapon at rest on a body or a shield has its hand put
  out of it each step (`LocoBody.RestNow`); the elbow's side was taken from where the elbow lay against the line
  to the hand's new place, which for a straight arm is mostly the push itself: the arm was rolled by up to half a
  turn, and back by the next step's pose. It is kept on the side it is on now (`TwoBoneIK.BendWay`). And each
  step's arm was solved from wherever the last step had left it, after the rest, a blow's bend or a knock had
  moved it: bent that way and its elbow twisted back to its pole, a straight arm rolled up to 78° a step for as
  long as the weapon lay there. An arm is solved from where its own solve last left it now (`LocoBody.Arm`).
- *The shield arm's elbow jumped.* 33 times its elbow moved over 20 cm in a step with the hand hardly moving
  (`python3 Tools/Playtest/elbows.py PlaytestLogs/<session>/units`): in the step a shield began to move to a blow
  the elbow was put behind the board at once, in the step it was let go it was put back, and each time the arm was
  solved again to clear its own chest it jumped out to the side. The elbow goes over as the hand does now, with
  the shield's weight, and turns about the line from the shoulder to the hand at most 720° a second
  (`ClipFighter.ShieldElbowTurn`).

Headless, 60 s of six on the real engine, before and after (seeds 1 and 2):

| | Mediums, before | after | With the Larges, before | after |
| --- | --- | --- | --- | --- |
| An upper arm turning over 25° a step for 4 steps | 32, 19 | 1, 5 | 23, 18 | 3, 3 |
| of those, about its own length | 16, 9 | 0, 2 | 12, 10 | 1, 2 |
| An elbow jumping over 20 cm, the hand under 12 | 77, 53 | 9, 7 | 34, 36 | 4, 10 |
| Joints put where the pose turned them at once | 376, 148 | 33, 46 | 238, 180 | 36, 62 |
| Steps in which a joint turned over 45° | 651, 309 | 128, 196 | 493, 366 | 207, 236 |
| A blade in another's body | 10, 3 | never | 1, 1 | never, 1 |

The strikes' aim is as it was to the centimetre (`strikes`: 0.07 m median), and the combos' and the dummy's
reports differ by a centimetre or a degree in a few lines.

**The Larges are back** in `Battle.unity` (they were taken out on 11 November while the whirl was looked for).
Headless a Large's head is within 2 cm of its pose 99% of the time and never more than 6 cm, and nothing whirls.
Unity has not been seen with them since.

Still there: a sword wrist turns 120° to 240° in 4 to 7 steps some 14 times a minute (the clips' own flicks, or
the hand's turn between two grips: not looked into); a swordsman's parrying elbow jumps when its hand is drawn in
close to the shoulder (47 cm a step, three steps running, in `sns_turn180`); once a shield arm was folded 177°.
A blow's bend takes a knee the same way `RestNow` took the elbow (`ClipFighter.Bend`): for the legs' own plan.

**The look with the Larges back** (12 November, 13:06, six units, 75 s: "some vibrating and shakiness"). The two
fixes above hold in Unity: no straight arm rolled, and the shield elbows jumped 7 times where they had 33. What
shook was one thing, and again the pose's: `python3 Tools/Playtest/shake.py PlaytestLogs/<session>/units` counts
how long each part's place against the pelvis goes back and forth step by step, for the body and for the pose.
Of the 450 unit-seconds the weapon hand shook for 59 and its elbow for 51, the pose shaking with them all but 5 of
those; every other part together 11. A Large's weapon hand shook 20 and 27 of its 75 s, a swordsman's 10: the
hand turned 19° to and fro and moved 4 cm with it, twenty to thirty times a second, under a club that stayed on
its line.

- *A roll the hand is allowed was taken for a turn it had failed to make* (`SwordParry.SolveArm`). The parry's arm
  asks its hand no further ahead than the turn the hand really has. The arm rig lets a hand roll about its grip
  when the wrist can't take a turn; that roll was counted as falling short, the next ask pulled back to the
  rolled hand, there the wrist took it unrolled, the ask went on to where it rolled again, and round, for as long
  as a guard the wrist can't quite take was held. The turn the hand has is taken without its roll now.

| Headless, 60 s of six, seeds 1 and 2 | With the Larges, before | after | Mediums, before | after |
| --- | --- | --- | --- | --- |
| The weapon hands shaking (unit-seconds of 360) | 35, 35 | 2.9, 1.4 | 24, 4.2 | 2.3, 1.0 |
| Their elbows | 27, 28 | 4.1, 1.7 | 18, 6.1 | 3.9, 2.2 |

The dummy's report: its sword parried 21 of 27 blows where it had 22 (a forehand at the chest glanced off low
quarte), its blade turned back on itself 62 times where it had 225, and was pushed out of the attacker 108 times
where it had 277; the combos' report is unchanged. Not looked into: which of a club's two edges a guard is held on
is chosen in the step the guard is taken up, sometimes straight out of the unit's own strike, and a poor choice
is kept for as long as the guard is held (seen for 12 s). A tenth of the time a Large's club lies 45° to 80° off
its guard's line, before this fix and after it.

**Heavy weapons, a first step** (12 November: "they are playing with polystyrene props", "the enemy stops the
club in its tracks and the club will slide and bounce from the head over the neck and then arm", "the weapon is
the steady one, the thing it hits gives way; exaggerate it, the sword too"). What the fights measured, with a new
figure in the fight report (`Tools/CompileCheck/Runner/HeftMeasure.cs`, "the weapons' weight"): of each blow that
met something, the weapon's speed along its own way once the hit stop is over, and how far the part struck and
the hips of the body struck went the blow's way. Before, a club that landed on a body at 16 m/s was at a
standstill or coming back 0.05 s after the hit stop (it came back in 11 of 21 blows), and a sword's blow moved the
hips of the body it landed on 3 cm in half a second. Every one of the 94 hits in the Unity log reads "the swing stopped":
a club was stopped by any body (a blunt blow "never cleaves"), a sword by any body its wielder's own weight; then
the hand was thrown back along the blow and the clip backed out and faded to the guard.

Three dials on the scene's Combat Feel, under "Heavy weapons", all exaggerated on purpose:

- *`swingKept`, 0.7.* A blow that lands on a body is not stopped by it and does not back out: it goes on along its
  arc at this share of its speed, picking up again over a third of a second. It strikes each body once (the rest
  of that body, its shield and its sword are not met again by the same blow) and goes on to what lies beyond with
  what is left. A shield, a parry and a friend still stop it; stopped by a shield its hand is thrown back, and
  its clip backs out, only by the share it would not have kept (0.3).
- *`weaponWeight`, 3.* What a body struck takes is this many times the weapon's own momentum: the bend, the lean,
  the reaction, the drive back, the stumble. Not the harm, the poise, or whether a guard is knocked open or a
  parry beaten. A shield gives as much further and its bearer is driven back as much harder. A combo's blows
  before its last drive the body back by their own weight (thrown three times as far, 7 of the dummy's 42 combo
  blows missed), and its last throws it back with all of it.
- *`strikeArmStrength`, 3.* Through a strike and 0.4 s after its contact the physical weapon arm (both, two-handed)
  is this many times stronger, has no slack and does not give to what it is held off by: the weapon pushes on.
  Stopped by a shield or a parry it gives as before.

| Headless, 60 s of six, seeds 1 and 2 | Before | After |
| --- | --- | --- |
| A club on a body: the weapon's speed along its way 0.05 s after the hit stop | -4%, -19% | 69%, 53% |
| it came back off the body | 8 of 16, 3 of 5 | 0 of 10, 0 of 7 |
| the part struck went, in 0.1 s | 38, 46 cm | 57, 65 cm |
| the hips of the body struck went, in 0.5 s | 48, 23 cm | 108, 65 cm |
| A sword on a body (six Mediums): it came back off the body | 13 of 26, 11 of 22 | 3 of 15, 5 of 15 |
| the part struck went, in 0.1 s | 17, 12 cm | 31, 20 cm |
| the hips went, in 0.5 s | 21, 2 cm | 17, 6 cm |
| A sword on a shield: the shield hand went, in 0.1 s | 12, 18 cm | 21, 28 cm |
| its bearer's hips went, in 0.5 s | 5, 9 cm | 17, 26 cm |
| A club on a shield (seed 2): its bearer's hips went, in 0.5 s | 25 cm | 62 cm |

`FIGHT_HEFT="1,0,1"` runs a fight as before (the weight, the share kept, the arm's strength; three more numbers
set `mostDrive`, `mostStumble`, `mostBend`). The strikes' aim and the dummy's combos are unchanged (42 of 42
land).

Not there yet, and known: a sword's blow still moves the body it lands on little (most land on a hand or a
forearm, which take four fifths of a blow themselves, or on a body in its own strike, which plays no reaction);
raising `mostDrive`, `mostStumble` and `mostBend` changed the medians little. A sword that is parried now stays
on the parrying blade and drives its bearer back further than one that lands (68 and 38 cm in half a second; 43
and 9 before). The hit stop still holds the weapon still for 0.04 to 0.16 s as it lands: shortened with the share
kept, the physical weapon was held up by a body that had not yet moved while its swing went on without it (a
twentieth of a strike's time over 31 cm behind, where it is 12), so it is as it was. The weapons'
own weight in the hand is as it was (a sword 1.3 kg, a club 2 kg). None of this has been seen in Unity.

**Arms that bend as a body does: the measure, and a plan** (12 November, the target test: "the shield arm often
bends in strange ways", "the shield can still go into the head", "we still need a robust system that prevents the
arms from bending in ways that the body does not"). Nothing in play has changed; this is what was found, and the
plan is the doc "Arms that bend as a body does, and the shield: findings and plan"
(https://claude.ai/artifact/PwoViLF1TYzcBSx7yQ1QrK).

Where the limits are today: the limit table (`JointLimitTable`) is put only on the old ragdoll joints, so it acts
only once a unit has fallen; the standing physical body (`UpperBodyArticulation`) leaves every joint free on all
three axes; the pose solves its arms with `TwoBoneIK` from a hand's place and a rough way for the elbow, with no
limit (the shield, the knocks, the strike's aim, the swordsman's tucked free arm); only the sword arm as it parries
(`ArmRig`) has limits, its own. And the table is in each bone's own frame, where a bone's roll about its length is
whatever the solver left: the same arm to look at can read as inside it or outside.

So an arm is now read as a body's arm, from where its joints are (`Tools/CompileCheck/Runner/ArmAnatomy.cs`): where
the elbow is against the chest (how high the upper arm is raised, and toward where: all the way up in front and out
to the side, 55 degrees straight back, 30 to 40 across the body), which way the elbow folds (the upper arm's turn
about its length, 100 in and 90 out), the elbow's bend (to 150), the forearm's roll (90 each way) and the wrist (80
to the palm or the back of the hand, 30 to the thumb, 45 to the little finger). The fight report prints, for each
kind of unit, each arm and each moment (guarding, striking, just struck), the share of the time the pose, the
physical body and the recorded clip itself are over 10 degrees past that, and each unit's shield or shield arm
inside its own head and trunk; `python3 Tools/Playtest/armrange.py PlaytestLogs/<session>/units` reads the same
from a playtest (the body in full; the pose's elbow, since the log has the pose's places and not its turns), with
the longest runs to find on the film; `dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll armrange` reads every frame
of every clip.

What it shows (two 60 s fights of six, the target test with Mediums and with Larges, and the three target-test
playtests of 16:22 to 16:49):

| Share of the time over 10 degrees past a person's range | In the game | The recorded clip alone |
| --- | --- | --- |
| A swordsman's free arm, in guard (its wrist) | 68 to 97% | 0% |
| A swordsman's sword arm, just struck (the elbow's place and fold) | 31 to 36% | 0 to 2% |
| The shield arm, just struck (the elbow folded the wrong way, up to 90 degrees past) | 22 to 38% | 0 to 3% |
| A shield man's sword arm, striking (the wrist, the forearm's roll) | 20% | 3 to 5% |
| A Large's club arm, striking | 17 to 19% | 14 to 15% |
| The shield arm, in guard | 5 to 7% | 0% |
| An arm nothing of ours moves (the target that only stands) | 0% | 0% |

The physical body is within a point or two of the pose in every row: it has no stops of its own. In the playtests
the shield target's arm is past a person's range 14 to 15% of the session, the free arms 31 to 96%, the attackers'
sword arms 10 to 24%. The shield or its arm is over 2 cm inside its own head 1 to 4% of the time (10 to 13 cm at
the deepest) and inside its own trunk 2 to 8% (17 to 24 cm). Over the 4,700 frames of the clips, the walks, turns
and reactions are never past the ranges and the combat clips 1 to 3% of the time, nearly all at the wrist (to the
thumb up to 68 to 76 degrees, where the range has 30): the recorded motion is a person's, so the ranges are to be
widened until all of it passes. The legs, against the old table only: feet and thighs past it 3 to 20% of the time;
their own reading is for the legs' work.

The plan, each step playtested: (2) the ranges set from the clips, and one gate at the end of the pose, after every
stage: the arm read as its seven numbers, each brought into its range, the arm built again from them, so what leaves
is an arm a body has; (3) the stages that ask for the most impossible arms (the free arm, the knocks) ask for
possible ones; (4) the shield held as a centre-grip shield is (the recorded guard already holds it so: the elbow
down and out, bent 90, the forearm at the enemy, the fist 30 cm before the chest): moved between wards that are
arms, the board fixed in the fist and turned by the wrist, kept out of its own head and trunk, a blow taken
through the elbow and the shoulder into the body; (5) the same ranges as hard stops in the physical joints (the
elbow a hinge); (6) the weapons' real weight; (7) the legs.

**The arm gate (the plan's step 2)** (12 November: "yes as you suggest", "you can go ahead"). After every stage of
the pose has had its say, and just before the pose goes to the physical body (`ClipFighter.Apply`, first thing),
each arm passes one gate (`Motion/ArmLimits.cs`: `ArmLimits`, the reading and the ranges; `ArmGate`, the gate, one
for each arm). The arm is read as a body's arm and each reading brought into its range, in this order, each by
turning the arm about one joint: where the elbow is (the whole arm about the shoulder, to the nearest place an
elbow can be), the elbow's bend (the forearm about the elbow), which way the elbow folds (the arm about the upper
arm's own length), the wrist (the hand, so the forearm lies in it as far to the palm and the thumb as a wrist
bends), the forearm's roll (the hand about the forearm). No bone changes its length and nothing is solved for: the
arm that leaves is made of turns that are each inside a range. What a stage asked for that no arm does, it does not
get: the hand stops short.

What the gate keeps to:

- An arm inside its range is not touched (200,000 arms at random: none moved by a tenth of a degree), so the clips
  and the walk are as they were.
- An arm may always be where the playing clip has its own (12 degrees of room about each reading of the clip's arm):
  the recorded motion is a person's. The ranges themselves are a person's, set from the clips: the fold 100 in and 95
  out, the bend to 150, the roll 95 in and 100 out, the wrist 80 to the palm or the back of the hand, 50 to the
  thumb, 55 to the little finger; against them the combat clips in use are over 10 degrees past in 0.5% of their frames,
  the walks, turns and reactions in none.
- The hand goes with the arm (its wrist bent as it was sent), and the forearm's roll then turns it back toward the
  way its thumb was sent, as far as a forearm rolls: an arm turned under its hand leaves what the hand holds facing
  as it was sent.
- It does not jump. A range that goes round (the fold, the roll, the wrist's bend) has a far side, where an arm sent
  is as near one end as the other; there the arm stays at the end it was at, and when it has to go to the other it
  goes through the range at the scene's Combat Feel's "Arm Goes Over At" (600 degrees a second, on top of however
  fast the arm sent moved itself), not across what is outside and not in a step. The same for the elbow's place.
  What it remembers of the step before it keeps as ways in the chest's frame, not as readings: a reading's zero
  swings round as an arm nears straight up or straight out. A range that closes on an arm (the fold's as an arm
  bends from straight or comes down from overhead, where the fold says nothing and its range is open all round) is
  come into at twice the pace: for those tenths of a second the arm is outside it.
- A hand bound to a two-handed weapon is left to its grip (none in the fights now).

The scene's Combat Feel, "Arms that bend as a body does": "Arms Held To A Body" (off: as before, to compare) and
"Arm Goes Over At". Headless: `FIGHT_ARMGATE=0`, and `FIGHT_ARMTRACE=1` writes each jump the gate itself made.
`dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll armgate` is its test: arms at random through it (the furthest past
a body's range after it 0.12 degrees), arms moved a degree a joint a step for 600,000 steps (its arm moved over 35
degrees in a step 6 times, the sent arm then deep outside every range at once), and an arm held still while the
clip's own arm wobbles.

Measured (60 s fights of six: four seeds with the gate; without it, two for the arms and four for the blows; the
target test, Mediums and Larges):

| Share of the time over 10 degrees past a body's range | The pose, before | The pose, with the gate | The physical body, with the gate |
| --- | --- | --- | --- |
| A swordsman's free arm, in guard | 85% | 0.0% | 0.0% |
| The shield arm, just struck | 24% | 0.6% | 10% |
| A swordsman's sword arm, just struck | 16% | 0.7% | 3.6% |
| A Large's club arm, just struck | 17% | 0.3% | 1.4% |
| A Large's club arm, striking | 13% | 0.0% | 0.4% |
| A shield man's sword arm, striking | 8% | 0.0% | 1.0% |
| The shield arm, in guard | 4.5% | 0.0% | 1.6% |

What is left in the pose is the arm on its way over. The physical body is still past a body's range where a blow
knocks it there or it trails a pose that swung over: it has no stops of its own yet (the plan's step 5); in the target
test the Large target's shield arm, just struck, is so 20% of the time. How often the gate has work: the swordsman's
free arm nearly always (its wrist: 87% of its guard), the shield arm just struck 35% of the time (23% from over 30
degrees past), the sword and club arms in a strike 15 to 17%: those are for the plan's steps 3 and 4.

The hands that stop short: of the blows started, 30% were blocked by a shield (37% before), 21% parried (18%), 47%
landed (38%) (250 and 289 blows). In the target test the shield target blocked 17 of the Mediums' blows where it
blocked 20, and 16 of the Larges' where it blocked 20. The shield and its arm are as often inside their own head
and trunk as before (the plan's step 4).

Also found: the stages themselves turn an arm's joint by over 60 degrees in a single step 36 to 66 times in a 60 s
fight of six (a pole flipped, a hand sent across the shoulder); the gate lets those through where they are from one
arm a body has to another, and makes 2 to 5 of its own (a hand held at the edge of the wrist's range, where a small
turn of the hand sent is a larger one along the edge). An upper arm kept turning fast about its own length
(`rolls.py`) 2, 3, 2 and 1 times with the gate and 0, 1, 5 and 1 without. Not seen in Unity yet.

Seen in Unity (12 November, 19:49, the target test, 169 s: "working pretty damn well", "have not seen true issues").
`armrange.py`, now against the game's own ranges, on that playtest and the one before the gate (16:49): the swordsmen's
free arms past a body's range 24 to 94% of the time before and 0.0 to 0.3% after, their sword arms 3 to 10% and 1 to
3%, the shield target's shield arm 14% and 5%.

**The shield held fast (the plan's step 4, first half)** (12 November: "the shield guy is blocking blows in inhuman
ways", "he would have his arm in a normal pose with the elbow pointing down", "it's the shield that's rotating out
of his grip", "the shield block anticipates the blow a bit too quick"). Three things, all on the scene's Combat
Feel under "Shield: how it moves to meet a blow":

- **Shield Fixed In Fist** (on). The board never turns in the hand: the pose's shield slot stays as the grip has it
  (`LocoBody.FaceShieldAt` only puts it back there), and the physical board is one piece with the hand, held as a
  blade is (`Upper.Hold`, not the grip joint that turned it), so no blow turns it in the fist either; that part
  changes when a unit is built (Reset). The board looks the way the back of the hand does, its bar through the fist
  along the thumb's line (`UnitController.ShieldGripFast`): the recorded guard holds its hand so, the back of it to
  the enemy, the thumb up, the forearm lying across behind the board. (First built with the face along the forearm,
  as the grip that turned had it at rest: "the shield is now pointing the wrong way by default", across the enemy's
  line or at the ground.) It is the arm that places the board (`ClipFighter.HoldShieldFast`): the arm as the guard's
  clip holds it is swung about the shoulder, as it is, until the fist points at where it is wanted, then bent or
  straightened in its own plane to reach it, so the elbow keeps its way against the arm (and is brought half way
  toward the lowest it can be, as an arm braced tucks it) and the board goes round with the arm, looking out from
  the shoulder: up over a fist raised, down from one lowered, to the side of one swung wide. The hand stays on the
  forearm as the clip has it, the wrist bending toward where the board should look by at most **Shield Wrist Turn**
  (15 degrees). (Tried first: the fist sent to the place with the elbow straight down and the hand rolled to face
  the blow; with the back of the hand as the board's face the forearm then pointed at the blow and the board met
  it edge on: 15% of blows blocked.) The fist stays in a box before the body: at least 26 cm before the chest, no
  higher than 8 cm over the base of the head, from 20 cm across the body's middle to 60 cm out on its own side (a
  Medium's).
- **Kept out of its owner.** A board the pose has within 6 cm of its own head, chest, spine or hips is moved off
  them, the fist with it, and the arm solved to there (`KeepShieldOff`): as the shield is raised, and again at the
  gate for whatever else moved the arm (a knock, its own strike).
- **Shield Reads From** (0.5). The shield does not move to meet a blow until that share of the blow's time from its
  start to its contact has passed: before that the blow is a wind-up.

Measured (the target test, Mediums on Mediums, 40 s; the fights of six, four seeds, against the same with the gate
alone):

| | Before | Now |
| --- | --- | --- |
| The board turned in the fist, on average (the most) | 71° (107°) | 0° (0°) |
| Where the board looks against the way its bearer faces, seen from above: in guard, just struck | 13°, 36° off | 25°, 33° off |
| The shield arm's elbow over 60° round from the lowest it could be: in guard, just struck | 15%, 46% of the time | 0.1%, 3.5% |
| Its elbow above its shoulder: in guard, just struck | 11%, 40% | 0.0%, 2.1% |
| The shield first moves, through the blow (the earliest) | 30% of the way (21%) | 51% (50%) |
| and before the blow's contact | 0.48 s | 0.22 to 0.30 s |
| The shield or its arm over 2 cm inside its own head, trunk (fights of six) | 0.3 to 2.2%, 2.1 to 5.8% | 0.7 to 2.2%, 0.5 to 1.8% |
| The Large target's shield inside its own trunk, just struck by Larges | 23% of the time | 0.2% |
| The physical shield arm past a body's range, just struck (fights of six) | 10% | 1.5% |
| The gate bringing the shield arm back, just struck | 35% of the time | 10% |
| Blows blocked by a shield, parried, landed (fights of six) | 30%, 21%, 47% | 33%, 22%, 47% |
| Blows the shield target blocked (target test, Mediums; Larges) | 17 of 54; 16 of 54 | 23 of 50; 14 of 50 |

Known: the posed board is clear of its owner (over a centimetre inside in 26 steps of four fights), but the physical
one can still be knocked into it by a blow, since its own body does not stop it: over 2 cm into its own head 4 to
9% of the time just after a blow in the target test. While a shield man strikes,
his shield arm is the strike clip's own (its elbow where the recorded one is). The blow is not yet taken through
the arm (the plan's step 4, second half). `FIGHT_SHIELD="0,15,0"` runs a fight with the shield as before (fixed or
not, the wrist's turn, the share read from). The fight report has "how the shield is held" and when it first moved.

The debug panel (the battle scene's): drawn 1.8 times the size, with the units' labels, and "Panel size" buttons
at its top to change that (kept between sessions), as the locomotion lab's has.

**Arms that don't snap (the plan's step 3)** (12 to 13 November, built overnight and checked headless only: not yet
seen in Unity). The gate kept an arm inside a body's range, but our own stages still threw one about inside it: a
parry's hand rolled over in a step (the parry's solver has no memory of the step before), a strike's wrist flicked,
an elbow straightened from 80 degrees in a step, a pole that flipped. Each of those went to the physical arm as a
joint turned 60 to 170 degrees in a sixtieth of a second, 60 to 70 times a fight; over 70 degrees the physical
joint was simply put there (some 40 times a fight), through whatever was in its way. Which stage did it is in the
fight report ("which stage of the pose first turned an arm's joint over 60 degrees in a step", `FIGHT_JUMPTRACE=1`
writes each one out): the parry about half, the strike's own arm a third, the shield's upper arm most of the rest.
Rather than a memory for each solver, the gate, which every arm goes through last, now also holds the speed. Three
things, each with its own switch on the scene's Combat Feel under "Arms that bend as a body does":

- **Arm Follows At Most** (2400 degrees a second; 0: off, as before). However far a stage threw the arm in a step,
  the gate follows it no faster than this (plus the gate's own pace of 600): the elbow's place, which way it folds,
  how far it is bent, the wrist, the forearm's roll, each on its own. A snap becomes a fast move over two or three
  steps (30 to 50 ms). The fastest recorded strike turns a joint 42 degrees in a step (2500 a second: an elbow
  straightening, a forearm rolling, a wrist snapping), and this lets that through. (First set to 1500: no joint then
  turned over 60 degrees in a step at all, but those three were held back a step or so in nearly every strike, and
  over four fights the attacks that hit anything fell from 77 to 68 in a hundred; with no limit, 78; at 2400, 81.
  The fight report counts what the gate held, "the gate held an arm's speed", the weapon arm in a strike apart.)
- **Arm Bones As A Body** (on). After the gate each arm's two bones are set the way a body's are for where its joints
  are (`ArmRig.SetBones`): the upper arm turned so the elbow's own axis lies across the arm, the forearm bent about
  that axis alone, the hand's turn kept. A limb is round, so nothing of this shows; what it stops is a solver's
  hidden roll of a bone going to the physical arm as a turn of its joints. An arm too straight to tell which way it
  folds keeps the way it folded the step before, and comes round to a new one no faster than the gate follows. With
  the forearm's roll now a body's, the hand's turn on it is held to the same speed (near the ends of a wrist's range
  the gate's own two limits together let a hand turn twice as fast). This is also what the physical elbow needs
  before it can be given a hinge's stops.
- **Free Hand Goes With Its Arm** (on). A swordsman's free arm is tucked behind him; its hand kept the turn its clip
  gave it for an arm held in front, which behind the back is a wrist bent further than a wrist goes, so the gate was
  turning it back 79 to 95% of the time he stood in guard. The hand now lies in line with its forearm, its back to
  his back (`SwordParry.TuckFreeArm`).

Measured (fights of six, 60 s, four seeds; the target test), against the same build with the three off
(`FIGHT_ARMSTEADY="0,0,0"`, which gives the fights of before to the blow):

| | Before | Now |
| --- | --- | --- |
| An arm's joint turned over 60° in a step, as sent to the physical body (a fight) | 59 to 73 times | 15, 9, 9, 4 |
| Physical joints put where the pose turned them at once (a fight) | 38 to 48 | 2, 2, 3, 1 |
| The same two in the target test (Mediums; Larges) | 45 and 33; 92 and 63 | 10 and 3; 20 and 7 |
| The gate turning the swordsman's free arm back, in guard | 79 to 95% of the time | 28 to 33% |
| and how far it turned that hand, 95% of the time under | 37° | 8° |
| Attacks that hit something (a body, a shield, a blade), of 317 and 318 | 77% | 81% |
| Blows blocked by a shield, parried, landed (fights of six) | 33%, 22%, 47% | 34%, 22%, 45% |
| Blows the shield target blocked, the sword target parried (target test, Mediums) | 31 of 31, 21 of 27 | 26 of 26, 22 of 28 |
| The same, Larges on Larges | 19 of 23, 13 of 26 | 15 of 21, 11 of 23 |
| The head off its pose, 99% of the time under (the four fights) | 5.4 to 7.8 cm | 4.7 to 9.6 cm |

("Now" is everything of this night on, as it is left; the tables of the stops and of the weapons' weight further
down were measured while the arm's speed was still held at 1500.)

The arm test (`armgate`) passes as it did. The fight report has, for each kind of unit, arm and state, how far the
gate moved the wrist from where the pose had it and how far it turned the hand: in a strike the weapon hand is moved
under 5 cm 95% of the time and 14 to 28 cm 1% of the time, before and now alike (that is the range, not the speed).

Not verified, for the first look in Unity: whether a parry or a shield now looks late (headless they block and
parry as often); whether the snaps that are left (a joint over 60 degrees in a step some ten times a fight, where
there were sixty) still show. Known: the swordsman's tucked
arm is still moved by the gate a fifth of the time (the place it is tucked to is at the edge of where an elbow
goes); a knock still poses a struck arm far outside the range for the gate to bring back (a struck sword arm a
third of the time, the wrist moved 40 cm): the knock itself is not yet shaped to the range. With the bones set as a
body's, the report's "in the pose as the stages left it" counts a stage's own roll of a bone against the bones the
gate set the step before, so it reads higher than it did; what reaches the body is the second number.

**Stops in the physical arm (the plan's step 5, as soft stops)** (13 November, built overnight and checked headless
only). The gate holds the animation's arm to a body's ranges; the physical arm had none of its own: struck, it gave
wherever it was pushed, up to 50 degrees aside of its pose, in range or not. Now the physical arm is read each step
as the pose's arm is (`ClipFighter.StopArm`: where the elbow is, which way it folds, its bend, the forearm's roll,
the wrist, against the same ranges with the clip's own room), and a joint over 2 degrees past its range is at its
stop (`UpperBodyArticulation.StopAt`): it is wanted at the end of its range nearest where it is, it does not give
way any further (inside its range an arm still gives and comes back as it did), and it is held there with **Arm
Stops Hold With** (4) times its muscle's whole strength, however much a blow has let its muscles go. On the scene's
Combat Feel: **Arms Have Stops** (on) and that strength.

These are stops made of the joints' own motors, not the engine's joint limits: a blow can still knock a joint past
its range for a step or two before the stop has it back. The engine's limits (an elbow that is a hinge in the
engine, as the plan had it) need the articulation probed in Unity for how it counts a limited joint, and the body's
"put a joint where the pose is" rewritten for joints with fewer than three turns; that is not something to build
blind. The pose side of it is done (the arm's bones are a body's, the elbow a hinge in the pose).

Measured (the stops off with `FIGHT_ARMSTOPS=0`, any other number the strength):

| | Stops off | Stops on |
| --- | --- | --- |
| Target test, Larges on Larges: a physical arm over 10° past its range | 73 times, 4.6 s in all, 0.27 s at the longest | 27 times, 1.5 s, 0.20 s |
| the arms just struck, over 10° past (over 30°) | 1.34% of the time (0.27%) | 0.50% (0.21%) |
| the arms in guard, over 10° past; the furthest | 0.37%; 77° | 0.02%; 28° |
| Blows the shield target blocked, the sword target parried | 16 of 25, 12 of 27 | 15 of 25, 11 of 25 |
| Fights of six (seeds 1, 2): the arms just struck, over 10° past | 2.28%, 1.27% | 0.74%, 0.77% |
| the arms in guard, over 10° past; the furthest | 0.16%, 0.22%; 84°, 77° | 0.05%, 0.06%; 24°, 40° |
| the arms striking, over 10° past | 0.47%, 0.83% | 0.27%, 0.57% |
| Blows blocked, parried, landed (fights of six, two seeds) | 42%, 20%, 30% | 29%, 19%, 43% |

Stops three times as strong (12) did no better (0.55% just struck, 1.9 s in all): what is left are spikes of a step
or two (half of them 0.02 to 0.03 s), up to 80 degrees in the reading of an arm nearly straight, where a few
centimetres of the elbow turn "which way it folds" by a right angle. The fight report has the lines ("the physical
arms, just struck", "a physical arm over 10 degrees past its range", "a physical arm at a stop"). Known: the board
knocked into its bearer's own head or trunk by a blow is not an arm past its range and is as it was (2 to 9% of the
time just after a blow in the target tests); the blocks, parries and landed blows of the two seeds differ by as much
as two fights do.

**Real weapon weight (the plan's step 6): built, and it does not do what the plan hoped** (13 November, overnight,
headless only). The plan: give the weapons their mass in the physical body, let the weight of a blow come from that
mass, and turn the script's multiplier ("Weapon Weight", 3) down. What is there now, on the scene's Combat Feel
under "Heavy weapons":

- **Weapons Have Their Mass** (on). What a fighter holds weighs in the physical body what it weighs at its bearer's
  size (weight goes with volume, as the damage rules already had it): a Large's club 3.8 kg where it was a
  Medium's 2, its shield 7.7 kg where it was 4. For a Medium nothing changes. Off: as before.
- **Weapon Mass** (1). A weapon that many times its real weight in the physical body, the arm that swings it as
  much stronger (`ClipFighter.MuscleOf`), the script's reactions as they were. Left at 1, for what follows.

Measured in the target test (60 s, one seed; each line the middle of 28 to 48 blows on the open target):

| Swords, Mediums on Mediums | Blows land at | The part struck goes, in 0.1 s | The hips, in 0.5 s | The weapon hand off its swing, 95% under | The sword parry held |
| --- | --- | --- | --- | --- | --- |
| As before | 11.8 m/s | 28 cm | 19 cm | 12 cm | 23 of 27 |
| Swords 3 times their weight, the arm as it was | 8.5 m/s | 22 cm | 11 cm | 32 cm | 21 of 27 |
| 3 times, the arm 3 times as strong | 11.0 m/s | 25 cm | 19 cm | 22 cm | 14 of 25 |
| 3 times, the arm 3 times as strong, the script's multiplier down to 1.7 | 6.9 m/s | 14 cm | 2 cm | 29 cm | 20 of 27 |
| 6 times, the arm as it was, the script's multiplier down to 1 | 11.8 m/s | 13 cm | -1 cm | 21 cm | 19 of 26 |

| Clubs, Larges on Larges | Blows land at | The part struck goes, in 0.1 s | The hips, in 0.5 s | The weapon hand off its swing, 95% under |
| --- | --- | --- | --- | --- |
| As before (the club 2 kg) | 12.9 m/s | 49 cm | 48 cm | 28 cm |
| Its real mass at a Large's size (3.8 kg): what is on now | 12.8 m/s | 51 cm | 45 cm | 22 cm |
| 3 times that, the arm 3 times as strong, the multiplier down to 1.8 | 13.2 m/s | 39 cm | 30 cm | 66 cm |

So: a heavier blade in the physics does not throw the body it lands on any further. What throws a struck body
(its bend, its lean, the reaction, the drive back) is the script acting on the pose; the physical blade meets a
physical upper body that is held to that pose by muscles of 600 N m and stands on legs that are the pose's own, so
its push is taken back within a step or two. Heavier, with the arm it has, the weapon trails its swing and lands
slower; with an arm as much stronger it lands as before by the script and beats a sword parry through the physics
(14 held of 25). With the script's multiplier turned down, as the plan had it, the blows are simply weaker. The
weight of a blow can only come from the weapon once the body it lands on is carried by its legs instead of placed
by the pose: that is the legs' work (the plan "Grounded legs", from "Supported hips"), not a dial. Until then the
script's "Weapon Weight" stays what makes a blow heavy.

`FIGHT_WMASS="3,1,1"` runs a fight with the weapons three times their weight (then: 1 the script's multiplier taken
down by as much, 0 as it was; then: 1 the arm as much stronger, 0 as it was); `FIGHT_WMASS=0` the weapons as
before. In fights of six (two seeds) with what is on now, against before: blocked 31%, parried 22%, landed 36%
(29%, 19%, 43%), no body borne down, no joint put.

**The legs' opinion (the plan "Grounded legs", step 0: see it)** (13 November, overnight). Nothing in play
changes: no foot, hip or step is moved by any of this. Each fighter now has a reading of what its legs make of
its body (`Locomotion/Grounding.cs`, read once a step from the body as the physics left it): what each foot is
doing, where the body's weight is and how fast it moves, the patch of ground its feet hold, and where a foot would
have to be to stop the body as it moves now. It is the reading `Tools/Playtest/ground.py` takes from a log, made
as the game runs (the same sole, the same thresholds: on one fight of Mediums the two give a foot planted 38 and
39% of the time, sliding 7 and 8%, the weight outside the support 19 and 20% of the time a fighter stands still).
Three things use it:

- **Show Legs Opinion** (the scene's Combat Feel, off): draws it on the ground under each fighter, over the game
  view (`Sandbox/GroundingView.cs`). Each sole's outline in the colour of what the foot is doing: green planted,
  red on the ground and sliding (over 10 cm/s), yellow just landing, orange in the air under 9 cm (too low to be a
  step), blue stepping. A white outline: the support. A dot under the body's weight: green over the support, red
  outside it. A cross where a foot is needed to stop the body: grey while that is on the support, orange when it is
  over 10 cm outside it.
- The playtest log: each unit's frames have `footL`, `footR` (0 planted, 1 sliding, 2 landing, 3 low, 4 stepping),
  `weightX`, `weightZ`, `weightH`, `neededX`, `neededZ`, and `weightOut`, `neededOut` (cm outside the support;
  -1 with both feet up).
- The fight report, "the legs' opinion": the plan's table, made by the report.

| The plan's table (fights of six, 60 s, seeds 1 and 2) | Unity logs (the plan) | Headless now |
| --- | --- | --- |
| A fighter is standing (not walking, not in a clip) | 2% of the time | 1%, 1% |
| A foot is planted (on the ground, not moving) | 30% | 27%, 28% |
| Both feet are on the ground | 6% | 4%, 3% |
| While walking, a foot is in the air | 85% | 97%, 97% |
| Swings lasting over 0.6 s | 40% of swings, 76% of the time feet are up | 50%, 51%; 80%, 81% |
| in them the foot goes, while the body goes | 60 cm, 34 cm | 70 and 43 cm, 71 and 44 cm |
| A foot on the ground and moving over 10 cm/s | 8% of the time, 2.8 m a minute each | 8%, 7%; 2.6, 2.3 m |
| Slides over 5 cm | 15 a minute each, the worst 42 cm | 14, 10; 52, 19 cm |
| On one foot, the body under 0.3 m/s: the weight outside that foot | 13 cm at the middle, a quarter over 28 cm | 15 and 32 cm, 15 and 26 cm |
| Still on both feet: the weight outside the support | 38% of the time | 23%, 26% |
| Both feet down in a clip: a foot needed over 10 cm outside the support | 40% of the time | 42%, 37% |

A foot's sole is measured from how high its ankle stands, and that is learnt as the fight goes (the height its
ankles are under one step in twenty, as the log tool takes it), not read from the rig: the clips stand a Large's
ankle at 10.3 cm where its rig has it at 12.8 at rest and its foot's box reaches 12.8 under it. By the rig's own
height a Large's feet counted as on the ground and sliding twice as often. **Found by this, and not changed: by
the log, the Larges' feet stand some 2.5 cm into the ground in a fight** (a Medium's 2 mm); to be looked at in
Unity. `python3 Tools/Playtest/legsview.py PlaytestLogs/<session> <time> out.png` draws one moment of a session
as the view does, each fighter from above.

And the legs read as a body's legs, as the arms are (`Motion/LegLimits.cs`; a measure, nothing is held to it): the
thigh's place against the hips, which way the knee folds, its bend, the ankle. The ranges are set from the clips,
as the arm's were (`legrange` reads every frame of every clip: the thigh to 125 degrees forward, 75 to the side, 55
behind, 30 across; the knee turned out to 85 and in to 50 from folding straight back; bent to 155, where the clips
reach 148; no clip is past them). In the fights the clips' own legs are never past, and the pose's are 1 to 2% of the time in a strike or a
reaction and 0.4 to 1% walking: a knee folding forward for a few frames of a combo (`sns_combo_forehand`: bent 40
to 60 degrees the wrong way), and ankles with the toes 30 to 70 degrees up where the foot is held level under a
shin that swings. That is what the legs' own gate will have to hold, when the legs are built.

Not verified: the view itself. It is drawn with the game's own on-screen drawing and its numbers are checked
against the log, but I have not seen it on a screen; if it draws nothing or in the wrong place, the log and the
report still hold.

**Stand and step (the plan "Grounded legs", step 1)** (13 November). In its guard a fighter now stands planted and
moves in short quick steps, with standing between. Three parts, all behind the Combat Feel's `standAndStep`
(off: the fighters walk exactly as they did; the same seeds give the same fights, blow for blow).

*The pacing* (`Locomotion/StepPacer.cs`, between the fight's steering and the walk). The steering asks each
fighter for a velocity, and near an enemy that is 0.3 to 0.8 m/s nearly all the time (closing on its distance,
backing off while it gathers, going round). Walked as asked, that was a walk in slow motion. Paced, the fighter
stands while what is asked adds up; at a step's worth (`stepAfter`, 0.22 m for a Medium) it goes that way at a
step's pace (`stepPace`, 0.75 m/s, or as fast as it is asked if that is faster) until the steering stops asking
that way or the ground asked for is covered; then it stands again. What is asked while it stands is forgotten over
a second and a half, so a nudge never makes a step. A velocity as fast as a walk (0.9 m/s for a Medium) is walked
as it is, and so is a shove.

*The step itself: in the stance* (`stepsInTheStance`; `Locomotor.StepsInStance`). The guard's standing clip is a
wide, deep stance (a Medium's feet 43 cm apart and 73 cm fore and aft, the left ahead). The guard's walking
clips are not: they are upright walks with the feet under the hips. Stepped with those, every step drew the
stance in and every stop stepped out into it again, the feet 35 to 40 cm from where the stance has them each time
it stood. So a paced step does not use the walk at all. The body goes, standing as the guard stands, and the feet
step after it one at a time to where the stance has them: the settling steps the body already had for finding its
stance after a strike, made to follow one another as each lands (`LocoBody.StepGap`, 0.24 s), the foot ahead the
way it goes first, and put down ahead of its place by a fifth of a second of the body's going (`footLandsAhead`)
so that it stands as much ahead of its place as behind it. A step takes a quarter of a second; one foot is always
down. Coming out of a walk or a shove, once it is slower than a walk it goes into the stance as it goes. With
`stepsInTheStance` off the step is walked with the guard's walk instead, at 1.6 times its recorded rhythm
(`walkedStepRhythm`) with the strides that much shorter, to compare.

*The distance held* (`FightLoop.RingHeldWithin`, `holdsItsDistanceWithin`). At its striking distance a fighter
holds it until its enemy is 18 cm nearer or further, then steps all the way back to it. A blow is thrown from up
to 35 cm out already, so this costs none. It turned out to matter little: with it or without, the fighters step
about as much, because what keeps them moving is not small corrections (see below).

Four fights of six, 60 s each, the same seeds with it off and on:

| | off (as before) | on |
|---|---|---|
| Standing still | 0 to 1% of the time | 9 to 15% |
| Stepping in the stance | none | 23 to 33% |
| Walking (the walk's animation) | 61 to 67% | 17 to 23% |
| In a clip (a strike, a reaction, a turn) | 32 to 39% | 34 to 46% |
| Both feet on the ground | 3 to 4% | 9 to 15% |
| A foot planted | 26 to 30% | 35 to 38% |
| A swing (foot off the ground until it is on it again), at the middle | 0.62 to 0.72 s | 0.27 s |
| Swings over 0.45 s, walking or standing | 76 to 79% of them | 28 to 33% |
| Swings over 1 s | 19 to 27% | 6 to 8% |
| A foot on the ground and sliding | 7 to 8% of the time, 2.4 to 2.9 m a minute | 3 to 4%, 1.1 to 1.6 m |
| Slides over 5 cm | 11 to 14 a minute for each fighter | 5 to 8 |
| On one foot, the body slow: the weight outside that foot | 14 to 19 cm | 27 to 31 cm |
| Blows started; blows that met something | 256; 259 | 268; 242 |

Against the plan's measures. Standing time and both-feet time are up. "No swing over 0.45 s" is not met: the
steps in the stance are a quarter of a second (13 to 18% of them counted longer, mostly a walk's last swing
ending as it stands), but a fifth of the time is still the walk's own animation, at its recorded 0.5 to 0.7 s a
swing: walking off a shove (11 to 13% of the time, at 0.9 to 1 m/s; a body kept off another is a shove) and
walking as fast as it is asked (5 to 9%). Those are the plan's steps 2 and 4. Blows met are within the 10% (7%
fewer), but one blow in ten now meets nothing where nearly every one met a body, a shield or a blade. I have not
found why; the likeliest is that a fighter backing off now gets away in a quick step where it used to drift, and
the strikes thrown as it goes fall short. Strikes that find nothing in reach before they start are up too (54 to
75 over the four fights).

Two things this step shows and leaves alone:

- **The fighters still step most of the time they are in guard**, and that is the fight asking, not the legs. In
  guard with no clip playing the steering asks nothing 3 to 13% of the time. Near its distance and free to move,
  a fighter is closing on it half the time (a third of a metre out on average, asked 0.6 m/s) and backing off from
  an enemy too near a third of the time (a fighter gathering holds out of its enemy's reach while that enemy
  closes: they chase); it is asked nothing a seventh to a quarter of the time. A fighter's root travels 27 m a
  minute (35 before). If they are to stand more than this, it is the tempo's distances and the circling that
  would have to ask less.
- **On one foot the weight is nowhere near that foot** (27 to 31 cm outside it, 14 to 19 before): the stance is
  wide and the hips stay in its middle while a foot steps. That is the plan's step 3 (the weight shifts before a
  foot lifts).

Found on the way, in the walk, and fixed for when a slow step is walked (these apply with `standAndStep` on):
strides shortened to almost nothing squashed the foot itself along the way it went, toe drawn in under the ankle,
so the foot read from them turned sideways and its toe dragged (sliding went from 8 to 13% of the time before
this was found; a foot is now moved as one piece); strides were shortened about the hip, which drew a staggered
stance in with them (now about the foot's own place in the cycle); a walk stepping quickly has both feet down for
a fortieth of a second a step and stepped on past it while the body's last drift died (it stays on both feet once
it is as good as stopped); and a foot held where the walk stopped was held as it stopped, heel up or toes up (it
stands flat).

The playtest log's frames have two more columns: `stepping` (0 standing still, 1 stepping in the stance, 2 a step
walked, 3 walking as asked, 4 walking off a shove, 5 a clip, 6 down) and `owed` (the ground asked for and not yet stepped,
cm). `python3 Tools/Playtest/steps.py PlaytestLogs/<session> 10 30 out.png` draws each fighter's feet over time
with what it was doing, to see the rhythm: stand, step, stand. The fight report's legs lines have the split of
standing and walking by what it was, each stand out of a walk, and the swings by what was playing. For a run:
`FIGHT_STEPS=0` (off), `FIGHT_STEPS="0.22,0.75,0.12,1,0.18"` (the step's worth, its pace, how quickly the body
picks it up, in the stance or walked, the distance held), `FIGHT_PACETRACE="A Medium,10,20"` (each step of one
fighter's pacing and feet), `FIGHT_PACEHIST=1` (what the steering asks, by speed), `FIGHT_SLIDETRACE=1` (each slide
over 5 cm with what the foot's lock was doing), `FIGHT_STANDTRACE=1` (each stand out of a walk, the feet against
the stance); `gaits` prints each walking clip's speed, stride and swing times.

Not verified: how any of it looks. Headless I can count feet and seconds, not see whether a body going at the
guard's height with its feet stepping under it reads as a fencer's step or as sliding on rails, whether the drop
from the upright walk into the stance after a shove is too abrupt, or whether a quarter-second step with a 6 cm
lift is too quick or too flat. Nor has it run in Unity.

In Unity since (the target test, 13 November, 36 s): the log reads as headless does. A swing lasts 0.27 s at the
middle in both, a foot is planted 44% of the time, on the ground and sliding 4%, the fighters stand 42% of the time.

**Planted or stepping, first part: a shove is stepped in the stance (the plan "Grounded legs", step 2)** (13
November). The plan's step 2 is four things: a planted foot is fixed, the hover lift and the fades go, what moved
the body at once becomes a push, a fighter pushed off another steps away. This is the last of them first, because
it was the most of what was left to see: after step 1 a fighter in guard still spent 11 to 13% of its time (a
struck target a third of it) walking off a shove in the guard's upright walk, standing up out of its stance at
every push and stepping out into it again after.

- *A shove is stepped in the stance* (`shovesSteppedInTheStance`). A fighter in guard that is pushed (kept off a
  body it has come too near, driven back by a blow that landed or that it blocked, a body thrown into it) keeps
  its stance: the body goes the way it is pushed and the feet step after it, as they do for a paced step. That
  holds for a push the feet can step after one at a time, under 1 m/s for a Medium; a harder one is walked until
  it has slowed to that, then goes into the stance as it goes. How far a shove carries the body is unchanged.
- *The feet step after whatever moves the body* (`Locomotor.Going`). Standing, the feet now follow how fast the
  root is really going, not only the walk's own velocity: ground given to a heavy blow, two bodies moved apart and
  a body pressed back all move the root at once, and the feet stepped after those late and slowly. They are
  stepped after now as a pace is. (They still move the root at once; making them pushes is not done.)
- *A step to its place is one arc* (`stepTakes`, `stepHeight`). The foot lifts (a tenth of the step's time), goes
  over to where it lands, and comes straight down onto it (the last fifth); where it lands is fixed as it gets
  there. Before, a stepping foot was also lifted by the rule that keeps a moving foot off the ground, which let it
  down only over the tenth of a second after it had stopped: the foot landed in the air and sank. And without
  that rule it went over the ground for its first and last centimetres. Standing only; in a strike or a reaction
  a foot let go is handled as it was.

Four fights of six, 60 s, the same seeds, step 1 against this:

| | step 1 | now |
|---|---|---|
| Standing still | 9 to 15% of the time | 10 to 16% |
| Stepping in the stance | 23 to 33% | 29 to 39% (4 to 12% after a shove) |
| Walking (the walk's animation) | 17 to 23% | 11 to 13% (walking off a shove 3 to 5%, asked to walk 6 to 8%) |
| Both feet on the ground | 9 to 15% | 13 to 18% |
| A foot planted | 35 to 38% | 38 to 42% |
| A swing, at the middle | 0.27 s | 0.22 s (a step in the stance 0.20 s) |
| Swings over 0.45 s, walking or standing | 28 to 33% of them | 16 to 21% (steps in the stance 7 to 11%) |
| A foot on the ground and sliding | 3 to 4% of the time, 1.1 to 1.6 m a minute | 3%, 1.0 to 1.9 m |
| On one foot, the body slow: the weight outside that foot | 27 to 31 cm | 37 to 40 cm |
| Blows started; blows that met something | 268; 242 | 256; 239 |

The last row but one is worse and will go on getting worse until the plan's step 3: the more a fighter steps in
its wide stance, the more of its time it is on one foot with its weight in the middle.

What the report now says of "planted or stepping", for what is left of this step (the legs' lines: what each
foot's hold is doing, how far a held foot's place goes and turns, how often a foot is let go because the body
went too far from it, how often it is lifted to hover):

- Standing, a held foot's place is fixed: over 1 cm in 2 to 5% of holds, never over 5.
- Walking and in clips its place goes over 1 cm in a third to a half of holds, but nearly all of that is the ankle
  going round a toe that stays (a foot rolling onto its toes). It turns over 20 degrees in 1 to 8% of holds: the
  body turned further than a foot is let turn (35 degrees) and the foot goes round with it. The plan has that in
  step 5 (a turn pivots to a limit, then steps).
- In a strike or a reaction a held foot is let go 20 to 25 times a minute for each fighter because the body has
  gone 21 to 25 cm from it (43 cm one time in ten): the strikes are scaled and snapped onto their targets (6 to
  12 m of root moved at once in a fight, all six fighters) over feet their clips planted. That is step 5 too.
- The hover lift still lifts a foot 20 to 23% of the time while walking and 12 to 16% in a clip, three quarters of
  that in a swing the clip has too low or on a foot let go in a clip; standing, 1 to 3%.
- Ground given to a blow still moves the root at 4 m/s (2.5 to 4.5 m a fight): a quarter to nearly half a metre in
  a tenth of a second. In the stance both feet are then let go at once and step after it, which is a hop back.

**Found on the way and fixed: the engine threw every body on the field.** A step's landing place, fixed in the
world, was held until the ground took the foot up. When a walk began under the step it had that foot in the air,
so the ground never took it up, the body walked on, and when the leg was past its reach the engine's next step
put every physical body in the fight at speeds of 10^12 and then at no place at all (seed 4 at 5.08 s; nothing
recovers from that). A foot come down over its landing now waits a twentieth of a second and no further than its
leg reaches, then goes back onto the clip's path. The engine has no guard of its own against this; any foot target
held in the world past a leg's reach would do it again.

For a run: `FIGHT_STEPS="0.22,0.75,0.12,1,0.18,0"` (the sixth: 0 walks a shove off as before).

Not verified: how it looks, and Unity. In particular whether a fighter driven back by a blow reads as catching
itself in its stance or as sliding back with its feet pattering under it; the feet take a fifth of a second a
step whatever the push, and the body is not leaning into it (steps 3 and 4).

In Unity since (the target test, 13 November, 78 s): as headless. A step in the stance lasts 0.22 s at the middle,
the two armed targets step in their stance 55 and 58% of the time and walk off a shove 9 and 10% (a third before).
The worst slides in that log are 54 to 59 cm, a target walking off a hard shove: the walk's own catch steps, which
this did not touch.

**The weight over the feet (the plan "Grounded legs", step 3)** (13 November). Three things, behind the Combat
Feel's `weightShifts` (off: the hips stay where the standing animation has them, as in step 2).

- *The weight shifts onto the carrying foot.* Standing in its stance, the hips go toward the foot that carries the
  body while the other steps, and come back between the feet when both are down (`weightShift`: 0.4 of the way
  from the middle of the stance to over that foot, about 17 cm for a Medium; `weightShiftTakes`). Never further
  than both legs reach.
- *A step from standing waits for it* (`stepWaitsForTheWeight`, 0.14 s). Settling into the stance after a strike
  or a stop, or stepping round to face, the weight goes over first and then the foot lifts; one foot at a time.
  Five to eight steps a minute for each fighter wait like this.
- *The hips are placed over the support.* Standing on both feet with the body's weight off what the feet hold
  (the upper body leaning, a stance landed a little ahead of the hips), the hips go the way the weight has to, up
  to 14% of a leg's length, until it is over them, and ease back when it would stay.

**The plan's measure for this step was wrong, and I have not met it.** It asked that on one foot the weight be
over that foot. In this stance the feet are 85 cm apart, so that is a 35 cm swing of the hips and everything on
them, there and back, for every step, and the steps are a fifth of a second. Nobody steps like that, and a body
does not need to: with its weight 35 cm from the carrying foot it falls toward the lifted one at 3.7 m/s2, which
in a fifth of a second is 9 cm and 0.8 m/s, and the foot coming down catches it. Pushed off the lifting foot
first, as a person does, the sway is 2 to 5 cm. So while a fighter steps at a step's pace the shift is a third of
`weightShift` (5 cm), and the full shift is for a step taken from standing, where the body has no going to carry
it. The measure that says whether a quick step holds the body up is where the body would have to put a foot to
stop itself, as the stepping foot comes down; the report has that now.

Four fights of six, 60 s, the same seeds, step 2 against this:

| | step 2 | now |
|---|---|---|
| Still on both feet: the weight off what they hold | 11 to 19% of the time (by over 3 cm 2 to 8%) | 4 to 11% (1 to 2%) |
| On one foot in the stance, the body slow: the weight outside that foot | 37 to 38 cm | 26 to 35 cm |
| The same, the body going: the weight outside that foot | 41 to 42 cm | 37 to 40 cm |
| The same: where a foot is needed to stop the body, outside that foot | 45 to 46 cm | 26 to 30 cm |
| As a stepping foot comes down: a foot needed over 10 cm outside the two | 19 to 20% of landings | 15 to 22% |
| Standing still; stepping in the stance; walking | 10 to 16%; 29 to 39%; 11 to 13% | 12 to 21%; 31 to 37%; 9 to 11% |
| A foot on the ground and sliding | 3% of the time, 1.0 to 1.9 m a minute | 3%, 1.1 to 1.7 m |
| Blows started; blows that met something | 256; 239 | 271; 268 |

Against the plan: standing with the weight inside the support over 95% of the time is met in three fights of four
(89, 94, 96 and 96%; over 3 cm outside 1 to 2% of the time in all four). On one foot over that foot is not, as
above. The landing row is unchanged: one landing in six still leaves the body needing a foot over 10 cm from
where its two are. Two feet in a stance hold a strip of ground 10 cm wide between them, and a body going across
that strip at a step's pace is ahead of it; that is the next step of the plan (a step lands where it holds the
body up), not this one.

Not done from the plan's step 3: "a lean the feet cannot hold triggers a step". A body pressed off its pose is
already carried the way it is pressed, and its feet now step after it; a step placed for the lean is step 4.

The playtest log's frames have `hipShift` (cm). The fight report has the weight over the feet in the stance, slow
and going, and each landing. For a run: `FIGHT_SHIFT=0` (off), `FIGHT_SHIFT="0.4,0.14"` (the share, the wait).

Not verified: how it looks, and Unity. The shift before a step from standing should be plain to see after each
strike, as the fighter settles: the hips go to one side, a foot steps, they go to the other, the other steps. If
it reads as rocking, `weightShift` is the dial; if the settling now drags, `stepWaitsForTheWeight`.

In Unity since (the target test, 13 November, 45 s): as headless. Standing on both feet the weight is off what they
hold 5% of the time; the hips are shifted 2 to 3 cm at the middle for a target stepping in its stance, 10 to 11 cm
one time in ten, 18 to 24 cm at the most.

**Steps where they are needed (the plan "Grounded legs", step 4)** (13 November). The plan's step 4 was: a shove
is answered by one to three steps placed where they catch the body, more for a harder shove, or by a stumble or a
fall when none can reach. Measured first, with a test made for it, most of that was already so after steps 1 to
3; what is built is the part that was not.

`shoves` (a new report: `dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll shoves`) stands one Medium in its guard
and shoves it at 0.3 to 2 m/s from four sides, each left to come to rest:

| Shoved at | walked off (before step 2) | stepped in the stance up to 1 m/s (steps 2 and 3) | up to 1.6 m/s (now) |
|---|---|---|---|
| 0.3 m/s | 4 steps, at rest after 1.3 s | 1 to 2 steps, 0.5 to 0.8 s | the same |
| 0.6 m/s | 4 steps, 1.4 s | 2 steps, 0.8 s | the same |
| 0.9 m/s | 4 to 5 steps, 1.5 s | 2 to 3 steps, 0.9 s | the same |
| 1.3 m/s | 4 to 6 steps, 1.7 to 2.1 s | 4 steps, 1.1 s, the first 0.2 s walked | 2 to 3 steps, 1.1 s, none walked |
| 2 m/s | 5 to 6 steps, 1.8 to 2.2 s | 4 to 5 steps, 1.2 to 1.6 s, 0.4 s walked | 4 to 5 steps, 1.2 s, 0.2 s walked |

- *Built: a harder shove is stepped in the stance* (`shoveSteppedUpTo`, 1.6 m/s for a Medium; at 1 it is as it
  was). Between a walk's speed and that, a shove stood the fighter up into the upright walk for a fifth of a
  second and dropped it into its stance again; now it keeps its stance, in two or three steps, with both feet off
  the ground for up to a fifth of a second as it is thrown (a hop). A blow that throws it back stumbling is still
  walked whatever its speed, as it was: that reaction's look is left alone.
- *Tried and taken out: stepping straight to where the body will stop.* The body's velocity dies away at a known
  rate, so where it comes to rest is known as it is shoved, and a foot could go straight to its place there. It
  took no fewer steps (two instead of three at 0.9 m/s from two sides, four instead of three from another), and
  it put both feet in the air more often: the foot left behind is at its leg's reach before the first has landed.
- *The hips over the support, a little further.* Brought only to the edge of what the feet hold, the weight stood
  on the edge and counted as off it up to 29% of the time in one fight; it is brought 2 cm inside, and is off it
  3 to 7% of the time in all four fights (the plan's step 3 asked under 5%).

In the fights the change is small, because it is a small share of them: walking off a shove was 3 to 5% of the
time and is 2 to 3%, walking in all 9 to 11% and 7 to 9%. Four fights of six against step 3: standing, stepping,
sliding, swings and blows are as they were (blows 271 and 296 started, 268 and 286 met).

**The plan's measure cannot be read off a fight.** It asks that after a push the place a foot is needed is back
on what the feet hold within three steps. The fight report counts it now (each shove: how many landings and how
long until the body stands on both feet with that place within 5 cm of them), but of the eight to twelve shoves a
minute each fighter takes, only one in four plays out: a third are overtaken by the next shove (a body kept off
another is shoved again every quarter of a second), nearly half by a strike or a reaction beginning. Of those that play out,
three landings at the middle. The `shoves` test is the measure that can be read: at rest in one to three steps up
to 1.3 m/s, four or five at 2 m/s.

Not done from the plan's step 4:

- *A stumble or a fall from what the legs can catch*: done since, below.
- *A step placed toward where the body needs it.* One landing in six still leaves the body needing a foot over
  10 cm from where its two are (15 to 22% of landings, as in step 3). Stepping straight to the resting place was
  the attempt at it.
- *The hard shove's own steps.* Over 1.6 m/s, and every stumble, is still the upright walk: 0.2 to 0.4 s with both
  feet off the ground at 2 m/s, and in your log of this morning slides of over half a metre.

For a run: `FIGHT_SHOVETO=1` (a shove stepped in the stance only under a walk's speed), `SHOVE_TO`, `SHOVE_STANCE=0`
for the `shoves` test.

Not verified: how it looks, and Unity. A fighter shoved hard (a body thrown into it, a blow it blocked from a
Large) should now go back in its stance in two or three quick steps with a small hop, where it stood up and
walked.

In Unity since (the target test, 13 November, 112 s): as headless; standing on both feet the weight is off what
they hold 3% of the time. Thirty of 129 blows that landed threw their target back stumbling, fifteen of them at
the most a stumble goes (2.8 m/s). The worst slide in the log, 95 cm, is one of those: a target thrown back at
2.8 m/s, the walk's catch steps dragging a foot.

**What the legs can catch decides a stumble or a fall (the rest of step 4)** (13 November; Combat Feel
`legsDecideAFall`, `legsCatchUpTo`; off: as before). Until now no blade knocked a standing fighter down, however
heavy the blow: whatever speed it would have thrown the body at was held to `mostStumble` (2.8 m/s) and the body
stumbled back at that. A Large's club on a Swordsman's chest asked 16 m/s and got 2.8. Now the speed the blow
would throw the body at, on top of how fast it is going already, is set against what its legs can do:

- up to 1.6 m/s (`shoveSteppedUpTo`): its stance catches it, two or three steps. A blow that "throws it back
  stumbling" this lightly is no longer stood up into the walk for it.
- up to 4.4 m/s (`legsCatchUpTo`): thrown back stumbling, the walk taking the catch steps, no faster than
  `mostStumble`, as before.
- faster: no step catches it. It goes down, and gets up again.

4.4 m/s is not set by eye: it is how fast the backward run in the animation set carries a body (`sns_run_back`,
4.41 m/s; the plain run 4.1 to 4.5). A body thrown faster than it can run backward falls. Because the speed
counts what the body is already doing, a second hard blow on a fighter still reeling from the first takes it down
where either alone would not.

A blow taken on the shield, or in the middle of the fighter's own strike, still never knocks it down: those were
not stumbles before and are not now.

Four fights of six, 60 s, the same seeds, against step 4 as it was this afternoon:

| | before | now |
|---|---|---|
| Knocked down by a blow | none | 1, 1, 2 and 3 a fight |
| Thrown back stumbling | 4, 10, 6 and 10 a fight | 4, 5, 2 and 9 |
| Walking (the walk's animation) | 7 to 9% of the time | 5 to 7% |
| Blows started; blows that met something | 296; 286 | 297; 292 |

All seven knock-downs are a Large's club: on a Swordsman's chest (asked 16 m/s), head (8) and sword hand (5), on
a Medium's head (4.5), pelvis (5, its own side's Large) and thigh (34), and one Large on the other's thigh (10).
No sword and no Medium's club reaches 4.4. In the target test (Mediums on Mediums) nobody is knocked down, as
before. With it off the same seed gives the same fight as this afternoon.

Not verified: how a knock-down and the getting up read in the middle of a fight of six. Falls from leg cuts were
shelved after your first playtests because the fall and the get-up did not read well, and this uses the same fall.
If it is too much, `legsCatchUpTo` up to 6 leaves only the heaviest (three of the seven), and `legsDecideAFall`
off is as it was.

In Unity since (13 November, two target tests, 38 s and 60 s): the knock-downs happen there too, and more often
than said above: one in the first test and four in the second, all a Medium's sword on a Medium (a thigh 6.9 m/s,
a shin 9.0, a chest 5.3, a head 4.9, a pelvis 5.8). The headless target test had none: its blows throw a target far
more lightly (0.7 m/s where Unity's are 3 to 4), which is not looked into. Each fall and getting up went through
without fault (three seconds on the ground).

**A body the engine loses is put back at once** (13 November; Combat Feel `lostBodyPutBackAtOnce`; off: as
before). The first of those two tests broke at 36.4 s: all six fighters gone from the scene at once. What the log
has: Attacker 2 struck with the spinning jump (`sns_combo_chop`) standing 0.8 m from Target shield. Its right leg,
swung round at waist height at 5 to 8 m/s, hooked the target's sword and sword hand and drew the arm out: the hand
11, 27, 45, 63 cm from its pose in four steps. A leg goes where its pose puts it and nothing stops it; the arm at
its full length could give no more, and the engine could not hold the body together: its pelvis, locked to what
carries it and never more than 2 mm off it, was 7 and 9 cm off for two steps, then 1.7 m, then 10^12 m. In that
same step the five others were thrown as far (a body moving that fast touches everything). All six were then taken
for borne down, fell where they had been thrown, and got up there. It is not the knock-downs: the one fall in that
test was twelve seconds earlier and the target had long been back on its feet. It could have happened in any test
since the upper bodies became physical; a leg met an arm 46 to 150 times in each of the tests before.

Now, after every physics step, a body whose pelvis is more than 3 cm off what carries it, or with a part more than
2.5 m from its pose or nowhere at all, is put back on its pose before anything reads where it is, and built anew
before the next step (put back only, a body that had been thrown kept places that are no numbers, and stayed
lost). The log has a line `[Upper] ... out of hand` each time, the fight report
a count.

Headless, the same fight with one fighter's sword hand pulled for four steps as a leg would pull it:

| | pulled with 2,000 kN | pulled with 20,000 kN |
|---|---|---|
| before | not run | the program stopped: no numbers anywhere |
| now | that body put back four times, nobody else touched, the fight goes on | all six thrown and put back three or four times, the fight goes on, everyone within 3 m of the middle |

In four fights of six with nothing pulled (60 s, the seeds as before), three never use it and are blow for blow
what they were. The fourth has the very thing in it, 5 s in: a fighter struck in the air comes down with its leg
through another's upper body. Before, that body was thrown about for six steps (its parts met at up to 127 m/s),
dragged its fighter 2 cm aside and happened to come back; now it is put back in the first of those steps, twice,
and goes on with its strike. (This is the place where an earlier fault of this session's made every body in the
fight a no-number.) That fight differs from there on: 91 blows where it had 93.

Not verified: that it holds in Unity against the leg that did it; the pull above is a stand-in, and the engine
there is the same one in another build. What you would see when it acts is an arm snapping back to its pose for a
frame, in place of everyone vanishing. It is a net, not the cure: a leg still cannot be stopped by what it hits.
That is what the plan's step 6 (physical legs) changes.

In Unity since (13 November, 96 s, the target test): the net was never used; the pelvis was never more than
1.4 mm off what carries it. Six knock-downs, each gone through without fault.

**Ground is given in the time there is (the plan "Grounded legs", step 2, the next part)** (13 November; Combat
Feel `groundGivenInTime`; off: as before). The plan's step 2 has "what moved the body at once becomes a push".
The largest of those moves is ground given to a blow: the step back was snapped in at 4 m/s whatever time was
left, 10 to 45 cm in three to seven physics steps. It only has to be there when the blow is, so it now takes the
time until then (less 0.04 s), no slower than 1 m/s and no faster than the 4 m/s it was snapped in at.

**It does less than was hoped, and the hop back is still there.** Four fights of six, 60 s (the frames have a new
column, `givingGround`; `Tools/Playtest/hops.py <fight folders>` reads each step back and the 0.45 s from it):

| | snapped in (two fights, 31 steps back) | in the time there is (four fights, 94) |
|---|---|---|
| A step back | 26 cm in 0.07 s | 33 cm in 0.14 s |
| Steps back slower than 2 m/s | none | 54% |
| Both feet off the ground at once, for 0.05 s or more | 94% of steps back | 97% |
| Both feet off the ground, of the 0.45 s from the step back | 52% | 46% |
| Blows that landed, of those started | 38% | 36% |

The body goes back half as fast, and where it stands when the blow arrives is as it was (as many blows land). But
both feet still leave the ground on nearly every step back. Two reasons, both measured: most ground is given by a
sword arm that is late to its parry, and that is read with a tenth of a second left, so half the steps back are
still snapped; and a foot's step takes 0.22 s, in which even the slower ones carry the body 30 cm, further than
the other foot can stay behind. A third of a metre in a seventh of a second cannot be stepped one foot at a time
by anything; it is a hop, for a person too.

So the plan's wording is wrong here, and what is left is a choice, not a fault to fix:

- leave it a hop and make it one on purpose: both feet off and down together, the body dipping to land, in place
  of two feet let go and brought after one by one; or
- give ground earlier (the lead is 0.22 s for a heavy blow, and none for a late arm), which changes the fight: an
  attacker follows a step back it sees early.

With it off, the same seed gives the same fight as before, blow for blow.

Left of the plan's step 2: the lift of a foot that moves over the ground with its sole down (it acts 20 to 23% of
the time while walking). It hides a foot the walk's clip swings too low; taking it out shows that foot sliding,
so it wants the swing's height mended first, not a switch.

Not verified: how the slower step back looks, and Unity.

**A step back is a hop, and a button to see it** (13 November; Combat Feel `stepBackIsAHop`; off: as before). Your
choice of the three above. A fighter standing in its guard that gives 12 cm of ground or more:

- both feet leave the ground as the body goes (a foot already in a step of its own goes with it, from where it
  is);
- each goes in one arc, 7 cm high for a Medium, to where the stance has it under the body as the body arrives;
  the feet are not put ahead of the stance the way the body goes, as a step's are;
- they come down together, 0.14 to 0.3 s after, and neither takes a step of its own for a quarter of a second
  unless the body is carried on away from it;
- the hips rise 2 cm in the air, sink 4.5 cm as the legs take the weight (in 0.07 s) and come up again (in 0.17).

Where the body goes and when is unchanged. A step back begun in a reaction clip, or by a fighter already walking
off a hard shove, is as it was: its walk steps after it.

**To see it:** the debug panel has a button, **Step back now (2)** (the key 2 does the same): every fighter
standing in its guard steps back 35 cm from its opponent at once, as to a blow read a tenth of a second before it
lands; those in a strike or a reaction do not, and the panel's message says how many did. Under it a toggle, **A
step back is a hop**, sets the Combat Feel's switch, so the same press shows it both ways. Best in the Target test
(1): the three targets stand in guard. The panel's speed buttons, or Pause and Step, show it slowly.

Headless, the button's step back (the target test, five presses, `FIGHT_STEPBACK="5,7,9,11,13"`), and four fights
of six (60 s; `Tools/Playtest/hops.py`):

| | the button, as before | the button, a hop | the fights, as before | the fights, a hop |
|---|---|---|---|---|
| Steps back | 18 | 19 | 94 | 74 |
| Both feet came down together (within 0.035 s) | 11% | 68% | 1% | 51% |
| and then both stood for | 0.03 s | 0.23 s | 0.03 s | 0.11 s |
| Blows that landed, of those started | | | 36% | 37% |

One fighter in the button's test, standing still as it began: both feet in the air from 0.03 to 0.15 s after, both
down at 0.17 s, both planted until 0.42 s; its hips 2.3 cm up in the air, 3.7 cm down at 0.23 s, level again at
0.4 s, never more than 1.5 cm in a physics step.

In the fights 48 of 79 steps back were hopped; the rest began in a reaction or with the fighter already thrown.
With the switch off the same seed gives the same fight as before, blow for blow.

Not verified: how it looks. The numbers say the feet leave and land together and the hips move smoothly; whether
it reads as a hop back or as a body jerked back with its legs tucked is for your eye. The dials are in the code
for now (`LocoBody.HopHeight`, `HopRise`, `HopDip`, `HopDipTime`); say which way it is off and they go on the
Combat Feel.

In Unity since (13 November): seen with the button both ways, and judged better without the hop. The switch is
off by default now and renamed `stepBackHops`, so a scene saved with the old one on does not keep it; the hop
stays in the code behind it, and the button and its toggle stay in the debug panel. Ground given in the time
there is (`groundGivenInTime`) stays on.

**The clips' feet: a planted foot pivots on the ball of the foot (the plan "Grounded legs", step 5)** (14 November,
overnight; Combat Feel `feetPivotOnTheBall`; off: as before). The plan's step 5 has two things: a lunge re-plans
where its feet land, and a turn pivots on a planted foot up to a limit, then steps. Measured first, the second is
where the sliding is, and the first is mostly so already.

*What the report says of a foot sliding in a clip* (new lines: by what the foot's hold was doing, by the clip, by
how far through the clip). In the target test (no knock-downs, 60 s) 5.8 m of sliding in clips for six fighters:
half of it in one clip, the turn to the right (2.9 m); each strike's clip 0.1 to 0.5 m. In four fights of six,
14 of 21 m in the three turn clips. The lunges are not where the feet slide: a strike's pose is built on its root,
so however the lunge is stretched or turned onto its target the front foot lands under the body as the clip has
it; what is left there is the back foot stepping after the body, 0.1 to 0.6 m a fight for each clip.

*What a turn did.* A planted foot keeps its own direction while the body turns over it, up to 35 degrees; past
that it has to go round with the body. It went round about its ankle, flat on the ground, its toes sweeping
sideways at 0.3 to 0.5 m/s: in one turn of 107 degrees a Large's toes swept 12 cm, then that foot stepped, and
the other landed and swept through the last 45 degrees.

*Now* it pivots on the ball of the foot: the toes stay where they are and the heel goes round, as a foot does. It
goes back to standing on its heel a moment (0.15 s) after its turn stops going with the body. The clips still
step each foot when they do; a foot whose leg runs out of reach still steps.

| | as before | pivoting on the ball |
|---|---|---|
| Target test: sliding in clips, six fighters, 60 s | 5.8 m | 3.6 m |
| of it in the turn to the right | 2.9 m | 0.5 m |
| a foot's sliding in all, each fighter | 1.3 m a minute | 0.8 m |
| slides over 5 cm, each fighter | 5 a minute | 3 |
| the open target hit; the shield's blocks and hits; the sword's parries and hits | 29; 29 and 3; 19 and 7 | 32; 27 and 3; 19 and 7 |
| Four fights of six: sliding in clips | 20.7 m | 15.5 m |
| of it in the turn clips | 13.9 m | 6.4 m |

*Whether it changes the fighting.* In the target test, no (the row above). In the four fights of six it looked as
if it did: more blows landed and more fighters died with it on (the same four seeds: 105 blows landed of 292
against 131 of 222). It is not so: fourteen fights of 15 s with other seeds, each run both ways, have 96 against
108 blows landed (44% and 49% of those met), the difference in a fight anywhere from 6 fewer to 8 more and not
distinguishable from none (t = 0.8). A fight of six goes another way from the first step that differs; four of
them say nothing about a few per cent.

**A fault in the measuring, found on the way and fixed.** After a knock-down the legs' reading was not made
while the fighter was down, and the first reading as it got up took each foot to have slid, in one step, from
where it stood before it fell: every knock-down counted as two to four metres of sliding in whatever clip the
fighter got up into, a turn as often as not (the "worst slide 157 cm" of this afternoon was one). The reading is
now forgotten when it has not been made for a moment. The slides in the tables before this one, from the day the
knock-downs came in, are too high by that; these are not.

**To see it:** the debug panel's **Turn now (3)** button (or the key 3): every fighter standing still turns 110
degrees to its right with its turn's animation, and the fight turns it back to its opponent, a second turn. Under
it the toggle **Planted feet pivot on the ball**. Watch the foot that stays on the ground: toes still and heel
swinging round, against the whole foot swivelling flat. Best in the Target test (1), slowed down.

Not verified: how it looks, and Unity. A heel swinging round a planted toe is what the numbers say happens; if a
heel now reads as skidding, that is for your eye.

**Physical legs (the plan "Grounded legs", step 6): built, and switched off** (14 November, overnight; Combat Feel
`physicalLegs`, off by default; the debug panel's toggle **Physical legs** sets it). It is off because nobody
has seen it: it is the largest change of the plan, and it should be looked at on purpose.

*What it is.* The thighs, shins and feet join the physical body the trunk and arms are: six more parts hung from
the hips, which the animation still carries, each driven by a muscle to where the animation and the foot
placement have it. The legs are held to the pose as the trunk is (no slack). A foot the pose has planted is
pinned where it is planted: a spring from that place to its ankle (120 kN/m for a Medium, giving past 1500 N),
not a lock, so a leg whose ends are both held does not fight itself, and a leg hooked or struck hard comes away.
Nothing of a body is stopped by the ground it stands on, legs included: where a foot stands is the pose's to say.

*What it is for.* Until now a leg went exactly where the animation put it and nothing stopped it or moved it: a
blade passed through a leg, a foot stood inside another fighter's foot, and a leg swung into another fighter
dragged whatever it hooked (which threw all six fighters out of the scene on 13 November).

The target test (60 s) and four fights of six (60 s), legs off against legs on:

| | legs off | physical legs |
|---|---|---|
| A body inside another's body by over 2 cm, the four fights | 5, 20, 4 and 13 times (4.6 s in all, 8 to 12 cm deep, most of them a foot or a shin) | never in three fights; 5 times in one (0.1 s, a hip in a hand) |
| the same, the target test | 8 times, 6.4 cm deep (feet in feet) | never |
| A planted foot, from where the pose has it | there exactly | within 0.3 to 0.6 cm 95% of the time; 0.8 to 2.4 cm 99% |
| A foot in the air | there exactly | within 1.2 to 2.0 cm 95% of the time |
| A foot's sliding, each fighter, target test | 0.8 m a minute, 3 slides over 5 cm | 1.1 m, 2 slides |
| Target test: the open target hit; the shield's blocks and hits; the sword's parries and hits | 32; 27 and 3; 19 and 7 | 30; 32 and 1; 18 and 9 |
| A body the engine lost | none | none |
| Knocked down and up again | as it was | eight times in the four fights, without fault |
| Time to compute a fight, headless | 1 | about 1.5 |

The plan's measures for this step: legs inside other bodies none over 2 cm (met but for one hip in one hand);
planted feet within 1 cm (95% of the time, not 99%); no body put back on its pose (met).

*How it got there.* Without the pins a planted foot crept under hips that move: within 0.7 cm 95% of the time,
2.1 cm 99%, and the legs' sliding went from 0.8 to 2.2 m a minute. Quicker muscles did nothing for it (twice and
four times as quick: the same). A pin of 30 kN/m helped a little; 120 kN/m brought the sliding back to what it
is without physical legs; 400 kN/m held the planted foot tighter and threw the leg in the air about (a knee
56 cm off its pose once). 120 it is.

*What it does not do yet.* **A blow to a leg hardly moves it.** The plan has "a blow to a leg knocks it aside and
it comes back". A leg a blow lands on is let go for half a second (its muscles eased, its foot unpinned) and sent
the blow's way; it moves 1 to 8 cm and is back in a tenth of a second. The joints are driven by speed, and a kick
is undone as soon as it is given. An arm's knock is put into the pose for that reason, and a leg's would have to
be; that is not built. What does work is the slow kind: a leg pressed on by a body or a shield gives and is
stopped, where it went through.

**To see it:** in the debug panel tick **Physical legs (new, off by default)**. Then, in the Target test (1):
standing and stepping should look as they did (press **Step back now (2)** and **Turn now (3)**); feet should not
swim or tremble on the ground; a fighter knocked down should fall and get up as before. **Knock a leg (4)** kicks
every fighter's right shin: expect a small twitch, as above, not a swing. The playtest log's table of each part
against its pose has the legs in it, so a run with it on tells me how the feet held in Unity.

Not verified: all of it, in Unity. The plan itself warns that the engine there has shown faults the headless one
did not, three times in the upper body's step, and the foot's pin is a use of the engine this project has not
made before (a sprung joint from a carried body to a part of the physical body; the hips' carrier is the same
joint, locked). Cost in Unity is not measured: six parts more a fighter and two joints.

**Left of the plan's step 2, and not done: the lift of a foot that skims the ground.** A foot moving over the
ground with its sole down is lifted by up to 7 cm; the plan has it taken out. It acts a fifth of the time while
walking, and what it hides is a walk's swing that the animation has low. Taking it out shows those feet skimming;
mending it means giving the walk's swing a height of its own, which changes how the walk looks, and the walk is
what you said to keep. It is left as it is.

In Unity since (14 November, 74 s of the target test with physical legs on): no fault, no body lost, 83 frames a
second. A planted foot is within 1.4 cm of its place 95% of the time and 4.1 cm 99%: looser than headless (0.3 to
0.6 and 0.8 to 2.4), not yet looked into. Looked at and found good, so **physical legs are on by default** (the
Combat Feel's switch is renamed `legsArePhysical`, so a scene saved with the old one off does not keep it).

**The tube test** (14 November; the debug panel's **Tube test (5)**). A controlled hit, apart from the fight: the
target test's three targets with no attackers (nothing in its hands; sword and shield; a sword), each with a tube
waiting beside it. **Hit (6)** sends every tube at its target; **R** (or the button, which then reads "reset")
stands them up fresh; new tubes are set when the last have gone, so they can be hit again without a reset.

A tube is a bar 1.2 m long and 8 cm thick, lying level, that comes at the target broadside in a straight line, as
a bat swung level does. It does not turn and does not fall. The sliders:

- **How hard**: its momentum, 5 to 600 N·s, the measure the fight's own blows are logged in (a Medium's sword cut
  is about 45, a Large's club 100 to 300).
- **How fast**: 2 to 25 m/s. With how hard, this sets its weight (60 N·s at 8 m/s is 7.5 kg): slower is heavier
  for the same hit, more of a shove.
- **How high**: 10 to 240 cm over the ground.
- **Nearer or further**: how far across the target the tube's end reaches, from 60 cm short of its middle (it
  clips the near edge or misses) to 60 cm past it.
- **Targets turned**: 0 to 355 degrees, so the tube meets the front (0), the left side (90), the back (180) or
  the right side (270); the label says which.
- **Also taken as a blow of the fight's**: off, the tube is only a body of the physics, and what it does to a
  fighter is what the physics does. On, the fighter also takes it as the fight takes a blow of that momentum on
  the part the tube first meets: its recoil, its arm knocked, its stumble, its fall. (A tube the shield stops
  before it reaches the body is not a blow.)
- **Targets: Mediums / Larges.**

Each hit is in the playtest log (`tube`: the weight, the speed, the part met, the side).

What it shows headless (`FIGHT_LINEUP=tubes FIGHT_TUBE="3,60,8,1.2,0.15,0,0"`: at 3 s, 60 N·s at 8 m/s, 1.2 m up,
15 cm past the middle, the front, the physics only), a tube on the chest of the target with nothing in its hands:

| | the physics only | also taken as a blow |
|---|---|---|
| 60 N·s (more than a sword cut) | the tube stops dead on the chest; the chest is not 1 mm off its pose | the chest 9 cm back, the fighter driven back 60 cm on its feet |
| 300 N·s (a Large's club) | not run | knocked down (thrown at 9.3 m/s), up again in a second and a half; the one with the shield took it on the shield arm and was thrown back stumbling 1.4 m |

The left column is the thing to know before the hits are made harder: **a blow's weight does nothing to the
physical body by touching it.** The hips are carried by the animation and each joint is driven straight back to
its pose, so the body is a wall to whatever strikes it. Everything a blow does today is done by the fight on top
(the right column). That is why a force added to the chest would not show yet, and what the plan's step 7 (hips
held with a limited strength) is for.

Not verified: the tube test in Unity (it is built against the headless engine and the compile check only): that
the tubes show, meet the bodies and are cleared.

**A fresh set of fighters froze the physics** (14 November, found by the tube test's first try in Unity: Hit (6)
sent three tubes and nothing moved). Not the tubes: any fresh set of fighters during play (R, the tube test, the
target test's button) stopped the engine for the rest of that play. A fighter with a physical body is told of
each of the engine's steps (`PhysicsSteps`), and a destroyed one was never taken off that list. The next step it
reached for its body, which had gone, and threw; that was before the engine was stepped, and it happened again
every step: 5,432 times in 91 s of that log, the engine never stepped once. The fighters stood (their poses are
not physics), so it looked as if only the tubes were broken. It had not shown before because no play since the
physical body had asked for a second set.

Two changes. A unit that is destroyed takes its fighter off the engine's steps (`ClipFighter.Release`, from
`UnitController.OnDestroy`). And the engine's stepping no longer depends on everyone told of it behaving: one
that throws is taken off and said so once in the console (`[PhysicsSteps] ... threw ... and is no longer told`),
and the others and the engine go on. The playtest log's console lines now also give the first three places in
the game's own code an exception came through (it gave only the top of the trace, which here was Unity's own
null check and named nobody).

Headless: `FIGHT_FALSESTART=0.55` raises a first set, fights it that long, destroys it as the sandbox does, and
says how many are still told of the engine's steps (3 before the change, 0 after); `FIGHT_THROWER=1` adds one that
throws (taken off at the first step, the tubes then meet their targets as without it). The headless engine does
not throw for a destroyed object as Unity does, so the freeze itself is not reproduced there: the count is what
is checked. Not verified in Unity.

In Unity since (14 November, 130 s, 27 hits from 60 to 600 N·s on the front, the back and low on the legs, the
physics only): the tubes show, meet the parts the log names and are cleared; no error. At 600 N·s the head was
pushed up to 38 cm off its pose and the spine to its 20 degree stop, and all of it sprang back: the body gives
and returns, and nothing that touches it can fell it.

**Controlled falls: the plan, and its step 1** (14 November). The research and the plan are a document of their
own, "Controlled falls: research and plan" (https://claude.ai/artifact/L1KorNskCqjo8dT32DSguE): what people do in
the 0.7 s a fall takes (bend the legs to get low, hands out with soft elbows, turn to land on what can take it,
the head kept off the ground), how games that get it right build it (the body stays physical and its joints
keep pulling toward a protective pose; nobody uses a limp ragdoll for the living), and eleven steps for this
fighter. Step 1 is built: falls on demand, and each fall measured. Steps 2 to 11 need the hips let go (the legs
plan's step 7).

*Falls on demand.* The tube test's panel has **Back, Forward, Left, Right** and **Thrown back**. Each turns the
targets so the tube meets the side that sends them that way, sends the tubes when they have turned (0.8 s), takes
the tube as a blow of 200 N·s (450 for thrown), and the target goes down whatever its legs say (the toggle "The
target goes down, whatever its legs say"; off, a tube is a blow like any other and the legs decide).

*Each fall measured* (`FallMeasure`; every fall of the fight's, not only the test's): the playtest log has a
`fall` line when a fighter begins to get up (the dead and those that stay down: after 3 s): which way it went
against the way it faced, what reached the ground first and when, when a hand, a knee, the hips, the trunk and
the head did, how fast the hips and the head were going, how far it slid. Headless the fight report gathers
them ("falls measured"). A part has reached the ground when its underside is within 3 cm of it; a knee when the
joint is within 12 cm. Nothing in a fall is changed.

What today's fall is, by those numbers (headless: 12 from the test, three each way, 3 thrown, 1 from a fight):

| | |
|---|---|
| the head reached the ground | 16 of 16 |
| how fast it was going | 4.1 m/s on average, 7.0 at most (a person who is trying: about 1.7; one who cannot: about 3.2) |
| the hips landed at | 2.2 m/s on average |
| on the ground after | about 0.45 s (a person: about 0.7 s; these legs give nothing) |
| a hand or knee down before the trunk | 9 of the 12 from the test, 0 of the 3 thrown |

Two things the numbers say that were not known. A hand or knee down first is no sign of bracing: a ragdoll's
hanging hand reaches the ground first by chance, three times in four, and holds nothing up. The head and the
hips are the measures that count. And the fall forgets the blow: one thrown at 13.9 m/s and one at 5.1 go down
the same, on the spot (the fall gives every body the same 1.4 m/s; `KnockOver` is always called with the same
knock). A hard blow that fells looks no harder than a soft one that does.

In Unity since (14 November, 36 falls from the buttons, all four ways): the buttons send the targets down the
way asked and every fall has its `fall` line; the head reached the ground in all 36, as headless.

Decided for the falls plan (14 November): the poses are authored in code; fighters keep hold of what they carry;
the dead fall limp while the living brace; the caught fall (a hand and a knee, and straight back up) is built;
how often they fall is left alone until the hit force and the staggers are in.

**Getting up** (14 November; the plan is a document of its own, "Getting up: research and plan",
https://claude.ai/artifact/TQNgYhYhCCX9sWzAMpcqnh). The get-up there was pulled the body, in 0.3 s, from however
it fell into a tidy lying pose, then played five mirror-image poses, facing the way the body happened to lie,
everyone to the same beat. The new one (`GetUp.Natural`; the Combat Feel's `getUpsAreNatural`, on by default; the
tube test's panel has a toggle beside the fall buttons, so the same fall can be watched both ways):

- **From where it lies.** The keys play from the first moment and each part leaves how it lay on its own clock
  (the head in 0.33 s, the arms in 0.54, the legs in 0.57, the trunk in 0.6): no tidy lying pose is passed
  through. Each step takes the pose a share of the way that is left to the keys' pose now; mixed afresh from how
  it lay each step, an arm half a turn from its key jumped across the body in one step when the moving key
  crossed over (33 m/s in the measure below).
- **Three routes, by how it lies**: from its back (its front more than 20 degrees up), from its front, and from
  a side (its front within 20 degrees of level), which is the route from the front begun rolled over.
- **Lopsided, as a fighter stands up in base.** The hand of the back foot's side goes down (for sword and shield
  the sword hand; the shield arm stays free, toward the enemy), the front foot is planted on its place in the
  stance, the hips lift on that hand and that foot with the trunk leaning to the hand, the back leg folds under
  to a half kneel, the hand leaves the ground, and it rises, the back foot landing on its place. From the front:
  hands planted by the chest, hands and knees, the front foot stepped through while that hand stays down, the
  half kneel, up. Seven keys instead of five; a key can now lean the body to a side.
- **Toward the enemy.** The body is turned about its hips toward whoever it is on (a sandbox target: the way it
  is set to face) by up to 35 degrees as it comes up off the ground.
- **Not all alike.** Each get-up is up to 8% faster or slower, by turns. 2 s for a Medium on average, as before.

Measured (a `getup` line in the playtest log when a fighter is up; headless the fight report's "get-ups"): the
route, how far it turned, the fastest any part moved in the first 0.7 s, the furthest a hand or foot its key has
on the ground was from where the key wanted it, and how far such hands and feet moved while their key held them
still. Headless, the same falls both ways (three targets, from the back and from the front):

| | as before | new |
|---|---|---|
| the fastest a part moved early | 5.9 to 9.3 m/s | 2.7 to 5.8 m/s (a Large: up to 6.5) |
| a planted hand or foot furthest from its key | 37 cm (from the back: the hand behind it could not reach the ground) | 0 cm from the back, 6 cm from the front (in the first step only) |
| planted hands and feet slid, in all | 2 to 3 cm | 3 to 7 cm |

What the numbers do not say is whether it looks like a person getting up. Not verified in Unity.

**Grounded legs, step 7: the hips held, not locked; and a blow's force** (14 November).

*The hips.* The physical body's hips were locked to where the pose has them, which made the body a wall: the tube
test, 600 N·s bent the spine to its stop and the hips did not move a millimetre. They are now held there by a
spring that gives (`UpperBodyArticulation.HipsSupported`; the Combat Feel's `hipsAreHeldNotLocked`, on; the body
is built again when it changes): as quick as 5 swings a second (`hipHoldQuickness`), damped so it does not swing
back and forth, and no stronger than 3 times the body's weight across the ground (`hipHoldStrength`; 8 times up
and down). They still turn exactly as the pose turns them. A body is "lost by the engine" now only when its
hips are 70 cm from the pose (it was 3 cm, which a held hip passes).

Headless, the tubes, the physics only (nothing of the fight's):

| | hips locked | hips held |
|---|---|---|
| 60 N·s on the chest | hips 0.0 cm, chest 0.0 cm off the pose | hips 0.6 cm, chest 0.5 cm, back within 0.1 s |
| 600 N·s on the chest | hips 0.0 cm (in Unity: the spine at its stop, the head 38 cm) | hips 3.0 cm, chest 17 cm, head 39 cm, back within 0.5 s |

That is what the physics honestly does with a blow: little. A 60 N·s cut on a 70 kg body is 0.9 m/s.

*A blow's force* (the Combat Feel's `hitForce`, 1; 0 is none). With every blow the fight lands
(`ClipFighter.TakeHit` to `UpperBodyArticulation.Strike`), on top of the weapon's touch and the fight's own
reactions: the blow's momentum times `hitForce`, along the blow, on the part struck (a shield's forearm, a
parrying sword's hand); half of that on each part joined to it (a chest's spine, head and upper arms); a quarter
on each part joined to those. It does two things:

- **It shoves the body**: the force goes into the parts as the physics' own impulse (no part sent faster than
  6 m/s), and through them into the hips' hold.
- **It bends the body**: each of those parts' joints is turned aside the way the force turns it, by the most the
  joint can be left aside (20 degrees for the trunk and head, 50 for an arm at rest) times tanh(force / 120 N·s
  at a Medium's mass), and comes back as a joint pushed aside does (0.3 s). The joints' muscles undo within the
  step any push that lasts one step, so the bend cannot come from the impulse: it is given to the joint as what
  it is left aside by.

Headless, a tube taken as a blow on the chest, where the chest and the head are 0.2 s after (cm from where they
were; after 0.3 s the fight's own reaction, which drives the body back half a metre, is most of it):

| | no hit force | hit force 1 | hit force 3 |
|---|---|---|---|
| 60 N·s | chest 7.6, head 9.9 | chest 10.8, head 16.4 | chest 13.4, head 19.4 |

A joint was turned aside 11 degrees at most by 60 N·s at hit force 1, 28 at 3, 42 by 200 N·s (an arm).

*Seeing it.* The tube test's panel: **Sword cut (60)**, **Heavy (200)**, **Club (600)** send the tubes as blows of
that size; a slider for the hit force; a toggle for the hips held or locked, and sliders for the hold's quickness
and strength.

In two fights of 45 s (headless, six fighters each): no body lost; the hips are 0.3 to 0.7 cm from the pose on
average and 4 to 22 cm at the hardest blows (a Large's club); feet are let go of and held as before; blows
landed, blocked and parried in the same numbers as with the hips locked, as far as two fights say.

Not done here: the hips let go for a fall (the falls plan's step 2), and the hips turning under a blow (they
are still turned only by the pose). Not verified in Unity.

In Unity since (14 November): the hips held and the hit force looked right at their defaults. One thing looked
like a fault and was not: the first hits "did nothing" because a tube was the physics only until a fall button
had switched it to a blow. A tube is now taken as a blow from the start (the toggle still turns it off).

**Controlled falls, steps 2 to 4: one body, with muscle, the head kept up** (14 November; the Combat Feel's
`fallsKeepTheBody`, on; `fallMuscle` 1; `fallsProtectTheHead`; `fallsReachForTheGround`; the tube test's panel
has the same beside the fall buttons).

A fighter that goes down no longer swaps its physical body for a limp ragdoll. The body it has is let go
(`UpperBodyArticulation.LetGo`): its hips are held by nothing, its parts have their weight (standing, they weigh
nothing on their joints), the ground stops it (standing, nothing of it is stopped by the ground), and each joint
is drawn to a pose by a muscle of **a person's strength** (the hip 220 N m, the knee 180, the ankle 100, the low
back 250, the chest 200, the neck 50, the shoulder 80, the elbow 60, the wrist 15, for a Medium; with the body's
weight and size), not the strength that follows a clip, which is twenty to forty times that. So a limb can be
held out, and a body's weight landing on it cannot be stopped. The legs have a third of that (they fold under
the weight; a person going down does not stand in a squat), the neck half as much again. Past what a joint of
that kind turns, it is held with six times a person's strength. A joint goes to its pose at 7 rad/s at most.

- **The pose** (`FallPose`) is no clip: it is made each step from how the body is. The hips are where the body's
  are. The trunk curls forward going backward and is held long going forward. The legs fold: thighs forward
  going backward (it sits down), knees bent with the feet back going forward (it kneels). The arms are thrown
  out for the first tenth of a second, then reach for the ground the body is going to, and a hand that can reach
  the ground goes to it. The head is held away from the ground: chin to the chest on its back (38 degrees),
  lifted on its front (30), tipped away on a side (28).
- **Its planted feet stay where they stood** until the trunk has tipped 50 degrees, the hips are down, or half
  a second has gone: a body knocked over goes over about its feet. (Let go at once, a blow high on the body
  turned it over in the air, feet at head height.)
- **Muscle**: all of it going down; easing to 0.4 of it over half a second once the hips are down. **The dead**
  have it for a tenth of a second and then none (0.03).
- **A fall remembers its blow**: a body thrown at 14 m/s goes down 2.8 times as fast as one only just felled
  (it was the same 1.4 m/s for every fall).
- **Getting up**: the body is taken up where it lies (`TakeUp`: held to the get-up's pose as a standing body is,
  its hips held again), so there is one body from the blow to standing.
- If any part of it is cut off, or it is to stay down, it falls as before (the ragdoll). A falling body the
  engine throws out of the world is given up for the ragdoll.

Headless, the fall buttons' falls (200 N·s, three targets each way; the Larges thrown with 450) and two fights
of a minute:

| | the ragdoll, as before | in the one body |
|---|---|---|
| the head reached the ground (the living) | 16 of 16 | 9 of 21 |
| in the fall test, backward / forward / left / right | 3, 3, 3, 3 of 3 | 1, 2, 1, 2 of 3 |
| two Larges thrown back | | 0 of 2 |
| in the fights | 1 of 1 | 3 of 7 |
| bodies lost by the engine | 0 | 0 |

What is not right yet, by the numbers: the hips reach the ground a quarter of a second after a 200 N·s blow, at
2 to 5 m/s. That is the blow: the body is free now and a blow of that size on the chest throws it flat. It is
harder than the ragdoll's fall (2.2 m/s), which no blow reached. The arms are at times half a turn from where
the pose wants them (a shoulder goes the long way round). The legs' fold is one rule for all falls. The steps
still to come (the falls by direction, a hand that lands held where it landed, the caught fall, the thrown
body's tuck) are where those go. Not verified in Unity: a body let go, with its weight, on the ground, has not
been run there.

In Unity since (14 November, 55 falls in the one body): it holds: no error, no body thrown out of the world, and
it was liked as it looked. Two things the log showed. At every get-up the body was carried to its feet for one
step and back ("out of hand: its pelvis 90 cm off what carries it", 45 times): making the get-up's keys leaves
the pose standing, and the body was taken up before they were made. It is taken up after. And the head reached
the ground in 51 of the 55, at 2.3 m/s at the middle (the ragdoll's 36 in Unity: 36 of 36, 2.6 m/s): a 200 N·s
blow has the body flat in a third of a second, before any reaching can help, so the numbers of people falling
on their own are not to be had from blows of that size. The hips land harder than the ragdoll's did (3.8 m/s
against 1.7), the blow being in the fall now. A `fall` line now says how it fell (the one body, its muscle).

**Controlled falls: the blow that fells, the legs, and the caught fall (step 8)** (14 November).

- **The blow that fells keeps 0.3 of its hit force** (`FelledHitForce`; `UpperBodyArticulation.EaseStrike` takes
  the rest back in the step it was given). Held by its hips a body takes a blow's force and comes back; let go
  in the same step, the whole of it (some 250 N·s for a 165 N·s blow) threw the free body flat in a fifth of a
  second, hips first at 4 m/s, before an arm could be put out. That was most of what was "not right yet" above.
  With it eased the same falls take 0.3 to 0.5 s to the hips.
- **The legs fold as the hips come down**, not by the clock: as folded as the hips are low. They go with the body
  and brake it; they do not pull it down.
- **A fall can be caught** (the Combat Feel's `fallsCanBeCaught`, on; it needs the falls in the one body and the
  new get-ups). A fighter thrown no more than 1.35 times as fast as its legs can catch goes down on legs that
  hold (1.6 times a person's, the rest 1.3). Half a second on, if its trunk has not gone over 60 degrees and it
  is all but still, it rises from where it is: from a knee and a hand (the get-up from its half kneel, 0.7 s) or
  from sitting (from its hand and its planted foot, 1.3 s), and is down about a second in all, not three. If
  its trunk goes over, it has fallen after all. `GetUp.StartKey` begins a get-up part of the way up. The tube
  test's panel has **A fall to catch (165)**.

Headless, the fall buttons' falls (200 N·s, three targets each way), the head reaching the ground:

| | the ragdoll | the one body, first | now |
|---|---|---|---|
| backward | 3 of 3 | 1 | 0 |
| forward | 3 of 3 | 2 | 0 |
| to its left | 3 of 3 | 1 | 1 (1.4 m/s) |
| to its right | 3 of 3 | 2 | 1 (1.6 m/s) |
| the hips land at, on average | 2.2 m/s | 3.3 | 1.1 to 3.2 by direction |

A 165 N·s blow from the front, three targets: none reached the ground with the head, one caught its fall sitting
and was up 1.9 s after the blow. Two fights of a minute: 10 falls, one caught, no body lost; of the six living
that fell, three reached the ground with the head (one thrown at 20 m/s, at 11 m/s). Not verified in Unity.

**The arms: the swordsman's free hand, the elbows in a fall, what a hand holds** (14 November, from the playtest:
the free arm behind the back looked wrong, the arms bent oddly catching a fall and getting up, and a held weapon
swung about and through the body in the get-up).

- **A swordsman with no shield carries its free hand before it** (the Combat Feel's `freeArmCarriedInFront`, on;
  `SwordParry.FreeArmInFront`): the elbow by its side and a little back, the forearm forward and a little in,
  the hand before the lower ribs, thumb up. It was tucked behind the left hip, a fencer's off hand; an arm that
  starts a fall behind the body has to come round it. The parry now keeps blows off that arm too, as it does a
  brute's. Off: behind the hip, as before.
- **In a fall the elbows point back and out of the trunk**, as an arm that takes weight bends. They were pointed
  by which way the body was going, so going backward they bent forward, as a knee does.
- **What the right hand holds** is kept out to its side and up in a fall, and in the new get-ups a weapon whose
  hand is off the ground is held up before it; in both the hand is turned no further on its wrist than a wrist
  turns (50 and 55 degrees). Asked for the whole turn, the hand was wanted half a turn from its forearm, the
  physical hand could not follow, and the weapon swung about.
- **Getting up, a swordsman with no shield puts its free hand down** and keeps its sword before it
  (`GetUp.PostWith`); a fighter with a shield still puts its sword hand down, and its sword is now laid clear
  as it does (it was only for one with no shield: `HoldsMain`).

Headless: the falls and get-ups run as before (no body lost, no pose that jumps); the hand is no longer the
joint furthest from the fall's pose. How the arms look is not something the numbers say. Not verified in Unity.

**Just up, the arms are eased into what a standing fighter's arms do** (14 November, from the playtest: the arms
and the sword still went through the body getting up, the swordsman's most). The get-up ends on the walk's own
pose; the step after, everything a standing fighter's arms do takes over at once (the sword's guard, the free
hand's place, the shield), and the arms were whipped there: in the Unity log of that morning the swordsman's
free arm turned at up to 89 rad/s at "got up" in 11 get-ups of 38, the open-handed one's forearm in 3. For 0.4 s
after it stands (`RoseEase`) the arms now go from where the get-up left them to where the fight has them, the
rest of the body as it is; a blow of its own takes its arm at once.

Measured now, in the `getup` line (written 0.6 s after it stands, so the taking-over is in it): in the pose, how
deep a forearm or hand and what the right hand holds went into its own trunk, head or legs and for how long;
and **as seen**, on the physical body, for how long either was inside. Headless, 23 get-ups from the fall test:
the pose's arm at most 7 cm in for 0.18 s, what it holds 9 cm for 0.08 s; as seen, an arm inside for 0.3 s at
most (5 of 23), what it holds 0.1 s at most (3 of 23). That is less than was seen in Unity, so either it is the
taking-over that was seen, or something Unity does that the headless engine does not: the next Unity log's
`getup` lines will say. Not verified in Unity.

In Unity since (14 November, 18 get-ups): noticeably better; as seen, an arm was inside the body for 0.13 s at
most and what it holds for 0.08 s. One thing was left, seen once right after a get-up: the swordsman's free arm
and sword through him as he turned about (`sns_turn180`; "UpperArmL turning at 70 rad/s").

**A swordsman's carried hand goes with its chest** (`SwordParry.FreeArmWithChest`). The free hand's place was
set against the way the feet face. In a turn on the spot the chest leads the feet by a quarter turn and more,
so the hand, kept "in front" of the feet, was taken across and through the chest, and came back at the turn's
end at 70 rad/s. It is now placed against the chest as the chest is turned and leant: 20 cm to its left, 20 cm
below its middle, 27 cm before it. Headless (`FIGHT_TURN="2,180"`, every fighter turned about; `FIGHT_FREEARM=feet`
for the old way): by the feet the same whip (65 rad/s) and the arm inside the body 0.25 s; by the chest no whip,
0.15 s.

A standing fighter's arms are now measured too while a playtest is recorded: a forearm or hand, or what the
right hand holds, inside its own body for 0.15 s or more is an `arms` line in the log, with the clip it was in.

In Unity since (14 November, log 14-02-18): the free arm held, but the sword was seen "rotating and going a bit
crazy" right after a get-up ("HandR turning at 63 rad/s", `sns_turn180`). Two faults, both older than the falls:

**A sword is taken up from where the arm is** (`ClipFighter.SwordTakenUpWhereItIs`). The guard keeps where it
has the sword as a place in the world, and moves it from there to where it is wanted at a hand's speed. Down and
getting up, the guard is not held, so that place stayed where the fighter stood before the fall: in the log, 1.4 m
behind and to the left of the body that got up. The arm reached for it there, behind its own back, the blade
pointing back through the body, for the whole turn that followed (the hand closes on its place at a hand's
speed, and the turning body carried the guard away about as fast). The guard is now taken up from the get-up's last pose as the
fighter stands, and afresh whenever it has not been held for 0.2 s. Headless (a thrown fall, then what the target
faces put behind it: `FIGHT_TUBE="3,450,8,1.3,0.15,0,1,1" FIGHT_FACE="4,173"`; `FIGHT_SWORDUP=0 FIGHT_TURNWAY=0` for
before): the pose had the sword hand behind its own chest for 0.87 s before, 0.00 s now; as seen, the arm inside
the body 0.08 s and the blade 0.05 s before, neither now. A new report line and `arms` log line say when a
pose has a sword hand behind its own chest.

**A guard turns round the way asked** (`Locomotor.GuardTurnsTheWayAsked`). A guard has one clip for turning
round, and it turns to the right (142 degrees). Asked to turn round to the left, it was played as little as a
clip is played (0.4 of it) to the right, and then again for all that was left: in the log, 55 degrees right and
then 165 more, each with the clip's deep dip, to face 140 degrees to the left. Now: a turn within 20 degrees of
straight behind goes round the way the clip goes; any other goes the way asked with the guard's quarter turn
played as far as it goes (140 degrees), and the little left is a small turn after. Headless
(`FIGHT_TURN="3,-150"`): before, 56 degrees to the right first; now 140 to the left in one. The feet slide as
much as before (0.83 m against 0.78 over the run).

To see it on demand, the tube test's panel has "A fall and then a turn": three buttons that fell the targets and,
while they are down, put what they stand facing behind them, so they get up into a turn round; and a switch
that puts both fixes off, to see it as it was. For the headless runs: `FIGHT_FACE="4,173"` turns what the targets
face at 4 s, `FIGHT_ARMTRACE="5,9,sword"` prints each step where the pose has the hands of the units so named.
Not verified in Unity.

In Unity since (14 November, log 14-33-56, 56 get-ups): the sword after a get-up was not seen again. Seen instead,
on the swordsman getting up from its back: "the leg he pulls back to push himself up goes a bit far back and
then the knee spins around through his torso".

**Getting up, the legs keep clear of the hips** (`GetUp.LegsClearTheHips`, Combat Feel "Get Up Legs Clear The
Hips"). Measured now in every `getup` line: the fastest the pose turned a thigh on the hips, the furthest it had
one past the hip's range, and the nearest an ankle came to its own hip (a leg folds to 0.30 of its length). The
get-up from its back, as it was, for all three fighters and not only the swordsman: a thigh turned at 81 to 90
rad/s, 140 to 145 degrees past the hip's range, an ankle 0.03 of a leg's length from its hip. Two things did it.
The planted foot was put on its stance's place, which for the swordsman (who posts on its free left hand, so
planted its right foot, the stance's back one) was under the hip it sat on: a leg folded to nothing has its knee
wherever the pose's pole says, which was straight up through the trunk and then round behind as the hips rose.
And the other foot was swept back while the hips were still low, through the hip itself, the knee swinging
round as it passed.

Now, from its back (a technical stand-up): the foot it plants is always its stance's front foot, and it sits
0.6 of a leg's length behind it (so it stands up a quarter of a metre toward its feet, as before); sitting, it
leans back a little with its back rounded and the other leg out before it, hardly bent; it lifts its hips high
on the hand and the planted foot (the bridge, 0.54 of a leg's length) with the trunk well over the hand; only
then the other leg swings back under the hips and its foot is set down behind on its ball; and it rises over
the planted foot into its stance. A swordsman's posting hand and planted foot are now on the same side. No
ankle is ever asked nearer its hip than a leg folds (`GetUp.FoldsTo`). From its front, the foot is stepped
through with the hips higher and less tipped forward, the back rounded instead.

Headless (the fall test, Mediums and Larges; `FIGHT_LEGSCLEAR=0` for before; `FIGHT_LEGTRACE="4.4,7,sword"` prints
each step where the pose has the knees and feet in the hips' frame): from its back a thigh turns at 10 to 15
rad/s (the leg's swing back), is at most 16 to 20 degrees past the hip's range, an ankle no nearer its hip than
0.32; the swordsman's foot no longer moves at 7 m/s as it is drawn in (3 m/s). From its front the thigh is at
most 46 degrees past (69 before; the swordsman 0, 22 before). The posting hand is at most 4 cm from where the
bridge wants it (0 before). The arms as before (none in the body). A fight of six ran as before. Not verified
in Unity.

Also from that log: a target thrown 3.6 m by the club (600 N·s), past its neighbour's spot, walked back into
its neighbour's waiting tube at head height and walked against it for seven seconds, 35 cm off its pose. A
waiting tube now touches nothing until it is sent. Known and left: walking off backward at once after a get-up,
the front leg can lock straight for a step (the walk's own, one step in one run); a hand or foot turns in one
step at the start of some get-ups ("FootL turning at 65 rad/s", five times in the log's 56).

**Controlled falls, steps 5 to 7: a hand that lands stays put, and a body that is down settles** (14 November;
Combat Feel "Falls Hands Stay Planted", "Fall Hand Hold", "Fallen Bodies Settle"). Measured first, in every `fall`
line: how far the hands went over the ground while they were on it, and how many times a hand that had reached
the ground left it again. As it was (the fall buttons, 200 N·s, three targets each way): the hands went 2.2 to
4.5 m over the ground in a fall, both together, and left it again 15 times in 11 falls. Two things did it.

- A hand that reached the ground was not held there: it skated. Now it is **held where it landed**
  (`ClipFighter.HandsStayWhereTheyLand`; `UpperBodyArticulation.PinHand`: a pin as a planted foot's, softer, and
  no stronger than a palm grips, 0.35 of the body's weight a hand, `fallHandHold`). The fall's pose puts the arm
  onto that place, so the arm bends under the body as it comes down. Pulled harder than it grips, the hand is
  dragged and brakes the body with that much; lifted, it has come away. It holds from the moment its underside
  is on the ground and it is going slower than 4.5 m/s, until 0.6 s after the hips are down.
- A body that was down went on being drawn to a pose made from how it lay, and that pose turned over as the
  body did: on its side it rocked from its front to its back and again for as long as it was down, its hands
  sweeping the ground (most of the hands' travel in a fall to a side: with the hands held but the body not yet
settling it was still 2.2 and 2.6 m; and why those falls lasted the full 2.5 s). Now it
  **settles** (`FallenBodiesSettle`): from a quarter of a second after its hips are down, over 0.4 s, the pose its
  muscles hold becomes the pose it is in; the head is still held up. Being still at once, it would be up one
  second after it went down every time, so a settled body stays down 1.4 s give or take 0.3 from one fall to the
  next: what bodies were down on average before (1.4 s in Unity over 56 falls, 1.6 headless).

Headless, the same falls, before and now:

| | backward | forward | to its left | to its right |
|---|---|---|---|---|
| the hands' way over the ground | 227 cm, now 23 | 218, now 22 | 452, now 16 | 216, now 15 |
| hands that left the ground again | 4, now 0 | 1, now 0 | 8, now 2 | 2, now 0 |
| the hips' way over the ground | 64 cm, now 26 | 9, now 0 | 109, now 20 | 39, now 5 |
| the head reached the ground | 0, now 0 | 1 at 3.3 m/s, now 1 at 0.6 | 0, now 0 | 0, now 0 |

Falling forward a fighter now ends on its hands and knees, its hips never down. Thrown (450 and 600 N·s) the
hands land late and are not dragged; the grip's strength made no difference to any of these falls (0.35 and 0.9
gave the same numbers), so it is a setting for harder throws than the test has. Headless only (`FIGHT_HANDSTAY=0`,
`FIGHT_SETTLE=0`, `FIGHT_HANDHOLD=0.9`). Not verified in Unity. Not done of steps 5 to 7: knees are not pinned, and
a body falling to a side does not turn to face the ground.

**Controlled falls, step 9: a body thrown off its feet tucks** (14 November; Combat Feel "Thrown Bodies Tuck").
Thrown at twice what its legs catch and more (9 m/s at a Medium's size: the open-handed target under the Thrown
back button and the Club blow; in fights, a Large's club at its hardest), a fighter is thrown, not knocked over:

- **The whole of it goes, and a little up** (0.3 of its speed). Knocked over, the legs are left behind and the
  body goes over its feet; thrown that way it was slammed flat in a quarter of a second.
- **It tucks** over 0.15 s (`FallPose`'s `tuck`): the back rounded, the chin down and the neck holding it there
  with three times a person's strength, the knees drawn half up, the arms in before the chest; an arm that holds
  a weapon stays out to its side with it.
- **It tumbles in its tuck** until its trunk or hips have been on the ground 0.45 s, then opens out; its hands
  land, are held, and stop it. It settles when it is going slower than 1.5 m/s, and is down half a second
  longer than one knocked over (and is not made to get up while it is still tumbling).

The fall's line now also says how far the hips turned over in all, and how fast the head was coming down when
it reached the ground (a head that rolls onto the ground goes fast along it and comes down slowly; one that is
slammed comes down fast). Headless, the open-handed target thrown from the front and from behind, 450 and 600
N·s (`FIGHT_TUCK=0` for before):

| | before | tucked |
|---|---|---|
| the head came down at | 0.2, 5.7, 5.3 and 1.7 m/s | 1.8, never, 2.2, never |
| the hips turned over | 100 to 150 degrees | 340 to 570 |
| it went over the ground, once down | 0.2 to 0.8 m | 0.1 to 3.8 m |
| it was down | 1.1 to 1.7 s | 1.8 to 3.5 s |

So thrown it is no longer slammed on its head or face, and it goes much further: it flies and tumbles where it
was put down on the spot. Thrown backward it turns right over backward, which is what a hard blow high on the
chest does to a body whose feet come with it; whether that reads as a tumble or as a stunt is for the eye. The
other falls are as they were (the four fall buttons give the same numbers). Not verified in Unity. Tried and
dropped: the tuck with only the trunk sent, as for a knock-over (it turned over in the air in a third of a
second and came down on its head at 5 m/s); a neck no stronger than in any fall (the head came down at 3 m/s
after the back); the tuck held until the body was slow (it slid on in a ball for five metres).

**Controlled falls, step 11: weapons, shields and sizes in falls, and what a fall costs** (14 November; Combat
Feel "Falls Arms Stay Out Of The Body", "Falls Shield Kept Before The Chest" and "Ragdolls Hold Solid Things").

**Measured first.** A fall's line in the log now also says, of the body as it is seen (the physical one), from the
step it goes down to the step it begins to get up: how long a forearm or hand was inside its own trunk, head or
thighs; how long its weapon was inside its trunk, head or legs, the fastest its end went and the lowest it came
over the ground; how long its shield was more than 4 cm into its body, the deepest it was and the lowest its board
came; and how far its hips rose again over the lowest they had been (`FallMeasure`, `ClipFighter.MeasureHeldInFall`;
measured while a playtest is recorded, and headless). The line also says when a fall was a ragdoll's. The report
has a line for the falls in the one body and one for the ragdolls, "what they held as they fell and lay".

**What it showed** (headless: the three targets, Mediums and Larges, the four fall buttons at 200 N·s and thrown
at 600; `FIGHT_TARGETS=MC` for a Large with its club):

- **Weapons were all right.** The ground stops what a hand holds, so no weapon's end went under it (lowest 0 to
  1 cm over it); a sword or a club was in its bearer's body for 0.8 s at the worst (a Large on its side) and
  mostly not at all; its end went 10 to 18 m/s at the most (24 thrown), a third of what a cut does. The fall's
  pose already keeps the blade out to its side and up.
- **Arms lay inside the body.** None of a body's parts stop each other (on its feet they must not: a guard has
  the arms against it). Fallen, an arm the body came down on stayed in it: the open-handed target thrown and
  tucked lay with its arms in its own chest for 1.95 s; a Large on its side had an arm in it for 0.7 s. And the
  fall's pose asked for it: the upper arm of a body on its side reached for the ground straight down, through its
  own chest.
- **A shield held the body up.** The shield arm reached for the ground as an empty one does; the board's edge
  met the ground first and stood there. The arm was 43 cm from its pose (a Medium) and 117 cm (a Large); the body
  hung on it, rolled off it sideways (its hips turned over 223 degrees falling forward, against 101 and 140 for
  the two without a shield), and a Large knocked forward ended on its back.

- **A ragdoll's sword went through the ground.** A fighter with a limb cut off, alive or dead, does not fall
  in its one body but as the rig's own parts, a ragdoll; half the falls of a test fight were these (three of
  six, seed 5), and the dead of them lie there for the rest of it. What a ragdoll holds was stopped by nothing
  (in a standing hand it only says what it touches, and nothing made it solid when the hand went limp; the
  fight before the clips did, `Weapon.SetLying`). A sword hung from a dead hand 84 cm down through the ground and
  swung there at 51 m/s; a shield lay 30 cm into the ground and 17 cm into its bearer; and a ragdoll's arm lay in
  its own body for as long as it lay (2.4 to 3 s of the 3 s measured), since none of the rig's parts stop each
  other either.

**What changed.**

1. **Fallen, its arms and its shield are stopped by its own body** (`UpperBodyArticulation.ArmsStoppedByBody`).
   From the step it is let go, its forearms, its hands and a shield on its arm are stopped by its own trunk, head
   and thighs, as they are by the ground; taken up again to get up, they pass as before. A blade is stopped by
   nothing of its bearer's. An arm that is in the body as it is let go (a blow drove it into the ribs) is not
   thrown out: it is stopped once the whole arm is out. (Stopped part by part, by the solids it was clear of and
   not by the one it was in, it was shut in, and lay in its own hips for as long as the body was down.)
2. **The fall's pose keeps the hands clear of the trunk** (`FallPose.ClearOfTrunk`): beside it on the arm's own
   side, before it, or behind it, whichever is nearest and over the ground, by 6 cm; a hand across the body has
   only the front to go round by. The trunk is a slab on the chest's axes, as wide and deep as the chest's solid
   (a Large's is 56 by 34 cm, a Medium's 31 by 18). A hand held where it landed is let go once the body has been
   over it for 0.1 s, and held again beside it (`ClipFighter.HandUnderFor`).
3. **A shield is kept before the chest** (`FallPose.ShieldKeptBeforeChest`): the hand 16 cm in front of the
   chest's front and 8 cm to its own side, the elbow down and out, as a guard holds it, from the first step of the
   fall to the last. Going forward the body comes down on its shield; going backward the shield lies over it. The
   shield hand is not held to the ground.

4. **What a ragdoll holds is solid, and its arms are stopped by its body** (`ClipFighter.RagdollsHoldSolidThings`,
   `HeldSolid`, `RagdollArmsToStop`). From the step a body falls as a ragdoll, its sword and its shield are
   stopped by the ground and by other bodies (by nothing of its own), and its forearms, hands and shield by its
   own trunk, head and thighs, each arm once the whole of it is out, as in the one body. Getting up, or built
   again, all of it is as before it fell.

| | before | now |
|---|---|---|
| thrown and tucked (600 N·s, from the front): an arm in its own body | 1.95 s | 0.07 s |
| the same from behind | 0.33 s | 0.07 s |
| a Large, the four falls: an arm in its body, the worst | 0.70 s | 0.68 s |
| a Large, the four falls: its weapon in its body, the worst | 0.80 s | 0.02 s |
| the shield arm from its pose as it falls forward (Medium, Large) | 43, 117 cm | 6, 6 cm |
| a Medium with a shield falling forward: its hips turned over | 223 degrees | 147 |
| a Large with a shield falling forward ends | on its back | on its front, on its shield |
| a Medium with a shield falling backward: its hips turned over | 128 degrees | 228 |
| ragdolls in the test fight: a sword's end against the ground, the lowest | 84 cm under it | 1 cm over it |
| the same: a sword's end, the fastest | 51 m/s | 11 m/s |
| the same: a shield against the ground, and in its bearer | 30 cm under, 17 cm in | on it, 5 cm in |
| the same: an arm in its own body, of the 3 s measured | 2.4 to 3.0 s | 0 s |

The row of the shield-bearer going backward is the cost: with its shield before it, a fighter going backward has one hand to brake with, not two,
and goes further over onto its shoulders (its head stays 15 cm up). The arm in a Large's body that is left
(0.68 s, falling to its left) is its sword arm passing its hips on the way to lying behind it; the 0.2 to 0.4 s
that the open-handed targets still show in a fall to a side is the tube driving the arm into the ribs as it
strikes, and the arm coming out.

**Sizes.** A Large falls by the same rules and the same pose, and comes out much as a Medium does, slower (its
hips are down 0.5 to 0.6 s after it goes, a Medium's 0.3 to 0.4). One difference to look at: knocked forward a
Large stays on its hands and knees, its hips 35 to 40 cm up and its trunk never on the ground, until it gets up;
a Medium's trunk comes down. A fall's muscles have a person's strength for their weight and size, and a Large has
more for its weight. A Large with its club falls as one with a sword does (the club in its body 0.07 s at the
worst). The Small was not tried: it is not in the battle, and the test's tube passes over its head.

**What a fall costs.** Nothing over standing. From the playtest logs of 14 November (Unity's editor, recording):
with the three targets a physics step's scripts took 4.4 ms while they stood, 4.2 ms in the second and a half
after a fall test's blow, and 4.7 ms while they were down and getting up (93 fall tests in one session; the
engine's own part 1.4 ms throughout). Headless, twelve bodies sent down together (`FIGHT_TUBEROWS=4`): the
physics and the muscles took 19.4 ms a step with all twelve on their feet and 21.1 ms with half or more down,
9% more; their arms stopped by their bodies made no difference that could be measured (5.6 ms against 5.8 without,
three bodies). The report says a step's cost, by whether any body is down or getting up (the units' own steps
there are the stub engine's, whose queries look through every collider, and say nothing of Unity's).

**What many fighters cost** is the fight itself, not its falls: in the same logs six fighters fighting took 8.0 ms
of scripts a physics step (the engine's part 2.4 ms, the units' steps 3.0, the recorder 1.0) against 4.4 ms for
three standing, so about 1.3 ms a fighter a step, and a step has 16.7 ms. In the editor, recording, twelve
fighters would take the whole of it. A sixteen-fighter fight runs headless without fault (`FIGHT_LINEUP=crowd`).

**The headless engine was wrong about one thing, and is put right.** Told that two solids stop each other from
now, or no longer do, PhysX goes on as it was for as long as the two stay near each other unless it is told to
look again; the runner told it to for free bodies and not for an articulated body's parts (`PhysXWorld`, where
`filteringDirty` is acted on). Unity looks again for both. So headless, a fallen body's arm that was told to pass
through its trunk again as it got up stayed stopped by it and was held a metre from its pose; and, since 14
November, a body getting up was still stopped by the ground it lay on, where Unity's is not. Both are as Unity
has them now. The numbers above are all from after that; the falls of the earlier steps come out a little
different in their details and the same in kind.

In the test fights (six for a minute, seeds 5 and 9) the falls in the one body had an arm in the body for 0.03 s
at the most and a weapon for 0.1 s, no weapon's end or shield under the ground, and nothing threw.

Headless only (`FIGHT_ARMSOUT=0`, or `pose` or `body` for one half; `FIGHT_SHIELDBEFORE=0`; `FIGHT_HELDSOLID=0`). Not verified in
Unity: what is seen there is the same engine told the same things, but nothing of this has been looked at. Found
on the way and not done here: getting up from its front, a kneeling knee's joint is 2 to 12 cm under the ground
for 0.05 to 0.65 s (the get-up's pose has it there; the ground stops nothing of a body getting up); thrown to its
right at 600 N·s (not tucked) a fighter's head comes down on the ground at 2.5 to 3 m/s; thrown backward at 650
N·s and more a tucked body's head still comes down at 3.4 to 4.7 m/s.

**Aimed attacks, step 0: the aim test, the red tint, and what today's blows do** (14 November; Combat Feel
"Struck Parts Flash Red"; the plan "Aimed attacks"). Nothing about how anyone fights is changed: this is the place
the new attacks will be built and judged in, and the numbers they have to beat.

**The aim test** (the place bar's **Aim**, or **7**; `AimTest`, `AimSpots`). One swordsman facing one target that
only stands there and takes no harm. Neither strikes by itself. The panel has a map of the body, nineteen spots:
the sixteen parts, the neck and the two shoulders (left and right are the target's own). Pressing a spot asks the
swordsman for one blow at it; **Again** repeats it; **Strike them all** goes through the nineteen, each asked once
both are back on their spots. The target holds nothing, sword and shield, or a sword (**Holds**), stands 0.8 to
2.6 m off (**Distance**, 1.4 m at first) and can be turned (**Turned**: 90 shows the swordsman its left side). For
now a blow is the one of today's nine strike clips that reaches nearest the spot, aimed at it and kept on it as
it plays (`UnitController.AimStrike`, `ClipFighter.StrikeNearest`).

**The red tint** (`HitFlashes`). The part of a body a blow lands on turns red and fades back to its own colour
over a second, in every place, the battle too: the part really struck, not the part aimed at. A blow stopped by a
shield or a sword tints nothing. Only what is drawn changes (the tint is laid over the part's material and taken
off again). The tick is in the panel under Hits and hips.

**What each blow is measured by** (the panel's "The last blow", an `aim` line in the playtest log, and headless):
what the blade met first, when, how fast and how far from the spot; whether that was the spot (a neck or a
shoulder has no solid of its own: it counts when the blow lands within 12 cm of it on a part round it); the
nearest the blade's striking part came to the spot before it met anything; how far its edge was turned off the
way it was going there; the furthest the blade as seen was from where its pose had it, and from the path its pose
took; and how far the swordsman's weight was outside his feet as the blow landed, and at the most.

**Today's numbers** (headless, the nineteen spots once each from 1.4 m; `FIGHT_LINEUP=aim`):

| the target | on the spot asked for | on another part | on a shield or a sword | on nothing |
|---|---|---|---|---|
| open hands, facing him | 5 | 13 | 0 | 1 |
| open hands, its left side to him | 2 | 17 | 0 | 0 |
| sword and shield | 0 | 5 | 12 | 2 |
| a sword | 0 | 5 | 14 | 0 |

- **Aim.** Against a target that does nothing, one blow in four lands on the part asked for (head, neck, chest,
  left upper arm, left shin). The belly and the hips are struck on the left arm in front of them, the right arm's
  spots on the head or the left arm, the feet not at all. Before it met anything the striking part of the blade
  came within 37 cm of the spot at the middle and 98 cm at the worst.
- **A guard stops everything.** Against a shield or a sword not one of nineteen lands where it was asked: today's
  blows go at the guard, not round it.
- **Timing cannot be chosen.** A blow meets something 0.18 to 1.13 s after it is asked (0.7 s at the middle); which
  it is depends on the clip, not on any moment wanted.
- **The edge** is 22 to 26 degrees off the way the blade is going, at the middle (0 is edge on).
- **The body is behind its pose, on its path.** At the worst of a blow the blade as seen is about 50 cm from where
  the pose has it, but only 11 to 14 cm off the path the pose took (27 to 47 cm at the worst): the body carries the
  blow out along the line it was given, late. A blow timed to a moment will have to allow for that.
- **Balance.** As the blow lands the swordsman's weight is 34 to 38 cm outside his feet at the middle (52 to 56 cm
  at the worst): the clips' lunges are stretched to reach, and land before the front foot does.
- Five of the nine clips are ever chosen (the forehand 8 or 9 times in 19).

Headless: `FIGHT_LINEUP=aim`, with `FIGHT_AIMKIT=open|shield|sword`, `FIGHT_AIMDIST=1.4`, `FIGHT_AIMTURN=90`,
`FIGHT_AIMROUNDS=2`; each blow is printed as it ends (`AIM ...`) and the report has the count. A regression fight
(six for a minute, seed 5) comes out blow for blow as before. Not verified in Unity: the place, the panel's body
map, the walk back to the spots, and the tint itself, which cannot be seen headless (the body's material has the
colours the tint is laid over, `_BaseColor` and `_Color`; that it shows on the drawn body is untested).

**Aimed attacks, steps 1 and 2: blows built in code, the sword arm and the trunk** (14 November; Combat Feel "Aim
Test Blows Are Built", "Blows Use The Trunk", "Blows Are Pushed"; `BuiltBlow`). Only the aim test asks for them. The
battle's attacks are still the strike clips, and a regression fight comes out blow for blow as before.

**What a built blow is.** Nothing is played. For the part asked for, as it stands:

- **The arm is a hammer's.** The upper arm, the forearm and the blade swing in one plane about the elbow's own
  hinge: the shoulder turns the upper arm, the elbow straightens, the wrist snaps the blade over, edge first. The
  blow is made of the joints' own turns, so each joint stays inside a body's range (a first version asked the arm
  to follow a blade's path, and the wrist was 70 to 110 degrees past its range).
- **At the target** the arm rig is asked for the arm that lays the blade's striking part on the spot with the wrist
  snapped forward (`ArmRig.Solve` with a keyed wrist). Round the line from the shoulder to the spot the whole arm
  can be turned; it is turned to where its swing brings the blade in along the line asked for (one of eight:
  straight down, down from the right or left, level from either, up from either, straight up), as near as a
  body's arm can be (`ArmLimits.Read`, the gate's own reading).
- **Wound up** is the same arm with each joint turned back, less far if the blade would be in its own body or the
  arm past its range. **Carried through** is each joint going on past the spot, slowing.
- **In time**: from wherever the sword is held to wound up, a stillness, then the trunk, the shoulder, the elbow
  and the wrist one after another, the elbow at a steady speed for the last 0.05 s into the spot; then back to
  where the sword was held. A wrist cut is planned to land 0.26 s after it is asked, an elbow cut 0.38 s.
- **The trunk** turns away as the blow is wound up, turns in before the arm goes so the sword shoulder is forward
  as the blade lands, and leans in as far as the reach needs (5 to 22 degrees). The arm is planned on the trunk as
  it will be at the spot and carried by it.
- **Pushed** (0 to 1, 1 at first): wound further back, a longer stillness at the top, carried further through.
  Where and when it lands does not move with it.
- **Judged as a clip's blow is.** The hit detection plays a built blow as it plays a strike clip (`StrikeHits.Built`):
  the blade is live from the start of the cut to the end of the carry-through; stopped by a shield, a sword or a
  body it backs out and goes back to its guard; a defender reads its coming path from the plan and blocks or
  parries it.

**In the aim test**: **Built blows** (off: the strike clips, from 1.4 m), **Line** and **Size** (chosen for the
part, or set by hand: the eight lines, wrist or elbow), **The trunk in the blow**, **Pushed**. With no line set,
of the lines the part may be cut on the one the arm makes best from where it stands is taken. The two stand 1.05 m
apart, about as near as the fight lets enemies come; the feet do not move yet.

**What it does** (headless, the nineteen spots once each from 1.05 m; `FIGHT_LINEUP=aim FIGHT_AIMDIST=1.05`):

| the target | on the spot asked for | on another part | on a shield or a sword | on nothing |
|---|---|---|---|---|
| open hands | 5 | 13 | 0 | 1 |
| sword and shield | 1 | 2 | 15 | 1 |
| a sword | 1 | 6 | 12 | 0 |

- **It lands when planned**: an elbow cut 0.40 s after it is asked against 0.38 planned, a wrist cut 0.27 to
  0.28 s against 0.26. Today's clips land 0.18 to 1.13 s after.
- **The edge** is 13 to 14 degrees off the blade's way as planned and 13 at the middle as measured (the clips: 22 to 26).
- **The arm is in range** throughout (0 to 3 degrees past, by the gate's own reading).
- **The blade as seen** is 5 cm off the path its pose takes at the middle and 10 at the worst (the clips: 13 and
  32); it is still about 50 cm behind its pose at the worst of a blow, as the clips' is.
- **Of the twelve spots in reach** (the head, neck, shoulders, chest, belly, hips, upper arms, thighs, a hand)
  five are hit: the head, the right shoulder, both upper arms, the left thigh. Two land 12 cm off on the chest
  (the neck, the left shoulder). Five are stopped by the target's own arm in the way (the chest, belly and hips
  behind its left arm): nothing yet checks what the blade will meet first.
- **Out of reach** from standing feet: the shins, the feet, the forearms and the far hand (11 to 54 cm short).

**Found on the way.**

- **The arm alone reaches nothing from where fighters stand.** A swordsman stands with his sword shoulder back:
  from 1.05 m apart his shoulder is 1.24 m from the other's head, and an arm and a blade reach 1.1 m. Tried
  nearer, his guard's point is already in the other's body. So the arm (step 1) was not worth showing without
  the trunk (step 2), and the two are one piece of work here. With the trunk the reach is about 1.25 m.
- **A hammer's arm cannot cut level from the right.** Straightening the elbow throws the hand away from the fold;
  for a blade to travel from right to left the arm would have to lie across the chest folded outward, at the end
  of a shoulder's range. Asked for, such a cut comes in 50 to 87 degrees off its line, so the choice takes a
  diagonal instead. A level forehand is made by the upper arm rolling and the trunk turning, with the palm up: it
  comes with the full cuts.

Headless: `FIGHT_AIMBUILT=0` (the clips), `FIGHT_AIMTRUNK=0` (the arm alone), `FIGHT_AIMLINE=DownFromRight`,
`FIGHT_AIMSIZE=Wrist`, `FIGHT_AIMPUSH=0.5`; `BLOW_TRACE=1` prints each blow's plan. Not verified in Unity: how any
of it looks, which is the point of it. The swordsman's weight is 27 cm outside his feet at the middle as a blow
lands (the clips: 34): the lean is not yet answered by a step. Low parts are not bent for (the plan had the knees
in step 2; they come with the step).

**Aimed attacks, made from the recordings: a strike taken apart and made again** (15 November; Combat Feel "Aim
Test Attacks Are Made", "Attacks Borrow Legs", "Clips Travel As Recorded" and the fourteen "Attack ..." style
levers; `AttackScore`, `AttackMaker`). The first built blows looked mechanical: an arm and a trunk worked out in
code, on feet that did not move. They are kept behind their switch and set aside. An attack is now made from a
recorded strike, so the whole body moves as the recording's does. Only the aim test asks for them; the battle's
attacks are still the strike clips.

**What a sword blow is made of** (read off the recorded forehand cut; times are against the blade's landing).

- **The guard** is side-on: hips turned 38 degrees to the right, left foot 0.34 m ahead, right 0.41 m behind.
- **The wind-up** (the first half second): the front foot lifts and steps 0.20 m forward, landing a quarter of a
  second before the blade does. The chest winds back to 62 degrees, the sword arm is drawn back and folded (elbow
  100 degrees, the blade cocked 108 degrees off the forearm).
- **The drive** (the last 0.17 s): the back knee straightens (46 to 21 degrees) and pushes; the hips travel 0.46 m
  forward and sink 12 cm; the front knee bends (41 to 81 degrees) and takes the weight; the back heel is up and
  that foot drags forward on its toe. Then each part peaks a little after the one below it: the hips' turn at
  -0.17 s (353 degrees a second), the chest's at -0.13 s (550), the arm, the elbow and the wrist together in the
  last 0.03 s (600 to 740). The point is going 36 m/s.
- **At the landing** the hips have turned 75 degrees, the chest 136, the arm is nearly straight.
- **Carried through and back**: the chest goes on another 38 degrees; everything is back in the guard 0.87 s after.

That order (legs, hips, chest, arm, hand, with the front leg as a brake) is what the sport studies find in a
fencing lunge, a throw and a bat swing. The rising backhand does it on a passing step (the back foot goes 1.1 m
past the front one, then the cut); the thrust does it standing, on the ball of the back foot.

**How an attack is made.**

- **The score.** A strike clip is taken apart into each body part's own track: the hips' place and turn, the
  chest's turn on the hips, the head's on the chest, each arm's bones in the chest's frame, each hand's turn,
  each ankle's place, each knee's way, each foot. Put together again it is the clip to within 0.3 to 1.5 cm. Its
  marks are read off the blade: when the drive begins, when it lands, when it is carried through.
- **The style** changes the tracks: how far each part goes in each phase, and when. Fourteen levers, each 1 for
  the recording's own: wind-up, arm swing, hip drive, twist, lean, lunge, drop, crouch, carry-through; wind-up
  time, drive time, back time; hips lead, arm lags. Six to start from: Plain, Heavy, Quick, Brawler, Low, Whip.
- **The aim.** Every moment of the cut at which the blade is still fast is tried as the moment to land. For each,
  the hips are lowered and the chest leaned for the target's height, the lunge stretched or shortened for its
  distance (0.3 to 1.8 times), the body turned over the wind-up for its line (no more than 50 degrees, about its
  planted foot), the wrist
  steers the last of it, and the sword hand makes up what is left. Each of these costs something; the moment that
  costs least is taken, and of the recordings the one that costs least.
- **The legs can be another recording's.** The chest and arms of one blow on the hips, legs, steps and travel of
  another, each phase stretched to fit: the thrust (recorded standing) on the forehand's lunge is a lunging
  thrust; the forehand on the thrust's legs is a cut from where it stands.
- **The moment.** Asked to land at a set time, the wind-up is stretched or shortened; the time the walk holds a
  clip back for its blade's speed limit is allowed for.
- What comes out is a clip like any strike clip, so the walk, the feet, the hit, the defender's read and the
  physical body all take it as they take a recording.

**A bug found on the way, in every strike** (Combat Feel "Clips Travel As Recorded"). A clip's travel was turned
into its own frame the wrong way round. A clip that turns as it travels then went off to a side by twice its
turn, and every strike turns 60 to 80 degrees: the rising cut's 0.42 m step forward went 0.33 m to the left, the
lunging backhand's 1.3 m forward went a little backward. It is fixed for the battle as well, so the regression
fight is no longer the same fight: 78 blows and 2 falls where it was 68 and 5 (seed 5, six fighters, a minute).
With the switch off it is the old fight blow for blow.

**Measured** (headless, a Medium with sword and shield 1.5 m from the target, each of the 19 spots twice).

- **As posed, the blade's striking part passes 4 cm from the spot at the middle, 17 at the worst.** The made
  clip's own blade is on the spot to the centimetre where no hand move is needed (`attacks`).
- **On an open target 21 of 38 land on the spot asked for** (the strike clips: 5 of 19 from 1.4 m; the built
  blows: 5 of 19 from 1.05 m). The rest land on a neighbour, 18 cm away at the middle: the blade as seen trails its pose by 40 to 60 cm
  at speed and meets a nearer part on the way in. Against sword and shield 27 of 38 are stopped by the shield,
  against a sword 27 of 38 are parried: nothing yet picks the open line.
- **Further out**: the thrust on the forehand's lunge, from 2.0 m, travels 0.5 m and passes 1 to 8 cm from the
  spots on the trunk as posed (on its own legs it reaches 1.3 m). The leaping chop, named and from 2.2 m, passes
  0 to 3 cm from the upper spots as posed.
- **Asked to land at a set moment**, blows met something 0.47 to 0.58 s after when asked for 0.5 s (0.52 at the
  middle), 0.87 to 0.97 s for 0.9 s, 1.08 to 1.27 s for 1.2 s (1.20 at the middle). The early and late ones are
  blows that met another part on the way, and a thrust, whose wind-up is short and stretches only so far.
- **The feet** stay planted as the recording's do. A body turned to bring its blow onto a line turns about the
  foot that is planted through the wind-up, not about its own middle (turned about its middle, a 35 degree turn
  slid the back foot 31 cm; about the foot, 9 cm, where the recorded forehand's back foot itself drags 9). A lunge
  stretched to reach drags its back foot that much further, as a longer lunge does.

**In the aim test** (key 7): Blows cycles made, built, clips. Blow and Legs choose the recording and whose legs;
Lands in sets the moment; the Style buttons and the Style levers set the style. Press a part.

Headless: `attacks` (the scores made again against their clips, the maker's own check, how long making takes:
1.7 ms); `ATTACK_DUMP=made.json ... attacks` then `python3 Tools/Mocap/Attacks/strip.py made.json out` draws made
attacks as film strips. `FIGHT_AIMMADE=0`, `FIGHT_AIMATTACK=sns_thrust`, `FIGHT_AIMLEGS=sns_forehand` (or `own`),
`FIGHT_AIMBORROW=0`, `FIGHT_AIMLAND=0.9`, `FIGHT_STYLEPRESET=Heavy`, `FIGHT_STYLE="WindUp=1.6,DriveTime=1.2"`,
`FIGHT_AIMWHO=shield`,
`FIGHT_TRAVEL=0` (the old travel). `Tools/Mocap/Attacks` has the scripts the clips were read with (`anatomy.py`:
each part's course through a strike; `look.py`: frame by frame).

Not verified in Unity: how any of it looks in motion. I have only seen stills of the made poses (the film
strips). Known: there are three plain recordings to make from (forehand cut, rising backhand, thrust), so there is
no plain low cut, overhead chop or descending backhand yet, and low parts are reached by crouching and leaning a
forehand or a rising cut; the thrust's recorded hand would point this sword up and back, so a thrust's blade is
turned onto its line in code; the rising cut's arm is past the rig's own shoulder range for part of its swing (as
its recording is), more so with a big arm swing (at 1.3 the arm is 21 degrees past range at the middle of a
blow, and the arm as seen falls behind); a body turned a long way (the most is 50 degrees) turns on its planted
foot as on a pivot, the foot itself not turning. Every
style ran the nineteen spots with nothing thrown, the pose's blade 4 to 5 cm from the spot at the middle.

**Fight rhythm from recordings, step 1: the duel lab and the style pack** (15 November; Combat Feel "The Battle
Is Retired"; `MoveStyle`, `DuelLab`; the plan "Fight Rhythm from Recordings"). The battle is retired: the fight
is being built again from a duel up, on the made attacks and on movement styles made from recordings. Nothing
about how a fighter walks is changed in this step.

- **The battle is retired.** It is not among the places at the top of the panels, the scene begins in the duel
  lab where it would have begun with the battle, and the walk lab's way to the fight goes to the duel lab. Its
  code is still there, and with the switch off it is a place again. The regression fight is the same fight (78
  blows, seed 5).
- **A movement style is a pack** (`MoveStyle`). The clips a guard walks with were found by their names in the
  walk itself. They are now a style: a named pack of recordings, each with a job (standing, walking forward and
  back, a step to each side, running forward and back, turning). The standard style is the sword and shield
  pack's guard, clip for clip what it was. The walk (planted feet, real steps, stride fitting, stand and step) is
  the same for every style; the capoeira pack will be another style.
- **The duel lab** (8, or Duel at the top of a panel): a swordsman with sword and shield and one that stands
  there, 3 m apart. The buttons move him about the other: Walk to, Back away, Stand, Circle left, Circle right,
  with how near he walks and how far he backs. He is steered as the fight steers a fighter (a velocity and a
  facing, to his walk), so what is seen is the walk the fight will have. The panel shows the style he moves in,
  the order, the distance, his speed.

Measured (headless, `FIGHT_LINEUP=aim FIGHT_DUEL="WalkTo:6,CircleLeft:7,BackAway:6,CircleRight:7,WalkTo:5"`,
`FIGHT_DUELAPART=3`): he walks in from 3 m to 1.44 m and stands, goes round at 0.66 m/s keeping his distance to
within 8 cm, backs away to 4.1 m and stands, walks in again.

**Found, and not yet understood: the feet's slide.** The fight's measure of feet sliding while down reads 0.4 cm
a second in the regression fight. In the duel lab it reads 1.6 cm a second for him going round (the worst single
slide 6 cm) and 3.7 cm a second walking in and backing away (the worst 41 cm), and 4 to 5 cm a second for the
one that only stands and turns to face him (the worst 65 cm). The old battle moved its fighters slower and less
than these orders do (it went round at 0.35 m/s), so this is either a slide that was always there at a walk's
pace and when turning on the spot, or the measure counting a foot that swings low over the ground as down (the
slides it traces are a foot 1.2 to 1.4 cm up). It is the first thing step 2 (the paces) has to settle, since
feet that stay planted at every pace is what that step is for. Not verified in Unity: anything.

**Fight rhythm, step 2 (first part): the paces** (15 November; Combat Feel "Guard Jogs"; the duel lab's Pace).
In the lab the swordsman went only at his guard's walking pace, in small steps. He can now be asked for any pace.

- **Pace** (the duel lab's slider, 0 to 3.6 m/s; all the way down is the guard's own walk as before). Under about
  1 m/s he stands and takes single steps; to about 2 m/s he walks; above it he jogs with his guard up.
- **The guard jogs.** Above a walk the guard had only its run forward and back (4 m/s, its stride shortened) and
  nothing of its own to the sides. The jog recordings (forward 2.6 m/s, back 2.4, left 3.0, right 2.5; recorded
  with empty hands) are taken apart as the strikes are: their hips and legs are put under the guard's own chest,
  head and arms, frame by frame at the same moment of the stride (`MoveStyle.LegsUnder`). The shield and sword
  are carried as the guard carries them. Off: as it was. It changes the regression fight (its run-ins are jogs):
  65 blows; with the switch off, the 78 it was.
- **Going round** holds its circle better: at a brisk pace he drifted outward by 2 m in six seconds; now by 0.4 m.

Measured (headless, from 5 m, `FIGHT_DUELPACE`): asked 2.6 m/s he reaches 2.5 walking in, 2.3 backing away and
going round; asked 3.4 he reaches 2.9 walking in over 3.5 m and 3.1 going round. His feet slide 0.2 cm a second at
2.6 m/s (the worst slide 4 cm) and 1.7 at 3.4 (the worst 9 cm); nothing fell. Still to come in step 2: the leaps
in and out, and the run on top of the jog (above about 3.2 m/s the jog's stride is stretched). Not verified in
Unity: how the jog's legs under the guard's arms look.

**Setting off from a stand is eased** (15 November; Combat Feel "Sets Off At A Walk's Rhythm"). Seen in Unity: as
he starts toward the other, the legs jump from the stance to the walk's position. Read off the session's log and
a headless trace: for the first quarter of a second, while the body had gone 5 cm, the feet stepped at the quick
rhythm of a positioning step (1.5 strides a second). The back foot was whipped from its place in the wide stance
to its place in the walk at 2 to 4 m/s, the front foot followed at once at 3.5 m/s, and the hips were jolted over
at 0.9 m/s in the first tenth of a second. Now, asked for a walk's pace or more, the first steps go at that pace's
own rhythm (0.93 strides a second), and the stance fades into the walk over a half-life of 0.2 s, not 0.1. The
trace: the back foot starts at 1 m/s and gathers speed with the body, the front foot stays planted through the
first 0.35 s, the hips go 0.3 to 0.5 m/s. The lab's panel shows how he set off (the fastest foot and hips in the
first 0.35 s). `FIGHT_SETOFF=0` is as it was; `FIGHT_DUELTRACE=1` writes a setting off step by step. The
regression fight with the paces' switches on is 84 blows; with them off, the 78 it was.

**Leaps** (15 November; the duel lab's Leap in, Leap out, Hop left, Hop right; `ClipFighter.LeapNow`). From a
stand in his stance he springs toward the other, away from it or to a side: both feet leave the ground together,
each goes in one arc to where the stance has it under the body as it arrives, and they come down together, the
hips sinking a little as the legs take the weight. It is the hop the walk already had for a step back from a blow
(built earlier and left switched off there), asked for on demand and in any direction, without that step's push.
How far (0.2 to 1.2 m) and how long (0.15 to 0.4 s) are sliders; he is carried no faster than 4 m/s and is in the
air 0.3 s at most. Measured (headless, `FIGHT_DUEL="LeapIn:2,LeapOut:2,HopLeft:2,HopRight:2"`,
`FIGHT_DUELLEAP`, `FIGHT_DUELLEAPTIME`): asked 0.50 m he goes 0.50 m, asked 1.00 m he goes 1.00 m, every one
hopped, nothing fell. The feet's slide measure reads 5 cm a second over those runs (the worst 46 cm): it cannot
tell feet a few centimetres up in a hop from feet sliding, so how a leap looks is for the eye. Not verified in
Unity. Not done yet in step 2: the run on top of the jog; a leap made from the leap recordings (this one is the
stance's feet moved in arcs, not a recording).

**Leaps made of a recorded jump** (15 November; Combat Feel "Leaps Are Made" and the six "Leap ..." levers;
`LeapMaker`). Seen in Unity, the stance's hop above did not work: the fighter was moved like a piece on a board,
the legs barely moving, the upper body doing nothing. A leap is now made as an attack is made, from a recording.

- **What a jump is** (read off the recorded jump on the spot, 1.7 s): the hips sink 30 cm as the chest folds
  forward and the arms swing back (the crouch, 0.4 s to its lowest); the legs straighten, the arms swing up and
  the hips shoot up (the push, 0.2 s), one foot leaving the ground after the other; the body is in the air with
  the legs tucked (0.45 s, the hips 31 cm above standing); the feet touch one after the other and the hips sink
  14 cm as the legs take the weight; and it straightens.
- **What a leap takes of it.** Those phases, each stretched to the leap's own time (crouch 0.22 s, push 0.12 s,
  the time in the air asked for, landing 0.14 s, back up 0.26 s). The hips' sink and rise, scaled to the leap (a
  short leap is a small crouch and a low flight). The trunk's fold and stretch and the arms' swing, added to the
  guard's own chest and arms, so the shield and sword stay held. The legs' tuck in the air.
- **What the leap adds: the way it goes.** The body travels the way asked: a little as it crouches, faster
  through the push, evenly through the flight, slowing as it lands. The chest leans that way as it pushes off,
  comes back through the flight and leans a little the other way to land. The foot nearer the way of the leap
  leaves the ground first. The other stays planted while the hips go away from it, so that leg straightens and
  pushes, and leaves last; they land in the same order. Forward it is the back leg that pushes, backward the
  front leg, to a side the leg on the other side.
- It is a clip the walk plays, as it plays a made attack. Off: the stance's hop.

The lab's sliders: how far, how long in the air, crouch, height, lean, arms, body, crouch time. Measured
(headless): asked 0.50 m he goes 0.50 m, asked 1.10 m he goes 1.10 m, each way; nothing fell. Looked at as stills
(`LEAP_DUMP=leaps.json ... attacks`, then `strip.py`): leaping forward, the hips sink and the chest leans in, the
back leg is straight behind him as the front foot comes up, he is in the air with it trailing, lands on the front
foot and comes upright. Not verified in Unity: how it moves.

The feet's slide found in step 1 was looked at in Unity: no slide seen. So the measure is taken to be counting
a low-swinging foot as down at a slow walk; that is not confirmed.

Also: three errors in the session of 15 November 14:39, in the duel lab. Something asked the old pose pipeline to
try out a blow (the sandbox's old test keys B, G, L, H, K, D, J, 2, 3, 4 do, and the old rhythm's feints; the log
does not say which), and a fighter that walks by the clips has none of that pipeline's parts. Such a blow is now
one that cannot land, not an error.

The extra animations (19 clips) are read; the four jogs are in use: jogs forward, back and to each side (2.4 to 3.0 m/s,
the pace between the walk and the run that was missing), a run backward, a side hop of 1.25 m in half a second
(the goalkeeper's sidestep), a forward jump of 1.3 m, two boxer's bounces with a jab, pivots. `python3
Tools/Mocap/Rhythm/rhythm_survey.py <folder>` reads a pack: each clip's length, travel, speed, turn, hip height,
foot contacts.



Unity, from that first look: the articulation probe read exactly as headless on every count (targets a turn
vector, stiffness for radians, the force limit a torque, a part built turned takes its turn on its own axes), so
what the headless engine shows is what Unity does. A frame cost 11.4 ms with the physical body against 7.7 ms
without (six units; `Physics.Simulate` 2.8 ms against 0.4), in a session where the bodies whirled and touched far
more than they should. Headless, a 30 s fight of six takes 175 s against 40 s held to the pose.

Known, and for the next steps: the legs are the pose's and push with no limit; a blow lands or not by the pose,
whatever the physical blade met; a blocked strike's clip plays on while the blade rests where it was stopped; a
unit's own blade and shield are not stopped by its own body; a joint put where the pose flipped it goes through
whatever is in its way.

For looking into a fight: `OVERLAP_WHO=1` splits each kind of overlap by who was in it, `OVERLAP_TRACE=1` prints
each step of each (with where the engine itself has the two shapes), `FIGHT_STRAY=1` each step a body is far off
its pose with what it touches, `FIGHT_WATCH="B Medium"` every step of one unit with its joints' targets and where
they are, `UPPER_DEBUG=1` what each body is built of and each joint the pose flips, `UPPER_GHOST=1` the bodies
touching nothing, `UPPER_ALLOW=0` the muscles held to their strength, `FIGHT_KICK="3,150"` every body thrown out of
hand at 3 s (and how fast its joints still turn each tenth of a second after), `UPPER_RETURN=0` the motors springs sent
the pose however far off they are (as before the second look), `UPPER_SLACK="15,1,40,0.3"` the slack's angle and
time and the taut angle and time, `UPPER_GIVEWAY=8` and `UPPER_GIVELAG=0.01` when a joint gives way (999: never),
`UPPER_FOLLOW`, `UPPER_DAMP`, `UPPER_EVEN=0`, `UPPER_WEIGHT=1`, `UPPER_ITER` and `UPPER_VITER` (the engine's steps
for a body), `FIGHT_MUSCLE=600`, `UPPER_MOSTRETURN`, `UPPER_JOINTSPEED`, `UPPER_PUSHOUT` the limits, `UPPER_SNAP=0` a joint the pose flips brought there by its muscle, `UPPER_AXES=parent`
speeds given on the wrong axes. `FIGHT_GIVE`, `FIGHT_GIVE_FROM=0.1,0.22`,
`FIGHT_PRESSED`, `FIGHT_BORNE`, `FIGHT_EASE` set the rules above for a run.

`Tools/CompileCheck` compiles every script against small UnityEngine/UnityEditor/URP stubs (with real vector maths)
and runs the headless reports:

```
cd Tools/CompileCheck
dotnet build Editor.csproj                     # runtime + editor scripts
dotnet build Runtime.csproj -p:LegacyInput=true
dotnet build Tests.csproj
dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll fight /tmp/fight 7     # a whole 3 v 3 fight (seed 7) with a playtest log; FIGHT_MIRROR=1 swaps the sides
python3 ../Playtest/fightview.py /tmp/fight/PlaytestLogs/<session> fight.mp4   # that fight as a video, seen from above
dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll physx                  # the real engine's checks (pass/fail)
dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll physx-arm              # the sword arm experiment
dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll locolab . 20           # the lab's walk
dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll spar /tmp/spar 40      # the lab's two fighters
dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll strikes /tmp/strikes   # each strike aimed; also dummyhits, combos, cores, footwork, parryreach, combatfilm
dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll armgate                # the arm gate on arms at random; armrange: the clips' own arms against the ranges
dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll legrange               # the clips' own legs against a leg's ranges
```

The stubs only cover the API this project uses, and the dry run has no collisions or joints (a ragdoll's parts fall
each on its own onto a flat ground); Unity is the final word.

**Fight rhythm, step 3: attacks from any pose** (15 November; Combat Feel "Attacks Start From Where He Is",
"Attack Carries For" and "Borrowed Legs Go Back At Their Pace"; the duel lab's "Strike at" buttons). Until now a
made attack could only be watched from a stand. Asked for while he walked, he stopped in one step (a body going
2.5 m/s lost 2 m/s of it between one step and the next), every limb stopped where it was and was then moved to
the attack's guard, and a foot caught in the air was put down where it happened to be. Now:

- **His speed is carried into the attack.** The way he was going runs down evenly to nothing over 0.4 s (the
  lever) under the attack's wind-up, so he covers half of what he would have at full speed: 28 cm from a walk of
  1.4 m/s, half a metre from a jog. The attack's own travel is added on top of that.
- **The aim allows for it.** The attack is fitted to where the target will be against him when the blade lands,
  not where it was when he began: walking in he lunges that much less, backing away that much more, going round
  he turns further.
- **A foot in the air finishes its step.** It stays in the air for as long as its step had left and comes down
  under the body as it arrives. The foot on the ground is held where it stands and steps after the body when it
  is left behind, as it does when he stands and steps.
- **Each limb keeps the speed it had** while it goes over to the attack (the fade between two motions now carries
  speed as well as place, for a made attack).
- **Standing too, the attack's travel begins from rest.** The recordings begin already moving, half a metre a
  second in their first frame; played as they are the body jumped to that speed in one step, every time.
- Off: all of that as it was.

In the duel lab: give him an order (walk to, back away, circle, at any pace), then press a part under "Strike
at". He strikes from where he is and goes on with his order. "He strikes: from where he is / stopped, from his
guard" switches old and new. Two lines tell what he was doing as the blow was asked and how its start went: how
much the speed of his root, hips, feet and sword hand changed in the blow's first step.

Measured (headless: `FIGHT_DUEL`, `FIGHT_DUELPACE`, `FIGHT_DUELNEAR`, `FIGHT_DUELBLOW="3.0:Chest,7:Belly"` asks a
blow at those seconds, `FIGHT_ANYPOSE=0` the old way, `FIGHT_COAST=0.4`). The change of the root's speed in the
blow's first step, old to new: standing and stepping in at 0.75 m/s, 0.36 to 0.06 m/s; walking in at 1.6 m/s,
1.08 to 0.07; jogging in at 2.5 m/s, 1.98 to 0.06; circling at 1.6 m/s, 1.27 to 0.05; backing away, 1.48 to
0.07; standing, 0.48 to 0.07. The hips as posed: 0.45 to 1.98 before, 0.01 to 0.17 now. The blade as posed
passes 0 to 5 cm from the spot asked for from all five ways of moving (3 cm standing); before, from a walk or a
jog, it fell 22 to 30 cm short, because he had stopped. Five blows at five parts while circling both ways: the
blade as posed within 0 to 6 cm of each. The standing aim test (19 parts at 1.5 m) is as it was: 9 or 10 on the
part asked for, the posed blade 3 to 4 cm from the spot at the middle. The six fighters' fight with the jog and
the eased setting off turned off is unchanged (78 blows): the battle's blows do not go this way yet.

Found on the way and mended: **a thrust on the forehand cut's legs threw him backward as it ended.** The cut's
legs take 0.6 s to step back to the guard; that was squeezed into the thrust's 0.1 s, so in the blow's last six
frames the body went back at 5 to 9 m/s and turned 60 degrees. From a stand he then stood; asked to walk on, he
kept that speed and was carried 1.5 m away. The legs now go back at their own pace and the arms wait in the
guard (Combat Feel "Borrowed Legs Go Back At Their Pace"; `FIGHT_LEGSBACK=0` the old way). After it the fastest
his root goes through five blows while circling is 1.8 m/s.

A way to look at what the fight poses: `FIGHT_FILM="film.json,3.0,4.2"` writes the bodies as posed, each step
between those seconds, and `python3 Tools/Mocap/Rhythm/film.py film.json out.png --moments 12` draws them as a
strip of stick figures from the side and from above (feet's locks, sword and all). The strips of these starts
show whole poses and the blade on the target. Not verified in Unity: how it moves. Stills 0.05 s apart cannot
show a snap or its absence, only the numbers above can, and they are of the pose, not of what the eye makes of
it. Known and not done: the planted foot's catching up is the stance's stepping, not a step made for the attack;
the turn he had while circling is not carried (only his travel); the limbs are faded point by point, not joint
by joint; the battle's own blows still start the old way.

**Attacks thrown on the move** (15 November; Combat Feel "Attacks Are Thrown On The Move" and the four "Moving
Blow ..." levers; `AttackOnTheMove`; the duel lab's "He strikes: on the move / planted / stopped (old)"). Asked
for in Unity after step 3: the blows out of a walk stop the walk, strike and walk on, which is right and kept,
but there should also be blows that happen while the movement goes on, with the legs not ignoring them.

A blow asked for while he walks (about 1 m/s or more: walking in, going round, at a jog) is now thrown over
the walk. The body is in layers, the first use of the plan's layers:

- **The legs, their steps and his travel are the walk's.** It goes on as it is steered.
- **The chest, the head and the arms are the blow's**, a made attack's clip as before.
- **The hips are both**: the walk's, turned part of the way to the blow's own hips, sunk as the blow sinks them
  and carried a few centimetres toward the target. The spine twists for the rest, so the chest and the blade go
  round against the way he faces exactly as the recording has them.

**How the legs answer the blow** (they do not walk on as if nothing happened):

- **The hips over them turn and sink** with the blow, so the knees bend and the thighs go round.
- **The walk is checked.** From just before the hips start in he slows, to 55% of his pace as the blade lands,
  and is back to his pace a third of a second after. The steps shorten and slow with it and pick up again: a
  catch in the stride, not a stop.
- **The walk steps in.** On the move there is no lunge, so the distance is the walk's to make up: it carries
  him toward the target, on top of where it was going, by what the blade would be short (63 cm at most, and no
  faster than a step; half as far back when he is too near). The legs take it as steps, because it is the walk
  that goes there.
- **The blade lands with a foot.** The walk's coming footfalls near the blow's own time are each tried as the
  moment to land, and the best taken. Measured, the foot comes down 0.06 to 0.11 s before the blade.
- A blow the walk cannot bring within reach even so is thrown planted, with its lunge, as in step 3; the lab
  says which was done and why.

The aim: the attack is fitted to where the walk will have him as the blade lands (going as he goes, checked,
stepping in, turning to keep facing his enemy), and the target is carried from the walking body's chest to the
recording's, so the recording's own maker fits it. As it goes, the weapon hand is pulled by what the walk has
gone off that plan.

Measured (headless: `FIGHT_ONTHEMOVE=0` turns it off, `FIGHT_MOVECHECK`, `FIGHT_MOVEHIPS`, `FIGHT_MOVEINTO`,
`FIGHT_MOVESTEPIN` the levers, `FIGHT_OVERTRACE=1` every step of the layering). The plan against the layering
with nothing else in the way (`attacks` mode, "thrown on the move"): the layered body's blade is 0 to 1.3 cm
from the target for the two cuts, walking in and going round; for the thrust 14 cm, which the wrist's steer
makes up in play as it does standing. In play, nine blows thrown on the move (walking in at 1.6 m/s, jogging in
at 2.5 m/s, going round both ways at 1.6 m/s, backing away; head, chest, belly, shoulder, thigh, shin): the
blade as posed passes 0, 1, 2, 2, 7, 12, 14, 17 and 19 cm from the spot asked for, 7 cm at the middle. Planted
blows are 0 to 5 cm: on the move is less exact, most of all the thrust and a blow from very near, where he
turns faster than the plan has him. His root's speed changes by 0.01 to 0.2 m/s in the blow's first step. The
standing aim test and the six fighters' fights (78 and 84 blows) are unchanged. Looked at as film strips: he
walks, jogs or goes round with the legs stepping throughout, the upper body winds up and cuts, and the blade
is on the target as it lands. Not verified in Unity: how it moves, and whether the catch in the stride reads as
one.

Also: the debug panel is wider (440, was 330), so the duel lab's rows fit without scrolling sideways. A blow's
line in the playtest log says when it was thrown on the move and how far the walk stepped in for it. Known and
not done: whether the blow's way is open is not looked at for a blow on the move (the first that fits is
thrown); only the three plain blows can be thrown so; standing and stepping slowly in his stance a blow is
always planted; the battle's own blows do not go this way yet.

**Fight rhythm, step 4: the ginga** (15 November; the duel lab's style "Standard / Capoeira"; Combat Feel "Ginga
Size", "Ginga Arms", "Ginga Sword Follows The Arm", "Ginga Sword Points Ahead", "Ginga Sword Trails";
`MoveStyle.Capoeira`). The second movement style: where the standard style stands still in its guard, the
capoeira style stands in the ginga, capoeira's base, never still.

- **The recordings.** Four takes of the ginga on the spot from the capoeira pack (empty hands; 2.9 to 3.8 s
  loops), baked as `cap_ginga`, `_b`, `_c`, `_d`. One loop of the plain take: the left foot steps back and
  across, the feet come wide, the right foot steps back and across, wide again; the hips sway a metre from side
  to side and sink 20 cm; the body turns 45 degrees each way; the arms swing across the face.
- **A stand that steps.** The walk stands in a guard's idle with the feet held where they stopped. In the ginga
  the feet go as the recording has them: down when it has them down, lifted and placed by it. Measured, they
  slide 0 to 0.2 cm a second.
- **Its size.** The ginga is made at two thirds of its recording (the lever): each part's track is taken, what it
  does is counted from the middle of the loop and multiplied, and the body put together again, so a foot that
  stands still in the recording still stands still. The arms have a lever of their own on top.
- **Sword in hand, never still.** He has a sword alone. The sword hand goes where the recorded hand goes. The
  blade is not laid along the recorded fingers (the hands were empty: it would sweep through his own head as the
  arm crosses his face): the wrist is held straight, so the blade is carried on the forearm and swings as the
  arm weaves; it is turned part of the way to where he faces; its point trails the hand; and it is kept out of
  his own body. Measured at two thirds: the sword's point goes 1.7 m/s at the middle and is never under 0.2 m/s;
  it stays 49 cm from his own middle line (27 cm with the blade as recorded); it points between 9 and 78
  degrees to his left of straight ahead and up to 30 degrees up.
- **The arms are the ginga's** while he stands in it and no blow is coming at him. A sword alone otherwise holds
  its guard still, ready to parry; it takes the sword up again, from where the ginga has it, when a blow comes
  or he walks.
- **To go anywhere** he walks and jogs as the standard style does, and goes back into the ginga where he stops.
  Blows are thrown as before: planted from the ginga, on the move from the walk.

In the duel lab: "Standard / Capoeira" at the top (it sets the lab out again, with a sword alone); under it the
ginga's take, its size, its arms, and the sword's rule with an old and new switch. Headless:
`FIGHT_DUELSTYLE=capoeira`, `FIGHT_GINGASIZE`, `FIGHT_GINGAARMS`, `FIGHT_GINGATAKE=0..3`, `FIGHT_GINGASWORD=0`,
`FIGHT_GINGAPOINTS`, `FIGHT_GINGALAG`. Run through standing, walking in, going round, backing away and three
blows: nothing threw, nothing fell, the blows land as they do in the standard style (the posed blade 1, 2 and 7
cm from the spot). The six fighters' fights are unchanged (78 and 84 blows). Looked at as film strips: the feet
step and the hips sway, the sword sweeps from ahead of him to across his left and back and stays in front of
his body. Not verified in Unity: how it moves, whether two thirds is the right size, whether the sword reads
as alive or as waved about. Known and not done: he does not travel in the ginga itself (the pack's travelling
ginga steps are single steps with turns, not loops; he walks instead); the ginga always begins again from its
first frame where he stops; a blow out of the ginga is the standard style's planted blow (step 5 is the blows
made for it); turning on the spot uses the standard style's turn.

**Movement styles as offshoots of the standard set: the plan** (15 November; no code in the game changed). The
first-pass ginga was judged too busy and plainly a loop. Before building anything else, a plan was written:
<https://claude.ai/artifact/AUmVKUKxhELrbbn3ZL8PJ4>. In short: a style is split into its manner (how the body is
carried, measured as a difference from the standard and laid over any standard motion, with a "style amount"
from 0 to 1), its vocabulary (moves only it has, such as the ginga's four-place step pattern, built from
standard steps instead of played) and its rhythm (tempo, restlessness, variation, as dials). The recordings
become the ruler, not the animation. The steps are S0 to S9 in the plan; nothing is built until it is agreed.

- **The measuring tool** is new: `python3 -I Tools/Mocap/Style/style_measure.py <sample.json>... [--feet]`
  prints a clip's carriage (hips height as a share of the leg, feet apart, lean, twist, where the hands are
  held against the chest), its motion (how far and how fast the hips and hands move) and its footwork (steps a
  second, step length, time on the ground, and with `--feet` where each foot lands). The plan's numbers come
  from it: the sword-and-shield walk is the plain walk plus a carriage; the capoeira carriage is the same across
  six clips; the four ginga takes fit one four-place pattern.

**Movement styles, steps S0 and S1: the profile, the manner layer, and the round trip** (15 November; Combat
Feel "Guard Walk Is Rebuilt From The Plain Walk", off; the duel lab's "His walk: recorded / rebuilt"). The plan
was agreed (all seven decisions; the second style will be the Great Sword pack). Two rules from the playtest go
with it: nothing is to be played as a plain recording any more, everything is rebuilt as the attacks were; and
making the standard set better comes first, so whatever can be rolled into the standard set is.

- **The profile (S0).** A style's manner is now a file of numbers:
  `Assets/_Battler/Resources/Locomotion/StyleProfiles.json`. For each style and each job it was measured in
  (standing, walking forward and back, a step to each side, running) it holds the carriage: the hips' height
  and turn, the chest's turn on the hips, the head's on the chest, which way each arm bone points and how each
  hand is turned, how the spine is bent, and how far each of these goes about its middle. Six styles are in
  it: plain (the relaxed walk), guard (sword and shield), sword, heavy, swagger, capoeira (its stand, from the
  four gingas). It is written by the measuring tool and read by the game; the recordings are only the ruler.
- **The measuring tool** is the headless `style` mode:
  `dotnet Tools/CompileCheck/bin/Tests/Debug/PhysicsBattler.Tests.dll style write` measures the recordings and
  writes the file; without `write` it prints the recordings in plain measures (lean, hips' height, where the
  hands are held, how fast they go), the file, and the round trip below. Its plain measures agree with the
  plan's (`style_measure.py` on the raw samples): the plain walk leans 1 degree with the hands 41 cm below the
  chest going 1.4 to 1.5 m/s; the guard's walk 9 degrees, 13 to 20 cm, 0.3 to 0.4 m/s; the gingas lean 30 to
  41 degrees with the hips at 0.73 to 0.83.
- **The manner layer (S1)** is `StyleManner` (`Locomotion/StyleManner.cs`): a body's parts are taken as their
  tracks, each upper part is moved from where the base style carries it to where the laid style does, what it
  does about that middle is made as big as the laid style has it (the guard's arms move a quarter as far as the
  plain walk's), and the spine is bent as the laid style bends it. The feet's points are not moved at all; the
  knees go on pointing where they pointed; the legs bend to the hips' new place.
- **The round trip (test T0).** The plain walk with the guard's manner laid on, against the guard's own
  recording, at the same moments of the stride (cm apart, root mean square):

  | Walking forward | hips | chest | head | elbows | hands | knees | feet |
  | --- | --- | --- | --- | --- | --- | --- | --- |
  | the plain walk as it is | 1.9 | 4.7 | 11.6 | 20.9 | 41.2 | 8.0 | 7.0 |
  | its carriage laid on | 1.8 | 2.5 | 4.8 | 9.1 | 15.2 | 7.4 | 7.0 |
  | its carriage and its motion | 1.7 | 1.7 | 2.6 | 2.9 | 4.1 | 7.3 | 7.0 |

  Walking back: the hands from 50.0 to 5.0, the head from 16.4 to 4.3. The sword-alone walk and the swagger
  come out the same way (hands 3.6 and 4.6). So the upper body of a style we own is made again from the plain
  walk and about sixty numbers to within 2 to 5 cm. The legs are the plain walk's and stay 7 cm from the
  guard's: its stride is 4 cm longer and its timing a little different. (The plan said the legs match to the
  centimetre; that was the hips' height and range. The feet do not, quite.)
- **What it also showed.** (1) The spine's own bend is part of a carriage: without it the chest was 3.0 and the
  head 4.9 cm off. (An earlier note here said the ginga's back is bent twice as far as the guard's: it is not, on
  average; the ginga's back moves more, which is what that number was.) (2) A carriage belongs to its job:
  the guard's standing carriage (hips turned 41 degrees, sitting at 0.85) laid on the walk is 14 cm off at the
  hips; a pack that lacks a job gets the nearest job's, and that is a real limit. (3) Where the feet stand is
  not manner: the guard's step to the side is a different step from the plain walk's (feet 21 to 28 cm apart
  from it, knees 19 to 38), so those are not rebuilt; stance and steps are vocabulary. (4) The run's arms are
  not got this way (hands 18 cm off): the guard pumps one arm and holds the other.
- **In the lab.** "His walk: recorded / rebuilt" in the duel lab (the standard style), with levers for how much
  carriage and how much motion: walk him to the other and back away, and switch as he goes. Rebuilt, he walks
  forward and back on the plain walk's recordings with the guard's manner from the file; his stand, his steps
  to the sides, his turns and his jog are the guard's own either way. Headless: `FIGHT_WALKREBUILT=1`,
  `FIGHT_REBUILTCARRIAGE`, `FIGHT_REBUILTMOTION`. Run at a walk (1.4 m/s) 4.85 m in and back out, both ways:
  the same distances, nothing thrown, no blade or body through another; as film strips the two are hard to tell
  apart walking in, and backing away the rebuilt one's stride is longer and his sword tips up a little as he
  sets off. The six fighters' fights are unchanged with the switch off (78 and 84 blows). Not verified in
  Unity: how the two compare to the eye, which is the point of the step.
- **Two made blows taken out of the lab's list**, for now: the spinning chop and the spinning backhand, which
  did not work made from their recordings (he turns right round to strike). The best fit never chose them.

New files that need their .meta from Unity: `Locomotion/StyleManner.cs`,
`Resources/Locomotion/StyleProfiles.json`.

**Movement styles, step S2: capoeira's manner over the standard set** (15 November; Combat Feel "Capoeira Style
Amount", 0; the duel lab's lever "Capoeira manner"). The round trip was looked at in Unity and the two walks
could not be told apart. One thing that look could not show, found after: a fighter with a sword or a shield
has its arms placed by the sword's and the shield's own guard, over whatever the walk does, so the two walks'
arms were the same arms. The look confirmed the trunk, the head and the legs; the arms are confirmed in numbers
only.

- **What it is.** One lever, the style amount, from 0 (the standard fighter exactly as he is: nothing is done)
  to 1. He does everything the standard does (his stance, his steps, his walk, his jog, his leaps, his blows)
  and is carried doing it as capoeira carries the body, from its profile: leaned well forward, sitting lower. It
  is laid in the walk (`Locomotor.LayManner`) on each step's pose, for the job he is doing (standing, walking
  forward, back, to a side, running, by the walk's own blend of directions), before a blow on the move is laid
  over it. Every fighter in the standard guard takes it.
- **Measured** (the same duel, standing, walking in, circling, backing away; `Tools/Mocap/Style/film_measure.py`
  on the fight's film):

  | | Standard | Amount 1 | The capoeira recordings |
  | --- | --- | --- | --- |
  | Trunk's lean (degrees) | 11 to 14 | 35 to 38 | 30 to 41 |
  | Hips (m up) standing / walking | 0.83 / 0.88 | 0.76 / 0.78 | (0.73 to 0.83 of standing straight; his come to 0.78 and 0.83) |
  | Planted feet slide (cm/s; the worst) | 1.7 to 6.6 (8.8 to 106) | 1.4 to 6.7 (8.8 to 106) | |
  | Knees, least bent (degrees) | 2 | 15 | |

  The feet are where the standard put them, to the millimetre. Swept from 0 to 1 through the same duel and
  through a fight of six blows: nothing jumps (the head's fastest and its sharpest change from one step to the
  next are the standard's: 2.6 against 2.6 m/s, 2.1 against 1.8).
- **Rules it needed**, each found by something going wrong:
  - *The hips are never turned by a manner.* How the hips are turned on the feet is the stance's (41 degrees in
    the guard's stand, 64 in its side step), and capoeira stands square: turning them would twist the legs.
  - *The lean goes toward where he faces*, not where his hips point: each of the hips, the chest and the head
    is tipped, against the root, from how the standard tips it to how capoeira does. Leaned along the hips, he
    leaned 41 degrees off his man, and the head was swung round at 3 m/s when the hips turned to set off.
  - *The carriage is the job's that the legs are doing*, taken at once with the walk's own switch (which fades
    the whole body, manner and all) and by the walk's own direction. Lagging, or read off his speed as he set
    off, it was the wrong job's for a few steps: the head jumped 5 to 20 cm in a step.
  - *What a part does is made bigger only as far as the job usually takes it* (twice its usual going): a body
    far from the job's middle is on its way between jobs, and that is not the style's to stretch.
  - *A blow from a stand is fitted for the body as the manner will carry it* (`AttackAim.Mannered`: the target is
    carried from the chest as it will be to the chest as recorded, as a blow on the move's is), its arms and
    hands the blow's own. Without that the posed blade was 25 to 33 cm off; with it 1 to 13.
  - *He comes half way up to strike* ("Kept In Blows", 0.5). Leaned right over, a blow at the chest has to be
    thrown upward; the maker then takes the rising cut, and the other's sword hand stops it 60 cm short.
  - *A blow's starting fade carries the manner* (the clip as it was a step ago is laid with it too): without,
    the head jumped at 4 m/s as each blow from a walk began.
- **Blows with it on** (the physical blade, the same batteries as before). From a stand, six at the chest:
  it met the chest in four (12 to 18 cm from the spot) and an arm in two; the standard's six met the chest in
  two and an arm in four. On the move, ten: much as the standard's (the part asked for or the one beside it,
  8 to 31 cm off; one at a far thigh met the arm 53 cm off in both); one thrown from 2.2 m fell 30 cm short,
  as its plan said it would. The six fighters' fight with the manner on all of them: 66 blows in the
  minute, nothing thrown; blades in their own bodies 9.1 s in all (8.0 s without it: the Large's club in its own
  chest, as before). With the amount at 0 the fights are unchanged (78 and 84 blows).
- **The levers** (duel lab, under "Capoeira manner"; Combat Feel "Capoeira Style ..."): *Squares up* (0.5: how
  much the chest and head turn square to the other), *Livelier* (0.5: how much further the body turns and sways
  than the standard's), *Kept in blows* (0.5), *Eyes up* (1: his head faces as the standard's does; at 0 it is
  carried as the recordings carry it, tipped well down), and in Combat Feel *Arms* (shows only on an arm that
  is the recording's own). The lab prints how much manner is on him. Headless: `FIGHT_STYLEAMOUNT`,
  `FIGHT_STYLEMOTION`, `FIGHT_STYLESQUARE`, `FIGHT_STYLEEYES`, `FIGHT_STYLEINBLOWS`, `FIGHT_STYLEARMS`,
  `FIGHT_STYLESWEEP=11` (the amount goes from 0 to 1 over so many seconds), `FIGHT_STYLETRACE=1`.
- **The profile** now has capoeira's carriage travelling forward and back, measured on two recordings that are
  never played: they are baked beside the tool (`Tools/Mocap/Style/style_bake.py` writes
  `Tools/Mocap/Style/RulerClips.json`, outside the game), and `style write` reads them with the game's clips.
  Its carriage is the same standing and travelling (hips 0.90 to 0.93, tipped 26 to 30 degrees).
- **Not verified in Unity**: all of how it looks. **Known and not done**: his arms are the standard guard's
  (the sword's and the shield's own guards place them: step S6); stepping to a side he is carried as capoeira
  walks forward (the pack has no step to a side); in a leap the manner's lean is added to the leap's own
  crouch (58 degrees at the deepest, 32 without); his stance is the guard's, one foot forward, where capoeira
  stands square (the ginga, steps S3 and S4).

**Movement styles, steps S3 and S4: the long step, and the ginga made of its pattern** (15 November; Combat Feel
"Ginga Is Made Of Its Pattern", on; the duel lab's style "Capoeira", "Ginga: made / recorded"). Step S2 was looked
at in Unity: no bugs seen, and it looks bad, as a posture alone on the standard stance and arms was bound to; the
look is to be judged when the footwork and the arms are there with it. These two steps are the footwork.

- **What the ginga is, measured** (headless `style steps`, with `STYLE_PLACES=1` for every tenth of a second;
  fifteen long steps in the four takes). He rests with one foot back: behind the front foot by 0.95 m and
  0.3 m outside it, the hips over the front foot, turned 22 degrees toward the back one, the chest turned back
  the other way so it stays square, the arm on the back foot's side across his face. To change sides the back
  foot sweeps forward to its place beside the front one (the front feet 1.05 m apart); as it lands the other
  foot sweeps back behind it; the hips shuttle across the whole time, pass 25 cm forward of the straight line
  and rise 7 cm, and sink as he arrives. He rests a third of a second and changes back. A long step goes 1.4
  to 2.2 m in 0.38 s a metre, the ankle rising 6 to 7 cm a metre, highest near the end that is behind him.
- **The long step (S3)** is `LongStep` (`Locomotion/StandPattern.cs`), a part of the standard set: where a
  swinging foot's ankle is at each moment of a step from one place to another (how long it takes, how high it
  rises and where, how its path bows away from the standing foot), the standing foot not moved at all.
- **The ginga (S4)** is `StandPattern`: nothing of a recording is played. The profile now holds the ginga's two
  resting poses (`poses` in `StyleProfiles.json`: "GingaRightBack", "GingaLeftBack", measured on the four takes
  where both feet are down and one is well behind). Each change of sides is made as a short clip the walk
  stands in: the feet by the long step; the hips' place, the turns of hips, chest and head and the free arm
  from the one resting pose to the other; everything else the standard stance's own, with capoeira's manner
  laid over it by the walk (all of it: the capoeira style is that style whole). As he comes to stand the
  pattern is laid out about him on the side his feet are nearest and each foot that is off its place steps onto
  it. He walks, jogs and strikes as the standard does, in the manner.
- **The sword stays in its guard.** The first pass gave both arms to the recording, and the sword pointed at the
  ground on one side and at the sky on the other. Now only the free arm is the ginga's; the sword arm is the
  sword's own guard, ready to parry, through the whole of it.
- **The levers** (duel lab, Capoeira; Combat Feel "Ginga ..."): *Restless* (0.4: how soon he changes sides
  again; 1 is the recordings' third of a second, near 0 he stays where he is), *Size* (0.7 of the recordings'),
  *Tempo* (1), *Varies* (0.5: where the feet land, how long he rests, how quick the steps), and a button
  "Change sides now" (turn Restless down and press it to look at one change at a time). The lab prints how many
  changes he has made. Headless: `FIGHT_GINGAMADE=0` (the recorded take), `FIGHT_GINGASTEP`, `FIGHT_GINGATEMPO`,
  `FIGHT_GINGARESTLESS`, `FIGHT_GINGAVARY`.
- **Measured** (`Tools/Mocap/Style/film_steps.py` on the fight's film; half a minute standing):

  | | As set (size 0.7, restless 0.4) | Size 0.9, restless 1 | The recordings |
  | --- | --- | --- | --- |
  | A step goes (m) | 1.05 to 1.16 | 1.34 to 1.49 | 1.41 to 2.16 |
  | and takes (s) | 0.42 to 0.50 | 0.52 to 0.63 | 0.50 to 0.83 |
  | The ankle rises (cm) | 6 to 8 | 6 to 10 | 8 to 17 |
  | From one step to the next (s) | 0.42 to 1.93 | 0.53 to 0.93 | 0.5 to 0.9 |
  | The hips go from side to side (m) | 0.88 | 1.11 | 1.0 to 1.5 |
  | The standing foot slides (cm a step) | 0.0 | 0.0 (0.1 at most) | 0 to 20 |

  At the recordings' own settings it is inside their ranges, but for the ankle's rise, which is a little low.
  The first pass's looped take slid its standing foot 2.8 cm a step (7.5 at most). Blows from the ginga and
  from its walk land as the standard's do (from a stand the chest, 19 cm from the spot; on the move as in step
  S2). Leaps, circling and walking off and back into it: nothing thrown. With the standard style the six
  fighters' fights are unchanged (78 and 84 blows).
- **Not verified in Unity**: all of how it looks. **Known and not done**: every change of sides is the same two
  steps (only their size and timing vary); the back foot stands flat, where the recordings have it on its ball;
  asked to come closer slowly he re-lays the pattern with short quick steps as he goes (step S5 is the ginga
  that travels and turns); the sword arm does not sway with the ginga, and the sword dips on one side; in a
  leap the manner's lean is still added to the leap's crouch.

New file that needs its .meta from Unity: `Locomotion/StandPattern.cs`.

**The two finished tests are off the standard style** (15 November, after a look in Unity). The lab's "Capoeira
manner" lever and "His walk: recorded / rebuilt" stayed where they were last left (the manner at 1, the walk
rebuilt), so the Standard style showed a leaned, sunk fighter. Both were tests of steps S1 and S2 and both are
done, so they are taken out of the lab and out of Combat Feel ("Capoeira Style Amount", "Guard Walk Is Rebuilt
From The Plain Walk" and its two levers): the Standard style is the standard fighter again, with nothing to set.
The manner is the Capoeira style's own, whole; its levers ("Livelier", "Squares up", "Kept in blows", "Eyes up")
are under Capoeira. Headless the tests still run (`FIGHT_STYLEAMOUNT`, `FIGHT_WALKREBUILT`).

**The ginga is made of its recordings' own motion, and is checked against them** (15 November; step S4, its
second way). The first way made the ginga from two rest poses and some numbers measured off the recordings. In
Unity it did not look like the recordings, and nothing I ran could have told me so: I had checked the numbers I
chose, never the look. Two things are changed.

- **A check that sets the fighter beside the recording** (`Tools/Mocap/Style/film_against.py`). A fighter
  standing in the made ginga writes, with each frame of the fight's film, which take he is in and the moment of
  it (and his size, and where the pattern has his hips and ankles). The tool takes the recording at that moment
  and sets the two side by side: a table, part by part, of how far his body is from the recording's (what is off
  the whole time is told apart as his build), and a strip of pictures, the recording above and the fighter
  below, from the front, the side and above. I look at the strip before anything goes to Unity.

  ```
  FIGHT_AIMWHO=open FIGHT_DUELSTYLE=capoeira FIGHT_DUEL="Stand:30" FIGHT_FILM=film.json,0.3,29.8 ... fight
  python3 Tools/Mocap/Style/film_against.py film.json --from 9.6 --to 14.8 --strip out.png --moments 12
  ```

- **The ginga is the takes themselves, edited** (`StandPattern`, rewritten). As an attack is made of a recorded
  strike, every part of the body at every moment is the recording's (the four takes `cap_ginga`, `_b`, `_c`,
  `_d`, taken apart into the parts' tracks once). What is changed: its size (the feet's places and the hips'
  travel), its pace, its rests (at each of a take's two rests, the moment the recorded man is stillest, he
  slows, is held as long as his restlessness lets him and rocks a little on and back through the take's own
  motion there), which take he is in (he goes over to another at a rest, with a 0.4 s fade), and the feet (a
  foot on the ground is held where it was put down; a foot in the air goes the recorded foot's own way, bent to
  leave from where it stood and land where the take has it now).
- **What the check found, that the numbers had not.** In the ginga the body's own rules were laid over the
  recording. "Weight Shifts" drew the hips 40% of the way to the standing foot, so they were up to 13 cm off
  the recording through each step and then went over to the other foot at 3 m/s as the feet changed (the
  recording's most: 2 m/s). The lift of a foot moving low over the ground raised the swinging foot 10 cm a
  second time. Both are left out while he stands in the made ginga (the recorded man shifted his own weight, and
  his foot comes back low to be set down). The rests are now at the moment of each both-feet-down stretch he
  is stillest at (the recorded man all but stops there: hips 0.2 m/s), not at its middle.
- **Measured** (empty hands, the other fighter 3 m off, half a minute standing; cm are the usual distance from
  the recording's motion at the same moment, his build taken out):

  | | As recorded (size 1, restless 1, varies 0) | As set (size 0.8, restless 0.4, varies 0.5) |
  | --- | --- | --- |
  | Head | 1.0 | 2.3 |
  | Hands | 4.6 | 4.9 |
  | Knees | 3.2 | 6.5 |
  | Feet (the recording's made as small) | 4.6 | 6.8 |
  | Hips to head lean (his; the recording's) | 40°; 41° | 32°; 35° |
  | Hips' fastest (m/s) | 2.2 | 2.0 |
  | The standing foot slides (cm a step; at most) | 0.0; 0.0 | 2.2; 7.1 |

  Every lever at both ends (size 0.4 and 1, tempo 0.6 and 1.5, restless 0.05 and 1, varies 0 and 1), walking
  off and back into it, a planted blow out of it (met the chest, 20 cm from the spot), the looped take with the
  switch off: nothing thrown. With the standard style the six fighters' fights are unchanged (78 and 84 blows).
- **Lab** ("Duel lab", "Capoeira"): "Hands: empty / sword" (empty at first, as the recordings were made),
  "Ginga: made / looped", the levers Restless, Size, Tempo, Varies, "Go on now", and two new buttons: "As
  recorded" (the levers set so that he does the recordings themselves) and "As set". A line says which take he
  is in and the moment of it.
- **Not verified in Unity**: all of how it looks. **Known and not done**:
  - *The sword.* With a sword in his hand the sword arm is still the parry guard's, not the ginga's (the blade
    comes within 9 cm of his own middle line with the other fighter 1 m off). It is the next step, on its own.
  - *Close to the other fighter.* The ginga does not keep its distance yet. Stood 1 m from the other, his
    sweeping hand touches him and is knocked 5 to 10 cm for a frame or two. From 1.6 m it does not.
  - *Going over to another take.* For the first step after it his feet are still where the take he left had
    them (up to 20 cm from the new take's), so his legs do not match the recording until he has stepped.
  - *The widest take at full size.* `cap_ginga` at size 1 is wider than his legs reach: the standing foot
    slides up to 16 cm. At 0.8 it is 5 to 7 cm in that take and none in `cap_ginga_b`.
  - *Leaving it to walk.* He walks off from wherever the ginga has him, his hips up to 0.4 m to one side and
    his feet wide: for a tenth of a second both feet are off the ground, and a foot is carried up to 0.5 m.
    The looped take did the same by 0.25 m. Step S5 is the ginga that travels and turns.
  - *The recordings' own seam.* Each take's loop closes at a rest with the hand's swing turned round in one
    frame. With rests he slows into it; "As recorded" shows it as it is.

New file: `Tools/Mocap/Style/film_against.py` (no .meta needed; it is outside Assets).

**The capoeira style is shelved; the standard movement is made of its parts** (16 November; the plan "Fight
Rhythm from Recordings", a step A put in before its step 6).

*Shelved.* The capoeira style did not come to look like its recordings, and the last pass of its ginga played
the recorded takes back, which is not how this project makes motion. The work goes back to the standard style.
Combat Feel "The Capoeira Style Is Shelved" (on): the duel lab has no "Capoeira" choice. Everything made for
the style is kept, untouched and unused (`StandPattern`, `StyleManner`, the profile file, `Tools/Mocap/Style`).

*Step A: the standard movement made of its parts.* Until now only the attacks and the leaps were made of parts;
standing, walking, jogging, stepping, starting, stopping and turning were recordings' points played as they
are. Now they are made the way the attacks are.

- **`MoveScore`** (new). Each movement recording is taken apart once into what every part of the body does
  against the part it sits on (`BodyTracks`: the hips over the ground, the chest on the hips, the head on the
  chest, each arm on the chest, each leg under the hips) and how the back is bent, frame by frame. Where the
  walk asked a recording for its points it now asks the score for the body at that moment, and the body is put
  together from the parts. The walk's own work on top (mixing paces, stretching each foot's stance, planted
  feet, the easing between moves) is as it was. A made attack or a made leap is read as it is.
- **Levers**, each one part's doing about the middle of what it does in that recording (Combat Feel "Move
  Bounce", "Move Sway", "Move Hip Turn", "Move Chest Turn", "Move Arm Swing", "Move Step Lift", "Move Lean",
  "Move Crouch"). Measured on the guard's walk (`style made`):

  | Lever set to | As recorded | With the lever |
  | --- | --- | --- |
  | Bounce 2 | hips rise and fall 7.0 cm | 14.0 cm |
  | Sway 2 | hips side to side 5.5 cm | 11.1 cm |
  | Hip turn 2 | hips turn 15.4° | 30.8° |
  | Chest turn 2 | chest turns 15.5° | 31.0° |
  | Arm swing 2 | hand goes 15.8 cm on the chest | 31.4 cm |
  | Step lift 2 | ankle rises 14.9 cm | 29.8 cm |
  | Lean 15° | hips to head leans 15.1° | 25.6° |
  | Crouch 10 cm | hips 97.4 cm up, knee bent 40° | 87.4 cm, 68° |

- **Lab** ("Duel lab", "How he moves"): "Movement: made / recorded", the eight levers, and three buttons: "At
  rest", and two tries of the levers to see that the parts answer, "Prowl" (lower, leaning in, hips swaying and
  turning more) and "Strut" (upright, bouncing, chest and arms swinging).
- **Measured.**
  - Made against recorded, the 18 recordings of the plain, guard and sword sets, every frame and half way
    between (`style made`): 0.1 to 1.0 mm the usual distance of any point. The furthest are knees: up to 12 mm
    in the walks and 37 mm at one moment of the run, where the recorded leg is stretched longer than its bones.
  - In the duel lab, the same orders with movement made and recorded (stand, walk in, circle, back away): the
    same part at the same moment is 0.016 cm apart as a rule, 4.4 cm at most (a shin, once); 13 frames of 811
    have any part over 1 cm apart.
  - Blows on the move (ten, circling, walking in, jogging in, backing away): the same with movement made as
    recorded. With the "Prowl" levers they land as well (8 to 20 cm from the spot where they meet the part).
  - The six fighters' fights with the switch off are as before (78 and 84 blows). With it on they are other
    fights (66, 93 and 77 blows in three runs; nothing thrown): a tenth of a millimetre is enough to send a
    60-second fight another way.
- **Not verified in Unity**: all of how it looks. **Known**:
  - Mixing two paces (a walk into a jog) is still done on the put-together bodies, point by point, not on the
    parts.
  - The levers are one setting for every fighter; they are not yet a style a unit carries.
  - A lever changes the movement, not the attacks: with Crouch or Lean set, a planted blow comes up to the
    attack's own height and the easing between them covers the difference.
  - "Arm swing" shows only on an arm that is the walk's own: near an enemy the sword and shield hands are
    placed by the guard.
  - Measures read off recordings once (a stance's foot places, the guard's held arm) still read the points.

New file that needs its .meta from Unity: `Locomotion/MoveScore.cs`.

**A blow asked for on the move stays on the move when it is out of reach** (16 November, from the duel lab:
going round the other from 4 m, every blow stopped him). Out of the walk's reach a blow was thrown planted,
with its lunge. Now it is thrown on the move all the same and misses; whether to strike at all from out of
reach is the fighter's to judge when the fight is built on this (the plan's step 8). Combat Feel "A Blow On
The Move Stays On The Move Out Of Reach" (on); in the duel lab, under "He strikes: on the move", "Out of reach:
on the move / plants". Headless (`FIGHT_SHORTPLANTS=1` for the old way), four blows going round at 4 m: he
keeps going at 1.5 to 1.6 m/s through each (the old way: down to 0.1 to 0.2 m/s), nothing thrown; the lab's
line says "out of reach, 190 cm short: thrown on the move". The six fighters' fights are unchanged.

**Fight rhythm, step 6, first part: the feint and the fake lunge** (16 November; `PostureMaker`). Postures are
the short bursts a fighter throws between blows to draw the other out. The first two, one for the upper body
and one for the lower, and the two together.

- **A posture is the beginning of a real attack, taken back.** The attack is made for its target as any attack
  is (`AttackMaker`), so a posture starts exactly as that blow would. Its body is then taken apart into its
  parts again and the parts are sent two ways, each part of the way into the attack and back the way it came:
  the upper body (the chest on the hips, the head, the arms, the hands) into the wind-up, and the lower body
  (the hips, the legs, the body's travel) into the lunge. The chest keeps facing as its own moment of the
  attack has it, whatever the hips under it do.
  - *A feint*: the whole body goes into the wind-up at the attack's own pace, to 0.85 of the way to where the
    hips would start in, slows, is held 0.06 s and comes back in 0.45 s. On the move it is laid over the walk,
    which goes on unchecked.
  - *A fake lunge*: the legs and hips go as far into the attack as carries the hips 0.3 m, in 0.28 s (sharply
    off, slowing all the way), and come back in 0.4 s. The arms are left to the guard. From a walk he stops for
    it.
  - *Both*: a feint with the legs going into the lunge under it, the two at their furthest together.
- **Lab** ("Duel lab", "Strike at"): "A press is: a blow / a feint / a fake lunge / both", then press a part of
  the body. The levers for the one chosen are under it (Combat Feel "Feint Depth", "Feint Hold", "Feint Back",
  "Fake Lunge Far", "Fake Lunge Out", "Fake Lunge Back"). The lab's line says what was thrown, when it was at
  its furthest and when it was over. Headless: `FIGHT_POSTURE=feint | lunge | both` with `FIGHT_DUELBLOW`.
- **Measured** (standing 1.8 m off, at the chest, on the forehand cut; and going round at 1.9 m):

  | | A feint | A fake lunge | Both |
  | --- | --- | --- | --- |
  | Over in (s) | 1.03 | 0.77 | 1.03 |
  | At its furthest after (s) | 0.47 | 0.28 | 0.47 |
  | The sword hand goes (cm) | 68 | 50 (carried by the body) | 68 |
  | The hips go (cm); fastest (m/s) | 29; 1.8 | 38; 2.6 | 31; 2.1 |
  | The front foot steps (cm) | 39 | 45 | 41 |
  | Back where it began, to (cm) | 1 to 4 | 1 to 4 | 1 to 5 |

  A feint's sword hand is where the real blow's is, to 3 cm, for its first 0.38 s. Nothing is struck by any of
  them. On the move the feint is thrown over the walk (1.05 and 1.13 s) and he keeps going. Nothing thrown in
  any run; the six fighters' fights with the made movement off are unchanged (78 and 84 blows).
- **Not verified in Unity**: all of how they look. **Known**:
  - A posture is played as a strike is, so the log writes it as an attack that "met nothing".
  - Which attack a posture is the start of is the one that would have been made for that part from there; the
    forehand's wind-up already steps in 39 cm, so on it a feint and "both" look much alike.
  - Not yet: the other postures of the plan (hop in and out, side hop, stance switch, drop; blade bluff, shield
    press, guard change), and the body map showing which parts each posture opens.

New file that needs its .meta from Unity: `Locomotion/PostureMaker.cs`.

**A fake lunge is thrown on the move** (16 November, from the duel lab: asked for while he walked, it stopped
him, where a feint goes over the walk). Now, walking, the walk's legs go on under it: his hips dart toward the
other (0.7 of the lunge's distance, as far as his walking legs let them) and sink as the lunge's do, and come
back; his feet keep to their way. From a stand it is the lunge on the attack's own legs, as before. Combat Feel
"A Fake Lunge Is Thrown On The Move" (on) and "Fake Lunge On The Move"; in the lab, under the fake lunge's
levers, "Walking: on the move / stops" and "Darts in". Headless (`FIGHT_LUNGESTOPS=1` for the old way), going
round at 1.9 m: he keeps going at 1.5 m/s through it and is never slower than 1.45 (the old way: to a stop);
his hips go 27 cm toward the other at 0.28 s and are back where they were, and sink 11 to 14 cm. With a feint
on it the same, at their furthest together. Nothing thrown; the six fighters' fights are unchanged.

*Seen in the playtest of 08:57, in the log and in its pictures:* at a forehand feint's furthest point the
blade lies against his own head for 0.18 s ("what its right hand holds was inside its own body"). The real cut
whips through that place; the feint slows and is held in it. Not mended yet.

*The playtests' pictures* (448 px, 8 a second, `PlaytestLogs/*/frames`): they are looked at for the moments a
playtest flags, and then taken out of the repository (131 000 of them were in it, 3.3 GB with the logs).

**Fight rhythm, step 6, second part: the drop; and a feint keeps its blade off his own head** (16 November).

- **The drop**: a sudden dip of the hips, as if loading to spring, and back up, over whatever he is doing.
  It is not a clip: it is laid for a moment on the parts his movement is made of (`MoveScore`'s levers, his
  own for as long as it lasts; `Locomotor.Dip`), so his feet stay where they are and his knees bend for it,
  standing or walking. Combat Feel "Drop Sinks" (10 cm), "Drop Leans" (8°), "Drop Down" (0.12 s), "Drop Up"
  (0.3 s); in the duel lab, "Drop" with its levers, under the leaps. Headless (`FIGHT_DROPAT="2,5"`): standing,
  his hips sink 10 cm, lowest 0.12 to 0.17 s after, and are back to under 1 cm; his lean goes from 10° to 16°;
  his feet move 0.2 cm. Going round at 1.5 m/s the same (12 cm), and he keeps going.
- **The hops** of the plan (hop in, hop out, side hop) are the lab's Leap in, Leap out and Hop left and right
  with "Leap" set to 0.3 to 0.6 m: made already, in step 2.
- **A feint stops short of where its blade would rest against his own head** (the playtest of 08:57: a forehand
  feint from 1.3 m held the blade against his head for 0.18 s; the real cut whips through there). The maker
  looks at the moment it would stop at: if the blade is nearer the middle of his head, neck or chest than 0.18 m
  there, it stops at the last moment of the wind-up the blade is clear (Combat Feel "Feint Blade Clear"; 0: as
  it was; in the lab under the feint's levers). Headless, the forehand feint from 1.3 m: it stops 0.39 s in
  where it stopped 0.46 s in, and the blade is 27 cm from the middle of his head at its nearest, where it was
  12 cm (touching it). The rising cut's feint, which was clear, is as it was.
- The six fighters' fights with the made movement off: unchanged (84 blows). Nothing thrown in any run.
- **Not verified in Unity**: all of how these look; and whether the log's "inside its own body" is gone (the
  headless run did not write it before the change either, only showed the blade touching).
- **Still to come in step 6**: the upper body's postures that go through the guard's own arms (blade bluff,
  shield press, guard change), the stance switch, and the body map showing which parts each posture opens.

**Fight rhythm, step 6, third part: the blade's bluffs, the shield press and the change of guard** (16
November). The upper body's postures that are not the start of an attack.

- **How.** Like the drop, none is a clip: each is laid for a moment on the parts his movement is made of
  (`MoveScore`'s levers, his own while it lasts; `Locomotor.Throw`, `Locomotor.GuardLift`), so it goes over
  whatever he is doing, standing or walking.
  - *Blade dip, flick, circle*: the sword hand is turned on the chest so the point dips (38°), goes aside (32°)
    or goes once round (22°), and comes back; the forearm takes a third of it.
  - *Shield press*: the shield arm is straightened forward of the chest and brought back.
  - *Guard: high / middle / low*: the whole sword arm turned at the shoulder, 35° up or 25° down, and kept
    there until it is changed (he goes to it at 140° a second).
- **Lab** ("Duel lab", "Sword and shield"): "Blade dip", "Blade flick", "Blade circle", "Shield press"; "Guard:
  high / middle / low"; the levers "Bluff size" and "Press far" (Combat Feel "Blade Bluff Size", "Shield Press
  Far", "Guard High Lift", "Guard Low Drop"). Headless: `FIGHT_BLUFFAT="2:dip,3.5:flick,5:circle,6.5:press"`,
  `FIGHT_GUARDAT="8:high,10:low"`, with `FIGHT_AIMWHO=shield`.
- **Measured** (sword and shield, standing 1.8 m off; going round at 1.6 m/s the same to a few cm):

  | | Goes | Furthest after | Back to |
  | --- | --- | --- | --- |
  | Blade dip: the point | 54 cm (47 down) | 0.12 s | 6 cm |
  | Blade flick: the point | 33 cm (32 aside) | 0.10 s | 6 cm |
  | Blade circle: the point | 28 cm round | 0.35 s | 6 cm |
  | Shield press: the shield hand | 24 cm (18 forward) | 0.18 s | 1 cm |
  | Guard high: the point | 30 cm higher (the hand 14) | | kept |
  | Guard low: the point | 59 cm lower (the hand 17) | | kept |

  Nothing thrown; no blade in his own body; the six fighters' fights with the made movement off are unchanged
  (84 blows).
- **Found on the way.** The headless duel's swordsman has a sword alone unless `FIGHT_AIMWHO=shield` is given;
  the lab's has sword and shield. A fighter with a sword alone has both arms placed by the sword's own guard
  (`SwordParry`, the stage "the parry and the free arm"), over whatever the movement gives them: on him these
  postures do not show. The feint, the fake lunge and the drop were measured on the sword alone; they do not go
  through the guard's arms, and the lab showed them on sword and shield. A new probe says where the hands are
  after each stage of the pose (`FIGHT_ARMPROBE=1`); the film now carries the walk's own hands (`rawh`).
- **Not verified in Unity**: all of how they look. **Known**:
  - On a fighter with a sword alone the bluffs and the change of guard do nothing yet (above).
  - A low guard drops the point 59 cm, to near the ground; the lever is there.
  - In a made attack or a leap none of these act: they are laid on his movement, not on the attack.
- **Still to come in step 6**: the stance switch, and the body map showing which parts each posture opens.
- **For the dance (step 8), from Renier:** the postures' levers are not to be one setting, nor plain chance: a
  fighter varies them in patterns that mean something. One makes many small drops and one deep one before he
  attacks; the other can learn that, and it is an opening.

**Fight rhythm, step 6, fourth part: what he leaves open, measured** (16 November; `Openings`). The plan's
"openings are measured, not hand-marked", as far as step 6 needs it: a map of what each posture opens.

- **Open** is looked at on the bodies as they are. A spot on his body (the aim test's spots) is open to the
  other when a straight line from the other's sword shoulder reaches it without first meeting his shield or
  another part of him; otherwise it is covered, and by what. The line is looked along in 3 cm steps over its last
  0.9 m (`Openings.Look`). A blade is not counted as cover.
- **Lab** ("Duel lab", "What he leaves open"): his body as a map, each spot green where it is open and grey
  with what covers it where it is not, ten times a second. Under it, "Last posture": for 1.4 s after any
  posture or blow asked for in the lab, what it opened that was covered and covered that was open, and when.
- **Measured** (sword and shield, standing 1.8 m off; seconds from when it was asked for):

  | | Opens | Covers |
  | --- | --- | --- |
  | Standing in his guard | head, neck, both shoulders, shins and feet are open; the shield covers chest, belly, hips, both arms, both thighs | |
  | Blade dip | nothing | nothing |
  | Shield press | left upper arm 0.07 to 0.40, left thigh and hips 0.10 to 0.33, chest 0.13 to 0.26 | right shoulder 0.13 to 0.30 |
  | Drop | chest 0.10 to 0.33, left upper arm 0.13 to 0.30 | right shin 0.10 to 0.33 |
  | Feint (forehand, at the chest) | sword arm and right thigh 0.13 to 0.75, chest and hips 0.20 to 0.66, belly 0.23 to 0.63, left upper arm and thigh 0.26 to 0.60 | left shoulder 0.69 to 0.79 |

  The feint's three repeats agree to 0.03 s. Nothing thrown; the six fighters' fights with the made movement
  off are unchanged (84 blows). Headless the lines begin `OPEN` and `OPENINGS`.
- **Not verified in Unity**: the map in the panel (how it is laid out and coloured), and all of the above on
  the lab's own bodies.
- **Known**: it is seen from one place, the other's sword shoulder, not along the way a particular blade would
  come (the aim test's own check does that for a blow about to be thrown); a sword held across the body does
  not count as cover; nobody acts on any of this yet. That is step 7.
- Step 6 is done but for the stance switch, which needs a guard with the other foot forward and is left out.

New file that needs its .meta from Unity: `Locomotion/Openings.cs`.

**Fight rhythm, step 7, first part: the shield test** (16 November; `OpeningTaker`, `ShieldTest`,
`ClipFighter.Uncover`). The plan's "how an attacker takes one", on the test Renier described: the target holds
a shield and uncovers a chosen part for a moment; the swordsman must hit that part while it is open.

- **The target uncovers** (`ClipFighter.Uncover`). First its tell, 0.3 s: the shield is drawn a little the
  other way (9 degrees). Then its shield arm swings at the shoulder, up or down, whichever leaves the board
  further from the straight line between the swordsman's sword shoulder and the part; it goes until that line
  is clear of the board by 7 cm, looked at on the bodies each step (so how far it swings is not set: 36 to 78
  degrees in the runs below); it stays away until the part has been open for the time asked, 0.4 s, and comes
  back. When the part was open is measured the same way as step 6's map (`Openings.LookAt`), three steps
  running to count. The swing is laid on the parts his movement is made of (two new levers, the shield arm
  aside and lifted), so it needs the made movement. While he uncovers, his shield does not go to meet a blow.
- **The swordsman takes it** (`OpeningTaker`), one of two ways, and two numbers decide which and how well:
  whether he **reads** (Combat Feel "The Swordsman Reads Openings"), and his **reaction time** ("Opening
  Reaction Time", 0.2 s).
  - *Reading*, he recognises the uncovering as it starts, knows from the last time he saw it when the part
    opened and shut, and starts his blow after his reaction time, made to land 40% of the way into the window
    (the attack maker lands a blow at a set moment). A blow may be hurried no further than "Quickest Opening
    Blow", 0.45 s. If it could not land before 90% of the window he holds it.
  - *Only reacting*, he starts his blow his reaction time after he sees the part open, as quick as a blow may
    be.
  - He cannot read an uncovering he has not seen: the first time, a reader only reacts, and what was measured
    then is what he goes by after. Change the part, the tell or the open time and it is new to him. A go in
    which his own blow landed while it was open does not teach him (a struck body shuts differently).
- **Lab** ("Duel lab", "Shield test", at the top): "Set out" (the target with sword and shield, the two 1.5 m
  apart), "Uncover" (once), "Loop"; "Uncovers: chest / belly / hips"; "Swordsman: reads it / only reacts",
  "Forget"; levers "Reaction", "Quickest blow", "Tell lasts", "Open for". Under them: the part as he sees it
  now and whether he has seen this uncovering; the last go (the window as measured, when the blow started,
  what it met and when, what he went by); and the count.
- **Measured** (headless, both with sword and shield, 1.5 m apart, reaction 0.2 s, tell 0.3 s, open for 0.4 s;
  times are seconds after the uncovering began):

  | | Goes | The blade arrived | Met |
  | --- | --- | --- | --- |
  | Only reacts (chest, belly, hips, two each) | 6 | 1.13 to 1.22, which is 0.22 to 0.37 after it shut | the shield, 6 of 6 |
  | Reads, first sight of each part (he can only react) | 3 | 1.10 to 1.15, 0.23 to 0.37 after it shut | the shield, 3 of 3 |
  | Reads, having seen it | 7 | 0.68 to 0.73, inside the window every time | the part asked for 3 times (chest twice, hips once); a part beside it 4 times; the shield never |
  | Reads, reaction 0.4 s | 2 | no blow | he held it: it could not land before 0.85 s and the part shuts at 0.82 |
  | Reads, no tell | 2 | no blow | he held it: the part is open 0.13 to 0.52 and his blow cannot land before 0.65 |
  | Only reacts, open for 0.9 s | 3 | 1.07 to 1.08, inside the window | the chest twice, the left upper arm once |

  The windows measured 0.35 to 0.50 s for the 0.4 asked (the chest 0.35 to 0.40). Nothing thrown; the six
  fighters' fight with the made movement off is unchanged (84 blows, 3 against 2). Headless:
  `FIGHT_SHIELDTEST="3:Chest,3:Chest,3:Belly"` in the duel lineup with `FIGHT_AIMKIT=shield`, and
  `FIGHT_READS=0`, `FIGHT_REACTION`, `FIGHT_TELL`, `FIGHT_OPENFOR`, `FIGHT_QUICKEST`; the lines begin
  `SHIELDTEST`.
- **Not verified in Unity**: all of it. The panel's layout; what the tell and the swing look like on the lab's
  bodies (headless the film draws no shield); whether the lab's bodies give the same windows.
- **Known**:
  - A blow hurried to 0.45 s is less true than one in its own time (0.62 s): reading, 4 of 7 landed on a part
    beside the one asked for (its sword hand for its chest; its left upper arm and its hips for its belly; its
    belly for its hips). They were past the shield and inside the window; the aim is the made attack's, not this step's.
  - Open is a straight line from the sword shoulder; a cut comes round. So the thighs are left out of the test
    (a low cut comes round under the board and lands after the line says shut), and the shield is never swung
    aside (that clears the line but puts the board where a forehand comes from).
  - With no tell there is nothing to read: the reader gains nothing over reacting, and knows it.
  - The reading is all or nothing (a switch), not a chance per posture met; the swordsman stands still for
    it; the target only does it when told. A fighter's own reading number, the real postures of step 6 in
    place of the scripted shield, and the fight using any of this are the next parts.

New files that need their .meta from Unity: `Locomotion/OpeningTaker.cs`, `Sandbox/ShieldTest.cs`.

**Fight rhythm, step 7, first part, after the first look in Unity** (16 November). Renier could not tell the
tell from the uncovering, thought the tell came a second or more early, and did not like the shield being
pulled into the body. This entry replaces the uncovering's description and the table in the entry above.

- **What the playtest showed** (the target's shield hand against its chest, from the pose log; the first go,
  the blow asked at 40.00 s):
  - *The tell could not be seen.* It moved the shield hand 3 cm for 0.3 s. Standing, that hand already drifts
    3 cm up and down with his breathing.
  - *The uncovering drew the shield in.* The arm was swung about the shoulder with the elbow bent, so the hand
    went 10 cm down and 19 cm back toward the chest, and the board tipped edge on.
  - *What read as a second move was the block.* The shield was down and in for about 0.3 s (39.8 to 40.1 s);
    then, the swordsman only reacting, it went 40 cm up to meet his late blow (40.3 to 40.8 s). That is the
    bigger move of the two, and it comes half a second after the uncovering.
  - The timing itself was as built: tell 0.3 s, away 0.3 s. Nothing ran a second long.
  - The shield test wrote nothing to the playtest log. It does now (`uncover`, `shieldtest`).
- **Changed**:
  - *The tell is a gesture of his sword*: the point of his blade dips about 50 cm and is held down for as
    long as the tell lasts (0.3 s), and comes back up as the shield goes. Sword for the tell, shield for the
    uncovering, so they cannot be mixed up.
  - *The shield is carried straight down*, still facing the swordsman, the elbow straightening for it (a new
    lever, the shield hand's shift, in place of the two swing levers): the hand goes about 24 cm straight down
    as seen (27 to 33 asked) and stays 41 to 43 cm before the chest throughout. It leaves 15 cm of room about the line (7 before), which a blade
    needs.
  - *The chest only.* The belly and hips sit behind the middle of the board and no clean move of the shield
    clears them (down 60 cm does not; aside to his left it is still between the two); they are left to the
    real postures.
  - *What is happening is written over the two as it happens*: over the target "TELL: his blade dips", then
    "CHEST OPEN"; over the swordsman "read the tell", "READ IT: striking" or "SAW IT OPEN: striking", and how
    it came out ("TAKEN", "TOO LATE: met the shield").
- **Measured again** (headless, the chest; reaction 0.2 s, tell 0.3 s, open for 0.4 s; seconds after it began):

  | | Goes | The blade arrived | Met |
  | --- | --- | --- | --- |
  | Only reacts | 3 | 1.05 to 1.12, 0.27 to 0.33 after it shut | the shield, 3 of 3 |
  | Reads, first sight | 1 | 1.12, 0.32 after it shut | the shield |
  | Reads, having seen it | 3 | 0.67 to 0.73, inside the window | the chest twice, the belly once |
  | Reads, reaction 0.4 s | 2 | no blow | he held it |

  The window measured 0.30 to 0.43 s. The six fighters' fight with the made movement off is unchanged (84
  blows, 3 against 2). Fewer goes than the first table: one part, not three.
- **From Renier, for what comes next**: the real tells are the fighters' gestures. A fighter's standing is
  broken up by gestures all the time, and it is their pattern that tells: most of his fake slashes are short,
  and the one before he really attacks is long. So reading is recognising the pattern, not a signal made for
  the purpose. The blade dip here stands in for that until the target plays the real postures.
- **Not verified in Unity**: the new tell and shield move, and the writing over the fighters.

**Fight rhythm, step 7, second part: the fakes test; a fake commits the shield** (16 November; `FakeTest`,
`OpeningTaker.Foresee`, `ClipFighter.PostureCommitsShield`). The shield test on the real postures of step 6 in
place of the scripted shield, with the tell where Renier put it: in the pattern of a fighter's gestures.

- **The target's fakes.** It throws fake slashes at the swordsman: feints made of the forehand cut as any feint
  is, only how deep they go and how long they are held differing. Measured one at a time, on a settled target
  (seconds after each began):

  | | Furthest at | Over in | Its chest open |
  | --- | --- | --- | --- |
  | Short fake (0.45 of a feint's depth, not held) | 0.24 | 0.73 | never (5 of 5) |
  | Long fake (full depth, held 0.25 s) | 0.37 | 1.11 | 0.27 to 0.30 until 0.75: about 0.47 s (4 of 4) |
  | Long fake, held 0.06 s | 0.37 | 0.93 | 0.32 to 0.38 until 0.57: about 0.2 s |

- **Its pattern**: so many short fakes (2), then a long one, a fake beginning every 1.5 s, round and round.
  Struck, or off its spot, it begins again once it is back and 1.2 s have passed.
- **A fake commits the shield** (Combat Feel "A Fake Commits The Shield", on). While a posture of his own plays
  (a feint, a fake lunge) his shield stays where the posture has it and does not go to meet a blow. Before
  this the shield met a blow in the middle of his own fake as readily as from his guard, so a fake cost nothing
  and the opening it left was only for show. Real blows are untouched: a fighter still blocks while he strikes.
- **The swordsman.**
  - *Only reacting*, he goes for the chest whenever he sees it open, as quick as a blow may be.
  - *Reading*, he first watches the pattern through once without striking. What he learns is what he saw:
    how many short fakes come before the long one, how long after the last of them it begins, and when it has
    the chest open. After that he lets the short ones go, counts them, and as the last short one begins he
    foresees the opening (`OpeningTaker.Foresee`): his blow is thrown in its own time (0.62 s; hurried only
    when there is not time for that), made to land 40% of the way into the window. It starts before the long
    fake does.
- **Lab** ("Duel lab", "Fakes test", under the shield test; "Set out" first): "Short fake", "Long fake" (one,
  now), "Pattern" (round and round); "Its pattern: steady / mixed up"; "Short ones", "Beat", "Long holds";
  "In a fake: shield committed / shield free (old)". Under them: what he has learned of the pattern, the last
  fake and what it opened, the last blow, and the count. Over the two as it happens: "short fake", "LONG FAKE:
  CHEST OPEN"; "watching: learning his pattern", "reads: lets the short fake go", "READ THE PATTERN: the long
  one is next", "READ THE PATTERN: striking", and how it came out.
- **Measured** (headless, about 45 s of the pattern each, both with sword and shield, 1.5 m apart, reaction
  0.2 s):

  | Swordsman | Blows | On the chest while open | In time, beside it | Other |
  | --- | --- | --- | --- | --- |
  | Reads, steady pattern | 8 | 4 | 3 (the left upper arm) | 1 met nothing; none on the shield, none at a short fake |
  | Only reacts, steady pattern | 16 | 0 | 0 | 12 at short fakes (all on the shield); 3 too late at a long one (one of them still reached the chest after it was covered); 1 met nothing |
  | Reads, pattern mixed up (1 to 3 short ones) | 5 | 1 | 0 | 4 thrown when it was not open (3 on the shield) |
  | Reads, steady, shield free in a fake (old) | 7 | 0 | 1 | 6 when it was covered (5 on the shield) |

  Reading, each blow began 1.43 s after the last short fake did, 0.08 s before the long one. Nothing thrown;
  the six fighters' fight with the made movement off is unchanged (84 blows, 3 against 2). Headless:
  `FIGHT_PATTERN=3` (and `FIGHT_PATTERNSHORTS`, `FIGHT_BEAT`, `FIGHT_MIXED=1`, `FIGHT_FAKECOMMITS=0`,
  `FIGHT_FAKES="3:short,5.5:long"`); the lines begin `FAKE`, `FAKEBLOW`, `LEARNED`, `FAKETALLY`.
- **Not verified in Unity**: all of it: the panel, the writing over the two, how a short and a long fake look
  on the target, and whether the lab's bodies give the same windows.
- **Known**:
  - In the first, undisturbed round the short fakes open nothing, and one who only reacts leaves them alone.
    He goes for them later: after his own blows have moved him and the target off their line, he sees the
    chest past the shield for a moment in a short fake too. So "wasted on a short fake" is partly his own
    doing.
  - Reading, 3 of 8 landed on the left upper arm, beside the chest: the made attack's aim.
  - The reader counts. Against a mixed-up pattern counting is wrong as often as right, and he has nothing
    else yet: he does not tell a long fake from a short one as it goes on.
  - The swordsman stands still, and the target neither strikes nor answers. Both fighters doing all of this to
    each other is step 8.

New file that needs its .meta from Unity: `Sandbox/FakeTest.cs`.

**The shield test and the fakes test set the lab out themselves** (16 November, from the playtest: "the dummy
does not have a shield"). Both tests need the target standing with sword and shield, and the duel lab starts
it with open hands. Pressed with no shield on the target, any button of either test (Uncover, Loop, Short
fake, Long fake, Pattern) now sets the lab out first, and what was pressed follows once the two stand ready
(1.5 s). A line over the fakes test's buttons says whether the target has its sword and shield. Not verified
in Unity.

**Downed play: the plan** (16 November). What two fighters do when one of them has been knocked down is a
document of its own, "Downed Play plan (knockdown actions)" (https://claude.ai/artifact/GnbHLxWQTbosPE5Fff3oAK):
the follow-up of the one left standing (a stab, a low slash, a kick, or a victory lap), the answer of the one
on the ground (rolling clear to face up, guarding from his back, getting up facing his enemy, rolling into the
legs), the three packs of recordings measured (kicks, rolls, get-ups), how it is built on the made attacks and
a new score for a body lying down, and ten steps. Nothing of it is built.

**Fight rhythm, step 8, the dance, first part: each fighter has levers of his own** (16 November). Until now
there was one set of levers for every fighter on the field: the movement levers of "How he moves" and the
sizes of the postures. Two fighters who are to dance must be told apart by how each carries himself, so each
now has his own (`FighterLevers`, kept on his walk as `Locomotor.Own`), laid on everyone's:
- how he moves: bounce, sway, hip turn, chest turn, arm swing and step lift, each times everyone's; lean and
  crouch added to everyone's;
- how big his postures are: his feints' depth, his fake lunges' distance, his drops' depth, his bluffs' size
  and his changes of guard, each times everyone's.

A fighter with none, or with all at rest, moves as everyone does. Three made-up fighters to try it with:
Prowler (low, leaning in, level, feet near the ground, small gestures, a deep drop), Strutter (upright,
bouncing, chest and arms swinging wide, feet lifted high, big gestures) and Bull (heavy and square, a long
fake lunge, few frills). They are tries of the levers, not styles decided on.

Switch: Combat Feel "Each Fighter Has His Own Levers" (on). Off: every fighter moves by everyone's alone, as
it was. In the duel lab, "Each his own": the old/new toggle, a row of the made-up fighters for him and one
for the other, and every lever of the one shown to set by hand. The one you move shows all of it; the other
only stands, so he shows his lean and crouch (and his feints, in the fakes test). What was picked is kept
when the lab is set out again.

Measured headless on the body as posed, the mover circling at 1.6 m/s (`FIGHT_OWN="Prowler,Strutter"`: the
mover's and the other's; `FIGHT_OWNON=0`: the switch off):

| the mover | hips carried | rise and fall | chest leans | feet lifted to | his drop sinks |
|---|---|---|---|---|---|
| Plain | 79.0 cm | 11.8 cm | 12.6° | 21.7 cm | 10.1 cm |
| Prowler | 73.0 cm | 6.5 cm | 18.0° | 17.7 cm | 15.0 cm |
| Strutter | 79.7 cm | 12.9 cm | 10.1° | 29.6 cm | 6.2 cm |
| Bull | 77.1 cm | 9.1 cm | 15.7° | 19.7 cm | 10.1 cm |
| Prowler, switch off | 79.0 cm | 11.8 cm | 12.6° | 21.7 cm | 10.1 cm |

The other, standing: as a Strutter he leans back 5° more than with the switch off; as a Prowler he stands
7.5 cm lower and leans 10° further in. With the switch off a fighter given levers is the plain one to the
millimetre.

Not verified: nothing of it seen in Unity; how the three look (only stick-figure strips were looked at); the
feint, fake lunge, bluff and guard levers were not measured, only the drop (the others are the same
multiplication). The fight itself gives no fighter levers of his own yet: only the duel lab does. Next in the
dance: each fighter's gestures in a pattern of his own, and each reading the other.

New file that needs its .meta from Unity: `Locomotion/FighterLevers.cs`.

**From the playtest of 16 November (evening): a walk kept up is walked; the actions window; readouts that do
not move the panel.**

*A walk kept up is walked, not stepped.* Under a walk's pace (about 0.9 m/s) every move was stood and stepped
in the stance: single short steps, made for a small change of where a fighter stands. At the duel lab's lowest
pace (his guard's own, 0.7 m/s) that is what he circled and walked up on. Now a fighter told to walk on (the
lab's Walk to, Back away, Circle) walks at any pace on his walk's own cycles, their rhythm slowed with the
pace (`Locomotor.WalksOn`, `ClipFighter.WalksOn`). Stepping is left for small changes of place. Switch: Combat
Feel "A Walk Kept Up Is Walked Not Stepped" (on); in the lab, "Slow orders: walked / stepped (old)".

The pack "Slow walks" (eight files, two of them doubles), measured:

| recording | its own pace | used |
|---|---|---|
| Walk Strafe Right | 0.62 m/s, upright, feet near each other | a choice for the slow circle (mirrored for the left) |
| Crouched Sneaking Left / Right | 1.06 / 1.13 m/s, low, each foot long on the ground | the slow circle, as set |
| Sneaking Forward | 2.2 m/s | no: faster than the guard's own walk (1.5 m/s) |
| Sneak Walk (two takes) | 2.0 and 2.6 m/s | no: the same |

So the pack gives a slow walk to the sides and none forward or back: walking up and backing away slowly are
the guard's own walk and walk back, slowed. A slow forward and a slow backward recording are still wanted.
The side recordings are used as the jogs are: their hips and legs under the guard's own chest and arms
(`MoveStyle`, `ClipSet.SneakL/R`, `SlowL/R`; baked as `sneak_left/right`, `slow_left/right`). Which one a slow
circle is walked on: Combat Feel "Slow Side Walk" (1: crouched, as set; 2: upright; 0: the guard's side-step
slowed); in the lab, "Slow circle". The slow recording is all of it at about 0.75 m/s and less and gives way
to the guard's side-step as he nears that one's pace.

Headless, circling at 0.7 m/s: he walks (the walk's cycle) where he stood and stepped; he covers the same
ground (3.1 m in 5 s); his feet slide 1.9 cm a second, as before (1.8). Not verified: nothing seen in Unity;
which of the three slow circles looks right (on stick figures the crouched one is a deep crouch, well under
his guard's stance; the upright one stands him up with his feet together; the side-step slowed looks most
like his guard). Only the duel lab's orders tell a fighter to walk on: the fight's own steering does not yet.

*The actions window.* In the duel lab every button that has a fighter do something is in a window of its own,
"Actions", at the right of the screen (it can be dragged): the pace at its top, then Move him and the leaps,
the postures, Strike at with its body map, and the buttons of the shield test and the fakes test. The panel
keeps the settings and what was measured. "Actions: in a window / in the panel" at the top of the duel lab
puts them back as they were; the choice is kept between plays.

*Readouts keep their height.* The lines that say what was measured (the shield test's "Now" and "Last go",
the fakes test's lines, the last blow and its start, the order, the last leap and posture, and the line at
the foot of the panel) are each a set number of lines tall whatever they say, so the panel under them no
longer jumps as their words change. A line longer than its room is cut off; its full text is in the log.
The pace's value no longer runs onto a second line.

New file that needs nothing from Unity: `Tools/Mocap/Combat/samples/*.json` (three samples). No new scripts.

**Fight rhythm, step 8, the dance, second part: two fighters gesturing in patterns of their own, each
reading the other** (17 November, built overnight without a playtest). The shield test and the fakes test had
one who gestured and one who struck. Here both do both (`Sandbox/Dance.cs`).

*His pattern.* Each fighter throws gestures on a beat of his own, then the one that comes before his real
blow (his tell), then the blow (`GesturePattern`). The four made-up fighters each have one, tries and not
styles decided on:

| fighter | before the tell | his tell | then |
|---|---|---|---|
| Plain | 2 short fakes, 1.5 s apart | a long fake | his blow 1.1 s after it begins |
| Prowler | 3 of blade flick / short fake, 1.2 s apart | a drop | 0.45 s after |
| Strutter | blade circle, short fake, 1.6 s apart | a long fake | 1.2 s after |
| Bull | 2 shield presses, 1.9 s apart | a blade dip | 0.3 s after |

Plain (a poorer swordsman's): always the same. Mixed up: one to three gestures before the tell, drawn afresh,
and one tell in three has no blow after it.

*What he learns, watching.* Nothing of the other's pattern is given him. From the other's last four blows:
the gesture that came the same time before at least two of them is the tell, and how long before; if the
last two of those had the same number of gestures before them, evenly spaced, he can count. And for each kind
of gesture, when it left the chest open (its longest opening, measured on the bodies as the tests do).

*What he does with it.* Seeing the tell, he stops gesturing until the blow has come, so that his shield is
free for it (a fake of his own commits his shield). Knowing the count, he knows the tell is next before it
begins, and strikes into it if it leaves an opening long enough: that is how a long fake is caught, which
cannot be told from a short one as it starts. A fake slash is made out for a long one only once it has gone
past where a short one turns back, and by then a blow no longer lands in it. A gesture he has not met he
cannot read: he goes for what he sees open, as one who only reacts does every time. One he knows leaves
nothing worth a blow he lets go.

*How the fight is going* is first kept here: blows landed less blows taken over the last 20 seconds, for
each. Nothing hangs on it yet.

Switches: Combat Feel "Fighters Read Each Other's Patterns" (on; off: nobody reads) and "A Stopped Blow Is
Answered" (off, see below). In the duel lab: "The dance" in the actions window (Dance, Forget), and in the
panel each fighter's pattern (the four, plain or mixed up), reads or only reacts, what he has learned, his
last blow, his tally, and how the fight is going. Pressing Dance the first time gives each the pattern that
goes with his own levers ("Each his own"). What happens is written over the two as it happens (HIS TELL,
SAW HIS TELL: covers for his blow, READ THE COUNT, LANDED...).

Headless, standing 1.5 m apart, 60 s each (`FIGHT_DANCE="Strutter,Prowler"`, `FIGHT_DANCEREADS="1,1"`,
`FIGHT_DANCEMIXED`, `FIGHT_DANCEREADING=0`, `FIGHT_DANCEANSWERS=1`):

| the two | blows | landed | what showed |
|---|---|---|---|
| Strutter and Prowler, both read | 8 and 7 | 1 and 1 | each learned the other's tell (long fake 1.2 s before, drop 0.5 s before); 6 of the 15 blows were expected and covered for |
| Plain reads, Plain only reacts | 10 and 10 | 1 and 0 | the reader expected 4 of the other's blows; the other none |
| the same, sides swapped | 14 and 7 | 2 and 1 | the reader learned a wrong tell from the other's reacting blows, and covered for nothing 5 times |
| Strutter mixed up, Prowler plain, both read | 4 and 10 | 1 and 4 | Prowler threw 8 blows by what he read; all 4 that landed caught the Strutter in a fake |

What this says, plainly. The reading shows in what they do: tells learned, blows expected and covered for,
blows thrown by the count into a long fake. It does not yet show in who wins: about one blow in six lands (11 of the 70 in the table),
the rest meet the shield whatever was read, and 60 seconds give too few landed blows to tell a reader from a
reactor. Four things found on the way:
- A blow that comes while the other is in a fake of his own lands on an arm or the hand more often than the
  chest: his shield is committed but still in the way of the chest.
- A fake wins a fighter nothing as the rules stand: it commits his shield and draws blows, and a blow wasted
  on it costs the other nothing. So the fighter who fakes most is hit most.
- To give a wasted blow a cost, "a stopped blow is answered" was tried: the one who stopped it strikes back
  at once. Every answer met the shield too (the one answered covers while still in his own blow), so it is
  left in, switched off.
- A reader facing one who only reacts learns wrong tells: the other's blows come from what he sees, not from
  his pattern.

Not verified: nothing seen in Unity. The numbers above are single runs. Carried on: the stance switch, arm
postures for sword-only fighters, the tell built into the attack. Next: the dance's third part, pacing by
the gap (they stand on their spots here).

New file that needs its .meta from Unity: `Sandbox/Dance.cs`.

**Fight rhythm, step 8, the dance, third part: paced by the gap** (17 November, built overnight without a
playtest). In the second part the two stood on their spots. Now they move about each other, each by how
restless and how aggressive he is (`Dance.StepFeet`; two new levers of a fighter's own, `FighterLevers.Restless`
and `Aggressive`, 0 to 1).

- *The distance he likes*: between just outside reach (2.2 m, root to root) and inside it (1.5 m), nearer the
  more aggressive he is. Each steers for his own, so an aggressive one closes on a cautious one, who gives
  ground: but only so much (1.5 m, won back slowly), and no further than 3.5 m from where the dance began;
  after that he stands, or goes round toward the middle.
- *Round and stand*: he goes round the other in spells (0.7 m/s, walked, on the slow side walk) and stands
  between them. A restless one's spells are long and his stands short; a calm one's the other way.
- *In for the blow, out after it*: when his pattern's blow is due, or a blow he has read, he comes in first
  (he throws it from 1.95 m or nearer; a blow on the move steps in the rest), and goes out again after it,
  sooner back in the more aggressive he is. A chance he sees from out of reach he lets go.
- *A blow he expects*: at the other's tell, a fighter under 0.6 aggressive steps out of reach of the blow; a
  more aggressive one stands and covers, as in the second part.
- *How the fight is going shifts it*: two blows ahead he is "pressing" (more aggressive by 0.25), two behind
  "shelled up" (less by 0.3); else "probing", or "coming in" from far off. This is the first thing that hangs
  on that number. What he is about is written over him when nothing else is.

The made-up fighters: Prowler restless 0.85, aggressive 0.3; Strutter 0.6, 0.55; Bull 0.2, 0.9; Plain 0.5, 0.5.

Switch: Combat Feel "The Dance Is Paced By The Gap" (on; off: they stand on their spots). In the duel lab,
under "The dance": "Their feet: paced by the gap / on their spots (old)", and a line with the gap now, what
each is about, and the gap and their walking so far. "Each his own" has the two new levers, Restless and
Aggressive.

Headless, 58 s each, both reading (`FIGHT_DANCE`, with `FIGHT_OWN` for their levers; `FIGHT_DANCEMOVES=0`:
on their spots; `FIGHT_DANCEGAP=1`: the gap twice a second):

| the two | the gap, on average (least to most) | each walked | blows landed | what each was about |
|---|---|---|---|---|
| Bull and Bull | 1.60 m (1.16 to 2.00) | 18 m | 0 and 0 of 10 and 9 | probing throughout |
| Bull and Prowler | 1.70 m (1.05 to 2.43) | 24 and 23 m | 0 and 1 of 9 and 8 | probing throughout |
| Strutter and Prowler | 1.93 m (1.19 to 2.79) | 30 and 31 m | 0 and 6 of 7 and 8 | Strutter shelled up 52% of the time, Prowler pressing 52% |
| Prowler and Prowler | 2.03 m (1.31 to 2.56) | 30 and 28 m | 0 and 0 of 9 and 7 | probing throughout |

So the levers do what they are for: two Bulls fight 40 cm nearer than two Prowlers and walk two thirds as
far. Feet slide 0.1 to 2.2 cm a second. In the one duel with blows landing, all six caught the Strutter in a
fake of his own, as in the second part: the fighter who fakes most is hit most.

Not verified: nothing seen in Unity, and how it looks is the whole point of this part: whether the circling
reads as two fighters measuring each other or as wandering, whether coming in and going out is too busy, how
the walks join. Single runs. The room they keep to is set where the dance began, not to the arena. The dance
is in the duel lab only: the fight's own steering does none of this yet. With this the dance's three parts
are in; the stance switch, arm postures for sword-only fighters and the tell built into the attack are still
carried on.

**From the playtests of 17 November (morning): the shield keeps its cover; the bash; light and heavy blows;
dodges, leaps and blows at chosen parts in the dance.** Renier: the shields were swung about constantly and
did not cover; shield fakes make no sense; the sword arm gestures, the shield stays; bash what comes too near;
more dips, dodges, leaps; attacks at legs and feet; a slow powerful class of blow and a fast weak one.

*The shield is held in cover* (`ClipFighter.ShieldCover`, in `RaiseShield`; Combat Feel "The Shield Is Held In
Cover", with how much of the arm is the cover's, standing and in a blow, and where the board is held). Whatever
the body does (standing, walking, a gesture of the sword arm, a blow of his own) the board is held before his
chest, looking at the enemy, in the body's own frame from the chest: it goes with the body and keeps a little
of the recording's arm, but stays in place. It leaves only to meet a blow. Headless, the same dance with it
off and on:

| | the board moves about (rms) | his chest open to the other |
|---|---|---|
| standing and walking, off | 16 to 20 cm | 29% and 92% of the time |
| standing and walking, on | 6 to 7 cm | 6 to 7% |
| in a sword fake, off | 20 to 26 cm | 47 to 55% |
| in a sword fake, on | 3 to 5 cm | 0 to 1% |
| in a blow of his own, off | 43 cm | 63 to 73% |
| in a blow of his own, on | 13 to 14 cm | 36 to 39% |

*A fake draws no guard* (`ClipFighter.FakesDrawNoGuard`; Combat Feel "A Fake Draws No Guard"). The playtest
of 07:52, with the cover on, still showed shields swung about. Its log: in its cover (67% of the time) a board
moved 6 to 7 cm and looked 26° off straight ahead; out of it (33%) it moved 30 to 33 cm and turned up to 140°
off; and of the 33 times a shield left its cover, 22 were just after the other began a fake. The guard read a
fake as the blow it begins as. Now a shield (or a parry) goes to meet only a real blow. Headless, the same
dance: a shield out of its cover 4 of 37 seconds, against 9 before. Not touched: how far and how far round a
shield goes when it does meet a real blow.

*No shield fakes.* The shield press is out of every pattern and off the lab's buttons; a sword fake no longer
takes the shield with it (Combat Feel "A Sword Fake Takes The Shield", off; it was "A Fake Commits The
Shield", on). The patterns are the sword arm's and the body's: more blade dips in every one.

*The shield bash* (`ClipFighter.Bash`, `BashIfNear`; Combat Feel "A Shield Bashes What Comes Too Near", with
how near and how hard). An enemy on his feet within 1.2 m and before him is driven out of his space: he sinks
behind the board, drives it and his body forward, and the enemy is shoved back stumbling at 2.6 m/s, his
guard open for 0.4 s and a blow of his own dropped. Not while his shield is meeting a blow, in a blow of his
own, or down; no sooner than 2.5 s after his last. Its sizes are from the recording "Sword And Shield Block"
(the body 26 cm forward and 12 cm down behind the board in a quarter of a second). In the fight, by itself;
in the lab, the "Shield bash" button. Headless on demand: his hips sank 8 cm, his body went in, the board
15 cm further ahead of his chest, and the other went from 1.17 m to 1.87 m off. A first version: Renier has
since asked for it to be a lab piece with sliders for how hard it is and how far he leaps in, the legs seen
doing the work, the board straight ahead (see the roadmap).

*Light and heavy blows* (`AttackClasses`, in `AttackMaker.cs`; `StrikeHits.Power`; Combat Feel "Blows Are
Light Or Heavy", with each one's power). The same recorded cut made two ways by its style: light, little
wind-up, back at once, landing at 0.6 of a plain blow's weight; heavy, wound far back and slowly, carried
through, at 1.6. Headless: a light blow lands 0.39 to 0.55 s after it starts, a heavy one 0.88 to 1.13 s; the
landed ones did 6.1 harm (light) and 27.3 (heavy). In the dance a fighter's own blow after his tell is heavy
or light by his own lever, and every blow thrown to take a chance is light; a fighter who was waiting for a
blow (he saw the tell) and is bold and in reach strikes into its wind-up: in one duel four times, two of them
landing on the head and chest. In the lab: "The blow is: plain / light / heavy" above the body map. The
fight's own blows are still plain.

*Dodges* (`ClipFighter.Dodge`; sizes from the recordings "Dodging" and "Dodging Right"): he ducks and leans
aside, his hips going that way, and comes back up; his feet stay, his shield goes with his chest. A gesture in
the Prowler's pattern, two lab buttons, and what a fighter who dodges does about a high blow he sees coming.
*Leaps* in the dance: in, to close the last of the gap for his blow; out, from a blow he expects. *Blows at
chosen parts*: each pattern has its own (the Prowler low, at legs and feet; the Strutter high; the Bull at
the body and thighs), going for the next that the other's shield does not cover; in the runs blows landed on
thighs, shins and a foot. New levers of a fighter's own: Leaps, Heavy blows, Dodges.

Not verified: only the shield's cover and the fakes fix have been seen in Unity (Renier: "much better");
the bash, the classes, the dodges and the leaps have not. The dodge and the bash are sized from their
recordings by hand, not baked from them. The ordinary fight of six is changed by the cover and the bash (it
has shields): see below.

The ordinary fight of six with the cover and the bash, against without them, over four seeds (a minute
each): 267 blows against 288; of the blows that met something, 31% stopped by a shield against 25%, 48%
landed against 49%, 22% parried against 26%. So the fight goes on much as it did, with a few more blows
stopped by shields and 7% fewer blows thrown. Each single fight comes out differently (the first seed's
winner changed), as any change makes it.

**The shield bash as a lab piece: three bashes, each with its own settings; every action's settings under
its buttons (the plan's step 9a, breaking the guard).** Renier: sliders for how hard a bash is and how far
he goes into the enemy; the legs seen doing the work of lunging him into it; the board straight ahead, not
off to the side like a slash; able to knock the enemy into a stumble or onto the ground; small, medium and
hard bashes, each with its own animation and sliders, the hardest going fast all the way into the enemy's
space and bashing him hard away; and all the sliders of the actions in the Actions window, under each
action, folded.

*Three bashes* (`BashLevers`, in `AttackMaker.cs`; `ClipFighter.BashKinds`, `Bash(at, whenever, kind)`;
Combat Feel "Small / Medium / Hard Bash ..." twelve settings each). Small: from where he stands; he sinks
behind his board, the board is driven straight ahead and his body goes in with it. Medium: he leaps in
behind his board (the lab's leap, made of the recorded jump, low and leaning in), stops short of the enemy,
and drives the board as he arrives. Hard: he crouches deep, leaps fast all the way to the enemy's own spot,
leaning well in, the board driven as it reaches him, and goes on through as the enemy is thrown out of it.
Each has its own: how far he leaps at most, how far short of the enemy the leap ends, the leap's speed, how
hard the arm drives, the weight it lands with, whether he goes on through, and the leap's crouch, lean,
height and wind-up (and, thrown standing, how far he sinks and how fast his body goes in).

*What a bash does is not set.* It is the speed the board arrives at the enemy with (his body's and his
arm's together, over the last eighth of a second, and no less than the body behind it), times the bash's
weight: under 1.5 m/s the enemy is shoved back a step; from there thrown back stumbling, his guard open
and a blow of his own dropped; from 4.5 m/s knocked down. One knocked down is thrown at the shove times
"Thrown" (Combat Feel "Bash Throws", 1.5): the falls' own rule then decides, under 9 m/s he goes over about
a metre back, from 9 he is thrown off his feet. Headless, each once from a stand, a Medium on a Medium:

| bash, from | his hips sink | he goes in | the board met him closing at | the enemy |
|---|---|---|---|---|
| small, 1.0 m | 8 cm | 0.4 m | 2.0 m/s | shoved back at 1.2 m/s, 0.55 m |
| small, 1.3 m | 8 cm | 0.4 m | 0.9 m/s (late, on his body's speed alone) | shoved back at 0.6 m/s, 0.3 m |
| medium, 1.6 m | 18 cm | leaps 0.9 m at 4 m/s, comes down after 0.6 m where he met him | 5.2 m/s | thrown back stumbling at 3.4 m/s, 1.8 m |
| medium, 2.0 m | 18 cm | leaps 1.2 m, comes down after 0.95 m | 5.4 m/s | thrown back stumbling at 3.5 m/s, 1.6 m in 1.2 s |
| hard, 1.0 m | 27 cm | leaps 0.9 m to the enemy's spot | 4.1 m/s (no room for a run) | thrown back stumbling at 3.7 m/s, 1.9 m |
| hard, 1.6 m | 34 cm | leaps 1.5 m at 6 m/s | 7.0 m/s | knocked down at 6.3 m/s |
| hard, 2.2 m | 34 cm | leaps 2.0 m at 6 m/s, his hips at most 5.2 m/s, to 0.2 m short of where the enemy stood | 7.8 m/s | knocked down at 7.0 m/s; thrown 4 m back ("Thrown" 1.5), 1.1 m at 1 |
| hard, 2.8 m | 34 cm | leaps 2.0 m (its most) | 8.3 m/s | knocked down at 7.5 m/s |

In every one the shield hand went along the line to the enemy (in the hard one 2.2 m along it, through
where he stood) and no more than 13 cm across it. By himself, in a fight, a fighter throws the small one
at an enemy who comes too near (as before); the medium and the hard are thrown in the lab only.

*The Actions window* (`BattleSandbox.ActionGroups`, `ActionSettings`). Each action now has its settings and
what was measured of it under its own buttons, folded shut behind a bar ("Moving: settings", "Leaps:
settings", "Postures: settings", "Small / Medium / Hard bash: settings", "What a bash does", "Strikes:
settings", "Shield test: settings", "Fakes test: settings", "The dance: settings"), opened by a press and
kept between sessions. The pace stays at the top; the rest scrolls when it is taller than the screen. The
panel keeps what is not an action: the shield's cover, the style, how he moves, each his own, what he
leaves open. "Actions: in the panel" puts everything back there, as it was. In the lab: "Small bash",
"Medium bash", "Hard bash", and "He bashes: the other / the air".

Not verified: nothing of this has been seen in Unity. The window's folding and scrolling cannot be run
headless: it compiles, and is built as the panel's own sections are. Each bash was run once per distance,
from a stand, against a standing Medium. Found and left: about a second and a half after a hard bash he
steps back half a metre (his guard's own spacing; the pounce, step 10, is where he should follow up
instead). How far a felled man is thrown is a step, about a metre or about four, because the falls'
"thrown off his feet" begins at 9 m/s; evening that out belongs to step 9c (stumbles and falls). The fight
of six and a dance were run once each with this in and ended as they should; no bash came up in that dance.

**From the playtest of 17 November, 11:28: the shield arm reaches; and the walking in the dance looked into
(research and a plan, nothing of the plan built).** Renier: the bashes are "pretty nice", he "really does
get in there", but the shield arm itself should extend more, and the arm's slider had little effect. And:
the walking in the dance stands out as bad; he "lerps into weird positions"; the mix of the forward, back
and side recordings does not feel balanced; look at the logs, research it from scratch, plan, and show it.

*The shield arm reaches* (`BashLevers.Reach`; Combat Feel "Small / Medium / Hard Bash Reach", 0.6, 0.8, 1;
the lab's "Arm reach" under each bash). What limited it: the cover already holds the board 30 cm ahead of
the chest, and a straight arm is only a hand's breadth further, so the old slider could add 12 to 14 cm.
Now the board is drawn in toward the chest first, the arm goes to its whole length, and the shield shoulder
is thrown in behind it (the chest turned about the upright by up to 40 degrees, the head left looking where
it did). Headless, the shield hand's travel ahead of the chest: 12 cm at reach 0, 30 cm at 0.6, 43 to 45 cm
at 1. "Arm drive" is now only how quickly it goes out. *What a bash does* now counts the arm's own speed
for 0.4 of the body's (Combat Feel "Bash Arm Counts"; the lab's "Arm counts"): in the playtest a small bash
with the arm driven at 0.09 s knocked a man down at 5.6 m/s from a stand, on the arm's speed alone, and one
hard bash read 14.4 m/s off a single step's catching up; the body's speed is now capped at what the leap
was asked to go. With that the weights are 0.9, 0.85 and 1.1 and "Thrown" 1.6. Headless, once each: small
from 1.0 m shoves at 1.1 m/s; medium from 1.6 m throws back stumbling at 3.5 m/s; hard from 1.6 and 2.2 m
knocks down at 6.5 and 6.7 m/s and the other lands 4 m back. Not seen in Unity.

*The walking in the dance.* The finding and the plan are in the document "Convincing walking in the dance"
(https://claude.ai/artifact/94Vvn9UMLa78XcCougaDLA). In short: the walk mixes recordings of three different
bodies (the guard's stand and side-steps: hips 85 cm, wide, the left foot ahead, the chest turned; its walk
forward and back: 9 cm taller, narrow, square; the crouched sneak chosen on 16 November for slow circling:
13 to 16 cm lower), and the dance changes the mix about once a second and never finishes a step. Measured in
the playtest: the hips of a standing or walking fighter over a band of 21 cm, and rising or sinking 10 cm or
more within 0.3 s once every 3 to 4 s. The plan: a footwork course and its meter; one carriage for every
step; whole steps, changed only at a landing; calmer feet in the dance; legs turned for diagonals in place
of a mix; the tall walk for far only. Two tools came of it: `walksurvey` (the test runner: every recording
the walk uses, its pace, hips' height, stance, and how far apart each two are as poses), and the film's own
walk state (`FIGHT_FILM` now writes, each step, which recordings are mixed and how much, the phase, pace,
way of travel, the levers' crouch and lean, and how far the last switch still carries the pose off), read
by a script kept outside the project. `Locomotor.LeversShown` and `SwitchOffsetNow` are for those reports.

**Everyone's movement back at rest (17 November).** The "Prowl" try under "How he moves" had been saved
into Renier's scene as everyone's movement (crouch 8 cm, lean 10 degrees, sway and hip turn 1.6), by "Keep
what I tune"; he did not mean it. Combat Feel's eight movement levers are renamed (`movementBounce`,
`movementSway`, `movementHipTurn`, `movementChestTurn`, `movementArmSwing`, `movementStepLift`,
`movementLean`, `movementCrouch`; they were `move...`), so the saved values are dropped and each takes its
value at rest. The Prowl and Strut try buttons are gone from "How he moves": those tries are a fighter's own
("Each his own": Prowler, Strutter, Bull), which are not saved into the scene. Not seen in Unity.

**The packs of 17 November looked over for their legs (a survey; nothing in the walk is changed).** Renier
sent sixteen packs without sword and shield, for the lower body: "find all the animations that are relevant
for our standard set"; none to be used as it is.

*The tools.* `Tools/Mocap/Combat/pack_survey.py` samples every FBX take of every pack (into a cache outside
the project) and measures what its legs do, at a Medium's size: pace, the way it goes against where the
hips face, the hips' height and turn, where the feet are kept (in the hips' own frame), footfalls, whether
it loops, how busy the hands are. `Tools/Mocap/Combat/packs_survey.csv` is that table for all 812 takes of
the 33 packs sent so far. `loco_bake.py` can take a take's own front as its facing (`yaw_from='front'`: sets
shot around where he aims; a character with no eye bones gave its chest's turn for its look).
`walksurvey` (the test runner) now sets candidates against the guard's stand, their legs under the guard's
own chest and arms, and sets of four side by side.

*What was found.* The guard stands turned: his hips face 54 degrees to the right of where he looks (68 to
72 in his side-steps), so for his legs a step toward the enemy is a step sideways and forward. The sword and
shield pack's own walk forward and back has its hips square (1 degree): that is why it never matched. Sets
shot around an aim keep the turn as they walk. An archer's walk forward and back (Longbow Locomotion Pack:
hips turned 70 degrees, 1.1 and 0.9 m/s, hips 5 cm above the stand's) are the advance and the retreat. Also
found: the archer's four quick dodge steps, a rifleman's walks eight ways (hips level with the stand) and
crouched walks eight ways, turns of 90 degrees on the spot (four packs), starts and stops (Shooter Pack), a
goalkeeper's small adjusting steps (two takes of 7 s), runs and sprints. 62 takes are kept as samples
(`bow_`, `axe_`, `rifle_`, `magic_`, `shooter_`, `keeper_`, `soccer_`); the four walks of the archer, the
axeman and the caster and the rifleman's eight are baked into `CombatClips.json` as legs to try (not used
by the walk).

*Measured* (`walksurvey`, each set's legs under the guard's own chest and arms): today's four recordings
(the pack's own walk forward and back, the guard's side-steps) are 28.6 cm apart as poses between
neighbouring ways and their carriages 23.7 cm; with the archer's forward and back, 17.0 and 7.8 cm; the
archer's own four, 20.0 and 9.3; the rifleman's four, 22.2 and 9.3. One headless dance with the archer's
legs for forward and back and the crouched sneak out (`MoveStyle.StanceWalks`, off; `FIGHT_STANCEWALK=1`,
`FIGHT_SLOWSIDE=0`): the hips' height by which recording leads 85, 85 and 83 cm, 83 to 84 standing (it was
74 to 75 on the sneak, 86 to 87 on the walk, 81 to 83 standing); the hips' band 10 to 11 cm (it was 20 to
22); a rise or sink of 10 cm within 0.3 s once every 10 to 14 s (it was every 5 to 8 s). The mixing itself
is as it was. Not seen in Unity, and the takes themselves have not been seen: measured only.

**Convincing walking, steps 1 and 2: the footwork course and its meter; the guard walks in one carriage.**
Renier, on the plan and the packs' survey: "Yes this sounds good. Proceed."

*The footwork course and its meter* (`DuelLab.BeginCourse`, `FootworkMeter`, in `DuelLab.cs`; the lab's
"Footwork course" under "Move him"). The same 24 seconds of footwork every time, at the dance's pace
(0.8 m/s) between 1.5 and 2.5 m: he retreats, advances, circles left, circles right, goes in and out twice,
and stops and goes four times. The meter measures him from its first step, only while he stands or walks: the
band his hips' height moved over (the middle 90% of the time); how often they rose or sank 10 cm or more
within 0.3 s; his steps in a spell of walking; how much of his walking was two recordings mixed, each a
quarter or more; his steps' tempo against the recordings'. Each fighter of the dance has a meter too (under
"The dance: settings"). The crouched sneak is out of the ordinary walk: Combat Feel "Slow Circle On" is 0, the
guard's own side-step slowed (it was "Slow Side Walk", 1; renamed so the saved 1 is dropped).

*The guard walks in one carriage* (`MoveStyle.OneCarriage`, `MoveScore.CarryAs`; Combat Feel "The Guard
Walks In One Carriage", on, and "Guard Keeps Its Stance", 0.5; the lab's "His walk: one carriage / as
recorded (old)" and "Keeps stance"). He steps forward and back on an archer's legs (Longbow Locomotion
Pack: hips turned 70 degrees from where he aims, as the guard's own side-steps' are), under his own
side-step's chest and arms. And each of his four steps is carried as his stand is: his hips at the stand's
height on average, and each foot's own place moved half-way to where the stand keeps that foot. Only the
step is the recording's own: how the hips rise, sway and turn about their middle, how each foot goes about
its place. His jogs and runs are as recorded. A walker makes its guard's clips again when the switch is
thrown (`MoveStyle.Stamp`), so old against new can be flipped in play.

Headless, the course, the same fighter:

| | his hips' band | his hips on each step: forward, back, left, right | his left foot ahead of his right, on average |
|---|---|---|---|
| as the playtest had it (the crouched sneak, each step as recorded) | 24 cm (69 to 94) | 91, 91, 73, 71 cm | 23 cm |
| the sneak out only | 14 cm (80 to 94) | 91, 91, 81, 84 cm | 30 cm |
| one carriage | 6 cm (80 to 86) | 84, 84, 82, 84 cm | 50 cm |

His stand's left foot is 73 cm ahead. His legs are never asked longer than they are (the ankle at most
0.985 of the leg's length from the hip, as before), and a foot on the ground slides 0.03 m/s on average
(0.04 before). Headless, a dance of 53 s, two fighters, old against new: the hips' band 21 and 22 cm
against 11 and 8; a rise or sink of 10 cm within 0.3 s 21 times in all against 11; the hips on whichever
recording leads 81 to 83 cm, 83 standing.

Not done by these two steps, and measured as it was: two recordings are mixed for 43 to 47% of the dance's
walking; a spell of walking holds 0.7 to 0.9 steps; steps go at 0.66 to 0.70 of the recordings' tempo.
Those are the plan's steps 3 to 5. Found and left for step 3: the first step out of a stand leaves the
planted foot behind until the leg cannot reach it, and his hips drop 10 to 15 cm for it (most to his left);
it was there before, and the wider stance kept makes it a little more frequent ("Keeps stance" at 1: four
times in the course; at 0.5: twice; at 0: once). Not verified: nothing of this has been seen in Unity; the
archer's take itself has not been seen, only measured.

**Quick turns round (17 November).** Renier, after the walk in one carriage: "I bashed myself in next to the
enemy, which is fine, but then he was something from 90-180 degrees looking away, and in a real fight you
would not turn around slowly. You will need a fast turn arounds in these cases, also for when they run. Here
are some turning animations. There's a quick turn in there that will work for this."

*What it was.* A fighter standing in his guard with his enemy at his side or behind him turned with the
guard's own clips: a quarter turn (88 degrees in 0.93 s) stretched to as much as 1.6 times, or the turn round
(0.8 s), which is recorded from a different, upright stance and goes one way only. Measured headless, the
other put round him at once: 150 degrees round, 0.77 s to face him (to within 15 degrees), his hips dipping
28 cm on the way (56 to 84 cm) and then a second turn of 40 degrees back; behind him, round the way the turn
round does not go (155 degrees round as it began, the other moving too): the quarter turn stretched to 139
degrees and the 41 then left eased round on his feet: 1.4 s.
Stepping, his facing was eased round at 200 degrees a second at most: 0.83 s for 150 degrees.

*The quick turn* (`Locomotor.QuickTurns`, `StartTurn`; Combat Feel "A Fighter Turns Round Quickly", on).
With an enemy within 4 m ("Quick Turn Within") and what he is to face 75 degrees or more off ("Quick Turn
From"), standing or stepping, he makes the quick turn round from the pack "Turns" (its "Quick 180 Turn"),
the shorter way (the take is mirrored for the left), as far round as is asked, played at 1.5 times ("Quick
Turn Pace"; a turn to his side up to a quarter quicker), and aimed as it goes at where the enemy is by then
(what is left of the clip's turn is stretched or shrunk onto what is left to turn). Out of a walk with a foot
in the air, the turn is taken up where it has that foot in the air.

*Never the recording as it is* (`MoveStyle.OnTheStand`). The take is made with empty hands from an upright
stance 13 cm taller than the guard's stand, feet near each other, hips square. Only its steps are used: frame
by frame, what its hips and feet do as that differs from its own first and last frames is laid on the
guard's stand. The hips' place, turn and lean; each foot's place, kept where it is in the world while the
take has it on the ground (the difference between the two stances is given while the foot is in the air, so
a planted foot is not dragged), and which way it points; the knees turned with the feet. The chest's twist
on the hips, the head, the arms, the shield and the sword are the stand's own throughout, and the first and
last frames are the stand itself. Two levers: how far the feet are drawn in from the stand's wide places at
the middle of the turn ("Quick Turn Draws Feet In", 0.5), and how much of the take's sinking is taken
("Quick Turn Sinks", 0.4: the take sinks 11 cm at its middle, to about the height the guard stands at
already).

*The turn a third of the way round.* The pack's "Left Turn" and "Right Turn" (104 degrees of the root's
turn in 0.93 s), made the same way, are the unhurried turn on the spot that is too big for the quarter turn
and not round the way the turn round goes: that case was the quarter turn stretched 1.6 times and stopped
40 degrees short.

*In the lab* (Actions, "Move him"): "Put the other: left / back left / behind / back right / right" puts the
other, at once, that far round him, as far off as he was; he turns to face him by himself, standing or while
he walks or circles. "His turn: quick / as it was (old)". "Last turn" says how long he took to face him
again. The levers are under "Moving: settings".

Headless (`FIGHT_BEHIND="4,150"`: the duel's other put 150 degrees round the mover at 4 s; `FIGHT_TURNTRACE=1`;
`FIGHT_QUICKTURN=0` for as it was):

| the other put | he was | as it was: faces him in | quick: faces him in | quick: free again in | his hips, quick |
|---|---|---|---|---|---|
| 90 degrees round | standing | 0.70 s | 0.40 s | 0.53 s | 79 to 84 cm |
| 150 | standing | 0.77 s (hips 56 to 84 cm) | 0.48 s | 0.60 s | 79 to 84 cm |
| 180 | standing | 0.73 s (hips 64 to 86 cm) | 0.53 s | 0.63 s | 79 to 84 cm |
| 150 | circling, 0.7 m/s | 0.83 s | 0.48 s | 0.60 s | 78 to 81 cm |
| 120 to his left | circling | not run | 0.28 s | 0.43 s | 79 to 81 cm |
| 180 | backing away | not run | 0.38 s | 0.48 s | 80 to 82 cm |

(The "as it was" figures for 90 and 180 standing are from another set-up, in which he was first turned away
from the other with his unhurried turn and then turned back by himself; the rest are the other put round
him.) In the fight of
six, a minute: 61 turns on the spot as it was; with quick turns 52, of which 25 quick (22 to the right, 3 to
the left; one out of a walk). No error, and the fight ran to its end both ways.

*Not done, and known.* Running: the pack's "Running To Turn" and "Walking Turn 180" are baked, each way
(`run_turn180`, `walk_turn180` and their `_m`), and not used yet: a runner in his guard still comes round
at 140 degrees a second. That goes with the walking plan's step 6 (far and near), where running is. Out of a
walk, in one of four cases a planted foot was moved 11 cm in one frame as the turn took over (the lock on it
was let go and taken again). The measure of sliding cannot tell a foot pivoting on its ball from one
sliding, so sliding under the turn is not measured. A Large (two-handed guard) has no quick turn: only the
standard sword-and-shield style. The get-ups are still the made ones of 14 November: the recorded "Getting
Up" takes are to be rebuilt into them in the roadmap's step 11 (Downed play); Renier asked for this again on
17 November. Not verified: nothing of this has been seen in Unity; the take has not been seen, only measured.

**Convincing walking, step 3, its first item: a leg left behind steps before the hips sink (17 November).**
Found in steps 1 and 2: most of what was left of the hips' jumps was the first step out of a stand. His
stand is wide, the left foot 73 cm ahead; as his body goes away from a planted foot, that foot stayed where
it was until its leg could not reach it even with the hips dropped all they are ever dropped (12% of the
leg's reach, 10 cm), and only then was it let go to step after him (`LocoBody.UpdateFeet`, "stretched").
So his hips sank 10 cm and more, at once, before the foot came. Now the foot is let go as soon as its leg is
all but straight: with the hips sunk 3% of the reach, about 2.5 cm (`LocoBody.SinksBeforeStep`; Combat Feel
"A Leg Left Behind Steps Before The Hips Sink", on, and "Hips Sink Before A Step", 0.03; the lab's "A leg
left behind: steps first / hips sink first (old)" under "Move him"; `FIGHT_SINKSBEFORESTEP=0.12` for as it
was).

Headless, old against new: the footwork course, his hips rose or sank 10 cm or more within 0.3 s twice
against never, the band (6 cm), the steps a spell and the tempo the same. A dance of 63 s, two fighters:
8 and 6 such jumps against 1 and 1; the hips' band 10 and 9 cm against 7 and 7; two recordings mixed for 43
to 50% of the walking either way, 0.6 to 1.1 steps a spell either way. Not measured: whether he now takes
more small catch steps than before (the foot let go sooner steps sooner); to be looked at in Unity. The rest
of step 3 (whole steps: a spell of walking is still under one step on average) is not begun.

**Every loadout turns round quickly; and a cut made while turning round (17 November, evening).** Renier, on
the quick turns: "Ok yes this works great. You will need to do the same then for the sword guy without the
shield. This is a standard set feature that should apply to all the loadouts, as everything should." And: "I
want him to also be able to do slashes while turning around ... if he thinks his enemy is attacking him as he
turns around. Here are some reference animations for you to source. Keep in mind that these are more like
spinning and jumping around ... first I want you to only source the upper body as a reference for the
attacks. Not using the animations as is. Still using the same system you built so that we can have sliders
to customize it. As in how far the blade goes. If he's more leading with the blade or if the blade is
lagging a bit behind his body turning."

*Every loadout.* The quick turns were made for the style named Standard only, which is what my note about
"the Large" came from. Every body in the game stands in that style today (a Medium with sword and shield, a
Swordsman with a sword alone, a Large: `LocoLibrary.CombatPrefix`), so all three already turned quickly; what
was missing was the rule. Now the turns are made for every style, on that style's own stand
(`MoveStyle.Clips`). Checked headless: a Swordsman, the other put 150 degrees round him, faces him in 0.48 s
with the quick turn; a sword-alone fighter as the one turned on, the same. In the fight of six, quick turns
and cuts made turning were made by Mediums, Swordsmen and Larges alike. (The lab can also put him round the
other, `DuelLab.PutMover`, `FIGHT_BEHINDOTHER`.)

*A cut made while turning round* (`TurnSlashes`, `ClipFighter.TurnSlashMade`, `UnitController.TurnSlashNow`;
Combat Feel "A Fighter Cuts As He Turns Round", on). About to make his quick turn round to an enemy within
2.2 m ("Turn Cut Within"), he makes it with a cut in it when that enemy is striking at him, and one time in
four when he is not ("Turn Cut Anyway").

- *The legs* are the quick turn's (the recorded turn's steps on his own stand), as far round as his enemy
  is. A turn's clip is given the marks a blow's legs need (`AttackScore.OfTurn`): the turn goes evenly
  through the cut's wind-up and drive, and the blade is to land when the turn is so far through.
- *What is above the hips* is a recorded cut's chest, head and arms, put on those legs with the graft the
  attacks already use for borrowed legs (`AttackScore.OnLegsOf`). Turning to his right: a backhand that goes
  round with him, from the pack "Spin Around Slash" (three takes: a high flat backhand, a backhand down
  across the body, a leaping chop). They spin 360 to 420 degrees and travel up to 2.9 m; none of that is
  used: only the chest's twist on the hips, the head and the arms. They are baked as `spin_backhand_high`,
  `spin_backhand_mid`, `spin_chop` and are not among the strikes a body knows. Turning to his left there was
  no reference (all three spin to the right): his own forehand, `sns_forehand`, is taken.
- *Made as every attack is* (`AttackMaker`): aimed at a part of his enemy (of chest, neck, head and belly,
  the one the cut reaches with least done to it), its blade able to land, be blocked and be parried. The aim
  may turn his whole body 20 degrees at most on top of the legs' turn; what it turns is given back after
  the blade has landed, so he ends facing his enemy.
- *The sliders* (Combat Feel; in the lab under "Move him"): "Blade goes" (0.5 to 1.6: how far the sword arm
  is drawn back before the cut and carried on after it) and "Blade leads" (-1 to 1: where in the turn the
  blade is to land: 0 as his body comes round; toward 1 while his body is only a little over half way
  round; toward -1 up to 0.2 s after his body is round).
- *In the lab:* "As he turns: just turns / cuts" makes every turn round a cut; press one of "Put the other".
  "His cut (right): high backhand / middle backhand / chop".

Headless, the other put round him at 1.5 m, every turn made with a cut (`FIGHT_TURNSLASH=always`,
`FIGHT_BLADEGOES`, `FIGHT_BLADELEADS`, `FIGHT_TURNCUT`, `FIGHT_TURNCUTLEFT`; `slash.py` reads the film):

| the other put | cut | the blade | his body then | he faces him in | hips |
|---|---|---|---|---|---|
| 150 to his right | high backhand | landed at 0.55 s (forearm, 8 damage) | 13 degrees past him | 0.38 s | 80 to 85 cm |
| 150, a Swordsman | high backhand | landed at 0.55 s (parried by the other's sword) | 7 past | 0.40 s | 80 to 84 cm |
| 90 to his right | high backhand | landed at 0.57 s (blocked by the shield) | 17 short | 1.05 s | 81 to 85 cm |
| 150 to his left | forehand | landed at 0.80 s (chest, 12 damage) | 2 short | 0.65 s | 63 to 84 cm |
| 120 to his left | forehand | met nothing | | 0.58 s | 65 to 84 cm |
| 150 right, blade leads 0.7 | high backhand | landed at 0.57 s (blocked) | 24 short | 1.23 s | 81 to 84 cm |
| 150 right, blade leads -0.7 | high backhand | landed at 0.57 s (blocked) | 12 past | 0.37 s | 80 to 85 cm |
| 150 right, blade goes 0.6 | high backhand | landed at 0.55 s, its fastest 20 m/s, 4 degrees right of where he faced, 136 degrees round in all | 13 past | 0.38 s | |
| 150 right, blade goes 1.5 | high backhand | landed at 0.57 s, its fastest 28 m/s, 50 degrees right of where he faced, 156 degrees round in all | 8 past | 0.40 s | |

(The two left-turn rows were run with the forehand before the turn was spread evenly through its wind-up;
after that change they were run with another forehand, which missed both times, and the first was put back
without being run again.) In the fight of six, a minute: 18 quick turns and 9 cuts made turning (by a
Medium, Swordsmen and Larges; 4 of them with the other striking at him); 4 landed, 5 met nothing; no error.

*What the sliders do and do not do, as measured.* "Blade goes" changes the sweep and its speed plainly.
"Blade leads" toward 1 has the blade land with his body 24 degrees short of his enemy where it was 13 past.
Toward -1 it changed little at this distance: with his enemy 1.5 m off the blade meets him as it is carried
round by the turn itself, before the cut proper. The blade landed at 0.55 to 0.57 s in every right-turn run
whatever the sliders. Whether the lag reads at a longer distance is not measured.

*Not good yet, and not verified.* The left turn: the forehand sinks his hips up to 20 cm (the recorded cut
is made from a low lunge; the aim lowers him to bring it down) and missed in two of four runs, the other
having stepped round. At 90 degrees the cut is short by 50 to 90 cm on its plan and lands on the shield; he
takes a second to face him. Twice in the fight a fighter began a second cut a quarter of a second after
the first. The cut's way back keeps him 1.3 s where the plain quick turn takes 0.6. "Blade leads" acts on
where in the turn the cut's fastest moment falls, not on an angle between blade and body. Nothing of this
has been seen in Unity, and the three takes have been measured, not watched.

**The turn cut's arm, after the first playtest (17 November, 21:10).** Renier: "Its pretty good. Close but
theres one issue. The arm does some weird twists as the swing comes to an end." In the session's frames
(2026-11-17_21-05-45; a high backhand at 15.65 s, a middle one at 39.33 s): after the blade has gone by, the
sword arm goes up and back with the blade pointing up behind him, comes round a second time, and only then
returns to his guard. Two causes, both found in the headless films as well:

- *The recording's own carry-through.* The backhands are taken from a man spinning right round. After the
  cut his body goes on round under his arm, so against his chest the arm wraps behind him and the wrist rolls
  over. On a fighter who stops turning as the blade lands that is his sword hand folded behind his back and
  unwinding. Now the recording is followed for a tenth of a second after the blade lands (the rest of the
  sweep across his front, out to his side), the arm is held there until 0.2 s after the landing, and then the
  arm, chest and head ease back to the guard over 0.35 s (`AttackScore.EndedShort`; Combat Feel "A Turn Cut
  Ends Short", "Turn Cut Sweeps On", "Turn Cut Carries On"; the lab's "After the cut: ends short / as
  recorded (old)").
- *The aim bending the arm onto the target after the blade had passed.* An attack's aim moves the sword hand
  up to 0.45 leg lengths (about 40 cm) and steers the wrist to put the blade on its target at the moment it
  plans the landing for (`AttackMaker.FitAt`), and the hand is pulled after a target that moves. For these
  cuts the plan's moment was 0.68 to 0.71 s in, while the blade, carried round by the turn, met the other at
  0.55 s. So 0.15 s after the blade had gone by, the hand was brought across and the wrist turned over. Now
  for a cut made turning round the hand is moved 0.08 leg lengths at most and the wrist steered 8 degrees at
  most, and the hand is not pulled after its target (`AttackAim.Sweeps`, `TurnSlashes.SweepsFree`; Combat
  Feel "A Turn Cut Sweeps Free"; the lab's "His aim: sweeps free / bent onto him (old)").

Headless, the other put 135 degrees round him at 2.0 m, where the cut meets nothing and so plays to its
end, old against new (`arm.py`: the sword arm from the blade's fastest moment to the end of the cut): the
elbow folds to 141 degrees against 82; the blade stands up to 119 degrees off the forearm against 84; the
blade turns at up to 3,600 degrees a second against 1,600; the blade's fastest 51 m/s against 29; the cut
1.27 s long against 1.07. At 1.5 m and 150 degrees the high backhand still lands at 0.53 s (blocked by the
shield) and the middle backhand at 0.72 s (blocked).

Not verified, and known: not seen in Unity. The left turn's forehand is a recording of a man who does not
spin, so it is not ended short; it sweeps free like the others. The plan still says "short by" a metre and
more for these cuts, since the aim no longer bends the arm to the target: the figure in the log means little
for them. With the aim this loose a cut is likelier to pass over or under a target that is not at the height
the cut goes at; how often is not measured. The explainer "Cutting As He Turns" was made before this fix and
does not show it.

**The turn cut, second playtest (17 November, 21:50).** Renier: the sword arm "seems fixed now", and two
more things.

- *"When I choose 'behind' while the turn also cuts then I get the feet breaking": the legs slip far apart, or
  the feet stay put and the legs twist round each other.* Reproduced headless (the other put 179 degrees
  round, `feet.py`): his legs were crossed for 1.05 s, and to his left 0.93 s; at 150 degrees never. It
  happens when the made cut turns his body past 180 degrees in all: the legs' 182 and the aim's 20 on top.
  His feet then end near where they began while his body has gone 200 degrees round. Where in the making of
  the clip the turn past 180 is lost is NOT found. What is done: a cut made turning round turns his body
  178 degrees at most in all (`TurnSlashes.MostRound`), so its legs go 158 at most and the rest is eased
  round as he comes back to his guard. After it: legs crossed 0.00 s both ways. Also `LocoClip.RootMotion`
  now counts a clip's turn frame by frame between two times, not as the shorter way between two facings
  (that was not the cause here, but it read a 200 degree turn as 160 the other way).
- *"Even if I'm on just turn, it seems like 1 in 10 turns also cuts."* The lab's "just turns" only switched
  the lab's "always" off; the fight's own rule (one turn in four with a cut when the other is not striking)
  stayed on. Now the lab's choice is the lab's two fighters' own (`UnitController.TurnCuts`,
  `DuelLab.TurnCuts`): "just turns" (as the lab begins: never a cut), "cuts" (every turn), "as in a fight".
  A fight's fighters are not touched by it.
- Found on the way: the other, once put round him, was sent back to the spot it stood on before
  (`ClipFighter.Home` was not moved with it). It is moved now. Headless the other still walks off after
  being put; why is not found, and it is why several of the headless cuts "met nothing".

**Get-ups made from the recordings (18 November, overnight).** Renier, 16:30 on the 17th: "you need to get
the new get up animations in, I still see the old ones". The pack "Getting Up" has a man getting up from his
back and from his front. Neither is played. Each is read off at twelve moments into the parts a get-up here
is made of, and the get-up system that was already there makes this fighter's keys of that.

- *Reading a recording off* (`Tools/Mocap/Combat/getup_keys.py`, which writes the table between the marks
  `<getup-takes>` in `Motion/GetUp.cs`; samples `getup_from_back.json`, 2.6 to 6.3 s of it, and
  `getup_from_front.json`, 1.6 to 6.1 s). At each moment: where the hips are; how the trunk is pitched,
  leaned and rolled (the line from the hips to between the shoulders, not the hips alone: a man piked on his
  hands with his back rounded read 15 degrees more upright at the hips than his shoulders were); how the
  back, chest and neck bend; where the ankles and wrists are; which way the knees and elbows point; how the
  feet are pitched. All of it against where he ends up standing and with his standing pose taken off, in leg
  lengths. He turns a little as he gets up; that is taken out, so the route is straight.
- *Making this fighter's keys of it* (`GetUp.MakeRecorded`). The table is fitted to his size and stance. The
  recording is mirrored when its last hand on the ground (the right) is not the one this fighter posts on:
  the sword hand with a shield, the free hand without one. Three things had to be fitted, because the
  fighter is not the recorded man:
  - His shoulders are further from his hips (0.53 m against 0.46) and his wrist lies lower on a flat hand
    (6 cm against 17). With the recorded hips and trunk his hands hung 20 cm short of the ground while he
    was "on his hands". Now a trunk that is nearer level than upright is tipped further (24 degrees at
    most), then the hips come down a little (a tenth of a leg at most), and what is still left the hand is
    off the ground by (`GetUp.HandsReach`).
  - His stance has one foot before the other; the recorded man stands nearly square. Until a foot sets off
    for its standing place it is measured from beside the other foot, as the recorded man's is.
  - The recorded man lies with his legs straight and stands up where his feet are, his hips sliding 0.83
    leg lengths toward them. This fighter's hips go as much of that as leaves his feet within reach
    (`GetUp.HipsTravel`, 0.92 of it for a Medium), and he is placed to stand up that far from where his
    hips lie (`GetUp.StandsAhead`).
- *What stays as it was.* He sets out from how he really lies; what he holds is kept off the ground; the
  shield comes up; he is turned toward his enemy while down; the feet end on the stance's own spots and the
  standing pose takes over for the last 18% of the time. A fall caught on a knee or sitting still rises by
  the keys made by hand. On a side he rolls onto his front and takes the front route.
- *Switch and levers.* Combat Feel "Get Ups Are From The Recordings" (on; off: the keys made by hand, as
  before) and "Recorded Get Up Takes" (0.5: the share of the recording's own 3.7 s and 4.5 s, so about 1.9 s and 2.3 s;
  a fighter is down about as long as before). Lab, under
  "Falls and getting up": "The way up: as recorded / made by hand (old)" and the lever; to see one, "Falls
  on demand" (Back, Forward, Left, Right) in the target test. Headless: `FIGHT_GETUPRECORDED=0`,
  `FIGHT_GETUPTAKES`.

Headless, the fall test's Medium knocked down backward and forward, made by hand against as recorded
(`gu.sh`; films `back_old/new.png`, `front_old/new.png`):

| | from his back, old | new | from his front, old | new |
|---|---|---|---|---|
| time (s) | 2.19 | 2.00 | 1.84 | 2.07 |
| a thigh's fastest turn on the hips (rad/s) | 12.5 | 10.9 | 14.8 | 8.3 |
| a thigh past what a hip bends (degrees) | 18 | 25 | 46 | 15 |
| a planted hand or foot off its spot at most (cm) | 4 | 5 | 6 | 0 |
| planted hands and feet slid in all (cm) | 5 | 5 | 5 | 0 |

From his front it is better on every count; from his back about level (the thigh 7 degrees further past the
hip, as he rocks forward over his feet in a deep squat). The swordsman (the recording mirrored), the unarmed
fighter, the Large (thrown, 450 N·s) and falls to either side all get up by it; nothing in its own body, no
body lost by the engine. The fight of six, a minute, seed 5: three get-ups each way, as recorded 2.1 to 3.1 s
(that run was at 0.6; the default is 0.5 now) against 1.95 to 2.3 s by hand; the fight ends the same way
(team A, 59 s against 56 s).

Not verified, and known: not seen in Unity, only as stick figures. From his back the recorded man sits up
with both hands behind him and rocks forward over both feet into a squat; that asks more of the knees than a
fighter's stand-up over one hand and one foot, and with a shield on the arm it may look wrong: the shield arm
is on the ground behind him for the first half. The recording's hands are open hands; a hand holding a sword
is still raised as far as keeps the blade out of the ground, so the sword hand does not really post. The
recordings "Getting Up" (from sitting) and "Standing Up" (a slower one from the front) are sampled but not
used: the first could replace the made keys for a fall caught sitting. The fighter no longer turns on the
ground as the recorded man does (40 degrees from his back); the turn toward his enemy is the system's own.

**The turn cut's legs with the other straight behind him: the real cause (18 November, overnight).** The cap
of 178 degrees was a way round it; what broke his legs is found and mended, and the cap is off.

- *The cause.* An attack's aim lengthens or shortens its lunge for the distance to its target: every part's
  travel from where it stood as the blow began is multiplied (`AttackMaker.Styled`, `Travel`; the fit's
  "Lunge", 0.3 to 1.8). For a lunge that is a longer or shorter step. A cut made turning round has a turn's
  legs: its feet's travel is the way they go round him. Multiplied, each foot was left part of the way
  round, or sent past it. With the other straight behind him the plan was short of him, the travel was
  multiplied (cut short, by where his feet ended; the multiplier itself was not read out), and the clip that
  was made ended with his left foot a fifth of a leg's length behind him and
  his right as far before him, in one line, where his stand has the left two fifths before him and a
  quarter to its side: his body right round, his feet near where they began. Found by writing the made clip out (`FIGHT_CLIPDUMP`) and reading
  its feet frame by frame (a script kept outside the project): the same turn at 150 degrees ended on his
  stand, at 179 it did not, and the only thing between them was the fit's travel. Now a blow on a turn's legs
  does not travel (`AttackScore.Turns`, `TurnSlashes.FeetGoRound`; `FIGHT_TURNCUTFEET=0` for as it was).
- *The cap is off* (`TurnSlashes.MostRound` 360; `FIGHT_TURNCUTROUND=178` for the cap). Headless, the other
  put 179 degrees round him, to his right and to his left: his legs crossed for 0.00 s; he faces him (within
  15 degrees) 0.38 s and 0.53 s after, against 0.48 and 0.55 s with the cap; 0.3 s after the cut he is 10 and
  2 degrees off him, against 13 and 7.
- *A cut that sweeps round drops and leans only a little for its target's height* (`TurnSlashes.MostDrop`
  0.07 of a leg, `MostLean` 14 degrees; any other attack 0.2 and 28). Turning 90 degrees to his left his
  hips sank 14 cm in the middle of the turn (67 cm at their lowest); now 6 cm (75 cm), and the cut still
  lands (the thigh).
- *The other, once put round him, stands where it is put* (`UnitController.Teleport` lets its feet go when it
  is moved more than 25 cm at once). Its feet were left locked where it stood before, 3 m off, and it was
  drawn back toward them at 1.2 m/s without taking a step, until its legs reached: headless that is why the
  other "walked off" and why cuts "met nothing". It also means the earlier headless figures for the turn
  cut were taken against an enemy sliding toward him, and are not to be trusted for where the blade lands.

Known, with the other standing still at 1.5 m: at 179 degrees (both ways) and at 90 the cut meets him (his
shield, or his thigh); at 150 degrees, both ways, it meets nothing: the blade goes by before the arm is out,
and without the travel the body does not close the distance. Not mended. Not seen in Unity.

**Convincing walking, steps 3, 4 and 5: whole steps, calmer feet in the dance, a diagonal turns the legs
(18 November, overnight).** All three are for a fighter told to walk on in his guard (the duel lab's orders
and the dance). The fight of six does not give that order, so it is not touched: the plan proves these in
the lab and the dance first.

*Step 3, whole steps* (`Locomotor.WholeSteps`, `WholeStep`; Combat Feel "A Step Once Begun Is Finished", on;
the lab's "His steps: whole / as asked (old)" under "Move him", with three levers). A step once begun keeps
its way, against the way he faces, and its pace until its foot lands; what he is asked at that moment is the
next step's. He stops only as a foot lands. The recordings for a step are chosen as it begins and not changed
in the middle of it. A step is taken at no less than 0.6 of the recording's own pace ("Whole Step Least
Pace") and 0.85 of its tempo ("Whole Step Least Tempo"): asked to go slower, he steps and then stands until
what he was asked has caught up by 12 cm ("Whole Step Pauses From"). Two things found on the way and built
in:
- Asked to go back the way he came while still going at more than half a metre a second, the step that
  follows is the one that stops him: his legs go on the way his body is going until it has slowed to 0.3 m/s,
  and only then step the other way. Stepping the new way at once, his feet went one way under a body still
  going the other for a third of a second.
- In a whole step his body takes up the step's pace with a half-life of 0.18 s (`StepHalfLife`; the walk's
  own is 0.35): a step's push, so the body and the feet agree sooner.

*Step 4, calmer feet in the dance* (`Dance.CalmFeet`; Combat Feel "The Dance Asks For Steps", on, "Dance
Steps Worth" 0.3 m, "Dance Stands At Least" 0.6 s; the lab's "Setting off: calm / restless (old)" in the
dance's settings). A dancer does not set off toward the other or away for less than a step's worth; once
going he goes all the way to the distance he likes (to 3 cm, where he used to stop 12 cm short and set off
again); once stopped he stands 0.6 s at least. Not held: the other too near, his own blow to come in for, a
blow to step out of.

*Step 5, a diagonal turns the legs* (`Locomotor.LegsTurned`; Combat Feel "A Diagonal Turns The Legs", on; the
lab's "A diagonal: legs turned / mixed (old)"). A whole step that goes between two of the guard's four ways
is taken on the nearest one's recording alone; the hips' joints and the legs are turned about the hips'
middle to the way he goes (47 degrees at most), and the spine, chest, head and arms are left as they are.
Only in whole steps, where the recording is chosen at a landing.

Headless, the footwork course (24 s, one fighter):

| | spells of walking | steps a spell | tempo against the recordings' | two recordings mixed | hips' band |
|---|---|---|---|---|---|
| as it was | 8 | 1.8 | 0.68 | 0% | 6 cm |
| whole steps | 7 | 4.1 | 0.88 | 14% | 6 cm |
| whole steps, legs turned | 7 | 4.0 | 0.89 | 0% | 7 cm |
| the night's last build (with the eased ending) | 8 | 3.4 | 0.90 | 0% | 6 cm |

Headless, the dance, Plain against Plain, 48 s, the two fighters, on the night's last build. (A dance run
is the same every time for the same build; but any change to how they move sends the dance another way, so
two rows are two different dances and small differences mean nothing.)

| | spells | steps a spell | tempo | two recordings mixed | set off from a stand |
|---|---|---|---|---|---|
| as it was | 25, 21 | 0.6, 1.2 | 0.65, 0.71 | 49%, 46% | once every 1.2 s each |
| whole steps and legs turned | 20, 20 | 1.6, 2.3 | 0.89, 0.90 | 3%, 11% | once every 1.0 s each |
| and calmer feet | 23, 26 | 1.6, 1.2 | 0.90, 0.88 | 13%, 4% | once every 1.4 s each |

An earlier build of the night with whole steps and the recordings still mixed (58 s): 1.0 and 1.7 steps a
spell, two recordings mixed for 33% and 50%.

The hips' band stays 7 to 8 cm and they rose or sank 10 cm within 0.3 s up to twice a fighter in each run,
with or without. A foot on the ground slides 2.8 to 3.0 cm a second with whole steps and the recordings
mixed, against 4.9 before; with the legs turned it is 4.6 to 5.4 again (a planted foot swivels on its ball
when the next step's recording has it turned another way). With the legs turned the hips' line stands up to
32 degrees off the shoulders' in the course (about 20 for most of it) and 38 to 44 in the dance, against 9.

*Step 4 does little so far, and I say so.* On an earlier build of the night the calm dance set off once
every 2.7 s each against 1.0, and I took that for the step's effect; on the last build it is 1.4 s against
1.0, and the spells of walking are not fewer (49 against 40). Counted: every one of their stops is "he is
where he wants to be", none is a gesture or a blow; and something presses (the other too near, a blow to
come in for or to step out of) for half of all the time, when nothing is held back. So the restlessness is
mostly the dance's own pattern of coming in and going out for its blows, which this step does not touch.
It wants another pass: the dance asking for steps, as the plan has it, in place of a way and a pace.

Two things added after the first measurements:
- *Told to stand in the middle of a step, he finishes it at 0.45 of its pace* (`Locomotor.StepEndsAt`). At
  its full pace he went 26 cm on after the order ended; now 5 to 6 cm (walk up 5 s, stand: he stands 5 cm
  past where the old walk stood).
- *Where his step ends is his spot* (`FightLoop`, a target told to stand while `Locomotor.StepUnderWay`).
  Held to the spot he was on when he was told to stand, he finished his step past it, stepped back 30 cm,
  and forward again: a fidget after every stop in the duel lab. The dance did not have it.

Against the plan's own checks: the tempo is within 20% of the recording's (met: 0.88 to 0.90); no change of
recording between landings (met by construction, for a walk kept up); two kinds at once: 3 to 13% of the
walking (the plan asks none: what is left is walking that is not a walk kept up, a shove walked off or a
blow thrown on the move); every spell one whole step or more: 1.2 to 2.3 on average in the dance, 4 in the
course, and a spell cut off by a blow or a turn can still end before its foot lands; stand to walk at most
once every 3 s while the other stands still: not met (see step 4 above), and not measured with the other
standing still.

Not verified, and known: none of it seen in Unity. In the course he ends up to 0.7 m from where the old
walk left him after the stop-and-go part (measured before the eased ending). The stands between steps
hardly happen at the dance's paces (3 to 5 a fighter in 48 s): the dance mostly asks for more than a step's
least pace. The explainer "Whole Steps In The Dance" was made on the earlier figures and corrected after.
The fight of six, a minute, on the last build: as on the build before these steps (team A after 51 s, 67 blows, no body lost by the engine), as it should be: it gives no order to walk on. Step 6 (far and near, with the running turn-rounds) is not begun.

**Renier's first look at the recorded get-ups, and the panel (18 November, morning).** "getting up from the
back has one leg, the leg he pulls back, positioned in a way that it intersects with the hips"; and the
debug panel "has stuff added now and then causing the debug to move the layout", with headings that are hard
to spot ("when I was looking for Bash").

- *The foot through the hips.* The recorded man, sitting up from his back, tucks one foot back under his other
  thigh, across his middle, and rises over it. Sitting with its hips on the ground, this body took that foot
  straight through them: from 63 cm before the hips to 17 cm to the far side of their middle and 9 cm
  behind, its shin passing 16 cm from the hips' middle (the get-ups made by hand come no nearer than 30).
  Now, while the hips are low, a foot that is near them fore and aft stays out at its own side by 0.3 of a
  leg (`GetUp.ClearOfHips`, `FeetClearHips`, `FootClearsHips`; Combat Feel "Recorded Get Up Feet Clear The
  Hips" and "Recorded Get Up Foot Clears"; the lab's "A foot by the hips: beside them / under them (old)"
  and its lever; `FIGHT_GETUPCLEAR=0`). It is let back to where the recording has it over the whole rise of
  the hips (0.3 to 0.9 of their standing height). Headless: the shin no nearer than 31 cm, the foot 34.
  The cost: as he stands the thigh of that leg swings in at 18 rad/s for a moment, where it was 11 (a
  Medium and an unarmed fighter; the swordsman, who takes the recording mirrored, 8). Tried and dropped:
  pushing the foot straight away from the hips (the thigh at 35 rad/s) and pointing its knee forward (a thigh
  115 degrees past the hip's range in the get-up from the front).
- *"As recorded" that changed nothing.* His log shows him pressing the older switch to "as before" and later
  "as recorded": with the older one off the newer one did nothing and said nothing, and the get-ups he then
  watched were the oldest ones (a planted hand 37 cm off its place). The two switches are one choice of
  three now: "The way up: as recorded / made by hand / oldest".
- *The map of what he leaves open no longer moves the panel.* A part's name grew "(shield)" as it was
  covered and wrapped onto a second line, so the rows changed height several times a second. Each name is one
  line of one width and height now, clipped, with R and L for right and left so the longest fits.
- *Headings.* A heading inside a section is larger, amber, on a tinted bar across the panel with a bright
  edge at its left (`Heading`).

Not seen in Unity. The compile check's stand-ins for Unity's panel calls gained the ones these use
(`GUIStyle.padding`, `normal`, `clipping`, `GUILayoutUtility.GetRect`, `GUILayout.ExpandWidth`).

**Fakes that are real attacks cut short; the other's answer; the panel's colours and search (18 November).**
Renier, 07:16: the sword fakes "just don't look like real fakes. They kind of look like he's sending signals
with his sword ... Lets just make the fakes use the actual attacks, except they are cut short. Not always cut
to the exact spot ... both the light and heavy attacks ... a slider that moves the cut-off forwards and
backwards, and another for how much they can differ ... remove the blade swing sounds from the fakes. Then I
want the opponent to sometimes do a block or dodge in response to a fake attack ... for now lets just get the
look of this reaction right. In general I have not seen the dodges in the dance yet". And the panel: "colour
code separate sections. Heading and buttons the same colour but the buttons should have a darker toned down
version; add a search box that works similar to how it works in unreal."

- *Why the old fake read as a signal.* A feint played an attack's wind-up and took it back before the blade
  came forward (`PostureMaker.FeintDepth`, 0.85 of the wind-up): the sword went up and came down again, and
  nothing ever moved toward the other.
- *A fake is a real attack, cut short* (`PostureMaker.CutShort`; Combat Feel "A Fake Is A Real Attack Cut
  Short", on). It is made as any blow is: a light or a heavy attack drawn for each fake (`CutFrom`, "Fakes Are
  Cut From": both, light, heavy; `NextFakeClass`), aimed at the other's chest by the attack maker. It is
  played to a point on its way from where its hips start in (0) to where it would land (1): the cut-off
  (`CutAt`, "Fake Cut Off", 0.5) give or take what it varies by, drawn for each fake (`CutVaries`, "Fake Cut
  Off Varies", 0.2: anywhere from 0.3 to 0.7). There it is checked for 0.04 s and the upper body goes straight
  back to guard in 0.4 s (`CutHold`, `CutBack`); the legs wind back the way they came. It strikes nothing and
  is not heard (`StrikeHits.FakeClip`: the blade's hit step and its swing sound are skipped for that clip).
  The dance's long fake is the same, held 0.25 s where it is checked; the fakes test's "Short fake" is cut at
  the near end of the range and its "Long fake" at the far end and held (`PostureMaker.NextCutAt`,
  `NextCutHold`). A fake lunge is as it was.
- *The other's answer* (for the look only: being taken in costs nothing yet). For each fake a draw decides
  whether the other takes it for the blow (`PostureMaker.FakeBelieved`, "A Fake Is Taken For A Blow", 0.6).
  If so the fake reads to him as the attack it is cut from until it is checked (`StrikeHits.ShotRead`,
  `BlowRead.Feint`), and he answers that: his shield or sword goes to meet it, or he dodges
  (`FakeDodged`, "A Fake Taken For A Blow Is Dodged", 0.5; `ClipFighter.FlinchAtFake`). Said over a dancer's
  head ("DODGES A FAKE", "BLOCKS A FAKE", "PARRIES A FAKE") and written to the playtest log (`fake`).
- *Why no dodge was ever seen in the dance.* A dodge was refused while the dodger was in a clip of his own,
  and a dancer is in a gesture of his own most of the time. Now he lets his own gesture go to dodge
  (`ClipFighter.DodgeAway`, `DodgeDropsGesture`; in a real blow of his own he still cannot, and blocks), and
  he ducks away from the side the blade comes from, where the side was drawn before. To see them: the lab's
  "Dodges: as set / every one (to see them)" (`ClipFighter.DodgesEvery`): every fake is believed and dodged,
  and in the dance every real blow seen in time is dodged, high or not.
- *In the lab.* With "Fakes test: settings" and "The dance: settings": "A fake is: a real attack cut short /
  the old feint", levers "Fake cut-off", "Cut-off varies", "Fake believed", "Fake dodged", "Cut from: both /
  light / heavy", "Dodges: as set / every one", and a line for the last fake (which attack, cut where).
  Headless: `FIGHT_FAKECUT=0`, `FIGHT_FAKECUTAT`, `FIGHT_FAKECUTVARIES`, `FIGHT_FAKEFROM`,
  `FIGHT_FAKEBELIEVED`, `FIGHT_FAKEDODGED`, `FIGHT_DODGEALL=1`, `FIGHT_DODGEDROPS=0`; the lines `DANCEFAKES`
  and `FAKEANSWER`.
- *Headless, two Plain fighters dancing 30 s* (one dance each: counts, not rates). As it is
  set: 21 fakes thrown, 13 taken for a blow, 7 of those blocked and 6 dodged. Before a gesture could be let
  go for a dodge: 14 taken for a blow, 1 dodged. With "every one" on: 14 taken for a blow, 13 dodged, 1
  blocked (he was in a blow of his own). With the old feint: none answered, as before. The fakes test, its
  pattern for 20 s: 17 fakes, 8 taken for a blow, 4 dodged. A fight of six ran its 40 s with nothing broken
  (a fake is a lab and dance thing so far: the six throw none).
- *Known: the fakes test's lesson is blurred.* Its short fakes "leave the chest covered" and its long one
  leaves it open. A real attack uncovers the chest whoever cuts it short: headless the short fakes now leave
  it open 0.2 to 0.5 s too, so a swordsman who only reacts has more to go for. Not mended; the reader still
  reads the pattern by its count.
- *The panel: a colour for each part.* Every heading and every section's bar has a colour of its own, from
  its name (so it is the same every time; two that follow each other are never neighbours on the colour
  wheel). The buttons under a heading, down to the next, are a darker, greyer tone of its colour; the one
  chosen in a row (its name in square brackets) is a brighter tone; an action's settings bar is the darkest
  (`ZoneOf`, `ZoneTone`, `ZoneButton`, `HeadingBar`). The aim test's chosen part is written "[Head]" like
  every other choice, where it was "> Head".
- *The panel: a search box* (`SearchBox`, at the top of the panel). With words in it every section is
  looked through and only the settings with those words in their name are drawn, each under its section's
  bar and its heading; the Actions window is filtered by the same words. A row (a name and its choices) is
  one setting, shown whole if the words are in its name or in any of its choices (`BeginRow`, `EndRow`,
  `Shown`; a row's words are read off in one layout and used from the next). Several words: all of them.
  What is measured, and text that is no setting, is left out while searching. Escape or "×" clears it; Enter,
  or a press anywhere else, gives the keys back to the game: while the box has them no key does anything in
  the game (`SandboxInput.Typing`). The walk lab's own panel has neither colours nor search yet.
- *The panel drawn without Unity* (`Tools/CompileCheck/Runner/PanelCheck.cs`:
  `PhysicsBattler.Tests.dll panel [battle|duel|aim|tubes|targets] [search words]`). The compile check's
  stand-ins for Unity's panel calls now write down every control asked for, and the real panel and Actions
  window are drawn once to lay out and once to paint. Checked: both passes ask for the same controls in the
  same order, and every row, column and scrolling view begun is ended. All five places pass, with and without
  a search; with words given it prints what is left of the panel (the duel lab, "dodge": Dodge left, Dodge
  right, "Fake dodged", "Dodges:").
- *The duel lab's footwork course leaves a line in the playtest log when it ends* (`course`): what the meter
  made of it and which walk it was on. The course Renier ran on the 18th was the walk lab's, which logs its
  own; the duel lab's is the "Footwork course" button there.

Not seen in Unity: the look of the checked attack, the dodge, a gesture dropped for a dodge (it ends with a
short fade), the colours, typing in the search box.

**Fakes, second pass; bashes in the dance (18 November, after Renier's look at the cut-short fakes).** "Only
the fast or light attacks look right, not the heavy. There are too many fake attacks and too few real
attacks. Each unit should have a ratio for fake to real attacks. Fakes only look right for the attacks where
the arm pulls back to behind his back. Attacks with a wind up where the sword winds up by going in to
position in front of his chest do not look right." And the bash: small and medium bashes "for when the enemy
comes too close into your space"; the heavy one "more as an attack to try and break the enemy's guard", which
the enemy can counter "by doing a medium bash of his own", with "the debug slider for the percentage chance".

- *Fakes are cut from light attacks* (Combat Feel "Fakes Cut From": 0 light, the default; 1 heavy; 2 both;
  the lab's "Cut from: light / heavy / both". A new setting, so a scene saved this morning does not keep the
  old "both").
- *Fakes are cut only from attacks that wind up behind him* (`AttackScore.WindsBehind`,
  `PostureMaker.FakesWindBehind`; Combat Feel "Fakes Come Only From Wind Ups Behind Him"; the lab's "Wind-up:
  behind him only / any (old)"; `FIGHT_FAKEBEHIND=0`). Measured on each attack's recording, against the way
  he faces as it begins: before the drive the blade must go at least a quarter of a leg behind his sword
  shoulder, and the sword hand must not first go out before his chest (more than 0.22 of a leg forward of the
  shoulder, or 0.1 across to his other side); a thrust never counts. Of the sword and shield's three plain
  attacks (`FIGHT_SCORES=1` prints them): the forehand's blade goes 46 cm behind him and its hand stays by
  his shoulder: faked. The rising cut's hand first goes 35 cm out before his chest and 42 cm across it, the
  sword upright before him, and only then round: not faked. The thrust: not faked. So for now every fake is
  a light forehand, on its own legs or another attack's; any attack added later that winds up behind him is
  faked too. A fighter with no such attack fakes from any.
- *A ratio of fakes to real attacks, each fighter's own* (`FighterLevers.Fakes`, "Fakes" under "Each his
  own": 0.5 is one fake for every two real attacks; Plain and Prowler 0.5, Strutter 1, Bull 0.25;
  `Dance.FakeRatio`, Combat Feel "The Dance Keeps Fakes To A Ratio", the lab's "Fakes: to his ratio / all his
  pattern has (old)"; `FIGHT_FAKERATIO=0`, `FIGHT_FAKES_PER`). A fake his pattern has that the ratio does not
  let him throw is left out and the pattern goes on at once, so his real attack comes sooner; if it was his
  tell, he drops before his blow instead. A hard bash counts as a real attack.
- *Bashes for space* (`Dance.StepSpace`, `Dance.SpaceBashes`; Combat Feel "The Dance Bashes For Space"; the
  lab's "Too near: one bashes him out / both bash (old)"; `FIGHT_SPACEBASH=0`). Before, each dancer bashed
  whatever was within 1.2 m whenever he could, with a small bash. Now, as the two come within that
  ("Bash within"), one of them bashes, once. If one came in at the other (a blow of his under way or due, or
  he was closing faster by 0.25 m/s), the one come in at bashes him back out: a medium bash half the times
  ("Medium bashes", Combat Feel "Space Bashes That Are Medium"), a small one the rest. If neither did (they
  drifted together), one of them, drawn, throws a small one to push them apart. One who cannot just then
  leaves it to the other. Said over him: "BASHES him back out", "BASHES them apart".
- *A hard bash is an attack* (`Dance.HardBashAttacks`, `BashAttack`; Combat Feel "A Hard Bash Is An Attack In
  The Dance"; the lab's "Hard bash: an attack / never (old)"; `FIGHT_HARDBASH=0`). Some of a dancer's own
  blows are a hard bash instead of a cut: his own "Guard breaks" (`FighterLevers.GuardBreaks`: Plain 0.2,
  Prowler 0.1, Strutter 0.15, Bull 0.3; `FIGHT_GUARDBREAKS`), twice as many after a blow of his that the other
  stopped. It is thrown from 1.3 to 2.4 m off.
- *A hard bash is countered by a medium bash* (`ClipFighter.CountersHardBash`, `SeesHardBash`,
  `StepCounter`; Combat Feel "A Hard Bash Is Countered By A Medium Bash"; in any fight, not only the dance;
  `FIGHT_COUNTER=0`). One with a shield who sees a hard bash coming braces where he stands and drives his
  board out with a medium bash, timed to be out as the other arrives (a gesture of his own is let go for
  it; in a blow of his own he cannot). Whether that keeps him on his feet is a draw: "Counter holds", 0 to
  100 per cent, 75 as it is (`CounterHolds`, Combat Feel "Counter Bash Holds"; `FIGHT_COUNTERHOLDS`; to be
  tied to defensive skill later). Held: the boards meet, he is shoved back a step, and the basher is stopped
  and thrown back off the board a step himself. Not held: he goes down as before. A counter does nothing by
  itself: what comes of the two boards meeting is settled as the hard bash arrives (board on board, within
  40 cm, or his middle reached). Said over him: "COUNTERS the hard bash: it holds", "HIS COUNTER DOES NOT
  HOLD". To see one: the duel lab's "Hard bash" at the other, both with shields; "The other's bash" under
  "Last bash" says how his counter went.
- *One who is down is let up* (`Dance.LetsHimUp`): the other throws nothing at him until he stands. A choice
  made so the dance can be watched; not asked for.
- *Headless* (two Plain fighters, one dance of 60 s each: counts, not rates). With
  everything of this step off: 32 fakes (16 each) and 19 real attacks, all cuts. As it is now: 11 fakes (5
  and 6), all light forehands, and 18 real attacks (15 cuts, 3 hard bashes); 32 fakes were left out by the
  ratio. So the fakes are a third of what they were, but the real attacks are no more than before (18 against
  19 in the minute): leaving a fake out brings the next blow sooner, and a fighter knocked down and getting
  up takes it back. Not mended: how often a dancer attacks is his pattern's beat, which this step did not
  touch. Of the 3 hard bashes, all were countered: 2 counters held, 1 did not and he went down. Bashes for
  space: 2 (1 small, 1 medium), both by the one come in at: two Plain fighters seldom come within 1.2 m.
  Two Bulls for 40 s (who like to be nearer): 3 bashes for space (1 small, 2 medium; 2 at one who came in, 1
  drifted together). With every blow a hard bash (`FIGHT_GUARDBREAKS=1`, 40 s): 8 thrown, 6 met by a counter
  that held, 1 knocked him down, 1 left no outcome (it met nothing, or the run ended first). In the duel lab's line-up, a hard bash from 1.9 m at the
  other: with "Counter holds" at 100 his counter held both times and they ended 1.65 m apart; at 0 he was
  knocked down 2.5 m back. The fight of six ran its 40 s with nothing broken.

Not seen in Unity: any of it. In particular the look of the braced counter, of the basher thrown back off
the board (his leap is dropped for a step back), and of a medium bash thrown from inside a metre.

- *His tuned cut-off is the default.* The playtest log of 18 November 08:46 ends with "Fake Cut Off" moved
  from 0.5 to 0.12 and fakes cut from light attacks only: 0.12 is the default now (give or take 0.2: from the
  moment the hips start in to a third of the way to the target). He also brought "Long Fake Hold" from 0.25
  to 0.09; that one is stored in the scene, so it is his to keep there ("Keep what I tune").

**A bash that is countered looks weak: research and a plan, nothing built (18 November, 11:12).** Renier:
"when the shield bash is blocked by another shield bash, all of that force just disappears ... Their bodies
barely move or respond to the collision. I want it to look like their bodies are absorbing this energy. I
want it to have some unpredictable effect on them." He asked for research and a planning document first, and
for whether the finding is local or belongs to the whole system. The document is "Impacts that carry weight"
(https://claude.ai/artifact/HkqvPHoe22TWSjZwxvdW5V). In short:

- *Measured.* A hard bash is an 80 kg body at 5.3 m/s: 424 N·s, ten times a sword cut on a shield (35 to 46
  N·s). On the recorded skeletons a countered hard bash changes the defender's trunk lean by 1 to 5 degrees
  and drops his hips 0 to 3 cm; a light cut on his shield changes his lean by 2 to 16 degrees.
- *Why.* A sword blow goes through `ClipFighter.TakeHit` (force on the physical body, `Recoil`, `KnockArmBy`,
  `Soften`, poise, the hit stop, the drive back and the stumble; `Jar` for the striker). A bash goes round it:
  `Bashed` only pushes the hips' ground speed or knocks him over, the basher gets nothing, and a shield held in
  cover is not looked at. The counter added this morning then wrote both speeds in by hand (1.2 m/s and 1 m/s
  back): of 424 N·s in, 16 come out.
- *The rule proposed for the whole game.* Every impact is an amount of momentum passed between two bodies;
  each gets it, equal and opposite; both show it.
- *The plan, five steps, each behind a switch:* an impact meter; one door for every impact (the bash through
  the sword blow's own); a momentum ledger for body on body (speeds worked out, levers for bounce, what the
  feet take and how much it varies); the bodies absorbing it over the quarter second a collision lasts; off
  balance by the legs' own rule, and the same door for the rest (the striker's jar on a shield, a leap landing
  on someone, kicks).

It waits for his word.

**Impacts that carry weight, steps 1 and 2: the meter, and the bash through the door (18 November, midday).**
Renier's answers to the plan (11:54): a counter that holds still costs the defender ground: yes; the basher
is thrown back a step for a medium and a hard bash, a light one for me to decide; sword blocks on a shield
feel weak too, "I want you to also look into this"; and on unpredictability: "I don't want you to add fake
unpredictable behaviour, I just want the variations that come from the real physics collisions". So nothing
in these steps is drawn by lot.

- *Step 1, the impact meter* (`Locomotion/ImpactMeter.cs`, `ImpactRecord`, `ImpactShown`; it changes nothing).
  For every bash that lands and every blow stopped on a shield, it watches the two bodies for 0.6 s and says:
  what came in along the line between them (two bodies meeting: each one's weight times its speed over the
  ground; a blade: the blow's own momentum); what is left as motion of the two bodies afterwards (their speed
  over the ground from 0.25 to 0.6 s after); what that leaves unaccounted for; what the door handed over and
  back; and what each body showed. First the reaction's own share, whatever else the body was doing (a bash of its
  own, a cut carried through): how far the reaction rocked its upper body and drove its arms back. Then all of
  it together on the posed skeleton: the angle its line from hips to head moved through, its shoulders turned,
  its hips dropped, its head shifted over its hips. In the lab: "Impact meter" under "Last bash". Headless: `IMPACT` lines.
  In the playtest log: `impact`. (The plan's own first figures took the trunk's lean on the line from hips to
  chest, which shows about a quarter of what the spine and chest turn together; the meter's line to the head
  shows it whole. The two are not to be compared.)
- *Step 2, the door* (`ClipFighter.TakeImpact`, `BashIsImpact`; Combat Feel "A Bash Lands As An Impact", on;
  the lab's "A bash lands: as an impact / as a shove (old)"; `FIGHT_BASHIMPACT=0`). A bash hands the other the
  momentum its board arrives with (its shove times his weight: 94 N·s for a small one, 285 for a medium, 468
  for a hard one in the lab) at the point it really met him, through the reactions a sword blow has: force
  on his physical body (`Upper.Strike`, "Bash Force On The Body", 0.12 of it); his upper body rocked about his
  hips by the lever of where it landed (`Recoil`; "Bash Rocks The Trunk", 0.17 of it against a blade's 1; it
  may rock him further than a blade may, "Bash Rocks Up To", 57 degrees against 34); his body bent where it
  landed (`AddImpact`); the part gone soft; poise lost (3 for each m/s of the shove).
  - *On a shield.* What the board meets is looked at now: his shield driven out in a counter, or held in
    cover between the two of them (its middle before his body toward the basher and within its own reach of
    the bash's line), or his body. On a shield his shield arm is driven back by the weight of it, as far as
    it has room before his chest, and his guard is knocked open if it is heavy enough (`KnockArmBy`, the
    blows' own "Shield Give Per Weight" and "Guard Open From"); what the arm takes comes into his trunk at his
    shield shoulder, so that is where its lever is: mostly back, and a little round, because the shield is on
    one side of him.
  - *Equal and opposite.* The basher takes the same amount back at his own shield arm (half of it when he
    met a body that gave way: `BashBackOffBody`): his arm driven back, his upper body rocked.
  - *Both stand still a moment as it lands* (`ImpactStop`, the blows' own least and most hit stop: 0.04 to
    0.16 s by the shove). Not when it knocks him over: held still as he was thrown, he went 0.8 m where he
    should go 2.5.
  - *What it does not change:* where the two end up. The shove, the stumble, the knock-down and the counter's
    fixed speeds are as they were; working those out is step 3.
- *A blow stopped on a shield jars the striker by all of it* (`StrikeHits.BlockJarsStriker`, `ClipFighter.Jar`
  with `whole`; Combat Feel "A Block Jars The Striker By All Of It", on; the lab's "A block jars: the striker by
  all of it / by a part (old)"; `FIGHT_BLOCKJARS=0`). What the shield took comes back at his weapon hand and
  rocks him as it rocks the blocker (0.6 of a blow on the body). Before, he was rocked by what a heavy weapon
  would not keep of its swing, times "Jar Scale": about a tenth of it. In any fight.
- *No dice.* One bash differs from the next by where the two boards or the board and the body really met, by
  which way the board was driven, and by how each stood: the lever about the hips is taken from those.
- *Measured* (headless, the duel lab's line-up, one bash each, old against new, on the meter). The reaction's own rock of the upper body (the spine and chest together), on both men alike, where it was 0
  to 1 degree before: a small bash 10 to 11 degrees, a medium one 27 to 28, a hard one 37 to 38. With all
  else each was doing, the line from hips to head of the one who took it moved: a small bash 6 degrees (2
  before), a medium one on his shield 18 (5), a hard one on his counter 18 (7); his head over his hips 6, 18
  and 17 cm (2, 5 and 6). The shield arm driven back: 8 to 25 cm (0 to 2). A hard bash that is not held
  still knocks him down 2.4 m back (2.55 before). A cut stopped on a shield (the aim test, the same two
  blows of 23 N·s old and new): the blocker rocked 20 degrees and his shield arm back 13 cm, both as before;
  the striker rocked 17 degrees, where it was 3. The fight of six ran its 40 s; it goes
  another way with the striker jarred (68 blows and 3 against 2 standing, where it was 60 and 3 against 1), and
  its fighters have a forearm or hand inside their own bodies for 4.1 s in all against 3.1 s (six fighters,
  40 s): to be watched.
- *Known.* The hips do not drop and the feet do not slide: that is step 4. A counter's two speeds are still
  written in, so the meter still reads about 290 N·s unaccounted for on a countered hard bash: step 3. On a
  countered hard bash the one who countered has a forearm or hand inside his own body for about a tenth of a
  second in all (none before). A medium and a hard bash rock the one who takes them about as far as each other:
  the rock is near its most by a medium one.

Not seen in Unity.

**A shield out of the floor in a get-up; the counter of a fighter's own crossings and the plan for them; a first
damping of impacts (18 November, afternoon).** Renier, 14:22: "for the new get up animations the shield now goes
through the floor. You at some point did a good update for your get up animations where you fixed this. please do
this for the new get up animations. ... I still see a lot of shield intersections with the shield users own body.
Here he is slashing through his own shield. Shield going through the body. This happens all the time. Please do
some research and try to come up with a plan that fixes this once and for all. ... Try to make it so that there
is a little dampening for physics objects that collide with the player bodies. but lets also try it for weapon on
shields currently everything is very bouncy and shaky when they hit things. I want there to be some absorption of
impacts so that it does not look so much like toys fighting." The research and the plan are the document "A
shield that stays outside its bearer" (https://claude.ai/artifact/9UZ1xrAueNhCM6Yn4wg2NN); what was built is here.

- *Why the shield went through the floor.* The get-up has had its own care of a shield since the first shields
  (the board flat under the forearm while the hands push, held up toward what he fights as it comes off them,
  kept out of his body, and what both hands hold kept out of the ground: `GetUp.HoldIn`, `HoldShieldUp`,
  `ShieldInBody`, `PosePipeline.KeepWeaponsOffGround`). All of it reads the weapons' measures off the loadout
  (`GetUp.Loadout`), which the old pose pipeline handed it. That pipeline went on 11 November, and a clip
  fighter's get-up was never handed one: none of it has run since. The made get-ups hid it (their hands were
  seldom on the ground); the recorded ones put the hands down.
- *A get-up knows what is held* (`ClipFighter.Kit`, set by the unit; `ClipFighter.GetUpsKnowWhatIsHeld`; Combat
  Feel "Get Ups Keep What The Hands Hold Off The Ground"; the lab's "A get-up knows what is held" with the
  get-ups' settings; `FIGHT_GETUPHELD=0`). And the ground clamp has its whole weight from a get-up's first step
  (`GetUp.HeldClearFromTheStart`; `FIGHT_GETUPCLAMP=0`): coming in over the blend from how he lay, as first
  made, it left a shield 17 cm under the ground a quarter of a second into a get-up from the front.
- *Measured* (headless, the tube test's shield bearer felled backward and forward; `GetUpShieldSeenLowest` and
  the get-up's line in the report: "its shield over the ground at the lowest"). From his back: the rim 32 cm
  under the ground, in it 0.85 s; now 6 cm over it, never in it. From his front: 36 cm under, 1.13 s; now 2 cm
  under at the worst, as it starts, never more than 2 cm. The sword too: in the old front get-up's film the blade
  dips under the ground once, in the new one it does not. In two fights of six, three get-ups of shield bearers:
  the rim 0, 0 and 2 cm over the ground at the lowest.
- *What that costs.* Starting a get-up from his front he lies on his board: the board is in his own hips for 0.05
  to 0.35 s at the height of the floor, and about a tenth of a second some 1.1 s in the raised board passes his
  head. In two fights of six that was 0.6 s of 4.7 s and 0.4 s of 1.7 s of getting up, where there was none (and
  a shield through the floor). Step 4 of the plan.
- *The counter of own crossings* (`SelfCrossings`, `ClipFighter.MeasureOwnCrossings`; it changes nothing). On
  the body as it is seen: his blade through his own board, his sword forearm and hand through it (lines against
  the board's disc), and the board's rim or face inside his own head, trunk, thighs, shins or sword upper arm;
  counted by what he was doing, by attack, by part, and whether the pose itself had the crossing too. In every
  headless report ("own crossings, as seen"), and in Unity each stretch of 0.05 s or more is written to the
  playtest log (`cross`). Strict: lines and a thin disc, where the meshes are thicker.
- *What it counted, as the game was this morning* (`FIGHT_ABSORB=0 FIGHT_GETUPHELD=0`). The fight of six (two
  shield bearers, 40 s): blade through his own shield 1.5 s, sword arm through it 1.2 s, shield in his own body
  1.3 s: 5% of the time, a crossing every 2.8 s for each bearer. Striking is two thirds of it (the lunging chop:
  blade through the board 0.65 s and arm 0.47 s of 9.5 s; the rising cut: arm 0.27 s of 3.4 s; the combo: the
  board in his body 0.63 s of 4.8 s); stumbling a third of its own time. The board is in the shield-side thigh
  first (0.63 s), which no guard knows. The pose itself had three quarters of the blade's crossings and nine
  tenths of the sword arm's, but a third of the board-in-body: that one is the physical body off its pose. The
  dance (two fighters, 60 s): 1.2% of the time, one every 8 s.
- *Impacts are absorbed: a first trial* (`ClipFighter.ImpactsAbsorbed`, `Absorbed`; Combat Feel "Impacts Are
  Absorbed", "Absorbed How Much" 0.6; the lab's "Impacts: absorbed / as before (old)" and "Absorbed how much"
  with the bash's settings; `FIGHT_ABSORB=0`, `FIGHT_ABSORBED`). What made hits look like toys was looked for
  first. Not the physics materials: all have no bounce already. The trunk's and the head's reaction came back on
  a spring damped at 0.55 of what stops one dead (`ReactDamping`): it swings past and back. The striker's hand
  is thrown back off a block or a parry by design ("so even a light blow visibly bounces": Combat Feel "Least
  Rebound"). A knocked arm or leg comes back as quickly as it went. Absorbed: the trunk and head settle without
  swinging past (`ReactSettles`: the damping toward 1; the kick made larger by `ReactGain`, worked out by
  stepping the spring as the game steps it, so a reaction goes as far as before); trunk, head and legs come
  back slower than they went (`ComesBack`: seven tenths at 0.6); a blade stopped by a shield or a parry keeps
  about half its spring back and backs out less (`ReboundKept`).
- *Tried and taken out: the knocked arm coming back slower.* A shield knocked back onto his own sword stayed
  there: in the fight of six his blade was through his own board 4.2 s of 80, against 1.5 s. A knocked arm comes
  back as quickly as before; it waits for step 1 of the plan.
- *Measured* (the impact meter, which now also reads the swing back and the seen body against its pose, and
  watches a whole second: `ImpactShown.SwungBack`, `Off`, `OffPast`, the turns; `FIGHT_SHAKETRACE=1` writes
  every step). A medium bash: both trunks rocked 27 to 28 degrees old and new; swung back past where they set
  out from 3 degrees, now 0.1. Blows on shields in the dance (7 in each of two dances of 45 s, so a direction):
  swung back 1.7 degrees on average (2.7 at the most), now under 0.1; the striker's sword hand knocked 19 cm
  off its pose, now 14.
- *Not mended: the tremble.* After a hit the seen hands and head ripple about a centimetre about where the pose
  has them, changing direction 5 to 11 times in the second after, absorbed or not. With the joints' dampers at
  twice and four times their strength (`UPPER_DAMP=6`, `12`) it is the same: the cause is not found.
- *Loose things* (the tube test, physics only: 5 kg at 4 m/s; the new line `TUBESPEED`): a tube stops dead on a
  trunk, glances off an upper arm at 0.7 m/s, comes back off a shield at 0.8 m/s. Left as it is.
- *Headless.* The fight of six ran its 40 s with all of it on. Its own crossings cannot be set against the
  morning's (4.0 s of 80 before; 5.2 s and 2.2 s in two fights with it on, in one of which a shield bearer lay
  down most of the time): every change makes another fight, and nothing here was meant to reduce them; the plan is.
  The panel check passes in all five places.

Not seen in Unity: any of it. In particular the get-up with the board under him as it starts, and whether a
trunk that settles without swinging back reads as weight or as stiffness.

**Shield and sword solid against their bearer on his feet; what he holds moves smoothly (18 November, evening).**
Renier, 16:45 and 16:53: "The way the shield shakes about all the time is one of the reasons I want this fixed.
Why cant we just have the shield and the sword have colliders that treats them as physics objects. that will
have them stay out of each other and out of their own body ... do the same for the sword, give it collision to
stay out of its own wearers body. But for both sword and shield, it should not be shaky. the way the sword and
shield connect with its own body should be smoothed and dampened so that it doesn't cause chaotic motion. The
absorbsion or dampening does not feel very noticeable. Perhaps because I'm specifically looking for it on the
sword an shield." This is step 3 of the plan "A shield that stays outside its bearer" brought forward at his
word, with a part of its step 1.

- *Where the shake came from* (looked for first; `HeldShake`, in every headless report as "what he holds
  trembles", and in the lab as a line while a playtest is recorded). A tremble is what is left of a place when
  its own smooth motion is taken off (the place less the average of five steps about it), read on the board's
  middle and the blade's tip as seen, as posed, and the seen against the posed. Not contacts: with the
  physical bodies touching nothing at all (`UPPER_GHOST=1`) it was the same. Not weak dampers (twice and four
  times, earlier today). It was the pose: a standing or walking fighter's board was moved 2 to 12 cm in single
  steps as it was raised to a blow, knocked by one, taken up by the walk from standing still, or carried by a
  trunk that was rocked or turned at once (`FIGHT_SHAKELOG=1` writes each), and the physical arm then chased it.
- *What he holds moves smoothly* (`ClipFighter.SettleHeld`, `HeldSettles`; Combat Feel "What He Holds Moves
  Smoothly"; the lab's "What he holds: moves smoothly / as posed (old)"; `FIGHT_SETTLE=0`). The last thing done
  to the pose, whatever came before: the shield hand of a sword and shield, and his sword hand while it is
  neither in a blow nor held to one, follow where the pose has them, place and turn, speeding up and settling in
  (a critically damped spring, against where he stands and the way he faces, so that what his trunk does at once
  they follow too). At 26 a second ("Held settles at": about a ninth of a second to arrive), and at 60 while the
  shield has somewhere to be: raised to a blow or driven in a bash ("Held settles at, pressed"). A sword hand is
  let go to its blow over a twentieth of a second and taken up again over an eighth. A lone sword's arm is its
  parry's and is left alone, as is anything two-handed.
- *A shield and a sword are stopped by their bearer's own body, on his feet*
  (`UpperBodyArticulation.ShieldStoppedOnFeet`, `BladeStoppedOnFeet`, `StopHeldThatAreOut`; Combat Feel "A Shield
  Is Stopped By Its Bearer's Body", "A Sword Is Stopped By Its Bearer's Body"; the lab's "His shield:" and "His
  sword: solid against his body / passes through (old)"; `FIGHT_OWNSOLID="0,0"`). The physics engine stops the
  board and the blade at his trunk, head, thighs and shins, as it did only while he fell and lay. Each held thing
  from when the whole of it is next clear of him: one that is in him is not thrown out. Not while he gets up
  (`HeldMayStop`): the get-up's pose has its own care, and stopped there a sword hand was thrown at 16 m/s.
- *The pose keeps the board out of his thighs too* (`ShieldKeptOutOfLegsToo`, with the shield's setting): the
  part it was in most often, which the pose's guard never knew. Solid, a board the pose asks for in a thigh is
  pressed there each stride.
- *Met softly* (`UpperBodyArticulation.FeelHeld`, `HeldMetSoftly`; Combat Feel "A Held Thing Meets His Body
  Softly"; the lab's "Against his body: met softly / driven on (old)"; `FIGHT_HELDSOFT=0`). While what an arm
  holds is within 2 cm of a part of his body that stops it, that arm's joints give way at once (2 degrees behind
  their pose, where they otherwise give way at 8) and press with a third of their strength: it rests there and
  comes away as the pose does. There is no spring in these motors to throw it back.
- *The jolt on the shield arm* (`ClipFighter.HeldJolt`, with "Impacts: absorbed"): the blow's force put at once
  on the physical shield arm, or on a parrying sword's hand, is less by how much impacts are absorbed (0.58 of
  it at 0.6). It threw the seen hand some 11 cm off its pose within three steps and let it back as quickly, on
  the very thing being watched; what the shield gives to a blow is its arm's own give, which now goes out and
  settles through the smoothing above.
- *Measured* (headless; old is this step's four settings off: `FIGHT_OWNSOLID=0,0 FIGHT_SETTLE=0
  FIGHT_HELDSOFT=0`; one run each, and a change makes another fight, so directions and not rates). The fight of
  six, 40 s: a shield's board trembling more than 5 mm, as seen: 16% of the time, now 6% (walking 9 to 4,
  standing 10 to 2, just struck 23 to 12, striking 22 to 12); its usual size 14 mm, now 8. The shield in its
  bearer's own body: 2.7 s of 80 fighter-seconds, now none of 51. A blade more than 2 cm in its own bearer's
  body (all six fighters): 135 times and 9.3 s, now 25 times and 1.2 s, the deepest 15.5 cm, now 8.6 (a
  two-hander's club into its own other hand, which stops nothing). The dance, two sword and shields, 30 s: the
  board trembling more than 5 mm 34% of the time, now 25%; its usual size 15.5 mm, now 8.2 (walking 11 to 5,
  striking 20 to 8, just struck 34 to 16, standing 11 to 7.5); the shield in his own body 1.8 s of 80, now 0.3
  s (its arm is held exactly to its pose while it meets a blow, and there nothing stops it); a shield hand
  knocked off its pose by a blade on the board 19, 4 and 9 cm (3 blows), now 4 to 11 cm (6 blows): too few to
  read. Blocks: 6 blows met
  the shield and none landed, where 3 met it and 1 landed. A medium and a hard bash rock both trunks as before
  (27 and 37 to 38 degrees); the hands change direction 3 to 7 times in the second after, where it was 5 to 11.
- *Not better.* A blade's tip trembles as much as it did (in the six, over 5 mm 61% of the time against 64%;
  standing, about a third of the time in both): most of it is the blows themselves, and standing it is the way
  he faces being turned from step to step, which carries a tip a metre out. His own blade and sword arm
  through his own board are not stopped by anything yet: no collision was asked between them, and the plan's
  step 1 and 2 are not built (the dance: 1.2 s old, 0.4 s new; too few to read).
- *Seen in the tube test.* The shield bearer struck by the tube now ends sitting, where he ended on his back:
  the board, solid, takes the tube against his chest. He then rises by the made keys for one who sits, in which
  a shin moved at 6.8 m/s early: that route is not this step's, and is noted.
- *In the lab,* with the bash's settings: "What he holds:", "His shield:", "His sword:", "Against his body:",
  each old against new, and two lines read while a playtest is recorded: "His own crossings" and "What he holds
  trembles". The panel check passes in all five places.

Not seen in Unity: any of it. In particular whether a shield that follows its pose a ninth of a second behind
reads as weight or as lateness, and how a blade resting against his own leg looks.

*Renier's playtest of 16:41 (the build before this step), read off its log's new `cross` lines:* two sword and
shields dancing for some 112 s: 62 crossings, 7.0 s in all. The shield in his own body 36 times and 3.9 s (the
shield-side thigh in 20 of them, the hips in 7, the chest in 5; 14 of them just after he was struck), his blade
through his own shield 16 times and 1.9 s, his sword arm through it 10 times and 1.2 s. Its two get-ups had the
shield 5 cm over the ground and 4 cm in it for 0.17 s. The thigh is what this step's guard and collision are for.

**The shield gives way to the sword (18 November, night).** Renier, 19:44, to the plan's first open question
(when sword and shield want the same space, which gives way): "yes this looks good. Proceed with the sword and
shied where the shield gives way to the sword".

- *Why his sword went through his own shield.* His two arms were each posed alone: the sword arm by the
  attack, the shield arm by its cover, and the board is held before his chest through his own blows ("The
  Shield Is Held In Cover"), its rim a hand's breadth past his breastbone on the sword's side. Nothing looked at
  the pair.
- *The rule* (`ClipFighter.GiveWayToSword`, `ShieldGivesWayToSword`; Combat Feel "The Shield Gives Way To The
  Sword"; the lab's "His shield and his sword: the shield gives way / pass through each other (old)";
  `FIGHT_GIVEWAY=0`). In the same last place the pose is finished in, before the board is kept out of his body:
  his sword's two lines (forearm to knuckles; blade from a hand's breadth up it to its tip) are taken where the
  pose has them and where they are seen (the physical sword: a blade stopped on the other's shield was up to 57
  cm at its tip from where the pose had it, and it is the seen blade that goes through the seen board), each with
  where it is going by how it moved this step, over the next 0.3 s ("Sword seen ahead", `SwordSeenAhead`).
  Wherever one of them passes through the board's face in that time, the board steps aside in its own plane: of
  sixteen ways across its face, the least that leaves every such place 6 cm clear of its rim ("Sword's lane",
  `SwordLane`; the forearm its own thickness more), ways across his own chest costing half as much again and
  ways up before his face a quarter more, and the way it is already giving kept unless another is clearly less.
  The path clear, it comes back over a quarter of a second (`LaneCloses`). What it is asked is followed smoothly
  as all his shield hand is, at the quicker rate while it gives way. The body's own guard and the collision
  with his body come after it and win.
- *Met softly, mended* (`UpperBodyArticulation.FeelHeld`). This evening's "met softly" also had the arm give way
  at once while what it held was against his body. That gives way both ways: an arm whose pose then drew its
  board away from his body stayed where it was, and with the board now stepping about for the sword, in a fight
  of six a board sat in its bearer's belly for 3 s with the pose clear of it. Taken out: against his body the arm
  presses with a third of its strength, and gives way as any joint does.
- *Measured* (headless). The aim test, a sword and shield striking at one 19 times, the same build with the rule
  off and on: his blade through his own shield 1.30 s, now 0.25 s; his sword arm through it 1.00 s, now 0.10 s;
  13 crossings of a twentieth of a second or more, now 5 (of 110 fighter-seconds). The pose itself had 1.85 s of
  those 2.30 s, now 0.17 s of 0.35 s: what is left is mostly the seen body off its pose. The board was asked
  aside on about a sixth of the striker's steps, 52 cm at the furthest. 16 of his blows met the other's shield
  and 14: his blows are as they were. The fight of six, 40 s, the two shield bearers on their feet throughout:
  his blade through his own shield 0.13 s, his sword arm 0.07 s, his shield in his own body 0.07 s (his sword
  upper arm): 0.3 s of 80 fighter-seconds and one crossing of a twentieth of a second or more, where this
  morning's game had 4.0 s and 29. A second fight of six (another seed), the same build with the rule off and
  on: his blade and sword arm through his own shield 2.4 s (22 crossings in all), now 0.2 s (6). The dance, 30 s: on his feet his blade through his shield 0.03 s and his sword
  arm never, where this afternoon's had 1.2 s.
- *What it costs.* The board moves in his own blows now: in the six it is asked aside up to 59 cm, and the
  tremble measure, which cannot tell a step aside from a shake, reads the board over 5 mm 26% of the time while
  he strikes (12% this evening) and 16% in all (6%); walking 6% (4), standing 7% (2). And a
  shield that has stepped aside for his cut is not before his chest for that moment.
- *Not covered.* His sword upper arm (the board in it 0.07 s of the six, 0.27 s of the dance; tried as a fifth
  line and taken out again: nothing could be read to be better, the aim test's crossings 0.60 s against 0.35
  s). A body that is falling, down or getting up (in the dance his blade was through his shield 0.8 s of 1.8 s
  down): the rule is for a fighter on his feet; the plan's step 4.
- *In the lab,* with the bash's settings: "His shield and his sword: the shield gives way / pass through each
  other (old)", and the levers "Sword's lane" and "Sword seen ahead". The panel check passes in all five places.

Not seen in Unity: any of it. In particular how far the board swings in a rising cut and a lunging chop, and
whether it reads as a shield opened for the blow or as a shield flung aside.

*Renier's playtest of 19:37 (the evening's build: solid against his body, smooth hands; before the shield gave
way), read off its log's `cross` lines:* two sword and shields for some 190 s. The shield in his own body 7
times and 1.2 s, 5 of them getting up and 2 on his feet (at 16:41: 36 times and 3.9 s, nearly all on his feet).
His blade through his own shield 18 times and 2.1 s, his sword arm through it 15 times and 2.1 s, 13 of those
in his own blows: what the shield giving way is for.

### 18 November, late: a briefing for new chats, and the test scripts saved

Nothing in the game changed. `CLAUDE.md` (at the top of the repository) is now the short briefing a new chat
reads first: how Renier wants the work done, the design rules in force, the layout of the code, how to test
without Unity, the plan documents with their state, where things stand and the roadmap. It is to be updated at
the end of every step. The headless test scripts, which had lived only in one chat's workspace, are saved in
`Tools/Headless/` with a README of their own (they write to `$S`, `/tmp/pb` unless set). The get-up sword shake
Renier flagged is reproduced and measured but not fixed: what is known is in `CLAUDE.md` under "In hand". The
frames of the 21:47 playtest log were looked at and removed.

The plan documents (30, from the first design spec to the shield and impacts plans of 18 November) are now also in
the repository as copies, in `Docs/Plans/`, with an index. Before this they were only Claude documents linked from
here.

### 19 November, overnight: off balance. The momentum ledger, and stumbles on real stumble steps

Renier, 18 November, night: stumbles first. A fighter who is bashed, kicked or struck hard is pushed off balance for
real: he stumbles on real stumble steps and keeps his legs under him if they can catch him, is turned round, or goes
down if they cannot. Both bodies of an impact absorb it and either can be thrown off balance. No invented randomness.
Two steps, each behind a Combat Feel switch, built and measured headless. **Nothing of it has been seen in Unity.**

**Step 1, the momentum ledger** (`ImpactLedger.cs`; the impacts plan's step 3; Combat Feel "A Bash's Speeds Are
Worked Out"). The speeds the two leave a shield bash with are worked out, not written in. In: both weights and both
speeds along the line between them; how much of the basher is behind his board is the bash's own "Weight". While
they are pressed together they go as one, and one who has set himself (a counter-bash) passes some of it to the
ground through his feet: at most his grip times his weight times a quarter of a second, by how many of his feet
are down (157 N·s for a Medium on both). Then they spring apart by "Impact Bounce" (0.2). What the other is handed
is taken from the whole of the basher, equal and opposite. This replaces the bash's own speed rule (board speed
times weight) and the countered hard bash's two fixed speeds (1.2 and 1 m/s). **A counter is no longer a draw**:
"Counter Bash Holds" does nothing while the ledger is on; he keeps his feet if his legs catch him.

**Step 2, stumbles** (`StumbleSteps.cs`; the Fight Rhythm plan's step 9c; Combat Feel "Off Balance He Stumbles On
Real Steps"). A body is a weight on legs. Thrown, it tips away from the feet that carry it, faster the further it is
past them, and only a foot put down ahead of it stops it. The model: his weight's place and speed over the ground;
the point where a foot would have to be to stop him (his place plus his speed times about a third of a second);
his two feet, each planted or in the air.

- *Off balance or not.* If what was done to him leaves that point inside what his feet hold, his stance takes it
  as before (a shove stepped in the stance). Outside it, he stumbles. Judged on what the impact did to him, not on
  his own walking.
- *The steps.* After a moment (0.1 s) the foot behind lifts and goes for that point, re-aimed as he goes, as far as
  a leg reaches from under his hips ("Stumble Step Reach", half a leg) and as fast as a foot goes ("Stumble Foot
  Speed", 5 m/s; a man who was already moving has that much more). A foot his body has left behind comes off the
  ground by itself. A foot that lands ahead of him takes speed out of him by how far ahead it is (the leg is a
  strut; what of his speed runs down it is lost), his knees give, and his upper body is rocked on.
- *How it ends.* Caught: both feet down, slow, his weight over them; his stand has his feet again and steps them
  back into its stance. Down: with no foot under him his hips fall, a foot far behind lets them down, and past
  "Stumble Down At" (0.42 of a leg) the fall takes him as he goes (a fall his legs go down on, as the controlled
  falls have it). No speed is written in for either.
- *Turned round.* An impact off his middle turns him: its lever about his hips, by "Stumble Turned By" (a quarter
  of it; his trunk takes the rest). The ground's grip on his soles stops it, quicker on two feet than on one.
- *The body.* `Locomotor.Carried` (he stands as his guard stands, carried where the stumble has him),
  `LocoBody.Bear` (his ankles are where the stumble puts them; his hips let down up to 0.4 of his legs' reach),
  his upper body tipped with his weight ("Stumble Trunk Tips").
- *One door.* `ClipFighter.ThrowBody` takes what throws him (a change of speed, the impulse and where it came in).
  The bash goes through it for both men, and so does a sword blow that throws a body back (it was "thrown back
  stumbling" on the walk, or down at a fixed 4.4 m/s). Kicks are to use it.

**Measured headless** (two Mediums, sword and shield, one standing; single runs, which repeat exactly on one build).
Old: `FIGHT_LEDGER=0 FIGHT_STUMBLES=0`.

| Bash | Old | New |
|---|---|---|
| Small, from 1 m | shoved at 1.1 m/s; went 0.46 m | handed 56 N·s; leaves at 0.7 m/s; one stumble step, 0.17 m. The basher is checked to 0.1 m/s |
| Medium, from 1.8 m | thrown back at 3.5 m/s on the walk; went 1.5 m | handed 179 N·s; leaves at 2.2 m/s; two stumble steps over 1.1 m in 0.8 s, on his feet. The basher leaves at 0.4 m/s: two small steps |
| Hard, countered | a draw: held, he left at 1.2 m/s and the basher at 1 m/s backward; 294 N·s in neither | handed 305 N·s; his feet (one was down) passed 78 N·s to the ground; carried back at 1.8 m/s; one stumble step, 0.42 m, on his feet. The basher is all but stopped (0.3 m/s). In neither: 0 |
| Hard, not countered | knocked down, thrown at 6 m/s | handed 272 N·s; thrown at 3.4 m/s; no foot gets under him; down after 1.9 m, on a knee and a hand first. The basher goes on at 0.6 m/s: one step |

The ledger's own check (what came in, less what both leave with and what the ground took): 3, 5, 0 and 4 N·s.

The model by itself (`Tools/Headless/stum.sh`): a Medium standing in a guard's stance keeps his feet thrown straight
back at up to 2.6 m/s (one step to 1.4 m/s, two at 1.7 to 2.0, three at 2.3 to 2.6) and goes down from 2.9. With
his feet in line with the push he holds more. At a foot speed of 5.5 the uncountered hard bash is a three metre
stumble on three steps and he keeps his feet: **which of the two is right is his call.**

The fight of six, 40 s, two seeds: 3 and 7 stumbles began; 2 and 4 ended on his feet (one to four steps, 0.2 to
2.7 m; turned up to 89 degrees), 1 and 2 down, all three from a Large's blow that threw a body at 10 m/s or more or a
three-step stumble that ran out. No crossings of his own sword and shield while stumbling (2.9 s watched in each).
No exceptions. A film strip of the medium bash's stumble (`film.sh`) shows two steps back and the knees bent under
him at the end.

**In the lab** (duel, Actions, "Stumbles and falls", under the bashes): "Stumble him: Small / Medium / Hard" stands
the other the right distance before him and bashes him. "Off balance: real stumble steps / the walk played fast
(old)". "A bash's speeds: worked out / written in (old)". "Hard bash met: countered / not countered" is where it
was. The last stumble of each is read out. The settings fold out under "Stumbles and falls: settings". The panel
check passes.

**Not verified, not done.**
- The look. Headless shows the feet, the distances and the outcome, not whether it reads as a stumble.
- The arms do nothing of their own: the shield stays in its cover and the sword arm in its guard. No arms thrown
  out for balance. (A sword arm made loose for 0.7 s was tried and taken out: see the budget, below.)
- A foot may cross the other leg in a stumble to the side. Not looked at.
- "Bodies that absorb" (knees and hips giving as the boards press, the arm folding, feet sliding) is not built.
  The stumble's own knees and hips are all there is of it.
- The small bash now moves the other 0.17 m where it moved him 0.46 m: the ledger's speed is lower than the old
  rule's. If it no longer clears his space, "Impact Bounce" up is the lever.
- A sword blow's throw is still the old rule's number (its weight times "Stumble Boost"), now uncapped: a Large's
  blow reads 10 m/s.
- More falls in a fight than before is likely (the old rule never felled a man under 4.4 m/s). Counts above.

**Headless switches.** `FIGHT_LEDGER=0`, `FIGHT_STUMBLES=0`, `FIGHT_BOUNCE`, `FIGHT_FEETTAKE`, `STUMBLE_FOOT`,
`STUMBLE_REACH`, `STUMBLE_TAKES`, `STUMBLE_BRAKE`, `STUMBLE_REACTS`, `STUMBLE_DOWN`, `STUMBLE_KNEES`,
`STUMBLE_TURNED`, `STUMBLE_TIPS`. Lines: `STUMBLE` (each stumble as it begins and ends). Scripts: `four.sh`,
`stum.sh`. His playtest logs carry `stumble` notes.

### 19 November, overnight: the sword that shook as a fighter got up: the cause, and a mend

Flagged by Renier on the 18th (it happens as the hand pushes him up). Found and mended behind a switch, headless
only. **Not seen in Unity.**

**The cause** (`GetUp.HoldIn` and `GetUp.Arm`). Getting up, the sword hand is turned toward where the blade is
asked to lie, but only as far as a wrist turns (55 degrees) from where the forearm points. About a second into a
get-up from his back his arm comes round, and for a tenth of a second the forearm points the blade down: up to
49 cm into the ground as posed. The rule that keeps a held thing out of the ground then lifted his wrist by that
much in one step. Lifting the wrist turns the forearm, the forearm turns the blade, and the blade swung up over
his head; the next step it was back. In the trace: the blade turned 45, 103, 105 and 140 degrees in single steps
and the hand moved 24 and 37 cm in single steps, between 1.07 and 1.18 s.

Ruled out on the way: the ground clamp at the end of the pose having no memory of its wrist turn and arm raise
(given one, nothing changed; taken out again).

**The mend** (Combat Feel "A Rising Fighter's Blade Is Turned Clear Of The Ground", on). Where the forearm would
point the blade into the ground, the wrist is turned on toward where the blade was asked to lie, only as far as
lifts it clear. That is past what a wrist turns, for those moments, in the pose; the arm is no longer thrown up.

**Measured headless** (a Medium with sword and shield felled by the tube, old against new on one build,
`Tools/Headless/gup.sh`; "the usual size" of the tremble, as the report has it):

| Get-up | | Old | New |
|---|---|---|---|
| From his back | the blade's largest turn in one step | 140 degrees | 23 degrees |
| | the hand's largest move in one step | 37 cm | 14 cm |
| | the tip's tremble, as seen / as posed | 140 / 136 mm | 63 / 56 mm |
| From his front | the tip's tremble, as seen / as posed | 83 / 84 mm | 31 / 26 mm |

**What is left.**
- The tip still reads over 5 mm for nine tenths of a get-up: it is a long blade on an arm that moves fast. Between
  1.17 and 1.38 s his sword hand travels 8 to 14 cm a step (5 to 8 m/s) as the arm comes round from behind him to
  before him. That is the get-up's own motion at half the recording's time, not a flip. Whether it still looks
  like a shake is for his eye.
- The blade turns 20 to 23 degrees in single steps four times in a get-up from his back.
- **The shield's tremble in a get-up is as it was** (the board as posed 5.3 mm, over 5 mm 27% of the time; standing
  it is 0.9 mm). Its cause was not found. Not the clamp's memory.
- `FIGHT_GETUPTRACE=1` (the `GETUPHELD` lines) is kept until he has seen it.

**In the lab:** target test, with the get-up rows: "Getting up, his blade: turned clear at the wrist / the arm
thrown up (old)". Fell him from his back with "Falls on demand" and watch about a second into the get-up.
Headless: `FIGHT_GETUPBLADE=0` is the old way.

### 19 November, overnight: shield and sword off his feet (measured only), the budget, and the walk lab's panel

**Shield and sword while falling, lying and getting up: measured, nothing built.** The crossing counter on a
shield bearer felled by the tube (`gup.sh`), as seen:

| | Falling or down | Getting up |
|---|---|---|
| Felled to his back | none in 1.2 s | his board in his hips 0.07 s of 1.8 s |
| Felled to his front | 0.02 s of 1.1 s | his board in his own body 0.50 s of 2.1 s: in his hips for 0.35 s as it begins (he lies on it), again for 0.07 s, and past his head for 0.08 s at 1.1 s |

In two fights of six the same: getting up, his board in his own body 11% and 26% of the time, his sword arm
through his board 8% and 12%. The pose itself has nearly all of it (0.50 of 0.52 s): it is not the physics. The
shield plan's step 4 has no design yet for the question underneath it: **where does a hand with a shield on its
arm post, and where does the board lie while he pushes up from his front?** Flat under the forearm is what puts it
under his hips. That is a decision of Renier's (out to his side? on its edge? carried on his back as he rises?),
so nothing was built.

**The budget: the shield work is locked into the check runs** (the shield plan's step 5). Every headless fight now
ends its report with two lines, and `Tools/Headless/checks.sh` gathers them into `$S/checks.verdict` (it now also
runs a dance of two and the four bashes):

- `CROSSINGS OK` or `CROSSINGS OVER BUDGET`: for fighters on their feet, all crossings of his own sword, shield
  and body together under 0.5% of the time watched, and no single one longer than 0.1 s (the plan's proposal).
  Falling, down and getting up are counted on the same line, apart, and are not held to it yet.
- `TREMBLE OK` or `TREMBLE OVER BUDGET`: standing or moving, his board as seen may show a tremble (over 5 mm) no
  more than 8% of that time. The 8% is a proposal of this night's, from the 6 to 7% the evening of the 18th
  measured. The blade's tip is told beside it with no budget: why it trembles is not known.

**It reads red today.** A fight of six, 25 s: crossings on his feet 0.90% (0.45 s of 50 s), the longest 0.12 s
(a blade through his own shield while moving); the board shows a tremble 11.5% of the time standing or moving.
Whether the night's stumbles made it worse is told below this entry, with the old way on the same seed.

**The walk lab's own panel** (`LocomotionLab.cs`) has what the battle's panel has: each section has a colour of
its own (its heading bright, its buttons a darker, greyer tone of it) and there is a search box under the panel's
size (type part of a name: every section opens and only what has the words in its name is shown, a row with its
plus and minus buttons kept whole; Esc or the cross clears it; the keys that drive the walker are not read while
the box has the keyboard). **Built blind:** it compiles, and the headless panel check does not draw this panel.

**The budget against the night's own work** (the fight of six, 25 s, seed 5 unless said; single runs, and a fight
that goes another way is another fight):

| Run | Crossings on his feet | The longest | Board shows a tremble |
|---|---|---|---|
| Old way (ledger off, stumbles off) | 0.20% | under 0.05 s | 6.3% |
| Ledger off, stumbles on | 0.20% | under 0.05 s | 6.5% |
| Ledger on, stumbles off | 0.30% | 0.07 s | 5.4% |
| **Both on (as set)** | **0.90%, and 1.50% in a second build** | 0.12 s, 0.28 s | 11.5%, 14.1% |
| Both on, seed 7 | 0.56% | under 0.05 s | 5.5% |
| Both on, no bash force on the body (`FIGHT_BASHFORCE=0`) | 0.63% | 0.13 s | 11.6% |
| Both on, a dance of two, 30 s | 0.72% | 0.13 s, his blade through his shield **while stumbling** | 10.0% |
| Old way, the same dance | 0.39% | 0.13 s, his shield in his own body while striking | 13.2% |

**So the lock's first catch is this night's stumbles.** In the six, with both switches on the crossings were over
the budget in four runs of four; with either off, within it (one run each). **The dance is over the budget the old
way too**, on the longest crossing (0.13 s in a blow of his own) and on the board's tremble (13.2%): the budget as
proposed is not met there by the game as it was on the 18th. What is known: in the dance one crossing was in the
stumble itself (the upper body tips and carries the sword arm with it; the shield's cover does not tip: "Trunk
tips" at 0 would be the test). In the six they were while moving, striking and just struck, with the seen board 59
to 94 degrees off its pose: a board knocked about, not a pose. It is not the force a bash puts back on the
basher's shield arm alone (off, still over). A sword arm made loose as a stumble began was tried for the arms and
taken out again (no better: 0.90% with it, 1.50% without). **Not pinned down. Not mended.**

**Why the blade's tip trembles on his feet: narrowed, not found.** (`FIGHT_SHAKELOG=1` now also writes `TIPLOG`
lines: each time the posed tip jumps 2 cm or more from its own smooth path while he stands or moves, with what he
was at.)

- Standing with nothing happening it does not tremble: two sword and shields 1.5 m apart for 68 s, the tip 0.4 mm
  as seen, never over 5 mm; the same with his sword not solid against his own body. So it is not the arm's motors
  and not his own body in the blade's way.
- In a fight it is mostly the pose. Standing in the six (25 s of it): the tip as posed 20 mm (over 5 mm a quarter
  of the time), as seen 21 mm (38%), the seen against the posed 14 mm.
- **In the six nearly all of it is the sword-alone fighters' parry**: of 320 such jumps in 25 s, 310 were a
  fighter without a shield, 167 of them in his parry's guard with no blow coming ("club guard", "medium guard")
  and about 120 in a parry going to meet a blow, which is meant to be fast. The guard between blows moving the tip
  2 cm a step is the thing to look at there.
- **For a sword and shield it comes in bursts**: in a dance of two, 44 jumps in 28 s, in runs of five or six steps
  (a tenth of a second) every 3 to 5 s, always with his legs in their walk's cycle, no blow coming, no gesture, not
  struck. What starts a burst is not known: the suspects are the hand-over back to the guard as a gesture or a fake
  ends, and the change between the stand and the walk. The next step is one more word on the `TIPLOG` line (what
  ended last, and how long ago).
- Nothing was changed for it.

### 19 November, morning: after Renier's first look at the stumbles

Renier, 19 November, 07:10, having played it: "this looks pretty good so far"; it reads as a stumble, but he wants
the points below addressed before he is sure. His notes, and what was done. **Headless only again.**

**1. "Bashing now seems to knock an enemy even before the shield makes contact."** Why: a bash "landed" when the
board's middle came within 46 cm of the other's middle, which is some 30 cm of air before the board. It always
did. Before the ledger nobody saw it, because the basher went on at full speed and his board caught the other up.
With the ledger the basher is checked the moment it "lands", so his board stopped short and the other flew off
untouched. Now (Combat Feel "A Bash Lands When The Board Touches Him"): it lands when the board's middle is within
22 cm of his middle (against his body), or within 12 cm of his own board along the bash if he holds that between
them. The leap is planned to carry that much further.

**2. "The upper body recoils … the lower body stands stiff. Then only after … does the lower body respond."** Two
causes. The hit stop: as a bash landed both men were held still for a moment, the legs with them, while the
physical upper body went on recoiling. With stumbles on there is no hit stop on a bash any more: the bodies really
move now. And the stumble waited a tenth of a second before the first foot lifted, with his knees straight until
then. Now his knees give at once as it comes in (Combat Feel "Stumble Knees Give At Once", 1) and the first foot
lifts after 0.05 s ("Stumble First Foot Lifts In").

**3. Harder bashes, and an extra hard one.** The ledger alone is two bodies colliding; a bash is also a push. Each
bash now has "pushes through" (Combat Feel "Small / Medium / Hard / Extra Hard Bash Pushes Through", m/s): the
speed his arm and legs add to his board while the two are pressed together. He drives on off the ground, so it is
not taken out of his own going, and the meter's books still balance (it shows what his legs put in). Small 2.4,
medium 0.3, hard 2, extra hard 6. The small bash's arm goes further too ("Small Bash Arm Goes" 0.9, was 0.6: the
board is driven 47 cm, was 37). **The extra hard bash** is the hard bash's leap driven through with everything he
has. For now it is only thrown from the lab's "Stumble him: Extra hard": no fighter throws it by himself yet.

| Bash at a standing Medium | The night before | Now |
|---|---|---|
| Small | left at 0.7 m/s: one step, 0.17 m | leaves at 1.9 m/s: two stumble steps over 0.73 m, on his feet |
| Medium | left at 2.2 m/s: two steps, 1.1 m | leaves at 2.1 m/s: two stumble steps over 0.84 m, on his feet |
| Hard, countered | carried back at 1.8 m/s: one step, 0.42 m | carried back at 3.1 m/s: three stumble steps over 2.2 m, turned 25 degrees, on his feet |
| Hard, not countered | thrown at 3.4 m/s: down after 1.9 m | thrown at 4.3 m/s: down, 3.0 m back after 1.2 s |
| Extra hard, not countered | (none) | thrown at 6.8 m/s: off his feet, 4.5 m back after 1.2 s |

**4. "A few cases where the shields get stuck to each other. The logs will show this."** No logs of the 19th were
in the repository when this was done, so the cases themselves were not read. One cause is known from the night's
own numbers: the ledger left the basher going forward at 0.3 to 1.6 m/s after his bash, into a man who had only
been carried back a step, so the two could end board against board. Now (Combat Feel "After A Bash He Holds His
Ground"): his own legs take what is left of his going forward. After each of the bashes above the two are 1.5 to
5.6 m apart 1.2 s later. **Whether that was his case is not known until his logs are read.**

**5. The sword as he gets up: "pinned to the ground in one position … not allowed to rotate more than only a few
degrees … should not be able to change rotation fast."** Built as said (Combat Feel "A Rising Fighter's Sword Hand
Stays Where It Lies"; `GetUp.PinSwordHand`, `GetUp.SettleSwordHand`). His sword hand stays where it lay when the
get-up began, brought down to the ground once if it was not on it, turned as it lay. It is let go when his arm
can no longer reach that spot or the get-up has the hand 45 cm off the ground, and goes to where the get-up has it
over 0.4 s. Pinned it may turn 5 degrees ("Rising Sword Hand May Turn"). Through the whole get-up it turns no
faster than 180 degrees a second ("Rising Sword Hand Turns At Most"; twice that once let go) and travels no
faster than 3 m/s. This is the last word on that hand each step: after the keys, the blend in from how he lay, the
blend out to his stand and the rule that keeps the blade out of the ground. The night's mend (the wrist turning
the blade clear) had the hand still going wherever each key put it, and the first try at this pin was undone by the
blend from how he lay and by the ground rule running after it.

| Get-up, the sword as finally posed | The night's mend | Pinned |
|---|---|---|
| From his back: the blade's largest turn in one step | 23 degrees | 10 degrees (his whole body turning is in that) |
| From his back: the tip's tremble, as seen / as posed | 63 / 56 mm | 22 / 18 mm |
| From his front: the tip's tremble, as seen / as posed | 31 / 26 mm | 21 / 17 mm |
| Pinned for | (not pinned) | 0.75 s from his back, 0.37 s from his front |

Not looked at: whether a blade held from turning now lies in the ground for a moment where it used to be flicked
clear (the crossing counter does not count the ground).

**In the lab.** Duel, Actions, "Stumbles and falls": "Stumble him: Small / Medium / Hard / Extra hard"; "A bash
lands: when the board touches / from 46 cm off (old)"; "After a bash: he holds his ground / he goes on (old)"; the
four "pushes through" levers, "Knees give at once" and "First foot lifts in" under the settings. Target test:
"Getting up, his sword hand: stays where it lies / goes with the keys (old)" with its two levers. Headless:
`FIGHT_BASHTOUCH=0`, `FIGHT_BASHHOLDS=0`, `FIGHT_PUSHES="2.4,0.3,2,6"`, `FIGHT_BASHKIND=extra`, `STUMBLE_ATONCE`,
`FIGHT_GETUPPIN=0`, `FIGHT_GETUPTURNS`; `GETUPFINAL` lines under `FIGHT_GETUPTRACE=1`.

**Fields renamed so their new defaults take in his scene:** "Small Bash Reach" is now "Small Bash Arm Goes"
(0.9); "Stumble Reacts In" is now "Stumble First Foot Lifts In" (0.05). Whatever he had set on the two old ones is
not carried over.

**The budget after the morning's changes** (the fight of six, 25 s; single runs). Still red: crossings on his feet
0.64% (seed 5) and 1.19% (seed 7), against 0.5%. Two things learnt:

- It is not the upper body tipping in a stumble: with "Stumble Trunk Tips" at 0 the same fight read 1.37%.
- On seed 7 every crossing on his feet was **his blade through his own shield while bashing** (four and more of
  0.05 to 0.07 s). The small bash's arm now goes further (drawn in to his chest, then driven out), and his sword in
  its guard is in the board's way. The shield gives way to the sword everywhere else; in a bash it is the sword
  that has to get out of the way, and nothing does that yet. On seed 5 they were in two made attacks thrown on a
  turn (a forehand on a left turn, a short high spinning backhand). **Not mended.**

### 19 November, later in the morning: his second look. The shield off his feet, two bodies in one place, and a plan for the first moment of a get-up

Renier, 19 November, 08:26, with three pictures and his logs pushed (06:44, 06:53 and 08:07): the shield goes
through his body as he gets up; a bash bends the shield arm with the board intersecting ("the shield should still
be enabled as a physics object here that does not intersect with the body… it will also keep the legs from falling
into the shield if the shield stays a physics object during the fall"); "as the fallen guy is leaping from his
fallen position into the start of the get up animation all kinds of weird things happen. I want you to make some
context aware transition there… Judge if this is a hard enough task to justify doing a planning document first";
and the getting stuck again at the end of the new log, "where the feet also gets stuck". **Everything below is
headless only.**

**What his log of 08:07 says** (280 s in the target test, 28 falls, 27 get-ups):

- Off his feet his own shield, sword and sword arm crossed him for 3.25 s while falling and lying (22 stretches:
  the board in his shins eight times, his blade through his board eight times) and for 7.08 s while getting up.
- Of the 7.08 s, **6.28 s (89%) is in the first 0.75 s of a get-up**: the blend from how he lay. In those moments
  the fastest part moved at 6.7 m/s on average (24 m/s at the worst), and it was the shield hand or forearm in 16
  of the 27.
- Two fighters were in the same place twice, both times with a body off its feet: 64.4 to 66.3 s (roots 3 cm
  apart at the nearest) and 274.0 to 275.9 s, the end he pointed at. In the two earlier logs I found no stretch
  where two standing fighters were held together; if the shields stuck there, I have not found it.
- **The lab's own "Stumble him" button did some of the damage.** Pressed while the target was down or getting up,
  it put him on the bash's spot at once: his body was thrown 71 to 78 cm back onto its pose ("out of hand… put
  back on its pose at once", at 259.4, 270.0 and 279.4 s), once he was moved 2.7 m in the middle of a fall
  (260.9 s), and he was bashed as he rose. Part of the "leap" he saw is that, and it was mine.

**1. A fallen fighter's shield stays solid against his body** (Combat Feel "A Fallen Fighter's Shield Stays Solid
Against His Body", on; `UpperBodyArticulation.ShieldStoppedOffFeet`). Until now, the step a body was let go to
fall, what it held passed through it again. Now the board goes on being stopped by his trunk, head, thighs and
shins while he falls and lies, each pair by itself, and a board up to 6 cm into a part is stopped at once and
eased out (no faster than 3 m/s). His sword passes through him off his feet as before. Getting up, the board
still passes: the get-up's pose asks for it (see the plan below).

The first build stopped it on trunk and legs only, and was worse: kept out of his trunk, the board lay in his
sword arm. So off his feet it is stopped by his other arm and by the sword in that hand too. One bash-down shows
nothing headless (headless falls are cleaner than his); six in a run do (`Tools/Headless/downs.sh`):

| Six bash-downs in a run: crossing while falling and lying | Old (passes through him) | Trunk and legs only | Now |
|---|---|---|---|
| Hard bash, not countered | 0.02 s | 0.40 s (sword arm through the board) | 0.00 s |
| Extra hard bash | 0.55 s (shins, blade through board) | 0.97 s | 0.00 s |
| The six, 40 s, seed 5 | 0.52 s | | 0.00 s |
| The six, 40 s, seed 7 | 0.21 s | | 0.00 s |

The falls themselves measure the same (hips landing, hands first, how far he slides). The crossings while getting
up are not changed by this: 0.75 and 1.25 s in the same two runs.

**2. Fighters keep apart off their feet too** (Combat Feel "Fighters Keep Apart Off Their Feet Too", on;
`ClipFighter.KeepsApartOffHisFeet`). The cause of the two bodies in one place: the rule that keeps bodies apart did
nothing if either was down or getting up. Two who fell together got up inside each other, and their physical
bodies locked. Now a body needs room to get up in (1.1 m, hips to hips, at a Medium's size): one on his feet steps
off a body that is down or rising there (the walk's own steps; nearer than 0.66 m he is moved off as well, at
1.5 m/s at most), and one lying does not start to rise while another stands or rises there (he waits 3 s more at
most; of two lying together one goes first). A body that is down is never moved. Measured from the hips, not the
root: a get-up's root is where he will stand, up to half a metre from where he lies.

The room test (`Tools/Headless/room.sh`: two felled side by side, three times): in the one fall of three where
they lay close, old way the two were 0.23 m apart at the nearest while getting up and under 0.6 m for 1.63 s; now
0.66 m and never under 0.6 m, and both were on their feet 5.1 s after instead of 3.9. In the other two falls they
lay further apart than the rule looks and nothing changed (in one of them they still came to 0.59 m for a moment).
In the fights of six the rule did not come into play at all. **Not reproduced headless: the lock itself** (no
body went "out of hand" in either way), so whether his stuck feet are gone is for his eye.

**3. The lab's "Stumble him" waits until both are on their feet.** Pressed while either is down, getting up or
still stumbling, it says so and throws the bash once both stand. `DuelLab.PutOtherAt` refuses a body that is off
its feet.

**4. A held thing caught on the wrong side of his body is let through** (Combat Feel "A Held Thing Caught On His
Own Body Is Let Through", on; the lab's "Caught on his own body: let through / stays caught (old)" beside "Against
his body"). Found on the way. What he holds is stopped by his body from the moment it is clear of it, wherever it
then is. In one run a board came out of a get-up under his thigh and was stopped there, his arm pressing it softly
against the leg it could not pass: it hung 29 cm below its cover, facing 89 degrees off his enemy, his chest open
82% of the time, for the nine seconds the run had left, and the next bash at him met nothing. Now, when what an arm
holds has been against his body for 0.4 s with that hand more than 12 cm from its pose, and the pose has it clear
of him, it passes through him once and is stopped again when it is out. In the run where it came up: cover as it
should be (chest open 12%), at the price of one crossing of 0.10 s on his feet. It is rare: once in that run, once
(a Large's club) in two fights of six.

**The budget, honestly.** Crossings on his feet are as red as they were, not redder (seed 5: 0.53% against 0.54%
with today's switches off; seed 7: 1.16% against 0.92%, two fights that went differently). **The tremble reads
over its proposed 8% in both sixes with the off-feet shield on: 10.5% and 11.0%, against 7.5% and 7.3% with it
off.** In seed 5 I traced it: the two fights are the same until the one shield bearer's fall at 24 s; with the
board solid he came to lie on his side instead of his front, got up from his side, and it is his board, in the ten
seconds after, that makes the difference (the walking bursts of roadmap item 4, and the moment he stands). I have
not shown why a get-up from a side should leave more of it, and two seeds went the same way, so I do not call it
chance. Nothing of today's is in force on his feet apart from point 4.

**5. The plan.** Is the first moment of a get-up hard enough for a plan? Yes: three causes meet there (the start
does not look at how he lies; the way there is a straight blend that does not know what is in the way; his body
stops being solid to itself in that step), and the fix is a new stage, not a tweak. The plan is
`Docs/Plans/2026-11-19-from-how-he-lies-to-how-he-gets-up.md`
([live](https://claude.ai/artifact/FAVSQjVZCbJ87jf6aargPa)): he reads how he lies, gathers himself as the physical
body he fell in, and the get-up takes over only when he is close. Five steps, five questions for him. Nothing of it
is built.

**To see it in Unity.** Target test or duel lab, a swordsman and a target with sword and shield. "Stumble him:
Hard" with "Hard bash met: not countered", or "Extra hard", several times; watch the board while he falls and
lies. The toggles are in the target test's settings: "Falling and lying, his shield: stays solid / passes through
him (old)" and "Off their feet, two fighters: keep apart / nothing keeps them apart (old)". For the room: fell
both near each other ("Falls on demand") and watch who rises first.

**Headless.** `Tools/Headless/downs.sh tag`, `Tools/Headless/room.sh tag`; old with `FIGHT_SHIELDDOWN=0`,
`FIGHT_ROOM=0`, `FIGHT_CAUGHT=0`. The report has two new lines ("caught on his own body and let through", "off his
feet").

**Not done.** The shield through his body while getting up (his first picture): it is the plan's subject. His
frames of the three logs are used and removed from the repository.

### 19 November, afternoon: his answers; what to press is highlighted; the first moment of a get-up measured, and held still

Renier, 19 November, 12:26, having played the morning's build: "Your updates seem good, did not see the shield
going through the body as he fell this time." His five answers on the plan "From how he lies to how he gets up":
all as recommended (physics leads the gather; up to half a second longer on the ground if a case needs it; the
shield flat on the ground out to his side, pushed up on that forearm, coming up edge first; build in the plan's
order; a get-up may differ each time). And a new standing rule: **"have a highlight around the buttons you want me
to press in the debug… each time you push your changes."**

**His log of 12:12** (392 s, 21 falls and get-ups, the three switches of the morning on; measured in Unity): his
own shield and sword crossing him while falling and lying 0.33 s in all (one stretch, his blade through his board),
against 3.25 s over 28 falls at 08:07. No body thrown back onto its pose. While getting up 2.35 s, **all of it in
the first 0.75 s**; the fastest part there 6.0 m/s on average, 19 m/s at the worst. So the falls are mended as he
saw, and the first moment of the get-up is what is left.

**1. "Look here": the panel shows what to test** (`BattleSandbox.cs`, "Look here"). A box at the top of the panel
("Test this") lists the steps for the last push in plain words, and every button, row, slider, heading, section,
readout and lab named for it has a yellow frame that breathes slowly, wherever it is drawn. "Hide this and the
yellow frames" turns it off until the next push brings a new list. Three things are set for a push: `LookTag`,
`LookSteps`, `LookHere` (names exactly as on the panel; `"Heading>Name"` for a name that occurs more than once). The
panel check now ends with a `LOOK HERE` line saying how many of the names are really drawn in that lab. **Built
blind:** the frames are drawn with calls the headless stub does nothing with, so that they sit where they should
is for his eye.

**2. The plan's step 1: the first moment measured, and a stop frame.** Every get-up now reports its own first
0.75 s as seen: how long his own shield, sword or sword arm crossed him, the fastest part and which, and how far
each hand went. It is in his `getup` log lines ("the start: …"), in the headless report ("the first 0.75 s of a
get-up"), and on the panel ("His last get-up"). And "Hold him at the start: on" stops the game the moment any
fighter begins to get up, before he has moved; "Step" then goes on a step at a time. No change to the motion.

**3. The plan's step 2: the start chosen by how he lies. Built, measured, and left off.** (Combat Feel "A Get-Up
Starts Nearest To How He Lies"; the lab's "The start: nearest to how he lies / by his hips only (old)".) Every
start he could use is set out and weighed: the one his hips say, from his side too if his hips are turned that way
at all, and each posting on either hand (a sword alone always posts on its free hand). What a start costs: how far
his hands, elbows, knees and ankles are from where it has them as the blend ends, and 0.6 m more for a hand whose
straight way there passes through his trunk. The nearest is taken, if it is nearer by 0.15 m.

By itself it does not help, and in the bash-downs it is a little worse:

| A get-up's first 0.75 s, the shield bearer | By his hips only (old) | Nearest to how he lies |
|---|---|---|
| Six hard bash-downs: crossing a get-up / fastest part | 0.11 s / 7.8 m/s | 0.19 s / 8.1 m/s |
| Six extra hard bash-downs: the same | 0.17 s / 5.5 m/s | 0.19 s / 6.7 m/s |
| Felled backward, forward, to his right, to his left (one each): crossing | 0.07, 0.42, 0.48, 0.08 s | 0.07, 0.33, 0.42, 0.23 s |

Why: the start nearest for his joints is often the mirrored one that posts on his shield hand (7 of 11 in the
bash-downs took another start than his hips say), and that get-up lays the board under him: the crossings moved
into his shield-side thigh. The cost does not count the board. So the switch is **off** (a new field, so his scene
takes the default), kept with its toggle for him to see, and for step 3 to build on with the board in the cost. A
fault found on the way and mended: a start weighed left the pose where it had been tried, and the get-up then
pinned his sword hand there and threw it at 59 m/s; each start is now weighed from the pose as it was.

**To see it in Unity** (and the panel now shows this itself): Tubes; "Falls and getting up"; "The start of a
get-up": "Hold him at the start: on"; then "Falls on demand": Back, Forward, Left or Right; "Step" a few times;
"Let him go on". "His last get-up" says how the first moment went.

**Headless.** `FIGHT_STARTNEAREST=1` turns the new start on. `Tools/Headless/downs.sh` prints the first-moment line.

**The budget.** Nothing of this is in force on his feet, and the two fights of six read exactly as they did this
morning (crossings 0.53% and 1.16%, tremble 10.5% and 11.0%: both still over, for the reasons given then).

**Next: the plan's step 3, the gather** (he draws himself to the start as the physical body he fell in). Not begun.

### 19 November, 13:40: thrown by nothing after a tube's fall

Renier, 13:30, with his log of 13:26 (the tube test, where the panel's new "Test this" had sent him): "Saw some
weird things where they were being pushed by something invisible a while after shoving them with the tubes."

**What it was** (his log, measured in Unity): 14 times a target that had just got up was "thrown off balance by a
blow" with no blow anywhere near, at 2.2, 4.5 or 6.2 m/s, and 6 of the 14 threw him down again. Each came as he
came to his feet, two to three seconds after the tube.

**Why.** A blow that throws a fighter off balance leaves its knock waiting for his next step on his feet. In the
tube's fall test the same tube also fells him in that step, so the knock was never taken: it lay with him through
the fall and the get-up and threw him the moment he stood. It is from the night's stumbles (the knock goes to his
legs uncapped now; before, the same stale knock was a smaller shove). It was in my own headless falls too: two
stumbles "by a blow" and a second fall in every one of this morning's four-sided runs, which I read past. His
other logs of the day have none (0 in each): it needs a blow that both throws and fells in one step, which a
tube's fall test always is and a fight's blow rarely is.

**The mend.** As a fall begins, a knock still waiting goes with it (`ClipFighter.Fall`): the fall has its own
speed. No switch: it is a fault, not a choice. Headless, felled from four sides: stumbles after the get-up 2 a run
before, 0 now; the report counts them ("knocks still waiting as he fell, dropped with the fall").

**To see it:** the same steps as before (the panel's "Test this" has them, with a seventh: once he is up, nothing
should push him any more).

### 19 November, 14:05: the weaker push that was left: a test target going back to its spot

Renier, 14:02 and 14:03, with his log of 14:01 (made on the mend above): "I'm still seeing them hit by some force
after getting up? Is this just a bug with the tubes test?… the force is much less strong and does not knock them
over."

His log: no throw by nothing any more (0, against 14). What is left: a target that got up 0.5 to 0.64 m from its
spot stood for half a second and then went back to the spot at 0.7 m/s, in its guard, on its stance's steps, over
about 0.8 s. Nothing near it: to the eye, a shove. One that got up within 0.3 m of its spot stayed, and was not
"pushed". So yes: it is the test's own doing (a target dummy "driven off its spot walks back to it"), not the
fight's.

**The mend.** A target dummy that fell stands where it got up: its spot goes with it while it is down or getting
up (`FightLoop.DummiesStayWhereTheyGetUp`, on). The tube test sets its tubes beside where the targets stand, and
only once all of them are up. A target driven back by a blow that keeps its feet still walks back, as before.
After several falls the same way the targets will have wandered; "Reset" puts them back.

**Not checked headless**: the headless tube lineup has no target dummies, so the walk back never happens there
(felled the same way before and after, the runs are alike). It compiles and the panel check passes. For his eye.

### 19 November, 14:40: he gathers himself before he gets up (the plan's step 3, a first build)

**Headless only.** Combat Feel "He Gathers Himself Before He Gets Up" (on), with "Gather Takes At Most" (0.6 s) and
"Gather Muscle" (1). The lab: Tubes, "Falls and getting up", "The start of a get-up": "From how he lies: gathers
himself / blends (old)", the two levers, and "His last gather".

**What it does.** A little before he would begin to rise (0.4 s before, once he lies still), the get-up is set out
as it would be, but he is not taken up. He goes on as the physical body he fell in, and the pose his muscles draw
him to is the get-up's start instead of the fall's. Only his joints are drawn, each from the part it sits in, so
his trunk lies as it lies; the ground, his own body and his shield stay solid to his limbs (this morning's work),
so an arm goes round his chest and not through it. His strength comes in over 0.15 s. When each hand and foot is
within 12 cm of the start, measured from his own hips, or after 0.6 s, the get-up takes over, and its blend starts
from the pose he then has. His sword arm is left out: that hand stays where it lies (the morning's pin).

**Two faults found in the first runs, both mended before this was pushed.** His sword hand was thrown at up to
43 m/s as the get-up took over: it had been drawn to the start with the rest, and the get-up pinned it where the
pose had it and not where it was. So the sword arm is not gathered, and the get-up takes over from the body as it
is seen.

**What it measured.** The same build, the gather on and off, the shield bearer bashed down (11 and 12 get-ups):

| The first 0.75 s of a get-up | Blends (old) | Gathers himself |
|---|---|---|
| Hard bash-downs: his shield or sword crossing him, a get-up | 0.19 s | 0.11 s |
| The same: the fastest part, on average / at most | 5.1 / 6.7 m/s | 4.6 / 6.5 m/s |
| Extra hard bash-downs: crossing, a get-up | 0.22 s | 0.14 s |
| The same: the fastest part, on average / at most | 5.9 / 9.9 m/s | 4.2 / 6.5 m/s |
| Crossing through the whole get-up, both runs together (12 and 11 get-ups) | 3.42 s | 1.73 s |

Felled by the tubes from four sides (three targets a run) it is less clear: the fastest part a little lower each
time (2.8, 3.3, 4.7, 3.4 m/s to 2.7, 3.2, 4.1, 2.7), the shield hand going less far in three of four (0.72 to
0.40 m from his back), and the crossings the same (the shield bearer from his front still 0.47 s: that is the
get-up's own pose with the board in his hips, the plan's step 5).

**What it does not do yet.**
- **About half the gathers end by the clock**, with a hand or foot still 15 to 30 cm from the start, mostly from a
  side: his trunk has to turn for those, and nothing turns his trunk yet (his hips are free in a fall body). The
  plan foresaw it: a soft hold on the hips is the next piece.
- A gather takes 0.27 to 0.50 s. It begins 0.4 s early, so he lies about a tenth of a second longer than before
  at the most (his answer 2 allows half a second).
- The start is still the one his hips say (step 2 is off).
- Whether it reads as a man pulling his limbs in, or as a twitch before he rises, is for his eye. `FIGHT_GATHER=0`
  is the old way headless; `FIGHT_GATHERMOST`, `FIGHT_GATHERMUSCLE` are the levers.

**It does not thrash, in the numbers:** while gathering the fastest part moves at about 3 m/s (3.5 at the most, in
two fights of six).

**The budget.** Two fights of six with it on: crossings on his feet 0.54% and 1.11% (this morning 0.53% and
1.16%: as red as before); the board's tremble 8.8% and 9.0% (this morning 10.5% and 11.0%; the budget is 8%):
still over, a little less.

### 19 November, 15:40: the gather seen by him; turning his trunk tried, and left off

Renier, 15:33, having played the gather: "I think it looks fine for where it is now, you can continue."

**His log of 15:29** (measured in Unity; the gather on; 56 get-ups in the tube test, 15 fall tests and 17 blows):
in a get-up's first 0.75 s his own shield or sword crossed him 0.05 s a get-up over all three targets, and 0.16 s
for the shield bearer alone (17 get-ups); the fastest part 4.2 m/s on average (6.7 at 08:07, 6.0 at 12:12). No
throw by nothing (0). The 29 stumbles "from a blow" in it are his own blow tests.

**Turning his trunk in the gather: built, measured, left off** (Combat Feel "Gather Turns His Trunk", 0; the lab's
lever of that name; `UpperBodyArticulation.TurnHips`). While he gathers, a turning hold on his hips draws them
toward how the get-up's start has them, no harder than a share of his weight on a metre's lever: what a man does by
pushing on the ground. At a quarter of his weight, felled from four sides:

| Felled to his right / to his left (three targets each) | Trunk as it lies | Trunk turned (0.25) |
|---|---|---|
| Gathers ended by the clock | 2 of 3 / 2 of 3 | 3 of 3 / 2 of 3 |
| His hips still from the start's as the get-up took over | not measured | 21 / 28 degrees |
| The fastest part in the get-up's first 0.75 s, on average | 4.1 / 2.7 m/s | 11.5 / 5.0 m/s |

It does not finish those gathers in the time they have, and it makes the take-over rougher: he is still rolling
when the get-up takes him up, and the get-up takes a body up as if it lay still. So it is off (0), with its lever
kept. **It needs the plan's step 4 first**: a take-over that takes him as he moves. With the lever at 0 the falls
measure exactly as the first build's did.

**Next:** step 4 (the take-over from the pose and the motion he has), then the trunk again, then step 5 (the board
carried round him).

### 19 November, 19:30: a get-up takes him up as he moves (the plan's step 4), and a fault found in the test rig

**Headless only. Nothing here has been seen in Unity.** Combat Feel "A Get-Up Takes Him Up As He Moves" (on), with
"Take Over Takes At Most" (0.6 s). The lab: Tubes, "Falls and getting up", "The start of a get-up": "Taken up: as he
moves / as if he lay still (old)", the lever "Take-over takes at most", and "His last get-up", which now ends with
the take-over's own figures. Headless: `FIGHT_TAKEOVER=0` is the old way, `FIGHT_TAKEOVERMOST` the lever.

**First, the fault in the test rig.** This afternoon's entry said that turning his trunk in the gather "makes the
take-over rougher: he is still rolling when the get-up takes him up" (a part at 11.5 m/s on average, 26.9 at most).
Traced step by step, that was wrong. He was taken up at 0.2 m/s, not rolling. The headless stand-in for Unity moved
a kinematic body badly: one set by its position and then moved with `MovePosition` in the same step was swept the
whole way in one physics step where Unity places it at once. A get-up locks his hips to their carrier, and with the
trunk's turn on the carrier was up to 36 degrees round from his hips at that moment: swept there and back in one
step, it whipped his legs (a foot at 27 m/s). Unity does not do this. The stub is mended
(`Tools/CompileCheck/Stubs/Engine/Physics.cs`, `Runner/PhysX/PhysXWorld.cs`). With it mended, and before any of
step 4, the same falls with the trunk's turn on measured 5.0 m/s on average and 7.1 at most. **So the trunk's turn
was set aside for a reason that was not true.** Two consequences: headless numbers from before this entry are not
comparable with numbers after it (every "old" figure below is measured again, on this build); and the turn is to be
looked at again on its merits (below).

**A new measure: the take-over itself.** The plan's measure for this step is "no jump in any part in the step the
get-up takes over". Each get-up now reports, for its first 0.2 s, the biggest change in any part's speed from one
step to the next, as seen (a part stopped dead or thrown shows as a jump), with which part and when, and how fast
his fastest part was moving as he was taken up. The old way stops him dead: over twelve tube falls its jump was
about what he was moving at (1.2, 2.7, 4.0 and 1.4 m/s from the four sides, taken up at 1.2, 1.9, 2.6 and 0.9).

**What step 4 does.** With the switch on, the blend from how he lay is the old one changed in three ways.

1. **He keeps the speed he has.** His body's pose is kept for the last four steps while he is down; as the get-up
   takes him up, the speed of his hips, of each trunk part's turn and of each hand and foot is read off them (over
   three steps: what lies on the ground trembles from step to step) and the pose is carried on at that speed, eased
   out over 0.15 s. And the step he is taken up in is the get-up's first step: before, his body was held to the pose
   he lay in for one whole step first, which is what stopped him.
2. **His limbs go by where their joints are.** Each hand and foot goes straight from where it is to where the get-up
   has it, the elbow or knee turning round the limb, where before each part of the limb turned by itself.
3. **A part that is close takes less time.** Each part's clock is the time what it has left needs at 1 m/s or 150
   degrees a second, no less than 0.2 s and no more than the lever (0.6 s) times its share. A limb goes as one.

Four smaller things go with it, all only while the switch is on, each found by a jump it caused:

- What he holds is no longer brought out of the floor in one go as the get-up begins. That lift put his shield hand
  6 to 11 cm from where his body had it, and his arm jumped there in the first step. The pose now begins from his
  body as it is and goes over, in 0.2 s, to the one that began clear of the floor.
- The turn that lifts his blade clear of the ground (this morning's mend) came in from nothing at 30 degrees a step,
  so for the get-up's first steps its own sword arm was thrown up to 44 cm and came down again. It is taken whole in
  the first pose.
- A fall caught on a knee was taken up with the pose his fall was drawing him to, not his body, on the animation
  rig, and the get-up read where his sword hand lay off that: thrown at 15 m/s in the first step in a fight of six.
  The get-up now reads the body he has, whichever way it begins.
- His sword hand's pin (his rule: it stays where it lies): the place it is pinned to comes to rest with the speed
  the hand had; the hand is no nearer his shoulder than 0.4 of his arm (as his trunk came over it the arm folded
  right up and threw the elbow round at 6.7 m/s: the hand now slides out along the ground and stays where it slid
  to); and the way his elbow bends comes round no faster than 720 degrees a second.

**What it measured.** The same build, old way and new. Tube falls are the same falls both ways (three targets a
side); the bash-downs and the fights are chaotic, so they are a direction.

| Felled by the tubes | Jump in the take-over, old to new (m/s, average / most) | Fastest part in the first 0.75 s | His shield or sword crossing him, a get-up |
|---|---|---|---|
| Onto his back | 1.2 / 1.4 to 1.2 / 1.5 | 2.7 / 3.7 to 2.3 / 2.4 | 0.00 to 0.00 s |
| Onto his front | 2.7 / 3.7 to 1.3 / 1.5 | 3.2 / 3.8 to 2.7 / 3.5 | 0.16 to 0.11 s |
| To his right | 4.0 / 5.2 to 2.0 / 2.6 | 4.1 / 5.2 to 3.7 / 5.1 | 0.19 to 0.16 s |
| To his left | 1.4 / 1.8 to 0.9 / 1.0 | 2.6 / 3.3 to 2.7 / 3.3 | 0.02 to 0.02 s |

| The shield bearer bashed down | Old | New |
|---|---|---|
| Hard: jump in the take-over, average / most | 2.8 / 5.3 m/s | 0.9 / 1.3 m/s |
| Hard: the fastest part in the first 0.75 s | 4.8 / 6.3 m/s | 3.9 / 6.3 m/s |
| Hard: crossing in the first 0.75 s, a get-up | 0.21 s | 0.14 s |
| Extra hard: jump in the take-over | 2.5 / 4.0 m/s | 1.7 / 3.5 m/s |
| Extra hard: the fastest part | 4.7 / 6.5 m/s | 3.8 / 6.7 m/s |
| Extra hard: crossing in the first 0.75 s | 0.20 s | 0.23 s |
| Crossing through the whole get-up, both runs | 2.31 s (10 get-ups) | 1.88 s (9 get-ups) |

So: the jump at the take-over is about halved, most where he was moving most; the fastest part is a little lower;
the crossings are not changed by this step, as expected (what is left of them is the get-up's own pose: step 5).

**The budget** (two fights of six, 40 s, old way and new on this build; a third seed where the first had no fall):
| Seed | Crossings on his feet, old to new (budget 0.5%) | The longest (budget 0.10 s) | Board's tremble (budget 8%) |
|---|---|---|---|
| 5 | 0.77% to 0.58% | 0.22 to 0.10 s | 9.8% to 8.9% |
| 3 | 0.75% to 0.73% | 0.17 to 0.17 s | 5.7% to 6.2% |
| 7 (nobody falls, so the same fight) | 0.58% to 0.58% | 0.10 to 0.10 s | 7.1% to 7.1% |

Still over on crossings in all three, as before, for the reasons in CLAUDE.md (his blade through his own shield
while striking or bashing); not made worse. Fights are chaotic: one run a seed says no more than that.
**In the fights the take-over itself did not measure better**, on very few get-ups: seed 5, the jump 2.0 m/s on
average over three get-ups the old way, 3.6 over two the new way (one of them his sword hand, 4.9 m/s 0.18 s in:
I take it to be the hand sliding out from under his shoulder, below, but did not trace it); seed 3,
one get-up each way, 2.7 to 0.9. In a fight a hand lies up to 1.2 m from where the get-up wants it, further than in
any tube fall, and that is where the new way has been tried least.

**What it does not do, and what is still rough.**
- The plan's own measure, "no jump", is not reached: about 1 m/s is left in most take-overs, and I do not know how
  much of that is the measure's own floor (a body on the ground changes speed from step to step).
- The fastest parts left are not the take-over's: his sword hand at 6 m/s about 0.4 s in, as its pin lets go in a
  get-up from his front with the hand a metre from where the get-up wants it (the same old way and new); and his
  shield arm's forearm at 6.5 m/s 0.2 s into one get-up from his front (new only, in the first extra-hard
  bash-down). Both are the get-up's own arm poses.
- His sword hand sliding out from under his shoulder starts at once, with no easing: probably the 4.9 m/s jump of
  his sword hand seen once in a fight of six (not traced).
- Tried and not kept (in `GetUp.TakesOverAsHeMoves`'s own words): adding what was left on top of the get-up's poses
  with no blend (the textbook way; it shows every jump the get-up's own first poses make, a forearm at 12 to 20
  m/s); limbs with far to go, or the sword arm, turned part by part as before (a forearm at 10 to 15 m/s); a limb
  kept no nearer its root than it lay (a shin at 21 m/s).

**The trunk's turn again, a first look only** ("Gather Turns His Trunk" at 0.25, still off by default). With the
stub mended and step 4 on, it no longer makes the take-over rougher: felled from the four sides, the fastest part
2.3, 2.5, 2.6 and 3.1 m/s (without it 2.3, 2.7, 3.7 and 2.7), the jump 1.0 to 1.3 m/s. But it still does not finish
the gathers: 0, 1, 3 and 2 of 3 ended by the clock (without it 0, 1, 2 and 2), his hips still 11 to 27 degrees from
the start's. It needs more strength or more time; that is the next piece.

**Setting up a new workspace** needed one thing the briefing did not say: `dotnet` was not installed
(`apt-get install -y dotnet-sdk-8.0`).

**Next:** the trunk's turn in the gather, on its merits; then step 5 (the board carried round him).

### 19 November, 22:10: after his look at step 4: knees out of the ground, the sword hand held still

**What he said (in Unity, step 4 on):** "The getting up is a mess. Knees sticking into the ground. STILL the
sword shakes like crazy when he pushes himself up. Can't you just pin the hand, keep it still as he uses it to push
himself up?" And: the shield intersects him. He pushed two logs (18:29 and 19:57; frames looked at and removed).

**His log of 19:57 (53 get-ups, 38 from his front), what it shows:**
- Step 4 did what it measures: the fastest part in a get-up's first 0.75 s about 3.0 m/s for the armed targets,
  against 5.0 to 5.5 in his log of 15:29. Crossing 0.12 s a get-up, the shield target from his front 0.36 (no
  change: that is the pose, step 5).
- **Knees:** from his front a knee is 17 to 18 cm under the ground, as posed and as seen, for about 0.8 s of every
  get-up. The pose's fault: the key has his hips lower than a kneel on this body's legs needs.
- **Sword:** while his sword hand is down the blade's tip moves 53 mm a step, usually. Headless, the old way gives
  the same 53.

**Built (all on by default, each with its old way):**
1. **A rising fighter's knees stay above the ground** (Combat Feel; lab row "Getting up, his knees: stay above the
   ground / where the pose puts them (old)"; `GetUp.KneelIfUnder`). Where a knee is solved under the floor it is put
   on the ground, out from his hip the way it was solved and a thigh's length from it, and the shin lies from there
   toward where the foot was asked: he kneels on it. In the keys and in the take-over's blends. Tried first and
   dropped: sliding the foot back until the knee came up (laid the leg out straight, 85 cm at 5 m/s).
2. **His sword hand is held still** (Combat Feel "A Rising Fighter's Sword Hand Is Held Still"; lab row "While it is
   pinned: held still / may turn a little (old)"). Whenever the hand is down on the ground the pose does not move it
   or turn it at all: where it lay, from when it has all but stopped until the pin lets go, and where the get-up
   then posts it. And the physical hand is pinned to that place and held to that turn
   (`UpperBodyArticulation.PinHand` and the new `HoldHandTurn`, 20 Hz, 300 N·m), taken hold of where it is and
   brought to the pose's place over 0.2 s. In the air its turn is eased (0.2 s).
3. **A get-up keeps the start he gathered for** (Combat Feel; no lab row: nothing to look at). The start was chosen
   afresh from his hips as the get-up took him up. With the kneel in the gather's pose a fighter rolled a little
   past the line and was taken up by another start than the one he had just been drawn to: a shin at 7.3 m/s in a
   tube fall, a shield forearm at 25 m/s in a fight of six. Now the gathered start holds unless his hips have rolled
   more than about 17 degrees past the line.

**Measured, headless, old way to new on the same build** (three targets felled by the tube from each side; old is
`FIGHT_GETUPKNEES=0 FIGHT_GETUPSTILL=0`):

| | from his front | his back | his right | his left | shield bearer bashed down hard |
| --- | --- | --- | --- | --- | --- |
| A knee at the lowest, as seen (cm over the ground) | -13 to 5 | 21 to 21 | -12 to 4 | 8 to 14 (lowest -14 to 4) | 13 to 18 (lowest -13 to 4) |
| A knee in the ground (s a get-up) | 0.66 to 0.00 | 0 | 0.68 to 0.00 | 0.28 to 0.00 | 0.11 to 0.00 |
| Sword hand held still (s a get-up) | 0.48 | 0.31 | 0.68 | 0.32 | 0.51 |
| Blade's tip trembling, share of the get-up over 5 mm, as seen | 62% to 39% | 71% to 55% | 45% to 35% | 81% to 58% | 91% to 64% |
| Fastest part in the first 0.75 s (m/s) | 2.7 to 3.0 | 2.3 to 2.3 | 3.7 to 3.6 | 2.7 to 2.7 | 3.9 to 4.3 |
| The take-over's jump (m/s) | 1.3 to 2.3 | 1.2 to 1.2 | 2.0 to 2.6 | 0.9 to 0.9 | 0.9 to 1.4 |

- **The sword, honestly.** While the hand is down the pose is now exactly still, and the seen tip moves about 3 mm
  a step where it moved 53 (3 mm is as fine as the pose log reads). But the hand is down only 0.3 to 0.7 s of a
  3.5 to 4.5 s get-up. The tremble measure over the whole get-up fell by about a third, not to nothing: what is
  left is while the hand is in the air and as he stands up, where the keys themselves swing the sword arm (the
  measure also reads fast curved motion as tremble). The physical hold adds little to the pose's (from his front
  44% without it, 39% with).
- **Worse, and left:** the take-over's jump from his front and right. In one get-up of three a shin jumps about
  4 m/s: he is taken up while the gather is still moving that leg (at 3.6 m/s) to its kneel. One bash-down of six
  now got up from his left side with the shield forearm at 8.4 m/s (step 5's arm).
- **Fights of six** (40 s; the fights differ after the first fall, so these are not like for like): seed 5 crossing
  on his feet 0.58% to 0.57%, board tremble 8.9% to 10.9%; seed 3 0.73% to 0.67%, 6.2% to 6.7%.
- **Not seen in Unity:** all of it. The turning hold is a joint drive of a kind this project has not used before
  (a slerp drive on the hand's pin): if the sword hand twitches or spins while he pushes up, that is it, and
  "While it is pinned: may turn a little (old)" turns it off.

**Mended on the way:** the kneel let go when his hip was lower than the knee's rest (a shin at 8 m/s in a six);
the foot's floor came in at once (now as far as the knee was lifted); the hold stopped a hand dead that was still
being carried at 2.8 m/s (a forearm jumping 5.6 m/s; it is held from when it has all but stopped).

**Next:** his shield through him as he gets up (the plan's step 5).

### 19 November, 22:55: his log of 20:10: off the edge of the floor, and the jump as he stands

His third log of the evening (20:10, the bash lab, step 4 on, 30 get-ups; the frames looked at and removed) is the
session he called a mess. Read with the pose log, it showed two things no report of mine measured.

**1. He was bashed off the edge of the floor, three times.** The scene's floor is 40 m square. The pair, bashed back
again and again for five minutes, had walked to its edge; three hard bashes threw the target over it. He fell 20 to
34 m (hips at y -20.5, -22.4 and -34.5), was "put back on its pose at once" from there, and that get-up had parts
at 55 to 82 m/s. Not a fault of the fall or the get-up: there was nothing under him.
**Mended:** `BattleSandbox.EnsureFloorPastTheEdge` adds an unseen floor, a millimetre lower, 4 km square, with the
floor's layer and surface. **Not checked headless** (the headless ground has no edge) **and not seen in Unity.**

**2. Every get-up ended with a jump.** In that log, in the step after he stood, his forearms jumped 8 to 10 m/s
(13 to 16 cm in one step) and his hips about 3 (the median of 29 get-ups; the tubes log of 19:57 has it smaller,
the shield forearm 4.6). Headless it was there all along, unmeasured: the report only looked at a get-up's first
0.75 s. Two causes:
- **The settling of what he holds was a fall old.** `SettleHeld` keeps where each hand had settled to, and how much
  of that to keep. It does not run while he is down, so as he stood it let go, over three steps, of a place from
  before he fell: the shield hand 12 cm out and back (8 to 9.5 m/s), the sword forearm 13 to 15 m/s in the bash
  lab. Now it starts afresh as he stands.
- **Only his arms were eased out of the get-up** (`EaseArmsFromGetUp`, 0.4 s). His hips, trunk, head and legs were
  where the stand has them in the next step. Now the whole of him is eased, the rest over 0.2 s
  (`ClipFighter.WholeBodyEasesOutOfGetUp`, `RoseBodyEase`), from the body as it is seen.

**New measure:** "the end of a get-up, as seen": the biggest jump in any part's speed from one step to the next,
from 0.15 s before he stands to 0.40 s after (`ClipFighter.GetUpEndJump`; a line in every fight's report and in
each get-up's log line; a lab's reset of its pair is not counted). Combat Feel "A Get-Up Ends Into His Stand Over A
Moment"; lab row "The end of a get-up: eased / at once (old)"; `FIGHT_GETUPEND=0`.

**Measured, headless, old to new on the same build (m/s):** tube falls from his front 4.6 to 0.7, his back 8.2 to
2.9 (a shin, 3.5 to 3.9, is what is left), his right 5.0 to 1.4, his left 5.8 to 0.9; the shield bearer bashed down
hard 14.3 to about 1.5 (each of six: 13.4 to 14.8 before; 1.3 to 1.6 and one 4.3 after). The fight of six, seed 5:
17.8 (44.6 at most) to 1.1, and crossing on his feet 0.57% to 0.09% (under the budget for the first time; the
fights differ after the first get-up, so part of that is chance), board tremble 10.9% to 10.3%. Seed 3 (one
get-up): 3.9 to 3.8, budgets the same.

**Also in that log, not yet looked into:** from his front the shield forearm is at 9 to 12 m/s about 0.15 s into
five of eleven get-ups (headless shows the same moment at 1.4 to 2.9). It is the shield arm at the take-over:
taken with step 5.

### 20 November, 00:30: his shield carried round him as he gets up (the plan's step 5), and arms that go out in a stumble

**1. The shield (step 5).** Getting up from his front or a side, the keys put the shield hand flat on the ground
under his shoulder, as an empty hand pushes up, and the hold toward the threat came in over the first third of a
second: the board stood in his chest and hips on the way (his screenshot of 19 November; "the shield intersects
him").

- **Built** (Combat Feel "A Rising Fighter's Shield Is Carried Round Him"; lab row "Getting up, his shield: carried
  round him / as recorded (old)"; `GetUp.ShieldCarriedRound`; `FIGHT_GETUPSHIELD=0`). From his front or a side the
  board lies on the ground out to his shield side, its middle 28 cm out from his shoulder, his hand on its grip
  and his elbow up and back as in a push-up: he pushes up on that hand. It comes up by how high his shoulder is
  (from 55 to 95 cm over the ground), turning from flat to facing what he fights and swinging 12 cm out round his
  side. The gather is given the same place, so the body draws its shield arm there before the get-up takes over.
  From his back nothing changes (it never crossed him there).
- **A reading taken.** His answer 3 was my wording: "his forearm flat on it; he pushes up on that forearm". This
  shield has a centre grip: its face is square to the forearm, so a forearm cannot lie along it. Taken instead: the
  board flat and out to his side as he chose, pushed up on by the hand that grips it.
- **Three things tried and changed on the way:** the arm fitted inside its joints' limits would not lay the board
  flat and changed the elbow's side as the hand moved (an elbow 43 cm across in one step): the carried arm is
  placed by plain geometry, the fitted arm coming back in as the board comes up. The hand led to its place in a
  straight line whipped the forearm (20 m/s): the arm comes there by turning its joints from the shape it had,
  over 0.3 s or longer if it is far (1.6 m/s). Which way is "out to his side" flipped as he rolled: chosen once a
  get-up.
- **Measured, headless, old to new (three targets felled by the tube; the shield bearer is one):** from his front,
  his shield or sword in his own body off his feet 0.47 s to 0.00 s; in the first 0.75 s 0.13 s a get-up to 0.00
  (0.38 to 0.00 at most). From his right 0.52 to 0.12 s, and 0.14 to 0.04 (0.43 to 0.12 at most). From his back
  and left nothing changed (the shield bearer got up from his back in both).
- **What it costs:** the fastest part in the first 0.75 s from his front 3.0 to 3.9 m/s (3.2 to 5.6 at most), the
  take-over's jump 2.3 to 3.3; from his right 3.6 to 3.9 and 2.6 to 2.4, and the end of the get-up 1.4 to 2.8
  (5.4 at most). As the board turns up, about 1.3 s in, its rim is 3 to 5 cm in the ground for a tenth of a second
  (before: 0 to 1 cm).
- **Not shown by these tests:** the shield bearer bashed down hard got up from his back all six times, so the
  bash-downs measure the same old and new. Fights of six: seed 3 the same (no shield bearer got up); seed 5 the
  fights differ (crossing in the first 0.75 s 0.18 to 0.13 s; on his feet 0.09% to 0.22%, both under the budget).
- **Not known:** whether the shield forearm's kick 0.15 s into a get-up from his front, seen in his log of 20:10
  (9 to 12 m/s), is gone in Unity. Its path there is a different one now.
- A drawing of the posed arm at six moments (from above and from his head end) was made from a trace and looked
  at: the board lies clear of his trunk beside his shoulder, the forearm comes straight down onto it, and it is
  up beside his shoulder by 1.5 s.

**2. Arms in a stumble.** His arms did nothing of their own while he stumbled: they held his guard.

- **Built** (Combat Feel "A Stumbling Fighter's Arms Go Out For Balance", lever "Stumbling Arms Go Out By"; lab
  row "Stumbling, his arms: go out for balance / hold his guard (old)" and lever "Arms go out by";
  `ClipFighter.ArmsGoOutInAStumble`, `StepArmsOut`; `FIGHT_ARMSOUT=0`, `STUMBLE_ARMS=1.5`). Each hand goes out to
  its side, 9 cm for each m/s he goes and 36 cm at the most, and up by two fifths of that; the hand on the far
  side from where he tips goes the further; both are left behind a little by the way he is thrown (5 cm for each
  m/s). Out quickly, back more slowly as he catches himself. Not the sword arm in a blow of its own.
- **Tried first:** turning his upper arms out at the shoulder. His guard has his hands straight ahead of his
  shoulders, so his elbows flared 25 to 64 degrees and his hands hardly moved (76 to 84 cm apart). Put before the
  cover and the guard it was undone by them: it is the last thing that poses his arms.
- **Measured, headless (`four.sh`, the same stumbles old and new):** a medium bash, two stumble steps: his hands
  72 cm apart, to 101 at the widest. A hard bash countered, three steps: 97 to 121. A hard bash not countered (he
  goes down): 72 to 124. The stumbles themselves are step for step the same: this changes his arms only. Crossing
  on his feet 0.00% in each, as before.
- **Fights of six (40 s):** moved out and still turned as his guard had it, his blade went through his own board
  in a stumble (0.15 s, seed 5). What each hand holds now turns out to its side with the hand (40 degrees at the
  most): crossing while stumbling 0.00 s of 6.7 s (seed 5) and 0.00 of 0.3 s (seed 3), as before the arms. On his
  feet in all: 0.67% and 0.72% (before tonight 0.58% and 0.73%: over the budget as it was; the fights differ).
- **Not seen in Unity.** A sword and a board thrown wide may read as too much: the lever is there for that.

### 20 November, 01:15: bodies that take an impact in their knees and hips (the impacts plan's step 4, a first build)

**Where it stood.** The plan's step 4 ("the bodies absorb it") was not built. Since the stumbles, the meter already
shows most of what it asked for on a held hard counter: both trunks move (26 and 16 degrees) and the bashed man's
hips drop 15 cm, because a man thrown off balance sinks with his stumble's knees. What was missing: **the one who
keeps his feet stood on stiff legs.** The basher's hips went down 4 cm on a hard counter, 0 cm on a small bash.

**Built** (Combat Feel "A Body Takes An Impact In Its Knees And Hips", with "Impact Contact Time", "Impact Knees
Give", "Impact Trunk Folds"; lab, under "Stumbles and falls": row "An impact, his knees and hips: give / stand stiff
(old)" and levers "Contact time", "Knees give to it", "Trunk folds to it"; `ClipFighter.BodiesTakeImpacts`,
`PressInto`, `StepPress`, `LocoBody.Pressed`; `FIGHT_PRESS=0`, `FIGHT_PRESSED="0.2,0.10,4"`).
- Whatever comes in through the door (`ThrowBody`: a bash on him, his own bash meeting a board, a heavy blow that
  throws him) presses him down as it lands. His hips drop by how much it changes his speed (10 cm for 3 m/s, never
  more than 15), lowest a fifth of a second after it, and he comes up again over about half a second. A spring,
  nothing drawn by lot. Where a stumble already has his hips lower, the stumble's is used: the two do not add.
- His trunk folds toward where it came from as he is pressed (4 degrees for a hard one).
- What his hands have settled to goes down and up with him (it is kept against his root, which does not dip).

**Measured, headless (`four.sh`, the same four bashes old and new; the stumbles are step for step the same):**

| | basher's hips down | bashed man's hips down | basher's trunk | bashed man's trunk |
| --- | --- | --- | --- | --- |
| Small bash | 0 to 5 cm | 11 to 11 | 13 to 12 degrees | 9 to 8 |
| Medium bash | 7 to 7 | 6 to 6 | 18 to 15 | 9 to 7 |
| Hard bash, countered | **4 to 11** | 15 to 15 | 26 to 25 | 16 to 14 |
| Hard bash, not countered | 4 to 11 | 43 to 42 (he goes down) | 35 to 27 | (down) |

- **The plan's target for a held hard counter** (both hips down at least 8 cm, both trunks at least 15 degrees):
  hips now 11 and 15 cm, met. Trunks 25 and 14 degrees: the bashed man's is a degree under, because the fold toward
  the impact takes from his rock back. With "Trunk folds to it" at 0 his trunk moves 16 again.
- **Fights of six (40 s, seeds 5, 3 and 7; the fights differ run to run):** crossing on his feet 0.67%, 0.72% and
  1.02% before; 0.57%, 0.82% and 0.35% after. Board tremble 9.0%, 5.9% and 8.3% before; 8.7%, 8.0% and 6.3% after.
  No worse on average, worse in one seed of three. (Before what his hands hold dipped with him, crossing on his
  feet was worse in all four runs tried.)
- **Not built of step 4:** the feet sliding a hand's width before the first step; the head lagging; a slow-motion
  button (the lab has none). The medium bash is unchanged because both men stumble in it.
- **Not seen in Unity.** This is the step the plan says he judges by eye.

### 20 November, 01:40: kicks, a first build (the fight rhythm plan's step 9b)

**Built** (Combat Feel "A Kick Throws A Body", "Kick Power", "Kick Goes Into Him"; duel lab, "Stumbles and falls":
row "Kick him: Front kick", levers "Kick power" and "Kicks into him", and a line "His last kick";
`ClipFighter.KickAt`, `StepKick`, `Kicked`; headless `FIGHT_KICK=1` with `FIGHT_BASHAT`, `FIGHT_KICKED="2.6,0.25"`).

- **The recording.** The plan says kicks are built from the kick recordings. Of those, one is in the repository:
  the sword-and-shield pack's kick (`Tools/Mocap/Combat/samples/sword_and_shield_kick.json`), a front kick with the
  back (right) foot, 1.2 s long, in place; the foot is furthest out 0.57 s in, 1.8 leg lengths ahead of where he
  began and chest high. It is baked into the game's clips as `sns_kick` with a role of its own, "kick", so nothing
  that picks a sword's strike can pick it (`combat_bake.py`, `KICKS`; nothing else in `CombatClips.json` changed).
  **The five standing kicks of the kick pack and the Capoeira kicks are not in the repository** (they were
  measured from files on his computer): they are for him to add, or for a session linked to his computer.
- **How it goes.** He turns to the other and plays the recording whole, his hips going in more or less than the
  recording's so that the foot ends "Kicks into him" (25 cm) past the other's front. From 0.38 to 0.68 s in, as the
  foot comes within 24 cm of the other's middle line (hips to head) or 30 cm of his shield hand, it lands. The
  other is thrown through the door every impact uses (`ThrowBody`): at "Kick power" (2.6 m/s for a man of the
  kicker's weight; six tenths of that on a board). Shoved, stumbling or down is his legs' to decide. His upper body
  rocks back and his guard is open a moment (not if his board took it). The kicker is checked and pressed by what
  his leg met.
- **Not built:** no fighter kicks by himself (lab only, like the extra hard bash); the kicker cannot be thrown off
  his one leg; low kicks at a man on the ground (the downed-play plan); the other kicks.

**Measured, headless (the duel, the other stood 1.4 m off unless said):**

| The other | Where it landed | Thrown at | What he did |
| --- | --- | --- | --- |
| Sword and shield, 1.2 m off | his board, 91 cm up, 0.43 s in | 1.6 m/s | one stumble step, 0.38 m, kept his feet |
| Sword and shield, 1.4 m | his board, 99 cm up | 1.6 | one step, 0.38 m |
| Sword and shield, 1.7 m | his board, 106 cm up | 1.6 | one step, 0.38 m |
| Sword only | his body, 112 cm up, the foot at 8.9 m/s | 2.6 | two steps, 1.58 m, kept his feet |
| No weapons | his body, 112 cm up | 2.6 | shoved back 0.97 m |
| Sword only, "Kick power" 4.2 | his body | 4.2 | thrown 2.8 m and down |
| Sword only, 2.3 m off | it met nothing | | |

The kicker ends 0.13 to 0.41 m forward of where he began. No crossing on his feet in any of them. A fight of six
(seed 7) is the same to the hundredth of a second with the kick's recording in the clips: nothing picks it.

**Not seen in Unity.** How the recording looks on his legs under a sword and shield, and whether the foot's
landing reads as a kick that connects, are for his eye. 112 cm is high for a front kick: it is where the recording
has the foot.

Also in this push: the README, plan and panel times of the impact step corrected to 01:15 (written as 01:20).

### 20 November, 02:00: the pounce on an opening, a first build (the fight rhythm plan's step 10)

**Built** (Combat Feel "A Fighter Pounces On An Opening", "Pounce Reads In"; duel lab, "Stumbles and falls": row "An
opening: he pounces / he stands off (old)", lever "Pounce reads in", and a line "His last pounce";
`DuelLab.StepPounce`, `DuelLab.Pounces`, `ClipFighter.CutBashShort`; `FIGHT_POUNCE=0`, `FIGHT_POUNCEREADS`; the
headless bash and kick tests print a `POUNCE` line 2.6 s after).

- **An opening** is the other on his feet and stumbling, his guard knocked open, or his back turned (120 degrees or
  more round from the mover). A man on the ground is not one here: that is Downed play.
- **What he does.** 0.15 s after it begins ("Pounce reads in"), as soon as he is free (not down or stumbling
  himself, not in a blow, a kick or a leap; the rest of his own bash is dropped once its board has landed), he goes
  for it. In reach (1.35 m) he strikes at once: a light blow at the chest. Out of reach, up to 4.5 m, he leaps in
  to that distance, a quick low leap at 6 m/s, and strikes as he lands. Once he has leapt he is committed: the blow
  follows for 1.2 s though the other has found his feet. Once for each opening.
- **The duel lab only**, as the plan has it until the fight of six takes the dance in (its step 13). The fight's
  own planning (`FightLoop`) is not touched.

**Measured, headless (old way: after a bash he stood off, 2.1 m apart, no blow):**

| What made the opening | He | The blow was thrown | It |
| --- | --- | --- | --- |
| Small bash (the other 0.96 m off, stumbling) | struck from where he stood | 0.20 s after the opening began | landed |
| Medium bash (1.14 m off) | leapt 0.75 m, 0.73 s after | 1.23 s after | met his shield |
| Hard bash, countered (thrown 2.2 m back) | leapt 2.12 m, 0.97 s after | 1.65 s after | met his shield |
| Kick on a body (1.20 m off) | leapt 1.24 m, 0.72 s after | 1.27 s after | was parried |
| Hard bash, not countered | nothing: the other went down | | (Downed play) |

- **He is late after his own bash or kick.** The opening begins as his board or foot lands, and he is not free
  for 0.7 to 1.0 s: his bash's leap and his kick's recording have to end first. By the time the blow is thrown the
  other is at the end of his stumble, and he blocks or parries it. Only after the small bash, thrown standing, is
  the pounce quick enough to land. Making it quicker means letting a blow start out of the end of a leap or a
  kick: not built.
- Crossing on his feet in these runs: 0.00% to 0.14% (0.00% before).
- **Not seen in Unity.**

### 20 November, 02:20: a man who is down: a first piece of Downed play (its plan's steps 0 and 1)

The Downed play plan (16 November) has ten steps and nothing of it was built. Its first question is whether a made
blow can be aimed at the ground at all, "better known at the start". This is that, and no more.

**Built** (Combat Feel "A Fighter Strikes A Man Who Is Down", **off by default**; duel lab, "Stumbles and falls":
row "A man who is down: he holds off / he strikes him", button "Strike him where he lies", and a line "His last
blow at a man who was down"; `DuelLab.StepDowned`, `StrikesTheDowned`, `StrikeDownedOnce`; `FIGHT_DOWNED=1`; the
headless bash and kick tests print a `DOWNED` line 4.4 s after).

- With the switch on, or asked for once from the lab: while the other is off his feet and within 6 m, has been
  down 0.45 s and his chest lies lower than 50 cm, the mover, as soon as he is free, leaps to 1.05 m from where the
  chest lies (twice at most) and strikes at it with the blows there are today, aimed at the spot (`AimStrike` at
  his chest). Once each time he is down.
- **Off by default, on purpose.** Whether a fighter holds off while his enemy is down is a decision of his that is
  still open, and with it on every fall in the lab would be followed up, also the ones he fells to watch a get-up.
- A reading taken: the plan's "near is 3 m" is for when a man may get up from his back. A man kicked or bashed
  down lands 3.5 m and more off, so the follow-up reaches further (6 m).

**Measured, headless (a sword-only target kicked down from 1.4 m, "Kick power" 3.6 and 4.2):**

| | Kick power 3.6 | Kick power 4.2 |
| --- | --- | --- |
| The other had been down | 1.68 s | 1.35 s |
| His chest lay | 1.55 m off, 17 cm over the ground | 1.67 m off, 16 cm |
| He leapt in | 0.50 m | 0.62 m |
| Struck from | 1.51 m, at a spot 16 cm up | 1.35 m, at a spot 13 cm up |
| The blow made | a forehand; "the blade expected to reach it" | a rising cut on a forehand; expected to reach |
| It | landed on him | landed on him |

- **So: yes, today's made blows can be aimed at a man on the ground, and land** (headless; two of two). The plan
  feared they could not.
- **Tried first and changed:** gone for the moment he was "down", the other was still in the air (his chest 94 to
  115 cm up) and slid on a metre and more: the blow fell 78 to 115 cm short. He now waits until he lies.
- **Not built (the rest of the plan):** the stab and the low slash as blows of their own; how low he bends and
  whether his hips stay over his feet; what the blow does to the man lying beyond being counted as a hit; low
  kicks; rolls; the guard from his back; reading the follow-up; victory laps; the numbers per fighter; any of it
  in the fight of six.
- **Not seen in Unity.**


### 20 November, 08:30: the second pass after his look: one motion, three kicks, a bare-handed man falls, the jump as he stands

He looked at the night's work in Unity (three logs, 06:28 to 06:42) and said: the unarmed dummy "stumbles back
pretty far with legs moving pretty fast, where I would expect it to fall"; the kick works, "let's also have soft,
medium and hard. The current one is soft. I want hard to be hard enough to kick the enemy making it fall
backwards"; a man who is down he saw once and wants a test scenario for; "if an enemy is knocked back far he needs
to run after it", and "he ran, stopped then attacked. That won't work... his running at the fallen enemy, and
attacking it must be one motion without a pause in between". All of it headless only below, unless it says his log.

**1. Running in and striking is one motion** (`DuelLab.GoFor` / `StepGoing`; Combat Feel "He Goes For Him At",
3.6 m/s; the lab lever of the same name; `FIGHT_GOESAT`; `FIGHT_GOTRACE=1` prints each step of the blow).

- The first build leapt in, landed, and then struck (his log, 427.75 s: "leapt 0.29 m and struck from 1.36 m").
  Now he runs at the spot, and **while he is still running** throws the blow, half a second's run before he is in
  reach, over his run (the blow on the move that the walk already had, `AttackOnTheMove`). Through the blow he is
  paced to be at his striking distance (1.1 m) as it lands; then he stops.
- Both use it: the pounce on an opening, and the blow at a man who is down.
- **A man who is down is run after from any distance** (was: within 6 m, after he lay). He sets off the moment
  the other is off his feet; only the blow waits until he lies (down 0.45 s, chest under 50 cm).
- Of the blows he could throw on the run he takes the best of those that **land within 0.75 s**
  (`AttackOnTheMove.LandsWithin`, set for this asking only). Found on the way: left to itself the planner took a
  rising cut that landed 1.32 s later, and he was at his distance and stood 0.5 s waiting for it: his "ran, stopped,
  then attacked" again, by another road.
- His own bash or kick is dropped once it has landed (`CutBashShort`, new `CutKickShort`), so he does not stand
  through the rest of it.
- **One-press tests** (his "we need to set up a test scenario"): duel lab, "Stumbles and falls", row "Knock him
  down and strike him: Near / Far". Near is a hard kick. Far is a kick made only for the test (8 m/s), to land him
  five metres off. Each also asks for the blow at the man who is down, once. A line "Going for him now" says where
  he is in it.

| Headless | Lay | He ran | Threw the blow | Slowest until it landed | As it landed | It met |
| --- | --- | --- | --- | --- | --- | --- |
| Down, near (hard kick, sword-only man) | 3.48 m off | 3.94 m at 3.6 m/s | 2.85 m from the spot, going 3.6 m/s | 2.4 m/s | going 2.2 m/s, 1.00 m from the spot | his left shin |
| Down, far (the test's kick, sword-only) | 4.58 m | 4.07 m at 3.6 | 2.87 m, going 3.6 | 3.4 | 3.4 m/s, 1.28 m | his left upper arm |
| Down, far (the same, shield man) | 4.18 m | 6.97 m at 3.6 | 2.88 m, going 3.6 | 3.2 | 3.2 m/s, 1.41 m | his left forearm |
| Pounce after a small bash | 1.66 m | 0.38 m | 2.00 m, going 1.8 | 1.8 | 2.7 m/s | blocked by the shield |
| Pounce after a medium bash | 2.30 m | (in reach) | 1.95 m, going 1.8 | 1.8 | 2.3 m/s | blocked by the shield |
| Pounce after a hard bash (countered) | 3.07 m | 0.68 m | 2.48 m, going 2.8 | 2.0 | 1.8 m/s | nothing |
| Pounce after a soft kick | 1.41 m | (in reach) | 1.49 m, going 0.8 | 0.8 | 1.7 m/s | blocked by the shield |

- Every blow was "thrown over his run"; in none did he stop before it landed.
- **Honest about it:** the blow at a man who is down is aimed at his chest and met a shin, an upper arm and a
  forearm: whatever of him lay nearest. A man lying with a shield got it in the way once. The pounce's blow is
  blocked three times in four, and after the hard bash the lab's dummy had walked back toward him and the blow
  passed it. He does slow as the blow lands (from 3.6 to 2.2 m/s at the least). None of it seen in Unity.

**2. Kicks: soft, medium, hard** (`ClipFighter.KickAt(at, whenever, kind)`; Combat Feel "Medium Kick Power" 3.3
and "Hard Kick Power" 5.2, "Kick Power" 2.6 is the soft one; the lab's "Kick him: Soft kick / Medium kick / Hard
kick" and the two new levers; `FIGHT_KICKKIND=soft|medium|hard|far`). One recording for all three. A harder kick
goes further through a board (0.6, 0.75, 0.9 of it).

| Headless, 1.2 s after | Soft (2.6 m/s) | Medium (3.3) | Hard (5.2) |
| --- | --- | --- | --- |
| A man with a sword only (on his body) | 2 stumble steps over 1.60 m, on his feet | 4 steps over 4.06 m, on his feet | **down**, 3.54 m off |
| A man behind a shield (on his board) | thrown at 1.6: 0.39 m back | thrown at 2.5: 2 steps over 1.29 m | thrown at 4.7: **down**, 3.34 m |
| A bare-handed man | 3 steps over 2.43 m, on his feet | **down**, 2.43 m | **down**, 3.61 m |

- Medium was tried at 3.6 first: a sword-only man went down from it, so it is 3.3 (at 3.0: 3 steps over 2.80 m).

**3. A man not in his guard stumbles and falls too** (Combat Feel "A Man Not In His Guard Stumbles And Falls
Too"; lab row "Not in his guard: he stumbles and falls / he is only shoved (old)"; `FIGHT_UNGUARDED=0`).

- The cause of what he saw: the door every impact goes through (`ThrowBody`) only let a fighter **standing in a
  guard** stumble or fall. Anyone else was shoved, however hard: in his log the bare-handed target was "shoved back
  at 2.6 to 6.0 m/s" by every kick and never stumbled.
- Headless, a hard kick on a bare-handed man: old, shoved 2.05 m in 1.2 s, on his feet; new, down (table above).
- It reaches the fight of six too: a fighter caught with his guard down can now stumble or fall there.

**4. The jump as he stands: found and mended** (`ClipFighter.ArmsOutForgottenOnceUp`; `FIGHT_ARMSFORGOT=0` for
the fault; `FIGHT_ROSETRACE=1` prints the easing step by step).

- His logs: in half of this morning's get-ups a hand or forearm jumped 8 to 13 m/s one step after he stood. The
  lab's falls on demand hardly showed it (0.7 to 3.9), the duel's fighters nearly always (8.5 to 12.2).
- **The cause was last night's "arms out in a stumble".** A man who stumbled and then fell kept, through the fall
  and the get-up, how far his arms were out as he went down. As he stood they were put out again by that much (up
  to 36 cm, in one step) and came back. The lab's falls on demand fell him with no stumble, which is why last
  night's measure did not see it. It is forgotten once he is up.
- Headless, a man kicked down and getting up: 20.6 m/s (his left hand, one step after he stood) before; 5.4, 3.4,
  4.2 and 5.1 after, and those are a foot or a shin taking a step a quarter of a second later.
- The fights of six (40 s): seed 5, 19.0 m/s on average (27.2 at most) before, 1.8 (4.6) after; seed 7, 19.0
  before, 2.0 after.

**5. Slow motion** in the lab: "Stumbles and falls", row "Slow motion: full speed / half / a quarter / a tenth".
The game's own clock slowed; nothing else changes. Only its drawing is checked headless.

**The fights of six** (40 s, new against the old way of 3 and 4): seed 5: crossings on his feet 0.00% (old 0.57%,
over its budget), board tremble 6.6% (old 8.7%, over); seed 7: crossings 0.33% (old 0.35%), tremble 7.7% (old
6.3%; the budget is 8%). All budgets kept. Off his feet in seed 5 a shield was in its bearer for 0.20 s of 14.1 s
(old 0.03 s of 9.3 s): more men fall now. No exceptions anywhere.

**The panel:** "Look here" lists this pass (13 names; drawn and framed 13 of 13 in both labs).

**Not done in this pass:** the kick lands 112 cm up for all three strengths (one recording); nothing kicks, pounces
or strikes a man who is down outside the duel lab; the blow at a man who is down is still a standing blow aimed
low. **None of this has been seen in Unity.**

### 20 November, 11:52: a stab for a man who is down, a pounce that goes for what is open, and the dance's fighters doing these by themselves

He looked at the second pass ("this all looks good") and asked for what I had listed next, with one change: "do all
that but not the fight of six, keep it in the dance for now". Three steps, pushed together. Headless only.

**1. A blow of its own for a man who is down** (the Downed play plan's step 1; Combat Feel "A Man Who Is Down Is
Struck From Beside Him" and "He Bends To A Man Who Is Down By"; duel lab row "A man who is down, his blow: a stab
from beside him / a standing blow, straight at him (old)" and lever "He bends to it by"; `RunIn.DownedComesBeside`,
`DownedBlow`, `DownedCrouch`, `DownedLean`, `DownedStandsOff`; `FIGHT_DOWNEDBLOW="beside,sns_thrust,0.2,1.8,0.8"`).

- **What was wrong, found by looking at where he stood:** the pushed blow ran straight at the man and swung a
  standing forehand or rising cut low. A cut travels along a line; from beside a man who lies it travels along his
  length, so it meets his head or his legs before his chest. And a blow's "it met" in my report was the **last**
  part it went through, not the first: mended, the report now lists every part in the order met.
- **Now:** he runs to a place beside the man's chest, 0.8 m from it and square to how the man lies, on the side he
  is nearer. He throws his thrust, the point going down, bent to it (hips carried 0.2 leg lengths lower, the
  chest's lean times 1.8): a stab. It goes for the first of chest, belly, hips, head and neck that is open from his
  sword shoulder. Going to a place, he throws the blow 0.35 s out and slows as he comes, so that he is on the place
  as it lands.
- A blow thrown on the run at a man who lies faces the spot it is for (it faced his hips, his root).

| Six knock-downs (sword, shield, bare-handed; kicked 3.5 m and 5 m off): what the blow met first | Trunk or head | The rest |
| --- | --- | --- |
| The old way (straight at him, the best-fitting blow) | 2 of 6 (head, spine) | an upper arm, a hand, nothing, one not read |
| A thrust from beside him, standing tall, 0.95 m off | 4 of 6 (chest twice, head, spine) | an upper arm twice |
| **The stab as set: bent, 0.8 m off, at what is open** | **5 of 6 (chest three times, head twice)** | a thigh |

- In all six he was never slower than 2.2 m/s before it landed (2.2 to 3.5).
- **Tried and not taken:** the same thrust with its own lunge, his run carried into it. It met the chest, but he
  braked from 3.6 m/s to a stand in a quarter of a second and stood 0.3 s before the point landed: "ran, stopped,
  then attacked" again.
- **Rough:** where the point lands is 12 to 59 cm over the ground: the higher ones are a man already getting up
  (the far cases). The thrust is "expected to fall short" by 1 to 43 cm and still lands: the man is not flat.
  How low he bends is a setting, not measured. Not built: the low slash.

**2. The pounce goes for what is open, and a stumbling man cannot guard** (Combat Feel "A Blow On The Run Goes
For What Is Open" and "A Stumbling Fighter Cannot Guard"; duel lab rows "An opening, his blow: what is open / his
chest (old)" and "Stumbling, his guard: he cannot guard / by the clock (old)"; `RunIn.StrikesWhereOpen`,
`ClipFighter.StumblingOpensGuard` with `StumbleOpensGuardHere`; `FIGHT_GOOPEN=0`, `FIGHT_STUMBLEOPEN=0`).

- The pounce's blow was aimed at the chest of a man whose board is before his chest. Now, as he throws it, chest,
  head, neck, sword shoulder, sword arm, belly and thighs are looked at in turn from his sword shoulder
  (`Openings.LookAt`) and the first with nothing before it is taken; and of the blows he can throw on the run he
  takes the one that lands soonest (`AttackOnTheMove.TakesSoonest`).
- That alone changed nothing: four of four still met the shield, now head high. **The cause:** a bash or a kick
  opened the other's guard for 0.8 s by the clock. The pounce's blow lands 1.0 to 1.6 s after the bash; by then a
  man still stumbling backward brought his board across as if he stood. Now his guard is open for as long as he
  is off balance and until he has found his feet.

| The pounce's blow, after | Old (at his chest; guard open 0.8 s) | What is open only | What is open, and a stumbling man cannot guard |
| --- | --- | --- | --- |
| a small bash | his shield | his shield | his shield (129 cm up; his guard open, the board hanging there) |
| a medium bash | his shield | his shield | his shield (he was on his feet again) |
| a hard bash (countered) | nothing | his shield | **his head** |
| a soft kick | his shield | his shield | **his head** |

- **Two of four land now; none did.** After a small or medium bash he is still stopped: honest.
- **A stumbling man cannot guard only in the duel lab and the dance.** Tried for every fighter, the fights of six
  went over their budget for a sword through its bearer's own shield: 0.82% and 0.97% of the time (seeds 5 and 7)
  against 0.00% and 0.33%, "while just struck", and the board's tremble 9.5% and 8.9% against 6.6% and 7.7%. More
  blows land on stumbling men, and a struck man's sword arm crosses his board: that fault is the hit reaction's,
  and it will show in the dance too. In the six it is as it was (the same numbers as before, to the digit).

**3. By themselves, in the dance** (Combat Feel "The Dance Pounces On An Opening", "The Dance Strikes A Man Who
Is Down", "The Dance Kicks", "Dance Kicks Instead Of Bashing"; dance panel rows "An opening (dance)", "A man down
(dance)", "Kicks (dance)" and lever "Kicks instead of bashing"; a line "By himself" for each dancer;
`Dance.Pounces`, `StrikesDowned`, `Kicks`, `KickShare`, `StepRun`; `FIGHT_DANCEOWN="1,1,1,0.35"`; report line
`DANCEOWN`).

- The running in and striking was the duel lab's own code. It is a class now, `RunIn` (in `DuelLab.cs`), and each
  dancer has one: he pounces on an opening and goes for the other when he is down, by himself. While he is going,
  his feet, his gestures and his pattern wait; his pattern begins again after.
- **Kicks by themselves:** a fighter with no shield has no bash, so a man who comes too near is kicked back out
  (a soft kick), and his guard-breaking attack is a hard kick. A fighter with a shield kicks instead of bashing 35%
  of the times: a soft kick for space, a medium kick as an attack.
- **A reading taken, written down:** he has not decided whether a fighter holds off while his enemy is down, and
  asked for fighters to do these by themselves in the dance. So in the dance it is **on** (to be seen), with its
  own toggle; everywhere else the game's own switch (off) still decides. **The fight of six is untouched.**

| Headless dances (two fighters, 45 to 70 s each) | Pounces | Kicks for space | Kicks as an attack | A man down |
| --- | --- | --- | --- | --- |
| Both with sword and shield, 45 s | 4 (4 struck at; one landed on a chest, one met a shield) | 2 soft | none | nobody fell |
| One with a sword only, one with a shield, 45 s | 2 (one landed on a head) | 1 soft | 1 hard | nobody fell |
| The same, 70 s | 1 | none | 1 hard (the sword-only man), 1 medium (the shield man: it landed, 3.3 m/s, a stumble) | nobody fell |
| Both with a sword only, 70 s | 2 | 1 soft | 1 hard | nobody fell |

- **Not exercised: going for a man who is down, in the dance.** Nobody went down in my four dances, so that one ran
  only in the duel lab (step 1 above). It is the same code.
- The hard kicks thrown as an attack from 1.7 to 1.8 m met nothing I could find a line for; the reach they are
  thrown from is now 1.6 m, not run again.
- **What it costs, as measured:** the first dance with all three on had a sword through its bearer's own shield
  0.97% of the time on his feet, against 0.56% with all three off and the guard by the clock (both over the six's
  0.5% budget, which the dance has never been held to), and the board's tremble 12.0% against 10.8%. The other
  dances: 0.73%, 0.13%, and one with no shield to measure.
- **The fights of six** (40 s, seeds 5 and 7), with everything of this entry in: crossings 0.00% and 0.33%, tremble
  6.6% and 7.7%: the same as before this entry, to the digit. Budgets kept.

**The panel:** "Look here" lists this push (14 names: drawn and framed 14 of 14 in the duel lab, whose panel has
the dance's settings; 10 of 14 in the tubes lab, the four dance rows not being drawn there).

**Not done:** the low slash; kicks at more than one height; a second blow after a pounce that is stopped; any of
it in the fight of six (on purpose: his word). **None of this has been seen in Unity.**

### 20 November, 17:05: he decides a fighter goes for a man who is down; the combos research and plan (a document, nothing built)

**His words (16:11):** "Ok all this looks fine for now. I do want a fighter to go for a man that is down." Then
he asked for a deep research document on combos, with five goals: one fluid motion (never attack, stop, attack);
each blow starting where the last one's blade ended; blows that slash through and do not stop at the part aimed
at; epic and powerful, with a real look at what fighting games do; the last blow the most powerful.

**The decision, built:** a fighter going for a man who is down is now the default in the duel lab as well as the
dance. The switch has a new name because its default changed: Combat Feel "A Fighter Goes For A Man Who Is Down"
(on; it was "A Fighter Strikes A Man Who Is Down", off). Lab row "A man who is down: he goes for him / he holds
off (old)". Not the fight of six (his word of the morning). Headless nothing changed: `FIGHT_DOWNED=1` still asks
for it, so falls and get-ups are measured on a man left alone; one run of a hard kick with it on still ends in the
stab (no exception). To watch a get-up undisturbed in Unity: "he holds off (old)".

**The document:** `Docs/Plans/2026-11-20-combos-that-flow-and-end-in-a-bang-research-and-plan.md`
(live: https://claude.ai/artifact/X5YKsPDm3LZYzefQoofnuX). Nothing of it is built. In short:

- A combo is one swing with several hits: a made attack ends at its blade's turn instead of in the guard, and the
  next starts from the blade as the last one left it. A blow on every footfall. Power 0.6, then 1.0, then 1.6; the
  last blow grows with the blows landed before it; the stab follows if he is floored. Every blow stays a real made
  attack (blocked: it bounces into another line; parried clean or struck into: the combo is over).
- Seven steps: the blade's path drawn and measured; two blows as one swing; three blows and the big one; against
  a man who defends; the dance; the show; more cuts. Each with its lab buttons and pass marks in the plan.
- From fighting games (pages read on the day, listed in the plan): a hit buys the time for the next; the recovery
  is skipped; the freeze on a hit is 0.18 to 0.25 s in Guilty Gear Xrd against our 0.04 to 0.16 s (finisher 0.13
  to 0.17 s); light then heavy; opener, links, ender and an ender that grows with the combo; the defender can break
  it. From Meyer's dusack and the sabre's moulinet: a cut ends where the next begins, and loops keep the blade
  moving.

**Measured for it** (scratch scripts, now in `Tools/Mocap/Combat/flow`; the blade worked out from the hand's bones,
so approximate; from the sampled mocap takes, not from the game):

| | |
|---|---|
| The recorded four-cut combo: hit to hit | 0.57 s, then 1.20 s (the last is a leaping chop) |
| After its first hit, the blade's point | carries on 5.5 m in 0.33 s, slowest 2.1 m/s, then lands again 0.23 s later |
| The nine recorded combos, 19 joins: hit to hit | 0.27 to 1.3 s, mostly 0.4 to 0.9 s |
| In them, the blade after a hit | carries on 2 to 9 m; slowest mostly 1.6 to 5.4 m/s (in 4 joins of 19 it nearly stops) |
| Two whole made attacks one after the other (forehand, rising) | about 1.7 s hit to hit (from the recordings' lengths) |
| Forehand's end to rising backhand's start, the hand | 32 cm (13 cm with the pack's quick forehand, not baked) |
| Rising backhand's end to the combo forehand's start, the hand | 24 cm; the point has to roll over the shoulder |
| The walk | a footfall every 0.55 s |

**Found on the way:** `sns_backhand_lunge` and `spin_backhand_mid` are the same take (he turns 396 degrees and
steps 2.4 m), and so are `sns_chop_side` and `spin_chop` (a leap with a full spin); `sns_chop_lunge` travels 3.6 m.
So the Medium knows six cuts, not eight. The old combo system (`ComboPlayer`, `StrikeCores`, `StrikeCombos`) still
runs in the fight of six and is not used by the plan: it plays the cores of recorded clips as they are (0.18 to
0.30 s before the hit to 0.18 to 0.35 s after it) and fades the pose between them. **A mistake of mine, corrected
the same evening:** I first wrote that it still cut each slash 0.06 s after the hit (`StrikeCore`'s header comment
said so; the constants had been changed after his 4 November playtest). The document, its picture, the film's note
and that comment now say what the code does.

**His log of 13:59 (Unity; 294 s: a target test, then from 42 s a dance of two sword-and-shield fighters). What he
saw before saying "all this looks fine for now":**

| In the dance, by themselves | Count |
|---|---|
| A man went down, and the other went for him | 12 of 12 |
| The stab met his body or head | 8 (chest 4, spine 1, pelvis 1, head 1, upper arm 1) |
| The stab met nothing | 3 |
| It was over before he got there | 1 |
| Pounces on a stumble or a turned back | 21: the blow met his shield 9 times, his head 2, a shin 1; over before he was near 8; could not go on 1 |
| Kick lines | 18: soft kicks by both, one medium; the kicked man thrown back stumbling |

He ran 2 to 5.4 m to a man who was down and threw the blow 0.7 to 1.75 s after setting off. Several stabs landed on
a man already rising (his chest 62 to 68 cm up). So the dance going for a downed man, which I never saw headless
(nobody fell in my four test dances), **does happen in Unity and mostly lands**. A contact sheet of two of them
(87 s and 136 s) shows the run, the crouch and the stab with the hit's flash. The frames are removed from the
repository; the text log and the per-unit files are kept.

**The panel:** "Look here" lists the one thing to see (6 names, 6 of 6 drawn and framed): a man kicked down in the
duel lab is now gone for without a button.

**Not verified:** nothing of the combos plan has been run as a combo or seen in Unity. The decision's new default
was built and compiled; the stab itself is as it was at midday.


### 20 November, 20:00: combos, step 1: the blade's path, measured in every recording and drawn in the lab (a view only)

**His word (18:42):** "Yes it sounds good. Go." So the combos plan's step 1 is built, with every default taken.
Nobody fights differently: it is two measuring tools, one for the recordings and one for the game.

**1. The recordings' blades, measured** (`BladeFlow.cs`; `Tools/Headless/flow.sh`, the runner's `flow`). For each
recorded cut, with the rig's own hold of its 0.9 m sword on the recorded hand: where the hand and the point are
against the body, how fast the point goes, the top of the swing before the landing and where the blade first turns
after it, the two corners those are in, and how far the cut is carried through. For each pair of cuts: where the
first is best left and the next best entered, and how much there is to make up there (the "bend": the gap between
the two points, between the two hands, the difference of the point's velocities and of how the body is turned,
each as the time a blade or a body would need for it; the longest counts).

| | |
|---|---|
| **The check:** the two places where the performer went straight on from one cut into the next (the four-cut combo's slices) | places 1 and 2 of 144 joins: bend 0.021 s and 0.029 s; every other join 0.055 s or more |
| Their hit to hit, as the report finds them | 0.57 s and 1.20 s: what the scratch scripts read off the raw take |
| With the Large's two-handed takes as well (361 joins) | the 4 joins where a performer went straight on are places 1, 2, 3 and 5; in the other two he did not go straight on (he left 0.10 and 0.40 s after his blade had turned), and those are left out of the check |
| Of the Medium's 144 joins | 3 join as they are (bend to 0.06 s), 31 with a bend (to 0.15 s, what the plan allows), 106 need a loop (to 0.30 s), 4 wind up again |

**The map, against the plan's first reading.** Six corners as before; two things differ.

| Cut | Starts | Ends | Carried through after the landing (recorded pace) |
|---|---|---|---|
| Forehand (`sns_forehand`; the combo's is the same) | high right | low left | 3.45 m, the blade swings 183 degrees, slowest 1.6 m/s 0.27 s after (the combo's: 5.90 m, 302 degrees, 2.5 m/s) |
| Rising backhand | low left | high right | 2.84 m, 122 degrees, 3.1 m/s |
| Flat backhand (the combo's slice) | low left | out right | 2.29 m and still at 9.5 m/s where the slice ends |
| Spinning backhands (three; the "stepping" one is the same take) | left shoulder | out right | 4.7 to 6.1 m |
| Chops made leaping (three; two are one take) | overhead | low front | 0.7 to 2.6 m: the blade stops dead, his hand at the ground |
| **The lunging chop** | overhead | **low right** (the plan had one "chop", ending low front) | 3.39 m |
| **Thrust** | high right | **high right**: the blade comes back over his sword shoulder (the plan had "high on the left") | 2.26 m |

**The joins the plan's table named, as the report has them** (recorded pace; the plan's first reading was the
hand's gap alone, from the raw takes with a guessed blade):

| First, then | Bend | Hit to hit | Between the points, the hands, the blades | The plan's first reading |
|---|---|---|---|---|
| Forehand, rising backhand (step 2's pair) | 0.11 s: joins with a bend | 0.40 s (left 0.23 s after the landing, entered 0.17 s before the next) | 64 cm, 43 cm, 16 degrees | 32 cm, joins as it is |
| Forehand, flat backhand | 0.07 s | 0.50 s | 50 cm, 29 cm, 18 degrees | 21 cm |
| Rising backhand, forehand | 0.15 s: needs a loop | 0.70 s | 1.21 m, 39 cm, 61 degrees: the point has to roll over | 24 cm, a small loop |
| Flat backhand, leaping chop (the performer's own) | 0.03 s | 1.20 s, with his loop over the head and his leap; 0.90 s is the soonest with a bend the plan allows | | a loop over the head |
| A chop, rising backhand | 0.21 to 0.28 s: needs a loop | | he ends crouched, his hand at the ground | 37 to 53 cm, joins as it is |
| Thrust, a spinning backhand | 0.11 to 0.12 s, but only entered at the spin's very start | 0.83 to 1.07 s | | 22 to 25 cm, joins |
| Thrust, forehand (not in the plan's table) | 0.10 s | 0.40 s | | |
| Forehand, rising backhand, forehand ("bam, bam, BAM") | | 0.40 + 0.70 = 1.10 s from the first hit to the third | | the plan's aim: 1.0 to 1.15 s |

So two of the plan's six joins do not hold with the rig's own blade (a chop into a rising backhand, a thrust into
a spinning backhand), and its first combo's pair holds: forehand into rising backhand needs a bend of a tenth of
a second, inside what the plan allows.

**What cost time, worth knowing:**
- A recording's root is no body frame to compare two recordings in: the bake turns each slice's root to where his
  head looked as the slice began, so the same moment of one take had its hand 108 degrees apart in two slices.
  `BladeFlow` measures against the hips themselves (the ground under them, the way they face).
- A recorded wind-up runs into its drive with no slowing (a forehand's point is at 8 to 18 m/s there, looping), so
  "where the cut starts" is not a slow moment: it is the top of the swing, the point furthest from where it lands.
  After the landing the blade does slow, and its turn is read off its speed.
- Where a cut "lands" is known no better than about 30 degrees (a flat cut's blade goes round 27 degrees in one
  frame as it lands). A join's turn of the body counts only over what a made attack's aim takes up anyway (50
  degrees, `AttackMaker.MostTurn`).
- Counting the bend as time alone made the search leave every cut a tenth of a second after it landed and enter
  every next one at the top of its swing. The bend counts ten times its length against the time
  (`BladeFlow.TimeCounts`), and a next cut is never entered after the top of its swing.

**2. The blade's path in the game** (`BladePath.cs`; drawn by `BladePathView.cs`; Combat Feel "The Blade's Path Is
Drawn", off by default; lab row "The blade's path: drawn / hidden" with the readout "His last blade path", in the
duel lab under "Strike at" and in the aim test). As a made attack is thrown its blade's path is worked out ahead
of time: a planted attack's from its clip (`MadeAttack.Blade`); a blow on the move's by doing ahead of time what
the walk does each step (the clip laid on the walk as it was, carried where the plan has the walk take him). Each
step while it plays the sword itself is read. Drawn over the game view: the plan in pale blue (thick through the
cut to where the blade turns), a white bar for the blade as it is planned to land, a yellow cross on the spot
asked for, the real path in orange with a dark dot for every step of the game's clock (dots close together: the
blade is slow). It stays until his next attack. Each finished attack writes a `bladepath`
line to the playtest log. Nothing is worked out with the switch off.

Headless (`FIGHT_BLADEPATH=1`: a `BLADEPATH` line an attack; `FIGHT_BLADEPATHSTEPS=1`: each step, plan beside
real): the aim test's 20 attacks and 2 thrown walking in, and the on-the-move battery (`mv.sh`: circling, walking
in, jogging in, backing away):

| | |
|---|---|
| The sword's point from its planned line, through the cut (22 attacks, planted and on the move) | 14 cm on average (median; mean 18 cm); at most 41 cm (median) |
| How far behind its plan it ran | 0.02 s: a physical sword follows its pose a step late, which at a blade's speed is half a metre, so the measure is "off the line", not "off at the same moment" |
| Blows on the move alone (11, of an earlier run of the day) | 10 to 23 cm on average, and one thrust thrown backing away at 34 cm: the plan worked out ahead of time is as near as a planted one's |
| Thrusts | the roughest: 14 to 47 cm on average |
| The fastest the point went as it landed | 10 to 36 m/s (a sword's limit in the game is 30); one cut made turning 4, one thrust 45 |
| **After landing on a body** (19 blows that landed, the aim test) | frozen 0.05 s every time (the hit stop, meant). Then: in 12 the point was under 5 m/s for 0.03 s or less; **in 7 it crawled under 5 m/s for a further 0.07 to 0.15 s** before sweeping on |
| **Carried on**, over the same stretch of the attack as the plan's carry-through | 16 of 19 at least 70% as far as planned (70 to 104%; the thrust further than planned); **3 well short: a forearm 45%, a shin 16%, an upper arm 10%** |

That is the line that matters for his third goal (a blow slashes through, it does not stop at the part aimed at).
By the game's own rule every one of the 19 "drove on through" the part with 70% of its speed kept. Most did, but
the sword starts again slowly after the freeze, and in a third of the blows it visibly sticks at the body for
about a tenth of a second and then whips on to catch its pose up (the point up to 1.9 m behind where its plan had
it at that moment). **The cause, traced at 20:25** (`FIGHT_BLADEPATHSTEPS=1` now prints the posed point beside
the seen one, and what solid thing of the other body the seen sword lies against): the pose does sweep on after
the freeze, at 8 to 22 m/s and within 4 to 10 cm of its plan, while the sword as seen moves at 1 to 6 m/s and
falls up to 1.9 m behind its pose. The sword is a solid thing (`UpperBodyArticulation.Hold`) and so is every part
of the struck man's body; in the blows that crawled the sword lay against his chest, spine, forearm or shin for
most of the 14 steps after the hit (4 to 14 of them), in the others for 0 to 5. So a blade the rules let through
is held by the body until it slips past. It is not seen in Unity; it is the same code there, so I expect it shows.
**Step 2 starts by mending this.**
The drawing has a dot on the orange line for every step of the game's clock, so a blade that sticks shows as dots
bunched up after the white bar, and the readout says "frozen 0.05 s, then under 5 m/s for 0.10 s more".

**A mistake of mine, corrected a quarter of an hour later (20:15):** the first push of this entry (20:00) said that five
blades "hitched" and were "carried only 10 to 20% as far as planned", and that it "may be the stand-in" because
"contact modification does nothing headless". Both were wrong. The 10 to 20% was the distance to the blade's first
slow moment, which is the hitch itself: read step by step, most of those blades swept on after it. And contact
modification in this project only softens a lying body's contact with the ground (`GroundContacts.cs`); it has
nothing to do with a blade and a body. The measure was changed to say what happens (the freeze, the crawl after
it, the carry over the plan's own stretch), and the plan's note, the briefing, the pick-up prompt and the film's
note were corrected. **A lesson: before giving a reason why a headless number might not hold in Unity, find the
code that would make it so.**

**Not checked, and said so:** the drawing itself is built blind. It uses the same lines-over-the-game-view
method as the legs' view, which he has seen working, but nobody has seen this one. The panel check passes with
the new row (`LOOK HERE (duel): 8 of 8`). A fight of six for 40 s (seed 5) with the switch off: crossings 0.00% on his feet and tremble 6.6%, both inside their budgets, no exception; it is the same fight as before by construction (with the switch off the two new calls return at once).

**Files:** new `Locomotion/BladeFlow.cs`, `Locomotion/BladePath.cs`, `Sandbox/BladePathView.cs`,
`Tools/CompileCheck/Runner/FlowReport.cs`, `Tools/Headless/flow.sh`. Touched: `ClipFighter.cs` (the path is
planned as an attack is thrown and followed at the top of each step: `PlanBladePath`, `TrackBladePath`),
`CombatFeel.cs`, `BattleSandbox.cs`, `FightReport.cs`, the runner's `Program.cs`. The scratch scripts in
`Tools/Mocap/Combat/flow` stay as the record of the plan's first reading.

**Its film** (3b2b, made by Opus at medium effort; 7 scenes, about 3.5 minutes): https://claude.ai/artifact/DicGDRVN9k8W211a5JwBj6

### 20 November, 21:15: his first look at the blade's path; the swordsman now steps back after he goes for a man

**What he said (20:44):** "After the first time I attack it seems like the blue dummy keeps walking into me. So for
all the attacks after that the dummy was standing right against me for the next attacks."

**What his two logs show** (`PlaytestLogs/2026-11-20_20-37-54` and `..._20-41-42`; frames looked at and removed):
his first forehand threw the target back stumbling; the swordsman pounced by himself, 0.66 m in, and then stood
where the pounce ended, as the duel lab's mover always has ("where it is now is its spot"). The target walked back
to its own spot, which was now 0.70 m from him. Every blow after that was asked for from 0.55 to 0.86 m, not the
1.5 m the lab sets out. It is this morning's pounce meeting the lab's old rule; the blade's path has no part in it.

**The mend** (`DuelLab.TakesDistanceAgain`, `DuelLab.StepDistance`; Combat Feel "After Going For Him He Takes His
Distance Again", on; lab row "After he goes for him: he steps back / he stands there (old)" and the readout "Their
distance"; `FIGHT_TAKESDISTANCE=0` for the old way): when a pounce or a blow at a man who is down is over, and his
order is to stand, he is given a spot as far from the other's spot as he stood before, straight back from it through
where he is (never through the man). The distance he keeps is learnt while both stand settled on their spots. He
waits for his blow to finish, and for the step his run stopped in. Once only, and again only if the other's spot
moves (a man who fell stands where he got up). An order of his own (move, leap, "Stumble him") ends it. Going round
or walking, he goes on with that order, as before. The dance is not touched.

**Measured headless** (`st.sh` with an open-handed target, a soft kick to make him stumble, then a blow asked for
6.5 s after; a sword blow does not make the headless target stumble, which it does in Unity): after the pounce the
next blow was asked for from 1.20 m with the mend (the 1.20 m he stood at), and from 1.10 m the old way (the two
spots 0.99 m apart). The headless pounce ends less close than his did (0.99 m against 0.70 m), so the old way looks
milder here than in his log. **Not seen in Unity.** Whether the step back looks natural I cannot tell.

**His logs on the blade (seen in Unity, by the numbers):** of 19 blows that landed on a body, 7 had the point stay
under 5 m/s for a further 0.08 to 0.10 s after the freeze, and several were carried through far less than planned
(0.36 m of 1.83; 0.91 m of 3.29; 1.74 m of 5.80). The same share as headless (7 of 19). So the blade sticking at
the body it cut is in Unity too. Most of these blows were thrown from much too close, so the "off its planned
path" numbers of these logs (34 to 60 cm) are not to be read; a fresh log from the right distance is wanted.

Checks: 0 errors; the panel's look list 8 of 8; `flow.sh` passes. The fight of six was not run: only the duel
lab's own code changed.

### 20 November, 21:25: his second look: the step back works in Unity, the drawing is seen, the blade sticks on half the blows

**What he said (21:22):** "They do not munch up now like it did before. The orange and blue lines are not super
close or far away from each other." With four pictures of the drawing. (Taken as: the two men no longer bunch up.)

**His log** (`PlaytestLogs/2026-11-20_21-17-58`, frames looked at and removed), **all seen in Unity:**

- **The step back works.** All 13 blows he asked for started from 1.38 to 1.64 m (before: 0.55 to 0.86 m). After a
  pounce the swordsman stands about a second with the target walking back into him (0.72 m at the nearest), then
  steps back in about 1.3 s. He waits for his blow to end first; that second could be shortened.
- **The drawing is no longer blind:** his pictures show the blue plan, the orange path with its dots, the white bar
  and the yellow cross where they should be.
- **Blue against orange, 27 blows that landed on a body:** planted forehands (15) 16 cm from the planned line
  (median), 38 cm at most, 0.01 s behind: the same as headless (14 cm). Blows on the move (12) 26 cm, 41 cm at most.
  In the pictures the orange runs wide of the blue over the top of the swing.
- **The blade sticks at the body in Unity:** 13 of 27 had the point under 5 m/s for a further 0.07 to 0.15 s after
  the freeze, and 5 of 27 were carried less than 70% as far as planned (0.48 m of 3.45; 0.96 of 3.62; 1.34 of 5.77;
  0.85 of 3.26; 1.64 of 3.13). The first forehand of each exchange is clean (frozen 0.05 s, carried 5.70 of 5.76 m);
  the blows that stick are mostly the ones thrown close in, during and after a pounce. Step 2 starts by mending it.

### 20 November, 22:08: combos, step 2: two blows as one swing; a sword passes the body it cut; he steps back sooner

He said (21:26) "sure. you can do this" to shortening the wait before the step back, and "yes please proceed with
step 2". All of this is **headless only** unless it says otherwise.

**1. A sword passes the body it cut** (`ClipFighter.LetBladePass`, `StepBladePass`; called by `StrikeHits` where a
blow "drove on through"; Combat Feel "A Blade Passes The Body It Cut", on; lab row "A blade that cut him: passes on /
sticks (old)"; `FIGHT_BLADEPASSES=0`). From the moment a blow drives on through a body, the striker's solid sword
does not touch that man's body parts; his shield and his sword still stop it. It is made solid against him again
when it has been clear of him for 0.3 s (their boxes apart), at most 1.5 s. Only where the labs and the dance say
so (`BladePassesHere`); the fights of six are as they were. `UpperBodyArticulation.Held` now says which of a body's
solid things are held in a hand.

| The aim test, 22 attacks | Sticks (old) | Passes on (new) |
| --- | --- | --- |
| Landed on a body | 19 | 17 |
| The point under 5 m/s for 0.07 s or more after the freeze | 7 | 0 |
| Carried less than 70% as far as planned | 3 | 0 |
| The point from its planned line (median; at most, median) | 17 cm; 43 cm | 12 cm; 28 cm |

**2. Two blows as one swing** (`AttackScore.EnteredFrom`, `ClipFighter.FlowInto` and `StepFlow`, `DuelLab.ThrowTwo`;
Combat Feel "A Second Blow Flows Out Of The First", on, and "The Join Bends Over" 0.12 s; lab row "Two blows: flowing
/ one after the other (old)" with the button "Throw two" and the readout "His last two blows"; `FIGHT_FLOWS=0`,
`FIGHT_THROWTWO="3,9"`, `FIGHT_FLOWBEND`, `FIGHT_FLOWBENDOVER`, `FIGHT_FLOWKEEPS`). A forehand down and a rising
backhand back up, at the chest. The forehand is an ordinary made attack. When its clip reaches the corner
`BladeFlow.Join` names (0.17 s after its landing in the recording's time; in the game about 0.3 s, for the freeze and
the 70% pace after a hit), a second made attack is begun from a score put together for it: over 0.12 s the body goes
from the forehand's own carry-through, playing on, to the rising backhand's wind-up 0.17 s before its landing, and
from there the backhand is as recorded. It is aimed, frozen on its hit and judged as any attack. A first blow that
is blocked or parried backs out as before and nothing follows.

| The plan's pass marks (duel lab, open-handed target, 7 throws standing and going round) | Mark | Measured |
| --- | --- | --- |
| First hit to second | 0.60 s or less (old way: 1.85 s) | 0.53 to 0.58 s in six, 0.67 s in one |
| The point's slowest between the hits (the freeze apart) | 2 m/s or more | 3.1 to 5.2 m/s |
| The point's travel between the hits | 0.6 m or more | 8 to 10 m |
| At the join, the posed point thrown off its course in one step | 10 cm or less | **15 to 19 cm: missed** |
| The second lands where it was aimed (the chest) | yes | **Not cleanly:** it met his right forearm or upper arm on the way in 5 of 7, 16 to 36 cm from the spot; the chest once, the head once |

What is not done, said plainly: the second blow is made as a planted blow even when he is walking (the maker for
blows on the move takes no score of this kind yet), so "a blow a step" is not there. Begun half a bend sooner, with
the blade still at 17 m/s, the join threw the point 38 cm off its course; at the corner it is 15 to 19 cm. Letting
the join carry faster parts on (`Locomotor.MostKeptAtNextSwitch`, 12 or 25 m/s) changed nothing, so the kink is not
that limit; its cause is not found. The second blow's own blade path joins `BladePath` as "sns_rising_out_of_forehand".

**3. He steps back sooner** (`DuelLab.StepsBackAfter`, 0.35 s; Combat Feel "He Steps Back This Long After His Blow
Lands"; lab lever "Steps back after"; `FIGHT_STEPSBACKAFTER=0` for the wait of 21:15): the step back does not wait
for his whole blow: 0.35 s after it lands, in the blow's own time, the rest of it is left (`Locomotor.EndShotNow`).
**Not measured:** the headless pounce run does not show the one-second wait his log showed (new and old come out
the same there, the two under 0.8 m apart from 6.6 to 8.4 s for another reason I have not traced), so this is
unverified until his next log.

Checks: 0 errors; the panel's look list 9 of 9; `flow.sh` passes; the fight of six (40 s, seed 5) within its budgets as before: CROSSINGS OK 0.00%, TREMBLE OK 6.6%, no exception.

Its film: https://claude.ai/artifact/T38Pqw9FVNzxGAafWvJvw7

### 20 November, 23:05: after his look at step 2: the join's jump, a fallen target's spot, the step back counted from rest

**What he said (22:40, 22:44):** "the 2 blows look really good. They feel like a single motion, you nailed that
part. The only issue is that the body itself seems to jump to a different pose between the 2 hits." And: "the enemy
getting stuck in my shield ... can see this in my latest logs", and to push bodies that absorb a hit up the list. He
is asleep; tonight's order is in `CLAUDE.md`.

**His logs** (`2026-11-20_22-28-11`, `..._22-34-23`; seen in Unity): 22 pairs thrown. The second blow landed in 18
and met nothing in 4; 0.47 to 0.72 s after the first (most 0.62 to 0.67); slow as it landed (2 to 13 m/s) and on his
right arm or hand more often than his chest. In Unity the first blow throws the target back stumbling at up to
2.8 m/s; headless a sword lands slower (10 against 12 to 16 m/s) and the target does not stumble
(`FIGHT_STUMBLEFROM=0.7` makes it). So in Unity the second blow chases a man who is leaving.

**1. The body's jump at the join: found, and half mended.** The maker fits a made attack to its aim (it turns the
body, drops the hips, leans the chest, moves the hand) and brings that in over the attack's wind-up. The blow entered
at the join had a wind-up of one frame, so all of it came in one step: 43 degrees of turn, hips 17 cm down, 28
degrees of lean (`FLOWMADE` and `FLOWJOIN` lines say this for every pair now). Mended: what is left of the wind-up is
no shorter than the bend (`AttackScore.JoinWindsOverBend`), and the root takes up the difference between the two
recordings' roots (`JoinTurnsRoot`; 7 degrees for this pair, so it matters little here). `FIGHT_JOINTURNS=0` for the
first build. Headless, 4 pairs: **the posed point is thrown 5 to 8 cm off its course at the join (it was 15 to 19;
the plan's mark of 10 is met)**; hit to hit 0.53 to 0.57 s. **What is left:** the maker still asks that much of the
second blow (turn 43 to 50, the most it may; drop and lean at their most), now over 0.13 s. Why is not found: from
nearer (1.15 m, 0.9 m) and at the head or the shoulder it asks the same; the rising backhand thrown alone needs
-19 degrees of turn and no drop or lean. By the join's own numbers the turn needed should be small (his root had
turned -72 degrees, the spot lay 69 to his right, the recording's root is at -79 where it is entered). Next: print
the maker's cost for each landing moment of the entered score; it lands it two frames after the recording's landing.
**So his body will still change pose quickly at the join, over an eighth of a second and not in one step. Not seen
in Unity.**

**2. A fallen target's spot follows it** (`FightLoop.DummySpotFollowsTheFall`; Combat Feel "A Fallen Target's Spot
Follows It"; `FIGHT_SPOTFOLLOWS=0`). The rule of 19 November ("a target that fell stands where it got up") sat in
the dummy's own steering, which a unit off its feet never reaches. So its spot stayed where it had stood, and once
up it walked back there. In his log of 22:34 the target got up at (0.09, 1.22) and walked 0.65 m to its old spot,
past the swordsman who had come to it, 0.7 m off, his board in its body from 28.3 to 30.6 s: "the enemy getting
stuck in my shield", and the same in the morning's "the dummy keeps walking into me". Headless (a medium kick floors
the target, `FIGHT_DOWNED=1`): up at (0.20, 3.65), it stands there; the swordsman never nearer than 1.01 m after.
**Not seen in Unity.** Two solid things 0.7 m apart will still cross; this takes away what brought them there.

**3. The step back is counted from where he comes to rest** (`DuelLab.ComesToRestUnder`, 0.8 m/s): his way back was
counted as his run was called off at 2 m/s, from a place he then ran a metre past.

Checks: 0 errors; the panel's look list 9 of 9. Not run again: the fight of six (the join and the lab's rules only;
`FightLoop`'s new lines are for target dummies).

### 20 November, 23:17: the body's jump at the join: the cause found and mended

The entry before this one said why the maker asked so much of the second blow was not found. It is found.

**The cause.** A score's tracks are in the frame the fighter faced as its clip began, with his travel and his turn
since then in them (`BodyMeasures.World`). The first build of the join (`AttackScore.EnteredFrom`) took each
recording's tracks as they were, as if they were against its root. By the corner the forehand's root has turned 68
degrees and the rising backhand's, where it is entered, 79. So the blow the maker was handed was not the blow: it
turned him 43 to 50 degrees, dropped his hips 17 cm and leaned him 28 degrees, the most it may, to bring that onto
his man. That is the pose his body jumped to (in his log's frames: crouched and folded at 140.2 s, between the hits).

**The mend** (Combat Feel "At The Join His Body Carries On", on; lab row "At the join, his body: carries on / jumps
(old)"; `FIGHT_JOINTURNS=0`): the joined score's own frame is where the blow before has him as it is left. The blow
before is put into that frame; the next is laid onto it, turned and moved so that his hips at the bend's end are
where the blow before has them and turned as it has them (for this pair: turned 18 degrees, moved 8 cm); both are
taken apart into tracks again there and bent one onto the other, the root's path with them.

| Headless, pairs standing and going round | Jumps (old) | Carries on (new) |
| --- | --- | --- |
| What the maker does to the second blow | turn 43 to 50 degrees, hips 17 cm down, lean 28, hand moved 23 to 38 cm, travel x0.30 | turn 0 to 7 degrees, hips 0 to 3 cm, lean 0 to 3, hand 2 to 13 cm, travel x1.00 |
| It lands, of its own recording | two frames late | at its recorded landing |
| First hit to second | 0.53 to 0.58 s | 0.55 to 0.58 s |
| The second blow from the spot asked for | 16 to 36 cm | 9 to 30 cm; the chest in 2 of 6, his right arm in 4 |
| At the join, the posed point off its course in a step | 15 to 19 cm (5 to 8 with the wind-up alone mended) | 12 to 13 cm |
| The point's slowest between the hits | 3.1 to 5.8 m/s | **0.4 to 2.0 m/s** |

**The blade now nearly stops at the corner** (the plan's mark is 2 m/s). The first build's blade kept moving because
it was the wrong blow. With "Join Makes Up At Most" at 0.25 (new Combat Feel lever and lab lever; default 0.15, the
plan's) the forehand is left 0.10 s after it lands, not 0.17: the point never under 4.2 m/s, 0.48 to 0.50 s hit to
hit, 11 to 13 cm from the spot, but the posed point is thrown 38 cm off its course at the join. Which reads better
is his to say; the default is the corner. With the target made to stumble as in Unity (`FIGHT_STUMBLEFROM=0.7`) the
second blow still lands, at 16 m/s, 13 to 30 cm from the spot. **Not seen in Unity.**

Checks: 0 errors; the panel's look list; `flow.sh`.

### 20 November, 23:38: a body takes a weapon's blow in its knees and trunk (bodies that absorb a hit, the blows' part)

He asked at 22:44 for "bodies that absorb a hit" to be pushed up. A first build for bashes and thrown bodies has
stood since 20 November, 01:15. What it left out is the blow that matters most now that a sword passes on through a
body: a weapon's blow pressed nobody (by the bash's rule, a press by how much his speed changes, a sword's half a
metre a second was a centimetre or nothing). A man cut across the chest stood on stiff legs with his trunk upright.

**Built** (`ClipFighter.PressByBlow`, `StepBlowPress`; Combat Feel "A Body Takes A Blow In Its Knees And Trunk", on,
with "Blow Knees Give" 0.06 m, "Blow Trunk Folds" 9 degrees, "Blow Chest Turns" 14 degrees; lab, under "Stumbles
and falls": row "A blow, his body: takes it / stands stiff (old)", three levers, and the readout "The last blow he
took"; `FIGHT_BLOWPRESS=0`, `FIGHT_BLOWPRESSED="0.06,9,14"`). A blow that lands on a body (not a shield) is weighed
by what it hands him (the hit's own count, 30 to 100 N s for a sword) against 60 N s for a Medium, and by that share:
his hips drop, his trunk folds the way the blow goes, and his chest is turned the way the blow goes round his
middle (struck on his right side going back, his right shoulder goes back). The same spring as an impact's: lowest
a contact time (0.2 s) after it lands, up again over about half a second. Nothing drawn by lot. A `took` line in the
playtest log for each.

**Measured, headless (the aim test, 18 blows that landed, 34 to 96 N s):** hips down 2.9 to 7.6 cm (median 6.5),
trunk folded 4 to 11 degrees (median 10), chest turned 1 to 14 degrees (median 5). Before: 0 to 1 cm and nothing.
The blade's own numbers are as they were (no blade crawls, 12 cm from its planned line).

**In the labs and the dance only** (the flag the labs set for a sword passing a body). On everywhere, a fight of six
(40 s, seed 5) went over both budgets: a blade through its bearer's own shield 1.12% of his time on his feet
(0.00% before), boards trembling 11.8% (6.6%): a trunk folded and turned by a blow takes the shield arm and the
sword arm with it. Kept to the labs: 0.00% and 6.6%, as before. **So before this goes into fights, what his hands
hold has to be carried through the fold and the turn** (as it is through the dip).

**Not built, of the plan's step 4:** the feet sliding a hand's width before the first step; the head lagging; the
striker's own arm taking the shock. **Not seen in Unity: how much is right is his eye's to say; the levers are there.**

Checks: 0 errors; the panel's look list 11 of 11; the six within its budgets.

### 20 November, 23:59: the second blow while he walks: tried, not done

The pair thrown while he goes round at a walking pace (1.6 m/s; at the guard's own 0.7 m/s the first blow is thrown
standing and the pair is as good as standing: the chest, 10 to 16 cm).

- **A blow over his walk can now be followed by another over the same walk** (`Locomotor.PlayOver` takes a second
  blow while the first is going, laid on against the guard the first began from; `ClipFighter.StrikeOnTheMove`
  takes a score put together for the occasion). **Off** (`ClipFighter.FlowsOverHisWalk`; `FIGHT_FLOWWALKS=1` to try
  it): thrown so, the maker turns the second blow 44 to 48 degrees and it lands 14 to 80 cm from its spot. Why is
  not found; the walk's own turning as he goes round, and how a blow on the move is fitted, are where to look.
- **Where the first blow went over his walk, the join is in the frame that blow began in** (`EnteredFrom`'s
  `overWalk`): over a walk his root is the walk's and he still faces his man, where a standing blow's root has
  turned 68 degrees. With it the maker turns the second blow 9 to 13 degrees and nothing else (it was 12 to 19
  degrees, a lean of 10 and the wrist steered 16 to 18).
- **Still wrong while he walks at 1.6 m/s:** the second blow, made standing, lands low (his hips, his thigh), 46 to
  60 cm from the chest; the posed point is thrown 30 to 34 cm off its course at the join; the first blow lands at
  3 m/s. 0.48 s hit to hit, the point never under 4.8 m/s. **So "a blow a step" is not there, and thrown while
  walking fast the pair is worse than standing.** Standing and at the guard's walk it is as the entry above says.

Checks: 0 errors; the panel's look list; `flow.sh`; the standing pair unchanged (17 and 8 cm from the spot).

**The night's film** (20 to 21 November; 3b2b, made by Opus at medium effort; 7 scenes, about 3 minutes 40): https://claude.ai/artifact/32AGujgomksYWrM7JgmTXW

### 21 November, 07:57: his morning look: he jumps in for the second blow; an attack speed slider; a stumbling target's spot; a pounce that asks for more

**What he said (07:38):** "The dummy now stands where it gets up, but when it stumbles it tries to walk back to where it
stood and then walks into me." "The jump is gone, looks good there." "Now that he pushes the enemy back he sometimes
misses the second slash because the enemy has moved back. To solve this I think you can do what they do on combos
and just make him jump, almost teleport in closer for the 2nd shot. So not if he didn't move much, but if he did
push the target back a bit then jump him in close enough for the 2nd to hit." "Have the speed of the attacks
tweakable ... right now it feels a little slow." "The attack that is triggered by the stumbles is a bit too
sensitive. The stumble should be a bit more before the stumble attack happens." And a reminder for after the combo
work: "when he runs at a downed enemy and stabs, that stab can look pretty weird ... on the cards should also be the
kick when down. I supplied you with a kick pack that contains the kick animations to be used for kicking the downed
enemy."

**His log** (`PlaytestLogs/2026-11-21_07-22-42`; frames looked at and removed; **seen in Unity**): 37 pairs thrown;
**the second blow met nothing in 20** and landed in 17 (slow, 3 to 14 m/s). 38 of 63 blows that landed threw the
target back stumbling, most at 2.8 m/s; 33 pounces. In the frames: the second blow cuts the air a metre short, then
the dummy walks back into the swordsman and stands in his shield.

**1. He jumps in for the second blow** (`ClipFighter.FlowJumpsIn`, in `StepFlow`; Combat Feel "For The Second Blow He
Jumps In", on, with "He Jumps In From" 0.2 m, "He Jumps In At Most" 1.4 m, "The Jump In Takes" 0.1 s; lab row "The
second blow: he jumps in / he stays (old)" and three levers; `FIGHT_FLOWJUMPS=0`, `FIGHT_FLOWJUMP="0.2,1.4,0.8"`). As
the second blow is begun, where its striking part would land is set against where his man will be as it lands (led
by 0.8 of how he is going). If that is more than 0.2 m out of reach, he himself is carried that far toward him over
0.1 s, evenly, and the blow is made for where that puts him. His feet are not stepped for it: a slide, as a fighting
game's combo has. Not after a blow thrown over his walk. (Tried first inside the blow's own score,
`AttackScore.EnteredFrom`'s `jump`: the maker then stretched the blow's travel as far as it may and it met nothing.
That is left in, unused.) **Headless** (`FIGHT_STUMBLEFROM=0.5`, the target going back at about 1 m/s; in Unity it
goes at up to 2.8): he jumped in 43 and 29 cm and the second blow landed at 16 m/s, 15 and 23 cm from its spot; the
maker's fit stayed light (travel x1.00). **Not seen in Unity; with a man going back three times as fast the jumps
will be longer than any I have measured.**

**2. An attack speed slider** (`AttackMaker.Pace`, `AttackStyle.AtPace`; Combat Feel "Attacks Are Thrown At", 1; lab
lever "Attack speed", 0.6 to 2, at the top of "Strike at"'s combo block; `FIGHT_ATTACKPACE`). Every weapon's blow is
made in its style with the wind-up, the drive and the way back divided by the pace, at the two doors every blow
passes (`ClipFighter.StrikeMade`, `StrikeOnTheMove`), so its landing time, its steps and the join are all counted in
the paced times. The blade lands that much faster and hits that much harder. Headless at 1.3: the second blow lands
0.18 s into its clip (0.23 at 1), 0.52 to 0.55 s hit to hit, no exception. At 1 nothing changes.

**3. A stumbling target's spot follows it** (`FightLoop.DummySpotFollowsAStumble`; Combat Feel "A Stumbling Target's
Spot Follows It"; `FIGHT_SPOTSTUMBLES=0`), and **the duel lab's swordsman takes his distance again when the other's
spot has moved** (`DuelLab.StepDistance`, `learntAt`): he steps in to the distance he kept. Headless (a soft kick):
the target stood where its stumble ended (3 cm from its spot), the swordsman stepped in to his 1.50 m.

**4. A stumble is an opening only once it has driven him far enough** (`RunIn.PounceFromStumble`, 0.6 m; Combat
Feel "A Stumble Is An Opening From"; lab lever "Pounce on a stumble of"; `FIGHT_POUNCEFROM`; 0 = any stumble, as it
was). A guard knocked open or a turned back are openings as before. **Not measured against his 33 pounces: the right
number is his eye's.**

**For later, after the combo work (his reminder):** kick a man who is down, with the kicks of the pack he supplied;
the stab at a downed man "can look pretty weird".

Checks: 0 errors; the panel's look list; `flow.sh`; the fight of six (40 s, seed 5) within its budgets as before: 0.00% and 6.6%, no exception.

**The morning's film** (21 November; 3b2b, made by Opus at medium effort; 5 scenes, about 2 minutes 45): https://claude.ai/artifact/XydAAxP3Xhn27PXaWvPYMh


### 21 November, 09:49: two attack speeds; he no longer falls after a long jump in; the panel needs no scrolling sideways

**What he said (08:44, with two pictures of the panel cut off at its right edge):** "Seems this attack speed is for
all attacks? give me a slider for the combos attack speed and one for the normal attacks. Sometimes when he leaps in
for the 2nd combo attack it causes him to fall. I want you to fix that but not in some aggressive way that disables
his physics or something. I'm getting this thing where much of the debug UI needs to be scrolled horizontally. Can
you make it so that it needs not scrolling horizontally."

**His log** (`PlaytestLogs/2026-11-21_08-33-25`; frames looked at and removed; **seen in Unity**): 39 pairs thrown,
the second blow landed in 31 (17 of 37 before the jump in). He jumped in 27 times. **The swordsman fell 3 times, each
after a jump in of 87 to 94 cm**, each "borne down, its body pressed 28 to 50 cm from where it stood". Jumps of 23 to
72 cm did not fell him. In the log's own record of his body, frame by frame: through the jump his root goes 14 cm a
frame (8.4 m/s) and his physical chest does not come with it at all: it is 12, 26, 37, 43, 48, 52 cm behind its pose
(59 and 49 or more in the other two), and still 31 cm behind a tenth of a second after the jump has ended. A chest
28 cm from its pose for a tenth of a second is what the game takes for a man borne down (`BorneDownFrom`), and down
he goes.

**Why his body stayed behind.** His physical body hangs by its hips from a carrier that goes where his posed hips
go, and that hold is by a spring no stronger than 3 times his weight (the plan "Grounded legs": so that a blow can
shift him). 3 weights is 3 g: in a tenth of a second it brings a body to 3 m/s, and the jump goes at 9.

**1. In the jump his body is carried with him** (`ClipFighter.FlowJumpCarriesBody`, `StepJumpIn`, `EndJumpIn`;
`UpperBodyArticulation.SendAll`; Combat Feel "In The Jump His Body Is Carried With Him", on; lab row "In the jump, his
body: carried / dragged (old)"; `FIGHT_FLOWJUMPCARRIES=0`). Nothing of his physics is put out, no rule is loosened,
and the borne-down rule is as it was. Two things: the jump now begins and ends gently (3u^2 - 2u^3 of its time; no
faster at its middle than half again its mean), and each step every part of his physical body is given the change
of speed his root has just had (`AddForce(.., VelocityChange)` on each link: the body goes on as one and no joint of
it is turned by it), all of it taken off again as the jump ends or is broken off. What presses on him in the jump
still presses. "His last two blows" (and the log's `flow` note) now says how far his chest got from its pose in the
jump.

**Headless could not show his fall: the cause is found.** As headless stood, a jump of 91 cm dragged the old way
left his chest 10 cm behind, not 52: the hold carried him. The headless engine read a joint's "maximum force" as an
impulse a physics step (PhysX's own default) and not as a force, as Unity does: the hold on the hips, "3 times his
weight", was in truth many times that and never gave. `PHYSX_JOINTFORCES` (`PhysXWorld.JointLimitsAreForces`) sets
the joint's flag as Unity has it. With it, and nothing else changed, the same jump leaves his chest 14, 24, 32, 39,
44, 48 cm behind, step by step: his log's 12, 26, 37, 43, 48, 52. (A first try at this seemed to show the flag made
no difference and that the hold had to be weakened to 0.5 weights as well. That was a muddle of mine: two runs
under one name, and I read the older one's file. Never start a run under a name that is still running.) **This
touches everything headless that shifts a body by its hips, back to 14 November when the hips were first held:**
blows that should move a man, stumbles, shoves. **From now on headless reads it as Unity does by
default** (`PHYSX_JOINTFORCES=0`: as it was). The fight of six with it is within both budgets (below). Headless
numbers of shoved bodies from before today were measured with the stiff hold and have not been measured again.

**Measured (headless,** `FIGHT_FLOWSHOVE=0.54 FIGHT_FLOWJUMPTRACE=1`; `FIGHT_FLOWSHOVE` puts his man further off at
the join, as a blow that drove him far would have**)**:

| the hold on the hips | the jump | his body | chest from its pose, at most | fell |
|---|---|---|---|---|
| as Unity has it (now the default), 3 weights | 91 cm | dragged (old) | 48 cm | no (over 28 cm for 0.08 s; the rule asks 0.10) |
| the same, 1.5 weights | 91 cm | dragged (old) | 58 cm | yes |
| the same, 0.5 weights | 88 cm | dragged (old) | 61 cm | yes |
| the same, 0.5 weights | 88 cm, twice | carried (new) | 13 cm | no |
| as Unity has it, 3 weights | 91 and 92 cm | carried (new) | 7 and 11 cm | no |
| as headless was (the limit an impulse) | 91 and 64 cm | dragged (old) | 10 and 7 cm | no |
| as headless was | 91 and 81 cm | carried (new) | 7 and 6 cm | no |

The second blow landed as before in every one (15 to 16 m/s). **Not seen in Unity.** A first build that set the
hips' own speed (`links[0].linearVelocity`) did nothing headless: the stand-in keeps that number and never hands it
to the engine. A change of speed on every link reaches it, and is the same thing for a body that goes as one.

**2. Two attack speeds** (`AttackMaker.ComboPace` beside `AttackMaker.Pace`; `AttackMaker.Paced(style, combo)`;
`ClipFighter.ThrowsACombo`, set about each of a pair's two blows by the lab's `ThrowTwo`/`StepTwo` and by
`StepFlow`; Combat Feel "A Combos Blows Are Thrown At", 1, beside "Attacks Are Thrown At", which is now a blow's on
its own; lab levers "Speed: single" and "Speed: combos"; `FIGHT_COMBOPACE`, `FIGHT_ATTACKPACE`). Headless, a pair at
the chest:

| | the first lands | the second lands | their clips |
|---|---|---|---|
| both at 1 (and with single at 1.3) | 0.60 s in | 0.23 s in | 1.50 and 1.08 s |
| combos at 1.3 | 0.47 s in | 0.18 s in | 1.17 and 0.83 s |

Raising the single speed left the pair as it was to the hundredth. (Hit to hit, as measured from blade on body,
moved less than that, 0.57 to 0.55 and 0.48 s in two pairs: where along its way the first blade meets the chest
varies from throw to throw.)

**3. The panel needs no scrolling sideways** (`BattleSandbox`: both scroll views have no bar across and are held at
x = 0; what is in them is laid out in a column as wide as the window less its edges and bar, `ViewEdges` 48; a row
is now a column of lines, `BeginRow`/`EndRow`, and `RowTakes` starts a new line when the next button or name would
not fit, by what its words need). Before, one row wider than the window made the whole view that wide, and every
line of text was then laid out to that width and cut off at the window's edge. **Built blind: the panel test checks
that every name is drawn and that the layout's begins and ends match, not how it looks.** What may still stick out:
a row of fixed-width buttons wider than the window (the body map's) is not wrapped, only kept from widening the rest.
The stand-in Unity got `GUIStyle.none`, `GUIStyle.CalcSize`, `GUISkin.verticalScrollbar` and the styled
`BeginScrollView`.

**Also:** `FLOWJUMP` and `JUMPTRACE` lines in the headless report (`FIGHT_FLOWJUMPTRACE=1`); `FLOWMADE` says when each
blow of a pair lands as made and at what pace.

Checks: 0 errors; the panel's look list 8 of 8; the panel test with search words (rows open and close evenly while searching);
`flow.sh` untouched. The fight of six (40 s, seed 5), today's code with headless as it was: 0.00% and 6.6%, as
before. **With the hold read as Unity has it (the new headless default): crossings 0.07% (budget 0.5%), the longest
0.05 s (budget 0.10 s); tremble 4.8% (budget 8%). That is the new baseline.** No exception in any run.

**Its film** (21 November, mid-morning; 3b2b, made by Opus at medium effort; 6 scenes, 24 beats, about 3 minutes 20): https://claude.ai/artifact/F7ew9gozgHUsen4kmxRVDk

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

**Its film:** https://claude.ai/artifact/LzVt4cUC7KmwicvM9Vo1N9

### 21 November, 15:07: combos, step 4: against a man who defends

**What he said (12:45):** "Do this and proceed": the fixes of 13:57, then on with the plan. This is the plan's step 4.
**Headless only: nothing here has been seen in Unity.**

**What was there:** a combo's blow stopped by a shield backed out and the combo was over; every blow went for the
spot asked for; a parried blow did not end a combo by any rule of its own; the dummy in the duel lab was always an
expert.

**Built** (Combat Feel switch, lab row in "Duel" > "Strike at", headless switch):

| What | Combat Feel | Lab | Headless (old way) |
|---|---|---|---|
| A blow his shield stops is not backed out of: the blocked blow is played back from where it stopped, and that bounce is the next blow's wind-up | A Blocked Blow Flows On | "A blocked blow: flows on / backs out (old)" | `FIGHT_COMBOBOUNCE=0` |
| Each blow after the first goes for the first spot of its own that his shield and arms do not cover as it begins | Each Blow Of A Combo Is Aimed At What Is Open | "Each blow goes for: what is open / the spot asked (old)" | `FIGHT_COMBOAIMS=0` |
| The big one on a shield knocks the guard open for 0.8 s: the shield arm is thrown wide (out and back, not across his body) | The Big One Knocks A Shield Open For | "The big one on a shield: knocks it open / as any blow (old)" | `FIGHT_COMBOOPENS=0` |
| A clean parry ends the combo | A Parry Ends A Combo | "A parry: ends the combo / does not (old)" | `FIGHT_COMBOPARRY=0` |
| Further off than a jump in and half a metre more, the combo ends (he does not swing at the air) | A Combo Ends When His Man Is Out Of Reach | none | `FIGHT_COMBOREACH=0` |
| The dummy: no guard, shield up, or parries; and how good he is at it | none (a lab setting) | "Dummy:", "Dummy's guard skill" | `FIGHT_AIMKIT=open\|shield\|sword`, `FIGHT_DUMMYSKILL=0.5` |

How: `ClipFighter.StepFlow` (`flowBounced`; `BladeFlow.JoinFrom` from the frame the blade stopped at;
`AttackScore.EnteredFrom(..., beforePace: FlowBouncesBack)` plays the blocked blow backward at half pace into the
next one; `FlowBounceWindsUp` 0.3 s). `FlowBlow.Spots` is each blow's own list of spots (the rising backhand low on his
sword side, the forehand and the chop over the shield, the spin low), looked at with `Openings.LookAt` from the
striker's sword shoulder. `StrikeHits.NotLiveFor`: a blow begun out of a bounce is not live for 0.15 s, or it met the
board again at once as it came away (in the first runs the "second blow" landed 0.07 s after the first, on the shield).
`StrikeHits.OpensGuardFor` and `TakeHit(..., opensGuardFor)`. `DuelLab.OtherGuardSkill`.
A hit that staggers him or makes him flinch already ends a combo (its reaction replaces the blow): not measured here,
there is no dummy that strikes back.

**Every fighter in the game is an expert with his shield** (`ShieldSkill` 1 everywhere but the walking lab). An expert
reads every blow, single or in a combo. So the step is measured against an expert and against a middling shield (0.5).

**Measured** (headless, the shield dummy, five combos or five single blows each, at his speeds: combos 1.88, single 1.15):

| | Combos with a blow on his body | Blows on his body | His guard knocked open | Floored |
|---|---|---|---|---|
| Single blows, expert shield | 0 of 5 | 0 | 0 | 0 |
| Single blows, middling shield | 0 of 5 | 0 | 0 | 0 |
| Combos the old way, expert shield | 0 of 5 (each ended at the first blocked blow) | 0 | 0 | 0 |
| Combos the old way, middling shield | 0 of 5 (the same) | 0 | 0 | 0 |
| **Combos now, expert shield** | **1 of 5** | 2 | 4 | 0 |
| **Combos now, middling shield** | **4 of 5** | 5 | 3 | 1 |
| Combos now, middling shield, all speeds at 1 | 2 of 4 | 4 | 3 | 1 |

(In the last two rows one of the five was thrown while the dummy still lay metres away from the combo before: it ended
at once, "his man is out of reach". At speed 1 it is left out of the count.)
Against the expert the three blows land on the shield in rhythm (first hit to last 0.9 to 1.1 s) and the last throws
his shield arm wide. Against the dummy who parries (an expert): 5 of 5 combos ended at the parried blow (4 by the new
rule; in 1 the blade glanced off his sword and the blow broke off by itself).

**The plan's three pass marks:**
- More combos get a blow onto the body than single attacks: **yes against a middling shield (4 of 5 against 0 of 5),
  barely against an expert (1 of 5 against 0 of 5).**
- A parried combo always ends: **yes, 5 of 5.**
- No more of the sword through his own shield than today: **not quite.** The striker's own sword through his own
  shield: none. But the dummy, struck three times instead of once, had his own blade through his own shield for
  0.05 to 0.17 s in a minute (0.09% to 0.29%; the longest 0.08 s; the budget is 0.5% and 0.10 s). The old way: 0.00 s.
  Before the shield arm was thrown wide instead of across his body it was 0.20 s at a time, over the budget.

**Not right, or not known:**
- The rising backhand aimed at a thigh or the hips often lands on the head: it is a rising cut and goes on up.
- Whether the bounce reads as one flowing swing can only be judged in Unity. In the stick figures it does.
- Only the forehand as the big one was measured five times; the chop and the spin against a shield were not.
- 2-blow combos against a shield were not measured.

Checks: 0 errors; the panel's look list 12 of 12; the fight of six (40 s, seed 5) at its baseline: crossings 0.07%, tremble 4.8%.

**Its film** (3b2b, made by Opus at medium effort; 6 scenes, 23 beats, about 3 minutes 15): https://claude.ai/artifact/GtQDpng7EsNv7xxjnJusJn

**Measured after the push (15:25; not in the film; headless, five combos each at his speeds, the middling shield):**
- The chop as the big one: 3 of 4 combos got a blow onto his body (5 blows), 2 floored him (the fifth was not
  begun: he was still down). The spin: 3 of 5 (6 blows), none floored, his guard knocked open twice.
- **The striker's own sword through his own shield, in the chop and spin combos.** Against the shield: 0.05 to
  0.07 s at a time. Against a dummy with no guard (so nothing of step 4 in it: this is steps 3 and 13:57 as pushed):
  chop 0.18 s of 30 s (0.61%, no stretch long enough to name), **spin 0.32 s of 30 s (1.04%), the longest 0.12 s:
  over both budgets (0.5%, 0.10 s).** The forehand combo: 0.00 s. So the spin, and less so the chop, pass his blade
  through his own shield, and did so before this step; I had not measured it at 13:57. Not mended. The six is not
  touched by it (no combos there).


### 21 November, 16:15: after his look at step 4: the combo that stopped, his speeds as defaults, a middling dummy; step 5 begun

**What he said (15:35):** "1) yes but it does still stop the combo many times, is this intentional? Might be good not
sure 2) Middling 3) yes. Ok so looks good. proceed but tell me what is up next and what is the roadmap looking like".
(1: the bounce reads as one flowing combo. 2: the lab's dummy is to be middling. 3: his speeds are to be the defaults.)

**His logs** (`PlaytestLogs/2026-11-21_15-21-06`, `_15-26-54`, `_15-27-30`; frames looked at and removed): 46 combos.
24 were thrown whole. 7 ended at a clean parry and 3 where the blade glanced off his sword: meant. **12 ended with
"the first blow ended before its corner" and the big one "not thrown": a bug.** Every one of them: the shield stopped
the *second* blow. And in 8 of them the first blow "met nothing" while the second was "out of the bounce": the combo
after a broken one began wrong.

**Two causes, both mended (`ClipFighter.StepFlow`, `FlowInto`, `EndFlow`):**
1. A blow his shield stops is over about two steps of the clock after its freeze (`Locomotor.CutShort`: it rewinds
   0.018 s). For a blocked first blow the bounce is taken in the step the freeze ends. For a blocked second blow the
   combo first "rolled" (the second blow became the one the third flows out of) in that step and took the bounce a
   step later: too late in Unity, every time; never too late headless. The roll now
   comes before the join in the same step.
2. `flowBounced` was left set when a combo broke off with a bounce not taken, and the next combo's second blow was
   begun "out of the bounce" in its first step, before its first blow had landed. It is cleared as a pair is opened
   and as a flow ends.
**Not confirmed:** the Unity timing cannot be made headless. Headless after it: spin combos against the expert shield,
5 of 5 thrown whole (4 of the spins out of a bounce), none "not thrown".

**His speeds are the defaults.** New Combat Feel fields (a changed default needs a new name): "A Blow On Its Own Goes
At" 1.15 (was "Attacks Are Thrown At", 1) and "A Combos Blows Go At" 1.88 (was "A Combos Blows Are Thrown At", 1).
**But a blow on its own goes at 1.15 only in the labs and the dance.** `AttackMaker.Pace` was every blow in the game:
at 1.15 the headless fight of six went from crossings 0.07% and tremble 4.8% to **0.29% and 5.9%** (one fight each;
inside the budgets of 0.5% and 8%, but worse). His rule is that the six's checks are not made worse, so:
`AttackMaker.InALab` (set by the sandbox while a lab is set out, and by the runner for the aim lineup) chooses between
`Pace` (1.15) and `FightPace` (Combat Feel "In A Battle A Blow On Its Own Goes At", 1). The six at 1: 0.07% and 4.8%
again. **His to say** whether battles go at 1.15 too.

**The lab's dummy is middling:** `DuelLab.OtherGuardSkill` 0.5, and the headless duel's too (`FIGHT_DUMMYSKILL=1` for
the expert every fighter in a battle still is).

**Step 5 of the combos plan, begun (the dance fights with combos; Combat Feel "The Dance Fights With Combos"; lab
"Combos (dance): on / off (old)"; `FIGHT_DANCECOMBOS=0`).** A first build, **not measured yet as the plan asks (four
dances of 70 s, with and without): the dances are running.** In the dance a combo starts when a dancer's light blow
lands on the other's body (he goes on out of it: `UnitController.GoOnIntoCombo`, `ClipFighter.FlowCombo(...,
firstHasLanded)`), or when the other is opened as he strikes: guard knocked open, stumbling, or in his own heavy
wind-up (`UnitController.ThrowCombo`, `Dance.OpenToCombo`). Each named dancer has his habit
(`GesturePattern.ComboBlows`, `ComboLast`, `ComboTakes`): the Strutter three blows ending in the big forehand, taking
0.9 of his chances; the Prowler three ending in the spin, 0.6; the Bull two, the second the big one, 0.5; a plain
dancer three ending in the forehand, 0.7. A `DANCECOMBOS` line for each dancer in the report; "His combos" in the
lab's readout for each dancer. One dance so far (Strutter and Prowler, 70 s, before two mistakes in it were mended: the
combos did not grow headless, and a combo's later blows went for his pattern's spot, a shin): 3 combos begun (2 into the
other's heavy wind-up, 1 out of a light blow that landed), all thrown whole, 5 blows on the body; the dance's crossings
0.69% against 0.33% without (one dance each: not a measurement).

Checks: 0 errors; the panel's look list 11 of 11; the fight of six (40 s, seed 5) at its baseline (0.07%, 4.8%).

### 21 November, 17:51: combos, step 5: the dance fights with combos

**What he said (15:35):** "Ok so looks good. proceed". This is the plan's step 5. **Headless only.**

**Built** (Combat Feel "The Dance Fights With Combos"; lab "Combos (dance): on / off (old)" and a readout "His combos"
under each dancer; `FIGHT_DANCECOMBOS=0`; a `DANCECOMBOS` line for each dancer). In the dance a fighter starts a combo
by himself:
1. **when his light blow lands on the other's body**: he goes on out of that blow into a rising backhand and, his
   habit having three, the big one (`Dance.OnHit` > `UnitController.GoOnIntoCombo` > `ClipFighter.FlowCombo(...,
   firstHasLanded)`; the landed blow counts as the combo's first, taken up a step later when the blow's own account
   of what it met is complete: `flowFirstOwed`);
2. **when the blow of his pounce lands** (a pounce is his run in at a man who stumbles, whose guard is knocked open or
   whose back is turned): the same;
3. **when the other is opened as he strikes**: stumbling, his guard knocked open, or winding up a heavy blow of his
   own. Then his blow is a combo's light first (`Dance.OpenToCombo`, `UnitController.ThrowCombo`).
A heavy blow of his own that lands does not start one. A combo goes for the chest, each later blow for what is open of
its own spots. **Each dancer has his habit** (`GesturePattern.ComboBlows`, `ComboLast`, `ComboTakes`): the Strutter
three blows ending in the big forehand, taking 0.9 of his chances; the Prowler three ending in the spin, 0.6; the Bull
two, the second the big one, 0.5; a plain dancer three ending in the forehand, 0.7.

**Measured: four dances of 70 s with combos and four without** (Strutter and Prowler, Bull and Strutter, Prowler and
Bull, two plain dancers; headless; one dance of each, and dances differ a good deal from one to the next):

| | With combos | Without |
|---|---|---|
| Combos begun | 19 (6, 4, 4, 5): 2 out of a light blow that landed, 12 out of a pounce's blow, 5 at a man opened as he struck | 0 |
| Thrown whole | 18 of 19 | |
| With a blow on his body after the first | 17 of 19 | |
| Blows landed on a body | 7 of the dance's own 46 blows, and 22 blows of combos | 8 of the dance's own 47 blows |
| Floored by a combo's last blow | 0 | |
| Men off their feet in the dance | 6 (2, 2, 1, 1) | 6 (1, 1, 2, 2) |
| His sword or shield through himself, on his feet | 0.86%, 0.22%, 0.45%, 0.73% (mean 0.57%) | 0.33%, 0.44%, 0.25%, 0.87% (mean 0.47%) |
| The shield's tremble | 15.1%, 11.8%, 12.6%, 13.0% | 14.5%, 12.0%, 12.6%, 17.9% |

About one combo every 15 seconds between the two. Nearly all of them come out of a pounce: the dancers stop most of
each other's plain blows (39 of 46 were stopped or missed), so a light blow seldom lands.

**Two combos ended with their thrower on the floor** (2 of the 12 out of a pounce, both in the first dance): he was
running at 2.2 m/s and already 0.4 m and 1.1 m from his man when the pounce's blow landed; the next blow planted his
feet and he ran on into him. The ten begun from 1.35 m and more, or standing, did not. **So a pounce's blow is not
gone on from when he is running (over 1 m/s) and within 1.25 m** (`Dance.ComboNotRunningOver`, `ComboNotWithin`).
The first dance again with that: 5 combos, all whole, all with a blow on the body (8 blows), nobody on the floor, 2
chances not taken by the rule. (Its crossings: 1.01%; the Prowler's spin, see below.)

**The plan's pass marks:** combos started and finished: 19 and 18 in four dances. The share of blows that land: 8
without, 29 with. Men floored: no more (6 and 6), and no combo's last blow floored a man. **The dance's budgets no
worse than today: not shown.** The crossings were a little worse on average (0.57% against 0.47%), but one dance
without combos was at 0.87% and one with at 0.22%: the pairings differ more than the two ways do. The dance was over
its crossings budget in 2 of 4 dances with combos and 1 of 4 without, and over its tremble budget in all eight.

**Not right, or not known:**
- The Prowler's big one is the spin, and the spin passes his own sword through his own shield (found at 15:25, not
  mended). It shows in the dance's crossings.
- Combos out of a pounce begin at close quarters and on the run: the stick figures show the two bodies overlapping
  for a moment in some of them.
- No dancer's guard was knocked open in these dances, so that way in was never taken (0 of 19).
- One dance each way per pairing is thin: a dance of 70 s takes 17 minutes.

Checks: 0 errors; the panel's look list 8 of 8; the fight of six (40 s, seed 5) at its baseline (0.07%, 4.8%).

**Its film** (3b2b, made by Opus at medium effort; 6 scenes, 24 beats, about 3 minutes 15): https://claude.ai/artifact/LEexE7y6CVHUMGC6mniyj5

### 21 November, 19:09: combos, step 6: the show (built blind)

**What he said (18:32):** "Ok I think everything looks good here. Proceed to the next task". The next task of the
combos plan is step 6. **Everything here was built blind: nothing headless can draw a streak or a spark, play a
sound, move the camera or slow the clock. Headless only counts that each cue was given.**

**His log first** (`PlaytestLogs/2026-11-21_18-22-54`, a dance of about six minutes in Unity; frames looked at and
removed): 26 combos, **25 thrown whole**, 13 of them with a blow begun out of a bounce off a shield: the bug mended at
16:15 (the combo that stopped when a shield took its second blow) is gone in Unity. The one that broke off: its first
blow met nothing. 18 of 26 put a blow on the body (28 blows); 4 men were on the floor after one. They began: 11 into
the other's heavy wind-up, 11 out of a pounce, 4 out of a light blow that landed.

**Built: six cues, each with its switch** (`ComboShow`, in `BladeStreak.cs`; lab "Duel" > "Strike at" > "The show:"
with a tick for each, "The show, all:", and the readout "The show so far"; `FIGHT_SHOW=0` or `"1,1,1,1,1,1"`):

| Cue | Combat Feel | How |
|---|---|---|
| A streak behind the blade that grows with each blow: short and pale, longer, the longest and brightest | A Combos Blade Leaves A Streak | `UnitController.StepStreak`: the walking lab's `BladeStreak` on the sword's slot, its look set by `ClipFighter.ComboRank` (1 light, 2 link, 3 big): lasts 0.1, 0.18, 0.3 s |
| Each swish a little higher than the one before | A Combos Swish Rises | `StrikeHits.SwingPitch` and `SwungPitch`; `Sfx.Play(id, at, volume, pitch)`; 0.14 of the pitch a blow (`ComboShow.SwishRises`) |
| The big one's deep hit | The Big One Lands With A Deep Hit | It was there (`SfxIds.HitShowstopper`); it has a switch now |
| The camera knocked the way the big one went | The Big One Kicks The Camera | `ComboShow.Kicked` > `OrbitCamera.Kick`: 0.22 m at the usual distance, gone in about 0.1 s, half on a shield |
| A third of a second of slow motion when a combo floors its man | A Flooring Combo Slows The Game | `ComboShow.Slowed` > `BattleSandbox.OnSlowed`: the time scale to 0.3 for 0.35 s of the real clock |
| A spark and a ring when a blow is stopped | A Stopped Blow Sparks And Rings | `HitSpark` (a dozen short bright lines, drawn by code, 0.2 s) and the parry's clang played higher: there is no ring of its own yet |

The cues are given from `ClipFighter.ComboHit` (`ComboShow.BlowMet`) and where a combo's man is first seen down
(`ComboShow.Floored`); `StrikeHits.LastPoint` is where a blow met what it met.
(The weapon's own `TrailRenderer`, `Weapon.ShowTrail`, was tried first and is not used: `UnitController.MainWeapon`
is never set, the clip fighters' blades are slots, not `Weapon`s.)

**Counted** (headless, five combos each, the defaults: combos 1.88, the dummy middling):

| Five combos against | Streaks begun (light, link, big) | Swishes pitched | Deep hits | Camera kicks | Slow beats | Sparks, rings |
|---|---|---|---|---|---|---|
| a man with no guard | 5, 5, 5 | 15 | 5 | 5 | 5 | 0, 0 |
| the shield dummy | 5, 4, 4 | 13 | 4 | 4 | 1 | 7, 7 |
| a man with no guard, the show off | 0 | 0 | 0 | 0 | 0 | 0 |

The show changes nothing in how they fight: with it on and off the same five combos landed the same 15 blows and his
own crossings were the same.

**Found on the way, not from the show:** the forehand combo against a man with no guard, at the default speeds, has
his own blade through his own shield for 0.28 s of 30 s (0.94%, over the lab's 0.5%; the longest 0.05 s), with the show
on and with it off. At 15:25 I wrote "the forehand combo: 0.00 s": that was against the shield dummy. Not mended.

**Not right, or not known:** how any of it looks or sounds. The streak's length and strength, the swish's rise, the
kick's size, the slow beat's length and the spark are first guesses. The slow beat slows sound too. The ring is a
stand-in.

Checks: 0 errors; the panel's look list 8 of 8; the fight of six (40 s, seed 5) at its baseline (0.07%, 4.8%).

**Its film** (3b2b, made by Opus at medium effort; 6 scenes, 24 beats, about 3 minutes 15): https://claude.ai/artifact/11GqpfgqDfXm7jkoN4BiZh
(The note the film was made from said the camera is kicked "a few centimetres"; it is about 18 cm at the usual distance. The film leaves the size out; the tooltip is put right.)

### 21 November, 20:08: shock waves through the body on a hit: research and plan (nothing built)

**What he said (20:01):** "Ok all this looks great [step 6, the show]. I want you to do some research and planning on
a new task now. I want you to add shock waves to the impacts ... to get some really juicy and impactful reactions ...
this does not need to have gameplay or physics implications ... fake it. So that you can have full control of how the
parts vibrate as the shock waves are sent through the body. So if the chest is hit it will be absorbing a lot of this
impact causing it to move back, dragging the upper arms, then lower arms etc as a wave travels through the body with
its peaks and valleys ... add some sliders ... for the frequency of the wave, how fast it dissipates. The wave
strength". **(20:04):** "I want this to be a bit over the top, more so than you will see in other games."

**The plan:** `Docs/Plans/2026-11-21-shock-waves-through-the-body-research-and-plan.md`; live:
https://claude.ai/artifact/FED9yrpFr7tz59LvbJLnd6 . In short: a faked wave on the pose, started at the struck part,
reaching each part after its distance along the body divided by a wave speed, each part then swinging as a dying
spring; sliders for frequency (7 a second), half-life (0.15 s), strength (9 cm for a heavy blow, about three times a
real body's 1 to 4 cm), wave speed (5 m/s), the share each joint passes on (0.75), loose ends (1.3), tilt, and one
"how over the top" dial. It goes in next to the bend (`ClipFighter.AddImpact`, `Bend`), which is already a pose-only
push that falls off with distance: the wave adds the delay and the ring. Six steps, the first one wave down one arm in
the duel lab with a "Ring him: chest" button. Must not touch: anything that decides a hit, the feet, our crossing and
tremble checks (they must read the body under the wave), the six.

**Researched** (pages opened): real flesh rings at about 15 swings a second (3 to 55), 3 mm to 4 cm, about three swings
(Schmitt and Günther 2011; Pain and Challis 2002; Play and others 2022); Smash shakes the struck fighter through the
freeze with the hit boxes kept still (Sakurai); the Unreal add-on "Easy Procedural Hit Reactions" is a falloff push
on springs with no travel; Holden's springs by half-life and frequency; Art Babbitt's successive breaking of joints.
One number (a ripple's speed in muscle, a few m/s) is from a search summary: the page did not open.

**Five questions put to him:** the wave through the freeze (recommended), the legs (fade at the shins), the attacker's
own arm on a stopped blow (yes, small), keep the bend under it (yes), how over the top to begin.

**Its film** (3b2b, made by Opus at medium effort; 8 scenes, 32 beats, about 5 minutes): https://claude.ai/artifact/5qRcjDgh6jbCCJivN8R1pK

### 21 November, 20:54: cloth that moves: samurai clothes on the fighters: research and plan (nothing built)

**What he said (20:47, with a picture of a mannequin in a kimono top, hakama and sash):** "Ok this looks great [the
shock wave plan]. But before we do this I want you to do another research and planning document for something else I
want to get in. I want them to wear clothes that are actual dynamic cloth. So you will need to model the clothes in a
way that will work well in Unity. And then I want you to implement them in Unity. ... we cant have the clothes
intersecting with the bodies. Though you may want to disable the rendering on the body parts that are completely
covered in clothes. The clothes need to swing and sway realistically. The cloths itself can be based on traditional
samurai attire like this ... the first version should have a pretty conservative mesh to test it out."

**The plan:** `Docs/Plans/2026-11-21-cloth-that-moves-samurai-clothes-research-and-plan.md`; live:
https://claude.ai/artifact/3Jev6uXAQk3Nsncq7GCtAh . In short: **our own small cloth solver** (points and threads,
position based dynamics), because it is the only kind that can be run and measured headless, and our bodies are about
fifteen rigid shapes (`UnitBuilder`: each part a box, a sphere or a capsule), the easy case. Unity's Cloth needs a
Skinned Mesh Renderer and only meets spheres and capsules listed by hand; Magica Cloth 2 and Obi Cloth need Burst and
the editor. Six safeguards against cloth in the body: pushed out of each part's shape, many small steps, a speed limit,
a leash to its place on the body, a memory of its side, covered parts hidden; and a quiet reset if it goes wrong.
Version 1: the hakama as the undivided, skirt kind; sleeves tied back; a fixed shell on the chest; clothes made by
code from flat pieces. Six steps, the first a skirt on the standing dummy with a "Shake him" button.

**Not opened (the fetch needed his approval and he was away), so from search summaries only:** Wikipedia's Kimono and
Hakama pages, Magica Cloth 2's and Obi Cloth's own pages, Ten Minute Physics lecture 14. Opened: Unity's Cloth manual,
Müller's lecture 15, Lewin's paper.

**Order now:** clothes, then the shock waves, then the combos plan's step 7. **Six questions put to him** (our own
solver or bought; the skirt kind first; sleeves tied back first; who wears them; Burst later; the order).

**Its film** (3b2b, made by Opus at medium effort; 8 scenes, 34 beats, about 5 minutes; added 21:22): https://claude.ai/artifact/TtEraxUG6Bk2VKuR9n6uiu
Checked by its maker: every beat looked at in light and dark, every number against the plan. Not tried: the voice,
the question box, saving notes.

### 21 November, 22:50: the shock wave through the body on a hit (built blind; his answers of 21:33)

**His answers (21:33), all taken:** the wave is seen in the freeze, and runs on every hit and impact; the legs as I
suggested (it fades through the thighs and stops at the shins, the feet stay planted); the attacker's own arm shudders
when his blow is stopped; the bend stays under it; "lets say 3 times" read as: the first values as planned, about three
times what a real body does (the dial "How over the top" is 1 for that, and goes up to 3 for wilder). Tonight's order:
shock waves, then clothes, then the combos plan's last step.

**What is built** (`Locomotion/ShockWave.cs`, one per fighter, `ClipFighter.Wave`):
- **Drawn only.** The wave moves the body's meshes (the "Mesh" objects under each part, and the sword's and shield's
  meshes under the hand slots) and nothing else. No collider, hit, check or physics sees it. A reading taken: the plan
  said to put it on the pose and measure how much of it the physical body shows, and to move it to the drawn body if too
  little showed; I put it straight on the drawn body, because then the checks read the body under the wave by
  construction (the plan's step 5) and the motors cannot smooth it.
- **The model as planned:** u_p(t) = A · k^j · m · exp(−(t − t_p)·ln2/h) · sin(2πf(t − t_p)), t_p = d_p / c, from the
  struck part out along the body (the distances from the rig). A heavy blow (60 N·s) moves the struck part 9 cm; a
  lighter one less; never more than 15 cm a part (all its waves together; times the dial when it is over 1).
- **Drawn as a skeleton from the hips out,** so the body stays joined: each part starts where the part before it has its
  joint and is turned (the least-squares turn over all the joints it carries: the chest carries the neck and both
  shoulders) so its far joints go where the wave puts them. The head and the hands are turned by the tilt, and take the
  sword and shield with them: the whip.
- **Every hit and impact** starts one (`TakeHit`: a blow on the body, on his shield (from the shield hand), a parry (from
  the sword hand), and a man lying down; `TakeImpact`: bashes, kicks and thrown bodies, from the shield hand or the part
  nearest the point). **The attacker's shudder:** a third as hard, at his sword hand, when a shield or a blade stops his
  blow (`StrikeHits`). **In the freeze:** the wave runs on its own clock, so it is seen while both are frozen; off, it
  waits for the freeze to end.
- **Labs and dance only** (`AttackMaker.InALab`): the fight of six has none until he says. The six's checks at seed 5:
  crossings 0.07%, tremble 4.8%, as the baseline; no wave begun.
- **Combat Feel:** "A Hit Sends A Shock Wave Through The Body", the eight sliders (frequency 7, dies away in 0.15 s,
  strength 9 cm, wave speed 5 m/s, each joint passes on 0.75, loose ends 1.3, tilt 1.5 degrees a cm, how over the top
  1), and three switches (in the freeze, the attacker's arm, the legs).
- **The lab** (duel lab, its own heading "The shock wave"): "Ring him:" chest, head, arm, shield, hips, shin (a heavy
  blow's wave with no blow); "The wave:" on / off (old); the sliders; "The wave also:" the three ticks; readouts "His
  last wave" (part by part) and "Waves so far".
- **Headless:** `FIGHT_RING="chest@2,head@4"` rings the duel's other and prints a `WAVE` line; `FIGHT_WAVE=0` turns it
  off; every report ends with a `SHOCKWAVES` line. Headless bodies have no meshes, so stand-ins are hung on each part
  for the ring to move.

**Measured headless (a ring of the chest, the default values):**

| Part | First moved | First swing | Swings | Still again |
|---|---|---|---|---|
| Chest | 0.02 s | 7.7 cm | 6 | 0.82 s |
| Head, upper arm | 0.07 s | 5.7 cm | 6 | 0.80 s |
| Forearm | 0.12 s | 4.3 cm | 5 | 0.78 s |
| Hands | 0.18 s | 3.2 cm | 5 | 0.78 s |
| Hips | 0.07 s | 4.2 cm | 5 | 0.73 s |
| Thigh | 0.10 s | 2.8 cm | 4 | 0.63 s |
| Shin | 0.18 s | 1.2 cm | 3 | 0.57 s |
| Foot | still | | | |

The sword's point swung 25 cm and the shield's rim 10 cm. Rung from the sword arm the point swung 33 cm; from the
shield, the rim 17 cm. **The drawn body is joined, but rigid parts cannot follow the wave exactly:** its joints are
within 2.7 to 5.1 cm of where the formula puts them (the head the furthest; worst before the least-squares turn, 7.7 cm,
at the shoulders and hips). In one dance of 70 s: 57 waves, 17 attackers' shudders;
the struck part swung 5.9 cm on average, 23 under 2 cm, 16 at the 15 cm cap. That dance's checks: crossings 1.01%,
tremble 15.9%, both over budget, as dances were before (0.47% to 0.57% on average for crossings at 17:51; they differ a
lot from dance to dance); the wave cannot move them (it touches no collider).

**Not seen in Unity. Not known:** whether the meshes are where I think (the "Mesh" objects under the rig's parts and
under the weapons' models: read from the prefab and `UnitBuilder`), how it looks, whether 7 swings a second reads as a
shudder or as jelly. The blade's streak follows the sword's slot, not its drawn mesh, so on a whip the streak does not
whip. Look list on the panel: "21 November, night: the shock wave".

**The shock wave's film** (3b2b, made by Opus at medium effort; 6 scenes, 24 beats, about 3 minutes): https://claude.ai/artifact/NxqQKD4EN8eZmRxQkkz9pZ

### 21 November, 23:55: clothes that move: a hakama, a top and two sleeves (built blind, headless only)

**His answers (21:33): as recommended** (our own solver, the skirt kind of hakama, sleeves tied back, one outfit in two
colours), after the shock waves. All in one build tonight: the plan's steps 1, 2 and 3, a first piece of step 4 (the
top), not step 5 (no fight of six) and not step 6.

**What is built:**
- **The solver** (`Locomotion/ClothNet.cs`): points and threads (round, down, both diagonals, and every other point to
  bend against), XPBD, 8 small steps a picture, written in plain numbers so it is cheap. **The six safeguards, all in:**
  pushed out of each part's own shape (capsule, box or sphere, read from the rig's colliders, as the shock wave draws
  them) to its surface and a 1.5 cm gap, last in every small step, twice if the first push lands it in the next part;
  the body's shapes moved on through the small steps; a speed limit of 2 cm a small step (about 10 m/s); a leash to its
  home place on the body; out the way it came in (its side); the covered parts hidden. And the net under them: a garment
  that loses a point, strays 2 m, or has a thread stretched to nine times its length is put back to hanging.
- **The garments** (`Locomotion/Clothes.cs`, one set a fighter, `UnitController.Clothes`), made by code from his rig:
  - **The hakama:** 40 columns by 16 rows (640 points), pinned at a sash ring 7 cm above his pelvis's joint, folded in
    pleats there, widening to a hem 0.84 m by 1 m across, down to about his ankles; dark blue for one side, dark red for
    the other.
  - **The sleeves:** 12 by 6 points each, a loose tube round each upper arm's own capsule from the shoulder to the
    elbow, close on the side towards his chest; pinned round the shoulder.
  - **The top:** a jacket over his chest from under the sash to his shoulders, held by the hips, the spine and the chest
    so it bends as he does, a little outside his body's shapes, with a collar. **Not simulated** (the plan's "snug
    shell"). Undyed, the sleeves too.
  - **Hidden under them:** his hips and thighs, his spine and chest, his upper arms. Drawn: head, forearms, hands, shins,
    feet.
- **Combat Feel:** "The Fighters Wear" (0 none, 1 the skirt, 2 the skirt and the top; 2 by default), "Covered Parts Are
  Hidden", and the sliders weight, stiffness, stretch, air, how much of his movement it feels (0.7), wind, gap, leash,
  small steps. **Labs and dance only.**
- **The lab** (duel lab, its own heading "Clothes"): "Clothes:" none (old) / skirt / skirt and top; **"Shake him"** (18
  seconds of moves: a walk round, a leap in, a blow, a leap out, a combo, a medium bash so the other stumbles, a hard
  kick that floors him, and his get-up); "Covered parts:" hidden / drawn; the sliders; the readout "The cloth so far".
- **Headless:** `FIGHT_CLOTHES=0|1|2`, `FIGHT_SHAKE=4` (the same run of moves; a `SHAKE` line at each), `FIGHT_CLOTHDUMP`
  (every point, `CLOTHPTS`, `BODYPTS`, `CLOTHNET` lines) and `Tools/Headless/cloth.py name [mover|other]`, which draws the
  cloth's points and the body from the front and the side. Every report ends with a `CLOTHES` line.

**Measured headless** ("Shake him" from 4 s, both men clothed, 22 s; and one dance of 20 s):

| | Shake him | The dance |
|---|---|---|
| A point of cloth inside his body (point-steps) | 0.040% | 0.21% |
| The deepest | 3.5 cm | 4.8 cm |
| Pictures with a point inside a part that is **drawn** (a shin, a forearm, the head) | 0.4% (10 of 2,642) | 1.2% (43 of 3,600) |
| Put back to hanging | 0 | 0 |
| Cost, a fighter a picture (headless, debug build) | 3.8 ms | 3.7 ms |

- **The plan's mark was no more than 1 picture in 1,000 with cloth in a drawn part: missed, 4 to 12 times over.** Most of
  it is the skirt against his shins (a kick, a fall). Inside a hidden part (the hips, thighs, chest, upper arms) cannot be
  seen.
- **Threads stretched to twice their length in most pictures** somewhere (a sleeve caught between the upper arm and the
  chest, the skirt over a knee): the cloth is pinched, not torn, and nothing exploded (no garment put back to hanging
  after I raised that net from 3 to 9 times a thread's length; before, the sleeves were put back 8 times in one run).
- **The look, drawn from the numbers** (`cloth.py`): the skirt hangs as a stiff wide bell from the sash to the floor, the
  legs inside it in his stance; in a fall it spreads on the floor and he lies in it, and it hangs again once he is up;
  the sleeves sit round the upper arms. Not seen in Unity: whether the meshes are lit and drawn as I mean (both sides
  drawn, the body's own material coloured), whether the drawn body pokes through the cloth (the cloth is kept off the
  physics shapes, which may be thinner than the drawn meshes), and whether it reads as cloth.
- **The costs:** the cloth runs in the labs and the dance only. Headless a dance runs several times slower with it on
  (`FIGHT_CLOTHES=0` for runs that do not need it). In Unity it is probably cheaper (not the debug headless build); not
  measured there.
- **Not done:** cloth against itself, the other man and his blade; the divided hakama; full wide sleeves; Large and
  Small not checked; a lost arm not checked (its sleeve is pinned to the upper arm, so it should go with a cut-off arm; not
  tried); the six.

### 21 November, 23:55: combos, step 7: more cuts (headless only, not seen in Unity)

The combos plan's last step, after the shock waves and the clothes (his order of 21:33).

- **Five cuts baked** from the packs we own (`Tools/Mocap/Combat/combat_bake.py`, the `cut_` lines; picked by the
  blade's way and height at its fastest, in the hips' frame): `cut_flat_forehand` (Mixamo "Standing Melee Attack
  Horizontal": right to left at 1.33 m, 34.5 m/s), `cut_quick_forehand` (the Pro Sword and Shield pack: down and across,
  landing 0.57 s in; baked, not used yet), `cut_flat_backhand` (left to right at the waist, 34 m/s),
  `cut_low_backhand` (across the thighs, 33.7 m/s) and `cut_leg_sweep` (at the shins; **its recorded blade is slow, 14.4
  m/s**), the last two from one take. The 112 clips already baked came out byte for byte the same. Not "sns_": nobody
  throws them on their own, only in a combo (`ClipFighter.StrikePool` lets a combo ask for one by name), and they are on
  `AttackScore.OtherBodies`, so the body's measures (and every other blow) are unchanged. The blade report
  (`Tools/Headless/flow.sh`) reads them with the rest.
- **Two new combos and a new last blow** (`FlowCombos.Kind`; Combat Feel "A Combos Cuts Are", 0 by default, as before;
  duel lab row "Combo's cuts:"): **three flat cuts** (flat forehand across his chest, flat backhand across his waist,
  the flat forehand again as the big one) and **high then low** (the forehand down from high, the low backhand across his
  thighs, the flat forehand as the big one). Chosen by the report's joins: flat forehand into flat backhand needs a
  bend of 0.12 s, back into the flat forehand 0.11; forehand into the low backhand 0.14, the low backhand into the flat
  forehand 0.15; the low backhand into the chop or the spin 0.29 (too much), so those two combos end in their own.
  **The sweep** is a fourth last blow for the plain combo ("Last blow:"; the rising backhand joins into it with 0.05 s).
- **His shield drawn aside** in the two new combos (Combat Feel "In A Flat Or Low Combo His Shield Is Drawn Aside", on;
  lab row "In those, his shield:"): the flat and low cuts sweep across where the shield is held. Down, back and a little
  out, 30 cm, while the combo lasts; his shield does not cover or intercept meanwhile.

**Measured headless, five combos each against a man with no guard, at the default speeds:**

| Combo | With a blow on him | Blows landed | Floored | His own blade through his own shield |
|---|---|---|---|---|
| Three flat cuts | 5 of 5 | 13 of 15 | 3 | 1.81% (held in cover: 2.93%) |
| High then low | 5 of 5 | 11 of 15 | 1 | 2.77% (held: 4.54%) |
| Plain, ending in the sweep | 4 of 5 | 11 | 4 | 0.28% |

First hit to last 0.8 to 1.25 s. **The two new combos are over the crossing budget (0.5%)** even with the shield drawn
aside: they are a lab choice, not the default, until that is mended (the forehand combo was over it too at 19:09,
0.94%). Not tried against a shield or a parry. Not in the dance (its patterns keep their combos).

**The fight of six after tonight's three builds** (seed 5, 40 s, the final build of 23:59): crossings 0.07%, tremble
4.8%, as the baseline; no shock wave begun, no clothes worn. None of tonight's work reaches the six.

**The night's films** (3b2b, made by Opus at medium effort): the clothes (6 scenes, 24 beats, about 3 minutes):
https://claude.ai/artifact/GA5FrDmgSqaxDYsk5eLmMq ; combos step 7 (5 scenes, 20 beats, about 3 minutes): https://claude.ai/artifact/P6fPxWrGjvhqqv5RLAbwEH . The shock wave's is above.

### 22 November, 07:20: his morning look; his own combo no longer fells him (headless can't show it)

**His words (07:12):** "Something about the combos broke and now they seem to fail and cause the attacker to fall every
time now." The shock wave "does what it was supposed to" but "looks very toy like now, instead of brutal. Lets think on
it." The clothes "just look like they are wearing Victorian dresses": he wants "a single cloth that is pinned at the
waist, open in front so that you can see the chest" (his drawing: a long robe from the shoulders to the ankles, open
down the front, a sash at the waist, long sleeves); the cloth is stiff "like paper or thick rubber", does not stop the
legs pushing through, and tanks the performance: "Is it possible to offload this to more cores?"

**His logs (06:38, 06:41, 07:00):** the swordsman fell after his own combo 1 time in 4, 1 in 21 and 1 in 2 combos, each
"borne down, its body pressed 32 to 40 cm from where it stood": in the last, the third blow of three flat cuts jumped
in 41 cm and left his chest 40 cm behind its pose, which the game takes for a man borne down. **It was not new:** the
same in 3 of 26 combos at 18:22 and 3 of 25 at 19:42 on the 21st; the new cuts, which swing his trunk further and
faster, make it likelier. Frame rate: about 41 frames a second with the clothes (07:00) against 73 the evening before;
the main thread about 5 ms longer a frame.

**Mended:** in his own combo and for 0.4 s after it, his chest left behind its pose is his own cuts and jump outrunning
his body, and it does not fell him (`ClipFighter.OwnComboNeverFellsHim`; Combat Feel "His Own Combo Never Bears Him
Down"; lab row "His own combo:" never fells him / can fell him (old); `FIGHT_OWNCOMBOFELLS=1` headless for the old way;
the report's `COMBOS so far` line counts the pictures it spared him). A man driven through by another in his combo is
not felled either in that time: the price. **Headless never showed these falls**, so the mend is unconfirmed until his
next log. The frames of the three logs were looked at (the 07:00 fall: the big flat forehand's jump, then flat on his
back) and removed.

### 22 November, 08:20: the open robe, worked out on other cores; the jolt (headless only, built blind)

**His words (07:12 and his answers at 07:2x):** the clothes looked like "Victorian dresses"; he wants "a single cloth that
is pinned at the waist, open in front so that you can see the chest" (his drawing: a long robe from the shoulders to the
ankles, open down the front, a sash); the cloth was stiff "like paper or thick rubber", let the legs through, and tanked
the frame rate: "Is it possible to offload this to more cores?" The shock wave "looks very toy like now, instead of
brutal." Answers: "Yes, build the jolt"; "No sleeves yet"; "Yes, add Burst now".

**The robe** (`Locomotion/Clothes.cs`, rewritten): one sheet of 36 columns by 24 rows (864 points), from his left front
edge round his back to his right front edge. Its collar row is held round the base of his neck in the chest's frame
(inside his shoulder joints); its seventh row is pinned by the sash in the hips' frame; the rows between ride on both
(their home blended from the chest's to the hips'). Open in front: 30 degrees each side at the collar, nearly closed at
the sash (6), opening again to 28 at the hem, so the legs come through. **Armholes:** no thread joins a front panel to the
back above the sash, so the arms come out at the sides. The cloth round the chest is a rounded square outside the chest
box's corners (an ellipse left the corners inside the cloth and pushed its points apart). A dark sash band is drawn round
the waist. Robe blue or red by side. Nothing of the body is hidden now. Softer: stiffness 0.25 (was 0.5), under new
Combat Feel names ("The Fighters Wear A Robe", "Robe Stiffness" and the rest: a changed default needs a new name).

**Legs through the cloth:** the across and down threads are kept out of every capsule of the body as threads, not only as
points (`ClothSolve.EdgeOut`): a limb cannot pass between two joined points. Two guards found on the way: an edge push next
to a pinned row threw the free point by depth / s (metres when the bone crossed near the pin): capped at the cloth's
thickness; and as the last word of each small step no thread is let past 1.6 times its length (`ClothSolve.TearsAt`).
Without them a man lying under his robe had it put back to hanging 20 times in six seconds.

**Other cores:** the sums are now one Burst-compatible core on plain numbers (`Locomotion/ClothSolve.cs`) over a packed
array (`ClothNet.cs`). In Unity each robe's picture is a Burst job (`Locomotion/ClothJob.cs`, `ClothRun`): begun after his
body has moved, taken up at the next picture, so the robe is one picture behind his body. Headless runs the same sums on
the main thread (`HEADLESS_STUB`; the stub has no jobs). Combat Feel "The Robe Is Worked Out On Other Cores" (on) and lab
row "Worked out:" on other cores / here (old). The asmdef now allows unsafe code and references Unity.Burst;
`com.unity.burst` 2.0.0 is in the manifest (it was already in the lock as a dependency). **`ClothJob.cs` is never compiled
headless: the first compile is in his Unity.**

**Measured headless (the duel lineup, "Shake him" from 4 s, 22 s; debug build, no Burst):**

| The robe | |
|---|---|
| A point inside his body | 0.050% of point-steps, the deepest 4.9 cm |
| Pictures with a thread at twice its length | 63 of 2642 (2.4%) |
| Put back to hanging | 0 (20 before the two guards) |
| Main thread, a fighter, a picture | 7.6 ms with the sums here; **0.9 ms without them** (the first clothes: about 4 ms, all of it here) |

The last line is the one that matters for his frame rate: in Unity the sums (7.1 ms here, debug and not Burst-compiled)
go to a worker thread, and what is left on the main thread is the shapes, the hand-over and the mesh (0.9 ms here). How
fast Burst makes the sums and what his frame rate becomes are **not known until he runs it**. A robe's points are seen
inside a drawn part in 14% of pictures (the front is open, nothing is hidden), mostly a centimetre or two.
`Tools/Headless/robe.py name [mover|other]` draws the robe as shaded cloth (lining pale) from his front, three-quarters
and side from `FIGHT_CLOTHDUMP`. Looked at: the open V at the chest, the sash, the legs coming through the front opening;
lying down it bunches round him.

**The jolt** (`ShockWave.Jolt`; Combat Feel "The Shock Wave Is A Jolt", on, and five levers; duel lab row "Its kind:"
jolt / jelly (old) and the jolt's sliders; `FIGHT_JOLT=0` headless for the jelly). Each part is knocked once along the
blow (x e^(1-x), peaking 0.03 s after the wave reaches it), with one small kick back, and is still in about a fifth of a
second; the wave runs at 14 m/s (was 5); his grip holds (hands 0.35 of the wave, forearms 0.68); a heavy one kicks the
camera (`ShockWave.Jolted`, heard by `OrbitCamera.Kick`). Headless, a ring at the chest:

| | Jelly (old) | Jolt |
|---|---|---|
| Chest | 7.7 cm, 6 swings, still at 0.82 s | 8.9 cm, 1 swing, still at 0.25 s |
| Hands reached at | 0.18 s | 0.07 s |
| Hands | 3.2 cm, 5 swings | 1.3 cm, 1 swing |
| Sword's point / shield's rim | 18.2 / 10.4 cm | 12.0 / 4.5 cm |

Not built of the jolt's plan: a squash of the struck part. The camera kick is not seen headless.

**The fight of six after the robe and the jolt** (seed 5, 40 s): crossings 0.07%, tremble 4.8%, as the baseline; none of it
reaches the six.

**Its film** (3b2b, made by Opus at medium effort; 6 scenes, 24 beats, about 3 minutes): https://claude.ai/artifact/4dFNQateNkfj5hViL6oZda

### 22 November, 08:55: the cloth is still not working; why, and a new plan (nothing built)

**His words (08:46, with a picture of the robe):** the cloth is still not working; instead of a robe, a long-sleeved
shirt and trousers to the feet; back to the drawing board with research on how others do it; "Cloth that does not
intersect with the body ever ... Spare no expense"; hiding covered parts may come later, but the solution should be so
robust that it is not needed. His picture: the swordsman's knee and thigh through his robe, his collar low across his
chest, the target's robe slid round with a front panel over one shoulder.

**Found: the cloth was kept out of the physics body, not the drawn one.** `Tools/Cloth/drawn_vs_physics.py` (with
`Tools/Cloth/fbxread.py`, a small binary FBX reader) measures each Medium part's drawn mesh against its physics shape
in `medium_rig.json`, in the part's own space, X mirrored as Unity imports it. 80 to 90% of the drawn surface of every
part but the hands and feet stands more than 5 mm outside its physics shape; the middle of the range 2.4 to 4.8 cm
(thigh 4.6, upper arm 3.9, chest 3.2), the far ends 6 to 16 cm (the joint pieces and mesh ends that overlap the next
part). The cloth was held 2 cm off the physics shapes, so mostly 1 to 3 cm inside what he sees. The 0.05% "inside"
the robe measured this morning was against the physics shapes and meant nothing for what is seen. Also: the cloth is
taken up a picture late since this morning's Burst job (a limb at 5 to 10 m/s moves 8 to 17 cm a picture); threads
were kept out of capsules but not boxes; the open robe is three panels held only at collar and sash.

**The plan** (`Docs/Plans/2026-11-22-clothes-that-never-go-into-the-body-research-and-plan.md`, live
https://claude.ai/artifact/NcFgFLd75rj2ZDECPKUVp1 ): bake each rigid piece's distance field from its mesh; every cloth
point has a home on a part 1.2 cm outside its drawn surface (over a joint, on the joint ball), carried with it the same
picture; a fenced wobble (max distance and backstop, as NVIDIA, Unity and Unreal cloth do; no small step longer than
the distance to the body, after OGC 2025); a last word on this picture's drawn body pushing points, thread middles and
triangle centres out; threads ≤ 2.5 cm on the limbs (a 2.5 cm thread over a 3 cm radius sags 2.7 mm). Budget: zero
inside, by two independent checks (baked fields every picture; exact mesh test on samples), on a torture rack of
10,000 poses, "Shake him", dances. Six steps; four questions wait for him. Not built.

Its film (3b2b, made by Opus at medium effort; 8 scenes, 33 beats, about 5 minutes): https://claude.ai/artifact/TSXxiad3ARayFmDX7qFWNS

### 22 November, 11:20: a shirt and trousers that never go into the body, steps 1 to 3 (headless only, built blind)

His answer to the plan at 09:21: "Soungs good yes to all. go" (never = no cloth inside the drawn parts it covers, the
body pressed into itself excepted; snug at shoulders, chest and thighs, looser at the cuffs and from the knee; keep the
sash; steps 1 to 3 together before he looks).

**What is built**
- **The drawn body, baked** (`Tools/Cloth/bake_body.py` → `Assets/_Battler/Resources/Locomotion/BodyFields_Medium.json`,
  3.3 MB, no `.meta`): each part's mesh (X mirrored as Unity imports it) as a distance grid, 8 mm cells, with the
  covered parts' triangles for an exact check. All meshes are closed. `Locomotion/BodyFields.cs` reads it; at start-up
  in Unity it compares the bake with the meshes Unity loaded (a `[Clothes]` console line; headless it cannot: the stand-in
  has no meshes).
- **Found today: the limb meshes' tops are hollow sockets** (the thigh's field is outside on its own axis from 4 cm
  below the hip joint up). A trouser leg's top could fold in through that socket, outside every part, and showed a ring
  of bare skin at the top of each thigh in every pose. Each limb's field is now joined with a ball at its joint (92% of
  the limb's narrowest top: thigh 6.6 cm, shin 4.0, upper arm 3.9, forearm 2.9).
- **The shirt and trousers, cut by code** (`Locomotion/Outfit.cs`): tubes of rings round each part's axis, each point
  1.2 cm (shirt 1.8) outside the drawn surface as it stands; shirt body, two sleeves to the wrists, trousers' waist, two
  legs to the ankles, the sash. Every point's home is on a part, or over a joint on that joint's ball (a blend of the two
  parts' turns). A trouser leg's top now stays mostly with the hips (`HipTopFollows` 0.35, to `HipBlendEnd` 14 cm down)
  so no gap opens over the hip; the waist goes down to 12 cm below the hips' centre.
- **The last word** (`Locomotion/OutfitSolve.cs`, one piece a thread in a Burst job, `OutfitJob.cs`): every point pushed
  out of the parts other pieces cover (best it can, 3 mm), then out of its own piece's parts (6 mm), summed over every
  part it is too near (out of a crease); the trousers smoothed twice and pushed out again (the crotch crumpled; the
  shirt is not smoothed: its sleeves' tops drew in off the shoulders); then up to six rounds pushing thread middles and
  face centres out.
- **Never late:** placed in `LateUpdate` (`UnitClips.LateUpdate` → `Clothes.Place`) on the body as drawn that picture;
  the job is finished there, before drawing.
- **Panel** (duel lab, "Clothes"): "Clothes:" shirt and trousers / robe (old) / none; "Shake him"; "X-ray:" (any point
  found inside drawn as a red cross through everything); "The last word:" on/off; "Off the skin", "Shirt off the skin",
  "Loose at the ends"; the readout "Cloth inside the body". Combat Feel: "What The Fighters Wear" (1) and the cloth's
  levers. Labs and dance only. The panel's look list is set for this (panel test: 10 of 10 framed).

**Measured headless** (the count is against the drawn body, joint balls included; the exact check against the mesh
triangles, every 20th picture):

| | points inside | thread middles | face centres | exact check |
|---|---|---|---|---|
| Torture rack, 1000 random poses within the joint limits | 3 of 5.6 million (deepest 0.3 mm) | 137 of 11 million | 15 of 5.4 million | 3 of 1.1 million |
| "Shake him", 22 s, the mover | 0 of 14.9 million | 128 of 29 million | 49 of 14 million | 1 of 1.5 million |

Counted apart, as agreed: cloth in a fold (an elbow or knee bent shut: 3,700 on the rack, 5,600 in the shake) and cloth
in a part another piece covers (the body pressed into itself: 45,000 and 49,000). **So not zero by the letter: a few in
a million between points, a fraction of a millimetre deep.** The robe was 1 to 3 cm inside what he saw.

Bare skin, by rays straight out from every vertex (`coverage.py`): neck, wrists and ankles by design; the thighs 1 to
12 points of about 520 (27 to 59 before the joint balls); the upper arms 3 to 58 of about 600 on the rack (the shoulder's
top in arms-overhead poses).

**Cost** (headless, one core, no Burst): 18 ms a fighter a picture in an optimised build in the shake (measured every
4th picture, as in a game), 22 ms measured every picture; the debug build 40 to 60. In Unity it runs Burst-compiled, a
piece to a worker thread (six pieces a fighter); how fast that is there is **not known**. The plan's mark for step 3
(under 1 ms a fighter on the main thread) is not shown.

**Not done or not known:** the Burst job and the start-up check have never been compiled (Unity only); the robe was not
measured against the bake (step 1 asked for it); nothing is hidden under the clothes (his "so robust we don't need to");
no wobble yet (step 4); the pictures in `$S` are the headless renderer's, not Unity's. **A lesson:** matplotlib's 3D
painted skin over cloth (it sorts whole faces); `outfit.py` now has a real depth test, and bare skin is judged by
`coverage.py`'s rays.

Its film (3b2b, made by Opus at medium effort; 6 scenes, 24 beats, about 3 minutes): https://claude.ai/artifact/Mmzde7X73orSrndiPmKXPd

### 22 November, 11:35: he could not see the clothes (mended blind)

His picture at 11:31: no clothes on either man; the readout said "none worn" while the start-up check had run ("16 parts
checked, the furthest off 0.1 mm (ThighR) - as baked": the bake matches his meshes). So the outfit was made but no
picture of it ever finished. The likely cause: the Burst job was handed the solver's settings as one struct that also
holds its arrays as pointers, and Unity refuses to schedule a job with a pointer field, so it threw every picture before
anything was drawn (headless never compiles the job). Mended: the job gets its settings as plain numbers
(`OutfitJob.cs`), and if the job ever fails, a `[Clothes] the job on other cores failed` error is logged once and the
clothes are worked out on the main thread from then on, so they are drawn anyway (`Outfit.Run`). **Not confirmed: if
they still do not show, the console's red lines are what I need.**
