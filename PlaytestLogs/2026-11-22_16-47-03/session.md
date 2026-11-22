# Playtest 2026-11-22_16-47-03

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 153.1 s of play (9189 physics steps), 1027 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-22_16-47-03`.

## Target

Present 3.6–90.1 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 12, 259 damage. Health at the end 0.


## Swordsman

Present 3.6–90.1 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 12 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 259 damage. Hits taken: 0, 0 damage. Health at the end 0.


## Target #2

Present 90.1–156.7 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 8 (0 on the part aimed at, 4 blocked, 0 parried, 0 on someone in the way, 0 on allies), 41 damage. Hits taken: 6, 18 damage. Health at the end 0.


## Swordsman #2

Present 90.1–156.7 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 6 (0 on the part aimed at, 4 blocked, 0 parried, 0 on someone in the way, 0 on allies), 18 damage. Hits taken: 8, 41 damage. Health at the end 0.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

5581 frames in 154.1 s (36 fps on average); 13 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 26.3 ms, 95% 37.2, 99% 43.4, worst 260.2. 142 hitches over 40 ms (3 over 100 ms). 594 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- Clothes: 309.8
- UnitStep: 43.4
- RecorderSample: 21.6
- CaptureSave: 15.5
- RecorderFlush: 12.9
- Sandbox: 7.3
- ArmSolve: 0.5
- CaptureRender: 0.2
- Compose: 0.0
- ComposeDry: 0.0
- Plan: 0.0
- Prepare: 0.0
- TryOut: 0.0
- Clearance: 0.0
- Sweep: 0.0
- Authority: 0.0

The worst frames:

- 0.05 s: 260 ms frame, 0 physics steps, garbage collected (gen0 5, gen1 5, gen2 5); ours: CaptureRender 1.3 ms (1); Unity: PlayerLoop 46.1 ms, Gfx_WaitForGfxCommandsFromMainThread 36.8 ms, Physics_Simulate 32.2 ms, GC_Collect 15.3 ms, Update_ScriptRunBehaviourUpdate 2.1 ms, BehaviourUpdate 2.1 ms, CPU_Total_Frame_Time 1.4 ms, CPU_Main_Thread_Frame_Time 0.9 ms
- 0.10 s: 201 ms frame, 3 physics steps; ours: UnitStep 119.4 ms (6), RecorderSample 13.8 ms (3), Sandbox 12.1 ms (3); Unity: Main_Thread 202.5 ms, Gfx_WaitForGfxCommandsFromMainThread 194.0 ms, PlayerLoop 193.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 155.0 ms, FixedBehaviourUpdate 155.0 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 25.6 ms, LateBehaviourUpdate 25.6 ms, GC_Collect 11.5 ms
- 86.61 s: 134 ms frame, 2 physics steps; ours: Clothes 9.8 ms (2), UnitStep 1.1 ms (4); Unity: Main_Thread 133.7 ms, PlayerLoop 130.8 ms, Gfx_WaitForGfxCommandsFromMainThread 128.4 ms, CPU_Total_Frame_Time 24.1 ms, CPU_Main_Thread_Frame_Time 22.5 ms, GC_Collect 15.4 ms, CPU_Render_Thread_Frame_Time 4.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 4.3 ms
- 86.66 s: 63 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 20.5 ms (6), RecorderSample 5.0 ms (3); Unity: Main_Thread 62.5 ms, PlayerLoop 57.1 ms, Gfx_WaitForGfxCommandsFromMainThread 56.4 ms, CPU_Total_Frame_Time 30.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 29.5 ms, FixedBehaviourUpdate 29.5 ms, CPU_Main_Thread_Frame_Time 28.4 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 17.0 ms
- 114.48 s: 57 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 8.9 ms (2), RecorderSample 1.9 ms (2), UnitStep 1.7 ms (4), RecorderFlush 1.4 ms (2); Unity: Main_Thread 57.4 ms, PlayerLoop 51.5 ms, Gfx_WaitForGfxCommandsFromMainThread 50.6 ms, CPU_Total_Frame_Time 32.4 ms, CPU_Main_Thread_Frame_Time 29.9 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 8.9 ms, LateBehaviourUpdate 8.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 7.9 ms
- 103.02 s: 55 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 8.6 ms (2), Sandbox 5.6 ms (3), UnitStep 2.2 ms (6), RecorderSample 0.9 ms (3); Unity: Main_Thread 52.9 ms, PlayerLoop 50.7 ms, Gfx_WaitForGfxCommandsFromMainThread 47.3 ms, CPU_Total_Frame_Time 32.0 ms, CPU_Main_Thread_Frame_Time 29.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 13.3 ms, FixedBehaviourUpdate 13.3 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 8.2 ms
- 115.83 s: 55 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 9.2 ms (2), UnitStep 2.1 ms (4), CaptureSave 1.9 ms (1), RecorderSample 0.9 ms (2), RecorderFlush 0.7 ms (2); Unity: Main_Thread 56.1 ms, PlayerLoop 52.4 ms, Gfx_WaitForGfxCommandsFromMainThread 49.8 ms, CPU_Total_Frame_Time 33.6 ms, CPU_Main_Thread_Frame_Time 31.2 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 12.7 ms, LateBehaviourUpdate 12.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 7.7 ms
- 113.78 s: 54 ms frame, 3 physics steps; ours: Clothes 7.9 ms (2), UnitStep 2.4 ms (6), RecorderSample 1.5 ms (3), RecorderFlush 0.7 ms (3); Unity: Main_Thread 57.8 ms, PlayerLoop 52.1 ms, Gfx_WaitForGfxCommandsFromMainThread 51.4 ms, CPU_Total_Frame_Time 28.7 ms, CPU_Main_Thread_Frame_Time 27.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 9.7 ms, FixedBehaviourUpdate 9.7 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 7.9 ms
- 25.47 s: 54 ms frame, 3 physics steps; ours: Clothes 8.2 ms (2), UnitStep 1.5 ms (6), RecorderSample 0.8 ms (3); Unity: Main_Thread 54.5 ms, PlayerLoop 52.5 ms, Gfx_WaitForGfxCommandsFromMainThread 49.2 ms, CPU_Total_Frame_Time 31.3 ms, CPU_Main_Thread_Frame_Time 27.2 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 9.7 ms, LateBehaviourUpdate 9.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 7.4 ms
- 117.31 s: 53 ms frame, 2 physics steps; ours: CaptureSave 11.5 ms (1), Clothes 10.4 ms (2), RecorderSample 2.0 ms (2), UnitStep 1.7 ms (4), RecorderFlush 1.7 ms (2); Unity: Main_Thread 53.6 ms, PlayerLoop 50.0 ms, Gfx_WaitForGfxCommandsFromMainThread 47.4 ms, CPU_Total_Frame_Time 31.3 ms, CPU_Main_Thread_Frame_Time 29.2 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 9.1 ms, LateBehaviourUpdate 9.1 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 7.4 ms
- 113.98 s: 53 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 7.9 ms (2), CaptureSave 1.9 ms (1), UnitStep 1.9 ms (4), RecorderSample 1.3 ms (2), RecorderFlush 1.1 ms (2); Unity: Main_Thread 52.9 ms, PlayerLoop 50.4 ms, Gfx_WaitForGfxCommandsFromMainThread 46.2 ms, CPU_Total_Frame_Time 45.5 ms, CPU_Main_Thread_Frame_Time 40.3 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 10.7 ms, LateBehaviourUpdate 10.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 7.6 ms
- 135.71 s: 52 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 10.4 ms (2), UnitStep 4.8 ms (4), CaptureSave 1.8 ms (1), RecorderSample 0.7 ms (2); Unity: Main_Thread 51.1 ms, PlayerLoop 49.0 ms, Gfx_WaitForGfxCommandsFromMainThread 45.2 ms, CPU_Total_Frame_Time 33.3 ms, CPU_Main_Thread_Frame_Time 31.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 9.4 ms, FixedBehaviourUpdate 9.4 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 9.1 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

