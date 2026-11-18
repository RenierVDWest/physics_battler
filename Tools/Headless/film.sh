#!/bin/bash
# film.sh name from to ENV... : a duel run filmed from..to seconds (all units), drawn to $S/name.png when done (foreground)
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
n=$1; a=$2; b=$3; shift 3
SECS=${SECS:-7} $H/aim.sh $n FIGHT_FILM="$S/$n.json,$a,$b" "$@"
