using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// How hands hold a weapon: the main hand at the grip, the second hand (two-handed weapons) further
    /// along it, each hand pointing along the weapon with its back toward its own elbow so the wrists
    /// aren't twisted. Shared by the swing, the carry and the equip, on the ghost and the physical body.
    /// </summary>
    public static class WeaponGrip
    {
        /// <summary>The second hand's grip: <paramref name="offset"/> metres along the weapon from the main grip.</summary>
        public static Vector3 SecondGrip(Vector3 grip, Quaternion handRotation, float offset)
        {
            return grip + handRotation * Vector3.forward * offset;
        }

        /// <summary>A hand on the weapon at <paramref name="grip"/>: forward along the weapon, up toward <paramref name="shoulder"/>.</summary>
        public static Quaternion HandRotation(Vector3 direction, Vector3 grip, Vector3 shoulder)
        {
            if (direction.sqrMagnitude < 1e-8f)
            {
                direction = Vector3.forward;
            }

            Vector3 up = Vector3.ProjectOnPlane(shoulder - grip, direction);
            if (up.sqrMagnitude < 1e-6f)
            {
                up = Vector3.ProjectOnPlane(Vector3.up, direction);
            }

            return Quaternion.LookRotation(direction.normalized, up.sqrMagnitude > 1e-8f ? up.normalized : Vector3.up);
        }

        /// <summary>Where an elbow should point: down, out to its side, and a little back.</summary>
        public static Vector3 ElbowPole(Transform root, int side)
        {
            return (-0.6f * root.up + 0.5f * side * root.right - 0.4f * root.forward).normalized;
        }

        /// <summary>
        /// Solves an arm (three transforms: upper arm, forearm, hand) so the hand's weapon slot is at
        /// <paramref name="grip"/>, with the hand turned to <paramref name="handRotation"/>.
        /// </summary>
        public static void SolveArm(Transform upper, Transform lower, Transform hand, Vector3 slotLocalPosition,
            Vector3 grip, Quaternion handRotation, Vector3 pole)
        {
            Vector3 wrist = grip - handRotation * slotLocalPosition;
            TwoBoneIK.Solve(upper, lower, hand, wrist, pole);
            hand.rotation = handRotation;
        }
    }
}
