# Playtest 2026-11-01_06-45-08

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 209.8 s of play (12591 physics steps), 1954 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-01_06-45-08`.

## A Small

Present 0.0–209.8 s. Steps 60, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 12 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.4 | 1 / 81 |
| Chest | Ragdoll | 0.1 / 11.0 | 1 / 58 |
| Head | Ragdoll | 0.2 / 11.0 | 3 / 128 |
| UpperArmL | Ragdoll | 0.2 / 15.1 | 1 / 108 |
| ForearmL | Ragdoll | 0.5 / 32.2 | 3 / 131 |
| HandL | Ragdoll | 0.7 / 24.7 | 7 / 139 |
| UpperArmR | Ragdoll | 0.1 / 1.3 | 1 / 89 |
| ForearmR | Ragdoll | 0.4 / 9.5 | 2 / 121 |
| HandR | Ragdoll | 0.6 / 27.7 | 11 / 142 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 17 damage. Hits taken: 4, 67 damage. Killed.

Fight Rhythm (Eskrima): 7 postures (AbanicoFlurry 3, TriangleStep 2, RedondaTwirl 2), 1 of them lead-ins to blows; energy at the end 0.57.

Balance: stumbled 3 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 20.80 s: ClubSwing at B Medium's KneeL: passed 0.0 cm from the aim point; nearest part B Medium's ShinL (10.8 cm); the KneeL had moved 4.4 cm since the aim was locked


## A Medium

Present 0.0–209.8 s. Steps 155, idle shuffles 13. Feet slid at most 0.0 cm while planted; the weight was at most 47 cm from the support point; hips dropped for reach by up to 7 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 3.6 | 1 / 61 |
| Chest | Ragdoll | 0.2 / 23.8 | 1 / 82 |
| Head | Ragdoll | 0.2 / 27.6 | 6 / 157 |
| UpperArmL | Ragdoll | 0.3 / 39.3 | 2 / 168 |
| ForearmL | Ragdoll | 0.8 / 39.2 | 3 / 175 |
| HandL | Ragdoll | 1.0 / 27.0 | 12 / 180 |
| UpperArmR | Ragdoll | 0.1 / 6.0 | 1 / 50 |
| ForearmR | Ragdoll | 0.3 / 13.1 | 1 / 57 |
| HandR | Ragdoll | 0.5 / 23.5 | 6 / 177 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 13 (4 on the part aimed at, 2 blocked, 0 on someone in the way, 2 on allies), 163 damage. Hits taken: 8, 35 damage. Killed.

Fight Rhythm (ShieldAndSword): 27 postures (HighFeint 9, ShieldPress 10, Peek 4, GuardChange 4), 10 of them lead-ins to blows; energy at the end 0.64.

Balance: stumbled 5 times, knocked down 2, launched 0, got up 2; thrown into others 0 times.

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.1 cm, max 0.1 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 11.72 s: SwordThrust at B Large's Head: passed 0.1 cm from the aim point; nearest part B Large's Head (17.3 cm); the Head had moved 34.0 cm since the aim was locked
- 19.72 s: SwordThrust at B Large's Belly: passed 0.1 cm from the aim point; nearest part B Large's Spine (0.0 cm); the Belly had moved 1.4 cm since the aim was locked
- 50.62 s: SwordThrust at B Small's Head: passed 0.1 cm from the aim point; nearest part B Small's Head (12.3 cm); the Head had moved 14.2 cm since the aim was locked
- 53.50 s: SwordThrust at B Small's Belly: passed 0.1 cm from the aim point; nearest part A Large's ShinL (16.9 cm); the Belly had moved 190.8 cm since the aim was locked
- 69.58 s: SwordThrust at B Medium's Head: passed 0.1 cm from the aim point; nearest part B Medium's Head (18.0 cm); the Head had moved 21.2 cm since the aim was locked


## A Large

Present 0.0–209.8 s. Steps 114, idle shuffles 5. Feet slid at most 0.0 cm while planted; the weight was at most 65 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 3.7 | 1 / 68 |
| Chest | Ragdoll | 0.2 / 40.4 | 0 / 46 |
| Head | Ragdoll | 0.3 / 35.8 | 3 / 102 |
| UpperArmL | Ragdoll | 0.4 / 42.5 | 2 / 174 |
| ForearmL | Ragdoll | 1.4 / 32.4 | 4 / 142 |
| HandL | Ragdoll | 2.4 / 36.7 | 10 / 143 |
| UpperArmR | Ragdoll | 0.2 / 9.5 | 1 / 46 |
| ForearmR | Ragdoll | 0.6 / 14.4 | 2 / 65 |
| HandR | Ragdoll | 1.1 / 45.7 | 8 / 155 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 15 (5 on the part aimed at, 4 blocked, 0 on someone in the way, 1 on allies), 172 damage. Hits taken: 19, 229 damage. Killed.

Fight Rhythm (Boxing): 29 postures (Shift 4, ClubFeint 8, JabFeint 10, BobAndWeave 7), 12 of them lead-ins to blows; energy at the end 0.38.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 6. The posed striking part passed within 3 cm of the aim point in 6; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.5 cm, max 1.5 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 6.43 s: ClubRising at B Small's Chest: passed 0.1 cm from the aim point; nearest part B Small's HandL (41.2 cm); the Chest had moved 45.5 cm since the aim was locked
- 10.77 s: ClubSmash at B Large's Head: passed 0.0 cm from the aim point; nearest part B Large's Head (14.2 cm); the Head had moved 18.2 cm since the aim was locked
- 15.80 s: ClubSmash at B Large's Head: passed 0.0 cm from the aim point; nearest part B Large's Head (6.9 cm); the Head had moved 10.2 cm since the aim was locked
- 25.90 s: ClubSwing at B Large's KneeL: passed 0.1 cm from the aim point; nearest part B Large's ShinL (15.0 cm); the KneeL had moved 25.3 cm since the aim was locked
- 30.07 s: ClubRising at B Large's Belly: passed 0.0 cm from the aim point; nearest part B Large's HandR (22.3 cm); the Belly had moved 30.2 cm since the aim was locked
- 53.15 s: ClubRising at B Small's Belly: passed 0.1 cm from the aim point; nearest part B Small's Spine (11.0 cm); the Belly had moved 5.0 cm since the aim was locked


## B Small

Present 0.0–209.8 s. Steps 273, idle shuffles 79. Feet slid at most 0.0 cm while planted; the weight was at most 14 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 0 / 35 |
| Chest | Pinned | 0.1 / 6.8 | 0 / 38 |
| Head | Pinned | 0.1 / 10.7 | 1 / 62 |
| UpperArmL | Pinned | 0.1 / 15.1 | 1 / 167 |
| ForearmL | Pinned | 0.3 / 29.7 | 1 / 180 |
| HandL | Pinned | 0.4 / 32.9 | 4 / 178 |
| UpperArmR | Pinned | 0.1 / 9.5 | 1 / 59 |
| ForearmR | Pinned | 0.3 / 8.6 | 2 / 90 |
| HandR | Pinned | 0.5 / 21.8 | 10 / 104 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 6 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 56 damage. Hits taken: 3, 51 damage. Health at the end 9.

Fight Rhythm (Eskrima): 72 postures (TriangleStep 15, RedondaTwirl 15, BaitRetreat 18, AbanicoFlurry 24), 7 of them lead-ins to blows; energy at the end 0.50.

Balance: stumbled 1 time, knocked down 0, launched 2, got up 2; thrown into others 0 times.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.65 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinR (24.6 cm); the KneeL had moved 70.3 cm since the aim was locked
- 23.43 s: ClubSwing at A Large's Belly: passed 0.0 cm from the aim point; nearest part A Large's Pelvis (0.0 cm); the Belly had moved 21.6 cm since the aim was locked
- 64.17 s: ClubSwing at A Large's KneeR: passed 0.0 cm from the aim point; nearest part A Large's ShinR (7.1 cm); the KneeR had moved 21.9 cm since the aim was locked


## B Medium

Present 0.0–209.8 s. Steps 201, idle shuffles 50. Feet slid at most 11.3 cm while planted; the weight was at most 88 cm from the support point; hips dropped for reach by up to 21 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 1.3 | 0 / 51 |
| Chest | ? | 0.1 / 14.2 | 0 / 104 |
| Head | ? | 0.1 / 35.3 | 3 / 162 |
| UpperArmL | ? | 0.1 / 31.7 | 1 / 141 |
| ForearmL | ? | 0.5 / 54.3 | 2 / 179 |
| HandL | ? | 0.5 / 77.7 | 7 / 179 |
| UpperArmR | ? | 0.1 / 3.3 | 0 / 51 |
| ForearmR | ? | 0.2 / 13.8 | 1 / 53 |
| HandR | ? | 0.3 / 23.6 | 4 / 179 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 12 (1 on the part aimed at, 2 blocked, 0 on someone in the way, 0 on allies), 125 damage. Hits taken: 9, 35 damage. Health at the end 0.

Fight Rhythm (?): 55 postures (HighFeint 7, ShieldPress 20, GuardChange 10, Peek 18), 5 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 4 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 8. The posed striking part passed within 3 cm of the aim point in 8; aim error mean 0.1 cm, max 0.5 cm (posed); physical weapon's closest pass mean 0.3 cm, max 0.8 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.77 s: SwordThrust at A Small's Neck: passed 0.5 cm from the aim point; nearest part A Small's Chest (3.7 cm); the Neck had moved 20.2 cm since the aim was locked
- 12.13 s: SwordCut at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (10.3 cm); the Chest had moved 4.5 cm since the aim was locked
- 16.90 s: SwordCleave at A Small's Belly: passed 0.0 cm from the aim point; nearest part A Small's Chest (13.2 cm); the Belly had moved 14.4 cm since the aim was locked
- 22.07 s: SwordBackhand at A Small's Belly: passed 0.0 cm from the aim point; nearest part A Small's Spine (2.3 cm); the Belly had moved 5.7 cm since the aim was locked
- 43.73 s: SwordThrust at A Large's Neck: passed 0.1 cm from the aim point; nearest part A Large's Chest (47.3 cm); the Neck had moved 65.4 cm since the aim was locked
- 50.73 s: SwordThrust at A Large's Neck: passed 0.1 cm from the aim point; nearest part A Large's Head (8.3 cm); the Neck had moved 21.5 cm since the aim was locked
- 76.18 s: SwordCut at A Large's ElbowL: passed 0.0 cm from the aim point; nearest part A Large's Head (27.4 cm); the ElbowL had moved 79.8 cm since the aim was locked
- 80.03 s: SwordBackhand at A Medium's ElbowR: passed 0.0 cm from the aim point; nearest part A Medium's Spine (36.4 cm); the ElbowR had moved 102.3 cm since the aim was locked


## B Large

Present 0.0–209.8 s. Steps 71, idle shuffles 4. Feet slid at most 0.0 cm while planted; the weight was at most 56 cm from the support point; hips dropped for reach by up to 8 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.2 | 1 / 18 |
| Chest | Ragdoll | 0.2 / 7.8 | 0 / 12 |
| Head | Ragdoll | 0.3 / 7.3 | 4 / 62 |
| UpperArmL | Ragdoll | 0.4 / 7.7 | 2 / 34 |
| ForearmL | Ragdoll | 1.5 / 17.6 | 5 / 67 |
| HandL | Ragdoll | 2.8 / 42.8 | 12 / 144 |
| UpperArmR | Ragdoll | 0.3 / 8.7 | 1 / 46 |
| ForearmR | Ragdoll | 0.9 / 17.1 | 3 / 65 |
| HandR | Ragdoll | 1.6 / 45.7 | 11 / 165 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 10 (2 on the part aimed at, 3 blocked, 1 on someone in the way, 0 on allies), 116 damage. Hits taken: 15, 231 damage. Killed.

Fight Rhythm (Boxing): 20 postures (Shift 4, JabFeint 6, ClubFeint 8, BobAndWeave 2), 8 of them lead-ins to blows; energy at the end 0.46.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.1 cm, max 0.1 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.87 s: ClubSwing at A Medium's Belly: passed 0.1 cm from the aim point; nearest part A Medium's UpperArmL (18.7 cm); the Belly had moved 49.3 cm since the aim was locked
- 27.60 s: ClubSmash at A Large's Head: passed 0.0 cm from the aim point; nearest part A Large's HandL (41.9 cm); the Head had moved 97.1 cm since the aim was locked
- 37.13 s: ClubSmash at A Large's Neck: passed 0.0 cm from the aim point; nearest part A Large's Chest (13.2 cm); the Neck had moved 19.2 cm since the aim was locked
- 45.87 s: ClubRising at A Large's Belly: passed 0.1 cm from the aim point; nearest part A Large's ForearmL (25.6 cm); the Belly had moved 28.8 cm since the aim was locked

