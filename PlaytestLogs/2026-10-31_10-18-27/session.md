# Playtest 2026-10-31_10-18-27

Unity 6000.6.7f1, scene Sandbox, physics 60 Hz, solver 12/4.
Recorded 66.8 s of play (4006 physics steps), 830 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and ghost), `world.csv.gz` (props), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_10-18-27`.

## Orc A (club)

Present 0.0–8.8 s. Time in states: Steady 5.0 s, Knockdown 2.9 s, GettingUp 0.9 s.
Hits taken 0 (0 damage). Catch steps 3, settle steps 1.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 1.4 | Orc B, 1.65 m | Strike | 16.6 m/s | 121° | 1.25 m (Strike) | abandoned: in Strike at θ -24°: unit went into Knockdown |

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 5.0 | out of catch steps (3/3) and falling: balance error 0.24 m ≥ 3× step threshold 0.08 m | HandR after 0.4 s | lying on its back | 0.43 / - m | - |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (1 s) | Attacking (4 s) | Knockdown (3 s) | GettingUp (1 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 6 / 10 | 11 / 22 | 66 / 113 | 79 / 90 |
| Spine | 2 / 3 | 2 / 8 | 4 / 12 | 7 / 12 |
| Chest | 1 / 1 | 1 / 4 | 3 / 4 | 7 / 10 |
| Head | 1 / 3 | 3 / 16 | 15 / 51 | 5 / 22 |
| UpperArmL | 1 / 9 | 1 / 4 | 8 / 42 | 2 / 8 |
| ForearmL | 2 / 13 | 2 / 8 | 11 / 35 | 4 / 26 |
| HandL | 1 / 5 | 3 / 10 | 10 / 24 | 6 / 27 |
| UpperArmR | 1 / 7 | 9 / 22 | 29 / 75 | 8 / 53 |
| ForearmR | 3 / 90 | 8 / 37 | 13 / 41 | 6 / 10 |
| HandR | 2 / 17 | 73 / 122 | 26 / 77 | 6 / 14 |
| ThighL | 4 / 27 | 5 / 8 | 4 / 23 | 20 / 52 |
| ShinL | 8 / 28 | 12 / 22 | 9 / 20 | 16 / 48 |
| FootL | 6 / 12 | 6 / 19 | 10 / 36 | 5 / 35 |
| ThighR | 4 / 10 | 4 / 9 | 9 / 30 | 26 / 32 |
| ShinR | 12 / 32 | 13 / 19 | 19 / 53 | 54 / 66 |
| FootR | 11 / 35 | 6 / 13 | 25 / 48 | 49 / 75 |


## Orc B

Present 0.0–8.8 s. Time in states: Steady 8.5 s, Flinch 0.3 s.
Hits taken 1 (8 damage). Catch steps 7, settle steps 7.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (8 s) | Flinch (0 s) |
| --- | --- | --- |
| Pelvis | 9 / 13 | 13 / 15 |
| Spine | 2 / 3 | 5 / 8 |
| Chest | 1 / 2 | 4 / 6 |
| Head | 1 / 4 | 5 / 9 |
| UpperArmL | 1 / 9 | 16 / 26 |
| ForearmL | 1 / 13 | 20 / 29 |
| HandL | 1 / 28 | 51 / 80 |
| UpperArmR | 1 / 7 | 3 / 5 |
| ForearmR | 1 / 13 | 2 / 3 |
| HandR | 1 / 4 | 3 / 4 |
| ThighL | 2 / 27 | 2 / 3 |
| ShinL | 5 / 28 | 5 / 5 |
| FootL | 4 / 12 | 6 / 8 |
| ThighR | 2 / 10 | 5 / 6 |
| ShinR | 6 / 32 | 6 / 7 |
| FootR | 10 / 35 | 11 / 12 |


## Orc A (club) #2

Present 8.8–20.2 s. Time in states: Steady 8.0 s, Knockdown 1.6 s, GettingUp 1.8 s.
Hits taken 0 (0 damage). Catch steps 6, settle steps 1.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 9.7 | Orc B #2, 1.64 m | Follow | 20.4 m/s | 122° | 0.57 m (Follow) | HIT Orc B #2 HandL at 7.2 m/s (Follow) |

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 13.3 | out of catch steps (3/3) and falling: balance error 0.24 m ≥ 3× step threshold 0.08 m | HandR after 0.6 s | lying face down | 0.83 / 0.83 m | stood up (pelvis 0.83 m, tilt 18° when the get-up ended) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (4 s) | Attacking (4 s) | Knockdown (2 s) | GettingUp (2 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 8 / 30 | 12 / 26 | 46 / 69 | 41 / 52 |
| Spine | 2 / 8 | 3 / 13 | 6 / 14 | 7 / 10 |
| Chest | 1 / 3 | 1 / 6 | 2 / 4 | 3 / 5 |
| Head | 1 / 3 | 3 / 19 | 16 / 47 | 7 / 32 |
| UpperArmL | 1 / 9 | 1 / 6 | 9 / 53 | 2 / 7 |
| ForearmL | 1 / 13 | 2 / 11 | 23 / 49 | 13 / 56 |
| HandL | 1 / 5 | 3 / 15 | 34 / 73 | 20 / 71 |
| UpperArmR | 1 / 7 | 9 / 19 | 18 / 65 | 4 / 38 |
| ForearmR | 2 / 90 | 9 / 43 | 13 / 37 | 7 / 21 |
| HandR | 3 / 17 | 73 / 124 | 23 / 77 | 8 / 32 |
| ThighL | 3 / 26 | 5 / 8 | 6 / 15 | 13 / 55 |
| ShinL | 7 / 27 | 12 / 22 | 15 / 21 | 24 / 37 |
| FootL | 10 / 21 | 7 / 19 | 24 / 38 | 27 / 76 |
| ThighR | 4 / 9 | 5 / 9 | 11 / 26 | 4 / 9 |
| ShinR | 10 / 31 | 13 / 19 | 31 / 59 | 28 / 44 |
| FootR | 9 / 35 | 6 / 15 | 37 / 44 | 31 / 50 |


## Orc B #2

Present 8.8–20.2 s. Time in states: Steady 10.4 s, Stagger 1.0 s.
Hits taken 1 (11 damage). Catch steps 4, settle steps 4.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (10 s) | Stagger (1 s) |
| --- | --- | --- |
| Pelvis | 8 / 11 | 9 / 11 |
| Spine | 1 / 3 | 2 / 7 |
| Chest | 1 / 1 | 2 / 7 |
| Head | 1 / 3 | 2 / 6 |
| UpperArmL | 1 / 9 | 8 / 38 |
| ForearmL | 1 / 17 | 8 / 45 |
| HandL | 1 / 20 | 21 / 78 |
| UpperArmR | 1 / 10 | 2 / 14 |
| ForearmR | 1 / 17 | 3 / 17 |
| HandR | 1 / 20 | 4 / 20 |
| ThighL | 3 / 26 | 5 / 5 |
| ShinL | 7 / 27 | 9 / 10 |
| FootL | 5 / 12 | 7 / 10 |
| ThighR | 4 / 10 | 5 / 6 |
| ShinR | 9 / 31 | 16 / 25 |
| FootR | 10 / 35 | 9 / 16 |


## Orc A (club) #3

Present 20.2–36.9 s. Time in states: Steady 13.1 s, Knockdown 1.8 s, GettingUp 1.8 s.
Hits taken 0 (0 damage). Catch steps 11, settle steps 1.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 21.5 | Orc B #3, 1.75 m | Strike | 17.8 m/s | 123° | 0.98 m (Strike) | abandoned: in Strike at θ -25°: unit went into Knockdown |
| 2 | 31.0 | Orc B #3, 1.08 m | Recover | 10.6 m/s | 117° | 0.20 m (Recover) | HIT Orc B #3 UpperArmL at 6.9 m/s (Follow) |

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 25.1 | out of catch steps (3/3) and falling: balance error 0.23 m ≥ 3× step threshold 0.08 m | HandR after 0.5 s | lying face down | 0.81 / 0.81 m | stood up (pelvis 0.81 m, tilt 18° when the get-up ended) |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (4 s) | Attacking (9 s) | Knockdown (2 s) | GettingUp (2 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 9 / 21 | 8 / 24 | 49 / 73 | 36 / 56 |
| Spine | 2 / 3 | 2 / 6 | 4 / 9 | 2 / 5 |
| Chest | 2 / 5 | 2 / 8 | 4 / 7 | 6 / 10 |
| Head | 1 / 4 | 3 / 42 | 14 / 52 | 6 / 26 |
| UpperArmL | 1 / 9 | 2 / 16 | 13 / 40 | 4 / 11 |
| ForearmL | 1 / 13 | 5 / 22 | 28 / 56 | 16 / 77 |
| HandL | 1 / 4 | 5 / 35 | 34 / 73 | 27 / 71 |
| UpperArmR | 1 / 7 | 7 / 92 | 22 / 75 | 3 / 36 |
| ForearmR | 2 / 90 | 10 / 38 | 12 / 38 | 7 / 21 |
| HandR | 2 / 17 | 71 / 178 | 22 / 78 | 6 / 25 |
| ThighL | 4 / 27 | 3 / 11 | 5 / 14 | 19 / 56 |
| ShinL | 15 / 34 | 14 / 29 | 16 / 27 | 29 / 41 |
| FootL | 8 / 21 | 11 / 21 | 29 / 41 | 17 / 61 |
| ThighR | 3 / 9 | 3 / 11 | 12 / 34 | 5 / 8 |
| ShinR | 17 / 31 | 14 / 24 | 30 / 71 | 30 / 53 |
| FootR | 9 / 35 | 7 / 20 | 38 / 47 | 30 / 49 |


## Orc B #3

Present 20.2–36.9 s. Time in states: Steady 12.9 s, Flinch 0.3 s, Stagger 3.5 s.
Hits taken 3 (24 damage). Catch steps 8, settle steps 7.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (13 s) | Flinch (0 s) | Stagger (4 s) |
| --- | --- | --- | --- |
| Pelvis | 7 / 12 | 5 / 7 | 9 / 23 |
| Spine | 2 / 7 | 6 / 7 | 2 / 4 |
| Chest | 1 / 3 | 2 / 3 | 2 / 6 |
| Head | 1 / 3 | 2 / 4 | 2 / 14 |
| UpperArmL | 1 / 10 | 2 / 6 | 7 / 40 |
| ForearmL | 1 / 17 | 5 / 8 | 3 / 26 |
| HandL | 1 / 19 | 5 / 12 | 3 / 25 |
| UpperArmR | 1 / 10 | 1 / 2 | 1 / 13 |
| ForearmR | 1 / 19 | 2 / 3 | 2 / 16 |
| HandR | 1 / 22 | 2 / 2 | 3 / 18 |
| ThighL | 3 / 27 | 14 / 21 | 5 / 8 |
| ShinL | 11 / 27 | 15 / 20 | 14 / 19 |
| FootL | 3 / 18 | 21 / 37 | 4 / 18 |
| ThighR | 3 / 10 | 5 / 9 | 4 / 13 |
| ShinR | 14 / 33 | 11 / 13 | 19 / 32 |
| FootR | 9 / 35 | 20 / 21 | 13 / 24 |


## Orc A (club) #4

Present 36.9–53.1 s. Time in states: Steady 9.9 s, Knockdown 3.4 s, GettingUp 2.9 s.
Hits taken 0 (0 damage). Catch steps 7, settle steps 0.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 38.1 | Orc B #4, 1.84 m | Follow | 17.1 m/s | 137° | 0.56 m (Follow) | HIT Orc B #4 HandL at 6.1 m/s (Follow) |
| 2 | 46.4 | Orc B #4, 1.12 m | Follow | 10.9 m/s | 151° | 0.40 m (Follow) | HIT Orc B #4 UpperArmL at 7.1 m/s (Follow) |

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 42.0 | out of catch steps (3/3) and falling: balance error 0.26 m ≥ 3× step threshold 0.08 m | HandR after 0.5 s | lying face down | 0.84 / 0.84 m | stood up (pelvis 0.84 m, tilt 14° when the get-up ended) |
| 2 | 50.3 | out of catch steps (3/3) and falling: balance error 0.23 m ≥ 3× step threshold 0.08 m | HandL after 0.5 s | lying face down | 0.49 / - m | - |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (2 s) | Attacking (8 s) | Knockdown (3 s) | GettingUp (3 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 8 / 26 | 10 / 25 | 56 / 80 | 44 / 60 |
| Spine | 3 / 6 | 3 / 8 | 2 / 6 | 5 / 9 |
| Chest | 2 / 5 | 2 / 9 | 3 / 9 | 6 / 10 |
| Head | 1 / 3 | 3 / 18 | 13 / 31 | 8 / 34 |
| UpperArmL | 1 / 9 | 2 / 9 | 28 / 57 | 22 / 54 |
| ForearmL | 1 / 13 | 1 / 8 | 27 / 62 | 11 / 62 |
| HandL | 1 / 5 | 3 / 21 | 44 / 82 | 33 / 81 |
| UpperArmR | 1 / 7 | 5 / 16 | 18 / 68 | 4 / 41 |
| ForearmR | 2 / 90 | 8 / 40 | 20 / 70 | 8 / 29 |
| HandR | 3 / 17 | 71 / 124 | 19 / 78 | 6 / 22 |
| ThighL | 4 / 27 | 3 / 10 | 8 / 32 | 16 / 60 |
| ShinL | 12 / 29 | 12 / 22 | 29 / 69 | 25 / 43 |
| FootL | 14 / 20 | 13 / 23 | 35 / 47 | 25 / 71 |
| ThighR | 15 / 18 | 14 / 24 | 5 / 38 | 5 / 23 |
| ShinR | 8 / 31 | 9 / 23 | 9 / 34 | 31 / 60 |
| FootR | 10 / 35 | 8 / 16 | 29 / 47 | 36 / 86 |


## Orc B #4

Present 36.9–53.1 s. Time in states: Steady 12.9 s, Flinch 0.3 s, Stagger 3.0 s.
Hits taken 4 (28 damage). Catch steps 5, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (13 s) | Flinch (0 s) | Stagger (3 s) |
| --- | --- | --- | --- |
| Pelvis | 4 / 9 | 9 / 10 | 11 / 17 |
| Spine | 2 / 3 | 2 / 3 | 1 / 3 |
| Chest | 1 / 2 | 2 / 3 | 2 / 5 |
| Head | 0 / 4 | 4 / 8 | 2 / 11 |
| UpperArmL | 1 / 13 | 22 / 32 | 4 / 25 |
| ForearmL | 0 / 13 | 18 / 22 | 1 / 10 |
| HandL | 1 / 39 | 55 / 78 | 2 / 17 |
| UpperArmR | 0 / 9 | 1 / 2 | 2 / 12 |
| ForearmR | 1 / 17 | 1 / 2 | 4 / 23 |
| HandR | 0 / 19 | 2 / 2 | 7 / 47 |
| ThighL | 2 / 27 | 2 / 4 | 2 / 3 |
| ShinL | 10 / 29 | 11 / 12 | 4 / 10 |
| FootL | 12 / 16 | 14 / 17 | 15 / 22 |
| ThighR | 14 / 19 | 13 / 21 | 6 / 13 |
| ShinR | 8 / 31 | 14 / 21 | 8 / 32 |
| FootR | 9 / 35 | 10 / 15 | 23 / 32 |


## Orc A (club) #5

Present 53.1–61.4 s. Time in states: Steady 4.7 s, Knockdown 1.8 s, GettingUp 1.8 s.
Hits taken 0 (0 damage). Catch steps 5, settle steps 0.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 53.7 | Orc B #5, 1.84 m | Follow | 18.1 m/s | 156° | 0.75 m (Follow) | HIT Orc B #5 HandL at 8.1 m/s (Follow) |

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 57.4 | out of catch steps (3/3) and falling: balance error 0.33 m ≥ 3× step threshold 0.08 m | HandR after 0.4 s | lying face down | 0.84 / 0.84 m | - |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (1 s) | Attacking (4 s) | Knockdown (2 s) | GettingUp (2 s) |
| --- | --- | --- | --- | --- |
| Pelvis | 9 / 28 | 8 / 26 | 63 / 84 | 47 / 71 |
| Spine | 3 / 9 | 4 / 14 | 5 / 7 | 6 / 10 |
| Chest | 1 / 2 | 0 / 2 | 2 / 3 | 3 / 3 |
| Head | 1 / 3 | 3 / 18 | 11 / 42 | 4 / 29 |
| UpperArmL | 1 / 7 | 2 / 9 | 30 / 52 | 11 / 34 |
| ForearmL | 2 / 13 | 2 / 12 | 32 / 52 | 22 / 42 |
| HandL | 2 / 4 | 3 / 16 | 51 / 82 | 41 / 83 |
| UpperArmR | 2 / 8 | 9 / 17 | 23 / 59 | 4 / 35 |
| ForearmR | 6 / 90 | 12 / 53 | 17 / 34 | 11 / 27 |
| HandR | 1 / 17 | 73 / 124 | 18 / 77 | 1 / 6 |
| ThighL | 7 / 27 | 4 / 11 | 8 / 15 | 15 / 57 |
| ShinL | 14 / 30 | 12 / 21 | 10 / 14 | 26 / 46 |
| FootL | 13 / 22 | 13 / 23 | 26 / 46 | 25 / 67 |
| ThighR | 13 / 18 | 15 / 22 | 6 / 51 | 3 / 17 |
| ShinR | 8 / 32 | 9 / 14 | 7 / 27 | 27 / 59 |
| FootR | 11 / 35 | 7 / 11 | 26 / 45 | 31 / 87 |


## Orc B #5

Present 53.1–61.4 s. Time in states: Steady 6.2 s, Stagger 2.1 s.
Hits taken 2 (18 damage). Catch steps 2, settle steps 0.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (6 s) | Stagger (2 s) |
| --- | --- | --- |
| Pelvis | 6 / 10 | 12 / 17 |
| Spine | 2 / 4 | 2 / 6 |
| Chest | 1 / 1 | 1 / 3 |
| Head | 0 / 2 | 2 / 7 |
| UpperArmL | 0 / 10 | 5 / 33 |
| ForearmL | 1 / 17 | 7 / 49 |
| HandL | 1 / 20 | 17 / 75 |
| UpperArmR | 1 / 10 | 2 / 14 |
| ForearmR | 1 / 17 | 2 / 18 |
| HandR | 1 / 18 | 2 / 22 |
| ThighL | 4 / 27 | 2 / 5 |
| ShinL | 7 / 30 | 5 / 15 |
| FootL | 15 / 16 | 12 / 18 |
| ThighR | 13 / 18 | 5 / 21 |
| ShinR | 5 / 32 | 8 / 24 |
| FootR | 13 / 35 | 19 / 26 |


## Orc A (club) #6

Present 61.4–66.8 s. Time in states: Steady 4.6 s, Knockdown 0.8 s.
Hits taken 0 (0 damage). Catch steps 3, settle steps 0.

| Swing | At (s) | Target | Reached | Fastest tip | Worst strike lag | Closest to target | Outcome |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | 62.4 | Orc B #6, 1.72 m | Strike | 16.6 m/s | 174° | 0.72 m (Strike) | abandoned: in Strike at θ -15°: unit went into Knockdown |

| Fall | At (s) | Why | Landed on | Lying | Get-up: best / end pelvis | Result |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 66.0 | out of catch steps (3/3) and falling: balance error 0.25 m ≥ 3× step threshold 0.08 m | HandR after 0.5 s | - | never started | - |

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (1 s) | Attacking (4 s) | Knockdown (1 s) |
| --- | --- | --- | --- |
| Pelvis | 9 / 11 | 9 / 20 | 29 / 49 |
| Spine | 2 / 2 | 2 / 12 | 9 / 13 |
| Chest | 1 / 1 | 1 / 2 | 2 / 3 |
| Head | 1 / 2 | 3 / 18 | 20 / 50 |
| UpperArmL | 1 / 7 | 1 / 5 | 15 / 46 |
| ForearmL | 2 / 13 | 2 / 7 | 19 / 40 |
| HandL | 1 / 4 | 2 / 9 | 14 / 24 |
| UpperArmR | 2 / 8 | 12 / 30 | 43 / 89 |
| ForearmR | 2 / 90 | 3 / 19 | 24 / 62 |
| HandR | 2 / 17 | 73 / 124 | 34 / 77 |
| ThighL | 5 / 27 | 3 / 5 | 8 / 12 |
| ShinL | 7 / 28 | 9 / 17 | 15 / 24 |
| FootL | 3 / 12 | 8 / 11 | 22 / 34 |
| ThighR | 6 / 10 | 4 / 10 | 21 / 33 |
| ShinR | 12 / 32 | 12 / 20 | 43 / 58 |
| FootR | 13 / 35 | 7 / 17 | 43 / 47 |


## Orc B #6

Present 61.4–66.8 s. Time in states: Steady 4.6 s, Stagger 0.7 s.
Hits taken 1 (13 damage). Catch steps 5, settle steps 4.

Joint tracking error, physical vs ghost (mean / max degrees):

| Part | Idle (5 s) | Stagger (1 s) |
| --- | --- | --- |
| Pelvis | 9 / 11 | 17 / 22 |
| Spine | 2 / 3 | 2 / 3 |
| Chest | 1 / 1 | 2 / 4 |
| Head | 1 / 3 | 5 / 17 |
| UpperArmL | 0 / 7 | 22 / 39 |
| ForearmL | 1 / 13 | 9 / 33 |
| HandL | 1 / 4 | 8 / 25 |
| UpperArmR | 1 / 10 | 3 / 17 |
| ForearmR | 1 / 12 | 3 / 11 |
| HandR | 1 / 5 | 4 / 14 |
| ThighL | 3 / 27 | 2 / 4 |
| ShinL | 7 / 28 | 7 / 11 |
| FootL | 2 / 12 | 2 / 8 |
| ThighR | 3 / 10 | 7 / 10 |
| ShinR | 8 / 32 | 19 / 26 |
| FootR | 11 / 35 | 9 / 15 |

