# Physics Battler

An auto battler where fights play out physically: attacks are animated toward the real position of the body part they
aim at, and whatever the weapon or a flying body actually touches takes the hit.
Unity 6.6 (6000.6.7f1), URP, PC.

Design spec (animation-first combat): https://claude.ai/artifact/HJeFzW1bbvt7n4gsLSUoUT

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

The panel has the same buttons plus a time scale slider and the recorder controls. Each unit's label
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

`Tools/CompileCheck` compiles every script against small UnityEngine/UnityEditor/URP stubs (with real vector maths),
runs the EditMode tests, and can dry-run the battle (six units on the board, with a tiny integrator standing in for
PhysX) to produce a playtest log for `analyze.py`:

```
cd Tools/CompileCheck
dotnet build Editor.csproj                     # runtime + editor scripts
dotnet build Runtime.csproj -p:LegacyInput=true
dotnet build Tests.csproj && dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll
dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll smoke /tmp/smoke   # dry run, writes a playtest log
dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll aim [tiles]         # every move at every part, every size pair
dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll swings [/tmp/gallery] # every move's arc, speed, depth, turn, steps
python3 ../Playtest/swings.py /tmp/gallery/PlaytestLogs               # review sheets of the gallery's swings
dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll balance [/tmp/getups]  # every body knocked down and got up, and a stumble
python3 ../Playtest/getups.py /tmp/getups                              # the get-ups drawn from the side and front
dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll combos                         # every body's combo strings at a standing opponent (milestone 10)
dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll rhythms /tmp/rhythms   # every style's base and posture: clearance, pops
python3 ../Playtest/rhythms.py /tmp/rhythms                            # them drawn through time, side and front
dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll walks /tmp/walks       # each body walking (forward, back, aside, round), CSVs for walkview.py
AIM_STYLES=1 dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll aim       # the aim matrix with each body's own stance
dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll fight /tmp/fight 7     # a whole 3 v 3 fight (seed 7) with a playtest log; FIGHT_MIRROR=1 swaps the sides
python3 ../Playtest/tempo.py /tmp/fight/PlaytestLogs/<session> [...]   # its rhythm and tempo (several sessions: pooled by body)
dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll windows                # every move's wind-up end, drive and contact on each body
python3 ../Playtest/fightview.py /tmp/fight/PlaytestLogs/<session> fight.mp4   # that fight as a video, seen from above
TEST_ONLY=Balance dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll      # only the tests whose name contains this
```

The stubs only cover the API this project uses, and the dry run has no collisions or joints (a ragdoll's parts fall
each on its own onto a flat ground); Unity is the final word.
