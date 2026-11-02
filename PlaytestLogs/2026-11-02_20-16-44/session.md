# Playtest 2026-11-02_20-16-44

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 48.9 s of play (2933 physics steps), 2851 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-02_20-16-44`.

## A Small

Present 0.0–48.9 s. Steps 15, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 6 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.0 | 4 / 46 |
| Chest | ? | 0.8 / 6.6 | 8 / 31 |
| Head | ? | 1.6 / 8.7 | 16 / 78 |
| UpperArmL | ? | 1.2 / 8.6 | 10 / 173 |
| ForearmL | ? | 2.1 / 14.6 | 12 / 97 |
| HandL | ? | 3.6 / 36.4 | 28 / 168 |
| UpperArmR | ? | 1.6 / 8.8 | 8 / 49 |
| ForearmR | ? | 2.8 / 19.5 | 15 / 78 |
| HandR | ? | 7.0 / 32.3 | 19 / 99 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 3, 68 damage. Health at the end 0.

Defence: 3 blows came at it (1 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 66% of the time; the worst 42° (FootL).
Ragdoll (on the ground 43.5 s): a joint more than 10° past its limit 12% of the time; the worst 138° (ShinR). Fastest part with a standing leg within 0.5 m: 0.9 m/s median, 5.6 m/s at the 95th percentile (935 steps); with none: 0.0 m/s median (1677 steps).

Look: within 25° of what it attended to 78% of the time standing (78% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 2 postures (StickSettle 1, AbanicoFlurry 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–48.9 s. Steps 98, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 60 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.1 / 29.1 | 3 / 137 |
| Chest | ? | 0.9 / 64.4 | 5 / 164 |
| Head | ? | 1.3 / 71.9 | 14 / 140 |
| UpperArmL | ? | 1.7 / 86.2 | 6 / 179 |
| ForearmL | ? | 2.3 / 97.5 | 7 / 180 |
| HandL | ? | 3.3 / 120.1 | 5 / 153 |
| UpperArmR | ? | 0.8 / 20.7 | 3 / 54 |
| ForearmR | ? | 1.7 / 42.6 | 4 / 115 |
| HandR | ? | 2.5 / 82.7 | 5 / 126 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 4 |
| ThighR | ? | 0.0 / 0.0 | 0 / 150 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 12 (0 on the part aimed at, 4 blocked, 0 on someone in the way, 2 on allies), 73 damage. Hits taken: 7, 74 damage. Health at the end 0.

Defence: 7 blows came at it (4 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 5.

Pose against the joint limits (standing): a joint more than 10° past its limit 36% of the time; the worst 115° (ShinR).
Ragdoll (on the ground 2.8 s): a joint more than 10° past its limit 69% of the time; the worst 148° (ForearmR). Fastest part with a standing leg within 0.5 m: 5.3 m/s median, 9.0 m/s at the 95th percentile (23 steps); with none: 3.3 m/s median (142 steps).

Look: within 25° of what it attended to 81% of the time standing (84% of the time it was within the head's reach, 96% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 87% of the time standing; its rim came within 0.1 cm of its own body (negative: inside).

Fight Rhythm (?): 5 postures (GuardChange 1, Peek 2, HighFeint 1, ShieldAdvance 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 4 times, knocked down 2, launched 0, got up 2; thrown into others 1 time.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.4 cm, max 1.2 cm (posed); physical weapon's closest pass mean 0.1 cm, max 0.3 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.47 s: SwordCut at B Large's Belly: passed 0.0 cm from the aim point; nearest part B Large's Pelvis (16.2 cm); the Belly had moved 33.4 cm since the aim was locked
- 5.23 s: SwordCut at B Large's Head: passed 1.2 cm from the aim point; nearest part B Large's Chest (28.1 cm); the Head had moved 60.9 cm since the aim was locked
- 41.90 s: SwordBackhand at B Medium's Belly: passed 0.0 cm from the aim point; nearest part B Medium's Pelvis (15.2 cm); the Belly had moved 17.8 cm since the aim was locked
- 43.88 s: SwordThrust at B Small's Belly: passed 0.3 cm from the aim point; nearest part B Small's Chest (12.3 cm); the Belly had moved 16.4 cm since the aim was locked


## A Large

Present 0.0–48.9 s. Steps 91, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 50 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 7.4 | 0 / 0 |
| Spine | Pinned | 0.0 / 7.4 | 2 / 19 |
| Chest | Pinned | 0.9 / 9.9 | 3 / 29 |
| Head | Pinned | 1.3 / 13.2 | 14 / 76 |
| UpperArmL | Pinned | 1.6 / 11.4 | 6 / 58 |
| ForearmL | Pinned | 3.9 / 24.1 | 9 / 58 |
| HandL | Pinned | 5.4 / 49.5 | 22 / 137 |
| UpperArmR | Muscled | 1.4 / 21.8 | 3 / 55 |
| ForearmR | Muscled | 2.6 / 30.0 | 4 / 64 |
| HandR | Muscled | 3.5 / 52.0 | 6 / 79 |
| ThighL | Kinematic | 0.0 / 7.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 1 |
| ThighR | Kinematic | 0.0 / 7.9 | 0 / 150 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 1 |

Hits dealt: 14 (3 on the part aimed at, 4 blocked, 0 on someone in the way, 1 on allies), 134 damage. Hits taken: 6, 72 damage. Health at the end 148.

Defence: 9 blows came at it (3 seen clearly, 6 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 5.

Pose against the joint limits (standing): a joint more than 10° past its limit 39% of the time; the worst 109° (ShinR).

Look: within 25° of what it attended to 84% of the time standing (86% of the time it was within the head's reach, 88% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 12 postures (BoxerSettle 1, BobAndWeave 3, ClubFeint 3, Shift 2, BoxerStalk 2, JabFeint 1), 3 of them lead-ins to blows; energy at the end 0.33.

Blows: 8. The posed striking part passed within 3 cm of the aim point in 7; aim error mean 1.3 cm, max 9.5 cm (posed); physical weapon's closest pass mean 2.8 cm, max 9.5 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.65 s: ClubRising at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's HandR (21.5 cm); the Belly had moved 27.5 cm since the aim was locked
- 4.50 s: ClubSwing at B Small's Head: passed 0.5 cm from the aim point; nearest part B Small's ForearmR (32.0 cm); the Head had moved 64.3 cm since the aim was locked
- 5.23 s: ClubBackhand at B Small's Neck: passed 9.5 cm from the aim point; nearest part B Small's ThighR (10.4 cm); the Neck had moved 61.2 cm since the aim was locked
- 11.77 s: ClubSwing at B Small's Belly: passed 0.1 cm from the aim point; nearest part B Small's ForearmL (13.7 cm); the Belly had moved 34.8 cm since the aim was locked
- 13.62 s: ClubBackhand at B Large's Neck: passed 0.0 cm from the aim point; nearest part B Large's Chest (46.6 cm); the Neck had moved 76.2 cm since the aim was locked
- 23.30 s: ClubBackhand at B Small's ElbowL: passed 0.1 cm from the aim point; nearest part B Small's HandR (1.6 cm); the ElbowL had moved 32.0 cm since the aim was locked
- 29.78 s: ClubSwing at B Medium's Head: passed 0.0 cm from the aim point; nearest part B Medium's Head (0.0 cm); the Head had moved 11.9 cm since the aim was locked
- 36.00 s: ClubSwing at B Medium's Neck: passed 0.0 cm from the aim point; nearest part B Medium's Head (7.6 cm); the Neck had moved 5.1 cm since the aim was locked


## B Small

Present 0.0–48.9 s. Steps 104, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 29 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 3 / 40 |
| Chest | ? | 0.6 / 6.9 | 5 / 31 |
| Head | ? | 0.9 / 8.1 | 8 / 65 |
| UpperArmL | ? | 1.0 / 12.2 | 5 / 71 |
| ForearmL | ? | 1.7 / 16.5 | 8 / 96 |
| HandL | ? | 2.3 / 36.3 | 21 / 133 |
| UpperArmR | ? | 0.9 / 8.6 | 5 / 43 |
| ForearmR | ? | 1.8 / 17.7 | 9 / 85 |
| HandR | ? | 3.7 / 38.9 | 11 / 101 |
| ThighL | ? | 0.0 / 0.0 | 0 / 145 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 11 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 3 (1 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 33 damage. Hits taken: 5, 62 damage. Health at the end 0.

Defence: 6 blows came at it (2 seen clearly, 4 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 60% of the time; the worst 85° (ShinL).
Ragdoll (on the ground 10.0 s): a joint more than 10° past its limit 92% of the time; the worst 144° (ForearmR). Fastest part with a standing leg within 0.5 m: 0.1 m/s median, 6.6 m/s at the 95th percentile (418 steps); with none: 1.5 m/s median (182 steps).

Look: within 25° of what it attended to 82% of the time standing (87% of the time it was within the head's reach, 93% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 10 postures (StickSettle 3, AbanicoFlurry 3, BaitRetreat 1, RedondaTwirl 2, StickStalk 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 0 times, knocked down 0, launched 3, got up 3; thrown into others 0 times.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.0 cm, max 0.1 cm (posed); physical weapon's closest pass mean 1.7 cm, max 4.3 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 21.42 s: ClubRising at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (27.0 cm); the KneeL had moved 40.3 cm since the aim was locked
- 22.72 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (26.6 cm); the KneeL had moved 31.3 cm since the aim was locked
- 40.95 s: ClubBackhand at A Large's ElbowR: passed 0.1 cm from the aim point; nearest part A Large's Pelvis (90.5 cm); the ElbowR had moved 62.0 cm since the aim was locked
- 42.32 s: ClubSwing at A Large's KneeL: passed 0.0 cm from the aim point; nearest part A Large's ShinL (6.0 cm); the KneeL had moved 14.4 cm since the aim was locked


## B Medium

Present 0.0–48.9 s. Steps 72, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 32 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.1 | 2 / 33 |
| Chest | ? | 0.7 / 9.8 | 4 / 31 |
| Head | ? | 1.0 / 6.6 | 8 / 44 |
| UpperArmL | ? | 1.1 / 9.2 | 4 / 64 |
| ForearmL | ? | 1.9 / 23.6 | 6 / 64 |
| HandL | ? | 2.6 / 32.8 | 6 / 106 |
| UpperArmR | ? | 1.1 / 8.2 | 4 / 37 |
| ForearmR | ? | 2.3 / 20.4 | 7 / 63 |
| HandR | ? | 3.7 / 40.1 | 9 / 82 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 5 (1 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 78 damage. Hits taken: 15, 92 damage. Health at the end 0.

Defence: 15 blows came at it (9 seen clearly, 6 at the edge of its view, 0 never seen); it raised the shield against 8.

Pose against the joint limits (standing): a joint more than 10° past its limit 32% of the time; the worst 36° (FootL).
Ragdoll (on the ground 8.5 s): a joint more than 10° past its limit 95% of the time; the worst 160° (ForearmL). Fastest part with a standing leg within 0.5 m: - median, - at the 95th percentile (0 steps); with none: 0.3 m/s median (508 steps).

Look: within 25° of what it attended to 85% of the time standing (86% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 88% of the time standing; its rim came within -1.7 cm of its own body (negative: inside).

Fight Rhythm (?): 2 postures (GuardChange 1, Peek 1), 0 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 6 times, knocked down 2, launched 0, got up 2; thrown into others 0 times.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.4 cm, max 0.7 cm (posed); physical weapon's closest pass mean 0.7 cm, max 0.7 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.98 s: SwordCut at A Small's Head: passed 0.1 cm from the aim point; nearest part A Small's Head (17.0 cm); the Head had moved 28.6 cm since the aim was locked
- 4.63 s: SwordBackhand at A Small's Chest: passed 0.7 cm from the aim point; nearest part A Small's UpperArmL (2.4 cm); the Chest had moved 16.2 cm since the aim was locked


## B Large

Present 0.0–48.9 s. Steps 29, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 35 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 4.2 | 4 / 129 |
| Chest | ? | 1.4 / 19.8 | 4 / 56 |
| Head | ? | 2.0 / 18.6 | 15 / 91 |
| UpperArmL | ? | 0.4 / 9.9 | 1 / 41 |
| ForearmL | ? | 0.9 / 24.1 | 2 / 58 |
| HandL | ? | 1.1 / 45.0 | 4 / 72 |
| UpperArmR | ? | 2.2 / 38.1 | 4 / 82 |
| ForearmR | ? | 3.9 / 66.3 | 6 / 80 |
| HandR | ? | 5.7 / 94.6 | 8 / 82 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 150 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 5 (2 on the part aimed at, 0 blocked, 0 on someone in the way, 2 on allies), 76 damage. Hits taken: 5, 46 damage. Health at the end 0.

Defence: 6 blows came at it (4 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 4.

Pose against the joint limits (standing): a joint more than 10° past its limit 24% of the time; the worst 102° (ShinR).
Ragdoll (on the ground 31.3 s): a joint more than 10° past its limit 2% of the time; the worst 49° (Head). Fastest part with a standing leg within 0.5 m: - median, - at the 95th percentile (0 steps); with none: 0.0 m/s median (1880 steps).

Look: within 25° of what it attended to 87% of the time standing (89% of the time it was within the head's reach, 95% of the time); head outside its limits 0 steps.

Fight Rhythm (?): 3 postures (BoxerSettle 1, BoxerStalk 1, BobAndWeave 1), 1 of them lead-ins to blows; energy at the end 0.00.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.6 cm, max 1.4 cm (posed); physical weapon's closest pass mean 9.5 cm, max 27.3 cm. 2 reached the part they were aimed at (nearest part within 5 cm at contact).

- 5.60 s: ClubRising at A Medium's Belly: passed 0.5 cm from the aim point; nearest part A Medium's ForearmL (41.5 cm); the Belly had moved 106.5 cm since the aim was locked
- 6.33 s: ClubSwing at A Medium's Belly: passed 0.5 cm from the aim point; nearest part A Medium's HandR (51.5 cm); the Belly had moved 54.8 cm since the aim was locked
- 7.10 s: ClubSmash at A Medium's Head: passed 1.4 cm from the aim point; nearest part A Medium's Head (3.2 cm); the Head had moved 9.4 cm since the aim was locked
- 12.25 s: ClubBackhand at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Chest (0.0 cm); the Chest had moved 14.2 cm since the aim was locked

## Shield contacts

90 times a shield overlapped another unit's body: 95% under 8.2 cm deep (70% under 2 cm; target 95%), the deepest 10.7 cm, the longest 0.22 s (target under 0.2 s). Each one is in events.log ("shield").

## Performance

22803 frames in 49.6 s (460 fps on average). Frame time: median 1.2 ms, 95% 6.2, 99% 12.3, worst 760.7. 55 hitches over 40 ms (16 over 100 ms). 68 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- ArmSolve: 221.1
- UnitStep: 197.5
- Compose: 170.3
- ComposeDry: 75.5
- Sandbox: 69.3
- RecorderSample: 66.4
- Clearance: 34.9
- Plan: 29.1
- RecorderFlush: 22.7
- CaptureSave: 20.8
- Prepare: 4.4
- Authority: 4.2
- TryOut: 2.1
- Sweep: 0.7
- CaptureRender: 0.1

The worst frames:

- 0.02 s: 761 ms frame, 1 physics steps, garbage collected (gen0 4, gen1 4, gen2 4); ours: UnitStep 37.7 ms (6), RecorderSample 34.1 ms (1), Compose 27.8 ms (12), ArmSolve 8.7 ms (18), Authority 3.8 ms (6), Sandbox 2.4 ms (1); Unity: Gfx_WaitForGfxCommandsFromMainThread 434.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 77.0 ms, FixedBehaviourUpdate 77.0 ms, Physics_Simulate 14.4 ms, GC_Collect 14.0 ms, GC_Alloc 2.1 ms, Update_ScriptRunBehaviourUpdate 1.4 ms, BehaviourUpdate 1.4 ms
- 4.73 s: 139 ms frame, 6 physics steps; ours: UnitStep 128.4 ms (36), ArmSolve 108.2 ms (1862), ComposeDry 86.9 ms (95), Clearance 36.5 ms (1), Compose 26.4 ms (30), Prepare 19.4 ms (7); Unity: Main_Thread 139.7 ms, Gfx_WaitForGfxCommandsFromMainThread 138.0 ms, PlayerLoop 134.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 132.0 ms, FixedBehaviourUpdate 131.9 ms, CPU_Total_Frame_Time 1.9 ms, FixedUpdate_PhysicsFixedUpdate 1.8 ms, Physics_Simulate 1.5 ms
- 35.06 s: 129 ms frame, 7 physics steps; ours: ArmSolve 99.1 ms (1554), Sandbox 95.7 ms (7), ComposeDry 93.7 ms (80), Plan 50.9 ms (1), Clearance 47.2 ms (1), UnitStep 23.6 ms (42); Unity: Main_Thread 129.5 ms, Gfx_WaitForGfxCommandsFromMainThread 128.1 ms, PlayerLoop 126.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 124.0 ms, FixedBehaviourUpdate 124.0 ms, FixedUpdate_PhysicsFixedUpdate 1.5 ms, Physics_Simulate 1.4 ms, CPU_Total_Frame_Time 1.1 ms
- 40.42 s: 124 ms frame, 1 physics steps; ours: Sandbox 115.3 ms (1), ComposeDry 112.0 ms (77), ArmSolve 105.2 ms (1478), Plan 55.3 ms (1), Clearance 51.4 ms (1), UnitStep 4.4 ms (6); Unity: Main_Thread 123.8 ms, Gfx_WaitForGfxCommandsFromMainThread 122.6 ms, PlayerLoop 121.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 120.4 ms, FixedBehaviourUpdate 120.4 ms, CPU_Total_Frame_Time 1.1 ms, GPU_Frame_Time 0.5 ms, FixedUpdate_PhysicsFixedUpdate 0.4 ms
- 43.61 s: 123 ms frame, 1 physics steps; ours: Sandbox 107.6 ms (1), ComposeDry 100.8 ms (82), ArmSolve 94.7 ms (1535), Plan 58.4 ms (1), Clearance 47.4 ms (1), RecorderSample 10.7 ms (1); Unity: Main_Thread 123.4 ms, Gfx_WaitForGfxCommandsFromMainThread 122.6 ms, PlayerLoop 121.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 121.2 ms, FixedBehaviourUpdate 121.2 ms, CPU_Total_Frame_Time 1.1 ms, CPU_Render_Thread_Frame_Time 0.5 ms, GPU_Frame_Time 0.5 ms
- 2.60 s: 121 ms frame, 1 physics steps; ours: Sandbox 95.4 ms (1), ComposeDry 78.5 ms (77), ArmSolve 71.6 ms (1111), Plan 49.4 ms (1), Clearance 34.9 ms (1), RecorderSample 14.5 ms (1); Unity: Main_Thread 120.3 ms, Gfx_WaitForGfxCommandsFromMainThread 119.3 ms, PlayerLoop 117.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 116.8 ms, FixedBehaviourUpdate 116.8 ms, CPU_Total_Frame_Time 1.7 ms, GPU_Frame_Time 0.5 ms, CPU_Render_Thread_Frame_Time 0.4 ms
- 4.03 s: 120 ms frame, 5 physics steps; ours: ArmSolve 96.1 ms (1383), Sandbox 84.5 ms (5), ComposeDry 81.0 ms (77), Clearance 35.8 ms (1), UnitStep 23.3 ms (30), Compose 20.8 ms (25); Unity: Main_Thread 117.1 ms, Gfx_WaitForGfxCommandsFromMainThread 115.9 ms, PlayerLoop 114.1 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 110.9 ms, FixedBehaviourUpdate 110.9 ms, FixedUpdate_PhysicsFixedUpdate 2.0 ms, Physics_Simulate 1.6 ms, CPU_Total_Frame_Time 1.1 ms
- 15.20 s: 118 ms frame, 1 physics steps; ours: Sandbox 111.0 ms (1), ComposeDry 107.4 ms (77), ArmSolve 99.9 ms (1430), Plan 54.4 ms (1), Clearance 50.2 ms (1), Prepare 3.1 ms (3); Unity: Main_Thread 117.0 ms, Gfx_WaitForGfxCommandsFromMainThread 116.2 ms, PlayerLoop 115.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 114.6 ms, FixedBehaviourUpdate 114.6 ms, CPU_Total_Frame_Time 1.1 ms, CPU_Render_Thread_Frame_Time 0.6 ms, FixedUpdate_PhysicsFixedUpdate 0.5 ms
- 34.93 s: 118 ms frame, 1 physics steps; ours: Sandbox 110.7 ms (1), ComposeDry 105.2 ms (113), ArmSolve 101.5 ms (1638), Plan 76.8 ms (1), Clearance 70.0 ms (2), Prepare 5.6 ms (2); Unity: Main_Thread 116.7 ms, Gfx_WaitForGfxCommandsFromMainThread 115.9 ms, PlayerLoop 115.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 114.7 ms, FixedBehaviourUpdate 114.7 ms, CPU_Total_Frame_Time 1.5 ms, GPU_Frame_Time 0.5 ms, CPU_Render_Thread_Frame_Time 0.5 ms
- 4.90 s: 116 ms frame, 3 physics steps; ours: UnitStep 107.1 ms (18), ArmSolve 91.4 ms (1567), ComposeDry 87.1 ms (77), Clearance 42.4 ms (1), Compose 15.7 ms (15), Prepare 3.1 ms (1); Unity: Main_Thread 117.0 ms, Gfx_WaitForGfxCommandsFromMainThread 114.9 ms, PlayerLoop 111.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 108.9 ms, FixedBehaviourUpdate 108.9 ms, CPU_Total_Frame_Time 41.8 ms, CPU_Main_Thread_Frame_Time 39.6 ms, FixedUpdate_PhysicsFixedUpdate 1.2 ms
- 43.30 s: 113 ms frame, 1 physics steps; ours: Sandbox 106.7 ms (1), ComposeDry 103.5 ms (77), ArmSolve 87.4 ms (1438), Plan 48.9 ms (1), Clearance 44.2 ms (1), Prepare 3.1 ms (1); Unity: Main_Thread 112.4 ms, Gfx_WaitForGfxCommandsFromMainThread 111.6 ms, PlayerLoop 110.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 109.8 ms, FixedBehaviourUpdate 109.8 ms, CPU_Total_Frame_Time 1.1 ms, FixedUpdate_PhysicsFixedUpdate 0.5 ms, GPU_Frame_Time 0.5 ms
- 3.21 s: 108 ms frame, 1 physics steps; ours: Sandbox 98.7 ms (1), ComposeDry 86.7 ms (92), ArmSolve 84.3 ms (1310), Clearance 34.8 ms (1), Prepare 19.0 ms (6), TryOut 8.9 ms (12); Unity: Main_Thread 107.5 ms, Gfx_WaitForGfxCommandsFromMainThread 106.4 ms, PlayerLoop 105.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 104.3 ms, FixedBehaviourUpdate 104.3 ms, CPU_Total_Frame_Time 1.1 ms, GPU_Frame_Time 0.7 ms, CPU_Render_Thread_Frame_Time 0.5 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

