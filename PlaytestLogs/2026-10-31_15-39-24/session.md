# Playtest 2026-10-31_15-39-24

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 46.6 s of play (2797 physics steps), 1190 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_15-39-24`.

## A Small

Present 0.0–46.6 s. Steps 77, idle shuffles 20. Feet slid at most 0.0 cm while planted; the weight was at most 16 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 1 / 11 |
| Chest | Pinned | 0.1 / 2.2 | 1 / 19 |
| Head | Pinned | 0.1 / 5.2 | 3 / 39 |
| UpperArmL | Pinned | 0.2 / 6.3 | 2 / 81 |
| ForearmL | Pinned | 0.6 / 28.2 | 3 / 167 |
| HandL | Pinned | 0.7 / 27.6 | 8 / 167 |
| UpperArmR | Pinned | 0.1 / 4.2 | 0 / 68 |
| ForearmR | Pinned | 0.1 / 10.7 | 1 / 99 |
| HandR | Pinned | 0.2 / 23.8 | 2 / 123 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Blows: 14. The posed striking part passed within 3 cm of the aim point in 14; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.1 cm, max 0.1 cm. 5 reached the part they were aimed at (nearest part within 5 cm at contact).

- 6.85 s: ClubSwing at B Medium's Belly: passed 0.1 cm from the aim point; nearest part B Medium's Spine (7.9 cm); the Belly had moved 1.6 cm since the aim was locked
- 9.70 s: ClubSwing at B Medium's KneeL: passed 0.1 cm from the aim point; nearest part B Medium's ShinL (2.5 cm); the KneeL had moved 1.6 cm since the aim was locked
- 12.03 s: ClubSwing at B Medium's Belly: passed 0.1 cm from the aim point; nearest part B Medium's Spine (6.3 cm); the Belly had moved 1.0 cm since the aim was locked
- 14.45 s: ClubSwing at B Medium's Belly: passed 0.1 cm from the aim point; nearest part B Medium's Spine (6.4 cm); the Belly had moved 1.0 cm since the aim was locked
- 16.93 s: ClubSwing at B Medium's ElbowL: passed 0.1 cm from the aim point; nearest part B Medium's ForearmL (0.3 cm); the ElbowL had moved 11.5 cm since the aim was locked
- 20.10 s: ClubSwing at B Medium's Belly: passed 0.1 cm from the aim point; nearest part B Medium's Spine (5.2 cm); the Belly had moved 2.4 cm since the aim was locked
- 22.37 s: ClubSwing at B Medium's ElbowL: passed 0.1 cm from the aim point; nearest part B Medium's ForearmL (3.3 cm); the ElbowL had moved 12.3 cm since the aim was locked
- 25.10 s: ClubSwing at B Medium's KneeL: passed 0.1 cm from the aim point; nearest part B Medium's ShinL (2.8 cm); the KneeL had moved 0.1 cm since the aim was locked
- 27.58 s: ClubSwing at B Medium's Belly: passed 0.1 cm from the aim point; nearest part B Medium's Spine (5.8 cm); the Belly had moved 1.2 cm since the aim was locked
- 30.58 s: ClubSwing at B Medium's KneeR: passed 0.1 cm from the aim point; nearest part B Medium's ShinR (4.2 cm); the KneeR had moved 1.2 cm since the aim was locked
- 32.98 s: ClubSwing at B Medium's KneeL: passed 0.1 cm from the aim point; nearest part B Medium's ThighL (3.8 cm); the KneeL had moved 1.8 cm since the aim was locked
- 36.30 s: ClubSwing at B Medium's Belly: passed 0.1 cm from the aim point; nearest part B Medium's Spine (7.1 cm); the Belly had moved 2.1 cm since the aim was locked
- 39.63 s: ClubSwing at B Medium's KneeR: passed 0.1 cm from the aim point; nearest part B Medium's ShinR (5.3 cm); the KneeR had moved 2.0 cm since the aim was locked
- 43.17 s: ClubSwing at B Medium's KneeL: passed 0.1 cm from the aim point; nearest part B Medium's ThighL (4.7 cm); the KneeL had moved 10.1 cm since the aim was locked


## A Medium

Present 0.0–46.6 s. Steps 68, idle shuffles 11. Feet slid at most 0.0 cm while planted; the weight was at most 34 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 1 / 10 |
| Chest | Pinned | 0.2 / 2.8 | 0 / 13 |
| Head | Pinned | 0.1 / 3.4 | 2 / 24 |
| UpperArmL | Pinned | 0.2 / 4.2 | 1 / 40 |
| ForearmL | Pinned | 0.4 / 12.0 | 1 / 59 |
| HandL | Pinned | 0.6 / 20.3 | 6 / 114 |
| UpperArmR | Pinned | 0.1 / 5.9 | 1 / 178 |
| ForearmR | Pinned | 0.2 / 19.1 | 1 / 69 |
| HandR | Pinned | 0.3 / 23.4 | 3 / 102 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Blows: 15. The posed striking part passed within 3 cm of the aim point in 15; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.1 cm, max 0.1 cm. 6 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.17 s: SwordBackhand at B Medium's Neck: passed 0.1 cm from the aim point; nearest part B Medium's Chest (8.6 cm); the Neck had moved 7.1 cm since the aim was locked
- 6.93 s: SwordThrust at B Medium's Head: passed 0.0 cm from the aim point; nearest part B Medium's Head (0.0 cm); the Head had moved 2.2 cm since the aim was locked
- 9.30 s: SwordBackhand at B Medium's ElbowR: passed 0.1 cm from the aim point; nearest part B Medium's UpperArmR (5.2 cm); the ElbowR had moved 1.1 cm since the aim was locked
- 11.90 s: SwordCut at B Medium's Neck: passed 0.1 cm from the aim point; nearest part B Medium's Head (6.6 cm); the Neck had moved 5.1 cm since the aim was locked
- 15.25 s: SwordCut at B Medium's Chest: passed 0.1 cm from the aim point; nearest part B Medium's Chest (7.1 cm); the Chest had moved 1.0 cm since the aim was locked
- 18.10 s: SwordCut at B Medium's Neck: passed 0.1 cm from the aim point; nearest part B Medium's Head (6.9 cm); the Neck had moved 2.0 cm since the aim was locked
- 21.08 s: SwordCut at B Medium's Chest: passed 0.1 cm from the aim point; nearest part B Medium's Chest (6.8 cm); the Chest had moved 0.4 cm since the aim was locked
- 23.42 s: SwordBackhand at B Medium's Chest: passed 0.1 cm from the aim point; nearest part B Medium's Chest (0.0 cm); the Chest had moved 1.0 cm since the aim was locked
- 26.87 s: SwordBackhand at B Medium's Head: passed 0.1 cm from the aim point; nearest part B Medium's Head (0.0 cm); the Head had moved 0.9 cm since the aim was locked
- 29.45 s: SwordBackhand at B Medium's ElbowR: passed 0.1 cm from the aim point; nearest part B Medium's UpperArmR (19.9 cm); the ElbowR had moved 17.4 cm since the aim was locked
- 32.45 s: SwordBackhand at B Medium's Head: passed 0.1 cm from the aim point; nearest part B Medium's Head (0.0 cm); the Head had moved 3.9 cm since the aim was locked
- 36.02 s: SwordThrust at B Medium's Belly: passed 0.0 cm from the aim point; nearest part B Medium's Spine (9.8 cm); the Belly had moved 6.0 cm since the aim was locked
- 38.43 s: SwordBackhand at B Medium's Head: passed 0.1 cm from the aim point; nearest part B Medium's Head (0.0 cm); the Head had moved 1.3 cm since the aim was locked
- 42.08 s: SwordThrust at B Medium's Head: passed 0.0 cm from the aim point; nearest part B Medium's Head (0.0 cm); the Head had moved 2.6 cm since the aim was locked
- 45.28 s: SwordBackhand at B Medium's ElbowR: passed 0.1 cm from the aim point; nearest part B Medium's Pelvis (12.0 cm); the ElbowR had moved 23.5 cm since the aim was locked


## A Large

Present 0.0–46.6 s. Steps 39, idle shuffles 5. Feet slid at most 0.0 cm while planted; the weight was at most 34 cm from the support point; hips dropped for reach by up to 6 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 0 / 11 |
| Chest | ? | 0.1 / 4.7 | 0 / 25 |
| Head | ? | 0.2 / 13.6 | 4 / 84 |
| UpperArmL | ? | 0.2 / 17.1 | 2 / 45 |
| ForearmL | ? | 1.4 / 43.1 | 3 / 128 |
| HandL | ? | 2.2 / 41.8 | 15 / 127 |
| UpperArmR | ? | 0.0 / 1.4 | 0 / 45 |
| ForearmR | ? | 0.2 / 7.3 | 1 / 60 |
| HandR | ? | 0.4 / 36.9 | 2 / 80 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Blows: 14. The posed striking part passed within 3 cm of the aim point in 14; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 2 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.00 s: ClubSmash at B Large's Chest: passed 0.0 cm from the aim point; nearest part B Large's Chest (37.3 cm); the Chest had moved 33.7 cm since the aim was locked
- 7.50 s: ClubSwing at B Small's ElbowL: passed 0.0 cm from the aim point; nearest part B Small's ForearmL (11.5 cm); the ElbowL had moved 14.6 cm since the aim was locked
- 10.83 s: ClubSwing at B Small's KneeR: passed 0.0 cm from the aim point; nearest part B Small's ShinR (11.0 cm); the KneeR had moved 1.9 cm since the aim was locked
- 13.67 s: ClubSwing at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (3.2 cm); the Neck had moved 2.2 cm since the aim was locked
- 16.28 s: ClubSmash at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (3.7 cm); the Neck had moved 1.9 cm since the aim was locked
- 19.57 s: ClubSmash at B Small's Chest: passed 0.0 cm from the aim point; nearest part B Small's UpperArmL (9.8 cm); the Chest had moved 9.6 cm since the aim was locked
- 23.37 s: ClubSwing at B Small's Head: passed 0.0 cm from the aim point; nearest part B Small's Head (4.0 cm); the Head had moved 2.6 cm since the aim was locked
- 26.40 s: ClubSwing at B Small's KneeL: passed 0.0 cm from the aim point; nearest part B Small's ShinL (6.8 cm); the KneeL had moved 1.9 cm since the aim was locked
- 29.02 s: ClubSwing at B Small's KneeL: passed 0.0 cm from the aim point; nearest part B Small's FootL (40.9 cm); the KneeL had moved 54.1 cm since the aim was locked
- 32.22 s: ClubSmash at B Small's Chest: passed 0.0 cm from the aim point; nearest part B Small's Chest (0.7 cm); the Chest had moved 4.3 cm since the aim was locked
- 35.73 s: ClubSmash at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (4.7 cm); the Neck had moved 2.3 cm since the aim was locked
- 38.27 s: ClubSwing at B Small's Head: passed 0.0 cm from the aim point; nearest part B Small's Head (10.2 cm); the Head had moved 6.4 cm since the aim was locked
- 40.95 s: ClubSwing at B Small's KneeL: passed 0.0 cm from the aim point; nearest part B Small's FootL (35.4 cm); the KneeL had moved 38.0 cm since the aim was locked
- 44.80 s: ClubSwing at B Small's Chest: passed 0.0 cm from the aim point; nearest part B Small's Chest (6.3 cm); the Chest had moved 2.5 cm since the aim was locked


## B Small

Present 0.0–46.6 s. Steps 76, idle shuffles 19. Feet slid at most 0.0 cm while planted; the weight was at most 17 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 0 / 11 |
| Chest | ? | 0.1 / 2.1 | 0 / 20 |
| Head | ? | 0.1 / 6.5 | 2 / 111 |
| UpperArmL | ? | 0.2 / 5.1 | 1 / 26 |
| ForearmL | ? | 0.5 / 8.8 | 2 / 29 |
| HandL | ? | 0.6 / 7.6 | 6 / 68 |
| UpperArmR | ? | 0.1 / 5.6 | 0 / 68 |
| ForearmR | ? | 0.1 / 11.7 | 1 / 101 |
| HandR | ? | 0.2 / 23.6 | 3 / 124 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Blows: 13. The posed striking part passed within 3 cm of the aim point in 13; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.1 cm, max 0.1 cm. 3 reached the part they were aimed at (nearest part within 5 cm at contact).

- 7.05 s: ClubSwing at A Large's KneeR: passed 0.1 cm from the aim point; nearest part A Large's ShinR (5.4 cm); the KneeR had moved 3.1 cm since the aim was locked
- 9.85 s: ClubSwing at A Large's KneeL: passed 0.1 cm from the aim point; nearest part A Large's ThighL (3.3 cm); the KneeL had moved 0.3 cm since the aim was locked
- 12.63 s: ClubSwing at A Large's KneeL: passed 0.1 cm from the aim point; nearest part A Large's ThighL (2.5 cm); the KneeL had moved 0.5 cm since the aim was locked
- 15.80 s: ClubSwing at A Large's KneeL: passed 0.1 cm from the aim point; nearest part A Large's ThighL (2.8 cm); the KneeL had moved 0.2 cm since the aim was locked
- 19.15 s: ClubSwing at A Large's KneeR: passed 0.1 cm from the aim point; nearest part A Large's ShinR (4.5 cm); the KneeR had moved 3.5 cm since the aim was locked
- 21.85 s: ClubSwing at A Large's KneeR: passed 0.1 cm from the aim point; nearest part A Large's ShinR (5.9 cm); the KneeR had moved 2.1 cm since the aim was locked
- 25.00 s: ClubSwing at A Large's KneeL: passed 0.1 cm from the aim point; nearest part A Large's ThighL (2.7 cm); the KneeL had moved 1.2 cm since the aim was locked
- 28.50 s: ClubSwing at A Large's KneeR: passed 0.1 cm from the aim point; nearest part A Large's ShinR (4.5 cm); the KneeR had moved 0.4 cm since the aim was locked
- 30.78 s: ClubSwing at A Large's KneeL: passed 0.1 cm from the aim point; nearest part A Large's ThighL (3.2 cm); the KneeL had moved 0.1 cm since the aim was locked
- 33.65 s: ClubSwing at A Large's KneeL: passed 0.1 cm from the aim point; nearest part A Large's ShinL (2.8 cm); the KneeL had moved 0.8 cm since the aim was locked
- 37.10 s: ClubSwing at A Large's KneeL: passed 0.1 cm from the aim point; nearest part A Large's ThighL (3.0 cm); the KneeL had moved 0.5 cm since the aim was locked
- 40.35 s: ClubSwing at A Large's KneeL: passed 0.1 cm from the aim point; nearest part A Large's ThighL (2.8 cm); the KneeL had moved 0.2 cm since the aim was locked
- 43.93 s: ClubSwing at A Large's KneeL: passed 0.1 cm from the aim point; nearest part A Large's ThighL (3.0 cm); the KneeL had moved 0.4 cm since the aim was locked


## B Medium

Present 0.0–46.6 s. Steps 47, idle shuffles 10. Feet slid at most 0.0 cm while planted; the weight was at most 30 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 7.1 | 1 / 104 |
| Chest | ? | 0.1 / 6.0 | 1 / 21 |
| Head | ? | 0.2 / 6.3 | 4 / 63 |
| UpperArmL | ? | 0.2 / 9.3 | 1 / 47 |
| ForearmL | ? | 0.5 / 21.4 | 2 / 168 |
| HandL | ? | 0.7 / 32.5 | 7 / 171 |
| UpperArmR | ? | 0.1 / 7.3 | 0 / 41 |
| ForearmR | ? | 0.2 / 4.8 | 1 / 62 |
| HandR | ? | 0.3 / 24.0 | 4 / 66 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Blows: 15. The posed striking part passed within 3 cm of the aim point in 15; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.1 cm, max 0.1 cm. 9 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.88 s: SwordBackhand at A Small's Neck: passed 0.1 cm from the aim point; nearest part A Small's Chest (10.5 cm); the Neck had moved 16.2 cm since the aim was locked
- 7.55 s: SwordBackhand at A Small's Belly: passed 0.1 cm from the aim point; nearest part A Small's Pelvis (4.7 cm); the Belly had moved 1.7 cm since the aim was locked
- 10.58 s: SwordCut at A Small's ElbowR: passed 0.1 cm from the aim point; nearest part A Small's ForearmR (7.0 cm); the ElbowR had moved 3.0 cm since the aim was locked
- 13.18 s: SwordBackhand at A Small's Neck: passed 0.1 cm from the aim point; nearest part A Small's Chest (3.6 cm); the Neck had moved 1.3 cm since the aim was locked
- 16.35 s: SwordBackhand at A Small's KneeR: passed 0.1 cm from the aim point; nearest part A Small's ShinR (0.2 cm); the KneeR had moved 2.8 cm since the aim was locked
- 19.57 s: SwordThrust at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (0.0 cm); the Chest had moved 1.4 cm since the aim was locked
- 22.42 s: SwordThrust at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (34.4 cm); the Chest had moved 52.7 cm since the aim was locked
- 25.92 s: SwordThrust at A Small's Neck: passed 0.0 cm from the aim point; nearest part A Small's Head (5.0 cm); the Neck had moved 2.0 cm since the aim was locked
- 29.23 s: SwordThrust at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (0.0 cm); the Chest had moved 2.6 cm since the aim was locked
- 31.55 s: SwordThrust at A Small's Belly: passed 0.0 cm from the aim point; nearest part A Small's Spine (6.0 cm); the Belly had moved 0.6 cm since the aim was locked
- 34.13 s: SwordThrust at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (0.0 cm); the Chest had moved 2.0 cm since the aim was locked
- 37.58 s: SwordThrust at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (3.2 cm); the Chest had moved 2.6 cm since the aim was locked
- 40.47 s: SwordCut at A Small's KneeR: passed 0.1 cm from the aim point; nearest part A Small's ShinR (5.0 cm); the KneeR had moved 1.7 cm since the aim was locked
- 42.85 s: SwordThrust at A Small's Neck: passed 0.0 cm from the aim point; nearest part A Small's Head (4.9 cm); the Neck had moved 4.4 cm since the aim was locked
- 45.57 s: SwordCut at A Small's Chest: passed 0.1 cm from the aim point; nearest part A Small's Chest (1.9 cm); the Chest had moved 1.9 cm since the aim was locked


## B Large

Present 0.0–46.6 s. Steps 69, idle shuffles 4. Feet slid at most 0.0 cm while planted; the weight was at most 37 cm from the support point; hips dropped for reach by up to 4 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 0 / 6 |
| Chest | Pinned | 0.2 / 2.6 | 0 / 4 |
| Head | Pinned | 0.1 / 2.0 | 2 / 20 |
| UpperArmL | Pinned | 0.2 / 3.6 | 2 / 17 |
| ForearmL | Pinned | 1.2 / 10.4 | 3 / 25 |
| HandL | Pinned | 2.2 / 16.9 | 14 / 77 |
| UpperArmR | Kinematic | 0.0 / 1.4 | 0 / 45 |
| ForearmR | Kinematic | 0.2 / 6.3 | 1 / 60 |
| HandR | Kinematic | 0.4 / 36.7 | 2 / 78 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Blows: 13. The posed striking part passed within 3 cm of the aim point in 13; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 5 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.45 s: ClubSwing at A Medium's Belly: passed 0.0 cm from the aim point; nearest part A Medium's UpperArmR (4.8 cm); the Belly had moved 8.7 cm since the aim was locked
- 7.20 s: ClubSmash at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Chest (4.1 cm); the Chest had moved 2.3 cm since the aim was locked
- 10.42 s: ClubSwing at A Large's Belly: passed 0.0 cm from the aim point; nearest part A Large's Chest (17.3 cm); the Belly had moved 3.2 cm since the aim was locked
- 13.50 s: ClubSmash at A Large's Head: passed 0.0 cm from the aim point; nearest part A Large's Head (0.0 cm); the Head had moved 11.4 cm since the aim was locked
- 16.42 s: ClubSmash at A Large's Head: passed 0.0 cm from the aim point; nearest part A Large's Chest (33.6 cm); the Head had moved 49.1 cm since the aim was locked
- 19.30 s: ClubSwing at A Large's Head: passed 0.0 cm from the aim point; nearest part A Large's Head (8.2 cm); the Head had moved 14.1 cm since the aim was locked
- 22.72 s: ClubSwing at A Large's Head: passed 0.0 cm from the aim point; nearest part A Large's Head (4.6 cm); the Head had moved 1.4 cm since the aim was locked
- 26.17 s: ClubSwing at A Large's Belly: passed 0.0 cm from the aim point; nearest part A Large's Spine (21.7 cm); the Belly had moved 6.2 cm since the aim was locked
- 29.33 s: ClubSmash at A Medium's Neck: passed 0.0 cm from the aim point; nearest part A Medium's Chest (32.0 cm); the Neck had moved 49.1 cm since the aim was locked
- 32.18 s: ClubSwing at A Medium's Head: passed 0.0 cm from the aim point; nearest part A Medium's Head (0.0 cm); the Head had moved 4.3 cm since the aim was locked
- 35.48 s: ClubSmash at A Large's Chest: passed 0.0 cm from the aim point; nearest part A Large's Chest (6.3 cm); the Chest had moved 9.1 cm since the aim was locked
- 39.45 s: ClubSmash at A Large's Neck: passed 0.0 cm from the aim point; nearest part A Large's Head (8.1 cm); the Neck had moved 2.7 cm since the aim was locked
- 43.43 s: ClubSmash at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Chest (1.2 cm); the Chest had moved 1.7 cm since the aim was locked

