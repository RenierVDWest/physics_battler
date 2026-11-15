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

            if (StandSteps)
            {
                set.IdleSteps = set.Idle.Source != null;
                set.Idle = Sized(set.Idle, StandSize, StandArms);
                set.Name = Name;
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

            if (GuardWalkRebuilt && this == Standard)
            {
                Rebuild(set);
            }

            return set;
        }

        /// <summary>
        /// The round trip (the plan "Movement Styles as Offshoots of the Standard Set", step S1, its test T0; the
        /// scene's Combat Feel, "Guard Walk Is Rebuilt From The Plain Walk"): the standard guard walks forward and
        /// back on the relaxed walk's own recordings, empty-handed and upright, with the guard's manner laid on them
        /// (<see cref="StyleManner"/>): its lean, its chest's turn, its arms held up and still, as its profile has
        /// them in numbers. Nothing of the guard's own walk is played. Off (as it is): the guard's recordings. The
        /// rest of the guard (its stand, its steps to the sides, its turns, its jog) is its own either way.
        /// </summary>
        public static bool GuardWalkRebuilt = false;

        /// <summary>How much of the guard's carriage and of its motion the rebuilt walk is given (1: all of each, as measured).</summary>
        public static float RebuiltCarriage = 1f, RebuiltMotion = 1f;

        private static void Rebuild(Locomotor.ClipSet set)
        {
            StyleProfile plain = StyleProfile.Get("plain"), guard = StyleProfile.Get("guard");
            if (plain == null || guard == null)
            {
                return;
            }

            foreach (var (job, clip) in new[] { (Job.WalkForward, "walk_fwd"), (Job.WalkBack, "walk_back") })
            {
                if (plain.Has(job) && guard.Has(job) && plain.For(job, out var from) && guard.For(job, out var to))
                {
                    var rebuilt = StyleManner.Over(LocoLibrary.Get(clip), from, to, "rebuilt_" + clip, RebuiltCarriage, RebuiltMotion);
                    if (rebuilt != null && job == Job.WalkForward) set.WalkF = rebuilt;
                    if (rebuilt != null && job == Job.WalkBack) set.WalkB = rebuilt;
                }
            }
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

        /// <summary>
        /// The style's stand steps (the plan's step 4: a base that is never still, capoeira's ginga): its standing clip
        /// is a loop in which the feet step on the spot, and the walk stands in it with the feet going as the
        /// recording has them, where it holds a guard's feet where they stopped.
        /// </summary>
        public bool StandSteps;

        /// <summary>
        /// How big a stepping stand is made against its recording (1: as recorded; the scene's Combat Feel, "Ginga
        /// Size"): its steps, its hips' sway and sinking, its body's turns and its arms' swings, each counted from
        /// the middle of the loop and multiplied (<see cref="Sized"/>).
        /// </summary>
        public static float StandSize = 0.67f;

        /// <summary>How much of a stepping stand's arm swings is kept on top of <see cref="StandSize"/> (1: all; the scene's Combat Feel, "Ginga Arms").</summary>
        public static float StandArms = 1f;

        /// <summary>The takes of the ginga the capoeira style can stand in (the clip library's names), and what each is.</summary>
        public static readonly (string clip, string about)[] GingaTakes =
        {
            ("cap_ginga", "the plain ginga: a step back to each side, 3.4 s"),
            ("cap_ginga_b", "a quieter take, 2.9 s"),
            ("cap_ginga_c", "a lower take, the body turning, 3.5 s"),
            ("cap_ginga_d", "a lower take, 3.8 s"),
        };

        /// <summary>
        /// A stepping stand made bigger or smaller than its recording, as a clip the walk stands in: each part's track
        /// (<see cref="BodyTracks"/>) is taken, what it does is counted from the middle of the loop (the hips' height
        /// from where they are highest; a foot's from the ground) and multiplied, and the body put together again. A
        /// foot that stands still in the recording stands still in it. (Made once for each size.)
        /// </summary>
        public static LocoClip Sized(LocoClip clip, float size, float arms)
        {
            if (clip == null || clip.Source == null || (Mathf.Abs(size - 1f) < 0.005f && Mathf.Abs(arms - 1f) < 0.005f))
            {
                return clip;
            }

            string name = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}_x{1:0.00}_{2:0.00}", clip.Name, size, arms);
            if (made.TryGetValue(name, out var sized))
            {
                return sized;
            }

            var m = BodyMeasures.OfClips;
            var d = clip.Source;
            var p = new Vector3[LocoClip.BoneCount];
            var frames = new BodyTracks[d.frames];
            for (int f = 0; f < d.frames; f++)
            {
                clip.Pose(f / d.rate, p, 1f);
                frames[f] = m.Decompose(p, f > 0 ? frames[f - 1] : (BodyTracks?)null);
            }

            // The middle of the loop: each track's mean (a way's: the mean way; a hand's turn: the mean turn).
            var mid = new BodyTracks();
            var handL = new float[4];
            var handR = new float[4];
            float top = float.MinValue, groundL = float.MaxValue, groundR = float.MaxValue;
            foreach (var t in frames)
            {
                mid.Pelvis += t.Pelvis; mid.PelvisTurn += t.PelvisTurn; mid.ChestTurn += t.ChestTurn; mid.HeadTurn += t.HeadTurn;
                mid.ClavL += t.ClavL; mid.ClavR += t.ClavR; mid.UpperL += t.UpperL; mid.UpperR += t.UpperR; mid.ForeL += t.ForeL; mid.ForeR += t.ForeR;
                mid.AnkleL += t.AnkleL; mid.AnkleR += t.AnkleR; mid.KneeL += t.KneeL; mid.KneeR += t.KneeR;
                mid.FootL += t.FootL; mid.FootR += t.FootR; mid.ToeL += t.ToeL; mid.ToeR += t.ToeR;
                handL[0] += t.HandL.x; handL[1] += t.HandL.y; handL[2] += t.HandL.z; handL[3] += t.HandL.w;
                handR[0] += t.HandR.x; handR[1] += t.HandR.y; handR[2] += t.HandR.z; handR[3] += t.HandR.w;
                top = Mathf.Max(top, t.Pelvis.y);
                groundL = Mathf.Min(groundL, t.AnkleL.y);
                groundR = Mathf.Min(groundR, t.AnkleR.y);
            }

            float k = 1f / Mathf.Max(1, frames.Length);
            mid.Pelvis *= k; mid.PelvisTurn *= k; mid.ChestTurn *= k; mid.HeadTurn *= k; mid.AnkleL *= k; mid.AnkleR *= k;
            mid.ClavL.Normalize(); mid.ClavR.Normalize(); mid.UpperL.Normalize(); mid.UpperR.Normalize(); mid.ForeL.Normalize(); mid.ForeR.Normalize();
            mid.KneeL.Normalize(); mid.KneeR.Normalize(); mid.FootL.Normalize(); mid.FootR.Normalize(); mid.ToeL.Normalize(); mid.ToeR.Normalize();
            mid.HandL = MeanTurn(handL);
            mid.HandR = MeanTurn(handR);
            float swing = size * arms;

            var data = new LocoClipData
            {
                name = name, role = d.role, loop = d.loop, rate = d.rate, frames = d.frames, legLength = d.legLength, duration = d.duration,
                speed = d.speed, direction = d.direction, turn = d.turn, stride = d.stride, markers = d.markers, root = d.root,
                contacts = d.contacts, phase = d.phase, kind = d.kind, set = d.set,
                about = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} at {1:0.00} of its size, its arms' swings at {2:0.00}", clip.Name, size, swing),
                points = new float[d.frames * LocoClip.BoneCount * 3],
            };
            for (int f = 0; f < d.frames; f++)
            {
                var t = frames[f];
                var r = t;
                r.Pelvis = new Vector3(mid.Pelvis.x + size * (t.Pelvis.x - mid.Pelvis.x), top - size * (top - t.Pelvis.y), mid.Pelvis.z + size * (t.Pelvis.z - mid.Pelvis.z));
                r.PelvisTurn = mid.PelvisTurn + size * (t.PelvisTurn - mid.PelvisTurn);
                r.ChestTurn = mid.ChestTurn + size * (t.ChestTurn - mid.ChestTurn);
                r.HeadTurn = mid.HeadTurn + size * (t.HeadTurn - mid.HeadTurn);
                r.AnkleL = new Vector3(mid.AnkleL.x + size * (t.AnkleL.x - mid.AnkleL.x), groundL + size * (t.AnkleL.y - groundL), mid.AnkleL.z + size * (t.AnkleL.z - mid.AnkleL.z));
                r.AnkleR = new Vector3(mid.AnkleR.x + size * (t.AnkleR.x - mid.AnkleR.x), groundR + size * (t.AnkleR.y - groundR), mid.AnkleR.z + size * (t.AnkleR.z - mid.AnkleR.z));
                r.KneeL = BodyTracks.Way(mid.KneeL, t.KneeL, size); r.KneeR = BodyTracks.Way(mid.KneeR, t.KneeR, size);
                r.FootL = BodyTracks.Way(mid.FootL, t.FootL, size); r.FootR = BodyTracks.Way(mid.FootR, t.FootR, size);
                r.ToeL = BodyTracks.Way(mid.ToeL, t.ToeL, size); r.ToeR = BodyTracks.Way(mid.ToeR, t.ToeR, size);
                r.ClavL = BodyTracks.Way(mid.ClavL, t.ClavL, swing); r.ClavR = BodyTracks.Way(mid.ClavR, t.ClavR, swing);
                r.UpperL = BodyTracks.Way(mid.UpperL, t.UpperL, swing); r.UpperR = BodyTracks.Way(mid.UpperR, t.UpperR, swing);
                r.ForeL = BodyTracks.Way(mid.ForeL, t.ForeL, swing); r.ForeR = BodyTracks.Way(mid.ForeR, t.ForeR, swing);
                r.HandL = Quaternion.SlerpUnclamped(mid.HandL, Quaternion.Dot(mid.HandL, t.HandL) < 0f ? new Quaternion(-t.HandL.x, -t.HandL.y, -t.HandL.z, -t.HandL.w) : t.HandL, swing);
                r.HandR = Quaternion.SlerpUnclamped(mid.HandR, Quaternion.Dot(mid.HandR, t.HandR) < 0f ? new Quaternion(-t.HandR.x, -t.HandR.y, -t.HandR.z, -t.HandR.w) : t.HandR, swing);
                m.Compose(r, p);
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

        private static Quaternion MeanTurn(float[] sum)
        {
            float length = Mathf.Sqrt(sum[0] * sum[0] + sum[1] * sum[1] + sum[2] * sum[2] + sum[3] * sum[3]);
            return length > 1e-6f ? new Quaternion(sum[0] / length, sum[1] / length, sum[2] / length, sum[3] / length) : Quaternion.identity;
        }

        /// <summary>The standard style: the sword and shield pack's guard, as the fight has moved until now.</summary>
        public static readonly MoveStyle Standard = new MoveStyle { Name = "Standard", Prefix = "sns_", JogPrefix = "jog_" };

        /// <summary>
        /// The capoeira style (the plan's step 4): it stands in the ginga, never still, one foot stepping back and then
        /// the other, the hips swaying under it; to go anywhere it walks and jogs as the standard style does.
        /// </summary>
        public static readonly MoveStyle Capoeira = MakeCapoeira();

        private static MoveStyle MakeCapoeira()
        {
            var style = new MoveStyle { Name = "Capoeira", Prefix = "sns_", JogPrefix = "jog_", StandSteps = true };
            style.Named[Job.Stand] = GingaTakes[0].clip;
            return style;
        }

        /// <summary>The take of the ginga the capoeira style stands in (<see cref="GingaTakes"/>).</summary>
        public static int GingaTake
        {
            get
            {
                for (int i = 0; i < GingaTakes.Length; i++)
                {
                    if (Capoeira.Named[Job.Stand] == GingaTakes[i].clip)
                    {
                        return i;
                    }
                }

                return 0;
            }
            set => Capoeira.Named[Job.Stand] = GingaTakes[Mathf.Clamp(value, 0, GingaTakes.Length - 1)].clip;
        }

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
