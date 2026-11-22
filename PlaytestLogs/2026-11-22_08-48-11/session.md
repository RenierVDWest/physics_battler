# Playtest 2026-11-22_08-48-11

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 44.1 s of play (2647 physics steps), 91 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-22_08-48-11`.

## Target

Present 1.4–45.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 10, 182 damage. Health at the end 0.


## Swordsman

Present 1.4–45.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 10 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 182 damage. Hits taken: 0, 0 damage. Health at the end 0.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

2781 frames in 44.9 s (62 fps on average); 283 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 15.0 ms, 95% 25.1, 99% 29.9, worst 263.7. 5 hitches over 40 ms (2 over 100 ms). 284 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 91.2
- RecorderSample: 21.6
- CaptureSave: 14.3
- RecorderFlush: 13.0
- Sandbox: 7.0
- ArmSolve: 0.3
- CaptureRender: 0.2
- Compose: 0.0
- ComposeDry: 0.0
- Plan: 0.0
- Prepare: 0.0
- TryOut: 0.0
- Clearance: 0.0
- Sweep: 0.0
- Authority: 0.0

The worst frames:

- 0.05 s: 264 ms frame, 0 physics steps, garbage collected (gen0 3, gen1 3, gen2 3); ours: CaptureRender 1.4 ms (1); Unity: PlayerLoop 51.4 ms, Gfx_WaitForGfxCommandsFromMainThread 44.1 ms, Physics_Simulate 43.5 ms, GC_Collect 10.9 ms, Update_ScriptRunBehaviourUpdate 2.8 ms, BehaviourUpdate 2.8 ms, CPU_Total_Frame_Time 1.6 ms, CPU_Main_Thread_Frame_Time 1.0 ms
- 0.10 s: 144 ms frame, 3 physics steps; ours: UnitStep 75.4 ms (6), RecorderSample 15.4 ms (3), Sandbox 12.9 ms (3); Unity: Main_Thread 144.0 ms, Gfx_WaitForGfxCommandsFromMainThread 135.3 ms, PlayerLoop 134.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 116.1 ms, FixedBehaviourUpdate 116.1 ms, GC_Collect 5.6 ms, Physics_Simulate 3.4 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 3.3 ms
- 39.06 s: 57 ms frame, 1 physics steps; ours: UnitStep 1.4 ms (2); Unity: CPU_Total_Frame_Time 19.5 ms, CPU_Main_Thread_Frame_Time 18.1 ms, Main_Thread 14.8 ms, PlayerLoop 13.3 ms, Gfx_WaitForGfxCommandsFromMainThread 9.8 ms, CPU_Render_Thread_Frame_Time 4.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.1 ms, FixedBehaviourUpdate 3.1 ms
- 11.75 s: 44 ms frame, 1 physics steps; ours: CaptureSave 1.9 ms (1), UnitStep 1.5 ms (2); Unity: Main_Thread 44.6 ms, PlayerLoop 40.4 ms, Gfx_WaitForGfxCommandsFromMainThread 39.3 ms, CPU_Total_Frame_Time 17.8 ms, CPU_Main_Thread_Frame_Time 16.7 ms, CPU_Render_Thread_Frame_Time 4.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.8 ms, FixedBehaviourUpdate 3.8 ms
- 9.73 s: 43 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: CaptureSave 11.3 ms (1), UnitStep 1.5 ms (2); Unity: Main_Thread 34.6 ms, PlayerLoop 33.0 ms, Gfx_WaitForGfxCommandsFromMainThread 29.5 ms, CPU_Total_Frame_Time 14.8 ms, CPU_Main_Thread_Frame_Time 13.5 ms, CPU_Render_Thread_Frame_Time 4.1 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.3 ms, FixedBehaviourUpdate 3.3 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

