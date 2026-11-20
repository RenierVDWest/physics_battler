# Playtest 2026-11-20_06-38-46

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 44.4 s of play (2664 physics steps), 147 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-20_06-38-46`.

## Target

Present 1.4–9.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Present 1.4–9.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Present 9.5–45.8 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 1, 0 damage. Health at the end 100.


## Swordsman #2

Present 9.5–45.8 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 1 (0 on the part aimed at, 1 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 0, 0 damage. Health at the end 100.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

8515 frames in 80.3 s (106 fps on average); 5855 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 11.2 ms, 95% 20.6, 99% 25.6, worst 246.2. 8 hitches over 40 ms (2 over 100 ms). 597 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 29.3
- RecorderSample: 16.5
- RecorderFlush: 8.2
- Sandbox: 4.8
- CaptureSave: 3.8
- ArmSolve: 1.6
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

- 0.05 s: 246 ms frame, 0 physics steps, garbage collected (gen0 3, gen1 3, gen2 3); ours: CaptureRender 1.3 ms (1); Unity: PlayerLoop 37.0 ms, Gfx_WaitForGfxCommandsFromMainThread 32.3 ms, Physics_Simulate 31.0 ms, GC_Collect 16.0 ms, Update_ScriptRunBehaviourUpdate 2.4 ms, BehaviourUpdate 2.4 ms, CPU_Total_Frame_Time 1.4 ms, CPU_Main_Thread_Frame_Time 1.0 ms
- 8.13 s: 118 ms frame, 1 physics steps; ours: Sandbox 4.0 ms (1), RecorderSample 0.7 ms (1), UnitStep 0.6 ms (2); Unity: Main_Thread 119.8 ms, PlayerLoop 117.8 ms, Gfx_WaitForGfxCommandsFromMainThread 115.9 ms, GC_Collect 13.1 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 7.0 ms, FixedBehaviourUpdate 7.0 ms, CPU_Total_Frame_Time 6.9 ms, CPU_Main_Thread_Frame_Time 6.1 ms
- 0.10 s: 86 ms frame, 3 physics steps; ours: UnitStep 31.0 ms (6), RecorderSample 14.0 ms (3), Sandbox 10.4 ms (3); Unity: Main_Thread 85.0 ms, Gfx_WaitForGfxCommandsFromMainThread 79.5 ms, PlayerLoop 76.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 65.4 ms, FixedBehaviourUpdate 65.4 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 2.6 ms, LateBehaviourUpdate 2.6 ms, Physics_Simulate 2.3 ms
- 25.20 s: 69 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 46.5 ms, Gfx_WaitForGfxCommandsFromMainThread 42.7 ms, PlayerLoop 8.9 ms, CPU_Total_Frame_Time 7.0 ms, CPU_Main_Thread_Frame_Time 6.2 ms, CPU_Render_Thread_Frame_Time 3.1 ms, GPU_Frame_Time 1.6 ms, GC_Alloc 0.3 ms
- 7.21 s: 68 ms frame, 1 physics steps; ours: UnitStep 0.8 ms (2), RecorderSample 0.6 ms (1), RecorderFlush 0.5 ms (1); Unity: Main_Thread 70.8 ms, Gfx_WaitForGfxCommandsFromMainThread 65.9 ms, PlayerLoop 13.2 ms, CPU_Total_Frame_Time 11.5 ms, CPU_Main_Thread_Frame_Time 9.0 ms, CPU_Render_Thread_Frame_Time 3.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.1 ms, FixedBehaviourUpdate 3.1 ms
- 39.36 s: 56 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 7.5 ms, CPU_Total_Frame_Time 6.8 ms, PlayerLoop 6.1 ms, CPU_Main_Thread_Frame_Time 5.7 ms, Gfx_WaitForGfxCommandsFromMainThread 4.1 ms, CPU_Render_Thread_Frame_Time 3.2 ms, GPU_Frame_Time 2.1 ms, GC_Alloc 0.3 ms
- 38.74 s: 47 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 27.4 ms, Gfx_WaitForGfxCommandsFromMainThread 24.0 ms, CPU_Total_Frame_Time 7.0 ms, CPU_Main_Thread_Frame_Time 6.2 ms, PlayerLoop 5.9 ms, CPU_Render_Thread_Frame_Time 3.3 ms, GPU_Frame_Time 1.7 ms, GC_Alloc 0.3 ms
- 38.76 s: 45 ms frame, 2 physics steps; ours: RecorderSample 1.3 ms (2), UnitStep 1.1 ms (4), RecorderFlush 1.1 ms (2); Unity: Main_Thread 63.7 ms, Gfx_WaitForGfxCommandsFromMainThread 59.6 ms, PlayerLoop 48.1 ms, CPU_Total_Frame_Time 10.3 ms, CPU_Main_Thread_Frame_Time 9.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 6.0 ms, FixedBehaviourUpdate 6.0 ms, CPU_Render_Thread_Frame_Time 3.0 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

