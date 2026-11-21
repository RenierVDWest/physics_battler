# Playtest 2026-11-21_12-35-52

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 206.6 s of play (12398 physics steps), 408 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-21_12-35-52`.

## Target

Present 1.3–207.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 61, 1274 damage. Health at the end 0.


## Swordsman

Present 1.3–207.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 61 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 1274 damage. Hits taken: 0, 0 damage. Health at the end 0.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

18634 frames in 419.3 s (44 fps on average); 6331 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 11.2 ms, 95% 19.5, 99% 22.4, worst 212416.0. 6 hitches over 40 ms (2 over 100 ms). 1500 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 18.2
- RecorderSample: 9.6
- RecorderFlush: 5.8
- CaptureSave: 5.0
- Sandbox: 3.1
- ArmSolve: 0.3
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

- 82.23 s: 212416 ms frame, 1 physics steps, garbage collected (gen0 6, gen1 6, gen2 6); ours: UnitStep 0.6 ms (2); Unity: Main_Thread 0.3 ms, Gfx_WaitForGfxCommandsFromMainThread 0.3 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms
- 0.05 s: 225 ms frame, 0 physics steps, garbage collected (gen0 3, gen1 3, gen2 3); ours: CaptureRender 1.2 ms (1); Unity: PlayerLoop 41.1 ms, Gfx_WaitForGfxCommandsFromMainThread 35.6 ms, Physics_Simulate 29.6 ms, GC_Collect 12.0 ms, CPU_Total_Frame_Time 2.4 ms, Update_ScriptRunBehaviourUpdate 2.3 ms, BehaviourUpdate 2.3 ms, CPU_Main_Thread_Frame_Time 1.3 ms
- 0.10 s: 99 ms frame, 3 physics steps; ours: UnitStep 42.8 ms (6), RecorderSample 13.5 ms (3), Sandbox 12.6 ms (3); Unity: Main_Thread 99.8 ms, Gfx_WaitForGfxCommandsFromMainThread 93.6 ms, PlayerLoop 91.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 78.9 ms, FixedBehaviourUpdate 78.9 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 2.6 ms, LateBehaviourUpdate 2.6 ms, Physics_Simulate 2.3 ms
- 58.56 s: 51 ms frame, 1 physics steps; ours: RecorderSample 0.7 ms (1), RecorderFlush 0.6 ms (1), UnitStep 0.6 ms (2); Unity: Main_Thread 13.7 ms, CPU_Total_Frame_Time 11.7 ms, PlayerLoop 11.6 ms, CPU_Main_Thread_Frame_Time 10.5 ms, Gfx_WaitForGfxCommandsFromMainThread 9.2 ms, CPU_Render_Thread_Frame_Time 3.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.1 ms, FixedBehaviourUpdate 3.1 ms
- 120.14 s: 50 ms frame, 1 physics steps; ours: UnitStep 0.6 ms (2); Unity: Main_Thread 21.1 ms, PlayerLoop 16.9 ms, Gfx_WaitForGfxCommandsFromMainThread 16.7 ms, CPU_Total_Frame_Time 10.5 ms, CPU_Main_Thread_Frame_Time 9.3 ms, CPU_Render_Thread_Frame_Time 4.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.8 ms, FixedBehaviourUpdate 2.8 ms
- 71.55 s: 49 ms frame, 1 physics steps; ours: RecorderSample 0.7 ms (1), UnitStep 0.6 ms (2), RecorderFlush 0.6 ms (1); Unity: Main_Thread 18.9 ms, PlayerLoop 16.8 ms, Gfx_WaitForGfxCommandsFromMainThread 14.2 ms, CPU_Total_Frame_Time 11.2 ms, CPU_Main_Thread_Frame_Time 9.9 ms, CPU_Render_Thread_Frame_Time 3.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.1 ms, FixedBehaviourUpdate 3.1 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

