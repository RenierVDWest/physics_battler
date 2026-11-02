# Playtest 2026-11-02_07-02-55

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 59.9 s of play (3597 physics steps), 4163 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-02_07-02-55`.

## A Small

Present 0.0–59.9 s. Steps 65, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 18 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.1 | 4 / 36 |
| Chest | Ragdoll | 0.8 / 7.0 | 5 / 63 |
| Head | Ragdoll | 1.0 / 10.6 | 17 / 144 |
| UpperArmL | Ragdoll | 1.2 / 11.5 | 10 / 158 |
| ForearmL | Ragdoll | 2.2 / 17.7 | 16 / 173 |
| HandL | Ragdoll | 3.0 / 36.0 | 26 / 179 |
| UpperArmR | Ragdoll | 0.8 / 20.4 | 28 / 170 |
| ForearmR | Ragdoll | 1.9 / 39.8 | 12 / 159 |
| HandR | Ragdoll | 2.1 / 31.5 | 19 / 176 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 146 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 7 (1 on the part aimed at, 4 blocked, 0 on someone in the way, 0 on allies), 42 damage. Hits taken: 4, 70 damage. Killed.

Defence: 7 blows came at it (4 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 5.

Pose against the joint limits (standing): a joint more than 10° past its limit 47% of the time; the worst 94° (ShinR).
Ragdoll (on the ground 40.0 s): a joint more than 10° past its limit 61% of the time; the worst 46° (ForearmR). Fastest part with a standing leg within 0.5 m: 0.0 m/s median, 3.1 m/s at the 95th percentile (388 steps); with none: 0.0 m/s median (2013 steps).

Look: within 25° of what it attended to 78% of the time standing (81% of the time it was within the head's reach, 96% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 7 postures (StickSettle 1, AbanicoFlurry 3, TriangleStep 1, RedondaTwirl 1, BaitRetreat 1), 1 of them lead-ins to blows; energy at the end 0.24.

Balance: stumbled 3 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.8 cm, max 0.8 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.28 s: ClubFan at B Medium's ElbowL: passed 0.8 cm from the aim point; nearest part B Medium's ForearmR (7.8 cm); the ElbowL had moved 56.3 cm since the aim was locked


## A Medium

Present 0.0–59.9 s. Steps 64, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 37 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.2 | 1 / 20 |
| Chest | Ragdoll | 0.4 / 5.9 | 2 / 47 |
| Head | Ragdoll | 0.6 / 14.4 | 10 / 68 |
| UpperArmL | Ragdoll | 0.7 / 16.3 | 6 / 168 |
| ForearmL | Ragdoll | 2.3 / 45.8 | 8 / 173 |
| HandL | Ragdoll | 2.8 / 47.8 | 5 / 91 |
| UpperArmR | Ragdoll | 0.4 / 16.9 | 5 / 91 |
| ForearmR | Ragdoll | 0.7 / 33.5 | 4 / 135 |
| HandR | Ragdoll | 1.0 / 46.1 | 9 / 175 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 1 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 46 |
| ShinR | Ragdoll | 0.0 / 2.3 | 0 / 46 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 2 |

Hits dealt: 4 (0 on the part aimed at, 2 blocked, 0 on someone in the way, 0 on allies), 18 damage. Hits taken: 24, 108 damage. Killed.

Defence: 20 blows came at it (12 seen clearly, 8 at the edge of its view, 0 never seen); it raised the shield against 13.

Pose against the joint limits (standing): a joint more than 10° past its limit 51% of the time; the worst 106° (FootR).
Ragdoll (on the ground 27.5 s): a joint more than 10° past its limit 99% of the time; the worst 174° (ForearmL). Fastest part with a standing leg within 0.5 m: 5.8 m/s median, 21.7 m/s at the 95th percentile (270 steps); with none: 0.0 m/s median (1379 steps).

Look: within 25° of what it attended to 86% of the time standing (88% of the time it was within the head's reach, 97% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 88% of the time standing; its rim came within -1.8 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 4 postures (GuardChange 1, HighFeint 1, ShieldAdvance 1, Peek 1), 2 of them lead-ins to blows; energy at the end 0.45.

Balance: stumbled 4 times, knocked down 1, launched 0, got up 1; thrown into others 8 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.2 cm, max 0.2 cm (posed); physical weapon's closest pass mean 1.3 cm, max 1.3 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 21.23 s: SwordBackhand at B Small's Belly: passed 0.2 cm from the aim point; nearest part B Small's ThighL (4.8 cm); the Belly had moved 17.8 cm since the aim was locked


## A Large

Present 0.0–59.9 s. Steps 123, idle shuffles 0. Feet slid at most 3.8 cm while planted; the weight was at most 57 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 21.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 21.0 | 1 / 23 |
| Chest | Pinned | 0.6 / 11.2 | 2 / 11 |
| Head | Pinned | 0.7 / 9.3 | 14 / 59 |
| UpperArmL | Pinned | 1.1 / 14.3 | 26 / 89 |
| ForearmL | Pinned | 3.9 / 40.8 | 11 / 63 |
| HandL | Pinned | 5.4 / 45.4 | 23 / 75 |
| UpperArmR | Kinematic | 0.4 / 9.4 | 11 / 92 |
| ForearmR | Kinematic | 0.9 / 17.5 | 4 / 68 |
| HandR | Kinematic | 1.8 / 51.7 | 11 / 99 |
| ThighL | Kinematic | 0.0 / 20.5 | 0 / 12 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 21.5 | 0 / 150 |
| ShinR | Kinematic | 0.0 / 8.2 | 0 / 0 |
| FootR | Kinematic | 0.0 / 5.2 | 0 / 1 |

Hits dealt: 18 (4 on the part aimed at, 3 blocked, 1 on someone in the way, 0 on allies), 227 damage. Hits taken: 7, 74 damage. Health at the end 146.

Defence: 3 blows came at it (3 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 34% of the time; the worst 161° (ShinR).

Look: within 25° of what it attended to 80% of the time standing (81% of the time it was within the head's reach, 93% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 12 postures (BoxerSettle 2, BobAndWeave 1, ClubFeint 4, JabFeint 3, BoxerStalk 2), 4 of them lead-ins to blows; energy at the end 0.44.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 7. The posed striking part passed within 3 cm of the aim point in 7; aim error mean 0.4 cm, max 1.0 cm (posed); physical weapon's closest pass mean 2.2 cm, max 6.0 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.55 s: ClubRising at B Small's KneeL: passed 0.3 cm from the aim point; nearest part B Small's ThighR (5.7 cm); the KneeL had moved 27.6 cm since the aim was locked
- 10.52 s: ClubSwing at B Small's KneeL: passed 0.6 cm from the aim point; nearest part B Small's ShinL (10.2 cm); the KneeL had moved 12.1 cm since the aim was locked
- 24.40 s: ClubSmash at B Large's Head: passed 1.0 cm from the aim point; nearest part B Large's Head (15.4 cm); the Head had moved 27.8 cm since the aim was locked
- 36.85 s: ClubSwing at B Medium's Head: passed 0.3 cm from the aim point; nearest part B Medium's Head (1.7 cm); the Head had moved 13.6 cm since the aim was locked
- 42.78 s: ClubSwing at B Small's Head: passed 0.4 cm from the aim point; nearest part B Small's Chest (49.6 cm); the Head had moved 57.7 cm since the aim was locked
- 45.03 s: ClubBackhand at B Small's Neck: passed 0.2 cm from the aim point; nearest part B Small's Chest (1.7 cm); the Neck had moved 6.3 cm since the aim was locked
- 58.53 s: ClubSwing at B Medium's Neck: passed 0.3 cm from the aim point; nearest part B Medium's Head (13.8 cm); the Neck had moved 19.7 cm since the aim was locked


## B Small

Present 0.0–59.9 s. Steps 120, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 28 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.3 | 2 / 46 |
| Chest | ? | 0.4 / 8.9 | 2 / 40 |
| Head | ? | 0.5 / 9.1 | 11 / 113 |
| UpperArmL | ? | 0.7 / 15.5 | 6 / 178 |
| ForearmL | ? | 1.7 / 21.2 | 11 / 179 |
| HandL | ? | 2.5 / 36.0 | 19 / 173 |
| UpperArmR | ? | 0.3 / 13.1 | 7 / 90 |
| ForearmR | ? | 0.9 / 27.0 | 7 / 152 |
| HandR | ? | 1.3 / 35.4 | 17 / 171 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 7 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 10 (0 on the part aimed at, 7 blocked, 0 on someone in the way, 0 on allies), 33 damage. Hits taken: 10, 76 damage. Health at the end 0.

Defence: 7 blows came at it (5 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 5.

Pose against the joint limits (standing): a joint more than 10° past its limit 57% of the time; the worst 66° (FootR).
Ragdoll (on the ground 20.8 s): a joint more than 10° past its limit 97% of the time; the worst 173° (ForearmR). Fastest part with a standing leg within 0.5 m: 0.3 m/s median, 9.2 m/s at the 95th percentile (381 steps); with none: 0.0 m/s median (868 steps).

Look: within 25° of what it attended to 78% of the time standing (81% of the time it was within the head's reach, 95% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 7 postures (StickSettle 2, AbanicoFlurry 1, BaitRetreat 1, RedondaTwirl 2, TriangleStep 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 0, launched 3, got up 3; thrown into others 1 time.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.5 cm, max 0.5 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 36.95 s: ClubSwing at A Medium's ElbowL: passed 0.5 cm from the aim point; nearest part A Medium's HandL (35.0 cm); the ElbowL had moved 55.4 cm since the aim was locked


## B Medium

Present 0.0–59.9 s. Steps 103, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 43 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Pinned | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 1.5 | 2 / 59 |
| Chest | Pinned | 0.5 / 21.9 | 2 / 48 |
| Head | Pinned | 0.6 / 15.3 | 13 / 79 |
| UpperArmL | Pinned | 0.8 / 19.3 | 6 / 179 |
| ForearmL | Pinned | 2.2 / 50.6 | 8 / 177 |
| HandL | Pinned | 2.7 / 38.3 | 5 / 149 |
| UpperArmR | Pinned | 0.3 / 4.9 | 5 / 179 |
| ForearmR | Pinned | 0.6 / 19.7 | 4 / 135 |
| HandR | Pinned | 1.0 / 25.6 | 9 / 129 |
| ThighL | Pinned | 0.0 / 0.0 | 0 / 0 |
| ShinL | Pinned | 0.0 / 0.0 | 0 / 0 |
| FootL | Pinned | 0.0 / 0.0 | 0 / 0 |
| ThighR | Pinned | 0.0 / 0.0 | 0 / 150 |
| ShinR | Pinned | 0.0 / 0.0 | 0 / 0 |
| FootR | Pinned | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 9 (3 on the part aimed at, 2 blocked, 0 on someone in the way, 1 on allies), 107 damage. Hits taken: 16, 92 damage. Health at the end 8.

Defence: 15 blows came at it (11 seen clearly, 4 at the edge of its view, 0 never seen); it raised the shield against 6.

Pose against the joint limits (standing): a joint more than 10° past its limit 43% of the time; the worst 104° (ShinR).
Ragdoll (on the ground 5.3 s): a joint more than 10° past its limit 95% of the time; the worst 169° (ForearmR). Fastest part with a standing leg within 0.5 m: 7.0 m/s median, 16.9 m/s at the 95th percentile (35 steps); with none: 3.3 m/s median (284 steps).

Look: within 25° of what it attended to 85% of the time standing (86% of the time it was within the head's reach, 97% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 91% of the time standing; its rim came within -2.7 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 8 postures (GuardChange 1, ShieldPress 2, Peek 3, HighFeint 2), 1 of them lead-ins to blows; energy at the end 0.53.

Balance: stumbled 5 times, knocked down 2, launched 0, got up 1; thrown into others 1 time. Still down at the end.

Blows: 8. The posed striking part passed within 3 cm of the aim point in 8; aim error mean 0.5 cm, max 1.2 cm (posed); physical weapon's closest pass mean 0.6 cm, max 1.1 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 5.03 s: SwordBackhand at A Small's KneeL: passed 0.3 cm from the aim point; nearest part A Small's ThighL (8.0 cm); the KneeL had moved 16.5 cm since the aim was locked
- 11.27 s: SwordBackhand at A Small's ElbowR: passed 1.2 cm from the aim point; nearest part A Small's UpperArmR (51.5 cm); the ElbowR had moved 41.7 cm since the aim was locked
- 12.87 s: SwordBackhand at A Small's ElbowR: passed 0.4 cm from the aim point; nearest part A Small's UpperArmR (37.9 cm); the ElbowR had moved 39.1 cm since the aim was locked
- 14.55 s: SwordBackhand at A Small's Chest: passed 0.7 cm from the aim point; nearest part A Small's Pelvis (50.1 cm); the Chest had moved 64.6 cm since the aim was locked
- 16.52 s: SwordCleave at A Small's Chest: passed 0.4 cm from the aim point; nearest part A Small's Chest (6.3 cm); the Chest had moved 30.7 cm since the aim was locked
- 18.18 s: SwordBackhand at A Small's Neck: passed 0.3 cm from the aim point; nearest part A Small's Chest (9.8 cm); the Neck had moved 9.6 cm since the aim was locked
- 19.92 s: SwordBackhand at A Small's Neck: passed 0.4 cm from the aim point; nearest part A Small's Chest (4.5 cm); the Neck had moved 5.4 cm since the aim was locked
- 36.43 s: SwordCut at A Medium's Head: passed 0.4 cm from the aim point; nearest part A Medium's Chest (15.6 cm); the Head had moved 24.6 cm since the aim was locked


## B Large

Present 0.0–59.9 s. Steps 76, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 50 cm from the support point; hips dropped for reach by up to 4 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 4.4 | 0 / 0 |
| Spine | ? | 0.0 / 4.5 | 1 / 19 |
| Chest | ? | 0.5 / 8.0 | 1 / 11 |
| Head | ? | 0.7 / 9.2 | 10 / 64 |
| UpperArmL | ? | 0.9 / 12.5 | 26 / 71 |
| ForearmL | ? | 2.8 / 23.7 | 9 / 63 |
| HandL | ? | 4.1 / 45.4 | 20 / 86 |
| UpperArmR | ? | 0.5 / 8.0 | 16 / 66 |
| ForearmR | ? | 1.1 / 17.5 | 4 / 68 |
| HandR | ? | 2.0 / 51.7 | 15 / 59 |
| ThighL | ? | 0.0 / 3.7 | 0 / 148 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 5.2 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 6 (1 on the part aimed at, 3 blocked, 1 on someone in the way, 0 on allies), 28 damage. Hits taken: 13, 157 damage. Health at the end 0.

Defence: 10 blows came at it (0 seen clearly, 10 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 7.

Pose against the joint limits (standing): a joint more than 10° past its limit 20% of the time; the worst 112° (ShinL).
Ragdoll (on the ground 20.0 s): a joint more than 10° past its limit 94% of the time; the worst 86° (ForearmL). Fastest part with a standing leg within 0.5 m: - median, - at the 95th percentile (0 steps); with none: 0.0 m/s median (1202 steps).

Look: within 25° of what it attended to 83% of the time standing (83% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 4 postures (BoxerSettle 1, Shift 1, ClubFeint 1, BobAndWeave 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 6 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.3 cm, max 0.3 cm (posed); physical weapon's closest pass mean 0.5 cm, max 0.8 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.47 s: ClubSwing at A Medium's Chest: passed 0.2 cm from the aim point; nearest part A Medium's Chest (34.4 cm); the Chest had moved 67.9 cm since the aim was locked
- 4.17 s: ClubBackhand at A Medium's Neck: passed 0.3 cm from the aim point; nearest part A Medium's HandL (21.2 cm); the Neck had moved 76.7 cm since the aim was locked

## Shield contacts

219 times a shield overlapped another unit's body: 95% under 5.8 cm deep (79% under 2 cm; target 95%), the deepest 11.6 cm, the longest 0.47 s (target under 0.2 s). Each one is in events.log ("shield").

