#!/bin/bash
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
run() { n=$1; shift; SECS=16 $H/aim.sh slow_$n FIGHT_DUEL="CircleLeft:5,CircleRight:4,WalkTo:2.5,BackAway:3" FIGHT_DUELAPART=2.5 FIGHT_FILM="$S/slow_$n.json,2.5,11" "$@"; }
run crouched; run sidestep FIGHT_SLOWSIDE=0
until [ -f $S/slow_crouched.done ] && [ -f $S/slow_sidestep.done ]; do sleep 5; done
echo all > $S/slow_all.done
