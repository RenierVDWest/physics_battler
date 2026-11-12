# Playtest 2026-11-12_16-49-44

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 59.1 s of play (3549 physics steps), 664 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-12_16-49-44`.

## Target open

Present 0.2–59.3 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 22, 283 damage. Health at the end 0.


## Attacker 1

Present 0.2–59.3 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 22 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 283 damage. Hits taken: 0, 0 damage. Health at the end 100.


## Target shield

Present 0.2–59.3 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 24, 34 damage. Health at the end 100.


## Attacker 2

Present 0.2–59.3 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 24 (0 on the part aimed at, 23 blocked, 0 parried, 0 on someone in the way, 0 on allies), 34 damage. Hits taken: 0, 0 damage. Health at the end 100.


## Target sword

Present 0.2–59.3 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 24, 51 damage. Health at the end 100.


## Attacker 3

Present 0.2–59.3 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 24 (0 on the part aimed at, 0 blocked, 18 parried, 0 on someone in the way, 0 on allies), 51 damage. Hits taken: 0, 0 damage. Health at the end 0.

## Defence (milestone 11)

- Medium: pressed 30 s, 82% of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 5 bursts at it answered with a counter, 0% (gate 30–50%), 0 of them ripostes between links.

## Performance

39697 frames in 610.9 s (65 fps on average); 36143 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 9.7 ms, 95% 13.4, 99% 26.0, worst 242454.0. 19 hitches over 40 ms (19 over 100 ms). 101 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 15.2
- ArmSolve: 8.1
- RecorderSample: 5.3
- RecorderFlush: 3.2
- CaptureSave: 0.3
- Sandbox: 0.2
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

- 53.93 s: 242454 ms frame, 0 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: nothing over 0.5 ms; Unity: Main_Thread 0.5 ms, Gfx_WaitForGfxCommandsFromMainThread 0.4 ms, CPU_Total_Frame_Time 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 51.72 s: 194251 ms frame, 0 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: nothing over 0.5 ms; Unity: Main_Thread 0.5 ms, Gfx_WaitForGfxCommandsFromMainThread 0.5 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 54.08 s: 41526 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 0.7 ms, Gfx_WaitForGfxCommandsFromMainThread 0.6 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 53.71 s: 6950 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 0.6 ms, Gfx_WaitForGfxCommandsFromMainThread 0.6 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms
- 56.16 s: 3037 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 0.5 ms, Gfx_WaitForGfxCommandsFromMainThread 0.5 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 53.77 s: 981 ms frame, 1 physics steps; ours: UnitStep 1.5 ms (6), RecorderSample 1.0 ms (1), RecorderFlush 0.7 ms (1); Unity: Main_Thread 0.5 ms, Gfx_WaitForGfxCommandsFromMainThread 0.5 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 56.52 s: 884 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.4 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms
- 56.30 s: 839 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 0.4 ms, Gfx_WaitForGfxCommandsFromMainThread 0.4 ms, CPU_Total_Frame_Time 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 51.34 s: 763 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 0.5 ms, Gfx_WaitForGfxCommandsFromMainThread 0.5 ms, CPU_Total_Frame_Time 0.1 ms, PlayerLoop 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms, Gfx_PresentFrame 0.0 ms
- 53.85 s: 762 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 0.5 ms, Gfx_WaitForGfxCommandsFromMainThread 0.5 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 52.22 s: 734 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 0.8 ms, Gfx_WaitForGfxCommandsFromMainThread 0.7 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms
- 51.65 s: 726 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 0.5 ms, Gfx_WaitForGfxCommandsFromMainThread 0.5 ms, CPU_Total_Frame_Time 0.0 ms, PlayerLoop 0.0 ms, CPU_Main_Thread_Frame_Time 0.0 ms, GC_Alloc 0.0 ms, GC_Collect 0.0 ms, WaitForTargetFPS 0.0 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

