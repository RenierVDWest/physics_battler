# Playtest 2026-11-01_10-50-42

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 50.8 s of play (3046 physics steps), 2704 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-01_10-50-42`.

## A Small

Present 0.0–50.8 s. Steps 71, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 16 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.2 | 1 / 22 |
| Chest | Ragdoll | 0.2 / 4.5 | 1 / 22 |
| Head | Ragdoll | 0.2 / 5.1 | 4 / 51 |
| UpperArmL | Ragdoll | 0.3 / 8.8 | 2 / 56 |
| ForearmL | Ragdoll | 0.7 / 9.5 | 6 / 167 |
| HandL | Ragdoll | 1.5 / 24.5 | 14 / 85 |
| UpperArmR | Ragdoll | 0.1 / 2.7 | 1 / 85 |
| ForearmR | Ragdoll | 0.3 / 10.0 | 2 / 119 |
| HandR | Ragdoll | 0.5 / 27.3 | 9 / 133 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 7 (1 on the part aimed at, 4 blocked, 2 on someone in the way, 0 on allies), 37 damage. Hits taken: 3, 68 damage. Killed.

Defence: 6 blows came at it (6 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 6.

Look: within 25° of what it attended to 79% of the time standing (83% of the time it was within the head's reach, 96% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 8 postures (StickSettle 2, AbanicoFlurry 3, TriangleStep 1, RedondaTwirl 2), 1 of them lead-ins to blows; energy at the end 0.45.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–50.8 s. Steps 124, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 70 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.9 | 1 / 89 |
| Chest | Pinned | 0.3 / 15.8 | 1 / 89 |
| Head | Pinned | 0.3 / 28.8 | 8 / 175 |
| UpperArmL | Pinned | 0.5 / 43.0 | 3 / 180 |
| ForearmL | Pinned | 1.0 / 51.3 | 5 / 179 |
| HandL | Pinned | 1.3 / 53.3 | 16 / 180 |
| UpperArmR | Pinned | 0.1 / 3.8 | 1 / 47 |
| ForearmR | Pinned | 0.3 / 10.6 | 2 / 177 |
| HandR | Pinned | 0.4 / 22.6 | 5 / 170 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 12 (2 on the part aimed at, 3 blocked, 0 on someone in the way, 2 on allies), 95 damage. Hits taken: 3, 21 damage. Health at the end 79.

Defence: 2 blows came at it (2 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 0.

Look: within 25° of what it attended to 86% of the time standing (86% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 98% of the time standing; its rim came within 1.1 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 9 postures (GuardChange 1, HighFeint 2, Peek 2, ShieldPress 2, ShieldAdvance 1, ShieldSettle 1), 1 of them lead-ins to blows; energy at the end 0.58.

Balance: stumbled 3 times, knocked down 1, launched 0, got up 1; thrown into others 0 times.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.1 cm, max 0.1 cm. 2 reached the part they were aimed at (nearest part within 5 cm at contact).

- 10.43 s: SwordThrust at B Large's Head: passed 0.1 cm from the aim point; nearest part B Large's UpperArmL (4.7 cm); the Head had moved 52.3 cm since the aim was locked
- 14.03 s: SwordThrust at B Large's Belly: passed 0.1 cm from the aim point; nearest part B Large's Spine (0.0 cm); the Belly had moved 5.7 cm since the aim was locked
- 20.57 s: SwordThrust at B Large's Neck: passed 0.1 cm from the aim point; nearest part B Large's Chest (21.7 cm); the Neck had moved 52.5 cm since the aim was locked
- 21.92 s: SwordBackhand at B Large's KneeR: passed 0.1 cm from the aim point; nearest part B Large's ShinR (0.0 cm); the KneeR had moved 2.9 cm since the aim was locked


## A Large

Present 0.0–50.8 s. Steps 99, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 49 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 0 / 13 |
| Chest | Pinned | 0.2 / 5.7 | 0 / 15 |
| Head | Pinned | 0.2 / 5.6 | 4 / 35 |
| UpperArmL | Pinned | 0.3 / 11.3 | 2 / 56 |
| ForearmL | Pinned | 1.7 / 29.8 | 5 / 47 |
| HandL | Pinned | 3.1 / 33.9 | 13 / 95 |
| UpperArmR | Pinned | 0.1 / 2.2 | 1 / 90 |
| ForearmR | Pinned | 0.5 / 32.3 | 2 / 108 |
| HandR | Pinned | 0.9 / 45.1 | 6 / 150 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 18 (5 on the part aimed at, 5 blocked, 2 on someone in the way, 1 on allies), 240 damage. Hits taken: 2, 14 damage. Health at the end 206.

Defence: 1 blows came at it (1 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 0.

Look: within 25° of what it attended to 87% of the time standing (87% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 13 postures (BoxerSettle 2, ClubFeint 5, BobAndWeave 2, JabFeint 4), 3 of them lead-ins to blows; energy at the end 0.49.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.5 cm, max 1.4 cm. 3 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.85 s: ClubSmash at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's UpperArmR (11.7 cm); the Neck had moved 30.3 cm since the aim was locked
- 10.33 s: ClubRising at B Small's Chest: passed 0.1 cm from the aim point; nearest part B Small's Chest (0.0 cm); the Chest had moved 13.3 cm since the aim was locked
- 32.72 s: ClubSmash at B Medium's Head: passed 0.0 cm from the aim point; nearest part B Medium's Head (3.4 cm); the Head had moved 10.8 cm since the aim was locked
- 45.87 s: ClubSwing at B Medium's Head: passed 0.1 cm from the aim point; nearest part B Medium's Head (0.0 cm); the Head had moved 15.3 cm since the aim was locked


## B Small

Present 0.0–50.8 s. Steps 18, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 8 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.0 | 1 / 4 |
| Chest | Ragdoll | 0.1 / 0.7 | 1 / 3 |
| Head | Ragdoll | 0.2 / 0.8 | 3 / 8 |
| UpperArmL | Ragdoll | 0.2 / 1.1 | 1 / 30 |
| ForearmL | Ragdoll | 0.5 / 9.6 | 3 / 91 |
| HandL | Ragdoll | 1.1 / 24.6 | 9 / 71 |
| UpperArmR | Ragdoll | 0.2 / 0.8 | 2 / 56 |
| ForearmR | Ragdoll | 0.5 / 9.3 | 3 / 89 |
| HandR | Ragdoll | 0.9 / 21.2 | 15 / 102 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 5, 63 damage. Killed.

Defence: 2 blows came at it (0 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 2.

Look: within 25° of what it attended to 67% of the time standing (82% of the time it was within the head's reach, 82% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.65.

Balance: stumbled 1 time, knocked down 1, launched 2, got up 1; thrown into others 2 times.


## B Medium

Present 0.0–50.8 s. Steps 85, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 40 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.1 | 0 / 10 |
| Chest | Ragdoll | 0.1 / 3.1 | 1 / 12 |
| Head | Ragdoll | 0.2 / 2.9 | 5 / 47 |
| UpperArmL | Ragdoll | 0.2 / 5.6 | 2 / 150 |
| ForearmL | Ragdoll | 0.9 / 54.0 | 3 / 128 |
| HandL | Ragdoll | 1.3 / 37.2 | 15 / 179 |
| UpperArmR | Ragdoll | 0.1 / 3.7 | 1 / 48 |
| ForearmR | Ragdoll | 0.3 / 6.8 | 1 / 45 |
| HandR | Ragdoll | 0.5 / 22.8 | 6 / 119 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 6 (2 on the part aimed at, 0 blocked, 0 on someone in the way, 3 on allies), 97 damage. Hits taken: 17, 104 damage. Killed.

Defence: 20 blows came at it (11 seen clearly, 9 at the edge of its view, 0 never seen); it raised the shield against 12.

Look: within 25° of what it attended to 88% of the time standing (90% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 98% of the time standing; its rim came within 1.0 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 5 postures (GuardChange 2, HighFeint 1, Peek 2), 0 of them lead-ins to blows; energy at the end 0.50.

Balance: stumbled 7 times, knocked down 1, launched 0, got up 1; thrown into others 0 times.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.3 cm, max 0.4 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 6.25 s: SwordCut at A Small's Head: passed 0.0 cm from the aim point; nearest part A Small's ForearmR (27.0 cm); the Head had moved 57.3 cm since the aim was locked
- 19.12 s: SwordCut at A Small's Belly: passed 0.0 cm from the aim point; nearest part A Small's Chest (26.6 cm); the Belly had moved 42.7 cm since the aim was locked
- 20.87 s: SwordCut at A Small's Neck: passed 0.0 cm from the aim point; nearest part A Small's Chest (0.0 cm); the Neck had moved 8.6 cm since the aim was locked
- 22.62 s: SwordCut at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (2.3 cm); the Chest had moved 4.0 cm since the aim was locked


## B Large

Present 0.0–50.8 s. Steps 40, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 28 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.2 | 1 / 15 |
| Chest | Ragdoll | 0.2 / 6.5 | 0 / 11 |
| Head | Ragdoll | 0.3 / 9.0 | 5 / 59 |
| UpperArmL | Ragdoll | 0.3 / 8.5 | 2 / 49 |
| ForearmL | Ragdoll | 1.4 / 30.6 | 4 / 105 |
| HandL | Ragdoll | 2.5 / 33.9 | 12 / 158 |
| UpperArmR | Ragdoll | 0.3 / 7.9 | 1 / 43 |
| ForearmR | Ragdoll | 0.8 / 12.9 | 3 / 64 |
| HandR | Ragdoll | 1.5 / 45.1 | 8 / 75 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (0 on the part aimed at, 2 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 19, 227 damage. Killed.

Defence: 14 blows came at it (7 seen clearly, 7 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 11.

Look: within 25° of what it attended to 83% of the time standing (83% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 4 postures (BoxerSettle 1, BoxerStalk 1, ClubFeint 1, BobAndWeave 1), 1 of them lead-ins to blows; energy at the end 0.63.

Balance: stumbled 3 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

