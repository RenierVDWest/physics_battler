using System.Collections.Generic;

namespace PhysicsBattler.Audio
{
    /// <summary>The ids of the game's sounds, so code can't misspell them.</summary>
    public static class SfxIds
    {
        // Feet and bodies on the ground.
        public const string StepWalk = "step_walk";
        public const string StepRun = "step_run";
        public const string Land = "land";
        public const string BodyFall = "body_fall";
        public const string BodyThud = "body_thud";

        // Swings.
        public const string SwingBlade = "swing_blade";
        public const string SwingClub = "swing_club";
        public const string SwingHeavy = "swing_heavy";

        // A weapon meeting a body.
        public const string HitBladeFlesh = "hit_blade_flesh";
        public const string HitClubFlesh = "hit_club_flesh";
        public const string HitShowstopper = "hit_showstopper";
        public const string Sever = "sever";
        public const string BodyImpact = "body_impact";

        // A weapon meeting a weapon or a shield.
        public const string ParryClang = "parry_clang";
        public const string BladeGlance = "blade_glance";
        public const string BladeShield = "blade_shield";
        public const string ClubShield = "club_shield";
        public const string ClubParry = "club_parry";
        public const string ShieldShield = "shield_shield";
        public const string BladeTap = "blade_tap";
        public const string GuardBreak = "guard_break";

        // Things dropped.
        public const string WeaponDrop = "weapon_drop";
        public const string ShieldDrop = "shield_drop";

        /// <summary>Sounds that were in the list once and have been replaced: taken out of DA_Sfx, their files deleted.</summary>
        public static readonly string[] Retired = { };

        // (Hexamus's two ambience slots: here the open field, and no second.)
        public const string AmbienceArena = "ambience_field";
        public const string AmbienceDeep = "ambience_none";
    }

    /// <summary>
    /// The sound list as first set up: a prompt, length and number of takes for every sound where two things meet in
    /// the fight (feet and bodies on the ground, swings, blades and clubs on bodies, on blades and on shields, things
    /// dropped), and the field's ambience. About 65 seconds of sound in all, around 2,600 ElevenLabs credits.
    /// </summary>
    public static class SfxDefaults
    {
        // Said at the end of most prompts, so the set sounds like one game.
        private const string Dry = " Game sound effect, close and dry, no music, no voices.";

        /// <summary>The group a sound belongs to by default (for a sound list made before groups).</summary>
        public static SfxGroup GroupFor(string id)
        {
            foreach (var entry in All())
            {
                if (entry.Id == id)
                {
                    return entry.Group;
                }
            }

            return SfxGroup.None;
        }

        public static IEnumerable<SfxEntry> All()
        {
            // Feet.
            yield return E(SfxGroup.Steps, SfxIds.StepWalk, "A single soft footstep of a leather boot on packed earth, a light gritty scuff." + Dry, 0.5f, 5, 0.45f, 0.08f, 0.5f);
            yield return E(SfxGroup.Steps, SfxIds.StepRun, "A single quick running footstep of a leather boot striking packed earth, a firm crisp thud." + Dry, 0.5f, 4, 0.55f, 0.1f, 0.5f);
            yield return E(SfxGroup.Steps, SfxIds.Land, "A warrior landing from a leap onto packed earth with both feet: a heavy solid thud, a puff of dirt, armour leather creaking." + Dry, 0.6f, 3, 0.8f);

            // Bodies on the ground.
            yield return E(SfxGroup.Bodies, SfxIds.BodyFall, "A man in leather armour collapsing onto packed earth: a heavy body thud, the breath knocked out, leather and buckles rattling." + Dry, 0.9f, 3, 0.9f);
            yield return E(SfxGroup.Bodies, SfxIds.BodyThud, "An arm or a leg knocking against the ground: a short dull soft thump on earth." + Dry, 0.5f, 3, 0.5f, 0.1f);
            yield return E(SfxGroup.Bodies, SfxIds.BodyImpact, "Two men crashing into each other in a fight: a heavy dull body slam, leather armour and a grunt of air." + Dry, 0.6f, 2, 0.8f);

            // Swings.
            yield return E(SfxGroup.Combat, SfxIds.SwingBlade, "A fast longsword swing cutting through the air: a sharp clean swish." + Dry, 0.5f, 4, 0.55f, 0.08f);
            yield return E(SfxGroup.Combat, SfxIds.SwingClub, "A heavy wooden club swung hard: a deep slow whoosh." + Dry, 0.6f, 3, 0.6f, 0.08f);
            yield return E(SfxGroup.Combat, SfxIds.SwingHeavy, "A huge two-handed overhead sword blow tearing through the air: a powerful deep whoosh building to a roar." + Dry, 0.8f, 2, 0.75f);

            // A weapon meeting a body.
            yield return E(SfxGroup.Combat, SfxIds.HitBladeFlesh, "A sword slicing into a body through leather: a wet meaty slash, punchy and short." + Dry, 0.6f, 4, 0.9f);
            yield return E(SfxGroup.Combat, SfxIds.HitClubFlesh, "A heavy wooden club smashing into a body: a deep crunching thump with a crack." + Dry, 0.6f, 3, 0.95f);
            yield return E(SfxGroup.Combat, SfxIds.HitShowstopper, "The final crushing blow of a combo: a huge heavy impact boom with a deep sub-bass punch and a short crunch, cinematic and satisfying." + Dry, 1f, 3, 0.9f, 0.04f);
            yield return E(SfxGroup.Combat, SfxIds.Sever, "A sword chopping clean through a limb: a sharp wet chop and a crunch of bone." + Dry, 0.8f, 2, 1f);

            // A weapon meeting a weapon or a shield.
            yield return E(SfxGroup.Weapons, SfxIds.ParryClang, "Two steel longswords clashing hard in a parry: a bright ringing metallic clang with a short shimmer." + Dry, 0.8f, 4, 0.85f, 0.07f);
            yield return E(SfxGroup.Weapons, SfxIds.BladeGlance, "A steel sword blade glancing and scraping along another blade: a quick metallic screech and slide." + Dry, 0.7f, 3, 0.75f);
            yield return E(SfxGroup.Weapons, SfxIds.BladeShield, "A steel sword striking a round wooden shield: a hard hollow wooden thunk with a metallic bite." + Dry, 0.6f, 4, 0.85f);
            yield return E(SfxGroup.Weapons, SfxIds.ClubShield, "A heavy wooden club slamming into a round wooden shield: a deep booming wooden knock." + Dry, 0.6f, 3, 0.9f);
            yield return E(SfxGroup.Weapons, SfxIds.ClubParry, "A heavy wooden club meeting a steel sword: a dull wooden crack with a short metal ring." + Dry, 0.6f, 3, 0.85f);
            yield return E(SfxGroup.Weapons, SfxIds.ShieldShield, "Two round wooden shields bashing together: a heavy hollow wooden clack with the rattle of their iron rims." + Dry, 0.6f, 3, 0.85f);
            yield return E(SfxGroup.Weapons, SfxIds.BladeTap, "A sword blade lightly touching a shield rim or another blade: a small metallic tick." + Dry, 0.5f, 3, 0.35f, 0.1f);
            yield return E(SfxGroup.Weapons, SfxIds.GuardBreak, "A shield guard smashed aside by a heavy blow: a loud wooden crack and the clatter of the shield knocked away." + Dry, 0.8f, 2, 0.9f);

            // Things dropped.
            yield return E(SfxGroup.Weapons, SfxIds.WeaponDrop, "A steel sword dropped onto packed earth: a metallic clatter and a short scrape." + Dry, 0.8f, 2, 0.75f);
            yield return E(SfxGroup.Weapons, SfxIds.ShieldDrop, "A round wooden shield falling flat onto packed earth: a hollow wooden thud and a wobble of its rim." + Dry, 0.8f, 2, 0.75f);

            // The ambience.
            yield return Loop(SfxGroup.Ambience, SfxIds.AmbienceArena, "An open grassy battlefield on a calm day: a soft steady wind, distant birds, grass rustling. Seamless loop, no music, no voices.");
        }

        private static SfxEntry E(SfxGroup group, string id, string prompt, float duration, int takes, float volume, float pitch = 0.06f, float influence = 0.45f)
        {
            return new SfxEntry
            {
                Id = id, Group = group, Prompt = prompt, Duration = duration, Takes = takes, Volume = volume, PitchVariation = pitch, PromptInfluence = influence,
            };
        }

        private static SfxEntry Loop(SfxGroup group, string id, string prompt)
        {
            return new SfxEntry
            {
                Id = id, Group = group, Prompt = prompt, Duration = 20f, Takes = 1, Loop = true, Volume = 0.6f, PitchVariation = 0f, PromptInfluence = 0.4f,
            };
        }
    }
}
