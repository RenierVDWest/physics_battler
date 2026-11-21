# Playtest 2026-11-21_12-32-15

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 209.8 s of play (12588 physics steps), 662 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-21_12-32-15`.

## Target

Present 1.3–116.6 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 47, 813 damage. Health at the end 0.


## Swordsman

Present 1.3–116.6 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 47 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 813 damage. Hits taken: 0, 0 damage. Health at the end 0.


## Target #2

Present 116.6–211.1 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 12 (0 on the part aimed at, 6 blocked, 0 parried, 0 on someone in the way, 0 on allies), 70 damage. Hits taken: 20, 39 damage. Health at the end 0.


## Swordsman #2

Present 116.6–211.1 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 20 (0 on the part aimed at, 15 blocked, 0 parried, 0 on someone in the way, 0 on allies), 39 damage. Hits taken: 12, 70 damage. Health at the end 0.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

17626 frames in 210.2 s (84 fps on average); 5189 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 11.8 ms, 95% 20.4, 99% 24.3, worst 234.8. 7 hitches over 40 ms (3 over 100 ms). 1473 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 43.2
- RecorderSample: 22.8
- RecorderFlush: 14.6
- CaptureSave: 10.9
- Sandbox: 7.7
- ArmSolve: 0.7
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

- 0.05 s: 235 ms frame, 0 physics steps, garbage collected (gen0 3, gen1 3, gen2 3); ours: CaptureRender 1.3 ms (1); Unity: PlayerLoop 42.3 ms, Gfx_WaitForGfxCommandsFromMainThread 35.5 ms, Physics_Simulate 32.9 ms, GC_Collect 13.1 ms, Update_ScriptRunBehaviourUpdate 2.4 ms, BehaviourUpdate 2.4 ms, CPU_Total_Frame_Time 1.5 ms, CPU_Main_Thread_Frame_Time 1.0 ms
- 156.88 s: 163 ms frame, 1 physics steps; ours: UnitStep 1.0 ms (2), RecorderSample 0.7 ms (1), RecorderFlush 0.6 ms (1); Unity: Main_Thread 24.6 ms, Gfx_WaitForGfxCommandsFromMainThread 19.1 ms, PlayerLoop 15.8 ms, CPU_Total_Frame_Time 13.9 ms, CPU_Main_Thread_Frame_Time 13.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 4.6 ms, FixedBehaviourUpdate 4.6 ms, CPU_Render_Thread_Frame_Time 3.6 ms
- 115.32 s: 118 ms frame, 1 physics steps; ours: UnitStep 0.6 ms (2); Unity: Main_Thread 116.4 ms, PlayerLoop 114.5 ms, Gfx_WaitForGfxCommandsFromMainThread 111.6 ms, GC_Collect 13.8 ms, CPU_Total_Frame_Time 11.5 ms, CPU_Main_Thread_Frame_Time 10.4 ms, CPU_Render_Thread_Frame_Time 3.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.2 ms
- 0.10 s: 86 ms frame, 3 physics steps; ours: UnitStep 31.2 ms (6), RecorderSample 13.3 ms (3), Sandbox 12.3 ms (3); Unity: Main_Thread 87.4 ms, Gfx_WaitForGfxCommandsFromMainThread 81.1 ms, PlayerLoop 79.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 67.1 ms, FixedBehaviourUpdate 67.1 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 2.6 ms, LateBehaviourUpdate 2.6 ms, Physics_Simulate 2.5 ms
- 162.54 s: 80 ms frame, 0 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: nothing over 0.5 ms; Unity: Main_Thread 16.0 ms, Gfx_WaitForGfxCommandsFromMainThread 11.7 ms, CPU_Total_Frame_Time 11.6 ms, PlayerLoop 11.2 ms, CPU_Main_Thread_Frame_Time 10.2 ms, CPU_Render_Thread_Frame_Time 3.6 ms, GC_Collect 3.1 ms, GPU_Frame_Time 1.8 ms
- 68.87 s: 50 ms frame, 1 physics steps; ours: UnitStep 0.6 ms (2); Unity: Main_Thread 19.3 ms, PlayerLoop 17.0 ms, Gfx_WaitForGfxCommandsFromMainThread 15.0 ms, CPU_Total_Frame_Time 11.6 ms, CPU_Main_Thread_Frame_Time 10.4 ms, CPU_Render_Thread_Frame_Time 3.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.9 ms, FixedBehaviourUpdate 2.9 ms
- 11.87 s: 47 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: CPU_Total_Frame_Time 14.6 ms, CPU_Main_Thread_Frame_Time 13.4 ms, Main_Thread 9.2 ms, PlayerLoop 7.6 ms, Gfx_WaitForGfxCommandsFromMainThread 5.1 ms, GPU_Frame_Time 3.8 ms, CPU_Render_Thread_Frame_Time 3.7 ms, GC_Alloc 0.3 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

