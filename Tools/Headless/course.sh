#!/bin/bash
# course.sh : lines "name ENV..." on stdin: the footwork course in the duel lineup, two runs at a time; prints each COURSE line
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
cd "$H/../.."
run() { n=$1; shift; rm -f $S/$n.done; SECS=28 $H/st.sh $n FIGHT_COURSE=1 FIGHT_BASHWITHIN=0.3 FIGHT_FILM="$S/$n.json,2,27,A" "$@"; }
mapfile -t lines; i=0; names=()
while [ $i -lt ${#lines[@]} ]; do
  l1=(${lines[$i]}); l2=(${lines[$((i+1))]})
  run "${l1[@]}"; names+=(${l1[0]})
  if [ ${#l2[@]} -gt 0 ]; then run "${l2[@]}"; names+=(${l2[0]}); until [ -f $S/${l1[0]}.done ] && [ -f $S/${l2[0]}.done ]; do sleep 4; done; else until [ -f $S/${l1[0]}.done ]; do sleep 4; done; fi
  i=$((i+2))
done
for n in "${names[@]}"; do echo "== $n"; grep -E "^COURSE|xception" $S/$n.txt | cut -c1-420; done
