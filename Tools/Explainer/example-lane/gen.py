import json, pathlib, re, math
out = pathlib.Path(__file__).parent
exec((out / "helpers.py").read_text(encoding="utf-8"))

def SM(x, y, t, attrs, d=0, size=None, cls="sm i", extra=""):
    e = ("font-size:%dpx" % size) if size else ""
    if extra: e = (e + ";" if e else "") + extra
    return T(x, y, t, cls, attrs + " " + st(d, e))
def M(x, y, t, attrs, d=0, red=False, ink=False, extra=""):
    if red:
        return T(x, y, t, "m", RED(attrs, d, extra))
    e = ("fill:var(--ink)" if ink else "") + ((";" if ink and extra else "") + extra if extra else "")
    return T(x, y, t, "m", attrs + " " + st(d, e))
def Mc(x, y, t, attrs, d=0, red=False, ink=False):
    return M(x, y, t, attrs, d, red, ink, "text-anchor:middle")
def Me(x, y, t, attrs, d=0, red=False, ink=False):
    return M(x, y, t, attrs, d, red, ink, "text-anchor:end")
def line(pts, attrs, cls="s", d=0, extra=""):
    return P("M" + " L".join("%.0f %.0f" % p for p in pts), attrs, cls, d, extra)
def BIG(x, y, t, attrs, d=0, size=80, red=False, strike=False):
    e = "font-size:%dpx" % size
    if red: e += ";fill:var(--signal)"
    if strike: e += ";fill:var(--ink2);text-decoration:line-through"
    return T(x, y, t, "big", attrs + " " + st(d, e))

# ---- the top view (a sketch): he faces up the screen, his sword on his right (screen right), shield on his left
SHX, SHY = 580, 640           # sword shoulder
CX, CY = 480, 640             # chest centre, the breastbone
BX0, BX1, BY = 360, 570, 512  # the board, edge on
R_EL, R_HA, R_TIP = 95, 195, 360
TH0, TH1 = -30, -112          # the sword now, and where its path reaches in the next 0.3 s
def pol(th, r):
    a = math.radians(th); return (SHX + r * math.cos(a), SHY + r * math.sin(a))
def cross_x(th, y):
    a = math.radians(th); t = (y - SHY) / math.sin(a); return SHX + t * math.cos(a)
XC = cross_x(TH1, BY)          # where the forearm's path meets the board's depth
LANE = 30
SHIFT = (XC - LANE) - BX1      # the board's step aside (negative: outward, screen left)

def body(attrs, d=0):
    return ['<ellipse class="el f" %s cx="%d" cy="%d" rx="115" ry="55" %s/>' % (attrs, CX, CY, st(d)),
            '<circle class="el s" %s cx="%d" cy="%d" r="36" %s/>' % (attrs, CX, CY, st(d, "fill:var(--sheet)"))]
def sword(th, attrs, d=0, fa_attrs=None, cls="s", ghost=False):
    fa = fa_attrs or attrs
    sh, el, ha, tp = (SHX, SHY), pol(th, R_EL), pol(th, R_HA), pol(th, R_TIP)
    a = math.radians(th); px, py = -math.sin(a) * 20, math.cos(a) * 20
    c = cls + (" dash" if ghost else "")
    return [line([sh, el], attrs, "s thin" if ghost else "s", d, "stroke-width:%d" % (3 if ghost else 7)),
            line([el, ha], fa, c, d, "stroke-width:%d" % (4 if ghost else 10)),
            line([ha, tp], fa, c, d, "stroke-width:%d" % (3 if ghost else 5)),
            line([(ha[0] - px, ha[1] - py), (ha[0] + px, ha[1] + py)], fa, c, d, "stroke-width:%d" % (3 if ghost else 6))]
def board(attrs, d=0, extra="stroke-width:20;stroke-linecap:butt"):
    return P("M%d %d H%d" % (BX0, BY, BX1), attrs, "s", d, extra)
def facing(attrs):
    return [P("M150 830 V700", attrs, "s thin", .1, "", False, True), M(172, 790, "HE FACES", attrs, .1)]
def fan_d(th0, th1):
    p0, p1, p2, p3 = pol(th0, R_EL), pol(th0, R_TIP), pol(th1, R_TIP), pol(th1, R_EL)
    return "M%.0f %.0f L%.0f %.0f A%d %d 0 0 0 %.0f %.0f L%.0f %.0f A%d %d 0 0 1 %.0f %.0f Z" % (
        p0 + p1 + (R_TIP, R_TIP) + p2 + p3 + (R_EL, R_EL) + p0)
def arc_d(th0, th1, r):
    p0, p1 = pol(th0, r), pol(th1, r)
    return "M%.0f %.0f A%d %d 0 0 0 %.0f %.0f" % (p0 + (r, r) + p1)
SKETCH = "seen from above · a sketch of the idea, not a measurement"
TX = 1090

S = []
# ================= 0. what you said, the bottom line
g = [M(60, 70, "WHAT YOU SAID · 19:44 · THE BOTTOM LINE FIRST", 'data-b="0"')]
a = 'data-b="0" data-dim="2"'
g += [R(60, 100, 1480, 220, a, "f", .1),
      M(100, 155, "PROCEED", a, .2, ink=True),
      SM(100, 210, "“yes this looks good. Proceed with the sword and shied where the shield gives way to the sword”", a, .4, 28)]
b = 'data-b="1" data-dim="2"'
g += [R(100, 240, 420, 56, b, "f pop", .1, "stroke:var(--signal);stroke-width:3"),
      Mc(310, 277, "IN · PUSHED · A SWITCH", b, .2, red=True),
      SM(560, 278, "his own sword and sword arm pass through his own shield far less", b, .6, 26)]
a = 'data-b="2" data-dim="3"'
g += [M(60, 400, "THE AIM TEST · THE SAME 19 BLOWS · RULE OFF, THEN ON · HEADLESS", a, .1, ink=True),
      SM(60, 445, "his sword and sword arm inside his own shield, in seconds", a, .2, 26),
      BIG(60, 560, "2.3 s", a, .4, 96, strike=True),
      T(300, 545, "›", "big", a + " " + st(.7, "font-size:80px;fill:var(--ink2)")),
      BIG(390, 560, "0.35 s", a, .9, 96, red=True),
      M(60, 600, "RULE OFF", a, .5), M(390, 600, "RULE ON", a, 1.0, red=True)]
a = 'data-b="3"'
bx = [("WHAT IT COSTS", ["the shield steps aside for his", "own cut, up to about half a metre"], True),
      ("WHAT TO LOOK AT", ["a shield opened for the blow,", "or a shield flung aside?"], False),
      ("NOT SEEN IN UNITY", ["measured headless,", "on 18 November"], False)]
for k, (h, ls, acc) in enumerate(bx):
    x = 60 + k * 510
    d = .2 + .9 * k
    g += [R(x, 620, 460, 240, a, "f pop", d, "stroke:var(--signal);stroke-width:4" if acc else ""),
          M(x + 25, 675, h, a, d + .1, red=acc, ink=not acc)]
    for j, s_ in enumerate(ls):
        g.append(SM(x + 25, 735 + 44 * j, s_, a, d + .3 + .2 * j, 26))
S.append(g)

# ================= 1. why his sword went through his own shield
g = [M(60, 70, "WHY HIS SWORD WENT THROUGH HIS OWN SHIELD", 'data-b="0"')]
a = 'data-b="0"'
g += body(a, .1) + facing(a)
g += [SM(60, 878, SKETCH, a, .1, 24)]
g += sword(TH0, 'data-b="0"', .3, 'data-b="0" data-hot="0"')
g += [P(arc_d(TH0 - 6, TH1 + 4, R_TIP + 30), 'data-b="0" data-acc="2"', "s thin", .8, "stroke-width:3", False, True),
      M(pol(-60, R_TIP + 55)[0], pol(-60, R_TIP + 55)[1], "HIS CUT", 'data-b="0"', 1.0)]
g += [board('data-b="0" data-hot="1"', .2),
      Me(BX0 - 20, BY + 8, "HIS SHIELD", 'data-b="0"', .3)]
a = 'data-b="1"'
g += [P("M%d %d V%d" % (CX, CY - 40, BY - 40), a, "s thin dash", .1),
      P("M%d %d v-14 H%d v14" % (CX, BY - 26, BX1), 'data-b="1" data-off="2"', "s a", .4, "stroke-width:3"),
      Mc(CX, CY + 95, "BREASTBONE", a, .2)]
a = 'data-b="2"'
g += sword(TH1, a, .2, None, "s a", ghost=True)
g += [xmark(XC + 10, BY, a, .9, 18)]
rows = [(0, "1 · EACH ARM POSED ALONE", ["the sword arm, by the attack", "the shield arm, by its own guard"], False),
        (1, "2 · SHIELD BEFORE HIS CHEST", ["right through his own blows", "its rim a hand's breadth past", "his breastbone, on the sword side"], False),
        (2, "3 · NOTHING LOOKED AT THE PAIR", ["his own cut swept straight", "through the edge of his shield"], True)]
y = 140
for b_, h, ls, red in rows:
    a = 'data-b="%d"%s' % (b_, "" if b_ == 2 else ' data-dim="%d"' % (b_ + 1))
    g.append(M(TX, y, h, a, .3, red=red, ink=not red))
    for j, s_ in enumerate(ls):
        g.append(SM(TX, y + 48 + 40 * j, s_, a, .6 + .3 * j, 26))
    y += 90 + 40 * len(ls)
S.append(g)

# ================= 2. the rule (the heart)
g = [M(60, 70, "THE RULE · THE SWORD'S PATH A MOMENT AHEAD", 'data-b="0"')]
a = 'data-b="0"'
g += body(a, .1) + facing(a)
g += [SM(60, 878, SKETCH, a, .1, 24)]
# the fan: where forearm and blade go in the next 0.3 s
fa = 'data-b="1" data-dim="4"'
g += [P(fan_d(TH0, TH1), fa, "s a dash", .1, "fill:var(--signal);fill-opacity:.07;stroke-width:3"),
      P(arc_d(TH0 - 4, TH1 + 6, R_TIP + 28), fa, "s a draw", .3, "stroke-width:3", False, True),
      M(610, 425, "THE NEXT 0.3 S", fa, .8, red=True)]
g += sword(TH1, fa, .6, None, "s a", ghost=True)
# the sword now: posed, and seen
g += sword(TH0, 'data-b="0"', .2, 'data-b="0" data-hot="0"')
g += sword(TH0 + 13, 'data-b="0" data-off="1"', .8, None, "s", ghost=True)
tp, ts = pol(TH0, R_TIP), pol(TH0 + 13, R_TIP)
g += [M(tp[0] - 40, tp[1] - 30, "AS POSED", 'data-b="0" data-off="1"', .5),
      M(ts[0] - 20, ts[1] + 40, "AS SEEN", 'data-b="0" data-off="1"', 1.0)]
# the board: steps aside at beat 2, comes back at beat 4 (two nested moves)
inner = board('data-b="0" data-hot="2"', .1) + Me(BX0 - 20, BY + 8, "HIS SHIELD", 'data-b="0"', .2)
g += ['<g class="el" data-b="0" data-mv="4" %s><g class="el" data-b="0" data-mv="2" %s>%s</g></g>' % (
    st(0, "--mx:%dpx;--my:0px" % -SHIFT), st(.4, "--mx:%dpx;--my:0px" % SHIFT), inner)]
g += [xmark(XC + 14, BY, 'data-b="1" data-off="2"', 1.2, 16)]
a = 'data-b="2" data-off="4"'
nx = BX1 + SHIFT
g += [P("M%.0f %d v-14 H%.0f v14" % (nx, BY - 18, XC), a, "s a", 1.2, "stroke-width:3"), Me(nx - 24, BY - 30, "6 CM LANE", a, 1.3, red=True)]
# sixteen ways, in the board's own plane, seen from behind him
RX, RY, RR = 240, 225, 78
a = 'data-b="3" data-dim="4"'
g.append('<circle class="el f" %s cx="%d" cy="%d" r="30" %s/>' % (a, RX, RY, st(.1)))
for k in range(16):
    ang = k * 22.5
    good = 90 <= ang <= 180
    c, s0 = math.cos(math.radians(ang)), math.sin(math.radians(ang))
    g.append(line([(RX + c * 38, RY + s0 * 38), (RX + c * RR, RY + s0 * RR)], a, "s a" if good else "s thin", .2 + .03 * k,
                  "stroke-width:%d" % (6 if good else 2)))
g += [Me(RX - RR - 12, RY + 8, "OUTWARD", a, .9, red=True),
      Mc(RX, RY + RR + 34, "DOWN", a, .9, red=True),
      M(RX + RR + 14, RY + 8, "ACROSS · COSTS MORE", a, 1.2),
      Mc(RX, RY - RR - 18, "UP · COSTS MORE", a, 1.2),
      SM(60, RY + RR + 80, "16 ways, in its own plane, from behind", a, 1.4, 22)]
a = 'data-b="4"'
g += [Mc((BX0 + BX1) / 2, BY - 34, "BACK IN A QUARTER SECOND", a, 1.0, red=True)]
steps = [(0, "1 · TWO LINES", ["his forearm and his blade,", "as posed and as seen", "seen: up to 57 cm off its pose"]),
         (1, "2 · THE NEXT 0.3 S", ["at the speed it moves now", "a sword at rest: no look ahead"]),
         (2, "3 · THE BOARD STEPS ASIDE", ["the least way of sixteen,", "a lane 6 cm clear of the rim"]),
         (3, "4 · OUTWARD AND DOWN", ["across his chest or up", "before his face cost more"]),
         (4, "5 · BACK IN 1/4 S, SMOOTHLY", ["then his body's guard and", "collision come after, and win"])]
y = 125
for b_, h, ls in steps:
    a = 'data-b="%d"%s' % (b_, "" if b_ == 4 else ' data-dim="%d"' % (b_ + 1))
    g.append(M(TX, y, h, a, .2, red=True))
    for j, s_ in enumerate(ls):
        g.append(SM(TX, y + 40 + 34 * j, s_, a, .4 + .3 * j, 25))
    y += 62 + 34 * len(ls)
S.append(g)

# ================= 3. the numbers
g = [M(60, 70, "RULE OFF AGAINST RULE ON · HEADLESS · THE SAME BUILD", 'data-b="0"')]
a = 'data-b="0" data-dim="3"'
g += [M(60, 130, "THE AIM TEST · ONE FIGHTER THROWS 19 BLOWS AT ANOTHER", a, .1, ink=True)]
for k in range(19):
    g.append(P("M%d 150 l16 22" % (1000 + k * 28), a, "s", .3 + .04 * k, "stroke-width:4"))
g += [R(1000, 190, 30, 20, a, "f", .2), M(1040, 208, "OFF", a, .2),
      R(1130, 190, 30, 20, a, "ink a", .2), M(1170, 208, "ON", a, .2, red=True)]
U = 500  # px per second
X0 = 560
rows = [(1, "HIS BLADE THROUGH HIS SHIELD", 1.30, 0.25, 250, .2),
        (1, "HIS SWORD ARM THROUGH IT", 1.00, 0.10, 350, 1.2)]
for b_, lab, off, on, y, d in rows:
    a = 'data-b="%d" data-dim="3"' % b_
    g += [M(60, y + 50, lab, a, d, ink=True),
          R(X0, y + 10, int(off * U), 30, a, "f", d + .1), M(X0 + off * U + 14, y + 35, "%.2f S" % off, a, d + .1),
          R(X0, y + 48, max(8, int(on * U)), 30, a, "ink a", d + .5), M(X0 + on * U + 14, y + 73, "%.2f S" % on, a, d + .5, red=True)]
a = 'data-b="2" data-dim="3"'
UC = 36
g += [M(60, 490, "LONG CROSSINGS", a, .1, ink=True), SM(60, 528, "a twentieth of a second or more", a, .2, 24),
      R(X0, 460, 13 * UC, 30, a, "f", .2), M(X0 + 13 * UC + 14, 485, "13", a, .2),
      R(X0, 498, 5 * UC, 30, a, "ink a", .5), M(X0 + 5 * UC + 14, 523, "5", a, .5, red=True),
      tick(1110, 495, a, 1.4),
      M(1150, 488, "BLOWS LAND AS BEFORE", a, 1.4, ink=True),
      SM(1150, 528, "met the other's shield: 16 off, 14 on", a, 1.6, 24)]
a = 'data-b="3"'
g += [R(60, 590, 700, 280, a, "f pop", .1),
      M(90, 640, "A FIGHT OF SIX · ANOTHER START", a, .2, ink=True),
      SM(90, 685, "blade and sword arm through his own shield", a, .3, 25),
      BIG(90, 790, "2.4 s", a, .5, 80, strike=True),
      T(300, 780, "›", "big", a + " " + st(.7, "font-size:64px;fill:var(--ink2)")),
      BIG(370, 790, "0.2 s", a, .9, 80, red=True),
      SM(90, 845, "rule off, then on", a, 1.1, 24)]
a = 'data-b="4"'
g += [R(800, 590, 740, 280, a, "f pop", .1, "stroke:var(--signal);stroke-width:4"),
      M(830, 640, "THE MAIN FIGHT OF SIX · RULE ON", a, .2, red=True),
      SM(830, 685, "both shield bearers on their feet, all 40 s", a, .3, 25),
      M(830, 735, "BLADE 0.13 S · SWORD ARM 0.07 S", a, .5),
      M(830, 770, "SHIELD IN HIS OWN BODY 0.07 S", a, .7),
      BIG(1340, 760, "0.3 s", a, 1.0, 64, red=True),
      M(1340, 795, "OF 80", a, 1.1, red=True),
      M(830, 840, "THIS MORNING'S GAME: 4.0 S", a, 1.4, ink=True)]
S.append(g)

# ================= 4. your playtest, the cost, what is left
g = [M(60, 70, "YOUR PLAYTEST OF 19:37 COUNTED ITSELF", 'data-b="0"')]
a = 'data-b="0" data-dim="3"'
g += [BIG(60, 200, "190 s", a, .2, 96),
      M(370, 140, "ABOUT · TWO SWORD AND SHIELD FIGHTERS", a, .4, ink=True),
      M(370, 180, "THIS EVENING'S BUILD · BEFORE THE SHIELD GAVE WAY", a, .6)]
UP = 22
X0 = 560
a = 'data-b="1" data-dim="3"'
g += [M(60, 290, "THE SHIELD IN HIS OWN BODY", a, .1, ink=True),
      R(X0, 245, 36 * UP, 30, a, "f", .2), M(X0 + 36 * UP + 14, 270, "36 · AT 16:41", a, .2),
      R(X0, 283, 7 * UP, 30, a, "ink a", .6), M(X0 + 7 * UP + 14, 308, "7 · ONLY 2 ON HIS FEET", a, .6, red=True),
      SM(60, 360, "this evening's collisions did in Unity what the headless runs said", a, 1.2, 25)]
a = 'data-b="2" data-dim="3"'
g += [M(60, 450, "HIS BLADE THROUGH HIS SHIELD", a, .1, ink=True),
      R(X0, 427, 18 * UP, 30, a, "ink a", .2), M(X0 + 18 * UP + 14, 452, "18", a, .2, red=True),
      M(60, 500, "HIS SWORD ARM THROUGH IT", a, .4, ink=True),
      R(X0, 477, 15 * UP, 30, a, "ink a", .5), M(X0 + 15 * UP + 14, 502, "15", a, .5, red=True),
      M(1060, 465, "13 OF THOSE IN HIS OWN BLOWS", a, .9),
      M(1060, 505, "WHAT TONIGHT'S RULE IS FOR", a, 1.2, red=True)]
a = 'data-b="3"'
g += [R(60, 570, 720, 300, a, "f pop", .1, "stroke:var(--signal);stroke-width:4"),
      M(90, 620, "WHAT IT COSTS", a, .2, red=True),
      SM(90, 670, "in his own blows the board moves aside,", a, .4, 26),
      SM(90, 710, "up to about half a metre", a, .6, 26),
      M(90, 765, "59 CM AT MOST · THE FIGHT OF SIX", a, .9, ink=True),
      SM(90, 830, "for that moment it is not before his chest", a, 1.2, 26)]
a = 'data-b="4"'
g += [R(820, 570, 720, 300, a, "f pop", .1),
      M(850, 620, "NOT COVERED", a, .2, ink=True),
      M(850, 675, "HIS SWORD UPPER ARM", a, .4, ink=True),
      SM(850, 715, "tried and taken out: nothing measurably better", a, .6, 25),
      M(850, 775, "FALLING · DOWN · GETTING UP", a, 1.0, ink=True),
      SM(850, 815, "the rule is for a fighter on his feet", a, 1.2, 25)]
S.append(g)

# ================= 5. where to look, what to say
g = [M(60, 70, "A CORRECTION · WHERE TO LOOK · WHAT TO SAY", 'data-b="0"')]
a = 'data-b="0" data-dim="1"'
g += [R(60, 100, 1480, 210, a, "f pop", .1, "stroke:var(--signal);stroke-width:4"),
      M(90, 150, "A CORRECTION TO THIS EVENING · “MET SOFTLY”", a, .2, red=True),
      SM(90, 198, "it also made the arm give way at once against his body, and held the board in him", a, .4, 26),
      M(90, 250, "IN ONE FIGHT · A BOARD IN ITS BEARER'S BELLY FOR 3 S", a, .8, ink=True),
      M(90, 288, "TAKEN OUT · THE ARM PRESSES WITH A THIRD OF ITS STRENGTH", a, 1.3, red=True)]
a = 'data-b="1" data-dim="3"'
g += [R(60, 340, 940, 530, a, "f", .1),
      M(90, 390, "IN UNITY · DUEL LAB · THE BASH SETTINGS", a, .2, ink=True),
      SM(90, 432, "or type “sword” in the search box", a, .3, 26),
      R(90, 466, 64, 34, a, "ink a", .5, "rx:17"),
      '<circle class="el" %s cx="137" cy="483" r="12" %s/>' % (a, st(.6, "fill:var(--sheet)")),
      SM(180, 492, "His shield and his sword:", a, .6, 27),
      SM(180, 532, "the shield gives way / pass through each other (old)", a, .8, 25)]
a = 'data-b="2" data-dim="3"'
lv = [("SWORD'S LANE", "how far clear", "6 CM", 600), ("SWORD SEEN AHEAD", "how early it opens", "0.3 S", 690)]
for k, (nm, sub, val, y) in enumerate(lv):
    d = .2 + .5 * k
    g += [M(90, y, nm, a, d, ink=True), SM(90, y + 36, sub, a, d + .1, 24),
          P("M420 %d H900" % (y - 8), a, "s thin", d + .1),
          '<circle class="el ink a" %s cx="560" cy="%d" r="13" %s/>' % (a, y - 8, st(d + .2)),
          Mc(560, y + 34, val, a, d + .3, red=True)]
g += [M(90, 800, "THROW A RISING CUT AND A FOREHAND", a, 1.3, ink=True),
      SM(90, 842, "and run a dance, with it on and off", a, 1.5, 25)]
a = 'data-b="3"'
g += [M(1040, 375, "WHAT YOU SAY NEXT", a, .1, red=True)]
qs = [("1", ["does the opening look right,", "or too far, or too early?", "THE TWO LEVERS"], 400),
      ("2", ["what next: his sword upper arm,", "the fallen and getting-up body,", "or attacks checked as built"], 640)]
for k, (n_, ls, y0) in enumerate(qs):
    d = .3 + 1.4 * k
    g += [R(1040, y0, 500, 220, a, "f pop", d, "stroke:var(--signal);stroke-width:4"),
          BIG(1060, y0 + 85, n_, a, d + .1, 60, red=True)]
    for j, s_ in enumerate(ls):
        if s_.isupper():
            g.append(M(1110, y0 + 55 + 48 * j, s_, a, d + .3 + .2 * j, ink=True))
        else:
            g.append(SM(1110, y0 + 55 + 44 * j, s_, a, d + .3 + .2 * j, 25))
S.append(g)

names_ = ["what you said", "why through", "the rule", "the numbers", "playtest, cost, left", "where to look"]
svg = ""
for i, gg in enumerate(S):
    svg += "  <!-- %d. %s -->\n  <g class=\"scene\">\n    %s\n  </g>\n\n" % (i, names_[i], "\n    ".join(gg))
(out / "scenes.svg").write_text(svg, encoding="utf-8")
exec((out / "script.py").read_text(encoding="utf-8"))
(out / "data.json").write_text(json.dumps(data, ensure_ascii=False, indent=1), encoding="utf-8")
nb_ = sum(len(s["beats"]) for s in data["scenes"])
nw = sum(len(b.split()) for s in data["scenes"] for b in s["beats"])
secs = sum(max(2.8, len(b.split()) * .345 + .7) for s in data["scenes"] for b in s["beats"])
print("ok", len(S), nb_, nw, "timer %.0f s" % secs, "shift %.0f xc %.0f" % (SHIFT, XC))
for i, s in enumerate(data["scenes"]):
    assert 3 <= len(s["beats"]) <= 5, i
assert len(S) == len(data["scenes"])
for i, gg in enumerate(S):
    mx = max(int(v) for x in gg for v in re.findall(r'data-(?:b|off|dim|acc|hot|mv)="(\d+)"', x))
    assert mx < len(data["scenes"][i]["beats"]), (i, mx)
note = (out / "note.md").read_text(encoding="utf-8")
for s in data["scenes"]:
    for b in s["beats"]:
        for n_ in re.findall(r"\d+(?:\.\d+)?", b):
            if n_ not in note:
                print("NUMBER NOT IN NOTE:", n_, "|", b)
vis = " ".join(re.findall(r">([^<]+)<", svg))
for n_ in set(re.findall(r"\d+(?:\.\d+)?", vis)):
    if n_ not in note:
        print("STAGE NUMBER NOT IN NOTE:", n_)
