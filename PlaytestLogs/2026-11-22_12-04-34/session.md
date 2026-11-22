# Playtest 2026-11-22_12-04-34

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 179.8 s of play (10788 physics steps), 1313 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-22_12-04-34`.

## Target

Present 1.2–180.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 6, 55 damage. Health at the end 100.


## Swordsman

Present 1.2–180.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 6 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 55 damage. Hits taken: 0, 0 damage. Health at the end 0.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

12109 frames in 180.4 s (67 fps on average); 3217 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 18.6 ms, 95% 29.8, 99% 37.1, worst 301.5. 42 hitches over 40 ms (2 over 100 ms). 754 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- Clothes: 190.0
- UnitStep: 39.9
- RecorderSample: 16.3
- CaptureSave: 12.8
- RecorderFlush: 8.7
- Sandbox: 5.8
- CaptureRender: 0.1
- ArmSolve: 0.1
- Compose: 0.0
- ComposeDry: 0.0
- Plan: 0.0
- Prepare: 0.0
- TryOut: 0.0
- Clearance: 0.0
- Sweep: 0.0
- Authority: 0.0

The worst frames:

- 0.10 s: 301 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 214.3 ms (6), RecorderSample 14.3 ms (3), Sandbox 12.2 ms (3); Unity: Main_Thread 301.4 ms, Gfx_WaitForGfxCommandsFromMainThread 292.6 ms, PlayerLoop 290.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 251.2 ms, FixedBehaviourUpdate 251.2 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 21.4 ms, LateBehaviourUpdate 21.4 ms, GC_Collect 16.7 ms
- 0.05 s: 230 ms frame, 0 physics steps, garbage collected (gen0 4, gen1 4, gen2 4); ours: CaptureRender 1.2 ms (1); Unity: PlayerLoop 45.0 ms, Gfx_WaitForGfxCommandsFromMainThread 38.0 ms, Physics_Simulate 32.0 ms, GC_Collect 11.4 ms, Update_ScriptRunBehaviourUpdate 2.3 ms, BehaviourUpdate 2.3 ms, CPU_Total_Frame_Time 1.3 ms, CPU_Main_Thread_Frame_Time 0.9 ms
- 104.73 s: 85 ms frame, 1 physics steps; ours: Clothes 3.6 ms (2), UnitStep 0.6 ms (2); Unity: CPU_Total_Frame_Time 20.3 ms, CPU_Main_Thread_Frame_Time 18.6 ms, Main_Thread 18.3 ms, PlayerLoop 16.7 ms, Gfx_WaitForGfxCommandsFromMainThread 13.3 ms, CPU_Render_Thread_Frame_Time 4.6 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 3.9 ms, LateBehaviourUpdate 3.9 ms
- 140.89 s: 81 ms frame, 0 physics steps; ours: Clothes 3.8 ms (2), CaptureSave 2.0 ms (1); Unity: CPU_Total_Frame_Time 20.1 ms, CPU_Main_Thread_Frame_Time 18.3 ms, Main_Thread 7.0 ms, PlayerLoop 6.7 ms, CPU_Render_Thread_Frame_Time 4.6 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 4.2 ms, LateBehaviourUpdate 4.2 ms, GPU_Frame_Time 1.8 ms
- 104.78 s: 74 ms frame, 3 physics steps; ours: Clothes 3.9 ms (2), UnitStep 1.7 ms (6), RecorderSample 0.6 ms (3); Unity: Main_Thread 137.8 ms, Gfx_WaitForGfxCommandsFromMainThread 124.3 ms, Loading_ReadObject 24.0 ms, CPU_Total_Frame_Time 17.6 ms, CPU_Main_Thread_Frame_Time 16.7 ms, PlayerLoop 11.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 6.8 ms, FixedBehaviourUpdate 6.8 ms
- 97.19 s: 66 ms frame, 2 physics steps; ours: Clothes 4.1 ms (2), UnitStep 1.2 ms (4), RecorderSample 0.7 ms (2); Unity: Main_Thread 23.4 ms, PlayerLoop 20.9 ms, CPU_Total_Frame_Time 18.6 ms, Gfx_WaitForGfxCommandsFromMainThread 17.8 ms, CPU_Main_Thread_Frame_Time 17.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 5.6 ms, FixedBehaviourUpdate 5.6 ms, CPU_Render_Thread_Frame_Time 4.6 ms
- 1.34 s: 59 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 4.0 ms (2), UnitStep 0.6 ms (2); Unity: Main_Thread 35.3 ms, PlayerLoop 31.8 ms, Gfx_WaitForGfxCommandsFromMainThread 29.3 ms, CPU_Total_Frame_Time 19.5 ms, CPU_Main_Thread_Frame_Time 18.0 ms, GC_Collect 6.4 ms, CPU_Render_Thread_Frame_Time 4.8 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 3.8 ms
- 176.26 s: 55 ms frame, 1 physics steps; ours: Clothes 3.8 ms (2), UnitStep 0.6 ms (2); Unity: Main_Thread 11.1 ms, Gfx_WaitForGfxCommandsFromMainThread 9.8 ms, PlayerLoop 7.4 ms, CPU_Total_Frame_Time 7.1 ms, CPU_Main_Thread_Frame_Time 4.4 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 4.1 ms, LateBehaviourUpdate 4.1 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.9 ms
- 35.27 s: 52 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 4.3 ms (2), CaptureSave 1.7 ms (1), UnitStep 0.8 ms (2); Unity: Main_Thread 47.2 ms, PlayerLoop 46.0 ms, Gfx_WaitForGfxCommandsFromMainThread 41.9 ms, CPU_Total_Frame_Time 27.5 ms, CPU_Main_Thread_Frame_Time 26.4 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 25.7 ms, LateBehaviourUpdate 25.7 ms, CPU_Render_Thread_Frame_Time 4.7 ms
- 169.23 s: 51 ms frame, 1 physics steps; ours: Clothes 3.9 ms (2), UnitStep 0.9 ms (2); Unity: Main_Thread 14.8 ms, Gfx_WaitForGfxCommandsFromMainThread 13.6 ms, CPU_Total_Frame_Time 10.6 ms, PlayerLoop 7.5 ms, CPU_Main_Thread_Frame_Time 7.2 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 3.9 ms, LateBehaviourUpdate 3.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.0 ms
- 129.72 s: 51 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 4.4 ms (2), CaptureSave 2.1 ms (1), UnitStep 2.0 ms (6), RecorderSample 0.8 ms (3); Unity: Main_Thread 46.0 ms, CPU_Total_Frame_Time 44.1 ms, PlayerLoop 43.4 ms, CPU_Main_Thread_Frame_Time 41.4 ms, Gfx_WaitForGfxCommandsFromMainThread 38.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 8.8 ms, FixedBehaviourUpdate 8.8 ms, CPU_Render_Thread_Frame_Time 5.8 ms
- 71.82 s: 51 ms frame, 1 physics steps; ours: Clothes 3.5 ms (2), UnitStep 0.6 ms (2); Unity: Main_Thread 48.1 ms, PlayerLoop 46.2 ms, Gfx_WaitForGfxCommandsFromMainThread 42.6 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 22.1 ms, LateBehaviourUpdate 22.1 ms, CPU_Total_Frame_Time 17.7 ms, CPU_Main_Thread_Frame_Time 16.7 ms, CPU_Render_Thread_Frame_Time 4.6 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

