using PhysicsBattler.Locomotion;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>
    /// Branch locomotion-lab, the walk joined to the fight (the plan's "Joining the arms and combat"):
    /// while the unit is free to walk (no blow, not knocked back, standing), the new locomotion
    /// (<see cref="Locomotor"/>, <see cref="LocoBody"/>) animates the root the fight moves: the feet
    /// locked where they land, the pelvis, spine, chest and head as the reference walk has them. The
    /// pose pipeline blends its own lower body and torso into the walk's by <see cref="WalkWeight"/>, and
    /// the Fight Rhythm, the guard, the look and the arms go on top as before. A blow, a knock-back or
    /// a stumble hands the feet back to the stance where the walk left them; free again, the walk takes
    /// them over where the stance has them.
    /// </summary>
    public partial class UnitController
    {
        /// <summary>The new walk in the fight (off: the old gait and stance). Off until the join is taken step by step in the lab.</summary>
        public static bool NewWalk = false;

        public Locomotor Walk { get; private set; }
        public LocoBody WalkBody { get; private set; }

        private bool walkOwnsFeet;

        /// <summary>The walk has the feet now (its locks say which are planted).</summary>
        public bool WalkOwnsFeet => walkOwnsFeet;
    }
}
