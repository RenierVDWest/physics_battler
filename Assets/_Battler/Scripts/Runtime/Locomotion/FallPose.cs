using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// The pose a falling body is drawn to (the plan "Controlled falls", steps 3 and 4): not a clip on a clock, but
    /// made each step from how the body is: which way it is tipping and going, and where the ground is. A person
    /// going down gets low, puts the hands out toward the ground they are going to, and keeps the head away from it;
    /// this is that, for whichever way the body goes. The body's muscles draw it to this with a person's strength
    /// (<see cref="Authority.UpperBodyArticulation.Falling"/>); the physics does the rest.
    /// </summary>
    public static class FallPose
    {
        /// <summary>The head is held away from the ground: chin to the chest going backward, lifted going forward, tipped away going to a side.</summary>
        public static bool ProtectsHead = true;

        /// <summary>The arms reach for the ground the body is going to; off, they are held a little out from the sides.</summary>
        public static bool Reaches = true;

        /// <summary>How far the chin is brought to the chest on its back, the head lifted on its front, and tipped away on a side (degrees).</summary>
        public const float ChinIn = 38f, HeadUp = 30f, HeadAside = 28f;

        /// <summary>
        /// The pose put on <paramref name="anim"/>: its hips where the body's are (<paramref name="hips"/>), the rest
        /// from how the body is turned to the ground at <paramref name="ground"/> (its height) and which way it is
        /// going (<paramref name="going"/>, m/s).
        /// </summary>
        /// <param name="into">How far into its protective pose it is (0 as it loses its feet: the arms thrown out, the legs as they stood; 1: getting low and reaching).</param>
        public static void Pose(AnimRig anim, Transform hips, Vector3 going, float ground, float size, float into = 1f)
        {
            into = Mathf.Clamp01(into);
            anim.ResetToRest();
            anim.Part(PartId.Pelvis).SetPositionAndRotation(hips.position, hips.rotation);
            Vector3 up = hips.up, front = hips.forward, right = hips.right;
            Vector3 level = new Vector3(going.x, 0f, going.z);
            Vector3 way = level.magnitude > 0.3f ? level.normalized : Vector3.zero;
            // (How it lies to the ground, and while it still stands, which way it is going.)
            float standing = Mathf.Clamp01(Vector3.Dot(up, Vector3.up));
            float faceDown = Vector3.Dot(front, Vector3.down), rightDown = Vector3.Dot(right, Vector3.down);
            float back = Mathf.Clamp01(Mathf.Max(-faceDown, standing * -Vector3.Dot(way, Flat(front))));
            float forward = Mathf.Clamp01(Mathf.Max(faceDown, standing * Vector3.Dot(way, Flat(front))));
            float side = Mathf.Clamp(rightDown + standing * Vector3.Dot(way, Flat(right)), -1f, 1f);

            // The trunk: curled forward going backward (a rounded back rolls), held long going forward.
            Bend(anim, PartId.Spine, 5f + 14f * back - 6f * forward, 0f);
            Bend(anim, PartId.Chest, 4f + 12f * back - 8f * forward, 0f);
            // The head: away from the ground.
            if (ProtectsHead)
            {
                Bend(anim, PartId.Head, ChinIn * back - HeadUp * forward, HeadAside * side);
            }

            // The legs: folded to get low, more going backward (it sits down).
            float leg = anim.RestLocalPosition(PartId.ShinL).magnitude + anim.RestLocalPosition(PartId.FootL).magnitude;
            // (Going backward the thighs come forward, as sitting down; going forward the knees bend with the feet back, as kneeling.)
            // (As folded as the hips are low: the legs do not pull the body down, they go with it and brake it. Folded
            // by the clock, at a limb's own speed, they drew the hips to the ground in a fifth of a second, faster
            // than anything falls.)
            float low = 1f - Mathf.Clamp01((hips.position.y - ground) / (1.05f * leg));
            float fold = Mathf.Lerp(0.08f, 0.95f, low);
            float sits = Mathf.Lerp(0.12f + 0.38f * fold, -0.3f * fold, forward);
            for (int f = 0; f < 2; f++)
            {
                var thigh = anim.Part(f == 0 ? PartId.ThighL : PartId.ThighR);
                Vector3 ankle = thigh.position - up * (leg * (0.88f - 0.35f * fold)) + front * (leg * sits);
                TwoBoneIK.Solve(thigh, anim.Part(f == 0 ? PartId.ShinL : PartId.ShinR), anim.Part(f == 0 ? PartId.FootL : PartId.FootR), ankle, front + 0.2f * up);
            }

            // The arms: out toward the ground it is going to, elbows a little bent; a hand that can reach the ground goes to it.
            float arm = anim.RestLocalPosition(PartId.ForearmL).magnitude + anim.RestLocalPosition(PartId.HandL).magnitude;
            for (int a = 0; a < 2; a++)
            {
                var upper = anim.Part(a == 0 ? PartId.UpperArmL : PartId.UpperArmR);
                Vector3 out_ = Flat(right) * (a == 0 ? -1f : 1f);
                if (out_.sqrMagnitude < 0.04f)
                {
                    // (On a side: "out" is along the ground, away from the head's end.)
                    out_ = Flat(front) * (a == 0 ? -1f : 1f);
                }

                // (At first thrown out and up, a start; then toward the ground it is going to.)
                Vector3 start = (out_.normalized * 0.8f + up * 0.15f + front * 0.35f).normalized;
                Vector3 reach = Reaches ? Vector3.Slerp(start, (Vector3.down * 0.9f + way * 0.65f + out_.normalized * 0.4f).normalized, into) : (-up * 0.85f + out_.normalized * 0.45f + front * 0.25f).normalized;
                Vector3 hand = upper.position + reach * (0.9f * arm);
                float floor = ground + 0.06f * size;
                if (hand.y < floor)
                {
                    hand.y = floor;
                }

                Vector3 elbow = out_.normalized + 0.3f * Vector3.up - 0.3f * way;
                TwoBoneIK.Solve(upper, anim.Part(a == 0 ? PartId.ForearmL : PartId.ForearmR), anim.Part(a == 0 ? PartId.HandL : PartId.HandR), hand, elbow);
            }
        }

        private static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v.sqrMagnitude > 1e-6f ? v.normalized : Vector3.zero;
        }

        /// <summary>A part bent from its rest: forward (degrees, + forward) and to its left (+).</summary>
        private static void Bend(AnimRig anim, PartId id, float forward, float left)
        {
            anim.Part(id).localRotation = Quaternion.Euler(forward, 0f, left) * anim.RestLocalRotation(id);
        }
    }
}
