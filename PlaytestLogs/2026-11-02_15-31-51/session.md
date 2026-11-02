# Playtest 2026-11-02_15-31-51

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 48.9 s of play (2936 physics steps), 4039 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-02_15-31-51`.

## A Small

Present 0.0–48.9 s. Steps 16, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 6 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.0 | 2 / 21 |
| Chest | ? | 0.4 / 4.1 | 2 / 14 |
| Head | ? | 0.5 / 6.1 | 11 / 75 |
| UpperArmL | ? | 0.5 / 6.2 | 7 / 148 |
| ForearmL | ? | 1.5 / 15.5 | 12 / 164 |
| HandL | ? | 3.0 / 36.0 | 24 / 117 |
| UpperArmR | ? | 0.5 / 5.6 | 4 / 50 |
| ForearmR | ? | 1.4 / 19.4 | 8 / 92 |
| HandR | ? | 2.2 / 31.8 | 23 / 89 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 3, 68 damage. Health at the end 0.

Defence: 3 blows came at it (1 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 100% of the time; the worst 57° (ForearmL).
Ragdoll (on the ground 43.6 s): a joint more than 10° past its limit 100% of the time; the worst 178° (ForearmL). Fastest part with a standing leg within 0.5 m: 1.0 m/s median, 9.2 m/s at the 95th percentile (1314 steps); with none: 0.0 m/s median (1299 steps).

Look: within 25° of what it attended to 76% of the time standing (77% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 2 postures (StickSettle 1, AbanicoFlurry 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–48.9 s. Steps 39, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 22 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.2 | 1 / 12 |
| Chest | Ragdoll | 0.3 / 3.5 | 2 / 14 |
| Head | Ragdoll | 0.5 / 4.0 | 10 / 56 |
| UpperArmL | Ragdoll | 0.6 / 5.8 | 4 / 98 |
| ForearmL | Ragdoll | 1.9 / 44.4 | 5 / 118 |
| HandL | Ragdoll | 2.8 / 79.7 | 3 / 55 |
| UpperArmR | Ragdoll | 0.3 / 3.4 | 2 / 41 |
| ForearmR | Ragdoll | 0.7 / 5.9 | 3 / 63 |
| HandR | Ragdoll | 1.3 / 27.7 | 11 / 76 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 3 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 1 on allies), 23 damage. Hits taken: 14, 103 damage. Killed.

Defence: 8 blows came at it (4 seen clearly, 4 at the edge of its view, 0 never seen); it raised the shield against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 100% of the time; the worst 40° (FootR).
Ragdoll (on the ground 33.0 s): a joint more than 10° past its limit 100% of the time; the worst 135° (ForearmL). Fastest part with a standing leg within 0.5 m: 0.0 m/s median, 6.0 m/s at the 95th percentile (829 steps); with none: 0.0 m/s median (1151 steps).

Look: within 25° of what it attended to 76% of the time standing (76% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 81% of the time standing; its rim came within 1.0 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 2 postures (GuardChange 1, Peek 1), 0 of them lead-ins to blows; energy at the end 0.51.

Balance: stumbled 3 times, knocked down 0, launched 1, got up 0; thrown into others 4 times.


## A Large

Present 0.0–48.9 s. Steps 84, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 56 cm from the support point; hips dropped for reach by up to 26 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.2 | 1 / 26 |
| Chest | Ragdoll | 0.6 / 11.2 | 1 / 16 |
| Head | Ragdoll | 0.8 / 14.0 | 11 / 49 |
| UpperArmL | Ragdoll | 1.0 / 13.0 | 5 / 50 |
| ForearmL | Ragdoll | 3.6 / 23.9 | 9 / 81 |
| HandL | Ragdoll | 4.7 / 45.4 | 22 / 121 |
| UpperArmR | Ragdoll | 0.5 / 14.7 | 2 / 61 |
| ForearmR | Ragdoll | 1.2 / 31.0 | 4 / 62 |
| HandR | Ragdoll | 1.8 / 50.7 | 9 / 63 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 149 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 1 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 149 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 7 (1 on the part aimed at, 2 blocked, 0 on someone in the way, 0 on allies), 64 damage. Hits taken: 20, 291 damage. Killed.

Defence: 20 blows came at it (8 seen clearly, 12 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 11.

Pose against the joint limits (standing): a joint more than 10° past its limit 99% of the time; the worst 110° (ShinL).
Ragdoll (on the ground 5.2 s): a joint more than 10° past its limit 100% of the time; the worst 112° (ForearmR). Fastest part with a standing leg within 0.5 m: - median, - at the 95th percentile (0 steps); with none: 0.0 m/s median (314 steps).

Look: within 25° of what it attended to 88% of the time standing (87% of the time it was within the head's reach, 86% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 5 postures (BoxerSettle 2, ClubFeint 1, JabFeint 1, BobAndWeave 1), 1 of them lead-ins to blows; energy at the end 0.05.

Balance: stumbled 4 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 7. The posed striking part passed within 3 cm of the aim point in 7; aim error mean 0.1 cm, max 0.3 cm (posed); physical weapon's closest pass mean 0.4 cm, max 0.5 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.75 s: ClubBackhand at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's UpperArmR (29.2 cm); the Belly had moved 52.5 cm since the aim was locked
- 5.92 s: ClubSwing at B Small's Chest: passed 0.0 cm from the aim point; nearest part B Small's ThighL (43.6 cm); the Chest had moved 60.9 cm since the aim was locked
- 10.40 s: ClubSwing at B Small's Chest: passed 0.0 cm from the aim point; nearest part B Small's ForearmL (28.0 cm); the Chest had moved 66.4 cm since the aim was locked
- 24.45 s: ClubBackhand at B Small's Chest: passed 0.0 cm from the aim point; nearest part B Small's UpperArmR (20.0 cm); the Chest had moved 36.0 cm since the aim was locked
- 26.73 s: ClubSwing at B Medium's Head: passed 0.0 cm from the aim point; nearest part B Medium's HandL (42.8 cm); the Head had moved 79.1 cm since the aim was locked
- 35.08 s: ClubRising at B Small's Belly: passed 0.3 cm from the aim point; nearest part B Small's UpperArmR (4.0 cm); the Belly had moved 6.9 cm since the aim was locked
- 41.53 s: ClubSwing at B Small's Belly: passed 0.1 cm from the aim point; nearest part B Small's UpperArmL (16.1 cm); the Belly had moved 42.2 cm since the aim was locked


## B Small

Present 0.0–48.9 s. Steps 100, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 31 cm from the support point; hips dropped for reach by up to 5 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 4.3 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 13.5 | 3 / 177 |
| Chest | Ragdoll | 0.5 / 21.6 | 3 / 168 |
| Head | Ragdoll | 0.6 / 37.9 | 8 / 128 |
| UpperArmL | Ragdoll | 0.7 / 49.4 | 5 / 176 |
| ForearmL | Ragdoll | 1.7 / 45.5 | 8 / 177 |
| HandL | Ragdoll | 2.1 / 36.0 | 19 / 176 |
| UpperArmR | Ragdoll | 0.4 / 20.2 | 3 / 111 |
| ForearmR | Ragdoll | 0.9 / 17.6 | 4 / 81 |
| HandR | Ragdoll | 1.3 / 27.1 | 14 / 93 |
| ThighL | Ragdoll | 0.0 / 4.3 | 0 / 5 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 23 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 4.3 | 0 / 150 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 1 on allies), 13 damage. Hits taken: 8, 69 damage. Killed.

Defence: 8 blows came at it (6 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 7.

Pose against the joint limits (standing): a joint more than 10° past its limit 100% of the time; the worst 152° (ShinR).
Ragdoll (on the ground 12.1 s): a joint more than 10° past its limit 100% of the time; the worst 175° (ShinR). Fastest part with a standing leg within 0.5 m: 3.5 m/s median, 8.1 m/s at the 95th percentile (141 steps); with none: 0.2 m/s median (582 steps).

Look: within 25° of what it attended to 84% of the time standing (88% of the time it was within the head's reach, 92% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 9 postures (StickSettle 2, AbanicoFlurry 3, RedondaTwirl 3, BaitRetreat 1), 1 of them lead-ins to blows; energy at the end 0.67.

Balance: stumbled 1 time, knocked down 2, launched 2, got up 4; thrown into others 2 times.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.0 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.2 cm, max 0.2 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 20.83 s: ClubBackhand at A Large's Belly: passed 0.0 cm from the aim point; nearest part A Large's Chest (20.7 cm); the Belly had moved 30.0 cm since the aim was locked
- 23.02 s: ClubBackhand at A Large's Belly: passed 0.1 cm from the aim point; nearest part A Large's Pelvis (14.7 cm); the Belly had moved 17.3 cm since the aim was locked


## B Medium

Present 0.0–48.9 s. Steps 81, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 35 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.1 | 1 / 11 |
| Chest | Ragdoll | 0.4 / 3.3 | 1 / 12 |
| Head | Ragdoll | 0.4 / 3.4 | 8 / 46 |
| UpperArmL | Ragdoll | 0.6 / 4.4 | 3 / 72 |
| ForearmL | Ragdoll | 1.3 / 23.5 | 3 / 104 |
| HandL | Ragdoll | 1.7 / 51.2 | 3 / 132 |
| UpperArmR | Ragdoll | 0.3 / 4.9 | 1 / 43 |
| ForearmR | Ragdoll | 0.5 / 7.6 | 2 / 64 |
| HandR | Ragdoll | 1.0 / 27.9 | 9 / 64 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 1 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 150 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 12 (4 on the part aimed at, 1 blocked, 0 on someone in the way, 1 on allies), 162 damage. Hits taken: 8, 142 damage. Killed.

Defence: 3 blows came at it (3 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 100% of the time; the worst 133° (ShinR).
Ragdoll (on the ground 12.1 s): a joint more than 10° past its limit 100% of the time; the worst 177° (ForearmL). Fastest part with a standing leg within 0.5 m: 7.3 m/s median, 15.6 m/s at the 95th percentile (18 steps); with none: 4.5 m/s median (705 steps).

Look: within 25° of what it attended to 85% of the time standing (85% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 93% of the time standing; its rim came within -2.9 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 7 postures (GuardChange 2, ShieldSettle 1, ShieldPress 1, Peek 2, HighFeint 1), 1 of them lead-ins to blows; energy at the end 0.34.

Balance: stumbled 3 times, knocked down 4, launched 0, got up 2; thrown into others 3 times.

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.7 cm, max 1.5 cm (posed); physical weapon's closest pass mean 0.7 cm, max 1.3 cm. 2 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.98 s: SwordCut at A Small's Head: passed 0.1 cm from the aim point; nearest part A Small's Head (15.8 cm); the Head had moved 28.0 cm since the aim was locked
- 4.63 s: SwordBackhand at A Small's Chest: passed 0.4 cm from the aim point; nearest part A Small's UpperArmL (3.3 cm); the Chest had moved 20.0 cm since the aim was locked
- 5.35 s: SwordCleave at A Small's Chest: passed 1.3 cm from the aim point; nearest part A Small's Chest (4.3 cm); the Chest had moved 15.2 cm since the aim was locked
- 31.43 s: SwordBackhand at A Large's Belly: passed 1.5 cm from the aim point; nearest part A Large's Spine (3.2 cm); the Belly had moved 14.0 cm since the aim was locked
- 41.85 s: SwordThrust at A Large's Chest: passed 0.1 cm from the aim point; nearest part A Large's UpperArmL (48.9 cm); the Chest had moved 106.7 cm since the aim was locked


## B Large

Present 0.0–48.9 s. Steps 82, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 41 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 1 / 24 |
| Chest | Pinned | 0.5 / 10.0 | 1 / 23 |
| Head | Pinned | 0.7 / 10.6 | 10 / 152 |
| UpperArmL | Pinned | 0.9 / 17.1 | 6 / 177 |
| ForearmL | Pinned | 3.9 / 48.4 | 10 / 129 |
| HandL | Pinned | 5.2 / 45.4 | 23 / 135 |
| UpperArmR | Pinned | 0.4 / 6.2 | 1 / 63 |
| ForearmR | Pinned | 1.0 / 22.8 | 3 / 63 |
| HandR | Pinned | 1.7 / 50.7 | 8 / 63 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 150 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 150 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 16 (1 on the part aimed at, 4 blocked, 0 on someone in the way, 3 on allies), 135 damage. Hits taken: 5, 26 damage. Health at the end 194.

Defence: 4 blows came at it (3 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 99% of the time; the worst 102° (ShinL).

Look: within 25° of what it attended to 87% of the time standing (87% of the time it was within the head's reach, 97% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 16 postures (BoxerSettle 3, BoxerStalk 2, BobAndWeave 2, ClubFeint 4, Shift 2, JabFeint 3), 5 of them lead-ins to blows; energy at the end 0.95.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.4 cm, max 1.2 cm (posed); physical weapon's closest pass mean 1.5 cm, max 2.6 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 5.05 s: ClubSwing at A Medium's Head: passed 0.0 cm from the aim point; nearest part A Medium's HandR (24.1 cm); the Head had moved 45.4 cm since the aim was locked
- 21.45 s: ClubRising at A Large's KneeL: passed 1.2 cm from the aim point; nearest part B Small's HandR (26.5 cm); the KneeL had moved 34.9 cm since the aim was locked
- 42.13 s: ClubSwing at A Large's Belly: passed 0.0 cm from the aim point; nearest part A Large's UpperArmL (17.5 cm); the Belly had moved 79.3 cm since the aim was locked

## Shield contacts

56 times a shield overlapped another unit's body: 95% under 4.0 cm deep (84% under 2 cm; target 95%), the deepest 4.2 cm, the longest 0.12 s (target under 0.2 s). Each one is in events.log ("shield").

