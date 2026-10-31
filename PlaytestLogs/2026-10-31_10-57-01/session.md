# Playtest 2026-10-31_10-57-01

Unity 6000.6.7f1, scene Sandbox, physics 60 Hz, solver 12/4.
Recorded 19.7 s of play (1181 physics steps), 280 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and ghost), `world.csv.gz` (props), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_10-57-01`.

## Orc A (club)

Present 0.0–19.7 s. Time in states: Steady 19.7 s.
Hits taken 0 (0 damage). Catch steps 13, settle steps 1.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 3.7 | Orc B, 1.63 m | Recover | 21.9 m/s | 76° | 0.11 m (Follow) | HIT Orc B Spine at 9.0 m/s (Follow) |
| 2 | 7.0 | Orc B, 1.22 m | Recover | 24.9 m/s | 129° | 0.21 m (Follow) | HIT Orc B Chest at 11.9 m/s (Follow) |
| 3 | 9.5 | Orc B, 1.02 m | Recover | 27.5 m/s | 145° | 0.14 m (Follow) | HIT Orc B Chest at 13.1 m/s (Follow) |
| 4 | 11.8 | Orc B, 0.64 m | Recover | 17.5 m/s | 177° | 0.30 m (Recover) | HIT Orc B ForearmL at 8.7 m/s (Follow) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (11 s) | Attacking (9 s) |
| --- | --- | --- |
| Pelvis | 4 / 9 | 7 / 17 |
| Spine | 1 / 4 | 3 / 7 |
| Chest | 0 / 1 | 1 / 4 |
| Head | 1 / 14 | 3 / 15 |
| UpperArmL | 0 / 9 | 3 / 11 |
| ForearmL | 1 / 13 | 5 / 17 |
| HandL | 1 / 6 | 6 / 24 |
| UpperArmR | 1 / 7 | 10 / 82 |
| ForearmR | 1 / 90 | 11 / 85 |
| HandR | 1 / 17 | 73 / 149 |
| ThighL | 10 / 27 | 6 / 19 |
| ShinL | 6 / 28 | 7 / 14 |
| FootL | 8 / 12 | 8 / 18 |
| ThighR | 3 / 10 | 4 / 13 |
| ShinR | 7 / 32 | 10 / 27 |
| FootR | 12 / 35 | 11 / 26 |


## Orc B

Present 0.0–19.7 s. Time in states: Steady 12.0 s, Flinch 0.9 s, Dead 6.8 s.
Hits taken 5 (105 damage). Catch steps 11, settle steps 10.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (12 s) | Flinch (1 s) | Dead (7 s) |
| --- | --- | --- | --- |
| Pelvis | 8 / 14 | 11 / 15 | 80 / 92 |
| Spine | 2 / 4 | 4 / 6 | 28 / 37 |
| Chest | 1 / 3 | 2 / 3 | 26 / 36 |
| Head | 1 / 11 | 7 / 16 | 77 / 98 |
| UpperArmL | 0 / 9 | 1 / 2 | 111 / 129 |
| ForearmL | 1 / 13 | 1 / 3 | 39 / 87 |
| HandL | 1 / 5 | 2 / 5 | 49 / 86 |
| UpperArmR | 1 / 7 | 1 / 3 | 94 / 117 |
| ForearmR | 1 / 13 | 1 / 2 | 77 / 86 |
| HandR | 1 / 4 | 1 / 4 | 41 / 79 |
| ThighL | 2 / 27 | 2 / 6 | 58 / 70 |
| ShinL | 5 / 28 | 6 / 11 | 63 / 76 |
| FootL | 4 / 12 | 6 / 8 | 32 / 51 |
| ThighR | 2 / 10 | 2 / 4 | 55 / 73 |
| ShinR | 5 / 32 | 5 / 7 | 44 / 77 |
| FootR | 11 / 35 | 10 / 12 | 35 / 45 |

