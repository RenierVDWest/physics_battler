#!/bin/bash
# held.sh prefix ENV... : the four fall directions (200 N·s), two at a time; one short line a fall, with what it held
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
pre=$1; shift
for pair in "0 180" "270 90"; do
  for t in $pair; do $H/run.sh ${pre}_$t FIGHT_SECONDS=8.2 FIGHT_TUBE="3,200,8,1.3,0.15,$t,1,1" "$@"; done
  for i in $(seq 1 34); do ok=1; for t in $pair; do [ -f $S/${pre}_$t.done ] || ok=0; done; [ $ok = 1 ] && break; sleep 3; done
done
for t in 0 180 270 90; do grep -a " fell " $S/${pre}_$t.txt | sed -E 's/^ *//; s/: first down.*the hips ([^,]*), the trunk [^,]*, the head ([^;]*);/: hips \1, head \2;/; s/landed on a hand or knee first/braced/; s/not on a hand or knee first/NOT braced/; s/its hips turned over ([0-9]+) degrees in all and rose again ([0-9]+) cm/turned \1, rose \2/; s/its hips turned over ([0-9]+) degrees in all/turned \1/; s/its hands went ([0-9]+) cm over the ground and left it again ([0-9]+) times/hands \1 cm, left \2/; s/an arm in its own body/arm in/; s/ over the ground\)/)/; s/; measured.*//' | cut -c1-360; python3 $S/handoff.py $S/${pre}_$t; done
grep -a -h "Exception\|exception" $S/${pre}_*.txt | head -3
