#!/bin/bash
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
# s5.sh <tag> [env...]: three 60 s fights of six Mediums on the real engine; prints what passes through what
S="$S"; tag="$1"; shift
cd "$H/../.."
for seed in 1 2 3; do (env "$@" PHYSX=1 FIGHT_SECONDS=60 timeout 1700 dotnet Tools/CompileCheck/bin/Tests/Debug/PhysicsBattler.Tests.dll fight $S/s5d_$seed $seed > $S/s5d_${tag}_$seed.txt 2>&1 &); done
sleep 2; until ! pgrep -f "PhysicsBattler.Tests.dll fight" >/dev/null; do sleep 5; done
for seed in 1 2 3; do grep "borne down (\|blows met\|roots moved\|fight over\|strike:\|otherwise:\|shield hand:\|the head:\|in its hand:\|'s head:\|a blade\|a shield in\|a body in\|feet sliding (all\|xception\|rror" $S/s5d_${tag}_$seed.txt | cut -c1-210; done
