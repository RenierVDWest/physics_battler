# Playtest 2026-10-31_15-17-09

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 29.7 s of play (1784 physics steps), 700 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_15-17-09`.

## A Small

Present 0.0–29.7 s. Steps 68, idle shuffles 10. Feet slid at most 0.0 cm while planted; the weight was at most 12 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 1 / 15 |
| Chest | Pinned | 0.1 / 2.1 | 1 / 14 |
| Head | Pinned | 0.1 / 3.1 | 1 / 17 |
| UpperArmL | Pinned | 0.1 / 2.1 | 2 / 105 |
| ForearmL | Pinned | 0.4 / 11.5 | 3 / 145 |
| HandL | Pinned | 0.6 / 14.4 | 5 / 104 |
| UpperArmR | Pinned | 0.1 / 3.7 | 2 / 109 |
| ForearmR | Pinned | 0.4 / 8.3 | 7 / 171 |
| HandR | Pinned | 0.5 / 17.3 | 23 / 180 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Blows: 8. The posed striking part passed within 3 cm of the aim point in 8; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 18.6 cm, max 29.4 cm. 5 reached the part they were aimed at (nearest part within 5 cm at contact).

- 9.28 s: ClubSwing at B Medium's KneeL: passed 0.0 cm from the aim point; nearest part B Medium's ThighL (1.3 cm); the KneeL had moved 1.6 cm since the aim was locked
- 12.08 s: ClubSwing at B Medium's KneeL: passed 0.0 cm from the aim point; nearest part B Medium's ShinL (2.1 cm); the KneeL had moved 1.4 cm since the aim was locked
- 14.80 s: ClubSwing at B Medium's KneeL: passed 0.0 cm from the aim point; nearest part B Medium's ThighL (1.8 cm); the KneeL had moved 1.3 cm since the aim was locked
- 17.23 s: ClubSwing at B Medium's KneeR: passed 0.0 cm from the aim point; nearest part B Medium's ShinR (2.1 cm); the KneeR had moved 0.7 cm since the aim was locked
- 20.08 s: ClubSwing at B Medium's KneeR: passed 0.0 cm from the aim point; nearest part B Medium's ShinR (1.5 cm); the KneeR had moved 1.5 cm since the aim was locked
- 23.52 s: ClubSwing at B Medium's KneeL: passed 0.0 cm from the aim point; nearest part B Medium's ShinL (2.7 cm); the KneeL had moved 3.1 cm since the aim was locked
- 25.73 s: ClubSwing at B Medium's KneeL: passed 0.0 cm from the aim point; nearest part B Medium's ShinL (2.1 cm); the KneeL had moved 1.3 cm since the aim was locked
- 28.70 s: ClubSwing at B Medium's KneeR: passed 0.0 cm from the aim point; nearest part B Medium's ThighR (1.9 cm); the KneeR had moved 0.3 cm since the aim was locked


## A Medium

Present 0.0–29.7 s. Steps 41, idle shuffles 6. Feet slid at most 0.0 cm while planted; the weight was at most 29 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 0 / 13 |
| Chest | Pinned | 0.1 / 3.0 | 0 / 19 |
| Head | Pinned | 0.2 / 8.1 | 2 / 61 |
| UpperArmL | Pinned | 0.2 / 7.4 | 2 / 40 |
| ForearmL | Pinned | 0.4 / 12.0 | 13 / 59 |
| HandL | Pinned | 0.7 / 20.3 | 35 / 114 |
| UpperArmR | Pinned | 0.2 / 7.8 | 2 / 65 |
| ForearmR | Pinned | 0.7 / 13.8 | 9 / 165 |
| HandR | Pinned | 1.0 / 23.4 | 19 / 177 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Blows: 10. The posed striking part passed within 3 cm of the aim point in 10; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 29.1 cm, max 61.7 cm. 5 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.72 s: SwordThrust at B Large's Chest: passed 0.0 cm from the aim point; nearest part B Large's Chest (15.9 cm); the Chest had moved 54.2 cm since the aim was locked
- 6.45 s: SwordThrust at B Medium's Belly: passed 0.0 cm from the aim point; nearest part B Medium's Spine (0.0 cm); the Belly had moved 6.7 cm since the aim was locked
- 9.13 s: SwordBackhand at B Medium's Belly: passed 0.1 cm from the aim point; nearest part B Medium's Spine (0.0 cm); the Belly had moved 1.9 cm since the aim was locked
- 11.98 s: SwordCut at B Medium's Head: passed 0.1 cm from the aim point; nearest part B Medium's HandR (39.7 cm); the Head had moved 53.9 cm since the aim was locked
- 15.23 s: SwordBackhand at B Medium's Chest: passed 0.1 cm from the aim point; nearest part B Medium's Chest (0.0 cm); the Chest had moved 1.1 cm since the aim was locked
- 17.93 s: SwordThrust at B Medium's Chest: passed 0.0 cm from the aim point; nearest part B Medium's ForearmR (17.0 cm); the Chest had moved 23.7 cm since the aim was locked
- 20.78 s: SwordBackhand at B Medium's Chest: passed 0.1 cm from the aim point; nearest part B Medium's HandL (25.7 cm); the Chest had moved 58.3 cm since the aim was locked
- 23.85 s: SwordThrust at B Medium's Head: passed 0.0 cm from the aim point; nearest part B Medium's Head (29.8 cm); the Head had moved 41.4 cm since the aim was locked
- 26.50 s: SwordThrust at B Medium's Chest: passed 0.0 cm from the aim point; nearest part B Medium's Chest (2.8 cm); the Chest had moved 2.1 cm since the aim was locked
- 28.77 s: SwordThrust at B Medium's Chest: passed 0.0 cm from the aim point; nearest part B Medium's Chest (3.5 cm); the Chest had moved 1.7 cm since the aim was locked


## A Large

Present 0.0–29.7 s. Steps 31, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 28 cm from the support point; hips dropped for reach by up to 6 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 0 / 7 |
| Chest | Pinned | 0.2 / 3.0 | 0 / 10 |
| Head | Pinned | 0.2 / 3.9 | 3 / 57 |
| UpperArmL | Pinned | 0.2 / 5.8 | 2 / 19 |
| ForearmL | Pinned | 1.2 / 11.6 | 5 / 53 |
| HandL | Pinned | 3.1 / 19.0 | 25 / 128 |
| UpperArmR | Pinned | 0.3 / 5.1 | 2 / 45 |
| ForearmR | Pinned | 0.9 / 8.3 | 6 / 87 |
| HandR | Pinned | 1.4 / 36.9 | 27 / 180 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Blows: 8. The posed striking part passed within 3 cm of the aim point in 8; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 80.5 cm, max 98.1 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.47 s: ClubSwing at B Large's Head: passed 0.0 cm from the aim point; nearest part B Large's Head (3.9 cm); the Head had moved 2.2 cm since the aim was locked
- 6.78 s: ClubSwing at B Small's Head: passed 0.0 cm from the aim point; nearest part B Small's Head (8.3 cm); the Head had moved 13.7 cm since the aim was locked
- 10.35 s: ClubSwing at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's HandR (27.9 cm); the Belly had moved 31.8 cm since the aim was locked
- 13.22 s: ClubSwing at B Small's KneeL: passed 0.0 cm from the aim point; nearest part B Small's FootL (24.3 cm); the KneeL had moved 24.9 cm since the aim was locked
- 16.23 s: ClubSmash at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (9.3 cm); the Neck had moved 11.7 cm since the aim was locked
- 19.75 s: ClubSmash at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (3.2 cm); the Neck had moved 3.1 cm since the aim was locked
- 23.30 s: ClubSmash at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (3.3 cm); the Neck had moved 0.9 cm since the aim was locked
- 27.17 s: ClubSmash at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (2.8 cm); the Neck had moved 0.9 cm since the aim was locked


## B Small

Present 0.0–29.7 s. Steps 66, idle shuffles 10. Feet slid at most 0.0 cm while planted; the weight was at most 11 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 1 / 16 |
| Chest | Pinned | 0.1 / 1.9 | 0 / 10 |
| Head | Pinned | 0.1 / 2.6 | 1 / 13 |
| UpperArmL | Pinned | 0.1 / 1.7 | 1 / 16 |
| ForearmL | Pinned | 0.3 / 5.1 | 2 / 21 |
| HandL | Pinned | 0.5 / 6.4 | 4 / 41 |
| UpperArmR | Pinned | 0.1 / 3.2 | 2 / 72 |
| ForearmR | Pinned | 0.3 / 9.0 | 5 / 106 |
| HandR | Pinned | 0.5 / 17.2 | 22 / 180 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Blows: 7. The posed striking part passed within 3 cm of the aim point in 7; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 22.5 cm, max 29.2 cm. 3 reached the part they were aimed at (nearest part within 5 cm at contact).

- 9.80 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (2.6 cm); the KneeL had moved 0.3 cm since the aim was locked
- 12.72 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (2.5 cm); the KneeL had moved 0.8 cm since the aim was locked
- 15.85 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (3.4 cm); the KneeL had moved 4.1 cm since the aim was locked
- 19.22 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (2.7 cm); the KneeL had moved 0.9 cm since the aim was locked
- 21.92 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (2.8 cm); the KneeL had moved 0.5 cm since the aim was locked
- 24.53 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (2.8 cm); the KneeL had moved 0.4 cm since the aim was locked
- 27.68 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (20.5 cm); the KneeL had moved 27.9 cm since the aim was locked


## B Medium

Present 0.0–29.7 s. Steps 31, idle shuffles 6. Feet slid at most 0.0 cm while planted; the weight was at most 27 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 0 / 14 |
| Chest | Pinned | 0.1 / 3.4 | 0 / 11 |
| Head | Pinned | 0.2 / 3.5 | 3 / 42 |
| UpperArmL | Pinned | 0.2 / 4.6 | 2 / 40 |
| ForearmL | Pinned | 0.3 / 13.8 | 13 / 59 |
| HandL | Pinned | 0.7 / 19.6 | 35 / 110 |
| UpperArmR | Pinned | 0.2 / 4.3 | 2 / 41 |
| ForearmR | Pinned | 0.5 / 4.8 | 5 / 90 |
| HandR | Pinned | 0.8 / 24.0 | 18 / 178 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Blows: 8. The posed striking part passed within 3 cm of the aim point in 8; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 39.7 cm, max 93.8 cm. 2 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.65 s: SwordCut at A Medium's Neck: passed 0.1 cm from the aim point; nearest part A Medium's Head (7.2 cm); the Neck had moved 3.0 cm since the aim was locked
- 7.78 s: SwordCut at A Small's Neck: passed 0.1 cm from the aim point; nearest part A Small's Chest (3.4 cm); the Neck had moved 9.9 cm since the aim was locked
- 11.37 s: SwordBackhand at A Small's KneeR: passed 0.1 cm from the aim point; nearest part A Small's ShinR (1.8 cm); the KneeR had moved 0.7 cm since the aim was locked
- 14.25 s: SwordCut at A Small's Neck: passed 0.1 cm from the aim point; nearest part A Small's Chest (2.0 cm); the Neck had moved 0.3 cm since the aim was locked
- 17.38 s: SwordBackhand at A Small's Belly: passed 0.1 cm from the aim point; nearest part A Small's Pelvis (3.9 cm); the Belly had moved 19.2 cm since the aim was locked
- 20.42 s: SwordBackhand at A Small's KneeR: passed 0.1 cm from the aim point; nearest part A Small's ShinR (3.3 cm); the KneeR had moved 3.3 cm since the aim was locked
- 23.73 s: SwordCut at A Small's Belly: passed 0.1 cm from the aim point; nearest part A Small's HandR (1.9 cm); the Belly had moved 2.6 cm since the aim was locked
- 27.10 s: SwordCut at A Small's Neck: passed 0.1 cm from the aim point; nearest part A Small's Chest (1.3 cm); the Neck had moved 3.4 cm since the aim was locked


## B Large

Present 0.0–29.7 s. Steps 30, idle shuffles 4. Feet slid at most 0.0 cm while planted; the weight was at most 36 cm from the support point; hips dropped for reach by up to 4 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 0 / 15 |
| Chest | Pinned | 0.2 / 2.8 | 0 / 5 |
| Head | Pinned | 0.2 / 3.8 | 1 / 16 |
| UpperArmL | Pinned | 0.2 / 3.4 | 2 / 16 |
| ForearmL | Pinned | 1.2 / 9.7 | 5 / 79 |
| HandL | Pinned | 3.3 / 21.5 | 20 / 129 |
| UpperArmR | Pinned | 0.2 / 4.5 | 2 / 45 |
| ForearmR | Pinned | 0.9 / 9.7 | 7 / 72 |
| HandR | Pinned | 1.3 / 36.7 | 29 / 179 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Blows: 8. The posed striking part passed within 3 cm of the aim point in 8; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 84.7 cm, max 106.8 cm. 2 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.70 s: ClubSwing at A Medium's Neck: passed 0.0 cm from the aim point; nearest part A Large's HandL (15.0 cm); the Neck had moved 48.2 cm since the aim was locked
- 7.02 s: ClubSwing at A Medium's Neck: passed 0.0 cm from the aim point; nearest part A Medium's Chest (23.4 cm); the Neck had moved 37.9 cm since the aim was locked
- 10.27 s: ClubSwing at A Medium's Neck: passed 0.0 cm from the aim point; nearest part A Medium's Chest (8.2 cm); the Neck had moved 3.5 cm since the aim was locked
- 14.03 s: ClubSwing at A Medium's Neck: passed 0.0 cm from the aim point; nearest part A Medium's Chest (8.8 cm); the Neck had moved 1.0 cm since the aim was locked
- 17.42 s: ClubSwing at A Medium's Neck: passed 0.0 cm from the aim point; nearest part A Medium's Chest (7.9 cm); the Neck had moved 2.4 cm since the aim was locked
- 21.08 s: ClubSmash at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Chest (1.5 cm); the Chest had moved 2.6 cm since the aim was locked
- 24.68 s: ClubSmash at A Medium's ElbowL: passed 0.0 cm from the aim point; nearest part A Medium's ForearmL (4.7 cm); the ElbowL had moved 4.4 cm since the aim was locked
- 28.45 s: ClubSmash at A Medium's Neck: passed 0.0 cm from the aim point; nearest part A Medium's Head (8.4 cm); the Neck had moved 2.9 cm since the aim was locked

