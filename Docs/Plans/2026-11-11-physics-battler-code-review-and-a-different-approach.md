# Physics battler: code review and a different approach

> Copy of the Claude document <https://claude.ai/artifact/5YY9GpcDGQRLVyZPRJjcsP>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 11, 2026 · @Renier

## The short version

**The foundation can deliver one of your four goals, and the last week was spent imitating the other three.** Animation-guided combat is real and good: the mocap walk, the strikes, the combos. But the body is an animated puppet, not a physical one. Physics is switched off or overruled at exactly the moments you care about: the strike, the block, the parry, the hit.

Three facts from the code and from your own playtest logs carry that verdict:

- **The physical body copies the pose.** In your 14:16 playtest of 6 November the legs sat on their pose exactly, and the torso and head within 1.1 cm for 95% of steps. A hit cannot move a body that is put back on its pose 60 times a second, so every reaction is drawn onto the pose by a script.
- **Weapons and shields don't exist for the physics engine.** Their colliders are triggers, and the striking and blocking arms are made kinematic. Nothing can physically stop a blade. Each block, parry and "blade stops in the body" is a prediction followed by rewinding the clip.
- **The work was done blind.** Every headless number came from a stand-in for the physics engine with no collisions and no joints. The code that runs in your game (the clip fighters) has no pass/fail tests at all.

**What I would do instead:** keep the animation as the *intent* and keep the legs and the walk as they are. Make the upper body, the sword and the shield truly physical: driven toward the clip by muscles of limited strength, with solid colliders, at a higher physics rate. Then a sword stops because a shield is in its way, and the struck part moves most and its neighbours less because joints connect them. The animation then listens to what the body actually did.

The plan below has six steps. The first makes real physics measurable, and the next two are cheap experiments that decide whether the rest is worth doing. Nothing that works today is removed until its replacement passes your playtest.

## What you asked for

Four goals, as you put them, each judged against what the code does today.

| Goal | What it needs | Today |
| --- | --- | --- |
| Combat guided by the animations | Clips as the source of every movement | **Met.** Mocap walk, guard, strikes and combos, retargeted to each body. The strongest part of the project. |
| Bodies behave realistically when hit | Momentum passing through a jointed body with mass | **Imitated.** A reaction clip, a bend spring drawn on the pose, and the root slid back. The code says why: "the pinned body is pulled back to its pose each step, so a kick on the physical part alone hardly shows." |
| Objects never intersect | Solid colliders that the physics engine keeps apart | **Not possible as built.** Blades and shields are triggers; kinematic arms pass through anything. Intersections are avoided by predicting them, one case at a time. |
| Sword and shield arms as in a real fight | Arms that aim and also yield: bind, get pushed, recover | **Half.** The parry positions come from fencing manuals, a good idea. But the arm is locked to its pose while it strikes or parries, so it can't give, be beaten aside or bind. |

The project's first prototype was the opposite extreme: a full active ragdoll with muscles and a balance assist, kept on `backup/active-ragdoll-v1`. It was abandoned for an animation-first spec. The swing from all physics to no physics skipped the middle, which is where games with this kind of combat sit.

## How it is built today

Each unit is two bodies: an animated skeleton that code poses 60 times a second, and a physical body of 16 rigid parts that is made to copy it. Every decision and every outcome happens on the animated side. One physics step runs like this:

1. **The fight loop** picks a target, where to walk and when to strike (`FightLoop`, the tempo).
2. **The walk** plays and blends mocap clips, fits them to the body and locks the feet (`Locomotor`, `LocoBody`).
3. **A strike** is a clip aimed at the target. Its blade path is worked out ahead of time, and the defender reads that path to plan a shield or parry position (`ClipFighter`, `SwordParry`).
4. **The hit finder** sweeps the blade against the other bodies' poses. Rules decide block, parry or hit, the damage, and whether the blade stops. A stopped blade is shown by winding the clip back (`StrikeHits`).
5. **The reaction** is scripted: a hit clip, a bend spring on the pose, recoil springs, and the root pushed back.
6. **Last, the physical body is moved onto the pose.** Legs are kinematic. Torso, head and arms have their velocity set toward the pose. The striking arm and the blocking arm are kinematic too.

Physics decides the motion in only three cases: a body knocked down or killed, a severed limb, and a dropped weapon.

| Part of the code | Lines | Role |
| --- | --- | --- |
| Clip fighters (`Locomotion/`, `UnitClips`) | 16,300 | What runs in your game |
| Old pose pipeline (`Motion/`, `Combat/`, much of `Units/`) | about 12,600 | The first fight system, kept behind a switch |
| Fight loop, perception, rig, sandbox, audio | about 17,000 | Shared by both |
| Diagnostics | 3,700 | Playtest logs |
| Headless tools | 13,800 | Reports and the stand-in engine |

That is 49,600 lines of game code in 294 commits over 8 days (30 October to 6 November).

## Base assumptions worth questioning

Seven assumptions hold the design up. The first two cause most of the struggle; the rest made it hard to see.

| Assumption | Where it shows | What I would assume instead |
| --- | --- | --- |
| **The animation is the authority and physics follows it exactly** | Legs kinematic; torso, head and arms forced onto the pose every step; striking and blocking arms kinematic | The animation is the *intent*. For the upper body and the weapons, physics is the truth, and the animation reads it back. |
| **A hit is a script: detect on the pose, decide by rules, play a reaction** | Every pair of things that can touch needs its own detector and its own reaction: blade on body, blade on shield, blade on blade, shield on body, body on body. `StrikeHits` 1,716 lines, `SwordParry` 1,363, `ClipFighter` 3,071; 467 tuning constants | Contacts come from the physics engine. Rules only read them: was it an edge, how hard, on which part. |
| **A fast swing can't be followed physically, so lock the arm** | "Pinned, the physical sword trailed a 16 m/s swing by tens of centimetres." True at 60 steps a second with a soft servo | Run physics faster (a 16 m/s tip moves 27 cm per step at 60 Hz, 7 cm at 240 Hz) and give the muscles the clip's own speed to follow. Don't turn physics off at the moment of contact. |
| **Headless numbers describe the physics** | The stand-in engine has no collisions and no joints (the README says so). Every gate in every plan was measured on it | Physical claims are measured in real PhysX, automatically, or not claimed. |
| **Anything may move the root** | Keeping bodies apart, drive-back, the combo's pull, giving ground and snaps all shift the root while the feet are locked. Planted feet drag 22 to 43 cm | The walk is the only thing that moves the root. A push is a speed the walk must absorb by stepping. |
| **Everyone is an expert, and balance comes from adding rules** | Clip fighters block and parry at skill 1; the profile's skill of 0.5 is never read. In your 14:16 playtest 70% of 110 blows ended on a shield or sword, with 3 deaths in 125 s. Concentration was then layered on top | Outcomes come from reach, timing and arm strength. Skill is reaction time and aim error. |
| **One run tells us if a change helped** | One seed is one fight, and small changes flip it: the outnumbered unit lasted 17 s on one seed and 60 s on another. Step 8 of the last plan was judged on 5 runs a side: 175 s against 202 s, inside the noise | 20 or more seeds a side, and the spread reported with the average. (Checked on 11 November: the live path repeats a seed exactly; only the old path drifts.) |

## Code review findings

Nine concrete problems, most serious first. The playtest figures are from your real Unity runs of 6 November (14:16 and 13:14), which log every part's physical and posed position at each step.

| Finding | Evidence |
| --- | --- |
| **The live code has no tests** | None of the 157 tests touches `ClipFighter`, `Locomotor`, `LocoBody`, `StrikeHits`, `SwordParry` or `ComboPlayer`. All cover the old path. 15 fail and were accepted as the baseline. One says it plainly: "12 steps after the hit the blade is still in the Chest." |
| **Physics was never measured** | The headless engine (`Stubs/Engine/Physics.cs`, 375 lines) lets a ragdoll's parts fall separately onto flat ground. No collisions, no joints. |
| **Pinned parts overrule the solver** | `AuthorityController` writes each pinned part's velocity directly every step. Whatever a contact did to that part is overwritten on the next step, so it pushes others like a wall and can't be pushed itself. |
| **Two truths about where the shield is** | The block is planned on the pose but happens on the physical hand's collider. The physical shield hand was more than 5 cm off its pose in 22% of steps, and only 15% of those were within a second of being struck. The rest is a soft servo lagging a moving pose. |
| **Blades have no speed** | 101 of 110 blows in the 14:16 run registered at exactly 16.0 m/s, the cap. Damage, weight and severing all read a number that is the same for nearly every blow. |
| **The root has many owners** | Five systems move it besides the walk. This is the cause of the dragging feet, and it can't be tuned away. |
| **Two fight systems in one code base** | About 12,600 lines of the old pose pipeline remain, with 46 either-or branches across 10 files. Every change has to be reasoned about twice. |
| **Hidden global state** | 34 public static values that code changes at run time, 28 static events and 12 environment-variable switches read by game code. A test can change the next test's result, and a setting changed in one place changes every unit. |
| **Cost per unit is high** | 6 units took 119 ms of game code per second of play, plus 74 ms of arm solving, with 316 garbage collections in 125 s. Lists and strings are built every step. An auto battler with 20 units, or a faster physics rate, would not fit as it stands. |

Two things are well made and worth keeping whatever comes next: the clip pipeline (baking, fitting clips to other bodies, foot locks, blending without averaging) and the playtest recorder, which made this review possible.

## Different approaches

There are four ways to build this kind of combat, and the project has tried the two ends. I recommend the second.

| Approach | What you get | Cost and risk |
| --- | --- | --- |
| **A. Animated puppet with scripts** (today) | Full control of every pose | Each new interaction is a new script. Real contact and "never intersect" stay out of reach. |
| **B. Physical upper body on animated legs** (recommended) | Blades and shields that really collide; hits that move the body by its mass; arms that give and bind. The walk stays as it is | Tuning muscle strength; a faster physics rate; two experiments needed before committing |
| **C. Whole body simulated, following the animation** | The most lifelike: balance, stumbles and falls all emerge | Balance is the hard part. Your first prototype tried it by hand. Research does it with a trained controller ([DReCon](https://www.ubisoft.com/en-us/studio/laforge/news/VjEIwquaIyEZZSw5RZI0V/drecon-datadriven-responsive-control-of-physicsbased-characters), Ubisoft La Forge), which needs a training setup |
| **D. Animation only, physics for corpses** | Cheapest, fully repeatable fights | Gives up goals 2 and 3 |

Approach B is how the closest published example works. [Hellish Quart](https://kubold.com/blog/2020/7/24/hellish-quart-a-game-about-historical-fencing), a sword duelling game built on motion capture, "uses active ragdolls and all damage your blade can do is calculated with realtime physics"; "the blades really clash using physics, and the characters use motion captured fencing techniques." B also leaves the door open to C later: the legs can go physical once the upper body works.

&#91;embedded content: today and approach B · who decides what happened\]

In approach B only the upper body, the sword and the shield cross over to physics. The legs and the walk stay on the animation's side.

**What B is made of**, piece by piece:

1. **Muscles with limited strength.** Each upper-body joint gets a motor aimed at the clip's joint angle, with a stiffness and a top force. Unobstructed, the arm follows the clip. Obstructed, it loses. The code already has this for one arm (the `Muscled` level in `AuthorityController`), unused by the clip fighters.
2. **Solid sword and shield.** Real colliders on the blade and the board, with continuous collision so a thin blade can't skip through.
3. **Physics at 240 steps a second**, the pose still at 60, with muscle targets blended between.
4. **The animation listens.** When the hand falls behind its clip because something is in the way, the clip slows or stalls there, then recovers through the guard. This replaces the wind-back code.
5. **Hits read from contacts.** The engine reports where, which way and how hard. Rules classify it (edge or flat, which part) and turn the impulse into damage and severing.
6. **Reactions emerge, with your clips layered on.** The struck part moves most and its neighbours less because joints connect them: your 100, 80, 60 sketch, by construction and in the blow's direction. Hit stop, the hit clips and the stumble stay, triggered by the measured impulse. The legs take the push as a speed and step, as the lab's shove already does.
7. **Defence becomes physical.** The shield and parry positions from the fencing manuals stay as muscle targets. A block succeeds when the shield is actually there. Skill becomes reaction delay, aim error and strength.

One technical choice is open and needs an experiment: Unity's ordinary joints with motors, or its articulation bodies. Articulations are solved so that "there is no unwanted stretch" ([Unity reference](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/articulationbody)), and their motors "can handle very large gains" and "larger mass ratios" ([PhysX documentation](https://nvidia-omniverse.github.io/PhysX/physx/5.4.1/docs/Articulations.html)). But their links can't be kinematic, so joining one to animated legs is unproven here.

## The plan

Six steps. Step 1 gives me real measurements. Steps 2 and 3 are experiments in the lab with a go or no-go at the end. Your standing rules hold throughout: one step at a time, each playtested by you; the walk, your animation packs and the feel of the reactions are kept; every new setting goes on Combat Feel with a tooltip; today's behaviour stays behind a switch until its replacement passes.

1. **Eyes on real physics.** A scenario runner on my side with the real PhysX engine under it in place of the stand-in: fixed scenes (a strike at a standing dummy from eight sides, a shield block, a sword parry, 1 v 1, 3 v 3) run headless. The same scenes run in Unity from one menu item, so your logs can confirm mine. Each step it records three new measures:
   - how deep any two things overlap that never should (blade in a body after a stopped blow, blade in a shield, body in body, weapon in the ground), from the engine's own overlap query;
   - how far each physical part is from its pose;
   - the force of every contact.

   The report runners become pass/fail tests.
   - Gate: a baseline for today's build, so every later claim is a before and after.
2. **Experiment: one physical sword arm.** In the lab, the Medium strikes a fixed post and a hanging weight. The weapon arm is on muscles, the blade is solid, physics runs at 240 steps a second. Two builds are compared: ordinary joints with motors, and an articulation.
   - Gate: swinging at nothing, the blade tip stays within 3 cm of the clip's at the fastest strike.
   - Gate: striking the post, the blade never goes more than 1 cm into it and stops without ringing.
   - Gate: 1,000 strikes without a jitter or a blow-up, and the cost per unit measured.
   - Gate (you): the swing still looks as crisp as the clip.
   - No-go: keep today's locked arm and improve contact another way. Steps 1, 4 and 6 still apply.
3. **Experiment: a hit on a physical body.** The dummy's spine, chest, head and arms are on muscles. The step 2 sword strikes it from eight sides at three weights.
   - Gate: the struck part moves most, each part further along the chain less, in the blow's direction.
   - Gate: the body is back on its clip within about 0.6 s without wobbling; pushed past its feet, the walk steps.
   - Gate (you): it reads as contact, better than today's bend. Hit stop and your hit clips stay as layers.
4. **The animation listens, and the walk owns the root.** A blocked blade stalls where it is blocked, binds, and recovers through the guard. Pushes of every kind reach the body only as a speed the walk absorbs by stepping.
   - Gate: planted feet stay within 2 cm through a 3 v 3 fight (today 22 to 43 cm).
5. **Shield and sword arm as a real fight has them.** The manual's guard and parry positions become muscle targets; shield and blades are solid against each other. Skill is reaction delay, aim error and strength, read from the unit's profile.
   - Gate: no step with a blade through a shield or through the other blade.
   - Gate: block rates measured at skill 0, 0.5 and 1 over 20 fights each; your playtest.
6. **Into the fight, and the clean-up.** Damage and severing read from contacts. The scripts this replaces are deleted. The old pose pipeline goes to a branch, as the first prototype did. `ClipFighter` is split into walk, attack, defence and reactions. Per-step garbage is removed.
   - Gate: the live code is under about 20,000 lines with tests on every part.
   - Gate: 6 units, your battle size, hold 60 frames a second on your machine with half of each frame to spare.

**Progress, 11 November: the real engine now runs under my tests.** On the branch `physical-body`, the headless runner has PhysX 5 under it in place of the stand-in. The whole fight runs on it with physical bodies, and a seed repeats exactly. It stands in for Unity closely, measured three ways:

| Check | In Unity | On my side |
| --- | --- | --- |
| The game's own joint probe (which way a motor turns a part, which side a limit stops it) | drive standard, limit sign -1, speed signs -1 -1 -1 | the same |
| Torso and head, distance from the pose (median, 95%, 99%) | 0.14, 1.12, 2.87 cm | 0.14, 1.05, 2.33 cm |
| Shield arm, the same | 0.62, 9.70, 17.1 cm | 0.51, 7.81, 15.8 cm |
| Weapon arm, the same | 0.00, 1.97, 6.22 cm | 0.00, 2.48, 5.34 cm |
| A ragdoll's joints pulled apart, at worst | 2 to 12 cm | 2 cm in a plain fall |

The Unity figures are from your playtest of 6 November 14:16; mine are from a 60 s fight of six Mediums. Found on the way: the test harness's project files had never been committed, so a fresh copy of the repository could not build the tests. They are in now.

**Step 1 is done: the baseline for today's build.** In 60 s of six Mediums fighting on the real engine (three seeds), counting only living units on their feet:

| What passes through what | How often | Deepest |
| --- | --- | --- |
| A blade more than 2 cm into another's body | 90 to 116 times, 3 to 4 s in all | 11 to 13 cm |
| A blade more than 1 cm into a shield | 77 to 107 times, 3 to 5 s | 19 to 26 cm, through the board |
| A blade through another blade | 9 to 17 times | 2 cm |
| A blade into its owner's own body | 43 to 66 times, 3 s | 10 cm |
| A shield into another's body | 90 to 100 times, 6 s | 13 to 17 cm |
| A body part into another's body | 14 to 25 times | 5 to 10 cm |

Headless, a sword is a thin capsule its length and a shield is its board. These are the numbers the later steps have to bring to zero.

**Step 2, first results: contact works, following the clip does not yet.** I ran the Medium's eight strikes with the sword arm held several ways, swung at nothing, at a fixed post and at a free 20 kg ball.

|  | Today's locked arm | Arm on motors, 240 steps a second |
| --- | --- | --- |
| Blade into a fixed post | passes straight through | at most 0.5 cm |
| A 20 kg ball left moving at | 9 m/s (a locked arm pushes as if it had no limit) | 0.2 to 0.7 m/s |
| Point of the blade behind its pose, slow parts of the blow | 0 cm | 5 to 15 cm |
| The same, around contact | 64 to 100 cm for three frames | 40 to 50 cm |
| Blow-ups | none | none |

Four things this taught me:

- **The clips swing faster than an arm can.** Around contact the posed blade's point moves at up to 67 m/s for two frames, about three times a real cut. Even today's locked arm is held to 15 m/s, so the sword you see is already up to a metre behind the pose that decides the hit. No physical arm should follow that; the fastest frames need slowing to a real cut's speed.
- **The motors already in the code can't hold a sword.** PhysX sizes them for the lighter of a joint's two parts, so the wrist's motor is built for the forearm, about a sixtieth of what a hand with a sword needs. Sized for what actually hangs from each joint they do much better, but the arm still wobbles 5 to 15 cm.
- **The arm needs a torso that can't be turned.** With the torso held as it is today, the arm's motor twisted the chest by up to 84 degrees. The upper body has to be one chain anchored to the hips.
- **60 steps a second is not enough for contact.** The blade went 5 cm into the post at 60 and 0.5 cm at 240.

So the gate "within 3 cm of the clip's" was the wrong test: the clip itself is not physical. The verdict for ordinary joints is a half pass. Next is the other build the plan named, the articulation, whose motors can be made far stiffer, together with slowing the blow's fastest frames. Removing the old pose pipeline is still waiting; I put the experiment first because it decides the direction.

**Step 2, second build: the articulation passes, once the blade has a speed limit.** I built the same sword arm the other way the plan named (chest, upper arm, forearm, hand and a solid 1.2 kg sword as one PhysX articulation, its motors aimed at the clip's joints) and ran the same eight strikes at nothing, at a post and at a 20 kg ball, 240 physics steps a second.

|  | Blade's point off its pose in the wind-up | Through the blow, at most | At contact | Deepest into the post | Ball left moving at |
| --- | --- | --- | --- | --- | --- |
| Ordinary joints with motors, limit 30 m/s | 81 cm | 77 cm | 53 cm | 0.2 cm | 0.3 m/s |
| Articulation, strong arm, no limit | 8 cm | 91 cm | 36 cm | 7.2 cm | 0.9 m/s |
| **Articulation, strong arm, limit 30 m/s** | **8 cm** | **44 cm** | **10 cm** | **0.1 cm** | **0.9 m/s** |
| Articulation, motors with no limit on their strength, limit 30 m/s | 7 cm | 19 cm | 9 cm | 0.1 cm | 2.9 m/s |
| Articulation, strong arm, limit 30 m/s, 120 steps a second | 14 cm | 45 cm | 13 cm | 7.7 cm | 0.7 m/s |

"Strong arm" is a shoulder that turns with 600 N m, an elbow with 420 and a wrist with 150, about five times a person's. No run blew up.

What this settles:

- **The articulation is the build to use.** It holds a slow pose within 8 cm at the point of a 90 cm blade, meets its target within 10 cm, stops on the post and gives the ball the push of a real blow. Ordinary joints are 40 to 80 cm off even in the wind-up.
- **The 67 m/s is in the clips themselves.** In the forehand the hand turns 83 degrees between two frames; the wrist's steer onto the target adds the rest. No arm of any believable strength follows that, and at that speed the blade also outruns the engine (7 cm into the post).
- **So a strike's blade now has a speed limit**, a new Combat Feel field, `mostBladeSpeed` (under "A blow's speed"; 0, the default, is off and changes nothing). With it set, the blade's point never goes faster than the limit, and the strike's clip is slowed over its fastest frames so the body waits for its blade. At 30 m/s a blow lands 0.05 s later than its clip does; a real cut's point goes 20 to 30 m/s.
- **Strength is what makes contact right.** Motors with no limit follow the clip closest, but they fight whatever they meet: the hand goes on at 37 m/s after the post and the ball is launched at up to 7 m/s.
- **240 steps a second are needed.** At 120 the blade is 8 cm into the post.
- For two or three steps in the fastest part of a blow the physical blade is up to 44 cm behind its pose, then back on it. That is the arm's strength showing, and it is the part your eyes have to judge.

One thing I tried and dropped: slowing the clip by how far the physical blade has fallen behind (the animation waiting for the arm directly). The pose jumps a metre in one step before the arm is behind at all, so it reacts too late and the arm overshoots. The speed limit works ahead of the arm instead.

Not measured yet: the cost for six units, a run of 1,000 strikes, and any of this in Unity itself. In a headless fight played with the limit on (the old posed way), parries fell from 27 to 12 in a minute: the defender's reading of when a blow lands doesn't know about the slowed clip yet. That gets fixed when the arm goes into the fight.

If you want an early look at how the slower blow reads, on the branch `physical-body` set Combat Feel > A blow's speed > Most Blade Speed to 30 and play a fight. It only changes the timing of the strikes; the arm itself is not in the game yet.

**The old pose pipeline: the first part is out, the rest needs cutting by hand.** On the branch every unit is now a clip fighter. The switch between the two fight systems is gone, and so is the sandbox's "Clip motion" toggle.

- Gone: the old path's stepping, the 30 places that chose between the two systems, all 157 old tests, 26 headless reports that drove only the old path, the Move Preview window, and about 280 functions and values that nothing reaches any more. The game's code is 41,900 lines where it was 46,400, and about 10,700 lines of tests and reports went with it.
- Checked after each stage: the clip fight's event logs on eight fights (three seeds, two line-ups, posed and on the real engine) are the same line for line as before the removal.
- Still there: about 11,500 lines in the old motion and combat folders. Live code still reaches them, so no rule can take them out. The sandbox's scenarios and the recorder use the old moves, the fight loop uses the old tempo, the clip fighter's get-up uses the old one, and the set-up menus build the old moves, postures and rhythms (37 assets). Each is a cut to make by hand with the logs compared after it. I will make them as the steps ahead replace what they hang on.

Two things you will notice in Unity on the branch. A spawn marker's preview shows the body at rest in its team's colour, unarmed, because the posed preview was the old pipeline's. And there are no unit tests for now: the old ones tested code that is gone, and the live path never had any. New ones come with steps 3 to 5.

**Step 3: a hit on a physical body. It is steady, and a real-sized reaction is small.** A Medium stands in its guard with its legs on the animation and its whole upper body (pelvis to head, both arms) as one physical body on muscles. I struck it with a ball of a known weight: 3 kg at 8 m/s stands for a sword's blow (the blade gave about that much in step 2), 20 kg at 8 m/s for a Large's club.

| A sword's blow to the chest | Head moved | Hands moved | Back on its pose | Its feet took it |
| --- | --- | --- | --- | --- |
| Today's body | 2.3 cm | 1.5 cm | 0.07 s | 0 |
| Upper body on muscles | 0.5 cm | 1.5 cm | at once | 0 |
| The same, and the legs take the blow | 1.1 cm | 4.1 cm | 1.4 s, walking | 11 cm |
| The same, muscles let go for a moment when struck | 1.7 cm | 5.2 cm | 1.4 s, walking | 11 cm |

- **Today's body swallows the blow.** Even the club's blow, six times a sword's, moves the chest 9 cm and it is back in a quarter of a second with its feet where they were. The blow's momentum just disappears.
- **The physical upper body is sound.** It never blew up, even under the club, and it never swung back and forth. It is back on its pose within 0.1 to 0.2 s.
- **It is also stiff, as a braced body is.** A sword's blow moves the struck part about 1 cm, and almost all of the blow arrives at the hips. A torso soft enough to fold for that blow could not follow its own animation.
- **So what a blow does to a body is mostly what it does to its feet.** When the blow goes to the walk as a change of speed (its weight over the body's mass, which the walk already answers with steps), a sword's blow takes the body 11 cm back, a heavy one 29 cm, and the club 90 cm at 1.5 m/s, which in the fight is off its feet.
- **Muscles that let go for a tenth of a second when struck** throw the arms and head two to three times as far, still without wobbling.
- One thing the plan's gate had wrong: the struck part is not the one that moves most. The free ends are, the hands.

What this means for the plan: the body's own reaction to a sword's blow, done truthfully, is small. A hit will read through the legs taking it (step 4, next), the muscles letting go for a moment, and what you already layer on top (hit stop, your hit clips). If you want more than the truth, there is one honest dial: a blow's weight scaled up. That is for your eyes in Unity later.

One build note: the engine's own motors size themselves for the one part below a joint, so a spine set to a sensible stiffness let the body sag 63 degrees. I size each motor by everything that hangs from its joint instead.

Not in this yet: the blow from the physical sword itself (the ball stands in for it), the shield, a blow to the legs, and anything in Unity. I drew three short films of these blows as stick figures at a quarter speed, sent with this note in the chat.

**Step 4: feet that don't slide.** This one changes the game on the branch, so it is the first you can look at in Unity.

I added a measure to my fight runs: how far a foot moves along the ground while its animation says it is planted. Six Mediums, three fights of 60 s:

|  | Worst single slide in each fight | Blows that met something |
| --- | --- | --- |
| Before | 18, 10, 55 cm | 252 |
| Now | 2, 2, 3 cm | 252 |

The cause was narrower than the review said. Of the things that move a body without walking it, one did nearly all of it: a combo's strike carrying its body in after the target, 24 m a minute across six units, at up to 7.2 m/s. A planted foot was let go when its leg could no longer reach and was then dragged along the ground after the body, 55 cm in a sixth of a second at the worst.

Two changes, both new Combat Feel fields under "Feet", both with tooltips:

- **Combo Pull Speed** (3 m/s, it was 7.2). How fast a combo's strike may carry the body in. At 3 the body still gets as far and as many blows are met; at 2 blows start falling short.
- **Moving Foot Lift** (7 cm, 0 turns it off). A foot moving over the ground is lifted like a stepping foot. A foot is then either planted or in the air.

What to know before you judge it:

- My measure counts a foot as sliding only within 1.5 cm of its standing height. A lifted foot counts as a step however quick it is, so the numbers can't tell you whether those quick steps look right. That is for your eyes.
- On the real engine the worst slide in a 30 s fight was 10 cm, once, in a walk. The rest were 1 to 2 cm.
- Giving ground to a blow and keeping two bodies apart still move a body directly, about 1 m a minute between them. With the lift they no longer drag a foot, so I left them.
- Fights no longer play out the same as before this step for a given seed.

To look at it: open the branch `physical-body` in Unity and play a fight. Watch feet during combos. To compare with before, set Combo Pull Speed to 7.2 and Moving Foot Lift to 0.

**Step 5, first part: the physical upper body is in the fight, behind a switch.** With Combat Feel > The physical upper body > Physical Upper Body on, each fighter's hips, torso, head and both arms are one jointed body on muscles of limited strength, with a solid sword and a solid shield, on legs the animation still moves. It is off by default, so the branch plays as before until you turn it on. It has not run in Unity yet: everything below is from the headless engine.

What passes through what, six Mediums, three fights of 60 s, all three together:

|  | Held to the pose (today) | Physical upper body |
| --- | --- | --- |
| A blade in another's body | 195 times, 7.2 s, up to 12.7 cm | once, 2.5 cm |
| A blade in a shield | 177 times, 9.1 s, up to 25 cm | 8 times, 0.4 s, up to 6 cm |
| A blade through a blade | 33 times, 0.7 s | 7 times, 0.1 s |
| A shield in another's body | 172 times, 12.9 s, up to 16.6 cm | never |
| A body in another's body | 24 times, 1.2 s | 11 times, 0.8 s (legs only: they are still the animation's) |

Four things I found on the way, each of which had bodies passing through each other or being thrown about:

- A fault in my own test rig, not in the game: with six units on the field, many pairs of shapes were detected by the engine but never actually stopped. Unity does not have this fault. It took a while to find, and it means any six-unit figure from before it was fixed was wrong; the table above is from after.
- The shield was turned in the hand by placing it, up to 47° in one step, straight through whatever was in the way. It is now its own part on a joint at the grip, turned by a motor.
- A unit that fell or died kept its physical upper body standing, invisibly, and the living fought it. It is now let go on the fall.
- The muscles need about 600 N m to swing a sword as fast as the animations do, which is several times a man's strength, and the hips give way to nothing. Pressed against a body, that folded it in half. Three rules now handle this, each a Combat Feel field with a tooltip: a weapon arm that is stopped gives (Blocked Arm Strength), a body pressed on is moved back by its feet (Pressed Back Speed), and a body borne right down falls and gets up (Borne Down From).

What to expect when you look, and what is not done yet:

- A blocked strike stops on the shield while the animation plays the strike out, then the arm comes back. Hits are still decided by the animation's blade, not the physical one, so now and then a blow will count that the eye saw stopped. Deciding hits from the contacts is step 6.
- The legs are still the animation's and push with unlimited strength. A leaping attack that lands on someone bears him down.
- A fighter's own blade and shield still pass through his own body.
- Speed is unknown in Unity. Headless it costs about four times the engine time of today's fight.

**To look at it:** pull `physical-body`, tick Physical Upper Body on the Combat Feel object, and press Spawn. Six Mediums is what I measured. Two things from the Console would help me most: the line starting `[Articulation probe]` (it says whether Unity's joints behave the way my headless engine assumes; if they do not, the physical body stays off and the line says so), and a playtest log of one fight.

**Step 5, second part: your first look, and what it found.** You saw the upper bodies spin like fan blades. I had only ever run six Mediums headless; your scene has two Larges with clubs. With your line-up the headless engine did the same thing, so I could take it apart there. Unity's probe line matched the headless engine on every count, which means what I measure headless is what Unity does.

The causes, in order of how much they mattered:

- **A turn has many names.** A joint's motor pulls three numbers toward the target's three numbers, and the same turn can be written as half a turn one way or half a turn the other, 360 apart. A shoulder that passed half a turn was sent the long way round at full strength, then round again. This was the fan. Each target is now written the way nearest to where the joint actually is.
- **Muscles at their limit.** I had capped every muscle at 600 N m so that a stopped blade would not shove too hard. But the animation itself asks for far more: a hit reaction snaps the torso over ten times faster than 600 N m can turn it. A muscle at its cap falls behind, then overshoots and swings. With the bodies touching nothing at all, heads were still 19 cm from the animation 5% of the time. Each muscle is now allowed, on top of its strength, exactly what the animation's own motion takes. Touching nothing, heads are now within 1 cm. The 600 N m only sets how hard a body pushes on what stops it.
- **The Large.** Its muscles were a fifth of what its body and club take to turn. They now scale with weight as well as size.
- **Joints the animation flips.** When an arm is straight, the animation's roll of that arm about its own length flips by up to 170 degrees in one step. No muscle can follow that, so such a joint is now put there directly. It happens 140 to 300 times in a fight of six.
- **The ground.** Rising cuts swing the blade through the ground, which stopped it and held the arm a metre back. The physical body now ignores the ground.
- **A rule of mine that made it worse.** The "stopped arm gives" rule also weakened any arm that was merely behind in a fast cut, so it never caught up. It now applies only when the hand is nearly still.

I also fixed an error from your log: a fallen body thrown into a standing fighter went through old code that no longer has a body to pose, and threw an exception.

The same measure as before, three fights of 60 s with your line-up (Swordsman, Medium with shield, Large with club a side):

|  | Held to the pose (today) | Physical upper body |
| --- | --- | --- |
| A blade in another's body | 456 times, 16.0 s | 10 times, 0.2 s |
| A blade in a shield | 160 times, 6.4 s | 8 times, 0.3 s |
| A blade through a blade | 63 times, 1.4 s | 11 times, 0.2 s |
| A shield in another's body | 175 times, 10.5 s | never |
| A body in another's body | 84 times, 1.9 s | 18 times, 1.1 s |

And how closely the physical body follows the animation, which I was not measuring properly before: the head is within 2 cm 95% of the time and within 4 to 6 cm 99% of the time. The weapon hand in a strike is within 2.5 cm half the time and within 9 to 19 cm 95% of the time. At the worst, for moments, a head is 36 to 75 cm off and a hand over a metre. As a safety net, a body that stays that far off for half a second is put back on its pose; this did not trigger in six fights.

In your session a frame cost 11.4 ms with the physical body and 7.7 ms without. That was with the bodies whirling, so expect it to be lower now, but I have not measured it.

**To look again:** pull `physical-body`, same switch, Spawn. I would like another playtest log either way.

**Progress, 11 November (night): step 5, after the second look in Unity.** The bodies still whirled in Unity, between two Mediums as well, though headless the same fights stayed whole. What was found:

- **The cause, as far as it is known.** A motor at its strength limit does not damp. A Medium's shoulder is at its limit a third of a degree from its pose; past that it pushed toward the pose with all it had and the damper's share was lost. A joint knocked off its pose was thrown at it and past it. Unity's log shows a head nodding 20° to 44° a step, ten times a second, then going right round, which is what an undamped neck of that strength does. A joint that had once gone round kept going. Headless had the same in small (a wrist swinging 70° to and fro, a shield hand 2 m off for a moment) and it died out there in a third of a second.
- **Not known:** why Unity keeps the swing growing where the headless engine lets it die. The motor and what it is asked are the same; the two engines are different builds of PhysX. So this is not yet proven fixed in Unity.
- **The change.** A joint off its pose is now asked for a speed toward it (20 rad/s at most, `returnSpeed` on Combat Feel, and no faster than it can stop from), so the damper works whatever the spring does. Thrown out of hand on purpose (every joint set turning at 150 rad/s), a body used to take a third of a second to settle and now takes a tenth.
- **Also:** the pose's speed of turn was five times out for a shoulder past half a turn; no joint may turn faster than 90 rad/s (was 200); a part inside another is pushed out at 3 m/s at most; a body with a joint over 60 rad/s for a fifth of a second is put back on its pose.
- **Headless figures, six fights:** the head within 1.4 cm of the pose 95% of the time (was 2), the shield hand within 7 cm (was 10 to 19), the weapon hand in a strike within 13 cm (was 19). A blade was in another's body 10 or 11 times for 0.4 to 0.6 s per line-up, a little more than before (2 and 10 times).
- **For the next look:** Unity measures three more things at start-up and adapts to them (a target speed's unit, the damping, which axes a speed is on), a body out of hand is written to the log as `[Upper]` lines, and the Combat Feel is saved with the session. The two Larges are switched off in the scene.

**Progress, 12 November: step 5, after the third look in Unity.** The bodies no longer went wild, but limbs still spun at points and the arms wobbled like springs after a blow.

- **Seen in the log.** 161 times in 75 s a joint kept turning fast with its pose near still, the sword wrists most. One sword hand swung 80° to and fro, four times a second, for three seconds.
- **Why Unity and not headless (found).** Unity's engine writes a ball joint's turn anywhere from 0° to 360°; the headless engine keeps it under 180°. Past 180° the engine's own spring pushes the joint sideways away from its target. In the log the fast joint was written past 180° in 85 lines of 146. The motors now have no spring at all, so the engine's numbering no longer matters.
- **Slack, as asked.** A blow that turns an arm 15° leaves it 15° aside. It keeps doing what the animation does, that much off, and comes back over about a second (faster from 40°, never more than 50°). The spine, chest and head come back in 0.3 s. An arm is pulled back quickly if its slack would leave it inside its own body or bend a joint past its range.
- **Tunable on Combat Feel:** `slackAngle`, `slackTime`, `tautAngle`, `tautTime`, `giveWayAngle`.
- **Headless, six one-minute fights:** a blade was in another's body 16 and 6 times per line-up (0.4 s and 0.1 s in all); the head within 1.8 cm of the pose 95% of the time; no body had to be put back on its pose.
- **Still open:** not yet seen in Unity. The legs are still the animation's, so an arm's slack is the only give in a body.

**Progress, 12 November (later): the limbs that spun, and the Larges back.** Your last log still had limbs spinning at points. I found two kinds in it, and both came from the animation pose, not from the physics. The physical arm only goes where the pose puts it.

- **A straight arm rolled about its own length** (7 times in your log, 240° to 550° in a fraction of a second). When an arm is nearly straight, the code could not tell reliably which way the elbow was bent. A sword resting on a shield or a body nudged the hand a little each step, and the arm was rolled round to match. Fixed in two places: the elbow now stays on the side it is on, and each arm is solved from its own last result.
- **The shield arm's elbow jumped** (33 times in your log, over 20 cm in one step while the hand hardly moved). The elbow was put behind the shield at once when the shield started to move, and put back at once when it finished. It now moves over smoothly with the shield.

Headless, 60 s fights of six, two seeds each, before → after:

|  | Mediums only | With the Larges |
| --- | --- | --- |
| Arms rolling about their own length | 16 and 9 → 0 and 2 | 12 and 10 → 1 and 2 |
| Elbow jumps | 77 and 53 → 9 and 7 | 34 and 36 → 4 and 10 |
| Joints the pose flipped at once | 376 and 148 → 33 and 46 | 238 and 180 → 36 and 62 |

The strikes aim exactly as before. The two Larges are switched on again in the Battle scene. Headless they behave: a Large's head stays within 2 cm of its pose 99% of the time. I have not seen them in Unity since they were taken out, so that is the thing to watch in your next playtest.

Still there, and not looked into: a sword wrist that flicks round quickly about 14 times a minute across six units, and a swordsman's parrying elbow that jumps when the hand is drawn in close to the shoulder.

Next is the legs, in the plan [Grounded legs: research and plan](https://claude.ai/artifact/P8Du2bu8f7hN8xMaVSY5xP).

**Progress, 12 November (13:06 log, the Larges back): the vibrating weapon arm.** Your log confirms the two limb fixes in Unity: no straight arm rolled, and the shield elbows jumped 7 times where they had 33.

The vibrating you saw was almost all one thing. The weapon hand and its elbow shook for 59 of the 450 unit-seconds in the log, and every other body part together for 11. It was worst on the Larges (20 and 27 of their 75 seconds). The hand turned about 19° back and forth and moved 4 cm with it, twenty to thirty times a second, while the weapon itself stayed on its line. It came from the animation pose, not the physics.

The cause: when a guard asks for a hand angle the wrist cannot quite reach, the hand is allowed to roll around the grip instead. The code that paces the hand mistook that roll for the hand falling behind, pulled the hand back, and then pushed it forward again, every step. It now ignores the allowed roll.

Headless, 60 s fights of six, two seeds, weapon hands shaking before → after: with the Larges 35 and 35 unit-seconds → 2.9 and 1.4; Mediums only 24 and 4.2 → 2.3 and 1.0.

One cost: in the test where a dummy parries 27 set blows, it now parries 21 where it parried 22.

Not fixed: a Large sometimes picks the wrong edge of its club for its guard and holds the club well off the guard's line until the next parry. That was there before this change too.

**Progress, 12 November (afternoon): heavy weapons, a first step.** You said the weapons feel like polystyrene props: the enemy stops the club in its tracks and the club slides and bounces over the body. Your log agrees. Every one of its 94 hits ended with the swing stopped, the hand thrown back and the attack backing out.

Three new dials on Combat Feel, under "Heavy weapons", all exaggerated on purpose:

- **`swingKept` (0.7).** A blow that lands on a body is no longer stopped by it. It carries on along its swing at 70% speed and picks up again. It hits each body once and can go on to hit whoever stands beyond. A shield, a parry or a friend still stops it, but it bounces back much less.
- **`weaponWeight` (3).** The body that is hit reacts as if the weapon were three times as heavy: it bends, leans and is driven back further. Damage is unchanged. A shield is knocked back further and its bearer is driven back harder.
- **`strikeArmStrength` (3).** During a strike the physical weapon arm is three times stronger and has no slack, so the weapon pushes on instead of being turned aside.

Headless, 60 s fights of six, two seeds, before → after:

|  | Before | After |
| --- | --- | --- |
| Club lands on a body: bounced back | 11 of 21 blows | 0 of 17 |
| Club: hips of the body struck, in 0.5 s | 48 and 23 cm | 108 and 65 cm |
| Sword lands on a body: bounced back | 24 of 48 | 8 of 30 |
| Sword on a shield: bearer's hips, in 0.5 s | 5 and 9 cm | 17 and 26 cm |

Not there yet: a sword blow that lands still moves the whole body little. Most sword hits land on a hand or forearm, or on someone in the middle of their own strike. The brief freeze when a blow lands (the hit stop) is unchanged. None of this has been seen in Unity.

## Risks and questions

**Risks:**

- **A muscled arm may look softer than the clip.** A motor always trails its target a little. Feeding it the clip's speed and stiffening it through the wind-up should close most of the gap. Step 2 measures it and you judge it.
- **A thin blade can still skip a thin limb.** Even at 240 steps a second a 16 m/s tip moves 7 cm per step. Continuous collision and a slightly thick blade collider are the answer; step 1's overlap measure will show if they hold.
- **Outcomes get less controllable.** When contact decides, a fight can turn on a blade catching an elbow. That is the appeal, and it makes balancing units harder.
- **PhysX does not repeat exactly** from run to run or machine to machine. See the first question.
- **Cost.** Four times the physics steps for 6 units is unmeasured. Step 2 measures it before anything depends on it.
- **A week of scripts goes.** Roughly 5,000 to 8,000 lines of prediction and reaction code would be deleted in step 6. They stay on a branch.

**Your answers (11 November):**

1. **Fights need not be repeatable.** Spontaneous contact matters more: a swing meant for one enemy that catches another's arm on the way must stay possible. So contact decides the outcome, and step 6 reads damage from it.
2. **Six units** fight at once. That is the cost budget.
3. **I test on my side**, as before, and you playtest and push logs. The better my tests, the better; so step 1 puts a real physics engine under them.
4. **A slightly softer swing is acceptable** if it buys real contact.
5. **The old pose pipeline goes**, and all of this work happens on its own branch, `physical-body`. `main` stays as it is.
6. **Mediums only for now.** The Large and the Small wait.
