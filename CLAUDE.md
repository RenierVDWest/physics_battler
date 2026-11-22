# Physics Battler: start here

This is the briefing a new chat reads first. It is kept short on purpose. The full build log is `README.md`
(over 8,000 lines: search it, never read it whole). **New entries are added at its end, never written over it: after
writing one, `wc -l README.md` must say more than before.** (On 21 November, 11:44, a commit left only the newest entry
in it, and two more commits did the same; found and put back whole at 14:20, nothing lost.) Update this file at the end of every milestone or step:
the "Where things stand" and "Roadmap" sections must always be true.

Last updated: 22 November 2026, 12:05 (the clothes looser and swinging, logged into his playtest log; headless only). Before that: 22 November 2026, 11:20 (the clothes plan's steps 1 to 3 built after his "go" at 09:21: a shirt and trousers carried on the baked drawn body, a last word, joint balls; headless only, built blind, waiting for his eye). Before that: 22 November 2026, 08:55 (his look at the robe at 08:46: "still not working"; he wants a long-sleeved shirt and trousers to the feet and a cloth that never goes into the body; found that the cloth was kept out of the physics shapes while the drawn body is 2 to 5 cm fatter; a new research plan is written, nothing built, four questions wait for him). Before that: 22 November 2026, 08:20 (his morning look answered: his own combo no longer fells him (07:20); the clothes rebuilt as one open robe whose sums run on other cores in a Burst job; the shock wave has a jolt beside the jelly; all headless only, waiting for his eye). Before that: 21 November 2026, 23:59 (his night order of 21:33 done: the shock wave (22:50), the clothes and the combos plan's step 7 (23:55) are built, headless only, waiting for his eye; the combos plan is fully built). Before that: 21 November 2026, 22:50 (the shock wave built). Before that: 21 November 2026, 21:22 (the cloth plan's film is made and linked; nothing else changed). Before that: 21 November 2026, 20:54 (he said the shock wave plan "looks great" at 20:47 and asked, before building it, for research and a plan for real moving cloth: samurai clothes on the fighters; that plan is written, nothing built, six questions wait for him). Before that: 21 November 2026, 20:08 (he said "all this looks great" of step 6, the show, at 20:01 and asked for research and a plan for shock waves through the body on a hit, "a bit over the top": the plan is written, nothing built, five questions wait for him). Before that: 21 November 2026, 19:09 (combos, step 6: the show, built blind and pushed, waiting for his eye and ear; he said "everything looks good here. Proceed to the next task" at 18:32 of step 5). Before that: 21 November 2026, 17:51 (combos, step 5: the dance fights with combos, built, measured over four dances and pushed, headless only, waiting for his eye). Before that: 21 November 2026, 16:15 (after his look at step 4: "looks good. proceed"; the combo that stopped when a shield took its second blow mended, unconfirmed in Unity; his speeds the defaults in the labs and the dance; the lab dummy middling; step 5, the dance fights with combos, has a first build being measured). Before that: 21 November 2026, 15:07 (combos, step 4: against a man who defends, built and pushed, headless only, waiting for his eye; he has not yet looked at the 13:57 fixes either). Before that: 21 November 2026, 13:57 (after his look at step 3: the big one made big and given its own speed, a new spin from his recording, the first blows keep him in reach; he said "Ok that looks good ... Do this and proceed" at 12:45, so step 4 is next without waiting). Before that: 21 November 2026, 11:43 (combos, step 3: three blows and the big one, built and pushed, waiting for his eye; he said "Ok this all looks good to me. you can proceed" at 10:41 to the two speeds, the carried jump and the panel). Before that: 21 November 2026, 09:49 (two attack speeds; the fall after a long jump in; the panel held to its width). Before that: 21 November 2026, 07:57 (his morning look answered: the jump in for the second blow, an attack speed slider, the dummy's spot, the pounce). Before that: 20 November 2026, 22:08 (step 2 of the combos plan is built and pushed, waiting for his eye; he said "Yes it sounds good. Go." to the combos plan at 18:42, every
default taken. **Step 1 of the combos plan is built and pushed, waiting for his eye:** every recording's blade is
measured by a report, and the duel lab can draw a made attack's planned blade path and the path the sword really
took. A view only: nobody fights differently. Next: step 2, two blows as one swing. Corrected at 20:15: what
the first push said about blades "hitching" at a body. **21:15, after his first look:** in the duel lab the
target ended right against the swordsman after his first pounce; mended, the swordsman now steps back to his
distance, and at 21:22 his log showed it works in Unity. His logs show the blade sticking at a body in Unity too).

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
- **A fighter goes for a man who is down** (20 November). In the duel lab and the dance; the fight of six only
  when he says.
- **Combos, his five goals (20 November):** one fluid motion, never attack, stop, attack; each blow starts where
  the last one's blade ended; a blow slashes through and does not stop at the part aimed at; they feel epic and
  powerful; the last blow is the most powerful. Combos are also a way to break a shielded guard (17 November).
  From 4 November, still his: about one second from the first hit to the third; a man driven back is followed;
  the long leaping chops only end a combo; streaks pale grey.
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
| `Locomotion/AttackMaker.cs`, `AttackScore.cs`, `AttackOnTheMove.cs` | Made attacks: a recording taken apart into its parts' tracks, changed by style and aim, put together again; and laid on the walk for a blow on the move. |
| `Locomotion/BladeFlow.cs`, `BladePath.cs`, `Sandbox/BladePathView.cs` | Combos, step 1 (measuring only): a recording's blade against the body, its corners and how two cuts join; a made attack's blade path as planned and as it went; the lab's drawing of it. |
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

Every plan document has a copy in `Docs/Plans/` (32 files; `Docs/Plans/README.md` is the index, with each one's
state). The live versions are Claude documents, linked from the index: progress notes are added there, and the copy
here is refreshed when a plan changes a lot. Read the one for the work in hand, never all of them.

The five in hand:

- `2026-11-22-clothes-that-never-go-into-the-body-research-and-plan.md`: **the clothes again, after his look at the
  robe (22 November, 08:46): a shirt and trousers that never go into the body.** He said go at 09:21; steps 1 to 3
  built (11:20), waiting for his eye; next step 4, the wobble.
- `2026-11-21-cloth-that-moves-samurai-clothes-research-and-plan.md`: **real moving cloth on the fighters (21
  November, 20:47).** Steps 1 to 3 and a first piece of step 4 built that night, waiting for his eye.
- `2026-11-21-shock-waves-through-the-body-research-and-plan.md`: **a faked wave through the body on a hit (21
  November, 20:01).** All six steps built that night (22:50), waiting for his eye.
- `2026-11-20-combos-that-flow-and-end-in-a-bang-research-and-plan.md`: **the work in hand: the combos plan.** A
  combo is one swing with several hits: the follow-through of each cut is the wind-up of the next. Seven steps;
  **step 1 (the blade's path measured and drawn) is built and waits for his eye; step 2 (two blows as one swing)
  is next** ([live](https://claude.ai/artifact/X5YKsPDm3LZYzefQoofnuX)).
- `2026-11-19-from-how-he-lies-to-how-he-gets-up.md`: the get-up plan; its leftovers wait while the combos are in
  hand (the roadmap below). His five answers are in (all as
  recommended). Steps 1 and 2 are built, step 3 has a first build he has seen, step 4 (the take-over) is built and
  waits for his eye; **next: the trunk's turn in the gather again; then step 5**
  ([live](https://claude.ai/artifact/FAVSQjVZCbJ87jf6aargPa)).
- `2026-11-18-fight-rhythm-from-recordings-plan.md`: **the main plan**
  ([live](https://claude.ai/artifact/GmgWWcXjaoQvCj5n5U5wLh)).
- `2026-11-18-a-shield-that-stays-outside-its-bearer.md`: mostly built; leftovers in the roadmap below
  ([live](https://claude.ai/artifact/9UZ1xrAueNhCM6Yn4wg2NN)).
- `2026-11-18-impacts-that-carry-weight.md`: three steps wait
  ([live](https://claude.ai/artifact/HkqvPHoe22TWSjZwxvdW5V)).

## Where things stand (20 November 2026)

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
- **The shield carried round him (the plan's step 5; 20 November, 00:30):** Combat Feel "A Rising Fighter's Shield
  Is Carried Round Him"; lab row "Getting up, his shield"; `GetUp.ShieldCarriedRound` (in `HoldShieldUp`: `raise`
  by shoulder height, the carried arm placed by plain geometry, `PoseAt` gives the gather the same place). From
  his front or a side the board lies on the ground out to his shield side, his hand on its grip, and swings up
  round his side. **A reading taken:** the plan said "his forearm flat on it"; the shield has a centre grip, so
  he pushes up on the gripping hand instead. Headless: in his own body off his feet 0.47 s to 0.00 from his front,
  0.52 to 0.12 from his right. Costs: the fastest part early 3.0 to 3.9 m/s; the rim 3 to 5 cm in the ground for
  0.1 s as it turns up.
- **Arms in a stumble (20 November, 00:30):** Combat Feel "A Stumbling Fighter's Arms Go Out For Balance" and
  lever; lab row "Stumbling, his arms" and lever "Arms go out by" (duel lab, "Stumbles and falls");
  `ClipFighter.StepArmsOut`, the last thing that poses his arms. Headless: his hands 72 to 101 cm apart in a
  medium bash's stumble, 97 to 121 in a hard one countered; the stumbles themselves unchanged.
- **Bodies that take an impact (the impacts plan's step 4, a first build; 20 November, 01:15):** Combat Feel "A
  Body Takes An Impact In Its Knees And Hips" and three levers; lab row "An impact, his knees and hips" and levers
  "Contact time", "Knees give to it", "Trunk folds to it" (duel lab, "Stumbles and falls");
  `ClipFighter.PressInto` (called at the top of `ThrowBody`), `StepPress`, `LocoBody.Pressed`. Headless: the
  basher's hips on a held hard counter 4 cm to 11. Not built: feet sliding, the head lagging, slow motion.
- **Kicks, a first build (the fight rhythm plan's step 9b; 20 November, 01:40):** Combat Feel "A Kick Throws A
  Body" and two levers; lab "Kick him: Front kick" with "Kick power" and "Kicks into him" (duel lab, "Stumbles and
  falls"); `ClipFighter.KickAt`, `StepKick`, `Kicked`; the recording `sns_kick` (role "kick"), baked from the one
  kick sample in the repository. Lab only: no fighter kicks by himself. **The kick pack's five standing kicks and
  the Capoeira kicks are not in the repository.** Headless: on a body 112 cm up, thrown at 2.6 m/s, two stumble
  steps; on a board 1.6 m/s, one step; at power 4.2 he goes down.
- **The pounce, a first build (the fight rhythm plan's step 10; 20 November, 02:00):** Combat Feel "A Fighter
  Pounces On An Opening" and "Pounce Reads In"; lab row "An opening: he pounces / he stands off (old)" (duel lab,
  "Stumbles and falls"); `DuelLab.StepPounce` (the duel's mover only; `FightLoop` untouched). The other stumbling,
  his guard open or his back turned: in reach he strikes at once, out of reach he leaps in and strikes. Headless:
  after a small bash the blow is thrown 0.2 s after the opening began and lands; after a medium or hard bash or a
  kick he is not free for 0.7 to 1.0 s and the blow (1.2 to 1.7 s after) is blocked or parried.
- **A man who is down, a first piece (the Downed play plan's steps 0 and 1; 20 November, 02:20):** Combat Feel "A
  Fighter Goes For A Man Who Is Down" (**on by default since his decision of 20 November, 16:11**; it was "A
  Fighter Strikes A Man Who Is Down", off; headless it stays off unless `FIGHT_DOWNED=1`, so that falls and get-ups
  are measured on a man left alone; to watch a get-up undisturbed in Unity: "he holds off (old)"); lab row
  "A man who is down" and button "Strike him where he lies" (duel lab, "Stumbles and falls");
  `DuelLab.StepDowned`. Headless: today's made blows, aimed at the chest of a man lying 13 to 16 cm up, reach and
  land (two of two): the plan's first question answered yes. The rest of that plan is not built.
- **The second pass, after his look (20 November, 08:30; the README's entry of that time has the tables):**
  - **He saw (Unity, three logs of 06:28 to 06:42):** knees fine; the kick works; the bare-handed dummy "stumbles
    back pretty far with legs moving pretty fast, where I would expect it to fall"; of a man who is down, "he ran,
    stopped then attacked. That won't work": running at him and striking "must be one motion without a pause",
    as the turn-and-strike had to be, and a man knocked far must be run after. He wants a test scenario for it.
  - **One motion:** `DuelLab.GoFor` / `StepGoing`: he runs at the spot (Combat Feel and lever "He Goes For Him
    At", 3.6 m/s) and throws the blow while still running, over his run, half a second's run before he is in
    reach; of the blows he can throw on the run, the best that lands within 0.75 s
    (`AttackOnTheMove.LandsWithin`). The pounce and the blow at a man who is down both use it; the leap is gone.
    A man who is down is run after from any distance, from the moment he is off his feet. Headless: every blow
    thrown over his run, never slower than 2.4 m/s before it landed (down), 0.8 to 2.0 (pounce, from near).
    **Rough:** aimed at the chest of a man lying, it met a shin, an arm; the pounce's blow is mostly blocked.
  - **One-press tests:** lab row "Knock him down and strike him: Near / Far" (Far is a kick of 8 m/s made only
    for the test).
  - **Kicks soft, medium, hard:** `KickAt(at, whenever, kind)`; Combat Feel "Kick Power" 2.6 (soft), "Medium
    Kick Power" 3.3, "Hard Kick Power" 5.2; three lab buttons. Headless: soft a step or two; medium four stumble
    steps over 4 m, on his feet (a bare-handed man goes down); hard down, through a board too.
  - **A man not in his guard stumbles and falls too:** `ClipFighter.UnguardedMenStumble`; Combat Feel of that
    name; lab row "Not in his guard". The door (`ThrowBody`) had only let a man standing in a guard stumble or
    fall; anyone else was shoved however hard. It reaches the fight of six.
  - **The jump as he stands:** in half his morning's get-ups a hand jumped 8 to 13 m/s one step after he stood.
    Cause: the night's arms-out kept, through a fall, how far his arms were out as he went down, and put them
    out again as he stood. Forgotten once he is up (`ArmsOutForgottenOnceUp`). Headless (a man kicked down): 20.6
    m/s to 3.4 to 5.4; the sixes 19.0 on average to 1.8 to 2.0. **A lesson: the lab's falls on demand fell him
    without a stumble, so the measure of the night could not see it. Measure a get-up after a real knock-down.**
  - **Slow motion:** lab row in "Stumbles and falls" (full, half, a quarter, a tenth).
  - Sixes (40 s, seeds 5 and 7): all budgets kept (the old way was over on seed 5).
  - Its film (3b2b, made by Opus at medium effort): https://claude.ai/artifact/N5MwJrqpxuP8x1ZEeqDxs8
- **After his look at the second pass ("this all looks good"; 20 November, 11:52; the README's entry of that time):**
  - **His word:** do the next things I listed, "but not the fight of six, keep it in the dance for now".
  - **The running in is a class now:** `RunIn` (in `DuelLab.cs`): `StepPounce`, `StepDowned`, `Step`. The duel
    lab has one for its mover, each dancer has one (`Dancer.Run`). Its switches are statics on `RunIn`.
  - **A stab for a man who is down:** he runs to beside the man's chest (0.8 m, square to how he lies), bends to
    it and throws his thrust, at the first of chest, belly, hips, head, neck that is open. Combat Feel "A Man Who
    Is Down Is Struck From Beside Him", "He Bends To A Man Who Is Down By"; duel lab row "A man who is down, his
    blow". Headless, six knock-downs, what it met first: trunk or head 2 of 6 the old way, 5 of 6 now.
    **A lesson: a blow's "it met" was the last part it went through; the report now lists them in order.**
  - **The pounce goes for what is open** (`RunIn.StrikesWhereOpen`, by `Openings.LookAt`; the soonest blow), and
    **a stumbling man cannot guard** (`ClipFighter.StumblingOpensGuard`, only where `StumbleOpensGuardHere` is
    set: the duel lab and the dance). Headless: two of four pounces land (none did). **For every fighter it put
    the fights of six over their crossing budget (0.8 to 1.0% against 0.0 to 0.3%), so the six is as it was.**
  - **By themselves in the dance:** `Dance.Pounces`, `Dance.StrikesDowned` (**on in the dance**, a reading
    taken: his decision on holding off is still open, and elsewhere `RunIn.StrikesTheDowned` is off),
    `Dance.Kicks`, `Dance.KickShare` 0.35: a fighter with no shield kicks for space and throws a hard kick as his
    guard-breaker; one with a shield kicks instead of bashing 35% of the times. Dance panel rows "An opening
    (dance)", "A man down (dance)", "Kicks (dance)"; a line "By himself" per dancer; report line `DANCEOWN`.
    Headless: pounces and kicks happen by themselves; **nobody went down in my dances, so going for a man who is
    down was not exercised in the dance.** The dance's own crossings rose (0.56% to 0.97% in one dance).
  - Its film (3b2b, made by Opus at medium effort): https://claude.ai/artifact/JfCuCLnTgYQgfyaUB1WJ3C
  - **Not done of what he asked: the get-up leftovers** (the sword moving while its hand is in the air, the shin
    kick at the start of one get-up in three, the shield's rim in the ground for a tenth of a second).
- **The night's two films** (3b2b, made by Opus at medium effort): the get-up,
  https://claude.ai/artifact/686Uo8dQEjeAjPCj1q8aHm ; the five first builds after it,
  https://claude.ai/artifact/KjzrrmCJAUsAwrSp4XBNnR . Their notes are the chat's morning update.
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

**Combos, step 1: the blade's path, measured and drawn (20 November, 20:00; headless only, waiting for his eye).**
A view only: nobody fights differently. The README's entry of 20:00 has the tables.

- **The recordings' blades** (`BladeFlow.cs`; `Tools/Headless/flow.sh`): each cut's hand and point against his hips,
  the top of its swing and where its blade turns after the landing, the two corners those are in, how far it is
  carried through; and for each pair of cuts where the first is best left and the next best entered, with the
  "bend" there (what there is to make up, as time). **The check passes:** the two places where the performer went
  straight on are the best two joins of 144 (bend 0.02 and 0.03 s; hit to hit 0.57 and 1.20 s, as the raw take).
- **The map against the plan's:** the same six corners; two corrections: the thrust comes back over his sword
  shoulder (high right), not high on the left; the lunging chop ends low right. **The joins:** forehand into rising
  backhand needs a bend of 0.11 s (inside the plan's 0.15 s) and is 0.40 s hit to hit at the recorded pace, left
  0.23 s after the landing and entered 0.17 s before the next; forehand, rising, forehand is 1.10 s first hit to
  third. A chop into a rising backhand and a thrust into a spinning backhand do not join as the plan had them.
- **In the game** (`BladePath.cs`, `BladePathView.cs`; Combat Feel "The Blade's Path Is Drawn", off; lab row "The
  blade's path: drawn / hidden" and the readout "His last blade path", in the duel lab under "Strike at" and in the
  aim test): the planned path of a made attack's blade (planted, or on the move by doing ahead of time what the walk
  does each step), a mark where it lands, and the sword's real path over it with a dot a step (bunched dots: a slow
  blade). Headless (`FIGHT_BLADEPATH=1`): the
  real point is 14 cm from its planned line on average, 0.02 s behind. A `bladepath` line goes to his playtest log
  for each attack while it is on. **The drawing is built blind** (the legs' view's method, which he has seen).
- **A finding for his third goal (a blow slashes through):** a blade is frozen 0.05 s as it lands (the hit stop,
  meant). Headless, of 19 blows that landed, 16 then swept on at least 70% as far as planned, but the sword starts
  again slowly: in 7 its point stayed under 5 m/s for a further 0.07 to 0.15 s (it sticks at the body, then whips
  on), and 3 fell well short (a forearm, a shin, an upper arm). **The cause (traced headless, not seen in Unity):**
  the pose sweeps on as the rule says, but the sword is a solid thing (`UpperBodyArticulation.Hold`) and so is every
  part of the struck body, so the sword as seen is held by the body (1 to 6 m/s while its pose goes at 8 to 22)
  until it slips past. The drawing's dots and the readout ("frozen 0.05 s, then under 5 m/s for 0.10 s more") show
  it. **Step 2 starts by mending this** (a blade the rules let through should pass the body it cut), **after
  reading his next log's `bladepath` lines to see what Unity does.**
- Its film (3b2b, made by Opus at medium effort): https://claude.ai/artifact/DicGDRVN9k8W211a5JwBj6
- **His first look (20:44) and what his two logs show:** the sticking is in Unity too (7 of 19 landed blows crawled
  0.08 to 0.10 s after the freeze; carried 0.36 m of 1.83, 0.91 of 3.29). But most blows were thrown from 0.55 to
  0.86 m: after his first pounce the swordsman stood where it ended and the target walked back to its spot, right
  against him. **Mended 21:15** (`DuelLab.TakesDistanceAgain`; Combat Feel "After Going For Him He Takes His
  Distance Again"; lab row "After he goes for him:", readout "Their distance"; `FIGHT_TAKESDISTANCE=0`): with no
  order but to stand, after a pounce or a blow at a man who is down he steps back to the distance he stood at.
  Headless: the next blow from 1.20 m, against 1.10 m the old way.
- **His second look (21:22; log `2026-11-20_21-17-58`), seen in Unity:** the step back works (13 asked blows from
  1.38 to 1.64 m), though the target is against him for about a second before he steps back. The drawing shows as
  meant (his pictures). Planted forehands run 16 cm from the planned line, blows on the move 26 cm. **The blade
  sticks at the body in Unity: 13 of 27 landed blows crawled 0.07 to 0.15 s after the freeze, 5 of 27 carried under
  70% of the plan**, mostly the close-in blows of a pounce. He said blue and orange are "not super close or far".
  He said "yes please proceed with step 2" at 21:26.

**Combos, step 2: two blows as one swing (20 November, 22:08; headless only, waiting for his eye).** README entry of 22:08.

- **A sword passes the body it cut** (`ClipFighter.LetBladePass`; Combat Feel "A Blade Passes The Body It Cut"; lab
  row "A blade that cut him:"; `FIGHT_BLADEPASSES=0`): from the moment a blow "drove on through", the striker's solid
  sword does not touch that man's body (shield and sword still stop it) until it is clear of him. Labs and dance
  only (`BladePassesHere`). Headless: blades that crawled after the freeze 7 of 19 -> 0 of 17; carried under 70%
  3 -> 0.
- **The join** (`AttackScore.EnteredFrom`, `ClipFighter.FlowInto`/`StepFlow`, `DuelLab.ThrowTwo`; Combat Feel "A
  Second Blow Flows Out Of The First", "The Join Bends Over"; lab row "Two blows:", button "Throw two", readout "His
  last two blows"; `FIGHT_THROWTWO`, `FIGHT_FLOWS=0`): forehand into rising backhand at the chest. The first is left
  at the corner `BladeFlow.Join` names; the second is a made attack from a score that bends from the first's
  carry-through into the second's wind-up over 0.12 s. Headless: 0.53 to 0.58 s hit to hit (one 0.67; old 1.85), the
  point never under 3.1 m/s, 8 to 10 m of travel. **Two marks missed:** the posed point is thrown 15 to 19 cm off its
  course in a step at the join (mark 10; cause not found; not the kept-speed limit), and the second blow meets his
  right arm on its way to the chest in 5 of 7. **Not done:** the second blow is made planted even when he walks.
- **He steps back sooner after a pounce** (`DuelLab.StepsBackAfter` 0.35 s; lab lever "Steps back after"): the blow's
  way back to the guard is left. **Unverified:** headless does not show the wait his log showed.
- A `flow` line goes to his playtest log for every "Throw two".
- **His look at step 2 (22:40): "They feel like a single motion, you nailed that part. The only issue is that the
  body itself seems to jump to a different pose between the 2 hits."** Found (23:05, README): the maker's fit of the
  entered blow (turn 43, hips 17 cm down, lean 28) came in over a wind-up of one frame. Now over the bend (0.13 s):
  the kink at the join is 5 to 8 cm headless (was 15 to 19). **Then the cause itself (20 November, 23:17, README): a
  score's tracks are in its clip's start frame, and the join took them as if against the root** (the two roots are
  turned 68 and 79 degrees there). Mended: both blows are put into the joined score's own frame and the next laid
  on so that his hips carry on (Combat Feel "At The Join His Body Carries On"; lab row "At the join, his body:";
  `FIGHT_JOINTURNS=0`). The maker now turns the second blow 0 to 7 degrees with no drop or lean. **The blade nearly
  stops at the corner now (0.4 to 2 m/s)**; lever "Join Makes Up At Most" 0.25 leaves the forehand sooner (4 m/s,
  0.49 s hit to hit, a bigger kink). Which reads better is his to say (`FLOWMADE`, `FLOWJOIN` lines). In his Unity logs
  the second blow landed in 18 of 22, slow, mostly on his right arm: the first blow throws the target back
  stumbling, which headless it does not (`FIGHT_STUMBLEFROM=0.7`).
- **"The enemy getting stuck in my shield" (22:44):** a fallen target's spot never followed it (the rule sat where a
  unit off its feet never comes), so once up it walked back to its old spot through the swordsman. Mended
  (`FightLoop.DummySpotFollowsTheFall`; Combat Feel "A Fallen Target's Spot Follows It"). And his step back is
  counted from where he comes to rest. Headless only.

- **A body takes a weapon's blow** (20 November, 23:38; README): `ClipFighter.PressByBlow`; Combat Feel "A Body Takes
  A Blow In Its Knees And Trunk" and three levers; lab row "A blow, his body:" under "Stumbles and falls", readout
  "The last blow he took"; `FIGHT_BLOWPRESS=0`. Hips drop, trunk folds the way the blow goes, chest turns with it,
  by the blow's weight (60 N s on a Medium is a hard one). Headless: hips 3 to 8 cm, fold 4 to 11 degrees, turn 1 to
  14. **Labs and dance only: everywhere, the six went over both budgets** (the folded trunk takes the arms and what
  they hold with it). Not seen in Unity.

**His morning look (21 November, 07:38) and what was built for it (07:57; README; headless only):** the jump at the
join is gone ("looks good there"). In his log the second blow met nothing in 20 of 37 pairs (the first throws the
target back). Built: **he jumps in for the second blow** (`ClipFighter.FlowJumpsIn`: carried toward his man over
0.1 s when the blow would fall 0.2 m short, the blow made for where that puts him; lab row "The second blow:" and
levers); **an attack speed slider** (`AttackMaker.Pace`; lab lever "Attack speed"); **a stumbling target's spot
follows it** and the swordsman steps in to his distance; **a pounce only on a stumble of 0.6 m or more** (lever
"Pounce on a stumble of"). **His reminder for after the combo work: kick a man who is down, with the kick pack he
supplied; the stab at a downed man "can look pretty weird".** Next for combos, after his look: step 3.
Its film: https://claude.ai/artifact/XydAAxP3Xhn27PXaWvPYMh

**His second look that morning (21 November, 08:44) and what was built for it (09:49; README):** in his log he
jumped in 27 times and the second blow landed in 31 of 39 pairs, but **he fell 3 times, each after a jump in of 87 to
94 cm**: his physical body hangs by its hips from a spring of 3 weights and was left 52 cm behind, which the game
took for a man borne down. Built: **in the jump his body is carried with him** (`ClipFighter.FlowJumpCarriesBody`,
`UpperBodyArticulation.SendAll`: an eased jump, and every part given the speed his root is carried at; no rule
loosened; lab row "In the jump, his body:"); **two attack speeds** (`AttackMaker.Pace` for a blow on its own,
`AttackMaker.ComboPace` for a pair's two; `ClipFighter.ThrowsACombo`; lab levers "Speed: single", "Speed: combos");
**the panel needs no scrolling sideways** (`BattleSandbox.BeginRow`/`RowTakes`: rows wrap, the views are held to the
window's width; **built blind**). **A gap between headless and Unity found and closed: the headless engine read a
joint's "maximum force" as an impulse a physics step, not a force, so the hold on the hips (3 weights) never gave
and a body carried or shoved by its hips moved far less than in Unity.** `PhysXWorld.JointLimitsAreForces` now sets
it as Unity has it by default (`PHYSX_JOINTFORCES=0`: as headless was). With it the old jump leaves his chest 14 to
48 cm behind, step by step, as his log's 12 to 52; carried, 7 and 11 cm, no fall. **The six's baseline with it:
crossings 0.07%, tremble 4.8% (before: 0.00% and 6.6%).** Headless numbers of shoved bodies from before 21 November
were measured with the stiff hold. **The jump and the panel are not seen in Unity.**
Its film: https://claude.ai/artifact/F7ew9gozgHUsen4kmxRVDk

**He looked (21 November, 10:41): "Ok this all looks good to me. you can proceed".** His log of 10:38 (Unity): 10
jumps in of 21 to 90 cm, his chest within 4 to 8 cm of its pose in every one, no fall: the carried jump works in
Unity. **The speeds he ended on: a single blow 1.15, a combo's blows 1.44** (his own Combat Feel; the code's defaults
are still 1 and 1; ask before changing them).

**Combos, step 3 is built (21 November, 11:43; README; headless only, waiting for his eye):** "Throw combo" in the
duel lab throws 2 or 3 blows as one swing. A combo is a chain of step 2's pairs (`ClipFighter.FlowCombo`, the roll
in `StepFlow`; `FlowCombos` in `AttackMaker.cs`; `DuelLab.ThrowCombo`): a light forehand (0.6), a rising backhand
(plain), and the big one: forehand, leaping chop or spin (`spin_backhand_mid`), entered at least 0.3 s before it
lands (its wind-up is the loop), landing at 1.6 after one landed blow and 2.0 after two, the showstopper (freeze up
to 0.20 s; its knock doubled and the man floored where two landed before it). Headless at speed 1: three hits in
1.25 s (forehand; the plan's mark is 1.15, met at speed 1.25; 1.07 s at his 1.44), 1.48 (chop), 1.60 (spin); the big
one 9 to 12 times as heavy as the first; floored each time; with two blows (0.70 s) he stumbles and stays up. The
jump in leads his man for 0.13 s at most (`FlowLeadsAtMost`): led for a big blow's whole wind-up he landed chest to
chest with him. **New tool: `Tools/Headless/strip.py`
draws a run's fighters as stick figures from its log: look at what a blow does before sending it to him.** Not
right yet: at his own combo speed of 1.44 the spin misses; the 2nd and 3rd blows are planted even when he walks; the chop's leap (hips +0.76 m) and the forehand's
throw (10 m/s) may be too much; the spin is wild. Step 4 (against a man who defends) is built: see below.
Its film: https://claude.ai/artifact/3cZJau7b6BLWyTdwq7sJZ1

**He looked at step 3 (21 November, 12:45): "Ok that looks good. The spin does not look good, does not spin all the
way around and the slash does not seem impactful ... For the forehand combo end I think that slash needs to look a
lot more impactful, feels weak atm. Do this and proceed".** He plays the combos at speed 1.88. Built for it
(13:57; README; headless only):
- **The big one has its own speed** (`AttackMaker.BigPace`, 1; lab "Speed: big one"): at 1.88 its wind-up and cut
  lasted 0.25 s, with no wait before it. Headless the blade reaches him about as fast as before (11 to 16 m/s); what
  changed is that it lands every time (5 of 5 against 4 of 5), with a wait, a bigger pose, and through him.
- **The big one is swung big** (`FlowCombos.LastBig`; lab "The big one's swing:") and drives on through him at 0.95.
- **A combo's earlier blows leave him in reach** (`StrikeHits.Linking`; lab "Before the big one, a blow:"): in his
  logs the light first blow threw him back stumbling or floored him, and the rest met him slowly or not at all.
- **The spin is a new cut, `spin_low`, baked from the recording he gave** (Mixamo "Standing Melee Attack 360 Low";
  `Tools/Mocap/Combat/combat_sample.py <folder>` then `combat_bake.py`). It is entered at its beginning
  (`FlowBlow.WindsUp`), may be turned 120 degrees more (`MostTurn`), lands at its recorded moment
  (`AttackAim.AsRecorded`) and is aimed at the hips. He turns about 400 degrees before the hit. Not right: it lands
  on the chest, and he ends chest to chest with the dummy.
- **A new recording must go on `AttackScore.OtherBodies`**: the body's measures (`BodyMeasures.OfClips`) are taken
  from every strike in the library, and `spin_low` in them moved every blow in the game: the six went to 0.77% and
  12.6%, over both budgets, until it was taken out of them. Numbers measured in between were thrown away.
- **One throw is not a measurement.** The same combo's blade speed runs from 9 to 16 m/s from throw to throw. Compare
  ways with five throws each (`FIGHT_THROWTWO=3,9,15,21,27`, `SECS=33`), at his speeds.
- Its film: https://claude.ai/artifact/LzVt4cUC7KmwicvM9Vo1N9

**Combos, step 4: against a man who defends (21 November, 15:07; README; headless only; waiting for his eye).**
- **A blocked blow flows on** (`ClipFighter.FlowsOffABlock`; lab "A blocked blow:"): the blocked blow is played back
  from where it stopped (`BladeFlow.JoinFrom`, `EnteredFrom(..., beforePace: FlowBouncesBack)`), and that is the next
  blow's wind-up (`FlowBounceWindsUp` 0.3 s). The blow out of a bounce is not live for 0.15 s
  (`StrikeHits.NotLiveFor`), or it meets the board again at once.
- **Each blow goes for what is open** (`FlowAimsAtOpen`, `FlowBlow.Spots`, `Openings.LookAt`; lab "Each blow goes
  for:"). **The big one on a shield knocks the guard open for 0.8 s** (`FlowLastOpensGuardFor`,
  `StrikeHits.OpensGuardFor`; the arm thrown wide, not across his body). **A parry ends the combo**
  (`FlowEndsOnAParry`). **Out of reach ends it** (`FlowEndsOutOfReach`).
- **Every fighter in the game is an expert with his shield** (`ShieldSkill` 1): he blocks every blow. The duel lab's
  dummy has "Dummy: no guard / shield up / parries" and "Dummy's guard skill" (`DuelLab.OtherGuardSkill`;
  `FIGHT_DUMMYSKILL`). Measure against 1 and 0.5.
- **Measured at his speeds, five each:** single blows 0 of 5 on the body (expert and middling). Combos the old way
  0 of 5 (each ends at the first block). Combos now: 1 of 5 against the expert (his guard knocked open 4 times),
  4 of 5 against the middling shield. Against the parrying dummy 5 of 5 combos ended at the parry.
- **Not right:** the dummy's own blade passes through his own shield a little as he takes three blows (0.09% to
  0.29%, the longest 0.08 s; within the budget, but the old way was 0.00). The rising backhand aimed low often lands
  on the head. Chop, spin and 2-blow combos against a shield were not measured five times.
- **Found after the push (15:25): the spin combo passes his own blade through his own shield** (a dummy with no
  guard, five combos: 1.04% of the time, the longest 0.12 s: over both budgets; the chop 0.61%; the forehand 0.00).
  It was so at 13:57 too and was not measured then. **Not mended: next thing to do on the spin.** Chop and spin
  against the middling shield: 3 of 4 and 3 of 5 combos with a blow on his body.
- Its film: https://claude.ai/artifact/GtQDpng7EsNv7xxjnJusJn
- **The build log was cut short by three of my commits today and put back whole** (see the top of this file).

**He looked at step 4 (21 November, 15:35): "yes [the bounce reads as one flowing combo] but it does still stop the
combo many times, is this intentional? ... Middling ... yes [his speeds as defaults]. Ok so looks good. proceed but
tell me what is up next and what is the roadmap".** (16:15; README.)
- **The stops were a bug in 12 of his 46 combos:** whenever the shield stopped the second blow the big one was never
  thrown, and the next combo began "out of the bounce" with its first blow skipped. Mended: the roll comes before the
  join in the same step (`StepFlow`), and `flowBounced` is cleared as a pair opens and as a flow ends. **Unity only:
  headless never showed it, so the fix is unconfirmed until his next log.** 7 more ended at a parry and 3 at a glance
  off his sword: meant.
- **His speeds are the defaults, under new field names:** "A Blow On Its Own Goes At" 1.15, "A Combos Blows Go At"
  1.88. **A blow on its own goes at 1.15 only in the labs and the dance** (`AttackMaker.InALab`); a battle keeps
  `FightPace` 1 (Combat Feel "In A Battle A Blow On Its Own Goes At"), because at 1.15 the six's checks were worse
  (0.29% and 5.9% against 0.07% and 4.8%). **Ask him whether battles go at 1.15 too.**
- **The duel lab's dummy is middling** (`DuelLab.OtherGuardSkill` 0.5; headless too, `FIGHT_DUMMYSKILL=1` for an expert).
- **Step 5, the dance fights with combos: a first build** (`Dance.Combos`, Combat Feel "The Dance Fights With
  Combos", lab "Combos (dance):", `FIGHT_DANCECOMBOS=0`; `UnitController.ThrowCombo` and `GoOnIntoCombo`;
  `GesturePattern.ComboBlows`, `ComboLast`, `ComboTakes`; `DANCECOMBOS` lines). A combo starts when his light blow
  lands, or the other is opened as he strikes (guard open, stumbling, his own heavy wind-up). **Not measured yet as
  the plan asks (four dances of 70 s, with and without); no look list and no film for it yet.**
- **Step 5 measured and pushed (17:51; README; headless only; waiting for his eye).** A dancer goes on into a
  combo when his light blow lands, when his pounce's blow lands, or throws one when the other is opened as he
  strikes. Four dances of 70 s: 19 combos begun (12 out of a pounce, 5 at a man opened, 2 out of a light blow), 18
  whole, 17 with a blow on the body after the first; blows landed on a body 29 against 8 without combos; men off
  their feet 6 and 6. **A pounce's blow is not gone on from when he is running and within 1.25 m**
  (`Dance.ComboNotWithin`): 2 of 12 such combos ended with their thrower on the floor. **Not shown: the dance's
  budgets no worse** (crossings 0.57% on average against 0.47%, but dances differ more than the two ways do; the
  dance is over its tremble budget either way). The Prowler's spin crosses his own shield. Headless dances'
  combos grow without `FIGHT_COMBO`.
- Step 5's film: https://claude.ai/artifact/LEexE7y6CVHUMGC6mniyj5
- **He looked at step 5 (18:32): "Ok I think everything looks good here. Proceed to the next task".** His log of
  18:22 (a dance in Unity): 26 combos, 25 thrown whole, 13 with a bounce off a shield that went on: **the stop bug
  is gone in Unity.** He did not say whether battles go at 1.15 too: they keep 1.
- **Step 6, the show (19:09; README; built blind, headless only counts the cues; waiting for his eye and ear).**
  `ComboShow` (in `BladeStreak.cs`): streaks that grow (`UnitController.StepStreak`, a `BladeStreak` on the sword's
  slot, by `ClipFighter.ComboRank`), a swish that rises (`StrikeHits.SwingPitch`, `Sfx.Play(..., pitch)`), the deep
  hit's switch, the camera kick (`OrbitCamera.Kick`), the slow beat (`BattleSandbox.OnSlowed`), a spark and a ring
  on a stopped blow (`HitSpark`; the ring is the parry's clang played higher). Lab "The show:" (a tick each), "The
  show, all:", "The show so far"; six Combat Feel switches; `FIGHT_SHOW=0`. Counted over five combos: no guard 15
  streaks, 15 swishes, 5 deep hits, 5 kicks, 5 slow beats; the shield dummy 13, 13, 4, 4, 1 and 7 sparks; off: none.
  **`UnitController.MainWeapon` is never set**: the clip fighters' blades are slots, not `Weapon`s.
- Step 6's film: https://claude.ai/artifact/11GqpfgqDfXm7jkoN4BiZh
- **Found (19:09): the forehand combo against no guard at the default speeds has his own blade through his own
  shield 0.94% of the time** (the same with the show off). With the spin's 1.04% and the chop's 0.61% it is the
  combos' own, at speed 1.88. Not mended.
- **He looked at step 6 (20:01): "Ok all this looks great."** No word on any single cue.
- **New task, his words (20:01, 20:04): shock waves through the body on a hit, faked, with sliders, "a bit over the
  top, more so than you will see in other games".** Researched and planned (20:08), nothing built:
  `Docs/Plans/2026-11-21-shock-waves-through-the-body-research-and-plan.md` (live:
  https://claude.ai/artifact/FED9yrpFr7tz59LvbJLnd6 ). A pose-only wave beside the bend (`AddImpact`, `Bend`): each
  part moves after its distance along the body over a wave speed, then swings as a dying spring. First values:
  7 swings a second, half-life 0.15 s, 9 cm for a heavy blow, 5 m/s, each joint passes on 0.75. Step 1: one wave
  down one arm, a "Ring him: chest" button. **Five questions wait for him** (the wave through the freeze, the legs,
  the attacker's arm, keep the bend, how over the top). **Build only after he answers or says go.**
  Its film: https://claude.ai/artifact/5qRcjDgh6jbCCJivN8R1pK
- **He looked at the shock wave plan (20:47): "Ok this looks great. But before we do this" he wants clothes.** The
  five questions of that plan are not answered; my recommendations stand until he says otherwise.
- **New task, his words (20:47): the fighters wear "actual dynamic cloth", samurai clothes after his picture, a
  conservative first version; no cloth through the body; covered parts may be hidden.** Researched and planned
  (20:54), nothing built: `Docs/Plans/2026-11-21-cloth-that-moves-samurai-clothes-research-and-plan.md` (live:
  https://claude.ai/artifact/3Jev6uXAQk3Nsncq7GCtAh ).
  Its film: https://claude.ai/artifact/TtEraxUG6Bk2VKuR9n6uiu
  Recommended: our own cloth solver (measurable headless), six
  safeguards, the hakama as a pleated skirt first, sleeves tied back, clothes made by code. Step 1: a skirt on the
  standing dummy, "Shake him". **Six questions wait for him. Build only after he answers or says go.**
  **The order he set: clothes, then shock waves, then the combos plan's step 7.**
- **His night order (21:33, going to bed): "First shock waves, then cloth, then the last combos step."** His answers to
  the shock wave plan: seen in the freeze "but also it should be happening all the time. For every hit and impact"; legs,
  the attacker's arm, keep the bend: yes; "lets say 3 times" (read as the first values, about three times a real body).
  To the cloth plan: as recommended (our own solver, the skirt kind, sleeves tied back, one outfit in two colours), but
  the shock waves first.
- **The shock wave is built (22:50; README; headless only; waiting for his eye):** `Locomotion/ShockWave.cs`
  (`ClipFighter.Wave`; stepped and drawn in `UnitClips.StepClips`). **Drawn only**: it moves the meshes under the rig's
  parts and the weapons' meshes, never the pose, colliders or physics, so the checks are untouched by construction.
  Drawn as a skeleton from the hips out (least-squares turns). Started in `TakeHit`, `TakeImpact` and, for the
  attacker's shudder, `StrikeHits` (`HoldLast` holds it through the freeze if that switch is off). Labs and dance only
  (`AttackMaker.InALab`). Duel lab heading "The shock wave": "Ring him:", sliders, switches, "His last wave".
  Headless: `FIGHT_RING`, `FIGHT_WAVE=0`, `WAVE` and `SHOCKWAVES` lines. A chest ring reaches the hands at 0.18 s and is
  still at 0.8 s; the drawn joints are within 3 to 5 cm of the formula. The six at seed 5: 0.07% and 4.8%, as before.
  **Not seen in Unity**; the streak does not whip with the drawn sword.
  Its film: https://claude.ai/artifact/NxqQKD4EN8eZmRxQkkz9pZ
- **The clothes are built (README "clothes that move"; headless only, built blind):** `Locomotion/ClothNet.cs` (the
  solver: XPBD, 8 small steps, the six safeguards, the net), `Locomotion/Clothes.cs` (the hakama, 640 points, pinned at a
  sash; two tied-back sleeves, 72 points each; a top over the chest that bends with him but is not simulated; covered
  parts hidden: hips, thighs, spine, chest, upper arms), `UnitController.Clothes`, stepped in `UnitClips.StepClips`
  after the shock wave (the clothes hang on the body as drawn). Combat Feel "The Fighters Wear" (2: skirt and top, by
  default) and the cloth sliders; duel lab heading "Clothes" with "Shake him". Labs and dance only. Headless:
  `FIGHT_CLOTHES=0|1|2`, `FIGHT_SHAKE`, `FIGHT_CLOTHDUMP`, `Tools/Headless/cloth.py name [mover|other]` (draws the
  cloth's points), `CLOTHES`, `SHAKE`, `CLOTHNET` lines. **Costly headless** (about 4 ms a fighter a picture in the
  debug build): turn it off (`FIGHT_CLOTHES=0`) for runs that do not need it; a dance with it runs several times slower.
  The colliders the cloth is pushed out of are the physics shapes; the drawn meshes may be fatter (not known).
  Its film: https://claude.ai/artifact/GA5FrDmgSqaxDYsk5eLmMq
- **Combos, step 7 is built (README "combos, step 7"; headless only):** five cuts baked (`cut_`), "Combo's cuts:" three
  flat cuts / high then low, a fourth last blow "sweep", the shield drawn aside in the new combos. The two new combos are
  over the crossing budget (1.8% and 2.8%): a lab choice, not the default. **The combos plan is now fully built.**
  Its film: https://claude.ai/artifact/P6fPxWrGjvhqqv5RLAbwEH
- **The fight of six after the three builds** (seed 5): 0.07% and 4.8%, as the baseline; none of it reaches the six.
- **His morning look (22 November, 07:12) and what was built for it (07:20 and 08:20; README; headless only, built blind):**
  - **His own combo never fells him** (`ClipFighter.OwnComboNeverFellsHim`; Combat Feel "His Own Combo Never Bears Him
    Down"; lab row "His own combo:"): the jump in left his chest behind its pose, which read as borne down. Headless never
    showed these falls: unconfirmed until his next log.
  - **The clothes are one open robe** (his drawing; no sleeves yet): `Clothes.cs` rewritten, 864 points, held at the
    collar (chest) and pinned by the sash (hips), open in front, armholes, stiffness 0.25, under new Combat Feel names
    ("The Fighters Wear A Robe", "Robe ..."). Threads are kept out of limbs as threads; no thread past 1.6 times its
    length. **The sums run on other cores:** `ClothSolve.cs` (pointer core), `ClothNet.cs` (packed array), `ClothJob.cs`
    (a Burst `IJob`, one picture behind his body; `#if !HEADLESS_STUB`, **never compiled headless: first compile in his
    Unity**). The asmdef allows unsafe code and references Unity.Burst; `com.unity.burst` 2.0.0 is in the manifest; the
    headless Runtime.csproj defines `HEADLESS_STUB` and allows unsafe blocks. Lab row "Worked out:". Headless: 0.05% of
    point-steps inside, never reset, 0.9 ms a fighter on the main thread without the sums. `Tools/Headless/robe.py`
    draws it as shaded cloth.
  - **The jolt** (`ShockWave.Jolt`, on; Combat Feel "The Shock Wave Is A Jolt" and five levers; lab row "Its kind:";
    `FIGHT_JOLT=0`): one hard knock and a small kick back, still in 0.25 s, at 14 m/s, the grip holds, a heavy one kicks
    the camera (`ShockWave.Jolted` → `OrbitCamera.Kick`). Not built: a squash of the struck part.
  - Its film: https://claude.ai/artifact/4dFNQateNkfj5hViL6oZda
- **His look at the robe (22 November, 08:46): "the cloth is still not working"**: a shirt and trousers instead, and
  cloth that never goes into the body, "Spare no expense". **The cause found:** the cloth was kept out of the
  *physics* shapes, but the drawn meshes stand 2.4 to 4.8 cm outside them in the middle of the range, up to 16 cm
  (`Tools/Cloth/drawn_vs_physics.py`, `Tools/Cloth/fbxread.py`). **Any cloth measure against the physics shapes is
  meaningless for what he sees.** New plan, nothing built:
  `Docs/Plans/2026-11-22-clothes-that-never-go-into-the-body-research-and-plan.md` (live
  https://claude.ai/artifact/NcFgFLd75rj2ZDECPKUVp1 ): baked distance fields of the drawn pieces, a carried shell, a
  fenced wobble (max distance, backstop), a last word on this picture's drawn body, budget zero by two checks.
  **Four questions wait for him; build only after he answers or says go.** Its film: https://claude.ai/artifact/TSXxiad3ARayFmDX7qFWNS
- **He said go (09:21): "Soungs good yes to all. go". Steps 1 to 3 built (11:20; README; headless only, built blind):**
  - `Tools/Cloth/bake_body.py` bakes `Resources/Locomotion/BodyFields_Medium.json` (the drawn meshes as distance grids,
    with the covered parts' triangles); `BodyFields.cs` reads it and checks it at start-up in Unity (`[Clothes]` line).
    **The limb meshes' tops are hollow sockets**: each limb's field is joined with a ball at its joint, or cloth slips in
    unseen (a band of bare skin at the top of each thigh).
  - `Outfit.cs` cuts the shirt, sleeves, trousers' waist, legs and sash as tubes; each point's home on a part or a joint's
    ball; `OutfitSolve.cs` (Burst `OutfitJob`, a piece a worker) the last word: points out of the others' parts (best
    effort) and their own, the trousers smoothed, then thread middles and face centres. Placed in `LateUpdate`.
    `Clothes.Kind` (0 none, 1 outfit, 2 robe). Duel lab "Clothes": "Clothes:", "Shake him", "X-ray:", "The last word:",
    the fit levers, "Cloth inside the body".
  - Headless: rack of 1000 poses, points inside 3 of 5.6 million, middles 137 of 11 million, exact check 3 of 1.1
    million; "Shake him" 0, 128 of 29 million, exact 1 of 1.5 million; folds and the body pressed into itself counted
    apart. Not zero by the letter: a few in a million, under a millimetre.
  - **Not known:** the cost in Unity (headless optimised, one core, no Burst: 18 ms a fighter a picture); whether the
    Burst job and the start-up check compile. `outfit.py` (own depth test), `coverage.py`, `FIGHT_CLOTHRACK`: Headless
    README. **matplotlib's 3D paints skin over cloth: never judge bare skin from it.**
  - Its film: https://claude.ai/artifact/Mmzde7X73orSrndiPmKXPd
  - **His first look (11:31): no clothes drawn** (the start-up check: the bake matches his meshes, 0.1 mm). Likely the
    job's settings held pointers, which Unity will not schedule; mended blind (11:35): plain numbers, and a fallback to
    the main thread with one `[Clothes]` error if the job fails.
  - **His logs (11:30 to 11:43) confirmed it** (`OutfitJob.Settings.Grid uses unsafe Pointers`); since the mend the
    job runs, frame median 18.5 ms. **He said (11:42) it is too tight to see cloth behaviour; (11:45) log what the cloth
    does.** Built 12:05 (README): looser (2 cm, shirt 3 cm, ends 1.6; recut when the lab's fit changes), **the wobble**
    (step 4: Verlet, weight, a pull home, threads, a fence by each point's sway; "The wobble:" and three levers), and the
    log: `cloth` lines twice a second, `clothdump` during "Shake him" and on "Log the cloth now", drawn by
    `Tools/Headless/unitylog_outfit.sh`; perf `ms_Clothes`. **Read his next log's `cloth` lines and draw its dumps first.**
- **What he was told is next (before tonight):** step 7 (more cuts, now built); combos in the six only when he says;
  then the kick at a man who is down (his kick pack), the stab at a downed man that looks odd, the spin combo's own
  sword through his own shield; then the older open things (get-up shield, stumble leftovers, absorbing a hit, the
  blade tip's tremble).

**The night of 20 to 21 November: his order (22:40 and 22:44, going to bed).** "Continue with the combos next steps
until you are blocked by me, then continue with other tasks." "You will need me to look at things between the
steps." In this order: (1) the combo fixes (the body's jump: **done**, above; the second blow's aim: better, 9 to 30 cm, his right arm
still in the way in 4 of 6; the second blow over his walk: **tried and off** (20 November, 23:59, README: it can be
thrown over the walk now, `FIGHT_FLOWWALKS=1`, but lands 14 to 80 cm off; walking at 1.6 m/s the pair made the
usual way lands low, 46 to 60 cm off, with a kink of 30 cm at the join)); (2) the enemy stuck in his shield (**done, above**); (3) **bodies that absorb a hit, pushed up
by him: the blows' part done, above** (a first build exists from 20 November, 01:15, for bashes and thrown bodies: knees and hips give, the trunk
folds; not built: a sword's blow pressing the struck man, feet sliding before the first step, the head lagging);
(4) more cuts baked (quick forehand, flat forehand, low cut) and the blade report on them; (5) the step back
measured; (6) stumble leftovers; (7) get-up leftovers; (8) old bugs; (9) kicks at more heights, the low slash.
**Stopped at 00:20 on the 21st with (1), (2) and the blows' part of (3) done and pushed; (4) to (9) not reached.
The night's film: https://claude.ai/artifact/32AGujgomksYWrM7JgmTXW**
**Not tonight (needs him):** combos step 3 and later; where the shield goes in a get-up; anything in the six. He gets
one short list in the morning, the panel's look list for the whole night, and one film.
- Its film: https://claude.ai/artifact/T38Pqw9FVNzxGAafWvJvw7
- **A mistake of the first push (corrected 20:15):** I wrote that this "may be the stand-in" because contact
  modification does nothing headless. It has nothing to do with it: contact modification here is only for a lying
  body on the ground (`GroundContacts.cs`). Before giving a reason why a headless number might not hold in Unity,
  find the code that would make it so.
- **What cost time:** a recording's root is not a body frame (each slice's root faces where his head looked as it
  began: 108 degrees apart for one moment of one take): compare against the hips. A recorded wind-up does not
  slow before its drive, so a cut "starts" at the top of its swing (the point furthest from where it lands).

**Combos: the research document and plan (20 November, 17:05).**

- The document: `Docs/Plans/2026-11-20-combos-that-flow-and-end-in-a-bang-research-and-plan.md`. The idea: a combo is
  one swing with several hits. Each made attack ends at its blade's turn instead of in the guard, and the next
  starts from the blade as the last one left it (a join that keeps the blade's speed and direction); a blow on
  every footfall; power 0.6, then 1.0, then 1.6; the last blow grows with the blows landed before it; then the
  stab if he is floored. Every blow stays a real made attack that can be blocked, parried or struck into.
- **Measured for it, from the mocap takes with the scratch scripts in `Tools/Mocap/Combat/flow`** (the blade worked
  out from the hand's bones, so approximate; not part of the headless tests): in the recorded four-cut combo the hits are 0.57 s and 1.20 s apart,
  the blade carries on 5.5 m after the first hit and is never slower than 2.1 m/s before the second; two whole
  made attacks one after the other would be about 1.7 s from hit to hit. Six corners where the sword hand is when a
  cut starts or ends (high right, low left, out right, overhead, low front, left shoulder); forehand and rising
  backhand run between high right and low left (the hand's gap at the join 32 cm, 13 cm with the pack's quick
  forehand, which is not baked).
- **Found on the way:** `sns_backhand_lunge` is the same take as `spin_backhand_mid` (he turns 396 degrees in it),
  and `sns_chop_side` the same as `spin_chop` (a leap with a full spin). So the Medium really knows six cuts:
  forehand, rising backhand, thrust, spinning backhand, and two chops (a spinning leap and a 3.6 m lunge). The flat
  backhand exists only as a slice of the combo take. A blade's limit is 30 m/s (`PhysicalBladeSpeed`), paced for
  24.
- Its film (3b2b, made by Opus at medium effort): https://claude.ai/artifact/T6S2n1EfFLszkRJWMsRRW7
- **The old combo system** (`ComboPlayer`, `StrikeCores`, `StrikeCombos`, `UnitCombo`) is untouched and still runs
  in the fight of six. The new plan does not use it: it plays the cores of recorded clips as they are (from
  0.18 to 0.30 s before the hit to 0.18 to 0.35 s after it: `StrikeCores.ExitAfter`, `MostFollowThrough`) and fades
  the pose between them, where the plan carries the blade's own path on. (Its first version cut 0.06 s after the
  hit; his 4 November playtest found the slash's tail lost and it was changed. I first wrote the plan as if it still
  cut there: corrected the same evening in the document, the picture and the film's note.)

## Roadmap

**Stumbles (step 9c) have a first build (19 November, above). Next on them, after his look:** tune by his answers;
arms thrown out for balance; a look at feet crossing in a stumble to the side; a sword blow's throw worked out as
a bash's is (it is still the old rule's number, uncapped).

**The night of 19 to 20 November: his order (said at 21:29, going to bed).** "Continue working through the
night... if you get to a point that you need to push and get feedback you can just move on to another task." In
this order: (1) finish the get-up fixes (**done and pushed**: 22:10, 22:55, 00:30);
(2) back to stumbles (**arms out for balance pushed 00:30**; left: feet crossing in a stumble to the side, a
sword blow's throw worked out as a bash's is, and in his log of 20:10 every hard bash's stumble had "0 stumble
steps" before he fell: look at that); (3) bodies that absorb a hit (**a first build pushed 01:15**); (4) kicks (**a first build pushed 01:40**, lab only); (5) pouncing on an opening (**a first build pushed 02:00**, duel lab only); (6) downed fighters (**a first piece pushed 02:20**, off unless asked for).
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
2. Bodies that absorb: a first build on 20 November, 01:15 (knees and hips give, the trunk folds; bashes and thrown bodies). Not built: a sword blow pressing the struck man, feet sliding, the head lagging. He asked on 20 November, 22:44, for this to be pushed up.
3. Off balance: built as stumbles (19 November). Left of it: a leap landing on someone, kicks built on the door.

**Combos (the plan of 20 November; he said go at 18:42).** Step 1 is built (above). **Next, after his look at the
drawing: step 2, two blows as one swing** (forehand into rising backhand, on the move, on a dummy with no guard;
lab toggle "Throw two: flowing / one after the other (old)"; pass marks in the plan's table: 0.60 s or less hit to
hit, the point never under 2 m/s between the hits, 60 cm or more of travel after the first hit, 10 cm or less at
the join, the second blow landing where it was aimed). What it starts from: `BladeFlow.Join` (where to leave and
enter), `BladePath` (the measure; it needs "the slowest between two hits" added), and two new things in the maker:
an attack that ends at its blade's turn instead of in the guard, and one that starts from the blade as the last
one left it. **Before it: read his log's `bladepath` lines for blades that stick at a body after the freeze ("then under 5
m/s for ... more"), and mend it: the solid sword is held by the solid body it cut (above).** Defaults taken: a
blow a step (about 1.1 s for three hits, tuned toward his one second), three blows by default, the spin as a choice
of last blow, the last blow's freeze tried up to 0.20 s, slow motion on a flooring last blow as a switch that
starts off.

**The push of 20 November, 11:52: he has looked (16:11): "all this looks fine for now"** (the stab, the pounce at
what is open, the dance by itself).
His log of 13:59 (Unity, a dance of 250 s): a man went down 12 times and the other went for him every time by
himself; the stab met body or head 8 times, nothing 3 times, once it was too late; several landed on a man already
rising. 21 pounces: 9 blows on his shield, 2 on his head, 8 over before he was near. The README's entry of 17:05 has
the table.
**Left over, in this order:** (1) the get-up leftovers he asked for and I did not reach: the sword moving while its
hand is in the air and as he stands; a small kick of a shin at the start of one get-up in three from his front or
right; the shield's rim 3 to 5 cm in the ground for 0.1 s about 1.3 s in (**tried on the 20th and taken out again:**
it is in the pose itself, "-3 cm as posed"; raising the hand at the end of `HoldShieldUp` by what the rim was under
changed nothing at that moment, whether gated on the place having come in or not, so the dip is put in after
`HoldShieldUp`: look at what `Compose` does after it, the mix toward the stand first. Raised in the gather as well
it changed the start and so the whole get-up, which looked like a fix and was not). (2) The hit reaction that puts a struck
man's sword arm through his own board: it is why a stumbling man's open guard cannot go into the six. (3) The low
slash; a second blow when a pounce is stopped; kicks at more than one height (needs the kick pack's recordings,
not in the repository). (4) Only when he says: any of it in the fight of six.

**Fight Rhythm, still to come**: walking step 6; kicks, the pounce and Downed play beyond their first builds (above); combos (their own plan now, above); the fight of six.

**Waiting on a decision from him**: what being fooled by a fake costs (today: nothing); whether real attacks
should be more frequent. (Decided on 20 November: a fighter does not hold off while his enemy is down.)

## Starting a new chat

`Docs/NextChat-prompt.md` is the prompt Renier pastes into a new chat (written 19 November, evening, for the
trunk's turn and step 5). When a chat ends with work left, rewrite it for the next one: where to start, what to do in order, where the
code is, how to measure it, and what cost time.

## Keeping this file true

At the end of a step: move it from "Roadmap" to "Where things stand", rewrite "In hand", and change the date
at the top. Record a decision when it is made, with its reason, here or in the plan document. Anything worth
knowing that lives only in a chat is lost when the chat ends.
