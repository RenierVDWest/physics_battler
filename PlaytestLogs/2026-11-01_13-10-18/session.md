# Playtest 2026-11-01_13-10-18

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 7.5 s of play (449 physics steps), 305 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-01_13-10-18`.

## A Small

Present 0.0–7.5 s. Steps 22, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 7 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.0 | 2 / 8 |
| Chest | ? | 0.3 / 1.6 | 2 / 7 |
| Head | ? | 0.4 / 2.0 | 10 / 66 |
| UpperArmL | ? | 0.5 / 2.1 | 3 / 44 |
| ForearmL | ? | 1.0 / 15.4 | 7 / 99 |
| HandL | ? | 2.3 / 36.0 | 22 / 82 |
| UpperArmR | ? | 0.5 / 2.1 | 3 / 75 |
| ForearmR | ? | 1.3 / 13.4 | 9 / 117 |
| HandR | ? | 2.9 / 31.6 | 34 / 123 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 3, 69 damage. Health at the end 0.

Defence: 3 blows came at it (2 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 64% of the time; the worst 42° (FootL).
Ragdoll (on the ground 0.0 s): a joint more than 10° past its limit 100% of the time; the worst 12° (FootL). Fastest part with a standing leg within 0.5 m: 2.5 m/s median, 2.6 m/s at the 95th percentile (2 steps); with none: - median (0 steps).

Look: within 25° of what it attended to 79% of the time standing (79% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–7.5 s. Steps 12, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 19 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 1 / 22 |
| Chest | ? | 0.4 / 6.4 | 2 / 21 |
| Head | ? | 0.5 / 9.1 | 14 / 54 |
| UpperArmL | ? | 0.6 / 12.4 | 3 / 54 |
| ForearmL | ? | 1.4 / 23.1 | 5 / 46 |
| HandL | ? | 2.4 / 29.0 | 22 / 99 |
| UpperArmR | ? | 0.5 / 4.3 | 2 / 50 |
| ForearmR | ? | 1.2 / 8.7 | 6 / 70 |
| HandR | ? | 2.2 / 29.6 | 17 / 84 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 1 on allies), 16 damage. Hits taken: 1, 12 damage. Health at the end 0.

Defence: 1 blows came at it (1 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 0.

Pose against the joint limits (standing): a joint more than 10° past its limit 51% of the time; the worst 163° (HandR).
Ragdoll (on the ground 1.1 s): a joint more than 10° past its limit 82% of the time; the worst 140° (HandR). Fastest part with a standing leg within 0.5 m: 3.4 m/s median, 6.2 m/s at the 95th percentile (67 steps); with none: - median (0 steps).

Look: within 25° of what it attended to 64% of the time standing (68% of the time it was within the head's reach, 95% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 75% of the time standing; its rim came within 14.2 cm of its own body (negative: inside).

Fight Rhythm (?): 1 postures (GuardChange 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 0 times, knocked down 1, launched 0, got up 1; thrown into others 0 times.


## A Large

Present 0.0–7.5 s. Steps 12, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 23 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 2 / 17 |
| Chest | Pinned | 0.8 / 6.4 | 2 / 9 |
| Head | Pinned | 1.0 / 7.4 | 11 / 31 |
| UpperArmL | Pinned | 1.2 / 11.0 | 4 / 50 |
| ForearmL | Pinned | 3.1 / 23.7 | 7 / 63 |
| HandL | Pinned | 4.2 / 45.4 | 18 / 76 |
| UpperArmR | Pinned | 1.2 / 6.2 | 4 / 45 |
| ForearmR | Pinned | 3.4 / 17.2 | 12 / 75 |
| HandR | Pinned | 6.0 / 56.9 | 31 / 97 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 4, 38 damage. Health at the end 182.

Defence: 4 blows came at it (1 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 4.

Pose against the joint limits (standing): a joint more than 10° past its limit 68% of the time; the worst 130° (HandR).

Look: within 25° of what it attended to 83% of the time standing (83% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 2 postures (BoxerSettle 1, BobAndWeave 1), 0 of them lead-ins to blows; energy at the end 0.99.


## B Small

Present 0.0–7.5 s. Steps 24, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 20 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 3 / 101 |
| Chest | ? | 0.5 / 18.1 | 3 / 68 |
| Head | ? | 0.7 / 23.9 | 11 / 70 |
| UpperArmL | ? | 0.9 / 23.7 | 6 / 138 |
| ForearmL | ? | 1.8 / 21.9 | 10 / 123 |
| HandL | ? | 2.7 / 36.0 | 25 / 153 |
| UpperArmR | ? | 0.3 / 1.9 | 3 / 57 |
| ForearmR | ? | 0.9 / 14.0 | 6 / 94 |
| HandR | ? | 1.9 / 23.5 | 21 / 118 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 1 on allies), 15 damage. Hits taken: 0, 0 damage. Health at the end 0.

Defence: 1 blows came at it (0 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 79% of the time; the worst 165° (HandR).

Look: within 25° of what it attended to 82% of the time standing (84% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 4 postures (StickSettle 1, AbanicoFlurry 2, RedondaTwirl 1), 0 of them lead-ins to blows; energy at the end 0.00.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.67 s: ClubSwing at A Large's ElbowL: passed 0.0 cm from the aim point; nearest part A Large's ForearmL (6.3 cm); the ElbowL had moved 14.8 cm since the aim was locked


## B Medium

Present 0.0–7.5 s. Steps 15, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 32 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 5.5 | 4 / 72 |
| Chest | ? | 1.2 / 38.2 | 4 / 58 |
| Head | ? | 1.1 / 36.7 | 18 / 109 |
| UpperArmL | ? | 1.7 / 38.1 | 8 / 177 |
| ForearmL | ? | 2.8 / 35.6 | 8 / 174 |
| HandL | ? | 2.9 / 37.0 | 18 / 177 |
| UpperArmR | ? | 0.3 / 3.9 | 2 / 51 |
| ForearmR | ? | 0.8 / 9.3 | 4 / 69 |
| HandR | ? | 1.7 / 30.5 | 12 / 113 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 3 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 69 damage. Hits taken: 0, 0 damage. Health at the end 0.

Pose against the joint limits (standing): a joint more than 10° past its limit 61% of the time; the worst 164° (HandR).

Look: within 25° of what it attended to 100% of the time standing (100% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 99% of the time standing; its rim came within -0.4 cm of its own body (negative: inside).

Fight Rhythm (?): 1 postures (GuardChange 1), 0 of them lead-ins to blows; energy at the end 0.00.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.2 cm, max 0.6 cm (posed); physical weapon's closest pass mean 0.3 cm, max 0.6 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.95 s: SwordThrust at A Small's Head: passed 0.6 cm from the aim point; nearest part A Small's Head (7.6 cm); the Head had moved 16.0 cm since the aim was locked
- 5.67 s: SwordCut at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Pelvis (11.0 cm); the Chest had moved 32.8 cm since the aim was locked
- 7.43 s: SwordBackhand at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (0.0 cm); the Chest had moved 4.0 cm since the aim was locked


## B Large

Present 0.0–7.5 s. Steps 16, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 34 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 2 / 17 |
| Chest | ? | 0.8 / 6.5 | 2 / 12 |
| Head | ? | 1.1 / 7.4 | 11 / 42 |
| UpperArmL | ? | 1.4 / 13.5 | 6 / 50 |
| ForearmL | ? | 4.5 / 23.7 | 12 / 63 |
| HandL | ? | 6.6 / 45.4 | 28 / 87 |
| UpperArmR | ? | 0.8 / 6.7 | 3 / 45 |
| ForearmR | ? | 2.2 / 22.1 | 8 / 75 |
| HandR | ? | 3.8 / 56.9 | 22 / 172 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 3 (0 on the part aimed at, 0 blocked, 1 on someone in the way, 0 on allies), 27 damage. Hits taken: 1, 8 damage. Health at the end 0.

Defence: 2 blows came at it (2 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 83% of the time; the worst 161° (HandR).

Look: within 25° of what it attended to 84% of the time standing (85% of the time it was within the head's reach, 93% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 2 postures (BoxerSettle 1, ClubFeint 1), 0 of them lead-ins to blows; energy at the end 0.00.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.07 s: ClubRising at A Medium's KneeR: passed 0.1 cm from the aim point; nearest part A Medium's ThighR (0.0 cm); the KneeR had moved 9.5 cm since the aim was locked

