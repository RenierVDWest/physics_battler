# Playtest 2026-11-02_15-28-37

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 4.5 s of play (270 physics steps), 228 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-02_15-28-37`.

## A Small

Present 0.0–4.5 s. Steps 13, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 7 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.0 | 1 / 6 |
| Chest | ? | 0.2 / 1.2 | 1 / 6 |
| Head | ? | 0.3 / 2.3 | 7 / 73 |
| UpperArmL | ? | 0.4 / 2.2 | 6 / 170 |
| ForearmL | ? | 1.2 / 15.4 | 10 / 99 |
| HandL | ? | 3.5 / 36.0 | 21 / 107 |
| UpperArmR | ? | 0.4 / 2.5 | 4 / 92 |
| ForearmR | ? | 1.1 / 14.5 | 9 / 119 |
| HandR | ? | 2.7 / 31.5 | 23 / 102 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 1, 25 damage. Health at the end 0.

Defence: 2 blows came at it (0 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 69% of the time; the worst 42° (FootL).

Look: within 25° of what it attended to 76% of the time standing (77% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 2 postures (StickSettle 1, AbanicoFlurry 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–4.5 s. Steps 10, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 7 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 1 / 6 |
| Chest | Pinned | 0.3 / 1.6 | 2 / 7 |
| Head | Pinned | 0.5 / 2.1 | 8 / 29 |
| UpperArmL | Pinned | 0.5 / 3.0 | 2 / 53 |
| ForearmL | Pinned | 1.2 / 22.3 | 4 / 97 |
| HandL | Pinned | 2.0 / 41.4 | 2 / 55 |
| UpperArmR | Pinned | 0.7 / 2.5 | 4 / 91 |
| ForearmR | Pinned | 2.0 / 17.3 | 9 / 95 |
| HandR | Pinned | 3.3 / 25.3 | 27 / 112 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 1, 0 damage. Health at the end 100.

Defence: 1 blows came at it (1 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 0.

Pose against the joint limits (standing): a joint more than 10° past its limit 30% of the time; the worst 31° (FootL).

Look: within 25° of what it attended to 86% of the time standing (86% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 98% of the time standing; its rim came within 14.2 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 2 postures (GuardChange 1, Peek 1), 0 of them lead-ins to blows; energy at the end 0.99.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Large

Present 0.0–4.5 s. Steps 10, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 31 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.2 | 2 / 17 |
| Chest | ? | 0.6 / 6.3 | 2 / 9 |
| Head | ? | 0.8 / 7.4 | 14 / 43 |
| UpperArmL | ? | 1.0 / 11.0 | 7 / 50 |
| ForearmL | ? | 4.9 / 30.9 | 13 / 74 |
| HandL | ? | 6.4 / 45.4 | 28 / 116 |
| UpperArmR | ? | 0.7 / 6.2 | 2 / 59 |
| ForearmR | ? | 1.4 / 13.1 | 6 / 68 |
| HandR | ? | 3.4 / 51.7 | 16 / 59 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (0 on the part aimed at, 0 blocked, 1 on someone in the way, 0 on allies), 7 damage. Hits taken: 1, 11 damage. Health at the end 0.

Defence: 1 blows came at it (1 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 45% of the time; the worst 37° (FootR).

Look: within 25° of what it attended to 92% of the time standing (91% of the time it was within the head's reach, 90% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 2 postures (BoxerSettle 1, ClubFeint 1), 0 of them lead-ins to blows; energy at the end 0.00.


## B Small

Present 0.0–4.5 s. Steps 14, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 12 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.0 | 1 / 8 |
| Chest | Pinned | 0.3 / 1.6 | 2 / 8 |
| Head | Pinned | 0.3 / 1.6 | 5 / 16 |
| UpperArmL | Pinned | 0.5 / 3.1 | 4 / 44 |
| ForearmL | Pinned | 1.4 / 15.4 | 8 / 99 |
| HandL | Pinned | 2.6 / 36.0 | 20 / 92 |
| UpperArmR | Kinematic | 0.3 / 1.2 | 3 / 80 |
| ForearmR | Kinematic | 1.0 / 15.7 | 7 / 100 |
| HandR | Kinematic | 2.1 / 24.5 | 18 / 106 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 11 damage. Hits taken: 0, 0 damage. Health at the end 60.

Defence: 1 blows came at it (0 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 65% of the time; the worst 39° (FootL).

Look: within 25° of what it attended to 88% of the time standing (87% of the time it was within the head's reach, 96% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 2 postures (StickSettle 1, AbanicoFlurry 1), 0 of them lead-ins to blows; energy at the end 0.89.


## B Medium

Present 0.0–4.5 s. Steps 8, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 25 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 1 / 16 |
| Chest | Pinned | 0.4 / 4.7 | 2 / 10 |
| Head | Pinned | 0.5 / 3.2 | 8 / 46 |
| UpperArmL | Pinned | 0.6 / 7.0 | 2 / 55 |
| ForearmL | Pinned | 1.2 / 23.5 | 3 / 55 |
| HandL | Pinned | 1.7 / 32.7 | 2 / 61 |
| UpperArmR | Kinematic | 0.5 / 2.6 | 3 / 97 |
| ForearmR | Kinematic | 1.5 / 19.7 | 7 / 99 |
| HandR | Kinematic | 2.3 / 25.6 | 18 / 115 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 25 damage. Hits taken: 0, 0 damage. Health at the end 100.

Pose against the joint limits (standing): a joint more than 10° past its limit 35% of the time; the worst 27° (FootR).

Look: within 25° of what it attended to 97% of the time standing (97% of the time it was within the head's reach, 94% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 99% of the time standing; its rim came within 1.0 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 1 postures (GuardChange 1), 0 of them lead-ins to blows; energy at the end 0.70.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.5 cm, max 0.5 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.93 s: SwordCut at A Small's Neck: passed 0.1 cm from the aim point; nearest part A Small's UpperArmR (23.3 cm); the Neck had moved 40.4 cm since the aim was locked


## B Large

Present 0.0–4.5 s. Steps 9, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 18 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 2 / 17 |
| Chest | Pinned | 0.7 / 6.3 | 2 / 9 |
| Head | Pinned | 0.8 / 7.4 | 6 / 15 |
| UpperArmL | Pinned | 1.3 / 11.0 | 6 / 50 |
| ForearmL | Pinned | 4.4 / 23.7 | 9 / 63 |
| HandL | Pinned | 5.6 / 45.4 | 22 / 72 |
| UpperArmR | Pinned | 0.6 / 6.2 | 2 / 59 |
| ForearmR | Pinned | 1.3 / 13.1 | 5 / 68 |
| HandR | Pinned | 3.4 / 51.7 | 15 / 59 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 1 / 148 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (0 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 1, 7 damage. Health at the end 213.

Pose against the joint limits (standing): a joint more than 10° past its limit 31% of the time; the worst 148° (ShinR).

Look: within 25° of what it attended to 97% of the time standing (97% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 2 postures (BoxerSettle 1, Shift 1), 0 of them lead-ins to blows; energy at the end 1.00.

## Shield contacts

12 times a shield overlapped another unit's body: 95% under 11.6 cm deep (42% under 2 cm; target 95%), the deepest 11.6 cm, the longest 0.07 s (target under 0.2 s). Each one is in events.log ("shield").

