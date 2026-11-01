# Playtest 2026-11-01_06-48-44

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 111.6 s of play (6699 physics steps), 1469 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-01_06-48-44`.

## A Small

Present 0.0–111.6 s. Steps 53, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 8 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 0 / 8 |
| Chest | ? | 0.1 / 1.5 | 0 / 8 |
| Head | ? | 0.1 / 1.4 | 1 / 9 |
| UpperArmL | ? | 0.1 / 1.7 | 1 / 25 |
| ForearmL | ? | 0.4 / 10.3 | 2 / 92 |
| HandL | ? | 0.6 / 24.7 | 7 / 73 |
| UpperArmR | ? | 0.1 / 1.2 | 1 / 89 |
| ForearmR | ? | 0.3 / 9.5 | 2 / 121 |
| HandR | ? | 0.6 / 27.7 | 10 / 142 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (0 on the part aimed at, 2 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 4, 66 damage. Health at the end 0.

Fight Rhythm (?): 8 postures (AbanicoFlurry 3, RedondaTwirl 1, BaitRetreat 2, TriangleStep 2), 2 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 3 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–111.6 s. Steps 171, idle shuffles 14. Feet slid at most 0.0 cm while planted; the weight was at most 38 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 5.7 | 1 / 106 |
| Chest | Pinned | 0.2 / 35.9 | 1 / 73 |
| Head | Pinned | 0.3 / 30.9 | 5 / 135 |
| UpperArmL | Pinned | 0.3 / 36.6 | 2 / 161 |
| ForearmL | Pinned | 0.9 / 41.9 | 4 / 179 |
| HandL | Pinned | 1.3 / 78.1 | 16 / 180 |
| UpperArmR | Pinned | 0.1 / 5.3 | 1 / 50 |
| ForearmR | Pinned | 0.3 / 11.6 | 1 / 48 |
| HandR | Pinned | 0.4 / 23.5 | 5 / 155 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 21 (4 on the part aimed at, 12 blocked, 0 on someone in the way, 1 on allies), 140 damage. Hits taken: 17, 88 damage. Health at the end 12.

Fight Rhythm (ShieldAndSword): 28 postures (HighFeint 10, ShieldPress 11, GuardChange 3, Peek 4), 11 of them lead-ins to blows; energy at the end 0.40.

Balance: stumbled 2 times, knocked down 2, launched 0, got up 2; thrown into others 1 time.

Blows: 7. The posed striking part passed within 3 cm of the aim point in 7; aim error mean 0.0 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.4 cm, max 0.8 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 11.80 s: SwordCleave at B Large's ElbowL: passed 0.0 cm from the aim point; nearest part B Large's Chest (3.8 cm); the ElbowL had moved 18.0 cm since the aim was locked
- 30.93 s: SwordBackhand at B Small's ElbowR: passed 0.0 cm from the aim point; nearest part B Small's HandR (28.6 cm); the ElbowR had moved 50.1 cm since the aim was locked
- 34.17 s: SwordBackhand at B Small's KneeR: passed 0.0 cm from the aim point; nearest part B Small's ShinR (4.7 cm); the KneeR had moved 0.8 cm since the aim was locked
- 37.50 s: SwordThrust at B Small's Belly: passed 0.1 cm from the aim point; nearest part B Small's Pelvis (4.4 cm); the Belly had moved 1.3 cm since the aim was locked
- 41.15 s: SwordBackhand at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Head (10.5 cm); the Neck had moved 10.3 cm since the aim was locked
- 45.82 s: SwordCut at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's HandL (25.9 cm); the Neck had moved 55.1 cm since the aim was locked
- 51.38 s: SwordBackhand at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's UpperArmL (24.8 cm); the Belly had moved 34.9 cm since the aim was locked


## A Large

Present 0.0–111.6 s. Steps 74, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 53 cm from the support point; hips dropped for reach by up to 26 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 2.5 | 1 / 53 |
| Chest | Ragdoll | 0.2 / 32.7 | 0 / 87 |
| Head | Ragdoll | 0.3 / 38.5 | 3 / 86 |
| UpperArmL | Ragdoll | 0.4 / 51.0 | 3 / 178 |
| ForearmL | Ragdoll | 1.5 / 30.3 | 5 / 94 |
| HandL | Ragdoll | 2.7 / 38.0 | 11 / 149 |
| UpperArmR | Ragdoll | 0.2 / 4.6 | 1 / 46 |
| ForearmR | Ragdoll | 0.6 / 14.6 | 2 / 65 |
| HandR | Ragdoll | 1.1 / 45.7 | 7 / 149 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 7 (1 on the part aimed at, 3 blocked, 0 on someone in the way, 0 on allies), 53 damage. Hits taken: 11, 104 damage. Killed.

Fight Rhythm (Boxing): 20 postures (Shift 5, JabFeint 6, ClubFeint 4, BobAndWeave 5), 6 of them lead-ins to blows; energy at the end 0.68.

Balance: stumbled 1 time, knocked down 1, launched 0, got up 0; thrown into others 1 time.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.0 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.2 cm, max 0.2 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 7.02 s: ClubRising at B Small's Chest: passed 0.1 cm from the aim point; nearest part B Small's UpperArmL (32.3 cm); the Chest had moved 44.7 cm since the aim was locked
- 22.23 s: ClubSmash at B Large's Neck: passed 0.0 cm from the aim point; nearest part B Large's HandR (27.6 cm); the Neck had moved 54.8 cm since the aim was locked
- 41.27 s: ClubSmash at B Medium's Neck: passed 0.0 cm from the aim point; nearest part B Medium's ForearmL (31.5 cm); the Neck had moved 61.6 cm since the aim was locked


## B Small

Present 0.0–111.6 s. Steps 163, idle shuffles 4. Feet slid at most 0.0 cm while planted; the weight was at most 33 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 2.1 | 1 / 163 |
| Chest | Ragdoll | 0.1 / 15.1 | 1 / 133 |
| Head | Ragdoll | 0.2 / 25.2 | 2 / 138 |
| UpperArmL | Ragdoll | 0.2 / 37.4 | 2 / 177 |
| ForearmL | Ragdoll | 0.4 / 34.8 | 2 / 145 |
| HandL | Ragdoll | 0.6 / 35.4 | 6 / 179 |
| UpperArmR | Ragdoll | 0.1 / 1.1 | 1 / 59 |
| ForearmR | Ragdoll | 0.3 / 8.6 | 1 / 90 |
| HandR | Ragdoll | 0.4 / 21.8 | 8 / 100 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 9 damage. Hits taken: 5, 73 damage. Killed.

Fight Rhythm (Eskrima): 8 postures (TriangleStep 2, RedondaTwirl 2, BaitRetreat 2, AbanicoFlurry 2), 3 of them lead-ins to blows; energy at the end 0.53.

Balance: stumbled 4 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.6 cm, max 0.6 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.65 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's FootL (43.5 cm); the KneeL had moved 78.3 cm since the aim was locked


## B Medium

Present 0.0–111.6 s. Steps 195, idle shuffles 11. Feet slid at most 0.0 cm while planted; the weight was at most 84 cm from the support point; hips dropped for reach by up to 16 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 4.4 | 1 / 65 |
| Chest | Pinned | 0.2 / 29.3 | 1 / 102 |
| Head | Pinned | 0.2 / 31.8 | 6 / 179 |
| UpperArmL | Pinned | 0.3 / 49.2 | 2 / 168 |
| ForearmL | Pinned | 0.9 / 34.4 | 3 / 179 |
| HandL | Pinned | 1.2 / 94.7 | 15 / 180 |
| UpperArmR | Kinematic | 0.1 / 5.4 | 1 / 51 |
| ForearmR | Kinematic | 0.3 / 11.8 | 1 / 47 |
| HandR | Kinematic | 0.5 / 23.6 | 6 / 178 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 21 (2 on the part aimed at, 10 blocked, 0 on someone in the way, 0 on allies), 151 damage. Hits taken: 20, 57 damage. Health at the end 43.

Fight Rhythm (ShieldAndSword): 27 postures (HighFeint 11, ShieldPress 5, GuardChange 5, Peek 6), 10 of them lead-ins to blows; energy at the end 0.53.

Balance: stumbled 4 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 6. The posed striking part passed within 3 cm of the aim point in 6; aim error mean 0.1 cm, max 0.6 cm (posed); physical weapon's closest pass mean 0.5 cm, max 0.9 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.93 s: SwordThrust at A Small's Head: passed 0.6 cm from the aim point; nearest part A Small's UpperArmL (17.6 cm); the Head had moved 31.8 cm since the aim was locked
- 12.28 s: SwordBackhand at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Spine (0.0 cm); the Chest had moved 7.0 cm since the aim was locked
- 16.73 s: SwordCut at A Small's Belly: passed 0.0 cm from the aim point; nearest part A Small's HandL (17.5 cm); the Belly had moved 30.1 cm since the aim was locked
- 20.98 s: SwordCleave at A Small's ElbowL: passed 0.0 cm from the aim point; nearest part A Small's Chest (13.6 cm); the ElbowL had moved 10.3 cm since the aim was locked
- 49.50 s: SwordBackhand at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (10.5 cm); the KneeL had moved 8.0 cm since the aim was locked
- 104.08 s: SwordBackhand at A Medium's Neck: passed 0.0 cm from the aim point; nearest part A Medium's Chest (10.8 cm); the Neck had moved 0.4 cm since the aim was locked


## B Large

Present 0.0–111.6 s. Steps 60, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 35 cm from the support point; hips dropped for reach by up to 8 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.2 | 1 / 12 |
| Chest | Ragdoll | 0.2 / 5.2 | 0 / 14 |
| Head | Ragdoll | 0.3 / 6.5 | 4 / 55 |
| UpperArmL | Ragdoll | 0.4 / 8.5 | 5 / 68 |
| ForearmL | Ragdoll | 0.6 / 21.8 | 2 / 69 |
| HandL | Ragdoll | 1.0 / 33.2 | 4 / 71 |
| UpperArmR | Ragdoll | 0.2 / 5.9 | 1 / 46 |
| ForearmR | Ragdoll | 0.7 / 16.4 | 2 / 65 |
| HandR | Ragdoll | 1.3 / 45.7 | 9 / 153 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 6 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 61 damage. Hits taken: 5, 63 damage. Killed.

Fight Rhythm (Boxing): 12 postures (Shift 1, JabFeint 4, BobAndWeave 3, ClubFeint 4), 5 of them lead-ins to blows; energy at the end 0.80.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.9 cm, max 1.1 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.98 s: ClubRising at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Chest (21.2 cm); the Chest had moved 35.9 cm since the aim was locked
- 9.93 s: ClubSmash at A Large's Head: passed 0.0 cm from the aim point; nearest part A Large's HandL (16.1 cm); the Head had moved 50.3 cm since the aim was locked
- 14.28 s: ClubSwing at A Medium's Neck: passed 0.1 cm from the aim point; nearest part A Medium's Chest (5.5 cm); the Neck had moved 8.5 cm since the aim was locked

