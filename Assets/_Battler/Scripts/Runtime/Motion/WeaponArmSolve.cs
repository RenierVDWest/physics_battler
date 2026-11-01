using PhysicsBattler.Combat;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// Milestone 9c: the weapon arm, solved in its joints' own angles. Each joint (the shoulder, the
    /// elbow and forearm, the wrist) is three angles as the joint table measures them (pitch, then the
    /// turns about Y and Z), and each angle stays inside the table's range: a shape the arm can't make
    /// is never chosen. Within those ranges the arm is moved, a few damped least-squares steps a pose,
    /// to weigh up, together: the blow's striking part where it is asked to be (most of all at the
    /// striking moment), the weapon pointing the asked way, the wrist inside its easy range, the elbow
    /// toward its pole (down and out), and the arm and blade out of its own body. A playing arm starts
    /// from last step's shape, carried on as it was moving, and no joint turns further a step than
    /// <see cref="ArmAngleStepCalm"/> (<see cref="ArmAngleStepStrike"/> at the striking moment): it
    /// never jumps.
    /// </summary>
    public sealed partial class PosePipeline
    {
        private static readonly PartId[] SolvedArm = { PartId.UpperArmR, PartId.ForearmR, PartId.HandR };

        /// <summary>The weapon arm's last playing shape: the shoulder, elbow and wrist angles (degrees).</summary>
        private readonly float[] armAngles = new float[9];
        private bool armAnglesKnown;

        /// <summary>How far, at most, each of the arm's joints turns in one playing step (degrees, its three angles together): at the striking moment, and elsewhere.</summary>
        public const float ArmAngleStepStrike = 90f, ArmAngleStepCalm = 25f;

        /// <summary>This pose's limit (between the two, by how near the striking moment it is).</summary>
        private float armAngleStep = ArmAngleStepCalm;

        /// <summary>The solve's damped least-squares steps per pose: playing (from last step's shape), and tried or fresh.</summary>
        public const int ArmIterationsPlaying = 8, ArmIterationsFresh = 12;

        /// <summary>
        /// What each kind of miss counts (a unit of each counts alike): the striking part off its
        /// target at the striking moment and away from it (per metre), the weapon off its asked way (per
        /// unit of direction, about a radian), the wrist past its easy range and the elbow off its pole
        /// (per degree), the arm or blade inside the margin of its own body (per metre), and a playing
        /// arm's change since last step (per degree; at the striking moment, and elsewhere).
        /// </summary>
        public const float ArmAimWeight = 150f, ArmAimWeightAway = 25f, ArmPointWeight = 6f, ArmWristWeight = 0.5f,
            ArmPoleWeight = 0.04f, ArmBodyWeight = 300f, ArmStillWeight = 0.03f, ArmStillCalm = 0.15f;

        /// <summary>How near its striking moment a blow's striking part is held to its path at full weight (share of progress), and over how much more it eases to the lower weight.</summary>
        public const float AimHeldNear = 0.05f, AimHeldRamp = 0.15f;

        private readonly float[] armResidual = new float[32];
        private readonly float[] armTrial = new float[32];
        private readonly float[,] armJacobian = new float[32, 9];

        /// <summary>While the Jacobian is taken: the arm is well clear of its body, so its nearness isn't measured.</summary>
        private bool armBodyClear;
        private float armLastClear;

        /// <summary>How far clear of the margin the arm must be for small turns not to be measured against the body (metres).</summary>
        private const float ArmClearSkip = 0.04f;

        /// <summary>The shape the arm is drawn toward (the planned striking shape), and how strongly (per degree; 0: none).</summary>
        private float[] armPlan;
        private float armPlanWeight;

        /// <summary>How strongly a playing arm is drawn to its blow's planned striking shape at the striking moment (per degree).</summary>
        public const float ArmPlanWeight = 0.2f;
        private readonly float[] armTheta = new float[9];
        private readonly float[] armStart = new float[9];

        /// <summary>
        /// Solves the weapon arm for this pose. <paramref name="anchorLocal"/> is the point of the
        /// weapon (in its slot) that must be at <paramref name="anchor"/>; <paramref name="aimWeight"/>
        /// how much that counts per metre.
        /// </summary>
        private readonly float[] armAnglesWas = new float[9], armAngleSpeedWas = new float[9];
        private bool armAnglesWasKnown;

        /// <summary>Keeps last step's arm angles, so a second solve this step (after the body leans) follows from them too.</summary>
        private void KeepArmAngles()
        {
            System.Array.Copy(armAngles, armAnglesWas, 9);
            System.Array.Copy(armAngleSpeed, armAngleSpeedWas, 9);
            armAnglesWasKnown = armAnglesKnown;
        }

        private void RestoreArmAngles()
        {
            if (!composingDry)
            {
                System.Array.Copy(armAnglesWas, armAngles, 9);
                System.Array.Copy(armAngleSpeedWas, armAngleSpeed, 9);
                armAnglesKnown = armAnglesWasKnown;
            }
        }

        private void SolveWeaponArm(Transform slot, Vector3 anchorLocal, Vector3 anchor, Quaternion asked, Vector3 pole,
            Vector2 easy, BodyClearance.Posed torso, float length, float aimWeight, bool playing, float lead, float[] plan, float planWeight, float nearStrike)
        {
            armAngleStep = Mathf.Lerp(ArmAngleStepCalm, ArmAngleStepStrike, nearStrike);
            armPlan = plan != null && !float.IsNaN(plan[0]) ? plan : null;
            armPlanWeight = armPlan != null ? planWeight : 0f;
            var upper = anim.Part(PartId.UpperArmR);
            var fore = anim.Part(PartId.ForearmR);
            var hand = anim.Part(PartId.HandR);
            Vector3 askedWay = asked * Vector3.forward;
            bool warm = playing && armAnglesKnown;
            // (Away from the striking moment, in the guard, a feint, the wind-up and follow-through, the
            // arm holds its shape more: it doesn't turn itself over for a little of the weapon's way.)
            float still = Mathf.Lerp(ArmStillCalm, ArmStillWeight, nearStrike);
            if (warm)
            {
                // (From last step's shape, carried on as it was moving: a fast blow isn't left behind.)
                for (int j = 0; j < 9; j++)
                {
                    armTheta[j] = armAngles[j] + lead * armAngleSpeed[j];
                }

                CapStep(armTheta, armAngles);
                System.Array.Copy(armAngles, armStart, 9);
            }
            else
            {
                FreshArm(armTheta, slot, anchorLocal, anchor, asked, pole);
                System.Array.Copy(armTheta, armStart, 9);
            }

            float energy = Optimize(armTheta, warm ? ArmIterationsPlaying : ArmIterationsFresh, warm,
                slot, anchorLocal, anchor, askedWay, pole, easy, torso, length, aimWeight, warm ? still : 0.005f);

            // (A playing arm that has lost its way, wedged against its limits or left behind: a fresh
            // solve too, and where that is better it heads there, no further a step than it may move.)
            float offNow = (slot.position + slot.rotation * anchorLocal - anchor).magnitude;
            bool lost = nearStrike > 0f && (offNow > ArmLostOff || armLastClear < 0f);
            if (warm && lost && energy > ArmLostEnergy)
            {
                var fresh = new float[9];
                FreshArm(fresh, slot, anchorLocal, anchor, asked, pole);
                Optimize(fresh, ArmIterationsFresh, false, slot, anchorLocal, anchor, askedWay, pole, easy, torso, length, aimWeight, 0f);
                var toward = (float[])fresh.Clone();
                CapStep(toward, armStart);
                float e = Optimize(toward, ArmIterationsPlaying, true, slot, anchorLocal, anchor, askedWay, pole, easy, torso, length, aimWeight, still);
                if (e < energy)
                {
                    System.Array.Copy(toward, armTheta, 9);
                    energy = e;
                }
            }

            SetArm(armTheta);
            if (playing)
            {
                for (int j = 0; j < 9; j++)
                {
                    armAngleSpeed[j] = armAnglesKnown ? armTheta[j] - armAngles[j] : 0f;
                }

                System.Array.Copy(armTheta, armAngles, 9);
                armAnglesKnown = true;
            }
        }

        /// <summary>A fresh start: the elbow toward its pole with the hand on the grip as asked, then inside the ranges.</summary>
        private void FreshArm(float[] theta, Transform slot, Vector3 anchorLocal, Vector3 anchor, Quaternion asked, Vector3 pole)
        {
            var upper = anim.Part(PartId.UpperArmR);
            var fore = anim.Part(PartId.ForearmR);
            var hand = anim.Part(PartId.HandR);
            TwoBoneIK.Solve(upper, fore, hand, HandFor(anchor - asked * anchorLocal, asked, slot), pole);
            hand.rotation = asked * Quaternion.Inverse(slot.localRotation);
            ReadArm(theta);
            ClampArm(theta);
        }

        /// <summary>How much each of the arm's angles moved last step (degrees): a playing solve starts that far on.</summary>
        private readonly float[] armAngleSpeed = new float[9];

        /// <summary>
        /// A playing arm has lost its way when, near the striking moment, its striking part is this far
        /// off (metres), or it is inside its own body, and its misses add up past ArmLostEnergy (squared units).
        /// </summary>
        public const float ArmLostOff = 0.02f, ArmLostEnergy = 25f;

        /// <summary>
        /// Damped least-squares steps from <paramref name="theta"/> (changed in place), every angle kept
        /// in its range; an angle at the end of its range that the step would push further is held
        /// there and the rest move without it. <paramref name="capped"/>: no angle moves more than
        /// this pose's step limit from where the step started. Returns the misses' sum of squares.
        /// </summary>
        private float Optimize(float[] theta, int iterations, bool capped, Transform slot, Vector3 anchorLocal, Vector3 anchor,
            Vector3 askedWay, Vector3 pole, Vector2 easy, BodyClearance.Posed torso, float length, float aimWeight, float still)
        {
            int n = ArmResiduals(theta, armResidual, slot, anchorLocal, anchor, askedWay, pole, easy, torso, length, aimWeight, still);
            float energy = Energy(armResidual, n);
            float lambda = 1e-2f;
            var frozen = new bool[9];
            var step = new float[9];
            var trial = new float[32];
            float clearNow = armLastClear;
            bool settled = false;
            for (int it = 0; it < iterations; it++)
            {
                const float h = 0.5f;
                armBodyClear = clearNow > ArmBodyMargin + ArmClearSkip;
                for (int j = 0; j < 9; j++)
                {
                    float keep = theta[j];
                    theta[j] = keep + h;
                    ArmResiduals(theta, armTrial, slot, anchorLocal, anchor, askedWay, pole, easy, torso, length, aimWeight, still);
                    theta[j] = keep;
                    for (int i = 0; i < n; i++)
                    {
                        armJacobian[i, j] = (armTrial[i] - armResidual[i]) / h;
                    }
                }

                armBodyClear = false;

                bool improved = false;
                for (int tries = 0; tries < 4 && !improved; tries++)
                {
                    System.Array.Clear(frozen, 0, 9);
                    float[] delta = null;
                    for (int pass = 0; pass < 3; pass++)
                    {
                        delta = DampedStep(armJacobian, armResidual, n, lambda, frozen);
                        bool more = false;
                        for (int j = 0; j < 9; j++)
                        {
                            if (!frozen[j] && AtEnd(theta, j, delta[j]))
                            {
                                frozen[j] = true;
                                more = true;
                            }
                        }

                        if (!more)
                        {
                            break;
                        }
                    }

                    for (int j = 0; j < 9; j++)
                    {
                        step[j] = theta[j] + Mathf.Clamp(delta[j], -60f, 60f);
                    }

                    ClampArm(step);
                    if (capped)
                    {
                        CapStep(step, armStart);
                    }

                    ArmResiduals(step, trial, slot, anchorLocal, anchor, askedWay, pole, easy, torso, length, aimWeight, still);
                    float e = Energy(trial, n);
                    if (e < energy)
                    {
                        System.Array.Copy(step, theta, 9);
                        System.Array.Copy(trial, armResidual, n);
                        settled = energy - e < ArmSettled * energy + 1e-4f;
                        energy = e;
                        clearNow = armLastClear;
                        lambda = Mathf.Max(1e-4f, lambda * 0.3f);
                        improved = true;
                    }
                    else
                    {
                        lambda *= 10f;
                    }
                }

                if (!improved || settled)
                {
                    break;
                }
            }

            SetArm(theta);
            return energy;
        }

        /// <summary>
        /// Holds each joint's change from <paramref name="from"/> to at most this pose's step limit
        /// (its three angles together), then inside the ranges.
        /// </summary>
        private void CapStep(float[] theta, float[] from)
        {
            for (int p = 0; p < 3; p++)
            {
                float a = theta[3 * p] - from[3 * p], b = theta[3 * p + 1] - from[3 * p + 1], c = theta[3 * p + 2] - from[3 * p + 2];
                float m = Mathf.Sqrt(a * a + b * b + c * c);
                if (m > armAngleStep)
                {
                    float k = armAngleStep / m;
                    theta[3 * p] = from[3 * p] + a * k;
                    theta[3 * p + 1] = from[3 * p + 1] + b * k;
                    theta[3 * p + 2] = from[3 * p + 2] + c * k;
                }
            }

            ClampArm(theta);
        }

        /// <summary>A step that takes off less than this share of the misses: the solve has settled.</summary>
        private const float ArmSettled = 0.01f;

        /// <summary>Whether an angle is at the end of its range and a step of <paramref name="delta"/> would push it further.</summary>
        private static bool AtEnd(float[] theta, int j, float delta)
        {
            var spec = JointLimitTable.For(SolvedArm[j / 3]);
            int k = j % 3;
            float lo = k == 0 ? spec.pitchMin : k == 1 ? -spec.twist : -spec.swing;
            float hi = k == 0 ? spec.pitchMax : k == 1 ? spec.twist : spec.swing;
            return (theta[j] >= hi - 0.05f && delta > 0f) || (theta[j] <= lo + 0.05f && delta < 0f);
        }

        private static float Energy(float[] r, int n)
        {
            float e = 0f;
            for (int i = 0; i < n; i++)
            {
                e += r[i] * r[i];
            }

            return e;
        }

        /// <summary>The damped least-squares step: (JᵀJ + λ·diag) δ = −Jᵀr.</summary>
        private static float[] DampedStep(float[,] J, float[] r, int n, float lambda, bool[] frozen)
        {
            var a = new float[9, 10];
            for (int p = 0; p < 9; p++)
            {
                for (int q = 0; q < 9; q++)
                {
                    float s = 0f;
                    for (int i = 0; i < n; i++)
                    {
                        s += J[i, p] * J[i, q];
                    }

                    a[p, q] = s;
                }

                float g = 0f;
                for (int i = 0; i < n; i++)
                {
                    g += J[i, p] * r[i];
                }

                a[p, 9] = -g;
            }

            for (int p = 0; p < 9; p++)
            {
                a[p, p] += lambda * (a[p, p] + 1e-3f);
                if (frozen[p])
                {
                    // (Held: its row says only that it doesn't move.)
                    for (int q = 0; q < 9; q++)
                    {
                        a[p, q] = 0f;
                        a[q, p] = q == p ? 1f : 0f;
                    }

                    a[p, p] = 1f;
                    a[p, 9] = 0f;
                }
            }

            // (Gaussian elimination with partial pivoting.)
            for (int c = 0; c < 9; c++)
            {
                int pivot = c;
                for (int k = c + 1; k < 9; k++)
                {
                    if (Mathf.Abs(a[k, c]) > Mathf.Abs(a[pivot, c]))
                    {
                        pivot = k;
                    }
                }

                if (pivot != c)
                {
                    for (int q = 0; q < 10; q++)
                    {
                        (a[c, q], a[pivot, q]) = (a[pivot, q], a[c, q]);
                    }
                }

                float d = a[c, c];
                if (Mathf.Abs(d) < 1e-9f)
                {
                    continue;
                }

                for (int k = c + 1; k < 9; k++)
                {
                    float f = a[k, c] / d;
                    for (int q = c; q < 10; q++)
                    {
                        a[k, q] -= f * a[c, q];
                    }
                }
            }

            var x = new float[9];
            for (int c = 8; c >= 0; c--)
            {
                float s = a[c, 9];
                for (int q = c + 1; q < 9; q++)
                {
                    s -= a[c, q] * x[q];
                }

                x[c] = Mathf.Abs(a[c, c]) > 1e-9f ? s / a[c, c] : 0f;
            }

            return x;
        }

        /// <summary>The weighted misses of the arm at these angles (sets the arm). Returns how many.</summary>
        private int ArmResiduals(float[] theta, float[] r, Transform slot, Vector3 anchorLocal, Vector3 anchor, Vector3 askedWay,
            Vector3 pole, Vector2 easy, BodyClearance.Posed torso, float length, float aimWeight, float still)
        {
            SetArm(theta);
            var upper = anim.Part(PartId.UpperArmR);
            var fore = anim.Part(PartId.ForearmR);
            var hand = anim.Part(PartId.HandR);
            int k = 0;
            // The striking part on its target.
            Vector3 off = slot.position + slot.rotation * anchorLocal - anchor;
            r[k++] = off.x * aimWeight;
            r[k++] = off.y * aimWeight;
            r[k++] = off.z * aimWeight;
            // The weapon pointing the asked way.
            Vector3 way = slot.rotation * Vector3.forward - askedWay;
            r[k++] = way.x * ArmPointWeight;
            r[k++] = way.y * ArmPointWeight;
            r[k++] = way.z * ArmPointWeight;
            // The wrist inside its easy range: the bend off the forearm's line, and the roll about it.
            WristOf(fore, hand, out float bend, out float roll);
            r[k++] = Mathf.Max(0f, bend - easy.x) * ArmWristWeight;
            r[k++] = Mathf.Max(0f, Mathf.Abs(roll) - easy.y) * ArmWristWeight;
            // The elbow toward its pole, round the shoulder-to-wrist line.
            Vector3 axis = hand.position - upper.position;
            Vector3 across = Vector3.ProjectOnPlane(fore.position - upper.position, axis);
            Vector3 poleAcross = Vector3.ProjectOnPlane(pole, axis);
            r[k++] = across.sqrMagnitude > 1e-8f && poleAcross.sqrMagnitude > 1e-8f ? Vector3.Angle(across, poleAcross) * ArmPoleWeight : 0f;
            // Out of its own body (a small turn of a joint can't bring an arm well clear of it in: then
            // not measured again).
            float clear = float.MaxValue;
            if (torso != null && !armBodyClear)
            {
                clear = Mathf.Min(torso.Arm(upper.position, fore.position, hand.position, slot.position),
                    torso.Blade(slot.position, slot.position + slot.rotation * (Vector3.forward * length)));
            }

            armLastClear = clear;
            r[k++] = Mathf.Max(0f, ArmBodyMargin - clear) * ArmBodyWeight;
            // Little change from where it started.
            for (int j = 0; j < 9; j++)
            {
                r[k++] = (theta[j] - armStart[j]) * still;
            }

            // Toward the planned striking shape.
            if (armPlanWeight > 0f && armPlan != null)
            {
                for (int j = 0; j < 9; j++)
                {
                    r[k++] = (theta[j] - armPlan[j]) * armPlanWeight;
                }
            }

            return k;
        }

        /// <summary>The wrist's bend off the forearm's line and its roll about it (degrees).</summary>
        private void WristOf(Transform fore, Transform hand, out float bend, out float roll)
        {
            Vector3 line = anim.RestLocalPosition(PartId.HandR).normalized;
            Quaternion rel = Quaternion.Inverse(fore.rotation) * hand.rotation;
            if (rel.w < 0f)
            {
                rel = new Quaternion(-rel.x, -rel.y, -rel.z, -rel.w);
            }

            float along = rel.x * line.x + rel.y * line.y + rel.z * line.z;
            roll = 2f * Mathf.Atan2(along, rel.w) * Mathf.Rad2Deg;
            bend = Vector3.Angle(line, rel * line);
        }

        /// <summary>Poses the weapon arm at these angles (each joint: pitch, then about Y and Z, from its rest).</summary>
        private void SetArm(float[] theta)
        {
            for (int p = 0; p < 3; p++)
            {
                var id = SolvedArm[p];
                float pitch = theta[3 * p], y = theta[3 * p + 1], z = theta[3 * p + 2];
                float sy = Mathf.Sin(0.5f * y * Mathf.Deg2Rad), sz = Mathf.Sin(0.5f * z * Mathf.Deg2Rad);
                var swing = new Quaternion(0f, sy, sz, Mathf.Sqrt(Mathf.Max(0f, 1f - sy * sy - sz * sz)));
                anim.Part(id).localRotation = anim.RestLocalRotation(id) * (swing * Quaternion.AngleAxis(pitch, Vector3.right));
            }
        }

        /// <summary>Reads the weapon arm's angles as it is posed now.</summary>
        private void ReadArm(float[] theta)
        {
            for (int p = 0; p < 3; p++)
            {
                var id = SolvedArm[p];
                JointMath.PastLimits(Quaternion.Inverse(anim.RestLocalRotation(id)) * anim.Part(id).localRotation, JointLimitTable.For(id),
                    out theta[3 * p], out theta[3 * p + 1], out theta[3 * p + 2]);
            }
        }

        /// <summary>How much of a joint's swing its two turns may take together (sin² of the half-turns; 1 is a half turn).</summary>
        private const float SwingMost = 0.9f;

        /// <summary>Holds each of the arm's angles inside the joint table's range.</summary>
        private static void ClampArm(float[] theta)
        {
            for (int p = 0; p < 3; p++)
            {
                var spec = JointLimitTable.For(SolvedArm[p]);
                theta[3 * p] = Mathf.Clamp(theta[3 * p], spec.pitchMin, spec.pitchMax);
                theta[3 * p + 1] = Mathf.Clamp(theta[3 * p + 1], -spec.twist, spec.twist);
                theta[3 * p + 2] = Mathf.Clamp(theta[3 * p + 2], -spec.swing, spec.swing);
                // (The two turns share one swing: both at their ends at once is no rotation at all. Kept
                // inside, as a joint's cone of swing keeps them.)
                float sy = Mathf.Sin(0.5f * theta[3 * p + 1] * Mathf.Deg2Rad), sz = Mathf.Sin(0.5f * theta[3 * p + 2] * Mathf.Deg2Rad);
                float both = sy * sy + sz * sz;
                if (both > SwingMost)
                {
                    float k = Mathf.Sqrt(SwingMost / both);
                    theta[3 * p + 1] = 2f * Mathf.Asin(sy * k) * Mathf.Rad2Deg;
                    theta[3 * p + 2] = 2f * Mathf.Asin(sz * k) * Mathf.Rad2Deg;
                }
            }
        }
    }
}
