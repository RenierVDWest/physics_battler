# Playtest 2026-11-02_21-30-27

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 38.8 s of play (2327 physics steps), 1818 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-02_21-30-27`.

## A Small

Present 0.0–38.8 s. Steps 22, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 11 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.0 | 4 / 18 |
| Chest | Ragdoll | 0.7 / 3.6 | 8 / 31 |
| Head | Ragdoll | 1.5 / 5.9 | 10 / 28 |
| UpperArmL | Ragdoll | 1.3 / 7.2 | 7 / 54 |
| ForearmL | Ragdoll | 2.1 / 14.6 | 18 / 178 |
| HandL | Ragdoll | 3.8 / 36.4 | 25 / 92 |
| UpperArmR | Ragdoll | 1.5 / 5.9 | 8 / 49 |
| ForearmR | Ragdoll | 2.8 / 19.5 | 15 / 78 |
| HandR | Ragdoll | 6.8 / 32.3 | 19 / 99 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 4, 73 damage. Killed.

Defence: 4 blows came at it (2 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 60% of the time; the worst 42° (FootL).
Ragdoll (on the ground 31.4 s): a joint more than 10° past its limit 56% of the time; the worst 141° (HandL). Fastest part with a standing leg within 0.5 m: 0.2 m/s median, 3.6 m/s at the 95th percentile (1387 steps); with none: 0.1 m/s median (496 steps).

Look: within 25° of what it attended to 79% of the time standing (79% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 2 postures (StickSettle 1, AbanicoFlurry 1), 0 of them lead-ins to blows; energy at the end 0.77.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–38.8 s. Steps 45, idle shuffles 4. Feet slid at most 0.0 cm while planted; the weight was at most 24 cm from the support point; hips dropped for reach by up to 1 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 5.8 | 2 / 84 |
| Chest | Ragdoll | 0.7 / 21.3 | 3 / 76 |
| Head | Ragdoll | 1.0 / 30.2 | 10 / 76 |
| UpperArmL | Ragdoll | 1.2 / 40.8 | 6 / 114 |
| ForearmL | Ragdoll | 2.8 / 57.1 | 8 / 177 |
| HandL | Ragdoll | 3.9 / 77.6 | 9 / 132 |
| UpperArmR | Ragdoll | 0.9 / 27.2 | 3 / 53 |
| ForearmR | Ragdoll | 1.7 / 34.7 | 5 / 67 |
| HandR | Ragdoll | 2.7 / 43.3 | 6 / 86 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 1 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 7, 32 damage. Killed.

Defence: 5 blows came at it (3 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 4.

Pose against the joint limits (standing): a joint more than 10° past its limit 26% of the time; the worst 100° (FootR).
Ragdoll (on the ground 8.5 s): a joint more than 10° past its limit 92% of the time; the worst 71° (FootR). Fastest part with a standing leg within 0.5 m: 0.0 m/s median, 2.7 m/s at the 95th percentile (484 steps); with none: 3.5 m/s median (26 steps).

Look: within 25° of what it attended to 86% of the time standing (87% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 92% of the time standing; its rim came within 1.0 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 4 postures (GuardChange 1, Peek 1, ShieldPress 1, ShieldSettle 1), 0 of them lead-ins to blows; energy at the end 1.00.

Balance: stumbled 4 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Large

Present 0.0–38.8 s. Steps 49, idle shuffles 2. Feet slid at most 5.6 cm while planted; the weight was at most 75 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 15.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 15.0 | 2 / 19 |
| Chest | Ragdoll | 0.7 / 15.7 | 2 / 28 |
| Head | Ragdoll | 1.1 / 14.9 | 12 / 115 |
| UpperArmL | Ragdoll | 1.2 / 14.1 | 4 / 41 |
| ForearmL | Ragdoll | 2.9 / 24.1 | 7 / 66 |
| HandL | Ragdoll | 4.4 / 45.0 | 18 / 107 |
| UpperArmR | Ragdoll | 0.8 / 21.2 | 2 / 46 |
| ForearmR | Ragdoll | 1.4 / 38.7 | 3 / 81 |
| HandR | Ragdoll | 2.2 / 73.8 | 4 / 100 |
| ThighL | Ragdoll | 0.0 / 14.5 | 0 / 11 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 15.4 | 0 / 150 |
| ShinR | Ragdoll | 0.0 / 8.4 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 6.7 | 0 / 0 |

Hits dealt: 6 (0 on the part aimed at, 1 blocked, 0 on someone in the way, 0 on allies), 71 damage. Hits taken: 2, 14 damage. Killed.

Pose against the joint limits (standing): a joint more than 10° past its limit 24% of the time; the worst 102° (ShinR).
Ragdoll (on the ground 3.0 s): a joint more than 10° past its limit 64% of the time; the worst 94° (Head). Fastest part with a standing leg within 0.5 m: 0.2 m/s median, 3.9 m/s at the 95th percentile (179 steps); with none: - median (0 steps).

Look: within 25° of what it attended to 91% of the time standing (88% of the time it was within the head's reach, 78% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 12 postures (BoxerSettle 2, ClubFeint 1, BobAndWeave 2, BoxerStalk 4, JabFeint 3), 1 of them lead-ins to blows; energy at the end 1.00.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.3 cm, max 0.6 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.00 s: ClubSwing at B Small's KneeL: passed 0.0 cm from the aim point; nearest part B Small's FootR (6.8 cm); the KneeL had moved 44.0 cm since the aim was locked
- 6.05 s: ClubRising at B Large's Belly: passed 0.1 cm from the aim point; nearest part B Large's Pelvis (39.1 cm); the Belly had moved 76.4 cm since the aim was locked
- 8.48 s: ClubBackhand at B Small's Belly: passed 0.0 cm from the aim point; nearest part B Small's Pelvis (0.0 cm); the Belly had moved 14.3 cm since the aim was locked
- 15.10 s: OffJab at B Large's Neck: passed 0.1 cm from the aim point; nearest part B Large's HandL (24.7 cm); the Neck had moved 73.6 cm since the aim was locked


## B Small

Present 0.0–38.8 s. Steps 60, idle shuffles 7. Feet slid at most 0.0 cm while planted; the weight was at most 9 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.0 | 3 / 18 |
| Chest | Pinned | 0.7 / 3.5 | 6 / 28 |
| Head | Pinned | 1.2 / 5.3 | 7 / 31 |
| UpperArmL | Pinned | 1.0 / 4.9 | 4 / 62 |
| ForearmL | Pinned | 1.2 / 14.4 | 7 / 96 |
| HandL | Pinned | 2.0 / 36.3 | 18 / 87 |
| UpperArmR | Muscled | 1.4 / 8.2 | 7 / 43 |
| ForearmR | Muscled | 2.4 / 17.7 | 11 / 69 |
| HandR | Muscled | 5.0 / 27.5 | 15 / 91 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 2, 38 damage. Health at the end 22.

Defence: 2 blows came at it (1 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 35% of the time; the worst 42° (FootR).
Ragdoll (on the ground 2.9 s): a joint more than 10° past its limit 65% of the time; the worst 176° (ForearmL). Fastest part with a standing leg within 0.5 m: 9.2 m/s median, 10.5 m/s at the 95th percentile (17 steps); with none: 2.1 m/s median (157 steps).

Look: within 25° of what it attended to 93% of the time standing (94% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 18 postures (StickSettle 3, AbanicoFlurry 6, BaitRetreat 1, StickStalk 3, TriangleStep 4, RedondaTwirl 1), 0 of them lead-ins to blows; energy at the end 0.99.

Balance: stumbled 0 times, knocked down 1, launched 1, got up 2; thrown into others 0 times.


## B Medium

Present 0.0–38.8 s. Steps 41, idle shuffles 6. Feet slid at most 0.0 cm while planted; the weight was at most 46 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 2 / 14 |
| Chest | Pinned | 0.6 / 4.1 | 4 / 26 |
| Head | Pinned | 1.1 / 8.5 | 9 / 54 |
| UpperArmL | Pinned | 1.0 / 6.6 | 3 / 55 |
| ForearmL | Pinned | 1.1 / 23.6 | 3 / 64 |
| HandL | Pinned | 0.9 / 32.8 | 1 / 61 |
| UpperArmR | Muscled | 1.2 / 9.3 | 5 / 30 |
| ForearmR | Muscled | 2.6 / 15.9 | 7 / 63 |
| HandR | Muscled | 3.9 / 28.2 | 8 / 82 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 1 |

Hits dealt: 8 (2 on the part aimed at, 0 blocked, 1 on someone in the way, 1 on allies), 112 damage. Hits taken: 1, 27 damage. Health at the end 73.

Defence: 1 blows came at it (1 seen clearly, 0 at the edge of its view, 0 never seen); it raised the shield against 0.

Pose against the joint limits (standing): a joint more than 10° past its limit 16% of the time; the worst 32° (FootL).
Ragdoll (on the ground 1.1 s): a joint more than 10° past its limit 82% of the time; the worst 38° (HandL). Fastest part with a standing leg within 0.5 m: - median, - at the 95th percentile (0 steps); with none: 2.8 m/s median (67 steps).

Look: within 25° of what it attended to 95% of the time standing (95% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 97% of the time standing; its rim came within 0.6 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 12 postures (GuardChange 4, ShieldSettle 2, ShieldPress 4, ShieldAdvance 1, Peek 1), 1 of them lead-ins to blows; energy at the end 1.00.

Balance: stumbled 1 time, knocked down 1, launched 0, got up 1; thrown into others 0 times.

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.2 cm, max 0.7 cm (posed); physical weapon's closest pass mean 0.3 cm, max 0.7 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.40 s: SwordCut at A Small's Neck: passed 0.1 cm from the aim point; nearest part A Small's Chest (3.1 cm); the Neck had moved 26.8 cm since the aim was locked
- 5.05 s: SwordBackhand at A Small's Chest: passed 0.3 cm from the aim point; nearest part A Small's Chest (0.0 cm); the Chest had moved 9.8 cm since the aim was locked
- 5.67 s: SwordCleave at A Small's Chest: passed 0.7 cm from the aim point; nearest part A Small's Chest (11.3 cm); the Chest had moved 2.7 cm since the aim was locked
- 7.38 s: SwordCut at A Small's Neck: passed 0.0 cm from the aim point; nearest part A Small's Head (10.9 cm); the Neck had moved 15.1 cm since the aim was locked
- 18.63 s: SwordCut at A Medium's ElbowL: passed 0.0 cm from the aim point; nearest part A Medium's Pelvis (11.2 cm); the ElbowL had moved 55.7 cm since the aim was locked


## B Large

Present 0.0–38.8 s. Steps 57, idle shuffles 3. Feet slid at most 0.0 cm while planted; the weight was at most 49 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 4.5 | 0 / 0 |
| Spine | Pinned | 0.0 / 18.0 | 4 / 177 |
| Chest | Pinned | 1.6 / 43.9 | 5 / 120 |
| Head | Pinned | 2.6 / 68.1 | 13 / 177 |
| UpperArmL | Pinned | 2.6 / 89.6 | 8 / 173 |
| ForearmL | Pinned | 4.3 / 64.3 | 11 / 166 |
| HandL | Pinned | 5.5 / 70.1 | 23 / 177 |
| UpperArmR | Muscled | 2.6 / 75.7 | 5 / 112 |
| ForearmR | Muscled | 5.0 / 90.8 | 7 / 105 |
| HandR | Muscled | 6.1 / 120.7 | 10 / 111 |
| ThighL | Kinematic | 0.0 / 4.4 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 4.6 | 0 / 149 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 6 (1 on the part aimed at, 3 blocked, 1 on someone in the way, 1 on allies), 38 damage. Hits taken: 4, 36 damage. Health at the end 184.

Defence: 8 blows came at it (5 seen clearly, 3 at the edge of its view, 0 never seen); it raised the shield against 0, and had nothing to defend with against 3.

Pose against the joint limits (standing): a joint more than 10° past its limit 17% of the time; the worst 102° (ShinR).

Look: within 25° of what it attended to 88% of the time standing (89% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 13 postures (BoxerSettle 1, ClubFeint 1, JabFeint 5, BobAndWeave 1, BoxerStalk 5), 1 of them lead-ins to blows; energy at the end 1.00.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 5.53 s: ClubSwing at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Chest (21.5 cm); the Chest had moved 44.7 cm since the aim was locked
- 14.33 s: ClubSwing at A Medium's ElbowR: passed 0.1 cm from the aim point; nearest part A Medium's ForearmR (5.1 cm); the ElbowR had moved 12.0 cm since the aim was locked

## Shield contacts

114 times a shield overlapped another unit's body: 95% under 7.8 cm deep (85% under 2 cm; target 95%), the deepest 12.7 cm, the longest 0.42 s (target under 0.2 s). Each one is in events.log ("shield").

## Performance

22900 frames in 39.6 s (579 fps on average). Frame time: median 1.2 ms, 95% 4.2, 99% 7.3, worst 775.7. 11 hitches over 40 ms (1 over 100 ms). 58 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 100.2
- Compose: 82.7
- ArmSolve: 76.8
- RecorderSample: 69.8
- RecorderFlush: 24.7
- CaptureSave: 20.5
- Sandbox: 17.4
- ComposeDry: 15.2
- Clearance: 11.2
- Plan: 8.5
- Authority: 5.4
- Prepare: 1.7
- Sweep: 0.7
- TryOut: 0.5
- CaptureRender: 0.1

The worst frames:

- 0.02 s: 776 ms frame, 1 physics steps, garbage collected (gen0 4, gen1 4, gen2 4); ours: RecorderSample 41.9 ms (1), UnitStep 36.1 ms (6), Compose 25.7 ms (12), ArmSolve 6.4 ms (18), Authority 3.7 ms (6), Sandbox 2.4 ms (1); Unity: Gfx_WaitForGfxCommandsFromMainThread 431.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 83.2 ms, FixedBehaviourUpdate 83.2 ms, Physics_Simulate 16.5 ms, GC_Collect 13.0 ms, GC_Alloc 2.3 ms, Update_ScriptRunBehaviourUpdate 1.3 ms, BehaviourUpdate 1.3 ms
- 2.27 s: 69 ms frame, 1 physics steps; ours: Sandbox 59.7 ms (1), Plan 35.8 ms (1), ComposeDry 31.9 ms (54), ArmSolve 28.7 ms (719), Clearance 21.8 ms (1), Prepare 11.2 ms (1); Unity: Main_Thread 69.2 ms, Gfx_WaitForGfxCommandsFromMainThread 68.2 ms, PlayerLoop 66.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 66.1 ms, FixedBehaviourUpdate 66.1 ms, CPU_Total_Frame_Time 1.2 ms, CPU_Render_Thread_Frame_Time 0.4 ms, FixedUpdate_PhysicsFixedUpdate 0.4 ms
- 14.08 s: 54 ms frame, 1 physics steps; ours: Sandbox 38.7 ms (1), ComposeDry 35.8 ms (54), ArmSolve 32.2 ms (846), Plan 31.5 ms (1), Clearance 29.1 ms (1), UnitStep 2.4 ms (6); Unity: Main_Thread 45.1 ms, Gfx_WaitForGfxCommandsFromMainThread 44.1 ms, PlayerLoop 43.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 42.0 ms, FixedBehaviourUpdate 42.0 ms, CPU_Total_Frame_Time 1.2 ms, FixedUpdate_PhysicsFixedUpdate 0.6 ms, CPU_Render_Thread_Frame_Time 0.5 ms
- 5.13 s: 49 ms frame, 1 physics steps; ours: UnitStep 45.4 ms (6), ComposeDry 38.8 ms (54), ArmSolve 35.5 ms (936), Clearance 30.2 ms (1), Compose 2.4 ms (5), Prepare 2.3 ms (1); Unity: Main_Thread 49.1 ms, Gfx_WaitForGfxCommandsFromMainThread 48.1 ms, PlayerLoop 47.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 46.1 ms, FixedBehaviourUpdate 46.1 ms, CPU_Total_Frame_Time 1.1 ms, FixedUpdate_PhysicsFixedUpdate 0.5 ms, GPU_Frame_Time 0.5 ms
- 3.90 s: 49 ms frame, 1 physics steps; ours: Sandbox 42.2 ms (1), ComposeDry 38.9 ms (54), ArmSolve 38.3 ms (774), Clearance 24.0 ms (1), Prepare 3.2 ms (2), UnitStep 3.0 ms (6); Unity: Main_Thread 48.7 ms, Gfx_WaitForGfxCommandsFromMainThread 47.8 ms, PlayerLoop 46.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 45.9 ms, FixedBehaviourUpdate 45.9 ms, CPU_Total_Frame_Time 1.2 ms, FixedUpdate_PhysicsFixedUpdate 0.5 ms, GPU_Frame_Time 0.5 ms
- 4.48 s: 48 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 40.1 ms (6), ComposeDry 31.4 ms (54), ArmSolve 28.5 ms (784), Clearance 19.5 ms (1), Compose 3.4 ms (5), Prepare 2.2 ms (1); Unity: Main_Thread 44.0 ms, Gfx_WaitForGfxCommandsFromMainThread 43.0 ms, PlayerLoop 41.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 40.8 ms, FixedBehaviourUpdate 40.8 ms, GC_Collect 3.0 ms, CPU_Total_Frame_Time 1.1 ms, FixedUpdate_PhysicsFixedUpdate 0.6 ms
- 8.75 s: 48 ms frame, 1 physics steps; ours: Sandbox 40.9 ms (1), ComposeDry 34.5 ms (57), ArmSolve 32.0 ms (853), Plan 31.9 ms (1), Clearance 27.2 ms (1), Prepare 4.2 ms (2); Unity: Main_Thread 47.4 ms, Gfx_WaitForGfxCommandsFromMainThread 46.3 ms, PlayerLoop 45.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 44.3 ms, FixedBehaviourUpdate 44.3 ms, CPU_Total_Frame_Time 1.1 ms, FixedUpdate_PhysicsFixedUpdate 0.6 ms, GPU_Frame_Time 0.5 ms
- 5.05 s: 46 ms frame, 1 physics steps; ours: Sandbox 38.8 ms (1), ArmSolve 26.0 ms (711), ComposeDry 25.9 ms (54), Plan 22.3 ms (1), Clearance 19.8 ms (1), UnitStep 2.8 ms (6); Unity: Main_Thread 45.5 ms, Gfx_WaitForGfxCommandsFromMainThread 44.3 ms, PlayerLoop 43.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 42.3 ms, FixedBehaviourUpdate 42.3 ms, CPU_Total_Frame_Time 1.1 ms, FixedUpdate_PhysicsFixedUpdate 0.5 ms, GPU_Frame_Time 0.5 ms
- 2.57 s: 45 ms frame, 1 physics steps; ours: Sandbox 37.9 ms (1), ArmSolve 30.5 ms (847), ComposeDry 28.0 ms (57), Clearance 19.2 ms (1), Prepare 6.7 ms (2), UnitStep 3.2 ms (6); Unity: Main_Thread 45.0 ms, Gfx_WaitForGfxCommandsFromMainThread 44.1 ms, PlayerLoop 42.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 41.9 ms, FixedBehaviourUpdate 41.9 ms, CPU_Total_Frame_Time 1.4 ms, FixedUpdate_PhysicsFixedUpdate 0.5 ms, GPU_Frame_Time 0.5 ms
- 9.81 s: 44 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Sandbox 35.0 ms (1), ComposeDry 28.7 ms (54), ArmSolve 28.1 ms (724), Plan 27.1 ms (1), Clearance 24.6 ms (1), Prepare 2.2 ms (1); Unity: Main_Thread 40.4 ms, Gfx_WaitForGfxCommandsFromMainThread 39.4 ms, PlayerLoop 38.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 37.2 ms, FixedBehaviourUpdate 37.2 ms, GC_Collect 3.0 ms, CPU_Total_Frame_Time 1.2 ms, FixedUpdate_PhysicsFixedUpdate 0.9 ms
- 2.74 s: 44 ms frame, 1 physics steps; ours: Sandbox 36.5 ms (1), ComposeDry 31.1 ms (54), ArmSolve 30.7 ms (806), Plan 26.8 ms (1), Clearance 24.3 ms (1), UnitStep 3.6 ms (6); Unity: Main_Thread 43.5 ms, Gfx_WaitForGfxCommandsFromMainThread 42.6 ms, PlayerLoop 41.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 40.8 ms, FixedBehaviourUpdate 40.8 ms, CPU_Total_Frame_Time 1.1 ms, CPU_Render_Thread_Frame_Time 0.3 ms, FixedUpdate_PhysicsFixedUpdate 0.3 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

