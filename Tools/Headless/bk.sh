#!/bin/bash
# bk.sh : each of the three bashes once, from several distances, two runs at a time
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
cd "$H/../.."
run() { rm -f $S/$1.done; SECS=8 $H/st.sh $1 FIGHT_BASHAT="4" FIGHT_BASHWITHIN=0.3 FIGHT_BASHKIND=$2 FIGHT_DUELAPART=$3 FIGHT_FILM="$S/$1.json,3.6,6.4"; }
pair() { run $1 $2 $3; run $4 $5 $6; until [ -f $S/$1.done ] && [ -f $S/$4.done ]; do sleep 4; done; }
pair bk_s10 small 1.0 bk_s13 small 1.3
pair bk_m16 medium 1.6 bk_m20 medium 2.0
pair bk_h16 hard 1.6 bk_h22 hard 2.2
pair bk_h28 hard 2.8 bk_m12 medium 1.2
for n in bk_s10 bk_s13 bk_m12 bk_m16 bk_m20 bk_h16 bk_h22 bk_h28; do echo == $n; grep -E "^BASHED|xception" $S/$n.txt | cut -c1-420; done
