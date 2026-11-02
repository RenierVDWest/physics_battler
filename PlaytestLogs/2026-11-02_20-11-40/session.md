# Playtest 2026-11-02_20-11-40

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 63.2 s of play (3790 physics steps), 2468 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-02_20-11-40`.

## A Small

Present 0.0–63.2 s. Steps 16, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 9 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.1 | 4 / 18 |
| Chest | Ragdoll | 0.7 / 3.6 | 8 / 31 |
| Head | Ragdoll | 1.5 / 5.9 | 10 / 37 |
| UpperArmL | Ragdoll | 1.1 / 5.4 | 7 / 54 |
| ForearmL | Ragdoll | 1.9 / 14.6 | 11 / 97 |
| HandL | Ragdoll | 2.9 / 36.4 | 23 / 83 |
| UpperArmR | Ragdoll | 1.5 / 5.8 | 8 / 49 |
| ForearmR | Ragdoll | 2.8 / 19.5 | 15 / 78 |
| HandR | Ragdoll | 6.9 / 32.3 | 19 / 99 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 3, 74 damage. Killed.

Defence: 3 blows came at it (3 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 64% of the time; the worst 42° (FootL).
Ragdoll (on the ground 57.5 s): a joint more than 10° past its limit 7% of the time; the worst 143° (ShinR). Fastest part with a standing leg within 0.5 m: 0.2 m/s median, 3.2 m/s at the 95th percentile (353 steps); with none: 0.0 m/s median (3096 steps).

Look: within 25° of what it attended to 80% of the time standing (81% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 2 postures (StickSettle 1, AbanicoFlurry 1), 0 of them lead-ins to blows; energy at the end 0.71.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–63.2 s. Steps 112, idle shuffles 7. Feet slid at most 0.0 cm while planted; the weight was at most 50 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 15.8 | 3 / 176 |
| Chest | Pinned | 0.8 / 31.5 | 5 / 178 |
| Head | Pinned | 1.2 / 50.1 | 12 / 179 |
| UpperArmL | Pinned | 1.5 / 68.5 | 7 / 173 |
| ForearmL | Pinned | 2.6 / 78.5 | 8 / 126 |
| HandL | Pinned | 3.9 / 86.0 | 9 / 179 |
| UpperArmR | Muscled | 1.2 / 21.0 | 4 / 62 |
| ForearmR | Muscled | 2.4 / 41.3 | 6 / 114 |
| HandR | Muscled | 3.6 / 65.7 | 8 / 144 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 149 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 150 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 10 (2 on the part aimed at, 5 blocked, 0 on someone in the way, 0 on allies), 70 damage. Hits taken: 13, 79 damage. Health at the end 21.

Defence: 13 blows came at it (10 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 7.

Pose against the joint limits (standing): a joint more than 10° past its limit 27% of the time; the worst 144° (ShinR).
Ragdoll (on the ground 1.3 s): a joint more than 10° past its limit 93% of the time; the worst 105° (ForearmL). Fastest part with a standing leg within 0.5 m: 9.3 m/s median, 11.1 m/s at the 95th percentile (24 steps); with none: 2.4 m/s median (52 steps).

Look: within 25° of what it attended to 87% of the time standing (88% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 91% of the time standing; its rim came within 1.0 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 8 postures (GuardChange 1, HighFeint 2, Peek 1, ShieldSettle 1, ShieldPress 2, ShieldAdvance 1), 2 of them lead-ins to blows; energy at the end 0.99.

Balance: stumbled 2 times, knocked down 1, launched 0, got up 1; thrown into others 0 times.

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.6 cm, max 1.2 cm (posed); physical weapon's closest pass mean 0.3 cm, max 0.4 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 19.13 s: SwordBackhand at B Medium's Chest: passed 1.2 cm from the aim point; nearest part B Medium's HandL (12.9 cm); the Chest had moved 49.8 cm since the aim was locked
- 30.28 s: SwordCut at B Medium's Belly: passed 0.0 cm from the aim point; nearest part B Medium's UpperArmL (82.4 cm); the Belly had moved 98.1 cm since the aim was locked
- 35.90 s: SwordCut at B Medium's Neck: passed 0.5 cm from the aim point; nearest part B Medium's UpperArmR (17.0 cm); the Neck had moved 39.2 cm since the aim was locked
- 45.82 s: SwordBackhand at B Large's Belly: passed 1.1 cm from the aim point; nearest part B Large's Spine (8.9 cm); the Belly had moved 49.0 cm since the aim was locked
- 53.98 s: SwordCut at B Large's Chest: passed 0.0 cm from the aim point; nearest part B Large's Chest (0.0 cm); the Chest had moved 21.2 cm since the aim was locked


## A Large

Present 0.0–63.2 s. Steps 94, idle shuffles 4. Feet slid at most 0.0 cm while planted; the weight was at most 53 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 2 / 23 |
| Chest | ? | 0.8 / 10.0 | 2 / 54 |
| Head | ? | 1.1 / 25.1 | 11 / 178 |
| UpperArmL | ? | 1.3 / 28.2 | 4 / 130 |
| ForearmL | ? | 2.8 / 54.4 | 7 / 122 |
| HandL | ? | 4.2 / 45.0 | 17 / 111 |
| UpperArmR | ? | 1.0 / 11.1 | 2 / 55 |
| ForearmR | ? | 2.2 / 20.2 | 4 / 64 |
| HandR | ? | 3.0 / 52.0 | 5 / 79 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 150 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 1 |

Hits dealt: 9 (5 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 108 damage. Hits taken: 12, 146 damage. Health at the end 0.

Defence: 20 blows came at it (15 seen clearly, 5 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 14.

Pose against the joint limits (standing): a joint more than 10° past its limit 20% of the time; the worst 81° (ShinR).

Look: within 25° of what it attended to 91% of the time standing (91% of the time it was within the head's reach, 97% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 5 postures (BoxerSettle 2, BobAndWeave 2, ClubFeint 1), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 3 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 7. The posed striking part passed within 3 cm of the aim point in 6; aim error mean 0.8 cm, max 4.9 cm (posed); physical weapon's closest pass mean 2.3 cm, max 8.6 cm. 2 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.65 s: ClubRising at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's HandR (21.3 cm); the Belly had moved 27.6 cm since the aim was locked
- 4.50 s: ClubSwing at B Small's Head: passed 0.5 cm from the aim point; nearest part B Small's FootR (43.0 cm); the Head had moved 68.5 cm since the aim was locked
- 5.12 s: ClubBackhand at B Small's Chest: passed 4.9 cm from the aim point; nearest part B Small's Chest (1.6 cm); the Chest had moved 7.2 cm since the aim was locked
- 11.00 s: ClubSwing at B Small's Head: passed 0.0 cm from the aim point; nearest part B Small's Head (0.0 cm); the Head had moved 4.6 cm since the aim was locked
- 13.03 s: ClubBackhand at B Large's Neck: passed 0.0 cm from the aim point; nearest part B Large's Chest (65.8 cm); the Neck had moved 111.0 cm since the aim was locked
- 15.02 s: ClubBackhand at B Large's Neck: passed 0.0 cm from the aim point; nearest part B Large's UpperArmL (30.8 cm); the Neck had moved 53.2 cm since the aim was locked
- 20.17 s: ClubSwing at B Small's Head: passed 0.1 cm from the aim point; nearest part B Small's Pelvis (35.3 cm); the Head had moved 70.5 cm since the aim was locked


## B Small

Present 0.0–63.2 s. Steps 39, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 33 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.1 | 4 / 33 |
| Chest | Ragdoll | 0.7 / 6.5 | 6 / 35 |
| Head | Ragdoll | 1.2 / 8.1 | 13 / 131 |
| UpperArmL | Ragdoll | 1.1 / 7.9 | 5 / 52 |
| ForearmL | Ragdoll | 1.4 / 14.4 | 7 / 96 |
| HandL | Ragdoll | 2.1 / 36.3 | 19 / 86 |
| UpperArmR | Ragdoll | 1.1 / 8.7 | 7 / 43 |
| ForearmR | Ragdoll | 2.3 / 17.7 | 12 / 84 |
| HandR | Ragdoll | 5.1 / 38.6 | 14 / 99 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 11 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 4 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 1 |

Hits dealt: 1 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 10 damage. Hits taken: 6, 60 damage. Killed.

Defence: 5 blows came at it (2 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 57% of the time; the worst 72° (FootL).
Ragdoll (on the ground 44.5 s): a joint more than 10° past its limit 10% of the time; the worst 163° (ForearmR). Fastest part with a standing leg within 0.5 m: 0.0 m/s median, 7.4 m/s at the 95th percentile (726 steps); with none: 0.0 m/s median (1942 steps).

Look: within 25° of what it attended to 83% of the time standing (86% of the time it was within the head's reach, 93% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.51.

Balance: stumbled 0 times, knocked down 3, launched 1, got up 3; thrown into others 1 time.


## B Medium

Present 0.0–63.2 s. Steps 102, idle shuffles 5. Feet slid at most 0.0 cm while planted; the weight was at most 56 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 15.8 | 3 / 176 |
| Chest | Ragdoll | 0.7 / 53.9 | 4 / 171 |
| Head | Ragdoll | 1.0 / 64.0 | 11 / 167 |
| UpperArmL | Ragdoll | 1.2 / 51.8 | 7 / 180 |
| ForearmL | Ragdoll | 2.9 / 52.8 | 8 / 177 |
| HandL | Ragdoll | 4.1 / 70.6 | 7 / 161 |
| UpperArmR | Ragdoll | 0.6 / 12.5 | 2 / 38 |
| ForearmR | Ragdoll | 1.3 / 20.7 | 4 / 73 |
| HandR | Ragdoll | 2.1 / 41.9 | 4 / 100 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 64 |
| ShinL | Ragdoll | 0.0 / 6.8 | 0 / 64 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 3 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 150 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 13 (2 on the part aimed at, 4 blocked, 0 on someone in the way, 0 on allies), 130 damage. Hits taken: 7, 14 damage. Killed.

Defence: 7 blows came at it (5 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 23% of the time; the worst 123° (ThighR).
Ragdoll (on the ground 11.4 s): a joint more than 10° past its limit 4% of the time; the worst 85° (ShinR). Fastest part with a standing leg within 0.5 m: - median, - at the 95th percentile (0 steps); with none: 0.0 m/s median (685 steps).

Look: within 25° of what it attended to 88% of the time standing (89% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 92% of the time standing; its rim came within 1.0 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 16 postures (GuardChange 2, ShieldSettle 1, ShieldPress 4, Peek 3, HighFeint 3, ShieldAdvance 3), 2 of them lead-ins to blows; energy at the end 1.00.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.4 cm, max 0.7 cm (posed); physical weapon's closest pass mean 0.6 cm, max 0.8 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.30 s: SwordCut at A Small's Head: passed 0.7 cm from the aim point; nearest part A Small's Head (5.7 cm); the Head had moved 15.8 cm since the aim was locked
- 4.95 s: SwordBackhand at A Small's Chest: passed 0.4 cm from the aim point; nearest part A Small's Chest (0.0 cm); the Chest had moved 6.2 cm since the aim was locked
- 5.67 s: SwordCleave at A Small's Chest: passed 0.7 cm from the aim point; nearest part A Small's Chest (12.8 cm); the Chest had moved 5.0 cm since the aim was locked
- 10.18 s: SwordCut at A Medium's Neck: passed 0.0 cm from the aim point; nearest part A Medium's Chest (5.1 cm); the Neck had moved 30.1 cm since the aim was locked
- 30.25 s: SwordCut at A Medium's KneeR: passed 0.1 cm from the aim point; nearest part A Medium's ShinR (20.4 cm); the KneeR had moved 38.3 cm since the aim was locked


## B Large

Present 0.0–63.2 s. Steps 115, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 64 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.2 | 2 / 41 |
| Chest | Ragdoll | 1.0 / 16.8 | 3 / 55 |
| Head | Ragdoll | 1.4 / 18.1 | 12 / 82 |
| UpperArmL | Ragdoll | 1.6 / 20.2 | 6 / 107 |
| ForearmL | Ragdoll | 3.7 / 45.6 | 9 / 133 |
| HandL | Ragdoll | 5.3 / 46.6 | 20 / 106 |
| UpperArmR | Ragdoll | 1.6 / 47.5 | 3 / 63 |
| ForearmR | Ragdoll | 2.9 / 75.1 | 4 / 64 |
| HandR | Ragdoll | 3.6 / 81.2 | 5 / 79 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 150 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 16 (4 on the part aimed at, 2 blocked, 0 on someone in the way, 1 on allies), 165 damage. Hits taken: 10, 120 damage. Killed.

Defence: 11 blows came at it (7 seen clearly, 4 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 8.

Pose against the joint limits (standing): a joint more than 10° past its limit 22% of the time; the worst 119° (ShinR).
Ragdoll (on the ground 2.4 s): a joint more than 10° past its limit 75% of the time; the worst 74° (HandL). Fastest part with a standing leg within 0.5 m: - median, - at the 95th percentile (0 steps); with none: 2.2 m/s median (142 steps).

Look: within 25° of what it attended to 85% of the time standing (86% of the time it was within the head's reach, 95% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 17 postures (BoxerSettle 3, BoxerStalk 3, BobAndWeave 4, JabFeint 2, ClubFeint 5), 7 of them lead-ins to blows; energy at the end 0.69.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.7 cm, max 1.9 cm (posed); physical weapon's closest pass mean 7.2 cm, max 12.2 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.97 s: ClubSwing at A Medium's Chest: passed 0.1 cm from the aim point; nearest part A Medium's UpperArmR (17.7 cm); the Chest had moved 53.1 cm since the aim was locked
- 11.13 s: ClubRising at A Medium's KneeR: passed 0.3 cm from the aim point; nearest part A Medium's ThighR (9.4 cm); the KneeR had moved 16.7 cm since the aim was locked
- 22.95 s: ClubRising at A Large's KneeL: passed 1.9 cm from the aim point; nearest part B Small's Chest (4.8 cm); the KneeL had moved 32.7 cm since the aim was locked

## Shield contacts

170 times a shield overlapped another unit's body: 95% under 8.8 cm deep (73% under 2 cm; target 95%), the deepest 14.2 cm, the longest 0.30 s (target under 0.2 s). Each one is in events.log ("shield").

