# Playtest 2026-11-17_19-10-44

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 192.4 s of play (11544 physics steps), 38 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-17_19-10-44`.

## Target

Present 2.0–194.4 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Present 2.0–194.4 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

30273 frames in 192.7 s (157 fps on average); 18752 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 7.8 ms, 95% 15.5, 99% 19.9, worst 301.5. 5 hitches over 40 ms (2 over 100 ms). 824 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 34.1
- RecorderSample: 19.4
- CaptureSave: 12.9
- RecorderFlush: 10.9
- Sandbox: 5.8
- CaptureRender: 0.1
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

- 0.05 s: 302 ms frame, 0 physics steps, garbage collected (gen0 2, gen1 2, gen2 2); ours: CaptureRender 3.9 ms (1); Unity: Physics_Simulate 43.5 ms, Gfx_WaitForGfxCommandsFromMainThread 28.3 ms, PlayerLoop 26.3 ms, GC_Collect 15.0 ms, Update_ScriptRunBehaviourUpdate 5.2 ms, BehaviourUpdate 5.2 ms, CPU_Total_Frame_Time 1.3 ms, CPU_Main_Thread_Frame_Time 0.9 ms
- 0.10 s: 103 ms frame, 3 physics steps; ours: RecorderSample 31.2 ms (3), UnitStep 28.8 ms (6), Sandbox 9.3 ms (3); Unity: Main_Thread 102.8 ms, Gfx_WaitForGfxCommandsFromMainThread 97.7 ms, PlayerLoop 90.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 80.2 ms, FixedBehaviourUpdate 80.2 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 3.2 ms, LateBehaviourUpdate 3.2 ms, Physics_Simulate 2.6 ms
- 16.59 s: 53 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: Main_Thread 5.8 ms, CPU_Total_Frame_Time 5.2 ms, CPU_Main_Thread_Frame_Time 3.8 ms, PlayerLoop 3.7 ms, Gfx_WaitForGfxCommandsFromMainThread 3.5 ms, CPU_Render_Thread_Frame_Time 1.9 ms, GPU_Frame_Time 0.7 ms, GC_Alloc 0.2 ms
- 165.57 s: 45 ms frame, 0 physics steps; ours: CaptureSave 1.8 ms (1); Unity: Main_Thread 7.3 ms, CPU_Total_Frame_Time 5.5 ms, PlayerLoop 5.3 ms, Gfx_WaitForGfxCommandsFromMainThread 4.7 ms, CPU_Main_Thread_Frame_Time 4.1 ms, CPU_Render_Thread_Frame_Time 1.9 ms, GPU_Frame_Time 0.9 ms, GC_Alloc 0.2 ms
- 167.42 s: 41 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 0.5 ms (2); Unity: Main_Thread 21.3 ms, PlayerLoop 18.9 ms, Gfx_WaitForGfxCommandsFromMainThread 18.4 ms, CPU_Total_Frame_Time 7.6 ms, CPU_Main_Thread_Frame_Time 6.6 ms, GC_Collect 4.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.4 ms, FixedBehaviourUpdate 2.4 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

