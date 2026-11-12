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
```

The stubs only cover the API this project uses, and the dry run has no collisions or joints (a ragdoll's parts fall
each on its own onto a flat ground); Unity is the final word.
