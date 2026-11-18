import json, pathlib, re
out = pathlib.Path(__file__).parent

def esc(t):
    return t.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")
def st(d=0, extra=""):
    return 'style="--d:%.1fs%s"' % (d, (";" + extra) if extra else "")
def redt(d=0, extra=""):
    return st(d, "fill:var(--signal)" + ((";" + extra) if extra else ""))
def T(x, y, text, cls, attrs):
    return '<text class="el %s" x="%.0f" y="%.0f" %s>%s</text>' % (cls, x, y, attrs, esc(text))
def P(d, attrs, cls="s", dd=0, extra="", draw=False, arrow=False):
    return '<path class="el %s%s" %s %s d="%s"%s %s/>' % (cls, " draw" if draw else "", attrs,
        'pathLength="1"' if draw else "", d, ' marker-end="url(#ah)"' if arrow else "", st(dd, extra))
def R(x, y, w, h, attrs, cls="f", dd=0, extra=""):
    return '<rect class="el %s" %s x="%d" y="%d" width="%d" height="%d" %s/>' % (cls, attrs, x, y, w, h, st(dd, extra))
def box(x, y, w, h, attrs, dd=0):
    return R(x, y, w, h, attrs, "f pop", dd, "stroke:var(--signal);stroke-width:4")
def xmark(x, y, attrs, d=0, r=16):
    return P("M%.0f %.0f l%d %d M%.0f %.0f l%d %d" % (x - r, y - r, 2 * r, 2 * r, x + r, y - r, -2 * r, 2 * r), attrs, "s a", d, "stroke-width:5")
def tick(x, y, attrs, d=0, r=18):
    return P("M%.0f %.0f l%.0f %.0f l%.0f %.0f" % (x - r, y, r * .7, r * .8, r * 1.5, -r * 1.7), attrs, "s a", d, "stroke-width:5")
def RED(attrs, d=0, extra=""):
    return attrs + " " + redt(d, extra)
def SH(attrs, d=0, extra=""):
    return attrs + " " + st(d, "fill:var(--sheet)" + ((";" + extra) if extra else ""))
def END(attrs, d=0, extra=""):
    return attrs + " " + st(d, "text-anchor:end" + ((";" + extra) if extra else ""))
def bracket(x0, x1, y, attrs, d=0, up=True):
    h = -14 if up else 14
    return P("M%.0f %.0f v%d H%.0f v%d" % (x0, y - h, h, x1, -h), attrs, "s thin", d)
def dot(x, y, attrs, d=0, r=13, acc=True):
    return '<circle class="el ink%s" %s cx="%.0f" cy="%.0f" r="%d" %s/>' % (" a" if acc else "", attrs, x, y, r, st(d))

# ---- a stick fighter seen from the side. Local coords: facing +x, y up is negative.
LEGS = dict(hip=(0, -150), k1=(-30, -75), f1=(-50, 0), k2=(45, -75), f2=(55, 0))
def pose(**kw):
    d = dict(LEGS); d.update(neck=(0, -270), head=(0, -302)); d.update(kw); return d
GUARD = dict(el=(25, -205), ha=(45, -215), sh=(45, -215))
POSES = {
 "guard":  pose(**GUARD),
 "shove":  pose(hip=(5, -150), neck=(18, -268), head=(24, -300), k1=(-40, -72), f1=(-70, 0),
                el=(60, -235), ha=(100, -232), sh=(100, -232)),
 "brace":  pose(hip=(-5, -138), neck=(12, -255), head=(18, -287), k1=(-50, -66), f1=(-90, 0), k2=(50, -70), f2=(70, 0),
                el=(58, -222), ha=(98, -220), sh=(98, -220)),
 "sway":   pose(neck=(-22, -268), head=(-30, -299), el=(5, -205), ha=(25, -218), sh=(25, -218)),
 "bend":   pose(hip=(-22, -142), k1=(-40, -72), k2=(30, -74), neck=(28, -250), head=(42, -280),
                el=(55, -190), ha=(75, -200), sh=(75, -200)),
 "step":   pose(hip=(-30, -146), k1=(-85, -78), f1=(-140, 0), k2=(20, -76), f2=(50, 0),
                neck=(-58, -262), head=(-70, -292), el=(-30, -205), ha=(-8, -215), sh=(-8, -215)),
 "stumble": pose(hip=(-5, -148), neck=(-48, -262), head=(-64, -290), k1=(-45, -75), f1=(-80, 0), k2=(30, -80), f2=(55, -14),
                el=(0, -215), ha=(28, -205), sh=(28, -205)),
 "down":   dict(hip=(0, -22), neck=(-118, -30), head=(-152, -32), k1=(50, -48), f1=(110, -12), k2=(55, -22), f2=(120, -4),
                el=(-88, -62), ha=(-62, -80)),
}
def fig(x, g, p, attrs, face=1, col="var(--ink)", s=1.0, dd=0, scol=None, extra=""):
    p = POSES[p]
    def q(k):
        return (x + face * s * p[k][0], g + s * p[k][1])
    ln = lambda pts, w=5, c=col: '<path d="M%s" style="fill:none;stroke:%s;stroke-width:%.1f;stroke-linecap:round;stroke-linejoin:round"/>' % (
        " L".join("%.0f %.0f" % q(k) for k in pts), c, w)
    parts = [ln(["f1", "k1", "hip", "k2", "f2"]), ln(["hip", "neck"]), ln(["neck", "el", "ha"])]
    hx, hy = q("head")
    parts.append('<circle cx="%.0f" cy="%.0f" r="%.0f" style="fill:var(--sheet);stroke:%s;stroke-width:4"/>' % (hx, hy, 24 * s, col))
    if "sh" in p:
        sx, sy = q("sh")
        parts.append('<path d="M%.0f %.0f V%.0f" style="fill:none;stroke:%s;stroke-width:%.0f;stroke-linecap:round"/>' % (
            sx + face * 8 * s, sy - 42 * s, sy + 42 * s, scol or col, 14 * s))
    return '<g class="el" %s %s>%s</g>' % (attrs, st(dd, extra), "".join(parts))
def board_x(x, p, face, s):
    return x + face * s * (POSES[p]["sh"][0] + 8)

GREY = "var(--ink2)"
SIG = "var(--signal)"
