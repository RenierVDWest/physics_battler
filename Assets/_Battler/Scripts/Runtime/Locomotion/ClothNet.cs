using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// A piece of cloth (the plan "Cloth that moves", 21 November; rebuilt 22 November after his look): a net of points
    /// joined by threads, moved by gravity and its own speed, then put right several times over (<see cref="ClothSolve"/>).
    /// Its numbers live in one packed array, so a picture can be handed whole to a worker thread (Burst, <see
    /// cref="ClothJob"/>) and taken back a picture later, or run here directly (the headless tests, or with
    /// <see cref="Clothes.OnOtherCores"/> off).
    ///
    /// The packed array: X, Y, Z, the place before (BX, BY, BZ), the place at the start of the small step (SX, SY, SZ),
    /// the home places at the last picture and at this one (H0X..H1Z), Free, Leash (each <see cref="Count"/> long),
    /// then the threads' lengths, then the measures. The threads' ends: all the first ends, then all the second.
    /// </summary>
    public sealed class ClothNet
    {
        public readonly int Count;

        /// <summary>Where the points are (world), as the last finished picture left them. Its owner may set them and call <see cref="Settle"/>.</summary>
        public readonly Vector3[] X;

        /// <summary>0 for a pinned point; 1 for a free one.</summary>
        public readonly float[] Free;

        /// <summary>Where each point is pinned, or its home place for the leash (world, set each picture by its owner).</summary>
        public readonly Vector3[] Home;

        /// <summary>How far each point may stray from its home place (metres).</summary>
        public readonly float[] Leash;

        public const int Arrays = 17;

        /// <summary>The packed numbers (see the class's summary).</summary>
        public float[] Packed { get; private set; }

        public int[] ThreadEnds { get; private set; } = new int[0];

        public byte[] ThreadKinds { get; private set; } = new byte[0];

        public int Threads { get; private set; }

        private readonly List<(int, int, float, byte)> joins = new List<(int, int, float, byte)>();
        private bool laid, homeKnown;

        /// <summary>The body's shapes this picture (start and end of the picture).</summary>
        public readonly List<ClothSolve.Shape> Shapes = new List<ClothSolve.Shape>(20);

        /// <summary>Per part: 1 if not drawn (a point in it cannot be seen).</summary>
        public readonly byte[] Hidden = new byte[16];

        public float Floor, Gap = 0.02f, Thickness = 0.02f, Air = 0.6f, Stretch = 0.15f, Bending = 6f, Feel = 0.85f;
        public int Steps = 8;
        public Vector3 Gravity = new Vector3(0f, -9.81f, 0f), Wind;
        public float Weight = 1f;

        // ---- What it measured ------------------------------------------------------------------------------------------

        public long Checked, Inside, InsideDrawn;
        public int PicturesInsideDrawn, Pinched, Resets;

        /// <summary>Thread-steps held at <see cref="ClothSolve.TearsAt"/> times their length (the last word against tearing).</summary>
        public long Held;
        public float Deepest, MostStretch;
        public string WhyReset = "";

        /// <summary>Put back to hanging (its owner sets the points where they hang, then <see cref="Settle"/>).</summary>
        public System.Action Reset;

        public ClothNet(int count)
        {
            Count = count;
            X = new Vector3[count];
            Free = new float[count];
            Home = new Vector3[count];
            Leash = new float[count];
            for (int i = 0; i < count; i++)
            {
                Free[i] = 1f;
                Leash[i] = 0.3f;
            }
        }

        public int Of(int array) => array * Count;

        public int LengthsAt => Arrays * Count;

        public int StatsAt => Arrays * Count + Threads;

        /// <summary>A thread between two points at their distance now (in <see cref="X"/>). Kind 0 straight, 1 bending (soft), 2 straight and kept from passing through a limb.</summary>
        public void Join(int a, int b, byte kind = 0)
        {
            joins.Add((a, b, Vector3.Distance(X[a], X[b]), kind));
            laid = false;
        }

        private void Lay()
        {
            if (laid)
            {
                return;
            }

            Threads = joins.Count;
            ThreadEnds = new int[2 * Threads];
            ThreadKinds = new byte[Threads];
            var old = Packed;
            Packed = new float[Arrays * Count + Threads + ClothSolve.StatCount];
            if (old != null)
            {
                System.Array.Copy(old, Packed, Mathf.Min(old.Length, Arrays * Count));
            }

            for (int k = 0; k < Threads; k++)
            {
                ThreadEnds[k] = joins[k].Item1;
                ThreadEnds[Threads + k] = joins[k].Item2;
                Packed[LengthsAt + k] = joins[k].Item3;
                ThreadKinds[k] = joins[k].Item4;
            }

            laid = true;
        }

        /// <summary>Every point put where <see cref="X"/> has it, with no speed.</summary>
        public void Settle()
        {
            Lay();
            for (int i = 0; i < Count; i++)
            {
                Packed[Of(0) + i] = Packed[Of(3) + i] = Packed[Of(6) + i] = X[i].x;
                Packed[Of(1) + i] = Packed[Of(4) + i] = Packed[Of(7) + i] = X[i].y;
                Packed[Of(2) + i] = Packed[Of(5) + i] = Packed[Of(8) + i] = X[i].z;
            }
        }

        /// <summary>The pins' last places forgotten (the cloth was put back where they are now).</summary>
        public void Forget() => homeKnown = false;

        /// <summary>
        /// Made ready for a picture: the home places, the free and leash numbers, and a share of the body's move given to
        /// the cloth outright (<paramref name="carry"/>: the part it hangs from, moved from where it was to where it is;
        /// all but <see cref="Feel"/> of it is given, speed and all, so it does not feel all of his movement).
        /// </summary>
        public void Prepare(System.Func<Vector3, Vector3> carry)
        {
            Lay();
            var f = Packed;
            for (int i = 0; i < Count; i++)
            {
                f[Of(12) + i] = Home[i].x;
                f[Of(13) + i] = Home[i].y;
                f[Of(14) + i] = Home[i].z;
                f[Of(15) + i] = Free[i];
                f[Of(16) + i] = Leash[i];
            }

            if (!homeKnown)
            {
                System.Array.Copy(f, Of(12), f, Of(9), 3 * Count);
                homeKnown = true;
            }

            if (carry != null && Feel < 1f)
            {
                float give = 1f - Mathf.Clamp01(Feel);
                for (int i = 0; i < Count; i++)
                {
                    if (Free[i] <= 0f)
                    {
                        continue;
                    }

                    Vector3 p = new Vector3(f[Of(0) + i], f[Of(1) + i], f[Of(2) + i]), b = new Vector3(f[Of(3) + i], f[Of(4) + i], f[Of(5) + i]);
                    Vector3 to = carry(p), toB = carry(b);
                    p += give * (to - p);
                    b += give * (toB - b);
                    f[Of(0) + i] = p.x; f[Of(1) + i] = p.y; f[Of(2) + i] = p.z;
                    f[Of(3) + i] = b.x; f[Of(4) + i] = b.y; f[Of(5) + i] = b.z;
                }
            }

            for (int s = 0; s < ClothSolve.StatCount; s++)
            {
                f[StatsAt + s] = 0f;
            }
        }

        /// <summary>The settings of a picture, for the sums (the pointers are filled in by whoever runs them).</summary>
        public ClothSolve.Data Settings(float dt)
        {
            Vector3 g = Gravity + Wind / Mathf.Max(0.05f, Weight);
            return new ClothSolve.Data
            {
                Count = Count, Threads = Threads, Shapes = Shapes.Count, Steps = Mathf.Max(1, Steps),
                Dt = dt, Gap = Gap, Thickness = Thickness, Air = Air, Floor = Floor, Stretch = Stretch, Bending = Bending,
                GX = g.x, GY = g.y, GZ = g.z,
            };
        }

        /// <summary>The picture run here, on this thread.</summary>
        public unsafe void RunHere(float dt)
        {
            var d = Settings(dt);
            var shapes = Shapes.Count > 0 ? Shapes.ToArray() : new ClothSolve.Shape[1];
            var work = new ClothSolve.Now[shapes.Length];
            var ends = ThreadEnds.Length > 0 ? ThreadEnds : new int[2];
            var kinds = ThreadKinds.Length > 0 ? ThreadKinds : new byte[1];
            fixed (float* f = Packed)
            fixed (int* e = ends)
            fixed (byte* k = kinds)
            fixed (byte* hidden = Hidden)
            fixed (ClothSolve.Shape* sh = shapes)
            fixed (ClothSolve.Now* w = work)
            {
                Point(ref d, f, e, k, hidden, sh, w);
                ClothSolve.Step(ref d);
            }

            Finish();
        }

        /// <summary>The pointers of a picture set on its packed numbers.</summary>
        public static unsafe void Point(ref ClothSolve.Data d, float* f, int* ends, byte* kinds, byte* hidden, ClothSolve.Shape* shapes, ClothSolve.Now* work)
        {
            int n = d.Count;
            d.X = f; d.Y = f + n; d.Z = f + 2 * n;
            d.BX = f + 3 * n; d.BY = f + 4 * n; d.BZ = f + 5 * n;
            d.SX = f + 6 * n; d.SY = f + 7 * n; d.SZ = f + 8 * n;
            d.H0X = f + 9 * n; d.H0Y = f + 10 * n; d.H0Z = f + 11 * n;
            d.H1X = f + 12 * n; d.H1Y = f + 13 * n; d.H1Z = f + 14 * n;
            d.Free = f + 15 * n; d.Leash = f + 16 * n;
            d.TL = f + Arrays * n;
            d.Stats = f + Arrays * n + d.Threads;
            d.TA = ends;
            d.TB = ends + d.Threads;
            d.TKind = kinds;
            d.Hidden = hidden;
            d.Shape = shapes;
            d.Work = work;
        }

        /// <summary>A picture finished (here or on another core): the points read back, the homes moved on, what it measured added up, and the net under it all.</summary>
        public void Finish()
        {
            var f = Packed;
            for (int i = 0; i < Count; i++)
            {
                X[i] = new Vector3(f[Of(0) + i], f[Of(1) + i], f[Of(2) + i]);
            }

            System.Array.Copy(f, Of(12), f, Of(9), 3 * Count);
            int st = StatsAt;
            Checked += (long)f[st];
            Inside += (long)f[st + 1];
            InsideDrawn += (long)f[st + 2];
            Deepest = Mathf.Max(Deepest, f[st + 3]);
            MostStretch = Mathf.Max(MostStretch, f[st + 4]);
            Pinched += f[st + 5] > 0f ? 1 : 0;
            PicturesInsideDrawn += f[st + 7] > 0f ? 1 : 0;
            Held += (long)f[st + 8];
            if (f[st + 6] > 0f)
            {
                Resets++;
                WhyReset = f[st + 6] == 1f ? "a point lost (NaN)" : f[st + 6] == 2f ? "a point 2 m from its place" : "a thread stretched to nine times its length";
                Reset?.Invoke();
            }
        }
    }
}
