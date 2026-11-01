using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// Where the shield controller wants the shield (milestone 9), for the pose to place within the
    /// body's limits: which way from the chest it goes (toward the threat; the pose keeps it inside
    /// the <see cref="Combat.ShieldCone"/>), which way its face looks when it is set against a blow,
    /// and how far out.
    /// </summary>
    public struct ShieldAim
    {
        /// <summary>0: the plain guard of before (straight ahead of the hips); 1: under control.</summary>
        public float weight;

        /// <summary>From the chest toward where the shield goes (world, unit).</summary>
        public Vector3 direction;

        /// <summary>Which way the face looks against a blow (world, unit), and how much of that over the guard's own angle (0 to 1).</summary>
        public Vector3 face;
        public float faceBlend;

        /// <summary>How far out from the chest, against the guard's distance (1).</summary>
        public float reach;
    }
}
