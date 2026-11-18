#!/bin/bash
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
# impset.sh : the four bashes, new and old, two at a time; prints the meter's lines
S=$S; cd $S
run2() { $H/imp1.sh $1 $2 $3 "${@:4}"; $H/imp1.sh ${1}o $2 $3 FIGHT_BASHIMPACT=0 "${@:4}"; timeout 170 bash -c "until [ -f imp_$1.done ] && [ -f imp_${1}o.done ]; do sleep 3; done"; }
run2 fh hard 1.9 FIGHT_COUNTERHOLDS=1
run2 fn hard 1.9 FIGHT_COUNTERHOLDS=0
run2 fm medium 1.8
run2 fs small 1.0
echo done > impset.done
