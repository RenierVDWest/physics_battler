#!/usr/bin/env python3
"""Step 1 of the locomotion clips: samples Mixamo FBX clips (binary, skeleton animation) into small
sample files, so the 18 MB "with skin" exports never need to be kept.

    python3 Tools/Mocap/Locomotion/loco_sample.py <folder with the FBX files>

The manifest below names each clip the game uses and the file it comes from (matched by the end
of the file name). Each clip is posed at 30 frames a second over its whole length and the world
positions of the bones the game reads are written, in Unity's axes (the FBX's x mirrored), in
metres, to Tools/Mocap/Locomotion/samples/<clip>.json.
"""
import json, os, sys
import numpy as np

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(HERE))
import mixamo_rhythm as mr

RATE = 30

BONES = ['Hips', 'Spine', 'Spine1', 'Spine2', 'Neck', 'Head', 'HeadTop_End', 'LeftEye', 'RightEye',
         'LeftShoulder', 'LeftArm', 'LeftForeArm', 'LeftHand', 'LeftHandIndex1', 'LeftHandMiddle1', 'LeftHandPinky1',
         'RightShoulder', 'RightArm', 'RightForeArm', 'RightHand', 'RightHandIndex1', 'RightHandMiddle1', 'RightHandPinky1',
         'LeftUpLeg', 'LeftLeg', 'LeftFoot', 'LeftToeBase', 'LeftToe_End',
         'RightUpLeg', 'RightLeg', 'RightFoot', 'RightToeBase', 'RightToe_End']

# clip id: (file name ending, what it is)
MANIFEST = {
    'idle': ('pack_idle.fbx', 'standing idle (the locomotion pack)'),
    'walk_fwd': ('19650d4c-Walking.fbx', 'forward walk, 1.67 m/s (the same as the pack walking)'),
    'walk_fwd_heavy': ('ed601e13-Walking_1.fbx', 'forward walk, 1.08 m/s, deep hips'),
    'walk_fwd_swagger': ('2b25aa76-Brutal_To_Happy_Walking.fbx', 'forward walk, 1.16 m/s, a swagger'),
    'walk_back': ('91f3c6ec-Walking_Backward.fbx', 'backward walk, 1.20 m/s'),
    'walk_back_2': ('257a81da-Walking_Backwards.fbx', 'backward walk, 1.13 m/s'),
    'walk_left_cross': ('pack_left_strafe_walking.fbx', 'crossover walk to the left, 1.73 m/s'),
    'walk_right_cross': ('pack_right_strafe_walking.fbx', 'crossover walk to the right, 1.73 m/s'),
    'walk_left_step': ('aadfdd72-Walk_Strafe_Left.fbx', 'side-steps to the left, 0.61 m/s'),
    'walk_right_step': ('bf2fee82-Walk_Strafe_Right.fbx', 'side-steps to the right, 0.61 m/s'),
    'run_fwd': ('pack_running.fbx', 'forward run, 4.4 m/s'),
    'run_left': ('pack_left_strafe.fbx', 'running to the left, 4.5 m/s'),
    'run_right': ('pack_right_strafe.fbx', 'running to the right, 4.5 m/s'),
    'start_walk': ('bca772a5-Start_Walking.fbx', 'standing to walking'),
    'stop_walk': ('c25c5d10-Stop_Walking.fbx', 'walking to standing'),
    'turn180_walk': ('88c71755-Walking_Turn_180.fbx', 'a walking 180 pivot'),
    'turn180_run': ('2cca589e-Running_Turn_180.fbx', 'a running 180 pivot'),
    'turn90_left': ('pack_left_turn_90.fbx', 'a 90 turn on the spot, left'),
    'turn90_right': ('pack_right_turn_90.fbx', 'a 90 turn on the spot, right'),
    'turn180_left': ('pack_left_turn.fbx', 'a 180 turn on the spot, left'),
    'turn180_right': ('pack_right_turn.fbx', 'a 180 turn on the spot, right'),
    'walkturn90_left': ('3c4e5c54-Left_Turn_W_Briefcase.fbx', 'a walking 90 turn, left (briefcase in hand)'),
    'walkturn90_right': ('9b460e34-Right_Turn_W__Briefcase.fbx', 'a walking 90 turn, right (briefcase in hand)'),
    'turn_to_run': ('47f60586-Turn_To_Running.fbx', 'a turn into a run'),
    'sidestep_left': ('c69634c9-Long_Left_Side_Step.fbx', 'one long combat side-step, left'),
    'sidestep_right': ('f13a67dc-Medium_Right_Side_Step.fbx', 'one combat side-step, right'),
}


def main(folder):
    out = os.path.join(HERE, 'samples')
    os.makedirs(out, exist_ok=True)
    files = os.listdir(folder)
    for clip, (ending, about) in MANIFEST.items():
        match = [f for f in files if f.endswith(ending)]
        if not match:
            print(f'{clip}: no file ending {ending}')
            continue
        sk = mr.Skeleton(os.path.join(folder, match[0]))
        n = max(2, int(round(sk.length * RATE)))
        frames = []
        for i in range(n + 1):
            w = sk.pose(min(sk.length, i / RATE))
            frames.append([round(float(c), 4) for b in BONES for c in w[b][1]])
        with open(os.path.join(out, clip + '.json'), 'w') as f:
            json.dump(dict(clip=clip, about=about, source=match[0], rate=RATE, length=round(sk.length, 4), bones=BONES, frames=frames), f, separators=(',', ':'))
        print(f'{clip}: {len(frames)} frames from {match[0]}')


if __name__ == '__main__':
    main(sys.argv[1])
