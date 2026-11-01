# Playtest 2026-11-01_21-20-02

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 62.9 s of play (3773 physics steps), 2574 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-01_21-20-02`.

## A Small

Present 0.0–62.9 s. Steps 40, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 12 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.1 | 3 / 43 |
| Chest | Ragdoll | 0.5 / 8.4 | 2 / 34 |
| Head | Ragdoll | 0.6 / 8.0 | 8 / 53 |
| UpperArmL | Ragdoll | 0.9 / 15.7 | 8 / 178 |
| ForearmL | Ragdoll | 1.7 / 26.6 | 11 / 175 |
| HandL | Ragdoll | 3.0 / 36.0 | 19 / 178 |
| UpperArmR | Ragdoll | 0.3 / 2.8 | 2 / 92 |
| ForearmR | Ragdoll | 0.7 / 14.5 | 5 / 119 |
| HandR | Ragdoll | 1.5 / 31.5 | 15 / 102 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (0 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 4, 57 damage. Killed.

Defence: 6 blows came at it (5 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 5.

Pose against the joint limits (standing): a joint more than 10° past its limit 47% of the time; the worst 42° (FootL).
Ragdoll (on the ground 47.3 s): a joint more than 10° past its limit 27% of the time; the worst 172° (ForearmR). Fastest part with a standing leg within 0.5 m: 0.0 m/s median, 4.4 m/s at the 95th percentile (1032 steps); with none: 0.0 m/s median (1808 steps).

Look: within 25° of what it attended to 81% of the time standing (85% of the time it was within the head's reach, 90% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 5 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 2, RedondaTwirl 1), 0 of them lead-ins to blows; energy at the end 0.75.

Balance: stumbled 3 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 1.1 cm, max 1.1 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.60 s: ClubSwing at B Medium's Head: passed 1.1 cm from the aim point; nearest part B Medium's Head (25.5 cm); the Head had moved 45.6 cm since the aim was locked


## A Medium

Present 0.0–62.9 s. Steps 115, idle shuffles 8. Feet slid at most 0.0 cm while planted; the weight was at most 47 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 2.3 | 1 / 71 |
| Chest | ? | 0.4 / 10.2 | 2 / 36 |
| Head | ? | 0.5 / 14.9 | 12 / 155 |
| UpperArmL | ? | 0.6 / 13.3 | 3 / 80 |
| ForearmL | ? | 1.5 / 38.5 | 4 / 101 |
| HandL | ? | 2.0 / 71.1 | 3 / 76 |
| UpperArmR | ? | 0.4 / 22.4 | 2 / 91 |
| ForearmR | ? | 0.9 / 21.7 | 4 / 178 |
| HandR | ? | 1.5 / 25.3 | 14 / 178 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 10 (1 on the part aimed at, 2 blocked, 0 on someone in the way, 4 on allies), 75 damage. Hits taken: 9, 21 damage. Health at the end 0.

Defence: 14 blows came at it (6 seen clearly, 8 at the edge of its view, 0 never seen); it raised the shield against 9.

Pose against the joint limits (standing): a joint more than 10° past its limit 29% of the time; the worst 103° (ThighR).

Look: within 25° of what it attended to 79% of the time standing (80% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 89% of the time standing; its rim came within -1.2 cm of its own body (negative: inside).

Fight Rhythm (?): 13 postures (GuardChange 4, Peek 2, HighFeint 2, ShieldPress 3, ShieldAdvance 2), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 3 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.4 cm, max 0.6 cm (posed); physical weapon's closest pass mean 0.5 cm, max 0.6 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 9.52 s: SwordCleave at B Large's Chest: passed 0.6 cm from the aim point; nearest part B Large's HandR (73.3 cm); the Chest had moved 96.8 cm since the aim was locked
- 21.55 s: SwordBackhand at B Large's ElbowL: passed 0.4 cm from the aim point; nearest part B Large's Chest (12.7 cm); the ElbowL had moved 37.7 cm since the aim was locked
- 49.28 s: SwordBackhand at B Medium's ElbowR: passed 0.3 cm from the aim point; nearest part B Medium's UpperArmR (9.5 cm); the ElbowR had moved 10.0 cm since the aim was locked


## A Large

Present 0.0–62.9 s. Steps 109, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 62 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 1.6 | 1 / 65 |
| Chest | Pinned | 0.6 / 35.1 | 2 / 151 |
| Head | Pinned | 0.8 / 61.4 | 10 / 106 |
| UpperArmL | Pinned | 1.1 / 77.4 | 6 / 153 |
| ForearmL | Pinned | 3.8 / 51.5 | 10 / 170 |
| HandL | Pinned | 5.4 / 62.0 | 23 / 157 |
| UpperArmR | Pinned | 0.5 / 9.4 | 2 / 59 |
| ForearmR | Pinned | 1.2 / 24.8 | 4 / 98 |
| HandR | Pinned | 2.4 / 51.7 | 11 / 180 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 18 (1 on the part aimed at, 3 blocked, 2 on someone in the way, 1 on allies), 202 damage. Hits taken: 7, 66 damage. Health at the end 154.

Defence: 1 blows came at it (0 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 24% of the time; the worst 114° (ShinR).

Look: within 25° of what it attended to 86% of the time standing (86% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 18 postures (BoxerSettle 1, ClubFeint 4, BobAndWeave 2, JabFeint 4, BoxerStalk 7), 3 of them lead-ins to blows; energy at the end 0.99.

Blows: 7. The posed striking part passed within 3 cm of the aim point in 7; aim error mean 0.4 cm, max 0.9 cm (posed); physical weapon's closest pass mean 0.6 cm, max 1.1 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.85 s: ClubSmash at B Small's Head: passed 0.3 cm from the aim point; nearest part B Small's UpperArmR (27.3 cm); the Head had moved 47.5 cm since the aim was locked
- 8.42 s: ClubRising at B Large's Chest: passed 0.3 cm from the aim point; nearest part B Large's Chest (24.1 cm); the Chest had moved 72.9 cm since the aim was locked
- 24.87 s: ClubSmash at B Large's Chest: passed 0.2 cm from the aim point; nearest part B Large's Head (59.6 cm); the Chest had moved 101.1 cm since the aim was locked
- 27.45 s: ClubSwing at B Small's Neck: passed 0.4 cm from the aim point; nearest part B Small's Head (19.3 cm); the Neck had moved 18.6 cm since the aim was locked
- 32.47 s: ClubSwing at B Medium's KneeR: passed 0.4 cm from the aim point; nearest part B Medium's ShinR (1.6 cm); the KneeR had moved 7.9 cm since the aim was locked
- 43.08 s: ClubSwing at B Medium's Belly: passed 0.3 cm from the aim point; nearest part B Medium's UpperArmR (8.4 cm); the Belly had moved 28.8 cm since the aim was locked
- 50.07 s: ClubRising at B Medium's Belly: passed 0.9 cm from the aim point; nearest part B Medium's UpperArmL (9.3 cm); the Belly had moved 49.9 cm since the aim was locked


## B Small

Present 0.0–62.9 s. Steps 78, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 11 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 2 / 74 |
| Chest | ? | 0.4 / 12.0 | 2 / 50 |
| Head | ? | 0.6 / 15.5 | 13 / 147 |
| UpperArmL | ? | 0.7 / 20.7 | 5 / 147 |
| ForearmL | ? | 1.6 / 23.1 | 8 / 103 |
| HandL | ? | 2.7 / 36.0 | 24 / 172 |
| UpperArmR | ? | 0.4 / 12.8 | 2 / 80 |
| ForearmR | ? | 0.8 / 15.7 | 5 / 100 |
| HandR | ? | 1.4 / 24.5 | 15 / 106 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 5 (1 on the part aimed at, 4 blocked, 0 on someone in the way, 0 on allies), 8 damage. Hits taken: 5, 61 damage. Health at the end 0.

Defence: 2 blows came at it (0 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 67% of the time; the worst 110° (FootL).
Ragdoll (on the ground 34.6 s): a joint more than 10° past its limit 11% of the time; the worst 168° (ShinR). Fastest part with a standing leg within 0.5 m: 0.0 m/s median, 4.6 m/s at the 95th percentile (999 steps); with none: 0.0 m/s median (1078 steps).

Look: within 25° of what it attended to 75% of the time standing (76% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 9 postures (StickSettle 3, AbanicoFlurry 4, RedondaTwirl 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 0 times, knocked down 0, launched 4, got up 3; thrown into others 1 time.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.6 cm, max 0.6 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 22.50 s: ClubSwing at A Medium's KneeL: passed 0.6 cm from the aim point; nearest part A Large's ShinL (41.4 cm); the KneeL had moved 55.1 cm since the aim was locked


## B Medium

Present 0.0–62.9 s. Steps 86, idle shuffles 5. Feet slid at most 0.0 cm while planted; the weight was at most 33 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 2.0 | 1 / 112 |
| Chest | Ragdoll | 0.4 / 6.0 | 1 / 17 |
| Head | Ragdoll | 0.4 / 7.4 | 10 / 51 |
| UpperArmL | Ragdoll | 0.6 / 8.7 | 3 / 127 |
| ForearmL | Ragdoll | 1.5 / 23.5 | 5 / 122 |
| HandL | Ragdoll | 2.5 / 60.3 | 4 / 118 |
| UpperArmR | Ragdoll | 0.3 / 7.1 | 1 / 97 |
| ForearmR | Ragdoll | 0.6 / 19.7 | 2 / 99 |
| HandR | Ragdoll | 0.9 / 25.6 | 9 / 115 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 12 (2 on the part aimed at, 2 blocked, 2 on someone in the way, 3 on allies), 120 damage. Hits taken: 13, 56 damage. Killed.

Defence: 11 blows came at it (7 seen clearly, 4 at the edge of its view, 0 never seen); it raised the shield against 5.

Pose against the joint limits (standing): a joint more than 10° past its limit 34% of the time; the worst 80° (ShinR).
Ragdoll (on the ground 7.0 s): a joint more than 10° past its limit 84% of the time; the worst 174° (ForearmL). Fastest part with a standing leg within 0.5 m: 5.4 m/s median, 16.8 m/s at the 95th percentile (71 steps); with none: 2.3 m/s median (350 steps).

Look: within 25° of what it attended to 85% of the time standing (87% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 86% of the time standing; its rim came within -1.9 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 10 postures (GuardChange 3, Peek 4, ShieldPress 2, HighFeint 1), 2 of them lead-ins to blows; energy at the end 0.65.

Balance: stumbled 5 times, knocked down 3, launched 0, got up 3; thrown into others 0 times.

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.5 cm, max 1.7 cm (posed); physical weapon's closest pass mean 0.4 cm, max 0.7 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.92 s: SwordBackhand at A Small's Chest: passed 1.7 cm from the aim point; nearest part A Small's Pelvis (25.3 cm); the Chest had moved 33.8 cm since the aim was locked
- 6.07 s: SwordBackhand at A Small's Chest: passed 0.4 cm from the aim point; nearest part A Small's Chest (13.8 cm); the Chest had moved 17.7 cm since the aim was locked
- 9.53 s: SwordBackhand at A Small's ElbowL: passed 0.3 cm from the aim point; nearest part A Small's ThighL (5.2 cm); the ElbowL had moved 20.5 cm since the aim was locked
- 11.18 s: SwordCut at A Small's Head: passed 0.1 cm from the aim point; nearest part A Small's Head (22.4 cm); the Head had moved 36.1 cm since the aim was locked
- 15.52 s: SwordCut at A Small's Neck: passed 0.3 cm from the aim point; nearest part A Small's Chest (4.0 cm); the Neck had moved 9.9 cm since the aim was locked


## B Large

Present 0.0–62.9 s. Steps 44, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 50 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 1.3 | 2 / 74 |
| Chest | Ragdoll | 0.8 / 30.7 | 2 / 44 |
| Head | Ragdoll | 1.1 / 24.0 | 9 / 45 |
| UpperArmL | Ragdoll | 1.5 / 53.0 | 7 / 129 |
| ForearmL | Ragdoll | 4.1 / 66.9 | 10 / 168 |
| HandL | Ragdoll | 5.0 / 51.3 | 20 / 171 |
| UpperArmR | Ragdoll | 0.9 / 23.9 | 2 / 59 |
| ForearmR | Ragdoll | 1.5 / 13.1 | 5 / 68 |
| HandR | Ragdoll | 2.7 / 51.7 | 13 / 84 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 3 (0 on the part aimed at, 3 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 13, 158 damage. Killed.

Defence: 15 blows came at it (6 seen clearly, 9 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 10.

Pose against the joint limits (standing): a joint more than 10° past its limit 26% of the time; the worst 155° (ShinL).
Ragdoll (on the ground 38.5 s): a joint more than 10° past its limit 2% of the time; the worst 45° (ForearmR). Fastest part with a standing leg within 0.5 m: 4.7 m/s median, 4.7 m/s at the 95th percentile (1 steps); with none: 0.0 m/s median (2310 steps).

Look: within 25° of what it attended to 85% of the time standing (85% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 3 postures (BoxerSettle 1, ClubFeint 1, BobAndWeave 1), 0 of them lead-ins to blows; energy at the end 0.43.

Balance: stumbled 4 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

## Shield contacts

130 times a shield overlapped another unit's body: 95% under 7.7 cm deep (62% under 2 cm; target 95%), the deepest 13.0 cm, the longest 0.30 s (target under 0.2 s). Each one is in events.log ("shield").

