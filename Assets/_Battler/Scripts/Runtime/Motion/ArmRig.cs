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
        /// The elbow null, as an angle round the arm (degrees): where on its circle round the
        /// shoulder-to-hand line the elbow sits. 0 the elbow hanging as a relaxed arm's does, +90 out to
        /// the arm's side, 180 up and over, -90 in across the body; the same for either arm. Measured
        /// from a reference carried smoothly with the arm's direction, so it never flips where an arm
        /// can point (a pole or null given as a point flips as the arm passes it).
        /// </summary>
        public float elbowSwivel;

        /// <summary>How far the arm keeps from its own torso (metres; 0: not checked).</summary>
        public float clearance;

        /// <summary>
        /// How much the elbow moves off its null so the wrist can take the hand's turn. 1: a degree of
        /// the turn the wrist can't take is worth 20 degrees of elbow (a shield's face, a hand placed
        /// just so). 0: the elbow keeps to its null and the hand turns as far as the wrist allows (a
        /// weapon: the arm swings it, the weapon points where the arm and wrist put it).
        /// </summary>
        public float turnWeight;

        /// <summary>
        /// How much of the wrist's bend (flexion and deviation; not the forearm's twist) the hand may
        /// use, as a share of its limits (0 or 1: all of it). A relaxed guard holds the wrist near
        /// straight and lets the weapon's angle give instead.
        /// </summary>
        public float wristEase;

        /// <summary>
        /// The elbow moved off its null only as far as the nearest best place (not searched round the
        /// whole circle): a held pose stays the same way of holding it as its goal moves, never
        /// jumping to another (a guard).
        /// </summary>
        public bool nearNull;

        /// <summary>
        /// A keyed wrist (a blow's): the hand's turn on the forearm (its local rotation, as
        /// <see cref="ArmRig.Wrist"/> makes it). The hand is then held so and the arm reaches the point
        /// with it, the elbow at its null; <see cref="handTurn"/> is not used.
        /// </summary>
        public bool wristGiven;

        public Quaternion wristLocal;

        public static ArmGoal Hand(Vector3 point, Vector3 pointInHand, Quaternion handTurn, float elbowSwivel) => new ArmGoal
        {
            point = point, pointInHand = pointInHand, handTurn = handTurn, elbowSwivel = elbowSwivel, turnWeight = 1f,
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

        /// <summary>How far the elbow is round its circle from the swivel asked (degrees).</summary>
        public float swivelOff;

        /// <summary>The hand's roll about its allowed line (degrees).</summary>
        public float roll;

        /// <summary>The forearm's twist (palm over), the wrist's flexion and its deviation toward the thumb (+) or little finger (-) (degrees).</summary>
        public float twist, flexion, deviation;

        /// <summary>The hand's turn on the forearm as solved (its local rotation): a guard's wrist, to blend a blow's keys from.</summary>
        public Quaternion wristLocal;

        /// <summary>The wrist the goal asked for, before its limits (degrees; for measuring a move's demands).</summary>
        public float askedTwist, askedFlexion, askedDeviation;

        /// <summary>The elbow's bend (degrees, 0 straight).</summary>
        public float elbowBend;

        /// <summary>How far the shoulder is past its limits (degrees).</summary>
        public float shoulderPast;

        /// <summary>How close the arm comes to its torso, past the clearance asked (metres; 0: clear).</summary>
        public float clearanceShort;

        /// <summary>For the gizmo: the shoulder, the elbow, the wrist, the elbow circle's centre and radius, and the way the swivel asked for (world).</summary>
        public Vector3 shoulder, elbow, wrist, circleCentre, nullWay;

        /// <summary>The elbow's swivel as solved (degrees, as <see cref="ArmGoal.elbowSwivel"/>).</summary>
        public float swivel;

        public float circleRadius;
    }

    /// <summary>
    /// Milestone 10 rebuild: the arm rig. One exact solve, with no memory, from the arm's controls
    /// (<see cref="ArmGoal"/>) to its joints, used by every arm (weapon, shield, bare hand, getting up).
    ///
    /// 1. Reach sets the elbow's bend (law of cosines), eased near a straight arm.
    /// 2. The elbow sits on a circle round the shoulder-to-wrist line; the elbow null (an angle round
    ///    the arm, from a reference carried smoothly with it) picks where.
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
        private const float SwivelCost = 0.01f, WristCost = 4f, ShoulderCost = 40f, RollCost = 0.04f, ClearanceCost = 10f;

        /// <summary>How far round its circle the reach moves the elbow off its way for the shoulder or the torso (degrees each way).</summary>
        public const float ReachSwing = 60f;
        private const int Samples = 24;

        // Where an arm reaches when relaxed (chest frame, x out): the shoulder's reach is measured from here.
        private static readonly Vector3 NeutralReach = new Vector3(0.3f, -0.5f, 0.8f);

        // Where the swivel's own reference is carried from (parallel transport; chest frame, x out): it
        // turns smoothly everywhere but straight opposite, which this puts low behind the far hip,
        // where no arm goes. (From the neutral reach, the opposite was up and back over the head: an
        // overhead wind-up's way, where the elbow flipped.)
        private static readonly Vector3 SwivelPole = new Vector3(0.3f, 0.5f, 0.8f);

        /// <param name="apply">False: only measured (the arm is left as it was).</param>
        public static ArmSolve Solve(AnimRig anim, bool right, in ArmGoal goal, BodyClearance.Posed torso = null, bool apply = true)
        {
            var chain = Chain.Of(anim, right);
            var g = goal;
            if (g.wristGiven)
            {
                // A keyed wrist: the hand held so, the arm reaching the point with it, the elbow at its null.
                var q = Quaternion.Inverse(chain.wristFrame) * g.wristLocal * chain.wristFrame;
                float past = Clamp(q, out var inside);
                Quaternion held = chain.wristFrame * inside * Quaternion.Inverse(chain.wristFrame);
                Vector3 toPoint = g.point - chain.shoulderAt;
                Vector3 axis = toPoint.sqrMagnitude > 1e-10f ? toPoint.normalized : chain.chest * Vector3.down;
                var keyed = Reach(chain, g, held, g.elbowSwivel, 90f, 0f, torso);
                keyed.excess = past;
                g.handTurn = keyed.hand;
                return Finish(chain, g, keyed, apply);
            }

            if (g.handTurn.w == 0f && g.handTurn.x == 0f && g.handTurn.y == 0f && g.handTurn.z == 0f)
            {
                g.handTurn = chain.hand.rotation;
            }

            // A: the wrist. The arm reaches for the wrist as the hand's turn would put it, the elbow at
            // its null (or moved off it as far as the goal's turn weight asks), and the wrist takes as
            // much of the turn as its limits allow.
            Eval wrist = Search(chain, g, g.point - g.handTurn * g.pointInHand, torso, g.nearNull ? 0f : (float?)null);
            // B: the reach. With the hand held on the forearm as A left it, the arm is a two-bone chain
            // from the shoulder to the hand's point: solved exactly for the point, the elbow as near
            // A's as the shoulder and the torso allow. C: the wrist taken again from that forearm, and
            // the reach again: the point exact, the wrist inside its limits, the turn as near as they let it.
            Eval best = wrist;
            if (g.pointInHand.sqrMagnitude > 1e-10f || wrist.excess > 1e-3f)
            {
                float swivelA = SwivelOf(chain, wrist.elbow, wrist.elbow + wrist.upper * wrist.foreLocal * chain.handRestPosition);
                best = Reach(chain, g, wrist.handLocal, swivelA, wrist.bend, wrist.roll, torso);
                Quaternion wanted = wrist.roll != 0f ? g.handTurn * Quaternion.AngleAxis(wrist.roll, g.rollAxisInHand) : g.handTurn;
                var q = Quaternion.Inverse(chain.wristFrame) * Quaternion.Inverse(best.upper * best.foreLocal * chain.handRest) * wanted * chain.wristFrame;
                float excess = Clamp(q, out var clamped, g.wristEase);
                best = Reach(chain, g, chain.wristFrame * clamped * Quaternion.Inverse(chain.wristFrame), swivelA, best.bend, wrist.roll, torso);
                best.excess = excess;
            }

            return Finish(chain, g, best, apply);
        }

        private static ArmSolve Finish(Chain chain, in ArmGoal g, in Eval best, bool apply)
        {
            Quaternion before0 = chain.upper.rotation, before1 = chain.fore.localRotation, before2 = chain.hand.localRotation;
            chain.Apply(best);
            var solve = best.Report(chain, g);
            solve.wristLocal = best.handLocal;
            {
                Quaternion asked = Quaternion.Inverse(best.upper * best.foreLocal * chain.handRest) * g.handTurn;
                Split(Quaternion.Inverse(chain.wristFrame) * asked * chain.wristFrame, out solve.askedTwist, out float ay, out float az);
                solve.askedFlexion = 4f * Mathf.Atan(ay) * Mathf.Rad2Deg;
                solve.askedDeviation = 4f * Mathf.Atan(az) * Mathf.Rad2Deg;
            }

            solve.pointMiss = Vector3.Distance(chain.hand.position + chain.hand.rotation * g.pointInHand, g.point);
            solve.swivel = SwivelOf(chain, solve.elbow, chain.hand.position);
            solve.swivelOff = Mathf.Abs(Mathf.DeltaAngle(solve.swivel, g.elbowSwivel));
            if (!apply)
            {
                chain.upper.rotation = before0;
                chain.fore.localRotation = before1;
                chain.hand.localRotation = before2;
            }

            return solve;
        }

        /// <summary>
        /// How far from the shoulder the arm can put a point of the hand (metres), the hand held on the
        /// forearm so (<paramref name="wristLocal"/>): the longest reach, the elbow's bend within its range.
        /// </summary>
        public static float MaxReach(AnimRig anim, bool right, Quaternion wristLocal, Vector3 pointInHand)
        {
            var c = Chain.Of(anim, right);
            var q = Quaternion.Inverse(c.wristFrame) * wristLocal * c.wristFrame;
            Clamp(q, out var inside);
            Vector3 v = c.handRestPosition + c.handRest * (c.wristFrame * inside * Quaternion.Inverse(c.wristFrame)) * pointInHand;
            ReachRange(c, v, out float full, out _);
            return full;
        }

        /// <summary>The longest and shortest reach of the chain shoulder-elbow-point, v the elbow-to-point line in the forearm's frame.</summary>
        private static void ReachRange(Chain c, Vector3 v, out float full, out float shortest)
        {
            Vector3 h = c.elbowAxis;
            Vector3 vp = v - h * Vector3.Dot(v, h);
            float a = Vector3.Dot(c.upperAlong, vp), b = Vector3.Dot(c.upperAlong, Vector3.Cross(h, vp));
            float r = Mathf.Sqrt(a * a + b * b), phase = Mathf.Atan2(b, a) * Mathf.Rad2Deg;
            float F(float bend) => a * Mathf.Cos(bend * Mathf.Deg2Rad) + b * Mathf.Sin(bend * Mathf.Deg2Rad);
            float most = Mathf.Max(F(0f), F(ElbowBendLimit)), least = Mathf.Min(F(0f), F(ElbowBendLimit));
            float at = Mathf.DeltaAngle(0f, phase), opposite = Mathf.DeltaAngle(0f, phase + 180f);
            if (at >= 0f && at <= ElbowBendLimit)
            {
                most = r;
            }

            if (opposite >= 0f && opposite <= ElbowBendLimit)
            {
                least = -r;
            }

            float lv2 = v.sqrMagnitude, l1 = c.l1;
            full = Mathf.Sqrt(Mathf.Max(1e-8f, l1 * l1 + lv2 + 2f * l1 * most));
            shortest = Mathf.Sqrt(Mathf.Max(0f, l1 * l1 + lv2 + 2f * l1 * least));
        }

        /// <summary>
        /// A wrist as a blow keys it: the forearm's turn (pronation +, the palm turned down from thumb
        /// forward), the wrist's flexion (toward the palm +) and its deviation (toward the thumb +), in
        /// degrees, the same for either hand. Returns the hand's local rotation on the forearm.
        /// </summary>
        public static Quaternion Wrist(AnimRig anim, bool right, Vector3 key)
        {
            float twist = right ? key.x : -key.x;
            float flexion = right ? -key.y : key.y;
            float ty = Mathf.Tan(0.25f * flexion * Mathf.Deg2Rad), tz = Mathf.Tan(0.25f * key.z * Mathf.Deg2Rad), t2 = ty * ty + tz * tz;
            var swing = new Quaternion(0f, 2f * ty / (1f + t2), 2f * tz / (1f + t2), (1f - t2) / (1f + t2));
            var frame = WristFrame(anim, right);
            return frame * (swing * Quaternion.AngleAxis(twist, Vector3.right)) * Quaternion.Inverse(frame);
        }

        /// <summary>A hand's local rotation on the forearm as a blow keys it (the inverse of <see cref="Wrist"/>).</summary>
        public static Vector3 WristKey(AnimRig anim, bool right, Quaternion local)
        {
            WristAngles(local, WristFrame(anim, right), out float twist, out float flexion, out float deviation);
            return new Vector3(right ? twist : -twist, right ? -flexion : flexion, deviation);
        }

        /// <summary>
        /// The reach (B): the hand held on the forearm (<paramref name="handLocal"/>), the arm a two-bone
        /// chain from the shoulder to the hand's point, the second "bone" the elbow-to-point line, which
        /// the elbow's hinge turns. The bend that puts the point at its distance from the shoulder is
        /// solved in closed form; the chain is then turned onto the point, the elbow as near
        /// <paramref name="way"/> as the shoulder's reach and the torso allow.
        /// </summary>
        private static Eval Reach(Chain c, in ArmGoal g, Quaternion handLocal, float swivel, float bendNear, float roll, BodyClearance.Posed torso)
        {
            Vector3 v = c.handRestPosition + c.handRest * handLocal * g.pointInHand;
            Vector3 toPoint = g.point - c.shoulderAt;
            float d = toPoint.magnitude;
            Vector3 n = d > 1e-5f ? toPoint / d : c.chest * Vector3.down;
            Vector3 h = c.elbowAxis;
            Vector3 vp = v - h * Vector3.Dot(v, h);
            float a = Vector3.Dot(c.upperAlong, vp), b = Vector3.Dot(c.upperAlong, Vector3.Cross(h, vp));
            float r = Mathf.Sqrt(a * a + b * b), phase = Mathf.Atan2(b, a) * Mathf.Rad2Deg;
            float lv2 = v.sqrMagnitude, l1 = c.l1;
            float F(float bend) => a * Mathf.Cos(bend * Mathf.Deg2Rad) + b * Mathf.Sin(bend * Mathf.Deg2Rad);
            // Reach: eased near the longest, and no shorter than the shortest.
            ReachRange(c, v, out float full, out float shortest);
            float soft = SoftReach * full;
            float reach = d > soft ? soft + (full - soft) * (1f - Mathf.Exp(-(d - soft) / Mathf.Max(1e-5f, full - soft))) : d;
            reach = Mathf.Clamp(reach, shortest + 1e-4f, full - 1e-5f);
            float want = (reach * reach - l1 * l1 - lv2) / (2f * l1);
            float bendAt = bendNear;
            if (r > 1e-6f)
            {
                // (Two bends can put the point at its distance; always the more bent one when it is in
                // range, so the arm never jumps from one to the other. With the hand straight on the
                // forearm it is the plain two-bone bend.)
                float off = Mathf.Acos(Mathf.Clamp(want / r, -1f, 1f)) * Mathf.Rad2Deg;
                bool inRange = false;
                foreach (float candidate in new[] { phase + off, phase - off })
                {
                    float bend = Mathf.DeltaAngle(0f, candidate);
                    if (!inRange && bend >= -0.01f && bend <= ElbowBendLimit + 0.01f)
                    {
                        inRange = true;
                        bendAt = Mathf.Clamp(bend, 0f, ElbowBendLimit);
                    }
                }

                if (!inRange)
                {
                    bendAt = Mathf.Abs(F(0f) - want) <= Mathf.Abs(F(ElbowBendLimit) - want) ? 0f : ElbowBendLimit;
                }
            }

            Quaternion foreLocal = Quaternion.AngleAxis(bendAt, h);
            Vector3 u = c.upperAlong * l1 + foreLocal * v;
            Quaternion onto = Quaternion.FromToRotation(u, n);
            var frame = new ReachFrame { n = n, onto = onto, toWay = 0f, foreLocal = foreLocal, handLocal = handLocal, bend = bendAt, roll = roll };

            // The chain turns about the shoulder-to-point line; the elbow's swivel is the arm's own,
            // round the shoulder-to-wrist line (as a rig's pole is: the point may be a weapon's head
            // anywhere round the body, the wrist never behind the back). Found where it is the one asked.
            float Off(float about)
            {
                Quaternion upper = Quaternion.AngleAxis(about, n) * onto;
                Vector3 elbow = c.shoulderAt + upper * (c.upperAlong * l1);
                Vector3 wristAt = elbow + upper * foreLocal * c.handRestPosition;
                return Mathf.DeltaAngle(swivel, SwivelOf(c, elbow, wristAt));
            }

            float atBest = 0f, offBest = float.MaxValue, before = Off(-180f), beforeAt = -180f;
            for (int i = 1; i <= Samples; i++)
            {
                float at = -180f + 360f * i / Samples, now = Off(at);
                // (A crossing of the asked swivel, not the wrap at the back of the circle.)
                if (Mathf.Sign(before) != Mathf.Sign(now) && Mathf.Abs(before - now) < 180f)
                {
                    float lo = beforeAt, hi = at, flo = before;
                    for (int k = 0; k < 14; k++)
                    {
                        float mid = 0.5f * (lo + hi), fmid = Off(mid);
                        if (Mathf.Sign(fmid) == Mathf.Sign(flo)) { lo = mid; flo = fmid; } else { hi = mid; }
                    }

                    float root = 0.5f * (lo + hi);
                    float left = Mathf.Abs(Off(root));
                    if (left < offBest)
                    {
                        offBest = left;
                        atBest = root;
                    }
                }
                else if (Mathf.Abs(now) < offBest)
                {
                    offBest = Mathf.Abs(now);
                    atBest = at;
                }

                before = now;
                beforeAt = at;
            }

            frame.toWay = atBest;
            // (The elbow kept at its way unless the shoulder (or, for an arm whose hand's turn matters,
            // the torso) says otherwise; then moved round from it downhill only, as far as needed and no
            // further than ReachSwing: a local descent from the keyed place, so the answer moves smoothly
            // with the goal and never jumps to another part of the circle.)
            Eval found = ReachAt(c, g, frame, 0f, torso);
            float step = 16f;
            while (found.cost > 1e-6f && step > 0.2f)
            {
                bool moved = false;
                foreach (float k in new[] { 1f, -1f })
                {
                    float at = found.swivel + k * step;
                    if (Mathf.Abs(at) > ReachSwing)
                    {
                        continue;
                    }

                    var e = ReachAt(c, g, frame, at, torso);
                    if (e.cost < found.cost - 1e-6f)
                    {
                        found = e;
                        moved = true;
                        break;
                    }
                }

                if (!moved)
                {
                    step *= 0.5f;
                }
            }

            Vector3 wristFound = found.elbow + found.upper * found.foreLocal * c.handRestPosition;
            found.nullWay = WantedWay(c, (wristFound - c.shoulderAt).normalized, swivel);
            return found;
        }

        private struct ReachFrame
        {
            public Vector3 n;
            public Quaternion onto, foreLocal, handLocal;
            public float toWay, bend, roll;
        }

        private static Eval ReachAt(Chain c, in ArmGoal g, in ReachFrame f, float swivel, BodyClearance.Posed torso)
        {
            var e = new Eval { swivel = swivel, bend = f.bend, roll = f.roll, foreLocal = f.foreLocal, handLocal = f.handLocal };
            e.upper = Quaternion.AngleAxis(f.toWay + swivel, f.n) * f.onto;
            e.elbow = c.shoulderAt + e.upper * (c.upperAlong * c.l1);
            e.centre = c.shoulderAt + f.n * Vector3.Dot(e.elbow - c.shoulderAt, f.n);
            e.radius = (e.elbow - e.centre).magnitude;
            Quaternion fore = e.upper * e.foreLocal;
            e.hand = fore * c.handRest * e.handLocal;
            e.shoulderPast = Mathf.Max(0f, Vector3.Angle(e.elbow - c.shoulderAt, c.neutral) - ShoulderReach);
            if (torso != null && g.clearance > 0f)
            {
                Vector3 wristAt = e.elbow + fore * c.handRestPosition;
                e.clearanceShort = Mathf.Max(0f, g.clearance - torso.Arm(c.shoulderAt, e.elbow, wristAt, wristAt + e.hand * g.pointInHand));
            }

            // (The torso's clearance moves the elbow only for an arm whose hand's turn matters; a weapon
            // arm's elbow keeps to its keys, and how close it comes is reported.)
            float off = Mathf.DeltaAngle(0f, swivel);
            e.cost = SwivelCost * off * off + ShoulderCost * e.shoulderPast * e.shoulderPast
                     + (g.turnWeight > 0f ? ClearanceCost * (100f * e.clearanceShort) * (100f * e.clearanceShort) : 0f);
            return e;
        }

        /// <summary>
        /// How far an arm's joint is past the rig's limits (degrees; 0 inside), for any rig built as the
        /// animation rig is (the pose or the physical body): the upper arm against the shoulder's
        /// reach, the elbow against its hinge (any turn off its axis, a bend past 145 or back past
        /// straight), the hand against the wrist's twist and swing. Other parts: 0.
        /// </summary>
        public static float PastLimits(AnimRig anim, PartId id, Quaternion parent, Quaternion child)
        {
            bool right = id == PartId.UpperArmR || id == PartId.ForearmR || id == PartId.HandR;
            switch (id)
            {
                case PartId.UpperArmR:
                case PartId.UpperArmL:
                {
                    Vector3 along = child * anim.RestLocalPosition(right ? PartId.ForearmR : PartId.ForearmL).normalized;
                    Vector3 neutral = parent * Side(NeutralReach, right).normalized;
                    return Mathf.Max(0f, Vector3.Angle(along, neutral) - ShoulderReach);
                }
                case PartId.ForearmR:
                case PartId.ForearmL:
                {
                    var local = Quaternion.Inverse(anim.RestLocalRotation(id)) * (Quaternion.Inverse(parent) * child);
                    if (local.w < 0f)
                    {
                        local = new Quaternion(-local.x, -local.y, -local.z, -local.w);
                    }

                    Vector3 h = ElbowAxis(anim, right);
                    float bend = 2f * Mathf.Atan2(Vector3.Dot(new Vector3(local.x, local.y, local.z), h), local.w) * Mathf.Rad2Deg;
                    float off = Quaternion.Angle(local, Quaternion.AngleAxis(bend, h));
                    return Mathf.Max(0f, Mathf.Max(off - HingeSlack, Mathf.Max(bend - ElbowBendLimit, -bend - HingeSlack)));
                }
                case PartId.HandR:
                case PartId.HandL:
                {
                    var local = Quaternion.Inverse(anim.RestLocalRotation(id)) * (Quaternion.Inverse(parent) * child);
                    return WristAngles(local, WristFrame(anim, right), out _, out _, out _);
                }
                default:
                    return 0f;
            }
        }

        /// <summary>How far an elbow may turn off its hinge, or back past straight, before it counts as past its limits (degrees).</summary>
        public const float HingeSlack = 2f;

        /// <summary>
        /// The elbow's swivel that points the elbow toward <paramref name="pole"/> (a way, world) with
        /// the hand at <paramref name="point"/>: for arms whose elbow is still aimed by a pole (the
        /// shield arm, the bare hand, the get-ups), the null a two-bone solve with that pole gives.
        /// </summary>
        public static float SwivelToward(AnimRig anim, bool right, Vector3 point, Vector3 pole)
        {
            var c = Chain.Of(anim, right);
            Vector3 n = point - c.shoulderAt;
            if (n.sqrMagnitude < 1e-10f)
            {
                return 0f;
            }

            n.Normalize();
            Vector3 way = pole.sqrMagnitude > 1e-10f ? pole.normalized : Vector3.zero;
            Vector3 off = way - n * Vector3.Dot(way, n);
            float sure = Mathf.Clamp01(off.magnitude / PoleSure);
            Vector3 elbowNow = anim.Part(right ? PartId.ForearmR : PartId.ForearmL).position;
            float now = SwivelOf(c, elbowNow, point);
            if (sure <= 0f)
            {
                return now;
            }

            // (A pole along the arm says nothing about the elbow: the nearer it is, the more the elbow
            // stays where it is, so it never flips round as a pole passes along the arm.)
            float fromPole = SwivelOf(c, c.shoulderAt + off.normalized, point);
            return sure >= 1f ? fromPole : now + Mathf.DeltaAngle(now, fromPole) * sure;
        }

        /// <summary>How far a pole must be off the arm's line (the sine of the angle) before it alone sets the elbow.</summary>
        public const float PoleSure = 0.5f;

        /// <summary>The elbow's swivel (degrees, as <see cref="ArmGoal.elbowSwivel"/>) for an elbow at a place, the hand's point at another.</summary>
        public static float SwivelOf(AnimRig anim, bool right, Vector3 elbow, Vector3 point) => SwivelOf(Chain.Of(anim, right), elbow, point);

        private static float SwivelOf(Chain c, Vector3 elbow, Vector3 point)
        {
            Vector3 toPoint = point - c.shoulderAt;
            Vector3 n = toPoint.sqrMagnitude > 1e-10f ? toPoint.normalized : c.chest * Vector3.down;
            Vector3 off = elbow - c.shoulderAt;
            off -= n * Vector3.Dot(off, n);
            return off.sqrMagnitude > 1e-10f ? Vector3.SignedAngle(Transported(c, n), off, n) * OutSign(c) : 0f;
        }

        /// <summary>Which way round the arm's line is out to its side (+1 or -1, so the swivel means the same for either arm).</summary>
        private static float OutSign(Chain c)
        {
            Vector3 u0 = Transported(c, c.neutral);
            Vector3 outWay = c.chest * (c.right ? Vector3.right : Vector3.left);
            return Vector3.Dot(Vector3.Cross(c.neutral, u0), outWay) >= 0f ? 1f : -1f;
        }

        /// <summary>The wrist's frame in the hand's (and forearm's) rest frame: x along the forearm, y toward the thumb (forward at rest), z = x × y.</summary>
        public static Quaternion WristFrame(AnimRig anim, bool right)
        {
            // (The physical wrist's joint frame: one definition for the pose and PhysX.)
            return PhysicsBattler.Muscles.JointFrames.Wrist(anim.RestLocalPosition(right ? PartId.HandR : PartId.HandL));
        }

        /// <summary>The elbow's hinge axis in the upper arm's rest frame (bending about it by a positive angle brings the forearm forward).</summary>
        public static Vector3 ElbowAxis(AnimRig anim, bool right)
        {
            return PhysicsBattler.Muscles.JointFrames.ElbowAxis(anim.RestLocalPosition(right ? PartId.ForearmR : PartId.ForearmL));
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

            Vector3 wanted = WantedWay(c, n, g.elbowSwivel);
            Vector3 nullWay = wanted;

            var frame = new Frame { n = n, wanted = wanted, alpha = alpha, reach = reach, wrist = c.shoulderAt + n * reach, nullWay = nullWay };
            Eval best = default;
            best.cost = float.MaxValue;
            float step = 360f / Samples;
            // (The null's own place first: if the arm fits there it is the best there is, every other
            // place costing its distance from the null. Most goals end here.)
            best = Evaluate(c, g, frame, around ?? 0f, torso);
            if (best.cost <= 1e-6f && !around.HasValue)
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

            // The hand: rolled about its allowed line as little as the wrist needs. (Sampled, then the
            // best found exactly between its neighbours, so the roll moves smoothly as the goal does
            // rather than stepping from sample to sample.)
            bool canRoll = g.rollAllowance > 0.01f && g.rollAxisInHand.sqrMagnitude > 1e-8f;
            var fg = g;
            float WristAt(float roll, out float excess, out Quaternion clamped)
            {
                Quaternion wanted = roll != 0f ? fg.handTurn * Quaternion.AngleAxis(roll, fg.rollAxisInHand) : fg.handTurn;
                Quaternion local = Quaternion.Inverse(fore * c.handRest) * wanted;
                var q = Quaternion.Inverse(c.wristFrame) * local * c.wristFrame;
                excess = Clamp(q, out clamped, fg.wristEase);
                return WristCost * excess * excess + RollCost * roll * roll;   // (the roll chosen on the wrist alone)
            }

            float bestWrist = WristAt(0f, out e.excess, out var bestClamped);
            e.roll = 0f;
            if (canRoll && e.excess > 1e-3f)
            {
                // (Unrolled first: a wrist that takes the turn as asked needs no roll.)
                float step = g.rollAllowance / 3f;
                var costs = new float[7];
                int at = 3;
                costs[3] = bestWrist;
                for (int k = -3; k <= 3; k++)
                {
                    if (k == 0)
                    {
                        continue;
                    }

                    costs[k + 3] = WristAt(step * k, out float ex, out var cl);
                    if (costs[k + 3] < bestWrist)
                    {
                        bestWrist = costs[k + 3];
                        at = k + 3;
                        e.roll = step * k;
                        e.excess = ex;
                        bestClamped = cl;
                    }
                }

                if (at > 0 && at < 6)
                {
                    float a = costs[at - 1], b = costs[at], d = costs[at + 1];
                    float curve = a - 2f * b + d;
                    if (curve > 1e-6f)
                    {
                        float roll = step * (at - 3 + 0.5f * (a - d) / curve);
                        float cost = WristAt(roll, out float ex, out var cl);
                        if (cost < bestWrist)
                        {
                            bestWrist = cost;
                            e.roll = roll;
                            e.excess = ex;
                            bestClamped = cl;
                        }
                    }
                }
            }

            e.handLocal = c.wristFrame * bestClamped * Quaternion.Inverse(c.wristFrame);
            e.hand = fore * c.handRest * e.handLocal;
            if (torso != null && g.clearance > 0f)
            {
                Vector3 wristAt = e.elbow + fore * c.handRestPosition;
                Vector3 grip = wristAt + e.hand * g.pointInHand;
                e.clearanceShort = Mathf.Max(0f, g.clearance - torso.Arm(c.shoulderAt, e.elbow, wristAt, grip));
            }

            float off = Mathf.DeltaAngle(0f, swivel);
            e.cost = SwivelCost * off * off + g.turnWeight * bestWrist + ShoulderCost * e.shoulderPast * e.shoulderPast
                     + (g.turnWeight > 0f ? ClearanceCost * (100f * e.clearanceShort) * (100f * e.clearanceShort) : 0f);
            return e;
        }

        /// <summary>The way the swivel asks for on the elbow's circle about n: the relaxed reference turned round by it.</summary>
        private static Vector3 WantedWay(Chain c, Vector3 n, float swivel)
        {
            return Quaternion.AngleAxis(swivel * OutSign(c), n) * Transported(c, n);
        }

        /// <summary>The relaxed elbow's way for an arm pointing along n: carried from the swivel's pole, where it hangs down.</summary>
        private static Vector3 Transported(Chain c, Vector3 n)
        {
            Vector3 n0 = c.pole;
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
        private static float Clamp(Quaternion q, out Quaternion clamped, float ease = 1f)
        {
            if (ease <= 0f || ease > 1f)
            {
                ease = 1f;
            }

            Split(q, out float twist, out float ty, out float tz);
            twist = Mathf.Clamp(twist, -TwistLimit, TwistLimit);
            float ay = Mathf.Tan(0.25f * ease * FlexLimit * Mathf.Deg2Rad), az = Mathf.Tan(0.25f * ease * DeviationLimit * Mathf.Deg2Rad);
            float e = (ty / ay) * (ty / ay) + (tz / az) * (tz / az);
            if (e > 1f)
            {
                float k = 1f / Mathf.Sqrt(e);
                ty *= k;
                tz *= k;
            }

            tz = Mathf.Min(tz, Mathf.Tan(0.25f * ease * RadialLimit * Mathf.Deg2Rad));
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
            public Vector3 shoulderAt, upperAlong, foreAlong, elbowAxis, handRestPosition, neutral, pole;
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
                c.pole = c.chest * Side(SwivelPole, right).normalized;
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
