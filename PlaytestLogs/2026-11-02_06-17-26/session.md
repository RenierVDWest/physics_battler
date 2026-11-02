# Playtest 2026-11-02_06-17-26

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 49.0 s of play (2940 physics steps), 3737 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-02_06-17-26`.

## A Small

Present 0.0–49.0 s. Steps 22, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 9 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.0 | 1 / 12 |
| Chest | Ragdoll | 0.3 / 2.4 | 2 / 25 |
| Head | Ragdoll | 0.5 / 4.5 | 6 / 30 |
| UpperArmL | Ragdoll | 0.4 / 8.4 | 5 / 126 |
| ForearmL | Ragdoll | 1.3 / 24.5 | 9 / 174 |
| HandL | Ragdoll | 2.3 / 36.0 | 14 / 179 |
| UpperArmR | Ragdoll | 0.5 / 2.3 | 39 / 118 |
| ForearmR | Ragdoll | 1.5 / 14.5 | 12 / 119 |
| HandR | Ragdoll | 2.5 / 31.5 | 28 / 102 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 4, 51 damage. Killed.

Defence: 4 blows came at it (3 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 52% of the time; the worst 42° (FootL).
Ragdoll (on the ground 40.3 s): a joint more than 10° past its limit 100% of the time; the worst 175° (ShinR). Fastest part with a standing leg within 0.5 m: 0.0 m/s median, 2.8 m/s at the 95th percentile (39 steps); with none: 0.0 m/s median (2381 steps).

Look: within 25° of what it attended to 79% of the time standing (79% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.78.

Balance: stumbled 3 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–49.0 s. Steps 72, idle shuffles 1. Feet slid at most 1.7 cm while planted; the weight was at most 47 cm from the support point; hips dropped for reach by up to 21 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.8 | 2 / 43 |
| Chest | Ragdoll | 0.5 / 8.8 | 2 / 53 |
| Head | Ragdoll | 0.8 / 21.7 | 14 / 115 |
| UpperArmL | Ragdoll | 1.0 / 31.7 | 7 / 175 |
| ForearmL | Ragdoll | 2.7 / 49.6 | 10 / 168 |
| HandL | Ragdoll | 3.0 / 56.8 | 5 / 99 |
| UpperArmR | Ragdoll | 0.5 / 18.8 | 19 / 177 |
| ForearmR | Ragdoll | 1.3 / 23.9 | 8 / 179 |
| HandR | Ragdoll | 2.1 / 39.9 | 16 / 174 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 9 (2 on the part aimed at, 0 blocked, 1 on someone in the way, 1 on allies), 126 damage. Hits taken: 13, 114 damage. Killed.

Defence: 5 blows came at it (4 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 33% of the time; the worst 100° (ShinL).
Ragdoll (on the ground 15.1 s): a joint more than 10° past its limit 84% of the time; the worst 173° (ForearmL). Fastest part with a standing leg within 0.5 m: 5.6 m/s median, 13.2 m/s at the 95th percentile (367 steps); with none: 45.2 m/s median (538 steps).

Look: within 25° of what it attended to 75% of the time standing (77% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 84% of the time standing; its rim came within 0.9 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 9 postures (GuardChange 2, HighFeint 3, Peek 3, ShieldPress 1), 3 of them lead-ins to blows; energy at the end 0.71.

Balance: stumbled 1 time, knocked down 3, launched 0, got up 3; thrown into others 6 times.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.3 cm, max 0.3 cm (posed); physical weapon's closest pass mean 0.3 cm, max 0.3 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 31.05 s: SwordThrust at B Small's Chest: passed 0.3 cm from the aim point; nearest part B Large's ThighR (15.6 cm); the Chest had moved 57.6 cm since the aim was locked
- 32.57 s: SwordCleave at B Large's Chest: passed 0.3 cm from the aim point; nearest part B Large's Chest (13.4 cm); the Chest had moved 61.8 cm since the aim was locked


## A Large

Present 0.0–49.0 s. Steps 56, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 41 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.2 | 1 / 35 |
| Chest | Ragdoll | 0.6 / 14.8 | 2 / 41 |
| Head | Ragdoll | 0.9 / 17.1 | 14 / 157 |
| UpperArmL | Ragdoll | 1.0 / 30.7 | 28 / 88 |
| ForearmL | Ragdoll | 2.7 / 45.6 | 9 / 101 |
| HandL | Ragdoll | 3.9 / 46.2 | 19 / 101 |
| UpperArmR | Ragdoll | 0.7 / 15.7 | 18 / 59 |
| ForearmR | Ragdoll | 1.1 / 13.1 | 5 / 68 |
| HandR | Ragdoll | 2.3 / 51.7 | 17 / 59 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 4 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 1 on allies), 36 damage. Hits taken: 19, 226 damage. Killed.

Defence: 16 blows came at it (10 seen clearly, 6 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 15.

Pose against the joint limits (standing): a joint more than 10° past its limit 20% of the time; the worst 141° (ShinR).
Ragdoll (on the ground 11.2 s): a joint more than 10° past its limit 97% of the time; the worst 60° (FootL). Fastest part with a standing leg within 0.5 m: - median, - at the 95th percentile (0 steps); with none: 0.0 m/s median (670 steps).

Look: within 25° of what it attended to 86% of the time standing (86% of the time it was within the head's reach, 89% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 4 postures (BoxerSettle 1, BobAndWeave 2, ClubFeint 1), 1 of them lead-ins to blows; energy at the end 0.27.

Balance: stumbled 4 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.4 cm, max 0.5 cm (posed); physical weapon's closest pass mean 4.2 cm, max 10.8 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.55 s: ClubRising at B Small's KneeL: passed 0.3 cm from the aim point; nearest part B Small's ThighR (5.7 cm); the KneeL had moved 27.6 cm since the aim was locked
- 4.50 s: ClubSwing at B Small's Belly: passed 0.5 cm from the aim point; nearest part B Small's FootL (39.3 cm); the Belly had moved 81.9 cm since the aim was locked
- 16.78 s: ClubRising at B Small's Belly: passed 0.5 cm from the aim point; nearest part A Medium's ThighL (16.4 cm); the Belly had moved 80.1 cm since the aim was locked
- 25.47 s: ClubSmash at B Large's Neck: passed 0.5 cm from the aim point; nearest part B Large's Head (9.4 cm); the Neck had moved 10.8 cm since the aim was locked


## B Small

Present 0.0–49.0 s. Steps 84, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 11 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 3.5 | 2 / 42 |
| Chest | Ragdoll | 0.4 / 5.3 | 2 / 35 |
| Head | Ragdoll | 0.4 / 8.8 | 9 / 126 |
| UpperArmL | Ragdoll | 0.5 / 10.6 | 8 / 53 |
| ForearmL | Ragdoll | 1.2 / 15.4 | 8 / 99 |
| HandL | Ragdoll | 1.8 / 36.0 | 17 / 165 |
| UpperArmR | Ragdoll | 0.4 / 13.2 | 4 / 80 |
| ForearmR | Ragdoll | 1.0 / 25.0 | 7 / 178 |
| HandR | Ragdoll | 1.9 / 33.7 | 16 / 178 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 3 (0 on the part aimed at, 0 blocked, 2 on someone in the way, 0 on allies), 23 damage. Hits taken: 7, 64 damage. Killed.

Defence: 6 blows came at it (2 seen clearly, 4 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 49% of the time; the worst 45° (FootR).
Ragdoll (on the ground 21.2 s): a joint more than 10° past its limit 95% of the time; the worst 173° (ForearmR). Fastest part with a standing leg within 0.5 m: 3.1 m/s median, 11.3 m/s at the 95th percentile (517 steps); with none: 0.0 m/s median (752 steps).

Look: within 25° of what it attended to 82% of the time standing (83% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 8 postures (StickSettle 2, AbanicoFlurry 3, BaitRetreat 1, RedondaTwirl 2), 1 of them lead-ins to blows; energy at the end 0.91.

Balance: stumbled 2 times, knocked down 0, launched 2, got up 2; thrown into others 2 times.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 1.4 cm, max 1.4 cm (posed); physical weapon's closest pass mean 1.3 cm, max 1.3 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 10.95 s: ClubRising at A Large's KneeL: passed 1.4 cm from the aim point; nearest part A Large's ThighL (13.5 cm); the KneeL had moved 20.3 cm since the aim was locked


## B Medium

Present 0.0–49.0 s. Steps 83, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 29 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 1.0 | 1 / 78 |
| Chest | Pinned | 0.4 / 23.5 | 2 / 80 |
| Head | Pinned | 0.5 / 22.7 | 10 / 99 |
| UpperArmL | Pinned | 0.6 / 34.4 | 4 / 166 |
| ForearmL | Pinned | 1.3 / 33.4 | 5 / 127 |
| HandL | Pinned | 1.6 / 32.7 | 3 / 65 |
| UpperArmR | Pinned | 0.4 / 14.6 | 9 / 111 |
| ForearmR | Pinned | 0.9 / 19.7 | 5 / 127 |
| HandR | Pinned | 1.5 / 27.7 | 12 / 147 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 8 (2 on the part aimed at, 0 blocked, 0 on someone in the way, 2 on allies), 106 damage. Hits taken: 8, 74 damage. Health at the end 26.

Defence: 1 blows came at it (1 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 0.

Pose against the joint limits (standing): a joint more than 10° past its limit 32% of the time; the worst 107° (ShinR).
Ragdoll (on the ground 4.5 s): a joint more than 10° past its limit 86% of the time; the worst 175° (ForearmL). Fastest part with a standing leg within 0.5 m: 5.2 m/s median, 11.7 m/s at the 95th percentile (62 steps); with none: 4.1 m/s median (210 steps).

Look: within 25° of what it attended to 85% of the time standing (87% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 93% of the time standing; its rim came within -3.0 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 10 postures (GuardChange 2, ShieldSettle 1, HighFeint 2, ShieldPress 2, Peek 2, ShieldAdvance 1), 2 of them lead-ins to blows; energy at the end 0.85.

Balance: stumbled 2 times, knocked down 3, launched 0, got up 3; thrown into others 3 times.

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.2 cm, max 0.3 cm (posed); physical weapon's closest pass mean 0.8 cm, max 1.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.25 s: SwordCut at A Small's Chest: passed 0.1 cm from the aim point; nearest part A Small's Spine (8.6 cm); the Chest had moved 7.7 cm since the aim was locked
- 5.05 s: SwordBackhand at A Small's Chest: passed 0.2 cm from the aim point; nearest part A Small's UpperArmR (3.7 cm); the Chest had moved 22.5 cm since the aim was locked
- 5.87 s: SwordCleave at A Small's Chest: passed 0.2 cm from the aim point; nearest part A Small's Chest (5.2 cm); the Chest had moved 19.0 cm since the aim was locked
- 8.63 s: SwordCut at A Small's Neck: passed 0.3 cm from the aim point; nearest part A Small's Chest (7.4 cm); the Neck had moved 5.7 cm since the aim was locked
- 31.93 s: SwordBackhand at A Large's Belly: passed 0.2 cm from the aim point; nearest part A Large's Pelvis (23.4 cm); the Belly had moved 38.4 cm since the aim was locked


## B Large

Present 0.0–49.0 s. Steps 79, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 45 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 17.3 | 2 / 101 |
| Chest | Pinned | 0.8 / 35.6 | 2 / 51 |
| Head | Pinned | 1.0 / 38.9 | 13 / 122 |
| UpperArmL | Pinned | 1.3 / 51.3 | 22 / 175 |
| ForearmL | Pinned | 4.2 / 75.9 | 12 / 171 |
| HandL | Pinned | 5.5 / 82.8 | 24 / 178 |
| UpperArmR | Pinned | 0.7 / 32.4 | 15 / 89 |
| ForearmR | Pinned | 1.7 / 40.6 | 6 / 109 |
| HandR | Pinned | 2.7 / 71.2 | 14 / 180 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 16 (4 on the part aimed at, 0 blocked, 2 on someone in the way, 3 on allies), 205 damage. Hits taken: 11, 127 damage. Health at the end 93.

Defence: 9 blows came at it (4 seen clearly, 5 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 5.

Pose against the joint limits (standing): a joint more than 10° past its limit 23% of the time; the worst 146° (ShinL).

Look: within 25° of what it attended to 81% of the time standing (81% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 14 postures (BoxerSettle 1, BoxerStalk 2, ClubFeint 5, BobAndWeave 3, JabFeint 2, Shift 1), 4 of them lead-ins to blows; energy at the end 0.65.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 7. The posed striking part passed within 3 cm of the aim point in 7; aim error mean 0.5 cm, max 1.3 cm (posed); physical weapon's closest pass mean 4.8 cm, max 4.8 cm. 2 reached the part they were aimed at (nearest part within 5 cm at contact).

- 5.03 s: ClubSwing at A Medium's Belly: passed 0.2 cm from the aim point; nearest part A Medium's Chest (9.3 cm); the Belly had moved 39.3 cm since the aim was locked
- 7.58 s: ClubSmash at A Large's Chest: passed 0.3 cm from the aim point; nearest part A Large's Chest (0.0 cm); the Chest had moved 31.1 cm since the aim was locked
- 17.10 s: ClubSwing at A Medium's Head: passed 0.4 cm from the aim point; nearest part A Large's ForearmR (18.9 cm); the Head had moved 159.0 cm since the aim was locked
- 19.13 s: ClubSmash at A Large's Head: passed 0.2 cm from the aim point; nearest part A Large's Head (1.0 cm); the Head had moved 13.6 cm since the aim was locked
- 29.18 s: ClubSmash at A Large's Head: passed 1.3 cm from the aim point; nearest part A Large's Head (14.8 cm); the Head had moved 29.6 cm since the aim was locked
- 32.43 s: ClubRising at A Large's Belly: passed 0.6 cm from the aim point; nearest part A Large's ForearmR (21.6 cm); the Belly had moved 30.6 cm since the aim was locked
- 40.05 s: ClubSwing at A Medium's Chest: passed 0.3 cm from the aim point; nearest part A Medium's Chest (49.2 cm); the Chest had moved 65.9 cm since the aim was locked

## Shield contacts

149 times a shield overlapped another unit's body: 95% under 7.6 cm deep (79% under 2 cm; target 95%), the deepest 13.4 cm, the longest 0.87 s (target under 0.2 s). Each one is in events.log ("shield").

