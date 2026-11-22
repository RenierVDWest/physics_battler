#!/bin/bash
# stR.sh name ENV... : the shield test in the duel lineup on the Release build (faster), in the background
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
n=$1; shift; rm -f "${S:?}/$n.done"; rm -rf "${S:?}/$n"
cd "$H/../.."
(env PHYSX=1 FIGHT_PHYSICAL=1 FIGHT_SECONDS=${SECS:-80} FIGHT_LINEUP=aim FIGHT_AIMKIT=shield FIGHT_AIMWHO=shield FIGHT_DUEL="Stand:90" FIGHT_DUELAPART=1.5 "$@" timeout 2400 dotnet Tools/CompileCheck/bin/Tests/Release/PhysicsBattler.Tests.dll fight $S/$n ${SEED:-5} > $S/$n.txt 2>&1; echo done > $S/$n.done) > /dev/null 2>&1 &
