#!/bin/bash
# falls4.sh prefix ENV... : the four fall directions (200 N·s), two at a time; prints the falls
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
pre=$1; shift
for pair in "0 180" "270 90"; do
  for t in $pair; do $H/run.sh ${pre}_$t FIGHT_SECONDS=8.2 FIGHT_TUBE="3,200,8,1.3,0.15,$t,1,1" "$@"; done
  for i in $(seq 1 34); do ok=1; for t in $pair; do [ -f $S/${pre}_$t.done ] || ok=0; done; [ $ok = 1 ] && break; sleep 3; done
done
for t in 0 180 270 90; do echo "== turned $t"; grep -a "fell " $S/${pre}_$t.txt | sed -E 's/^ *//; s/first down[^;]*; //; s/landed on a hand or knee first/braced/; s/not on a hand or knee first/NOT braced/; s/ \(thrown.*//; s/ \(the fall test.*//' | cut -c1-330; grep -a "falls measured" $S/${pre}_$t.txt | sed -E 's/.*Of the living: //' | cut -c1-330; done
