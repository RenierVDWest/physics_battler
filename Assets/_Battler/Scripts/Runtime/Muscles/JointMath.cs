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
                case PartId.HandR: return new JointLimitSpec(-15f, 30f, 80f, 40f);       // a gripping wrist (9c): toward the thumb 15, the little finger 30; the forearm's roll (shown at the wrist) 80; bend and extend 40
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
        /// Milestone 10 (the arm rebuild's step 1): the limits PhysX holds, in each joint's own frame
        /// (<see cref="JointFrames"/>): X the joint's axis, Y its secondary, Z the third. The arm's are
        /// the arm rig's own model: the elbow a hinge about the rig's elbow axis (bent forward up to
        /// <c>ArmRig.ElbowBendLimit</c>, back past straight by its slack), the wrist twisting about the
        /// forearm (the rig's twist) and bending in an ellipse about the thumb and the palm, round at
        /// the rig's flexion (its deviation, narrower and lopsided, is inside it whichever way PhysX
        /// turns the ellipse with the twist). So every arm the rig poses is one PhysX holds. The
        /// shoulder, and every other joint, as the table has them (frame: the part's own).
        /// </summary>
        public static JointLimitSpec Physical(PartId part)
        {
            switch (part)
            {
                case PartId.ForearmL:
                case PartId.ForearmR:
                    // (Off the hinge: on the ground, locked. Held weakly (a get-up), room for the poses
                    // still made in the old model (the off arm, the get-ups: their elbow bends about the
                    // part's X, up to 30 degrees off this hinge, and rolls) until they move onto the rig.)
                    return new JointLimitSpec(-PhysicsBattler.Motion.ArmRig.HingeSlack, PhysicsBattler.Motion.ArmRig.ElbowBendLimit, 60f, 45f);
                case PartId.HandL:
                case PartId.HandR:
                    return new JointLimitSpec(-PhysicsBattler.Motion.ArmRig.TwistLimit, PhysicsBattler.Motion.ArmRig.TwistLimit,
                        PhysicsBattler.Motion.ArmRig.FlexLimit, PhysicsBattler.Motion.ArmRig.FlexLimit);
                default:
                    return For(part);
            }
        }

        /// <summary>
        /// The limits on the ground (in the joint's frame, as <see cref="Physical"/>): the hinges are
        /// hinges: the knee bends only one way and the elbow neither sideways nor rolled (the
        /// forearm rolls at the wrist) (0: locked, once the joint has eased in).
        /// </summary>
        public static JointLimitSpec ForRagdoll(PartId part)
        {
            var spec = Physical(part);
            switch (part)
            {
                case PartId.ShinL:
                case PartId.ShinR:
                case PartId.ForearmL:
                case PartId.ForearmR:
                    spec.twist = 0f;
                    spec.swing = 0f;
                    break;
            }

            return spec;
        }
    }

    /// <summary>
    /// Milestone 10 (the arm rebuild's step 1): each joint's frame, in its part's rest frame (the
    /// rotation whose X, Y and Z are the joint's axis, secondary axis and third axis). The pose (the
    /// arm rig) and PhysX share it, so a turn means the same to both. Most joints: the part's own
    /// frame. The elbow: X along the rig's elbow axis (a positive turn bends the forearm forward), Y
    /// along the forearm. The wrist: X along the forearm (its twist), Y toward the thumb.
    /// </summary>
    public static class JointFrames
    {
        /// <summary>
        /// The frame for a part. <paramref name="restLocalPosition"/>: where the part sits in its
        /// parent at rest (the parent's bone); <paramref name="childRestLocalPosition"/>: where its
        /// child sits in it (its own bone; zero if none); <paramref name="restLocalRotation"/>: its
        /// rest turn in its parent (the frame is given in the part's own rest frame).
        /// </summary>
        public static Quaternion For(PartId id, Vector3 restLocalPosition, Vector3 childRestLocalPosition, Quaternion restLocalRotation)
        {
            Quaternion inParent;
            switch (id)
            {
                case PartId.ForearmL:
                case PartId.ForearmR:
                    inParent = Elbow(restLocalPosition, childRestLocalPosition);
                    break;
                case PartId.HandL:
                case PartId.HandR:
                    inParent = Wrist(restLocalPosition);
                    break;
                default:
                    return Quaternion.identity;
            }

            return Quaternion.Inverse(restLocalRotation) * inParent;
        }

        /// <summary>The bone of a part (where its own child sits in it) that its frame needs (the forearm's, for the elbow); zero for the rest.</summary>
        public static Vector3 BoneOf(PartId id, System.Func<PartId, Vector3> restLocalPosition)
        {
            switch (id)
            {
                case PartId.ForearmL: return restLocalPosition(PartId.HandL);
                case PartId.ForearmR: return restLocalPosition(PartId.HandR);
                default: return Vector3.zero;
            }
        }

        /// <summary>The frame of a built body's joint (from its parts' rest places).</summary>
        public static Quaternion For(UnitRig rig, PartId id)
        {
            var part = rig.Part(id);
            return For(id, part.restLocalPosition, BoneOf(id, other => rig.Part(other).restLocalPosition), part.restLocalRotation);
        }

        /// <summary>The elbow's hinge axis (in the upper arm's frame) from the upper arm's bone: bending about it by a positive angle brings the forearm forward.</summary>
        public static Vector3 ElbowAxis(Vector3 upperArmBone) => Vector3.Cross(upperArmBone.normalized, Vector3.forward).normalized;

        /// <summary>The elbow's frame: X its hinge axis, Y along the forearm (squared to it).</summary>
        public static Quaternion Elbow(Vector3 upperArmBone, Vector3 forearmBone)
        {
            Vector3 x = ElbowAxis(upperArmBone);
            Vector3 y = forearmBone.sqrMagnitude > 1e-10f ? forearmBone.normalized : upperArmBone.normalized;
            y = (y - x * Vector3.Dot(y, x)).normalized;
            return Quaternion.LookRotation(Vector3.Cross(x, y), y);
        }

        /// <summary>The wrist's frame from the forearm's bone: X along the forearm, Y toward the thumb (forward at rest), Z = X × Y.</summary>
        public static Quaternion Wrist(Vector3 forearmBone)
        {
            Vector3 along = forearmBone.normalized;
            Vector3 thumb = Vector3.forward - along * Vector3.Dot(Vector3.forward, along);
            return Quaternion.LookRotation(Vector3.Cross(along, thumb.normalized), Vector3.Cross(Vector3.Cross(along, thumb.normalized), along));
        }

        /// <summary>A joint's turn (as the part's local rotation against its rest) in the joint's frame.</summary>
        public static Quaternion ToJoint(Quaternion local, Quaternion frame) => Quaternion.Inverse(frame) * local * frame;

        /// <summary>Points a ConfigurableJoint's axes along the frame (the joint is set up again from the pose it is in: call it at rest).</summary>
        public static void Apply(ConfigurableJoint joint, Quaternion frame)
        {
            Vector3 axis = frame * Vector3.right, secondary = frame * Vector3.up;
            if ((joint.axis - axis).sqrMagnitude > 1e-10f)
            {
                joint.axis = axis;
            }

            if ((joint.secondaryAxis - secondary).sqrMagnitude > 1e-10f)
            {
                joint.secondaryAxis = secondary;
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

        /// <summary>
        /// The same for a joint whose axes are <paramref name="frame"/> (<see cref="JointFrames"/>):
        /// the turn asked of the part (against its rest), taken into the joint's frame.
        /// </summary>
        public static Quaternion TargetRotation(Quaternion targetLocal, Quaternion restLocal, Quaternion frame)
        {
            var turn = Quaternion.Inverse(restLocal) * targetLocal;
            return TargetRotation(JointFrames.ToJoint(turn, frame), Quaternion.identity);
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
        /// Milestone 10: PhysX doesn't hold the two swing limits (about Y and Z) one at a time, as the
        /// table and the pose solvers have them (a box: each within its own limit). It holds them
        /// together, as an ellipse (in the tangent of a quarter of the swing): a swing near both
        /// limits at once is outside it. A held arm's poses go right into those corners (the Large's
        /// guard: the shoulder rolled 90 and out 70), and there the joint's limit fought the pose,
        /// leaving the arm stuck 30 to 40 degrees off it for a second at a time (the 07:02 playtest).
        /// These are the swing limits of the smallest such ellipse (of the same shape) that holds the
        /// whole box. False if none does inside PhysX's 177 degrees (the shoulder's 130 out to the
        /// side with its roll of 90): that joint is left free.
        /// </summary>
        public static bool ConeAround(JointLimitSpec spec, out JointLimitSpec cone)
        {
            cone = spec;
            float sy = Mathf.Sin(0.5f * spec.twist * Mathf.Deg2Rad), sz = Mathf.Sin(0.5f * spec.swing * Mathf.Deg2Rad);
            if (sy * sy + sz * sz >= 1f)
            {
                return false;   // (the box's corner is past a half turn of swing)
            }

            // (The box's worst point is its corner: both tangents are largest there.)
            float w = Mathf.Sqrt(1f - sy * sy - sz * sz);
            float ty = sy / (1f + w), tz = sz / (1f + w);
            float baseY = Mathf.Tan(0.25f * spec.twist * Mathf.Deg2Rad), baseZ = Mathf.Tan(0.25f * spec.swing * Mathf.Deg2Rad);
            float k = Mathf.Max(1f, Mathf.Sqrt(Sq(ty / Mathf.Max(1e-4f, baseY)) + Sq(tz / Mathf.Max(1e-4f, baseZ))));
            cone.twist = 4f * Mathf.Atan(k * baseY) * Mathf.Rad2Deg;
            cone.swing = 4f * Mathf.Atan(k * baseZ) * Mathf.Rad2Deg;
            return cone.twist <= 177f && cone.swing <= 177f;
        }

        private static float Sq(float x) => x * x;

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

        /// <summary>A turn as a turn vector: its axis times its angle (radians), the short way round.</summary>
        public static Vector3 TurnVector(Quaternion q)
        {
            if (q.w < 0f)
            {
                q = new Quaternion(-q.x, -q.y, -q.z, -q.w);
            }

            var v = new Vector3(q.x, q.y, q.z);
            float s = v.magnitude;
            return s < 1e-7f ? Vector3.zero : v / s * (2f * Mathf.Atan2(s, q.w));
        }

        /// <summary>The turn a turn vector stands for (its axis times its angle in radians).</summary>
        public static Quaternion FromTurnVector(Vector3 v)
        {
            float angle = v.magnitude;
            return angle < 1e-7f ? Quaternion.identity : Quaternion.AngleAxis(angle * Mathf.Rad2Deg, v / angle);
        }

        /// <summary>The same turn as <paramref name="v"/> (a turn vector, radians), written the way nearest <paramref name="last"/>: a turn and it plus a whole turn about its axis are one.</summary>
        public static Vector3 TurnVectorNear(Vector3 v, Vector3 last)
        {
            float angle = v.magnitude;
            if (angle < 1e-6f)
            {
                return v;
            }

            Vector3 axis = v / angle, best = v;
            for (int k = -2; k <= 2; k++)
            {
                Vector3 other = axis * (angle + k * 2f * Mathf.PI);
                if ((other - last).sqrMagnitude < (best - last).sqrMagnitude)
                {
                    best = other;
                }
            }

            return best;
        }
    }
}
