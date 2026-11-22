using System.Collections.Generic;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// A fighter's clothes. Rebuilt 22 November after his look at the first version ("they are wearing Victorian
    /// dresses"; "a single cloth that is pinned at the waist, open in front so that you can see the chest"; his drawing:
    /// a long robe from the shoulders to the ankles, open down the front, a sash at the waist): one open robe, held at
    /// the shoulders and pinned by a sash at the waist, open down the front above and below the sash, no sleeves yet
    /// (his answer of 07:25). Soft cloth; the body's limbs kept from passing between its points; each fighter's robe
    /// worked out on another core (<see cref="OnOtherCores"/>, Burst) and taken up a picture later.
    /// </summary>
    public sealed class Clothes
    {
        // ---- Switches and sliders (Combat Feel pushes them) ----------------------------------------------------------

        /// <summary>
        /// What the fighters in the labs and the dance wear: 0 nothing; 1 a shirt and trousers that never go into the
        /// drawn body (<see cref="Outfit"/>, 22 November, after his look at the robe: "still not working"); 2 the open robe
        /// of this morning (old); 3 the shirt and trousers cut and sewn like real clothes, draped once and carried on the
        /// drawn body (<see cref="SewnOutfit"/>, 22 November afternoon).
        /// </summary>
        public static int Kind = 3;

        /// <summary>The robe is worn (Kind 2).</summary>
        public static bool Wear => Kind == 2;

        /// <summary>On: each robe is worked out on another core (a worker thread, compiled by Burst) and taken up a picture later. Off: here, on the main thread.</summary>
        public static bool OnOtherCores = true;

        public const float DefaultWeight = 1f, DefaultStiffness = 0.25f, DefaultStretch = 0.025f, DefaultAir = 0.6f, DefaultFeel = 0.85f,
            DefaultWind = 0.3f, DefaultGap = 0.02f, DefaultLeash = 0.5f;

        public const int DefaultSteps = 8;

        /// <summary>Heavy hangs straight and swings slowly; light flutters in the wind (1: like cotton).</summary>
        public static float Weight = DefaultWeight;

        /// <summary>How hard it is to fold (0 soft as silk, 1 stiff as canvas).</summary>
        public static float Stiffness = DefaultStiffness;

        /// <summary>How far a thread may give (share of its length, roughly).</summary>
        public static float Stretch = DefaultStretch;

        /// <summary>How fast a swing dies away (a second).</summary>
        public static float Air = DefaultAir;

        /// <summary>How much of his movement the cloth feels: 1 thrown about by everything he does; lower calmer.</summary>
        public static float Feel = DefaultFeel;

        /// <summary>A light breeze (m/s squared on cloth of weight 1).</summary>
        public static float Wind = DefaultWind;

        /// <summary>How far off the body the cloth is held (metres).</summary>
        public static float Gap = DefaultGap;

        /// <summary>How far the hem may stray from where it hangs (metres; less higher up).</summary>
        public static float Leash = DefaultLeash;

        /// <summary>Small steps a picture: more is safer and costs more.</summary>
        public static int Steps = DefaultSteps;

        /// <summary>The colours of the two sides' robes, and of the sash.</summary>
        public static readonly Color[] Colours = { new Color(0.16f, 0.2f, 0.36f), new Color(0.42f, 0.12f, 0.1f) };

        public static Color SashColour = new Color(0.08f, 0.07f, 0.06f);

        // ---- Measured, everywhere --------------------------------------------------------------------------------------

        public static long AllChecked, AllInside, AllInsideDrawn, AllHeld;
        public static float AllDeepest, AllStretch, AllCostMs, AllSumsMs;
        public static int AllResets, AllSteps, AllPinched, AllPicturesSeenInside;

        /// <summary>The last garment put back to hanging, and why.</summary>
        public static string LastReset = "";

        /// <summary>Everything the clothes measured so far, told.</summary>
        public static string Counted() => AllSteps == 0 ? "none worn" : string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0} cloth steps; a point inside his body {1:0.000}% of the time ({2} of {3} point-steps), the deepest {4:0.0} cm; in {5} pictures ({6:0.0}% of them); a thread stretched {7:0}% at most, to twice its length in {8} pictures, held at {13:0.0} times its length {14} times; put back to hanging {9} times{10}; {11:0.00} ms a fighter a picture on the main thread ({12}; of it the sums {15:0.00} ms, the rest {16:0.00})",
            AllSteps, AllChecked > 0 ? 100.0 * AllInside / AllChecked : 0.0, AllInside, AllChecked, 100f * AllDeepest, AllPicturesSeenInside,
            100.0 * AllPicturesSeenInside / AllSteps, 100f * AllStretch, AllPinched, AllResets, LastReset.Length > 0 ? " (the last: " + LastReset + ")" : "",
            AllCostMs / AllSteps, Threaded ? "the sums on other cores" : "the sums here too", ClothSolve.TearsAt, AllHeld, AllSumsMs / AllSteps, (AllCostMs - AllSumsMs) / AllSteps);

        /// <summary>True when the sums run on other cores (in Unity with the switch on).</summary>
        public static bool Threaded
        {
            get
            {
#if HEADLESS_STUB
                return false;
#else
                return OnOtherCores;
#endif
            }
        }

        // ---- The garment -----------------------------------------------------------------------------------------------

        private struct Frame
        {
            public Vector3 At;
            public Quaternion Turn;

            public Vector3 Of(Vector3 local) => At + Turn * local;

            public Vector3 Into(Vector3 world) => Quaternion.Inverse(Turn) * (world - At);
        }

        private sealed class Garment
        {
            public string Name;
            public ClothNet Net;
            public int Columns, Rows, Sash;
            public Vector3[] OnChest, OnHips; // each point's place against the chest and against the hips
            public float[] ChestShare;         // how much of its home place is the chest's (1 at the shoulders, 0 at the sash and below)
            public GameObject Drawn;
            public Mesh Mesh;
            public readonly List<Vector3> Verts = new List<Vector3>();
            public readonly List<int> Tris = new List<int>();
            public bool TrisSet;
            public Frame Was;
            public bool Known;
#if !HEADLESS_STUB
            public readonly ClothRun Run = new ClothRun();
#endif
        }

        private Garment robe;
        private Outfit outfit;
        private bool outfitTried;
        private Vector3 outfitFit;
        private SewnOutfit sewn;
        private bool sewnTried;

        /// <summary>His shirt and trousers as tubes (Kind 1), if he wears them.</summary>
        public Outfit Outfit => outfit;

        /// <summary>His shirt and trousers cut and sewn (Kind 3), if he wears them.</summary>
        public SewnOutfit Sewn => sewn;

        /// <summary>The shirt and trousers put on the body as it is drawn this picture (Unity: LateUpdate).</summary>
        public void Place()
        {
            if (outfit == null && sewn == null)
            {
                return;
            }

            using (PhysicsBattler.Diagnostics.Perf.Begin(PhysicsBattler.Diagnostics.PerfSection.Clothes))
            {
                outfit?.Place(Time.deltaTime);
                sewn?.Place();
            }

            Trace();
        }

        private PhysicsBattler.Units.UnitController traceUnit;
        private float traceNext, dumpNext;
        private int dumpsSeen;

        /// <summary>
        /// What the cloth is doing, into his playtest log: a "cloth" line twice a second; and the whole cloth with the body
        /// as drawn ("clothdump" lines, the headless dump's format) every so often while "Shake him" runs, and once when
        /// "Log the cloth now" is pressed. `Tools/Headless/unitylog_outfit.sh` turns them into pictures.
        /// </summary>
        private void Trace()
        {
            if (traceUnit == null && owner != null)
            {
                traceUnit = owner.GetComponent<PhysicsBattler.Units.UnitController>();
            }

            if (traceUnit == null)
            {
                return;
            }

            float now = Time.time;
            if (now >= traceNext)
            {
                traceNext = now + 0.5f;
                PhysicsBattler.Diagnostics.PlaytestTrace.Note(traceUnit, "cloth", sewn != null ? sewn.Report() : outfit.Report());
            }

            bool asked = Outfit.DumpRequests != dumpsSeen;
            bool timed = Outfit.DumpEvery > 0f && now >= dumpNext;
            if (asked || timed)
            {
                dumpsSeen = Outfit.DumpRequests;
                dumpNext = now + Mathf.Max(0.2f, Outfit.DumpEvery);
                string who = owner.name.Replace(' ', '_');
                string when = now.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                foreach (var line in sewn != null ? sewn.DumpLines(when, who) : outfit.DumpLines(when, who))
                {
                    PhysicsBattler.Diagnostics.PlaytestTrace.Note(traceUnit, "clothdump", line);
                }
            }
        }
        private readonly UnitRig rig;
        private readonly ShockWave wave;
        private readonly float size;
        private readonly int side;
        private readonly Transform owner;
        private bool worn;
        private float lastDt = 1f / 60f;

        public Clothes(UnitRig rig, ShockWave wave, float size, int side, Transform owner)
        {
            this.rig = rig;
            this.wave = wave;
            this.size = Mathf.Max(0.3f, size);
            this.side = side;
            this.owner = owner;
        }

        /// <summary>His robe's points (for the lab and the report).</summary>
        public int Points => robe != null ? robe.Net.Count : outfit != null ? outfit.Points : sewn != null ? sewn.Points : 0;

        private Frame FrameOf(PartId p)
        {
            var t = rig.Part(p).transform;
            // (As the body is drawn: the shock wave's turn on the part, so the robe picks it up.)
            Vector3 at = wave != null ? wave.DrawnPoint(p, t.position) : t.position;
            Quaternion turn = wave != null ? wave.DrawnTurn(p) * t.rotation : t.rotation;
            return new Frame { At = at, Turn = turn };
        }

        private Vector3 HomeOf(Garment g, int i, Frame chest, Frame hips)
        {
            float w = g.ChestShare[i];
            return w <= 0f ? hips.Of(g.OnHips[i]) : w >= 1f ? chest.Of(g.OnChest[i]) : Vector3.Lerp(hips.Of(g.OnHips[i]), chest.Of(g.OnChest[i]), w);
        }

        /// <summary>
        /// One picture: the picture begun on another core last time taken up and drawn; then the body's shapes and the
        /// pins brought to where his body is now, and the next picture begun (or, off the other cores, run here).
        /// </summary>
        public void Step(float dt, bool allowed)
        {
            if (rig == null)
            {
                return;
            }

            // (The shirt and trousers: carried on the body; placed as the picture is drawn (Place), or here headless.)
            bool wantOutfit = allowed && Kind == 1;
            // (Its fit changed (the lab's sliders): cut again.)
            var fit = new Vector3(Outfit.Gap, Outfit.ShirtGap, Outfit.Loose);
            if (outfit != null && fit != outfitFit)
            {
                outfit.Release();
                outfit = null;
                outfitTried = false;
            }

            outfitFit = fit;
            if (wantOutfit && outfit == null && !outfitTried)
            {
                outfitTried = true;
                outfit = Outfit.Make(rig, wave, size, side, owner);
                if (outfit != null)
                {
                    Debug.Log("[Clothes] " + outfit.Check);
                    var unit = owner != null ? owner.GetComponent<PhysicsBattler.Units.UnitController>() : null;
                    if (unit != null)
                    {
                        PhysicsBattler.Diagnostics.PlaytestTrace.Note(unit, "clothes", outfit.Check);
                    }
                }
            }
            else if (!wantOutfit && outfit != null)
            {
                outfit.Release();
                outfit = null;
                outfitTried = false;
            }

            // (The shirt and trousers cut and sewn (the plan "Clothes cut and sewn like real clothes", step 3): carried on
            // the body as drawn, placed with the picture as the tubes are.)
            bool wantSewn = allowed && Kind == 3;
            if (wantSewn && sewn == null && !sewnTried)
            {
                sewnTried = true;
                sewn = SewnOutfit.Make(rig, wave, size, side, owner);
                string said = sewn != null ? sewn.Check : "no sewn clothes for this body (Garments_Medium.json not found, or no baked body)";
                Debug.Log("[Clothes] " + said);
                var unit = owner != null ? owner.GetComponent<PhysicsBattler.Units.UnitController>() : null;
                if (unit != null)
                {
                    PhysicsBattler.Diagnostics.PlaytestTrace.Note(unit, "clothes", said);
                }
            }
            else if (!wantSewn && sewn != null)
            {
                sewn.Release();
                sewn = null;
                sewnTried = false;
            }

#if HEADLESS_STUB
            // (Headless there is no LateUpdate: placed here, after the step, on the body as it stands.)
            outfit?.Place(dt);
            sewn?.Place();
#endif

            bool want = allowed && Wear;
            if (want != worn)
            {
                Make(want);
            }

            if (robe == null)
            {
                return;
            }

            var clock = System.Diagnostics.Stopwatch.StartNew();
            var g = robe;
            var net = g.Net;
#if !HEADLESS_STUB
            g.Run.End(net);
#endif
            Count(net);
            Draw(g);
            var chest = FrameOf(PartId.Chest);
            var hips = FrameOf(PartId.Pelvis);
            // (Jumped far in one picture (a teleport, a set-out): put back to hanging where he is now.)
            if (g.Known && (hips.At - g.Was.At).sqrMagnitude > 1f)
            {
                Hang(g);
            }

            for (int i = 0; i < net.Count; i++)
            {
                net.Home[i] = HomeOf(g, i, chest, hips);
            }

            BodyShapes(net);
            net.Gap = Gap * size;
            net.Thickness = 0.025f * size;
            net.Air = Air;
            net.Feel = Feel;
            net.Weight = Weight;
            net.Steps = Mathf.Clamp(Steps, 1, 32);
            net.Stretch = 6f * Mathf.Clamp(Stretch, 0f, 0.2f);
            net.Bending = 10f * (1f - Mathf.Clamp01(Stiffness)) + 0.05f;
            net.Wind = WindNow(dt) * Wind;
            net.Floor = 0f;
            Frame was = g.Known ? g.Was : hips, now = hips;
            net.Prepare(p => now.Of(was.Into(p)));
            g.Was = hips;
            g.Known = true;
            lastDt = dt;
            if (Threaded)
            {
#if !HEADLESS_STUB
                g.Run.Begin(net, dt);
#endif
            }
            else
            {
                var sums = System.Diagnostics.Stopwatch.StartNew();
                net.RunHere(dt);
                AllSumsMs += (float)sums.Elapsed.TotalMilliseconds;
            }

            AllSteps++;
            AllCostMs += (float)clock.Elapsed.TotalMilliseconds;
        }

        private long seenChecked, seenInside, seenDrawn, seenHeld;
        private int seenPictures, seenPinched, seenResets;
        private float windClock;

        private void Count(ClothNet net)
        {
            AllChecked += net.Checked - seenChecked;
            AllInside += net.Inside - seenInside;
            AllInsideDrawn += net.InsideDrawn - seenDrawn;
            AllPicturesSeenInside += net.PicturesInsideDrawn - seenPictures;
            AllPinched += net.Pinched - seenPinched;
            AllHeld += net.Held - seenHeld;
            seenHeld = net.Held;
            if (net.Resets > seenResets)
            {
                AllResets += net.Resets - seenResets;
                LastReset = robe.Name + ": " + net.WhyReset;
            }

            seenChecked = net.Checked;
            seenInside = net.Inside;
            seenDrawn = net.InsideDrawn;
            seenPictures = net.PicturesInsideDrawn;
            seenPinched = net.Pinched;
            seenResets = net.Resets;
            AllDeepest = Mathf.Max(AllDeepest, net.Deepest);
            AllStretch = Mathf.Max(AllStretch, net.MostStretch);
        }

        private Vector3 WindNow(float dt)
        {
            windClock += dt;
            float gust = 0.6f + 0.4f * Mathf.Sin(windClock * 0.9f) * Mathf.Sin(windClock * 2.3f + 1f);
            return new Vector3(0.8f, 0f, 0.6f) * gust;
        }

        /// <summary>His body's shapes from the colliders of his parts (as drawn), as they were and as they are now.</summary>
        private void BodyShapes(ClothNet net)
        {
            var before = net.Shapes.ToArray();
            net.Shapes.Clear();
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
                Vector3 Drawn(Vector3 x) => wave != null ? wave.DrawnPoint(id, x) : x;
                var s = new ClothSolve.Shape { Part = (int)id };
                Vector3 scale = t.lossyScale;
                float most = Mathf.Max(Mathf.Abs(scale.x), Mathf.Max(Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
                Vector3 a, b;
                Quaternion q = Quaternion.identity;
                if (col is CapsuleCollider cap)
                {
                    Vector3 axis = cap.direction == 0 ? Vector3.right : cap.direction == 1 ? Vector3.up : Vector3.forward;
                    float half = Mathf.Max(0f, 0.5f * cap.height - cap.radius);
                    s.Kind = 1;
                    a = Drawn(t.TransformPoint(cap.center - axis * half));
                    b = Drawn(t.TransformPoint(cap.center + axis * half));
                    s.Radius = cap.radius * most;
                }
                else if (col is SphereCollider ball)
                {
                    s.Kind = 0;
                    a = b = Drawn(t.TransformPoint(ball.center));
                    s.Radius = ball.radius * most;
                }
                else if (col is BoxCollider box)
                {
                    s.Kind = 2;
                    a = b = Drawn(t.TransformPoint(box.center));
                    q = turn * t.rotation;
                    Vector3 half = 0.5f * Vector3.Scale(box.size, new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
                    s.Hx = half.x; s.Hy = half.y; s.Hz = half.z;
                }
                else
                {
                    continue;
                }

                s.A1x = a.x; s.A1y = a.y; s.A1z = a.z;
                s.B1x = b.x; s.B1y = b.y; s.B1z = b.z;
                s.Q1x = q.x; s.Q1y = q.y; s.Q1z = q.z; s.Q1w = q.w;
                // (Where it was at the last picture, if it was there; else where it is.)
                bool found = false;
                foreach (var w in before)
                {
                    if (w.Part == s.Part)
                    {
                        s.A0x = w.A1x; s.A0y = w.A1y; s.A0z = w.A1z;
                        s.B0x = w.B1x; s.B0y = w.B1y; s.B0z = w.B1z;
                        s.Q0x = w.Q1x; s.Q0y = w.Q1y; s.Q0z = w.Q1z; s.Q0w = w.Q1w;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    s.A0x = s.A1x; s.A0y = s.A1y; s.A0z = s.A1z;
                    s.B0x = s.B1x; s.B0y = s.B1y; s.B0z = s.B1z;
                    s.Q0x = s.Q1x; s.Q0y = s.Q1y; s.Q0z = s.Q1z; s.Q0w = s.Q1w;
                }

                net.Shapes.Add(s);
            }
        }

        // ---- Made -----------------------------------------------------------------------------------------------------

        private void Make(bool wear)
        {
            if (robe != null)
            {
#if !HEADLESS_STUB
                robe.Run.Drop();
#endif
                if (robe.Drawn != null)
                {
                    if (Application.isPlaying)
                    {
                        Object.Destroy(robe.Drawn);
                    }
                    else
                    {
                        Object.DestroyImmediate(robe.Drawn);
                    }
                }

                if (sash != null)
                {
                    if (Application.isPlaying)
                    {
                        Object.Destroy(sash);
                    }
                    else
                    {
                        Object.DestroyImmediate(sash);
                    }
                }
            }

            robe = null;
            sash = null;
            worn = wear;
            seenChecked = seenInside = seenDrawn = seenHeld = 0;
            seenPictures = seenPinched = seenResets = 0;
            if (!wear)
            {
                return;
            }

            robe = Robe();
            Hang(robe);
        }

        /// <summary>
        /// The robe, cut as one cloth of 36 columns by 24 rows: from its left front edge, round his back, to its right
        /// front edge; from a collar on his shoulders to a little above his ankles. Its top row is held on the shoulders
        /// (the chest), its seventh row is pinned by the sash at his waist (the hips). Open in front: wide at the chest,
        /// nearly closed at the sash, opening again towards the hem so the legs come through. Between the collar and the
        /// sash it lies close on his trunk on a short leash; below the sash it hangs, and swings on a long one.
        /// </summary>
        private Garment Robe()
        {
            const int C = 36, R = 24, S = 6;
            var g = new Garment
            {
                Name = "robe", Columns = C, Rows = R, Sash = S, Net = new ClothNet(C * R),
                OnChest = new Vector3[C * R], OnHips = new Vector3[C * R], ChestShare = new float[C * R],
            };
            float s = size;
            var chest = FrameOf(PartId.Chest);
            var hips = FrameOf(PartId.Pelvis);
            // (Rows above the sash in the chest's own space, from the collar down; the sash and below in the hips'.)
            for (int r = 0; r < R; r++)
            {
                bool above = r < S;
                float open, y, rx, rz;
                if (above)
                {
                    float k = r / (float)S; // 0 at the collar, toward 1 at the sash
                    // (The collar lies round the base of his neck, inside his shoulder joints (21 cm out from his
                    // chest's middle on a Medium), so the arms come out beside it through the armholes. Below it the
                    // cloth goes round his chest's box (31 by 18 cm) outside its corners: a rounded square, not an
                    // ellipse, or the box's corners stand inside the cloth and push its points apart.)
                    open = Mathf.Lerp(30f, 10f, k);
                    rx = (r == 0 ? 0.14f : Mathf.Lerp(0.19f, 0.18f, k)) * s;
                    rz = (r == 0 ? 0.105f : Mathf.Lerp(0.12f, 0.135f, k)) * s;
                    y = Mathf.Lerp(0.205f, -0.24f, k) * s; // chest space: the collar on the shoulders down toward the sash
                }
                else
                {
                    float k = (r - S) / (float)(R - 1 - S); // 0 at the sash, 1 at the hem
                    open = Mathf.Lerp(6f, 28f, Mathf.Sqrt(k));
                    rx = Mathf.Lerp(0.18f, 0.23f, Mathf.Sqrt(k)) * s;
                    rz = Mathf.Lerp(0.135f, 0.2f, Mathf.Sqrt(k)) * s;
                    y = Mathf.Lerp(0.07f, -0.86f, k) * s; // hips space: the sash down to the hem
                }

                for (int c = 0; c < C; c++)
                {
                    // (From the left front edge, round his back, to the right front edge; 0 degrees is his front.)
                    float deg = Deg(c, open);
                    float a = deg * Mathf.Deg2Rad;
                    float sx = Mathf.Sin(a), cz = Mathf.Cos(a);
                    if (above && r > 0)
                    {
                        sx = Mathf.Sign(sx) * Mathf.Sqrt(Mathf.Abs(sx));
                        cz = Mathf.Sign(cz) * Mathf.Sqrt(Mathf.Abs(cz));
                    }

                    var local = new Vector3(sx * rx, y, cz * rz);
                    int i = r * C + c;
                    Vector3 world = above ? chest.Of(local) : hips.Of(local);
                    g.Net.X[i] = world;
                    g.OnChest[i] = chest.Into(world);
                    g.OnHips[i] = hips.Into(world);
                    g.ChestShare[i] = above ? 1f - r / (float)S : 0f;
                }
            }

            Weave(g);
            for (int i = 0; i < g.Net.Count; i++)
            {
                int r = i / C;
                g.Net.Free[i] = r == 0 || r == S ? 0f : 1f;
                g.Net.Leash[i] = r < S ? 0.05f * s : (0.05f + Leash * (r - S) / (float)(R - 1 - S)) * s;
            }

            return g;
        }

        private const int PanelColumns = 9; // each front panel's columns; the back has the rest

        /// <summary>
        /// Where column <paramref name="c"/> goes round him (degrees from his front): the left front panel from the
        /// opening to his side (90), the back from side to side, the right front panel from his other side (270) to the
        /// opening; so the two sides fall between the same columns on every row, and the armholes can be cut there.
        /// </summary>
        private static float Deg(int c, float open)
        {
            const int C = 36, F = PanelColumns;
            if (c < F)
            {
                return open + (88f - open) * c / (F - 1f);
            }

            if (c >= C - F)
            {
                return 272f + (360f - open - 272f) * (c - (C - F)) / (F - 1f);
            }

            return 92f + 176f * (c - F) / (C - 2f * F - 1f);
        }

        /// <summary>True for a thread across the armhole (above the sash, between a front panel and the back at his side).</summary>
        private static bool AcrossArmhole(Garment g, int r, int c0, int c1)
        {
            if (r >= g.Sash)
            {
                return false;
            }

            int lo = Mathf.Min(c0, c1), hi = Mathf.Max(c0, c1);
            return (lo < PanelColumns && hi >= PanelColumns) || (lo < g.Columns - PanelColumns && hi >= g.Columns - PanelColumns);
        }

        /// <summary>The threads of an open sheet: across and down (kept from passing through a limb), both diagonals, and every other point across and down to bend against (soft).</summary>
        private static void Weave(Garment g)
        {
            int C = g.Columns, R = g.Rows;
            var net = g.Net;
            int At(int r, int c) => r * C + c;
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    // (No sleeves yet: the arms come out through an armhole at each side, from the collar down to the
                    // sash; no thread crosses it, or the arm hanging in the robe would stretch it.)
                    if (c + 1 < C && !AcrossArmhole(g, r, c, c + 1))
                    {
                        net.Join(At(r, c), At(r, c + 1), 2);
                    }

                    if (r + 1 < R)
                    {
                        net.Join(At(r, c), At(r + 1, c), 2);
                        if (c + 1 < C && !AcrossArmhole(g, r, c, c + 1))
                        {
                            net.Join(At(r, c), At(r + 1, c + 1));
                            net.Join(At(r, c + 1), At(r + 1, c));
                        }
                    }

                    if (c + 2 < C && !AcrossArmhole(g, r, c, c + 2))
                    {
                        net.Join(At(r, c), At(r, c + 2), 1);
                    }

                    if (r + 2 < R)
                    {
                        net.Join(At(r, c), At(r + 2, c), 1);
                    }
                }
            }

            // (The mesh: two sides, each its own triangles, so either is lit and seen.)
            g.Tris.Clear();
            int n = net.Count;
            for (int r = 0; r + 1 < R; r++)
            {
                for (int c = 0; c + 1 < C; c++)
                {
                    if (AcrossArmhole(g, r, c, c + 1))
                    {
                        continue;
                    }

                    int a = At(r, c), b = At(r, c + 1), d = At(r + 1, c), e = At(r + 1, c + 1);
                    g.Tris.AddRange(new[] { a, d, b, b, d, e });
                    g.Tris.AddRange(new[] { n + a, n + b, n + d, n + b, n + e, n + d });
                }
            }
        }

        /// <summary>The robe put back to hanging: every point at its home place on him, still.</summary>
        private void Hang(Garment g)
        {
#if !HEADLESS_STUB
            g.Run.Drop();
#endif
            var net = g.Net;
            var chest = FrameOf(PartId.Chest);
            var hips = FrameOf(PartId.Pelvis);
            for (int i = 0; i < net.Count; i++)
            {
                net.X[i] = HomeOf(g, i, chest, hips);
                net.Home[i] = net.X[i];
            }

            net.Settle();
            net.Forget();
            net.Reset = () => Hang(g);
            g.Was = hips;
            g.Known = true;
        }

        // ---- Drawn ----------------------------------------------------------------------------------------------------

        private GameObject sash;
        private Mesh sashMesh;
        private bool sashTrisSet;
        private readonly List<Vector3> sashVerts = new List<Vector3>();
        private readonly List<int> sashTris = new List<int>();

        private void Draw(Garment g)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (g.Drawn == null)
            {
                g.Drawn = new GameObject("Clothes: " + g.Name);
                if (owner != null)
                {
                    g.Drawn.transform.SetParent(owner, false);
                }

                g.Mesh = new Mesh();
                g.Mesh.MarkDynamic();
                g.Drawn.AddComponent<MeshFilter>().sharedMesh = g.Mesh;
                g.Drawn.AddComponent<MeshRenderer>().sharedMaterial = Cloth(side);
                g.TrisSet = false;
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
            if (!g.TrisSet)
            {
                g.Mesh.SetTriangles(g.Tris, 0);
                g.TrisSet = true;
            }

            g.Mesh.RecalculateNormals();
            g.Mesh.RecalculateBounds();
            DrawSash(g);
        }

        /// <summary>The sash: a band round the robe's sash row, a little outside it, 6 cm high.</summary>
        private void DrawSash(Garment g)
        {
            if (sash == null)
            {
                sash = new GameObject("Clothes: sash");
                if (owner != null)
                {
                    sash.transform.SetParent(owner, false);
                }

                sashMesh = new Mesh();
                sashMesh.MarkDynamic();
                sashTrisSet = false;
                sash.AddComponent<MeshFilter>().sharedMesh = sashMesh;
                sash.AddComponent<MeshRenderer>().sharedMaterial = Cloth(2);
                sashTris.Clear();
                const int N = 24;
                for (int c = 0; c < N; c++)
                {
                    int a = c * 2, b = ((c + 1) % N) * 2;
                    sashTris.AddRange(new[] { a, a + 1, b, b, a + 1, b + 1 });
                    sashTris.AddRange(new[] { 2 * N + a, 2 * N + b, 2 * N + a + 1, 2 * N + b, 2 * N + b + 1, 2 * N + a + 1 });
                }
            }

            var hips = FrameOf(PartId.Pelvis);
            sashVerts.Clear();
            float s = size;
            for (int pass = 0; pass < 2; pass++)
            {
                for (int c = 0; c < 24; c++)
                {
                    float a = 2f * Mathf.PI * c / 24f;
                    float rx = (0.18f + 0.02f) * s, rz = (0.135f + 0.02f) * s;
                    Vector3 lo = hips.Of(new Vector3(Mathf.Sin(a) * rx, 0.04f * s, Mathf.Cos(a) * rz));
                    Vector3 hi = hips.Of(new Vector3(Mathf.Sin(a) * rx, 0.1f * s, Mathf.Cos(a) * rz));
                    sashVerts.Add(owner != null ? sash.transform.InverseTransformPoint(lo) : lo);
                    sashVerts.Add(owner != null ? sash.transform.InverseTransformPoint(hi) : hi);
                }
            }

            sashMesh.SetVertices(sashVerts);
            if (!sashTrisSet)
            {
                sashMesh.SetTriangles(sashTris, 0);
                sashTrisSet = true;
            }

            sashMesh.RecalculateNormals();
            sashMesh.RecalculateBounds();
        }

        private static readonly Material[] cloth = new Material[3];

        /// <summary>The cloth's look: the body's own material, coloured for his side (2: the sash).</summary>
        private Material Cloth(int which)
        {
            which = which == 2 ? 2 : which == 1 ? 1 : 0;
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
            Color colour = which == 2 ? SashColour : Colours[which];
            m.color = colour;
            m.SetColor("_BaseColor", colour);
            m.SetFloat("_Smoothness", 0.15f);
            cloth[which] = m;
            return m;
        }

        /// <summary>His robe taken off, its picture in flight dropped, its meshes gone (his unit is going).</summary>
        public void Release()
        {
            Make(false);
            outfit?.Release();
            outfit = null;
            sewn?.Release();
            sewn = null;
        }

        /// <summary>Every point of the robe, with its garment's name, for a picture drawn from the numbers.</summary>
        public IEnumerable<(string, Vector3)> AllPoints()
        {
            if (robe == null)
            {
                yield break;
            }

            for (int i = 0; i < robe.Net.Count; i++)
            {
                yield return (i / robe.Columns == robe.Sash ? "sash" : robe.Name, robe.Net.X[i]);
            }
        }

        /// <summary>The robe's own measure: points, pictures pinched, the most stretch, point-steps inside, by part.</summary>
        public string Told()
        {
            if (robe == null)
            {
                return "no robe";
            }

            var n = robe.Net;
            // (The thread stretched most now: which rows and columns it joins, and its kind.)
            float worst = 0f;
            string where = "";
            for (int k = 0; k < n.Threads; k++)
            {
                int a = n.ThreadEnds[k], b = n.ThreadEnds[n.Threads + k];
                float rest = n.Packed[n.LengthsAt + k];
                float now = rest > 1e-5f ? Vector3.Distance(n.X[a], n.X[b]) / rest : 0f;
                if (now > worst)
                {
                    worst = now;
                    where = string.Format(System.Globalization.CultureInfo.InvariantCulture, " (now the most: {0:0.00} times, row {1} column {2} to row {3} column {4}, kind {5}, rest {6:0.0} cm)",
                        now, a / robe.Columns, a % robe.Columns, b / robe.Columns, b % robe.Columns, n.ThreadKinds[k], 100f * rest);
                }
            }

            return where + string.Format(System.Globalization.CultureInfo.InvariantCulture, "robe: {0} points, pinched in {1} pictures, stretched {2:0}% at most, inside {3} of {4}, the deepest {5:0.0} cm, put back {6}",
                n.Count, n.Pinched, 100f * n.MostStretch, n.Inside, n.Checked, 100f * n.Deepest, n.Resets);
        }
    }
}
