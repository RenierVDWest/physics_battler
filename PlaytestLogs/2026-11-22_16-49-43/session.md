# Playtest 2026-11-22_16-49-43

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 52.9 s of play (3175 physics steps), 411 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-22_16-49-43`.

## Target

Present 1.6–54.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 2, 9 damage. Health at the end 100.


## Swordsman

Present 1.6–54.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 2 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 9 damage. Hits taken: 0, 0 damage. Health at the end 100.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

2096 frames in 53.4 s (39 fps on average); 38 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 24.7 ms, 95% 36.1, 99% 44.5, worst 233.7. 42 hitches over 40 ms (3 over 100 ms). 232 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- Clothes: 286.3
- UnitStep: 43.8
- RecorderSample: 17.5
- CaptureSave: 16.2
- RecorderFlush: 9.5
- Sandbox: 5.9
- CaptureRender: 0.2
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

- 0.05 s: 234 ms frame, 0 physics steps, garbage collected (gen0 4, gen1 4, gen2 4); ours: CaptureRender 1.3 ms (1); Unity: PlayerLoop 46.1 ms, Gfx_WaitForGfxCommandsFromMainThread 39.0 ms, Physics_Simulate 32.5 ms, GC_Collect 10.7 ms, Update_ScriptRunBehaviourUpdate 2.5 ms, BehaviourUpdate 2.5 ms, CPU_Total_Frame_Time 1.4 ms, CPU_Main_Thread_Frame_Time 0.9 ms
- 0.10 s: 183 ms frame, 3 physics steps; ours: UnitStep 100.5 ms (6), RecorderSample 14.0 ms (3), Sandbox 12.6 ms (3); Unity: Main_Thread 182.4 ms, PlayerLoop 174.9 ms, Gfx_WaitForGfxCommandsFromMainThread 174.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 137.1 ms, FixedBehaviourUpdate 137.1 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 24.9 ms, LateBehaviourUpdate 24.9 ms, GC_Collect 10.2 ms
- 43.68 s: 163 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 125.0 ms (4), Clothes 8.2 ms (2), CaptureSave 1.7 ms (1); Unity: Main_Thread 162.2 ms, PlayerLoop 161.0 ms, Gfx_WaitForGfxCommandsFromMainThread 156.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 128.5 ms, FixedBehaviourUpdate 128.5 ms, CPU_Total_Frame_Time 30.5 ms, CPU_Main_Thread_Frame_Time 29.2 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 19.2 ms
- 49.54 s: 57 ms frame, 1 physics steps; ours: UnitStep 30.7 ms (2); Unity: Main_Thread 56.0 ms, PlayerLoop 54.9 ms, Gfx_WaitForGfxCommandsFromMainThread 50.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 32.7 ms, FixedBehaviourUpdate 32.7 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 11.5 ms, LateBehaviourUpdate 11.5 ms, CPU_Total_Frame_Time 11.2 ms
- 26.78 s: 55 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 8.2 ms (2), UnitStep 1.2 ms (4); Unity: Main_Thread 55.2 ms, PlayerLoop 53.1 ms, Gfx_WaitForGfxCommandsFromMainThread 50.1 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 30.7 ms, LateBehaviourUpdate 30.7 ms, CPU_Total_Frame_Time 27.4 ms, CPU_Main_Thread_Frame_Time 25.6 ms, GC_Collect 5.4 ms
- 10.52 s: 53 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 7.9 ms (2), UnitStep 0.7 ms (2); Unity: Main_Thread 54.0 ms, PlayerLoop 51.9 ms, Gfx_WaitForGfxCommandsFromMainThread 48.5 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 33.5 ms, LateBehaviourUpdate 33.5 ms, CPU_Total_Frame_Time 31.3 ms, CPU_Main_Thread_Frame_Time 29.4 ms, GC_Collect 4.9 ms
- 31.06 s: 53 ms frame, 3 physics steps; ours: UnitStep 13.4 ms (6), Clothes 7.4 ms (2), CaptureSave 1.7 ms (1), RecorderSample 1.4 ms (3), RecorderFlush 1.1 ms (3), Sandbox 0.9 ms (3); Unity: Main_Thread 53.4 ms, PlayerLoop 50.8 ms, Gfx_WaitForGfxCommandsFromMainThread 47.4 ms, CPU_Total_Frame_Time 28.5 ms, CPU_Main_Thread_Frame_Time 26.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 20.9 ms, FixedBehaviourUpdate 20.9 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 11.1 ms
- 28.14 s: 52 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 7.6 ms (2), UnitStep 1.0 ms (4); Unity: CPU_Total_Frame_Time 30.5 ms, Main_Thread 28.3 ms, CPU_Main_Thread_Frame_Time 27.3 ms, PlayerLoop 26.3 ms, Gfx_WaitForGfxCommandsFromMainThread 22.7 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 8.0 ms, LateBehaviourUpdate 8.0 ms, CPU_Render_Thread_Frame_Time 4.6 ms
- 23.75 s: 52 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 9.9 ms (2), UnitStep 0.7 ms (2); Unity: Main_Thread 52.0 ms, PlayerLoop 48.9 ms, Gfx_WaitForGfxCommandsFromMainThread 46.5 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 25.8 ms, LateBehaviourUpdate 25.8 ms, CPU_Total_Frame_Time 23.2 ms, CPU_Main_Thread_Frame_Time 22.1 ms, GC_Collect 5.7 ms
- 14.67 s: 51 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: Clothes 7.5 ms (2), CaptureSave 1.8 ms (1), UnitStep 1.0 ms (2); Unity: Main_Thread 51.8 ms, PlayerLoop 50.0 ms, Gfx_WaitForGfxCommandsFromMainThread 46.7 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 29.5 ms, LateBehaviourUpdate 29.5 ms, CPU_Total_Frame_Time 23.7 ms, CPU_Main_Thread_Frame_Time 22.7 ms, GC_Collect 4.5 ms
- 30.57 s: 51 ms frame, 2 physics steps; ours: Sandbox 16.1 ms (2), Clothes 8.7 ms (2), UnitStep 7.0 ms (4), CaptureSave 1.7 ms (1); Unity: Main_Thread 50.7 ms, PlayerLoop 48.8 ms, Gfx_WaitForGfxCommandsFromMainThread 45.5 ms, CPU_Total_Frame_Time 27.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 26.7 ms, FixedBehaviourUpdate 26.7 ms, CPU_Main_Thread_Frame_Time 26.2 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 9.5 ms
- 45.02 s: 50 ms frame, 2 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 13.0 ms (4), Clothes 4.0 ms (2), RecorderSample 0.5 ms (2); Unity: Main_Thread 48.9 ms, PlayerLoop 47.4 ms, Gfx_WaitForGfxCommandsFromMainThread 43.5 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 20.2 ms, LateBehaviourUpdate 20.2 ms, CPU_Total_Frame_Time 17.7 ms, CPU_Main_Thread_Frame_Time 16.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 16.7 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

