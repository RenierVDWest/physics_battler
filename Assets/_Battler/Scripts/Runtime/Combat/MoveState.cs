using System.Collections.Generic;
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

        /// <summary>Milestone 10: what the off hand strikes with: what it holds, or its bare fist (<see cref="WeaponDefinition.BareFist"/>).</summary>
        public WeaponDefinition OffHand => Off != null ? Off : WeaponDefinition.BareFist;

        /// <summary>The weapon a move is thrown with: the main weapon, or the off hand's.</summary>
        public WeaponDefinition For(MoveDefinition move) => move != null && move.hand == MoveHand.Off ? OffHand : Main;
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

        /// <summary>Why the blow ended early (stopped, flinched, staggered, killed), or null.</summary>
        public string Cancelled;
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
        public float Blend => Move.BlendAt(T) * Fade;

        /// <summary>1 while the blow plays; a cut-short blow fades to 0 from where it stopped (milestone 3).</summary>
        public float Fade = 1f;

        /// <summary>A blow stopped by what it struck bounces back: it plays backwards as it fades.</summary>
        public bool Rewind;

        /// <summary>Milestone 9c: how far back a stopped blow may play (its time, seconds): just back out of what it met, never into its wind-up.</summary>
        public float RewindFloor;
        public float Body => Move.BodyAt(T) / (Mathf.Abs(BodyPeak) > 1e-4f ? BodyPeak : 1f);
        public bool Active => Progress >= Move.activeWindow.x && Progress <= Move.activeWindow.y;

        /// <summary>A point on the path (world), at a progress along it.</summary>
        public Vector3 PathPoint(float progress)
        {
            if (Path != null && Path.Length > 1)
            {
                return MoveDefinition.CatmullRom(Path, progress);
            }

            Vector3 p = Move.PointAt(progress);
            return Aim + Frame * new Vector3(p.x * WidthScale, p.y * Scale, p.z * Scale);
        }

        // ---- Swing and Thrust shapes: the path laid out for this blow (SwingPath) ---------------

        /// <summary>The striking part's path (world), evenly in the move's travel; null for the Path shape.</summary>
        public Vector3[] Path;

        /// <summary>Where the swing turns about at each path point (world): the body's pivot as the body moves.</summary>
        public Vector3[] Pivots;

        /// <summary>The swing plane's normal (world): the weapon turns about it. Thrusts: unused.</summary>
        public Vector3 PlaneNormal = Vector3.up;

        /// <summary>The line a thrust is driven along (world, unit).</summary>
        public Vector3 ThrustLine = Vector3.forward;

        /// <summary>The weapon's reach from the pivot at full stretch (metres).</summary>
        public float FullReach;

        /// <summary>How fast the blow plays (1; a cleave through a body slows it, and it picks up again).</summary>
        public float TimeScale = 1f;

        public bool Generated => Path != null && Path.Length > 1;

        /// <summary>What the posed swing did (for the log): its speed through contact and at its fastest (m/s), how far past the aim point it carried (metres) and the arc it swept from the release to the end of the follow-through (degrees).</summary>
        public float ContactSpeed, PeakSpeed, Depth, Arc;

        /// <summary>The way the striking part was going through the striking moment (posed).</summary>
        public Vector3 ContactWay;

        /// <summary>
        /// Milestone 9c: the weapon arm's shape at the striking moment as the planner tried it (its
        /// joints' angles; NaN until tried). Shared with the copies the planner tries, which fill it;
        /// the playing arm is drawn toward it as the moment comes.
        /// </summary>
        public readonly float[] ContactArm = { float.NaN, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };

        /// <summary>
        /// The arm rebuild: where the hand is at the striking moment, from the aim, in the aim frame
        /// (metres): solved when the blow is planned, so the striking part lands on the aim with the
        /// contact's wrist and elbow keys.
        /// </summary>
        public Vector3 ContactHand;

        public bool HasContactHand;

        /// <summary>How far the striking part still lands from the aim with the planned contact hand (metres): out of reach, or past the wrist's keys.</summary>
        public float ContactHandMiss;

        /// <summary>
        /// The arm rebuild: how far the contact hand was moved, played, to land as the body really
        /// stood at the striking moment; kept from then on, so the follow-through carries on from the
        /// blow that landed rather than from one found again each step.
        /// </summary>
        public Vector3 ContactShift;

        public bool HasContactShift;

        /// <summary>Where the shoulder was in the planned contact pose (world; for measuring how the played blow differs).</summary>
        public Vector3 ContactShoulder;

        /// <summary>The performance pass: the striking part at the planner's clearance poses (normalized times; from where the body stood then), for the posed path to reuse.</summary>
        public float[] PosedTimes;
        public Vector3[] PosedStrikes;
        public int PosedCount;

        /// <summary>The performance pass: the posed path's samples, all posed already (the planner's, a few a step): progress and the striking part from where the body stood.</summary>
        public List<(float at, Vector3 strike)> PosedPath;
        public Vector3 LastRadial;
        public float LastProgress, LastDeviation;
        public bool HasLastRadial;

        public Vector3 PivotAt(float progress) => Pivots != null && Pivots.Length > 1 ? MoveDefinition.CatmullRom(Pivots, progress) : Aim;

        /// <summary>The hips' and the chest's turn now (degrees, + right), the lean (degrees forward), the crouch (share of the leg), the drive past contact (metres) and the wrist's lag (degrees).</summary>
        public float HipTurn => Move.HipTurnAt(T);
        public float ChestTurn => Move.ChestTurnAt(T);
        public float Lean => Move.LeanAt(T);
        public float Crouch => Move.CrouchAt(T);
        public float Drive => Move.DriveAt(T) * Scale;
        public float Lag => Move.LagAt(T);

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
