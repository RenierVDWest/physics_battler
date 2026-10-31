# Playtest 2026-10-31_10-33-58

Unity 6000.6.7f1, scene Sandbox, physics 60 Hz, solver 12/4.
Recorded 26.9 s of play (1612 physics steps), 593 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and ghost), `world.csv.gz` (props), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_10-33-58`.

## Orc A (club)

Present 0.0–5.3 s. Time in states: Steady 5.3 s.
Hits taken 0 (0 damage). Catch steps 1, settle steps 0.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 3.3 | Orc B, 1.64 m | Strike | 15.9 m/s | 173° | 1.37 m (Strike) | abandoned: removed from the scene |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (3 s) | Attacking (2 s) |
| --- | --- | --- |
| Pelvis | 5 / 21 | 5 / 9 |
| Spine | 8 / 18 | 5 / 9 |
| Chest | 2 / 4 | 3 / 15 |
| Head | 1 / 29 | 2 / 6 |
| UpperArmL | 25 / 61 | 62 / 88 |
| ForearmL | 17 / 61 | 25 / 47 |
| HandL | 70 / 149 | 76 / 150 |
| UpperArmR | 17 / 36 | 73 / 114 |
| ForearmR | 64 / 111 | 40 / 92 |
| HandR | 113 / 175 | 115 / 151 |
| ThighL | 3 / 23 | 3 / 4 |
| ShinL | 7 / 26 | 13 / 18 |
| FootL | 6 / 17 | 12 / 17 |
| ThighR | 3 / 13 | 2 / 5 |
| ShinR | 9 / 35 | 10 / 13 |
| FootR | 7 / 30 | 3 / 8 |


## Orc B

Present 0.0–5.3 s. Time in states: Steady 5.3 s.
Hits taken 0 (0 damage). Catch steps 4, settle steps 4.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (5 s) |
| --- | --- |
| Pelvis | 9 / 11 |
| Spine | 2 / 3 |
| Chest | 1 / 1 |
| Head | 1 / 3 |
| UpperArmL | 1 / 9 |
| ForearmL | 1 / 13 |
| HandL | 1 / 5 |
| UpperArmR | 1 / 7 |
| ForearmR | 1 / 13 |
| HandR | 1 / 4 |
| ThighL | 2 / 27 |
| ShinL | 5 / 28 |
| FootL | 4 / 12 |
| ThighR | 3 / 10 |
| ShinR | 7 / 32 |
| FootR | 11 / 35 |


## Orc A (club) #2

Present 5.3–6.9 s. Time in states: Steady 1.6 s.
Hits taken 0 (0 damage). Catch steps 1, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (2 s) |
| --- | --- |
| Pelvis | 6 / 21 |
| Spine | 9 / 18 |
| Chest | 2 / 4 |
| Head | 3 / 28 |
| UpperArmL | 25 / 61 |
| ForearmL | 18 / 61 |
| HandL | 70 / 149 |
| UpperArmR | 17 / 36 |
| ForearmR | 66 / 111 |
| HandR | 113 / 175 |
| ThighL | 4 / 24 |
| ShinL | 6 / 28 |
| FootL | 5 / 17 |
| ThighR | 4 / 14 |
| ShinR | 9 / 37 |
| FootR | 8 / 30 |


## Orc B #2

Present 5.3–6.9 s. Time in states: Steady 1.6 s.
Hits taken 0 (0 damage). Catch steps 1, settle steps 1.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (2 s) |
| --- | --- |
| Pelvis | 9 / 11 |
| Spine | 2 / 3 |
| Chest | 1 / 1 |
| Head | 1 / 3 |
| UpperArmL | 1 / 8 |
| ForearmL | 1 / 13 |
| HandL | 1 / 5 |
| UpperArmR | 1 / 8 |
| ForearmR | 2 / 13 |
| HandR | 1 / 5 |
| ThighL | 3 / 28 |
| ShinL | 5 / 30 |
| FootL | 5 / 12 |
| ThighR | 4 / 10 |
| ShinR | 9 / 34 |
| FootR | 12 / 36 |


## Orc A (club) #3

Present 6.9–18.9 s. Time in states: Steady 12.0 s.
Hits taken 0 (0 damage). Catch steps 6, settle steps 1.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 13.0 | Orc B #3, 1.73 m | Recover | 16.0 m/s | 178° | 0.14 m (Follow) | HIT Orc B #3 HandR at 5.0 m/s (WindUp) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (8 s) | Attacking (4 s) |
| --- | --- | --- |
| Pelvis | 5 / 25 | 6 / 23 |
| Spine | 7 / 13 | 3 / 8 |
| Chest | 2 / 13 | 6 / 16 |
| Head | 3 / 30 | 5 / 13 |
| UpperArmL | 25 / 61 | 49 / 90 |
| ForearmL | 9 / 57 | 21 / 51 |
| HandL | 71 / 149 | 89 / 180 |
| UpperArmR | 14 / 36 | 50 / 113 |
| ForearmR | 40 / 85 | 26 / 70 |
| HandR | 122 / 175 | 109 / 179 |
| ThighL | 4 / 22 | 3 / 10 |
| ShinL | 9 / 25 | 12 / 20 |
| FootL | 4 / 19 | 5 / 12 |
| ThighR | 5 / 11 | 3 / 15 |
| ShinR | 16 / 34 | 18 / 28 |
| FootR | 8 / 32 | 7 / 25 |


## Orc B #3

Present 6.9–18.9 s. Time in states: Steady 9.9 s, Flinch 0.3 s, Stagger 1.8 s.
Hits taken 2 (26 damage). Catch steps 10, settle steps 7.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (10 s) | Flinch (0 s) | Stagger (2 s) |
| --- | --- | --- | --- |
| Pelvis | 9 / 12 | 9 / 12 | 13 / 22 |
| Spine | 2 / 3 | 2 / 3 | 2 / 4 |
| Chest | 2 / 2 | 3 / 4 | 2 / 3 |
| Head | 1 / 3 | 1 / 2 | 4 / 17 |
| UpperArmL | 1 / 9 | 1 / 1 | 2 / 13 |
| ForearmL | 1 / 17 | 1 / 2 | 2 / 18 |
| HandL | 1 / 19 | 1 / 2 | 3 / 20 |
| UpperArmR | 1 / 13 | 11 / 17 | 3 / 15 |
| ForearmR | 1 / 12 | 15 / 20 | 2 / 11 |
| HandR | 1 / 33 | 52 / 66 | 3 / 17 |
| ThighL | 4 / 27 | 3 / 5 | 4 / 7 |
| ShinL | 7 / 27 | 9 / 15 | 10 / 18 |
| FootL | 2 / 12 | 3 / 4 | 2 / 4 |
| ThighR | 3 / 9 | 4 / 5 | 4 / 10 |
| ShinR | 13 / 31 | 17 / 21 | 20 / 27 |
| FootR | 10 / 35 | 10 / 17 | 17 / 36 |


## Orc A (club) #4

Present 18.9–20.8 s. Time in states: Steady 1.9 s.
Hits taken 0 (0 damage). Catch steps 1, settle steps 1.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 19.9 | Orc B #4, 1.82 m | WindUp | 8.7 m/s | 0° | - | abandoned: removed from the scene |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (1 s) | Attacking (1 s) |
| --- | --- | --- |
| Pelvis | 7 / 20 | 4 / 6 |
| Spine | 11 / 13 | 8 / 12 |
| Chest | 3 / 15 | 4 / 9 |
| Head | 5 / 20 | 3 / 10 |
| UpperArmL | 25 / 61 | 37 / 80 |
| ForearmL | 16 / 52 | 19 / 43 |
| HandL | 73 / 149 | 72 / 84 |
| UpperArmR | 16 / 37 | 30 / 86 |
| ForearmR | 69 / 110 | 71 / 93 |
| HandR | 113 / 175 | 116 / 137 |
| ThighL | 4 / 24 | 2 / 6 |
| ShinL | 9 / 28 | 7 / 14 |
| FootL | 6 / 16 | 5 / 7 |
| ThighR | 9 / 13 | 9 / 11 |
| ShinR | 9 / 34 | 11 / 13 |
| FootR | 8 / 31 | 7 / 8 |


## Orc B #4

Present 18.9–20.8 s. Time in states: Steady 1.9 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (2 s) |
| --- | --- |
| Pelvis | 5 / 6 |
| Spine | 3 / 3 |
| Chest | 1 / 2 |
| Head | 1 / 2 |
| UpperArmL | 1 / 7 |
| ForearmL | 1 / 12 |
| HandL | 1 / 4 |
| UpperArmR | 1 / 9 |
| ForearmR | 1 / 13 |
| HandR | 1 / 5 |
| ThighL | 5 / 27 |
| ShinL | 10 / 30 |
| FootL | 15 / 15 |
| ThighR | 17 / 18 |
| ShinR | 8 / 32 |
| FootR | 9 / 35 |


## Orc A (club) #5

Present 20.8–22.1 s. Time in states: Steady 1.3 s.
Hits taken 0 (0 damage). Catch steps 1, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (1 s) |
| --- | --- |
| Pelvis | 8 / 21 |
| Spine | 4 / 16 |
| Chest | 2 / 6 |
| Head | 4 / 29 |
| UpperArmL | 26 / 61 |
| ForearmL | 15 / 60 |
| HandL | 72 / 149 |
| UpperArmR | 18 / 36 |
| ForearmR | 69 / 111 |
| HandR | 113 / 175 |
| ThighL | 3 / 22 |
| ShinL | 8 / 24 |
| FootL | 8 / 17 |
| ThighR | 8 / 13 |
| ShinR | 8 / 30 |
| FootR | 8 / 29 |


## Orc B #5

Present 20.8–22.1 s. Time in states: Steady 1.3 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (1 s) |
| --- | --- |
| Pelvis | 5 / 5 |
| Spine | 3 / 3 |
| Chest | 1 / 1 |
| Head | 0 / 2 |
| UpperArmL | 1 / 8 |
| ForearmL | 1 / 13 |
| HandL | 1 / 3 |
| UpperArmR | 1 / 8 |
| ForearmR | 1 / 12 |
| HandR | 1 / 4 |
| ThighL | 5 / 25 |
| ShinL | 10 / 26 |
| FootL | 15 / 16 |
| ThighR | 17 / 18 |
| ShinR | 8 / 28 |
| FootR | 9 / 34 |


## Orc A (club) #6

Present 22.1–23.5 s. Time in states: Steady 1.4 s.
Hits taken 0 (0 damage). Catch steps 1, settle steps 1.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (1 s) |
| --- | --- |
| Pelvis | 6 / 21 |
| Spine | 9 / 18 |
| Chest | 2 / 4 |
| Head | 3 / 24 |
| UpperArmL | 25 / 61 |
| ForearmL | 18 / 60 |
| HandL | 70 / 149 |
| UpperArmR | 17 / 36 |
| ForearmR | 67 / 121 |
| HandR | 113 / 175 |
| ThighL | 4 / 25 |
| ShinL | 7 / 28 |
| FootL | 4 / 17 |
| ThighR | 3 / 13 |
| ShinR | 10 / 37 |
| FootR | 8 / 31 |


## Orc B #6

Present 22.1–23.5 s. Time in states: Steady 1.4 s.
Hits taken 0 (0 damage). Catch steps 1, settle steps 1.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (1 s) |
| --- | --- |
| Pelvis | 9 / 11 |
| Spine | 2 / 3 |
| Chest | 1 / 1 |
| Head | 1 / 3 |
| UpperArmL | 1 / 7 |
| ForearmL | 2 / 13 |
| HandL | 1 / 5 |
| UpperArmR | 2 / 9 |
| ForearmR | 1 / 13 |
| HandR | 1 / 5 |
| ThighL | 4 / 29 |
| ShinL | 9 / 31 |
| FootL | 2 / 12 |
| ThighR | 4 / 10 |
| ShinR | 10 / 35 |
| FootR | 12 / 36 |


## Orc A (club) #7

Present 23.5–26.9 s. Time in states: Steady 3.4 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (3 s) |
| --- | --- |
| Pelvis | 12 / 22 |
| Spine | 4 / 10 |
| Chest | 4 / 11 |
| Head | 2 / 18 |
| UpperArmL | 23 / 61 |
| ForearmL | 15 / 59 |
| HandL | 66 / 149 |
| UpperArmR | 27 / 50 |
| ForearmR | 16 / 85 |
| HandR | 109 / 175 |
| ThighL | 5 / 22 |
| ShinL | 11 / 24 |
| FootL | 10 / 13 |
| ThighR | 6 / 10 |
| ShinR | 8 / 32 |
| FootR | 7 / 32 |


## Orc B #7

Present 23.5–26.9 s. Time in states: Steady 3.4 s.
Hits taken 0 (0 damage). Catch steps 3, settle steps 3.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (3 s) |
| --- | --- |
| Pelvis | 10 / 12 |
| Spine | 2 / 4 |
| Chest | 1 / 2 |
| Head | 1 / 3 |
| UpperArmL | 1 / 8 |
| ForearmL | 1 / 13 |
| HandL | 1 / 4 |
| UpperArmR | 1 / 7 |
| ForearmR | 1 / 12 |
| HandR | 1 / 4 |
| ThighL | 5 / 26 |
| ShinL | 4 / 26 |
| FootL | 4 / 12 |
| ThighR | 4 / 9 |
| ShinR | 14 / 30 |
| FootR | 8 / 34 |

