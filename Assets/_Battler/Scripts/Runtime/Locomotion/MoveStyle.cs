using System.Collections.Generic;
using UnityEngine;

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

        /// <summary>
        /// The style's brisk pace (the plan's step 2; the scene's Combat Feel, "Guard Jogs"): above a walk the guard
        /// jogs, forward, back and to each side, where it had only its run forward and back (4 m/s, its stride
        /// shortened to the pace) and nothing of its own to the sides. The jogs were recorded with empty hands
        /// (2.4 to 3.0 m/s): their hips and legs are put under the guard's own chest, head and arms
        /// (<see cref="LegsUnder"/>), so the shield and the sword are carried as the guard carries them.
        /// </summary>
        public static bool Jogs = true;

        /// <summary>The library's prefix for the legs a style jogs on ("jog_"), or null: it has no jog.</summary>
        public string JogPrefix;

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
            if (set.Idle == null || set.WalkF == null)
            {
                return null;
            }

            if (Jogs && !string.IsNullOrEmpty(JogPrefix))
            {
                // (Each jog's legs under the guard's chest and arms as they are in the nearest thing it has: its run
                // forward and back, its walk for the sides.)
                set.RunF = LegsUnder(LocoLibrary.Get(JogPrefix + "fwd"), set.RunF ?? set.WalkF, Prefix + "jog") ?? set.RunF;
                set.RunB = LegsUnder(LocoLibrary.Get(JogPrefix + "back"), set.RunB ?? set.WalkB ?? set.WalkF, Prefix + "jog_back") ?? set.RunB;
                set.RunL = LegsUnder(LocoLibrary.Get(JogPrefix + "left"), set.StepL ?? set.WalkF, Prefix + "jog_left");
                set.RunR = LegsUnder(LocoLibrary.Get(JogPrefix + "right"), set.StepR ?? set.WalkF, Prefix + "jog_right");
            }

            return set;
        }

        private static readonly Dictionary<string, LocoClip> made = new Dictionary<string, LocoClip>();

        /// <summary>
        /// One step cycle's hips and legs under another's chest, head and arms, frame by frame at the same moment of
        /// the stride (the left foot lands at the same time in both): a clip the walk plays like any cycle. Its travel,
        /// its feet's contacts and its stride are the legs'. Null when either is missing or is not a step cycle. (Made
        /// once for each name.)
        /// </summary>
        public static LocoClip LegsUnder(LocoClip legs, LocoClip upper, string name)
        {
            if (legs == null || upper == null || legs.Source == null || !legs.HasPhase || !upper.HasPhase)
            {
                return null;
            }

            if (made.TryGetValue(name, out var clip))
            {
                return clip;
            }

            var m = BodyMeasures.OfClips;
            var d = legs.Source;
            var data = new LocoClipData
            {
                name = name, role = d.role, loop = d.loop, rate = d.rate, frames = d.frames, legLength = d.legLength, duration = d.duration,
                speed = d.speed, direction = d.direction, turn = d.turn, stride = d.stride, markers = d.markers, root = d.root,
                contacts = d.contacts, phase = d.phase, kind = d.kind, set = "guard",
                about = legs.Name + "'s hips and legs under " + upper.Name + "'s chest, head and arms",
                points = new float[d.frames * LocoClip.BoneCount * 3],
            };
            Vector3[] under = new Vector3[LocoClip.BoneCount], over = new Vector3[LocoClip.BoneCount], p = new Vector3[LocoClip.BoneCount];
            for (int f = 0; f < d.frames; f++)
            {
                float time = f / d.rate;
                legs.Pose(time, under, 1f);
                upper.Pose(upper.TimeAtPhase(legs.PhaseAt(time)), over, 1f);
                var lower = m.Decompose(under);
                var t = m.Decompose(over);
                // (The chest's twist on the hips, the head and the arms are the upper clip's; where these hips turn and
                // lean, the chest goes with them.)
                t.Pelvis = lower.Pelvis; t.PelvisTurn = lower.PelvisTurn;
                t.AnkleL = lower.AnkleL; t.AnkleR = lower.AnkleR; t.KneeL = lower.KneeL; t.KneeR = lower.KneeR;
                t.FootL = lower.FootL; t.FootR = lower.FootR; t.ToeL = lower.ToeL; t.ToeR = lower.ToeR;
                m.Compose(t, p);
                for (int i = 0; i < LocoClip.BoneCount; i++)
                {
                    int at = (f * LocoClip.BoneCount + i) * 3;
                    data.points[at] = p[i].x;
                    data.points[at + 1] = p[i].y;
                    data.points[at + 2] = p[i].z;
                }
            }

            return made[name] = new LocoClip(data);
        }

        /// <summary>The standard style: the sword and shield pack's guard, as the fight has moved until now.</summary>
        public static readonly MoveStyle Standard = new MoveStyle { Name = "Standard", Prefix = "sns_", JogPrefix = "jog_" };

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
