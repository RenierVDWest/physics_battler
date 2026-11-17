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
    # (The plan "Fight Rhythm from Recordings", step 2: the jogs, recorded with empty hands, for the pace between the
    # walk and the run. Their legs are put under the guard's own chest and arms: MoveStyle.)
    # (A jump on the spot, empty hands: the crouch, the push, the flight, the landing. What a leap is made of: LeapMaker.)
    'jump_spot': ('jump_far', 'jump', False, 'A jump on the spot, empty hands'),
    'jog_fwd': ('jog_forward', 'cycle', True, 'Jogging forward, empty hands'),
    'jog_back': ('jog_backward', 'cycle', True, 'Jogging backward, empty hands'),
    'jog_left': ('jog_strafe_left', 'cycle', True, 'Jogging to the left, empty hands'),
    'jog_right': ('jog_strafe_right', 'cycle', True, 'Jogging to the right, empty hands'),
    # (A slow walk to the side, recorded with empty hands (16 November, the pack "Slow walks"): for circling on at a
    # pace well under the guard's own side-step. Its legs are put under the guard's own chest and arms, as the jogs'
    # are: MoveStyle. It is mirrored for the left (SLOW_MIRRORED). The pack's other takes are not slower than what
    # the guard has: its sneaks forward cover 2.0 to 2.6 m/s. Its crouched sneaks to the side go 1.1 m/s, as the
    # guard's side-steps do, but low and with each foot longer on the ground: the other choice for a slow circle.)
    'slow_right': ('walk_strafe_right_1', 'cycle', True, 'Walking slowly to the right, empty hands'),
    'sneak_left': ('crouched_sneaking_left', 'cycle', True, 'Stepping carefully to the left, crouched, empty hands'),
    'sneak_right': ('crouched_sneaking_right', 'cycle', True, 'Stepping carefully to the right, crouched, empty hands'),
    # (The plan "Convincing walking in the dance": legs for the guard's steps, picked by tools/pack_survey.py from the
    # packs of 17 November for how near their lower body is to the guard's own stance (hips turned 55 to 70 degrees
    # from where he looks, the left foot ahead, low). None is used as it is: MoveStyle puts a take's legs under the
    # guard's own chest and arms, on the guard's carriage. The bow's walks keep their hips turned 63 to 75 degrees
    # whichever way they go, as the guard's own side-steps do: its walk forward is the advance the sword and shield
    # pack lacks, its walk back the retreat.)
    'bow_walk_fwd': ('bow_walk_forward', 'cycle', True, 'An archer walking toward where he aims, his hips turned 70 degrees from it (Longbow Locomotion Pack)'),
    'bow_walk_back': ('bow_walk_back', 'cycle', True, 'An archer walking back from where he aims, his hips turned 70 degrees'),
    'bow_walk_left': ('bow_walk_left', 'cycle', True, 'An archer walking to his left of where he aims'),
    'bow_walk_right': ('bow_walk_right', 'cycle', True, 'An archer walking to his right of where he aims'),
    'axe_walk_fwd': ('axe_walk_forward', 'cycle', True, 'An axeman in his stance walking forward (Pro Melee Axe Pack)'),
    'axe_walk_back': ('axe_walk_back', 'cycle', True, 'An axeman in his stance walking back'),
    'axe_walk_left': ('axe_walk_left', 'cycle', True, 'An axeman in his stance walking left'),
    'axe_walk_right': ('axe_walk_right', 'cycle', True, 'An axeman in his stance walking right'),
    'magic_walk_fwd': ('magic_walk_forward', 'cycle', True, 'A caster in his stance walking forward (Magic Locomotion Pack)'),
    'magic_walk_back': ('magic_walk_back', 'cycle', True, 'A caster in his stance walking back'),
    'magic_walk_left': ('magic_walk_left', 'cycle', True, 'A caster in his stance walking left'),
    'magic_walk_right': ('magic_walk_right', 'cycle', True, 'A caster in his stance walking right'),
    'rifle_walk_fwd': ('rifle_walk_forward', 'cycle', True, 'A rifleman aiming, walking forward (Rifle 8-Way Locomotion Pack)'),
    'rifle_walk_fwd_left': ('rifle_walk_forward_left', 'cycle', True, 'A rifleman aiming, walking forward and left'),
    'rifle_walk_fwd_right': ('rifle_walk_forward_right', 'cycle', True, 'A rifleman aiming, walking forward and right'),
    'rifle_walk_left': ('rifle_walk_left', 'cycle', True, 'A rifleman aiming, walking left'),
    'rifle_walk_right': ('rifle_walk_right', 'cycle', True, 'A rifleman aiming, walking right'),
    'rifle_walk_back': ('rifle_walk_backward', 'cycle', True, 'A rifleman aiming, walking back'),
    'rifle_walk_back_left': ('rifle_walk_backward_left', 'cycle', True, 'A rifleman aiming, walking back and left'),
    'rifle_walk_back_right': ('rifle_walk_backward_right', 'cycle', True, 'A rifleman aiming, walking back and right'),
    # (The plan "Fight Rhythm from Recordings", step 4: the ginga, capoeira's base, recorded with empty hands. A stand
    # that steps: on the spot, one foot back and across and then the other, the hips swaying under it. The walk
    # stands in it as it stands in a guard's idle, its feet's steps the recording's own: MoveStyle.Capoeira.)
    'cap_ginga': ('capoeira', 'guard_idle', True, 'The ginga, on the spot (3.4 s: a step back to each side)'),
    'cap_ginga_b': ('ginga_variation_3', 'guard_idle', True, 'The ginga, on the spot, a quieter take (2.9 s)'),
    'cap_ginga_c': ('ginga_variation_1', 'guard_idle', True, 'The ginga, on the spot, a lower take with a turn of the body (3.5 s)'),
    'cap_ginga_d': ('ginga_variation_2', 'guard_idle', True, 'The ginga, on the spot, a lower take (3.8 s)'),
    'sns_turn_left': ('sword_and_shield_turn', 'turn', False, 'Medium guard, turning left on the spot'),
    'sns_turn_right': ('sword_and_shield_turn_b', 'turn', False, 'Medium guard, turning right on the spot'),
    'sns_turn180': ('sword_and_shield_180_turn', 'turn', False, 'Medium guard, turning round'),
    'sns_turn180_b': ('sword_and_shield_180_turn_1', 'turn', False, 'Medium guard, turning round the other way'),
    'sns_block': ('sword_and_shield_block', 'block', False, 'Medium shield block'),
    # (Being struck and blocking, from the Pro Sword and Shield pack: in place, from the guard and back to it.)
    'sns_impact_light': ('pack_sns_impact', 'impact', False, 'Medium struck lightly: the head and chest flinch back'),
    'sns_impact_body': ('pack_sns_impact_2', 'impact', False, 'Medium struck in the body: it doubles over'),
    'sns_impact_heavy': ('pack_sns_impact_3', 'impact', False, 'Medium struck hard: the upper body thrown back and to its right'),
    'sns_block_raise': ('pack_sns_block', 'block_raise', False, 'Medium raising its shield into a block, from the guard'),
    'sns_block_hold': ('pack_sns_block_idle', 'block_hold', True, 'Medium holding its shield up in a block'),
    'sns_block_lower': ('pack_sns_block_2', 'block_lower', False, 'Medium lowering its shield from a block back to the guard'),
    'lg_idle_b': ('large_great_sword_idle_02', 'guard_idle', True, 'Large two-handed guard, standing (short take)'),
    'lg_walk': ('large_great_sword_walk', 'cycle', True, 'Large two-handed guard, walking forward'),
    'lg_step_left': ('large_great_sword_strafe', 'cycle', True, 'Large two-handed guard, side-stepping left'),
    'lg_step_right': ('large_great_sword_strafe_02', 'cycle', True, 'Large two-handed guard, side-stepping right'),
}

# The sword-alone guard (the sword-only dummy, and any unit without its shield): the sword and shield guard
# mirrored, right foot forward, the trunk turned sword side to the opponent as a sabre fencer's (its arms are
# posed by the parry). name: (guard clip it mirrors)
SWORD_GUARD = {
    'sw_idle': 'sns_idle', 'sw_idle_b': 'sns_idle_b', 'sw_idle_c': 'sns_idle_c',
    'sw_walk': 'sns_walk', 'sw_walk_back': 'sns_walk_back',
    'sw_step_left': 'sns_step_right', 'sw_step_right': 'sns_step_left',
    'sw_run': 'sns_run', 'sw_run_back': 'sns_run_back',
    'sw_turn_left': 'sns_turn_right', 'sw_turn_right': 'sns_turn_left',
    'sw_turn180': 'sns_turn180_b', 'sw_turn180_b': 'sns_turn180',
}

# Hit reactions by the side the blow came from (Renier's Mixamo clips, 6 November: the plan "Fighting off several
# attackers", step 6), unarmed, in place, back to where they began. name: (sample, about); each hit_left_ one is
# mirrored into a hit_right_ one. (Read off the clips: facing +z, the head thrown to its right (+x) = a blow from its left.)
HITS = {
    'hit_body': ('hit_to_body', 'Struck in the body from in front: doubles over, the hips back'),
    'hit_body_b': ('hit_to_body_1', 'Struck in the body from in front: doubles over (second take)'),
    'hit_body_c': ('hit_to_body_2', 'Struck in the body from in front: doubles over (third take)'),
    'hit_rib': ('rib_hit', 'Struck lightly in the body from in front: bends a little'),
    'hit_left_head': ('head_hit', 'Struck on the head from its front left: the head thrown back and to its right'),
    'hit_left_head_big': ('big_hit_to_head_1', 'Struck hard on the head from its front left: thrown back, right and down'),
    'hit_left_head_big_b': ('big_hit_to_head_2', 'Struck hard on the head from its front left (second take)'),
    'hit_left_rib': ('big_rib_hit', 'Struck hard in the ribs from its left: bent over to its right'),
    'hit_back_head': ('big_hit_to_head', 'Struck from behind: the head thrown forward and down, the hips back'),
}

# Strikes: name: (sample, from s, to s, contact s, kind, about)
# (The Large holds its club two-handed: only the two-handed clips are its references. Its one-handed club combo
# and the great sword idles whose hands come apart were taken out.)
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
}


# The slow walks mirrored left to right. name: (the clip it mirrors)
SLOW_MIRRORED = {'slow_left': 'slow_right'}

# (Sets shot around where he aims: a take's own front is where he looks, whatever his head and chest do.)
FRONT_SETS = {'bow', 'axe', 'magic', 'rifle'}


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
        clips.append(bake_one(name, d, B, F, loop, role, about, yaw_from='front' if name.split('_')[0] in FRONT_SETS else 'hips' if role in ('turn', 'block') else 'head'))
    for name, of in SLOW_MIRRORED.items():
        sample, role, loop, about = GUARD[of]
        d, B, F = load(sample)
        lb.LOOPS.discard(name)
        if loop:
            lb.LOOPS.add(name)
        c = lb.bake(name, dict(d, about=about.replace('right', 'left') + ' (mirrored)'), B, lb.mirror(B, F), mirrored_of=of, yaw_from='head')
        c['role'] = role
        c['kind'] = ''
        c['set'] = 'guard'
        clips.append(c)
    for name, of in SWORD_GUARD.items():
        sample, role, loop, about = GUARD[of]
        d, B, F = load(sample)
        lb.LOOPS.discard(name)
        if loop:
            lb.LOOPS.add(name)
        dd = dict(d, about=about.replace('Medium guard', 'Medium sword guard (mirrored: right foot forward)'))
        c = lb.bake(name, dd, B, lb.mirror(B, F), mirrored_of=of, yaw_from='hips' if role in ('turn', 'block') else 'head')
        c['role'] = role
        c['kind'] = ''
        c['set'] = 'guard'
        clips.append(c)
    for name, (sample, about) in HITS.items():
        d, B, F = load(sample)
        clips.append(bake_one(name, d, B, F, False, 'impact', about, 'hit', yaw_from='hips'))
        if name.startswith('hit_left_'):
            mirrored = name.replace('hit_left_', 'hit_right_')
            dd = dict(d, about=about.replace('its front left', 'its front right').replace('its left', 'its right').replace('to its right', 'to its left') + ' (mirrored)')
            c = lb.bake(mirrored, dd, B, lb.mirror(B, F), mirrored_of=name, yaw_from='hips')
            c['role'] = 'impact'
            c['kind'] = 'hit'
            c['set'] = 'guard'
            clips.append(c)
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
