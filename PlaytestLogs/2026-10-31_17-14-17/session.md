# Playtest 2026-10-31_17-14-17

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 99.6 s of play (5978 physics steps), 2522 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_17-14-17`.

## A Small

Present 0.0–68.1 s. Steps 59, idle shuffles 7. Feet slid at most 74.3 cm while planted; the weight was at most 20 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 39.6 / 63.4 | 64 / 138 |
| Spine | ? | 47.6 / 76.7 | 69 / 137 |
| Chest | ? | 58.6 / 93.0 | 67 / 120 |
| Head | ? | 74.5 / 118.5 | 78 / 146 |
| UpperArmL | ? | 74.2 / 118.5 | 85 / 171 |
| ForearmL | ? | 78.2 / 125.7 | 62 / 170 |
| HandL | ? | 94.0 / 150.5 | 92 / 179 |
| UpperArmR | ? | 65.4 / 104.1 | 60 / 180 |
| ForearmR | ? | 51.8 / 85.4 | 55 / 148 |
| HandR | ? | 51.1 / 87.2 | 83 / 179 |
| ThighL | ? | 38.0 / 64.5 | 57 / 114 |
| ShinL | ? | 27.7 / 59.8 | 44 / 176 |
| FootL | ? | 17.5 / 38.2 | 49 / 153 |
| ThighR | ? | 35.0 / 59.5 | 71 / 179 |
| ShinR | ? | 29.2 / 66.2 | 86 / 178 |
| FootR | ? | 22.2 / 80.1 | 53 / 180 |

Hits dealt: 7 (0 on the part aimed at, 3 blocked, 0 on someone in the way, 0 on allies), 34 damage. Hits taken: 5, 70 damage. Health at the end 0.


## A Medium

Present 0.0–68.1 s. Steps 74, idle shuffles 6. Feet slid at most 189.2 cm while planted; the weight was at most 22 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 82.8 / 193.7 | 75 / 180 |
| Spine | ? | 79.9 / 186.7 | 79 / 180 |
| Chest | ? | 77.6 / 180.1 | 76 / 180 |
| Head | ? | 81.9 / 187.9 | 71 / 180 |
| UpperArmL | ? | 80.3 / 180.9 | 78 / 180 |
| ForearmL | ? | 61.3 / 151.8 | 71 / 180 |
| HandL | ? | 75.6 / 182.5 | 74 / 179 |
| UpperArmR | ? | 82.1 / 195.8 | 51 / 178 |
| ForearmR | ? | 81.8 / 191.0 | 57 / 180 |
| HandR | ? | 79.4 / 180.8 | 81 / 180 |
| ThighL | ? | 79.3 / 185.3 | 68 / 179 |
| ShinL | ? | 72.7 / 166.9 | 78 / 180 |
| FootL | ? | 82.1 / 189.2 | 65 / 180 |
| ThighR | ? | 89.1 / 209.1 | 59 / 146 |
| ShinR | ? | 79.4 / 188.5 | 63 / 164 |
| FootR | ? | 68.6 / 163.8 | 63 / 156 |

Hits dealt: 12 (2 on the part aimed at, 5 blocked, 0 on someone in the way, 0 on allies), 126 damage. Hits taken: 7, 118 damage. Health at the end 0.


## A Large

Present 0.0–68.1 s. Steps 55, idle shuffles 5. Feet slid at most 153.9 cm while planted; the weight was at most 44 cm from the support point; hips dropped for reach by up to 6 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 45.0 / 151.4 | 39 / 140 |
| Spine | ? | 50.9 / 173.0 | 42 / 152 |
| Chest | ? | 56.5 / 193.3 | 47 / 165 |
| Head | ? | 65.7 / 232.9 | 48 / 180 |
| UpperArmL | ? | 76.9 / 267.2 | 50 / 180 |
| ForearmL | ? | 87.0 / 299.4 | 58 / 180 |
| HandL | ? | 106.2 / 356.6 | 64 / 180 |
| UpperArmR | ? | 51.1 / 183.2 | 28 / 176 |
| ForearmR | ? | 50.2 / 170.5 | 23 / 178 |
| HandR | ? | 48.7 / 196.2 | 51 / 180 |
| ThighL | ? | 48.5 / 163.9 | 22 / 132 |
| ShinL | ? | 36.5 / 123.8 | 30 / 132 |
| FootL | ? | 20.0 / 72.4 | 41 / 172 |
| ThighR | ? | 38.0 / 138.7 | 53 / 180 |
| ShinR | ? | 39.8 / 136.6 | 52 / 180 |
| FootR | ? | 43.8 / 153.9 | 52 / 180 |

Hits dealt: 8 (3 on the part aimed at, 2 blocked, 0 on someone in the way, 0 on allies), 133 damage. Hits taken: 12, 234 damage. Health at the end 0.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 9.55 s: ClubSwing at B Small's Head: passed 0.0 cm from the aim point; nearest part B Large's HandL (50.9 cm); the Head had moved 79.3 cm since the aim was locked


## B Small

Present 0.0–68.1 s. Steps 32, idle shuffles 1. Feet slid at most 157.1 cm while planted; the weight was at most 22 cm from the support point; hips dropped for reach by up to 7 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 69.8 / 119.2 | 104 / 178 |
| Spine | ? | 75.8 / 116.5 | 115 / 178 |
| Chest | ? | 82.8 / 117.5 | 129 / 180 |
| Head | ? | 97.1 / 125.1 | 106 / 179 |
| UpperArmL | ? | 80.8 / 108.5 | 85 / 172 |
| ForearmL | ? | 62.7 / 85.5 | 108 / 180 |
| HandL | ? | 69.1 / 111.4 | 111 / 177 |
| UpperArmR | ? | 107.3 / 143.1 | 67 / 180 |
| ForearmR | ? | 101.3 / 138.9 | 117 / 179 |
| HandR | ? | 87.5 / 121.6 | 85 / 180 |
| ThighL | ? | 64.2 / 112.0 | 108 / 179 |
| ShinL | ? | 70.5 / 119.7 | 103 / 180 |
| FootL | ? | 83.0 / 134.3 | 124 / 180 |
| ThighR | ? | 72.6 / 131.1 | 105 / 180 |
| ShinR | ? | 75.3 / 148.4 | 98 / 179 |
| FootR | ? | 62.3 / 157.1 | 87 / 179 |

Hits dealt: 1 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 12 damage. Hits taken: 2, 77 damage. Health at the end 0.


## B Medium

Present 0.0–68.1 s. Steps 90, idle shuffles 14. Feet slid at most 0.0 cm while planted; the weight was at most 28 cm from the support point; hips dropped for reach by up to 9 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 1 / 55 |
| Chest | ? | 0.2 / 16.5 | 1 / 43 |
| Head | ? | 0.2 / 13.7 | 3 / 52 |
| UpperArmL | ? | 0.3 / 20.6 | 2 / 143 |
| ForearmL | ? | 0.7 / 30.0 | 3 / 179 |
| HandL | ? | 1.1 / 33.0 | 11 / 175 |
| UpperArmR | ? | 0.1 / 10.7 | 0 / 41 |
| ForearmR | ? | 0.2 / 8.7 | 1 / 62 |
| HandR | ? | 0.4 / 24.0 | 4 / 88 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 12 (5 on the part aimed at, 3 blocked, 0 on someone in the way, 0 on allies), 179 damage. Hits taken: 18, 98 damage. Health at the end 0.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 14.83 s: SwordThrust at A Small's Belly: passed 0.0 cm from the aim point; nearest part A Small's HandR (12.5 cm); the Belly had moved 18.8 cm since the aim was locked
- 43.18 s: SwordThrust at A Large's Belly: passed 0.0 cm from the aim point; nearest part A Large's Spine (5.7 cm); the Belly had moved 0.6 cm since the aim was locked


## B Large

Present 0.0–68.1 s. Steps 68, idle shuffles 9. Feet slid at most 0.0 cm while planted; the weight was at most 42 cm from the support point; hips dropped for reach by up to 4 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 0 / 11 |
| Chest | ? | 0.1 / 4.8 | 0 / 8 |
| Head | ? | 0.1 / 3.8 | 2 / 48 |
| UpperArmL | ? | 0.2 / 8.4 | 1 / 55 |
| ForearmL | ? | 0.7 / 14.0 | 2 / 49 |
| HandL | ? | 1.4 / 20.7 | 5 / 69 |
| UpperArmR | ? | 0.0 / 3.4 | 0 / 45 |
| ForearmR | ? | 0.2 / 6.3 | 1 / 60 |
| HandR | ? | 0.4 / 36.7 | 2 / 78 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 14 (8 on the part aimed at, 1 blocked, 0 on someone in the way, 3 on allies), 281 damage. Hits taken: 10, 168 damage. Health at the end 0.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 41.13 s: ClubSmash at A Large's Head: passed 0.0 cm from the aim point; nearest part A Large's Chest (63.0 cm); the Head had moved 89.0 cm since the aim was locked


## A Small #2

Present 68.1–99.6 s. Steps 67, idle shuffles 4. Feet slid at most 153.3 cm while planted; the weight was at most 32 cm from the support point; hips dropped for reach by up to 4 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 30.9 / 128.4 | 42 / 179 |
| Spine | Ragdoll | 33.2 / 132.3 | 45 / 180 |
| Chest | Ragdoll | 36.8 / 142.6 | 41 / 180 |
| Head | Ragdoll | 42.0 / 163.0 | 43 / 180 |
| UpperArmL | Ragdoll | 44.9 / 176.0 | 39 / 178 |
| ForearmL | Ragdoll | 45.4 / 179.3 | 43 / 179 |
| HandL | Ragdoll | 52.6 / 202.8 | 42 / 180 |
| UpperArmR | Ragdoll | 36.3 / 144.8 | 32 / 157 |
| ForearmR | Ragdoll | 27.8 / 112.3 | 25 / 162 |
| HandR | Ragdoll | 26.4 / 116.8 | 41 / 179 |
| ThighL | Ragdoll | 32.8 / 139.2 | 35 / 180 |
| ShinL | Ragdoll | 36.2 / 163.7 | 38 / 179 |
| FootL | Ragdoll | 34.2 / 153.3 | 38 / 180 |
| ThighR | Ragdoll | 27.5 / 118.0 | 35 / 180 |
| ShinR | Ragdoll | 22.0 / 111.8 | 37 / 180 |
| FootR | Ragdoll | 22.4 / 104.4 | 36 / 180 |

Hits dealt: 5 (0 on the part aimed at, 0 blocked, 1 on someone in the way, 0 on allies), 55 damage. Hits taken: 5, 73 damage. Killed.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 2.1 cm, max 2.1 cm (posed); physical weapon's closest pass mean 2.1 cm, max 2.1 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 85.95 s: ClubSwing at B Medium's Head: passed 2.1 cm from the aim point; nearest part B Medium's HandR (47.8 cm); the Head had moved 52.5 cm since the aim was locked


## A Medium #2

Present 68.1–99.6 s. Steps 56, idle shuffles 5. Feet slid at most 0.0 cm while planted; the weight was at most 26 cm from the support point; hips dropped for reach by up to 6 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 1 / 21 |
| Chest | ? | 0.3 / 6.2 | 1 / 28 |
| Head | ? | 0.4 / 10.9 | 8 / 81 |
| UpperArmL | ? | 0.5 / 8.3 | 2 / 40 |
| ForearmL | ? | 0.9 / 20.2 | 4 / 66 |
| HandL | ? | 1.4 / 20.3 | 16 / 175 |
| UpperArmR | ? | 0.3 / 13.3 | 2 / 122 |
| ForearmR | ? | 0.5 / 16.2 | 2 / 64 |
| HandR | ? | 0.7 / 24.3 | 7 / 102 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 8 (2 on the part aimed at, 1 blocked, 1 on someone in the way, 0 on allies), 96 damage. Hits taken: 7, 0 damage. Health at the end 0.


## A Large #2

Present 68.1–99.6 s. Steps 46, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 39 cm from the support point; hips dropped for reach by up to 5 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 0 / 20 |
| Chest | ? | 0.2 / 8.7 | 0 / 16 |
| Head | ? | 0.1 / 5.7 | 2 / 29 |
| UpperArmL | ? | 0.2 / 9.4 | 2 / 28 |
| ForearmL | ? | 1.4 / 18.7 | 5 / 79 |
| HandL | ? | 3.5 / 43.8 | 17 / 166 |
| UpperArmR | ? | 0.0 / 1.2 | 0 / 46 |
| ForearmR | ? | 0.2 / 6.2 | 1 / 60 |
| HandR | ? | 0.4 / 36.3 | 2 / 79 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 9 (3 on the part aimed at, 0 blocked, 2 on someone in the way, 0 on allies), 248 damage. Hits taken: 2, 15 damage. Health at the end 0.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 71.53 s: ClubSwing at B Large's Neck: passed 0.0 cm from the aim point; nearest part B Large's Chest (6.6 cm); the Neck had moved 28.2 cm since the aim was locked


## B Small #2

Present 68.1–99.6 s. Steps 35, idle shuffles 1. Feet slid at most 82.6 cm while planted; the weight was at most 26 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 40.9 / 71.8 | 57 / 116 |
| Spine | ? | 43.0 / 75.2 | 60 / 131 |
| Chest | ? | 45.7 / 77.1 | 62 / 136 |
| Head | ? | 51.2 / 90.1 | 80 / 179 |
| UpperArmL | ? | 55.5 / 93.0 | 95 / 180 |
| ForearmL | ? | 47.3 / 89.2 | 97 / 180 |
| HandL | ? | 63.3 / 118.7 | 98 / 180 |
| UpperArmR | ? | 44.7 / 84.3 | 83 / 180 |
| ForearmR | ? | 45.9 / 88.0 | 79 / 178 |
| HandR | ? | 42.5 / 100.6 | 68 / 180 |
| ThighL | ? | 44.2 / 77.4 | 65 / 157 |
| ShinL | ? | 37.0 / 73.2 | 63 / 180 |
| FootL | ? | 34.7 / 82.6 | 60 / 179 |
| ThighR | ? | 37.2 / 66.3 | 60 / 162 |
| ShinR | ? | 33.2 / 73.3 | 67 / 178 |
| FootR | ? | 30.8 / 79.2 | 70 / 179 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 3, 69 damage. Health at the end 0.


## B Medium #2

Present 68.1–99.6 s. Steps 53, idle shuffles 5. Feet slid at most 19.9 cm while planted; the weight was at most 19 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 7.1 / 88.7 | 10 / 139 |
| Spine | ? | 8.6 / 106.6 | 9 / 118 |
| Chest | ? | 10.7 / 127.2 | 9 / 101 |
| Head | ? | 13.3 / 162.3 | 22 / 174 |
| UpperArmL | ? | 12.0 / 145.8 | 19 / 175 |
| ForearmL | ? | 11.5 / 119.2 | 13 / 176 |
| HandL | ? | 12.5 / 144.2 | 30 / 179 |
| UpperArmR | ? | 13.1 / 156.3 | 14 / 177 |
| ForearmR | ? | 11.9 / 140.0 | 16 / 172 |
| HandR | ? | 14.7 / 175.2 | 20 / 174 |
| ThighL | ? | 6.1 / 86.9 | 12 / 167 |
| ShinL | ? | 4.5 / 70.9 | 8 / 158 |
| FootL | ? | 0.8 / 17.8 | 8 / 158 |
| ThighR | ? | 6.9 / 89.7 | 13 / 156 |
| ShinR | ? | 4.5 / 68.0 | 7 / 104 |
| FootR | ? | 0.6 / 19.9 | 6 / 139 |

Hits dealt: 8 (0 on the part aimed at, 5 blocked, 2 on someone in the way, 0 on allies), 30 damage. Hits taken: 7, 108 damage. Health at the end 0.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 91.77 s: SwordCut at A Medium's Neck: passed 0.1 cm from the aim point; nearest part A Medium's Head (7.9 cm); the Neck had moved 0.9 cm since the aim was locked


## B Large #2

Present 68.1–99.6 s. Steps 29, idle shuffles 2. Feet slid at most 209.1 cm while planted; the weight was at most 16 cm from the support point; hips dropped for reach by up to 4 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 31.0 / 199.4 | 21 / 179 |
| Spine | Ragdoll | 33.0 / 211.4 | 24 / 180 |
| Chest | Ragdoll | 36.8 / 245.8 | 25 / 178 |
| Head | Ragdoll | 45.0 / 303.6 | 29 / 180 |
| UpperArmL | Ragdoll | 46.4 / 305.7 | 25 / 180 |
| ForearmL | Ragdoll | 45.7 / 298.9 | 31 / 179 |
| HandL | Ragdoll | 53.8 / 337.3 | 34 / 180 |
| UpperArmR | Ragdoll | 39.2 / 275.6 | 22 / 180 |
| ForearmR | Ragdoll | 33.2 / 243.7 | 29 / 180 |
| HandR | Ragdoll | 37.6 / 273.0 | 28 / 180 |
| ThighL | Ragdoll | 33.3 / 220.1 | 19 / 180 |
| ShinL | Ragdoll | 31.1 / 205.6 | 19 / 178 |
| FootL | Ragdoll | 29.7 / 209.1 | 20 / 179 |
| ThighR | Ragdoll | 27.7 / 172.7 | 22 / 180 |
| ShinR | Ragdoll | 28.8 / 207.2 | 24 / 180 |
| FootR | Ragdoll | 25.0 / 164.3 | 23 / 180 |

Hits dealt: 6 (0 on the part aimed at, 2 blocked, 1 on someone in the way, 0 on allies), 58 damage. Hits taken: 12, 221 damage. Killed.

