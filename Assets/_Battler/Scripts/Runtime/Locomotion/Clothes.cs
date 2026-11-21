using System.Collections.Generic;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// A fighter's clothes (the plan "Cloth that moves: samurai clothes", 21 November): garments made by code from flat
    /// pieces, pinned to his body, simulated by <see cref="ClothNet"/> against his body's own shapes, drawn as meshes,
    /// and the parts of him they cover hidden. Version 1: the hakama as one pleated skirt (the undivided kind) pinned at
    /// a sash on his hips; tied-back sleeves on the upper arms.
    /// </summary>
    public sealed class Clothes
    {
        // ---- Switches and sliders (Combat Feel pushes them) ----------------------------------------------------------

        /// <summary>What the fighters in the labs and the dance wear: 0 none (old), 1 the skirt, 2 the skirt and sleeves.</summary>
        public static int Wear = 0;

        /// <summary>On: the parts the clothes cover are not drawn (the hips and thighs under the skirt, the upper arms under the sleeves).</summary>
        public static bool HideCovered = true;

        /// <summary>On: the cloth's points are drawn as small marks (the lab's look at the net).</summary>
        public static bool ShowPoints;

        public const float DefaultWeight = 1f, DefaultStiffness = 0.6f, DefaultStretch = 0.03f, DefaultAir = 1.2f, DefaultFeel = 0.7f,
            DefaultWind = 0.3f, DefaultGap = 0.015f, DefaultLeash = 0.25f;

        public const int DefaultSteps = 8;

        /// <summary>Heavy hangs straight and swings slowly; light flutters in the wind (1: like cotton).</summary>
        public static float Weight = DefaultWeight;

        /// <summary>How hard it is to fold (0 soft as silk, 1 stiff as canvas).</summary>
        public static float Stiffness = DefaultStiffness;

        /// <summary>How far a thread may stretch (share of its length, roughly).</summary>
        public static float Stretch = DefaultStretch;

        /// <summary>How fast a swing dies away (a second).</summary>
        public static float Air = DefaultAir;

        /// <summary>How much of his movement the cloth feels: 1 thrown about by everything he does; lower calmer.</summary>
        public static float Feel = DefaultFeel;

        /// <summary>A light breeze (m/s squared on cloth of weight 1).</summary>
        public static float Wind = DefaultWind;

        /// <summary>How far off the body the cloth is held (metres).</summary>
        public static float Gap = DefaultGap;

        /// <summary>How far a sleeve's point may stray from its place (metres; the skirt's hem twice as far).</summary>
        public static float Leash = DefaultLeash;

        /// <summary>Small steps a picture: more is safer and costs more.</summary>
        public static int Steps = DefaultSteps;

        /// <summary>The colours of the two sides' clothes: a hakama and its sleeves.</summary>
        public static readonly Color[] Colours = { new Color(0.16f, 0.2f, 0.36f), new Color(0.42f, 0.12f, 0.1f) };

        // ---- Measured, everywhere --------------------------------------------------------------------------------------

        public static long AllChecked, AllInside;

        /// <summary>The last garment put back to hanging, and why.</summary>
        public static string LastReset = "";
        public static float AllDeepest, AllStretch, AllCostMs;
        public static int AllResets, AllSteps, AllPinched;

        /// <summary>Everything the clothes measured so far, told.</summary>
        public static string Counted() => AllSteps == 0 ? "none worn" : string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0} cloth steps; a point inside his body {1:0.000}% of the time ({2} of {3} point-steps), the deepest {4:0.0} cm; a thread stretched {5:0}% at most, to twice its length in {9} pictures; put back to hanging {6} times{8}; {7:0.00} ms a fighter a picture",
            AllSteps, AllChecked > 0 ? 100.0 * AllInside / AllChecked : 0.0, AllInside, AllChecked, 100f * AllDeepest, 100f * AllStretch, AllResets, AllSteps > 0 ? AllCostMs / AllSteps : 0f, LastReset.Length > 0 ? " (the last: " + LastReset + ")" : "", AllPinched);

        // ---- One garment -----------------------------------------------------------------------------------------------

        private sealed class Garment
        {
            public string Name;
            public PartId On;
            public ClothNet Net;
            public int Columns, Rows;
            public Vector3[] Rest;         // each point's place against the part it hangs from (its local space)
            public GameObject Drawn;
            public Mesh Mesh;
            public readonly List<Vector3> Verts = new List<Vector3>();
            public readonly List<int> Tris = new List<int>();
            public Frame Was;
            public bool Known;
        }

        private readonly List<Garment> garments = new List<Garment>();
        private readonly UnitRig rig;
        private readonly ShockWave wave;
        private readonly float size;
        private readonly int side;
        private readonly Transform owner;
        private int worn = -1;
        private readonly List<Renderer> hidden = new List<Renderer>();

        public Clothes(UnitRig rig, ShockWave wave, float size, int side, Transform owner)
        {
            this.owner = owner;
            this.rig = rig;
            this.wave = wave;
            this.size = Mathf.Max(0.3f, size);
            this.side = side;
        }

        /// <summary>What he wears now (0 none).</summary>
        public int Worn => Mathf.Max(0, worn);

        /// <summary>His garments' points (for the lab and the report).</summary>
        public int Points
        {
            get
            {
                int n = 0;
                foreach (var g in garments)
                {
                    n += g.Net.Count;
                }

                return n;
            }
        }

        /// <summary>His own measure so far: point-steps checked, inside the body, the deepest, the most stretch, resets.</summary>
        public long Checked, Inside;

        public float Deepest, MostStretch;
        public int Resets;

        /// <summary>A place and a turn (the stub engine has no matrices).</summary>
        private struct Frame
        {
            public Vector3 At;
            public Quaternion Turn;

            public Vector3 Of(Vector3 local) => At + Turn * local;

            public Vector3 Into(Vector3 world) => Quaternion.Inverse(Turn) * (world - At);
        }

        private Frame FrameOf(PartId p)
        {
            var t = rig.Part(p).transform;
            // (As the body is drawn: the shock wave's turn on the part, so the clothes pick it up.)
            Vector3 at = wave != null ? wave.DrawnPoint(p, t.position) : t.position;
            Quaternion turn = wave != null ? wave.DrawnTurn(p) * t.rotation : t.rotation;
            return new Frame { At = at, Turn = turn };
        }

        /// <summary>
        /// One picture: what he wears made or taken off if the switch changed; then the body's shapes and the pins brought
        /// to where his body is, the cloth stepped, the meshes drawn.
        /// </summary>
        public void Step(float dt, bool allowed)
        {
            int want = allowed ? Mathf.Clamp(Wear, 0, 2) : 0;
            if (rig == null)
            {
                return;
            }

            if (want != worn)
            {
                Make(want);
            }

            if (garments.Count == 0)
            {
                return;
            }

            var clock = System.Diagnostics.Stopwatch.StartNew();
            var shapes = BodyShapes();
            foreach (var g in garments)
            {
                var net = g.Net;
                var frame = FrameOf(g.On);
                // (Jumped far in one picture (a teleport, a set-out): put back to hanging where he is now.)
                if (g.Known && (frame.At - g.Was.At).sqrMagnitude > 1f)
                {
                    Hang(g, frame);
                }

                for (int i = 0; i < net.Count; i++)
                {
                    net.Home[i] = frame.Of(g.Rest[i]);
                }

                // (The body's shapes, as they were at the last picture and as they are now.)
                for (int k = 0; k < shapes.Count; k++)
                {
                    var s = shapes[k];
                    if (k < net.Shapes.Count && net.Shapes[k].Part == s.Part)
                    {
                        s.A0 = net.Shapes[k].A1;
                        s.B0 = net.Shapes[k].B1;
                        s.Turn0 = net.Shapes[k].Turn1;
                    }

                    shapes[k] = s;
                }

                net.Shapes.Clear();
                net.Shapes.AddRange(shapes);
                net.Gap = Gap * size;
                net.Thickness = 0.02f * size;
                net.Air = Air;
                net.Feel = Feel;
                net.Weight = Weight;
                net.Steps = Mathf.Clamp(Steps, 1, 32);
                net.Stretch = 6f * Mathf.Clamp(Stretch, 0f, 0.2f);
                net.Bending = 8f * (1f - Mathf.Clamp01(Stiffness)) + 0.05f;
                net.Wind = WindNow() * Wind;
                net.Floor = 0f;
                Frame was = g.Known ? g.Was : frame, now = frame;
                int resetsWere = net.Resets;
                net.Step(dt, p => now.Of(was.Into(p)));
                if (net.Resets > resetsWere)
                {
                    LastReset = g.Name + ": " + net.WhyReset;
                }

                g.Was = frame;
                g.Known = true;
                Draw(g);
                Checked = 0;
                Inside = 0;
            }

            float ms = (float)clock.Elapsed.TotalMilliseconds;
            AllSteps++;
            AllCostMs += ms;
            long c = 0, inside = 0;
            Deepest = 0f;
            MostStretch = 0f;
            Resets = 0;
            foreach (var g in garments)
            {
                c += g.Net.Checked;
                inside += g.Net.Inside;
                Deepest = Mathf.Max(Deepest, g.Net.Deepest);
                MostStretch = Mathf.Max(MostStretch, g.Net.MostStretch);
                Resets += g.Net.Resets;
            }

            int pinched = 0;
            foreach (var g in garments)
            {
                pinched += g.Net.Pinched;
            }

            AllPinched += pinched - lastPinched;
            lastPinched = pinched;
            AllChecked += c - lastChecked;
            AllInside += inside - lastInside;
            AllResets += Resets - lastResets;
            lastChecked = c;
            lastInside = inside;
            lastResets = Resets;
            Checked = c;
            Inside = inside;
            AllDeepest = Mathf.Max(AllDeepest, Deepest);
            AllStretch = Mathf.Max(AllStretch, MostStretch);
        }

        private long lastChecked, lastInside;
        private int lastResets, lastPinched;
        private float windClock;

        private Vector3 WindNow()
        {
            windClock += 1f / 60f;
            float gust = 0.6f + 0.4f * Mathf.Sin(windClock * 0.9f) * Mathf.Sin(windClock * 2.3f + 1f);
            return new Vector3(0.8f, 0f, 0.6f) * gust;
        }

        /// <summary>His body's shapes from the colliders of his parts (as drawn), for the cloth to be pushed out of.</summary>
        private List<ClothNet.Shape> BodyShapes()
        {
            var list = new List<ClothNet.Shape>(16);
            foreach (var id in PartInfo.All)
            {
                if (id == PartId.Head || rig.IsSevered(id))
                {
                    continue;
                }

                var part = rig.Part(id);
                var col = part != null ? part.partCollider : null;
                if (col == null)
                {
                    continue;
                }

                var t = col.transform;
                Quaternion turn = wave != null ? wave.DrawnTurn(id) : Quaternion.identity;
                System.Func<Vector3, Vector3> drawn = x => wave != null ? wave.DrawnPoint(id, x) : x;
                var s = new ClothNet.Shape { Part = (int)id };
                Vector3 scale = t.lossyScale;
                float most = Mathf.Max(Mathf.Abs(scale.x), Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
                if (col is CapsuleCollider cap)
                {
                    Vector3 axis = cap.direction == 0 ? Vector3.right : cap.direction == 1 ? Vector3.up : Vector3.forward;
                    float half = Mathf.Max(0f, 0.5f * cap.height - cap.radius);
                    s.Kind = 1;
                    s.A1 = drawn(t.TransformPoint(cap.center - axis * half));
                    s.B1 = drawn(t.TransformPoint(cap.center + axis * half));
                    s.Radius = cap.radius * most;
                }
                else if (col is SphereCollider ball)
                {
                    s.Kind = 0;
                    s.A1 = s.B1 = drawn(t.TransformPoint(ball.center));
                    s.Radius = ball.radius * most;
                }
                else if (col is BoxCollider box)
                {
                    s.Kind = 2;
                    s.A1 = drawn(t.TransformPoint(box.center));
                    s.Turn1 = turn * t.rotation;
                    s.Half = 0.5f * Vector3.Scale(box.size, new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
                }
                else
                {
                    continue;
                }

                s.A0 = s.A1;
                s.B0 = s.B1;
                s.Turn0 = s.Turn1;
                list.Add(s);
            }

            return list;
        }

        // ---- Made -----------------------------------------------------------------------------------------------------

        private void Make(int what)
        {
            foreach (var g in garments)
            {
                if (g.Drawn != null)
                {
                    if (Application.isPlaying)
                    {
                        Object.Destroy(g.Drawn);
                    }
                    else
                    {
                        Object.DestroyImmediate(g.Drawn);
                    }
                }
            }

            garments.Clear();
            Hide(false);
            worn = what;
            lastChecked = lastInside = 0;
            lastResets = lastPinched = 0;
            if (what <= 0)
            {
                return;
            }

            garments.Add(Skirt());
            if (what >= 2)
            {
                garments.Add(Sleeve(PartId.UpperArmL));
                garments.Add(Sleeve(PartId.UpperArmR));
            }

            foreach (var g in garments)
            {
                Hang(g, FrameOf(g.On));
            }

            Hide(HideCovered);
        }

        /// <summary>
        /// The hakama, the undivided kind: one pleated skirt from a sash at his waist to a little above his ankles. Cut as a
        /// strip of 40 columns by 16 rows, wrapped into a tube round his hips, wider at the hem; the top row pinned to the
        /// sash, folded into pleats there (five in front, two behind), the folds fading down to the hem.
        /// </summary>
        private Garment Skirt()
        {
            const int C = 40, R = 16;
            var g = new Garment { Name = "hakama", On = PartId.Pelvis, Columns = C, Rows = R, Net = new ClothNet(C * R), Rest = new Vector3[C * R] };
            float s = size;
            // (In the pelvis's own space: y up, z his front. The waist 7 cm above the pelvis's joint (about the navel); the
            // hem 93 cm below it.)
            float top = 0.07f * s, bottom = -0.86f * s;
            for (int r = 0; r < R; r++)
            {
                float down = r / (R - 1f);
                float y = Mathf.Lerp(top, bottom, down);
                float rx = Mathf.Lerp(0.17f, 0.42f, Mathf.Pow(down, 0.45f)) * s;
                float rz = Mathf.Lerp(0.13f, 0.5f, Mathf.Pow(down, 0.45f)) * s;
                for (int c = 0; c < C; c++)
                {
                    float a = 2f * Mathf.PI * c / C;
                    // (The pleats: a fold out and in every four columns, deepest at the sash.)
                    int k = c % 4;
                    float pleat = (k == 0 ? 1f : k == 2 ? -1f : 0f) * 0.018f * s * (1f - 0.6f * down);
                    float sx = Mathf.Sin(a), cz = Mathf.Cos(a);
                    g.Rest[r * C + c] = new Vector3(sx * (rx + pleat), y, cz * (rz + pleat));
                }
            }

            Weave(g, true);
            for (int i = 0; i < g.Net.Count; i++)
            {
                int r = i / C;
                g.Net.Free[i] = r == 0 ? 0f : 1f;
                g.Net.Leash[i] = (0.06f + 2f * Leash * (r / (R - 1f))) * s;
            }

            return g;
        }

        /// <summary>A sleeve tied back: a short loose tube round the upper arm from the shoulder to the elbow, its top pinned round the shoulder.</summary>
        private Garment Sleeve(PartId arm)
        {
            const int C = 12, R = 6;
            var g = new Garment { Name = arm == PartId.UpperArmL ? "left sleeve" : "right sleeve", On = arm, Columns = C, Rows = R, Net = new ClothNet(C * R), Rest = new Vector3[C * R] };
            float s = size;
            var t = rig.Part(arm).transform;
            var elbow = rig.Part(arm == PartId.UpperArmL ? PartId.ForearmL : PartId.ForearmR).transform;
            // (Along the arm's own capsule in its own space, from a little above the shoulder's joint to near the elbow,
            // loose round it: its radius, the gap and a few centimetres more, more at the bottom.)
            Quaternion into = Quaternion.Inverse(t.rotation);
            Vector3 end = into * (elbow.position - t.position);
            float armRadius = 0.04f * s;
            if (rig.Part(arm).partCollider is CapsuleCollider cap)
            {
                var ct = cap.transform;
                Vector3 axis = cap.direction == 0 ? Vector3.right : cap.direction == 1 ? Vector3.up : Vector3.forward;
                float half = Mathf.Max(0f, 0.5f * cap.height - cap.radius);
                end = into * (ct.TransformPoint(cap.center + axis * half) - t.position);
                Vector3 scale = ct.lossyScale;
                armRadius = cap.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
            }

            Vector3 along = end.sqrMagnitude > 1e-6f ? end.normalized : Vector3.down;
            Vector3 side1 = Vector3.Cross(along, Vector3.forward);
            if (side1.sqrMagnitude < 0.25f)
            {
                side1 = Vector3.Cross(along, Vector3.right);
            }

            side1.Normalize();
            Vector3 side2 = Vector3.Cross(along, side1).normalized;
            float length = end.magnitude;
            // (Loose on the outside, close on the inside: his chest is two or three centimetres from his arm.)
            Vector3 outward = into * (t.position - rig.Part(PartId.Chest).transform.position);
            outward -= along * Vector3.Dot(outward, along);
            outward = outward.sqrMagnitude > 1e-8f ? outward.normalized : side1;
            for (int r = 0; r < R; r++)
            {
                float down = r / (R - 1f);
                float loose = Mathf.Lerp(0.012f, 0.035f, down) * s;
                float radius = armRadius + Gap * s + loose;
                Vector3 centre = along * Mathf.Lerp(-0.02f * s, length, down) + outward * (0.8f * loose);
                for (int c = 0; c < C; c++)
                {
                    float a = 2f * Mathf.PI * c / C;
                    g.Rest[r * C + c] = centre + (side1 * Mathf.Cos(a) + side2 * Mathf.Sin(a)) * radius;
                }
            }

            Weave(g, true);
            for (int i = 0; i < g.Net.Count; i++)
            {
                int r = i / C;
                g.Net.Free[i] = r == 0 ? 0f : 1f;
                g.Net.Leash[i] = (0.03f + Leash * (r / (R - 1f))) * s;
            }

            return g;
        }

        /// <summary>The threads of a tube: round and down, the two diagonals, and every other point round and down to bend against.</summary>
        private static void Weave(Garment g, bool tube)
        {
            int C = g.Columns, R = g.Rows;
            var net = g.Net;
            for (int i = 0; i < net.Count; i++)
            {
                net.X[i] = g.Rest[i];
            }

            int At(int r, int c) => r * C + ((c % C) + C) % C;
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    if (tube || c + 1 < C)
                    {
                        net.Join(At(r, c), At(r, c + 1));
                    }

                    if (r + 1 < R)
                    {
                        net.Join(At(r, c), At(r + 1, c));
                        net.Join(At(r, c), At(r + 1, c + 1));
                        net.Join(At(r, c + 1), At(r + 1, c));
                    }

                    if (tube || c + 2 < C)
                    {
                        net.Join(At(r, c), At(r, c + 2), true);
                    }

                    if (r + 2 < R)
                    {
                        net.Join(At(r, c), At(r + 2, c), true);
                    }
                }
            }

            // (The mesh: two sides, each its own triangles, so either is lit and seen.)
            g.Tris.Clear();
            int n = net.Count;
            for (int r = 0; r + 1 < R; r++)
            {
                for (int c = 0; c < (tube ? C : C - 1); c++)
                {
                    int a = At(r, c), b = At(r, c + 1), d = At(r + 1, c), e = At(r + 1, c + 1);
                    g.Tris.AddRange(new[] { a, d, b, b, d, e });
                    g.Tris.AddRange(new[] { n + a, n + b, n + d, n + b, n + e, n + d });
                }
            }
        }

        /// <summary>A garment put back to hanging: every point at its place on him, still; then let fall for a moment.</summary>
        private void Hang(Garment g, Frame frame)
        {
            var net = g.Net;
            for (int i = 0; i < net.Count; i++)
            {
                net.X[i] = frame.Of(g.Rest[i]);
                net.Home[i] = net.X[i];
            }

            net.Settle();
            net.Forget();
            net.Reset = () => Hang(g, FrameOf(g.On));
            g.Was = frame;
            g.Known = true;
        }

        /// <summary>The parts the clothes cover hidden, or shown again.</summary>
        private void Hide(bool hide)
        {
            foreach (var r in hidden)
            {
                if (r != null)
                {
                    r.enabled = true;
                }
            }

            hidden.Clear();
            if (!hide || worn <= 0)
            {
                return;
            }

            var covered = new List<PartId> { PartId.Pelvis, PartId.ThighL, PartId.ThighR };
            if (worn >= 2)
            {
                covered.Add(PartId.UpperArmL);
                covered.Add(PartId.UpperArmR);
            }

            foreach (var id in covered)
            {
                var part = rig.Part(id);
                if (part == null)
                {
                    continue;
                }

                foreach (var r in part.GetComponentsInChildren<Renderer>(true))
                {
                    // (Only the part's own mesh, not what it holds.)
                    if (r.transform.parent == part.transform && r.enabled)
                    {
                        r.enabled = false;
                        hidden.Add(r);
                    }
                }
            }
        }

        private bool hiddenAsSet = true;

        /// <summary>The switch "covered parts hidden" followed.</summary>
        public void FollowHide()
        {
            if (hiddenAsSet != HideCovered)
            {
                hiddenAsSet = HideCovered;
                Hide(HideCovered);
            }
        }

        // ---- Drawn ----------------------------------------------------------------------------------------------------

        private void Draw(Garment g)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (g.Drawn == null)
            {
                g.Drawn = new GameObject("Clothes: " + g.Name);
                // (Under his unit, so it goes when he goes; its corners are put in the unit's own space each picture.)
                if (owner != null)
                {
                    g.Drawn.transform.SetParent(owner, false);
                }

                g.Mesh = new Mesh();
                g.Mesh.MarkDynamic();
                g.Drawn.AddComponent<MeshFilter>().sharedMesh = g.Mesh;
                var renderer = g.Drawn.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = Cloth(side);
            }

            g.Verts.Clear();
            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < g.Net.Count; i++)
                {
                    g.Verts.Add(owner != null ? g.Drawn.transform.InverseTransformPoint(g.Net.X[i]) : g.Net.X[i]);
                }
            }

            g.Mesh.SetVertices(g.Verts);
            if (!trisSet.Contains(g))
            {
                g.Mesh.SetTriangles(g.Tris, 0);
                trisSet.Add(g);
            }

            g.Mesh.RecalculateNormals();
            g.Mesh.RecalculateBounds();
        }

        private readonly HashSet<Garment> trisSet = new HashSet<Garment>();

        private static readonly Material[] cloth = new Material[2];

        /// <summary>The cloth's look: the body's own material, coloured for his side.</summary>
        private Material Cloth(int team)
        {
            team = team == 1 ? 1 : 0;
            if (cloth[team] != null)
            {
                return cloth[team];
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
            m.color = Colours[team];
            m.SetColor("_BaseColor", Colours[team]);
            m.SetFloat("_Smoothness", 0.15f);
            cloth[team] = m;
            return m;
        }

        /// <summary>His clothes taken off and their meshes gone (his unit is going).</summary>
        public void Release()
        {
            Make(0);
        }

        /// <summary>Each garment's own measure: points, pictures pinched, the most stretch, point-steps inside.</summary>
        public string Told()
        {
            var b = new System.Text.StringBuilder();
            foreach (var g in garments)
            {
                b.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}: {1} points, pinched in {2} pictures, stretched {3:0}% at most, inside {4} of {5}, the deepest {6:0.0} cm, put back {7}, last pinched {8}; ",
                    g.Name, g.Net.Count, g.Net.Pinched, 100f * g.Net.MostStretch, g.Net.Inside, g.Net.Checked, 100f * g.Net.Deepest, g.Net.Resets, g.Net.Worst));
            }

            return b.ToString();
        }

        // ---- For the report: the cloth's shape --------------------------------------------------------------------------

        /// <summary>Every point of every garment, with its garment's name, for a picture drawn from the numbers.</summary>
        public IEnumerable<(string, Vector3)> AllPoints()
        {
            foreach (var g in garments)
            {
                for (int i = 0; i < g.Net.Count; i++)
                {
                    yield return (g.Name, g.Net.X[i]);
                }
            }
        }
    }
}
