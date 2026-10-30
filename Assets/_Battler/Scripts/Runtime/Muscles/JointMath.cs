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

    public static class JointLimits
    {
        /// <summary>Starting limits for a humanoid in a relaxed stand (the design spec's table, signed).</summary>
        public static JointLimitSpec For(PartId part)
        {
            switch (part)
            {
                case PartId.Spine: return new JointLimitSpec(-25f, 40f, 30f, 25f);
                case PartId.Chest: return new JointLimitSpec(-20f, 30f, 30f, 20f);
                case PartId.Head: return new JointLimitSpec(-40f, 45f, 70f, 30f);
                case PartId.UpperArmL:
                case PartId.UpperArmR: return new JointLimitSpec(-160f, 50f, 80f, 100f);   // forward raise is negative
                case PartId.ForearmL:
                case PartId.ForearmR: return new JointLimitSpec(-145f, 5f, 60f, 10f);      // the elbow bends forward
                case PartId.HandL:
                case PartId.HandR: return new JointLimitSpec(-70f, 70f, 40f, 45f);
                case PartId.ThighL:
                case PartId.ThighR: return new JointLimitSpec(-110f, 35f, 40f, 45f);       // forward raise is negative
                case PartId.ShinL:
                case PartId.ShinR: return new JointLimitSpec(-5f, 145f, 8f, 8f);            // the knee bends backward
                case PartId.FootL:
                case PartId.FootR: return new JointLimitSpec(-40f, 45f, 15f, 25f);
                default: return new JointLimitSpec(0f, 0f, 0f, 0f);
            }
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
