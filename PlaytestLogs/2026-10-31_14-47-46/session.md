# Playtest 2026-10-31_14-47-46

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 4.8 s of play (290 physics steps), 147 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_14-47-46`.

## A Small

Present 0.0–4.8 s. Steps 22, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 2 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.0 | 0 / 2 |
| Chest | ? | 0.0 / 0.2 | 0 / 4 |
| Head | ? | 0.1 / 0.5 | 1 / 3 |
| UpperArmL | ? | 0.1 / 0.4 | 0 / 2 |
| ForearmL | ? | 0.1 / 0.3 | 1 / 2 |
| HandL | ? | 0.2 / 0.3 | 2 / 4 |
| UpperArmR | ? | 0.1 / 0.9 | 1 / 73 |
| ForearmR | ? | 0.2 / 7.3 | 3 / 118 |
| HandR | ? | 0.4 / 18.0 | 16 / 108 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |


## A Medium

Present 0.0–4.8 s. Steps 10, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 14 cm from the support point; hips dropped for reach by up to 2 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 0 / 9 |
| Chest | ? | 0.1 / 2.6 | 1 / 21 |
| Head | ? | 0.2 / 5.7 | 3 / 26 |
| UpperArmL | ? | 0.2 / 4.9 | 2 / 40 |
| ForearmL | ? | 0.4 / 12.0 | 14 / 59 |
| HandL | ? | 0.9 / 20.3 | 34 / 114 |
| UpperArmR | ? | 0.3 / 9.5 | 6 / 135 |
| ForearmR | ? | 1.1 / 32.0 | 9 / 177 |
| HandR | ? | 1.5 / 23.4 | 15 / 174 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 37.7 cm, max 37.7 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.22 s: SwordCut at B Medium's Head: passed 0.1 cm from the aim point; nearest part B Large's HandR (2.5 cm); the Head had moved 82.7 cm since the aim was locked


## A Large

Present 0.0–4.8 s. Steps 11, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 26 cm from the support point; hips dropped for reach by up to 9 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 0 / 6 |
| Chest | ? | 0.2 / 2.3 | 0 / 3 |
| Head | ? | 0.2 / 3.0 | 1 / 9 |
| UpperArmL | ? | 0.2 / 2.2 | 1 / 11 |
| ForearmL | ? | 0.8 / 6.7 | 2 / 22 |
| HandL | ? | 1.3 / 16.5 | 4 / 29 |
| UpperArmR | ? | 0.2 / 3.3 | 2 / 51 |
| ForearmR | ? | 0.8 / 6.5 | 5 / 61 |
| HandR | ? | 1.5 / 41.6 | 29 / 180 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 56.9 cm, max 56.9 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.58 s: ClubSwing at B Medium's Neck: passed 0.0 cm from the aim point; nearest part B Medium's Chest (14.3 cm); the Neck had moved 26.9 cm since the aim was locked


## B Small

Present 0.0–4.8 s. Steps 22, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 2 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.0 | 0 / 2 |
| Chest | ? | 0.0 / 0.3 | 0 / 4 |
| Head | ? | 0.1 / 0.6 | 1 / 3 |
| UpperArmL | ? | 0.1 / 0.4 | 1 / 2 |
| ForearmL | ? | 0.1 / 0.4 | 1 / 3 |
| HandL | ? | 0.2 / 0.6 | 3 / 7 |
| UpperArmR | ? | 0.1 / 1.0 | 1 / 74 |
| ForearmR | ? | 0.2 / 7.9 | 3 / 113 |
| HandR | ? | 0.4 / 17.9 | 16 / 85 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |


## B Medium

Present 0.0–4.8 s. Steps 12, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 27 cm from the support point; hips dropped for reach by up to 6 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 1 / 9 |
| Chest | ? | 0.2 / 2.8 | 0 / 14 |
| Head | ? | 0.2 / 5.9 | 2 / 22 |
| UpperArmL | ? | 0.2 / 2.3 | 2 / 40 |
| ForearmL | ? | 0.5 / 13.8 | 13 / 59 |
| HandL | ? | 0.9 / 19.6 | 35 / 110 |
| UpperArmR | ? | 0.2 / 6.6 | 2 / 41 |
| ForearmR | ? | 0.6 / 4.8 | 4 / 62 |
| HandR | ? | 0.9 / 24.0 | 12 / 94 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 13.9 cm, max 13.9 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.70 s: SwordBackhand at A Medium's ElbowR: passed 0.1 cm from the aim point; nearest part A Medium's ForearmR (3.8 cm); the ElbowR had moved 1.7 cm since the aim was locked


## B Large

Present 0.0–4.8 s. Steps 11, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 26 cm from the support point; hips dropped for reach by up to 9 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.0 | 0 / 6 |
| Chest | ? | 0.2 / 2.6 | 0 / 3 |
| Head | ? | 0.2 / 3.3 | 1 / 12 |
| UpperArmL | ? | 0.2 / 2.2 | 1 / 15 |
| ForearmL | ? | 0.8 / 10.4 | 2 / 19 |
| HandL | ? | 1.5 / 20.3 | 4 / 31 |
| UpperArmR | ? | 0.2 / 3.8 | 2 / 52 |
| ForearmR | ? | 0.9 / 7.0 | 5 / 62 |
| HandR | ? | 1.5 / 41.6 | 28 / 174 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 56.1 cm, max 56.1 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.08 s: ClubSwing at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Chest (0.5 cm); the Chest had moved 7.5 cm since the aim was locked

