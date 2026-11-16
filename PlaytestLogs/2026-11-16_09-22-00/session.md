# Playtest 2026-11-16_09-22-00

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 127.6 s of play (7656 physics steps), 84 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-16_09-22-00`.

## Target

Present 0.7–128.3 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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


## Swordsman

Present 0.7–128.3 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

29121 frames in 127.8 s (228 fps on average); 21473 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 6.5 ms, 95% 12.2, 99% 15.0, worst 190.5. 4 hitches over 40 ms (1 over 100 ms). 356 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 34.3
- RecorderSample: 23.0
- RecorderFlush: 15.0
- CaptureSave: 3.8
- Sandbox: 2.4
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

- 0.05 s: 190 ms frame, 0 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: CaptureRender 1.2 ms (1); Unity: Physics_Simulate 34.5 ms, Gfx_WaitForGfxCommandsFromMainThread 18.1 ms, PlayerLoop 16.5 ms, CPU_Total_Frame_Time 5.4 ms, GC_Collect 5.2 ms, CPU_Main_Thread_Frame_Time 4.7 ms, Update_ScriptRunBehaviourUpdate 2.4 ms, BehaviourUpdate 2.4 ms
- 0.10 s: 87 ms frame, 3 physics steps; ours: UnitStep 40.6 ms (6), RecorderSample 16.7 ms (3), Sandbox 4.4 ms (3); Unity: Main_Thread 87.5 ms, Gfx_WaitForGfxCommandsFromMainThread 84.2 ms, PlayerLoop 80.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 72.5 ms, FixedBehaviourUpdate 72.5 ms, CPU_Total_Frame_Time 8.2 ms, CPU_Main_Thread_Frame_Time 6.8 ms, GC_Collect 6.6 ms
- 66.89 s: 56 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 0.6 ms (2); Unity: Main_Thread 12.2 ms, PlayerLoop 10.5 ms, Gfx_WaitForGfxCommandsFromMainThread 10.2 ms, CPU_Total_Frame_Time 4.5 ms, CPU_Main_Thread_Frame_Time 3.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.6 ms, FixedBehaviourUpdate 2.6 ms, CPU_Render_Thread_Frame_Time 1.5 ms
- 6.83 s: 44 ms frame, 1 physics steps; ours: nothing over 0.5 ms; Unity: CPU_Total_Frame_Time 10.9 ms, CPU_Main_Thread_Frame_Time 9.0 ms, Main_Thread 7.1 ms, Gfx_WaitForGfxCommandsFromMainThread 5.3 ms, PlayerLoop 4.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.4 ms, FixedBehaviourUpdate 2.4 ms, CPU_Render_Thread_Frame_Time 2.0 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

