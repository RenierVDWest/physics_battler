# Playtest 2026-11-23_18-21-20

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 58.3 s of play (3500 physics steps), 424 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-23_18-21-20`.

## Target

Present 4.1–62.4 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 5, 41 damage. Health at the end 0.


## Swordsman

Present 4.1–62.4 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 5 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 41 damage. Hits taken: 0, 0 damage. Health at the end 0.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

1978 frames in 69.4 s (28 fps on average); 0 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 28.0 ms, 95% 40.6, 99% 48.4, worst 8718.1. 111 hitches over 40 ms (7 over 100 ms). 225 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- Clothes: 234.8
- UnitStep: 36.2
- CaptureSave: 23.1
- RecorderSample: 15.8
- RecorderFlush: 9.8
- Sandbox: 4.7
- ArmSolve: 0.4
- CaptureRender: 0.1
- Compose: 0.0
- ComposeDry: 0.0
- Plan: 0.0
- Prepare: 0.0
- TryOut: 0.0
- Clearance: 0.0
- Sweep: 0.0
- Authority: 0.0

The worst frames:

- 56.76 s: 8718 ms frame, 2 physics steps, garbage collected (gen0 4, gen1 4, gen2 4); ours: Clothes 12.3 ms (3), CaptureSave 1.8 ms (1), UnitStep 1.2 ms (4), RecorderSample 1.1 ms (2), RecorderFlush 0.9 ms (2); Unity: Main_Thread 0.3 ms, Gfx_WaitForGfxCommandsFromMainThread 0.2 ms, CPU_Total_Frame_Time 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 50.75 s: 609 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 9.0 ms (3), CaptureSave 1.6 ms (1), UnitStep 0.8 ms (2), RecorderSample 0.7 ms (1), RecorderFlush 0.6 ms (1); Unity: Main_Thread 0.3 ms, Gfx_WaitForGfxCommandsFromMainThread 0.3 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 57.33 s: 542 ms frame, 2 physics steps; ours: Clothes 12.1 ms (3), UnitStep 1.6 ms (4), RecorderSample 1.0 ms (2), RecorderFlush 0.8 ms (2); Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.3 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms
- 51.76 s: 410 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 7.8 ms (3), CaptureSave 1.7 ms (1), UnitStep 1.3 ms (4), RecorderSample 1.1 ms (2), RecorderFlush 1.0 ms (2); Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.3 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms
- 53.17 s: 389 ms frame, 1 physics steps; ours: Clothes 9.8 ms (3), UnitStep 0.8 ms (2), RecorderSample 0.6 ms (1), RecorderFlush 0.5 ms (1); Unity: Main_Thread 0.3 ms, Gfx_WaitForGfxCommandsFromMainThread 0.3 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 0.10 s: 271 ms frame, 3 physics steps; ours: UnitStep 202.8 ms (6), RecorderSample 14.1 ms (3), Sandbox 11.6 ms (3); Unity: Main_Thread 271.3 ms, PlayerLoop 264.4 ms, Gfx_WaitForGfxCommandsFromMainThread 263.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 238.3 ms, FixedBehaviourUpdate 238.3 ms, GC_Collect 12.5 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 11.2 ms, LateBehaviourUpdate 11.2 ms
- 0.05 s: 241 ms frame, 0 physics steps, garbage collected (gen0 5, gen1 5, gen2 5); ours: CaptureRender 0.9 ms (1); Unity: PlayerLoop 46.5 ms, Gfx_WaitForGfxCommandsFromMainThread 38.2 ms, Physics_Simulate 32.8 ms, GC_Collect 9.9 ms, Update_ScriptRunBehaviourUpdate 1.9 ms, BehaviourUpdate 1.9 ms, CPU_Total_Frame_Time 1.4 ms, GC_Alloc 0.9 ms
- 48.58 s: 55 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 11.4 ms (3), CaptureSave 11.2 ms (1), UnitStep 1.2 ms (4), RecorderSample 0.8 ms (2), RecorderFlush 0.6 ms (2); Unity: Main_Thread 56.0 ms, PlayerLoop 53.6 ms, Gfx_WaitForGfxCommandsFromMainThread 50.7 ms, CPU_Total_Frame_Time 34.9 ms, CPU_Main_Thread_Frame_Time 33.0 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 9.1 ms, LateBehaviourUpdate 9.1 ms, GC_Collect 5.8 ms
- 23.67 s: 54 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 7.8 ms (3), CaptureSave 2.3 ms (1), UnitStep 1.5 ms (4); Unity: Main_Thread 44.7 ms, PlayerLoop 41.4 ms, Gfx_WaitForGfxCommandsFromMainThread 38.8 ms, CPU_Total_Frame_Time 27.3 ms, CPU_Main_Thread_Frame_Time 25.3 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 9.3 ms, LateBehaviourUpdate 9.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 5.5 ms
- 27.06 s: 53 ms frame, 2 physics steps; ours: CaptureSave 11.2 ms (1), Clothes 8.3 ms (3), UnitStep 1.2 ms (4), RecorderSample 1.1 ms (2), RecorderFlush 0.9 ms (2); Unity: Main_Thread 58.2 ms, PlayerLoop 55.1 ms, Gfx_WaitForGfxCommandsFromMainThread 52.6 ms, CPU_Total_Frame_Time 24.7 ms, CPU_Main_Thread_Frame_Time 22.9 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 9.3 ms, LateBehaviourUpdate 9.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 5.8 ms
- 22.65 s: 53 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: CaptureSave 11.3 ms (1), Clothes 7.5 ms (3), UnitStep 1.3 ms (4), RecorderSample 0.6 ms (2); Unity: Main_Thread 48.3 ms, PlayerLoop 45.2 ms, Gfx_WaitForGfxCommandsFromMainThread 42.7 ms, CPU_Total_Frame_Time 30.2 ms, CPU_Main_Thread_Frame_Time 27.5 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 10.7 ms, LateBehaviourUpdate 10.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 5.1 ms
- 23.17 s: 52 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: CaptureSave 11.5 ms (1), Clothes 7.6 ms (3), UnitStep 1.6 ms (4); Unity: Main_Thread 46.3 ms, PlayerLoop 44.2 ms, Gfx_WaitForGfxCommandsFromMainThread 40.3 ms, CPU_Total_Frame_Time 28.5 ms, CPU_Main_Thread_Frame_Time 27.3 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 9.2 ms, LateBehaviourUpdate 9.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 5.5 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

