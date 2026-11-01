# Playtest 2026-11-01_12-02-20

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 61.9 s of play (3715 physics steps), 2269 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-01_12-02-20`.

## A Small

Present 0.0–61.9 s. Steps 93, idle shuffles 2. Feet slid at most 13.2 cm while planted; the weight was at most 36 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 6.5 | 2 / 140 |
| Chest | Ragdoll | 0.3 / 20.0 | 2 / 76 |
| Head | Ragdoll | 0.4 / 19.5 | 6 / 139 |
| UpperArmL | Ragdoll | 0.5 / 27.1 | 3 / 164 |
| ForearmL | Ragdoll | 1.1 / 31.5 | 6 / 170 |
| HandL | Ragdoll | 1.7 / 45.6 | 15 / 178 |
| UpperArmR | Ragdoll | 0.3 / 15.6 | 2 / 150 |
| ForearmR | Ragdoll | 0.6 / 13.3 | 5 / 175 |
| HandR | Ragdoll | 1.5 / 32.7 | 18 / 172 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 4 (0 on the part aimed at, 2 blocked, 2 on someone in the way, 0 on allies), 17 damage. Hits taken: 5, 64 damage. Killed.

Defence: 7 blows came at it (6 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 6.

Pose against the joint limits (standing): a joint more than 10° past its limit 60% of the time; the worst 165° (HandR).
Ragdoll (on the ground 32.2 s): a joint more than 10° past its limit 23% of the time; the worst 155° (HandR). Fastest part with a standing leg within 0.5 m: 0.0 m/s median, 2.2 m/s at the 95th percentile (612 steps); with none: 0.0 m/s median (1321 steps).

Look: within 25° of what it attended to 79% of the time standing (85% of the time it was within the head's reach, 92% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 11 postures (StickSettle 1, AbanicoFlurry 4, RedondaTwirl 3, TriangleStep 2, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.53.

Balance: stumbled 1 time, knocked down 0, launched 1, got up 1; thrown into others 0 times.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.6 cm, max 1.2 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.72 s: ClubSwing at B Medium's ElbowL: passed 0.0 cm from the aim point; nearest part B Medium's Pelvis (20.4 cm); the ElbowL had moved 53.0 cm since the aim was locked
- 29.80 s: ClubSmash at B Medium's Head: passed 0.0 cm from the aim point; nearest part B Medium's Head (18.8 cm); the Head had moved 25.2 cm since the aim was locked


## A Medium

Present 0.0–61.9 s. Steps 129, idle shuffles 4. Feet slid at most 0.0 cm while planted; the weight was at most 63 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.6 | 1 / 56 |
| Chest | Pinned | 0.2 / 15.7 | 1 / 47 |
| Head | Pinned | 0.3 / 16.0 | 7 / 94 |
| UpperArmL | Pinned | 0.4 / 26.0 | 2 / 120 |
| ForearmL | Pinned | 1.2 / 34.5 | 5 / 177 |
| HandL | Pinned | 1.9 / 61.8 | 14 / 178 |
| UpperArmR | Pinned | 0.2 / 8.4 | 1 / 140 |
| ForearmR | Pinned | 0.5 / 45.2 | 2 / 142 |
| HandR | Pinned | 0.7 / 22.6 | 9 / 179 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 19 (3 on the part aimed at, 8 blocked, 0 on someone in the way, 4 on allies), 141 damage. Hits taken: 8, 36 damage. Health at the end 64.

Defence: 5 blows came at it (5 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 54% of the time; the worst 165° (HandR).
Ragdoll (on the ground 2.9 s): a joint more than 10° past its limit 92% of the time; the worst 55° (HandR). Fastest part with a standing leg within 0.5 m: 5.1 m/s median, 7.0 m/s at the 95th percentile (4 steps); with none: 2.6 m/s median (169 steps).

Look: within 25° of what it attended to 85% of the time standing (87% of the time it was within the head's reach, 97% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 93% of the time standing; its rim came within 10.8 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 16 postures (GuardChange 3, HighFeint 4, Peek 2, ShieldPress 3, ShieldAdvance 3, ShieldSettle 1), 4 of them lead-ins to blows; energy at the end 0.79.

Balance: stumbled 1 time, knocked down 1, launched 0, got up 1; thrown into others 3 times.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.8 cm, max 0.8 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 12.20 s: SwordBackhand at B Large's Belly: passed 0.0 cm from the aim point; nearest part B Large's Pelvis (20.9 cm); the Belly had moved 25.9 cm since the aim was locked
- 34.52 s: SwordCut at B Medium's KneeL: passed 0.0 cm from the aim point; nearest part B Medium's ShinL (9.2 cm); the KneeL had moved 9.7 cm since the aim was locked
- 53.13 s: SwordBackhand at B Medium's Neck: passed 0.0 cm from the aim point; nearest part B Medium's Chest (6.5 cm); the Neck had moved 9.1 cm since the aim was locked
- 54.92 s: SwordThrust at B Medium's Neck: passed 0.1 cm from the aim point; nearest part B Medium's Chest (10.2 cm); the Neck had moved 8.7 cm since the aim was locked


## A Large

Present 0.0–61.9 s. Steps 69, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 47 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.3 | 0 / 18 |
| Chest | ? | 0.2 / 7.6 | 1 / 16 |
| Head | ? | 0.3 / 5.1 | 5 / 38 |
| UpperArmL | ? | 0.4 / 7.9 | 2 / 61 |
| ForearmL | ? | 1.4 / 25.6 | 4 / 79 |
| HandL | ? | 2.4 / 33.3 | 12 / 101 |
| UpperArmR | ? | 0.2 / 4.7 | 1 / 27 |
| ForearmR | ? | 0.5 / 16.0 | 2 / 62 |
| HandR | ? | 0.9 / 43.9 | 6 / 125 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 10 (4 on the part aimed at, 0 blocked, 2 on someone in the way, 2 on allies), 167 damage. Hits taken: 5, 80 damage. Health at the end 0.

Defence: 1 blows came at it (1 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 0.

Pose against the joint limits (standing): a joint more than 10° past its limit 50% of the time; the worst 165° (HandR).
Ragdoll (on the ground 23.3 s): a joint more than 10° past its limit 95% of the time; the worst 58° (Head). Fastest part with a standing leg within 0.5 m: - median, - at the 95th percentile (0 steps); with none: 0.0 m/s median (1400 steps).

Look: within 25° of what it attended to 90% of the time standing (90% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 12 postures (BoxerSettle 2, ClubFeint 1, BobAndWeave 1, Shift 1, JabFeint 4, BoxerStalk 3), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.5 cm, max 0.9 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.93 s: ClubSwing at B Small's Chest: passed 0.1 cm from the aim point; nearest part B Small's Pelvis (9.0 cm); the Chest had moved 57.7 cm since the aim was locked
- 8.57 s: ClubSwing at B Small's Head: passed 0.1 cm from the aim point; nearest part B Small's UpperArmR (23.0 cm); the Head had moved 29.8 cm since the aim was locked
- 14.02 s: ClubSmash at B Small's Head: passed 0.0 cm from the aim point; nearest part B Small's Head (12.1 cm); the Head had moved 25.4 cm since the aim was locked
- 18.98 s: ClubSwing at B Medium's Chest: passed 0.1 cm from the aim point; nearest part B Medium's UpperArmL (5.5 cm); the Chest had moved 30.8 cm since the aim was locked


## B Small

Present 0.0–61.9 s. Steps 28, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 12 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 1 / 6 |
| Chest | ? | 0.1 / 1.1 | 1 / 15 |
| Head | ? | 0.2 / 3.0 | 4 / 15 |
| UpperArmL | ? | 0.2 / 3.6 | 1 / 37 |
| ForearmL | ? | 0.5 / 13.4 | 3 / 93 |
| HandL | ? | 0.8 / 25.0 | 8 / 85 |
| UpperArmR | ? | 0.2 / 2.3 | 1 / 41 |
| ForearmR | ? | 0.5 / 9.3 | 3 / 89 |
| HandR | ? | 1.1 / 21.0 | 17 / 110 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 6, 81 damage. Health at the end 0.

Defence: 3 blows came at it (1 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 64% of the time; the worst 164° (HandR).
Ragdoll (on the ground 50.8 s): a joint more than 10° past its limit 99% of the time; the worst 169° (ForearmR). Fastest part with a standing leg within 0.5 m: 1.2 m/s median, 8.1 m/s at the 95th percentile (380 steps); with none: 0.0 m/s median (2669 steps).

Look: within 25° of what it attended to 77% of the time standing (85% of the time it was within the head's reach, 86% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 0 times, knocked down 0, launched 2, got up 2; thrown into others 3 times.


## B Medium

Present 0.0–61.9 s. Steps 100, idle shuffles 4. Feet slid at most 0.0 cm while planted; the weight was at most 28 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 1.1 | 1 / 97 |
| Chest | Ragdoll | 0.2 / 26.2 | 1 / 46 |
| Head | Ragdoll | 0.3 / 27.6 | 8 / 165 |
| UpperArmL | Ragdoll | 0.4 / 21.8 | 3 / 179 |
| ForearmL | Ragdoll | 1.2 / 50.2 | 5 / 164 |
| HandL | Ragdoll | 1.4 / 62.3 | 13 / 169 |
| UpperArmR | Ragdoll | 0.2 / 7.9 | 1 / 42 |
| ForearmR | Ragdoll | 0.4 / 12.0 | 2 / 61 |
| HandR | Ragdoll | 0.6 / 22.4 | 8 / 148 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 11 (1 on the part aimed at, 3 blocked, 1 on someone in the way, 4 on allies), 110 damage. Hits taken: 19, 79 damage. Killed.

Defence: 19 blows came at it (15 seen clearly, 4 at the edge of its view, 0 never seen); it raised the shield against 10, and had nothing to defend with against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 47% of the time; the worst 162° (HandR).
Ragdoll (on the ground 8.8 s): a joint more than 10° past its limit 53% of the time; the worst 142° (ShinR). Fastest part with a standing leg within 0.5 m: 2.3 m/s median, 5.9 m/s at the 95th percentile (50 steps); with none: 0.6 m/s median (475 steps).

Look: within 25° of what it attended to 90% of the time standing (91% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 96% of the time standing; its rim came within 5.5 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 6 postures (GuardChange 1, HighFeint 2, Peek 2, ShieldPress 1), 2 of them lead-ins to blows; energy at the end 0.17.

Balance: stumbled 3 times, knocked down 3, launched 0, got up 1; thrown into others 1 time.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.0 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.4 cm, max 0.4 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 17.62 s: SwordCut at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's ForearmL (16.6 cm); the Chest had moved 28.0 cm since the aim was locked
- 31.65 s: SwordCleave at A Small's Belly: passed 0.1 cm from the aim point; nearest part A Small's Head (25.7 cm); the Belly had moved 30.0 cm since the aim was locked


## B Large

Present 0.0–61.9 s. Steps 34, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 17 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.2 | 1 / 11 |
| Chest | Ragdoll | 0.2 / 4.7 | 1 / 11 |
| Head | Ragdoll | 0.4 / 5.4 | 7 / 60 |
| UpperArmL | Ragdoll | 0.4 / 9.7 | 2 / 35 |
| ForearmL | Ragdoll | 1.1 / 18.5 | 3 / 47 |
| HandL | Ragdoll | 1.9 / 33.3 | 10 / 72 |
| UpperArmR | Ragdoll | 0.4 / 5.9 | 2 / 51 |
| ForearmR | Ragdoll | 0.9 / 13.0 | 3 / 62 |
| HandR | Ragdoll | 1.7 / 43.9 | 10 / 85 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 15, 164 damage. Killed.

Defence: 7 blows came at it (5 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 5.

Pose against the joint limits (standing): a joint more than 10° past its limit 39% of the time; the worst 129° (HandR).
Ragdoll (on the ground 44.1 s): a joint more than 10° past its limit 94% of the time; the worst 47° (Head). Fastest part with a standing leg within 0.5 m: 0.0 m/s median, 3.1 m/s at the 95th percentile (935 steps); with none: 0.0 m/s median (1711 steps).

Look: within 25° of what it attended to 88% of the time standing (87% of the time it was within the head's reach, 83% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 3 postures (BoxerSettle 1, BoxerStalk 1, BobAndWeave 1), 1 of them lead-ins to blows; energy at the end 0.66.

Balance: stumbled 3 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

