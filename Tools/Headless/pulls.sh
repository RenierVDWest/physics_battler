H="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"; S="${S:-/tmp/pb}"; mkdir -p "$S"
cd "$H/../CompileCheck"
LOCO_TRACE=1 timeout 600 dotnet bin/Tests/Debug/PhysicsBattler.Tests.dll locolab $S/lab ${1:-200} > $S/lab.txt 2>&1
grep -v "released\|foot moved\|^  *[0-9]*: " $S/lab.txt | tail -6
python3 - <<'PY'
import re,collections
d=collections.defaultdict(list)
for l in open('$S/lab.txt'):
    m=re.match(r'\s+(.*?): (L|R) released.*pull ([0-9.]+) cm',l)
    if m: d[m.group(1)].append(float(m.group(3)))
for k,v in d.items(): v.sort(); print(f'{k:28s} n {len(v):3d} median {v[len(v)//2]:5.1f} max {v[-1]:5.1f}')
PY
