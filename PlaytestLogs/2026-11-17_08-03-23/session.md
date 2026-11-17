# Playtest 2026-11-17_08-03-23

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 163.0 s of play (9782 physics steps), 352 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-17_08-03-23`.

## Target

Present 4.3–7.7 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Present 4.3–7.7 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Present 7.7–167.4 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 4 (0 on the part aimed at, 2 blocked, 0 parried, 0 on someone in the way, 0 on allies), 10 damage. Hits taken: 13, 17 damage. Health at the end 100.


## Swordsman #2

Present 7.7–167.4 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 13 (0 on the part aimed at, 11 blocked, 0 parried, 0 on someone in the way, 0 on allies), 17 damage. Hits taken: 4, 10 damage. Health at the end 100.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

25011 frames in 191.8 s (130 fps on average); 15263 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 8.6 ms, 95% 17.6, 99% 21.5, worst 9158.3. 11 hitches over 40 ms (7 over 100 ms). 546 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 31.6
- RecorderSample: 20.1
- RecorderFlush: 13.4
- Sandbox: 7.6
- CaptureSave: 7.0
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

- 117.69 s: 9158 ms frame, 1 physics steps; ours: UnitStep 0.5 ms (2); Unity: Main_Thread 0.5 ms, Gfx_WaitForGfxCommandsFromMainThread 0.5 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 12.29 s: 3766 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: CaptureSave 1.8 ms (1), UnitStep 0.7 ms (2); Unity: Main_Thread 5.0 ms, PlayerLoop 4.1 ms, Gfx_WaitForGfxCommandsFromMainThread 2.7 ms, GC_Alloc 0.2 ms, CPU_Total_Frame_Time 0.0 ms, Camera_Render 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Collect 0.0 ms
- 16.46 s: 1448 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 0.7 ms (2); Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.4 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms
- 15.72 s: 1204 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.4 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 128.37 s: 1188 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.3 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 16.07 s: 865 ms frame, 0 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: nothing over 0.5 ms; Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.4 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 0.05 s: 196 ms frame, 0 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: CaptureRender 1.0 ms (1); Unity: Physics_Simulate 32.8 ms, PlayerLoop 24.4 ms, Gfx_WaitForGfxCommandsFromMainThread 22.6 ms, GC_Collect 10.5 ms, Update_ScriptRunBehaviourUpdate 1.9 ms, BehaviourUpdate 1.9 ms, CPU_Total_Frame_Time 1.3 ms, CPU_Main_Thread_Frame_Time 0.8 ms
- 16.15 s: 88 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 5.7 ms, CPU_Total_Frame_Time 4.8 ms, PlayerLoop 4.2 ms, CPU_Main_Thread_Frame_Time 3.6 ms, Gfx_WaitForGfxCommandsFromMainThread 3.3 ms, CPU_Render_Thread_Frame_Time 2.2 ms, GPU_Frame_Time 1.7 ms, GC_Alloc 0.2 ms
- 3.37 s: 88 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 87.3 ms, PlayerLoop 84.8 ms, Gfx_WaitForGfxCommandsFromMainThread 84.6 ms, GC_Collect 10.8 ms, CPU_Total_Frame_Time 8.2 ms, CPU_Main_Thread_Frame_Time 6.7 ms, CPU_Render_Thread_Frame_Time 1.9 ms, GPU_Frame_Time 0.9 ms
- 0.10 s: 87 ms frame, 3 physics steps; ours: UnitStep 34.5 ms (6), RecorderSample 14.3 ms (3), Sandbox 8.7 ms (3); Unity: Main_Thread 88.2 ms, Gfx_WaitForGfxCommandsFromMainThread 84.1 ms, PlayerLoop 77.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 67.6 ms, FixedBehaviourUpdate 67.6 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 2.6 ms, LateBehaviourUpdate 2.6 ms, Physics_Simulate 2.4 ms
- 15.80 s: 67 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 5.9 ms, CPU_Total_Frame_Time 4.5 ms, PlayerLoop 4.2 ms, Gfx_WaitForGfxCommandsFromMainThread 3.8 ms, CPU_Main_Thread_Frame_Time 3.7 ms, CPU_Render_Thread_Frame_Time 2.0 ms, GPU_Frame_Time 0.8 ms, GC_Alloc 0.2 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

