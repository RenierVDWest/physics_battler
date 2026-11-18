#!/bin/bash
# bk2.sh name kind apart [ENV...] ; name kind apart [ENV...] ... given as lines on stdin: two runs at a time
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
cd "$H/../.."
names=()
run() { n=$1; k=$2; a=$3; shift 3; rm -f $S/$n.done; SECS=${SECS:-9} $H/st.sh $n FIGHT_BASHAT="4" FIGHT_BASHWITHIN=0.3 FIGHT_BASHKIND=$k FIGHT_DUELAPART=$a FIGHT_FILM="$S/$n.json,3.6,7.4" "$@"; }
mapfile -t lines
i=0
while [ $i -lt ${#lines[@]} ]; do
  l1=(${lines[$i]}); l2=(${lines[$((i+1))]})
  run "${l1[@]}"; names+=(${l1[0]})
  if [ ${#l2[@]} -gt 0 ]; then run "${l2[@]}"; names+=(${l2[0]}); until [ -f $S/${l1[0]}.done ] && [ -f $S/${l2[0]}.done ]; do sleep 4; done; else until [ -f $S/${l1[0]}.done ]; do sleep 4; done; fi
  i=$((i+2))
done
for n in "${names[@]}"; do echo == $n; grep -E "^BASHED|xception" $S/$n.txt | cut -c1-420; done
