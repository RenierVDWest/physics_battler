using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The shirt and trousers' sums for one picture, one piece (tube) at a time, on plain numbers through pointers, so the
    /// same code runs compiled by Burst on Unity's worker threads, a piece to a thread (<see cref="OutfitJob"/>), and
    /// directly in the headless tests (22 November). For one piece: every point to its home on the body as drawn; the
    /// last word (out of the parts other pieces cover, as best it can; out of its own piece's parts, with the last
    /// word; then threads' middles and faces' centres, rounds of them); and the fast measure. See <see cref="Outfit"/>.
    /// </summary>
    public static unsafe class OutfitSolve
    {
        public const int Parts = 16;

        /// <summary>A sewn garment's point is moved at most this far by the thread middles and triangle centres in one round (metres).</summary>
        public const float ShoveMost = 0.01f;

        /// <summary>A sewn garment's point carried round a joint is pushed out straight from the joint at most this far (metres); if that is not enough, it goes back and the plain push out takes it.</summary>
        public const float RadialMost = 0.04f;

        /// <summary>One body part's baked field: where its grid sits in the shared array, and its shape.</summary>
        public struct PartField
        {
            public int Offset, Nx, Ny, Nz, Has;
            public float Cell, Bound;
            public Vector3 Min, Centre;
        }

        /// <summary>One body part as drawn this picture: its place, turn and the turn back, and its bounding sphere.</summary>
        public struct Frame
        {
            public Vector3 P, BC;
            public Quaternion R, I;
            public float BR; // negative: not there (no field, or cut off)
        }

        /// <summary>A cloth point's home: on one part (H, in its space), or on a joint's ball (DA, DB, R, W).</summary>
        public struct Anchor
        {
            public int Joint, A, B;
            public Vector3 H, DA, DB;
            public float R, W;
        }

        /// <summary>One piece: its points' place in the arrays, its rings and columns, its own parts and the others (bits).</summary>
        public struct Piece
        {
            public int Start, Rings, Cols, Own, Others, Smooths;
        }

        /// <summary>What one piece measured in one picture.</summary>
        public struct Stats
        {
            public long PointsChecked, PointsIn, MiddlesChecked, MiddlesIn, CentresChecked, CentresIn, Pushed, StillIn, Crossed, Folded;
            public float Deepest;
            public int LastInPart, LastInKind; // the part, and 1 a point, 2 a middle; -1 none
        }

        public struct Data
        {
            public short* Grid;
            public PartField* Fields;
            public Frame* Frames;
            public Anchor* Anchors;
            public Piece* Pieces;
            public Vector3* X, Home, Prev;
            public float* DNear, Sway, RestU, RestV;
            public byte* Moved, Inside;
            public Stats* Stats;
            public float Size, PointMargin, MiddleMargin, OtherMargin;
            public int LastWord, Rounds, Measure, Smooth;
            // (The wobble: on, the picture's time, how far a point may stray from its home at most (times its own
            // sway), its weight, how much of the way home it is drawn each picture, how much of its speed it loses,
            // the thread passes, and Reset: every point put home and stilled.)
            public int WobbleOn, Iters, Reset;
            public float Dt, Wobble, Weight, Hold, Damp;
            // (Sewn garments (<see cref="SewnOutfit"/>): each point carried by one or two parts, the garments, and their
            // threads (pairs of points) and triangles (triples), as places in the shared point arrays.)
            public Skin* Skins;
            public MeshPiece* Meshes;
            public int* Threads, Tris;
            public Vector3* Shove; // (a sewn garment's scratch: the biggest push each point got from a middle this round)
            public int KeepMost, RadialOut;
            public int* AdjStart, Adj; // (sewn: each point's neighbours along its threads, for smoothing the last word's pushes)
            public int FoldsStay; // (sewn: cloth inside a crease of his body pressed into itself (an elbow bent shut) stays there, hidden in the fold, not pushed out of it) // (sewn: a point keeps its biggest push from a middle (1) or adds them all (0); a point shared round a joint is pushed out straight from the joint (1))
        }

        /// <summary>A sewn garment's point carried by one or two parts: its places in their spaces (scale 1) and B's share.</summary>
        public struct Skin
        {
            public int A, B;
            public float W;
            public Vector3 HA, HB;
            // (Shared by two parts that meet at a joint (J, the one whose place is the joint): its way from the joint in
            // each part's space, OA and OB, turned with each and blended, at its own distance R from the joint, as the
            // tubes' joint balls have it, so a bent elbow does not draw the cloth in. J is -1 for a plain blend.)
            public int J;
            public Vector3 OA, OB;
            public float R;
        }

        /// <summary>One sewn garment: its points, its threads and triangles (where they start in Threads and Tris, and how many), the parts it is kept out of (bits).</summary>
        public struct MeshPiece
        {
            public int Start, Count, ThreadStart, ThreadCount, TriStart, TriCount, Own;
        }

        // ---- The body's fields -------------------------------------------------------------------------------------

        private static float Cellv(ref Data d, PartField* f, int x, int y, int z) => d.Grid[f->Offset + (z * f->Ny + y) * f->Nx + x] * 0.0001f;

        /// <summary>The distance from a part's drawn surface at a point in its own space (scale 1, metres).</summary>
        private static float Local(ref Data d, int part, Vector3 p)
        {
            PartField* f = d.Fields + part;
            float fx = (p.x - f->Min.x) / f->Cell, fy = (p.y - f->Min.y) / f->Cell, fz = (p.z - f->Min.z) / f->Cell;
            float cx = fx < 0f ? 0f : fx > f->Nx - 1.001f ? f->Nx - 1.001f : fx;
            float cy = fy < 0f ? 0f : fy > f->Ny - 1.001f ? f->Ny - 1.001f : fy;
            float cz = fz < 0f ? 0f : fz > f->Nz - 1.001f ? f->Nz - 1.001f : fz;
            float ex = fx - cx, ey = fy - cy, ez = fz - cz;
            float extra = f->Cell * Mathf.Sqrt(ex * ex + ey * ey + ez * ez);
            int ix = (int)cx, iy = (int)cy, iz = (int)cz;
            float tx = cx - ix, ty = cy - iy, tz = cz - iz;
            float c000 = Cellv(ref d, f, ix, iy, iz), c100 = Cellv(ref d, f, ix + 1, iy, iz), c010 = Cellv(ref d, f, ix, iy + 1, iz), c110 = Cellv(ref d, f, ix + 1, iy + 1, iz);
            float c001 = Cellv(ref d, f, ix, iy, iz + 1), c101 = Cellv(ref d, f, ix + 1, iy, iz + 1), c011 = Cellv(ref d, f, ix, iy + 1, iz + 1), c111 = Cellv(ref d, f, ix + 1, iy + 1, iz + 1);
            float c00 = c000 + (c100 - c000) * tx, c10 = c010 + (c110 - c010) * tx, c01 = c001 + (c101 - c001) * tx, c11 = c011 + (c111 - c011) * tx;
            float c0 = c00 + (c10 - c00) * ty, c1 = c01 + (c11 - c01) * ty;
            return c0 + (c1 - c0) * tz + extra;
        }

        private static Vector3 Rotate(Quaternion q, Vector3 v)
        {
            // (q v q*, written out: no engine call, so Burst can take it.)
            float x2 = q.x * 2f, y2 = q.y * 2f, z2 = q.z * 2f;
            float xx = q.x * x2, yy = q.y * y2, zz = q.z * z2, xy = q.x * y2, xz = q.x * z2, yz = q.y * z2, wx = q.w * x2, wy = q.w * y2, wz = q.w * z2;
            return new Vector3(
                (1f - (yy + zz)) * v.x + (xy - wz) * v.y + (xz + wy) * v.z,
                (xy + wz) * v.x + (1f - (xx + zz)) * v.y + (yz - wx) * v.z,
                (xz - wy) * v.x + (yz + wx) * v.y + (1f - (xx + yy)) * v.z);
        }

        /// <summary>The distance from a part's drawn surface (metres, at his size; negative inside); far parts by a cheap bound.</summary>
        private static float DistanceTo(ref Data d, int part, Vector3 at)
        {
            Frame* fr = d.Frames + part;
            if (fr->BR < 0f)
            {
                return 10f;
            }

            Vector3 w = at - fr->BC;
            float far = fr->BR + 0.05f;
            float sq = w.x * w.x + w.y * w.y + w.z * w.z;
            if (sq > far * far)
            {
                return Mathf.Sqrt(sq) - fr->BR;
            }

            return Local(ref d, part, Rotate(fr->I, at - fr->P) / d.Size) * d.Size;
        }

        private static Vector3 OutOf(ref Data d, int part, Vector3 at)
        {
            Frame* fr = d.Frames + part;
            float h = d.Fields[part].Cell * 0.5f;
            Vector3 p = Rotate(fr->I, at - fr->P) / d.Size;
            var g = new Vector3(
                Local(ref d, part, p + new Vector3(h, 0f, 0f)) - Local(ref d, part, p - new Vector3(h, 0f, 0f)),
                Local(ref d, part, p + new Vector3(0f, h, 0f)) - Local(ref d, part, p - new Vector3(0f, h, 0f)),
                Local(ref d, part, p + new Vector3(0f, 0f, h)) - Local(ref d, part, p - new Vector3(0f, 0f, h)));
            float m = Mathf.Sqrt(g.x * g.x + g.y * g.y + g.z * g.z);
            return Rotate(fr->R, m > 1e-6f ? g / m : new Vector3(0f, 1f, 0f));
        }

        private static float NearestOf(ref Data d, int set, Vector3 at, out int which)
        {
            float best = 10f;
            which = -1;
            for (int p = 0; p < Parts; p++)
            {
                if ((set & (1 << p)) == 0)
                {
                    continue;
                }

                float dist = DistanceTo(ref d, p, at);
                if (dist < best)
                {
                    best = dist;
                    which = p;
                }
            }

            return best;
        }

        /// <summary>Pushed out of a set of parts to a margin (the ways out of every part it is too near, summed: out of a crease).</summary>
        private static float PushOutOf(ref Data d, int set, ref Vector3 at, float margin, int tries, Stats* st)
        {
            for (int k = 0; k < tries; k++)
            {
                Vector3 way = Vector3.zero;
                float most = 0f;
                for (int p = 0; p < Parts; p++)
                {
                    if ((set & (1 << p)) == 0)
                    {
                        continue;
                    }

                    float dist = DistanceTo(ref d, p, at);
                    if (dist < margin)
                    {
                        way += OutOf(ref d, p, at) * (margin - dist);
                        most = Mathf.Max(most, margin - dist);
                    }
                }

                if (most <= 0f)
                {
                    break;
                }

                float m = Mathf.Sqrt(way.x * way.x + way.y * way.y + way.z * way.z);
                at += (m > 1e-6f ? way / m : new Vector3(0f, 1f, 0f)) * (most + 0.0005f);
                st->Pushed++;
            }

            return NearestOf(ref d, set, at, out _);
        }

        private static bool InAFold(ref Data d, int own, Vector3 at, int inPart)
        {
            for (int p = 0; p < Parts; p++)
            {
                if ((own & (1 << p)) != 0 && p != inPart && DistanceTo(ref d, p, at) < 0.01f)
                {
                    return true;
                }
            }

            return false;
        }

        // ---- One piece ---------------------------------------------------------------------------------------------

        private static Vector3 Home(ref Data d, int i)
        {
            Anchor* an = d.Anchors + i;
            if (an->Joint == 0)
            {
                Frame* fa = d.Frames + an->A;
                return fa->P + Rotate(fa->R, an->H * d.Size);
            }

            Frame* a = d.Frames + an->A;
            Frame* b = d.Frames + an->B;
            Vector3 da = Rotate(a->R, an->DA), db = Rotate(b->R, an->DB);
            // (Turned partway: the two ways blended and made unit again.)
            Vector3 dir = da * (1f - an->W) + db * an->W;
            float m = Mathf.Sqrt(dir.x * dir.x + dir.y * dir.y + dir.z * dir.z);
            dir = m > 1e-6f ? dir / m : db;
            return b->P + dir * (an->R * d.Size);
        }

        private static void MiddleOut(ref Data d, Piece* pc, int a, int b, int c, int e, Stats* st)
        {
            Vector3* x = d.X;
            Vector3 m;
            float reach;
            if (c < 0)
            {
                m = 0.5f * (x[a] + x[b]);
                reach = Mathf.Min(d.DNear[a], d.DNear[b]) - 0.5f * (x[a] - x[b]).magnitude;
            }
            else
            {
                m = 0.25f * (x[a] + x[b] + x[c] + x[e]);
                reach = Mathf.Min(Mathf.Min(d.DNear[a] - (m - x[a]).magnitude, d.DNear[b] - (m - x[b]).magnitude), Mathf.Min(d.DNear[c] - (m - x[c]).magnitude, d.DNear[e] - (m - x[e]).magnitude));
            }

            if (reach >= d.MiddleMargin)
            {
                return;
            }

            Vector3 way = Vector3.zero;
            float most = 0f;
            for (int p = 0; p < Parts; p++)
            {
                if ((pc->Own & (1 << p)) == 0)
                {
                    continue;
                }

                float dist = DistanceTo(ref d, p, m);
                if (dist < d.MiddleMargin)
                {
                    way += OutOf(ref d, p, m) * (d.MiddleMargin - dist);
                    most = Mathf.Max(most, d.MiddleMargin - dist);
                }
            }

            if (most <= 0f)
            {
                return;
            }

            float wm = Mathf.Sqrt(way.x * way.x + way.y * way.y + way.z * way.z);
            Vector3 push = (wm > 1e-6f ? way / wm : new Vector3(0f, 1f, 0f)) * (most + 0.0005f);
            x[a] += push; d.Moved[a] = 1; d.DNear[a] = Mathf.Min(d.DNear[a], 0f);
            x[b] += push; d.Moved[b] = 1; d.DNear[b] = Mathf.Min(d.DNear[b], 0f);
            if (c >= 0)
            {
                x[c] += push; d.Moved[c] = 1; d.DNear[c] = Mathf.Min(d.DNear[c], 0f);
                x[e] += push; d.Moved[e] = 1; d.DNear[e] = Mathf.Min(d.DNear[e], 0f);
            }

            st->Pushed++;
        }

        private static bool MiddleIn(ref Data d, Piece* pc, Vector3 at, float reach, Stats* st)
        {
            if (reach >= 0.0001f)
            {
                return false;
            }

            float dist = NearestOf(ref d, pc->Own, at, out int which);
            if (dist >= 0f)
            {
                return false;
            }

            if (InAFold(ref d, pc->Own, at, which))
            {
                st->Folded++;
                return false;
            }

            st->LastInPart = which;
            st->LastInKind = 2;
            return true;
        }

        /// <summary>
        /// Every point to its home on the body as drawn; with the wobble on, carried on from where it was by its own speed
        /// and its weight, drawn part of the way home, its threads kept near their cut length (stretched hard, crumpled
        /// easily), and never further from home than its fence (the plan's step 4: a fenced wobble, as NVIDIA's, Unity's
        /// and Unreal's cloth do with max distance). The last word comes after it.
        /// </summary>
        private static void Wobble(ref Data d, Piece* pc, int n, int s0)
        {
            Vector3* x = d.X, h = d.Home, pv = d.Prev;
            bool on = d.WobbleOn != 0 && d.Wobble > 0f;
            for (int i = s0; i < s0 + n; i++)
            {
                h[i] = Home(ref d, i);
            }

            if (!on || d.Reset != 0)
            {
                for (int i = s0; i < s0 + n; i++)
                {
                    x[i] = h[i];
                    pv[i] = h[i];
                }

                return;
            }

            float dt = d.Dt < 1f / 240f ? 1f / 240f : d.Dt > 1f / 30f ? 1f / 30f : d.Dt;
            Vector3 fall = new Vector3(0f, -9.81f * d.Weight * dt * dt, 0f);
            for (int i = s0; i < s0 + n; i++)
            {
                float fence = d.Wobble * d.Sway[i] * d.Size;
                Vector3 p = x[i];
                Vector3 off = p - h[i];
                // (No sway here, or thrown far in one picture (set out, a teleport): home, still.)
                if (fence <= 1e-5f || off.x * off.x + off.y * off.y + off.z * off.z > 0.09f)
                {
                    x[i] = h[i];
                    pv[i] = h[i];
                    continue;
                }

                Vector3 v = (p - pv[i]) * (1f - d.Damp);
                pv[i] = p;
                p += v + fall;
                p += (h[i] - p) * d.Hold;
                x[i] = p;
            }

            for (int it = 0; it < d.Iters; it++)
            {
                for (int r = 0; r < pc->Rings; r++)
                {
                    for (int c = 0; c < pc->Cols; c++)
                    {
                        int a = s0 + r * pc->Cols + c;
                        Thread(ref d, a, s0 + r * pc->Cols + (c + 1) % pc->Cols, d.RestU[a] * d.Size);
                        if (r + 1 < pc->Rings)
                        {
                            Thread(ref d, a, a + pc->Cols, d.RestV[a] * d.Size);
                        }
                    }
                }
            }

            for (int i = s0; i < s0 + n; i++)
            {
                float fence = d.Wobble * d.Sway[i] * d.Size;
                Vector3 off = x[i] - h[i];
                float m = Mathf.Sqrt(off.x * off.x + off.y * off.y + off.z * off.z);
                if (m > fence)
                {
                    x[i] = m > 1e-6f ? h[i] + off * (fence / m) : h[i];
                }
            }
        }

        /// <summary>A thread between two points brought toward its cut length: all the way if stretched, a little if crumpled. A point with no sway does not move.</summary>
        private static void Thread(ref Data d, int a, int b, float rest)
        {
            float wa = d.Sway[a] > 0f ? 1f : 0f, wb = d.Sway[b] > 0f ? 1f : 0f;
            if (wa + wb <= 0f)
            {
                return;
            }

            Vector3 e = d.X[b] - d.X[a];
            float len = Mathf.Sqrt(e.x * e.x + e.y * e.y + e.z * e.z);
            if (len < 1e-6f)
            {
                return;
            }

            float k = len > rest ? 1f : 0.1f;
            Vector3 fix = e * ((len - rest) / len * k / (wa + wb));
            d.X[a] += fix * wa;
            d.X[b] -= fix * wb;
        }

        /// <summary>One piece's picture: homes, the last word, the measure. Writes only its own points and its own stats.</summary>
        public static void Piece1(ref Data d, int t)
        {
            Piece* pc = d.Pieces + t;
            Stats* st = d.Stats + t;
            *st = default;
            st->LastInPart = -1;
            int n = pc->Rings * pc->Cols, s0 = pc->Start;
            Wobble(ref d, pc, n, s0);

            if (d.LastWord != 0)
            {
                for (int i = s0; i < s0 + n; i++)
                {
                    PushOutOf(ref d, pc->Others, ref d.X[i], d.OtherMargin, 2, st);
                    d.DNear[i] = PushOutOf(ref d, pc->Own, ref d.X[i], d.PointMargin, 5, st);
                    d.Moved[i] = 0;
                    if (d.DNear[i] < 0f)
                    {
                        st->StillIn++;
                    }
                }

                // (Smoothed: a point pushed one way and its neighbour another leave a spike or a crumple; each pass
                // draws every point halfway to the middle of its four neighbours, then out again.)
                for (int pass = 0; pass < (pc->Smooths != 0 ? d.Smooth : 0); pass++)
                {
                    for (int r = 0; r < pc->Rings; r++)
                    {
                        for (int c = 0; c < pc->Cols; c++)
                        {
                            int i = s0 + r * pc->Cols + c;
                            Vector3 sum = d.X[s0 + r * pc->Cols + (c + 1) % pc->Cols] + d.X[s0 + r * pc->Cols + (c + pc->Cols - 1) % pc->Cols];
                            float k = 2f;
                            if (r > 0) { sum += d.X[i - pc->Cols]; k += 1f; }
                            if (r + 1 < pc->Rings) { sum += d.X[i + pc->Cols]; k += 1f; }
                            d.X[i] = 0.5f * d.X[i] + 0.5f * (sum / k);
                        }
                    }

                    for (int i = s0; i < s0 + n; i++)
                    {
                        PushOutOf(ref d, pc->Others, ref d.X[i], d.OtherMargin, 2, st);
                        d.DNear[i] = PushOutOf(ref d, pc->Own, ref d.X[i], d.PointMargin, 5, st);
                    }
                }

                for (int round = 0; round < d.Rounds; round++)
                {
                    for (int r = 0; r < pc->Rings; r++)
                    {
                        for (int c = 0; c < pc->Cols; c++)
                        {
                            int a = s0 + r * pc->Cols + c, b = s0 + r * pc->Cols + (c + 1) % pc->Cols;
                            MiddleOut(ref d, pc, a, b, -1, -1, st);
                            if (r + 1 < pc->Rings)
                            {
                                MiddleOut(ref d, pc, a, a + pc->Cols, -1, -1, st);
                                MiddleOut(ref d, pc, a, b, a + pc->Cols, b + pc->Cols, st);
                            }
                        }
                    }

                    bool any = false;
                    for (int i = s0; i < s0 + n; i++)
                    {
                        if (d.Moved[i] != 0)
                        {
                            any = true;
                            d.Moved[i] = 0;
                            d.DNear[i] = PushOutOf(ref d, pc->Own, ref d.X[i], d.PointMargin, 3, st);
                        }
                    }

                    if (!any)
                    {
                        break;
                    }
                }
            }

            if (d.Measure == 0)
            {
                return;
            }

            for (int i = s0; i < s0 + n; i++)
            {
                st->PointsChecked++;
                float dist = NearestOf(ref d, pc->Own, d.X[i], out int which);
                d.DNear[i] = dist;
                d.Inside[i] = 0;
                if (dist < 0f && InAFold(ref d, pc->Own, d.X[i], which))
                {
                    st->Folded++;
                }
                else if (dist < 0f)
                {
                    st->PointsIn++;
                    d.Inside[i] = 1;
                    st->Deepest = Mathf.Max(st->Deepest, -dist);
                    st->LastInPart = which;
                    st->LastInKind = 1;
                }

                if (NearestOf(ref d, pc->Others, d.X[i], out _) < 0f)
                {
                    st->Crossed++;
                }
            }

            Vector3* x = d.X;
            for (int r = 0; r < pc->Rings; r++)
            {
                for (int c = 0; c < pc->Cols; c++)
                {
                    int a = s0 + r * pc->Cols + c, b = s0 + r * pc->Cols + (c + 1) % pc->Cols;
                    st->MiddlesChecked++;
                    if (MiddleIn(ref d, pc, 0.5f * (x[a] + x[b]), Mathf.Min(d.DNear[a], d.DNear[b]) - 0.5f * (x[a] - x[b]).magnitude, st))
                    {
                        st->MiddlesIn++;
                    }

                    if (r + 1 < pc->Rings)
                    {
                        int e = a + pc->Cols, f = b + pc->Cols;
                        st->MiddlesChecked++;
                        if (MiddleIn(ref d, pc, 0.5f * (x[a] + x[e]), Mathf.Min(d.DNear[a], d.DNear[e]) - 0.5f * (x[a] - x[e]).magnitude, st))
                        {
                            st->MiddlesIn++;
                        }

                        st->CentresChecked++;
                        Vector3 m = 0.25f * (x[a] + x[b] + x[e] + x[f]);
                        float reach = Mathf.Min(Mathf.Min(d.DNear[a] - (m - x[a]).magnitude, d.DNear[b] - (m - x[b]).magnitude), Mathf.Min(d.DNear[e] - (m - x[e]).magnitude, d.DNear[f] - (m - x[f]).magnitude));
                        if (MiddleIn(ref d, pc, m, reach, st))
                        {
                            st->CentresIn++;
                        }
                    }
                }
            }
        }

        // ---- A sewn garment (the plan "Clothes cut and sewn like real clothes", step 3) ----------------------------

        /// <summary>A thread's middle (c &lt; 0) or a triangle's centre pushed out of a set of parts, its two or three points with it.</summary>
        private static void MiddleOutOf(ref Data d, int own, int a, int b, int c, Stats* st)
        {
            Vector3* x = d.X;
            Vector3 m;
            float reach;
            if (c < 0)
            {
                m = 0.5f * (x[a] + x[b]);
                reach = Mathf.Min(d.DNear[a], d.DNear[b]) - 0.5f * (x[a] - x[b]).magnitude;
            }
            else
            {
                m = (x[a] + x[b] + x[c]) * (1f / 3f);
                reach = Mathf.Min(d.DNear[a] - (m - x[a]).magnitude, Mathf.Min(d.DNear[b] - (m - x[b]).magnitude, d.DNear[c] - (m - x[c]).magnitude));
            }

            // (Near enough to nothing that the middle cannot be in: the distance at each point less the way to the middle.)
            if (reach >= d.MiddleMargin)
            {
                return;
            }

            if (d.FoldsStay != 0)
            {
                float dm = NearestOf(ref d, own, m, out int inPart);
                if (dm < 0f && InAFold(ref d, own, m, inPart))
                {
                    return;
                }
            }

            Vector3 way = Vector3.zero;
            float most = 0f;
            for (int p = 0; p < Parts; p++)
            {
                if ((own & (1 << p)) == 0)
                {
                    continue;
                }

                float dist = DistanceTo(ref d, p, m);
                if (dist < d.MiddleMargin)
                {
                    way += OutOf(ref d, p, m) * (d.MiddleMargin - dist);
                    most = Mathf.Max(most, d.MiddleMargin - dist);
                }
            }

            if (most <= 0f)
            {
                return;
            }

            float wm = Mathf.Sqrt(way.x * way.x + way.y * way.y + way.z * way.z);
            Vector3 push = (wm > 1e-6f ? way / wm : new Vector3(0f, 1f, 0f)) * (most + 0.0005f);
            // (Kept, not added: a point shares six threads and six triangles, and in a crease every one of them pushed it
            // its whole way out, 14 cm at a bent elbow. Each point takes the biggest push it was given, after the round.)
            Shove(ref d, a, push);
            Shove(ref d, b, push);
            if (c >= 0)
            {
                Shove(ref d, c, push);
            }

            st->Pushed++;
        }

        private static void Shove(ref Data d, int i, Vector3 push)
        {
            if (d.KeepMost == 0)
            {
                d.X[i] += push;
                d.Moved[i] = 1;
                d.DNear[i] = Mathf.Min(d.DNear[i], 0f);
                return;
            }

            // (At most a centimetre a round: the rounds otherwise went on lifting a point over a bent elbow, 8 cm out.)
            float m2 = push.sqrMagnitude;
            if (m2 > ShoveMost * ShoveMost)
            {
                push *= ShoveMost / Mathf.Sqrt(m2);
            }

            Vector3 had = d.Shove[i];
            if (d.Moved[i] != 1 || push.sqrMagnitude > had.sqrMagnitude)
            {
                d.Shove[i] = push;
            }

            d.Moved[i] = 1;
            d.DNear[i] = Mathf.Min(d.DNear[i], 0f);
        }

        private static bool MiddleInOf(ref Data d, int own, Vector3 at, float reach, Stats* st)
        {
            if (reach >= 0.0001f)
            {
                return false;
            }

            float dist = NearestOf(ref d, own, at, out int which);
            if (dist >= 0f)
            {
                return false;
            }

            if (InAFold(ref d, own, at, which))
            {
                st->Folded++;
                return false;
            }

            st->LastInPart = which;
            st->LastInKind = 2;
            return true;
        }

        /// <summary>
        /// One sewn garment's picture: every point carried to its home (its one or two parts as drawn, blended), the
        /// last word (each point, then rounds of thread middles and triangle centres, out of the drawn body), and the
        /// fast measure. Writes only its own points and its own stats, so garments can run on different threads.
        /// </summary>
        public static void Mesh1(ref Data d, int g)
        {
            MeshPiece* mp = d.Meshes + g;
            Stats* st = d.Stats + g;
            *st = default;
            st->LastInPart = -1;
            int s0 = mp->Start, s1 = mp->Start + mp->Count;
            for (int i = s0; i < s1; i++)
            {
                Skin* sk = d.Skins + i;
                Frame* fa = d.Frames + sk->A;
                Vector3 h;
                if (sk->W > 0f && sk->J >= 0)
                {
                    Frame* fb = d.Frames + sk->B;
                    Vector3 dir = Rotate(fa->R, sk->OA) * (1f - sk->W) + Rotate(fb->R, sk->OB) * sk->W;
                    float m = Mathf.Sqrt(dir.x * dir.x + dir.y * dir.y + dir.z * dir.z);
                    dir = m > 1e-6f ? dir / m : Rotate(fb->R, sk->OB);
                    h = d.Frames[sk->J].P + dir * (sk->R * d.Size);
                }
                else
                {
                    h = fa->P + Rotate(fa->R, sk->HA * d.Size);
                    if (sk->W > 0f)
                    {
                        Frame* fb = d.Frames + sk->B;
                        h = h * (1f - sk->W) + (fb->P + Rotate(fb->R, sk->HB * d.Size)) * sk->W;
                    }
                }

                d.Home[i] = h;
                d.X[i] = h;
            }

            int* th = d.Threads + 2 * mp->ThreadStart;
            int* tr = d.Tris + 3 * mp->TriStart;
            if (d.LastWord != 0)
            {
                for (int i = s0; i < s1; i++)
                {
                    // (A point carried round a joint and inside: out along its own way from the joint, so the points
                    // round a bent elbow or knee all go out the same way and do not tangle through each other.)
                    Skin* sk = d.Skins + i;
                    if (d.RadialOut != 0 && sk->J >= 0 && sk->W > 0f)
                    {
                        Vector3 jp = d.Frames[sk->J].P;
                        Vector3 way = d.X[i] - jp;
                        float wl = Mathf.Sqrt(way.x * way.x + way.y * way.y + way.z * way.z);
                        if (wl > 1e-5f)
                        {
                            // (At most 4 cm: a point deep in the bend whose way from the joint runs along the bone would
                            // go far through him (17 cm); the plain push out takes it from there.)
                            way /= wl;
                            Vector3 from = d.X[i];
                            for (int k = 0; k < 12; k++)
                            {
                                float dist = NearestOf(ref d, mp->Own, d.X[i], out _);
                                if (dist >= d.PointMargin)
                                {
                                    break;
                                }

                                float step = Mathf.Max(0.002f, d.PointMargin - dist);
                                if ((d.X[i] - from).sqrMagnitude > RadialMost * RadialMost)
                                {
                                    d.X[i] = from;
                                    break;
                                }

                                d.X[i] += way * step;
                                st->Pushed++;
                            }
                        }
                    }

                    d.Moved[i] = 0;
                    if (d.FoldsStay != 0)
                    {
                        float dh = NearestOf(ref d, mp->Own, d.X[i], out int inPart);
                        if (dh < 0f && InAFold(ref d, mp->Own, d.X[i], inPart))
                        {
                            d.DNear[i] = dh;
                            continue;
                        }
                    }

                    d.DNear[i] = PushOutOf(ref d, mp->Own, ref d.X[i], d.PointMargin, 5, st);
                    if (d.DNear[i] < 0f)
                    {
                        st->StillIn++;
                    }
                }

                // (Smoothed, before the rounds: a point pushed far while its neighbours were not makes a flap (a bent
                // elbow in a wind-up, 14 cm). Each pass takes every point's push halfway to the mean of its neighbours'
                // pushes, and out again where that moved it.)
                for (int pass = 0; pass < (d.AdjStart != null ? d.Smooth : 0); pass++)
                {
                    for (int i = s0; i < s1; i++)
                    {
                        Vector3 sum = Vector3.zero;
                        int a0 = d.AdjStart[i], a1 = d.AdjStart[i + 1];
                        for (int k = a0; k < a1; k++)
                        {
                            int j = d.Adj[k];
                            sum += d.X[j] - d.Home[j];
                        }

                        d.Shove[i] = a1 > a0 ? sum / (a1 - a0) : d.X[i] - d.Home[i];
                    }

                    for (int i = s0; i < s1; i++)
                    {
                        Vector3 mine = d.X[i] - d.Home[i];
                        Vector3 change = 0.5f * (d.Shove[i] - mine);
                        if (change.sqrMagnitude < 1e-6f)
                        {
                            continue;
                        }

                        d.X[i] += change;
                        d.DNear[i] = PushOutOf(ref d, mp->Own, ref d.X[i], d.PointMargin, 3, st);
                    }
                }

                // (Rounds: the first goes over every thread and triangle; each later one only over those with a point
                // that was moved, since nothing else changed. Moved: 1 pushed in this round, 2 pushed in the last.)
                for (int round = 0; round < d.Rounds; round++)
                {
                    bool all = round == 0;
                    for (int k = 0; k < mp->ThreadCount; k++)
                    {
                        int a = th[2 * k], b = th[2 * k + 1];
                        if (all || d.Moved[a] != 0 || d.Moved[b] != 0)
                        {
                            MiddleOutOf(ref d, mp->Own, a, b, -1, st);
                        }
                    }

                    for (int k = 0; k < mp->TriCount; k++)
                    {
                        int a = tr[3 * k], b = tr[3 * k + 1], c = tr[3 * k + 2];
                        if (all || d.Moved[a] != 0 || d.Moved[b] != 0 || d.Moved[c] != 0)
                        {
                            MiddleOutOf(ref d, mp->Own, a, b, c, st);
                        }
                    }

                    bool any = false;
                    for (int i = s0; i < s1; i++)
                    {
                        if (d.Moved[i] == 1)
                        {
                            any = true;
                            d.Moved[i] = 2;
                            if (d.KeepMost != 0)
                            {
                                d.X[i] += d.Shove[i];
                            }

                            d.DNear[i] = PushOutOf(ref d, mp->Own, ref d.X[i], d.PointMargin, 3, st);
                        }
                        else
                        {
                            d.Moved[i] = 0;
                        }
                    }

                    if (!any)
                    {
                        break;
                    }
                }

                for (int i = s0; i < s1; i++)
                {
                    d.Moved[i] = 0;
                }


            }

            if (d.Measure == 0)
            {
                return;
            }

            for (int i = s0; i < s1; i++)
            {
                st->PointsChecked++;
                float dist = NearestOf(ref d, mp->Own, d.X[i], out int which);
                d.DNear[i] = dist;
                d.Inside[i] = 0;
                if (dist < 0f && InAFold(ref d, mp->Own, d.X[i], which))
                {
                    st->Folded++;
                }
                else if (dist < 0f)
                {
                    st->PointsIn++;
                    d.Inside[i] = 1;
                    st->Deepest = Mathf.Max(st->Deepest, -dist);
                    st->LastInPart = which;
                    st->LastInKind = 1;
                }
            }

            Vector3* x = d.X;
            for (int k = 0; k < mp->ThreadCount; k++)
            {
                int a = th[2 * k], b = th[2 * k + 1];
                st->MiddlesChecked++;
                if (MiddleInOf(ref d, mp->Own, 0.5f * (x[a] + x[b]), Mathf.Min(d.DNear[a], d.DNear[b]) - 0.5f * (x[a] - x[b]).magnitude, st))
                {
                    st->MiddlesIn++;
                }
            }

            for (int k = 0; k < mp->TriCount; k++)
            {
                int a = tr[3 * k], b = tr[3 * k + 1], c = tr[3 * k + 2];
                st->CentresChecked++;
                Vector3 m = (x[a] + x[b] + x[c]) * (1f / 3f);
                float reach = Mathf.Min(d.DNear[a] - (m - x[a]).magnitude, Mathf.Min(d.DNear[b] - (m - x[b]).magnitude, d.DNear[c] - (m - x[c]).magnitude));
                if (MiddleInOf(ref d, mp->Own, m, reach, st))
                {
                    st->CentresIn++;
                }
            }
        }
    }
}
