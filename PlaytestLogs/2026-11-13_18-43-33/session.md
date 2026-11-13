# Playtest 2026-11-13_18-43-33

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 38.2 s of play (2290 physics steps), 3527 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-13_18-43-33`.

## Target open

Present 4.1–42.3 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.0 | 0 / 0 |
| Chest | Pinned | 0.0 / 0.0 | 0 / 0 |
| Head | Pinned | 0.0 / 0.0 | 0 / 0 |
| UpperArmL | Pinned | 0.0 / 0.0 | 0 / 0 |
| ForearmL | Pinned | 0.0 / 0.0 | 0 / 0 |
| HandL | Pinned | 0.0 / 0.0 | 0 / 0 |
| UpperArmR | Pinned | 0.0 / 0.0 | 0 / 0 |
| ForearmR | Pinned | 0.0 / 0.0 | 0 / 0 |
| HandR | Pinned | 0.0 / 0.0 | 0 / 0 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 11, 185 damage. Health at the end 100.


## Attacker 1

Present 4.1–42.3 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.0 | 0 / 0 |
| Chest | ? | 0.0 / 0.0 | 0 / 0 |
| Head | ? | 0.0 / 0.0 | 0 / 0 |
| UpperArmL | ? | 0.0 / 0.0 | 0 / 0 |
| ForearmL | ? | 0.0 / 0.0 | 0 / 0 |
| HandL | ? | 0.0 / 0.0 | 0 / 0 |
| UpperArmR | ? | 0.0 / 0.0 | 0 / 0 |
| ForearmR | ? | 0.0 / 0.0 | 0 / 0 |
| HandR | ? | 0.0 / 0.0 | 0 / 0 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 11 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 185 damage. Hits taken: 0, 0 damage. Health at the end 0.


## Target shield

Present 4.1–42.3 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.0 | 0 / 0 |
| Chest | ? | 0.0 / 0.0 | 0 / 0 |
| Head | ? | 0.0 / 0.0 | 0 / 0 |
| UpperArmL | ? | 0.0 / 0.0 | 0 / 0 |
| ForearmL | ? | 0.0 / 0.0 | 0 / 0 |
| HandL | ? | 0.0 / 0.0 | 0 / 0 |
| UpperArmR | ? | 0.0 / 0.0 | 0 / 0 |
| ForearmR | ? | 0.0 / 0.0 | 0 / 0 |
| HandR | ? | 0.0 / 0.0 | 0 / 0 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 10, 20 damage. Health at the end 0.


## Attacker 2

Present 4.1–42.3 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 0.0 | 0 / 0 |
| Chest | ? | 0.0 / 0.0 | 0 / 0 |
| Head | ? | 0.0 / 0.0 | 0 / 0 |
| UpperArmL | ? | 0.0 / 0.0 | 0 / 0 |
| ForearmL | ? | 0.0 / 0.0 | 0 / 0 |
| HandL | ? | 0.0 / 0.0 | 0 / 0 |
| UpperArmR | ? | 0.0 / 0.0 | 0 / 0 |
| ForearmR | ? | 0.0 / 0.0 | 0 / 0 |
| HandR | ? | 0.0 / 0.0 | 0 / 0 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 10 (0 on the part aimed at, 9 blocked, 0 parried, 0 on someone in the way, 0 on allies), 20 damage. Hits taken: 0, 0 damage. Health at the end 0.


## Target sword

Present 4.1–42.3 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.0 | 0 / 0 |
| Chest | Pinned | 0.0 / 0.0 | 0 / 0 |
| Head | Pinned | 0.0 / 0.0 | 0 / 0 |
| UpperArmL | Pinned | 0.0 / 0.0 | 0 / 0 |
| ForearmL | Pinned | 0.0 / 0.0 | 0 / 0 |
| HandL | Pinned | 0.0 / 0.0 | 0 / 0 |
| UpperArmR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ForearmR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| HandR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 14, 19 damage. Health at the end 100.


## Attacker 3

Present 4.1–42.3 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.0 | 0 / 0 |
| Chest | Pinned | 0.0 / 0.0 | 0 / 0 |
| Head | Pinned | 0.0 / 0.0 | 0 / 0 |
| UpperArmL | Pinned | 0.0 / 0.0 | 0 / 0 |
| ForearmL | Pinned | 0.0 / 0.0 | 0 / 0 |
| HandL | Pinned | 0.0 / 0.0 | 0 / 0 |
| UpperArmR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ForearmR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| HandR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 14 (0 on the part aimed at, 0 blocked, 12 parried, 0 on someone in the way, 0 on allies), 19 damage. Hits taken: 0, 0 damage. Health at the end 100.

## Defence (milestone 11)

- Medium: pressed 63 s, 91% of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 10 bursts at it answered with a counter, 0% (gate 30–50%), 0 of them ripostes between links.

## Performance

5708 frames in 40.4 s (141 fps on average); 4039 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 11.0 ms, 95% 54.2, 99% 69.5, worst 589.7. 231 hitches over 40 ms (4 over 100 ms). 36 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 229.6
- ArmSolve: 103.3
- RecorderSample: 71.4
- RecorderFlush: 43.5
- CaptureSave: 13.3
- Sandbox: 3.9
- CaptureRender: 0.3
- Compose: 0.0
- ComposeDry: 0.0
- Plan: 0.0
- Prepare: 0.0
- TryOut: 0.0
- Clearance: 0.0
- Sweep: 0.0
- Authority: 0.0

The worst frames:

- 0.05 s: 590 ms frame, 0 physics steps, garbage collected (gen0 5, gen1 5, gen2 5); ours: nothing over 0.5 ms; Unity: Physics_Simulate 85.7 ms, GC_Collect 35.7 ms, PlayerLoop 12.1 ms, Gfx_WaitForGfxCommandsFromMainThread 11.4 ms, CPU_Total_Frame_Time 5.6 ms, CPU_Main_Thread_Frame_Time 3.0 ms, CPU_Render_Thread_Frame_Time 1.9 ms, GPU_Frame_Time 0.7 ms
- 17.38 s: 356 ms frame, 3 physics steps; ours: RecorderSample 316.5 ms (3), RecorderFlush 315.1 ms (3), UnitStep 15.4 ms (18), ArmSolve 6.0 ms (18); Unity: Main_Thread 355.9 ms, PlayerLoop 352.8 ms, Gfx_WaitForGfxCommandsFromMainThread 352.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 348.2 ms, FixedBehaviourUpdate 348.2 ms, CPU_Total_Frame_Time 38.1 ms, CPU_Main_Thread_Frame_Time 34.5 ms, Physics_Simulate 9.2 ms
- 0.10 s: 249 ms frame, 3 physics steps; ours: UnitStep 109.6 ms (18), RecorderSample 59.0 ms (3), ArmSolve 24.1 ms (32), Sandbox 8.6 ms (3), CaptureRender 2.9 ms (1), RecorderFlush 0.7 ms (3); Unity: Main_Thread 253.3 ms, Gfx_WaitForGfxCommandsFromMainThread 246.9 ms, PlayerLoop 228.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 206.1 ms, FixedBehaviourUpdate 206.1 ms, GC_Collect 8.5 ms, Physics_Simulate 7.8 ms, CPU_Total_Frame_Time 7.2 ms
- 32.49 s: 120 ms frame, 3 physics steps; ours: UnitStep 8.9 ms (18), ArmSolve 3.4 ms (12), CaptureSave 1.9 ms (1), RecorderSample 1.1 ms (3); Unity: Main_Thread 120.3 ms, Gfx_WaitForGfxCommandsFromMainThread 118.4 ms, PlayerLoop 118.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 114.0 ms, FixedBehaviourUpdate 114.0 ms, Physics_Simulate 86.5 ms, CPU_Total_Frame_Time 3.1 ms, CPU_Main_Thread_Frame_Time 2.0 ms
- 2.08 s: 97 ms frame, 3 physics steps; ours: Sandbox 34.4 ms (3), UnitStep 27.8 ms (18), ArmSolve 7.1 ms (20), RecorderSample 3.3 ms (3), CaptureSave 2.7 ms (1), RecorderFlush 1.6 ms (3); Unity: Main_Thread 97.9 ms, Gfx_WaitForGfxCommandsFromMainThread 94.1 ms, PlayerLoop 91.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 82.5 ms, FixedBehaviourUpdate 82.5 ms, CPU_Total_Frame_Time 54.3 ms, CPU_Main_Thread_Frame_Time 46.9 ms, Physics_Simulate 9.4 ms
- 0.66 s: 95 ms frame, 3 physics steps; ours: UnitStep 17.0 ms (18), ArmSolve 8.6 ms (24), RecorderSample 3.3 ms (3), RecorderFlush 1.7 ms (3); Unity: CPU_Total_Frame_Time 48.9 ms, Main_Thread 48.1 ms, Gfx_WaitForGfxCommandsFromMainThread 44.9 ms, CPU_Main_Thread_Frame_Time 44.4 ms, PlayerLoop 43.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 38.5 ms, FixedBehaviourUpdate 38.5 ms, Physics_Simulate 10.1 ms
- 3.88 s: 90 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 25.1 ms (18), RecorderSample 17.6 ms (3), ArmSolve 13.9 ms (18), RecorderFlush 11.6 ms (3); Unity: Main_Thread 80.4 ms, Gfx_WaitForGfxCommandsFromMainThread 76.0 ms, PlayerLoop 74.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 62.1 ms, FixedBehaviourUpdate 62.1 ms, CPU_Total_Frame_Time 56.0 ms, CPU_Main_Thread_Frame_Time 51.1 ms, Physics_Simulate 10.6 ms
- 2.91 s: 83 ms frame, 3 physics steps; ours: UnitStep 14.5 ms (18), ArmSolve 4.9 ms (6), RecorderSample 3.6 ms (3), RecorderFlush 2.0 ms (3); Unity: Main_Thread 80.8 ms, Gfx_WaitForGfxCommandsFromMainThread 76.6 ms, PlayerLoop 43.8 ms, CPU_Total_Frame_Time 38.1 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 38.1 ms, FixedBehaviourUpdate 38.1 ms, CPU_Main_Thread_Frame_Time 34.6 ms, Physics_Simulate 11.6 ms
- 2.86 s: 83 ms frame, 3 physics steps; ours: UnitStep 17.2 ms (18), ArmSolve 5.0 ms (7), RecorderSample 4.6 ms (3), RecorderFlush 2.9 ms (3); Unity: Main_Thread 66.3 ms, Gfx_WaitForGfxCommandsFromMainThread 61.8 ms, PlayerLoop 46.9 ms, CPU_Total_Frame_Time 44.2 ms, CPU_Main_Thread_Frame_Time 40.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 40.2 ms, FixedBehaviourUpdate 40.2 ms, Physics_Simulate 10.9 ms
- 14.82 s: 80 ms frame, 3 physics steps; ours: UnitStep 30.2 ms (18), CaptureSave 16.3 ms (1), RecorderSample 4.7 ms (3), RecorderFlush 3.0 ms (3), ArmSolve 2.0 ms (6); Unity: Main_Thread 80.4 ms, Gfx_WaitForGfxCommandsFromMainThread 77.2 ms, PlayerLoop 77.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 54.7 ms, FixedBehaviourUpdate 54.7 ms, CPU_Total_Frame_Time 32.7 ms, CPU_Main_Thread_Frame_Time 29.6 ms, Physics_Simulate 11.5 ms
- 5.95 s: 77 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 25.3 ms (18), ArmSolve 13.0 ms (22), RecorderSample 6.7 ms (3), Sandbox 0.6 ms (3); Unity: Main_Thread 68.7 ms, Gfx_WaitForGfxCommandsFromMainThread 64.9 ms, PlayerLoop 63.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 51.4 ms, FixedBehaviourUpdate 51.4 ms, CPU_Total_Frame_Time 42.3 ms, CPU_Main_Thread_Frame_Time 39.1 ms, Physics_Simulate 10.5 ms
- 8.03 s: 74 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 26.1 ms (18), ArmSolve 14.0 ms (24), RecorderSample 6.7 ms (3), RecorderFlush 0.9 ms (3); Unity: Main_Thread 76.6 ms, PlayerLoop 73.0 ms, Gfx_WaitForGfxCommandsFromMainThread 72.8 ms, CPU_Total_Frame_Time 56.0 ms, CPU_Main_Thread_Frame_Time 52.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 52.4 ms, FixedBehaviourUpdate 52.4 ms, Physics_Simulate 11.5 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

