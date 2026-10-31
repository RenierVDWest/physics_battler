# Playtest 2026-10-31_17-22-28

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 7.6 s of play (459 physics steps), 241 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_17-22-28`.

## A Small

Present 0.0–7.6 s. Steps 24, idle shuffles 0. Feet slid at most 3.1 cm while planted; the weight was at most 8 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.4 / 29.7 | 0 / 24 |
| Spine | ? | 0.4 / 29.7 | 1 / 55 |
| Chest | ? | 0.6 / 34.3 | 1 / 48 |
| Head | ? | 0.7 / 43.4 | 4 / 72 |
| UpperArmL | ? | 0.6 / 33.7 | 2 / 64 |
| ForearmL | ? | 0.6 / 20.5 | 2 / 39 |
| HandL | ? | 0.8 / 30.1 | 5 / 42 |
| UpperArmR | ? | 0.8 / 48.6 | 1 / 68 |
| ForearmR | ? | 0.9 / 49.1 | 3 / 99 |
| HandR | ? | 0.8 / 38.5 | 6 / 123 |
| ThighL | ? | 0.4 / 29.3 | 1 / 83 |
| ShinL | ? | 0.1 / 7.1 | 0 / 44 |
| FootL | ? | 0.0 / 0.1 | 0 / 1 |
| ThighR | ? | 0.4 / 30.2 | 1 / 89 |
| ShinR | ? | 0.2 / 10.9 | 1 / 110 |
| FootR | ? | 0.1 / 6.0 | 0 / 20 |

Hits dealt: 1 (0 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 2, 71 damage. Health at the end 0.


## A Medium

Present 0.0–7.6 s. Steps 16, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 16 cm from the support point; hips dropped for reach by up to 2 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 2 / 44 |
| Chest | ? | 0.5 / 12.5 | 2 / 35 |
| Head | ? | 0.5 / 13.3 | 7 / 95 |
| UpperArmL | ? | 0.8 / 18.5 | 4 / 170 |
| ForearmL | ? | 0.9 / 13.5 | 3 / 59 |
| HandL | ? | 1.2 / 20.3 | 15 / 147 |
| UpperArmR | ? | 0.6 / 11.5 | 3 / 94 |
| ForearmR | ? | 0.7 / 12.7 | 3 / 60 |
| HandR | ? | 0.9 / 23.4 | 10 / 103 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 39 damage. Hits taken: 1, 41 damage. Health at the end 0.


## A Large

Present 0.0–7.6 s. Steps 15, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 23 cm from the support point; hips dropped for reach by up to 6 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 0 / 9 |
| Chest | Pinned | 0.2 / 3.8 | 0 / 6 |
| Head | Pinned | 0.2 / 2.7 | 3 / 41 |
| UpperArmL | Pinned | 0.3 / 6.1 | 3 / 44 |
| ForearmL | Pinned | 1.8 / 32.3 | 7 / 144 |
| HandL | Pinned | 3.3 / 71.4 | 16 / 164 |
| UpperArmR | Kinematic | 0.1 / 2.6 | 1 / 45 |
| ForearmR | Kinematic | 0.3 / 7.3 | 1 / 60 |
| HandR | Kinematic | 0.7 / 36.9 | 3 / 80 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (0 on the part aimed at, 0 blocked, 1 on someone in the way, 0 on allies), 32 damage. Hits taken: 2, 34 damage. Health at the end 186.


## B Small

Present 0.0–7.6 s. Steps 27, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 11 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 0 / 6 |
| Chest | Pinned | 0.1 / 1.2 | 0 / 4 |
| Head | Pinned | 0.1 / 0.8 | 1 / 13 |
| UpperArmL | Pinned | 0.1 / 1.4 | 1 / 46 |
| ForearmL | Pinned | 0.3 / 4.2 | 1 / 45 |
| HandL | Pinned | 0.4 / 15.7 | 4 / 50 |
| UpperArmR | Kinematic | 0.1 / 1.3 | 1 / 68 |
| ForearmR | Kinematic | 0.2 / 11.7 | 1 / 101 |
| HandR | Kinematic | 0.3 / 23.6 | 3 / 124 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 9 damage. Hits taken: 1, 16 damage. Health at the end 44.


## B Medium

Present 0.0–7.6 s. Steps 19, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 15 cm from the support point; hips dropped for reach by up to 2 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 0 / 7 |
| Chest | Pinned | 0.1 / 2.0 | 0 / 5 |
| Head | Pinned | 0.2 / 1.5 | 3 / 30 |
| UpperArmL | Pinned | 0.2 / 2.9 | 2 / 49 |
| ForearmL | Pinned | 0.9 / 23.0 | 4 / 85 |
| HandL | Pinned | 1.5 / 19.6 | 14 / 156 |
| UpperArmR | Kinematic | 0.1 / 1.5 | 1 / 41 |
| ForearmR | Kinematic | 0.3 / 4.8 | 1 / 62 |
| HandR | Kinematic | 0.5 / 24.0 | 4 / 66 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 71 damage. Hits taken: 2, 39 damage. Health at the end 61.


## B Large

Present 0.0–7.6 s. Steps 15, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 22 cm from the support point; hips dropped for reach by up to 4 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 1.0 | 1 / 49 |
| Chest | Pinned | 0.6 / 27.4 | 1 / 44 |
| Head | Pinned | 0.5 / 23.5 | 4 / 100 |
| UpperArmL | Pinned | 0.9 / 41.7 | 5 / 179 |
| ForearmL | Pinned | 1.8 / 27.0 | 5 / 62 |
| HandL | Pinned | 3.0 / 28.6 | 13 / 94 |
| UpperArmR | Pinned | 0.6 / 32.4 | 2 / 70 |
| ForearmR | Pinned | 0.9 / 36.8 | 3 / 89 |
| HandR | Pinned | 1.2 / 36.7 | 6 / 127 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (1 on the part aimed at, 0 blocked, 1 on someone in the way, 0 on allies), 66 damage. Hits taken: 1, 15 damage. Health at the end 205.

