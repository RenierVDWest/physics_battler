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
        /// <summary>
        /// A style's manner is laid over him as he strikes (<see cref="StyleManner"/>; a blow thrown from a stand):
        /// the carriages it goes between and how much of it. The blow is fitted for the body as the manner will
        /// have it: leaned and sunk, its sword arm hanging from the chest as the recording's does.
        /// </summary>
        public bool Mannered;
        public Carriage MannerFrom, MannerTo;
        public MannerDials MannerDials;

        /// <summary>It is aimed (off: the score as it is, its own contact).</summary>
        public bool Aimed;

        /// <summary>Where the blade's striking part is to land, in the fighter's own frame as the attack begins (leg lengths; x to its right, y over the ground, z forward).</summary>
        public Vector3 Target;

        /// <summary>The blade is to land this long after the attack begins (seconds; 0: as the score has it).</summary>
        public float LandIn;

        /// <summary>
        /// How the body is going as the attack begins (its own frame then, leg lengths a second, flat; the plan "Fight
        /// Rhythm from Recordings", step 3: attacks from any pose). It is carried on into the attack and spent over
        /// <see cref="AttackMaker.CoastTime"/>: a walking body does not stop dead to strike, it runs out under the
        /// wind-up. The aim allows for the ground that covers.
        /// </summary>
        public Vector3 Carry;

        /// <summary>
        /// The attack's own travel begins from the speed the body has (<see cref="Carry"/>; standing, from rest): the
        /// recordings begin already moving, half a metre a second in their first frame, and played as they are the
        /// body jumps to that speed in one step.
        /// </summary>
        public bool Eased;

        /// <summary>
        /// Thrown on the move (<see cref="AttackOnTheMove"/>): the walk goes on under the blow, so the body's travel is
        /// the walk's, not the recording's, and the blow's upper body sits on the walk's hips. The attack is fitted
        /// to where the walk will have him as the blade lands.
        /// </summary>
        public bool OnTheMove;

        /// <summary>On the move: how he walks as it begins (his own frame then, leg lengths a second) and how fast he turns (degrees a second, + to his right).</summary>
        public Vector3 Walks;
        public float Turns;

        /// <summary>On the move: he keeps facing this place as he goes (his own frame as it begins, leg lengths): his enemy. Without, he turns on at the rate he turns now.</summary>
        public bool Faces;
        public Vector3 FacesAt;

        /// <summary>On the move: the walk's body against its root as the blow begins (leg lengths): the hips the blow's chest is to sit on.</summary>
        public BodyTracks WalkNow;

        /// <summary>On the move: how the blow sits on the walk, and how far the hips go into it and which way (against the root, leg lengths).</summary>
        public MoveBlowStyle Move;
        public Vector3 Into;

        /// <summary>
        /// How long each foot is still in the air as the attack begins (seconds; 0: it is on the ground): a foot caught
        /// in the middle of its step finishes that step, and is not put down where it happens to be.
        /// </summary>
        public float AirL, AirR;
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

        /// <summary>When the hips start in (seconds into the clip): the wind-up is over.</summary>
        public float DriveAt;

        /// <summary>Thrown on the move: what it was asked for (its target in the fighter's own frame as it begins, how he walked). <see cref="Aim"/> then holds the same target as the recording's own frame has it.</summary>
        public bool OnTheMove;
        public AttackAim Asked;

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

        /// <summary>Thrown on the move: the step in the walk takes under the blow, on top of where it was going (the fighter's own frame as it begins, leg lengths).</summary>
        public Vector3 StepIn;

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

        /// <summary>
        /// An attack begins from where the body is (the scene's Combat Feel, "Attacks Start From Where He Is"): the way
        /// he is going is carried on into it, a foot in the air finishes its step, and each part keeps the speed it had
        /// while it goes over to the attack. Off: he stops in one step and the attack begins from his guard.
        /// </summary>
        public static bool FromAnyPose = true;

        /// <summary>How long the way he was going is carried on into an attack (seconds): his speed runs down evenly to nothing over it, so he covers half of what he would have at full speed.</summary>
        public static float CoastTime = 0.4f;

        /// <summary>
        /// How far the speed the body had has taken it by a time of the made clip (seconds; the frame the attack began
        /// in, leg lengths). <paramref name="carried"/>: that speed against the attack's own at its start
        /// (<see cref="Carried"/>), run down evenly to nothing over <see cref="CoastTime"/>.
        /// </summary>
        internal static Vector3 Coast(Vector3 carried, float time)
        {
            if (carried.sqrMagnitude < 1e-8f || CoastTime <= 0f)
            {
                return Vector3.zero;
            }

            float x = Mathf.Clamp01(time / CoastTime);
            return carried * (CoastTime * (x - 0.5f * x * x));
        }

        /// <summary>
        /// The speed the body has as the attack begins against the speed the attack's own travel begins at (leg
        /// lengths a second, the frame it began in): carried on top of that travel and run down, the body's speed is
        /// the same in the attack's first step as it was in the step before.
        /// </summary>
        internal static Vector3 Carried(AttackScore score, in AttackAim aim, float far, float windTime)
        {
            if (!aim.Eased)
            {
                return aim.Carry;
            }

            int n = Mathf.Min(2, score.Root.Length - 1);
            Vector3 begins = n > 0 ? (score.Root[n] - score.Root[0]) * (score.Rate / n * far / Mathf.Max(0.05f, windTime)) : Vector3.zero;
            return aim.Carry - new Vector3(begins.x, 0f, begins.z);
        }

        /// <summary>How much the wind-up's time is multiplied by for an attack to land when asked (the blade's hold not counted), as <see cref="Build"/> times it.</summary>
        private static float WindTime(AttackScore score, in AttackStyle style, in AttackAim aim, float tau)
        {
            float drive = score.Drive / score.Rate, driveTime = Mathf.Clamp(style.DriveTime, 0.4f, 3f);
            float windTime = Mathf.Max(0.05f, style.WindUpTime);
            if (aim.Aimed && aim.LandIn > 0f && drive > 0.02f)
            {
                windTime = (aim.LandIn - (tau - drive) * driveTime) / drive;
            }

            return Mathf.Clamp(windTime, LeastWindUpTime, MostWindUpTime);
        }

        /// <summary>When a moment of the recording comes in the made clip (seconds), as <see cref="Build"/> times it.</summary>
        private static float LandsAfter(AttackScore score, in AttackStyle style, in AttackAim aim, float tau)
        {
            float drive = score.Drive / score.Rate;
            return drive * WindTime(score, style, aim, tau) + (tau - drive) * Mathf.Clamp(style.DriveTime, 0.4f, 3f);
        }

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

            /// <summary>Thrown on the move: the step in the walk takes under the blow (the frame he is in as it begins, leg lengths).</summary>
            public Vector3 StepIn;
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

        /// <summary>
        /// The root's place at a time of the recording with the aim's turn coming in as it goes (the frame the attack
        /// began in): each stretch of its own travel turned by as much of the turn as is in by then, and the whole
        /// carried round the planted foot (<see cref="AttackScore.Pivot"/>).
        /// </summary>
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

            // (The turn is about the planted foot, not about the body's middle: the root is carried round that foot, so
            // the foot stays where it stands and the body swings about it.)
            return at + score.Pivot - Quaternion.Euler(0f, turn * TurnShare(score, time), 0f) * score.Pivot;
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
            // (The way he was going carries him on under the attack: by the landing the target is that much nearer, or
            // further, or more to a side, and the attack is fitted to where it will be against him then.)
            float landsAfter = LandsAfter(score, style, aim, tau), windTime = WindTime(score, style, aim, tau);
            // (Thrown on the move: the walk carries him, so there is no travel of the blow's own to stretch, and the
            // target is taken where it will be against the recording's own body when the walk has him at the landing.)
            bool moving = aim.OnTheMove;
            float driveOut = score.Drive / score.Rate * windTime;
            Vector3 coast = moving ? Vector3.zero : Coast(Carried(score, aim, style.Lunge, windTime), landsAfter);
            bool mannered = !moving && aim.Mannered;
            Vector3 target = moving ? Virtual(score, style, fit, aim, tau, landsAfter, driveOut) : aim.Target - coast;
            if (mannered)
            {
                target = Mannered(score, style, fit, aim, tau, target);
            }

            Vector2 want = new Vector2(target.x, target.z);
            Vector3 goes = (Styled(score, style, fit, tau).Pelvis - score.Frames[0].Pelvis) * style.Lunge;
            goes.y = 0f;
            bool travels = !moving && goes.sqrMagnitude > 0.12f * 0.12f;
            Vector3 grip, way, hips, reach;
            for (int i = 0; i < 6; i++)
            {
                // (The attack's own speed at its start goes with its travel's stretch: what is carried against it is found again.)
                coast = moving ? Vector3.zero : Coast(Carried(score, aim, style.Lunge * fit.Lunge, windTime), landsAfter);
                target = moving ? Virtual(score, style, fit, aim, tau, landsAfter, driveOut) : aim.Target - coast;
                if (mannered)
                {
                    target = Mannered(score, style, fit, aim, tau, target);
                }

                want = new Vector2(target.x, target.z);
                Sword(score, style, fit, tau, m, hold, out grip, out way, out hips, out reach);
                Vector3 strike = grip + way * hold.Strike;
                Vector2 at = new Vector2(strike.x, strike.z);
                float wantFar = want.magnitude;

                // The height: half of what is missing by the hips, the rest by the chest's lean, about the hips.
                // (A thrust's blade is taken to carry on the arm's line, so it is the arm that is brought onto the target.)
                float missing = target.y - strike.y;
                float drop = Mathf.Clamp(fit.Drop - 0.5f * missing, -MostRise, MostDrop);
                missing += drop - fit.Drop;
                fit.Drop = drop;
                float lever = new Vector2(strike.x - hips.x, strike.z - hips.z).magnitude;
                if (lever > 0.3f)
                {
                    fit.Lean = Mathf.Clamp(fit.Lean - 0.7f * Mathf.Atan2(missing, lever) * Mathf.Rad2Deg, -MostLeanBack, MostLean);
                }

                fit.Turn = Mathf.Clamp(fit.Turn + Mathf.DeltaAngle(Bearing(at), Bearing(want)), -MostTurn, MostTurn);
                if (moving && aim.Move.StepIn > 0f)
                {
                    // (On the move the distance is the walk's to make up: it steps in toward the target, or holds back
                    // from it, by what the blade is short or long. Along the line from him to the target as the blow begins.)
                    Vector3 line = new Vector3(aim.Target.x, 0f, aim.Target.z);
                    if (line.sqrMagnitude > 1e-6f)
                    {
                        float along = Vector3.Dot(fit.StepIn, line.normalized) + (wantFar - at.magnitude);
                        float furthest = Mathf.Min(aim.Move.StepIn, AttackOnTheMove.MostStepInPace * landsAfter);
                        fit.StepIn = line.normalized * Mathf.Clamp(along, -0.5f * furthest, furthest);
                    }
                }

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
            coast = moving ? Vector3.zero : Coast(Carried(score, aim, style.Lunge * fit.Lunge, windTime), landsAfter);
            target = moving ? Virtual(score, style, fit, aim, tau, landsAfter, driveOut) : aim.Target - coast;
            if (mannered)
            {
                target = Mannered(score, style, fit, aim, tau, target);
            }

            Sword(score, style, fit, tau, m, hold, out grip, out way, out hips, out reach);
            way = Steered(way, target - grip, score.Thrust ? hold.MostThrustSteer : hold.MostSteer, out steer);
            lands = grip + way * hold.Strike;
            Vector3 left = target - lands;
            Vector3 move = Vector3.ClampMagnitude(left, MostHandMove);
            // (No further than the arm is long: a hand asked past its reach stays at the arm's end.)
            float most = 0.99f * (m.UpperLen[1] + m.ForeLen[1]) * hold.ArmScale;
            if ((reach + move).magnitude > most)
            {
                move = (reach + move).normalized * most - reach;
            }

            shortBy = (left - move).magnitude;
            lands += move + coast;
            // (In the recording's own frame, the turn taken out, for the arm's solve.)
            handMove = Quaternion.Euler(0f, -fit.Turn * TurnShare(score, tau), 0f) * move;
            // (What each costs: a turn of 50 degrees as much as a lean of 50, a drop of 0.35 leg lengths, half a leg length
            // of the hand's own move, or landing a second off the recording's own moment. Falling short costs most.)
            cost = 10f * shortBy + 2f * move.magnitude + 0.5f * Mathf.Abs(fit.Lunge - 1f) + 0.7f * fit.StepIn.magnitude + Mathf.Abs(fit.Turn) / 50f
                + Mathf.Abs(fit.Lean) / 50f + Mathf.Abs(fit.Drop) / 0.35f + steer / 90f + Mathf.Abs(tau - contact);
            return fit;
        }

        /// <summary>The chest's place and turn for a body's tracks with its spine bent as given.</summary>
        private static void ChestOf(in BodyTracks t, in SpineShape spine, out Vector3 at, out Quaternion turn)
        {
            Quaternion pel = Quaternion.Euler(t.PelvisTurn);
            turn = pel * Quaternion.Euler(t.ChestTurn);
            at = t.Pelvis + pel * spine.Spine + Quaternion.Slerp(pel, turn, BodyMeasures.SpineShare1) * spine.Spine1 + Quaternion.Slerp(pel, turn, BodyMeasures.SpineShare2) * spine.Spine2;
        }

        /// <summary>
        /// A blow thrown from a stand by a fighter a style's manner is laid over: its target as the recording's own
        /// body has it (the frame the blow begins in, leg lengths). The manner leans and sinks the body after the
        /// blow is made, and the sword arm hangs from the chest the same either way; so the target is carried from
        /// the chest as the manner will have it at the landing to the chest as the recording has it. Fitted to
        /// that, the blow lands on the target with the manner on. (The same carrying as a blow on the move's:
        /// <see cref="Virtual"/>.)
        /// </summary>
        private static Vector3 Mannered(AttackScore score, in AttackStyle style, in Fit fit, in AttackAim aim, float tau, Vector3 target)
        {
            float far = style.Lunge * fit.Lunge;
            var t = Styled(score, style, fit, tau);
            Vector3 root = RootOf(score, tau) * far, rootOut = RootAt(score, far, fit.Turn, tau);
            float facing = FacingOf(score, tau);
            // (The body against its own root, as the walk has it when it lays the manner.)
            var back = Quaternion.Euler(0f, -facing, 0f);
            var onRoot = t;
            onRoot.Pelvis = back * (t.Pelvis - root);
            onRoot.Pelvis.y = t.Pelvis.y;
            onRoot.PelvisTurn.y -= facing;
            var laid = StyleManner.Lay(onRoot, aim.MannerFrom, aim.MannerTo, aim.MannerDials);
            ChestOf(onRoot, aim.MannerFrom.Spine, out Vector3 chest, out Quaternion che);
            ChestOf(laid, SpineShape.Lerp(aim.MannerFrom.Spine, aim.MannerTo.Spine, aim.MannerDials.Carriage), out Vector3 chestLaid, out Quaternion cheLaid);
            // (The root's frame out to the frame the blow begins in, and back.)
            var outTurn = Quaternion.Euler(0f, fit.Turn * TurnShare(score, tau) + facing, 0f);
            Vector3 flat = Quaternion.Inverse(outTurn) * new Vector3(target.x - rootOut.x, 0f, target.z - rootOut.z);
            Vector3 onRootTarget = new Vector3(flat.x, target.y, flat.z);
            Vector3 own = chest + che * (Quaternion.Inverse(cheLaid) * (onRootTarget - chestLaid));
            Vector3 there = rootOut + outTurn * new Vector3(own.x, 0f, own.z);
            return new Vector3(there.x, own.y, there.z);
        }

        private static float FacingOf(AttackScore score, float time)
        {
            float f = Mathf.Clamp(time * score.Rate, 0f, score.Frames.Length - 1);
            int a = Mathf.Min(Mathf.FloorToInt(f), score.Frames.Length - 2);
            return a < 0 ? score.Facing[0] : Mathf.Lerp(score.Facing[a], score.Facing[a + 1], f - a);
        }

        /// <summary>The chest's place (the last spine joint, where the shoulders hang) and its turn, for a body's tracks (the same frame).</summary>
        private static void ChestOf(in BodyTracks t, BodyMeasures m, out Vector3 at, out Quaternion turn)
        {
            Quaternion pel = Quaternion.Euler(t.PelvisTurn);
            turn = pel * Quaternion.Euler(t.ChestTurn);
            at = t.Pelvis + pel * m.Spine + Quaternion.Slerp(pel, turn, BodyMeasures.SpineShare1) * m.Spine1 + Quaternion.Slerp(pel, turn, BodyMeasures.SpineShare2) * m.Spine2;
        }

        /// <summary>
        /// A blow thrown on the move at its landing, both ways: the chest as the recording's own body has it (the
        /// frame the recording begins in, the fit's turn in), and the chest as it is when the blow is laid on the walk
        /// (the frame he is in as the blow begins): the walk's body as it is now, taken to where the walk will have
        /// it, with the blow layered on it as the walk will layer it (<see cref="AttackOnTheMove.Layer"/>).
        /// </summary>
        private static void OnTheMove(AttackScore score, in AttackStyle style, in Fit fit, in AttackAim aim, float tau, float landsAfter, float driveOut, BodyMeasures m,
            out Vector3 chestOwn, out Quaternion turnOwn, out Vector3 chestWalking, out Quaternion turnWalking, out BodyTracks layered)
        {
            float far = style.Lunge * fit.Lunge;
            var t = Styled(score, style, fit, tau);
            var guard = score.Frames[0];
            Vector3 root = RootOf(score, tau) * far, rootOut = RootAt(score, far, fit.Turn, tau);
            float turnIn = fit.Turn * TurnShare(score, tau), facing = FacingOf(score, tau);
            // The recording's own.
            ChestOf(t, m, out Vector3 chest, out Quaternion che);
            var turn = Quaternion.Euler(0f, turnIn, 0f);
            Vector3 flat = rootOut + turn * new Vector3(chest.x - root.x, 0f, chest.z - root.z);
            chestOwn = new Vector3(flat.x, chest.y, flat.z);
            turnOwn = turn * che;
            // Laid on the walk: the blow and its guard against the recording's root, as its clip has them.
            var back = Quaternion.Euler(0f, -facing, 0f);
            var blow = t;
            blow.Pelvis = back * (t.Pelvis - root);
            blow.Pelvis.y = t.Pelvis.y;
            blow.PelvisTurn.y -= facing;
            var first = guard;
            first.Pelvis = Quaternion.Euler(0f, -score.Facing[0], 0f) * (guard.Pelvis - score.Root[0] * far);
            first.Pelvis.y = guard.Pelvis.y;
            first.PelvisTurn.y -= score.Facing[0];
            layered = AttackOnTheMove.Layer(aim.WalkNow, blow, first, facing + turnIn - score.Facing[0], 1f, aim.Into, aim.Move);
            ChestOf(layered, m, out Vector3 onRoot, out Quaternion cheOnRoot);
            AttackOnTheMove.Walked(aim.Walks, aim.Turns, aim.Faces ? aim.FacesAt : (Vector3?)null, fit.StepIn, landsAfter, driveOut, landsAfter, aim.Move.Check, out Vector3 at, out float turned);
            var walked = Quaternion.Euler(0f, turned, 0f);
            Vector3 there = at + walked * new Vector3(onRoot.x, 0f, onRoot.z);
            chestWalking = new Vector3(there.x, onRoot.y, there.z);
            turnWalking = walked * cheOnRoot;
        }

        /// <summary>
        /// A blow thrown on the move: its target as the recording's own body has it (the frame the recording begins
        /// in, leg lengths). On the move the blow's chest and arms sit on the walk's hips, where the walk has taken
        /// them by the landing; the recording has them on its own. The sword arm hangs from the chest the same in
        /// both, so the target is carried from the one chest to the other: where it is against the walking body's
        /// chest at the landing is where it is put against the recording's. Fitted to that, the blow lands on the
        /// target when it is thrown over the walk.
        /// </summary>
        private static Vector3 Virtual(AttackScore score, in AttackStyle style, in Fit fit, in AttackAim aim, float tau, float landsAfter, float driveOut)
        {
            OnTheMove(score, style, fit, aim, tau, landsAfter, driveOut, BodyMeasures.OfClips, out Vector3 chestOwn, out Quaternion turnOwn, out Vector3 chestWalking, out Quaternion turnWalking, out _);
            return chestOwn + turnOwn * (Quaternion.Inverse(turnWalking) * (aim.Target - chestWalking));
        }

        /// <summary>
        /// For the reports, a blow thrown on the move as its plan has it at the landing: its hips against the walk's
        /// root (leg lengths) and how they are turned (degrees), and where its blade's striking part lands (the frame
        /// he is in as the blow begins, leg lengths).
        /// </summary>
        public static void PlannedOnTheMove(MadeAttack made, out Vector3 hipsOnRoot, out float hipsYaw, out Vector3 lands)
        {
            var score = made.Score;
            AttackStyle style = made.Style;
            Fit fit = made.Plan;
            AttackAim aim = made.Asked;
            float tau = fit.Tau;
            float landsAfter = LandsAfter(score, style, aim, tau), driveOut = score.Drive / score.Rate * WindTime(score, style, aim, tau);
            OnTheMove(score, style, fit, aim, tau, landsAfter, driveOut, BodyMeasures.OfClips, out Vector3 chestOwn, out Quaternion turnOwn, out Vector3 chestWalking, out Quaternion turnWalking, out BodyTracks layered);
            hipsOnRoot = layered.Pelvis;
            hipsYaw = layered.PelvisTurn.y;
            lands = chestWalking + turnWalking * (Quaternion.Inverse(turnOwn) * (made.Lands - chestOwn));
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
            var made = new MadeAttack { Score = score, Hold = hold, Aim = aim, Asked = aim, ScoreTime = contact, Lunge = 1f };
            // (Thrown on the move the travel is the walk's: the style's lunge has nothing to stretch.)
            AttackStyle used = style;
            if (aim.Aimed && aim.OnTheMove)
            {
                used.Lunge = 1f;
            }

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
                AttackStyle styled = used;
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
                    fit = FitAt(score, used, aim, hold, m, contact, out made.Lands, out handMove, out made.Short, out made.Steer, out made.Cost);
                }

                made.ScoreTime = fit.Tau;
                made.Turn = fit.Turn;
                made.Lunge = fit.Lunge;
                made.Drop = fit.Drop;
                made.Lean = fit.Lean;
                made.HandMoved = handMove.magnitude;
                made.StepIn = fit.StepIn;
            }
            else
            {
                Sword(score, used, fit, contact, m, hold, out Vector3 grip, out Vector3 way, out _, out _);
                made.Lands = grip + way * hold.Strike;
            }

            made.Style = used;
            made.Plan = fit;
            made.HandMove = handMove;
            if (aim.Aimed && aim.OnTheMove)
            {
                // (From here on it is made as the recording's own body would make it, for the target as that body has
                // it; thrown over the walk, that is the target asked for.)
                made.OnTheMove = true;
                AttackAim own = aim;
                own.Target = Virtual(score, used, fit, aim, fit.Tau, LandsAfter(score, used, aim, fit.Tau), drive * WindTime(score, used, aim, fit.Tau));
                own.Carry = Vector3.zero;
                own.Eased = false;
                own.AirL = own.AirR = 0f;
                made.Aim = own;
            }

            if (aim.Aimed && !aim.OnTheMove && aim.Mannered)
            {
                // (From here on it is made for the target as the recording's own body has it: with the manner laid on, that is the target asked for.)
                float windTime = WindTime(score, used, aim, fit.Tau), landsAfter = LandsAfter(score, used, aim, fit.Tau);
                Vector3 real = aim.Target - Coast(Carried(score, aim, used.Lunge * fit.Lunge, windTime), landsAfter);
                AttackAim own = aim;
                own.Target = aim.Target + (Mannered(score, used, fit, aim, fit.Tau, real) - real);
                made.Aim = own;
            }

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
            made.DriveAt = driveOut;

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
            Vector3 carried = Carried(score, aim, far, windTime);
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
                    PointBlade(ref t, m, hold, aim.Aimed, Quaternion.Euler(0f, -fit.Turn * TurnShare(score, s), 0f) * (aim.Target - Coast(carried, u) - RootAt(score, far, fit.Turn, s)) + RootOf(score, s) * far, w);
                }

                m.Compose(t, p);
                // The root: the recording's own, its travel multiplied as the body's is and turned as the aim's turn comes
                // in; the points against the recording's root, so the body goes with it.
                float fs = Mathf.Clamp(s * score.Rate, 0f, score.Frames.Length - 1);
                int a = Mathf.Min(Mathf.FloorToInt(fs), score.Frames.Length - 2);
                float share = fs - a;
                Vector3 root = Vector3.Lerp(score.Root[a], score.Root[a + 1], share) * far;
                float facing = Mathf.Lerp(score.Facing[a], score.Facing[a + 1], share);
                // (And carried on the way he was going as it began, that speed running down.)
                Vector3 rootOut = RootAt(score, far, fit.Turn, s) + Coast(carried, u);
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
                // (A foot that was in the air as the attack began stays in the air until its step would have landed.)
                data.contacts[f * 2] = u < aim.AirL ? 0 : (int)score.Contacts[near * 2];
                data.contacts[f * 2 + 1] = u < aim.AirR ? 0 : (int)score.Contacts[near * 2 + 1];
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
