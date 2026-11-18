#!/bin/bash
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
S="$S"
rm -rf "${S:?}/now"; $H/snap.sh $S/now >/dev/null 2>&1
bad=0
for f in $S/base/*.ev $S/base/physx.out $S/base/arm.out; do n=$S/now/$(basename $f); if ! cmp -s $f $n; then echo "DIFFERS: $(basename $f) ($(wc -l < $f) vs $(wc -l < $n) lines)"; bad=1; fi; done
[ $bad = 0 ] && echo "ALL SAME ($(ls $S/base/*.ev | wc -l) fights, physx checks, arm experiment)"
