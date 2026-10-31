using UnityEngine;

namespace PhysicsBattler.Combat
{
    /// <summary>What a unit holds: its main weapon and off-hand item, and how much they are scaled.</summary>
    public sealed class Loadout
    {
        public WeaponDefinition Main;
        public WeaponDefinition Off;

        /// <summary>Weapons are built for the medium body; units scale them by their size.</summary>
        public float Scale = 1f;

        public bool HasShield => Off != null && Off.IsShield;
    }

    /// <summary>
    /// One blow under way: the move, the aim point locked where the body part was when it started,
    /// the aim frame and lunge worked out then, and how far the move has got. Plain data, so a blow
    /// can be tried out (planned) and thrown away.
    /// </summary>
    public sealed class MoveState
    {
        public MoveDefinition Move;
        public WeaponDefinition Weapon;
        public object Target;              // the unit aimed at (a UnitController; kept untyped for the pure maths)
        public AimPart Part;

        /// <summary>The aim point, locked at the start (world).</summary>
        public Vector3 Aim;

        /// <summary>Aim frame: z away from the attacker (horizontal), y up.</summary>
        public Quaternion Frame;

        /// <summary>The whole lunge at contact (world, horizontal).</summary>
        public Vector3 Lunge;

        /// <summary>Path and weapon scale: the attacker's size.</summary>
        public float Scale = 1f;

        /// <summary>Sideways path scale: the attacker's breadth (shoulder width against the medium body).</summary>
        public float WidthScale = 1f;

        public float Duration = 1f;
        public float Time;
        public float ContactTime;          // normalized
        public float BodyPeak = 1f;        // the body curve at contact, so the lunge is whole there
        public float DriveStart;           // normalized: when the body curve turns forward after the wind-up (the lunge's steps start)

        public bool ContactPassed;
        public bool SteppedOut, LeadOut, SteppedBack, RearMoved, RearOut, RearBack;
        public float LeadOutAt;

        /// <summary>The striking part's position the step before (animated and physical), to judge contact on the sweep.</summary>
        public Vector3 LastStrike, LastPhysical;
        public bool HasLastStrike, HasLastPhysical;

        /// <summary>
        /// The physical weapon's closest pass to the aim point during the active window (metres; -1
        /// before it is measured). It trails the pose by a physics step, so it is judged over the
        /// whole window, not at the contact moment.
        /// </summary>
        public float PhysicalClosest = -1f;

        public float T => Duration > 0f ? Mathf.Clamp01(Time / Duration) : 1f;
        public bool Done => Time >= Duration;
        public float Progress => Move.ProgressAt(T);
        public float Blend => Move.BlendAt(T);
        public float Body => Move.BodyAt(T) / (Mathf.Abs(BodyPeak) > 1e-4f ? BodyPeak : 1f);
        public bool Active => Progress >= Move.activeWindow.x && Progress <= Move.activeWindow.y;

        /// <summary>A point on the path (world), at a progress along it.</summary>
        public Vector3 PathPoint(float progress)
        {
            Vector3 p = Move.PointAt(progress);
            return Aim + Frame * new Vector3(p.x * WidthScale, p.y * Scale, p.z * Scale);
        }

        public MoveState CopyAt(float normalizedTime)
        {
            var copy = (MoveState)MemberwiseClone();
            copy.Time = normalizedTime * Duration;
            copy.HasLastStrike = copy.HasLastPhysical = false;
            copy.PhysicalClosest = -1f;
            return copy;
        }
    }
}
