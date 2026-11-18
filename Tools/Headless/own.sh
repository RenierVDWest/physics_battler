#!/bin/bash
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
run() { n=$1; shift; SECS=13 $H/aim.sh own_$n FIGHT_DUELPACE=1.6 FIGHT_DUEL="CircleLeft:5,WalkTo:2,BackAway:3" FIGHT_DUELAPART=2.5 FIGHT_DROPAT=11.6 "$@"; }
run Plain FIGHT_OWN="Plain,Strutter" FIGHT_FILM="$S/own_Plain.json,2.5,6"; run Prowler FIGHT_OWN="Prowler,Strutter" FIGHT_FILM="$S/own_Prowler.json,2.5,6"
until [ -f $S/own_Plain.done ] && [ -f $S/own_Prowler.done ]; do sleep 5; done
run Strutter FIGHT_OWN="Strutter,Prowler" FIGHT_FILM="$S/own_Strutter.json,2.5,6"; run Bull FIGHT_OWN="Bull,Prowler" FIGHT_FILM="$S/own_Bull.json,2.5,6"
until [ -f $S/own_Strutter.done ] && [ -f $S/own_Bull.done ]; do sleep 5; done
run off FIGHT_OWN="Prowler,Strutter" FIGHT_OWNON=0; run none
until [ -f $S/own_off.done ] && [ -f $S/own_none.done ]; do sleep 5; done
for n in Plain Prowler Strutter Bull off none; do echo "== $n"; grep -E "^OWN|^DROP|xception" $S/own_$n.txt; grep -c "" $S/own_$n.txt; done
