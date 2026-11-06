# Playtest 2026-11-06_14-15-22

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 62.3 s of play (3739 physics steps), 838 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-06_14-15-22`.

## A Medium

Present 1.5–63.8 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 25 (0 on the part aimed at, 7 blocked, 8 parried, 0 on someone in the way, 0 on allies), 113 damage. Hits taken: 1, 9 damage. Health at the end 91.


## A Medium 2

Present 1.5–63.8 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 8 (0 on the part aimed at, 0 blocked, 3 parried, 0 on someone in the way, 0 on allies), 172 damage. Hits taken: 10, 16 damage. Health at the end 84.


## A Large

Present 1.5–63.8 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 10 (0 on the part aimed at, 0 blocked, 4 parried, 0 on someone in the way, 0 on allies), 73 damage. Hits taken: 8, 126 damage. Health at the end 94.


## B Medium

Present 1.5–63.8 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 1 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 9 damage. Hits taken: 12, 32 damage. Killed.


## B Medium 2

Present 1.5–63.8 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 5 (0 on the part aimed at, 4 blocked, 0 parried, 0 on someone in the way, 0 on allies), 16 damage. Hits taken: 11, 112 damage. Health at the end 0.


## B Large

Present 1.5–63.8 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 13 (0 on the part aimed at, 5 blocked, 1 parried, 1 on someone in the way, 0 on allies), 126 damage. Hits taken: 20, 213 damage. Killed.

## Defence (milestone 11)

- Medium: pressed 34 s, 61% of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 1 of 5 bursts at it answered with a counter, 20% (gate 30–50%), 0 of them ripostes between links.
- Large: pressed 28 s, 75% of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 2 of 11 bursts at it answered with a counter, 18% (gate 30–50%), 0 of them ripostes between links.

## Performance

24604 frames in 62.6 s (393 fps on average); 20869 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 6.3 ms, 95% 10.7, 99% 15.9, worst 285.9. 2 hitches over 40 ms (2 over 100 ms). 142 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 189.2
- ArmSolve: 145.0
- RecorderSample: 46.8
- RecorderFlush: 26.0
- Sandbox: 3.7
- CaptureSave: 2.9
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

- 0.05 s: 286 ms frame, 0 physics steps, garbage collected (gen0 8, gen1 8, gen2 8); ours: nothing over 0.5 ms; Unity: GC_Collect 27.2 ms, PlayerLoop 4.4 ms, Gfx_WaitForGfxCommandsFromMainThread 3.9 ms, CPU_Total_Frame_Time 1.5 ms, CPU_Main_Thread_Frame_Time 0.8 ms, GPU_Frame_Time 0.5 ms, CPU_Render_Thread_Frame_Time 0.5 ms, GC_Alloc 0.4 ms
- 0.10 s: 105 ms frame, 3 physics steps; ours: RecorderSample 49.1 ms (3), UnitStep 31.0 ms (18), ArmSolve 13.2 ms (32), Sandbox 5.6 ms (3), CaptureRender 1.0 ms (1); Unity: Main_Thread 110.5 ms, Gfx_WaitForGfxCommandsFromMainThread 101.6 ms, PlayerLoop 98.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 88.8 ms, FixedBehaviourUpdate 88.7 ms, GC_Collect 5.2 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 3.2 ms, LateBehaviourUpdate 3.2 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

