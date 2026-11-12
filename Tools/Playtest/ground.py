# ground.py <session dir> [unit filter]: how the feet meet the ground (planted, sliding, hovering, stepping) and whether the body is balanced over them
import sys, gzip, csv, math, glob, os, collections
d=sys.argv[1]; only=sys.argv[2] if len(sys.argv)>2 else ''
M={'Pelvis':11.429,'Spine':9.796,'Chest':16.327,'Head':6.531,'UpperArmR':2.449,'ForearmR':1.469,'HandR':0.816,'UpperArmL':2.449,'ForearmL':1.469,'HandL':0.816,'ThighR':8.163,'ShinR':3.755,'FootR':1.306,'ThighL':8.163,'ShinL':3.755,'FootL':1.306}
CH={'Pelvis':'Spine','Spine':'Chest','Chest':'Head','UpperArmR':'ForearmR','ForearmR':'HandR','UpperArmL':'ForearmL','ForearmL':'HandL','ThighR':'ShinR','ShinR':'FootR','ThighL':'ShinL','ShinL':'FootL'}
def rot(q,v):
    x,y,z,w=q; vx,vy,vz=v
    tx=2*(y*vz-z*vy); ty=2*(z*vx-x*vz); tz=2*(x*vy-y*vx)
    return (vx+w*tx+(y*tz-z*ty), vy+w*ty+(z*tx-x*tz), vz+w*tz+(x*ty-y*tx))
def add(a,b): return tuple(x+y for x,y in zip(a,b))
def hull(pts):
    pts=sorted(set(pts))
    if len(pts)<3: return pts
    def cross(o,a,b): return (a[0]-o[0])*(b[1]-o[1])-(a[1]-o[1])*(b[0]-o[0])
    lo=[]
    for p in pts:
        while len(lo)>=2 and cross(lo[-2],lo[-1],p)<=0: lo.pop()
        lo.append(p)
    up=[]
    for p in reversed(pts):
        while len(up)>=2 and cross(up[-2],up[-1],p)<=0: up.pop()
        up.append(p)
    return lo[:-1]+up[:-1]
def outside(h,p):
    n=len(h)
    if n==0: return float('nan')
    inside=True; best=1e9
    for i in range(n):
        a=h[i]; b=h[(i+1)%n]
        ex,ey=b[0]-a[0],b[1]-a[1]; px,py=p[0]-a[0],p[1]-a[1]
        if n>=3 and ex*py-ey*px<0: inside=False
        L=ex*ex+ey*ey; t=max(0,min(1,(px*ex+py*ey)/L)) if L>0 else 0
        best=min(best,math.hypot(px-t*ex,py-t*ey))
    return 0.0 if (inside and n>=3) else best
tot=collections.Counter(); runs=[]; standout=[]; xout=[]; steps=[]; lean=[]
for f in sorted(glob.glob(d+'/units/*.pose.csv.gz')):
    if only and only not in f: continue
    rows=[r for r in csv.DictReader(gzip.open(f,'rt')) if r['Pelvis.y'] not in ('','NaN')]
    fr={r['t']:r for r in csv.DictReader(gzip.open(f.replace('.pose.','.frames.'),'rt'))}
    name=os.path.basename(f)[3:].split('.')[0]
    P=lambda r,p: (float(r[p+'.x']),float(r[p+'.y']),float(r[p+'.z']))
    Q=lambda r,p: (float(r[p+'.qx']),float(r[p+'.qy']),float(r[p+'.qz']),float(r[p+'.qw']))
    up=[r for r in rows if float(r['Pelvis.y'])>0.62]
    if len(up)<60: continue
    stand={}
    for s in 'LR':
        ys=sorted(float(r['Foot'+s+'.y']) for r in up); stand[s]=ys[len(ys)//20]
    prev=None; pcom=None; run={s:None for s in 'LR'}; air={s:None for s in 'LR'}
    for r in rows:
        if float(r['Pelvis.y'])<=0.62: prev=None; pcom=None; run={s:None for s in 'LR'}; air={s:None for s in 'LR'}; continue
        try: pos={p:P(r,p) for p in M}
        except Exception: prev=None; continue
        t=float(r['t']); frm=fr.get(r['t'],{}); mode=frm.get('clipMode','?')
        com=[0,0,0]; m=0
        for p,mass in M.items():
            c=pos[p]
            if p in CH: c=tuple((a+b)/2 for a,b in zip(pos[p],pos[CH[p]]))
            elif p=='Head': c=(c[0],c[1]+0.1,c[2])
            for i in range(3): com[i]+=mass*c[i]
            m+=mass
        com=[c/m for c in com]
        foot={}
        for s in 'LR':
            q=Q(r,'Foot'+s); a=pos['Foot'+s]; h=stand[s]
            heel=add(a,rot(q,(0,-(h-0.015),-0.02))); toe=add(a,rot(q,(0,-(h-0.015),0.10)))
            low=min(heel[1],toe[1])
            touch=low<0.02
            cp=a if (heel[1]<0.015 and toe[1]<0.015) else (toe if toe[1]<heel[1] else heel)
            foot[s]=(touch,cp,low,heel,toe,q)
        if prev is not None:
            dt=t-prev[0]
            if 0<dt<0.05:
                tot['steps']+=1
                vcom=[(com[i]-pcom[i])/dt for i in range(3)]
                for s in 'LR':
                    touch,cp,low,heel,toe,q=foot[s]; ptouch,pcp=prev[1][s][0],prev[1][s][1]
                    tot['foot']+=1; tot['foot'+mode]+=1
                    pheel,ptoe=prev[1][s][3],prev[1][s][4]
                    mv=0.0; on=False
                    # the same point of the sole, on the ground in both steps: the toe's, else the heel's
                    if toe[1]<0.02 and ptoe[1]<0.02: mv=math.hypot(toe[0]-ptoe[0],toe[2]-ptoe[2]); on=True
                    elif heel[1]<0.02 and pheel[1]<0.02: mv=math.hypot(heel[0]-pheel[0],heel[2]-pheel[2]); on=True
                    if on:
                        if mv/dt>0.10:
                            tot['sliding']+=1; tot['sliding'+mode]+=1; tot['slid_cm']+=mv*100; tot['slid_cm'+mode]+=mv*100
                            if mv/dt>0.5: tot['slidfast']+=1
                            if run[s] is None: run[s]=[t,0.0,mode]
                            run[s][1]+=mv*100
                        else:
                            tot['planted']+=1; tot['planted'+mode]+=1
                            if run[s] is not None: runs.append((run[s][1],name,s,run[s][0],run[s][2])); run[s]=None
                    else:
                        if run[s] is not None: runs.append((run[s][1],name,s,run[s][0],run[s][2])); run[s]=None
                        if not touch:
                            if low<0.09: tot['hover']+=1; tot['hover'+mode]+=1
                            else: tot['high']+=1; tot['high'+mode]+=1
                        else: tot['landing']+=1
                    # steps: from leaving the ground to touching again
                    if not touch:
                        if air[s] is None: air[s]=[t,prev[2]['Foot'+s],0.0]
                        air[s][2]=max(air[s][2],low)
                    elif air[s] is not None:
                        a=pos['Foot'+s]; b=air[s][1]
                        steps.append((t-air[s][0], math.hypot(a[0]-b[0],a[2]-b[2]), air[s][2], mode)); air[s]=None
                pts=[]
                for s in 'LR':
                    touch,cp,low,heel,toe,q=foot[s]
                    if touch:
                        fw=rot(q,(0,0,1)); L=math.hypot(fw[0],fw[2]) or 1; sx,sz=fw[2]/L,-fw[0]/L
                        for p in ((heel,) if heel[1]<0.02 else ())+((toe,) if toe[1]<0.02 else ()):
                            for side in (-0.05,0.05): pts.append((p[0]+sx*side,p[2]+sz*side))
                h=hull(pts); speed=math.hypot(vcom[0],vcom[2])
                both=foot['L'][0] and foot['R'][0]
                if len(h)>=2 and not both:
                    o1=outside(h,(com[0],com[2])); w=math.sqrt(max(0.3,com[1])/9.81)
                    x1=outside(h,(com[0]+vcom[0]*w,com[2]+vcom[2]*w))
                    if mode!='2': lean.append((o1,x1,speed))
                if len(h)>=3:
                    o=outside(h,(com[0],com[2])); w=math.sqrt(max(0.3,com[1])/9.81)
                    xo=outside(h,(com[0]+vcom[0]*w,com[2]+vcom[2]*w))
                    if both and speed<0.15: standout.append(o)
                    if both: xout.append((xo,mode,speed))
                k='both' if both else ('one' if (foot['L'][0] or foot['R'][0]) else 'none'); tot[k]+=1; tot[k+mode]+=1; tot['n'+mode]+=1
        prev=(t,foot,pos); pcom=com
T=tot['steps']/60; F=max(1,tot['foot'])
print(f"{T:.0f} unit-seconds on their feet")
print(f"a foot is: planted {100*tot['planted']/F:.0f}% of the time, on the ground and moving over 10 cm/s {100*tot['sliding']/F:.0f}% (over 50 cm/s {100*tot['slidfast']/F:.0f}%), in the air with its sole under 9 cm {100*tot['hover']/F:.0f}%, in the air higher {100*tot['high']/F:.0f}%")
print(f"the body has both feet on the ground {100*tot['both']/max(1,tot['steps']):.0f}% of the time, one {100*tot['one']/max(1,tot['steps']):.0f}%, none {100*tot['none']/max(1,tot['steps']):.0f}%")
for mode,label in (('0','standing'),('1','walking (the guard cycles blended)'),('2','in a clip (a strike, a reaction, a turn)')):
    Fm=max(1,tot['foot'+mode]); Nm=max(1,tot['n'+mode])
    print(f"  {label}, {100*tot['n'+mode]/max(1,tot['steps']):.0f}% of the time: a foot planted {100*tot['planted'+mode]/Fm:.0f}%, sliding {100*tot['sliding'+mode]/Fm:.0f}%, up under 9 cm {100*tot['hover'+mode]/Fm:.0f}%, up higher {100*tot['high'+mode]/Fm:.0f}%; both feet down {100*tot['both'+mode]/Nm:.0f}%, one {100*tot['one'+mode]/Nm:.0f}%, neither {100*tot['none'+mode]/Nm:.0f}%; slid {tot['slid_cm'+mode]/100/(Nm/60)*60:.1f} m a minute")
runs.sort(reverse=True)
print(f"sliding: {tot['slid_cm']/max(1,T)*60/100:.2f} m per unit-minute; slides over 5 cm: {sum(1 for x in runs if x[0]>5)} ({sum(1 for x in runs if x[0]>5)/max(1,T)*60:.1f} per unit-minute), over 10 cm: {sum(1 for x in runs if x[0]>10)}, over 20 cm: {sum(1 for x in runs if x[0]>20)}")
print('  over 5 cm by what was playing (0 standing, 1 walking, 2 a clip):',dict(collections.Counter(x[4] for x in runs if x[0]>5)))
print('  worst:',' | '.join(f'{x[0]:.0f} cm {x[1]} {x[2]} at {x[3]:.2f} s (mode {x[4]})' for x in runs[:5]))
if steps:
    n=len(steps); du=sorted(x[0] for x in steps); le=sorted(x[1] for x in steps); hi=sorted(x[2] for x in steps)
    print(f"steps: {n} ({n/max(1,T):.1f} a second per unit); time in the air: median {du[n//2]:.2f} s; length: median {le[n//2]*100:.0f} cm, a quarter under {le[n//4]*100:.0f} cm; highest point of the sole: median {hi[n//2]*100:.0f} cm")
    short=sum(1 for x in steps if x[1]<0.10); print(f"  steps shorter than 10 cm: {100*short/n:.0f}%; shorter than 5 cm: {100*sum(1 for x in steps if x[1]<0.05)/n:.0f}%")
    for mode,label in (('0','standing'),('1','walking'),('2','in a clip')):
        ss=[x for x in steps if x[3]==mode]
        if ss: print(f"  {label}: {len(ss)} steps, median length {sorted(x[1] for x in ss)[len(ss)//2]*100:.0f} cm")
so=sorted(standout)
if so: print(f"still on both feet ({len(so)/60:.0f} unit-seconds): centre of gravity outside the feet's support {100*sum(1 for x in so if x>0.0)/len(so):.0f}% of the time, by over 3 cm {100*sum(1 for x in so if x>0.03)/len(so):.0f}%, over 6 cm {100*sum(1 for x in so if x>0.06)/len(so):.0f}%; worst {so[-1]*100:.0f} cm")
for mode,label in (('0','standing'),('1','walking'),('2','in a clip (a strike, a reaction)')):
    xs=sorted(x[0] for x in xout if x[1]==mode)
    if xs: print(f"both feet down, {label} ({len(xs)/60:.0f} unit-seconds): the point a foot would have to be on to stop the body is outside the support {100*sum(1 for x in xs if x>0.0)/len(xs):.0f}% of the time, by over 10 cm {100*sum(1 for x in xs if x>0.10)/len(xs):.0f}%, over 25 cm {100*sum(1 for x in xs if x>0.25)/len(xs):.0f}%")

if lean:
    for lo,hi,label in ((0,0.3,'under 0.3 m/s'),(0.3,0.8,'0.3 to 0.8 m/s')):
        xs=[x for x in lean if lo<=x[2]<hi]
        if xs:
            o=sorted(x[0] for x in xs); c=sorted(x[1] for x in xs)
            print(f"on one foot, standing or walking, body moving {label} ({len(xs)/60:.0f} unit-seconds): centre of gravity outside that foot by a median {o[len(o)//2]*100:.0f} cm (a quarter over {o[3*len(o)//4]*100:.0f} cm); the point a foot would have to be on to stop the body, a median {c[len(c)//2]*100:.0f} cm outside it")
