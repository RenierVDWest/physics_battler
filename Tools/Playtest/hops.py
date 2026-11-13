# hops.py <fight dirs...>: each step back (the frames' givingGround going to 1) and the 0.45 s from it: how long the root was moved, how fast, and the time with both feet off the ground
import sys,gzip,csv,glob
T=[0]*8
for d in sys.argv[1:]:
    logs=sorted(glob.glob(d+'/PlaytestLogs/*'))[-1]
    n=hop=clip=0; both=secs=moved=took=0.0; slow=0
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
                    b=sum(1 for r in w if int(r[fl])>=3 and int(r[fr])>=3)
                    both+=b/60.0; secs+=27/60.0; hop+=1 if b>=3 else 0
                    clip+=1 if rows[i][cm]=='2' else 0
                    dist=((float(rows[j-1][2])-float(rows[i-1][2]))**2+(float(rows[j-1][3])-float(rows[i-1][3]))**2)**0.5
                    moved+=dist; took+=(j-i)/60.0
                    slow+=1 if dist/max(1e-6,(j-i)/60.0)<2.0 else 0
                i=j
            else: i+=1
    print(f"{d.split('/')[-1]:8} steps back {n:3}: {100*moved/max(1,n):3.0f} cm in {took/max(1,n):.2f} s each; under 2 m/s: {slow:2}; both feet off the ground {100*both/max(1e-6,secs):3.0f}% of the 0.45 s from it; a hop (both off 0.05 s or more): {hop:2} ({100*hop/max(1,n):.0f}%); begun in a clip {clip}")
    for k,v in enumerate((n,hop,both,secs,clip,moved,took,slow)): T[k]+=v
print(f"all      steps back {T[0]}: {100*T[5]/max(1,T[0]):.0f} cm in {T[6]/max(1,T[0]):.2f} s each; under 2 m/s: {T[7]} ({100*T[7]/max(1,T[0]):.0f}%); both feet off {100*T[2]/max(1e-6,T[3]):.0f}% of the time; hops {T[1]} ({100*T[1]/max(1,T[0]):.0f}%)")
