#!/bin/bash
H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
cd "$H/../.."
for seed in 5 6 7; do
  SEED=$seed SECS=40 $H/dance.sh calm_$seed FIGHT_DANCE="Plain,Plain"
  SEED=$seed SECS=40 $H/dance.sh rest_$seed FIGHT_DANCE="Plain,Plain" FIGHT_DANCECALM=0
  until [ -f $S/dance_calm_$seed.done ] && [ -f $S/dance_rest_$seed.done ]; do sleep 5; done
done
for seed in 5 6 7; do
  SEED=$seed SECS=40 $H/dance.sh old_$seed FIGHT_DANCE="Plain,Plain" FIGHT_DANCECALM=0 FIGHT_WHOLESTEPS=0 FIGHT_LEGSTURNED=0
done
until [ -f $S/dance_old_5.done ] && [ -f $S/dance_old_6.done ] && [ -f $S/dance_old_7.done ]; do sleep 5; done
echo all > $S/calm6.done
