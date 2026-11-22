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
            public Vector3* X;
            public float* DNear;
            public byte* Moved, Inside;
            public Stats* Stats;
            public float Size, PointMargin, MiddleMargin, OtherMargin;
            public int LastWord, Rounds, Measure, Smooth;
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

        /// <summary>One piece's picture: homes, the last word, the measure. Writes only its own points and its own stats.</summary>
        public static void Piece1(ref Data d, int t)
        {
            Piece* pc = d.Pieces + t;
            Stats* st = d.Stats + t;
            *st = default;
            st->LastInPart = -1;
            int n = pc->Rings * pc->Cols, s0 = pc->Start;
            for (int i = s0; i < s0 + n; i++)
            {
                d.X[i] = Home(ref d, i);
            }

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
    }
}
