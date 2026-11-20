#!/bin/bash
# flow.sh [all|raw|raw speeds] : the recorded cuts' blades (the combos plan's step 1): the map of corners and every pair's join. Quick, in the foreground; kept in $S/flow.txt. Ends "FLOW OK" when the performer's own joins come out best.
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
cd "$H/../.."
timeout 100 dotnet Tools/CompileCheck/bin/Tests/Debug/PhysicsBattler.Tests.dll flow "$@" > $S/flow.txt 2>&1
sed -n '1,/^THE JOINS/p' $S/flow.txt | cut -c1-200; sed -n "/^THE PERFORMERS' OWN/,\$p" $S/flow.txt | cut -c1-330
echo "(the whole report: $S/flow.txt)"
