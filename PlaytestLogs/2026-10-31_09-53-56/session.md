# Playtest 2026-10-31_09-53-56

Unity 6000.6.7f1, scene Sandbox, physics 60 Hz, solver 12/4.
Recorded 152.9 s of play (9176 physics steps), 2031 events, 1 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and ghost), `world.csv.gz` (props), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_09-53-56`.

## Orc A (club)

Present 0.0–44.2 s. Time in states: Steady 44.1 s.
Hits taken 0 (0 damage). Catch steps 19, settle steps 16.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 9.6 | Orc B, 1.39 m | Recover | 23.8 m/s | 177° | 0.25 m (Follow) | HIT Orc B UpperArmL at 8.6 m/s (Follow) |
| 2 | 14.8 | Orc B, 1.33 m | Recover | 24.3 m/s | 175° | 0.18 m (Follow) | HIT Orc B UpperArmL at 8.1 m/s (Follow) |
| 3 | 18.8 | Orc B, 1.47 m | Recover | 23.2 m/s | 166° | 0.21 m (Follow) | HIT Orc B Head at 10.0 m/s (Follow) |
| 4 | 22.8 | Orc B, 1.79 m | Recover | 18.7 m/s | 172° | 0.60 m (Follow) | HIT Orc B Chest at 6.4 m/s (Follow) |
| 5 | 32.4 | Orc B, 2.35 m | Recover | 24.3 m/s | 81° | 0.65 m (Follow) | HIT Orc B UpperArmR at 6.2 m/s (Follow) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (32 s) | Attacking (12 s) |
| --- | --- | --- |
| Pelvis | 6 / 13 | 13 / 46 |
| Spine | 1 / 6 | 6 / 21 |
| Chest | 0 / 3 | 2 / 10 |
| Head | 1 / 8 | 8 / 37 |
| UpperArmL | 0 / 9 | 3 / 15 |
| ForearmL | 1 / 13 | 6 / 21 |
| HandL | 1 / 7 | 7 / 31 |
| UpperArmR | 1 / 7 | 8 / 81 |
| ForearmR | 2 / 90 | 16 / 80 |
| HandR | 3 / 35 | 79 / 174 |
| ThighL | 3 / 27 | 3 / 10 |
| ShinL | 6 / 28 | 10 / 28 |
| FootL | 5 / 26 | 8 / 40 |
| ThighR | 3 / 10 | 5 / 18 |
| ShinR | 8 / 32 | 14 / 37 |
| FootR | 9 / 35 | 10 / 33 |


## Orc B

Present 0.0–44.2 s. Time in states: Steady 32.8 s, Flinch 0.3 s, Stagger 7.4 s, Knockdown 1.8 s, GettingUp 1.8 s.
Hits taken 6 (97 damage). Catch steps 16, settle steps 7.

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 26.2 | out of catch steps and stuck off balance for 0.4 s after a bonus step (error 0.13 m > 0.05 m) | Pelvis after 0.7 s | lying on its back | 0.80 / 0.80 m | stood up (pelvis 0.80 m, tilt 23° when the get-up ended) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (33 s) | Flinch (0 s) | Stagger (7 s) | Knockdown (2 s) | GettingUp (2 s) |
| --- | --- | --- | --- | --- | --- |
| Pelvis | 8 / 30 | 11 / 15 | 10 / 18 | 82 / 110 | 66 / 170 |
| Spine | 1 / 4 | 1 / 2 | 2 / 7 | 9 / 14 | 8 / 14 |
| Chest | 1 / 2 | 2 / 2 | 1 / 4 | 5 / 11 | 4 / 8 |
| Head | 0 / 3 | 3 / 7 | 8 / 62 | 18 / 36 | 9 / 36 |
| UpperArmL | 0 / 10 | 1 / 1 | 3 / 23 | 8 / 48 | 3 / 9 |
| ForearmL | 1 / 18 | 1 / 1 | 2 / 18 | 13 / 42 | 5 / 17 |
| HandL | 1 / 20 | 1 / 2 | 3 / 24 | 10 / 17 | 5 / 21 |
| UpperArmR | 1 / 11 | 9 / 10 | 2 / 16 | 9 / 50 | 6 / 50 |
| ForearmR | 1 / 17 | 5 / 12 | 2 / 16 | 13 / 42 | 8 / 56 |
| HandR | 1 / 20 | 3 / 6 | 2 / 17 | 11 / 26 | 7 / 34 |
| ThighL | 4 / 27 | 4 / 5 | 3 / 7 | 9 / 23 | 15 / 72 |
| ShinL | 5 / 28 | 7 / 8 | 5 / 13 | 27 / 76 | 16 / 50 |
| FootL | 5 / 25 | 9 / 10 | 10 / 20 | 15 / 36 | 14 / 40 |
| ThighR | 3 / 12 | 4 / 5 | 5 / 12 | 6 / 15 | 6 / 11 |
| ShinR | 8 / 32 | 16 / 21 | 14 / 31 | 8 / 15 | 27 / 48 |
| FootR | 10 / 35 | 14 / 26 | 16 / 35 | 12 / 29 | 30 / 85 |


## Orc A (club) #2

Present 44.2–49.7 s. Time in states: Steady 5.6 s.
Hits taken 0 (0 damage). Catch steps 6, settle steps 5.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 45.1 | Orc B #2, 1.15 m | Recover | 22.6 m/s | 180° | 0.09 m (Follow) | HIT Orc B #2 UpperArmL at 12.2 m/s (Follow) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (3 s) | Attacking (2 s) |
| --- | --- | --- |
| Pelvis | 9 / 15 | 14 / 40 |
| Spine | 2 / 6 | 6 / 18 |
| Chest | 1 / 2 | 2 / 8 |
| Head | 1 / 5 | 10 / 37 |
| UpperArmL | 1 / 7 | 4 / 16 |
| ForearmL | 1 / 13 | 6 / 23 |
| HandL | 1 / 5 | 8 / 34 |
| UpperArmR | 1 / 8 | 8 / 105 |
| ForearmR | 5 / 90 | 16 / 71 |
| HandR | 5 / 27 | 83 / 171 |
| ThighL | 4 / 27 | 4 / 11 |
| ShinL | 6 / 27 | 10 / 21 |
| FootL | 6 / 15 | 12 / 27 |
| ThighR | 4 / 9 | 6 / 17 |
| ShinR | 11 / 31 | 15 / 31 |
| FootR | 10 / 35 | 12 / 35 |


## Orc B #2

Present 44.2–49.7 s. Time in states: Steady 3.2 s, Stagger 2.3 s.
Hits taken 1 (14 damage). Catch steps 3, settle steps 2.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (3 s) | Stagger (2 s) |
| --- | --- | --- |
| Pelvis | 7 / 10 | 10 / 24 |
| Spine | 2 / 2 | 2 / 4 |
| Chest | 1 / 1 | 1 / 4 |
| Head | 1 / 3 | 3 / 22 |
| UpperArmL | 1 / 10 | 4 / 25 |
| ForearmL | 2 / 17 | 3 / 23 |
| HandL | 2 / 20 | 3 / 13 |
| UpperArmR | 1 / 10 | 2 / 15 |
| ForearmR | 2 / 17 | 2 / 16 |
| HandR | 2 / 20 | 2 / 16 |
| ThighL | 4 / 27 | 5 / 20 |
| ShinL | 7 / 27 | 8 / 21 |
| FootL | 5 / 13 | 6 / 18 |
| ThighR | 5 / 14 | 9 / 15 |
| ShinR | 7 / 31 | 8 / 15 |
| FootR | 11 / 35 | 12 / 20 |


## Orc A (club) #3

Present 49.7–54.9 s. Time in states: Steady 3.2 s, Knockdown 1.9 s, GettingUp 0.1 s.
Hits taken 0 (0 damage). Catch steps 3, settle steps 0.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 51.8 | Orc B #3, 1.59 m | Strike | 8.6 m/s | 100° | 2.36 m (Strike) | abandoned: in Strike at θ -132°: unit went into Knockdown |

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 52.9 | out of catch steps (3/3) and falling: balance error 0.29 m ≥ 3× step threshold 0.08 m | Pelvis after 0.4 s | lying on its side | 0.30 / - m | - |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (2 s) | Attacking (1 s) | Knockdown (2 s) |
| --- | --- | --- | --- |
| Pelvis | 11 / 12 | 9 / 17 | 80 / 93 |
| Spine | 1 / 3 | 2 / 3 | 2 / 7 |
| Chest | 1 / 2 | 2 / 5 | 5 / 8 |
| Head | 1 / 3 | 4 / 11 | 16 / 43 |
| UpperArmL | 1 / 7 | 2 / 5 | 7 / 18 |
| ForearmL | 1 / 13 | 3 / 8 | 10 / 35 |
| HandL | 1 / 4 | 4 / 12 | 10 / 25 |
| UpperArmR | 1 / 8 | 11 / 36 | 15 / 76 |
| ForearmR | 2 / 90 | 11 / 33 | 20 / 81 |
| HandR | 2 / 17 | 65 / 111 | 17 / 73 |
| ThighL | 3 / 27 | 5 / 8 | 6 / 22 |
| ShinL | 7 / 27 | 10 / 15 | 7 / 23 |
| FootL | 2 / 12 | 6 / 9 | 21 / 50 |
| ThighR | 5 / 9 | 2 / 7 | 5 / 18 |
| ShinR | 15 / 31 | 15 / 18 | 9 / 23 |
| FootR | 8 / 35 | 4 / 14 | 10 / 34 |


## Orc B #3

Present 49.7–54.9 s. Time in states: Steady 5.2 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (5 s) |
| --- | --- |
| Pelvis | 8 / 11 |
| Spine | 1 / 3 |
| Chest | 1 / 2 |
| Head | 1 / 3 |
| UpperArmL | 1 / 7 |
| ForearmL | 1 / 13 |
| HandL | 1 / 11 |
| UpperArmR | 1 / 9 |
| ForearmR | 1 / 13 |
| HandR | 1 / 5 |
| ThighL | 4 / 27 |
| ShinL | 16 / 27 |
| FootL | 2 / 12 |
| ThighR | 2 / 9 |
| ShinR | 11 / 31 |
| FootR | 11 / 35 |


## Goblin

Present 51.8–54.9 s. Time in states: Steady 0.6 s, Knockdown 1.5 s, GettingUp 1.0 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 0.

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 52.4 | stability 17 < knockdownBelow 25 (lost 30.7) | Chest after 0.9 s | lying on its back | 0.24 / - m | - |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (1 s) | Knockdown (2 s) | GettingUp (1 s) |
| --- | --- | --- | --- |
| Pelvis | 6 / 8 | 91 / 136 | 63 / 88 |
| Spine | 3 / 4 | 11 / 37 | 9 / 16 |
| Chest | 1 / 1 | 8 / 16 | 7 / 9 |
| Head | 1 / 1 | 23 / 66 | 9 / 33 |
| UpperArmL | 2 / 9 | 16 / 62 | 7 / 11 |
| ForearmL | 2 / 12 | 19 / 47 | 5 / 22 |
| HandL | 2 / 4 | 19 / 47 | 7 / 23 |
| UpperArmR | 1 / 7 | 14 / 51 | 7 / 47 |
| ForearmR | 3 / 11 | 14 / 65 | 8 / 48 |
| HandR | 2 / 4 | 18 / 44 | 6 / 23 |
| ThighL | 7 / 26 | 18 / 69 | 14 / 54 |
| ShinL | 10 / 26 | 23 / 72 | 15 / 39 |
| FootL | 17 / 22 | 29 / 102 | 5 / 37 |
| ThighR | 17 / 19 | 18 / 79 | 36 / 46 |
| ShinR | 8 / 29 | 13 / 108 | 45 / 63 |
| FootR | 14 / 34 | 25 / 92 | 46 / 56 |


## Orc A (club) #4

Present 54.9–83.8 s. Time in states: Steady 28.9 s.
Hits taken 0 (0 damage). Catch steps 10, settle steps 10.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 60.0 | Orc B #4, 1.56 m | Recover | 22.0 m/s | 172° | 0.28 m (Follow) | HIT Orc B #4 Chest at 11.5 m/s (Follow) |
| 2 | 64.4 | Orc B #4, 1.52 m | Recover | 21.1 m/s | 165° | 0.18 m (Follow) | HIT Orc B #4 Chest at 11.5 m/s (Follow) |
| 3 | 73.4 | Orc B #4, 2.35 m | Recover | 24.5 m/s | 56° | 0.67 m (Follow) | HIT Goblin #2 Head at 9.1 m/s (Follow) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (22 s) | Attacking (7 s) |
| --- | --- | --- |
| Pelvis | 5 / 12 | 12 / 37 |
| Spine | 2 / 4 | 3 / 11 |
| Chest | 1 / 4 | 4 / 15 |
| Head | 1 / 7 | 8 / 33 |
| UpperArmL | 1 / 9 | 5 / 18 |
| ForearmL | 0 / 12 | 4 / 16 |
| HandL | 1 / 8 | 7 / 27 |
| UpperArmR | 0 / 7 | 6 / 40 |
| ForearmR | 2 / 90 | 16 / 73 |
| HandR | 3 / 26 | 73 / 174 |
| ThighL | 4 / 25 | 4 / 11 |
| ShinL | 6 / 26 | 11 / 32 |
| FootL | 7 / 15 | 11 / 51 |
| ThighR | 10 / 19 | 9 / 26 |
| ShinR | 7 / 28 | 13 / 40 |
| FootR | 8 / 34 | 11 / 36 |


## Orc B #4

Present 54.9–83.8 s. Time in states: Steady 20.0 s, Stagger 5.6 s, Knockdown 1.5 s, GettingUp 1.8 s.
Hits taken 3 (86 damage). Catch steps 13, settle steps 8.

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 67.0 | out of catch steps (3/3) and falling: balance error 0.16 m ≥ 3× step threshold 0.05 m | HandR after 0.6 s | lying on its back | 0.84 / 0.84 m | stood up (pelvis 0.84 m, tilt 28° when the get-up ended) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (20 s) | Stagger (6 s) | Knockdown (1 s) | GettingUp (2 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 7 / 37 | 11 / 22 | 62 / 90 | 65 / 92 |
| Spine | 1 / 6 | 3 / 11 | 6 / 12 | 6 / 8 |
| Chest | 1 / 3 | 1 / 7 | 4 / 8 | 6 / 10 |
| Head | 0 / 4 | 5 / 38 | 18 / 35 | 4 / 25 |
| UpperArmL | 0 / 10 | 4 / 43 | 8 / 50 | 1 / 4 |
| ForearmL | 1 / 17 | 3 / 17 | 14 / 43 | 2 / 19 |
| HandL | 1 / 20 | 4 / 23 | 10 / 17 | 3 / 16 |
| UpperArmR | 1 / 10 | 2 / 18 | 14 / 46 | 9 / 50 |
| ForearmR | 1 / 17 | 3 / 16 | 10 / 41 | 15 / 51 |
| HandR | 2 / 35 | 4 / 21 | 9 / 18 | 22 / 40 |
| ThighL | 3 / 26 | 3 / 7 | 5 / 17 | 13 / 58 |
| ShinL | 7 / 26 | 6 / 12 | 13 / 58 | 11 / 45 |
| FootL | 7 / 30 | 16 / 25 | 14 / 34 | 3 / 32 |
| ThighR | 4 / 18 | 6 / 14 | 17 / 49 | 23 / 32 |
| ShinR | 8 / 29 | 14 / 34 | 31 / 54 | 29 / 44 |
| FootR | 11 / 34 | 21 / 39 | 22 / 33 | 37 / 80 |


## Goblin #2

Present 73.4–83.8 s. Time in states: Steady 1.5 s, Dead 8.9 s.
Hits taken 1 (45 damage). Catch steps 0, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (1 s) | Dead (9 s) |
| --- | --- | --- |
| Pelvis | 7 / 9 | 80 / 89 |
| Spine | 3 / 4 | 39 / 53 |
| Chest | 1 / 1 | 25 / 42 |
| Head | 1 / 2 | 71 / 139 |
| UpperArmL | 1 / 8 | 107 / 149 |
| ForearmL | 1 / 12 | 79 / 101 |
| HandL | 1 / 4 | 76 / 106 |
| UpperArmR | 5 / 12 | 118 / 131 |
| ForearmR | 2 / 13 | 34 / 89 |
| HandR | 1 / 15 | 66 / 79 |
| ThighL | 6 / 28 | 47 / 58 |
| ShinL | 12 / 31 | 72 / 76 |
| FootL | 20 / 22 | 42 / 47 |
| ThighR | 18 / 20 | 51 / 59 |
| ShinR | 9 / 32 | 72 / 79 |
| FootR | 12 / 35 | 43 / 46 |


## Orc A (club) #5

Present 83.8–98.2 s. Time in states: Steady 11.2 s, Knockdown 1.4 s, GettingUp 1.8 s.
Hits taken 0 (0 damage). Catch steps 14, settle steps 6.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 84.4 | Orc B #5, 1.84 m | Follow | 24.9 m/s | 168° | 0.38 m (Follow) | HIT Orc B #5 Chest at 9.3 m/s (Follow) |
| 2 | 90.4 | Orc B #5, 1.21 m | Recover | 23.5 m/s | 172° | 0.31 m (Follow) | HIT Orc B #5 UpperArmL at 5.8 m/s (Follow) |
| 3 | 94.7 | Orc B #5, 0.95 m | Recover | 20.4 m/s | 127° | 0.35 m (Follow) | HIT Orc B #5 UpperArmL at 5.8 m/s (Follow) |

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 86.0 | out of catch steps (3/3) and falling: balance error 0.71 m ≥ 3× step threshold 0.08 m | UpperArmR after 0.4 s | lying on its back | 0.82 / 0.82 m | stood up (pelvis 0.82 m, tilt 29° when the get-up ended) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (5 s) | Attacking (6 s) | Knockdown (1 s) | GettingUp (2 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 9 / 36 | 13 / 44 | 135 / 179 | 83 / 179 |
| Spine | 3 / 10 | 5 / 24 | 5 / 7 | 7 / 9 |
| Chest | 1 / 2 | 1 / 4 | 2 / 2 | 2 / 4 |
| Head | 2 / 16 | 9 / 43 | 24 / 55 | 7 / 40 |
| UpperArmL | 1 / 7 | 4 / 18 | 14 / 76 | 6 / 14 |
| ForearmL | 2 / 13 | 6 / 19 | 17 / 65 | 7 / 19 |
| HandL | 2 / 15 | 7 / 27 | 17 / 51 | 13 / 30 |
| UpperArmR | 1 / 8 | 8 / 80 | 15 / 36 | 4 / 44 |
| ForearmR | 11 / 90 | 23 / 81 | 17 / 22 | 13 / 36 |
| HandR | 2 / 22 | 79 / 169 | 17 / 78 | 2 / 9 |
| ThighL | 4 / 26 | 3 / 10 | 8 / 14 | 16 / 57 |
| ShinL | 7 / 26 | 12 / 39 | 12 / 25 | 13 / 45 |
| FootL | 10 / 33 | 11 / 38 | 14 / 29 | 18 / 33 |
| ThighR | 11 / 22 | 9 / 26 | 14 / 55 | 3 / 17 |
| ShinR | 8 / 28 | 14 / 31 | 12 / 36 | 27 / 53 |
| FootR | 8 / 34 | 9 / 22 | 20 / 43 | 31 / 88 |


## Orc B #5

Present 83.8–98.2 s. Time in states: Steady 8.7 s, Flinch 0.6 s, Stagger 5.1 s.
Hits taken 6 (52 damage). Catch steps 5, settle steps 6.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (9 s) | Flinch (1 s) | Stagger (5 s) |
| --- | --- | --- | --- |
| Pelvis | 4 / 16 | 11 / 16 | 7 / 15 |
| Spine | 2 / 4 | 2 / 3 | 3 / 6 |
| Chest | 0 / 1 | 1 / 2 | 1 / 3 |
| Head | 1 / 7 | 8 / 17 | 5 / 38 |
| UpperArmL | 1 / 15 | 13 / 21 | 2 / 16 |
| ForearmL | 1 / 18 | 6 / 14 | 3 / 21 |
| HandL | 1 / 20 | 4 / 12 | 5 / 23 |
| UpperArmR | 1 / 10 | 1 / 3 | 3 / 17 |
| ForearmR | 1 / 17 | 1 / 2 | 4 / 16 |
| HandR | 1 / 18 | 1 / 4 | 4 / 26 |
| ThighL | 3 / 26 | 3 / 7 | 4 / 7 |
| ShinL | 6 / 26 | 8 / 19 | 9 / 25 |
| FootL | 7 / 22 | 9 / 22 | 8 / 19 |
| ThighR | 10 / 19 | 11 / 16 | 10 / 21 |
| ShinR | 7 / 28 | 8 / 10 | 5 / 25 |
| FootR | 9 / 34 | 9 / 10 | 11 / 21 |


## Orc A (club) #6

Present 98.2–107.5 s. Time in states: Steady 9.3 s.
Hits taken 0 (0 damage). Catch steps 3, settle steps 4.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 99.7 | Orc B #6, 1.86 m | Recover | 21.9 m/s | 177° | 0.28 m (Follow) | HIT Orc B #6 Head at 14.3 m/s (Follow) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (7 s) | Attacking (2 s) |
| --- | --- | --- |
| Pelvis | 4 / 12 | 13 / 37 |
| Spine | 2 / 3 | 4 / 11 |
| Chest | 1 / 2 | 4 / 12 |
| Head | 1 / 4 | 8 / 31 |
| UpperArmL | 1 / 7 | 5 / 19 |
| ForearmL | 0 / 12 | 4 / 17 |
| HandL | 1 / 5 | 7 / 28 |
| UpperArmR | 0 / 7 | 8 / 30 |
| ForearmR | 2 / 90 | 17 / 60 |
| HandR | 3 / 19 | 79 / 128 |
| ThighL | 3 / 26 | 4 / 10 |
| ShinL | 11 / 27 | 14 / 23 |
| FootL | 5 / 12 | 8 / 17 |
| ThighR | 13 / 19 | 7 / 19 |
| ShinR | 7 / 29 | 14 / 31 |
| FootR | 9 / 34 | 12 / 34 |


## Orc B #6

Present 98.2–107.5 s. Time in states: Steady 5.9 s, Knockdown 1.6 s, GettingUp 1.8 s.
Hits taken 1 (71 damage). Catch steps 3, settle steps 1.

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 101.1 | stability 14 < knockdownBelow 25 (lost 86.1) | ForearmR after 0.4 s | lying on its side | 0.79 / 0.79 m | stood up (pelvis 0.79 m, tilt 23° when the get-up ended) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (6 s) | Knockdown (2 s) | GettingUp (2 s) |
| --- | --- | --- | --- |
| Pelvis | 5 / 26 | 81 / 106 | 54 / 86 |
| Spine | 2 / 3 | 3 / 9 | 6 / 10 |
| Chest | 1 / 5 | 7 / 19 | 6 / 8 |
| Head | 1 / 5 | 31 / 122 | 6 / 29 |
| UpperArmL | 1 / 7 | 14 / 52 | 3 / 8 |
| ForearmL | 0 / 12 | 14 / 46 | 2 / 7 |
| HandL | 1 / 4 | 14 / 32 | 2 / 7 |
| UpperArmR | 0 / 8 | 14 / 50 | 8 / 49 |
| ForearmR | 1 / 13 | 10 / 43 | 13 / 44 |
| HandR | 1 / 5 | 11 / 30 | 17 / 36 |
| ThighL | 4 / 26 | 6 / 12 | 9 / 56 |
| ShinL | 8 / 26 | 6 / 11 | 11 / 46 |
| FootL | 10 / 15 | 11 / 21 | 3 / 34 |
| ThighR | 13 / 31 | 9 / 32 | 39 / 49 |
| ShinR | 9 / 29 | 11 / 32 | 32 / 65 |
| FootR | 9 / 34 | 15 / 33 | 34 / 89 |


## Orc A (club) #7

Present 107.5–139.3 s. Time in states: Steady 29.1 s, Knockdown 1.4 s, GettingUp 1.2 s.
Hits taken 0 (0 damage). Catch steps 12, settle steps 4.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 110.2 | Orc B #7, 1.67 m | Recover | 17.2 m/s | 173° | 0.56 m (Follow) | HIT Orc B #7 Head at 7.2 m/s (Follow) |
| 2 | 128.2 | Orc B #7, 1.58 m | Recover | 15.5 m/s | 180° | 0.95 m (Recover) | missed: club passed 0.95 m from the target point (in Recover) |
| 3 | 133.4 | Orc B #7, 1.59 m | Recover | 18.5 m/s | 178° | 0.72 m (Follow) | missed: club passed 0.72 m from the target point (in Follow) |

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 136.6 | out of catch steps (3/3) and falling: balance error 0.30 m ≥ 3× step threshold 0.08 m | HandR after 0.4 s | lying on its back | 0.56 / - m | - |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (22 s) | Attacking (7 s) | Knockdown (1 s) | GettingUp (1 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 4 / 18 | 10 / 38 | 68 / 88 | 86 / 94 |
| Spine | 1 / 7 | 4 / 17 | 5 / 13 | 4 / 8 |
| Chest | 0 / 2 | 1 / 3 | 2 / 4 | 7 / 10 |
| Head | 0 / 6 | 6 / 29 | 16 / 31 | 5 / 23 |
| UpperArmL | 0 / 7 | 3 / 14 | 9 / 50 | 2 / 6 |
| ForearmL | 1 / 13 | 5 / 22 | 12 / 43 | 3 / 24 |
| HandL | 1 / 9 | 6 / 26 | 8 / 21 | 4 / 24 |
| UpperArmR | 1 / 8 | 34 / 179 | 14 / 36 | 9 / 43 |
| ForearmR | 1 / 90 | 8 / 28 | 4 / 11 | 2 / 10 |
| HandR | 2 / 19 | 87 / 180 | 9 / 19 | 5 / 16 |
| ThighL | 9 / 28 | 10 / 27 | 13 / 51 | 27 / 64 |
| ShinL | 2 / 37 | 7 / 21 | 12 / 50 | 15 / 51 |
| FootL | 5 / 47 | 7 / 25 | 14 / 30 | 4 / 32 |
| ThighR | 2 / 11 | 3 / 13 | 11 / 39 | 18 / 24 |
| ShinR | 13 / 34 | 18 / 32 | 18 / 60 | 49 / 64 |
| FootR | 10 / 36 | 10 / 45 | 28 / 50 | 44 / 78 |


## Orc B #7

Present 107.5–139.3 s. Time in states: Steady 30.8 s, Stagger 1.0 s.
Hits taken 1 (32 damage). Catch steps 46, settle steps 45.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (31 s) | Stagger (1 s) |
| --- | --- | --- |
| Pelvis | 15 / 25 | 13 / 16 |
| Spine | 2 / 4 | 2 / 4 |
| Chest | 2 / 4 | 2 / 3 |
| Head | 1 / 5 | 28 / 69 |
| UpperArmL | 1 / 10 | 2 / 11 |
| ForearmL | 1 / 17 | 4 / 16 |
| HandL | 1 / 19 | 5 / 22 |
| UpperArmR | 1 / 13 | 2 / 16 |
| ForearmR | 0 / 12 | 2 / 11 |
| HandR | 1 / 15 | 3 / 15 |
| ThighL | 6 / 28 | 7 / 12 |
| ShinL | 6 / 30 | 4 / 9 |
| FootL | 8 / 33 | 6 / 10 |
| ThighR | 4 / 10 | 5 / 8 |
| ShinR | 10 / 34 | 15 / 23 |
| FootR | 17 / 36 | 11 / 17 |


## Orc A (club) #8

Present 139.3–152.9 s. Time in states: Steady 13.6 s.
Hits taken 0 (0 damage). Catch steps 6, settle steps 8.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 140.3 | Orc B #8, 1.60 m | Recover | 22.0 m/s | 172° | 0.25 m (Follow) | HIT Orc B #8 Chest at 11.6 m/s (Follow) |
| 2 | 148.9 | Orc B #8, 1.01 m | Recover | 17.1 m/s | 163° | 0.05 m (Follow) | HIT Orc B #8 Chest at 9.1 m/s (Follow) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (9 s) | Attacking (5 s) |
| --- | --- | --- |
| Pelvis | 6 / 18 | 12 / 40 |
| Spine | 2 / 4 | 4 / 13 |
| Chest | 1 / 3 | 4 / 15 |
| Head | 1 / 7 | 8 / 33 |
| UpperArmL | 1 / 8 | 3 / 14 |
| ForearmL | 1 / 13 | 6 / 21 |
| HandL | 1 / 5 | 6 / 25 |
| UpperArmR | 0 / 8 | 5 / 25 |
| ForearmR | 3 / 90 | 16 / 59 |
| HandR | 3 / 27 | 80 / 171 |
| ThighL | 4 / 28 | 4 / 12 |
| ShinL | 5 / 31 | 12 / 38 |
| FootL | 7 / 35 | 10 / 29 |
| ThighR | 10 / 24 | 8 / 30 |
| ShinR | 5 / 33 | 11 / 30 |
| FootR | 9 / 35 | 10 / 38 |


## Orc B #8

Present 139.3–152.9 s. Time in states: Steady 9.0 s, Stagger 2.8 s, Knockdown 1.6 s, GettingUp 0.3 s.
Hits taken 3 (46 damage). Catch steps 4, settle steps 2.

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 151.0 | stability 16 < knockdownBelow 25 (lost 29.1) | Pelvis after 0.4 s | lying on its back | 0.33 / - m | - |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (9 s) | Stagger (3 s) | Knockdown (2 s) | GettingUp (0 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 3 / 10 | 12 / 27 | 83 / 107 | 124 / 169 |
| Spine | 0 / 3 | 3 / 8 | 6 / 13 | 12 / 19 |
| Chest | 0 / 2 | 1 / 2 | 5 / 10 | 5 / 9 |
| Head | 0 / 4 | 7 / 36 | 27 / 84 | 29 / 46 |
| UpperArmL | 1 / 13 | 3 / 16 | 11 / 47 | 22 / 25 |
| ForearmL | 0 / 12 | 2 / 12 | 12 / 43 | 15 / 28 |
| HandL | 1 / 15 | 4 / 20 | 11 / 23 | 24 / 33 |
| UpperArmR | 0 / 8 | 2 / 16 | 9 / 50 | 20 / 41 |
| ForearmR | 1 / 17 | 3 / 12 | 14 / 42 | 14 / 32 |
| HandR | 1 / 19 | 3 / 13 | 13 / 25 | 15 / 27 |
| ThighL | 5 / 29 | 3 / 6 | 5 / 16 | 24 / 47 |
| ShinL | 15 / 31 | 8 / 18 | 8 / 50 | 27 / 39 |
| FootL | 3 / 19 | 2 / 10 | 14 / 47 | 20 / 33 |
| ThighR | 2 / 10 | 5 / 13 | 5 / 15 | 18 / 38 |
| ShinR | 9 / 35 | 19 / 41 | 18 / 58 | 60 / 63 |
| FootR | 9 / 36 | 13 / 20 | 11 / 33 | 53 / 89 |

## Marks

- mark 1 at 122.592 s 

