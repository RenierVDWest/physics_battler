# Playtest 2026-10-31_10-56-21

Unity 6000.6.7f1, scene Sandbox, physics 60 Hz, solver 12/4.
Recorded 25.8 s of play (1550 physics steps), 217 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and ghost), `world.csv.gz` (props), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_10-56-21`.

## Orc A (club)

Present 0.0–15.9 s. Time in states: Steady 15.9 s.
Hits taken 0 (0 damage). Catch steps 8, settle steps 5.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 5.0 | Orc B, 1.62 m | Recover | 25.6 m/s | 83° | 0.11 m (Follow) | HIT Orc B Chest at 9.4 m/s (Follow) |
| 2 | 11.0 | Orc B, 1.34 m | Recover | 27.8 m/s | 82° | 0.08 m (Follow) | HIT Orc B Spine at 7.4 m/s (Follow) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (11 s) | Attacking (5 s) |
| --- | --- | --- |
| Pelvis | 6 / 10 | 7 / 19 |
| Spine | 1 / 4 | 2 / 7 |
| Chest | 1 / 1 | 1 / 4 |
| Head | 1 / 8 | 3 / 13 |
| UpperArmL | 0 / 9 | 2 / 8 |
| ForearmL | 1 / 13 | 4 / 15 |
| HandL | 0 / 5 | 5 / 23 |
| UpperArmR | 1 / 7 | 11 / 60 |
| ForearmR | 1 / 90 | 8 / 46 |
| HandR | 1 / 17 | 68 / 130 |
| ThighL | 4 / 27 | 3 / 6 |
| ShinL | 7 / 28 | 8 / 19 |
| FootL | 5 / 12 | 8 / 16 |
| ThighR | 4 / 10 | 5 / 11 |
| ShinR | 11 / 32 | 12 / 21 |
| FootR | 10 / 35 | 8 / 14 |


## Orc B

Present 0.0–15.9 s. Time in states: Steady 15.3 s, Flinch 0.6 s.
Hits taken 2 (56 damage). Catch steps 6, settle steps 8.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (15 s) | Flinch (1 s) |
| --- | --- | --- |
| Pelvis | 7 / 11 | 8 / 10 |
| Spine | 1 / 4 | 2 / 3 |
| Chest | 1 / 2 | 1 / 3 |
| Head | 1 / 4 | 2 / 7 |
| UpperArmL | 0 / 9 | 1 / 1 |
| ForearmL | 1 / 13 | 1 / 1 |
| HandL | 0 / 5 | 1 / 1 |
| UpperArmR | 0 / 7 | 1 / 2 |
| ForearmR | 1 / 13 | 1 / 1 |
| HandR | 1 / 4 | 1 / 1 |
| ThighL | 2 / 27 | 1 / 2 |
| ShinL | 5 / 28 | 5 / 9 |
| FootL | 4 / 12 | 4 / 6 |
| ThighR | 3 / 10 | 2 / 3 |
| ShinR | 9 / 32 | 8 / 13 |
| FootR | 10 / 35 | 10 / 10 |


## Orc A (club) #2

Present 15.9–18.5 s. Time in states: Steady 2.6 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 1.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (3 s) |
| --- | --- |
| Pelvis | 6 / 10 |
| Spine | 1 / 4 |
| Chest | 1 / 1 |
| Head | 1 / 3 |
| UpperArmL | 0 / 7 |
| ForearmL | 1 / 13 |
| HandL | 1 / 4 |
| UpperArmR | 1 / 8 |
| ForearmR | 1 / 90 |
| HandR | 1 / 17 |
| ThighL | 4 / 27 |
| ShinL | 10 / 29 |
| FootL | 6 / 12 |
| ThighR | 5 / 10 |
| ShinR | 14 / 33 |
| FootR | 11 / 35 |


## Orc B #2

Present 15.9–18.5 s. Time in states: Steady 2.6 s.
Hits taken 0 (0 damage). Catch steps 2, settle steps 2.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (3 s) |
| --- | --- |
| Pelvis | 9 / 11 |
| Spine | 2 / 3 |
| Chest | 1 / 1 |
| Head | 1 / 3 |
| UpperArmL | 1 / 7 |
| ForearmL | 1 / 13 |
| HandL | 1 / 4 |
| UpperArmR | 1 / 9 |
| ForearmR | 1 / 13 |
| HandR | 1 / 5 |
| ThighL | 2 / 27 |
| ShinL | 5 / 29 |
| FootL | 5 / 12 |
| ThighR | 3 / 10 |
| ShinR | 8 / 33 |
| FootR | 11 / 35 |


## Orc A (club) #3

Present 18.5–23.1 s. Time in states: Steady 4.5 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 1.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (5 s) |
| --- | --- |
| Pelvis | 4 / 11 |
| Spine | 2 / 3 |
| Chest | 1 / 2 |
| Head | 1 / 3 |
| UpperArmL | 0 / 7 |
| ForearmL | 1 / 13 |
| HandL | 1 / 4 |
| UpperArmR | 1 / 8 |
| ForearmR | 1 / 90 |
| HandR | 1 / 17 |
| ThighL | 6 / 27 |
| ShinL | 16 / 28 |
| FootL | 2 / 12 |
| ThighR | 3 / 10 |
| ShinR | 19 / 32 |
| FootR | 10 / 35 |


## Orc B #3

Present 18.5–23.1 s. Time in states: Steady 4.5 s.
Hits taken 0 (0 damage). Catch steps 4, settle steps 4.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (5 s) |
| --- | --- |
| Pelvis | 10 / 12 |
| Spine | 2 / 3 |
| Chest | 2 / 2 |
| Head | 1 / 3 |
| UpperArmL | 1 / 7 |
| ForearmL | 1 / 13 |
| HandL | 1 / 4 |
| UpperArmR | 1 / 9 |
| ForearmR | 1 / 13 |
| HandR | 1 / 5 |
| ThighL | 3 / 27 |
| ShinL | 8 / 28 |
| FootL | 2 / 12 |
| ThighR | 4 / 10 |
| ShinR | 15 / 32 |
| FootR | 8 / 35 |


## Orc A (club) #4

Present 23.1–25.8 s. Time in states: Steady 2.8 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (3 s) |
| --- | --- |
| Pelvis | 5 / 5 |
| Spine | 3 / 3 |
| Chest | 1 / 1 |
| Head | 0 / 1 |
| UpperArmL | 1 / 9 |
| ForearmL | 0 / 12 |
| HandL | 1 / 4 |
| UpperArmR | 0 / 7 |
| ForearmR | 1 / 90 |
| HandR | 1 / 17 |
| ThighL | 5 / 26 |
| ShinL | 11 / 27 |
| FootL | 15 / 16 |
| ThighR | 17 / 18 |
| ShinR | 8 / 29 |
| FootR | 8 / 34 |


## Orc B #4

Present 23.1–25.8 s. Time in states: Steady 2.8 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (3 s) |
| --- | --- |
| Pelvis | 6 / 7 |
| Spine | 3 / 3 |
| Chest | 1 / 1 |
| Head | 0 / 1 |
| UpperArmL | 1 / 9 |
| ForearmL | 1 / 12 |
| HandL | 1 / 4 |
| UpperArmR | 0 / 7 |
| ForearmR | 1 / 12 |
| HandR | 1 / 4 |
| ThighL | 5 / 26 |
| ShinL | 10 / 27 |
| FootL | 15 / 16 |
| ThighR | 18 / 18 |
| ShinR | 8 / 29 |
| FootR | 10 / 34 |

