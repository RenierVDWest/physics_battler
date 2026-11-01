# Playtest 2026-10-31_21-46-23

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 25.0 s of play (1502 physics steps), 871 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_21-46-23`.

## A Small

Present 0.0–25.0 s. Steps 68, idle shuffles 4. Feet slid at most 0.0 cm while planted; the weight was at most 16 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 3.6 | 1 / 53 |
| Chest | ? | 0.2 / 25.3 | 1 / 91 |
| Head | ? | 0.3 / 25.1 | 4 / 120 |
| UpperArmL | ? | 0.3 / 33.5 | 2 / 177 |
| ForearmL | ? | 0.6 / 75.9 | 3 / 179 |
| HandL | ? | 0.9 / 69.5 | 9 / 173 |
| UpperArmR | ? | 0.1 / 1.1 | 1 / 70 |
| ForearmR | ? | 0.4 / 7.1 | 2 / 96 |
| HandR | ? | 0.7 / 25.9 | 12 / 148 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 3 (0 on the part aimed at, 3 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 4, 32 damage. Health at the end 0.

Fight Rhythm (?): 14 postures (TriangleStep 3, AbanicoFlurry 4, BaitRetreat 4, RedondaTwirl 3), 3 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 3 times, knocked down 0, launched 1, got up 1; thrown into others 0 times.


## A Medium

Present 0.0–25.0 s. Steps 64, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 28 cm from the support point; hips dropped for reach by up to 10 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.6 | 1 / 100 |
| Chest | ? | 0.2 / 29.1 | 1 / 64 |
| Head | ? | 0.4 / 29.5 | 5 / 121 |
| UpperArmL | ? | 0.5 / 42.4 | 3 / 159 |
| ForearmL | ? | 0.9 / 40.3 | 4 / 176 |
| HandL | ? | 1.2 / 62.5 | 13 / 144 |
| UpperArmR | ? | 0.2 / 7.6 | 1 / 45 |
| ForearmR | ? | 0.4 / 14.4 | 2 / 135 |
| HandR | ? | 0.7 / 29.0 | 8 / 179 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 5 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 2 on allies), 70 damage. Hits taken: 3, 28 damage. Health at the end 0.

Fight Rhythm (?): 11 postures (HighFeint 3, ShieldPress 5, Peek 2, GuardChange 1), 4 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Large

Present 0.0–25.0 s. Steps 38, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 31 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 1 / 18 |
| Chest | ? | 0.2 / 8.1 | 0 / 22 |
| Head | ? | 0.3 / 10.5 | 4 / 60 |
| UpperArmL | ? | 0.4 / 17.6 | 2 / 50 |
| ForearmL | ? | 1.5 / 26.7 | 4 / 70 |
| HandL | ? | 2.4 / 48.0 | 12 / 110 |
| UpperArmR | ? | 0.2 / 3.5 | 1 / 52 |
| ForearmR | ? | 0.7 / 17.3 | 2 / 68 |
| HandR | ? | 1.1 / 36.8 | 8 / 121 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 6 (2 on the part aimed at, 0 blocked, 2 on someone in the way, 0 on allies), 81 damage. Hits taken: 4, 41 damage. Health at the end 0.

Fight Rhythm (?): 12 postures (Shift 1, ClubFeint 4, JabFeint 4, BobAndWeave 3), 5 of them lead-ins to blows; energy at the end 0.00.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.17 s: ClubSmash at B Large's Head: passed 0.0 cm from the aim point; nearest part B Large's Head (0.0 cm); the Head had moved 4.6 cm since the aim was locked
- 9.47 s: ClubSwing at B Small's Belly: passed 0.1 cm from the aim point; nearest part B Small's ThighL (18.9 cm); the Belly had moved 61.5 cm since the aim was locked
- 19.25 s: ClubRising at B Small's Belly: passed 0.1 cm from the aim point; nearest part B Small's Pelvis (20.0 cm); the Belly had moved 37.5 cm since the aim was locked


## B Small

Present 0.0–25.0 s. Steps 79, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 7 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 0 / 3 |
| Chest | ? | 0.1 / 0.7 | 0 / 9 |
| Head | ? | 0.1 / 2.0 | 1 / 27 |
| UpperArmL | ? | 0.1 / 3.4 | 1 / 47 |
| ForearmL | ? | 0.4 / 10.9 | 2 / 106 |
| HandL | ? | 0.6 / 20.6 | 8 / 116 |
| UpperArmR | ? | 0.1 / 1.1 | 1 / 60 |
| ForearmR | ? | 0.4 / 12.4 | 2 / 90 |
| HandR | ? | 0.7 / 20.1 | 12 / 100 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 8 damage. Hits taken: 4, 33 damage. Health at the end 0.

Fight Rhythm (?): 11 postures (TriangleStep 2, AbanicoFlurry 5, BaitRetreat 3, RedondaTwirl 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 0 times, knocked down 0, launched 2, got up 2; thrown into others 2 times.


## B Medium

Present 0.0–25.0 s. Steps 50, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 27 cm from the support point; hips dropped for reach by up to 4 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 1 / 43 |
| Chest | ? | 0.2 / 10.2 | 1 / 46 |
| Head | ? | 0.2 / 14.8 | 4 / 61 |
| UpperArmL | ? | 0.3 / 25.1 | 1 / 86 |
| ForearmL | ? | 0.7 / 36.6 | 3 / 157 |
| HandL | ? | 1.0 / 13.5 | 12 / 120 |
| UpperArmR | ? | 0.1 / 1.2 | 0 / 49 |
| ForearmR | ? | 0.2 / 4.5 | 1 / 80 |
| HandR | ? | 0.4 / 28.6 | 5 / 154 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 6 (1 on the part aimed at, 1 blocked, 0 on someone in the way, 1 on allies), 64 damage. Hits taken: 4, 16 damage. Health at the end 0.

Fight Rhythm (?): 9 postures (HighFeint 3, ShieldPress 4, Peek 2), 2 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 1, launched 0, got up 1; thrown into others 0 times.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 1.7 cm, max 1.7 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 5.27 s: SwordCut at A Small's ElbowL: passed 0.0 cm from the aim point; nearest part A Small's Pelvis (9.9 cm); the ElbowL had moved 3.3 cm since the aim was locked
- 8.85 s: SwordCleave at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (0.0 cm); the Chest had moved 9.3 cm since the aim was locked
- 14.22 s: SwordCleave at A Small's Belly: passed 0.0 cm from the aim point; nearest part A Small's Spine (7.0 cm); the Belly had moved 14.8 cm since the aim was locked


## B Large

Present 0.0–25.0 s. Steps 37, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 75 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 1 / 45 |
| Chest | ? | 0.4 / 18.4 | 1 / 70 |
| Head | ? | 0.5 / 44.5 | 4 / 82 |
| UpperArmL | ? | 0.7 / 65.7 | 3 / 174 |
| ForearmL | ? | 1.7 / 48.5 | 5 / 143 |
| HandL | ? | 2.5 / 64.5 | 13 / 158 |
| UpperArmR | ? | 0.3 / 29.6 | 1 / 55 |
| ForearmR | ? | 0.7 / 20.8 | 2 / 72 |
| HandR | ? | 1.1 / 38.0 | 8 / 179 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 4 (0 on the part aimed at, 1 blocked, 0 on someone in the way, 1 on allies), 28 damage. Hits taken: 10, 127 damage. Health at the end 0.

Fight Rhythm (?): 11 postures (Shift 1, JabFeint 4, ClubFeint 3, BobAndWeave 3), 3 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 18.95 s: ClubRising at A Small's KneeR: passed 0.0 cm from the aim point; nearest part A Small's FootR (34.0 cm); the KneeR had moved 77.0 cm since the aim was locked

