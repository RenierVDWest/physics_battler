#!/bin/bash
# thrown.sh prefix impulse ENV... : thrown falls from the front, the back and a side; prints them
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
pre=$1; imp=$2; shift; shift
for pair in $PAIRS; do
  for t in ${pair//_/ }; do $H/run.sh ${pre}_$t FIGHT_SECONDS=9 FIGHT_TUBE="3,$imp,8,1.3,0.15,$t,1,1" "$@"; done
  for i in $(seq 1 36); do ok=1; for t in ${pair//_/ }; do [ -f $S/${pre}_$t.done ] || ok=0; done; [ $ok = 1 ] && break; sleep 3; done
done
for t in $ALLT; do echo "== turned $t"; grep -a "fell " $S/${pre}_$t.txt | sort -u | sed -E 's/^ *//; s/first down[^;]*; //; s/, a knee [^,]*//; s/, the trunk [^,]*//; s/landed on a hand or knee first/braced/; s/not on a hand or knee first/NOT braced/; s/its hips turned over/turned/; s/ degrees in all/ deg/; s/its hands went ([0-9]+) cm over the ground and left it again ([0-9]+) times/hands \1 cm, lifted \2/; s/, faster than its legs can catch \(4.4\): down//; s/the fall test: a tube of [0-9]* N·s on its [a-z ]*/test/' | cut -c1-260; grep -a "falls it tucked\|Exception\|threw" $S/${pre}_$t.txt | cut -c1-120; done
