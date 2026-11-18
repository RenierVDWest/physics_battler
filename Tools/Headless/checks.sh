#!/bin/bash
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
cd "$H/../.."
rm -f $S/checks.done
# 1. the battle's bodies (a Large with its club, a Small) in the four falls
$H/held.sh cb FIGHT_TARGETS=MC > $S/cb.out 2>&1
# 2. twelve bodies sent down together (cost), and three for the same
RUNFOR=560 $H/run.sh rows4 FIGHT_SECONDS=8.2 FIGHT_TUBEROWS=4 FIGHT_TUBE="3,200,8,1.3,0.15,180,1,1"
until [ -f $S/rows4.done ]; do sleep 5; done
# 3. sixteen in a fight
rm -rf "${S:?}/crowd"; env PHYSX=1 FIGHT_PHYSICAL=1 FIGHT_SECONDS=15 FIGHT_LINEUP=crowd timeout 900 dotnet Tools/CompileCheck/bin/Tests/Debug/PhysicsBattler.Tests.dll fight $S/crowd 5 > $S/crowd.txt 2>&1
# 4. the six, a minute
rm -rf "${S:?}/reg"; env PHYSX=1 FIGHT_PHYSICAL=1 FIGHT_SECONDS=60 timeout 1200 dotnet Tools/CompileCheck/bin/Tests/Debug/PhysicsBattler.Tests.dll fight $S/reg 5 > $S/reg.txt 2>&1
echo done > $S/checks.done
