#!/bin/bash
# shk.sh name... : the tremble and crossings tables of runs
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
for n in "$@"; do echo "== $n"; grep -a -A12 "what he holds trembles" $S/$n.txt | grep -a -B14 "in all" | grep -a "moving\|standing\|striking\|just struck\|in all" | sed -E 's/ +/ /g' | cut -c1-260; grep -a -A11 "own crossings" $S/$n.txt | grep -a "own crossings\|pose itself\|by part" | sed -E 's/ +/ /g; s/, as seen//; s/: seconds of each, and the share of that time//' | cut -c1-250; grep -a "^DANCEBLOW" $S/$n.txt | grep -ao "LANDED\|met the shield\|met nothing\|parried\|dodged" | sort | uniq -c | tr '\n' ' '; echo; done
