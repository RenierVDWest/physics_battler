# Playtest 2026-11-01_10-54-53

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 24.0 s of play (1438 physics steps), 847 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-01_10-54-53`.

## A Small

Present 0.0–24.0 s. Steps 27, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 10 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 1.7 | 2 / 79 |
| Chest | Ragdoll | 0.3 / 14.8 | 2 / 78 |
| Head | Ragdoll | 0.4 / 18.5 | 5 / 79 |
| UpperArmL | Ragdoll | 0.5 / 17.9 | 3 / 96 |
| ForearmL | Ragdoll | 1.0 / 37.1 | 8 / 149 |
| HandL | Ragdoll | 2.2 / 34.3 | 17 / 154 |
| UpperArmR | Ragdoll | 0.1 / 2.5 | 1 / 85 |
| ForearmR | Ragdoll | 0.4 / 10.0 | 2 / 119 |
| HandR | Ragdoll | 0.7 / 27.3 | 11 / 133 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 24 damage. Hits taken: 4, 39 damage. Killed.

Defence: 3 blows came at it (3 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 2.

Look: within 25° of what it attended to 78% of the time standing (79% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.83.

Balance: stumbled 3 times, knocked down 1, launched 0, got up 0; thrown into others 1 time.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.60 s: ClubSwing at B Medium's Neck: passed 0.0 cm from the aim point; nearest part B Medium's Head (7.4 cm); the Neck had moved 16.6 cm since the aim was locked


## A Medium

Present 0.0–24.0 s. Steps 51, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 27 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 1.3 | 1 / 34 |
| Chest | Pinned | 0.2 / 11.6 | 1 / 53 |
| Head | Pinned | 0.3 / 12.1 | 5 / 89 |
| UpperArmL | Pinned | 0.3 / 17.2 | 2 / 93 |
| ForearmL | Pinned | 1.2 / 21.3 | 4 / 125 |
| HandL | Pinned | 1.5 / 17.7 | 19 / 178 |
| UpperArmR | Pinned | 0.2 / 9.0 | 1 / 47 |
| ForearmR | Pinned | 0.4 / 13.5 | 2 / 55 |
| HandR | Pinned | 0.6 / 22.6 | 7 / 168 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 4 (1 on the part aimed at, 1 blocked, 0 on someone in the way, 1 on allies), 32 damage. Hits taken: 11, 74 damage. Health at the end 26.

Defence: 11 blows came at it (4 seen clearly, 7 at the edge of its view, 0 never seen); it raised the shield against 5.

Look: within 25° of what it attended to 83% of the time standing (84% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 93% of the time standing; its rim came within 4.5 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 4 postures (GuardChange 1, HighFeint 1, ShieldPress 1, Peek 1), 1 of them lead-ins to blows; energy at the end 0.74.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 6.70 s: SwordThrust at B Large's Chest: passed 0.1 cm from the aim point; nearest part B Large's Chest (7.3 cm); the Chest had moved 37.3 cm since the aim was locked


## A Large

Present 0.0–24.0 s. Steps 37, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 35 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.2 | 1 / 11 |
| Chest | Ragdoll | 0.2 / 4.6 | 1 / 13 |
| Head | Ragdoll | 0.3 / 6.9 | 5 / 78 |
| UpperArmL | Ragdoll | 0.4 / 10.9 | 3 / 56 |
| ForearmL | Ragdoll | 1.7 / 24.3 | 6 / 82 |
| HandL | Ragdoll | 3.4 / 35.8 | 14 / 102 |
| UpperArmR | Ragdoll | 0.2 / 2.2 | 1 / 43 |
| ForearmR | Ragdoll | 0.5 / 15.0 | 2 / 64 |
| HandR | Ragdoll | 1.0 / 45.1 | 6 / 153 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 6 (2 on the part aimed at, 0 blocked, 1 on someone in the way, 0 on allies), 101 damage. Hits taken: 3, 53 damage. Health at the end 93.

Look: within 25° of what it attended to 85% of the time standing (85% of the time it was within the head's reach, 97% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 4 postures (BoxerSettle 1, ClubFeint 1, BobAndWeave 1, BoxerStalk 1), 0 of them lead-ins to blows; energy at the end 0.33.

Balance: stumbled 0 times, knocked down 1, launched 0, got up 0; thrown into others 1 time. Still down at the end.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.93 s: ClubSwing at B Small's Belly: passed 0.1 cm from the aim point; nearest part B Small's HandL (0.0 cm); the Belly had moved 35.7 cm since the aim was locked
- 6.28 s: ClubSmash at B Large's Head: passed 0.0 cm from the aim point; nearest part B Large's Head (14.0 cm); the Head had moved 25.4 cm since the aim was locked


## B Small

Present 0.0–24.0 s. Steps 61, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 21 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 1 / 18 |
| Chest | Pinned | 0.1 / 3.6 | 1 / 26 |
| Head | Pinned | 0.2 / 6.1 | 3 / 21 |
| UpperArmL | Pinned | 0.2 / 10.7 | 2 / 69 |
| ForearmL | Pinned | 0.6 / 22.6 | 4 / 91 |
| HandL | Pinned | 1.2 / 24.6 | 10 / 109 |
| UpperArmR | Kinematic | 0.1 / 1.7 | 1 / 56 |
| ForearmR | Kinematic | 0.3 / 9.3 | 2 / 89 |
| HandR | Kinematic | 0.5 / 21.2 | 9 / 102 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 3 (1 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 39 damage. Hits taken: 1, 21 damage. Health at the end 39.

Defence: 1 blows came at it (0 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Look: within 25° of what it attended to 86% of the time standing (89% of the time it was within the head's reach, 96% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 7 postures (StickSettle 2, AbanicoFlurry 3, RedondaTwirl 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.59.

Balance: stumbled 0 times, knocked down 0, launched 1, got up 1; thrown into others 0 times.


## B Medium

Present 0.0–24.0 s. Steps 51, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 40 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 5.1 | 1 / 86 |
| Chest | Pinned | 0.3 / 31.1 | 1 / 78 |
| Head | Pinned | 0.4 / 24.3 | 11 / 179 |
| UpperArmL | Pinned | 0.5 / 38.1 | 3 / 174 |
| ForearmL | Pinned | 1.2 / 33.6 | 5 / 159 |
| HandL | Pinned | 1.5 / 48.9 | 16 / 173 |
| UpperArmR | Pinned | 0.1 / 1.7 | 1 / 48 |
| ForearmR | Pinned | 0.3 / 6.8 | 1 / 45 |
| HandR | Pinned | 0.4 / 22.8 | 5 / 119 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 11 (2 on the part aimed at, 5 blocked, 2 on someone in the way, 0 on allies), 80 damage. Hits taken: 3, 34 damage. Health at the end 66.

Defence: 2 blows came at it (1 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 1.

Look: within 25° of what it attended to 85% of the time standing (87% of the time it was within the head's reach, 92% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 97% of the time standing; its rim came within 1.0 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 8 postures (GuardChange 2, ShieldPress 2, HighFeint 1, Peek 1, ShieldAdvance 2), 2 of them lead-ins to blows; energy at the end 0.79.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 5.15 s: SwordCleave at A Small's ElbowR: passed 0.0 cm from the aim point; nearest part A Small's Spine (1.0 cm); the ElbowR had moved 33.6 cm since the aim was locked
- 7.23 s: SwordCut at A Small's KneeR: passed 0.0 cm from the aim point; nearest part A Small's ShinR (7.7 cm); the KneeR had moved 9.6 cm since the aim was locked
- 8.95 s: SwordCut at A Small's KneeL: passed 0.0 cm from the aim point; nearest part A Small's ShinL (9.7 cm); the KneeL had moved 2.5 cm since the aim was locked


## B Large

Present 0.0–24.0 s. Steps 36, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 29 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 1 / 18 |
| Chest | Pinned | 0.2 / 7.6 | 0 / 11 |
| Head | Pinned | 0.3 / 6.1 | 4 / 24 |
| UpperArmL | Pinned | 0.3 / 6.5 | 2 / 36 |
| ForearmL | Pinned | 1.3 / 18.7 | 4 / 51 |
| HandL | Pinned | 2.4 / 33.9 | 11 / 80 |
| UpperArmR | Pinned | 0.3 / 8.3 | 1 / 43 |
| ForearmR | Pinned | 0.8 / 12.9 | 3 / 64 |
| HandR | Pinned | 1.4 / 45.1 | 9 / 76 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (0 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 8, 165 damage. Health at the end 55.

Defence: 8 blows came at it (3 seen clearly, 5 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 5.

Look: within 25° of what it attended to 86% of the time standing (86% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 5 postures (BoxerSettle 1, BoxerStalk 1, ClubFeint 1, JabFeint 1, BobAndWeave 1), 2 of them lead-ins to blows; energy at the end 0.82.

Balance: stumbled 2 times, knocked down 0, launched 1, got up 1; thrown into others 0 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 12.92 s: ClubSmash at A Medium's Head: passed 0.0 cm from the aim point; nearest part A Medium's HandL (23.5 cm); the Head had moved 34.6 cm since the aim was locked

