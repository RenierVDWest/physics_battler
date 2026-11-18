# Playtest 2026-11-18_14-06-46

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 188.0 s of play (11282 physics steps), 431 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-18_14-06-46`.

## Target

Present 5.1–47.8 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Present 5.1–47.8 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Present 47.8–193.2 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 10 (0 on the part aimed at, 6 blocked, 0 parried, 0 on someone in the way, 0 on allies), 41 damage. Hits taken: 5, 3 damage. Health at the end 100.


## Swordsman #2

Present 47.8–193.2 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 5 (0 on the part aimed at, 4 blocked, 0 parried, 0 on someone in the way, 0 on allies), 3 damage. Hits taken: 10, 41 damage. Health at the end 100.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

22446 frames in 244.6 s (92 fps on average); 11371 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 11.6 ms, 95% 21.2, 99% 41.8, worst 3894.3. 123 hitches over 40 ms (14 over 100 ms). 1586 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 28.1
- RecorderSample: 17.2
- RecorderFlush: 11.0
- CaptureSave: 6.9
- Sandbox: 5.6
- ArmSolve: 0.2
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

- 60.49 s: 3894 ms frame, 1 physics steps, garbage collected (gen0 3, gen1 3, gen2 3); ours: RecorderSample 0.6 ms (1), UnitStep 0.6 ms (2), RecorderFlush 0.5 ms (1); Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.3 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 0.05 s: 270 ms frame, 0 physics steps, garbage collected (gen0 3, gen1 3, gen2 3); ours: CaptureRender 1.3 ms (1); Unity: PlayerLoop 41.9 ms, Physics_Simulate 35.6 ms, Gfx_WaitForGfxCommandsFromMainThread 32.5 ms, GC_Collect 10.0 ms, Update_ScriptRunBehaviourUpdate 2.1 ms, BehaviourUpdate 2.1 ms, CPU_Total_Frame_Time 1.4 ms, CPU_Main_Thread_Frame_Time 0.9 ms
- 5.05 s: 153 ms frame, 1 physics steps; ours: UnitStep 0.6 ms (2), RecorderSample 0.6 ms (1); Unity: CPU_Total_Frame_Time 19.4 ms, CPU_Main_Thread_Frame_Time 15.7 ms, Main_Thread 10.5 ms, PlayerLoop 8.6 ms, Gfx_WaitForGfxCommandsFromMainThread 6.8 ms, CPU_Render_Thread_Frame_Time 3.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.8 ms, FixedBehaviourUpdate 2.8 ms
- 9.78 s: 144 ms frame, 1 physics steps; ours: UnitStep 0.6 ms (2); Unity: CPU_Total_Frame_Time 10.2 ms, Main_Thread 9.5 ms, CPU_Main_Thread_Frame_Time 8.6 ms, PlayerLoop 8.4 ms, Gfx_WaitForGfxCommandsFromMainThread 6.2 ms, CPU_Render_Thread_Frame_Time 2.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.6 ms, FixedBehaviourUpdate 2.6 ms
- 0.10 s: 130 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 37.9 ms (6), RecorderSample 14.8 ms (3), Sandbox 9.4 ms (3); Unity: Main_Thread 104.4 ms, Gfx_WaitForGfxCommandsFromMainThread 93.9 ms, PlayerLoop 90.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 72.6 ms, FixedBehaviourUpdate 72.6 ms, Loading_ReadObject 5.6 ms, GC_Collect 5.6 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 3.0 ms
- 95.82 s: 119 ms frame, 2 physics steps; ours: UnitStep 1.1 ms (4), RecorderSample 0.8 ms (2), RecorderFlush 0.5 ms (2); Unity: Main_Thread 32.8 ms, Gfx_WaitForGfxCommandsFromMainThread 28.6 ms, PlayerLoop 15.5 ms, CPU_Total_Frame_Time 12.5 ms, CPU_Main_Thread_Frame_Time 9.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 5.7 ms, FixedBehaviourUpdate 5.7 ms, CPU_Render_Thread_Frame_Time 3.2 ms
- 2.23 s: 116 ms frame, 2 physics steps; ours: UnitStep 1.1 ms (4), RecorderSample 0.7 ms (2); Unity: Main_Thread 38.9 ms, Gfx_WaitForGfxCommandsFromMainThread 35.2 ms, CPU_Total_Frame_Time 13.5 ms, PlayerLoop 11.8 ms, CPU_Main_Thread_Frame_Time 11.8 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 5.2 ms, FixedBehaviourUpdate 5.2 ms, CPU_Render_Thread_Frame_Time 2.9 ms
- 178.51 s: 113 ms frame, 1 physics steps; ours: UnitStep 0.6 ms (2), RecorderSample 0.6 ms (1); Unity: Main_Thread 56.7 ms, Gfx_WaitForGfxCommandsFromMainThread 53.3 ms, PlayerLoop 9.9 ms, CPU_Total_Frame_Time 7.6 ms, CPU_Main_Thread_Frame_Time 5.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.2 ms, FixedBehaviourUpdate 3.2 ms, CPU_Render_Thread_Frame_Time 2.7 ms
- 43.83 s: 111 ms frame, 0 physics steps; ours: CaptureSave 1.8 ms (1); Unity: Main_Thread 9.8 ms, PlayerLoop 8.2 ms, CPU_Total_Frame_Time 7.6 ms, Gfx_WaitForGfxCommandsFromMainThread 6.6 ms, CPU_Main_Thread_Frame_Time 6.3 ms, CPU_Render_Thread_Frame_Time 3.1 ms, GPU_Frame_Time 2.3 ms, GC_Alloc 0.2 ms
- 42.69 s: 111 ms frame, 1 physics steps; ours: UnitStep 0.5 ms (2); Unity: Main_Thread 107.8 ms, PlayerLoop 104.3 ms, Gfx_WaitForGfxCommandsFromMainThread 103.5 ms, GC_Collect 11.3 ms, CPU_Total_Frame_Time 6.4 ms, CPU_Main_Thread_Frame_Time 5.5 ms, CPU_Render_Thread_Frame_Time 2.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.4 ms
- 58.89 s: 110 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: CPU_Total_Frame_Time 12.1 ms, CPU_Main_Thread_Frame_Time 10.7 ms, Main_Thread 9.8 ms, Gfx_WaitForGfxCommandsFromMainThread 6.4 ms, PlayerLoop 6.0 ms, CPU_Render_Thread_Frame_Time 2.8 ms, GPU_Frame_Time 1.1 ms, GC_Alloc 0.3 ms
- 46.92 s: 106 ms frame, 2 physics steps; ours: UnitStep 1.1 ms (4); Unity: Main_Thread 23.9 ms, Gfx_WaitForGfxCommandsFromMainThread 20.4 ms, PlayerLoop 11.3 ms, CPU_Total_Frame_Time 10.2 ms, CPU_Main_Thread_Frame_Time 8.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 4.8 ms, FixedBehaviourUpdate 4.8 ms, CPU_Render_Thread_Frame_Time 2.8 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

