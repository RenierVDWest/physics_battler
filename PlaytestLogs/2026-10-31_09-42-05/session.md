# Playtest 2026-10-31_09-42-05

Unity 6000.6.7f1, scene Sandbox, physics 60 Hz, solver 12/4.
Recorded 39.2 s of play (2351 physics steps), 341 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and ghost), `world.csv.gz` (props), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_09-42-05`.

## Orc A (club)

Present 0.0–39.2 s. Time in states: Steady 39.2 s.
Hits taken 0 (0 damage). Catch steps 11, settle steps 3.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 2.7 | Orc B, 1.81 m | Recover | 21.4 m/s | 178° | 0.84 m (Recover) | missed: club passed 0.84 m from the target point (in Recover) |
| 2 | 7.3 | Orc B, 1.86 m | Recover | 13.0 m/s | 179° | 0.96 m (Recover) | missed: club passed 0.96 m from the target point (in Recover) |
| 3 | 11.5 | Orc B, 1.86 m | Recover | 14.4 m/s | 180° | 1.09 m (Recover) | missed: club passed 1.09 m from the target point (in Recover) |
| 4 | 17.4 | Orc B, 1.89 m | Recover | 13.8 m/s | 180° | 0.93 m (Recover) | missed: club passed 0.93 m from the target point (in Recover) |
| 5 | 20.5 | Orc B, 1.90 m | Recover | 13.9 m/s | 180° | 0.97 m (Recover) | missed: club passed 0.97 m from the target point (in Recover) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (27 s) | Attacking (12 s) |
| --- | --- | --- |
| Pelvis | 4 / 13 | 8 / 35 |
| Spine | 1 / 5 | 5 / 13 |
| Chest | 0 / 2 | 2 / 9 |
| Head | 1 / 9 | 8 / 24 |
| UpperArmL | 1 / 9 | 4 / 17 |
| ForearmL | 1 / 13 | 7 / 26 |
| HandL | 1 / 10 | 9 / 31 |
| UpperArmR | 1 / 7 | 66 / 180 |
| ForearmR | 3 / 90 | 23 / 55 |
| HandR | 3 / 41 | 119 / 179 |
| ThighL | 4 / 7 | 3 / 7 |
| ShinL | 2 / 10 | 7 / 24 |
| FootL | 8 / 18 | 10 / 26 |
| ThighR | 1 / 5 | 3 / 10 |
| ShinR | 4 / 10 | 7 / 53 |
| FootR | 5 / 19 | 8 / 44 |


## Orc B

Present 0.0–39.2 s. Time in states: Steady 39.2 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (39 s) |
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

