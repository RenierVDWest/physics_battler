#!/bin/bash
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
cd "$H/../.."
rm -f $S/dance5_all.done
SECS=50 $H/dance.sh m_sp FIGHT_DANCE="Strutter,Prowler" FIGHT_OWN="Strutter,Prowler"; SECS=50 $H/dance.sh m_bp FIGHT_DANCE="Bull,Prowler" FIGHT_OWN="Bull,Prowler"
until [ -f $S/dance_m_sp.done ] && [ -f $S/dance_m_bp.done ]; do sleep 5; done
SECS=50 $H/dance.sh m_bb FIGHT_DANCE="Bull,Bull" FIGHT_OWN="Bull,Bull"; SECS=50 $H/dance.sh m_pp FIGHT_DANCE="Prowler,Prowler" FIGHT_OWN="Prowler,Prowler"
until [ -f $S/dance_m_bb.done ] && [ -f $S/dance_m_pp.done ]; do sleep 5; done
echo all > $S/dance5_all.done
