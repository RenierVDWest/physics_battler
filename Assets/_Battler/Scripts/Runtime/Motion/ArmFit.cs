using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// Milestone 9c: an arm posed by a two-bone solve and a hand turned in the world (the get-up's
    /// keys, the shield held up off the ground) is fitted inside its joints' limits. The shoulder and
    /// elbow, three angles each as the joint table measures them, are held inside their ranges, and a
    /// few damped least-squares steps bring the hand back to its place; the wrist takes up the hand's
    /// turn as far as its range goes, with the forearm's roll helping. A person can't turn a hand past
    /// where the wrist goes, so the rest of the turn is given up, never the place.
    /// </summary>
    public sealed class ArmFit
    {
        /// <summary>The steps of the fit.</summary>
        public const int Iterations = 14;

        private const float PlaceWeight = 60f;   // per metre
        private const float TurnWeight = 2f;   // per radian
        private const float SwingMost = 0.9f;
        private const float MigrateStep = 4f, CarryStep = 9f;   // degrees a step
        private const float LostEnergy = (0.04f * PlaceWeight) * (0.04f * PlaceWeight);

        private readonly float[] theta = new float[6], trial = new float[6], step = new float[6], warm = new float[6];
        private readonly float[] residual = new float[6], moved = new float[6];
        private readonly float[,] jacobian = new float[6, 6], system = new float[6, 7];
        private readonly bool[] frozen = new bool[6];
        private readonly float[] baseShape = new float[6];
        private bool warmKnown, baseKnown;
        private int lastFrame = -10;

        /// <summary>The place error (metres) and how far the hand's turn was given up (degrees) in the last fit.</summary>
        public float LastOff { get; private set; }

        public float LastTurnOff { get; private set; }

        /// <summary>
        /// Fits the arm (<paramref name="upper"/>, <paramref name="fore"/>, <paramref name="hand"/>) as
        /// it is posed now inside its joints' limits, its hand kept at <paramref name="target"/> and
        /// turned to <paramref name="handTurn"/> as near as the wrist allows. <paramref name="frame"/>
        /// counts the fits: from one to the next the arm carries on from its last shape (no jumps).
        /// </summary>
        public void Fit(AnimRig anim, PartId upper, PartId fore, PartId hand, Vector3 target, Quaternion handTurn, int frame)
        {
            var ids = new[] { upper, fore, hand };
            var handPart = anim.Part(hand);
            // (Within one step a second fit carries on from the same last shape as the first.)
            if (frame != lastFrame)
            {
                baseKnown = warmKnown && frame == lastFrame + 1;
                System.Array.Copy(warm, baseShape, 6);
                lastFrame = frame;
            }

            bool carry = baseKnown;

            // From the pose as it came (inside the ranges), and, carrying on, from the last shape too:
            // the better fit, the last shape's unless the other is clearly better.
            for (int p = 0; p < 2; p++)
            {
                Read(anim, ids[p], out theta[3 * p], out theta[3 * p + 1], out theta[3 * p + 2]);
            }

            Clamp(ids, theta);
            float fresh = Solve(anim, ids, handPart, target, handTurn);
            if (fresh > LostEnergy)
            {
                // (Stuck against a limit from there: tried again from the arm hanging at rest.)
                var stuck = (float[])theta.Clone();
                System.Array.Clear(theta, 0, 6);
                float relaxed = Solve(anim, ids, handPart, target, handTurn);
                if (relaxed < fresh)
                {
                    fresh = relaxed;
                }
                else
                {
                    System.Array.Copy(stuck, theta, 6);
                }
            }

            if (carry)
            {
                var freshTheta = (float[])theta.Clone();
                System.Array.Copy(baseShape, theta, 6);
                float carried = Solve(anim, ids, handPart, target, handTurn);
                // (No joint turns more than CarryStep in a step, however far the fit would take it.)
                for (int j = 0; j < 6; j++)
                {
                    theta[j] = baseShape[j] + Mathf.Clamp(theta[j] - baseShape[j], -CarryStep, CarryStep);
                }

                // (Its place off by more than 4 cm: lost. It heads for the fresh shape, no joint more
                // than MigrateStep a step: the arm moves over, it doesn't jump.)
                if (carried > LostEnergy && fresh < 0.5f * carried)
                {
                    for (int j = 0; j < 6; j++)
                    {
                        theta[j] = baseShape[j] + Mathf.Clamp(freshTheta[j] - baseShape[j], -MigrateStep, MigrateStep);
                    }
                }
            }

            Set(anim, ids, theta, handTurn);
            System.Array.Copy(theta, warm, 6);
            warmKnown = true;
            LastOff = (handPart.position - target).magnitude;
            LastTurnOff = Quaternion.Angle(handTurn, handPart.rotation);
        }

        /// <summary>Forgets the last shape (the next fit starts afresh).</summary>
        public void Reset()
        {
            warmKnown = false;
            baseKnown = false;
        }

        private float Solve(AnimRig anim, PartId[] ids, Transform handPart, Vector3 target, Quaternion handTurn)
        {
            Set(anim, ids, theta, handTurn);
            float lambda = 2f;
            float energy = Residuals(handPart, target, handTurn, residual);
            for (int it = 0; it < Iterations && energy > 1e-5f; it++)
            {
                for (int j = 0; j < 6; j++)
                {
                    System.Array.Copy(theta, trial, 6);
                    trial[j] += 0.5f;
                    Set(anim, ids, trial, handTurn);
                    Residuals(handPart, target, handTurn, moved);
                    for (int r = 0; r < 6; r++)
                    {
                        jacobian[r, j] = (moved[r] - residual[r]) / 0.5f;
                    }
                }

                // (An angle held at its limit that the step would push further out is left out of it.)
                System.Array.Clear(frozen, 0, 6);
                for (int pass = 0; pass < 2; pass++)
                {
                    if (!StepFor(lambda))
                    {
                        break;
                    }

                    bool again = false;
                    for (int j = 0; j < 6; j++)
                    {
                        if (!frozen[j] && AtLimit(ids, j, theta[j], step[j]))
                        {
                            frozen[j] = true;
                            again = true;
                        }
                    }

                    if (!again)
                    {
                        break;
                    }
                }

                for (int j = 0; j < 6; j++)
                {
                    trial[j] = theta[j] + (frozen[j] ? 0f : Mathf.Clamp(step[j], -30f, 30f));
                }

                Clamp(ids, trial);
                Set(anim, ids, trial, handTurn);
                float e = Residuals(handPart, target, handTurn, moved);
                if (e < energy)
                {
                    System.Array.Copy(trial, theta, 6);
                    System.Array.Copy(moved, residual, 6);
                    energy = e;
                    lambda = Mathf.Max(0.2f, lambda * 0.5f);
                }
                else
                {
                    lambda *= 3f;
                }
            }

            return energy;
        }

        /// <summary>(JᵀJ + λ²) step = -Jᵀ r, the frozen angles left out.</summary>
        private bool StepFor(float lambda)
        {
            for (int a = 0; a < 6; a++)
            {
                for (int b = 0; b < 6; b++)
                {
                    float sum = 0f;
                    if (!frozen[a] && !frozen[b])
                    {
                        for (int r = 0; r < 6; r++)
                        {
                            sum += jacobian[r, a] * jacobian[r, b];
                        }
                    }

                    system[a, b] = sum + (a == b ? lambda * lambda : 0f);
                }

                float rhs = 0f;
                if (!frozen[a])
                {
                    for (int r = 0; r < 6; r++)
                    {
                        rhs -= jacobian[r, a] * residual[r];
                    }
                }

                system[a, 6] = rhs;
            }

            return SolveInPlace(system, 6, step);
        }

        private static bool AtLimit(PartId[] ids, int j, float value, float change)
        {
            var spec = JointLimitTable.For(ids[j / 3]);
            float low, high;
            switch (j % 3)
            {
                case 0:
                    low = spec.pitchMin;
                    high = spec.pitchMax;
                    break;
                case 1:
                    low = -spec.twist;
                    high = spec.twist;
                    break;
                default:
                    low = -spec.swing;
                    high = spec.swing;
                    break;
            }

            return (value <= low + 0.01f && change < 0f) || (value >= high - 0.01f && change > 0f);
        }

        /// <summary>How far a joint of the pose is past its limits (degrees; 0 inside).</summary>
        public static float Past(AnimRig anim, PartId id)
        {
            return Read(anim, id, out _, out _, out _);
        }

        private static float Read(AnimRig anim, PartId id, out float pitch, out float y, out float z)
        {
            Quaternion local = Quaternion.Inverse(anim.RestLocalRotation(id)) * anim.Part(id).localRotation;
            return JointMath.PastLimits(local, JointLimitTable.For(id), out pitch, out y, out z);
        }

        /// <summary>The shoulder and elbow to their angles; the wrist turned to the hand's turn, inside its range.</summary>
        private static void Set(AnimRig anim, PartId[] ids, float[] angles, Quaternion handTurn)
        {
            for (int p = 0; p < 2; p++)
            {
                anim.Part(ids[p]).localRotation = anim.RestLocalRotation(ids[p]) * Joint(angles[3 * p], angles[3 * p + 1], angles[3 * p + 2]);
            }

            var hand = anim.Part(ids[2]);
            hand.rotation = handTurn;
            Read(anim, ids[2], out float pitch, out float y, out float z);
            ClampOne(ids[2], ref pitch, ref y, ref z);
            hand.localRotation = anim.RestLocalRotation(ids[2]) * Joint(pitch, y, z);
        }

        private static Quaternion Joint(float pitch, float y, float z)
        {
            float sy = Mathf.Sin(0.5f * y * Mathf.Deg2Rad), sz = Mathf.Sin(0.5f * z * Mathf.Deg2Rad);
            var swing = new Quaternion(0f, sy, sz, Mathf.Sqrt(Mathf.Max(0f, 1f - sy * sy - sz * sz)));
            return swing * Quaternion.AngleAxis(pitch, Vector3.right);
        }

        private static void Clamp(PartId[] ids, float[] angles)
        {
            for (int p = 0; p < 2; p++)
            {
                ClampOne(ids[p], ref angles[3 * p], ref angles[3 * p + 1], ref angles[3 * p + 2]);
            }
        }

        private static void ClampOne(PartId id, ref float pitch, ref float y, ref float z)
        {
            var spec = JointLimitTable.For(id);
            pitch = Mathf.Clamp(pitch, spec.pitchMin, spec.pitchMax);
            y = Mathf.Clamp(y, -spec.twist, spec.twist);
            z = Mathf.Clamp(z, -spec.swing, spec.swing);
            float sy = Mathf.Sin(0.5f * y * Mathf.Deg2Rad), sz = Mathf.Sin(0.5f * z * Mathf.Deg2Rad);
            float both = sy * sy + sz * sz;
            if (both > SwingMost)
            {
                float k = Mathf.Sqrt(SwingMost / both);
                y = 2f * Mathf.Asin(sy * k) * Mathf.Rad2Deg;
                z = 2f * Mathf.Asin(sz * k) * Mathf.Rad2Deg;
            }
        }

        private static float Residuals(Transform hand, Vector3 target, Quaternion handTurn, float[] r)
        {
            Vector3 off = (hand.position - target) * PlaceWeight;
            Quaternion d = handTurn * Quaternion.Inverse(hand.rotation);
            if (d.w < 0f)
            {
                d = new Quaternion(-d.x, -d.y, -d.z, -d.w);
            }

            d.ToAngleAxis(out float angle, out Vector3 axis);
            Vector3 turn = float.IsFinite(axis.x) && axis.sqrMagnitude > 0.5f ? axis.normalized * (angle * Mathf.Deg2Rad * TurnWeight) : Vector3.zero;
            r[0] = off.x;
            r[1] = off.y;
            r[2] = off.z;
            r[3] = turn.x;
            r[4] = turn.y;
            r[5] = turn.z;
            return off.sqrMagnitude + turn.sqrMagnitude;
        }

        /// <summary>Gaussian elimination with partial pivoting on an n × (n + 1) system.</summary>
        private static bool SolveInPlace(float[,] m, int n, float[] x)
        {
            for (int c = 0; c < n; c++)
            {
                int pivot = c;
                for (int r = c + 1; r < n; r++)
                {
                    if (Mathf.Abs(m[r, c]) > Mathf.Abs(m[pivot, c]))
                    {
                        pivot = r;
                    }
                }

                if (Mathf.Abs(m[pivot, c]) < 1e-9f)
                {
                    return false;
                }

                if (pivot != c)
                {
                    for (int k = c; k <= n; k++)
                    {
                        (m[c, k], m[pivot, k]) = (m[pivot, k], m[c, k]);
                    }
                }

                for (int r = c + 1; r < n; r++)
                {
                    float f = m[r, c] / m[c, c];
                    for (int k = c; k <= n; k++)
                    {
                        m[r, k] -= f * m[c, k];
                    }
                }
            }

            for (int r = n - 1; r >= 0; r--)
            {
                float sum = m[r, n];
                for (int k = r + 1; k < n; k++)
                {
                    sum -= m[r, k] * x[k];
                }

                x[r] = sum / m[r, r];
            }

            return true;
        }
    }
}
