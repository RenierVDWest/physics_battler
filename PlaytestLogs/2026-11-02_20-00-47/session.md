# Playtest 2026-11-02_20-00-47

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 41.8 s of play (2511 physics steps), 2882 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-02_20-00-47`.

## A Small

Present 0.0–41.8 s. Steps 15, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 6 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.0 | 4 / 24 |
| Chest | Ragdoll | 0.8 / 4.7 | 8 / 31 |
| Head | Ragdoll | 1.7 / 8.7 | 12 / 68 |
| UpperArmL | Ragdoll | 1.2 / 7.6 | 10 / 171 |
| ForearmL | Ragdoll | 2.0 / 14.6 | 10 / 97 |
| HandL | Ragdoll | 3.3 / 36.4 | 25 / 131 |
| UpperArmR | Ragdoll | 1.7 / 9.1 | 9 / 49 |
| ForearmR | Ragdoll | 3.0 / 19.5 | 16 / 78 |
| HandR | Ragdoll | 7.4 / 32.3 | 20 / 99 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 3, 66 damage. Killed.

Defence: 3 blows came at it (2 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 67% of the time; the worst 42° (FootL).
Ragdoll (on the ground 36.5 s): a joint more than 10° past its limit 3% of the time; the worst 169° (ShinL). Fastest part with a standing leg within 0.5 m: 0.0 m/s median, 0.6 m/s at the 95th percentile (339 steps); with none: 0.0 m/s median (1851 steps).

Look: within 25° of what it attended to 74% of the time standing (76% of the time it was within the head's reach, 95% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 2 postures (StickSettle 1, AbanicoFlurry 1), 0 of them lead-ins to blows; energy at the end 0.79.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–41.8 s. Steps 61, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 31 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.1 | 2 / 15 |
| Chest | Ragdoll | 0.6 / 4.5 | 4 / 43 |
| Head | Ragdoll | 1.1 / 12.4 | 14 / 99 |
| UpperArmL | Ragdoll | 1.2 / 13.5 | 6 / 68 |
| ForearmL | Ragdoll | 2.8 / 28.2 | 10 / 178 |
| HandL | Ragdoll | 4.0 / 29.2 | 12 / 95 |
| UpperArmR | Ragdoll | 1.0 / 9.5 | 4 / 39 |
| ForearmR | Ragdoll | 2.0 / 20.1 | 6 / 62 |
| HandR | Ragdoll | 3.0 / 28.1 | 7 / 81 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 5 (0 on the part aimed at, 1 blocked, 1 on someone in the way, 0 on allies), 65 damage. Hits taken: 11, 104 damage. Killed.

Defence: 11 blows came at it (8 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 6.

Pose against the joint limits (standing): a joint more than 10° past its limit 38% of the time; the worst 34° (FootL).
Ragdoll (on the ground 9.8 s): a joint more than 10° past its limit 75% of the time; the worst 112° (ForearmL). Fastest part with a standing leg within 0.5 m: 0.7 m/s median, 6.1 m/s at the 95th percentile (541 steps); with none: 1.9 m/s median (46 steps).

Look: within 25° of what it attended to 78% of the time standing (79% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 91% of the time standing; its rim came within 1.1 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 2 postures (GuardChange 1, Peek 1), 0 of them lead-ins to blows; energy at the end 0.32.

Balance: stumbled 3 times, knocked down 1, launched 0, got up 1; thrown into others 0 times.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 4.6 cm, max 6.3 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.37 s: SwordCut at B Large's Head: passed 2.8 cm from the aim point; nearest part B Large's Chest (9.8 cm); the Head had moved 17.6 cm since the aim was locked
- 13.53 s: SwordCut at B Medium's Head: passed 6.3 cm from the aim point; nearest part B Medium's HandL (23.1 cm); the Head had moved 85.4 cm since the aim was locked


## A Large

Present 0.0–41.8 s. Steps 59, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 38 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.2 | 2 / 19 |
| Chest | Ragdoll | 0.7 / 7.4 | 2 / 9 |
| Head | Ragdoll | 1.0 / 7.3 | 13 / 70 |
| UpperArmL | Ragdoll | 1.2 / 11.4 | 4 / 41 |
| ForearmL | Ragdoll | 3.0 / 24.1 | 7 / 58 |
| HandL | Ragdoll | 4.3 / 45.0 | 19 / 101 |
| UpperArmR | Ragdoll | 0.9 / 8.5 | 2 / 55 |
| ForearmR | Ragdoll | 1.7 / 14.9 | 3 / 64 |
| HandR | Ragdoll | 2.5 / 52.0 | 4 / 79 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 9 (2 on the part aimed at, 0 blocked, 1 on someone in the way, 0 on allies), 124 damage. Hits taken: 9, 92 damage. Health at the end 66.

Defence: 13 blows came at it (7 seen clearly, 6 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 6.

Pose against the joint limits (standing): a joint more than 10° past its limit 30% of the time; the worst 67° (FootR).
Ragdoll (on the ground 6.1 s): a joint more than 10° past its limit 86% of the time; the worst 49° (HandL). Fastest part with a standing leg within 0.5 m: - median, - at the 95th percentile (0 steps); with none: 0.1 m/s median (365 steps).

Look: within 25° of what it attended to 87% of the time standing (85% of the time it was within the head's reach, 90% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 4 postures (BoxerSettle 1, BobAndWeave 2, ClubFeint 1), 1 of them lead-ins to blows; energy at the end 0.17.

Balance: stumbled 1 time, knocked down 1, launched 0, got up 0; thrown into others 0 times. Still down at the end.

Blows: 7. The posed striking part passed within 3 cm of the aim point in 6; aim error mean 1.2 cm, max 7.9 cm (posed); physical weapon's closest pass mean 4.3 cm, max 8.6 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.65 s: ClubRising at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's HandR (21.5 cm); the Belly had moved 27.5 cm since the aim was locked
- 4.50 s: ClubSwing at B Small's Head: passed 0.5 cm from the aim point; nearest part B Small's UpperArmL (42.0 cm); the Head had moved 65.0 cm since the aim was locked
- 5.12 s: ClubBackhand at B Small's ElbowL: passed 7.9 cm from the aim point; nearest part B Small's ThighL (20.4 cm); the ElbowL had moved 17.9 cm since the aim was locked
- 6.98 s: ClubBackhand at B Large's Belly: passed 0.0 cm from the aim point; nearest part B Large's HandL (66.5 cm); the Belly had moved 114.4 cm since the aim was locked
- 12.35 s: ClubBackhand at B Large's Neck: passed 0.0 cm from the aim point; nearest part B Large's Head (19.1 cm); the Neck had moved 31.1 cm since the aim was locked
- 15.30 s: ClubSwing at B Small's Head: passed 0.1 cm from the aim point; nearest part B Small's Pelvis (36.2 cm); the Head had moved 75.2 cm since the aim was locked
- 21.62 s: ClubRising at B Small's Belly: passed 0.1 cm from the aim point; nearest part B Small's HandR (12.2 cm); the Belly had moved 13.5 cm since the aim was locked


## B Small

Present 0.0–41.8 s. Steps 47, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 17 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.1 | 3 / 33 |
| Chest | Ragdoll | 0.7 / 6.5 | 5 / 31 |
| Head | Ragdoll | 1.0 / 8.1 | 11 / 65 |
| UpperArmL | Ragdoll | 1.0 / 7.2 | 6 / 169 |
| ForearmL | Ragdoll | 1.9 / 22.6 | 9 / 170 |
| HandL | Ragdoll | 2.6 / 36.3 | 21 / 153 |
| UpperArmR | Ragdoll | 1.0 / 8.6 | 5 / 43 |
| ForearmR | Ragdoll | 2.0 / 17.7 | 10 / 85 |
| HandR | Ragdoll | 4.2 / 38.9 | 13 / 101 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 4 |

Hits dealt: 3 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 26 damage. Hits taken: 4, 66 damage. Killed.

Defence: 5 blows came at it (0 seen clearly, 5 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 55% of the time; the worst 74° (FootL).
Ragdoll (on the ground 23.9 s): a joint more than 10° past its limit 92% of the time; the worst 156° (ForearmL). Fastest part with a standing leg within 0.5 m: 1.3 m/s median, 7.9 m/s at the 95th percentile (983 steps); with none: 0.3 m/s median (452 steps).

Look: within 25° of what it attended to 76% of the time standing (82% of the time it was within the head's reach, 92% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 4 postures (StickSettle 1, AbanicoFlurry 2, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.46.

Balance: stumbled 0 times, knocked down 1, launched 1, got up 2; thrown into others 0 times.


## B Medium

Present 0.0–41.8 s. Steps 79, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 36 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 3 / 85 |
| Chest | Pinned | 0.8 / 23.0 | 5 / 85 |
| Head | Pinned | 1.3 / 48.1 | 12 / 73 |
| UpperArmL | Pinned | 1.3 / 40.8 | 5 / 104 |
| ForearmL | Pinned | 2.4 / 53.4 | 7 / 129 |
| HandL | Pinned | 3.2 / 65.0 | 8 / 130 |
| UpperArmR | Muscled | 1.4 / 43.7 | 5 / 100 |
| ForearmR | Muscled | 2.5 / 42.8 | 7 / 165 |
| HandR | Muscled | 4.2 / 84.2 | 9 / 179 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 150 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 12 (4 on the part aimed at, 3 blocked, 0 on someone in the way, 0 on allies), 173 damage. Hits taken: 4, 53 damage. Health at the end 47.

Defence: 6 blows came at it (5 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 28% of the time; the worst 98° (ShinR).
Ragdoll (on the ground 1.1 s): a joint more than 10° past its limit 97% of the time; the worst 62° (HandR). Fastest part with a standing leg within 0.5 m: 3.2 m/s median, 3.2 m/s at the 95th percentile (7 steps); with none: 4.0 m/s median (60 steps).

Look: within 25° of what it attended to 81% of the time standing (82% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 96% of the time standing; its rim came within -0.4 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 10 postures (GuardChange 2, Peek 1, HighFeint 4, ShieldPress 2, ShieldAdvance 1), 5 of them lead-ins to blows; energy at the end 0.95.

Balance: stumbled 0 times, knocked down 1, launched 0, got up 1; thrown into others 0 times.

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.2 cm, max 0.4 cm (posed); physical weapon's closest pass mean 0.5 cm, max 0.6 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.98 s: SwordCut at A Small's Head: passed 0.1 cm from the aim point; nearest part A Small's UpperArmR (14.1 cm); the Head had moved 31.4 cm since the aim was locked
- 4.63 s: SwordBackhand at A Small's Chest: passed 0.4 cm from the aim point; nearest part A Small's UpperArmL (2.4 cm); the Chest had moved 16.2 cm since the aim was locked
- 5.45 s: SwordCleave at A Small's Chest: passed 0.4 cm from the aim point; nearest part A Small's Chest (9.5 cm); the Chest had moved 11.7 cm since the aim was locked
- 30.90 s: SwordBackhand at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Chest (10.8 cm); the Chest had moved 31.7 cm since the aim was locked
- 35.73 s: SwordBackhand at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (1.0 cm); the KneeL had moved 0.8 cm since the aim was locked


## B Large

Present 0.0–41.8 s. Steps 66, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 55 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 3 / 80 |
| Chest | Pinned | 1.1 / 10.5 | 4 / 44 |
| Head | Pinned | 1.7 / 12.5 | 14 / 108 |
| UpperArmL | Pinned | 1.6 / 22.5 | 5 / 51 |
| ForearmL | Pinned | 3.4 / 24.7 | 8 / 59 |
| HandL | Pinned | 4.7 / 45.0 | 19 / 87 |
| UpperArmR | Muscled | 2.0 / 33.7 | 4 / 46 |
| ForearmR | Muscled | 3.5 / 52.6 | 5 / 64 |
| HandR | Muscled | 4.7 / 60.9 | 7 / 79 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 9 (3 on the part aimed at, 1 blocked, 0 on someone in the way, 1 on allies), 75 damage. Hits taken: 7, 81 damage. Health at the end 139.

Defence: 5 blows came at it (4 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 4.

Pose against the joint limits (standing): a joint more than 10° past its limit 25% of the time; the worst 47° (FootL).

Look: within 25° of what it attended to 83% of the time standing (83% of the time it was within the head's reach, 95% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 13 postures (BoxerSettle 2, BoxerStalk 3, BobAndWeave 3, ClubFeint 2, JabFeint 3), 3 of them lead-ins to blows; energy at the end 1.00.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.7 cm, max 2.9 cm (posed); physical weapon's closest pass mean 2.4 cm, max 2.9 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.12 s: OffJab at A Medium's Chest: passed 2.9 cm from the aim point; nearest part A Medium's ForearmR (25.9 cm); the Chest had moved 31.0 cm since the aim was locked
- 5.63 s: ClubRising at A Medium's KneeL: passed 0.3 cm from the aim point; nearest part A Medium's ThighL (17.0 cm); the KneeL had moved 2.2 cm since the aim was locked
- 19.68 s: ClubBackhand at A Large's Belly: passed 0.0 cm from the aim point; nearest part A Large's UpperArmL (23.4 cm); the Belly had moved 38.2 cm since the aim was locked
- 28.20 s: ClubBackhand at A Large's Chest: passed 0.0 cm from the aim point; nearest part A Large's ForearmL (4.1 cm); the Chest had moved 24.5 cm since the aim was locked
- 30.15 s: ClubBackhand at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (5.5 cm); the KneeL had moved 9.2 cm since the aim was locked

## Shield contacts

138 times a shield overlapped another unit's body: 95% under 4.8 cm deep (86% under 2 cm; target 95%), the deepest 9.2 cm, the longest 0.83 s (target under 0.2 s). Each one is in events.log ("shield").

