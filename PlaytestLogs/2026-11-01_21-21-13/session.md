# Playtest 2026-11-01_21-21-13

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 51.8 s of play (3111 physics steps), 1697 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-01_21-21-13`.

## A Small

Present 0.0–51.8 s. Steps 23, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 7 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.0 | 1 / 9 |
| Chest | ? | 0.3 / 1.8 | 1 / 6 |
| Head | ? | 0.3 / 1.1 | 6 / 31 |
| UpperArmL | ? | 0.4 / 1.4 | 2 / 44 |
| ForearmL | ? | 0.8 / 15.4 | 6 / 99 |
| HandL | ? | 2.0 / 36.0 | 19 / 82 |
| UpperArmR | ? | 0.3 / 1.5 | 3 / 92 |
| ForearmR | ? | 0.9 / 14.5 | 6 / 119 |
| HandR | ? | 2.0 / 31.5 | 20 / 102 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 4, 67 damage. Health at the end 0.

Defence: 4 blows came at it (2 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 63% of the time; the worst 42° (FootL).
Ragdoll (on the ground 43.3 s): a joint more than 10° past its limit 100% of the time; the worst 99° (FootL). Fastest part with a standing leg within 0.5 m: 0.6 m/s median, 6.0 m/s at the 95th percentile (110 steps); with none: 0.0 m/s median (2485 steps).

Look: within 25° of what it attended to 83% of the time standing (85% of the time it was within the head's reach, 97% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 3 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–51.8 s. Steps 35, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 24 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.1 | 1 / 27 |
| Chest | Ragdoll | 0.4 / 8.0 | 1 / 25 |
| Head | Ragdoll | 0.4 / 8.4 | 12 / 57 |
| UpperArmL | Ragdoll | 0.6 / 10.6 | 7 / 62 |
| ForearmL | Ragdoll | 1.5 / 30.0 | 4 / 76 |
| HandL | Ragdoll | 2.1 / 43.9 | 5 / 79 |
| UpperArmR | Ragdoll | 0.3 / 2.5 | 2 / 91 |
| ForearmR | Ragdoll | 0.8 / 17.3 | 3 / 95 |
| HandR | Ragdoll | 1.3 / 25.3 | 13 / 112 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 3 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 34 damage. Hits taken: 5, 81 damage. Killed.

Defence: 5 blows came at it (1 seen clearly, 4 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 41% of the time; the worst 39° (FootL).
Ragdoll (on the ground 30.7 s): a joint more than 10° past its limit 88% of the time; the worst 116° (HandR). Fastest part with a standing leg within 0.5 m: 0.0 m/s median, 4.0 m/s at the 95th percentile (640 steps); with none: 0.0 m/s median (1199 steps).

Look: within 25° of what it attended to 79% of the time standing (79% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 96% of the time standing; its rim came within 14.2 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 2 postures (GuardChange 1, Peek 1), 0 of them lead-ins to blows; energy at the end 0.49.

Balance: stumbled 2 times, knocked down 2, launched 0, got up 2; thrown into others 0 times.


## A Large

Present 0.0–51.8 s. Steps 68, idle shuffles 2. Feet slid at most 45.9 cm while planted; the weight was at most 63 cm from the support point; hips dropped for reach by up to 26 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 1 / 34 |
| Chest | ? | 0.6 / 12.7 | 1 / 16 |
| Head | ? | 0.8 / 10.0 | 14 / 61 |
| UpperArmL | ? | 0.9 / 11.0 | 4 / 60 |
| ForearmL | ? | 2.9 / 48.8 | 8 / 72 |
| HandL | ? | 4.5 / 50.0 | 19 / 114 |
| UpperArmR | ? | 0.6 / 9.9 | 1 / 59 |
| ForearmR | ? | 1.0 / 23.4 | 3 / 68 |
| HandR | ? | 1.8 / 51.7 | 8 / 70 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 8 (2 on the part aimed at, 0 blocked, 1 on someone in the way, 1 on allies), 89 damage. Hits taken: 23, 227 damage. Health at the end 0.

Defence: 34 blows came at it (9 seen clearly, 25 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 26.

Pose against the joint limits (standing): a joint more than 10° past its limit 25% of the time; the worst 127° (ShinL).
Ragdoll (on the ground 7.1 s): a joint more than 10° past its limit 93% of the time; the worst 56° (Head). Fastest part with a standing leg within 0.5 m: 3.8 m/s median, 8.1 m/s at the 95th percentile (27 steps); with none: 1.0 m/s median (399 steps).

Look: within 25° of what it attended to 74% of the time standing (74% of the time it was within the head's reach, 90% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 7 postures (BoxerSettle 1, BobAndWeave 2, ClubFeint 2, JabFeint 2), 2 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.7 cm, max 1.9 cm (posed); physical weapon's closest pass mean 1.6 cm, max 2.9 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 7.45 s: ClubSwing at B Small's Neck: passed 0.4 cm from the aim point; nearest part B Small's UpperArmR (31.9 cm); the Neck had moved 47.6 cm since the aim was locked
- 9.82 s: ClubSmash at B Small's Neck: passed 0.2 cm from the aim point; nearest part B Small's Chest (29.3 cm); the Neck had moved 36.7 cm since the aim was locked
- 12.00 s: ClubSwing at B Large's Head: passed 0.4 cm from the aim point; nearest part B Large's Head (18.2 cm); the Head had moved 24.6 cm since the aim was locked
- 37.33 s: ClubSwing at B Large's Neck: passed 1.9 cm from the aim point; nearest part B Large's Chest (34.8 cm); the Neck had moved 54.1 cm since the aim was locked


## B Small

Present 0.0–51.8 s. Steps 146, idle shuffles 5. Feet slid at most 0.0 cm while planted; the weight was at most 38 cm from the support point; hips dropped for reach by up to 9 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.6 | 2 / 56 |
| Chest | ? | 0.4 / 10.2 | 2 / 98 |
| Head | ? | 0.6 / 21.7 | 10 / 77 |
| UpperArmL | ? | 0.8 / 35.2 | 6 / 170 |
| ForearmL | ? | 2.1 / 40.3 | 10 / 165 |
| HandL | ? | 2.7 / 36.0 | 24 / 180 |
| UpperArmR | ? | 0.3 / 12.8 | 2 / 80 |
| ForearmR | ? | 0.6 / 15.7 | 3 / 100 |
| HandR | ? | 1.0 / 24.5 | 11 / 106 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 12 (3 on the part aimed at, 0 blocked, 0 on someone in the way, 1 on allies), 106 damage. Hits taken: 1, 9 damage. Health at the end 0.

Defence: 3 blows came at it (2 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 50% of the time; the worst 166° (ShinR).
Ragdoll (on the ground 1.0 s): a joint more than 10° past its limit 58% of the time; the worst 146° (ForearmL). Fastest part with a standing leg within 0.5 m: 4.2 m/s median, 7.0 m/s at the 95th percentile (36 steps); with none: 1.9 m/s median (24 steps).

Look: within 25° of what it attended to 75% of the time standing (80% of the time it was within the head's reach, 91% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 14 postures (StickSettle 3, AbanicoFlurry 5, RedondaTwirl 3, BaitRetreat 1, StickStalk 1, TriangleStep 1), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 0 times, knocked down 1, launched 0, got up 1; thrown into others 0 times.

Blows: 7. The posed striking part passed within 3 cm of the aim point in 7; aim error mean 0.7 cm, max 1.0 cm (posed); physical weapon's closest pass mean 0.8 cm, max 1.0 cm. 2 reached the part they were aimed at (nearest part within 5 cm at contact).

- 15.83 s: ClubSwing at A Large's ElbowR: passed 1.0 cm from the aim point; nearest part A Large's UpperArmR (11.9 cm); the ElbowR had moved 13.2 cm since the aim was locked
- 20.28 s: ClubSwing at A Large's ElbowR: passed 0.9 cm from the aim point; nearest part A Large's UpperArmR (17.4 cm); the ElbowR had moved 20.5 cm since the aim was locked
- 22.25 s: ClubSwing at A Large's KneeR: passed 0.6 cm from the aim point; nearest part A Large's ThighL (27.1 cm); the KneeR had moved 55.6 cm since the aim was locked
- 25.33 s: ClubSwing at A Large's KneeL: passed 0.6 cm from the aim point; nearest part A Large's ThighL (14.9 cm); the KneeL had moved 33.9 cm since the aim was locked
- 31.40 s: ClubRising at A Large's Belly: passed 0.7 cm from the aim point; nearest part A Large's Spine (0.0 cm); the Belly had moved 14.2 cm since the aim was locked
- 33.52 s: ClubSwing at A Large's Belly: passed 0.5 cm from the aim point; nearest part A Large's Spine (2.8 cm); the Belly had moved 7.9 cm since the aim was locked
- 38.22 s: ClubRising at A Large's KneeL: passed 0.5 cm from the aim point; nearest part A Large's ThighL (32.8 cm); the KneeL had moved 44.7 cm since the aim was locked


## B Medium

Present 0.0–51.8 s. Steps 115, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 45 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 2 / 21 |
| Chest | Pinned | 0.5 / 6.0 | 2 / 15 |
| Head | Pinned | 0.5 / 5.2 | 11 / 60 |
| UpperArmL | Pinned | 0.7 / 9.2 | 3 / 55 |
| ForearmL | Pinned | 1.3 / 23.5 | 3 / 60 |
| HandL | Pinned | 1.6 / 32.7 | 2 / 69 |
| UpperArmR | Pinned | 0.4 / 5.6 | 2 / 97 |
| ForearmR | Pinned | 0.9 / 19.7 | 4 / 99 |
| HandR | Pinned | 1.4 / 25.6 | 13 / 143 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 14 (6 on the part aimed at, 0 blocked, 0 on someone in the way, 2 on allies), 201 damage. Hits taken: 4, 14 damage. Health at the end 86.

Pose against the joint limits (standing): a joint more than 10° past its limit 35% of the time; the worst 89° (ShinR).
Ragdoll (on the ground 1.1 s): a joint more than 10° past its limit 96% of the time; the worst 112° (HandR). Fastest part with a standing leg within 0.5 m: - median, - at the 95th percentile (0 steps); with none: 2.5 m/s median (67 steps).

Look: within 25° of what it attended to 79% of the time standing (80% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 98% of the time standing; its rim came within -0.9 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 14 postures (GuardChange 5, ShieldSettle 1, ShieldPress 2, HighFeint 3, Peek 2, ShieldAdvance 1), 3 of them lead-ins to blows; energy at the end 0.48.

Balance: stumbled 2 times, knocked down 1, launched 0, got up 1; thrown into others 0 times.

Blows: 10. The posed striking part passed within 3 cm of the aim point in 10; aim error mean 0.3 cm, max 1.0 cm (posed); physical weapon's closest pass mean 0.5 cm, max 1.4 cm. 3 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.90 s: SwordThrust at A Small's Belly: passed 0.3 cm from the aim point; nearest part A Small's Spine (0.0 cm); the Belly had moved 11.4 cm since the aim was locked
- 5.48 s: SwordThrust at A Small's Head: passed 0.1 cm from the aim point; nearest part A Small's UpperArmR (10.2 cm); the Head had moved 15.7 cm since the aim was locked
- 7.37 s: SwordCut at A Small's Chest: passed 0.2 cm from the aim point; nearest part A Small's Spine (4.1 cm); the Chest had moved 19.9 cm since the aim was locked
- 15.68 s: SwordCleave at A Medium's Belly: passed 0.7 cm from the aim point; nearest part A Medium's UpperArmL (26.5 cm); the Belly had moved 48.8 cm since the aim was locked
- 17.68 s: SwordCut at A Medium's Neck: passed 0.2 cm from the aim point; nearest part A Medium's Chest (8.2 cm); the Neck had moved 9.2 cm since the aim was locked
- 27.33 s: SwordThrust at A Large's Belly: passed 0.1 cm from the aim point; nearest part A Large's Spine (4.6 cm); the Belly had moved 2.6 cm since the aim was locked
- 32.52 s: SwordThrust at A Large's Belly: passed 0.1 cm from the aim point; nearest part A Large's ForearmR (8.7 cm); the Belly had moved 6.9 cm since the aim was locked
- 34.10 s: SwordThrust at A Large's Neck: passed 0.1 cm from the aim point; nearest part A Large's HandR (21.2 cm); the Neck had moved 45.9 cm since the aim was locked
- 35.75 s: SwordBackhand at A Large's Neck: passed 0.2 cm from the aim point; nearest part A Large's Chest (37.1 cm); the Neck had moved 59.9 cm since the aim was locked
- 42.12 s: SwordThrust at A Large's Belly: passed 1.0 cm from the aim point; nearest part A Large's Spine (4.8 cm); the Belly had moved 1.5 cm since the aim was locked


## B Large

Present 0.0–51.8 s. Steps 85, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 47 cm from the support point; hips dropped for reach by up to 7 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 3.8 | 1 / 46 |
| Chest | ? | 0.6 / 12.4 | 1 / 25 |
| Head | ? | 0.7 / 16.0 | 12 / 110 |
| UpperArmL | ? | 0.9 / 27.1 | 5 / 124 |
| ForearmL | ? | 3.4 / 59.2 | 9 / 120 |
| HandL | ? | 4.9 / 48.1 | 21 / 145 |
| UpperArmR | ? | 0.5 / 11.9 | 2 / 68 |
| ForearmR | ? | 1.1 / 16.2 | 4 / 68 |
| HandR | ? | 2.0 / 51.7 | 10 / 114 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 11 (1 on the part aimed at, 2 blocked, 1 on someone in the way, 1 on allies), 96 damage. Hits taken: 11, 127 damage. Health at the end 0.

Defence: 9 blows came at it (7 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 7.

Pose against the joint limits (standing): a joint more than 10° past its limit 29% of the time; the worst 143° (ShinL).

Look: within 25° of what it attended to 79% of the time standing (80% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 9 postures (BoxerSettle 2, ClubFeint 3, BobAndWeave 3, Shift 1), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.3 cm, max 0.5 cm (posed); physical weapon's closest pass mean 0.8 cm, max 1.2 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.43 s: ClubSwing at A Medium's Belly: passed 0.3 cm from the aim point; nearest part A Medium's Chest (2.6 cm); the Belly had moved 31.1 cm since the aim was locked
- 5.95 s: ClubSwing at A Large's Belly: passed 0.3 cm from the aim point; nearest part B Small's Head (73.2 cm); the Belly had moved 139.3 cm since the aim was locked
- 8.55 s: ClubSmash at A Small's Neck: passed 0.4 cm from the aim point; nearest part A Small's Chest (11.8 cm); the Neck had moved 24.3 cm since the aim was locked
- 38.47 s: ClubSmash at A Large's Neck: passed 0.2 cm from the aim point; nearest part A Large's HandL (55.4 cm); the Neck had moved 121.4 cm since the aim was locked
- 45.15 s: ClubSmash at A Large's Head: passed 0.5 cm from the aim point; nearest part A Large's Head (58.7 cm); the Head had moved 75.8 cm since the aim was locked

## Shield contacts

41 times a shield overlapped another unit's body: 95% under 3.7 cm deep (80% under 2 cm; target 95%), the deepest 10.2 cm, the longest 0.08 s (target under 0.2 s). Each one is in events.log ("shield").

