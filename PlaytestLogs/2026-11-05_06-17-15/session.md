# Playtest 2026-11-05_06-17-15

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 5.6 s of play (338 physics steps), 466 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-05_06-17-15`.

## A Small

Present 51.0–56.6 s. Steps 18, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 7 cm from the support point; hips dropped for reach by up to 0 cm.

Feet: 15 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 55°, 95% under 79°.
Walking: 4.7 s (100% forward, 0% sideways, 0% backward), the body went 2.4 m and the feet 2.7 m each on average (feet/body 1.15; well under 1 is gliding); the feet point off the line it walks along (either way) median 8°, 95% under 43°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 7 / 68 |
| Chest | Pinned | 1.2 / 12.4 | 9 / 41 |
| Head | Pinned | 1.9 / 15.6 | 13 / 103 |
| UpperArmL | Pinned | 1.9 / 14.4 | 9 / 59 |
| ForearmL | Pinned | 2.3 / 14.6 | 12 / 97 |
| HandL | Pinned | 2.9 / 36.4 | 24 / 83 |
| UpperArmR | Muscled | 2.2 / 15.6 | 11 / 52 |
| ForearmR | Muscled | 3.9 / 19.6 | 17 / 78 |
| HandR | Muscled | 7.9 / 32.7 | 22 / 100 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 1, 5 damage. Health at the end 55.

Pose against the joint limits (standing): a joint more than 10° past its limit 62% of the time; the worst 42° (FootL).

Look: within 25° of what it attended to 85% of the time standing (85% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 4 postures (StickSettle 1, AbanicoFlurry 1, TriangleStep 1, RedondaTwirl 1), 0 of them lead-ins to blows; energy at the end 0.99.


## A Medium

Present 51.0–56.6 s. Steps 12, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 10 cm from the support point; hips dropped for reach by up to 0 cm.

Feet: 9 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 16°, 95% under 72°.
Walking: 2.9 s (95% forward, 0% sideways, 5% backward), the body went 1.8 m and the feet 1.8 m each on average (feet/body 1.00; well under 1 is gliding); the feet point off the line it walks along (either way) median 8°, 95% under 32°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 3 / 12 |
| Chest | Pinned | 1.0 / 3.5 | 8 / 20 |
| Head | Pinned | 2.0 / 5.9 | 14 / 39 |
| UpperArmL | Pinned | 1.9 / 5.5 | 6 / 53 |
| ForearmL | Pinned | 2.0 / 22.3 | 5 / 56 |
| HandL | Pinned | 2.1 / 28.5 | 4 / 66 |
| UpperArmR | Muscled | 2.5 / 7.2 | 10 / 29 |
| ForearmR | Muscled | 5.3 / 14.7 | 16 / 62 |
| HandR | Muscled | 8.9 / 34.2 | 21 / 84 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 2, 17 damage. Health at the end 83.

Defence: 2 blows came at it (1 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 1, set its weapon across 0.

Pose against the joint limits (standing): a joint more than 10° past its limit 28% of the time; the worst 30° (FootL).

Look: within 25° of what it attended to 86% of the time standing (86% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 87% of the time standing; its rim came within 14.2 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 2 postures (GuardChange 1, Peek 1), 0 of them lead-ins to blows; energy at the end 0.94.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Large

Present 51.0–56.6 s. Steps 9, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 29 cm from the support point; hips dropped for reach by up to 3 cm.

Feet: 7 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 24°, 95% under 76°.
Walking: 2.5 s (100% forward, 0% sideways, 0% backward), the body went 1.8 m and the feet 2.0 m each on average (feet/body 1.13; well under 1 is gliding); the feet point off the line it walks along (either way) median 8°, 95% under 29°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 2 / 19 |
| Chest | Pinned | 0.9 / 7.4 | 3 / 9 |
| Head | Pinned | 1.4 / 7.3 | 15 / 60 |
| UpperArmL | Pinned | 1.7 / 9.9 | 6 / 41 |
| ForearmL | Pinned | 4.1 / 24.1 | 11 / 58 |
| HandL | Pinned | 6.5 / 45.0 | 29 / 90 |
| UpperArmR | Kinematic | 1.3 / 8.5 | 3 / 57 |
| ForearmR | Kinematic | 2.1 / 17.0 | 5 / 64 |
| HandR | Kinematic | 4.1 / 52.0 | 7 / 79 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 3 (1 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 27 damage. Hits taken: 0, 0 damage. Health at the end 220.

Pose against the joint limits (standing): a joint more than 10° past its limit 61% of the time; the worst 46° (FootL).

Look: within 25° of what it attended to 88% of the time standing (82% of the time it was within the head's reach, 70% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 1 postures (BoxerSettle 1), 0 of them lead-ins to blows; energy at the end 0.55.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.5 cm, max 0.9 cm (posed); physical weapon's closest pass mean 0.7 cm, max 1.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 54.83 s: ClubSwing at B Small's Chest: passed 0.0 cm from the aim point; nearest part B Small's UpperArmR (4.8 cm); the Chest had moved 25.3 cm since the aim was locked
- 55.57 s: ClubBackhand at B Small's Neck: passed 0.9 cm from the aim point; nearest part B Small's ThighL (17.4 cm); the Neck had moved 47.3 cm since the aim was locked
- 56.52 s: ClubSwing at B Small's Chest: passed 0.7 cm from the aim point; nearest part B Small's HandR (16.7 cm); the Chest had moved 55.5 cm since the aim was locked


## B Small

Present 51.0–56.6 s. Steps 12, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 11 cm from the support point; hips dropped for reach by up to 12 cm.

Feet: 9 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 52°, 95% under 68°.
Walking: 3.6 s (100% forward, 0% sideways, 0% backward), the body went 2.2 m and the feet 1.9 m each on average (feet/body 0.89; well under 1 is gliding); the feet point off the line it walks along (either way) median 8°, 95% under 16°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.0 | 4 / 17 |
| Chest | Ragdoll | 0.7 / 3.4 | 7 / 28 |
| Head | Ragdoll | 1.2 / 5.2 | 8 / 25 |
| UpperArmL | Ragdoll | 1.0 / 4.5 | 7 / 67 |
| ForearmL | Ragdoll | 1.8 / 14.4 | 9 / 96 |
| HandL | Ragdoll | 2.9 / 36.3 | 22 / 83 |
| UpperArmR | Ragdoll | 1.1 / 5.0 | 7 / 43 |
| ForearmR | Ragdoll | 2.3 / 17.7 | 12 / 69 |
| HandR | Ragdoll | 5.4 / 27.5 | 15 / 91 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 3, 27 damage. Health at the end 33.

Defence: 3 blows came at it (0 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 0, set its weapon across 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 71% of the time; the worst 40° (FootR).
Ragdoll (on the ground 1.8 s): a joint more than 10° past its limit 88% of the time; the worst 132° (FootL). Fastest part with a standing leg within 0.5 m: 2.7 m/s median, 9.7 m/s at the 95th percentile (105 steps); with none: - median (0 steps).

Look: within 25° of what it attended to 84% of the time standing (86% of the time it was within the head's reach, 97% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 2 postures (StickSettle 1, AbanicoFlurry 1), 0 of them lead-ins to blows; energy at the end 0.84.

Balance: stumbled 0 times, knocked down 1, launched 0, got up 0; thrown into others 0 times. Still down at the end.


## B Medium

Present 51.0–56.6 s. Steps 6, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 7 cm from the support point; hips dropped for reach by up to 0 cm.

Feet: 5 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 13°, 95% under 65°.
Walking: 2.8 s (100% forward, 0% sideways, 0% backward), the body went 1.9 m and the feet 2.1 m each on average (feet/body 1.09; well under 1 is gliding); the feet point off the line it walks along (either way) median 8°, 95% under 28°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 3 / 9 |
| Chest | Pinned | 0.9 / 2.7 | 6 / 15 |
| Head | Pinned | 1.5 / 4.2 | 10 / 37 |
| UpperArmL | Pinned | 1.6 / 4.5 | 4 / 55 |
| ForearmL | Pinned | 1.5 / 23.6 | 4 / 64 |
| HandL | Pinned | 1.3 / 32.8 | 2 / 61 |
| UpperArmR | Pinned | 1.7 / 6.0 | 8 / 24 |
| ForearmR | Pinned | 4.2 / 12.6 | 13 / 63 |
| HandR | Pinned | 7.3 / 28.2 | 17 / 82 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 2, 19 damage. Health at the end 81.

Pose against the joint limits (standing): a joint more than 10° past its limit 28% of the time; the worst 22° (ForearmR).
Ragdoll (on the ground 1.6 s): a joint more than 10° past its limit 62% of the time; the worst 150° (ShinR). Fastest part with a standing leg within 0.5 m: 4.3 m/s median, 8.9 m/s at the 95th percentile (36 steps); with none: 2.6 m/s median (61 steps).

Look: within 25° of what it attended to 99% of the time standing (99% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 100% of the time standing; its rim came within 11.9 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 1 postures (GuardChange 1), 0 of them lead-ins to blows; energy at the end 0.93.

Balance: stumbled 1 time, knocked down 1, launched 0, got up 0; thrown into others 1 time. Still down at the end.


## B Large

Present 51.0–56.6 s. Steps 11, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 26 cm from the support point; hips dropped for reach by up to 2 cm.

Feet: 9 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 20°, 95% under 73°.
Walking: 2.6 s (100% forward, 0% sideways, 0% backward), the body went 1.6 m and the feet 1.6 m each on average (feet/body 1.00; well under 1 is gliding); the feet point off the line it walks along (either way) median 9°, 95% under 38°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 2 / 26 |
| Chest | Pinned | 1.0 / 10.9 | 3 / 30 |
| Head | Pinned | 1.6 / 9.3 | 21 / 162 |
| UpperArmL | Pinned | 1.9 / 16.1 | 8 / 55 |
| ForearmL | Pinned | 5.0 / 25.1 | 14 / 66 |
| HandL | Pinned | 7.6 / 45.0 | 30 / 106 |
| UpperArmR | Kinematic | 1.3 / 12.7 | 3 / 22 |
| ForearmR | Kinematic | 2.4 / 23.3 | 5 / 64 |
| HandR | Kinematic | 4.1 / 52.0 | 7 / 79 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 3 (1 on the part aimed at, 1 blocked, 0 parried, 0 on someone in the way, 1 on allies), 33 damage. Hits taken: 0, 0 damage. Health at the end 220.

Pose against the joint limits (standing): a joint more than 10° past its limit 36% of the time; the worst 28° (ForearmR).

Look: within 25° of what it attended to 91% of the time standing (91% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 2 postures (BoxerSettle 1, ClubFeint 1), 0 of them lead-ins to blows; energy at the end 0.69.

## Defence (milestone 11)

- Small: pressed 5 s, 0% of it in the shell (gate 70%); 0 of 3 combo links defended, 0% (gate 30–60%); 0 of 3 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.
- Medium: pressed 3 s, 39% of it in the shell (gate 70%); 1 of 2 combo links defended, 50% (gate 30–60%); 1 of 3 blows that met it on its guard (1 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.
- Large: pressed 7 s, 0% of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 2 bursts at it answered with a counter, 0% (gate 30–50%), 0 of them ripostes between links.

## Shield contacts

10 times a shield overlapped another unit's body: 95% under 4.0 cm deep (70% under 2 cm; target 95%), the deepest 4.0 cm, the longest 0.03 s (target under 0.2 s). Each one is in events.log ("shield").

## Performance

2311 frames in 5.8 s (400 fps on average); 1986 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 6.1 ms, 95% 13.4, 99% 58.1, worst 118.4. 7 hitches over 40 ms (1 over 100 ms). 16 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 156.0
- ArmSolve: 109.7
- Compose: 108.0
- RecorderSample: 93.7
- RecorderFlush: 37.9
- ComposeDry: 28.9
- Sandbox: 22.8
- Clearance: 20.2
- Authority: 7.0
- Prepare: 5.4
- Sweep: 2.5
- TryOut: 2.1
- CaptureSave: 0.8
- CaptureRender: 0.2
- Plan: 0.0

The worst frames:

- 0.05 s: 118 ms frame, 0 physics steps; ours: Compose 8.5 ms (6); Unity: PlayerLoop 10.5 ms, Gfx_WaitForGfxCommandsFromMainThread 9.1 ms, Loading_ReadObject 5.3 ms, CPU_Total_Frame_Time 1.9 ms, CPU_Main_Thread_Frame_Time 1.4 ms, CPU_Render_Thread_Frame_Time 0.7 ms, GPU_Frame_Time 0.6 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 0.2 ms
- 0.10 s: 99 ms frame, 3 physics steps; ours: RecorderSample 42.5 ms (3), UnitStep 30.4 ms (18), Compose 13.2 ms (18), ArmSolve 3.6 ms (54), Sandbox 3.1 ms (3), CaptureRender 1.4 ms (1); Unity: Main_Thread 97.7 ms, Gfx_WaitForGfxCommandsFromMainThread 94.7 ms, PlayerLoop 89.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 78.2 ms, FixedBehaviourUpdate 78.2 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 3.1 ms, LateBehaviourUpdate 3.1 ms, GC_Collect 3.0 ms
- 2.20 s: 62 ms frame, 1 physics steps; ours: Sandbox 49.7 ms (1), ComposeDry 29.6 ms (54), ArmSolve 26.2 ms (707), Clearance 19.6 ms (1), Prepare 11.5 ms (1), UnitStep 6.4 ms (6); Unity: Main_Thread 61.6 ms, PlayerLoop 60.4 ms, Gfx_WaitForGfxCommandsFromMainThread 59.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 57.7 ms, FixedBehaviourUpdate 57.7 ms, CPU_Total_Frame_Time 1.3 ms, CPU_Main_Thread_Frame_Time 0.8 ms, GPU_Frame_Time 0.6 ms
- 4.81 s: 62 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 54.4 ms (6), ComposeDry 41.6 ms (66), ArmSolve 40.2 ms (1058), Clearance 25.6 ms (1), Prepare 10.7 ms (5), TryOut 4.8 ms (10); Unity: Main_Thread 66.3 ms, Gfx_WaitForGfxCommandsFromMainThread 64.8 ms, PlayerLoop 64.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 56.8 ms, FixedBehaviourUpdate 56.8 ms, CPU_Total_Frame_Time 2.2 ms, GC_Collect 2.1 ms, CPU_Main_Thread_Frame_Time 1.1 ms
- 4.09 s: 58 ms frame, 1 physics steps; ours: UnitStep 52.4 ms (6), ComposeDry 35.6 ms (54), ArmSolve 33.8 ms (872), Clearance 25.3 ms (1), Prepare 2.2 ms (1), Compose 1.6 ms (4); Unity: Main_Thread 57.8 ms, PlayerLoop 56.6 ms, Gfx_WaitForGfxCommandsFromMainThread 56.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 54.0 ms, FixedBehaviourUpdate 54.0 ms, CPU_Total_Frame_Time 1.4 ms, CPU_Main_Thread_Frame_Time 0.9 ms, FixedUpdate_PhysicsFixedUpdate 0.6 ms
- 2.73 s: 45 ms frame, 1 physics steps; ours: Sandbox 37.2 ms (1), ComposeDry 31.0 ms (54), ArmSolve 29.9 ms (774), Clearance 24.5 ms (1), UnitStep 2.9 ms (6), Compose 2.4 ms (6); Unity: Main_Thread 45.2 ms, PlayerLoop 44.1 ms, Gfx_WaitForGfxCommandsFromMainThread 43.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 41.8 ms, FixedBehaviourUpdate 41.8 ms, CPU_Total_Frame_Time 1.3 ms, CPU_Main_Thread_Frame_Time 0.8 ms, GPU_Frame_Time 0.5 ms
- 4.46 s: 43 ms frame, 1 physics steps; ours: Sandbox 32.1 ms (1), ArmSolve 29.2 ms (750), ComposeDry 28.6 ms (54), Clearance 21.4 ms (1), UnitStep 3.9 ms (6), Compose 3.3 ms (4); Unity: Main_Thread 42.0 ms, PlayerLoop 40.8 ms, Gfx_WaitForGfxCommandsFromMainThread 40.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 38.3 ms, FixedBehaviourUpdate 38.3 ms, CPU_Total_Frame_Time 1.3 ms, CPU_Main_Thread_Frame_Time 0.8 ms, FixedUpdate_PhysicsFixedUpdate 0.6 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

