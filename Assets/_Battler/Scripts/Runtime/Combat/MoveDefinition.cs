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
        SwordCleave,
        ClubRising,
    }

    /// <summary>How a move's path is made.</summary>
    public enum MoveShape
    {
        /// <summary>Points drawn by hand in the aim frame (the first moves; kept for custom ones).</summary>
        Path,

        /// <summary>A swing: the weapon turned about a pivot in the body through a big arc, at full reach (see the Swing fields).</summary>
        Swing,

        /// <summary>A thrust: drawn back, then driven straight through the aim point, point first.</summary>
        Thrust,
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
    public partial class MoveDefinition : ScriptableObject
    {
        [Header("Aim")]
        public StrikeWith strikeWith = StrikeWith.Edge;
        [Tooltip("The body parts this move can be aimed at.")]
        public AimParts targets = AimParts.All;
        [Tooltip("Swing and Thrust: the move is built from the numbers below (a real swing by construction). Path: the hand-drawn path further down.")]
        public MoveShape shape = MoveShape.Path;

        [Header("Swing (medium size; Swing and Thrust shapes)")]
        [Tooltip("Degrees the swing plane tilts: 0 a flat swing, 90 straight down from overhead, negative rising from below.")]
        public float planeTilt = 40f;
        [Tooltip("Swings from the left across to the right (the weapon arm wound across the body).")]
        public bool backhand;
        [Tooltip("Degrees behind the contact the swing is wound up to (180: the weapon straight behind the pivot).")]
        public float windUpAngle = 190f;
        [Tooltip("Degrees past the contact the swing carries on.")]
        public float followAngle = 130f;
        [Tooltip("Degrees behind the contact the guard sits on the arc: the wind-up starts there.")]
        public float guardAngle = 40f;
        [Tooltip("Metres the striking part drives on past the aim point into the target (the arm reaches full stretch just after contact). Thrusts: how far the point is driven past the aim.")]
        public float through = 0.45f;
        [Tooltip("The swing's pivot from the chest: metres toward the weapon side and up.")]
        public float pivotSide = 0.05f;
        public float pivotUp = 0.1f;
        [Tooltip("Metres the pivot sits out in front of the chest (a backhand's arc kept clear of the body as the arm comes across).")]
        public float pivotForward;
        [Range(0.4f, 1f), Tooltip("How far out the weapon is wound up, against full reach.")]
        public float windUpRadius = 0.85f;
        [Tooltip("Degrees after contact the arm reaches full stretch.")]
        public float stretchAfter = 12f;
        [Tooltip("Degrees the weapon trails the hand while it accelerates (it whips through at contact and leads after).")]
        public float wristLag = 35f;
        [Tooltip("Thrusts: how close to the pivot the point is drawn back (metres).")]
        public float drawBack = 0.3f;

        [Header("Swing timing (seconds at medium size; the follow-through follows from the speed)")]
        public float windUpTime = 0.3f;
        [Tooltip("The tense moment at the top of the wind-up.")]
        public float holdTime = 0.06f;
        [Tooltip("From the release to the contact.")]
        public float strikeTime = 0.2f;
        [Range(0.1f, 0.8f), Tooltip("Share of the strike spent accelerating (then flat out through contact).")]
        public float accelShare = 0.4f;
        public float recoveryTime = 0.25f;

        [Header("Swing body")]
        [Tooltip("Degrees the hips and chest turn away in the wind-up (the hips lead the arm, the chest follows).")]
        public float turnBack = 80f;
        [Tooltip("Degrees they turn through past square by the end of the follow-through.")]
        public float turnThrough = 55f;
        [Tooltip("Degrees of lean in the wind-up (negative: arched back) and at contact (forward into the blow).")]
        public float leanBack = -8f;
        public float leanThrough = 25f;
        [Tooltip("Hips lowered (share of the leg) at the loaded pose, and at contact.")]
        public float crouchLoad = 0.08f;
        public float crouchStrike;
        [Tooltip("How far the body rocks back in the wind-up (share of the rock-back, 0 to 1).")]
        public float rockBack = 0.6f;
        [Tooltip("The rear foot steps through to the front (big swings, thrusts) instead of the lead foot stepping in.")]
        public bool passingStep;
        [Tooltip("Metres the body keeps driving forward after contact (thrusts).")]
        public float drive;

        [Header("Timing (Path shape)")]
        [Min(0.1f), Tooltip("Seconds at medium size (bigger units are slower, smaller ones quicker). Swing and Thrust shapes work it out from their timing.")]
        public float duration = 0.7f;
        [Tooltip("How far along the path (0 to 1) the striking part is over the move's time (0 to 1). Steep is fast.")]
        public AnimationCurve speed = SmoothCurves.Through(0f, 0f, 0.4f, 0.32f, 0.58f, 0.6f, 0.72f, 0.82f, 1f, 1f);
        [Tooltip("How much the move takes over from the guard (0 to 1). Must be 1 at contact.")]
        public AnimationCurve blend = SmoothCurves.Through(0f, 0f, 0.15f, 1f, 0.8f, 1f, 1f, 0f);
        [Tooltip("Progress along the path (0 to 1) during which the weapon can strike. Swing and Thrust shapes set it themselves.")]
        public Vector2 activeWindow = new Vector2(0.45f, 0.8f);
        [Min(0f), Tooltip("Seconds the move freezes at a hit (milestone 3).")]
        public float hitStop = 0.06f;

        [Header("Path of the striking part (Path shape; aim frame, medium size)")]
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

        [Header("Body (Path shape: all of it; Swing and Thrust: reach and lunge)")]
        [Tooltip("Path shape: distance from the attacker's chest to the aim point at contact (medium size). Swings work it out from their reach and Through.")]
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
        public float ProgressAt(float t) => shape == MoveShape.Path ? Mathf.Clamp01(speed.Evaluate(t)) : SwingProgress(t);

        public float BlendAt(float t) => shape == MoveShape.Path ? Mathf.Clamp01(blend.Evaluate(t)) : SwingBlend(t);

        public float BodyAt(float t) => shape == MoveShape.Path ? bodyCurve.Evaluate(t) : SwingBody(t);

        public float TwistAt(float progress) => Mathf.Lerp(twistStart, twistEnd, progress);

        public float ContactProgress => shape != MoveShape.Path ? SwingContactProgress
            : path.Length > 1 ? Mathf.Clamp01((float)contactIndex / (path.Length - 1)) : 0f;

        /// <summary>Seconds at medium size.</summary>
        public float Duration => shape == MoveShape.Path ? duration : SwingDuration;

        /// <summary>The move's time (0 to 1) at which the striking part reaches the contact point.</summary>
        public float ContactTime()
        {
            if (shape != MoveShape.Path)
            {
                return SwingContactTime;
            }

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

        /// <summary>
        /// Fills in a ready-made move. Overboard on purpose (milestone 3's swing rework): big arcs at
        /// full stretch, the whole body turning behind them, carried deep through the target.
        /// </summary>
        public void ApplyDefaults(MoveStyle style)
        {
            shape = MoveShape.Swing;
            strikeWith = StrikeWith.Edge;
            targets = AimParts.All;
            backhand = false;
            planeTilt = 40f;
            windUpAngle = 190f;
            followAngle = 130f;
            guardAngle = 40f;
            through = 0.45f;
            pivotSide = 0.05f;
            pivotUp = 0.1f;
            pivotForward = 0f;
            windUpRadius = 0.6f;
            stretchAfter = 35f;
            wristLag = 35f;
            drawBack = 0.3f;
            windUpTime = 0.45f;
            holdTime = 0.09f;
            strikeTime = 0.3f;
            accelShare = 0.4f;
            recoveryTime = 0.45f;
            turnBack = 100f;
            turnThrough = 70f;
            leanBack = -8f;
            leanThrough = 25f;
            crouchLoad = 0.08f;
            crouchStrike = 0f;
            rockBack = 0.6f;
            passingStep = false;
            drive = 0f;
            maxLunge = 1.2f;
            weaponArmPin = 1f;
            hitStop = 0.07f;
            // (The Path shape's fields are left as they were: unused by these.)

            switch (style)
            {
                case MoveStyle.SwordCut:
                    // Forehand: wound up high behind the right shoulder, cut down diagonally across and
                    // through the target, out low on the left.
                    break;

                case MoveStyle.SwordBackhand:
                    // Wound across the body to the left, cut out and through to the right.
                    backhand = true;
                    planeTilt = 25f;
                    // (Not the head: wound up high across the body, the arm crosses its own face.)
                    targets = AimParts.Neck | AimParts.Chest | AimParts.Belly | AimParts.Elbows | AimParts.Knees;
                    pivotSide = -0.05f;
                    pivotForward = 0.18f;   // the arc sits out in front of the body as the arm comes across
                    windUpAngle = 160f;
                    followAngle = 120f;
                    through = 0.6f;
                    strikeTime = 0.28f;
                    turnBack = 90f;
                    turnThrough = 65f;
                    break;

                case MoveStyle.SwordCleave:
                    // Flat and wide at waist to chest height: steps through and sweeps everything in reach.
                    planeTilt = 5f;
                    windUpAngle = 210f;
                    followAngle = 150f;
                    through = 0.5f;
                    windUpTime = 0.51f;
                    holdTime = 0.12f;
                    strikeTime = 0.32f;
                    turnBack = 110f;
                    turnThrough = 80f;
                    leanThrough = 18f;
                    crouchLoad = 0.1f;
                    passingStep = true;
                    targets = AimParts.Chest | AimParts.Belly | AimParts.Elbows;
                    break;

                case MoveStyle.SwordThrust:
                    // Run them through: drawn back by the hip with the body coiled, then the point and the
                    // whole body driven forward together, arm locked, the point 0.6 m past the aim.
                    shape = MoveShape.Thrust;
                    strikeWith = StrikeWith.Tip;
                    targets = AimParts.Body;
                    through = 0.6f;
                    drawBack = 0.3f;
                    pivotSide = 0.2f;
                    pivotUp = 0.12f;
                    windUpTime = 0.48f;
                    holdTime = 0.12f;
                    strikeTime = 0.1f;
                    accelShare = 0.5f;
                    recoveryTime = 0.45f;
                    turnBack = 45f;
                    turnThrough = -15f;
                    leanBack = -6f;
                    leanThrough = 30f;
                    crouchLoad = 0.1f;
                    crouchStrike = 0.08f;
                    passingStep = true;
                    drive = 0.35f;
                    maxLunge = 1.4f;
                    break;

                case MoveStyle.ClubSwing:
                    // The haymaker: wound far back round the right, stepped through, swung flat out
                    // through the target and round to the left.
                    strikeWith = StrikeWith.Head;
                    planeTilt = 10f;
                    windUpAngle = 220f;
                    followAngle = 160f;
                    through = 0.5f;
                    windUpTime = 0.54f;
                    holdTime = 0.12f;
                    strikeTime = 0.36f;
                    recoveryTime = 0.48f;
                    turnBack = 115f;
                    turnThrough = 85f;
                    leanThrough = 20f;
                    crouchLoad = 0.1f;
                    passingStep = true;
                    wristLag = 40f;
                    break;

                case MoveStyle.ClubSmash:
                    // Over the top: raised up behind the back on tiptoe, brought down through the target
                    // toward the ground past it, dropping into a deep crouch.
                    strikeWith = StrikeWith.Head;
                    targets = AimParts.Head | AimParts.Neck | AimParts.Chest;
                    planeTilt = 85f;
                    windUpAngle = 210f;
                    followAngle = 110f;
                    guardAngle = 60f;
                    through = 0.45f;
                    pivotSide = 0.3f;   // the arc comes down past the head, not through it
                    pivotUp = 0.2f;
                    windUpTime = 0.57f;
                    holdTime = 0.15f;
                    strikeTime = 0.34f;
                    recoveryTime = 0.51f;
                    turnBack = 25f;
                    turnThrough = 15f;
                    leanBack = -20f;
                    leanThrough = 40f;
                    crouchLoad = -0.03f;
                    crouchStrike = 0.15f;
                    wristLag = 40f;
                    break;

                case MoveStyle.ClubRising:
                    // From low behind, swung up and through: for knees, belly and chest.
                    strikeWith = StrikeWith.Head;
                    targets = AimParts.Knees | AimParts.Belly | AimParts.Chest;
                    planeTilt = -50f;
                    windUpAngle = 160f;
                    followAngle = 140f;
                    guardAngle = 30f;
                    through = 0.4f;
                    windUpTime = 0.48f;
                    holdTime = 0.09f;
                    strikeTime = 0.22f;
                    recoveryTime = 0.45f;
                    turnBack = 85f;
                    turnThrough = 60f;
                    leanBack = 15f;
                    leanThrough = -10f;
                    crouchLoad = 0.14f;
                    crouchStrike = 0f;
                    wristLag = 40f;
                    break;
            }

            Bake();
        }

        private void OnValidate() => Bake();

        /// <summary>Swing and Thrust shapes: the duration and active window follow from the swing's numbers.</summary>
        public void Bake()
        {
            if (shape == MoveShape.Path)
            {
                return;
            }

            duration = SwingDuration;
            float start = SwingTravel(0) + SwingTravel(1) + 0.2f * SwingTravel(2);
            activeWindow = new Vector2(start / SwingTotal, 1f);
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
