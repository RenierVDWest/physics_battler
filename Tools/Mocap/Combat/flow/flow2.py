import sys, math
import numpy as np
from flow import load, blade, unit
def run(name, side='Right', real=14.0, length=0.9):
    d,b,f=load(name); rate=d['rate']; n=len(f)
    grip,way,tip=blade(b,f,side,length); hips=f[:,b['Hips']]; rel=tip-hips
    v=np.zeros(n); v[1:-1]=np.linalg.norm(rel[2:]-rel[:-2],axis=1)*rate/2; v[0]=v[1]; v[-1]=v[-2]
    peaks=[]
    for i in range(1,n-1):
        if v[i]>=real and v[i]>=v[i-1] and v[i]>=v[i+1]:
            if peaks and (i-peaks[-1])/rate<0.25:
                if v[i]>v[peaks[-1]]: peaks[-1]=i
            else: peaks.append(i)
    out=[]
    print(f"== {name} ({n/rate:.2f} s): {len(peaks)} blows at "+", ".join(f"{p/rate:.2f}" for p in peaks)+" s; tip speeds "+", ".join(f"{v[p]:.0f}" for p in peaks)+" m/s")
    for k,p in enumerate(peaks):
        nxt=peaks[k+1] if k+1<len(peaks) else None
        hi=nxt if nxt is not None else min(n-1,p+int(0.7*rate))
        m=p+int(np.argmin(v[p:hi+1]))
        arc=float(np.sum(np.linalg.norm(rel[p+1:m+1]-rel[p:m],axis=1))) if m>p else 0
        # the angle the blade itself sweeps from the blow to the slowest point (degrees, summed step by step)
        sw=0.0
        for i in range(p,m):
            sw+=math.degrees(math.acos(float(np.clip(np.dot(way[i],way[i+1]),-1,1))))
        a0=max(p,m-2); a1=min(n-1,m+2)
        din=unit((rel[m]-rel[a0])[None])[0]; dout=unit((rel[a1]-rel[m])[None])[0]
        turn=math.degrees(math.acos(float(np.clip(np.dot(din,dout),-1,1)))) if m>a0 and a1>m else float('nan')
        d1=rel[min(n-1,p+1)]-rel[max(0,p-1)]
        hor='R>L' if d1[0]<0 else 'L>R'; ver='down' if d1[1]<-0.03 else 'up' if d1[1]>0.03 else 'flat'
        fw='in' if d1[2]>0.03 else 'back' if d1[2]<-0.03 else ''
        hv=np.linalg.norm((hips[min(n-1,p+3)]-hips[max(0,p-3)])[[0,2]])*rate/(min(n-1,p+3)-max(0,p-3))
        s=f"   blow {k+1} at {p/rate:.2f} s: {hor} {ver} {fw}, tip {tip[p,1]:.2f} m up, {v[p]:.0f} m/s; carries on {arc:.1f} m ({sw:.0f} deg of blade) over {(m-p)/rate:.2f} s to its slowest {v[m]:.1f} m/s, path turns {turn:.0f} deg there"
        if nxt is not None: s+=f"; next blow {(nxt-p)/rate:.2f} s after this one ({(nxt-m)/rate:.2f} s after the slowest point)"
        s+=f"; hips {hv:.1f} m/s"
        print(s)
for a in sys.argv[1:]:
    side='Right'
    if a.endswith(':L'): a=a[:-2]; side='Left'
    try: run(a,side)
    except Exception as e: print('!!',a,repr(e))
