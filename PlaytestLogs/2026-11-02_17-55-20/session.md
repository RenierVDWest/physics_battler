# Playtest 2026-11-02_17-55-20

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 31.1 s of play (1867 physics steps), 2397 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-02_17-55-20`.

## A Small

Present 0.0–31.1 s. Steps 15, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 6 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 4 / 23 |
| Chest | ? | 0.8 / 4.5 | 8 / 31 |
| Head | ? | 1.6 / 6.4 | 13 / 69 |
| UpperArmL | ? | 1.1 / 6.7 | 7 / 133 |
| ForearmL | ? | 1.7 / 16.3 | 9 / 98 |
| HandL | ? | 2.5 / 36.2 | 22 / 125 |
| UpperArmR | ? | 1.5 / 7.0 | 9 / 49 |
| ForearmR | ? | 2.9 / 19.5 | 16 / 78 |
| HandR | ? | 7.4 / 32.3 | 20 / 99 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 3, 54 damage. Health at the end 0.

Defence: 3 blows came at it (1 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 99% of the time; the worst 43° (ForearmL).
Ragdoll (on the ground 25.8 s): a joint more than 10° past its limit 2% of the time; the worst 86° (ShinL). Fastest part with a standing leg within 0.5 m: 3.3 m/s median, 6.3 m/s at the 95th percentile (75 steps); with none: 0.0 m/s median (1470 steps).

Look: within 25° of what it attended to 75% of the time standing (76% of the time it was within the head's reach, 96% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 2 postures (StickSettle 1, AbanicoFlurry 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–31.1 s. Steps 25, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 18 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.5 | 5 / 121 |
| Chest | Ragdoll | 1.1 / 13.1 | 8 / 41 |
| Head | Ragdoll | 2.0 / 10.1 | 18 / 179 |
| UpperArmL | Ragdoll | 1.4 / 17.8 | 6 / 179 |
| ForearmL | Ragdoll | 2.1 / 26.5 | 8 / 114 |
| HandL | Ragdoll | 2.6 / 52.2 | 4 / 67 |
| UpperArmR | Ragdoll | 2.5 / 19.4 | 9 / 56 |
| ForearmR | Ragdoll | 5.0 / 32.9 | 13 / 88 |
| HandR | Ragdoll | 7.2 / 57.3 | 16 / 107 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 15 damage. Hits taken: 10, 69 damage. Killed.

Defence: 5 blows came at it (4 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 2, and had nothing to defend with against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 75% of the time; the worst 35° (FootL).
Ragdoll (on the ground 18.5 s): a joint more than 10° past its limit 8% of the time; the worst 131° (FootL). Fastest part with a standing leg within 0.5 m: 0.1 m/s median, 5.8 m/s at the 95th percentile (669 steps); with none: 0.0 m/s median (443 steps).

Look: within 25° of what it attended to 81% of the time standing (81% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 83% of the time standing; its rim came within 6.0 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 3 postures (GuardChange 1, HighFeint 1, Peek 1), 1 of them lead-ins to blows; energy at the end 0.79.

Balance: stumbled 4 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Large

Present 0.0–31.1 s. Steps 62, idle shuffles 0. Feet slid at most 3.8 cm while planted; the weight was at most 50 cm from the support point; hips dropped for reach by up to 26 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 2 / 40 |
| Chest | Pinned | 0.9 / 16.7 | 3 / 25 |
| Head | Pinned | 1.3 / 20.2 | 13 / 49 |
| UpperArmL | Pinned | 1.6 / 19.6 | 6 / 52 |
| ForearmL | Pinned | 3.8 / 24.2 | 9 / 61 |
| HandL | Pinned | 4.9 / 44.9 | 23 / 117 |
| UpperArmR | Muscled | 1.7 / 21.6 | 3 / 30 |
| ForearmR | Muscled | 2.8 / 29.0 | 4 / 64 |
| HandR | Muscled | 3.7 / 52.0 | 6 / 79 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 150 |
| ShinL | Kinematic | 0.0 / 11.9 | 0 / 12 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 2 |
| ShinR | Kinematic | 0.0 / 13.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 11.4 | 0 / 0 |

Hits dealt: 8 (3 on the part aimed at, 2 blocked, 1 on someone in the way, 1 on allies), 63 damage. Hits taken: 9, 94 damage. Health at the end 126.

Defence: 12 blows came at it (5 seen clearly, 7 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 8.

Pose against the joint limits (standing): a joint more than 10° past its limit 99% of the time; the worst 87° (ShinL).

Look: within 25° of what it attended to 84% of the time standing (84% of the time it was within the head's reach, 93% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 6 postures (BoxerSettle 2, ClubFeint 1, BobAndWeave 2, Shift 1), 0 of them lead-ins to blows; energy at the end 0.71.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.2 cm, max 0.4 cm (posed); physical weapon's closest pass mean 0.4 cm, max 0.5 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.87 s: ClubSwing at B Small's Head: passed 0.1 cm from the aim point; nearest part B Small's UpperArmR (28.4 cm); the Head had moved 46.9 cm since the aim was locked
- 9.10 s: ClubSwing at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's Spine (2.6 cm); the Belly had moved 9.8 cm since the aim was locked
- 20.92 s: ClubRising at B Small's Belly: passed 0.4 cm from the aim point; nearest part B Small's Chest (17.4 cm); the Belly had moved 9.1 cm since the aim was locked


## B Small

Present 0.0–31.1 s. Steps 39, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 16 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 3.4 | 7 / 103 |
| Chest | Ragdoll | 1.2 / 12.6 | 12 / 123 |
| Head | Ragdoll | 2.0 / 14.4 | 18 / 90 |
| UpperArmL | Ragdoll | 2.8 / 30.2 | 15 / 168 |
| ForearmL | Ragdoll | 4.3 / 41.6 | 21 / 145 |
| HandL | Ragdoll | 6.2 / 58.6 | 38 / 178 |
| UpperArmR | Ragdoll | 1.9 / 20.7 | 11 / 121 |
| ForearmR | Ragdoll | 3.4 / 34.0 | 17 / 136 |
| HandR | Ragdoll | 6.7 / 46.1 | 22 / 148 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (0 on the part aimed at, 0 blocked, 1 on someone in the way, 0 on allies), 11 damage. Hits taken: 9, 65 damage. Killed.

Defence: 4 blows came at it (3 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 98% of the time; the worst 54° (ForearmL).
Ragdoll (on the ground 15.2 s): a joint more than 10° past its limit 32% of the time; the worst 165° (ForearmL). Fastest part with a standing leg within 0.5 m: 2.1 m/s median, 7.7 m/s at the 95th percentile (286 steps); with none: 0.0 m/s median (624 steps).

Look: within 25° of what it attended to 81% of the time standing (85% of the time it was within the head's reach, 90% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.47.

Balance: stumbled 1 time, knocked down 3, launched 0, got up 3; thrown into others 1 time.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.2 cm, max 0.2 cm (posed); physical weapon's closest pass mean 11.3 cm, max 11.3 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 8.08 s: ClubSwing at A Large's ElbowL: passed 0.2 cm from the aim point; nearest part A Large's HandL (74.5 cm); the ElbowL had moved 111.5 cm since the aim was locked


## B Medium

Present 0.0–31.1 s. Steps 54, idle shuffles 1. Feet slid at most 7.8 cm while planted; the weight was at most 66 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 24.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 24.0 | 2 / 28 |
| Chest | Pinned | 0.7 / 22.7 | 3 / 25 |
| Head | Pinned | 0.8 / 19.4 | 9 / 40 |
| UpperArmL | Pinned | 1.0 / 19.3 | 3 / 56 |
| ForearmL | Pinned | 1.5 / 23.6 | 3 / 55 |
| HandL | Pinned | 1.7 / 32.8 | 4 / 61 |
| UpperArmR | Kinematic | 0.8 / 21.7 | 3 / 34 |
| ForearmR | Kinematic | 1.9 / 22.6 | 5 / 63 |
| HandR | Kinematic | 2.9 / 31.4 | 7 / 82 |
| ThighL | Kinematic | 0.0 / 23.7 | 0 / 150 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 24.3 | 0 / 149 |
| ShinR | Kinematic | 0.0 / 12.3 | 0 / 0 |
| FootR | Kinematic | 0.0 / 9.4 | 0 / 0 |

Hits dealt: 6 (2 on the part aimed at, 0 blocked, 0 on someone in the way, 1 on allies), 90 damage. Hits taken: 15, 40 damage. Health at the end 60.

Defence: 2 blows came at it (1 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 100% of the time; the worst 154° (ShinR).
Ragdoll (on the ground 5.6 s): a joint more than 10° past its limit 78% of the time; the worst 132° (FootL). Fastest part with a standing leg within 0.5 m: 2.8 m/s median, 11.0 m/s at the 95th percentile (267 steps); with none: 3.8 m/s median (70 steps).

Look: within 25° of what it attended to 82% of the time standing (84% of the time it was within the head's reach, 96% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 96% of the time standing; its rim came within 1.0 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 5 postures (GuardChange 1, ShieldSettle 1, Peek 1, ShieldPress 1, HighFeint 1), 1 of them lead-ins to blows; energy at the end 0.53.

Balance: stumbled 1 time, knocked down 2, launched 0, got up 1; thrown into others 12 times.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.3 cm, max 0.7 cm (posed); physical weapon's closest pass mean 0.6 cm, max 0.7 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.98 s: SwordCut at A Small's Head: passed 0.1 cm from the aim point; nearest part A Small's UpperArmR (15.0 cm); the Head had moved 30.7 cm since the aim was locked
- 4.63 s: SwordBackhand at A Small's Chest: passed 0.4 cm from the aim point; nearest part A Small's Spine (4.3 cm); the Chest had moved 17.5 cm since the aim was locked
- 5.35 s: SwordCleave at A Small's Chest: passed 0.7 cm from the aim point; nearest part A Small's Chest (14.4 cm); the Chest had moved 6.6 cm since the aim was locked
- 7.28 s: SwordCleave at A Medium's ElbowL: passed 0.0 cm from the aim point; nearest part A Medium's UpperArmL (7.5 cm); the ElbowL had moved 19.5 cm since the aim was locked


## B Large

Present 0.0–31.1 s. Steps 59, idle shuffles 0. Feet slid at most 8.5 cm while planted; the weight was at most 95 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 31.4 | 0 / 0 |
| Spine | Pinned | 0.1 / 31.4 | 2 / 30 |
| Chest | Pinned | 1.0 / 38.9 | 3 / 44 |
| Head | Pinned | 1.5 / 38.5 | 14 / 77 |
| UpperArmL | Pinned | 1.9 / 45.0 | 6 / 52 |
| ForearmL | Pinned | 4.2 / 41.2 | 10 / 61 |
| HandL | Pinned | 5.9 / 44.9 | 26 / 120 |
| UpperArmR | Muscled | 1.8 / 35.2 | 3 / 60 |
| ForearmR | Muscled | 3.2 / 66.6 | 4 / 64 |
| HandR | Muscled | 4.1 / 96.5 | 6 / 101 |
| ThighL | Kinematic | 0.0 / 31.0 | 0 / 149 |
| ShinL | Kinematic | 0.0 / 8.4 | 0 / 0 |
| FootL | Kinematic | 0.0 / 6.7 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 31.9 | 0 / 148 |
| ShinR | Kinematic | 0.0 / 20.7 | 0 / 0 |
| FootR | Kinematic | 0.0 / 17.8 | 0 / 0 |

Hits dealt: 8 (4 on the part aimed at, 0 blocked, 0 on someone in the way, 1 on allies), 90 damage. Hits taken: 4, 46 damage. Health at the end 174.

Defence: 3 blows came at it (2 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0.

Pose against the joint limits (standing): a joint more than 10° past its limit 100% of the time; the worst 101° (ShinR).

Look: within 25° of what it attended to 85% of the time standing (86% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 8 postures (BoxerSettle 3, BoxerStalk 1, BobAndWeave 1, ClubFeint 2, Shift 1), 3 of them lead-ins to blows; energy at the end 0.48.

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.5 cm, max 0.7 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 5.07 s: ClubSwing at A Medium's Belly: passed 0.0 cm from the aim point; nearest part A Medium's Spine (28.1 cm); the Belly had moved 46.9 cm since the aim was locked
- 10.45 s: ClubSwing at A Medium's Head: passed 0.0 cm from the aim point; nearest part A Large's HandL (48.9 cm); the Head had moved 90.3 cm since the aim was locked
- 12.50 s: ClubSwing at A Medium's Neck: passed 0.1 cm from the aim point; nearest part A Medium's UpperArmR (21.1 cm); the Neck had moved 38.0 cm since the aim was locked
- 14.47 s: ClubBackhand at A Large's ElbowR: passed 0.0 cm from the aim point; nearest part A Large's ForearmR (13.4 cm); the ElbowR had moved 22.8 cm since the aim was locked
- 18.08 s: ClubBackhand at A Large's Chest: passed 0.1 cm from the aim point; nearest part A Large's HandR (11.8 cm); the Chest had moved 30.9 cm since the aim was locked

## Shield contacts

74 times a shield overlapped another unit's body: 95% under 9.3 cm deep (69% under 2 cm; target 95%), the deepest 10.2 cm, the longest 0.17 s (target under 0.2 s). Each one is in events.log ("shield").

