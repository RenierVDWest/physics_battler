# How the fight clumps: for each moment, the biggest knot of standing, living units (each within 1.3 m of the next).
# python3 Tools/Playtest/clump.py PlaytestLogs/<session> [...]
import gzip,sys,glob,csv,math
def load(d):
    U={}
    for f in sorted(glob.glob(d+'/units/*.frames.csv.gz')):
        r=list(csv.DictReader(gzip.open(f,'rt')))
        U[f.split('/')[-1].split('.')[0]]={round(float(x['t']),3):(float(x['rootX']),float(x['rootZ']),float(x.get('pelvisH',1))) for x in r}
    return U
for d in sys.argv[1:]:
    U=load(d); names=list(U)
    dead={}
    for line in open(d+'/events.log',encoding='utf-8'):
        parts=[x.strip() for x in line.split('|')]
        if len(parts)>=4 and parts[3]=='death':
            dead[parts[2].replace(' ','_')]=float(parts[0])
    ts=sorted(set.intersection(*[set(v) for v in U.values()])) if U else []
    hist={}
    for t in ts:
        pts=[(n,U[n][t]) for n in names if U[n][t][2]>0.4 and t < dead.get(n[3:],1e9)]  # standing and alive
        # connected components with link < 1.3
        seen=set(); best=0
        for i,(n,p) in enumerate(pts):
            if n in seen: continue
            stack=[n]; comp=0; seen.add(n)
            while stack:
                a=stack.pop(); comp+=1; pa=U[a][t]
                for m,q in pts:
                    if m not in seen and math.hypot(pa[0]-q[0],pa[1]-q[1])<1.3: seen.add(m); stack.append(m)
            best=max(best,comp)
        hist[best]=hist.get(best,0)+1
    tot=sum(hist.values())
    if not tot: continue
    print(d.split('/')[-1], ' '.join(f"knot of {k}: {100*v/tot:.0f}%" for k,v in sorted(hist.items())))
