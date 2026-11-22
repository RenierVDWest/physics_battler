# Playtest 2026-11-22_07-00-12

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 593.6 s of play (35619 physics steps), 194 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-22_07-00-12`.

## Target

Present 2.5–596.1 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 18, 144 damage. Health at the end 100.


## Swordsman

Present 2.5–596.1 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 18 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 144 damage. Hits taken: 0, 0 damage. Health at the end 0.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

34381 frames in 781.2 s (44 fps on average); 3898 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 17.5 ms, 95% 27.7, 99% 34.9, worst 137412.7. 53 hitches over 40 ms (4 over 100 ms). 3471 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 160.8
- RecorderSample: 10.6
- CaptureSave: 8.6
- RecorderFlush: 5.3
- Sandbox: 4.7
- CaptureRender: 0.1
- ArmSolve: 0.0
- Compose: 0.0
- ComposeDry: 0.0
- Plan: 0.0
- Prepare: 0.0
- TryOut: 0.0
- Clearance: 0.0
- Sweep: 0.0
- Authority: 0.0

The worst frames:

- 439.10 s: 137413 ms frame, 1 physics steps, garbage collected (gen0 5, gen1 5, gen2 5); ours: UnitStep 0.6 ms (2); Unity: Main_Thread 0.2 ms, Gfx_WaitForGfxCommandsFromMainThread 0.2 ms, CPU_Total_Frame_Time 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 41.46 s: 49801 ms frame, 1 physics steps, garbage collected (gen0 4, gen1 4, gen2 4); ours: UnitStep 0.6 ms (2); Unity: Main_Thread 1.6 ms, Gfx_WaitForGfxCommandsFromMainThread 1.2 ms, CPU_Total_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 0.05 s: 236 ms frame, 0 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: CaptureRender 1.3 ms (1); Unity: PlayerLoop 47.5 ms, Gfx_WaitForGfxCommandsFromMainThread 40.6 ms, Physics_Simulate 31.7 ms, GC_Collect 9.7 ms, Update_ScriptRunBehaviourUpdate 2.3 ms, BehaviourUpdate 2.3 ms, CPU_Total_Frame_Time 1.6 ms, CPU_Main_Thread_Frame_Time 1.0 ms
- 0.10 s: 103 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 36.2 ms (6), RecorderSample 14.4 ms (3), Sandbox 12.7 ms (3); Unity: Main_Thread 103.7 ms, Gfx_WaitForGfxCommandsFromMainThread 96.0 ms, PlayerLoop 95.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 73.1 ms, FixedBehaviourUpdate 73.1 ms, GC_Collect 5.3 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 2.7 ms, LateBehaviourUpdate 2.7 ms
- 571.39 s: 76 ms frame, 2 physics steps; ours: UnitStep 10.6 ms (4), RecorderSample 0.5 ms (2); Unity: Main_Thread 32.5 ms, PlayerLoop 28.2 ms, CPU_Total_Frame_Time 27.1 ms, Gfx_WaitForGfxCommandsFromMainThread 26.2 ms, CPU_Main_Thread_Frame_Time 25.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 15.5 ms, FixedBehaviourUpdate 15.5 ms, CPU_Render_Thread_Frame_Time 5.3 ms
- 438.43 s: 69 ms frame, 1 physics steps; ours: UnitStep 0.7 ms (2); Unity: Main_Thread 17.7 ms, CPU_Total_Frame_Time 16.2 ms, CPU_Main_Thread_Frame_Time 14.2 ms, PlayerLoop 13.6 ms, Gfx_WaitForGfxCommandsFromMainThread 12.4 ms, CPU_Render_Thread_Frame_Time 4.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.2 ms, FixedBehaviourUpdate 3.2 ms
- 455.83 s: 67 ms frame, 3 physics steps; ours: UnitStep 17.4 ms (6), RecorderSample 1.9 ms (3); Unity: Main_Thread 66.5 ms, Gfx_WaitForGfxCommandsFromMainThread 57.6 ms, PlayerLoop 56.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 25.4 ms, FixedBehaviourUpdate 25.4 ms, CPU_Total_Frame_Time 19.1 ms, CPU_Main_Thread_Frame_Time 17.4 ms, CPU_Render_Thread_Frame_Time 5.0 ms
- 252.76 s: 65 ms frame, 2 physics steps; ours: UnitStep 12.6 ms (4); Unity: CPU_Total_Frame_Time 55.0 ms, CPU_Main_Thread_Frame_Time 36.1 ms, Main_Thread 32.2 ms, PlayerLoop 28.5 ms, Gfx_WaitForGfxCommandsFromMainThread 26.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 17.7 ms, FixedBehaviourUpdate 17.7 ms, CPU_Render_Thread_Frame_Time 4.9 ms
- 593.64 s: 58 ms frame, 3 physics steps; ours: UnitStep 15.8 ms (6), RecorderSample 1.7 ms (3); Unity: Main_Thread 74.2 ms, Gfx_WaitForGfxCommandsFromMainThread 68.3 ms, PlayerLoop 44.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 23.0 ms, FixedBehaviourUpdate 23.0 ms, CPU_Total_Frame_Time 19.4 ms, CPU_Main_Thread_Frame_Time 17.6 ms, CPU_Render_Thread_Frame_Time 4.7 ms
- 593.59 s: 53 ms frame, 1 physics steps; ours: UnitStep 5.2 ms (2); Unity: CPU_Total_Frame_Time 26.7 ms, CPU_Main_Thread_Frame_Time 24.1 ms, Main_Thread 23.3 ms, PlayerLoop 21.4 ms, Gfx_WaitForGfxCommandsFromMainThread 17.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 7.3 ms, FixedBehaviourUpdate 7.3 ms, CPU_Render_Thread_Frame_Time 4.8 ms
- 250.54 s: 53 ms frame, 2 physics steps; ours: UnitStep 13.9 ms (4), CaptureSave 12.3 ms (1), RecorderSample 1.1 ms (2), RecorderFlush 0.8 ms (2); Unity: Main_Thread 52.6 ms, PlayerLoop 49.0 ms, Gfx_WaitForGfxCommandsFromMainThread 45.1 ms, CPU_Total_Frame_Time 38.8 ms, CPU_Main_Thread_Frame_Time 35.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 19.6 ms, FixedBehaviourUpdate 19.6 ms, CPU_Render_Thread_Frame_Time 5.1 ms
- 252.80 s: 49 ms frame, 3 physics steps; ours: UnitStep 19.6 ms (6), RecorderSample 0.8 ms (3); Unity: Main_Thread 81.7 ms, Gfx_WaitForGfxCommandsFromMainThread 74.7 ms, PlayerLoop 45.0 ms, CPU_Total_Frame_Time 36.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 27.7 ms, FixedBehaviourUpdate 27.7 ms, CPU_Main_Thread_Frame_Time 27.4 ms, CPU_Render_Thread_Frame_Time 5.1 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

