# Playtest 2026-11-11_20-08-25

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 12.7 s of play (764 physics steps), 2635 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-11_20-08-25`.

## A Medium

Present 2.0–14.7 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 4 (0 on the part aimed at, 0 blocked, 1 parried, 0 on someone in the way, 0 on allies), 17 damage. Hits taken: 2, 0 damage. Health at the end 0.


## A Medium 2

Present 2.0–14.7 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 1 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 16 damage. Hits taken: 1, 0 damage. Health at the end 0.


## A Large

Present 2.0–14.7 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 3 (0 on the part aimed at, 0 blocked, 1 parried, 0 on someone in the way, 0 on allies), 45 damage. Hits taken: 2, 18 damage. Health at the end 0.


## B Medium

Present 2.0–14.7 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 1 (0 on the part aimed at, 0 blocked, 1 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 1, 16 damage. Health at the end 0.


## B Medium 2

Present 2.0–14.7 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 2 (0 on the part aimed at, 0 blocked, 1 parried, 0 on someone in the way, 0 on allies), 18 damage. Hits taken: 1, 36 damage. Health at the end 0.


## B Large

Present 2.0–14.7 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 2 (0 on the part aimed at, 2 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 6, 26 damage. Health at the end 194.

## Defence (milestone 11)

- Medium: pressed 9 s, 45% of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 1 of 3 bursts at it answered with a counter, 33% (gate 30–50%), 0 of them ripostes between links.
- Large: pressed 7 s, 66% of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 1 of 4 bursts at it answered with a counter, 25% (gate 30–50%), 0 of them ripostes between links.

## Performance

2566 frames in 13.1 s (196 fps on average); 1812 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 10.9 ms, 95% 16.4, 99% 27.0, worst 337.4. 4 hitches over 40 ms (2 over 100 ms). 14 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 197.6
- ArmSolve: 134.2
- RecorderSample: 54.2
- RecorderFlush: 32.2
- CaptureSave: 6.5
- Sandbox: 5.2
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

- 0.05 s: 337 ms frame, 0 physics steps, garbage collected (gen0 4, gen1 4, gen2 4); ours: nothing over 0.5 ms; Unity: Physics_Simulate 28.7 ms, GC_Collect 25.4 ms, Gfx_WaitForGfxCommandsFromMainThread 4.9 ms, PlayerLoop 4.6 ms, CPU_Total_Frame_Time 2.1 ms, CPU_Main_Thread_Frame_Time 1.5 ms, CPU_Render_Thread_Frame_Time 0.7 ms, GC_Alloc 0.4 ms
- 0.10 s: 116 ms frame, 3 physics steps; ours: UnitStep 52.3 ms (18), RecorderSample 28.4 ms (3), ArmSolve 19.8 ms (32), Sandbox 5.9 ms (3), CaptureRender 1.4 ms (1); Unity: Main_Thread 120.2 ms, Gfx_WaitForGfxCommandsFromMainThread 117.5 ms, PlayerLoop 106.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 98.5 ms, FixedBehaviourUpdate 98.5 ms, Physics_Simulate 4.2 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 2.4 ms, LateBehaviourUpdate 2.4 ms
- 2.96 s: 43 ms frame, 1 physics steps; ours: Sandbox 16.1 ms (1), UnitStep 10.3 ms (6), RecorderSample 9.8 ms (1), ArmSolve 2.3 ms (4), RecorderFlush 0.6 ms (1); Unity: Main_Thread 46.3 ms, PlayerLoop 44.4 ms, Gfx_WaitForGfxCommandsFromMainThread 44.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 39.1 ms, FixedBehaviourUpdate 39.1 ms, CPU_Total_Frame_Time 15.4 ms, CPU_Main_Thread_Frame_Time 13.9 ms, GC_Collect 3.7 ms
- 4.55 s: 42 ms frame, 1 physics steps; ours: UnitStep 3.8 ms (6), ArmSolve 2.8 ms (8); Unity: CPU_Total_Frame_Time 10.3 ms, Main_Thread 9.8 ms, CPU_Main_Thread_Frame_Time 9.3 ms, PlayerLoop 8.9 ms, Gfx_WaitForGfxCommandsFromMainThread 7.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 7.3 ms, FixedBehaviourUpdate 7.3 ms, Physics_Simulate 2.0 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

