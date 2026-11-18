#!/bin/bash
# tkw.sh label ENV... : B Open thrown from the front and the back at 400, 500, 550, 650, 700; head and turn in one line each
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
lab=$1; shift
for imp in 400 500 550 650 700; do
  for t in 0 180; do $H/run.sh ${lab}_${imp}_$t FIGHT_SECONDS=10.5 FIGHT_TUBE="3,$imp,8,1.3,0.15,$t,1,1" "$@"; done
  for i in $(seq 1 36); do [ -f $S/${lab}_${imp}_0.done ] && [ -f $S/${lab}_${imp}_180.done ] && break; sleep 3; done
done
for imp in 400 500 550 650 700; do for t in 0 180; do grep -a "B Open fell" $S/${lab}_${imp}_$t.txt | head -1 | sed -E "s/^ *B Open fell/$imp/; s/: first down.*the head ([^;]*);/: head \1;/; s/its hips turned over/turned/; s/ degrees in all/ deg/; s/its hands went ([0-9]+) cm over the ground and left it again ([0-9]+) times/hands \1 cm/; s/; measured ([0-9.]+) s.*/; \1 s/; s/not on a hand or knee first/NOT braced/; s/landed on a hand or knee first/braced/" | cut -c1-230; done; done
