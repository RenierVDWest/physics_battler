# shake.py <session>/units [mm] [reversals] [window steps]: how long each part shakes: its place against the pelvis going back and forth
# (its step-to-step move turning back on itself, each move over <mm>, at least <reversals> times in <window> steps), for the body and for the pose
import sys, gzip, csv, math, glob, os
d=sys.argv[1]; mm=float(sys.argv[2]) if len(sys.argv)>2 else 3; need=int(sys.argv[3]) if len(sys.argv)>3 else 3; win=int(sys.argv[4]) if len(sys.argv)>4 else 8
parts=['Spine','Chest','Head','UpperArmL','ForearmL','HandL','UpperArmR','ForearmR','HandR','ThighL','ShinL','FootL','ThighR','ShinR','FootR']
extra=[('tip','ghostTip')]
def sub(a,b): return tuple(x-y for x,y in zip(a,b))
def dot(a,b): return sum(x*y for x,y in zip(a,b))
tot={}
detail=[]
for f in sorted(glob.glob(d+'/*.pose.csv.gz')):
    rows=list(csv.DictReader(gzip.open(f,'rt')))
    name=os.path.basename(f).split('.')[0]
    def series(col, g):
        out=[]
        for r in rows:
            try:
                if g=='g': p=tuple(float(r[col+'.g'+a]) for a in 'xyz'); o=tuple(float(r['Pelvis.g'+a]) for a in 'xyz')
                elif g=='': p=tuple(float(r[col+'.'+a]) for a in 'xyz'); o=tuple(float(r['Pelvis.'+a]) for a in 'xyz')
                else: p=tuple(float(r[col+'.'+a]) for a in 'xyz'); o=tuple(float(r['Pelvis.'+(g if g=='g' else '')+a]) for a in 'xyz')
                out.append((float(r['t']),sub(p,o)))
            except Exception: out.append(None)
        return out
    def shaking(ser):
        rev=[0]*len(ser)
        for i in range(2,len(ser)):
            if ser[i] is None or ser[i-1] is None or ser[i-2] is None: continue
            a=sub(ser[i][1],ser[i-1][1]); b=sub(ser[i-1][1],ser[i-2][1])
            la=math.sqrt(dot(a,a)); lb=math.sqrt(dot(b,b))
            if la>mm/1000 and lb>mm/1000 and dot(a,b)<-0.5*la*lb: rev[i]=1
        on=[0]*len(ser); run=0
        for i in range(len(ser)):
            lo=max(0,i-win+1)
            if sum(rev[lo:i+1])>=need:
                for k in range(lo,i+1): on[k]=1
        return on
    line=[]
    for p in parts+['tip']:
        try:
            if p=='tip':
                b=[]; g=[]
                for r in rows:
                    try:
                        b.append((float(r['t']),sub(tuple(float(r['tip.'+a]) for a in 'xyz'),tuple(float(r['Pelvis.'+a]) for a in 'xyz'))))
                        g.append((float(r['t']),sub(tuple(float(r['ghostTip.'+a]) for a in 'xyz'),tuple(float(r['Pelvis.g'+a]) for a in 'xyz'))))
                    except Exception: b.append(None); g.append(None)
            else:
                b=series(p,''); g=series(p,'g')
            ob=shaking(b); og=shaking(g)
        except KeyError: continue
        sb=sum(ob)/60; sg=sum(og)/60; both=sum(1 for x,y in zip(ob,og) if x and y)/60
        tot.setdefault(p,[0,0,0]); tot[p][0]+=sb; tot[p][1]+=sg; tot[p][2]+=both
        if sb>0.3 or sg>0.3: line.append(f'{p} {sb:.1f}/{sg:.1f}')
        # episodes for the body
        i=0
        while i<len(ob):
            if ob[i]:
                j=i
                while j<len(ob) and ob[j]: j+=1
                if b[i]: detail.append((j-i,name,p,b[i][0],b[j-1][0] if b[j-1] else 0, sum(og[i:j])/(j-i)))
                i=j
            else: i+=1
    print(f'{name[:16]:16} seconds shaking, body/pose: '+', '.join(line))
print('all units, seconds shaking: body / pose / both at once')
for p in parts+['tip']:
    if p in tot: print(f'  {p:10} {tot[p][0]:6.1f} / {tot[p][1]:6.1f} / {tot[p][2]:6.1f}')
detail.sort(reverse=True)
print('longest body shakes:')
for n,name,p,t0,t1,share in detail[:25]: print(f'  {name[:16]:16} {p:10} {t0:6.2f}-{t1:6.2f} s ({n/60:.2f} s), the pose shaking {share*100:.0f}% of it')
