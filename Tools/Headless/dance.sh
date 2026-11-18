#!/bin/bash
# dance.sh tag ENV... : one dance of SECS seconds in the shield lineup; waits; prints
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
tag=$1; shift
SECS=${SECS:-45} $H/st.sh dance_$tag "$@"
