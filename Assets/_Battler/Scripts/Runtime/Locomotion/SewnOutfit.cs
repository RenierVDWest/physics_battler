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
        [Serializable] private class GarmentData { public string name; public string[] pieces; public float[] x; public int[] piece; public int[] tris; public int[] threads; public float[] threadLength; public int[] bends; public float[] bendLength; public float[] heldFrom; }
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

        // ---- The moving cloth (step 4; Combat Feel pushes these) --------------------------------------------------

        public const float DefaultWeight = 1f, DefaultAir = 0.93f, DefaultInertia = 0.5f, DefaultSquash = 0.2f, DefaultBend = 0.05f;
        public const float DefaultHemsSwing = 0.10f, DefaultFencePerMetre = 0.20f, DefaultHold = 0.03f;
        public const int DefaultSteps = 4, DefaultSweeps = 4;

        /// <summary>How much of the way back to the drape's shape the cloth is drawn each picture: more holds it, less lets it swing and lie where it was thrown.</summary>
        public static float HoldShape = DefaultHold;

        /// <summary>On (new): the cloth moves by itself on the body (swings, hangs, folds). Off (old): carried exactly (step 3).</summary>
        public static bool Moves = true;

        /// <summary>How heavy the cloth hangs: 1 about cotton (gravity times this).</summary>
        public static float Weight = DefaultWeight;

        /// <summary>The share of its own speed (against his body) the cloth keeps each picture (at 60 a second): less is calmer, more swings on longer.</summary>
        public static float Air = DefaultAir;

        /// <summary>How much of his own movement the cloth feels (0: carried along with him; 1: all of it, as loose cloth would): a leap leaves the hems behind for a moment.</summary>
        public static float Inertia = DefaultInertia;

        /// <summary>How hard a thread resists being squashed (a share of the way back to its length each sweep; stretching is always taken back whole): less, and spare cloth folds more readily.</summary>
        public static float Squash = DefaultSquash;

        /// <summary>How hard the cloth resists folding (the bends' share each sweep).</summary>
        public static float Bend = DefaultBend;

        /// <summary>The most any point may be from its carried place (metres): the hems and cuffs, the furthest along the cloth from where it is held.</summary>
        public static float HemsSwing = DefaultHemsSwing;

        /// <summary>Each point's reach grows by this much for every metre along the cloth from where it is held (the collar and shoulder seams; the waistband), up to <see cref="HemsSwing"/>.</summary>
        public static float FencePerMetre = DefaultFencePerMetre;

        /// <summary>Small steps a picture, and sweeps over the threads each small step.</summary>
        public static int Steps = DefaultSteps, Sweeps = DefaultSweeps;

        public static string Counted() => Pictures == 0 ? "none worn" : string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0} pictures; points inside {1} of {2}; thread middles {3} of {4}; triangle centres {5} of {6}; deepest {7:0.0} mm; pushed {8}, still in after the push {9}; folds (body pressed into itself) {10}; exact check {11} of {12} ({13} pictures, folds {14}); points through (a limb out through the cloth) {17}; {15:0.00} ms a picture{16}",
            Pictures, PointsIn, PointsChecked, MiddlesIn, MiddlesChecked, CentresIn, CentresChecked, 1000f * Deepest, Pushed, StillIn, Folded,
            ExactIn, ExactChecked, ExactPictures, ExactFolded, CostMs / Mathf.Max(1, Pictures), LastIn.Length > 0 ? "; last in: " + LastIn : "", Through);

        /// <summary>Cloth points measured further in than his skin along their own way out by more than 1 cm (a limb come out through the cloth).</summary>
        public static long Through;

        public static void Forget()
        {
            Pictures = PointsChecked = PointsIn = MiddlesChecked = MiddlesIn = CentresChecked = CentresIn = Pushed = StillIn = Folded = Through = 0;
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
        private Vector3[] r = new Vector3[0], rPrev = new Vector3[0], homeWas = new Vector3[0], homeWas2 = new Vector3[0];
        private float[] threadRest = new float[0], bendRest = new float[0], fence = new float[0];
        private int[] bends = new int[0];
        private bool resetNext = true, movedLast;
        private float dt = 1f / 60f;
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

            int total = 0, totalThreads = 0, totalTris = 0, totalBends = 0;
            foreach (var g in file.garments)
            {
                total += g.x.Length / 3;
                totalThreads += g.threads.Length / 2;
                totalTris += g.tris.Length / 3;
                totalBends += g.bends != null ? g.bends.Length / 2 : 0;
            }

            r = new Vector3[total];
            rPrev = new Vector3[total];
            homeWas = new Vector3[total];
            homeWas2 = new Vector3[total];
            fence = new float[total];
            threadRest = new float[totalThreads];
            bends = new int[2 * totalBends];
            bendRest = new float[totalBends];
            int atBend = 0;

            x = new Vector3[total];
            home = new Vector3[total];
            pieceOf = new string[total];
            restX = new Vector3[total];
            tuckW = new float[total];
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
                    restX[at + i] = p;
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

                    sk.GA = Quaternion.Inverse(restRot[(int)best]) * Vector3.down;
                    skins[at + i] = sk;
                }

                for (int t = 0; t < g.threads.Length; t++)
                {
                    threads[2 * atThread + t] = at + g.threads[t];
                }

                for (int t = 0; t < g.threads.Length / 2; t++)
                {
                    threadRest[atThread + t] = g.threadLength != null && t < g.threadLength.Length ? g.threadLength[t] : 0.02f;
                }

                int nb = g.bends != null ? g.bends.Length / 2 : 0;
                for (int t = 0; t < 2 * nb; t++)
                {
                    bends[2 * atBend + t] = at + g.bends[t];
                }

                for (int t = 0; t < nb; t++)
                {
                    bendRest[atBend + t] = g.bendLength != null && t < g.bendLength.Length ? g.bendLength[t] : 0.03f;
                }

                // (Each point's reach: by how far along the cloth it is from where the garment is held; none in the
                // band the sash gathers.)
                for (int i = 0; i < n; i++)
                {
                    string piece = g.pieces[g.piece[i]];
                    float held = g.heldFrom != null && i < g.heldFrom.Length ? g.heldFrom[i] : 0f;
                    float y = g.x[3 * i + 1];
                    bool underSash = file.sash != null && piece.StartsWith("shirt_", StringComparison.Ordinal) && y > file.sash.lo - 0.01f && y < file.sash.hi + 0.01f;
                    fence[at + i] = underSash ? 0f : held;
                }

                for (int t = 0; t < g.tris.Length; t++)
                {
                    tris[3 * atTri + t] = at + g.tris[t];
                }

                meshes[k] = new OutfitSolve.MeshPiece
                {
                    Start = at, Count = n, ThreadStart = atThread, ThreadCount = g.threads.Length / 2, TriStart = atTri, TriCount = g.tris.Length / 3, Own = own,
                    BendStart = atBend, BendCount = nb,
                };
                garments.Add(made);
                at += n;
                atThread += g.threads.Length / 2;
                atTri += g.tris.Length / 3;
                atBend += nb;
            }

            stats = new OutfitSolve.Stats[meshes.Length];
            // (Each point's distance along the cloth from the nearest point that is held (reach 0: the neck band, the
            // shoulder seams, the waistband, the band under the sash), along the threads at their flat lengths, so the
            // reach grows smoothly away from every hold: a point just below the sash may not hang far from it.)
            for (int sweep = 0; sweep < 400; sweep++)
            {
                bool changed = false;
                for (int t = 0; t < threads.Length / 2; t++)
                {
                    int a = threads[2 * t], b = threads[2 * t + 1];
                    float l = threadRest[t];
                    if (fence[a] + l < fence[b] - 1e-5f) { fence[b] = fence[a] + l; changed = true; }
                    if (fence[b] + l < fence[a] - 1e-5f) { fence[a] = fence[b] + l; changed = true; }
                }

                if (!changed)
                {
                    break;
                }
            }

            // (The hems and cuffs, for the measures: the points of each piece furthest along the cloth from where it is
            // held.)
            hemOf = new bool[total];
            var mostReach = new Dictionary<string, float>();
            for (int i = 0; i < total; i++)
            {
                mostReach.TryGetValue(pieceOf[i], out float m);
                mostReach[pieceOf[i]] = Mathf.Max(m, fence[i]);
            }

            for (int i = 0; i < total; i++)
            {
                hemOf[i] = fence[i] > 0.1f && fence[i] >= 0.9f * mostReach[pieceOf[i]];
            }

            Layers();

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
            Dynamic = Moves ? 1 : 0, Steps = Mathf.Clamp(Steps, 1, 16), Iters = Mathf.Clamp(Sweeps, 1, 8), Dt = dt, Reset = resetNext ? 1 : 0,
            Gravity = 9.81f * Mathf.Max(0f, Weight), Keep = Mathf.Clamp01(Air), Inertia = Mathf.Clamp01(Inertia), Squash = Mathf.Clamp01(Squash), BendK = Mathf.Clamp01(Bend),
            HoldShape = Mathf.Clamp01(HoldShape), Pluck = pluckNext, Backstop = Backstop ? 1 : 0, Tucked = TuckUnderShirt ? 1 : 0, CollideEvery = Mathf.Clamp(PushEvery, 1, 8), Longest = Mathf.Max(1f, LongestThread),
        };

        /// <summary>On (new, 19:30): no moving cloth point goes further in toward his body than his skin along its own way out, so a limb cannot come out through the far side of its sleeve or trouser leg. Off: only pushed out of him the nearest way.</summary>
        public static bool Backstop = true;

        /// <summary>On (new, 19:30): the trousers under the shirt are drawn close to his body (the shirt covers them) and the shirt over them kept 6 mm further off, so the trousers never show through the shirt. Off: both where the drape left them.</summary>
        public static bool TuckUnderShirt = true;

        /// <summary>The moving cloth: his body pushes it out every this many small steps, and on the last (1: every small step; 2 since 19:45, half the cost of the push).</summary>
        public static int PushEvery = 2;

        /// <summary>The moving cloth: after all else, any thread longer than this share of its flat length is drawn in (so none shows as a spike).</summary>
        public static float LongestThread = 1.2f;

        private Vector3 pluckNext;

        /// <summary>Throws every free point of the clothes forward of his hips (as far as its fence allows) in the next picture, to see how the cloth swings and settles ("Pluck the cloth" in the lab).</summary>
        public void Pluck()
        {
            pluckNext = frames[(int)PartId.Pelvis].R * Vector3.forward * 0.8f;
        }

        /// <summary>Each point's reach this picture (metres at scale 1): its distance from where it is held times <see cref="FencePerMetre"/>, at most <see cref="HemsSwing"/>.</summary>
        private void Fences()
        {
            for (int i = 0; i < fence.Length; i++)
            {
                fenceNow[i] = Mathf.Min(Mathf.Max(0f, HemsSwing), fence[i] * Mathf.Max(0f, FencePerMetre)) * (TuckUnderShirt && tuckW.Length == fence.Length ? 1f - 0.75f * tuckW[i] : 1f);
            }
        }

        private float[] fenceNow = new float[0];
        private bool[] hemOf = new bool[0];
        private Vector3[] restX = new Vector3[0];
        private float[] tuckW = new float[0];

        /// <summary>
        /// Each point's way out of his body at rest and its backstop; and which trousers are under the shirt (tucked close
        /// to him, not swinging) and which shirt is over trousers (kept 6 mm further off). A trousers point is under the
        /// shirt where the nearest shirt point is within 5 cm and further off his body; wholly so 7 cm or more above the
        /// shirt's hem (along the cloth), not at all within 2 cm of it.
        /// </summary>
        private void Layers()
        {
            int n = restX.Length;
            var d0 = new float[n];
            for (int i = 0; i < n; i++)
            {
                Vector3 p = restX[i];
                PartId m = PartId.Pelvis;
                float best = 1e9f;
                foreach (var id in Outfit.Covered)
                {
                    float dd = RestDistance(id, p);
                    if (dd < best)
                    {
                        best = dd;
                        m = id;
                    }
                }

                const float h = 0.002f;
                var grad = new Vector3(
                    RestDistance(m, p + new Vector3(h, 0f, 0f)) - RestDistance(m, p - new Vector3(h, 0f, 0f)),
                    RestDistance(m, p + new Vector3(0f, h, 0f)) - RestDistance(m, p - new Vector3(0f, h, 0f)),
                    RestDistance(m, p + new Vector3(0f, 0f, h)) - RestDistance(m, p - new Vector3(0f, 0f, h)));
                Vector3 nw = grad.sqrMagnitude > 1e-12f ? grad.normalized : Vector3.up;
                d0[i] = best;
                var sk = skins[i];
                sk.NA = Quaternion.Inverse(restRot[sk.A]) * nw;
                sk.Back = Mathf.Max(0f, best - Outfit.PointMargin);
                skins[i] = sk;
            }

            Garment shirt = null, trousers = null;
            foreach (var g in garments)
            {
                if (g.Name == "shirt") shirt = g;
                if (g.Name == "trousers") trousers = g;
            }

            if (shirt == null || trousers == null)
            {
                return;
            }

            // (The shirt's hem: the furthest along the cloth below the sash.)
            float hem = 0f;
            for (int i = shirt.Start; i < shirt.Start + shirt.Count; i++)
            {
                if (file.sash == null || restX[i].y < file.sash.lo)
                {
                    hem = Mathf.Max(hem, fence[i]);
                }
            }

            const float cell = 0.05f;
            long Key(Vector3 v) => ((long)Mathf.FloorToInt(v.x / cell) + 1000) * 4000000L + ((long)Mathf.FloorToInt(v.y / cell) + 1000) * 2000L + (Mathf.FloorToInt(v.z / cell) + 1000);
            Dictionary<long, List<int>> Bins(Garment g)
            {
                var bins = new Dictionary<long, List<int>>();
                for (int i = g.Start; i < g.Start + g.Count; i++)
                {
                    long k = Key(restX[i]);
                    if (!bins.TryGetValue(k, out var list))
                    {
                        bins[k] = list = new List<int>();
                    }

                    list.Add(i);
                }

                return bins;
            }

            int Nearest(Dictionary<long, List<int>> bins, Vector3 p)
            {
                int found = -1;
                float bestSq = cell * cell;
                int cx = Mathf.FloorToInt(p.x / cell), cy = Mathf.FloorToInt(p.y / cell), cz = Mathf.FloorToInt(p.z / cell);
                for (int a = -1; a <= 1; a++)
                for (int b = -1; b <= 1; b++)
                for (int c = -1; c <= 1; c++)
                {
                    long k = ((long)(cx + a) + 1000) * 4000000L + ((long)(cy + b) + 1000) * 2000L + (cz + c + 1000);
                    if (!bins.TryGetValue(k, out var list))
                    {
                        continue;
                    }

                    foreach (int j in list)
                    {
                        float sq = (restX[j] - p).sqrMagnitude;
                        if (sq < bestSq)
                        {
                            bestSq = sq;
                            found = j;
                        }
                    }
                }

                return found;
            }

            float Under(int s) => Mathf.Clamp01((hem - fence[s] - 0.02f) / 0.05f);
            var shirtBins = Bins(shirt);
            var trouserBins = Bins(trousers);
            TuckedPoints = 0;
            for (int t = trousers.Start; t < trousers.Start + trousers.Count; t++)
            {
                int s = Nearest(shirtBins, restX[t]);
                if (s < 0 || d0[s] <= d0[t] + 0.002f)
                {
                    continue;
                }

                float w = Under(s);
                tuckW[t] = w;
                var sk = skins[t];
                sk.Tuck = w * Mathf.Max(0f, d0[t] - Outfit.PointMargin - 0.001f);
                skins[t] = sk;
                TuckedPoints += w > 0.5f ? 1 : 0;
            }

            for (int s = shirt.Start; s < shirt.Start + shirt.Count; s++)
            {
                int t = Nearest(trouserBins, restX[s]);
                if (t < 0 || d0[t] >= d0[s] - 0.002f)
                {
                    continue;
                }

                var sk = skins[s];
                sk.Extra = 0.006f * Under(s);
                skins[s] = sk;
            }
        }

        /// <summary>How many trousers points are tucked under the shirt (more than half).</summary>
        public int TuckedPoints { get; private set; }

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
        public int[] BendArray => bends;
        public float[] ThreadRestArray => threadRest;
        public float[] BendRestArray => bendRest;
        public float[] FenceArray => fenceNow;
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
            fixed (Vector3* rr = r)
            fixed (Vector3* rp = rPrev)
            fixed (Vector3* hw = homeWas)
            fixed (Vector3* hw2 = homeWas2)
            fixed (float* trs = threadRest)
            fixed (float* brs = bendRest)
            fixed (float* fc = fenceNow)
            fixed (int* bds = bends)
            fixed (float* dn = dNear)
            fixed (byte* mv = moved)
            fixed (byte* ins = insideFlags)
            fixed (OutfitSolve.Stats* st = stats)
            {
                var d = Settings(measure);
                d.Grid = g; d.Fields = pf; d.Frames = fr0; d.Skins = sk; d.Meshes = mp; d.Threads = th; d.Tris = tr; d.AdjStart = ast; d.Adj = ad;
                d.X = xs; d.Home = hm; d.Shove = sv; d.DNear = dn;
                d.R = rr; d.RPrev = rp; d.HomeWas = hw; d.HomeWas2 = hw2; d.ThreadRest = trs; d.BendRest = brs; d.Fence = fc; d.Bends = bds; d.Moved = mv; d.Inside = ins; d.Stats = st;
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
        public void Place(float dt)
        {
            if (x.Length == 0)
            {
                return;
            }

            var clock = System.Diagnostics.Stopwatch.StartNew();
            this.dt = Mathf.Clamp(dt > 0f ? dt : 1f / 60f, 1f / 240f, 1f / 15f);
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

            resetNext = true; // (a pose by hand, unrelated to the last: nothing carried over)
            Run(clock);
            resetNext = true;
        }

        /// <summary>
        /// Unity: the picture begun (the frames in, the garments given to the worker threads) without waiting, so both men's
        /// clothes are worked out at once while the game goes on; <see cref="PlaceEnd"/> waits, copies out and draws.
        /// </summary>
        public void PlaceBegin(float dt)
        {
            if (x.Length == 0)
            {
                return;
            }

            var clock = System.Diagnostics.Stopwatch.StartNew();
            this.dt = Mathf.Clamp(dt > 0f ? dt : 1f / 60f, 1f / 240f, 1f / 15f);
            Frames();
            Begin();
            mainMs = (float)clock.Elapsed.TotalMilliseconds;
        }

        /// <summary>The picture begun by <see cref="PlaceBegin"/> waited for, copied out and drawn.</summary>
        public void PlaceEnd()
        {
            var clock = System.Diagnostics.Stopwatch.StartNew();
            End(clock, mainMs);
        }

        private void Run(System.Diagnostics.Stopwatch clock)
        {
            Begin();
            End(clock, 0f);
        }

        private bool begun, threadedNow, measureNow;
        private float mainMs;

        private void Begin()
        {
            FillFrames();
            if (fenceNow.Length != fence.Length)
            {
                fenceNow = new float[fence.Length];
            }

            Fences();
            if (Moves != movedLast)
            {
                resetNext = true;
                movedLast = Moves;
            }

            bool measure = runs++ % Mathf.Max(1, MeasureEvery) == 0;
            bool threaded = false;
#if !HEADLESS_STUB
            threaded = Outfit.OnOtherCores && !jobFailed;
            if (threaded)
            {
                // (If the job cannot run, say so once and work it out here, so the clothes are still drawn.)
                try
                {
                    job.Start(this, measure);
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

            begun = true;
            threadedNow = threaded;
            measureNow = measure;
        }

        private void End(System.Diagnostics.Stopwatch clock, float before)
        {
            if (!begun)
            {
                return;
            }

            begun = false;
            bool threaded = threadedNow, measure = measureNow;
#if !HEADLESS_STUB
            if (threaded)
            {
                try
                {
                    job.Finish(this);
                }
                catch (Exception e)
                {
                    jobFailed = true;
                    Debug.LogError("[Clothes] the sewn clothes' job on other cores failed; worked out on the main thread from now on: " + e);
                    RunHere(measure);
                }
            }
#endif
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
                CentresChecked += st.CentresChecked; CentresIn += st.CentresIn; Pushed += st.Pushed; StillIn += st.StillIn; Folded += st.Folded; Through += st.Crossed;
                Deepest = Mathf.Max(Deepest, st.Deepest);
                if (st.LastInPart >= 0)
                {
                    LastIn = garments[k].Name + (st.LastInKind == 2 ? " (a middle or a centre)" : "") + " in the " + (PartId)st.LastInPart;
                }
            }

            resetNext = false;
            pluckNext = Vector3.zero;
            Shivers();
            ExactCheck();
            RanOnOtherCores = threaded;
            LastMs = before + (float)clock.Elapsed.TotalMilliseconds;
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
                // (One matrix for the garment, not a walk up his hierarchy for every point.)
                Matrix4x4 toLocal = owner != null ? g.Drawn.transform.worldToLocalMatrix : Matrix4x4.identity;
                for (int i = g.Start; i < g.Start + g.Count; i++)
                {
                    g.Verts.Add(toLocal.MultiplyPoint3x4(x[i]));
                }

                for (int i = 0; i < g.Count; i++)
                {
                    g.Verts.Add(g.Verts[i]);
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
            begun = false;
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

        private Vector3[] offWas = new Vector3[0];

        /// <summary>How far the cloth moved against his body this picture, on average (metres): under a millimetre when he stands still, or it shivers.</summary>
        public float Shiver { get; private set; }

        private void Shivers()
        {
            if (offWas.Length != x.Length)
            {
                offWas = new Vector3[x.Length];
            }

            double sum = 0;
            for (int i = 0; i < x.Length; i++)
            {
                Vector3 off = x[i] - home[i];
                sum += (off - offWas[i]).magnitude;
                offWas[i] = off;
            }

            Shiver = x.Length > 0 ? (float)(sum / x.Length) : 0f;
        }

        /// <summary>
        /// What the moving cloth is doing now, in numbers: how far its points are from their carried places (all; and the
        /// hems and cuffs, those whose reach is 8 cm or more, with how far they hang below them), the shiver, and the most
        /// stretched thread against its flat length.
        /// </summary>
        public (float all, float ends, float hang, float shiver, float stretch) Motion()
        {
            double sumAll = 0, sumEnds = 0, hang = 0;
            int nEnds = 0;
            for (int i = 0; i < x.Length; i++)
            {
                Vector3 off = x[i] - home[i];
                sumAll += off.magnitude;
                if (hemOf.Length == x.Length && hemOf[i])
                {
                    sumEnds += off.magnitude;
                    hang += -off.y;
                    nEnds++;
                }
            }

            float most = 0f;
            for (int k = 0; k < threadRest.Length; k++)
            {
                float rest = threadRest[k] * size;
                if (rest > 1e-5f)
                {
                    most = Mathf.Max(most, (x[threads[2 * k + 1]] - x[threads[2 * k]]).magnitude / rest);
                }
            }

            return ((float)(x.Length > 0 ? sumAll / x.Length : 0), (float)(nEnds > 0 ? sumEnds / nEnds : 0), (float)(nEnds > 0 ? hang / nEnds : 0), Shiver, most);
        }

        /// <summary>For the lab: the cloth's distance from where it would be carried, its shiver, and its threads against their flat lengths, this picture.</summary>
        public string MovingReport()
        {
            if (x.Length == 0)
            {
                return "no cloth";
            }

            var mo = Motion();
            int over = 0, n = 0;
            for (int k = 0; k < threadRest.Length; k++)
            {
                float rest = threadRest[k] * size;
                if (rest > 1e-5f)
                {
                    n++;
                    over += (x[threads[2 * k + 1]] - x[threads[2 * k]]).sqrMagnitude > 1.1025f * rest * rest ? 1 : 0;
                }
            }

            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}; from where it would be carried {1:0.0} cm, the hems and cuffs {2:0.0} cm; shiver {3:0.0} mm a picture; threads over 105%: {4} of {5}, the longest {6:0}%",
                Moves ? "moving" : "carried (old)", 100f * mo.all, 100f * mo.ends, 1000f * mo.shiver, over, n, 100f * mo.stretch);
        }

        /// <summary>The longest threads this picture against their flat lengths, as drawn and as carried, with their pieces and parts (for the headless trace).</summary>
        public string LongestThreads(int count)
        {
            var order = new List<(float, int)>();
            for (int k = 0; k < threadRest.Length; k++)
            {
                float rest = threadRest[k] * size;
                if (rest > 1e-5f)
                {
                    order.Add(((x[threads[2 * k + 1]] - x[threads[2 * k]]).magnitude / rest, k));
                }
            }

            order.Sort((p, q) => q.Item1.CompareTo(p.Item1));
            int over5 = 0, over20 = 0;
            foreach (var o in order)
            {
                over5 += o.Item1 > 1.05f ? 1 : 0;
                over20 += o.Item1 > 1.2f ? 1 : 0;
            }

            var sb = new System.Text.StringBuilder();
            sb.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture, "over 105%: {0} of {1}, over 120%: {2}; ", over5, order.Count, over20));
            for (int j = 0; j < Mathf.Min(count, order.Count); j++)
            {
                int k = order[j].Item2, a = threads[2 * k], b = threads[2 * k + 1];
                float rest = threadRest[k] * size;
                sb.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}-{1} {2:0}% (carried {3:0}%, {4:0.0} cm flat; parts {5}/{6}w{7:0.00} and {8}/{9}w{10:0.00}, reach {11:0.00}/{12:0.00}; off home {13:0.0}/{14:0.0} cm, depth {15:0.0}/{16:0.0} cm); ",
                    pieceOf[a], pieceOf[b], 100f * order[j].Item1, 100f * (home[b] - home[a]).magnitude / rest, 100f * rest,
                    (PartId)skins[a].A, (PartId)skins[a].B, skins[a].W, (PartId)skins[b].A, (PartId)skins[b].B, skins[b].W, fence[a], fence[b],
                    100f * (x[a] - home[a]).magnitude, 100f * (x[b] - home[b]).magnitude, dNear.Length > a ? 100f * dNear[a] : 0f, dNear.Length > b ? 100f * dNear[b] : 0f));
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

            var mo = Motion();
            return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                "sewn, {0}; {1}; {2} points; from its places on him on average {3:0.0} cm, the most {4:0.0} cm; the hems and cuffs {5:0.0} cm, hanging {6:0.0} cm; shiver {7:0.0} mm a picture; the longest thread {8:0}% of its length; the last word pushed {9} times; inside at the last measure {10}; {11:0.00} ms",
                Moves ? "moving" : "carried", RanOnOtherCores ? "other cores" : "main thread", x.Length, 100f * mo.all, 100f * Mathf.Sqrt(most), 100f * mo.ends, 100f * mo.hang,
                1000f * mo.shiver, 100f * mo.stretch, pushed, inside.Count, LastMs);
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
