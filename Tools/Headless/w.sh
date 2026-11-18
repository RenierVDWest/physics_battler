#!/bin/bash
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
# w.sh <tag> <seed> <secs> [env...]: one physical fight (default line-up unless FIGHT_LINEUP given) to w_<tag>_<seed>; prints how often joints thrash and the report's lines
S="$S"; tag="$1"; seed="$2"; secs="$3"; shift 3
cd "$H/../.."
rm -rf "${S:?}/w_${tag}_${seed}"
env "$@" PHYSX=1 FIGHT_PHYSICAL=1 FIGHT_SECONDS=$secs timeout 1700 dotnet Tools/CompileCheck/bin/Tests/Debug/PhysicsBattler.Tests.dll fight $S/w_${tag}_${seed} $seed > $S/w_${tag}_${seed}.txt 2>&1
d=$S/w_${tag}_${seed}/PlaytestLogs/$(ls $S/w_${tag}_${seed}/PlaytestLogs | tail -1)
{ echo "== $tag seed $seed"; python3 $S/tools/thrash.py $d/units 45 | tail -1; echo "contacts $(grep -c ' contact ' $d/events.log)"; grep "borne down (\|blows met\|roots moved\|fight over\|strike:\|otherwise:\|shield hand:\|the head:\|in its hand:\|a blade\|a shield in\|a body in\|xception\|rror" $S/w_${tag}_${seed}.txt | cut -c1-230; } > $S/w_${tag}_${seed}.sum
