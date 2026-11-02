# Playtest 2026-11-02_17-56-23

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 49.6 s of play (2976 physics steps), 3807 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-02_17-56-23`.

## A Small

Present 0.0–49.6 s. Steps 108, idle shuffles 7. Feet slid at most 0.0 cm while planted; the weight was at most 35 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 11.3 | 4 / 87 |
| Chest | Pinned | 0.8 / 21.3 | 7 / 162 |
| Head | Pinned | 1.4 / 30.3 | 13 / 82 |
| UpperArmL | Pinned | 1.6 / 51.3 | 10 / 163 |
| ForearmL | Pinned | 3.0 / 45.0 | 17 / 177 |
| HandL | Pinned | 4.0 / 71.1 | 29 / 179 |
| UpperArmR | Muscled | 0.9 / 7.5 | 5 / 49 |
| ForearmR | Muscled | 1.8 / 19.5 | 8 / 78 |
| HandR | Muscled | 3.6 / 32.3 | 11 / 99 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 26 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 149 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 9 (3 on the part aimed at, 0 blocked, 2 on someone in the way, 1 on allies), 97 damage. Hits taken: 4, 28 damage. Health at the end 32.

Defence: 3 blows came at it (2 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 97% of the time; the worst 83° (ShinR).
Ragdoll (on the ground 2.3 s): a joint more than 10° past its limit 68% of the time; the worst 55° (HandL). Fastest part with a standing leg within 0.5 m: - median, - at the 95th percentile (0 steps); with none: 2.8 m/s median (135 steps).

Look: within 25° of what it attended to 81% of the time standing (84% of the time it was within the head's reach, 95% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 16 postures (StickSettle 3, AbanicoFlurry 4, BaitRetreat 1, RedondaTwirl 3, TriangleStep 4, StickStalk 1), 0 of them lead-ins to blows; energy at the end 1.00.

Balance: stumbled 1 time, knocked down 1, launched 1, got up 2; thrown into others 0 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.2 cm, max 0.2 cm (posed); physical weapon's closest pass mean 10.3 cm, max 10.3 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 23.72 s: ClubSwing at B Medium's Neck: passed 0.2 cm from the aim point; nearest part A Large's ForearmL (50.1 cm); the Neck had moved 163.1 cm since the aim was locked


## A Medium

Present 0.0–49.6 s. Steps 77, idle shuffles 6. Feet slid at most 0.0 cm while planted; the weight was at most 29 cm from the support point; hips dropped for reach by up to 6 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 2 / 15 |
| Chest | Pinned | 0.6 / 4.5 | 4 / 44 |
| Head | Pinned | 1.0 / 11.0 | 10 / 74 |
| UpperArmL | Pinned | 1.0 / 7.5 | 4 / 53 |
| ForearmL | Pinned | 1.5 / 22.4 | 5 / 100 |
| HandL | Pinned | 2.1 / 47.7 | 4 / 74 |
| UpperArmR | Muscled | 1.1 / 20.9 | 4 / 56 |
| ForearmR | Muscled | 2.4 / 38.3 | 6 / 84 |
| HandR | Muscled | 3.7 / 58.9 | 8 / 112 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 150 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 9 (0 on the part aimed at, 3 blocked, 1 on someone in the way, 0 on allies), 44 damage. Hits taken: 8, 49 damage. Health at the end 51.

Defence: 6 blows came at it (3 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 100% of the time; the worst 86° (ShinR).
Ragdoll (on the ground 2.4 s): a joint more than 10° past its limit 80% of the time; the worst 82° (ForearmR). Fastest part with a standing leg within 0.5 m: 4.9 m/s median, 7.5 m/s at the 95th percentile (45 steps); with none: 2.0 m/s median (98 steps).

Look: within 25° of what it attended to 83% of the time standing (83% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 90% of the time standing; its rim came within 1.0 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 14 postures (GuardChange 3, HighFeint 2, Peek 2, ShieldPress 4, ShieldAdvance 3), 3 of them lead-ins to blows; energy at the end 1.00.

Balance: stumbled 2 times, knocked down 2, launched 0, got up 1; thrown into others 3 times.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.0 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 16.88 s: SwordBackhand at B Medium's Chest: passed 0.1 cm from the aim point; nearest part B Medium's HandL (2.9 cm); the Chest had moved 50.7 cm since the aim was locked
- 32.53 s: SwordCut at B Large's ElbowL: passed 0.0 cm from the aim point; nearest part B Large's HandR (31.1 cm); the ElbowL had moved 57.2 cm since the aim was locked


## A Large

Present 0.0–49.6 s. Steps 61, idle shuffles 5. Feet slid at most 0.0 cm while planted; the weight was at most 64 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 2 / 19 |
| Chest | ? | 1.0 / 7.5 | 4 / 22 |
| Head | ? | 1.6 / 7.7 | 11 / 50 |
| UpperArmL | ? | 1.6 / 11.5 | 5 / 52 |
| ForearmL | ? | 3.0 / 29.1 | 8 / 82 |
| HandL | ? | 4.1 / 49.3 | 18 / 123 |
| UpperArmR | ? | 1.8 / 18.1 | 4 / 66 |
| ForearmR | ? | 3.8 / 29.4 | 6 / 67 |
| HandR | ? | 5.2 / 52.0 | 9 / 106 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 150 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 6 (3 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 92 damage. Hits taken: 9, 92 damage. Health at the end 0.

Defence: 10 blows came at it (5 seen clearly, 5 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 9.

Pose against the joint limits (standing): a joint more than 10° past its limit 89% of the time; the worst 99° (ShinR).

Look: within 25° of what it attended to 90% of the time standing (89% of the time it was within the head's reach, 97% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 11 postures (BoxerSettle 2, ClubFeint 3, BobAndWeave 3, JabFeint 1, BoxerStalk 2), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 6. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 3.5 cm, max 19.6 cm (posed); physical weapon's closest pass mean 8.6 cm, max 8.6 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.65 s: ClubRising at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's HandR (21.3 cm); the Belly had moved 28.2 cm since the aim was locked
- 18.08 s: ClubBackhand at B Large's Neck: passed 0.2 cm from the aim point; nearest part B Large's Chest (24.9 cm); the Neck had moved 42.5 cm since the aim was locked
- 23.30 s: ClubBackhand at B Large's Neck: passed 0.1 cm from the aim point; nearest part B Large's Chest (12.6 cm); the Neck had moved 28.7 cm since the aim was locked
- 28.43 s: ClubRising at B Large's KneeL: passed 19.6 cm from the aim point; nearest part A Small's Head (46.1 cm); the KneeL had moved 3.6 cm since the aim was locked
- 29.40 s: ClubSwing at B Large's Head: passed 0.9 cm from the aim point; nearest part B Large's Head (14.6 cm); the Head had moved 24.7 cm since the aim was locked
- 31.12 s: ClubBackhand at B Large's Neck: passed 0.0 cm from the aim point; nearest part B Large's HandL (15.7 cm); the Neck had moved 49.0 cm since the aim was locked


## B Small

Present 0.0–49.6 s. Steps 66, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 17 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.1 | 5 / 60 |
| Chest | Ragdoll | 0.9 / 11.3 | 8 / 62 |
| Head | Ragdoll | 1.6 / 11.8 | 19 / 69 |
| UpperArmL | Ragdoll | 1.7 / 11.2 | 10 / 84 |
| ForearmL | Ragdoll | 3.9 / 32.7 | 21 / 180 |
| HandL | Ragdoll | 7.3 / 67.5 | 36 / 178 |
| UpperArmR | Ragdoll | 1.5 / 23.6 | 7 / 68 |
| ForearmR | Ragdoll | 2.7 / 30.8 | 11 / 90 |
| HandR | Ragdoll | 4.4 / 37.6 | 14 / 121 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 5 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 2 on allies), 51 damage. Hits taken: 6, 65 damage. Killed.

Defence: 8 blows came at it (7 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 7.

Pose against the joint limits (standing): a joint more than 10° past its limit 83% of the time; the worst 43° (ForearmL).
Ragdoll (on the ground 28.1 s): a joint more than 10° past its limit 88% of the time; the worst 175° (ShinL). Fastest part with a standing leg within 0.5 m: 1.8 m/s median, 9.1 m/s at the 95th percentile (658 steps); with none: 0.0 m/s median (1026 steps).

Look: within 25° of what it attended to 81% of the time standing (83% of the time it was within the head's reach, 96% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 6 postures (StickSettle 1, AbanicoFlurry 2, BaitRetreat 1, RedondaTwirl 1, StickStalk 1), 0 of them lead-ins to blows; energy at the end 0.13.

Balance: stumbled 0 times, knocked down 0, launched 1, got up 1; thrown into others 0 times.


## B Medium

Present 0.0–49.6 s. Steps 40, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 37 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.1 / 28.1 | 4 / 87 |
| Chest | Ragdoll | 1.0 / 23.0 | 6 / 93 |
| Head | Ragdoll | 1.5 / 24.2 | 13 / 91 |
| UpperArmL | Ragdoll | 1.8 / 21.6 | 6 / 101 |
| ForearmL | Ragdoll | 2.2 / 23.6 | 8 / 63 |
| HandL | Ragdoll | 2.8 / 32.8 | 8 / 86 |
| UpperArmR | Ragdoll | 2.1 / 48.2 | 7 / 108 |
| ForearmR | Ragdoll | 4.2 / 67.4 | 11 / 168 |
| HandR | Ragdoll | 5.6 / 42.5 | 14 / 164 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 7 |

Hits dealt: 2 (1 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 11 damage. Hits taken: 13, 105 damage. Killed.

Defence: 6 blows came at it (3 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 4.

Pose against the joint limits (standing): a joint more than 10° past its limit 100% of the time; the worst 72° (FootR).
Ragdoll (on the ground 29.0 s): a joint more than 10° past its limit 80% of the time; the worst 163° (ShinL). Fastest part with a standing leg within 0.5 m: 1.6 m/s median, 7.4 m/s at the 95th percentile (1115 steps); with none: 0.0 m/s median (622 steps).

Look: within 25° of what it attended to 90% of the time standing (91% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 94% of the time standing; its rim came within -0.8 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 2 postures (GuardChange 1, Peek 1), 0 of them lead-ins to blows; energy at the end 0.54.

Balance: stumbled 3 times, knocked down 2, launched 0, got up 2; thrown into others 2 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.97 s: SwordBackhand at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's UpperArmR (18.7 cm); the Chest had moved 36.7 cm since the aim was locked


## B Large

Present 0.0–49.6 s. Steps 70, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 54 cm from the support point; hips dropped for reach by up to 26 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 13.1 | 0 / 0 |
| Spine | Pinned | 0.0 / 13.1 | 2 / 26 |
| Chest | Pinned | 0.9 / 13.7 | 3 / 32 |
| Head | Pinned | 1.5 / 14.2 | 12 / 84 |
| UpperArmL | Pinned | 1.6 / 15.5 | 6 / 67 |
| ForearmL | Pinned | 3.7 / 45.5 | 9 / 179 |
| HandL | Pinned | 4.7 / 63.9 | 21 / 120 |
| UpperArmR | Muscled | 1.6 / 21.8 | 4 / 56 |
| ForearmR | Muscled | 3.2 / 41.7 | 5 / 64 |
| HandR | Muscled | 4.4 / 52.0 | 7 / 79 |
| ThighL | Kinematic | 0.0 / 12.6 | 0 / 150 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 13.6 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 2.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 11 (2 on the part aimed at, 2 blocked, 1 on someone in the way, 2 on allies), 106 damage. Hits taken: 12, 136 damage. Health at the end 84.

Defence: 14 blows came at it (6 seen clearly, 8 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 11.

Pose against the joint limits (standing): a joint more than 10° past its limit 100% of the time; the worst 114° (ShinL).

Look: within 25° of what it attended to 82% of the time standing (83% of the time it was within the head's reach, 97% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 10 postures (BoxerSettle 2, BoxerStalk 1, ClubFeint 2, JabFeint 2, BobAndWeave 2, Shift 1), 3 of them lead-ins to blows; energy at the end 0.69.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.2 cm, max 0.5 cm (posed); physical weapon's closest pass mean 0.2 cm, max 0.5 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.92 s: ClubRising at A Medium's Belly: passed 0.1 cm from the aim point; nearest part A Medium's Chest (13.8 cm); the Belly had moved 33.2 cm since the aim was locked
- 5.65 s: ClubSwing at A Medium's Belly: passed 0.5 cm from the aim point; nearest part A Medium's FootR (70.7 cm); the Belly had moved 116.6 cm since the aim was locked
- 36.22 s: ClubSwing at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Chest (8.9 cm); the Chest had moved 38.3 cm since the aim was locked
- 39.72 s: ClubSwing at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Chest (9.0 cm); the Chest had moved 38.2 cm since the aim was locked

## Shield contacts

159 times a shield overlapped another unit's body: 95% under 4.0 cm deep (81% under 2 cm; target 95%), the deepest 19.0 cm, the longest 0.35 s (target under 0.2 s). Each one is in events.log ("shield").

