# Playtest 2026-10-31_18-31-15

Unity 6000.6.7f1, scene Battle, physics 60 Hz, solver 12/4.
Recorded 68.9 s of play (4136 physics steps), 2228 events, 0 marks.

Files: `events.log` (what happened and why), `units/*.frames.csv.gz` (per-step state), `units/*.pose.csv.gz` (per-step pose, physical and animated), `world.csv.gz` (props, if any), `config/` (tuning assets), `meta.json`.
Analyse with `python3 Tools/Playtest/analyze.py PlaytestLogs/2026-10-31_18-31-15`.

## A Small

Present 0.0–68.9 s. Steps 33, idle shuffles 0. Feet slid at most 0.0 cm while planted; the weight was at most 11 cm from the support point; hips dropped for reach by up to 3 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 209.4 / 533.0 | 125 / 180 |
| Spine | Ragdoll | 208.5 / 533.0 | 126 / 180 |
| Chest | Ragdoll | 208.0 / 542.5 | 122 / 180 |
| Head | Ragdoll | 207.4 / 555.4 | 122 / 180 |
| UpperArmL | Ragdoll | 213.0 / 563.2 | 116 / 180 |
| ForearmL | Ragdoll | 200.4 / 567.8 | 120 / 180 |
| HandL | Ragdoll | 201.5 / 564.1 | 99 / 180 |
| UpperArmR | Ragdoll | 203.7 / 546.8 | 94 / 180 |
| ForearmR | Ragdoll | 195.4 / 551.7 | 96 / 180 |
| HandR | Ragdoll | 204.1 / 567.7 | 106 / 180 |
| ThighL | Ragdoll | 214.9 / 545.4 | 103 / 180 |
| ShinL | Ragdoll | 222.8 / 576.1 | 106 / 180 |
| FootL | Ragdoll | 228.4 / 587.1 | 102 / 180 |
| ThighR | Ragdoll | 205.3 / 528.2 | 114 / 180 |
| ShinR | Ragdoll | 200.1 / 543.1 | 112 / 180 |
| FootR | Ragdoll | 212.5 / 564.4 | 109 / 180 |

Hits dealt: 0 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 0 on allies), 0 damage. Hits taken: 2, 66 damage. Killed.


## A Medium

Present 0.0–68.9 s. Steps 78, idle shuffles 7. Feet slid at most 0.0 cm while planted; the weight was at most 52 cm from the support point; hips dropped for reach by up to 16 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 41.2 / 150.7 | 40 / 179 |
| Spine | ? | 42.1 / 148.1 | 36 / 180 |
| Chest | ? | 44.5 / 160.1 | 31 / 179 |
| Head | ? | 49.0 / 202.9 | 36 / 180 |
| UpperArmL | ? | 48.6 / 192.2 | 36 / 180 |
| ForearmL | ? | 48.4 / 185.0 | 33 / 179 |
| HandL | ? | 50.7 / 210.9 | 49 / 180 |
| UpperArmR | ? | 47.1 / 197.5 | 33 / 174 |
| ForearmR | ? | 42.1 / 181.9 | 39 / 180 |
| HandR | ? | 47.0 / 224.7 | 45 / 180 |
| ThighL | ? | 42.9 / 161.5 | 34 / 180 |
| ShinL | ? | 50.0 / 195.9 | 29 / 180 |
| FootL | ? | 53.7 / 228.7 | 30 / 179 |
| ThighR | ? | 39.5 / 145.8 | 48 / 179 |
| ShinR | ? | 41.6 / 161.4 | 45 / 180 |
| FootR | ? | 51.0 / 223.5 | 44 / 180 |

Hits dealt: 8 (1 on the part aimed at, 2 blocked, 0 on someone in the way, 3 on allies), 96 damage. Hits taken: 14, 107 damage. Health at the end 0.

Blows: 1. The posed striking part passed within 3 cm of the aim point in 1; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.1 cm, max 0.1 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 8.38 s: SwordThrust at B Small's Head: passed 0.1 cm from the aim point; nearest part B Small's Chest (14.1 cm); the Head had moved 19.6 cm since the aim was locked


## A Large

Present 0.0–68.9 s. Steps 78, idle shuffles 5. Feet slid at most 37.9 cm while planted; the weight was at most 40 cm from the support point; hips dropped for reach by up to 6 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 2.9 / 129.8 | 2 / 115 |
| Spine | ? | 3.3 / 151.6 | 2 / 119 |
| Chest | ? | 4.0 / 185.9 | 2 / 122 |
| Head | ? | 4.9 / 248.0 | 6 / 164 |
| UpperArmL | ? | 4.7 / 228.6 | 5 / 179 |
| ForearmL | ? | 4.8 / 172.3 | 7 / 180 |
| HandL | ? | 6.3 / 205.1 | 11 / 179 |
| UpperArmR | ? | 4.6 / 236.9 | 2 / 108 |
| ForearmR | ? | 4.6 / 221.7 | 4 / 178 |
| HandR | ? | 4.6 / 205.1 | 6 / 174 |
| ThighL | ? | 2.9 / 131.8 | 3 / 130 |
| ShinL | ? | 1.8 / 96.0 | 2 / 132 |
| FootL | ? | 0.5 / 33.4 | 2 / 139 |
| ThighR | ? | 2.6 / 115.4 | 3 / 156 |
| ShinR | ? | 2.0 / 108.6 | 2 / 131 |
| FootR | ? | 0.6 / 38.2 | 2 / 134 |

Hits dealt: 13 (1 on the part aimed at, 4 blocked, 1 on someone in the way, 0 on allies), 123 damage. Hits taken: 16, 225 damage. Health at the end 0.

Blows: 4. The posed striking part passed within 3 cm of the aim point in 4; aim error mean 0.1 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.2 cm, max 0.2 cm. 2 reached the part they were aimed at (nearest part within 5 cm at contact).

- 4.05 s: ClubSmash at B Large's Head: passed 0.0 cm from the aim point; nearest part B Large's Chest (31.0 cm); the Head had moved 60.8 cm since the aim was locked
- 13.53 s: ClubRising at B Small's Chest: passed 0.1 cm from the aim point; nearest part B Small's Chest (4.6 cm); the Chest had moved 3.5 cm since the aim was locked
- 17.57 s: ClubSwing at B Small's Head: passed 0.1 cm from the aim point; nearest part B Small's Head (9.5 cm); the Head had moved 13.7 cm since the aim was locked
- 27.08 s: ClubSmash at B Medium's Chest: passed 0.0 cm from the aim point; nearest part B Medium's Chest (0.8 cm); the Chest had moved 13.4 cm since the aim was locked


## B Small

Present 0.0–68.9 s. Steps 56, idle shuffles 3. Feet slid at most 202.4 cm while planted; the weight was at most 13 cm from the support point; hips dropped for reach by up to 12 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Ragdoll | 117.3 / 167.5 | 79 / 141 |
| Spine | Ragdoll | 122.6 / 174.3 | 69 / 135 |
| Chest | Ragdoll | 130.6 / 187.2 | 68 / 132 |
| Head | Ragdoll | 144.0 / 207.2 | 81 / 156 |
| UpperArmL | Ragdoll | 143.7 / 212.1 | 72 / 180 |
| ForearmL | Ragdoll | 133.8 / 194.7 | 86 / 178 |
| HandL | Ragdoll | 134.9 / 195.2 | 103 / 180 |
| UpperArmR | Ragdoll | 136.8 / 192.9 | 96 / 179 |
| ForearmR | Ragdoll | 116.2 / 165.7 | 101 / 179 |
| HandR | Ragdoll | 111.4 / 159.0 | 117 / 179 |
| ThighL | Ragdoll | 117.2 / 168.6 | 66 / 180 |
| ShinL | Ragdoll | 101.2 / 142.9 | 85 / 178 |
| FootL | Ragdoll | 88.8 / 136.1 | 86 / 180 |
| ThighR | Ragdoll | 113.8 / 162.8 | 83 / 180 |
| ShinR | Ragdoll | 104.5 / 171.7 | 90 / 180 |
| FootR | Ragdoll | 115.4 / 202.4 | 92 / 179 |

Hits dealt: 3 (0 on the part aimed at, 0 blocked, 0 on someone in the way, 1 on allies), 24 damage. Hits taken: 3, 62 damage. Killed.


## B Medium

Present 0.0–68.9 s. Steps 118, idle shuffles 10. Feet slid at most 0.0 cm while planted; the weight was at most 43 cm from the support point; hips dropped for reach by up to 24 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | Kinematic | 0.0 / 0.0 | 0 / 0 |
| Spine | Pinned | 0.0 / 1.4 | 1 / 66 |
| Chest | Pinned | 0.3 / 19.3 | 1 / 97 |
| Head | Pinned | 0.3 / 31.0 | 5 / 102 |
| UpperArmL | Pinned | 0.5 / 49.6 | 3 / 178 |
| ForearmL | Pinned | 1.0 / 55.6 | 4 / 158 |
| HandL | Pinned | 1.3 / 39.2 | 16 / 180 |
| UpperArmR | Pinned | 0.1 / 4.6 | 0 / 41 |
| ForearmR | Pinned | 0.2 / 10.7 | 1 / 62 |
| HandR | Pinned | 0.3 / 24.0 | 3 / 92 |
| ThighL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootL | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ThighR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| ShinR | Kinematic | 0.0 / 0.0 | 0 / 0 |
| FootR | Kinematic | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 15 (6 on the part aimed at, 5 blocked, 1 on someone in the way, 0 on allies), 177 damage. Hits taken: 10, 38 damage. Health at the end 62.

Blows: 3. The posed striking part passed within 3 cm of the aim point in 3; aim error mean 0.2 cm, max 0.6 cm (posed); physical weapon's closest pass mean 0.3 cm, max 0.6 cm. 1 reached the part they were aimed at (nearest part within 5 cm at contact).

- 5.28 s: SwordThrust at A Small's Head: passed 0.6 cm from the aim point; nearest part A Small's Head (12.7 cm); the Head had moved 20.9 cm since the aim was locked
- 8.28 s: SwordThrust at A Small's Neck: passed 0.1 cm from the aim point; nearest part A Small's UpperArmR (19.3 cm); the Neck had moved 21.9 cm since the aim was locked
- 35.48 s: SwordCut at A Medium's Head: passed 0.0 cm from the aim point; nearest part A Medium's Head (0.8 cm); the Head had moved 18.8 cm since the aim was locked


## B Large

Present 0.0–68.9 s. Steps 68, idle shuffles 7. Feet slid at most 0.0 cm while planted; the weight was at most 52 cm from the support point; hips dropped for reach by up to 4 cm.

Physical parts against the pose (mean / max): distance in cm, angle in degrees.

| Part | Authority | Distance | Angle |
| --- | --- | --- | --- |
| Pelvis | ? | 0.0 / 0.0 | 0 / 0 |
| Spine | ? | 0.0 / 1.4 | 0 / 47 |
| Chest | ? | 0.2 / 19.6 | 0 / 60 |
| Head | ? | 0.2 / 24.5 | 2 / 82 |
| UpperArmL | ? | 0.3 / 27.7 | 2 / 142 |
| ForearmL | ? | 1.3 / 71.7 | 4 / 173 |
| HandL | ? | 2.0 / 75.5 | 8 / 176 |
| UpperArmR | ? | 0.1 / 5.9 | 0 / 45 |
| ForearmR | ? | 0.2 / 17.4 | 1 / 91 |
| HandR | ? | 0.5 / 51.7 | 2 / 176 |
| ThighL | ? | 0.0 / 0.0 | 0 / 0 |
| ShinL | ? | 0.0 / 0.0 | 0 / 0 |
| FootL | ? | 0.0 / 0.0 | 0 / 0 |
| ThighR | ? | 0.0 / 0.0 | 0 / 0 |
| ShinR | ? | 0.0 / 0.0 | 0 / 0 |
| FootR | ? | 0.0 / 0.0 | 0 / 0 |

Hits dealt: 14 (5 on the part aimed at, 6 blocked, 1 on someone in the way, 0 on allies), 160 damage. Hits taken: 8, 82 damage. Health at the end 0.

Blows: 5. The posed striking part passed within 3 cm of the aim point in 5; aim error mean 0.0 cm, max 0.1 cm (posed); physical weapon's closest pass mean 0.2 cm, max 0.2 cm. 0 reached the part they were aimed at (nearest part within 5 cm at contact).

- 3.98 s: ClubSwing at A Medium's Neck: passed 0.1 cm from the aim point; nearest part A Medium's Head (10.9 cm); the Neck had moved 7.0 cm since the aim was locked
- 21.27 s: ClubSmash at A Medium's Neck: passed 0.0 cm from the aim point; nearest part A Medium's UpperArmR (9.5 cm); the Neck had moved 22.8 cm since the aim was locked
- 42.88 s: ClubSwing at A Medium's Neck: passed 0.1 cm from the aim point; nearest part A Medium's Chest (9.0 cm); the Neck had moved 8.0 cm since the aim was locked
- 48.40 s: ClubSmash at A Medium's Chest: passed 0.0 cm from the aim point; nearest part A Medium's Chest (24.6 cm); the Chest had moved 48.7 cm since the aim was locked
- 67.55 s: ClubSmash at A Large's Chest: passed 0.0 cm from the aim point; nearest part A Large's Chest (81.8 cm); the Chest had moved 123.2 cm since the aim was locked

