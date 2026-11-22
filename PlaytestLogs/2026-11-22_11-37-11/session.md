# Playtest 2026-11-22_11-37-11

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 129.4 s of play (7766 physics steps), 106 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-22_11-37-11`.

## Target

Present 1.6–131.0 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 8, 92 damage. Health at the end 0.


## Swordsman

Present 1.6–131.0 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 8 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 92 damage. Hits taken: 0, 0 damage. Health at the end 0.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

6168 frames in 130.1 s (47 fps on average); 22 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 19.4 ms, 95% 30.3, 99% 36.7, worst 290.0. 25 hitches over 40 ms (2 over 100 ms). 709 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 39.0
- RecorderSample: 16.5
- CaptureSave: 15.3
- RecorderFlush: 9.1
- Sandbox: 6.3
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

- 0.10 s: 290 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 194.2 ms (6), RecorderSample 15.2 ms (3), Sandbox 12.4 ms (3); Unity: Main_Thread 290.4 ms, PlayerLoop 282.0 ms, Gfx_WaitForGfxCommandsFromMainThread 281.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 232.9 ms, FixedBehaviourUpdate 232.9 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 27.8 ms, LateBehaviourUpdate 27.8 ms, GC_Collect 16.2 ms
- 0.05 s: 241 ms frame, 0 physics steps, garbage collected (gen0 5, gen1 5, gen2 5); ours: CaptureRender 1.3 ms (1); Unity: PlayerLoop 46.8 ms, Gfx_WaitForGfxCommandsFromMainThread 39.7 ms, Physics_Simulate 32.9 ms, GC_Collect 14.8 ms, Update_ScriptRunBehaviourUpdate 2.5 ms, BehaviourUpdate 2.5 ms, CPU_Total_Frame_Time 1.4 ms, CPU_Main_Thread_Frame_Time 0.9 ms
- 1.72 s: 53 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: RecorderSample 10.4 ms (1), RecorderFlush 10.2 ms (1), UnitStep 0.7 ms (2); Unity: Main_Thread 46.2 ms, PlayerLoop 44.0 ms, Gfx_WaitForGfxCommandsFromMainThread 40.3 ms, CPU_Total_Frame_Time 22.8 ms, CPU_Main_Thread_Frame_Time 20.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 13.1 ms, FixedBehaviourUpdate 13.1 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 4.8 ms
- 37.62 s: 51 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 1.6 ms (4), RecorderSample 1.2 ms (2), RecorderFlush 0.7 ms (2); Unity: Main_Thread 47.4 ms, PlayerLoop 44.2 ms, Gfx_WaitForGfxCommandsFromMainThread 41.7 ms, CPU_Total_Frame_Time 27.0 ms, CPU_Main_Thread_Frame_Time 25.2 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 6.9 ms, LateBehaviourUpdate 6.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 6.5 ms
- 126.14 s: 48 ms frame, 2 physics steps; ours: CaptureSave 1.7 ms (1), UnitStep 1.2 ms (4); Unity: Main_Thread 27.3 ms, PlayerLoop 24.5 ms, CPU_Total_Frame_Time 22.3 ms, Gfx_WaitForGfxCommandsFromMainThread 21.1 ms, CPU_Main_Thread_Frame_Time 21.1 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 5.8 ms, FixedBehaviourUpdate 5.8 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 5.3 ms
- 1.92 s: 48 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 6.2 ms (4), CaptureSave 1.7 ms (1); Unity: CPU_Total_Frame_Time 46.7 ms, Main_Thread 43.6 ms, CPU_Main_Thread_Frame_Time 41.0 ms, PlayerLoop 38.8 ms, Gfx_WaitForGfxCommandsFromMainThread 38.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 10.0 ms, FixedBehaviourUpdate 10.0 ms, CPU_Render_Thread_Frame_Time 4.9 ms
- 2.08 s: 48 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 2.2 ms (6), CaptureSave 2.0 ms (1), RecorderSample 0.8 ms (3), RecorderFlush 0.5 ms (3); Unity: CPU_Total_Frame_Time 43.6 ms, Main_Thread 40.8 ms, CPU_Main_Thread_Frame_Time 38.8 ms, PlayerLoop 37.8 ms, Gfx_WaitForGfxCommandsFromMainThread 35.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 9.2 ms, FixedBehaviourUpdate 9.2 ms, CPU_Render_Thread_Frame_Time 5.2 ms
- 112.19 s: 48 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 1.2 ms (4), RecorderSample 0.6 ms (2); Unity: Main_Thread 53.1 ms, Gfx_WaitForGfxCommandsFromMainThread 47.4 ms, PlayerLoop 46.5 ms, CPU_Total_Frame_Time 18.4 ms, CPU_Main_Thread_Frame_Time 16.8 ms, GC_Collect 5.1 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 4.9 ms, FixedBehaviourUpdate 4.9 ms
- 126.19 s: 47 ms frame, 2 physics steps; ours: UnitStep 1.8 ms (4); Unity: Main_Thread 52.0 ms, Gfx_WaitForGfxCommandsFromMainThread 45.1 ms, PlayerLoop 28.3 ms, CPU_Total_Frame_Time 20.0 ms, CPU_Main_Thread_Frame_Time 18.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 7.3 ms, FixedBehaviourUpdate 7.3 ms, CPU_Render_Thread_Frame_Time 5.0 ms
- 22.58 s: 47 ms frame, 1 physics steps; ours: UnitStep 0.6 ms (2); Unity: Main_Thread 47.8 ms, PlayerLoop 44.8 ms, Gfx_WaitForGfxCommandsFromMainThread 42.2 ms, CPU_Total_Frame_Time 24.4 ms, CPU_Main_Thread_Frame_Time 23.2 ms, CPU_Render_Thread_Frame_Time 4.6 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 4.1 ms, LateBehaviourUpdate 4.1 ms
- 123.70 s: 46 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 1.5 ms (2), RecorderSample 0.6 ms (1); Unity: Main_Thread 40.4 ms, PlayerLoop 37.7 ms, Gfx_WaitForGfxCommandsFromMainThread 31.6 ms, CPU_Total_Frame_Time 31.3 ms, CPU_Main_Thread_Frame_Time 28.1 ms, CPU_Render_Thread_Frame_Time 5.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 4.7 ms, FixedBehaviourUpdate 4.7 ms
- 37.05 s: 45 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: CaptureSave 1.8 ms (1), UnitStep 1.2 ms (4), RecorderSample 1.2 ms (2), RecorderFlush 1.0 ms (2); Unity: Main_Thread 44.2 ms, PlayerLoop 41.0 ms, Gfx_WaitForGfxCommandsFromMainThread 39.0 ms, CPU_Total_Frame_Time 38.0 ms, CPU_Main_Thread_Frame_Time 33.4 ms, GC_Collect 6.1 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 5.7 ms, LateBehaviourUpdate 5.7 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

