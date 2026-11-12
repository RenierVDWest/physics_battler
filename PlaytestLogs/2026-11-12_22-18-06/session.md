# Playtest 2026-11-12_22-18-06

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 135.0 s of play (8100 physics steps), 1154 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-12_22-18-06`.

## Target open

Present 7.9–142.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 47, 539 damage. Health at the end 100.


## Attacker 1

Present 7.9–142.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 47 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 539 damage. Hits taken: 0, 0 damage. Health at the end 100.


## Target shield

Present 7.9–142.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 51, 54 damage. Health at the end 100.


## Attacker 2

Present 7.9–142.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 51 (0 on the part aimed at, 47 blocked, 0 parried, 0 on someone in the way, 0 on allies), 54 damage. Hits taken: 0, 0 damage. Health at the end 100.


## Target sword

Present 7.9–142.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 49, 163 damage. Health at the end 0.


## Attacker 3

Present 7.9–142.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 49 (0 on the part aimed at, 0 blocked, 30 parried, 0 on someone in the way, 0 on allies), 163 damage. Hits taken: 0, 0 damage. Health at the end 0.

## Defence (milestone 11)

- Medium: pressed 61 s, 91% of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 11 bursts at it answered with a counter, 0% (gate 30–50%), 0 of them ripostes between links.

## Performance

48362 frames in 184.2 s (263 fps on average); 40276 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 10.1 ms, 95% 13.8, 99% 23.2, worst 337.3. 7 hitches over 40 ms (2 over 100 ms). 169 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 127.6
- ArmSolve: 66.6
- RecorderSample: 39.1
- RecorderFlush: 23.7
- CaptureSave: 3.0
- Sandbox: 1.6
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

- 0.05 s: 337 ms frame, 0 physics steps, garbage collected (gen0 4, gen1 4, gen2 4); ours: nothing over 0.5 ms; Unity: Physics_Simulate 34.6 ms, GC_Collect 28.5 ms, PlayerLoop 5.2 ms, Gfx_WaitForGfxCommandsFromMainThread 4.6 ms, CPU_Total_Frame_Time 4.3 ms, CPU_Main_Thread_Frame_Time 3.6 ms, CPU_Render_Thread_Frame_Time 0.7 ms, GPU_Frame_Time 0.5 ms
- 0.10 s: 129 ms frame, 3 physics steps; ours: UnitStep 50.2 ms (18), RecorderSample 43.5 ms (3), ArmSolve 15.7 ms (32), Sandbox 4.6 ms (3), CaptureRender 0.8 ms (1); Unity: Main_Thread 131.0 ms, Gfx_WaitForGfxCommandsFromMainThread 128.0 ms, PlayerLoop 119.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 111.9 ms, FixedBehaviourUpdate 111.9 ms, Physics_Simulate 4.3 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 2.3 ms, LateBehaviourUpdate 2.3 ms
- 76.18 s: 84 ms frame, 1 physics steps; ours: UnitStep 2.3 ms (6), RecorderSample 1.1 ms (1), RecorderFlush 0.8 ms (1), ArmSolve 0.6 ms (2); Unity: Main_Thread 62.0 ms, Gfx_WaitForGfxCommandsFromMainThread 60.0 ms, PlayerLoop 23.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 15.0 ms, FixedBehaviourUpdate 15.0 ms, Physics_Simulate 9.8 ms, CPU_Total_Frame_Time 1.9 ms, CPU_Main_Thread_Frame_Time 1.2 ms
- 16.99 s: 66 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: CPU_Total_Frame_Time 10.4 ms, CPU_Main_Thread_Frame_Time 8.8 ms, Main_Thread 3.4 ms, Gfx_WaitForGfxCommandsFromMainThread 2.3 ms, PlayerLoop 1.6 ms, CPU_Render_Thread_Frame_Time 1.0 ms, GPU_Frame_Time 0.7 ms, Physics_Interpolation 0.0 ms
- 106.70 s: 54 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 14.0 ms, Gfx_WaitForGfxCommandsFromMainThread 12.7 ms, PlayerLoop 2.0 ms, CPU_Render_Thread_Frame_Time 1.7 ms, CPU_Total_Frame_Time 1.7 ms, CPU_Main_Thread_Frame_Time 1.1 ms, GPU_Frame_Time 0.6 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 0.0 ms
- 107.19 s: 51 ms frame, 1 physics steps; ours: UnitStep 4.1 ms (6), ArmSolve 2.5 ms (6), RecorderSample 0.8 ms (1); Unity: Main_Thread 39.8 ms, Gfx_WaitForGfxCommandsFromMainThread 38.0 ms, PlayerLoop 18.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 8.3 ms, FixedBehaviourUpdate 8.3 ms, Physics_Simulate 2.1 ms, CPU_Total_Frame_Time 2.0 ms, CPU_Main_Thread_Frame_Time 1.3 ms
- 106.65 s: 41 ms frame, 1 physics steps; ours: UnitStep 3.9 ms (6), ArmSolve 2.6 ms (8), RecorderSample 1.0 ms (1), RecorderFlush 0.7 ms (1); Unity: Main_Thread 11.4 ms, PlayerLoop 10.2 ms, Gfx_WaitForGfxCommandsFromMainThread 10.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 8.1 ms, FixedBehaviourUpdate 8.1 ms, Physics_Simulate 1.9 ms, CPU_Total_Frame_Time 1.7 ms, CPU_Main_Thread_Frame_Time 1.1 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

