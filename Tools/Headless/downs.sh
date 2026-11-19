#!/bin/bash
# downs.sh tag ENV... : a shield bearer bashed down six times in a run (hard, not countered, and extra hard), two runs side by side; prints each run's crossings off his feet
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; export S="${S:-/tmp/pb}"; tag=$1; shift
cd $S; rm -f dn_${tag}_h.done dn_${tag}_x.done
SECS=41 $H/st.sh dn_${tag}_h FIGHT_DUELAPART=1.9 FIGHT_BASHAT="4,10,16,22,28,34" FIGHT_BASHPUT="1.9,1.6,2.1" FIGHT_BASHKIND=hard FIGHT_COUNTER=0 "$@" > /dev/null 2>&1
SECS=41 $H/st.sh dn_${tag}_x FIGHT_DUELAPART=1.9 FIGHT_BASHAT="4,10,16,22,28,34" FIGHT_BASHPUT="1.9,1.6,2.1" FIGHT_BASHKIND=extra "$@" > /dev/null 2>&1
timeout 900 bash -c "until [ -f $S/dn_${tag}_h.done ] && [ -f $S/dn_${tag}_x.done ]; do sleep 3; done"
for t in h x; do n=dn_${tag}_$t; echo "== $n"
  sed -n '/own crossings, as seen/,/what he holds trembles/p' $S/$n.txt | grep -a "falling or down\|getting up \|by part" | grep -av "Medium:" | sed -E 's/ +/ /g' | cut -c1-230
  sed -n '/own crossings, as seen/,/what he holds trembles/p' $S/$n.txt | grep -a "it began falling or down" | sed -E 's/ +/ /g; s/ \[the seen.*//' | cut -c1-200
  grep -a "the first 0.75 s of a get-up" $S/$n.txt | sed -E 's/^ +//' | cut -c1-420
  grep -a "^CROSSINGS" $S/$n.txt | cut -c1-330; grep -ac "^STUMBLE.*: down" $S/$n.txt; grep -ac Exception $S/$n.txt; done
