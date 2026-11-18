# Plan: straight wrists, a close shield, and hits that land softly

> Copy of the Claude document <https://claude.ai/artifact/UJNwdKChsM7vidDhArKF9W>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 1, 2026 · @Renier

## What you saw, and what done looks like

Five problems from the M9b playtest. Each one becomes a goal we can measure.

| # | What you saw | Done when |
| --- | --- | --- |
| 1 | Wrists bent hard to point the weapon: the hand cocks back, sideways or over, while the arm stays where it was | The arm points the weapon and the wrist stays near straight, as in the reference attack FBXs, through guards, wind-ups, strikes and follow-throughs |
| 2 | The shield bobs and turns in all sorts of ways, and passes through enemy bodies | The shield moves only when the threat moves, eased, and never passes through a body (its own or an enemy's) |
| 3 | The shield is held too far out in front | The shield sits close in front of the chest and shoulder, the shield arm bent, as in your "Be here" sketch |
| 4 | A struck limb or shield buzzes like a flicked antenna: 5 to 10 turns in under a second | A struck limb or shield is knocked aside once, then comes back in one smooth return (no back-and-forth) |
| 5 | A strong blow that bounces off a shield throws the arm into an unnatural position | Recoil stays within the joints' limits and the arm recovers into its guard |

## Why it happens

All five come from three design choices in the code, not from tuning. The numbers come from your logs `12-02-20` and `12-04-47` (standing units, physical body), and from 10 of your reference attack FBXs, measured the same way.

**1. The weapon decides the hand, and the wrist absorbs the rest.** A blow or guard sets where the grip goes and which way the blade points (out from a pivot in the chest, edge leading). The arm IK only reaches the hand to the grip, and the wrist is twisted to whatever the blade needs. In a blow the wrist clamp is switched off, so the blade always wins. The swing model points the blade straight out from the chest, along the arm. But our grip holds the blade square across the fist, so the wrist must bend about 90 degrees to put the blade in line with the arm.

**2. The physical arm is held by velocity pinning, with its joints free.** Each step, every arm part's velocity is set to land on its pose, blended with its current velocity by the pin (0.7 for arms). That is an under-damped servo. At pin 0.7 it rings at about 9 Hz, and at 0.15 at about 4 Hz. After a hit the pin drops and then ramps back up, so the ring's pitch rises through the recovery: your antenna buzz. While pinned, the arm joints have no limits at all (by design, so they don't fight the pose). Whatever an impulse does to the arm, nothing physical stops it.

**3. Weapons and shields have no body.** They are trigger colliders on the hand, with no mass. A 4 kg shield adds nothing to the 0.7 kg hand, so a blow on the shield spins a hand that is far too light. The shield cannot touch an enemy, so it passes through them. Where the shield goes is computed from the threat each step (guard, track the enemy's weapon, block, raise), plus the rhythm's sway, so it moves whenever anything moves. It is held at 0.88 of the arm's reach.

| Problem | Cause | Measured |
| --- | --- | --- |
| Bent wrists | 1 | Weapon wrist bend (excluding roll), median / 95th: blows 101–122° / 145–167°, guard 36–56° / 64–97°. Reference attacks: 14–34° / 29–68°, median about 23° / 45°. The weapon wrist is the joint past its limits in 95% of blow steps where the pose is past them, and the pose is past them in 94% of blow steps. |
| Shield bobbing | 3 | With no hit in the last second, the shield face turns at 64–102°/s median, 1,000–1,350°/s at the 99th percentile |
| Shield through enemies | 3 | Shield colliders are triggers; nothing stops them |
| Shield too far out | 3 | Shield hand 0.67 m from the chest (medium, median) |
| Antenna buzz | 2, 3 | 1 s after a blocked blow, the shield hand reverses its turn 8 times (median, up to 12), swings up to 58° off its pose (median, up to 169°) and spins at up to 92 rad/s. Struck parts: 5 reversals in 1 s. |
| Recoil out of control | 1, 2 | After a blow stopped by a body or shield, the weapon hand is up to 0.92 m from its pose and up to 180° turned from it. The stopped blow also plays backwards along its own path ("rewind"), which can drag the arm back into its wind-up. |

## Fix 1: the arm points the weapon

We turn the order around. The arm's pose points the weapon, and the wrist stays near straight: it only adds what the reference swings show a wrist adding.

1. **A real grip.** A handle crosses the fist diagonally, so at a straight wrist the blade leans toward the forearm's line instead of standing square to it. Start at 20 degrees of lean, checked against a picture sheet of each body holding each weapon. The wrist's own range is narrowed to what a gripping wrist does: toward the little finger 30°, toward the thumb 15°, bend and extend 40°. The forearm rolls up to 80° each way, as now.
2. **One arm solver for guards and blows.** It is a small iterative IK over the whole arm: the shoulder (3 axes), the elbow, the forearm's roll and the wrist (2 axes). It meets its goals in this order:
   1. The striking point lands exactly on its path. This is the only hard goal, so blows still hit what they aim at.
   2. The blade points close to the way the move asks.
   3. The edge leads.
   4. The wrist stays near straight: cheap up to about 15°, steeply dearer after, and never past its limits.
   5. The elbow stays down and out, and changes little from the last step (no flips).
3. **Swings an arm can make.** The swing builder no longer forces the blade straight out from the chest all the way round. Through the wind-up and the follow-through, the blade points where the arm's turn carries it. It lines up toward the chest's radius only around contact, where the reference swings also extend it.
4. **Guards by the arm.** Each style's guard is restated as where the hand is and which way the forearm points, and the blade follows. A sword guard holds the forearm forward and up, with the blade up and forward. The guard data is retuned against the reference idles and attacks.
5. **Same pose everywhere.** The blow planner's trial poses use the same solver, so a blow is only chosen if the real arm can make it. The clearance checks (blade against its own body) run on that pose.
6. **Wrist and elbow limits stay on in the physical arm**, also while pinned, once the pose keeps inside them. (See Fix 4.)

The reference FBXs become the yardstick, not decoration. A new runner and the log analysis report wrist bend in the same terms as the references.

Considered and left for later: driving the arm directly from the reference attack curves (retargeted mocap), with IK only correcting the aim. It gives the most natural swings but replaces the move system. It fits better with M10 (combos).

## Fix 2: the shield sits close, moves calmly, and is solid

The shield becomes a heavy, solid thing held close to the body. It turns only when the threat really moves.

**Close to the chest.** The guard is built from the arm, not from a distance out:

- The elbow is tucked by the ribs and bent about 90°. The forearm points forward and the shield faces where the forearm points (centre grip).
- The shield hand sits about one forearm's length in front of the elbow: 0.30–0.40 m from the chest for the medium body (now 0.67 m), scaled by size. This is your "Be here".
- Raised (High): the elbow lifts and the forearm points forward and up, with the shield at eye height, still close.
- Blocking: the shield meets the blow by turning the chest and forearm first. The elbow opens to at most about 130°. Your Sword and Shield reference keeps the shield elbow at 65° (median) to 110° (95th).

**Calm aim.**

- The shield aims at a threat direction that changes only when the threat moves more than about 12°. A new aim holds for at least 0.3 s.
- Aim changes follow a critically damped spring (it arrives without overshoot), at most 250°/s outside a block.
- Tracking the enemy's weapon only applies to a wind-up that is in striking range and clearly seen. Otherwise the shield stays in its guard.
- The rhythm's sway on the shield arm drops to a few degrees and centimetres.
- Shield states (guard, track, block, raised) hold for at least 0.25 s before changing.

**Solid.**

- **Placement:** where the shield's pose would cut into an enemy body or weapon within reach, it slides out along its face or turns edge-on. It already does this for its own body.
- **Physics:** the shield gets a real collider on the hand's body (not a trigger), so it touches enemy bodies and weapons. Contacts between the shield and a body are capped (the same contact modification as the bodies on the ground), so the shield stops against a body without shoving it across the board.
- **Weight:** the hand's body carries the shield's mass and inertia (0.7 kg hand + 4 kg shield). A blow on the shield moves a heavy thing, not a light hand.

Blows are still detected by the sweep, as now, so blocking and hits keep working while the shield is made solid.

## Fix 3: a hit knocks the limb aside once

The pinning is replaced by a critically damped servo, and the visible knock moves into the pose. A struck arm or shield is whacked out of the way once and swings back in one smooth return, with no back-and-forth.

**A servo that cannot ring.** Each step the part's velocity is set by a spring and damper toward its pose, solved implicitly (stable at any stiffness). It also takes the pose's own velocity, so it follows a moving pose without having to be stiff. With damping ratio ζ = 1 it never overshoots, whatever the stiffness. For a part's position (the turn is the same):

```latex
v_{n+1} = v^{pose} + \frac{(v_n - v^{pose}) + h\,\omega^2 e_n}{1 + 2\zeta h \omega + h^2 \omega^2}
```

Here e is the part's distance from its pose, h the physics step, ω = 2π f the stiffness, and ζ = 1. Each region gets its own f. Arms hold at about 6 Hz. While an arm or shield is recovering from a hit it drops to about 2 Hz and eases back over 0.4 s, staying critically damped all the way, so the return is slow and soft rather than a buzz. The pins in the unit profiles become these frequencies.

**The knock lives in the pose.** As the body's recoil already does for the hips, chest and head, a hit on an arm or shield kicks a critically damped spring in the arm's pose (about 2.5 Hz). It moves the hand back along the blow and turns it with the blow's lever, then returns. Its size saturates with the impulse: up to about 25° and 12 cm (medium, scaled by size). The physical impulse into the part is capped low, about 2 m/s on a hand. A blow on the shield is spread over the hand, forearm, upper arm and chest by their mass, the shield's 4 kg included. What you see is the designed knock; the physics only adds weight to it.

Done when, in the second after a hit: the struck part turns back at most once (now 5 for a body part, 8 for the shield hand), sits at most 30° off its pose (now up to 169°), and spins at most 15 rad/s (now up to 92).

## Fix 4: recoil that stays in control

A blow that bounces off something springs back a short, bounded way and settles into the guard, with the arm inside its limits throughout.

- **A designed rebound instead of a rewind.** A stopped blow no longer plays backwards along its own path, which can carry the arm back into its wind-up. The weapon springs back along the reverse of its travel: 15–25 cm (more for a harder blow and a harder stop), critically damped. The arm solver keeps the wrist straight through it, and the pose blends into the guard over about 0.3 s.
- **The attacker feels it in the pose too.** The blow's push back up the weapon arm uses the same pose knock as Fix 3, so it is bounded and smooth.
- **No switch at contact.** The weapon arm is no longer switched from kinematic (during the blow) to pinned (when it stops). That switch happens exactly when the arm is struck hardest. The servo holds it in both.
- **Joints keep their limits while standing.** The arms' anatomical limits (shoulder, elbow hinge, wrist) stay on in the physical body all the time, a few degrees wider than the pose's own. On the ground the ragdoll's limits apply, as now. This is only safe once the pose stays inside them (Fix 1); a check in the log flags any step where it does not.
- **No flips.** The servo turns each part the short way from where it is now. A pose that jumps by more than 120° in one step (a blend glitch) is eased in rather than followed.

Done when, after a blow stopped by a body or shield, the weapon hand stays within 0.2 m and 40° of its pose (now up to 0.92 m and 180°). While standing, no physical joint is more than 10° past its limits (now the wrist in 94% of blow steps).

## How we'll know

Every problem gets a number, measured the same way here (on the pose) and in your playtest logs (on the physical body). The physics side can only be judged from your logs, because my test setup has no physics joints.

| Problem | Measure | Now | Target | Checked |
| --- | --- | --- | --- | --- |
| Bent wrists | Weapon wrist bend, median / 95th (roll excluded), as in the references | Blows 110° / 155°; guard 50° / 80° | ≤ 25° / ≤ 45° (references: 23° / 45°) | Here (new `wrists` runner, every move and guard) and your logs |
| Bent wrists | Pose steps with a joint past its limits | 94% of blow steps | 0% | Here and your logs |
| Shield bobbing | Shield face turning with no hit in the last second, median / 99th | 64–102°/s / 1,000–1,350°/s | ≤ 20°/s / ≤ 250°/s | Here (`shield` runner) and your logs |
| Shield too far out | Shield hand to chest, medium | 0.67 m | 0.30–0.40 m | Here and your logs |
| Shield through enemies | Steps where the shield is inside an enemy body | Not measured (triggers) | 0, beyond 2 cm | Here (pose) and your logs (new column) |
| Antenna buzz | Turn reversals of the struck part in 1 s after a hit; off its pose; spin | 5–8; up to 169°; up to 92 rad/s | ≤ 1; ≤ 30°; ≤ 15 rad/s | Servo maths here (tests); the rest in your logs |
| Recoil | Weapon hand off its pose after a stopped blow | Up to 0.92 m and 180° | ≤ 0.2 m and ≤ 40° | Your logs |
| Recoil | Physical joints more than 10° past their limits while standing | Wrist in 94% of blow steps | Never | Your logs |

The log analysis gets a section for these (`analyze.py` and a standalone script), and also measures older logs, so each playtest can be set against this one. Tests pin the pose-side targets so they cannot quietly regress. The sandbox gets a **shield test**: a sword and a club take turns striking a shield-holder's shield, for you to judge the knock and the return by eye.

## Order of work

This becomes milestone 9c, in two pushes with a playtest after each. The old M9c (solid swords and clubs, getting up with a shield) moves to 9d. The shield's solidity comes forward into this one.

**Push 1: calm and soft** (low risk; you can judge the buzz early)

1. The measures first: the `wrists` runner, the wobble, shield and recoil analysis, and today's numbers as the baseline.
2. The critically damped servo and the pose knock on arms and shield (Fix 3).
3. The shield held close and aimed calmly (Fix 2, the first two parts).
4. The sandbox shield test, tests, README; push; your playtest.

**Push 2: straight wrists and solid shield** (the big one)

5. The grip angle, wrist ranges and the arm solver for guards and blows, with swings and guards rebuilt on it (Fix 1). The aim matrix must still show 0 misses.
6. The physical limits stay on while standing, the designed rebound, and no switch at contact (Fix 4).
7. The solid, weighted shield and its placement against enemy bodies (Fix 2, the last part).
8. Picture sheets of each body holding each weapon in guard and through each blow, set beside the reference frames; tests, README; push; your playtest.

One decision for you: the grip lean (step 5) starts at 20°. I'll show it on the picture sheet before building the swings on it.
