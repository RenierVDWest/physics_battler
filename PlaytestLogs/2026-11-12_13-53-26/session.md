# Playtest 2026-11-12_13-53-26

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 46.1 s of play (2765 physics steps), 579 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-12_13-53-26`.

## A Medium

Present 1.5–47.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 9 (0 on the part aimed at, 0 blocked, 3 parried, 3 on someone in the way, 0 on allies), 62 damage. Hits taken: 17, 46 damage. Health at the end 54.


## A Medium 2

Present 1.5–47.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 15 (0 on the part aimed at, 9 blocked, 0 parried, 0 on someone in the way, 0 on allies), 52 damage. Hits taken: 12, 56 damage. Health at the end 44.


## A Large

Present 1.5–47.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 9 (0 on the part aimed at, 0 blocked, 3 parried, 0 on someone in the way, 0 on allies), 79 damage. Hits taken: 10, 45 damage. Health at the end 175.


## B Medium

Present 1.5–47.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 12 (0 on the part aimed at, 0 blocked, 6 parried, 0 on someone in the way, 0 on allies), 56 damage. Hits taken: 15, 52 damage. Health at the end 48.


## B Medium 2

Present 1.5–47.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 17 (0 on the part aimed at, 11 blocked, 1 parried, 0 on someone in the way, 1 on allies), 64 damage. Hits taken: 4, 31 damage. Health at the end 69.


## B Large

Present 1.5–47.5 s. Steps 0, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 0 cm from the support point; hips dropped for reach by up to 0 cm.

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

Hits dealt: 11 (0 on the part aimed at, 2 blocked, 5 parried, 0 on someone in the way, 0 on allies), 45 damage. Hits taken: 15, 128 damage. Health at the end 92.

## Defence (milestone 11)

- Medium: pressed 22 s, 64% of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 2 of 4 bursts at it answered with a counter, 50% (gate 30–50%), 0 of them ripostes between links.
- Large: pressed 11 s, 50% of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 0 of 0 blows that met it on its guard (0 blocked, 0 parried, 0 on its guarding arms); 0 guard breaks, 0.0 a minute on its feet (gate about 1 at most); 1 of 4 bursts at it answered with a counter, 25% (gate 30–50%), 0 of them ripostes between links.

## Performance

10595 frames in 46.5 s (228 fps on average); 7841 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 10.5 ms, 95% 15.3, 99% 23.6, worst 341.3. 2 hitches over 40 ms (2 over 100 ms). 39 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 200.1
- ArmSolve: 118.6
- RecorderSample: 55.8
- RecorderFlush: 34.6
- CaptureSave: 5.8
- Sandbox: 3.5
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

- 0.05 s: 341 ms frame, 0 physics steps, garbage collected (gen0 3, gen1 3, gen2 3); ours: nothing over 0.5 ms; Unity: Physics_Simulate 34.5 ms, GC_Collect 23.5 ms, Gfx_WaitForGfxCommandsFromMainThread 5.6 ms, PlayerLoop 5.2 ms, CPU_Total_Frame_Time 1.4 ms, CPU_Main_Thread_Frame_Time 0.9 ms, CPU_Render_Thread_Frame_Time 0.6 ms, GPU_Frame_Time 0.5 ms
- 0.10 s: 146 ms frame, 3 physics steps; ours: UnitStep 60.8 ms (18), RecorderSample 45.3 ms (3), ArmSolve 21.6 ms (32), Sandbox 6.3 ms (3), CaptureRender 1.2 ms (1); Unity: Main_Thread 150.3 ms, Gfx_WaitForGfxCommandsFromMainThread 147.7 ms, PlayerLoop 136.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 127.2 ms, FixedBehaviourUpdate 127.2 ms, Physics_Simulate 4.4 ms, PreLateUpdate_ScriptRunBehaviourLateUpdate 2.4 ms, LateBehaviourUpdate 2.4 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

## Console warnings and errors

- 47.54 s warning: Assembly for Assembly Definition File 'Assets/_Battler/Tests/EditMode/PhysicsBattler.Tests.EditMode.asmdef' will not be compiled, because it has no scripts associated with it.
- 47.54 s warning: Assets\_Battler\Scripts\Runtime\Units\UnitController.cs(66,78): warning CS0067: The event 'UnitController.MoveContact' is never used
- 47.54 s warning: Assets\_Battler\Scripts\Runtime\Units\UnitCombo.cs(44,70): warning CS0067: The event 'UnitController.ComboChanged' is never used
- 47.54 s warning: Assets\_Battler\Scripts\Runtime\Units\UnitCombat.cs(90,22): warning CS0414: The field 'UnitController.hasLastSweep' is assigned but its value is never used
- 47.54 s warning: Assets\_Battler\Scripts\Runtime\Locomotion\ClipFighter.cs(324,55): warning CS0414: The field 'ClipFighter.shieldSpeedNow' is assigned but its value is never used
- 47.54 s warning: Assets\_Battler\Scripts\Runtime\Units\UnitTempo.cs(34,23): warning CS0414: The field 'UnitController.moodBlend' is assigned but its value is never used
- 47.54 s warning: Assets\_Battler\Scripts\Runtime\Units\UnitRagdoll.cs(22,23): warning CS0414: The field 'UnitController.groundCalm' is assigned but its value is never used
- 47.54 s warning: Assets\_Battler\Scripts\Runtime\Units\UnitBalance.cs(86,23): warning CS0414: The field 'UnitController.downCalm' is assigned but its value is never used
- 47.54 s warning: Assets\_Battler\Scripts\Runtime\Units\UnitRhythm.cs(50,23): warning CS0414: The field 'UnitController.heldDrift' is assigned but its value is never used
- 47.54 s warning: Assets\_Battler\Scripts\Runtime\Motion\HeadLook.cs(36,33): warning CS0414: The field 'HeadLook.pitchSpeed' is assigned but its value is never used
- 47.54 s warning: Assets\_Battler\Scripts\Runtime\Units\GroundContacts.cs(82,29): warning CS0414: The field 'GroundContacts.dirty' is assigned but its value is never used
- 47.54 s warning: Assets\_Battler\Scripts\Runtime\Locomotion\ClipFighter.cs(323,40): warning CS0414: The field 'ClipFighter.shieldReadHeld' is assigned but its value is never used
- 47.54 s warning: Assets\_Battler\Scripts\Runtime\Units\UnitTempo.cs(36,22): warning CS0414: The field 'UnitController.moodStanding' is assigned but its value is never used
- 47.54 s warning: Assets\_Battler\Scripts\Runtime\Motion\HeadLook.cs(35,23): warning CS0414: The field 'HeadLook.chosenAt' is assigned but its value is never used
- 47.54 s warning: Assets\_Battler\Scripts\Runtime\Units\UnitLook.cs(19,23): warning CS0414: The field 'UnitController.lookWeight' is assigned but its value is never used
- 47.54 s warning: Assets\_Battler\Scripts\Runtime\Units\UnitLook.cs(19,49): warning CS0414: The field 'UnitController.glanceUntil' is assigned but its value is never used
- 47.54 s warning: Assets\_Battler\Scripts\Runtime\Units\UnitBalance.cs(90,21): warning CS0414: The field 'UnitController.stumbleSteps' is assigned but its value is never used
- 47.54 s warning: Assets\_Battler\Scripts\Runtime\Units\UnitLook.cs(20,32): warning CS0414: The field 'UnitController.glanceAt' is assigned but its value is never used
- 47.54 s warning: Assets\_Battler\Scripts\Runtime\Motion\Tempo.cs(183,21): warning CS0414: The field 'TempoDirector.blocked' is assigned but its value is never used
- 47.54 s warning: Assets\_Battler\Scripts\Runtime\Units\UnitCombat.cs(89,23): warning CS0414: The field 'UnitController.momentumLeft' is assigned but its value is never used

