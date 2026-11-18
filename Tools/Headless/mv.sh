#!/bin/bash
# mv.sh tag ENV... : on-the-move battery: circle left/right at 1.9 m with blows at five parts (the circle order given again before each so its distance is kept), walk in, jog in
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
tag=$1; shift
SECS=20 $H/aim.sh ${tag}_circ FIGHT_DUELPACE=1.6 FIGHT_DUEL="CircleLeft:9,CircleRight:9" FIGHT_DUELAPART=1.9 FIGHT_DUELBLOW="3:Head,5.5:ThighL,8:Belly,12:ShoulderR,14.5:ShinR,17:Chest" "$@"
SECS=12 $H/aim.sh ${tag}_in FIGHT_DUELPACE=1.6 FIGHT_DUEL="WalkTo:3.5,BackAway:2.5,WalkTo:4" FIGHT_DUELAPART=5 FIGHT_DUELBLOW="3.03:Chest,8.5:Belly" "$@"
until [ -f $S/${tag}_circ.done ] && [ -f $S/${tag}_in.done ]; do sleep 5; done
SECS=7 $H/aim.sh ${tag}_jog FIGHT_DUELPACE=2.5 FIGHT_DUEL="WalkTo:5" FIGHT_DUELAPART=7.5 FIGHT_DUELBLOW="${TJ:-3.72}:Chest" "$@"
SECS=7 $H/aim.sh ${tag}_back FIGHT_DUELPACE=1.6 FIGHT_DUEL="BackAway:5" FIGHT_DUELAPART=1.0 FIGHT_DUELBLOW="2.3:Chest" "$@"
until [ -f $S/${tag}_jog.done ] && [ -f $S/${tag}_back.done ]; do sleep 5; done
for n in circ in jog back; do echo "== ${tag}_$n"; grep -E "^DUELBLOW|^ONTHEMOVE made|^AIM|xception" $S/${tag}_$n.txt | sed -E 's/asked for the ([a-z ]+) \(.*with (made_[a-z_]+)[^:]*: /\1, \2: /; s/; its striking part.*(the blade as posed)/; \1/; s/; the body went.*//; s/at its landing.*the plan: /plan: /; s/, Cycle, feet down [0-9]\/[0-9]//' | cut -c1-300; done
