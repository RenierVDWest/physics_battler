using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Combat
{
    /// <summary>
    /// Milestone 10: a blow of the off hand (the left): the jab, later the shield bash. The striking
    /// part (the knuckles, the shield's face) goes straight from the hand's guard to the aim point and
    /// a little through, and straight back, on the off arm (posed by <c>PosePipeline</c>); the weapon
    /// arm keeps its guard. This locks the aim and works out the lunge: the step in that brings the
    /// lead shoulder, turned in behind the blow at contact, within the arm's reach of the aim point.
    /// Whether it lands is the planner's dry run, as for every blow.
    /// </summary>
    public static class OffHandPath
    {
        /// <summary>How much of the arm's full length a blow reaches with (the elbow a little bent at contact: a jab doesn't lock out).</summary>
        public const float ReachShare = 0.9f;

        /// <summary>How far past the palm's grip the knuckles are (metres, medium size).</summary>
        public const float KnuckleBeyondGrip = 0.025f;

        public static MoveState Prepare(UnitController attacker, MoveDefinition move, WeaponDefinition weapon, Vector3 aim, AimPart part, object target)
        {
            var anim = attacker.Anim;
            float size = attacker.Body.SizeFactor;
            Vector3 root = attacker.Root.position;
            Vector3 flat = aim - root;
            flat.y = 0f;
            Vector3 forward = flat.sqrMagnitude > 1e-6f ? flat.normalized : attacker.Root.forward;
            var face = Quaternion.LookRotation(forward, Vector3.up);

            // (The lead shoulder where it will be at contact: turned in with the body's turn into the
            // blow, round the spine, then carried by the lunge.)
            Vector3 shoulder = anim.Part(PartId.UpperArmL).position;
            Vector3 aboutSpine = shoulder - root;
            aboutSpine = Quaternion.Inverse(Quaternion.LookRotation(Vector3.ProjectOnPlane(attacker.Root.forward, Vector3.up).sqrMagnitude > 1e-6f
                ? Vector3.ProjectOnPlane(attacker.Root.forward, Vector3.up) : forward, Vector3.up)) * aboutSpine;
            float contact = move.ContactTime();
            Vector3 shoulderLocal = Quaternion.Euler(0f, move.TurnAt(contact), 0f) * aboutSpine;
            Vector3 toAim = aim - (root + face * shoulderLocal);

            float arm = ArmLength(anim) + KnuckleReach(anim, size);
            float reach = ReachShare * arm;
            float rise = toAim.y;
            float side = Vector3.Dot(toAim, face * Vector3.right);
            float ahead = Vector3.Dot(toAim, forward);
            float across = Mathf.Sqrt(Mathf.Max(reach * reach - rise * rise - side * side, (0.15f * size) * (0.15f * size)));
            float lunge = ahead - across;
            if (lunge > move.maxLunge * size || lunge < -0.4f * size)
            {
                return null;
            }

            // The aim frame looks from the chest (where it will be at contact) to the aim point.
            float chestHeight = anim.Part(PartId.Chest).position.y;
            float pitch = Mathf.Atan2(aim.y - chestHeight, Mathf.Max(1e-3f, Vector3.Dot(aim - root, forward) - lunge)) * Mathf.Rad2Deg;
            pitch = Mathf.Clamp(pitch, -MovePlanner.MaxTiltDown, MovePlanner.MaxTiltUp);
            var frame = face * Quaternion.Euler(-pitch, 0f, 0f);
            return new MoveState
            {
                Move = move,
                Weapon = weapon,
                Target = target,
                Part = part,
                Aim = aim,
                Frame = frame,
                Lunge = forward * lunge,
                Scale = size,
                WidthScale = attacker.Body.WidthFactor,
                Duration = move.Duration * Mathf.Sqrt(size),
                ContactTime = contact,
                BodyPeak = move.BodyAt(contact),
                DriveStart = MovePlanner.DriveStart(move, contact),
                ThrustLine = toAim.sqrMagnitude > 1e-6f ? toAim.normalized : forward,
                FullReach = arm,
            };
        }

        /// <summary>The off arm's length, shoulder to wrist.</summary>
        public static float ArmLength(AnimRig anim) => anim.RestLocalPosition(PartId.ForearmL).magnitude + anim.RestLocalPosition(PartId.HandL).magnitude;

        /// <summary>The knuckles in the off hand's own frame (along the forearm's line, past the grip).</summary>
        public static Vector3 KnuckleLocal(AnimRig anim, float size) =>
            PhysicsBattler.Motion.ArmRig.WristFrame(anim, false) * Vector3.right * KnuckleReach(anim, size);

        /// <summary>How far the knuckles are from the wrist.</summary>
        public static float KnuckleReach(AnimRig anim, float size) =>
            (anim.SlotWeaponL != null ? anim.SlotWeaponL.localPosition.magnitude : 0.085f * size) + KnuckleBeyondGrip * size;
    }
}
