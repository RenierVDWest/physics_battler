# Playtest 2026-11-03_07-00-20

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 97.7 s of play (5861 physics steps), 3561 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-03_07-00-20`.

## A Small

Present 0.0–97.7 s. Steps 144, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 19 cm from the support point; hips dropped for reach by up to 3 cm.

Feet: 110 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 49°, 95% under 74°.
Walking: 29.5 s (95% forward, 3% sideways, 2% backward), the body went 13.8 m and the feet 16.1 m each on average (feet/body 1.16; well under 1 is gliding); the feet point off the line it walks along (either way) median 12°, 95% under 45°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 4 / 175 |
| Chest | ? | 0.8 / 20.3 | 6 / 63 |
| Head | ? | 1.2 / 26.8 | 11 / 141 |
| UpperArmL | ? | 1.2 / 19.1 | 6 / 155 |
| ForearmL | ? | 1.5 / 24.9 | 7 / 97 |
| HandL | ? | 2.1 / 36.4 | 20 / 168 |
| UpperArmR | ? | 1.4 / 30.6 | 7 / 60 |
| ForearmR | ? | 2.8 / 31.9 | 12 / 119 |
| HandR | ? | 5.0 / 40.6 | 15 / 172 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 1 |

Hits dealt: 3 (0 on the part aimed at, 0 blocked, 0 parried, 1 on someone in the way, 0 on allies), 21 damage. Hits taken: 9, 61 damage. Health at the end 0.

Defence: 6 blows came at it (5 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, set its weapon across 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 66% of the time; the worst 52° (FootR).
Ragdoll (on the ground 51.2 s): a joint more than 10° past its limit 97% of the time; the worst 161° (ForearmL). Fastest part with a standing leg within 0.5 m: 6.1 m/s median, 8.2 m/s at the 95th percentile (42 steps); with none: 0.0 m/s median (3029 steps).

Look: within 25° of what it attended to 79% of the time standing (83% of the time it was within the head's reach, 93% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 16 postures (StickSettle 3, AbanicoFlurry 5, BaitRetreat 3, RedondaTwirl 4, TriangleStep 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 3 times, knocked down 1, launched 1, got up 2; thrown into others 1 time.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.0 cm, max 0.0 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 8.17 s: ClubBackhand at B Medium's KneeR: passed 0.0 cm from the aim point; nearest part B Medium's ThighL (24.0 cm); the KneeR had moved 39.3 cm since the aim was locked


## A Medium

Present 0.0–97.7 s. Steps 210, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 50 cm from the support point; hips dropped for reach by up to 3 cm.

Feet: 164 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 28°, 95% under 80°.
Walking: 35.9 s (74% forward, 4% sideways, 23% backward), the body went 17.3 m and the feet 21.6 m each on average (feet/body 1.25; well under 1 is gliding); the feet point off the line it walks along (either way) median 11°, 95% under 46°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 5.4 | 0 / 0 |
| Spine | ? | 0.0 / 5.4 | 2 / 27 |
| Chest | ? | 0.7 / 8.2 | 4 / 34 |
| Head | ? | 1.0 / 8.7 | 8 / 56 |
| UpperArmL | ? | 1.1 / 9.4 | 4 / 53 |
| ForearmL | ? | 1.3 / 22.3 | 3 / 70 |
| HandL | ? | 1.5 / 43.5 | 3 / 108 |
| UpperArmR | ? | 1.1 / 13.2 | 5 / 42 |
| ForearmR | ? | 2.5 / 28.3 | 7 / 82 |
| HandR | ? | 3.8 / 44.2 | 9 / 108 |
| ThighL | ? | 0.0 / 5.1 | 0 / 4 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 4 |
| ThighR | ? | 0.0 / 5.7 | 0 / 148 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 12 (0 on the part aimed at, 4 blocked, 0 parried, 1 on someone in the way, 2 on allies), 74 damage. Hits taken: 18, 88 damage. Health at the end 0.

Defence: 13 blows came at it (9 seen clearly, 4 at the edge of its view, 0 never seen); it raised the shield against 8, set its weapon across 0.

Pose against the joint limits (standing): a joint more than 10° past its limit 37% of the time; the worst 81° (ShinR).
Ragdoll (on the ground 4.4 s): a joint more than 10° past its limit 83% of the time; the worst 176° (ForearmL). Fastest part with a standing leg within 0.5 m: 3.9 m/s median, 8.0 m/s at the 95th percentile (113 steps); with none: 2.6 m/s median (149 steps).

Look: within 25° of what it attended to 89% of the time standing (90% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 96% of the time standing; its rim came within 1.1 cm of its own body (negative: inside).

Fight Rhythm (?): 13 postures (GuardChange 1, Peek 4, HighFeint 4, ShieldPress 2, ShieldSettle 2), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 8 times, knocked down 3, launched 0, got up 3; thrown into others 4 times.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.2 cm, max 0.3 cm (posed); physical weapon's closest pass mean 0.5 cm, max 0.8 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 29.12 s: SwordCut at B Medium's Belly: passed 0.2 cm from the aim point; nearest part B Medium's UpperArmL (17.6 cm); the Belly had moved 48.0 cm since the aim was locked
- 39.67 s: SwordBackhand at B Medium's ElbowL: passed 0.3 cm from the aim point; nearest part B Medium's HandL (28.7 cm); the ElbowL had moved 67.9 cm since the aim was locked
- 49.43 s: SwordBackhand at B Medium's ElbowL: passed 0.2 cm from the aim point; nearest part B Medium's UpperArmL (40.5 cm); the ElbowL had moved 42.6 cm since the aim was locked
- 78.17 s: SwordCleave at B Medium's ElbowR: passed 0.1 cm from the aim point; nearest part B Medium's Pelvis (38.8 cm); the ElbowR had moved 31.5 cm since the aim was locked


## A Large

Present 0.0–97.7 s. Steps 199, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 54 cm from the support point; hips dropped for reach by up to 18 cm.

Feet: 154 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 23°, 95% under 62°.
Walking: 45.4 s (46% forward, 0% sideways, 54% backward), the body went 22.4 m and the feet 29.2 m each on average (feet/body 1.31; well under 1 is gliding); the feet point off the line it walks along (either way) median 15°, 95% under 54°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 2 / 36 |
| Chest | ? | 0.9 / 13.0 | 3 / 31 |
| Head | ? | 1.3 / 13.2 | 12 / 102 |
| UpperArmL | ? | 1.5 / 19.8 | 5 / 73 |
| ForearmL | ? | 3.4 / 29.1 | 8 / 63 |
| HandL | ? | 4.4 / 45.0 | 21 / 179 |
| UpperArmR | ? | 1.6 / 24.1 | 3 / 57 |
| ForearmR | ? | 3.0 / 46.9 | 4 / 79 |
| HandR | ? | 3.7 / 52.6 | 6 / 159 |
| ThighL | ? | 0.0 / 0.0 | 0 / 150 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 150 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 21 (1 on the part aimed at, 9 blocked, 2 parried, 1 on someone in the way, 1 on allies), 123 damage. Hits taken: 12, 93 damage. Health at the end 0.

Defence: 12 blows came at it (4 seen clearly, 8 at the edge of its view, 0 never seen); it raised the shield against 0, set its weapon across 6.

Pose against the joint limits (standing): a joint more than 10° past its limit 40% of the time; the worst 151° (ShinR).

Look: within 25° of what it attended to 85% of the time standing (86% of the time it was within the head's reach, 95% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 13 postures (BoxerSettle 1, BobAndWeave 4, ClubFeint 5, JabFeint 1, Shift 1, BoxerStalk 1), 3 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 9. The posed striking part passed within 3 cm of the aim point in 8; aim error mean 0.5 cm, max 3.4 cm (posed); physical weapon's closest pass mean 3.1 cm, max 8.6 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.65 s: ClubRising at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's Head (25.6 cm); the Belly had moved 28.8 cm since the aim was locked
- 4.50 s: ClubSwing at B Small's Belly: passed 0.6 cm from the aim point; nearest part B Small's FootL (60.7 cm); the Belly had moved 74.8 cm since the aim was locked
- 5.12 s: ClubBackhand at B Small's ElbowL: passed 3.4 cm from the aim point; nearest part B Small's UpperArmR (3.8 cm); the ElbowL had moved 41.9 cm since the aim was locked
- 11.07 s: ClubRising at B Small's Belly: passed 0.1 cm from the aim point; nearest part B Small's Pelvis (14.6 cm); the Belly had moved 14.6 cm since the aim was locked
- 18.55 s: OffJab at B Large's Head: passed 0.1 cm from the aim point; nearest part B Large's HandL (33.0 cm); the Head had moved 72.2 cm since the aim was locked
- 19.43 s: OffJab at B Large's Chest: passed 0.2 cm from the aim point; nearest part B Large's Chest (12.6 cm); the Chest had moved 42.8 cm since the aim was locked
- 29.50 s: ClubSwing at B Medium's Chest: passed 0.0 cm from the aim point; nearest part B Medium's UpperArmL (1.0 cm); the Chest had moved 28.2 cm since the aim was locked
- 51.75 s: OffJab at B Medium's Head: passed 0.0 cm from the aim point; nearest part B Medium's Head (29.1 cm); the Head had moved 28.6 cm since the aim was locked
- 94.67 s: ClubRising at B Medium's Belly: passed 0.0 cm from the aim point; nearest part B Medium's ForearmL (6.3 cm); the Belly had moved 28.3 cm since the aim was locked


## B Small

Present 0.0–97.7 s. Steps 40, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 11 cm from the support point; hips dropped for reach by up to 12 cm.

Feet: 38 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 44°, 95% under 91°.
Walking: 8.3 s (87% forward, 3% sideways, 10% backward), the body went 3.8 m and the feet 4.3 m each on average (feet/body 1.13; well under 1 is gliding); the feet point off the line it walks along (either way) median 8°, 95% under 72°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.0 | 3 / 20 |
| Chest | ? | 0.6 / 3.9 | 5 / 28 |
| Head | ? | 0.9 / 5.2 | 7 / 80 |
| UpperArmL | ? | 1.0 / 6.1 | 5 / 57 |
| ForearmL | ? | 1.4 / 14.4 | 6 / 96 |
| HandL | ? | 1.9 / 36.3 | 17 / 107 |
| UpperArmR | ? | 1.0 / 7.8 | 6 / 48 |
| ForearmR | ? | 2.5 / 17.7 | 11 / 88 |
| HandR | ? | 4.2 / 29.1 | 13 / 103 |
| ThighL | ? | 0.0 / 0.0 | 0 / 48 |
| ShinL | ? | 0.0 / 0.0 | 0 / 48 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 6 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 7, 71 damage. Health at the end 0.

Defence: 4 blows came at it (3 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, set its weapon across 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 52% of the time; the worst 116° (FootL).
Ragdoll (on the ground 80.6 s): a joint more than 10° past its limit 7% of the time; the worst 163° (ForearmL). Fastest part with a standing leg within 0.5 m: 0.1 m/s median, 9.3 m/s at the 95th percentile (172 steps); with none: 0.0 m/s median (4664 steps).

Look: within 25° of what it attended to 80% of the time standing (87% of the time it was within the head's reach, 92% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 0 times, knocked down 2, launched 2, got up 2; thrown into others 1 time.


## B Medium

Present 0.0–97.7 s. Steps 201, idle shuffles 1. Feet slid at most 4.2 cm while planted; the weight was at most 54 cm from the support point; hips dropped for reach by up to 21 cm.

Feet: 151 plants, 1 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 4 cm, twisted 0°, 2.5 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 21°, 95% under 74°.
Walking: 51.3 s (48% forward, 10% sideways, 42% backward), the body went 24.5 m and the feet 32.4 m each on average (feet/body 1.32; well under 1 is gliding); the feet point off the line it walks along (either way) median 13°, 95% under 65°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 2.0 | 3 / 30 |
| Chest | ? | 0.8 / 8.6 | 5 / 54 |
| Head | ? | 1.1 / 14.0 | 9 / 53 |
| UpperArmL | ? | 1.4 / 13.1 | 5 / 164 |
| ForearmL | ? | 1.8 / 32.3 | 6 / 169 |
| HandL | ? | 2.7 / 52.7 | 4 / 91 |
| UpperArmR | ? | 1.7 / 27.5 | 6 / 62 |
| ForearmR | ? | 3.5 / 39.5 | 10 / 85 |
| HandR | ? | 5.4 / 46.7 | 12 / 106 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 150 |
| ShinR | ? | 0.0 / 1.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 7 (0 on the part aimed at, 3 blocked, 1 parried, 0 on someone in the way, 1 on allies), 22 damage. Hits taken: 24, 110 damage. Health at the end 0.

Defence: 26 blows came at it (15 seen clearly, 11 at the edge of its view, 0 never seen); it raised the shield against 18, set its weapon across 0.

Pose against the joint limits (standing): a joint more than 10° past its limit 46% of the time; the worst 164° (ShinR).
Ragdoll (on the ground 7.8 s): a joint more than 10° past its limit 90% of the time; the worst 177° (ForearmL). Fastest part with a standing leg within 0.5 m: 2.9 m/s median, 9.0 m/s at the 95th percentile (159 steps); with none: 1.3 m/s median (311 steps).

Look: within 25° of what it attended to 89% of the time standing (89% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 92% of the time standing; its rim came within 1.0 cm of its own body (negative: inside).

Fight Rhythm (?): 16 postures (GuardChange 1, HighFeint 7, Peek 6, ShieldPress 2), 2 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 8 times, knocked down 3, launched 0, got up 3; thrown into others 2 times.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 2.4 cm, max 4.6 cm (posed); physical weapon's closest pass mean 0.5 cm, max 0.5 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 12.57 s: SwordThrust at A Small's Chest: passed 4.6 cm from the aim point; nearest part A Small's Chest (3.1 cm); the Chest had moved 6.8 cm since the aim was locked
- 87.08 s: SwordBackhand at A Large's KneeR: passed 0.1 cm from the aim point; nearest part A Large's HandR (40.6 cm); the KneeR had moved 56.8 cm since the aim was locked


## B Large

Present 0.0–97.7 s. Steps 194, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 58 cm from the support point; hips dropped for reach by up to 3 cm.

Feet: 163 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 26°, 95% under 81°.
Walking: 37.8 s (54% forward, 4% sideways, 43% backward), the body went 20.3 m and the feet 24.7 m each on average (feet/body 1.21; well under 1 is gliding); the feet point off the line it walks along (either way) median 12°, 95% under 61°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 6.9 | 0 / 0 |
| Spine | ? | 0.0 / 6.9 | 3 / 132 |
| Chest | ? | 1.1 / 61.3 | 4 / 172 |
| Head | ? | 1.9 / 106.1 | 14 / 117 |
| UpperArmL | ? | 2.4 / 110.7 | 7 / 153 |
| ForearmL | ? | 4.1 / 88.1 | 10 / 127 |
| HandL | ? | 5.4 / 106.8 | 24 / 179 |
| UpperArmR | ? | 2.4 / 98.5 | 5 / 176 |
| ForearmR | ? | 4.1 / 131.0 | 6 / 180 |
| HandR | ? | 4.7 / 181.4 | 8 / 180 |
| ThighL | ? | 0.0 / 6.5 | 0 / 150 |
| ShinL | ? | 0.0 / 15.3 | 0 / 52 |
| FootL | ? | 0.0 / 0.0 | 0 / 3 |
| ThighR | ? | 0.0 / 7.3 | 0 / 149 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 2 |

Hits dealt: 23 (3 on the part aimed at, 5 blocked, 5 parried, 0 on someone in the way, 3 on allies), 153 damage. Hits taken: 12, 62 damage. Health at the end 0.

Defence: 10 blows came at it (4 seen clearly, 6 at the edge of its view, 0 never seen); it raised the shield against 0, set its weapon across 5, and had nothing to defend with against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 43% of the time; the worst 117° (FootL).

Look: within 25° of what it attended to 86% of the time standing (88% of the time it was within the head's reach, 91% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 21 postures (BoxerSettle 2, ClubFeint 9, BobAndWeave 3, JabFeint 6, BoxerStalk 1), 5 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 15. The posed striking part passed within 3 cm of the aim point in 14; aim error mean 0.4 cm, max 3.6 cm (posed); physical weapon's closest pass mean 2.6 cm, max 15.4 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 11.97 s: ClubBackhand at A Medium's Chest: passed 0.9 cm from the aim point; nearest part A Medium's Chest (3.2 cm); the Chest had moved 18.9 cm since the aim was locked
- 14.03 s: ClubBackhand at A Small's KneeL: passed 0.1 cm from the aim point; nearest part A Small's ThighR (14.4 cm); the KneeL had moved 22.5 cm since the aim was locked
- 30.28 s: ClubRising at A Small's Belly: passed 0.5 cm from the aim point; nearest part A Small's Chest (13.0 cm); the Belly had moved 8.8 cm since the aim was locked
- 37.98 s: ClubBackhand at A Small's KneeR: passed 0.1 cm from the aim point; nearest part A Small's ForearmR (30.4 cm); the KneeR had moved 56.5 cm since the aim was locked
- 43.22 s: ClubRising at A Small's Belly: passed 0.1 cm from the aim point; nearest part A Small's Pelvis (0.2 cm); the Belly had moved 16.5 cm since the aim was locked
- 47.03 s: ClubBackhand at A Small's ElbowR: passed 3.6 cm from the aim point; nearest part A Small's Pelvis (46.9 cm); the ElbowR had moved 86.1 cm since the aim was locked
- 49.10 s: ClubRising at A Small's KneeL: passed 0.1 cm from the aim point; nearest part A Small's ThighL (3.2 cm); the KneeL had moved 1.9 cm since the aim was locked
- 58.40 s: ClubRising at A Medium's Chest: passed 0.1 cm from the aim point; nearest part A Medium's UpperArmR (23.1 cm); the Chest had moved 49.0 cm since the aim was locked
- 60.35 s: ClubRising at A Large's Belly: passed 0.1 cm from the aim point; nearest part A Large's UpperArmL (30.7 cm); the Belly had moved 46.4 cm since the aim was locked
- 72.67 s: ClubSwing at A Large's Chest: passed 0.6 cm from the aim point; nearest part A Large's HandR (18.3 cm); the Chest had moved 65.5 cm since the aim was locked
- 74.85 s: ClubSwing at A Large's Head: passed 0.1 cm from the aim point; nearest part A Large's HandR (16.1 cm); the Head had moved 53.7 cm since the aim was locked
- 79.33 s: ClubRising at A Large's Belly: passed 0.0 cm from the aim point; nearest part A Large's UpperArmL (39.5 cm); the Belly had moved 66.5 cm since the aim was locked
- 80.07 s: ClubSwing at A Large's Head: passed 0.5 cm from the aim point; nearest part A Large's Head (18.2 cm); the Head had moved 28.7 cm since the aim was locked
- 85.62 s: ClubBackhand at A Medium's Neck: passed 0.0 cm from the aim point; nearest part A Medium's Head (12.2 cm); the Neck had moved 25.3 cm since the aim was locked
- 89.08 s: ClubSwing at A Medium's Belly: passed 0.1 cm from the aim point; nearest part A Medium's ForearmL (21.1 cm); the Belly had moved 37.1 cm since the aim was locked

## Defence (milestone 11)

- Small: pressed 10 s, 56% of it in the shell (gate 70%); 0 of 3 combo links defended, 0% (gate 30–60%); 2 of 11 blows that met it on its guard (0 blocked, 2 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 1 of 3 bursts at it answered with a counter, 33% (gate 30–50%), 0 of them ripostes between links.
- Medium: pressed 45 s, 73% of it in the shell (gate 70%); 6 of 8 combo links defended, 75% (gate 30–60%); 21 of 34 blows that met it on its guard (21 blocked, 0 parried, 0 on its guarding arms); 3 guard breaks, 1.2 a minute on its feet (gate about 1 at most); 6 of 14 bursts at it answered with a counter, 43% (gate 30–50%), 1 of them ripostes between links.
- Large: pressed 57 s, 61% of it in the shell (gate 70%); 0 of 2 combo links defended, 0% (gate 30–60%); 8 of 21 blows that met it on its guard (0 blocked, 6 parried, 2 on its guarding arms); 2 guard breaks, 0.6 a minute on its feet (gate about 1 at most); 9 of 20 bursts at it answered with a counter, 45% (gate 30–50%), 0 of them ripostes between links.

## Shield contacts

129 times a shield overlapped another unit's body: 95% under 4.1 cm deep (81% under 2 cm; target 95%), the deepest 8.9 cm, the longest 0.10 s (target under 0.2 s). Each one is in events.log ("shield").

## Performance

59102 frames in 98.4 s (600 fps on average); 53258 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 5.1 ms, 95% 9.4, 99% 11.4, worst 733.6. 8 hitches over 40 ms (1 over 100 ms). 155 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 117.0
- ArmSolve: 100.1
- Compose: 97.9
- RecorderSample: 79.5
- RecorderFlush: 30.4
- Sandbox: 28.0
- ComposeDry: 25.8
- Plan: 23.2
- Clearance: 18.5
- Authority: 5.2
- Prepare: 2.9
- TryOut: 1.2
- Sweep: 0.5
- CaptureSave: 0.1
- CaptureRender: 0.0

The worst frames:

- 0.02 s: 734 ms frame, 1 physics steps, garbage collected (gen0 4, gen1 4, gen2 4); ours: UnitStep 34.8 ms (6), RecorderSample 34.1 ms (1), Compose 24.3 ms (12), ArmSolve 6.4 ms (18), Authority 3.6 ms (6), Sandbox 2.3 ms (1); Unity: Gfx_WaitForGfxCommandsFromMainThread 415.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 73.9 ms, FixedBehaviourUpdate 73.9 ms, Physics_Simulate 15.4 ms, GC_Collect 9.8 ms, GC_Alloc 2.2 ms, Update_ScriptRunBehaviourUpdate 1.3 ms, BehaviourUpdate 1.3 ms
- 4.60 s: 71 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 64.8 ms (6), ComposeDry 51.0 ms (69), ArmSolve 48.5 ms (1275), Clearance 25.5 ms (1), Prepare 12.6 ms (6), TryOut 5.1 ms (12); Unity: Main_Thread 71.1 ms, Gfx_WaitForGfxCommandsFromMainThread 70.0 ms, PlayerLoop 68.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 67.2 ms, FixedBehaviourUpdate 67.2 ms, CPU_Total_Frame_Time 1.2 ms, FixedUpdate_PhysicsFixedUpdate 0.7 ms, CPU_Render_Thread_Frame_Time 0.6 ms
- 2.27 s: 68 ms frame, 1 physics steps; ours: Sandbox 57.4 ms (1), ComposeDry 29.7 ms (54), ArmSolve 26.4 ms (717), Clearance 19.8 ms (1), Prepare 11.5 ms (1), UnitStep 6.1 ms (6); Unity: Main_Thread 67.8 ms, Gfx_WaitForGfxCommandsFromMainThread 66.6 ms, PlayerLoop 65.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 65.0 ms, FixedBehaviourUpdate 65.0 ms, CPU_Total_Frame_Time 1.1 ms, GPU_Frame_Time 0.5 ms, FixedUpdate_PhysicsFixedUpdate 0.4 ms
- 3.87 s: 61 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 55.7 ms (6), ComposeDry 48.1 ms (54), ArmSolve 40.8 ms (1034), Clearance 23.8 ms (1), Prepare 2.2 ms (2), Compose 1.9 ms (4); Unity: Main_Thread 60.6 ms, Gfx_WaitForGfxCommandsFromMainThread 59.6 ms, PlayerLoop 58.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 57.5 ms, FixedBehaviourUpdate 57.5 ms, CPU_Total_Frame_Time 1.2 ms, FixedUpdate_PhysicsFixedUpdate 0.6 ms, Physics_Simulate 0.5 ms
- 23.08 s: 48 ms frame, 1 physics steps; ours: Sandbox 41.0 ms (1), ComposeDry 38.0 ms (54), ArmSolve 33.6 ms (869), Clearance 31.3 ms (1), Prepare 2.1 ms (1), UnitStep 2.0 ms (6); Unity: Main_Thread 48.1 ms, Gfx_WaitForGfxCommandsFromMainThread 47.1 ms, PlayerLoop 46.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 45.1 ms, FixedBehaviourUpdate 45.1 ms, CPU_Total_Frame_Time 1.1 ms, FixedUpdate_PhysicsFixedUpdate 0.7 ms, Physics_Simulate 0.6 ms
- 93.53 s: 46 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Sandbox 36.1 ms (1), ComposeDry 32.8 ms (54), ArmSolve 30.6 ms (807), Clearance 25.5 ms (1), RecorderSample 4.6 ms (1), UnitStep 3.0 ms (6); Unity: Main_Thread 46.3 ms, Gfx_WaitForGfxCommandsFromMainThread 45.4 ms, PlayerLoop 44.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 43.7 ms, FixedBehaviourUpdate 43.7 ms, GC_Collect 2.7 ms, CPU_Total_Frame_Time 1.1 ms, GPU_Frame_Time 0.5 ms
- 11.45 s: 45 ms frame, 1 physics steps; ours: UnitStep 42.0 ms (6), ComposeDry 36.5 ms (54), ArmSolve 35.2 ms (859), Clearance 26.4 ms (1), Prepare 2.1 ms (1), Compose 1.8 ms (5); Unity: Main_Thread 45.0 ms, Gfx_WaitForGfxCommandsFromMainThread 44.2 ms, PlayerLoop 43.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 42.8 ms, FixedBehaviourUpdate 42.8 ms, CPU_Total_Frame_Time 1.1 ms, GPU_Frame_Time 0.5 ms, CPU_Render_Thread_Frame_Time 0.4 ms
- 79.42 s: 40 ms frame, 1 physics steps; ours: UnitStep 36.7 ms (6), ComposeDry 32.2 ms (54), ArmSolve 30.8 ms (827), Clearance 21.9 ms (1), Prepare 2.3 ms (1), RecorderSample 1.0 ms (1); Unity: Main_Thread 40.0 ms, Gfx_WaitForGfxCommandsFromMainThread 39.2 ms, PlayerLoop 38.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 37.7 ms, FixedBehaviourUpdate 37.7 ms, CPU_Total_Frame_Time 1.3 ms, GPU_Frame_Time 0.5 ms, FixedUpdate_PhysicsFixedUpdate 0.4 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

