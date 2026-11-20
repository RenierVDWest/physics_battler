# Playtest 2026-11-20_22-28-11

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 255.9 s of play (15355 physics steps), 310 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-20_22-28-11`.

## Target

Present 2.6–258.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 47, 402 damage. Health at the end 100.


## Swordsman

Present 2.6–258.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 47 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 402 damage. Hits taken: 0, 0 damage. Health at the end 0.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

32300 frames in 295.3 s (109 fps on average); 16973 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 10.1 ms, 95% 17.3, 99% 19.7, worst 241.0. 3 hitches over 40 ms (1 over 100 ms). 2232 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 33.2
- RecorderSample: 16.5
- RecorderFlush: 9.8
- CaptureSave: 7.0
- Sandbox: 5.7
- CaptureRender: 0.1
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

- 0.05 s: 241 ms frame, 0 physics steps, garbage collected (gen0 3, gen1 3, gen2 3); ours: CaptureRender 1.3 ms (1); Unity: PlayerLoop 38.3 ms, Physics_Simulate 33.6 ms, Gfx_WaitForGfxCommandsFromMainThread 33.5 ms, GC_Collect 10.5 ms, Update_ScriptRunBehaviourUpdate 2.5 ms, BehaviourUpdate 2.5 ms, CPU_Total_Frame_Time 1.4 ms, CPU_Main_Thread_Frame_Time 0.9 ms
- 0.10 s: 90 ms frame, 3 physics steps; ours: UnitStep 29.9 ms (6), RecorderSample 19.5 ms (3), Sandbox 12.2 ms (3); Unity: Main_Thread 92.3 ms, Gfx_WaitForGfxCommandsFromMainThread 85.9 ms, PlayerLoop 83.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 72.0 ms, FixedBehaviourUpdate 72.0 ms, GC_Collect 5.3 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 2.7 ms, LateBehaviourUpdate 2.7 ms
- 134.84 s: 48 ms frame, 1 physics steps; ours: UnitStep 0.6 ms (2); Unity: Main_Thread 11.5 ms, PlayerLoop 9.9 ms, CPU_Total_Frame_Time 8.1 ms, Gfx_WaitForGfxCommandsFromMainThread 7.2 ms, CPU_Main_Thread_Frame_Time 6.8 ms, CPU_Render_Thread_Frame_Time 3.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.6 ms, FixedBehaviourUpdate 2.6 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

