# armrange.py <session>/units [past, degrees]: each arm read as a body's arm (the same reading as the headless fight
# report's, Tools/CompileCheck/Runner/ArmAnatomy.cs): where the elbow is against the chest, how the upper arm is
# turned about its length (which way the elbow folds), the elbow's bend, the forearm's roll, the wrist. For the
# physical body (all five) and the pose (the first three: the log has the pose's places, not its turns), the share
# of the session each is past a person's range by more than [past] (10), and the longest runs, to find on the film.
import sys, gzip, csv, math, glob, os
d=sys.argv[1]; thr=float(sys.argv[2]) if len(sys.argv)>2 else 10
def qmul(a,b):
    ax,ay,az,aw=a; bx,by,bz,bw=b
    return (aw*bx+ax*bw+ay*bz-az*by, aw*by-ax*bz+ay*bw+az*bx, aw*bz+ax*by-ay*bx+az*bw, aw*bw-ax*bx-ay*by-az*bz)
def qinv(a): return (-a[0],-a[1],-a[2],a[3])
def rot(q,v): return qmul(qmul(q,(v[0],v[1],v[2],0)),qinv(q))[:3]
def sub(a,b): return tuple(x-y for x,y in zip(a,b))
def add(a,b): return tuple(x+y for x,y in zip(a,b))
def mul(a,k): return tuple(x*k for x in a)
def dot(a,b): return sum(x*y for x,y in zip(a,b))
def cross(a,b): return (a[1]*b[2]-a[2]*b[1], a[2]*b[0]-a[0]*b[2], a[0]*b[1]-a[1]*b[0])
def nrm(a):
    n=math.sqrt(dot(a,a)) or 1; return tuple(x/n for x in a)
def ang(a,b): return math.degrees(math.acos(max(-1,min(1,dot(nrm(a),nrm(b))))))
def perp(v,n): return sub(v,mul(n,dot(v,n)))
def fromto(a,b):   # the short turn from a to b (unit vectors)
    c=cross(a,b); w=1+dot(a,b)
    if w<1e-6: return (1,0,0,0)
    n=math.sqrt(dot(c,c)+w*w); return (c[0]/n,c[1]/n,c[2]/n,w/n)
TOWARD=[-180,-150,-120,-90,-60,-40,-20,0,100,120,140,160,180]; HIGH=[55,35,30,40,75,110,160,180,180,140,100,70,55]
def highest(t):
    for i in range(1,len(TOWARD)):
        if t<=TOWARD[i]:
            k=(t-TOWARD[i-1])/(TOWARD[i]-TOWARD[i-1]); return HIGH[i-1]+(HIGH[i]-HIGH[i-1])*k
    return HIGH[-1]
def read(chest,S,E,W,hand,left):
    back=qinv(chest)
    M=(lambda v:(-v[0],v[1],v[2])) if left else (lambda v:v)
    u=nrm(M(rot(back,sub(E,S)))); f=nrm(M(rot(back,sub(W,E))))
    raised=math.degrees(math.acos(max(-1,min(1,-u[1])))); toward=math.degrees(math.atan2(u[0],u[2]))
    reach=0
    if raised>30 and raised>highest(toward):
        reach=180
        for t in range(-180,180,10):
            r=math.radians(min(raised,highest(t))); tr=math.radians(t)
            reach=min(reach,ang(u,(math.sin(r)*math.sin(tr),-math.cos(r),math.sin(r)*math.cos(tr))))
    bend=ang(u,f); pbend=max(0,bend-150)
    sw=fromto((0,-1,0),u); ahead=rot(sw,(0,0,1)); inward=rot(sw,(-1,0,0)); fold=perp(f,u)
    folded=20<bend<175 and raised<165
    turn=0; pturn=0; roll=0; proll=0; pwrist=0
    if folded:
        turn=math.degrees(math.atan2(dot(fold,inward),dot(fold,ahead))); pturn=max(0,turn-100,-90-turn)
    if hand is not None:
        fingers=M(rot(back,rot(hand,(0,-1,0)))); thumb=M(rot(back,rot(hand,(0,0,1)))); palm=M(rot(back,rot(hand,(1,0,0) if left else (-1,0,0))))
        flex=-math.degrees(math.atan2(dot(f,palm),dot(f,fingers))); tw=-math.degrees(math.asin(max(-1,min(1,dot(f,thumb)))))
        pwrist=max(0,abs(flex)-80,tw-30,-45-tw)
        if folded:
            up=nrm(perp(mul(u,-1),f)); t=perp(thumb,f); med=cross(f,up)
            roll=math.degrees(math.atan2(dot(t,med),dot(t,up))); proll=max(0,abs(roll)-90)
    return (reach,pturn,pbend,proll,pwrist),(raised,toward,turn,bend,roll)
WHAT=['elbow\'s place','upper arm\'s turn','elbow\'s bend','forearm\'s roll','wrist']
for f in sorted(glob.glob(d+'/*.pose.csv.gz')):
    rows=list(csv.DictReader(gzip.open(f,'rt')))
    def Q(r,p): return tuple(float(r[p+'.q'+a]) for a in 'xyzw')
    def P(r,p,g=''): return tuple(float(r[p+'.'+g+a]) for a in 'xyz')
    name=os.path.basename(f).replace('.pose.csv.gz','')
    for s in 'LR':
        n=0; body=[0]*6; pose=[0]*4; worst=[0]*5; pworst=[0]*3; runs=[]; run=None
        for r in rows:
            try:
                c=Q(r,'Chest'); b,how=read(c,P(r,'UpperArm'+s),P(r,'Forearm'+s),P(r,'Hand'+s),Q(r,'Hand'+s),s=='L')
                p,phow=read(c,P(r,'UpperArm'+s,'g'),P(r,'Forearm'+s,'g'),P(r,'Hand'+s,'g'),None,s=='L')
            except Exception: continue
            n+=1
            for i in range(5):
                body[i]+=b[i]>thr; worst[i]=max(worst[i],b[i])
            for i in range(3):
                pose[i]+=p[i]>thr; pworst[i]=max(pworst[i],p[i])
            body[5]+=max(b)>thr; pose[3]+=max(p[:3])>thr
            t=float(r['t'])
            if max(b)>30:
                i=max(range(5),key=lambda k:b[k])
                if run and t-run[1]<0.1: run[1]=t; run[2]=max(run[2],max(b)); run[3].add(WHAT[i])
                else:
                    run=[t,t,max(b),{WHAT[i]},how]; runs.append(run)
        if n==0: continue
        parts='; '.join(f'{WHAT[i]} {100*body[i]/n:.1f}% (worst {worst[i]:.0f}°' + (f', pose {100*pose[i]/n:.1f}%, {pworst[i]:.0f}°' if i<3 else '') + ')' for i in range(5) if worst[i]>thr)
        print(f'{name:18} {"left" if s=="L" else "right"} arm ({n/60:.0f} s): body {100*body[5]/n:.1f}% past, pose (place, turn, bend) {100*pose[3]/n:.1f}%: {parts}')
        for a in sorted(runs,key=lambda x:-(x[1]-x[0]))[:3]:
            if a[1]-a[0]>=0.15:
                h=a[4]; print(f'      {a[0]:6.2f}-{a[1]:6.2f} s: up to {a[2]:.0f}° past ({", ".join(sorted(a[3]))}); at its start raised {h[0]:.0f}° toward {h[1]:.0f}°, turned {h[2]:.0f}, bent {h[3]:.0f}, rolled {h[4]:.0f}')
