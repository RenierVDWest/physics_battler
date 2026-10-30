using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// A planned swing: the weapon turns about the attacker's shoulder on a tilted plane, and the
    /// plane and reach are chosen so the weapon's sweet spot passes through the target at θ = 0.
    /// </summary>
    public struct SwingPlan
    {
        /// <summary>Shoulder position the plan was made from (the untwisted shoulder).</summary>
        public Vector3 shoulder;
        public Vector3 target;
        /// <summary>Weapon direction at contact (θ = 0): shoulder to target.</summary>
        public Vector3 contactDirection;
        /// <summary>Swing axis. Positive θ turns the weapon up and over toward the follow-through.</summary>
        public Vector3 axis;
        public float rollDegrees;
        public float distance;
        /// <summary>Shoulder-to-grip distance at contact.</summary>
        public float contactReach;
        /// <summary>How far the body must lean in for the sweet spot to reach (0 when in range).</summary>
        public float extraLean;
        /// <summary>1 when the sweet spot lands; less when the target is so close it is hit with the shaft.</summary>
        public float crowdedFactor;
        public bool inRange;
    }

    /// <summary>The weapon and arm numbers the solver needs.</summary>
    public struct SwingReach
    {
        public float sweetSpot;        // grip to sweet spot
        public float tip;              // grip to tip
        public float minArm;           // shortest comfortable shoulder-to-grip distance
        public float maxArm;           // longest (arm nearly straight)
        public float maxLean;          // how far the body can lean in to reach
    }

    public static class SwingSolver
    {
        /// <summary>
        /// Plans a swing from <paramref name="shoulder"/> at <paramref name="target"/>.
        /// <paramref name="side"/> is +1 for a right-handed swing (the low point of the arc on the
        /// attacker's right), -1 for left. <paramref name="rollDegrees"/> tilts the swing plane:
        /// 0 is vertical (underhand or overhead), 90 is a flat sweep.
        /// </summary>
        public static SwingPlan Plan(Vector3 shoulder, Vector3 target, SwingReach reach, float rollDegrees, int side)
        {
            var plan = new SwingPlan { shoulder = shoulder, target = target, rollDegrees = rollDegrees };
            Vector3 toTarget = target - shoulder;
            plan.distance = toTarget.magnitude;
            plan.contactDirection = plan.distance > 1e-4f ? toTarget / plan.distance : Vector3.forward;

            // Horizontal axis across the contact direction: rotating about it by +θ lifts the weapon.
            Vector3 across = Vector3.Cross(plan.contactDirection, Vector3.up);
            if (across.sqrMagnitude < 1e-6f)
            {
                across = Vector3.Cross(plan.contactDirection, Vector3.forward);
            }

            across.Normalize();
            // Rolling the axis about the contact direction tilts the plane; positive roll for a
            // right-handed swing puts the bottom of the arc out to the attacker's right.
            plan.axis = Quaternion.AngleAxis(rollDegrees * side, plan.contactDirection) * across;

            float wanted = plan.distance - reach.sweetSpot;
            plan.inRange = wanted <= reach.maxArm + reach.maxLean;
            plan.extraLean = Mathf.Clamp(wanted - reach.maxArm, 0f, reach.maxLean);
            plan.contactReach = Mathf.Clamp(wanted, reach.minArm, reach.maxArm);
            plan.crowdedFactor = wanted >= reach.minArm
                ? 1f
                : Mathf.Clamp01((plan.distance - reach.minArm) / Mathf.Max(0.01f, reach.sweetSpot));
            return plan;
        }

        /// <summary>The weapon's direction at swing angle θ (degrees).</summary>
        public static Vector3 Direction(in SwingPlan plan, float theta)
        {
            return Quaternion.AngleAxis(theta, plan.axis) * plan.contactDirection;
        }

        /// <summary>The swing angle of a direction, measured in the plan's plane.</summary>
        public static float AngleOf(in SwingPlan plan, Vector3 direction)
        {
            Vector3 inPlane = Vector3.ProjectOnPlane(direction, plan.axis);
            if (inPlane.sqrMagnitude < 1e-6f)
            {
                return 0f;
            }

            return Vector3.SignedAngle(plan.contactDirection, inPlane, plan.axis);
        }

        /// <summary>
        /// The longest shoulder-to-grip distance that keeps the whole weapon at least
        /// <paramref name="clearance"/> above the ground when it points along <paramref name="direction"/>.
        /// </summary>
        public static float GroundLimitedReach(Vector3 shoulder, Vector3 direction, float tip, float groundY,
            float clearance)
        {
            if (direction.y > -0.02f)
            {
                return float.MaxValue;
            }

            return (shoulder.y - groundY - clearance) / -direction.y - tip;
        }

        /// <summary>
        /// The smallest roll (from <paramref name="rollDegrees"/> up to 85) at which the weapon stays
        /// clear of the ground over the swing from <paramref name="lowestTheta"/> to 0, with the arm at
        /// its shortest. Long weapons swung low need a flatter plane.
        /// </summary>
        public static float RollForClearance(Vector3 shoulder, Vector3 target, SwingReach reach, float rollDegrees,
            int side, float lowestTheta, float groundY, float clearance)
        {
            for (float roll = rollDegrees; roll < 85f; roll += 5f)
            {
                var plan = Plan(shoulder, target, reach, roll, side);
                if (Clears(plan, reach, lowestTheta, groundY, clearance))
                {
                    return roll;
                }
            }

            return 85f;
        }

        private static bool Clears(in SwingPlan plan, SwingReach reach, float lowestTheta, float groundY, float clearance)
        {
            for (float theta = lowestTheta; theta <= 0f; theta += 5f)
            {
                Vector3 direction = Direction(plan, theta);
                float lowest = plan.shoulder.y + Mathf.Min(0f, direction.y) * (reach.minArm + reach.tip);
                if (lowest < groundY + clearance)
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>Easing curves for the swing phases.</summary>
    public static class Ease
    {
        public static float InOutCubic(float t)
        {
            t = Mathf.Clamp01(t);
            return t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }

        public static float OutCubic(float t)
        {
            t = Mathf.Clamp01(t);
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        public static float InPower(float t, float power)
        {
            return Mathf.Pow(Mathf.Clamp01(t), Mathf.Max(1f, power));
        }

        public static float Smooth(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * (3f - 2f * t);
        }
    }
}
