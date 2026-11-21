using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// A piece of cloth (the plan "Cloth that moves", 21 November): a net of points joined by threads, moved by gravity
    /// and its own speed, then put right several times over: a thread too long or too short pulls its points back, a
    /// point inside the body is pushed out, a pinned point goes back to its pin, a point too far from its place is kept
    /// on its leash. Position based (XPBD), in many small steps a picture. Plain C#, no Unity physics: it can be run
    /// and measured headless. (Its sums are written out in plain numbers: they run tens of thousands of times a picture.)
    ///
    /// The six safeguards against cloth in the body: pushed out of each part's own shape to its surface and a gap;
    /// small steps (the body's shapes moved on through each); a speed limit; a leash to its home place; it remembers
    /// its side (pushed out the way it came from); and the covered parts are hidden (<see cref="Clothes"/>). Under them a
    /// net: a garment gone wrong is quietly put back to hanging.
    /// </summary>
    public sealed class ClothNet
    {
        // ---- The points ---------------------------------------------------------------------------------------------

        public readonly int Count;

        /// <summary>Where the points are (world). Its owner may set them and then call <see cref="Settle"/>.</summary>
        public readonly Vector3[] X;

        /// <summary>0 for a pinned point; 1 for a free one.</summary>
        public readonly float[] Free;

        /// <summary>Where each point is pinned, or its home place for the leash (world, set each picture by its owner).</summary>
        public readonly Vector3[] Home;

        /// <summary>How far each point may stray from its home place (metres).</summary>
        public readonly float[] Leash;

        private readonly float[] x, y, z, bx, by, bz, sx, sy, sz, hx0, hy0, hz0, hx1, hy1, hz1;
        private bool homeKnown;

        private int[] ta = new int[64], tb = new int[64];
        private float[] tl = new float[64];
        private bool[] tbend = new bool[64];
        private int threads;

        // ---- What it meets ------------------------------------------------------------------------------------------

        /// <summary>A shape of the body, as it was at the start of the picture and as it is at its end (world).</summary>
        public struct Shape
        {
            public int Kind; // 0 sphere, 1 capsule, 2 box
            public Vector3 A0, B0, A1, B1;         // sphere: A its centre; capsule: A and B the ends' centres; box: A its centre
            public float Radius;
            public Quaternion Turn0, Turn1;        // box: its turn
            public Vector3 Half;                   // box: half its size
            public int Part;
        }

        public readonly List<Shape> Shapes = new List<Shape>(20);

        // (Each shape as it is in the small step: worked out once a step, not once a point.)
        private struct Now
        {
            public int Kind;
            public float Ax, Ay, Az, Dx, Dy, Dz, InvLen2, R, Bound, Cx, Cy, Cz;
            public float Ex, Ey, Ez;
            public float M00, M01, M02, M10, M11, M12, M20, M21, M22; // box: world to its own (rows)
        }

        private Now[] now = new Now[20];

        /// <summary>The floor's height (world).</summary>
        public float Floor;

        // ---- Its settings --------------------------------------------------------------------------------------------

        public float Gap = 0.015f, Thickness = 0.02f, Air = 1f, Feel = 0.7f, Weight = 1f;
        public int Steps = 8;
        public Vector3 Gravity = new Vector3(0f, -9.81f, 0f), Wind;

        /// <summary>How much a thread gives, a small step (its compliance over the step's time squared, against a point's weight of 1: 0 is rigid, 1 meets it halfway).</summary>
        public float Stretch = 0.1f, Bending = 3f;

        // ---- What it measured ------------------------------------------------------------------------------------------

        /// <summary>Point-steps checked, and how many of them ended inside the body (inside its shape itself, past the gap).</summary>
        public long Checked, Inside;

        /// <summary>Of those inside, how many were inside a part that is drawn (not hidden under the clothes); and the pictures with any point inside a drawn part.</summary>
        public long InsideDrawn;

        public int PicturesInsideDrawn;

        /// <summary>Point-steps inside each part (by part number).</summary>
        public readonly long[] InsideBy = new long[16];

        /// <summary>The parts hidden under the clothes (by part number): a point inside one cannot be seen.</summary>
        public bool[] HiddenParts = new bool[16];

        /// <summary>The deepest a point ended inside the body (metres), the furthest a thread was stretched (share), resets.</summary>
        public float Deepest, MostStretch;

        public int Resets;

        /// <summary>Pictures in which a thread was stretched to twice its length (cloth caught between two parts of the body).</summary>
        public int Pinched;

        /// <summary>Why it was last put back to hanging.</summary>
        public string WhyReset = "";

        /// <summary>The last thread found stretched to twice its length (its two points).</summary>
        public string Worst = "";

        public ClothNet(int count)
        {
            Count = count;
            X = new Vector3[count];
            Free = new float[count];
            Home = new Vector3[count];
            Leash = new float[count];
            x = new float[count]; y = new float[count]; z = new float[count];
            bx = new float[count]; by = new float[count]; bz = new float[count];
            sx = new float[count]; sy = new float[count]; sz = new float[count];
            hx0 = new float[count]; hy0 = new float[count]; hz0 = new float[count];
            hx1 = new float[count]; hy1 = new float[count]; hz1 = new float[count];
            for (int i = 0; i < count; i++)
            {
                Free[i] = 1f;
                Leash[i] = 0.3f;
            }
        }

        /// <summary>A thread between two points at their distance now (in <see cref="X"/>); a bending one is softer.</summary>
        public void Join(int a, int b, bool bending = false)
        {
            if (threads == ta.Length)
            {
                System.Array.Resize(ref ta, threads * 2);
                System.Array.Resize(ref tb, threads * 2);
                System.Array.Resize(ref tl, threads * 2);
                System.Array.Resize(ref tbend, threads * 2);
            }

            ta[threads] = a;
            tb[threads] = b;
            tl[threads] = Vector3.Distance(X[a], X[b]);
            tbend[threads] = bending;
            threads++;
        }

        /// <summary>Every point put where <see cref="X"/> has it, with no speed.</summary>
        public void Settle()
        {
            for (int i = 0; i < Count; i++)
            {
                x[i] = bx[i] = sx[i] = X[i].x;
                y[i] = by[i] = sy[i] = X[i].y;
                z[i] = bz[i] = sz[i] = X[i].z;
            }
        }

        /// <summary>The pins' last places forgotten (the cloth was put back where they are now).</summary>
        public void Forget() => homeKnown = false;

        /// <summary>
        /// One picture: the body moved from where it was; <paramref name="carry"/> is that move for the part the cloth
        /// hangs from (a share of it is given to the cloth outright, so it does not feel all of his movement); then the
        /// small steps.
        /// </summary>
        public void Step(float dt, System.Func<Vector3, Vector3> carry)
        {
            if (dt <= 0f)
            {
                return;
            }

            for (int i = 0; i < Count; i++)
            {
                hx1[i] = Home[i].x;
                hy1[i] = Home[i].y;
                hz1[i] = Home[i].z;
            }

            if (!homeKnown)
            {
                System.Array.Copy(hx1, hx0, Count);
                System.Array.Copy(hy1, hy0, Count);
                System.Array.Copy(hz1, hz0, Count);
                homeKnown = true;
            }

            // (How much of his movement it feels: the rest is given to it with the body, speed and all.)
            if (carry != null && Feel < 1f)
            {
                float give = 1f - Mathf.Clamp01(Feel);
                for (int i = 0; i < Count; i++)
                {
                    if (Free[i] <= 0f)
                    {
                        continue;
                    }

                    Vector3 to = carry(new Vector3(x[i], y[i], z[i]));
                    Vector3 toB = carry(new Vector3(bx[i], by[i], bz[i]));
                    x[i] += give * (to.x - x[i]); y[i] += give * (to.y - y[i]); z[i] += give * (to.z - z[i]);
                    bx[i] += give * (toB.x - bx[i]); by[i] += give * (toB.y - by[i]); bz[i] += give * (toB.z - bz[i]);
                }
            }

            int n = Mathf.Max(1, Steps);
            float h = dt / n;
            float most = Thickness, most2 = most * most;
            float keep = Mathf.Exp(-Air * h);
            Vector3 pull = (Gravity + Wind / Mathf.Max(0.05f, Weight)) * (h * h);
            float a = Mathf.Max(0f, Stretch), ab = Mathf.Max(0f, Bending);
            if (now.Length < Shapes.Count)
            {
                now = new Now[Shapes.Count * 2];
            }

            for (int s = 0; s < n; s++)
            {
                float t = (s + 1f) / n;
                ShapesAt(t);
                for (int i = 0; i < Count; i++)
                {
                    sx[i] = x[i]; sy[i] = y[i]; sz[i] = z[i];
                    if (Free[i] <= 0f)
                    {
                        x[i] = bx[i] = hx0[i] + (hx1[i] - hx0[i]) * t;
                        y[i] = by[i] = hy0[i] + (hy1[i] - hy0[i]) * t;
                        z[i] = bz[i] = hz0[i] + (hz1[i] - hz0[i]) * t;
                        continue;
                    }

                    float vx = (x[i] - bx[i]) * keep, vy = (y[i] - by[i]) * keep, vz = (z[i] - bz[i]) * keep;
                    bx[i] = x[i]; by[i] = y[i]; bz[i] = z[i];
                    x[i] += vx + pull.x; y[i] += vy + pull.y; z[i] += vz + pull.z;
                }

                // (The threads: XPBD, one pass each small step.)
                for (int k = 0; k < threads; k++)
                {
                    int i = ta[k], j = tb[k];
                    float wi = Free[i], wj = Free[j], w = wi + wj;
                    if (w <= 0f)
                    {
                        continue;
                    }

                    float dx = x[i] - x[j], dy = y[i] - y[j], dz = z[i] - z[j];
                    float len = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
                    if (len < 1e-7f)
                    {
                        continue;
                    }

                    float lambda = -(len - tl[k]) / (w + (tbend[k] ? ab : a)) / len;
                    x[i] += wi * lambda * dx; y[i] += wi * lambda * dy; z[i] += wi * lambda * dz;
                    x[j] -= wj * lambda * dx; y[j] -= wj * lambda * dy; z[j] -= wj * lambda * dz;
                }

                // (Out of the body, the floor, the leash, the speed limit.)
                float floor = Floor + Gap;
                for (int i = 0; i < Count; i++)
                {
                    if (Free[i] <= 0f)
                    {
                        continue;
                    }

                    // (The speed limit and the leash first; then out of the body, last, so nothing after it puts the
                    // point back in.)
                    float mx = x[i] - sx[i], my = y[i] - sy[i], mz = z[i] - sz[i], m2 = mx * mx + my * my + mz * mz;
                    if (m2 > most2)
                    {
                        float f = most / Mathf.Sqrt(m2);
                        x[i] = sx[i] + mx * f; y[i] = sy[i] + my * f; z[i] = sz[i] + mz * f;
                    }

                    float hx = hx0[i] + (hx1[i] - hx0[i]) * t, hy = hy0[i] + (hy1[i] - hy0[i]) * t, hz = hz0[i] + (hz1[i] - hz0[i]) * t;
                    float ox = x[i] - hx, oy = y[i] - hy, oz = z[i] - hz, o2 = ox * ox + oy * oy + oz * oz, l = Leash[i];
                    if (o2 > l * l)
                    {
                        float f = l / Mathf.Sqrt(o2);
                        x[i] = hx + ox * f; y[i] = hy + oy * f; z[i] = hz + oz * f;
                    }

                    if (y[i] < floor)
                    {
                        y[i] = floor;
                        // (Friction on the floor: it does not slide on by itself.)
                        bx[i] += 0.5f * (x[i] - bx[i]);
                        bz[i] += 0.5f * (z[i] - bz[i]);
                    }

                    // (Twice if the first moved it: pushed out of one part it can land in the next, a thigh against a shin.)
                    if (PushOut(i))
                    {
                        PushOut(i);
                    }
                }
            }

            System.Array.Copy(hx1, hx0, Count);
            System.Array.Copy(hy1, hy0, Count);
            System.Array.Copy(hz1, hz0, Count);
            for (int i = 0; i < Count; i++)
            {
                X[i] = new Vector3(x[i], y[i], z[i]);
            }

            Measure();
        }

        private static Vector3 Lerp(Vector3 p, Vector3 q, float t) => p + (q - p) * t;

        private void ShapesAt(float t)
        {
            for (int k = 0; k < Shapes.Count; k++)
            {
                var sh = Shapes[k];
                var w = new Now { Kind = sh.Kind };
                if (sh.Kind == 2)
                {
                    Vector3 c = Lerp(sh.A0, sh.A1, t);
                    Quaternion turn = Quaternion.Slerp(sh.Turn0, sh.Turn1, t);
                    // (The box's own axes in the world: the rows of the world-to-own turn.)
                    Vector3 r0 = turn * Vector3.right, r1 = turn * Vector3.up, r2 = turn * Vector3.forward;
                    w.M00 = r0.x; w.M01 = r0.y; w.M02 = r0.z;
                    w.M10 = r1.x; w.M11 = r1.y; w.M12 = r1.z;
                    w.M20 = r2.x; w.M21 = r2.y; w.M22 = r2.z;
                    w.Cx = c.x; w.Cy = c.y; w.Cz = c.z;
                    w.Ex = sh.Half.x + Gap; w.Ey = sh.Half.y + Gap; w.Ez = sh.Half.z + Gap;
                    w.Bound = Mathf.Sqrt(w.Ex * w.Ex + w.Ey * w.Ey + w.Ez * w.Ez);
                }
                else
                {
                    Vector3 p = Lerp(sh.A0, sh.A1, t), q = sh.Kind == 1 ? Lerp(sh.B0, sh.B1, t) : p;
                    Vector3 d = q - p;
                    w.Ax = p.x; w.Ay = p.y; w.Az = p.z;
                    w.Dx = d.x; w.Dy = d.y; w.Dz = d.z;
                    float d2 = d.sqrMagnitude;
                    w.InvLen2 = d2 > 1e-10f ? 1f / d2 : 0f;
                    w.R = sh.Radius + Gap;
                    Vector3 c = 0.5f * (p + q);
                    w.Cx = c.x; w.Cy = c.y; w.Cz = c.z;
                    w.Bound = 0.5f * Mathf.Sqrt(d2) + w.R;
                }

                now[k] = w;
            }
        }

        /// <summary>A point pushed out of every shape of the body it is in, to its surface and the gap: the way it came from (its side).</summary>
        private bool PushOut(int i)
        {
            bool moved = false;
            int count = Shapes.Count;
            for (int k = 0; k < count; k++)
            {
                ref Now w = ref now[k];
                float px = x[i], py = y[i], pz = z[i];
                float cx = px - w.Cx, cy = py - w.Cy, cz = pz - w.Cz;
                if (cx * cx + cy * cy + cz * cz > w.Bound * w.Bound)
                {
                    continue;
                }

                if (w.Kind == 2)
                {
                    // (Into the box's own space.)
                    float lx = w.M00 * cx + w.M01 * cy + w.M02 * cz;
                    float ly = w.M10 * cx + w.M11 * cy + w.M12 * cz;
                    float lz = w.M20 * cx + w.M21 * cy + w.M22 * cz;
                    float dx = w.Ex - Mathf.Abs(lx), dy = w.Ey - Mathf.Abs(ly), dz = w.Ez - Mathf.Abs(lz);
                    if (dx <= 0f || dy <= 0f || dz <= 0f)
                    {
                        continue;
                    }

                    // (Out through the face it came in by: where it was at the start of the step, if that was outside.)
                    float qx = sx[i] - w.Cx, qy = sy[i] - w.Cy, qz = sz[i] - w.Cz;
                    float wx = w.M00 * qx + w.M01 * qy + w.M02 * qz;
                    float wy = w.M10 * qx + w.M11 * qy + w.M12 * qz;
                    float wz = w.M20 * qx + w.M21 * qy + w.M22 * qz;
                    float ox = Mathf.Abs(wx) - w.Ex, oy = Mathf.Abs(wy) - w.Ey, oz = Mathf.Abs(wz) - w.Ez;
                    bool outside = ox > 0f || oy > 0f || oz > 0f;
                    int axis = outside ? (ox >= oy && ox >= oz ? 0 : oy >= oz ? 1 : 2) : (dx <= dy && dx <= dz ? 0 : dy <= dz ? 1 : 2);
                    if (axis == 0) lx = (outside ? (wx >= 0f ? 1f : -1f) : (lx >= 0f ? 1f : -1f)) * w.Ex;
                    else if (axis == 1) ly = (outside ? (wy >= 0f ? 1f : -1f) : (ly >= 0f ? 1f : -1f)) * w.Ey;
                    else lz = (outside ? (wz >= 0f ? 1f : -1f) : (lz >= 0f ? 1f : -1f)) * w.Ez;
                    // (Back to the world.)
                    x[i] = w.Cx + w.M00 * lx + w.M10 * ly + w.M20 * lz;
                    y[i] = w.Cy + w.M01 * lx + w.M11 * ly + w.M21 * lz;
                    z[i] = w.Cz + w.M02 * lx + w.M12 * ly + w.M22 * lz;
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

                // (Its side: the way from the bone to where it was at the start of the step, if that was outside.)
                float fx = sx[i] - nx, fy = sy[i] - ny, fz = sz[i] - nz, f2 = fx * fx + fy * fy + fz * fz;
                if (f2 < w.R * w.R * 0.81f)
                {
                    fx = ex; fy = ey; fz = ez; f2 = e2;
                }

                if (f2 < 1e-12f)
                {
                    fx = 0f; fy = 1f; fz = 0f; f2 = 1f;
                }

                float g = w.R / Mathf.Sqrt(f2);
                x[i] = nx + fx * g; y[i] = ny + fy * g; z[i] = nz + fz * g;
                moved = true;
            }

            return moved;
        }

        /// <summary>What the picture left: how many points inside the body, how deep, how far a thread is stretched; and the net under it all.</summary>
        private void Measure()
        {
            string wrong = null;
            for (int i = 0; i < Count && wrong == null; i++)
            {
                if (float.IsNaN(x[i]) || float.IsNaN(y[i]) || float.IsNaN(z[i]))
                {
                    wrong = "a point lost (NaN)";
                }
                else if ((X[i] - Home[i]).sqrMagnitude > 4f)
                {
                    wrong = "a point 2 m from its place";
                }
            }

            // (Inside the body's shapes themselves (the gap apart), at the end of the picture.)
            for (int k = 0; k < Shapes.Count; k++)
            {
                ref Now w = ref now[k];
                if (w.Kind == 2)
                {
                    w.Ex -= Gap; w.Ey -= Gap; w.Ez -= Gap;
                }
                else
                {
                    w.R -= Gap;
                }
            }

            bool seen = false;
            for (int i = 0; i < Count; i++)
            {
                if (Free[i] <= 0f)
                {
                    continue;
                }

                Checked++;
                float deep = DepthNow(i, out int part);
                if (deep > 0f)
                {
                    Inside++;
                    if (part >= 0 && part < InsideBy.Length)
                    {
                        InsideBy[part]++;
                    }

                    Deepest = Mathf.Max(Deepest, deep);
                    if (part < 0 || part >= HiddenParts.Length || !HiddenParts[part])
                    {
                        InsideDrawn++;
                        seen = true;
                    }
                }
            }

            if (seen)
            {
                PicturesInsideDrawn++;
            }

            bool pinched = false;
            for (int k = 0; k < threads; k++)
            {
                if (tbend[k] || tl[k] < 1e-5f)
                {
                    continue;
                }

                float s = Vector3.Distance(X[ta[k]], X[tb[k]]) / tl[k] - 1f;
                if (s > 1f)
                {
                    Worst = $"{ta[k]}-{tb[k]} at {100f * s:0}%";
                }

                MostStretch = Mathf.Max(MostStretch, s);
                pinched |= s > 1f;
                if (s > 8f && wrong == null)
                {
                    wrong = $"a thread stretched {100f * s:0}% (points {ta[k]} and {tb[k]})";
                }
            }

            if (pinched)
            {
                Pinched++;
            }

            if (wrong != null)
            {
                Resets++;
                WhyReset = wrong;
                Reset?.Invoke();
            }
        }

        private float DepthNow(int i, out int part)
        {
            float deepest = 0f;
            part = -1;
            for (int k = 0; k < Shapes.Count; k++)
            {
                ref Now w = ref now[k];
                float cx = x[i] - w.Cx, cy = y[i] - w.Cy, cz = z[i] - w.Cz;
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
                    float inBox = Mathf.Min(dx, Mathf.Min(dy, dz));
                    if (dx > 0f && dy > 0f && dz > 0f && inBox > deepest)
                    {
                        deepest = inBox;
                        part = Shapes[k].Part;
                    }

                    continue;
                }

                float along = ((x[i] - w.Ax) * w.Dx + (y[i] - w.Ay) * w.Dy + (z[i] - w.Az) * w.Dz) * w.InvLen2;
                along = along < 0f ? 0f : along > 1f ? 1f : along;
                float ex = x[i] - w.Ax - w.Dx * along, ey = y[i] - w.Ay - w.Dy * along, ez = z[i] - w.Az - w.Dz * along;
                float inCap = w.R - Mathf.Sqrt(ex * ex + ey * ey + ez * ez);
                if (inCap > deepest)
                {
                    deepest = inCap;
                    part = Shapes[k].Part;
                }
            }

            return deepest;
        }

        /// <summary>Put back to hanging (its owner sets the points where they hang, then <see cref="Settle"/>).</summary>
        public System.Action Reset;
    }
}
