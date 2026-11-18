#!/bin/bash
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
cd "$H/../.."
printf '%s\n' "e_h22 hard 2.2" "e_h16 hard 1.6" | SECS=10 $H/bk2.sh
rm -f $S/dance_e.done $S/reg_e.done
SECS=30 $H/dance.sh e FIGHT_DANCE="Prowler,Bull"
SECS=40 $H/reg.sh reg_e
until [ -f $S/dance_e.done ] && [ -f $S/reg_e.done ]; do sleep 5; done
echo == dance; grep -E "^BASHES|^BASH |xception|^DANCETALLY" $S/dance_e.txt | cut -c1-300 | head -20
echo == six; grep -E "^BASHES|xception|blows|Blows" $S/reg_e.txt | cut -c1-300 | head -12
