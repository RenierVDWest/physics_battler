# Playtest 2026-10-31_10-15-07

Unity 6000.6.7f1, scene Sandbox, physics 60 Hz, solver 12/4.
Recorded 176.5 s of play (10592 physics steps), 2193 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and ghost), `world.csv.gz` (props), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_10-15-07`.

## Orc A (club)

Present 0.0–19.2 s. Time in states: Steady 15.5 s, Knockdown 1.9 s, GettingUp 1.8 s.
Hits taken 0 (0 damage). Catch steps 10, settle steps 9.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 3.3 | Orc B, 1.39 m | Recover | 23.4 m/s | 177° | 0.29 m (Follow) | HIT Orc B UpperArmL at 8.4 m/s (Follow) |
| 2 | 8.0 | Orc B, 1.65 m | Follow | 23.3 m/s | 180° | 0.44 m (Follow) | HIT Orc B Head at 11.3 m/s (Follow) |
| 3 | 15.5 | Orc B, 1.16 m | Recover | 23.1 m/s | 180° | 0.84 m (Strike) | missed: club passed 0.84 m from the target point (in Strike) |

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 9.6 | out of catch steps (3/3) and falling: balance error 0.57 m ≥ 3× step threshold 0.08 m | HandR after 0.4 s | lying face down | 0.83 / 0.83 m | stood up (pelvis 0.83 m, tilt 15° when the get-up ended) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (9 s) | Attacking (6 s) | Knockdown (2 s) | GettingUp (2 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 6 / 27 | 10 / 44 | 88 / 102 | 43 / 55 |
| Spine | 2 / 8 | 5 / 18 | 6 / 13 | 7 / 11 |
| Chest | 0 / 3 | 2 / 9 | 3 / 6 | 3 / 5 |
| Head | 1 / 49 | 7 / 48 | 14 / 49 | 4 / 34 |
| UpperArmL | 1 / 9 | 3 / 14 | 16 / 81 | 4 / 13 |
| ForearmL | 1 / 13 | 6 / 23 | 29 / 66 | 19 / 84 |
| HandL | 1 / 5 | 7 / 30 | 38 / 75 | 38 / 74 |
| UpperArmR | 1 / 7 | 24 / 169 | 17 / 32 | 3 / 34 |
| ForearmR | 3 / 90 | 23 / 62 | 24 / 53 | 8 / 27 |
| HandR | 3 / 35 | 85 / 179 | 29 / 80 | 7 / 18 |
| ThighL | 2 / 27 | 3 / 8 | 14 / 26 | 13 / 51 |
| ShinL | 8 / 40 | 11 / 20 | 16 / 30 | 26 / 34 |
| FootL | 8 / 31 | 8 / 24 | 41 / 45 | 26 / 68 |
| ThighR | 3 / 10 | 4 / 21 | 12 / 47 | 4 / 28 |
| ShinR | 9 / 32 | 15 / 35 | 10 / 26 | 27 / 58 |
| FootR | 10 / 35 | 10 / 40 | 32 / 48 | 32 / 102 |


## Orc B

Present 0.0–19.2 s. Time in states: Steady 15.8 s, Stagger 3.3 s.
Hits taken 2 (63 damage). Catch steps 3, settle steps 7.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (16 s) | Stagger (3 s) |
| --- | --- | --- |
| Pelvis | 7 / 12 | 12 / 18 |
| Spine | 1 / 3 | 3 / 9 |
| Chest | 1 / 2 | 1 / 4 |
| Head | 0 / 3 | 10 / 101 |
| UpperArmL | 0 / 10 | 3 / 22 |
| ForearmL | 1 / 17 | 3 / 17 |
| HandL | 1 / 20 | 4 / 26 |
| UpperArmR | 1 / 10 | 2 / 15 |
| ForearmR | 1 / 17 | 2 / 15 |
| HandR | 1 / 20 | 3 / 15 |
| ThighL | 3 / 27 | 2 / 6 |
| ShinL | 7 / 28 | 4 / 10 |
| FootL | 7 / 41 | 11 / 14 |
| ThighR | 4 / 11 | 4 / 14 |
| ShinR | 8 / 32 | 9 / 43 |
| FootR | 10 / 35 | 19 / 32 |


## Orc A (club) #2

Present 19.2–43.7 s. Time in states: Steady 16.6 s, Knockdown 4.3 s, GettingUp 3.6 s.
Hits taken 0 (0 damage). Catch steps 23, settle steps 11.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 19.7 | Orc B #2, 1.70 m | Strike | 21.6 m/s | 177° | 1.03 m (Strike) | abandoned: in Strike at θ 8°: unit went into Knockdown |
| 2 | 27.3 | Orc B #2, 1.70 m | Recover | 24.1 m/s | 168° | 0.42 m (Follow) | HIT Orc B #2 UpperArmL at 5.6 m/s (Follow) |
| 3 | 31.5 | Orc B #2, 1.71 m | Recover | 24.2 m/s | 155° | 0.51 m (Follow) | HIT Orc B #2 UpperArmL at 7.2 m/s (Follow) |
| 4 | 36.1 | Orc B #2, 1.82 m | Follow | 23.8 m/s | 166° | 0.52 m (Follow) | HIT Orc B #2 Chest at 6.4 m/s (Follow) |

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 21.2 | out of catch steps (3/3) and falling: balance error 0.23 m ≥ 3× step threshold 0.08 m | HandR after 0.9 s | lying on its back | 0.82 / 0.82 m | stood up (pelvis 0.82 m, tilt 28° when the get-up ended) |
| 2 | 37.9 | out of catch steps (3/3) and falling: balance error 0.33 m ≥ 3× step threshold 0.08 m | HandL after 0.3 s | lying on its back | 0.80 / 0.80 m | stood up (pelvis 0.80 m, tilt 20° when the get-up ended) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (9 s) | Attacking (8 s) | Knockdown (4 s) | GettingUp (4 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 9 / 37 | 14 / 50 | 64 / 97 | 59 / 89 |
| Spine | 2 / 7 | 6 / 22 | 5 / 11 | 6 / 10 |
| Chest | 1 / 3 | 2 / 10 | 3 / 6 | 4 / 9 |
| Head | 1 / 5 | 8 / 35 | 18 / 58 | 4 / 19 |
| UpperArmL | 1 / 7 | 3 / 15 | 13 / 75 | 2 / 12 |
| ForearmL | 1 / 13 | 6 / 22 | 13 / 60 | 3 / 26 |
| HandL | 1 / 7 | 7 / 32 | 20 / 82 | 4 / 26 |
| UpperArmR | 1 / 8 | 10 / 68 | 13 / 60 | 5 / 50 |
| ForearmR | 2 / 90 | 16 / 61 | 19 / 62 | 6 / 18 |
| HandR | 3 / 17 | 81 / 132 | 17 / 81 | 5 / 17 |
| ThighL | 3 / 28 | 4 / 10 | 7 / 30 | 13 / 56 |
| ShinL | 7 / 30 | 11 / 40 | 15 / 42 | 15 / 52 |
| FootL | 6 / 15 | 8 / 44 | 22 / 47 | 9 / 39 |
| ThighR | 5 / 15 | 5 / 16 | 12 / 36 | 14 / 34 |
| ShinR | 11 / 34 | 14 / 24 | 33 / 76 | 29 / 52 |
| FootR | 10 / 36 | 10 / 33 | 28 / 47 | 36 / 101 |


## Orc B #2

Present 19.2–43.7 s. Time in states: Steady 21.7 s, Flinch 0.6 s, Stagger 2.1 s.
Hits taken 4 (63 damage). Catch steps 3, settle steps 3.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (22 s) | Flinch (1 s) | Stagger (2 s) |
| --- | --- | --- | --- |
| Pelvis | 4 / 10 | 6 / 8 | 10 / 18 |
| Spine | 1 / 4 | 3 / 5 | 3 / 7 |
| Chest | 0 / 1 | 1 / 2 | 1 / 4 |
| Head | 0 / 10 | 11 / 20 | 5 / 27 |
| UpperArmL | 0 / 10 | 4 / 8 | 5 / 25 |
| ForearmL | 1 / 18 | 3 / 12 | 2 / 10 |
| HandL | 1 / 20 | 2 / 6 | 3 / 13 |
| UpperArmR | 1 / 9 | 1 / 2 | 2 / 17 |
| ForearmR | 1 / 16 | 1 / 2 | 2 / 16 |
| HandR | 1 / 19 | 2 / 4 | 3 / 17 |
| ThighL | 3 / 28 | 3 / 5 | 3 / 6 |
| ShinL | 9 / 30 | 10 / 13 | 7 / 10 |
| FootL | 6 / 12 | 9 / 11 | 6 / 9 |
| ThighR | 4 / 10 | 3 / 6 | 3 / 7 |
| ShinR | 9 / 34 | 11 / 14 | 11 / 28 |
| FootR | 9 / 36 | 9 / 11 | 13 / 18 |


## Orc A (club) #3

Present 43.7–54.9 s. Time in states: Steady 7.0 s, Knockdown 2.3 s, GettingUp 1.9 s.
Hits taken 0 (0 damage). Catch steps 7, settle steps 0.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 47.7 | Orc B #3, 1.58 m | Strike | 8.9 m/s | 116° | 2.28 m (Strike) | abandoned: in Strike at θ -112°: unit went into Knockdown |
| 2 | 52.4 | Orc B #3, 2.14 m | Strike | 9.7 m/s | 56° | 2.83 m (Load) | abandoned: in Strike at θ -117°: unit went into Knockdown |

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 48.9 | out of catch steps (3/3) and falling: balance error 0.23 m ≥ 3× step threshold 0.08 m | Pelvis after 0.4 s | upright | 0.79 / 0.79 m | FAILED: fell again 1.8 s after standing (out of catch steps and stuck off balance for 0.4 s after a bonus step (error 0.21 m > 0.08 m)) |
| 2 | 53.6 | out of catch steps and stuck off balance for 0.4 s after a bonus step (error 0.21 m > 0.08 m) | HandR after 0.3 s | bent over or sitting (chest tilted 41°) | 0.24 / - m | - |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (5 s) | Attacking (2 s) | Knockdown (2 s) | GettingUp (2 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 11 / 23 | 10 / 18 | 80 / 104 | 49 / 81 |
| Spine | 1 / 3 | 2 / 4 | 5 / 10 | 4 / 8 |
| Chest | 1 / 2 | 2 / 7 | 9 / 16 | 6 / 8 |
| Head | 1 / 3 | 4 / 16 | 20 / 64 | 5 / 22 |
| UpperArmL | 1 / 8 | 2 / 5 | 6 / 18 | 2 / 13 |
| ForearmL | 1 / 13 | 3 / 9 | 10 / 34 | 4 / 29 |
| HandL | 1 / 4 | 3 / 10 | 11 / 25 | 6 / 31 |
| UpperArmR | 1 / 7 | 6 / 29 | 33 / 77 | 6 / 47 |
| ForearmR | 2 / 90 | 9 / 34 | 32 / 82 | 11 / 37 |
| HandR | 2 / 17 | 59 / 110 | 49 / 77 | 11 / 36 |
| ThighL | 2 / 26 | 3 / 6 | 6 / 27 | 17 / 53 |
| ShinL | 7 / 26 | 9 / 16 | 7 / 21 | 18 / 57 |
| FootL | 2 / 12 | 7 / 10 | 21 / 46 | 4 / 49 |
| ThighR | 4 / 10 | 4 / 9 | 7 / 24 | 17 / 28 |
| ShinR | 15 / 31 | 15 / 20 | 9 / 20 | 42 / 66 |
| FootR | 8 / 34 | 5 / 14 | 19 / 50 | 37 / 129 |


## Orc B #3

Present 43.7–54.9 s. Time in states: Steady 11.2 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (11 s) |
| --- | --- |
| Pelvis | 8 / 11 |
| Spine | 1 / 3 |
| Chest | 1 / 2 |
| Head | 0 / 3 |
| UpperArmL | 0 / 8 |
| ForearmL | 1 / 13 |
| HandL | 0 / 4 |
| UpperArmR | 0 / 8 |
| ForearmR | 1 / 12 |
| HandR | 0 / 4 |
| ThighL | 4 / 26 |
| ShinL | 16 / 26 |
| FootL | 2 / 12 |
| ThighR | 1 / 9 |
| ShinR | 10 / 30 |
| FootR | 11 / 34 |


## Orc A (club) #4

Present 54.9–69.6 s. Time in states: Steady 11.6 s, Knockdown 1.4 s, GettingUp 1.8 s.
Hits taken 0 (0 damage). Catch steps 8, settle steps 3.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 55.5 | Orc B #4, 1.83 m | Recover | 21.5 m/s | 168° | 0.45 m (Follow) | HIT Orc B #4 Head at 10.5 m/s (Follow) |
| 2 | 58.7 | Orc B #4, 2.09 m | Strike | 20.7 m/s | 98° | 2.61 m (Strike) | abandoned: in Strike at θ -39°: unit went into Knockdown |
| 3 | 63.5 | Orc B #4, 1.95 m | Recover | 14.9 m/s | 178° | 1.28 m (Recover) | missed: club passed 1.28 m from the target point (in Recover) |

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 60.0 | out of catch steps (3/3) and falling: balance error 0.23 m ≥ 3× step threshold 0.08 m | HandL after 0.5 s | upright | 0.85 / 0.85 m | stood up (pelvis 0.85 m, tilt 6° when the get-up ended) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (5 s) | Attacking (6 s) | Knockdown (1 s) | GettingUp (2 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 5 / 16 | 11 / 47 | 73 / 87 | 35 / 67 |
| Spine | 2 / 4 | 3 / 7 | 8 / 12 | 5 / 8 |
| Chest | 1 / 4 | 3 / 15 | 11 / 18 | 4 / 7 |
| Head | 1 / 9 | 6 / 31 | 20 / 47 | 5 / 32 |
| UpperArmL | 1 / 9 | 4 / 19 | 12 / 35 | 3 / 11 |
| ForearmL | 1 / 12 | 3 / 17 | 12 / 37 | 2 / 9 |
| HandL | 1 / 8 | 6 / 28 | 20 / 50 | 4 / 15 |
| UpperArmR | 0 / 7 | 25 / 175 | 23 / 58 | 5 / 40 |
| ForearmR | 2 / 90 | 15 / 62 | 37 / 52 | 9 / 27 |
| HandR | 3 / 17 | 83 / 178 | 49 / 80 | 8 / 29 |
| ThighL | 4 / 26 | 4 / 8 | 15 / 30 | 12 / 51 |
| ShinL | 6 / 26 | 9 / 19 | 37 / 56 | 23 / 77 |
| FootL | 7 / 15 | 8 / 19 | 28 / 47 | 12 / 36 |
| ThighR | 10 / 18 | 9 / 27 | 18 / 27 | 10 / 16 |
| ShinR | 3 / 28 | 13 / 29 | 54 / 76 | 27 / 39 |
| FootR | 9 / 34 | 9 / 28 | 28 / 45 | 32 / 61 |


## Orc B #4

Present 54.9–69.6 s. Time in states: Steady 10.2 s, Stagger 4.6 s.
Hits taken 3 (65 damage). Catch steps 4, settle steps 2.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (10 s) | Stagger (5 s) |
| --- | --- | --- |
| Pelvis | 5 / 7 | 7 / 19 |
| Spine | 2 / 3 | 2 / 6 |
| Chest | 1 / 2 | 1 / 6 |
| Head | 0 / 2 | 7 / 97 |
| UpperArmL | 1 / 13 | 4 / 47 |
| ForearmL | 0 / 12 | 3 / 18 |
| HandL | 1 / 16 | 4 / 60 |
| UpperArmR | 1 / 9 | 1 / 13 |
| ForearmR | 1 / 17 | 2 / 19 |
| HandR | 1 / 19 | 2 / 25 |
| ThighL | 2 / 26 | 2 / 5 |
| ShinL | 9 / 26 | 8 / 17 |
| FootL | 9 / 16 | 8 / 16 |
| ThighR | 12 / 18 | 10 / 21 |
| ShinR | 9 / 28 | 15 / 34 |
| FootR | 8 / 34 | 11 / 19 |


## Orc A (club) #5

Present 69.6–80.2 s. Time in states: Steady 10.6 s.
Hits taken 0 (0 damage). Catch steps 4, settle steps 5.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 71.7 | Orc B #5, 1.84 m | Recover | 25.1 m/s | 176° | 0.11 m (Follow) | HIT Orc B #5 UpperArmL at 12.0 m/s (Follow) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (8 s) | Attacking (2 s) |
| --- | --- | --- |
| Pelvis | 6 / 15 | 14 / 44 |
| Spine | 2 / 4 | 5 / 17 |
| Chest | 1 / 1 | 1 / 3 |
| Head | 0 / 3 | 9 / 31 |
| UpperArmL | 0 / 7 | 4 / 17 |
| ForearmL | 1 / 13 | 6 / 21 |
| HandL | 1 / 5 | 7 / 30 |
| UpperArmR | 1 / 8 | 9 / 30 |
| ForearmR | 2 / 90 | 27 / 88 |
| HandR | 1 / 17 | 78 / 129 |
| ThighL | 4 / 26 | 3 / 10 |
| ShinL | 5 / 27 | 11 / 21 |
| FootL | 8 / 15 | 10 / 22 |
| ThighR | 10 / 19 | 11 / 23 |
| ShinR | 7 / 29 | 9 / 22 |
| FootR | 9 / 34 | 10 / 33 |


## Orc B #5

Present 69.6–80.2 s. Time in states: Steady 6.2 s, Stagger 0.8 s, Knockdown 1.8 s, GettingUp 1.8 s.
Hits taken 1 (26 damage). Catch steps 5, settle steps 1.

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 74.0 | out of catch steps (3/3) and falling: balance error 0.15 m ≥ 3× step threshold 0.05 m | Pelvis after 0.5 s | lying on its back | 0.79 / 0.79 m | stood up (pelvis 0.79 m, tilt 23° when the get-up ended) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (6 s) | Stagger (1 s) | Knockdown (2 s) | GettingUp (2 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 6 / 26 | 19 / 23 | 78 / 106 | 61 / 169 |
| Spine | 3 / 4 | 3 / 5 | 6 / 12 | 5 / 19 |
| Chest | 1 / 3 | 1 / 3 | 4 / 10 | 4 / 11 |
| Head | 0 / 3 | 7 / 17 | 18 / 36 | 9 / 39 |
| UpperArmL | 0 / 7 | 16 / 35 | 8 / 46 | 5 / 16 |
| ForearmL | 1 / 12 | 8 / 30 | 13 / 40 | 5 / 32 |
| HandL | 1 / 4 | 8 / 22 | 10 / 21 | 6 / 32 |
| UpperArmR | 1 / 8 | 2 / 14 | 9 / 49 | 8 / 38 |
| ForearmR | 1 / 13 | 3 / 16 | 14 / 44 | 14 / 31 |
| HandR | 0 / 4 | 4 / 15 | 11 / 17 | 21 / 40 |
| ThighL | 4 / 26 | 2 / 4 | 7 / 37 | 11 / 53 |
| ShinL | 10 / 27 | 6 / 11 | 11 / 54 | 12 / 37 |
| FootL | 11 / 15 | 13 / 21 | 17 / 48 | 6 / 32 |
| ThighR | 14 / 29 | 7 / 21 | 13 / 53 | 37 / 49 |
| ShinR | 7 / 29 | 16 / 35 | 11 / 59 | 32 / 61 |
| FootR | 9 / 34 | 22 / 32 | 16 / 46 | 35 / 81 |


## Orc A (club) #6

Present 80.2–90.3 s. Time in states: Steady 10.1 s.
Hits taken 0 (0 damage). Catch steps 2, settle steps 4.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 83.2 | Orc B #6, 1.60 m | Recover | 17.0 m/s | 179° | 0.56 m (Follow) | HIT Orc B #6 Head at 5.2 m/s (Follow) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (8 s) | Attacking (2 s) |
| --- | --- | --- |
| Pelvis | 4 / 9 | 11 / 34 |
| Spine | 1 / 3 | 6 / 18 |
| Chest | 0 / 1 | 1 / 3 |
| Head | 0 / 2 | 7 / 28 |
| UpperArmL | 0 / 9 | 4 / 16 |
| ForearmL | 1 / 13 | 5 / 21 |
| HandL | 1 / 8 | 6 / 25 |
| UpperArmR | 1 / 7 | 17 / 51 |
| ForearmR | 1 / 90 | 7 / 21 |
| HandR | 2 / 17 | 80 / 128 |
| ThighL | 4 / 27 | 6 / 15 |
| ShinL | 13 / 27 | 15 / 28 |
| FootL | 2 / 12 | 7 / 44 |
| ThighR | 1 / 9 | 3 / 14 |
| ShinR | 10 / 31 | 13 / 23 |
| FootR | 11 / 35 | 8 / 14 |


## Orc B #6

Present 80.2–90.3 s. Time in states: Steady 9.8 s, Flinch 0.3 s.
Hits taken 1 (22 damage). Catch steps 1, settle steps 1.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (10 s) | Flinch (0 s) |
| --- | --- | --- |
| Pelvis | 6 / 13 | 9 / 13 |
| Spine | 1 / 3 | 2 / 2 |
| Chest | 1 / 2 | 1 / 2 |
| Head | 1 / 43 | 48 / 57 |
| UpperArmL | 0 / 9 | 1 / 1 |
| ForearmL | 1 / 13 | 1 / 2 |
| HandL | 0 / 4 | 2 / 2 |
| UpperArmR | 1 / 7 | 1 / 2 |
| ForearmR | 0 / 12 | 0 / 1 |
| HandR | 0 / 4 | 1 / 2 |
| ThighL | 5 / 27 | 5 / 9 |
| ShinL | 12 / 27 | 9 / 14 |
| FootL | 6 / 12 | 5 / 11 |
| ThighR | 5 / 9 | 8 / 9 |
| ShinR | 7 / 31 | 10 / 13 |
| FootR | 8 / 35 | 9 / 9 |


## Orc A (club) #7

Present 90.3–113.7 s. Time in states: Steady 23.4 s.
Hits taken 0 (0 damage). Catch steps 15, settle steps 10.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 94.5 | Orc B #7, 1.66 m | Recover | 17.0 m/s | 175° | 0.56 m (Follow) | HIT Orc B #7 Head at 7.2 m/s (Follow) |
| 2 | 101.8 | Orc B #7, 1.66 m | Recover | 20.3 m/s | 171° | 0.81 m (Recover) | missed: club passed 0.81 m from the target point (in Recover) |
| 3 | 106.4 | Orc B #7, 1.66 m | Recover | 17.0 m/s | 175° | 0.67 m (Follow) | missed: club passed 0.67 m from the target point (in Follow) |
| 4 | 110.7 | Orc B #7, 1.73 m | Recover | 16.7 m/s | 180° | 0.68 m (Follow) | missed: club passed 0.68 m from the target point (in Follow) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (14 s) | Attacking (10 s) |
| --- | --- | --- |
| Pelvis | 7 / 14 | 12 / 43 |
| Spine | 1 / 4 | 5 / 19 |
| Chest | 0 / 1 | 1 / 4 |
| Head | 1 / 6 | 6 / 26 |
| UpperArmL | 0 / 7 | 3 / 14 |
| ForearmL | 1 / 13 | 5 / 21 |
| HandL | 1 / 8 | 6 / 26 |
| UpperArmR | 2 / 9 | 21 / 179 |
| ForearmR | 1 / 90 | 8 / 30 |
| HandR | 3 / 21 | 86 / 180 |
| ThighL | 7 / 28 | 8 / 24 |
| ShinL | 1 / 29 | 7 / 21 |
| FootL | 5 / 12 | 8 / 27 |
| ThighR | 2 / 10 | 3 / 19 |
| ShinR | 15 / 34 | 18 / 35 |
| FootR | 8 / 36 | 9 / 51 |


## Orc B #7

Present 90.3–113.7 s. Time in states: Steady 22.4 s, Stagger 0.9 s.
Hits taken 1 (32 damage). Catch steps 30, settle steps 28.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (22 s) | Stagger (1 s) |
| --- | --- | --- |
| Pelvis | 13 / 19 | 13 / 16 |
| Spine | 2 / 4 | 2 / 4 |
| Chest | 2 / 3 | 2 / 3 |
| Head | 1 / 5 | 28 / 69 |
| UpperArmL | 1 / 9 | 2 / 11 |
| ForearmL | 1 / 17 | 4 / 16 |
| HandL | 1 / 18 | 5 / 22 |
| UpperArmR | 1 / 14 | 3 / 16 |
| ForearmR | 0 / 12 | 2 / 11 |
| HandR | 1 / 16 | 4 / 15 |
| ThighL | 6 / 28 | 6 / 11 |
| ShinL | 5 / 29 | 5 / 9 |
| FootL | 7 / 30 | 6 / 10 |
| ThighR | 4 / 10 | 5 / 8 |
| ShinR | 10 / 34 | 16 / 23 |
| FootR | 15 / 36 | 11 / 18 |


## Orc A (club) #8

Present 113.7–122.6 s. Time in states: Steady 8.9 s.
Hits taken 0 (0 damage). Catch steps 3, settle steps 2.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 114.5 | Orc B #8, 1.83 m | Recover | 22.7 m/s | 176° | 0.16 m (Follow) | HIT Orc B #8 Chest at 11.8 m/s (Follow) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (7 s) | Attacking (2 s) |
| --- | --- | --- |
| Pelvis | 3 / 7 | 13 / 31 |
| Spine | 0 / 1 | 2 / 5 |
| Chest | 1 / 2 | 4 / 16 |
| Head | 1 / 3 | 9 / 38 |
| UpperArmL | 1 / 8 | 4 / 18 |
| ForearmL | 0 / 12 | 4 / 16 |
| HandL | 1 / 6 | 6 / 28 |
| UpperArmR | 0 / 8 | 8 / 30 |
| ForearmR | 2 / 90 | 17 / 65 |
| HandR | 2 / 18 | 80 / 130 |
| ThighL | 8 / 28 | 11 / 28 |
| ShinL | 3 / 30 | 10 / 23 |
| FootL | 5 / 14 | 11 / 41 |
| ThighR | 8 / 17 | 11 / 32 |
| ShinR | 7 / 33 | 11 / 52 |
| FootR | 8 / 35 | 13 / 30 |


## Orc B #8

Present 113.7–122.6 s. Time in states: Steady 4.9 s, Stagger 0.8 s, Knockdown 1.4 s, GettingUp 1.8 s.
Hits taken 1 (36 damage). Catch steps 5, settle steps 1.

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 116.7 | out of catch steps (3/3) and falling: balance error 0.21 m ≥ 3× step threshold 0.05 m | Pelvis after 0.4 s | lying on its back | 0.79 / 0.79 m | stood up (pelvis 0.79 m, tilt 23° when the get-up ended) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (5 s) | Stagger (1 s) | Knockdown (1 s) | GettingUp (2 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 6 / 26 | 16 / 20 | 66 / 92 | 54 / 92 |
| Spine | 0 / 1 | 2 / 4 | 3 / 5 | 3 / 5 |
| Chest | 1 / 5 | 4 / 8 | 5 / 9 | 6 / 7 |
| Head | 1 / 4 | 13 / 38 | 17 / 36 | 6 / 27 |
| UpperArmL | 1 / 8 | 5 / 14 | 11 / 47 | 3 / 9 |
| ForearmL | 0 / 12 | 4 / 12 | 14 / 41 | 2 / 17 |
| HandL | 1 / 4 | 7 / 20 | 11 / 21 | 2 / 17 |
| UpperArmR | 1 / 8 | 3 / 16 | 16 / 50 | 9 / 47 |
| ForearmR | 1 / 13 | 4 / 11 | 12 / 44 | 14 / 45 |
| HandR | 1 / 5 | 5 / 12 | 11 / 27 | 21 / 40 |
| ThighL | 10 / 28 | 4 / 8 | 12 / 39 | 13 / 64 |
| ShinL | 2 / 30 | 2 / 4 | 13 / 59 | 11 / 47 |
| FootL | 9 / 14 | 13 / 23 | 18 / 47 | 3 / 30 |
| ThighR | 13 / 31 | 8 / 20 | 22 / 58 | 38 / 48 |
| ShinR | 9 / 33 | 22 / 43 | 12 / 59 | 30 / 63 |
| FootR | 10 / 35 | 19 / 29 | 31 / 37 | 37 / 109 |


## Orc A (club) #9

Present 122.6–136.5 s. Time in states: Steady 4.2 s, Knockdown 3.4 s, GettingUp 6.3 s.
Hits taken 0 (0 damage). Catch steps 6, settle steps 0.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 124.4 | Orc B #9, 1.85 m | Follow | 25.0 m/s | 170° | 0.42 m (Follow) | HIT Orc B #9 Chest at 8.3 m/s (Follow) |

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 126.0 | out of catch steps (3/3) and falling: balance error 0.66 m ≥ 3× step threshold 0.08 m | ShinR after 0.5 s | bent over or sitting (chest tilted 64°) | 0.78 / 0.78 m | FAILED: fell again 0.8 s after standing (out of catch steps (3/3) and falling: balance error 0.33 m ≥ 3× step threshold 0.08 m) |
| 2 | 129.8 | out of catch steps (3/3) and falling: balance error 0.33 m ≥ 3× step threshold 0.08 m | Pelvis after 0.4 s | lying on its back | 0.77 / - m | FAILED: knocked down again while getting up |
| 3 | 134.3 | get-up failed after 3.3 s: pelvis 0.77 m (needs 0.74), chest tilt 39° (needs ≤ 35) | Pelvis after 0.3 s | bent over or sitting (chest tilted 50°) | 0.45 / - m | - |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (3 s) | Attacking (2 s) | Knockdown (3 s) | GettingUp (6 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 12 / 31 | 15 / 45 | 90 / 159 | 53 / 90 |
| Spine | 3 / 4 | 5 / 16 | 4 / 13 | 5 / 15 |
| Chest | 1 / 2 | 1 / 3 | 2 / 3 | 3 / 7 |
| Head | 1 / 2 | 9 / 29 | 18 / 54 | 5 / 45 |
| UpperArmL | 1 / 9 | 4 / 11 | 14 / 76 | 2 / 13 |
| ForearmL | 1 / 13 | 6 / 16 | 14 / 66 | 3 / 30 |
| HandL | 1 / 5 | 6 / 23 | 12 / 53 | 3 / 34 |
| UpperArmR | 1 / 7 | 11 / 28 | 14 / 48 | 5 / 54 |
| ForearmR | 7 / 90 | 25 / 81 | 22 / 42 | 16 / 59 |
| HandR | 2 / 17 | 79 / 131 | 9 / 78 | 3 / 9 |
| ThighL | 4 / 27 | 2 / 6 | 7 / 17 | 9 / 57 |
| ShinL | 8 / 30 | 11 / 20 | 8 / 22 | 10 / 53 |
| FootL | 11 / 15 | 10 / 21 | 12 / 34 | 3 / 29 |
| ThighR | 18 / 29 | 9 / 27 | 15 / 43 | 35 / 60 |
| ShinR | 10 / 32 | 13 / 18 | 17 / 43 | 26 / 68 |
| FootR | 13 / 35 | 7 / 16 | 27 / 43 | 32 / 113 |


## Orc B #9

Present 122.6–136.5 s. Time in states: Steady 8.3 s, Flinch 0.3 s, Stagger 5.3 s.
Hits taken 4 (36 damage). Catch steps 5, settle steps 2.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (8 s) | Flinch (0 s) | Stagger (5 s) |
| --- | --- | --- | --- |
| Pelvis | 5 / 8 | 3 / 6 | 5 / 10 |
| Spine | 3 / 5 | 2 / 3 | 2 / 5 |
| Chest | 1 / 2 | 1 / 2 | 1 / 2 |
| Head | 1 / 4 | 3 / 5 | 6 / 37 |
| UpperArmL | 9 / 25 | 6 / 7 | 2 / 16 |
| ForearmL | 11 / 31 | 8 / 9 | 3 / 14 |
| HandL | 11 / 45 | 2 / 8 | 4 / 28 |
| UpperArmR | 1 / 9 | 1 / 2 | 2 / 16 |
| ForearmR | 1 / 17 | 1 / 2 | 2 / 18 |
| HandR | 1 / 18 | 1 / 2 | 2 / 20 |
| ThighL | 3 / 27 | 7 / 11 | 3 / 6 |
| ShinL | 8 / 30 | 6 / 12 | 7 / 15 |
| FootL | 10 / 15 | 4 / 6 | 7 / 15 |
| ThighR | 14 / 18 | 7 / 8 | 7 / 20 |
| ShinR | 9 / 32 | 10 / 10 | 13 / 25 |
| FootR | 9 / 35 | 8 / 9 | 10 / 27 |


## Orc A (club) #10

Present 136.5–150.0 s. Time in states: Steady 13.5 s.
Hits taken 0 (0 damage). Catch steps 11, settle steps 7.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 139.3 | Orc B #10, 1.56 m | Recover | 25.1 m/s | 173° | 0.15 m (Follow) | HIT Orc B #10 UpperArmL at 11.8 m/s (Follow) |
| 2 | 144.0 | Orc B #10, 1.39 m | Recover | 21.5 m/s | 160° | 0.15 m (Follow) | HIT Orc B #10 UpperArmL at 10.2 m/s (Follow) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (9 s) | Attacking (5 s) |
| --- | --- | --- |
| Pelvis | 9 / 20 | 16 / 47 |
| Spine | 2 / 7 | 7 / 21 |
| Chest | 1 / 2 | 1 / 3 |
| Head | 1 / 11 | 9 / 34 |
| UpperArmL | 1 / 8 | 3 / 12 |
| ForearmL | 1 / 13 | 6 / 20 |
| HandL | 1 / 9 | 6 / 25 |
| UpperArmR | 1 / 8 | 8 / 27 |
| ForearmR | 5 / 90 | 27 / 87 |
| HandR | 1 / 19 | 79 / 169 |
| ThighL | 4 / 29 | 6 / 13 |
| ShinL | 10 / 31 | 15 / 55 |
| FootL | 5 / 24 | 9 / 35 |
| ThighR | 3 / 10 | 5 / 18 |
| ShinR | 11 / 35 | 14 / 23 |
| FootR | 10 / 36 | 10 / 26 |


## Orc B #10

Present 136.5–150.0 s. Time in states: Steady 7.1 s, Stagger 2.8 s, Knockdown 1.8 s, GettingUp 1.8 s.
Hits taken 3 (47 damage). Catch steps 4, settle steps 2.

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 146.1 | stability 0 < knockdownBelow 25 (lost 47.1) | Pelvis after 0.4 s | lying on its back | 0.79 / 0.79 m | - |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (7 s) | Stagger (3 s) | Knockdown (2 s) | GettingUp (2 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 6 / 22 | 12 / 26 | 87 / 116 | 50 / 165 |
| Spine | 1 / 3 | 2 / 4 | 6 / 10 | 8 / 19 |
| Chest | 1 / 2 | 1 / 3 | 4 / 11 | 4 / 10 |
| Head | 0 / 3 | 4 / 19 | 32 / 110 | 10 / 42 |
| UpperArmL | 1 / 9 | 9 / 35 | 9 / 46 | 5 / 17 |
| ForearmL | 1 / 17 | 4 / 25 | 12 / 43 | 7 / 21 |
| HandL | 1 / 18 | 3 / 17 | 11 / 25 | 8 / 25 |
| UpperArmR | 1 / 10 | 2 / 15 | 9 / 49 | 7 / 54 |
| ForearmR | 1 / 17 | 2 / 16 | 14 / 47 | 8 / 63 |
| HandR | 1 / 18 | 3 / 18 | 12 / 25 | 8 / 43 |
| ThighL | 4 / 29 | 3 / 5 | 5 / 25 | 14 / 76 |
| ShinL | 14 / 31 | 14 / 19 | 9 / 52 | 27 / 54 |
| FootL | 3 / 13 | 3 / 12 | 15 / 51 | 9 / 42 |
| ThighR | 2 / 10 | 4 / 7 | 8 / 35 | 5 / 18 |
| ShinR | 8 / 35 | 17 / 34 | 10 / 35 | 29 / 55 |
| FootR | 10 / 36 | 11 / 20 | 12 / 32 | 32 / 86 |


## Orc A (club) #11

Present 150.0–164.0 s. Time in states: Steady 10.2 s, Knockdown 2.6 s, GettingUp 1.2 s.
Hits taken 0 (0 damage). Catch steps 3, settle steps 2.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 158.4 | Orc B #11, 1.42 m | Follow | 17.4 m/s | 178° | 0.49 m (Follow) | HIT Orc B #11 Head at 4.1 m/s (Follow) |

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 160.2 | out of catch steps (3/3) and falling: balance error 0.25 m ≥ 3× step threshold 0.08 m | HandL after 0.5 s | lying face down | 0.61 / - m | - |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (8 s) | Attacking (2 s) | Knockdown (3 s) | GettingUp (1 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 9 / 11 | 11 / 33 | 86 / 105 | 69 / 110 |
| Spine | 1 / 2 | 6 / 17 | 5 / 9 | 8 / 13 |
| Chest | 1 / 1 | 1 / 2 | 3 / 6 | 2 / 5 |
| Head | 1 / 3 | 8 / 30 | 12 / 45 | 7 / 21 |
| UpperArmL | 0 / 7 | 3 / 13 | 48 / 72 | 30 / 41 |
| ForearmL | 1 / 13 | 6 / 21 | 29 / 65 | 15 / 21 |
| HandL | 0 / 4 | 6 / 24 | 41 / 60 | 66 / 82 |
| UpperArmR | 1 / 8 | 20 / 51 | 14 / 37 | 11 / 41 |
| ForearmR | 1 / 90 | 8 / 21 | 13 / 64 | 3 / 7 |
| HandR | 2 / 17 | 85 / 131 | 13 / 80 | 8 / 24 |
| ThighL | 4 / 27 | 5 / 13 | 10 / 36 | 21 / 53 |
| ShinL | 16 / 28 | 19 / 58 | 14 / 26 | 20 / 53 |
| FootL | 1 / 12 | 7 / 46 | 36 / 46 | 29 / 64 |
| ThighR | 2 / 10 | 4 / 17 | 6 / 46 | 5 / 22 |
| ShinR | 11 / 33 | 15 / 37 | 8 / 30 | 36 / 63 |
| FootR | 11 / 35 | 10 / 32 | 18 / 43 | 41 / 76 |


## Orc B #11

Present 150.0–164.0 s. Time in states: Steady 13.7 s, Flinch 0.3 s.
Hits taken 1 (14 damage). Catch steps 0, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (14 s) | Flinch (0 s) |
| --- | --- | --- |
| Pelvis | 8 / 11 | 10 / 11 |
| Spine | 1 / 2 | 2 / 3 |
| Chest | 1 / 1 | 1 / 1 |
| Head | 1 / 41 | 46 / 55 |
| UpperArmL | 0 / 7 | 0 / 0 |
| ForearmL | 1 / 13 | 1 / 1 |
| HandL | 0 / 4 | 1 / 2 |
| UpperArmR | 1 / 10 | 1 / 1 |
| ForearmR | 0 / 12 | 0 / 0 |
| HandR | 0 / 5 | 1 / 1 |
| ThighL | 4 / 27 | 3 / 4 |
| ShinL | 16 / 28 | 16 / 17 |
| FootL | 2 / 12 | 1 / 2 |
| ThighR | 1 / 10 | 3 / 3 |
| ShinR | 10 / 33 | 11 / 12 |
| FootR | 11 / 35 | 11 / 11 |


## Orc A (club) #12

Present 164.0–176.5 s. Time in states: Steady 12.5 s.
Hits taken 0 (0 damage). Catch steps 6, settle steps 6.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 165.6 | Orc B #12, 1.47 m | Recover | 17.3 m/s | 178° | 0.53 m (Follow) | missed: club passed 0.53 m from the target point (in Follow) |
| 2 | 173.0 | Orc B #12, 1.44 m | Recover | 16.6 m/s | 175° | 0.63 m (Follow) | missed: club passed 0.63 m from the target point (in Follow) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (8 s) | Attacking (5 s) |
| --- | --- | --- |
| Pelvis | 5 / 10 | 12 / 38 |
| Spine | 0 / 1 | 2 / 6 |
| Chest | 1 / 3 | 3 / 14 |
| Head | 1 / 4 | 7 / 31 |
| UpperArmL | 0 / 7 | 3 / 14 |
| ForearmL | 1 / 13 | 6 / 22 |
| HandL | 1 / 7 | 6 / 25 |
| UpperArmR | 2 / 8 | 18 / 55 |
| ForearmR | 1 / 90 | 7 / 20 |
| HandR | 3 / 17 | 80 / 133 |
| ThighL | 5 / 27 | 7 / 13 |
| ShinL | 2 / 27 | 9 / 20 |
| FootL | 5 / 13 | 9 / 30 |
| ThighR | 3 / 9 | 4 / 15 |
| ShinR | 8 / 31 | 14 / 35 |
| FootR | 11 / 35 | 9 / 19 |


## Orc B #12

Present 164.0–176.5 s. Time in states: Steady 12.5 s.
Hits taken 0 (0 damage). Catch steps 0, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (13 s) |
| --- | --- |
| Pelvis | 8 / 10 |
| Spine | 0 / 1 |
| Chest | 1 / 2 |
| Head | 0 / 3 |
| UpperArmL | 0 / 7 |
| ForearmL | 1 / 12 |
| HandL | 0 / 4 |
| UpperArmR | 1 / 9 |
| ForearmR | 0 / 12 |
| HandR | 0 / 5 |
| ThighL | 12 / 27 |
| ShinL | 3 / 27 |
| FootL | 6 / 12 |
| ThighR | 2 / 9 |
| ShinR | 10 / 31 |
| FootR | 11 / 35 |

