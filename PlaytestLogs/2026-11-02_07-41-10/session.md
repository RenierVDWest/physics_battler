# Playtest 2026-11-02_07-41-10

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 23.5 s of play (1413 physics steps), 1402 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-02_07-41-10`.

## A Small

Present 0.0–23.5 s. Steps 23, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 15 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.1 | 2 / 9 |
| Chest | Ragdoll | 0.3 / 1.8 | 2 / 10 |
| Head | Ragdoll | 0.3 / 1.6 | 7 / 33 |
| UpperArmL | Ragdoll | 0.5 / 3.4 | 4 / 44 |
| ForearmL | Ragdoll | 1.5 / 15.4 | 8 / 99 |
| HandL | Ragdoll | 2.6 / 36.0 | 22 / 82 |
| UpperArmR | Ragdoll | 0.3 / 1.5 | 3 / 92 |
| ForearmR | Ragdoll | 0.8 / 14.5 | 7 / 119 |
| HandR | Ragdoll | 2.0 / 31.5 | 17 / 102 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (0 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 3, 41 damage. Killed.

Defence: 3 blows came at it (2 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0.

Pose against the joint limits (standing): a joint more than 10° past its limit 62% of the time; the worst 42° (FootL).
Ragdoll (on the ground 16.5 s): a joint more than 10° past its limit 4% of the time; the worst 117° (FootR). Fastest part with a standing leg within 0.5 m: 0.0 m/s median, 0.8 m/s at the 95th percentile (529 steps); with none: 0.0 m/s median (461 steps).

Look: within 25° of what it attended to 75% of the time standing (75% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 2 postures (StickSettle 1, AbanicoFlurry 1), 0 of them lead-ins to blows; energy at the end 0.81.

Balance: stumbled 3 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–23.5 s. Steps 49, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 27 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 2 / 47 |
| Chest | Pinned | 0.4 / 4.5 | 2 / 41 |
| Head | Pinned | 0.8 / 12.4 | 13 / 71 |
| UpperArmL | Pinned | 0.9 / 17.9 | 8 / 148 |
| ForearmL | Pinned | 3.5 / 25.0 | 10 / 121 |
| HandL | Pinned | 4.9 / 35.6 | 11 / 93 |
| UpperArmR | Pinned | 0.4 / 5.0 | 2 / 91 |
| ForearmR | Pinned | 1.2 / 28.0 | 5 / 132 |
| HandR | Pinned | 1.6 / 27.2 | 13 / 112 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 5 (0 on the part aimed at, 0 blocked, 2 on someone in the way, 0 on allies), 46 damage. Hits taken: 7, 68 damage. Health at the end 32.

Defence: 5 blows came at it (2 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 34% of the time; the worst 48° (FootR).
Ragdoll (on the ground 1.2 s): a joint more than 10° past its limit 93% of the time; the worst 78° (ForearmL). Fastest part with a standing leg within 0.5 m: 4.0 m/s median, 4.6 m/s at the 95th percentile (12 steps); with none: 0.9 m/s median (62 steps).

Look: within 25° of what it attended to 76% of the time standing (77% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 91% of the time standing; its rim came within 1.0 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 4 postures (GuardChange 1, Peek 2, HighFeint 1), 1 of them lead-ins to blows; energy at the end 0.24.

Balance: stumbled 2 times, knocked down 1, launched 0, got up 1; thrown into others 0 times.


## A Large

Present 0.0–23.5 s. Steps 44, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 29 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 2 / 74 |
| Chest | Pinned | 0.9 / 29.0 | 2 / 27 |
| Head | Pinned | 0.9 / 27.3 | 15 / 73 |
| UpperArmL | Pinned | 1.2 / 23.4 | 6 / 50 |
| ForearmL | Pinned | 4.5 / 28.4 | 11 / 65 |
| HandL | Pinned | 5.8 / 45.4 | 27 / 111 |
| UpperArmR | Pinned | 0.6 / 39.0 | 2 / 59 |
| ForearmR | Pinned | 1.1 / 28.0 | 4 / 68 |
| HandR | Pinned | 2.3 / 73.5 | 9 / 108 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 150 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 6 (2 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 89 damage. Hits taken: 2, 27 damage. Health at the end 193.

Defence: 5 blows came at it (2 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 31% of the time; the worst 123° (ShinR).

Look: within 25° of what it attended to 77% of the time standing (77% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 5 postures (BoxerSettle 1, ClubFeint 2, JabFeint 1, BobAndWeave 1), 1 of them lead-ins to blows; energy at the end 0.16.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.3 cm, max 0.5 cm (posed); physical weapon's closest pass mean 0.7 cm, max 1.2 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.55 s: ClubRising at B Small's KneeR: passed 0.2 cm from the aim point; nearest part B Small's HandR (7.2 cm); the KneeR had moved 18.6 cm since the aim was locked
- 4.50 s: ClubSwing at B Small's Head: passed 0.3 cm from the aim point; nearest part B Small's HandR (41.8 cm); the Head had moved 69.9 cm since the aim was locked
- 5.35 s: ClubSmash at B Small's Head: passed 0.5 cm from the aim point; nearest part B Small's ForearmR (14.6 cm); the Head had moved 66.4 cm since the aim was locked
- 11.15 s: ClubBackhand at B Small's Chest: passed 0.2 cm from the aim point; nearest part B Small's Chest (3.2 cm); the Chest had moved 9.1 cm since the aim was locked


## B Small

Present 0.0–23.5 s. Steps 47, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 9 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.0 | 1 / 8 |
| Chest | Pinned | 0.2 / 1.7 | 1 / 6 |
| Head | Pinned | 0.3 / 1.5 | 5 / 18 |
| UpperArmL | Pinned | 0.3 / 2.0 | 2 / 44 |
| ForearmL | Pinned | 0.8 / 15.4 | 4 / 99 |
| HandL | Pinned | 1.3 / 36.0 | 14 / 82 |
| UpperArmR | Pinned | 0.3 / 1.8 | 2 / 80 |
| ForearmR | Pinned | 0.8 / 15.7 | 5 / 100 |
| HandR | Pinned | 1.6 / 24.5 | 18 / 106 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 145 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 3, 47 damage. Health at the end 13.

Defence: 4 blows came at it (1 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 68% of the time; the worst 96° (ShinR).
Ragdoll (on the ground 5.2 s): a joint more than 10° past its limit 94% of the time; the worst 174° (ForearmL). Fastest part with a standing leg within 0.5 m: 9.2 m/s median, 14.4 m/s at the 95th percentile (41 steps); with none: 3.1 m/s median (272 steps).

Look: within 25° of what it attended to 87% of the time standing (88% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 5 postures (StickSettle 2, AbanicoFlurry 2, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.99.

Balance: stumbled 0 times, knocked down 0, launched 2, got up 2; thrown into others 0 times.


## B Medium

Present 0.0–23.5 s. Steps 59, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 30 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.6 | 3 / 68 |
| Chest | Pinned | 0.9 / 19.9 | 4 / 157 |
| Head | Pinned | 1.2 / 28.7 | 18 / 178 |
| UpperArmL | Pinned | 1.4 / 32.2 | 8 / 102 |
| ForearmL | Pinned | 3.8 / 58.4 | 9 / 128 |
| HandL | Pinned | 4.8 / 59.2 | 14 / 110 |
| UpperArmR | Kinematic | 1.2 / 63.8 | 3 / 97 |
| ForearmR | Kinematic | 1.7 / 83.5 | 5 / 159 |
| HandR | Kinematic | 2.2 / 88.6 | 12 / 156 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 10 (2 on the part aimed at, 3 blocked, 1 on someone in the way, 2 on allies), 97 damage. Hits taken: 4, 19 damage. Health at the end 81.

Defence: 1 blows came at it (0 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 33% of the time; the worst 32° (FootL).

Look: within 25° of what it attended to 81% of the time standing (81% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 92% of the time standing; its rim came within 1.0 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 7 postures (GuardChange 1, ShieldPress 3, Peek 1, HighFeint 2), 3 of them lead-ins to blows; energy at the end 0.55.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.2 cm, max 0.3 cm (posed); physical weapon's closest pass mean 0.7 cm, max 1.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.93 s: SwordCut at A Small's Neck: passed 0.3 cm from the aim point; nearest part A Small's Chest (0.0 cm); the Neck had moved 14.3 cm since the aim was locked
- 5.75 s: SwordBackhand at A Small's Chest: passed 0.2 cm from the aim point; nearest part A Small's Spine (2.0 cm); the Chest had moved 12.9 cm since the aim was locked
- 6.47 s: SwordCleave at A Small's Chest: passed 0.2 cm from the aim point; nearest part A Small's UpperArmR (6.1 cm); the Chest had moved 2.0 cm since the aim was locked
- 23.40 s: SwordThrust at A Medium's Belly: passed 0.1 cm from the aim point; nearest part A Medium's HandR (10.3 cm); the Belly had moved 4.8 cm since the aim was locked


## B Large

Present 0.0–23.5 s. Steps 32, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 35 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 1 / 17 |
| Chest | Pinned | 0.5 / 6.3 | 1 / 9 |
| Head | Pinned | 0.6 / 7.4 | 13 / 50 |
| UpperArmL | Pinned | 0.8 / 11.0 | 4 / 50 |
| ForearmL | Pinned | 3.0 / 23.7 | 8 / 66 |
| HandL | Pinned | 4.2 / 45.4 | 21 / 96 |
| UpperArmR | Pinned | 0.5 / 6.9 | 2 / 59 |
| ForearmR | Pinned | 1.1 / 34.9 | 3 / 68 |
| HandR | Pinned | 1.8 / 51.7 | 8 / 140 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 5 (2 on the part aimed at, 2 blocked, 0 on someone in the way, 0 on allies), 55 damage. Hits taken: 8, 84 damage. Health at the end 136.

Defence: 11 blows came at it (6 seen clearly, 5 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 8.

Pose against the joint limits (standing): a joint more than 10° past its limit 18% of the time; the worst 29° (Chest).

Look: within 25° of what it attended to 80% of the time standing (80% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 3 postures (BoxerSettle 1, BoxerStalk 1, BobAndWeave 1), 1 of them lead-ins to blows; energy at the end 0.53.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.4 cm, max 0.4 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 10.68 s: ClubSmash at A Medium's Head: passed 0.4 cm from the aim point; nearest part A Medium's Head (2.4 cm); the Head had moved 11.9 cm since the aim was locked

## Shield contacts

80 times a shield overlapped another unit's body: 95% under 7.4 cm deep (79% under 2 cm; target 95%), the deepest 10.6 cm, the longest 0.43 s (target under 0.2 s). Each one is in events.log ("shield").

