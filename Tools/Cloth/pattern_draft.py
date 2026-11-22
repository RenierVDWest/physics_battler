"""pattern_draft.py [out.png]: a first draft of the shirt and trouser pieces for the Medium (1.83 m), in cm, drawn
flat (the plan "Clothes cut and sewn like real clothes", 22 November). Measures from measure_body.py."""
import sys, numpy as np, matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt

def bez(p0, p1, p2, n=12):
    t = np.linspace(0, 1, n)[:, None]
    return (1-t)**2*np.array(p0) + 2*(1-t)*t*np.array(p1) + t**2*np.array(p2)

# --- shirt body (half piece mirrored), origin at centre front/back, y up from hem
W = 30.0          # half width: chest 96 + 24 ease = 120 -> 30 a quarter
L = 66.0          # length, side neck point to hem
neckW = 7.5       # half neck width
shDrop = 4.5      # shoulder drop
shPt = 23.0       # shoulder point from centre
ahD = 25.0        # armhole depth from shoulder point
def body(front):
    nd = 8.0 if front else 2.0
    top = L
    pts = [(0, 0), (W, 0), (W, top - shDrop - ahD)]
    # armhole curve up to the shoulder point
    pts += list(map(tuple, bez((W, top - shDrop - ahD), (shPt - 0.5, top - shDrop - ahD + 3), (shPt, top - shDrop), 14)))[1:]
    pts += [(neckW, top)]
    pts += list(map(tuple, bez((neckW, top), (neckW, top - nd), (0, top - nd), 10)))[1:]
    half = np.array(pts)
    other = half[::-1].copy(); other[:, 0] *= -1
    return np.vstack([half, other[1:-1]])
# --- sleeve: cap height 9, bicep 44, cuff 26, length 60
def sleeve():
    capH, bic, cuff, ln = 9.0, 44.0, 26.0, 60.0
    b, c = bic / 2, cuff / 2
    cap = bez((-b, ln - capH), (0, ln + capH * 0.9), (b, ln - capH), 24)
    return np.vstack([[(-c, 0), (c, 0)], cap[::-1][::-1][::-1] if False else cap[::-1]][::-1]) if False else np.vstack([[(-c, 0), (c, 0), (b, ln - capH)], cap[::-1][1:-1], [(-b, ln - capH)]])
band = np.array([(0, 0), (40, 0), (40, 3), (0, 3)])
# --- trousers: front and back leg (cm), y up from hem; hip 100 total, rise front 25 back 30, inseam 79
def leg(front):
    hipHalf = 24.0 if front else 26.0
    ext = 6.0 if front else 12.0
    rise = 25.0 if front else 30.0
    ins = 79.0
    hem = 21.0 if front else 23.0
    knee = 24.0 if front else 26.0
    # x: 0 at side seam, grows toward centre
    pts = [(0, 0), (hem, 0), (knee, 46), (hipHalf + ext, ins)]
    crotch = bez((hipHalf + ext, ins), (hipHalf, ins + 2), (hipHalf, ins + rise * 0.6), 10)
    pts += list(map(tuple, crotch))[1:]
    pts += [(hipHalf, ins + rise), (0, ins + rise - (0 if front else 0))]
    return np.array(pts)
wb = np.array([(0, 0), (80, 0), (80, 4), (0, 4)])

fig, ax = plt.subplots(1, 2, figsize=(13, 7.5), gridspec_kw={"width_ratios": [1.25, 1]})
def draw(a, poly, at, label, dims):
    p = poly + np.array(at)
    a.fill(p[:, 0], p[:, 1], fc=(0.86, 0.89, 0.95), ec=(0.2, 0.25, 0.4), lw=1.3)
    cx, cy = (p[:, 0].min() + p[:, 0].max()) / 2 - (6 if "leg" in label else 0), (p[:, 1].min() + p[:, 1].max()) / 2
    a.text(cx, cy + 4, label, ha="center", va="center", fontsize=10, weight="bold")
    a.text(cx, cy - 5, dims, ha="center", va="center", fontsize=8, color=(0.25, 0.25, 0.25))
a = ax[0]
draw(a, body(True), (32, 0), "Front", "width 60 · length 66\nneck drop 8")
draw(a, body(False), (100, 0), "Back", "width 60 · length 66\nneck drop 2")
draw(a, sleeve(), (32, -78), "Sleeve ×2", "bicep 44 · cuff 26\nlength 60 · cap 9")
draw(a, band, (78, -40), "Neck band", "\n\n40 × 3")
a.set_title("Shirt (cm): untucked, straight from chest to hem, set-in sleeves, round neck", fontsize=10)
a = ax[1]
draw(a, leg(True), (0, 0), "Front leg ×2", "hip 24 · rise 25\ninseam 79 · hem 21")
draw(a, leg(False), (48, 0), "Back leg ×2", "hip 26 · rise 30\ninseam 79 · hem 23")
draw(a, wb, (6, -16), "Waistband", "\n\n80 × 4")
a.set_title("Trousers (cm): loose, straight, drawstring waist, to the feet", fontsize=10)
for a in ax:
    a.set_aspect("equal"); a.axis("off")
plt.tight_layout()
plt.savefig(sys.argv[1] if len(sys.argv) > 1 else "pieces.png", dpi=110)
print("ok")
