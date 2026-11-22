# Playtest 2026-11-22_13-59-39

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 15.9 s of play (953 physics steps), 88 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-22_13-59-39`.

## Target

Present 1.7–17.6 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Present 1.7–17.6 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

733 frames in 16.3 s (45 fps on average); 0 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 20.0 ms, 95% 30.6, 99% 36.0, worst 291.6. 4 hitches over 40 ms (2 over 100 ms). 86 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- Clothes: 176.4
- UnitStep: 48.9
- RecorderSample: 17.2
- CaptureSave: 15.4
- RecorderFlush: 9.6
- Sandbox: 5.8
- CaptureRender: 0.2
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

- 0.10 s: 292 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 204.4 ms (6), RecorderSample 14.8 ms (3), Sandbox 12.2 ms (3); Unity: Main_Thread 291.7 ms, PlayerLoop 283.6 ms, Gfx_WaitForGfxCommandsFromMainThread 282.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 242.2 ms, FixedBehaviourUpdate 242.2 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 20.7 ms, LateBehaviourUpdate 20.7 ms, GC_Collect 15.6 ms
- 0.05 s: 236 ms frame, 0 physics steps, garbage collected (gen0 4, gen1 4, gen2 4); ours: CaptureRender 1.3 ms (1); Unity: PlayerLoop 46.6 ms, Gfx_WaitForGfxCommandsFromMainThread 40.3 ms, Physics_Simulate 31.8 ms, GC_Collect 10.5 ms, Update_ScriptRunBehaviourUpdate 2.4 ms, BehaviourUpdate 2.4 ms, CPU_Total_Frame_Time 1.4 ms, CPU_Main_Thread_Frame_Time 0.9 ms
- 1.92 s: 50 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 6.4 ms (4), Clothes 4.3 ms (2), CaptureSave 1.9 ms (1); Unity: Main_Thread 49.4 ms, PlayerLoop 47.2 ms, Gfx_WaitForGfxCommandsFromMainThread 44.2 ms, CPU_Total_Frame_Time 28.0 ms, CPU_Main_Thread_Frame_Time 25.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 10.3 ms, FixedBehaviourUpdate 10.3 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 5.3 ms
- 7.68 s: 43 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 3.9 ms (2), UnitStep 1.2 ms (4); Unity: Main_Thread 37.7 ms, PlayerLoop 33.5 ms, Gfx_WaitForGfxCommandsFromMainThread 32.3 ms, CPU_Total_Frame_Time 17.4 ms, CPU_Main_Thread_Frame_Time 15.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 4.8 ms, FixedBehaviourUpdate 4.8 ms, CPU_Render_Thread_Frame_Time 4.4 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

