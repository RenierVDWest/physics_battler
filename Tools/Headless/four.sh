#!/bin/bash
# four.sh tag ENV... : small, medium, hard countered, hard not countered, two at a time; prints the lines
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; export S="${S:-/tmp/pb}"; tag=$1; shift
$H/imp1.sh ${tag}_m medium 1.8 "$@"; $H/imp1.sh ${tag}_h1 hard 1.9 "$@"
timeout 420 bash -c "until [ -f $S/imp_${tag}_m.done ] && [ -f $S/imp_${tag}_h1.done ]; do sleep 3; done"
$H/imp1.sh ${tag}_s small 1.0 "$@"; $H/imp1.sh ${tag}_h0 hard 1.9 FIGHT_COUNTER=0 "$@"
timeout 420 bash -c "until [ -f $S/imp_${tag}_s.done ] && [ -f $S/imp_${tag}_h0.done ]; do sleep 3; done"
for t in s m h1 h0; do echo "== $t"; grep -h "^IMPACT\|^BASHED\|^STUMBLE\|Exception\|   at " $S/imp_${tag}_$t.txt | cut -c1-${CUT:-520}; done
