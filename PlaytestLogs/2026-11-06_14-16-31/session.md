# Playtest 2026-11-06_14-16-31

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 124.7 s of play (7482 physics steps), 1884 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-06_14-16-31`.

## A Medium

Present 1.8–126.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 19 (0 on the part aimed at, 6 blocked, 5 parried, 1 on someone in the way, 0 on allies), 142 damage. Hits taken: 23, 41 damage. Health at the end 59.


## A Medium 2

Present 1.8–126.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 6 (0 on the part aimed at, 5 blocked, 0 parried, 0 on someone in the way, 0 on allies), 12 damage. Hits taken: 13, 107 damage. Killed.


## A Large

Present 1.8–126.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 38 (0 on the part aimed at, 19 blocked, 10 parried, 0 on someone in the way, 0 on allies), 224 damage. Hits taken: 12, 128 damage. Health at the end 92.


## B Medium

Present 1.8–126.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 4 (0 on the part aimed at, 0 blocked, 2 parried, 0 on someone in the way, 0 on allies), 45 damage. Hits taken: 13, 90 damage. Killed.


## B Medium 2

Present 1.8–126.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 28 (0 on the part aimed at, 5 blocked, 11 parried, 0 on someone in the way, 0 on allies), 203 damage. Hits taken: 33, 60 damage. Health at the end 40.


## B Large

Present 1.8–126.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 15 (0 on the part aimed at, 13 blocked, 1 parried, 0 on someone in the way, 0 on allies), 27 damage. Hits taken: 16, 227 damage. Killed.

## Defence (milestone 11)

- Medium: pressed 71 s, 65% of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 7 of 14 bursts at it answered with a counter, 50% (gate 30–50%), 0 of them ripostes between links.
- Large: pressed 24 s, 60% of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 1 of 7 bursts at it answered with a counter, 14% (gate 30–50%), 0 of them ripostes between links.

## Performance

61573 frames in 124.9 s (493 fps on average); 54093 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 4.6 ms, 95% 9.4, 99% 13.0, worst 240.9. 2 hitches over 40 ms (1 over 100 ms). 316 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 119.4
- ArmSolve: 74.2
- RecorderSample: 44.0
- RecorderFlush: 23.9
- Sandbox: 2.7
- CaptureSave: 0.6
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

- 0.05 s: 241 ms frame, 0 physics steps, garbage collected (gen0 8, gen1 8, gen2 8); ours: nothing over 0.5 ms; Unity: GC_Collect 32.4 ms, PlayerLoop 4.5 ms, Gfx_WaitForGfxCommandsFromMainThread 4.3 ms, CPU_Total_Frame_Time 1.6 ms, CPU_Main_Thread_Frame_Time 1.1 ms, CPU_Render_Thread_Frame_Time 0.7 ms, GPU_Frame_Time 0.5 ms, GC_Alloc 0.4 ms
- 0.10 s: 95 ms frame, 3 physics steps; ours: UnitStep 37.2 ms (18), RecorderSample 30.6 ms (3), ArmSolve 19.3 ms (32), Sandbox 6.0 ms (3), CaptureRender 1.0 ms (1); Unity: Main_Thread 98.6 ms, Gfx_WaitForGfxCommandsFromMainThread 96.3 ms, PlayerLoop 86.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 77.2 ms, FixedBehaviourUpdate 77.2 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 3.1 ms, LateBehaviourUpdate 3.1 ms, CPU_Total_Frame_Time 2.5 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

