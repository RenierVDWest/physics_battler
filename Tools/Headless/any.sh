#!/bin/bash
# any.sh tag ENV... : the any-pose battery (walk in at three paces, circle, back away), two at a time; prints DUELBLOW/DUELSTART/AIM
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
tag=$1; shift
run() { n=$1; shift; SECS=${SECS:-9} $H/aim.sh ${tag}_$n "$@"; }
run step FIGHT_DUEL="WalkTo:6" FIGHT_DUELAPART=3.2 FIGHT_DUELBLOW="${T1:-3.1}:Chest" "$@"
run walk FIGHT_DUELPACE=1.6 FIGHT_DUELNEAR=1 FIGHT_DUEL="WalkTo:6" FIGHT_DUELAPART=4 FIGHT_DUELBLOW="${T2:-3.03}:Chest" "$@"
until [ -f $S/${tag}_step.done ] && [ -f $S/${tag}_walk.done ]; do sleep 3; done
run jog FIGHT_DUELPACE=2.5 FIGHT_DUELNEAR=1 FIGHT_DUEL="WalkTo:6" FIGHT_DUELAPART=6 FIGHT_DUELBLOW="${T3:-3.42}:Chest" "$@"
run circle FIGHT_DUELPACE=1.6 FIGHT_DUEL="CircleLeft:7" FIGHT_DUELAPART=1.9 FIGHT_DUELBLOW="${T4:-4.0}:Chest" "$@"
until [ -f $S/${tag}_jog.done ] && [ -f $S/${tag}_circle.done ]; do sleep 3; done
run back FIGHT_DUELPACE=1.6 FIGHT_DUEL="BackAway:6" FIGHT_DUELAPART=1.2 FIGHT_DUELBLOW="${T5:-2.2}:Chest" "$@"
run stand FIGHT_DUEL="Stand:6" FIGHT_DUELAPART=2.0 FIGHT_DUELBLOW="3.0:Chest" "$@"
until [ -f $S/${tag}_back.done ] && [ -f $S/${tag}_stand.done ]; do sleep 3; done
for n in step walk jog circle back stand; do echo "== ${tag}_$n"; grep -E "^DUELBLOW|^DUELSTART|^AIM" $S/${tag}_$n.txt | sed -E 's/; its weight.*(the blade as posed)/; ... \1/' | sed -E 's/its striking part came within.*(the blade as posed)/\1/' | cut -c1-900; done
