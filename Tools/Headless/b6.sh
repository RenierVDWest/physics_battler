#!/bin/bash
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
# b6.sh <tag> <seeds, e.g. "1 2"> <secs> [env...]: physical fights in parallel to b6_<tag>_<seed>; for each, the limb-spin measures and the report's lines
S="$S"; tag="$1"; seeds="$2"; secs="$3"; shift 3
cd ${ROOT:-$H/../..}
pids=""
for seed in $seeds; do
  rm -rf "${S:?}/b6_${tag}_${seed}"
  env "$@" PHYSX=1 FIGHT_PHYSICAL=1 FIGHT_SECONDS=$secs timeout 3000 dotnet Tools/CompileCheck/bin/Tests/Debug/PhysicsBattler.Tests.dll fight $S/b6_${tag}_${seed} $seed > $S/b6_${tag}_${seed}.txt 2>&1 &
  pids="$pids $!"
done
wait $pids
for seed in $seeds; do
  d=$S/b6_${tag}_${seed}/PlaytestLogs/$(ls $S/b6_${tag}_${seed}/PlaytestLogs | tail -1)
  echo "== $tag seed $seed"
  python3 $S/tools/roll.py $d/units | tail -1
  python3 $S/tools/roll.py $d/units | grep ROLL | grep -v "hand off [0-9][0-9][0-9]" | cut -c1-150
  python3 $S/tools/elbows.py $d/units | tail -1
  python3 $S/tools/thrash.py $d/units 45 | tail -1
  python3 $S/tools/shake.py $d/units | grep "^  ForearmR\|^  HandR\|^  Head \|^  UpperArm"
  grep "a sword on\|a club on\|borne down (\|fight over\|strike:\|otherwise:\|shield hand:\|the head:\|in its hand:\|a blade\|a shield in\|a body in\|xception\|rror" $S/b6_${tag}_${seed}.txt | grep -v "^\[Upper\]" | cut -c1-200
  echo "joints put, by unit: $(grep -o "joints put where the pose turned them at once: [0-9]*" $S/b6_${tag}_${seed}.txt | grep -o "[0-9]*$" | tr '\n' ' ')"
done
