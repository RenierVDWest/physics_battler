# Playtest 2026-11-23_06-57-33

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 149.8 s of play (8986 physics steps), 890 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-23_06-57-33`.

## Target

Present 3.4–153.1 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 13, 183 damage. Health at the end 0.


## Swordsman

Present 3.4–153.1 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 13 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 183 damage. Hits taken: 0, 0 damage. Health at the end 0.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

5262 frames in 150.5 s (35 fps on average); 0 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 26.9 ms, 95% 38.5, 99% 44.4, worst 297.1. 193 hitches over 40 ms (2 over 100 ms). 653 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- Clothes: 280.5
- UnitStep: 38.2
- CaptureSave: 27.9
- RecorderSample: 16.3
- RecorderFlush: 9.5
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

- 0.10 s: 297 ms frame, 3 physics steps; ours: UnitStep 198.2 ms (6), RecorderSample 15.1 ms (3), Sandbox 11.8 ms (3); Unity: Main_Thread 294.6 ms, PlayerLoop 284.1 ms, Gfx_WaitForGfxCommandsFromMainThread 282.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 235.2 ms, FixedBehaviourUpdate 235.2 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 27.2 ms, LateBehaviourUpdate 27.2 ms, GC_Collect 11.5 ms
- 0.05 s: 283 ms frame, 0 physics steps, garbage collected (gen0 7, gen1 7, gen2 7); ours: CaptureRender 0.8 ms (1); Unity: PlayerLoop 51.6 ms, Gfx_WaitForGfxCommandsFromMainThread 38.5 ms, Physics_Simulate 35.2 ms, GC_Collect 11.9 ms, Update_ScriptRunBehaviourUpdate 1.7 ms, BehaviourUpdate 1.7 ms, CPU_Total_Frame_Time 1.4 ms, CPU_Main_Thread_Frame_Time 1.0 ms
- 31.90 s: 60 ms frame, 1 physics steps; ours: CaptureSave 11.4 ms (1), Clothes 6.9 ms (3), UnitStep 0.7 ms (2); Unity: Main_Thread 61.0 ms, PlayerLoop 59.1 ms, Gfx_WaitForGfxCommandsFromMainThread 55.7 ms, CPU_Total_Frame_Time 26.8 ms, CPU_Main_Thread_Frame_Time 25.7 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 7.9 ms, LateBehaviourUpdate 7.9 ms, CPU_Render_Thread_Frame_Time 4.7 ms
- 75.20 s: 54 ms frame, 2 physics steps; ours: CaptureSave 11.3 ms (1), Clothes 7.9 ms (3), UnitStep 1.2 ms (4), RecorderSample 0.6 ms (2); Unity: Main_Thread 53.7 ms, PlayerLoop 51.6 ms, Gfx_WaitForGfxCommandsFromMainThread 47.6 ms, CPU_Total_Frame_Time 37.0 ms, CPU_Main_Thread_Frame_Time 35.0 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 7.9 ms, LateBehaviourUpdate 7.9 ms, CPU_Render_Thread_Frame_Time 5.1 ms
- 128.71 s: 52 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: CaptureSave 11.3 ms (1), Clothes 8.1 ms (3), UnitStep 1.4 ms (4), RecorderSample 0.6 ms (2); Unity: Main_Thread 52.2 ms, PlayerLoop 50.1 ms, Gfx_WaitForGfxCommandsFromMainThread 46.6 ms, CPU_Total_Frame_Time 35.5 ms, CPU_Main_Thread_Frame_Time 33.6 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 8.2 ms, LateBehaviourUpdate 8.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 5.1 ms
- 118.69 s: 51 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: CaptureSave 11.2 ms (1), Clothes 7.9 ms (3), UnitStep 1.1 ms (4); Unity: Main_Thread 51.2 ms, PlayerLoop 49.3 ms, Gfx_WaitForGfxCommandsFromMainThread 45.9 ms, CPU_Total_Frame_Time 34.5 ms, CPU_Main_Thread_Frame_Time 32.6 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 7.7 ms, LateBehaviourUpdate 7.7 ms, GC_Collect 5.1 ms
- 134.45 s: 51 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 7.8 ms (3), UnitStep 1.7 ms (6), CaptureSave 1.7 ms (1), RecorderSample 1.3 ms (3), RecorderFlush 0.8 ms (3); Unity: Main_Thread 54.3 ms, PlayerLoop 52.0 ms, Gfx_WaitForGfxCommandsFromMainThread 49.0 ms, CPU_Total_Frame_Time 33.2 ms, CPU_Main_Thread_Frame_Time 31.3 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 9.1 ms, LateBehaviourUpdate 9.1 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 7.9 ms
- 127.70 s: 49 ms frame, 2 physics steps; ours: CaptureSave 11.2 ms (1), Clothes 8.2 ms (3), UnitStep 1.2 ms (4); Unity: Main_Thread 50.3 ms, PlayerLoop 48.1 ms, Gfx_WaitForGfxCommandsFromMainThread 44.7 ms, CPU_Total_Frame_Time 34.3 ms, CPU_Main_Thread_Frame_Time 32.0 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 10.8 ms, LateBehaviourUpdate 10.8 ms, GPU_Frame_Time 6.3 ms
- 20.90 s: 49 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: CaptureSave 11.3 ms (1), Clothes 9.4 ms (3), UnitStep 1.2 ms (4); Unity: Main_Thread 45.5 ms, PlayerLoop 43.4 ms, Gfx_WaitForGfxCommandsFromMainThread 40.1 ms, CPU_Total_Frame_Time 27.8 ms, CPU_Main_Thread_Frame_Time 26.5 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 7.5 ms, LateBehaviourUpdate 7.5 ms, CPU_Render_Thread_Frame_Time 4.8 ms
- 68.32 s: 49 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 7.4 ms (3), CaptureSave 1.9 ms (1), UnitStep 1.9 ms (6), RecorderSample 1.2 ms (3); Unity: Main_Thread 52.0 ms, PlayerLoop 49.8 ms, Gfx_WaitForGfxCommandsFromMainThread 46.6 ms, CPU_Total_Frame_Time 35.7 ms, CPU_Main_Thread_Frame_Time 34.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 8.6 ms, FixedBehaviourUpdate 8.6 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 8.0 ms
- 13.06 s: 49 ms frame, 1 physics steps; ours: CaptureSave 11.2 ms (1), Clothes 7.4 ms (3), UnitStep 0.8 ms (2); Unity: Main_Thread 49.5 ms, PlayerLoop 46.4 ms, Gfx_WaitForGfxCommandsFromMainThread 44.0 ms, CPU_Total_Frame_Time 32.3 ms, CPU_Main_Thread_Frame_Time 30.3 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 9.3 ms, LateBehaviourUpdate 9.3 ms, CPU_Render_Thread_Frame_Time 4.9 ms
- 19.43 s: 48 ms frame, 2 physics steps; ours: CaptureSave 11.2 ms (1), Clothes 7.8 ms (3), UnitStep 1.3 ms (4); Unity: Main_Thread 48.0 ms, PlayerLoop 45.8 ms, Gfx_WaitForGfxCommandsFromMainThread 41.9 ms, CPU_Total_Frame_Time 36.7 ms, CPU_Main_Thread_Frame_Time 35.4 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 9.4 ms, LateBehaviourUpdate 9.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 5.6 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

