# Playtest 2026-11-02_07-22-11

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 29.8 s of play (1791 physics steps), 2511 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-02_07-22-11`.

## A Small

Present 0.0–29.8 s. Steps 30, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 7 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 3.7 | 2 / 33 |
| Chest | Ragdoll | 0.3 / 5.4 | 2 / 53 |
| Head | Ragdoll | 0.5 / 5.5 | 9 / 95 |
| UpperArmL | Ragdoll | 0.6 / 14.3 | 6 / 178 |
| ForearmL | Ragdoll | 1.6 / 18.6 | 10 / 175 |
| HandL | Ragdoll | 2.9 / 45.6 | 21 / 165 |
| UpperArmR | Ragdoll | 0.5 / 8.3 | 4 / 92 |
| ForearmR | Ragdoll | 1.0 / 14.5 | 8 / 119 |
| HandR | Ragdoll | 2.5 / 31.5 | 23 / 102 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 2 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 2 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 9, 60 damage. Killed.

Defence: 7 blows came at it (2 seen clearly, 5 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 6.

Pose against the joint limits (standing): a joint more than 10° past its limit 62% of the time; the worst 98° (FootL).
Ragdoll (on the ground 18.1 s): a joint more than 10° past its limit 92% of the time; the worst 170° (ForearmL). Fastest part with a standing leg within 0.5 m: 2.5 m/s median, 7.2 m/s at the 95th percentile (296 steps); with none: 0.0 m/s median (792 steps).

Look: within 25° of what it attended to 72% of the time standing (74% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.73.

Balance: stumbled 2 times, knocked down 2, launched 1, got up 2; thrown into others 1 time.


## A Medium

Present 0.0–29.8 s. Steps 53, idle shuffles 0. Feet slid at most 227.6 cm while planted; the weight was at most 26 cm from the support point; hips dropped for reach by up to 21 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.8 / 108.0 | 0 / 0 |
| Spine | Pinned | 0.9 / 108.0 | 2 / 46 |
| Chest | Pinned | 1.4 / 113.2 | 2 / 18 |
| Head | Pinned | 1.6 / 116.6 | 11 / 66 |
| UpperArmL | Pinned | 1.7 / 115.2 | 6 / 179 |
| ForearmL | Pinned | 3.5 / 121.8 | 10 / 173 |
| HandL | Pinned | 4.8 / 121.4 | 5 / 63 |
| UpperArmR | Kinematic | 1.5 / 115.7 | 7 / 91 |
| ForearmR | Kinematic | 2.4 / 131.6 | 6 / 134 |
| HandR | Kinematic | 2.9 / 130.3 | 18 / 172 |
| ThighL | Kinematic | 0.9 / 107.7 | 0 / 150 |
| ShinL | Kinematic | 0.8 / 103.6 | 0 / 0 |
| FootL | Kinematic | 0.8 / 100.9 | 0 / 0 |
| ThighR | Kinematic | 0.8 / 108.4 | 0 / 10 |
| ShinR | Kinematic | 1.2 / 151.0 | 0 / 0 |
| FootR | Kinematic | 1.2 / 152.1 | 0 / 0 |

Hits dealt: 3 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 59 damage. Hits taken: 7, 90 damage. Health at the end 10.

Defence: 6 blows came at it (2 seen clearly, 4 at the edge of its view, 0 never seen); it raised the shield against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 38% of the time; the worst 122° (ShinL).
Ragdoll (on the ground 5.7 s): a joint more than 10° past its limit 92% of the time; the worst 175° (ForearmL). Fastest part with a standing leg within 0.5 m: 12.3 m/s median, 17.3 m/s at the 95th percentile (35 steps); with none: 6.5 m/s median (306 steps).

Look: within 25° of what it attended to 76% of the time standing (81% of the time it was within the head's reach, 94% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 89% of the time standing; its rim came within 1.0 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 6 postures (GuardChange 1, HighFeint 1, Peek 2, ShieldSettle 1, ShieldAdvance 1), 2 of them lead-ins to blows; energy at the end 0.40.

Balance: stumbled 2 times, knocked down 2, launched 0, got up 2; thrown into others 2 times.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 1.7 cm, max 4.7 cm (posed); physical weapon's closest pass mean 0.2 cm, max 0.2 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 15.97 s: SwordCut at B Large's Chest: passed 0.1 cm from the aim point; nearest part B Large's Chest (0.1 cm); the Chest had moved 36.1 cm since the aim was locked
- 23.00 s: SwordThrust at B Large's Belly: passed 4.7 cm from the aim point; nearest part B Large's HandR (28.2 cm); the Belly had moved 32.5 cm since the aim was locked
- 29.08 s: SwordCut at B Medium's Belly: passed 0.3 cm from the aim point; nearest part B Medium's Chest (18.2 cm); the Belly had moved 47.9 cm since the aim was locked


## A Large

Present 0.0–29.8 s. Steps 36, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 56 cm from the support point; hips dropped for reach by up to 5 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 1 / 18 |
| Chest | Pinned | 0.6 / 7.3 | 1 / 18 |
| Head | Pinned | 0.7 / 8.0 | 17 / 81 |
| UpperArmL | Pinned | 1.0 / 11.0 | 15 / 55 |
| ForearmL | Pinned | 3.1 / 23.7 | 9 / 63 |
| HandL | Pinned | 4.2 / 45.4 | 24 / 92 |
| UpperArmR | Pinned | 0.4 / 6.3 | 7 / 59 |
| ForearmR | Pinned | 0.7 / 28.1 | 5 / 68 |
| HandR | Pinned | 1.4 / 51.7 | 9 / 59 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 149 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 3 |

Hits dealt: 4 (2 on the part aimed at, 0 blocked, 0 on someone in the way, 1 on allies), 70 damage. Hits taken: 13, 166 damage. Health at the end 54.

Defence: 12 blows came at it (3 seen clearly, 9 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 8.

Pose against the joint limits (standing): a joint more than 10° past its limit 25% of the time; the worst 154° (ShinR).
Ragdoll (on the ground 2.2 s): a joint more than 10° past its limit 99% of the time; the worst 144° (HandL). Fastest part with a standing leg within 0.5 m: 3.0 m/s median, 9.2 m/s at the 95th percentile (130 steps); with none: - median (0 steps).

Look: within 25° of what it attended to 77% of the time standing (78% of the time it was within the head's reach, 91% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 2 postures (BoxerSettle 1, BobAndWeave 1), 0 of them lead-ins to blows; energy at the end 0.42.

Balance: stumbled 1 time, knocked down 1, launched 0, got up 1; thrown into others 2 times.

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.4 cm, max 0.5 cm (posed); physical weapon's closest pass mean 0.6 cm, max 1.2 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.55 s: ClubRising at B Small's KneeL: passed 0.5 cm from the aim point; nearest part B Small's ThighR (5.6 cm); the KneeL had moved 27.6 cm since the aim was locked
- 9.15 s: ClubSmash at B Small's Chest: passed 0.5 cm from the aim point; nearest part B Small's Chest (16.8 cm); the Chest had moved 34.5 cm since the aim was locked
- 16.32 s: ClubSmash at B Large's Neck: passed 0.5 cm from the aim point; nearest part B Large's Chest (45.2 cm); the Neck had moved 81.7 cm since the aim was locked
- 18.35 s: ClubBackhand at B Large's Neck: passed 0.1 cm from the aim point; nearest part B Large's Head (13.2 cm); the Neck had moved 13.8 cm since the aim was locked
- 28.28 s: ClubBackhand at B Small's Neck: passed 0.5 cm from the aim point; nearest part B Small's Chest (22.1 cm); the Neck had moved 35.6 cm since the aim was locked


## B Small

Present 0.0–29.8 s. Steps 80, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 18 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.1 / 11.9 | 4 / 178 |
| Chest | Ragdoll | 0.7 / 27.1 | 4 / 178 |
| Head | Ragdoll | 0.9 / 33.9 | 14 / 133 |
| UpperArmL | Ragdoll | 1.1 / 53.2 | 11 / 178 |
| ForearmL | Ragdoll | 2.5 / 51.7 | 15 / 179 |
| HandL | Ragdoll | 3.1 / 55.5 | 24 / 177 |
| UpperArmR | Ragdoll | 0.4 / 16.4 | 4 / 154 |
| ForearmR | Ragdoll | 0.8 / 26.9 | 7 / 176 |
| HandR | Ragdoll | 1.3 / 34.1 | 13 / 175 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 45 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 45 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 1 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 6 |

Hits dealt: 7 (5 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 66 damage. Hits taken: 4, 42 damage. Health at the end 18.

Defence: 3 blows came at it (1 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 49% of the time; the worst 114° (FootR).
Ragdoll (on the ground 3.0 s): a joint more than 10° past its limit 89% of the time; the worst 173° (ShinR). Fastest part with a standing leg within 0.5 m: 3.9 m/s median, 9.1 m/s at the 95th percentile (89 steps); with none: 7.6 m/s median (89 steps).

Look: within 25° of what it attended to 68% of the time standing (78% of the time it was within the head's reach, 85% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 8 postures (StickSettle 2, AbanicoFlurry 2, BaitRetreat 2, StickStalk 1, RedondaTwirl 1), 1 of them lead-ins to blows; energy at the end 0.29.

Balance: stumbled 1 time, knocked down 1, launched 1, got up 1; thrown into others 0 times. Still down at the end.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.7 cm, max 1.8 cm (posed); physical weapon's closest pass mean 0.8 cm, max 0.8 cm. 2 reached the part they were aimed at (nearest part within 5 cm at contact).

- 15.78 s: ClubFanBackhand at A Large's ElbowL: passed 1.8 cm from the aim point; nearest part A Large's Pelvis (31.2 cm); the ElbowL had moved 31.1 cm since the aim was locked
- 16.75 s: ClubBackhand at A Large's ElbowL: passed 0.4 cm from the aim point; nearest part A Large's ForearmL (3.4 cm); the ElbowL had moved 9.2 cm since the aim was locked
- 18.70 s: ClubRising at A Large's Chest: passed 0.1 cm from the aim point; nearest part A Large's Chest (0.0 cm); the Chest had moved 39.0 cm since the aim was locked
- 24.58 s: ClubBackhand at A Large's Belly: passed 0.3 cm from the aim point; nearest part A Large's Spine (8.0 cm); the Belly had moved 9.6 cm since the aim was locked


## B Medium

Present 0.0–29.8 s. Steps 61, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 40 cm from the support point; hips dropped for reach by up to 16 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 5.5 | 2 / 144 |
| Chest | Pinned | 0.6 / 30.2 | 3 / 124 |
| Head | Pinned | 1.0 / 67.2 | 14 / 133 |
| UpperArmL | Pinned | 1.3 / 70.6 | 7 / 179 |
| ForearmL | Pinned | 2.6 / 63.3 | 9 / 177 |
| HandL | Pinned | 3.2 / 58.7 | 4 / 81 |
| UpperArmR | Kinematic | 0.8 / 54.3 | 5 / 144 |
| ForearmR | Kinematic | 1.1 / 32.2 | 7 / 178 |
| HandR | Kinematic | 1.4 / 29.1 | 14 / 178 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 1 |

Hits dealt: 7 (1 on the part aimed at, 2 blocked, 0 on someone in the way, 1 on allies), 68 damage. Hits taken: 6, 43 damage. Health at the end 57.

Defence: 2 blows came at it (2 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 37% of the time; the worst 51° (FootR).
Ragdoll (on the ground 3.1 s): a joint more than 10° past its limit 97% of the time; the worst 173° (ForearmR). Fastest part with a standing leg within 0.5 m: 8.7 m/s median, 11.1 m/s at the 95th percentile (28 steps); with none: 3.8 m/s median (156 steps).

Look: within 25° of what it attended to 86% of the time standing (86% of the time it was within the head's reach, 96% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 95% of the time standing; its rim came within -0.7 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 7 postures (GuardChange 2, Peek 1, ShieldSettle 1, ShieldPress 2, HighFeint 1), 2 of them lead-ins to blows; energy at the end 0.28.

Balance: stumbled 1 time, knocked down 1, launched 0, got up 1; thrown into others 3 times.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.5 cm, max 1.4 cm (posed); physical weapon's closest pass mean 2.8 cm, max 9.2 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.05 s: SwordCut at A Small's Chest: passed 0.2 cm from the aim point; nearest part A Small's Spine (7.7 cm); the Chest had moved 27.6 cm since the aim was locked
- 5.42 s: SwordCleave at A Small's Chest: passed 1.4 cm from the aim point; nearest part A Small's Pelvis (53.8 cm); the Chest had moved 106.5 cm since the aim was locked
- 7.80 s: SwordThrust at A Small's Head: passed 0.1 cm from the aim point; nearest part A Small's Head (30.6 cm); the Head had moved 32.4 cm since the aim was locked
- 14.60 s: SwordBackhand at A Small's KneeL: passed 0.3 cm from the aim point; nearest part A Small's HandL (27.7 cm); the KneeL had moved 47.0 cm since the aim was locked


## B Large

Present 0.0–29.8 s. Steps 56, idle shuffles 1. Feet slid at most 17.6 cm while planted; the weight was at most 86 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 32.1 | 0 / 0 |
| Spine | Ragdoll | 0.1 / 32.1 | 2 / 24 |
| Chest | Ragdoll | 0.7 / 26.8 | 1 / 16 |
| Head | Ragdoll | 0.7 / 21.5 | 12 / 58 |
| UpperArmL | Ragdoll | 0.8 / 26.1 | 8 / 63 |
| ForearmL | Ragdoll | 2.8 / 42.6 | 8 / 114 |
| HandL | Ragdoll | 4.0 / 45.4 | 17 / 133 |
| UpperArmR | Ragdoll | 0.5 / 21.8 | 10 / 61 |
| ForearmR | Ragdoll | 1.2 / 20.6 | 4 / 71 |
| HandR | Ragdoll | 2.0 / 51.7 | 12 / 88 |
| ThighL | Ragdoll | 0.0 / 31.6 | 0 / 146 |
| ShinL | Ragdoll | 0.0 / 6.3 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 5.9 | 0 / 1 |
| ThighR | Ragdoll | 0.0 / 32.5 | 0 / 148 |
| ShinR | Ragdoll | 0.0 / 22.6 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 20.0 | 0 / 5 |

Hits dealt: 7 (2 on the part aimed at, 0 blocked, 1 on someone in the way, 1 on allies), 68 damage. Hits taken: 5, 79 damage. Killed.

Defence: 5 blows came at it (3 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 4.

Pose against the joint limits (standing): a joint more than 10° past its limit 31% of the time; the worst 132° (ShinR).
Ragdoll (on the ground 2.9 s): a joint more than 10° past its limit 79% of the time; the worst 47° (ForearmR). Fastest part with a standing leg within 0.5 m: 1.9 m/s median, 4.1 m/s at the 95th percentile (65 steps); with none: 0.3 m/s median (110 steps).

Look: within 25° of what it attended to 85% of the time standing (85% of the time it was within the head's reach, 93% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 6 postures (BoxerSettle 1, BoxerStalk 1, ClubFeint 2, BobAndWeave 1, JabFeint 1), 1 of them lead-ins to blows; energy at the end 0.55.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.3 cm, max 0.3 cm (posed); physical weapon's closest pass mean 2.2 cm, max 5.4 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.42 s: ClubSwing at A Medium's Chest: passed 0.2 cm from the aim point; nearest part A Medium's Head (45.3 cm); the Chest had moved 78.0 cm since the aim was locked
- 4.12 s: ClubBackhand at A Medium's Neck: passed 0.3 cm from the aim point; nearest part A Large's FootL (86.2 cm); the Neck had moved 563.1 cm since the aim was locked
- 8.93 s: ClubSwing at A Small's KneeR: passed 0.3 cm from the aim point; nearest part A Small's FootR (13.2 cm); the KneeR had moved 28.0 cm since the aim was locked
- 11.90 s: ClubSwing at A Large's Head: passed 0.3 cm from the aim point; nearest part A Large's HandR (7.2 cm); the Head had moved 34.7 cm since the aim was locked

## Shield contacts

97 times a shield overlapped another unit's body: 95% under 9.5 cm deep (70% under 2 cm; target 95%), the deepest 20.2 cm, the longest 0.38 s (target under 0.2 s). Each one is in events.log ("shield").

