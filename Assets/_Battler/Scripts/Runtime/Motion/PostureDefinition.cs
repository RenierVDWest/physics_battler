using System;
using PhysicsBattler.Combat;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>What a posture is for: feints and footwork lead into blows, defensive ones answer them.</summary>
    public enum PostureKind
    {
        Feint,
        Footwork,
        Guard,
        Flourish,
        Defence,
    }

    /// <summary>
    /// One key of a posture: where the body and hands are, against the base, at a moment. Lengths are
    /// for the medium body (hips, crouch and travel in leg lengths; hands in metres), scaled by the
    /// unit's size; angles in degrees. Unit space: x right, y up, z forward.
    /// </summary>
    [Serializable]
    public struct PostureKey
    {
        [Tooltip("When, as a share of the posture (0 to 1).")]
        [Range(0f, 1f)] public float at;
        [Tooltip("Hips moved, in leg lengths (x right, y up, z forward).")]
        public Vector3 hips;
        [Tooltip("Hips lowered, in leg lengths (knees bending).")]
        public float crouch;
        [Tooltip("Upper body bent forward (+) or back, degrees.")]
        public float lean;
        [Tooltip("Upper body bent to the right (+) or left, degrees.")]
        public float roll;
        [Tooltip("Hips and chest turned to the right (+) or left, degrees.")]
        public float turn;
        [Tooltip("Head: pitch (+ down) and yaw (+ right), degrees.")]
        public Vector2 head;
        [Tooltip("Weapon hand's grip moved from its guard, metres (x right, y up, z forward).")]
        public Vector3 main;
        [Tooltip("Weapon turned: pitch (+ point down), yaw (+ right), roll, degrees.")]
        public Vector3 mainTurn;
        [Tooltip("Off hand (or shield) moved from its guard, metres.")]
        public Vector3 off;
        [Tooltip("Shield turned: pitch, yaw, roll, degrees.")]
        public Vector3 offTurn;
        [Tooltip("Where the unit has moved on the board since the posture began, in leg lengths (kept at the end: footwork that goes somewhere).")]
        public Vector3 travel;
    }

    public enum PostureFoot
    {
        Lead,
        Rear,
    }

    public enum PostureStepTo
    {
        /// <summary>The foot's own spot in the stance, moved by the offset.</summary>
        OffsetFromHome,
        /// <summary>Back to its spot.</summary>
        Home,
    }

    /// <summary>A planned step during a posture (the stance takes no steps of its own meanwhile).</summary>
    [Serializable]
    public struct PostureStep
    {
        [Range(0f, 1f)] public float at;
        public PostureFoot foot;
        public PostureStepTo to;
        [Tooltip("From the foot's spot, in leg lengths (x right, z forward).")]
        public Vector3 offset;
        [Tooltip("How long the step takes, as a share of the posture.")]
        public float length;
    }

    /// <summary>A window in a posture when body parts are open to a blow.</summary>
    [Serializable]
    public struct OpeningWindow
    {
        [Range(0f, 1f)] public float from;
        [Range(0f, 1f)] public float to;
        public AimParts parts;
    }

    /// <summary>
    /// A posture (Fight Rhythm): a short burst of a fighting style's characteristic movement, played
    /// over the base between blows. A handful of key poses, interpolated smoothly from and back to
    /// the base; planned steps; optionally the wind-up of one of the unit's own blows played part of
    /// the way and back (a weapon feint looks exactly like the real thing); the openings it leaves.
    /// </summary>
    [CreateAssetMenu(menuName = "Physics Battler/Posture", fileName = "PD_Posture")]
    public class PostureDefinition : ScriptableObject
    {
        public PostureKind kind = PostureKind.Feint;
        [Tooltip("Seconds (medium size; bigger units are a little slower).")]
        public float duration = 0.5f;
        [Tooltip("How often it is picked against the style's other postures.")]
        public float weight = 1f;
        [Tooltip("Played as the lead-in to a blow (a feint before the real thing).")]
        public bool leadIn;

        [Header("Weapon feint")]
        [Tooltip("A move of the unit's weapon (its name without MD_, e.g. ClubSwing) whose wind-up is played part of the way and back. Empty: none.")]
        public string feintMove = "";
        [Tooltip("How far into the move's wind-up the feint goes (0 to 1).")]
        [Range(0f, 1f)] public float feintDepth = 0.5f;

        [Header("Movement")]
        public PostureKey[] keys = Array.Empty<PostureKey>();
        public PostureStep[] steps = Array.Empty<PostureStep>();

        [Header("Openings and phrases")]
        public OpeningWindow[] openings = Array.Empty<OpeningWindow>();
        [Tooltip("The posture that tends to follow this one (its name without PD_), and how much more likely it gets.")]
        public string followedBy = "";
        public float followBias = 1.5f;

        public string Label => name.StartsWith("PD_") ? name.Substring(3) : name;

        /// <summary>
        /// The pose at a share of the posture: the keys interpolated smoothly (Catmull-Rom) from the
        /// base at 0 back to the base at 1; travel is kept at the end.
        /// </summary>
        public PostureKey Sample(float u)
        {
            u = Mathf.Clamp01(u);
            int n = keys.Length;
            if (n == 0)
            {
                return default;
            }

            // The keys with the base before and after them.
            float At(int i) => i <= 0 ? 0f : i > n ? 1f : keys[i - 1].at;
            PostureKey Key(int i) => i <= 0 ? default : i > n ? End() : keys[i - 1];
            int seg = 0;
            while (seg < n && u >= At(seg + 1))
            {
                seg++;
            }

            float t0 = At(seg), t1 = At(seg + 1);
            float s = t1 > t0 ? (u - t0) / (t1 - t0) : 1f;
            s = Mathf.Lerp(s, s * s * (3f - 2f * s), 0.5f);   // eased a little, without stopping at each key
            var a = Key(seg - 1);
            var b = Key(seg);
            var c = Key(seg + 1);
            var d = Key(seg + 2);
            return new PostureKey
            {
                at = u,
                hips = Spline(a.hips, b.hips, c.hips, d.hips, s),
                crouch = Spline(a.crouch, b.crouch, c.crouch, d.crouch, s),
                lean = Spline(a.lean, b.lean, c.lean, d.lean, s),
                roll = Spline(a.roll, b.roll, c.roll, d.roll, s),
                turn = Spline(a.turn, b.turn, c.turn, d.turn, s),
                head = new Vector2(Spline(a.head.x, b.head.x, c.head.x, d.head.x, s), Spline(a.head.y, b.head.y, c.head.y, d.head.y, s)),
                main = Spline(a.main, b.main, c.main, d.main, s),
                mainTurn = Spline(a.mainTurn, b.mainTurn, c.mainTurn, d.mainTurn, s),
                off = Spline(a.off, b.off, c.off, d.off, s),
                offTurn = Spline(a.offTurn, b.offTurn, c.offTurn, d.offTurn, s),
                travel = Vector3.Lerp(b.travel, c.travel, s * s * (3f - 2f * s)),
            };
        }

        /// <summary>The base again at the end, the travel kept.</summary>
        private PostureKey End() => new PostureKey { at = 1f, travel = keys.Length > 0 ? keys[keys.Length - 1].travel : Vector3.zero };

        /// <summary>The body parts open at a share of the posture.</summary>
        public AimParts OpenAt(float u)
        {
            var open = AimParts.None;
            foreach (var w in openings)
            {
                if (u >= w.from && u <= w.to)
                {
                    open |= w.parts;
                }
            }

            return open;
        }

        /// <summary>How far into the feint's wind-up the weapon is at a share of the posture (0 to 1): out, a moment held, back.</summary>
        public float FeintAt(float u)
        {
            float rise = Smooth(u / 0.45f);
            float fall = 1f - Smooth((u - 0.6f) / 0.4f);
            return Mathf.Min(rise, fall);
        }

        private static float Smooth(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3f - 2f * x);
        }

        private static float Spline(float a, float b, float c, float d, float u)
        {
            float u2 = u * u, u3 = u2 * u;
            return 0.5f * (2f * b + (c - a) * u + (2f * a - 5f * b + 4f * c - d) * u2 + (3f * b - a - 3f * c + d) * u3);
        }

        private static Vector3 Spline(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float u) =>
            new Vector3(Spline(a.x, b.x, c.x, d.x, u), Spline(a.y, b.y, c.y, d.y, u), Spline(a.z, b.z, c.z, d.z, u));
    }
}
