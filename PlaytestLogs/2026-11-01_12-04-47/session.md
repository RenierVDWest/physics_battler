# Playtest 2026-11-01_12-04-47

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 41.2 s of play (2471 physics steps), 2371 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-01_12-04-47`.

## A Small

Present 0.0–41.2 s. Steps 19, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 7 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 1 / 4 |
| Chest | ? | 0.1 / 0.8 | 1 / 5 |
| Head | ? | 0.2 / 1.5 | 3 / 9 |
| UpperArmL | ? | 0.2 / 2.1 | 1 / 24 |
| ForearmL | ? | 0.4 / 9.7 | 4 / 93 |
| HandL | ? | 1.2 / 25.0 | 11 / 150 |
| UpperArmR | ? | 0.2 / 1.4 | 2 / 64 |
| ForearmR | ? | 0.5 / 9.9 | 4 / 114 |
| HandR | ? | 1.1 / 26.4 | 19 / 118 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 3, 62 damage. Health at the end 0.

Defence: 3 blows came at it (1 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 45% of the time; the worst 42° (FootL).
Ragdoll (on the ground 33.3 s): a joint more than 10° past its limit 100% of the time; the worst 153° (ForearmL). Fastest part with a standing leg within 0.5 m: 1.0 m/s median, 3.3 m/s at the 95th percentile (159 steps); with none: 0.0 m/s median (1839 steps).

Look: within 25° of what it attended to 82% of the time standing (83% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–41.2 s. Steps 63, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 24 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 1.0 | 1 / 100 |
| Chest | ? | 0.3 / 29.3 | 1 / 82 |
| Head | ? | 0.4 / 30.2 | 10 / 105 |
| UpperArmL | ? | 0.6 / 32.4 | 4 / 175 |
| ForearmL | ? | 1.5 / 40.7 | 10 / 178 |
| HandL | ? | 1.8 / 55.3 | 17 / 168 |
| UpperArmR | ? | 0.2 / 7.4 | 1 / 45 |
| ForearmR | ? | 0.4 / 13.7 | 2 / 69 |
| HandR | ? | 0.6 / 22.2 | 8 / 176 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 5 (1 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 53 damage. Hits taken: 17, 84 damage. Health at the end 0.

Defence: 7 blows came at it (7 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 4.

Pose against the joint limits (standing): a joint more than 10° past its limit 43% of the time; the worst 164° (HandR).
Ragdoll (on the ground 5.2 s): a joint more than 10° past its limit 87% of the time; the worst 151° (HandL). Fastest part with a standing leg within 0.5 m: 4.7 m/s median, 14.0 m/s at the 95th percentile (272 steps); with none: 4.1 m/s median (39 steps).

Look: within 25° of what it attended to 87% of the time standing (87% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 100% of the time standing; its rim came within 18.8 cm of its own body (negative: inside).

Fight Rhythm (?): 5 postures (GuardChange 1, HighFeint 2, Peek 2), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 7 times, knocked down 2, launched 0, got up 2; thrown into others 6 times.


## A Large

Present 0.0–41.2 s. Steps 84, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 43 cm from the support point; hips dropped for reach by up to 5 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.5 | 1 / 35 |
| Chest | ? | 0.3 / 16.0 | 1 / 61 |
| Head | ? | 0.4 / 42.2 | 7 / 149 |
| UpperArmL | ? | 0.5 / 42.0 | 3 / 69 |
| ForearmL | ? | 1.7 / 46.1 | 5 / 109 |
| HandL | ? | 3.0 / 67.8 | 14 / 164 |
| UpperArmR | ? | 0.2 / 5.7 | 1 / 29 |
| ForearmR | ? | 0.6 / 19.4 | 2 / 62 |
| HandR | ? | 1.0 / 43.9 | 6 / 131 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 12 (1 on the part aimed at, 3 blocked, 1 on someone in the way, 0 on allies), 114 damage. Hits taken: 14, 146 damage. Health at the end 0.

Defence: 10 blows came at it (5 seen clearly, 5 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 6.

Pose against the joint limits (standing): a joint more than 10° past its limit 62% of the time; the worst 164° (ShinR).

Look: within 25° of what it attended to 92% of the time standing (93% of the time it was within the head's reach, 93% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 5 postures (BoxerSettle 1, ClubFeint 2, BobAndWeave 1, JabFeint 1), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 3 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.5 cm, max 0.9 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.85 s: ClubSmash at B Small's Head: passed 0.0 cm from the aim point; nearest part B Small's UpperArmR (28.1 cm); the Head had moved 46.9 cm since the aim was locked
- 11.93 s: ClubRising at B Small's Belly: passed 0.1 cm from the aim point; nearest part B Small's HandR (9.8 cm); the Belly had moved 10.2 cm since the aim was locked
- 23.28 s: ClubRising at B Medium's KneeL: passed 0.1 cm from the aim point; nearest part B Medium's ThighL (13.0 cm); the KneeL had moved 3.9 cm since the aim was locked
- 33.97 s: ClubRising at B Medium's Chest: passed 0.0 cm from the aim point; nearest part B Medium's Pelvis (40.1 cm); the Chest had moved 50.1 cm since the aim was locked
- 36.88 s: ClubSwing at B Large's Belly: passed 0.1 cm from the aim point; nearest part B Large's Pelvis (10.7 cm); the Belly had moved 50.2 cm since the aim was locked


## B Small

Present 0.0–41.2 s. Steps 57, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 41 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 11.1 | 2 / 169 |
| Chest | ? | 0.3 / 18.3 | 2 / 75 |
| Head | ? | 0.3 / 20.9 | 7 / 146 |
| UpperArmL | ? | 0.4 / 33.8 | 3 / 178 |
| ForearmL | ? | 0.9 / 30.8 | 6 / 177 |
| HandL | ? | 1.3 / 42.5 | 13 / 165 |
| UpperArmR | ? | 0.1 / 2.0 | 1 / 41 |
| ForearmR | ? | 0.4 / 9.3 | 2 / 89 |
| HandR | ? | 0.7 / 21.0 | 13 / 110 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 20 damage. Hits taken: 6, 62 damage. Health at the end 0.

Defence: 2 blows came at it (0 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 70% of the time; the worst 162° (HandR).
Ragdoll (on the ground 20.3 s): a joint more than 10° past its limit 88% of the time; the worst 173° (ForearmR). Fastest part with a standing leg within 0.5 m: 0.7 m/s median, 7.6 m/s at the 95th percentile (816 steps); with none: 0.0 m/s median (402 steps).

Look: within 25° of what it attended to 83% of the time standing (89% of the time it was within the head's reach, 91% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 5 postures (StickSettle 2, AbanicoFlurry 2, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 0 times, knocked down 1, launched 2, got up 2; thrown into others 3 times.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.4 cm, max 0.4 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 9.85 s: ClubSwing at A Large's Belly: passed 0.0 cm from the aim point; nearest part A Large's UpperArmR (72.3 cm); the Belly had moved 98.4 cm since the aim was locked
- 11.27 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ThighL (5.1 cm); the KneeL had moved 9.8 cm since the aim was locked


## B Medium

Present 0.0–41.2 s. Steps 86, idle shuffles 1. Feet slid at most 25.0 cm while planted; the weight was at most 34 cm from the support point; hips dropped for reach by up to 21 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.8 | 1 / 61 |
| Chest | ? | 0.2 / 17.7 | 1 / 54 |
| Head | ? | 0.4 / 16.5 | 7 / 53 |
| UpperArmL | ? | 0.5 / 21.8 | 5 / 177 |
| ForearmL | ? | 1.5 / 49.2 | 8 / 171 |
| HandL | ? | 2.2 / 56.4 | 19 / 178 |
| UpperArmR | ? | 0.2 / 14.5 | 1 / 103 |
| ForearmR | ? | 0.6 / 22.1 | 3 / 145 |
| HandR | ? | 1.1 / 51.2 | 12 / 177 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 7 (2 on the part aimed at, 0 blocked, 1 on someone in the way, 0 on allies), 119 damage. Hits taken: 12, 51 damage. Health at the end 0.

Defence: 6 blows came at it (3 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 59% of the time; the worst 164° (HandR).
Ragdoll (on the ground 3.1 s): a joint more than 10° past its limit 91% of the time; the worst 150° (HandR). Fastest part with a standing leg within 0.5 m: 4.5 m/s median, 11.3 m/s at the 95th percentile (184 steps); with none: 0.7 m/s median (4 steps).

Look: within 25° of what it attended to 87% of the time standing (88% of the time it was within the head's reach, 97% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 98% of the time standing; its rim came within 16.3 cm of its own body (negative: inside).

Fight Rhythm (?): 7 postures (GuardChange 2, ShieldSettle 1, HighFeint 2, Peek 2), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 6 times, knocked down 2, launched 0, got up 2; thrown into others 3 times.

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.0 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.8 cm, max 1.0 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.93 s: SwordCut at A Small's Head: passed 0.0 cm from the aim point; nearest part A Small's UpperArmR (32.3 cm); the Head had moved 48.5 cm since the aim was locked
- 5.82 s: SwordCut at A Small's Head: passed 0.0 cm from the aim point; nearest part A Small's Head (13.3 cm); the Head had moved 14.7 cm since the aim was locked
- 7.87 s: SwordBackhand at A Small's Belly: passed 0.1 cm from the aim point; nearest part A Small's Pelvis (0.6 cm); the Belly had moved 3.1 cm since the aim was locked
- 33.38 s: SwordCut at A Large's Chest: passed 0.0 cm from the aim point; nearest part A Large's Chest (11.5 cm); the Chest had moved 44.8 cm since the aim was locked
- 38.48 s: SwordCut at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (0.0 cm); the KneeL had moved 18.5 cm since the aim was locked


## B Large

Present 0.0–41.2 s. Steps 81, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 33 cm from the support point; hips dropped for reach by up to 5 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 1 / 26 |
| Chest | ? | 0.2 / 11.0 | 1 / 19 |
| Head | ? | 0.4 / 13.0 | 5 / 75 |
| UpperArmL | ? | 0.5 / 17.7 | 2 / 61 |
| ForearmL | ? | 1.7 / 36.8 | 5 / 76 |
| HandL | ? | 2.9 / 33.3 | 14 / 100 |
| UpperArmR | ? | 0.3 / 5.9 | 1 / 123 |
| ForearmR | ? | 1.0 / 53.0 | 3 / 130 |
| HandR | ? | 1.5 / 43.9 | 9 / 177 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 12 (1 on the part aimed at, 6 blocked, 2 on someone in the way, 0 on allies), 54 damage. Hits taken: 10, 128 damage. Health at the end 0.

Defence: 11 blows came at it (6 seen clearly, 5 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 8.

Pose against the joint limits (standing): a joint more than 10° past its limit 57% of the time; the worst 164° (HandR).

Look: within 25° of what it attended to 80% of the time standing (82% of the time it was within the head's reach, 97% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 9 postures (BoxerSettle 1, BoxerStalk 1, ClubFeint 3, BobAndWeave 1, JabFeint 2, Shift 1), 2 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 5.03 s: ClubSwing at A Medium's Belly: passed 0.1 cm from the aim point; nearest part A Medium's Chest (0.0 cm); the Belly had moved 35.2 cm since the aim was locked
- 12.22 s: ClubSwing at A Medium's Belly: passed 0.1 cm from the aim point; nearest part A Medium's Spine (6.9 cm); the Belly had moved 33.2 cm since the aim was locked
- 16.48 s: ClubSwing at A Large's Head: passed 0.1 cm from the aim point; nearest part A Large's Head (0.0 cm); the Head had moved 21.9 cm since the aim was locked

