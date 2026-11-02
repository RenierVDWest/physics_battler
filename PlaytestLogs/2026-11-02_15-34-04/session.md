# Playtest 2026-11-02_15-34-04

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 16.1 s of play (964 physics steps), 1249 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-02_15-34-04`.

## A Small

Present 0.0–16.1 s. Steps 50, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 16 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 1.4 | 4 / 96 |
| Chest | Pinned | 0.8 / 16.3 | 4 / 79 |
| Head | Pinned | 1.1 / 16.6 | 16 / 112 |
| UpperArmL | Pinned | 1.2 / 18.8 | 10 / 180 |
| ForearmL | Pinned | 2.9 / 33.6 | 17 / 177 |
| HandL | Pinned | 4.2 / 49.3 | 25 / 172 |
| UpperArmR | Pinned | 1.0 / 35.9 | 10 / 174 |
| ForearmR | Pinned | 2.8 / 76.1 | 13 / 174 |
| HandR | Pinned | 3.4 / 69.4 | 22 / 174 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 10 damage. Hits taken: 3, 38 damage. Health at the end 22.

Defence: 5 blows came at it (2 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 100% of the time; the worst 70° (ForearmL).

Look: within 25° of what it attended to 74% of the time standing (76% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.91.

Balance: stumbled 3 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–16.1 s. Steps 38, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 29 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.1 / 8.6 | 5 / 176 |
| Chest | Pinned | 0.8 / 21.1 | 3 / 95 |
| Head | Pinned | 1.3 / 44.3 | 14 / 174 |
| UpperArmL | Pinned | 1.7 / 105.6 | 5 / 178 |
| ForearmL | Pinned | 2.3 / 120.5 | 6 / 164 |
| HandL | Pinned | 2.8 / 96.8 | 4 / 167 |
| UpperArmR | Pinned | 0.6 / 37.0 | 2 / 129 |
| ForearmR | Pinned | 0.9 / 19.3 | 4 / 63 |
| HandR | Pinned | 1.3 / 27.7 | 12 / 87 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 3 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 43 damage. Hits taken: 6, 62 damage. Health at the end 38.

Defence: 3 blows came at it (3 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 100% of the time; the worst 34° (FootL).

Look: within 25° of what it attended to 92% of the time standing (92% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 90% of the time standing; its rim came within 1.2 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 4 postures (GuardChange 1, Peek 2, HighFeint 1), 0 of them lead-ins to blows; energy at the end 0.61.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 8.60 s: SwordThrust at B Large's Belly: passed 0.1 cm from the aim point; nearest part B Large's Chest (0.0 cm); the Belly had moved 4.2 cm since the aim was locked


## A Large

Present 0.0–16.1 s. Steps 33, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 64 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 1 / 17 |
| Chest | Pinned | 0.4 / 6.4 | 1 / 9 |
| Head | Pinned | 0.6 / 7.4 | 11 / 48 |
| UpperArmL | Pinned | 0.9 / 11.0 | 5 / 50 |
| ForearmL | Pinned | 3.4 / 23.7 | 10 / 63 |
| HandL | Pinned | 5.4 / 45.4 | 25 / 117 |
| UpperArmR | Kinematic | 0.3 / 6.2 | 1 / 37 |
| ForearmR | Kinematic | 0.7 / 13.5 | 2 / 62 |
| HandR | Kinematic | 1.4 / 50.7 | 7 / 63 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 4 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 54 damage. Hits taken: 2, 21 damage. Health at the end 199.

Defence: 7 blows came at it (4 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 5.

Pose against the joint limits (standing): a joint more than 10° past its limit 97% of the time; the worst 51° (ForearmL).

Look: within 25° of what it attended to 82% of the time standing (81% of the time it was within the head's reach, 89% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 4 postures (BoxerSettle 1, ClubFeint 1, JabFeint 1, BobAndWeave 1), 0 of them lead-ins to blows; energy at the end 0.28.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.1 cm, max 0.2 cm (posed); physical weapon's closest pass mean 8.5 cm, max 8.5 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.65 s: ClubRising at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's HandR (17.2 cm); the Belly had moved 30.8 cm since the aim was locked
- 11.00 s: ClubSwing at B Large's Neck: passed 0.2 cm from the aim point; nearest part B Large's Chest (5.7 cm); the Neck had moved 34.7 cm since the aim was locked


## B Small

Present 0.0–16.1 s. Steps 50, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 26 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 3 / 42 |
| Chest | Pinned | 0.5 / 8.0 | 3 / 56 |
| Head | Pinned | 0.6 / 13.0 | 10 / 85 |
| UpperArmL | Pinned | 0.8 / 20.6 | 6 / 74 |
| ForearmL | Pinned | 2.4 / 40.0 | 11 / 123 |
| HandL | Pinned | 3.5 / 50.7 | 26 / 168 |
| UpperArmR | Pinned | 0.3 / 7.9 | 2 / 53 |
| ForearmR | Pinned | 0.7 / 17.6 | 5 / 170 |
| HandR | Pinned | 1.3 / 33.7 | 12 / 141 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 2 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 21 damage. Hits taken: 1, 10 damage. Health at the end 50.

Defence: 2 blows came at it (1 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 100% of the time; the worst 52° (FootL).
Ragdoll (on the ground 0.9 s): a joint more than 10° past its limit 100% of the time; the worst 156° (ShinR). Fastest part with a standing leg within 0.5 m: 5.0 m/s median, 11.1 m/s at the 95th percentile (28 steps); with none: 1.1 m/s median (26 steps).

Look: within 25° of what it attended to 81% of the time standing (85% of the time it was within the head's reach, 93% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 5 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 2, RedondaTwirl 1), 0 of them lead-ins to blows; energy at the end 0.51.

Balance: stumbled 0 times, knocked down 0, launched 1, got up 1; thrown into others 0 times.

Blows: 6. The posed striking part passed within 3 cm of the aim point in 6; aim error mean 1.0 cm, max 1.7 cm (posed); physical weapon's closest pass mean 1.6 cm, max 3.9 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 10.97 s: ClubFan at A Large's ElbowL: passed 0.1 cm from the aim point; nearest part A Large's Pelvis (24.1 cm); the ElbowL had moved 97.5 cm since the aim was locked
- 11.37 s: ClubRising at A Large's KneeL: passed 1.4 cm from the aim point; nearest part A Large's ThighL (14.2 cm); the KneeL had moved 29.3 cm since the aim was locked
- 11.98 s: ClubRising at A Large's KneeL: passed 1.3 cm from the aim point; nearest part A Large's ShinL (31.6 cm); the KneeL had moved 40.4 cm since the aim was locked
- 12.38 s: ClubRising at A Large's KneeL: passed 1.2 cm from the aim point; nearest part A Large's ThighL (10.1 cm); the KneeL had moved 16.2 cm since the aim was locked
- 12.90 s: ClubSmash at A Large's Chest: passed 1.7 cm from the aim point; nearest part A Large's ForearmL (24.6 cm); the Chest had moved 44.7 cm since the aim was locked
- 14.08 s: ClubRising at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (18.8 cm); the KneeL had moved 16.0 cm since the aim was locked


## B Medium

Present 0.0–16.1 s. Steps 27, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 39 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 1 / 8 |
| Chest | ? | 0.4 / 2.4 | 2 / 11 |
| Head | ? | 0.5 / 3.5 | 11 / 73 |
| UpperArmL | ? | 0.6 / 4.4 | 3 / 56 |
| ForearmL | ? | 1.6 / 23.5 | 5 / 55 |
| HandL | ? | 2.5 / 32.8 | 4 / 65 |
| UpperArmR | ? | 0.2 / 4.5 | 1 / 70 |
| ForearmR | ? | 0.4 / 9.8 | 2 / 103 |
| HandR | ? | 0.9 / 40.4 | 7 / 65 |
| ThighL | ? | 0.0 / 0.0 | 0 / 56 |
| ShinL | ? | 0.0 / 11.4 | 0 / 56 |
| FootL | ? | 0.0 / 0.0 | 0 / 1 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 2 |

Hits dealt: 6 (0 on the part aimed at, 0 blocked, 4 on someone in the way, 0 on allies), 96 damage. Hits taken: 3, 28 damage. Health at the end 0.

Defence: 1 blows came at it (1 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 100% of the time; the worst 111° (FootR).
Ragdoll (on the ground 1.3 s): a joint more than 10° past its limit 100% of the time; the worst 103° (ForearmL). Fastest part with a standing leg within 0.5 m: 2.0 m/s median, 5.6 m/s at the 95th percentile (68 steps); with none: 3.7 m/s median (9 steps).

Look: within 25° of what it attended to 99% of the time standing (99% of the time it was within the head's reach, 95% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 95% of the time standing; its rim came within -1.0 cm of its own body (negative: inside).

Fight Rhythm (?): 3 postures (GuardChange 1, Peek 1, ShieldPress 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 1, launched 0, got up 1; thrown into others 1 time.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.3 cm, max 0.3 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.07 s: SwordBackhand at A Small's Belly: passed 0.0 cm from the aim point; nearest part A Small's ForearmR (0.6 cm); the Belly had moved 24.7 cm since the aim was locked
- 10.92 s: SwordThrust at A Small's Neck: passed 0.0 cm from the aim point; nearest part A Small's Head (0.0 cm); the Neck had moved 4.3 cm since the aim was locked
- 12.72 s: SwordBackhand at A Small's ElbowR: passed 0.0 cm from the aim point; nearest part A Small's UpperArmR (21.9 cm); the ElbowR had moved 21.8 cm since the aim was locked


## B Large

Present 0.0–16.1 s. Steps 27, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 37 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 5.3 | 5 / 172 |
| Chest | Pinned | 1.3 / 34.0 | 3 / 129 |
| Head | Pinned | 1.9 / 71.3 | 13 / 97 |
| UpperArmL | Pinned | 2.9 / 110.9 | 7 / 169 |
| ForearmL | Pinned | 4.8 / 83.9 | 11 / 171 |
| HandL | Pinned | 6.0 / 79.3 | 24 / 175 |
| UpperArmR | Kinematic | 0.6 / 6.2 | 2 / 37 |
| ForearmR | Kinematic | 1.2 / 13.5 | 4 / 62 |
| HandR | Kinematic | 2.2 / 50.7 | 10 / 63 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 3 (0 on the part aimed at, 2 blocked, 0 on someone in the way, 1 on allies), 16 damage. Hits taken: 6, 86 damage. Health at the end 134.

Defence: 7 blows came at it (4 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 6.

Pose against the joint limits (standing): a joint more than 10° past its limit 99% of the time; the worst 51° (ForearmL).

Look: within 25° of what it attended to 77% of the time standing (84% of the time it was within the head's reach, 87% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 3 postures (BoxerSettle 1, BoxerStalk 1, BobAndWeave 1), 1 of them lead-ins to blows; energy at the end 0.57.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

## Shield contacts

73 times a shield overlapped another unit's body: 95% under 10.3 cm deep (71% under 2 cm; target 95%), the deepest 20.8 cm, the longest 0.17 s (target under 0.2 s). Each one is in events.log ("shield").

