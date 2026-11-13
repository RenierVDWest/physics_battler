using System.Collections.Generic;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The legs' opinion of a body (the plan "Grounded legs", step 0: see it; nothing here moves anything). Read from
    /// the body as the physics left it, each step: what each foot is doing (planted, sliding, landing, low in the air,
    /// up), where the body's weight is (its centre of gravity) and how fast it moves, the patch of ground its feet
    /// hold (the support: the outline round the parts of the soles that are down), and where a foot would have to be
    /// to stop the body as it moves now (the capture point: the weight's place carried on by its speed for as long as
    /// a body that tall takes to tip). A body is balanced while its weight is over its support, and can stop without a
    /// step while the capture point is.
    ///
    /// The same readings as <c>Tools/Playtest/ground.py</c> takes from a playtest log, here as the game runs: for the
    /// fight report, the log, and the debug view (the scene's Combat Feel, "Show Legs Opinion").
    /// </summary>
    public sealed class Grounding
    {
        public enum FootState
        {
            /// <summary>On the ground and still.</summary>
            Planted,

            /// <summary>On the ground and moving over <see cref="SlideFrom"/>.</summary>
            Sliding,

            /// <summary>Touching the ground this step, not the step before (or by another end of its sole).</summary>
            Landing,

            /// <summary>In the air, its sole under <see cref="LowUnder"/>: too low to be a step.</summary>
            Low,

            /// <summary>In the air, stepping.</summary>
            High,
        }

        public struct Foot
        {
            public FootState State;

            /// <summary>Some of its sole is on the ground.</summary>
            public bool Touching;

            /// <summary>Its heel's and its toe's end is on the ground.</summary>
            public bool HeelDown, ToeDown;

            /// <summary>The middle of its sole's heel end and of its toe end.</summary>
            public Vector3 Heel, Toe;

            /// <summary>The four corners of its sole: heel inner and outer, toe outer and inner (round its outline).</summary>
            public Vector3 A, B, C, D;

            /// <summary>The lowest point of its sole over the ground (metres).</summary>
            public float Lowest;

            /// <summary>How fast the part of its sole that is down moves along the ground (m/s; 0 in the air).</summary>
            public float Speed;

            /// <summary>How far it has slid in this slide so far (metres), and how long it has been in the air (seconds).</summary>
            public float Slid, UpFor;
        }

        /// <summary>A sole's end this close over the ground is on it (metres at a Medium's size).</summary>
        public const float Touch = 0.02f;

        /// <summary>A foot on the ground moving faster than this is sliding (m/s).</summary>
        public const float SlideFrom = 0.10f;

        /// <summary>A foot in the air with its sole under this is low, not stepping (metres at a Medium's size).</summary>
        public const float LowUnder = 0.09f;

        /// <summary>The feet: left, right.</summary>
        public readonly Foot[] Feet = new Foot[2];

        /// <summary>The body's centre of gravity, and its speed.</summary>
        public Vector3 Weight, WeightSpeed;

        /// <summary>Where a foot is needed to stop the body as it moves now (on the ground).</summary>
        public Vector3 Needed;

        /// <summary>The support: the outline, on the ground, round the parts of the soles that are down (x and z; empty with both feet up).</summary>
        public readonly List<Vector2> Support = new List<Vector2>();

        /// <summary>How far outside the support the weight is, and the place a foot is needed (metres; 0: inside; with both feet up, not a number).</summary>
        public float WeightOut, NeededOut;

        /// <summary>The ground's height under it.</summary>
        public float GroundY;

        /// <summary>Read at least twice running (its speeds are known).</summary>
        public bool Known { get; private set; }

        /// <summary>Both feet on the ground, one, none.</summary>
        public int FeetDown => (Feet[0].Touching ? 1 : 0) + (Feet[1].Touching ? 1 : 0);

        // (How high its ankles stand: what a foot's sole is measured from. The rig's own rest height is not it: the clips
        // stand a Large's ankle 2.5 cm lower than its rig has it at rest. Learnt as the fight goes, as the log's reading
        // learns it: the height its ankles are under one step in twenty.)
        private readonly int[] ankleSeen = new int[400];   // (how often an ankle was seen at each millimetre over the ground)
        private int anklesSeen;
        private float standing = -1f;

        /// <summary>How high over the ground its ankles stand when its feet are down (metres): the rest height at first, then learnt.</summary>
        public float StandingAnkle => standing;

        private Vector3 weightWas;
        private readonly Vector3[] heelWas = new Vector3[2], toeWas = new Vector3[2];
        private readonly bool[] heelWasDown = new bool[2], toeWasDown = new bool[2];
        private readonly List<Vector2> points = new List<Vector2>();

        private static readonly PartId[] FootParts = { PartId.FootL, PartId.FootR };

        // (Each part's weight sits half way to the part that hangs from it; a hand's, a foot's and the head's at the part itself, the head's a little up.)
        private static readonly (PartId part, PartId next)[] Limbs =
        {
            (PartId.Pelvis, PartId.Spine), (PartId.Spine, PartId.Chest), (PartId.Chest, PartId.Head),
            (PartId.UpperArmL, PartId.ForearmL), (PartId.ForearmL, PartId.HandL), (PartId.UpperArmR, PartId.ForearmR), (PartId.ForearmR, PartId.HandR),
            (PartId.ThighL, PartId.ShinL), (PartId.ShinL, PartId.FootL), (PartId.ThighR, PartId.ShinR), (PartId.ShinR, PartId.FootR),
        };

        private static readonly PartId[] Ends = { PartId.Head, PartId.HandL, PartId.HandR, PartId.FootL, PartId.FootR };

        /// <summary>Forgets the step before (the body was put somewhere else: a first step, a get-up).</summary>
        public void Forget() => Known = false;

        /// <summary>
        /// Reads the body as it is now.
        /// </summary>
        /// <param name="groundY">The ground's height under it.</param>
        /// <param name="size">Its size against a Medium's (1).</param>
        /// <param name="restAnkle">How high its ankle is over the ground in the rig at rest (metres): where its sole is taken to be until its standing height is learnt.</param>
        public void Read(UnitRig rig, float groundY, float size, float restAnkle, float dt)
        {
            if (rig == null)
            {
                Known = false;
                return;
            }

            GroundY = groundY;
            // The weight.
            Vector3 sum = Vector3.zero;
            float mass = 0f;
            foreach (var (part, next) in Limbs)
            {
                var p = rig.Part(part);
                var n = rig.Part(next);
                if (p == null || n == null || p.body == null)
                {
                    continue;
                }

                sum += p.body.mass * (0.5f * (p.transform.position + n.transform.position));
                mass += p.body.mass;
            }

            foreach (var part in Ends)
            {
                var p = rig.Part(part);
                if (p == null || p.body == null)
                {
                    continue;
                }

                sum += p.body.mass * (p.transform.position + (part == PartId.Head ? Vector3.up * (0.1f * size) : Vector3.zero));
                mass += p.body.mass;
            }

            if (mass <= 0f)
            {
                Known = false;
                return;
            }

            Weight = sum / mass;
            bool timed = Known && dt > 1e-6f;
            WeightSpeed = timed ? (Weight - weightWas) / dt : Vector3.zero;
            weightWas = Weight;

            // The feet.
            LearnStanding(rig, groundY, size, restAnkle);
            float touch = Touch * size;
            points.Clear();
            for (int f = 0; f < 2; f++)
            {
                var part = rig.Part(FootParts[f]);
                var foot = Feet[f];
                if (part == null || rig.IsSevered(FootParts[f]))
                {
                    foot.Touching = foot.HeelDown = foot.ToeDown = false;
                    foot.State = FootState.High;
                    foot.Lowest = float.NaN;
                    Feet[f] = foot;
                    heelWasDown[f] = toeWasDown[f] = false;
                    continue;
                }

                Sole(part, size, standing, out foot.A, out foot.B, out foot.C, out foot.D);
                foot.Heel = 0.5f * (foot.A + foot.B);
                foot.Toe = 0.5f * (foot.C + foot.D);
                foot.HeelDown = foot.Heel.y - groundY < touch;
                foot.ToeDown = foot.Toe.y - groundY < touch;
                foot.Touching = foot.HeelDown || foot.ToeDown;
                foot.Lowest = Mathf.Min(foot.Heel.y, foot.Toe.y) - groundY;
                // (The same end of the sole, down this step and the one before: the toe's, else the heel's.)
                bool on = false;
                float moved = 0f;
                if (timed && foot.ToeDown && toeWasDown[f])
                {
                    moved = Flat(foot.Toe - toeWas[f]).magnitude;
                    on = true;
                }
                else if (timed && foot.HeelDown && heelWasDown[f])
                {
                    moved = Flat(foot.Heel - heelWas[f]).magnitude;
                    on = true;
                }

                foot.Speed = on ? moved / dt : 0f;
                if (on)
                {
                    bool slides = foot.Speed > SlideFrom;
                    foot.State = slides ? FootState.Sliding : FootState.Planted;
                    foot.Slid = slides ? foot.Slid + moved : 0f;
                    foot.UpFor = 0f;
                }
                else if (foot.Touching)
                {
                    foot.State = FootState.Landing;
                    foot.Slid = 0f;
                    foot.UpFor = 0f;
                }
                else
                {
                    foot.State = foot.Lowest < LowUnder * size ? FootState.Low : FootState.High;
                    foot.Slid = 0f;
                    foot.UpFor += dt;
                }

                heelWas[f] = foot.Heel;
                toeWas[f] = foot.Toe;
                heelWasDown[f] = foot.HeelDown;
                toeWasDown[f] = foot.ToeDown;
                Feet[f] = foot;
                if (foot.HeelDown)
                {
                    points.Add(new Vector2(foot.A.x, foot.A.z));
                    points.Add(new Vector2(foot.B.x, foot.B.z));
                }

                if (foot.ToeDown)
                {
                    points.Add(new Vector2(foot.C.x, foot.C.z));
                    points.Add(new Vector2(foot.D.x, foot.D.z));
                }
            }

            // The support, and the weight and the place a foot is needed against it.
            Hull(points, Support);
            float tips = Mathf.Sqrt(Mathf.Max(0.3f, Weight.y - groundY) / 9.81f);
            Needed = new Vector3(Weight.x + WeightSpeed.x * tips, groundY, Weight.z + WeightSpeed.z * tips);
            WeightOut = Outside(Support, new Vector2(Weight.x, Weight.z));
            NeededOut = Outside(Support, new Vector2(Needed.x, Needed.z));
            Known = true;
        }

        private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);

        /// <summary>A body is up (its ankles' height is learnt, as the log's reading has it) while its hips are over this (metres at a Medium's size).</summary>
        public const float UpOver = 0.62f;

        private void LearnStanding(UnitRig rig, float groundY, float size, float restAnkle)
        {
            if (standing < 0f)
            {
                // (Until it is learnt: the rest height, less the 2 mm a Medium's guard stands under it.)
                standing = restAnkle - 0.002f * size;
            }

            var hips = rig.Part(PartId.Pelvis);
            if (hips == null || hips.transform.position.y - groundY <= UpOver * size)
            {
                return;
            }

            for (int f = 0; f < 2; f++)
            {
                var part = rig.Part(FootParts[f]);
                if (part == null || rig.IsSevered(FootParts[f]))
                {
                    continue;
                }

                int mm = Mathf.Clamp(Mathf.RoundToInt(1000f * (part.transform.position.y - groundY)), 0, ankleSeen.Length - 1);
                ankleSeen[mm]++;
                anklesSeen++;
            }

            // (From two seconds on, and then looked up again each second.)
            if (anklesSeen >= 240 && anklesSeen % 120 < 2)
            {
                int under = anklesSeen / 20, count = 0;
                for (int mm = 0; mm < ankleSeen.Length; mm++)
                {
                    count += ankleSeen[mm];
                    if (count > under)
                    {
                        standing = 0.001f * mm;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// The four corners of a foot's sole: under the ankle by its standing height less a centimetre and a half, from 2 cm
        /// behind the ankle to 10 cm before it, 10 cm wide (a Medium's): the sole <c>ground.py</c> reads a log with, so
        /// the two give the same numbers. (The box a foot is built of is longer and lies lower: by its corners a foot
        /// carried a centimetre over the ground counted as on it and sliding.)
        /// </summary>
        private static void Sole(BodyPart part, float size, float standing, out Vector3 a, out Vector3 b, out Vector3 c, out Vector3 d)
        {
            var turn = part.transform.rotation;
            Vector3 ankle = part.transform.position;
            float down = -(standing - 0.015f * size), wide = 0.05f * size;
            a = ankle + turn * new Vector3(-wide, down, -0.02f * size);
            b = ankle + turn * new Vector3(wide, down, -0.02f * size);
            c = ankle + turn * new Vector3(wide, down, 0.10f * size);
            d = ankle + turn * new Vector3(-wide, down, 0.10f * size);
        }

        /// <summary>The outline round some points on the ground, counter-clockwise (fewer than three: the points themselves).</summary>
        private static void Hull(List<Vector2> pts, List<Vector2> hull)
        {
            hull.Clear();
            if (pts.Count == 0)
            {
                return;
            }

            pts.Sort((p, q) => p.x != q.x ? p.x.CompareTo(q.x) : p.y.CompareTo(q.y));
            if (pts.Count < 3)
            {
                hull.AddRange(pts);
                return;
            }

            // (The lower chain, then the upper.)
            for (int pass = 0; pass < 2; pass++)
            {
                int start = hull.Count;
                for (int k = 0; k < pts.Count; k++)
                {
                    var p = pass == 0 ? pts[k] : pts[pts.Count - 1 - k];
                    while (hull.Count - start >= 2 && Cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0f)
                    {
                        hull.RemoveAt(hull.Count - 1);
                    }

                    hull.Add(p);
                }

                hull.RemoveAt(hull.Count - 1);
            }
        }

        private static float Cross(Vector2 o, Vector2 a, Vector2 b) => (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);

        /// <summary>How far a point is outside an outline (metres; 0: inside it; no outline: not a number; a line or a point: how far from it).</summary>
        public static float Outside(List<Vector2> hull, Vector2 p)
        {
            int n = hull.Count;
            if (n == 0)
            {
                return float.NaN;
            }

            bool inside = n >= 3;
            float best = float.MaxValue;
            for (int i = 0; i < n; i++)
            {
                Vector2 a = hull[i], b = hull[(i + 1) % n], e = b - a, to = p - a;
                if (n >= 3 && e.x * to.y - e.y * to.x < 0f)
                {
                    inside = false;
                }

                float len = e.sqrMagnitude;
                float along = len > 0f ? Mathf.Clamp01(Vector2.Dot(to, e) / len) : 0f;
                best = Mathf.Min(best, (to - e * along).magnitude);
            }

            return inside ? 0f : best;
        }
    }
}
