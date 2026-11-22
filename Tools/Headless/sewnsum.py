#!/usr/bin/env python3
"""sewnsum.py log...: sums the moving sewn clothes of headless runs (FIGHT_SEWNTRACE=1 lines): standing 2 to 4 s, the
leaps of "Shake him", shiver, the longest thread, the cost, and the SEWNLONG thread counts."""
import sys, re
def rd(path):
    tr={}; lg={}
    for line in open(path,errors='replace'):
        if line.startswith('SEWNTRACE'):
            f=line.split(); t=float(f[1]); who=f[2]
            tr.setdefault(who,[]).append(dict(t=t,off=float(f[6]),ends=float(f[8]),hang=float(f[10]),shiv=float(f[12]),str=float(f[14]),ms=float(f[16])))
        elif line.startswith('SEWNLONG'):
            m=re.match(r'SEWNLONG (\S+) (\S+) over 105%: (\d+) of (\d+), over 120%: (\d+); \S+ (\d+)%',line)
            if m: lg.setdefault(m.group(2),[]).append((float(m.group(1)),int(m.group(3)),int(m.group(5)),int(m.group(6))))
    return tr,lg
for path in sys.argv[1:]:
    tr,lg=rd(path)
    print('==',path)
    for who in ('mover','other'):
        T=tr.get(who,[])
        if not T: continue
        base=[r for r in T if 2<=r['t']<4]
        b=lambda k: sum(r[k] for r in base)/max(1,len(base))
        def mx(a,z,k): 
            w=[r[k] for r in T if a<=r['t']<z]; return max(w) if w else 0
        # settle after leap out at 8.2: first t>=8.2 after the peak where shiver stays < 1 mm for 3 samples
        settle=None
        for a,(lo,hi) in (('in',(6.0,6.8)),('out',(8.2,9.0))): pass
        def settle_after(t0,t1):
            w=[r for r in T if t0<=r['t']<t1]
            if not w: return None
            pk=max(range(len(w)),key=lambda i:w[i]['ends'])
            for i in range(pk,len(w)-2):
                if all(w[j]['shiv']<1.0 for j in range(i,i+3)): return w[i]['t']-w[pk]['t']
            return None
        L=lg.get(who,[])
        Ls=[x for x in L if 2<=x[0]<4.5]
        print(f"{who}: still off {b('off'):.2f} hems {b('ends'):.2f} hang {b('hang'):.2f} cm shiver {b('shiv'):.2f} mm | leap in hems max {mx(6.0,6.8,'ends'):.1f} out {mx(8.2,9.0,'ends'):.1f} cm, settled {settle_after(8.2,9.0)} s after peak | shiver max {mx(0.5,99,'shiv'):.1f} | str still {b('str'):.0f}% max {mx(0.5,99,'str'):.0f}% | ms {sum(r['ms'] for r in T)/len(T):.1f}")
        if Ls: print(f"   still: over105 {sum(x[1] for x in Ls)/len(Ls):.0f} over120 {sum(x[2] for x in Ls)/len(Ls):.0f} of 19690; all: over105 mean {sum(x[1] for x in L)/len(L):.0f} max {max(x[1] for x in L)}, over120 mean {sum(x[2] for x in L)/len(L):.0f}")
    for line in open(path,errors='replace'):
        if line.startswith('SEWN '): print('  ',line[:230].strip())
