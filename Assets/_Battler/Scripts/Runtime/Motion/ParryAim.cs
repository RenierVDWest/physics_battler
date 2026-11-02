using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// Milestone 11: where the parry controller wants the weapon (a club or a stick set across a
    /// seen blow's path, as a shield is): the point on the blow's path the blade should cross, and
    /// which way the blade lies there. The pose puts the weapon's middle on the point, blends it in
    /// over the guard by <see cref="weight"/>, and solves the arm as for any guard.
    /// </summary>
    public struct ParryAim
    {
        /// <summary>0: the guard; 1: set across the blow.</summary>
        public float weight;

        /// <summary>The point the blade crosses (world).</summary>
        public Vector3 point;

        /// <summary>Which way the blade lies through the point, grip to tip (world, unit).</summary>
        public Vector3 blade;
    }
}
