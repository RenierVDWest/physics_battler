using UnityEngine;

namespace PhysicsBattler.Combat
{
    /// <summary>The ready-made moves, used to fill new assets.</summary>
    public enum MoveStyle
    {
        SwordCut,
        SwordBackhand,
        SwordThrust,
        ClubSwing,
        ClubSmash,
    }

    /// <summary>
    /// One aimed blow, ported from Hexamus's attack motions and re-based on the target: the path is
    /// where the weapon's striking part goes, in an aim frame whose origin is the body part aimed at
    /// (locked where it was when the blow started), so one move works at any distance, height or angle.
    ///
    /// Aim frame (metres, medium size; units scale it with their size): x to the attacker's right,
    /// y up, z away from the attacker (through the target). The path point at
    /// <see cref="contactIndex"/> is the contact, normally (0, 0, 0).
    ///
    /// The unit lunges so the aim point is <see cref="reach"/> from its chest at contact, turns and
    /// leans as the body curves say, and its weapon arm is solved (two-bone IK) so the striking part
    /// lies on the path, the blade pointing out from the chest through it (or at the aim point, for
    /// a thrust). Edit these while the game runs; the next blow uses the changes.
    /// </summary>
    [CreateAssetMenu(menuName = "Physics Battler/Move", fileName = "MD_")]
    public class MoveDefinition : ScriptableObject
    {
        [Header("Aim")]
        public StrikeWith strikeWith = StrikeWith.Edge;
        [Tooltip("The body parts this move can be aimed at.")]
        public AimParts targets = AimParts.All;

        [Header("Timing")]
        [Min(0.1f), Tooltip("Seconds at medium size (bigger units are slower, smaller ones quicker).")]
        public float duration = 0.7f;
        [Tooltip("How far along the path (0 to 1) the striking part is over the move's time (0 to 1). Steep is fast.")]
        public AnimationCurve speed = SmoothCurves.Through(0f, 0f, 0.4f, 0.32f, 0.58f, 0.6f, 0.72f, 0.82f, 1f, 1f);
        [Tooltip("How much the move takes over from the guard (0 to 1). Must be 1 at contact.")]
        public AnimationCurve blend = SmoothCurves.Through(0f, 0f, 0.15f, 1f, 0.8f, 1f, 1f, 0f);
        [Tooltip("Progress along the path (0 to 1) during which the weapon can strike: contacts are logged in it.")]
        public Vector2 activeWindow = new Vector2(0.45f, 0.8f);
        [Min(0f), Tooltip("Seconds the move freezes at a hit (milestone 3).")]
        public float hitStop = 0.06f;

        [Header("Path of the striking part (aim frame, medium size)")]
        public Vector3[] path =
        {
            new Vector3(0.3f, -0.3f, -0.45f), new Vector3(0.55f, 0.4f, -0.75f), new Vector3(0.42f, 0.22f, -0.3f),
            Vector3.zero, new Vector3(-0.36f, -0.12f, 0.04f), new Vector3(-0.6f, -0.2f, -0.06f),
        };
        [Tooltip("The path point that is the contact.")]
        public int contactIndex = 3;
        [Tooltip("The blade points at the aim point the whole time, in line with the weapon arm's shoulder (thrusts), instead of out from the chest through the striking part.")]
        public bool pointForward;
        [Tooltip("Metres above the chest the blade points out from: raise it for blows that come down from above.")]
        public float pivotRaise;

        [Header("Body")]
        [Tooltip("Distance from the attacker's chest to the aim point at contact (medium size). The lunge makes it so.")]
        public float reach = 1f;
        [Tooltip("The longest lunge, metres at medium size. Further than this is out of reach.")]
        public float maxLunge = 1.1f;
        [Tooltip("Lunge and lean over time (0 to 1 at contact). Dip below 0 to rock back before striking.")]
        public AnimationCurve bodyCurve = SmoothCurves.Through(0f, 0f, 0.3f, -0.25f, 0.58f, 1f, 0.75f, 0.85f, 1f, 0f);
        [Tooltip("Degrees the body leans forward at the body curve's peak.")]
        public float lean = 12f;
        [Tooltip("Degrees the hips and chest are turned at the start and end of the path (+ turns right).")]
        public float twistStart = 35f;
        public float twistEnd = -40f;

        [Header("Physics")]
        [Range(0f, 1f), Tooltip("How firmly the weapon arm is pinned to the pose during the move.")]
        public float weaponArmPin = 1f;

        /// <summary>Progress along the path (0 to 1) at the move's time (0 to 1).</summary>
        public float ProgressAt(float t) => Mathf.Clamp01(speed.Evaluate(t));

        public float BlendAt(float t) => Mathf.Clamp01(blend.Evaluate(t));

        public float BodyAt(float t) => bodyCurve.Evaluate(t);

        public float TwistAt(float progress) => Mathf.Lerp(twistStart, twistEnd, progress);

        public float ContactProgress => path.Length > 1 ? Mathf.Clamp01((float)contactIndex / (path.Length - 1)) : 0f;

        /// <summary>The move's time (0 to 1) at which the striking part reaches the contact point.</summary>
        public float ContactTime()
        {
            float wanted = ContactProgress;
            float lo = 0f, hi = 1f;
            for (int i = 0; i < 40; i++)
            {
                float mid = 0.5f * (lo + hi);
                if (ProgressAt(mid) < wanted)
                {
                    lo = mid;
                }
                else
                {
                    hi = mid;
                }
            }

            return hi;
        }

        /// <summary>The striking part on the path (aim frame, medium size): a smooth curve through every point.</summary>
        public Vector3 PointAt(float progress) => CatmullRom(path, progress);

        public static Vector3 CatmullRom(Vector3[] points, float progress)
        {
            if (points == null || points.Length == 0)
            {
                return Vector3.zero;
            }

            if (points.Length == 1)
            {
                return points[0];
            }

            float position = Mathf.Clamp01(progress) * (points.Length - 1);
            int i = Mathf.Min((int)position, points.Length - 2);
            float s = position - i;
            var p0 = points[Mathf.Max(i - 1, 0)];
            var p1 = points[i];
            var p2 = points[i + 1];
            var p3 = points[Mathf.Min(i + 2, points.Length - 1)];
            return 0.5f * (2f * p1 + (p2 - p0) * s + (2f * p0 - 5f * p1 + 4f * p2 - p3) * (s * s)
                           + (3f * p1 - p0 - 3f * p2 + p3) * (s * s * s));
        }

        public static MoveDefinition Create(MoveStyle style)
        {
            var move = CreateInstance<MoveDefinition>();
            move.name = "MD_" + style;
            move.ApplyDefaults(style);
            return move;
        }

        /// <summary>Fills in a ready-made move.</summary>
        public void ApplyDefaults(MoveStyle style)
        {
            blend = SmoothCurves.Through(0f, 0f, 0.15f, 1f, 0.8f, 1f, 1f, 0f);
            pointForward = false;
            pivotRaise = 0f;
            weaponArmPin = 1f;
            hitStop = 0.06f;
            contactIndex = 3;
            switch (style)
            {
                case MoveStyle.SwordCut:
                    // A forehand cut: wound up high on the right, down across the target to low on the left.
                    strikeWith = StrikeWith.Edge;
                    targets = AimParts.All;
                    duration = 0.7f;
                    speed = SmoothCurves.Through(0f, 0f, 0.4f, 0.32f, 0.58f, 0.6f, 0.72f, 0.82f, 1f, 1f);
                    path = new[]
                    {
                        new Vector3(0.3f, -0.3f, -0.45f),    // from the guard
                        new Vector3(0.55f, 0.4f, -0.75f),    // wound up high on the right
                        new Vector3(0.42f, 0.22f, -0.3f),    // coming in
                        Vector3.zero,                        // through the aim point
                        new Vector3(-0.36f, -0.12f, 0.04f),  // out the other side
                        new Vector3(-0.6f, -0.2f, -0.06f),   // follow-through, out low on the left (clear of the hips)
                    };
                    reach = 1f;
                    maxLunge = 1.1f;
                    bodyCurve = SmoothCurves.Through(0f, 0f, 0.3f, -0.25f, 0.58f, 1f, 0.75f, 0.85f, 1f, 0f);
                    lean = 12f;
                    twistStart = 35f;
                    twistEnd = -40f;
                    activeWindow = new Vector2(0.45f, 0.78f);
                    break;

                case MoveStyle.SwordBackhand:
                    // The other way: wound up across the body on the left, cut out to the right.
                    strikeWith = StrikeWith.Edge;
                    targets = AimParts.All;
                    duration = 0.7f;
                    speed = SmoothCurves.Through(0f, 0f, 0.4f, 0.32f, 0.58f, 0.6f, 0.72f, 0.82f, 1f, 1f);
                    path = new[]
                    {
                        new Vector3(0.25f, -0.3f, -0.45f),
                        new Vector3(-0.42f, 0.38f, -0.58f),  // wound up high on the left, out from the chest
                        new Vector3(-0.38f, 0.18f, -0.24f),
                        Vector3.zero,
                        new Vector3(0.38f, -0.1f, 0.04f),
                        new Vector3(0.6f, -0.18f, -0.06f),   // out low on the right (clear of the hips)
                    };
                    reach = 0.95f;
                    maxLunge = 1.1f;
                    bodyCurve = SmoothCurves.Through(0f, 0f, 0.3f, -0.2f, 0.58f, 1f, 0.75f, 0.85f, 1f, 0f);
                    lean = 10f;
                    twistStart = -30f;
                    twistEnd = 35f;
                    activeWindow = new Vector2(0.45f, 0.78f);
                    break;

                case MoveStyle.SwordThrust:
                    // Drawn back, then driven straight at the aim point, point first.
                    strikeWith = StrikeWith.Tip;
                    targets = AimParts.Body;
                    duration = 0.6f;
                    speed = SmoothCurves.Through(0f, 0f, 0.45f, 0.38f, 0.6f, 0.6f, 0.75f, 0.75f, 1f, 1f);
                    path = new[]
                    {
                        new Vector3(0.25f, -0.3f, -0.5f),
                        new Vector3(0.28f, -0.12f, -0.6f),   // drawn back
                        new Vector3(0.08f, -0.03f, -0.22f),
                        Vector3.zero,                        // home
                        new Vector3(0f, 0f, 0.06f),
                        new Vector3(0.22f, -0.25f, -0.55f),  // pulled back out
                    };
                    pointForward = true;
                    reach = 1.22f;
                    maxLunge = 1.2f;
                    bodyCurve = SmoothCurves.Through(0f, 0f, 0.42f, -0.3f, 0.6f, 1f, 0.75f, 0.9f, 1f, 0f);
                    lean = 16f;
                    twistStart = 20f;
                    twistEnd = -15f;
                    activeWindow = new Vector2(0.5f, 0.72f);
                    break;

                case MoveStyle.ClubSwing:
                    // A heavy forehand swing, wound far back on the right.
                    strikeWith = StrikeWith.Head;
                    targets = AimParts.All;
                    duration = 0.8f;
                    speed = SmoothCurves.Through(0f, 0f, 0.42f, 0.3f, 0.6f, 0.6f, 0.74f, 0.82f, 1f, 1f);
                    path = new[]
                    {
                        new Vector3(0.3f, -0.3f, -0.45f),
                        new Vector3(0.62f, 0.3f, -0.8f),     // wound far back
                        new Vector3(0.48f, 0.12f, -0.28f),   // coming round
                        Vector3.zero,                        // through the aim point
                        new Vector3(-0.38f, -0.06f, 0f),     // out to the left
                        new Vector3(-0.48f, -0.28f, -0.38f), // follow-through
                    };
                    reach = 1f;
                    maxLunge = 1.1f;
                    bodyCurve = SmoothCurves.Through(0f, 0f, 0.32f, -0.35f, 0.6f, 1f, 0.78f, 0.8f, 1f, 0f);
                    lean = 10f;
                    twistStart = 45f;
                    twistEnd = -55f;
                    activeWindow = new Vector2(0.45f, 0.78f);
                    weaponArmPin = 0.95f;
                    break;

                case MoveStyle.ClubSmash:
                    // Raised high overhead, then brought straight down onto the aim point.
                    strikeWith = StrikeWith.Head;
                    targets = AimParts.Head | AimParts.Neck | AimParts.Chest | AimParts.Elbows;
                    duration = 0.9f;
                    speed = SmoothCurves.Through(0f, 0f, 0.45f, 0.5f, 0.62f, 0.8f, 0.76f, 0.9f, 1f, 1f);
                    path = new[]
                    {
                        new Vector3(0.25f, -0.3f, -0.45f),
                        new Vector3(0.3f, 0.6f, -0.6f),      // raised up beside and behind the head
                        new Vector3(0.12f, 0.78f, -0.34f),   // over the top
                        new Vector3(0.03f, 0.4f, -0.12f),    // coming down
                        Vector3.zero,                        // onto the aim point
                        new Vector3(0.03f, -0.12f, -0.05f),  // jarred
                    };
                    contactIndex = 4;
                    pivotRaise = 0.35f;
                    reach = 0.95f;
                    maxLunge = 1f;
                    bodyCurve = SmoothCurves.Through(0f, 0f, 0.45f, -0.5f, 0.62f, 1f, 0.8f, 0.9f, 1f, 0f);
                    lean = 20f;
                    twistStart = 10f;
                    twistEnd = 0f;
                    activeWindow = new Vector2(0.65f, 0.9f);
                    weaponArmPin = 0.95f;
                    break;
            }
        }
    }

    /// <summary>AnimationCurves from a few points, smooth through each and easing in and out at the ends.</summary>
    public static class SmoothCurves
    {
        /// <param name="points">Alternating time and value: t0, v0, t1, v1, ... with times increasing.</param>
        public static AnimationCurve Through(params float[] points)
        {
            int count = points.Length / 2;
            var keys = new Keyframe[count];
            for (int i = 0; i < count; i++)
            {
                float slope = 0f;
                if (i > 0 && i < count - 1)
                {
                    slope = (points[(i + 1) * 2 + 1] - points[(i - 1) * 2 + 1]) / (points[(i + 1) * 2] - points[(i - 1) * 2]);
                }

                keys[i] = new Keyframe(points[i * 2], points[i * 2 + 1], slope, slope);
            }

            return new AnimationCurve(keys);
        }
    }
}
