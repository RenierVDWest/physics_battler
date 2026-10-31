# Playtest 2026-10-31_15-19-58

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 16.9 s of play (1014 physics steps), 588 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_15-19-58`.

## A Small

Present 0.0–16.9 s. Steps 48, idle shuffles 4. Feet slid at most 0.0 cm while planted; the weight was at most 10 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 0 / 7 |
| Chest | ? | 0.1 / 0.9 | 0 / 14 |
| Head | ? | 0.1 / 2.6 | 1 / 21 |
| UpperArmL | ? | 0.1 / 3.4 | 2 / 125 |
| ForearmL | ? | 0.4 / 10.7 | 3 / 169 |
| HandL | ? | 0.5 / 15.6 | 5 / 171 |
| UpperArmR | ? | 0.1 / 2.3 | 2 / 71 |
| ForearmR | ? | 0.3 / 8.3 | 6 / 108 |
| HandR | ? | 0.5 / 17.3 | 20 / 180 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 23.2 cm, max 27.5 cm. 2 reached the part they were aimed at (nearest part within 5 cm at contact).

- 9.23 s: ClubSwing at B Medium's KneeL: passed 0.0 cm from the aim point; nearest part B Medium's ThighL (1.3 cm); the KneeL had moved 1.6 cm since the aim was locked
- 11.97 s: ClubSwing at B Medium's KneeL: passed 0.0 cm from the aim point; nearest part B Medium's ShinL (2.0 cm); the KneeL had moved 0.9 cm since the aim was locked
- 14.62 s: ClubSwing at B Medium's KneeL: passed 0.0 cm from the aim point; nearest part B Medium's ShinL (2.3 cm); the KneeL had moved 1.9 cm since the aim was locked


## A Medium

Present 0.0–16.9 s. Steps 28, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 32 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 1 / 30 |
| Chest | ? | 0.3 / 6.2 | 1 / 69 |
| Head | ? | 0.4 / 9.4 | 4 / 68 |
| UpperArmL | ? | 0.5 / 18.5 | 3 / 70 |
| ForearmL | ? | 0.6 / 13.8 | 14 / 66 |
| HandL | ? | 1.0 / 20.3 | 37 / 118 |
| UpperArmR | ? | 0.6 / 28.0 | 4 / 148 |
| ForearmR | ? | 1.3 / 32.8 | 11 / 158 |
| HandR | ? | 1.9 / 42.3 | 25 / 180 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 36.0 cm, max 52.1 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.43 s: SwordCut at B Medium's Neck: passed 0.1 cm from the aim point; nearest part B Medium's Chest (16.2 cm); the Neck had moved 24.8 cm since the aim was locked
- 6.72 s: SwordCut at B Medium's KneeL: passed 0.1 cm from the aim point; nearest part B Medium's ShinL (4.7 cm); the KneeL had moved 3.0 cm since the aim was locked
- 9.23 s: SwordCut at B Medium's Chest: passed 0.1 cm from the aim point; nearest part B Medium's Chest (5.1 cm); the Chest had moved 1.1 cm since the aim was locked
- 12.00 s: SwordBackhand at B Medium's ElbowR: passed 0.1 cm from the aim point; nearest part B Medium's UpperArmR (4.8 cm); the ElbowR had moved 1.7 cm since the aim was locked
- 15.28 s: SwordThrust at B Medium's Neck: passed 0.0 cm from the aim point; nearest part B Medium's Head (6.4 cm); the Neck had moved 0.8 cm since the aim was locked


## A Large

Present 0.0–16.9 s. Steps 28, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 38 cm from the support point; hips dropped for reach by up to 6 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 1 / 11 |
| Chest | Pinned | 0.2 / 3.3 | 0 / 8 |
| Head | Pinned | 0.3 / 3.7 | 3 / 29 |
| UpperArmL | Pinned | 0.3 / 6.0 | 3 / 87 |
| ForearmL | Pinned | 1.7 / 35.6 | 6 / 111 |
| HandL | Pinned | 3.9 / 61.9 | 22 / 179 |
| UpperArmR | Pinned | 0.3 / 4.6 | 3 / 90 |
| ForearmR | Pinned | 1.3 / 31.2 | 7 / 111 |
| HandR | Pinned | 2.3 / 71.5 | 24 / 164 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 78.7 cm, max 87.2 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.40 s: ClubSwing at B Large's ElbowR: passed 0.0 cm from the aim point; nearest part B Large's UpperArmR (14.6 cm); the ElbowR had moved 27.3 cm since the aim was locked
- 7.27 s: ClubSmash at B Small's Chest: passed 0.0 cm from the aim point; nearest part B Small's Chest (1.7 cm); the Chest had moved 14.7 cm since the aim was locked
- 10.90 s: ClubSwing at B Small's ElbowR: passed 0.0 cm from the aim point; nearest part B Small's ForearmR (11.5 cm); the ElbowR had moved 2.2 cm since the aim was locked
- 13.95 s: ClubSwing at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's HandR (2.5 cm); the Belly had moved 1.1 cm since the aim was locked


## B Small

Present 0.0–16.9 s. Steps 51, idle shuffles 4. Feet slid at most 0.0 cm while planted; the weight was at most 12 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 1 / 13 |
| Chest | ? | 0.1 / 1.8 | 0 / 9 |
| Head | ? | 0.1 / 2.1 | 1 / 10 |
| UpperArmL | ? | 0.1 / 1.7 | 1 / 18 |
| ForearmL | ? | 0.3 / 5.1 | 2 / 21 |
| HandL | ? | 0.4 / 5.8 | 4 / 41 |
| UpperArmR | ? | 0.1 / 2.5 | 2 / 72 |
| ForearmR | ? | 0.3 / 9.0 | 4 / 108 |
| HandR | ? | 0.5 / 17.2 | 19 / 178 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 24.5 cm, max 29.8 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 10.05 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (2.7 cm); the KneeL had moved 0.3 cm since the aim was locked
- 12.93 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (2.7 cm); the KneeL had moved 0.6 cm since the aim was locked
- 15.87 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (2.7 cm); the KneeL had moved 0.1 cm since the aim was locked


## B Medium

Present 0.0–16.9 s. Steps 25, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 31 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 1 / 18 |
| Chest | Pinned | 0.2 / 5.4 | 1 / 30 |
| Head | Pinned | 0.3 / 8.0 | 5 / 53 |
| UpperArmL | Pinned | 0.3 / 15.1 | 2 / 84 |
| ForearmL | Pinned | 0.6 / 24.4 | 14 / 107 |
| HandL | Pinned | 1.0 / 19.6 | 38 / 176 |
| UpperArmR | Pinned | 0.3 / 6.7 | 4 / 170 |
| ForearmR | Pinned | 0.7 / 16.9 | 6 / 159 |
| HandR | Pinned | 1.3 / 24.0 | 22 / 180 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 76.0 cm, max 110.0 cm. 5 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.58 s: SwordBackhand at A Medium's KneeR: passed 0.1 cm from the aim point; nearest part A Medium's ShinR (1.5 cm); the KneeR had moved 0.5 cm since the aim was locked
- 8.05 s: SwordThrust at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (0.0 cm); the Chest had moved 6.8 cm since the aim was locked
- 10.67 s: SwordThrust at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (2.1 cm); the Chest had moved 1.9 cm since the aim was locked
- 12.95 s: SwordCut at A Small's Chest: passed 0.1 cm from the aim point; nearest part A Small's Chest (2.8 cm); the Chest had moved 0.8 cm since the aim was locked
- 16.03 s: SwordBackhand at A Small's KneeR: passed 0.1 cm from the aim point; nearest part A Small's ShinR (2.4 cm); the KneeR had moved 0.8 cm since the aim was locked


## B Large

Present 0.0–16.9 s. Steps 22, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 33 cm from the support point; hips dropped for reach by up to 4 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 1 / 12 |
| Chest | ? | 0.2 / 3.4 | 0 / 4 |
| Head | ? | 0.3 / 5.5 | 3 / 27 |
| UpperArmL | ? | 0.3 / 5.5 | 3 / 156 |
| ForearmL | ? | 1.5 / 32.4 | 6 / 131 |
| HandL | ? | 3.5 / 32.9 | 19 / 149 |
| UpperArmR | ? | 0.3 / 5.2 | 2 / 45 |
| ForearmR | ? | 1.0 / 10.9 | 8 / 78 |
| HandR | ? | 1.8 / 36.7 | 27 / 179 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 72.0 cm, max 82.0 cm. 3 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.72 s: ClubSwing at A Medium's KneeL: passed 0.0 cm from the aim point; nearest part A Medium's ThighL (14.8 cm); the KneeL had moved 10.8 cm since the aim was locked
- 6.52 s: ClubSmash at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Chest (0.0 cm); the Chest had moved 6.9 cm since the aim was locked
- 10.18 s: ClubSmash at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Chest (1.0 cm); the Chest had moved 2.3 cm since the aim was locked
- 12.73 s: ClubSwing at A Medium's KneeL: passed 0.0 cm from the aim point; nearest part A Medium's ShinL (6.5 cm); the KneeL had moved 17.4 cm since the aim was locked
- 15.50 s: ClubSmash at A Medium's Head: passed 0.0 cm from the aim point; nearest part A Medium's Head (1.3 cm); the Head had moved 6.5 cm since the aim was locked

