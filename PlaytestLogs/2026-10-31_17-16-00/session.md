# Playtest 2026-10-31_17-16-00

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 147.7 s of play (8863 physics steps), 2828 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_17-16-00`.

## A Small

Present 0.0–52.8 s. Steps 34, idle shuffles 4. Feet slid at most 85.0 cm while planted; the weight was at most 14 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 41.3 / 67.8 | 102 / 178 |
| Spine | ? | 45.3 / 67.8 | 106 / 180 |
| Chest | ? | 52.4 / 75.4 | 95 / 167 |
| Head | ? | 63.3 / 99.9 | 85 / 146 |
| UpperArmL | ? | 57.8 / 89.2 | 81 / 170 |
| ForearmL | ? | 57.4 / 89.4 | 88 / 180 |
| HandL | ? | 75.7 / 122.8 | 94 / 179 |
| UpperArmR | ? | 64.7 / 97.8 | 100 / 180 |
| ForearmR | ? | 59.6 / 89.1 | 115 / 180 |
| HandR | ? | 63.9 / 101.3 | 103 / 179 |
| ThighL | ? | 39.1 / 69.0 | 84 / 179 |
| ShinL | ? | 40.3 / 81.7 | 81 / 180 |
| FootL | ? | 42.3 / 85.0 | 85 / 178 |
| ThighR | ? | 42.0 / 69.9 | 107 / 180 |
| ShinR | ? | 46.6 / 87.9 | 117 / 180 |
| FootR | ? | 39.5 / 81.3 | 111 / 180 |

Hits dealt: 3 (0 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 16 damage. Hits taken: 4, 63 damage. Health at the end 0.


## A Medium

Present 0.0–52.8 s. Steps 80, idle shuffles 11. Feet slid at most 0.0 cm while planted; the weight was at most 33 cm from the support point; hips dropped for reach by up to 8 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 1 / 42 |
| Chest | ? | 0.2 / 11.8 | 1 / 44 |
| Head | ? | 0.4 / 13.7 | 7 / 112 |
| UpperArmL | ? | 0.4 / 7.9 | 1 / 40 |
| ForearmL | ? | 0.7 / 13.3 | 3 / 69 |
| HandL | ? | 1.1 / 20.5 | 12 / 179 |
| UpperArmR | ? | 0.3 / 18.9 | 1 / 160 |
| ForearmR | ? | 0.4 / 35.0 | 2 / 118 |
| HandR | ? | 0.5 / 23.4 | 6 / 155 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 14 (3 on the part aimed at, 10 blocked, 0 on someone in the way, 0 on allies), 79 damage. Hits taken: 9, 75 damage. Health at the end 0.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 36.23 s: SwordCut at B Medium's ElbowL: passed 0.1 cm from the aim point; nearest part B Medium's ForearmL (29.0 cm); the ElbowL had moved 36.3 cm since the aim was locked
- 47.70 s: SwordThrust at B Medium's Neck: passed 0.0 cm from the aim point; nearest part B Medium's HandL (35.4 cm); the Neck had moved 105.4 cm since the aim was locked


## A Large

Present 0.0–52.8 s. Steps 69, idle shuffles 5. Feet slid at most 0.0 cm while planted; the weight was at most 21 cm from the support point; hips dropped for reach by up to 6 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 0 / 16 |
| Chest | ? | 0.2 / 6.7 | 0 / 13 |
| Head | ? | 0.2 / 7.8 | 3 / 69 |
| UpperArmL | ? | 0.3 / 12.0 | 2 / 71 |
| ForearmL | ? | 1.4 / 52.7 | 5 / 160 |
| HandL | ? | 2.7 / 47.3 | 16 / 175 |
| UpperArmR | ? | 0.1 / 6.6 | 0 / 56 |
| ForearmR | ? | 0.2 / 8.2 | 1 / 60 |
| HandR | ? | 0.4 / 36.9 | 2 / 80 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 14 (8 on the part aimed at, 2 blocked, 2 on someone in the way, 0 on allies), 325 damage. Hits taken: 9, 142 damage. Health at the end 0.


## B Small

Present 0.0–52.8 s. Steps 32, idle shuffles 2. Feet slid at most 698.4 cm while planted; the weight was at most 14 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 509.7 / 658.3 | 68 / 180 |
| Spine | ? | 512.8 / 662.5 | 85 / 180 |
| Chest | ? | 519.7 / 672.2 | 102 / 180 |
| Head | ? | 532.2 / 691.9 | 112 / 179 |
| UpperArmL | ? | 523.5 / 678.7 | 37 / 180 |
| ForearmL | ? | 523.2 / 697.1 | 66 / 179 |
| HandL | ? | 521.6 / 705.3 | 107 / 180 |
| UpperArmR | ? | 533.3 / 695.3 | 95 / 179 |
| ForearmR | ? | 556.5 / 717.9 | 131 / 179 |
| HandR | ? | 558.4 / 721.2 | 139 / 179 |
| ThighL | ? | 505.5 / 653.4 | 65 / 180 |
| ShinL | ? | 522.4 / 675.7 | 76 / 180 |
| FootL | ? | 539.6 / 698.4 | 95 / 179 |
| ThighR | ? | 511.6 / 663.0 | 100 / 178 |
| ShinR | ? | 535.0 / 692.1 | 108 / 180 |
| FootR | ? | 547.3 / 707.8 | 93 / 180 |

Hits dealt: 2 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 17 damage. Hits taken: 3, 72 damage. Health at the end 0.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 6.90 s: ClubSwing at A Large's KneeL: passed 0.1 cm from the aim point; nearest part A Large's ShinL (4.4 cm); the KneeL had moved 9.3 cm since the aim was locked


## B Medium

Present 0.0–52.8 s. Steps 79, idle shuffles 8. Feet slid at most 25.5 cm while planted; the weight was at most 36 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 8.7 / 90.9 | 9 / 100 |
| Spine | ? | 10.6 / 111.9 | 11 / 121 |
| Chest | ? | 13.5 / 138.3 | 10 / 153 |
| Head | ? | 17.0 / 172.9 | 13 / 179 |
| UpperArmL | ? | 16.8 / 175.4 | 11 / 177 |
| ForearmL | ? | 15.0 / 150.9 | 17 / 180 |
| HandL | ? | 13.6 / 132.8 | 22 / 178 |
| UpperArmR | ? | 15.0 / 155.2 | 13 / 180 |
| ForearmR | ? | 11.7 / 121.1 | 12 / 178 |
| HandR | ? | 10.4 / 118.7 | 16 / 180 |
| ThighL | ? | 7.8 / 82.2 | 10 / 114 |
| ShinL | ? | 7.3 / 77.5 | 9 / 109 |
| FootL | ? | 1.9 / 20.6 | 9 / 115 |
| ThighR | ? | 8.2 / 85.6 | 10 / 108 |
| ShinR | ? | 5.6 / 62.6 | 9 / 116 |
| FootR | ? | 1.9 / 27.2 | 10 / 131 |

Hits dealt: 11 (3 on the part aimed at, 3 blocked, 0 on someone in the way, 0 on allies), 127 damage. Hits taken: 18, 111 damage. Health at the end 0.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.0 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.1 cm, max 0.1 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 23.78 s: SwordCut at A Medium's KneeR: passed 0.1 cm from the aim point; nearest part A Medium's ThighR (51.4 cm); the KneeR had moved 49.6 cm since the aim was locked
- 29.52 s: SwordThrust at A Medium's Neck: passed 0.0 cm from the aim point; nearest part A Medium's Head (18.0 cm); the Neck had moved 24.8 cm since the aim was locked
- 35.80 s: SwordThrust at A Large's Belly: passed 0.0 cm from the aim point; nearest part A Large's HandR (15.1 cm); the Belly had moved 17.2 cm since the aim was locked
- 43.33 s: SwordThrust at A Large's Chest: passed 0.0 cm from the aim point; nearest part A Large's Chest (3.3 cm); the Chest had moved 0.3 cm since the aim was locked


## B Large

Present 0.0–52.8 s. Steps 51, idle shuffles 3. Feet slid at most 127.9 cm while planted; the weight was at most 29 cm from the support point; hips dropped for reach by up to 4 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 45.5 / 140.8 | 32 / 106 |
| Spine | ? | 52.9 / 163.7 | 30 / 103 |
| Chest | ? | 61.9 / 191.8 | 30 / 104 |
| Head | ? | 78.0 / 243.3 | 44 / 139 |
| UpperArmL | ? | 73.5 / 229.5 | 24 / 79 |
| ForearmL | ? | 64.0 / 201.2 | 39 / 130 |
| HandL | ? | 68.7 / 211.2 | 56 / 180 |
| UpperArmR | ? | 70.2 / 217.7 | 28 / 115 |
| ForearmR | ? | 57.3 / 177.5 | 18 / 109 |
| HandR | ? | 51.7 / 160.0 | 31 / 124 |
| ThighL | ? | 44.7 / 138.7 | 25 / 146 |
| ShinL | ? | 27.0 / 87.4 | 29 / 98 |
| FootL | ? | 12.2 / 42.7 | 23 / 80 |
| ThighR | ? | 41.1 / 126.9 | 29 / 180 |
| ShinR | ? | 28.8 / 105.1 | 39 / 178 |
| FootR | ? | 30.0 / 128.0 | 23 / 171 |

Hits dealt: 9 (2 on the part aimed at, 3 blocked, 0 on someone in the way, 0 on allies), 136 damage. Hits taken: 10, 237 damage. Health at the end 0.


## A Small #2

Present 52.8–111.7 s. Steps 30, idle shuffles 3. Feet slid at most 73.8 cm while planted; the weight was at most 16 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 51.2 / 67.1 | 68 / 100 |
| Spine | ? | 59.2 / 77.9 | 73 / 101 |
| Chest | ? | 68.4 / 90.2 | 83 / 111 |
| Head | ? | 82.9 / 109.8 | 71 / 165 |
| UpperArmL | ? | 73.5 / 97.0 | 43 / 67 |
| ForearmL | ? | 67.1 / 88.7 | 53 / 86 |
| HandL | ? | 73.6 / 96.6 | 84 / 131 |
| UpperArmR | ? | 83.8 / 111.0 | 74 / 131 |
| ForearmR | ? | 75.0 / 99.4 | 54 / 132 |
| HandR | ? | 66.6 / 87.7 | 56 / 180 |
| ThighL | ? | 47.6 / 65.7 | 80 / 121 |
| ShinL | ? | 53.3 / 72.7 | 101 / 162 |
| FootL | ? | 54.3 / 73.8 | 127 / 170 |
| ThighR | ? | 48.8 / 65.0 | 119 / 180 |
| ShinR | ? | 30.3 / 70.1 | 54 / 176 |
| FootR | ? | 26.4 / 58.4 | 44 / 180 |

Hits dealt: 2 (0 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 8 damage. Hits taken: 4, 60 damage. Health at the end 0.


## A Medium #2

Present 52.8–111.7 s. Steps 76, idle shuffles 12. Feet slid at most 0.0 cm while planted; the weight was at most 35 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.5 | 1 / 24 |
| Chest | ? | 0.2 / 7.0 | 1 / 29 |
| Head | ? | 0.2 / 7.9 | 5 / 92 |
| UpperArmL | ? | 0.3 / 9.5 | 1 / 41 |
| ForearmL | ? | 0.7 / 20.1 | 3 / 66 |
| HandL | ? | 1.1 / 20.3 | 11 / 175 |
| UpperArmR | ? | 0.2 / 10.5 | 2 / 168 |
| ForearmR | ? | 0.3 / 19.0 | 1 / 119 |
| HandR | ? | 0.4 / 24.3 | 5 / 156 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 12 (1 on the part aimed at, 7 blocked, 0 on someone in the way, 0 on allies), 80 damage. Hits taken: 9, 70 damage. Health at the end 0.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 81.80 s: SwordThrust at B Large's Chest: passed 0.0 cm from the aim point; nearest part B Large's Chest (3.2 cm); the Chest had moved 0.4 cm since the aim was locked


## A Large #2

Present 52.8–111.7 s. Steps 64, idle shuffles 6. Feet slid at most 0.0 cm while planted; the weight was at most 21 cm from the support point; hips dropped for reach by up to 5 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.0 | 0 / 39 |
| Chest | ? | 0.1 / 16.6 | 0 / 23 |
| Head | ? | 0.1 / 15.3 | 1 / 29 |
| UpperArmL | ? | 0.2 / 14.9 | 1 / 50 |
| ForearmL | ? | 1.0 / 36.0 | 3 / 142 |
| HandL | ? | 1.7 / 44.5 | 8 / 176 |
| UpperArmR | ? | 0.1 / 15.5 | 0 / 46 |
| ForearmR | ? | 0.2 / 14.6 | 1 / 60 |
| HandR | ? | 0.4 / 36.3 | 2 / 115 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 12 (5 on the part aimed at, 2 blocked, 2 on someone in the way, 0 on allies), 243 damage. Hits taken: 5, 64 damage. Health at the end 0.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 56.37 s: ClubSwing at B Large's Head: passed 0.0 cm from the aim point; nearest part B Large's Head (14.2 cm); the Head had moved 30.5 cm since the aim was locked


## B Small #2

Present 52.8–111.7 s. Steps 41, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 23 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 102.4 / 133.9 | 100 / 158 |
| Spine | ? | 101.0 / 131.6 | 88 / 152 |
| Chest | ? | 101.6 / 135.3 | 92 / 140 |
| Head | ? | 105.7 / 145.6 | 101 / 179 |
| UpperArmL | ? | 105.5 / 141.9 | 67 / 177 |
| ForearmL | ? | 106.7 / 140.3 | 59 / 178 |
| HandL | ? | 112.0 / 146.9 | 66 / 176 |
| UpperArmR | ? | 103.0 / 145.1 | 94 / 180 |
| ForearmR | ? | 116.1 / 151.7 | 108 / 180 |
| HandR | ? | 91.4 / 140.7 | 125 / 180 |
| ThighL | ? | 102.7 / 134.3 | 89 / 180 |
| ShinL | ? | 101.8 / 136.0 | 70 / 172 |
| FootL | ? | 103.1 / 138.8 | 92 / 180 |
| ThighR | ? | 104.3 / 136.6 | 108 / 180 |
| ShinR | ? | 115.0 / 151.4 | 137 / 180 |
| FootR | ? | 134.8 / 179.0 | 99 / 179 |

Hits dealt: 2 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 24 damage. Hits taken: 3, 65 damage. Health at the end 0.


## B Medium #2

Present 52.8–111.7 s. Steps 77, idle shuffles 6. Feet slid at most 117.8 cm while planted; the weight was at most 39 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 30.7 / 110.5 | 29 / 111 |
| Spine | ? | 34.1 / 119.1 | 36 / 130 |
| Chest | ? | 40.9 / 130.7 | 33 / 105 |
| Head | ? | 49.1 / 157.3 | 35 / 143 |
| UpperArmL | ? | 48.3 / 155.4 | 23 / 130 |
| ForearmL | ? | 41.2 / 131.1 | 25 / 173 |
| HandL | ? | 44.3 / 138.2 | 50 / 174 |
| UpperArmR | ? | 44.8 / 146.4 | 39 / 174 |
| ForearmR | ? | 36.4 / 115.6 | 46 / 180 |
| HandR | ? | 33.0 / 104.2 | 38 / 173 |
| ThighL | ? | 27.8 / 109.6 | 29 / 171 |
| ShinL | ? | 20.5 / 86.0 | 30 / 179 |
| FootL | ? | 25.9 / 90.9 | 29 / 178 |
| ThighR | ? | 31.6 / 107.8 | 28 / 136 |
| ShinR | ? | 30.5 / 103.7 | 38 / 157 |
| FootR | ? | 36.3 / 117.9 | 40 / 172 |

Hits dealt: 11 (6 on the part aimed at, 3 blocked, 0 on someone in the way, 0 on allies), 135 damage. Hits taken: 15, 112 damage. Health at the end 0.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 69.75 s: SwordThrust at A Large's Belly: passed 0.0 cm from the aim point; nearest part A Large's Spine (6.2 cm); the Belly had moved 0.4 cm since the aim was locked
- 85.10 s: SwordThrust at A Large's Head: passed 0.0 cm from the aim point; nearest part A Large's Head (3.4 cm); the Head had moved 9.1 cm since the aim was locked


## B Large #2

Present 52.8–111.7 s. Steps 34, idle shuffles 3. Feet slid at most 43.3 cm while planted; the weight was at most 20 cm from the support point; hips dropped for reach by up to 4 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 61.4 / 134.9 | 42 / 113 |
| Spine | ? | 73.5 / 161.7 | 51 / 114 |
| Chest | ? | 89.1 / 196.4 | 47 / 121 |
| Head | ? | 112.2 / 248.2 | 64 / 179 |
| UpperArmL | ? | 95.7 / 212.4 | 59 / 180 |
| ForearmL | ? | 76.7 / 171.4 | 61 / 172 |
| HandL | ? | 82.1 / 186.7 | 64 / 179 |
| UpperArmR | ? | 112.5 / 248.1 | 48 / 178 |
| ForearmR | ? | 123.1 / 272.6 | 60 / 180 |
| HandR | ? | 124.4 / 275.9 | 65 / 180 |
| ThighL | ? | 54.5 / 119.9 | 38 / 150 |
| ShinL | ? | 30.7 / 77.3 | 47 / 131 |
| FootL | ? | 17.7 / 48.1 | 66 / 149 |
| ThighR | ? | 59.2 / 130.0 | 66 / 152 |
| ShinR | ? | 38.9 / 86.5 | 32 / 72 |
| FootR | ? | 12.4 / 27.9 | 11 / 88 |

Hits dealt: 7 (1 on the part aimed at, 4 blocked, 0 on someone in the way, 2 on allies), 106 damage. Hits taken: 10, 224 damage. Health at the end 0.


## A Small #3

Present 111.7–147.7 s. Steps 43, idle shuffles 7. Feet slid at most 0.0 cm while planted; the weight was at most 15 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 34.0 / 86.6 | 39 / 110 |
| Spine | Ragdoll | 39.1 / 99.2 | 45 / 114 |
| Chest | Ragdoll | 46.0 / 116.7 | 45 / 115 |
| Head | Ragdoll | 55.7 / 141.8 | 58 / 154 |
| UpperArmL | Ragdoll | 56.9 / 145.2 | 19 / 97 |
| ForearmL | Ragdoll | 53.0 / 135.1 | 32 / 141 |
| HandL | Ragdoll | 56.7 / 150.5 | 43 / 157 |
| UpperArmR | Ragdoll | 48.8 / 123.3 | 65 / 180 |
| ForearmR | Ragdoll | 50.5 / 140.0 | 55 / 178 |
| HandR | Ragdoll | 63.4 / 169.4 | 58 / 179 |
| ThighL | Ragdoll | 32.9 / 83.6 | 48 / 139 |
| ShinL | Ragdoll | 37.4 / 95.5 | 39 / 172 |
| FootL | Ragdoll | 29.1 / 75.1 | 39 / 167 |
| ThighR | Ragdoll | 31.6 / 81.6 | 59 / 172 |
| ShinR | Ragdoll | 35.8 / 91.0 | 45 / 142 |
| FootR | Ragdoll | 22.7 / 59.0 | 36 / 136 |

Hits dealt: 3 (0 on the part aimed at, 2 blocked, 0 on someone in the way, 0 on allies), 10 damage. Hits taken: 4, 73 damage. Killed.


## A Medium #3

Present 111.7–147.7 s. Steps 55, idle shuffles 5. Feet slid at most 166.6 cm while planted; the weight was at most 18 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 9.1 / 153.2 | 12 / 141 |
| Spine | Ragdoll | 9.9 / 162.1 | 13 / 177 |
| Chest | Ragdoll | 10.9 / 181.8 | 13 / 175 |
| Head | Ragdoll | 13.1 / 217.5 | 17 / 179 |
| UpperArmL | Ragdoll | 13.0 / 188.6 | 9 / 178 |
| ForearmL | Ragdoll | 10.6 / 150.9 | 17 / 180 |
| HandL | Ragdoll | 12.4 / 171.1 | 21 / 180 |
| UpperArmR | Ragdoll | 12.0 / 224.6 | 14 / 180 |
| ForearmR | Ragdoll | 11.9 / 207.4 | 14 / 178 |
| HandR | Ragdoll | 11.9 / 188.9 | 16 / 177 |
| ThighL | Ragdoll | 8.9 / 144.2 | 15 / 180 |
| ShinL | Ragdoll | 7.9 / 133.5 | 14 / 177 |
| FootL | Ragdoll | 10.1 / 166.6 | 13 / 178 |
| ThighR | Ragdoll | 9.0 / 157.1 | 8 / 152 |
| ShinR | Ragdoll | 6.4 / 129.8 | 9 / 173 |
| FootR | Ragdoll | 5.3 / 147.9 | 9 / 174 |

Hits dealt: 7 (1 on the part aimed at, 4 blocked, 0 on someone in the way, 0 on allies), 39 damage. Hits taken: 8, 112 damage. Killed.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 135.92 s: SwordCut at B Medium's KneeL: passed 0.1 cm from the aim point; nearest part B Medium's ShinL (0.0 cm); the KneeL had moved 8.8 cm since the aim was locked


## A Large #3

Present 111.7–147.7 s. Steps 49, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 31 cm from the support point; hips dropped for reach by up to 4 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 0 / 19 |
| Chest | ? | 0.1 / 8.2 | 0 / 15 |
| Head | ? | 0.1 / 6.2 | 1 / 27 |
| UpperArmL | ? | 0.2 / 8.4 | 1 / 37 |
| ForearmL | ? | 0.9 / 19.1 | 3 / 108 |
| HandL | ? | 1.8 / 33.9 | 8 / 95 |
| UpperArmR | ? | 0.1 / 5.4 | 0 / 46 |
| ForearmR | ? | 0.3 / 6.6 | 1 / 60 |
| HandR | ? | 0.6 / 36.5 | 3 / 80 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 7 (3 on the part aimed at, 2 blocked, 0 on someone in the way, 0 on allies), 106 damage. Hits taken: 6, 71 damage. Health at the end 0.


## B Small #3

Present 111.7–147.7 s. Steps 51, idle shuffles 5. Feet slid at most 0.0 cm while planted; the weight was at most 26 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 28.1 / 89.9 | 61 / 180 |
| Spine | ? | 29.1 / 86.0 | 59 / 180 |
| Chest | ? | 31.4 / 87.8 | 63 / 180 |
| Head | ? | 33.9 / 89.9 | 58 / 179 |
| UpperArmL | ? | 32.3 / 91.3 | 62 / 180 |
| ForearmL | ? | 31.5 / 101.0 | 71 / 180 |
| HandL | ? | 42.7 / 136.1 | 68 / 178 |
| UpperArmR | ? | 36.1 / 94.4 | 55 / 180 |
| ForearmR | ? | 47.5 / 128.0 | 50 / 154 |
| HandR | ? | 58.1 / 159.6 | 45 / 180 |
| ThighL | ? | 28.7 / 94.8 | 64 / 180 |
| ShinL | ? | 36.0 / 128.3 | 57 / 180 |
| FootL | ? | 38.8 / 154.0 | 54 / 180 |
| ThighR | ? | 27.8 / 90.4 | 59 / 180 |
| ShinR | ? | 32.5 / 115.4 | 52 / 175 |
| FootR | ? | 30.6 / 126.0 | 50 / 177 |

Hits dealt: 2 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 16 damage. Hits taken: 3, 62 damage. Health at the end 0.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 129.00 s: ClubSwing at A Large's Belly: passed 0.1 cm from the aim point; nearest part A Large's Spine (6.7 cm); the Belly had moved 0.5 cm since the aim was locked
- 132.72 s: ClubSwing at A Large's Belly: passed 0.1 cm from the aim point; nearest part A Large's Spine (7.2 cm); the Belly had moved 0.9 cm since the aim was locked


## B Medium #3

Present 111.7–147.7 s. Steps 60, idle shuffles 6. Feet slid at most 0.0 cm while planted; the weight was at most 34 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 0 / 18 |
| Chest | Pinned | 0.1 / 5.4 | 0 / 24 |
| Head | Pinned | 0.2 / 8.2 | 3 / 46 |
| UpperArmL | Pinned | 0.2 / 11.4 | 2 / 150 |
| ForearmL | Pinned | 0.7 / 42.9 | 3 / 177 |
| HandL | Pinned | 1.1 / 21.2 | 12 / 176 |
| UpperArmR | Pinned | 0.1 / 5.8 | 0 / 37 |
| ForearmR | Pinned | 0.2 / 4.6 | 1 / 60 |
| HandR | Pinned | 0.3 / 23.8 | 3 / 64 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 8 (3 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 123 damage. Hits taken: 10, 21 damage. Health at the end 79.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 127.85 s: SwordBackhand at A Small's KneeL: passed 0.1 cm from the aim point; nearest part A Small's ShinL (16.5 cm); the KneeL had moved 17.6 cm since the aim was locked
- 130.43 s: SwordThrust at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (2.3 cm); the Chest had moved 1.7 cm since the aim was locked


## B Large #3

Present 111.7–147.7 s. Steps 42, idle shuffles 4. Feet slid at most 0.0 cm while planted; the weight was at most 20 cm from the support point; hips dropped for reach by up to 4 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 0 / 12 |
| Chest | Pinned | 0.1 / 5.3 | 0 / 9 |
| Head | Pinned | 0.1 / 3.3 | 1 / 19 |
| UpperArmL | Pinned | 0.2 / 6.5 | 1 / 72 |
| ForearmL | Pinned | 0.8 / 15.2 | 2 / 49 |
| HandL | Pinned | 1.4 / 25.9 | 6 / 73 |
| UpperArmR | Pinned | 0.1 / 2.6 | 0 / 46 |
| ForearmR | Pinned | 0.2 / 8.2 | 1 / 61 |
| HandR | Pinned | 0.4 / 36.2 | 2 / 85 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 8 (1 on the part aimed at, 1 blocked, 1 on someone in the way, 0 on allies), 116 damage. Hits taken: 4, 72 damage. Health at the end 148.

