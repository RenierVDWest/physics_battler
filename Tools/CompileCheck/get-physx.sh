#!/bin/sh
# Fetches the real PhysX engine for the headless runner: Cysharp's MagicPhysX (.NET bindings for NVIDIA PhysX 5, MIT),
# at the commit the runner was written against. Kept out of the repository (ThirdParty/ is ignored): run this once on a
# new machine, then build Tests.csproj; without it the runner builds as before, on the dry run with no collisions.
set -e
cd "$(dirname "$0")"
mkdir -p ThirdParty
if [ ! -d ThirdParty/MagicPhysX ]; then
  git clone -q https://github.com/Cysharp/MagicPhysX.git ThirdParty/MagicPhysX
fi
git -C ThirdParty/MagicPhysX checkout -q c320362da782c9f5815d77b78bd7980acf40e7d3
echo "MagicPhysX at $(git -C ThirdParty/MagicPhysX rev-parse --short HEAD)"
