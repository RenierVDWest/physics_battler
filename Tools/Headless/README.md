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
| `run.sh name FIGHT_SECONDS=8.5 FIGHT_TUBE="3,200,8,1.3,0.15,<angle>,1,1"` | The tube test: a tube knocks fighters down (angle 0 back, 180 front, 90 and 270 the sides). "B Medium" is the shield bearer. |
| `gup.sh tag "270 180" [ENV…]` | Tube falls from the given sides, then each get-up's speeds, floor and tremble. Waits and prints. |
| `gu.sh file.txt` | The get-up lines of a report, shortened. |
| `shk.sh names…`, `gw.sh names…`, `six.sh names…` | Tables out of finished reports: tremble, own crossings, the shield giving way. |
| `film.sh name from to [ENV…]` | A duel filmed between two times; draw it with `Tools/Mocap/Rhythm/film.py`. |
| `checks.sh` | The regular check runs (falls, twelve bodies, sixteen in a fight, the six). Long. |

The rest are one-off comparisons from particular days, kept for reference.

## Switches used most

`FIGHT_GETUPHELD=0` (get-ups do not know what the hands hold), `FIGHT_GETUPCLAMP=0`, `FIGHT_GETUPRECORDED=0`,
`FIGHT_ABSORB=0`, `FIGHT_ABSORBED=0.6`, `FIGHT_OWNSOLID="1,1"` (shield, sword solid against his own body;
`"0,0"` is the old way), `FIGHT_SETTLE=0` or `"26,60"`, `FIGHT_HELDSOFT=0`, `FIGHT_GIVEWAY=0` or `"0.06,0.3"`,
`FIGHT_SHAKELOG=1` and `FIGHT_SHAKETRACE=1` (traces), `UPPER_GHOST=1` (no contacts on the upper body),
`UPPER_DAMP`, `UPPER_FOLLOW`, `UPPER_GIVEWAY`.

## Film strips

`python3 Tools/Mocap/Rhythm/film.py film.json out.png --moments 10 --from t0 --to t1 --title T`, from a run
with `FIGHT_FILM="$S/name.json,3,8.5,medium"`.
