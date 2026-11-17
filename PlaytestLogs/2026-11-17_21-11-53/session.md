# Playtest 2026-11-17_21-11-53

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 1190.5 s of play (71429 physics steps), 81 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-17_21-11-53`.

## Target

Present 1.3–1191.7 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 2, 35 damage. Health at the end 0.


## Swordsman

Present 1.3–1191.7 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 2 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 35 damage. Hits taken: 0, 0 damage. Health at the end 0.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

237141 frames in 1201.5 s (197 fps on average); 165784 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 6.3 ms, 95% 12.3, 99% 14.9, worst 7633.1. 21 hitches over 40 ms (11 over 100 ms). 6695 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 28.4
- RecorderSample: 15.3
- RecorderFlush: 6.3
- Sandbox: 5.6
- CaptureSave: 1.4
- CaptureRender: 0.0
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

- 19.77 s: 7633 ms frame, 1 physics steps; ours: UnitStep 0.5 ms (2); Unity: Main_Thread 8.5 ms, PlayerLoop 7.2 ms, Gfx_WaitForGfxCommandsFromMainThread 5.9 ms, CPU_Total_Frame_Time 4.7 ms, CPU_Main_Thread_Frame_Time 3.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.9 ms, FixedBehaviourUpdate 2.9 ms, CPU_Render_Thread_Frame_Time 2.0 ms
- 19.92 s: 1332 ms frame, 3 physics steps; ours: RecorderSample 10.2 ms (3), RecorderFlush 9.8 ms (3), UnitStep 1.4 ms (6); Unity: Main_Thread 138.9 ms, Gfx_WaitForGfxCommandsFromMainThread 135.2 ms, PlayerLoop 22.4 ms, CPU_Total_Frame_Time 18.6 ms, CPU_Main_Thread_Frame_Time 17.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 16.6 ms, FixedBehaviourUpdate 16.6 ms, Physics_Simulate 2.9 ms
- 19.82 s: 1043 ms frame, 3 physics steps; ours: UnitStep 1.3 ms (6), RecorderSample 0.7 ms (3); Unity: Main_Thread 7951.6 ms, Gfx_WaitForGfxCommandsFromMainThread 7946.3 ms, PlayerLoop 7788.3 ms, CPU_Total_Frame_Time 7.5 ms, CPU_Main_Thread_Frame_Time 6.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 6.6 ms, FixedBehaviourUpdate 6.6 ms, Physics_Simulate 2.6 ms
- 0.05 s: 273 ms frame, 0 physics steps, garbage collected (gen0 3, gen1 3, gen2 3); ours: CaptureRender 4.2 ms (1); Unity: Physics_Simulate 44.3 ms, Gfx_WaitForGfxCommandsFromMainThread 30.1 ms, PlayerLoop 27.8 ms, GC_Collect 19.2 ms, Update_ScriptRunBehaviourUpdate 5.4 ms, BehaviourUpdate 5.4 ms, CPU_Total_Frame_Time 3.1 ms, CPU_Main_Thread_Frame_Time 1.9 ms
- 19.87 s: 237 ms frame, 3 physics steps; ours: UnitStep 1.5 ms (6), RecorderSample 1.1 ms (3), RecorderFlush 0.7 ms (3); Unity: Main_Thread 842.4 ms, Gfx_WaitForGfxCommandsFromMainThread 838.4 ms, PlayerLoop 17.1 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 8.5 ms, FixedBehaviourUpdate 8.5 ms, CPU_Total_Frame_Time 7.9 ms, CPU_Main_Thread_Frame_Time 6.9 ms, Physics_Simulate 3.5 ms
- 33.48 s: 217 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: CPU_Total_Frame_Time 10.8 ms, CPU_Main_Thread_Frame_Time 9.9 ms, Main_Thread 5.8 ms, PlayerLoop 4.3 ms, Gfx_WaitForGfxCommandsFromMainThread 3.0 ms, CPU_Render_Thread_Frame_Time 2.2 ms, GPU_Frame_Time 0.8 ms, GC_Alloc 0.2 ms
- 20.25 s: 168 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 0.7 ms (2); Unity: Main_Thread 170.9 ms, Gfx_WaitForGfxCommandsFromMainThread 167.7 ms, PlayerLoop 19.4 ms, CPU_Total_Frame_Time 4.7 ms, CPU_Main_Thread_Frame_Time 3.9 ms, GC_Collect 3.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.0 ms, FixedBehaviourUpdate 3.0 ms
- 19.97 s: 150 ms frame, 3 physics steps; ours: CaptureSave 11.6 ms (1), UnitStep 1.7 ms (6), RecorderSample 1.0 ms (3), RecorderFlush 0.6 ms (3); Unity: Main_Thread 1339.0 ms, Gfx_WaitForGfxCommandsFromMainThread 1335.0 ms, PlayerLoop 30.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 9.0 ms, FixedBehaviourUpdate 9.0 ms, CPU_Total_Frame_Time 8.5 ms, CPU_Main_Thread_Frame_Time 7.2 ms, CPU_Render_Thread_Frame_Time 4.8 ms
- 20.30 s: 149 ms frame, 3 physics steps; ours: UnitStep 1.4 ms (6), RecorderSample 1.0 ms (3), RecorderFlush 0.6 ms (3); Unity: Main_Thread 29.0 ms, Gfx_WaitForGfxCommandsFromMainThread 25.9 ms, PlayerLoop 25.1 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 7.8 ms, FixedBehaviourUpdate 7.8 ms, CPU_Total_Frame_Time 7.6 ms, CPU_Main_Thread_Frame_Time 6.6 ms, Physics_Simulate 3.0 ms
- 53.82 s: 135 ms frame, 1 physics steps; ours: UnitStep 0.7 ms (2); Unity: Main_Thread 134.4 ms, PlayerLoop 131.4 ms, Gfx_WaitForGfxCommandsFromMainThread 131.0 ms, CPU_Total_Frame_Time 4.6 ms, CPU_Main_Thread_Frame_Time 3.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.9 ms, FixedBehaviourUpdate 2.9 ms, CPU_Render_Thread_Frame_Time 2.1 ms
- 12.83 s: 105 ms frame, 1 physics steps; ours: UnitStep 0.9 ms (2); Unity: CPU_Total_Frame_Time 19.1 ms, CPU_Main_Thread_Frame_Time 17.2 ms, Main_Thread 13.3 ms, PlayerLoop 11.8 ms, CPU_Render_Thread_Frame_Time 4.1 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.6 ms, FixedBehaviourUpdate 3.6 ms, Physics_Simulate 1.4 ms
- 0.10 s: 95 ms frame, 3 physics steps; ours: UnitStep 41.1 ms (6), RecorderSample 14.7 ms (3), Sandbox 9.6 ms (3); Unity: Main_Thread 99.9 ms, Gfx_WaitForGfxCommandsFromMainThread 95.5 ms, PlayerLoop 86.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 76.7 ms, FixedBehaviourUpdate 76.7 ms, CPU_Total_Frame_Time 3.5 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 2.9 ms, LateBehaviourUpdate 2.9 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

