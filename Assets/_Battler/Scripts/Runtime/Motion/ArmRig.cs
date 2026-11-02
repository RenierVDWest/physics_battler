using PhysicsBattler.Combat;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// What an arm is asked to do (the arm rig's controls, as an animator's rig has them): put a point
    /// of the hand somewhere, turn the hand a way, and point the elbow toward a null.
    /// </summary>
    public struct ArmGoal
    {
        /// <summary>Where the hand's point goes (world).</summary>
        public Vector3 point;

        /// <summary>The point, in the hand's own frame (the grip, a weapon's striking part, a shield's face...; zero: the wrist).</summary>
        public Vector3 pointInHand;

        /// <summary>How the hand is wanted turned (world).</summary>
        public Quaternion handTurn;

        /// <summary>A line in the hand's frame the hand may roll about when the wrist can't take the turn (a weapon's grip line); zero: none.</summary>
        public Vector3 rollAxisInHand;

        /// <summary>How far it may roll about that line, each way (degrees).</summary>
        public float rollAllowance;

        /// <summary>
        /// The elbow null: the way the elbow points, from the shoulder, in the chest's frame with x out
        /// to this arm's side (mirrored for the left arm), y up, z forward. The elbow goes as near it as
        /// the arm allows. Zero: the arm's relaxed way (<see cref="ArmRig.Relaxed"/>).
        /// </summary>
        public Vector3 elbowNull;

        /// <summary>How far the arm keeps from its own torso (metres; 0: not checked).</summary>
        public float clearance;

        public static ArmGoal Hand(Vector3 point, Vector3 pointInHand, Quaternion handTurn, Vector3 elbowNull) => new ArmGoal
        {
            point = point, pointInHand = pointInHand, handTurn = handTurn, elbowNull = elbowNull,
        };
    }

    /// <summary>What the arm rig made of a goal, for the caller, the log and the gizmo.</summary>
    public struct ArmSolve
    {
        /// <summary>How far the hand's point is from where it was asked (metres).</summary>
        public float pointMiss;

        /// <summary>How far the hand's turn is from the one asked (degrees, the roll it was allowed included).</summary>
        public float turnMiss;

        /// <summary>The part of the turn the wrist couldn't take (degrees; 0: the arm made it, perhaps with a roll).</summary>
        public float wristExcess;

        /// <summary>How far the elbow is round its circle from the null's way (degrees).</summary>
        public float swivelOff;

        /// <summary>The hand's roll about its allowed line (degrees).</summary>
        public float roll;

        /// <summary>The forearm's twist (palm over), the wrist's flexion and its deviation toward the thumb (+) or little finger (-) (degrees).</summary>
        public float twist, flexion, deviation;

        /// <summary>The elbow's bend (degrees, 0 straight).</summary>
        public float elbowBend;

        /// <summary>How far the shoulder is past its limits (degrees).</summary>
        public float shoulderPast;

        /// <summary>How close the arm comes to its torso, past the clearance asked (metres; 0: clear).</summary>
        public float clearanceShort;

        /// <summary>For the gizmo: the shoulder, the elbow, the wrist, the elbow circle's centre and radius, and the null's way (world).</summary>
        public Vector3 shoulder, elbow, wrist, circleCentre, nullWay;

        public float circleRadius;
    }

    /// <summary>
    /// Milestone 10 rebuild: the arm rig. One exact solve, with no memory, from the arm's controls
    /// (<see cref="ArmGoal"/>) to its joints, used by every arm (weapon, shield, bare hand, getting up).
    ///
    /// 1. Reach sets the elbow's bend (law of cosines), eased near a straight arm.
    /// 2. The elbow sits on a circle round the shoulder-to-wrist line; the null picks where.
    /// 3. The elbow is a hinge, bending about its anatomical axis only.
    /// 4. The hand's turn relative to the forearm splits into the forearm's twist and the wrist's
    ///    swing, clamped to the shape PhysX holds (a twist range, a swing ellipse in the tangent of a
    ///    quarter of the angle), with the deviation toward the thumb kept under 20 degrees.
    /// 5. What the wrist can't take moves the elbow round its circle, to the nearest place where it
    ///    fits (Tolani, Goswami and Badler 2000: 24 places, then refined), and only then rolls the
    ///    hand about its allowed line.
    /// 6. What still doesn't fit is reported (<see cref="ArmSolve"/>).
    ///
    /// The same goal gives the same arm, whatever the arm was doing before: there is nothing to wedge.
    /// </summary>
    public static class ArmRig
    {
        /// <summary>The forearm's twist each way from a hand held thumb forward (degrees).</summary>
        public const float TwistLimit = 80f;

        /// <summary>The wrist's flexion and extension, and its deviation (the ellipse PhysX will hold; degrees).</summary>
        public const float FlexLimit = 60f, DeviationLimit = 35f;

        /// <summary>Deviation toward the thumb (degrees); toward the little finger the ellipse's 35.</summary>
        public const float RadialLimit = 20f;

        /// <summary>The elbow's bend at most (degrees).</summary>
        public const float ElbowBendLimit = 145f;

        /// <summary>How far the upper arm may point from the relaxed forward reach (degrees): straight up, out, down and across, not behind the back.</summary>
        public const float ShoulderReach = 125f;

        /// <summary>A reach past this share of the arm's length is eased (soft IK: no pop as the arm straightens).</summary>
        public const float SoftReach = 0.97f;

        // The search's weights: a degree of wrist past its limits costs as much as 20
        // degrees of elbow off its null, a degree of shoulder past its reach ten times that (the shoulder gives last), a degree of roll as much as 2.
        private const float SwivelCost = 0.01f, WristCost = 4f, ShoulderCost = 40f, RollCost = 0.04f, ClearanceCost = 40f;
        private const int Samples = 24;

        /// <summary>The arm's relaxed way, where the elbow hangs: the null's way with none given (chest frame, x out).</summary>
        public static readonly Vector3 Relaxed = new Vector3(0.35f, -1f, -0.15f);

        // Where an arm reaches when relaxed (chest frame, x out): the swivel's own reference is carried
        // from here (parallel transport), so it turns smoothly everywhere except straight back across
        // the body, where no arm goes.
        private static readonly Vector3 NeutralReach = new Vector3(0.3f, -0.5f, 0.8f);

        public static ArmSolve Solve(AnimRig anim, bool right, in ArmGoal goal, BodyClearance.Posed torso = null)
        {
            var chain = Chain.Of(anim, right);
            var g = goal;
            if (g.handTurn.w == 0f && g.handTurn.x == 0f && g.handTurn.y == 0f && g.handTurn.z == 0f)
            {
                g.handTurn = chain.hand.rotation;
            }

            // The whole circle searched once; then, if the hand came out turned otherwise than asked
            // (its point then lands elsewhere), the wrist is put where the point lands with the hand as
            // it is and the search refined about the elbow already found.
            Eval best = Search(chain, g, g.point - g.handTurn * g.pointInHand, torso, null);
            if (g.pointInHand.sqrMagnitude > 1e-10f && Quaternion.Angle(best.hand, g.handTurn) > 0.01f)
            {
                best = Search(chain, g, g.point - best.hand * g.pointInHand, torso, best.swivel);
            }

            chain.Apply(best);
            var solve = best.Report(chain, g);
            solve.pointMiss = Vector3.Distance(chain.hand.position + chain.hand.rotation * g.pointInHand, g.point);
            return solve;
        }

        /// <summary>The way an elbow null points, in the world, for this arm (chest frame, x out to the arm's side).</summary>
        public static Vector3 NullWay(AnimRig anim, bool right, Vector3 elbowNull)
        {
            var chest = anim.Part(PartId.Chest).rotation;
            Vector3 n = elbowNull.sqrMagnitude > 1e-8f ? elbowNull : Relaxed;
            return chest * new Vector3(right ? n.x : -n.x, n.y, n.z);
        }

        /// <summary>The wrist's frame in the hand's (and forearm's) rest frame: x along the forearm, y toward the thumb (forward at rest), z = x × y.</summary>
        public static Quaternion WristFrame(AnimRig anim, bool right)
        {
            Vector3 along = anim.RestLocalPosition(right ? PartId.HandR : PartId.HandL).normalized;
            Vector3 thumb = Vector3.forward - along * Vector3.Dot(Vector3.forward, along);
            return Quaternion.LookRotation(Vector3.Cross(along, thumb.normalized), Vector3.Cross(Vector3.Cross(along, thumb.normalized), along));
        }

        /// <summary>The elbow's hinge axis in the upper arm's rest frame (bending about it by a positive angle brings the forearm forward).</summary>
        public static Vector3 ElbowAxis(AnimRig anim, bool right)
        {
            Vector3 along = anim.RestLocalPosition(right ? PartId.ForearmR : PartId.ForearmL).normalized;
            return Vector3.Cross(along, Vector3.forward).normalized;
        }

        /// <summary>
        /// The wrist's turn as PhysX splits it, in the wrist's frame: the forearm's twist, the flexion
        /// (about y) and the deviation toward the thumb (about z, +). Degrees; also how far past the
        /// limits (0 inside).
        /// </summary>
        public static float WristAngles(Quaternion handLocal, Quaternion wristFrame, out float twist, out float flexion, out float deviation)
        {
            var q = Quaternion.Inverse(wristFrame) * handLocal * wristFrame;
            Split(q, out twist, out float ty, out float tz);
            flexion = 4f * Mathf.Atan(ty) * Mathf.Rad2Deg;
            deviation = 4f * Mathf.Atan(tz) * Mathf.Rad2Deg;
            Clamp(q, out var clamped);
            return Quaternion.Angle(q, clamped);
        }

        // ---- The search -------------------------------------------------------------------------

        private static Eval Search(Chain c, in ArmGoal g, Vector3 wrist, BodyClearance.Posed torso, float? around)
        {
            Vector3 toWrist = wrist - c.shoulderAt;
            float d = toWrist.magnitude;
            Vector3 n = d > 1e-5f ? toWrist / d : c.chest * Vector3.down;
            // Reach: eased near straight, and no closer than the elbow's full bend allows.
            float full = c.l1 + c.l2;
            float soft = SoftReach * full;
            float reach = d > soft ? soft + (full - soft) * (1f - Mathf.Exp(-(d - soft) / (full - soft))) : d;
            float bendLimitRad = ElbowBendLimit * Mathf.Deg2Rad;
            float shortest = Mathf.Sqrt(c.l1 * c.l1 + c.l2 * c.l2 + 2f * c.l1 * c.l2 * Mathf.Cos(bendLimitRad));
            reach = Mathf.Max(reach, shortest + 1e-4f);
            reach = Mathf.Min(reach, full - 1e-4f);
            float cosAlpha = Mathf.Clamp((c.l1 * c.l1 + reach * reach - c.l2 * c.l2) / (2f * c.l1 * reach), -1f, 1f);
            float alpha = Mathf.Acos(cosAlpha);

            // The null's way on the circle (falling back to the relaxed reference where the arm points
            // along the null).
            Vector3 reference = Transported(c, n);
            Vector3 nullWay = NullWay(c.anim, c.right, g.elbowNull);
            Vector3 onPlane = nullWay - n * Vector3.Dot(nullWay, n);
            float w = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.15f, 0.4f, onPlane.magnitude));
            float toNull = onPlane.sqrMagnitude > 1e-10f ? Vector3.SignedAngle(reference, onPlane, n) : 0f;
            Vector3 wanted = Quaternion.AngleAxis(toNull * w, n) * reference;

            var frame = new Frame { n = n, wanted = wanted, alpha = alpha, reach = reach, wrist = c.shoulderAt + n * reach, nullWay = nullWay };
            Eval best = default;
            best.cost = float.MaxValue;
            float step = 360f / Samples;
            // (The null's own place first: if the arm fits there it is the best there is, every other
            // place costing its distance from the null. Most goals end here.)
            best = Evaluate(c, g, frame, around ?? 0f, torso);
            if (best.cost <= SwivelCost * Mathf.DeltaAngle(0f, around ?? 0f) * Mathf.DeltaAngle(0f, around ?? 0f) + 1e-6f && (!around.HasValue || Mathf.Abs(around.Value) < 1e-3f))
            {
                return best;
            }

            if (!around.HasValue)
            {
                for (int i = 0; i < Samples; i++)
                {
                    var e = Evaluate(c, g, frame, -180f + step * i, torso);
                    if (e.cost < best.cost)
                    {
                        best = e;
                    }
                }
            }

            // Refined about the best (the cost is smooth there).
            for (int round = 0; round < 3; round++)
            {
                step *= 0.25f;
                float centre = best.swivel;
                for (int k = -3; k <= 3; k++)
                {
                    if (k == 0)
                    {
                        continue;
                    }

                    var e = Evaluate(c, g, frame, centre + k * step, torso);
                    if (e.cost < best.cost)
                    {
                        best = e;
                    }
                }
            }

            return best;
        }

        private struct Frame
        {
            public Vector3 n, wanted, wrist, nullWay;
            public float alpha, reach;
        }

        private struct Eval
        {
            public float cost, swivel, roll, excess, shoulderPast, clearanceShort, bend;
            public Quaternion upper, foreLocal, handLocal, hand;
            public Vector3 elbow, centre, nullWay;
            public float radius;

            public ArmSolve Report(Chain c, in ArmGoal g)
            {
                var s = new ArmSolve
                {
                    turnMiss = Quaternion.Angle(hand, g.handTurn), wristExcess = excess, swivelOff = Mathf.Abs(swivel), roll = roll,
                    elbowBend = bend, shoulderPast = shoulderPast, clearanceShort = clearanceShort,
                    shoulder = c.shoulderAt, elbow = elbow, wrist = c.hand.position, circleCentre = centre, circleRadius = radius, nullWay = nullWay,
                };
                WristAngles(handLocal, c.wristFrame, out s.twist, out s.flexion, out s.deviation);
                return s;
            }
        }

        private static Eval Evaluate(Chain c, in ArmGoal g, in Frame f, float swivel, BodyClearance.Posed torso)
        {
            var e = new Eval { swivel = swivel, nullWay = f.nullWay };
            Vector3 way = Quaternion.AngleAxis(swivel, f.n) * f.wanted;
            e.centre = c.shoulderAt + f.n * (c.l1 * Mathf.Cos(f.alpha));
            e.radius = c.l1 * Mathf.Sin(f.alpha);
            e.elbow = e.centre + way * e.radius;
            Vector3 upperWay = (e.elbow - c.shoulderAt) / c.l1;
            // The hinge: the arm's plane holds the shoulder-to-wrist line and the elbow's way.
            Vector3 hinge = Vector3.Cross(way, f.n).normalized;
            e.upper = Basis(upperWay, hinge) * c.restBasisInverse;
            Vector3 foreNow = e.upper * c.foreAlong;
            e.bend = Mathf.Clamp(Vector3.SignedAngle(foreNow, f.wrist - e.elbow, hinge), 0f, ElbowBendLimit);
            e.foreLocal = Quaternion.AngleAxis(e.bend, c.elbowAxis);
            Quaternion fore = e.upper * e.foreLocal;

            // The shoulder: the upper arm points within its reach of the relaxed forward reach (up,
            // out to the side, down and across the chest, not behind the back). Its roll is the elbow
            // null's to set.
            e.shoulderPast = Mathf.Max(0f, Vector3.Angle(upperWay, c.neutral) - ShoulderReach);

            // The hand: rolled about its allowed line as little as the wrist needs.
            float bestWrist = float.MaxValue;
            bool canRoll = g.rollAllowance > 0.01f && g.rollAxisInHand.sqrMagnitude > 1e-8f;
            int rolls = canRoll ? 7 : 1;
            for (int r = 0; r < rolls; r++)
            {
                // (Unrolled first: a wrist that takes the turn as asked needs no roll.)
                int k = r == 0 ? 0 : (r + 1) / 2 * (r % 2 == 1 ? 1 : -1);
                float roll = canRoll ? g.rollAllowance * k / 3f : 0f;
                if (r > 0 && e.excess <= 1e-3f)
                {
                    break;
                }

                Quaternion wanted = canRoll ? g.handTurn * Quaternion.AngleAxis(roll, g.rollAxisInHand) : g.handTurn;
                Quaternion local = Quaternion.Inverse(fore * c.handRest) * wanted;
                var q = Quaternion.Inverse(c.wristFrame) * local * c.wristFrame;
                float excess = Clamp(q, out var clamped);
                float cost = WristCost * excess * excess + RollCost * roll * roll;
                if (cost < bestWrist)
                {
                    bestWrist = cost;
                    e.roll = roll;
                    e.excess = excess;
                    e.handLocal = c.wristFrame * clamped * Quaternion.Inverse(c.wristFrame);
                }
            }

            e.hand = fore * c.handRest * e.handLocal;
            if (torso != null && g.clearance > 0f)
            {
                Vector3 wristAt = e.elbow + fore * c.handRestPosition;
                Vector3 grip = wristAt + e.hand * g.pointInHand;
                e.clearanceShort = Mathf.Max(0f, g.clearance - torso.Arm(c.shoulderAt, e.elbow, wristAt, grip));
            }

            float off = Mathf.DeltaAngle(0f, swivel);
            e.cost = SwivelCost * off * off + bestWrist + ShoulderCost * e.shoulderPast * e.shoulderPast
                     + ClearanceCost * (100f * e.clearanceShort) * (100f * e.clearanceShort);
            return e;
        }

        /// <summary>The relaxed elbow's way for an arm pointing along n: carried from the neutral reach, where it hangs down.</summary>
        private static Vector3 Transported(Chain c, Vector3 n)
        {
            Vector3 n0 = c.neutral;
            Vector3 down = c.chest * Vector3.down;
            Vector3 u0 = (down - n0 * Vector3.Dot(down, n0)).normalized;
            Vector3 u = Quaternion.FromToRotation(n0, n) * u0;
            return (u - n * Vector3.Dot(u, n)).normalized;
        }

        private static Vector3 Side(Vector3 v, bool right) => right ? v : new Vector3(-v.x, v.y, v.z);

        /// <summary>A rotation whose x axis is <paramref name="x"/> and y axis <paramref name="y"/> (made square to x).</summary>
        private static Quaternion Basis(Vector3 x, Vector3 y)
        {
            Vector3 z = Vector3.Cross(x, y);
            return Quaternion.LookRotation(z, Vector3.Cross(z, x));
        }

        /// <summary>A rotation split as PhysX splits it: the twist about x (degrees) and the swing's tangents of a quarter angle about y and z.</summary>
        private static void Split(Quaternion q, out float twist, out float ty, out float tz)
        {
            if (q.w < 0f)
            {
                q = new Quaternion(-q.x, -q.y, -q.z, -q.w);
            }

            twist = 2f * Mathf.Atan2(q.x, q.w) * Mathf.Rad2Deg;
            var swing = q * Quaternion.Inverse(Quaternion.AngleAxis(twist, Vector3.right));
            if (swing.w < 0f)
            {
                swing = new Quaternion(-swing.x, -swing.y, -swing.z, -swing.w);
            }

            ty = swing.y / (1f + swing.w);
            tz = swing.z / (1f + swing.w);
        }

        /// <summary>The wrist's turn (in its frame) clamped to its limits; returns how far it was past them (degrees).</summary>
        private static float Clamp(Quaternion q, out Quaternion clamped)
        {
            Split(q, out float twist, out float ty, out float tz);
            twist = Mathf.Clamp(twist, -TwistLimit, TwistLimit);
            float ay = Mathf.Tan(0.25f * FlexLimit * Mathf.Deg2Rad), az = Mathf.Tan(0.25f * DeviationLimit * Mathf.Deg2Rad);
            float e = (ty / ay) * (ty / ay) + (tz / az) * (tz / az);
            if (e > 1f)
            {
                float k = 1f / Mathf.Sqrt(e);
                ty *= k;
                tz *= k;
            }

            tz = Mathf.Min(tz, Mathf.Tan(0.25f * RadialLimit * Mathf.Deg2Rad));
            float t2 = ty * ty + tz * tz;
            var swing = new Quaternion(0f, 2f * ty / (1f + t2), 2f * tz / (1f + t2), (1f - t2) / (1f + t2));
            clamped = swing * Quaternion.AngleAxis(twist, Vector3.right);
            return Quaternion.Angle(q, clamped);
        }

        // ---- The arm's parts --------------------------------------------------------------------

        private struct Chain
        {
            public AnimRig anim;
            public bool right;
            public Transform upper, fore, hand;
            public Quaternion chest, upperRest, handRest, wristFrame, restBasisInverse;
            public Vector3 shoulderAt, upperAlong, foreAlong, elbowAxis, handRestPosition, neutral;
            public float l1, l2;

            public static Chain Of(AnimRig anim, bool right)
            {
                var c = new Chain { anim = anim, right = right };
                PartId u = right ? PartId.UpperArmR : PartId.UpperArmL, f = right ? PartId.ForearmR : PartId.ForearmL, h = right ? PartId.HandR : PartId.HandL;
                c.upper = anim.Part(u);
                c.fore = anim.Part(f);
                c.hand = anim.Part(h);
                c.chest = anim.Part(PartId.Chest).rotation;
                c.upperRest = anim.RestLocalRotation(u);
                c.handRest = anim.RestLocalRotation(h);
                c.shoulderAt = c.upper.position;
                // (Lengths as the rig holds them, scaled with the unit.)
                Vector3 elbowLocal = anim.RestLocalPosition(f), wristLocal = anim.RestLocalPosition(h);
                c.l1 = Vector3.Scale(elbowLocal, c.upper.lossyScale).magnitude;
                c.l2 = Vector3.Scale(wristLocal, c.fore.lossyScale).magnitude;
                c.handRestPosition = Vector3.Scale(wristLocal, c.fore.lossyScale);
                c.upperAlong = elbowLocal.normalized;
                c.foreAlong = wristLocal.normalized;
                c.elbowAxis = ElbowAxis(anim, right);
                c.restBasisInverse = Quaternion.Inverse(Basis(c.upperAlong, c.elbowAxis));
                c.wristFrame = WristFrame(anim, right);
                c.neutral = c.chest * Side(NeutralReach, right).normalized;
                return c;
            }

            public void Apply(in Eval e)
            {
                upper.rotation = e.upper;
                fore.localRotation = anim.RestLocalRotation(right ? PartId.ForearmR : PartId.ForearmL) * e.foreLocal;
                hand.localRotation = handRest * e.handLocal;
            }
        }
    }
}
