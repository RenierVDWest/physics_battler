# Playtest 2026-11-17_21-41-53

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 134.0 s of play (8040 physics steps), 80 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-17_21-41-53`.

## Target

Present 1.9–135.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 4, 37 damage. Health at the end 100.


## Swordsman

Present 1.9–135.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 4 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 37 damage. Hits taken: 0, 0 damage. Health at the end 100.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

26527 frames in 134.3 s (198 fps on average); 18534 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 6.4 ms, 95% 11.2, 99% 14.2, worst 225.9. 14 hitches over 40 ms (1 over 100 ms). 1011 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 30.3
- RecorderSample: 16.8
- RecorderFlush: 9.6
- Sandbox: 5.1
- CaptureSave: 5.1
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

- 0.05 s: 226 ms frame, 0 physics steps, garbage collected (gen0 4, gen1 4, gen2 4); ours: CaptureRender 1.2 ms (1); Unity: PlayerLoop 28.1 ms, Physics_Simulate 27.7 ms, Gfx_WaitForGfxCommandsFromMainThread 25.0 ms, GC_Collect 10.0 ms, Update_ScriptRunBehaviourUpdate 2.2 ms, BehaviourUpdate 2.2 ms, CPU_Total_Frame_Time 1.4 ms, CPU_Main_Thread_Frame_Time 0.9 ms
- 63.46 s: 76 ms frame, 3 physics steps; ours: Sandbox 21.6 ms (3), UnitStep 6.9 ms (6), RecorderSample 6.2 ms (3), RecorderFlush 3.9 ms (3); Unity: Main_Thread 79.5 ms, PlayerLoop 72.4 ms, Gfx_WaitForGfxCommandsFromMainThread 67.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 44.1 ms, FixedBehaviourUpdate 44.1 ms, CPU_Total_Frame_Time 22.4 ms, CPU_Main_Thread_Frame_Time 18.3 ms, GC_Collect 13.0 ms
- 0.10 s: 75 ms frame, 3 physics steps; ours: UnitStep 27.9 ms (6), RecorderSample 14.0 ms (3), Sandbox 8.5 ms (3); Unity: Main_Thread 75.1 ms, Gfx_WaitForGfxCommandsFromMainThread 68.6 ms, PlayerLoop 67.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 59.4 ms, FixedBehaviourUpdate 59.4 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 2.7 ms, LateBehaviourUpdate 2.7 ms, Physics_Simulate 1.9 ms
- 63.41 s: 72 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: CaptureSave 8.8 ms (1), UnitStep 4.4 ms (4), RecorderSample 2.1 ms (2), Sandbox 0.8 ms (2); Unity: Main_Thread 71.2 ms, PlayerLoop 64.0 ms, Gfx_WaitForGfxCommandsFromMainThread 61.6 ms, CPU_Total_Frame_Time 23.6 ms, CPU_Main_Thread_Frame_Time 20.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 14.2 ms, FixedBehaviourUpdate 14.2 ms, CPU_Render_Thread_Frame_Time 5.3 ms
- 48.99 s: 65 ms frame, 3 physics steps; ours: CaptureSave 19.9 ms (1), UnitStep 6.2 ms (6), RecorderSample 3.5 ms (3), RecorderFlush 2.6 ms (3), Sandbox 1.3 ms (3); Unity: Main_Thread 66.5 ms, PlayerLoop 59.6 ms, Gfx_WaitForGfxCommandsFromMainThread 54.8 ms, CPU_Total_Frame_Time 31.7 ms, CPU_Main_Thread_Frame_Time 26.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 18.5 ms, FixedBehaviourUpdate 18.5 ms, GC_Collect 8.3 ms
- 48.94 s: 54 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 4.2 ms (4), RecorderSample 2.5 ms (2), RecorderFlush 0.8 ms (2), Sandbox 0.5 ms (2); Unity: Main_Thread 53.8 ms, PlayerLoop 46.4 ms, Gfx_WaitForGfxCommandsFromMainThread 44.0 ms, CPU_Total_Frame_Time 36.4 ms, CPU_Main_Thread_Frame_Time 29.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 12.3 ms, FixedBehaviourUpdate 12.3 ms, CPU_Render_Thread_Frame_Time 6.9 ms
- 63.71 s: 50 ms frame, 2 physics steps; ours: CaptureSave 7.4 ms (1), UnitStep 3.5 ms (4), RecorderSample 3.3 ms (2), RecorderFlush 2.6 ms (2); Unity: Main_Thread 49.1 ms, PlayerLoop 43.8 ms, CPU_Total_Frame_Time 41.9 ms, Gfx_WaitForGfxCommandsFromMainThread 38.7 ms, CPU_Main_Thread_Frame_Time 36.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 13.0 ms, FixedBehaviourUpdate 13.0 ms, CPU_Render_Thread_Frame_Time 6.4 ms
- 48.42 s: 47 ms frame, 3 physics steps; ours: UnitStep 6.7 ms (6), RecorderSample 1.8 ms (3), Sandbox 1.0 ms (3), RecorderFlush 0.7 ms (3); Unity: Main_Thread 67.3 ms, Gfx_WaitForGfxCommandsFromMainThread 59.5 ms, PlayerLoop 39.0 ms, CPU_Total_Frame_Time 26.6 ms, CPU_Main_Thread_Frame_Time 23.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 20.6 ms, FixedBehaviourUpdate 20.6 ms, Physics_Simulate 7.3 ms
- 63.62 s: 45 ms frame, 2 physics steps; ours: CaptureSave 7.1 ms (1), RecorderSample 3.0 ms (2), UnitStep 2.8 ms (4), RecorderFlush 2.5 ms (2); Unity: CPU_Total_Frame_Time 79.6 ms, CPU_Main_Thread_Frame_Time 72.4 ms, Main_Thread 44.5 ms, PlayerLoop 38.0 ms, Gfx_WaitForGfxCommandsFromMainThread 34.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 11.5 ms, FixedBehaviourUpdate 11.5 ms, CPU_Render_Thread_Frame_Time 7.3 ms
- 48.58 s: 45 ms frame, 2 physics steps; ours: UnitStep 6.4 ms (4), CaptureSave 5.1 ms (1), RecorderSample 1.4 ms (2), RecorderFlush 0.7 ms (2), Sandbox 0.6 ms (2); Unity: Main_Thread 47.7 ms, PlayerLoop 40.2 ms, CPU_Total_Frame_Time 36.4 ms, Gfx_WaitForGfxCommandsFromMainThread 36.3 ms, CPU_Main_Thread_Frame_Time 30.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 14.9 ms, FixedBehaviourUpdate 14.9 ms, CPU_Render_Thread_Frame_Time 4.8 ms
- 63.66 s: 44 ms frame, 3 physics steps; ours: UnitStep 5.4 ms (6), RecorderSample 1.8 ms (3), RecorderFlush 1.1 ms (3), Sandbox 1.0 ms (3); Unity: Main_Thread 44.5 ms, CPU_Total_Frame_Time 39.0 ms, PlayerLoop 38.2 ms, Gfx_WaitForGfxCommandsFromMainThread 34.9 ms, CPU_Main_Thread_Frame_Time 33.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 18.4 ms, FixedBehaviourUpdate 18.3 ms, CPU_Render_Thread_Frame_Time 7.9 ms
- 48.70 s: 43 ms frame, 2 physics steps; ours: CaptureSave 5.2 ms (1), UnitStep 3.9 ms (4), RecorderSample 2.3 ms (2), RecorderFlush 1.9 ms (2), Sandbox 0.5 ms (2); Unity: Main_Thread 44.1 ms, PlayerLoop 36.7 ms, Gfx_WaitForGfxCommandsFromMainThread 35.0 ms, CPU_Total_Frame_Time 33.6 ms, CPU_Main_Thread_Frame_Time 24.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 12.2 ms, FixedBehaviourUpdate 12.2 ms, CPU_Render_Thread_Frame_Time 6.7 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

