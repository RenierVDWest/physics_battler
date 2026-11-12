# rolls.py <session>/units [deg a step] [steps]: runs where an upper arm keeps turning fast against the chest: how much of the turn is about
# its own length (ROLL: over 80%), the elbow's bend (the body's and the pose's, degrees), how far the hand is off its pose, the clip mode
import sys, gzip, csv, math, glob, os
d=sys.argv[1]; thr=float(sys.argv[2]) if len(sys.argv)>2 else 25; need=int(sys.argv[3]) if len(sys.argv)>3 else 4
def qmul(a,b):
    ax,ay,az,aw=a; bx,by,bz,bw=b
    return (aw*bx+ax*bw+ay*bz-az*by, aw*by-ax*bz+ay*bw+az*bx, aw*bz+ax*by-ay*bx+az*bw, aw*bw-ax*bx-ay*by-az*bz)
def qinv(a): return (-a[0],-a[1],-a[2],a[3])
def rot(q,v):
    r=qmul(qmul(q,(v[0],v[1],v[2],0)),qinv(q)); return r[:3]
def tv(q):
    if q[3]<0: q=tuple(-x for x in q)
    s=math.sqrt(q[0]**2+q[1]**2+q[2]**2)
    if s<1e-9: return (0,0,0)
    a=2*math.degrees(math.atan2(s,q[3])); return tuple(x/s*a for x in q[:3])
def sub(a,b): return tuple(x-y for x,y in zip(a,b))
def nrm(a):
    n=math.sqrt(sum(x*x for x in a)) or 1; return tuple(x/n for x in a)
def dot(a,b): return sum(x*y for x,y in zip(a,b))
def bend(a,b,c):
    u=nrm(sub(b,a)); v=nrm(sub(c,b)); return math.degrees(math.acos(max(-1,min(1,dot(u,v)))))
tot=0; rolls=0
for f in sorted(glob.glob(d+'/*.pose.csv.gz')):
    rows=list(csv.DictReader(gzip.open(f,'rt')))
    fr={}
    try:
        for r in csv.DictReader(gzip.open(f.replace('.pose.','.frames.'),'rt')): fr[round(float(r['t']),3)]=r
    except Exception: pass
    def Q(r,p): return tuple(float(r[p+'.q'+a]) for a in 'xyzw')
    def P(r,p,g=''): return tuple(float(r[p+'.'+g+a]) for a in 'xyz')
    for s in 'LR':
        c,p,n='UpperArm'+s,'Chest','Forearm'+s; h='Hand'+s
        run=[]; prev=None
        def flush():
            global tot, rolls
            if len(run)>=need:
                net=[sum(x[1][i] for x in run) for i in range(3)]
                allt=sum(math.sqrt(sum(v*v for v in x[1])) for x in run)
                about=sum(abs(x[2]) for x in run)
                pb=sum(x[3] for x in run)/len(run); gb=sum(x[4] for x in run)/len(run); off=max(x[5] for x in run)
                m=fr.get(round(run[0][0],3),{}).get('clipMode','?')
                tot+=1; isroll=about>0.8*allt; rolls+=isroll
                print(f'{os.path.basename(f)[:14]:14} {c:10} {run[0][0]:6.2f}-{run[-1][0]:6.2f} s {len(run):3} steps  turned {allt:5.0f}°, {about/allt*100:3.0f}% about its length  elbow bend {pb:3.0f}° (pose {gb:3.0f}°)  hand off {off*100:3.0f} cm  mode {m} {"ROLL" if isroll else ""}')
        for r in rows:
            try:
                if prev is not None:
                    a=qmul(qinv(Q(r,p)),Q(r,c)); b=qmul(qinv(Q(prev,p)),Q(prev,c))
                    dv=tv(qmul(qinv(b),a)); m=math.sqrt(sum(x*x for x in dv))   # on the part's own axes
                    ln=nrm(rot(qinv(Q(r,c)), sub(P(r,n),P(r,c))))
                    if m>thr: run.append((float(r['t']),dv,dot(dv,ln), bend(P(r,c),P(r,n),P(r,h)), bend(P(r,c,'g'),P(r,n,'g'),P(r,h,'g')), math.dist(P(r,h),P(r,h,'g'))))
                    else: flush(); run=[]
            except Exception as e: pass
            prev=r
        flush()
print(f'{tot} runs, {rolls} mostly about the arm\'s own length')
