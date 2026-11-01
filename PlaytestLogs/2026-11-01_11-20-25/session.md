# Playtest 2026-11-01_11-20-25

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 74.3 s of play (4458 physics steps), 2016 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-01_11-20-25`.

## A Small

Present 0.0–74.3 s. Steps 30, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 9 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 1 / 19 |
| Chest | ? | 0.1 / 3.8 | 1 / 9 |
| Head | ? | 0.2 / 2.5 | 3 / 23 |
| UpperArmL | ? | 0.1 / 1.2 | 1 / 24 |
| ForearmL | ? | 0.3 / 9.5 | 2 / 91 |
| HandL | ? | 0.6 / 24.5 | 5 / 71 |
| UpperArmR | ? | 0.1 / 0.9 | 1 / 85 |
| ForearmR | ? | 0.4 / 10.0 | 2 / 119 |
| HandR | ? | 0.7 / 27.3 | 10 / 133 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (0 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 2, 42 damage. Health at the end 0.

Defence: 3 blows came at it (2 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Look: within 25° of what it attended to 82% of the time standing (82% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–74.3 s. Steps 164, idle shuffles 5. Feet slid at most 0.0 cm while planted; the weight was at most 50 cm from the support point; hips dropped for reach by up to 21 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 4.8 | 1 / 94 |
| Chest | ? | 0.3 / 34.8 | 1 / 83 |
| Head | ? | 0.3 / 32.6 | 7 / 170 |
| UpperArmL | ? | 0.4 / 59.6 | 3 / 179 |
| ForearmL | ? | 1.1 / 51.7 | 4 / 180 |
| HandL | ? | 1.4 / 59.3 | 15 / 179 |
| UpperArmR | ? | 0.1 / 4.4 | 1 / 136 |
| ForearmR | ? | 0.4 / 30.9 | 2 / 117 |
| HandR | ? | 0.6 / 22.6 | 7 / 178 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 19 (2 on the part aimed at, 10 blocked, 2 on someone in the way, 4 on allies), 134 damage. Hits taken: 12, 48 damage. Health at the end 0.

Defence: 9 blows came at it (7 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 4.

Look: within 25° of what it attended to 87% of the time standing (88% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 97% of the time standing; its rim came within 1.2 cm of its own body (negative: inside).

Fight Rhythm (?): 24 postures (GuardChange 2, HighFeint 7, Peek 6, ShieldAdvance 6, ShieldPress 2, ShieldSettle 1), 3 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 2 times, knocked down 1, launched 0, got up 1; thrown into others 0 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.33 s: SwordBackhand at B Large's ElbowR: passed 0.0 cm from the aim point; nearest part B Large's Chest (2.2 cm); the ElbowR had moved 12.6 cm since the aim was locked


## A Large

Present 0.0–74.3 s. Steps 148, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 63 cm from the support point; hips dropped for reach by up to 26 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.9 | 1 / 36 |
| Chest | ? | 0.3 / 17.1 | 1 / 43 |
| Head | ? | 0.4 / 17.3 | 6 / 151 |
| UpperArmL | ? | 0.5 / 27.6 | 3 / 138 |
| ForearmL | ? | 2.2 / 46.3 | 7 / 176 |
| HandL | ? | 3.5 / 42.6 | 15 / 152 |
| UpperArmR | ? | 0.3 / 7.6 | 1 / 78 |
| ForearmR | ? | 0.8 / 25.9 | 3 / 93 |
| HandR | ? | 1.4 / 45.1 | 9 / 134 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 15 (3 on the part aimed at, 5 blocked, 1 on someone in the way, 1 on allies), 167 damage. Hits taken: 6, 64 damage. Health at the end 0.

Defence: 4 blows came at it (3 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 2.

Look: within 25° of what it attended to 91% of the time standing (91% of the time it was within the head's reach, 96% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 21 postures (BoxerSettle 2, ClubFeint 6, BobAndWeave 3, JabFeint 5, BoxerStalk 5), 5 of them lead-ins to blows; energy at the end 0.00.

Blows: 10. The posed striking part passed within 3 cm of the aim point in 10; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.5 cm, max 1.4 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.88 s: ClubSwing at B Small's Head: passed 0.1 cm from the aim point; nearest part B Small's UpperArmR (33.1 cm); the Head had moved 48.0 cm since the aim was locked
- 6.50 s: ClubSmash at B Small's Chest: passed 0.0 cm from the aim point; nearest part B Small's Pelvis (1.0 cm); the Chest had moved 45.3 cm since the aim was locked
- 8.32 s: ClubRising at B Large's KneeL: passed 0.1 cm from the aim point; nearest part B Large's ShinL (9.8 cm); the KneeL had moved 12.1 cm since the aim was locked
- 11.62 s: ClubRising at B Small's Chest: passed 0.1 cm from the aim point; nearest part B Small's UpperArmR (4.1 cm); the Chest had moved 19.4 cm since the aim was locked
- 27.32 s: ClubSwing at B Small's Belly: passed 0.1 cm from the aim point; nearest part B Small's ForearmR (17.7 cm); the Belly had moved 33.7 cm since the aim was locked
- 32.38 s: ClubSmash at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's Head (10.8 cm); the Neck had moved 50.8 cm since the aim was locked
- 49.33 s: ClubSwing at B Medium's ElbowR: passed 0.1 cm from the aim point; nearest part B Medium's Head (62.5 cm); the ElbowR had moved 93.5 cm since the aim was locked
- 56.92 s: ClubSmash at B Medium's Neck: passed 0.0 cm from the aim point; nearest part B Medium's Chest (4.4 cm); the Neck had moved 24.6 cm since the aim was locked
- 61.07 s: ClubRising at B Medium's Belly: passed 0.1 cm from the aim point; nearest part B Medium's Spine (6.0 cm); the Belly had moved 25.3 cm since the aim was locked
- 66.43 s: ClubRising at B Medium's Belly: passed 0.1 cm from the aim point; nearest part B Medium's Chest (12.6 cm); the Belly had moved 28.9 cm since the aim was locked


## B Small

Present 0.0–74.3 s. Steps 80, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 19 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 1 / 29 |
| Chest | ? | 0.1 / 5.7 | 1 / 33 |
| Head | ? | 0.2 / 5.7 | 3 / 38 |
| UpperArmL | ? | 0.2 / 8.4 | 2 / 57 |
| ForearmL | ? | 0.8 / 21.4 | 7 / 177 |
| HandL | ? | 2.0 / 29.1 | 12 / 175 |
| UpperArmR | ? | 0.1 / 2.4 | 1 / 56 |
| ForearmR | ? | 0.4 / 9.3 | 2 / 89 |
| HandR | ? | 0.6 / 21.2 | 11 / 102 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 7 damage. Hits taken: 4, 83 damage. Health at the end 0.

Defence: 5 blows came at it (3 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 5.

Look: within 25° of what it attended to 80% of the time standing (84% of the time it was within the head's reach, 93% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 7 postures (StickSettle 2, AbanicoFlurry 2, RedondaTwirl 2, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 1, launched 2, got up 3; thrown into others 0 times.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.6 cm, max 0.6 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.93 s: ClubRising at A Large's Belly: passed 0.0 cm from the aim point; nearest part A Large's HandR (14.8 cm); the Belly had moved 25.2 cm since the aim was locked
- 9.88 s: ClubSwing at A Large's KneeR: passed 0.0 cm from the aim point; nearest part A Large's ShinR (38.8 cm); the KneeR had moved 57.9 cm since the aim was locked


## B Medium

Present 0.0–74.3 s. Steps 138, idle shuffles 5. Feet slid at most 24.1 cm while planted; the weight was at most 32 cm from the support point; hips dropped for reach by up to 21 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 4.6 | 1 / 71 |
| Chest | ? | 0.3 / 28.6 | 1 / 72 |
| Head | ? | 0.3 / 25.7 | 9 / 176 |
| UpperArmL | ? | 0.4 / 38.6 | 3 / 180 |
| ForearmL | ? | 1.3 / 44.9 | 5 / 178 |
| HandL | ? | 1.6 / 46.7 | 19 / 180 |
| UpperArmR | ? | 0.2 / 6.4 | 1 / 48 |
| ForearmR | ? | 0.3 / 9.3 | 1 / 76 |
| HandR | ? | 0.5 / 23.0 | 5 / 130 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 12 (1 on the part aimed at, 7 blocked, 2 on someone in the way, 0 on allies), 91 damage. Hits taken: 19, 103 damage. Health at the end 0.

Defence: 28 blows came at it (15 seen clearly, 13 at the edge of its view, 0 never seen); it raised the shield against 19.

Look: within 25° of what it attended to 90% of the time standing (90% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 98% of the time standing; its rim came within 0.2 cm of its own body (negative: inside).

Fight Rhythm (?): 5 postures (GuardChange 1, ShieldPress 1, Peek 2, HighFeint 1), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 6 times, knocked down 2, launched 0, got up 2; thrown into others 0 times.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.3 cm, max 0.6 cm (posed); physical weapon's closest pass mean 0.6 cm, max 0.6 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.90 s: SwordThrust at A Small's Head: passed 0.6 cm from the aim point; nearest part A Small's Head (12.8 cm); the Head had moved 22.2 cm since the aim was locked
- 7.55 s: SwordCleave at A Small's Belly: passed 0.0 cm from the aim point; nearest part A Small's Spine (0.0 cm); the Belly had moved 16.6 cm since the aim was locked


## B Large

Present 0.0–74.3 s. Steps 37, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 12 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 0 / 5 |
| Chest | ? | 0.2 / 2.2 | 1 / 19 |
| Head | ? | 0.4 / 5.6 | 6 / 66 |
| UpperArmL | ? | 0.6 / 14.8 | 6 / 128 |
| ForearmL | ? | 3.3 / 41.9 | 14 / 179 |
| HandL | ? | 4.4 / 41.4 | 18 / 166 |
| UpperArmR | ? | 0.4 / 10.2 | 4 / 93 |
| ForearmR | ? | 0.2 / 12.9 | 1 / 64 |
| HandR | ? | 0.3 / 45.1 | 2 / 75 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 5, 59 damage. Health at the end 0.

Defence: 2 blows came at it (1 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 2.

Look: within 25° of what it attended to 89% of the time standing (89% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 11 postures (BoxerSettle 1, BoxerStalk 4, BobAndWeave 2, JabFeint 2, Shift 2), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

