using UnityEngine;

namespace PhysicsBattler.Combat
{
    /// <summary>
    /// Where a held shield can face (milestone 9), measured from the chest: from 40 degrees across
    /// the body (toward the weapon side) to 90 degrees out to its own side, from 35 degrees down
    /// (over the legs) to 55 degrees up (overhead). Outside it, a shield would have to go behind the
    /// back or twist the arm round: a unit can't block there, it turns to face the blow first.
    /// </summary>
    public static class ShieldCone
    {
        public const float Across = 40f, Out = 90f, Down = 35f, Up = 55f;

        /// <summary>A direction's yaw against a chest facing (degrees, + right) and its pitch (+ up).</summary>
        public static void Angles(Vector3 direction, float chestYaw, out float yaw, out float pitch)
        {
            Vector3 flat = new Vector3(direction.x, 0f, direction.z);
            yaw = flat.sqrMagnitude > 1e-8f ? Mathf.DeltaAngle(chestYaw, Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg) : 0f;
            pitch = Mathf.Atan2(direction.y, Mathf.Max(1e-4f, flat.magnitude)) * Mathf.Rad2Deg;
        }

        /// <summary>Is a direction (yaw against the chest, pitch) one a shield in that hand can face?</summary>
        public static bool Contains(float yaw, float pitch, bool leftHand, float slack = 0f)
        {
            float outward = leftHand ? -yaw : yaw;
            return outward <= Out + slack && outward >= -Across - slack && pitch >= -Down - slack && pitch <= Up + slack;
        }

        public static bool Contains(Vector3 direction, float chestYaw, bool leftHand, float slack = 0f)
        {
            Angles(direction, chestYaw, out float yaw, out float pitch);
            return Contains(yaw, pitch, leftHand, slack);
        }

        /// <summary>The nearest direction inside the cone (yaw against the chest and pitch, in place).</summary>
        public static void Clamp(ref float yaw, ref float pitch, bool leftHand)
        {
            float outward = Mathf.Clamp(leftHand ? -yaw : yaw, -Across, Out);
            yaw = leftHand ? -outward : outward;
            pitch = Mathf.Clamp(pitch, -Down, Up);
        }

        /// <summary>How far outside the cone a direction is (degrees; 0 inside).</summary>
        public static float Outside(Vector3 direction, float chestYaw, bool leftHand)
        {
            Angles(direction, chestYaw, out float yaw, out float pitch);
            float y = yaw, p = pitch;
            Clamp(ref y, ref p, leftHand);
            return Mathf.Max(Mathf.Abs(Mathf.DeltaAngle(yaw, y)), Mathf.Abs(pitch - p));
        }

        /// <summary>The direction at a yaw against the chest facing and a pitch.</summary>
        public static Vector3 Direction(float chestYaw, float yaw, float pitch) => Quaternion.Euler(-pitch, chestYaw + yaw, 0f) * Vector3.forward;

        /// <summary>A chest's facing (degrees): its forward, flattened (or the fallback when it points straight up or down).</summary>
        public static float FacingOf(Quaternion chest, float fallback)
        {
            Vector3 f = chest * Vector3.forward;
            f.y = 0f;
            return f.sqrMagnitude > 1e-6f ? Mathf.Atan2(f.x, f.z) * Mathf.Rad2Deg : fallback;
        }
    }
}
