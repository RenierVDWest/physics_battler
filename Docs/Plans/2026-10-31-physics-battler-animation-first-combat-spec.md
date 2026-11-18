# Physics Battler: Animation-First Combat Spec

> Copy of the Claude document <https://claude.ai/artifact/HJeFzW1bbvt7n4gsLSUoUT>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Oct 31, 2026 · @Renier

## Summary

The new build makes animation the authority over how a unit moves, and lets physics decide what happens when weapons and bodies actually meet. Attacks are programmatic, as in Hexamus: the weapon is steered to the real position of the body part being attacked.

The first build failed because physics was the only path to the pose. Every limb chased a ghost through joint drives, and the body was held up by balance forces. When the drives lagged or the forces fought, the club fell short, the stance floated, and nothing could correct it. Six playtests in a row showed a new failure each time.

The new approach is a middle ground:

- **From Hexamus:** programmatic attacks that move the hand null, and with it the weapon's tip, edge or head, to the actual null of the targeted body part (neck, chest, elbow), at any distance.
- **From the first build:** the rig, part colliders, masses and joint limits; physical bodies that collide, recoil, ragdoll and come apart.
- **New in between:** each body part has an authority level (kinematic, pinned, loose, ragdoll). Legs are kinematic and foot-planted while standing, so they always carry the body. Arms and torso are pinned bodies that reach their pose on time but still react to blows.

Attacks reach because the weapon's path is solved against where the targeted body part was when the attack started, with a lunge to close the gap. A target that moves in the meantime may be struck somewhere else, or missed. What a blow does is physical: whatever the weapon actually strikes takes the hit, intended target or not. The contact's speed, the weapon's mass and the body part struck decide damage, recoil, knockdown or a severed limb.

## How other games solve this

Shipped combat games almost all keep animation in charge and add physics on top, per bone and for a limited time. Fully physics-driven characters (Gang Beasts, TABS, Half Sword) exist, but their wobble is the point of those games, which is what we want to avoid.

| Game or tech | How animation and physics share the body | What we take |
| --- | --- | --- |
| [Unreal physics-driven animation](https://dev.epicgames.com/documentation/en-us/unreal-engine/physics-driven-animation-in-unreal-engine) | Simulation is switched on from a bone down its chain, and a per-chain physics blend weight (0 = keyframes, 1 = physics) is ramped up on a hit and back down | Per-region authority with a weight that ramps in and out; hit reactions as short partial ragdolls |
| [Unreal motion warping](https://dev.epicgames.com/documentation/en-us/unreal-engine/motion-warping-in-unreal-engine) | Warp windows in an attack's timeline adjust root motion so the character's position and rotation match a named target by the window's end | Warp windows in our moves: close the distance and turn to face during the wind-up |
| [Just Cause 2 (Avalanche)](https://www.sambuz.com/doc/physics-ics-meets-s-anim-imation-ion-charac-racter-ter-ppt-presentation-926923) | Root motion decoupled from posture; ragdolls driven toward animation poses by motors or impulses; get-up by driving the ragdoll to the nearest get-up start pose, then blending; animation events fire physical impulses; a layered pose pipeline (ragdoll, IK, aim) | Pose-matched get-ups; impulses fired from move events; a fixed layer order |
| [PuppetMaster for Unity](https://discussions.unity.com/t/puppetmaster-advanced-character-physics-tool-released/599766?page=40) | Each body has muscle drives toward the animation plus pinning, which sets the body's velocity toward its animated position (pin weight 1 arrives in one step); behaviours switch between puppet, unpinned (knocked down) and recovery | Velocity pinning, so pinned limbs arrive on time instead of lagging behind drives; the puppet / unpinned / recover states |
| [DReCon (Ubisoft La Forge)](https://www.ubisoft.com/en-us/studio/laforge/news/VjEIwquaIyEZZSw5RZI0V/drecon-datadriven-responsive-control-of-physicsbased-characters) | A kinematic controller produces the target motion; a learned policy corrects the physical character's drive targets so it can balance. It names our exact failure: forces as puppet strings cause floating and foot sliding | Confirmation that physical balance needs either learning or very heavy engineering; we keep the kinematic target and skip the learned balance |
| [Blending ragdoll back to animation](https://www.moddb.com/news/blending-ragdoll-physics-and-animation-in-unity) | Face-up or face-down picked from the pelvis's up vector, then each bone blended from the ragdoll pose to the chosen get-up animation | The get-up selection and per-part blend |
| [Overgrowth (Wolfire, GDC 2014)](https://gdcvault.com/play/1020049/Animation-Bootcamp-An-Indie-Approach) | Interactive, fluid animation from very few keyframes plus simple procedural layers | Few-key authored moves with procedural layers on top, which suits generated clips |
| Hexamus (yours) | Kinematic motion assets: a hand path, speed and body curves; a hit when the hand passes closest to the target; the rules decide and the body shows it; physics only on death and knockdown | Contact-driven hits and "rules decide, body shows"; the motion asset ideas carry over to move assets |

The common pattern: the pose comes from animation, gameplay owns outcomes, and physics is a weighted layer per body region that rises on impact and falls back.

## Principles: who decides what

Animation owns the pose and gameplay owns the intent, but physics owns contact: what a weapon or a flung body actually strikes is a real outcome, fed through the damage rules.

| Question | Owner | Physics' part |
| --- | --- | --- |
| Where a unit stands | Gameplay, on the 8 by 8 grid | A knockback or ragdoll can carry a body off its tile; it is re-seated on the nearest free tile when it gets up |
| Whether a unit is standing | Gameplay (poise and reaction state) | Physical hits deal the poise damage that knocks it down |
| What pose a unit is in | Programmatic moves and clips, then IK layers | Lag, follow-through and recoil on pinned parts |
| What an attack aims at | Gameplay picks the target unit and body part; the move steers the weapon to it | None |
| What a blow hits | Physics: whatever the weapon touches during the move's active window | All of it; stray hits count |
| How much a blow does | Damage rules, fed by the contact (weapon, speed, body part, edge, point or blunt) | The contact data |
| How a hit unit moves | Reaction state | The motion of loose and ragdolled parts |
| Bodies striking bodies | Damage rules, fed by impact speed and mass | The collision |

Five rules follow from this:

1. **The pose is always valid without physics.** Switch every body to kinematic and units still stand, walk and swing correctly. Physics adds to that; it never holds anything up.
2. **Physics authority is per region, weighted and temporary.** It ramps up on an event (a hit, a clash, a knockdown) and back down to the move's level.
3. **Blows are aimed, not hoped for.** The weapon is steered to where the target's body part really was when the attack started, so the blow arrives there unless the target has moved or something physically gets in the way.
4. **Contacts are outcomes.** Every weapon contact during an active window, and every hard body-to-body impact, goes through the same damage rules. Fights won't replay identically, and that's accepted.
5. **Everything is observable.** The playtest log records the aimed pose, the physical pose, the authority per region, and every contact with what it did.

## Architecture

Each unit runs one fixed pipeline per physics step: gameplay picks the target and body part, the move and grounding layers build a pose aimed at that part's real position, the authority layer sets how much physics may bend it, and PhysX steps. Every contact PhysX reports goes back to the damage rules.

&#91;embedded content: per-step pipeline · gameplay plus 4 layers\]

Steps 1 to 3 are deterministic and testable without Unity, so the aim can be proven before a playtest. Step 4 only moves what step 3 allows, but whatever it touches counts.

## Attacks: aimed at the real body part

Attacks are programmatic, as in Hexamus: when an attack starts, it locks the current position of the chosen body part's null on the target, and the move steers the weapon to that point. Distance doesn't change the look of the blow, because a lunge closes the gap.

### The move asset

A `MoveDefinition` describes a blow relative to the point it aims at, not as a fixed animation:

- **Aim:** which body part null to strike (head, neck, chest, elbow, knee) and with what part of the weapon: the tip (thrust), the edge (cut) or the head (club).
- **Path:** the points the weapon's striking part passes through: wind-up, approach, the aim point itself, and the follow-through. They are set in an aim frame whose origin is the target null and whose axes come from the strike direction, so one move works at any distance, height or angle.
- **Timing:** a speed curve along the path (slow wind-up, fast strike, eased follow-through) and markers: commit point, active window, recovery, cancel window.
- **Body:** lunge, lean, twist and crouch curves as in Hexamus, and what the off hand does: rest, raise a shield, or grip the same weapon.
- **Cut line** (edged weapons): for a sever, the edge sweeps across the limb at the joint, square to the bone.
- **Authority curves** per body region, and **events** (hit stop, shake, sound) at marker times.

### Solving each step

1. **Choose.** Gameplay picks the target unit, the body part and a move that can reach it: arm reach plus the move's longest lunge. Out of reach means stepping closer on the grid first.
2. **Lock the aim.** The moment the attack starts, the aim null's current position is taken and fixed in the world. The blow goes to that point, not to where the body part will be when it arrives.
3. **Lunge and turn.** The root moves and turns so the locked point is inside the arm's reach at contact. The lunge is as long as the gap needs, up to the move's limit (about one tile).
4. **Steer the weapon.** Sample the path by the speed curve. Two-bone IK puts the hand where the weapon's striking part lies on the path, with the hand turned so the edge or tip leads. Spine and shoulder twist help when the arm alone can't reach.
5. **Land where it lands.** If the target swayed, ducked or stepped during the wind-up, the weapon still goes to the locked point and strikes whatever is there now: another body part, another unit, or nothing. This is why combat idle matters (see *Combat idle and footwork*).

### What the blow hits

During the active window the weapon is a pinned physical body, and every contact it makes with any unit is a hit:

- **The intended target:** if the target held still, the path runs through its aim null and the weapon meets it on the contact frame. If a fast weapon passes through without a collision registering, a sweep along that step's weapon path still catches it.
- **Something in the way:** another unit, a raised arm, a shield. That body takes the blow. A heavy blocker stops the swing, and the move goes to a recoil. A light one is bowled through, and the swing carries on to the target with less force.
- **What decides the result:** the body part struck, the weapon's speed at the contact point, its mass and kind (edge, point or blunt), and the angle. The damage rules turn these into damage, poise damage, a reaction and possibly a sever.
- **Feel:** hit stop freezes the attacker and every unit struck for 60 to 100 ms, and the contact's impulse goes into the part struck.

### Weight and speed

Weapon weight and strength matter as tuning, not simulation. A move's wind-up and recovery are time-scaled by the weapon's mass against the wielder's strength. A heavier weapon also lowers the weapon arm's pin weight, so it lags and follows through more. Attack speed (an auto battler stat) scales the whole move.

### Clashes, blocks and parries

- **Clash:** a weapon meeting a wall, a shield or another weapon during the active window is a contact too; the attacker's move branches to a recoil with hit stop.
- **Shield block:** the off hand raises the shield, a pinned body, along the predicted strike line. Whatever hits it is blocked physically.
- **Parry:** a parry is itself an aimed move whose aim point is the attacker's weapon at the attacker's contact time, so the weapons visibly meet. Contact inside its guard window turns the attack into a recoil.

## Combat idle and footwork

Units never stand still in a fight, because an attack goes to where a body part was when it started. Every bit of movement between the start of a blow and its arrival changes what it strikes, so how a unit moves while idle is part of its fighting style.

- **Combat idle:** a procedural layer keeps the body moving: sway, bob, weaving of the head and chest, weight shifting between the feet, small shuffles. It moves the aim nulls, so a blow started a moment ago may now land on a shoulder instead of the chest.
- **Feints:** short fake-out moves (a half step in, a dip, a shoulder drop) that invite an attack and then move the body away from it.
- **Reactive dodges:** a unit that sees an attack start can respond after its reaction time: a lean, a duck, a sidestep or a step back. It either avoids the blow or makes it land on a different part.
- **Styles:** each unit type has a combat style asset: how much and how fast it sways, how often it feints, its reaction time, how likely it is to dodge and which way. A big unit might barely move and take blows; a small one might weave constantly.
- **Testing it:** the log records, for every blow, the part aimed at and the part actually struck, so styles can be compared by how often they turn a chest hit into an arm hit or a miss.

## Standing and moving

The legs carry the body because the pose is built that way, not because forces hold it up. The feet are planted by IK, the pelvis sits where the legs can reach, and weight is shown through timing and shifts.

### Stance

- **An authored idle stance**: feet apart and staggered (one forward, one back), knees bent, a slow breathing cycle. Each unit type can have its own.
- **Foot planting**: while a foot is down, it is locked to its ground point and IK bends the leg to reach it. Clips carry a foot-contact curve per foot (measured on import from the foot's height and speed, or keyed by hand).
- **Pelvis height**: if a planted foot can't be reached, the pelvis is lowered until it can, so a leg is never stretched straight or left dangling.
- **Kinematic legs**: while a unit is on its feet, its pelvis and legs are kinematic bodies moved exactly to the pose. They still push props and other loose bodies, but nothing pushes them.

### Showing weight

The first build tried to get weight from simulation. Here it comes from the pose:

- **Weight shift**: a procedural layer moves the pelvis so the centre of mass (from the animated pose, the part masses and the weapon) stays over the support polygon between the feet. Reaching out with the club pulls the hips back over the back foot. The support polygon code from the first build is reused.
- **Timing**: anticipation in the wind-up, a heavy contact, follow-through and a slow recovery, all authored and scaled by weapon weight.
- **Plant and settle**: a small pelvis dip when a foot lands or a blow lands.
- **Lag**: pinned arms and the weapon trail their pose slightly and overshoot when stopped, which reads as mass.

### Stepping and turning

When a lunge, a turn or a shove moves the body too far from its feet, a procedural step re-plants the foot: it lifts, swings along an arc to its new stance spot, and plants. A step is triggered when a foot is more than about 0.25 m from its stance spot, or the body has turned more than about 30° over it. The stance and step-target code from the first build is reused; only the physics goes.

### Locomotion

- **An 8 by 8 grid** (for now; it may change): gameplay moves units from tile to tile, and the root follows a smooth path between tile centres. Animation follows the root.
- **Walk and run clips** play at a rate of actual speed over the clip's authored speed, so feet don't slide; foot IK cleans up the rest.
- **Moves carry their own root motion:** lunges, dodges and staggers move the root within the tile or onto a free neighbouring tile.
- **Starting, stopping and turning on the spot** use short transition steps; a clip set per unit type.

## Physics authority and reactions

Every body part runs at one of four authority levels, set per region by the current move or reaction and blended over 0.1 to 0.3 s. Physics takes over a region when something hits it and hands it back on a timer, and every contact it reports can be a hit.

### The four levels

| Level | How the part moves | Used for |
| --- | --- | --- |
| Kinematic | Moved exactly to the animated pose each physics step; pushes others, isn't pushed | Pelvis and legs while on their feet |
| Pinned | Dynamic; its velocity is set toward the animated pose each step (pin weight 0 to 1, as PuppetMaster does), with joint drives for rotation | Torso, head and arms during moves; the weapon arm |
| Loose | Dynamic; joint drives only, weak or no pin | A region just hit (flinch), arms in a stagger |
| Ragdoll | Dynamic; limits only, drives near zero | Knockdowns, launches, death |

Pinning is the key change from the first build. Drives alone lagged behind the ghost and fell short. Pinning sets the velocity needed to arrive this step, so a pin weight of 1 lands on the pose on time. A weight below 1 gives lag and overshoot that reads as mass. Pinned parts hang off kinematic parents, so there is no balance to lose.

### Regions

Five regions, each with its own weight curve: **legs and pelvis**, **torso and head**, **weapon arm**, **off arm**, and the **weapon**. The weapon is fixed to the hand, so it follows the weapon arm's level.

### Reactions

There are no hit animations: every reaction is the body recoiling as physics objects. Gameplay only picks how far the physics may go, from damage against poise. Size matters on its own, because the same impulse moves a 15 kg unit about 19 times as much as a 280 kg one:

| Reaction | What moves the body | Gameplay |
| --- | --- | --- |
| Recoil (every hit) | The struck part goes loose and takes the impulse; the parts joined to it follow through their joints, less the further away they are, as the rig absorbs the shock. Pins ramp back over 0.3 to 0.6 s | No interrupt unless the blow is hard |
| Knocked back | What the struck part doesn't absorb moves the whole unit: the root gets the remaining impulse divided by the unit's mass, and the legs take procedural catch steps along it | The current move is cancelled |
| Knockdown | Full ragdoll with the impulse | Down for a set time |
| Launch | Ragdoll with a large impulse; it can strike other units, which take damage and react too | Big hits only |

### Getting up

1. Wait for the ragdoll to settle (slow pelvis, low speed for about 0.3 s), or for the down timer.
2. Pick a get-up clip by which way the pelvis faces: face up or face down.
3. Move the unit's root under the ragdoll's pelvis, facing along its body.
4. Blend each part from its ragdoll pose to the get-up clip's first frame over about 0.3 s, legs first, then play the clip with the legs kinematic again.

### Collisions

- **Own body:** pinned arms collide with the torso. The weapon never collides with its own wielder; the first build showed that this traps it. Ragdolls collide with everything.
- **Other units:** units are kept apart by gameplay steering. Kinematic parts of two units don't collide with each other; pinned, loose and ragdoll parts do.
- **Props:** crates and barrels collide with every level, as now.

### Hits from physics

- **Weapon contacts:** every unit the weapon touches during an active window takes a hit (see *What the blow hits*).
- **Body impacts:** a launched or falling body that strikes another unit hard deals impact damage and poise damage to both, scaled by relative speed and mass. Above the poise limit the second unit goes down too, so one big hit can bowl over a line.
- **Thresholds:** brushing contacts and slow pushes do nothing (below about 3 m/s for bodies, below the weapon's minimum hit speed for weapons), and each pair of bodies has a short cooldown so one collision isn't counted twice.

### Severing limbs

- **When:** any limb can come off. An edged weapon severs at a joint (shoulder, elbow, wrist, hip, knee, ankle, neck) when its edge meets the limb within about 5 cm of the joint null, moving across the bone, faster than the weapon's sever speed, and with enough damage to that part.
- **What happens:** the joint breaks and the limb leaves the unit as a loose physics object with its own collider and mass. The unit loses what the limb did: its weapon if it was the weapon arm, its footing if it was a leg.
- **On purpose:** a move can take a joint as its aim point, so a deliberate sever is a programmatic cut through the real elbow null, like the decapitations in Hexamus.
- **Slow blades and blunt weapons** don't sever: a slow edge only damages the limb, and a club knocks it away and can leave it loose and useless for a while. Clubs never sever, for now.

### Back onto the grid

A knocked-down or launched body can come to rest anywhere. While down, it occupies the tile it lies mostly on, so others path around it. When it gets up, it takes the nearest free tile to where its pelvis lies.

## Authoring motion

Attacks and hit reactions are programmatic move assets, as in Hexamus. Clips are used only for motion that doesn't aim at anything: idle stances, walking and running, and get-ups.

### Programmatic moves

- **What they cover:** attacks, parries, blocks, dodges, feints and combat idles. Each is a move asset (see *The move asset*) with a path in the target's aim frame, curves and markers, editable in the inspector while the game runs.
- **The first set:** I write them, porting what works from Hexamus's motion assets: club swing and overhead smash, sword cut and thrust, a cut aimed at a joint, shield block, a dodge and a combat idle per size.
- **No hit animations:** a struck unit recoils as physics objects (see Reactions); only its catch steps are procedural.

### Clips

Idle stances, walk and run cycles, and get-ups are ordinary Unity clips on the null rig. I generate the first ones from a few key poses. You can edit them in Unity's Animation window, or replace them with clips animated in Softimage or Houdini on a reference rig FBX that setup exports, as Hexamus does for character parts.

### Order each physics step

1. **Base:** idle or locomotion clip, blended by speed.
2. **Move:** the programmatic move: lunge and body curves, then the weapon path solved with arm IK onto the aim point.
3. **Combat idle:** sway, bob, weave, shuffles and feints, layered on top.
4. **Grounding:** weight shift, foot IK and steps.
5. **Physics targets:** the result sets the kinematic parts and the pin targets, then PhysX steps.

Our own code runs these in a fixed order, not an Animator's own update, so the same logic runs in tests without Unity's player loop.

### Move preview

An editor window plays a move against a dummy target at a chosen distance, height and body part. It scrubs through the move and draws the weapon's path, the aim null, the lunge and the reach, and reports how close the striking part passed to the aim point. It does for moves what *Draw Attack Paths* does in Hexamus.

## What we keep and what we drop

About half the first build carries over: the rig, the setup pipeline, the tools and most of the geometry code. What goes is everything that held the body up or drove the swing through physics.

| Part | Fate | Why |
| --- | --- | --- |
| Null rig, part list, colliders, masses, joint limits (`UnitBuilder`, rig file) | Keep | You said the rig and colliders are good; they become the physics bodies, and joints become breakable for severs |
| Project setup, setup versions and migrations, prefab and scene builders | Keep | Same pipeline, new assets |
| Playtest recorder, events log, pictures, `analyze.py`, compile and smoke harness | Keep, extend | New columns: authority per region, aim error, every contact and what it did |
| `TwoBoneIK`, `WeaponGrip`, swing plane maths (`SwingSolver`) | Keep | Used to steer the weapon onto the path |
| `Stance` foot spots and step targets, `SupportPolygon` | Keep | Used kinematically for stepping and the weight shift |
| Joint drive maths (`JointMath.DriveGains`, `JointConventions`) | Keep, smaller role | Rotation drives for pinned parts |
| Hexamus motion assets (`AttackMotion`, `AttackPlayer`) | Port | Paths, speed and body curves, off-hand modes; re-based on the target's body part null |
| Impact system, combat events | Rework | Every weapon and body contact becomes a hit through the damage rules |
| Sandbox | Rework | An 8 by 8 board with two teams |
| `BalanceController`, upright anchors, vertical and horizontal assist | Drop | No balance simulation |
| Gravity compensation, leg support, load boosts in `MuscleController` | Drop | Legs are kinematic; arms are pinned |
| `AttackPlanner` and `SwingDynamics` phase timing | Drop | Replaced by aimed moves |
| Physical catch steps, settle steps | Drop | Steps become procedural |

The current main moves to a backup branch first, so nothing is lost.

## Testing and observability

Most of the new system can be tested without PhysX, which the first build could not. Everything up to the physics targets is deterministic maths on the animation rig, so I can check it with automated tests before you open Unity.

### Automated checks (in the compile harness)

- **Aim accuracy:** for every move, against target nulls across its reach, heights and angles, the weapon's striking part passes within 3 cm of the aim null at contact.
- **Reach coverage:** every distance up to arm reach plus the longest lunge has a move that reaches it.
- **Feet:** no planted foot moves more than 1 cm while it is down; no leg is overstretched.
- **Weight:** in idle and at contact, the centre of mass is inside the support polygon.
- **Own body:** no swing path passes through its own wielder's torso or legs.

### What playtests check

The physics layer needs PhysX, so it is checked in play. Scripted sandbox scenes make that repeatable: a bystander placed in a swing's path, a heavy blocker, a cut aimed at an elbow, a launch into a line of units. The playtest log gains:

| Measure | What it tells me |
| --- | --- |
| Aim error per blow (aimed, then physical) | Whether the blow reached, and how far physics pulled it off |
| Every contact: who, which part, speed, intended or stray, damage, reaction | What each collision did to the fight |
| Severs and body impacts | Whether they happen when they should, and only then |
| Authority per region over time | What physics was allowed to do when |
| Physical-to-aimed pose error per region | How far pinned parts lagged |
| Foot slide per step | Whether feet stayed planted |

The pictures at 8 per second stay, so I can compare what you saw with the numbers.

## The first build

The first Unity build is a 3 v 3 fight on the 8 by 8 board, with one unit of each size per side, walking, attacking and recoiling physically. It covers milestones 1 to 3, with walking on the grid and combat idle brought forward from milestone 6.

| Unit | Height | Mass | Build |
| --- | --- | --- | --- |
| Small | about 0.9 m (half human) | about 15 kg | Lean, with a large head, hands and feet for its size |
| Medium | about 1.8 m | about 80 kg | Heroic human: broad shoulders, narrow waist |
| Large | about 2.6 m | about 280 kg | Taller and much broader: huge shoulders, arms and legs, a small head for its size |

- **Unthemed base meshes:** roughly anatomical with exaggerated proportions, made of separate body parts on the null rig, like the first orc. They're designed before any code changes.
- **Size decides the force:** a blow's impulse comes from the attacker's weapon speed and mass. The struck part recoils, its neighbours follow through their joints, and what the struck body doesn't absorb moves the whole unit. The same blow moves the small unit about 19 times as far as the large one, so a large unit hitting a small one sends it flying, while two large units barely shift each other.
- **Aimed at real heights:** a medium unit slashing at the small unit swings down to the small unit's actual chest; slashing at the large unit, it reaches up to the large unit's chest.
- **Animations:** walk cycles and programmatic attacks. There are no hit animations: hits are the body recoiling as physics objects.

## Milestones

The order follows the two failures you named: first a stance that is visibly carried by the legs, then blows that reach the body part they aim at. Physical hits, knockdowns and severs come back after both work.

&#91;embedded content: milestones M0 to M8 · each with its playtest gate\]

M1 and M2 run with physics at its lowest level (legs kinematic, upper body pinned firmly), so whatever goes wrong there is aim or animation, which tests catch. From M3 on, physics earns its place back one kind of contact at a time.

**Progress, 1 November 2026:** M0 to M6 are built, each with automated tests for its gate.

- **Captured motion** (between M4 and M5, ahead of M8): each body's idle and walk come from Mixamo clips. The idles are the base of its Fight Rhythm; the walk plays when the unit moves.
- **M5 Severing:** a fast sword edge that crosses a limb square to the bone, within about 5 cm of a joint, cuts it off there. The limb leaves as a loose physics object, with whatever its hand held. Losing the weapon arm disarms the unit, losing a leg puts it down for good, and losing the head kills it. Every wound bleeds. Clubs and thrusts don't sever. The gate test passes: a cut aimed at the elbow takes the arm off there.
- **M6 Fight loop:** units pick targets and walk tile to tile to a free tile next to their target, never two on a tile. They strike at body parts, and the fight ends on its own when one team can't fight any more. In headless runs, 3 v 3 fights end in about 1 to 2 minutes.
- **Openings** (decided for M6, open to change): an opening is a body part the target leaves open right now, either from a Fight Rhythm posture or from a guard sagging when tired. Each style reads openings some of the time (small 75%, medium 55%, large 35%). When it reads one, it aims at the open part and strikes up to 0.8 s early to catch it.

## Decisions and open questions

Decided on 31 October 2026:

- **Board:** an 8 by 8 grid for now; it may change.
- **Physics creates outcomes:** whatever a weapon or a flung body actually strikes takes damage, recoil, a knockdown or a severed limb, target or not. Fights won't replay identically.
- **Friendly fire:** full damage, for now.
- **Motion:** attacks are programmatic and aimed at real body part nulls, as in Hexamus; the aim is locked where the part was when the attack started. Clips only for idle, locomotion and get-ups. No hit animations: hits are physical recoil.
- **Combat idle:** units never stand still; sway, feints and dodges are part of each unit's style and change what blows strike.
- **Severing:** any limb. A fast blade near a joint severs; a slow blade only damages; clubs knock limbs away and never sever, for now.
- **Big hits:** launched bodies damage and can knock down the units they hit.
- **Second weapon:** a sword and shield unit comes in milestone 2, for cuts, severs, blocks and parries.

Still open:

- [ ] **Fighting on maimed:** units keep fighting after losing limbs. How each loss looks and plays (switching hands, hopping, kicking, crawling) is to be designed through play.

## Sources

- [Physics-driven animation in Unreal Engine](https://dev.epicgames.com/documentation/en-us/unreal-engine/physics-driven-animation-in-unreal-engine) (Epic)
- [Motion warping in Unreal Engine](https://dev.epicgames.com/documentation/en-us/unreal-engine/motion-warping-in-unreal-engine) (Epic)
- [Physics Meets Animation: Character Stunts, Just Cause 2](https://www.sambuz.com/doc/physics-ics-meets-s-anim-imation-ion-charac-racter-ter-ppt-presentation-926923) (Avalanche Studios talk slides)
- [PuppetMaster discussion thread, page 40](https://discussions.unity.com/t/puppetmaster-advanced-character-physics-tool-released/599766?page=40) (author's answers on pinning and muscles)
- [DReCon: data-driven responsive control of physics-based characters](https://www.ubisoft.com/en-us/studio/laforge/news/VjEIwquaIyEZZSw5RZI0V/drecon-datadriven-responsive-control-of-physicsbased-characters) (Ubisoft La Forge)
- [Blending ragdoll physics and animation in Unity](https://www.moddb.com/news/blending-ragdoll-physics-and-animation-in-unity) (ModDB)
- [Animation Bootcamp: An Indie Approach to Procedural Animation](https://gdcvault.com/play/1020049/Animation-Bootcamp-An-Indie-Approach) (David Rosen, GDC 2014)
- Hexamus `Docs/AttackMotions.md` (your own project)
