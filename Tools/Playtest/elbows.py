# elbows.py <units dir> [elbow cm] [hand cm]: steps where the pose's elbow jumps (against its shoulder) while its hand hardly moves: the elbow flung to the other side of the arm
import sys, gzip, csv, math, glob, os
d=sys.argv[1]; need=float(sys.argv[2]) if len(sys.argv)>2 else 20; most=float(sys.argv[3]) if len(sys.argv)>3 else 12
tot=0
for f in sorted(glob.glob(d+'/*.pose.csv.gz')):
    rows=list(csv.DictReader(gzip.open(f,'rt')))
    fr={}
    try:
        for r in csv.DictReader(gzip.open(f.replace('.pose.','.frames.'),'rt')): fr[round(float(r['t']),3)]=r
    except Exception: pass
    def P(r,p): return tuple(float(r[p+'.g'+a]) for a in 'xyz')
    for s in 'LR':
        prev=None; n=0; first=[]
        for r in rows:
            try:
                sh=P(r,'UpperArm'+s); el=tuple(a-b for a,b in zip(P(r,'Forearm'+s),sh)); hd=tuple(a-b for a,b in zip(P(r,'Hand'+s),sh))
            except Exception: prev=None; continue
            if prev and float(r['t'])-prev[2]<0.03:
                e=math.dist(el,prev[0])*100; h=math.dist(hd,prev[1])*100
                if e>need and h<most:
                    n+=1
                    if len(first)<4: first.append(f"{float(r['t']):.2f} s ({e:.0f} cm, hand {h:.0f}, mode {fr.get(round(float(r['t']),3),{}).get('clipMode','?')})")
            prev=(el,hd,float(r['t']))
        tot+=n
        if n: print(f'{os.path.basename(f)[:16]:16} arm {s}: {n} elbow jumps; first at '+', '.join(first))
print(tot,'in all')
