# Playtest 2026-10-31_16-35-04

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 81.8 s of play (4907 physics steps), 3117 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_16-35-04`.

## A Small

Present 0.0–24.9 s. Steps 34, idle shuffles 3. Feet slid at most 41.5 cm while planted; the weight was at most 17 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 20.7 / 50.9 | 37 / 123 |
| Spine | ? | 26.0 / 63.7 | 38 / 102 |
| Chest | ? | 32.4 / 76.3 | 42 / 133 |
| Head | ? | 42.7 / 101.3 | 49 / 171 |
| UpperArmL | ? | 39.4 / 92.8 | 41 / 179 |
| ForearmL | ? | 36.8 / 88.6 | 59 / 175 |
| HandL | ? | 44.5 / 122.8 | 67 / 179 |
| UpperArmR | ? | 40.2 / 97.1 | 53 / 179 |
| ForearmR | ? | 28.0 / 66.1 | 23 / 180 |
| HandR | ? | 26.6 / 76.3 | 33 / 179 |
| ThighL | ? | 18.0 / 45.3 | 49 / 169 |
| ShinL | ? | 15.9 / 38.5 | 49 / 171 |
| FootL | ? | 2.0 / 24.8 | 50 / 175 |
| ThighR | ? | 20.2 / 49.0 | 43 / 163 |
| ShinR | ? | 10.4 / 26.6 | 46 / 180 |
| FootR | ? | 15.0 / 41.6 | 41 / 178 |

Hits dealt: 3 (0 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 18 damage. Hits taken: 3, 80 damage. Health at the end 0.


## A Medium

Present 0.0–24.9 s. Steps 50, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 30 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 1.4 / 23.7 | 48 / 177 |
| Chest | ? | 12.2 / 50.5 | 30 / 180 |
| Head | ? | 15.7 / 70.1 | 35 / 178 |
| UpperArmL | ? | 17.5 / 83.6 | 57 / 179 |
| ForearmL | ? | 13.3 / 84.8 | 47 / 178 |
| HandL | ? | 9.9 / 149.5 | 75 / 180 |
| UpperArmR | ? | 13.4 / 66.0 | 42 / 180 |
| ForearmR | ? | 8.3 / 50.1 | 29 / 178 |
| HandR | ? | 7.0 / 51.0 | 36 / 179 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 7 (1 on the part aimed at, 0 blocked, 1 on someone in the way, 1 on allies), 103 damage. Hits taken: 8, 35 damage. Health at the end 0.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.1 cm, max 0.1 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 11.50 s: SwordBackhand at B Small's KneeR: passed 0.1 cm from the aim point; nearest part B Small's ShinR (8.1 cm); the KneeR had moved 8.1 cm since the aim was locked
- 22.83 s: SwordCut at B Small's Belly: passed 0.1 cm from the aim point; nearest part B Small's Spine (4.6 cm); the Belly had moved 4.0 cm since the aim was locked


## A Large

Present 0.0–24.9 s. Steps 39, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 33 cm from the support point; hips dropped for reach by up to 6 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 9.7 | 4 / 129 |
| Chest | ? | 0.5 / 12.2 | 1 / 27 |
| Head | ? | 0.5 / 6.5 | 8 / 122 |
| UpperArmL | ? | 0.5 / 9.1 | 3 / 138 |
| ForearmL | ? | 1.7 / 39.1 | 5 / 111 |
| HandL | ? | 2.7 / 44.7 | 11 / 155 |
| UpperArmR | ? | 0.4 / 5.9 | 2 / 46 |
| ForearmR | ? | 1.3 / 30.6 | 4 / 95 |
| HandR | ? | 2.1 / 36.9 | 10 / 179 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 7 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 109 damage. Hits taken: 8, 132 damage. Health at the end 0.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 6.48 s: ClubSmash at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (1.7 cm); the Neck had moved 10.5 cm since the aim was locked
- 9.92 s: ClubSwing at B Small's KneeL: passed 0.0 cm from the aim point; nearest part B Small's ThighL (7.7 cm); the KneeL had moved 2.8 cm since the aim was locked
- 12.55 s: ClubSwing at B Large's Neck: passed 0.0 cm from the aim point; nearest part B Large's Chest (11.9 cm); the Neck had moved 9.2 cm since the aim was locked


## B Small

Present 0.0–24.9 s. Steps 51, idle shuffles 7. Feet slid at most 0.0 cm while planted; the weight was at most 14 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.1 / 19.1 | 2 / 179 |
| Chest | ? | 0.4 / 39.6 | 2 / 179 |
| Head | ? | 0.4 / 52.5 | 5 / 125 |
| UpperArmL | ? | 0.5 / 42.5 | 4 / 180 |
| ForearmL | ? | 1.1 / 52.7 | 6 / 175 |
| HandL | ? | 1.4 / 58.7 | 11 / 179 |
| UpperArmR | ? | 0.4 / 54.5 | 3 / 176 |
| ForearmR | ? | 1.0 / 54.2 | 5 / 176 |
| HandR | ? | 1.4 / 56.7 | 11 / 173 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 5 (1 on the part aimed at, 2 blocked, 1 on someone in the way, 0 on allies), 28 damage. Hits taken: 4, 51 damage. Health at the end 0.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.1 cm, max 0.1 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 7.25 s: ClubSwing at A Large's KneeL: passed 0.1 cm from the aim point; nearest part A Large's ShinL (2.7 cm); the KneeL had moved 4.2 cm since the aim was locked
- 19.18 s: ClubSwing at A Large's Belly: passed 0.1 cm from the aim point; nearest part A Large's Chest (19.7 cm); the Belly had moved 27.7 cm since the aim was locked


## B Medium

Present 0.0–24.9 s. Steps 39, idle shuffles 4. Feet slid at most 0.0 cm while planted; the weight was at most 25 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.5 | 1 / 36 |
| Chest | ? | 0.3 / 10.6 | 2 / 61 |
| Head | ? | 0.6 / 27.7 | 11 / 174 |
| UpperArmL | ? | 0.9 / 27.7 | 10 / 150 |
| ForearmL | ? | 1.8 / 58.0 | 7 / 177 |
| HandL | ? | 2.0 / 29.9 | 22 / 179 |
| UpperArmR | ? | 0.6 / 28.4 | 2 / 87 |
| ForearmR | ? | 1.0 / 42.6 | 4 / 141 |
| HandR | ? | 1.3 / 42.7 | 11 / 176 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 6 (2 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 122 damage. Hits taken: 4, 48 damage. Health at the end 0.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.1 cm, max 0.1 cm. 3 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.60 s: SwordCut at A Small's Head: passed 0.1 cm from the aim point; nearest part A Small's Head (4.0 cm); the Head had moved 19.3 cm since the aim was locked
- 7.27 s: SwordThrust at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (4.2 cm); the Chest had moved 3.6 cm since the aim was locked
- 10.32 s: SwordBackhand at A Small's KneeL: passed 0.1 cm from the aim point; nearest part A Small's ShinL (4.6 cm); the KneeL had moved 1.8 cm since the aim was locked
- 13.93 s: SwordCut at A Small's Neck: passed 0.1 cm from the aim point; nearest part A Small's Chest (4.0 cm); the Neck had moved 1.9 cm since the aim was locked


## B Large

Present 0.0–24.9 s. Steps 30, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 23 cm from the support point; hips dropped for reach by up to 4 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 1 / 33 |
| Chest | ? | 0.3 / 14.1 | 1 / 25 |
| Head | ? | 0.3 / 9.5 | 2 / 35 |
| UpperArmL | ? | 0.3 / 8.1 | 2 / 88 |
| ForearmL | ? | 1.1 / 10.3 | 3 / 49 |
| HandL | ? | 2.2 / 27.8 | 9 / 117 |
| UpperArmR | ? | 0.3 / 11.9 | 2 / 46 |
| ForearmR | ? | 1.6 / 33.0 | 6 / 92 |
| HandR | ? | 3.1 / 36.7 | 14 / 176 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 7 (1 on the part aimed at, 3 blocked, 1 on someone in the way, 0 on allies), 88 damage. Hits taken: 8, 122 damage. Health at the end 0.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 6.72 s: ClubSwing at A Medium's Belly: passed 0.0 cm from the aim point; nearest part A Medium's Spine (9.8 cm); the Belly had moved 7.3 cm since the aim was locked


## A Small #2

Present 24.9–81.8 s. Steps 31, idle shuffles 4. Feet slid at most 0.0 cm while planted; the weight was at most 12 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 53.4 / 76.0 | 100 / 179 |
| Spine | Ragdoll | 59.6 / 86.9 | 83 / 137 |
| Chest | Ragdoll | 70.2 / 104.6 | 73 / 122 |
| Head | Ragdoll | 87.5 / 131.3 | 98 / 167 |
| UpperArmL | Ragdoll | 77.9 / 123.4 | 46 / 156 |
| ForearmL | Ragdoll | 80.7 / 128.3 | 47 / 132 |
| HandL | Ragdoll | 92.4 / 148.1 | 59 / 140 |
| UpperArmR | Ragdoll | 86.6 / 126.8 | 78 / 138 |
| ForearmR | Ragdoll | 69.9 / 101.7 | 62 / 107 |
| HandR | Ragdoll | 69.4 / 99.5 | 79 / 178 |
| ThighL | Ragdoll | 50.4 / 73.3 | 61 / 178 |
| ShinL | Ragdoll | 59.2 / 105.1 | 80 / 179 |
| FootL | Ragdoll | 51.9 / 117.4 | 114 / 180 |
| ThighR | Ragdoll | 52.6 / 76.4 | 123 / 180 |
| ShinR | Ragdoll | 53.1 / 87.9 | 102 / 179 |
| FootR | Ragdoll | 37.9 / 74.3 | 85 / 160 |

Hits dealt: 2 (0 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 8 damage. Hits taken: 4, 76 damage. Killed.


## A Medium #2

Present 24.9–81.8 s. Steps 79, idle shuffles 12. Feet slid at most 0.0 cm while planted; the weight was at most 29 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 1 / 47 |
| Chest | ? | 0.3 / 10.4 | 2 / 86 |
| Head | ? | 0.6 / 25.8 | 9 / 130 |
| UpperArmL | ? | 0.6 / 17.7 | 3 / 140 |
| ForearmL | ? | 1.0 / 24.8 | 4 / 106 |
| HandL | ? | 1.5 / 30.2 | 17 / 178 |
| UpperArmR | ? | 0.6 / 42.9 | 5 / 179 |
| ForearmR | ? | 1.4 / 57.6 | 6 / 166 |
| HandR | ? | 1.8 / 37.3 | 15 / 180 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 11 (3 on the part aimed at, 2 blocked, 0 on someone in the way, 0 on allies), 170 damage. Hits taken: 8, 0 damage. Health at the end 0.

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.1 cm, max 0.1 cm. 3 reached the part they were aimed at (nearest part within 5 cm at contact).

- 37.98 s: SwordBackhand at B Small's KneeR: passed 0.1 cm from the aim point; nearest part B Small's ShinR (2.0 cm); the KneeR had moved 4.5 cm since the aim was locked
- 41.38 s: SwordBackhand at B Small's Belly: passed 0.1 cm from the aim point; nearest part B Small's Chest (0.0 cm); the Belly had moved 13.0 cm since the aim was locked
- 46.22 s: SwordThrust at B Medium's Belly: passed 0.0 cm from the aim point; nearest part B Medium's Spine (8.8 cm); the Belly had moved 5.2 cm since the aim was locked
- 61.07 s: SwordThrust at B Large's Chest: passed 0.0 cm from the aim point; nearest part B Large's Chest (3.0 cm); the Chest had moved 1.2 cm since the aim was locked
- 64.60 s: SwordCut at B Large's KneeL: passed 0.1 cm from the aim point; nearest part B Large's ShinL (0.7 cm); the KneeL had moved 3.4 cm since the aim was locked


## A Large #2

Present 24.9–81.8 s. Steps 54, idle shuffles 6. Feet slid at most 0.0 cm while planted; the weight was at most 45 cm from the support point; hips dropped for reach by up to 5 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 0 / 50 |
| Chest | ? | 0.2 / 19.0 | 0 / 28 |
| Head | ? | 0.2 / 10.2 | 4 / 145 |
| UpperArmL | ? | 0.3 / 16.4 | 2 / 112 |
| ForearmL | ? | 1.5 / 32.7 | 6 / 168 |
| HandL | ? | 2.9 / 52.2 | 15 / 180 |
| UpperArmR | ? | 0.2 / 11.9 | 2 / 53 |
| ForearmR | ? | 1.1 / 33.0 | 3 / 91 |
| HandR | ? | 1.7 / 36.3 | 7 / 180 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 11 (5 on the part aimed at, 3 blocked, 0 on someone in the way, 0 on allies), 216 damage. Hits taken: 9, 200 damage. Health at the end 0.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 35.43 s: ClubSwing at B Small's Chest: passed 0.0 cm from the aim point; nearest part B Small's Chest (21.2 cm); the Chest had moved 39.1 cm since the aim was locked
- 53.37 s: ClubSwing at B Medium's Belly: passed 0.0 cm from the aim point; nearest part B Medium's ForearmR (4.8 cm); the Belly had moved 3.6 cm since the aim was locked


## B Small #2

Present 24.9–81.8 s. Steps 31, idle shuffles 5. Feet slid at most 146.7 cm while planted; the weight was at most 17 cm from the support point; hips dropped for reach by up to 2 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 86.1 / 126.4 | 114 / 178 |
| Spine | Ragdoll | 94.0 / 138.0 | 98 / 157 |
| Chest | Ragdoll | 104.4 / 152.7 | 90 / 164 |
| Head | Ragdoll | 120.5 / 175.8 | 97 / 179 |
| UpperArmL | Ragdoll | 125.5 / 183.8 | 42 / 179 |
| ForearmL | Ragdoll | 114.8 / 168.8 | 40 / 179 |
| HandL | Ragdoll | 117.3 / 180.9 | 87 / 179 |
| UpperArmR | Ragdoll | 105.6 / 159.3 | 74 / 179 |
| ForearmR | Ragdoll | 98.2 / 154.7 | 66 / 179 |
| HandR | Ragdoll | 105.6 / 170.7 | 35 / 180 |
| ThighL | Ragdoll | 87.3 / 128.6 | 68 / 178 |
| ShinL | Ragdoll | 79.3 / 144.5 | 92 / 179 |
| FootL | Ragdoll | 59.4 / 147.2 | 106 / 178 |
| ThighR | Ragdoll | 79.1 / 117.6 | 77 / 180 |
| ShinR | Ragdoll | 90.2 / 134.5 | 101 / 180 |
| FootR | Ragdoll | 72.1 / 118.2 | 116 / 180 |

Hits dealt: 1 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 6 damage. Hits taken: 3, 62 damage. Killed.


## B Medium #2

Present 24.9–81.8 s. Steps 49, idle shuffles 6. Feet slid at most 47.5 cm while planted; the weight was at most 30 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 41.1 / 102.1 | 54 / 140 |
| Spine | ? | 48.4 / 120.9 | 43 / 128 |
| Chest | ? | 55.4 / 136.3 | 47 / 127 |
| Head | ? | 66.1 / 162.8 | 68 / 179 |
| UpperArmL | ? | 52.2 / 130.4 | 12 / 109 |
| ForearmL | ? | 54.4 / 134.2 | 25 / 154 |
| HandL | ? | 59.9 / 161.7 | 54 / 179 |
| UpperArmR | ? | 72.7 / 177.6 | 68 / 178 |
| ForearmR | ? | 63.1 / 157.7 | 67 / 180 |
| HandR | ? | 68.4 / 165.9 | 78 / 179 |
| ThighL | ? | 33.3 / 84.3 | 59 / 177 |
| ShinL | ? | 28.6 / 71.9 | 69 / 180 |
| FootL | ? | 16.2 / 47.7 | 68 / 180 |
| ThighR | ? | 43.5 / 108.5 | 66 / 180 |
| ShinR | ? | 22.8 / 63.7 | 70 / 180 |
| FootR | ? | 8.3 / 27.1 | 61 / 179 |

Hits dealt: 8 (0 on the part aimed at, 4 blocked, 0 on someone in the way, 0 on allies), 76 damage. Hits taken: 11, 107 damage. Health at the end 0.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.1 cm, max 0.1 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 35.37 s: SwordCut at A Small's ElbowR: passed 0.1 cm from the aim point; nearest part A Small's HandR (12.1 cm); the ElbowR had moved 24.7 cm since the aim was locked
- 37.68 s: SwordBackhand at A Small's Belly: passed 0.1 cm from the aim point; nearest part A Small's UpperArmL (9.1 cm); the Belly had moved 6.8 cm since the aim was locked
- 40.55 s: SwordBackhand at A Small's Chest: passed 0.1 cm from the aim point; nearest part A Small's Chest (0.0 cm); the Chest had moved 13.5 cm since the aim was locked


## B Large #2

Present 24.9–81.8 s. Steps 59, idle shuffles 4. Feet slid at most 128.6 cm while planted; the weight was at most 30 cm from the support point; hips dropped for reach by up to 4 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 27.3 / 162.5 | 31 / 180 |
| Spine | ? | 32.7 / 186.6 | 28 / 180 |
| Chest | ? | 38.2 / 210.5 | 28 / 177 |
| Head | ? | 46.2 / 257.1 | 32 / 180 |
| UpperArmL | ? | 46.5 / 260.7 | 32 / 180 |
| ForearmL | ? | 49.5 / 272.2 | 32 / 178 |
| HandL | ? | 61.8 / 333.1 | 33 / 179 |
| UpperArmR | ? | 43.3 / 244.3 | 26 / 178 |
| ForearmR | ? | 46.4 / 251.1 | 31 / 179 |
| HandR | ? | 52.1 / 276.6 | 36 / 180 |
| ThighL | ? | 27.4 / 171.1 | 31 / 180 |
| ShinL | ? | 20.6 / 130.6 | 31 / 180 |
| FootL | ? | 9.6 / 92.5 | 30 / 180 |
| ThighR | ? | 24.3 / 140.8 | 28 / 180 |
| ShinR | ? | 15.7 / 104.8 | 25 / 180 |
| FootR | ? | 8.3 / 141.9 | 24 / 179 |

Hits dealt: 12 (4 on the part aimed at, 4 blocked, 2 on someone in the way, 0 on allies), 194 damage. Hits taken: 10, 225 damage. Health at the end 0.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 50.22 s: ClubSwing at A Large's Neck: passed 0.0 cm from the aim point; nearest part A Large's HandL (27.0 cm); the Neck had moved 85.5 cm since the aim was locked
- 55.83 s: ClubSwing at A Medium's Neck: passed 0.0 cm from the aim point; nearest part A Medium's Chest (38.7 cm); the Neck had moved 56.1 cm since the aim was locked

