using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>One strike going on into another: where the first is left (<see cref="At"/>, seconds of its own
    /// time) and where the second is taken up (<see cref="Entry"/>), and how well the two poses meet there.</summary>
    public sealed class StrikeLink
    {
        public LocoClip From, To;
        public float At, Entry, Cost;

        /// <summary>
        /// How far the root's facing turns at the link (degrees): each clip's root faces the way its hips and head
        /// faced where it was cut, so two clips' roots can differ by a good deal where their bodies meet. Turned by
        /// this, the second strike's hips face the way the first's do.
        /// </summary>
        public float Turn;

        /// <summary>How far the root moves at the link (metres at a Medium's size, against the first strike's root
        /// then): the clips' roots sit a little differently under their hips, so the second's hips land on the first's.</summary>
        public Vector3 Shift;

        public override string ToString() => $"{From.Name} at {At:0.00} s -> {To.Name} from {Entry:0.00} s (turn {Turn:+0;-0}, cost {Cost:0.00})";
    }

    /// <summary>
    /// Combos from single strikes (the combat plan's step 5): for every pair of strikes, the moment in the
    /// first's recovery and the moment in the second's wind-up where the two poses meet best (the hands, the
    /// weapon's line, the hips, the feet, and how the hands are moving), as a cost. A low cost is a natural
    /// follow-on (a forehand into a backhand); the pairs the mocap actor did in a row meet at almost nothing.
    /// A combo is a chain of strikes, each the best follow-on of the last that the chain hasn't used.
    /// </summary>
    public static class StrikeCombos
    {
        /// <summary>The second strike keeps at least this much of its wind-up before contact (seconds).</summary>
        public const float LeastWindUp = 0.25f;

        /// <summary>The first strike is left no sooner than this after its contact (seconds): once the blade is through.</summary>
        public const float LeaveAfter = 0.1f;

        /// <summary>
        /// The time from one blow's contact to the next's is part of a link's cost (a combo is a rhythm: Bam! Bam!
        /// Bam!): this much a second of it, and this much more a second past <see cref="BeatWithin"/>.
        /// </summary>
        public const float TimeWeight = 0.2f, LateWeight = 2.5f, BeatWithin = 0.55f;

        /// <summary>What the time between two contacts adds to a link's cost (seconds of the clips).</summary>
        /// <summary>A follow-on's chamber (its wind-up from where it is taken up) plays up to this much faster (<see cref="Locomotor.Chamber"/>).</summary>
        public const float ChamberSpeed = 1.2f;

        /// <summary>The seconds as they pass of a follow-on's wind-up of so many seconds of its clip, its chamber quickened (easing back to its own pace by contact).</summary>
        public static float Chambered(float windUp) => windUp / (0.5f * (ChamberSpeed + 1f) + (ChamberSpeed - 1f) / 6f);

        public static float TimeCost(float beat) => TimeWeight * beat + LateWeight * Mathf.Max(0f, beat - BeatWithin);

        /// <summary>The cost's weights: metres of a hand, of the hips, of a foot; radians of the weapon's line; m/s of a hand.</summary>
        public const float HandWeight = 1f, OffHandWeight = 0.4f, HipsWeight = 1f, FootWeight = 0.6f, BladeWeight = 0.25f, MotionWeight = 0.08f;

        private static readonly Vector3[] a0 = new Vector3[LocoClip.BoneCount], a1 = new Vector3[LocoClip.BoneCount];
        private static readonly Vector3[] b0 = new Vector3[LocoClip.BoneCount], b1 = new Vector3[LocoClip.BoneCount];

        /// <summary>The way the right hand's weapon points in a pose (its knuckles' line, leaned toward the fingers).</summary>
        internal static Vector3 Blade(Vector3[] p)
        {
            Vector3 wrist = p[(int)LocoBone.RightHand];
            Vector3 along = (p[(int)LocoBone.RightHandMiddle] - wrist).normalized;
            Vector3 side = (p[(int)LocoBone.RightHandIndex] - p[(int)LocoBone.RightHandPinky]).normalized;
            return (side * Mathf.Cos(20f * Mathf.Deg2Rad) + along * Mathf.Sin(20f * Mathf.Deg2Rad)).normalized;
        }

        /// <summary>A clip's pose at a time, in metres at a Medium's size, against its root then; and a moment later.</summary>
        private static void PoseAt(LocoClip clip, float t, Vector3[] now, Vector3[] next)
        {
            const float leg = 0.845f;
            clip.Pose(t, now, leg);
            clip.Pose(Mathf.Min(clip.Duration, t + 0.05f), next, leg);
            // (The moment later against the root now: the root's own move taken out.)
            clip.RootMotion(t, Mathf.Min(clip.Duration, t + 0.05f), out var moved, out float turned);
            var turn = Quaternion.Euler(0f, turned, 0f);
            Vector3 step = new Vector3(moved.x, 0f, moved.y) * leg;
            for (int i = 0; i < next.Length; i++)
            {
                next[i] = turn * next[i] + step;
            }
        }

        /// <summary>The way the hips face in a pose (degrees, against its root).</summary>
        public static float HipsYaw(Vector3[] p)
        {
            Vector3 right = p[(int)LocoBone.RightUpLeg] - p[(int)LocoBone.LeftUpLeg];
            right.y = 0f;
            Vector3 forward = Vector3.Cross(right, Vector3.up);
            return Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        }

        private static void Turned(Vector3[] p, float degrees)
        {
            var turn = Quaternion.Euler(0f, degrees, 0f);
            for (int i = 0; i < p.Length; i++)
            {
                p[i] = turn * p[i];
            }
        }

        private static void Shifted(Vector3[] p, Vector3 by)
        {
            for (int i = 0; i < p.Length; i++)
            {
                p[i] += by;
            }
        }

        private static float Cost(Vector3[] a, Vector3[] aNext, Vector3[] b, Vector3[] bNext)
        {
            float D(LocoBone bone) => (a[(int)bone] - b[(int)bone]).magnitude;
            float V(LocoBone bone) => ((aNext[(int)bone] - a[(int)bone]) - (bNext[(int)bone] - b[(int)bone])).magnitude / 0.05f;
            float blade = Vector3.Angle(Blade(a), Blade(b)) * Mathf.Deg2Rad;
            return HandWeight * D(LocoBone.RightHand) + OffHandWeight * D(LocoBone.LeftHand) + HipsWeight * D(LocoBone.Hips)
                   + FootWeight * (D(LocoBone.LeftFoot) + D(LocoBone.RightFoot)) + BladeWeight * blade
                   + MotionWeight * (V(LocoBone.RightHand) + 0.5f * V(LocoBone.Hips));
        }

        /// <summary>The best place to go from <paramref name="a"/> into <paramref name="b"/>.</summary>
        public static StrikeLink Best(LocoClip a, LocoClip b)
        {
            var best = new StrikeLink { From = a, To = b, Cost = float.MaxValue };
            if (a == null || b == null || a.ContactFrame < 0 || b.ContactFrame < 0)
            {
                return best;
            }

            float aContact = a.ContactFrame / a.Rate;
            float linkFrom = Mathf.Min(aContact + LeaveAfter, a.Duration);
            float bContact = b.ContactFrame / b.Rate;
            float lastEntry = Mathf.Max(0f, bContact - LeastWindUp);
            float step = 1f / Mathf.Max(a.Rate, b.Rate);
            for (float ta = linkFrom; ta <= a.Duration + 1e-4f; ta += step)
            {
                PoseAt(a, ta, a0, a1);
                float yawA = HipsYaw(a0);
                for (float tb = 0f; tb <= lastEntry + 1e-4f; tb += step)
                {
                    PoseAt(b, tb, b0, b1);
                    // (Compared with the hips facing the same way: the root turns by that at the link.)
                    float turn = Mathf.DeltaAngle(HipsYaw(b0), yawA);
                    Turned(b0, turn);
                    Turned(b1, turn);
                    // (And with the hips over the same place.)
                    Vector3 shift = a0[(int)LocoBone.Hips] - b0[(int)LocoBone.Hips];
                    shift.y = 0f;
                    Shifted(b0, shift);
                    Shifted(b1, shift);
                    float cost = Cost(a0, a1, b0, b1) + TimeCost((ta - aContact) + Chambered(bContact - tb));
                    if (cost < best.Cost)
                    {
                        best.Cost = cost;
                        best.At = ta;
                        best.Entry = tb;
                        best.Turn = turn;
                        best.Shift = shift;
                    }
                }
            }

            return best;
        }

        /// <summary>
        /// The best way into <paramref name="b"/> from <paramref name="a"/> left at <paramref name="at"/> (its own time):
        /// a strike cut short (stopped by what it met) is left where it stopped, not at its link mark, and its
        /// body is turned and placed as it is there.
        /// </summary>
        public static StrikeLink BestAt(LocoClip a, float at, LocoClip b)
        {
            var best = new StrikeLink { From = a, To = b, Cost = float.MaxValue, At = at };
            if (a == null || b == null || b.ContactFrame < 0)
            {
                return best;
            }

            float bContactAt = b.ContactFrame / b.Rate;
            float lastEntry = Mathf.Max(0f, bContactAt - LeastWindUp);
            float since = a.ContactFrame >= 0 ? Mathf.Max(0f, at - a.ContactFrame / a.Rate) : 0f;
            float step = 1f / Mathf.Max(a.Rate, b.Rate);
            PoseAt(a, Mathf.Clamp(at, 0f, a.Duration), a0, a1);
            float yawA = HipsYaw(a0);
            for (float tb = 0f; tb <= lastEntry + 1e-4f; tb += step)
            {
                PoseAt(b, tb, b0, b1);
                float turn = Mathf.DeltaAngle(HipsYaw(b0), yawA);
                Turned(b0, turn);
                Turned(b1, turn);
                Vector3 shift = a0[(int)LocoBone.Hips] - b0[(int)LocoBone.Hips];
                shift.y = 0f;
                Shifted(b0, shift);
                Shifted(b1, shift);
                float cost = Cost(a0, a1, b0, b1) + TimeCost(since + Chambered(bContactAt - tb));
                if (cost < best.Cost)
                {
                    best.Cost = cost;
                    best.Entry = tb;
                    best.Turn = turn;
                    best.Shift = shift;
                }
            }

            return best;
        }

        /// <summary>For each place the first strike could be left, the best entry into the second and its pose cost (no lateness).</summary>
        public static List<(float at, float entry, float cost)> Curve(LocoClip a, LocoClip b)
        {
            var curve = new List<(float, float, float)>();
            float linkFrom = Mathf.Min(a.ContactFrame / a.Rate + LeaveAfter, a.Duration);
            float lastEntry = Mathf.Max(0f, b.ContactFrame / b.Rate - LeastWindUp);
            float step = 2f / Mathf.Max(a.Rate, b.Rate);
            for (float ta = linkFrom; ta <= a.Duration + 1e-4f; ta += step)
            {
                PoseAt(a, ta, a0, a1);
                float yawA = HipsYaw(a0), bestCost = float.MaxValue, bestEntry = 0f;
                for (float tb = 0f; tb <= lastEntry + 1e-4f; tb += step)
                {
                    PoseAt(b, tb, b0, b1);
                    float turn = Mathf.DeltaAngle(HipsYaw(b0), yawA);
                    Turned(b0, turn);
                    Turned(b1, turn);
                    Vector3 shift = a0[(int)LocoBone.Hips] - b0[(int)LocoBone.Hips];
                    shift.y = 0f;
                    Shifted(b0, shift);
                    Shifted(b1, shift);
                    float cost = Cost(a0, a1, b0, b1);
                    if (cost < bestCost)
                    {
                        bestCost = cost;
                        bestEntry = tb;
                    }
                }

                curve.Add((ta, bestEntry, bestCost));
            }

            return curve;
        }

        private static readonly Dictionary<(LocoClip, LocoClip), StrikeLink> links = new Dictionary<(LocoClip, LocoClip), StrikeLink>();

        /// <summary>The link between two strikes (worked out once).</summary>
        public static StrikeLink Link(LocoClip a, LocoClip b)
        {
            if (!links.TryGetValue((a, b), out var link))
            {
                link = Best(a, b);
                links[(a, b)] = link;
            }

            return link;
        }

        /// <summary>
        /// A combo of up to <paramref name="count"/> strikes from <paramref name="first"/>: each next the lowest
        /// cost follow-on of the last that the chain hasn't used, if it costs at most <paramref name="mostCost"/>.
        /// </summary>
        public static List<StrikeLink> Chain(LocoClip first, IList<LocoClip> strikes, int count, float mostCost = 2.2f)
        {
            var chain = new List<StrikeLink>();
            var used = new HashSet<LocoClip> { first };
            var last = first;
            while (chain.Count + 1 < count)
            {
                StrikeLink best = null;
                foreach (var next in strikes)
                {
                    if (used.Contains(next))
                    {
                        continue;
                    }

                    var link = Link(last, next);
                    if (link.Cost <= mostCost && (best == null || link.Cost < best.Cost))
                    {
                        best = link;
                    }
                }

                if (best == null)
                {
                    break;
                }

                chain.Add(best);
                used.Add(best.To);
                last = best.To;
            }

            return chain;
        }
    }
}
