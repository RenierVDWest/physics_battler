using System.Collections.Generic;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// A movement style as a pack (the plan "Fight Rhythm from Recordings", step 1): a named set of recordings, each
    /// with a job in the walk. The walk itself (planted feet, real steps, stride fitting, stand and step) is the same
    /// for every style; what a style changes is which recordings it steps with. Give the walk another pack and the
    /// fighter moves another way.
    ///
    /// <para>The jobs: standing; walking forward and back; a step to each side; running forward and back; turning on
    /// the spot to each side and about. A job with no clip named takes the clip its prefix has for it
    /// (<see cref="UsualNames"/>: "sns_" + "walk_back"); a job the pack has nothing for is filled from the relaxed walk,
    /// as it always was.</para>
    /// </summary>
    public sealed class MoveStyle
    {
        /// <summary>What the style is called (the panels show it).</summary>
        public string Name;

        /// <summary>The clip library's prefix for this pack's recordings ("sns_": the sword and shield's guard).</summary>
        public string Prefix;

        /// <summary>The jobs, in the order of <see cref="UsualNames"/>.</summary>
        public enum Job
        {
            Stand,
            WalkForward,
            WalkBack,
            StepLeft,
            StepRight,
            RunForward,
            RunBack,
            TurnLeft,
            TurnRight,
            TurnAbout,
        }

        /// <summary>What each job's clip is usually called, after the prefix. (Standing: the steadier take first.)</summary>
        public static readonly string[][] UsualNames =
        {
            new[] { "idle_b", "idle" }, new[] { "walk" }, new[] { "walk_back" }, new[] { "step_left" }, new[] { "step_right" },
            new[] { "run" }, new[] { "run_back" }, new[] { "turn_left" }, new[] { "turn_right" }, new[] { "turn180" },
        };

        /// <summary>A job's clip by its full name in the library, where it is not the prefix's usual one.</summary>
        public readonly Dictionary<Job, string> Named = new Dictionary<Job, string>();

        /// <summary>The clip a job is done with (null: the pack has none, and the relaxed walk's is used).</summary>
        public LocoClip Clip(Job job)
        {
            if (Named.TryGetValue(job, out string name))
            {
                return LocoLibrary.Get(name);
            }

            foreach (string usual in UsualNames[(int)job])
            {
                var clip = LocoLibrary.Get(Prefix + usual);
                if (clip != null)
                {
                    return clip;
                }
            }

            return null;
        }

        /// <summary>The pack as the walk takes it (null when it has nothing to stand or walk with).</summary>
        public Locomotor.ClipSet Clips()
        {
            var set = new Locomotor.ClipSet
            {
                Name = Prefix,
                Idle = Clip(Job.Stand),
                WalkF = Clip(Job.WalkForward),
                WalkB = Clip(Job.WalkBack),
                StepL = Clip(Job.StepLeft),
                StepR = Clip(Job.StepRight),
                RunF = Clip(Job.RunForward),
                RunB = Clip(Job.RunBack),
                TurnL = Clip(Job.TurnLeft),
                TurnR = Clip(Job.TurnRight),
                Turn180 = Clip(Job.TurnAbout),
            };
            return set.Idle != null && set.WalkF != null ? set : null;
        }

        /// <summary>The standard style: the sword and shield pack's guard, as the fight has moved until now.</summary>
        public static readonly MoveStyle Standard = new MoveStyle { Name = "Standard", Prefix = "sns_" };

        private static readonly Dictionary<string, MoveStyle> byPrefix = new Dictionary<string, MoveStyle> { { "sns_", Standard } };

        /// <summary>The style a guard's clips make (by their prefix: "sns_" is <see cref="Standard"/>; "lg_", the Large's two-handed guard).</summary>
        public static MoveStyle Of(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                return null;
            }

            if (!byPrefix.TryGetValue(prefix, out var style))
            {
                byPrefix[prefix] = style = new MoveStyle { Name = prefix.TrimEnd('_'), Prefix = prefix };
            }

            return style;
        }
    }
}
