# Playtest 2026-11-19_13-26-45

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 2.1 s of play (126 physics steps), 22 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-19_13-26-45`.

## Target

Present 2.7–4.8 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Present 2.7–4.8 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

152 frames in 2.3 s (65 fps on average); 40 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 12.4 ms, 95% 24.8, 99% 65.6, worst 232.1. 4 hitches over 40 ms (1 over 100 ms). 16 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 54.6
- RecorderSample: 26.3
- RecorderFlush: 14.7
- Sandbox: 8.5
- CaptureSave: 7.5
- CaptureRender: 0.7
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

- 0.05 s: 232 ms frame, 0 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: CaptureRender 1.4 ms (1); Unity: PlayerLoop 38.8 ms, Gfx_WaitForGfxCommandsFromMainThread 32.8 ms, Physics_Simulate 32.3 ms, GC_Collect 10.7 ms, Update_ScriptRunBehaviourUpdate 2.2 ms, BehaviourUpdate 2.2 ms, CPU_Total_Frame_Time 1.3 ms, CPU_Main_Thread_Frame_Time 0.9 ms
- 0.10 s: 93 ms frame, 3 physics steps; ours: UnitStep 41.3 ms (6), RecorderSample 13.2 ms (3), Sandbox 9.3 ms (3); Unity: Main_Thread 94.1 ms, Gfx_WaitForGfxCommandsFromMainThread 87.2 ms, PlayerLoop 86.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 73.8 ms, FixedBehaviourUpdate 73.8 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 2.6 ms, LateBehaviourUpdate 2.6 ms, Physics_Simulate 2.3 ms
- 0.27 s: 66 ms frame, 1 physics steps; ours: UnitStep 0.6 ms (2); Unity: Main_Thread 65.2 ms, Gfx_WaitForGfxCommandsFromMainThread 60.9 ms, PlayerLoop 29.1 ms, CPU_Total_Frame_Time 9.8 ms, CPU_Main_Thread_Frame_Time 9.0 ms, CPU_Render_Thread_Frame_Time 3.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.8 ms, FixedBehaviourUpdate 2.8 ms
- 1.75 s: 51 ms frame, 2 physics steps; ours: CaptureSave 1.8 ms (1), UnitStep 1.2 ms (4); Unity: Main_Thread 17.4 ms, PlayerLoop 14.6 ms, CPU_Total_Frame_Time 14.1 ms, Gfx_WaitForGfxCommandsFromMainThread 13.3 ms, CPU_Main_Thread_Frame_Time 10.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 5.0 ms, FixedBehaviourUpdate 5.0 ms, CPU_Render_Thread_Frame_Time 3.5 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

