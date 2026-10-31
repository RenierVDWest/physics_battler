# Playtest 2026-10-31_15-18-30

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 3.7 s of play (221 physics steps), 136 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_15-18-30`.

## A Small

Present 0.0–3.7 s. Steps 17, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 2 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.0 | 0 / 2 |
| Chest | Pinned | 0.0 / 0.3 | 0 / 4 |
| Head | Pinned | 0.1 / 0.6 | 1 / 3 |
| UpperArmL | Pinned | 0.1 / 0.4 | 0 / 2 |
| ForearmL | Pinned | 0.1 / 0.3 | 1 / 2 |
| HandL | Pinned | 0.2 / 0.4 | 2 / 4 |
| UpperArmR | Pinned | 0.1 / 1.0 | 1 / 71 |
| ForearmR | Pinned | 0.2 / 8.3 | 2 / 104 |
| HandR | Pinned | 0.4 / 17.3 | 11 / 78 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |


## A Medium

Present 0.0–3.7 s. Steps 10, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 4 cm from the support point; hips dropped for reach by up to 2 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 0 / 19 |
| Chest | ? | 0.1 / 3.6 | 1 / 32 |
| Head | ? | 0.3 / 7.7 | 3 / 38 |
| UpperArmL | ? | 0.3 / 9.8 | 2 / 40 |
| ForearmL | ? | 0.5 / 12.0 | 15 / 62 |
| HandL | ? | 1.0 / 20.3 | 36 / 114 |
| UpperArmR | ? | 0.4 / 14.6 | 3 / 115 |
| ForearmR | ? | 1.0 / 38.7 | 4 / 125 |
| HandR | ? | 1.1 / 23.4 | 9 / 121 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |


## A Large

Present 0.0–3.7 s. Steps 10, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 29 cm from the support point; hips dropped for reach by up to 6 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.0 | 1 / 27 |
| Chest | ? | 0.3 / 6.4 | 1 / 10 |
| Head | ? | 0.3 / 4.0 | 2 / 31 |
| UpperArmL | ? | 0.4 / 5.9 | 3 / 105 |
| ForearmL | ? | 1.0 / 18.0 | 5 / 130 |
| HandL | ? | 2.5 / 54.8 | 10 / 174 |
| UpperArmR | ? | 0.4 / 4.4 | 2 / 45 |
| ForearmR | ? | 1.0 / 7.3 | 8 / 60 |
| HandR | ? | 1.7 / 36.9 | 28 / 175 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.50 s: ClubSmash at B Large's Chest: passed 0.0 cm from the aim point; nearest part B Large's Chest (16.7 cm); the Chest had moved 56.5 cm since the aim was locked


## B Small

Present 0.0–3.7 s. Steps 17, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 2 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.0 | 0 / 2 |
| Chest | Pinned | 0.0 / 0.3 | 0 / 5 |
| Head | Pinned | 0.1 / 0.7 | 1 / 3 |
| UpperArmL | Pinned | 0.1 / 0.4 | 1 / 2 |
| ForearmL | Pinned | 0.1 / 0.4 | 1 / 3 |
| HandL | Pinned | 0.2 / 0.6 | 3 / 7 |
| UpperArmR | Pinned | 0.1 / 1.1 | 1 / 72 |
| ForearmR | Pinned | 0.2 / 9.0 | 2 / 106 |
| HandR | Pinned | 0.4 / 17.2 | 11 / 81 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |


## B Medium

Present 0.0–3.7 s. Steps 10, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 4 cm from the support point; hips dropped for reach by up to 2 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.0 | 0 / 1 |
| Chest | Pinned | 0.1 / 0.3 | 0 / 4 |
| Head | Pinned | 0.1 / 0.9 | 1 / 4 |
| UpperArmL | Pinned | 0.1 / 1.8 | 1 / 40 |
| ForearmL | Pinned | 0.4 / 13.8 | 14 / 59 |
| HandL | Pinned | 0.8 / 19.6 | 35 / 110 |
| UpperArmR | Pinned | 0.1 / 0.5 | 1 / 41 |
| ForearmR | Pinned | 0.3 / 4.8 | 2 / 62 |
| HandR | Pinned | 0.7 / 24.0 | 5 / 66 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |


## B Large

Present 0.0–3.7 s. Steps 10, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 31 cm from the support point; hips dropped for reach by up to 4 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.0 | 1 / 9 |
| Chest | Pinned | 0.2 / 2.1 | 0 / 3 |
| Head | Pinned | 0.2 / 2.5 | 2 / 11 |
| UpperArmL | Pinned | 0.3 / 2.1 | 1 / 27 |
| ForearmL | Pinned | 0.9 / 18.4 | 1 / 16 |
| HandL | Pinned | 1.4 / 19.8 | 4 / 22 |
| UpperArmR | Pinned | 0.3 / 3.1 | 2 / 45 |
| ForearmR | Pinned | 0.9 / 6.3 | 7 / 60 |
| HandR | Pinned | 1.7 / 36.7 | 25 / 104 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.32 s: ClubSmash at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Chest (0.0 cm); the Chest had moved 10.0 cm since the aim was locked

