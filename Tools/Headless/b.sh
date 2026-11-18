#!/bin/bash
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
# b.sh : builds the four projects; refuses while a fight runs
[ "$(pgrep -f PhysicsBattler.Tests.dll | wc -l)" != "0" ] && { echo "A FIGHT IS RUNNING"; exit 1; }
cd "$H/../CompileCheck" && for p in Editor.csproj "Runtime.csproj -p:LegacyInput=true" Runtime.csproj Tests.csproj; do timeout 100 dotnet build $p -v q 2>&1 | grep -E "error|rror\(s\)" | sort -u; done
