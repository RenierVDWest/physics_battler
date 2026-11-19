# Physics Battler: start here

This is the briefing a new chat reads first. It is kept short on purpose. The full build log is `README.md`
(over 6,000 lines: search it, never read it whole). Update this file at the end of every milestone or step:
the "Where things stand" and "Roadmap" sections must always be true.

Last updated: 19 November 2026, 22:55 (he saw step 4 and called the get-up a mess. Mended and pushed, headless
only: knees, the sword hand, the jump as he stands, and the floor's edge he was bashed off. He has gone to bed and
set the night's order: see "Roadmap").

## The project in a few lines

A Unity auto battler (Unity 6000.6.7f1, URP, PC) that is meant to be a granular combat simulator: fighters on
physical bodies who move, read each other, aim blows at body parts and meet each other's weapons and shields
for real. Renier (the developer) playtests in Unity and pushes logs. Claude works headless in a cloud
workspace and cannot open Unity.

- Repository: `RenierVDWest/physics_battler`. **Work on the branch `physical-body`.** (His notes call it private;
  on 19 November the tool that attaches it said it is public. He has been told.)
- Other branches are history: `main`, `locomotion-lab`, `arm-rebuild`, `backup/active-ragdoll-v1`.
- Only Mediums fight for now (Large and Small wait). Loadouts: sword and shield, sword only.

## How Renier wants the work done

These are standing rules. They hold in every chat.

**Talking to him**
- Short, simple explanations, bottom line first. He wants the high-level picture in plain words.
- When a change needs his eye, make it happen on demand: a debug button, an old/new toggle in the lab.
- **Every push that needs his eye highlights what to press** (his rule, 19 November): the debug panel shows the
  steps in a box at its top and draws a yellow frame round each button, row, slider, heading, section and lab he
  is asked to press or watch. Set it in `BattleSandbox.cs`, "Look here": `LookTag` (a new one every push),
  `LookSteps` (plain steps) and `LookHere` (the names, exactly as on the panel; `"Heading>Name"` for a name
  that occurs more than once). Check with the panel test that every name in `LookHere` is really drawn.
- Report honestly what is unverified. Headless numbers are not what he sees in Unity: say which is which.
- The 3b2b rule: every document made for him, and every finished and pushed step that needs his look, gets a
  "3b2b" (a short narrated, animated explainer). It is generated before the update is sent: the link first,
  a short note after. Ordinary back-and-forth stays normal. It is made by a subagent on Opus at medium
  effort, never on the main model (the `3b2b` skill has the details). A copy of the kit is in
  `Tools/Explainer/kit/`, so it can be made with his computer off; `Tools/Explainer/example-lane/` is one of this
  project's own explainers to copy the format from (its note, script and the generator of its drawings).
- Do not spawn subagents for anything else. The brief that works for the explainer's subagent is
  `Tools/Explainer/brief-template.md`.

**Changing the game**
- Big changes go one step at a time. Each step sits behind a Combat Feel switch (`CombatFeel.cs`) with a
  descriptive tooltip, and has lab buttons or an old/new toggle (`BattleSandbox.cs`).
- New fields take their defaults from code. A field already saved in his scene keeps the scene's value, so a
  changed default needs a **new field name**.
- Each step ends with: a README entry, a progress note in its plan document, a commit and a push.
- Every feature of the standard style applies to all loadouts.

**Hard rules**
- **Never create Unity `.meta` files.** He commits them from Unity. (Files outside `Assets/` need none.)
- **The ElevenLabs API key never goes into the project or onto GitHub.** It lives in Unity EditorPrefs under
  `Hexamus.ElevenLabs.ApiKey`.
- His email address is private: never send it to an outside service without his say.
- With `rm`, guard the variable: `"${S:?}"`.

**Git**
- `git stash; git pull --rebase; git stash pop` before a push. Commit and push before ending a turn (a stop
  hook enforces it).

**Playtest logs**
- He pushes them to `PlaytestLogs/<date_time>/`. Read the text logs, and look at the frames
  (`frames/frames.csv` is the index; make a contact sheet and look at it).
- Once used, remove the frames from the repository and commit:
  `git rm -r -q --cached PlaytestLogs/*/frames; rm -rf PlaytestLogs/*/frames`.
- His logs carry `cross` lines (a fighter's own sword, shield and body crossing) and `getup` lines.

## Design rules (his decisions, in force)

- Attacks are thrown on the move by default. A planted attack only when a stop is planned.
- **No recorded animation is ever played as it is.** Everything is rebuilt from its parts and moved in code,
  each part relative to the part it sits in. Recordings are reference only.
- The standard style comes first. Other styles (capoeira, two-handed) are shelved until it is solid.
- There must be a way to check a made animation against its reference before he sees it.
- The shield is held steady in its best cover. It never fakes. Gestures belong to the sword arm. The shield's
  one move of its own is the bash (small and medium push an enemy out of his space; the hard one breaks a
  guard and can be countered by a medium bash).
- Where the sword and the shield want the same space, the shield gives way to the sword.
- A fake is a real light attack cut short, with no swing sound. Each fighter has his own ratio of fakes to
  real attacks.
- Tells come from the fighter's own gestures and their pattern.
- Impacts: every force has an equal and opposite reaction; both bodies absorb it. **No invented randomness in
  impacts**: variation comes only from how the bodies really meet.
- The slowest stepping walk is only for small adjustments, never for a continuous walk.
- Debug panel: every action's sliders sit under that action in the actions window; sections are colour
  coded; there is a search box.

## How the code is laid out

Runtime code is in `Assets/_Battler/Scripts/Runtime/`. The scene is `Assets/_Battler/Scenes/Battle.unity`.

The pose pipeline, in order: `FightLoop` → `UnitClips` (part of `UnitController`) → `ClipFighter` →
`Locomotor` → `LocoBody` → the body.

| File | What it is |
|---|---|
| `Locomotion/ClipFighter.cs` | One fighter: his state, blows, reactions, what he holds, his measures. The biggest file. |
| `Locomotion/CombatFeel.cs` | Every switch and lever, pushed into the code's statics in `Push`. |
| `Sandbox/BattleSandbox.cs` | The debug panel and the labs (battle, duel, targets, tubes, aim). |
| `Locomotion/LocomotionLab.cs` | The walk lab (its own, older panel). |
| `Authority/UpperBodyArticulation.cs` | The physical upper body, driven to the pose by damper-only motors. |
| `Motion/GetUp.cs`, `Motion/GroundClamp.cs`, `Motion/PosePipeline.cs` | Get-ups, and keeping what the hands hold out of the ground. |
| `Locomotion/StrikeHits.cs`, `ImpactMeter.cs`, `SelfCrossings.cs` | What a blow does, and the measures (impacts, own crossings, tremble). |
| `Locomotion/ImpactLedger.cs`, `StumbleSteps.cs` | The momentum ledger for two bodies that meet; a fighter off balance (stumble steps, turned, down). `ClipFighter.ThrowBody` is the one door into it. |
| `Tools/CompileCheck/Runner/FightReport.cs` | The headless fight and its report; reads the `FIGHT_…` switches. |

A fighter's step: `Move(dt)` (clock, measures, the pose) then `Apply(dt)` (the arms held, eased, settled, then
physics). `Anim.Part(id)` is the posed skeleton; `Rig` is the seen, physical one.

## Testing without Unity

A stub Unity plus real PhysX runs the game's own code headless. Details: README, "Compile check and tests
without Unity". Scripts: `Tools/Headless/` (see its README).

First time in a new workspace:

```
dotnet --version || apt-get install -y dotnet-sdk-8.0   # the workspace may come without it
cd Tools/CompileCheck && ./get-physx.sh      # fetches PhysX bindings (not in the repository)
../Headless/b.sh                             # builds the four projects: wants "0 Error(s)" four times
```

Rules that cost time when broken:

- **Never build while a fight is running** (`b.sh` refuses). A rebuild under a running fight spoils its result.
  Run `b.sh` in a command of its own: a command line that also names the test dll makes it see itself as a fight
  and refuse, and what follows then runs on the old build.
- Two fights at a time at most (two cores). Two fights of six for 40 s, side by side, take about ten minutes.
- A command in the foreground is cut off after about two minutes: start runs in the background (the scripts
  do) and wait in a loop for their `.done` file.
- Fights are chaotic: after any change a run differs. One run is a direction, not a rate. For a firm number
  use the aim test (nearly repeatable), the same build with the switch on and off, or a second seed.
- The stub lacks some of Unity (`Vector3Int`, `Vector2.normalized`). Contact modification does nothing
  headless. Collision callbacks reach only objects that carry the script.
- Unity has the last word. A headless pass is "looks right in the numbers", not "verified".
- **The stand-in can be the thing that is wrong.** On 19 November a foot "thrown at 27 m/s" was the stub moving a
  kinematic body in a way Unity does not (mended; `Tools/Headless/README.md`). A number that looks like a fault
  in the game is traced step by step before it is believed. Headless numbers from before that mend (19 November,
  about 16:30) are not comparable with numbers after it.

The panel can be checked without Unity:
`dotnet Tools/CompileCheck/bin/Tests/Debug/PhysicsBattler.Tests.dll panel [battle|duel|aim|tubes|targets] [search words]`
prints "PANEL OK".

## Plans and documents

Every plan document has a copy in `Docs/Plans/` (31 files; `Docs/Plans/README.md` is the index, with each one's
state). The live versions are Claude documents, linked from the index: progress notes are added there, and the copy
here is refreshed when a plan changes a lot. Read the one for the work in hand, never all of them.

The four in hand:

- `2026-11-19-from-how-he-lies-to-how-he-gets-up.md`: **the work in hand.** His five answers are in (all as
  recommended). Steps 1 and 2 are built, step 3 has a first build he has seen, step 4 (the take-over) is built and
  waits for his eye; **next: the trunk's turn in the gather again; then step 5**
  ([live](https://claude.ai/artifact/FAVSQjVZCbJ87jf6aargPa)).
- `2026-11-18-fight-rhythm-from-recordings-plan.md`: **the main plan**
  ([live](https://claude.ai/artifact/GmgWWcXjaoQvCj5n5U5wLh)).
- `2026-11-18-a-shield-that-stays-outside-its-bearer.md`: mostly built; leftovers in the roadmap below
  ([live](https://claude.ai/artifact/9UZ1xrAueNhCM6Yn4wg2NN)).
- `2026-11-18-impacts-that-carry-weight.md`: three steps wait
  ([live](https://claude.ai/artifact/HkqvPHoe22TWSjZwxvdW5V)).

## Where things stand (19 November 2026)

Working and approved by Renier: walking in the dance, quick turn-arounds, recorded get-ups rebuilt from parts,
attacks made from recordings and thrown on the move, the three shield bashes, fakes as real attacks cut
short, the shield held in cover, the sword and shield solid against their bearer's body, hands that move
smoothly, the shield giving way to the sword.

**19 November, 22:10: after his look at step 4 (headless only, waiting for his eye):**

- He saw step 4 in Unity and said the get-up is a mess: "Knees sticking into the ground. STILL the sword shakes
  like crazy when he pushes himself up. Can't you just pin the hand", and the shield intersects him. His log of
  19:57: step 4 measures as built (fastest part about 3.0 m/s against 5.0 to 5.5 at 15:29); from his front a knee
  is 17 cm under the ground for 0.8 s a get-up; the blade's tip moves 53 mm a step while the sword hand is down.
- **Knees stay above the ground** (Combat Feel "A Rising Fighter's Knees Stay Above The Ground"; lab row "Getting
  up, his knees"; `GetUp.KneelIfUnder`): a knee solved under the floor kneels on it. Headless: from his front
  -13 cm for 0.66 s, to +5 cm and never under.
- **His sword hand is held still** (Combat Feel "A Rising Fighter's Sword Hand Is Held Still"; lab row "While it is
  pinned"; `GetUp.SwordHandHeldStill`, `ClipFighter.SwordHandHeldBodily`, `UpperBodyArticulation.HoldHandTurn`):
  while the hand is down the pose is frozen and the physical hand pinned and held to its turn. Headless: the tip
  3 mm a step while down (was 53), but the hand is down only 0.3 to 0.7 s a get-up; the tremble over the whole
  get-up is about a third less (from his front 62% to 39% of the time over 5 mm). **The turning hold is a joint
  drive never seen in Unity.**
- **A get-up keeps the start he gathered for** (Combat Feel switch; `ClipFighter.KeepsTheStartHeGatheredFor`).
- Left worse: the take-over's jump from his front and right (1.3 to 2.3 and 2.0 to 2.6 m/s: a shin, in one get-up
  of three, taken up while the gather still moves that leg).
- The README's entry of 22:10 has the table.
- **His log of 20:10 (22:55):** the target was bashed **off the edge of the scene's 40 m floor** three times, fell
  20 to 34 m and was put back (parts at 55 to 82 m/s): `BattleSandbox.EnsureFloorPastTheEdge` now adds an unseen
  floor past the edge (**not testable headless**). And **every get-up ended with a jump** (forearms 8 to 10 m/s,
  hips 3, in the step after he stood): `SettleHeld` kept a fall-old place for each hand, and only his arms were
  eased out of the get-up. Mended (Combat Feel "A Get-Up Ends Into His Stand Over A Moment"; lab row "The end of
  a get-up"; `ClipFighter.WholeBodyEasesOutOfGetUp`). New report line "the end of a get-up, as seen": headless
  4.6 to 14.3 m/s before, 0.7 to 2.9 after.
- **A lesson:** a report that only measures the first 0.75 s of a get-up cannot see its end. When he says it looks
  wrong and the numbers say fine, read his pose log step by step (`/tmp`-style scripts: per-part speed from one
  step to the next) before believing the numbers.
- **Seen in that log, not yet looked into:** from his front the shield forearm at 9 to 12 m/s about 0.15 s into
  five of eleven get-ups (headless 1.4 to 2.9 at the same moment). With step 5.

**The evening of the 19th: the plan's step 4, headless only, waiting for his eye:**

- **A get-up takes him up as he moves** (Combat Feel switch of that name, on; lever "Take Over Takes At Most",
  0.6 s; `GetUp.TakesOverAsHeMoves`). The blend from how he lay is the old one changed three ways: the pose is
  carried on at the speed his body had, eased out over 0.15 s, and the step he is taken up in is the get-up's first
  step (before, he was held still for one step: stopped dead); his limbs go by where their joints are; a part that
  is close takes less time. With it: what he holds comes out of the floor over 0.2 s, not in the first step; the
  blade's turn clear of the ground is whole in the first pose; a get-up reads the body he has (a caught fall did
  not); his sword hand's pin comes to rest, keeps his arm from folding right up, and turns his elbow slowly.
  To see it: Tubes, "Falls and getting up", "The start of a get-up": "Taken up: as he moves / as if he lay still
  (old)". Headless: `FIGHT_TAKEOVER=0` is the old way.
- **Its measure** (new, in every report and in his `getup` lines): "the take-over": the biggest jump in a part's
  speed from one step to the next in a get-up's first 0.2 s, and how fast he was moving as he was taken up.
  Old way to new, same build: tube falls from his back, front, right, left 1.2, 2.7, 4.0, 1.4 m/s to 1.2, 1.3, 2.0,
  0.9; hard bash-downs 2.8 to 0.9; extra hard 2.5 to 1.7. The fastest part in the first 0.75 s a little lower
  (bash-downs 4.8 to 3.9 and 4.7 to 3.8). **Crossings are not changed by it** (they are the get-up's own pose:
  step 5). About 1 m/s of jump is left; not known how much of that is the measure's own floor.
- **Still rough:** his sword hand at 6 m/s about 0.4 s in, as its pin lets go in a get-up from his front (old way
  too); his shield arm's forearm at 6.5 m/s 0.2 s into one get-up from his front (new only); his sword hand
  sliding out from under his shoulder starts with no easing (probably the 4.9 m/s jump seen once in a fight of
  six; not traced). In the fights of six the take-over did not measure better (very few get-ups; hands lie up to
  1.2 m from where the get-up wants them there). What was tried and not kept
  is written in `GetUp.TakesOverAsHeMoves`'s comment and the README's entry.
- **The "rougher take-over" with the trunk's turn on was a fault in the headless stub, not in the game** (above,
  "Testing without Unity"). He was taken up at 0.2 m/s, not rolling. With the stub mended and step 4 on, the turn
  at 0.25 does not make the take-over rougher (fastest part 2.3 to 3.1 m/s from the four sides), but it still does
  not finish the gathers (0, 1, 3 and 2 of 3 ended by the clock; hips 11 to 27 degrees off). Still off (0).

**The afternoon of the 19th (after his third look, 12:26):**

- **Plan step 3, a first build: he gathers himself before he gets up** (Combat Feel switch of that name, on;
  headless only). 0.4 s before he would rise the get-up is set out, and he goes on as the fall body with the
  get-up's start as the pose his muscles draw him to (`StepFallBody`, `BeginGetUp(true)`); the get-up takes over
  when his hands and feet are within 12 cm of it (from his hips) or after 0.6 s. His sword arm is left out.
  Bash-downs, on against off: crossing in a get-up's first 0.75 s 0.19 to 0.11 s and 0.22 to 0.14 s; the fastest
  part 5.9 to 4.2 m/s. **About half the gathers end by the clock** (from a side): nothing turns his trunk yet.
  **Seen by him at 15:33: "it looks fine for where it is now, you can continue."** His log of 15:29 (56 get-ups):
  crossing in a get-up's first 0.75 s 0.05 s a get-up (the shield bearer 0.16 s), the fastest part 4.2 m/s.
- **Turning his trunk in the gather: built and left off** ("Gather Turns His Trunk", 0; `Upper.TurnHips`). At a
  quarter of his weight it did not finish the gathers from a side. (It was also measured to make the take-over
  rougher, 11.5 m/s against 4.1: that was the stub's fault, see the evening's notes above.)

- **Thrown by nothing after a tube's fall: mended** (his log of 13:26: 14 times a target just up was thrown at up to
  6.2 m/s with no blow, 6 of them down again). A blow's knock that was still waiting when he fell lay with him and
  threw him as he stood. A waiting knock now goes with the fall (`ClipFighter.Fall`). Seen by him at 14:03: "much
  less strong and does not knock them over".
- **The weaker push left after it: a test target walking back to its spot** (his log of 14:01: just up, 0.5 to
  0.6 m off its spot, a guarding target shuffled back at 0.7 m/s with nothing near it). A target dummy that fell
  now stands where it got up (`FightLoop.DummiesStayWhereTheyGetUp`); the tubes are set beside where it stands, once
  all are up. Not checkable headless (the headless tube lineup has no target dummies). **Seen by him at 15:47: "the
  target stays."**

- **He saw the morning's build in Unity:** "did not see the shield going through the body as he fell this time."
  His log of 12:12 agrees (21 falls): crossing while falling and lying 0.33 s in all (3.25 s at 08:07), no body
  thrown back onto its pose. While getting up 2.35 s, all of it in the first 0.75 s.
- **His answers on the get-up plan, all as recommended:** physics leads the gather; up to half a second longer on
  the ground if a case needs it; the shield flat on the ground out to his shield side, pushed up on that forearm,
  coming up edge first; build in the plan's order; a get-up may differ each time.
- **"Look here" on the panel** (his new standing rule, above): the box "Test this" and the yellow frames. Built
  blind, and **seen by him at 15:47: "the yellow lines for the UI debug is good."** Keep doing it the same way.
  The panel check ends with a `LOOK HERE` line.
- **Plan step 1, built:** every get-up reports its first 0.75 s (crossing, fastest part, how far each hand went):
  in his `getup` lines, the headless report and the panel's "His last get-up". "Hold him at the start: on" (Tubes,
  "Falls and getting up", "The start of a get-up") stops the game as a get-up begins.
- **Plan step 2, built and left off:** "A Get-Up Starts Nearest To How He Lies" weighs every start (his hips' way,
  from his side, either hand posting) by how far his joints are from it. Alone it does not help (crossing in the
  first moment 0.19 s a get-up against 0.14 s over twelve bash-downs): the nearest start often posts on his shield
  hand and lays the board under him. For step 3 the cost has to count the board. `FIGHT_STARTNEAREST=1`.

**His second look (19 November, 08:26), worked in the same morning; the falls since seen by him in Unity:**

- His log of 08:07 (28 falls, 27 get-ups): his own shield and sword crossed him 3.25 s while falling and lying and
  7.08 s while getting up, and **89% of the get-up's crossings are in its first 0.75 s** (the blend from how he lay;
  parts at 6.7 m/s on average there).
- **A fallen fighter's shield stays solid against his body** (Combat Feel switch of that name): trunk, head, thighs,
  shins, his other arm and the sword in it, while he falls and lies. Six bash-downs in a run: crossing while falling
  and lying 0.55 s to 0.00 s (`Tools/Headless/downs.sh`). Getting up it still passes (the plan).
- **Fighters keep apart off their feet too** (Combat Feel): a body needs 1.1 m to get up in, hips to hips; one on his
  feet steps off it, one lying waits for room (3 s at most). The cause of two bodies locked in one place: the
  keep-apart rule did nothing if either was down. `Tools/Headless/room.sh`. The lock itself was not reproduced
  headless.
- **The lab's "Stumble him" waits until both stand.** Pressed on a man who was down it put him on the spot at once
  (his body thrown 75 cm back onto its pose): some of the "leap" he saw was the button.
- **A held thing caught on the wrong side of his body is let through** (Combat Feel; on his feet): a board stopped
  under his thigh hung 29 cm below its cover for nine seconds. Rare.
- **The tremble budget reads red in the six with the off-feet shield on** (10.5%, 11.0%; off 7.5%, 7.3%). Traced in
  one seed to the one shield bearer after his fall: he lay on his side, not his front, and got up from there. Why a
  get-up from a side leaves more tremble is not found. Crossings on his feet are as red as before, not redder.
- **The plan "From how he lies to how he gets up"** (above): he reads how he lies, gathers himself as the physical
  body he fell in, and the get-up takes over when he is close. Five questions wait for him.

**Stumbles: built overnight on the 19th; Renier played them that morning: "this looks pretty good so far", it
reads as a stumble. His notes were worked in the same morning (headless only, waiting for his second look):**

- A bash lands when the board touches him (it landed 30 cm short, and he flew off untouched).
- No hit stop on a bash, and his knees give at once: the legs no longer stand stiff while the upper body recoils.
- Each bash "pushes through" (a lever each). Small is harder and its arm goes further; hard a little harder; a new
  **extra hard** bash throws a man 4.5 m back (lab only: "Stumble him: Extra hard"; no fighter throws it yet).
- After a bash the basher holds his ground (a likely cause of the shields stuck together; **his logs of the 19th
  were not in the repository, so his cases are unread: ask for them**).
- Decisions of his: use the new system completely (no going back to speeds written in); hard should knock hard;
  extra hard much harder; small must clear his space.

**The first build's own notes.** Two switches, both on.

- The momentum ledger ("A Bash's Speeds Are Worked Out"): what the two leave a bash with is worked out from what
  each brought. A counter is no longer a draw.
- Stumbles ("Off Balance He Stumbles On Real Steps"): a fighter thrown off balance steps for the point that would
  stop him; caught in one step or several, turned, or down. Both men of a bash, and a body thrown by a heavy blow.
- To see it: duel lab, Actions, "Stumbles and falls": "Stumble him: Small / Medium / Hard", with the old/new rows
  under it. Headless: `Tools/Headless/four.sh tag`, `Tools/Headless/stum.sh`.
- Still open: the arms do nothing of their own yet; when fighters should throw the extra hard bash by themselves;
  how far a man may be turned ("Turned by": he did not follow the question; see the chat's answer in the README).
- **It breaks the shield budget (below):** with both switches on, his own sword and shield cross him 0.6 to 1.5% of
  the time on his feet (old way 0.2%). It is not the upper body tipping (tried at 0). On one seed every crossing was
  his blade through his own shield **while bashing**: since the 19th the small bash's arm goes further, and nothing
  takes his sword out of the board's way in a bash. On another they were in two made attacks thrown on a turn.
  Next: the sword arm clears for a bash.
- Figures: the README's entry of 19 November.

**The shield budget is in the check runs (19 November).** Every headless fight ends with a `CROSSINGS` and a
`TREMBLE` line ("OK" or "OVER BUDGET"); `Tools/Headless/checks.sh` gathers them into `$S/checks.verdict`. On his
feet: all crossings under 0.5% of the time, none longer than 0.1 s; his board shows a tremble no more than 8% of
the time standing or moving (the 8% is a proposal). Off his feet is counted apart, not held to it yet. **Any new
action has to pass it before it is handed over.**

**The sword that shook as a fighter got up: the night's mend did not look improved to him (19 November). His
rule, built that morning, headless only:** the sword hand is pinned to the ground where it lay, may turn only a
few degrees while pinned, and never turns fast (Combat Feel "A Rising Fighter's Sword Hand Stays Where It Lies";
180 degrees a second at most, 3 m/s at most; the last word on that hand each step). From his back the tip's
tremble as seen went from 63 to 22 mm. To see it: target test, "Getting up, his sword hand: stays where it lies /
goes with the keys (old)". Waiting for his eye.

**The night's own mend, kept under it:**

- The cause: the sword hand is turned only as far as a wrist turns from the forearm, and about a second into a
  get-up from the back the forearm points the blade up to 49 cm into the ground. The rule that keeps held things
  out of the ground threw the whole arm up by that much in one step, the blade swung over his head, and came back
  the next step.
- The mend (Combat Feel "A Rising Fighter's Blade Is Turned Clear Of The Ground"): the wrist turns the blade on,
  only as far as lifts it clear. From the back the blade's largest turn in one step went from 140 to 23 degrees,
  the tip's tremble as seen from 140 to 63 mm.
- To see it: target test, "Getting up, his blade: turned clear at the wrist / the arm thrown up (old)", then
  "Falls on demand" from his back. Headless: `Tools/Headless/gup.sh n "270 180"`, old with `FIGHT_GETUPBLADE=0`.
- **Not mended: the shield's tremble in a get-up** (the board as posed 5.3 mm, over 5 mm 27% of the time). Cause
  not found; it is not the ground clamp lacking a memory (tried). And his sword hand still travels at 5 to 8 m/s
  for a fifth of a second as the arm comes round: the get-up's own motion, for his eye.
- `FIGHT_GETUPTRACE=1` (`GETUPHELD` lines) is temporary and can go once he has seen it.

## Roadmap

**Stumbles (step 9c) have a first build (19 November, above). Next on them, after his look:** tune by his answers;
arms thrown out for balance; a look at feet crossing in a stumble to the side; a sword blow's throw worked out as
a bash's is (it is still the old rule's number, uncapped).

**The night of 19 to 20 November: his order (said at 21:29, going to bed).** "Continue working through the
night... if you get to a point that you need to push and get feedback you can just move on to another task." In
this order: (1) finish the get-up fixes (knees and sword hand: pushed 22:10; then the shield through him, step 5);
(2) back to stumbles; (3) bodies that absorb a hit; (4) kicks; (5) pouncing on an opening; (6) downed fighters.
Each gets its switch, lab toggle, measure, push and (if it needs his eye) its 3b2b; the panel's "Look here" list
covers the whole night, not only the last push. He gets one short list in the morning.

**The first moment of a get-up** (`Docs/Plans/2026-11-19-…`; his answers are in). Steps 1 and 2 are built, step 3
has a first build he has seen ("fine for where it is now"), step 4 is built and waits for his eye. **Next on it, in
this order:** the trunk's turn in the gather (built, off: `GatherTurns`; at 0.25 it is too weak or too short to
finish a gather from his side: try more strength, a longer gather for those cases (his answer 2 allows half a
second), or both; about half the gathers still end by the clock for want of it); then step 5, the board carried
round him (his answer 3): the crossings left are the get-up's own pose, and so are the fast arms left after step
4. The start's cost (step 2) must count the board before it is turned on.

**Bugs and leftovers**
0. A get-up from a side leaves the board trembling more for some seconds after (19 November, above): cause not found.
1. The get-up shield tremble (above; the sword's flip is mended, waiting for his eye).
2. Shield and sword while falling and lying: mended on the 19th (above). While getting up: it is the plan's step 5
   (from his front the board is in his hips for 0.35 s as the get-up begins and passes his head at 1.1 s; the pose
   itself has it). **Waits on a decision of his:** where a hand with a shield on its arm posts, and where the board
   lies while he pushes up (my recommendation is in the plan).
3. The lock is in (above). It reads red with the night's stumbles on.
4. The blade's tip trembles on his feet: narrowed on the 19th, not found. Standing alone it does not. In the six
   it is nearly all the sword-alone fighters' parry guard. For a sword and shield it comes in tenth-of-a-second
   bursts as he walks, cause unknown. `FIGHT_SHAKELOG=1` writes `TIPLOG` lines; the README's entry has the rest.
5. The walk lab's own panel has section colours and a search box since the 19th, **built blind** (it compiles; no
   headless check draws that panel). To be looked at in Unity.
6. Smaller: the sword's upper arm is not covered by the give-way; walking step 4 is weak; the turn's cut
   misses at 150 degrees.

**Impacts that carry weight** (on the 18th he said the three steps sound great, and that the bash will cause
most stumbles)
1. The momentum ledger: built (19 November).
2. Bodies that absorb: knees give, hips drop, the arm folds, feet slide. **Not built.** Needs his eye.
3. Off balance: built as stumbles (19 November). Left of it: a leap landing on someone, kicks built on the door.

**Fight Rhythm, still to come**: walking step 6; kicks (on `ThrowBody`); pounce on an opening; Downed play; combos; the fight of six.

**Waiting on a decision from him**: what being fooled by a fake costs (today: nothing); whether real attacks
should be more frequent; whether a fighter holds off while his enemy is down.

## Starting a new chat

`Docs/NextChat-prompt.md` is the prompt Renier pastes into a new chat (written 19 November, evening, for the
trunk's turn and step 5). When a chat ends with work left, rewrite it for the next one: where to start, what to do in order, where the
code is, how to measure it, and what cost time.

## Keeping this file true

At the end of a step: move it from "Roadmap" to "Where things stand", rewrite "In hand", and change the date
at the top. Record a decision when it is made, with its reason, here or in the plan document. Anything worth
knowing that lives only in a chat is lost when the chat ends.
