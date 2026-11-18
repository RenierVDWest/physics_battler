#!/bin/bash
# tsl.sh : lines "name degrees order [ENV...]": the other put round the mover at 4 s, every quick turn made with a cut; two at a time; then slash.py
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
names=()
run() { n=$1; d=$2; o=$3; shift 3; rm -f $S/$n.done; SECS=8 $H/st.sh $n FIGHT_DUEL="$o:90" FIGHT_BEHIND="4,$d" FIGHT_TURNSLASH=always FIGHT_HITS=1 FIGHT_TURNTRACE=1 FIGHT_FILM="$S/$n.json,3.6,7.8" "$@"; }
mapfile -t lines
i=0
while [ $i -lt ${#lines[@]} ]; do
  l1=(${lines[$i]}); l2=(${lines[$((i+1))]})
  run "${l1[@]}"; names+=(${l1[0]})
  if [ ${#l2[@]} -gt 0 ]; then run "${l2[@]}"; names+=(${l2[0]}); until [ -f $S/${l1[0]}.done ] && [ -f $S/${l2[0]}.done ]; do sleep 4; done; else until [ -f $S/${l1[0]}.done ]; do sleep 4; done; fi
  i=$((i+2))
done
for n in "${names[@]}"; do grep -E "xception|^\[turn\]" $S/$n.txt | cut -c1-400 | head -4; python3 $S/slash.py $n; done
