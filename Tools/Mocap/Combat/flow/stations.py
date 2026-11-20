import sys, math, json
import numpy as np
from flow import load, blade, unit
TAKES = {
 'forehand':      ('sword_and_shield_slash_3', 0.0, 1.50, 0.63),
 'rising':        ('sword_and_shield_slash_1', 0.0, 1.67, 0.83),
 'thrust':        ('sword_and_shield_attack_2', 0.0, 1.00, 0.48),
 'spin_backhand': ('sword_and_shield_attack_1', 0.0, 1.73, 0.77),
 'leap_chop':     ('sword_and_shield_slash_2', 0.0, 2.43, 1.40),
 'chop_lunge':    ('sword_and_shield_attack_3', 0.0, 2.33, 1.17),
 'spin_high':     ('spin_attack_2', 0.0, 1.30, 0.53),
 'combo_fore':    ('sword_and_shield_slash', 0.0, 1.05, 0.67),
 'combo_back':    ('sword_and_shield_slash', 0.80, 1.60, 1.23),
 'combo_chop':    ('sword_and_shield_slash', 1.37, 3.53, 2.47),
 'quick_fore':    ('pack_sns_slash_5', 0.0, 9.0, None),
}
def frame_of(b, f, i):
    L=f[i,b['LeftUpLeg']]; R=f[i,b['RightUpLeg']]
    right=R-L; right[1]=0; right=right/max(1e-9,np.linalg.norm(right))
    up=np.array([0.,1.,0.])
    return right, up
def run(name):
    take,a,bb,ct=TAKES[name]
    d,b,f=load(take); rate=d['rate']; n=len(f)
    grip,way,tip=blade(b,f,'Right',0.9); hips=f[:,b['Hips']]; rel=tip-hips
    v=np.zeros(n); v[1:-1]=np.linalg.norm(rel[2:]-rel[:-2],axis=1)*rate/2; v[0]=v[1]; v[-1]=v[-2]
    i0=int(round(a*rate)); i1=min(n-1,int(round(bb*rate)))
    if ct is None:
        c=i0+int(np.argmax(v[i0:i1+1]))
    else:
        c0=int(round(ct*rate)); lo=max(i0,c0-4); hi=min(i1,c0+4); c=lo+int(np.argmax(v[lo:hi+1]))
    # entry: the slowest moment of the point in the 0.6 s before the blow (not the last 0.1 s)
    lo=max(i0+1,c-int(0.7*rate)); hi=max(lo+1,c-int(0.1*rate)); e=None
    for i in range(hi,lo-1,-1):
        if v[i]<=v[i-1] and v[i]<=v[i+1] and v[i]<0.35*v[c]: e=i; break
    if e is None: e=lo+int(np.argmin(v[lo:hi+1]))
    # exit: the slowest moment in the 0.7 s after (not the first 0.1 s)
    lo2=min(i1,c+int(0.1*rate)); hi2=min(i1-1,c+int(0.8*rate)); x=None
    for i in range(lo2,hi2+1):
        if v[i]<=v[i-1] and v[i]<=v[i+1] and v[i]<0.35*v[c]: x=i; break
    if x is None: x=lo2+int(np.argmin(v[lo2:hi2+1]))
    # forward: hips' facing at the take's first frame of this clip (toward the target for the in-place cuts)
    r0,up=frame_of(b,f,i0)
    def fwd_from(right):
        # choose the horizontal vector perpendicular to right that points the way the chest faces: use the spine's lean of the head ahead of hips is unreliable; use the toes
        cand=np.cross(right,up)
        toe=(f[i0,b['LeftToeBase']]+f[i0,b['RightToeBase']])/2-(f[i0,b['LeftFoot']]+f[i0,b['RightFoot']])/2
        toe[1]=0
        return cand if np.dot(cand,toe)>=0 else -cand
    f0=fwd_from(r0)
    sign=1.0 if np.dot(np.cross(r0,up),f0)>=0 else -1.0
    def local(i, p):
        right,_=frame_of(b,f,i); fw=np.cross(right,up)*sign
        q=p-hips[i]
        return np.array([np.dot(q,right), q[1], np.dot(q,fw)])
    def yaw(i):
        right,_=frame_of(b,f,i); fw=np.cross(right,up)*sign
        return math.degrees(math.atan2(np.dot(fw,r0), np.dot(fw,f0)))
    def veldir(i):
        j0=max(0,i-1); j1=min(n-1,i+1)
        return local(j1,tip[j1])-local(j0,tip[j0])
    out=dict(name=name, take=take, rate=rate, contact=(c-i0)/rate, peak=float(v[c]),
             entry_t=(e-i0)/rate, entry_before=(c-e)/rate, entry_speed=float(v[e]), entry_grip=local(e,grip[e]), entry_tip=local(e,tip[e]), entry_yaw=yaw(e),
             exit_t=(x-i0)/rate, exit_after=(x-c)/rate, exit_speed=float(v[x]), exit_grip=local(x,grip[x]), exit_tip=local(x,tip[x]), exit_yaw=yaw(x),
             con_grip=local(c,grip[c]), con_tip=local(c,tip[c]), con_yaw=yaw(c),
             exit_arc=float(np.sum(np.linalg.norm(rel[c+1:x+1]-rel[c:x],axis=1))),
             entry_vel=veldir(min(n-1,e+2)), exit_vel=veldir(max(0,x-2)),
             hips_travel=float(np.linalg.norm((hips[x]-hips[e])[[0,2]])))
    return out
def station(g, t):
    side='R' if g[0]>0.12 else 'L' if g[0]<-0.12 else 'C'
    h='high' if g[1]>0.45 else 'mid' if g[1]>0.05 else 'low'
    d=t-g; dn=d/max(1e-9,np.linalg.norm(d))
    pt='up' if dn[1]>0.6 else 'down' if dn[1]<-0.6 else ('fwd' if dn[2]>0.5 else 'back' if dn[2]<-0.5 else ('out-R' if dn[0]>0 else 'out-L'))
    return f"hand {h} {side}, point {pt}"
if __name__=='__main__':
    R={}
    for k in TAKES:
        try: R[k]=run(k)
        except Exception as ex: print('!!',k,repr(ex))
    for k,r in R.items():
        print(f"{k:15s} contact {r['contact']:.2f}s peak {r['peak']:.0f} m/s | ENTRY {r['entry_before']:.2f}s before ({r['entry_speed']:.1f} m/s): {station(r['entry_grip'],r['entry_tip'])}; grip {np.round(r['entry_grip'],2)} tip {np.round(r['entry_tip'],2)} yaw {r['entry_yaw']:.0f} | EXIT {r['exit_after']:.2f}s after ({r['exit_speed']:.1f} m/s, {r['exit_arc']:.1f} m on): {station(r['exit_grip'],r['exit_tip'])}; grip {np.round(r['exit_grip'],2)} tip {np.round(r['exit_tip'],2)} yaw {r['exit_yaw']:.0f}")
    print()
    names=list(R)
    print('JOIN: from (rows) exit -> to (cols) entry: tip gap m / grip gap m / blade angle deg / pelvis yaw diff')
    def ang(u,w):
        return math.degrees(math.acos(float(np.clip(np.dot(u/np.linalg.norm(u), w/np.linalg.norm(w)),-1,1))))
    T={}
    for a in names:
        for c in names:
            ra,rc=R[a],R[c]
            tg=float(np.linalg.norm(ra['exit_tip']-rc['entry_tip'])); gg=float(np.linalg.norm(ra['exit_grip']-rc['entry_grip']))
            ba=ang(ra['exit_tip']-ra['exit_grip'], rc['entry_tip']-rc['entry_grip'])
            yd=abs(((ra['exit_yaw']-rc['entry_yaw']+180)%360)-180)
            va=ang(ra['exit_vel'], rc['entry_vel']) if np.linalg.norm(ra['exit_vel'])>1e-6 and np.linalg.norm(rc['entry_vel'])>1e-6 else float('nan')
            T[(a,c)]=(tg,gg,ba,yd,va)
    hdr=' '*15+''.join(f"{c[:11]:>12s}" for c in names); print(hdr)
    for a in names:
        print(f"{a:15s}"+''.join(f"{T[(a,c)][0]:5.2f}/{T[(a,c)][1]:4.2f}  " for c in names))
    print('\nblade angle (deg) / path turn (deg)')
    print(hdr)
    for a in names:
        print(f"{a:15s}"+''.join(f"{T[(a,c)][2]:5.0f}/{T[(a,c)][4]:4.0f}  " for c in names))
