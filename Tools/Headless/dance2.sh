#!/bin/bash
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
cd "$H/../.."
until [ -f $S/dance_rr2.done ] && [ -f $S/dance_nr.done ]; do sleep 5; done
SECS=45 $H/dance.sh rn FIGHT_DANCE="Strutter,Prowler" FIGHT_DANCEREADS="1,0"; SECS=45 $H/dance.sh mix FIGHT_DANCE="Strutter,Prowler" FIGHT_DANCEREADS="1,1" FIGHT_DANCEMIXED="1,0"
until [ -f $S/dance_rn.done ] && [ -f $S/dance_mix.done ]; do sleep 5; done
SECS=45 $H/dance.sh bp FIGHT_DANCE="Bull,Prowler" FIGHT_DANCEREADS="1,1"; SECS=45 $H/dance.sh pp FIGHT_DANCE="Plain,Plain" FIGHT_DANCEREADS="1,0"
until [ -f $S/dance_bp.done ] && [ -f $S/dance_pp.done ]; do sleep 5; done
echo all > $S/dance_all.done
