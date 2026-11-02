# Playtest 2026-11-02_20-01-34

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 31.5 s of play (1893 physics steps), 2228 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-02_20-01-34`.

## A Small

Present 0.0–31.5 s. Steps 24, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 11 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.1 / 16.4 | 8 / 113 |
| Chest | ? | 1.4 / 18.5 | 10 / 122 |
| Head | ? | 2.0 / 24.4 | 14 / 87 |
| UpperArmL | ? | 2.3 / 37.1 | 13 / 146 |
| ForearmL | ? | 2.6 / 22.8 | 13 / 149 |
| HandL | ? | 3.1 / 36.4 | 22 / 98 |
| UpperArmR | ? | 1.5 / 5.9 | 8 / 49 |
| ForearmR | ? | 2.6 / 19.5 | 15 / 78 |
| HandR | ? | 6.7 / 32.3 | 19 / 99 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 10 damage. Hits taken: 4, 62 damage. Health at the end 0.

Defence: 5 blows came at it (2 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 57% of the time; the worst 42° (FootL).
Ragdoll (on the ground 23.7 s): a joint more than 10° past its limit 63% of the time; the worst 88° (ShinL). Fastest part with a standing leg within 0.5 m: 0.0 m/s median, 1.4 m/s at the 95th percentile (374 steps); with none: 0.0 m/s median (1046 steps).

Look: within 25° of what it attended to 73% of the time standing (76% of the time it was within the head's reach, 95% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.63 s: ClubSwing at B Medium's Neck: passed 0.0 cm from the aim point; nearest part B Medium's Head (8.4 cm); the Neck had moved 11.1 cm since the aim was locked


## A Medium

Present 0.0–31.5 s. Steps 48, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 44 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 3 / 49 |
| Chest | ? | 1.0 / 14.2 | 5 / 59 |
| Head | ? | 1.5 / 21.9 | 18 / 111 |
| UpperArmL | ? | 1.9 / 37.5 | 8 / 93 |
| ForearmL | ? | 3.4 / 46.3 | 9 / 153 |
| HandL | ? | 4.5 / 50.3 | 10 / 89 |
| UpperArmR | ? | 1.5 / 14.6 | 5 / 35 |
| ForearmR | ? | 2.9 / 14.7 | 8 / 62 |
| HandR | ? | 4.1 / 28.1 | 10 / 81 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 150 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (0 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 12 damage. Hits taken: 13, 105 damage. Health at the end 0.

Defence: 5 blows came at it (4 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 40% of the time; the worst 86° (ShinR).
Ragdoll (on the ground 5.8 s): a joint more than 10° past its limit 79% of the time; the worst 178° (ForearmL). Fastest part with a standing leg within 0.5 m: 4.1 m/s median, 11.4 m/s at the 95th percentile (153 steps); with none: 3.2 m/s median (196 steps).

Look: within 25° of what it attended to 80% of the time standing (81% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 90% of the time standing; its rim came within 0.6 cm of its own body (negative: inside).

Fight Rhythm (?): 2 postures (GuardChange 1, Peek 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 5 times, knocked down 3, launched 0, got up 3; thrown into others 3 times.


## A Large

Present 0.0–31.5 s. Steps 48, idle shuffles 1. Feet slid at most 17.0 cm while planted; the weight was at most 89 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 31.6 | 0 / 0 |
| Spine | ? | 0.0 / 31.7 | 2 / 37 |
| Chest | ? | 0.7 / 40.9 | 2 / 26 |
| Head | ? | 1.1 / 39.5 | 10 / 71 |
| UpperArmL | ? | 1.2 / 45.8 | 4 / 48 |
| ForearmL | ? | 2.7 / 41.3 | 7 / 77 |
| HandL | ? | 4.3 / 63.8 | 17 / 150 |
| UpperArmR | ? | 1.0 / 34.3 | 2 / 22 |
| ForearmR | ? | 1.8 / 33.9 | 4 / 64 |
| HandR | ? | 2.9 / 52.0 | 5 / 79 |
| ThighL | ? | 0.0 / 31.2 | 0 / 146 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 32.1 | 0 / 0 |
| ShinR | ? | 0.0 / 22.0 | 0 / 0 |
| FootR | ? | 0.0 / 19.4 | 0 / 0 |

Hits dealt: 2 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 32 damage. Hits taken: 14, 174 damage. Health at the end 0.

Defence: 17 blows came at it (10 seen clearly, 7 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 14.

Pose against the joint limits (standing): a joint more than 10° past its limit 26% of the time; the worst 134° (ShinL).

Look: within 25° of what it attended to 87% of the time standing (85% of the time it was within the head's reach, 89% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 2 postures (BoxerSettle 1, BobAndWeave 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.5 cm, max 2.1 cm (posed); physical weapon's closest pass mean 0.9 cm, max 2.2 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.87 s: ClubSwing at B Small's ElbowR: passed 0.0 cm from the aim point; nearest part B Small's UpperArmL (63.8 cm); the ElbowR had moved 108.0 cm since the aim was locked
- 6.23 s: ClubSwing at B Small's Head: passed 0.1 cm from the aim point; nearest part B Small's Chest (26.5 cm); the Head had moved 33.6 cm since the aim was locked
- 8.33 s: ClubBackhand at B Small's ElbowR: passed 0.0 cm from the aim point; nearest part B Small's UpperArmR (49.8 cm); the ElbowR had moved 56.6 cm since the aim was locked
- 9.60 s: OffJab at B Small's Chest: passed 2.1 cm from the aim point; nearest part B Small's UpperArmL (21.8 cm); the Chest had moved 36.7 cm since the aim was locked
- 16.00 s: ClubSwing at B Small's Chest: passed 0.0 cm from the aim point; nearest part B Small's Head (22.4 cm); the Chest had moved 42.6 cm since the aim was locked


## B Small

Present 0.0–31.5 s. Steps 94, idle shuffles 0. Feet slid at most 5.0 cm while planted; the weight was at most 24 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.1 / 20.5 | 9 / 180 |
| Chest | ? | 1.4 / 25.1 | 9 / 178 |
| Head | ? | 2.1 / 46.5 | 19 / 111 |
| UpperArmL | ? | 2.2 / 36.9 | 13 / 176 |
| ForearmL | ? | 3.4 / 33.4 | 15 / 178 |
| HandL | ? | 3.7 / 36.3 | 29 / 122 |
| UpperArmR | ? | 2.1 / 52.2 | 8 / 178 |
| ForearmR | ? | 3.4 / 69.6 | 13 / 174 |
| HandR | ? | 6.0 / 74.0 | 16 / 180 |
| ThighL | ? | 0.0 / 0.0 | 0 / 148 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 2 |
| ThighR | ? | 0.0 / 0.0 | 0 / 144 |
| ShinR | ? | 0.0 / 0.0 | 0 / 4 |
| FootR | ? | 0.0 / 0.0 | 0 / 1 |

Hits dealt: 6 (3 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 54 damage. Hits taken: 5, 45 damage. Health at the end 0.

Defence: 6 blows came at it (1 seen clearly, 5 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 5.

Pose against the joint limits (standing): a joint more than 10° past its limit 55% of the time; the worst 151° (ShinR).
Ragdoll (on the ground 2.1 s): a joint more than 10° past its limit 59% of the time; the worst 138° (ForearmL). Fastest part with a standing leg within 0.5 m: 3.0 m/s median, 5.5 m/s at the 95th percentile (107 steps); with none: 0.1 m/s median (21 steps).

Look: within 25° of what it attended to 72% of the time standing (81% of the time it was within the head's reach, 85% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 11 postures (StickSettle 2, AbanicoFlurry 4, RedondaTwirl 3, BaitRetreat 2), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 0 times, knocked down 2, launched 0, got up 2; thrown into others 2 times.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 3.1 cm, max 12.0 cm (posed); physical weapon's closest pass mean 7.6 cm, max 15.1 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 10.72 s: ClubBackhand at A Large's KneeR: passed 0.1 cm from the aim point; nearest part A Large's ThighR (10.2 cm); the KneeR had moved 24.1 cm since the aim was locked
- 12.45 s: ClubSwing at A Large's ElbowL: passed 0.0 cm from the aim point; nearest part A Large's UpperArmL (16.9 cm); the ElbowL had moved 26.4 cm since the aim was locked
- 21.47 s: ClubBackhand at A Large's KneeR: passed 0.1 cm from the aim point; nearest part A Large's ThighR (17.3 cm); the KneeR had moved 13.8 cm since the aim was locked
- 25.00 s: ClubBackhand at A Large's ElbowL: passed 12.0 cm from the aim point; nearest part A Large's UpperArmL (1.2 cm); the ElbowL had moved 19.2 cm since the aim was locked


## B Medium

Present 0.0–31.5 s. Steps 68, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 41 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.5 | 3 / 74 |
| Chest | ? | 0.9 / 15.6 | 5 / 87 |
| Head | ? | 1.4 / 29.0 | 15 / 70 |
| UpperArmL | ? | 1.8 / 43.4 | 12 / 178 |
| ForearmL | ? | 4.2 / 53.2 | 12 / 174 |
| HandL | ? | 3.8 / 58.8 | 8 / 139 |
| UpperArmR | ? | 1.5 / 23.7 | 5 / 62 |
| ForearmR | ? | 2.9 / 38.3 | 8 / 99 |
| HandR | ? | 4.6 / 55.9 | 10 / 121 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 1 |
| ThighR | ? | 0.0 / 0.0 | 0 / 149 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 2 |

Hits dealt: 8 (2 on the part aimed at, 1 blocked, 0 on someone in the way, 1 on allies), 127 damage. Hits taken: 6, 56 damage. Health at the end 0.

Defence: 1 blows came at it (0 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 35% of the time; the worst 98° (ShinR).
Ragdoll (on the ground 2.3 s): a joint more than 10° past its limit 77% of the time; the worst 174° (ForearmL). Fastest part with a standing leg within 0.5 m: 6.2 m/s median, 10.7 m/s at the 95th percentile (23 steps); with none: 3.9 m/s median (116 steps).

Look: within 25° of what it attended to 88% of the time standing (89% of the time it was within the head's reach, 96% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 90% of the time standing; its rim came within 1.1 cm of its own body (negative: inside).

Fight Rhythm (?): 6 postures (GuardChange 1, ShieldPress 1, Peek 2, ShieldSettle 1, HighFeint 1), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 2 times, knocked down 2, launched 0, got up 2; thrown into others 2 times.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 1.5 cm, max 5.3 cm (posed); physical weapon's closest pass mean 0.4 cm, max 0.4 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.98 s: SwordCut at A Small's KneeL: passed 0.1 cm from the aim point; nearest part A Small's ThighL (0.0 cm); the KneeL had moved 7.8 cm since the aim was locked
- 5.72 s: SwordBackhand at A Small's Chest: passed 0.4 cm from the aim point; nearest part A Small's ForearmL (9.3 cm); the Chest had moved 17.3 cm since the aim was locked
- 6.45 s: SwordCleave at A Small's Chest: passed 0.4 cm from the aim point; nearest part A Small's Head (6.8 cm); the Chest had moved 25.4 cm since the aim was locked
- 7.97 s: SwordThrust at A Small's Belly: passed 5.3 cm from the aim point; nearest part A Small's Chest (0.0 cm); the Belly had moved 11.6 cm since the aim was locked


## B Large

Present 0.0–31.5 s. Steps 60, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 54 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 2 / 19 |
| Chest | ? | 0.8 / 7.4 | 3 / 30 |
| Head | ? | 1.3 / 12.3 | 14 / 97 |
| UpperArmL | ? | 1.5 / 11.2 | 6 / 44 |
| ForearmL | ? | 4.1 / 24.1 | 10 / 58 |
| HandL | ? | 5.5 / 45.0 | 23 / 173 |
| UpperArmR | ? | 1.3 / 25.7 | 3 / 40 |
| ForearmR | ? | 2.4 / 38.8 | 4 / 75 |
| HandR | ? | 3.7 / 57.6 | 6 / 98 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 150 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 12 (3 on the part aimed at, 2 blocked, 1 on someone in the way, 2 on allies), 137 damage. Hits taken: 3, 31 damage. Health at the end 0.

Defence: 4 blows came at it (3 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 4.

Pose against the joint limits (standing): a joint more than 10° past its limit 36% of the time; the worst 89° (ShinR).

Look: within 25° of what it attended to 83% of the time standing (82% of the time it was within the head's reach, 97% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 7 postures (BoxerSettle 1, BoxerStalk 1, ClubFeint 2, JabFeint 1, BobAndWeave 1, Shift 1), 2 of them lead-ins to blows; energy at the end 0.00.

Blows: 6. The posed striking part passed within 3 cm of the aim point in 6; aim error mean 0.1 cm, max 0.3 cm (posed); physical weapon's closest pass mean 2.6 cm, max 9.6 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.12 s: OffJab at A Medium's Head: passed 0.3 cm from the aim point; nearest part A Medium's UpperArmR (21.9 cm); the Head had moved 59.8 cm since the aim was locked
- 5.98 s: ClubSwing at A Medium's Belly: passed 0.1 cm from the aim point; nearest part A Medium's ForearmL (42.8 cm); the Belly had moved 42.8 cm since the aim was locked
- 8.28 s: ClubSwing at A Small's Head: passed 0.1 cm from the aim point; nearest part B Medium's HandL (27.9 cm); the Head had moved 58.8 cm since the aim was locked
- 15.52 s: ClubRising at A Medium's KneeL: passed 0.1 cm from the aim point; nearest part A Medium's ThighL (7.3 cm); the KneeL had moved 13.3 cm since the aim was locked
- 22.25 s: ClubSwing at A Large's Neck: passed 0.0 cm from the aim point; nearest part A Large's Head (5.8 cm); the Neck had moved 18.2 cm since the aim was locked
- 24.65 s: ClubBackhand at A Large's KneeR: passed 0.0 cm from the aim point; nearest part A Large's ThighR (5.6 cm); the KneeR had moved 12.2 cm since the aim was locked

## Shield contacts

186 times a shield overlapped another unit's body: 95% under 3.9 cm deep (89% under 2 cm; target 95%), the deepest 12.7 cm, the longest 0.22 s (target under 0.2 s). Each one is in events.log ("shield").

