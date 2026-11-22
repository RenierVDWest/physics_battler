# Playtest 2026-11-22_18-56-21

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 118.6 s of play (7119 physics steps), 2489 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-22_18-56-21`.

## Target

Present 1.1–42.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 2, 16 damage. Health at the end 0.


## Swordsman

Present 1.1–42.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 2 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 16 damage. Hits taken: 0, 0 damage. Health at the end 0.


## Target #2

Present 42.9–119.7 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 8 (0 on the part aimed at, 6 blocked, 0 parried, 0 on someone in the way, 0 on allies), 7 damage. Hits taken: 22, 134 damage. Health at the end 100.


## Swordsman #2

Present 42.9–119.7 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 22 (0 on the part aimed at, 12 blocked, 0 parried, 0 on someone in the way, 0 on allies), 134 damage. Hits taken: 8, 7 damage. Health at the end 100.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

3591 frames in 424.9 s (8 fps on average); 1 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 38.4 ms, 95% 54.2, 99% 70.0, worst 179384.0. 1544 hitches over 40 ms (25 over 100 ms). 287 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- Clothes: 169.9
- UnitStep: 12.6
- RecorderSample: 6.2
- CaptureSave: 4.3
- RecorderFlush: 4.0
- Sandbox: 2.1
- ArmSolve: 0.1
- CaptureRender: 0.0
- Compose: 0.0
- ComposeDry: 0.0
- Plan: 0.0
- Prepare: 0.0
- TryOut: 0.0
- Clearance: 0.0
- Sweep: 0.0
- Authority: 0.0

The worst frames:

- 113.89 s: 179384 ms frame, 1 physics steps, garbage collected (gen0 3, gen1 3, gen2 3); ours: Clothes 18.4 ms (2), UnitStep 1.0 ms (2); Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.4 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms
- 113.38 s: 20338 ms frame, 1 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: Clothes 19.3 ms (2), UnitStep 0.7 ms (2); Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.4 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms
- 100.50 s: 17995 ms frame, 2 physics steps, garbage collected (gen0 3, gen1 3, gen2 3); ours: Clothes 21.0 ms (2), UnitStep 2.1 ms (4), CaptureSave 1.8 ms (1), RecorderSample 1.4 ms (2), RecorderFlush 1.1 ms (2); Unity: Main_Thread 0.2 ms, Gfx_WaitForGfxCommandsFromMainThread 0.1 ms, CPU_Total_Frame_Time 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 75.50 s: 16929 ms frame, 3 physics steps, garbage collected (gen0 4, gen1 4, gen2 4); ours: Clothes 20.4 ms (2), UnitStep 2.0 ms (6), RecorderSample 1.6 ms (3), RecorderFlush 1.2 ms (3); Unity: Main_Thread 11.8 ms, PlayerLoop 10.2 ms, Gfx_WaitForGfxCommandsFromMainThread 7.0 ms, GC_Alloc 0.4 ms, CPU_Total_Frame_Time 0.0 ms, Camera_Render 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 68.63 s: 12823 ms frame, 3 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: Clothes 22.0 ms (2), UnitStep 2.0 ms (6); Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.3 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms
- 106.74 s: 12110 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 18.6 ms (2), UnitStep 0.8 ms (2), RecorderSample 0.7 ms (1), RecorderFlush 0.6 ms (1); Unity: Main_Thread 0.3 ms, Gfx_WaitForGfxCommandsFromMainThread 0.2 ms, CPU_Total_Frame_Time 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 108.92 s: 10555 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 18.3 ms (2), UnitStep 0.8 ms (2), RecorderSample 0.8 ms (1), RecorderFlush 0.6 ms (1); Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.4 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 106.63 s: 4046 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 19.4 ms (2), UnitStep 1.0 ms (2); Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.4 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms
- 100.67 s: 2326 ms frame, 2 physics steps; ours: Clothes 20.5 ms (2), UnitStep 2.1 ms (4), RecorderSample 1.4 ms (2), RecorderFlush 1.2 ms (2), ArmSolve 0.5 ms (8); Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.4 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 71.78 s: 1692 ms frame, 2 physics steps, garbage collected (gen0 3, gen1 3, gen2 3); ours: Clothes 19.9 ms (2), CaptureSave 1.7 ms (1), UnitStep 1.7 ms (4), RecorderSample 1.2 ms (2), RecorderFlush 0.6 ms (2); Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.4 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 70.17 s: 1591 ms frame, 3 physics steps, garbage collected (gen0 3, gen1 3, gen2 3); ours: Clothes 20.5 ms (2), CaptureSave 5.7 ms (1), UnitStep 2.1 ms (6), RecorderSample 1.6 ms (3), RecorderFlush 1.3 ms (3); Unity: Main_Thread 10.8 ms, PlayerLoop 9.6 ms, Gfx_WaitForGfxCommandsFromMainThread 6.4 ms, GC_Alloc 0.4 ms, CPU_Total_Frame_Time 0.1 ms, Camera_Render 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Collect 0.0 ms
- 107.64 s: 1534 ms frame, 0 physics steps; ours: Clothes 18.4 ms (2); Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.4 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

