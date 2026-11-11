"""How closely the physical body follows its pose, and how often two living bodies overlap, from a real playtest log.

    python3 Tools/Playtest/puppet.py PlaytestLogs/<session>

Reads units/*.pose.csv.gz (each part's physical position and its posed one, every physics step) and events.log (deaths).
Used by the code review of 11 November 2026: https://claude.ai/artifact/5YY9GpcDGQRLVyZPRJjcsP
"""
import gzip, csv, sys, glob, os, math
import numpy as np
L = sys.argv[1]
PARTS = ["Pelvis","Spine","Chest","Head","UpperArmL","ForearmL","HandL","UpperArmR","ForearmR","HandR","ThighL","ShinL","FootL","ThighR","ShinR","FootR"]
BONES = [("Pelvis","Chest",0.13),("Chest","Head",0.09),("UpperArmL","ForearmL",0.05),("ForearmL","HandL",0.04),("UpperArmR","ForearmR",0.05),("ForearmR","HandR",0.04),
         ("ThighL","ShinL",0.07),("ShinL","FootL",0.05),("ThighR","ShinR",0.07),("ShinR","FootR",0.05)]
units = {}
for f in sorted(glob.glob(os.path.join(L,"units","*.pose.csv.gz"))):
    name = os.path.basename(f).split(".")[0]
    with gzip.open(f,"rt") as fh:
        r = csv.reader(fh); head = next(r); idx = {h:i for i,h in enumerate(head)}
        rows = [row for row in r if len(row)==len(head)]
    a = np.array([[float(x) if x != "" else np.nan for x in row] for row in rows], dtype=float)
    units[name] = (a, idx)
# deaths
dead = {}
for line in open(os.path.join(L,"events.log"), errors="ignore"):
    p = [x.strip() for x in line.split("|")]
    if len(p) >= 5 and p[3] == "death":
        dead[p[2]] = float(p[0])
def alive_mask(name, t):
    n = name.split("_",1)[1].replace("_"," ")
    return t < dead.get(n, 1e9)
print("== tracking error |physical - pose| (cm), units alive")
by = {}
for name,(a,idx) in units.items():
    t = a[:,0]; m = alive_mask(name,t)
    for p in PARTS:
        d = np.linalg.norm(a[m][:,[idx[p+".x"],idx[p+".y"],idx[p+".z"]]] - a[m][:,[idx[p+".gx"],idx[p+".gy"],idx[p+".gz"]]],axis=1)*100
        by.setdefault(p,[]).append(d[~np.isnan(d)])
for grp,ps in (("legs",["Pelvis","ThighL","ShinL","FootL","ThighR","ShinR","FootR"]),("torso+head",["Spine","Chest","Head"]),("shield arm",["UpperArmL","ForearmL","HandL"]),("weapon arm",["UpperArmR","ForearmR","HandR"])):
    d = np.concatenate([x for p in ps for x in by[p]])
    print(f"  {grp:11s}: median {np.median(d):.2f}  95% {np.percentile(d,95):.2f}  99% {np.percentile(d,99):.2f}  max {d.max():.1f}   share of steps over 5 cm: {100*(d>5).mean():.2f}%")
# interpenetration between units (alive both), bone capsules
def segdist(p1,q1,p2,q2):
    d1=q1-p1; d2=q2-p2; r=p1-p2
    a=(d1*d1).sum(1); e=(d2*d2).sum(1); f=(d2*r).sum(1); c=(d1*r).sum(1); b=(d1*d2).sum(1)
    den=a*e-b*b
    s=np.where(den>1e-9, np.clip((b*f-c*e)/np.maximum(den,1e-9),0,1), 0.0)
    tt=np.clip((b*s+f)/np.maximum(e,1e-9),0,1)
    s=np.clip((b*tt-c)/np.maximum(a,1e-9),0,1)
    return np.linalg.norm((p1+d1*s[:,None])-(p2+d2*tt[:,None]),axis=1)
names=list(units)
print("== bodies passing through each other (bone capsules of two living units overlapping by more than 3 cm)")
tot_steps=0; pen_steps=0; worst=0; pairs_pen={}
tmin=max(units[n][0][0,0] for n in names); 
for i in range(len(names)):
    for j in range(i+1,len(names)):
        A,ia=units[names[i]]; B,ib=units[names[j]]
        n=min(len(A),len(B)); A=A[:n]; B=B[:n]; t=A[:,0]
        m=alive_mask(names[i],t)&alive_mask(names[j],t)
        if m.sum()==0: continue
        A=A[m];B=B[m]
        deep=np.zeros(len(A))
        for (a1,a2,ra) in BONES:
            pa=A[:,[ia[a1+".x"],ia[a1+".y"],ia[a1+".z"]]]; qa=A[:,[ia[a2+".x"],ia[a2+".y"],ia[a2+".z"]]]
            for (b1,b2,rb) in BONES:
                pb=B[:,[ib[b1+".x"],ib[b1+".y"],ib[b1+".z"]]]; qb=B[:,[ib[b2+".x"],ib[b2+".y"],ib[b2+".z"]]]
                d=segdist(pa,qa,pb,qb)
                deep=np.fmax(deep,np.nan_to_num((ra+rb)-d,nan=0.0))
        near = np.linalg.norm(A[:,[ia["Pelvis.x"],ia["Pelvis.z"]]]-B[:,[ib["Pelvis.x"],ib["Pelvis.z"]]],axis=1)<2.0
        tot_steps+=near.sum(); pen=(deep>0.03); pen_steps+=pen.sum(); worst=max(worst,deep.max())
        if pen.sum(): pairs_pen[(names[i],names[j])]=(pen.sum(),deep.max())
print(f"  steps with two living units within 2 m: {tot_steps} ({tot_steps/60:.0f} unit-pair seconds); overlapping >3 cm: {pen_steps} ({100*pen_steps/max(1,tot_steps):.1f}%); deepest {worst*100:.0f} cm")
for k,v in sorted(pairs_pen.items(), key=lambda kv:-kv[1][0])[:6]: print("   ",k[0],"x",k[1],f"{v[0]/60:.1f} s, deepest {v[1]*100:.0f} cm")
