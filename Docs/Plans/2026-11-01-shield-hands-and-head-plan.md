# Shield, Hands and Head: Plan

> Copy of the Claude document <https://claude.ai/artifact/HCsdNXmL5zVwV1ep1ta5Gj>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 1, 2026 · @Renier

## Summary

**Recommendation: do all of it now, as milestone 9, before combos.** The rest of the combat plan builds on these:

- Combos and signature attacks need natural wrists and solid weapons.
- Defence needs the controlled shield.
- Dodges need to know what the unit can see.

Built on today's shield and wrists, those milestones would have to be tuned twice. And the faults show up in every fight now. In your three logs, 14 of 26 shield raises met blows from more than 60° off the unit's facing, and 10 from more than 100°. The shield hand swung up to 171° away from its usual angle.

M9 comes in three stages. Each ends with its checks and a playtest:

| Stage | What it adds | What it fixes |
| --- | --- | --- |
| M9a: see and guard | Perception and memory, the head's look-at, the shield controller | Blocks from behind, the wild shield, the stiff head |
| M9b: hands | Closed-fist meshes, wrist limits, grips | Broken-looking wrists, the shield and club grips |
| M9c: solid weapons | Ground clamp, shield shoves, shield get-ups | Weapons in the floor, the shield in bodies, get-ups through the shield |

The combat plan moves back by one: combos become M10, defence M11, dodging M12 and signature attacks M13. Nothing gets built until you agree to this order.

## What the screenshots show

Four gaps in the code explain all eleven screenshots:

- The shield is posed with no idea where the threat is, or whether the unit can see it.
- Weapons and shields are ghosts to physics.
- Wrists take any turn the weapon asks for.
- The head looks nowhere in particular.

Your three new logs bear this out:

- **Blocking blind.** 14 of 26 shields were raised against blows from more than 60° off where the unit faced, and 10 of those came from more than 100°, from behind.
- **A shield out of control.** The shield hand turned up to 139–171° from its usual way. In the unit that blocked most often (18% of the time), it was more than 134° off for 10% of the time.

| What you saw | What happens | Where in the code |
| --- | --- | --- |
| The shield rotates all over and blocks behind the back | A block points the shield straight back along the attacker's aim line and centres it 0.3 m in front of the locked aim point. It ignores where the defender faces. Nothing checks that the blow comes from the front or was seen: the only test is a dice roll. Out of a block, the guard is fixed to the root's yaw, not to the chest, so it swings as the hips and chest turn. | `UnitCombat.OnIncoming` and `CombatBeforePose` (BlockCenter, BlockNormal); `PosePipeline.GuardPose` and the block blend |
| The shield goes through enemies | Every weapon and shield collider is a trigger, and it ignores its owner. Triggers push nothing. Units are kept apart only by a body-radius push between hips and roots. | `Weapon.Equip` sets isTrigger; `UnitController.Separate` |
| The shield goes through its own wielder | The guard, the tempo stances, postures and the block target all move the shield, and none of them checks the shield's disc against the torso, head or legs. The arm solve only clamps the reach. | `PosePipeline` guard, `RhythmGuard`, block blend |
| Swords and shields go through the floor | The same triggers, and nothing in the pose keeps a held weapon above the ground. | `Weapon.Equip`; no ground clamp in the pose |
| Getting up drives the shield into the floor or the body | Both get-ups put both hands on the ground, holding something or not. A held weapon only changes the wrist's turn. | `GetUp` keys, `GetUp.Arm()` |
| Wrists bent like broken ones (the shield hand, both club grips) | The weapon's wanted rotation sets the hand outright. There is no wrist limit in the pose. The physical wrist limits apply only once the arm goes loose. | `PosePipeline` hand = rotation × inverse(slot); `JointMath` limits free while pinned |
| Open, mitten-like hands | The hand meshes are sculpted from palm shapes and a thumb cone. There is no fist anywhere. | `SourceArt/Bodies/Scripts` (`sculpt.py`, `simple.py`) |
| The head stares ahead | There is no look-at. The head only undoes 90% of the turn below it and adds the rhythm's nods. It looks at its own blow's aim point, and only during the blow. | `PosePipeline` step 3 |
| (Not shown, same cause) Reacting to unseen blows, choosing unseen targets | No field of view anywhere: blocks, pressure and target choice treat every unit as all-seeing. | `OnIncoming`, `FightLoop.ChooseTarget` |

## Research

The body's real limits give each plan its numbers. A shield faces its threat, angled and held forward. A wrist bends about 60° each way and less to the sides. A neck turns about 80°. The eyes cover about 210° but see detail only in the middle. Games that model perception give each agent a view cone and a memory, not full knowledge.

| Topic | What the source says | What we take |
| --- | --- | --- |
| [Viking round shield](https://hurstwic.com/history/articles/manufacturing/text/viking_shields.htm) (Hurstwic) | The hand grips a bar behind the boss, with no arm strap, so the shield turns freely in the hand. It is held forward of the body and angled, so blows glance off. There is also a high "peek-a-boo" guard by the head. Edge punches, bashes and binds. 80–90 cm across, about 5–7 kg. | A center grip: the forearm points at the boss, so the shield's face follows the forearm. It is held forward and tilted 15–25°, never flat against the chest. |
| [Wrist range of motion](https://pmc.ncbi.nlm.nih.gov/articles/PMC3016043) | Flexion about 65°, extension about 64°, ulnar deviation about 42°, radial about 20°. The useful "dart thrower's" path runs obliquely from radial extension to ulnar flexion: hammering, throwing. | Wrist limits of flexion 60°, extension 55°, ulnar 35°, radial 15°. Anything past them is taken up by the forearm's roll and the elbow. Swings travel the dart thrower's path. |
| [Cervical spine range](https://boneandspine.com/range-motion-cervical-spine/) | Flexion 80–90°, extension about 70°, side bend 20–45°, rotation up to about 90° each way, about half of it at C1–C2. | Head limits: turn ±80° (the chest adds up to 30°), down 60°, up 50°, tilt ±30°. |
| [Field of view](https://en.wikipedia.org/wiki/Field_of_view) | A little over 210° across, about 114° of it seen by both eyes, about 150° top to bottom. Motion is seen almost as well at the edge as in the middle. Form and colour are not. | A unit sees about ±100° from where its head points. It sees clearly (a blow's line, an opening) only within ±30°. At the edges it notices movement, more slowly. |
| [Eye and head in a gaze shift](https://pmc.ncbi.nlm.nih.gov/articles/PMC4370616) | The eyes alone cover about ±18°, and up to ±30°. Past that the head joins in, about 35% of the turn across and 20% up and down. A head turn takes about 0.2–0.8 s. | Our heads have no eyes, so the head points within about 20° of what it watches and doesn't track every twitch. A big look takes 0.25–0.5 s. |
| [Unity look-at weights](https://docs.unity3d.com/410/Documentation/ScriptReference/Animator.SetLookAtWeight.html) | A look-at is shared between body, head and eyes by weights, and a clamp weight keeps it in natural limits. | The look is spread over chest, neck and head, and clamped by the limits above. |
| [Unreal AI sight config](https://dev.epicgames.com/documentation/en-us/unreal-engine/API/Runtime/AIModule/UAISenseConfig_Sight) | Sight has a radius to notice, a larger radius to keep seeing, and a peripheral half-angle. A target last seen nearby can count as still seen. | Each unit keeps a list of what it knows: who, where last seen, when, and how clearly. It notices at one angle and keeps track at a wider one. |
| [The Last of Us NPC AI](https://arxiv.org/pdf/2207.00682) | A plain view cone missed players standing right next to the NPC, which made it look blind. Their fix widens the view the closer the target is. | The view widens close in: within about a body length, anything that touches or is beside the unit is felt, if not seen. |

## Plan A: a shield under control

The shield gets its own controller. It always holds the shield in front, angled toward the threat the unit knows about, kept off the body. It moves it onto a blow's line only when the unit has seen that blow and the line is within the shield's reach. A real center grip makes most of this fall out: the forearm points at the boss, so the shield's face follows the forearm, and the wrist stays straight.

### How the shield is placed

1. **The threat.** This is where the danger is: the opponent's chest by default, the opponent's weapon while it winds up, or a seen blow's path at contact. It comes from perception (Plan E), so an unseen threat moves nothing.
2. **Coverage cone.** The shield may face from 40° across the body (the weapon side) to 90° out to the shield side, from 35° down (the legs) to 55° up (overhead). Its direction is the threat's, clamped into that cone and measured from the chest, not the hips.
3. **The grip.** The shield's center sits 0.75–0.9 of the arm's reach out from the shoulder, along that direction. The forearm points at the boss, knuckles forward. The face is turned 15–25° off the threat line, so blows glance off, as the sources describe.
4. **Clear of the body.** Points round the rim are checked against the wielder's chest, belly, head and thighs. A rim that would enter the body moves the shield outward, then gives up some cover. It never goes into the body.
5. **Limited speed.** It turns at most about 300° a second and moves at most 2.5 m/s × size, eased in and out. Blocks are quick and returns are slow.

### Its states

| State | Shield | When |
| --- | --- | --- |
| Guard | Toward the opponent's chest at shoulder height, tilted 15° | Default |
| Track | Follows the opponent's weapon as it winds up | A seen wind-up |
| Block | Onto the seen blow's path at contact, face against the weapon's travel, in front of the part aimed at | A seen blow inside the cone, after the reaction time |
| High guard | Raised to the eyes, the "peek-a-boo" guard | Overhead blows; the shell (combat plan, Plan 5) |
| Low guard | Down over the knees | Low blows |
| Bash | Driven forward rim-first | The shield bash (combat plan, combos) |
| Fallen | Kept between the unit and the threat while it gets up | Plan C |

### What a block now needs

- **Seen.** The unit must have perceived the blow (Plan E): reaction time ×1 in clear view, ×1.5 at the edge of view, no block at all unseen.
- **In reach.** The blow's line must fall inside the cone. Seen but outside it, the unit turns its chest and feet toward the blow if there is time (turn speed against the time left to contact). Otherwise it can't block; from M12 it may dodge.
- **The real path.** The block meets the blow's path at contact, between the weapon and the part aimed at. Today it points back along the attacker's aim frame.

### Checks

- The shield faces within 45° of the threat for at least 95% of the time standing.
- Nothing goes behind the chest's plane, and the shield hand stays within 40° of its guard turn for 90% of the time (today 23–134°).
- No blocks against unseen blows (today 10 of 26 came from behind).
- No shield rim inside its own wielder by more than 2 cm.
- A `shield` runner shows a medium unit attacked from 12 directions, blocking only the ones it sees and reaches.

## Plan B: weapons and shields that collide

Weapons stay out of the ground and out of bodies two ways. The pose keeps them out to begin with, and a contact pushes back when that fails. Hits stay with the sweep, as now. Turning weapons into fully solid physics colliders isn't the first step. The hands that carry them are pinned bodies chasing a pose, so solid weapons pressing on each other and on pinned bodies would shake and fight the drives. That road stays open for dropped weapons only.

### 1. In the pose: kept out

- **The ground.** After the arms are solved, points along each held weapon (the tip, edge and pommel, and points round the shield's rim) are checked against the ground. Any below it plus a 2 cm margin lift the weapon: it turns about the grip, then the hand rises if turning isn't enough. This runs in every state, getting up included.
- **The wielder.** The shield is handled in Plan A. The sword and club already keep clear of their own body during blows (the clearance check). The same check now runs on the guard and on postures too.

### 2. On contact: pushed back

Each step, the shield's disc and each weapon's line are checked against every other unit's body (and later their weapons):

- **Shield against a body.** The other unit is shoved along the shield's face by how deep the overlap is: through the push that keeps units apart today, and through the knock-back once the overlap passes about 5 cm. The wielder's shield gives a little too, so it rests against the body instead of passing through. The shield bash in the combos builds on this.
- **A weapon resting against a body** (not a blow): the weapon gives way along its length. A blow is still the sweep's business.
- **Weapons against weapons** come with the parries (combat plan, defence). The same pass is built to take them.

### 3. Physics colliders, where they help

- Weapon colliders move onto the unused Weapon layer, with a collision matrix of their own (today every layer collides with every other).
- A weapon that leaves a hand (a severed arm, later disarms) gets solid colliders and a rigidbody, so it lands and rests on the floor. Today it stays a ghost on the cut-off limb.
- Held weapons stay triggers for the sweep. The pose and the contact pass keep them honest.

### Checks

- In fights and get-ups, no held weapon point goes more than 1 cm below the ground in the pose.
- A shield's overlap with another unit's body is under 2 cm in 95% of contacts, and no overlap lasts more than 0.2 s.
- A dropped weapon comes to rest on the floor.
- A new `contacts` section in the logs counts and measures each overlap.

## Plan C: getting up with a shield

A unit holding a shield gets its own get-ups. From its back, it pushes up on the sword hand alone while the shield stays up between it and the threat. From its front, it keeps today's push and lays the shield flat on the floor instead of through it. Today both get-ups plant both hands on the ground, whatever they hold.

### From the back (face up)

1. It rolls a little onto the sword-arm side, the shield arm coming across the chest.
2. The sword hand pushes off the ground on the fist, as with the knuckles. The blade lies flat along the ground and points back and away from the head and body; the clearance check guards this.
3. A knee comes up on the shield side, then the foot plants. The shield is in the **Fallen** state (Plan A), held toward the nearest threat it has seen and kept off the ground.
4. It rises behind the shield, and the sword comes up to its guard last.

### From the front (face down)

The push you liked stays: both arms push up from under the shoulders. The shield is held flat on the floor under the forearm, face down, with its rim resting on the ground. The ground clamp from Plan B lifts the shield hand so that the shield's lowest point sits at the floor. As the unit rises, the shield swings up into its guard.

### How it is built

- The get-up keys gain a shield set for each kind. The shield arm's targets come from the shield controller (Plan A, Fallen state) instead of the keys.
- Every get-up runs with the ground clamp (Plan B) for both weapons.
- A Mixamo get-up with sword and shield would help as reference if you have one. Otherwise the keys are made by hand and checked in the get-up film strips (`balance` runner and `getups.py`), as the get-ups were.

### Checks

The balance runner's get-ups for a medium unit, from its back and from its front, must all pass:

- It gets up.
- No pose pops, and the standing pose takes over smoothly (the existing gates).
- No weapon or shield point more than 1 cm below the floor.
- No blade or shield inside its own body.
- Over the second half of the get-up, the shield faces within 45° of the threat at least 80% of the time.

## Plan D: hands

Every hand becomes a closed fist with a channel for the handle. Every wrist gets human limits in the pose, and anything past them goes to the forearm's roll and the elbow instead of the wrist. Each weapon gets a real grip: a power grip for the club and stick, a hammer grip for the sword, a center grip for the shield.

### The fist

- **The mesh.** A new hand mesh for each body, made by the existing body build scripts (`SourceArt/Bodies/Scripts`, Blender as a Python module, which runs here). It is a palm block, four curled finger rolls, and the thumb wrapped over the index finger. A round channel runs through the curled fingers, so a handle visibly passes through the fist.
- **Kept as is.** The physical hand's box collider and the hand's mass.
- **The slot.** Each weapon's grip slot lines up with that channel, so the handle sits in the fist and not on the palm. The open hand comes back later for unarmed fighting.

### Wrist limits in the pose

The limits come from the research, a little inside the human range:

| Wrist | Limit |
| --- | --- |
| Flexion (palm toward the forearm) | 60° |
| Extension (back of the hand toward the forearm) | 55° |
| Ulnar deviation (toward the little finger) | 35° |
| Radial deviation (toward the thumb) | 15° |
| Twist | None at the wrist: the forearm rolls up to ±80° |

After the arm is solved, the wrist's turn against the forearm is measured. Anything outside the limits goes, in order:

1. Into the forearm's roll (pronation or supination).
2. Into the elbow's swing: the arm solve's pole moves.
3. Into the weapon itself, as a last resort. The weapon turns a little less than the move asked, and `TryOut` and the planner see the real pose, so aim stays honest.

The physical wrist joints get the same limits while pinned. Today they are free until the arm goes loose.

### Grips

| Weapon | Grip | In guard | In a swing |
| --- | --- | --- | --- |
| Club and stick | Power grip: the handle across the palm on a slant, the weapon about 100° from the forearm | Wrist straight to slightly bent back | Along the dart thrower's path, from radial extension to ulnar flexion, as in hammering |
| Sword | Hammer grip: the blade about 80–90° from the forearm | Wrist straight; the blade's angle comes from ulnar deviation and the forearm's roll | The same path; the move's wrist lag (35°) stays inside the limits |
| Shield | Center grip behind the boss, knuckles forward | The forearm points at the boss, wrist straight (Plan A) | Bash: punched along the forearm |

Each style's guard angles are measured again, so that the wrists sit within 15° of straight in guard.

### Checks

- A `wrists` column and a report line in the logs give each wrist's angle outside its limits.
- In guard: no frame outside the limits. During blows: at most 2% of frames more than 5° outside.
- The aim matrix keeps zero misses with the clamped wrists.
- A picture sheet of each body's fists on each weapon, front and side, goes to you before it is merged.

## Plan E: the head, what it looks at, and what the unit knows

The head becomes where a unit's knowledge comes from. Each unit sees from its head, within human limits. It knows only what it has seen, felt or remembers, and every decision that reacts to the fight (blocks, dodges, pressure, turning, choosing a target) uses that knowledge instead of the truth. The head points at what holds its attention, and that is visible life in itself.

### What it sees

| Zone | Where | Notices | Reaction time |
| --- | --- | --- | --- |
| Clear | Within ±30° of where the head points | A blow's line, an opening, a wind-up | ×1 |
| Edge | ±30–100° across, ±70° up and down | Movement: a unit, a swing starting | ×1.5, and it must look to read openings |
| Felt | Within about a body length, any direction | A touch, a body bumping it, being hit | Only after the contact |
| Unseen | Everything else | Nothing | No reaction |

There is no occlusion at first: units don't yet hide each other. It is easy to add later as a ray to the head.

### What it knows

A small memory per unit holds each other unit: where it was last seen, when, how clearly (clear or edge), and which way it was moving. Seen units update each step. One out of view is remembered for about 3 s, its position carried forward along its last motion, then forgotten. Being hit by an unseen unit puts it in memory at once: it knows where the blow came from.

### Where the head looks

- **Attention.** Each step, an attention score picks one point to look at:
  - a seen blow coming at it;
  - the opponent's weapon winding up;
  - its current opponent's head and chest;
  - something new at the edge of its view;
  - now and then, a glance at the nearest other unit, enemy or ally.
- **Hold and latency.** A new point must beat the current one by a margin and is held at least 0.4 s, so the head doesn't twitch. A new stimulus is answered after 0.1–0.2 s.
- **Sharing the turn.** The look is spread over the chest (20%), neck (35%) and head (45%). It is clamped to the limits from the research: turn ±80° (plus 30° from the chest), down 60°, up 50°, tilt ±30°. A big look takes 0.25–0.5 s, eased in and out.
- **No eyes.** The head lands within about 20° of what it watches, not dead on. During its own blow it keeps looking at its aim point, as now.
- **The body follows.** A threat seen at the edge, outside the shield's cone, turns the chest, then the feet, toward it.

### What uses it

| Decision | Today | With perception |
| --- | --- | --- |
| Block | Any blow, from anywhere | A seen blow inside the shield's cone (Plan A) |
| Dodge (combat plan, M12) | (not yet) | A seen blow only; clear view gives the best timing |
| Pressure and the shell | Every blow at it counts | Seen blows count; unseen ones only once they land |
| Reading openings | Any time | Only in clear view |
| Choosing a target | Every enemy, by distance | Known enemies; one that hits it from behind becomes known and turns it round |
| Turning to face | Always the target | The target, then any seen threat, by attention |

### Skill levers

These join the per-unit skill block you approved:

- Field of view (wide-eyed to tunnel-visioned).
- The edge-of-view reaction multiplier.
- How long it remembers.

A "deeper perception" for special units later (hearing, a sixth sense) is knowledge with low confidence. It can turn the unit and raise its guard in time, but the shield still moves only within the body's limits.

### Now, or later?

Now, together with the shield. The shield's "only what it sees" rule needs this knowledge. So do the dodges and the defence in the combat plan, and the attention-driven head is the cheapest piece of life the units can get. The first version leaves out occlusion and hearing.

### Checks

- No block, dodge or pressure from a blow the unit didn't perceive.
- The head stays inside its limits on every frame.
- It points within 25° of its attention point 80% of the time.
- A seen blow draws the head to the attacker within 0.3 s.
- A `look` runner films a unit with enemies walking round it, with the zones drawn.

## Schedule

&#91;embedded content: roadmap · M9 in three stages, then the combat plan\]

Each stage ends with its checks and a playtest for you, as before. M9a comes first because the shield controller needs perception to know what it may block. The hands come next because the controller's grip and the clamp both lean on the wrist limits.
