# Playtest 2026-10-31_16-36-53

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 44.8 s of play (2686 physics steps), 1214 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_16-36-53`.

## A Small

Present 0.0–44.8 s. Steps 37, idle shuffles 5. Feet slid at most 108.9 cm while planted; the weight was at most 18 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 44.5 / 73.1 | 91 / 154 |
| Spine | ? | 46.4 / 76.8 | 90 / 158 |
| Chest | ? | 51.4 / 83.6 | 84 / 162 |
| Head | ? | 59.3 / 102.7 | 105 / 180 |
| UpperArmL | ? | 49.8 / 81.0 | 24 / 71 |
| ForearmL | ? | 43.9 / 73.3 | 44 / 115 |
| HandL | ? | 48.6 / 83.1 | 96 / 161 |
| UpperArmR | ? | 66.1 / 114.2 | 77 / 178 |
| ForearmR | ? | 77.2 / 129.5 | 64 / 177 |
| HandR | ? | 83.5 / 138.4 | 110 / 180 |
| ThighL | ? | 41.7 / 68.5 | 82 / 136 |
| ShinL | ? | 44.1 / 72.9 | 85 / 153 |
| FootL | ? | 50.8 / 86.9 | 78 / 175 |
| ThighR | ? | 47.7 / 78.3 | 74 / 179 |
| ShinR | ? | 59.0 / 99.8 | 74 / 179 |
| FootR | ? | 62.9 / 108.9 | 83 / 180 |

Hits dealt: 3 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 26 damage. Hits taken: 4, 66 damage. Health at the end 0.


## A Medium

Present 0.0–44.8 s. Steps 51, idle shuffles 2. Feet slid at most 147.0 cm while planted; the weight was at most 28 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 46.6 / 111.0 | 68 / 180 |
| Spine | Ragdoll | 54.7 / 128.2 | 68 / 180 |
| Chest | Ragdoll | 63.5 / 143.7 | 62 / 180 |
| Head | Ragdoll | 75.2 / 168.2 | 87 / 180 |
| UpperArmL | Ragdoll | 69.0 / 160.9 | 48 / 179 |
| ForearmL | Ragdoll | 67.5 / 171.2 | 48 / 180 |
| HandL | Ragdoll | 77.0 / 180.4 | 68 / 180 |
| UpperArmR | Ragdoll | 74.9 / 171.5 | 58 / 179 |
| ForearmR | Ragdoll | 63.2 / 155.4 | 72 / 180 |
| HandR | Ragdoll | 55.7 / 166.9 | 70 / 180 |
| ThighL | Ragdoll | 41.6 / 110.7 | 45 / 179 |
| ShinL | Ragdoll | 25.6 / 76.3 | 56 / 180 |
| FootL | Ragdoll | 16.8 / 116.8 | 60 / 180 |
| ThighR | Ragdoll | 46.2 / 109.8 | 57 / 179 |
| ShinR | Ragdoll | 33.0 / 104.2 | 55 / 177 |
| FootR | Ragdoll | 22.3 / 147.0 | 53 / 180 |

Hits dealt: 6 (3 on the part aimed at, 1 blocked, 1 on someone in the way, 0 on allies), 96 damage. Hits taken: 5, 124 damage. Killed.


## A Large

Present 0.0–44.8 s. Steps 39, idle shuffles 3. Feet slid at most 83.7 cm while planted; the weight was at most 31 cm from the support point; hips dropped for reach by up to 6 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 30.3 / 125.7 | 33 / 131 |
| Spine | Ragdoll | 34.1 / 140.4 | 30 / 139 |
| Chest | Ragdoll | 40.0 / 155.6 | 27 / 158 |
| Head | Ragdoll | 51.4 / 200.5 | 39 / 177 |
| UpperArmL | Ragdoll | 50.5 / 204.3 | 42 / 180 |
| ForearmL | Ragdoll | 36.5 / 148.4 | 37 / 179 |
| HandL | Ragdoll | 31.4 / 129.3 | 37 / 172 |
| UpperArmR | Ragdoll | 44.4 / 177.2 | 47 / 180 |
| ForearmR | Ragdoll | 52.5 / 202.1 | 36 / 178 |
| HandR | Ragdoll | 63.7 / 239.2 | 41 / 177 |
| ThighL | Ragdoll | 31.1 / 125.9 | 35 / 177 |
| ShinL | Ragdoll | 15.2 / 65.2 | 38 / 179 |
| FootL | Ragdoll | 21.9 / 88.6 | 30 / 178 |
| ThighR | Ragdoll | 28.1 / 121.1 | 18 / 100 |
| ShinR | Ragdoll | 19.9 / 80.6 | 25 / 161 |
| FootR | Ragdoll | 20.6 / 84.0 | 27 / 176 |

Hits dealt: 8 (3 on the part aimed at, 1 blocked, 2 on someone in the way, 0 on allies), 176 damage. Hits taken: 10, 222 damage. Killed.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 7.10 s: ClubSwing at B Small's Chest: passed 0.0 cm from the aim point; nearest part B Small's Chest (5.2 cm); the Chest had moved 3.1 cm since the aim was locked
- 9.77 s: ClubSwing at B Small's Head: passed 0.0 cm from the aim point; nearest part B Small's Chest (17.8 cm); the Head had moved 40.7 cm since the aim was locked


## B Small

Present 0.0–44.8 s. Steps 29, idle shuffles 2. Feet slid at most 93.6 cm while planted; the weight was at most 13 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 72.9 / 95.8 | 137 / 180 |
| Spine | Ragdoll | 81.9 / 107.1 | 133 / 180 |
| Chest | Ragdoll | 92.3 / 120.7 | 135 / 180 |
| Head | Ragdoll | 106.7 / 141.6 | 139 / 179 |
| UpperArmL | Ragdoll | 86.2 / 116.4 | 125 / 179 |
| ForearmL | Ragdoll | 92.1 / 122.2 | 114 / 180 |
| HandL | Ragdoll | 101.6 / 148.5 | 72 / 180 |
| UpperArmR | Ragdoll | 119.2 / 155.7 | 75 / 137 |
| ForearmR | Ragdoll | 126.4 / 165.0 | 131 / 178 |
| HandR | Ragdoll | 130.8 / 170.4 | 107 / 179 |
| ThighL | Ragdoll | 63.6 / 86.0 | 66 / 159 |
| ShinL | Ragdoll | 61.3 / 92.9 | 120 / 180 |
| FootL | Ragdoll | 39.4 / 96.3 | 137 / 179 |
| ThighR | Ragdoll | 75.6 / 99.6 | 115 / 179 |
| ShinR | Ragdoll | 50.4 / 66.7 | 120 / 180 |
| FootR | Ragdoll | 27.4 / 54.2 | 127 / 180 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 3, 69 damage. Killed.


## B Medium

Present 0.0–44.8 s. Steps 48, idle shuffles 10. Feet slid at most 0.0 cm while planted; the weight was at most 28 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 16.6 | 2 / 127 |
| Chest | Pinned | 0.6 / 17.0 | 3 / 59 |
| Head | Pinned | 0.5 / 23.1 | 10 / 109 |
| UpperArmL | Pinned | 1.1 / 33.6 | 36 / 180 |
| ForearmL | Pinned | 3.4 / 51.4 | 13 / 173 |
| HandL | Pinned | 2.9 / 67.7 | 42 / 180 |
| UpperArmR | Pinned | 0.7 / 20.4 | 3 / 66 |
| ForearmR | Pinned | 1.0 / 28.3 | 4 / 120 |
| HandR | Pinned | 1.2 / 39.7 | 19 / 176 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 8 (3 on the part aimed at, 0 blocked, 1 on someone in the way, 0 on allies), 180 damage. Hits taken: 4, 26 damage. Health at the end 74.

Blows: 6. The posed striking part passed within 3 cm of the aim point in 6; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.1 cm, max 0.1 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.93 s: SwordCut at A Small's ElbowL: passed 0.1 cm from the aim point; nearest part A Small's Spine (0.0 cm); the ElbowL had moved 17.9 cm since the aim was locked
- 7.60 s: SwordThrust at A Small's Belly: passed 0.0 cm from the aim point; nearest part A Small's HandR (6.1 cm); the Belly had moved 2.1 cm since the aim was locked
- 10.07 s: SwordCut at A Small's Belly: passed 0.1 cm from the aim point; nearest part A Small's Spine (4.7 cm); the Belly had moved 4.7 cm since the aim was locked
- 13.45 s: SwordCut at A Small's ElbowL: passed 0.1 cm from the aim point; nearest part A Small's Spine (1.9 cm); the ElbowL had moved 13.1 cm since the aim was locked
- 16.95 s: SwordBackhand at A Small's Neck: passed 0.1 cm from the aim point; nearest part A Small's Chest (3.3 cm); the Neck had moved 0.6 cm since the aim was locked
- 19.55 s: SwordCut at A Large's Neck: passed 0.1 cm from the aim point; nearest part A Large's Chest (34.5 cm); the Neck had moved 46.3 cm since the aim was locked


## B Large

Present 0.0–44.8 s. Steps 38, idle shuffles 6. Feet slid at most 0.0 cm while planted; the weight was at most 22 cm from the support point; hips dropped for reach by up to 4 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 0 / 28 |
| Chest | ? | 0.2 / 10.8 | 0 / 29 |
| Head | ? | 0.2 / 12.1 | 2 / 79 |
| UpperArmL | ? | 0.2 / 8.7 | 2 / 124 |
| ForearmL | ? | 1.0 / 41.2 | 3 / 81 |
| HandL | ? | 1.7 / 25.9 | 7 / 109 |
| UpperArmR | ? | 0.2 / 10.9 | 2 / 54 |
| ForearmR | ? | 1.7 / 33.0 | 9 / 109 |
| HandR | ? | 5.3 / 38.2 | 18 / 171 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 10 (6 on the part aimed at, 1 blocked, 2 on someone in the way, 0 on allies), 232 damage. Hits taken: 9, 203 damage. Health at the end 0.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 7.22 s: ClubSwing at A Medium's Neck: passed 0.0 cm from the aim point; nearest part A Medium's Chest (40.9 cm); the Neck had moved 53.9 cm since the aim was locked

