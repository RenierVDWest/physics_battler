# Playtest 2026-10-31_10-21-26

Unity 6000.6.7f1, scene Sandbox, physics 60 Hz, solver 12/4.
Recorded 2.4 s of play (146 physics steps), 30 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and ghost), `world.csv.gz` (props), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_10-21-26`.

## Orc A (club)

Present 0.0–2.4 s. Time in states: Steady 2.4 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 1.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (2 s) |
| --- | --- |
| Pelvis | 6 / 10 |
| Spine | 1 / 3 |
| Chest | 0 / 1 |
| Head | 1 / 3 |
| UpperArmL | 1 / 9 |
| ForearmL | 1 / 13 |
| HandL | 1 / 5 |
| UpperArmR | 1 / 7 |
| ForearmR | 2 / 90 |
| HandR | 2 / 17 |
| ThighL | 4 / 27 |
| ShinL | 9 / 28 |
| FootL | 6 / 12 |
| ThighR | 4 / 10 |
| ShinR | 13 / 32 |
| FootR | 10 / 35 |


## Orc B

Present 0.0–2.4 s. Time in states: Steady 2.4 s.
Hits taken 0 (0 damage). Catch steps 2, settle steps 2.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (2 s) |
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
| FootL | 5 / 12 |
| ThighR | 3 / 10 |
| ShinR | 8 / 32 |
| FootR | 11 / 35 |

