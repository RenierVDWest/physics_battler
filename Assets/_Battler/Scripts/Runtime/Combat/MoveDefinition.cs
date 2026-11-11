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

        /// <summary>Milestone 10: the club's backhand, the return of a hook.</summary>
        ClubBackhand,

        /// <summary>Milestone 10: the fan strike (abanico), a short wrist-led arc, forehand and backhand.</summary>
        ClubFan,
        ClubFanBackhand,

        /// <summary>Milestone 10: the lead hand's jab, a bare fist straight out and back.</summary>
        OffJab,

        /// <summary>Milestone 10: the shield bash, the shield's face driven into the target and back.</summary>
        ShieldBash,

        /// <summary>Milestone 10: the club's butt strike (punyo), hammered down into the face or the collarbone from close in.</summary>
        ClubPunyo,
    }

    /// <summary>Milestone 10: which hand throws a blow.</summary>
    public enum MoveHand
    {
        /// <summary>The weapon hand (the right).</summary>
        Main,

        /// <summary>The other hand (the left): a bare fist's jab, a shield's bash.</summary>
        Off,
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
        [Tooltip("The bodies (fighting styles) that throw this blow: the fan strikes are the small body's eskrima, not the large one's.")]
        public ComboBodies bodies = ComboBodies.All;
        [Tooltip("Milestone 10: the hand that throws it. Off: the left hand's own blow (the jab, the shield bash); the weapon arm keeps its guard.")]
        public MoveHand hand = MoveHand.Main;

        /// <summary>
        /// Milestone 10: a close blow, its striking part going straight from the hand's guard to the
        /// aim point and back (OffHandPath; posed on its own, not by keys): the off hand's blows, and
        /// a butt strike.
        /// </summary>
        public bool Close => hand == MoveHand.Off || strikeWith == StrikeWith.Butt;

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

        [Header("Elbow null (degrees round the arm: 0 hanging relaxed, +90 out to the side, 180 up and over, -90 in across)")]
        [Tooltip("At the end of the wind-up (held through the hold). The guard's is at the start and the end of the blow.")]
        public float elbowWindUp;
        [Tooltip("At the striking moment.")]
        public float elbowContact;
        [Tooltip("At the end of the follow-through; the recovery eases back to the guard's.")]
        public float elbowFollow;

        [Header("Hand keys (where the hand is, from the shoulder, in the blow's aim frame: x right, y up, z toward the target; metres, medium size)")]
        [Tooltip("At the end of the wind-up (held through the hold). The contact hand is solved when the blow is planned, so the striking part lands on the aim.")]
        public Vector3 handWindUp;
        [Tooltip("At the end of the follow-through.")]
        public Vector3 handFollow;

        [Header("Wrist keys (x: the forearm's turn, palm down +; y: flexion toward the palm +; z: toward the thumb +; degrees)")]
        [Tooltip("At the end of the wind-up (held through the hold). The guard's wrist is at the start and the end of the blow.")]
        public Vector3 wristWindUp;
        [Tooltip("At the striking moment.")]
        public Vector3 wristContact;
        [Tooltip("At the end of the follow-through.")]
        public Vector3 wristFollow;

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

        /// <summary>The bodies a ready-made move is for (milestone 10: the fan strikes are the small body's).</summary>
        public static ComboBodies DefaultBodies(MoveStyle style) =>
            style == MoveStyle.ClubFan || style == MoveStyle.ClubFanBackhand ? ComboBodies.Small
            : style == MoveStyle.OffJab ? ComboBodies.Large
            : style == MoveStyle.ClubPunyo ? ComboBodies.Small : ComboBodies.All;

        /// <summary>
        /// The arm rebuild's starting elbow nulls for the ready-made moves (degrees round the arm, as
        /// <see cref="PhysicsBattler.Motion.ArmGoal.elbowSwivel"/>): a first pass from technique, to be
        /// keyed by eye on the arm bench. Forehand swings wind up with the elbow out and high and come
        /// through with it lower; overhead blows keep it forward under the weapon; backhands wind up
        /// with it in across the chest and open it out; thrusts keep it low behind the blade; fans
        /// keep it still, the wrist doing the work.
        /// </summary>
        public static void DefaultElbows(MoveStyle style, out float windUp, out float contact, out float follow)
        {
            switch (style)
            {
                // (The arm rebuild: posed as designed (keyfit's seeds: where the hand, the elbow and the
                // blade are at the wind-up and follow-through), then fitted near that by
                // Tools/CompileCheck's keyfit for the arc, landing, clearance, wrist limits and speed
                // through contact, and looked at in armfilm's stick figures.)
                case MoveStyle.SwordCut:
                    windUp = 120f; contact = 84f; follow = 66f;
                    break;
                case MoveStyle.SwordBackhand:
                    windUp = -22f; contact = 66f; follow = 123f;
                    break;
                case MoveStyle.ClubSwing:
                    windUp = 120f; contact = 133f; follow = 38f;
                    break;
                case MoveStyle.ClubSmash:
                    windUp = 128f; contact = -17f; follow = 126f;
                    break;
                case MoveStyle.SwordCleave:
                    windUp = -25f; contact = 53f; follow = 130f;
                    break;
                case MoveStyle.ClubRising:
                    windUp = -41f; contact = 36f; follow = -11f;
                    break;
                case MoveStyle.ClubBackhand:
                    windUp = -28f; contact = 81f; follow = 130f;
                    break;
                case MoveStyle.SwordThrust:
                    windUp = 0f; contact = 10f; follow = 10f;
                    break;
                case MoveStyle.ClubFan:
                    windUp = 30f; contact = 20f; follow = 20f;
                    break;
                case MoveStyle.ClubFanBackhand:
                    windUp = -20f; contact = 20f; follow = 40f;
                    break;
                default:
                    windUp = contact = follow = 20f;
                    break;
            }
        }

        /// <summary>
        /// The arm rebuild's starting wrist keys (forearm turn, flexion, deviation toward the thumb): a
        /// first pass from technique, keyed by eye later. A swing is wound up with the wrist cocked
        /// back and toward the thumb, comes through it straight and rolls over after; a backhand winds
        /// up with the palm down and the wrist bent in; a thrust keeps the blade level and drives the
        /// point up a little; a fan is nearly all wrist.
        /// </summary>
        public static void DefaultWrists(MoveStyle style, out Vector3 windUp, out Vector3 contact, out Vector3 follow)
        {
            switch (style)
            {
                // (The arm rebuild: posed as designed (keyfit's seeds: where the hand, the elbow and the
                // blade are at the wind-up and follow-through), then fitted near that by
                // Tools/CompileCheck's keyfit for the arc, landing, clearance, wrist limits and speed
                // through contact, and looked at in armfilm's stick figures.)
                case MoveStyle.SwordCut:
                    windUp = new Vector3(45f, 12f, -29f);
                    contact = new Vector3(77f, -10f, -17f);
                    follow = new Vector3(36f, 41f, -12f);
                    break;
                case MoveStyle.SwordBackhand:
                    windUp = new Vector3(40f, 21f, -12f);
                    contact = new Vector3(59f, 1f, -2f);
                    follow = new Vector3(73f, -22f, -17f);
                    break;
                case MoveStyle.ClubSwing:
                    windUp = new Vector3(70f, 15f, -16f);
                    contact = new Vector3(-64f, 30f, -7f);
                    follow = new Vector3(72f, 27f, -7f);
                    break;
                case MoveStyle.ClubSmash:
                    windUp = new Vector3(45f, 40f, -12f);
                    contact = new Vector3(11f, -6f, -25f);
                    follow = new Vector3(7f, 43f, -13f);
                    break;
                case MoveStyle.SwordCleave:
                    windUp = new Vector3(-55f, -60f, 0f);
                    contact = new Vector3(21f, 4f, -12f);
                    follow = new Vector3(-9f, 23f, 1f);
                    break;
                case MoveStyle.ClubRising:
                    windUp = new Vector3(-50f, 28f, -27f);
                    contact = new Vector3(14f, 15f, -3f);
                    follow = new Vector3(-22f, 13f, -15f);
                    break;
                case MoveStyle.ClubBackhand:
                    windUp = new Vector3(50f, 33f, 1f);
                    contact = new Vector3(64f, -27f, -2f);
                    follow = new Vector3(70f, -18f, -26f);
                    break;
                case MoveStyle.SwordThrust:
                    // (Driven home in line: the wrist turned toward the little finger, the blade along
                    // the forearm.)
                    windUp = new Vector3(0f, 0f, -25f);
                    contact = new Vector3(0f, 0f, -30f);
                    follow = new Vector3(0f, 0f, -25f);
                    break;
                case MoveStyle.ClubFan:
                    windUp = new Vector3(0f, -45f, 15f);
                    contact = new Vector3(0f, 0f, -5f);
                    follow = new Vector3(0f, 25f, -20f);
                    break;
                case MoveStyle.ClubFanBackhand:
                    windUp = new Vector3(40f, 40f, 0f);
                    contact = new Vector3(30f, 0f, -5f);
                    follow = new Vector3(20f, -30f, 10f);
                    break;
                default:
                    windUp = contact = follow = Vector3.zero;
                    break;
            }
        }

        /// <summary>
        /// The arm rebuild's starting hand keys (hand-first blows: the hand is the arm's effector, the
        /// weapon follows it): a forehand winds up back and out to the side and follows through low
        /// across the body; an overhead blow winds up above and behind the head and follows through
        /// low in front; a rising blow from low behind the hip to high across; a backhand from the
        /// other shoulder out to the side; a thrust drawn back by the chest; a fan close to the
        /// shoulder, little arm and all wrist. In the aim frame, from the shoulder, medium size.
        /// </summary>
        public static void DefaultHands(MoveStyle style, out Vector3 windUp, out Vector3 follow)
        {
            switch (style)
            {
                // (The arm rebuild: posed as designed (keyfit's seeds: where the hand, the elbow and the
                // blade are at the wind-up and follow-through), then fitted near that by
                // Tools/CompileCheck's keyfit for the arc, landing, clearance, wrist limits and speed
                // through contact, and looked at in armfilm's stick figures.)
                case MoveStyle.SwordCut:
                    windUp = new Vector3(0.3f, 0.32f, -0.31f);
                    follow = new Vector3(-0.27f, -0.38f, 0.24f);
                    break;
                case MoveStyle.SwordBackhand:
                    windUp = new Vector3(-0.23f, 0.08f, 0.22f);
                    follow = new Vector3(0.55f, 0.03f, -0.02f);
                    break;
                case MoveStyle.ClubSwing:
                    windUp = new Vector3(0.26f, 0.27f, -0.38f);
                    follow = new Vector3(-0.41f, -0.27f, 0.23f);
                    break;
                case MoveStyle.ClubSmash:
                    windUp = new Vector3(0.24f, 0.39f, -0.08f);
                    follow = new Vector3(-0.22f, -0.45f, 0.23f);
                    break;
                case MoveStyle.SwordCleave:
                    windUp = new Vector3(0.08f, 0.32f, -0.08f);
                    follow = new Vector3(-0.18f, -0.48f, 0.18f);
                    break;
                case MoveStyle.ClubRising:
                    windUp = new Vector3(0.26f, -0.46f, -0.18f);
                    follow = new Vector3(-0.18f, 0.34f, 0.16f);
                    break;
                case MoveStyle.ClubBackhand:
                    windUp = new Vector3(-0.28f, 0.07f, 0.21f);
                    follow = new Vector3(0.54f, 0.1f, 0.07f);
                    break;
                case MoveStyle.SwordThrust:
                    // (Chambered by the hip, the forearm and the point toward the target, then driven
                    // straight out.)
                    windUp = new Vector3(0.05f, -0.28f, 0.12f);
                    follow = new Vector3(-0.05f, 0.05f, 0.6f);
                    break;
                case MoveStyle.ClubFan:
                    windUp = new Vector3(0.25f, 0.1f, 0.15f);
                    follow = new Vector3(-0.05f, -0.05f, 0.3f);
                    break;
                case MoveStyle.ClubFanBackhand:
                    windUp = new Vector3(-0.05f, 0.1f, 0.2f);
                    follow = new Vector3(0.3f, 0f, 0.25f);
                    break;
                default:
                    windUp = new Vector3(0.3f, 0f, 0f);
                    follow = new Vector3(-0.2f, -0.2f, 0.3f);
                    break;
            }
        }

        /// <summary>
        /// As <see cref="Phase"/>, for the hand's travel: from the hold to the striking moment and on
        /// through the follow-through it goes as the swing's own progress does (accelerating into the
        /// blow, slowing after), elsewhere eased.
        /// </summary>
        public void HandPhase(float t, out int from, out int to, out float share)
        {
            Phase(t, out from, out to, out share);
            if (shape == MoveShape.Path || (from != 1 || to != 2) && (from != 2 || to != 3))
            {
                return;
            }

            float t1 = WindUpEnd, t3 = ContactTime(), t4 = SwingFollowEnd;
            float t2 = Mathf.Clamp(t1 + Mathf.Max(0f, holdTime) / Mathf.Max(1e-3f, SwingDuration), t1, t3);
            float a = from == 1 ? ProgressAt(t2) : ProgressAt(t3), b = from == 1 ? ProgressAt(t3) : ProgressAt(t4);
            share = b - a > 1e-5f ? Mathf.Clamp01((ProgressAt(t) - a) / (b - a)) : share;
        }

        /// <summary>
        /// Where a normalized time of the blow falls among its keys (0 the guard, 1 the wind-up, 2 the
        /// contact, 3 the follow-through): between key <paramref name="from"/> and <paramref name="to"/>,
        /// eased (no corner at a key).
        /// </summary>
        public void Phase(float t, out int from, out int to, out float share)
        {
            float t1 = WindUpEnd, t3 = ContactTime(), t4 = SwingFollowEnd;
            float t2 = Mathf.Clamp(shape == MoveShape.Path ? t1 : t1 + Mathf.Max(0f, holdTime) / Mathf.Max(1e-3f, SwingDuration), t1, t3);
            float s;
            if (t <= t1) { from = 0; to = 1; s = t / Mathf.Max(1e-4f, t1); }
            else if (t <= t2) { from = 1; to = 1; s = 0f; }
            else if (t <= t3) { from = 1; to = 2; s = (t - t2) / Mathf.Max(1e-4f, t3 - t2); }
            else if (t <= t4) { from = 2; to = 3; s = (t - t3) / Mathf.Max(1e-4f, t4 - t3); }
            else { from = 3; to = 0; s = (t - t4) / Mathf.Max(1e-4f, 1f - t4); }
            s = Mathf.Clamp01(s);
            share = s * s * (3f - 2f * s);
        }

        /// <summary>A wrist key by its number (1 the wind-up, 2 the contact, 3 the follow-through).</summary>
        public Vector3 WristKey(int key) => key == 1 ? wristWindUp : key == 2 ? wristContact : wristFollow;

        /// <summary>The elbow null (degrees round the arm) at a normalized time of the blow: the guard's at its start and end, the keys between, eased.</summary>
        public float ElbowSwivel(float t, float guard)
        {
            float Key(int k) => k == 1 ? elbowWindUp : k == 2 ? elbowContact : k == 3 ? elbowFollow : guard;
            HandPhase(t, out int from, out int to, out float share);
            return Through(from, to, share, k => new Vector3(Key(k), 0f, 0f)).x;
        }

        /// <summary>
        /// The progress along the blow at its strike's start (the wind-up's key, after the hold), its
        /// striking moment (the contact's key) and its follow-through's end (that key): the knots of the
        /// keys' spline through the strike.
        /// </summary>
        public void KeyProgress(out float windUp, out float contact, out float follow)
        {
            float t1 = WindUpEnd, t3 = ContactTime(), t4 = SwingFollowEnd;
            float t2 = Mathf.Clamp(t1 + Mathf.Max(0f, holdTime) / Mathf.Max(1e-3f, SwingDuration), t1, t3);
            windUp = ProgressAt(t2);
            contact = Mathf.Max(windUp + 1e-4f, ProgressAt(t3));
            follow = Mathf.Max(contact + 1e-4f, ProgressAt(t4));
        }

        /// <summary>
        /// Between two keys at a share of the way: through the striking moment (wind-up to contact,
        /// contact to follow-through) on a spline with its knots at the keys' progress, so what turns
        /// into the contact carries on turning out of it at the same rate, with no corner and no change
        /// of pace (the wind-up's key starts from its hold and the follow-through's ends at its stop, as
        /// the blow's own pace does); between the others, straight.
        /// </summary>
        public Vector3 Through(int from, int to, float share, System.Func<int, Vector3> key)
        {
            if (from == 1 && to == 2 || from == 2 && to == 3)
            {
                KeyProgress(out float p1, out float p2, out float p3);
                Vector3 k1 = key(1), k2 = key(2), k3 = key(3);
                // (The rate through the contact: the keys either side over their span of progress.)
                Vector3 m2 = (k3 - k1) / (p3 - p1);
                Vector3 m1 = (k2 - k1) / (p2 - p1), m3 = (k3 - k2) / (p3 - p2);
                Vector3 a = from == 1 ? k1 : k2, b = from == 1 ? k2 : k3;
                Vector3 ma = from == 1 ? m1 : m2, mb = from == 1 ? m2 : m3;
                float span = from == 1 ? p2 - p1 : p3 - p2;
                float t = share, t2 = t * t, t3 = t2 * t;
                return (2f * t3 - 3f * t2 + 1f) * a + (t3 - 2f * t2 + t) * span * ma + (-2f * t3 + 3f * t2) * b + (t3 - t2) * span * mb;
            }

            return Vector3.Lerp(key(from), key(to), share);
        }

        /// <summary>Whether a unit of this size throws this blow (none listed: every body does).</summary>
        public bool UsableBy(float sizeFactor) => bodies == ComboBodies.None || (bodies & ComboDefinition.BodyOf(sizeFactor)) != 0;

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
            bodies = DefaultBodies(style);
            hand = MoveHand.Main;
            DefaultElbows(style, out elbowWindUp, out elbowContact, out elbowFollow);
            DefaultWrists(style, out wristWindUp, out wristContact, out wristFollow);
            DefaultHands(style, out handWindUp, out handFollow);
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
            windUpTime = 0.59f;
            holdTime = 0.09f;
            // (The arm rebuild: the strikes 30% quicker and the wind-ups 30% slower than the path-made
            // ones. A hand keyed as a person swings carries the striking part a shorter way through the
            // strike than those paths did and a longer one into the wind-up, and at their pace it came
            // through contact no faster than it wound up.)
            strikeTime = 0.21f;
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
                    strikeTime = 0.2f;
                    turnBack = 90f;
                    turnThrough = 65f;
                    break;

                case MoveStyle.SwordCleave:
                    // Flat and wide at waist to chest height: steps through and sweeps everything in reach.
                    planeTilt = 5f;
                    windUpAngle = 210f;
                    followAngle = 150f;
                    through = 0.5f;
                    windUpTime = 0.66f;
                    holdTime = 0.12f;
                    strikeTime = 0.22f;
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
                    windUpTime = 0.7f;
                    holdTime = 0.12f;
                    strikeTime = 0.25f;
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
                    windUpTime = 0.74f;
                    holdTime = 0.15f;
                    strikeTime = 0.24f;
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
                    windUpTime = 0.62f;
                    holdTime = 0.09f;
                    strikeTime = 0.17f;
                    recoveryTime = 0.45f;
                    turnBack = 85f;
                    turnThrough = 60f;
                    leanBack = 15f;
                    leanThrough = -10f;
                    crouchLoad = 0.14f;
                    crouchStrike = 0f;
                    wristLag = 40f;
                    break;

                case MoveStyle.ClubBackhand:
                    // Milestone 10: the hook's return. Wound across the body to the left, swung out flat
                    // through the target and round to the right.
                    strikeWith = StrikeWith.Head;
                    backhand = true;
                    targets = AimParts.Neck | AimParts.Chest | AimParts.Belly | AimParts.Elbows | AimParts.Knees;
                    planeTilt = 20f;
                    windUpAngle = 170f;
                    followAngle = 120f;
                    through = 0.5f;
                    pivotSide = -0.05f;
                    pivotForward = 0.18f;
                    windUpTime = 0.59f;
                    holdTime = 0.08f;
                    strikeTime = 0.21f;
                    recoveryTime = 0.45f;
                    turnBack = 90f;
                    turnThrough = 65f;
                    leanThrough = 15f;
                    wristLag = 40f;
                    break;

                case MoveStyle.ClubFan:
                case MoveStyle.ClubFanBackhand:
                    // Milestone 10: the fan strike (abanico). A short arc led by the wrist, from close
                    // by the shoulder: little wind-up, little body, quick; strung side to side in a
                    // flurry, at the head and the hands.
                    strikeWith = StrikeWith.Head;
                    backhand = style == MoveStyle.ClubFanBackhand;
                    targets = AimParts.Head | AimParts.Neck | AimParts.Elbows;
                    planeTilt = backhand ? 20f : 35f;
                    windUpAngle = 100f;
                    followAngle = 60f;
                    guardAngle = 30f;
                    through = 0.25f;
                    pivotSide = backhand ? -0.05f : 0.05f;
                    pivotForward = backhand ? 0.1f : 0.05f;
                    windUpRadius = 0.45f;
                    stretchAfter = 20f;
                    wristLag = 50f;
                    drawBack = 0.15f;
                    windUpTime = 0.22f;
                    holdTime = 0.04f;
                    strikeTime = 0.2f;   // (the arm keeps up with it: at 0.15 s the backhand's strike outran the arm)
                    recoveryTime = 0.3f;
                    turnBack = 30f;
                    turnThrough = 25f;
                    leanBack = -3f;
                    leanThrough = 8f;
                    crouchLoad = 0.04f;
                    rockBack = 0.25f;
                    maxLunge = 0.5f;
                    hitStop = 0.04f;
                    break;

                case MoveStyle.OffJab:
                    // Milestone 10: the boxer's jab. The lead fist snaps straight out from its guard to
                    // the target and straight back, the shoulder turned in behind it (the body turns
                    // right, the lead shoulder forward), a half step in; quick, light, the club kept up.
                    // (The fist's way is the off hand's own, OffHandPath; the swing's numbers below give
                    // the timing and the body.)
                    hand = MoveHand.Off;
                    shape = MoveShape.Thrust;
                    strikeWith = StrikeWith.Fist;
                    targets = AimParts.Head | AimParts.Neck | AimParts.Chest;
                    through = 0.08f;
                    drawBack = 0.05f;
                    pivotSide = -0.2f;
                    pivotUp = 0.12f;
                    windUpTime = 0.1f;
                    holdTime = 0.02f;
                    strikeTime = 0.12f;
                    accelShare = 0.5f;
                    recoveryTime = 0.22f;
                    turnBack = -6f;
                    turnThrough = -22f;
                    leanBack = 0f;
                    leanThrough = 8f;
                    crouchLoad = 0.03f;
                    crouchStrike = 0.02f;
                    rockBack = 0.15f;
                    drive = 0.15f;
                    maxLunge = 0.35f;
                    hitStop = 0.04f;
                    elbowWindUp = elbowContact = elbowFollow = 0f;
                    break;

                case MoveStyle.ClubPunyo:
                    // Milestone 10: the eskrima butt strike (punyo). From close in, the fist raised by
                    // the shoulder, the stick standing up behind it, then hammered down and forward so
                    // the butt below the fist lands on the face, the neck or the collarbone, and on
                    // through a little; the body drops into it. (A close blow: its way is straight, from
                    // the raised fist, OffHandPath's for the weapon hand.)
                    hand = MoveHand.Main;
                    shape = MoveShape.Thrust;
                    strikeWith = StrikeWith.Butt;
                    targets = AimParts.Head | AimParts.Neck | AimParts.Chest;
                    through = 0.15f;
                    drawBack = 0.22f;   // (here: how far the fist is raised over its guard, in the chamber)
                    pivotSide = 0.2f;
                    pivotUp = 0.12f;
                    windUpTime = 0.16f;
                    holdTime = 0.03f;
                    strikeTime = 0.12f;
                    accelShare = 0.5f;
                    recoveryTime = 0.28f;
                    turnBack = 15f;
                    turnThrough = 10f;
                    leanBack = -2f;
                    leanThrough = 12f;
                    crouchLoad = 0.03f;
                    crouchStrike = 0.06f;
                    rockBack = 0.2f;
                    drive = 0.2f;
                    passingStep = false;
                    maxLunge = 0.35f;
                    hitStop = 0.05f;
                    elbowWindUp = elbowContact = elbowFollow = 0f;
                    break;

                case MoveStyle.ShieldBash:
                    // Milestone 10: the shield bash. The shield drawn in a little, then driven straight
                    // out at the target's chest or head, face first, the body stepping in behind it and
                    // the lead shoulder turning in; a shove more than a blow, to break a guard or a
                    // stance. (The shield's way is the off hand's own, OffHandPath, as the jab's.)
                    hand = MoveHand.Off;
                    shape = MoveShape.Thrust;
                    strikeWith = StrikeWith.Face;
                    targets = AimParts.Head | AimParts.Chest | AimParts.Belly;
                    through = 0.12f;
                    drawBack = 0.08f;
                    pivotSide = -0.2f;
                    pivotUp = 0.1f;
                    windUpTime = 0.18f;
                    holdTime = 0.04f;
                    strikeTime = 0.14f;
                    accelShare = 0.5f;
                    recoveryTime = 0.3f;
                    turnBack = -8f;
                    turnThrough = -25f;
                    leanBack = -3f;
                    leanThrough = 14f;
                    crouchLoad = 0.06f;
                    crouchStrike = 0.03f;
                    rockBack = 0.3f;
                    drive = 0.3f;
                    passingStep = false;
                    maxLunge = 0.45f;
                    hitStop = 0.06f;
                    elbowWindUp = elbowContact = elbowFollow = 0f;
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
