# Physics Combat Animation — Design Spec

> Copy of the Claude document <https://claude.ai/artifact/NyEcLWt3RMRcpocjXwJU3V>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Oct 30, 2026 · @Renier

## Summary

We build an **active ragdoll**: every unit is two rigs at once. An invisible *animation rig* of nulls is posed procedurally (as in Hexamus), and a visible *physical rig* of rigidbody body parts chases that pose with spring-driven joints. The whole system hangs on one idea:

> **The animation is a wish. Physics is the truth.**

The procedural code never moves a visible body part directly. It only says where each part *would like* to be. Joint motors ("muscles") and a balance assist try to get it there, and whatever gets in the way — a prop, another unit, a club — wins or loses by physics. Every effect in your brief falls out of that one rule:

- **The club hits whatever is in the way**, because the club is a real rigidbody that is only *pulled* along the planned arc. Damage comes from what it physically touches, not from who it was aimed at.
- **Units shove and knock each other over**, because their body parts are always solid, during every animation.
- **The hit orc reels but tries to stay standing**, because its muscles never switch off. A hit lowers its *stability* for a moment, the balance assist weakens, it staggers or falls, then the assist ramps back up and it recovers.
- **An interrupted attacker swings into someone else**, because an interruption only weakens his muscles and balance. Nobody cancels the club's momentum, so it keeps travelling and hits whatever it reaches.

The main concession to keep this simple: balance is **not** physically honest. A tunable "hand of god" force holds the pelvis up and the torso upright, and hits take that support away. This is how [Totally Accurate Battle Simulator](https://landfall.se/totally-accurate-battle-simulator) and Gang Beasts work, and TABS is the closest existing game to what you describe — a physics-driven auto-battle sim. It looks a bit gamey, and it is robust and fully tunable.

The first prototype is two placeholder orcs: one swings a body-length club low along its side and into the other's chest, and the victim reels, staggers and tries to stay on its feet.

## How hard is this?

Getting it *working* is medium: active ragdolls are a well-trodden path with known recipes. Getting it to *look good and read clearly* is hard and never quite finished — it is tuning, not code. The genuinely hard parts are not the animation at all: they are **performance with many units** and **non-determinism**, which are game-design decisions we should make early.

| Requirement | Difficulty | Why | How we keep it manageable |
| --- | --- | --- | --- |
| Body parts stay physical while animating | Medium | Joint chains in PhysX jitter, stretch or explode under big mass differences and violent impacts | Spring-driven joints (not kinematic), sane mass ratios, more solver iterations, primitive colliders only |
| Programmatic, distance-aware attacks | Medium | Needs a small motion planner plus arm IK instead of hand-placed keys | You already have the procedural-motion mindset from Hexamus; the swing is a parametric arc, not a curve per attack |
| Heavy weapon with real momentum | Medium | Fast swings tunnel through colliders; Unity clamps angular velocity by default; a heavy club on a light hand destabilises the chain | Continuous collision on weapons, raised velocity caps, separate *physics mass* and *gameplay mass* for weapons |
| Club hits whatever is in its path | Easy once the above works | It is just a collision | Damage is decided by contact and impact speed, never by the attack's intended target |
| Hit reactions that stay standing | Medium with an assist, very hard without | Physically honest balance (stepping controllers, SIMBICON-style) is a research problem | Hand-of-god balance assist scaled by a stability value, plus procedural catch-steps |
| Interrupted attacks that hit others | Easy, it emerges | Comes free if interruptions only weaken muscles and never cancel motion | No special code; we just test it |
| Looking good and readable | Hard, ongoing | Physics looks floaty, twitchy or drunk until it is tuned | Strength profiles per state, short localised "muscle dips" on hits, debug tools to tune live |
| Many units at once | Hard | An auto battler might have 20–30 units × about 16 bodies = 300–500 jointed rigidbodies | Budget early, low-detail physics for distant or idle units, measure in milestone 6 |
| Same result on every machine | Very hard | PhysX is not deterministic across machines, so a fight can't be replayed or verified from its inputs | A design decision (see Open questions): fine for single-player or live-streamed fights, a problem for async PvP |

Overall: very doable for one unit type and a prototype scene. The risk is in scale and in how players read and predict fights whose outcome is partly chaotic — the fun you want, and also the thing that is hardest to balance.

## Architecture

Each unit runs one ordered loop every physics step: read the body, decide, pose the animation rig, drive the muscles, balance, simulate, then turn contacts into hits that feed back into the next step. Everything runs in `FixedUpdate`, called in order by a single `UnitController` per unit, so the wish and the truth never drift a frame apart.

&#91;embedded content: one unit's physics step · 8 stages in a loop\]

Steps 1–6 are our code; step 7 is the only place bodies actually move, and step 8 feeds what happened into the next pass.

One difference from Hexamus worth calling out: there, plain-C# Core rules were the source of truth and Unity only presented them. Here **the physics simulation is the source of truth**, so combat outcomes live in the Unity runtime. Damage, health and stats can still be plain data, but *whether a hit happened* is decided by PhysX.

| Component | What it does |
| --- | --- |
| `UnitRig` | Holds references to every null and body part by name; the shared vocabulary of the whole system |
| `PhysicalBodyBuilder` (editor) | Adds rigidbodies, colliders and joints to the authored rig from per-part definitions |
| `AnimRig` | The invisible ghost copy of the null hierarchy that procedural code poses |
| `PoseComposer` | Builds the ghost's pose each step: base pose, action overlay, IK, look-at |
| `AttackPlanner` + `SwingSolver` | Turns an attack definition, the weapon and the target into weapon and body targets over time |
| `MuscleController` | Writes joint drive targets and strengths; handles strength profiles and hit dips |
| `BalanceController` | Hand-of-god support, catch steps, re-anchoring the unit's root to its body |
| `UnitState` | Stability value and the Steady / Flinch / Stagger / Knockdown / GettingUp / Dead state machine |
| `ImpactSystem` | Turns collisions into hit events: damage, extra impulse, stability loss, muscle dips |
| `CombatDebug` | Ghost overlay, push tool, time controls, swing gizmos, HUD |

## The rig

The rig keeps your Hexamus convention — a tree of named nulls, one separately pivoted mesh under each at zero offset — and grows it from 6 floating parts to 15 jointed ones. Arms and legs need real segments now, because elbows and knees are where hits bend the body.

&#91;embedded content: orc rig · 15 body parts, 2 weapon slots, 1 logical root\]

The character's right side is drawn on the left, as if facing you. The aim markers (`Aim_Chest`, `Aim_Head`) are left out of the drawing; they sit under Chest and Head.

**Conventions**

- **Real-world scale.** 1 unit = 1 m, Y up, units face +Z. The placeholder orc is about 2.0 m tall. Physics tuning is scale-sensitive: at Hexamus's 0.8 m chibi scale, falls look fast and twitchy under real gravity.
- **Every null has identity rotation in the rest pose** (as in your Hexamus reference rig, where every transform is a plain translation). This is the single biggest simplifier: the joint math collapses to one inverse quaternion (see Muscles).
- **Rest pose = relaxed stand**, arms hanging slightly away from the body, not a T-pose. Joint limits are measured from rest, so a rest pose close to idle keeps limits simple.
- **Pivots at the joint**: thigh at the hip, shin at the knee, forearm at the elbow, and so on. The mesh under each null sits at zero offset.
- **Markers** are extra nulls with no mesh and no physics: `Slot_Weapon_R` / `Slot_Weapon_L` under the hands (grip point and orientation), and `Aim_Chest` / `Aim_Head` (where attackers aim on this unit).

**From one authored rig to two runtime rigs**

1. You author one prefab: the null tree plus meshes. For the prototype I generate the placeholder orc from Unity primitives by script, with the final null names, so your Softimage/Houdini parts can drop in later without touching code.
2. An editor command, *Build Physical Body*, adds a Rigidbody, a primitive collider and a joint to each body-part null, reading sizes, masses and limits from per-part definitions. Baking in the editor (rather than at runtime) lets us inspect and tweak every joint in the Inspector.
3. At spawn, the unit clones its null tree without meshes or physics: that clone is the `AnimRig` ghost. It stays parented under the unit's root and moves with it.
4. The physical body is **unparented** at spawn. In Unity, moving the parent of dynamic rigidbodies teleports them, so the body must never sit under a transform that gameplay moves.

## The physical body

Each of the 15 body parts is a Rigidbody with one primitive collider, joined to its parent part by a `ConfigurableJoint`. The orc weighs 120 kg in physics; masses follow human proportions, but hands and feet are made heavier than real so no joint links bodies more than about 4:1 apart — large mass ratios across a joint are the main cause of ragdoll jitter and stretching.

| Part | Parent | Collider | Mass % | Mass (kg) | Joint feel |
| --- | --- | --- | --- | --- | --- |
| Pelvis | — (free) | Box | 14 | 16.8 | Root body; balance forces act here |
| Spine | Pelvis | Box | 13 | 15.6 | Bend ±30°, twist ±30° |
| Chest | Spine | Box | 15 | 18.0 | Bend ±25°, twist ±30° |
| Head | Chest | Sphere | 7 | 8.4 | Nod ±40°, turn ±70° |
| UpperArm L/R | Chest | Capsule | 4 each | 4.8 | Ball joint, wide limits |
| Forearm L/R | UpperArm | Capsule | 3 each | 3.6 | Hinge 0–140° |
| Hand L/R | Forearm | Box | 2 each | 2.4 | ±45°, small twist |
| Thigh L/R | Pelvis | Capsule | 9 each | 10.8 | Ball joint, forward 100°, back 30° |
| Shin L/R | Thigh | Capsule | 5 each | 6.0 | Hinge 0–140° |
| Foot L/R | Shin | Box | 2.5 each | 3.0 | ±30° |

All limits are starting points for the placeholder and are tuned in milestone 1. Hinge-like joints use a tight *Limited* range (about ±5°) on their off-axes rather than *Locked*, so one rotation drive mode works the same on every joint.

**Collision rules**

- **A unit never collides with itself.** At spawn we call `Physics.IgnoreCollision` for every pair of its own colliders, its weapon included (about 130 pairs, once). We lose arms visibly blocking against the torso; we gain a big chunk of stability.
- **Layers:** `Body`, `Weapon`, `Prop`, `Ground`. The ghost rig has no colliders at all.
- **Physic materials:** feet high friction (about 0.8) so stances hold; everything else around 0.4; no bounce.

**ConfigurableJoint or ArticulationBody?**

Unity offers two ways to build a jointed body. We start with Rigidbody + `ConfigurableJoint`:

- It interacts with everything else in the scene in the ordinary way.
- Joints can break at runtime, which keeps disarms and dismemberment (as in Hexamus) on the table.
- Most active-ragdoll knowledge and examples use it.

`ArticulationBody` is the backup. Its reduced-coordinate solver cannot stretch joints apart and copes much better with heavy weapons on light limbs, but it is awkward to break apart at runtime and its drives are set per axis. The muscle code sits behind one small interface, so if milestones 2 or 5 show joints stretching under club hits that tuning can't fix, swapping is a contained change.

## Placeholder orc

The placeholder orc is built by script in Blender: 15 rigid parts plus the 2 m club, about 11,300 triangles in total. Every joint stays closed at extreme bends, and no two colliders of different bodies come closer than 2.5 cm at rest.

&#91;image: Placeholder orc: front, side, three-quarter and back\]

**Technical rules it follows**

1. **Ball joints hide the seams.** At every joint, both parts carry a sphere centred exactly on the shared pivot (elbow, knee, wrist, ankle, shoulder, hip). Rotating about the pivot keeps the union a closed ball, so no gap ever opens. The parent's sphere is a few millimetres bigger than the child's, so the two never z-fight. The waist hides inside a thick belt; the neck runs down into the hunched traps.
2. **Colliders stop short of every joint.** Each limb capsule ends 3–7 cm before the pivot at both ends. The overlap zones are covered by mesh only, so colliders are never forced into each other. Torso boxes are stacked with a 2.5 cm gap between them.
3. **Pivots and nulls.** Each mesh's origin is its joint. Every null has identity rotation, the arms hang 14° out from vertical, and feet sit exactly on the ground.
4. **Tilted capsules go on child objects.** Unity's `CapsuleCollider` only runs along a local axis, and the arm bones are tilted. So each capsule is described by its two end centres and is built on a child object rotated to match.
5. **The club joins the hand.** Its two capsules (shaft and head) are grip-relative and are added to `Hand_R`'s rigidbody as a compound. Markers give the grip, the sweet spot (1.5 m out, mid-head) and the tip.

&#91;image: Colliders over the faded body: a gap at every joint\]

&#91;image: Joint close-ups in an extreme pose: every joint stays closed\]

**Known trade-offs**

- The shoulder and hip balls are not covered by any collider. A club that lands exactly there passes into the neighbouring collider a few centimetres later. If that matters, the chest box can widen by about 3 cm before it meets the arm capsules.
- Parts are overlapping primitives, not merged meshes. That's fine for a placeholder; final art will want clean, merged parts.
- The chest strap is a flat slab and doesn't hug the torso.

**What ships** (Orc\_Placeholder.zip): one FBX per part plus the club (Forward −Z, Up Y, Apply Transform, scale 1), an assembled reference FBX for Softimage or Houdini, `orc_rig.json` (nulls, masses, colliders and club markers in Unity axes), the .blend, and the scripts that rebuild, check and render it all.

## Muscles

Every joint runs a spring (a *Slerp drive*) that rotates the physical part toward the ghost's local rotation for the same null. Because every null rests at identity rotation, the drive target is just the inverse of the ghost's local rotation:

```csharp
// Joint set up once, in the rest pose:
//   joint.axis = Vector3.right; joint.secondaryAxis = Vector3.up;
//   joint.rotationDriveMode = RotationDriveMode.Slerp;
// With those axes the joint frame matches the part's local frame, so:
joint.targetRotation = Quaternion.Inverse(ghost.localRotation) * restLocalRotation; // restLocalRotation == identity
```

The joint remembers the orientation it had when it was created, so joints are built while the body is in its rest pose. If the rest pose ever changes, the joints are rebuilt.

**Strength, not position, is what we animate.** Each joint's spring, damper and maximum force come from three multipliers:

```text
spring = baseStiffness(part) × groupStrength(group) × stateMultiplier(state)
maxForce caps it, so a muscle can push hard but never infinitely hard
```

- **baseStiffness** is per part and scaled by the part's mass, so tuning carries over to bigger or smaller units. Damping starts at about 10% of the spring and is tuned for "heavy" versus "snappy".
- **Muscle groups:** Legs, Core (pelvis–spine–chest), Neck, Arm L, Arm R. Each has a strength from 0 to 1 that states and hits change.
- **maxForce is what makes weight matter.** During a strike the weapon arm is boosted but capped. When the club meets a light goblin, the arm overpowers it and follows through; when it meets a heavy orc or a wall, the arm stalls and the club stops. No special code — just a capped muscle meeting resistance.

**Hit dips.** When a part is hit, that part and its neighbours drop to about 20% strength for roughly 0.15 s, then recover over 0.4 s. A chest hit softens the spine and neck for a moment, so the torso folds and the head whips back, then the unit pulls itself together. This one trick does most of the work of making hits look physical.

**Optional sharpening:** also feed the ghost's angular velocity into `joint.targetAngularVelocity`, so fast motions track with less lag. Leave it off until something feels too floaty.

## Balance and stability

Units stay up because an invisible, tunable support holds the pelvis at standing height and the torso upright; hits take that support away in proportion to how hard they were, and it grows back. Muscles never switch off, so even a falling unit keeps its shape and looks like it is trying.

**1. The balance assist ("hand of god")**

Each physics step, a spring-damper pulls the physical pelvis toward the ghost pelvis and turns the pelvis and chest toward the ghost's orientation:

```text
force  = balance × (kp × positionError − kd × pelvisVelocity)        // as acceleration, so mass-independent
torque = balance × (kpRot × rotationError − kdRot × angularVelocity)  // on pelvis and chest
```

- Vertical and horizontal gains are separate: strong vertical support keeps the unit from sagging, weaker horizontal support lets it be shoved.
- `balance` runs from 0 to 1 and is set by the unit's state. At 1 the unit is planted; at 0.3 it wobbles and can be pushed around; at 0 it falls.
- If needed, add the TABS trick: a small upward pull on the head and a downward pull on the feet. It makes bodies "string up" convincingly.

**2. Stability**

Every unit has a stability value (0–100, think "poise"). Hits and shoves subtract from it; it regrows after a short delay. Heavy units lose less per hit. The value sets the unit's state, and the state sets `balance` and muscle strength:

| State | Enters when | balance | Muscles | What you see |
| --- | --- | --- | --- | --- |
| Steady | Stability above 60 | 1.0 | 100% | Planted, idle or acting |
| Flinch | A hit that leaves stability above 60 | 0.8 | Local dip only | Torso and head take the hit, feet stay put |
| Stagger | Stability 25–60 | 0.3 → 1 | 70% | Reels, takes catch steps, regains footing |
| Knockdown | Stability below 25, or torso tilted past about 60°, or pelvis too low | 0 | 30%, brace pose | Falls, but keeps its shape and tries to brace |
| GettingUp | Body has come to rest on the ground | 0 → 1 over about 1.2 s | Ramps to 100% | Pulled back up onto its feet |
| Dead | Health reaches 0 | 0 | Fade to 0 over 0.3 s | Full ragdoll |

The physical check matters: a unit tipped past the point of no return falls even if its stability is high. That keeps the state honest to what the body is actually doing.

**3. Catch steps**

While flinching or staggering, the unit checks whether its center of mass is drifting outside its feet. If it is, the ghost steps a foot toward where the body is going — the *capture point*, `centerOfMass + velocity × √(height / g)`, a standard robotics shortcut for "where to step to stop". The foot farther from that point moves; muscles and foot friction carry it out. After about three steps without recovering, the unit gives up and falls. This is the cheapest way to get the "stumble back but stay up" look you described.

**4. Re-anchoring**

The unit's logical root (where gameplay thinks it is) follows its physical pelvis whenever the two drift more than about 0.15 m apart. Without this, a unit shoved a metre away would be dragged back to its old spot by its own balance assist. With it, being knocked aside actually moves you on the board.

**5. Getting up**

Once the body has nearly stopped (pelvis slower than about 0.3 m/s for 0.5 s), the ghost is placed over the pelvis in a crouch, facing whichever way the body landed. The balance assist ramps from 0 to 1 while the ghost rises to standing. It reads as being pulled up by strings — gamey, fitting the TABS-like tone, and good enough for the prototype. A proper push-up-and-rise can come later.

## Weapons

A held weapon becomes part of the hand: its colliders are added under the hand's Rigidbody, so hand and club are one rigid body with the club's weight and leverage. That is the most stable way to hold a long, heavy object in PhysX — a separate club body welded on with a joint wobbles at the end of a 2 m lever.

**Weapon definition (one data asset per weapon)**

| Field | Club example | Used for |
| --- | --- | --- |
| Length | 2.0 m | Reach, swing planning |
| Grip offset | Handle end | Where `Slot_Weapon_R` holds it |
| Sweet spot | 1.5 m from the grip, mid-head | The point the swing aims at the target |
| Colliders | Capsule shaft + sphere head | Contact detection |
| Physics mass | 6 kg | What PhysX simulates; keeps the arm chain stable |
| Gameplay mass | 40 kg | What hits *feel* like: damage, knockback, stability loss |
| Damage, knockback, stability scales | Per weapon | Tuning per weapon type |

**Two masses, on purpose.** A real 40 kg club on a 2.4 kg hand would tear the joint chain apart. So PhysX sees a 6 kg club, which still swings with visible weight, and the impact system uses the 40 kg gameplay mass when it decides how hard a hit lands. This is the main "gamey" concession for weapons, and it is invisible to the player.

**Momentum comes from three things working together:** the planner's timing (slow wind-up, accelerating strike), the arm's capped muscle force (the club lags behind its target and keeps travelling when the target stops), and the club's own inertia. Because of the lag, a club that misses carries on past the target and the arm has to haul it back.

**Fast-swing settings.** The hand-plus-weapon body uses *Continuous Speculative* collision detection, which, unlike the other continuous mode, handles fast rotation — and a swing is mostly rotation. Its `maxAngularVelocity` is raised from Unity's default of 7 rad/s, which would otherwise silently cap the swing speed.

**Later, not now:**

- **Disarms.** On a big enough hit, the club's colliders move onto a new Rigidbody that inherits the hand's velocity, and it flies off (the same runtime trick as Hexamus's dismemberment).
- **Two-handed grips.** The ghost's off hand reaches for a second grip point by IK; the physical off hand just follows its target and is never jointed to the weapon.

## Attack planning

An attack is not a curve per attack but a **parametric arc**: the weapon rotates around the attacker's shoulder on a tilted plane, and the plane, the arc and the arm's reach are solved fresh each time so the weapon's sweet spot passes through the target's aim point at the moment of fastest swing. Different distances and target heights give different-looking swings from the same definition.

**Geometry of one swing** (all on the ghost rig)

1. `S` = the weapon-side shoulder, read *after* the body overlay has twisted and leaned the torso.
2. `T` = the target's `Aim_Chest` marker on its **physical** chest, predicted forward by its velocity × time to contact.
3. `d₀ = normalize(T − S)` is the club's direction at contact.
4. The swing axis `a` is perpendicular to `d₀`, rolled by the attack's *plane roll*: 0° gives a vertical plane (overhead or underhand), 90° a horizontal sweep. The orc's low rising swing is about 45°, a diagonal plane like a golf swing: in a near-vertical plane, a 2 m club held at shoulder height would drive into the ground at the bottom of the arc.
5. The club's direction at swing angle θ is `d(θ) = rotate(d₀, a, θ)`. θ = 0 is contact; about −150° points back and down beside the orc's leg; about +50° is the follow-through past the target.
6. The grip sits at `G(θ) = S + d(θ) × e`, where `e` is how far the arm extends; the sweet spot is at `G + d(θ) × s` (s = sweet-spot distance along the club).
7. Two-bone IK places shoulder, elbow and hand to reach `G`, with the hand turned so the club points along `d(θ)` and its striking face leads the swing.

&#91;embedded content: orc club swing · side view, 4 phases\]

The club is pulled back low along the arc, coils briefly while the target locks, then accelerates around the bottom so its sweet spot reaches T at full speed. In 3D the tilted plane carries the arc past the orc's leg rather than through it.

**Reach decides the swing.** At contact the sweet spot must reach the target, so `e = |T − S| − s`, clamped to the arm's comfortable range:

- **In range** — the arm extends just enough. Every distance inside the range gives a slightly different arc.
- **Too far** — the orc leans in (pelvis forward up to about 0.2 m), then adds a step with its lead foot during the wind-up. Still too far: the attack isn't valid, and the AI must close distance first.
- **Too close** — contact slides down the shaft toward the hand: a *crowded hit*, with damage scaled down by how far from the sweet spot it landed.

A consequence worth knowing: with a 2 m club, a sweet spot at 1.5 m and a 0.8 m arm, the orc's ideal spacing is about **2 m centre to centre**. An enemy standing right against him gets a crowded hit. The weapon sets a natural engagement range, which maps neatly onto an auto battler's "range" stat.

**Phases of the orc's club swing**

| Phase | Duration | Club (θ) | Body | Feel |
| --- | --- | --- | --- | --- |
| Wind-up | 0.8 s | From carry to −150°, low beside the leg | Torso twists away 35°, weight back, knees bend | Slow ease-in-out: the club is heavy |
| Load | 0.15 s | Creeps to −160° | Holds the coil; **target locks at the end** | Telegraph moment |
| Strike | 0.35 s | −160° to +50°, fastest at 0° | Torso unwinds 25° past centre, weight forward, arm extends, arm muscles boosted | Accelerating: momentum builds |
| Follow-through | 0.3 s | Decelerates past the target | Body carried forward by the club | Ends early after a solid hit |
| Recover | 0.6 s | Back to carry | Arm muscles at 60%, so the club drags back | Vulnerable window |

The head looks at the target throughout — cheap, and a large readability win. An attack-speed stat simply scales all durations.

**Commit point.** Until the end of Load the target point is updated every step, so the swing tracks a moving target. From then on the arc is frozen in world space: a target that steps away gets missed, and anything that steps into the arc gets hit. This makes attacks readable and dodgeable, and it is what lets bystanders catch a club.

**The ground.** The wind-up's club head passes close to the ground. By default the solver keeps every point of the club at least 0.15 m up, first by bending the elbow, then by flattening the swing plane. Optionally we let the head scrape the ground on a low-friction material, which would look very heavy — worth trying once the basics work.

**Everything is data.** An attack definition holds the phase durations, θ angles, plane roll, body-overlay amounts, easing curves, muscle boosts and damage multiplier per phase. An overhead slam or a horizontal sweep is a new data asset. A thrust (spear) needs a second solver mode that moves the grip along a straight line instead of an arc.

## Impacts

A hit is any contact between a weapon collider and another unit's body part that is fast enough; it does not matter who the attack was aimed at. PhysX supplies the contact; the impact system decides what it means and adds a deliberate, tunable shove on top, because raw PhysX contacts between muscle-driven bodies are too soft and too random to read well.

**Detecting a hit**

1. The hand-plus-weapon body gets the collision callback. If the collider it hit carries a `BodyPart` component, we have a victim and a part.
2. Impact speed = the closing speed of the two contact points along the contact normal (from `GetPointVelocity` on both bodies).
3. Below about 2 m/s it is a touch, not a hit. Each weapon can hit each victim once per 0.25 s, so contact jitter doesn't produce a burst of hits.

**Resolving it**

```text
damage          = weapon.damage × (speed / referenceSpeed) × part.multiplier × phaseIntent × crowdedFactor
impulse         = weapon.gameplayMass × speed × weapon.knockback
stabilityLoss   = impulse × weapon.stabilityScale / victim.poiseResistance
```

- **Damage scales linearly with speed**, not with kinetic energy: a squared term makes glancing blows worthless and full swings absurd. Head about 1.5×, limbs about 0.7×.
- **phaseIntent** is 1.0 during Strike and Follow-through, about 0.5 for an abandoned swing still flying, and about 0.3 for a club that merely swings into someone while its owner falls. Emergent hits count, but less.
- **The extra impulse** is split about 60% onto the struck part and 40% onto the victim's pelvis, so the whole body is moved instead of one limb flailing. Its direction blends the contact normal with the weapon's velocity, with a slight upward lift so knockbacks read from a top-down camera.
- **The victim** loses stability (which may change its state) and gets a hit dip on the struck part and its neighbours.
- **The attacker:** on its first solid hit the swing moves straight to Follow-through. If the victim was much heavier and the club stalled, the attacker's arm takes a short dip of its own — a visible bounce off.

**Hit event.** Every hit is published as one event — attacker, victim, part, point, normal, speed, damage, impulse, stability loss — for health, VFX, sound, camera shake and the debug log. Optional juice for big hits only: a 50–70 ms hit-stop. With many units fighting, global hit-stop on every hit would be tiring.

**Non-weapon contact.** Bodies shoving, a unit falling on another, a crate tipping onto someone: these go through the same path but cost stability only, no health. That alone produces most of the "walked into each other and fell over" moments.

## Interruptions and emergent moments

The spontaneous moments you want need no special code; they come from four rules that every system respects.

1. **Never cancel motion — only change intent and strength.** Nothing ever snaps a body part or weapon to a new place. An interruption lowers muscle strength and balance; momentum does the rest.
2. **Damage belongs to contact, not to intention.** Whatever the weapon physically meets is what gets hit.
3. **An attack yields to its owner's body.** If the attacker drops into Stagger or Knockdown, its attack is *abandoned*: the planner stops steering, and the ghost arm copies the physical arm, so the muscles stop pulling the club anywhere. The club flies on under its own inertia.
4. **A flying weapon stays dangerous** while it is fast, at reduced damage (the phaseIntent values in Impacts).

**Your side-hit example, step by step**

| Moment | What happens | Which rule |
| --- | --- | --- |
| Orc A is in Strike, club accelerating | Normal swing toward orc B | — |
| Orc C hits A in the side | A loses stability and enters Stagger; balance drops to 0.3; A's core muscles dip | Impacts |
| The attack is abandoned | Planner stops; A's arm muscles stop steering the club | Rule 3 |
| A topples sideways | Balance assist too weak to hold him; catch steps fail or have no room | Balance |
| The club keeps travelling, now along a new path | Its inertia plus A's falling body carry it | Rule 1 |
| It strikes goblin D standing nearby | Fast enough to count; D takes reduced damage and a shove | Rules 2 and 4 |

**Things to test deliberately** (each becomes a button in the sandbox):

- A crate placed between attacker and target: the club should hit the crate, knock it away, and lose speed.
- A small goblin walking into the arc after the commit point: it takes the hit; orc B is spared or takes a weaker one.
- The attacker hit from the side during wind-up, during strike, and during recovery.
- Two units walking into each other: both should shove, and the lighter one should give way.

**Friendly fire** follows from rule 2: a swing will physically hit allies. Whether it *damages* them is a design choice (see Open questions); the physics happens either way.

## Unity setup and gotchas

We use the same Unity 6000.0 + URP setup as Hexamus and the built-in PhysX 3D physics. These are the starting settings; each one prevents a specific failure we would otherwise hit in the first week.

| Setting | Start at | Why |
| --- | --- | --- |
| Fixed timestep | 1/60 s (try 1/90 if jittery) | Joint chains and fast swings get more stable with smaller steps; cost rises with it |
| Solver type | Temporal Gauss Seidel | Holds jointed chains together noticeably better than the default solver |
| Solver iterations (body parts) | 12–20 position, 4–8 velocity | Stiffer joints, less stretching; set per Rigidbody so props stay cheap |
| `maxAngularVelocity` (limbs, weapon hand) | About 50 rad/s | Unity's default of 7 rad/s silently caps fast swings |
| Max depenetration velocity | About 3–5 m/s | Stops overlapping bodies from launching each other |
| Interpolation | Interpolate on every visible body | Smooth motion between physics steps |
| Collision detection | Continuous Speculative on weapon hands; Discrete elsewhere | Fast rotating swings don't tunnel; everything else stays cheap |
| Sleeping | Off for living units; on for corpses and props | Muscle-driven bodies must never fall asleep mid-fight |
| Enhanced determinism | On | Same-machine repeatability for debugging (not cross-machine) |
| Joint preprocessing | Try off if a joint jitters | A common ragdoll stability fix |

**Rules of thumb that save days**

- Never set `transform.position` or `rotation` on a dynamic body; forces, joint targets and the balance assist are the only ways in.
- Never parent a living body under a transform that moves (see The rig).
- Keep connected masses within about 4:1; when one joint still misbehaves, its `massScale` / `connectedMassScale` are the next knob.
- Primitive colliders only — no mesh colliders on body parts.
- Run all of a unit's logic in `FixedUpdate`, in one explicit order, from one controller. Spreading it across components with their own update timing gives one-step lags that look like jitter.

## Prototype and milestones

The prototype is one sandbox scene: a 20 × 20 m floor, two placeholder orcs 2 m apart facing each other, a crate, a barrel, and a small goblin (the same rig scaled to about 60%) waiting off to the side for interference tests. A panel of buttons and sliders drives every scenario.

Each milestone ends with something you can watch, and none starts until the previous one passes its test. Each one fixes a class of problems before the next adds more moving parts — the usual way active-ragdoll projects die is debugging balance, muscles and attacks all at once.

1. **Project and placeholder orc.** Unity project, physics settings, layers, folders. The orc generated from primitives by script, with the final null names. *Test:* the prefab matches the rig conventions; swapping one primitive for a mesh needs no code changes.
2. **Dead ragdoll.** *Build Physical Body*: rigidbodies, colliders, joints and limits. All muscles off. *Test:* dropped from 2 m, the orc collapses believably — no limbs bending backwards, no jitter on the ground, no explosions.
3. **Muscles and balance.** Ghost rig with a breathing idle (as in Hexamus), Slerp drives, balance assist, the push tool. *Test:* the orc stands still with no visible jitter; a push makes it wobble and recover; a hard push makes it stumble.
4. **Stability and reactions.** Stability value, state machine, hit dips, catch steps, re-anchoring, knockdown, get-up. *Test:* pushes of increasing strength give flinch, then stagger with steps, then a fall and a get-up — and never a limp collapse.
5. **Swing on the ghost only.** Attack planner, arc solver, arm IK, body overlay, swing gizmos; physics muscles off for the arm, so we judge the motion alone. A distance slider from 1.2 to 2.8 m. *Test:* every distance gives a sensible, heavy-looking swing whose sweet spot passes through the target's chest; too far leans or steps; too close shows a crowded hit.
6. **The real thing.** Physical swing, the impact system, the victim's reaction. *Test:* orc A swings, the club connects with orc B's chest, B reels back and tries to stay on its feet — your target scenario.
7. **Interference and scale.** The four emergent tests from Interruptions as buttons, then 10 v 10 orcs on the floor with a frame-time readout. *Test:* the crate, goblin and side-hit scenarios behave as described; we know our per-unit physics cost and how many units fit the budget.

**Project layout**

```text
Assets/_Project/
  Scripts/Rig/        UnitRig, BodyPart, AnimRig, PhysicalBodyBuilder (Editor)
  Scripts/Motion/     PoseComposer, TwoBoneIK, AttackPlanner, SwingSolver
  Scripts/Physics/    MuscleController, BalanceController
  Scripts/Combat/     UnitState, ImpactSystem, HitEvent
  Scripts/Debug/      CombatDebug, PushTool, SandboxPanel
  Data/               part, muscle, weapon and attack definitions (ScriptableObjects)
  Prefabs/  Scenes/Sandbox.unity
```

## The Unity project

The prototype lives in [RenierVDWest/physics\_battler](https://github.com/RenierVDWest/physics_battler) (Unity 6000.0.60f1, URP). Milestones 1 to 6 are written in code but have not yet run in Unity, so the first sessions are checking and tuning, in the order of the README's "First run" list.

| Milestone | In code | Still to do in Unity |
| --- | --- | --- |
| 1. Project and placeholder orc | Self-running setup: URP, materials, FBX import, data assets, prefabs, Sandbox scene | Open the project and confirm the setup log is clean |
| 2. Dead ragdoll | `UnitBuilder`: bodies, child colliders, ConfigurableJoints, signed limits | Kill Orc B and watch the fall |
| 3. Muscles and balance | `MuscleController`, `BalanceController`, push tool | Tune standing stiffness and wobble |
| 4. Stability and reactions | `UnitState`, hit dips, catch steps, re-anchoring, settle steps, getting up | Tune the stagger and fall thresholds |
| 5. Swing on the ghost | `SwingSolver`, `AttackPlanner`, two-bone IK, swing gizmos | Judge the swing at several distances |
| 6. The real thing | `Weapon`, `ImpactSystem`, `HitEvent` log | Tune hit speed, damage and knockback |
| 7. Interference and scale | Goblin, barrel, crate and side-hit buttons | Run them, then try 10 v 10 |

**Decisions made while building it**

- **A joint probe instead of trusting memory.** ConfigurableJoint has two sign conventions that are easy to get backwards: which way the drive target turns, and which side of the X limit is "low". At startup two boxes in a private physics scene measure both, and every joint uses the result. The sandbox panel shows what it found.
- **Subtree boost.** Acceleration drives only account for a joint's own child body, so a shoulder carrying a forearm, a hand and a club would sag. Each joint's spring is multiplied by the inertia of everything below it, divided by the child's own.
- **A carry pose for the club.** In the rest pose the club points straight ahead and would reach into Orc B at 2 m. Idle, the hand carries it tipped 15° down and 15° out. The wind-up starts from there and sweeps back along the orc's right side.
- **Settle steps.** A steady unit whose feet have drifted far from their rest spots steps them back under itself, so the stance tidies up after a stagger.
- **Compile check without Unity.** `Tools/CompileCheck` compiles every script against small UnityEngine/UnityEditor/URP stubs, checked against Unity 6000.0's reference source, and runs the 25 EditMode tests (swing geometry, IK, joint maths, rig file). Unity's own compile is still the final word.

## Debug tools

Most of this project is tuning, so the tools come before the features they tune; each lands in the milestone that first needs it.

- **Ghost overlay** — the animation rig drawn as wire shapes over the physical body. The gap between them is the gap between wish and truth, and it explains almost every visual bug at a glance.
- **Push tool** — click a body part to hit it with an adjustable impulse; drag to throw.
- **Time controls** — slow motion from 0.05× to 1×, pause, and single physics-step advance.
- **Swing gizmos** — the planned arc, the sweet spot's path, the predicted target point, the commit moment and the actual contact point.
- **Unit HUD** — a stability bar, the current state, and each muscle group's live strength above every unit.
- **Hit log** — the last 20 hit events with speed, damage, impulse and stability loss.
- **Live tuning** — every definition is a ScriptableObject, editable during Play; changes persist after Play stops, which is what we want while tuning.
- **Scenario panel** — Attack, Reset, distance slider, Hit attacker from side, Drop crate, Goblin into the arc.

## Risks and open questions

The biggest risks are scale and determinism, not the animation itself, and both depend on answers only you can give.

| Risk | Early warning | Fallback |
| --- | --- | --- |
| Joints stretch or jitter under club hits | Milestones 2 and 6: limbs separating, hands vibrating | Smaller timestep, more iterations, mass scales; then switch to ArticulationBody |
| Units look drunk or floaty | Milestone 3: constant wobble at idle | Stronger vertical support, more damping, the head/feet string trick |
| Fights are unreadable | Milestone 7: you can't tell who hit whom | Hit-stop on big hits, clearer telegraphs (longer Load), camera shake, stronger lift on knockbacks |
| Too expensive for full battles | Milestone 7 frame time | Fewer bodies for small or distant units, simplified physics for idle units, cap on units per battle |
| Outcomes differ between machines | Any networked or replayed fight | Run each battle on one machine and stream or record it, or keep battles local |
| Balancing chaos | Playtests: the same matchup swings wildly | Lean on stats for the average outcome and let physics decide only the margins |

**Open questions** — my working assumption in brackets; I'll go with it unless you say otherwise.

1. **Target platform?** \[PC.\] Mobile would cut the unit budget sharply.
2. **How are battles played against others?** \[Single-player, or each battle simulated on one machine.\] Underlords-style async PvP that re-simulates fights elsewhere doesn't work with non-deterministic physics.
3. **How many units per battle?** \[Up to about 20 in total.\]
4. **Unit scale and style?** \[Real-world scale, orc about 2.0 m.\] Not Hexamus's 0.8 m chibi scale, for physics reasons.
5. **Placeholder orc?** \[I generate it from Unity primitives by script, with the final null names.\] Your Softimage/Houdini parts drop in later.
6. **Friendly fire?** \[Allies get hit physically and lose stability, but take no damage.\]
7. **Board movement?** \[Units move freely in continuous space during battle; the board is only for placement.\]
8. **Camera?** \[A high, angled camera like Underlords.\] That would favour big, readable body reactions over subtle limb detail.
9. **Project name and repo?** \[A fresh repo, Unity 6000.0 URP, same as Hexamus.\]
