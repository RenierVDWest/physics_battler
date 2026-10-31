using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Combat
{
    /// <summary>
    /// Does a blow keep clear of the attacker's own body? Measures, on the animation rig, how far
    /// the weapon (grip to tip) and the weapon arm (forearm and hand) stay from the unit's own
    /// pelvis, spine, chest and head: their colliders, posed as the animation has them. Negative
    /// means passing through. The planner turns down blows that would pass through their wielder.
    /// </summary>
    public static class BodyClearance
    {
        private static readonly PartId[] Torso = { PartId.Pelvis, PartId.Spine, PartId.Chest, PartId.Head };

        /// <summary>How deep the arm may sink into the chest's collider where it meets the shoulder (metres at medium size).</summary>
        public const float ArmChestAllowance = 0.03f;

        /// <summary>The smallest clearance in the current animated pose, and which part it is from.</summary>
        public static float Measure(UnitRig rig, AnimRig anim, float weaponLength, float scale, out PartId closest, out bool weaponHit)
        {
            closest = PartId.Chest;
            weaponHit = false;
            float best = float.MaxValue;
            var slot = anim.SlotWeaponR;
            Vector3 grip = slot.position;
            Vector3 tip = slot.position + slot.rotation * (Vector3.forward * weaponLength * scale);
            Vector3 elbow = anim.Part(PartId.ForearmR).position;
            Vector3 wrist = anim.Part(PartId.HandR).position;
            foreach (var id in Torso)
            {
                float blade = Distance(rig, anim, id, grip, tip);
                if (blade < best)
                {
                    best = blade;
                    closest = id;
                    weaponHit = true;
                }

                // The forearm and hand: against the head everywhere, against the chest with some
                // allowance (the colliders meet at the shoulder).
                if (id == PartId.Head || id == PartId.Chest)
                {
                    float arm = Mathf.Min(Distance(rig, anim, id, elbow, wrist), Distance(rig, anim, id, wrist, grip));
                    if (id == PartId.Chest)
                    {
                        arm += ArmChestAllowance * scale;
                    }

                    if (arm < best)
                    {
                        best = arm;
                        closest = id;
                        weaponHit = false;
                    }
                }
            }

            return best;
        }

        /// <summary>Distance from a segment to a part's collider posed on the animation rig (negative: inside).</summary>
        public static float Distance(UnitRig rig, AnimRig anim, PartId id, Vector3 a, Vector3 b)
        {
            var part = rig.Part(id);
            var collider = part != null ? part.partCollider : null;
            if (collider == null)
            {
                return float.MaxValue;
            }

            // The collider's pose relative to its part (fixed), applied to the part's animated pose.
            Transform holder = collider.transform, physical = part.transform;
            Quaternion inverse = Quaternion.Inverse(physical.rotation);
            Quaternion relativeRotation = inverse * holder.rotation;
            Vector3 relativePosition = inverse * (holder.position - physical.position);
            var posed = anim.Part(id);
            Quaternion rotation = posed.rotation * relativeRotation;
            Vector3 position = posed.position + posed.rotation * relativePosition;
            Quaternion toLocal = Quaternion.Inverse(rotation);

            float best = float.MaxValue;
            const int samples = 12;
            for (int i = 0; i <= samples; i++)
            {
                Vector3 p = Vector3.Lerp(a, b, i / (float)samples);
                Vector3 local = toLocal * (p - position);
                best = Mathf.Min(best, ShapeDistance(collider, local));
            }

            return best;
        }

        private static float ShapeDistance(Collider collider, Vector3 p)
        {
            switch (collider)
            {
                case BoxCollider box:
                {
                    Vector3 h = box.size * 0.5f;
                    Vector3 d = p - box.center;
                    Vector3 q = new Vector3(Mathf.Abs(d.x) - h.x, Mathf.Abs(d.y) - h.y, Mathf.Abs(d.z) - h.z);
                    Vector3 outside = new Vector3(Mathf.Max(q.x, 0f), Mathf.Max(q.y, 0f), Mathf.Max(q.z, 0f));
                    return outside.magnitude + Mathf.Min(Mathf.Max(q.x, Mathf.Max(q.y, q.z)), 0f);
                }
                case SphereCollider sphere:
                    return (p - sphere.center).magnitude - sphere.radius;
                case CapsuleCollider capsule:
                {
                    Vector3 d = p - capsule.center;
                    Vector3 axis = capsule.direction == 0 ? Vector3.right : capsule.direction == 2 ? Vector3.forward : Vector3.up;
                    float half = Mathf.Max(0f, capsule.height * 0.5f - capsule.radius);
                    float along = Mathf.Clamp(Vector3.Dot(d, axis), -half, half);
                    return (d - axis * along).magnitude - capsule.radius;
                }
                default:
                    return float.MaxValue;
            }
        }
    }
}
