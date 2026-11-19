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
- The panel check ends with `LOOK HERE (lab): n of m names drawn and framed`: the names of the panel's "look here"
  list (`BattleSandbox.cs`) that are really drawn in that lab, with every section open.

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
