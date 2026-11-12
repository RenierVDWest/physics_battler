using UnityEngine;

/// <summary>
/// An arm read as a body's arm, from where its joints are (not from how its bones happen to be rolled): where the
/// elbow is against the chest (how high the upper arm is raised, and toward where), how the upper arm is turned
/// about its own length (which way the elbow folds), the elbow's bend, the forearm's roll (the thumb against the
/// elbow's fold) and the wrist's bend. And how far each is past what a person's arm does (degrees; 0 inside).
/// Both arms are read as a right arm (the left mirrored): out is away from the body's middle.
/// </summary>
public struct ArmAnatomy
{
    /// <summary>The upper arm against straight down (0 hanging, 90 level, 180 overhead) and toward where (0 forward, 90 out to the side, 180 back, negative across the body).</summary>
    public float Raised, Toward;

    /// <summary>The upper arm's turn about its length (positive: the forearm folds in toward the body's middle; negative: out); the elbow's bend (0 straight).</summary>
    public float Turn, Bend;

    /// <summary>The forearm's roll (positive: thumb turned in, palm down), the wrist bent toward the palm (negative: the back of the hand) and toward the thumb (negative: the little finger).</summary>
    public float Roll, Flex, Thumbward;

    /// <summary>How far past a person's range (degrees): the elbow's place, the upper arm's turn, the elbow's bend, the forearm's roll, the wrist.</summary>
    public float PastReach, PastTurn, PastBend, PastRoll, PastWrist;

    public float Worst => Mathf.Max(Mathf.Max(PastReach, PastTurn), Mathf.Max(PastBend, Mathf.Max(PastRoll, PastWrist)));

    public const float MostTurnIn = 100f, MostTurnOut = 90f, MostBend = 150f, MostRoll = 90f, MostFlex = 80f, MostThumbward = 30f, MostLittleFinger = 45f;

    // (How high the upper arm goes, by where it points: all the way up in front and out to the side; behind, and across the body, the trunk and the shoulder stop it.)
    private static readonly float[] TowardAt = { -180f, -150f, -120f, -90f, -60f, -40f, -20f, 0f, 100f, 120f, 140f, 160f, 180f };
    private static readonly float[] HighestAt = { 55f, 35f, 30f, 40f, 75f, 110f, 160f, 180f, 180f, 140f, 100f, 70f, 55f };

    public static float Highest(float toward)
    {
        for (int i = 1; i < TowardAt.Length; i++)
        {
            if (toward <= TowardAt[i])
            {
                return Mathf.Lerp(HighestAt[i - 1], HighestAt[i], Mathf.InverseLerp(TowardAt[i - 1], TowardAt[i], toward));
            }
        }

        return HighestAt[HighestAt.Length - 1];
    }

    /// <summary>Reads an arm. <paramref name="left"/>: the left arm (mirrored). The hand's own axes at rest: fingers down, thumb forward, palm toward the body.</summary>
    public static ArmAnatomy Read(Quaternion chest, Vector3 shoulder, Vector3 elbow, Vector3 wrist, Quaternion hand, bool left)
    {
        var a = new ArmAnatomy();
        var back = Quaternion.Inverse(chest);
        Vector3 M(Vector3 v) => left ? new Vector3(-v.x, v.y, v.z) : v;
        Vector3 u = M(back * (elbow - shoulder)).normalized, f = M(back * (wrist - elbow)).normalized;
        a.Raised = Mathf.Acos(Mathf.Clamp(-u.y, -1f, 1f)) * Mathf.Rad2Deg;
        a.Toward = Mathf.Atan2(u.x, u.z) * Mathf.Rad2Deg;
        // (How far from the nearest place an elbow can be: straight up is one place, whichever way it is said to point.)
        if (a.Raised > 30f && a.Raised > Highest(a.Toward))
        {
            float nearest = 180f;
            for (float toward = -180f; toward < 180f; toward += 10f)
            {
                float raised = Mathf.Min(a.Raised, Highest(toward)) * Mathf.Deg2Rad, t = toward * Mathf.Deg2Rad;
                var allowed = new Vector3(Mathf.Sin(raised) * Mathf.Sin(t), -Mathf.Cos(raised), Mathf.Sin(raised) * Mathf.Cos(t));
                nearest = Mathf.Min(nearest, Vector3.Angle(u, allowed));
            }

            a.PastReach = nearest;
        }
        a.Bend = Vector3.Angle(u, f);
        a.PastBend = Mathf.Max(0f, a.Bend - MostBend);
        // (The fold a hanging arm has, forward, carried to where the upper arm points the short way: a forearm raised in
        // front folds up, one held out to the side folds forward.)
        var swing = Quaternion.FromToRotation(Vector3.down, u);
        Vector3 ahead = swing * Vector3.forward, inward = swing * Vector3.left;
        Vector3 fold = Vector3.ProjectOnPlane(f, u);
        bool folded = a.Bend > 20f && a.Bend < 175f && a.Raised < 165f;
        if (folded)
        {
            a.Turn = Mathf.Atan2(Vector3.Dot(fold, inward), Vector3.Dot(fold, ahead)) * Mathf.Rad2Deg;
            a.PastTurn = Mathf.Max(0f, Mathf.Max(a.Turn - MostTurnIn, -MostTurnOut - a.Turn));
        }

        Vector3 fingers = M(back * (hand * Vector3.down)), thumb = M(back * (hand * Vector3.forward)), palm = M(back * (hand * (left ? Vector3.right : Vector3.left)));
        a.Flex = Mathf.Atan2(Vector3.Dot(f, palm), Vector3.Dot(f, fingers)) * -Mathf.Rad2Deg;
        a.Thumbward = Mathf.Asin(Mathf.Clamp(Vector3.Dot(f, thumb), -1f, 1f)) * -Mathf.Rad2Deg;
        a.PastWrist = Mathf.Max(0f, Mathf.Max(Mathf.Abs(a.Flex) - MostFlex, Mathf.Max(a.Thumbward - MostThumbward, -MostLittleFinger - a.Thumbward)));
        if (folded)
        {
            // (Thumb up the arm, toward the shoulder's side of the fold, is the forearm unrolled.)
            Vector3 up = Vector3.ProjectOnPlane(-u, f).normalized, t = Vector3.ProjectOnPlane(thumb, f);
            Vector3 medial = Vector3.Cross(f, up);
            a.Roll = Mathf.Atan2(Vector3.Dot(t, medial), Vector3.Dot(t, up)) * Mathf.Rad2Deg;
            a.PastRoll = Mathf.Max(0f, Mathf.Abs(a.Roll) - MostRoll);
        }

        return a;
    }
}
