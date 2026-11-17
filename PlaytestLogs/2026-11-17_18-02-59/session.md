# Playtest 2026-11-17_18-02-59

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 424.4 s of play (25465 physics steps), 520 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-17_18-02-59`.

## Target

Present 1.9–5.8 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Present 1.9–5.8 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Present 5.8–426.3 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Pinned | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.0 | 0 / 0 |
| Chest | Pinned | 0.0 / 0.0 | 0 / 0 |
| Head | Pinned | 0.0 / 0.0 | 0 / 0 |
| UpperArmL | Pinned | 0.0 / 0.0 | 0 / 0 |
| ForearmL | Pinned | 0.0 / 0.0 | 0 / 0 |
| HandL | Pinned | 0.0 / 0.0 | 0 / 0 |
| UpperArmR | Pinned | 0.0 / 0.0 | 0 / 0 |
| ForearmR | Pinned | 0.0 / 0.0 | 0 / 0 |
| HandR | Pinned | 0.0 / 0.0 | 0 / 0 |
| ThighL | Pinned | 0.0 / 0.0 | 0 / 0 |
| ShinL | Pinned | 0.0 / 0.0 | 0 / 0 |
| FootL | Pinned | 0.0 / 0.0 | 0 / 0 |
| ThighR | Pinned | 0.0 / 0.0 | 0 / 0 |
| ShinR | Pinned | 0.0 / 0.0 | 0 / 0 |
| FootR | Pinned | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 25 (0 on the part aimed at, 20 blocked, 0 parried, 0 on someone in the way, 0 on allies), 30 damage. Hits taken: 20, 25 damage. Health at the end 100.


## Swordsman #2

Present 5.8–426.3 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 20 (0 on the part aimed at, 15 blocked, 0 parried, 0 on someone in the way, 0 on allies), 25 damage. Hits taken: 25, 30 damage. Health at the end 100.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

66734 frames in 424.7 s (157 fps on average); 41303 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 7.9 ms, 95% 15.4, 99% 18.9, worst 261.0. 6 hitches over 40 ms (2 over 100 ms). 1973 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 36.5
- RecorderSample: 23.5
- RecorderFlush: 15.2
- Sandbox: 9.4
- CaptureSave: 7.3
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

- 0.05 s: 261 ms frame, 0 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: CaptureRender 3.5 ms (1); Unity: Physics_Simulate 34.4 ms, Gfx_WaitForGfxCommandsFromMainThread 29.0 ms, PlayerLoop 27.4 ms, GC_Collect 11.6 ms, Update_ScriptRunBehaviourUpdate 4.7 ms, BehaviourUpdate 4.7 ms, CPU_Total_Frame_Time 1.8 ms, CPU_Main_Thread_Frame_Time 1.3 ms
- 3.93 s: 114 ms frame, 1 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 112.5 ms, PlayerLoop 110.5 ms, Gfx_WaitForGfxCommandsFromMainThread 109.5 ms, GC_Collect 10.9 ms, CPU_Total_Frame_Time 7.4 ms, CPU_Main_Thread_Frame_Time 6.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.4 ms, FixedBehaviourUpdate 2.4 ms
- 0.10 s: 78 ms frame, 3 physics steps; ours: UnitStep 27.5 ms (6), RecorderSample 14.9 ms (3), Sandbox 9.2 ms (3); Unity: Main_Thread 80.1 ms, Gfx_WaitForGfxCommandsFromMainThread 75.8 ms, PlayerLoop 72.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 61.7 ms, FixedBehaviourUpdate 61.7 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 2.8 ms, LateBehaviourUpdate 2.8 ms, Physics_Simulate 2.2 ms
- 2.13 s: 61 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: RecorderSample 0.7 ms (1), UnitStep 0.7 ms (2), RecorderFlush 0.6 ms (1); Unity: Main_Thread 20.5 ms, Gfx_WaitForGfxCommandsFromMainThread 17.6 ms, CPU_Total_Frame_Time 9.5 ms, CPU_Main_Thread_Frame_Time 7.7 ms, PlayerLoop 7.5 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.3 ms, FixedBehaviourUpdate 3.3 ms, CPU_Render_Thread_Frame_Time 2.1 ms
- 186.42 s: 55 ms frame, 1 physics steps; ours: UnitStep 0.6 ms (2); Unity: Main_Thread 9.5 ms, PlayerLoop 7.8 ms, Gfx_WaitForGfxCommandsFromMainThread 6.9 ms, CPU_Total_Frame_Time 5.3 ms, CPU_Main_Thread_Frame_Time 3.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.7 ms, FixedBehaviourUpdate 2.7 ms, CPU_Render_Thread_Frame_Time 2.0 ms
- 379.42 s: 47 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: CPU_Total_Frame_Time 9.0 ms, CPU_Main_Thread_Frame_Time 6.8 ms, Main_Thread 4.9 ms, PlayerLoop 3.6 ms, Gfx_WaitForGfxCommandsFromMainThread 2.7 ms, CPU_Render_Thread_Frame_Time 1.8 ms, GPU_Frame_Time 0.9 ms, GC_Alloc 0.1 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

