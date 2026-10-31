# Playtest 2026-10-31_09-40-05

Unity 6000.6.7f1, scene Sandbox, physics 60 Hz, solver 12/4.
Recorded 91.5 s of play (5490 physics steps), 1074 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and ghost), `world.csv.gz` (props), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_09-40-05`.

## Orc A (club)

Present 0.0–19.4 s. Time in states: Steady 19.4 s.
Hits taken 0 (0 damage). Catch steps 9, settle steps 6.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 2.8 | Orc B, 1.81 m | Recover | 16.3 m/s | 179° | 0.94 m (Recover) | missed: club passed 0.94 m from the target point (in Recover) |
| 2 | 8.4 | Orc B, 1.81 m | Recover | 14.9 m/s | 179° | 0.87 m (Recover) | missed: club passed 0.87 m from the target point (in Recover) |
| 3 | 13.5 | Orc B, 1.83 m | Recover | 15.8 m/s | 179° | 1.04 m (Recover) | missed: club passed 1.04 m from the target point (in Recover) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (12 s) | Attacking (7 s) |
| --- | --- | --- |
| Pelvis | 3 / 8 | 8 / 27 |
| Spine | 1 / 7 | 5 / 12 |
| Chest | 0 / 2 | 2 / 7 |
| Head | 1 / 11 | 7 / 27 |
| UpperArmL | 0 / 9 | 4 / 13 |
| ForearmL | 1 / 13 | 7 / 20 |
| HandL | 1 / 9 | 8 / 27 |
| UpperArmR | 1 / 7 | 70 / 180 |
| ForearmR | 4 / 90 | 24 / 48 |
| HandR | 4 / 46 | 121 / 180 |
| ThighL | 2 / 4 | 3 / 12 |
| ShinL | 4 / 13 | 9 / 67 |
| FootL | 3 / 11 | 9 / 36 |
| ThighR | 4 / 7 | 5 / 16 |
| ShinR | 3 / 8 | 6 / 15 |
| FootR | 7 / 13 | 9 / 20 |


## Orc B

Present 0.0–19.4 s. Time in states: Steady 19.4 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (19 s) |
| --- | --- |
| Pelvis | 0 / 0 |
| Spine | 0 / 0 |
| Chest | 0 / 1 |
| Head | 0 / 1 |
| UpperArmL | 0 / 9 |
| ForearmL | 0 / 13 |
| HandL | 0 / 5 |
| UpperArmR | 0 / 7 |
| ForearmR | 0 / 13 |
| HandR | 0 / 4 |
| ThighL | 1 / 1 |
| ShinL | 2 / 2 |
| FootL | 1 / 1 |
| ThighR | 1 / 1 |
| ShinR | 1 / 2 |
| FootR | 1 / 1 |


## Orc A (club) #2

Present 19.4–31.7 s. Time in states: Steady 12.3 s.
Hits taken 0 (0 damage). Catch steps 4, settle steps 3.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 20.4 | Orc B #2, 1.81 m | Recover | 15.7 m/s | 180° | 0.90 m (Recover) | missed: club passed 0.90 m from the target point (in Recover) |
| 2 | 30.8 | Orc B #2, 1.82 m | Load | 14.3 m/s | 0° | 1.88 m (Load) | abandoned: removed from the scene |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (9 s) | Attacking (3 s) |
| --- | --- | --- |
| Pelvis | 3 / 5 | 7 / 24 |
| Spine | 1 / 6 | 4 / 11 |
| Chest | 0 / 2 | 2 / 7 |
| Head | 1 / 8 | 7 / 25 |
| UpperArmL | 0 / 8 | 4 / 13 |
| ForearmL | 1 / 13 | 7 / 19 |
| HandL | 1 / 8 | 8 / 24 |
| UpperArmR | 1 / 8 | 83 / 178 |
| ForearmR | 2 / 90 | 27 / 48 |
| HandR | 3 / 42 | 117 / 178 |
| ThighL | 1 / 6 | 2 / 7 |
| ShinL | 4 / 12 | 7 / 24 |
| FootL | 4 / 7 | 8 / 29 |
| ThighR | 3 / 6 | 4 / 12 |
| ShinR | 3 / 12 | 6 / 12 |
| FootR | 8 / 13 | 8 / 19 |


## Orc B #2

Present 19.4–31.7 s. Time in states: Steady 12.3 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (12 s) |
| --- | --- |
| Pelvis | 0 / 0 |
| Spine | 0 / 1 |
| Chest | 0 / 1 |
| Head | 0 / 1 |
| UpperArmL | 0 / 8 |
| ForearmL | 1 / 13 |
| HandL | 0 / 5 |
| UpperArmR | 0 / 8 |
| ForearmR | 1 / 13 |
| HandR | 0 / 5 |
| ThighL | 1 / 6 |
| ShinL | 2 / 12 |
| FootL | 1 / 6 |
| ThighR | 1 / 6 |
| ShinR | 1 / 12 |
| FootR | 1 / 6 |


## Orc A (club) #3

Present 31.7–39.1 s. Time in states: Steady 7.3 s.
Hits taken 0 (0 damage). Catch steps 6, settle steps 3.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 32.5 | Orc B #3, 2.27 m | Recover | 29.4 m/s | 147° | 1.20 m (Follow) | missed: club passed 1.20 m from the target point (in Follow) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (5 s) | Attacking (2 s) |
| --- | --- | --- |
| Pelvis | 8 / 34 | 19 / 60 |
| Spine | 1 / 5 | 5 / 13 |
| Chest | 2 / 6 | 6 / 21 |
| Head | 2 / 15 | 13 / 37 |
| UpperArmL | 1 / 9 | 6 / 20 |
| ForearmL | 1 / 13 | 8 / 30 |
| HandL | 1 / 7 | 11 / 38 |
| UpperArmR | 1 / 23 | 67 / 173 |
| ForearmR | 4 / 90 | 29 / 88 |
| HandR | 4 / 35 | 83 / 176 |
| ThighL | 3 / 10 | 5 / 15 |
| ShinL | 4 / 14 | 11 / 24 |
| FootL | 4 / 18 | 6 / 28 |
| ThighR | 3 / 12 | 6 / 25 |
| ShinR | 8 / 14 | 9 / 24 |
| FootR | 4 / 30 | 7 / 39 |


## Orc B #3

Present 31.7–39.1 s. Time in states: Steady 7.3 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (7 s) |
| --- | --- |
| Pelvis | 0 / 0 |
| Spine | 0 / 0 |
| Chest | 0 / 1 |
| Head | 0 / 2 |
| UpperArmL | 0 / 9 |
| ForearmL | 1 / 13 |
| HandL | 0 / 5 |
| UpperArmR | 0 / 7 |
| ForearmR | 1 / 13 |
| HandR | 0 / 4 |
| ThighL | 1 / 1 |
| ShinL | 3 / 3 |
| FootL | 1 / 2 |
| ThighR | 1 / 1 |
| ShinR | 3 / 3 |
| FootR | 1 / 2 |


## Orc A (club) #4

Present 39.1–43.9 s. Time in states: Steady 4.8 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (5 s) |
| --- | --- |
| Pelvis | 1 / 1 |
| Spine | 0 / 1 |
| Chest | 0 / 1 |
| Head | 0 / 1 |
| UpperArmL | 1 / 8 |
| ForearmL | 0 / 12 |
| HandL | 1 / 4 |
| UpperArmR | 0 / 8 |
| ForearmR | 1 / 90 |
| HandR | 2 / 17 |
| ThighL | 1 / 5 |
| ShinL | 1 / 11 |
| FootL | 1 / 6 |
| ThighR | 0 / 5 |
| ShinR | 1 / 11 |
| FootR | 1 / 6 |


## Orc B #4

Present 39.1–43.9 s. Time in states: Steady 4.8 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (5 s) |
| --- | --- |
| Pelvis | 0 / 0 |
| Spine | 1 / 1 |
| Chest | 0 / 1 |
| Head | 0 / 1 |
| UpperArmL | 1 / 8 |
| ForearmL | 0 / 12 |
| HandL | 1 / 4 |
| UpperArmR | 0 / 9 |
| ForearmR | 1 / 13 |
| HandR | 1 / 5 |
| ThighL | 1 / 5 |
| ShinL | 2 / 11 |
| FootL | 1 / 6 |
| ThighR | 0 / 5 |
| ShinR | 1 / 11 |
| FootR | 1 / 5 |


## Orc A (club) #5

Present 43.9–50.6 s. Time in states: Steady 6.8 s.
Hits taken 0 (0 damage). Catch steps 6, settle steps 2.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 45.8 | Orc B #5, 1.05 m | Recover | 22.8 m/s | 178° | 0.11 m (Recover) | HIT Orc B #5 Head at 3.9 m/s (Follow) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (4 s) | Attacking (2 s) |
| --- | --- | --- |
| Pelvis | 4 / 17 | 14 / 36 |
| Spine | 2 / 11 | 5 / 14 |
| Chest | 0 / 1 | 2 / 6 |
| Head | 1 / 10 | 9 / 28 |
| UpperArmL | 1 / 7 | 6 / 17 |
| ForearmL | 1 / 13 | 8 / 23 |
| HandL | 1 / 12 | 9 / 30 |
| UpperArmR | 1 / 10 | 40 / 178 |
| ForearmR | 4 / 90 | 27 / 83 |
| HandR | 1 / 23 | 115 / 177 |
| ThighL | 2 / 8 | 4 / 8 |
| ShinL | 4 / 15 | 7 / 16 |
| FootL | 5 / 15 | 7 / 13 |
| ThighR | 3 / 10 | 9 / 19 |
| ShinR | 1 / 13 | 7 / 28 |
| FootR | 3 / 9 | 10 / 40 |


## Orc B #5

Present 43.9–50.6 s. Time in states: Steady 6.2 s, Flinch 0.6 s.
Hits taken 2 (12 damage). Catch steps 0, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (6 s) | Flinch (1 s) |
| --- | --- | --- |
| Pelvis | 0 / 4 | 2 / 4 |
| Spine | 1 / 2 | 2 / 3 |
| Chest | 0 / 5 | 1 / 5 |
| Head | 2 / 45 | 37 / 58 |
| UpperArmL | 0 / 7 | 1 / 2 |
| ForearmL | 1 / 13 | 1 / 2 |
| HandL | 0 / 4 | 1 / 2 |
| UpperArmR | 1 / 8 | 1 / 2 |
| ForearmR | 1 / 13 | 1 / 4 |
| HandR | 0 / 4 | 1 / 3 |
| ThighL | 1 / 2 | 1 / 2 |
| ShinL | 2 / 5 | 3 / 4 |
| FootL | 1 / 2 | 2 / 3 |
| ThighR | 1 / 2 | 2 / 3 |
| ShinR | 1 / 4 | 2 / 5 |
| FootR | 1 / 3 | 2 / 3 |


## Orc A (club) #6

Present 50.6–91.5 s. Time in states: Steady 40.9 s.
Hits taken 0 (0 damage). Catch steps 20, settle steps 8.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 51.1 | Orc B #6, 1.85 m | Recover | 16.7 m/s | 178° | 0.97 m (Recover) | missed: club passed 0.97 m from the target point (in Recover) |
| 2 | 55.8 | Orc B #6, 1.84 m | Recover | 17.3 m/s | 178° | 0.88 m (Recover) | missed: club passed 0.88 m from the target point (in Recover) |
| 3 | 59.4 | Orc B #6, 1.85 m | Recover | 15.7 m/s | 178° | 1.12 m (Recover) | missed: club passed 1.12 m from the target point (in Recover) |
| 4 | 73.6 | Orc B #6, 1.85 m | Recover | 15.9 m/s | 179° | 1.13 m (Recover) | missed: club passed 1.13 m from the target point (in Recover) |
| 5 | 78.5 | Orc B #6, 1.84 m | Recover | 17.4 m/s | 180° | 0.72 m (Recover) | missed: club passed 0.72 m from the target point (in Recover) |
| 6 | 86.2 | Orc B #6, 1.84 m | Recover | 16.5 m/s | 178° | 1.06 m (Recover) | missed: club passed 1.06 m from the target point (in Recover) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (26 s) | Attacking (14 s) |
| --- | --- | --- |
| Pelvis | 6 / 16 | 9 / 40 |
| Spine | 2 / 8 | 5 / 13 |
| Chest | 1 / 1 | 1 / 5 |
| Head | 1 / 9 | 7 / 19 |
| UpperArmL | 1 / 8 | 4 / 14 |
| ForearmL | 1 / 13 | 6 / 16 |
| HandL | 1 / 11 | 7 / 20 |
| UpperArmR | 2 / 17 | 76 / 179 |
| ForearmR | 1 / 90 | 19 / 53 |
| HandR | 4 / 40 | 118 / 180 |
| ThighL | 3 / 7 | 4 / 14 |
| ShinL | 7 / 14 | 8 / 29 |
| FootL | 4 / 16 | 5 / 25 |
| ThighR | 2 / 6 | 4 / 12 |
| ShinR | 5 / 17 | 7 / 39 |
| FootR | 6 / 17 | 7 / 40 |


## Orc B #6

Present 50.6–91.5 s. Time in states: Steady 40.9 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (41 s) |
| --- | --- |
| Pelvis | 0 / 1 |
| Spine | 0 / 1 |
| Chest | 0 / 1 |
| Head | 0 / 1 |
| UpperArmL | 0 / 8 |
| ForearmL | 0 / 13 |
| HandL | 0 / 14 |
| UpperArmR | 1 / 9 |
| ForearmR | 0 / 13 |
| HandR | 0 / 5 |
| ThighL | 1 / 6 |
| ShinL | 3 / 12 |
| FootL | 2 / 6 |
| ThighR | 1 / 6 |
| ShinR | 1 / 12 |
| FootR | 1 / 6 |


## Goblin

Present 55.8–91.5 s. Time in states: Steady 32.0 s, Knockdown 1.8 s, GettingUp 1.8 s.
Hits taken 0 (0 damage). Catch steps 3, settle steps 1.

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 59.4 | stability 22 < knockdownBelow 25 (lost 77.9) | Pelvis after 1.1 s | lying on its side | 0.51 / 0.51 m | stood up (pelvis 0.51 m, tilt 22° when the get-up ended) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (32 s) | Knockdown (2 s) | GettingUp (2 s) |
| --- | --- | --- | --- |
| Pelvis | 4 / 28 | 104 / 178 | 42 / 77 |
| Spine | 1 / 4 | 11 / 40 | 7 / 24 |
| Chest | 1 / 3 | 9 / 38 | 4 / 5 |
| Head | 0 / 5 | 22 / 83 | 6 / 32 |
| UpperArmL | 0 / 7 | 19 / 69 | 2 / 10 |
| ForearmL | 1 / 13 | 19 / 42 | 4 / 31 |
| HandL | 1 / 4 | 19 / 65 | 4 / 34 |
| UpperArmR | 1 / 10 | 25 / 64 | 20 / 73 |
| ForearmR | 0 / 13 | 13 / 25 | 18 / 52 |
| HandR | 1 / 5 | 16 / 72 | 30 / 53 |
| ThighL | 7 / 22 | 25 / 99 | 6 / 13 |
| ShinL | 1 / 8 | 13 / 107 | 8 / 50 |
| FootL | 5 / 23 | 19 / 80 | 25 / 72 |
| ThighR | 6 / 11 | 21 / 69 | 16 / 22 |
| ShinR | 7 / 13 | 18 / 61 | 17 / 52 |
| FootR | 5 / 13 | 20 / 85 | 20 / 85 |

