Pick up my Unity project "Physics Battler". Repository `RenierVDWest/physics_battler`, branch `physical-body`. You
have my permission to push to that branch.

## Start

1. Attach the repository with push access and clone it. (On 19 November the first ask for push access was refused
   and went through when asked again after I said "you may push" in the chat. If it is refused, tell me at once and
   carry on reading.) Read `CLAUDE.md` at its top first and follow it: it is the briefing, and it is up to date as
   of 19 November 2026, 19:30. `README.md` is the build log (over 7,000 lines: search it, never read it whole). The
   last entries, from "### 19 November, later in the morning" on, are that day's.
2. Read the plan for the work in hand: `Docs/Plans/2026-11-19-from-how-he-lies-to-how-he-gets-up.md`, with its
   progress notes at the end. Its live version is https://claude.ai/artifact/FAVSQjVZCbJ87jf6aargPa: add your own
   progress notes there with the Docs tools (insert at the end), and to the copy in the repository.
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

We are working on what happens when a fighter falls and gets up.

- Seen by me in Unity and good: the shield no longer goes through his body as he falls; the "thrown by nothing"
  after a tube fall is gone; a test target stays where it got up; the gather (he pulls his arms and legs in before
  he gets up) "looks fine for where it is now"; the yellow frames and the "Test this" box on the debug panel.
- **Not seen by me yet: step 4, the take-over** ("A Get-Up Takes Him Up As He Moves", on). If I have looked and
  pushed a log, start there: read my `getup` lines (each ends with "the take-over: taken up moving at ..., the
  biggest jump in a part's speed in one step ...") and compare with the headless figures in the README's last entry.
- The plan's steps: 1 built. 2 built, off (it did not help alone). 3 (the gather) seen. 4 (the take-over) built,
  headless only. The trunk's turn in the gather: built, off (lever "Gather turns his trunk" at 0).
- The check budgets in the fight of six are about where they were (crossings on his feet 0.58% to 0.73% against
  0.5%; shield tremble 6% to 9% against 8%). Do not make them worse.

## What to do now, in this order

**1. If I have looked at step 4:** work my notes in first.

**2. The trunk's turn in the gather** (`ClipFighter.GatherTurns`, `Upper.TurnHips`; Combat Feel "Gather Turns His
Trunk"). About half the gathers end by the clock, mostly when he lies on his side, because nothing turns his trunk.
At 0.25 of his weight the turn does not finish them (0, 1, 3 and 2 of 3 by the clock from his back, front, right,
left; his hips still 11 to 27 degrees off), and it no longer makes the take-over rougher (that was a fault in the
headless stub, mended). Try more strength (0.5, 0.75), and a longer gather where his hips are still far off (my
answer 2 allows half a second more on the ground). It must not look like he is turned by a hand: that is for my
eye. A changed default needs a new field name in `CombatFeel.cs` (my scene keeps the old field's value).

**3. Then step 5: the shield carried round him as he gets up.** My decision: the shield lies flat on the ground out
to his shield side, he pushes up on that forearm, and the shield comes up edge first. The crossings left in a
get-up are the get-up's own poses (the board in his hips as it begins, past his head at about 1.1 s), and so are
the fast arms left after step 4 (his shield arm's forearm at 6.5 m/s 0.2 s into a get-up from his front; his sword
hand at 6 m/s as its pin lets go 0.4 s in).

**4. Then step 2 again:** count the board in what a start costs, and see whether choosing the start then helps.

## Where the code is (all in `Assets/_Battler/Scripts/Runtime/`)

- `Locomotion/ClipFighter.cs`: the `Footings.Down` branch of `StepFallen` holds the gather (`gathering`,
  `GatherFurthest()`, the statics `GathersBeforeGetUp`, `GatherMost`, `GatherMuscle`, `GatherTurns`) and keeps his
  body's last four poses (`bodyPoses`). `BeginGetUp(true)` only sets the get-up out; `BeginGetUp()` takes him up;
  `FirstStepOfGetUp` runs the get-up's first step in the same game step. `StepGettingUp` is a step of getting up.
  `MeasureGetUpStart` measures the first 0.75 s and the take-over's jump.
- `Motion/GetUp.cs`: `Begin` (reads his speeds, sets each part's clock), `Compose` (the take-over is the branch
  `takingOver`; the old blend is under it), `PoseBuffer.SpeedsSince` and `Carry`, `PinSwordHand`, `Finish`.
  `TakesOverAsHeMoves`'s comment says what was tried and not kept.
- `Authority/UpperBodyArticulation.cs`: `LetGo`, `TakeUp`, `TurnHips` (the trunk's turning hold).

## How to measure

- `Tools/Headless/gup.sh tag "0 180"` and `"90 270"`: felled from his back, front, right and left (three targets;
  the same falls every time, so old against new is exact). `Tools/Headless/downs.sh tag`: a shield bearer bashed
  down, hard and extra hard (about ten minutes; chaotic). Old ways: `FIGHT_TAKEOVER=0`, `FIGHT_GATHER=0`,
  `FIGHT_GATHERTURNS=0.25` (a lever, not an old way), `FIGHT_STARTNEAREST=1`, `FIGHT_SHIELDDOWN=0`.
- Read in a run's report: "the first 0.75 s of a get-up", "the take-over of a get-up", "a get-up takes him up as he
  moves", "he gathers himself before a get-up", and the `CROSSINGS` and `TREMBLE` budget lines. Each get-up's own
  line (search "the start:") has the same for that get-up, with which part and when.
- Then fights of six (seeds 5 and 3, 40 s; seed 7 has no fall) for the budget. The figures to beat are in the
  README's last entry of 19 November.

## Things that cost time (so you do not repeat them)

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
- When I say something looks pushed or thrown "by nothing", look in my log for a `stumble` with no `tube` or
  `struck` just before it, and at how the unit's root moves after `getup`.

Begin with what is first in the list. If something is unclear, take the most reasonable reading, write down which
one you took, and carry on.
