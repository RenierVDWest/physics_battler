#!/bin/bash
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
cd "$H/../.."
rm -f $S/dance3_all.done
SECS=60 $H/dance.sh v_pp FIGHT_DANCE="Plain,Plain" FIGHT_DANCEREADS="1,0"; SECS=60 $H/dance.sh v_rr FIGHT_DANCE="Strutter,Prowler" FIGHT_DANCEREADS="1,1"
until [ -f $S/dance_v_pp.done ] && [ -f $S/dance_v_rr.done ]; do sleep 5; done
SECS=60 $H/dance.sh v_rn FIGHT_DANCE="Strutter,Prowler" FIGHT_DANCEREADS="1,0"; SECS=60 $H/dance.sh v_nr FIGHT_DANCE="Strutter,Prowler" FIGHT_DANCEREADS="0,1"
until [ -f $S/dance_v_rn.done ] && [ -f $S/dance_v_nr.done ]; do sleep 5; done
SECS=60 $H/dance.sh v_mix FIGHT_DANCE="Strutter,Prowler" FIGHT_DANCEREADS="1,1" FIGHT_DANCEMIXED="1,0"; SECS=60 $H/dance.sh v_off FIGHT_DANCE="Strutter,Prowler" FIGHT_DANCEREADS="1,1" FIGHT_DANCEREADING=0
until [ -f $S/dance_v_mix.done ] && [ -f $S/dance_v_off.done ]; do sleep 5; done
echo all > $S/dance3_all.done
