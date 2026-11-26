# Credits for outside material

Material in this repository that is someone else's work, and the terms it is used under.

## The 100STYLE dataset (walking footage)

`Assets/_Battler/Resources/Locomotion/WalkFootage.json` is baked from **The 100STYLE Dataset - Ian Mason**
(Ian Mason, Sebastian Starke, Taku Komura: "Real-Time Style Modelling of Human Locomotion via Feature-Wise
Transformations and Local Motion Phases", 2022), published under **Creative Commons Attribution 4.0**
(CC BY 4.0, https://creativecommons.org/licenses/by/4.0/). Source: https://www.ianxmason.com/100style/ and
Zenodo record 8127870.

Used: the styles Neutral (all its takes) and StartStop (forward, backward, sideways, transitions, standing).
Changed: read from BVH at 60 frames a second and written at 30; rebuilt on this game's own body measures (ankle
height, foot, hips, shoulders, bone lengths); the slow circling of the room taken out of the walks' and runs' root
tracks; cut to the frames the dataset's `Frame_Cuts.csv` names. The baker is `Tools/Mocap/Locomotion/bvh_bake.py`.

A game that ships with this footage must carry this credit where its credits are shown.
