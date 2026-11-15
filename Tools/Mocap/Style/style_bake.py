#!/usr/bin/env python3
"""Ruler clips: recordings a movement style is measured on and that the game never plays (the plan "Movement Styles
as Offshoots of the Standard Set": "the recordings are the ruler"). Baked as the combat clips are (points in leg
lengths against the root) but written beside this tool, outside the game's Resources:

    python3 Tools/Mocap/Style/style_bake.py        ->  Tools/Mocap/Style/RulerClips.json

The headless measuring tool ("style write") reads them with the game's own clips. To measure a new pack: sample
its clips (Tools/Mocap/Combat/combat_sample.py), name them in RULER below, bake, and name them in the tool's
sources (Tools/CompileCheck/Runner/StyleReport.cs).
"""
import json, os, sys

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(os.path.dirname(HERE), 'Combat'))
import combat_bake as cb

OUT = os.path.join(HERE, 'RulerClips.json')

# name: (sample, about)
RULER = {
    'cap_ginga_fwd': ('ginga_forward', 'Capoeira: the ginga travelling forward'),
    'cap_ginga_back': ('ginga_backward', 'Capoeira: the ginga travelling back'),
}


def main():
    clips, bones = [], None
    for name, (sample, about) in RULER.items():
        d, B, F = cb.load(sample)
        bones = B
        clips.append(cb.bake_one(name, d, B, F, False, 'ruler', about))
    with open(OUT, 'w') as f:
        json.dump(dict(about='Ruler clips baked by Tools/Mocap/Style/style_bake.py: measured, never played.', bones=bones, clips=clips), f, separators=(',', ':'))
    for c in clips:
        print(f"{c['name']:18s} {c['duration']:5.2f}s speed {c['speed'] * c['legLength']:4.2f} m/s dir {c['direction']:+6.1f} turn {c['turn']:+6.1f}")
    print(f'{len(clips)} clips -> {OUT} ({os.path.getsize(OUT) / 1e6:.2f} MB)')


if __name__ == '__main__':
    main()
