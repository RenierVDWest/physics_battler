#!/bin/bash
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
S="$S"; cd $S
run() { tag=$1; shift; $H/b6.sh $tag "1" 60 FIGHT_LINEUP=targets "$@" > $S/$tag.log 2>&1; }
run wAm FIGHT_WMASS=0 & run wCm FIGHT_WMASS=3,0 & run wDm FIGHT_WMASS=3,1 & wait
run wEm FIGHT_WMASS=6,1 & run wAl FIGHT_TARGETS=LL FIGHT_WMASS=0 & run wBl FIGHT_TARGETS=LL FIGHT_WMASS=1,1 & wait
run wCl FIGHT_TARGETS=LL FIGHT_WMASS=3,0 & run wDl FIGHT_TARGETS=LL FIGHT_WMASS=3,1 & run wEl FIGHT_TARGETS=LL FIGHT_WMASS=6,1 & wait
echo all-finished > $S/wm.done
