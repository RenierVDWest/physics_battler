# Playtest 2026-10-31_08-06-38

Unity 6000.0.60f1, scene Sandbox, physics 60 Hz, solver 12/4.
Recorded 0,0 s of play (3485 physics steps), 456 events, 3 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and ghost), `world.csv.gz` (props), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_08-06-38`.

## Orc A (club)

Present 0,0–19,9 s. Time in states: Steady 5,1 s, Knockdown 8,2 s, GettingUp 6,6 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 0.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 2,7 | Orc B, 1,64 m | Recover | 4,7 m/s | 130° | 0,72 m (Recover) | abandoned: in Recover at θ 50°: unit went into Knockdown |

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 5,0 | tipped: chest tilt 60° > maxTiltDegrees 60° | HandL after 0,1 s | lying face down | 0,90 / 0,90 m | FAILED: fell again 0,0 s after standing (tipped: chest tilt 75° > maxTiltDegrees 60°) |
| 2 | 7,4 | tipped: chest tilt 75° > maxTiltDegrees 60° | HandR after 0,2 s | lying face down | 0,90 / 0,90 m | FAILED: fell again 0,0 s after standing (tipped: chest tilt 73° > maxTiltDegrees 60°) |
| 3 | 10,0 | tipped: chest tilt 73° > maxTiltDegrees 60° | HandL after 0,2 s | lying face down | 0,90 / 0,90 m | FAILED: fell again 0,0 s after standing (tipped: chest tilt 83° > maxTiltDegrees 60°) |
| 4 | 12,8 | tipped: chest tilt 83° > maxTiltDegrees 60° | HandR after 0,2 s | lying face down | 0,90 / 0,90 m | FAILED: fell again 0,0 s after standing (tipped: chest tilt 82° > maxTiltDegrees 60°) |
| 5 | 15,6 | tipped: chest tilt 82° > maxTiltDegrees 60° | HandR after 0,2 s | lying face down | 0,90 / 0,90 m | FAILED: fell again 0,0 s after standing (tipped: chest tilt 82° > maxTiltDegrees 60°) |
| 6 | 18,5 | tipped: chest tilt 82° > maxTiltDegrees 60° | HandL after 0,2 s | - | never started | - |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (3 s) | Attacking (2 s) | Knockdown (8 s) | GettingUp (7 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 20 / 83 | 35 / 63 | 68 / 81 | 85 / 105 |
| Spine | 2 / 4 | 3 / 6 | 4 / 18 | 3 / 10 |
| Chest | 1 / 3 | 2 / 4 | 3 / 10 | 2 / 8 |
| Head | 27 / 45 | 32 / 42 | 28 / 50 | 36 / 58 |
| UpperArmL | 6 / 17 | 10 / 27 | 39 / 73 | 23 / 66 |
| ForearmL | 8 / 28 | 13 / 30 | 51 / 102 | 42 / 141 |
| HandL | 6 / 22 | 18 / 42 | 59 / 102 | 54 / 87 |
| UpperArmR | 12 / 23 | 20 / 61 | 25 / 79 | 13 / 64 |
| ForearmR | 38 / 90 | 32 / 67 | 40 / 87 | 25 / 64 |
| HandR | 45 / 72 | 145 / 179 | 57 / 80 | 56 / 76 |
| ThighL | 6 / 13 | 10 / 31 | 30 / 53 | 18 / 53 |
| ShinL | 7 / 12 | 13 / 29 | 42 / 77 | 16 / 41 |
| FootL | 6 / 14 | 18 / 25 | 27 / 49 | 18 / 57 |
| ThighR | 11 / 16 | 17 / 27 | 20 / 44 | 17 / 37 |
| ShinR | 11 / 20 | 22 / 37 | 32 / 80 | 19 / 49 |
| FootR | 6 / 13 | 15 / 41 | 28 / 47 | 24 / 83 |


## Orc B

Present 0,0–19,9 s. Time in states: Steady 19,9 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (20 s) |
| --- | --- |
| Pelvis | 14 / 19 |
| Spine | 2 / 2 |
| Chest | 1 / 2 |
| Head | 28 / 38 |
| UpperArmL | 5 / 9 |
| ForearmL | 7 / 14 |
| HandL | 5 / 15 |
| UpperArmR | 5 / 7 |
| ForearmR | 7 / 13 |
| HandR | 5 / 12 |
| ThighL | 7 / 11 |
| ShinL | 11 / 19 |
| FootL | 5 / 9 |
| ThighR | 7 / 11 |
| ShinR | 11 / 19 |
| FootR | 5 / 9 |


## Orc A (club) #2

Present 19,9–48,8 s. Time in states: Steady 16,1 s, Knockdown 7,1 s, GettingUp 5,7 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 0.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 21,3 | Orc B #2, 1,67 m | Recover | 5,9 m/s | 173° | 0,95 m (Recover) | missed: club passed 0,95 m from the target point (in Recover) |
| 2 | 25,0 | Orc B #2, 1,66 m | Recover | 5,5 m/s | 178° | 1,07 m (Recover) | missed: club passed 1,07 m from the target point (in Recover) |
| 3 | 29,0 | Orc B #2, 1,69 m | Recover | 8,2 m/s | 175° | 0,96 m (Recover) | missed: club passed 0,96 m from the target point (in Recover) |
| 4 | 33,0 | Orc B #2, 1,69 m | Recover | 10,9 m/s | 157° | 0,56 m (Recover) | missed: club passed 0,56 m from the target point (in Recover) |

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 36,0 | tipped: chest tilt 60° > maxTiltDegrees 60° | HandL after 0,3 s | lying face down | 0,90 / 0,90 m | FAILED: fell again 0,0 s after standing (tipped: chest tilt 83° > maxTiltDegrees 60°) |
| 2 | 38,5 | tipped: chest tilt 83° > maxTiltDegrees 60° | HandL after 0,2 s | lying face down | 0,90 / 0,90 m | FAILED: fell again 0,0 s after standing (tipped: chest tilt 82° > maxTiltDegrees 60°) |
| 3 | 41,2 | tipped: chest tilt 82° > maxTiltDegrees 60° | HandL after 0,1 s | lying face down | 0,90 / 0,90 m | FAILED: fell again 0,0 s after standing (tipped: chest tilt 84° > maxTiltDegrees 60°) |
| 4 | 44,2 | tipped: chest tilt 84° > maxTiltDegrees 60° | HandL after 0,2 s | lying face down | 0,90 / 0,90 m | FAILED: fell again 0,0 s after standing (tipped: chest tilt 83° > maxTiltDegrees 60°) |
| 5 | 46,9 | tipped: chest tilt 83° > maxTiltDegrees 60° | HandL after 0,1 s | lying face down | 0,37 / - m | - |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (7 s) | Attacking (10 s) | Knockdown (7 s) | GettingUp (6 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 20 / 83 | 27 / 64 | 69 / 81 | 86 / 105 |
| Spine | 2 / 4 | 2 / 9 | 3 / 18 | 3 / 8 |
| Chest | 1 / 3 | 2 / 11 | 3 / 9 | 2 / 7 |
| Head | 27 / 49 | 30 / 57 | 29 / 56 | 40 / 69 |
| UpperArmL | 9 / 25 | 13 / 38 | 40 / 60 | 15 / 26 |
| ForearmL | 12 / 37 | 17 / 47 | 63 / 88 | 25 / 53 |
| HandL | 15 / 38 | 25 / 66 | 62 / 83 | 50 / 82 |
| UpperArmR | 12 / 35 | 36 / 177 | 18 / 53 | 17 / 64 |
| ForearmR | 34 / 90 | 34 / 98 | 37 / 80 | 33 / 57 |
| HandR | 66 / 97 | 130 / 180 | 65 / 81 | 59 / 78 |
| ThighL | 6 / 13 | 8 / 24 | 39 / 63 | 23 / 53 |
| ShinL | 8 / 24 | 12 / 30 | 44 / 77 | 16 / 41 |
| FootL | 10 / 18 | 18 / 77 | 29 / 47 | 21 / 58 |
| ThighR | 10 / 25 | 13 / 43 | 22 / 44 | 18 / 38 |
| ShinR | 11 / 31 | 18 / 44 | 29 / 78 | 21 / 63 |
| FootR | 10 / 26 | 18 / 54 | 30 / 47 | 27 / 72 |


## Orc B #2

Present 19,9–48,8 s. Time in states: Steady 28,9 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (29 s) |
| --- | --- |
| Pelvis | 14 / 19 |
| Spine | 2 / 2 |
| Chest | 1 / 2 |
| Head | 28 / 38 |
| UpperArmL | 5 / 9 |
| ForearmL | 7 / 14 |
| HandL | 5 / 15 |
| UpperArmR | 5 / 7 |
| ForearmR | 7 / 13 |
| HandR | 5 / 12 |
| ThighL | 7 / 11 |
| ShinL | 11 / 19 |
| FootL | 5 / 9 |
| ThighR | 7 / 11 |
| ShinR | 11 / 19 |
| FootR | 5 / 9 |


## Orc A (club) #3

Present 48,8–0,0 s. Time in states: Steady 3,1 s, Knockdown 3,5 s, GettingUp 2,6 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 0.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 49,0 | Orc B #3, 1,81 m | Recover | 7,3 m/s | 143° | 0,66 m (Recover) | missed: club passed 0,66 m from the target point (in Recover) |

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 51,9 | tipped: chest tilt 61° > maxTiltDegrees 60° | HandL after 0,2 s | lying face down | 0,90 / 0,90 m | FAILED: fell again 0,0 s after standing (tipped: chest tilt 75° > maxTiltDegrees 60°) |
| 2 | 54,5 | tipped: chest tilt 75° > maxTiltDegrees 60° | HandL after 0,2 s | lying face down | 0,90 / 0,90 m | FAILED: fell again 0,0 s after standing (tipped: chest tilt 77° > maxTiltDegrees 60°) |
| 3 | 57,3 | tipped: chest tilt 77° > maxTiltDegrees 60° | HandL after 0,2 s | - | never started | - |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (1 s) | Attacking (2 s) | Knockdown (4 s) | GettingUp (3 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 38 / 77 | 28 / 57 | 70 / 92 | 87 / 105 |
| Spine | 2 / 3 | 4 / 10 | 4 / 19 | 3 / 10 |
| Chest | 1 / 1 | 2 / 8 | 3 / 9 | 2 / 8 |
| Head | 29 / 49 | 30 / 47 | 33 / 68 | 45 / 70 |
| UpperArmL | 13 / 19 | 12 / 33 | 44 / 62 | 15 / 32 |
| ForearmL | 18 / 30 | 16 / 32 | 52 / 87 | 22 / 61 |
| HandL | 17 / 29 | 23 / 51 | 48 / 83 | 42 / 81 |
| UpperArmR | 18 / 45 | 75 / 176 | 15 / 52 | 17 / 50 |
| ForearmR | 25 / 90 | 32 / 76 | 29 / 76 | 9 / 29 |
| HandR | 42 / 83 | 124 / 175 | 54 / 84 | 44 / 70 |
| ThighL | 13 / 23 | 9 / 19 | 35 / 56 | 23 / 58 |
| ShinL | 7 / 13 | 12 / 25 | 40 / 77 | 18 / 68 |
| FootL | 12 / 20 | 16 / 47 | 31 / 48 | 20 / 43 |
| ThighR | 6 / 12 | 23 / 59 | 21 / 51 | 17 / 30 |
| ShinR | 6 / 10 | 14 / 25 | 30 / 78 | 24 / 70 |
| FootR | 8 / 15 | 11 / 25 | 31 / 45 | 30 / 82 |


## Orc B #3

Present 48,8–0,0 s. Time in states: Steady 9,3 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (9 s) |
| --- | --- |
| Pelvis | 14 / 20 |
| Spine | 1 / 2 |
| Chest | 1 / 2 |
| Head | 27 / 38 |
| UpperArmL | 5 / 9 |
| ForearmL | 7 / 14 |
| HandL | 5 / 14 |
| UpperArmR | 5 / 7 |
| ForearmR | 6 / 13 |
| HandR | 5 / 11 |
| ThighL | 8 / 11 |
| ShinL | 11 / 19 |
| FootL | 5 / 9 |
| ThighR | 8 / 11 |
| ShinR | 11 / 19 |
| FootR | 5 / 9 |

## Marks

- mark 1 at 5,507 s 
- mark 2 at 36,035 s 
- mark 3 at 42,515 s 

