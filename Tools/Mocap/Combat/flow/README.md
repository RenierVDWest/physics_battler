# Blade flow in the mocap takes (scratch)

Scratch scripts behind the combos research and plan of 20 November 2026
(`Docs/Plans/2026-11-20-combos-that-flow-and-end-in-a-bang-research-and-plan.md`). They read the sampled takes in
`../samples` (30 frames a second, 33 bones, world positions in metres). They are not part of the game or of the
headless tests. The plan's step 1 turns them into a proper report that reads the baked recordings.

The blade is not in the takes. It is worked out from the right hand's bones: 8 cm along the fingers for the grip,
0.9 m of blade leaning 20 degrees from the knuckles' line. So every speed and distance here is approximate. The
point's speed is measured against the hips, so a walking body's own travel is not counted.

- `python3 flow2.py <take> [<take>:L ...]`: the blows in a take (the point over 14 m/s), the time between them, how
  far the blade carries on after each, how slow it gets before the next, and how its path turns there.
- `python3 stations.py`: for each cut the Medium knows, where the sword hand and the point are when the blade last
  turns before the hit and first turns after it (in the hips' own frame: right, up, forward), and for every pair
  of cuts the gap between one's end and the other's start.
- `flow.py`: the loader and the blade, used by both.

Found with them: in the four-cut sword and shield combo the hits are 0.57 s and 1.20 s apart, the blade carries on
5.5 m after the first hit and is never slower than 2.1 m/s before the second. `sword_and_shield_attack_1` and
`spin_attack_1` are the same motion (he turns 396 degrees), as are `sword_and_shield_slash_2` and `spin_slash`.
