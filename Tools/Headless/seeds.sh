#!/bin/bash
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
cd "$H/../.."
rm -f $S/seeds_all.done
for seed in 6 7 8; do
  SEED=$seed $H/reg.sh sd_new_$seed FIGHT_MOVEMADE=0; SEED=$seed $H/reg.sh sd_old_$seed FIGHT_MOVEMADE=0 FIGHT_SHIELDCOVER=0 FIGHT_BASH=0
  until [ -f $S/sd_new_$seed.done ] && [ -f $S/sd_old_$seed.done ]; do sleep 5; done
done
echo all > $S/seeds_all.done
