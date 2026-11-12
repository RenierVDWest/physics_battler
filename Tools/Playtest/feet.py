# feet.py <session dir>: by the ankles' heights: how long feet are down, how long and how far they swing, and how fast the body goes meanwhile
import sys, gzip, csv, math, glob, os, collections
d=sys.argv[1]
tot=collections.Counter(); spells=[]; drift=[]; byspeed=collections.defaultdict(lambda: collections.Counter())
for f in sorted(glob.glob(d+'/units/*.pose.csv.gz')):
    rows=[r for r in csv.DictReader(gzip.open(f,'rt')) if r['Pelvis.y'] not in ('','NaN')]
    fr={r['t']:r for r in csv.DictReader(gzip.open(f.replace('.pose.','.frames.'),'rt'))}
    up=[r for r in rows if float(r['Pelvis.y'])>0.62]
    if len(up)<60: continue
    stand={s:sorted(float(r['Foot'+s+'.y']) for r in up)[len(up)//20] for s in 'LR'}
    prev=None; sp={s:None for s in 'LR'}; held={s:None for s in 'LR'}
    for r in rows:
        if float(r['Pelvis.y'])<=0.62: prev=None; sp={s:None for s in 'LR'}; held={s:None for s in 'LR'}; continue
        t=float(r['t']); frm=fr.get(r['t'],{}); mode=frm.get('clipMode','?')
        a={s:(float(r['Foot'+s+'.x']),float(r['Foot'+s+'.y']),float(r['Foot'+s+'.z'])) for s in 'LR'}
        dn={s:a[s][1]<stand[s]+0.02 for s in 'LR'}
        root=(float(r['Pelvis.x']),float(r['Pelvis.z']))
        if prev is not None and 0<t-prev[0]<0.05:
            dt=t-prev[0]; tot['n']+=1
            rs=math.hypot(root[0]-prev[2][0],root[1]-prev[2][1])/dt
            k='both' if dn['L'] and dn['R'] else ('none' if not dn['L'] and not dn['R'] else 'one')
            tot[k]+=1
            band='under 0.2 m/s' if rs<0.2 else ('0.2 to 0.6 m/s' if rs<0.6 else ('0.6 to 1.2 m/s' if rs<1.2 else 'over 1.2 m/s'))
            byspeed[band][k]+=1; byspeed[band]['n']+=1
            for s in 'LR':
                pa=prev[1][s]
                if dn[s] and prev[3][s]:
                    mv=math.hypot(a[s][0]-pa[0],a[s][2]-pa[2])
                    tot['downsteps']+=1; tot['down_cm']+=mv*100
                    if mv/dt>0.05: tot['downmoving']+=1
                    if held[s] is None: held[s]=[a[s],0.0]
                    held[s][1]=max(held[s][1],math.hypot(a[s][0]-held[s][0][0],a[s][2]-held[s][0][2]))
                else:
                    if held[s] is not None and not dn[s]: drift.append(held[s][1]); held[s]=None
                if not dn[s]:
                    if sp[s] is None: sp[s]=[t,pa,0.0,0.0,mode,root]
                    sp[s][2]=max(sp[s][2],a[s][1]-stand[s])
                elif sp[s] is not None:
                    spells.append((t-sp[s][0], math.hypot(a[s][0]-sp[s][1][0],a[s][2]-sp[s][1][2]), sp[s][2], sp[s][4], math.hypot(root[0]-sp[s][5][0],root[1]-sp[s][5][1])))
                    sp[s]=None
        prev=(t,a,root,dn)
T=tot['n']/60
print(f"{T:.0f} unit-seconds on their feet (a foot counts as down while its ankle is within 2 cm of its standing height)")
print(f"both feet down {100*tot['both']/tot['n']:.0f}% of the time, one {100*tot['one']/tot['n']:.0f}%, neither {100*tot['none']/tot['n']:.0f}%")
for band in ('under 0.2 m/s','0.2 to 0.6 m/s','0.6 to 1.2 m/s','over 1.2 m/s'):
    c=byspeed[band]
    if c['n']: print(f"  body moving {band} ({100*c['n']/tot['n']:.0f}% of the time): both down {100*c['both']/c['n']:.0f}%, one {100*c['one']/c['n']:.0f}%, neither {100*c['none']/c['n']:.0f}%")
n=len(spells); du=sorted(x[0] for x in spells); le=sorted(x[1] for x in spells); hi=sorted(x[2] for x in spells)
print(f"swings (ankle up): {n} ({n/T:.1f} a second per unit); lasting: median {du[n//2]:.2f} s, a quarter over {du[3*n//4]:.2f} s, a tenth over {du[9*n//10]:.2f} s; length: median {le[n//2]*100:.0f} cm; ankle's rise: median {hi[n//2]*100:.0f} cm")
long=[x for x in spells if x[0]>0.6]
print(f"  swings lasting over 0.6 s: {len(long)} ({100*len(long)/n:.0f}%), taking {100*sum(x[0] for x in long)/sum(x[0] for x in spells):.0f}% of all the time feet are up; in them the foot goes {sorted(x[1] for x in long)[len(long)//2]*100:.0f} cm while the body goes {sorted(x[4] for x in long)[len(long)//2]*100:.0f} cm (medians)")
print(f"down feet: moving over 5 cm/s {100*tot['downmoving']/max(1,tot['downsteps']):.1f}% of the time; {tot['down_cm']/100/T*60:.2f} m per unit-minute; a foot's furthest drift from where it came down: median {sorted(drift)[len(drift)//2]*100:.1f} cm, a tenth over {sorted(drift)[9*len(drift)//10]*100:.1f} cm, worst {max(drift)*100:.0f} cm")
