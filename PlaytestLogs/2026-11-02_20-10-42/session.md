# Playtest 2026-11-02_20-10-42

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 53.4 s of play (3202 physics steps), 3009 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-02_20-10-42`.

## A Small

Present 0.0–53.4 s. Steps 15, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 6 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.1 | 4 / 20 |
| Chest | Ragdoll | 0.8 / 3.9 | 9 / 37 |
| Head | Ragdoll | 1.9 / 9.6 | 13 / 60 |
| UpperArmL | Ragdoll | 1.4 / 7.6 | 9 / 168 |
| ForearmL | Ragdoll | 2.1 / 14.6 | 12 / 97 |
| HandL | Ragdoll | 3.7 / 36.4 | 27 / 104 |
| UpperArmR | Ragdoll | 2.0 / 8.5 | 10 / 49 |
| ForearmR | Ragdoll | 3.2 / 19.5 | 17 / 78 |
| HandR | Ragdoll | 7.9 / 32.3 | 22 / 99 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 3, 70 damage. Killed.

Defence: 3 blows came at it (1 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 64% of the time; the worst 42° (FootL).
Ragdoll (on the ground 48.0 s): a joint more than 10° past its limit 100% of the time; the worst 69° (FootL). Fastest part with a standing leg within 0.5 m: 2.4 m/s median, 4.2 m/s at the 95th percentile (85 steps); with none: 0.0 m/s median (2794 steps).

Look: within 25° of what it attended to 75% of the time standing (77% of the time it was within the head's reach, 96% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 2 postures (StickSettle 1, AbanicoFlurry 1), 0 of them lead-ins to blows; energy at the end 0.71.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–53.4 s. Steps 66, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 38 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 9.8 | 4 / 151 |
| Chest | ? | 1.0 / 23.5 | 5 / 125 |
| Head | ? | 1.6 / 39.1 | 12 / 118 |
| UpperArmL | ? | 2.0 / 65.1 | 11 / 178 |
| ForearmL | ? | 3.1 / 56.3 | 10 / 167 |
| HandL | ? | 3.5 / 59.2 | 7 / 69 |
| UpperArmR | ? | 1.3 / 10.8 | 4 / 33 |
| ForearmR | ? | 2.5 / 20.3 | 7 / 62 |
| HandR | ? | 3.9 / 28.1 | 9 / 81 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 2 |
| ThighR | ? | 0.0 / 0.0 | 0 / 150 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 5 (0 on the part aimed at, 2 blocked, 0 on someone in the way, 0 on allies), 51 damage. Hits taken: 18, 110 damage. Health at the end 0.

Defence: 17 blows came at it (9 seen clearly, 8 at the edge of its view, 0 never seen); it raised the shield against 7.

Pose against the joint limits (standing): a joint more than 10° past its limit 45% of the time; the worst 93° (ShinR).
Ragdoll (on the ground 14.2 s): a joint more than 10° past its limit 92% of the time; the worst 175° (ForearmR). Fastest part with a standing leg within 0.5 m: 4.3 m/s median, 9.1 m/s at the 95th percentile (158 steps); with none: 0.3 m/s median (694 steps).

Look: within 25° of what it attended to 80% of the time standing (82% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 83% of the time standing; its rim came within 1.0 cm of its own body (negative: inside).

Fight Rhythm (?): 3 postures (GuardChange 1, HighFeint 1, Peek 1), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 4 times, knocked down 4, launched 0, got up 3; thrown into others 5 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 44.63 s: SwordCleave at B Medium's Belly: passed 0.0 cm from the aim point; nearest part B Medium's Pelvis (24.8 cm); the Belly had moved 56.9 cm since the aim was locked


## A Large

Present 0.0–53.4 s. Steps 78, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 58 cm from the support point; hips dropped for reach by up to 5 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 2 / 24 |
| Chest | ? | 0.8 / 10.1 | 3 / 43 |
| Head | ? | 1.3 / 13.6 | 14 / 79 |
| UpperArmL | ? | 1.4 / 16.6 | 5 / 41 |
| ForearmL | ? | 3.3 / 24.1 | 8 / 64 |
| HandL | ? | 4.7 / 45.0 | 20 / 97 |
| UpperArmR | ? | 1.6 / 28.3 | 3 / 56 |
| ForearmR | ? | 2.9 / 53.6 | 5 / 65 |
| HandR | ? | 4.0 / 74.4 | 7 / 94 |
| ThighL | ? | 0.0 / 0.0 | 0 / 150 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 149 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 9 (2 on the part aimed at, 0 blocked, 1 on someone in the way, 0 on allies), 100 damage. Hits taken: 10, 128 damage. Health at the end 0.

Defence: 12 blows came at it (6 seen clearly, 6 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 8.

Pose against the joint limits (standing): a joint more than 10° past its limit 28% of the time; the worst 91° (ShinL).
Ragdoll (on the ground 13.1 s): a joint more than 10° past its limit 59% of the time; the worst 39° (Head). Fastest part with a standing leg within 0.5 m: 0.2 m/s median, 3.2 m/s at the 95th percentile (461 steps); with none: 0.4 m/s median (326 steps).

Look: within 25° of what it attended to 81% of the time standing (80% of the time it was within the head's reach, 88% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 8 postures (BoxerSettle 1, BobAndWeave 3, ClubFeint 3, JabFeint 1), 2 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 6. The posed striking part passed within 3 cm of the aim point in 6; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.4 cm, max 0.5 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.87 s: ClubSwing at B Small's Head: passed 0.1 cm from the aim point; nearest part B Small's Chest (28.5 cm); the Head had moved 46.9 cm since the aim was locked
- 17.85 s: ClubRising at B Small's Chest: passed 0.1 cm from the aim point; nearest part B Small's HandR (43.7 cm); the Chest had moved 59.6 cm since the aim was locked
- 19.93 s: ClubSwing at B Large's ElbowR: passed 0.0 cm from the aim point; nearest part B Large's Pelvis (48.5 cm); the ElbowR had moved 113.5 cm since the aim was locked
- 24.20 s: ClubSwing at B Small's Head: passed 0.1 cm from the aim point; nearest part B Small's Pelvis (38.2 cm); the Head had moved 69.9 cm since the aim was locked
- 27.52 s: ClubSwing at B Large's Neck: passed 0.0 cm from the aim point; nearest part B Large's HandL (20.1 cm); the Neck had moved 89.5 cm since the aim was locked
- 29.72 s: ClubRising at B Small's KneeL: passed 0.1 cm from the aim point; nearest part B Small's ShinL (13.3 cm); the KneeL had moved 18.5 cm since the aim was locked


## B Small

Present 0.0–53.4 s. Steps 65, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 32 cm from the support point; hips dropped for reach by up to 2 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.6 | 3 / 68 |
| Chest | Ragdoll | 0.6 / 12.1 | 5 / 34 |
| Head | Ragdoll | 1.0 / 8.7 | 10 / 55 |
| UpperArmL | Ragdoll | 0.9 / 14.0 | 6 / 52 |
| ForearmL | Ragdoll | 1.9 / 14.4 | 9 / 96 |
| HandL | Ragdoll | 2.5 / 36.3 | 25 / 107 |
| UpperArmR | Ragdoll | 0.8 / 6.6 | 5 / 43 |
| ForearmR | Ragdoll | 1.7 / 17.7 | 8 / 69 |
| HandR | Ragdoll | 3.6 / 27.5 | 11 / 91 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 15 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 2 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 3 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 1 |

Hits dealt: 2 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 17 damage. Hits taken: 8, 63 damage. Killed.

Defence: 6 blows came at it (2 seen clearly, 4 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 5.

Pose against the joint limits (standing): a joint more than 10° past its limit 52% of the time; the worst 59° (FootL).
Ragdoll (on the ground 27.6 s): a joint more than 10° past its limit 93% of the time; the worst 167° (ForearmL). Fastest part with a standing leg within 0.5 m: 5.3 m/s median, 9.0 m/s at the 95th percentile (217 steps); with none: 0.0 m/s median (1440 steps).

Look: within 25° of what it attended to 76% of the time standing (83% of the time it was within the head's reach, 88% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 5 postures (StickSettle 1, AbanicoFlurry 2, BaitRetreat 2), 0 of them lead-ins to blows; energy at the end 0.42.

Balance: stumbled 0 times, knocked down 2, launched 2, got up 3; thrown into others 3 times.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.0 cm, max 0.1 cm (posed); physical weapon's closest pass mean 4.0 cm, max 9.2 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 7.82 s: ClubSwing at A Large's Belly: passed 0.1 cm from the aim point; nearest part A Large's Chest (25.5 cm); the Belly had moved 23.4 cm since the aim was locked
- 13.08 s: ClubRising at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (20.3 cm); the KneeL had moved 18.5 cm since the aim was locked
- 14.38 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (19.9 cm); the KneeL had moved 21.7 cm since the aim was locked


## B Medium

Present 0.0–53.4 s. Steps 97, idle shuffles 2. Feet slid at most 5.3 cm while planted; the weight was at most 63 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 18.9 | 0 / 0 |
| Spine | Pinned | 0.0 / 18.9 | 2 / 87 |
| Chest | Pinned | 0.7 / 20.8 | 3 / 55 |
| Head | Pinned | 0.8 / 20.6 | 9 / 66 |
| UpperArmL | Pinned | 1.2 / 37.0 | 5 / 152 |
| ForearmL | Pinned | 2.1 / 46.2 | 6 / 98 |
| HandL | Pinned | 3.3 / 60.4 | 6 / 152 |
| UpperArmR | Muscled | 0.7 / 17.0 | 3 / 37 |
| ForearmR | Muscled | 1.6 / 24.8 | 4 / 63 |
| HandR | Muscled | 2.3 / 28.2 | 5 / 82 |
| ThighL | Kinematic | 0.0 / 18.6 | 0 / 56 |
| ShinL | Kinematic | 0.0 / 15.8 | 0 / 56 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 2 |
| ThighR | Kinematic | 0.0 / 19.2 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 9.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 6.7 | 0 / 0 |

Hits dealt: 11 (2 on the part aimed at, 4 blocked, 0 on someone in the way, 0 on allies), 122 damage. Hits taken: 8, 65 damage. Health at the end 35.

Defence: 4 blows came at it (2 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 33% of the time; the worst 106° (FootL).
Ragdoll (on the ground 4.6 s): a joint more than 10° past its limit 99% of the time; the worst 173° (ForearmL). Fastest part with a standing leg within 0.5 m: 3.6 m/s median, 5.6 m/s at the 95th percentile (16 steps); with none: 3.7 m/s median (257 steps).

Look: within 25° of what it attended to 83% of the time standing (84% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 93% of the time standing; its rim came within -1.4 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 10 postures (GuardChange 1, ShieldSettle 2, HighFeint 2, Peek 2, ShieldPress 2, ShieldAdvance 1), 2 of them lead-ins to blows; energy at the end 0.40.

Balance: stumbled 3 times, knocked down 3, launched 0, got up 3; thrown into others 1 time.

Blows: 7. The posed striking part passed within 3 cm of the aim point in 7; aim error mean 0.2 cm, max 0.7 cm (posed); physical weapon's closest pass mean 0.4 cm, max 0.7 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.00 s: SwordCut at A Small's Head: passed 0.0 cm from the aim point; nearest part A Small's Head (17.8 cm); the Head had moved 30.6 cm since the aim was locked
- 4.65 s: SwordBackhand at A Small's Chest: passed 0.4 cm from the aim point; nearest part A Small's UpperArmL (0.8 cm); the Chest had moved 22.0 cm since the aim was locked
- 5.37 s: SwordCleave at A Small's Chest: passed 0.7 cm from the aim point; nearest part A Small's Chest (5.9 cm); the Chest had moved 7.8 cm since the aim was locked
- 16.55 s: SwordThrust at A Medium's Belly: passed 0.1 cm from the aim point; nearest part A Medium's UpperArmR (29.2 cm); the Belly had moved 61.7 cm since the aim was locked
- 32.77 s: SwordCut at A Large's Chest: passed 0.0 cm from the aim point; nearest part A Large's Spine (19.6 cm); the Chest had moved 74.2 cm since the aim was locked
- 34.43 s: SwordCleave at A Large's Belly: passed 0.0 cm from the aim point; nearest part A Large's Pelvis (24.1 cm); the Belly had moved 33.5 cm since the aim was locked
- 44.57 s: SwordCut at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Pelvis (41.7 cm); the Chest had moved 74.7 cm since the aim was locked


## B Large

Present 0.0–53.4 s. Steps 83, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 45 cm from the support point; hips dropped for reach by up to 15 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 2 / 43 |
| Chest | Pinned | 1.0 / 17.9 | 3 / 47 |
| Head | Pinned | 1.5 / 18.6 | 11 / 94 |
| UpperArmL | Pinned | 1.7 / 17.1 | 6 / 51 |
| ForearmL | Pinned | 3.7 / 36.3 | 10 / 98 |
| HandL | Pinned | 5.4 / 45.0 | 22 / 172 |
| UpperArmR | Muscled | 1.8 / 44.4 | 4 / 61 |
| ForearmR | Muscled | 3.5 / 76.4 | 5 / 76 |
| HandR | Muscled | 4.6 / 76.0 | 7 / 111 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 150 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 150 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 12 (2 on the part aimed at, 2 blocked, 0 on someone in the way, 3 on allies), 143 damage. Hits taken: 10, 105 damage. Health at the end 115.

Defence: 8 blows came at it (6 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 5.

Pose against the joint limits (standing): a joint more than 10° past its limit 24% of the time; the worst 121° (ShinL).

Look: within 25° of what it attended to 87% of the time standing (87% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 14 postures (BoxerSettle 2, BobAndWeave 2, ClubFeint 4, JabFeint 4, BoxerStalk 2), 3 of them lead-ins to blows; energy at the end 0.94.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 6. The posed striking part passed within 3 cm of the aim point in 6; aim error mean 0.1 cm, max 0.5 cm (posed); physical weapon's closest pass mean 0.5 cm, max 0.7 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.33 s: ClubSwing at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Chest (23.1 cm); the Chest had moved 51.3 cm since the aim was locked
- 4.18 s: ClubBackhand at A Medium's ElbowL: passed 0.2 cm from the aim point; nearest part A Medium's HandL (74.2 cm); the ElbowL had moved 77.2 cm since the aim was locked
- 4.92 s: ClubSwing at A Medium's Head: passed 0.5 cm from the aim point; nearest part A Medium's FootR (47.6 cm); the Head had moved 100.3 cm since the aim was locked
- 9.95 s: ClubBackhand at A Medium's Neck: passed 0.1 cm from the aim point; nearest part A Medium's Head (1.3 cm); the Neck had moved 4.5 cm since the aim was locked
- 23.75 s: ClubRising at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Head (7.1 cm); the Chest had moved 26.6 cm since the aim was locked
- 44.88 s: ClubSwing at A Medium's Head: passed 0.0 cm from the aim point; nearest part A Medium's Chest (39.1 cm); the Head had moved 59.6 cm since the aim was locked

## Shield contacts

186 times a shield overlapped another unit's body: 95% under 6.9 cm deep (80% under 2 cm; target 95%), the deepest 12.7 cm, the longest 0.53 s (target under 0.2 s). Each one is in events.log ("shield").

