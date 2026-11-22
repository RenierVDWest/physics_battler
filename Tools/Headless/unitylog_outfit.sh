#!/bin/bash
# unitylog_outfit.sh <playtest folder> <who> [Part] [every]: the cloth Unity drew, from his playtest log's "clothdump"
# lines ("Shake him" writes them every 1.5 s, "Log the cloth now" once), drawn with the real body meshes by outfit.py
# into $S/unity_<who>.outfit.png. who: the unit's name in the log (Swordsman, Target). every: draw every n-th dump.
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
log="$1/events.log"; who="$2"; part="$3"; every="${4:-1}"
grep -a "| clothdump | " "$log" | sed 's/^.*| clothdump | //' | grep -a " $who " > "$S/unity_$who.all"
# (keep every n-th picture, at most six)
awk -v e="$every" '{ if (!($2 in seen)) { seen[$2]=1; n++; keep[$2] = ((n-1) % e == 0 && kept < 6); if (keep[$2]) kept++ } if (keep[$2]) print }' "$S/unity_$who.all" > "$S/unity_$who.txt"
echo "$(cut -d' ' -f2 "$S/unity_$who.txt" | sort -u | wc -l) pictures of $who"
cd "$H/../.." && S="$S" python3 -I "$H/outfit.py" "unity_$who" "$who" $part
