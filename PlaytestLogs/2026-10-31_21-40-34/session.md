# Playtest 2026-10-31_21-40-34

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 272.6 s of play (16355 physics steps), 3539 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_21-40-34`.

## A Small

Present 0.0–10.2 s. Steps 40, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 10 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 0 / 2 |
| Chest | ? | 0.1 / 0.4 | 0 / 5 |
| Head | ? | 0.1 / 1.1 | 2 / 9 |
| UpperArmL | ? | 0.1 / 0.8 | 1 / 48 |
| ForearmL | ? | 0.3 / 4.9 | 2 / 57 |
| HandL | ? | 0.5 / 18.7 | 8 / 114 |
| UpperArmR | ? | 0.1 / 0.9 | 1 / 70 |
| ForearmR | ? | 0.3 / 7.1 | 2 / 96 |
| HandR | ? | 0.6 / 25.9 | 10 / 148 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 10 damage. Hits taken: 2, 17 damage. Health at the end 0.

Fight Rhythm (?): 6 postures (TriangleStep 3, AbanicoFlurry 2, BaitRetreat 1), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 6.93 s: ClubSwing at B Medium's KneeL: passed 0.0 cm from the aim point; nearest part B Medium's ThighL (8.7 cm); the KneeL had moved 14.6 cm since the aim was locked


## A Medium

Present 0.0–10.2 s. Steps 24, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 15 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 1.0 | 0 / 44 |
| Chest | ? | 0.1 / 15.7 | 1 / 70 |
| Head | ? | 0.3 / 23.1 | 3 / 156 |
| UpperArmL | ? | 0.3 / 31.4 | 3 / 174 |
| ForearmL | ? | 0.7 / 47.9 | 3 / 169 |
| HandL | ? | 0.7 / 14.6 | 9 / 175 |
| UpperArmR | ? | 0.1 / 2.0 | 2 / 105 |
| ForearmR | ? | 0.5 / 16.6 | 3 / 106 |
| HandR | ? | 1.0 / 29.0 | 8 / 123 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 2, 40 damage. Health at the end 0.

Fight Rhythm (?): 3 postures (HighFeint 2, ShieldPress 1), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 1, launched 0, got up 1; thrown into others 1 time.


## A Large

Present 0.0–10.2 s. Steps 19, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 18 cm from the support point; hips dropped for reach by up to 5 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 0 / 12 |
| Chest | ? | 0.2 / 5.1 | 0 / 14 |
| Head | ? | 0.3 / 11.2 | 4 / 55 |
| UpperArmL | ? | 0.4 / 13.8 | 2 / 45 |
| ForearmL | ? | 1.3 / 26.7 | 4 / 70 |
| HandL | ? | 2.1 / 48.0 | 11 / 105 |
| UpperArmR | ? | 0.3 / 6.6 | 2 / 98 |
| ForearmR | ? | 0.8 / 22.5 | 3 / 108 |
| HandR | ? | 1.3 / 36.8 | 8 / 116 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 8 damage. Hits taken: 4, 74 damage. Health at the end 0.

Fight Rhythm (?): 5 postures (Shift 1, ClubFeint 1, JabFeint 2, BobAndWeave 1), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## B Small

Present 0.0–10.2 s. Steps 35, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 6 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 0 / 5 |
| Chest | ? | 0.1 / 1.0 | 0 / 4 |
| Head | ? | 0.1 / 1.2 | 2 / 20 |
| UpperArmL | ? | 0.1 / 0.9 | 1 / 47 |
| ForearmL | ? | 0.4 / 10.6 | 2 / 57 |
| HandL | ? | 0.5 / 18.7 | 7 / 116 |
| UpperArmR | ? | 0.1 / 1.1 | 1 / 60 |
| ForearmR | ? | 0.4 / 12.4 | 2 / 90 |
| HandR | ? | 0.7 / 20.1 | 12 / 100 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 8 damage. Hits taken: 1, 8 damage. Health at the end 0.

Fight Rhythm (?): 6 postures (TriangleStep 2, AbanicoFlurry 3, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 0 times, knocked down 0, launched 1, got up 0; thrown into others 0 times.


## B Medium

Present 0.0–10.2 s. Steps 26, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 35 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.0 | 1 / 9 |
| Chest | ? | 0.2 / 2.6 | 0 / 6 |
| Head | ? | 0.2 / 2.5 | 4 / 34 |
| UpperArmL | ? | 0.2 / 4.7 | 1 / 25 |
| ForearmL | ? | 0.7 / 12.2 | 3 / 51 |
| HandL | ? | 1.0 / 13.5 | 14 / 160 |
| UpperArmR | ? | 0.1 / 0.9 | 0 / 49 |
| ForearmR | ? | 0.2 / 4.5 | 1 / 80 |
| HandR | ? | 0.3 / 28.6 | 3 / 89 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 17 damage. Hits taken: 1, 10 damage. Health at the end 0.

Fight Rhythm (?): 3 postures (HighFeint 1, ShieldPress 1, Peek 1), 0 of them lead-ins to blows; energy at the end 0.00.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.7 cm, max 0.8 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 5.30 s: SwordBackhand at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (0.0 cm); the Chest had moved 2.6 cm since the aim was locked
- 8.85 s: SwordBackhand at A Small's KneeR: passed 0.0 cm from the aim point; nearest part A Small's ShinR (10.6 cm); the KneeR had moved 5.3 cm since the aim was locked


## B Large

Present 0.0–10.2 s. Steps 23, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 29 cm from the support point; hips dropped for reach by up to 5 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.5 | 0 / 31 |
| Chest | ? | 0.2 / 15.0 | 0 / 30 |
| Head | ? | 0.2 / 12.2 | 6 / 172 |
| UpperArmL | ? | 0.4 / 20.1 | 2 / 47 |
| ForearmL | ? | 1.5 / 27.4 | 5 / 92 |
| HandL | ? | 2.5 / 48.6 | 13 / 132 |
| UpperArmR | ? | 0.1 / 3.2 | 1 / 55 |
| ForearmR | ? | 0.6 / 16.6 | 2 / 72 |
| HandR | ? | 0.9 / 38.0 | 6 / 116 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 3 (2 on the part aimed at, 0 blocked, 1 on someone in the way, 0 on allies), 62 damage. Hits taken: 0, 0 damage. Health at the end 0.

Fight Rhythm (?): 5 postures (Shift 1, JabFeint 2, BobAndWeave 1, ClubFeint 1), 2 of them lead-ins to blows; energy at the end 0.00.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.27 s: ClubSwing at A Medium's Neck: passed 0.1 cm from the aim point; nearest part A Medium's Chest (18.7 cm); the Neck had moved 22.1 cm since the aim was locked


## A Small #2

Present 10.2–73.9 s. Steps 105, idle shuffles 24. Feet slid at most 0.0 cm while planted; the weight was at most 4 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 0 / 11 |
| Chest | ? | 0.1 / 2.1 | 0 / 38 |
| Head | ? | 0.1 / 10.0 | 1 / 83 |
| UpperArmL | ? | 0.1 / 6.0 | 1 / 54 |
| ForearmL | ? | 0.2 / 5.7 | 2 / 61 |
| HandL | ? | 0.5 / 20.0 | 9 / 146 |
| UpperArmR | ? | 0.1 / 7.8 | 1 / 71 |
| ForearmR | ? | 0.3 / 7.4 | 2 / 92 |
| HandR | ? | 0.6 / 25.9 | 12 / 143 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 1, 6 damage. Health at the end 0.

Fight Rhythm (?): 39 postures (BaitRetreat 9, AbanicoFlurry 11, TriangleStep 12, RedondaTwirl 7), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium #2

Present 10.2–73.9 s. Steps 107, idle shuffles 9. Feet slid at most 0.0 cm while planted; the weight was at most 40 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 0 / 13 |
| Chest | ? | 0.1 / 3.8 | 0 / 8 |
| Head | ? | 0.1 / 3.5 | 2 / 23 |
| UpperArmL | ? | 0.1 / 6.0 | 1 / 34 |
| ForearmL | ? | 0.3 / 15.7 | 1 / 60 |
| HandL | ? | 0.5 / 15.3 | 6 / 157 |
| UpperArmR | ? | 0.1 / 2.5 | 0 / 48 |
| ForearmR | ? | 0.2 / 21.6 | 1 / 84 |
| HandR | ? | 0.4 / 29.0 | 5 / 173 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 1 on allies), 10 damage. Hits taken: 0, 0 damage. Health at the end 0.

Fight Rhythm (?): 25 postures (GuardChange 7, ShieldPress 8, Peek 3, HighFeint 7), 0 of them lead-ins to blows; energy at the end 0.00.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.8 cm, max 1.2 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 51.28 s: SwordCut at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's Spine (6.4 cm); the Belly had moved 0.3 cm since the aim was locked
- 57.33 s: SwordCut at B Small's Chest: passed 0.0 cm from the aim point; nearest part B Small's HandL (8.8 cm); the Chest had moved 9.7 cm since the aim was locked
- 59.65 s: SwordCut at B Small's Chest: passed 0.0 cm from the aim point; nearest part B Small's Chest (7.4 cm); the Chest had moved 5.0 cm since the aim was locked
- 61.95 s: SwordBackhand at B Small's Chest: passed 0.0 cm from the aim point; nearest part B Small's UpperArmL (8.6 cm); the Chest had moved 12.3 cm since the aim was locked


## A Large #2

Present 10.2–73.9 s. Steps 47, idle shuffles 8. Feet slid at most 0.0 cm while planted; the weight was at most 57 cm from the support point; hips dropped for reach by up to 4 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 0 / 8 |
| Chest | ? | 0.1 / 3.4 | 0 / 6 |
| Head | ? | 0.1 / 4.9 | 1 / 16 |
| UpperArmL | ? | 0.1 / 8.0 | 1 / 64 |
| ForearmL | ? | 0.8 / 27.2 | 2 / 78 |
| HandL | ? | 1.1 / 47.8 | 6 / 118 |
| UpperArmR | ? | 0.1 / 2.9 | 1 / 52 |
| ForearmR | ? | 0.4 / 15.0 | 1 / 68 |
| HandR | ? | 0.7 / 38.0 | 5 / 112 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (0 on the part aimed at, 0 blocked, 1 on someone in the way, 0 on allies), 17 damage. Hits taken: 0, 0 damage. Health at the end 0.

Fight Rhythm (?): 29 postures (JabFeint 10, BobAndWeave 6, ClubFeint 11, Shift 2), 0 of them lead-ins to blows; energy at the end 0.00.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 1.3 cm, max 1.3 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 68.87 s: ClubSwing at B Small's Chest: passed 0.1 cm from the aim point; nearest part B Small's Chest (1.7 cm); the Chest had moved 7.6 cm since the aim was locked


## B Small #2

Present 10.2–73.9 s. Steps 82, idle shuffles 25. Feet slid at most 0.0 cm while planted; the weight was at most 4 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 0 / 3 |
| Chest | ? | 0.0 / 0.5 | 0 / 4 |
| Head | ? | 0.1 / 0.9 | 1 / 6 |
| UpperArmL | ? | 0.1 / 1.1 | 1 / 51 |
| ForearmL | ? | 0.2 / 5.2 | 2 / 59 |
| HandL | ? | 0.5 / 19.2 | 9 / 130 |
| UpperArmR | ? | 0.1 / 1.0 | 1 / 56 |
| ForearmR | ? | 0.4 / 10.2 | 2 / 86 |
| HandR | ? | 0.7 / 21.3 | 14 / 140 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 5, 26 damage. Health at the end 0.

Fight Rhythm (?): 36 postures (RedondaTwirl 7, BaitRetreat 10, AbanicoFlurry 13, TriangleStep 6), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 0, launched 1, got up 0; thrown into others 2 times.


## B Medium #2

Present 10.2–73.9 s. Steps 110, idle shuffles 10. Feet slid at most 0.0 cm while planted; the weight was at most 11 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 0 / 3 |
| Chest | ? | 0.0 / 0.8 | 0 / 5 |
| Head | ? | 0.1 / 1.5 | 1 / 10 |
| UpperArmL | ? | 0.1 / 1.5 | 0 / 23 |
| ForearmL | ? | 0.3 / 7.6 | 1 / 43 |
| HandL | ? | 0.4 / 14.9 | 4 / 112 |
| UpperArmR | ? | 0.1 / 2.5 | 1 / 48 |
| ForearmR | ? | 0.4 / 20.5 | 2 / 80 |
| HandR | ? | 0.6 / 28.0 | 7 / 180 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 4, 23 damage. Health at the end 0.

Fight Rhythm (?): 25 postures (ShieldPress 8, Peek 6, GuardChange 4, HighFeint 7), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 1, launched 0, got up 0; thrown into others 1 time.


## B Large #2

Present 10.2–73.9 s. Steps 43, idle shuffles 10. Feet slid at most 0.0 cm while planted; the weight was at most 12 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 0 / 3 |
| Chest | ? | 0.1 / 1.3 | 0 / 3 |
| Head | ? | 0.1 / 2.3 | 1 / 10 |
| UpperArmL | ? | 0.1 / 2.7 | 1 / 51 |
| ForearmL | ? | 0.6 / 28.0 | 2 / 74 |
| HandL | ? | 0.9 / 48.9 | 5 / 103 |
| UpperArmR | ? | 0.1 / 2.6 | 0 / 55 |
| ForearmR | ? | 0.3 / 16.1 | 1 / 72 |
| HandR | ? | 0.5 / 39.9 | 4 / 117 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Fight Rhythm (?): 29 postures (ClubFeint 10, Shift 4, JabFeint 9, BobAndWeave 6), 0 of them lead-ins to blows; energy at the end 0.00.


## A Small #3

Present 73.9–272.6 s. Steps 265, idle shuffles 83. Feet slid at most 0.0 cm while planted; the weight was at most 4 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 2.3 | 0 / 36 |
| Chest | ? | 0.0 / 6.2 | 0 / 23 |
| Head | ? | 0.1 / 2.5 | 1 / 25 |
| UpperArmL | ? | 0.1 / 2.8 | 0 / 49 |
| ForearmL | ? | 0.1 / 3.5 | 1 / 56 |
| HandL | ? | 0.2 / 18.2 | 4 / 112 |
| UpperArmR | ? | 0.1 / 6.6 | 1 / 171 |
| ForearmR | ? | 0.4 / 16.6 | 2 / 172 |
| HandR | ? | 0.6 / 25.7 | 13 / 147 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Fight Rhythm (?): 110 postures (AbanicoFlurry 38, RedondaTwirl 24, TriangleStep 26, BaitRetreat 22), 0 of them lead-ins to blows; energy at the end 0.00.


## A Medium #3

Present 73.9–272.6 s. Steps 315, idle shuffles 34. Feet slid at most 0.0 cm while planted; the weight was at most 9 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 0 / 2 |
| Chest | ? | 0.0 / 0.5 | 0 / 2 |
| Head | ? | 0.0 / 0.7 | 1 / 3 |
| UpperArmL | ? | 0.0 / 0.6 | 0 / 27 |
| ForearmL | ? | 0.2 / 4.8 | 1 / 46 |
| HandL | ? | 0.3 / 16.2 | 4 / 115 |
| UpperArmR | ? | 0.0 / 0.9 | 0 / 43 |
| ForearmR | ? | 0.1 / 4.6 | 1 / 80 |
| HandR | ? | 0.3 / 29.5 | 4 / 85 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Fight Rhythm (?): 75 postures (Peek 20, GuardChange 17, HighFeint 25, ShieldPress 13), 0 of them lead-ins to blows; energy at the end 0.00.


## A Large #3

Present 73.9–272.6 s. Steps 83, idle shuffles 31. Feet slid at most 0.0 cm while planted; the weight was at most 14 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.0 | 0 / 1 |
| Chest | ? | 0.1 / 0.5 | 0 / 3 |
| Head | ? | 0.1 / 1.2 | 1 / 8 |
| UpperArmL | ? | 0.1 / 2.2 | 1 / 47 |
| ForearmL | ? | 0.4 / 26.9 | 1 / 70 |
| HandL | ? | 0.5 / 46.9 | 3 / 103 |
| UpperArmR | ? | 0.1 / 2.3 | 0 / 52 |
| ForearmR | ? | 0.2 / 15.8 | 0 / 69 |
| HandR | ? | 0.4 / 37.4 | 3 / 114 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Fight Rhythm (?): 85 postures (BobAndWeave 23, Shift 18, JabFeint 23, ClubFeint 21), 0 of them lead-ins to blows; energy at the end 0.00.


## B Small #3

Present 73.9–272.6 s. Steps 246, idle shuffles 86. Feet slid at most 0.0 cm while planted; the weight was at most 4 cm from the support point; hips dropped for reach by up to 2 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.0 | 0 / 1 |
| Chest | ? | 0.0 / 0.3 | 0 / 3 |
| Head | ? | 0.1 / 1.0 | 1 / 6 |
| UpperArmL | ? | 0.1 / 0.6 | 0 / 48 |
| ForearmL | ? | 0.1 / 5.0 | 1 / 56 |
| HandL | ? | 0.2 / 18.3 | 4 / 114 |
| UpperArmR | ? | 0.1 / 0.9 | 1 / 61 |
| ForearmR | ? | 0.3 / 10.7 | 2 / 90 |
| HandR | ? | 0.6 / 21.3 | 12 / 114 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Fight Rhythm (?): 113 postures (AbanicoFlurry 37, BaitRetreat 18, RedondaTwirl 26, TriangleStep 32), 0 of them lead-ins to blows; energy at the end 0.00.


## B Medium #3

Present 73.9–272.6 s. Steps 317, idle shuffles 35. Feet slid at most 0.0 cm while planted; the weight was at most 10 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 0 / 1 |
| Chest | Pinned | 0.0 / 0.3 | 0 / 2 |
| Head | Pinned | 0.0 / 0.6 | 1 / 4 |
| UpperArmL | Pinned | 0.0 / 0.5 | 0 / 25 |
| ForearmL | Pinned | 0.2 / 6.3 | 1 / 45 |
| HandL | Pinned | 0.3 / 14.7 | 3 / 115 |
| UpperArmR | Pinned | 0.0 / 0.7 | 0 / 46 |
| ForearmR | Pinned | 0.1 / 4.4 | 1 / 78 |
| HandR | Pinned | 0.2 / 28.7 | 4 / 88 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Fight Rhythm (ShieldAndSword): 78 postures (HighFeint 26, Peek 20, GuardChange 17, ShieldPress 15), 0 of them lead-ins to blows; energy at the end 0.83.


## B Large #3

Present 73.9–272.6 s. Steps 92, idle shuffles 32. Feet slid at most 0.0 cm while planted; the weight was at most 15 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.0 | 0 / 1 |
| Chest | ? | 0.1 / 0.6 | 0 / 4 |
| Head | ? | 0.1 / 2.1 | 1 / 7 |
| UpperArmL | ? | 0.1 / 2.6 | 1 / 44 |
| ForearmL | ? | 0.4 / 25.9 | 1 / 72 |
| HandL | ? | 0.5 / 47.7 | 3 / 103 |
| UpperArmR | ? | 0.1 / 2.9 | 0 / 56 |
| ForearmR | ? | 0.2 / 18.6 | 0 / 73 |
| HandR | ? | 0.4 / 37.9 | 3 / 123 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Fight Rhythm (?): 86 postures (JabFeint 26, ClubFeint 25, Shift 19, BobAndWeave 16), 0 of them lead-ins to blows; energy at the end 0.00.

## Console warnings and errors

- 74.16 s Exception: MissingReferenceException: The object of type 'PhysicsBattler.Units.UnitController' has been destroyed but you are still trying to access it. Your script should either check if it is null or you should not destroy the object.

