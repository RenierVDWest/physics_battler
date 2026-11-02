# Playtest 2026-11-02_07-22-52

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 18.7 s of play (1122 physics steps), 1484 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-02_07-22-52`.

## A Small

Present 0.0–18.7 s. Steps 31, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 16 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.1 | 2 / 20 |
| Chest | Ragdoll | 0.3 / 3.9 | 2 / 18 |
| Head | Ragdoll | 0.4 / 4.5 | 8 / 51 |
| UpperArmL | Ragdoll | 0.5 / 6.7 | 4 / 89 |
| ForearmL | Ragdoll | 1.3 / 15.4 | 8 / 108 |
| HandL | Ragdoll | 2.0 / 42.5 | 16 / 81 |
| UpperArmR | Ragdoll | 0.3 / 1.9 | 8 / 92 |
| ForearmR | Ragdoll | 0.8 / 14.5 | 7 / 119 |
| HandR | Ragdoll | 1.8 / 31.5 | 16 / 102 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 9 damage. Hits taken: 4, 65 damage. Killed.

Defence: 4 blows came at it (3 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 56% of the time; the worst 42° (FootL).
Ragdoll (on the ground 7.3 s): a joint more than 10° past its limit 29% of the time; the worst 168° (ShinR). Fastest part with a standing leg within 0.5 m: 3.3 m/s median, 7.2 m/s at the 95th percentile (68 steps); with none: 0.2 m/s median (367 steps).

Look: within 25° of what it attended to 77% of the time standing (77% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.61.

Balance: stumbled 3 times, knocked down 1, launched 0, got up 1; thrown into others 0 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.5 cm, max 0.5 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.48 s: ClubBackhand at B Medium's Chest: passed 0.5 cm from the aim point; nearest part B Medium's Chest (16.6 cm); the Chest had moved 42.2 cm since the aim was locked


## A Medium

Present 0.0–18.7 s. Steps 30, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 20 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 1 / 14 |
| Chest | ? | 0.4 / 4.1 | 2 / 25 |
| Head | ? | 0.6 / 8.3 | 7 / 40 |
| UpperArmL | ? | 0.6 / 11.8 | 5 / 143 |
| ForearmL | ? | 1.9 / 40.0 | 9 / 172 |
| HandL | ? | 2.5 / 47.2 | 3 / 55 |
| UpperArmR | ? | 0.6 / 8.9 | 4 / 91 |
| ForearmR | ? | 1.1 / 17.3 | 6 / 95 |
| HandR | ? | 1.8 / 25.3 | 16 / 112 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 1 |

Hits dealt: 2 (0 on the part aimed at, 2 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 8, 59 damage. Health at the end 0.

Defence: 3 blows came at it (2 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0.

Pose against the joint limits (standing): a joint more than 10° past its limit 40% of the time; the worst 77° (FootR).
Ragdoll (on the ground 3.4 s): a joint more than 10° past its limit 98% of the time; the worst 174° (ForearmR). Fastest part with a standing leg within 0.5 m: 9.7 m/s median, 22.9 m/s at the 95th percentile (170 steps); with none: 0.5 m/s median (35 steps).

Look: within 25° of what it attended to 79% of the time standing (80% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 92% of the time standing; its rim came within 1.1 cm of its own body (negative: inside).

Fight Rhythm (?): 5 postures (GuardChange 1, HighFeint 2, ShieldAdvance 1, Peek 1), 2 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 1, launched 0, got up 1; thrown into others 4 times.


## A Large

Present 0.0–18.7 s. Steps 34, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 20 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 1 / 17 |
| Chest | ? | 0.5 / 6.3 | 1 / 9 |
| Head | ? | 0.6 / 7.4 | 9 / 26 |
| UpperArmL | ? | 0.9 / 11.0 | 20 / 50 |
| ForearmL | ? | 2.5 / 23.7 | 8 / 63 |
| HandL | ? | 3.9 / 45.4 | 22 / 94 |
| UpperArmR | ? | 0.7 / 8.4 | 17 / 59 |
| ForearmR | ? | 1.6 / 17.1 | 6 / 68 |
| HandR | ? | 2.9 / 51.7 | 19 / 79 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 13, 123 damage. Health at the end 0.

Defence: 12 blows came at it (8 seen clearly, 4 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 10.

Pose against the joint limits (standing): a joint more than 10° past its limit 20% of the time; the worst 48° (FootR).

Look: within 25° of what it attended to 87% of the time standing (87% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 3 postures (BoxerSettle 1, ClubFeint 1, BobAndWeave 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 4 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## B Small

Present 0.0–18.7 s. Steps 43, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 41 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 1.5 | 3 / 124 |
| Chest | ? | 0.5 / 15.3 | 3 / 82 |
| Head | ? | 0.6 / 14.9 | 10 / 108 |
| UpperArmL | ? | 0.8 / 22.9 | 6 / 65 |
| ForearmL | ? | 2.2 / 19.6 | 13 / 99 |
| HandL | ? | 2.6 / 36.0 | 23 / 102 |
| UpperArmR | ? | 0.2 / 2.1 | 2 / 80 |
| ForearmR | ? | 0.6 / 15.7 | 4 / 100 |
| HandR | ? | 1.1 / 24.5 | 11 / 106 |
| ThighL | ? | 0.0 / 0.0 | 0 / 29 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 148 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 16 damage. Hits taken: 4, 14 damage. Health at the end 0.

Defence: 1 blows came at it (0 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 59% of the time; the worst 79° (ShinR).
Ragdoll (on the ground 3.1 s): a joint more than 10° past its limit 80% of the time; the worst 162° (ForearmL). Fastest part with a standing leg within 0.5 m: 3.1 m/s median, 12.7 m/s at the 95th percentile (115 steps); with none: 3.5 m/s median (73 steps).

Look: within 25° of what it attended to 77% of the time standing (81% of the time it was within the head's reach, 94% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 4 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1, RedondaTwirl 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 0 times, knocked down 1, launched 1, got up 2; thrown into others 2 times.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.7 cm, max 0.8 cm (posed); physical weapon's closest pass mean 0.4 cm, max 0.5 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 9.70 s: ClubSwing at A Large's ElbowR: passed 0.7 cm from the aim point; nearest part A Large's Chest (24.0 cm); the ElbowR had moved 59.8 cm since the aim was locked
- 15.18 s: ClubBackhand at A Large's KneeR: passed 0.8 cm from the aim point; nearest part A Large's ThighR (30.9 cm); the KneeR had moved 41.5 cm since the aim was locked


## B Medium

Present 0.0–18.7 s. Steps 38, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 32 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 2 / 25 |
| Chest | ? | 0.6 / 7.4 | 2 / 38 |
| Head | ? | 0.7 / 14.0 | 15 / 54 |
| UpperArmL | ? | 0.9 / 16.0 | 5 / 163 |
| ForearmL | ? | 2.1 / 40.5 | 7 / 180 |
| HandL | ? | 2.5 / 35.6 | 5 / 103 |
| UpperArmR | ? | 0.5 / 14.2 | 5 / 97 |
| ForearmR | ? | 1.0 / 19.7 | 5 / 99 |
| HandR | ? | 1.4 / 25.6 | 12 / 142 |
| ThighL | ? | 0.0 / 0.0 | 0 / 52 |
| ShinL | ? | 0.0 / 14.1 | 0 / 52 |
| FootL | ? | 0.0 / 0.0 | 0 / 5 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 1 |

Hits dealt: 6 (1 on the part aimed at, 1 blocked, 1 on someone in the way, 0 on allies), 71 damage. Hits taken: 3, 9 damage. Health at the end 0.

Defence: 3 blows came at it (2 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 38% of the time; the worst 115° (FootL).

Look: within 25° of what it attended to 82% of the time standing (84% of the time it was within the head's reach, 93% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 85% of the time standing; its rim came within -3.6 cm of its own body (negative: inside).

Fight Rhythm (?): 6 postures (GuardChange 2, ShieldPress 2, HighFeint 2), 2 of them lead-ins to blows; energy at the end 0.00.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.3 cm, max 0.6 cm (posed); physical weapon's closest pass mean 1.2 cm, max 2.4 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.93 s: SwordBackhand at A Small's KneeR: passed 0.6 cm from the aim point; nearest part A Small's ShinL (6.0 cm); the KneeR had moved 10.4 cm since the aim was locked
- 8.92 s: SwordThrust at A Small's Chest: passed 0.1 cm from the aim point; nearest part A Small's Chest (9.8 cm); the Chest had moved 15.2 cm since the aim was locked
- 10.60 s: SwordBackhand at A Small's Chest: passed 0.3 cm from the aim point; nearest part A Small's UpperArmR (10.2 cm); the Chest had moved 28.2 cm since the aim was locked
- 12.42 s: SwordCut at A Small's Chest: passed 0.1 cm from the aim point; nearest part A Small's Chest (9.4 cm); the Chest had moved 9.2 cm since the aim was locked


## B Large

Present 0.0–18.7 s. Steps 39, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 51 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 2 / 17 |
| Chest | ? | 0.6 / 6.3 | 2 / 12 |
| Head | ? | 0.7 / 8.9 | 14 / 49 |
| UpperArmL | ? | 1.1 / 11.0 | 16 / 69 |
| ForearmL | ? | 4.2 / 29.6 | 12 / 87 |
| HandL | ? | 6.1 / 45.4 | 26 / 96 |
| UpperArmR | ? | 0.3 / 6.8 | 5 / 62 |
| ForearmR | ? | 0.8 / 21.4 | 5 / 68 |
| HandR | ? | 1.6 / 51.7 | 7 / 81 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 9 (2 on the part aimed at, 1 blocked, 0 on someone in the way, 1 on allies), 81 damage. Hits taken: 0, 0 damage. Health at the end 0.

Defence: 1 blows came at it (1 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 30% of the time; the worst 42° (FootR).

Look: within 25° of what it attended to 79% of the time standing (80% of the time it was within the head's reach, 97% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 4 postures (BoxerSettle 1, ClubFeint 2, JabFeint 1), 1 of them lead-ins to blows; energy at the end 0.00.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.4 cm, max 0.5 cm (posed); physical weapon's closest pass mean 1.1 cm, max 1.1 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.28 s: ClubSwing at A Medium's Head: passed 0.3 cm from the aim point; nearest part A Medium's Chest (38.6 cm); the Head had moved 56.0 cm since the aim was locked
- 8.13 s: ClubRising at A Large's KneeL: passed 0.5 cm from the aim point; nearest part A Large's ThighL (0.0 cm); the KneeL had moved 4.8 cm since the aim was locked
- 9.17 s: ClubSwing at A Large's Head: passed 0.5 cm from the aim point; nearest part A Large's HandR (1.5 cm); the Head had moved 23.9 cm since the aim was locked

## Shield contacts

37 times a shield overlapped another unit's body: 95% under 7.5 cm deep (59% under 2 cm; target 95%), the deepest 7.7 cm, the longest 0.08 s (target under 0.2 s). Each one is in events.log ("shield").

