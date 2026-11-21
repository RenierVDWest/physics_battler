#!/bin/bash
# wait.sh name... : waits for the runs of those names to end (their .done files), at most 110 s so that it fits one foreground command; says "still running" if they have not. Use it before b.sh and before reading a run's file.
S="${S:-/tmp/pb}"
for i in $(seq 1 22); do
  ok=1; for n in "$@"; do [ -f "$S/$n.done" ] || ok=0; done
  [ $ok = 1 ] && exit 0
  sleep 5
done
echo "still running"; exit 1
