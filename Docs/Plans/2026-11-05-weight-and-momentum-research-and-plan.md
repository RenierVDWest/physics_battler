# Weight and momentum: research and plan

> Copy of the Claude document <https://claude.ai/artifact/CvCKHzT7jXkHXDSntCJaHa>, taken on 18 November 2026. The document is the
> live version; this copy may be behind it. Pictures and embedded charts are left out of the copy.

Nov 5, 2026 · @Renier

## What the playtest showed

The Large's 2nd and 3rd club blows go unparried because the parry never sees them as new blows. On top of that, the game carries a blow's weight in its numbers, but the look throws most of it away.

**In the logs.** Against the expert sword dummy, the Large's combos got the first blow parried or glanced, then cut through on most of the rest. The Medium's combos show the same pattern headless: whenever its opening forehand glances off, the low backhand that follows goes through the pelvis, at skill 0.5 and at skill 1 alike.

**Cause 1: a combo's next blow isn't read as a new blow.** The parry starts a new read only when the blow's time-into-the-clip drops. A combo's follow-on starts partway into its own clip, so that time starts high and never drops. The parry keeps its last parry, and with contact under 0.3 s away that parry is locked. The shield uses the same test.

**Cause 2 (to confirm): the hit knocks the parry out of place.** Once a blow glances or lands, the dummy reels and is driven back at 1.4–2.2 m/s. The hit's lean and arm knock are put on the pose after the parry has placed the sword, which moves the blade off its line for the next blow.

**A club cuts through bodies.** One Large club blow struck the head, upper arm and pelvis in turn. The cleave rule lets any blow carry on when the attacker far outweighs the struck body: 280 kg against 80 kg keeps 78% of the swing. A club should stop in the body and shove it, not sweep through its parts.

**Weight is in the numbers but not in the look.** The Large's blow carries 172 N·s against a Medium sword's 32, over 5 times as much. But the lean, head snap, arm knock and drive-back all hit their ceilings (0.6 rad, 0.12 m, 2.2 m/s), so both blows read the same. And the attacker's body supplies most of a blow's effective mass, so a Medium's club is only 1.2 times its sword's momentum.

## Research: how weight shows

A heavy weapon is not met the way a light one is: a skilled defender turns it aside, takes it near the hilt, and gives ground. Only a novice tries to stop it square on the blade.

**Where the weight sits.** A one-handed sword balances about 5–10 cm from the guard, and its centre of percussion sits about two-thirds of the way down the blade. Mass further from the hand feels heavier and hits harder ([BladeForums](https://www.bladeforums.com/threads/understanding-sword-balance-distal-taper-point-of-balance-and-center-of-percussion.2046798)). A club or mace puts nearly all its mass in the head. From that it follows, though the source doesn't say so, that nearly all a club's mass arrives at the strike, against a fraction of a sword's.

**Two ways to meet a blow.** Meyer's parries either oppose a blow head-on, "direct opposition to the line of force", or redirect it with "minimal force perpendicular to the line" of the attack. The strong of the blade against the weak of the other's gives the most leverage ([Scholar Victoria on Meyer](https://scholarvictoria.com/2015/12/23/meyers-longsword-on-parrying)).

**Against a strong cut.** A cutting sword defends mostly by evading and deflecting. A strong cut that must be resisted directly is taken on the flat. Holding the edge out to stop a heavy blow costs the counter and leaves an opening ([ARMA, on parrying](https://thearma.org/essays/parrying2.htm)).

**What this means for the game:**

- A clean hit with a club rocks and shoves the body far more than a sword's, which cuts in and pushes less.
- A shield takes a club's weight into the arm and the body: the shield is driven back, the body rocks back, and the feet give ground.
- A sword parrying a club deflects it rather than stopping it, takes it near the hilt, and steps back to meet it early in its arc. Its arm visibly gives with the blow.
- A sword that meets a heavy club square on, far from its hilt, is beaten: the club goes on, slowed.

## How the game handles weight today

One number, the blow's momentum, is worked out at every contact. Each part that reacts to it then saturates early or ignores it.

| Where | What it does with the blow's weight | Where weight is lost |
| --- | --- | --- |
| Damage rules | Momentum = effective mass × speed. The effective mass is the weapon's own mass plus 2.9% of the attacker's body. A club's head puts 0.9 of it into the body, an edge 0.55, a point 0.45. Poise damage follows. | The weapon's mass is small next to the body's share: a Medium's sword is 3.6 kg effective, its club 4.3 kg. |
| Cleaving | A blow goes on through the body when the attacker outweighs it. | A club cleaves like a blade: it should stop in the body and shove it. |
| Hit finder | A combo's blows knock 1.5×, its showstopper 2×; a glance keeps half; a slanted shield turns some aside. The hit stop grows a little with the blow, capped at 0.1 s (0.17 s for a showstopper). | The hit stop barely tells a club from a sword. |
| Hit reactions | The upper body leans and the head snaps, on curves that flatten out for big blows. The reaction clip is chosen by the blow per kilogram. The body is driven back, capped at 2.2 m/s. | A Medium's sword and the Large's club both reach the ceiling. |
| Shield block | The shield arm is knocked aside, the body rocked a little and driven back. | The arm's knock flattens out the same way. A heavy blow never opens the guard. |
| Sword parry | The parrying arm is knocked aside the same way. A clean parry stops the blow; one taken on the weak of the blade, or crossing flat, glances on at half speed. | Nothing makes a heavy blow harder to stop, or beats the parry. The parry never gives ground. |
| Attacker | A stopped blow throws the hand back, at most 0.18 m. | A club met by a sword rebounds as a sword does. |
| The fight | Stumbles from 0.8 m/s of knock, knock-downs from 2.2, launches from 4. | Not yet joined to the clip fighters (the main plan's step 5). |

## The design: one weight, felt everywhere

Every contact gets one measure, the **blow's weight**: the momentum the struck body takes, over its mass, as the speed it would give the whole body (m/s). It is worked out once, where the hit is resolved. Every reaction is then scaled by it, with ceilings high enough that a heavy blow still looks heavier.

**Where the weight comes from.**

- Each weapon gets a strike share: how much of its own mass arrives where it strikes. A club's head carries about 0.9 of the club; a sword at its sweet spot about 0.5; a thrust's point less.
- The body behind the blow stays as it is (2.9% of the attacker's mass), so a Large's swing is still felt. But the weapon now counts for more: at the same body, a club lands about twice as heavy as a sword.
- A club never cleaves. It stops in the first part it meets and its momentum goes into the body: a shove, not a cut through. Edges cleave as now.

**What each blow weighs** (on the logs' figures; the Medium-club row is the proposal's estimate):

| Blow | Momentum into the body | Weight on the struck body | Felt as |
| --- | --- | --- | --- |
| Medium's sword on a Medium | 32 N·s | 0.40 m/s | Rocked: a lean and a head snap, a small step back |
| Medium's club on a Medium | about 65 N·s | about 0.8 m/s | Staggered: thrown back, a catch step |
| Large's club on a Medium | 172 N·s | 2.15 m/s | Thrown back stumbling (floored in the fight) |
| Medium's sword on a Large | 32 N·s | 0.11 m/s | Barely moves it |
| Large's club on a Large | 172 N·s | 0.61 m/s | Rocked hard |

**Felt in a clean hit.** The lean, head snap and the struck part's knock grow with the weight. The reaction clip goes light, body, then heavy. The drive-back follows the weight, with a ceiling past the fight's stumble speed. A blow past that speed makes the body stumble back with catch steps.

**Felt in a shield block.** The shield takes most of the blow, less what a slanted shield turns aside. The shield arm is driven back, the body rocks back, and the feet give ground, all by the weight. A blow heavy enough knocks the shield aside and leaves the guard open for a moment. With the poise gone it breaks the guard, as the fight's guard breaks do.

**Felt in a parry.** The sword arm gives with the blow: the hand is driven back along it, the elbow bends, and the body turns and steps with it.

- A parry on the forte stops a blow up to a strength set by skill and by where on the blade it lands.
- A heavier blow taken square on beats the parry: it goes on into the body, slowed by what the sword took.
- A heavy blow taken at a slant glances off instead, which is how an expert meets a club.

**Felt by the attacker.** A light blade that is stopped rebounds sharply. A heavy club rebounds less and drags on through. The hit stop grows with the weight, from about 0.06 s for a sword to about 0.15 s for the Large's club.

## Parrying heavy blows

The parry must meet every blow of a combo, take a club's weight in the arm, and step back to be where it can meet it.

**1. Every blow read as new.** A blow is new when the attacker's strike clip changes, not when its time into the clip drops. The parry then reads each follow-on afresh, and it can still change its pick until contact is close. The shield gets the same fix.

**2. The parry kept on its line.** A hit's lean and arm knock no longer move the sword after the parry has placed it. The knock goes into the parry itself: its hand is pushed back by the blow's weight and comes back at the hand's speed. The next parry starts from where the knock left the arm.

**3. Stepping back to parry ("giving ground").** The defender retires a quick step along the attacker's line, snapped in as the combo's magnet is. The walk's step-in runs backwards: about 0.1 s and 0.2–0.5 m at a Medium's size, timed to land just before contact. It steps back when:

- the blow will land too deep, inside the reach of its forte;
- the blow is heavy, so meeting it earlier in its arc, further out, takes it before it is at full speed on the body;
- its arm can't get to the blade in time from where it is.

An expert steps early and the right amount. A novice steps late, too little, or not at all. A shield fighter gives ground the same way against a heavy blow. The fight's shell already gives ground under each blow, so this is that rule made physical.

**4. The arm taking the weight.** At a parry, the hand is driven back along the blow by its weight. That is much further for the Large's club than for a sword, with a higher ceiling than today's 0.12 m. The elbow bends, the shoulders turn with it, and the body is pushed back a step. A clean parry of a sword stops crisply; a parried club visibly drives the defender back.

## The steps

Five steps, all in the lab, each gated headless and then by your playtest. They run before the main plan's step 4, the fight, which then takes them over. The rules live in the clip fighter and the damage rules, so the fight gets them as they are.

1. **Every blow of a combo met.** New blows are read by the clip changing, for the parry and the shield. The hit's knock goes into the parry instead of moving it.
   - Gate (headless): against the expert sword dummy, the follow-ons are parried, including the Medium's low backhand after a glance (0 of 5 today) and all of the Large's.
   - Gate (you): the Large's combo at the sword dummy.
   - **Done (5 November), headless gate passed; your playtest next.** Fixing the read wasn't enough: a combo's next blow came into view only 0.15 s before contact, too late for the arm to get from a high parry to a low one. So the parry now reads a combo's next blow ahead, out of the last one's follow-through, planned as the strike will be played. That gives about 0.25 s of warning. A parry taken on that early read gets one more look when the real blow comes into view, and the parry held from the last blow stays only if it actually meets the new blade. The knock goes into the parry. Against the expert sword dummy, the Medium's 42 combo blows: 26 parried, 7 glanced, 9 through (was 20, 8, 13); the Large's club 9 of 9 parried. Its low backhand after a glance still gets through more often than not. Stepping back is wired in but held back: stepping back on any slight lateness moved the body away from where the arm was putting the sword, and more blows got through (22 parried, 11 through). It now steps back only when the arm would be 0.05 s late, which the early read means it never is in these runs. The visible step back against heavy blows comes in step 5.
2. **The blow's weight.** Weapons get their strike share; a club stops in the body instead of cleaving; every contact carries one weight figure into the log.
   - Gate (headless): the weight table above for each body and weapon. Combos still land as now.
   - **Done (5 November), headless gate passed; your playtest next.** The strike share was already there: a club's head passes 0.9 of its momentum into the body, an edge 0.55, so a club already lands about twice as heavy as a sword. What changed: a blunt blow (a club's head, a fist, a shield's face) never cleaves, and stops in the first body it meets, in a combo too. Every contact logs its weight. Single blows on the open Medium: a Medium's sword weighs 0.37 m/s (median), its club 0.65, the Large's club 1.24; the Large's club went through a body 0 times (was 10). The fight's own swing still cleaves until it takes these rules up. One cost: the Large's combos on the open dummy now land 2 of 3. Its backhand stops in the body before turning him as its sweep through did, so the forehand after it needs a 67° turn, and an aimed turn goes to 60° at most.
3. **Weight in a clean hit.** The lean, head snap, part knock, reaction clip, drive-back and stumble steps all follow the weight. The hit stop grows with it.
   - Gate (headless): a Medium's club drives a Medium back about twice as far as its sword. The Large's club throws it back stumbling.
   - Gate (you): club against sword on the same dummy.
   - **Done (5 November), headless gate and your playtest passed.** The hit stop is now 0.03 s plus 0.05 s per m/s of weight (0.04 to 0.16 s). The drive-back is stronger and caps at 2.2 m/s. From 1 m/s of weight, the body skips its reaction clip and is thrown back stumbling at once (at most 2.8 m/s), and it can't strike for 0.7 s while it finds its feet. Single blows on the open Medium: a Medium's sword stops it 0.05 s and moves it 0.22 m; its club 0.07 s and 0.44 m (twice the sword); the Large's club 0.10 s and 0.97 m, every blow a stumble. All of it is on Combat Feel, under "Hit stop" and "Driven back". Parry and shield counts held or improved. Light blows now get a shorter stop than before: a sword thrust to the head went from 0.07 s to 0.04 s.
4. **Weight in a block and a parry.** The shield arm and body are rocked by the weight, and a heavy blow knocks the guard open. The parry arm gives with the blow; a square parry against a much heavier blow is beaten; the attacker's rebound follows its weight.
   - Gate (headless): shield and parry rates per skill against the Medium and the Large.
   - Gate (you): playtest.
   - **Done (5 November), headless gate passed; your playtest next.** A held parry first gives along the blow by its weight, with the attacking weapon staying on the blade, then sweeps it aside. A Medium's sword drives it back 0.05 m, its club 0.06, the Large's club up to 0.12. It never gives more than half the way to the body. A square parry holds blows up to 0.8 m/s of weight for a novice and 2 for an expert; a combo's final blow counts double. A heavier blow beats it: the blade is crushed back into the defender at the blow's pace, 60% of the blow lands on what it is crushed into, and the combo goes on. A glance is never beaten. The shield arm is knocked back by the blow's weight. From 1 m/s on the shield, the guard is knocked open: the arm is thrown wide and comes back slowly, and the shield meets nothing for 0.5 s. A blow stopped on a shield or a blade throws the attacker's hand back by its weight and jars its upper body. Headless, parry and shield rates are unchanged except against the Large: a novice's parries of its combos are beaten (3 of 3), and its single blows knock a mid-skill shield's guard open every time. The parrying hand's fastest speed fell from 20 to 15 m/s. Every value is on Combat Feel, each with a tooltip.
5. **Giving ground.** The parry and the shield step back against deep or heavy blows, by skill, snapped in like the magnet. The feet stay planted between the steps.
   - Gate (headless): the expert parries the Large's combos; planted feet stay within 2 cm.
   - Gate (you): playtest.
   - **Done (6 November), headless; your playtest next.** A shield steps back about 0.22 s before contact from a blow it reads coming at least 0.9 m/s heavy, 0.25 to 0.5 m by how heavy. It meets the blow further out, up to 30% lighter. This happens for every one of the Large's blows and the Medium's combo blows, never for the Medium's single blows or the Small's. A parry steps back only when its arm would be late (as before) or when the blow would beat it. Stepping back from lighter or deep blows took the body away from where its arm was putting the sword, and 2 more of the Large's single blows got through. Depth is kept on Combat Feel, off by default. Headless at skill 1: the Large's combos were blocked 9 of 9 (were 6, with 3 missed), the parries are as before, and the defender's planted feet stay within 0.2 cm (the gate is 2 cm). All six settings are on Combat Feel under "Giving ground".

## Questions

**Renier's answers (5 November): all four as recommended.** Heavy blows can beat a parry. In the lab heavy blows make the body stumble, not fall. The weapon counts for more. Step 1 goes first, on its own.

**Pinned for later: controlled falls.** A death can go limp, but someone whose legs are knocked out, or who is floored by a club, should fall under control, not as a ragdoll. It is a deep dive of its own, to be brought back when the fight's knock-downs come in.

1. **Can a heavy blow beat a parry?** Recommended: yes. A club much heavier than the parry can stop, taken square on, goes on into the body, slowed. A skilled defender avoids that by deflecting and stepping back. A novice gets beaten.
2. **Can one clean heavy blow floor someone in the lab?** The Large's club lands at 2.15 m/s on a Medium, the fight's knock-down speed. Leg falls were shelved because the fall and get-up looked wrong. Recommended: in the lab, heavy blows make the body stumble back with catch steps, not fall. Falls come with the fight's own balance rules and get-up in the main plan's step 5.
3. **Should the weapon count for more against the body behind it?** Today a Medium's club is only 1.2× its sword's momentum. Recommended: yes. Each weapon's strike share makes a club land about twice as heavy as a sword at the same body; the body behind it still counts, so the Large stays the heaviest hitter.
4. **Order.** Recommended: step 1 (every combo blow met) first and on its own, since it is a bug, then the weight steps in order.

## Sources

- [Understanding sword balance: distal taper, point of balance and centre of percussion](https://www.bladeforums.com/threads/understanding-sword-balance-distal-taper-point-of-balance-and-center-of-percussion.2046798) (BladeForums)
- [Meyer's longsword: on parrying](https://scholarvictoria.com/2015/12/23/meyers-longsword-on-parrying) (Scholar Victoria)
- [On parrying](https://thearma.org/essays/parrying2.htm) (ARMA)
