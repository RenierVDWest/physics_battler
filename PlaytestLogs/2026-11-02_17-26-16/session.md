# Playtest 2026-11-02_17-26-16

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 82.8 s of play (4970 physics steps), 5284 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-02_17-26-16`.

## A Small

Present 0.0–82.8 s. Steps 15, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 6 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.0 | 3 / 22 |
| Chest | ? | 0.5 / 4.3 | 4 / 15 |
| Head | ? | 0.8 / 7.7 | 8 / 65 |
| UpperArmL | ? | 0.8 / 6.7 | 9 / 174 |
| ForearmL | ? | 1.5 / 15.5 | 9 / 98 |
| HandL | ? | 2.5 / 36.0 | 23 / 115 |
| UpperArmR | ? | 0.9 / 7.7 | 10 / 58 |
| ForearmR | ? | 3.3 / 22.1 | 24 / 88 |
| HandR | ? | 9.8 / 34.4 | 36 / 106 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 3, 66 damage. Health at the end 0.

Defence: 3 blows came at it (1 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 99% of the time; the worst 43° (ForearmL).
Ragdoll (on the ground 77.5 s): a joint more than 10° past its limit 50% of the time; the worst 168° (ShinR). Fastest part with a standing leg within 0.5 m: 0.1 m/s median, 4.3 m/s at the 95th percentile (2775 steps); with none: 0.0 m/s median (1874 steps).

Look: within 25° of what it attended to 75% of the time standing (76% of the time it was within the head's reach, 95% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 2 postures (StickSettle 1, AbanicoFlurry 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–82.8 s. Steps 150, idle shuffles 7. Feet slid at most 0.0 cm while planted; the weight was at most 36 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 2 / 53 |
| Chest | ? | 0.6 / 14.5 | 4 / 52 |
| Head | ? | 0.9 / 15.5 | 10 / 54 |
| UpperArmL | ? | 1.1 / 29.1 | 6 / 157 |
| ForearmL | ? | 2.2 / 41.3 | 6 / 165 |
| HandL | ? | 2.9 / 44.3 | 6 / 103 |
| UpperArmR | ? | 1.0 / 23.9 | 8 / 96 |
| ForearmR | ? | 3.7 / 44.2 | 15 / 180 |
| HandR | ? | 7.8 / 81.0 | 22 / 179 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 1 |
| ThighR | ? | 0.0 / 0.0 | 0 / 149 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 2 |

Hits dealt: 18 (2 on the part aimed at, 7 blocked, 0 on someone in the way, 4 on allies), 169 damage. Hits taken: 14, 86 damage. Health at the end 0.

Defence: 8 blows came at it (7 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 100% of the time; the worst 140° (ThighR).
Ragdoll (on the ground 4.0 s): a joint more than 10° past its limit 79% of the time; the worst 144° (ForearmL). Fastest part with a standing leg within 0.5 m: 4.0 m/s median, 7.8 m/s at the 95th percentile (50 steps); with none: 3.0 m/s median (192 steps).

Look: within 25° of what it attended to 80% of the time standing (82% of the time it was within the head's reach, 97% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 88% of the time standing; its rim came within 0.2 cm of its own body (negative: inside).

Fight Rhythm (?): 19 postures (GuardChange 4, Peek 5, HighFeint 5, ShieldSettle 2, ShieldAdvance 2, ShieldPress 1), 2 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 2 times, knocked down 3, launched 0, got up 3; thrown into others 1 time.

Blows: 6. The posed striking part passed within 3 cm of the aim point in 6; aim error mean 0.5 cm, max 1.8 cm (posed); physical weapon's closest pass mean 0.9 cm, max 1.8 cm. 3 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.22 s: SwordBackhand at B Large's Chest: passed 1.8 cm from the aim point; nearest part B Large's Chest (0.0 cm); the Chest had moved 41.0 cm since the aim was locked
- 22.22 s: SwordBackhand at B Medium's Belly: passed 1.0 cm from the aim point; nearest part B Medium's Spine (0.0 cm); the Belly had moved 12.8 cm since the aim was locked
- 47.93 s: SwordThrust at B Medium's Chest: passed 0.0 cm from the aim point; nearest part A Large's ForearmR (65.6 cm); the Chest had moved 100.5 cm since the aim was locked
- 53.60 s: SwordCut at B Medium's Head: passed 0.0 cm from the aim point; nearest part B Medium's UpperArmR (21.0 cm); the Head had moved 61.2 cm since the aim was locked
- 56.33 s: SwordBackhand at B Medium's Chest: passed 0.1 cm from the aim point; nearest part B Medium's Chest (0.0 cm); the Chest had moved 13.0 cm since the aim was locked
- 66.07 s: SwordCut at B Medium's Head: passed 0.0 cm from the aim point; nearest part B Medium's HandL (45.4 cm); the Head had moved 81.1 cm since the aim was locked


## A Large

Present 0.0–82.8 s. Steps 155, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 55 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 2.1 | 0 / 0 |
| Spine | Pinned | 0.0 / 2.1 | 2 / 49 |
| Chest | Pinned | 0.9 / 20.3 | 3 / 34 |
| Head | Pinned | 1.2 / 16.7 | 15 / 114 |
| UpperArmL | Pinned | 1.6 / 34.3 | 8 / 173 |
| ForearmL | Pinned | 4.8 / 64.5 | 13 / 180 |
| HandL | Pinned | 7.3 / 82.0 | 27 / 160 |
| UpperArmR | Muscled | 1.2 / 17.0 | 7 / 70 |
| ForearmR | Muscled | 4.8 / 56.6 | 11 / 132 |
| HandR | Muscled | 7.9 / 78.5 | 17 / 171 |
| ThighL | Kinematic | 0.0 / 1.6 | 0 / 150 |
| ShinL | Kinematic | 0.0 / 19.0 | 0 / 53 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 2.5 | 0 / 149 |
| ShinR | Kinematic | 0.0 / 1.3 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 1 |

Hits dealt: 24 (3 on the part aimed at, 9 blocked, 2 on someone in the way, 3 on allies), 184 damage. Hits taken: 8, 110 damage. Health at the end 110.

Defence: 4 blows came at it (4 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 100% of the time; the worst 150° (ShinR).

Look: within 25° of what it attended to 90% of the time standing (91% of the time it was within the head's reach, 90% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 21 postures (BoxerSettle 3, ClubFeint 6, JabFeint 7, BoxerStalk 4, BobAndWeave 1), 8 of them lead-ins to blows; energy at the end 0.99.

Blows: 12. The posed striking part passed within 3 cm of the aim point in 12; aim error mean 0.1 cm, max 0.5 cm (posed); physical weapon's closest pass mean 4.6 cm, max 12.9 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.65 s: ClubRising at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's ForearmR (14.7 cm); the Belly had moved 32.1 cm since the aim was locked
- 4.50 s: ClubSwing at B Small's Head: passed 0.5 cm from the aim point; nearest part B Small's FootL (38.5 cm); the Head had moved 83.4 cm since the aim was locked
- 8.55 s: ClubSwing at B Small's KneeL: passed 0.0 cm from the aim point; nearest part B Large's UpperArmR (22.6 cm); the KneeL had moved 54.5 cm since the aim was locked
- 14.72 s: ClubRising at B Small's Belly: passed 0.4 cm from the aim point; nearest part B Small's HandR (24.9 cm); the Belly had moved 31.8 cm since the aim was locked
- 21.42 s: ClubRising at B Small's Belly: passed 0.1 cm from the aim point; nearest part B Small's HandR (4.0 cm); the Belly had moved 10.6 cm since the aim was locked
- 26.93 s: ClubRising at B Medium's Belly: passed 0.0 cm from the aim point; nearest part B Medium's Chest (8.7 cm); the Belly had moved 43.0 cm since the aim was locked
- 29.05 s: ClubSwing at B Small's Head: passed 0.1 cm from the aim point; nearest part B Small's Head (14.9 cm); the Head had moved 29.7 cm since the aim was locked
- 35.60 s: ClubSwing at B Small's Head: passed 0.0 cm from the aim point; nearest part B Small's Head (32.5 cm); the Head had moved 32.8 cm since the aim was locked
- 40.05 s: ClubSwing at B Small's Neck: passed 0.1 cm from the aim point; nearest part B Small's Head (20.6 cm); the Neck had moved 39.3 cm since the aim was locked
- 42.23 s: ClubSwing at B Medium's Chest: passed 0.0 cm from the aim point; nearest part B Medium's Chest (13.9 cm); the Chest had moved 40.4 cm since the aim was locked
- 47.72 s: ClubRising at B Medium's Chest: passed 0.0 cm from the aim point; nearest part B Medium's Head (18.9 cm); the Chest had moved 42.1 cm since the aim was locked
- 60.20 s: ClubSwing at B Medium's Belly: passed 0.2 cm from the aim point; nearest part B Medium's Chest (32.8 cm); the Belly had moved 68.1 cm since the aim was locked


## B Small

Present 0.0–82.8 s. Steps 79, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 14 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.1 | 2 / 28 |
| Chest | Ragdoll | 0.4 / 5.5 | 3 / 39 |
| Head | Ragdoll | 0.6 / 9.3 | 7 / 70 |
| UpperArmL | Ragdoll | 0.6 / 5.7 | 3 / 61 |
| ForearmL | Ragdoll | 1.0 / 15.4 | 5 / 98 |
| HandL | Ragdoll | 1.5 / 36.0 | 17 / 146 |
| UpperArmR | Ragdoll | 0.7 / 8.4 | 7 / 52 |
| ForearmR | Ragdoll | 2.4 / 20.1 | 16 / 79 |
| HandR | Ragdoll | 6.3 / 29.3 | 25 / 102 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 1 |

Hits dealt: 2 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 20 damage. Hits taken: 6, 64 damage. Killed.

Defence: 10 blows came at it (4 seen clearly, 6 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 8.

Pose against the joint limits (standing): a joint more than 10° past its limit 100% of the time; the worst 51° (FootL).
Ragdoll (on the ground 48.8 s): a joint more than 10° past its limit 95% of the time; the worst 145° (ForearmL). Fastest part with a standing leg within 0.5 m: 1.1 m/s median, 7.7 m/s at the 95th percentile (869 steps); with none: 0.0 m/s median (2057 steps).

Look: within 25° of what it attended to 82% of the time standing (85% of the time it was within the head's reach, 96% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 3 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1), 0 of them lead-ins to blows; energy at the end 0.78.

Balance: stumbled 0 times, knocked down 1, launched 4, got up 5; thrown into others 0 times.


## B Medium

Present 0.0–82.8 s. Steps 137, idle shuffles 4. Feet slid at most 0.0 cm while planted; the weight was at most 28 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 8.9 | 3 / 162 |
| Chest | Ragdoll | 0.7 / 17.8 | 4 / 94 |
| Head | Ragdoll | 1.2 / 29.5 | 14 / 106 |
| UpperArmL | Ragdoll | 1.6 / 42.7 | 7 / 172 |
| ForearmL | Ragdoll | 2.6 / 64.1 | 8 / 174 |
| HandL | Ragdoll | 3.7 / 63.5 | 8 / 84 |
| UpperArmR | Ragdoll | 1.1 / 18.3 | 8 / 74 |
| ForearmR | Ragdoll | 3.6 / 33.7 | 13 / 125 |
| HandR | Ragdoll | 6.7 / 69.4 | 20 / 162 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 1 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 11 (1 on the part aimed at, 4 blocked, 1 on someone in the way, 0 on allies), 114 damage. Hits taken: 25, 119 damage. Killed.

Defence: 29 blows came at it (18 seen clearly, 11 at the edge of its view, 0 never seen); it raised the shield against 20.

Pose against the joint limits (standing): a joint more than 10° past its limit 100% of the time; the worst 80° (FootR).
Ragdoll (on the ground 15.8 s): a joint more than 10° past its limit 95% of the time; the worst 176° (ForearmL). Fastest part with a standing leg within 0.5 m: 0.5 m/s median, 5.8 m/s at the 95th percentile (867 steps); with none: 4.4 m/s median (80 steps).

Look: within 25° of what it attended to 83% of the time standing (83% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 83% of the time standing; its rim came within -2.6 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 4 postures (GuardChange 2, ShieldSettle 1, Peek 1), 0 of them lead-ins to blows; energy at the end 0.15.

Balance: stumbled 10 times, knocked down 3, launched 0, got up 3; thrown into others 2 times.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 1.2 cm, max 3.7 cm (posed); physical weapon's closest pass mean 0.5 cm, max 0.6 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.98 s: SwordCut at A Small's Head: passed 0.1 cm from the aim point; nearest part A Small's UpperArmR (14.5 cm); the Head had moved 29.4 cm since the aim was locked
- 4.63 s: SwordBackhand at A Small's Chest: passed 0.4 cm from the aim point; nearest part A Small's UpperArmL (3.2 cm); the Chest had moved 17.6 cm since the aim was locked
- 5.45 s: SwordCleave at A Small's Chest: passed 0.4 cm from the aim point; nearest part A Small's Chest (9.4 cm); the Chest had moved 14.8 cm since the aim was locked
- 59.77 s: SwordCut at A Large's Head: passed 3.7 cm from the aim point; nearest part A Large's UpperArmR (7.3 cm); the Head had moved 40.3 cm since the aim was locked


## B Large

Present 0.0–82.8 s. Steps 28, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 11 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 2 / 18 |
| Chest | ? | 0.8 / 6.7 | 3 / 16 |
| Head | ? | 1.7 / 7.4 | 17 / 72 |
| UpperArmL | ? | 2.4 / 11.5 | 16 / 81 |
| ForearmL | ? | 9.0 / 51.1 | 30 / 178 |
| HandL | ? | 9.8 / 52.3 | 35 / 177 |
| UpperArmR | ? | 0.6 / 8.2 | 2 / 36 |
| ForearmR | ? | 1.8 / 20.5 | 5 / 73 |
| HandR | ? | 3.6 / 55.0 | 8 / 81 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 5, 58 damage. Health at the end 0.

Defence: 3 blows came at it (2 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 99% of the time; the worst 51° (ForearmL).
Ragdoll (on the ground 65.6 s): a joint more than 10° past its limit 88% of the time; the worst 101° (HandL). Fastest part with a standing leg within 0.5 m: 0.1 m/s median, 4.5 m/s at the 95th percentile (250 steps); with none: 0.0 m/s median (3687 steps).

Look: within 25° of what it attended to 91% of the time standing (91% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 6 postures (BoxerSettle 1, BoxerStalk 3, BobAndWeave 1, JabFeint 1), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

## Shield contacts

210 times a shield overlapped another unit's body: 95% under 6.3 cm deep (78% under 2 cm; target 95%), the deepest 17.9 cm, the longest 0.47 s (target under 0.2 s). Each one is in events.log ("shield").

