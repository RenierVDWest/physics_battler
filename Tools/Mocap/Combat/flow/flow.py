#!/usr/bin/env python3
"""Blade flow in sampled mocap takes: where the blows are, how long between them, how slow the blade gets between,
how far it carries through, and how its path turns between blows. Blade approximated from the recorded hand."""
import json, sys, math, os
import numpy as np
S=os.path.join(os.path.dirname(os.path.abspath(__file__)),'..','samples','')
def load(name):
    d=json.load(open(S+name+'.json'))
    b={n:i for i,n in enumerate(d['bones'])}
    f=np.array(d['frames'],dtype=float).reshape(len(d['frames']),-1,3)
    return d,b,f
def unit(v):
    n=np.linalg.norm(v,axis=-1,keepdims=True); return v/np.maximum(n,1e-9)
def blade(b,f,side='Right',length=0.9):
    hand=f[:,b[side+'Hand']]; idx=f[:,b[side+'HandIndex1']]; mid=f[:,b[side+'HandMiddle1']]; pin=f[:,b[side+'HandPinky1']]
    x=unit(idx-pin); along=mid-hand; fz=unit(along-x*np.sum(along*x,axis=1,keepdims=True))
    way=unit(0.94*x+0.34*fz)
    grip=hand+0.08*fz
    return grip, way, grip+way*length
def smooth(a,k=1):
    if k<=0: return a
    out=a.copy()
    for i in range(len(a)):
        lo=max(0,i-k); hi=min(len(a),i+k+1); out[i]=a[lo:hi].mean(axis=0)
    return out
def analyse(name, side='Right', peak_min=6.0, length=0.9, verbose=True):
    d,b,f=load(name); rate=d['rate']; n=len(f)
    grip,way,tip=blade(b,f,side,length)
    # tip against the hips' travel (so a walking body's own travel is not counted as blade speed)
    hips=f[:,b['Hips']]
    rel=tip-hips
    v=np.zeros(n); v[1:-1]=np.linalg.norm(rel[2:]-rel[:-2],axis=1)*rate/2; v[0]=v[1]; v[-1]=v[-2]
    vw=np.zeros(n); vw[1:-1]=np.linalg.norm(tip[2:]-tip[:-2],axis=1)*rate/2; vw[0]=vw[1]; vw[-1]=vw[-2]
    hv=np.zeros(n); hv[1:-1]=np.linalg.norm((hips[2:]-hips[:-2])[:,[0,2]],axis=1)*rate/2
    # peaks: local maxima of the tip's speed over peak_min, at least 0.2 s apart
    peaks=[]
    for i in range(1,n-1):
        if v[i]>=peak_min and v[i]>=v[i-1] and v[i]>=v[i+1]:
            if peaks and (i-peaks[-1])/rate<0.2:
                if v[i]>v[peaks[-1]]: peaks[-1]=i
            else: peaks.append(i)
    rows=[]
    for k,p in enumerate(peaks):
        nxt=peaks[k+1] if k+1<len(peaks) else None
        # after the blow: where the blade is slowest before the next blow (or within 0.8 s)
        hi=nxt if nxt is not None else min(n-1,p+int(0.8*rate))
        seg=v[p:hi+1]; m=p+int(np.argmin(seg))
        arc=float(np.sum(np.linalg.norm(rel[p+1:m+1]-rel[p:m],axis=1))) if m>p else 0.0
        # how the path turns at the slowest point: its way just before against just after
        a0=max(p,m-3); a1=min(n-1,m+3)
        din=unit((rel[m]-rel[a0])[None])[0] if m>a0 else np.zeros(3); dout=unit((rel[a1]-rel[m])[None])[0] if a1>m else np.zeros(3)
        turn=math.degrees(math.acos(float(np.clip(np.dot(din,dout),-1,1)))) if m>a0 and a1>m else float('nan')
        rows.append(dict(t=p/rate, peak=float(v[p]), next=(nxt-p)/rate if nxt is not None else None, slow=float(v[m]), slow_t=(m-p)/rate, arc=arc, turn=turn,
                         tip_y=float(tip[p,1]), dirx=float((rel[min(n-1,p+1)]-rel[max(0,p-1)])[0]), diry=float((rel[min(n-1,p+1)]-rel[max(0,p-1)])[1]), hips=float(hv[p])))
    if verbose:
        print(f"== {name}: {n/rate:.2f} s, {len(peaks)} blows (tip over {peak_min:.0f} m/s against the hips)")
        for r in rows:
            nx=f"next blow {r['next']:.2f} s later" if r['next'] is not None else "last"
            hor='to his left' if r['dirx']<0 else 'to his right'
            ver='down' if r['diry']<-0.02 else 'up' if r['diry']>0.02 else 'level'
            print(f"   {r['t']:.2f} s  tip {r['peak']:4.1f} m/s, going {hor} and {ver} at {r['tip_y']:.2f} m up; after it: {r['arc']:.2f} m of travel to its slowest, {r['slow']:.1f} m/s, {r['slow_t']:.2f} s later, where its path turns {r['turn']:.0f} deg; {nx}; hips going {r['hips']:.1f} m/s")
    return rows, v, tip, rel, rate
if __name__=='__main__':
    for a in sys.argv[1:]:
        side='Right'
        if a.endswith(':L'): a=a[:-2]; side='Left'
        try: analyse(a, side)
        except Exception as e: print('!!',a,e)
