#!/bin/bash
# psum.sh tag... : the planted battery's blows in one line each
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
for t in "$@"; do for f in $S/${t}_*.txt; do n=$(basename $f .txt); grep -h "^AIM" $f | sed -E "s/^AIM [0-9.]+ asked for the ([a-z ]+) \(([0-9.]+) m apart.*with (made_[a-z_]+).*: met the ([A-Za-z]+) .*, ([0-9]+) cm from the spot.*its striking part came within ([0-9]+) cm.*the blade as posed came within ([0-9]+) cm.*/$n: \1, \2 m apart, \3, met \4 \5 cm off; striking part within \6; posed within \7/"; done; done
