# Playtest 2026-11-03_13-59-57

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 7.3 s of play (439 physics steps), 537 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-03_13-59-57`.

## A Small

Present 8.4–15.7 s. Steps 21, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 6 cm from the support point; hips dropped for reach by up to 0 cm.

Feet: 17 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 56°, 95% under 74°.
Walking: 5.0 s (80% forward, 0% sideways, 20% backward), the body went 2.2 m and the feet 2.4 m each on average (feet/body 1.12; well under 1 is gliding); the feet point off the line it walks along (either way) median 8°, 95% under 33°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 5 / 25 |
| Chest | Pinned | 0.9 / 4.9 | 11 / 81 |
| Head | Pinned | 2.2 / 19.8 | 14 / 68 |
| UpperArmL | Pinned | 1.7 / 9.8 | 9 / 154 |
| ForearmL | Pinned | 2.0 / 14.6 | 11 / 97 |
| HandL | Pinned | 3.2 / 36.4 | 28 / 109 |
| UpperArmR | Muscled | 2.3 / 20.3 | 12 / 86 |
| ForearmR | Muscled | 3.8 / 23.0 | 19 / 94 |
| HandR | Muscled | 7.5 / 32.3 | 25 / 166 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 2, 34 damage. Health at the end 26.

Defence: 3 blows came at it (1 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, set its weapon across 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 57% of the time; the worst 47° (FootL).

Look: within 25° of what it attended to 78% of the time standing (80% of the time it was within the head's reach, 97% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.81.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 8.4–15.7 s. Steps 21, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 27 cm from the support point; hips dropped for reach by up to 3 cm.

Feet: 15 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 21°, 95% under 75°.
Walking: 3.7 s (100% forward, 0% sideways, 0% backward), the body went 2.4 m and the feet 2.5 m each on average (feet/body 1.04; well under 1 is gliding); the feet point off the line it walks along (either way) median 9°, 95% under 30°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 3 / 12 |
| Chest | Pinned | 0.8 / 3.5 | 5 / 20 |
| Head | Pinned | 1.2 / 5.5 | 11 / 39 |
| UpperArmL | Pinned | 1.4 / 5.5 | 4 / 53 |
| ForearmL | Pinned | 1.6 / 22.3 | 4 / 56 |
| HandL | Pinned | 1.4 / 28.5 | 2 / 55 |
| UpperArmR | Muscled | 1.3 / 7.2 | 6 / 45 |
| ForearmR | Muscled | 2.9 / 14.7 | 8 / 62 |
| HandR | Muscled | 4.4 / 28.1 | 10 / 81 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 3 (1 on the part aimed at, 0 blocked, 1 parried, 0 on someone in the way, 0 on allies), 26 damage. Hits taken: 0, 0 damage. Health at the end 100.

Pose against the joint limits (standing): a joint more than 10° past its limit 28% of the time; the worst 32° (FootL).

Look: within 25° of what it attended to 83% of the time standing (83% of the time it was within the head's reach, 91% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 100% of the time standing; its rim came within 1.3 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 2 postures (GuardChange 1, Peek 1), 0 of them lead-ins to blows; energy at the end 0.82.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 1.8 cm, max 2.5 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 12.60 s: SwordBackhand at B Large's Chest: passed 2.5 cm from the aim point; nearest part B Large's UpperArmL (12.3 cm); the Chest had moved 28.0 cm since the aim was locked
- 13.43 s: SwordCleave at B Large's Chest: passed 1.0 cm from the aim point; nearest part B Large's Chest (9.9 cm); the Chest had moved 20.1 cm since the aim was locked


## A Large

Present 8.4–15.7 s. Steps 17, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 40 cm from the support point; hips dropped for reach by up to 3 cm.

Feet: 13 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 20°, 95% under 59°.
Walking: 2.5 s (100% forward, 0% sideways, 0% backward), the body went 1.8 m and the feet 2.0 m each on average (feet/body 1.13; well under 1 is gliding); the feet point off the line it walks along (either way) median 8°, 95% under 29°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 2 / 19 |
| Chest | Pinned | 0.9 / 7.4 | 2 / 9 |
| Head | Pinned | 1.2 / 7.3 | 14 / 49 |
| UpperArmL | Pinned | 1.6 / 11.0 | 6 / 41 |
| ForearmL | Pinned | 4.4 / 24.1 | 11 / 63 |
| HandL | Pinned | 7.0 / 45.0 | 29 / 101 |
| UpperArmR | Kinematic | 1.0 / 8.5 | 2 / 55 |
| ForearmR | Kinematic | 1.6 / 14.9 | 4 / 64 |
| HandR | Kinematic | 3.1 / 52.0 | 6 / 79 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 3 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 43 damage. Hits taken: 0, 0 damage. Health at the end 220.

Pose against the joint limits (standing): a joint more than 10° past its limit 55% of the time; the worst 42° (FootR).

Look: within 25° of what it attended to 87% of the time standing (83% of the time it was within the head's reach, 73% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 1 postures (BoxerSettle 1), 0 of them lead-ins to blows; energy at the end 0.40.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 1.3 cm, max 3.4 cm (posed); physical weapon's closest pass mean 4.2 cm, max 8.6 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 12.03 s: ClubRising at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's Chest (25.0 cm); the Belly had moved 33.6 cm since the aim was locked
- 12.88 s: ClubSwing at B Small's Head: passed 0.5 cm from the aim point; nearest part B Small's Head (36.5 cm); the Head had moved 49.0 cm since the aim was locked
- 13.62 s: ClubBackhand at B Small's Chest: passed 3.4 cm from the aim point; nearest part B Small's Pelvis (22.9 cm); the Chest had moved 58.2 cm since the aim was locked


## B Small

Present 8.4–15.7 s. Steps 12, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 11 cm from the support point; hips dropped for reach by up to 12 cm.

Feet: 9 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 54°, 95% under 69°.
Walking: 3.4 s (100% forward, 0% sideways, 0% backward), the body went 2.1 m and the feet 1.8 m each on average (feet/body 0.89; well under 1 is gliding); the feet point off the line it walks along (either way) median 8°, 95% under 17°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.3 | 4 / 39 |
| Chest | Pinned | 0.8 / 9.9 | 8 / 70 |
| Head | Pinned | 1.5 / 9.3 | 10 / 94 |
| UpperArmL | Pinned | 1.4 / 16.2 | 9 / 159 |
| ForearmL | Pinned | 2.2 / 23.4 | 10 / 96 |
| HandL | Pinned | 3.2 / 36.3 | 24 / 99 |
| UpperArmR | Pinned | 1.2 / 5.1 | 7 / 43 |
| ForearmR | Pinned | 2.5 / 17.7 | 12 / 69 |
| HandR | Pinned | 5.7 / 27.5 | 15 / 91 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 2, 36 damage. Health at the end 24.

Defence: 3 blows came at it (0 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 0, set its weapon across 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 71% of the time; the worst 39° (FootL).
Ragdoll (on the ground 2.6 s): a joint more than 10° past its limit 97% of the time; the worst 173° (ShinR). Fastest part with a standing leg within 0.5 m: 8.0 m/s median, 22.8 m/s at the 95th percentile (30 steps); with none: 3.1 m/s median (123 steps).

Look: within 25° of what it attended to 86% of the time standing (86% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 2 postures (StickSettle 1, AbanicoFlurry 1), 0 of them lead-ins to blows; energy at the end 0.78.

Balance: stumbled 0 times, knocked down 1, launched 1, got up 0; thrown into others 0 times. Still down at the end.


## B Medium

Present 8.4–15.7 s. Steps 16, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 29 cm from the support point; hips dropped for reach by up to 3 cm.

Feet: 13 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 19°, 95% under 65°.
Walking: 3.8 s (77% forward, 0% sideways, 23% backward), the body went 2.2 m and the feet 2.5 m each on average (feet/body 1.10; well under 1 is gliding); the feet point off the line it walks along (either way) median 8°, 95% under 44°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 2 / 9 |
| Chest | Pinned | 0.6 / 2.7 | 4 / 15 |
| Head | Pinned | 0.9 / 4.2 | 11 / 37 |
| UpperArmL | Pinned | 1.0 / 4.5 | 4 / 55 |
| ForearmL | Pinned | 1.5 / 23.6 | 5 / 64 |
| HandL | Pinned | 2.0 / 32.8 | 3 / 61 |
| UpperArmR | Muscled | 0.8 / 6.0 | 4 / 24 |
| ForearmR | Muscled | 2.0 / 12.6 | 6 / 63 |
| HandR | Muscled | 3.2 / 28.2 | 8 / 82 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (0 on the part aimed at, 0 blocked, 1 parried, 0 on someone in the way, 0 on allies), 34 damage. Hits taken: 0, 0 damage. Health at the end 100.

Pose against the joint limits (standing): a joint more than 10° past its limit 38% of the time; the worst 29° (FootL).

Look: within 25° of what it attended to 94% of the time standing (94% of the time it was within the head's reach, 85% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 99% of the time standing; its rim came within 0.7 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 2 postures (GuardChange 1, Peek 1), 0 of them lead-ins to blows; energy at the end 0.71.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.3 cm, max 0.4 cm (posed); physical weapon's closest pass mean 0.5 cm, max 0.6 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 12.37 s: SwordCut at A Small's Head: passed 0.1 cm from the aim point; nearest part A Small's UpperArmR (22.4 cm); the Head had moved 40.3 cm since the aim was locked
- 13.80 s: SwordCleave at A Small's Chest: passed 0.4 cm from the aim point; nearest part A Small's Chest (3.4 cm); the Chest had moved 20.2 cm since the aim was locked


## B Large

Present 8.4–15.7 s. Steps 11, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 19 cm from the support point; hips dropped for reach by up to 0 cm.

Feet: 9 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 24°, 95% under 46°.
Walking: 5.3 s (41% forward, 0% sideways, 59% backward), the body went 2.8 m and the feet 2.3 m each on average (feet/body 0.85; well under 1 is gliding); the feet point off the line it walks along (either way) median 10°, 95% under 36°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 2 / 19 |
| Chest | Pinned | 1.1 / 7.4 | 3 / 20 |
| Head | Pinned | 1.7 / 7.3 | 12 / 49 |
| UpperArmL | Pinned | 1.8 / 9.9 | 6 / 41 |
| ForearmL | Pinned | 3.7 / 24.1 | 10 / 58 |
| HandL | Pinned | 4.7 / 45.0 | 22 / 72 |
| UpperArmR | Kinematic | 2.2 / 16.8 | 4 / 25 |
| ForearmR | Kinematic | 3.8 / 21.5 | 8 / 64 |
| HandR | Kinematic | 5.9 / 52.0 | 12 / 95 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 4, 33 damage. Health at the end 187.

Defence: 4 blows came at it (3 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, set its weapon across 4.

Pose against the joint limits (standing): a joint more than 10° past its limit 37% of the time; the worst 51° (FootL).

Look: within 25° of what it attended to 85% of the time standing (85% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 3 postures (BoxerSettle 1, BoxerStalk 1, BobAndWeave 1), 1 of them lead-ins to blows; energy at the end 0.96.

## Defence (milestone 11)

- Small: pressed 5 s, 29% of it in the shell (gate 70%); 1 of 6 combo links defended, 17% (gate 30–60%); 1 of 4 blows that met it on its guard (0 blocked, 1 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 1 bursts at it answered with a counter, 0% (gate 30–50%), 0 of them ripostes between links.
- Medium: pressed 4 s, 0% of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 2 bursts at it answered with a counter, 0% (gate 30–50%), 0 of them ripostes between links.
- Large: pressed 11 s, 34% of it in the shell (gate 70%); 2 of 3 combo links defended, 67% (gate 30–60%); 3 of 4 blows that met it on its guard (0 blocked, 1 parried, 2 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 2 bursts at it answered with a counter, 0% (gate 30–50%), 0 of them ripostes between links.

## Shield contacts

16 times a shield overlapped another unit's body: 95% under 2.6 cm deep (88% under 2 cm; target 95%), the deepest 2.6 cm, the longest 0.07 s (target under 0.2 s). Each one is in events.log ("shield").

## Performance

2585 frames in 7.5 s (346 fps on average); 2165 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 6.6 ms, 95% 15.2, 99% 58.4, worst 107.0. 11 hitches over 40 ms (1 over 100 ms). 21 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 193.8
- ArmSolve: 147.5
- Compose: 131.8
- RecorderSample: 88.2
- ComposeDry: 52.1
- Clearance: 35.2
- RecorderFlush: 35.1
- Sandbox: 29.4
- Plan: 10.3
- Authority: 7.2
- Prepare: 7.0
- CaptureSave: 4.2
- Sweep: 2.8
- TryOut: 2.3
- CaptureRender: 0.1

The worst frames:

- 0.05 s: 107 ms frame, 0 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Compose 8.5 ms (6), CaptureRender 0.5 ms (1); Unity: Gfx_WaitForGfxCommandsFromMainThread 7.0 ms, PlayerLoop 5.8 ms, CPU_Total_Frame_Time 4.4 ms, Update_ScriptRunBehaviourUpdate 1.3 ms, BehaviourUpdate 1.3 ms, CPU_Main_Thread_Frame_Time 0.8 ms, CPU_Render_Thread_Frame_Time 0.6 ms, GPU_Frame_Time 0.5 ms
- 0.10 s: 93 ms frame, 3 physics steps; ours: RecorderSample 38.6 ms (3), UnitStep 35.8 ms (18), Compose 19.2 ms (18), ArmSolve 8.7 ms (54), Sandbox 3.0 ms (3), Authority 1.2 ms (18); Unity: Main_Thread 92.8 ms, Gfx_WaitForGfxCommandsFromMainThread 90.0 ms, PlayerLoop 86.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 79.6 ms, FixedBehaviourUpdate 79.6 ms, CPU_Total_Frame_Time 5.4 ms, GC_Collect 5.0 ms, CPU_Main_Thread_Frame_Time 4.5 ms
- 2.65 s: 71 ms frame, 1 physics steps; ours: Sandbox 59.4 ms (1), ComposeDry 31.0 ms (54), ArmSolve 27.2 ms (729), Clearance 20.4 ms (1), Prepare 11.7 ms (1), UnitStep 5.7 ms (6); Unity: Main_Thread 70.2 ms, Gfx_WaitForGfxCommandsFromMainThread 68.7 ms, PlayerLoop 68.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 66.1 ms, FixedBehaviourUpdate 66.1 ms, CPU_Total_Frame_Time 1.3 ms, CPU_Main_Thread_Frame_Time 0.8 ms, CPU_Render_Thread_Frame_Time 0.6 ms
- 3.88 s: 61 ms frame, 1 physics steps; ours: UnitStep 55.3 ms (6), ComposeDry 47.7 ms (54), ArmSolve 41.9 ms (1064), Clearance 23.6 ms (1), Compose 3.0 ms (5), Prepare 2.1 ms (1); Unity: Main_Thread 60.7 ms, PlayerLoop 59.6 ms, Gfx_WaitForGfxCommandsFromMainThread 59.1 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 57.0 ms, FixedBehaviourUpdate 57.0 ms, CPU_Total_Frame_Time 1.4 ms, CPU_Main_Thread_Frame_Time 0.9 ms, FixedUpdate_PhysicsFixedUpdate 0.7 ms
- 4.62 s: 59 ms frame, 1 physics steps; ours: UnitStep 53.0 ms (6), ArmSolve 40.5 ms (1033), ComposeDry 39.9 ms (63), Clearance 25.5 ms (1), Prepare 8.5 ms (4), TryOut 4.1 ms (8); Unity: Main_Thread 58.3 ms, PlayerLoop 57.3 ms, Gfx_WaitForGfxCommandsFromMainThread 56.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 54.7 ms, FixedBehaviourUpdate 54.7 ms, CPU_Total_Frame_Time 1.7 ms, CPU_Main_Thread_Frame_Time 1.0 ms, FixedUpdate_PhysicsFixedUpdate 0.6 ms
- 4.53 s: 58 ms frame, 1 physics steps; ours: UnitStep 52.9 ms (6), ComposeDry 45.5 ms (54), ArmSolve 40.9 ms (1078), Clearance 22.5 ms (1), Compose 3.0 ms (5), Prepare 2.5 ms (1); Unity: Main_Thread 58.4 ms, PlayerLoop 57.4 ms, Gfx_WaitForGfxCommandsFromMainThread 57.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 54.7 ms, FixedBehaviourUpdate 54.7 ms, CPU_Total_Frame_Time 1.2 ms, CPU_Main_Thread_Frame_Time 0.8 ms, FixedUpdate_PhysicsFixedUpdate 0.6 ms
- 4.80 s: 58 ms frame, 1 physics steps; ours: UnitStep 50.2 ms (6), ComposeDry 42.3 ms (54), ArmSolve 39.2 ms (1005), Clearance 32.0 ms (1), Compose 4.0 ms (5), RecorderSample 2.5 ms (1); Unity: Main_Thread 61.3 ms, PlayerLoop 59.8 ms, Gfx_WaitForGfxCommandsFromMainThread 59.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 52.7 ms, FixedBehaviourUpdate 52.7 ms, GC_Collect 2.2 ms, CPU_Total_Frame_Time 1.3 ms, CPU_Main_Thread_Frame_Time 0.8 ms
- 3.76 s: 49 ms frame, 1 physics steps; ours: UnitStep 43.5 ms (6), ComposeDry 33.5 ms (54), ArmSolve 33.4 ms (909), Clearance 24.5 ms (1), Compose 3.4 ms (5), Prepare 2.8 ms (1); Unity: Main_Thread 48.8 ms, PlayerLoop 47.7 ms, Gfx_WaitForGfxCommandsFromMainThread 47.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 45.1 ms, FixedBehaviourUpdate 45.1 ms, CPU_Total_Frame_Time 1.4 ms, CPU_Main_Thread_Frame_Time 0.9 ms, FixedUpdate_PhysicsFixedUpdate 0.7 ms
- 4.08 s: 48 ms frame, 1 physics steps; ours: UnitStep 38.8 ms (6), ComposeDry 36.5 ms (57), ArmSolve 34.7 ms (930), Clearance 26.1 ms (4), Sandbox 4.3 ms (1), Plan 4.2 ms (1); Unity: Main_Thread 48.0 ms, PlayerLoop 47.0 ms, Gfx_WaitForGfxCommandsFromMainThread 46.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 44.5 ms, FixedBehaviourUpdate 44.5 ms, CPU_Total_Frame_Time 1.4 ms, CPU_Main_Thread_Frame_Time 0.9 ms, FixedUpdate_PhysicsFixedUpdate 0.6 ms
- 3.16 s: 43 ms frame, 1 physics steps; ours: Sandbox 35.3 ms (1), ArmSolve 29.3 ms (789), ComposeDry 28.4 ms (60), Clearance 20.3 ms (1), Prepare 6.7 ms (3), UnitStep 3.1 ms (6); Unity: Main_Thread 43.3 ms, PlayerLoop 42.2 ms, Gfx_WaitForGfxCommandsFromMainThread 42.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 40.0 ms, FixedBehaviourUpdate 40.0 ms, CPU_Total_Frame_Time 1.7 ms, CPU_Main_Thread_Frame_Time 0.9 ms, CPU_Render_Thread_Frame_Time 0.7 ms
- 2.74 s: 41 ms frame, 1 physics steps; ours: Sandbox 31.7 ms (1), ArmSolve 27.7 ms (773), ComposeDry 27.5 ms (54), Clearance 19.3 ms (1), UnitStep 2.8 ms (6), Compose 2.4 ms (6); Unity: Main_Thread 40.0 ms, Gfx_WaitForGfxCommandsFromMainThread 38.7 ms, PlayerLoop 38.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 36.3 ms, FixedBehaviourUpdate 36.3 ms, CPU_Total_Frame_Time 1.3 ms, CPU_Main_Thread_Frame_Time 0.8 ms, GPU_Frame_Time 0.6 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

