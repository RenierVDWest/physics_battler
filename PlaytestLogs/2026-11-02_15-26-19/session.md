# Playtest 2026-11-02_15-26-19

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 28.6 s of play (1717 physics steps), 1582 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-02_15-26-19`.

## A Small

Present 0.0–28.6 s. Steps 27, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 9 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.0 | 2 / 10 |
| Chest | ? | 0.3 / 1.9 | 2 / 8 |
| Head | ? | 0.3 / 1.4 | 4 / 10 |
| UpperArmL | ? | 0.3 / 2.8 | 3 / 44 |
| ForearmL | ? | 1.1 / 15.4 | 6 / 99 |
| HandL | ? | 1.9 / 36.0 | 17 / 109 |
| UpperArmR | ? | 0.3 / 1.5 | 3 / 92 |
| ForearmR | ? | 0.8 / 14.5 | 6 / 119 |
| HandR | ? | 2.0 / 31.5 | 17 / 102 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (0 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 4, 47 damage. Health at the end 0.

Defence: 5 blows came at it (3 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 56% of the time; the worst 42° (FootL).
Ragdoll (on the ground 20.0 s): a joint more than 10° past its limit 99% of the time; the worst 64° (FootL). Fastest part with a standing leg within 0.5 m: 2.8 m/s median, 4.0 m/s at the 95th percentile (28 steps); with none: 0.0 m/s median (1174 steps).

Look: within 25° of what it attended to 80% of the time standing (80% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 2 postures (StickSettle 1, AbanicoFlurry 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 3 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–28.6 s. Steps 61, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 36 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 2 / 30 |
| Chest | ? | 0.6 / 8.7 | 3 / 23 |
| Head | ? | 0.8 / 11.6 | 15 / 73 |
| UpperArmL | ? | 1.1 / 11.1 | 13 / 179 |
| ForearmL | ? | 5.4 / 52.3 | 17 / 172 |
| HandL | ? | 7.7 / 78.9 | 14 / 79 |
| UpperArmR | ? | 0.5 / 11.4 | 2 / 91 |
| ForearmR | ? | 0.9 / 21.1 | 4 / 166 |
| HandR | ? | 1.3 / 30.2 | 11 / 112 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 1 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 5 (0 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 29 damage. Hits taken: 11, 64 damage. Health at the end 0.

Defence: 7 blows came at it (1 seen clearly, 5 at the edge of its view, 1 never seen); it raised the shield against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 42% of the time; the worst 48° (FootR).
Ragdoll (on the ground 2.5 s): a joint more than 10° past its limit 97% of the time; the worst 173° (ForearmR). Fastest part with a standing leg within 0.5 m: 4.7 m/s median, 11.6 m/s at the 95th percentile (147 steps); with none: 8.1 m/s median (4 steps).

Look: within 25° of what it attended to 77% of the time standing (78% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 85% of the time standing; its rim came within 1.1 cm of its own body (negative: inside).

Fight Rhythm (?): 7 postures (GuardChange 1, HighFeint 2, ShieldAdvance 1, Peek 2, ShieldPress 1), 2 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 3 times, knocked down 1, launched 0, got up 1; thrown into others 4 times.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.2 cm, max 0.2 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 12.47 s: SwordBackhand at B Large's Chest: passed 0.2 cm from the aim point; nearest part B Large's HandL (18.3 cm); the Chest had moved 43.2 cm since the aim was locked
- 15.12 s: SwordThrust at B Large's Head: passed 0.1 cm from the aim point; nearest part B Large's Head (14.3 cm); the Head had moved 29.7 cm since the aim was locked


## A Large

Present 0.0–28.6 s. Steps 66, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 38 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 22.6 | 0 / 0 |
| Spine | Pinned | 0.0 / 22.6 | 1 / 17 |
| Chest | Pinned | 0.6 / 17.1 | 1 / 9 |
| Head | Pinned | 0.6 / 13.4 | 14 / 56 |
| UpperArmL | Pinned | 1.0 / 18.2 | 5 / 50 |
| ForearmL | Pinned | 4.1 / 35.4 | 11 / 81 |
| HandL | Pinned | 5.9 / 53.6 | 27 / 118 |
| UpperArmR | Pinned | 0.4 / 12.2 | 1 / 59 |
| ForearmR | Pinned | 0.7 / 13.1 | 3 / 68 |
| HandR | Pinned | 1.7 / 51.7 | 9 / 59 |
| ThighL | Kinematic | 0.0 / 23.3 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 21.3 | 0 / 11 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 21.9 | 0 / 150 |
| ShinR | Kinematic | 0.0 / 11.3 | 0 / 0 |
| FootR | Kinematic | 0.0 / 8.5 | 0 / 0 |

Hits dealt: 6 (0 on the part aimed at, 0 blocked, 1 on someone in the way, 0 on allies), 45 damage. Hits taken: 8, 84 damage. Health at the end 136.

Defence: 4 blows came at it (3 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 35% of the time; the worst 97° (ShinR).

Look: within 25° of what it attended to 85% of the time standing (86% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 4 postures (BoxerSettle 1, BobAndWeave 1, ClubFeint 1, JabFeint 1), 0 of them lead-ins to blows; energy at the end 0.29.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.3 cm, max 0.4 cm (posed); physical weapon's closest pass mean 1.6 cm, max 1.8 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 11.52 s: ClubBackhand at B Small's Belly: passed 0.4 cm from the aim point; nearest part B Small's Pelvis (0.0 cm); the Belly had moved 5.8 cm since the aim was locked
- 16.80 s: ClubBackhand at B Small's Chest: passed 0.2 cm from the aim point; nearest part B Small's UpperArmR (13.6 cm); the Chest had moved 39.2 cm since the aim was locked


## B Small

Present 0.0–28.6 s. Steps 63, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 11 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 1 / 11 |
| Chest | ? | 0.2 / 2.1 | 1 / 10 |
| Head | ? | 0.3 / 2.2 | 5 / 32 |
| UpperArmL | ? | 0.4 / 4.4 | 2 / 44 |
| ForearmL | ? | 0.9 / 15.4 | 5 / 99 |
| HandL | ? | 1.5 / 36.0 | 16 / 99 |
| UpperArmR | ? | 0.2 / 2.2 | 2 / 80 |
| ForearmR | ? | 0.6 / 15.7 | 4 / 100 |
| HandR | ? | 1.3 / 24.5 | 14 / 106 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 22 damage. Hits taken: 3, 19 damage. Health at the end 0.

Defence: 4 blows came at it (3 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 4.

Pose against the joint limits (standing): a joint more than 10° past its limit 58% of the time; the worst 51° (FootL).
Ragdoll (on the ground 3.5 s): a joint more than 10° past its limit 86% of the time; the worst 174° (ForearmR). Fastest part with a standing leg within 0.5 m: 7.4 m/s median, 8.1 m/s at the 95th percentile (8 steps); with none: 4.9 m/s median (199 steps).

Look: within 25° of what it attended to 90% of the time standing (90% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 5 postures (StickSettle 2, AbanicoFlurry 2, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 0 times, knocked down 0, launched 2, got up 2; thrown into others 0 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.5 cm, max 0.5 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 9.38 s: ClubRising at A Large's KneeL: passed 0.5 cm from the aim point; nearest part A Large's ThighL (6.2 cm); the KneeL had moved 16.7 cm since the aim was locked


## B Medium

Present 0.0–28.6 s. Steps 62, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 32 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.3 | 2 / 18 |
| Chest | Pinned | 0.5 / 5.2 | 2 / 37 |
| Head | Pinned | 0.7 / 13.0 | 14 / 60 |
| UpperArmL | Pinned | 0.9 / 18.2 | 10 / 137 |
| ForearmL | Pinned | 4.5 / 53.5 | 15 / 179 |
| HandL | Pinned | 5.2 / 59.2 | 13 / 86 |
| UpperArmR | Kinematic | 0.4 / 5.1 | 2 / 97 |
| ForearmR | Kinematic | 0.8 / 19.7 | 4 / 124 |
| HandR | Kinematic | 1.2 / 25.6 | 10 / 135 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 11 (2 on the part aimed at, 2 blocked, 0 on someone in the way, 1 on allies), 111 damage. Hits taken: 3, 0 damage. Health at the end 100.

Defence: 1 blows came at it (0 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 32% of the time; the worst 43° (FootR).

Look: within 25° of what it attended to 80% of the time standing (80% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 94% of the time standing; its rim came within -3.1 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 8 postures (GuardChange 1, ShieldPress 3, Peek 2, HighFeint 2), 3 of them lead-ins to blows; energy at the end 0.66.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 6. The posed striking part passed within 3 cm of the aim point in 6; aim error mean 0.3 cm, max 1.1 cm (posed); physical weapon's closest pass mean 0.8 cm, max 1.1 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.93 s: SwordCut at A Small's Chest: passed 0.1 cm from the aim point; nearest part A Small's Chest (6.4 cm); the Chest had moved 6.4 cm since the aim was locked
- 5.73 s: SwordBackhand at A Small's Chest: passed 0.2 cm from the aim point; nearest part A Small's Spine (0.0 cm); the Chest had moved 15.4 cm since the aim was locked
- 6.55 s: SwordCleave at A Small's Chest: passed 0.2 cm from the aim point; nearest part A Small's Chest (2.3 cm); the Chest had moved 25.3 cm since the aim was locked
- 8.57 s: SwordBackhand at A Small's Belly: passed 1.1 cm from the aim point; nearest part A Small's Pelvis (0.0 cm); the Belly had moved 11.4 cm since the aim was locked
- 12.63 s: SwordThrust at A Medium's Chest: passed 0.1 cm from the aim point; nearest part A Medium's UpperArmR (13.9 cm); the Chest had moved 29.9 cm since the aim was locked
- 24.13 s: SwordCut at A Medium's Neck: passed 0.2 cm from the aim point; nearest part A Medium's Chest (8.0 cm); the Neck had moved 29.8 cm since the aim was locked


## B Large

Present 0.0–28.6 s. Steps 55, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 40 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 1 / 19 |
| Chest | Pinned | 0.6 / 7.4 | 1 / 10 |
| Head | Pinned | 0.7 / 7.4 | 10 / 51 |
| UpperArmL | Pinned | 0.9 / 11.0 | 4 / 63 |
| ForearmL | Pinned | 3.2 / 23.7 | 9 / 74 |
| HandL | Pinned | 4.8 / 45.4 | 23 / 127 |
| UpperArmR | Kinematic | 0.6 / 8.6 | 2 / 59 |
| ForearmR | Kinematic | 1.2 / 21.3 | 4 / 68 |
| HandR | Kinematic | 2.1 / 51.7 | 11 / 147 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 5 (1 on the part aimed at, 2 blocked, 0 on someone in the way, 0 on allies), 34 damage. Hits taken: 9, 65 damage. Health at the end 155.

Defence: 10 blows came at it (8 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 8.

Pose against the joint limits (standing): a joint more than 10° past its limit 32% of the time; the worst 57° (FootR).

Look: within 25° of what it attended to 86% of the time standing (86% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 5 postures (BoxerSettle 1, Shift 1, ClubFeint 1, JabFeint 1, BobAndWeave 1), 1 of them lead-ins to blows; energy at the end 0.67.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 1.3 cm, max 3.5 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.47 s: ClubSwing at A Medium's Chest: passed 0.2 cm from the aim point; nearest part A Medium's Chest (28.5 cm); the Chest had moved 60.2 cm since the aim was locked
- 6.25 s: ClubSmash at A Large's Head: passed 3.5 cm from the aim point; nearest part A Large's UpperArmL (9.4 cm); the Head had moved 34.9 cm since the aim was locked
- 9.38 s: ClubSmash at A Small's Head: passed 0.1 cm from the aim point; nearest part B Medium's HandL (53.0 cm); the Head had moved 129.8 cm since the aim was locked

## Shield contacts

208 times a shield overlapped another unit's body: 95% under 3.6 cm deep (91% under 2 cm; target 95%), the deepest 7.9 cm, the longest 0.47 s (target under 0.2 s). Each one is in events.log ("shield").

