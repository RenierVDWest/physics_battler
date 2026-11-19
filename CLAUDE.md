# Physics Battler: start here

This is the briefing a new chat reads first. It is kept short on purpose. The full build log is `README.md`
(over 6,000 lines: search it, never read it whole). Update this file at the end of every milestone or step:
the "Where things stand" and "Roadmap" sections must always be true.

Last updated: 19 November 2026, morning (after Renier's first look at the night's stumbles).

## The project in a few lines

A Unity auto battler (Unity 6000.6.7f1, URP, PC) that is meant to be a granular combat simulator: fighters on
physical bodies who move, read each other, aim blows at body parts and meet each other's weapons and shields
for real. Renier (the developer) playtests in Unity and pushes logs. Claude works headless in a cloud
workspace and cannot open Unity.

- Repository: `RenierVDWest/physics_battler` (private). **Work on the branch `physical-body`.**
- Other branches are history: `main`, `locomotion-lab`, `arm-rebuild`, `backup/active-ragdoll-v1`.
- Only Mediums fight for now (Large and Small wait). Loadouts: sword and shield, sword only.

## How Renier wants the work done

These are standing rules. They hold in every chat.

**Talking to him**
- Short, simple explanations, bottom line first. He wants the high-level picture in plain words.
- When a change needs his eye, make it happen on demand: a debug button, an old/new toggle in the lab.
- Report honestly what is unverified. Headless numbers are not what he sees in Unity: say which is which.
- The 3b2b rule: every document made for him, and every finished and pushed step that needs his look, gets a
  "3b2b" (a short narrated, animated explainer). It is generated before the update is sent: the link first,
  a short note after. Ordinary back-and-forth stays normal. It is made by a subagent on Opus at medium
  effort, never on the main model (the `3b2b` skill has the details). A copy of the kit is in
  `Tools/Explainer/kit/`, so it can be made with his computer off; `Tools/Explainer/example-lane/` is one of this
  project's own explainers to copy the format from (its note, script and the generator of its drawings).
- Do not spawn subagents for anything else.

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
cd Tools/CompileCheck && ./get-physx.sh      # fetches PhysX bindings (not in the repository)
../Headless/b.sh                             # builds the four projects: wants "0 Error(s)" four times
```

Rules that cost time when broken:

- **Never build while a fight is running** (`b.sh` refuses). A rebuild under a running fight spoils its result.
- Two fights at a time at most (two cores). Two fights of six for 40 s, side by side, take about ten minutes.
- A command in the foreground is cut off after about two minutes: start runs in the background (the scripts
  do) and wait in a loop for their `.done` file.
- Fights are chaotic: after any change a run differs. One run is a direction, not a rate. For a firm number
  use the aim test (nearly repeatable), the same build with the switch on and off, or a second seed.
- The stub lacks some of Unity (`Vector3Int`, `Vector2.normalized`). Contact modification does nothing
  headless. Collision callbacks reach only objects that carry the script.
- Unity has the last word. A headless pass is "looks right in the numbers", not "verified".

The panel can be checked without Unity:
`dotnet Tools/CompileCheck/bin/Tests/Debug/PhysicsBattler.Tests.dll panel [battle|duel|aim|tubes|targets] [search words]`
prints "PANEL OK".

## Plans and documents

Every plan document has a copy in `Docs/Plans/` (30 files; `Docs/Plans/README.md` is the index, with each one's
state). The live versions are Claude documents, linked from the index: progress notes are added there, and the copy
here is refreshed when a plan changes a lot. Read the one for the work in hand, never all of them.

The three in hand:

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
  the time on his feet (old way 0.2%). One crossing is in the stumble itself (the upper body tips, the shield's
  cover does not); the rest is not pinned down. First thing to look at after his look.
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

**Bugs and leftovers**
1. The get-up shield tremble (above; the sword's flip is mended, waiting for his eye).
2. Shield and sword while falling, lying and getting up: measured on the 19th (from his front the board is in his
   hips for 0.35 s as the get-up begins and passes his head at 1.1 s; the pose itself has it). **Waits on a decision
   of his:** where a hand with a shield on its arm posts, and where the board lies while he pushes up.
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

## Keeping this file true

At the end of a step: move it from "Roadmap" to "Where things stand", rewrite "In hand", and change the date
at the top. Record a decision when it is made, with its reason, here or in the plan document. Anything worth
knowing that lives only in a chat is lost when the chat ends.
