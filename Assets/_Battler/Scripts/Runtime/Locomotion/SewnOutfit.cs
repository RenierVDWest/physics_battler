using System;
using System.Collections.Generic;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The shirt and trousers cut and sewn like real clothes, carried on the body as drawn (the plan "Clothes cut and
    /// sewn like real clothes", step 3; 22 November, his "ok this looks fine, proceed").
    ///
    /// The garments were cut from flat pattern pieces, sewn and draped once on the drawn body, headless
    /// (<c>Tools/Cloth/pattern.py</c> and <c>drape.py</c>), into <c>Resources/Locomotion/Garments_Medium.json</c>: one
    /// welded mesh a garment, its points in the hips' space of the rest pose. Here each point is carried by one or two
    /// body parts (the nearest of the parts its piece may ride: a sleeve its arm, the shirt's body the trunk, a trouser
    /// leg its thigh and shin), blended smoothly where two are near, so the cloth keeps the folds of the drape and moves
    /// exactly with him, never a picture late. Then the last word, as the tubes have it: every point, thread middle and
    /// triangle centre still inside the drawn body is pushed straight out. No cloth moves by itself yet (step 4).
    /// </summary>
    public sealed class SewnOutfit
    {
        // ---- The file --------------------------------------------------------------------------------------------

        [Serializable] private class SashData { public float lo, hi, a, b, cz; }
        [Serializable] private class GarmentData { public string name; public string[] pieces; public float[] x; public int[] piece; public int[] tris; public int[] threads; public float[] threadLength; public float[] heldFrom; }
        [Serializable] private class RestPart { public string name; public float[] at; }
        [Serializable] private class FileData { public string body; public float pelvisY; public SashData sash; public RestPart[] restParts; public GarmentData[] garments; }

        private static readonly Dictionary<string, FileData> files = new Dictionary<string, FileData>();

        private static FileData FileOf(string body)
        {
            if (files.TryGetValue(body, out var have))
            {
                return have;
            }

            FileData made = null;
            string text = LocoLibrary.ReadText("Locomotion/Garments_" + body, "_Battler/Resources/Locomotion/Garments_" + body + ".json");
            if (!string.IsNullOrEmpty(text))
            {
                made = JsonUtility.FromJson<FileData>(text);
                if (made != null && (made.garments == null || made.garments.Length == 0))
                {
                    made = null;
                }
            }

            files[body] = made;
            return made;
        }

        // ---- Measures (all men since the scene began) ------------------------------------------------------------

        public static long Pictures, PointsChecked, PointsIn, MiddlesChecked, MiddlesIn, CentresChecked, CentresIn, Pushed, StillIn, Folded;
        public static long ExactChecked, ExactIn, ExactFolded, ExactPictures;
        public static float Deepest, CostMs;
        public static string LastIn = "";

        /// <summary>The headless exact check runs every this many pictures (0: never). Unity: never (it is slow).</summary>
        public static int ExactEvery = 20;

        /// <summary>The fast measure is taken every this many pictures (it costs about as much again as the cloth).</summary>
        public static int MeasureEvery = 4;

        /// <summary>Rounds of thread middles and triangle centres pushed out a picture, at most.</summary>
        public static int Rounds = 6;

        /// <summary>Within this of a covered part, the second-nearest part a point may ride shares it (metres): 0 there, half where both are as near.</summary>
        public static float BlendOver = 0.04f;

        /// <summary>On (new): a point shared by two parts that meet at a joint is carried round the joint at its own distance from it (as the tubes' joint balls), so a bent elbow or knee does not draw the cloth into him. Off: the two places blended straight (it sinks in at a bend).</summary>
        public static bool JointBalls = true;

        /// <summary>On (new): the shirt below the sash rides the thighs it hangs over as well as his hips (a lifted knee lifts the hem). Off: his hips alone, and a thigh comes through the hem for the last word to push out.</summary>
        public static bool ShirtRidesThighs = true;

        public static string Counted() => Pictures == 0 ? "none worn" : string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0} pictures; points inside {1} of {2}; thread middles {3} of {4}; triangle centres {5} of {6}; deepest {7:0.0} mm; pushed {8}, still in after the push {9}; folds (body pressed into itself) {10}; exact check {11} of {12} ({13} pictures, folds {14}); {15:0.00} ms a picture{16}",
            Pictures, PointsIn, PointsChecked, MiddlesIn, MiddlesChecked, CentresIn, CentresChecked, 1000f * Deepest, Pushed, StillIn, Folded,
            ExactIn, ExactChecked, ExactPictures, ExactFolded, CostMs / Mathf.Max(1, Pictures), LastIn.Length > 0 ? "; last in: " + LastIn : "");

        public static void Forget()
        {
            Pictures = PointsChecked = PointsIn = MiddlesChecked = MiddlesIn = CentresChecked = CentresIn = Pushed = StillIn = Folded = 0;
            ExactChecked = ExactIn = ExactFolded = ExactPictures = 0;
            Deepest = CostMs = 0f;
            LastIn = "";
        }

        // ---- One man's clothes -----------------------------------------------------------------------------------

        private sealed class Garment
        {
            public string Name;
            public int Start, Count, ThreadStart, ThreadCount, TriStart, TriCount;
            public int[] Tris;
            public GameObject Drawn;
            public Mesh Mesh;
            public readonly List<Vector3> Verts = new List<Vector3>();
            public bool TrisSet;
        }

        private readonly UnitRig rig;
        private readonly ShockWave wave;
        private readonly BodyFields fields;
        private readonly float size;
        private readonly int side;
        private readonly Transform owner;
        private readonly List<Garment> garments = new List<Garment>();
        private FileData file;

        private readonly Vector3[] restPos = new Vector3[PartInfo.Count];
        private readonly Quaternion[] restRot = new Quaternion[PartInfo.Count];
        private readonly Vector3[] fp = new Vector3[PartInfo.Count];
        private readonly Quaternion[] fr = new Quaternion[PartInfo.Count];
        private readonly Quaternion[] fi = new Quaternion[PartInfo.Count];
        private readonly Vector3[] bc = new Vector3[PartInfo.Count];
        private readonly float[] br = new float[PartInfo.Count];

        private short[] grid = new short[0];
        private readonly OutfitSolve.PartField[] partFields = new OutfitSolve.PartField[OutfitSolve.Parts];
        private readonly OutfitSolve.Frame[] frames = new OutfitSolve.Frame[OutfitSolve.Parts];
        private OutfitSolve.Skin[] skins = new OutfitSolve.Skin[0];
        private OutfitSolve.MeshPiece[] meshes = new OutfitSolve.MeshPiece[0];
        private OutfitSolve.Stats[] stats = new OutfitSolve.Stats[0];
        private int[] threads = new int[0], tris = new int[0], adjStart = new int[1], adj = new int[0];
        private Vector3[] x = new Vector3[0], home = new Vector3[0], shove = new Vector3[0];
        private float[] dNear = new float[0];
        private byte[] moved = new byte[0], insideFlags = new byte[0];
        private readonly List<int> inside = new List<int>();
        private int runs, exactCount;
        private bool jobFailed;
#if !HEADLESS_STUB
        private readonly SewnRun job = new SewnRun();
#endif

        /// <summary>Cloth points in all (both garments).</summary>
        public int Points => x.Length;

        /// <summary>What the start-up check found: the baked body against the meshes Unity loaded, and his rest pose against the drape's.</summary>
        public string Check { get; private set; } = "";

        /// <summary>Did the last picture run on other cores (true), or here?</summary>
        public bool RanOnOtherCores { get; private set; }

        /// <summary>The main thread's time for the last picture (ms).</summary>
        public float LastMs { get; private set; }

        /// <summary>Points that were inside the drawn body at the last measure (for the x-ray and the report).</summary>
        public int InsideNow => inside.Count;

        private SewnOutfit(UnitRig rig, ShockWave wave, BodyFields fields, float size, int side, Transform owner)
        {
            this.rig = rig;
            this.wave = wave;
            this.fields = fields;
            this.size = Mathf.Max(0.3f, size);
            this.side = side;
            this.owner = owner;
        }

        /// <summary>The sewn clothes put on this body, or null if no baked body or draped garments of its size are there.</summary>
        public static SewnOutfit Make(UnitRig rig, ShockWave wave, float size, int side, Transform owner)
        {
            var fields = BodyFields.For("Medium");
            var data = FileOf("Medium");
            if (fields == null || data == null || rig == null)
            {
                return null;
            }

            var made = new SewnOutfit(rig, wave, fields, size, side, owner) { file = data };
            float restOff = made.Put();
            made.Check = fields.CheckAgainst(rig, made.size, Outfit.Covered)
                + string.Format(System.Globalization.CultureInfo.InvariantCulture, "; sewn clothes {0} points; his rest pose against the drape's: {1:0.0} mm at most", made.x.Length, 1000f * restOff);
            return made;
        }

        // ---- Put on: each point's parts and its places on them --------------------------------------------------

        /// <summary>The parts' places at rest, in the hips' space, at scale 1 (as <see cref="Outfit"/> has them).</summary>
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
            return f == null ? 10f : f.Distance(Quaternion.Inverse(restRot[(int)p]) * (at - restPos[(int)p]));
        }

        /// <summary>The parts a piece's points may ride (the nearest one or two of them carry each point).</summary>
        private static PartId[] RidesOn(string piece)
        {
            switch (piece)
            {
                case "shirt_front":
                case "shirt_back": return new[] { PartId.Pelvis, PartId.Spine, PartId.Chest, PartId.UpperArmR, PartId.UpperArmL, PartId.ThighR, PartId.ThighL };
                case "sleeve_R": return new[] { PartId.UpperArmR, PartId.ForearmR, PartId.Chest };
                case "sleeve_L": return new[] { PartId.UpperArmL, PartId.ForearmL, PartId.Chest };
                case "neck_band": return new[] { PartId.Chest };
                case "leg_front_R":
                case "leg_back_R": return new[] { PartId.Pelvis, PartId.ThighR, PartId.ShinR };
                case "leg_front_L":
                case "leg_back_L": return new[] { PartId.Pelvis, PartId.ThighL, PartId.ShinL };
                default: return new[] { PartId.Pelvis };
            }
        }

        /// <summary>
        /// How near a point is to a part it may ride, at rest. The shirt's body rides the thighs only below the sash. A shoulder only counts near its joint: the shirt's body
        /// rides an upper arm within 8 cm of the shoulder joint (the armhole goes with the arm, the side stays with the
        /// trunk); a sleeve rides the chest within 10 cm of it (the sleeve's top over the shoulder).
        /// </summary>
        private float RideDistance(string piece, PartId p, Vector3 at)
        {
            // (The shirt below the sash rides the thighs it hangs over, partly where his hips are as near, so a lifted
            // knee carries the hem up with it instead of coming through it.)
            if ((p == PartId.ThighR || p == PartId.ThighL) && (!ShirtRidesThighs || file.sash == null || at.y > file.sash.lo))
            {
                return 10f;
            }

            bool arm = p == PartId.UpperArmR || p == PartId.UpperArmL;
            if (arm && piece.StartsWith("shirt_", StringComparison.Ordinal) && (at - restPos[(int)p]).sqrMagnitude > 0.08f * 0.08f)
            {
                return 10f;
            }

            if (p == PartId.Chest && piece.StartsWith("sleeve", StringComparison.Ordinal))
            {
                var shoulder = piece.EndsWith("R", StringComparison.Ordinal) ? PartId.UpperArmR : PartId.UpperArmL;
                if ((at - restPos[(int)shoulder]).sqrMagnitude > 0.10f * 0.10f)
                {
                    return 10f;
                }
            }

            return RestDistance(p, at);
        }

        /// <summary>Every garment's points, threads and triangles taken from the file; each point given its parts; the solver's arrays made. Returns how far his rest pose is from the drape's (metres).</summary>
        private float Put()
        {
            Rest();
            float restOff = 0f;
            if (file.restParts != null)
            {
                foreach (var rp in file.restParts)
                {
                    if (rp.at != null && rp.at.Length == 3 && PartInfo.TryParse(rp.name, out var id))
                    {
                        restOff = Mathf.Max(restOff, (restPos[(int)id] - new Vector3(rp.at[0], rp.at[1], rp.at[2])).magnitude);
                    }
                }
            }

            int total = 0, totalThreads = 0, totalTris = 0;
            foreach (var g in file.garments)
            {
                total += g.x.Length / 3;
                totalThreads += g.threads.Length / 2;
                totalTris += g.tris.Length / 3;
            }

            x = new Vector3[total];
            home = new Vector3[total];
            pieceOf = new string[total];
            shove = new Vector3[total];
            skins = new OutfitSolve.Skin[total];
            threads = new int[2 * totalThreads];
            tris = new int[3 * totalTris];
            dNear = new float[total];
            moved = new byte[total];
            insideFlags = new byte[total];
            int at = 0, atThread = 0, atTri = 0;
            int own = 0;
            foreach (var id in Outfit.Covered)
            {
                own |= 1 << (int)id;
            }

            meshes = new OutfitSolve.MeshPiece[file.garments.Length];
            for (int k = 0; k < file.garments.Length; k++)
            {
                var g = file.garments[k];
                int n = g.x.Length / 3;
                var made = new Garment
                {
                    Name = g.name, Start = at, Count = n, ThreadStart = atThread, ThreadCount = g.threads.Length / 2,
                    TriStart = atTri, TriCount = g.tris.Length / 3, Tris = g.tris,
                };
                for (int i = 0; i < n; i++)
                {
                    var p = new Vector3(g.x[3 * i], g.x[3 * i + 1], g.x[3 * i + 2]);
                    string piece = g.pieces[g.piece[i]];
                    pieceOf[at + i] = piece;
                    PartId best = PartId.Pelvis, next = PartId.Pelvis;
                    float d1 = 1e9f, d2 = 1e9f;
                    foreach (var part in RidesOn(piece))
                    {
                        if (fields.Of(part) == null)
                        {
                            continue;
                        }

                        float d = RideDistance(piece, part, p);
                        if (d < d1)
                        {
                            d2 = d1; next = best;
                            d1 = d; best = part;
                        }
                        else if (d < d2)
                        {
                            d2 = d; next = part;
                        }
                    }

                    // (Smooth across a joint: half each where both parts are as near, none of the second BlendOver further on.)
                    float w = d2 < 9f ? 0.5f * Mathf.Clamp01(1f - (d2 - d1) / Mathf.Max(1e-4f, BlendOver)) : 0f;
                    var sk = new OutfitSolve.Skin
                    {
                        A = (int)best, B = (int)next, W = w, J = -1,
                        HA = Quaternion.Inverse(restRot[(int)best]) * (p - restPos[(int)best]),
                        HB = Quaternion.Inverse(restRot[(int)next]) * (p - restPos[(int)next]),
                    };
                    // (Two parts that meet at a joint: carried round the joint at its own distance from it.)
                    PartId joint = PartInfo.HasParent(next) && PartInfo.Parent(next) == best ? next
                        : PartInfo.HasParent(best) && PartInfo.Parent(best) == next ? best : (PartId)(-1);
                    if (w > 0f && (int)joint >= 0 && JointBalls)
                    {
                        Vector3 j = restPos[(int)joint];
                        sk.J = (int)joint;
                        sk.R = (p - j).magnitude;
                        sk.OA = Quaternion.Inverse(restRot[(int)best]) * (p - j);
                        sk.OB = Quaternion.Inverse(restRot[(int)next]) * (p - j);
                    }

                    skins[at + i] = sk;
                }

                for (int t = 0; t < g.threads.Length; t++)
                {
                    threads[2 * atThread + t] = at + g.threads[t];
                }

                for (int t = 0; t < g.tris.Length; t++)
                {
                    tris[3 * atTri + t] = at + g.tris[t];
                }

                meshes[k] = new OutfitSolve.MeshPiece
                {
                    Start = at, Count = n, ThreadStart = atThread, ThreadCount = g.threads.Length / 2, TriStart = atTri, TriCount = g.tris.Length / 3, Own = own,
                };
                garments.Add(made);
                at += n;
                atThread += g.threads.Length / 2;
                atTri += g.tris.Length / 3;
            }

            stats = new OutfitSolve.Stats[meshes.Length];
            // (Each point's neighbours along its threads, for the smoothing.)
            adjStart = new int[total + 1];
            for (int t = 0; t < threads.Length; t++)
            {
                adjStart[threads[t] + 1]++;
            }

            for (int i = 0; i < total; i++)
            {
                adjStart[i + 1] += adjStart[i];
            }

            adj = new int[threads.Length];
            var fill = (int[])adjStart.Clone();
            for (int t = 0; t < threads.Length; t += 2)
            {
                adj[fill[threads[t]]++] = threads[t + 1];
                adj[fill[threads[t + 1]]++] = threads[t];
            }

            int cells = 0;
            foreach (var id in PartInfo.All)
            {
                var f = fields.Of(id);
                if (f != null)
                {
                    cells += f.Grid.Length;
                }
            }

            grid = new short[cells];
            int c0 = 0;
            foreach (var id in PartInfo.All)
            {
                var f = fields.Of(id);
                if (f == null)
                {
                    continue;
                }

                Array.Copy(f.Grid, 0, grid, c0, f.Grid.Length);
                partFields[(int)id] = new OutfitSolve.PartField { Offset = c0, Nx = f.Nx, Ny = f.Ny, Nz = f.Nz, Has = 1, Cell = f.Cell, Bound = f.Bound, Min = f.Min, Centre = f.Centre };
                c0 += f.Grid.Length;
            }

            return restOff;
        }

        // ---- Each picture ----------------------------------------------------------------------------------------

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

        private void FillFrames()
        {
            foreach (var id in PartInfo.All)
            {
                int p = (int)id;
                var f = fields.Of(id);
                bool there = f != null && Array.IndexOf(Outfit.Covered, id) >= 0 && !rig.IsSevered(id);
                bc[p] = f != null ? fp[p] + fr[p] * (f.Centre * size) : fp[p];
                br[p] = there ? f.Bound * size : -1f;
                frames[p] = new OutfitSolve.Frame { P = fp[p], R = fr[p], I = fi[p], BC = bc[p], BR = br[p] };
            }
        }

        /// <summary>The settings of a picture for the solver (the pointers are filled in by whoever runs it).</summary>
        public OutfitSolve.Data Settings(bool measure) => new OutfitSolve.Data
        {
            Size = size, PointMargin = Outfit.PointMargin, MiddleMargin = Outfit.MiddleMargin, OtherMargin = Outfit.OtherMargin,
            LastWord = Outfit.LastWordOn ? 1 : 0, Rounds = Mathf.Max(0, Rounds), Measure = measure ? 1 : 0,
            KeepMost = KeepMost ? 1 : 0, RadialOut = RadialOut ? 1 : 0, Smooth = Mathf.Max(0, Smooths), FoldsStay = FoldsStay ? 1 : 0,
        };

        /// <summary>On: cloth inside a crease where his body is pressed into itself (inside one part and within 1 cm of another: an elbow bent shut) stays there, folded in, and is counted as a fold; off: pushed out of it, which made flaps.</summary>
        public static bool FoldsStay = true;

        /// <summary>Passes of smoothing over the last word's pushes, before its rounds (0: none; tried at 2 and 3 with no gain once folds stay folded).</summary>
        public static int Smooths;

        /// <summary>On: a point keeps the biggest push a thread middle or triangle centre gave it in a round (and at most 1 cm). Off: it takes them all, added.</summary>
        public static bool KeepMost;

        /// <summary>On: a point carried round a joint, inside him, is pushed straight out from the joint first (at most 4 cm). Off by default: it moved points further than the plain push did, 15 to 17 cm against 14 in "Shake him", for about as much inside.</summary>
        public static bool RadialOut;

        // (For the job: the arrays as they are.)
        public short[] GridArray => grid;
        public OutfitSolve.PartField[] PartFieldArray => partFields;
        public OutfitSolve.Frame[] FrameArray => frames;
        public OutfitSolve.Skin[] SkinArray => skins;
        public OutfitSolve.MeshPiece[] MeshArray => meshes;
        public int[] ThreadArray => threads;
        public int[] TriArray => tris;
        public int[] AdjStartArray => adjStart;
        public int[] AdjArray => adj;
        public Vector3[] PointArray => x;
        public Vector3[] HomeArray => home;
        public OutfitSolve.Stats[] StatsArray => stats;
        public byte[] InsideArray => insideFlags;

        /// <summary>The sums here, on this thread (headless, or with "on other cores" off).</summary>
        public unsafe void RunHere(bool measure)
        {
            fixed (short* g = grid)
            fixed (OutfitSolve.PartField* pf = partFields)
            fixed (OutfitSolve.Frame* fr0 = frames)
            fixed (OutfitSolve.Skin* sk = skins)
            fixed (OutfitSolve.MeshPiece* mp = meshes)
            fixed (int* th = threads)
            fixed (int* tr = tris)
            fixed (int* ast = adjStart)
            fixed (int* ad = adj)
            fixed (Vector3* xs = x)
            fixed (Vector3* hm = home)
            fixed (Vector3* sv = shove)
            fixed (float* dn = dNear)
            fixed (byte* mv = moved)
            fixed (byte* ins = insideFlags)
            fixed (OutfitSolve.Stats* st = stats)
            {
                var d = Settings(measure);
                d.Grid = g; d.Fields = pf; d.Frames = fr0; d.Skins = sk; d.Meshes = mp; d.Threads = th; d.Tris = tr; d.AdjStart = ast; d.Adj = ad;
                d.X = xs; d.Home = hm; d.Shove = sv; d.DNear = dn; d.Moved = mv; d.Inside = ins; d.Stats = st;
                for (int k = 0; k < meshes.Length; k++)
                {
                    OutfitSolve.Mesh1(ref d, k);
                }
            }
        }

        /// <summary>
        /// The picture: every point to its home on the body as drawn now; then the last word; then the checks; then the
        /// drawing. In Unity it is called as the picture is about to be drawn (LateUpdate); headless after each step.
        /// </summary>
        public void Place()
        {
            if (x.Length == 0)
            {
                return;
            }

            var clock = System.Diagnostics.Stopwatch.StartNew();
            Frames();
            Run(clock);
        }

        /// <summary>The same picture on a body posed by hand (the torture rack): each part's place and turn given.</summary>
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

            Run(clock);
        }

        private void Run(System.Diagnostics.Stopwatch clock)
        {
            FillFrames();
            bool measure = runs++ % Mathf.Max(1, MeasureEvery) == 0;
            bool threaded = false;
#if !HEADLESS_STUB
            threaded = Outfit.OnOtherCores && !jobFailed;
            if (threaded)
            {
                // (If the job cannot run, say so once and work it out here, so the clothes are still drawn.)
                try
                {
                    job.Run(this, measure);
                }
                catch (Exception e)
                {
                    jobFailed = true;
                    threaded = false;
                    Debug.LogError("[Clothes] the sewn clothes' job on other cores failed; worked out on the main thread from now on: " + e);
                }
            }
#endif
            if (!threaded)
            {
                RunHere(measure);
            }

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
                CentresChecked += st.CentresChecked; CentresIn += st.CentresIn; Pushed += st.Pushed; StillIn += st.StillIn; Folded += st.Folded;
                Deepest = Mathf.Max(Deepest, st.Deepest);
                if (st.LastInPart >= 0)
                {
                    LastIn = garments[k].Name + (st.LastInKind == 2 ? " (a middle or a centre)" : "") + " in the " + (PartId)st.LastInPart;
                }
            }

            ExactCheck();
            RanOnOtherCores = threaded;
            LastMs = (float)clock.Elapsed.TotalMilliseconds;
            CostMs += LastMs;
            Pictures++;
            Draw();
        }

        // ---- The exact check (headless) --------------------------------------------------------------------------

        private float DistanceTo(PartId id, Vector3 at)
        {
            int p = (int)id;
            return br[p] < 0f ? 10f : fields.Of(id).Distance(fi[p] * (at - fp[p]) / size) * size;
        }

        private void ExactCheck()
        {
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
            for (int i = 0; i < x.Length; i++)
            {
                Exact(x[i]);
            }

            for (int k = 0; k < tris.Length; k += 3)
            {
                Exact((x[tris[k]] + x[tris[k + 1]] + x[tris[k + 2]]) * (1f / 3f));
            }
        }

        private void Exact(Vector3 at)
        {
            ExactChecked++;
            foreach (var id in Outfit.Covered)
            {
                var f = fields.Of(id);
                if (f == null || br[(int)id] < 0f || (at - bc[(int)id]).sqrMagnitude > br[(int)id] * br[(int)id])
                {
                    continue;
                }

                if (f.InsideExactly(fi[(int)id] * (at - fp[(int)id]) / size))
                {
                    bool fold = false;
                    foreach (var other in Outfit.Covered)
                    {
                        if (other != id && DistanceTo(other, at) < 0.01f)
                        {
                            fold = true;
                            break;
                        }
                    }

                    if (fold)
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

        // ---- The torture rack ------------------------------------------------------------------------------------

        /// <summary>A body posed at random within each joint's limits: the parts' places and turns, the hips at <paramref name="at"/> (as <see cref="Outfit.RandomPose"/>).</summary>
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

        // ---- Drawn -----------------------------------------------------------------------------------------------

        private static readonly Material[] cloth = new Material[5];
        private GameObject sash, xray;
        private Mesh sashMesh, xrayMesh;
        private readonly List<Vector3> sashVerts = new List<Vector3>();

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
            Color colour = which == 4 ? Outfit.Sash : which >= 2 ? Outfit.Trousers[which - 2] : Outfit.Shirt[which];
            m.color = colour;
            m.SetColor("_BaseColor", colour);
            m.SetFloat("_Smoothness", 0.1f);
            cloth[which] = m;
            return m;
        }

        private GameObject NewDrawn(string name, Mesh mesh, Material material)
        {
            var go = new GameObject(name);
            if (owner != null)
            {
                go.transform.SetParent(owner, false);
            }

            mesh.MarkDynamic();
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            return go;
        }

        private Vector3 Local(GameObject go, Vector3 p) => owner != null ? go.transform.InverseTransformPoint(p) : p;

        private void Draw()
        {
#if HEADLESS_STUB
            return;
#else
            if (!Application.isPlaying)
            {
                return;
            }

            foreach (var g in garments)
            {
                if (g.Drawn == null)
                {
                    g.Mesh = new Mesh();
                    bool trousers = g.Name == "trousers";
                    g.Drawn = NewDrawn("Clothes: " + g.Name + " (sewn)", g.Mesh, Cloth((trousers ? 2 : 0) + (side == 1 ? 1 : 0)));
                    g.TrisSet = false;
                }

                // (Both sides: each point twice, the second set's triangles turned the other way.)
                g.Verts.Clear();
                for (int pass = 0; pass < 2; pass++)
                {
                    for (int i = g.Start; i < g.Start + g.Count; i++)
                    {
                        g.Verts.Add(Local(g.Drawn, x[i]));
                    }
                }

                g.Mesh.SetVertices(g.Verts);
                if (!g.TrisSet)
                {
                    var both = new int[2 * g.Tris.Length];
                    for (int k = 0; k < g.Tris.Length; k += 3)
                    {
                        both[k] = g.Tris[k]; both[k + 1] = g.Tris[k + 1]; both[k + 2] = g.Tris[k + 2];
                        int o = g.Tris.Length;
                        both[o + k] = g.Count + g.Tris[k]; both[o + k + 1] = g.Count + g.Tris[k + 2]; both[o + k + 2] = g.Count + g.Tris[k + 1];
                    }

                    g.Mesh.SetTriangles(both, 0);
                    g.TrisSet = true;
                }

                g.Mesh.RecalculateNormals();
                g.Mesh.RecalculateBounds();
            }

            DrawSash();
            DrawXRay();
#endif
        }

        private const int SashColumns = 72;

        /// <summary>The sash: a band round the gathered shirt at the waist, carried by his hips.</summary>
        private void DrawSash()
        {
            var s = file.sash;
            if (s == null || s.a <= 0f)
            {
                return;
            }

            if (sash == null)
            {
                sashMesh = new Mesh();
                sash = NewDrawn("Clothes: sash (sewn)", sashMesh, Cloth(4));
                var tri = new List<int>();
                for (int j = 0; j < SashColumns; j++)
                {
                    int j1 = (j + 1) % SashColumns;
                    tri.AddRange(new[] { j, j1, SashColumns + j1, j, SashColumns + j1, SashColumns + j });
                    int o = 2 * SashColumns;
                    tri.AddRange(new[] { o + j, o + SashColumns + j1, o + j1, o + j, o + SashColumns + j, o + SashColumns + j1 });
                }

                sashVerts.Clear();
                for (int k = 0; k < 4 * SashColumns; k++)
                {
                    sashVerts.Add(Vector3.zero);
                }

                sashMesh.SetVertices(sashVerts);
                sashMesh.SetTriangles(tri, 0);
            }

            int pel = (int)PartId.Pelvis;
            for (int pass = 0; pass < 2; pass++)
            {
                for (int row = 0; row < 2; row++)
                {
                    float y = row == 0 ? s.lo : s.hi;
                    for (int j = 0; j < SashColumns; j++)
                    {
                        float t = 2f * Mathf.PI * j / SashColumns;
                        var p = new Vector3((s.a + 0.006f) * Mathf.Cos(t), y, s.cz + (s.b + 0.006f) * Mathf.Sin(t));
                        sashVerts[pass * 2 * SashColumns + row * SashColumns + j] = Local(sash, fp[pel] + fr[pel] * (p * size));
                    }
                }
            }

            sashMesh.SetVertices(sashVerts);
            sashMesh.RecalculateNormals();
            sashMesh.RecalculateBounds();
        }

        private readonly List<Vector3> xrayVerts = new List<Vector3>();
        private readonly List<int> xrayLines = new List<int>();

        private void DrawXRay()
        {
            if (!Outfit.XRay && xray == null)
            {
                return;
            }

            if (xray == null)
            {
                xrayMesh = new Mesh();
                var shader = Shader.Find("Hidden/Internal-Colored");
                var m = new Material(shader != null ? shader : Shader.Find("Sprites/Default"));
                m.color = Color.red;
                m.SetInt("_ZTest", 8); // (always: drawn through everything)
                m.renderQueue = 5000;
                xray = NewDrawn("Clothes: x-ray (sewn)", xrayMesh, m);
                xray.transform.SetParent(null, false);
            }

            xray.SetActive(Outfit.XRay);
            xrayVerts.Clear();
            xrayLines.Clear();
            if (Outfit.XRay)
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

        /// <summary>The clothes taken off (their meshes gone).</summary>
        public void Release()
        {
            foreach (var g in garments)
            {
                Gone(g.Drawn);
            }

            Gone(sash);
            Gone(xray);
            garments.Clear();
            x = new Vector3[0];
#if !HEADLESS_STUB
            job.Drop();
#endif
        }

        private static void Gone(GameObject go)
        {
            if (go == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(go);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        // ---- What it is doing, for the logs ----------------------------------------------------------------------

        /// <summary>The parts as drawn this picture: place and turn.</summary>
        public (Vector3, Quaternion) PartFrame(PartId p) => (fp[(int)p], fr[(int)p]);

        /// <summary>Each garment's name and its points now.</summary>
        public IEnumerable<(string, Vector3[])> Garments()
        {
            foreach (var g in garments)
            {
                var pts = new Vector3[g.Count];
                Array.Copy(x, g.Start, pts, 0, g.Count);
                yield return (g.Name, pts);
            }
        }

        private string[] pieceOf = new string[0];

        /// <summary>The points the last word moved furthest from their places on him this picture: piece, parts, how far (for finding faults).</summary>
        public string Worst(int count)
        {
            var order = new List<int>();
            for (int i = 0; i < x.Length; i++)
            {
                order.Add(i);
            }

            order.Sort((a, b) => (x[b] - home[b]).sqrMagnitude.CompareTo((x[a] - home[a]).sqrMagnitude));
            var sb = new System.Text.StringBuilder();
            for (int k = 0; k < Mathf.Min(count, order.Count); k++)
            {
                int i = order[k];
                var sk = skins[i];
                float dh = 10f, dx = 10f;
                string inPart = "";
                foreach (var id in Outfit.Covered)
                {
                    float a = DistanceTo(id, home[i]);
                    if (a < dh) { dh = a; inPart = id.ToString(); }
                    dx = Mathf.Min(dx, DistanceTo(id, x[i]));
                }

                sb.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture, "[home {0:0.0} cm from {1}, now {2:0.0}] ", 100f * dh, inPart, 100f * dx));
                sb.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} {1:0.0} cm ({2}{3}); ", pieceOf[i], 100f * (x[i] - home[i]).magnitude,
                    (PartId)sk.A, sk.W > 0f ? "/" + (PartId)sk.B + (sk.J >= 0 ? " round " + (PartId)sk.J : "") + " " + sk.W.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) : ""));
            }

            return sb.ToString();
        }

        /// <summary>One line on what the clothes are doing this picture.</summary>
        public string Report()
        {
            if (x.Length == 0)
            {
                return "no cloth";
            }

            long pushed = 0;
            float most = 0f;
            foreach (var st in stats)
            {
                pushed += st.Pushed;
            }

            for (int i = 0; i < x.Length; i++)
            {
                most = Mathf.Max(most, (x[i] - home[i]).sqrMagnitude);
            }

            return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "sewn, carried; {0}; {1} points; the last word pushed {2} times, the most {3:0.0} cm off its place on him; inside at the last measure {4}; {5:0.00} ms",
                RanOnOtherCores ? "other cores" : "main thread", x.Length, pushed, 100f * Mathf.Sqrt(most), inside.Count, LastMs);
        }

        /// <summary>The whole cloth and the body's parts as drawn: SEWNPTS (each garment's points, in the file's order) and PARTFRAMES lines (Tools/Headless/sewn.py reads them).</summary>
        public IEnumerable<string> DumpLines(string at, string who)
        {
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            foreach (var (name, pts) in Garments())
            {
                var sb = new System.Text.StringBuilder();
                foreach (var p in pts)
                {
                    sb.Append(p.x.ToString("0.0000", inv)).Append(':').Append(p.y.ToString("0.0000", inv)).Append(':').Append(p.z.ToString("0.0000", inv)).Append(' ');
                }

                yield return $"SEWNPTS {at} {who} {name} {sb}";
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
