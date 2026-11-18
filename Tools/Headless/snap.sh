#!/bin/bash
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
# snap.sh <outdir>: the clip fight's logs for three seeds (posed, and on the real engine), and the engine's own checks
out="$1"; mkdir -p "$out"; cd "$H/../.."
run() { # name env...
  name="$1"; shift
  d=$(mktemp -d)
  env "$@" timeout 900 dotnet Tools/CompileCheck/bin/Tests/Debug/PhysicsBattler.Tests.dll fight "$d" "$SEED" > "$out/$name.txt" 2>&1
  f=$(find "$d" -name events.log | head -1)
  if [ -n "$f" ]; then tail -n +2 "$f" > "$out/$name.ev"; else echo "NO LOG" > "$out/$name.ev"; fi
  grep -v "fight session:" "$out/$name.txt" > "$out/$name.out"; rm -f "$out/$name.txt"
  rm -rf "$d"
}
for SEED in 1 2 3; do
  run clip$SEED CLIP_FIGHT=1 FIGHT_SECONDS=60 &
  run med$SEED CLIP_FIGHT=1 FIGHT_LINEUP=mediums FIGHT_SECONDS=40 &
done
wait
SEED=1 run physx1 PHYSX=1 CLIP_FIGHT=1 FIGHT_LINEUP=mediums FIGHT_SECONDS=30 &
SEED=2 run physx2 PHYSX=1 PHYSX_SUBSTEPS=4 CLIP_FIGHT=1 FIGHT_LINEUP=duel FIGHT_SECONDS=20 &
timeout 600 dotnet Tools/CompileCheck/bin/Tests/Debug/PhysicsBattler.Tests.dll physx > "$out/physx.out" 2>&1 &
ARM_SETUP="limited strength (shoulder 600 N m, elbow 420, wrist 150), 240" ARM_BLADE=30 timeout 900 dotnet Tools/CompileCheck/bin/Tests/Debug/PhysicsBattler.Tests.dll physx-arm > "$out/arm.out" 2>&1 &
wait
cd "$out" && md5sum *.ev *.out > sums.txt; wc -l *.ev | tail -1
