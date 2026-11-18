#!/bin/bash
# st.sh name ENV... : the shield test in the duel lineup (target and swordsman with sword and shield, 1.5 m apart), in the background
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
n=$1; shift
$H/aim.sh $n FIGHT_AIMKIT=shield FIGHT_AIMWHO=shield FIGHT_DUEL="Stand:90" FIGHT_DUELAPART=1.5 "$@"
