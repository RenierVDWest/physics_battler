#!/bin/bash
# tb.sh : lines on stdin "name degrees [ENV...]": A turns away by degrees at 3 s and back by himself; two at a time; then the turn backs
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
names=()
run() { n=$1; d=$2; shift 2; rm -f $S/$n.done; SECS=8 $H/st.sh $n FIGHT_TURN="3,$d,a m" FIGHT_FILM="$S/$n.json,2.8,7.8" "$@"; }
mapfile -t lines
i=0
while [ $i -lt ${#lines[@]} ]; do
  l1=(${lines[$i]}); l2=(${lines[$((i+1))]})
  run "${l1[@]}"; names+=(${l1[0]})
  if [ ${#l2[@]} -gt 0 ]; then run "${l2[@]}"; names+=(${l2[0]}); until [ -f $S/${l1[0]}.done ] && [ -f $S/${l2[0]}.done ]; do sleep 4; done; else until [ -f $S/${l1[0]}.done ]; do sleep 4; done; fi
  i=$((i+2))
done
for n in "${names[@]}"; do grep -E "xception" $S/$n.txt | head -2; python3 $S/turnback.py $S/$n.json | grep " A:"; done
