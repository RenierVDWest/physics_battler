# Playtest 2026-11-01_10-46-19

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 11.7 s of play (703 physics steps), 664 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-01_10-46-19`.

## A Small

Present 0.0–11.7 s. Steps 25, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 11 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.6 | 1 / 10 |
| Chest | Ragdoll | 0.2 / 2.0 | 1 / 14 |
| Head | Ragdoll | 0.3 / 3.3 | 5 / 94 |
| UpperArmL | Ragdoll | 0.3 / 3.5 | 2 / 98 |
| ForearmL | Ragdoll | 0.7 / 16.4 | 5 / 91 |
| HandL | Ragdoll | 1.3 / 24.5 | 16 / 167 |
| UpperArmR | Ragdoll | 0.2 / 2.3 | 2 / 85 |
| ForearmR | Ragdoll | 0.6 / 10.0 | 4 / 119 |
| HandR | Ragdoll | 1.1 / 27.3 | 16 / 133 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 4, 75 damage. Killed.

Defence: 3 blows came at it (1 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Look: within 25° of what it attended to 69% of the time standing (75% of the time it was within the head's reach, 92% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 3 postures (StickSettle 1, AbanicoFlurry 1, RedondaTwirl 1), 0 of them lead-ins to blows; energy at the end 0.68.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–11.7 s. Steps 16, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 10 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 0 / 7 |
| Chest | ? | 0.1 / 2.0 | 0 / 11 |
| Head | ? | 0.2 / 5.5 | 5 / 19 |
| UpperArmL | ? | 0.2 / 6.1 | 2 / 106 |
| ForearmL | ? | 1.1 / 46.3 | 4 / 86 |
| HandL | ? | 1.4 / 53.0 | 13 / 166 |
| UpperArmR | ? | 0.2 / 4.2 | 1 / 47 |
| ForearmR | ? | 0.7 / 6.8 | 3 / 47 |
| HandR | ? | 1.1 / 22.6 | 14 / 110 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 2, 35 damage. Health at the end 0.

Defence: 2 blows came at it (1 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 1.

Look: within 25° of what it attended to 72% of the time standing (76% of the time it was within the head's reach, 94% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 91% of the time standing; its rim came within 22.2 cm of its own body (negative: inside).

Fight Rhythm (?): 3 postures (GuardChange 1, HighFeint 1, Peek 1), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 2, launched 0, got up 1; thrown into others 0 times.


## A Large

Present 0.0–11.7 s. Steps 24, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 26 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 1 / 21 |
| Chest | ? | 0.3 / 9.9 | 1 / 18 |
| Head | ? | 0.3 / 5.0 | 6 / 50 |
| UpperArmL | ? | 0.4 / 11.6 | 3 / 58 |
| ForearmL | ? | 1.9 / 18.7 | 6 / 58 |
| HandL | ? | 3.6 / 33.9 | 14 / 89 |
| UpperArmR | ? | 0.2 / 2.2 | 1 / 43 |
| ForearmR | ? | 0.7 / 22.4 | 2 / 64 |
| HandR | ? | 1.2 / 45.1 | 7 / 144 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 5 (1 on the part aimed at, 0 blocked, 1 on someone in the way, 0 on allies), 52 damage. Hits taken: 0, 0 damage. Health at the end 0.

Defence: 2 blows came at it (1 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Look: within 25° of what it attended to 82% of the time standing (81% of the time it was within the head's reach, 97% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 2 postures (BoxerSettle 1, ClubFeint 1), 0 of them lead-ins to blows; energy at the end 0.00.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.82 s: ClubSwing at B Small's Neck: passed 0.1 cm from the aim point; nearest part B Small's UpperArmR (11.9 cm); the Neck had moved 30.9 cm since the aim was locked


## B Small

Present 0.0–11.7 s. Steps 21, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 8 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.0 | 0 / 4 |
| Chest | Pinned | 0.1 / 0.8 | 0 / 4 |
| Head | Pinned | 0.1 / 0.8 | 2 / 7 |
| UpperArmL | Pinned | 0.1 / 1.7 | 1 / 30 |
| ForearmL | Pinned | 0.4 / 9.6 | 3 / 91 |
| HandL | Pinned | 0.9 / 24.6 | 8 / 71 |
| UpperArmR | Kinematic | 0.1 / 0.6 | 1 / 56 |
| ForearmR | Kinematic | 0.4 / 9.3 | 2 / 89 |
| HandR | Kinematic | 0.7 / 21.2 | 11 / 102 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 6, 40 damage. Health at the end 20.

Defence: 1 blows came at it (0 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Look: within 25° of what it attended to 85% of the time standing (86% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 2 postures (StickSettle 1, AbanicoFlurry 1), 0 of them lead-ins to blows; energy at the end 0.72.

Balance: stumbled 0 times, knocked down 0, launched 1, got up 1; thrown into others 5 times.


## B Medium

Present 0.0–11.7 s. Steps 29, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 41 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 1 / 10 |
| Chest | ? | 0.2 / 2.9 | 1 / 23 |
| Head | ? | 0.2 / 4.0 | 7 / 42 |
| UpperArmL | ? | 0.3 / 6.8 | 2 / 38 |
| ForearmL | ? | 1.0 / 19.4 | 5 / 102 |
| HandL | ? | 1.7 / 25.9 | 16 / 176 |
| UpperArmR | ? | 0.1 / 1.7 | 1 / 48 |
| ForearmR | ? | 0.4 / 6.8 | 2 / 45 |
| HandR | ? | 0.6 / 22.8 | 7 / 119 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 5 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 2 on allies), 78 damage. Hits taken: 1, 0 damage. Health at the end 0.

Defence: 1 blows came at it (1 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 0.

Look: within 25° of what it attended to 84% of the time standing (83% of the time it was within the head's reach, 95% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 95% of the time standing; its rim came within 1.1 cm of its own body (negative: inside).

Fight Rhythm (?): 3 postures (GuardChange 2, HighFeint 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.0 cm, max 0.1 cm (posed); physical weapon's closest pass mean 1.3 cm, max 1.3 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.03 s: SwordCut at A Small's Belly: passed 0.0 cm from the aim point; nearest part A Small's Pelvis (4.3 cm); the Belly had moved 22.7 cm since the aim was locked
- 6.17 s: SwordCleave at A Small's Belly: passed 0.1 cm from the aim point; nearest part A Small's Chest (2.3 cm); the Belly had moved 22.9 cm since the aim was locked
- 8.17 s: SwordCut at A Small's Neck: passed 0.0 cm from the aim point; nearest part A Small's Chest (17.0 cm); the Neck had moved 30.2 cm since the aim was locked


## B Large

Present 0.0–11.7 s. Steps 24, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 47 cm from the support point; hips dropped for reach by up to 2 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 1 / 11 |
| Chest | ? | 0.3 / 4.9 | 1 / 9 |
| Head | ? | 0.3 / 6.4 | 4 / 24 |
| UpperArmL | ? | 0.4 / 6.3 | 3 / 55 |
| ForearmL | ? | 2.0 / 19.2 | 7 / 50 |
| HandL | ? | 3.8 / 33.9 | 15 / 74 |
| UpperArmR | ? | 0.3 / 5.9 | 1 / 43 |
| ForearmR | ? | 0.7 / 12.9 | 3 / 64 |
| HandR | ? | 1.3 / 45.1 | 7 / 75 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (0 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 17 damage. Hits taken: 9, 54 damage. Health at the end 0.

Defence: 3 blows came at it (1 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Look: within 25° of what it attended to 82% of the time standing (83% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 3 postures (BoxerSettle 1, Shift 1, BobAndWeave 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.35 s: ClubSmash at A Medium's Head: passed 0.0 cm from the aim point; nearest part A Medium's Chest (29.2 cm); the Head had moved 42.3 cm since the aim was locked

