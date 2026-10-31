# Physics Battler

An auto battler where fights play out physically: attacks are animated toward the real position of the body part they
aim at, and whatever the weapon or a flying body actually touches takes the hit.
Unity 6.6 (6000.6.7f1), URP, PC.

Design spec (animation-first combat): https://claude.ai/artifact/HJeFzW1bbvt7n4gsLSUoUT

The first prototype (active ragdolls with muscles and a balance assist) is kept on the branch
`backup/active-ragdoll-v1`. This branch starts over from the spec above.

## Current state: milestone 6, the fight loop

### Milestone 6: the fight loop

Press Play and the two teams fight it out with no help (`Battle/FightLoop.cs`):
- **Targets.** Each unit picks the nearest enemy that can still fight, measured by the walk there. It keeps
  the target it has, and favours one that is already hurt or already fighting it. Someone on the ground is a
  target only when no one is standing.
- **Tile to tile.** It walks across the board from tile centre to tile centre (8 ways, round others, never
  cutting their corners) to a free tile next to its target, playing its captured walk at a speed that goes
  with the square root of its size. The tiles a unit stands on and is heading for are held, so no two share
  one, and a body on the ground holds the tile it lies on. A unit knocked off its tile, or onto someone
  else's, walks to the nearest free one. After getting up it goes back onto the board.
- **On its tile** it closes in or gives ground a little (never off the tile). When its timer comes round,
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
| `Motion/` | `CombatStyle` (tuning), `BodyMeasure`, `Stance`, `CombatIdle`, `PosePipeline`, `TwoBoneIK`, `GetUp` (get-up key poses, `PoseBuffer`), `RhythmDefinition`, `PostureDefinition`, `RhythmPlayer` (Fight Rhythms) |
| `Authority/` | `AuthorityController` (levels per region, pinning) |
| `Combat/` | `WeaponDefinition`, `MoveDefinition`, `MoveState`, `MovePlanner`, `Weapon` (the weapon in the hand), `AimPart`, `DamageRules`, `BodyClearance` |
| `Units/` | `UnitController` (the per-step loop, blows, the lunge's steps), `UnitCombat` (health, poise, the weapon sweep, reactions, blocks), `UnitBalance` (stumbles, knock-downs, launches, body impacts, get-ups), `UnitRhythm` (the Fight Rhythm on the unit: steps, travel, feints, lead-ins), `UnitProfile` |
| `Board/` | `BoardGrid` (tiles), `BoardView` (the board mesh) |
| `Sandbox/` | `BattleSandbox` (spawning, panel, keys), `UnitSpawn` (the scene's markers), `Brawl` (close in and strike), `OrbitCamera`, `SandboxInput` |
| `Diagnostics/` | `PlaytestRecorder`, `UnitTrack`, `FrameCapture` |
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
dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll rhythms /tmp/rhythms   # every style's base and posture: clearance, pops
python3 ../Playtest/rhythms.py /tmp/rhythms                            # them drawn through time, side and front
dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll walks /tmp/walks       # each body walking (forward, back, aside, round), CSVs for walkview.py
AIM_STYLES=1 dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll aim       # the aim matrix with each body's own stance
dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll fight /tmp/fight 7     # a whole 3 v 3 fight (seed 7) with a playtest log; FIGHT_MIRROR=1 swaps the sides
python3 ../Playtest/fightview.py /tmp/fight/PlaytestLogs/<session> fight.mp4   # that fight as a video, seen from above
TEST_ONLY=Balance dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll      # only the tests whose name contains this
```

The stubs only cover the API this project uses, and the dry run has no collisions or joints (a ragdoll's parts fall
each on its own onto a flat ground); Unity is the final word.
