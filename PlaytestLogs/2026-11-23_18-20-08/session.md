# Playtest 2026-11-23_18-20-08

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 53.9 s of play (3237 physics steps), 262 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-23_18-20-08`.

## Target

Present 1.2–55.2 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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


## Swordsman

Present 1.2–55.2 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

2968 frames in 54.5 s (54 fps on average); 326 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 15.4 ms, 95% 33.6, 99% 40.4, worst 319.9. 28 hitches over 40 ms (2 over 100 ms). 96 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- Clothes: 379.5
- UnitStep: 42.7
- CaptureSave: 25.6
- RecorderSample: 15.7
- RecorderFlush: 7.8
- Sandbox: 5.7
- CaptureRender: 0.1
- Compose: 0.0
- ComposeDry: 0.0
- ArmSolve: 0.0
- Plan: 0.0
- Prepare: 0.0
- TryOut: 0.0
- Clearance: 0.0
- Sweep: 0.0
- Authority: 0.0

The worst frames:

- 0.10 s: 320 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 229.7 ms (6), RecorderSample 24.8 ms (3), Sandbox 12.3 ms (3); Unity: Main_Thread 315.8 ms, PlayerLoop 307.7 ms, Gfx_WaitForGfxCommandsFromMainThread 306.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 276.8 ms, FixedBehaviourUpdate 276.8 ms, GC_Collect 15.6 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 12.3 ms, LateBehaviourUpdate 12.3 ms
- 0.05 s: 253 ms frame, 0 physics steps, garbage collected (gen0 5, gen1 5, gen2 5); ours: CaptureRender 1.2 ms (1); Unity: PlayerLoop 48.4 ms, Gfx_WaitForGfxCommandsFromMainThread 38.9 ms, Physics_Simulate 36.6 ms, GC_Collect 9.6 ms, Update_ScriptRunBehaviourUpdate 2.3 ms, BehaviourUpdate 2.3 ms, CPU_Render_Thread_Frame_Time 1.6 ms, CPU_Total_Frame_Time 1.6 ms
- 22.33 s: 68 ms frame, 3 physics steps; ours: Clothes 7.2 ms (3), UnitStep 1.7 ms (6), RecorderSample 0.5 ms (3); Unity: Main_Thread 90.1 ms, Gfx_WaitForGfxCommandsFromMainThread 77.9 ms, CPU_Total_Frame_Time 24.4 ms, CPU_Main_Thread_Frame_Time 23.1 ms, PlayerLoop 15.1 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 7.6 ms, LateBehaviourUpdate 7.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 6.5 ms
- 33.59 s: 64 ms frame, 2 physics steps; ours: Clothes 7.8 ms (3), UnitStep 1.4 ms (4); Unity: Main_Thread 35.8 ms, Gfx_WaitForGfxCommandsFromMainThread 34.0 ms, PlayerLoop 14.5 ms, CPU_Total_Frame_Time 13.7 ms, CPU_Main_Thread_Frame_Time 10.5 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 7.6 ms, LateBehaviourUpdate 7.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 6.3 ms
- 22.28 s: 53 ms frame, 2 physics steps; ours: Clothes 9.2 ms (3), CaptureSave 1.8 ms (1), UnitStep 1.2 ms (4); Unity: Main_Thread 30.1 ms, CPU_Total_Frame_Time 29.0 ms, PlayerLoop 28.3 ms, CPU_Main_Thread_Frame_Time 27.2 ms, Gfx_WaitForGfxCommandsFromMainThread 24.9 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 8.7 ms, LateBehaviourUpdate 8.7 ms, CPU_Render_Thread_Frame_Time 5.3 ms
- 18.53 s: 50 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 7.6 ms (3), UnitStep 1.3 ms (4); Unity: Main_Thread 43.1 ms, Gfx_WaitForGfxCommandsFromMainThread 36.5 ms, PlayerLoop 35.2 ms, CPU_Total_Frame_Time 29.5 ms, CPU_Main_Thread_Frame_Time 25.1 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 7.8 ms, LateBehaviourUpdate 7.8 ms, CPU_Render_Thread_Frame_Time 5.5 ms
- 23.06 s: 50 ms frame, 1 physics steps; ours: Clothes 7.2 ms (3), UnitStep 0.8 ms (2); Unity: CPU_Total_Frame_Time 26.1 ms, CPU_Main_Thread_Frame_Time 22.9 ms, Main_Thread 14.0 ms, Gfx_WaitForGfxCommandsFromMainThread 12.5 ms, PlayerLoop 12.0 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 8.9 ms, LateBehaviourUpdate 8.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.8 ms
- 3.30 s: 49 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 8.1 ms (3), CaptureSave 1.6 ms (1), UnitStep 0.7 ms (2), RecorderSample 0.6 ms (1), RecorderFlush 0.5 ms (1); Unity: Main_Thread 49.8 ms, PlayerLoop 46.1 ms, Gfx_WaitForGfxCommandsFromMainThread 44.1 ms, CPU_Total_Frame_Time 28.4 ms, CPU_Main_Thread_Frame_Time 26.6 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 10.0 ms, LateBehaviourUpdate 10.0 ms, GC_Collect 5.5 ms
- 20.19 s: 48 ms frame, 2 physics steps; ours: Clothes 7.3 ms (3), UnitStep 1.2 ms (4), RecorderSample 0.6 ms (2); Unity: Main_Thread 31.0 ms, PlayerLoop 28.0 ms, Gfx_WaitForGfxCommandsFromMainThread 24.9 ms, CPU_Total_Frame_Time 23.7 ms, CPU_Main_Thread_Frame_Time 22.7 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 7.5 ms, LateBehaviourUpdate 7.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 5.5 ms
- 5.63 s: 48 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 7.7 ms (3), UnitStep 1.6 ms (6), RecorderSample 1.0 ms (3), RecorderFlush 0.6 ms (3); Unity: Main_Thread 47.0 ms, PlayerLoop 45.0 ms, Gfx_WaitForGfxCommandsFromMainThread 41.2 ms, CPU_Total_Frame_Time 33.6 ms, CPU_Main_Thread_Frame_Time 30.9 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 8.2 ms, LateBehaviourUpdate 8.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 7.2 ms
- 18.58 s: 47 ms frame, 3 physics steps; ours: Clothes 7.7 ms (3), RecorderSample 5.7 ms (3), UnitStep 2.7 ms (6), CaptureSave 2.2 ms (1); Unity: Main_Thread 59.0 ms, Gfx_WaitForGfxCommandsFromMainThread 52.1 ms, PlayerLoop 47.1 ms, CPU_Total_Frame_Time 30.2 ms, CPU_Main_Thread_Frame_Time 28.1 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 14.3 ms, FixedBehaviourUpdate 14.3 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 10.1 ms
- 0.15 s: 45 ms frame, 3 physics steps; ours: Clothes 9.4 ms (2), RecorderSample 1.6 ms (3), UnitStep 1.5 ms (6), RecorderFlush 1.2 ms (3); Unity: Main_Thread 49.7 ms, Gfx_WaitForGfxCommandsFromMainThread 43.4 ms, PlayerLoop 43.3 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 16.4 ms, LateBehaviourUpdate 16.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 8.1 ms, FixedBehaviourUpdate 8.1 ms, CPU_Total_Frame_Time 2.9 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

