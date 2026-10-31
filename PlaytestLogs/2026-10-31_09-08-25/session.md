# Playtest 2026-10-31_09-08-25

Unity 6000.6.7f1, scene Sandbox, physics 60 Hz, solver 12/4.
Recorded 37.2 s of play (2231 physics steps), 519 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and ghost), `world.csv.gz` (props), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_09-08-25`.

## Orc A (club)

Present 0.0–11.0 s. Time in states: Steady 11.0 s.
Hits taken 0 (0 damage). Catch steps 3, settle steps 1.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 1.8 | Orc B, 1.81 m | Recover | 28.4 m/s | 175° | 0.70 m (Follow) | HIT Orc B Head at 12.4 m/s (WindUp) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (9 s) | Attacking (2 s) |
| --- | --- | --- |
| Pelvis | 12 / 17 | 10 / 39 |
| Spine | 8 / 11 | 6 / 15 |
| Chest | 1 / 2 | 3 / 8 |
| Head | 4 / 7 | 7 / 29 |
| UpperArmL | 6 / 9 | 8 / 41 |
| ForearmL | 4 / 13 | 10 / 33 |
| HandL | 11 / 27 | 16 / 44 |
| UpperArmR | 8 / 11 | 83 / 180 |
| ForearmR | 8 / 90 | 21 / 63 |
| HandR | 10 / 37 | 102 / 157 |
| ThighL | 7 / 12 | 7 / 13 |
| ShinL | 7 / 15 | 11 / 22 |
| FootL | 24 / 37 | 18 / 29 |
| ThighR | 3 / 7 | 5 / 14 |
| ShinR | 10 / 20 | 8 / 39 |
| FootR | 20 / 43 | 11 / 39 |


## Orc B

Present 0.0–11.0 s. Time in states: Steady 6.6 s, Stagger 0.9 s, Knockdown 1.7 s, GettingUp 1.8 s.
Hits taken 2 (61 damage). Catch steps 2, settle steps 0.

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 3.0 | stability 0 < knockdownBelow 25 (lost 75.8) | Pelvis after 0.6 s | lying on its back | 0.92 / 0.92 m | stood up (pelvis 0.92 m, tilt 2° when the get-up ended) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (7 s) | Stagger (1 s) | Knockdown (2 s) | GettingUp (2 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 11 / 20 | 9 / 12 | 75 / 100 | 28 / 179 |
| Spine | 4 / 8 | 6 / 9 | 11 / 20 | 10 / 22 |
| Chest | 1 / 2 | 2 / 3 | 5 / 9 | 2 / 5 |
| Head | 4 / 4 | 27 / 71 | 17 / 31 | 11 / 39 |
| UpperArmL | 7 / 11 | 7 / 15 | 12 / 48 | 9 / 27 |
| ForearmL | 5 / 13 | 6 / 20 | 15 / 46 | 7 / 21 |
| HandL | 13 / 25 | 18 / 49 | 27 / 56 | 22 / 63 |
| UpperArmR | 7 / 10 | 7 / 16 | 12 / 43 | 11 / 45 |
| ForearmR | 6 / 13 | 7 / 22 | 11 / 37 | 11 / 65 |
| HandR | 14 / 26 | 15 / 43 | 20 / 50 | 20 / 81 |
| ThighL | 7 / 12 | 3 / 8 | 6 / 9 | 18 / 29 |
| ShinL | 12 / 21 | 9 / 18 | 8 / 26 | 15 / 38 |
| FootL | 22 / 45 | 10 / 18 | 20 / 47 | 35 / 77 |
| ThighR | 8 / 16 | 6 / 10 | 6 / 20 | 7 / 16 |
| ShinR | 20 / 37 | 14 / 21 | 10 / 26 | 12 / 38 |
| FootR | 15 / 26 | 21 / 31 | 21 / 51 | 27 / 73 |


## Orc A (club) #2

Present 11.0–20.2 s. Time in states: Steady 6.2 s, Knockdown 1.4 s, GettingUp 1.6 s.
Hits taken 0 (0 damage). Catch steps 4, settle steps 0.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 11.9 | Orc B #2, 1.80 m | Recover | 24.6 m/s | 179° | 0.44 m (Follow) | HIT Orc B #2 Head at 9.7 m/s (Follow) |
| 2 | 15.5 | Orc B #2, 1.82 m | Recover | 32.2 m/s | 175° | 0.50 m (Follow) | HIT Orc B #2 Head at 7.2 m/s (WindUp) |

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 17.2 | out of catch steps and stuck off balance for 0.4 s after a bonus step (error 0.25 m > 0.24 m) | Pelvis after 0.6 s | lying on its side | 0.90 / - m | - |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (2 s) | Attacking (4 s) | Knockdown (1 s) | GettingUp (2 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 9 / 18 | 9 / 43 | 82 / 106 | 18 / 85 |
| Spine | 4 / 7 | 6 / 19 | 7 / 15 | 11 / 17 |
| Chest | 1 / 2 | 3 / 11 | 3 / 4 | 2 / 4 |
| Head | 4 / 5 | 7 / 36 | 18 / 36 | 8 / 44 |
| UpperArmL | 6 / 10 | 7 / 54 | 11 / 60 | 4 / 11 |
| ForearmL | 5 / 13 | 9 / 36 | 14 / 44 | 6 / 20 |
| HandL | 12 / 23 | 15 / 56 | 19 / 33 | 14 / 44 |
| UpperArmR | 7 / 14 | 62 / 180 | 16 / 36 | 8 / 45 |
| ForearmR | 13 / 90 | 29 / 76 | 36 / 84 | 30 / 45 |
| HandR | 16 / 32 | 113 / 175 | 33 / 50 | 36 / 47 |
| ThighL | 3 / 8 | 6 / 13 | 12 / 29 | 19 / 25 |
| ShinL | 6 / 11 | 8 / 24 | 12 / 29 | 31 / 51 |
| FootL | 9 / 19 | 12 / 40 | 36 / 48 | 30 / 95 |
| ThighR | 4 / 8 | 5 / 11 | 6 / 19 | 5 / 15 |
| ShinR | 6 / 13 | 10 / 22 | 9 / 23 | 16 / 41 |
| FootR | 12 / 27 | 14 / 46 | 17 / 30 | 26 / 72 |


## Orc B #2

Present 11.0–20.2 s. Time in states: Steady 3.2 s, Stagger 2.4 s, Dead 3.6 s.
Hits taken 3 (124 damage). Catch steps 4, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (3 s) | Stagger (2 s) | Dead (4 s) |
| --- | --- | --- | --- |
| Pelvis | 9 / 16 | 8 / 11 | 127 / 146 |
| Spine | 6 / 14 | 6 / 9 | 36 / 54 |
| Chest | 1 / 2 | 1 / 3 | 20 / 31 |
| Head | 4 / 5 | 19 / 78 | 60 / 73 |
| UpperArmL | 7 / 11 | 6 / 13 | 80 / 111 |
| ForearmL | 5 / 14 | 5 / 21 | 75 / 99 |
| HandL | 14 / 46 | 14 / 50 | 47 / 70 |
| UpperArmR | 7 / 11 | 6 / 18 | 76 / 98 |
| ForearmR | 6 / 15 | 5 / 21 | 58 / 80 |
| HandR | 13 / 43 | 12 / 47 | 40 / 109 |
| ThighL | 5 / 9 | 6 / 10 | 48 / 60 |
| ShinL | 9 / 17 | 12 / 23 | 43 / 76 |
| FootL | 18 / 39 | 17 / 25 | 28 / 72 |
| ThighR | 3 / 13 | 7 / 12 | 40 / 71 |
| ShinR | 8 / 20 | 13 / 26 | 44 / 80 |
| FootR | 18 / 41 | 21 / 38 | 42 / 66 |


## Orc A (club) #3

Present 20.2–24.4 s. Time in states: Steady 4.1 s.
Hits taken 0 (0 damage). Catch steps 7, settle steps 2.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 20.8 | Orc B #3, 1.81 m | Recover | 28.2 m/s | 169° | 0.18 m (Recover) | HIT Orc B #3 HandL at 16.2 m/s (Follow) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (2 s) | Attacking (2 s) |
| --- | --- | --- |
| Pelvis | 8 / 19 | 8 / 24 |
| Spine | 2 / 5 | 2 / 4 |
| Chest | 2 / 3 | 4 / 18 |
| Head | 4 / 7 | 7 / 23 |
| UpperArmL | 6 / 12 | 8 / 19 |
| ForearmL | 3 / 12 | 4 / 12 |
| HandL | 10 / 19 | 13 / 37 |
| UpperArmR | 7 / 13 | 49 / 179 |
| ForearmR | 13 / 90 | 48 / 116 |
| HandR | 19 / 41 | 104 / 178 |
| ThighL | 4 / 8 | 3 / 8 |
| ShinL | 6 / 15 | 7 / 26 |
| FootL | 9 / 19 | 12 / 32 |
| ThighR | 6 / 13 | 5 / 11 |
| ShinR | 5 / 10 | 5 / 19 |
| FootR | 12 / 25 | 11 / 34 |


## Orc B #3

Present 20.2–24.4 s. Time in states: Steady 2.0 s, Knockdown 1.5 s, GettingUp 0.7 s.
Hits taken 2 (35 damage). Catch steps 0, settle steps 0.

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 22.2 | stability 3 < knockdownBelow 25 (lost 97.0) | HandL after 0.4 s | lying on its back | 0.53 / - m | - |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (2 s) | Knockdown (2 s) | GettingUp (1 s) |
| --- | --- | --- | --- |
| Pelvis | 7 / 13 | 97 / 110 | 28 / 87 |
| Spine | 3 / 5 | 6 / 20 | 6 / 7 |
| Chest | 2 / 3 | 9 / 20 | 3 / 9 |
| Head | 4 / 4 | 20 / 65 | 17 / 48 |
| UpperArmL | 6 / 10 | 57 / 150 | 8 / 19 |
| ForearmL | 3 / 12 | 33 / 96 | 5 / 19 |
| HandL | 10 / 17 | 49 / 86 | 22 / 50 |
| UpperArmR | 6 / 8 | 12 / 55 | 16 / 44 |
| ForearmR | 6 / 13 | 23 / 60 | 23 / 77 |
| HandR | 12 / 23 | 37 / 82 | 23 / 73 |
| ThighL | 4 / 8 | 12 / 30 | 20 / 28 |
| ShinL | 10 / 18 | 29 / 76 | 41 / 53 |
| FootL | 12 / 22 | 37 / 51 | 32 / 45 |
| ThighR | 6 / 11 | 11 / 24 | 6 / 22 |
| ShinR | 6 / 10 | 12 / 45 | 16 / 44 |
| FootR | 16 / 36 | 23 / 46 | 28 / 69 |


## Orc A (club) #4

Present 24.4–27.6 s. Time in states: Steady 3.2 s.
Hits taken 0 (0 damage). Catch steps 2, settle steps 1.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 25.0 | Orc B #4, 1.81 m | Recover | 31.7 m/s | 156° | 0.21 m (Follow) | HIT Orc B #4 Spine at 20.3 m/s (Follow) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (1 s) | Attacking (2 s) |
| --- | --- | --- |
| Pelvis | 6 / 12 | 9 / 39 |
| Spine | 0 / 0 | 1 / 5 |
| Chest | 1 / 2 | 3 / 20 |
| Head | 3 / 5 | 7 / 42 |
| UpperArmL | 6 / 8 | 10 / 70 |
| ForearmL | 5 / 13 | 10 / 25 |
| HandL | 12 / 32 | 15 / 34 |
| UpperArmR | 7 / 9 | 75 / 179 |
| ForearmR | 10 / 90 | 25 / 92 |
| HandR | 16 / 52 | 101 / 168 |
| ThighL | 10 / 19 | 6 / 17 |
| ShinL | 7 / 13 | 5 / 16 |
| FootL | 13 / 23 | 10 / 26 |
| ThighR | 5 / 9 | 8 / 22 |
| ShinR | 12 / 26 | 14 / 31 |
| FootR | 11 / 22 | 19 / 50 |


## Orc B #4

Present 24.4–27.6 s. Time in states: Steady 1.6 s, Knockdown 1.5 s, GettingUp 0.1 s.
Hits taken 1 (67 damage). Catch steps 0, settle steps 0.

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 26.0 | stability 0 < knockdownBelow 25 (lost 122.1) | HandR after 0.7 s | lying on its side | 0.32 / - m | - |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (2 s) | Knockdown (2 s) |
| --- | --- | --- |
| Pelvis | 8 / 16 | 108 / 141 |
| Spine | 4 / 9 | 5 / 8 |
| Chest | 1 / 2 | 4 / 9 |
| Head | 3 / 4 | 19 / 38 |
| UpperArmL | 7 / 11 | 15 / 49 |
| ForearmL | 5 / 13 | 17 / 48 |
| HandL | 12 / 25 | 23 / 51 |
| UpperArmR | 7 / 10 | 37 / 82 |
| ForearmR | 5 / 13 | 12 / 40 |
| HandR | 12 / 23 | 24 / 52 |
| ThighL | 3 / 7 | 9 / 39 |
| ShinL | 9 / 16 | 13 / 57 |
| FootL | 14 / 22 | 16 / 28 |
| ThighR | 3 / 5 | 10 / 37 |
| ShinR | 9 / 18 | 12 / 57 |
| FootR | 12 / 21 | 14 / 30 |


## Orc A (club) #5

Present 27.6–37.2 s. Time in states: Steady 9.6 s.
Hits taken 0 (0 damage). Catch steps 6, settle steps 2.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 29.0 | Orc B #5, 1.80 m | Recover | 23.0 m/s | 145° | 0.67 m (Recover) | missed: club passed 0.67 m from the target point (in Recover) |
| 2 | 33.4 | Orc B #5, 1.86 m | Recover | 20.4 m/s | 145° | 0.58 m (Strike) | HIT Orc B #5 UpperArmL at 12.3 m/s (Follow) |
| 3 | 36.7 | Orc B #5, 1.70 m | WindUp | 10.3 m/s | 0° | - | abandoned: recording stopped |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (5 s) | Attacking (5 s) |
| --- | --- | --- |
| Pelvis | 9 / 16 | 9 / 43 |
| Spine | 6 / 10 | 6 / 14 |
| Chest | 1 / 1 | 1 / 3 |
| Head | 4 / 5 | 5 / 35 |
| UpperArmL | 6 / 10 | 8 / 58 |
| ForearmL | 5 / 14 | 9 / 25 |
| HandL | 14 / 33 | 16 / 42 |
| UpperArmR | 14 / 38 | 66 / 180 |
| ForearmR | 8 / 90 | 18 / 45 |
| HandR | 19 / 54 | 107 / 178 |
| ThighL | 3 / 8 | 4 / 10 |
| ShinL | 8 / 17 | 10 / 18 |
| FootL | 16 / 43 | 16 / 30 |
| ThighR | 4 / 8 | 6 / 17 |
| ShinR | 8 / 18 | 11 / 20 |
| FootR | 13 / 37 | 14 / 30 |


## Orc B #5

Present 27.6–37.2 s. Time in states: Steady 6.9 s, Flinch 0.3 s, Stagger 2.3 s.
Hits taken 2 (31 damage). Catch steps 5, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (7 s) | Flinch (0 s) | Stagger (2 s) |
| --- | --- | --- | --- |
| Pelvis | 11 / 21 | 12 / 17 | 5 / 11 |
| Spine | 6 / 9 | 7 / 8 | 4 / 6 |
| Chest | 1 / 2 | 2 / 3 | 1 / 2 |
| Head | 4 / 6 | 4 / 7 | 2 / 10 |
| UpperArmL | 7 / 22 | 15 / 24 | 10 / 39 |
| ForearmL | 6 / 16 | 9 / 20 | 7 / 54 |
| HandL | 14 / 45 | 12 / 23 | 15 / 130 |
| UpperArmR | 7 / 14 | 6 / 8 | 5 / 17 |
| ForearmR | 3 / 12 | 3 / 4 | 2 / 10 |
| HandR | 12 / 30 | 12 / 18 | 7 / 28 |
| ThighL | 5 / 10 | 4 / 7 | 5 / 10 |
| ShinL | 9 / 19 | 8 / 15 | 7 / 16 |
| FootL | 22 / 40 | 29 / 36 | 10 / 29 |
| ThighR | 3 / 8 | 4 / 7 | 3 / 9 |
| ShinR | 7 / 16 | 6 / 12 | 5 / 11 |
| FootR | 11 / 25 | 10 / 16 | 10 / 24 |

