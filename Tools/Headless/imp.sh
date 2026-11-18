#!/bin/bash
# imp.sh tag kind apart ENV... : one bash at 4 s in the duel line-up, new and old side by side; prints the IMPACT lines
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
tag=$1; kind=$2; apart=$3; shift 3
cd $S; rm -f imp_${tag}_new.done imp_${tag}_old.done
(SECS=7 $H/st.sh imp_${tag}_new FIGHT_DUELAPART=$apart FIGHT_BASHAT="4" FIGHT_BASHKIND=$kind "$@" > /dev/null 2>&1 &)
(SECS=7 $H/st.sh imp_${tag}_old FIGHT_DUELAPART=$apart FIGHT_BASHAT="4" FIGHT_BASHKIND=$kind FIGHT_BASHIMPACT=0 "$@" > /dev/null 2>&1 &)
timeout 110 bash -c "until [ -f imp_${tag}_new.done ] && [ -f imp_${tag}_old.done ]; do sleep 3; done"
for v in new old; do echo "== $tag $v"; grep -h "^IMPACT\|^BASHED\|Exception" imp_${tag}_$v.txt | cut -c1-700; done
