# Playtest 2026-11-06_13-16-19

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 44.3 s of play (2659 physics steps), 1163 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-06_13-16-19`.

## A Medium

Present 2.6–46.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 9 (0 on the part aimed at, 7 blocked, 0 parried, 0 on someone in the way, 0 on allies), 16 damage. Hits taken: 5, 119 damage. Killed.


## A Medium 2

Present 2.6–46.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 5 (0 on the part aimed at, 0 blocked, 4 parried, 0 on someone in the way, 0 on allies), 34 damage. Hits taken: 24, 109 damage. Health at the end 0.


## A Large

Present 2.6–46.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 6 (0 on the part aimed at, 1 blocked, 0 parried, 0 on someone in the way, 0 on allies), 116 damage. Hits taken: 23, 238 damage. Killed.


## B Medium

Present 2.6–46.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 8 (0 on the part aimed at, 0 blocked, 3 parried, 0 on someone in the way, 0 on allies), 139 damage. Hits taken: 9, 16 damage. Health at the end 0.


## B Medium 2

Present 2.6–46.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 26 (0 on the part aimed at, 6 blocked, 3 parried, 5 on someone in the way, 0 on allies), 185 damage. Hits taken: 6, 109 damage. Health at the end 0.


## B Large

Present 2.6–46.9 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 20 (0 on the part aimed at, 9 blocked, 2 parried, 2 on someone in the way, 2 on allies), 173 damage. Hits taken: 7, 72 damage. Health at the end 0.

## Defence (milestone 11)

- Medium: pressed 28 s, 64% of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 5 of 11 bursts at it answered with a counter, 45% (gate 30–50%), 0 of them ripostes between links.
- Large: pressed 20 s, 78% of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 3 of 7 bursts at it answered with a counter, 43% (gate 30–50%), 0 of them ripostes between links.

## Performance

21325 frames in 44.6 s (478 fps on average); 18671 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 5.5 ms, 95% 10.0, 99% 13.9, worst 258.0. 2 hitches over 40 ms (2 over 100 ms). 117 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 151.9
- ArmSolve: 101.6
- RecorderSample: 48.8
- RecorderFlush: 28.4
- Sandbox: 3.7
- CaptureSave: 2.1
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

- 0.05 s: 258 ms frame, 0 physics steps, garbage collected (gen0 6, gen1 6, gen2 6); ours: nothing over 0.5 ms; Unity: GC_Collect 25.7 ms, Gfx_WaitForGfxCommandsFromMainThread 4.4 ms, PlayerLoop 4.4 ms, CPU_Total_Frame_Time 1.3 ms, CPU_Main_Thread_Frame_Time 0.8 ms, CPU_Render_Thread_Frame_Time 0.5 ms, GC_Alloc 0.4 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 0.2 ms
- 0.10 s: 104 ms frame, 3 physics steps; ours: RecorderSample 42.7 ms (3), UnitStep 36.2 ms (18), ArmSolve 19.3 ms (32), Sandbox 5.8 ms (3), CaptureRender 1.0 ms (1); Unity: Main_Thread 107.4 ms, Gfx_WaitForGfxCommandsFromMainThread 105.1 ms, PlayerLoop 97.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 87.9 ms, FixedBehaviourUpdate 87.9 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 3.0 ms, LateBehaviourUpdate 3.0 ms, Update_ScriptRunBehaviourUpdate 1.7 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

