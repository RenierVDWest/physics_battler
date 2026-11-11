# Playtest 2026-11-11_20-07-45

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 17.8 s of play (1066 physics steps), 8239 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-11_20-07-45`.

## A Medium

Present 3.5–21.2 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.0 | 0 / 0 |
| Chest | Pinned | 0.0 / 0.0 | 0 / 0 |
| Head | Pinned | 0.0 / 0.0 | 0 / 0 |
| UpperArmL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ForearmL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| HandL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| UpperArmR | Pinned | 0.0 / 0.0 | 0 / 0 |
| ForearmR | Pinned | 0.0 / 0.0 | 0 / 0 |
| HandR | Pinned | 0.0 / 0.0 | 0 / 0 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 4 (0 on the part aimed at, 0 blocked, 0 parried, 1 on someone in the way, 0 on allies), 85 damage. Hits taken: 3, 20 damage. Health at the end 80.


## A Medium 2

Present 3.5–21.2 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Chest | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Head | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| UpperArmL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ForearmL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| HandL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| UpperArmR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ForearmR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| HandR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 3, 114 damage. Killed.


## A Large

Present 3.5–21.2 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 3 (0 on the part aimed at, 0 blocked, 2 parried, 0 on someone in the way, 0 on allies), 6 damage. Hits taken: 5, 78 damage. Health at the end 142.


## B Medium

Present 3.5–21.2 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 6 (0 on the part aimed at, 0 blocked, 1 parried, 0 on someone in the way, 0 on allies), 78 damage. Hits taken: 6, 58 damage. Health at the end 42.


## B Medium 2

Present 3.5–21.2 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Chest | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Head | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| UpperArmL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ForearmL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| HandL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| UpperArmR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ForearmR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| HandR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 3 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 114 damage. Hits taken: 0, 0 damage. Health at the end 100.


## B Large

Present 3.5–21.2 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 5 (0 on the part aimed at, 1 blocked, 0 parried, 0 on someone in the way, 2 on allies), 38 damage. Hits taken: 4, 51 damage. Health at the end 169.

## Defence (milestone 11)

- Medium: pressed 21 s, 50% of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 1 of 5 bursts at it answered with a counter, 20% (gate 30–50%), 0 of them ripostes between links.
- Large: pressed 6 s, 15% of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 1 of 3 bursts at it answered with a counter, 33% (gate 30–50%), 0 of them ripostes between links.

## Performance

3515 frames in 18.2 s (193 fps on average); 2469 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 10.8 ms, 95% 16.5, 99% 27.6, worst 353.0. 6 hitches over 40 ms (2 over 100 ms). 17 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 177.7
- ArmSolve: 119.7
- RecorderSample: 52.8
- RecorderFlush: 31.1
- CaptureSave: 5.1
- Sandbox: 4.7
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

- 0.05 s: 353 ms frame, 0 physics steps, garbage collected (gen0 4, gen1 4, gen2 4); ours: nothing over 0.5 ms; Unity: Gfx_WaitForGfxCommandsFromMainThread 278.2 ms, Physics_Simulate 28.7 ms, GC_Collect 24.4 ms, PlayerLoop 7.8 ms, CPU_Total_Frame_Time 2.8 ms, CPU_Main_Thread_Frame_Time 2.0 ms, CPU_Render_Thread_Frame_Time 0.6 ms, GPU_Frame_Time 0.5 ms
- 0.10 s: 135 ms frame, 3 physics steps; ours: UnitStep 53.6 ms (18), RecorderSample 40.7 ms (3), ArmSolve 20.2 ms (32), Sandbox 6.1 ms (3), CaptureRender 1.1 ms (1); Unity: Main_Thread 147.5 ms, Gfx_WaitForGfxCommandsFromMainThread 144.5 ms, PlayerLoop 122.1 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 113.9 ms, FixedBehaviourUpdate 113.9 ms, Physics_Simulate 5.0 ms, GC_Collect 4.4 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 2.3 ms
- 15.41 s: 71 ms frame, 1 physics steps; ours: RecorderSample 1.3 ms (1), UnitStep 1.1 ms (6), RecorderFlush 0.9 ms (1); Unity: Main_Thread 16.3 ms, Gfx_WaitForGfxCommandsFromMainThread 14.7 ms, PlayerLoop 12.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 7.7 ms, FixedBehaviourUpdate 7.7 ms, CPU_Total_Frame_Time 4.0 ms, Physics_Simulate 3.9 ms, CPU_Main_Thread_Frame_Time 1.7 ms
- 15.46 s: 59 ms frame, 3 physics steps; ours: UnitStep 2.7 ms (18), RecorderSample 2.6 ms (3), RecorderFlush 1.8 ms (3); Unity: Main_Thread 77.7 ms, Gfx_WaitForGfxCommandsFromMainThread 75.8 ms, PlayerLoop 21.1 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 17.3 ms, FixedBehaviourUpdate 17.3 ms, Physics_Simulate 8.4 ms, CPU_Total_Frame_Time 3.1 ms, CPU_Main_Thread_Frame_Time 1.7 ms
- 13.39 s: 55 ms frame, 2 physics steps; ours: UnitStep 5.2 ms (12), RecorderSample 3.0 ms (2), RecorderFlush 2.2 ms (2), ArmSolve 1.6 ms (10); Unity: Main_Thread 51.9 ms, Gfx_WaitForGfxCommandsFromMainThread 49.3 ms, PlayerLoop 27.7 ms, CPU_Total_Frame_Time 21.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 16.2 ms, FixedBehaviourUpdate 16.2 ms, CPU_Main_Thread_Frame_Time 10.1 ms, Physics_Simulate 5.5 ms
- 16.30 s: 41 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 3.3 ms, PlayerLoop 2.3 ms, CPU_Total_Frame_Time 2.2 ms, CPU_Main_Thread_Frame_Time 1.4 ms, CPU_Render_Thread_Frame_Time 0.6 ms, Gfx_WaitForGfxCommandsFromMainThread 0.1 ms, Update_ScriptRunBehaviourUpdate 0.0 ms, BehaviourUpdate 0.0 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

