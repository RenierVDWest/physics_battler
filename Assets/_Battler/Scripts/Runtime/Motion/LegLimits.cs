using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// A leg read as a body's leg (the plan "Grounded legs", step 0, the legs' own reading; a measure only, nothing is
    /// held to it yet): from where its joints are, not from how its bones are rolled. Both legs read as a right leg.
    /// </summary>
    public struct LegReading
    {
        /// <summary>The thigh against straight down (degrees), and which way it is raised (0 forward, 90 out to its own side, 180 back, negative across the other leg).</summary>
        public float Raised, Toward;

        /// <summary>Which way the knee folds, against straight back as the thigh carries it (degrees; positive: the knee turned out, its shin swung in toward the other leg; negative: the knee turned in).</summary>
        public float Turn;

        /// <summary>The knee's bend (degrees; 0 straight).</summary>
        public float Bend;

        /// <summary>The ankle: the foot's toes down from square to the shin (degrees; negative: toes up).</summary>
        public float Point;

        /// <summary>How far past a body's range each is (degrees; 0: within it): the thigh's place, the knee's fold, its bend, the ankle.</summary>
        public float PastPlace, PastTurn, PastBend, PastAnkle;

        /// <summary>How much the knee's fold says (0 for a straight leg, 1 bent).</summary>
        public float Folded;

        public float Worst => Mathf.Max(Mathf.Max(PastPlace, PastTurn), Mathf.Max(PastBend, PastAnkle));
    }

    /// <summary>
    /// A leg's ranges, set as the arm's were from the recorded clips, which are a person's (<c>legrange</c> reads every
    /// frame of every clip against them): the thigh raised to 125 forward and forward-out, 75 to the side, 55 behind
    /// (against the line from the hips to the spine over them, which leans with the trunk), 30 across; the knee turned
    /// out to 85 and in to 50 from folding straight back; bent to 155; the ankle 35 up, 60 down (from anatomy: a clip
    /// has no sole to read). A measure: nothing is held to them yet.
    /// </summary>
    public static class LegLimits
    {
        private static readonly float[] TowardAt = { -180f, -135f, -90f, -45f, 0f, 45f, 90f, 135f, 180f };
        private static readonly float[] HighestAt = { 55f, 35f, 30f, 75f, 125f, 125f, 75f, 55f, 55f };

        /// <summary>How far the knee turns out and in from folding straight back (degrees), the knee's most bend, and the ankle's toes up and down from square.</summary>
        public const float KneeOut = 85f, KneeIn = 50f, MostBend = 155f, ToesUp = 35f, ToesDown = 60f;

        /// <summary>The highest a thigh is raised toward a side (degrees from straight down).</summary>
        public static float Highest(float toward)
        {
            toward = Mathf.Clamp(Mathf.DeltaAngle(0f, toward), -180f, 180f);
            for (int i = 1; i < TowardAt.Length; i++)
            {
                if (toward <= TowardAt[i])
                {
                    float t = Mathf.InverseLerp(TowardAt[i - 1], TowardAt[i], toward);
                    return Mathf.Lerp(HighestAt[i - 1], HighestAt[i], t * t * (3f - 2f * t));
                }
            }

            return HighestAt[HighestAt.Length - 1];
        }

        /// <summary>
        /// A leg read: its hips' turn (z forward, y up), where its hip, knee and ankle are, and which way its toes
        /// point (from the ankle).
        /// </summary>
        public static LegReading Read(Quaternion hips, Vector3 hip, Vector3 knee, Vector3 ankle, Vector3 toes, bool left)
        {
            var r = new LegReading();
            var back = Quaternion.Inverse(hips);
            Vector3 u = back * (knee - hip), s = back * (ankle - knee), t = back * toes;
            if (u.sqrMagnitude < 1e-10f || s.sqrMagnitude < 1e-10f)
            {
                return r;
            }

            if (left)
            {
                u.x = -u.x;
                s.x = -s.x;
                t.x = -t.x;
            }

            u.Normalize();
            s.Normalize();
            r.Raised = Mathf.Acos(Mathf.Clamp(-u.y, -1f, 1f)) * Mathf.Rad2Deg;
            r.Toward = Mathf.Atan2(u.x, u.z) * Mathf.Rad2Deg;
            // (Nearly straight down, which way it is raised says little: the reach there is what every side allows.)
            float says = ArmLimits.Smooth(3f, 15f, r.Raised);
            float highest = Mathf.Lerp(30f, Highest(r.Toward), says);
            r.PastPlace = Mathf.Max(0f, r.Raised - highest);

            r.Bend = Vector3.Angle(u, s);
            r.PastBend = Mathf.Max(0f, r.Bend - MostBend);
            r.Folded = ArmLimits.Smooth(10f, 30f, r.Bend);
            Vector3 fold = Vector3.ProjectOnPlane(s, u);
            if (fold.sqrMagnitude > 1e-8f && r.Raised < 179f)
            {
                fold.Normalize();
                var swing = Quaternion.FromToRotation(Vector3.down, u);
                Vector3 behind = swing * Vector3.back, inward = swing * Vector3.left;
                r.Turn = Mathf.Atan2(Vector3.Dot(fold, inward), Vector3.Dot(fold, behind)) * Mathf.Rad2Deg;
                r.PastTurn = r.Folded * Mathf.Max(0f, Mathf.Max(r.Turn - KneeOut, -KneeIn - r.Turn));
            }

            if (t.sqrMagnitude > 1e-10f)
            {
                r.Point = 90f - Vector3.Angle(t.normalized, s);
                r.PastAnkle = Mathf.Max(0f, Mathf.Max(r.Point - ToesDown, -ToesUp - r.Point));
            }

            return r;
        }

        /// <summary>A clip's own leg, from its points (the hips and the spine over them, the two hips' joints, the leg's hip, knee and ankle).</summary>
        public static LegReading ReadClip(Vector3[] points, int hips, int spine, int leftHip, int rightHip, bool left)
        {
            int h = left ? leftHip : rightHip;
            if (points == null || points.Length <= Mathf.Max(rightHip + 3, Mathf.Max(hips, spine)))
            {
                return default;
            }

            Vector3 up = points[spine] - points[hips], right = points[rightHip] - points[leftHip];
            Vector3 forward = Vector3.Cross(right, up);
            if (forward.sqrMagnitude < 1e-10f || up.sqrMagnitude < 1e-10f)
            {
                return default;
            }

            // (Not its ankle: a clip has the ankle and the ball of the foot, and the line between them points down at rest
            // by an angle that is the skeleton's own; the pose's foot has a sole to read.)
            return Read(Quaternion.LookRotation(forward, up), points[h], points[h + 1], points[h + 2], Vector3.zero, left);
        }
    }
}
