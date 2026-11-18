#!/bin/bash
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
S="$S"; cd $S
run() { tag=$1; shift; $H/b6.sh $tag "1" 60 FIGHT_LINEUP=targets "$@" > $S/$tag.log 2>&1; }
run xFm FIGHT_WMASS=3,0,1 & run xGm FIGHT_WMASS=3,1,1 & wait
run xHl FIGHT_TARGETS=LL FIGHT_WMASS=3,1,1 & run xIm FIGHT_WMASS=2,1,1 & wait
echo all-finished > $S/wm2.done
