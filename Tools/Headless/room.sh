#!/bin/bash
# room.sh tag ENV... : the room test: two shield bearers felled side by side, 0.6 m apart, three times in a run; new and old (FIGHT_ROOM=0) side by side; prints the ROOM lines
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; export S="${S:-/tmp/pb}"; tag=$1; shift
cd $S; rm -f room_${tag}_new.done room_${tag}_old.done
SECS=30 $H/st.sh room_${tag}_new FIGHT_FELLBOTH="4,12,20" "$@" > /dev/null 2>&1
SECS=30 $H/st.sh room_${tag}_old FIGHT_FELLBOTH="4,12,20" FIGHT_ROOM=0 "$@" > /dev/null 2>&1
timeout 560 bash -c "until [ -f $S/room_${tag}_new.done ] && [ -f $S/room_${tag}_old.done ]; do sleep 3; done"
for t in new old; do n=room_${tag}_$t; echo "== $n"; grep -a "^ROOM " $S/$n.txt | cut -c1-560; echo "out of hand: $(grep -ac 'out of hand' $S/$n.txt); exceptions: $(grep -ac Exception $S/$n.txt)"; grep -a "^CROSSINGS" $S/$n.txt | cut -c1-250; done
