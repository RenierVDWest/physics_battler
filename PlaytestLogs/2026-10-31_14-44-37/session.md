# Playtest 2026-10-31_14-44-37

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 152.5 s of play (9149 physics steps), 3460 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_14-44-37`.

## A Small

Present 0.0–152.5 s. Steps 247, idle shuffles 71. Feet slid at most 0.0 cm while planted; the weight was at most 17 cm from the support point; hips dropped for reach by up to 26 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.4 | 1 / 71 |
| Chest | ? | 0.1 / 6.4 | 1 / 74 |
| Head | ? | 0.2 / 10.6 | 2 / 133 |
| UpperArmL | ? | 0.2 / 9.7 | 2 / 141 |
| ForearmL | ? | 0.5 / 14.7 | 3 / 162 |
| HandL | ? | 0.7 / 25.5 | 6 / 176 |
| UpperArmR | ? | 0.2 / 17.4 | 4 / 174 |
| ForearmR | ? | 0.6 / 53.7 | 8 / 180 |
| HandR | ? | 0.8 / 64.6 | 28 / 180 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Blows: 52. The posed striking part passed within 3 cm of the aim point in 52; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 18.2 cm, max 34.3 cm. 21 reached the part they were aimed at (nearest part within 5 cm at contact).

- 9.62 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (2.8 cm); the KneeL had moved 1.0 cm since the aim was locked
- 12.40 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (2.6 cm); the KneeL had moved 0.5 cm since the aim was locked
- 15.65 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (2.8 cm); the KneeL had moved 1.0 cm since the aim was locked
- 18.40 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (2.8 cm); the KneeL had moved 0.4 cm since the aim was locked
- 21.62 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (2.8 cm); the KneeL had moved 0.5 cm since the aim was locked
- 23.78 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (2.7 cm); the KneeL had moved 0.0 cm since the aim was locked
- 26.05 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (2.8 cm); the KneeL had moved 0.7 cm since the aim was locked
- 29.18 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (2.6 cm); the KneeL had moved 0.2 cm since the aim was locked
- 31.45 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (2.8 cm); the KneeL had moved 1.0 cm since the aim was locked
- 33.70 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (6.2 cm); the KneeL had moved 9.7 cm since the aim was locked
- 36.48 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (18.7 cm); the KneeL had moved 26.0 cm since the aim was locked
- 39.42 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (35.1 cm); the KneeL had moved 43.4 cm since the aim was locked
- 41.65 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (4.8 cm); the KneeL had moved 7.2 cm since the aim was locked
- 44.45 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (2.7 cm); the KneeL had moved 0.3 cm since the aim was locked
- 46.87 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (2.7 cm); the KneeL had moved 0.5 cm since the aim was locked
- 50.15 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (2.5 cm); the KneeL had moved 0.6 cm since the aim was locked
- 53.35 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (2.4 cm); the KneeL had moved 0.8 cm since the aim was locked
- 55.47 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (2.0 cm); the KneeL had moved 7.1 cm since the aim was locked
- 58.53 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (2.8 cm); the KneeL had moved 0.3 cm since the aim was locked
- 61.02 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (23.6 cm); the KneeL had moved 58.2 cm since the aim was locked
- 64.02 s: ClubSwing at B Large's KneeR: passed 0.0 cm from the aim point; nearest part B Large's ThighR (17.7 cm); the KneeR had moved 29.7 cm since the aim was locked
- 66.70 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (4.5 cm); the KneeL had moved 9.4 cm since the aim was locked
- 68.98 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (2.7 cm); the KneeL had moved 0.4 cm since the aim was locked
- 71.63 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (2.8 cm); the KneeL had moved 0.5 cm since the aim was locked
- 74.97 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (1.4 cm); the KneeL had moved 3.5 cm since the aim was locked
- 77.92 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (2.5 cm); the KneeL had moved 0.6 cm since the aim was locked
- 81.00 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (1.8 cm); the KneeL had moved 2.8 cm since the aim was locked
- 84.02 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (2.8 cm); the KneeL had moved 0.6 cm since the aim was locked
- 86.28 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (2.8 cm); the KneeL had moved 0.8 cm since the aim was locked
- 88.83 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (0.0 cm); the KneeL had moved 25.7 cm since the aim was locked
- 91.90 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (13.0 cm); the KneeL had moved 51.5 cm since the aim was locked
- 95.17 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (2.5 cm); the KneeL had moved 21.9 cm since the aim was locked
- 97.28 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (2.7 cm); the KneeL had moved 0.1 cm since the aim was locked
- 100.27 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (2.6 cm); the KneeL had moved 0.7 cm since the aim was locked
- 102.68 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (2.6 cm); the KneeL had moved 0.3 cm since the aim was locked
- 105.68 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (2.4 cm); the KneeL had moved 0.9 cm since the aim was locked
- 108.88 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (2.7 cm); the KneeL had moved 0.1 cm since the aim was locked
- 111.90 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (2.8 cm); the KneeL had moved 0.7 cm since the aim was locked
- 115.28 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (2.1 cm); the KneeL had moved 6.3 cm since the aim was locked
- 117.42 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (2.8 cm); the KneeL had moved 0.6 cm since the aim was locked
- 120.08 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (2.4 cm); the KneeL had moved 0.9 cm since the aim was locked
- 122.52 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (1.9 cm); the KneeL had moved 20.7 cm since the aim was locked
- 124.72 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (3.1 cm); the KneeL had moved 2.8 cm since the aim was locked
- 127.62 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (2.6 cm); the KneeL had moved 0.5 cm since the aim was locked
- 130.57 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (2.8 cm); the KneeL had moved 0.4 cm since the aim was locked
- 133.87 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (2.8 cm); the KneeL had moved 0.7 cm since the aim was locked
- 136.95 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (2.6 cm); the KneeL had moved 0.4 cm since the aim was locked
- 139.42 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (4.3 cm); the KneeL had moved 34.0 cm since the aim was locked
- 142.63 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (0.0 cm); the KneeL had moved 33.5 cm since the aim was locked
- 145.72 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (4.6 cm); the KneeL had moved 13.5 cm since the aim was locked
- 148.02 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (2.6 cm); the KneeL had moved 0.4 cm since the aim was locked
- 151.12 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (2.7 cm); the KneeL had moved 0.1 cm since the aim was locked


## A Medium

Present 0.0–152.5 s. Steps 178, idle shuffles 39. Feet slid at most 5.6 cm while planted; the weight was at most 54 cm from the support point; hips dropped for reach by up to 78 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 1 / 38 |
| Chest | ? | 0.2 / 9.6 | 0 / 40 |
| Head | ? | 0.2 / 15.9 | 2 / 75 |
| UpperArmL | ? | 0.2 / 9.4 | 2 / 71 |
| ForearmL | ? | 0.4 / 15.3 | 13 / 138 |
| HandL | ? | 0.7 / 31.3 | 35 / 175 |
| UpperArmR | ? | 0.2 / 23.5 | 2 / 158 |
| ForearmR | ? | 0.6 / 34.5 | 6 / 175 |
| HandR | ? | 0.9 / 40.9 | 20 / 180 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Blows: 48. The posed striking part passed within 3 cm of the aim point in 48; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 47.4 cm, max 122.6 cm. 17 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.83 s: SwordBackhand at B Medium's Belly: passed 0.1 cm from the aim point; nearest part B Medium's Spine (0.0 cm); the Belly had moved 10.9 cm since the aim was locked
- 7.10 s: SwordBackhand at B Medium's Head: passed 0.1 cm from the aim point; nearest part B Medium's Head (19.7 cm); the Head had moved 23.2 cm since the aim was locked
- 10.68 s: SwordCut at B Medium's KneeR: passed 0.1 cm from the aim point; nearest part B Medium's ThighR (20.8 cm); the KneeR had moved 22.8 cm since the aim was locked
- 13.90 s: SwordCut at B Medium's Head: passed 0.1 cm from the aim point; nearest part B Medium's Head (15.0 cm); the Head had moved 24.6 cm since the aim was locked
- 16.70 s: SwordBackhand at B Medium's Belly: passed 0.1 cm from the aim point; nearest part B Medium's HandL (9.5 cm); the Belly had moved 10.6 cm since the aim was locked
- 19.53 s: SwordCut at B Medium's Chest: passed 0.1 cm from the aim point; nearest part B Medium's Chest (33.1 cm); the Chest had moved 54.6 cm since the aim was locked
- 23.17 s: SwordCut at B Medium's Belly: passed 0.1 cm from the aim point; nearest part B Medium's ForearmR (11.6 cm); the Belly had moved 10.2 cm since the aim was locked
- 25.93 s: SwordCut at B Medium's Neck: passed 0.1 cm from the aim point; nearest part B Large's UpperArmL (60.8 cm); the Neck had moved 88.8 cm since the aim was locked
- 29.35 s: SwordCut at B Medium's Belly: passed 0.1 cm from the aim point; nearest part B Medium's Spine (5.2 cm); the Belly had moved 1.4 cm since the aim was locked
- 32.42 s: SwordBackhand at B Medium's KneeL: passed 0.1 cm from the aim point; nearest part B Medium's ShinL (3.0 cm); the KneeL had moved 2.5 cm since the aim was locked
- 35.83 s: SwordCut at B Medium's Head: passed 0.1 cm from the aim point; nearest part B Medium's Head (0.6 cm); the Head had moved 1.7 cm since the aim was locked
- 39.35 s: SwordBackhand at B Medium's Neck: passed 0.1 cm from the aim point; nearest part B Medium's Head (8.0 cm); the Neck had moved 2.3 cm since the aim was locked
- 42.85 s: SwordBackhand at B Medium's Belly: passed 0.1 cm from the aim point; nearest part B Medium's Spine (1.4 cm); the Belly had moved 18.5 cm since the aim was locked
- 46.10 s: SwordBackhand at B Medium's Belly: passed 0.1 cm from the aim point; nearest part B Medium's HandL (3.5 cm); the Belly had moved 23.0 cm since the aim was locked
- 48.68 s: SwordCut at B Medium's Chest: passed 0.1 cm from the aim point; nearest part B Medium's HandL (45.4 cm); the Chest had moved 62.1 cm since the aim was locked
- 52.28 s: SwordThrust at B Medium's Chest: passed 0.0 cm from the aim point; nearest part B Medium's Chest (1.4 cm); the Chest had moved 1.2 cm since the aim was locked
- 55.85 s: SwordBackhand at B Medium's Neck: passed 0.1 cm from the aim point; nearest part B Medium's Chest (7.4 cm); the Neck had moved 1.4 cm since the aim was locked
- 59.47 s: SwordCut at B Medium's ElbowR: passed 0.1 cm from the aim point; nearest part B Medium's ForearmR (6.0 cm); the ElbowR had moved 1.1 cm since the aim was locked
- 61.97 s: SwordCut at B Medium's Neck: passed 0.1 cm from the aim point; nearest part B Medium's Chest (8.2 cm); the Neck had moved 1.9 cm since the aim was locked
- 64.83 s: SwordBackhand at B Medium's KneeR: passed 0.1 cm from the aim point; nearest part B Medium's ShinR (0.4 cm); the KneeR had moved 0.7 cm since the aim was locked
- 67.82 s: SwordCut at B Medium's Chest: passed 0.1 cm from the aim point; nearest part B Medium's Chest (1.0 cm); the Chest had moved 1.1 cm since the aim was locked
- 71.03 s: SwordCut at B Medium's Belly: passed 0.1 cm from the aim point; nearest part B Medium's Spine (4.0 cm); the Belly had moved 3.8 cm since the aim was locked
- 74.58 s: SwordBackhand at B Medium's Head: passed 0.1 cm from the aim point; nearest part B Medium's Head (0.0 cm); the Head had moved 1.5 cm since the aim was locked
- 77.15 s: SwordCut at B Medium's Belly: passed 0.1 cm from the aim point; nearest part B Medium's HandR (6.6 cm); the Belly had moved 25.3 cm since the aim was locked
- 80.37 s: SwordBackhand at B Medium's Chest: passed 0.1 cm from the aim point; nearest part B Medium's Chest (0.0 cm); the Chest had moved 1.0 cm since the aim was locked
- 82.92 s: SwordCut at B Medium's Neck: passed 0.1 cm from the aim point; nearest part A Large's HandL (37.4 cm); the Neck had moved 63.9 cm since the aim was locked
- 86.25 s: SwordThrust at B Medium's Belly: passed 0.0 cm from the aim point; nearest part B Medium's Spine (4.2 cm); the Belly had moved 1.1 cm since the aim was locked
- 88.92 s: SwordCut at B Medium's Head: passed 0.1 cm from the aim point; nearest part B Medium's Head (0.9 cm); the Head had moved 2.2 cm since the aim was locked
- 91.28 s: SwordThrust at B Medium's Neck: passed 0.0 cm from the aim point; nearest part B Medium's Chest (47.0 cm); the Neck had moved 57.5 cm since the aim was locked
- 94.25 s: SwordBackhand at B Medium's ElbowL: passed 0.1 cm from the aim point; nearest part B Medium's Pelvis (51.6 cm); the ElbowL had moved 59.8 cm since the aim was locked
- 97.45 s: SwordBackhand at B Medium's Chest: passed 0.1 cm from the aim point; nearest part B Medium's Chest (11.2 cm); the Chest had moved 31.0 cm since the aim was locked
- 100.70 s: SwordBackhand at B Medium's Chest: passed 0.1 cm from the aim point; nearest part B Medium's Chest (38.2 cm); the Chest had moved 60.7 cm since the aim was locked
- 104.03 s: SwordBackhand at B Medium's Neck: passed 0.1 cm from the aim point; nearest part B Medium's Chest (7.4 cm); the Neck had moved 13.2 cm since the aim was locked
- 106.83 s: SwordThrust at B Medium's Belly: passed 0.0 cm from the aim point; nearest part B Medium's HandL (13.4 cm); the Belly had moved 33.7 cm since the aim was locked
- 109.33 s: SwordThrust at B Medium's Chest: passed 0.0 cm from the aim point; nearest part B Medium's Chest (0.2 cm); the Chest had moved 1.5 cm since the aim was locked
- 112.78 s: SwordThrust at B Medium's Neck: passed 0.0 cm from the aim point; nearest part B Medium's Head (7.0 cm); the Neck had moved 1.3 cm since the aim was locked
- 115.20 s: SwordCut at B Medium's Belly: passed 0.1 cm from the aim point; nearest part B Medium's Pelvis (31.5 cm); the Belly had moved 54.4 cm since the aim was locked
- 118.33 s: SwordBackhand at B Medium's Chest: passed 0.1 cm from the aim point; nearest part B Medium's Chest (8.0 cm); the Chest had moved 8.2 cm since the aim was locked
- 121.58 s: SwordBackhand at B Medium's Neck: passed 0.1 cm from the aim point; nearest part B Medium's Chest (7.6 cm); the Neck had moved 4.4 cm since the aim was locked
- 125.07 s: SwordCut at B Medium's ElbowR: passed 0.1 cm from the aim point; nearest part B Medium's ForearmR (4.4 cm); the ElbowR had moved 1.0 cm since the aim was locked
- 127.95 s: SwordThrust at B Medium's Belly: passed 0.0 cm from the aim point; nearest part B Medium's Spine (5.9 cm); the Belly had moved 1.5 cm since the aim was locked
- 130.75 s: SwordBackhand at B Medium's Belly: passed 0.1 cm from the aim point; nearest part B Medium's Spine (5.3 cm); the Belly had moved 2.6 cm since the aim was locked
- 134.30 s: SwordThrust at B Medium's Neck: passed 0.0 cm from the aim point; nearest part B Medium's Head (5.4 cm); the Neck had moved 1.7 cm since the aim was locked
- 137.57 s: SwordThrust at B Medium's Chest: passed 0.0 cm from the aim point; nearest part B Medium's Chest (0.0 cm); the Chest had moved 1.3 cm since the aim was locked
- 141.20 s: SwordBackhand at B Medium's Head: passed 0.1 cm from the aim point; nearest part B Medium's Head (0.0 cm); the Head had moved 2.1 cm since the aim was locked
- 144.48 s: SwordCut at B Medium's KneeL: passed 0.1 cm from the aim point; nearest part B Medium's ShinL (2.6 cm); the KneeL had moved 1.4 cm since the aim was locked
- 147.70 s: SwordCut at B Medium's Neck: passed 0.1 cm from the aim point; nearest part B Medium's Chest (6.5 cm); the Neck had moved 3.6 cm since the aim was locked
- 151.40 s: SwordCut at B Medium's Neck: passed 0.1 cm from the aim point; nearest part B Medium's Chest (7.5 cm); the Neck had moved 2.4 cm since the aim was locked


## A Large

Present 0.0–152.5 s. Steps 111, idle shuffles 20. Feet slid at most 0.0 cm while planted; the weight was at most 45 cm from the support point; hips dropped for reach by up to 30 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 1 / 40 |
| Chest | ? | 0.3 / 11.3 | 1 / 37 |
| Head | ? | 0.4 / 19.2 | 5 / 121 |
| UpperArmL | ? | 0.4 / 15.2 | 2 / 39 |
| ForearmL | ? | 1.1 / 14.8 | 3 / 41 |
| HandL | ? | 1.8 / 19.4 | 7 / 91 |
| UpperArmR | ? | 0.5 / 13.0 | 5 / 180 |
| ForearmR | ? | 1.3 / 45.4 | 10 / 178 |
| HandR | ? | 2.1 / 44.1 | 38 / 180 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Blows: 45. The posed striking part passed within 3 cm of the aim point in 45; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 75.1 cm, max 122.9 cm. 11 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.13 s: ClubSwing at B Medium's Chest: passed 0.0 cm from the aim point; nearest part B Large's ForearmL (19.5 cm); the Chest had moved 51.3 cm since the aim was locked
- 7.72 s: ClubSwing at B Small's Head: passed 0.0 cm from the aim point; nearest part B Small's Head (8.6 cm); the Head had moved 17.5 cm since the aim was locked
- 10.98 s: ClubSmash at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (2.3 cm); the Neck had moved 2.0 cm since the aim was locked
- 14.77 s: ClubSwing at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (3.4 cm); the Neck had moved 2.3 cm since the aim was locked
- 17.88 s: ClubSmash at B Small's Head: passed 0.0 cm from the aim point; nearest part B Small's Head (9.3 cm); the Head had moved 21.7 cm since the aim was locked
- 20.52 s: ClubSwing at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's HandR (2.0 cm); the Belly had moved 3.2 cm since the aim was locked
- 24.12 s: ClubSmash at B Small's ElbowL: passed 0.0 cm from the aim point; nearest part B Small's Chest (9.5 cm); the ElbowL had moved 24.6 cm since the aim was locked
- 27.67 s: ClubSmash at B Small's Head: passed 0.0 cm from the aim point; nearest part B Small's Head (0.0 cm); the Head had moved 2.9 cm since the aim was locked
- 30.73 s: ClubSmash at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (1.8 cm); the Neck had moved 3.8 cm since the aim was locked
- 34.55 s: ClubSwing at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (2.8 cm); the Neck had moved 1.5 cm since the aim was locked
- 38.02 s: ClubSwing at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's UpperArmR (2.1 cm); the Belly had moved 2.4 cm since the aim was locked
- 41.45 s: ClubSmash at B Small's ElbowR: passed 0.0 cm from the aim point; nearest part B Small's UpperArmR (4.0 cm); the ElbowR had moved 15.6 cm since the aim was locked
- 44.55 s: ClubSwing at B Small's Head: passed 0.0 cm from the aim point; nearest part B Small's Head (7.4 cm); the Head had moved 18.0 cm since the aim was locked
- 47.83 s: ClubSwing at B Small's KneeR: passed 0.0 cm from the aim point; nearest part B Small's ThighR (6.3 cm); the KneeR had moved 1.2 cm since the aim was locked
- 50.42 s: ClubSwing at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (3.0 cm); the Neck had moved 2.1 cm since the aim was locked
- 52.98 s: ClubSwing at B Small's Head: passed 0.0 cm from the aim point; nearest part B Small's Head (0.8 cm); the Head had moved 1.6 cm since the aim was locked
- 55.88 s: ClubSwing at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (3.2 cm); the Neck had moved 1.5 cm since the aim was locked
- 59.65 s: ClubSmash at B Small's ElbowR: passed 0.0 cm from the aim point; nearest part B Small's UpperArmR (6.6 cm); the ElbowR had moved 7.0 cm since the aim was locked
- 62.98 s: ClubSwing at B Small's ElbowL: passed 0.0 cm from the aim point; nearest part B Small's HandR (4.8 cm); the ElbowL had moved 2.9 cm since the aim was locked
- 65.98 s: ClubSwing at B Small's Chest: passed 0.0 cm from the aim point; nearest part B Small's Chest (1.6 cm); the Chest had moved 1.1 cm since the aim was locked
- 69.58 s: ClubSmash at B Small's ElbowR: passed 0.0 cm from the aim point; nearest part B Small's ForearmR (3.0 cm); the ElbowR had moved 2.6 cm since the aim was locked
- 72.97 s: ClubSwing at B Small's Chest: passed 0.0 cm from the aim point; nearest part B Small's Chest (4.1 cm); the Chest had moved 7.3 cm since the aim was locked
- 76.85 s: ClubSmash at B Small's ElbowR: passed 0.0 cm from the aim point; nearest part B Small's HandR (32.7 cm); the ElbowR had moved 50.5 cm since the aim was locked
- 79.58 s: ClubSwing at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's HandL (34.0 cm); the Neck had moved 38.8 cm since the aim was locked
- 83.45 s: ClubSmash at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (2.3 cm); the Neck had moved 3.8 cm since the aim was locked
- 87.25 s: ClubSwing at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (24.1 cm); the Neck had moved 30.7 cm since the aim was locked
- 89.88 s: ClubSwing at B Small's Head: passed 0.0 cm from the aim point; nearest part B Small's Head (5.1 cm); the Head had moved 7.9 cm since the aim was locked
- 93.27 s: ClubSwing at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's UpperArmR (17.9 cm); the Belly had moved 33.8 cm since the aim was locked
- 96.30 s: ClubSwing at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (2.9 cm); the Neck had moved 3.2 cm since the aim was locked
- 100.02 s: ClubSwing at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's HandR (29.4 cm); the Belly had moved 34.6 cm since the aim was locked
- 103.27 s: ClubSmash at B Small's Head: passed 0.0 cm from the aim point; nearest part B Small's Head (17.7 cm); the Head had moved 21.8 cm since the aim was locked
- 106.93 s: ClubSmash at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (2.3 cm); the Neck had moved 1.4 cm since the aim was locked
- 110.37 s: ClubSmash at B Small's Head: passed 0.0 cm from the aim point; nearest part B Small's Chest (6.9 cm); the Head had moved 15.0 cm since the aim was locked
- 113.92 s: ClubSwing at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (2.6 cm); the Neck had moved 1.1 cm since the aim was locked
- 117.02 s: ClubSmash at B Small's Head: passed 0.0 cm from the aim point; nearest part B Small's Head (0.4 cm); the Head had moved 3.3 cm since the aim was locked
- 120.60 s: ClubSwing at B Small's KneeL: passed 0.0 cm from the aim point; nearest part B Small's ThighL (4.5 cm); the KneeL had moved 1.1 cm since the aim was locked
- 123.52 s: ClubSmash at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (1.5 cm); the Neck had moved 0.9 cm since the aim was locked
- 126.95 s: ClubSwing at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's Pelvis (4.9 cm); the Belly had moved 7.4 cm since the aim was locked
- 130.70 s: ClubSmash at B Small's ElbowR: passed 0.0 cm from the aim point; nearest part B Small's ForearmR (2.8 cm); the ElbowR had moved 1.8 cm since the aim was locked
- 134.20 s: ClubSwing at B Small's Head: passed 0.0 cm from the aim point; nearest part B Small's Head (0.0 cm); the Head had moved 2.4 cm since the aim was locked
- 136.80 s: ClubSmash at B Small's Head: passed 0.0 cm from the aim point; nearest part B Small's Head (0.0 cm); the Head had moved 1.9 cm since the aim was locked
- 140.48 s: ClubSwing at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (3.6 cm); the Neck had moved 1.8 cm since the aim was locked
- 143.82 s: ClubSmash at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Chest (9.5 cm); the Neck had moved 17.0 cm since the aim was locked
- 147.43 s: ClubSmash at B Small's Chest: passed 0.0 cm from the aim point; nearest part B Small's Chest (5.0 cm); the Chest had moved 4.1 cm since the aim was locked
- 150.37 s: ClubSwing at B Small's Head: passed 0.0 cm from the aim point; nearest part B Small's Head (0.4 cm); the Head had moved 7.3 cm since the aim was locked


## B Small

Present 0.0–152.5 s. Steps 253, idle shuffles 67. Feet slid at most 7.4 cm while planted; the weight was at most 23 cm from the support point; hips dropped for reach by up to 30 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 3.1 | 1 / 88 |
| Chest | ? | 0.1 / 8.0 | 1 / 126 |
| Head | ? | 0.2 / 16.3 | 2 / 150 |
| UpperArmL | ? | 0.2 / 13.0 | 2 / 66 |
| ForearmL | ? | 0.5 / 12.4 | 3 / 106 |
| HandL | ? | 0.6 / 10.0 | 6 / 120 |
| UpperArmR | ? | 0.2 / 25.9 | 4 / 173 |
| ForearmR | ? | 0.5 / 40.4 | 8 / 174 |
| HandR | ? | 0.8 / 37.3 | 28 / 180 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Blows: 51. The posed striking part passed within 3 cm of the aim point in 51; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 19.6 cm, max 46.8 cm. 20 reached the part they were aimed at (nearest part within 5 cm at contact).

- 9.58 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (2.6 cm); the KneeL had moved 0.3 cm since the aim was locked
- 12.62 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (2.6 cm); the KneeL had moved 0.5 cm since the aim was locked
- 15.12 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (24.4 cm); the KneeL had moved 30.8 cm since the aim was locked
- 17.92 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (0.0 cm); the KneeL had moved 22.9 cm since the aim was locked
- 20.77 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's HandR (36.8 cm); the KneeL had moved 41.4 cm since the aim was locked
- 24.03 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (4.3 cm); the KneeL had moved 20.1 cm since the aim was locked
- 26.32 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (1.2 cm); the KneeL had moved 4.4 cm since the aim was locked
- 29.27 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (2.7 cm); the KneeL had moved 0.3 cm since the aim was locked
- 32.37 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (2.8 cm); the KneeL had moved 0.5 cm since the aim was locked
- 35.77 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (2.8 cm); the KneeL had moved 0.6 cm since the aim was locked
- 38.27 s: ClubSwing at A Large's KneeR: passed 0.0 cm from the aim point; nearest part A Large's ShinR (11.7 cm); the KneeR had moved 17.7 cm since the aim was locked
- 40.93 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (3.1 cm); the KneeL had moved 2.9 cm since the aim was locked
- 44.42 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (1.1 cm); the KneeL had moved 2.8 cm since the aim was locked
- 46.80 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (2.8 cm); the KneeL had moved 0.7 cm since the aim was locked
- 48.97 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (2.8 cm); the KneeL had moved 0.6 cm since the aim was locked
- 51.27 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (2.8 cm); the KneeL had moved 1.1 cm since the aim was locked
- 53.73 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (2.7 cm); the KneeL had moved 0.5 cm since the aim was locked
- 56.38 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (12.2 cm); the KneeL had moved 18.1 cm since the aim was locked
- 59.80 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (35.7 cm); the KneeL had moved 68.9 cm since the aim was locked
- 62.08 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (2.7 cm); the KneeL had moved 0.4 cm since the aim was locked
- 64.98 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (2.5 cm); the KneeL had moved 0.6 cm since the aim was locked
- 67.38 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (2.4 cm); the KneeL had moved 0.9 cm since the aim was locked
- 70.38 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (2.7 cm); the KneeL had moved 0.3 cm since the aim was locked
- 73.17 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (7.0 cm); the KneeL had moved 8.9 cm since the aim was locked
- 76.17 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (2.6 cm); the KneeL had moved 0.5 cm since the aim was locked
- 79.03 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (2.8 cm); the KneeL had moved 0.7 cm since the aim was locked
- 82.20 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (2.8 cm); the KneeL had moved 0.8 cm since the aim was locked
- 84.90 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (2.3 cm); the KneeL had moved 1.0 cm since the aim was locked
- 87.20 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (0.0 cm); the KneeL had moved 22.3 cm since the aim was locked
- 90.07 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (0.0 cm); the KneeL had moved 3.3 cm since the aim was locked
- 93.20 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (24.4 cm); the KneeL had moved 54.2 cm since the aim was locked
- 96.05 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (2.2 cm); the KneeL had moved 8.2 cm since the aim was locked
- 99.45 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (2.3 cm); the KneeL had moved 0.9 cm since the aim was locked
- 102.68 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (2.7 cm); the KneeL had moved 0.2 cm since the aim was locked
- 105.10 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (2.5 cm); the KneeL had moved 0.6 cm since the aim was locked
- 107.23 s: ClubSwing at A Large's KneeR: passed 0.0 cm from the aim point; nearest part A Large's ShinR (18.4 cm); the KneeR had moved 25.6 cm since the aim was locked
- 109.93 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (0.1 cm); the KneeL had moved 4.3 cm since the aim was locked
- 113.12 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (2.8 cm); the KneeL had moved 0.3 cm since the aim was locked
- 115.80 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (2.8 cm); the KneeL had moved 0.7 cm since the aim was locked
- 118.12 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (2.8 cm); the KneeL had moved 0.1 cm since the aim was locked
- 120.93 s: ClubSwing at A Large's KneeR: passed 0.0 cm from the aim point; nearest part A Large's ShinR (29.8 cm); the KneeR had moved 37.0 cm since the aim was locked
- 123.90 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (38.4 cm); the KneeL had moved 45.3 cm since the aim was locked
- 127.18 s: ClubSwing at A Large's KneeR: passed 0.0 cm from the aim point; nearest part A Large's ShinR (6.6 cm); the KneeR had moved 11.1 cm since the aim was locked
- 130.42 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (6.3 cm); the KneeL had moved 14.2 cm since the aim was locked
- 133.28 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (2.8 cm); the KneeL had moved 0.6 cm since the aim was locked
- 135.58 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (2.8 cm); the KneeL had moved 0.7 cm since the aim was locked
- 138.20 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (2.8 cm); the KneeL had moved 0.2 cm since the aim was locked
- 140.75 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (22.1 cm); the KneeL had moved 27.7 cm since the aim was locked
- 143.67 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (2.6 cm); the KneeL had moved 2.8 cm since the aim was locked
- 147.07 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (0.0 cm); the KneeL had moved 6.6 cm since the aim was locked
- 150.20 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (2.8 cm); the KneeL had moved 1.1 cm since the aim was locked


## B Medium

Present 0.0–152.5 s. Steps 188, idle shuffles 39. Feet slid at most 9.2 cm while planted; the weight was at most 51 cm from the support point; hips dropped for reach by up to 78 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 11.0 | 1 / 89 |
| Chest | ? | 0.2 / 24.0 | 1 / 45 |
| Head | ? | 0.2 / 30.8 | 2 / 101 |
| UpperArmL | ? | 0.2 / 31.6 | 2 / 104 |
| ForearmL | ? | 0.4 / 21.2 | 13 / 142 |
| HandL | ? | 0.7 / 19.9 | 35 / 165 |
| UpperArmR | ? | 0.2 / 35.6 | 2 / 175 |
| ForearmR | ? | 0.7 / 77.9 | 7 / 166 |
| HandR | ? | 1.0 / 108.4 | 20 / 180 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Blows: 50. The posed striking part passed within 3 cm of the aim point in 50; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 51.2 cm, max 132.1 cm. 24 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.58 s: SwordThrust at A Medium's Neck: passed 0.0 cm from the aim point; nearest part A Medium's Head (5.6 cm); the Neck had moved 7.6 cm since the aim was locked
- 6.55 s: SwordThrust at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Chest (1.8 cm); the Chest had moved 1.1 cm since the aim was locked
- 10.10 s: SwordCut at A Medium's Belly: passed 0.1 cm from the aim point; nearest part A Medium's Spine (5.3 cm); the Belly had moved 2.0 cm since the aim was locked
- 13.65 s: SwordCut at A Medium's Belly: passed 0.1 cm from the aim point; nearest part A Medium's Chest (11.0 cm); the Belly had moved 12.0 cm since the aim was locked
- 16.87 s: SwordThrust at A Medium's Head: passed 0.0 cm from the aim point; nearest part A Medium's Chest (40.4 cm); the Head had moved 63.4 cm since the aim was locked
- 19.40 s: SwordCut at A Medium's Neck: passed 0.1 cm from the aim point; nearest part A Medium's Chest (7.0 cm); the Neck had moved 5.7 cm since the aim was locked
- 22.58 s: SwordThrust at A Medium's Neck: passed 0.0 cm from the aim point; nearest part A Medium's Head (5.8 cm); the Neck had moved 0.7 cm since the aim was locked
- 25.95 s: SwordCut at A Medium's KneeR: passed 0.1 cm from the aim point; nearest part A Medium's ShinR (35.8 cm); the KneeR had moved 44.1 cm since the aim was locked
- 28.40 s: SwordBackhand at A Medium's KneeR: passed 0.1 cm from the aim point; nearest part A Medium's ShinR (0.9 cm); the KneeR had moved 0.8 cm since the aim was locked
- 31.40 s: SwordCut at A Medium's Belly: passed 0.1 cm from the aim point; nearest part A Medium's Spine (8.4 cm); the Belly had moved 2.9 cm since the aim was locked
- 34.40 s: SwordCut at A Medium's Neck: passed 0.1 cm from the aim point; nearest part A Medium's Chest (7.5 cm); the Neck had moved 1.2 cm since the aim was locked
- 37.02 s: SwordBackhand at A Medium's Belly: passed 0.1 cm from the aim point; nearest part A Medium's Spine (1.4 cm); the Belly had moved 2.8 cm since the aim was locked
- 40.10 s: SwordBackhand at A Medium's Chest: passed 0.1 cm from the aim point; nearest part A Medium's Chest (0.4 cm); the Chest had moved 1.0 cm since the aim was locked
- 42.62 s: SwordBackhand at A Medium's Neck: passed 0.1 cm from the aim point; nearest part A Medium's Head (6.6 cm); the Neck had moved 13.7 cm since the aim was locked
- 45.57 s: SwordThrust at A Medium's Head: passed 0.0 cm from the aim point; nearest part A Medium's Head (0.0 cm); the Head had moved 1.5 cm since the aim was locked
- 48.28 s: SwordCut at A Medium's Head: passed 0.1 cm from the aim point; nearest part A Medium's Head (0.0 cm); the Head had moved 1.0 cm since the aim was locked
- 50.63 s: SwordBackhand at A Medium's Neck: passed 0.1 cm from the aim point; nearest part A Medium's Chest (7.5 cm); the Neck had moved 3.9 cm since the aim was locked
- 53.95 s: SwordThrust at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Chest (0.0 cm); the Chest had moved 1.8 cm since the aim was locked
- 56.50 s: SwordThrust at A Medium's Neck: passed 0.0 cm from the aim point; nearest part A Medium's Head (6.0 cm); the Neck had moved 2.8 cm since the aim was locked
- 60.05 s: SwordBackhand at A Medium's Chest: passed 0.1 cm from the aim point; nearest part A Medium's HandL (22.7 cm); the Chest had moved 44.2 cm since the aim was locked
- 63.63 s: SwordBackhand at A Medium's KneeL: passed 0.1 cm from the aim point; nearest part A Medium's ShinL (3.4 cm); the KneeL had moved 2.8 cm since the aim was locked
- 66.67 s: SwordBackhand at A Medium's Neck: passed 0.1 cm from the aim point; nearest part A Medium's Chest (8.3 cm); the Neck had moved 0.5 cm since the aim was locked
- 70.30 s: SwordBackhand at A Medium's KneeL: passed 0.1 cm from the aim point; nearest part A Medium's ShinL (3.6 cm); the KneeL had moved 2.0 cm since the aim was locked
- 73.77 s: SwordBackhand at A Medium's KneeL: passed 0.1 cm from the aim point; nearest part A Medium's ShinL (1.1 cm); the KneeL had moved 1.7 cm since the aim was locked
- 76.77 s: SwordThrust at A Medium's Head: passed 0.0 cm from the aim point; nearest part A Medium's Head (0.0 cm); the Head had moved 1.4 cm since the aim was locked
- 79.18 s: SwordBackhand at A Medium's Head: passed 0.1 cm from the aim point; nearest part A Medium's Head (0.0 cm); the Head had moved 1.2 cm since the aim was locked
- 82.38 s: SwordCut at A Medium's ElbowR: passed 0.1 cm from the aim point; nearest part A Medium's UpperArmR (7.5 cm); the ElbowR had moved 3.3 cm since the aim was locked
- 85.57 s: SwordBackhand at A Medium's Head: passed 0.1 cm from the aim point; nearest part A Medium's Head (0.0 cm); the Head had moved 2.1 cm since the aim was locked
- 88.13 s: SwordBackhand at A Medium's KneeL: passed 0.1 cm from the aim point; nearest part A Medium's ShinL (3.3 cm); the KneeL had moved 1.2 cm since the aim was locked
- 91.25 s: SwordThrust at A Medium's Neck: passed 0.0 cm from the aim point; nearest part A Medium's Chest (18.9 cm); the Neck had moved 28.4 cm since the aim was locked
- 94.20 s: SwordBackhand at A Medium's Belly: passed 0.1 cm from the aim point; nearest part A Medium's Pelvis (22.0 cm); the Belly had moved 43.0 cm since the aim was locked
- 97.45 s: SwordThrust at A Medium's Belly: passed 0.0 cm from the aim point; nearest part A Medium's Pelvis (31.3 cm); the Belly had moved 52.4 cm since the aim was locked
- 100.67 s: SwordBackhand at A Medium's Chest: passed 0.1 cm from the aim point; nearest part A Medium's Chest (24.9 cm); the Chest had moved 46.7 cm since the aim was locked
- 104.17 s: SwordThrust at A Medium's Belly: passed 0.0 cm from the aim point; nearest part A Medium's Pelvis (44.1 cm); the Belly had moved 68.6 cm since the aim was locked
- 106.42 s: SwordThrust at A Medium's Belly: passed 0.0 cm from the aim point; nearest part A Medium's Spine (4.0 cm); the Belly had moved 1.3 cm since the aim was locked
- 108.75 s: SwordThrust at A Medium's Neck: passed 0.0 cm from the aim point; nearest part A Medium's Head (6.9 cm); the Neck had moved 3.7 cm since the aim was locked
- 112.15 s: SwordCut at A Medium's ElbowR: passed 0.1 cm from the aim point; nearest part A Medium's ForearmR (6.8 cm); the ElbowR had moved 2.0 cm since the aim was locked
- 115.08 s: SwordBackhand at A Medium's Chest: passed 0.1 cm from the aim point; nearest part A Medium's Chest (0.0 cm); the Chest had moved 11.6 cm since the aim was locked
- 117.98 s: SwordThrust at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Chest (0.0 cm); the Chest had moved 2.2 cm since the aim was locked
- 120.83 s: SwordBackhand at A Medium's Belly: passed 0.1 cm from the aim point; nearest part A Medium's Spine (4.9 cm); the Belly had moved 2.4 cm since the aim was locked
- 123.98 s: SwordCut at A Medium's Neck: passed 0.1 cm from the aim point; nearest part A Medium's Chest (8.3 cm); the Neck had moved 4.1 cm since the aim was locked
- 127.22 s: SwordThrust at A Medium's Belly: passed 0.0 cm from the aim point; nearest part A Medium's Spine (6.6 cm); the Belly had moved 1.8 cm since the aim was locked
- 129.73 s: SwordBackhand at A Medium's Chest: passed 0.1 cm from the aim point; nearest part A Medium's Chest (0.0 cm); the Chest had moved 0.7 cm since the aim was locked
- 132.67 s: SwordThrust at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Chest (1.3 cm); the Chest had moved 0.8 cm since the aim was locked
- 136.28 s: SwordBackhand at A Medium's Belly: passed 0.1 cm from the aim point; nearest part A Medium's Spine (3.2 cm); the Belly had moved 1.0 cm since the aim was locked
- 139.43 s: SwordCut at A Medium's Head: passed 0.1 cm from the aim point; nearest part A Medium's Head (0.0 cm); the Head had moved 1.6 cm since the aim was locked
- 142.83 s: SwordThrust at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Chest (2.5 cm); the Chest had moved 1.7 cm since the aim was locked
- 145.40 s: SwordBackhand at A Medium's Head: passed 0.1 cm from the aim point; nearest part A Medium's Head (0.0 cm); the Head had moved 0.4 cm since the aim was locked
- 147.85 s: SwordBackhand at A Medium's Chest: passed 0.1 cm from the aim point; nearest part A Medium's Chest (29.8 cm); the Chest had moved 54.1 cm since the aim was locked
- 150.70 s: SwordCut at A Medium's Neck: passed 0.1 cm from the aim point; nearest part A Medium's Chest (8.2 cm); the Neck had moved 2.1 cm since the aim was locked


## B Large

Present 0.0–152.5 s. Steps 116, idle shuffles 19. Feet slid at most 0.0 cm while planted; the weight was at most 64 cm from the support point; hips dropped for reach by up to 65 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 1 / 53 |
| Chest | ? | 0.4 / 11.7 | 1 / 31 |
| Head | ? | 0.4 / 17.1 | 6 / 110 |
| UpperArmL | ? | 0.5 / 15.7 | 2 / 41 |
| ForearmL | ? | 1.2 / 14.3 | 3 / 50 |
| HandL | ? | 1.9 / 24.4 | 8 / 80 |
| UpperArmR | ? | 0.5 / 14.3 | 6 / 180 |
| ForearmR | ? | 1.4 / 60.5 | 10 / 180 |
| HandR | ? | 2.1 / 42.3 | 40 / 180 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Blows: 48. The posed striking part passed within 3 cm of the aim point in 48; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 74.8 cm, max 123.0 cm. 25 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.13 s: ClubSmash at A Medium's Head: passed 0.0 cm from the aim point; nearest part A Medium's Head (4.4 cm); the Head had moved 6.8 cm since the aim was locked
- 7.07 s: ClubSwing at A Small's KneeR: passed 0.0 cm from the aim point; nearest part A Small's ThighR (17.3 cm); the KneeR had moved 17.5 cm since the aim was locked
- 10.23 s: ClubSwing at A Small's Belly: passed 0.0 cm from the aim point; nearest part A Small's HandR (24.3 cm); the Belly had moved 29.5 cm since the aim was locked
- 13.15 s: ClubSwing at A Small's Head: passed 0.0 cm from the aim point; nearest part A Small's Head (3.8 cm); the Head had moved 3.8 cm since the aim was locked
- 16.63 s: ClubSwing at A Small's Neck: passed 0.0 cm from the aim point; nearest part A Small's Chest (3.3 cm); the Neck had moved 2.8 cm since the aim was locked
- 19.63 s: ClubSwing at A Small's Head: passed 0.0 cm from the aim point; nearest part A Small's Head (0.4 cm); the Head had moved 0.5 cm since the aim was locked
- 23.08 s: ClubSwing at A Small's Head: passed 0.0 cm from the aim point; nearest part A Small's Head (0.9 cm); the Head had moved 2.9 cm since the aim was locked
- 26.87 s: ClubSwing at A Small's KneeL: passed 0.0 cm from the aim point; nearest part A Small's ThighL (4.3 cm); the KneeL had moved 1.9 cm since the aim was locked
- 30.32 s: ClubSwing at A Small's KneeL: passed 0.0 cm from the aim point; nearest part A Small's ThighL (6.9 cm); the KneeL had moved 1.9 cm since the aim was locked
- 33.17 s: ClubSwing at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (1.8 cm); the Chest had moved 0.9 cm since the aim was locked
- 35.98 s: ClubSwing at A Small's Belly: passed 0.0 cm from the aim point; nearest part A Small's ForearmR (1.5 cm); the Belly had moved 0.4 cm since the aim was locked
- 38.92 s: ClubSmash at A Small's ElbowR: passed 0.0 cm from the aim point; nearest part A Small's ForearmR (2.1 cm); the ElbowR had moved 2.8 cm since the aim was locked
- 42.07 s: ClubSwing at A Small's KneeR: passed 0.0 cm from the aim point; nearest part A Small's ShinR (5.5 cm); the KneeR had moved 1.7 cm since the aim was locked
- 45.27 s: ClubSwing at A Small's Head: passed 0.0 cm from the aim point; nearest part A Small's Head (1.0 cm); the Head had moved 0.7 cm since the aim was locked
- 48.23 s: ClubSwing at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (1.8 cm); the Chest had moved 1.6 cm since the aim was locked
- 51.10 s: ClubSmash at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (0.0 cm); the Chest had moved 3.6 cm since the aim was locked
- 54.38 s: ClubSmash at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (1.0 cm); the Chest had moved 0.6 cm since the aim was locked
- 57.43 s: ClubSmash at A Small's Head: passed 0.0 cm from the aim point; nearest part A Small's Head (0.0 cm); the Head had moved 3.2 cm since the aim was locked
- 60.90 s: ClubSmash at A Small's ElbowL: passed 0.0 cm from the aim point; nearest part A Small's ForearmL (3.2 cm); the ElbowL had moved 7.5 cm since the aim was locked
- 63.85 s: ClubSmash at A Small's ElbowR: passed 0.0 cm from the aim point; nearest part A Small's Chest (10.5 cm); the ElbowR had moved 30.0 cm since the aim was locked
- 66.47 s: ClubSmash at A Small's Neck: passed 0.0 cm from the aim point; nearest part A Small's Chest (1.2 cm); the Neck had moved 2.9 cm since the aim was locked
- 69.85 s: ClubSwing at A Small's Belly: passed 0.0 cm from the aim point; nearest part A Small's ForearmR (1.0 cm); the Belly had moved 2.7 cm since the aim was locked
- 72.83 s: ClubSwing at A Small's KneeR: passed 0.0 cm from the aim point; nearest part A Small's ShinR (6.2 cm); the KneeR had moved 1.8 cm since the aim was locked
- 75.37 s: ClubSwing at A Small's Head: passed 0.0 cm from the aim point; nearest part A Small's Chest (3.8 cm); the Head had moved 21.8 cm since the aim was locked
- 78.70 s: ClubSwing at A Small's Head: passed 0.0 cm from the aim point; nearest part A Small's Head (0.0 cm); the Head had moved 4.1 cm since the aim was locked
- 81.50 s: ClubSmash at A Small's ElbowL: passed 0.0 cm from the aim point; nearest part A Small's ShinL (2.1 cm); the ElbowL had moved 26.5 cm since the aim was locked
- 85.37 s: ClubSmash at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (0.0 cm); the Chest had moved 2.1 cm since the aim was locked
- 88.82 s: ClubSmash at A Small's Head: passed 0.0 cm from the aim point; nearest part A Small's Head (16.1 cm); the Head had moved 28.2 cm since the aim was locked
- 91.80 s: ClubSwing at A Small's Belly: passed 0.0 cm from the aim point; nearest part A Small's Pelvis (12.3 cm); the Belly had moved 4.4 cm since the aim was locked
- 95.25 s: ClubSwing at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Spine (13.6 cm); the Chest had moved 24.2 cm since the aim was locked
- 98.63 s: ClubSwing at A Small's Neck: passed 0.0 cm from the aim point; nearest part A Small's Chest (2.4 cm); the Neck had moved 1.0 cm since the aim was locked
- 101.25 s: ClubSmash at A Small's ElbowR: passed 0.0 cm from the aim point; nearest part A Small's ForearmR (1.4 cm); the ElbowR had moved 0.2 cm since the aim was locked
- 104.12 s: ClubSwing at A Small's Head: passed 0.0 cm from the aim point; nearest part A Small's Head (0.0 cm); the Head had moved 1.6 cm since the aim was locked
- 107.72 s: ClubSmash at A Small's Head: passed 0.0 cm from the aim point; nearest part A Small's Head (0.0 cm); the Head had moved 2.9 cm since the aim was locked
- 110.42 s: ClubSwing at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (2.0 cm); the Chest had moved 2.1 cm since the aim was locked
- 113.42 s: ClubSwing at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (2.3 cm); the Chest had moved 2.2 cm since the aim was locked
- 116.35 s: ClubSwing at A Small's Neck: passed 0.0 cm from the aim point; nearest part A Small's Chest (4.1 cm); the Neck had moved 2.3 cm since the aim was locked
- 118.93 s: ClubSwing at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (2.0 cm); the Chest had moved 2.2 cm since the aim was locked
- 122.58 s: ClubSmash at A Small's Neck: passed 0.0 cm from the aim point; nearest part A Small's Head (21.2 cm); the Neck had moved 27.4 cm since the aim was locked
- 126.30 s: ClubSmash at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (4.5 cm); the Chest had moved 3.4 cm since the aim was locked
- 129.40 s: ClubSwing at A Small's KneeL: passed 0.0 cm from the aim point; nearest part A Small's ThighL (5.4 cm); the KneeL had moved 1.3 cm since the aim was locked
- 132.58 s: ClubSmash at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (0.7 cm); the Chest had moved 2.0 cm since the aim was locked
- 135.78 s: ClubSmash at A Small's Head: passed 0.0 cm from the aim point; nearest part A Small's Head (0.0 cm); the Head had moved 5.4 cm since the aim was locked
- 139.23 s: ClubSmash at A Small's Neck: passed 0.0 cm from the aim point; nearest part A Small's Head (4.6 cm); the Neck had moved 5.4 cm since the aim was locked
- 142.63 s: ClubSwing at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (14.6 cm); the Chest had moved 28.7 cm since the aim was locked
- 145.88 s: ClubSwing at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (0.0 cm); the Chest had moved 8.7 cm since the aim was locked
- 148.75 s: ClubSmash at A Small's Neck: passed 0.0 cm from the aim point; nearest part A Small's Head (22.4 cm); the Neck had moved 28.5 cm since the aim was locked
- 152.00 s: ClubSmash at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (5.2 cm); the Chest had moved 3.9 cm since the aim was locked

