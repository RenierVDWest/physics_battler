# Playtest 2026-11-01_21-17-34

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 59.6 s of play (3577 physics steps), 2218 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-01_21-17-34`.

## A Small

Present 0.0–59.6 s. Steps 25, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 10 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.1 | 3 / 46 |
| Chest | Ragdoll | 0.5 / 8.8 | 5 / 119 |
| Head | Ragdoll | 0.8 / 13.8 | 10 / 57 |
| UpperArmL | Ragdoll | 1.0 / 13.9 | 5 / 57 |
| ForearmL | Ragdoll | 1.6 / 15.4 | 9 / 99 |
| HandL | Ragdoll | 2.7 / 36.0 | 23 / 82 |
| UpperArmR | Ragdoll | 1.4 / 34.6 | 10 / 134 |
| ForearmR | Ragdoll | 4.2 / 65.7 | 13 / 133 |
| HandR | Ragdoll | 5.4 / 69.4 | 24 / 173 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 11 damage. Hits taken: 3, 63 damage. Killed.

Defence: 3 blows came at it (3 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 60% of the time; the worst 42° (FootL).
Ragdoll (on the ground 50.8 s): a joint more than 10° past its limit 100% of the time; the worst 110° (HandR). Fastest part with a standing leg within 0.5 m: 3.6 m/s median, 4.5 m/s at the 95th percentile (19 steps); with none: 0.0 m/s median (3031 steps).

Look: within 25° of what it attended to 76% of the time standing (77% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.73.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–59.6 s. Steps 112, idle shuffles 4. Feet slid at most 0.0 cm while planted; the weight was at most 31 cm from the support point; hips dropped for reach by up to 2 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.2 | 1 / 15 |
| Chest | Ragdoll | 0.4 / 4.6 | 1 / 12 |
| Head | Ragdoll | 0.5 / 5.3 | 10 / 50 |
| UpperArmL | Ragdoll | 0.6 / 6.2 | 3 / 57 |
| ForearmL | Ragdoll | 1.7 / 26.2 | 5 / 50 |
| HandL | Ragdoll | 2.3 / 40.8 | 5 / 131 |
| UpperArmR | Ragdoll | 0.4 / 5.1 | 2 / 91 |
| ForearmR | Ragdoll | 0.8 / 17.3 | 4 / 112 |
| HandR | Ragdoll | 1.4 / 28.9 | 13 / 134 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 11 (0 on the part aimed at, 4 blocked, 0 on someone in the way, 3 on allies), 77 damage. Hits taken: 21, 101 damage. Killed.

Defence: 24 blows came at it (10 seen clearly, 14 at the edge of its view, 0 never seen); it raised the shield against 15.

Pose against the joint limits (standing): a joint more than 10° past its limit 35% of the time; the worst 39° (FootL).
Ragdoll (on the ground 4.4 s): a joint more than 10° past its limit 91% of the time; the worst 161° (ForearmL). Fastest part with a standing leg within 0.5 m: 3.9 m/s median, 8.4 m/s at the 95th percentile (82 steps); with none: 0.6 m/s median (184 steps).

Look: within 25° of what it attended to 75% of the time standing (76% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 89% of the time standing; its rim came within -1.3 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 5 postures (GuardChange 1, HighFeint 2, Peek 2), 0 of them lead-ins to blows; energy at the end 0.54.

Balance: stumbled 2 times, knocked down 1, launched 0, got up 1; thrown into others 2 times.


## A Large

Present 0.0–59.6 s. Steps 66, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 39 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.4 | 1 / 48 |
| Chest | Ragdoll | 0.5 / 19.3 | 1 / 33 |
| Head | Ragdoll | 0.7 / 19.7 | 11 / 108 |
| UpperArmL | Ragdoll | 0.8 / 27.6 | 5 / 125 |
| ForearmL | Ragdoll | 3.1 / 50.4 | 8 / 146 |
| HandL | Ragdoll | 4.2 / 65.2 | 19 / 168 |
| UpperArmR | Ragdoll | 0.4 / 6.2 | 1 / 59 |
| ForearmR | Ragdoll | 0.9 / 13.5 | 3 / 68 |
| HandR | Ragdoll | 1.8 / 51.7 | 8 / 107 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 9 (2 on the part aimed at, 0 blocked, 1 on someone in the way, 0 on allies), 118 damage. Hits taken: 24, 230 damage. Killed.

Defence: 19 blows came at it (7 seen clearly, 12 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 17.

Pose against the joint limits (standing): a joint more than 10° past its limit 19% of the time; the worst 123° (ShinR).
Ragdoll (on the ground 11.9 s): a joint more than 10° past its limit 13% of the time; the worst 72° (FootR). Fastest part with a standing leg within 0.5 m: 2.5 m/s median, 6.5 m/s at the 95th percentile (89 steps); with none: 0.0 m/s median (623 steps).

Look: within 25° of what it attended to 81% of the time standing (80% of the time it was within the head's reach, 95% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 8 postures (BoxerSettle 1, ClubFeint 3, JabFeint 2, BoxerStalk 1, BobAndWeave 1), 2 of them lead-ins to blows; energy at the end 0.23.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.5 cm, max 0.8 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.82 s: ClubSwing at B Small's KneeL: passed 0.5 cm from the aim point; nearest part B Small's ThighR (15.8 cm); the KneeL had moved 16.2 cm since the aim was locked
- 6.17 s: ClubSmash at B Large's Chest: passed 0.2 cm from the aim point; nearest part B Large's ForearmL (14.9 cm); the Chest had moved 29.8 cm since the aim was locked
- 21.65 s: ClubSwing at B Large's Belly: passed 0.4 cm from the aim point; nearest part B Large's Spine (0.0 cm); the Belly had moved 23.2 cm since the aim was locked
- 40.17 s: ClubRising at B Large's KneeL: passed 0.8 cm from the aim point; nearest part B Large's ThighL (0.0 cm); the KneeL had moved 4.3 cm since the aim was locked


## B Small

Present 0.0–59.6 s. Steps 170, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 42 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 4.7 | 2 / 159 |
| Chest | ? | 0.4 / 7.2 | 2 / 136 |
| Head | ? | 0.5 / 23.9 | 10 / 129 |
| UpperArmL | ? | 0.6 / 33.5 | 5 / 173 |
| ForearmL | ? | 1.9 / 46.3 | 10 / 172 |
| HandL | ? | 3.0 / 84.0 | 27 / 167 |
| UpperArmR | ? | 0.2 / 1.7 | 1 / 80 |
| ForearmR | ? | 0.5 / 15.7 | 3 / 100 |
| HandR | ? | 1.0 / 24.5 | 11 / 106 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 16 (6 on the part aimed at, 0 blocked, 0 on someone in the way, 1 on allies), 143 damage. Hits taken: 1, 9 damage. Health at the end 0.

Defence: 1 blows came at it (0 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 46% of the time; the worst 110° (ThighR).
Ragdoll (on the ground 3.0 s): a joint more than 10° past its limit 83% of the time; the worst 174° (ForearmL). Fastest part with a standing leg within 0.5 m: 7.2 m/s median, 8.0 m/s at the 95th percentile (22 steps); with none: 7.0 m/s median (157 steps).

Look: within 25° of what it attended to 79% of the time standing (83% of the time it was within the head's reach, 93% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 18 postures (StickSettle 4, AbanicoFlurry 6, RedondaTwirl 4, TriangleStep 2, BaitRetreat 1, StickStalk 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 0 times, knocked down 0, launched 1, got up 1; thrown into others 0 times.

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.5 cm, max 0.6 cm (posed); physical weapon's closest pass mean 0.1 cm, max 0.1 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 24.57 s: ClubSwing at A Large's KneeL: passed 0.6 cm from the aim point; nearest part A Large's ShinL (14.8 cm); the KneeL had moved 23.1 cm since the aim was locked
- 26.27 s: ClubRising at A Large's Belly: passed 0.3 cm from the aim point; nearest part A Large's ForearmR (0.9 cm); the Belly had moved 14.5 cm since the aim was locked
- 38.17 s: ClubSwing at A Large's ElbowL: passed 0.3 cm from the aim point; nearest part A Large's Chest (3.8 cm); the ElbowL had moved 12.2 cm since the aim was locked
- 40.17 s: ClubSwing at A Large's Belly: passed 0.5 cm from the aim point; nearest part A Large's UpperArmR (89.5 cm); the Belly had moved 128.5 cm since the aim was locked
- 56.52 s: ClubRising at A Medium's KneeR: passed 0.5 cm from the aim point; nearest part A Medium's ThighL (0.0 cm); the KneeR had moved 66.9 cm since the aim was locked


## B Medium

Present 0.0–59.6 s. Steps 106, idle shuffles 4. Feet slid at most 0.0 cm while planted; the weight was at most 33 cm from the support point; hips dropped for reach by up to 2 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 2 / 22 |
| Chest | Pinned | 0.4 / 6.4 | 2 / 15 |
| Head | Pinned | 0.5 / 5.5 | 9 / 57 |
| UpperArmL | Pinned | 0.6 / 8.4 | 3 / 62 |
| ForearmL | Pinned | 1.5 / 31.6 | 4 / 55 |
| HandL | Pinned | 2.0 / 32.7 | 5 / 116 |
| UpperArmR | Pinned | 0.3 / 6.3 | 1 / 97 |
| ForearmR | Pinned | 0.7 / 19.7 | 3 / 99 |
| HandR | Pinned | 1.2 / 25.6 | 11 / 154 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 17 (4 on the part aimed at, 8 blocked, 0 on someone in the way, 1 on allies), 131 damage. Hits taken: 7, 29 damage. Health at the end 71.

Defence: 4 blows came at it (3 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 27% of the time; the worst 36° (FootR).
Ragdoll (on the ground 2.0 s): a joint more than 10° past its limit 76% of the time; the worst 171° (ForearmL). Fastest part with a standing leg within 0.5 m: 6.8 m/s median, 10.0 m/s at the 95th percentile (29 steps); with none: 5.3 m/s median (92 steps).

Look: within 25° of what it attended to 81% of the time standing (81% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 93% of the time standing; its rim came within -1.5 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 14 postures (GuardChange 3, ShieldPress 4, Peek 3, HighFeint 4), 3 of them lead-ins to blows; energy at the end 0.64.

Balance: stumbled 1 time, knocked down 1, launched 0, got up 1; thrown into others 1 time.

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.2 cm, max 0.3 cm (posed); physical weapon's closest pass mean 0.4 cm, max 0.8 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.92 s: SwordBackhand at A Small's ElbowR: passed 0.2 cm from the aim point; nearest part A Small's UpperArmR (19.5 cm); the ElbowR had moved 13.5 cm since the aim was locked
- 6.67 s: SwordBackhand at A Small's Neck: passed 0.3 cm from the aim point; nearest part A Small's Head (6.3 cm); the Neck had moved 10.4 cm since the aim was locked
- 8.77 s: SwordCut at A Small's Head: passed 0.1 cm from the aim point; nearest part A Small's Head (7.1 cm); the Head had moved 8.2 cm since the aim was locked
- 28.32 s: SwordThrust at A Medium's Head: passed 0.1 cm from the aim point; nearest part A Medium's Chest (18.9 cm); the Head had moved 35.7 cm since the aim was locked
- 31.78 s: SwordThrust at A Medium's Neck: passed 0.1 cm from the aim point; nearest part A Medium's Head (11.7 cm); the Neck had moved 3.0 cm since the aim was locked


## B Large

Present 0.0–59.6 s. Steps 90, idle shuffles 4. Feet slid at most 0.0 cm while planted; the weight was at most 34 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 1.2 | 1 / 79 |
| Chest | Pinned | 0.5 / 32.2 | 1 / 50 |
| Head | Pinned | 0.7 / 31.2 | 9 / 58 |
| UpperArmL | Pinned | 0.9 / 55.6 | 4 / 130 |
| ForearmL | Pinned | 2.6 / 59.6 | 7 / 176 |
| HandL | Pinned | 3.8 / 45.4 | 18 / 122 |
| UpperArmR | Pinned | 0.5 / 8.9 | 1 / 59 |
| ForearmR | Pinned | 0.9 / 13.1 | 3 / 68 |
| HandR | Pinned | 1.7 / 51.7 | 8 / 85 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 10 (2 on the part aimed at, 2 blocked, 1 on someone in the way, 1 on allies), 95 damage. Hits taken: 14, 175 damage. Health at the end 45.

Defence: 17 blows came at it (9 seen clearly, 8 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 14.

Pose against the joint limits (standing): a joint more than 10° past its limit 20% of the time; the worst 93° (ShinR).

Look: within 25° of what it attended to 88% of the time standing (89% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 6 postures (BoxerSettle 1, BoxerStalk 1, BobAndWeave 1, Shift 1, JabFeint 1, ClubFeint 1), 3 of them lead-ins to blows; energy at the end 0.57.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.4 cm, max 0.4 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 17.12 s: ClubSwing at A Medium's KneeL: passed 0.4 cm from the aim point; nearest part A Medium's ThighL (14.6 cm); the KneeL had moved 8.1 cm since the aim was locked
- 23.55 s: ClubSwing at A Large's Head: passed 0.3 cm from the aim point; nearest part A Large's Head (3.1 cm); the Head had moved 20.5 cm since the aim was locked

## Shield contacts

68 times a shield overlapped another unit's body: 95% under 6.3 cm deep (66% under 2 cm; target 95%), the deepest 15.4 cm, the longest 0.70 s (target under 0.2 s). Each one is in events.log ("shield").

