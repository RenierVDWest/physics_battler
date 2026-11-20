# Playtest 2026-11-20_13-59-28

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 294.3 s of play (17656 physics steps), 594 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-20_13-59-28`.

## Target

Present 1.5–42.3 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Present 1.5–42.3 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Present 42.3–295.7 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 15 (0 on the part aimed at, 8 blocked, 0 parried, 0 on someone in the way, 0 on allies), 51 damage. Hits taken: 29, 140 damage. Health at the end 100.


## Swordsman #2

Present 42.3–295.7 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 29 (0 on the part aimed at, 15 blocked, 0 parried, 0 on someone in the way, 0 on allies), 140 damage. Hits taken: 15, 51 damage. Health at the end 100.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

24452 frames in 298.6 s (82 fps on average); 7198 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 12.3 ms, 95% 21.3, 99% 27.4, worst 299.5. 27 hitches over 40 ms (7 over 100 ms). 1434 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 49.1
- RecorderSample: 23.9
- RecorderFlush: 14.6
- CaptureSave: 10.7
- Sandbox: 8.2
- ArmSolve: 1.1
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

- 0.30 s: 299 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 70.4 ms, Gfx_WaitForGfxCommandsFromMainThread 66.4 ms, CPU_Total_Frame_Time 10.8 ms, CPU_Main_Thread_Frame_Time 9.7 ms, PlayerLoop 7.0 ms, CPU_Render_Thread_Frame_Time 3.5 ms, GPU_Frame_Time 0.7 ms, GC_Alloc 0.3 ms
- 0.05 s: 259 ms frame, 0 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: CaptureRender 1.3 ms (1); Unity: PlayerLoop 42.1 ms, Physics_Simulate 35.7 ms, Gfx_WaitForGfxCommandsFromMainThread 34.8 ms, GC_Collect 11.6 ms, Update_ScriptRunBehaviourUpdate 2.3 ms, BehaviourUpdate 2.3 ms, CPU_Total_Frame_Time 1.4 ms, CPU_Main_Thread_Frame_Time 1.0 ms
- 210.15 s: 188 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 0.9 ms (2); Unity: Main_Thread 190.6 ms, Gfx_WaitForGfxCommandsFromMainThread 186.2 ms, PlayerLoop 78.6 ms, CPU_Total_Frame_Time 10.5 ms, CPU_Main_Thread_Frame_Time 9.6 ms, GC_Collect 3.7 ms, CPU_Render_Thread_Frame_Time 3.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.3 ms
- 10.02 s: 171 ms frame, 1 physics steps; ours: UnitStep 0.6 ms (2); Unity: Main_Thread 68.8 ms, Gfx_WaitForGfxCommandsFromMainThread 65.0 ms, CPU_Total_Frame_Time 15.0 ms, CPU_Main_Thread_Frame_Time 9.9 ms, PlayerLoop 9.1 ms, CPU_Render_Thread_Frame_Time 3.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.5 ms, FixedBehaviourUpdate 2.5 ms
- 10.07 s: 163 ms frame, 3 physics steps; ours: UnitStep 1.6 ms (6), RecorderSample 1.0 ms (3), RecorderFlush 0.7 ms (3); Unity: Main_Thread 245.7 ms, Gfx_WaitForGfxCommandsFromMainThread 240.7 ms, PlayerLoop 69.4 ms, CPU_Total_Frame_Time 15.9 ms, CPU_Main_Thread_Frame_Time 12.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 7.7 ms, FixedBehaviourUpdate 7.7 ms, CPU_Render_Thread_Frame_Time 3.4 ms
- 40.88 s: 136 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 128.1 ms, PlayerLoop 126.1 ms, Gfx_WaitForGfxCommandsFromMainThread 124.1 ms, GC_Collect 16.6 ms, CPU_Total_Frame_Time 6.9 ms, CPU_Main_Thread_Frame_Time 6.1 ms, CPU_Render_Thread_Frame_Time 3.2 ms, GPU_Frame_Time 1.3 ms
- 0.10 s: 101 ms frame, 3 physics steps; ours: UnitStep 45.0 ms (6), RecorderSample 15.2 ms (3), Sandbox 12.2 ms (3); Unity: Main_Thread 103.2 ms, PlayerLoop 95.3 ms, Gfx_WaitForGfxCommandsFromMainThread 95.1 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 83.2 ms, FixedBehaviourUpdate 83.2 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 2.8 ms, LateBehaviourUpdate 2.8 ms, Physics_Simulate 2.5 ms
- 210.20 s: 81 ms frame, 3 physics steps; ours: UnitStep 2.0 ms (6), RecorderSample 1.0 ms (3), RecorderFlush 0.6 ms (3); Unity: Main_Thread 81.7 ms, Gfx_WaitForGfxCommandsFromMainThread 77.4 ms, PlayerLoop 72.7 ms, CPU_Total_Frame_Time 15.5 ms, CPU_Main_Thread_Frame_Time 12.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 8.5 ms, FixedBehaviourUpdate 8.5 ms, CPU_Render_Thread_Frame_Time 3.9 ms
- 29.93 s: 78 ms frame, 1 physics steps; ours: UnitStep 0.6 ms (2); Unity: Main_Thread 16.9 ms, Gfx_WaitForGfxCommandsFromMainThread 12.8 ms, CPU_Total_Frame_Time 11.7 ms, PlayerLoop 9.7 ms, CPU_Main_Thread_Frame_Time 9.2 ms, CPU_Render_Thread_Frame_Time 3.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.6 ms, FixedBehaviourUpdate 2.6 ms
- 292.46 s: 78 ms frame, 1 physics steps; ours: UnitStep 1.3 ms (2), RecorderSample 0.6 ms (1), RecorderFlush 0.5 ms (1); Unity: Main_Thread 12.4 ms, PlayerLoop 10.5 ms, CPU_Total_Frame_Time 9.0 ms, Gfx_WaitForGfxCommandsFromMainThread 8.1 ms, CPU_Main_Thread_Frame_Time 7.8 ms, CPU_Render_Thread_Frame_Time 4.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.8 ms, FixedBehaviourUpdate 3.8 ms
- 2.22 s: 74 ms frame, 1 physics steps; ours: CaptureSave 1.7 ms (1), UnitStep 0.7 ms (2), RecorderSample 0.6 ms (1); Unity: Main_Thread 20.3 ms, Gfx_WaitForGfxCommandsFromMainThread 16.2 ms, PlayerLoop 12.9 ms, CPU_Total_Frame_Time 12.2 ms, CPU_Main_Thread_Frame_Time 10.4 ms, CPU_Render_Thread_Frame_Time 3.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.6 ms, FixedBehaviourUpdate 3.6 ms
- 293.43 s: 74 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: CPU_Total_Frame_Time 23.2 ms, CPU_Main_Thread_Frame_Time 21.4 ms, Main_Thread 14.6 ms, Gfx_WaitForGfxCommandsFromMainThread 10.9 ms, PlayerLoop 6.6 ms, CPU_Render_Thread_Frame_Time 3.3 ms, GPU_Frame_Time 1.5 ms, GC_Alloc 0.3 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

