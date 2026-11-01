using PhysicsBattler.Combat;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// Milestone 9c: a held weapon or shield kept out of the ground in the pose. After the arms are
    /// solved, points along the weapon (the pommel, along the blade or haft to the tip, each with the
    /// weapon's thickness round it) or round the shield's rim are measured against the ground. Any
    /// below it (plus a margin) lift the weapon: it turns about the wrist, inside the wrist's limits,
    /// then the whole arm turns up at the shoulder, the elbow keeping its bend. It runs on every pose:
    /// standing, in a blow, and getting up.
    /// </summary>
    public static class GroundClamp
    {
        /// <summary>How far above the ground a held weapon's lowest point is kept (metres at medium size).</summary>
        public const float Margin = 0.02f;

        /// <summary>How far the wrist turns at most to lift a weapon (degrees), before the arm rises; and how far the arm turns up at the shoulder at most each time.</summary>
        public const float MostTurn = 60f, MostRaise = 60f;

        /// <summary>
        /// How soft the floor is (metres at medium size): a weapon coming down is eased up from about
        /// this far above the margin, not stopped dead at it, so the arm never jerks.
        /// </summary>
        public const float Soft = 0.03f;

        /// <summary>
        /// Where a held thing whose lowest point is <paramref name="gap"/> over the ground is kept:
        /// never under <paramref name="margin"/>, the same well above it, and smoothly in between
        /// (a soft-plus: its speed never jumps as the floor takes over).
        /// </summary>
        public static float SoftGap(float gap, float margin, float soft)
        {
            float x = (gap - margin) / soft;
            return margin + soft * (x > 20f ? x : Mathf.Log(1f + Mathf.Exp(x)));
        }

        /// <summary>
        /// The held thing's lowest point over the ground (metres; negative: below it), its slot posed
        /// as it is now.
        /// </summary>
        public static float Gap(Transform slot, WeaponDefinition weapon, float scale, float groundY)
        {
            return Lowest(slot, weapon, scale, out _) - groundY;
        }

        /// <summary>Keeps the held thing in the hand on side <paramref name="right"/> above the ground. Returns its gap after.</summary>
        /// <remarks>
        /// <paramref name="free"/>: once, on a body as it lay (a ragdoll's pose, which can have what it
        /// holds anywhere): the wrist may go as far as it is needed and the arm turn twice as far,
        /// a few times over, so it starts out of the floor.
        /// </remarks>
        public static float Keep(AnimRig anim, bool right, WeaponDefinition weapon, float scale, float size, float groundY, float weight = 1f, bool free = false)
        {
            if (weapon == null || weight <= 0f)
            {
                return weapon == null ? float.MaxValue : Gap(right ? anim.SlotWeaponR : anim.SlotWeaponL, weapon, scale, groundY);
            }

            var slot = right ? anim.SlotWeaponR : anim.SlotWeaponL;
            var upperId = right ? PartId.UpperArmR : PartId.UpperArmL;
            var foreId = right ? PartId.ForearmR : PartId.ForearmL;
            var handId = right ? PartId.HandR : PartId.HandL;
            var upper = anim.Part(upperId);
            var fore = anim.Part(foreId);
            var hand = anim.Part(handId);
            float margin = Margin * size, soft = Soft * size;
            float lowWas = Lowest(slot, weapon, scale, out _);
            if (lowWas - groundY > margin + 20f * soft)
            {
                return lowWas - groundY;
            }

            // (Lifted to its soft place: at least the margin, eased in.)
            float want = groundY + SoftGap(lowWas - groundY, margin, soft) - 1e-4f;
            Quaternion upperWas = upper.localRotation, foreWas = fore.localRotation, handWas = hand.localRotation;

            // 1. Turned about the wrist, as far as lifts the low part of it to its place (measured where
            // the low part is: a point that moves smoothly as the weapon turns, not whichever sample
            // is lowest), at most MostTurn and never further past the wrist's limits than the pose had
            // it (a clamp into them would snap the hand the moment the floor takes over).
            float low = Lowest(slot, weapon, scale, out _, out Vector3 at);
            Vector3 lever = at - hand.position;
            Vector3 axis = Vector3.Cross(lever, Vector3.up);
            float reach = lever.magnitude;
            if (low < want && axis.sqrMagnitude > 1e-6f && reach > 1e-3f)
            {
                axis.Normalize();
                Quaternion from = hand.rotation;
                float pastBefore = free ? float.MaxValue : Mathf.Max(0f, Past(anim, handId)) + 0.5f;
                float turn = Mathf.Min(free ? 3f * MostTurn : MostTurn, Raise(lever, want - low));
                // (A low part hanging straight under the wrist can't be raised by turning it, and which
                // way to turn it is anyone's guess: the wrist gives way to the arm as it comes under.)
                float level = new Vector2(lever.x, lever.z).magnitude / reach;
                turn *= Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.2f, 0.45f, level));
                if (weapon.IsShield)
                {
                    // (A shield lying near flat: turning it only tips one side of its rim down, and
                    // its low part is under the hand. The wrist gives way to the arm as it flattens.)
                    float tilt = Vector3.ProjectOnPlane(Vector3.down, slot.rotation * Vector3.forward).magnitude;
                    turn *= Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.25f, 0.6f, tilt));
                }

                hand.rotation = Quaternion.AngleAxis(turn, axis) * from;
                if (Past(anim, handId) > pastBefore)
                {
                    // (As far as the wrist may go.)
                    float lo = 0f, hi = turn;
                    for (int k = 0; k < 10; k++)
                    {
                        float mid = 0.5f * (lo + hi);
                        hand.rotation = Quaternion.AngleAxis(mid, axis) * from;
                        if (Past(anim, handId) > pastBefore)
                        {
                            hi = mid;
                        }
                        else
                        {
                            lo = mid;
                        }
                    }

                    hand.rotation = Quaternion.AngleAxis(lo, axis) * from;
                }
            }

            // 2. Still short: the whole arm turns up at the shoulder (the elbow keeps its bend and its
            // side), as far as lifts the low part to its place, at most MostRaise; twice over (it turns
            // with the arm).
            for (int pass = 0; pass < (free ? 4 : 3); pass++)
            {
                low = Lowest(slot, weapon, scale, out _, out at);
                if (low >= want)
                {
                    break;
                }

                lever = at - upper.position;
                axis = Vector3.Cross(lever, Vector3.up);
                if (axis.sqrMagnitude < 1e-6f)
                {
                    axis = Vector3.Cross(hand.position - upper.position, Vector3.up);
                }

                if (axis.sqrMagnitude < 1e-8f)
                {
                    break;
                }

                upper.rotation = Quaternion.AngleAxis(Mathf.Min(free ? 2f * MostRaise : MostRaise, Raise(lever, want - low)), axis.normalized) * upper.rotation;
            }

            // (Part of the way: from the pose as it came.)
            if (weight < 1f)
            {
                upper.localRotation = Quaternion.Slerp(upperWas, upper.localRotation, weight);
                fore.localRotation = Quaternion.Slerp(foreWas, fore.localRotation, weight);
                hand.localRotation = Quaternion.Slerp(handWas, hand.localRotation, weight);
            }

            return Lowest(slot, weapon, scale, out _) - groundY;
        }

        /// <summary>The turn (degrees) about a level line square to <paramref name="lever"/> that raises its end by <paramref name="by"/>.</summary>
        private static float Raise(Vector3 lever, float by)
        {
            float len = lever.magnitude;
            float from = Mathf.Asin(Mathf.Clamp(lever.y / len, -1f, 1f));
            float to = Mathf.Asin(Mathf.Clamp((lever.y + by) / len, -1f, 1f));
            return Mathf.Max(0f, (to - from) * Mathf.Rad2Deg);
        }

        /// <summary>The held thing's lowest point (world y), and where it is.</summary>
        public static float Lowest(Transform slot, WeaponDefinition weapon, float scale, out Vector3 at)
        {
            return Lowest(slot, weapon, scale, out at, out _);
        }

        /// <summary>
        /// The held thing's lowest point (world y), where it is, and where its low part is: the points
        /// near the bottom weighed together (within about a centimetre of it), a place that moves
        /// smoothly as the thing turns (on a shield lying flat, its middle).
        /// </summary>
        public static float Lowest(Transform slot, WeaponDefinition weapon, float scale, out Vector3 at, out Vector3 lowPart)
        {
            at = slot.position;
            lowPart = at;
            float lowest = float.MaxValue;
            if (weapon.IsShield)
            {
                // The rim: a circle round the face's middle, square to its face.
                Vector3 centre = slot.position + slot.rotation * (weapon.faceCenter * scale);
                Vector3 face = slot.rotation * Vector3.forward;
                float r = weapon.radius * scale;
                Vector3 down = Vector3.ProjectOnPlane(Vector3.down, face);
                Vector3 rim = down.sqrMagnitude > 1e-8f ? centre + down.normalized * r : centre;
                lowPart = centre + down * r;
                // (Facing up, its back is lower than its face: the board is about 5 cm thick.)
                if (Vector3.Dot(face, Vector3.up) > 0f)
                {
                    rim -= face * (0.05f * scale);
                    lowPart -= face * (0.05f * scale);
                }
                at = rim;
                return rim.y;
            }

            // Along the weapon, pommel to tip, each point with the weapon's cross-section round it (a
            // blade is flat: wide across its edges, thin through).
            float xUp = (slot.rotation * Vector3.right).y, yUp = (slot.rotation * Vector3.up).y;
            float pommel = weapon.tip.z - weapon.length;
            const int samples = 8;
            var ys = new float[samples + 1];
            var ps = new Vector3[samples + 1];
            for (int i = 0; i <= samples; i++)
            {
                float z = Mathf.Lerp(pommel, weapon.tip.z, i / (float)samples);
                Vector2 half = HalfSize(weapon, z) * scale;
                Vector3 p = slot.position + slot.rotation * (new Vector3(0f, 0f, z) * scale);
                float y = p.y - Mathf.Sqrt(half.x * half.x * xUp * xUp + half.y * half.y * yUp * yUp);
                ys[i] = y;
                ps[i] = p;
                if (y < lowest)
                {
                    lowest = y;
                    at = p;
                }
            }

            // (The low part: each point weighed by how near the bottom it is.)
            float k = 0.01f * scale, total = 0f;
            Vector3 sum = Vector3.zero;
            for (int i = 0; i <= samples; i++)
            {
                float w = Mathf.Exp(-(ys[i] - lowest) / k);
                total += w;
                sum += w * ps[i];
            }

            lowPart = sum / total;
            return lowest;
        }

        /// <summary>How far the weapon reaches out from its axis at this point along it, across (X) and along its edges (Y) (metres at medium size).</summary>
        private static Vector2 HalfSize(WeaponDefinition weapon, float z)
        {
            if (weapon.kind == WeaponKind.Sword)
            {
                // (The crossguard by the grip, the blade beyond, the handle and pommel behind.)
                return Mathf.Abs(z - 0.075f) < 0.03f ? new Vector2(0.015f, 0.095f) : z > 0.075f ? new Vector2(0.01f, 0.03f) : new Vector2(0.02f, 0.02f);
            }

            // (A club's head; its handle: round.)
            float r = z >= 0.33f ? Mathf.Max(0.03f, weapon.sweepRadius) : 0.025f;
            return new Vector2(r, r);
        }

        /// <summary>How far a joint is past its limits (the joint table; degrees, 0 inside).</summary>
        private static float Past(AnimRig anim, PartId id)
        {
            Quaternion local = Quaternion.Inverse(anim.RestLocalRotation(id)) * anim.Part(id).localRotation;
            return JointMath.PastLimits(local, JointLimitTable.For(id), out _, out _, out _);
        }
    }
}
