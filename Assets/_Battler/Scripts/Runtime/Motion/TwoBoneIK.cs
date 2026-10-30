using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// Analytic two-bone IK (shoulder-elbow-wrist, hip-knee-ankle) on the ghost rig's transforms.
    /// Three steps: bend the middle joint to the right angle for the distance, aim the chain at the
    /// target, then twist it about the root-to-target line so the middle joint points at the pole.
    /// </summary>
    public static class TwoBoneIK
    {
        private const float Epsilon = 1e-4f;

        /// <param name="upper">The root bone (shoulder or hip null).</param>
        /// <param name="lower">The middle bone (elbow or knee null).</param>
        /// <param name="end">The end null (wrist or ankle); only its position is used.</param>
        /// <param name="target">Where the end should go (world).</param>
        /// <param name="pole">World direction the middle joint should point toward (e.g. forward for a knee).</param>
        public static void Solve(Transform upper, Transform lower, Transform end, Vector3 target, Vector3 pole)
        {
            Vector3 a = upper.position;
            Vector3 b = lower.position;
            Vector3 c = end.position;

            float upperLength = (b - a).magnitude;
            float lowerLength = (c - b).magnitude;
            if (upperLength < Epsilon || lowerLength < Epsilon)
            {
                return;
            }

            float reach = Mathf.Clamp((target - a).magnitude,
                Mathf.Abs(upperLength - lowerLength) + Epsilon, upperLength + lowerLength - Epsilon);

            // 1. Bend: the angle between the two bones (0 = straight) that puts the end at the reach.
            float cosInterior = Mathf.Clamp(
                (upperLength * upperLength + lowerLength * lowerLength - reach * reach) / (2f * upperLength * lowerLength),
                -1f, 1f);
            float wantedBend = 180f - Mathf.Acos(cosInterior) * Mathf.Rad2Deg;
            float currentBend = Vector3.Angle(b - a, c - b);

            Vector3 bendAxis = Vector3.Cross(b - a, c - b);
            if (bendAxis.sqrMagnitude < Epsilon * Epsilon)
            {
                // Straight chain: bend so the middle joint moves toward the pole.
                bendAxis = Vector3.Cross(pole, b - a);
                if (bendAxis.sqrMagnitude < Epsilon * Epsilon)
                {
                    bendAxis = Vector3.Cross(Vector3.up, b - a);
                }
            }

            bendAxis.Normalize();
            lower.rotation = Quaternion.AngleAxis(wantedBend - currentBend, bendAxis) * lower.rotation;

            // 2. Aim the whole chain at the target.
            Vector3 toEnd = end.position - a;
            Vector3 toTarget = target - a;
            if (toEnd.sqrMagnitude > Epsilon && toTarget.sqrMagnitude > Epsilon)
            {
                upper.rotation = Quaternion.FromToRotation(toEnd, toTarget) * upper.rotation;
            }

            // 3. Twist about the root-to-target line so the middle joint faces the pole.
            if (toTarget.sqrMagnitude > Epsilon)
            {
                Vector3 axis = toTarget.normalized;
                Vector3 middle = Vector3.ProjectOnPlane(lower.position - a, axis);
                Vector3 wanted = Vector3.ProjectOnPlane(pole, axis);
                if (middle.sqrMagnitude > Epsilon * Epsilon && wanted.sqrMagnitude > Epsilon * Epsilon)
                {
                    float twist = Vector3.SignedAngle(middle, wanted, axis);
                    upper.rotation = Quaternion.AngleAxis(twist, axis) * upper.rotation;
                }
            }
        }
    }
}
