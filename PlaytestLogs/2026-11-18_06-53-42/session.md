# Playtest 2026-11-18_06-53-42

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 313.0 s of play (18781 physics steps), 513 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-18_06-53-42`.

## Target

Present 3.2–7.4 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Present 3.2–7.4 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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


## Target #2

Present 7.4–316.2 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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
| UpperArmR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ForearmR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| HandR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 15 (0 on the part aimed at, 13 blocked, 0 parried, 0 on someone in the way, 0 on allies), 21 damage. Hits taken: 42, 140 damage. Health at the end 100.


## Swordsman #2

Present 7.4–316.2 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 42 (0 on the part aimed at, 26 blocked, 0 parried, 0 on someone in the way, 0 on allies), 140 damage. Hits taken: 15, 21 damage. Health at the end 0.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

40256 frames in 520.3 s (77 fps on average); 21519 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 9.1 ms, 95% 15.8, 99% 19.2, worst 87161.1. 23 hitches over 40 ms (9 over 100 ms). 2218 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 23.0
- RecorderSample: 13.9
- RecorderFlush: 9.3
- Sandbox: 5.6
- CaptureSave: 5.5
- CaptureRender: 0.0
- ArmSolve: 0.0
- Compose: 0.0
- ComposeDry: 0.0
- Plan: 0.0
- Prepare: 0.0
- TryOut: 0.0
- Clearance: 0.0
- Sweep: 0.0
- Authority: 0.0

The worst frames:

- 246.75 s: 87161 ms frame, 0 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: nothing over 0.5 ms; Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.3 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms
- 280.01 s: 63984 ms frame, 1 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: UnitStep 0.7 ms (2); Unity: Main_Thread 0.3 ms, Gfx_WaitForGfxCommandsFromMainThread 0.3 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 110.22 s: 18472 ms frame, 0 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: nothing over 0.5 ms; Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.4 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 243.49 s: 15851 ms frame, 1 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: RecorderSample 0.6 ms (1), UnitStep 0.6 ms (2), RecorderFlush 0.5 ms (1); Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.3 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 109.00 s: 15530 ms frame, 0 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: nothing over 0.5 ms; Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.4 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms
- 100.13 s: 5514 ms frame, 1 physics steps, garbage collected (gen0 3, gen1 3, gen2 3); ours: UnitStep 0.6 ms (2); Unity: Main_Thread 0.3 ms, Gfx_WaitForGfxCommandsFromMainThread 0.1 ms, CPU_Total_Frame_Time 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms
- 56.72 s: 537 ms frame, 0 physics steps; ours: CaptureSave 529.7 ms (1); Unity: Main_Thread 536.8 ms, PlayerLoop 535.2 ms, Gfx_WaitForGfxCommandsFromMainThread 533.9 ms, CPU_Total_Frame_Time 13.1 ms, CPU_Main_Thread_Frame_Time 12.0 ms, CPU_Render_Thread_Frame_Time 2.7 ms, GPU_Frame_Time 0.7 ms, GC_Alloc 0.2 ms
- 0.05 s: 219 ms frame, 0 physics steps, garbage collected (gen0 3, gen1 3, gen2 3); ours: CaptureRender 1.3 ms (1); Unity: Physics_Simulate 34.3 ms, PlayerLoop 26.4 ms, Gfx_WaitForGfxCommandsFromMainThread 25.1 ms, GC_Collect 12.7 ms, Update_ScriptRunBehaviourUpdate 2.1 ms, BehaviourUpdate 2.1 ms, CPU_Total_Frame_Time 1.5 ms, CPU_Main_Thread_Frame_Time 0.9 ms
- 4.25 s: 101 ms frame, 1 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: UnitStep 0.5 ms (2); Unity: Main_Thread 99.7 ms, PlayerLoop 97.8 ms, Gfx_WaitForGfxCommandsFromMainThread 96.8 ms, GC_Collect 10.0 ms, CPU_Total_Frame_Time 7.0 ms, CPU_Main_Thread_Frame_Time 6.1 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.3 ms, FixedBehaviourUpdate 2.3 ms
- 0.10 s: 95 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 37.4 ms (6), RecorderSample 14.8 ms (3), Sandbox 9.2 ms (3); Unity: Main_Thread 95.1 ms, Gfx_WaitForGfxCommandsFromMainThread 90.4 ms, PlayerLoop 86.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 71.9 ms, FixedBehaviourUpdate 71.9 ms, GC_Collect 6.1 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 3.1 ms, LateBehaviourUpdate 3.1 ms
- 47.03 s: 68 ms frame, 3 physics steps; ours: RecorderSample 6.3 ms (3), UnitStep 4.3 ms (6), CaptureSave 3.5 ms (1), RecorderFlush 2.1 ms (3), Sandbox 0.8 ms (3); Unity: Main_Thread 64.6 ms, PlayerLoop 60.0 ms, Gfx_WaitForGfxCommandsFromMainThread 54.9 ms, CPU_Total_Frame_Time 51.7 ms, CPU_Main_Thread_Frame_Time 47.1 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 29.9 ms, FixedBehaviourUpdate 29.8 ms, Physics_Simulate 11.8 ms
- 47.08 s: 67 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: RecorderSample 4.7 ms (3), UnitStep 4.7 ms (6), Sandbox 1.9 ms (3), RecorderFlush 1.9 ms (3); Unity: Main_Thread 70.5 ms, PlayerLoop 65.9 ms, Gfx_WaitForGfxCommandsFromMainThread 60.0 ms, CPU_Total_Frame_Time 58.8 ms, CPU_Main_Thread_Frame_Time 54.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 28.8 ms, FixedBehaviourUpdate 28.8 ms, Physics_Simulate 11.3 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

