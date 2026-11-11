using PhysicsBattler.Combat;
using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>
    /// Milestone 11: the weapon parry. A unit without a shield meets a blow it has seen with its
    /// weapon, as a shield-bearer raises its shield: after its reaction time the blade is set across
    /// the blow's path where it passes a little out from the chest, lying square to the way the blow
    /// travels (upright against a swing from the side, flat over the head against one from above).
    /// The weapon arm is posed exactly while it parries. A blow whose blade meets the parrying weapon
    /// is stopped there (the sweep's weapon-on-weapon contact): no damage, and more of its poise
    /// comes through than through a shield.
    /// </summary>
    public partial class UnitController
    {
        /// <summary>How far the weapon is set across an incoming blow (0 to 1).</summary>
        public float ParryBlend { get; private set; }

        /// <summary>The blow it means to parry, who threw it, and when the weapon goes up and comes down.</summary>
        private MoveState parryMove;
        private UnitController parryAttacker;
        private float parryFrom, parryUntil;

        /// <summary>Can it parry at all: a weapon that isn't a shield, in a hand it still has.</summary>
        public bool CanParry => Loadout != null && Loadout.Main != null && !Loadout.Main.IsShield && Loadout.Main.kind != WeaponKind.Fist
            && MainWeapon != null && !Rig.IsSevered(PartId.HandR);

        /// <summary>A seen blow: it will set its weapon across it after its reaction time.</summary>
        private void StartParry(UnitController attacker, MoveState move, float reaction)
        {
            parryMove = move;
            parryAttacker = attacker;
            parryFrom = LocalTime + reaction;
            parryUntil = LocalTime + Mathf.Max(0f, move.Duration * Mathf.Min(1f, move.Move.activeWindow.y + 0.1f) - move.Time);
        }

        /// <summary>The parry let go at once (knocked down, struck hard, killed).</summary>
        private void DropParry()
        {
            parryMove = null;
            parryAttacker = null;
            ParryBlend = 0f;
            Pose.Parry = default;
        }
    }
}
