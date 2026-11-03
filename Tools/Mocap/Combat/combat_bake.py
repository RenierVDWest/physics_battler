#!/usr/bin/env python3
"""Step 2 of the combat clips: bakes the sampled guard and strike clips (combat_sample.py) the way the
locomotion's baker does (Tools/Mocap/Locomotion/loco_bake.py: root, bones against it in leg lengths,
foot contacts, a loop's gait phase), into Assets/_Battler/Resources/Locomotion/CombatClips.json, which the
game's clip library reads beside the locomotion's.

    python3 Tools/Mocap/Combat/combat_bake.py [--report]

The guard clips are baked whole. Each strike is cut from its clip (combo clips are only material: each
strike in them is its own clip), with its marks: markers = [contact frame, link frame] within the cut,
and its kind and height in `kind`. The cut times below come from the weapon tip's speed (a strike's
contact is its peak; the cuts sit at the quiet moments between), checked by eye in the films.
"""
import json, math, os, sys
import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(os.path.dirname(HERE), 'Locomotion'))
import loco_bake as lb

ROOT = os.path.abspath(os.path.join(HERE, '..', '..', '..'))
OUT = os.path.join(ROOT, 'Assets', '_Battler', 'Resources', 'Locomotion', 'CombatClips.json')

# Guard clips: name: (sample, role, loop, about)
GUARD = {
    'sns_idle': ('sword_and_shield_idle_1', 'guard_idle', True, 'Medium guard, standing (8.7 s take)'),
    'sns_idle_b': ('sword_and_shield_idle', 'guard_idle', True, 'Medium guard, standing (2.5 s take)'),
    'sns_idle_c': ('sword_and_shield_idle_2', 'guard_idle', True, 'Medium guard, standing (3.7 s take)'),
    'sns_walk': ('sword_and_shield_walk', 'cycle', True, 'Medium guard, walking forward'),
    'sns_walk_back': ('sword_and_shield_walk_backwards_02', 'cycle', True, 'Medium guard, walking backward'),
    'sns_step_left': ('sword_and_shield_strafe_1', 'cycle', True, 'Medium guard, side-stepping left'),
    'sns_step_right': ('sword_and_shield_strafe_2', 'cycle', True, 'Medium guard, side-stepping right'),
    'sns_run': ('sword_and_shield_run', 'cycle', True, 'Medium guard, running forward'),
    'sns_run_back': ('sword_and_shield_run_backwards', 'cycle', True, 'Medium guard, running backward'),
    'sns_turn_left': ('sword_and_shield_turn', 'turn', False, 'Medium guard, turning left on the spot'),
    'sns_turn_right': ('sword_and_shield_turn_b', 'turn', False, 'Medium guard, turning right on the spot'),
    'sns_turn180': ('sword_and_shield_180_turn', 'turn', False, 'Medium guard, turning round'),
    'sns_turn180_b': ('sword_and_shield_180_turn_1', 'turn', False, 'Medium guard, turning round the other way'),
    'sns_block': ('sword_and_shield_block', 'block', False, 'Medium shield block'),
    'lg_idle': ('large_great_sword_idle', 'guard_idle', True, 'Large two-handed guard, standing'),
    'lg_idle_b': ('large_great_sword_idle_02', 'guard_idle', True, 'Large two-handed guard, standing (short take)'),
    'lg_idle_c': ('large_great_sword_idle_03', 'guard_idle', True, 'Large two-handed guard, standing (third take)'),
    'lg_walk': ('large_great_sword_walk', 'cycle', True, 'Large two-handed guard, walking forward'),
    'lg_step_left': ('large_great_sword_strafe', 'cycle', True, 'Large two-handed guard, side-stepping left'),
    'lg_step_right': ('large_great_sword_strafe_02', 'cycle', True, 'Large two-handed guard, side-stepping right'),
}

# Strikes: name: (sample, from s, to s, contact s, kind, about)
STRIKES = {
    'sns_forehand': ('sword_and_shield_slash_3', 0.0, 1.50, 0.63, 'forehand high', 'Medium forehand cut, diagonal down, in place'),
    'sns_rising': ('sword_and_shield_slash_1', 0.0, 1.67, 0.83, 'rising middle', 'Medium rising backhand cut, in place'),
    'sns_thrust': ('sword_and_shield_attack_2', 0.0, 1.00, 0.48, 'thrust high', 'Medium high thrust over the shield, in place'),
    'sns_backhand_lunge': ('sword_and_shield_attack_1', 0.0, 1.73, 0.77, 'backhand middle', 'Medium stepping backhand, a long lunge'),
    'sns_chop_side': ('sword_and_shield_slash_2', 0.0, 2.43, 1.40, 'downward high', 'Medium overhead chop with a side step'),
    'sns_chop_lunge': ('sword_and_shield_attack_3', 0.0, 2.33, 1.17, 'downward high', 'Medium overhead chop with a lunge'),
    'sns_combo_forehand': ('sword_and_shield_slash', 0.0, 1.05, 0.67, 'forehand high', 'Medium forehand, from the four-cut combo'),
    'sns_combo_backhand': ('sword_and_shield_slash', 1.05, 1.37, 1.23, 'backhand middle', 'Medium backhand, from the four-cut combo (cut before the leap: both feet down)'),
    'sns_combo_chop': ('sword_and_shield_slash', 1.37, 3.53, 2.47, 'downward high', 'Medium leaping spin chop, from the four-cut combo (from both feet down to standing again)'),
    'lg_swing': ('large_great_sword_slash_02', 0.0, 1.83, 0.87, 'forehand middle', 'Large heavy two-handed swing, in place'),
    'lg_slash_a': ('large_great_sword_slash', 0.0, 1.25, 0.80, 'forehand high', 'Large two-handed strike, advancing (first of three)'),
    'lg_slash_b': ('large_great_sword_slash', 1.25, 2.15, 1.73, 'backhand middle', 'Large two-handed strike, advancing (second of three)'),
    'lg_slash_c': ('large_great_sword_slash', 2.15, 3.53, 2.60, 'backhand low', 'Large two-handed strike, advancing (third of three)'),
    'lg_club_a': ('large_two_hand_club_combo', 0.0, 2.45, 1.65, 'downward high', 'Large two-handed club swing (first of three)'),
    'lg_club_b': ('large_two_hand_club_combo', 2.45, 3.50, 2.87, 'forehand middle', 'Large two-handed club swing (second of three)'),
    'lg_club_c': ('large_two_hand_club_combo', 3.50, 5.43, 3.93, 'backhand middle', 'Large two-handed club swing (third of three)'),
    'lg_jab': ('large_one_hand_club_combo', 0.0, 1.00, 0.67, 'thrust middle', 'Large one-handed club jab'),
    'lg_smash_a': ('large_one_hand_club_combo', 1.00, 1.90, 1.30, 'downward high', 'Large one-handed club smash'),
    'lg_smash_b': ('large_one_hand_club_combo', 1.90, 2.55, 2.30, 'downward high', 'Large one-handed club smash, the other way'),
    'lg_smash_c': ('large_one_hand_club_combo', 2.55, 3.63, 2.83, 'forehand middle', 'Large one-handed club swing to finish'),
}


def load(sample):
    with open(os.path.join(HERE, 'samples', sample + '.json')) as f:
        d = json.load(f)
    B = d['bones']
    F = np.array(d['frames'], float).reshape(len(d['frames']), len(B), 3)
    return d, B, F


def bake_one(name, d, B, F, loop, role, about, kind='', markers=None, yaw_from='head'):
    lb.LOOPS.discard(name)
    if loop:
        lb.LOOPS.add(name)
    d = dict(d, about=about)
    c = lb.bake(name, d, B, F, yaw_from=yaw_from)
    c['role'] = role
    c['kind'] = kind
    c['set'] = 'guard'
    if markers is not None:
        c['markers'] = markers
    return c


def main(report):
    clips = []
    bones = None
    for name, (sample, role, loop, about) in GUARD.items():
        d, B, F = load(sample)
        bones = B
        clips.append(bake_one(name, d, B, F, loop, role, about, yaw_from='hips' if role in ('turn', 'block') else 'head'))
    for name, (sample, a, b, contact, kind, about) in STRIKES.items():
        d, B, F = load(sample)
        rate = d['rate']
        i0, i1 = int(round(a * rate)), min(len(F) - 1, int(round(b * rate)))
        hit = int(round(contact * rate)) - i0
        # (A next strike may follow from a third of the way from contact to the end.)
        link = hit + max(1, (i1 - i0 - hit) // 3)
        clips.append(bake_one(name, d, B, F[i0:i1 + 1], False, 'strike', about, kind, [hit, link], yaw_from='hips'))
    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    with open(OUT, 'w') as f:
        json.dump(dict(about='Combat clips baked by Tools/Mocap/Combat/combat_bake.py; points in leg lengths against the root (x right, y up, z forward).',
                       bones=bones, clips=clips), f, separators=(',', ':'))
    print(f'{len(clips)} clips, {sum(c["frames"] for c in clips)} frames, {os.path.getsize(OUT) / 1e6:.2f} MB -> {OUT}')
    if report:
        for c in clips:
            con = np.array(c['contacts']).reshape(-1, 2)
            down = (con & 4) > 0
            print(f"{c['name']:20s} {c['role']:10s} {c['duration']:5.2f}s speed {c['speed'] * c['legLength']:4.2f} m/s dir {c['direction']:+6.1f} "
                  f"turn {c['turn']:+6.1f}; down L {down[:, 0].mean() * 100:3.0f}% R {down[:, 1].mean() * 100:3.0f}%; markers {c['markers']} {c.get('kind', '')}")


if __name__ == '__main__':
    main('--report' in sys.argv)
