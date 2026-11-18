#!/bin/bash
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
cd "$H/../.."
printf '%s\n' "f_s10 small 1.0" "f_s10r0 small 1.0 FIGHT_BASHREACH=0" "f_s10r1 small 1.0 FIGHT_BASHREACH=1" "f_h22 hard 2.2" "f_m16 medium 1.6" "f_h22r0 hard 2.2 FIGHT_BASHREACH=0" | SECS=8 $H/bk2.sh
rm -f $S/dance_w3.done
SECS=45 $H/dance.sh w3 FIGHT_DANCE="Plain,Plain" FIGHT_SLOWSIDE=0 FIGHT_FILM="$S/w3.json,3,45"
until [ -f $S/dance_w3.done ]; do sleep 5; done
