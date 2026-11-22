# Playtest 2026-11-22_11-30-05

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 63.1 s of play (3786 physics steps), 7831 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-22_11-30-05`.

## Target

Present 2.6–65.7 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Present 2.6–65.7 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

## Defence (milestone 11)

- Medium: pressed 0 s, n/a of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 0 of 0 bursts at it answered with a counter, n/a (gate 30–50%), 0 of them ripostes between links.

## Performance

4041 frames in 63.7 s (63 fps on average); 488 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 14.6 ms, 95% 24.7, 99% 30.7, worst 282.0. 7 hitches over 40 ms (4 over 100 ms). 405 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 45.1
- RecorderSample: 19.4
- CaptureSave: 14.9
- RecorderFlush: 8.7
- Sandbox: 6.0
- CaptureRender: 0.2
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

- 0.10 s: 282 ms frame, 3 physics steps; ours: UnitStep 210.9 ms (6), RecorderSample 15.4 ms (3), Sandbox 12.3 ms (3); Unity: Main_Thread 283.0 ms, PlayerLoop 274.8 ms, Gfx_WaitForGfxCommandsFromMainThread 274.7 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 248.6 ms, FixedBehaviourUpdate 248.6 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 13.6 ms, LateBehaviourUpdate 13.6 ms, GC_Collect 11.0 ms
- 0.05 s: 249 ms frame, 0 physics steps, garbage collected (gen0 4, gen1 4, gen2 4); ours: CaptureRender 1.5 ms (1); Unity: PlayerLoop 50.2 ms, Gfx_WaitForGfxCommandsFromMainThread 42.6 ms, Physics_Simulate 35.6 ms, GC_Collect 10.1 ms, Update_ScriptRunBehaviourUpdate 2.8 ms, BehaviourUpdate 2.8 ms, CPU_Total_Frame_Time 1.5 ms, CPU_Main_Thread_Frame_Time 1.0 ms
- 17.06 s: 134 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 106.3 ms (2); Unity: Main_Thread 132.6 ms, PlayerLoop 131.2 ms, Gfx_WaitForGfxCommandsFromMainThread 127.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 108.5 ms, FixedBehaviourUpdate 108.5 ms, CPU_Total_Frame_Time 19.5 ms, CPU_Main_Thread_Frame_Time 18.1 ms, GC_Collect 6.0 ms
- 54.42 s: 113 ms frame, 1 physics steps; ours: UnitStep 0.5 ms (2); Unity: CPU_Total_Frame_Time 15.8 ms, CPU_Main_Thread_Frame_Time 14.3 ms, Main_Thread 13.6 ms, PlayerLoop 12.0 ms, Gfx_WaitForGfxCommandsFromMainThread 8.8 ms, CPU_Render_Thread_Frame_Time 4.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.6 ms, FixedBehaviourUpdate 2.6 ms
- 61.38 s: 98 ms frame, 1 physics steps; ours: CaptureSave 1.9 ms (1), UnitStep 0.7 ms (2); Unity: Main_Thread 25.8 ms, Gfx_WaitForGfxCommandsFromMainThread 20.5 ms, PlayerLoop 17.6 ms, CPU_Total_Frame_Time 10.8 ms, CPU_Main_Thread_Frame_Time 9.8 ms, CPU_Render_Thread_Frame_Time 4.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.5 ms, FixedBehaviourUpdate 3.5 ms
- 16.60 s: 43 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: RecorderSample 8.2 ms (1), UnitStep 2.8 ms (2); Unity: Main_Thread 37.2 ms, PlayerLoop 35.2 ms, Gfx_WaitForGfxCommandsFromMainThread 31.0 ms, CPU_Total_Frame_Time 15.2 ms, CPU_Main_Thread_Frame_Time 14.2 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 13.2 ms, FixedBehaviourUpdate 13.2 ms, CPU_Render_Thread_Frame_Time 4.7 ms
- 1.75 s: 41 ms frame, 1 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: UnitStep 0.7 ms (2); Unity: Main_Thread 38.2 ms, PlayerLoop 35.8 ms, Gfx_WaitForGfxCommandsFromMainThread 32.4 ms, CPU_Total_Frame_Time 14.2 ms, CPU_Main_Thread_Frame_Time 12.6 ms, GC_Collect 5.9 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 4.6 ms, LateBehaviourUpdate 4.6 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

## Console warnings and errors

- 2.62 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.62 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.67 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.67 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.70 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.70 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.72 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.72 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.74 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.74 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.76 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.76 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.77 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.77 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.79 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.79 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.80 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.80 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.82 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.82 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.83 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.83 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.84 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.84 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.86 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.86 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.88 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.88 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.89 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- 2.89 s Exception: InvalidOperationException: OutfitJob.Settings.Grid uses unsafe Pointers which is not allowed. Unsafe Pointers can lead to crashes and no safety against race conditions can be provided. If you really need to use unsafe pointers, you can disable this check using [NativeDisableUnsafePtrRestriction].
- … and 7768 more (see events.log)

