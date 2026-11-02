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

        /// <summary>Seconds the weapon takes to come across (and to go back).</summary>
        public const float ParryRise = 0.12f;

        /// <summary>A parry is set this far out from the chest along the blow's path (per unit of body size).</summary>
        public const float ParryOut = 0.45f;

        /// <summary>A parry meets the incoming blade this share of its length in from its striking part.</summary>
        public const float ParryInside = 0.25f;

        /// <summary>A parry's crossing point is at least this far in front of the chest (per unit of body size), and this much more per unit of size it lies across to the off side.</summary>
        public const float ParryFront = 0.35f, ParryFrontAcross = 0.8f;

        /// <summary>Is the weapon set across blows now (a blow meeting it is parried)?</summary>
        public bool Parrying => ParryBlend >= 0.5f;

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

        /// <summary>Each step, before the pose: where the weapon goes, and whether it is across a blow.</summary>
        private void UpdateParry(float dt)
        {
            bool up = false;
            var aim = Pose.Parry;
            if (parryMove != null && parryAttacker != null && parryAttacker.CurrentMove == parryMove && !parryMove.ContactPassed
                && IsAlive && CanAct && !IsAttacking && CanParry && LocalTime >= parryFrom && LocalTime <= parryUntil)
            {
                Vector3 chest = Anim.Part(PartId.Chest).position;
                float size = Body.SizeFactor;
                Vector3 facingForward = Quaternion.Euler(0f, facingYaw, 0f) * Vector3.forward;
                Vector3 facingRight = Quaternion.Euler(0f, facingYaw, 0f) * Vector3.right;

                // Where the incoming blade will be as its striking part comes within reach of the
                // chest: the striking part there, and the blade back from it toward the attacker's
                // hand (a swing's: from the pivot it turns about; a thrust's: along its line).
                var m = parryMove;
                float contact = m.Move.ProgressAt(m.ContactTime);
                float p = contact;
                for (int i = 1; i <= 12; i++)
                {
                    p = Mathf.Max(0f, contact - 0.04f * i);
                    if ((m.PathPoint(p) - chest).magnitude >= ParryOut * size)
                    {
                        break;
                    }
                }

                Vector3 strike = m.PathPoint(p);
                Vector3 along = m.Move.shape == MoveShape.Thrust ? m.ThrustLine
                    : m.Generated && m.Pivots != null ? strike - m.PivotAt(p)
                    : strike - parryAttacker.Anim.Part(PartId.HandR).position;
                along = along.sqrMagnitude > 1e-6f ? along.normalized : -facingForward;
                float inLength = (m.Weapon != null ? m.Weapon.length : 0.8f) * (parryAttacker.Loadout != null ? parryAttacker.Loadout.Scale : 1f);
                Vector3 at = strike - along * (ParryInside * inLength);

                // (In front of the body, the more so the further across to the off side: the weapon arm
                // reaches across in front of the chest, not through it.)
                float across = Mathf.Max(0f, -Vector3.Dot(at - chest, facingRight)) / Mathf.Max(0.1f, size);
                float front = (ParryFront + ParryFrontAcross * Mathf.Min(across, 0.5f)) * size;
                float ahead = Vector3.Dot(at - chest, facingForward);
                if (ahead < front)
                {
                    at += facingForward * (front - ahead);
                }

                Vector3 toAttacker = parryAttacker.Anim.Part(PartId.Chest).position - chest;
                aim.point = at;
                aim.blade = ParryBlade(along, toAttacker, facingRight);
                up = true;
            }

            ParryBlend = Mathf.MoveTowards(ParryBlend, up ? 1f : 0f, dt / ParryRise);
            aim.weight = ParryBlend;
            Pose.Parry = aim;
            if (parryMove != null && LocalTime > parryUntil && ParryBlend <= 0f)
            {
                parryMove = null;
                parryAttacker = null;
            }
        }

        /// <summary>
        /// Which way a parrying blade lies (grip to tip): square to the incoming blade and to the way
        /// to the attacker (across it, in the plane that faces the attacker); tip up when it stands, across to the left (the grip on the weapon side)
        /// when it lies flat.
        /// </summary>
        public static Vector3 ParryBlade(Vector3 travel, Vector3 toAttacker, Vector3 right)
        {
            Vector3 blade = Vector3.Cross(travel, toAttacker);
            if (blade.sqrMagnitude < 1e-8f)
            {
                return Vector3.up;
            }

            blade.Normalize();
            if (Mathf.Abs(blade.y) > 0.35f)
            {
                return blade.y < 0f ? -blade : blade;
            }

            return Vector3.Dot(blade, right) > 0f ? -blade : blade;
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
