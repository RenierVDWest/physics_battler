# Playtest 2026-11-22_08-36-15

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 368.6 s of play (22117 physics steps), 273 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-22_08-36-15`.

## Target

Present 1.6–370.2 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 34, 445 damage. Health at the end 0.


## Swordsman

Present 1.6–370.2 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 34 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 445 damage. Hits taken: 0, 0 damage. Health at the end 0.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

47693 frames in 625.2 s (76 fps on average); 25646 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 14.4 ms, 95% 23.4, 99% 28.3, worst 242.6. 10 hitches over 40 ms (3 over 100 ms). 4851 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 47.0
- RecorderSample: 11.0
- RecorderFlush: 6.4
- CaptureSave: 4.1
- Sandbox: 3.9
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

- 0.05 s: 243 ms frame, 0 physics steps, garbage collected (gen0 3, gen1 3, gen2 3); ours: CaptureRender 1.2 ms (1); Unity: PlayerLoop 47.3 ms, Gfx_WaitForGfxCommandsFromMainThread 40.3 ms, Physics_Simulate 33.4 ms, GC_Collect 11.0 ms, Update_ScriptRunBehaviourUpdate 2.2 ms, BehaviourUpdate 2.2 ms, CPU_Total_Frame_Time 1.6 ms, CPU_Main_Thread_Frame_Time 1.1 ms
- 300.82 s: 149 ms frame, 1 physics steps; ours: UnitStep 1.5 ms (2); Unity: Main_Thread 15.2 ms, PlayerLoop 13.6 ms, Gfx_WaitForGfxCommandsFromMainThread 10.1 ms, CPU_Total_Frame_Time 9.2 ms, CPU_Main_Thread_Frame_Time 8.5 ms, CPU_Render_Thread_Frame_Time 4.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.9 ms, FixedBehaviourUpdate 3.9 ms
- 0.10 s: 147 ms frame, 3 physics steps; ours: UnitStep 85.2 ms (6), RecorderSample 14.3 ms (3), Sandbox 12.3 ms (3); Unity: Main_Thread 145.8 ms, Gfx_WaitForGfxCommandsFromMainThread 138.1 ms, PlayerLoop 137.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 122.0 ms, FixedBehaviourUpdate 122.0 ms, GC_Collect 5.6 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 2.8 ms, LateBehaviourUpdate 2.8 ms
- 178.30 s: 99 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 14.7 ms, CPU_Total_Frame_Time 12.5 ms, PlayerLoop 11.7 ms, CPU_Main_Thread_Frame_Time 10.6 ms, Gfx_WaitForGfxCommandsFromMainThread 8.7 ms, CPU_Render_Thread_Frame_Time 4.9 ms, GPU_Frame_Time 0.8 ms, GC_Alloc 0.4 ms
- 191.41 s: 54 ms frame, 1 physics steps; ours: UnitStep 1.6 ms (2); Unity: Main_Thread 17.1 ms, PlayerLoop 14.8 ms, Gfx_WaitForGfxCommandsFromMainThread 11.2 ms, CPU_Total_Frame_Time 10.9 ms, CPU_Main_Thread_Frame_Time 9.2 ms, CPU_Render_Thread_Frame_Time 5.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 4.1 ms, FixedBehaviourUpdate 4.1 ms
- 149.10 s: 52 ms frame, 1 physics steps; ours: UnitStep 1.6 ms (2); Unity: CPU_Total_Frame_Time 20.4 ms, CPU_Main_Thread_Frame_Time 18.4 ms, Main_Thread 16.5 ms, PlayerLoop 14.1 ms, Gfx_WaitForGfxCommandsFromMainThread 11.5 ms, CPU_Render_Thread_Frame_Time 4.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 4.1 ms, FixedBehaviourUpdate 4.1 ms
- 368.00 s: 49 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 21.0 ms, CPU_Total_Frame_Time 18.9 ms, CPU_Main_Thread_Frame_Time 17.8 ms, Gfx_WaitForGfxCommandsFromMainThread 16.0 ms, PlayerLoop 10.2 ms, CPU_Render_Thread_Frame_Time 4.1 ms, GPU_Frame_Time 1.5 ms, GC_Alloc 0.4 ms
- 178.15 s: 41 ms frame, 1 physics steps; ours: UnitStep 1.5 ms (2); Unity: Main_Thread 23.2 ms, PlayerLoop 20.9 ms, Gfx_WaitForGfxCommandsFromMainThread 17.7 ms, CPU_Total_Frame_Time 13.5 ms, CPU_Main_Thread_Frame_Time 12.6 ms, CPU_Render_Thread_Frame_Time 4.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.9 ms, FixedBehaviourUpdate 3.9 ms
- 199.19 s: 41 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: CaptureSave 1.9 ms (1), UnitStep 1.5 ms (2); Unity: Main_Thread 37.5 ms, PlayerLoop 35.2 ms, Gfx_WaitForGfxCommandsFromMainThread 31.9 ms, CPU_Total_Frame_Time 23.2 ms, CPU_Main_Thread_Frame_Time 20.4 ms, CPU_Render_Thread_Frame_Time 4.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.8 ms, FixedBehaviourUpdate 3.8 ms
- 35.31 s: 40 ms frame, 1 physics steps; ours: UnitStep 1.5 ms (2); Unity: Main_Thread 40.8 ms, PlayerLoop 38.6 ms, Gfx_WaitForGfxCommandsFromMainThread 35.0 ms, CPU_Total_Frame_Time 14.0 ms, CPU_Main_Thread_Frame_Time 12.9 ms, CPU_Render_Thread_Frame_Time 4.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.3 ms, FixedBehaviourUpdate 3.3 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

