# Playtest 2026-11-02_07-01-54

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 55.7 s of play (3344 physics steps), 2727 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-02_07-01-54`.

## A Small

Present 0.0–55.7 s. Steps 20, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 7 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.0 | 1 / 8 |
| Chest | ? | 0.3 / 1.6 | 2 / 7 |
| Head | ? | 0.4 / 2.8 | 9 / 76 |
| UpperArmL | ? | 0.4 / 2.6 | 6 / 80 |
| ForearmL | ? | 1.1 / 15.4 | 11 / 134 |
| HandL | ? | 3.1 / 36.0 | 20 / 161 |
| UpperArmR | ? | 0.4 / 3.0 | 7 / 92 |
| ForearmR | ? | 1.1 / 14.5 | 10 / 119 |
| HandR | ? | 2.6 / 31.5 | 20 / 102 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 3, 49 damage. Health at the end 0.

Defence: 3 blows came at it (1 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 64% of the time; the worst 42° (FootL).
Ragdoll (on the ground 49.4 s): a joint more than 10° past its limit 100% of the time; the worst 88° (FootR). Fastest part with a standing leg within 0.5 m: 0.0 m/s median, 0.1 m/s at the 95th percentile (325 steps); with none: 0.0 m/s median (2636 steps).

Look: within 25° of what it attended to 76% of the time standing (76% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 2 postures (StickSettle 1, AbanicoFlurry 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 3 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–55.7 s. Steps 34, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 27 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 2 / 17 |
| Chest | ? | 0.5 / 4.9 | 2 / 13 |
| Head | ? | 0.5 / 4.3 | 12 / 83 |
| UpperArmL | ? | 0.7 / 7.7 | 4 / 62 |
| ForearmL | ? | 1.5 / 28.5 | 3 / 63 |
| HandL | ? | 1.7 / 32.1 | 1 / 55 |
| UpperArmR | ? | 0.3 / 5.7 | 6 / 91 |
| ForearmR | ? | 0.9 / 17.3 | 5 / 122 |
| HandR | ? | 1.4 / 25.3 | 10 / 148 |
| ThighL | ? | 0.0 / 0.0 | 0 / 1 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 6 (2 on the part aimed at, 1 blocked, 1 on someone in the way, 0 on allies), 64 damage. Hits taken: 3, 13 damage. Health at the end 0.

Defence: 4 blows came at it (2 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0.

Pose against the joint limits (standing): a joint more than 10° past its limit 28% of the time; the worst 32° (FootL).
Ragdoll (on the ground 41.7 s): a joint more than 10° past its limit 6% of the time; the worst 173° (ForearmL). Fastest part with a standing leg within 0.5 m: 3.2 m/s median, 10.0 m/s at the 95th percentile (10 steps); with none: 0.0 m/s median (2490 steps).

Look: within 25° of what it attended to 77% of the time standing (77% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 94% of the time standing; its rim came within 1.0 cm of its own body (negative: inside).

Fight Rhythm (?): 4 postures (GuardChange 1, Peek 1, HighFeint 1, ShieldAdvance 1), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 0 times, knocked down 1, launched 0, got up 0; thrown into others 1 time.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.2 cm, max 0.4 cm (posed); physical weapon's closest pass mean 0.3 cm, max 0.3 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.17 s: SwordBackhand at B Large's Chest: passed 0.4 cm from the aim point; nearest part B Large's Chest (2.2 cm); the Chest had moved 23.9 cm since the aim was locked
- 5.08 s: SwordCleave at B Large's Chest: passed 0.2 cm from the aim point; nearest part B Large's HandR (4.1 cm); the Chest had moved 37.6 cm since the aim was locked
- 13.43 s: SwordThrust at B Medium's Head: passed 0.1 cm from the aim point; nearest part B Medium's Head (11.9 cm); the Head had moved 28.7 cm since the aim was locked


## A Large

Present 0.0–55.7 s. Steps 105, idle shuffles 2. Feet slid at most 16.8 cm while planted; the weight was at most 89 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 32.5 | 0 / 0 |
| Spine | ? | 0.0 / 32.5 | 2 / 27 |
| Chest | ? | 0.7 / 27.6 | 1 / 20 |
| Head | ? | 0.8 / 23.0 | 12 / 61 |
| UpperArmL | ? | 1.0 / 32.4 | 24 / 108 |
| ForearmL | ? | 4.0 / 59.0 | 11 / 111 |
| HandL | ? | 5.2 / 52.1 | 21 / 126 |
| UpperArmR | ? | 0.6 / 19.1 | 14 / 88 |
| ForearmR | ? | 1.3 / 30.8 | 5 / 71 |
| HandR | ? | 1.9 / 60.1 | 12 / 81 |
| ThighL | ? | 0.0 / 32.0 | 0 / 35 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 32.9 | 0 / 149 |
| ShinR | ? | 0.0 / 22.1 | 0 / 0 |
| FootR | ? | 0.0 / 19.3 | 0 / 3 |

Hits dealt: 15 (2 on the part aimed at, 4 blocked, 2 on someone in the way, 0 on allies), 144 damage. Hits taken: 5, 77 damage. Health at the end 0.

Defence: 6 blows came at it (4 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 33% of the time; the worst 138° (ShinR).

Look: within 25° of what it attended to 88% of the time standing (88% of the time it was within the head's reach, 87% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 14 postures (BoxerSettle 2, BobAndWeave 3, ClubFeint 4, JabFeint 2, BoxerStalk 3), 3 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 10. The posed striking part passed within 3 cm of the aim point in 10; aim error mean 0.3 cm, max 0.9 cm (posed); physical weapon's closest pass mean 4.6 cm, max 10.9 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.55 s: ClubRising at B Small's KneeL: passed 0.5 cm from the aim point; nearest part B Small's ThighR (5.6 cm); the KneeL had moved 27.6 cm since the aim was locked
- 4.50 s: ClubSwing at B Small's Belly: passed 0.9 cm from the aim point; nearest part B Small's FootR (25.2 cm); the Belly had moved 45.2 cm since the aim was locked
- 10.42 s: ClubRising at B Small's KneeL: passed 0.3 cm from the aim point; nearest part B Small's ShinL (10.0 cm); the KneeL had moved 11.5 cm since the aim was locked
- 11.60 s: ClubSwing at B Small's ElbowR: passed 0.4 cm from the aim point; nearest part B Large's ThighL (0.0 cm); the ElbowR had moved 9.4 cm since the aim was locked
- 12.33 s: ClubRising at B Small's Chest: passed 0.3 cm from the aim point; nearest part B Small's Head (6.1 cm); the Chest had moved 10.1 cm since the aim was locked
- 14.48 s: ClubSmash at B Medium's Chest: passed 0.2 cm from the aim point; nearest part B Medium's Chest (10.1 cm); the Chest had moved 32.5 cm since the aim was locked
- 22.37 s: ClubBackhand at B Small's Chest: passed 0.2 cm from the aim point; nearest part B Small's Spine (0.0 cm); the Chest had moved 13.6 cm since the aim was locked
- 24.67 s: ClubSmash at B Medium's Head: passed 0.2 cm from the aim point; nearest part B Medium's Head (17.5 cm); the Head had moved 20.2 cm since the aim was locked
- 31.02 s: ClubSwing at B Medium's KneeR: passed 0.4 cm from the aim point; nearest part B Medium's ThighR (0.3 cm); the KneeR had moved 15.1 cm since the aim was locked
- 42.18 s: ClubFan at B Medium's Head: passed 0.1 cm from the aim point; nearest part B Medium's Head (20.7 cm); the Head had moved 33.4 cm since the aim was locked


## B Small

Present 0.0–55.7 s. Steps 41, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 9 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 4 / 46 |
| Chest | ? | 0.7 / 8.9 | 7 / 64 |
| Head | ? | 1.5 / 10.5 | 18 / 68 |
| UpperArmL | ? | 2.1 / 21.0 | 13 / 105 |
| ForearmL | ? | 4.7 / 27.1 | 20 / 175 |
| HandL | ? | 5.8 / 38.5 | 27 / 135 |
| UpperArmR | ? | 1.7 / 12.1 | 11 / 118 |
| ForearmR | ? | 2.1 / 28.6 | 13 / 176 |
| HandR | ? | 3.1 / 32.8 | 25 / 173 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 6, 60 damage. Health at the end 0.

Defence: 7 blows came at it (4 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 54% of the time; the worst 51° (FootR).
Ragdoll (on the ground 40.5 s): a joint more than 10° past its limit 95% of the time; the worst 173° (ForearmR). Fastest part with a standing leg within 0.5 m: 1.8 m/s median, 7.3 m/s at the 95th percentile (280 steps); with none: 0.0 m/s median (2148 steps).

Look: within 25° of what it attended to 88% of the time standing (90% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 0 times, knocked down 1, launched 2, got up 2; thrown into others 0 times.


## B Medium

Present 0.0–55.7 s. Steps 96, idle shuffles 4. Feet slid at most 6.4 cm while planted; the weight was at most 78 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 19.4 | 0 / 0 |
| Spine | ? | 0.0 / 19.4 | 2 / 30 |
| Chest | ? | 0.6 / 20.1 | 2 / 76 |
| Head | ? | 0.7 / 26.5 | 13 / 89 |
| UpperArmL | ? | 0.9 / 25.4 | 5 / 177 |
| ForearmL | ? | 1.9 / 40.8 | 6 / 175 |
| HandL | ? | 2.0 / 53.3 | 3 / 111 |
| UpperArmR | ? | 0.4 / 13.6 | 14 / 180 |
| ForearmR | ? | 1.1 / 19.7 | 9 / 172 |
| HandR | ? | 1.9 / 38.4 | 16 / 180 |
| ThighL | ? | 0.0 / 19.5 | 0 / 150 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 19.4 | 0 / 150 |
| ShinR | ? | 0.0 / 9.8 | 0 / 0 |
| FootR | ? | 0.0 / 7.6 | 0 / 0 |

Hits dealt: 9 (4 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 121 damage. Hits taken: 11, 97 damage. Health at the end 0.

Defence: 10 blows came at it (8 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 6.

Pose against the joint limits (standing): a joint more than 10° past its limit 36% of the time; the worst 164° (ShinR).
Ragdoll (on the ground 5.0 s): a joint more than 10° past its limit 96% of the time; the worst 174° (ForearmR). Fastest part with a standing leg within 0.5 m: 8.1 m/s median, 13.1 m/s at the 95th percentile (95 steps); with none: 3.1 m/s median (204 steps).

Look: within 25° of what it attended to 83% of the time standing (84% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 90% of the time standing; its rim came within -0.5 cm of its own body (negative: inside).

Fight Rhythm (?): 7 postures (GuardChange 2, ShieldSettle 1, Peek 3, ShieldPress 1), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 6 times, knocked down 3, launched 0, got up 3; thrown into others 0 times.

Blows: 7. The posed striking part passed within 3 cm of the aim point in 7; aim error mean 0.4 cm, max 0.9 cm (posed); physical weapon's closest pass mean 1.9 cm, max 5.7 cm. 3 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.93 s: SwordCut at A Small's Neck: passed 0.1 cm from the aim point; nearest part A Small's UpperArmR (23.6 cm); the Neck had moved 40.9 cm since the aim was locked
- 4.63 s: SwordBackhand at A Small's Chest: passed 0.2 cm from the aim point; nearest part A Small's Pelvis (4.6 cm); the Chest had moved 19.4 cm since the aim was locked
- 5.60 s: SwordCleave at A Small's Chest: passed 0.2 cm from the aim point; nearest part A Small's Chest (7.9 cm); the Chest had moved 23.7 cm since the aim was locked
- 14.05 s: SwordBackhand at A Medium's KneeR: passed 0.3 cm from the aim point; nearest part A Medium's ShinR (0.0 cm); the KneeR had moved 15.5 cm since the aim was locked
- 39.97 s: SwordThrust at A Large's Chest: passed 0.9 cm from the aim point; nearest part A Large's Chest (4.6 cm); the Chest had moved 40.6 cm since the aim was locked
- 48.50 s: SwordThrust at A Large's Neck: passed 0.9 cm from the aim point; nearest part A Large's Head (29.5 cm); the Neck had moved 46.6 cm since the aim was locked
- 49.97 s: SwordBackhand at A Large's Chest: passed 0.3 cm from the aim point; nearest part A Large's Chest (0.0 cm); the Chest had moved 21.0 cm since the aim was locked


## B Large

Present 0.0–55.7 s. Steps 10, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 11 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 1 / 17 |
| Chest | ? | 0.5 / 6.3 | 1 / 9 |
| Head | ? | 0.7 / 7.4 | 6 / 22 |
| UpperArmL | ? | 0.9 / 11.0 | 15 / 50 |
| ForearmL | ? | 3.8 / 23.7 | 9 / 63 |
| HandL | ? | 5.0 / 45.4 | 22 / 72 |
| UpperArmR | ? | 0.8 / 6.2 | 24 / 59 |
| ForearmR | ? | 1.7 / 13.1 | 7 / 68 |
| HandR | ? | 4.1 / 51.7 | 23 / 59 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 6, 65 damage. Health at the end 0.

Defence: 3 blows came at it (3 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 29% of the time; the worst 24° (FootR).
Ragdoll (on the ground 50.8 s): a joint more than 10° past its limit 86% of the time; the worst 128° (FootR). Fastest part with a standing leg within 0.5 m: 0.2 m/s median, 5.2 m/s at the 95th percentile (975 steps); with none: 0.0 m/s median (2071 steps).

Look: within 25° of what it attended to 99% of the time standing (99% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 2 postures (BoxerSettle 1, BoxerStalk 1), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 1, launched 0, got up 0; thrown into others 1 time.

## Shield contacts

43 times a shield overlapped another unit's body: 95% under 8.0 cm deep (77% under 2 cm; target 95%), the deepest 9.7 cm, the longest 0.07 s (target under 0.2 s). Each one is in events.log ("shield").

## Console warnings and errors

- 55.72 s Exception: MissingReferenceException: The object of type 'PhysicsBattler.Combat.Weapon' has been destroyed but you are still trying to access it. Your script should either check if it is null or you should not destroy the object.
- 55.72 s Exception: MissingReferenceException: The object of type 'PhysicsBattler.Combat.Weapon' has been destroyed but you are still trying to access it. Your script should either check if it is null or you should not destroy the object.

