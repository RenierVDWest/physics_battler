# Locomotion lab

43,8 s, driven by hand. Draw the film with `python3 Tools/Mocap/Locomotion/locofilm.py locolab-2026-11-05_21-37-37-driven/locolab.json film.gif` (from PlaytestLogs/).

## Measures

Plants: 94. Planted feet moved (posed) at most 13,9 cm, 0,27 cm on average (gate under 1 cm).
The locks pulled the feet from the clips' by 15,3 cm (median), 42,5 cm (95%), 54,1 cm at most; 40 locks let go early (over-stretched).
Feet landed off the clips' by 0,0 cm (median), 16,6 cm (95%); the fastest swinging foot went 29,8 m/s against the body (right at 35,17 s (driving)).
Walking 21,7 s, running 0,0 s; both feet up while walking 1,23 s (5,7%, gate 0%); steps lifted before the other foot was down: 14.
Feet travelled 13,0 m against the body's 10,0 m: 1,30 (gate 0.95 to 1.05).
Planted soles off the ground at most 15,0 cm (gate 1 cm); the ankles missed their targets by at most 34,4 cm; fastest part 37,7 m/s (ForearmL at 35,17 s (driving)).
  3,53 s, driving: the right foot swung at 5,2 m/s (OneShot sns_chop_lunge)
  16,53 s, driving: the right foot swung at 5,1 m/s (OneShot sns_chop_side)
  16,88 s, driving: the left foot swung at 5,2 m/s (OneShot sns_chop_side)
  17,98 s, driving: the left foot swung at 14,1 m/s (OneShot sns_chop_side)
  18,30 s, driving: the right foot swung at 5,3 m/s (OneShot sns_chop_side)
  19,72 s, driving: the right foot swung at 5,7 m/s (OneShot sns_chop_side)
  20,15 s, driving: the left foot swung at 5,2 m/s (OneShot sns_chop_side)
  26,97 s, driving: the right foot swung at 7,3 m/s (OneShot sns_chop_side)
  27,28 s, driving: the left foot swung at 5,2 m/s (OneShot sns_chop_side)
  30,32 s, driving: the right foot swung at 5,5 m/s (Cycle)
  33,17 s, driving: the right foot swung at 6,8 m/s (OneShot sns_chop_side)
  33,55 s, driving: the left foot swung at 5,1 m/s (OneShot sns_chop_side)
  35,15 s, driving: the left foot swung at 20,8 m/s (OneShot sns_chop_side)
  35,47 s, driving: the right foot swung at 5,3 m/s (OneShot sns_chop_side)
  35,90 s, driving: the right foot swung at 7,7 m/s (OneShot sns_thrust)
  39,35 s, driving: the left foot swung at 8,3 m/s (OneShot sns_chop_side)
  39,67 s, driving: the right foot swung at 5,0 m/s (OneShot sns_chop_side)
  42,00 s, driving: the right foot swung at 10,1 m/s (OneShot sns_impact_body)
  38,25 s, driving: the right foot moved 10,1 cm while planted
  42,68 s, driving: the right foot moved 13,9 cm while planted

## Legs

- 0,90 s: stand
- 2,42 s: struck at the dummy (middle (chest), 1,67 m away; it has a sword only) with sns_chop_lunge: turn -2 degrees, lunge x0,16, the arm to make up 0,76 m
- 3,25 s: the parry: blow 1: 0,30 s to contact, aim (0,01,1,44,0,18), land (0,01,1,53,0,20), chose St George (holding none)
- 3,45 s: the parry: blow 1 read again: 0,10 s to contact, aim (-0,03,1,58,0,10), would choose St George
- 3,55 s: sns_chop_lunge at 16,0 m/s: parried by the sword (St George, crossed at 73 degrees, on its blade 0,31 from the hilt, on the attacker's 0,38) (flinch, impulse 46 N·s, weight 0,58 m/s, hit stop 0,06 s); the dummy: parried with St George, then driven back at 0,7 m/s
- 4,77 s: sns_chop_lunge: turn -2°, lunge x0,16, the arm made up 0,76 m; missed: the blade passed 40 cm from where it was aimed
- 5,22 s: the dummy strikes back with a combo: sns_thrust > sns_combo_forehand (2,03) > sns_chop_lunge (4,88)
- 5,55 s: the dummy's sns_thrust at 16,0 m/s: blocked by the shield, square on (flinch, impulse 46 N·s, weight 0,76 m/s, hit stop 0,07 s); the walker: the shield knocked back, then driven back at 0,8 m/s
- 5,90 s: the dummy's sns_combo_forehand at 10,1 m/s: swept through the left forearm (recoil, impulse 20 N·s, weight 0,38 m/s, hit stop 0,05 s); the walker: light reaction
- 5,97 s: the dummy's sns_combo_forehand at 16,0 m/s: blocked by the shield, square on (recoil, impulse 0 N·s, weight 0,00 m/s, hit stop 0,04 s); the walker: the shield knocked back
- 6,55 s: the dummy's sns_chop_lunge at 16,0 m/s: blocked by the shield, square on (stagger, impulse 46 N·s, weight 1,01 m/s, hit stop 0,14 s, the showstopper); the walker: the guard knocked open, then driven back at 0,9 m/s
- 16,47 s: combo from sns_chop_side (cores: sns_chop_side > sns_thrust (3,54) > sns_forehand (1,87), each blow cut just after its contact and the next taken up just before its own, the last the showstopper)
- 16,63 s: the parry: blow 2: 0,31 s to contact, aim (0,07,0,48,0,04), land (0,14,1,14,0,25), chose St George (holding none)
- 16,78 s: the parry: blow 2 read again: 0,20 s to contact, aim (0,10,0,64,0,09), would choose hanging
- 16,83 s: the parry: blow 2 read again: 0,17 s to contact, aim (0,04,0,79,0,15), would choose high tierce
- 16,93 s: the parry: blow 2 read again: 0,09 s to contact, aim (0,03,0,91,0,08), would choose St George
- 17,00 s: sns_chop_side at 16,0 m/s: parried by the sword (St George, crossed at 55 degrees, on its blade 0,16 from the hilt, on the attacker's 0,49) (flinch, impulse 46 N·s, weight 0,87 m/s, hit stop 0,07 s); the dummy: parried with St George, then driven back at 2,2 m/s
- 17,10 s: the combo ended
- 17,70 s: sns_chop_side: turn +50°, lunge x0,20, the arm made up 0,82 m; missed: the blade passed 18 cm from where it was aimed
- 17,97 s: combo from sns_chop_side (cores: sns_chop_side > sns_thrust (3,54) > sns_chop_lunge (3,25), each blow cut just after its contact and the next taken up just before its own, the last the showstopper)
- 18,13 s: the parry: blow 3: 0,31 s to contact, aim (-0,04,0,48,0,90), land (-0,04,0,48,0,90), chose low prime (holding none)
- 18,28 s: the parry: blow 3 read again: 0,20 s to contact, aim (-0,12,0,61,0,94), would choose septime
- 18,40 s: the parry: blow 3 read again: 0,12 s to contact, aim (-0,04,0,77,0,92), would choose septime
- 18,47 s: the parry: blow 3 read again: 0,06 s to contact, aim (-0,03,0,84,0,75), would choose low quarte
- 18,53 s: the parry: blow 3 read again: 0,00 s to contact, aim (-0,11,0,70,0,70), would choose septime
- 18,57 s: the parry: blow 3 read again: 0,00 s to contact, aim (-0,11,0,52,0,78), would choose low prime
- 18,58 s: the parry: blow 3 read again: 0,00 s to contact, aim (-0,09,0,34,0,80), would choose low prime
- 18,62 s: the parry: blow 4: 0,29 s to contact, aim (-0,11,2,06,1,49), land (-0,11,2,06,1,49), chose hanging (holding low prime)
- 18,65 s: sns_chop_side: turn +49°, lunge x1,34, the arm made up 1,09 m; missed: the blade passed 66 cm from where it was aimed
- 18,65 s: the parry: blow 4 read again: 0,24 s to contact, aim (0,15,0,88,0,48), would choose tierce
- 18,68 s: the parry: blow 4 read again: 0,21 s to contact, aim (0,16,0,86,0,31), would choose tierce
- 18,72 s: the parry: blow 4 read again: 0,18 s to contact, aim (0,05,1,02,0,25), would choose tierce
- 18,75 s: the parry: blow 4 read again: 0,15 s to contact, aim (0,03,1,20,0,24), would choose quarte
- 18,88 s: sns_thrust at 16,0 m/s: parried by the sword (hanging, crossed at 71 degrees, on its blade 0,25 from the hilt, on the attacker's 0,96; it gave 0,09 m) (flinch, impulse 46 N·s, weight 0,87 m/s, hit stop 0,07 s); the dummy: parried with hanging, then driven back at 2,2 m/s
- 18,98 s: the combo ended
- 19,13 s: sns_thrust: turn +3°, lunge x1,00, the arm made up 0,17 m; missed: the blade passed 8 cm from where it was aimed
- 19,60 s: the dummy strikes back with sns_chop_lunge, at 1,6 m: turn -5 degrees, lunge x0,28
- 19,68 s: combo from sns_chop_side (cores: sns_chop_side > sns_thrust (3,54) > sns_forehand (1,87), each blow cut just after its contact and the next taken up just before its own, the last the showstopper)
- 20,23 s: sns_chop_side at 16,0 m/s: swept through the pelvis (stagger, impulse 32 N·s, weight 0,60 m/s, hit stop 0,06 s); the dummy: its strike cut short, then driven back at 1,0 m/s
- 20,23 s: the parry: blow 5: 0,02 s to contact, aim (-0,03,1,11,0,22), land (-0,03,1,27,0,21), chose St George (holding none)
- 20,23 s: the dummy's sns_chop_lunge: missed
- 20,35 s: the parry: blow 5 read again: 0,00 s to contact, aim (-0,02,0,81,0,57), would choose low quarte
- 20,37 s: the parry: blow 5 read again: 0,00 s to contact, aim (0,00,0,61,0,69), would choose septime
- 20,40 s: the parry: blow 6: 0,29 s to contact, aim (-0,14,2,10,1,12), land (-0,14,2,10,1,12), chose hanging (holding St George)
- 20,43 s: sns_chop_side: turn +51°, lunge x1,34, the arm made up 0,61 m; swept through the pelvis (stagger, impulse 32 N·s, weight 0,60 m/s, hit stop 0,06 s); the dummy: its strike cut short, then driven back at 1,0 m/s
- 20,43 s: the parry: blow 6 read again: 0,24 s to contact, aim (0,04,0,91,0,26), would choose quarte
- 20,50 s: the parry: blow 6 read again: 0,18 s to contact, aim (0,01,1,02,0,16), would choose quarte
- 20,53 s: the parry: blow 6 read again: 0,15 s to contact, aim (0,00,1,16,0,16), would choose quarte
- 20,55 s: the parry: blow 6 read again: 0,13 s to contact, aim (0,00,1,45,0,19), would choose high tierce
- 20,57 s: the parry: blow 6 read again: 0,12 s to contact, aim (0,01,1,61,0,13), would choose high tierce
- 20,67 s: sns_thrust at 16,0 m/s: parried by the sword (hanging, crossed at 70 degrees, on its blade 0,28 from the hilt, on the attacker's 0,94; it gave 0,09 m) (stagger, impulse 46 N·s, weight 0,87 m/s, hit stop 0,07 s); the dummy: parried with hanging, then driven back at 2,2 m/s
- 20,77 s: the combo ended
- 20,92 s: sns_thrust: turn +6°, lunge x1,00, the arm made up 0,13 m; missed: the blade passed 9 cm from where it was aimed
- 21,38 s: the dummy strikes back with sns_chop_lunge, at 1,6 m: turn -4 degrees, lunge x0,53
- 22,52 s: the dummy's sns_chop_lunge at 16,0 m/s: blocked by the shield, square on (flinch, impulse 46 N·s, weight 0,51 m/s, hit stop 0,06 s); the walker: the shield knocked back
- 26,88 s: combo from sns_chop_side (cores: sns_chop_side > sns_thrust (3,54) > sns_chop_lunge (3,25), each blow cut just after its contact and the next taken up just before its own, the last the showstopper)
- 27,05 s: the parry: blow 7: 0,31 s to contact, aim (0,08,0,43,0,07), land (0,18,1,09,0,30), chose high tierce (holding none)
- 27,22 s: the parry: blow 7 read again: 0,19 s to contact, aim (0,07,0,61,0,09), would choose hanging
- 27,27 s: the parry: blow 7 read again: 0,16 s to contact, aim (0,02,0,76,0,11), would choose high quarte
- 27,35 s: the parry: blow 7 read again: 0,09 s to contact, aim (0,02,0,87,0,00), would choose high tierce
- 27,42 s: sns_chop_side at 16,0 m/s: parried by the sword (St George, crossed at 52 degrees, on its blade 0,48 from the hilt, on the attacker's 0,74) (flinch, impulse 46 N·s, weight 0,87 m/s, hit stop 0,07 s); the dummy: parried with St George, then driven back at 2,2 m/s
- 27,52 s: the combo ended
- 28,12 s: sns_chop_side: turn +60°, lunge x0,20, the arm made up 0,80 m; missed: the blade passed 8 cm from where it was aimed
- 28,58 s: the dummy strikes back with a combo: sns_combo_forehand > sns_combo_backhand (1,41) > sns_chop_lunge (3,33)
- 28,95 s: the dummy's sns_combo_forehand at 16,0 m/s: blocked by the shield, square on (flinch, impulse 46 N·s, weight 0,76 m/s, hit stop 0,07 s); the walker: the shield knocked back, then driven back at 0,7 m/s
- 29,38 s: the dummy's sns_backhand_lunge at 15,3 m/s: blocked by the shield, square on (flinch, impulse 44 N·s, weight 0,73 m/s, hit stop 0,07 s); the walker: the shield knocked back, then driven back at 0,7 m/s
- 29,88 s: the dummy's sns_chop_lunge at 16,0 m/s: swept through the head (stagger, impulse 32 N·s, weight 0,80 m/s, hit stop 0,13 s, the showstopper); the walker: heavy reaction, then driven back at 0,7 m/s
- 30,03 s: the dummy's sns_chop_lunge at 7,0 m/s: blocked by the shield, square on (recoil, impulse 0 N·s, weight 0,00 m/s, hit stop 0,13 s, the showstopper); the walker: the shield knocked back
- 33,13 s: combo from sns_chop_side (cores: sns_chop_side > sns_thrust (3,54) > sns_forehand (1,87), each blow cut just after its contact and the next taken up just before its own, the last the showstopper)
- 33,30 s: the parry: blow 8: 0,31 s to contact, aim (0,15,0,44,0,16), land (0,24,1,07,0,27), chose high tierce (holding none)
- 33,38 s: the parry: blow 8 read again: 0,25 s to contact, aim (0,13,0,50,0,06), would choose St George
- 33,47 s: the parry: blow 8 read again: 0,19 s to contact, aim (0,11,0,66,0,18), would choose St George
- 33,55 s: the parry: blow 8 read again: 0,13 s to contact, aim (0,07,0,78,0,21), would choose high tierce
- 33,68 s: sns_chop_side at 16,0 m/s: parried by the sword (St George, crossed at 78 degrees, on its blade 0,23 from the hilt, on the attacker's 0,73) (flinch, impulse 46 N·s, weight 0,87 m/s, hit stop 0,07 s); the dummy: parried with St George, then driven back at 2,1 m/s
- 33,78 s: the combo ended
- 34,37 s: sns_chop_side: turn +60°, lunge x0,20, the arm made up 0,89 m; missed: the blade passed 14 cm from where it was aimed
- 34,83 s: the dummy strikes back with sns_chop_lunge, at 1,6 m: turn -1 degrees, lunge x0,47
- 35,13 s: combo from sns_chop_side (cores: sns_chop_side > sns_thrust (3,54) > sns_chop_lunge (3,25), each blow cut just after its contact and the next taken up just before its own, the last the showstopper)
- 35,68 s: sns_chop_side at 16,0 m/s: swept through the pelvis (flinch, impulse 32 N·s, weight 0,60 m/s, hit stop 0,06 s); the dummy: its strike cut short, then driven back at 0,9 m/s
- 35,68 s: the parry: blow 9: 0,02 s to contact, aim (0,00,1,21,0,01), land (-0,02,1,50,0,12), chose St George (holding none)
- 35,68 s: the dummy's sns_chop_lunge: missed
- 35,77 s: the parry: blow 9 read again: 0,00 s to contact, aim (-0,06,1,37,0,28), would choose St George
- 35,78 s: the parry: blow 9 read again: 0,00 s to contact, aim (-0,03,1,07,0,29), would choose quarte
- 35,80 s: the parry: blow 9 read again: 0,00 s to contact, aim (-0,02,0,88,0,37), would choose low quarte
- 35,82 s: the parry: blow 9 read again: 0,00 s to contact, aim (-0,02,0,68,0,48), would choose septime
- 35,85 s: the parry: blow 10: 0,29 s to contact, aim (-0,24,2,12,1,25), land (-0,24,2,12,1,25), chose hanging (holding St George)
- 35,88 s: sns_chop_side: turn +48°, lunge x0,15, the arm made up 0,14 m; swept through the pelvis (flinch, impulse 32 N·s, weight 0,60 m/s, hit stop 0,06 s); the dummy: its strike cut short, then driven back at 0,9 m/s
- 35,88 s: the parry: blow 10 read again: 0,24 s to contact, aim (-0,01,0,96,0,36), would choose quarte
- 35,90 s: the parry: blow 10 read again: 0,23 s to contact, aim (0,11,0,90,0,18), would choose tierce
- 35,93 s: the parry: blow 10 read again: 0,20 s to contact, aim (0,04,1,18,0,11), would choose quarte
- 35,97 s: the parry: blow 10 read again: 0,16 s to contact, aim (-0,01,1,39,0,09), would choose high tierce
- 36,12 s: sns_thrust at 16,0 m/s: parried by the sword (hanging, crossed at 71 degrees, on its blade 0,29 from the hilt, on the attacker's 1,00; it gave 0,09 m) (stagger, impulse 46 N·s, weight 0,87 m/s, hit stop 0,07 s); the dummy: parried with hanging, then driven back at 2,2 m/s
- 36,22 s: the combo ended
- 36,37 s: sns_thrust: turn +9°, lunge x1,00, the arm made up 0,10 m; missed: the blade passed 9 cm from where it was aimed
- 36,83 s: the dummy strikes back with sns_chop_lunge, at 1,6 m: turn -4 degrees, lunge x0,45
- 37,97 s: the dummy's sns_chop_lunge at 16,0 m/s: blocked by the shield, square on (flinch, impulse 46 N·s, weight 0,51 m/s, hit stop 0,06 s); the walker: the shield knocked back
- 39,33 s: combo from sns_chop_side (cores: sns_chop_side > sns_thrust (3,54) > sns_forehand (1,87), each blow cut just after its contact and the next taken up just before its own, the last the showstopper)
- 39,50 s: the parry: blow 11: 0,31 s to contact, aim (0,10,0,52,0,03), land (0,06,1,29,0,28), chose St George (holding none)
- 39,67 s: the parry: blow 11 read again: 0,19 s to contact, aim (0,08,0,69,0,10), would choose hanging
- 39,73 s: the parry: blow 11 read again: 0,14 s to contact, aim (0,01,0,82,0,13), would choose high tierce
- 39,82 s: the parry: blow 11 read again: 0,08 s to contact, aim (0,04,0,90,0,03), would choose St George
- 39,87 s: sns_chop_side at 16,0 m/s: parried by the sword (St George, crossed at 66 degrees, on its blade 0,22 from the hilt, on the attacker's 0,55) (flinch, impulse 46 N·s, weight 0,87 m/s, hit stop 0,07 s); the dummy: parried with St George, then driven back at 2,2 m/s
- 39,97 s: the combo ended
- 40,57 s: sns_chop_side: turn +51°, lunge x0,20, the arm made up 0,79 m; missed: the blade passed 13 cm from where it was aimed
- 41,03 s: the dummy strikes back with a combo: sns_combo_forehand > sns_combo_backhand (1,41) > sns_chop_lunge (3,33)
- 41,40 s: the dummy's sns_combo_forehand at 16,0 m/s: blocked by the shield, square on (flinch, impulse 46 N·s, weight 0,76 m/s, hit stop 0,07 s); the walker: the shield knocked back, then driven back at 0,7 m/s
- 41,82 s: the dummy's sns_backhand_lunge at 16,0 m/s: swept through the pelvis (flinch, impulse 32 N·s, weight 0,60 m/s, hit stop 0,06 s); the walker: body reaction, then driven back at 1,7 m/s
- 41,90 s: the dummy's sns_backhand_lunge at 15,1 m/s: blocked by the shield, square on (recoil, impulse 0 N·s, weight 0,00 m/s, hit stop 0,04 s); the walker: the shield knocked back
- 42,38 s: the dummy's sns_chop_lunge at 16,0 m/s: swept through the head (stagger, impulse 32 N·s, weight 0,80 m/s, hit stop 0,13 s, the showstopper); the walker: heavy reaction, then driven back at 0,7 m/s
- 42,53 s: the dummy's sns_chop_lunge at 6,7 m/s: blocked by the shield, square on (recoil, impulse 0 N·s, weight 0,00 m/s, hit stop 0,13 s, the showstopper); the walker: the shield knocked back
