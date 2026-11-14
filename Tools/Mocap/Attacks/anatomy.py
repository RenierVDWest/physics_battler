"""Each strike clip's parts, measured against its contact: python3 anatomy.py [clip ...]"""
import sys
import numpy as np
from attack_lib import *


def course(x, t, ic):
    """A channel through a blow: at the start, wound up (its furthest from where it is at contact, before it), at contact,
    carried through (its furthest on, after it), at the end; when it is a tenth, half and nine tenths of the way from
    wound up to carried through; and its fastest."""
    xc = x[ic]
    before = x[:ic + 1]
    iw = int(np.argmax(np.abs(before - xc)))
    way = np.sign(xc - x[iw]) or 1.0
    after = x[ic:]
    k = int(np.argmax((after - xc) * way))
    # (No further than where it turns back.)
    jf = ic + k
    lo, hi = x[iw], x[jf]
    seg = (x[iw:jf + 1] - lo) / (hi - lo) if abs(hi - lo) > 1e-9 else np.zeros(jf - iw + 1)
    def when(share):
        idx = np.where(seg >= share)[0]
        if len(idx) == 0:
            return float('nan')
        j = idx[0]
        if j == 0:
            return t[iw]
        a, b = seg[j - 1], seg[j]
        return t[iw + j - 1] + (share - a) / max(1e-9, b - a) * (t[iw + j] - t[iw + j - 1])
    v = np.gradient(x) / np.gradient(t)
    iv = iw + int(np.argmax(np.abs(v[iw:jf + 1]))) if jf > iw else iw
    return dict(start=x[0], wound=lo, t_wound=t[iw], contact=xc, on=hi, t_on=t[jf], end=x[-1], t10=when(0.1), t50=when(0.5), t90=when(0.9), fastest=v[iv], t_fast=t[iv], travel=hi - lo)


def steps(c, ch, side):
    """A foot's times off the ground: when it leaves, when it is down again, how far it went and how high."""
    down = ch[side + '_down']
    an = ch[side + '_ankle']
    tc = c.tc()
    out = []
    i = 0
    while i < c.n:
        if not down[i]:
            j = i
            while j < c.n and not down[j]:
                j += 1
            a, b = max(0, i - 1), min(c.n - 1, j)
            out.append((tc[i], tc[b] if j < c.n else float('nan'), an[b] - an[a], float(np.max(an[i:j, 1]) - an[a, 1])))
            i = j
        else:
            i += 1
    return out


def report(name):
    c = Clip(name); ch = channels(c); tc = c.tc(); ic = c.contact
    print(f"\n=== {name}: {c.about} | {c.n / c.rate:.2f} s, contact {ic / c.rate:.2f} s in, {(c.n - 1 - ic) / c.rate:.2f} s after")
    rows = [('pelvis forward (m)', ch['pelvis'][:, 2]), ('pelvis to its right (m)', ch['pelvis'][:, 0]), ('pelvis height (m)', ch['pelvis'][:, 1]),
            ('pelvis turn (deg)', ch['pelvis_yaw']), ('chest turn (deg)', ch['chest_yaw']), ('twist chest on pelvis', ch['twist']),
            ('chest lean forward', ch['chest_pitch']), ('chest lean to its right', ch['chest_roll']),
            ('upper arm raised', ch['r_raised']), ('upper arm toward (0 fwd, 90 out)', ch['r_toward']), ('elbow bend', ch['r_elbow']), ('blade off the forearm', ch['cock']),
            ('front knee bend', ch['l_knee']), ('rear knee bend', ch['r_knee']), ('head turn', ch['head_yaw'])]
    print(f"{'':34} start  wound(@t)     contact  on(@t)        end   | 10%    50%    90%   | fastest(@t)")
    for label, x in rows:
        k = course(x, tc, ic)
        f = '{:6.2f}' if 'm)' in label else '{:6.0f}'
        print(f"{label:34} {f.format(k['start'])} {f.format(k['wound'])}({k['t_wound']:+.2f}) {f.format(k['contact'])} {f.format(k['on'])}({k['t_on']:+.2f}) {f.format(k['end'])} | {k['t10']:+.2f}  {k['t50']:+.2f}  {k['t90']:+.2f} | {k['fastest']:7.1f}({k['t_fast']:+.2f})")
    for label, x in (('hand speed (m/s)', speed(ch['r_hand'], c.rate)), ('tip speed (m/s)', speed(ch['tip'], c.rate)), ('pelvis speed (m/s)', speed(ch['pelvis'], c.rate))):
        i = int(np.argmax(x))
        print(f"{label:34} fastest {x[i]:5.1f} at {tc[i]:+.2f}; at contact {x[ic]:5.1f}")
    for side, nm in (('l', 'left foot (front in guard)'), ('r', 'right foot (rear in guard)')):
        an = ch[side + '_ankle']
        s = '; '.join(f"off {a:+.2f} to {b:+.2f}: {d[2]:+.2f} fwd {d[0]:+.2f} right, {h:.2f} up" for a, b, d, h in steps(c, ch, side))
        heel = ch[side + '_heel_down']; toe = ch[side + '_toe_down']
        only_toe = [tc[i] for i in range(c.n) if toe[i] and not heel[i]]
        print(f"{nm:34} at {an[0, 2]:+.2f} fwd {an[0, 0]:+.2f} right; {s or 'never off the ground'}{'; on its toes ' + f'{only_toe[0]:+.2f} to {only_toe[-1]:+.2f}' if only_toe else ''}")
    b = ch['blade']; s = ch['strike']
    v = np.gradient(s, axis=0) * c.rate
    print(f"{'blade at contact':34} points {b[ic, 0]:+.2f} right {b[ic, 1]:+.2f} up {b[ic, 2]:+.2f} fwd; striking part at {s[ic, 0]:+.2f} right {s[ic, 1]:.2f} up {s[ic, 2]:+.2f} fwd, going {unit(v[ic])[0]:+.2f} right {unit(v[ic])[1]:+.2f} up {unit(v[ic])[2]:+.2f} fwd at {np.linalg.norm(v[ic]):.1f} m/s")
    print(f"{'weight at contact':34} {ch['com'][ic, 2] - ch['com'][0, 2]:+.2f} m forward of where it began, {ch['com'][ic, 1] - ch['com'][0, 1]:+.2f} m up; front foot {ch['l_ankle'][ic, 2] - ch['com'][ic, 2]:+.2f} m ahead of it, rear foot {ch['r_ankle'][ic, 2] - ch['com'][ic, 2]:+.2f}")


if __name__ == '__main__':
    for n in (sys.argv[1:] or STRIKES):
        report(n)
