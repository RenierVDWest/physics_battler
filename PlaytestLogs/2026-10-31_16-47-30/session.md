# Playtest 2026-10-31_16-47-30

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 53.8 s of play (3229 physics steps), 2795 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_16-47-30`.

## A Small

Present 0.0–53.8 s. Steps 33, idle shuffles 3. Feet slid at most 73.3 cm while planted; the weight was at most 16 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 42.1 / 57.9 | 99 / 156 |
| Spine | ? | 48.4 / 68.0 | 94 / 147 |
| Chest | ? | 60.3 / 82.5 | 92 / 131 |
| Head | ? | 77.7 / 106.7 | 67 / 117 |
| UpperArmL | ? | 65.3 / 90.6 | 46 / 81 |
| ForearmL | ? | 51.5 / 71.1 | 94 / 132 |
| HandL | ? | 57.4 / 84.0 | 101 / 140 |
| UpperArmR | ? | 81.2 / 110.6 | 104 / 176 |
| ForearmR | ? | 67.6 / 92.0 | 132 / 179 |
| HandR | ? | 60.4 / 82.7 | 104 / 178 |
| ThighL | ? | 35.3 / 48.0 | 113 / 178 |
| ShinL | ? | 36.5 / 59.9 | 88 / 177 |
| FootL | ? | 18.8 / 37.7 | 84 / 177 |
| ThighR | ? | 45.6 / 63.9 | 79 / 180 |
| ShinR | ? | 43.1 / 75.4 | 105 / 179 |
| FootR | ? | 35.5 / 82.0 | 82 / 174 |

Hits dealt: 3 (0 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 19 damage. Hits taken: 4, 61 damage. Health at the end 0.


## A Medium

Present 0.0–53.8 s. Steps 60, idle shuffles 6. Feet slid at most 172.0 cm while planted; the weight was at most 32 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 38.3 / 120.2 | 54 / 164 |
| Spine | ? | 39.6 / 121.8 | 75 / 180 |
| Chest | ? | 47.8 / 125.8 | 77 / 180 |
| Head | ? | 58.0 / 143.1 | 77 / 179 |
| UpperArmL | ? | 54.8 / 140.4 | 62 / 180 |
| ForearmL | ? | 49.5 / 143.1 | 65 / 179 |
| HandL | ? | 57.5 / 172.5 | 77 / 180 |
| UpperArmR | ? | 60.1 / 153.0 | 71 / 179 |
| ForearmR | ? | 57.4 / 147.2 | 67 / 180 |
| HandR | ? | 61.1 / 159.9 | 84 / 180 |
| ThighL | ? | 36.2 / 119.2 | 34 / 178 |
| ShinL | ? | 50.1 / 154.9 | 54 / 180 |
| FootL | ? | 44.3 / 141.0 | 57 / 180 |
| ThighR | ? | 41.4 / 125.9 | 44 / 180 |
| ShinR | ? | 52.5 / 154.4 | 46 / 180 |
| FootR | ? | 58.0 / 173.8 | 57 / 180 |

Hits dealt: 7 (2 on the part aimed at, 3 blocked, 0 on someone in the way, 0 on allies), 81 damage. Hits taken: 9, 118 damage. Health at the end 0.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.1 cm, max 0.1 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.62 s: SwordBackhand at B Medium's Head: passed 0.1 cm from the aim point; nearest part B Medium's Chest (47.2 cm); the Head had moved 59.7 cm since the aim was locked
- 8.18 s: SwordCut at B Small's Head: passed 0.1 cm from the aim point; nearest part B Small's Head (0.0 cm); the Head had moved 5.1 cm since the aim was locked
- 24.17 s: SwordThrust at B Large's Belly: passed 0.0 cm from the aim point; nearest part B Large's Spine (8.1 cm); the Belly had moved 13.4 cm since the aim was locked


## A Large

Present 0.0–53.8 s. Steps 65, idle shuffles 6. Feet slid at most 0.0 cm while planted; the weight was at most 31 cm from the support point; hips dropped for reach by up to 6 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 1 / 57 |
| Chest | ? | 0.3 / 21.1 | 1 / 40 |
| Head | ? | 0.4 / 18.6 | 4 / 76 |
| UpperArmL | ? | 0.5 / 21.1 | 3 / 119 |
| ForearmL | ? | 1.8 / 47.6 | 7 / 180 |
| HandL | ? | 3.2 / 62.1 | 12 / 180 |
| UpperArmR | ? | 0.3 / 17.2 | 2 / 53 |
| ForearmR | ? | 1.5 / 35.3 | 5 / 128 |
| HandR | ? | 2.2 / 46.0 | 10 / 177 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 12 (7 on the part aimed at, 1 blocked, 1 on someone in the way, 1 on allies), 370 damage. Hits taken: 12, 160 damage. Health at the end 0.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 9.7 cm, max 29.1 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 6.63 s: ClubSwing at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (4.5 cm); the Neck had moved 3.5 cm since the aim was locked
- 33.50 s: ClubSwing at B Medium's KneeL: passed 0.0 cm from the aim point; nearest part A Medium's ForearmL (29.7 cm); the KneeL had moved 63.6 cm since the aim was locked
- 38.87 s: ClubSwing at B Large's Neck: passed 0.0 cm from the aim point; nearest part B Large's Head (9.9 cm); the Neck had moved 0.9 cm since the aim was locked
- 44.85 s: ClubSmash at B Medium's Neck: passed 0.0 cm from the aim point; nearest part B Medium's Head (64.4 cm); the Neck had moved 86.7 cm since the aim was locked


## B Small

Present 0.0–53.8 s. Steps 28, idle shuffles 1. Feet slid at most 62.3 cm while planted; the weight was at most 10 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 37.9 / 50.2 | 131 / 180 |
| Spine | ? | 46.4 / 56.7 | 129 / 180 |
| Chest | ? | 58.2 / 73.2 | 106 / 154 |
| Head | ? | 76.4 / 98.4 | 101 / 180 |
| UpperArmL | ? | 78.8 / 99.6 | 73 / 178 |
| ForearmL | ? | 64.1 / 92.3 | 79 / 168 |
| HandL | ? | 80.3 / 111.8 | 102 / 180 |
| UpperArmR | ? | 63.9 / 86.9 | 67 / 180 |
| ForearmR | ? | 60.7 / 77.7 | 67 / 179 |
| HandR | ? | 55.3 / 72.5 | 118 / 179 |
| ThighL | ? | 39.4 / 51.3 | 124 / 180 |
| ShinL | ? | 41.3 / 59.6 | 89 / 126 |
| FootL | ? | 18.0 / 36.1 | 77 / 139 |
| ThighR | ? | 31.6 / 49.0 | 90 / 179 |
| ShinR | ? | 33.9 / 58.6 | 87 / 175 |
| FootR | ? | 21.6 / 66.0 | 95 / 180 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 2, 70 damage. Health at the end 0.


## B Medium

Present 0.0–53.8 s. Steps 63, idle shuffles 9. Feet slid at most 37.0 cm while planted; the weight was at most 27 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 17.0 / 102.7 | 28 / 180 |
| Spine | ? | 19.3 / 117.0 | 27 / 179 |
| Chest | ? | 23.8 / 143.5 | 26 / 162 |
| Head | ? | 29.5 / 176.8 | 34 / 180 |
| UpperArmL | ? | 28.7 / 171.2 | 33 / 180 |
| ForearmL | ? | 24.4 / 141.4 | 26 / 167 |
| HandL | ? | 26.9 / 156.9 | 43 / 180 |
| UpperArmR | ? | 28.2 / 172.2 | 22 / 179 |
| ForearmR | ? | 29.8 / 180.8 | 24 / 179 |
| HandR | ? | 34.0 / 211.6 | 34 / 179 |
| ThighL | ? | 16.1 / 98.2 | 22 / 152 |
| ShinL | ? | 14.8 / 90.8 | 15 / 98 |
| FootL | ? | 6.0 / 37.2 | 16 / 121 |
| ThighR | ? | 16.8 / 103.0 | 19 / 179 |
| ShinR | ? | 22.4 / 137.6 | 21 / 179 |
| FootR | ? | 23.9 / 157.7 | 26 / 179 |

Hits dealt: 11 (4 on the part aimed at, 0 blocked, 1 on someone in the way, 0 on allies), 148 damage. Hits taken: 8, 151 damage. Health at the end 0.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.1 cm, max 0.1 cm. 2 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.60 s: SwordCut at A Small's Belly: passed 0.1 cm from the aim point; nearest part A Small's Spine (2.6 cm); the Belly had moved 14.0 cm since the aim was locked
- 13.92 s: SwordBackhand at A Small's Head: passed 0.1 cm from the aim point; nearest part A Small's Head (0.0 cm); the Head had moved 1.4 cm since the aim was locked
- 25.88 s: SwordThrust at A Large's Belly: passed 0.0 cm from the aim point; nearest part A Large's Spine (5.8 cm); the Belly had moved 0.8 cm since the aim was locked
- 35.60 s: SwordThrust at A Large's Head: passed 0.0 cm from the aim point; nearest part A Large's Chest (28.2 cm); the Head had moved 43.3 cm since the aim was locked


## B Large

Present 0.0–53.8 s. Steps 43, idle shuffles 5. Feet slid at most 186.7 cm while planted; the weight was at most 23 cm from the support point; hips dropped for reach by up to 4 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 33.9 / 166.5 | 27 / 136 |
| Spine | ? | 36.4 / 181.5 | 29 / 155 |
| Chest | ? | 40.0 / 198.3 | 31 / 160 |
| Head | ? | 47.0 / 233.1 | 35 / 180 |
| UpperArmL | ? | 38.6 / 195.9 | 28 / 175 |
| ForearmL | ? | 31.9 / 174.7 | 38 / 180 |
| HandL | ? | 34.1 / 212.0 | 41 / 180 |
| UpperArmR | ? | 52.7 / 266.3 | 28 / 177 |
| ForearmR | ? | 51.0 / 269.2 | 34 / 178 |
| HandR | ? | 47.9 / 287.1 | 41 / 179 |
| ThighL | ? | 30.5 / 149.5 | 30 / 162 |
| ShinL | ? | 29.8 / 155.0 | 31 / 179 |
| FootL | ? | 31.6 / 171.0 | 27 / 180 |
| ThighR | ? | 36.4 / 179.8 | 25 / 170 |
| ShinR | ? | 28.3 / 168.0 | 21 / 180 |
| FootR | ? | 29.7 / 186.7 | 25 / 179 |

Hits dealt: 13 (1 on the part aimed at, 3 blocked, 4 on someone in the way, 0 on allies), 164 damage. Hits taken: 11, 222 damage. Health at the end 0.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 7.25 s: ClubSwing at A Medium's ElbowR: passed 0.0 cm from the aim point; nearest part A Medium's HandR (6.1 cm); the ElbowR had moved 29.8 cm since the aim was locked

