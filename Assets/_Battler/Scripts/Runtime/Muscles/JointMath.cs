using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Muscles
{
    /// <summary>
    /// Angle limits for one joint, in the part's local frame (X right, Y up, Z forward).
    /// Pitch is rotation about local X with the same sign as Quaternion.AngleAxis(angle, Vector3.right):
    /// positive pitch tips a vertical bone's top forward and swings a hanging bone backward.
    /// Twist (about Y) and swing (about Z) are symmetric.
    /// </summary>
    [System.Serializable]
    public struct JointLimitSpec
    {
        public float pitchMin;
        public float pitchMax;
        public float twist;
        public float swing;

        public JointLimitSpec(float pitchMin, float pitchMax, float twist, float swing)
        {
            this.pitchMin = pitchMin;
            this.pitchMax = pitchMax;
            this.twist = twist;
            this.swing = swing;
        }
    }

    public static class JointLimitTable
    {
        /// <summary>
        /// A person's joint ranges, a little inside them (the design spec's table, signed; milestone 9b
        /// set them against the research: no side limit under 15 degrees, which PhysX can't hold, the
        /// neck as the head's look has it, the forearm's roll at the wrist: PhysX can't keep a roll at a
        /// bent elbow).
        /// </summary>
        public static JointLimitSpec For(PartId part)
        {
            switch (part)
            {
                case PartId.Spine: return new JointLimitSpec(-25f, 40f, 30f, 25f);
                case PartId.Chest: return new JointLimitSpec(-20f, 30f, 30f, 20f);
                case PartId.Head: return new JointLimitSpec(-50f, 60f, 80f, 30f);          // up 50, down 60, turn 80, tilt 30
                case PartId.UpperArmL:
                case PartId.UpperArmR: return new JointLimitSpec(-160f, 50f, 90f, 130f);   // forward raise is negative; out to the side 130 (a guard high by the head)
                case PartId.ForearmL:
                case PartId.ForearmR: return new JointLimitSpec(-145f, 5f, 60f, 15f);      // the elbow bends forward; the forearm rolls 60 each way
                case PartId.HandL:
                case PartId.HandR: return new JointLimitSpec(-15f, 35f, 80f, 57f);       // toward the thumb 15, the little finger 35; the forearm's roll (shown at the wrist) 80; flexion and extension about 57
                case PartId.ThighL:
                case PartId.ThighR: return new JointLimitSpec(-110f, 35f, 45f, 45f);       // forward raise is negative
                case PartId.ShinL:
                case PartId.ShinR: return new JointLimitSpec(-5f, 145f, 15f, 15f);          // the knee bends backward
                case PartId.FootL:
                case PartId.FootR: return new JointLimitSpec(-40f, 45f, 25f, 25f);
                default: return new JointLimitSpec(0f, 0f, 0f, 0f);
            }
        }

        /// <summary>
        /// The limits on the ground: the same, but the hinges are hinges: the knee bends only one
        /// way and the elbow doesn't bend sideways (0: locked, once the joint has eased in).
        /// </summary>
        public static JointLimitSpec ForRagdoll(PartId part)
        {
            var spec = For(part);
            switch (part)
            {
                case PartId.ShinL:
                case PartId.ShinR:
                    spec.twist = 0f;
                    spec.swing = 0f;
                    break;
                case PartId.ForearmL:
                case PartId.ForearmR:
                    spec.swing = 0f;
                    break;
            }

            return spec;
        }
    }

    /// <summary>Joint maths shared by the builder, the muscles and the convention probe.</summary>
    public static class JointMath
    {
        /// <summary>
        /// The ConfigurableJoint.targetRotation that drives a part whose rest local rotation was
        /// <paramref name="restLocal"/> toward <paramref name="targetLocal"/>. The joint's axis must be
        /// Vector3.right and its secondary axis Vector3.up, so joint space equals the part's local space.
        /// </summary>
        public static Quaternion TargetRotation(Quaternion targetLocal, Quaternion restLocal)
        {
            var standard = Quaternion.Inverse(targetLocal) * restLocal;
            return JointConventions.DriveInverted ? Quaternion.Inverse(standard) : standard;
        }

        /// <summary>Applies a limit spec, honouring the measured sign of the twist (X) limit.</summary>
        public static void ApplyLimits(ConfigurableJoint joint, JointLimitSpec spec)
        {
            float low = spec.pitchMin;
            float high = spec.pitchMax;
            if (JointConventions.TwistLimitSign < 0)
            {
                low = -spec.pitchMax;
                high = -spec.pitchMin;
            }

            joint.lowAngularXLimit = new SoftJointLimit { limit = Mathf.Clamp(low, -177f, 177f) };
            joint.highAngularXLimit = new SoftJointLimit { limit = Mathf.Clamp(high, -177f, 177f) };
            joint.angularYLimit = new SoftJointLimit { limit = Mathf.Clamp(spec.twist, 0f, 177f) };
            joint.angularZLimit = new SoftJointLimit { limit = Mathf.Clamp(spec.swing, 0f, 177f) };
        }

        /// <summary>
        /// A joint's turn against its rest (<paramref name="local"/>: the child's rotation in its
        /// parent's frame, the rest taken out), split as PhysX does: the bend about X (signed, as
        /// <see cref="JointLimitSpec"/> has it) and the swing that follows, about Y and Z. How far
        /// past its limits it is (degrees; 0 inside).
        /// </summary>
        public static float PastLimits(Quaternion local, JointLimitSpec spec, out float pitch, out float aboutY, out float aboutZ)
        {
            if (local.w < 0f)
            {
                local = new Quaternion(-local.x, -local.y, -local.z, -local.w);
            }

            pitch = 2f * Mathf.Atan2(local.x, local.w) * Mathf.Rad2Deg;
            var twist = Quaternion.AngleAxis(pitch, Vector3.right);
            var swing = local * Quaternion.Inverse(twist);
            if (swing.w < 0f)
            {
                swing = new Quaternion(-swing.x, -swing.y, -swing.z, -swing.w);
            }

            aboutY = 2f * Mathf.Asin(Mathf.Clamp(swing.y, -1f, 1f)) * Mathf.Rad2Deg;
            aboutZ = 2f * Mathf.Asin(Mathf.Clamp(swing.z, -1f, 1f)) * Mathf.Rad2Deg;
            float past = Mathf.Max(spec.pitchMin - pitch, pitch - spec.pitchMax);
            past = Mathf.Max(past, Mathf.Abs(aboutY) - spec.twist);
            past = Mathf.Max(past, Mathf.Abs(aboutZ) - spec.swing);
            return Mathf.Max(0f, past);
        }

        /// <summary>Signed rotation angle about X (degrees) of a rotation that is mostly about X.</summary>
        public static float PitchOf(Quaternion q)
        {
            return 2f * Mathf.Atan2(q.x, q.w) * Mathf.Rad2Deg;
        }

        /// <summary>Wraps an angle into -180..180.</summary>
        public static float Wrap(float degrees)
        {
            degrees %= 360f;
            if (degrees > 180f)
            {
                degrees -= 360f;
            }
            else if (degrees < -180f)
            {
                degrees += 360f;
            }

            return degrees;
        }

        /// <summary>
        /// The angular velocity (rad/s, in <paramref name="from"/>'s own frame) that turns
        /// <paramref name="from"/> into <paramref name="to"/> in <paramref name="dt"/> seconds, the shortest way.
        /// </summary>
        public static Vector3 BodyAngularVelocity(Quaternion from, Quaternion to, float dt)
        {
            var delta = Quaternion.Inverse(from) * to;
            if (delta.w < 0f)
            {
                delta = new Quaternion(-delta.x, -delta.y, -delta.z, -delta.w);
            }

            var vector = new Vector3(delta.x, delta.y, delta.z);
            float sine = vector.magnitude;
            if (sine < 1e-7f || dt <= 0f)
            {
                return Vector3.zero;
            }

            float angle = 2f * Mathf.Atan2(sine, delta.w);
            return vector / sine * (angle / dt);
        }

        /// <summary>
        /// Spring and damper for an acceleration-based drive that should move a load
        /// <paramref name="boost"/> times the inertia PhysX scales the drive by, at <paramref name="hz"/>
        /// with damping ratio <paramref name="dampingRatio"/> (both hold for the load, not for the bare part).
        /// </summary>
        public static void DriveGains(float hz, float dampingRatio, float strength, float boost, out float spring, out float damper)
        {
            strength = Mathf.Max(0f, strength);
            float omega = 2f * Mathf.PI * hz;
            spring = strength * omega * omega * boost;
            damper = 2f * dampingRatio * omega * Mathf.Sqrt(strength) * boost;
        }

        /// <summary>
        /// The rotation needed to turn <paramref name="from"/> into <paramref name="to"/>, as an axis
        /// times angle in radians (shortest way round). Used for PD torques.
        /// </summary>
        public static Vector3 RotationError(Quaternion from, Quaternion to)
        {
            var delta = to * Quaternion.Inverse(from);
            if (delta.w < 0f)
            {
                delta = new Quaternion(-delta.x, -delta.y, -delta.z, -delta.w);
            }

            delta.ToAngleAxis(out float angle, out Vector3 axis);
            if (float.IsNaN(axis.x) || angle < 0.0001f)
            {
                return Vector3.zero;
            }

            return axis.normalized * (angle * Mathf.Deg2Rad);
        }
    }
}
