using System.Collections.Generic;
using UnityEngine;

namespace PhysicsBattler.Muscles
{
    /// <summary>
    /// The area on the ground a standing unit is supported by: the convex hull of the soles of the
    /// feet that are down. A body is balanced while its capture point is inside it; how far outside
    /// it lies is the balance error. (Before, the error was the distance to the midpoint between the
    /// feet, which ignored the stance: a wide, staggered stance is supported in every direction.)
    /// Works on the ground plane, x and z as a Vector2's x and y.
    /// </summary>
    public static class SupportPolygon
    {
        /// <summary>Adds a foot's sole (a rectangle) to <paramref name="points"/>.</summary>
        public static void AddSole(List<Vector2> points, Vector3 ankle, Quaternion footRotation, float length, float width, float forwardOffset)
        {
            Vector3 forward = footRotation * Vector3.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 1e-6f)
            {
                forward = Vector3.forward;
            }

            forward.Normalize();
            var f = new Vector2(forward.x, forward.z);
            var r = new Vector2(f.y, -f.x);
            var centre = new Vector2(ankle.x, ankle.z) + f * forwardOffset;
            float halfLength = length * 0.5f;
            float halfWidth = width * 0.5f;
            points.Add(centre + f * halfLength + r * halfWidth);
            points.Add(centre + f * halfLength - r * halfWidth);
            points.Add(centre - f * halfLength + r * halfWidth);
            points.Add(centre - f * halfLength - r * halfWidth);
        }

        /// <summary>Convex hull, counter-clockwise (Andrew's monotone chain). Reuses <paramref name="hull"/>.</summary>
        public static void Hull(List<Vector2> points, List<Vector2> hull)
        {
            hull.Clear();
            if (points.Count < 3)
            {
                hull.AddRange(points);
                return;
            }

            points.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
            for (int pass = 0; pass < 2; pass++)
            {
                int start = hull.Count;
                for (int k = 0; k < points.Count; k++)
                {
                    var p = points[pass == 0 ? k : points.Count - 1 - k];
                    while (hull.Count >= start + 2 && Cross(hull[hull.Count - 2], hull[hull.Count - 1], p) <= 0f)
                    {
                        hull.RemoveAt(hull.Count - 1);
                    }

                    hull.Add(p);
                }

                hull.RemoveAt(hull.Count - 1);
            }
        }

        private static float Cross(Vector2 o, Vector2 a, Vector2 b) => (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);

        /// <summary>How far <paramref name="p"/> is outside the counter-clockwise hull (0 inside).</summary>
        public static float DistanceOutside(List<Vector2> hull, Vector2 p)
        {
            if (hull.Count == 0)
            {
                return 0f;
            }

            if (hull.Count == 1)
            {
                return Vector2.Distance(hull[0], p);
            }

            bool inside = hull.Count >= 3;
            float best = float.MaxValue;
            for (int i = 0; i < hull.Count; i++)
            {
                var a = hull[i];
                var b = hull[(i + 1) % hull.Count];
                if (Cross(a, b, p) < 0f)
                {
                    inside = false;
                }

                var ab = b - a;
                float t = ab.sqrMagnitude > 1e-12f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / ab.sqrMagnitude) : 0f;
                best = Mathf.Min(best, Vector2.Distance(a + ab * t, p));
            }

            return inside ? 0f : best;
        }

        public static Vector2 Centroid(List<Vector2> hull)
        {
            var sum = Vector2.zero;
            foreach (var p in hull)
            {
                sum += p;
            }

            return hull.Count > 0 ? sum / hull.Count : sum;
        }
    }
}
