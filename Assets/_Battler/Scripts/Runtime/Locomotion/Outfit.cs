using System.Collections.Generic;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// A long-sleeved shirt and trousers to the ankles that never go into the drawn body (the plan "Clothes that never go
    /// into the body", steps 1 to 3, 22 November; his words: "Cloth that does not intersect with the body ever").
    ///
    /// The garments are cut by code from the baked drawn body (<see cref="BodyFields"/>): tubes round each part's axis,
    /// each ring on rays out to the drawn surface and a set gap beyond it. Every cloth point gets a home on a body part, in
    /// that part's own space; over a joint the home is on the joint's ball, at a fixed distance from its centre, turned
    /// partway between the two parts. Each picture the homes are put where the drawn parts are (so the cloth is carried
    /// with the body exactly, never late), and then the last word: every point, every thread's middle and every little
    /// face's centre is checked against every covered part's baked field, all together, and pushed straight out if it is
    /// closer than its margin. Then two checks count anything still inside: the same fields every picture, and (headless,
    /// every so often) an exact test against the mesh triangles that does not use the fields.
    ///
    /// No wobble yet (the plan's step 4): the clothes move exactly with him.
    /// </summary>
    public sealed class Outfit
    {
        // ---- Settings (Combat Feel pushes them) ---------------------------------------------------------------------

        // (Looser since his look at 11:42, "so tight that I cant see any cloth like deformation": 1.2 cm, 1.8 cm and 1
        // were the first cut.)
        public const float DefaultGap = 0.02f, DefaultShirtGap = 0.03f, DefaultLoose = 1.6f;
        public const float DefaultWobble = 0.05f, DefaultWeight = 1f, DefaultHold = 0.1f;

        /// <summary>The wobble (the plan's step 4): on, the cloth swings and hangs within a fence round its home; off (old): carried exactly.</summary>
        public static bool WobbleOn = true;

        /// <summary>How far a point may stray from its home at most (metres, at the cuffs and the trouser bottoms; less toward the shoulders and hips, none at the collar, waist and sash).</summary>
        public static float Wobble = DefaultWobble;

        /// <summary>The cloth's weight (1: about cotton): how much it hangs.</summary>
        public static float Weight = DefaultWeight;

        /// <summary>How much of the way home a point is drawn each picture (more: it keeps its shape; less: it swings and hangs freely).</summary>
        public static float Hold = DefaultHold;

        /// <summary>How much of its speed a point loses each picture.</summary>
        public static float Damp = 0.05f;

        /// <summary>Passes over the threads each picture.</summary>
        public static int ThreadPasses = 4;

        /// <summary>How far the trousers and sleeves are cut off the drawn skin (metres, at a Medium's size).</summary>
        public static float Gap = DefaultGap;

        /// <summary>How far the shirt's body is cut off the drawn skin (it lies over the trousers' waist).</summary>
        public static float ShirtGap = DefaultShirtGap;

        /// <summary>The dial on the slack at the cuffs and the bottoms of the trousers (1: as cut; 0: tight everywhere).</summary>
        public static float Loose = DefaultLoose;

        /// <summary>On: the last word pushes anything inside back out. Off: the homes alone (for comparing).</summary>
        public static bool LastWordOn = true;

        /// <summary>On (Unity): any cloth point inside the drawn body is drawn as a red cross through everything.</summary>
        /// <summary>How far down the trousers' waist goes, in the hips' space (m; the hip joints are near -0.07).</summary>
        public static float WaistBottom = -0.12f;
        /// <summary>How much a trouser leg's top ring follows the thigh (0: stays with the hips, 1: goes with the thigh).</summary>
        public static float HipTopFollows = 0.35f;
        /// <summary>How far down the thigh the trouser leg goes from following the hips to following the thigh (m).</summary>
        public static float HipBlendEnd = 0.14f;
        public static bool XRay;

        /// <summary>Margins the last word keeps (metres): points, thread middles and face centres off the drawn surface.</summary>
        public const float PointMargin = 0.006f, MiddleMargin = 0.003f;

        public static readonly Color[] Shirt = { new Color(0.55f, 0.6f, 0.72f), new Color(0.72f, 0.56f, 0.5f) };
        public static readonly Color[] Trousers = { new Color(0.14f, 0.17f, 0.27f), new Color(0.3f, 0.12f, 0.1f) };
        public static Color Sash = new Color(0.08f, 0.07f, 0.06f);

        // ---- Measured, everywhere -------------------------------------------------------------------------------------

        public static long Pictures, PointsChecked, PointsIn, MiddlesChecked, MiddlesIn, CentresChecked, CentresIn, Pushed, StillIn;
        public static long ExactChecked, ExactIn, ExactPictures, ExactFolded;
        public static float Deepest, CostMs;
        public static readonly long[] InByPart = new long[PartInfo.Count];
        public static string LastIn = "";

        /// <summary>Every check so far, told.</summary>
        public static string Counted()
        {
            if (Pictures == 0)
            {
                return "none worn";
            }

            var worst = new System.Text.StringBuilder();
            for (int p = 0; p < InByPart.Length; p++)
            {
                if (InByPart[p] > 0)
                {
                    worst.Append(worst.Length > 0 ? ", " : "").Append((PartId)p).Append(' ').Append(InByPart[p]);
                }
            }

            return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "{0} pictures; inside the drawn parts each piece covers (the fields): points {1} of {2}, thread middles {3} of {4}, face centres {5} of {6}, the deepest {7:0.0} mm{8}; the exact check (mesh triangles) {9} inside of {10} in {11} pictures (and {18} in a fold); the last word pushed {12} times, left {13} it could not free; points in a part another piece covers (the body pressed into itself, that part's own cloth over it) {16}; caught in a fold of the body (an elbow or knee bent shut, no room for cloth) {17}; {14:0.00} ms a fighter a picture{15}",
                Pictures, PointsIn, PointsChecked, MiddlesIn, MiddlesChecked, CentresIn, CentresChecked, 1000f * Deepest,
                worst.Length > 0 ? " (by part: " + worst + ")" : "", ExactIn, ExactChecked, ExactPictures, Pushed, StillIn,
                CostMs / Pictures, LastIn.Length > 0 ? "; the last inside: " + LastIn : "", Crossed, Folded, ExactFolded);
        }

        /// <summary>Every measure set back to nothing (a new test).</summary>
        public static void Forget()
        {
            Pictures = PointsChecked = PointsIn = MiddlesChecked = MiddlesIn = CentresChecked = CentresIn = Pushed = StillIn = Crossed = Folded = 0;
            ExactChecked = ExactIn = ExactPictures = ExactFolded = 0;
            Deepest = CostMs = 0f;
            System.Array.Clear(InByPart, 0, InByPart.Length);
            LastIn = "";
        }

        // ---- The garments ---------------------------------------------------------------------------------------------

        private sealed class Tube
        {
            public string Name;
            public int Rings, Cols, Start;
            public PartId[] Own, Others;
            public bool Trousers, IsSash;
            public GameObject Drawn;
            public Mesh Mesh;
            public readonly List<Vector3> Verts = new List<Vector3>();
            public readonly List<int> Tris = new List<int>();
            public bool TrisSet;
        }

        private readonly List<Tube> tubes = new List<Tube>();
        private Vector3[] x = new Vector3[0];
        // (The wobble's: each point's home this picture, where it was the picture before, its sway, and its threads'
        // cut lengths to the next point round (U) and along (V), at scale 1.)
        private Vector3[] home = new Vector3[0], prev = new Vector3[0];
        private float[] sway = new float[0], restU = new float[0], restV = new float[0];
        private readonly List<float> swayAll = new List<float>();
        private bool still = true;
        private OutfitSolve.Anchor[] anchors = new OutfitSolve.Anchor[0];
        private readonly UnitRig rig;
        private readonly ShockWave wave;
        private readonly BodyFields fields;
        private readonly float size;
        private readonly int side;
        private readonly Transform owner;
        private readonly Vector3[] fp = new Vector3[PartInfo.Count];
        private readonly Quaternion[] fr = new Quaternion[PartInfo.Count];
        private readonly Quaternion[] fi = new Quaternion[PartInfo.Count];
        private readonly Vector3[] restPos = new Vector3[PartInfo.Count];
        private readonly Quaternion[] restRot = new Quaternion[PartInfo.Count];

        /// <summary>The parts the clothes cover: the last word keeps the cloth out of all of them.</summary>
        public static readonly PartId[] Covered =
        {
            PartId.Pelvis, PartId.Spine, PartId.Chest, PartId.UpperArmL, PartId.ForearmL, PartId.UpperArmR, PartId.ForearmR,
            PartId.ThighL, PartId.ShinL, PartId.ThighR, PartId.ShinR,
        };

        /// <summary>Cloth points in all.</summary>
        public int Points => x.Length;

        /// <summary>What the start-up check found (the baked body against what Unity loaded).</summary>
        public string Check { get; private set; } = "";

        private Outfit(UnitRig rig, ShockWave wave, BodyFields fields, float size, int side, Transform owner)
        {
            this.rig = rig;
            this.wave = wave;
            this.fields = fields;
            this.size = Mathf.Max(0.3f, size);
            this.side = side;
            this.owner = owner;
        }

        /// <summary>The outfit cut for this body, or null if no baked body of its size is there.</summary>
        public static Outfit Make(UnitRig rig, ShockWave wave, float size, int side, Transform owner)
        {
            var fields = BodyFields.For("Medium");
            if (fields == null || rig == null)
            {
                return null;
            }

            var made = new Outfit(rig, wave, fields, size, side, owner);
            made.Cut();
            made.Check = fields.CheckAgainst(rig, made.size, Covered);
            return made;
        }

        // ---- Cutting -------------------------------------------------------------------------------------------------

        /// <summary>The parts' places at rest, in the hips' space, at scale 1.</summary>
        private void Rest()
        {
            foreach (var id in PartInfo.All)
            {
                var part = rig.Part(id);
                Vector3 lp = part != null ? part.restLocalPosition / size : Vector3.zero;
                Quaternion lr = part != null ? part.restLocalRotation : Quaternion.identity;
                if (id == PartId.Pelvis || !PartInfo.HasParent(id))
                {
                    restPos[(int)id] = Vector3.zero;
                    restRot[(int)id] = Quaternion.identity;
                    continue;
                }

                int up = (int)PartInfo.Parent(id);
                restPos[(int)id] = restPos[up] + restRot[up] * lp;
                restRot[(int)id] = restRot[up] * lr;
            }
        }

        private float RestDistance(PartId p, Vector3 at)
        {
            var f = fields.Of(p);
            return f == null ? 1f : f.Distance(Quaternion.Inverse(restRot[(int)p]) * (at - restPos[(int)p]));
        }

        private float RestUnion(Vector3 at, PartId[] parts)
        {
            float d = 1f;
            foreach (var p in parts)
            {
                // (An upper arm in the shirt's body's cut: only its round shoulder ball counts (within 7 cm of the
                // joint), so the shirt goes over the tops of the shoulders, and stays there however the arm turns.)
                if ((p == PartId.UpperArmL || p == PartId.UpperArmR) && System.Array.IndexOf(parts, PartId.Chest) >= 0
                    && (at - restPos[(int)p]).sqrMagnitude > 0.07f * 0.07f)
                {
                    continue;
                }

                d = Mathf.Min(d, RestDistance(p, at));
            }

            return d;
        }

        /// <summary>Out along a ray from the axis until the drawn surface of the given parts is <paramref name="gap"/> behind (rest space).</summary>
        private float RayOut(Vector3 centre, Vector3 dir, PartId[] parts, float gap, bool midline = false)
        {
            float r = 0f;
            bool wasIn = false;
            for (int k = 0; k < 220; k++)
            {
                // (A trouser leg's root stops at the body's middle, a centimetre past it: the two legs meet there, over
                // the crotch, as a seam does.)
                if (midline && Mathf.Abs(dir.x) > 1e-4f && Mathf.Sign(centre.x + dir.x * r) != Mathf.Sign(centre.x) && Mathf.Abs(centre.x + dir.x * r) > 0.01f)
                {
                    return r;
                }

                float d = RestUnion(centre + dir * r, parts);
                if (d < 0f)
                {
                    wasIn = true;
                }

                if (d >= gap && (wasIn || r > 0.02f))
                {
                    return r;
                }

                r += Mathf.Max(0.001f, Mathf.Min(0.004f, Mathf.Abs(d - gap) * 0.5f));
            }

            return r;
        }

        private struct Station
        {
            public Vector3 C, Axis, U, V;
            public float Slack, Gap;
            public PartId[] CutBy;
            public byte Joint;
            public PartId A, B;
            public float W;
            public bool Midline;
            public float Sway;
        }

        private void Cut()
        {
            Rest();
            var all = new List<Vector3>();
            var anchorList = new List<OutfitSolve.Anchor>();
            tubes.Clear();
            swayAll.Clear();

            // (The shirt's body: up the trunk from a little below the waist to the base of the neck; finer over the
            // shoulders' tops, where the rings close in fast.)
            var trunk = new List<Station>();
            var ys = new List<float>();
            for (int i = 0; i <= 21; i++)
            {
                ys.Add(-0.04f + 0.54f * i / 21f);
            }

            for (int i = 1; i <= 6; i++)
            {
                ys.Add(0.50f + 0.06f * i / 6f);
            }

            PartId[] trunkParts = { PartId.Pelvis, PartId.Spine, PartId.Chest };
            PartId[] trunkAndShoulders = { PartId.Pelvis, PartId.Spine, PartId.Chest, PartId.UpperArmL, PartId.UpperArmR };
            foreach (float y in ys)
            {
                var s = new Station { C = new Vector3(0f, y, 0f), Axis = Vector3.up, U = Vector3.right, V = Vector3.forward, Gap = ShirtGap, CutBy = trunkAndShoulders };
                float spineY = restPos[(int)PartId.Spine].y, chestY = restPos[(int)PartId.Chest].y;
                if (y < spineY - 0.05f) { s.A = PartId.Pelvis; }
                else if (y < spineY + 0.05f) { s.Joint = 1; s.A = PartId.Pelvis; s.B = PartId.Spine; s.W = (y - (spineY - 0.05f)) / 0.10f; }
                else if (y < chestY - 0.05f) { s.A = PartId.Spine; }
                else if (y < chestY + 0.05f) { s.Joint = 1; s.A = PartId.Spine; s.B = PartId.Chest; s.W = (y - (chestY - 0.05f)) / 0.10f; }
                else { s.A = PartId.Chest; }

                // (The shirt sways a little: none under the sash and at the collar and shoulders, most just above the
                // sash, where it blouses.)
                s.Sway = y < 0.12f || y > 0.47f ? 0f : y < 0.2f ? 0.5f : Mathf.Lerp(0.5f, 0.15f, Mathf.Clamp01((y - 0.2f) / 0.2f));
                trunk.Add(s);
            }

            AddTube("shirt", trunk, 40, false, trunkParts, all, anchorList);
            // (The sleeves, from just out of the shoulder's ball to a little above the wrist.)
            AddLimb("left sleeve", PartId.Chest, PartId.UpperArmL, PartId.ForearmL, PartId.HandL, 0.0f, 0.012f, 20, Gap, 0.015f, false, all, anchorList);
            AddLimb("right sleeve", PartId.Chest, PartId.UpperArmR, PartId.ForearmR, PartId.HandR, 0.0f, 0.012f, 20, Gap, 0.015f, false, all, anchorList);
            // (The trousers' waist: round the hips from a little below the hip joints to above the shirt's hem.)
            var waist = new List<Station>();
            for (int i = 0; i <= 9; i++)
            {
                float y = WaistBottom + (0.13f - WaistBottom) * i / 9f;
                waist.Add(new Station { C = new Vector3(0f, y, 0f), Axis = Vector3.up, U = Vector3.right, V = Vector3.forward, Gap = Gap, CutBy = new[] { PartId.Pelvis }, A = PartId.Pelvis });
            }

            AddTube("trousers' waist", waist, 40, true, new[] { PartId.Pelvis, PartId.Spine }, all, anchorList);
            AddLimb("left trouser leg", PartId.Pelvis, PartId.ThighL, PartId.ShinL, PartId.FootL, -0.06f, 0.015f, 28, Gap, 0.035f, true, all, anchorList);
            AddLimb("right trouser leg", PartId.Pelvis, PartId.ThighR, PartId.ShinR, PartId.FootR, -0.06f, 0.015f, 28, Gap, 0.035f, true, all, anchorList);
            // (The sash: a band round the shirt at the waist, a little outside it.)
            var sash = new List<Station>();
            for (int i = 0; i <= 2; i++)
            {
                float y = 0.04f + 0.03f * i;
                sash.Add(new Station { C = new Vector3(0f, y, 0f), Axis = Vector3.up, U = Vector3.right, V = Vector3.forward, Gap = ShirtGap + 0.007f, CutBy = trunkParts, A = PartId.Pelvis });
            }

            AddTube("sash", sash, 40, false, new[] { PartId.Pelvis, PartId.Spine }, all, anchorList);
            tubes[tubes.Count - 1].IsSash = true;
            x = all.ToArray();
            sway = swayAll.ToArray();
            home = new Vector3[x.Length];
            prev = new Vector3[x.Length];
            restU = new float[x.Length];
            restV = new float[x.Length];
            foreach (var tb in tubes)
            {
                for (int r = 0; r < tb.Rings; r++)
                {
                    for (int c = 0; c < tb.Cols; c++)
                    {
                        int a = tb.Start + r * tb.Cols + c;
                        restU[a] = (x[tb.Start + r * tb.Cols + (c + 1) % tb.Cols] - x[a]).magnitude;
                        restV[a] = r + 1 < tb.Rings ? (x[a + tb.Cols] - x[a]).magnitude : 0f;
                    }
                }
            }

            still = true;
            anchors = anchorList.ToArray();
            tubeOf = new int[x.Length];
            for (int k = 0; k < tubes.Count; k++)
            {
                for (int i = 0; i < tubes[k].Rings * tubes[k].Cols; i++)
                {
                    tubeOf[tubes[k].Start + i] = k;
                }
            }

            Prepare();
        }

        /// <summary>
        /// A limb's tube: from <paramref name="from"/> metres along the upper part's axis past its joint to
        /// <paramref name="endShort"/> metres before the end part's joint; the root over the joint with the trunk, the
        /// middle joint (elbow, knee) on its ball; slack growing toward the end.
        /// </summary>
        private void AddLimb(string name, PartId trunk, PartId upper, PartId lower, PartId end, float from, float endShort, int cols, float gap, float slack, bool trousers, List<Vector3> all, List<OutfitSolve.Anchor> anchorList)
        {
            Vector3 s0 = restPos[(int)upper], e0 = restPos[(int)lower], w0 = restPos[(int)end];
            float l1 = (e0 - s0).magnitude, l2 = (w0 - e0).magnitude, total = l1 + l2 - endShort;
            var list = new List<Station>();
            // (Stations every 2.5 cm, every 1.25 cm over the joints, where the cloth turns.)
            var ts = new List<float>();
            for (float t0 = from; t0 < total; )
            {
                ts.Add(t0);
                bool atJoint = t0 < (trousers ? HipBlendEnd : 0.08f) || Mathf.Abs(t0 - l1) < 0.06f;
                t0 += atJoint ? 0.0125f : 0.025f;
            }

            ts.Add(total);
            int n = ts.Count - 1;
            Vector3 u = Vector3.zero;
            PartId[] cutBy = { upper, lower };
            PartId[] rootCut = { upper, lower, trunk };
            for (int i = 0; i <= n; i++)
            {
                float t = ts[i];
                Vector3 c = t <= l1 ? s0 + (e0 - s0) * (t / l1) : e0 + (w0 - e0) * ((t - l1) / l2);
                Vector3 a1 = (e0 - s0) / l1, a2 = (w0 - e0) / l2;
                float k = Mathf.Clamp01((t - (l1 - 0.04f)) / 0.08f);
                Vector3 axis = Vector3.Slerp(a1, a2, k).normalized;
                // (The ring's own across and front, carried on from the last so the tube does not twist.)
                if (i == 0)
                {
                    u = Vector3.Cross(axis, Vector3.forward);
                    if (u.sqrMagnitude < 1e-4f)
                    {
                        u = Vector3.Cross(axis, Vector3.right);
                    }
                }
                else
                {
                    u -= axis * Vector3.Dot(u, axis);
                }

                u.Normalize();
                // (The root's rings sit a little further out, over the shirt's shoulder or the trousers' waist.)
                float rootGap = t < 0.06f ? 0.006f * (1f - Mathf.Clamp01(t / 0.06f)) + (t < 0f ? 0.004f : 0f) : 0f;
                bool seam = trousers && t < 0.12f;
                var st = new Station { C = c, Axis = axis, U = u, V = Vector3.Cross(axis, u).normalized, Gap = gap + rootGap, CutBy = seam ? rootCut : cutBy, Midline = seam };
                st.Slack = t > l1 * (trousers ? 1f : 0.5f) ? slack * Mathf.Clamp01((t - l1 * (trousers ? 1f : 0.5f)) / (total - l1 * (trousers ? 1f : 0.5f))) : 0f;
                // (Over the hip the trouser leg's top stays mostly with the hips and is stretched over the ball as the
                // thigh swings, so no gap opens between it and the waist; the sleeve's top follows the arm more.)
                float rootEnd = trousers ? HipBlendEnd : 0.08f;
                if (t < rootEnd)
                {
                    st.Joint = 1; st.A = trunk; st.B = upper; st.W = Mathf.Lerp(trousers ? HipTopFollows : 0.9f, 1f, Mathf.Clamp01((t - from) / (rootEnd - from)));
                }
                else if (t < l1 - 0.045f)
                {
                    st.A = upper;
                }
                else if (t < l1 + 0.045f)
                {
                    st.Joint = 1; st.A = upper; st.B = lower; st.W = (t - (l1 - 0.045f)) / 0.09f;
                }
                else
                {
                    st.A = lower;
                }

                // (A limb's cloth sways more toward its end: none over the joint with the trunk, all of it at the cuff or
                // the trouser bottom.)
                float swayFrom = trousers ? 0.10f : 0.06f;
                st.Sway = Mathf.Pow(Mathf.Clamp01((t - swayFrom) / (total - swayFrom)), trousers ? 1.3f : 1.1f);
                list.Add(st);
            }

            AddTube(name, list, cols, trousers, trousers ? new[] { upper, lower, trunk } : new[] { upper, lower }, all, anchorList);
        }

        private void AddTube(string name, List<Station> stations, int cols, bool trousers, PartId[] own, List<Vector3> all, List<OutfitSolve.Anchor> anchorList)
        {
            var others = new List<PartId>();
            foreach (var id in Covered)
            {
                if (System.Array.IndexOf(own, id) < 0)
                {
                    others.Add(id);
                }
            }

            var tube = new Tube { Name = name, Rings = stations.Count, Cols = cols, Start = all.Count, Trousers = trousers, Own = own, Others = others.ToArray() };
            foreach (var st in stations)
            {
                for (int c = 0; c < cols; c++)
                {
                    float a = 2f * Mathf.PI * c / cols;
                    Vector3 dir = (Mathf.Cos(a) * st.U + Mathf.Sin(a) * st.V).normalized;
                    float r = RayOut(st.C, dir, st.CutBy, st.Gap, st.Midline) + st.Slack * Loose;
                    Vector3 p = st.C + dir * r;
                    all.Add(p);
                    swayAll.Add(st.Sway);
                    var an = new OutfitSolve.Anchor { Joint = st.Joint, A = (int)st.A, B = (int)st.B, W = st.W };
                    if (st.Joint == 0)
                    {
                        an.H = Quaternion.Inverse(restRot[(int)st.A]) * (p - restPos[(int)st.A]);
                    }
                    else
                    {
                        Vector3 d = p - restPos[(int)st.B];
                        an.R = d.magnitude;
                        Vector3 dn = an.R > 1e-6f ? d / an.R : Vector3.up;
                        an.DA = Quaternion.Inverse(restRot[(int)st.A]) * dn;
                        an.DB = Quaternion.Inverse(restRot[(int)st.B]) * dn;
                    }

                    anchorList.Add(an);
                }
            }

            // (Rings smoothed round: a ring point never closer in than its neighbours' average less a little, so one
            // ray that slipped between two bumps does not make a dent; only outward, never in.)
            tubes.Add(tube);
        }

        // ---- Each picture ---------------------------------------------------------------------------------------------

        /// <summary>The body's parts as drawn now (the shock wave's turn included).</summary>
        private void Frames()
        {
            foreach (var id in PartInfo.All)
            {
                var part = rig.Part(id);
                if (part == null)
                {
                    continue;
                }

                var t = part.transform;
                fp[(int)id] = wave != null ? wave.DrawnPoint(id, t.position) : t.position;
                fr[(int)id] = wave != null ? wave.DrawnTurn(id) * t.rotation : t.rotation;
                fi[(int)id] = Quaternion.Inverse(fr[(int)id]);
            }
        }

        private readonly Vector3[] bc = new Vector3[PartInfo.Count];
        private readonly float[] br = new float[PartInfo.Count];
        private int[] tubeOf = new int[0];

        // (The solver's arrays: the body's fields (once), the pieces and homes (once), and each picture's frames, points,
        // scratch and measures.)
        private short[] grid = new short[0];
        private readonly OutfitSolve.PartField[] partFields = new OutfitSolve.PartField[OutfitSolve.Parts];
        private readonly OutfitSolve.Frame[] frames = new OutfitSolve.Frame[OutfitSolve.Parts];
        private OutfitSolve.Piece[] pieces = new OutfitSolve.Piece[0];
        private OutfitSolve.Stats[] stats = new OutfitSolve.Stats[0];
        private float[] dNear = new float[0];
        private byte[] moved = new byte[0], insideFlags = new byte[0];
#if !HEADLESS_STUB
        private readonly OutfitRun job = new OutfitRun();
#endif

        /// <summary>On (Unity): each picture's sums run on other cores, a piece to a core (Burst), finished before the picture is drawn. Off: here.</summary>
        public static bool OnOtherCores = true;

        /// <summary>Margins of the last word against parts of other pieces (the body pressed into itself): best kept, not promised.</summary>
        public const float OtherMargin = 0.003f;

        /// <summary>How many rounds of middles and corners the last word may take.</summary>
        public static int Rounds = 6;
        /// <summary>The fast measure is taken every this many pictures (1: every one; headless tests use 1).</summary>
        public static int MeasureEvery = 4;
        private int runs;
        private bool jobFailed;
        /// <summary>Passes that smooth the trousers after the last word's first push (each followed by another push out): the
        /// crotch, where the two legs and the waist meet, crumpled. Not the shirt: smoothed, its sleeves' tops drew in off
        /// the shoulders (measured on the rack, 22 November).</summary>
        public static int Smooth = 2;

        /// <summary>Cloth inside a covered part that another piece covers (where the body presses into itself), counted apart.</summary>
        public static long Crossed;

        /// <summary>Cloth caught in a fold of the body (counted apart; see InAFold).</summary>
        public static long Folded;

        /// <summary>The solver's arrays made, once the pieces are cut.</summary>
        private void Prepare()
        {
            int total = 0;
            foreach (var id in PartInfo.All)
            {
                var f = fields.Of(id);
                if (f != null)
                {
                    total += f.Grid.Length;
                }
            }

            grid = new short[total];
            int at = 0;
            foreach (var id in PartInfo.All)
            {
                var f = fields.Of(id);
                if (f == null)
                {
                    continue;
                }

                System.Array.Copy(f.Grid, 0, grid, at, f.Grid.Length);
                partFields[(int)id] = new OutfitSolve.PartField { Offset = at, Nx = f.Nx, Ny = f.Ny, Nz = f.Nz, Has = 1, Cell = f.Cell, Bound = f.Bound, Min = f.Min, Centre = f.Centre };
                at += f.Grid.Length;
            }

            pieces = new OutfitSolve.Piece[tubes.Count];
            for (int k = 0; k < tubes.Count; k++)
            {
                var t = tubes[k];
                int own = 0, others = 0;
                foreach (var id in t.Own)
                {
                    own |= 1 << (int)id;
                }

                foreach (var id in t.Others)
                {
                    others |= 1 << (int)id;
                }

                pieces[k] = new OutfitSolve.Piece { Start = t.Start, Rings = t.Rings, Cols = t.Cols, Own = own, Others = others, Smooths = t.Trousers ? 1 : 0 };
            }

            stats = new OutfitSolve.Stats[tubes.Count];
            dNear = new float[x.Length];
            moved = new byte[x.Length];
            insideFlags = new byte[x.Length];
        }

        /// <summary>Each part as drawn now, for the solver: place, turn, turn back, bounding sphere.</summary>
        private void FillFrames()
        {
            foreach (var id in PartInfo.All)
            {
                int p = (int)id;
                var f = fields.Of(id);
                bool there = f != null && System.Array.IndexOf(Covered, id) >= 0 && !rig.IsSevered(id);
                bc[p] = f != null ? fp[p] + fr[p] * (f.Centre * size) : fp[p];
                br[p] = there ? f.Bound * size : -1f;
                frames[p] = new OutfitSolve.Frame { P = fp[p], R = fr[p], I = fi[p], BC = bc[p], BR = br[p] };
            }
        }

        /// <summary>The sums here, on this thread (headless, or with "on other cores" off).</summary>
        public unsafe void RunHere(bool measure)
        {
            fixed (short* g = grid)
            fixed (OutfitSolve.PartField* pf = partFields)
            fixed (OutfitSolve.Frame* fr0 = frames)
            fixed (OutfitSolve.Anchor* an = anchors)
            fixed (OutfitSolve.Piece* pc = pieces)
            fixed (Vector3* xs = x)
            fixed (Vector3* hm = home)
            fixed (Vector3* pv = prev)
            fixed (float* sw = sway)
            fixed (float* ru = restU)
            fixed (float* rv = restV)
            fixed (float* dn = dNear)
            fixed (byte* mv = moved)
            fixed (byte* ins = insideFlags)
            fixed (OutfitSolve.Stats* st = stats)
            {
                var d = Settings(measure);
                d.Grid = g; d.Fields = pf; d.Frames = fr0; d.Anchors = an; d.Pieces = pc; d.X = xs; d.Home = hm; d.Prev = pv; d.Sway = sw; d.RestU = ru; d.RestV = rv; d.DNear = dn; d.Moved = mv; d.Inside = ins; d.Stats = st;
                for (int t = 0; t < pieces.Length; t++)
                {
                    OutfitSolve.Piece1(ref d, t);
                }
            }
        }

        /// <summary>The settings of a picture for the solver (the pointers are filled in by whoever runs it).</summary>
        public OutfitSolve.Data Settings(bool measure) => new OutfitSolve.Data
        {
            Size = size, PointMargin = PointMargin, MiddleMargin = MiddleMargin, OtherMargin = OtherMargin,
            LastWord = LastWordOn ? 1 : 0, Rounds = Mathf.Max(0, Rounds), Measure = measure ? 1 : 0, Smooth = Mathf.Max(0, Smooth),
            WobbleOn = WobbleOn ? 1 : 0, Iters = Mathf.Max(0, ThreadPasses), Reset = still ? 1 : 0, Dt = dt,
            Wobble = Mathf.Max(0f, Wobble), Weight = Mathf.Max(0f, Weight), Hold = Mathf.Clamp01(Hold), Damp = Mathf.Clamp01(Damp),
        };

        private float dt = 1f / 60f;

        // (For the job: the arrays as they are.)
        public short[] GridArray => grid;
        public OutfitSolve.PartField[] PartFieldArray => partFields;
        public OutfitSolve.Frame[] FrameArray => frames;
        public OutfitSolve.Anchor[] AnchorArray => anchors;
        public OutfitSolve.Piece[] PieceArray => pieces;
        public Vector3[] PointArray => x;
        public Vector3[] HomeArray => home;
        public Vector3[] PrevArray => prev;
        public float[] SwayArray => sway;
        public float[] RestUArray => restU;
        public float[] RestVArray => restV;
        public OutfitSolve.Stats[] StatsArray => stats;
        public byte[] InsideArray => insideFlags;

        /// <summary>
        /// The picture: every point to its home on the body as drawn now; then the last word; then the checks; then the
        /// drawing. In Unity it is called as the picture is about to be drawn (LateUpdate), so it sees the body exactly as
        /// drawn; headless after each step.
        /// </summary>
        public void Place(float dt)
        {
            if (x.Length == 0)
            {
                return;
            }

            var clock = System.Diagnostics.Stopwatch.StartNew();
            this.dt = dt > 0f ? dt : 1f / 60f;
            Frames();
            Run(clock);
            still = false;
        }

        /// <summary>
        /// The same picture on a body posed by hand (the torture rack): each part's place and turn given, nothing of the
        /// game's body read or moved.
        /// </summary>
        public void PlaceWith(Vector3[] pos, Quaternion[] rot)
        {
            if (x.Length == 0)
            {
                return;
            }

            var clock = System.Diagnostics.Stopwatch.StartNew();
            for (int p = 0; p < PartInfo.Count; p++)
            {
                fp[p] = pos[p];
                fr[p] = rot[p];
                fi[p] = Quaternion.Inverse(rot[p]);
            }

            still = true; // (a pose by hand, unrelated to the last: no wobble carried over)
            Run(clock);
            still = true;
        }

        /// <summary>
        /// A body posed at random within each joint's limits (the torture rack): the parts' places and turns, the hips at
        /// <paramref name="at"/>. Every joint turned by a random pitch, twist and swing inside its limits.
        /// </summary>
        public void RandomPose(System.Random random, Vector3 at, Vector3[] pos, Quaternion[] rot)
        {
            foreach (var id in PartInfo.All)
            {
                var part = rig.Part(id);
                var lim = PhysicsBattler.Muscles.JointLimitTable.For(id);
                float pitch = Mathf.Lerp(lim.pitchMin, lim.pitchMax, (float)random.NextDouble());
                float twist = lim.twist * (2f * (float)random.NextDouble() - 1f);
                float swing = lim.swing * (2f * (float)random.NextDouble() - 1f);
                Quaternion turn = Quaternion.AngleAxis(pitch, Vector3.right) * Quaternion.AngleAxis(swing, Vector3.forward) * Quaternion.AngleAxis(twist, Vector3.up);
                Quaternion lr = (part != null ? part.restLocalRotation : Quaternion.identity) * turn;
                if (id == PartId.Pelvis || !PartInfo.HasParent(id))
                {
                    pos[(int)id] = at;
                    rot[(int)id] = Quaternion.Euler(0f, 360f * (float)random.NextDouble(), 0f);
                    continue;
                }

                int up = (int)PartInfo.Parent(id);
                pos[(int)id] = pos[up] + rot[up] * (part != null ? part.restLocalPosition : Vector3.zero);
                rot[(int)id] = rot[up] * lr;
            }
        }

        private void Run(System.Diagnostics.Stopwatch clock)
        {
            FillFrames();
            bool threaded = false;
            // (The measure costs about as much again as the cloth itself: in a game it is taken every few pictures.)
            bool measure = runs++ % Mathf.Max(1, MeasureEvery) == 0;
#if !HEADLESS_STUB
            threaded = OnOtherCores && !jobFailed;
            if (threaded)
            {
                // (If the job cannot run, say so once and work it out here, so the clothes are still drawn.)
                try
                {
                    job.Run(this, measure);
                }
                catch (System.Exception e)
                {
                    jobFailed = true;
                    threaded = false;
                    Debug.LogError("[Clothes] the job on other cores failed; worked out on the main thread from now on: " + e);
                }
            }
#endif
            if (!threaded)
            {
                RunHere(measure);
            }

            // (What the pieces measured, added up. Between measures the x-ray keeps the last measured points.)
            if (measure)
            {
                inside.Clear();
                for (int i = 0; i < insideFlags.Length; i++)
                {
                    if (insideFlags[i] != 0)
                    {
                        inside.Add(i);
                    }
                }
            }

            for (int k = 0; k < stats.Length; k++)
            {
                var st = stats[k];
                PointsChecked += st.PointsChecked; PointsIn += st.PointsIn; MiddlesChecked += st.MiddlesChecked; MiddlesIn += st.MiddlesIn;
                CentresChecked += st.CentresChecked; CentresIn += st.CentresIn; Pushed += st.Pushed; StillIn += st.StillIn; Crossed += st.Crossed; Folded += st.Folded;
                Deepest = Mathf.Max(Deepest, st.Deepest);
                if (st.LastInPart >= 0)
                {
                    InByPart[st.LastInPart] += st.PointsIn + st.MiddlesIn + st.CentresIn;
                    LastIn = tubes[k].Name + (st.LastInKind == 2 ? " (a middle or face)" : "") + " in the " + (PartId)st.LastInPart;
                }
            }

            ExactCheck();
            RanOnOtherCores = threaded;
            LastMs = (float)clock.Elapsed.TotalMilliseconds;
            CostMs += (float)clock.Elapsed.TotalMilliseconds;
            Pictures++;
            Draw();
        }

        /// <summary>The headless exact check runs every this many pictures (0: never). Unity: never (it is slow).</summary>
        public static int ExactEvery = 20;

        private int exactCount;
        private readonly List<int> inside = new List<int>();

        private void ExactCheck()
        {
            // (The exact check, headless only, every so often: points and face centres against the mesh triangles of
            // the piece's own parts.)
            bool headless = false;
#if HEADLESS_STUB
            headless = true;
#endif
            if (!headless || ExactEvery <= 0 || ++exactCount < ExactEvery)
            {
                return;
            }

            exactCount = 0;
            ExactPictures++;
            foreach (var t in tubes)
            {
                for (int r = 0; r < t.Rings; r++)
                {
                    for (int c = 0; c < t.Cols; c++)
                    {
                        int a = t.Start + r * t.Cols + c;
                        Exact(t, x[a]);
                        if (r + 1 < t.Rings)
                        {
                            int b = t.Start + r * t.Cols + (c + 1) % t.Cols;
                            Exact(t, 0.25f * (x[a] + x[b] + x[a + t.Cols] + x[b + t.Cols]));
                        }
                    }
                }
            }
        }

        private float DistanceTo(PartId id, Vector3 at)
        {
            int p = (int)id;
            if (br[p] < 0f)
            {
                return 10f;
            }

            return fields.Of(id).Distance(fi[p] * (at - fp[p]) / size) * size;
        }

        private bool InAFold(Tube t, Vector3 at, PartId inPart)
        {
            foreach (var id in t.Own)
            {
                if (id != inPart && DistanceTo(id, at) < 0.01f)
                {
                    return true;
                }
            }

            return false;
        }

        private void Exact(Tube t, Vector3 at)
        {
            ExactChecked++;
            foreach (var id in t.Own)
            {
                var f = fields.Of(id);
                if (f == null || br[(int)id] < 0f)
                {
                    continue;
                }

                Vector3 w = at - bc[(int)id];
                if (w.sqrMagnitude > br[(int)id] * br[(int)id])
                {
                    continue;
                }

                if (f.InsideExactly(fi[(int)id] * (at - fp[(int)id]) / size))
                {
                    if (InAFold(t, at, id))
                    {
                        ExactFolded++;
                    }
                    else
                    {
                        ExactIn++;
                    }

                    return;
                }
            }
        }

        // ---- Drawn ----------------------------------------------------------------------------------------------------

        private static readonly Material[] cloth = new Material[5];
        private GameObject xray;
        private Mesh xrayMesh;

        private Material Cloth(int which)
        {
            if (cloth[which] != null)
            {
                return cloth[which];
            }

            Material from = null;
            var pelvis = rig.Part(PartId.Pelvis);
            if (pelvis != null)
            {
                foreach (var r in pelvis.GetComponentsInChildren<Renderer>(true))
                {
                    if (r.sharedMaterial != null)
                    {
                        from = r.sharedMaterial;
                        break;
                    }
                }
            }

            var m = from != null ? new Material(from) : new Material(Shader.Find("Universal Render Pipeline/Lit"));
            Color colour = which == 4 ? Sash : which >= 2 ? Trousers[which - 2] : Shirt[which];
            m.color = colour;
            m.SetColor("_BaseColor", colour);
            m.SetFloat("_Smoothness", 0.1f);
            cloth[which] = m;
            return m;
        }

        private void Draw()
        {
#if HEADLESS_STUB
            return;
#else
            if (!Application.isPlaying)
            {
                return;
            }

            foreach (var t in tubes)
            {
                if (t.Drawn == null)
                {
                    t.Drawn = new GameObject("Clothes: " + t.Name);
                    if (owner != null)
                    {
                        t.Drawn.transform.SetParent(owner, false);
                    }

                    t.Mesh = new Mesh();
                    t.Mesh.MarkDynamic();
                    t.Drawn.AddComponent<MeshFilter>().sharedMesh = t.Mesh;
                    t.Drawn.AddComponent<MeshRenderer>().sharedMaterial = Cloth(t.IsSash ? 4 : (t.Trousers ? 2 : 0) + (side == 1 ? 1 : 0));
                    t.TrisSet = false;
                    t.Tris.Clear();
                    int n = t.Rings * t.Cols;
                    for (int r = 0; r + 1 < t.Rings; r++)
                    {
                        for (int c = 0; c < t.Cols; c++)
                        {
                            int a = r * t.Cols + c, b = r * t.Cols + (c + 1) % t.Cols, d = a + t.Cols, e = b + t.Cols;
                            t.Tris.AddRange(new[] { a, b, d, b, e, d });
                            t.Tris.AddRange(new[] { n + a, n + d, n + b, n + b, n + d, n + e });
                        }
                    }
                }

                t.Verts.Clear();
                for (int pass = 0; pass < 2; pass++)
                {
                    for (int i = 0; i < t.Rings * t.Cols; i++)
                    {
                        Vector3 p = x[t.Start + i];
                        t.Verts.Add(owner != null ? t.Drawn.transform.InverseTransformPoint(p) : p);
                    }
                }

                t.Mesh.SetVertices(t.Verts);
                if (!t.TrisSet)
                {
                    t.Mesh.SetTriangles(t.Tris, 0);
                    t.TrisSet = true;
                }

                t.Mesh.RecalculateNormals();
                t.Mesh.RecalculateBounds();
            }

            DrawXRay();
#endif
        }

        private readonly List<Vector3> xrayVerts = new List<Vector3>();
        private readonly List<int> xrayLines = new List<int>();

        private void DrawXRay()
        {
            if (!XRay && xray == null)
            {
                return;
            }

            if (xray == null)
            {
                xray = new GameObject("Clothes: x-ray");
                xrayMesh = new Mesh();
                xrayMesh.MarkDynamic();
                xray.AddComponent<MeshFilter>().sharedMesh = xrayMesh;
                var shader = Shader.Find("Hidden/Internal-Colored");
                var m = new Material(shader != null ? shader : Shader.Find("Sprites/Default"));
                m.color = Color.red;
                m.SetInt("_ZTest", 8); // (always: drawn through everything)
                m.renderQueue = 5000;
                xray.AddComponent<MeshRenderer>().sharedMaterial = m;
            }

            xray.SetActive(XRay);
            xrayVerts.Clear();
            xrayLines.Clear();
            if (XRay)
            {
                const float s = 0.012f;
                foreach (int i in inside)
                {
                    Vector3 p = x[i];
                    foreach (var w in new[] { Vector3.right, Vector3.up, Vector3.forward })
                    {
                        xrayLines.Add(xrayVerts.Count);
                        xrayVerts.Add(p - w * s);
                        xrayLines.Add(xrayVerts.Count);
                        xrayVerts.Add(p + w * s);
                    }
                }
            }

            xrayMesh.Clear();
            xrayMesh.SetVertices(xrayVerts);
            xrayMesh.SetIndices(xrayLines.ToArray(), MeshTopology.Lines, 0);
            xrayMesh.RecalculateBounds();
        }

        /// <summary>The outfit taken off (its meshes gone).</summary>
        public void Release()
        {
            foreach (var t in tubes)
            {
                if (t.Drawn != null)
                {
                    if (Application.isPlaying)
                    {
                        Object.Destroy(t.Drawn);
                    }
                    else
                    {
                        Object.DestroyImmediate(t.Drawn);
                    }
                }
            }

            if (xray != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(xray);
                }
                else
                {
                    Object.DestroyImmediate(xray);
                }
            }

            tubes.Clear();
            x = new Vector3[0];
#if !HEADLESS_STUB
            job.Drop();
#endif
        }

        /// <summary>Points that are inside the drawn body this picture (for the x-ray and the report).</summary>
        public int InsideNow => inside.Count;

        /// <summary>Every tube's points, ring by ring, for a picture drawn from the numbers: (name, rings, columns, points).</summary>
        public IEnumerable<(string, int, int, Vector3[])> Tubes()
        {
            foreach (var t in tubes)
            {
                var pts = new Vector3[t.Rings * t.Cols];
                System.Array.Copy(x, t.Start, pts, 0, pts.Length);
                yield return (t.Name, t.Rings, t.Cols, pts);
            }
        }

        /// <summary>The parts as drawn this picture (for the same picture): place and turn.</summary>
        public (Vector3, Quaternion) PartFrame(PartId p) => (fp[(int)p], fr[(int)p]);

        // ---- What the cloth is doing, for his playtest log (22 November, his ask at 11:45) -------------------------

        /// <summary>While above 0 (Shake him sets it), a full picture of the cloth goes to the log this often (seconds).</summary>
        public static float DumpEvery;

        /// <summary>Set by the lab's "Log the cloth now": one full picture of each man's cloth at the next picture.</summary>
        public static int DumpRequests;

        /// <summary>Did the last picture run on other cores (true), or here?</summary>
        public bool RanOnOtherCores { get; private set; }

        /// <summary>The main thread's time for the last picture (ms).</summary>
        public float LastMs { get; private set; }

        /// <summary>
        /// One line on what the cloth is doing this picture: how far its points are from their homes (all, and at the
        /// cuffs and trouser bottoms), how far the ends hang, how many are at their fence, what the last word pushed,
        /// what is inside, and the cost.
        /// </summary>
        public string Report()
        {
            if (x.Length == 0)
            {
                return "no cloth";
            }

            double sumAll = 0, sumEnds = 0, hang = 0;
            int nAll = 0, nEnds = 0, atFence = 0;
            float most = 0f;
            string mostIn = "";
            foreach (var t in tubes)
            {
                for (int i = t.Start; i < t.Start + t.Rings * t.Cols; i++)
                {
                    if (sway[i] <= 0f)
                    {
                        continue;
                    }

                    Vector3 off = x[i] - home[i];
                    float m = off.magnitude;
                    sumAll += m; nAll++;
                    if (m > most) { most = m; mostIn = t.Name; }
                    if (sway[i] > 0.8f) { sumEnds += m; hang += -off.y; nEnds++; }
                    float fence = Wobble * sway[i] * size;
                    if (fence > 0f && m >= fence * 0.97f) { atFence++; }
                }
            }

            long pushed = 0;
            foreach (var st in stats) { pushed += st.Pushed; }

            return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "wobble {0}; {1}; {2} points, {3} free to sway; from home: on average {4:0.0} cm, the most {5:0.0} cm ({6}); at the cuffs and trouser bottoms {7:0.0} cm, hanging {8:0.0} cm; at their fence {9}; the last word pushed {10} times; inside now {11}; {12:0.00} ms; fit {13:0.000}/{14:0.000}/{15:0.0}, reach {16:0.000}, weight {17:0.0}, hold {18:0.00}",
                WobbleOn ? "on" : "off", RanOnOtherCores ? "other cores" : "main thread", x.Length, nAll,
                nAll > 0 ? 100.0 * sumAll / nAll : 0.0, 100f * most, mostIn, nEnds > 0 ? 100.0 * sumEnds / nEnds : 0.0, nEnds > 0 ? 100.0 * hang / nEnds : 0.0,
                atFence, pushed, inside.Count, LastMs, Gap, ShirtGap, Loose, Wobble, Weight, Hold);
        }

        /// <summary>The whole cloth and the body's parts as drawn, as the headless dump's lines (outfit.py and coverage.py read them).</summary>
        public IEnumerable<string> DumpLines(string at, string who)
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            foreach (var (tubeName, rings, cols, pts) in Tubes())
            {
                var sb = new System.Text.StringBuilder();
                foreach (var p in pts)
                {
                    sb.Append(p.x.ToString("0.0000", inv)).Append(':').Append(p.y.ToString("0.0000", inv)).Append(':').Append(p.z.ToString("0.0000", inv)).Append(' ');
                }

                yield return $"OUTFITTUBE {at} {who} {tubeName.Replace(' ', '_').Replace("'", "")} {rings} {cols} {sb}";
            }

            var sf = new System.Text.StringBuilder();
            foreach (var id in PartInfo.All)
            {
                var (p, q) = PartFrame(id);
                sf.Append(string.Format(inv, "{0}:{1:0.0000}:{2:0.0000}:{3:0.0000}:{4:0.00000}:{5:0.00000}:{6:0.00000}:{7:0.00000} ", id, p.x, p.y, p.z, q.x, q.y, q.z, q.w));
            }

            yield return $"PARTFRAMES {at} {who} {sf}";
        }
    }
}
