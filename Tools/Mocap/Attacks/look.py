import sys, numpy as np
from attack_lib import *
name = sys.argv[1] if len(sys.argv) > 1 else 'sns_forehand'
every = int(sys.argv[2]) if len(sys.argv) > 2 else 2
c = Clip(name); ch = channels(c); tc = c.tc()
print(name, c.about, '| frames', c.n, 'contact', c.contact, 'link', c.link)
print(' t     | pelvis z    x    y  yaw pit rol | chest yaw pit rol twist | Rarm rais towd elb cock | bladeY  tipv | Lft z   y  kn dn | Rft z   y  kn dn | head')
tipv = speed(ch['tip'], c.rate)
for i in range(0, c.n, every):
    f = lambda b: ('H' if ch[b+'_heel_down'][i] else '-') + ('T' if ch[b+'_toe_down'][i] else '-')
    print(f"{tc[i]:+.2f}{'*' if i==c.contact else ' '} | {ch['pelvis'][i,2]:+.2f} {ch['pelvis'][i,0]:+.2f} {ch['pelvis'][i,1]:.2f} {ch['pelvis_yaw'][i]:+4.0f} {ch['pelvis_pitch'][i]:+3.0f} {ch['pelvis_roll'][i]:+3.0f} | "
          f"{ch['chest_yaw'][i]:+4.0f} {ch['chest_pitch'][i]:+3.0f} {ch['chest_roll'][i]:+3.0f} {ch['twist'][i]:+4.0f} | {ch['r_raised'][i]:4.0f} {ch['r_toward'][i]:+4.0f} {ch['r_elbow'][i]:3.0f} {ch['cock'][i]:3.0f} | "
          f"{ch['blade'][i,1]:+.2f} {tipv[i]:5.1f} | {ch['l_ankle'][i,2]:+.2f} {ch['l_ankle'][i,1]:.2f} {ch['l_knee'][i]:3.0f} {f('l')} | {ch['r_ankle'][i,2]:+.2f} {ch['r_ankle'][i,1]:.2f} {ch['r_knee'][i]:3.0f} {f('r')} | {ch['head_yaw'][i]:+4.0f}")
