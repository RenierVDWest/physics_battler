# Playtest 2026-11-17_07-06-23

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 701.0 s of play (42060 physics steps), 86 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-17_07-06-23`.

## Target

Present 1.2–702.2 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Present 1.2–702.2 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

123138 frames in 701.2 s (176 fps on average); 81092 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 7.3 ms, 95% 14.1, 99% 18.0, worst 188.8. 4 hitches over 40 ms (1 over 100 ms). 2297 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 29.8
- RecorderSample: 14.8
- RecorderFlush: 6.9
- Sandbox: 5.8
- CaptureSave: 1.3
- CaptureRender: 0.0
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

- 0.05 s: 189 ms frame, 0 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: CaptureRender 1.4 ms (1); Unity: Physics_Simulate 32.1 ms, PlayerLoop 23.1 ms, Gfx_WaitForGfxCommandsFromMainThread 22.7 ms, GC_Collect 6.1 ms, Update_ScriptRunBehaviourUpdate 2.5 ms, BehaviourUpdate 2.5 ms, CPU_Total_Frame_Time 1.6 ms, CPU_Main_Thread_Frame_Time 1.0 ms
- 0.10 s: 92 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 36.0 ms (6), RecorderSample 14.1 ms (3), Sandbox 8.6 ms (3); Unity: Main_Thread 92.6 ms, Gfx_WaitForGfxCommandsFromMainThread 88.1 ms, PlayerLoop 84.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 68.5 ms, FixedBehaviourUpdate 68.5 ms, GC_Collect 7.3 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 3.0 ms, LateBehaviourUpdate 3.0 ms
- 6.22 s: 56 ms frame, 1 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 8.4 ms, CPU_Total_Frame_Time 7.5 ms, Gfx_WaitForGfxCommandsFromMainThread 6.3 ms, PlayerLoop 6.2 ms, CPU_Main_Thread_Frame_Time 5.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.6 ms, FixedBehaviourUpdate 2.6 ms, CPU_Render_Thread_Frame_Time 1.8 ms
- 536.21 s: 55 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 8.2 ms, CPU_Total_Frame_Time 5.8 ms, Gfx_WaitForGfxCommandsFromMainThread 5.7 ms, PlayerLoop 4.5 ms, CPU_Main_Thread_Frame_Time 4.1 ms, CPU_Render_Thread_Frame_Time 2.0 ms, GPU_Frame_Time 0.6 ms, GC_Alloc 0.2 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

