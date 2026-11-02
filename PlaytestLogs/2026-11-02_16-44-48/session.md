# Playtest 2026-11-02_16-44-48

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 56.1 s of play (3365 physics steps), 4324 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-02_16-44-48`.

## A Small

Present 0.0–56.1 s. Steps 43, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 21 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 3 / 62 |
| Chest | ? | 0.5 / 11.1 | 4 / 111 |
| Head | ? | 1.1 / 29.6 | 8 / 93 |
| UpperArmL | ? | 1.1 / 23.7 | 8 / 174 |
| ForearmL | ? | 2.2 / 27.8 | 12 / 166 |
| HandL | ? | 2.6 / 38.7 | 23 / 177 |
| UpperArmR | ? | 0.8 / 20.7 | 4 / 106 |
| ForearmR | ? | 1.0 / 19.4 | 5 / 92 |
| HandR | ? | 1.1 / 31.8 | 11 / 89 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 25 damage. Hits taken: 4, 53 damage. Health at the end 0.

Defence: 2 blows came at it (0 seen clearly, 1 at the edge of its view, 1 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 100% of the time; the worst 56° (ForearmL).
Ragdoll (on the ground 43.9 s): a joint more than 10° past its limit 100% of the time; the worst 172° (ForearmL). Fastest part with a standing leg within 0.5 m: 0.1 m/s median, 5.0 m/s at the 95th percentile (1735 steps); with none: 0.0 m/s median (901 steps).

Look: within 25° of what it attended to 83% of the time standing (85% of the time it was within the head's reach, 94% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 4 postures (StickSettle 1, AbanicoFlurry 1, RedondaTwirl 1, StickStalk 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–56.1 s. Steps 103, idle shuffles 4. Feet slid at most 0.0 cm while planted; the weight was at most 55 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 1 / 24 |
| Chest | ? | 0.4 / 7.1 | 1 / 20 |
| Head | ? | 0.4 / 9.0 | 8 / 35 |
| UpperArmL | ? | 0.6 / 10.1 | 2 / 53 |
| ForearmL | ? | 1.3 / 22.8 | 3 / 87 |
| HandL | ? | 1.4 / 28.5 | 2 / 72 |
| UpperArmR | ? | 0.3 / 8.4 | 1 / 41 |
| ForearmR | ? | 0.5 / 6.3 | 2 / 63 |
| HandR | ? | 0.9 / 27.7 | 9 / 114 |
| ThighL | ? | 0.0 / 0.0 | 0 / 57 |
| ShinL | ? | 0.0 / 12.4 | 0 / 57 |
| FootL | ? | 0.0 / 0.0 | 0 / 1 |
| ThighR | ? | 0.0 / 0.0 | 0 / 150 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 9 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 1 on allies), 120 damage. Hits taken: 15, 65 damage. Health at the end 0.

Defence: 10 blows came at it (2 seen clearly, 8 at the edge of its view, 0 never seen); it raised the shield against 4.

Pose against the joint limits (standing): a joint more than 10° past its limit 100% of the time; the worst 125° (ShinR).
Ragdoll (on the ground 4.1 s): a joint more than 10° past its limit 100% of the time; the worst 173° (ForearmR). Fastest part with a standing leg within 0.5 m: 7.1 m/s median, 17.6 m/s at the 95th percentile (132 steps); with none: 5.4 m/s median (115 steps).

Look: within 25° of what it attended to 83% of the time standing (84% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 90% of the time standing; its rim came within 1.0 cm of its own body (negative: inside).

Fight Rhythm (?): 6 postures (GuardChange 1, Peek 2, HighFeint 1, ShieldSettle 1, ShieldPress 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 5 times, knocked down 2, launched 0, got up 1; thrown into others 2 times.

Blows: 5. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.6 cm, max 3.2 cm (posed); physical weapon's closest pass mean 0.5 cm, max 0.5 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 9.47 s: SwordCleave at B Large's Chest: passed 0.0 cm from the aim point; nearest part B Large's ForearmR (6.2 cm); the Chest had moved 21.5 cm since the aim was locked
- 25.92 s: SwordCut at B Large's Neck: passed 3.2 cm from the aim point; nearest part B Large's Chest (7.8 cm); the Neck had moved 30.2 cm since the aim was locked
- 38.00 s: SwordBackhand at B Large's KneeR: passed 0.0 cm from the aim point; nearest part B Large's ShinR (24.1 cm); the KneeR had moved 35.3 cm since the aim was locked
- 39.70 s: SwordBackhand at B Medium's ElbowL: passed 0.0 cm from the aim point; nearest part B Medium's Pelvis (15.4 cm); the ElbowL had moved 41.8 cm since the aim was locked
- 45.32 s: SwordBackhand at B Medium's Chest: passed 0.0 cm from the aim point; nearest part B Medium's Chest (9.8 cm); the Chest had moved 34.7 cm since the aim was locked


## A Large

Present 0.0–56.1 s. Steps 97, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 48 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 8.7 | 0 / 0 |
| Spine | ? | 0.0 / 8.7 | 2 / 34 |
| Chest | ? | 0.6 / 14.0 | 2 / 27 |
| Head | ? | 0.8 / 12.0 | 15 / 82 |
| UpperArmL | ? | 1.1 / 12.6 | 6 / 135 |
| ForearmL | ? | 4.0 / 39.6 | 10 / 110 |
| HandL | ? | 5.7 / 45.4 | 26 / 133 |
| UpperArmR | ? | 0.4 / 15.4 | 1 / 108 |
| ForearmR | ? | 0.9 / 56.2 | 3 / 108 |
| HandR | ? | 1.5 / 50.7 | 7 / 106 |
| ThighL | ? | 0.0 / 8.4 | 0 / 146 |
| ShinL | ? | 0.0 / 10.4 | 0 / 57 |
| FootL | ? | 0.0 / 0.0 | 0 / 3 |
| ThighR | ? | 0.0 / 8.9 | 0 / 150 |
| ShinR | ? | 0.0 / 10.5 | 0 / 4 |
| FootR | ? | 0.0 / 0.0 | 0 / 5 |

Hits dealt: 19 (3 on the part aimed at, 4 blocked, 1 on someone in the way, 2 on allies), 153 damage. Hits taken: 9, 91 damage. Health at the end 0.

Defence: 10 blows came at it (4 seen clearly, 6 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 4.

Pose against the joint limits (standing): a joint more than 10° past its limit 99% of the time; the worst 135° (ShinR).
Ragdoll (on the ground 5.3 s): a joint more than 10° past its limit 94% of the time; the worst 109° (ForearmR). Fastest part with a standing leg within 0.5 m: - median, - at the 95th percentile (0 steps); with none: 0.0 m/s median (317 steps).

Look: within 25° of what it attended to 83% of the time standing (84% of the time it was within the head's reach, 78% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 13 postures (BoxerSettle 4, BobAndWeave 4, ClubFeint 2, JabFeint 1, BoxerStalk 2), 4 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 1, launched 0, got up 0; thrown into others 0 times.

Blows: 13. The posed striking part passed within 3 cm of the aim point in 12; aim error mean 0.7 cm, max 7.5 cm (posed); physical weapon's closest pass mean 2.5 cm, max 8.5 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.65 s: ClubRising at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's UpperArmR (18.8 cm); the Belly had moved 30.4 cm since the aim was locked
- 4.50 s: ClubSwing at B Small's Belly: passed 0.6 cm from the aim point; nearest part B Small's Pelvis (38.4 cm); the Belly had moved 40.9 cm since the aim was locked
- 5.12 s: ClubBackhand at B Small's Chest: passed 7.5 cm from the aim point; nearest part B Small's Chest (11.6 cm); the Chest had moved 1.8 cm since the aim was locked
- 6.98 s: ClubRising at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (37.9 cm); the KneeL had moved 40.2 cm since the aim was locked
- 11.48 s: ClubSwing at B Small's KneeL: passed 0.0 cm from the aim point; nearest part B Small's ThighL (1.0 cm); the KneeL had moved 9.1 cm since the aim was locked
- 16.65 s: ClubBackhand at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Spine (28.3 cm); the Neck had moved 46.9 cm since the aim was locked
- 20.88 s: ClubRising at B Small's KneeR: passed 0.5 cm from the aim point; nearest part B Small's HandL (5.2 cm); the KneeR had moved 24.7 cm since the aim was locked
- 21.63 s: ClubSwing at B Small's Head: passed 0.5 cm from the aim point; nearest part B Small's Chest (18.7 cm); the Head had moved 40.8 cm since the aim was locked
- 27.02 s: ClubSwing at B Small's Neck: passed 0.1 cm from the aim point; nearest part B Small's UpperArmL (31.6 cm); the Neck had moved 56.2 cm since the aim was locked
- 32.43 s: ClubSwing at B Small's Chest: passed 0.0 cm from the aim point; nearest part B Small's Head (11.6 cm); the Chest had moved 37.5 cm since the aim was locked
- 39.90 s: ClubBackhand at B Medium's KneeL: passed 0.1 cm from the aim point; nearest part B Medium's ThighL (2.5 cm); the KneeL had moved 6.0 cm since the aim was locked
- 45.10 s: ClubSwing at B Medium's Head: passed 0.0 cm from the aim point; nearest part B Medium's Chest (31.6 cm); the Head had moved 49.8 cm since the aim was locked
- 47.63 s: ClubSwing at B Medium's ElbowR: passed 0.0 cm from the aim point; nearest part B Medium's UpperArmR (3.4 cm); the ElbowR had moved 14.6 cm since the aim was locked


## B Small

Present 0.0–56.1 s. Steps 54, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 10 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 1 / 18 |
| Chest | ? | 0.3 / 3.6 | 1 / 25 |
| Head | ? | 0.3 / 7.7 | 8 / 84 |
| UpperArmL | ? | 0.4 / 5.1 | 3 / 64 |
| ForearmL | ? | 1.2 / 15.4 | 5 / 99 |
| HandL | ? | 1.7 / 36.0 | 17 / 125 |
| UpperArmR | ? | 0.3 / 7.6 | 2 / 45 |
| ForearmR | ? | 0.8 / 17.6 | 5 / 180 |
| HandR | ? | 1.5 / 34.1 | 14 / 166 |
| ThighL | ? | 0.0 / 0.0 | 0 / 56 |
| ShinL | ? | 0.0 / 0.0 | 0 / 56 |
| FootL | ? | 0.0 / 0.0 | 0 / 4 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 1 |

Hits dealt: 2 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 15 damage. Hits taken: 8, 64 damage. Health at the end 0.

Defence: 9 blows came at it (3 seen clearly, 6 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 5.

Pose against the joint limits (standing): a joint more than 10° past its limit 97% of the time; the worst 71° (FootR).
Ragdoll (on the ground 32.8 s): a joint more than 10° past its limit 100% of the time; the worst 177° (ForearmR). Fastest part with a standing leg within 0.5 m: 1.1 m/s median, 6.5 m/s at the 95th percentile (1391 steps); with none: 0.0 m/s median (575 steps).

Look: within 25° of what it attended to 76% of the time standing (79% of the time it was within the head's reach, 96% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 0 times, knocked down 3, launched 2, got up 4; thrown into others 1 time.


## B Medium

Present 0.0–56.1 s. Steps 89, idle shuffles 1. Feet slid at most 9.0 cm while planted; the weight was at most 73 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 24.2 | 0 / 0 |
| Spine | ? | 0.0 / 24.2 | 3 / 177 |
| Chest | ? | 0.8 / 32.0 | 3 / 159 |
| Head | ? | 1.0 / 65.9 | 10 / 161 |
| UpperArmL | ? | 1.2 / 59.7 | 5 / 177 |
| ForearmL | ? | 2.1 / 40.8 | 5 / 170 |
| HandL | ? | 2.1 / 47.0 | 3 / 69 |
| UpperArmR | ? | 0.6 / 62.3 | 2 / 177 |
| ForearmR | ? | 0.7 / 24.4 | 3 / 154 |
| HandR | ? | 1.1 / 27.9 | 8 / 65 |
| ThighL | ? | 0.0 / 23.9 | 0 / 33 |
| ShinL | ? | 0.0 / 1.6 | 0 / 0 |
| FootL | ? | 0.0 / 1.2 | 0 / 0 |
| ThighR | ? | 0.0 / 24.4 | 0 / 150 |
| ShinR | ? | 0.0 / 13.3 | 0 / 0 |
| FootR | ? | 0.0 / 10.7 | 0 / 2 |

Hits dealt: 8 (3 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 93 damage. Hits taken: 13, 106 damage. Health at the end 0.

Defence: 10 blows came at it (4 seen clearly, 3 at the edge of its view, 3 never seen); it raised the shield against 5.

Pose against the joint limits (standing): a joint more than 10° past its limit 100% of the time; the worst 148° (ShinR).
Ragdoll (on the ground 11.3 s): a joint more than 10° past its limit 100% of the time; the worst 177° (ForearmL). Fastest part with a standing leg within 0.5 m: 1.6 m/s median, 7.8 m/s at the 95th percentile (233 steps); with none: 3.6 m/s median (442 steps).

Look: within 25° of what it attended to 86% of the time standing (87% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 85% of the time standing; its rim came within 1.0 cm of its own body (negative: inside).

Fight Rhythm (?): 8 postures (GuardChange 2, HighFeint 2, Peek 3, ShieldPress 1), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 3 times, knocked down 4, launched 0, got up 3; thrown into others 3 times.

Blows: 8. The posed striking part passed within 3 cm of the aim point in 7; aim error mean 4.1 cm, max 31.2 cm (posed); physical weapon's closest pass mean 3.2 cm, max 16.5 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 10.42 s: SwordCleave at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (7.0 cm); the Chest had moved 14.6 cm since the aim was locked
- 12.12 s: SwordBackhand at A Small's Chest: passed 0.0 cm from the aim point; nearest part A Small's Chest (10.6 cm); the Chest had moved 23.8 cm since the aim was locked
- 16.80 s: SwordThrust at A Medium's Head: passed 0.0 cm from the aim point; nearest part A Medium's Head (28.0 cm); the Head had moved 42.0 cm since the aim was locked
- 31.18 s: SwordThrust at A Large's Neck: passed 0.0 cm from the aim point; nearest part A Large's Chest (27.8 cm); the Neck had moved 57.6 cm since the aim was locked
- 32.58 s: SwordBackhand at A Large's Belly: passed 0.0 cm from the aim point; nearest part A Large's Spine (14.9 cm); the Belly had moved 48.4 cm since the aim was locked
- 38.03 s: SwordThrust at A Large's Head: passed 0.0 cm from the aim point; nearest part A Large's Head (32.4 cm); the Head had moved 23.1 cm since the aim was locked
- 49.75 s: SwordCut at A Large's Head: passed 31.2 cm from the aim point; nearest part A Large's UpperArmL (36.7 cm); the Head had moved 19.1 cm since the aim was locked
- 50.32 s: SwordBackhand at A Large's Chest: passed 1.6 cm from the aim point; nearest part A Large's Chest (32.2 cm); the Chest had moved 52.4 cm since the aim was locked


## B Large

Present 0.0–56.1 s. Steps 75, idle shuffles 1. Feet slid at most 4.8 cm while planted; the weight was at most 45 cm from the support point; hips dropped for reach by up to 26 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 3.1 | 0 / 0 |
| Spine | ? | 0.0 / 3.1 | 1 / 17 |
| Chest | ? | 0.5 / 8.3 | 1 / 10 |
| Head | ? | 0.7 / 7.4 | 10 / 43 |
| UpperArmL | ? | 0.9 / 11.0 | 4 / 50 |
| ForearmL | ? | 3.4 / 27.8 | 9 / 68 |
| HandL | ? | 4.9 / 45.4 | 24 / 158 |
| UpperArmR | ? | 0.5 / 6.2 | 2 / 37 |
| ForearmR | ? | 1.3 / 17.1 | 4 / 62 |
| HandR | ? | 1.9 / 50.7 | 9 / 69 |
| ThighL | ? | 0.0 / 2.6 | 0 / 51 |
| ShinL | ? | 0.0 / 16.6 | 0 / 51 |
| FootL | ? | 0.0 / 0.0 | 0 / 1 |
| ThighR | ? | 0.0 / 3.5 | 0 / 149 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 1 |

Hits dealt: 9 (1 on the part aimed at, 6 blocked, 1 on someone in the way, 1 on allies), 43 damage. Hits taken: 12, 117 damage. Health at the end 0.

Defence: 13 blows came at it (6 seen clearly, 7 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 11.

Pose against the joint limits (standing): a joint more than 10° past its limit 97% of the time; the worst 114° (FootL).
Ragdoll (on the ground 18.0 s): a joint more than 10° past its limit 100% of the time; the worst 164° (ForearmR). Fastest part with a standing leg within 0.5 m: 3.7 m/s median, 3.8 m/s at the 95th percentile (3 steps); with none: 0.0 m/s median (1079 steps).

Look: within 25° of what it attended to 81% of the time standing (82% of the time it was within the head's reach, 97% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 9 postures (BoxerSettle 2, ClubFeint 3, JabFeint 2, BobAndWeave 2), 3 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 0 times, knocked down 1, launched 0, got up 0; thrown into others 0 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.5 cm, max 0.5 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 33.50 s: ClubSwing at A Medium's Head: passed 0.0 cm from the aim point; nearest part A Medium's Head (1.9 cm); the Head had moved 15.8 cm since the aim was locked

## Shield contacts

115 times a shield overlapped another unit's body: 95% under 6.7 cm deep (72% under 2 cm; target 95%), the deepest 8.4 cm, the longest 0.27 s (target under 0.2 s). Each one is in events.log ("shield").

## Console warnings and errors

- 56.07 s Exception: MissingReferenceException: The object of type 'PhysicsBattler.Combat.Weapon' has been destroyed but you are still trying to access it. Your script should either check if it is null or you should not destroy the object.
- 56.07 s Exception: MissingReferenceException: The object of type 'PhysicsBattler.Combat.Weapon' has been destroyed but you are still trying to access it. Your script should either check if it is null or you should not destroy the object.

