#!/bin/bash
# imp1.sh tag kind apart ENV... : one bash at 4 s, in the background
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
tag=$1; kind=$2; apart=$3; shift 3
cd $S; rm -f imp_$tag.done
SECS=7 $H/st.sh imp_$tag FIGHT_DUELAPART=$apart FIGHT_BASHAT="4" FIGHT_BASHKIND=$kind "$@" > /dev/null 2>&1
