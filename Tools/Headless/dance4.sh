#!/bin/bash
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
cd "$H/../.."
rm -f $S/dance4_all.done
SECS=60 $H/dance.sh x_rr FIGHT_DANCE="Strutter,Prowler" FIGHT_DANCEREADS="1,1"; SECS=60 $H/dance.sh x_pp FIGHT_DANCE="Plain,Plain" FIGHT_DANCEREADS="1,0"
until [ -f $S/dance_x_rr.done ] && [ -f $S/dance_x_pp.done ]; do sleep 5; done
SECS=60 $H/dance.sh x_pps FIGHT_DANCE="Plain,Plain" FIGHT_DANCEREADS="0,1"; SECS=60 $H/dance.sh x_mix FIGHT_DANCE="Strutter,Prowler" FIGHT_DANCEREADS="1,1" FIGHT_DANCEMIXED="1,0"
until [ -f $S/dance_x_pps.done ] && [ -f $S/dance_x_mix.done ]; do sleep 5; done
echo all > $S/dance4_all.done
