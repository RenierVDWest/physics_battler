# Sound

The sound tool is Hexamus's, brought over as it is. Every sound in the game comes from one list, **DA_Sfx**
(`Assets/_Battler/Audio/Resources`). For each sound it holds the prompt it's made from, its length, how many
takes to make, how loud it plays and how much its pitch varies. The clips are made from those prompts with
ElevenLabs, inside Unity.

## Making the sounds

1. Open **Physics Battler > Audio > Sound Effects**. The first time, it makes DA_Sfx with the list below.
2. **API key**: already filled in if you entered it in Hexamus on this computer. The window keeps it in the same
   Unity preference (Unity keeps these per computer, not per project), never in the project or on GitHub. If it's
   empty, paste your ElevenLabs key; it needs the **Sound Effects** permission.
3. Press **Generate missing**. Every take not yet on disk is made, two at a time, and saved as
   `Audio/Sfx/{id}_01.mp3`, `_02`... The full list is 23 sounds and 66 takes, about 2,600 credits (40 a second).
4. Listen right in the window: under each sound, **▶ 1**, **▶ 2**... play its takes (■ stops). Don't like one?
   **⟳** next to it makes just that take again (a re-roll). Each sound's **volume** slider is there too.
5. Change what a sound is made from: click its name. Its **prompt**, **Follow prompt** (lower, around 0.2, gives
   takes that differ more), **Seconds** and **Takes** are there; then ⟳ a take. **Back to the original prompt**
   undoes your wording.

Any clip named `{id}_NN` (mp3, wav or ogg) in `Audio/Sfx` is picked up, so you can drop in sounds of your own. The
clips are kept in Git directly; commit them.

## The mix

**Physics Battler > Audio > Add the Sound object to this scene** puts a **Sound** object (Sound Settings) in the
open scene: everything on or off, the effects and ambience volumes, and a switch and volume for each group
(Steps, Combat, Weapons, Bodies, Ambience), each sound's own switch and volume under its group. Without it the
sounds play at the defaults. A random take each time, never the same twice running, at a slightly different
pitch, panned toward where it happens on screen; the silence at a take's start is skipped so a hit lands on its
frame; the same sound at most three times at once.

## Where they play (`Scripts/Runtime/Audio/SoundHooks.cs`)

| Where two things meet | Sound |
|---|---|
| A foot planted (the lab, the new walk), a running one; a leap landing | step_walk, step_run, land |
| The fight's own gait: a physical foot coming down | step_walk, step_run |
| A swing, just before its blow lands; a combo's showstopper | swing_blade, swing_club, swing_heavy |
| A sword or a club on a body | hit_blade_flesh, hit_club_flesh |
| The showstopper landing (with its hit) | hit_showstopper |
| Sword on sword (a parry), a club on a sword | parry_clang, club_parry |
| A blade glancing off a blade | blade_glance |
| A sword or a club on a wooden shield | blade_shield, club_shield |
| A guard broken | guard_break |
| A blade lightly touching a shield or a blade | blade_tap |
| Two shields meeting | shield_shield |
| Two bodies colliding | body_impact |
| A limb cut off | sever |
| A body falling on the ground; a limb knocking it | body_fall, body_thud |
| A dropped sword or shield hitting the ground | weapon_drop, shield_drop |
| The field (a loop) | ambience_field |

Headless, `SOUND_LOG=1` with any runner counts the sounds it asked for.
