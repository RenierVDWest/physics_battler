using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// How a long step is taken (the plan "Movement Styles as Offshoots of the Standard Set", step S3): a foot swept
    /// a long way over the ground while the other stays planted. Read off the capoeira pack's gingas (fifteen steps
    /// of 1.4 to 2.2 m): a step takes 0.38 s for each metre it goes; the ankle rises 6 to 7 cm for each metre, and is
    /// highest near the end of the step that is behind him (late in a step back, early in a step forward); its path
    /// bows a little off the straight line, away from the foot that stands.
    /// </summary>
    public struct LongStepStyle
    {
        /// <summary>How high the ankle rises, as a share of how far the step goes; and the least it rises (leg lengths).</summary>
        public float Lift, LeastLift;

        /// <summary>Where along a step back the foot is highest (0 to 1 of the swing); a step forward is highest as far from its end.</summary>
        public float HighAt;

        /// <summary>How far the path bows off the straight line, away from the standing foot, as a share of how far the step goes.</summary>
        public float Bow;

        /// <summary>How long a step takes for each leg length it goes, and the least it takes (seconds).</summary>
        public float TimePerLength, LeastTime;

        public static LongStepStyle Plain => new LongStepStyle { Lift = 0.065f, LeastLift = 0.04f, HighAt = 0.7f, Bow = 0.08f, TimePerLength = 0.34f, LeastTime = 0.24f };
    }

    /// <summary>
    /// The long step (the plan's step S3), a part of the standard set: where a swinging foot's ankle is at each
    /// moment of a step from one place to another, and how long the step takes. The foot that stands is not moved.
    /// </summary>
    public static class LongStep
    {
        public static LongStepStyle Style = LongStepStyle.Plain;

        /// <summary>How long a step from one place to another takes (seconds; the places in leg lengths).</summary>
        public static float Time(Vector3 from, Vector3 to, in LongStepStyle style) =>
            Mathf.Max(style.LeastTime, style.TimePerLength * new Vector2(to.x - from.x, to.z - from.z).magnitude);

        /// <summary>Where the swinging foot's ankle is, a share <paramref name="u"/> of the way through its step (the places' own frame and units).</summary>
        /// <param name="planted">Where the other foot stands: the path bows away from it.</param>
        public static Vector3 Swing(Vector3 from, Vector3 to, float u, Vector3 planted, in LongStepStyle style)
        {
            u = Mathf.Clamp01(u);
            Vector3 line = new Vector3(to.x - from.x, 0f, to.z - from.z);
            float far = line.magnitude;
            if (far < 1e-4f)
            {
                return Vector3.Lerp(from, to, u);
            }

            Vector3 along = line / far, across = new Vector3(along.z, 0f, -along.x);
            float side = Vector3.Dot(0.5f * (from + to) - planted, across) >= 0f ? 1f : -1f;
            float highAt = Mathf.Clamp(to.z < from.z ? style.HighAt : 1f - style.HighAt, 0.15f, 0.85f);
            // (The swing's own share bent so its middle falls where the foot is highest.)
            float bent = Mathf.Pow(u, Mathf.Log(0.5f) / Mathf.Log(highAt));
            float lift = Mathf.Max(style.LeastLift, style.Lift * far) * Mathf.Sin(Mathf.PI * bent);
            float gone = u * u * (3f - 2f * u);
            return Vector3.Lerp(from, to, gone) + across * (side * style.Bow * far * Mathf.Sin(Mathf.PI * u)) + Vector3.up * lift;
        }
    }

    /// <summary>
    /// A stand that steps, as a pattern (the plan's step S4: the ginga): where the feet go, in what order, and how
    /// the body goes with them, made step by step on the standard set's own stance and never the same twice. Nothing
    /// of a recording is played. What the pattern is comes from the style's profile: the two poses the ginga rests
    /// in (one foot back, then the other), measured on the pack's four takes: where each foot stands, where the
    /// hips are over them, how the hips and the chest are turned, how the arms are held.
    ///
    /// <para><b>The ginga, as measured.</b> He rests with one foot back, behind and a little outside the other,
    /// the hips over the front foot and turned toward the back one, the chest turned the other way so it stays
    /// square, the arm on the back foot's side across his face. To change sides the back foot sweeps forward to
    /// its place beside the front one (a long step); as it lands the other foot sweeps back behind it (another);
    /// the hips shuttle across under him the whole time, rising a little, and sink as he arrives. Then he rests,
    /// a third of a second in the recordings, and changes back.</para>
    /// <para><b>What is made of it.</b> Each change of sides is made as a short clip the walk stands in: the feet
    /// by <see cref="LongStep"/>, the standing foot not moved at all; the hips, the turns and the arms from the
    /// one resting pose to the other; everything else the standard stance's own (a style's manner is laid over it
    /// by the walk: <see cref="StyleManner"/>). How big, how fast, how restless and how varied are dials.</para>
    /// </summary>
    public sealed class StandPattern
    {
        /// <summary>The capoeira style stands in the ginga made of its pattern (the scene's Combat Feel, "Ginga Is Made Of Its Pattern"); off: the recorded take, looped, as the first pass had it.</summary>
        public static bool Generated = true;

        /// <summary>How big the pattern is made against the recordings' (their feet's places, their hips' travel; 1: as recorded, the front feet 1.05 m apart and the step back 0.95 m behind).</summary>
        public static float Size = 0.7f;

        /// <summary>How fast its steps are taken against the recordings' (1: as recorded, 0.38 s a metre).</summary>
        public static float Tempo = 1f;

        /// <summary>How restless he is: how soon he changes sides again (1: after a third of a second, as the recordings; 0.4: after a second and more; near 0: he stays as he stands).</summary>
        public static float Restless = 0.4f;

        /// <summary>How much one change of sides differs from the next (0: every one alike; 1: the feet's places a tenth of the pattern's size apart, the rests up to six tenths longer or shorter, the steps an eighth quicker or slower).</summary>
        public static float Vary = 0.5f;

        /// <summary>A change of sides asked for now (the duel lab's button): counted up; each pattern takes it once, at its next rest.</summary>
        public static int ChangesAsked;

        public const float Rate = 60f;

        /// <summary>How far forward of the straight line between the two rests the hips pass in the middle of a change, how far they rise there, and how far they sink into a rest (leg lengths at the recordings' size).</summary>
        public const float HipsForward = 0.28f, HipsRise = 0.08f, RestSink = 0.03f;

        /// <summary>The hips start across this long before the first foot lifts, and he has settled this long after the second lands (seconds).</summary>
        public const float HipsLead = 0.06f, Settle = 0.1f;

        /// <summary>A foot nearer its place than this when he comes to stand is left where it is (leg lengths).</summary>
        public const float NearEnough = 0.08f;

        private readonly Carriage rightBack, leftBack;
        private readonly BodyTracks stance;
        private readonly System.Random random;
        private static int made;

        private Vector3 footL, footR, origin;
        private bool rightIsBack;
        private int asked;
        private float swayAt;

        /// <summary>The clip he is standing in rests until this moment of it (seconds), then changes sides.</summary>
        public float RestEnds { get; private set; }

        /// <summary>How many changes of sides it has made.</summary>
        public int Changes { get; private set; }

        private StandPattern(in Carriage rightBack, in Carriage leftBack, in BodyTracks stance)
        {
            this.rightBack = rightBack;
            this.leftBack = leftBack;
            this.stance = stance;
            random = new System.Random(7919 * ++made);
            asked = ChangesAsked;
        }

        /// <summary>The ginga's pattern from a style's profile, on a stance (its clip's first pose); null when the profile has not the ginga's two poses.</summary>
        public static StandPattern Ginga(StyleProfile profile, LocoClip stanceClip)
        {
            if (profile == null || stanceClip == null || !profile.Pose("GingaRightBack", out var right) || !profile.Pose("GingaLeftBack", out var left))
            {
                return null;
            }

            var p = new Vector3[LocoClip.BoneCount];
            stanceClip.Pose(0f, p, 1f);
            return new StandPattern(right, left, BodyMeasures.OfClips.Decompose(p));
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

        private static float Smooth(float u)
        {
            u = Mathf.Clamp01(u);
            return u * u * (3f - 2f * u);
        }

        /// <summary>A number from -1 to 1, as much of it as the variation asks.</summary>
        private float Varies() => Vary * (float)(random.NextDouble() * 2.0 - 1.0);

        /// <summary>Where a foot stands when it is the front one, and when it is the back one (the pattern's own frame at the recordings' size, on the ground).</summary>
        private Vector3 Front(int foot) => Flat(foot == 0 ? rightBack.Mid.AnkleL : leftBack.Mid.AnkleR);
        private Vector3 Back(int foot) => Flat(foot == 0 ? leftBack.Mid.AnkleL : rightBack.Mid.AnkleR);

        /// <summary>
        /// The body as it rests on one side: the standard stance's own, with its feet where they stand, its hips over
        /// the front foot as the recorded pose has them, turned as it turns them, the chest turned on them as it is,
        /// the head half way back to square, the arms the pose's.
        /// </summary>
        private BodyTracks Rest(bool rightBackNow, Vector3 l, Vector3 r, bool hipsOverRoot = false)
        {
            var pose = rightBackNow ? rightBack : leftBack;
            var t = stance;
            Vector3 front = rightBackNow ? l : r, frontPlace = Front(rightBackNow ? 0 : 1);
            Vector3 hips = hipsOverRoot ? Vector3.zero : front + (Flat(pose.Mid.Pelvis) - frontPlace) * Size;
            t.Pelvis = new Vector3(hips.x, stance.Pelvis.y - RestSink, hips.z);
            t.PelvisTurn.y = pose.Mid.PelvisTurn.y;
            t.ChestTurn.y = pose.Mid.ChestTurn.y;
            t.HeadTurn.y = -0.5f * (t.PelvisTurn.y + t.ChestTurn.y);
            t.ClavL = pose.Mid.ClavL; t.ClavR = pose.Mid.ClavR; t.UpperL = pose.Mid.UpperL; t.UpperR = pose.Mid.UpperR;
            t.ForeL = pose.Mid.ForeL; t.ForeR = pose.Mid.ForeR; t.HandL = pose.Mid.HandL; t.HandR = pose.Mid.HandR;
            t.AnkleL = new Vector3(l.x, stance.AnkleL.y, l.z);
            t.AnkleR = new Vector3(r.x, stance.AnkleR.y, r.z);
            if (!rightBackNow)
            {
                // (The standard stance has its left foot forward: with the right forward the feet lie as its mirror.)
                t.FootL = Mirrored(stance.FootR); t.FootR = Mirrored(stance.FootL);
                t.ToeL = Mirrored(stance.ToeR); t.ToeR = Mirrored(stance.ToeL);
            }

            return t;
        }

        private static Vector3 Mirrored(Vector3 v) => new Vector3(-v.x, v.y, v.z);

        private struct Move
        {
            public int Foot;
            public Vector3 From, To;
            public float Start, Time;
        }

        /// <summary>
        /// He has come to stand, his feet where they are (the walk's root frame, leg lengths): the pattern is laid
        /// out about him on the side his feet are nearest (the foot that is behind stays the back one), and the
        /// first clip steps each foot that is off its place onto it.
        /// </summary>
        public LocoClip Begin(Vector3 ankleL, Vector3 ankleR)
        {
            rightIsBack = ankleR.z <= ankleL.z;
            footL = Flat(ankleL);
            footR = Flat(ankleR);
            // (Laid out so that the side's resting hips are where his hips are now: over the root.)
            origin = -Flat((rightIsBack ? rightBack : leftBack).Mid.Pelvis) * Size;
            int back = rightIsBack ? 1 : 0, front = 1 - back;
            Vector3 backTo = origin + Back(back) * Size, frontTo = origin + Front(front) * Size;
            var moves = new List<Move>();
            float at = 0f;
            foreach (var (foot, to) in new[] { (back, backTo), (front, frontTo) })
            {
                Vector3 from = foot == 0 ? footL : footR;
                if ((to - from).magnitude > NearEnough)
                {
                    float time = LongStep.Time(from, to, LongStep.Style) / Mathf.Max(0.3f, Tempo);
                    moves.Add(new Move { Foot = foot, From = from, To = to, Start = at, Time = time });
                    at += time + 0.03f;
                }
            }

            Vector3 l0 = footL, r0 = footR;
            foreach (var move in moves)
            {
                if (move.Foot == 0) footL = move.To; else footR = move.To;
            }

            RestEnds = 0f;
            asked = ChangesAsked;
            return Segment(rightIsBack, rightIsBack, l0, r0, footL, footR, moves, 0f, 1f, Mathf.Max(0.1f, at + 0.05f), true);
        }

        /// <summary>The next clip to stand in: he rests on the side he is on, as long as his restlessness lets him, and changes sides.</summary>
        public LocoClip Next()
        {
            float tempo = Mathf.Max(0.3f, Tempo * (1f + 0.12f * Varies()));
            float rest = Mathf.Clamp(0.3f / Mathf.Pow(Mathf.Max(0.05f, Restless), 1.5f) * (1f + 0.6f * Varies()) - Settle, HipsLead, 8f);
            int first = rightIsBack ? 1 : 0, second = 1 - first;
            // (Each foot's place, a little off the pattern's own each time.)
            Vector3 Place(Vector3 station) => origin + station * (Size * (1f + 0.1f * Varies())) + new Vector3(Varies(), 0f, Varies()) * (0.04f * Size);
            Vector3 firstFrom = first == 0 ? footL : footR, secondFrom = second == 0 ? footL : footR;
            Vector3 firstTo = Place(Front(first)), secondTo = Place(Back(second));
            float firstTime = LongStep.Time(firstFrom, firstTo, LongStep.Style) / tempo, secondTime = LongStep.Time(secondFrom, secondTo, LongStep.Style) / tempo;
            var moves = new List<Move>
            {
                new Move { Foot = first, From = firstFrom, To = firstTo, Start = rest, Time = firstTime },
                new Move { Foot = second, From = secondFrom, To = secondTo, Start = rest + firstTime + 0.03f, Time = secondTime },
            };
            Vector3 l0 = footL, r0 = footR;
            footL = first == 0 ? firstTo : secondTo;
            footR = first == 1 ? firstTo : secondTo;
            float ends = rest + firstTime + 0.03f + secondTime;
            RestEnds = rest;
            bool was = rightIsBack;
            rightIsBack = !rightIsBack;
            Changes++;
            return Segment(was, rightIsBack, l0, r0, footL, footR, moves, rest - HipsLead, ends, ends + Settle, false);
        }

        /// <summary>A change of sides has been asked for since this was last asked (the duel lab's button): once for each asking.</summary>
        public bool ChangeAsked()
        {
            if (asked == ChangesAsked)
            {
                return false;
            }

            asked = ChangesAsked;
            return true;
        }

        private LocoClip Segment(bool fromRightBack, bool toRightBack, Vector3 l0, Vector3 r0, Vector3 l1, Vector3 r1, List<Move> moves, float bodyFrom, float bodyTo, float total, bool entry)
        {
            var m = BodyMeasures.OfClips;
            int frames = Mathf.Max(2, Mathf.CeilToInt(total * Rate) + 1);
            var data = new LocoClipData
            {
                name = "ginga", role = "guard_idle", kind = "", set = "guard", loop = false, rate = Rate, frames = frames, legLength = Locomotor.ClipLeg,
                duration = (frames - 1) / Rate, root = new float[frames * 3], points = new float[frames * LocoClip.BoneCount * 3], contacts = new int[frames * 2],
                about = "the ginga, made of its pattern",
            };
            // (Coming to stand, the hips begin over the root, where they are, and go to their place over the front foot as the feet are brought to theirs.)
            BodyTracks from = Rest(fromRightBack, l0, r0, entry), to = Rest(toRightBack, l1, r1);
            var p = new Vector3[LocoClip.BoneCount];
            float size = Mathf.Max(0.1f, Size);
            int down = (int)(LocoContact.Down | LocoContact.Heel | LocoContact.Toe);
            for (int f = 0; f < frames; f++)
            {
                float time = Mathf.Min(f / Rate, total);
                float s = entry ? Smooth(time / Mathf.Max(0.05f, total)) : Smooth((time - bodyFrom) / Mathf.Max(0.05f, bodyTo - bodyFrom));
                var t = BodyTracks.Lerp(from, to, s);
                if (!entry)
                {
                    // (The hips pass forward of the straight line and rise as they cross, and sink as he arrives.)
                    float crossing = Mathf.Sin(Mathf.PI * s);
                    t.Pelvis.z += HipsForward * size * crossing;
                    t.Pelvis.y += (HipsRise * size + RestSink) * crossing;
                }

                // (Resting he is not quite still: he breathes, and his chest turns a little.)
                float sway = swayAt + time;
                t.Pelvis.y -= 0.012f * (0.5f - 0.5f * Mathf.Cos(2f * Mathf.PI * sway / 2.6f));
                t.ChestTurn.y += 1.5f * Mathf.Sin(2f * Mathf.PI * sway / 3.4f);

                // The feet: each where it stands, or on its step.
                for (int foot = 0; foot < 2; foot++)
                {
                    Vector3 at = foot == 0 ? l0 : r0;
                    float gone = 0f;
                    bool air = false;
                    foreach (var move in moves)
                    {
                        if (move.Foot != foot || time < move.Start)
                        {
                            continue;
                        }

                        float u = Mathf.Clamp01((time - move.Start) / Mathf.Max(0.02f, move.Time));
                        Vector3 planted = foot == 0 ? (time >= move.Start ? CurrentOf(1, moves, r0, time) : r0) : CurrentOf(0, moves, l0, time);
                        at = LongStep.Swing(move.From, move.To, u, planted, LongStep.Style);
                        gone = u;
                        air = u > 0f && u < 1f;
                    }

                    float ground = foot == 0 ? stance.AnkleL.y : stance.AnkleR.y;
                    Vector3 ankle = new Vector3(at.x, ground + at.y, at.z);
                    if (foot == 0)
                    {
                        t.AnkleL = ankle;
                        t.FootL = BodyTracks.Way(from.FootL, to.FootL, gone);
                        t.ToeL = BodyTracks.Way(from.ToeL, to.ToeL, gone);
                    }
                    else
                    {
                        t.AnkleR = ankle;
                        t.FootR = BodyTracks.Way(from.FootR, to.FootR, gone);
                        t.ToeR = BodyTracks.Way(from.ToeR, to.ToeR, gone);
                    }

                    data.contacts[f * 2 + foot] = air ? 0 : down;
                }

                // (The knees point ahead of him, however the hips are turned.)
                var hips = Quaternion.Inverse(Quaternion.Euler(t.PelvisTurn));
                t.KneeL = hips * Vector3.forward;
                t.KneeR = hips * Vector3.forward;
                m.Compose(t, p);
                for (int i = 0; i < LocoClip.BoneCount; i++)
                {
                    int o = (f * LocoClip.BoneCount + i) * 3;
                    data.points[o] = p[i].x;
                    data.points[o + 1] = p[i].y;
                    data.points[o + 2] = p[i].z;
                }
            }

            swayAt += total;
            return new LocoClip(data);
        }

        /// <summary>Where a foot stands at a time, on the ground: its place before its steps, or after the last it has finished (a foot in the air counts as where it left).</summary>
        private static Vector3 CurrentOf(int foot, List<Move> moves, Vector3 start, float time)
        {
            Vector3 at = start;
            foreach (var move in moves)
            {
                if (move.Foot == foot && time >= move.Start + move.Time)
                {
                    at = move.To;
                }
            }

            return at;
        }
    }
}
