using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// An arm read as a body's arm, from where its joints are (not from how its bones happen to be rolled): where the
    /// elbow is against the chest (how high the upper arm is raised, and toward where), how the upper arm is turned
    /// about its own length (which way the elbow folds), the elbow's bend, the forearm's roll (the thumb against the
    /// elbow's fold) and the wrist's bend. And how far each is past what a person's arm does (degrees; 0 inside).
    /// Both arms are read as a right arm (the left mirrored): out is away from the body's middle.
    /// </summary>
    public struct ArmReading
    {
        /// <summary>The upper arm against straight down (0 hanging, 90 level, 180 overhead) and toward where (0 forward, 90 out to the side, 180 back, negative across the body).</summary>
        public float Raised, Toward;

        /// <summary>The upper arm's turn about its length (positive: the forearm folds in toward the body's middle; negative: out); the elbow's bend (0 straight).</summary>
        public float Turn, Bend;

        /// <summary>The forearm's roll (positive: thumb turned in, palm down), the wrist bent toward the palm (negative: the back of the hand) and toward the thumb (negative: the little finger).</summary>
        public float Roll, Flex, Thumbward;

        /// <summary>How much the elbow's fold says (0 to 1): a straight arm has no fold to read, nor one straight overhead a way it is turned; the turn's and the roll's ranges open as it goes to 0.</summary>
        public float Folded;

        /// <summary>The upper arm's way in the chest's frame, as a right arm's (x out, y up, z forward).</summary>
        public Vector3 Along;

        /// <summary>How far past a person's range (degrees): the elbow's place, the upper arm's turn, the elbow's bend, the forearm's roll, the wrist.</summary>
        public float PastReach, PastTurn, PastBend, PastRoll, PastWrist;

        /// <summary>Whether it was read at all (a clip's arm to give room for: none, no room).</summary>
        public bool Known;

        public float Worst => Mathf.Max(Mathf.Max(PastReach, PastTurn), Mathf.Max(PastBend, Mathf.Max(PastRoll, PastWrist)));
    }

    /// <summary>
    /// The arms' limits, one model for every arm and every stage of the pose (the plan "Arms that bend as a body
    /// does", step 2). <see cref="Read"/> reads an arm as a body's arm; <see cref="ArmGate"/> is the gate at the end
    /// of the pose: whatever arm the stages left, each of its seven readings is brought into its range and the arm
    /// turned to them (the shoulder first, then the elbow's bend, its fold, the wrist, the forearm's roll), so the arm
    /// that leaves is one a body has. An arm already inside is not touched. The ranges are a person's, wide enough
    /// for all but a hundredth of the recorded clips' frames; and an arm may always be where the clip playing has
    /// it (the recorded motion is a person's: what it does, a body can do), with a little room about it.
    /// </summary>
    public static class ArmLimits
    {
        /// <summary>The gate on (the scene's Combat Feel, "Arms Held To A Body").</summary>
        public static bool On = true;

        /// <summary>After the gate, an arm's two bones are set as a body's are for where its joints are (the scene's Combat Feel, "Arm Bones As A Body").</summary>
        public static bool BonesAsABody = true;

        /// <summary>The upper arm's turn about its length: the forearm folded in toward the body's middle, and out (degrees from the fold a hanging arm has, carried the short way to where the upper arm points).</summary>
        public const float TurnIn = 100f, TurnOut = 95f;

        /// <summary>The elbow's bend (degrees; it straightens to 0 and no further, being read from the joints' places).</summary>
        public const float MostBend = 150f;

        /// <summary>The forearm's roll: the thumb turned in (palm down) and out (palm up), from thumb up the arm.</summary>
        public const float RollIn = 95f, RollOut = 100f;

        /// <summary>The wrist: bent to the palm or to the back of the hand, toward the thumb, toward the little finger.</summary>
        public const float MostFlex = 80f, MostThumbward = 50f, MostLittleFinger = 55f;

        /// <summary>The room about the playing clip's own arm (degrees, each reading).</summary>
        public const float ClipRoom = 12f;

        /// <summary>How much being near where the arm was let go the step before counts against being near where it is sent, in choosing the elbow's place (a share).</summary>
        public const float StayNear = 0.35f;

        /// <summary>Sent this near the far side of a range that goes round (degrees from the point as far from one end as the other), a turn stays at the end it was at.</summary>
        public const float StayAtEnd = 45f;

        /// <summary>Under this raised, the elbow is free to be anywhere (degrees from hanging).</summary>
        public const float FreeUnder = 30f;

        // (How high the upper arm goes, by where it points: all the way up in front and out to the side; behind, and
        // across the body, the trunk and the shoulder stop it.)
        private static readonly float[] TowardAt = { -180f, -150f, -120f, -90f, -60f, -40f, -20f, 0f, 100f, 120f, 140f, 160f, 180f };
        private static readonly float[] HighestAt = { 55f, 35f, 30f, 40f, 75f, 110f, 160f, 180f, 180f, 140f, 100f, 70f, 55f };

        /// <summary>How high the upper arm goes when it points <paramref name="toward"/> (degrees from hanging).</summary>
        public static float Highest(float toward)
        {
            toward = Mathf.Repeat(toward + 180f, 360f) - 180f;
            for (int i = 1; i < TowardAt.Length; i++)
            {
                if (toward <= TowardAt[i])
                {
                    return Mathf.Lerp(HighestAt[i - 1], HighestAt[i], Mathf.InverseLerp(TowardAt[i - 1], TowardAt[i], toward));
                }
            }

            return HighestAt[HighestAt.Length - 1];
        }

        internal static Vector3 Mirror(Vector3 v, bool left) => left ? new Vector3(-v.x, v.y, v.z) : v;

        private static Vector3 At(float raised, float toward)
        {
            float r = raised * Mathf.Deg2Rad, t = toward * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(r) * Mathf.Sin(t), -Mathf.Cos(r), Mathf.Sin(r) * Mathf.Cos(t));
        }

        internal static float Smooth(float from, float to, float x)
        {
            float t = Mathf.Clamp01((x - from) / (to - from));
            return t * t * (3f - 2f * t);
        }

        /// <summary>How much the fold says, from the bend and how high the arm is raised.</summary>
        internal static float FoldWeight(float bend, float raised) => Smooth(15f, 35f, bend) * (1f - Smooth(160f, 178f, bend)) * (1f - Smooth(150f, 170f, raised));

        /// <summary>
        /// The nearest way an upper arm can point to <paramref name="u"/> (a right arm's, in the chest's frame):
        /// <paramref name="u"/> itself where an elbow can be there, or within the clip's room of the clip's own.
        /// </summary>
        internal static Vector3 NearestAllowed(Vector3 u, in ArmReading clip) => NearestAllowed(u, clip, false, default);

        /// <summary>
        /// The same, choosing between places about as near the one nearer <paramref name="last"/> (where the arm was
        /// let go the step before): an elbow sent deep into where none can be is as near one edge as another, and
        /// went from edge to edge as it moved.
        /// </summary>
        internal static Vector3 NearestAllowed(Vector3 u, in ArmReading clip, bool hasLast, Vector3 last)
        {
            float raised = Mathf.Acos(Mathf.Clamp(-u.y, -1f, 1f)) * Mathf.Rad2Deg;
            if (raised <= FreeUnder)
            {
                return u;
            }

            float toward = Mathf.Atan2(u.x, u.z) * Mathf.Rad2Deg;
            if (raised <= Highest(toward))
            {
                return u;
            }

            if (clip.Known && Vector3.Angle(u, clip.Along) <= ClipRoom)
            {
                return u;
            }

            // (The nearest place on the edge of where an elbow can be: looked for round the whole of it, then closer
            // and closer about the nearest found. Straight up is one place, whichever way it is said to point.)
            float best = 0f, bestAngle = 360f;
            float from = -180f, to = 180f, step = 10f;
            for (int pass = 0; pass < 3; pass++)
            {
                for (float t = from; t <= to; t += step)
                {
                    Vector3 at = At(Mathf.Min(raised, Highest(t)), t);
                    float angle = Vector3.Angle(u, at) + (hasLast ? StayNear * Vector3.Angle(at, last) : 0f);
                    if (angle < bestAngle)
                    {
                        bestAngle = angle;
                        best = t;
                    }
                }

                from = best - step;
                to = best + step;
                step *= 0.1f;
            }

            Vector3 allowed = At(Mathf.Min(raised, Highest(best)), best);
            if (clip.Known)
            {
                // (Or the edge of the room about the clip's own, where that is nearer.)
                Vector3 nearClip = Vector3.RotateTowards(clip.Along, u, ClipRoom * Mathf.Deg2Rad, 0f);
                if (Vector3.Angle(u, nearClip) + (hasLast ? StayNear * Vector3.Angle(nearClip, last) : 0f) < bestAngle)
                {
                    allowed = nearClip;
                }
            }

            return allowed.normalized;
        }

        /// <summary>A range that also holds the clip's own reading, with its room.</summary>
        private static void Widen(ref float low, ref float high, bool known, float clip)
        {
            if (known)
            {
                low = Mathf.Min(low, clip - ClipRoom);
                high = Mathf.Max(high, clip + ClipRoom);
            }
        }

        /// <summary>
        /// The same for a reading that goes round (a turn, a roll, the wrist's bend to the palm): the end of the range on
        /// the clip's side goes out to it, by as much as the clip's reading says (<paramref name="says"/>, 0 to 1).
        /// Widened as a plain number, a clip's reading going past half a turn (179 to -179) threw the room from one end
        /// of the range to the other, and an arm held at the end went to and fro with it, step by step.
        /// </summary>
        private static void WidenRound(ref float low, ref float high, float says, float clip)
        {
            if (says <= 0f)
            {
                return;
            }

            float middle = 0.5f * (low + high);
            clip = Near(clip, middle);
            if (clip >= middle)
            {
                high = Mathf.Max(high, Mathf.Lerp(high, clip + ClipRoom, says));
            }
            else
            {
                low = Mathf.Min(low, Mathf.Lerp(low, clip - ClipRoom, says));
            }
        }

        /// <summary>An angle that goes round (a turn, a roll) brought into a range: itself inside, else the nearer end.</summary>
        internal static float Into(float angle, float low, float high) => Into(angle, low, high, false, 0f);

        /// <summary>The same; sent near the far side of the range, it stays at the end nearer <paramref name="last"/> (where it was let go the step before).</summary>
        internal static float Into(float angle, float low, float high, bool hasLast, float last)
        {
            if (high - low >= 360f)
            {
                return angle;
            }

            // (Said the way nearest the range's middle.)
            float middle = 0.5f * (low + high);
            angle = Near(angle, middle);
            if (angle >= low && angle <= high)
            {
                return angle;
            }

            if (hasLast && 180f - Mathf.Abs(angle - middle) < StayAtEnd)
            {
                return Near(last, middle) > middle ? high : low;
            }

            return angle > high ? high : low;
        }

        /// <summary>An angle that goes round, said the way nearest <paramref name="middle"/>.</summary>
        internal static float Near(float angle, float middle) => middle + Mathf.Repeat(angle - middle + 180f, 360f) - 180f;

        /// <summary>How far an angle that goes round is past a range (degrees).</summary>
        private static float PastRound(float angle, float low, float high)
        {
            if (high - low >= 360f)
            {
                return 0f;
            }

            float middle = 0.5f * (low + high);
            angle = middle + Mathf.Repeat(angle - middle + 180f, 360f) - 180f;
            return Mathf.Abs(Mathf.Clamp(angle, low, high) - angle);
        }

        internal static void TurnRange(float folded, in ArmReading clip, out float low, out float high)
        {
            low = -TurnOut;
            high = TurnIn;
            WidenRound(ref low, ref high, clip.Known ? clip.Folded : 0f, clip.Turn);
            low = Mathf.Lerp(Mathf.Min(low, -180f), low, folded);
            high = Mathf.Lerp(Mathf.Max(high, 180f), high, folded);
        }

        internal static void RollRange(float folded, in ArmReading clip, out float low, out float high)
        {
            low = -RollOut;
            high = RollIn;
            WidenRound(ref low, ref high, clip.Known ? clip.Folded : 0f, clip.Roll);
            low = Mathf.Lerp(Mathf.Min(low, -180f), low, folded);
            high = Mathf.Lerp(Mathf.Max(high, 180f), high, folded);
        }

        internal static void WristRanges(in ArmReading clip, out float flexLow, out float flexHigh, out float thumbLow, out float thumbHigh)
        {
            flexLow = -MostFlex;
            flexHigh = MostFlex;
            thumbLow = -MostLittleFinger;
            thumbHigh = MostThumbward;
            // (A clip's hand with the forearm near its thumb's line has no bend to the palm to read.)
            WidenRound(ref flexLow, ref flexHigh, clip.Known ? 1f - Smooth(60f, 80f, Mathf.Abs(clip.Thumbward)) : 0f, clip.Flex);
            Widen(ref thumbLow, ref thumbHigh, clip.Known, clip.Thumbward);
            // (Past a quarter turn to the thumb's side the hand's roll can't be read.)
            thumbLow = Mathf.Max(thumbLow, -85f);
            thumbHigh = Mathf.Min(thumbHigh, 85f);
        }

        /// <summary>The forearm's way in the hand's own axes for a wrist bent <paramref name="flex"/> to the palm and <paramref name="thumbward"/> to the thumb.</summary>
        internal static Vector3 WristAt(float flex, float thumbward, bool left)
        {
            float f = flex * Mathf.Deg2Rad, t = thumbward * Mathf.Deg2Rad;
            return Vector3.down * (Mathf.Cos(t) * Mathf.Cos(f)) - PalmSide(left) * (Mathf.Cos(t) * Mathf.Sin(f)) - Vector3.forward * Mathf.Sin(t);
        }

        /// <summary>
        /// The nearest way the forearm can lie in the hand to <paramref name="inHand"/> (the forearm's way in the hand's own
        /// axes): itself where a wrist bends so, else the nearest on the edge of where it does (between about as near, the
        /// one nearer <paramref name="last"/>). Looked for as a way, not reading by reading: with the forearm near the
        /// thumb's line the bend to the palm says nothing, and a hand held to it was rolled right over.
        /// </summary>
        internal static Vector3 NearestWrist(Vector3 inHand, bool left, in ArmReading clip, bool hasLast, Vector3 last)
        {
            Vector3 palm = PalmSide(left);
            float flex = -Mathf.Atan2(Vector3.Dot(inHand, palm), Vector3.Dot(inHand, Vector3.down)) * Mathf.Rad2Deg;
            float thumbward = -Mathf.Asin(Mathf.Clamp(Vector3.Dot(inHand, Vector3.forward), -1f, 1f)) * Mathf.Rad2Deg;
            WristRanges(clip, out float flexLow, out float flexHigh, out float thumbLow, out float thumbHigh);
            if (PastRound(flex, flexLow, flexHigh) <= 0f)
            {
                return thumbward >= thumbLow && thumbward <= thumbHigh ? inHand : WristAt(flex, Mathf.Clamp(thumbward, thumbLow, thumbHigh), left);
            }

            // (Past the bend to the palm or to the back of the hand: the nearest on either edge.)
            Vector3 best = inHand;
            float bestCost = float.MaxValue;
            for (int end = 0; end < 2; end++)
            {
                float at = end == 0 ? flexLow : flexHigh;
                Vector3 level = WristAt(at, 0f, left);
                float along = Vector3.Dot(inHand, level), toThumb = Vector3.Dot(inHand, Vector3.forward);
                float t = along <= 1e-6f ? (toThumb > 0f ? -90f : 90f) : -Mathf.Atan2(toThumb, along) * Mathf.Rad2Deg;
                Vector3 there = WristAt(at, Mathf.Clamp(t, thumbLow, thumbHigh), left);
                float cost = Vector3.Angle(inHand, there) + (hasLast ? StayNear * Vector3.Angle(there, last) : 0f);
                if (cost < bestCost)
                {
                    bestCost = cost;
                    best = there;
                }
            }

            return best;
        }

        internal static float MostBendWith(in ArmReading clip) => clip.Known ? Mathf.Max(MostBend, clip.Bend + ClipRoom) : MostBend;

        internal static Vector3 PalmSide(bool left) => left ? Vector3.right : Vector3.left;

        /// <summary>
        /// Reads an arm. The hand's own axes at rest: fingers down, thumb forward, palm toward the body.
        /// <paramref name="clip"/>: the playing clip's own arm (<see cref="ReadClip"/>), which the ranges make room for.
        /// </summary>
        public static ArmReading Read(Quaternion chest, Vector3 shoulder, Vector3 elbow, Vector3 wrist, Quaternion hand, bool left, in ArmReading clip = default)
        {
            var a = new ArmReading { Known = true };
            var back = Quaternion.Inverse(chest);
            Vector3 u = Mirror(back * (elbow - shoulder), left).normalized, f = Mirror(back * (wrist - elbow), left).normalized;
            a.Along = u;
            a.Raised = Mathf.Acos(Mathf.Clamp(-u.y, -1f, 1f)) * Mathf.Rad2Deg;
            a.Toward = Mathf.Atan2(u.x, u.z) * Mathf.Rad2Deg;
            a.PastReach = Vector3.Angle(u, NearestAllowed(u, clip));
            a.Bend = Vector3.Angle(u, f);
            a.PastBend = Mathf.Max(0f, a.Bend - MostBendWith(clip));
            a.Folded = FoldWeight(a.Bend, a.Raised);
            // (The fold a hanging arm has, forward, carried to where the upper arm points the short way: a forearm raised in
            // front folds up, one held out to the side folds forward.)
            var swing = Quaternion.FromToRotation(Vector3.down, u);
            Vector3 ahead = swing * Vector3.forward, inward = swing * Vector3.left;
            Vector3 fold = Vector3.ProjectOnPlane(f, u);
            if (a.Folded > 0f && fold.sqrMagnitude > 1e-10f)
            {
                a.Turn = Mathf.Atan2(Vector3.Dot(fold, inward), Vector3.Dot(fold, ahead)) * Mathf.Rad2Deg;
                TurnRange(a.Folded, clip, out float low, out float high);
                a.PastTurn = PastRound(a.Turn, low, high);
            }

            var handBack = Quaternion.Inverse(hand);
            Vector3 inHand = handBack * (wrist - elbow).normalized;
            a.Flex = -Mathf.Atan2(Vector3.Dot(inHand, PalmSide(left)), Vector3.Dot(inHand, Vector3.down)) * Mathf.Rad2Deg;
            a.Thumbward = -Mathf.Asin(Mathf.Clamp(Vector3.Dot(inHand, Vector3.forward), -1f, 1f)) * Mathf.Rad2Deg;
            a.PastWrist = Vector3.Angle(inHand, NearestWrist(inHand, left, clip, false, default));
            if (a.Folded > 0f)
            {
                // (Thumb up the arm, toward the shoulder's side of the fold, is the forearm unrolled.)
                Vector3 thumb = Mirror(back * (hand * Vector3.forward), left);
                Vector3 up = Vector3.ProjectOnPlane(-u, f), t = Vector3.ProjectOnPlane(thumb, f);
                if (up.sqrMagnitude > 1e-10f && t.sqrMagnitude > 1e-10f)
                {
                    up.Normalize();
                    Vector3 medial = Vector3.Cross(f, up);
                    a.Roll = Mathf.Atan2(Vector3.Dot(t, medial), Vector3.Dot(t, up)) * Mathf.Rad2Deg;
                    RollRange(a.Folded, clip, out float low, out float high);
                    a.PastRoll = PastRound(a.Roll, low, high);
                }
            }

            return a;
        }

        /// <summary>
        /// A clip's own arm, from the clip's points (indexed as <c>LocoBone</c>: the chest's frame from the neck over the
        /// upper spine and the shoulders' line, the hand's from its knuckles), read the same way with the ranges alone.
        /// </summary>
        public static ArmReading ReadClip(Vector3[] points, int neck, int spine, int leftShoulder, int rightShoulder, bool left)
        {
            int s = left ? leftShoulder : rightShoulder;
            if (points == null || points.Length <= Mathf.Max(rightShoulder + 5, Mathf.Max(neck, spine)))
            {
                return default;
            }

            Vector3 up = points[neck] - points[spine], right = points[rightShoulder] - points[leftShoulder];
            Vector3 forward = Vector3.Cross(right, up);
            Vector3 wrist = points[s + 2], along = points[s + 4] - wrist, side = Vector3.ProjectOnPlane(points[s + 3] - points[s + 5], along);
            if (forward.sqrMagnitude < 1e-10f || along.sqrMagnitude < 1e-10f || side.sqrMagnitude < 1e-10f)
            {
                return default;
            }

            return Read(Quaternion.LookRotation(forward, up), points[s], points[s + 1], wrist, Quaternion.LookRotation(side, -along), left);
        }
    }

    /// <summary>
    /// The gate at the end of the pose, for one arm (<see cref="ArmLimits"/>). It remembers where it let the arm go
    /// the step before, for two things a range alone can't give: an arm sent deep outside is as near one edge of the
    /// range as another, and stays at the one it was at instead of going from edge to edge; and when it does have to
    /// go from one to the other (the elbow's fold sent right round), it goes there through the range at an arm's
    /// pace (<see cref="CatchUp"/>), not in a step. An arm inside its range, and not coming back from such a move,
    /// is not touched.
    /// </summary>
    public sealed class ArmGate
    {
        /// <summary>How fast the arm goes over when where it is let go jumps (degrees a second, on top of however much the arm sent moved itself): the scene's Combat Feel, "Arm Goes Over At".</summary>
        public static float CatchUp = DefaultCatchUp;

        public const float DefaultCatchUp = 600f;

        /// <summary>
        /// The fastest the gate follows the arm it is sent, each of the elbow's place, its fold, the wrist and the
        /// forearm's roll (degrees a second, before the pace is added; 0: however fast it is sent): the scene's Combat
        /// Feel, "Arm Follows At Most". A sword arm in a strike turns about its shoulder at 900 or so.
        /// </summary>
        public static float MostSent = DefaultMostSent;

        public const float DefaultMostSent = 1500f;

        /// <summary>
        /// A gap in a range that goes round narrower than this (degrees: the fold's turn and the forearm's roll, as
        /// the arm straightens or goes overhead and their ranges open) is crossed the short way; a wider one is gone
        /// round, through the range.
        /// </summary>
        public const float NarrowGap = 70f;

        /// <summary>An arm bent less than this has no fold to hold (degrees): which way so straight an arm folds is read from millimetres of elbow.</summary>
        public const float NoFoldUnder = 8f;

        // (Where the arm was let go the step before, and how it came to each of the gate's steps then, as ways in the
        // chest's frame (a right arm's) or in the hand's, not as readings: a reading's zero swings round as the arm
        // nears straight up or straight out, and what was held to a reading swung with it.)
        private bool known, foldKnown, thumbKnown, wristKnown, boneWayKnown, handKnown;
        private Vector3 boneWay;
        private Quaternion handWas;
        private float outBend, inBend;
        private Vector3 outAlong, outFold, outThumb, outWrist;
        private Vector3 inAlong, inFold, inThumb, inWrist;
        private float inSays, twist;

        /// <summary>Forgets where the arm was (it was put somewhere else: a first step, a get-up).</summary>
        public void Forget()
        {
            known = false;
            boneWayKnown = false;
            handKnown = false;
        }

        private static float Toward(float from, float to, float most) => Mathf.Abs(to - from) <= most ? to : from + Mathf.Sign(to - from) * most;

        /// <summary>
        /// Where a turn that goes round is let go: <paramref name="into"/> (where the range has it), come to from
        /// <paramref name="last"/> (where it was let go the step before, read as it lies now) by no more than
        /// <paramref name="most"/>: through the range, or the short way across a gap that is narrow. Left outside its
        /// range, it comes in faster by <paramref name="pace"/>.
        /// </summary>
        private static float From(float last, float into, float low, float high, float most, float pace)
        {
            float gap = 360f - (high - low);
            if (gap < NarrowGap)
            {
                float by = Mathf.DeltaAngle(last, into);
                return Mathf.Abs(by) <= most ? into : last + Mathf.Sign(by) * most;
            }

            // (Left outside it by a range that closed as the arm moved (an arm coming down from overhead, bending from
            // straight): it comes in the short way, at the same pace, not in a step.)
            float middle = 0.5f * (low + high);
            last = ArmLimits.Near(last, middle);
            if (last < low || last > high)
            {
                // (To the range's nearer end first, and on through the range with what is left of the step: never the
                // short way to the far end, which is across what is outside.)
                float end = last < low ? low : high;
                float reach = most + pace;
                if (Mathf.Abs(end - last) >= reach)
                {
                    return last + Mathf.Sign(end - last) * reach;
                }

                most = Mathf.Max(0f, reach - Mathf.Abs(end - last));
                last = end;
            }

            return Toward(last, into, most);
        }

        /// <summary>
        /// The gate, on an arm given by its joints' places (world): the turns that bring it into a body's range, each
        /// about its own joint and to be given in this order: <paramref name="turnUpper"/> to the upper arm (about
        /// the shoulder: the forearm and the hand go with it), then <paramref name="turnFore"/> to the forearm (about
        /// the elbow), then <paramref name="turnHand"/> to the hand. False (and no turns) for an arm let through as it is.
        /// </summary>
        public bool Hold(Quaternion chest, Vector3 shoulder, Vector3 elbow, Vector3 wrist, Quaternion hand, bool left, in ArmReading clip, float dt,
            out Quaternion turnUpper, out Quaternion turnFore, out Quaternion turnHand)
        {
            turnUpper = turnFore = turnHand = Quaternion.identity;
            Vector3 upperWay = elbow - shoulder, foreWay = wrist - elbow;
            if (upperWay.sqrMagnitude < 1e-10f || foreWay.sqrMagnitude < 1e-10f)
            {
                known = false;
                return false;
            }

            upperWay.Normalize();
            foreWay.Normalize();
            var back = Quaternion.Inverse(chest);
            float pace = CatchUp * dt;
            // (However far the arm sent moved in the step, it is followed no faster than an arm moves: a stage that
            // threw the elbow, its fold or the hand round in a step has it go there over a few.)
            float quick = MostSent > 0f ? MostSent * dt : float.MaxValue;
            bool held = false;
            var handSent = hand;

            // 1. Where the elbow is: the whole arm turned about the shoulder to the nearest place an elbow can be.
            Vector3 u = ArmLimits.Mirror(back * upperWay, left).normalized;
            Vector3 alongCame = u;
            Vector3 allowed = ArmLimits.NearestAllowed(u, clip, known, outAlong);
            if (known)
            {
                float most = Mathf.Min(Vector3.Angle(alongCame, inAlong), quick) + pace;
                if (Vector3.Angle(outAlong, allowed) > most)
                {
                    allowed = ArmLimits.NearestAllowed(Vector3.RotateTowards(outAlong, allowed, most * Mathf.Deg2Rad, 0f).normalized, clip, true, outAlong);
                }
            }

            if (Vector3.Angle(u, allowed) > 0.01f)
            {
                var turn = Quaternion.FromToRotation(upperWay, chest * ArmLimits.Mirror(allowed, left));
                turnUpper = turn;
                upperWay = (turn * upperWay).normalized;
                foreWay = (turn * foreWay).normalized;
                hand = turn * hand;
                u = ArmLimits.Mirror(back * upperWay, left).normalized;
                held = true;
            }

            // 2. The elbow's bend: the forearm turned back about the elbow, in the arm's own plane.
            float raised = Mathf.Acos(Mathf.Clamp(-u.y, -1f, 1f)) * Mathf.Rad2Deg;
            float bend = Vector3.Angle(upperWay, foreWay);
            Vector3 foldNow = Vector3.ProjectOnPlane(foreWay, upperWay);
            var unbend = Quaternion.identity;
            float mostBend = ArmLimits.MostBendWith(clip);
            bool foldFromLast = false;
            if (bend > mostBend + 0.01f && foldNow.sqrMagnitude > 1e-10f)
            {
                // (Folded right back on itself, an arm's fold can't be read from it: it opens the way it was folded the step before.)
                Vector3 was = known && foldKnown ? Vector3.ProjectOnPlane(chest * ArmLimits.Mirror(outFold, left), upperWay) : Vector3.zero;
                if (was.sqrMagnitude > 1e-6f && bend > 165f)
                {
                    foldNow = Vector3.ProjectOnPlane(Vector3.Slerp(foldNow.normalized, was.normalized, ArmLimits.Smooth(165f, 177f, bend)), upperWay);
                    foldFromLast = true;
                }

                float r = mostBend * Mathf.Deg2Rad;
                Vector3 then = upperWay * Mathf.Cos(r) + foldNow.normalized * Mathf.Sin(r);
                unbend = Quaternion.FromToRotation(foreWay, then);
                foreWay = then.normalized;
                hand = unbend * hand;
                bend = mostBend;
                foldNow = Vector3.ProjectOnPlane(foreWay, upperWay);
                held = true;
            }

            // (The bend as it was sent is followed no faster than an elbow bends: an arm thrown straight, or folded, in a
            // step opens or closes over a few, in its own plane; one too straight to say which way it folds, the way it
            // folded the step before.)
            float bendCame = bend;
            if (known && quick < float.MaxValue)
            {
                float most = Mathf.Min(Mathf.Abs(bendCame - inBend), quick) + pace;
                if (Mathf.Abs(bend - outBend) > most)
                {
                    float to = outBend + Mathf.Sign(bend - outBend) * most;
                    Vector3 was = Vector3.ProjectOnPlane(chest * ArmLimits.Mirror(outFold, left), upperWay);
                    float says = foldNow.sqrMagnitude > 1e-10f ? ArmLimits.Smooth(FoldKeptUnder, FoldReadFrom, bend) : 0f;
                    Vector3 way = was.sqrMagnitude < 1e-6f ? foldNow : says <= 0f ? was : Vector3.ProjectOnPlane(Vector3.Slerp(was.normalized, foldNow.normalized, says), upperWay);
                    if (way.sqrMagnitude > 1e-10f)
                    {
                        float r = to * Mathf.Deg2Rad;
                        Vector3 then = upperWay * Mathf.Cos(r) + way.normalized * Mathf.Sin(r);
                        var more = Quaternion.FromToRotation(foreWay, then);
                        unbend = more * unbend;
                        foreWay = then.normalized;
                        hand = more * hand;
                        bend = to;
                        foldNow = Vector3.ProjectOnPlane(foreWay, upperWay);
                        held = true;
                    }
                }
            }

            turnFore = unbend;

            // 3. Which way the elbow folds: the arm turned about the upper arm's own length (the forearm's own turn
            // goes with it: given after the upper arm's, it is the same turn carried round).
            float folded = ArmLimits.FoldWeight(bend, raised);
            bool foldHeld = false;
            float twistNow = 0f;
            Vector3 foldCame = Vector3.zero;
            // (Whenever the arm has a fold to read, however little it says: its range is then open all the way round, and
            // the arm, if it was still on its way over, goes on from where it was instead of being dropped where it was sent.)
            if (bend >= NoFoldUnder && foldNow.sqrMagnitude > 1e-6f && raised < 179f)
            {
                foldNow.Normalize();
                Vector3 f = ArmLimits.Mirror(back * foreWay, left);
                var swing = Quaternion.FromToRotation(Vector3.down, u);
                Vector3 ahead = swing * Vector3.forward, inward = swing * Vector3.left;
                Vector3 fold = Vector3.ProjectOnPlane(f, u).normalized;
                foldCame = fold;
                float turned = Mathf.Atan2(Vector3.Dot(fold, inward), Vector3.Dot(fold, ahead)) * Mathf.Rad2Deg;
                ArmLimits.TurnRange(folded, clip, out float low, out float high);
                Vector3 was = Vector3.ProjectOnPlane(outFold, u);
                if (known && !foldKnown && Mathf.Abs(twist) > 0.01f)
                {
                    // (Coming from an arm too straight to read, still turned about its length by what was left of an earlier
                    // turn: its fold was where that turn has the fold that came.)
                    was = ArmLimits.Mirror(back * (Quaternion.AngleAxis(twist, upperWay) * foldNow), left);
                    inFold = fold;
                }

                bool from = known && (foldKnown || Mathf.Abs(twist) > 0.01f) && was.sqrMagnitude > 1e-6f;
                float last = from ? Mathf.Atan2(Vector3.Dot(was, inward), Vector3.Dot(was, ahead)) * Mathf.Rad2Deg : 0f;
                float into = ArmLimits.Into(turned, low, high, from, last);
                if (from)
                {
                    // (As far as the fold that came turned itself, and the pace; one read from the step before, the pace alone.)
                    float moved = foldFromLast ? 0f : Mathf.Min(Vector3.Angle(fold, Vector3.ProjectOnPlane(inFold, u)), quick);
                    into = From(last, into, low, high, moved + pace, pace);
                }

                if (Mathf.Abs(Mathf.DeltaAngle(turned, into)) > 0.01f)
                {
                    float r = into * Mathf.Deg2Rad;
                    fold = ahead * Mathf.Cos(r) + inward * Mathf.Sin(r);
                    Vector3 foldThen = chest * ArmLimits.Mirror(fold, left);
                    float by = Mathf.Atan2(Vector3.Dot(Vector3.Cross(foldNow, foldThen), upperWay), Vector3.Dot(foldNow, foldThen)) * Mathf.Rad2Deg;
                    var turn = Quaternion.AngleAxis(by, upperWay);
                    turnUpper = turn * turnUpper;
                    turnFore = turn * unbend * Quaternion.Inverse(turn);
                    foreWay = (turn * foreWay).normalized;
                    hand = turn * hand;
                    twistNow = by;
                    held = true;
                }

                outFold = fold.normalized;
                foldHeld = true;
            }
            else if (known && Mathf.Abs(twist) > 0.01f)
            {
                // (Too straight to read which way it folds: what the arm was turned about its length by the step before
                // is let out at the pace, not dropped in a step (a straight arm shows no fold, but its hand goes with the turn).)
                twistNow = Toward(twist, 0f, pace);
                var turn = Quaternion.AngleAxis(twistNow, upperWay);
                turnUpper = turn * turnUpper;
                turnFore = turn * unbend * Quaternion.Inverse(turn);
                foreWay = (turn * foreWay).normalized;
                hand = turn * hand;
                held = true;
            }

            twist = twistNow;

            // 4. The wrist: the hand turned so the forearm lies in it as far to the palm and to the thumb as a wrist bends.
            // (The hand as the arm's turns carried it: its wrist bent as it was sent.)
            var carried = hand;
            Vector3 inHand = (Quaternion.Inverse(hand) * foreWay).normalized;
            Vector3 wristCame = inHand;
            Vector3 lies = ArmLimits.NearestWrist(inHand, left, clip, known && wristKnown, outWrist);
            if (known && wristKnown)
            {
                float most = Mathf.Min(Vector3.Angle(wristCame, inWrist), quick) + pace;
                if (Vector3.Angle(outWrist, lies) > most)
                {
                    lies = ArmLimits.NearestWrist(Vector3.RotateTowards(outWrist, lies, most * Mathf.Deg2Rad, 0f).normalized, left, clip, true, outWrist);
                }
            }

            if (lies != inHand && Vector3.Angle(inHand, lies) > 0.01f)
            {
                hand = Quaternion.FromToRotation(hand * lies, foreWay) * hand;
                held = true;
            }

            // 5. The forearm's roll: the hand turned about the forearm. Toward the way its thumb was sent, as far as a
            // forearm rolls: an arm turned under its hand (its elbow's fold brought round, the arm nearly straight)
            // leaves what the hand holds facing as it was sent, the forearm's roll taking up the turn.
            bool rollHeld = false;
            Vector3 thumbCame = Vector3.zero;
            {
                Vector3 f = ArmLimits.Mirror(back * foreWay, left);
                Vector3 lay = Vector3.ProjectOnPlane(ArmLimits.Mirror(back * (hand * Vector3.forward), left), f);
                Vector3 t = Vector3.ProjectOnPlane(ArmLimits.Mirror(back * (handSent * Vector3.forward), left), f);
                // (A thumb sent nearly along the forearm says little of the roll: by as much, the thumb as the hand lies, and no faster than the pace.)
                float says = ArmLimits.Smooth(0.05f, 0.25f, t.sqrMagnitude);
                Vector3 sentThumb = t.sqrMagnitude > 1e-6f ? t.normalized : Vector3.zero;
                if (says < 1f && lay.sqrMagnitude > 1e-6f)
                {
                    Vector3 both = (t.sqrMagnitude > 1e-6f ? t.normalized * says : Vector3.zero) + lay.normalized * (1f - says);
                    t = both.sqrMagnitude > 1e-4f ? both : lay;
                }

                if (t.sqrMagnitude > 1e-6f && lay.sqrMagnitude > 1e-6f)
                {
                    t.Normalize();
                    lay.Normalize();
                    thumbCame = sentThumb;
                    Vector3 was = Vector3.ProjectOnPlane(outThumb, f);
                    bool from = known && thumbKnown && was.sqrMagnitude > 1e-6f;
                    // (As far as the thumb sent turned itself, by as much as it says of the roll, now and the step before; and the pace.)
                    float most = from ? Mathf.Min(Mathf.Min(says, inSays) * Vector3.Angle(sentThumb, Vector3.ProjectOnPlane(inThumb, f)), quick) + pace : 0f;
                    Vector3 up = Vector3.ProjectOnPlane(-u, f);
                    Vector3 goes;
                    if (bend >= NoFoldUnder && up.sqrMagnitude > 1e-6f)
                    {
                        up.Normalize();
                        Vector3 medial = Vector3.Cross(f, up);
                        float roll = Mathf.Atan2(Vector3.Dot(t, medial), Vector3.Dot(t, up)) * Mathf.Rad2Deg;
                        ArmLimits.RollRange(folded, clip, out float low, out float high);
                        float last = from ? Mathf.Atan2(Vector3.Dot(was, medial), Vector3.Dot(was, up)) * Mathf.Rad2Deg : 0f;
                        float into = ArmLimits.Into(roll, low, high, from, last);
                        if (from)
                        {
                            into = From(last, into, low, high, most, pace);
                        }

                        float r = into * Mathf.Deg2Rad;
                        goes = up * Mathf.Cos(r) + medial * Mathf.Sin(r);
                    }
                    else
                    {
                        // (An arm too straight to say how its forearm is rolled: toward the thumb as sent, at the pace.)
                        goes = from ? Vector3.ProjectOnPlane(Vector3.RotateTowards(was.normalized, t, most * Mathf.Deg2Rad, 0f), f).normalized : t;
                    }

                    float by = Mathf.Atan2(Vector3.Dot(Vector3.Cross(lay, goes), f), Vector3.Dot(lay, goes)) * Mathf.Rad2Deg;
                    if (Mathf.Abs(by) > 0.01f)
                    {
                        // (About the forearm as it lies in the world: the same turn, the left arm's the other way round.)
                        hand = Quaternion.AngleAxis(left ? -by : by, foreWay) * hand;
                        held = true;
                    }

                    outThumb = goes;
                    inSays = says;
                    rollHeld = true;
                }
            }

            turnHand = hand * Quaternion.Inverse(carried);
            outAlong = u;
            outWrist = lies;
            outBend = bend;
            inBend = bendCame;
            inAlong = alongCame;
            inFold = foldCame;
            inThumb = thumbCame;
            inWrist = wristCame;
            foldKnown = foldHeld;
            thumbKnown = rollHeld;
            wristKnown = true;
            known = true;
            return held;
        }

        /// <summary>
        /// The gate on a posed arm: its bones turned so it is an arm a body has; and then (<see cref="ArmLimits.BonesAsABody"/>)
        /// its two bones set as a body's are for where its joints now are, the hand's turn kept. How far the arm was past
        /// (degrees; 0: let through as it was).
        /// </summary>
        public float Hold(AnimRig anim, bool left, in ArmReading clip, float dt)
        {
            Transform chest = anim.Part(PartId.Chest), upper = anim.Part(left ? PartId.UpperArmL : PartId.UpperArmR), fore = anim.Part(left ? PartId.ForearmL : PartId.ForearmR),
                hand = anim.Part(left ? PartId.HandL : PartId.HandR);
            if (!ArmLimits.On || chest == null || upper == null || fore == null || hand == null)
            {
                known = false;
                boneWayKnown = false;
                return 0f;
            }

            float past = ArmLimits.Read(chest.rotation, upper.position, fore.position, hand.position, hand.rotation, left, clip).Worst;
            bool held = Hold(chest.rotation, upper.position, fore.position, hand.position, hand.rotation, left, clip, dt, out var turnUpper, out var turnFore, out var turnHand);
            if (held)
            {
                upper.rotation = turnUpper * upper.rotation;
                fore.rotation = turnFore * fore.rotation;
                hand.rotation = turnHand * hand.rotation;
            }

            if (ArmLimits.BonesAsABody)
            {
                SetBones(anim, left, chest, upper, fore, hand, dt);
            }
            else
            {
                boneWayKnown = false;
                handKnown = false;
            }

            return held ? Mathf.Max(past, 0.01f) : 0f;
        }

        /// <summary>An arm bent less than this has its fold kept from the step before, and one bent more than <see cref="FoldReadFrom"/> has it read from where its joints are (degrees; between, by as much of each).</summary>
        public const float FoldKeptUnder = 3f, FoldReadFrom = 12f;

        /// <summary>
        /// The arm's bones set as a body's are (<see cref="ArmRig.SetBones"/>) for where its joints are, whatever posed
        /// it and however that rolled its bones. Which way the elbow folds is read from the joints' places; an arm too
        /// straight to read it from keeps the way it folded the step before, carried with the arm.
        /// </summary>
        private void SetBones(AnimRig anim, bool left, Transform chest, Transform upper, Transform fore, Transform hand, float dt)
        {
            Vector3 shoulder = upper.position, elbow = fore.position, wrist = hand.position;
            Vector3 n = wrist - shoulder;
            if (n.sqrMagnitude < 1e-10f)
            {
                return;
            }

            n.Normalize();
            var handTurn = hand.rotation;
            Vector3 way = Vector3.ProjectOnPlane(elbow - shoulder, n);
            float bend = Vector3.Angle(elbow - shoulder, wrist - elbow);
            float says = way.sqrMagnitude > 1e-10f ? ArmLimits.Smooth(FoldKeptUnder, FoldReadFrom, bend) : 0f;
            Vector3 kept = boneWayKnown ? Vector3.ProjectOnPlane(chest.rotation * boneWay, n) : Vector3.zero;
            Vector3 use;
            if (kept.sqrMagnitude < 1e-6f)
            {
                // (Nothing kept: as read, or, dead straight, down and a little back as an arm hangs.)
                use = way.sqrMagnitude > 1e-10f ? way : Vector3.ProjectOnPlane(chest.rotation * new Vector3(0f, -1f, -0.3f), n);
            }
            else if (says <= 0f)
            {
                use = kept;
            }
            else
            {
                // (Toward the fold as it reads, by as much as it says and no faster than the gate follows an arm: an arm
                // that comes out of straight folding another way has its upper arm turned to it over a few steps.)
                float most = MostSent > 0f ? says * (MostSent + CatchUp) * dt : 180f;
                use = Vector3.RotateTowards(kept.normalized, way.normalized, most * Mathf.Deg2Rad, 0f);
            }

            use = Vector3.ProjectOnPlane(use, n);
            if (use.sqrMagnitude < 1e-10f)
            {
                return;
            }

            use.Normalize();
            ArmRig.SetBones(anim, !left, elbow, wrist, use);
            hand.rotation = handTurn;
            // (The hand on the forearm, now that the forearm's own roll is a body's: turned no faster than a wrist and a
            // forearm turn a hand. The gate holds the wrist's bend and the forearm's roll each to that, but near the ends
            // of a wrist's range the two together turned the hand twice as far.)
            if (MostSent > 0f)
            {
                float most = (MostSent + CatchUp) * dt;
                if (handKnown && Quaternion.Angle(handWas, hand.localRotation) > most)
                {
                    hand.localRotation = Quaternion.RotateTowards(handWas, hand.localRotation, most);
                }

                handWas = hand.localRotation;
                handKnown = true;
            }
            else
            {
                handKnown = false;
            }

            boneWay = Quaternion.Inverse(chest.rotation) * use;
            boneWayKnown = true;
        }
    }
}
