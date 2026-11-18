# Playtest 2026-11-18_10-59-53

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 155.9 s of play (9354 physics steps), 269 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-18_10-59-53`.

## Target

Present 2.5–35.1 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Present 2.5–35.1 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Present 35.1–158.3 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 10 (0 on the part aimed at, 2 blocked, 0 parried, 0 on someone in the way, 0 on allies), 51 damage. Hits taken: 6, 2 damage. Health at the end 100.


## Swordsman #2

Present 35.1–158.3 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 6 (0 on the part aimed at, 5 blocked, 0 parried, 0 on someone in the way, 0 on allies), 2 damage. Hits taken: 10, 51 damage. Health at the end 100.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

21816 frames in 383.7 s (57 fps on average); 12490 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 10.5 ms, 95% 18.7, 99% 23.1, worst 179016.3. 14 hitches over 40 ms (6 over 100 ms). 1410 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 15.3
- RecorderSample: 9.9
- RecorderFlush: 5.9
- CaptureSave: 3.2
- Sandbox: 3.1
- ArmSolve: 0.0
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

- 95.64 s: 179016 ms frame, 1 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: UnitStep 0.8 ms (2), RecorderSample 0.8 ms (1), RecorderFlush 0.6 ms (1); Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.3 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms
- 111.70 s: 3646 ms frame, 0 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: nothing over 0.5 ms; Unity: Main_Thread 0.5 ms, Gfx_WaitForGfxCommandsFromMainThread 0.5 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms
- 0.05 s: 247 ms frame, 0 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: CaptureRender 1.3 ms (1); Unity: Physics_Simulate 39.3 ms, PlayerLoop 36.0 ms, Gfx_WaitForGfxCommandsFromMainThread 34.1 ms, GC_Collect 10.2 ms, Update_ScriptRunBehaviourUpdate 3.5 ms, BehaviourUpdate 3.5 ms, CPU_Total_Frame_Time 2.4 ms, CPU_Main_Thread_Frame_Time 1.2 ms
- 109.48 s: 112 ms frame, 1 physics steps; ours: UnitStep 0.6 ms (2); Unity: CPU_Total_Frame_Time 14.9 ms, CPU_Main_Thread_Frame_Time 12.5 ms, Main_Thread 11.9 ms, PlayerLoop 9.4 ms, Gfx_WaitForGfxCommandsFromMainThread 8.2 ms, CPU_Render_Thread_Frame_Time 3.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.9 ms, FixedBehaviourUpdate 2.9 ms
- 109.45 s: 112 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 0.7 ms (2); Unity: Main_Thread 11.6 ms, CPU_Total_Frame_Time 9.8 ms, PlayerLoop 9.6 ms, Gfx_WaitForGfxCommandsFromMainThread 8.2 ms, CPU_Main_Thread_Frame_Time 7.7 ms, CPU_Render_Thread_Frame_Time 3.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.9 ms, FixedBehaviourUpdate 2.9 ms
- 32.68 s: 109 ms frame, 1 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: nothing over 0.5 ms; Unity: Main_Thread 107.7 ms, PlayerLoop 105.6 ms, Gfx_WaitForGfxCommandsFromMainThread 103.8 ms, GC_Collect 10.6 ms, CPU_Total_Frame_Time 6.3 ms, CPU_Main_Thread_Frame_Time 5.4 ms, CPU_Render_Thread_Frame_Time 2.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.2 ms
- 0.10 s: 88 ms frame, 3 physics steps; ours: UnitStep 37.8 ms (6), RecorderSample 11.3 ms (3), Sandbox 9.4 ms (3); Unity: Main_Thread 91.8 ms, Gfx_WaitForGfxCommandsFromMainThread 86.0 ms, PlayerLoop 80.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 69.7 ms, FixedBehaviourUpdate 69.7 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 2.9 ms, LateBehaviourUpdate 2.9 ms, Physics_Simulate 2.6 ms
- 26.06 s: 73 ms frame, 1 physics steps; ours: UnitStep 0.6 ms (2); Unity: Main_Thread 24.3 ms, Gfx_WaitForGfxCommandsFromMainThread 20.5 ms, CPU_Total_Frame_Time 16.3 ms, PlayerLoop 10.7 ms, CPU_Main_Thread_Frame_Time 8.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.4 ms, FixedBehaviourUpdate 3.4 ms, CPU_Render_Thread_Frame_Time 3.3 ms
- 121.26 s: 70 ms frame, 0 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: nothing over 0.5 ms; Unity: Main_Thread 10.4 ms, PlayerLoop 8.8 ms, CPU_Total_Frame_Time 8.3 ms, Gfx_WaitForGfxCommandsFromMainThread 7.4 ms, CPU_Main_Thread_Frame_Time 6.0 ms, CPU_Render_Thread_Frame_Time 3.5 ms, GC_Collect 3.0 ms, GPU_Frame_Time 1.0 ms
- 92.96 s: 69 ms frame, 1 physics steps; ours: RecorderSample 0.8 ms (1), UnitStep 0.7 ms (2), RecorderFlush 0.7 ms (1); Unity: CPU_Total_Frame_Time 17.2 ms, CPU_Main_Thread_Frame_Time 15.1 ms, Main_Thread 12.5 ms, PlayerLoop 10.2 ms, Gfx_WaitForGfxCommandsFromMainThread 8.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.6 ms, FixedBehaviourUpdate 3.6 ms, CPU_Render_Thread_Frame_Time 3.2 ms
- 141.36 s: 48 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: CPU_Total_Frame_Time 12.0 ms, CPU_Main_Thread_Frame_Time 10.1 ms, Main_Thread 8.6 ms, PlayerLoop 6.2 ms, Gfx_WaitForGfxCommandsFromMainThread 5.2 ms, CPU_Render_Thread_Frame_Time 2.8 ms, GPU_Frame_Time 1.0 ms, GC_Alloc 0.3 ms
- 32.92 s: 48 ms frame, 1 physics steps; ours: UnitStep 0.6 ms (2); Unity: Main_Thread 10.8 ms, CPU_Total_Frame_Time 10.5 ms, CPU_Main_Thread_Frame_Time 8.8 ms, PlayerLoop 8.7 ms, Gfx_WaitForGfxCommandsFromMainThread 6.8 ms, CPU_Render_Thread_Frame_Time 3.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.6 ms, FixedBehaviourUpdate 2.6 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

