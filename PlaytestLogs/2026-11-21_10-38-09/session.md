# Playtest 2026-11-21_10-38-09

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 125.4 s of play (7525 physics steps), 276 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-21_10-38-09`.

## Target

Present 1.5–126.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 42, 401 damage. Health at the end 100.


## Swordsman

Present 1.5–126.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 42 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 401 damage. Hits taken: 0, 0 damage. Health at the end 100.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

11438 frames in 125.6 s (91 fps on average); 3966 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 11.0 ms, 95% 19.4, 99% 22.4, worst 220.0. 4 hitches over 40 ms (1 over 100 ms). 952 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 38.0
- RecorderSample: 20.3
- RecorderFlush: 12.5
- CaptureSave: 10.1
- Sandbox: 6.2
- ArmSolve: 0.3
- CaptureRender: 0.1
- Compose: 0.0
- ComposeDry: 0.0
- Plan: 0.0
- Prepare: 0.0
- TryOut: 0.0
- Clearance: 0.0
- Sweep: 0.0
- Authority: 0.0

The worst frames:

- 0.05 s: 220 ms frame, 0 physics steps, garbage collected (gen0 3, gen1 3, gen2 3); ours: CaptureRender 1.3 ms (1); Unity: PlayerLoop 38.8 ms, Gfx_WaitForGfxCommandsFromMainThread 33.2 ms, Physics_Simulate 30.2 ms, GC_Collect 12.5 ms, Update_ScriptRunBehaviourUpdate 2.4 ms, BehaviourUpdate 2.4 ms, CPU_Total_Frame_Time 1.4 ms, CPU_Main_Thread_Frame_Time 1.0 ms
- 0.10 s: 83 ms frame, 3 physics steps; ours: UnitStep 29.8 ms (6), RecorderSample 13.3 ms (3), Sandbox 12.6 ms (3); Unity: Main_Thread 84.2 ms, Gfx_WaitForGfxCommandsFromMainThread 78.0 ms, PlayerLoop 77.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 65.5 ms, FixedBehaviourUpdate 65.5 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 2.5 ms, LateBehaviourUpdate 2.5 ms, Physics_Simulate 2.1 ms
- 124.62 s: 56 ms frame, 2 physics steps; ours: CaptureSave 1.8 ms (1), UnitStep 1.1 ms (4); Unity: Main_Thread 42.2 ms, Gfx_WaitForGfxCommandsFromMainThread 38.0 ms, PlayerLoop 14.7 ms, CPU_Total_Frame_Time 10.8 ms, CPU_Main_Thread_Frame_Time 9.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 4.7 ms, FixedBehaviourUpdate 4.7 ms, CPU_Render_Thread_Frame_Time 3.4 ms
- 0.92 s: 49 ms frame, 1 physics steps; ours: UnitStep 0.7 ms (2); Unity: Main_Thread 13.5 ms, CPU_Total_Frame_Time 13.5 ms, CPU_Main_Thread_Frame_Time 11.9 ms, PlayerLoop 11.1 ms, Gfx_WaitForGfxCommandsFromMainThread 8.9 ms, CPU_Render_Thread_Frame_Time 3.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.7 ms, FixedBehaviourUpdate 2.7 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

