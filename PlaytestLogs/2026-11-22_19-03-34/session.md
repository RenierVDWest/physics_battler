# Playtest 2026-11-22_19-03-34

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 97.4 s of play (5844 physics steps), 1627 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-22_19-03-34`.

## Target

Present 1.8–99.2 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Present 1.8–99.2 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

2374 frames in 99.0 s (24 fps on average); 0 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 40.1 ms, 95% 50.7, 99% 59.3, worst 515.1. 1223 hitches over 40 ms (4 over 100 ms). 176 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- Clothes: 447.6
- UnitStep: 34.0
- CaptureSave: 15.3
- RecorderSample: 15.3
- RecorderFlush: 7.8
- Sandbox: 5.2
- CaptureRender: 0.2
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

- 37.58 s: 515 ms frame, 2 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: Clothes 17.9 ms (2), CaptureSave 1.8 ms (1), UnitStep 1.3 ms (4); Unity: Main_Thread 37.8 ms, PlayerLoop 36.6 ms, CPU_Total_Frame_Time 34.6 ms, CPU_Main_Thread_Frame_Time 33.4 ms, Gfx_WaitForGfxCommandsFromMainThread 32.8 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 18.0 ms, LateBehaviourUpdate 18.0 ms, CPU_Render_Thread_Frame_Time 5.0 ms
- 0.05 s: 242 ms frame, 0 physics steps, garbage collected (gen0 4, gen1 4, gen2 4); ours: CaptureRender 1.5 ms (1); Unity: PlayerLoop 45.1 ms, Gfx_WaitForGfxCommandsFromMainThread 37.9 ms, Physics_Simulate 32.6 ms, GC_Collect 10.4 ms, Update_ScriptRunBehaviourUpdate 2.5 ms, BehaviourUpdate 2.5 ms, CPU_Total_Frame_Time 1.5 ms, CPU_Main_Thread_Frame_Time 1.0 ms
- 0.10 s: 206 ms frame, 3 physics steps; ours: UnitStep 109.2 ms (6), RecorderSample 14.2 ms (3), Sandbox 12.0 ms (3); Unity: Main_Thread 206.7 ms, PlayerLoop 198.7 ms, Gfx_WaitForGfxCommandsFromMainThread 198.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 145.5 ms, FixedBehaviourUpdate 145.5 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 38.8 ms, LateBehaviourUpdate 38.8 ms, GC_Collect 10.9 ms
- 37.63 s: 199 ms frame, 3 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: Clothes 17.9 ms (2), UnitStep 1.7 ms (6), RecorderSample 1.4 ms (3); Unity: Main_Thread 668.6 ms, Gfx_WaitForGfxCommandsFromMainThread 650.7 ms, GC_Alloc 82.8 ms, PlayerLoop 41.1 ms, CPU_Total_Frame_Time 37.7 ms, CPU_Main_Thread_Frame_Time 36.5 ms, GC_Collect 29.1 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 20.2 ms
- 3.49 s: 71 ms frame, 3 physics steps; ours: Clothes 19.0 ms (2), CaptureSave 1.7 ms (1), UnitStep 1.6 ms (6), RecorderSample 1.1 ms (3), RecorderFlush 0.8 ms (3); Unity: Main_Thread 67.4 ms, Gfx_WaitForGfxCommandsFromMainThread 61.6 ms, PlayerLoop 61.2 ms, CPU_Total_Frame_Time 57.9 ms, CPU_Main_Thread_Frame_Time 52.7 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 19.9 ms, LateBehaviourUpdate 19.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 7.8 ms
- 3.24 s: 71 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 20.9 ms (2), UnitStep 2.1 ms (6), CaptureSave 1.8 ms (1), RecorderSample 0.9 ms (3), RecorderFlush 0.5 ms (3); Unity: Main_Thread 71.5 ms, Gfx_WaitForGfxCommandsFromMainThread 65.7 ms, PlayerLoop 65.4 ms, CPU_Total_Frame_Time 63.8 ms, CPU_Main_Thread_Frame_Time 58.3 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 20.5 ms, LateBehaviourUpdate 20.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 8.0 ms
- 3.64 s: 68 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 21.0 ms (2), UnitStep 1.5 ms (6), RecorderSample 1.4 ms (3), RecorderFlush 1.1 ms (3); Unity: Main_Thread 62.1 ms, CPU_Total_Frame_Time 59.5 ms, PlayerLoop 57.2 ms, Gfx_WaitForGfxCommandsFromMainThread 56.4 ms, CPU_Main_Thread_Frame_Time 55.0 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 19.9 ms, LateBehaviourUpdate 19.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 7.7 ms
- 1.74 s: 68 ms frame, 3 physics steps; ours: Clothes 19.0 ms (2), RecorderSample 8.5 ms (3), RecorderFlush 8.2 ms (3), CaptureSave 1.8 ms (1), UnitStep 1.6 ms (6); Unity: Main_Thread 72.7 ms, PlayerLoop 69.3 ms, Gfx_WaitForGfxCommandsFromMainThread 66.8 ms, CPU_Total_Frame_Time 63.6 ms, CPU_Main_Thread_Frame_Time 58.4 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 20.6 ms, LateBehaviourUpdate 20.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 15.0 ms
- 3.44 s: 67 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 21.8 ms (2), UnitStep 1.5 ms (6), RecorderSample 0.9 ms (3), RecorderFlush 0.6 ms (3); Unity: CPU_Total_Frame_Time 71.5 ms, CPU_Main_Thread_Frame_Time 65.4 ms, Main_Thread 59.4 ms, PlayerLoop 55.0 ms, Gfx_WaitForGfxCommandsFromMainThread 53.7 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 19.1 ms, LateBehaviourUpdate 19.1 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 7.1 ms
- 3.04 s: 66 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 21.0 ms (2), UnitStep 1.7 ms (6), RecorderSample 1.3 ms (3), RecorderFlush 1.0 ms (3); Unity: Main_Thread 63.8 ms, PlayerLoop 58.3 ms, Gfx_WaitForGfxCommandsFromMainThread 58.1 ms, CPU_Total_Frame_Time 45.2 ms, CPU_Main_Thread_Frame_Time 42.3 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 19.7 ms, LateBehaviourUpdate 19.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 7.8 ms
- 1.49 s: 64 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 19.1 ms (2), CaptureSave 1.7 ms (1), UnitStep 1.6 ms (6), RecorderSample 0.8 ms (3); Unity: Main_Thread 59.1 ms, PlayerLoop 54.1 ms, Gfx_WaitForGfxCommandsFromMainThread 53.3 ms, CPU_Total_Frame_Time 44.2 ms, CPU_Main_Thread_Frame_Time 42.1 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 19.4 ms, LateBehaviourUpdate 19.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 7.5 ms
- 1.79 s: 64 ms frame, 3 physics steps; ours: Clothes 20.5 ms (2), UnitStep 1.5 ms (6), RecorderSample 1.4 ms (3), RecorderFlush 1.1 ms (3); Unity: Main_Thread 61.0 ms, CPU_Total_Frame_Time 58.6 ms, PlayerLoop 55.9 ms, Gfx_WaitForGfxCommandsFromMainThread 55.8 ms, CPU_Main_Thread_Frame_Time 54.0 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 18.6 ms, LateBehaviourUpdate 18.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 8.0 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

