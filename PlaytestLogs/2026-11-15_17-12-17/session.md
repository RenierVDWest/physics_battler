# Playtest 2026-11-15_17-12-17

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 211.2 s of play (12674 physics steps), 121 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-15_17-12-17`.

## Target

Present 8.4–219.6 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 18, 143 damage. Health at the end 100.


## Swordsman

Present 8.4–219.6 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 18 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 143 damage. Hits taken: 0, 0 damage. Health at the end 100.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

51243 frames in 363.8 s (141 fps on average); 38593 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 6.3 ms, 95% 11.7, 99% 18.2, worst 152281.6. 13 hitches over 40 ms (4 over 100 ms). 486 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 19.0
- RecorderSample: 12.8
- RecorderFlush: 8.1
- CaptureSave: 2.6
- Sandbox: 1.1
- ArmSolve: 0.0
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

- 15.79 s: 152282 ms frame, 0 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: nothing over 0.5 ms; Unity: Main_Thread 0.5 ms, Gfx_WaitForGfxCommandsFromMainThread 0.4 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms
- 0.05 s: 179 ms frame, 0 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: CaptureRender 1.3 ms (1); Unity: Physics_Simulate 34.2 ms, PlayerLoop 18.1 ms, Gfx_WaitForGfxCommandsFromMainThread 17.4 ms, GC_Collect 6.2 ms, CPU_Total_Frame_Time 2.6 ms, Update_ScriptRunBehaviourUpdate 2.1 ms, BehaviourUpdate 2.1 ms, CPU_Main_Thread_Frame_Time 1.2 ms
- 156.15 s: 107 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: CPU_Total_Frame_Time 14.3 ms, CPU_Main_Thread_Frame_Time 8.4 ms, Main_Thread 5.0 ms, Gfx_WaitForGfxCommandsFromMainThread 3.0 ms, PlayerLoop 3.0 ms, CPU_Render_Thread_Frame_Time 1.5 ms, GPU_Frame_Time 0.6 ms, GC_Alloc 0.1 ms
- 156.20 s: 101 ms frame, 3 physics steps; ours: UnitStep 1.4 ms (6), RecorderSample 1.2 ms (3), RecorderFlush 0.8 ms (3); Unity: Main_Thread 117.3 ms, Gfx_WaitForGfxCommandsFromMainThread 114.9 ms, PlayerLoop 13.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 9.1 ms, FixedBehaviourUpdate 9.1 ms, CPU_Total_Frame_Time 5.3 ms, Physics_Simulate 3.9 ms, CPU_Main_Thread_Frame_Time 3.1 ms
- 188.40 s: 94 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: CPU_Total_Frame_Time 8.4 ms, CPU_Main_Thread_Frame_Time 6.7 ms, Main_Thread 6.0 ms, Gfx_WaitForGfxCommandsFromMainThread 4.2 ms, PlayerLoop 3.0 ms, CPU_Render_Thread_Frame_Time 1.5 ms, GPU_Frame_Time 1.1 ms, GC_Alloc 0.1 ms
- 0.10 s: 76 ms frame, 3 physics steps; ours: UnitStep 32.8 ms (6), RecorderSample 14.4 ms (3), Sandbox 4.0 ms (3); Unity: Main_Thread 78.0 ms, Gfx_WaitForGfxCommandsFromMainThread 74.5 ms, PlayerLoop 68.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 61.2 ms, FixedBehaviourUpdate 61.2 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 2.8 ms, LateBehaviourUpdate 2.8 ms, Physics_Simulate 2.3 ms
- 157.52 s: 63 ms frame, 1 physics steps; ours: UnitStep 0.6 ms (2); Unity: Main_Thread 25.2 ms, Gfx_WaitForGfxCommandsFromMainThread 23.0 ms, PlayerLoop 6.1 ms, CPU_Total_Frame_Time 5.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.8 ms, FixedBehaviourUpdate 2.8 ms, CPU_Main_Thread_Frame_Time 2.8 ms, CPU_Render_Thread_Frame_Time 1.5 ms
- 15.02 s: 57 ms frame, 1 physics steps; ours: UnitStep 0.5 ms (2); Unity: Main_Thread 7.0 ms, PlayerLoop 5.4 ms, Gfx_WaitForGfxCommandsFromMainThread 5.2 ms, CPU_Total_Frame_Time 4.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.8 ms, FixedBehaviourUpdate 2.8 ms, CPU_Main_Thread_Frame_Time 2.2 ms, CPU_Render_Thread_Frame_Time 1.5 ms
- 210.57 s: 54 ms frame, 1 physics steps; ours: UnitStep 0.5 ms (2); Unity: Main_Thread 6.8 ms, CPU_Total_Frame_Time 5.7 ms, PlayerLoop 5.3 ms, Gfx_WaitForGfxCommandsFromMainThread 5.0 ms, CPU_Main_Thread_Frame_Time 2.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.5 ms, FixedBehaviourUpdate 2.5 ms, CPU_Render_Thread_Frame_Time 1.7 ms
- 115.78 s: 52 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 6.7 ms, CPU_Total_Frame_Time 6.1 ms, Gfx_WaitForGfxCommandsFromMainThread 5.2 ms, CPU_Main_Thread_Frame_Time 4.9 ms, PlayerLoop 2.9 ms, CPU_Render_Thread_Frame_Time 1.3 ms, GPU_Frame_Time 0.6 ms, GC_Alloc 0.1 ms
- 23.87 s: 49 ms frame, 1 physics steps; ours: UnitStep 0.6 ms (2); Unity: Main_Thread 8.6 ms, Gfx_WaitForGfxCommandsFromMainThread 6.9 ms, PlayerLoop 5.9 ms, CPU_Total_Frame_Time 4.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.1 ms, FixedBehaviourUpdate 3.1 ms, CPU_Main_Thread_Frame_Time 2.2 ms, CPU_Render_Thread_Frame_Time 1.4 ms
- 14.03 s: 49 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 3.9 ms, CPU_Total_Frame_Time 3.8 ms, Gfx_WaitForGfxCommandsFromMainThread 2.4 ms, PlayerLoop 2.3 ms, CPU_Main_Thread_Frame_Time 2.2 ms, CPU_Render_Thread_Frame_Time 1.3 ms, GPU_Frame_Time 0.6 ms, GC_Alloc 0.1 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

