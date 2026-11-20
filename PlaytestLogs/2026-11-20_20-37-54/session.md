# Playtest 2026-11-20_20-37-54

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 168.9 s of play (10134 physics steps), 145 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-20_20-37-54`.

## Target

Present 2.1–167.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 10, 106 damage. Health at the end 0.


## Swordsman

Present 2.1–167.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 10 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 106 damage. Hits taken: 0, 0 damage. Health at the end 0.


## Target #2

Present 167.9–171.0 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Present 167.9–171.0 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

25161 frames in 220.7 s (114 fps on average); 15039 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 10.0 ms, 95% 15.8, 99% 19.3, worst 251.2. 5 hitches over 40 ms (2 over 100 ms). 1709 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 29.4
- RecorderSample: 14.6
- RecorderFlush: 8.7
- CaptureSave: 5.1
- Sandbox: 4.8
- CaptureRender: 0.1
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

- 0.05 s: 251 ms frame, 0 physics steps, garbage collected (gen0 4, gen1 4, gen2 4); ours: CaptureRender 1.5 ms (1); Unity: PlayerLoop 44.8 ms, Gfx_WaitForGfxCommandsFromMainThread 37.8 ms, Physics_Simulate 32.4 ms, GC_Collect 10.8 ms, Update_ScriptRunBehaviourUpdate 2.6 ms, BehaviourUpdate 2.6 ms, CPU_Total_Frame_Time 1.3 ms, CPU_Main_Thread_Frame_Time 0.9 ms
- 165.79 s: 100 ms frame, 0 physics steps, garbage collected (gen0 3, gen1 3, gen2 3); ours: nothing over 0.5 ms; Unity: Main_Thread 99.5 ms, PlayerLoop 97.8 ms, Gfx_WaitForGfxCommandsFromMainThread 95.9 ms, CPU_Total_Frame_Time 13.4 ms, CPU_Main_Thread_Frame_Time 12.6 ms, GC_Collect 9.1 ms, CPU_Render_Thread_Frame_Time 3.4 ms, GPU_Frame_Time 1.7 ms
- 0.10 s: 84 ms frame, 3 physics steps; ours: UnitStep 30.9 ms (6), RecorderSample 13.6 ms (3), Sandbox 11.1 ms (3); Unity: Main_Thread 84.5 ms, PlayerLoop 76.5 ms, Gfx_WaitForGfxCommandsFromMainThread 74.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 65.3 ms, FixedBehaviourUpdate 65.3 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 2.7 ms, LateBehaviourUpdate 2.7 ms, Physics_Simulate 2.1 ms
- 135.67 s: 48 ms frame, 1 physics steps; ours: UnitStep 0.7 ms (2); Unity: CPU_Total_Frame_Time 13.6 ms, CPU_Main_Thread_Frame_Time 12.4 ms, Main_Thread 12.3 ms, PlayerLoop 9.4 ms, Gfx_WaitForGfxCommandsFromMainThread 8.6 ms, CPU_Render_Thread_Frame_Time 3.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.8 ms, FixedBehaviourUpdate 2.8 ms
- 154.97 s: 44 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 8.8 ms, CPU_Total_Frame_Time 8.7 ms, CPU_Main_Thread_Frame_Time 7.1 ms, PlayerLoop 6.8 ms, Gfx_WaitForGfxCommandsFromMainThread 5.0 ms, CPU_Render_Thread_Frame_Time 3.5 ms, GPU_Frame_Time 1.6 ms, GC_Alloc 0.3 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

