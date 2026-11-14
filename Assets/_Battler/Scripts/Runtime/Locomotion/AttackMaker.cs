using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// How a fighter makes an attack of a score (the plan "Aimed attacks"): how far each part of the body goes in each
    /// phase of the blow, and when. 1 is the recording's own; a time of 0 is the recording's own. What a fighting
    /// style is made of.
    ///
    /// <para>The phases: the <b>wind-up</b> (from the guard to the moment the hips start in), the <b>drive</b> (from
    /// there to the blade's landing: legs, hips, chest, arm, each a little after the one before), the
    /// <b>carry-through</b> (the blade slowing again past the landing) and the <b>way back</b> to the guard.</para>
    /// </summary>
    public struct AttackStyle
    {
        /// <summary>How far the body is wound up before the drive: the hips' turn and the chest's twist away from the blow, times this.</summary>
        public float WindUp;

        /// <summary>How far the sword arm travels from where it was held in the guard, in the wind-up (drawn back) and the carry-through, times this. At the landing itself it is the recording's.</summary>
        public float ArmSwing;

        /// <summary>How far the hips turn through the blow, times this (over the whole blow). The chest faces the world as it did: the spine twists that much less, or more.</summary>
        public float HipDrive;

        /// <summary>How far the chest twists on the hips, times this.</summary>
        public float Twist;

        /// <summary>How far the chest leans, forward and to the side, times this.</summary>
        public float Lean;

        /// <summary>How far the body travels over the ground, its hips and its steps with it, times this. (An aimed attack sets its own travel on top, to reach.)</summary>
        public float Lunge;

        /// <summary>How far the hips sink or rise through the blow, times this.</summary>
        public float Drop;

        /// <summary>How much lower the hips are carried through the whole attack (leg lengths; 0: as recorded): the knees bend for it.</summary>
        public float Crouch;

        /// <summary>How far the hips and the chest carry on after the blade has landed, times this.</summary>
        public float FollowThrough;

        /// <summary>How long the wind-up takes (from the start to the drive), the drive and the carry-through (from the hips' start to the blade slow again), and the way back (from there to the end), times this.</summary>
        public float WindUpTime, DriveTime, RecoveryTime;

        /// <summary>How much sooner the legs and hips go in the drive than the recording has them (seconds; under 0: later), and how much later the sword arm.</summary>
        public float HipLead, ArmLag;

        /// <summary>The recording's own.</summary>
        public static AttackStyle Plain => new AttackStyle { WindUp = 1f, ArmSwing = 1f, HipDrive = 1f, Twist = 1f, Lean = 1f, Lunge = 1f, Drop = 1f, FollowThrough = 1f, WindUpTime = 1f, DriveTime = 1f, RecoveryTime = 1f };

        /// <summary>
        /// Styles to start from (the aim test's buttons): the same blow made as different fighters would throw it.
        /// Heavy: wound far back and slowly, thrown from the hips, carried all the way through. Quick: little wind-up,
        /// a short fast blow from the guard, back at once. Brawler: big arm swings on a quiet body. Low: crouched, deep
        /// in the knees, leaning in. Whip: the hips well ahead and the arm late.
        /// </summary>
        public static readonly (string name, AttackStyle style)[] Presets =
        {
            ("Plain", Plain),
            ("Heavy", new AttackStyle { WindUp = 1.6f, ArmSwing = 1.25f, HipDrive = 1.2f, Twist = 1f, Lean = 1.2f, Lunge = 1.2f, Drop = 1.5f, FollowThrough = 1.4f, WindUpTime = 1.5f, DriveTime = 1.2f, RecoveryTime = 1.4f, ArmLag = 0.02f }),
            ("Quick", new AttackStyle { WindUp = 0.5f, ArmSwing = 0.75f, HipDrive = 0.8f, Twist = 0.9f, Lean = 0.8f, Lunge = 0.9f, Drop = 0.7f, FollowThrough = 0.5f, WindUpTime = 0.65f, DriveTime = 0.85f, RecoveryTime = 0.7f }),
            ("Brawler", new AttackStyle { WindUp = 0.8f, ArmSwing = 1.2f, HipDrive = 0.6f, Twist = 0.7f, Lean = 1.3f, Lunge = 1f, Drop = 0.8f, FollowThrough = 1.3f, WindUpTime = 1.1f, DriveTime = 1f, RecoveryTime = 1.2f, HipLead = -0.03f }),
            ("Low", new AttackStyle { WindUp = 1f, ArmSwing = 1f, HipDrive = 1.1f, Twist = 1f, Lean = 1.5f, Lunge = 1.3f, Drop = 1.6f, Crouch = 0.12f, FollowThrough = 1f, WindUpTime = 1f, DriveTime = 1f, RecoveryTime = 1f }),
            ("Whip", new AttackStyle { WindUp = 1.2f, ArmSwing = 1.1f, HipDrive = 1.3f, Twist = 1f, Lean = 1f, Lunge = 1f, Drop = 1f, FollowThrough = 1.2f, WindUpTime = 0.9f, DriveTime = 0.9f, RecoveryTime = 1f, HipLead = 0.04f, ArmLag = 0.04f }),
        };
    }

    /// <summary>What an attack is made for: where the blade is to land and when.</summary>
    public struct AttackAim
    {
        /// <summary>It is aimed (off: the score as it is, its own contact).</summary>
        public bool Aimed;

        /// <summary>Where the blade's striking part is to land, in the fighter's own frame as the attack begins (leg lengths; x to its right, y over the ground, z forward).</summary>
        public Vector3 Target;

        /// <summary>The blade is to land this long after the attack begins (seconds; 0: as the score has it).</summary>
        public float LandIn;
    }

    /// <summary>An attack made of a score: the clip to play, and how it was aimed.</summary>
    public sealed class MadeAttack
    {
        public AttackScore Score;
        public LocoClip Clip;
        public SwordHold Hold;
        public AttackAim Aim;

        /// <summary>When the blade lands (seconds into the clip), and the moment of the score that is (seconds into the recording).</summary>
        public float ContactAt, ScoreTime;

        /// <summary>
        /// How much later than its clip has it the blade is expected to land (seconds): the walk slows a strike's clip
        /// where its blade would go faster than a sword can be swung (<see cref="LocoBody.BladePace"/>), and the recorded
        /// cuts whip theirs through at twice that. Allowed for when an attack is asked to land at a set moment.
        /// </summary>
        public float Held;

        /// <summary>When the blade is expected to land, counted from the attack's start (seconds): its clip's landing and the hold.</summary>
        public float LandsIn => ContactAt + Held;

        /// <summary>How far the body is turned over the wind-up to bring the blade onto its target (degrees, + to its right), and what its travel was multiplied by.</summary>
        public float Turn, Lunge;

        /// <summary>How far the hips were lowered (leg lengths; under 0: raised) and the chest leaned forward (degrees; under 0: back) to bring the blade to the target's height.</summary>
        public float Drop, Lean;

        /// <summary>How far the wrist turns the blade onto the target as it lands (degrees).</summary>
        public float Steer;

        /// <summary>How far the sword hand was moved at contact to put the blade on the spot (leg lengths), and how far short of the spot the blade still is, as made (leg lengths; 0: on it).</summary>
        public float HandMoved, Short;

        /// <summary>What it cost to bring this score's blade onto the target (no unit: the less, the better the score fits the target; <see cref="AttackMaker"/>).</summary>
        public float Cost;

        /// <summary>Where the blade's striking part is at contact, as made (the fighter's own frame as the attack begins, leg lengths).</summary>
        public Vector3 Lands;

        /// <summary>What the blade is expected to meet first on its way, in words ("" when not looked at), and whether that is what it was made for.</summary>
        public string Meets = "";
        public bool WayOpen = true;

        // (The plan the clip is made from.)
        internal AttackStyle Style;
        internal AttackMaker.Fit Plan;
        internal Vector3 HandMove;

        private readonly Vector3[] pose = new Vector3[LocoClip.BoneCount];

        /// <summary>
        /// The sword at a time of the made clip (seconds), in the fighter's own frame as the attack begins (leg lengths):
        /// its grip and its tip, <paramref name="length"/> along. As the pose will have it: the fighter's own hold, the
        /// wrist's steer onto the target about the landing.
        /// </summary>
        public void Blade(float time, float length, out Vector3 grip, out Vector3 tip)
        {
            time = Mathf.Clamp(time, 0f, Clip.Duration);
            Clip.Pose(time, pose, 1f);
            Clip.RootMotion(0f, time, out Vector2 moved, out float turned);
            Vector3 shoulder = pose[(int)LocoBone.RightArm];
            Vector3 wrist = shoulder + (pose[(int)LocoBone.RightHand] - shoulder) * Hold.ArmScale;
            Quaternion hand = BodyMeasures.HandFrame(pose, 1);
            Vector3 local = wrist + hand * Hold.Grip;
            Vector3 way = hand * Hold.Blade;
            var turn = Quaternion.Euler(0f, turned, 0f);
            Vector3 at = turn * new Vector3(local.x, 0f, local.z) + new Vector3(moved.x, 0f, moved.y);
            grip = new Vector3(at.x, local.y, at.z);
            way = turn * way;
            if (Aim.Aimed)
            {
                // (The wrist's steer, as the pose has it: a cut's over its last 0.15 s, a thrust's from 0.4 of its wind-up; let go after.)
                float c = ContactAt, from = Score.Thrust ? 0.4f * c : Mathf.Max(0f, c - 0.15f);
                float v = time <= c ? Mathf.Clamp01((time - from) / Mathf.Max(0.05f, c - from)) : 1f - Mathf.Clamp01((time - c) / 0.25f);
                way = AttackMaker.Steered(way, Aim.Target - grip, (Score.Thrust ? Hold.MostThrustSteer : Hold.MostSteer) * v * v * (3f - 2f * v), out _);
            }

            tip = grip + way * length;
        }
    }

    /// <summary>
    /// Makes an attack of a score (<see cref="AttackScore"/>): each body part's recorded track is taken, changed by the
    /// style and the aim, and the body put together again, frame by frame, as a clip the walk plays like any strike.
    ///
    /// <para><b>The style</b> (<see cref="AttackStyle"/>) changes how far each part goes and when: the wind-up's size, the
    /// sword arm's swing, the hips' drive, the chest's twist and lean, the lunge and the drop, the carry-through; how
    /// long the wind-up, the drive and the way back take; how much the hips lead and the arm trails.</para>
    /// <para><b>The aim.</b> A cut's blade crosses a range of heights on its way through. Every moment of the cut at
    /// which the blade is still fast is tried as the moment to land. For each: the hips are lowered and the chest
    /// leaned so the blade is at the target's height then; the body's travel is stretched or shortened so it is at
    /// the target's distance; the body is turned over the wind-up so it is on the target's line; the wrist steers the
    /// blade the last of the way, as far as a wrist turns; and what is still left, the sword hand makes up, the arm
    /// solved for it. Each of these costs something, and the moment that costs least is taken: the cut is landed
    /// where in its arc it fits the target best.</para>
    /// <para><b>The moment.</b> The wind-up is stretched or shortened so the blade lands when it is asked to; the drive
    /// itself, from the hips' start to the landing, is the body's own (times the style's).</para>
    /// </summary>
    public static class AttackMaker
    {
        /// <summary>The aim test's attacks are made of the recorded strikes' scores (the scene's Combat Feel, "Aim Test Attacks Are Made"); off, built blows or the strike clips themselves.</summary>
        public static bool On = true;

        /// <summary>The style attacks are made in (the scene's Combat Feel, "Attack Style").</summary>
        public static AttackStyle Style = AttackStyle.Plain;

        /// <summary>A made clip's frames a second.</summary>
        public const float Rate = 60f;

        /// <summary>The body's travel is stretched or shortened by no more than this to reach, and the wind-up's time.</summary>
        public const float LeastLunge = 0.3f, MostLunge = 1.8f, LeastWindUpTime = 0.35f, MostWindUpTime = 3f;

        /// <summary>The body is turned no further than this over the wind-up to bring the blade onto its line (degrees).</summary>
        public const float MostTurn = 50f;

        /// <summary>To bring the blade to its target's height the hips are lowered no further than this, or raised (leg lengths), and the chest leaned forward or back no further (degrees).</summary>
        public const float MostDrop = 0.2f, MostRise = 0.04f, MostLean = 28f, MostLeanBack = 10f;

        /// <summary>The sword hand makes up no more than this at contact (leg lengths).</summary>
        public const float MostHandMove = 0.45f;

        /// <summary>The blade can be taken to land at any moment of the cut at which it goes at least this share of its fastest.</summary>
        public const float FastEnough = 0.45f;

        private static float Smooth(float from, float to, float x)
        {
            if (Mathf.Abs(to - from) < 1e-6f)
            {
                return x >= to ? 1f : 0f;
            }

            float u = Mathf.Clamp01((x - from) / (to - from));
            return u * u * (3f - 2f * u);
        }

        /// <summary>A way turned toward another by no more than so many degrees (and by how many it was).</summary>
        internal static Vector3 Steered(Vector3 way, Vector3 toward, float most, out float by)
        {
            by = 0f;
            Vector3 axis = Vector3.Cross(way, toward);
            if (most <= 0f || toward.sqrMagnitude < 1e-6f || axis.sqrMagnitude < 1e-10f)
            {
                return way;
            }

            by = Mathf.Min(Vector3.Angle(way, toward), most);
            return Quaternion.AngleAxis(by, axis.normalized) * way;
        }

        /// <summary>A way turned on from where it was in the guard: the turn from there to here, times so much.</summary>
        private static Vector3 Further(Vector3 guard, Vector3 now, float times)
        {
            Quaternion.FromToRotation(guard, now).ToAngleAxis(out float angle, out Vector3 axis);
            return angle < 0.01f || axis.sqrMagnitude < 1e-8f ? now : Quaternion.AngleAxis(angle * times, axis) * guard;
        }

        /// <summary>A score's tracks at a time of the recording (seconds), straight between its frames.</summary>
        private static BodyTracks At(AttackScore score, float time)
        {
            float f = Mathf.Clamp(time * score.Rate, 0f, score.Frames.Length - 1);
            int a = Mathf.Min(Mathf.FloorToInt(f), score.Frames.Length - 2);
            return a < 0 ? score.Frames[0] : BodyTracks.Lerp(score.Frames[a], score.Frames[a + 1], f - a);
        }

        /// <summary>How an attack is fitted to its target: the moment of the recording at which it lands, its travel's stretch, the body's turn, the hips' drop and the chest's lean.</summary>
        internal struct Fit
        {
            public float Tau, Lunge, Turn, Drop, Lean;
        }

        /// <summary>
        /// The body at a time of the recording as the style and the fit have it: the legs and hips taken a little sooner
        /// in the drive and the sword arm a little later, each part's travel from where it stood in the guard multiplied,
        /// the hips lowered and the chest leaned for the target's height.
        /// </summary>
        private static BodyTracks Styled(AttackScore score, in AttackStyle style, in Fit fit, float time)
        {
            float drive = score.Drive / score.Rate, contact = score.Contact / score.Rate, follow = score.Follow / score.Rate, end = score.Duration;
            // (Only in the drive: before it and after it every part is at the recording's own time.)
            float inDrive = Smooth(drive - 0.15f, drive, time) * (1f - Smooth(contact, contact + 0.15f, time));
            var t = At(score, time);
            if (Mathf.Abs(style.HipLead) > 1e-4f)
            {
                var legs = At(score, time + style.HipLead * inDrive);
                t.Pelvis = legs.Pelvis; t.PelvisTurn = legs.PelvisTurn;
                t.AnkleL = legs.AnkleL; t.AnkleR = legs.AnkleR; t.KneeL = legs.KneeL; t.KneeR = legs.KneeR;
                t.FootL = legs.FootL; t.FootR = legs.FootR; t.ToeL = legs.ToeL; t.ToeR = legs.ToeR;
            }

            if (Mathf.Abs(style.ArmLag) > 1e-4f)
            {
                var arm = At(score, time - style.ArmLag * inDrive);
                t.ClavR = arm.ClavR; t.UpperR = arm.UpperR; t.ForeR = arm.ForeR; t.HandR = arm.HandR;
            }

            // Each part's travel from the guard, multiplied: the wind-up's share before the landing, the carry-through's after.
            var guard = score.Frames[0];
            float before = 1f - Smooth(drive, contact, time), after = Smooth(contact, follow, time) * (1f - Smooth(follow, end, time));
            float turned = 1f + (style.WindUp - 1f) * before + (style.FollowThrough - 1f) * after;
            // (No further than a body goes: the hips a quarter past the furthest the recording turns them, the chest's
            // twist on the hips no past 80 degrees or the recording's own furthest. The recorded cuts already carry the
            // chest most of the way round; pushed past that, the body faces backward.)
            // The hips' drive moves the turn between the hips and the spine: the chest faces the world as it did, so
            // the blow goes where it went, thrown more from the hips or more from the shoulders.
            float hipsFar = 1.25f * score.MostHipTurn, hipsWent = t.PelvisTurn.y - guard.PelvisTurn.y;
            float hipsPlain = Mathf.Clamp(hipsWent * turned, -hipsFar, hipsFar), hips = Mathf.Clamp(hipsWent * style.HipDrive * turned, -hipsFar, hipsFar);
            t.PelvisTurn.y = guard.PelvisTurn.y + hips;
            float twist = Mathf.Max(80f, score.MostTwist);
            t.ChestTurn.y = Mathf.Clamp(guard.ChestTurn.y + (t.ChestTurn.y - guard.ChestTurn.y) * style.Twist * turned - (hips - hipsPlain), -twist, twist);
            t.ChestTurn.x = guard.ChestTurn.x + (t.ChestTurn.x - guard.ChestTurn.x) * style.Lean;
            t.ChestTurn.z = guard.ChestTurn.z + (t.ChestTurn.z - guard.ChestTurn.z) * style.Lean;
            float swing = 1f + (style.ArmSwing - 1f) * (before + after);
            if (Mathf.Abs(swing - 1f) > 1e-3f)
            {
                t.UpperR = Further(guard.UpperR, t.UpperR, swing);
                t.ForeR = Further(guard.ForeR, t.ForeR, swing);
                t.HandR = Quaternion.SlerpUnclamped(guard.HandR, t.HandR, swing);
            }

            t.Pelvis.y = guard.Pelvis.y + (t.Pelvis.y - guard.Pelvis.y) * style.Drop;
            // (The hips and the feet together over the ground: a longer lunge is longer steps, the feet under it.)
            float far = style.Lunge * fit.Lunge;
            t.Pelvis = Travel(guard.Pelvis, t.Pelvis, far);
            t.AnkleL = Travel(guard.AnkleL, t.AnkleL, far);
            t.AnkleR = Travel(guard.AnkleR, t.AnkleR, far);

            // The stance's crouch, in soon after the start and out again by the end; the fit's drop and lean, in by the
            // landing and out by the end. The feet stay where they are: the knees bend for it. The head stays up.
            float stance = Smooth(0f, Mathf.Min(0.2f, Mathf.Max(0.05f, drive)), time) * (1f - Smooth(Mathf.Max(follow, end - 0.25f), end, time));
            float tau = fit.Tau > 0f ? fit.Tau : contact;
            float fitted = time <= tau ? Smooth(0f, tau, time) : 1f - Smooth(tau, end, time);
            t.Pelvis.y -= style.Crouch * stance + fit.Drop * fitted;
            t.ChestTurn.x += fit.Lean * fitted;
            t.HeadTurn.x -= 0.7f * fit.Lean * fitted;
            return t;
        }

        /// <summary>How much of the aim's turn is in at a time of the recording: all of it by the time the drive begins, so the lunge goes the way the blow does.</summary>
        private static float TurnShare(AttackScore score, float time) => Smooth(0f, Mathf.Max(0.05f, score.Drive / score.Rate), time);

        /// <summary>The root's place at a time of the recording with the aim's turn coming in as it goes (the frame the attack began in): each stretch of its own travel turned by as much of the turn as is in by then.</summary>
        private static Vector3 RootAt(AttackScore score, float far, float turn, float time)
        {
            Vector3 at = Vector3.zero;
            float f = Mathf.Clamp(time * score.Rate, 0f, score.Frames.Length - 1);
            // (Step by step while the turn is still coming in; from the drive on all of it is in, and the rest is one stretch.)
            int whole = Mathf.Min(score.Frames.Length - 1, Mathf.Max(1, score.Drive));
            for (int i = 0; i < whole && i < f; i++)
            {
                float share = Mathf.Min(1f, f - i);
                Vector3 step = (score.Root[i + 1] - score.Root[i]) * (far * share);
                at += Quaternion.Euler(0f, turn * TurnShare(score, (i + 0.5f * share) / score.Rate), 0f) * step;
            }

            if (f > whole)
            {
                at += Quaternion.Euler(0f, turn, 0f) * ((RootOf(score, time) - score.Root[whole]) * far);
            }

            return at;
        }

        private static Vector3 RootOf(AttackScore score, float time)
        {
            float f = Mathf.Clamp(time * score.Rate, 0f, score.Frames.Length - 1);
            int a = Mathf.Min(Mathf.FloorToInt(f), score.Frames.Length - 2);
            return Vector3.Lerp(score.Root[a], score.Root[a + 1], f - a);
        }

        /// <summary>The sword at a time of the recording, made with a style and a fit (the frame the attack began in; the sword hand not moved, the wrist not steered): its grip, its blade's way, the hips.</summary>
        private static void Sword(AttackScore score, in AttackStyle style, in Fit fit, float time, BodyMeasures m, in SwordHold hold, out Vector3 grip, out Vector3 way, out Vector3 hips, out Vector3 reach)
        {
            float far = style.Lunge * fit.Lunge;
            var t = Styled(score, style, fit, time);
            AttackScore.SwordOf(t, m, hold, score.Thrust, out Vector3 shoulder, out Vector3 wrist, out Vector3 g, out Vector3 w);
            var turn = Quaternion.Euler(0f, fit.Turn * TurnShare(score, time), 0f);
            Vector3 root = RootOf(score, time) * far, rootOut = RootAt(score, far, fit.Turn, time);
            Vector3 Out(Vector3 p)
            {
                Vector3 flat = rootOut + turn * new Vector3(p.x - root.x, 0f, p.z - root.z);
                return new Vector3(flat.x, p.y, flat.z);
            }

            grip = Out(g);
            hips = Out(t.Pelvis);
            way = turn * w;
            // (From the shoulder to the wrist, for how much further the arm has to give.)
            reach = Out(wrist) - Out(shoulder);
        }

        private static Vector3 Travel(Vector3 from, Vector3 to, float times) => new Vector3(from.x + (to.x - from.x) * times, to.y, from.z + (to.z - from.z) * times);

        private static float Bearing(Vector2 v) => Mathf.Atan2(v.x, v.y) * Mathf.Rad2Deg;

        /// <summary>
        /// An attack fitted to land at one moment of its recording: the drop and the lean for the target's height, the
        /// travel for its distance, the turn for its line, found by trying a few times over (each changes what the
        /// others need). What the wrist's steer and then the sword hand have to make up, and what it all costs.
        /// </summary>
        private static Fit FitAt(AttackScore score, in AttackStyle style, in AttackAim aim, in SwordHold hold, BodyMeasures m, float tau, out Vector3 lands, out Vector3 handMove, out float shortBy, out float steer, out float cost)
        {
            var fit = new Fit { Tau = tau, Lunge = 1f };
            float contact = score.Contact / score.Rate;
            Vector2 want = new Vector2(aim.Target.x, aim.Target.z);
            Vector3 goes = (Styled(score, style, fit, tau).Pelvis - score.Frames[0].Pelvis) * style.Lunge;
            goes.y = 0f;
            bool travels = goes.sqrMagnitude > 0.12f * 0.12f;
            Vector3 grip, way, hips, reach;
            for (int i = 0; i < 6; i++)
            {
                Sword(score, style, fit, tau, m, hold, out grip, out way, out hips, out reach);
                Vector3 strike = grip + way * hold.Strike;
                Vector2 at = new Vector2(strike.x, strike.z);
                float wantFar = want.magnitude;

                // The height: half of what is missing by the hips, the rest by the chest's lean, about the hips.
                // (A thrust's blade is taken to carry on the arm's line, so it is the arm that is brought onto the target.)
                float missing = aim.Target.y - strike.y;
                float drop = Mathf.Clamp(fit.Drop - 0.5f * missing, -MostRise, MostDrop);
                missing += drop - fit.Drop;
                fit.Drop = drop;
                float lever = new Vector2(strike.x - hips.x, strike.z - hips.z).magnitude;
                if (lever > 0.3f)
                {
                    fit.Lean = Mathf.Clamp(fit.Lean - 0.7f * Mathf.Atan2(missing, lever) * Mathf.Rad2Deg, -MostLeanBack, MostLean);
                }

                fit.Turn = Mathf.Clamp(fit.Turn + Mathf.DeltaAngle(Bearing(at), Bearing(want)), -MostTurn, MostTurn);
                if (travels)
                {
                    // (How much further the blade lands for each share more of travel: the travel's part along the line to the blade.)
                    Vector3 along = Quaternion.Euler(0f, fit.Turn, 0f) * goes;
                    float gain = Vector2.Dot(at, new Vector2(along.x, along.z)) / Mathf.Max(1e-4f, at.magnitude);
                    if (Mathf.Abs(gain) > 0.05f)
                    {
                        fit.Lunge = Mathf.Clamp(fit.Lunge + (wantFar - at.magnitude) / gain, LeastLunge, MostLunge);
                    }
                }
            }

            // The wrist's steer the last of the way, then the sword hand for what is still left.
            Sword(score, style, fit, tau, m, hold, out grip, out way, out hips, out reach);
            way = Steered(way, aim.Target - grip, score.Thrust ? hold.MostThrustSteer : hold.MostSteer, out steer);
            lands = grip + way * hold.Strike;
            Vector3 left = aim.Target - lands;
            Vector3 move = Vector3.ClampMagnitude(left, MostHandMove);
            // (No further than the arm is long: a hand asked past its reach stays at the arm's end.)
            float most = 0.99f * (m.UpperLen[1] + m.ForeLen[1]) * hold.ArmScale;
            if ((reach + move).magnitude > most)
            {
                move = (reach + move).normalized * most - reach;
            }

            shortBy = (left - move).magnitude;
            lands += move;
            // (In the recording's own frame, the turn taken out, for the arm's solve.)
            handMove = Quaternion.Euler(0f, -fit.Turn * TurnShare(score, tau), 0f) * move;
            // (What each costs: a turn of 50 degrees as much as a lean of 50, a drop of 0.35 leg lengths, half a leg length
            // of the hand's own move, or landing a second off the recording's own moment. Falling short costs most.)
            cost = 10f * shortBy + 2f * move.magnitude + 0.5f * Mathf.Abs(fit.Lunge - 1f) + Mathf.Abs(fit.Turn) / 50f
                + Mathf.Abs(fit.Lean) / 50f + Mathf.Abs(fit.Drop) / 0.35f + steer / 90f + Mathf.Abs(tau - contact);
            return fit;
        }

        /// <summary>
        /// An attack made of a score, in a style, for an aim, by a fighter that holds its sword so. With
        /// <paramref name="build"/> off it is only planned (how it is fitted and what that costs): its clip is made by
        /// <see cref="Build"/>.
        /// </summary>
        public static MadeAttack Make(AttackScore score, in AttackStyle style, in AttackAim aim, in SwordHold hold, bool build = true)
        {
            var m = BodyMeasures.OfClips;
            float drive = score.Drive / score.Rate, contact = score.Contact / score.Rate, follow = score.Follow / score.Rate, end = score.Duration;
            var made = new MadeAttack { Score = score, Hold = hold, Aim = aim, ScoreTime = contact, Lunge = 1f };
            var fit = new Fit { Tau = contact, Lunge = 1f };
            Vector3 handMove = Vector3.zero;

            if (aim.Aimed)
            {
                // Every moment of the cut at which the blade is fast is tried; the one that costs least is taken.
                // (A thrust lands where it is furthest out: its recording's own landing.)
                float fastest = 0f;
                for (int f = 0; f < score.StrikeSpeed.Length; f++)
                {
                    fastest = Mathf.Max(fastest, score.StrikeSpeed[f]);
                }

                made.Cost = float.MaxValue;
                AttackStyle styled = style;
                AttackAim aimed = aim;
                SwordHold held = hold;
                void Try(float tau)
                {
                    float f = Mathf.Clamp(tau * score.Rate, 0f, score.StrikeSpeed.Length - 1);
                    int a = Mathf.Min(Mathf.FloorToInt(f), score.StrikeSpeed.Length - 2);
                    if (tau < drive || tau > follow || (!score.Thrust && Mathf.Lerp(score.StrikeSpeed[a], score.StrikeSpeed[a + 1], f - a) < FastEnough * fastest))
                    {
                        return;
                    }

                    var tried = FitAt(score, styled, aimed, held, m, tau, out Vector3 lands, out Vector3 move, out float shortBy, out float steer, out float cost);
                    if (cost < made.Cost)
                    {
                        made.Cost = cost;
                        fit = tried;
                        handMove = move;
                        made.Lands = lands;
                        made.Short = shortBy;
                        made.Steer = steer;
                    }
                }

                // (Each thirtieth of a second, then finer about the best, twice: the blade crosses 15 cm of height in a sixtieth.)
                for (float tau = score.Thrust ? contact : drive; tau <= (score.Thrust ? contact : follow) + 1e-4f; tau += 1f / 30f)
                {
                    Try(tau);
                }

                for (float step = 1f / 90f; step > 1f / 300f && !score.Thrust && made.Cost < float.MaxValue; step /= 3f)
                {
                    float about = fit.Tau;
                    Try(about - step);
                    Try(about + step);
                }

                if (made.Cost == float.MaxValue)
                {
                    fit = FitAt(score, style, aim, hold, m, contact, out made.Lands, out handMove, out made.Short, out made.Steer, out made.Cost);
                }

                made.ScoreTime = fit.Tau;
                made.Turn = fit.Turn;
                made.Lunge = fit.Lunge;
                made.Drop = fit.Drop;
                made.Lean = fit.Lean;
                made.HandMoved = handMove.magnitude;
            }
            else
            {
                Sword(score, style, fit, contact, m, hold, out Vector3 grip, out Vector3 way, out _, out _);
                made.Lands = grip + way * hold.Strike;
            }

            made.Style = style;
            made.Plan = fit;
            made.HandMove = handMove;
            if (build)
            {
                Build(made);
            }

            return made;
        }

        /// <summary>A planned attack's clip made (<see cref="Make"/> with its clip left for later: of several plans only the one taken is made).</summary>
        public static void Build(MadeAttack made)
        {
            if (made == null || made.Clip != null)
            {
                return;
            }

            var m = BodyMeasures.OfClips;
            var score = made.Score;
            AttackStyle style = made.Style;
            AttackAim aim = made.Aim;
            SwordHold hold = made.Hold;
            Fit fit = made.Plan;
            Vector3 handMove = made.HandMove;
            float drive = score.Drive / score.Rate, follow = score.Follow / score.Rate, end = score.Duration;

            // How long the walk will hold the clip back through the drive, for its blade's speed: where the point would go
            // faster than the limit, the clip is slowed to it.
            made.Held = 0f;
            float limit = CombatFeel.MostBladeSpeed * LocoBody.BladePaceShare;
            if (limit > 0f)
            {
                float each = 1f / Rate, ds = each / Mathf.Clamp(style.DriveTime, 0.4f, 3f);
                Sword(score, style, fit, drive, m, hold, out Vector3 grip, out Vector3 way, out _, out _);
                Vector3 last = grip + way * hold.Length;
                for (float s = drive + ds; s <= made.ScoreTime + 1e-4f; s += ds)
                {
                    Sword(score, style, fit, s, m, hold, out grip, out way, out _, out _);
                    Vector3 point = grip + way * hold.Length;
                    float speed = (point - last).magnitude * hold.Leg / each;
                    made.Held += speed > limit ? each * (speed / limit - 1f) : 0f;
                    last = point;
                }
            }

            // The times: the wind-up stretched or shortened (to land when asked, and so the landing falls on a frame), the
            // drive and the carry-through by the style, the way back by the style.
            float tauAt = made.ScoreTime;
            float driveTime = Mathf.Clamp(style.DriveTime, 0.4f, 3f), backTime = Mathf.Max(0.2f, style.RecoveryTime);
            float windTime = Mathf.Max(0.05f, style.WindUpTime);
            if (aim.Aimed && aim.LandIn > 0f && drive > 0.02f)
            {
                windTime = (aim.LandIn - made.Held - (tauAt - drive) * driveTime) / drive;
            }

            windTime = Mathf.Clamp(windTime, LeastWindUpTime, MostWindUpTime);
            int contactFrame = Mathf.Max(2, Mathf.RoundToInt((drive * windTime + (tauAt - drive) * driveTime) * Rate));
            if (drive > 0.02f)
            {
                windTime = Mathf.Max(0.05f, (contactFrame / Rate - (tauAt - drive) * driveTime) / drive);
            }

            float driveOut = drive * windTime, followOut = driveOut + (follow - drive) * driveTime, endOut = followOut + (end - follow) * backTime;
            int frames = Mathf.Max(contactFrame + 2, Mathf.CeilToInt(endOut * Rate) + 1);
            made.ContactAt = contactFrame / Rate;

            var data = new LocoClipData
            {
                name = "made_" + score.Name,
                role = "strike",
                kind = score.Kind,
                set = "guard",
                loop = false,
                rate = Rate,
                frames = frames,
                legLength = Locomotor.ClipLeg,
                duration = (frames - 1) / Rate,
                markers = new[] { contactFrame, Mathf.Min(frames - 1, contactFrame + Mathf.Max(1, Mathf.RoundToInt((score.Link - score.Contact) / score.Rate * driveTime * Rate))) },
                root = new float[frames * 3],
                points = new float[frames * LocoClip.BoneCount * 3],
                contacts = new int[frames * 2],
                about = "made of " + score.Name,
            };
            var p = new Vector3[LocoClip.BoneCount];
            float far = style.Lunge * fit.Lunge;
            for (int f = 0; f < frames; f++)
            {
                // The moment of the recording this frame is.
                float u = f / Rate;
                float s = u <= driveOut ? u / windTime : u <= followOut ? drive + (u - driveOut) / driveTime : follow + (u - followOut) / backTime;
                s = Mathf.Clamp(s, 0f, end);
                var t = Styled(score, style, fit, s);
                if (handMove.sqrMagnitude > 1e-8f)
                {
                    // (Most at the landing: in from the drive's start, out again by the time the blow is carried through.)
                    float w = s <= tauAt ? Smooth(drive, tauAt, s) : 1f - Smooth(tauAt, Mathf.Max(tauAt + 0.05f, follow), s);
                    MoveHand(ref t, m, handMove * (w / Mathf.Max(0.5f, hold.ArmScale)));
                }

                if (score.Thrust)
                {
                    // A thrust's point leads: its blade is turned to where it is going (the target, or straight on along
                    // the arm with none), from 0.4 of its wind-up, and let go on the way back. (The recorded thrusting
                    // hand is held so that this fighter's sword would point up and back in it.)
                    float w = s <= tauAt ? Smooth(0.4f * drive, Mathf.Max(0.4f * drive + 0.05f, drive), s) : 1f - Smooth(follow, end, s);
                    PointBlade(ref t, m, hold, aim.Aimed, Quaternion.Euler(0f, -fit.Turn * TurnShare(score, s), 0f) * (aim.Target - RootAt(score, far, fit.Turn, s)) + RootOf(score, s) * far, w);
                }

                m.Compose(t, p);
                // The root: the recording's own, its travel multiplied as the body's is and turned as the aim's turn comes
                // in; the points against the recording's root, so the body goes with it.
                float fs = Mathf.Clamp(s * score.Rate, 0f, score.Frames.Length - 1);
                int a = Mathf.Min(Mathf.FloorToInt(fs), score.Frames.Length - 2);
                float share = fs - a;
                Vector3 root = Vector3.Lerp(score.Root[a], score.Root[a + 1], share) * far;
                float facing = Mathf.Lerp(score.Facing[a], score.Facing[a + 1], share);
                Vector3 rootOut = RootAt(score, far, fit.Turn, s);
                data.root[f * 3] = rootOut.x;
                data.root[f * 3 + 1] = rootOut.z;
                data.root[f * 3 + 2] = facing + fit.Turn * TurnShare(score, s);
                var back = Quaternion.Euler(0f, -facing, 0f);
                for (int i = 0; i < LocoClip.BoneCount; i++)
                {
                    Vector3 local = back * new Vector3(p[i].x - root.x, 0f, p[i].z - root.z);
                    int at = (f * LocoClip.BoneCount + i) * 3;
                    data.points[at] = local.x;
                    data.points[at + 1] = p[i].y;
                    data.points[at + 2] = local.z;
                }

                int near = Mathf.Clamp(Mathf.RoundToInt(fs), 0, score.Frames.Length - 1);
                data.contacts[f * 2] = (int)score.Contacts[near * 2];
                data.contacts[f * 2 + 1] = (int)score.Contacts[near * 2 + 1];
            }

            made.Clip = new LocoClip(data);
        }

        /// <summary>
        /// The sword hand turned so its blade points at a place (the same frame as the tracks; with no place, on along
        /// the arm's line), by so much of the way (0 to 1).
        /// </summary>
        private static void PointBlade(ref BodyTracks t, BodyMeasures m, in SwordHold hold, bool aimed, Vector3 at, float weight)
        {
            if (weight <= 0f)
            {
                return;
            }

            AttackScore.SwordOf(t, m, hold, false, out Vector3 shoulder, out _, out Vector3 grip, out Vector3 way);
            Vector3 want = aimed ? at - grip : grip - shoulder;
            if (want.sqrMagnitude < 1e-8f)
            {
                return;
            }

            Quaternion pel = Quaternion.Euler(t.PelvisTurn), che = pel * Quaternion.Euler(t.ChestTurn);
            var turn = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(way, want.normalized), weight);
            t.HandR = Quaternion.Inverse(che) * turn * (che * t.HandR);
        }

        /// <summary>The sword hand moved so far (the same frame as the tracks), the arm solved for it: the shoulder where it is, the elbow kept on the side it was, the hand's turn as it was.</summary>
        private static void MoveHand(ref BodyTracks t, BodyMeasures m, Vector3 by)
        {
            Quaternion pel = Quaternion.Euler(t.PelvisTurn), che = pel * Quaternion.Euler(t.ChestTurn);
            Vector3 upper = che * t.UpperR.normalized * m.UpperLen[1], fore = che * t.ForeR.normalized * m.ForeLen[1];
            Vector3 toWrist = upper + fore + by;
            float a = m.UpperLen[1], b = m.ForeLen[1];
            float far = Mathf.Clamp(toWrist.magnitude, Mathf.Abs(a - b) + 1e-3f, 0.995f * (a + b));
            Vector3 axis = toWrist.sqrMagnitude > 1e-10f ? toWrist.normalized : che * Vector3.forward;
            float along = (far * far + a * a - b * b) / (2f * far);
            float off = Mathf.Sqrt(Mathf.Max(0f, a * a - along * along));
            // (The elbow off the line from shoulder to wrist the way it was.)
            Vector3 way = upper - axis * Vector3.Dot(upper, axis);
            way = way.sqrMagnitude > 1e-10f ? way.normalized : che * Vector3.down;
            Vector3 elbow = axis * along + way * off, wrist = axis * far;
            Quaternion toChest = Quaternion.Inverse(che);
            t.UpperR = (toChest * elbow).normalized;
            t.ForeR = (toChest * (wrist - elbow)).normalized;
        }
    }
}
