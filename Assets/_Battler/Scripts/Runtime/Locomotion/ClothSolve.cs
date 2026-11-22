using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The cloth's sums, on plain numbers through pointers, so the same code runs compiled by Burst on Unity's worker
    /// threads (<see cref="ClothJob"/>) and directly in the headless tests (22 November, his "Is it possible to offload
    /// this to more cores?"). One call is one picture: the small steps, each moving the points by their speed and
    /// gravity, then the threads, the speed limit, the leash, the floor, out of the body's shapes point by point, and
    /// the body's capsules kept from passing between two joined points (the edges); then what it measures.
    /// </summary>
    public static unsafe class ClothSolve
    {
        /// <summary>No thread is let longer than this many times its length (the last word of each small step).</summary>
        public const float TearsAt = 1.6f;

        /// <summary>A shape of the body at the start of the picture (0) and at its end (1).</summary>
        public struct Shape
        {
            public int Kind; // 0 sphere, 1 capsule, 2 box
            public int Part;
            public float Radius;
            public float A0x, A0y, A0z, B0x, B0y, B0z, A1x, A1y, A1z, B1x, B1y, B1z;
            public float Q0x, Q0y, Q0z, Q0w, Q1x, Q1y, Q1z, Q1w; // a box's turn
            public float Hx, Hy, Hz; // half a box's size
        }

        /// <summary>Everything one picture of one garment needs, as pointers to its numbers and its settings.</summary>
        public struct Data
        {
            public int Count, Threads, Shapes, Steps;
            public float* X, Y, Z, BX, BY, BZ, SX, SY, SZ, H0X, H0Y, H0Z, H1X, H1Y, H1Z, Free, Leash;
            public int* TA, TB;
            public float* TL;
            public byte* TKind; // 0 a straight thread, 1 a bending one, 2 a straight thread that is also an edge
            public Shape* Shape;
            public Now* Work; // room for one "now" a shape
            public byte* Hidden; // per part: 1 if not drawn
            public float* Stats; // checked, inside, inside a drawn part, deepest, most stretch, pinched, wrong (1 NaN, 2 far, 3 torn), worst thread, threads held at TearsAt
            public float Dt, Gap, Thickness, Air, Floor, Stretch, Bending, GX, GY, GZ;
        }

        /// <summary>A shape as it is in one small step.</summary>
        public struct Now
        {
            public int Kind, Part;
            public float Ax, Ay, Az, Dx, Dy, Dz, InvLen2, R, Bound, Cx, Cy, Cz, Ex, Ey, Ez;
            public float M00, M01, M02, M10, M11, M12, M20, M21, M22;
        }

        public const int StatCount = 9;

        public static void Step(ref Data d)
        {
            int n = d.Steps < 1 ? 1 : d.Steps;
            float h = d.Dt / n;
            float most = d.Thickness, most2 = most * most;
            float keep = Mathf.Exp(-d.Air * h);
            float pgx = d.GX * h * h, pgy = d.GY * h * h, pgz = d.GZ * h * h;
            float a = d.Stretch < 0f ? 0f : d.Stretch, ab = d.Bending < 0f ? 0f : d.Bending;
            float floor = d.Floor + d.Gap;
            for (int s = 0; s < n; s++)
            {
                float t = (s + 1f) / n;
                ShapesAt(ref d, t);
                for (int i = 0; i < d.Count; i++)
                {
                    d.SX[i] = d.X[i]; d.SY[i] = d.Y[i]; d.SZ[i] = d.Z[i];
                    if (d.Free[i] <= 0f)
                    {
                        d.X[i] = d.BX[i] = d.H0X[i] + (d.H1X[i] - d.H0X[i]) * t;
                        d.Y[i] = d.BY[i] = d.H0Y[i] + (d.H1Y[i] - d.H0Y[i]) * t;
                        d.Z[i] = d.BZ[i] = d.H0Z[i] + (d.H1Z[i] - d.H0Z[i]) * t;
                        continue;
                    }

                    float vx = (d.X[i] - d.BX[i]) * keep, vy = (d.Y[i] - d.BY[i]) * keep, vz = (d.Z[i] - d.BZ[i]) * keep;
                    d.BX[i] = d.X[i]; d.BY[i] = d.Y[i]; d.BZ[i] = d.Z[i];
                    d.X[i] += vx + pgx; d.Y[i] += vy + pgy; d.Z[i] += vz + pgz;
                }

                // (The threads: XPBD, one pass each small step.)
                for (int k = 0; k < d.Threads; k++)
                {
                    int i = d.TA[k], j = d.TB[k];
                    float wi = d.Free[i], wj = d.Free[j], w = wi + wj;
                    if (w <= 0f)
                    {
                        continue;
                    }

                    float dx = d.X[i] - d.X[j], dy = d.Y[i] - d.Y[j], dz = d.Z[i] - d.Z[j];
                    float len = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
                    if (len < 1e-7f)
                    {
                        continue;
                    }

                    float lambda = -(len - d.TL[k]) / (w + (d.TKind[k] == 1 ? ab : a)) / len;
                    d.X[i] += wi * lambda * dx; d.Y[i] += wi * lambda * dy; d.Z[i] += wi * lambda * dz;
                    d.X[j] -= wj * lambda * dx; d.Y[j] -= wj * lambda * dy; d.Z[j] -= wj * lambda * dz;
                }

                for (int i = 0; i < d.Count; i++)
                {
                    if (d.Free[i] <= 0f)
                    {
                        continue;
                    }

                    // (The speed limit and the leash first; then the floor; then out of the body, last.)
                    float mx = d.X[i] - d.SX[i], my = d.Y[i] - d.SY[i], mz = d.Z[i] - d.SZ[i], m2 = mx * mx + my * my + mz * mz;
                    if (m2 > most2)
                    {
                        float f = most / Mathf.Sqrt(m2);
                        d.X[i] = d.SX[i] + mx * f; d.Y[i] = d.SY[i] + my * f; d.Z[i] = d.SZ[i] + mz * f;
                    }

                    float hx = d.H0X[i] + (d.H1X[i] - d.H0X[i]) * t, hy = d.H0Y[i] + (d.H1Y[i] - d.H0Y[i]) * t, hz = d.H0Z[i] + (d.H1Z[i] - d.H0Z[i]) * t;
                    float ox = d.X[i] - hx, oy = d.Y[i] - hy, oz = d.Z[i] - hz, o2 = ox * ox + oy * oy + oz * oz, l = d.Leash[i];
                    if (o2 > l * l)
                    {
                        float f = l / Mathf.Sqrt(o2);
                        d.X[i] = hx + ox * f; d.Y[i] = hy + oy * f; d.Z[i] = hz + oz * f;
                    }

                    if (d.Y[i] < floor)
                    {
                        d.Y[i] = floor;
                        d.BX[i] += 0.5f * (d.X[i] - d.BX[i]);
                        d.BZ[i] += 0.5f * (d.Z[i] - d.BZ[i]);
                    }

                    if (PushOut(ref d, i))
                    {
                        PushOut(ref d, i);
                    }
                }

                // (The body's capsules kept from passing between two joined points: a leg through the cloth.)
                for (int k = 0; k < d.Threads; k++)
                {
                    if (d.TKind[k] == 2)
                    {
                        EdgeOut(ref d, d.TA[k], d.TB[k]);
                    }
                }

                // (The last word: no thread longer than TearsAt times its length, whatever the body did; the cloth
                // may then be left a little in a limb for a moment, but it is never torn or thrown back to hanging.
                // Added 22 November: a man lying under his robe had it reset 20 times in six seconds.)
                for (int k = 0; k < d.Threads; k++)
                {
                    if (d.TKind[k] == 1)
                    {
                        continue;
                    }

                    int i = d.TA[k], j = d.TB[k];
                    float wi = d.Free[i], wj = d.Free[j], w = wi + wj;
                    float dx = d.X[i] - d.X[j], dy = d.Y[i] - d.Y[j], dz = d.Z[i] - d.Z[j];
                    float len = Mathf.Sqrt(dx * dx + dy * dy + dz * dz), most3 = TearsAt * d.TL[k];
                    if (w <= 0f || len <= most3)
                    {
                        continue;
                    }

                    float f = (len - most3) / (w * len);
                    d.X[i] -= wi * f * dx; d.Y[i] -= wi * f * dy; d.Z[i] -= wi * f * dz;
                    d.X[j] += wj * f * dx; d.Y[j] += wj * f * dy; d.Z[j] += wj * f * dz;
                    d.Stats[8] += 1f;
                }
            }

            Measure(ref d);
        }

        private static void ShapesAt(ref Data d, float t)
        {
            for (int k = 0; k < d.Shapes; k++)
            {
                Shape sh = d.Shape[k];
                Now w = default;
                w.Kind = sh.Kind;
                w.Part = sh.Part;
                if (sh.Kind == 2)
                {
                    float cx = sh.A0x + (sh.A1x - sh.A0x) * t, cy = sh.A0y + (sh.A1y - sh.A0y) * t, cz = sh.A0z + (sh.A1z - sh.A0z) * t;
                    // (The turn eased between the two: a normalised blend, near enough for one picture's turn.)
                    float dot = sh.Q0x * sh.Q1x + sh.Q0y * sh.Q1y + sh.Q0z * sh.Q1z + sh.Q0w * sh.Q1w;
                    float sg = dot < 0f ? -1f : 1f;
                    float qx = sh.Q0x + (sg * sh.Q1x - sh.Q0x) * t, qy = sh.Q0y + (sg * sh.Q1y - sh.Q0y) * t, qz = sh.Q0z + (sg * sh.Q1z - sh.Q0z) * t, qw = sh.Q0w + (sg * sh.Q1w - sh.Q0w) * t;
                    float ql = Mathf.Sqrt(qx * qx + qy * qy + qz * qz + qw * qw);
                    if (ql < 1e-8f)
                    {
                        qw = 1f; ql = 1f;
                    }

                    qx /= ql; qy /= ql; qz /= ql; qw /= ql;
                    // (The box's own axes in the world: the rows of the world-to-own turn.)
                    w.M00 = 1f - 2f * (qy * qy + qz * qz); w.M01 = 2f * (qx * qy + qw * qz); w.M02 = 2f * (qx * qz - qw * qy);
                    w.M10 = 2f * (qx * qy - qw * qz); w.M11 = 1f - 2f * (qx * qx + qz * qz); w.M12 = 2f * (qy * qz + qw * qx);
                    w.M20 = 2f * (qx * qz + qw * qy); w.M21 = 2f * (qy * qz - qw * qx); w.M22 = 1f - 2f * (qx * qx + qy * qy);
                    w.Cx = cx; w.Cy = cy; w.Cz = cz;
                    w.Ex = sh.Hx + d.Gap; w.Ey = sh.Hy + d.Gap; w.Ez = sh.Hz + d.Gap;
                    w.Bound = Mathf.Sqrt(w.Ex * w.Ex + w.Ey * w.Ey + w.Ez * w.Ez);
                }
                else
                {
                    float px = sh.A0x + (sh.A1x - sh.A0x) * t, py = sh.A0y + (sh.A1y - sh.A0y) * t, pz = sh.A0z + (sh.A1z - sh.A0z) * t;
                    float qx = px, qy = py, qz = pz;
                    if (sh.Kind == 1)
                    {
                        qx = sh.B0x + (sh.B1x - sh.B0x) * t; qy = sh.B0y + (sh.B1y - sh.B0y) * t; qz = sh.B0z + (sh.B1z - sh.B0z) * t;
                    }

                    w.Ax = px; w.Ay = py; w.Az = pz;
                    w.Dx = qx - px; w.Dy = qy - py; w.Dz = qz - pz;
                    float d2 = w.Dx * w.Dx + w.Dy * w.Dy + w.Dz * w.Dz;
                    w.InvLen2 = d2 > 1e-10f ? 1f / d2 : 0f;
                    w.R = sh.Radius + d.Gap;
                    w.Cx = 0.5f * (px + qx); w.Cy = 0.5f * (py + qy); w.Cz = 0.5f * (pz + qz);
                    w.Bound = 0.5f * Mathf.Sqrt(d2) + w.R;
                }

                d.Work[k] = w;
            }
        }

        /// <summary>A point pushed out of every shape it is in, to its surface and the gap, the way it came from (its side).</summary>
        private static bool PushOut(ref Data d, int i)
        {
            bool moved = false;
            for (int k = 0; k < d.Shapes; k++)
            {
                Now w = d.Work[k];
                float px = d.X[i], py = d.Y[i], pz = d.Z[i];
                float cx = px - w.Cx, cy = py - w.Cy, cz = pz - w.Cz;
                if (cx * cx + cy * cy + cz * cz > w.Bound * w.Bound)
                {
                    continue;
                }

                if (w.Kind == 2)
                {
                    float lx = w.M00 * cx + w.M01 * cy + w.M02 * cz;
                    float ly = w.M10 * cx + w.M11 * cy + w.M12 * cz;
                    float lz = w.M20 * cx + w.M21 * cy + w.M22 * cz;
                    float dx = w.Ex - Mathf.Abs(lx), dy = w.Ey - Mathf.Abs(ly), dz = w.Ez - Mathf.Abs(lz);
                    if (dx <= 0f || dy <= 0f || dz <= 0f)
                    {
                        continue;
                    }

                    float qx = d.SX[i] - w.Cx, qy = d.SY[i] - w.Cy, qz = d.SZ[i] - w.Cz;
                    float wx = w.M00 * qx + w.M01 * qy + w.M02 * qz;
                    float wy = w.M10 * qx + w.M11 * qy + w.M12 * qz;
                    float wz = w.M20 * qx + w.M21 * qy + w.M22 * qz;
                    float ox = Mathf.Abs(wx) - w.Ex, oy = Mathf.Abs(wy) - w.Ey, oz = Mathf.Abs(wz) - w.Ez;
                    bool outside = ox > 0f || oy > 0f || oz > 0f;
                    int axis = outside ? (ox >= oy && ox >= oz ? 0 : oy >= oz ? 1 : 2) : (dx <= dy && dx <= dz ? 0 : dy <= dz ? 1 : 2);
                    if (axis == 0) lx = (outside ? (wx >= 0f ? 1f : -1f) : (lx >= 0f ? 1f : -1f)) * w.Ex;
                    else if (axis == 1) ly = (outside ? (wy >= 0f ? 1f : -1f) : (ly >= 0f ? 1f : -1f)) * w.Ey;
                    else lz = (outside ? (wz >= 0f ? 1f : -1f) : (lz >= 0f ? 1f : -1f)) * w.Ez;
                    d.X[i] = w.Cx + w.M00 * lx + w.M10 * ly + w.M20 * lz;
                    d.Y[i] = w.Cy + w.M01 * lx + w.M11 * ly + w.M21 * lz;
                    d.Z[i] = w.Cz + w.M02 * lx + w.M12 * ly + w.M22 * lz;
                    moved = true;
                    continue;
                }

                float along = ((px - w.Ax) * w.Dx + (py - w.Ay) * w.Dy + (pz - w.Az) * w.Dz) * w.InvLen2;
                along = along < 0f ? 0f : along > 1f ? 1f : along;
                float nx = w.Ax + w.Dx * along, ny = w.Ay + w.Dy * along, nz = w.Az + w.Dz * along;
                float ex = px - nx, ey = py - ny, ez = pz - nz, e2 = ex * ex + ey * ey + ez * ez;
                if (e2 >= w.R * w.R)
                {
                    continue;
                }

                float fx = d.SX[i] - nx, fy = d.SY[i] - ny, fz = d.SZ[i] - nz, f2 = fx * fx + fy * fy + fz * fz;
                if (f2 < w.R * w.R * 0.81f)
                {
                    fx = ex; fy = ey; fz = ez; f2 = e2;
                }

                if (f2 < 1e-12f)
                {
                    fx = 0f; fy = 1f; fz = 0f; f2 = 1f;
                }

                float g = w.R / Mathf.Sqrt(f2);
                d.X[i] = nx + fx * g; d.Y[i] = ny + fy * g; d.Z[i] = nz + fz * g;
                moved = true;
            }

            return moved;
        }

        /// <summary>
        /// A thread between two points kept out of every capsule of the body: the nearest points of the thread and the
        /// capsule's bone found, and if they are nearer than the capsule's radius and the gap, both points moved out
        /// together, each by its share, so the thread's nearest point is on the surface. Without it a thin limb passes
        /// between two points that are each outside it.
        /// </summary>
        private static void EdgeOut(ref Data d, int i, int j)
        {
            float wi = d.Free[i], wj = d.Free[j];
            if (wi + wj <= 0f)
            {
                return;
            }

            for (int k = 0; k < d.Shapes; k++)
            {
                Now w = d.Work[k];
                if (w.Kind != 1)
                {
                    continue;
                }

                float px = d.X[i], py = d.Y[i], pz = d.Z[i];
                float ux = d.X[j] - px, uy = d.Y[j] - py, uz = d.Z[j] - pz;
                float mx = px + 0.5f * ux - w.Cx, my = py + 0.5f * uy - w.Cy, mz = pz + 0.5f * uz - w.Cz;
                float half = 0.5f * Mathf.Sqrt(ux * ux + uy * uy + uz * uz);
                if (mx * mx + my * my + mz * mz > (w.Bound + half) * (w.Bound + half))
                {
                    continue;
                }

                // (The nearest points of two segments: the thread p + s u, the bone a + r v.)
                float vx = w.Dx, vy = w.Dy, vz = w.Dz;
                float rx = px - w.Ax, ry = py - w.Ay, rz = pz - w.Az;
                float uu = ux * ux + uy * uy + uz * uz, vv = vx * vx + vy * vy + vz * vz, uv = ux * vx + uy * vy + uz * vz;
                float ur = ux * rx + uy * ry + uz * rz, vr = vx * rx + vy * ry + vz * rz;
                float den = uu * vv - uv * uv;
                float s = den > 1e-12f ? (uv * vr - vv * ur) / den : 0f;
                s = s < 0f ? 0f : s > 1f ? 1f : s;
                float r = vv > 1e-12f ? (uv * s + vr) / vv : 0f;
                r = r < 0f ? 0f : r > 1f ? 1f : r;
                s = uu > 1e-12f ? (uv * r - ur) / uu : 0f;
                s = s < 0f ? 0f : s > 1f ? 1f : s;
                float cx = px + ux * s, cy = py + uy * s, cz = pz + uz * s;
                float bx = w.Ax + vx * r, by = w.Ay + vy * r, bz = w.Az + vz * r;
                float nx = cx - bx, ny = cy - by, nz = cz - bz, dist = Mathf.Sqrt(nx * nx + ny * ny + nz * nz);
                if (dist >= w.R || dist < 1e-6f || s <= 0f || s >= 1f)
                {
                    // (Its ends are kept out point by point; only a bone through the thread's middle is mended here.)
                    continue;
                }

                nx /= dist; ny /= dist; nz /= dist;
                float depth = w.R - dist;
                float ai = (1f - s) * wi, aj = s * wj;
                float lambda = depth / (ai * (1f - s) + aj * s);
                // (Neither end moved further than the thickness in one go: with one end pinned and the bone near it the
                // free end would be thrown depth / s, metres for a bone at its pinned end (22 November: the robe's
                // rows next to its collar and sash were thrown, and reset).)
                float far = Mathf.Max(lambda * ai, lambda * aj);
                if (far > d.Thickness)
                {
                    lambda *= d.Thickness / far;
                }

                d.X[i] += nx * lambda * ai; d.Y[i] += ny * lambda * ai; d.Z[i] += nz * lambda * ai;
                d.X[j] += nx * lambda * aj; d.Y[j] += ny * lambda * aj; d.Z[j] += nz * lambda * aj;
            }
        }

        private static void Measure(ref Data d)
        {
            float* st = d.Stats;
            for (int k = 0; k < d.Shapes; k++)
            {
                Now w = d.Work[k];
                if (w.Kind == 2)
                {
                    w.Ex -= d.Gap; w.Ey -= d.Gap; w.Ez -= d.Gap;
                }
                else
                {
                    w.R -= d.Gap;
                }

                d.Work[k] = w;
            }

            bool seen = false;
            for (int i = 0; i < d.Count; i++)
            {
                float x = d.X[i], y = d.Y[i], z = d.Z[i];
                if (float.IsNaN(x) || float.IsNaN(y) || float.IsNaN(z))
                {
                    st[6] = 1f;
                    return;
                }

                float hx = x - d.H1X[i], hy = y - d.H1Y[i], hz = z - d.H1Z[i];
                if (hx * hx + hy * hy + hz * hz > 4f)
                {
                    st[6] = 2f;
                }

                if (d.Free[i] <= 0f)
                {
                    continue;
                }

                st[0] += 1f;
                float deepest = 0f;
                int part = -1;
                for (int k = 0; k < d.Shapes; k++)
                {
                    Now w = d.Work[k];
                    float cx = x - w.Cx, cy = y - w.Cy, cz = z - w.Cz;
                    if (cx * cx + cy * cy + cz * cz > w.Bound * w.Bound)
                    {
                        continue;
                    }

                    float inside;
                    if (w.Kind == 2)
                    {
                        float lx = w.M00 * cx + w.M01 * cy + w.M02 * cz;
                        float ly = w.M10 * cx + w.M11 * cy + w.M12 * cz;
                        float lz = w.M20 * cx + w.M21 * cy + w.M22 * cz;
                        float dx = w.Ex - Mathf.Abs(lx), dy = w.Ey - Mathf.Abs(ly), dz = w.Ez - Mathf.Abs(lz);
                        inside = dx > 0f && dy > 0f && dz > 0f ? Mathf.Min(dx, Mathf.Min(dy, dz)) : 0f;
                    }
                    else
                    {
                        float along = ((x - w.Ax) * w.Dx + (y - w.Ay) * w.Dy + (z - w.Az) * w.Dz) * w.InvLen2;
                        along = along < 0f ? 0f : along > 1f ? 1f : along;
                        float ex = x - w.Ax - w.Dx * along, ey = y - w.Ay - w.Dy * along, ez = z - w.Az - w.Dz * along;
                        inside = w.R - Mathf.Sqrt(ex * ex + ey * ey + ez * ez);
                    }

                    if (inside > deepest)
                    {
                        deepest = inside;
                        part = w.Part;
                    }
                }

                if (deepest > 0f)
                {
                    st[1] += 1f;
                    st[3] = Mathf.Max(st[3], deepest);
                    if (part < 0 || part >= 16 || d.Hidden[part] == 0)
                    {
                        st[2] += 1f;
                        seen = true;
                    }
                }
            }

            if (seen)
            {
                st[7] += 1f;
            }

            for (int k = 0; k < d.Threads; k++)
            {
                if (d.TKind[k] == 1 || d.TL[k] < 1e-5f)
                {
                    continue;
                }

                int i = d.TA[k], j = d.TB[k];
                float dx = d.X[i] - d.X[j], dy = d.Y[i] - d.Y[j], dz = d.Z[i] - d.Z[j];
                float s = Mathf.Sqrt(dx * dx + dy * dy + dz * dz) / d.TL[k] - 1f;
                st[4] = Mathf.Max(st[4], s);
                if (s > 1f)
                {
                    st[5] = 1f;
                }

                if (s > 8f)
                {
                    st[6] = 3f;
                }
            }
        }
    }
}
