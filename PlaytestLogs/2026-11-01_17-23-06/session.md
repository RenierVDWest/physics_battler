# Playtest 2026-11-01_17-23-06

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 53.1 s of play (3189 physics steps), 2770 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-01_17-23-06`.

## A Small

Present 0.0–53.1 s. Steps 49, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 14 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.3 | 2 / 40 |
| Chest | Ragdoll | 0.4 / 9.2 | 2 / 33 |
| Head | Ragdoll | 0.5 / 8.4 | 10 / 123 |
| UpperArmL | Ragdoll | 0.2 / 9.4 | 3 / 175 |
| ForearmL | Ragdoll | 0.6 / 26.9 | 4 / 151 |
| HandL | Ragdoll | 1.5 / 55.8 | 9 / 178 |
| UpperArmR | Ragdoll | 0.4 / 6.3 | 3 / 90 |
| ForearmR | Ragdoll | 0.9 / 15.0 | 6 / 116 |
| HandR | Ragdoll | 1.7 / 31.6 | 17 / 119 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (0 on the part aimed at, 0 blocked, 1 on someone in the way, 0 on allies), 20 damage. Hits taken: 2, 22 damage. Killed.

Defence: 8 blows came at it (2 seen clearly, 6 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 7.

Pose against the joint limits (standing): a joint more than 10° past its limit 53% of the time; the worst 123° (UpperArmR).
Ragdoll (on the ground 36.4 s): a joint more than 10° past its limit 83% of the time; the worst 165° (ShinR). Fastest part with a standing leg within 0.5 m: 0.2 m/s median, 4.8 m/s at the 95th percentile (658 steps); with none: 0.0 m/s median (1523 steps).

Look: within 25° of what it attended to 82% of the time standing (83% of the time it was within the head's reach, 96% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 6 postures (StickSettle 1, AbanicoFlurry 2, RedondaTwirl 1, BaitRetreat 2), 0 of them lead-ins to blows; energy at the end 0.88.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 10.38 s: ClubSwing at B Medium's KneeL: passed 0.0 cm from the aim point; nearest part B Medium's ShinR (8.2 cm); the KneeL had moved 46.9 cm since the aim was locked


## A Medium

Present 0.0–53.1 s. Steps 116, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 35 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 3.1 | 2 / 68 |
| Chest | Pinned | 0.6 / 25.3 | 3 / 144 |
| Head | Pinned | 0.9 / 44.6 | 17 / 174 |
| UpperArmL | Pinned | 1.2 / 51.1 | 6 / 173 |
| ForearmL | Pinned | 2.5 / 77.4 | 8 / 180 |
| HandL | Pinned | 3.3 / 82.4 | 5 / 142 |
| UpperArmR | Pinned | 0.5 / 17.3 | 2 / 64 |
| ForearmR | Pinned | 1.0 / 21.7 | 5 / 117 |
| HandR | Pinned | 1.7 / 27.2 | 18 / 84 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 10 (1 on the part aimed at, 1 blocked, 1 on someone in the way, 4 on allies), 111 damage. Hits taken: 11, 72 damage. Health at the end 28.

Defence: 9 blows came at it (7 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 4.

Pose against the joint limits (standing): a joint more than 10° past its limit 43% of the time; the worst 129° (UpperArmR).
Ragdoll (on the ground 1.4 s): a joint more than 10° past its limit 85% of the time; the worst 129° (ForearmL). Fastest part with a standing leg within 0.5 m: 4.9 m/s median, 5.9 m/s at the 95th percentile (34 steps); with none: 0.6 m/s median (51 steps).

Look: within 25° of what it attended to 73% of the time standing (80% of the time it was within the head's reach, 89% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 77% of the time standing; its rim came within -0.2 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 8 postures (GuardChange 2, HighFeint 2, ShieldPress 3, Peek 1), 1 of them lead-ins to blows; energy at the end 0.45.

Balance: stumbled 3 times, knocked down 1, launched 0, got up 1; thrown into others 0 times.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.58 s: SwordThrust at B Large's Belly: passed 0.1 cm from the aim point; nearest part B Large's Spine (8.0 cm); the Belly had moved 13.8 cm since the aim was locked
- 29.78 s: SwordCut at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's HandR (0.0 cm); the Belly had moved 18.1 cm since the aim was locked
- 49.08 s: SwordCleave at B Medium's ElbowL: passed 0.0 cm from the aim point; nearest part B Medium's FootL (69.2 cm); the ElbowL had moved 138.7 cm since the aim was locked


## A Large

Present 0.0–53.1 s. Steps 100, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 57 cm from the support point; hips dropped for reach by up to 5 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 10.7 | 2 / 90 |
| Chest | Pinned | 1.0 / 35.5 | 2 / 90 |
| Head | Pinned | 1.3 / 67.6 | 13 / 171 |
| UpperArmL | Pinned | 1.6 / 56.1 | 7 / 179 |
| ForearmL | Pinned | 4.4 / 64.9 | 11 / 150 |
| HandL | Pinned | 5.3 / 48.3 | 24 / 154 |
| UpperArmR | Pinned | 0.8 / 51.4 | 2 / 124 |
| ForearmR | Pinned | 1.6 / 74.5 | 5 / 178 |
| HandR | Pinned | 2.9 / 54.0 | 13 / 126 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 16 (2 on the part aimed at, 5 blocked, 2 on someone in the way, 0 on allies), 142 damage. Hits taken: 5, 65 damage. Health at the end 107.

Defence: 1 blows came at it (1 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 43% of the time; the worst 128° (UpperArmR).

Look: within 25° of what it attended to 84% of the time standing (85% of the time it was within the head's reach, 97% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 17 postures (BoxerSettle 3, ClubFeint 5, BobAndWeave 2, BoxerStalk 3, JabFeint 3, Shift 1), 3 of them lead-ins to blows; energy at the end 0.89.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 6. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 1.0 cm, max 5.7 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.82 s: ClubSwing at B Small's KneeL: passed 0.0 cm from the aim point; nearest part B Small's ThighR (16.1 cm); the KneeL had moved 16.2 cm since the aim was locked
- 14.63 s: ClubSmash at B Large's Head: passed 0.0 cm from the aim point; nearest part B Large's ForearmL (1.9 cm); the Head had moved 21.6 cm since the aim was locked
- 20.32 s: ClubSmash at B Small's Chest: passed 5.7 cm from the aim point; nearest part B Small's Chest (21.9 cm); the Chest had moved 43.4 cm since the aim was locked
- 22.67 s: ClubRising at B Small's Chest: passed 0.1 cm from the aim point; nearest part B Small's Chest (7.1 cm); the Chest had moved 16.6 cm since the aim was locked
- 31.42 s: ClubSwing at B Medium's Head: passed 0.1 cm from the aim point; nearest part A Medium's HandL (32.9 cm); the Head had moved 55.8 cm since the aim was locked
- 48.37 s: ClubSwing at B Medium's Head: passed 0.1 cm from the aim point; nearest part B Medium's UpperArmL (26.8 cm); the Head had moved 55.2 cm since the aim was locked


## B Small

Present 0.0–53.1 s. Steps 87, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 19 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 4.1 | 3 / 177 |
| Chest | Ragdoll | 0.4 / 28.5 | 3 / 144 |
| Head | Ragdoll | 0.7 / 25.6 | 15 / 179 |
| UpperArmL | Ragdoll | 0.9 / 45.1 | 6 / 180 |
| ForearmL | Ragdoll | 1.8 / 49.0 | 10 / 158 |
| HandL | Ragdoll | 3.1 / 75.3 | 24 / 177 |
| UpperArmR | Ragdoll | 0.4 / 5.9 | 3 / 131 |
| ForearmR | Ragdoll | 1.1 / 21.5 | 8 / 155 |
| HandR | Ragdoll | 1.8 / 25.0 | 21 / 137 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 3 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 3 on allies), 20 damage. Hits taken: 9, 69 damage. Killed.

Defence: 4 blows came at it (2 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 63% of the time; the worst 129° (UpperArmR).
Ragdoll (on the ground 27.9 s): a joint more than 10° past its limit 98% of the time; the worst 164° (HandR). Fastest part with a standing leg within 0.5 m: 3.0 m/s median, 7.5 m/s at the 95th percentile (318 steps); with none: 0.0 m/s median (1356 steps).

Look: within 25° of what it attended to 79% of the time standing (81% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 10 postures (StickSettle 2, AbanicoFlurry 2, TriangleStep 1, RedondaTwirl 2, StickStalk 1, BaitRetreat 2), 2 of them lead-ins to blows; energy at the end 0.55.

Balance: stumbled 2 times, knocked down 0, launched 2, got up 2; thrown into others 3 times.


## B Medium

Present 0.0–53.1 s. Steps 89, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 41 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.7 | 2 / 41 |
| Chest | Ragdoll | 0.6 / 13.9 | 2 / 55 |
| Head | Ragdoll | 0.7 / 15.2 | 14 / 78 |
| UpperArmL | Ragdoll | 0.9 / 21.3 | 5 / 138 |
| ForearmL | Ragdoll | 2.5 / 50.2 | 8 / 151 |
| HandL | Ragdoll | 3.5 / 69.6 | 5 / 83 |
| UpperArmR | Ragdoll | 0.4 / 8.1 | 2 / 69 |
| ForearmR | Ragdoll | 0.9 / 19.4 | 4 / 84 |
| HandR | Ragdoll | 1.5 / 27.2 | 15 / 123 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 15 (0 on the part aimed at, 3 blocked, 1 on someone in the way, 7 on allies), 147 damage. Hits taken: 13, 123 damage. Killed.

Defence: 11 blows came at it (8 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 8.

Pose against the joint limits (standing): a joint more than 10° past its limit 40% of the time; the worst 146° (ShinR).
Ragdoll (on the ground 9.5 s): a joint more than 10° past its limit 53% of the time; the worst 169° (ForearmL). Fastest part with a standing leg within 0.5 m: 3.7 m/s median, 5.8 m/s at the 95th percentile (12 steps); with none: 1.3 m/s median (558 steps).

Look: within 25° of what it attended to 85% of the time standing (85% of the time it was within the head's reach, 95% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 91% of the time standing; its rim came within -1.4 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 6 postures (GuardChange 1, HighFeint 2, Peek 2, ShieldPress 1), 1 of them lead-ins to blows; energy at the end 0.13.

Balance: stumbled 5 times, knocked down 2, launched 0, got up 2; thrown into others 0 times.

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.1 cm, max 0.1 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.05 s: SwordCut at A Small's Belly: passed 0.0 cm from the aim point; nearest part A Small's Pelvis (0.0 cm); the Belly had moved 21.7 cm since the aim was locked
- 5.98 s: SwordCut at A Small's Belly: passed 0.0 cm from the aim point; nearest part A Small's Spine (8.0 cm); the Belly had moved 26.1 cm since the aim was locked
- 11.45 s: SwordCleave at A Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's ForearmL (10.6 cm); the Belly had moved 65.4 cm since the aim was locked
- 18.62 s: SwordBackhand at A Medium's Belly: passed 0.0 cm from the aim point; nearest part B Small's Head (39.2 cm); the Belly had moved 56.1 cm since the aim was locked
- 30.05 s: SwordCut at A Medium's Head: passed 0.0 cm from the aim point; nearest part A Medium's Chest (22.1 cm); the Head had moved 45.5 cm since the aim was locked


## B Large

Present 0.0–53.1 s. Steps 39, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 32 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 1.7 | 1 / 81 |
| Chest | Ragdoll | 0.6 / 26.7 | 1 / 46 |
| Head | Ragdoll | 0.7 / 35.4 | 10 / 174 |
| UpperArmL | Ragdoll | 0.9 / 43.2 | 4 / 175 |
| ForearmL | Ragdoll | 2.6 / 60.9 | 7 / 175 |
| HandL | Ragdoll | 3.6 / 75.5 | 16 / 164 |
| UpperArmR | Ragdoll | 0.6 / 6.2 | 2 / 55 |
| ForearmR | Ragdoll | 1.1 / 20.0 | 4 / 62 |
| HandR | Ragdoll | 2.0 / 54.0 | 10 / 67 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (1 on the part aimed at, 0 blocked, 1 on someone in the way, 0 on allies), 39 damage. Hits taken: 14, 137 damage. Killed.

Defence: 8 blows came at it (5 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 7.

Pose against the joint limits (standing): a joint more than 10° past its limit 15% of the time; the worst 119° (UpperArmR).
Ragdoll (on the ground 25.6 s): a joint more than 10° past its limit 98% of the time; the worst 72° (ForearmL). Fastest part with a standing leg within 0.5 m: - median, - at the 95th percentile (0 steps); with none: 0.0 m/s median (1536 steps).

Look: within 25° of what it attended to 83% of the time standing (84% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 3 postures (BoxerSettle 1, BoxerStalk 1, BobAndWeave 1), 1 of them lead-ins to blows; energy at the end 0.78.

Balance: stumbled 4 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 10.25 s: ClubSwing at A Medium's Head: passed 0.1 cm from the aim point; nearest part A Medium's Chest (45.0 cm); the Head had moved 69.7 cm since the aim was locked

