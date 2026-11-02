# Playtest 2026-11-02_06-16-13

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 59.1 s of play (3545 physics steps), 2670 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-02_06-16-13`.

## A Small

Present 0.0–59.1 s. Steps 20, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 7 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.0 | 1 / 9 |
| Chest | Ragdoll | 0.3 / 1.7 | 2 / 7 |
| Head | Ragdoll | 0.4 / 2.5 | 9 / 75 |
| UpperArmL | Ragdoll | 0.4 / 2.3 | 6 / 73 |
| ForearmL | Ragdoll | 1.2 / 15.4 | 12 / 120 |
| HandL | Ragdoll | 3.2 / 36.0 | 21 / 155 |
| UpperArmR | Ragdoll | 0.4 / 2.6 | 7 / 92 |
| ForearmR | Ragdoll | 1.1 / 14.5 | 10 / 119 |
| HandR | Ragdoll | 2.6 / 31.5 | 20 / 102 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 3, 49 damage. Killed.

Defence: 3 blows came at it (1 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 64% of the time; the worst 42° (FootL).
Ragdoll (on the ground 52.8 s): a joint more than 10° past its limit 11% of the time; the worst 143° (ShinR). Fastest part with a standing leg within 0.5 m: 0.1 m/s median, 1.5 m/s at the 95th percentile (405 steps); with none: 0.0 m/s median (2761 steps).

Look: within 25° of what it attended to 75% of the time standing (76% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 2 postures (StickSettle 1, AbanicoFlurry 1), 0 of them lead-ins to blows; energy at the end 0.75.

Balance: stumbled 3 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–59.1 s. Steps 87, idle shuffles 5. Feet slid at most 0.0 cm while planted; the weight was at most 36 cm from the support point; hips dropped for reach by up to 6 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.3 | 2 / 36 |
| Chest | Ragdoll | 0.6 / 10.4 | 3 / 64 |
| Head | Ragdoll | 1.0 / 21.2 | 14 / 103 |
| UpperArmL | Ragdoll | 1.3 / 25.7 | 12 / 177 |
| ForearmL | Ragdoll | 3.8 / 50.1 | 14 / 179 |
| HandL | Ragdoll | 3.9 / 47.4 | 8 / 110 |
| UpperArmR | Ragdoll | 0.8 / 12.3 | 20 / 180 |
| ForearmR | Ragdoll | 1.8 / 18.1 | 11 / 150 |
| HandR | Ragdoll | 2.7 / 25.5 | 19 / 163 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 8 (1 on the part aimed at, 4 blocked, 0 on someone in the way, 1 on allies), 50 damage. Hits taken: 19, 108 damage. Killed.

Defence: 18 blows came at it (13 seen clearly, 5 at the edge of its view, 0 never seen); it raised the shield against 10.

Pose against the joint limits (standing): a joint more than 10° past its limit 31% of the time; the worst 154° (ShinR).
Ragdoll (on the ground 11.6 s): a joint more than 10° past its limit 95% of the time; the worst 163° (ShinR). Fastest part with a standing leg within 0.5 m: - median, - at the 95th percentile (0 steps); with none: 0.0 m/s median (696 steps).

Look: within 25° of what it attended to 87% of the time standing (88% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 95% of the time standing; its rim came within 1.0 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 5 postures (GuardChange 1, HighFeint 2, Peek 2), 0 of them lead-ins to blows; energy at the end 0.19.

Balance: stumbled 2 times, knocked down 0, launched 1, got up 1; thrown into others 0 times.


## A Large

Present 0.0–59.1 s. Steps 101, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 49 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 29.7 | 2 / 176 |
| Chest | ? | 0.8 / 37.0 | 2 / 134 |
| Head | ? | 1.1 / 56.9 | 17 / 154 |
| UpperArmL | ? | 1.3 / 70.9 | 30 / 177 |
| ForearmL | ? | 3.2 / 56.7 | 10 / 179 |
| HandL | ? | 4.2 / 78.7 | 25 / 179 |
| UpperArmR | ? | 0.6 / 13.6 | 17 / 86 |
| ForearmR | ? | 1.6 / 24.4 | 6 / 89 |
| HandR | ? | 2.7 / 51.7 | 17 / 118 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 5 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 59 damage. Hits taken: 21, 227 damage. Health at the end 0.

Defence: 27 blows came at it (12 seen clearly, 15 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 23.

Pose against the joint limits (standing): a joint more than 10° past its limit 32% of the time; the worst 94° (FootL).
Ragdoll (on the ground 4.8 s): a joint more than 10° past its limit 79% of the time; the worst 110° (HandL). Fastest part with a standing leg within 0.5 m: - median, - at the 95th percentile (0 steps); with none: 0.5 m/s median (287 steps).

Look: within 25° of what it attended to 84% of the time standing (85% of the time it was within the head's reach, 92% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 6 postures (BoxerSettle 1, ClubFeint 2, BobAndWeave 2, BoxerStalk 1), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 5 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.5 cm, max 0.8 cm (posed); physical weapon's closest pass mean 0.8 cm, max 1.4 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.85 s: ClubSmash at B Small's Neck: passed 0.4 cm from the aim point; nearest part B Small's UpperArmR (10.1 cm); the Neck had moved 30.2 cm since the aim was locked
- 9.12 s: ClubSmash at B Small's Head: passed 0.4 cm from the aim point; nearest part B Small's UpperArmR (10.5 cm); the Head had moved 36.0 cm since the aim was locked
- 29.75 s: ClubSwing at B Large's ElbowR: passed 0.4 cm from the aim point; nearest part B Large's HandR (49.5 cm); the ElbowR had moved 73.1 cm since the aim was locked
- 45.00 s: ClubRising at B Small's KneeR: passed 0.8 cm from the aim point; nearest part B Small's ShinR (15.9 cm); the KneeR had moved 16.7 cm since the aim was locked


## B Small

Present 0.0–59.1 s. Steps 136, idle shuffles 4. Feet slid at most 0.0 cm while planted; the weight was at most 23 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 4.3 | 2 / 62 |
| Chest | Pinned | 0.3 / 6.5 | 2 / 74 |
| Head | Pinned | 0.4 / 7.8 | 9 / 72 |
| UpperArmL | Pinned | 0.5 / 10.9 | 5 / 134 |
| ForearmL | Pinned | 1.4 / 16.7 | 8 / 169 |
| HandL | Pinned | 1.7 / 36.0 | 16 / 163 |
| UpperArmR | Pinned | 0.3 / 16.7 | 9 / 177 |
| ForearmR | Pinned | 0.7 / 22.9 | 6 / 152 |
| HandR | Pinned | 1.4 / 24.5 | 13 / 106 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 7 (3 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 60 damage. Hits taken: 5, 51 damage. Health at the end 9.

Defence: 6 blows came at it (2 seen clearly, 4 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 4.

Pose against the joint limits (standing): a joint more than 10° past its limit 57% of the time; the worst 51° (FootL).
Ragdoll (on the ground 5.7 s): a joint more than 10° past its limit 89% of the time; the worst 155° (HandR). Fastest part with a standing leg within 0.5 m: 3.6 m/s median, 8.4 m/s at the 95th percentile (162 steps); with none: 1.7 m/s median (180 steps).

Look: within 25° of what it attended to 80% of the time standing (83% of the time it was within the head's reach, 95% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 13 postures (StickSettle 2, AbanicoFlurry 5, BaitRetreat 1, RedondaTwirl 4, TriangleStep 1), 1 of them lead-ins to blows; energy at the end 0.53.

Balance: stumbled 1 time, knocked down 1, launched 3, got up 4; thrown into others 1 time.

Blows: 9. The posed striking part passed within 3 cm of the aim point in 9; aim error mean 0.6 cm, max 1.0 cm (posed); physical weapon's closest pass mean 0.7 cm, max 1.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 16.78 s: ClubSwing at A Large's KneeL: passed 1.0 cm from the aim point; nearest part A Large's ThighL (7.2 cm); the KneeL had moved 2.7 cm since the aim was locked
- 20.25 s: ClubSwing at A Large's ElbowL: passed 0.4 cm from the aim point; nearest part A Large's ForearmR (36.2 cm); the ElbowL had moved 52.0 cm since the aim was locked
- 20.70 s: ClubRising at A Large's KneeL: passed 0.5 cm from the aim point; nearest part A Large's ThighL (0.0 cm); the KneeL had moved 8.0 cm since the aim was locked
- 22.85 s: ClubSwing at A Large's KneeL: passed 0.6 cm from the aim point; nearest part A Large's ShinL (11.4 cm); the KneeL had moved 18.4 cm since the aim was locked
- 41.13 s: ClubRising at A Large's KneeL: passed 0.5 cm from the aim point; nearest part A Large's ThighL (8.2 cm); the KneeL had moved 20.2 cm since the aim was locked
- 51.43 s: ClubSwing at A Large's ElbowL: passed 0.5 cm from the aim point; nearest part A Large's ForearmL (18.2 cm); the ElbowL had moved 35.9 cm since the aim was locked
- 52.62 s: ClubSwing at A Large's ElbowL: passed 0.2 cm from the aim point; nearest part A Large's ForearmL (23.7 cm); the ElbowL had moved 25.2 cm since the aim was locked
- 53.07 s: ClubRising at A Large's KneeR: passed 0.5 cm from the aim point; nearest part A Large's ThighR (27.2 cm); the KneeR had moved 20.1 cm since the aim was locked
- 54.65 s: ClubSwing at A Large's KneeR: passed 0.6 cm from the aim point; nearest part A Large's ShinR (37.8 cm); the KneeR had moved 46.7 cm since the aim was locked


## B Medium

Present 0.0–59.1 s. Steps 122, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 29 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 2 / 26 |
| Chest | Pinned | 0.5 / 7.6 | 2 / 64 |
| Head | Pinned | 0.6 / 21.1 | 10 / 81 |
| UpperArmL | Pinned | 0.8 / 31.1 | 7 / 179 |
| ForearmL | Pinned | 2.3 / 53.4 | 8 / 171 |
| HandL | Pinned | 2.9 / 47.9 | 5 / 83 |
| UpperArmR | Pinned | 0.4 / 15.4 | 7 / 97 |
| ForearmR | Pinned | 0.7 / 19.7 | 4 / 152 |
| HandR | Pinned | 1.1 / 25.6 | 9 / 115 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 19 (5 on the part aimed at, 12 blocked, 0 on someone in the way, 1 on allies), 118 damage. Hits taken: 9, 75 damage. Health at the end 25.

Defence: 7 blows came at it (5 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 4.

Pose against the joint limits (standing): a joint more than 10° past its limit 29% of the time; the worst 112° (FootL).
Ragdoll (on the ground 3.0 s): a joint more than 10° past its limit 92% of the time; the worst 174° (ForearmL). Fastest part with a standing leg within 0.5 m: 4.9 m/s median, 15.6 m/s at the 95th percentile (23 steps); with none: 8.4 m/s median (155 steps).

Look: within 25° of what it attended to 86% of the time standing (86% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 92% of the time standing; its rim came within -0.5 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 14 postures (GuardChange 1, ShieldSettle 3, Peek 2, ShieldPress 2, HighFeint 4, ShieldAdvance 2), 2 of them lead-ins to blows; energy at the end 1.00.

Balance: stumbled 1 time, knocked down 1, launched 0, got up 1; thrown into others 2 times.

Blows: 7. The posed striking part passed within 3 cm of the aim point in 7; aim error mean 0.3 cm, max 0.8 cm (posed); physical weapon's closest pass mean 0.3 cm, max 0.5 cm. 2 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.93 s: SwordCut at A Small's Neck: passed 0.1 cm from the aim point; nearest part A Small's UpperArmR (23.3 cm); the Neck had moved 40.9 cm since the aim was locked
- 4.63 s: SwordBackhand at A Small's Chest: passed 0.1 cm from the aim point; nearest part A Small's Pelvis (4.7 cm); the Chest had moved 20.7 cm since the aim was locked
- 5.53 s: SwordCleave at A Small's Chest: passed 0.2 cm from the aim point; nearest part A Small's Chest (4.3 cm); the Chest had moved 21.4 cm since the aim was locked
- 33.48 s: SwordCut at A Medium's Neck: passed 0.3 cm from the aim point; nearest part A Medium's Chest (0.0 cm); the Neck had moved 5.1 cm since the aim was locked
- 46.13 s: SwordCut at A Medium's Neck: passed 0.3 cm from the aim point; nearest part A Medium's Chest (3.2 cm); the Neck had moved 7.8 cm since the aim was locked
- 47.43 s: SwordCut at A Medium's Neck: passed 0.3 cm from the aim point; nearest part A Medium's Chest (3.0 cm); the Neck had moved 18.6 cm since the aim was locked
- 49.20 s: SwordThrust at A Medium's Head: passed 0.8 cm from the aim point; nearest part A Medium's Head (0.5 cm); the Head had moved 8.6 cm since the aim was locked


## B Large

Present 0.0–59.1 s. Steps 108, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 50 cm from the support point; hips dropped for reach by up to 4 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.5 | 1 / 36 |
| Chest | ? | 0.5 / 17.0 | 1 / 25 |
| Head | ? | 0.7 / 11.5 | 10 / 93 |
| UpperArmL | ? | 0.9 / 16.2 | 21 / 123 |
| ForearmL | ? | 3.2 / 53.1 | 9 / 150 |
| HandL | ? | 4.7 / 45.4 | 22 / 105 |
| UpperArmR | ? | 0.5 / 8.5 | 9 / 120 |
| ForearmR | ? | 1.3 / 28.3 | 5 / 78 |
| HandR | ? | 2.4 / 51.7 | 11 / 119 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 17 (3 on the part aimed at, 2 blocked, 0 on someone in the way, 2 on allies), 182 damage. Hits taken: 5, 36 damage. Health at the end 0.

Defence: 9 blows came at it (6 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 23% of the time; the worst 92° (ShinL).

Look: within 25° of what it attended to 84% of the time standing (84% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 13 postures (BoxerSettle 1, BoxerStalk 3, BobAndWeave 3, ClubFeint 3, Shift 1, JabFeint 2), 3 of them lead-ins to blows; energy at the end 0.00.

Blows: 7. The posed striking part passed within 3 cm of the aim point in 7; aim error mean 0.3 cm, max 0.5 cm (posed); physical weapon's closest pass mean 0.1 cm, max 0.1 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 27.90 s: ClubSmash at A Large's Chest: passed 0.2 cm from the aim point; nearest part A Large's HandR (13.2 cm); the Chest had moved 37.5 cm since the aim was locked
- 31.27 s: ClubRising at A Large's Chest: passed 0.3 cm from the aim point; nearest part A Large's Chest (23.8 cm); the Chest had moved 68.9 cm since the aim was locked
- 38.95 s: ClubSmash at A Large's Head: passed 0.5 cm from the aim point; nearest part A Large's ForearmL (21.6 cm); the Head had moved 43.2 cm since the aim was locked
- 42.95 s: ClubRising at A Large's Belly: passed 0.2 cm from the aim point; nearest part A Large's Chest (26.9 cm); the Belly had moved 26.0 cm since the aim was locked
- 45.45 s: ClubSmash at A Large's Neck: passed 0.2 cm from the aim point; nearest part A Large's UpperArmR (47.6 cm); the Neck had moved 88.1 cm since the aim was locked
- 50.82 s: ClubSwing at A Large's Head: passed 0.3 cm from the aim point; nearest part A Large's HandR (11.9 cm); the Head had moved 38.2 cm since the aim was locked
- 54.25 s: ClubSwing at A Large's Belly: passed 0.4 cm from the aim point; nearest part A Large's ForearmR (29.5 cm); the Belly had moved 47.6 cm since the aim was locked

## Shield contacts

90 times a shield overlapped another unit's body: 95% under 4.5 cm deep (80% under 2 cm; target 95%), the deepest 6.3 cm, the longest 0.13 s (target under 0.2 s). Each one is in events.log ("shield").

