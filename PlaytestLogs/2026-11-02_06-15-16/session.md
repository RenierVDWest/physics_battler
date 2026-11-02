# Playtest 2026-11-02_06-15-16

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 52.3 s of play (3139 physics steps), 3385 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-02_06-15-16`.

## A Small

Present 0.0–52.3 s. Steps 25, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 7 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.0 | 2 / 21 |
| Chest | Ragdoll | 0.3 / 4.1 | 2 / 29 |
| Head | Ragdoll | 0.4 / 5.5 | 9 / 96 |
| UpperArmL | Ragdoll | 0.5 / 7.0 | 7 / 93 |
| ForearmL | Ragdoll | 1.5 / 26.9 | 10 / 119 |
| HandL | Ragdoll | 3.0 / 50.6 | 18 / 160 |
| UpperArmR | Ragdoll | 0.5 / 5.4 | 10 / 92 |
| ForearmR | Ragdoll | 1.0 / 14.5 | 10 / 119 |
| HandR | Ragdoll | 2.3 / 31.5 | 20 / 131 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 4, 74 damage. Killed.

Defence: 4 blows came at it (1 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 54% of the time; the worst 42° (FootL).
Ragdoll (on the ground 44.3 s): a joint more than 10° past its limit 99% of the time; the worst 174° (ForearmR). Fastest part with a standing leg within 0.5 m: 0.1 m/s median, 4.5 m/s at the 95th percentile (407 steps); with none: 0.0 m/s median (2250 steps).

Look: within 25° of what it attended to 74% of the time standing (75% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.63.

Balance: stumbled 3 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–52.3 s. Steps 104, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 46 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 29.1 | 4 / 170 |
| Chest | Ragdoll | 1.1 / 54.5 | 5 / 153 |
| Head | Ragdoll | 1.5 / 39.1 | 19 / 128 |
| UpperArmL | Ragdoll | 2.1 / 58.1 | 13 / 178 |
| ForearmL | Ragdoll | 5.3 / 76.1 | 15 / 180 |
| HandL | Ragdoll | 6.7 / 76.5 | 11 / 178 |
| UpperArmR | Ragdoll | 1.0 / 21.9 | 23 / 176 |
| ForearmR | Ragdoll | 2.2 / 23.1 | 12 / 176 |
| HandR | Ragdoll | 2.9 / 25.3 | 22 / 178 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 11 (3 on the part aimed at, 1 blocked, 1 on someone in the way, 0 on allies), 142 damage. Hits taken: 11, 104 damage. Killed.

Defence: 8 blows came at it (2 seen clearly, 6 at the edge of its view, 0 never seen); it raised the shield against 4.

Pose against the joint limits (standing): a joint more than 10° past its limit 29% of the time; the worst 165° (ShinL).
Ragdoll (on the ground 6.6 s): a joint more than 10° past its limit 84% of the time; the worst 161° (ForearmL). Fastest part with a standing leg within 0.5 m: 1.2 m/s median, 9.4 m/s at the 95th percentile (239 steps); with none: 5.5 m/s median (157 steps).

Look: within 25° of what it attended to 80% of the time standing (83% of the time it was within the head's reach, 97% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 92% of the time standing; its rim came within 1.0 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 11 postures (GuardChange 3, Peek 3, HighFeint 3, ShieldPress 1, ShieldAdvance 1), 3 of them lead-ins to blows; energy at the end 0.82.

Balance: stumbled 1 time, knocked down 1, launched 1, got up 2; thrown into others 1 time.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.3 cm, max 0.4 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.12 s: SwordBackhand at B Large's Chest: passed 0.4 cm from the aim point; nearest part B Large's Chest (20.1 cm); the Chest had moved 20.0 cm since the aim was locked
- 5.02 s: SwordCleave at B Large's Chest: passed 0.3 cm from the aim point; nearest part B Large's UpperArmL (6.7 cm); the Chest had moved 52.3 cm since the aim was locked
- 30.53 s: SwordThrust at B Large's Chest: passed 0.1 cm from the aim point; nearest part B Large's Chest (93.8 cm); the Chest had moved 130.9 cm since the aim was locked
- 43.95 s: SwordCut at B Medium's Head: passed 0.4 cm from the aim point; nearest part B Medium's Head (13.8 cm); the Head had moved 35.7 cm since the aim was locked


## A Large

Present 0.0–52.3 s. Steps 65, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 70 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 1.4 | 2 / 85 |
| Chest | Ragdoll | 0.6 / 33.8 | 2 / 83 |
| Head | Ragdoll | 0.8 / 30.5 | 14 / 157 |
| UpperArmL | Ragdoll | 1.1 / 57.6 | 30 / 160 |
| ForearmL | Ragdoll | 3.4 / 70.5 | 11 / 179 |
| HandL | Ragdoll | 4.6 / 97.0 | 22 / 165 |
| UpperArmR | Ragdoll | 0.6 / 48.7 | 18 / 117 |
| ForearmR | Ragdoll | 1.2 / 61.1 | 4 / 87 |
| HandR | Ragdoll | 2.2 / 99.2 | 16 / 99 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 8 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 1 on allies), 74 damage. Hits taken: 20, 220 damage. Killed.

Defence: 24 blows came at it (12 seen clearly, 12 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 22.

Pose against the joint limits (standing): a joint more than 10° past its limit 23% of the time; the worst 111° (FootL).
Ragdoll (on the ground 7.6 s): a joint more than 10° past its limit 95% of the time; the worst 171° (ShinR). Fastest part with a standing leg within 0.5 m: 2.8 m/s median, 2.8 m/s at the 95th percentile (1 steps); with none: 0.0 m/s median (455 steps).

Look: within 25° of what it attended to 80% of the time standing (83% of the time it was within the head's reach, 88% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 7 postures (BoxerSettle 2, BobAndWeave 3, ClubFeint 2), 2 of them lead-ins to blows; energy at the end 0.49.

Blows: 6. The posed striking part passed within 3 cm of the aim point in 6; aim error mean 0.3 cm, max 0.5 cm (posed); physical weapon's closest pass mean 3.9 cm, max 10.4 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.55 s: ClubRising at B Small's KneeL: passed 0.5 cm from the aim point; nearest part B Small's ThighR (5.6 cm); the KneeL had moved 27.6 cm since the aim was locked
- 8.02 s: ClubRising at B Small's Chest: passed 0.3 cm from the aim point; nearest part B Small's Chest (3.5 cm); the Chest had moved 18.6 cm since the aim was locked
- 8.97 s: ClubSwing at B Small's Neck: passed 0.3 cm from the aim point; nearest part B Small's Head (103.6 cm); the Neck had moved 138.3 cm since the aim was locked
- 9.82 s: ClubSmash at B Small's Chest: passed 0.2 cm from the aim point; nearest part B Small's UpperArmR (22.1 cm); the Chest had moved 43.7 cm since the aim was locked
- 12.12 s: ClubSwing at B Large's Head: passed 0.3 cm from the aim point; nearest part B Large's Chest (16.4 cm); the Head had moved 24.8 cm since the aim was locked
- 34.17 s: ClubSmash at B Small's Chest: passed 0.3 cm from the aim point; nearest part B Small's UpperArmR (8.2 cm); the Chest had moved 28.7 cm since the aim was locked


## B Small

Present 0.0–52.3 s. Steps 118, idle shuffles 5. Feet slid at most 0.0 cm while planted; the weight was at most 20 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 2 / 55 |
| Chest | Pinned | 0.3 / 10.4 | 2 / 121 |
| Head | Pinned | 0.5 / 21.8 | 9 / 80 |
| UpperArmL | Pinned | 0.6 / 29.8 | 8 / 180 |
| ForearmL | Pinned | 1.8 / 40.7 | 10 / 171 |
| HandL | Pinned | 2.4 / 36.0 | 20 / 176 |
| UpperArmR | Pinned | 0.3 / 12.1 | 4 / 80 |
| ForearmR | Pinned | 0.6 / 25.4 | 4 / 176 |
| HandR | Pinned | 1.2 / 32.8 | 11 / 170 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 12 (4 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 106 damage. Hits taken: 5, 43 damage. Health at the end 17.

Defence: 6 blows came at it (1 seen clearly, 5 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 48% of the time; the worst 127° (ShinR).
Ragdoll (on the ground 8.1 s): a joint more than 10° past its limit 94% of the time; the worst 174° (ShinR). Fastest part with a standing leg within 0.5 m: 4.0 m/s median, 19.8 m/s at the 95th percentile (193 steps); with none: 4.8 m/s median (291 steps).

Look: within 25° of what it attended to 78% of the time standing (81% of the time it was within the head's reach, 94% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 11 postures (StickSettle 2, AbanicoFlurry 4, BaitRetreat 1, TriangleStep 1, StickStalk 2, RedondaTwirl 1), 1 of them lead-ins to blows; energy at the end 0.63.

Balance: stumbled 0 times, knocked down 0, launched 3, got up 3; thrown into others 1 time.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.6 cm, max 0.6 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 44.68 s: ClubSwing at A Large's KneeR: passed 0.6 cm from the aim point; nearest part A Large's ThighR (0.0 cm); the KneeR had moved 14.6 cm since the aim was locked


## B Medium

Present 0.0–52.3 s. Steps 127, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 46 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 4 / 74 |
| Chest | Pinned | 1.0 / 18.5 | 4 / 96 |
| Head | Pinned | 1.4 / 40.9 | 17 / 169 |
| UpperArmL | Pinned | 1.5 / 37.1 | 7 / 149 |
| ForearmL | Pinned | 2.2 / 25.9 | 8 / 175 |
| HandL | Pinned | 2.5 / 32.7 | 5 / 93 |
| UpperArmR | Pinned | 1.3 / 31.8 | 16 / 176 |
| ForearmR | Pinned | 2.1 / 43.1 | 10 / 176 |
| HandR | Pinned | 2.5 / 33.1 | 14 / 176 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 19 (6 on the part aimed at, 2 blocked, 2 on someone in the way, 3 on allies), 241 damage. Hits taken: 4, 51 damage. Health at the end 49.

Defence: 2 blows came at it (1 seen clearly, 0 at the edge of its view, 1 never seen); it raised the shield against 0.

Pose against the joint limits (standing): a joint more than 10° past its limit 30% of the time; the worst 103° (ShinR).

Look: within 25° of what it attended to 79% of the time standing (80% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 97% of the time standing; its rim came within -0.6 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 14 postures (GuardChange 1, ShieldSettle 1, HighFeint 4, Peek 3, ShieldPress 3, ShieldAdvance 2), 2 of them lead-ins to blows; energy at the end 0.56.

Blows: 9. The posed striking part passed within 3 cm of the aim point in 9; aim error mean 0.3 cm, max 1.1 cm (posed); physical weapon's closest pass mean 0.2 cm, max 0.4 cm. 3 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.08 s: SwordCut at A Small's Chest: passed 0.2 cm from the aim point; nearest part A Small's Pelvis (12.0 cm); the Chest had moved 31.0 cm since the aim was locked
- 4.68 s: SwordBackhand at A Small's Chest: passed 0.2 cm from the aim point; nearest part A Small's Chest (2.1 cm); the Chest had moved 10.6 cm since the aim was locked
- 5.58 s: SwordCleave at A Small's Belly: passed 0.3 cm from the aim point; nearest part A Small's UpperArmL (2.4 cm); the Belly had moved 11.9 cm since the aim was locked
- 8.02 s: SwordCut at A Small's Head: passed 0.1 cm from the aim point; nearest part A Small's Head (3.7 cm); the Head had moved 4.5 cm since the aim was locked
- 10.77 s: SwordCut at A Medium's Head: passed 0.2 cm from the aim point; nearest part B Large's HandR (61.2 cm); the Head had moved 239.2 cm since the aim was locked
- 30.68 s: SwordThrust at A Medium's Chest: passed 0.8 cm from the aim point; nearest part A Medium's Chest (44.7 cm); the Chest had moved 70.2 cm since the aim was locked
- 40.40 s: SwordThrust at A Large's Belly: passed 0.1 cm from the aim point; nearest part A Large's Spine (11.9 cm); the Belly had moved 16.8 cm since the aim was locked
- 41.58 s: SwordThrust at A Large's Belly: passed 1.1 cm from the aim point; nearest part A Large's Spine (1.7 cm); the Belly had moved 4.7 cm since the aim was locked
- 49.27 s: SwordThrust at A Medium's Belly: passed 0.1 cm from the aim point; nearest part A Medium's HandL (21.6 cm); the Belly had moved 64.5 cm since the aim was locked


## B Large

Present 0.0–52.3 s. Steps 41, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 49 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 4.6 | 1 / 36 |
| Chest | Ragdoll | 0.5 / 14.0 | 1 / 26 |
| Head | Ragdoll | 0.7 / 17.7 | 10 / 66 |
| UpperArmL | Ragdoll | 0.9 / 22.7 | 27 / 179 |
| ForearmL | Ragdoll | 2.7 / 84.0 | 8 / 158 |
| HandL | Ragdoll | 3.6 / 88.7 | 19 / 174 |
| UpperArmR | Ragdoll | 0.6 / 12.8 | 11 / 59 |
| ForearmR | Ragdoll | 1.3 / 24.4 | 5 / 68 |
| HandR | Ragdoll | 2.4 / 51.7 | 14 / 76 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 4 (1 on the part aimed at, 1 blocked, 1 on someone in the way, 0 on allies), 41 damage. Hits taken: 14, 154 damage. Killed.

Defence: 13 blows came at it (8 seen clearly, 5 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 9.

Pose against the joint limits (standing): a joint more than 10° past its limit 18% of the time; the worst 36° (FootR).
Ragdoll (on the ground 20.2 s): a joint more than 10° past its limit 52% of the time; the worst 126° (UpperArmL). Fastest part with a standing leg within 0.5 m: 0.1 m/s median, 3.7 m/s at the 95th percentile (476 steps); with none: 0.4 m/s median (736 steps).

Look: within 25° of what it attended to 85% of the time standing (85% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 2 postures (BoxerSettle 1, BobAndWeave 1), 0 of them lead-ins to blows; energy at the end 0.43.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.3 cm, max 0.3 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.45 s: ClubSwing at A Medium's ElbowL: passed 0.3 cm from the aim point; nearest part A Medium's Pelvis (67.4 cm); the ElbowL had moved 94.5 cm since the aim was locked

## Shield contacts

210 times a shield overlapped another unit's body: 95% under 5.9 cm deep (83% under 2 cm; target 95%), the deepest 8.2 cm, the longest 0.28 s (target under 0.2 s). Each one is in events.log ("shield").

