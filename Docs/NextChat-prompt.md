Pick up my Unity project "Physics Battler". Repository `RenierVDWest/physics_battler`, branch `physical-body`. You
have my permission to push to that branch.

## Start

1. Attach the repository with push access and clone it. (On 19 November the first ask for push access was refused
   and went through when asked again after I said "you may push" in the chat. If it is refused, tell me at once and
   carry on reading.) Read `CLAUDE.md` at its top first and follow it: it is the briefing, and it is up to date as
   of 22 November 2026, 08:20 (newer than the rest of this prompt: where the two disagree, CLAUDE.md and the
   README's last entries are right). On the morning of 22 November, after my look, three things were built blind,
   headless only, and wait for my look: my own combo no longer fells me (README "### 22 November, 07:20"), the
   clothes rebuilt as one open robe whose sums run on other cores in a Burst job, and the shock wave's jolt beside the
   jelly (both "### 22 November, 08:20"). **`ClothJob.cs` (the Burst job) has never been compiled headless: if my
   Unity reports a compile error, mend that first.** `README.md` is
   the build log (over 9,000 lines: search it, never read it whole, and add entries at its end, never write over
   it).
2. Read the plans for the work in hand: `Docs/Plans/2026-11-21-shock-waves-through-the-body-research-and-plan.md`
   (live https://claude.ai/artifact/FED9yrpFr7tz59LvbJLnd6) and
   `Docs/Plans/2026-11-21-cloth-that-moves-samurai-clothes-research-and-plan.md` (live
   https://claude.ai/artifact/3Jev6uXAQk3Nsncq7GCtAh), and the progress notes at their ends. Add your own progress
   notes there with the Docs tools (insert at the end), and to the copies in the repository. What comes after them is
   in CLAUDE.md ("What he was told is next" and the roadmap).
3. Set up the headless build: `dotnet --version || apt-get install -y dotnet-sdk-8.0`, then
   `cd Tools/CompileCheck && ./get-physx.sh`, then `Tools/Headless/b.sh` in a command by itself (it wants
   "0 Error(s)" four times). `export S=/tmp/pb2` for the run outputs. Read `Tools/Headless/README.md`.

## How I want the work done (all of it is in CLAUDE.md; the parts that matter most)

- Talk to me simply, as if I'm not smart and not following things closely. Bottom line first. Short.
- One step at a time. Each step: a Combat Feel switch with a tooltip, an old/new toggle or button in the lab,
  measured headless, a README entry, a note in the plan (live and copy), CLAUDE.md brought up to date, a commit and
  a push.
- Every push that needs my eye: set the panel's "Look here" list (`BattleSandbox.cs`: `LookTag`, `LookSteps`,
  `LookHere`) so the debug panel shows the steps and puts yellow frames on what I should press, and check it with
  the panel test (it prints a `LOOK HERE` line: every name must be drawn).
- Every finished and pushed step that needs my look, and every document: a "3b2b" explainer first, made by a
  subagent on Opus at medium effort, never on the main model. The brief that works is in
  `Tools/Explainer/brief-template.md`; the kit is in `Tools/Explainer/kit/`. Link first, short note after. Small
  fixes in ordinary back-and-forth need no film. No other subagents.
- Be honest about what is unverified. Headless numbers are not what I see in Unity: say which is which.
- Never create `.meta` files. Never put an API key or my email in the repository. Guard `rm` variables.
- When I push logs: read the text log, look at a contact sheet of the frames, then remove the frames from the
  repository and commit.
- Check the clock before you write a time into a note (`date +%H:%M` in the workspace is my local time).

## Where things stand (the short version)

On the evening of 19 November I looked at the get-up and called it a mess (knees in the ground, the sword shaking
as he pushes up, the shield through him). I went to bed and told you to work through the night, in this order:
finish the get-up, stumbles, bodies that absorb a hit, kicks, pouncing on an opening, downed fighters. **All six
have a first build pushed. I have seen none of it.** Everything is headless only.

- **Getting up** (the plan `2026-11-19-from-how-he-lies-to-how-he-gets-up.md`): knees stay above the ground; the
  sword hand is held still while it is down; the shield is carried round him (step 5); the whole of him is eased
  into his stand; the floor carries on past its edge (my target was bashed off the edge of the floor three times in
  my log of 20:10). Film: https://claude.ai/artifact/686Uo8dQEjeAjPCj1q8aHm
- **Stumbles:** his hands go out for balance.
- **An impact:** his knees and hips take it (the impacts plan's step 4, a first build).
- **A kick:** one front kick, from the lab's "Kick him" (the only kick recording in the repository).
- **The pounce:** in the duel lab, he goes for a man who stumbles. After a small bash it lands; after a medium or
  hard bash or a kick he is free too late and is blocked.
- **A man who is down:** "Strike him where he lies", on demand. Off unless asked: whether fighters hold off is my
  decision, still open.
- The film for the five after the get-up: https://claude.ai/artifact/KjzrrmCJAUsAwrSp4XBNnR

**On the morning of 20 November I looked** (three logs) and said: knees fine; the kick works, and I want soft,
medium and hard, with hard knocking him over backwards; the bare-handed dummy runs backwards on fast legs where I
would expect it to fall; of a man who is down, "he ran, stopped then attacked. That won't work": running at him
and striking must be one motion, and a man knocked far must be run after; and I want a test scenario for it.

**The second pass (pushed 08:30, headless only, I have not seen it):**
- Running in and striking is one motion, for the pounce and for a man who is down (`DuelLab.GoFor`): he throws
  the blow while still running, to land as he arrives. One-press tests: "Knock him down and strike him: Near / Far".
- Kicks soft, medium, hard. Hard knocks him over, through a shield too.
- A man not in his guard stumbles and falls as any fighter does (before, only a man standing in a guard could).
- The jump as he stands was last night's arms-out kept through a fall; mended. **The lab's falls on demand could
  not show it: measure a get-up after a real knock-down** (`FIGHT_KICK=1 FIGHT_KICKKIND=hard`).
- "Slow motion" in the lab.
- Its film: https://claude.ai/artifact/N5MwJrqpxuP8x1ZEeqDxs8

**I looked at the second pass and said "this all looks good", and: do the next things, "but not the fight of six,
keep it in the dance for now".** Pushed about midday on the 20th, headless only, I have not seen it:
- A man who is down is stabbed from beside his chest, bent to it (was: a standing blow that hit shins and arms).
- The pounce goes for what is open, and a stumbling man cannot guard (duel lab and dance only: for everyone it
  broke the six's crossing budget).
- In the dance, by themselves: pounces, kicks (for space, and as a guard-breaker), and going for a man who is down
  (decided since: see below). `RunIn` in `DuelLab.cs` is the running-in; each dancer has one.
- Its film: https://claude.ai/artifact/JfCuCLnTgYQgfyaUB1WJ3C
- **Not reached, and I asked for it: the get-up leftovers** (the first three lines of "rough" below).

**I looked at the midday push (my log of 13:59) and said at 16:11: "all this looks fine for now. I do want a fighter
to go for a man that is down."** So:
- A fighter goes for a man who is down: now the default in the duel lab as well as the dance (Combat Feel "A Fighter
  Goes For A Man Who Is Down"; lab row "A man who is down: he goes for him / he holds off (old)"). Not the fight of
  six. In my log's dance a man went down 12 times and the other went for him 12 times; 8 stabs met body or head.
- I asked for a deep research document and plan for combos, with five goals (one fluid motion; each blow starting
  where the last one's blade ended; follow-through; epic and powerful; the last blow the biggest). It is written:
  `Docs/Plans/2026-11-20-combos-that-flow-and-end-in-a-bang-research-and-plan.md`. **Nothing of it is built.**
- Its film: https://claude.ai/artifact/T6S2n1EfFLszkRJWMsRRW7

**At 18:42 I said "Yes it sounds good. Go."** Every default in the plan is taken. **Step 1 is built and pushed
(20:00), headless only, I have not seen it.** It is two measuring tools; nobody fights differently:
- A report of every recording's blade (`Tools/Headless/flow.sh`): the corners each cut starts and ends in, how far
  it is carried through, and how each pair joins. Its check passes (the performer's own two joins are the best of
  144). It corrected the plan in two places (the thrust comes back high right; a chop into a rising backhand and a
  thrust into a spinning backhand do not join as written).
- In the duel lab, under "Strike at": "The blade's path: drawn / hidden". Drawn: the planned path of an attack's
  blade in pale blue, a white bar where it is to land, the sword's real path in orange with a dot a step (bunched
  dots: a slow blade), and the numbers in "His last blade path". With it on, my playtest log gets a `bladepath` line for every attack. **The drawing is built blind.**
- Headless, a blade is frozen 0.05 s as it lands (meant), and then starts again slowly: in 7 of 19 blows its
  point stayed under 5 m/s for a further 0.07 to 0.15 s before sweeping on, and 3 of 19 fell well short of their
  planned carry-through. The cause, traced headless: the pose sweeps on, but the sword is solid and so is the body
  it struck, so the sword is held by the body until it slips past. Not seen in Unity. My third goal for combos is
  that a blow slashes through, so mending this comes first. (The first push said "it may be the stand-in": that
  was a guess and is withdrawn.)
- Its film: https://claude.ai/artifact/DicGDRVN9k8W211a5JwBj6
- The panel's "Look here" list is step 1's push (four steps, eight names; all drawn).

## What to do now, in this order

**0. Combos (the work in hand).** If I have looked at the blade's path and pushed a log: read its `bladepath`
lines first (search the text log for "bladepath"). Each says how far the blade was planned to be carried on after
it landed and how far it really was, what it met in the blow's own words, and how long it was frozen on the hit
and then crawled ("frozen 0.05 s, then under 5 m/s for 0.10 s more"). If blades stick at a body in Unity too, mend
it first, as its own switch with an old/new toggle: a blade the rules let through ("drove on through") should not be
held by the body it cut (the sword's solid copy, `UpperBodyArticulation.Hold`, against the struck man's solid
parts; `FIGHT_BLADEPATHSTEPS=1` prints the posed point, the seen point and what the sword lies against, each
step). Mind the shield and a parrying sword: those must still stop it. Tell me simply. Then **step 2: two blows as one swing** (forehand into rising backhand, on the move, on a dummy
with no guard; lab toggle "Throw two: flowing / one after the other (old)"; the pass marks are in the plan's
table). It starts from `BladeFlow.Join` (leave the forehand 0.23 s after it lands, enter the rising backhand 0.17 s
before it lands; 64 cm between the points and 43 cm between the hands to make up) and needs two new things in the
attack maker: an attack that ends at its blade's turn instead of in the guard, and one that starts from the blade
as the last one left it. If I have not looked yet, ask me to, and do not build step 2 on a drawing nobody has seen.

**1. If I have looked and pushed logs:** work my notes in first, one at a time. Read my pose log step by step, not
only the report lines: on 19 November the report said a get-up was fine while my log showed him falling off the
floor and jumping as he stood. `/tmp`-style scripts are quick to rewrite: per-part speed from one step to the next
out of `units/*.pose.csv.gz`.

**2. Things I know are rough from the night (the README's entries say more):**
- The sword still moves while the sword hand is in the air and as he stands (the hand is down only 0.3 to 0.7 s of
  a get-up).
- One get-up in three from his front or right starts with a small kick of a shin (he is taken up while the gather
  still moves that leg to its kneel).
- As the shield turns up, about 1.3 s into a get-up from his front, its rim is 3 to 5 cm in the ground for 0.1 s.
- The pounce's blow is still stopped after a small or medium bash (two of four land).
- A struck man's sword arm goes through his own board (the hit reaction): it is why a stumbling man's open guard
  could not go into the six, and it shows more in the dance now.
- Going for a man who is down in a dance: seen only in my Unity log of 13:59 (12 of 12 gone for, 8 stabs landed, 3
  met nothing, 1 too late; several landed on a man already rising). Never in a headless dance.
- The kick lands 112 cm up for all three strengths (one recording): high for a front kick.

**3. Left on the get-up plan:** the trunk's turn in the gather (`ClipFighter.GatherTurns`, built, off: at 0.25 it
does not finish the gathers; try more strength or a longer gather), and step 2 (count the board in what a start
costs).

**4. Left on the night's other steps:** feet crossing in a stumble to the side; a sword blow's throw worked out as
a bash's is; of the impacts plan's step 4: feet that slide, the head lagging; more kicks
(**the kick pack's five standing kicks and the Capoeira kicks are not in the repository: ask me for them**), and
fighters kicking by themselves; the rest of the Downed play plan (stab and low slash as their own blows, low
kicks, rolls, the guard from his back, reading, victory laps).

**5. Decisions waiting on me:** whether a fighter holds off while his enemy is down; what being fooled by a fake
costs; whether real attacks should be more frequent.

## Where the code is (all in `Assets/_Battler/Scripts/Runtime/`)

- `Locomotion/ClipFighter.cs`: the `Footings.Down` branch of `StepFallen` holds the gather; `BeginGetUp(true)` only
  sets the get-up out (and remembers the start gathered for); `BeginGetUp()` takes him up; `StepGettingUp` is a
  step of getting up and holds the sword hand's bodily hold; `EaseArmsFromGetUp` eases the whole of him into his
  stand; `MeasureGetUpStart` and `MeasureGetUpEnd` measure the two ends. `ThrowBody` is the door every impact goes
  through (`PressInto` at its top: knees and hips). `StepArmsOut` (arms in a stumble), `StepPress`, `KickAt` /
  `StepKick` / `Kicked`, `CutBashShort`.
- `Motion/GetUp.cs`: `Begin`, `Compose` (the take-over is the branch `takingOver`), `KneelIfUnder`,
  `PinSwordHand` / `SettleSwordHand` (the hand held still), `HoldShieldUp` (the carried shield: `raise`,
  `ShieldSideAlongGround`, `PlaceShieldArm`'s plain geometry), `PoseAt` (what the gather is drawn to).
- `Authority/UpperBodyArticulation.cs`: `PinHand`, `HoldHandTurn` (a joint drive never seen in Unity), `TakeUp`.
- `Sandbox/DuelLab.cs`: `StepPounce`, `StepDowned`. `Sandbox/BattleSandbox.cs`: `EnsureFloorPastTheEdge`, the lab
  rows (search for a row's name), `LookTag` / `LookSteps` / `LookHere`.
- `Locomotion/LocoBody.cs`: `Pressed` (the hips' dip for an impact).
- **Combos, step 1:** `Locomotion/BladeFlow.cs` (a recording's blade against his hips; `Enter` the top of the swing,
  `Leave` the blade's turn after the landing, `From` / `To` the corners, `Join(first, next)` and `At(...)` with the
  bend). `Locomotion/BladePath.cs` (`Plan`: a made attack's planned path, planted or on the move; `Went`, `Measure`,
  `Tell`). `Sandbox/BladePathView.cs` (the drawing, from `BattleSandbox.OnGUI`). In `ClipFighter.cs`:
  `PlanBladePath` (called as an attack is thrown, in `StrikeMade` and `StrikeOnTheMove`) and `TrackBladePath` (at
  the top of `Move`). The made attacks themselves: `AttackMaker.cs` (`Build`, its time-warp of wind-up, drive and
  way back; `MadeAttack.Blade`), `AttackOnTheMove.cs` (`Layer`, `Walked`), `Locomotor.OverStep`.

## How to measure

- `Tools/Headless/gup.sh tag "0 180"` and `"90 270"`: felled from his back, front, right and left (three targets;
  the same falls every time, so old against new is exact). `Tools/Headless/downs.sh tag`: a shield bearer bashed
  down, hard and extra hard (about ten minutes; chaotic). Old ways: `FIGHT_TAKEOVER=0`, `FIGHT_GATHER=0`,
  `FIGHT_GATHERTURNS=0.25` (a lever, not an old way), `FIGHT_STARTNEAREST=1`, `FIGHT_SHIELDDOWN=0`.
- Read in a run's report: "the first 0.75 s of a get-up", "the take-over of a get-up", "a get-up takes him up as he
  moves", "he gathers himself before a get-up", and the `CROSSINGS` and `TREMBLE` budget lines. Each get-up's own
  line (search "the start:") has the same for that get-up, with which part and when.
- "the end of a get-up, as seen" (the jump as he stands) and "a rising fighter's knees stay above the ground" are
  report lines too. Old ways from the night: `FIGHT_GETUPKNEES=0`, `FIGHT_GETUPSTILL=0`, `FIGHT_GETUPEND=0`,
  `FIGHT_GETUPSHIELD=0`, `FIGHT_ARMSOUT=0`, `FIGHT_PRESS=0`, `FIGHT_POUNCE=0`; `FIGHT_KICK=1` and `FIGHT_DOWNED=1`
  ask for a kick and for a blow at a man who is down (in the game he goes for a downed man by default since 20
  November; headless it stays off unless asked, so falls and get-ups are measured on a man left alone). `Tools/Headless/four.sh tag` runs the four bashes.
- Then fights of six (seeds 5, 3 and 7, 40 s) for the budget: crossing on his feet 0.35% to 0.82% against 0.5%,
  board tremble 6.3% to 8.7% against 8%, as the night left them. They differ run to run after the first fall: one
  seed proves nothing; seed 7 is the same to the hundredth when a change does not touch a fight.

- **Combos:** `Tools/Headless/flow.sh` (no fight, seconds; ends `FLOW OK`). `Tools/Headless/aim.sh name
  FIGHT_BLADEPATH=1` prints a `BLADEPATH` line an attack (add `FIGHT_BLADEPATHSTEPS=1` for every step);
  `Tools/Headless/mv.sh tag FIGHT_BLADEPATH=1` is the same on the move (its circling run takes seven minutes).

## Things that cost time (so you do not repeat them)

- A recording's root is not a body frame: the bake turns each slice's root to where his head looked as the slice
  began, so one moment of one take sits 108 degrees apart in two slices. Compare blades against the hips.
- A physical sword runs a step or two behind its pose, which at a blade's speed is half a metre. Measure it against
  the planned line, not against the plan at the same moment.

- `b.sh` refuses, and what follows runs on the old build, if the same command line also names the test dll. Run it
  alone. Never build while a run is going.
- **The headless stand-in can be the thing that is wrong.** A foot "thrown at 27 m/s" was the stub, not the game.
  Before believing a number that says the game does something violent, trace it step by step.
- A get-up reads the pose as it begins (where his sword hand lies). Anything that poses the animation rig before a
  get-up must put the body's pose back.
- The get-up's own first poses jump (the sword arm for its first steps, the shield arm's elbow about 0.15 s in).
  A blend from how he lay hides that; anything that follows the get-up's poses exactly from the first step shows it.
- A hand taken straight from one place to another, with the hand pinned somewhere else or passing close to its own
  shoulder, swings the elbow round in a step. Look at "posed" speeds (the pose) before blaming the physics.
- The headless tube lineup has no target dummies, so a test dummy's own behaviour cannot be checked headless.
- Headless falls are cleaner than mine. A crossing that shows in my log may need many falls to show headless.
- The report only knows what it measures. It measured a get-up's first 0.75 s and missed the jump at its end and
  the floor's edge. When I say it looks wrong and the numbers say fine, the numbers are missing something.
- A fighter in the labs can walk off the scene's 40 m floor in a long session (mended with an unseen floor; the
  headless ground has no edge, so this cannot be checked headless).
- A solver that fits an arm inside its joints' limits can change the elbow's side from one step to the next as the
  hand moves. Where a pose must be smooth, place it by plain geometry and turn the joints toward it.
- Something that poses his arms and runs before the cover and the guard is undone by them: it must run last
  (`Apply`, after `SettleHeld`).
- When I say something looks pushed or thrown "by nothing", look in my log for a `stumble` with no `tube` or
  `struck` just before it, and at how the unit's root moves after `getup`.

- In the duel lab the swordsman stands where his own order leaves him. After a pounce of his own he used to stand
  where it ended, and the target, walking back to its spot, came right up against him: every blow I asked for after
  that was from far too close. He steps back to his distance now (20 November, 21:15). When I say a dummy "walks
  into me", look at where each stood in the units' files before anything else.

- 21 November: until then the headless engine read a joint's "maximum force" as an impulse a step, so the hold on
  the hips never gave: a man carried off at once (the jump in for a second blow) fell in Unity and never headless.
  Mended and on by default (`PHYSX_JOINTFORCES=0` for the old; `Tools/Headless/README.md`); the six's baseline is
  now crossings 0.07%, tremble 4.8%. Headless numbers of shoved bodies from before were with the stiff hold. And the
  debug panel's layout cannot be seen headless: the rows that wrap (`BattleSandbox.RowTakes`) were built blind; ask
  me how it looks. And never start a headless run under a name that is still running.

- 21 November: `Tools/Headless/strip.py` draws a headless run's fighters as stick figures from its log. Use it
  before sending me anything that changes how a body moves: numbers alone missed things before.

- Combos, step 2 (20 November, late): the join's kink (the posed point thrown 15 to 19 cm off its course in one
  step as the second blow begins) has no found cause; letting faster parts carry on did not change it. Look at the
  made attack's own aim (its turn, its hand's move and its wrist's steer over a wind-up of a quarter of a second)
  and at the 70% pace after a hit going back to full pace at the join. And the second blow of a pair is made
  planted even when I walk: the maker for blows on the move needs to take a score that enters out of another.

- A score's tracks (`AttackScore.Frames`) are in the frame the fighter faced as its clip began, his travel and turn
  in them. They are not against his root. I lost two hours to taking them the other way at the combos' join.
- A sword lands slower headless than in Unity (10 against 12 to 16 m/s), so a headless target does not stumble from
  a sword's blow and mine does. `FIGHT_STUMBLEFROM=0.7` makes it.
- `b.sh` refuses when the command that calls it names the test dll anywhere. Run it in a command of its own.

Begin with what is first in the list. If something is unclear, take the most reasonable reading, write down which
one you took, and carry on.
