#!/bin/bash
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
# s4.sh <tag> [env...]: three 60 s fights of six Mediums; prints the roots moved at once and the feet's slide
S="$S"; tag="$1"; shift
cd "$H/../.."
for seed in 1 2 3; do (env "$@" FIGHT_LINEUP=mediums FIGHT_SECONDS=60 timeout 900 dotnet Tools/CompileCheck/bin/Tests/Debug/PhysicsBattler.Tests.dll fight $S/s4_$seed $seed > $S/s4_${tag}_$seed.txt 2>&1 &); done
sleep 2; until ! pgrep -f "PhysicsBattler.Tests.dll fight" >/dev/null; do sleep 3; done
for seed in 1 2 3; do grep "roots moved at once\|feet sliding on the ground (all\|fight over\|blows met" $S/s4_${tag}_$seed.txt | cut -c1-300; done
