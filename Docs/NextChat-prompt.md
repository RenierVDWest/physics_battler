# Prompt for the next chat (written 19 November 2026, 15:45)

Paste everything below the line into a new chat.

---

Pick up my Unity project "Physics Battler". Repository `RenierVDWest/physics_battler` (private), branch
`physical-body`. You have my permission to push to that branch.

## Start

1. Attach the repository with push access and clone it. Read `CLAUDE.md` at its top first and follow it: it is the
   briefing, and it is up to date as of 19 November 2026, 15:40. `README.md` is the build log (almost 7,000 lines:
   search it, never read it whole). The last entries, from "### 19 November, later in the morning" on, are today's.
2. Read the plan for the work in hand: `Docs/Plans/2026-11-19-from-how-he-lies-to-how-he-gets-up.md`, with its
   progress notes at the end. Its live version is https://claude.ai/artifact/FAVSQjVZCbJ87jf6aargPa: add your own
   progress notes there with the Docs tools (insert at the end), and to the copy in the repository.
3. Set up the headless build: `cd Tools/CompileCheck && ./get-physx.sh`, then `Tools/Headless/b.sh` in a command by
   itself (it wants "0 Error(s)" four times). `export S=/tmp/pb2` for the run outputs. Read `Tools/Headless/README.md`.

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

Today we worked on what happens when a fighter falls and gets up.

- Seen by me in Unity and good: the shield no longer goes through his body as he falls; the big "thrown by nothing"
  after a tube fall is gone; the gather (he pulls his arms and legs in before he gets up) "looks fine for where it
  is now".
- Not yet confirmed by me: whether the yellow frames and the "Test this" box sit on the right things; whether a
  test target now stays where it got up (no shuffle back to its spot).
- The plan's steps: 1 (measure and stop frame) built. 2 (start chosen by how he lies) built, off: it did not help
  alone. 3 (the gather) has a first build, on. Turning his trunk in the gather was built and is off (lever
  "Gather turns his trunk" at 0): it made the take-over rougher.
- The check budgets are still over in the fight of six (crossings on his feet 0.54% and 1.11% against 0.5%; shield
  tremble 8.8% and 9.0% against 8%), for reasons written in CLAUDE.md. Do not make them worse.

## What to do now, in this order

**1. Step 4: the take-over.** When the gather ends, the get-up takes him up as if he lay still, and runs its old
blend from the gathered pose. It should take the pose and the motion he really has.

Where the code is (all in `Assets/_Battler/Scripts/Runtime/`):
- `Locomotion/ClipFighter.cs`: the `Footings.Down` branch holds the gather (`gathering`, `gatherTime`,
  `GatherFurthest()`, the statics `GathersBeforeGetUp`, `GatherMost`, `GatherMuscle`, `GatherTurns`).
  `BeginGetUp(true)` only sets the get-up out; `BeginGetUp()` takes him up. At the take-over the pose is put on the
  seen body first (`fallAsIs.Capture(Rig); fallAsIs.Apply(Anim, false)`), because a get-up reads things off the pose
  as it begins (where his sword hand lies). `StepFallBody` has the gather's branch: `getUp.PoseAt(0f)` is the pose
  his muscles draw him to, and his sword arm is left as it lies.
- `Motion/GetUp.cs`: `Begin`, `Compose` (the blend from how he lay: `mixed.MixToward(keyed, …)` over `BlendIn`,
  0.6 s, each part on its own clock by `NaturalShare`), `PoseAt`, `Finish`, and the sword hand's pin
  (`BeginSwordHand`, `PinSwordHand`, `SettleSwordHand`).
- `Authority/UpperBodyArticulation.cs`: `LetGo` (falls), `TakeUp` (taken up: the hips' carrier is put on the posed
  pelvis and locked), `BeginFallStep` and `SubstepFalling`, `TurnHips` (the trunk's turning hold, off).

What step 4 should do, from the plan: begin from the pose he has; blend what is left by where the joints are, not
by turning each part; keep the speed he has instead of stopping him dead; take less time when he is already close.
Put it behind a switch with an old/new toggle beside "From how he lies" (Tubes, "Falls and getting up", "The start
of a get-up").

How to measure it:
- `Tools/Headless/gup.sh tag "0 180"` and `"90 270"`: he is felled from his back, front, right and left (three
  targets). `Tools/Headless/downs.sh tag`: a shield bearer bashed down six times, hard and extra hard (about ten
  minutes a pair; one bash-down alone shows nothing). Old ways: `FIGHT_GATHER=0`, `FIGHT_GATHERTURNS=0.25`,
  `FIGHT_STARTNEAREST=1`, `FIGHT_SHIELDDOWN=0`, `FIGHT_ROOM=0`, `FIGHT_CAUGHT=0`.
- Read these lines in a run's report: "the first 0.75 s of a get-up" (crossing, fastest part, how far the hands
  went), "he gathers himself before a get-up" (how long, how many ended by the clock, how far off), and the
  `CROSSINGS` and `TREMBLE` budget lines. Today's figures to beat are in the README's entries of 19 November.
- Then two fights of six (seeds 5 and 7, 40 s) for the budget.

**2. Then turn his trunk in the gather again** (`GatherTurns`, try 0.25). About half the gathers end by the clock,
mostly when he lies on his side, because nothing turns his trunk. With step 4 in, see whether it now finishes them
without making the take-over rougher (it went from 4.1 to 11.5 m/s before).

**3. Then step 5: the shield carried round him as he gets up.** My decision: the shield lies flat on the ground out
to his shield side, he pushes up on that forearm, and the shield comes up edge first. The crossings that are left
in a get-up are the get-up's own poses (the board in his hips as it begins, past his head at about 1.1 s).

**4. Then step 2 again:** count the board in what a start costs, and see whether choosing the start then helps.

## Things that cost time today (so you do not repeat them)

- `b.sh` refuses, and what follows runs on the old build, if the same command line also names the test dll. Run it
  alone.
- A get-up reads the pose as it begins. Anything that poses the animation rig before a get-up (weighing starts,
  the gather) must put the pose back, or his sword hand is thrown across the room.
- The headless tube lineup has no target dummies, so a test dummy's own behaviour (walking back to its spot) cannot
  be checked headless. Say so when it matters.
- Headless falls are cleaner than mine. A crossing that shows in my log may need many falls to show headless.
- When I say something looks pushed or thrown "by nothing", look in my log for a `stumble` with no `tube` or
  `struck` just before it, and at how the unit's root moves after `getup`.

Begin with step 4. If something is unclear, take the most reasonable reading, write down which one you took, and
carry on.
