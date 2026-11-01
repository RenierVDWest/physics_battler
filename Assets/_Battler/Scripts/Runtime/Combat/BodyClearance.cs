using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Combat
{
    /// <summary>
    /// Does a blow keep clear of the attacker's own body? Measures, on the animation rig, how far
    /// the weapon (grip to tip) and the weapon arm (the far half of the upper arm, the forearm and
    /// the hand) stay from the unit's own
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
            Vector3 shoulder = anim.Part(PartId.UpperArmR).position;
            Vector3 elbow = anim.Part(PartId.ForearmR).position;
            Vector3 wrist = anim.Part(PartId.HandR).position;
            Vector3 upperFar = Vector3.Lerp(shoulder, elbow, 0.5f);
            foreach (var id in Torso)
            {
                float blade = Distance(rig, anim, id, grip, tip);
                if (blade < best)
                {
                    best = blade;
                    closest = id;
                    weaponHit = true;
                }

                // The arm (milestone 9c: the far half of the upper arm, the forearm and the hand) against
                // the whole torso, the chest with some allowance (the colliders meet at the shoulder).
                // (The upper arm hangs against the torso's side: it has the allowance everywhere.)
                float arm = Mathf.Min(Distance(rig, anim, id, upperFar, elbow) + ArmChestAllowance * scale,
                    Mathf.Min(Distance(rig, anim, id, elbow, wrist), Distance(rig, anim, id, wrist, grip))
                    + (id == PartId.Chest ? ArmChestAllowance * scale : 0f));

                if (arm < best)
                {
                    best = arm;
                    closest = id;
                    weaponHit = false;
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

        /// <summary>
        /// Milestone 9c: the torso's colliders as the animation has them now, posed once, so the arm
        /// solver can ask many times how close an arm or blade would come.
        /// </summary>
        public sealed class Posed
        {
            private readonly Collider[] shapes = new Collider[Torso.Length];
            private readonly Vector3[] positions = new Vector3[Torso.Length];
            private readonly Quaternion[] toLocal = new Quaternion[Torso.Length];
            private readonly Vector3[] centres = new Vector3[Torso.Length];
            private readonly float[] bounds = new float[Torso.Length];
            private readonly float scale;

            /// <summary>Further than this from a shape's bounding sphere, a segment isn't measured (metres): it is clear.</summary>
            private const float Far = 0.08f;

            public Posed(UnitRig rig, AnimRig anim, float scale)
            {
                this.scale = scale;
                for (int i = 0; i < Torso.Length; i++)
                {
                    var part = rig.Part(Torso[i]);
                    var collider = part != null ? part.partCollider : null;
                    if (collider == null || rig.IsSevered(Torso[i]))
                    {
                        continue;
                    }

                    Transform holder = collider.transform, physical = part.transform;
                    Quaternion inverse = Quaternion.Inverse(physical.rotation);
                    var posed = anim.Part(Torso[i]);
                    Quaternion rotation = posed.rotation * (inverse * holder.rotation);
                    positions[i] = posed.position + posed.rotation * (inverse * (holder.position - physical.position));
                    toLocal[i] = Quaternion.Inverse(rotation);
                    shapes[i] = collider;
                    Bound(collider, out Vector3 c, out bounds[i]);
                    centres[i] = positions[i] + rotation * c;
                }
            }

            /// <summary>
            /// How close the arm (the far half of the upper arm, the forearm and the hand: shoulder to
            /// elbow to wrist to grip) comes to the torso, as <see cref="Measure"/> counts it (the
            /// chest's allowance added).
            /// </summary>
            public float Arm(Vector3 shoulder, Vector3 elbow, Vector3 wrist, Vector3 grip)
            {
                float best = float.MaxValue;
                Vector3 upperFar = Vector3.Lerp(shoulder, elbow, 0.5f);
                for (int i = 0; i < Torso.Length; i++)
                {
                    if (shapes[i] == null)
                    {
                        continue;
                    }

                    float allowance = ArmChestAllowance * scale;
                    float d = Mathf.Min(Segment(i, upperFar, elbow) + allowance,
                        Mathf.Min(Segment(i, elbow, wrist), Segment(i, wrist, grip)) + (Torso[i] == PartId.Chest ? allowance : 0f));
                    best = Mathf.Min(best, d);
                }

                return best;
            }

            /// <summary>How close the blade (grip to tip) comes to the pelvis, spine, chest and head.</summary>
            public float Blade(Vector3 grip, Vector3 tip)
            {
                float best = float.MaxValue;
                for (int i = 0; i < Torso.Length; i++)
                {
                    if (shapes[i] != null)
                    {
                        best = Mathf.Min(best, Segment(i, grip, tip));
                    }
                }

                return best;
            }

            private float Segment(int i, Vector3 a, Vector3 b)
            {
                // (Well clear of its bounding sphere: as clear as that says, no need to look closer.)
                Vector3 ab = b - a;
                float t = ab.sqrMagnitude > 1e-10f ? Mathf.Clamp01(Vector3.Dot(centres[i] - a, ab) / ab.sqrMagnitude) : 0f;
                float far = Vector3.Distance(a + ab * t, centres[i]) - bounds[i];
                if (far > Far)
                {
                    return far;
                }

                float best = float.MaxValue;
                const int samples = 12;
                for (int k = 0; k <= samples; k++)
                {
                    best = Mathf.Min(best, ShapeDistance(shapes[i], toLocal[i] * (Vector3.Lerp(a, b, k / (float)samples) - positions[i])));
                }

                return best;
            }
        }

        private static void Bound(Collider collider, out Vector3 centre, out float radius)
        {
            switch (collider)
            {
                case BoxCollider box:
                    centre = box.center;
                    radius = (box.size * 0.5f).magnitude;
                    return;
                case SphereCollider sphere:
                    centre = sphere.center;
                    radius = sphere.radius;
                    return;
                case CapsuleCollider capsule:
                    centre = capsule.center;
                    radius = Mathf.Max(capsule.radius, capsule.height * 0.5f);
                    return;
                default:
                    centre = Vector3.zero;
                    radius = 10f;
                    return;
            }
        }

        /// <summary>Distance from a point to a part's collider posed on the animation rig (negative: inside).</summary>
        public static float PointDistance(UnitRig rig, AnimRig anim, PartId id, Vector3 p)
        {
            var part = rig.Part(id);
            var collider = part != null ? part.partCollider : null;
            if (collider == null || rig.IsSevered(id))
            {
                return float.MaxValue;
            }

            Transform holder = collider.transform, physical = part.transform;
            Quaternion inverse = Quaternion.Inverse(physical.rotation);
            Quaternion relativeRotation = inverse * holder.rotation;
            Vector3 relativePosition = inverse * (holder.position - physical.position);
            var posed = anim.Part(id);
            Quaternion rotation = posed.rotation * relativeRotation;
            Vector3 position = posed.position + posed.rotation * relativePosition;
            return ShapeDistance(collider, Quaternion.Inverse(rotation) * (p - position));
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
