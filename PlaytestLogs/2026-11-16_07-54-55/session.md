# Playtest 2026-11-16_07-54-55

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 272.0 s of play (16323 physics steps), 73 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-16_07-54-55`.

## Target

Present 1.5–273.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 7, 44 damage. Health at the end 0.


## Swordsman

Present 1.5–273.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 7 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 44 damage. Hits taken: 0, 0 damage. Health at the end 0.

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

68953 frames in 272.2 s (253 fps on average); 52638 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 6.0 ms, 95% 11.3, 99% 14.4, worst 178.4. 3 hitches over 40 ms (1 over 100 ms). 740 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 32.5
- RecorderSample: 19.5
- RecorderFlush: 11.6
- CaptureSave: 6.1
- Sandbox: 1.6
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

- 0.05 s: 178 ms frame, 0 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: CaptureRender 1.2 ms (1); Unity: Physics_Simulate 33.2 ms, Gfx_WaitForGfxCommandsFromMainThread 17.3 ms, PlayerLoop 16.1 ms, GC_Collect 5.3 ms, Update_ScriptRunBehaviourUpdate 2.2 ms, BehaviourUpdate 2.2 ms, CPU_Total_Frame_Time 1.4 ms, CPU_Main_Thread_Frame_Time 0.9 ms
- 0.10 s: 85 ms frame, 3 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 33.6 ms (6), RecorderSample 14.1 ms (3), Sandbox 4.2 ms (3); Unity: Main_Thread 79.6 ms, Gfx_WaitForGfxCommandsFromMainThread 75.9 ms, PlayerLoop 72.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 61.6 ms, FixedBehaviourUpdate 61.6 ms, GC_Collect 3.0 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 2.5 ms, LateBehaviourUpdate 2.5 ms
- 216.87 s: 48 ms frame, 1 physics steps; ours: RecorderSample 0.9 ms (1), RecorderFlush 0.6 ms (1), UnitStep 0.5 ms (2); Unity: Main_Thread 48.5 ms, PlayerLoop 46.5 ms, Gfx_WaitForGfxCommandsFromMainThread 46.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.0 ms, FixedBehaviourUpdate 3.0 ms, CPU_Total_Frame_Time 3.0 ms, CPU_Main_Thread_Frame_Time 2.0 ms, CPU_Render_Thread_Frame_Time 1.1 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

