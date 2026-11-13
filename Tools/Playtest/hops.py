# hops.py <fight dirs...>: each step back (the frames' givingGround going to 1) and the 0.45 s from it: how long the root was moved, how fast,
# the time with both feet off the ground, whether the two feet came down together (within 0.035 s), and how long both then stood
import sys,gzip,csv,glob
T=[0]*11
for d in sys.argv[1:]:
    logs=sorted(glob.glob(d+'/PlaytestLogs/*'))[-1]
    n=hop=clip=tog=slow=landed2=0; both=secs=moved=took=stood=0.0
    for f in glob.glob(logs+'/units/*.frames.csv.gz'):
        rows=list(csv.reader(gzip.open(f,'rt'))); h=rows[0]; rows=rows[1:]
        g=h.index('givingGround'); fl=h.index('footL'); fr=h.index('footR'); cm=h.index('clipMode'); st=h.index('stepping')
        i=1
        while i<len(rows):
            if rows[i][g]=='1' and rows[i-1][g]=='0':
                j=i
                while j<len(rows) and rows[j][g]=='1': j+=1
                w=rows[i:i+27]
                if len(w)==27 and all(r[st]!='6' for r in w):
                    n+=1
                    air=[(int(r[fl])>=3,int(r[fr])>=3) for r in w]
                    b=sum(1 for a in air if a[0] and a[1])
                    both+=b/60.0; secs+=27/60.0; hop+=1 if b>=3 else 0
                    clip+=1 if rows[i][cm]=='2' else 0
                    dist=((float(rows[j-1][2])-float(rows[i-1][2]))**2+(float(rows[j-1][3])-float(rows[i-1][3]))**2)**0.5
                    moved+=dist; took+=(j-i)/60.0
                    slow+=1 if dist/max(1e-6,(j-i)/60.0)<2.0 else 0
                    # the first landing of each foot after it was in the air
                    land=[None,None]
                    for side in (0,1):
                        up=False
                        for k,a in enumerate(air):
                            if a[side]: up=True
                            elif up: land[side]=k; break
                    if land[0] is not None and land[1] is not None:
                        landed2+=1
                        if abs(land[0]-land[1])<=2:
                            tog+=1
                            k=max(land); s=0
                            while k<len(air) and not air[k][0] and not air[k][1]: s+=1; k+=1
                            stood+=s/60.0
                i=j
            else: i+=1
    print(f"{d.split('/')[-1]:8} steps back {n:3}: {100*moved/max(1,n):3.0f} cm in {took/max(1,n):.2f} s; both feet in the air {100*both/max(1e-6,secs):3.0f}% of the 0.45 s; both came down in it: {landed2:2}, together: {tog:2} ({100*tog/max(1,n):.0f}% of steps back), then both stood {stood/max(1,tog):.2f} s; begun in a clip {clip}")
    for k,v in enumerate((n,hop,both,secs,clip,moved,took,slow,landed2,tog,stood)): T[k]+=v
print(f"all      steps back {T[0]}: {100*T[5]/max(1,T[0]):.0f} cm in {T[6]/max(1,T[0]):.2f} s; both feet in the air {100*T[2]/max(1e-6,T[3]):.0f}% of the time; the feet came down together in {T[9]} ({100*T[9]/max(1,T[0]):.0f}%), then both stood {T[10]/max(1,T[9]):.2f} s")
