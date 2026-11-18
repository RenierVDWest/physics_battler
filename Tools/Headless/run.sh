#!/bin/bash
# run.sh name ENV=... : a tubes-lineup run in the background
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
n=$1; shift; rm -f "${S:?}"/$n.done; rm -rf "${S:?}/$n"
cd "$H/../.."
(env PHYSX=1 FIGHT_PHYSICAL=1 FIGHT_LINEUP=tubes "$@" timeout ${RUNFOR:-115} dotnet Tools/CompileCheck/bin/Tests/Debug/PhysicsBattler.Tests.dll fight $S/$n 7 > $S/$n.txt 2>&1; echo done > $S/$n.done) > /dev/null 2>&1 &
