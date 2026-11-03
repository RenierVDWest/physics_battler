# Playtest 2026-11-03_06-45-34

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 76.1 s of play (4568 physics steps), 3648 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-03_06-45-34`.

## A Small

Present 0.0–76.1 s. Steps 99, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 34 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.9 | 4 / 66 |
| Chest | ? | 0.8 / 12.1 | 8 / 180 |
| Head | ? | 1.4 / 23.5 | 11 / 73 |
| UpperArmL | ? | 1.4 / 38.2 | 7 / 161 |
| ForearmL | ? | 1.6 / 28.7 | 9 / 160 |
| HandL | ? | 2.4 / 36.4 | 21 / 163 |
| UpperArmR | ? | 1.5 / 29.2 | 8 / 165 |
| ForearmR | ? | 2.9 / 52.5 | 13 / 137 |
| HandR | ? | 5.7 / 51.8 | 16 / 134 |
| ThighL | ? | 0.0 / 0.0 | 0 / 5 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 3 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (0 on the part aimed at, 2 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 6, 78 damage. Health at the end 0.

Defence: 7 blows came at it (5 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, set its weapon across 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 66% of the time; the worst 50° (FootL).
Ragdoll (on the ground 43.3 s): a joint more than 10° past its limit 99% of the time; the worst 173° (ShinR). Fastest part with a standing leg within 0.5 m: 1.0 m/s median, 7.6 m/s at the 95th percentile (83 steps); with none: 0.0 m/s median (2514 steps).

Look: within 25° of what it attended to 86% of the time standing (88% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 15 postures (StickSettle 5, AbanicoFlurry 6, RedondaTwirl 4), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 4 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.7 cm, max 1.4 cm (posed); physical weapon's closest pass mean 6.1 cm, max 11.5 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.58 s: ClubFanBackhand at B Medium's ElbowR: passed 1.4 cm from the aim point; nearest part B Medium's UpperArmR (18.9 cm); the ElbowR had moved 29.1 cm since the aim was locked
- 26.75 s: ClubSmash at B Medium's Chest: passed 0.1 cm from the aim point; nearest part B Medium's ForearmL (8.4 cm); the Chest had moved 41.4 cm since the aim was locked


## A Medium

Present 0.0–76.1 s. Steps 131, idle shuffles 2. Feet slid at most 8.0 cm while planted; the weight was at most 67 cm from the support point; hips dropped for reach by up to 21 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 24.3 | 0 / 0 |
| Spine | ? | 0.1 / 24.3 | 4 / 177 |
| Chest | ? | 1.0 / 38.8 | 5 / 162 |
| Head | ? | 1.4 / 51.8 | 11 / 160 |
| UpperArmL | ? | 1.7 / 62.9 | 6 / 176 |
| ForearmL | ? | 2.1 / 80.1 | 6 / 180 |
| HandL | ? | 2.7 / 83.7 | 6 / 166 |
| UpperArmR | ? | 1.5 / 59.7 | 6 / 143 |
| ForearmR | ? | 3.0 / 57.9 | 8 / 177 |
| HandR | ? | 4.5 / 82.0 | 10 / 177 |
| ThighL | ? | 0.0 / 24.0 | 0 / 31 |
| ShinL | ? | 0.0 / 3.2 | 0 / 0 |
| FootL | ? | 0.0 / 2.4 | 0 / 0 |
| ThighR | ? | 0.0 / 24.6 | 0 / 146 |
| ShinR | ? | 0.0 / 12.6 | 0 / 0 |
| FootR | ? | 0.0 / 9.7 | 0 / 0 |

Hits dealt: 5 (2 on the part aimed at, 0 blocked, 1 parried, 0 on someone in the way, 0 on allies), 41 damage. Hits taken: 22, 123 damage. Health at the end 0.

Defence: 15 blows came at it (8 seen clearly, 7 at the edge of its view, 0 never seen); it raised the shield against 8, set its weapon across 0.

Pose against the joint limits (standing): a joint more than 10° past its limit 40% of the time; the worst 119° (ThighR).
Ragdoll (on the ground 15.3 s): a joint more than 10° past its limit 95% of the time; the worst 175° (ForearmL). Fastest part with a standing leg within 0.5 m: 6.7 m/s median, 17.0 m/s at the 95th percentile (187 steps); with none: 0.1 m/s median (728 steps).

Look: within 25° of what it attended to 85% of the time standing (87% of the time it was within the head's reach, 97% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 90% of the time standing; its rim came within 1.0 cm of its own body (negative: inside).

Fight Rhythm (?): 5 postures (GuardChange 1, HighFeint 2, Peek 2), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 9 times, knocked down 3, launched 0, got up 3; thrown into others 7 times.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 4.9 cm, max 9.2 cm (posed); physical weapon's closest pass mean 4.3 cm, max 7.4 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 14.17 s: SwordBackhand at B Large's ElbowR: passed 0.1 cm from the aim point; nearest part B Large's Spine (0.0 cm); the ElbowR had moved 14.3 cm since the aim was locked
- 22.02 s: SwordCut at B Large's Head: passed 9.2 cm from the aim point; nearest part B Large's UpperArmR (0.0 cm); the Head had moved 71.0 cm since the aim was locked
- 30.78 s: SwordBackhand at B Large's Neck: passed 4.9 cm from the aim point; nearest part B Large's HandR (35.1 cm); the Neck had moved 51.6 cm since the aim was locked
- 47.07 s: SwordCleave at B Large's Belly: passed 5.5 cm from the aim point; nearest part B Large's ForearmL (115.5 cm); the Belly had moved 59.0 cm since the aim was locked


## A Large

Present 0.0–76.1 s. Steps 153, idle shuffles 1. Feet slid at most 77.0 cm while planted; the weight was at most 85 cm from the support point; hips dropped for reach by up to 26 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 26.3 | 0 / 0 |
| Spine | ? | 0.0 / 26.3 | 2 / 106 |
| Chest | ? | 1.0 / 35.6 | 3 / 137 |
| Head | ? | 1.5 / 68.1 | 14 / 171 |
| UpperArmL | ? | 2.0 / 103.0 | 7 / 178 |
| ForearmL | ? | 4.2 / 78.1 | 15 / 179 |
| HandL | ? | 6.3 / 84.0 | 26 / 180 |
| UpperArmR | ? | 1.1 / 28.9 | 2 / 40 |
| ForearmR | ? | 2.1 / 40.6 | 4 / 66 |
| HandR | ? | 3.0 / 55.4 | 5 / 90 |
| ThighL | ? | 0.0 / 25.8 | 0 / 150 |
| ShinL | ? | 0.0 / 5.5 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 26.7 | 0 / 150 |
| ShinR | ? | 0.0 / 18.6 | 0 / 0 |
| FootR | ? | 0.0 / 16.6 | 0 / 0 |

Hits dealt: 21 (2 on the part aimed at, 2 blocked, 4 parried, 3 on someone in the way, 3 on allies), 154 damage. Hits taken: 7, 80 damage. Health at the end 0.

Defence: 3 blows came at it (0 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 0, set its weapon across 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 32% of the time; the worst 136° (ShinR).
Ragdoll (on the ground 4.5 s): a joint more than 10° past its limit 92% of the time; the worst 62° (Head). Fastest part with a standing leg within 0.5 m: 0.2 m/s median, 4.2 m/s at the 95th percentile (256 steps); with none: 1.3 m/s median (11 steps).

Look: within 25° of what it attended to 86% of the time standing (87% of the time it was within the head's reach, 93% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 13 postures (BoxerSettle 3, ClubFeint 4, JabFeint 2, BoxerStalk 2, BobAndWeave 2), 3 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 8. The posed striking part passed within 3 cm of the aim point in 8; aim error mean 0.3 cm, max 1.8 cm (posed); physical weapon's closest pass mean 1.1 cm, max 2.4 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.00 s: ClubSwing at B Small's Neck: passed 0.0 cm from the aim point; nearest part B Small's UpperArmR (16.0 cm); the Neck had moved 31.5 cm since the aim was locked
- 10.03 s: ClubSwing at B Small's Chest: passed 0.0 cm from the aim point; nearest part B Small's ForearmL (17.4 cm); the Chest had moved 48.4 cm since the aim was locked
- 12.13 s: ClubRising at B Large's Belly: passed 0.0 cm from the aim point; nearest part B Large's Pelvis (26.0 cm); the Belly had moved 68.2 cm since the aim was locked
- 16.27 s: ClubRising at B Small's Chest: passed 0.1 cm from the aim point; nearest part B Small's Chest (15.2 cm); the Chest had moved 20.9 cm since the aim was locked
- 22.05 s: ClubRising at B Small's Belly: passed 1.8 cm from the aim point; nearest part A Medium's ThighR (3.7 cm); the Belly had moved 57.4 cm since the aim was locked
- 40.02 s: ClubSwing at B Large's Head: passed 0.6 cm from the aim point; nearest part B Large's HandL (4.1 cm); the Head had moved 24.8 cm since the aim was locked
- 43.27 s: OffJab at B Large's Head: passed 0.0 cm from the aim point; nearest part B Large's Head (27.2 cm); the Head had moved 32.1 cm since the aim was locked
- 54.12 s: ClubSwing at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ThighL (66.0 cm); the KneeL had moved 80.1 cm since the aim was locked


## B Small

Present 0.0–76.1 s. Steps 49, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 10 cm from the support point; hips dropped for reach by up to 10 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 4 / 32 |
| Chest | ? | 0.8 / 5.9 | 7 / 45 |
| Head | ? | 1.2 / 9.3 | 8 / 52 |
| UpperArmL | ? | 1.2 / 12.0 | 7 / 133 |
| ForearmL | ? | 1.7 / 31.0 | 8 / 155 |
| HandL | ? | 2.2 / 36.3 | 20 / 104 |
| UpperArmR | ? | 1.3 / 11.6 | 8 / 64 |
| ForearmR | ? | 3.1 / 21.2 | 13 / 69 |
| HandR | ? | 5.3 / 27.5 | 17 / 91 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 2 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 2 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 10, 63 damage. Health at the end 0.

Defence: 6 blows came at it (4 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, set its weapon across 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 56% of the time; the worst 47° (FootL).
Ragdoll (on the ground 56.2 s): a joint more than 10° past its limit 66% of the time; the worst 160° (ForearmL). Fastest part with a standing leg within 0.5 m: 0.1 m/s median, 6.0 m/s at the 95th percentile (1075 steps); with none: 0.0 m/s median (2295 steps).

Look: within 25° of what it attended to 82% of the time standing (85% of the time it was within the head's reach, 94% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 3 times, knocked down 2, launched 1, got up 2; thrown into others 3 times.


## B Medium

Present 0.0–76.1 s. Steps 164, idle shuffles 3. Feet slid at most 12.5 cm while planted; the weight was at most 75 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 24.1 | 0 / 0 |
| Spine | ? | 0.0 / 24.1 | 2 / 145 |
| Chest | ? | 0.6 / 34.4 | 3 / 133 |
| Head | ? | 0.8 / 47.4 | 9 / 102 |
| UpperArmL | ? | 1.0 / 64.4 | 4 / 177 |
| ForearmL | ? | 1.5 / 49.3 | 4 / 180 |
| HandL | ? | 1.7 / 46.3 | 2 / 69 |
| UpperArmR | ? | 0.6 / 24.5 | 2 / 34 |
| ForearmR | ? | 1.4 / 24.8 | 4 / 63 |
| HandR | ? | 2.0 / 28.2 | 4 / 82 |
| ThighL | ? | 0.0 / 23.8 | 0 / 145 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 24.4 | 0 / 149 |
| ShinR | ? | 0.0 / 16.2 | 0 / 0 |
| FootR | ? | 0.0 / 14.2 | 0 / 0 |

Hits dealt: 13 (3 on the part aimed at, 1 blocked, 1 parried, 0 on someone in the way, 0 on allies), 147 damage. Hits taken: 3, 21 damage. Health at the end 0.

Defence: 6 blows came at it (4 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 1, set its weapon across 0.

Pose against the joint limits (standing): a joint more than 10° past its limit 39% of the time; the worst 103° (ShinR).
Ragdoll (on the ground 1.2 s): a joint more than 10° past its limit 100% of the time; the worst 75° (ForearmR). Fastest part with a standing leg within 0.5 m: 2.9 m/s median, 5.2 m/s at the 95th percentile (42 steps); with none: 2.1 m/s median (29 steps).

Look: within 25° of what it attended to 93% of the time standing (93% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 94% of the time standing; its rim came within -0.7 cm of its own body (negative: inside).

Fight Rhythm (?): 10 postures (GuardChange 2, Peek 3, ShieldSettle 3, HighFeint 2), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 1, launched 0, got up 1; thrown into others 0 times.

Blows: 9. The posed striking part passed within 3 cm of the aim point in 6; aim error mean 1.8 cm, max 5.4 cm (posed); physical weapon's closest pass mean 1.5 cm, max 5.4 cm. 2 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.17 s: SwordBackhand at A Small's Neck: passed 0.1 cm from the aim point; nearest part A Small's UpperArmR (23.8 cm); the Neck had moved 39.3 cm since the aim was locked
- 18.47 s: SwordThrust at A Small's Belly: passed 4.6 cm from the aim point; nearest part A Small's Chest (3.3 cm); the Belly had moved 18.7 cm since the aim was locked
- 24.52 s: SwordCleave at A Small's ElbowL: passed 0.0 cm from the aim point; nearest part A Small's Chest (7.0 cm); the ElbowL had moved 38.0 cm since the aim was locked
- 30.92 s: SwordThrust at A Small's Neck: passed 0.1 cm from the aim point; nearest part A Small's Head (67.6 cm); the Neck had moved 66.7 cm since the aim was locked
- 32.30 s: SwordThrust at A Small's Belly: passed 5.4 cm from the aim point; nearest part A Small's Spine (0.0 cm); the Belly had moved 16.3 cm since the aim was locked
- 32.83 s: SwordBackhand at A Small's Neck: passed 0.6 cm from the aim point; nearest part A Small's Chest (6.3 cm); the Neck had moved 10.1 cm since the aim was locked
- 48.60 s: SwordThrust at A Large's Neck: passed 0.0 cm from the aim point; nearest part A Large's Chest (99.2 cm); the Neck had moved 96.1 cm since the aim was locked
- 61.05 s: SwordCut at A Large's Belly: passed 0.1 cm from the aim point; nearest part A Large's Spine (0.0 cm); the Belly had moved 34.8 cm since the aim was locked
- 64.33 s: SwordCut at A Medium's Chest: passed 5.1 cm from the aim point; nearest part A Medium's HandL (48.8 cm); the Chest had moved 98.0 cm since the aim was locked


## B Large

Present 0.0–76.1 s. Steps 135, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 53 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.7 | 2 / 85 |
| Chest | ? | 0.9 / 34.3 | 3 / 96 |
| Head | ? | 1.4 / 50.8 | 12 / 79 |
| UpperArmL | ? | 1.7 / 73.7 | 6 / 170 |
| ForearmL | ? | 3.5 / 76.0 | 10 / 138 |
| HandL | ? | 5.1 / 84.7 | 23 / 166 |
| UpperArmR | ? | 1.5 / 28.0 | 3 / 65 |
| ForearmR | ? | 3.0 / 47.8 | 4 / 95 |
| HandR | ? | 3.6 / 60.7 | 6 / 159 |
| ThighL | ? | 0.0 / 0.0 | 0 / 148 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 150 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 11 (2 on the part aimed at, 6 blocked, 0 parried, 2 on someone in the way, 1 on allies), 76 damage. Hits taken: 24, 165 damage. Health at the end 0.

Defence: 21 blows came at it (10 seen clearly, 11 at the edge of its view, 0 never seen); it raised the shield against 0, set its weapon across 6, and had nothing to defend with against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 39% of the time; the worst 105° (ShinR).

Look: within 25° of what it attended to 90% of the time standing (91% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 7 postures (BoxerSettle 1, Shift 1, BobAndWeave 2, ClubFeint 1, JabFeint 1, BoxerStalk 1), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 3 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 6. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 2.3 cm, max 13.1 cm (posed); physical weapon's closest pass mean 4.6 cm, max 12.8 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.40 s: ClubSwing at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Chest (9.6 cm); the Chest had moved 36.7 cm since the aim was locked
- 15.43 s: ClubRising at A Medium's Chest: passed 13.1 cm from the aim point; nearest part A Medium's HandL (58.5 cm); the Chest had moved 104.6 cm since the aim was locked
- 22.02 s: ClubRising at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Large's Pelvis (67.9 cm); the Chest had moved 111.4 cm since the aim was locked
- 38.02 s: ClubSwing at A Medium's Head: passed 0.6 cm from the aim point; nearest part A Medium's Chest (25.8 cm); the Head had moved 25.8 cm since the aim was locked
- 52.22 s: ClubBackhand at A Medium's ElbowL: passed 0.1 cm from the aim point; nearest part A Medium's Chest (25.0 cm); the ElbowL had moved 67.1 cm since the aim was locked
- 66.38 s: ClubBackhand at A Medium's Neck: passed 0.1 cm from the aim point; nearest part A Medium's Head (11.1 cm); the Neck had moved 14.1 cm since the aim was locked

## Defence (milestone 11)

- Small: pressed 13 s, 65% of it in the shell (gate 70%); 0 of 4 combo links defended, 0% (gate 30–60%); 3 of 11 blows that met it on its guard (0 blocked, 3 parried, 0 on its guarding arms); 1 guard breaks, 1.4 a minute on its feet (gate about 1 at most); 1 of 2 bursts at it answered with a counter, 50% (gate 30–50%), 0 of them ripostes between links.
- Medium: pressed 29 s, 66% of it in the shell (gate 70%); 4 of 10 combo links defended, 40% (gate 30–60%); 11 of 18 blows that met it on its guard (11 blocked, 0 parried, 0 on its guarding arms); 1 guard breaks, 0.5 a minute on its feet (gate about 1 at most); 8 of 13 bursts at it answered with a counter, 62% (gate 30–50%), 0 of them ripostes between links.
- Large: pressed 32 s, 57% of it in the shell (gate 70%); 1 of 7 combo links defended, 14% (gate 30–60%); 7 of 23 blows that met it on its guard (0 blocked, 3 parried, 4 on its guarding arms); 1 guard breaks, 0.4 a minute on its feet (gate about 1 at most); 3 of 9 bursts at it answered with a counter, 33% (gate 30–50%), 3 of them ripostes between links.

## Shield contacts

190 times a shield overlapped another unit's body: 95% under 6.2 cm deep (67% under 2 cm; target 95%), the deepest 9.8 cm, the longest 0.25 s (target under 0.2 s). Each one is in events.log ("shield").

## Performance

44780 frames in 84.8 s (528 fps on average); 40232 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 5.0 ms, 95% 9.5, 99% 16.3, worst 1047.4. 33 hitches over 40 ms (15 over 100 ms). 74 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 104.8
- ArmSolve: 89.9
- Compose: 86.4
- RecorderSample: 65.6
- RecorderFlush: 26.1
- ComposeDry: 24.8
- Sandbox: 24.4
- Plan: 17.6
- Clearance: 17.4
- Authority: 4.5
- Prepare: 2.6
- TryOut: 1.0
- Sweep: 0.6
- CaptureSave: 0.4
- CaptureRender: 0.0

The worst frames:

- 48.50 s: 1047 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 0.2 ms, Gfx_WaitForGfxCommandsFromMainThread 0.2 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 48.29 s: 763 ms frame, 1 physics steps; ours: UnitStep 3.0 ms (6), Compose 2.8 ms (4), ArmSolve 2.2 ms (50), RecorderSample 1.6 ms (1), RecorderFlush 0.9 ms (1); Unity: Main_Thread 0.3 ms, Gfx_WaitForGfxCommandsFromMainThread 0.3 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms
- 0.02 s: 741 ms frame, 1 physics steps, garbage collected (gen0 3, gen1 3, gen2 3); ours: UnitStep 35.3 ms (6), RecorderSample 31.0 ms (1), Compose 24.6 ms (12), ArmSolve 6.3 ms (18), Authority 3.6 ms (6), Sandbox 2.2 ms (1); Unity: Gfx_WaitForGfxCommandsFromMainThread 401.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 71.1 ms, FixedBehaviourUpdate 71.1 ms, Physics_Simulate 14.5 ms, GC_Collect 5.9 ms, GC_Alloc 2.2 ms, Update_ScriptRunBehaviourUpdate 1.3 ms, BehaviourUpdate 1.3 ms
- 43.49 s: 714 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 0.2 ms, Gfx_WaitForGfxCommandsFromMainThread 0.2 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 43.26 s: 590 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 0.2 ms, Gfx_WaitForGfxCommandsFromMainThread 0.2 ms, CPU_Total_Frame_Time 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 42.58 s: 578 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 0.3 ms, Gfx_WaitForGfxCommandsFromMainThread 0.3 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 38.96 s: 575 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 0.2 ms, Gfx_WaitForGfxCommandsFromMainThread 0.2 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 36.87 s: 539 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 0.2 ms, Gfx_WaitForGfxCommandsFromMainThread 0.2 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms
- 35.72 s: 518 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 0.2 ms, Gfx_WaitForGfxCommandsFromMainThread 0.2 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 37.41 s: 512 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 0.3 ms, Gfx_WaitForGfxCommandsFromMainThread 0.3 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms
- 59.45 s: 482 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 0.3 ms, Gfx_WaitForGfxCommandsFromMainThread 0.2 ms, CPU_Total_Frame_Time 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 52.02 s: 475 ms frame, 1 physics steps; ours: UnitStep 2.0 ms (6), Compose 1.8 ms (4), RecorderSample 1.5 ms (1), ArmSolve 1.5 ms (35), RecorderFlush 0.8 ms (1); Unity: Main_Thread 0.2 ms, Gfx_WaitForGfxCommandsFromMainThread 0.2 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

