# Playtest 2026-11-03_07-02-17

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 40.5 s of play (2429 physics steps), 3043 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`, `perf.csv.gz` (a row a rendered frame: frame time, physics steps, our work by section, Unity's counters, garbage collection), `perf_counters.txt`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-11-03_07-02-17`.

## A Small

Present 0.0–40.5 s. Steps 29, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 21 cm from the support point; hips dropped for reach by up to 1 cm.

Feet: 24 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 51°, 95% under 73°.
Walking: 6.7 s (95% forward, 0% sideways, 5% backward), the body went 3.0 m and the feet 3.2 m each on average (feet/body 1.09; well under 1 is gliding); the feet point off the line it walks along (either way) median 8°, 95% under 44°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| Spine | Ragdoll | 0.0 / 0.0 | 3 / 18 |
| Chest | Ragdoll | 0.6 / 3.6 | 6 / 31 |
| Head | Ragdoll | 1.2 / 8.6 | 12 / 74 |
| UpperArmL | Ragdoll | 1.1 / 7.9 | 8 / 147 |
| ForearmL | Ragdoll | 1.8 / 14.6 | 9 / 97 |
| HandL | Ragdoll | 2.7 / 36.4 | 24 / 83 |
| UpperArmR | Ragdoll | 1.1 / 8.6 | 6 / 49 |
| ForearmR | Ragdoll | 2.1 / 19.5 | 10 / 78 |
| HandR | Ragdoll | 4.5 / 32.3 | 13 / 149 |
| ThighL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ShinL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootL | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| ThighR | Ragdoll | 0.0 / 0.0 | 0 / 149 |
| ShinR | Ragdoll | 0.0 / 0.0 | 0 / 0 |
| FootR | Ragdoll | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 1 (0 on the part aimed at, 0 blocked, 0 parried, 1 on someone in the way, 0 on allies), 11 damage. Hits taken: 3, 64 damage. Killed.

Defence: 3 blows came at it (1 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, set its weapon across 1.

Pose against the joint limits (standing): a joint more than 10° past its limit 59% of the time; the worst 104° (ThighR).
Ragdoll (on the ground 30.3 s): a joint more than 10° past its limit 60% of the time; the worst 152° (HandL). Fastest part with a standing leg within 0.5 m: 0.3 m/s median, 4.4 m/s at the 95th percentile (1106 steps); with none: 0.0 m/s median (711 steps).

Look: within 25° of what it attended to 75% of the time standing (78% of the time it was within the head's reach, 95% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 4 postures (StickSettle 1, AbanicoFlurry 1, BaitRetreat 1, RedondaTwirl 1), 0 of them lead-ins to blows; energy at the end 0.77.

Balance: stumbled 2 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Medium

Present 0.0–40.5 s. Steps 86, idle shuffles 4. Feet slid at most 0.0 cm while planted; the weight was at most 42 cm from the support point; hips dropped for reach by up to 0 cm.

Feet: 75 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 34°, 95% under 88°.
Walking: 8.5 s (85% forward, 5% sideways, 9% backward), the body went 4.7 m and the feet 6.2 m each on average (feet/body 1.31; well under 1 is gliding); the feet point off the line it walks along (either way) median 10°, 95% under 42°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 4 / 80 |
| Chest | Pinned | 1.1 / 22.9 | 8 / 157 |
| Head | Pinned | 2.0 / 39.0 | 13 / 70 |
| UpperArmL | Pinned | 3.2 / 68.9 | 14 / 176 |
| ForearmL | Pinned | 8.1 / 106.4 | 23 / 149 |
| HandL | Pinned | 14.6 / 141.0 | 22 / 173 |
| UpperArmR | Muscled | 2.3 / 21.9 | 10 / 179 |
| ForearmR | Muscled | 1.6 / 32.2 | 4 / 87 |
| HandR | Muscled | 2.4 / 56.6 | 5 / 105 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 60 |
| ShinL | Kinematic | 0.0 / 9.0 | 0 / 60 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 1 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 147 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 2 (0 on the part aimed at, 0 blocked, 2 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 4, 11 damage. Health at the end 8.

Defence: 4 blows came at it (2 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 2, set its weapon across 0.

Pose against the joint limits (standing): a joint more than 10° past its limit 25% of the time; the worst 98° (FootL).

Look: within 25° of what it attended to 82% of the time standing (83% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 94% of the time standing; its rim came within 1.0 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 15 postures (GuardChange 1, HighFeint 2, Peek 3, ShieldSettle 1, ShieldPress 5, ShieldAdvance 3), 1 of them lead-ins to blows; energy at the end 0.99.

Balance: stumbled 3 times, knocked down 0, launched 0, got up 0; thrown into others 0 times.


## A Large

Present 0.0–40.5 s. Steps 82, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 64 cm from the support point; hips dropped for reach by up to 3 cm.

Feet: 67 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 22°, 95% under 83°.
Walking: 17.7 s (23% forward, 6% sideways, 71% backward), the body went 8.9 m and the feet 11.1 m each on average (feet/body 1.24; well under 1 is gliding); the feet point off the line it walks along (either way) median 10°, 95% under 81°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 2 / 19 |
| Chest | Pinned | 0.9 / 7.8 | 3 / 28 |
| Head | Pinned | 1.3 / 17.5 | 15 / 63 |
| UpperArmL | Pinned | 1.7 / 18.9 | 6 / 65 |
| ForearmL | Pinned | 3.5 / 24.1 | 9 / 58 |
| HandL | Pinned | 4.8 / 45.0 | 22 / 92 |
| UpperArmR | Muscled | 1.8 / 23.5 | 4 / 60 |
| ForearmR | Muscled | 3.2 / 39.5 | 5 / 91 |
| HandR | Muscled | 4.4 / 54.3 | 8 / 176 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 149 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 9 (3 on the part aimed at, 2 blocked, 1 parried, 0 on someone in the way, 0 on allies), 73 damage. Hits taken: 8, 58 damage. Health at the end 162.

Defence: 9 blows came at it (3 seen clearly, 6 at the edge of its view, 0 never seen); it raised the shield against 0, set its weapon across 4.

Pose against the joint limits (standing): a joint more than 10° past its limit 55% of the time; the worst 116° (ShinR).

Look: within 25° of what it attended to 80% of the time standing (85% of the time it was within the head's reach, 90% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 8 postures (BoxerSettle 2, BobAndWeave 3, ClubFeint 2, JabFeint 1), 2 of them lead-ins to blows; energy at the end 0.60.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 6. The posed striking part passed within 3 cm of the aim point in 6; aim error mean 0.1 cm, max 0.5 cm (posed); physical weapon's closest pass mean 2.5 cm, max 9.0 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.78 s: ClubRising at B Small's Belly: passed 0.1 cm from the aim point; nearest part B Small's Chest (28.7 cm); the Belly had moved 35.9 cm since the aim was locked
- 5.77 s: ClubRising at B Large's KneeL: passed 0.0 cm from the aim point; nearest part B Large's ShinL (10.7 cm); the KneeL had moved 9.9 cm since the aim was locked
- 6.72 s: ClubSwing at B Large's Head: passed 0.5 cm from the aim point; nearest part B Large's Chest (0.0 cm); the Head had moved 22.0 cm since the aim was locked
- 12.18 s: ClubSwing at B Small's Head: passed 0.1 cm from the aim point; nearest part B Small's UpperArmL (38.6 cm); the Head had moved 76.7 cm since the aim was locked
- 17.97 s: ClubBackhand at B Small's Neck: passed 0.1 cm from the aim point; nearest part B Small's Chest (12.7 cm); the Neck had moved 22.3 cm since the aim was locked
- 21.85 s: ClubSwing at B Medium's Head: passed 0.0 cm from the aim point; nearest part B Medium's Head (1.3 cm); the Head had moved 12.4 cm since the aim was locked


## B Small

Present 0.0–40.5 s. Steps 101, idle shuffles 2. Feet slid at most 0.0 cm while planted; the weight was at most 10 cm from the support point; hips dropped for reach by up to 12 cm.

Feet: 81 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 46°, 95% under 72°.
Walking: 24.3 s (92% forward, 4% sideways, 3% backward), the body went 11.8 m and the feet 13.4 m each on average (feet/body 1.13; well under 1 is gliding); the feet point off the line it walks along (either way) median 9°, 95% under 47°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.1 | 3 / 30 |
| Chest | Pinned | 0.7 / 5.4 | 6 / 73 |
| Head | Pinned | 1.1 / 14.2 | 8 / 111 |
| UpperArmL | Pinned | 1.1 / 16.8 | 5 / 117 |
| ForearmL | Pinned | 1.2 / 22.6 | 6 / 138 |
| HandL | Pinned | 1.8 / 36.3 | 17 / 129 |
| UpperArmR | Muscled | 1.4 / 7.3 | 8 / 46 |
| ForearmR | Muscled | 3.0 / 17.7 | 13 / 69 |
| HandR | Muscled | 5.3 / 27.5 | 16 / 91 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 parried, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 4, 35 damage. Health at the end 25.

Defence: 4 blows came at it (3 seen clearly, 1 at the edge of its view, 0 never seen); it raised the shield against 0, set its weapon across 2.

Pose against the joint limits (standing): a joint more than 10° past its limit 65% of the time; the worst 51° (FootR).
Ragdoll (on the ground 4.0 s): a joint more than 10° past its limit 84% of the time; the worst 167° (ShinL). Fastest part with a standing leg within 0.5 m: 9.1 m/s median, 19.5 m/s at the 95th percentile (55 steps); with none: 1.8 m/s median (182 steps).

Look: within 25° of what it attended to 86% of the time standing (87% of the time it was within the head's reach, 98% of the time); head outside its limits 0 steps.

Fight Rhythm (Eskrima): 11 postures (StickSettle 3, AbanicoFlurry 4, BaitRetreat 1, RedondaTwirl 3), 0 of them lead-ins to blows; energy at the end 1.00.

Balance: stumbled 1 time, knocked down 1, launched 2, got up 3; thrown into others 0 times.


## B Medium

Present 0.0–40.5 s. Steps 100, idle shuffles 1. Feet slid at most 0.0 cm while planted; the weight was at most 41 cm from the support point; hips dropped for reach by up to 18 cm.

Feet: 76 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 25°, 95% under 69°.
Walking: 13.5 s (61% forward, 20% sideways, 20% backward), the body went 6.8 m and the feet 8.7 m each on average (feet/body 1.28; well under 1 is gliding); the feet point off the line it walks along (either way) median 25°, 95% under 74°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.7 | 5 / 102 |
| Chest | Pinned | 1.2 / 21.6 | 6 / 78 |
| Head | Pinned | 1.4 / 41.6 | 13 / 80 |
| UpperArmL | Pinned | 2.0 / 52.6 | 10 / 136 |
| ForearmL | Pinned | 3.8 / 46.1 | 11 / 173 |
| HandL | Pinned | 4.2 / 35.8 | 12 / 96 |
| UpperArmR | Muscled | 1.9 / 20.3 | 6 / 47 |
| ForearmR | Muscled | 3.8 / 29.9 | 9 / 126 |
| HandR | Muscled | 5.7 / 52.0 | 14 / 162 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 8 (3 on the part aimed at, 1 blocked, 1 parried, 0 on someone in the way, 0 on allies), 113 damage. Hits taken: 3, 26 damage. Health at the end 74.

Defence: 5 blows came at it (3 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 1, set its weapon across 0.

Pose against the joint limits (standing): a joint more than 10° past its limit 29% of the time; the worst 43° (FootL).
Ragdoll (on the ground 1.1 s): a joint more than 10° past its limit 96% of the time; the worst 118° (Head). Fastest part with a standing leg within 0.5 m: - median, - at the 95th percentile (0 steps); with none: 2.0 m/s median (67 steps).

Look: within 25° of what it attended to 91% of the time standing (91% of the time it was within the head's reach, 100% of the time); head outside its limits 0 steps.
Shield: faced within 45° of its threat 93% of the time standing; its rim came within 1.1 cm of its own body (negative: inside).

Fight Rhythm (ShieldAndSword): 9 postures (GuardChange 2, Peek 2, HighFeint 2, ShieldPress 2, ShieldSettle 1), 2 of them lead-ins to blows; energy at the end 0.60.

Balance: stumbled 1 time, knocked down 1, launched 0, got up 1; thrown into others 0 times.

Blows: 7. The posed striking part passed within 3 cm of the aim point in 7; aim error mean 0.6 cm, max 2.7 cm (posed); physical weapon's closest pass mean 0.3 cm, max 1.0 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.10 s: SwordCut at A Small's Head: passed 1.2 cm from the aim point; nearest part A Small's Head (15.1 cm); the Head had moved 27.7 cm since the aim was locked
- 8.37 s: SwordThrust at A Small's Head: passed 0.1 cm from the aim point; nearest part A Small's Head (16.9 cm); the Head had moved 21.9 cm since the aim was locked
- 10.17 s: SwordCleave at A Small's Chest: passed 0.2 cm from the aim point; nearest part A Small's Chest (4.4 cm); the Chest had moved 33.2 cm since the aim was locked
- 12.47 s: SwordThrust at A Medium's Belly: passed 0.1 cm from the aim point; nearest part A Medium's Chest (21.7 cm); the Belly had moved 45.3 cm since the aim was locked
- 16.75 s: SwordCleave at A Medium's Belly: passed 0.1 cm from the aim point; nearest part A Medium's Pelvis (40.0 cm); the Belly had moved 69.8 cm since the aim was locked
- 38.38 s: SwordThrust at A Large's Chest: passed 0.0 cm from the aim point; nearest part A Large's HandL (28.3 cm); the Chest had moved 48.6 cm since the aim was locked
- 39.42 s: SwordThrust at A Large's Chest: passed 2.7 cm from the aim point; nearest part A Large's HandR (9.7 cm); the Chest had moved 25.5 cm since the aim was locked


## B Large

Present 0.0–40.5 s. Steps 86, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 39 cm from the support point; hips dropped for reach by up to 3 cm.

Feet: 64 plants, 0 slid over 3 cm, 0 twisted over 10°, 0 floated and 0 sank over 2.5 cm (worst: slid 0 cm, twisted 0°, 0.0 cm up, 0.0 cm down). Planted feet: off the ground 0% of the time, in it 0%; sole height median 0.0 cm; sliding (physical) median 0 cm/s, 95% under 0 cm/s; (posed) 95% under 0 cm/s; twisting 95% under 0°/s; pointing off their planned way 95% under 0°, off the hips' way median 18°, 95% under 59°.
Walking: 21.7 s (48% forward, 12% sideways, 41% backward), the body went 10.6 m and the feet 13.6 m each on average (feet/body 1.28; well under 1 is gliding); the feet point off the line it walks along (either way) median 12°, 95% under 74°.

Physical parts against the pose while standing (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 0.2 | 2 / 19 |
| Chest | Pinned | 0.8 / 7.4 | 3 / 41 |
| Head | Pinned | 1.4 / 18.6 | 12 / 68 |
| UpperArmL | Pinned | 1.6 / 25.8 | 7 / 164 |
| ForearmL | Pinned | 3.9 / 55.2 | 10 / 79 |
| HandL | Pinned | 4.9 / 63.6 | 24 / 155 |
| UpperArmR | Muscled | 1.6 / 33.0 | 3 / 59 |
| ForearmR | Muscled | 3.1 / 63.8 | 5 / 91 |
| HandR | Muscled | 4.0 / 79.1 | 7 / 115 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 150 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 7 (0 on the part aimed at, 1 blocked, 2 parried, 0 on someone in the way, 1 on allies), 30 damage. Hits taken: 5, 34 damage. Health at the end 186.

Defence: 5 blows came at it (3 seen clearly, 2 at the edge of its view, 0 never seen); it raised the shield against 0, set its weapon across 4.

Pose against the joint limits (standing): a joint more than 10° past its limit 41% of the time; the worst 96° (ShinR).

Look: within 25° of what it attended to 86% of the time standing (87% of the time it was within the head's reach, 99% of the time); head outside its limits 0 steps.

Fight Rhythm (Boxing): 8 postures (BoxerSettle 2, BoxerStalk 1, BobAndWeave 1, ClubFeint 3, JabFeint 1), 1 of them lead-ins to blows; energy at the end 0.87.

Balance: stumbled 1 time, knocked down 0, launched 0, got up 0; thrown into others 0 times.

Blows: 2. The posed striking part passed within 3 cm of the aim point in 2; aim error mean 0.5 cm, max 0.9 cm (posed); physical weapon's closest pass mean 0.0 cm, max 0.0 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 29.18 s: ClubSwing at A Large's ElbowL: passed 0.0 cm from the aim point; nearest part A Large's ForearmL (34.2 cm); the ElbowL had moved 48.6 cm since the aim was locked
- 34.00 s: ClubSwing at A Large's Neck: passed 0.9 cm from the aim point; nearest part A Large's HandR (12.0 cm); the Neck had moved 68.6 cm since the aim was locked

## Defence (milestone 11)

- Small: pressed 8 s, 54% of it in the shell (gate 70%); 0 of 0 combo links defended, n/a (gate 30–60%); 1 of 7 blows that met it on its guard (0 blocked, 1 parried, 0 on its guarding arms); 1 guard breaks, 1.5 a minute on its feet (gate about 1 at most); 1 of 2 bursts at it answered with a counter, 50% (gate 30–50%), 0 of them ripostes between links.
- Medium: pressed 9 s, 21% of it in the shell (gate 70%); 3 of 7 combo links defended, 43% (gate 30–60%); 4 of 7 blows that met it on its guard (4 blocked, 0 parried, 0 on its guarding arms); 1 guard breaks, 0.8 a minute on its feet (gate about 1 at most); 0 of 5 bursts at it answered with a counter, 0% (gate 30–50%), 0 of them ripostes between links.
- Large: pressed 31 s, 73% of it in the shell (gate 70%); 1 of 6 combo links defended, 17% (gate 30–60%); 7 of 13 blows that met it on its guard (0 blocked, 5 parried, 2 on its guarding arms); 1 guard breaks, 0.8 a minute on its feet (gate about 1 at most); 1 of 10 bursts at it answered with a counter, 10% (gate 30–50%), 0 of them ripostes between links.

## Shield contacts

285 times a shield overlapped another unit's body: 95% under 3.8 cm deep (93% under 2 cm; target 95%), the deepest 21.4 cm, the longest 1.95 s (target under 0.2 s). Each one is in events.log ("shield").

## Performance

20196 frames in 45.8 s (441 fps on average); 17804 quiet ones (no physics, under 20 ms) not written. Frame time of those written: median 5.6 ms, 95% 10.6, 99% 36.6, worst 1699.6. 22 hitches over 40 ms (7 over 100 ms). 85 garbage collections.

Our work, milliseconds per second of play (a section inside another counts in both):

- UnitStep: 111.3
- Compose: 90.9
- ArmSolve: 89.8
- RecorderSample: 74.4
- RecorderFlush: 30.7
- ComposeDry: 22.5
- Sandbox: 22.0
- Clearance: 16.4
- Plan: 14.7
- Authority: 5.2
- Prepare: 2.4
- TryOut: 0.8
- Sweep: 0.7
- CaptureSave: 0.5
- CaptureRender: 0.0

The worst frames:

- 33.08 s: 1700 ms frame, 3 physics steps; ours: UnitStep 5.6 ms (18), Compose 4.9 ms (15), ArmSolve 3.8 ms (81), RecorderSample 3.6 ms (3), RecorderFlush 1.0 ms (3); Unity: Main_Thread 310.5 ms, PlayerLoop 302.5 ms, Gfx_WaitForGfxCommandsFromMainThread 19.3 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 9.4 ms, FixedBehaviourUpdate 9.4 ms, CPU_Total_Frame_Time 1.3 ms, FixedUpdate_PhysicsFixedUpdate 1.2 ms, Physics_Simulate 1.0 ms
- 38.33 s: 902 ms frame, 0 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: nothing over 0.5 ms; Unity: CPU_Total_Frame_Time 1.3 ms, Main_Thread 1.3 ms, CPU_Main_Thread_Frame_Time 0.8 ms, PlayerLoop 0.8 ms, Gfx_WaitForGfxCommandsFromMainThread 0.6 ms, GPU_Frame_Time 0.6 ms, CPU_Render_Thread_Frame_Time 0.5 ms, GC_Alloc 0.0 ms
- 39.36 s: 834 ms frame, 0 physics steps; ours: nothing over 0.5 ms; Unity: CPU_Total_Frame_Time 1.2 ms, Main_Thread 1.2 ms, CPU_Main_Thread_Frame_Time 0.8 ms, PlayerLoop 0.7 ms, GPU_Frame_Time 0.5 ms, CPU_Render_Thread_Frame_Time 0.5 ms, GC_Alloc 0.0 ms, Physics_Interpolation 0.0 ms
- 0.02 s: 756 ms frame, 1 physics steps, garbage collected (gen0 4, gen1 4, gen2 4); ours: UnitStep 38.4 ms (6), RecorderSample 33.5 ms (1), Compose 28.0 ms (12), ArmSolve 7.1 ms (18), Authority 4.0 ms (6), Sandbox 2.3 ms (1); Unity: Gfx_WaitForGfxCommandsFromMainThread 428.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 77.2 ms, FixedBehaviourUpdate 77.2 ms, Physics_Simulate 15.5 ms, GC_Collect 10.8 ms, GC_Alloc 2.0 ms, Update_ScriptRunBehaviourUpdate 1.5 ms, BehaviourUpdate 1.5 ms
- 37.37 s: 696 ms frame, 1 physics steps; ours: RecorderSample 1.5 ms (1), UnitStep 1.3 ms (6), Compose 1.1 ms (5), RecorderFlush 0.8 ms (1), ArmSolve 0.8 ms (13); Unity: Main_Thread 4.8 ms, PlayerLoop 4.2 ms, Gfx_WaitForGfxCommandsFromMainThread 4.1 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 2.9 ms, FixedBehaviourUpdate 2.9 ms, CPU_Total_Frame_Time 1.2 ms, CPU_Main_Thread_Frame_Time 0.7 ms, CPU_Render_Thread_Frame_Time 0.6 ms
- 33.13 s: 450 ms frame, 3 physics steps; ours: UnitStep 5.5 ms (18), Compose 4.8 ms (15), RecorderSample 3.9 ms (3), ArmSolve 3.7 ms (79), RecorderFlush 1.2 ms (3); Unity: Main_Thread 1694.8 ms, Gfx_WaitForGfxCommandsFromMainThread 1685.6 ms, PlayerLoop 15.9 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 9.5 ms, FixedBehaviourUpdate 9.5 ms, CPU_Total_Frame_Time 1.4 ms, FixedUpdate_PhysicsFixedUpdate 1.3 ms, Physics_Simulate 1.0 ms
- 33.03 s: 290 ms frame, 0 physics steps, garbage collected (gen0 1, gen1 1, gen2 1); ours: nothing over 0.5 ms; Unity: Main_Thread 3.9 ms, PlayerLoop 3.3 ms, GC_Collect 1.3 ms, CPU_Total_Frame_Time 1.3 ms, CPU_Main_Thread_Frame_Time 0.8 ms, CPU_Render_Thread_Frame_Time 0.6 ms, GPU_Frame_Time 0.6 ms, Physics_Interpolation 0.0 ms
- 34.08 s: 63 ms frame, 3 physics steps; ours: RecorderSample 5.1 ms (3), UnitStep 4.9 ms (18), Compose 4.1 ms (15), ArmSolve 3.2 ms (61), RecorderFlush 2.9 ms (3); Unity: Main_Thread 59.9 ms, Gfx_WaitForGfxCommandsFromMainThread 58.0 ms, PlayerLoop 14.1 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 10.2 ms, FixedBehaviourUpdate 10.2 ms, CPU_Total_Frame_Time 2.9 ms, CPU_Main_Thread_Frame_Time 1.6 ms, FixedUpdate_PhysicsFixedUpdate 1.0 ms
- 3.98 s: 53 ms frame, 1 physics steps; ours: Sandbox 45.0 ms (1), ArmSolve 41.9 ms (800), ComposeDry 41.6 ms (54), Clearance 24.1 ms (1), UnitStep 4.0 ms (6), Compose 3.3 ms (5); Unity: Main_Thread 52.9 ms, Gfx_WaitForGfxCommandsFromMainThread 52.0 ms, PlayerLoop 51.0 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 50.0 ms, FixedBehaviourUpdate 50.0 ms, CPU_Total_Frame_Time 1.1 ms, FixedUpdate_PhysicsFixedUpdate 0.7 ms, CPU_Render_Thread_Frame_Time 0.6 ms
- 34.03 s: 52 ms frame, 1 physics steps; ours: UnitStep 2.0 ms (6), Compose 1.7 ms (5), RecorderSample 1.5 ms (1), ArmSolve 1.3 ms (23), RecorderFlush 0.7 ms (1); Unity: Main_Thread 15.5 ms, Gfx_WaitForGfxCommandsFromMainThread 14.3 ms, CPU_Total_Frame_Time 6.9 ms, PlayerLoop 6.2 ms, CPU_Main_Thread_Frame_Time 5.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 3.5 ms, FixedBehaviourUpdate 3.5 ms, CPU_Render_Thread_Frame_Time 0.7 ms
- 39.05 s: 49 ms frame, 1 physics steps; ours: UnitStep 44.6 ms (6), ComposeDry 38.5 ms (54), ArmSolve 35.7 ms (969), Clearance 27.4 ms (1), Compose 2.9 ms (5), Prepare 2.0 ms (1); Unity: Main_Thread 49.1 ms, PlayerLoop 48.0 ms, Gfx_WaitForGfxCommandsFromMainThread 46.6 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 45.4 ms, FixedBehaviourUpdate 45.4 ms, CPU_Total_Frame_Time 1.2 ms, CPU_Main_Thread_Frame_Time 0.7 ms, GPU_Frame_Time 0.6 ms
- 33.18 s: 46 ms frame, 3 physics steps; ours: UnitStep 5.2 ms (18), RecorderSample 4.8 ms (3), Compose 4.7 ms (15), ArmSolve 3.6 ms (78), RecorderFlush 2.5 ms (3); Unity: Main_Thread 450.5 ms, Gfx_WaitForGfxCommandsFromMainThread 448.3 ms, PlayerLoop 14.4 ms, FixedUpdate_ScriptRunBehaviourFixedUpdate 10.1 ms, FixedBehaviourUpdate 10.1 ms, CPU_Total_Frame_Time 4.4 ms, CPU_Main_Thread_Frame_Time 3.9 ms, FixedUpdate_PhysicsFixedUpdate 1.0 ms

Unity's counters recorded: Main Thread, CPU Main Thread Frame Time, CPU Render Thread Frame Time, CPU Total Frame Time, GPU Frame Time, PlayerLoop, EditorLoop, FixedUpdate.PhysicsFixedUpdate, FixedUpdate.ScriptRunBehaviourFixedUpdate, Physics.Simulate, Physics.FetchResults, Physics.Interpolation, Update.ScriptRunBehaviourUpdate, PreLateUpdate.ScriptRunBehaviourLateUpdate, BehaviourUpdate, FixedBehaviourUpdate, LateBehaviourUpdate, Camera.Render, Gfx.WaitForPresentOnGfxThread, Gfx.WaitForGfxCommandsFromMainThread, Gfx.PresentFrame, WaitForTargetFPS, GC.Collect, GC.Alloc, GC Allocated In Frame, GC Allocation In Frame Count, GC Used Memory, GC Reserved Memory, Total Used Memory, System Used Memory, Active Dynamic Bodies, Active Kinematic Bodies, Dynamic Bodies, Kinematic Bodies, Static Colliders, Active Constraints, Trigger Overlaps, Batches Count, SetPass Calls Count, Draw Calls Count, Triangles Count, Loading.ReadObject, Shader.CreateGPUProgram.

