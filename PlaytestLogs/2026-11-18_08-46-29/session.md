# Playtest 2026-11-18_08-46-29

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 667.2 s of play (40032 physics steps), 1390 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-18_08-46-29`.

## Target

Present 1.4–30.0 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Present 1.4–30.0 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Present 30.0–668.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 58 (0 on the part aimed at, 38 blocked, 0 parried, 0 on someone in the way, 0 on allies), 128 damage. Hits taken: 59, 208 damage. Health at the end 100.


## Swordsman #2

Present 30.0–668.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 59 (0 on the part aimed at, 43 blocked, 0 parried, 0 on someone in the way, 0 on allies), 208 damage. Hits taken: 58, 128 damage. Health at the end 100.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

80905 frames in 2105.4 s (38 fps on average); 40941 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 9.5 ms, 95% 17.2, 99% 20.0, worst 841568.8. 10 hitches over 40 ms (9 over 100 ms). 5021 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 11.9
- RecorderSample: 7.4
- RecorderFlush: 5.0
- Sandbox: 2.8
- CaptureSave: 1.1
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

- 467.81 s: 841569 ms frame, 0 physics steps, garbage collected (gen0 7, gen1 7, gen2 7); ours: nothing over 0.5 ms; Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.4 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 260.47 s: 234823 ms frame, 1 physics steps, garbage collected (gen0 3, gen1 3, gen2 3); ours: Sandbox 2.5 ms (1), UnitStep 0.9 ms (2); Unity: Main_Thread 0.3 ms, Gfx_WaitForGfxCommandsFromMainThread 0.2 ms, CPU_Total_Frame_Time 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 506.78 s: 115693 ms frame, 1 physics steps, garbage collected (gen0 3, gen1 3, gen2 3); ours: UnitStep 0.6 ms (2); Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.3 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 404.58 s: 77953 ms frame, 0 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: nothing over 0.5 ms; Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.3 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 427.93 s: 77771 ms frame, 1 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: UnitStep 0.7 ms (2); Unity: Main_Thread 1.4 ms, Gfx_WaitForGfxCommandsFromMainThread 1.2 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 643.60 s: 74732 ms frame, 0 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: nothing over 0.5 ms; Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.3 ms, CPU_Total_Frame_Time 0.1 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms
- 509.37 s: 15427 ms frame, 0 physics steps, garbage collected (gen0 7, gen1 7, gen2 7); ours: nothing over 0.5 ms; Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.4 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms
- 0.05 s: 228 ms frame, 0 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: CaptureRender 1.3 ms (1); Unity: PlayerLoop 41.3 ms, Gfx_WaitForGfxCommandsFromMainThread 40.4 ms, Physics_Simulate 35.5 ms, GC_Collect 11.6 ms, Loading_ReadObject 4.8 ms, Update_ScriptRunBehaviourUpdate 2.4 ms, BehaviourUpdate 2.4 ms, CPU_Total_Frame_Time 1.3 ms
- 28.71 s: 107 ms frame, 1 physics steps; ours: CaptureSave 2.1 ms (1), UnitStep 0.5 ms (2); Unity: Main_Thread 111.4 ms, PlayerLoop 109.4 ms, Gfx_WaitForGfxCommandsFromMainThread 108.1 ms, GC_Collect 12.0 ms, CPU_Total_Frame_Time 5.7 ms, CPU_Main_Thread_Frame_Time 5.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.1 ms, FixedBehaviourUpdate 3.1 ms
- 0.10 s: 82 ms frame, 3 physics steps; ours: UnitStep 29.0 ms (6), RecorderSample 15.2 ms (3), Sandbox 9.3 ms (3); Unity: Main_Thread 83.0 ms, Gfx_WaitForGfxCommandsFromMainThread 77.6 ms, PlayerLoop 74.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 63.8 ms, FixedBehaviourUpdate 63.8 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 3.1 ms, LateBehaviourUpdate 3.1 ms, Physics_Simulate 2.4 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

