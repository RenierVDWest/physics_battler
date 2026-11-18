#!/bin/bash
# stum.sh ENV... : the stumble model by itself (no fighter): thrown at twelve speeds from five sides; STUMBLE_FOOT=5.5 and the like set its levers
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$H/../.." && env "$@" dotnet Tools/CompileCheck/bin/Tests/Debug/PhysicsBattler.Tests.dll stumble 2>&1 | grep STUMBLE
