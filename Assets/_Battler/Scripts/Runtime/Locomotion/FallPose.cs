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

        /// <summary>How far a hand is turned on its wrist to keep what it holds clear of the body (degrees).</summary>
        public const float WristTurns = 50f;

        /// <summary>How far the chin is brought to the chest on its back, the head lifted on its front, and tipped away on a side (degrees).</summary>
        public const float ChinIn = 38f, HeadUp = 30f, HeadAside = 28f;

        /// <summary>Tucked: how far the spine and the chest are rounded forward (degrees).</summary>
        public const float TuckSpine = 26f, TuckChest = 22f, TuckChin = 50f;

        /// <summary>Tucked: how far the knees are drawn up (0: the legs before it, a little bent, as sitting in the air; 1: the knees to the chest, which spins it fastest).</summary>
        public static float TuckLegs = 0.5f;

        /// <summary>
        /// A hand is posed clear of its own trunk (the plan "Controlled falls", step 11): beside it, before it or
        /// behind it, never in it or through it to the far side. Off, the arm of a body on its side reached for the
        /// ground straight down through its own chest, and lay there.
        /// </summary>
        public static bool HandsClearOfTrunk = true;

        /// <summary>
        /// Falling, a shield is kept before the chest, its board between the body and what it is falling toward (the
        /// plan "Controlled falls", step 11): the forearm across the front of the body as a guard holds it, the elbow
        /// down and out. Going forward the body comes down on its shield; going backward the shield lies over it.
        /// Off: the shield arm reached for the ground as an empty one does, the board's edge met the ground first
        /// and stood there, and the body was held up on it and went over it sideways (a Large falling forward rose
        /// 27 cm again on its shield and came down on its back).
        /// </summary>
        public static bool ShieldKeptBeforeChest = true;

        /// <summary>The shield hand's place before the chest: this far in front of the chest's front, to its own side of the middle, and above the chest's middle (metres at a Medium's size).</summary>
        public const float ShieldBefore = 0.16f, ShieldAside = 0.08f, ShieldUp = 0.02f;

        /// <summary>How far outside the trunk a hand is posed (metres at a Medium's size), and how far inside its edge a hand has to be to be in it.</summary>
        public const float HandClears = 0.06f, HandInBy = 0.02f;

        /// <summary>
        /// Where a hand goes to be clear of its own trunk: <paramref name="hand"/> itself if it is clear, or the
        /// nearest place beside the trunk on the arm's own side, before it, or behind it that is not under the
        /// ground. The trunk is taken as a slab on the chest's axes, from under the hips to the neck, half as wide
        /// and as deep as <paramref name="half"/> says.
        /// </summary>
        /// <param name="arm">0: the left arm's hand, 1: the right's.</param>
        /// <param name="clear">How far outside the trunk's sides it has to be (metres; less than 0: this far inside still counts as clear).</param>
        /// <param name="moved">It was in the trunk, or reached through it.</param>
        public static Vector3 ClearOfTrunk(Transform pelvis, Transform chest, Transform head, Vector3 hand, int arm, float size, Vector2 half, float floor, float clear, out bool moved)
        {
            moved = false;
            if (pelvis == null || chest == null || head == null)
            {
                return hand;
            }

            if (half.x <= 0f || half.y <= 0f)
            {
                half = new Vector2(0.155f, 0.09f) * size;
            }

            Vector3 side = chest.right * (arm == 0 ? -1f : 1f), front = chest.forward, up = chest.up;
            float along = Vector3.Dot(hand - pelvis.position, up);
            if (along < -0.1f * size || along > Vector3.Dot(head.position - pelvis.position, up) + 0.05f * size)
            {
                return hand;
            }

            float w = half.x + clear, d = half.y + clear;
            float x = Vector3.Dot(hand - chest.position, side), z = Vector3.Dot(hand - chest.position, front);
            if (x >= w || Mathf.Abs(z) >= d)
            {
                return hand;
            }

            moved = true;
            // (Before it, always; beside it and behind it for a hand on its own side: one across the body has only the front to go round by.)
            Vector3 best = hand + front * (d - z);
            float least = best.y >= floor - 0.02f * size ? d - z : float.MaxValue;
            if (x >= 0f)
            {
                Vector3 beside = hand + side * (w - x), behind = hand - front * (d + z);
                if (beside.y >= floor - 0.02f * size && w - x < least)
                {
                    best = beside;
                    least = w - x;
                }

                if (behind.y >= floor - 0.02f * size && d + z < least)
                {
                    best = behind;
                    least = d + z;
                }

                if (least == float.MaxValue)
                {
                    // (None is over the ground: beside it, along the ground.)
                    best = beside;
                }
            }

            return best;
        }

        /// <summary>
        /// The pose put on <paramref name="anim"/>: its hips where the body's are (<paramref name="hips"/>), the rest
        /// from how the body is turned to the ground at <paramref name="ground"/> (its height) and which way it is
        /// going (<paramref name="going"/>, m/s).
        /// </summary>
        /// <param name="into">How far into its protective pose it is (0 as it loses its feet: the arms thrown out, the legs as they stood; 1: getting low and reaching).</param>
        /// <param name="holdsMain">Its right hand holds a weapon: the blade is kept out to its side and up, clear of its body.</param>
        /// <param name="handL">Its left hand has landed and is held there (world): the arm is posed onto it. Null: it reaches as it falls.</param>
        /// <param name="handR">The same, its right.</param>
        /// <param name="tuck">How far it is tucked (0 to 1): a body thrown off its feet curls up as it flies and tumbles: the back rounded, the chin down, the knees drawn up, the arms in before the chest (an arm that holds a weapon stays out with it).</param>
        /// <param name="trunk">Half its chest's width and depth (metres), for keeping its hands clear of it; zero: a Medium's at its size.</param>
        /// <param name="holdsShield">Its left arm bears a shield: kept before the chest (<see cref="ShieldKeptBeforeChest"/>).</param>
        public static void Pose(AnimRig anim, Transform hips, Vector3 going, float ground, float size, float into = 1f, bool holdsMain = false, Vector3? handL = null, Vector3? handR = null, float tuck = 0f, Vector2 trunk = default, bool holdsShield = false)
        {
            if (trunk.x <= 0f || trunk.y <= 0f)
            {
                trunk = new Vector2(0.155f, 0.09f) * size;
            }

            into = Mathf.Clamp01(into);
            tuck = Mathf.Clamp01(tuck);
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
            // (Tucked: rounded whichever way it goes.)
            Bend(anim, PartId.Spine, Mathf.Lerp(5f + 14f * back - 6f * forward, TuckSpine, tuck), 0f);
            Bend(anim, PartId.Chest, Mathf.Lerp(4f + 12f * back - 8f * forward, TuckChest, tuck), 0f);
            // The head: away from the ground. (Tucked: the chin down.)
            if (ProtectsHead)
            {
                Bend(anim, PartId.Head, Mathf.Lerp(ChinIn * back - HeadUp * forward, TuckChin, tuck), HeadAside * side * (1f - tuck));
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
                // (Tucked: the knees drawn up before the body.)
                ankle = Vector3.Lerp(ankle, thigh.position - up * (leg * Mathf.Lerp(0.78f, 0.42f, TuckLegs)) + front * (leg * Mathf.Lerp(0.42f, 0.36f, TuckLegs)), tuck);
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
                if (HandsClearOfTrunk)
                {
                    hand = ClearOfTrunk(anim.Part(PartId.Pelvis), anim.Part(PartId.Chest), anim.Part(PartId.Head), hand, a, size, trunk, floor, HandClears * size, out _);
                }

                if (hand.y < floor)
                {
                    hand.y = floor;
                }

                // (A hand that has landed stays where it landed: the arm is posed onto it, and bends as the body comes down over it.)
                var landed = a == 0 ? handL : handR;
                if (landed.HasValue)
                {
                    hand = landed.Value;
                }

                var chestNow = anim.Part(PartId.Chest);
                bool armed = a == 1 && holdsMain && anim.SlotWeaponR != null;
                bool shielded = a == 0 && holdsShield && ShieldKeptBeforeChest && !landed.HasValue;
                if (shielded)
                {
                    // (Its shield: before the chest, from the first moment to the last, tucked or not.)
                    hand = chestNow.position + chestNow.forward * (trunk.y + ShieldBefore * size) - chestNow.right * (ShieldAside * size) + chestNow.up * (ShieldUp * size);
                }
                else if (tuck > 0f && !landed.HasValue)
                {
                    // (Tucked: the hand in before the chest, the elbow down by the ribs. An arm that holds a weapon
                    // stays out to its side with it: a blade is not hugged.)
                    Vector3 inAt = armed
                        ? upper.position + (out_.normalized * 0.75f + chestNow.up * 0.3f + chestNow.forward * 0.3f).normalized * (0.85f * arm)
                        : chestNow.position + chestNow.forward * (0.2f * size) + chestNow.right * ((a == 0 ? 0.07f : -0.07f) * size) + chestNow.up * (0.04f * size);
                    hand = Vector3.Lerp(hand, inAt, tuck);
                }

                // (The elbows back and out of the trunk, as an arm that takes weight bends: pushing up from the ground,
                // sat back on the hands. Pointed by which way the body was going they bent forward going backward, as a
                // knee does, which no elbow can.)
                var chest = anim.Part(PartId.Chest);
                Vector3 elbow = -0.8f * chest.forward + 0.6f * chest.right * (a == 0 ? -1f : 1f) + 0.15f * chest.up;
                if (shielded)
                {
                    // (The elbow down and out by the ribs, as a guard has it.)
                    elbow = -0.6f * chest.up - 0.75f * chest.right - 0.2f * chest.forward;
                }
                else if (tuck > 0f && !armed && !landed.HasValue)
                {
                    elbow = Vector3.Slerp(elbow, -0.9f * chest.up + 0.45f * chest.right * (a == 0 ? -1f : 1f) - 0.1f * chest.forward, tuck);
                }

                var handPart = anim.Part(a == 0 ? PartId.HandL : PartId.HandR);
                TwoBoneIK.Solve(upper, anim.Part(a == 0 ? PartId.ForearmL : PartId.ForearmR), handPart, hand, elbow);
                if (a == 1 && holdsMain && anim.SlotWeaponR != null)
                {
                    // (What it holds: out to its side and up, away from its body and its head, whatever the arm does.)
                    Vector3 blade = (out_.normalized * 0.8f + Vector3.up * 0.45f + chest.forward * 0.2f).normalized;
                    // (As far as a wrist turns: asked for the whole of it, the hand was wanted half a turn from where its
                    // forearm had it, which no wrist does and no muscle of a wrist's strength holds.)
                    var asked = Quaternion.LookRotation(blade, Vector3.up) * Quaternion.Inverse(anim.SlotWeaponR.localRotation);
                    handPart.rotation = Quaternion.RotateTowards(handPart.parent.rotation * anim.RestLocalRotation(PartId.HandR), asked, WristTurns);
                }
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
