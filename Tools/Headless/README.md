# Headless test scripts

Small shell scripts that run the game's own code without Unity (see `CLAUDE.md` and the README's "Compile
check and tests without Unity"). They were written during the work and are kept as they were used.

- Output goes to `$S` (default `/tmp/pb`): `<name>.txt` is the report, `<name>.done` appears when the run
  has ended, `<name>/` holds the playtest log. Set `S=...` to put it elsewhere.
- Most scripts start a run in the background and return at once. Wait for the `.done` file.
- Extra settings go after the name as `NAME=value` pairs (the `FIGHT_…` switches read in
  `Tools/CompileCheck/Runner/FightReport.cs`).
- Build first with `b.sh`. Never build while a run is going.

## The ones that matter

| Script | What it does |
|---|---|
| `b.sh` | Builds the four projects. Wants "0 Error(s)" four times. Refuses while a fight runs. |
| `reg.sh name [ENV…]` | The fight of six. `SECS=40` and `SEED=5` set its length and seed. About ten minutes for two side by side. |
| `aim.sh name FIGHT_AIMKIT=shield FIGHT_AIMWHO=shield` | The aim test: 19 blows by one sword-and-shield fighter at another. Nearly repeatable: the best test for a firm on/off number. |
| `st.sh name [ENV…]` | Two sword-and-shield fighters standing 1.5 m apart. Nobody strikes unless a bash or a dance is asked for. |
| `dance.sh tag FIGHT_DANCE="Plain,Plain" [ENV…]` | Two fighters dancing (`SECS=25`). Output `dance_<tag>.txt`. |
| `imp1.sh tag kind apart [ENV…]` | One shield bash at 4 s. Output `imp_<tag>.txt` (the `IMPACT` lines). |
| `four.sh tag [ENV…]` | The four bashes at a standing fighter (small, medium, hard countered, hard not countered), two at a time; prints the `BASHED`, `STUMBLE` and `IMPACT` lines. About eight minutes. |
| `stum.sh [STUMBLE_FOOT=5.5 …]` | The stumble model by itself, no fighter: thrown at twelve speeds from five sides. Instant. |
| `run.sh name FIGHT_SECONDS=8.5 FIGHT_TUBE="3,200,8,1.3,0.15,<angle>,1,1"` | The tube test: a tube knocks fighters down (angle 0 back, 180 front, 90 and 270 the sides). "B Medium" is the shield bearer. |
| `gup.sh tag "270 180" [ENV…]` | Tube falls from the given sides, then each get-up's speeds, floor and tremble. Waits and prints. |
| `gu.sh file.txt` | The get-up lines of a report, shortened. |
| `shk.sh names…`, `gw.sh names…`, `six.sh names…` | Tables out of finished reports: tremble, own crossings, the shield giving way. |
| `film.sh name from to [ENV…]` | A duel filmed between two times; draw it with `Tools/Mocap/Rhythm/film.py`. |
| `checks.sh` | The regular check runs (falls, twelve bodies, sixteen in a fight, the six). Long. |
| `flow.sh [all\|raw\|raw speeds]` | The recorded cuts' blades (the combos plan's step 1; no fight, a few seconds): the map of corners, and every pair's join. Ends `FLOW OK` when the joins where a performer went straight on come out best. The whole report: `$S/flow.txt`. |

The rest are one-off comparisons from particular days, kept for reference.

## Off his feet (19 November)

- `downs.sh tag ENV...`: a shield bearer bashed down six times in a run (hard not countered, and extra hard), two
  runs side by side; prints each run's crossings while falling, lying and getting up. About ten minutes. One bash
  alone shows almost nothing: headless falls are cleaner than Renier's, and it takes several to see a crossing.
  `FIGHT_BASHPUT="1.9,1.6,2.1"` is what makes it possible: 0.6 s before each asked bash the other is put that far
  before the basher, as the lab's "Stumble him" does (never while he is down).
- `room.sh tag ENV...`: the room test. Two shield bearers felled side by side 0.6 m apart, three times, new and
  old (`FIGHT_ROOM=0`) side by side; prints `ROOM` lines (how near the two bodies were while one got up).
  `FIGHT_FELLBOTH="4,12,20"` fells them.
- `FIGHT_SHIELDDOWN=0`: a falling and lying fighter's shield passes through his own body, the old way.
  `FIGHT_ROOM=0`: nothing keeps two apart while one is off his feet. `FIGHT_CAUGHT=0`: a held thing caught on the
  wrong side of his own body stays there.
- `FIGHT_STARTNEAREST=1`: a get-up's start is the one nearest to how he lies (off by default). Every fight's report
  has a line "the first 0.75 s of a get-up" (crossing, fastest part, how far the hands went); `downs.sh` prints it.
- `FIGHT_GETUPKNEES=0`: a rising fighter's knees go where the pose puts them, under the floor or not (old).
  `FIGHT_GETUPSTILL=0`: his pinned sword hand may turn a little and is not held still (old). `FIGHT_HANDPIN=0`: the
  pose alone holds it still, the physical hand is not pinned to it. `FIGHT_HANDTURN="20,300"`: how quick the
  turning hold is (Hz) and the most it holds with (N·m). `FIGHT_SWORDEASES=0.2`: over how long the hand's turn is
  eased in the air. `FIGHT_KEEPSTART=0`: a get-up's start is chosen afresh as it takes him up, not kept from the
  gather (old). Report lines: "a rising fighter's knees stay above the ground" and "a get-up keeps the start he
  gathered for". `gup.sh tag "180 0"` fells three targets from the sides named and prints each get-up.
- `FIGHT_GETUPEND=0`: only his arms are eased out of a get-up and what he holds is not settled afresh (old).
  `FIGHT_ROSEBODY=0`: the easing starts from the get-up's last pose, not from the seen body. Report line "the end of
  a get-up, as seen": the biggest jump in a part's speed in one step, from 0.15 s before he stands to 0.40 s after.
- `FIGHT_GETUPSHIELD=0`: a rising fighter's shield hand goes under his shoulder as the keys have it (old; the
  plan's step 5 off). `FIGHT_ARMSOUT=0`: a stumbling fighter's arms hold his guard (old). `STUMBLE_ARMS=1.5`: how
  much they go out. Each stumble's line says how far his hands went out and how far apart they were.
- `FIGHT_PRESS=0`: a body does not take an impact in its knees and hips (old). `FIGHT_PRESSED="0.2,0.10,4"`: the
  contact time (s), how far the knees give (m) and how far the trunk folds (degrees). The meter's IMPACT line has
  "hips down" for both men; `four.sh tag` runs the four bashes.
- `FIGHT_KICK=1`: at each `FIGHT_BASHAT` second the mover kicks the other instead of bashing him
  (`FIGHT_KICKED="2.6,0.25"`: the kick's power in m/s and how far into him in metres; `FIGHT_AIMKIT=sword|open`
  after `st.sh`'s name for a target without a shield). Lines `KICK` and `KICKED`.
- `FIGHT_KICKKIND=soft|medium|hard|far`: which kick `FIGHT_KICK=1` throws (far: 8 m/s, the lab's test of a man
  knocked far). `FIGHT_UNGUARDED=0`: a man not in a guard is only ever shoved (old). `FIGHT_GOESAT=3.6`: how fast
  the duel's mover runs at a man he pounces on or who is down; `FIGHT_GOTRACE=1` prints `GO` lines, each step of
  the blow thrown on that run. `FIGHT_ARMSFORGOT=0`: the jump as he stands after a stumble and a fall (the fault);
  `FIGHT_ROSETRACE=1` prints `ROSE` lines, each step of the easing out of a get-up. **When running a script with
  a time, put `SECS=12` before the script's name, not after it** (after, it is only passed on and the run takes
  its full 80 s). **Two cores: no more than four runs at once.**
- `FIGHT_DOWNEDBLOW="beside,sns_thrust,0.2,1.8,0.8"`: the blow at a man who is down: beside or straight; the
  recording (best: the best fit); how much lower his hips; how far his chest leans; how far from the chest he
  stands (then, optional: a lunge's time, the distance it is thrown from, how far toward the man's head).
  `FIGHT_GOOPEN=0`: a blow on the run always goes for the chest (old). `FIGHT_STUMBLEOPEN=0`: a stumbling man's
  guard opens by the clock (old; it only ever counts in the duel lab and the dance).
  `FIGHT_DANCEOWN="1,1,1,0.35"`: in a dance, by themselves: pounce, go for a man who is down, kick, and how many of
  a shield's bashes are a kick. A dance's report has `DANCEOWN` lines. **A dance of 70 s takes about 17 minutes
  here; a batch of six 10 s duels about 13.**
- `FIGHT_FLOWJUMPS=0`: he does not jump in for the second blow (old). `FIGHT_FLOWJUMP="0.2,1.4,0.8"`: from how far
  short (m), at most (m), and the share of his man's going he is led by. Use `FIGHT_STUMBLEFROM=0.5` with it, or the
  headless target does not go back. `FLOWMADE` says "[he jumped in N cm; his man was going V m/s]".
- `FIGHT_COMBO="3,chop"`: `FIGHT_THROWTWO` throws a combo of 2 or 3 blows, a three's last a `forehand`, `chop` or
  `spin`; light, plain, heavy unless `FIGHT_COMBOGROWS=0`. Without `FIGHT_COMBO` it throws two plain blows, as step 2
  had them. `FIGHT_COMBOLAST="1.6,0.25,0.2,0.3,0.2"`: the last blow's power, its growth for a blow landed, its longest
  freeze (s), its least wind-up (s), and the most to make up at its join (s; 0: the smoothest).
  `FIGHT_COMBOFLOORS=0`: it floors him only if it throws him. `FIGHT_COMBOSPIN=name`: the recording the spin is. A
  `COMBO` line as each combo's account is ready (1.5 s after its last hit), a `FLOW` line for each pair of it.
  `FIGHT_DOWNED=1` (and no `FIGHT_POUNCE=0`) to see him go for the man it floored.
- `FIGHT_BIGPACE=1`: the big one's own speed (the first blows go at `FIGHT_COMBOPACE`). `FIGHT_BIGSWING=0`: swung as
  recorded. `FIGHT_BIGSTYLE="1.25,1.15,1.15,1.1,1.1,1.15,1.2,1.4,1.15,0.9,0.02,0.02"`: its style, to try others.
  `FIGHT_COMBOKEEPS=0`: a combo's earlier blows throw him as any blow does. **Renier plays at
  `FIGHT_COMBOPACE=1.88 FIGHT_ATTACKPACE=1.15`: measure combos there as well as at 1.**
- **Combos against a man who defends (step 4).** `st.sh` is the shield dummy; add `FIGHT_AIMKIT=sword` for one who
  parries, `FIGHT_AIMKIT=open` for no guard. `FIGHT_DUMMYSKILL=0.5`: how good he is with shield and parry (1: an
  expert, as every fighter is today, who blocks every blow of a combo and every single blow: measure at 0.5 too).
  `FIGHT_COMBOBOUNCE=0`: a blocked blow backs out and the combo is over (old). `FIGHT_COMBOAIMS=0`: every blow at the
  spot asked for. `FIGHT_COMBOOPENS=0`: the big one on a shield only knocks it back. `FIGHT_COMBOPARRY=0`: a parry does
  not end a combo. `FIGHT_COMBOREACH=0`: a combo does not end where his man is out of reach.
  `FIGHT_COMBOBOUNCED="-0.5,0.3,0.15"`: the pace the blocked blow is played back at, the least the blow out of the
  bounce winds up (s), how long its blade is not live (s). `COMBOS so far` counts combos with a blow on his body,
  blows begun out of a bounce, combos a parry ended and guards knocked open. Single blows to compare with:
  `FIGHT_DUELBLOW="3:Chest,9:Head,15:Hips,21:Chest,27:Head"` and read the `AIM` lines and "its hits".
- **Defaults since 21 November, 16:15:** a combo's blows go at 1.88 and a blow on its own at 1.15 in the aim
  lineup (the aim test, the duel, the dance: `AttackMaker.InALab`), and at 1 in every other lineup, the six among
  them; `FIGHT_ATTACKPACE` sets both. The duel's dummy is middling (`FIGHT_DUMMYSKILL=1` for an expert).
- **The dance fights with combos (step 5):** `FIGHT_DANCECOMBOS=0` for the old way. A `DANCECOMBOS` line for each
  dancer: combos begun and why, how they came out, his habit, and his last combo blow by blow. A dance's combos grow
  (light first, the big one last) without `FIGHT_COMBO`.
- **The show (step 6):** `FIGHT_SHOW=0` for none of its cues, `"1,1,1,1,1,1"` for streaks, the rising swish, the
  deep hit, the camera kick, the slow beat, sparks. `COMBOS so far` has "the show: streaks begun ... ; swishes
  pitched ...": headless can only count that a cue was given.
- A new recording: put the FBX in a folder of its own, `python3 Tools/Mocap/Combat/combat_sample.py <folder>`, add a
  line to `STRIKES` in `combat_bake.py` (sample, cut from, cut to, landing, kind, about), run it, and read the new
  cut's line in `flow.sh all`. Check with a diff of the clips that nothing else moved.
- `strip.py name t0 t1 [frames]`: the two fighters of a run as stick figures between two times of its log (the
  events' first column), from the side and from above: `$S/name.strip.png`. Look at it with the Read tool. The
  sword is not in it (headless does not record the blade). `wait.sh name...`: waits for runs to end, at most 110 s.
- `FIGHT_ATTACKPACE=1.3`: a blow on its own thrown that much faster. `FIGHT_COMBOPACE=1.3`: both blows of a pair
  (`FIGHT_THROWTWO`). `FLOWMADE` says when each blow lands as made and both paces.
- `FIGHT_FLOWJUMPCARRIES=0`: in the jump in his body is dragged after his root (old). `FIGHT_FLOWSHOVE=0.54`: for
  the tests, at the join his man is put that much further off, which makes a jump in of about 0.9 m.
  `FIGHT_FLOWJUMPTRACE=1`: `JUMPTRACE` lines, step by step (root, chest from its pose, hips from the carrier). A
  `FLOWJUMP` line after each pair: how far he jumped, how far his chest got from its pose, falls.
- **A joint's "maximum force" is a force, as in Unity (since 21 November).** Until then the engine read it as an
  impulse a physics step (PhysX's default), so the hold on the hips, 3 weights, never gave: a root carried off at
  9 m/s kept the body within 10 cm, where Unity left it 52 cm behind. Now 48 cm here. `PHYSX_JOINTFORCES=0`: as it
  was, to compare with numbers from before. The six with it: crossings 0.07%, tremble 4.8% (before: 0.00%, 6.6%).
  Everything measured before that shoves a body by its hips was measured with the stiff hold.
- The stand-in's `ArticulationBody.linearVelocity` can be read but setting it does not reach the engine. `AddForce`
  with `VelocityChange` does.
- Never start a run under a name that is still running: both write the same file. Wait with
  `pgrep -f 'Tests.dll fight'` in a foreground command of its own (in a backgrounded command it finds itself).
- `FIGHT_SPOTSTUMBLES=0`: a stumbling target keeps its old spot (old). `FIGHT_POUNCEFROM=0`: he pounces on any stumble (old).
- `FIGHT_BLOWPRESS=0`: a weapon's blow does not press, fold or turn the body it lands on (old).
  `FIGHT_BLOWPRESSED="0.06,9,14"`: hips (m), fold and turn (degrees) for a hard blow. Read the `took` lines in the
  run's own playtest log (`$S/name/PlaytestLogs/*/events.log`). It counts in the aim lineup, not in a fight.
- `FIGHT_JOINTURNS=0`: the join as first built (his body jumps at it). Every pair prints `FLOWMADE` (what the maker
  did to each blow: turn, travel, drop, lean, hand) and `FLOWJOIN` (the join's frames and angles).
  `FIGHT_TWOSECOND=Head`: the second blow's spot. `FIGHT_STUMBLEFROM=0.7`: the target stumbles from a sword's blow
  as it does in Unity. `FIGHT_SPOTFOLLOWS=0`: a fallen target keeps its old spot (old).
- **`b.sh` also refuses when the command line that calls it names the test dll anywhere, even after it: run it in
  a command of its own, every time.**
- `FIGHT_THROWTWO="3,9,15"`: the duel's mover throws two blows at the chest at those seconds (forehand, rising
  backhand); a `FLOW` line as each ends: where it was joined, what each met, hit to hit, the point's travel and
  slowest between the hits, the kink at the join, how far from the spot the second landed. `FIGHT_FLOWS=0`: two
  whole attacks (old). `FIGHT_FLOWBEND=0.15`, `FIGHT_FLOWBENDOVER=0.12`, `FIGHT_FLOWKEEPS=12`: the join's levers.
  Use `st.sh name FIGHT_AIMKIT=open FIGHT_POUNCE=0 FIGHT_THROWTWO=...`, and `FIGHT_DUEL="CircleLeft:90"` for a
  walking man. **A run of 22 s with three throws takes about four minutes; read it when its `.done` is there.**
- `FIGHT_BLADEPASSES=0`: a sword stays solid against the body its blow drove on through (old). In the aim lineup
  it passes; in a fight only with `FIGHT_PASSESINFIGHTS=1`. Measure with `aim.sh name FIGHT_BLADEPATH=1` and count
  the `BLADEPATH` lines with "then under 5 m/s for".
- `FIGHT_STEPSBACKAFTER=0`: after a pounce he waits for his whole blow before he steps back (as on 20 November, 21:15).
- `FIGHT_TAKESDISTANCE=0`: after going for a man the duel's mover stands where it ended (old); by default he
  steps back to the distance he stood at. Every `DUELBLOW` is followed by an `APART` line (how far apart as the
  blow is asked for, the distance he keeps, what he did after his last going), and with `FIGHT_BASHAT` there is an
  `APART` line 6 s after each asked bash or kick. A sword blow does not make the headless target stumble: use
  `FIGHT_KICK=1 FIGHT_KICKKIND=soft FIGHT_BASHPUT=1.2` to get a pounce.
- `FIGHT_POUNCE=0`: the duel's mover does not pounce on an opening (old). `FIGHT_POUNCEREADS=0.15`: how soon he
  reads one. Every bash or kick asked for with `FIGHT_BASHAT` prints a `POUNCE` line 2.6 s after it.
- `FIGHT_DOWNED=1`: the duel's mover goes for a man who is down. **In the game this is on by default since 20
  November** (Combat Feel "A Fighter Goes For A Man Who Is Down"); headless it stays off unless asked, so that falls
  and get-ups are measured on a man left alone. Every bash or
  kick asked for with `FIGHT_BASHAT` prints a `DOWNED` line 4.4 s after it.
- `FIGHT_TAKEOVER=0`: a get-up takes him up as if he lay still and blends from how he lay, the old way (the plan's
  step 4 off). `FIGHT_TAKEOVERMOST=0.6`: the most its take-over takes (seconds). Every fight's report has two lines
  for it: "a get-up takes him up as he moves" (on or off, how long what was left took, how far the furthest hand or
  foot was) and "the take-over of a get-up, as seen (its first 0.20 s)": the biggest jump in a part's speed from
  one step to the next (a part stopped dead or thrown shows as a jump) and how fast he was moving as he was taken
  up. Each get-up's own line ends with the same.
- **The stub places a kinematic body at once (19 November).** A kinematic body set by its `position` or `rotation`
  and then moved with `MovePosition` or `MoveRotation` in the same step is now placed first and swept on from
  there, as Unity does it. Before, it was swept the whole way in one physics step, and what was locked to it was
  whipped along. That, and nothing in the game, was the "rougher take-over" measured with the trunk's turn on (a
  foot at 27 m/s): the hips' carrier was a long way round from the hips as the get-up locked them to it. Headless
  numbers from before this are not comparable with numbers after it.
- The panel check ends with `LOOK HERE (lab): n of m names drawn and framed`: the names of the panel's "look here"
  list (`BattleSandbox.cs`) that are really drawn in that lab, with every section open.

## Blade paths (the combos plan, step 1; 20 November)

- `flow.sh` (above) reads the recordings and runs no fight. `BladeFlow.cs` has what each number means. Its check
  fails if the search or the frames it compares in are wrong: the performers' own joins are slices of one take cut
  at a shared frame, so they must come out with next to nothing to make up.
- `FIGHT_BLADEPATH=1` (any lineup with made attacks: the aim test, a duel, the dance): every made attack's blade
  path is planned as it is thrown and followed as it goes, and a `BLADEPATH` line is printed as it ends: the cut,
  its corners, the plan (when it lands, how near the spot, how far it is carried on to its turn), as it went (how
  fast, how near, what it met in the blow's own words, the freeze and the crawl after it, how far it was carried
  on), and how far the sword's point was from its planned line. In the game this is Combat Feel "The Blade's Path Is Drawn", off.
- `FIGHT_BLADEPATHSTEPS=1` with it: a `BLADESTEP` line for every step of every attack, the plan beside the sword
  as it went. A picture of one is quickly made from these (the planned point's path against the real one, from
  behind him and from his side).
- The on-the-move battery with it: `mv.sh tag FIGHT_BLADEPATH=1`. The circling run takes about seven minutes.
- In a `BLADEPATH` line, "frozen 0.05 s" is the hit stop (meant); "then under 5 m/s for 0.10 s more" is the blade
  crawling after it (`BladePath.CrawlsUnder`, `CrawlWithin`), which is the blade sticking at the body; "carried on"
  is measured over the same stretch of the attack as the plan's "carried on ... to its turn", so the two compare.
  Headless on 20 November: 7 of 19 landed blows crawled 0.07 to 0.15 s. The cause: the pose sweeps on, the solid
  sword is held by the solid body it struck. `FIGHT_BLADEPATHSTEPS=1` shows it: each `BLADESTEP` line ends with the
  posed point and "against:", the solid parts of other bodies the seen sword overlaps that step. (Contact
  modification has nothing to do with it: here it only softens a lying body's contact with the ground.)

## The budget

Every fight's report ends with `CROSSINGS OK|OVER BUDGET` and `TREMBLE OK|OVER BUDGET` (see `SelfCrossings.Budget`
and `HeldShake.Budget`). `checks.sh` writes them all to `$S/checks.verdict`, with `CHECKS: within budget` or
`CHECKS: OVER BUDGET` as its last line. `FIGHT_SHAKELOG=1` also writes `TIPLOG` lines (the posed blade tip jumping
while he stands or moves, and what he was at).

## Switches used most

`FIGHT_GETUPHELD=0` (get-ups do not know what the hands hold), `FIGHT_GETUPCLAMP=0`, `FIGHT_GETUPRECORDED=0`,
`FIGHT_BASHTOUCH=0`, `FIGHT_BASHHOLDS=0`, `FIGHT_PUSHES="2.4,0.3,2,6"`, `FIGHT_BASHKIND=extra`, `FIGHT_GETUPPIN=0`,
`FIGHT_GETUPTURNS` (19 November, morning: see the README's entry), `FIGHT_STUMBLES=0` (off balance is the guard's walk played fast, the old way), `FIGHT_LEDGER=0` (a bash's speeds
written in, the old way), `FIGHT_BOUNCE`, `FIGHT_FEETTAKE`, `STUMBLE_FOOT`, `STUMBLE_REACH`, `STUMBLE_TAKES`,
`STUMBLE_BRAKE`, `STUMBLE_REACTS`, `STUMBLE_DOWN`, `STUMBLE_KNEES`, `STUMBLE_TURNED`, `STUMBLE_TIPS` (the stumble's levers),
`FIGHT_ABSORB=0`, `FIGHT_ABSORBED=0.6`, `FIGHT_OWNSOLID="1,1"` (shield, sword solid against his own body;
`"0,0"` is the old way), `FIGHT_SETTLE=0` or `"26,60"`, `FIGHT_HELDSOFT=0`, `FIGHT_GIVEWAY=0` or `"0.06,0.3"`,
`FIGHT_SHAKELOG=1` and `FIGHT_SHAKETRACE=1` (traces), `UPPER_GHOST=1` (no contacts on the upper body),
`UPPER_DAMP`, `UPPER_FOLLOW`, `UPPER_GIVEWAY`.

## Film strips

`python3 Tools/Mocap/Rhythm/film.py film.json out.png --moments 10 --from t0 --to t1 --title T`, from a run
with `FIGHT_FILM="$S/name.json,3,8.5,medium"`.

## 21 November, night: shock waves, clothes, more cuts

- `FIGHT_RING="chest@2,head@4"` (chest, head, arm, shield, hips, shin): ring the duel's other with no blow; a `WAVE` line
  when it settles. `FIGHT_WAVE=0`: no shock wave. Every report: a `SHOCKWAVES` line.
- `FIGHT_CLOTHES=0|1|2`: what the labs' and the dance's fighters wear (2 by default: skirt and top). **The cloth is
  costly headless: use `FIGHT_CLOTHES=0` for runs that do not need it.** `FIGHT_SHAKE=4`: the lab's "Shake him" from that
  second (`SHAKE` lines). `FIGHT_CLOTHDUMP="5.5,17"`: every point (`CLOTHPTS`, `BODYPTS`, `CLOTHNET`); then
  `python3 Tools/Headless/cloth.py name [mover|other]` draws them into `$S/name.cloth.png`. Every report: a `CLOTHES` line.
  Since 22 November the clothes are one robe (`FIGHT_CLOTHES=0|1`): `python3 Tools/Headless/robe.py name [mover|other]`
  draws it as shaded cloth (its lining pale) from his front, three-quarters and side into `$S/name.robe.png`. The
  `CLOTHES`/`SHAKE` lines split the cost into the sums (on other cores in Unity) and the rest (the main thread).
  `FIGHT_JOLT=0`: the shock wave as the old jelly instead of the jolt.
- `FIGHT_COMBOKIND=flat|low`: three flat cuts, or high then low; `FIGHT_COMBO=3,sweep`: the sweep as the last blow.
  `FIGHT_SHIELDASIDE=0`: the shield held in cover through those combos.

## 22 November, late morning: the shirt and trousers

- `FIGHT_CLOTHES=0|1|2`: none, the shirt and trousers (default), the robe (old). `FIGHT_SHAKE=4` as before; the `SHAKE`
  line now counts the shirt and trousers against the **drawn** body (baked fields), not the physics shapes.
- `FIGHT_CLOTHDUMP="6.1,13,21"`: `OUTFITTUBE` (every piece's points), `PARTFRAMES` (each part as drawn), `OUTFITNOW`.
  `python3 -I Tools/Headless/outfit.py name [mover|other|rack] [Part]` draws the real body meshes and the cloth from
  four sides into `$S/name.outfit.png` (with a part name: close up, `name.zoom.outfit.png`; `HIDE="shirt,sash"` leaves
  pieces out). **It has its own depth test** (numba): matplotlib's 3D painted skin over cloth and showed bare stripes
  that were not there. Run it with `-I`, or `coverage.py` beside it hides the `coverage` package numba needs.
- `python3 -I Tools/Headless/coverage.py name [who]`: for every vertex of the covered meshes, a ray straight out; bare if
  it meets no cloth within 20 cm (hidden if inside another part). Bare by design: neck, wrists, ankles.
- `FIGHT_CLOTHRACK=n`: the torture rack: at 2 s, n random poses within the joint limits, the outfit on each (`RACK`
  lines, the last three dumped as who `rack`); then it ends.
- `FIGHT_CLOTHSMOOTH=n` (trousers' smoothing passes, 2), `FIGHT_CLOTHMEASURE=n` (measure every n-th picture; headless 1,
  a game 4).
- The body is baked by `python3 -I Tools/Cloth/bake_body.py medium` into
  `Assets/_Battler/Resources/Locomotion/BodyFields_Medium.json` (no `.meta`: he makes it in Unity). Each limb's field is
  joined with a ball at its joint: the limb meshes' tops are hollow sockets, and cloth slipped into them unseen.
- **From his Unity logs:** `Tools/Headless/unitylog_outfit.sh PlaytestLogs/<folder> Swordsman [Part] [every]` draws the
  `clothdump` lines ("Shake him" writes one every 1.5 s, "Log the cloth now" one) with the real meshes into
  `$S/unity_Swordsman.outfit.png`. The `cloth` lines (twice a second) say what it is doing; `ms_Clothes` in perf.csv.
  Headless the same report is in the `OUTFITNOW` line.

## 22 November, afternoon: clothes cut and sewn (Tools/Cloth)

- `python3 -I Tools/Cloth/pattern.py [out.json] [cut.png]`: the shirt's and trousers' flat pieces drafted from the
  Medium's measures (every number a lever in `P`), cut into 2 cm triangles, the seams paired, each piece placed round
  him with the arms raised 60 degrees. Writes `Assets/_Battler/Resources/Locomotion/GarmentPattern_Medium.json` and a
  drawing of the cut, and prints every seam's two lengths.
- `python3 -I Tools/Cloth/drape.py [pattern.json] [out.json]`: sews and drapes them on the baked body (about 75 s, the
  same result every run) into `Garments_Medium.json`, and prints the measures (inside the body, seam gaps, stretch,
  folds, the waist line, the neck band, bare skin by rays). `DRAPE_SNAP=folder` draws every stage; `DRAPE_DEBUG=1`
  prints the widest seams, longest threads and the hems after each stage; `DRAPE_<LEVER>=value` for any of `LEVERS`.
- `python3 -I Tools/Cloth/garment.py [Garments.json] [out.png] [neck|armpit|waist|hem]`: the draped garments on the
  body from five sides, true depth, smooth shading (`zbuf.py`); `PX=600` bigger, `HIDE=shirt,sash` to leave things out.

## 22 November, afternoon: the sewn clothes on the moving body (step 3)

- `FIGHT_CLOTHES=3`: the shirt and trousers cut and sewn (`SewnOutfit`, from `Garments_Medium.json`), carried on the
  drawn body with the last word (the game's default in the labs and the dance now; 1 is the tubes). **Headless the
  clothes are off unless `FIGHT_CLOTHES` is given** (since 22 November, 15:55): they never touch the fight. Costly headless: about
  23 ms a fighter a picture in a Release build (`cd Tools/CompileCheck && dotnet build Tests.csproj -c Release`, then
  run `bin/Tests/Release/PhysicsBattler.Tests.dll`), several times that in Debug. `FIGHT_CLOTHES=0` for runs that do
  not need clothes.
- `FIGHT_CLOTHDUMP="6.1,12.5"` writes `SEWNPTS` (each garment's points, in the file's order), `PARTFRAMES`, `SEWNNOW`
  and `SEWNWORST` (the points the last word moved furthest, with their parts) lines. `FIGHT_CLOTHRACK=200` puts them
  on random poses (`RACK` lines; the first three dumped as who `rack`). The final `SEWN` line has the totals.
- `python3 -I Tools/Headless/sewn.py name [mover|other|rack] [neck|armpit|waist|arm|hem|elbows]`: the dumps drawn on
  the real body meshes, posed, true depth, five sides, a row a moment, into `$S/name.<who>.sewn.png`. It reads a Unity
  log's `clothdump` lines the same way (give the log's path as the name).
- **The moving sewn cloth (step 4, 22 November evening):** `FIGHT_SEWNMOVES=0` carried as in step 3. Levers:
  `FIGHT_SEWNWEIGHT`, `FIGHT_SEWNAIR`, `FIGHT_SEWNINERTIA`, `FIGHT_SEWNSQUASH`, `FIGHT_SEWNBEND`, `FIGHT_SEWNSWING`
  (metres), `FIGHT_SEWNREACH` (per metre), `FIGHT_SEWNHOLD`, `FIGHT_SEWNSTEPS`, `FIGHT_SEWNSWEEPS`.
  `FIGHT_SEWNPLUCK=2`: both men's cloth thrown forward at 2 s (`SEWNPLUCK` line). `FIGHT_LASTWORD=0`: no last word.
  `FIGHT_SEWNTRACE=1`: `SEWNTRACE t who hips x:y:z off cm ends cm hang cm shiver mm stretch % ms` every 0.1 s (`2`:
  every picture), and once a second `SEWNLONG`: threads over 105% and 120%, then the five longest, as drawn and as
  carried, with their parts, reach, distance from carried and depth (negative: inside). `python3
  Tools/Headless/sewnsum.py log...` sums a run (standing 2 to 4 s, the leap in 6.0 to 6.8 s, the leap out 8.2 to 9.0
  s, the thread counts). **`b.sh` builds Debug; these runs use `bin/Tests/Release`: build that with `dotnet build
  Tests.csproj -c Release` first.**
- **After his look (19:45):** `FIGHT_SEWNBACKSTOP=0` (no backstop), `FIGHT_SEWNTUCK=0` (trousers as draped under the
  shirt), `FIGHT_SEWNLONGEST=1.2` (threads drawn in over this), `FIGHT_SEWNPUSHEVERY=2` (the body's push every n small
  steps and the last). The `SEWN` line now ends with "points through (a limb out through the cloth)": points more than
  1 cm past his skin along their own way out. `SEWNTIME`: headless time in the moving step (threads and bends, the
  body's push) and in the last word.
- `FIGHT_SEWNBALLS=0` (two parts' points blended straight, not carried round the joint), `FIGHT_SEWNTHIGHS=0` (the
  shirt's hem on his hips alone), `FIGHT_SEWNROUNDS=n` (rounds of middles, 6), `FIGHT_SEWNFOLDS=0` (cloth in a crease pushed out
  of it: flaps), `FIGHT_SEWNSMOOTH=n`, `FIGHT_SEWNKEEPMOST=1`, `FIGHT_SEWNRADIAL=1` (ways tried and left off).

## Clothes that slide, and the shield that lets go (22 November, 22:00)

- `stR.sh name ENV...`: the shield duel (as `st.sh`) on the **Release** build, faster: build it first with
  `dotnet build Tools/CompileCheck/Tests.csproj -c Release`. The cloth runs use `FIGHT_CLOTHES=3` (the sewn clothes;
  without it no clothes are worn headless) and `FIGHT_SHAKE=4` ("Shake him").
- `SHIELDIN` line (every report): a part of the other man more than 2 cm inside a shield, as seen, how long and how
  deep; the stretches with what each was doing, how far the part and the board were from their poses. And how often
  a shield let go of a man stuck in it. `FIGHT_SHIELDLETSGO=0` (old), `FIGHT_LETGOAFTER`, `FIGHT_LETGODEEP`.
  **The stuck board of his log could not be made headless:** headless the engine pushes a board out of a chest in
  0.03 s; in his Unity it stayed 9 cm in for 1.1 s.
- `SOLIDS` lines (every report): each part's solid in its own space and the board in the shield hand's space.
  `python3 Tools/Headless/shieldin_log.py PlaytestLogs/<date_time> <report with SOLIDS> [shielded units]` reads the
  same measure out of one of his logs that has no `shieldin` lines (approximate: the board taken as fixed in the hand).
- `SEWNLOOK` line: what he would see of the cloth, on measured pictures: a spike that shows (a thread over 2 times its
  length with both ends and its middle outside him), over 3 times; pinched threads (under 60% of their length) at the
  armpit, elbow, knee; the shoulder seam's ridge beyond the drape's; how far a limb's roll was left out of its sleeve.
- Switches, each the old way: `FIGHT_SEWNROLL=1` (sleeves roll with the limb), `FIGHT_SEWNSLIDE=0` (no slide, no
  leash, no grip; levers `FIGHT_SEWNSLIDEBY`, `FIGHT_SEWNSLIDEHOLD`, `FIGHT_SEWNGRIP`, `FIGHT_SEWNGRIPTRUNK`,
  `FIGHT_SEWNLEASHSLACK`), `FIGHT_SEWNSEAMS=0` (each side of the shoulder seam by its own rule), `FIGHT_SEWNJOINTREACH=1` (on: it starts off),
  `FIGHT_SEWNFLOOR=0`, `FIGHT_SEWNLONE=0`, `FIGHT_SEWNJOLT=0` (no cap on a hard stop's throw).
- A fall test: `FIGHT_KICK=1 FIGHT_KICKED="5.2,0.25" FIGHT_BASHAT="3,11,19" FIGHT_BASHPUT="1.4"` (a hard kick that
  knocks him down, three times).

## 23 November: the guard's sleeve spike, at his picture rate

- `FIGHT_CLOTHPICTURE=n`: the clothes placed once every n physics steps, their time summed (`Clothes.HeadlessPictureEvery`).
  **2 is about his Unity's picture rate (35 a second): measure cloth there.** At 1 (60 a second, as before) the guard's
  spike his log showed did not appear.
- The guard as in his log of 06:57: `SECS=5 Tools/Headless/stR.sh name FIGHT_AIMKIT=open FIGHT_CLOTHES=3 FIGHT_SEWNTRACE=1
  FIGHT_CLOTHPICTURE=2` (the swordsman with sword and shield 1.5 m from a bare-handed target, standing; the run lasts about
  15 s whatever SECS says). The falls: add `FIGHT_KICK=1 FIGHT_KICKKIND=hard FIGHT_BASHAT="3,11" FIGHT_BASHPUT="1.4"` and
  leave out `FIGHT_AIMKIT` (both with shields; the kicked man is "other").
- `FIGHT_SEWNROUNDSMOST=0`: the last word's rounds with no limit (the old way); or metres (default 0.03).
- `FIGHT_PTRACE=i` (`FIGHT_PTRACESIDE=1`: the other man): `PTRACE` lines, point i stage by stage each picture (carried,
  moved, the points' pass, the rounds, the lone spikes: how far off its place, its own offset and speed, how deep in which
  part), `PMID` lines for each middle or centre that pushed it, and `PHELD` lines for every point held back (with
  `FIGHT_PTRACE=-1` only those). `SEWNLONG` names each thread's points (`#a/#b`). `SEWNTRACE` lines end with the crotch's
  longest thread and the worst spike that shows, with where.
