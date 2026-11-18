#!/bin/bash
# gup.sh tag "angles" ENV... : a shield bearer felled (0 back,180 front,90,270 sides), two at a time; prints each get-up's speeds, floor and tremble
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
tag=$1; angles=($2); shift 2
cd "$H/../.."
T='3,200,8,1.3,0.15'
i=0
while [ $i -lt ${#angles[@]} ]; do
  a=${angles[$i]}; b=${angles[$((i+1))]}
  $H/run.sh gu_${tag}_$a FIGHT_SECONDS=9 FIGHT_TUBE="$T,$a,1,1" FIGHT_SHAKELOG=1 "$@"
  [ -n "$b" ] && $H/run.sh gu_${tag}_$b FIGHT_SECONDS=9 FIGHT_TUBE="$T,$b,1,1" FIGHT_SHAKELOG=1 "$@"
  timeout 110 bash -c "until [ -f $S/gu_${tag}_$a.done ] && { [ -z '$b' ] || [ -f $S/gu_${tag}_$b.done ]; }; do sleep 3; done"
  i=$((i+2))
done
for a in "${angles[@]}"; do
  n=gu_${tag}_$a
  echo "== $n"
  grep -a "got up" $S/$n.txt | sed -E 's/^ *//' | grep -o "^[A-Za-z ]*got up from [a-z ]*\|fastest a part moved early [^;]*\|planted hand or foot at most [0-9]* cm[^,]*\|its shield over the ground at the lowest[^;]*\|in the ground for [0-9.]* s" | tr '\n' '|' | sed 's/|B /\nB /g'; echo
  grep -a -A12 "what he holds trembles" $S/$n.txt | grep -a "getting up" | sed -E 's/ +/ /g' | cut -c1-260
done
