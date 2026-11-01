# Playtest 2026-11-01_13-09-49

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 24.0 s of play (1441 physics steps), 1464 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-01_13-09-49`.

## A Small

Present 0.0–24.0 s. Steps 20, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 7 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.0 | 2 / 9 |
| Chest | Ragdoll | 0.3 / 1.8 | 2 / 5 |
| Head | Ragdoll | 0.4 / 1.8 | 5 / 17 |
| UpperArmL | Ragdoll | 0.4 / 2.0 | 3 / 44 |
| ForearmL | Ragdoll | 0.9 / 15.4 | 6 / 99 |
| HandL | Ragdoll | 2.3 / 36.0 | 22 / 92 |
| UpperArmR | Ragdoll | 0.4 / 1.7 | 3 / 75 |
| ForearmR | Ragdoll | 1.2 / 13.4 | 9 / 117 |
| HandR | Ragdoll | 2.7 / 31.6 | 30 / 125 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 3, 70 damage. Killed.

Defence: 3 blows came at it (2 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 69% of the time; the worst 42° (FootL).
Ragdoll (on the ground 16.7 s): a joint more than 10° past its limit 6% of the time; the worst 123° (HandR). Fastest part with a standing leg within 0.5 m: 0.0 m/s median, 0.0 m/s at the 95th percentile (22 steps); with none: 0.0 m/s median (978 steps).

Look: within 25° of what it attended to 76% of the time standing (76% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 2 postures (StickSettle 1, AbanicoFlurry 1), 0 of them lead-ins to blows; energy at the end 0.70.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–24.0 s. Steps 11, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 23 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.1 | 1 / 10 |
| Chest | Ragdoll | 0.4 / 2.7 | 2 / 9 |
| Head | Ragdoll | 0.5 / 2.7 | 8 / 18 |
| UpperArmL | Ragdoll | 0.6 / 5.0 | 2 / 54 |
| ForearmL | Ragdoll | 1.2 / 23.1 | 3 / 46 |
| HandL | Ragdoll | 1.5 / 29.0 | 13 / 66 |
| UpperArmR | Ragdoll | 0.5 / 4.3 | 3 / 50 |
| ForearmR | Ragdoll | 1.4 / 9.1 | 6 / 70 |
| HandR | Ragdoll | 2.5 / 29.6 | 22 / 178 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 8 damage. Hits taken: 4, 26 damage. Killed.

Defence: 1 blows came at it (1 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 0.

Pose against the joint limits (standing): a joint more than 10° past its limit 49% of the time; the worst 146° (HandR).
Ragdoll (on the ground 18.2 s): a joint more than 10° past its limit 67% of the time; the worst 162° (HandL). Fastest part with a standing leg within 0.5 m: 1.5 m/s median, 11.9 m/s at the 95th percentile (860 steps); with none: 0.0 m/s median (234 steps).

Look: within 25° of what it attended to 89% of the time standing (89% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 90% of the time standing; its rim came within 1.4 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 3 postures (GuardChange 1, HighFeint 1, ShieldPress 1), 1 of them lead-ins to blows; energy at the end 0.75.

Balance: stumbled 0 times, knocked down 1, launched 0, got up 0; thrown into others 3 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.70 s: SwordBackhand at B Large's Chest: passed 0.0 cm from the aim point; nearest part B Large's Chest (5.6 cm); the Chest had moved 24.0 cm since the aim was locked


## A Large

Present 0.0–24.0 s. Steps 49, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 53 cm from the support point; hips dropped for reach by up to 5 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 1.5 | 3 / 156 |
| Chest | Pinned | 1.0 / 54.1 | 2 / 81 |
| Head | Pinned | 1.2 / 77.2 | 14 / 177 |
| UpperArmL | Pinned | 1.6 / 90.6 | 6 / 180 |
| ForearmL | Pinned | 3.5 / 56.1 | 10 / 179 |
| HandL | Pinned | 5.0 / 71.6 | 22 / 160 |
| UpperArmR | Pinned | 0.9 / 10.6 | 4 / 161 |
| ForearmR | Pinned | 2.6 / 73.5 | 9 / 165 |
| HandR | Pinned | 3.7 / 56.9 | 20 / 147 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 6 (0 on the part aimed at, 0 blocked, 2 on someone in the way, 0 on allies), 85 damage. Hits taken: 11, 110 damage. Health at the end 110.

Defence: 13 blows came at it (4 seen clearly, 9 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 12.

Pose against the joint limits (standing): a joint more than 10° past its limit 61% of the time; the worst 160° (HandR).

Look: within 25° of what it attended to 85% of the time standing (85% of the time it was within the head's reach, 90% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 3 postures (BoxerSettle 1, ClubFeint 1, BobAndWeave 1), 0 of them lead-ins to blows; energy at the end 0.41.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.55 s: ClubRising at B Small's Chest: passed 0.1 cm from the aim point; nearest part B Small's UpperArmR (5.8 cm); the Chest had moved 24.6 cm since the aim was locked
- 22.60 s: ClubRising at B Small's Belly: passed 0.1 cm from the aim point; nearest part B Large's ThighL (22.6 cm); the Belly had moved 54.6 cm since the aim was locked


## B Small

Present 0.0–24.0 s. Steps 67, idle shuffles 0. Feet slid at most 12.9 cm while planted; the weight was at most 33 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 3 / 125 |
| Chest | Pinned | 0.5 / 10.0 | 3 / 64 |
| Head | Pinned | 0.6 / 15.0 | 12 / 158 |
| UpperArmL | Pinned | 0.8 / 23.9 | 5 / 115 |
| ForearmL | Pinned | 1.8 / 27.1 | 10 / 174 |
| HandL | Pinned | 2.6 / 36.0 | 24 / 177 |
| UpperArmR | Pinned | 0.5 / 9.1 | 3 / 57 |
| ForearmR | Pinned | 1.3 / 22.3 | 8 / 179 |
| HandR | Pinned | 2.7 / 57.5 | 23 / 170 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 4 (0 on the part aimed at, 2 blocked, 0 on someone in the way, 0 on allies), 17 damage. Hits taken: 3, 54 damage. Health at the end 6.

Defence: 2 blows came at it (1 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 72% of the time; the worst 165° (HandR).
Ragdoll (on the ground 3.3 s): a joint more than 10° past its limit 91% of the time; the worst 163° (ForearmR). Fastest part with a standing leg within 0.5 m: 8.0 m/s median, 9.2 m/s at the 95th percentile (32 steps); with none: 4.0 m/s median (164 steps).

Look: within 25° of what it attended to 79% of the time standing (82% of the time it was within the head's reach, 94% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 6 postures (StickSettle 1, AbanicoFlurry 3, RedondaTwirl 2), 0 of them lead-ins to blows; energy at the end 0.43.

Balance: stumbled 0 times, knocked down 0, launched 2, got up 1; thrown into others 1 time. Still down at the end.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.2 cm, max 0.2 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 14.00 s: ClubRising at A Large's Belly: passed 0.0 cm from the aim point; nearest part A Large's ForearmR (0.7 cm); the Belly had moved 11.1 cm since the aim was locked
- 17.72 s: ClubSwing at A Large's Belly: passed 0.0 cm from the aim point; nearest part A Large's ForearmR (22.1 cm); the Belly had moved 23.8 cm since the aim was locked


## B Medium

Present 0.0–24.0 s. Steps 59, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 52 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 1.5 | 5 / 92 |
| Chest | Pinned | 1.2 / 22.9 | 5 / 117 |
| Head | Pinned | 1.5 / 36.6 | 28 / 178 |
| UpperArmL | Pinned | 2.2 / 55.7 | 11 / 176 |
| ForearmL | Pinned | 3.7 / 73.5 | 15 / 177 |
| HandL | Pinned | 3.8 / 49.7 | 22 / 175 |
| UpperArmR | Pinned | 0.3 / 4.3 | 2 / 67 |
| ForearmR | Pinned | 0.9 / 30.4 | 4 / 81 |
| HandR | Pinned | 1.6 / 30.5 | 15 / 164 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 10 (1 on the part aimed at, 0 blocked, 1 on someone in the way, 0 on allies), 141 damage. Hits taken: 2, 0 damage. Health at the end 100.

Pose against the joint limits (standing): a joint more than 10° past its limit 58% of the time; the worst 165° (HandR).

Look: within 25° of what it attended to 90% of the time standing (91% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 95% of the time standing; its rim came within 1.0 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 5 postures (GuardChange 2, ShieldSettle 1, ShieldAdvance 1, HighFeint 1), 1 of them lead-ins to blows; energy at the end 0.35.

Blows: 6. The posed striking part passed within 3 cm of the aim point in 6; aim error mean 0.2 cm, max 0.6 cm (posed); physical weapon's closest pass mean 0.6 cm, max 1.0 cm. 2 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.90 s: SwordThrust at A Small's Belly: passed 0.3 cm from the aim point; nearest part A Small's Spine (0.0 cm); the Belly had moved 14.8 cm since the aim was locked
- 5.65 s: SwordCleave at A Small's Belly: passed 0.0 cm from the aim point; nearest part A Small's Pelvis (15.4 cm); the Belly had moved 34.3 cm since the aim was locked
- 7.45 s: SwordThrust at A Small's Head: passed 0.6 cm from the aim point; nearest part A Small's Head (0.0 cm); the Head had moved 12.1 cm since the aim was locked
- 18.67 s: SwordThrust at A Large's Head: passed 0.1 cm from the aim point; nearest part A Large's HandL (0.6 cm); the Head had moved 24.4 cm since the aim was locked
- 20.30 s: SwordThrust at A Large's Chest: passed 0.1 cm from the aim point; nearest part A Large's HandR (3.3 cm); the Chest had moved 26.5 cm since the aim was locked
- 22.80 s: SwordCut at A Large's Neck: passed 0.0 cm from the aim point; nearest part A Large's Chest (84.7 cm); the Neck had moved 112.1 cm since the aim was locked


## B Large

Present 0.0–24.0 s. Steps 50, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 23 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 1 / 17 |
| Chest | ? | 0.6 / 6.4 | 2 / 17 |
| Head | ? | 0.9 / 7.4 | 10 / 43 |
| UpperArmL | ? | 1.1 / 14.6 | 5 / 50 |
| ForearmL | ? | 3.6 / 23.7 | 10 / 161 |
| HandL | ? | 4.7 / 45.4 | 20 / 99 |
| UpperArmR | ? | 1.1 / 8.4 | 4 / 74 |
| ForearmR | ? | 3.0 / 38.4 | 10 / 98 |
| HandR | ? | 4.5 / 56.9 | 27 / 164 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 25 damage. Hits taken: 8, 70 damage. Health at the end 0.

Defence: 4 blows came at it (2 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 56% of the time; the worst 151° (HandR).

Look: within 25° of what it attended to 81% of the time standing (85% of the time it was within the head's reach, 95% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 6 postures (BoxerSettle 1, BoxerStalk 1, BobAndWeave 1, ClubFeint 2, JabFeint 1), 2 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

