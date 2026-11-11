using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>What the shield is doing (milestone 9).</summary>
    public enum ShieldState
    {
        None,

        /// <summary>Held toward the enemy it is minding (its chest).</summary>
        Guard,

        /// <summary>Following a seen wind-up: the enemy's weapon.</summary>
        Track,

        /// <summary>Set on a seen blow's path, its face against the weapon's travel.</summary>
        Block,

        /// <summary>Raised to the eyes (shelled up).</summary>
        High,
    }

    /// <summary>
    /// The shield under control (milestone 9). The shield always points at the threat the unit
    /// knows of: the chest of the enemy it is minding, that enemy's weapon while it winds up, a seen
    /// blow's path at contact. It moves there at a person's pace (a block quickly, back to the guard
    /// more slowly), and only inside the cone a shield arm can cover from the chest (the pose clamps
    /// it there and keeps it out of the body). A seen blow outside that cone turns the unit to face
    /// it; it can block once the blow is inside, if there is time. What it hasn't seen moves nothing.
    /// </summary>
    public partial class UnitController
    {
    }
}
