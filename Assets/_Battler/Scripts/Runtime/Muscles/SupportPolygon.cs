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
        private static float Cross(Vector2 o, Vector2 a, Vector2 b) => (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);
    }
}
