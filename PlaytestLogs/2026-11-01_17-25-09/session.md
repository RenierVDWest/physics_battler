# Playtest 2026-11-01_17-25-09

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 23.3 s of play (1396 physics steps), 2132 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-01_17-25-09`.

## A Small

Present 0.0–23.3 s. Steps 29, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 7 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.0 | 2 / 13 |
| Chest | Ragdoll | 0.4 / 2.6 | 2 / 10 |
| Head | Ragdoll | 0.5 / 3.7 | 9 / 100 |
| UpperArmL | Ragdoll | 0.5 / 3.5 | 7 / 167 |
| ForearmL | Ragdoll | 1.2 / 15.4 | 9 / 99 |
| HandL | Ragdoll | 3.4 / 37.9 | 21 / 178 |
| UpperArmR | Ragdoll | 0.5 / 3.8 | 3 / 90 |
| ForearmR | Ragdoll | 1.1 / 15.0 | 7 / 116 |
| HandR | Ragdoll | 2.1 / 31.6 | 20 / 99 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 4, 74 damage. Killed.

Defence: 4 blows came at it (4 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 4.

Pose against the joint limits (standing): a joint more than 10° past its limit 58% of the time; the worst 42° (FootL).
Ragdoll (on the ground 13.0 s): a joint more than 10° past its limit 83% of the time; the worst 171° (ForearmL). Fastest part with a standing leg within 0.5 m: 2.7 m/s median, 14.1 m/s at the 95th percentile (586 steps); with none: 0.1 m/s median (196 steps).

Look: within 25° of what it attended to 80% of the time standing (81% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.66.

Balance: stumbled 3 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–23.3 s. Steps 40, idle shuffles 0. Feet slid at most 25.7 cm while planted; the weight was at most 74 cm from the support point; hips dropped for reach by up to 21 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.6 | 2 / 50 |
| Chest | Pinned | 0.5 / 13.2 | 2 / 42 |
| Head | Pinned | 0.7 / 11.2 | 15 / 147 |
| UpperArmL | Pinned | 0.8 / 20.7 | 5 / 113 |
| ForearmL | Pinned | 2.0 / 33.1 | 5 / 87 |
| HandL | Pinned | 2.4 / 52.1 | 4 / 95 |
| UpperArmR | Kinematic | 0.5 / 5.3 | 3 / 155 |
| ForearmR | Kinematic | 1.1 / 38.1 | 6 / 172 |
| HandR | Kinematic | 1.8 / 27.2 | 16 / 90 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 3 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 33 damage. Hits taken: 3, 8 damage. Health at the end 92.

Defence: 1 blows came at it (1 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 0.

Pose against the joint limits (standing): a joint more than 10° past its limit 37% of the time; the worst 125° (UpperArmR).
Ragdoll (on the ground 4.0 s): a joint more than 10° past its limit 86% of the time; the worst 174° (ForearmL). Fastest part with a standing leg within 0.5 m: 1.7 m/s median, 8.9 m/s at the 95th percentile (72 steps); with none: 9.2 m/s median (169 steps).

Look: within 25° of what it attended to 84% of the time standing (86% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 94% of the time standing; its rim came within -2.4 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 5 postures (GuardChange 1, HighFeint 2, Peek 2), 0 of them lead-ins to blows; energy at the end 0.55.

Balance: stumbled 1 time, knocked down 1, launched 0, got up 1; thrown into others 1 time.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.7 cm, max 1.3 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 20.42 s: SwordCut at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's Pelvis (9.4 cm); the Belly had moved 39.5 cm since the aim was locked
- 22.03 s: SwordThrust at B Small's Belly: passed 0.1 cm from the aim point; nearest part B Small's Chest (12.9 cm); the Belly had moved 20.8 cm since the aim was locked


## A Large

Present 0.0–23.3 s. Steps 48, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 43 cm from the support point; hips dropped for reach by up to 11 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.4 | 3 / 75 |
| Chest | Pinned | 1.0 / 30.1 | 2 / 47 |
| Head | Pinned | 1.3 / 25.9 | 17 / 105 |
| UpperArmL | Pinned | 1.8 / 48.9 | 8 / 145 |
| ForearmL | Pinned | 4.5 / 42.9 | 12 / 103 |
| HandL | Pinned | 5.4 / 45.4 | 23 / 174 |
| UpperArmR | Pinned | 0.9 / 21.0 | 3 / 55 |
| ForearmR | Pinned | 1.8 / 21.1 | 6 / 69 |
| HandR | Pinned | 3.1 / 54.0 | 16 / 137 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (0 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 10 damage. Hits taken: 11, 117 damage. Health at the end 103.

Defence: 13 blows came at it (2 seen clearly, 11 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 12.

Pose against the joint limits (standing): a joint more than 10° past its limit 47% of the time; the worst 130° (UpperArmR).

Look: within 25° of what it attended to 84% of the time standing (83% of the time it was within the head's reach, 79% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 4 postures (BoxerSettle 1, BobAndWeave 2, ClubFeint 1), 1 of them lead-ins to blows; energy at the end 0.67.

Balance: stumbled 3 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.1 cm, max 0.1 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.85 s: ClubSmash at B Small's Head: passed 0.0 cm from the aim point; nearest part B Small's UpperArmR (27.5 cm); the Head had moved 47.3 cm since the aim was locked


## B Small

Present 0.0–23.3 s. Steps 65, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 32 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.5 | 2 / 65 |
| Chest | Pinned | 0.4 / 9.5 | 4 / 115 |
| Head | Pinned | 0.8 / 21.6 | 13 / 116 |
| UpperArmL | Pinned | 1.0 / 30.8 | 8 / 149 |
| ForearmL | Pinned | 2.6 / 49.5 | 11 / 177 |
| HandL | Pinned | 3.3 / 38.8 | 25 / 169 |
| UpperArmR | Pinned | 0.3 / 6.5 | 2 / 77 |
| ForearmR | Pinned | 0.8 / 15.9 | 5 / 99 |
| HandR | Pinned | 1.3 / 25.0 | 15 / 99 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 3 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 24 damage. Hits taken: 3, 33 damage. Health at the end 27.

Defence: 5 blows came at it (2 seen clearly, 2 at the edge of its view, 1 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 68% of the time; the worst 126° (UpperArmR).
Ragdoll (on the ground 1.0 s): a joint more than 10° past its limit 97% of the time; the worst 128° (HandL). Fastest part with a standing leg within 0.5 m: 2.8 m/s median, 6.6 m/s at the 95th percentile (60 steps); with none: - median (0 steps).

Look: within 25° of what it attended to 67% of the time standing (76% of the time it was within the head's reach, 87% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 7 postures (StickSettle 1, AbanicoFlurry 3, BaitRetreat 1, RedondaTwirl 2), 0 of them lead-ins to blows; energy at the end 0.71.

Balance: stumbled 3 times, knocked down 1, launched 0, got up 1; thrown into others 0 times.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 1.0 cm, max 1.2 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 14.67 s: ClubSwing at A Large's KneeR: passed 0.0 cm from the aim point; nearest part A Medium's ThighR (22.5 cm); the KneeR had moved 42.5 cm since the aim was locked
- 18.02 s: ClubSwing at A Large's Belly: passed 0.0 cm from the aim point; nearest part A Large's ForearmR (33.2 cm); the Belly had moved 33.0 cm since the aim was locked


## B Medium

Present 0.0–23.3 s. Steps 53, idle shuffles 0. Feet slid at most 2.8 cm while planted; the weight was at most 34 cm from the support point; hips dropped for reach by up to 21 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 2 / 23 |
| Chest | Pinned | 0.5 / 6.6 | 2 / 31 |
| Head | Pinned | 0.7 / 10.1 | 17 / 106 |
| UpperArmL | Pinned | 0.8 / 11.4 | 5 / 71 |
| ForearmL | Pinned | 2.1 / 23.5 | 5 / 93 |
| HandL | Pinned | 2.6 / 50.6 | 5 / 129 |
| UpperArmR | Pinned | 0.4 / 3.7 | 2 / 69 |
| ForearmR | Pinned | 0.8 / 17.0 | 4 / 79 |
| HandR | Pinned | 1.4 / 27.2 | 13 / 79 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 5 (3 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 92 damage. Hits taken: 2, 26 damage. Health at the end 74.

Defence: 1 blows came at it (0 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0.

Pose against the joint limits (standing): a joint more than 10° past its limit 43% of the time; the worst 124° (UpperArmR).
Ragdoll (on the ground 1.5 s): a joint more than 10° past its limit 93% of the time; the worst 152° (ForearmR). Fastest part with a standing leg within 0.5 m: 6.7 m/s median, 9.0 m/s at the 95th percentile (34 steps); with none: 3.3 m/s median (55 steps).

Look: within 25° of what it attended to 87% of the time standing (88% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 95% of the time standing; its rim came within -3.0 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 5 postures (GuardChange 1, ShieldSettle 1, HighFeint 2, ShieldAdvance 1), 1 of them lead-ins to blows; energy at the end 0.27.

Balance: stumbled 1 time, knocked down 1, launched 0, got up 1; thrown into others 1 time.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.0 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.5 cm, max 1.4 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.12 s: SwordThrust at A Small's Head: passed 0.1 cm from the aim point; nearest part A Small's Chest (11.6 cm); the Head had moved 13.7 cm since the aim was locked
- 5.72 s: SwordCut at A Small's ElbowL: passed 0.0 cm from the aim point; nearest part A Small's Pelvis (7.8 cm); the ElbowL had moved 28.8 cm since the aim was locked
- 7.52 s: SwordBackhand at A Small's Belly: passed 0.1 cm from the aim point; nearest part A Small's Pelvis (0.0 cm); the Belly had moved 2.9 cm since the aim was locked
- 10.35 s: SwordCut at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's ThighR (24.1 cm); the Chest had moved 65.2 cm since the aim was locked


## B Large

Present 0.0–23.3 s. Steps 45, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 31 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 1.7 | 2 / 59 |
| Chest | ? | 0.7 / 24.3 | 2 / 52 |
| Head | ? | 1.0 / 27.8 | 14 / 166 |
| UpperArmL | ? | 1.2 / 44.0 | 7 / 133 |
| ForearmL | ? | 4.7 / 58.4 | 12 / 167 |
| HandL | ? | 5.6 / 55.8 | 24 / 169 |
| UpperArmR | ? | 0.5 / 6.2 | 2 / 55 |
| ForearmR | ? | 1.2 / 20.0 | 4 / 62 |
| HandR | ? | 2.3 / 54.0 | 12 / 84 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 7 (0 on the part aimed at, 0 blocked, 1 on someone in the way, 1 on allies), 68 damage. Hits taken: 1, 10 damage. Health at the end 0.

Defence: 3 blows came at it (2 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 44% of the time; the worst 130° (UpperArmR).

Look: within 25° of what it attended to 75% of the time standing (77% of the time it was within the head's reach, 96% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 5 postures (BoxerSettle 1, BoxerStalk 1, BobAndWeave 2, ClubFeint 1), 2 of them lead-ins to blows; energy at the end 0.00.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 1.4 cm, max 1.4 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 5.03 s: ClubSwing at A Medium's Belly: passed 0.1 cm from the aim point; nearest part A Medium's HandL (18.7 cm); the Belly had moved 36.6 cm since the aim was locked
- 16.70 s: ClubSwing at A Large's Chest: passed 0.1 cm from the aim point; nearest part A Large's UpperArmL (34.9 cm); the Chest had moved 79.5 cm since the aim was locked

