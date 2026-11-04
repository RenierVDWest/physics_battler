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

        /// <summary>A combo of cores: how far the next blow lands off the way the last one did, which its aim turns the body by in its wind-up (degrees).</summary>
        public float Aim;

        /// <summary>A combo of cores: how well the weapon hand goes on the way it was moving into the next blow's start (1 straight on, -1 straight back), and the actor did the two in a row (<see cref="StrikeCombos.MocapPairs"/>).</summary>
        public float Flow = 1f;
        public bool Mocap;

        public override string ToString() => $"{From.Name} at {At:0.00} s -> {To.Name} from {Entry:0.00} s (turn {Turn:+0;-0}{(Aim != 0f ? $", aim {Aim:+0;-0}" : "")}, cost {Cost:0.00})";
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
        // ---- Combos of cores (the plan "Combo feel"): Bam! Bam! BAM! -----------------------------------------------

        /// <summary>A follow-on's chamber (its core's lead) plays in about this long (seconds), quickened as far as <see cref="MostChamberSpeed"/>; a normal core's at <see cref="ChamberSpeed"/>.</summary>
        public const float CoreChamber = 0.42f, MostChamberSpeed = 2f;

        /// <summary>A combo's opener is taken up from the guard this far before its contact (seconds of its clip): its wind-up's start is skipped, so its beat is a follow-on's.</summary>
        public const float OpenLead = 0.4f;

        /// <summary>
        /// The most a blow may land off the way the body faces where a combo takes it up (degrees): an opener is taken up
        /// earlier until it lands within this of straight ahead (a spinning chop from before its spin); a follow-on whose
        /// aim would turn the body more is not chosen (a spinning chop never ends a combo).
        /// </summary>
        public const float MostAim = 45f;

        /// <summary>Where a combo's opener is taken up: <see cref="OpenLead"/> before contact (a leap at its takeoff), or earlier, as late as it still lands within <see cref="MostAim"/> of straight ahead.</summary>
        public static float OpenerEntry(LocoClip clip)
        {
            if (!StrikeCores.TryGet(clip, out var core))
            {
                return 0f;
            }

            float from = core.Leap ? core.Entry : Mathf.Max(0f, core.Contact - OpenLead);
            float best = from, least = float.MaxValue;
            for (float t = from; t >= 0f; t -= 1f / 60f)
            {
                float off = Mathf.Abs(StrikeCores.Bearing(clip, t));
                if (off <= MostAim)
                {
                    return t;
                }

                if (off < least)
                {
                    least = off;
                    best = t;
                }
            }

            return best;
        }

        /// <summary>
        /// What a link's turns add to its cost a full 90 degrees: the root's turn at the link (the hips lined up), and
        /// the aim's turn onto the target the next blow needs in its wind-up (<see cref="StrikeLink.Aim"/>), past
        /// <see cref="FreeAim"/>. A combo that keeps facing its target reads as one fighter's; one that spins doesn't.
        /// </summary>
        public const float CoreTurnWeight = 0.4f, AimWeight = 2.5f, FreeAim = 15f;

        /// <summary>What a link's hand going back the way it came adds to its cost (all the way back: this much), and what a pair the actor did in a row takes off it.</summary>
        public const float FlowWeight = 0.6f, MocapBonus = 0.6f;

        /// <summary>The strikes the mocap actor did in a row, first then next (the four-cut combo; the Large's three-strike sets): the most natural links there are.</summary>
        public static readonly (string, string)[] MocapPairs =
        {
            ("sns_combo_forehand", "sns_combo_backhand"), ("sns_combo_backhand", "sns_combo_chop"),
            ("lg_slash_a", "lg_slash_b"), ("lg_slash_b", "lg_slash_c"), ("lg_club_a", "lg_club_b"), ("lg_club_b", "lg_club_c"),
        };

        private static bool IsMocapPair(LocoClip a, LocoClip b)
        {
            foreach (var (x, y) in MocapPairs)
            {
                if (a.Name == x && b.Name == y)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The chamber speed (<see cref="Locomotor.Chamber"/>) a core of this lead plays at: up to the most, so on average it takes <see cref="CoreChamber"/> (the quickened pace eases back to the clip's own by contact).</summary>
        public static float CoreChamberSpeed(float lead)
        {
            // (Lerp(s, 1, u^2) averages (2s + 1) / 3.)
            float average = lead / CoreChamber;
            return Mathf.Clamp((3f * average - 1f) / 2f, ChamberSpeed, MostChamberSpeed);
        }

        /// <summary>How two strikes' poses meet, <paramref name="a"/> left at <paramref name="at"/> and <paramref name="b"/> taken up at <paramref name="entry"/>: the root's turn and move that line the hips up, and the cost (no time in it).</summary>
        public static StrikeLink Meet(LocoClip a, float at, LocoClip b, float entry)
        {
            var link = new StrikeLink { From = a, To = b, At = at, Entry = entry };
            PoseAt(a, Mathf.Clamp(at, 0f, a.Duration), a0, a1);
            PoseAt(b, Mathf.Clamp(entry, 0f, b.Duration), b0, b1);
            float turn = Mathf.DeltaAngle(HipsYaw(b0), HipsYaw(a0));
            Turned(b0, turn);
            Turned(b1, turn);
            Vector3 shift = a0[(int)LocoBone.Hips] - b0[(int)LocoBone.Hips];
            shift.y = 0f;
            Shifted(b0, shift);
            Shifted(b1, shift);
            link.Turn = turn;
            link.Shift = shift;
            link.Cost = Cost(a0, a1, b0, b1);
            // (The figure-eight: the weapon hand goes on the way it was moving, round into the next blow's chamber,
            // rather than back the way it came. 1: straight on; -1: straight back.)
            Vector3 moving = a1[(int)LocoBone.RightHand] - a0[(int)LocoBone.RightHand];
            Vector3 toNext = b0[(int)LocoBone.RightHand] - a0[(int)LocoBone.RightHand];
            link.Flow = moving.sqrMagnitude > 1e-8f && toNext.magnitude > 0.1f ? Vector3.Dot(moving.normalized, toNext.normalized) : 1f;
            return link;
        }

        private static readonly Dictionary<(LocoClip, LocoClip), StrikeLink> coreLinks = new Dictionary<(LocoClip, LocoClip), StrikeLink>();

        /// <summary>One core into the next: <paramref name="a"/> cut at its exit, <paramref name="b"/> taken up at its entry (worked out once). Its cost adds the root's turn.</summary>
        public static StrikeLink CoreLink(LocoClip a, LocoClip b)
        {
            if (!coreLinks.TryGetValue((a, b), out var link))
            {
                StrikeCores.TryGet(a, out var ca);
                StrikeCores.TryGet(b, out var cb);
                link = Meet(a, ca.Exit, b, cb.Entry);
                // (The last blow landed on its target; the root turns on with it to its exit, then by the link's turn,
                // and the next lands at its bearing from there: the aim turns the body by what that is off the target.)
                a.RootMotion(ca.Contact, ca.Exit, out _, out float turnedOn);
                link.Aim = Mathf.DeltaAngle(turnedOn + link.Turn + StrikeCores.Bearing(b, cb.Entry), StrikeCores.Bearing(a, ca.Contact));
                link.Mocap = IsMocapPair(a, b);
                link.Cost += CoreTurnWeight * Mathf.Abs(link.Turn) / 90f + AimWeight * Mathf.Max(0f, Mathf.Abs(link.Aim) - FreeAim) / 90f
                             + FlowWeight * 0.5f * (1f - link.Flow) - (link.Mocap ? MocapBonus : 0f);
                coreLinks[(a, b)] = link;
            }

            return link;
        }

        /// <summary>The strikes that may end a combo: the leaps, or with none, the heaviest (<see cref="StrikeCore.Weight"/>); a leap never opens one or goes in its middle.</summary>
        public static List<LocoClip> Finishers(IList<LocoClip> strikes)
        {
            var leaps = new List<LocoClip>();
            LocoClip heaviest = null;
            float most = float.MinValue;
            foreach (var c in strikes)
            {
                if (!StrikeCores.TryGet(c, out var core))
                {
                    continue;
                }

                if (core.Leap)
                {
                    leaps.Add(c);
                }

                if (core.Weight > most)
                {
                    most = core.Weight;
                    heaviest = c;
                }
            }

            if (leaps.Count == 0 && heaviest != null)
            {
                leaps.Add(heaviest);
            }

            return leaps;
        }

        /// <summary>
        /// A combo of cores from the picked strike, Bam! Bam! BAM!: three blows ending on a finisher (<see cref="Finishers"/>).
        /// A finisher picked ends the combo, two quick blows before it; any other opens it. The rest are the quick strikes
        /// whose cores meet best, the links' costs summed (a heavier finisher a little cheaper). The links in order:
        /// the first's <see cref="StrikeLink.From"/> is the opener.
        /// </summary>
        /// <summary>Combos within this much of the best one's cost are as good: the lab goes round them, one each time (variety).</summary>
        public const float ChainSlack = 0.5f;

        /// <summary>
        /// A combo of cores from the picked strike, Bam! Bam! BAM!: three blows ending on a finisher (<see cref="Finishers"/>).
        /// A finisher picked ends the combo, two quick blows before it; any other opens it. The rest are the quick strikes
        /// whose cores meet best, the links' costs summed (a heavier finisher a little cheaper), no link turning the body
        /// more than <see cref="MostAim"/> onto the target. The links in order: the first's <see cref="StrikeLink.From"/>
        /// is the opener. <paramref name="variant"/> picks among the combos as good as the best (<see cref="CoreChains"/>).
        /// </summary>
        public static List<StrikeLink> CoreChain(LocoClip picked, IList<LocoClip> strikes, int count = 3, int variant = 0)
        {
            var chains = CoreChains(picked, strikes, count);
            return chains.Count == 0 ? new List<StrikeLink>() : chains[((variant % chains.Count) + chains.Count) % chains.Count];
        }

        /// <summary>The combos the picked strike is in, best first, those within <see cref="ChainSlack"/> of the best.</summary>
        public static List<List<StrikeLink>> CoreChains(LocoClip picked, IList<LocoClip> strikes, int count = 3)
        {
            var found = new List<(float cost, List<StrikeLink> links)>();
            if (picked == null || !StrikeCores.TryGet(picked, out _))
            {
                return new List<List<StrikeLink>>();
            }

            var finishers = Finishers(strikes);
            var quick = new List<LocoClip>();
            foreach (var c in strikes)
            {
                if (StrikeCores.TryGet(c, out var core) && !core.Leap && !finishers.Contains(c))
                {
                    quick.Add(c);
                }
            }

            bool pickedEnds = finishers.Contains(picked);
            foreach (float mostAim in new[] { MostAim, 180f })
            {
                Collect(found, pickedEnds ? quick : new List<LocoClip> { picked }, quick, pickedEnds ? new List<LocoClip> { picked } : finishers, count, mostAim);
                if (found.Count == 0 && pickedEnds && mostAim < 180f)
                {
                    // (A showstopper picked that no blow can lead into facing its target (a spinning chop): it opens instead.)
                    var others = new List<LocoClip>();
                    foreach (var c in quick)
                    {
                        if (c != picked)
                        {
                            others.Add(c);
                        }
                    }

                    var ends = new List<LocoClip>();
                    foreach (var c in finishers)
                    {
                        if (c != picked)
                        {
                            ends.Add(c);
                        }
                    }

                    Collect(found, new List<LocoClip> { picked }, others, ends, count, mostAim);
                }

                if (found.Count > 0)
                {
                    break;
                }
            }

            found.Sort((x, y) => x.cost.CompareTo(y.cost));
            var chains = new List<List<StrikeLink>>();
            foreach (var f in found)
            {
                if (f.cost <= found[0].cost + ChainSlack)
                {
                    chains.Add(f.links);
                }
            }

            return chains;
        }

        private static void Collect(List<(float, List<StrikeLink>)> found, List<LocoClip> openers, List<LocoClip> middles, List<LocoClip> ends, int count, float mostAim)
        {
            float Weight(LocoClip c) => StrikeCores.TryGet(c, out var core) ? core.Weight : 0f;
            bool Turns(StrikeLink l) => Mathf.Abs(l.Aim) > mostAim;
            foreach (var a in openers)
            {
                foreach (var c in ends)
                {
                    if (c == a)
                    {
                        continue;
                    }

                    // (Too few strikes for a middle one: two blows, the finisher last.)
                    bool middle = false;
                    if (count >= 3)
                    {
                        foreach (var b in middles)
                        {
                            if (b == a || b == c || Turns(CoreLink(a, b)) || Turns(CoreLink(b, c)))
                            {
                                continue;
                            }

                            middle = true;
                            found.Add((CoreLink(a, b).Cost + CoreLink(b, c).Cost - 0.2f * Weight(c), new List<StrikeLink> { CoreLink(a, b), CoreLink(b, c) }));
                        }
                    }

                    if (!middle && (count < 3 || middles.Count <= 1) && !Turns(CoreLink(a, c)))
                    {
                        found.Add((CoreLink(a, c).Cost - 0.2f * Weight(c), new List<StrikeLink> { CoreLink(a, c) }));
                    }
                }
            }
        }
    }
}
