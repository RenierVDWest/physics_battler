# Playtest 2026-11-22_07-17-24

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 295.0 s of play (17701 physics steps), 166 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-22_07-17-24`.

## Target

Present 1.1–296.1 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 16, 238 damage. Health at the end 0.


## Swordsman

Present 1.1–296.1 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 16 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 238 damage. Hits taken: 0, 0 damage. Health at the end 0.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

17459 frames in 295.4 s (59 fps on average); 1330 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 15.2 ms, 95% 26.7, 99% 33.4, worst 241.6. 41 hitches over 40 ms (2 over 100 ms). 1918 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 142.7
- RecorderSample: 16.4
- CaptureSave: 13.3
- RecorderFlush: 9.1
- Sandbox: 6.2
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

- 0.05 s: 242 ms frame, 0 physics steps, garbage collected (gen0 4, gen1 4, gen2 4); ours: CaptureRender 1.3 ms (1); Unity: PlayerLoop 48.6 ms, Gfx_WaitForGfxCommandsFromMainThread 40.9 ms, Physics_Simulate 33.7 ms, GC_Collect 14.8 ms, Update_ScriptRunBehaviourUpdate 2.4 ms, BehaviourUpdate 2.4 ms, CPU_Total_Frame_Time 2.3 ms, CPU_Main_Thread_Frame_Time 1.7 ms
- 0.10 s: 134 ms frame, 3 physics steps; ours: UnitStep 67.5 ms (6), RecorderSample 18.9 ms (3), Sandbox 14.1 ms (3); Unity: Main_Thread 134.6 ms, PlayerLoop 127.0 ms, Gfx_WaitForGfxCommandsFromMainThread 126.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 111.6 ms, FixedBehaviourUpdate 111.6 ms, GC_Collect 5.2 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 2.8 ms, LateBehaviourUpdate 2.8 ms
- 169.68 s: 94 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 5.9 ms (6), Sandbox 0.6 ms (3); Unity: Main_Thread 67.5 ms, Gfx_WaitForGfxCommandsFromMainThread 61.9 ms, PlayerLoop 27.6 ms, CPU_Total_Frame_Time 14.1 ms, CPU_Main_Thread_Frame_Time 13.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 12.6 ms, FixedBehaviourUpdate 12.6 ms, CPU_Render_Thread_Frame_Time 4.7 ms
- 46.90 s: 71 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 37.4 ms (6), CaptureSave 1.7 ms (1), RecorderSample 0.9 ms (3), RecorderFlush 0.5 ms (3); Unity: Main_Thread 66.3 ms, CPU_Total_Frame_Time 64.6 ms, PlayerLoop 63.9 ms, CPU_Main_Thread_Frame_Time 62.6 ms, Gfx_WaitForGfxCommandsFromMainThread 60.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 43.9 ms, FixedBehaviourUpdate 43.9 ms, CPU_Render_Thread_Frame_Time 4.5 ms
- 46.65 s: 66 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 36.6 ms (6), RecorderSample 0.7 ms (3); Unity: Main_Thread 62.1 ms, PlayerLoop 59.9 ms, Gfx_WaitForGfxCommandsFromMainThread 57.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 41.8 ms, FixedBehaviourUpdate 41.7 ms, CPU_Total_Frame_Time 38.7 ms, CPU_Main_Thread_Frame_Time 36.7 ms, CPU_Render_Thread_Frame_Time 4.6 ms
- 46.75 s: 62 ms frame, 3 physics steps; ours: UnitStep 36.7 ms (6), CaptureSave 1.8 ms (1), RecorderSample 0.6 ms (3); Unity: Main_Thread 61.6 ms, PlayerLoop 59.4 ms, Gfx_WaitForGfxCommandsFromMainThread 56.1 ms, CPU_Total_Frame_Time 43.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 42.6 ms, FixedBehaviourUpdate 42.6 ms, CPU_Main_Thread_Frame_Time 41.6 ms, CPU_Render_Thread_Frame_Time 4.5 ms
- 46.80 s: 61 ms frame, 3 physics steps; ours: UnitStep 37.1 ms (6), RecorderSample 0.9 ms (3), RecorderFlush 0.6 ms (3); Unity: Main_Thread 60.5 ms, CPU_Total_Frame_Time 59.5 ms, PlayerLoop 58.3 ms, CPU_Main_Thread_Frame_Time 57.4 ms, Gfx_WaitForGfxCommandsFromMainThread 55.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 43.4 ms, FixedBehaviourUpdate 43.4 ms, CPU_Render_Thread_Frame_Time 4.6 ms
- 46.85 s: 60 ms frame, 3 physics steps; ours: UnitStep 36.7 ms (6); Unity: CPU_Total_Frame_Time 62.1 ms, Main_Thread 61.0 ms, CPU_Main_Thread_Frame_Time 59.9 ms, PlayerLoop 58.1 ms, Gfx_WaitForGfxCommandsFromMainThread 55.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 42.2 ms, FixedBehaviourUpdate 42.2 ms, CPU_Render_Thread_Frame_Time 4.8 ms
- 46.70 s: 60 ms frame, 3 physics steps; ours: UnitStep 37.0 ms (6), RecorderSample 0.6 ms (3); Unity: Main_Thread 64.6 ms, PlayerLoop 62.6 ms, Gfx_WaitForGfxCommandsFromMainThread 59.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 42.6 ms, FixedBehaviourUpdate 42.6 ms, CPU_Total_Frame_Time 39.2 ms, CPU_Main_Thread_Frame_Time 37.3 ms, CPU_Render_Thread_Frame_Time 4.8 ms
- 46.60 s: 60 ms frame, 3 physics steps; ours: UnitStep 37.0 ms (6); Unity: Main_Thread 59.5 ms, PlayerLoop 57.4 ms, Gfx_WaitForGfxCommandsFromMainThread 54.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 42.6 ms, FixedBehaviourUpdate 42.6 ms, CPU_Total_Frame_Time 26.3 ms, CPU_Main_Thread_Frame_Time 24.2 ms, CPU_Render_Thread_Frame_Time 4.5 ms
- 130.63 s: 58 ms frame, 1 physics steps; ours: UnitStep 1.9 ms (2); Unity: Main_Thread 16.9 ms, PlayerLoop 15.1 ms, CPU_Total_Frame_Time 14.1 ms, CPU_Main_Thread_Frame_Time 13.0 ms, Gfx_WaitForGfxCommandsFromMainThread 11.9 ms, GPU_Frame_Time 7.5 ms, CPU_Render_Thread_Frame_Time 4.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 4.3 ms
- 169.63 s: 56 ms frame, 1 physics steps; ours: UnitStep 1.9 ms (2); Unity: Main_Thread 17.9 ms, PlayerLoop 15.6 ms, CPU_Total_Frame_Time 13.9 ms, CPU_Main_Thread_Frame_Time 13.0 ms, Gfx_WaitForGfxCommandsFromMainThread 12.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 4.8 ms, FixedBehaviourUpdate 4.8 ms, CPU_Render_Thread_Frame_Time 4.4 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

