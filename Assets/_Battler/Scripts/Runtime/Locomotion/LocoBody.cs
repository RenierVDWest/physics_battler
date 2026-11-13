using PhysicsBattler.Motion;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Locomotion
{
    /// <summary>
    /// Puts the locomotion's pose on the rig (the locomotion lab). The clip's bones are retargeted by
    /// landmarks, so a body of other proportions takes the same motion: the pelvis, the spine, the
    /// chest and the head are turned to the frames the clip's bones make (their up along the bone,
    /// their right along the hips, the shoulders, the eyes); the arms reach for where the clip's hands
    /// are against the shoulders, the elbows toward the clip's; the legs reach for their feet.
    ///
    /// The feet are held: a foot is locked where it lands for exactly as long as its clip says it is
    /// down; while only its toe is down it turns about the toe; when it lifts, what the lock held it
    /// from is faded out. The hips drop just enough for both legs to reach.
    /// </summary>
    public sealed class LocoBody
    {
        private readonly AnimRig anim;
        private readonly Locomotor loco;
        private readonly float pelvisAboveHips;
        private readonly float legReach;
        private readonly Vector3 restFootDir;

        /// <summary>The ground's height.</summary>
        public float GroundY;

        /// <summary>A foot's hold on the ground.</summary>
        public sealed class FootLock
        {
            public bool Locked;
            public bool OnToe;
            public Vector3 Ankle;      // the ankle held (world; its height still the clip's)
            public Vector3 ToePoint;   // the toe held while only the toe is down
            public Vector3 Fade, FadeSpeed;
            public Vector3 FadeFrom;   // the release's offset, faded out over the swing
            public float FadeTime, FadeOver;
            public bool Stepping;      // let go while it should be down: a quick step onto the clip's place
            public float LockedFor;
            public Vector3 Target;     // where the ankle goes this step
            public float YawHold;      // degrees added to the clip foot's turn: a planted foot keeps its own
            public float LockYaw, YawFrom;
            public Vector3 Posed;      // where the clip alone put it
            public float Released;     // the last release's pull (metres: how far the lock held it from the clip)
            public int Releases, ForcedReleases;
            public bool StepArc;       // a step to its place taken as one arc (standing): lift, over, down
            public bool StepLanded;    // that step is over where it lands, and coming down onto it
            public Vector3 StepLand;   // where that is (world)
            public bool Hopping;       // that step is its part of a hop: let go with the other foot, down with it
            public float HopFrom;      // how high it was when the hop took it up, already in a step of its own (metres)
            public float LandedOff;    // the last landing's leftover offset from the clip's foot (metres)
            public int Landings;
        }

        public readonly FootLock[] Feet = { new FootLock(), new FootLock() };

        /// <summary>The point of a foot on the ground (world): its heel's, or its toe's while it stands on its toe.</summary>
        public Vector3 ContactPoint(int f)
        {
            var foot = anim.Part(f == 0 ? PartId.FootL : PartId.FootR);
            return Feet[f].OnToe ? foot.position + foot.rotation * restToe : foot.position;
        }

        private readonly Vector3 restToe;

        /// <summary>Debugging: each lock's release is printed (the runner sets it).</summary>
        public static System.Action<string> Trace;

        /// <summary>How far the hips were dropped for the legs to reach (metres).</summary>
        public float PelvisDrop { get; private set; }

        private float dropSpeed;

        /// <summary>A lock lets go if the clip's foot has gone this far from it (leg lengths): the leg would over-stretch.</summary>
        /// <summary>A planted foot is let go when it is this far from the clip's (leg lengths), or out of the leg's reach.</summary>
        public const float UnlockRadius = 0.5f;

        public LocoBody(AnimRig anim, Locomotor loco)
        {
            loco.RootTurned += OnRootTurned;
            this.anim = anim;
            this.loco = loco;
            loco.ShotPace = BladePace;
            pelvisAboveHips = -anim.RestLocalPosition(PartId.ThighL).y;
            legReach = anim.RestLocalPosition(PartId.ShinL).magnitude + anim.RestLocalPosition(PartId.FootL).magnitude;

            // The foot's resting line (heel to toe tip, against the root) in the idle, with its turn out
            // taken off: a foot that points as it does at rest is posed flat and straight.
            var idle = LocoLibrary.Get("idle");
            Vector3 d = idle != null ? idle.Point(LocoBone.LeftToeEnd, 0f) - idle.Point(LocoBone.LeftFoot, 0f) : new Vector3(0f, -0.08f, 0.15f);
            restFootDir = new Vector3(0f, d.y, new Vector2(d.x, d.z).magnitude).normalized;
            Vector3 t = idle != null ? idle.Point(LocoBone.LeftToeBase, 0f) - idle.Point(LocoBone.LeftFoot, 0f) : new Vector3(0f, -0.08f, 0.12f);
            restToe = new Vector3(0f, t.y, new Vector2(t.x, t.z).magnitude) * loco.Leg;
            // (The heel's underside: under the ankle, a little behind it.)
            restHeel = new Vector3(0f, restToe.y, -0.2f * restToe.z);
            soleToe = restToe;
        }

        private Quaternion RootTurn => Quaternion.Euler(0f, loco.Facing, 0f);

        /// <summary>
        /// The root's facing as what is kept to it sees it (the shield's way, the knees' forward): a combo's link turns
        /// the root at once (<see cref="Locomotor.TurnRoot"/>), the pose kept where it is, and these follow it over
        /// <see cref="RootTurnEase"/> rather than snapping round (the shield hand jumped 30 m/s at a 70 degree link).
        /// </summary>
        private Quaternion SmoothRootTurn => Quaternion.Euler(0f, loco.Facing - rootTurnLeft, 0f);

        private float rootTurnLeft;

        /// <summary>The time a link's root turn takes to reach what is kept to the root's facing (seconds, the e-folding).</summary>
        public float RootTurnEase = 0.08f;

        private Vector3 restHeel, soleToe;

        /// <summary>
        /// The foot's sole as the body's foot collider has it (a box, in the foot's own frame): its heel
        /// and toe corners are kept above the ground. Without it, the clip's toe and a heel under the ankle.
        /// </summary>
        public void SoleFrom(UnitRig rig)
        {
            var box = rig != null ? rig.Part(PartId.FootL).partCollider as BoxCollider : null;
            if (box == null)
            {
                return;
            }

            Vector3 c = box.center, half = 0.5f * box.size;
            float bottom = c.y - half.y;
            restHeel = new Vector3(0f, bottom, c.z - half.z);
            soleToe = new Vector3(0f, bottom, c.z + half.z);
        }

        private Vector3 RawWorld(LocoBone bone) => loco.Position + RootTurn * loco.RawPoints[(int)bone] + Vector3.up * GroundY;

        private Vector3 World(LocoBone bone) => loco.Position + RootTurn * loco.Points[(int)bone] + Vector3.up * GroundY;

        /// <summary>Poses the rig for this step (after the locomotor's step).</summary>
        public void Pose(float dt)
        {
            poseDt = dt;
            rootTurnLeft *= Mathf.Exp(-dt / Mathf.Max(1e-3f, RootTurnEase));
            HeldArms();
            StepHandKnock(dt);
            StepBind(dt);
            StepRest(dt);
            UpdateAimWeights(dt);
            // The feet first (the hips drop for them).
            UpdateFeet(dt);
            Pelvis(out Vector3 pelvisAt, out var pelvisTurn);
            pelvisAt += ShiftHips(pelvisAt, pelvisTurn, dt);
            pelvisAt.y += HopHips(dt);
            float drop = 0f;
            for (int f = 0; f < 2; f++)
            {
                Vector3 hip = HipPoint(f, pelvisAt, pelvisTurn);
                Vector3 to = Feet[f].Target - hip;
                float flat = new Vector2(to.x, to.z).sqrMagnitude;
                float reach = 0.995f * legReach;
                float most = Mathf.Sqrt(Mathf.Max(0f, reach * reach - flat));
                drop = Mathf.Max(drop, -to.y - most);
            }

            float wantDrop = Mathf.Clamp(drop, 0f, 0.12f * legReach);
            // (Down at once, up again smoothly: a leg is never left short of its foot.)
            if (wantDrop >= PelvisDrop)
            {
                PelvisDrop = wantDrop;
                dropSpeed = 0f;
            }
            else
            {
                float x = PelvisDrop;
                Springs.Track(ref x, ref dropSpeed, wantDrop, 0.12f, dt);
                PelvisDrop = x;
            }

            anim.Part(PartId.Pelvis).SetPositionAndRotation(pelvisAt + Vector3.down * PelvisDrop, pelvisTurn);

            // The spine, the chest and the head.
            anim.Part(PartId.Spine).rotation = SpineRotation;
            anim.Part(PartId.Chest).rotation = ChestRotation;
            anim.Part(PartId.Head).rotation = HeadRotation;

            // The arms: the hands where the clip's are against the shoulders, the elbows toward the clip's.
            Arm(PartId.UpperArmL, PartId.ForearmL, PartId.HandL, LocoBone.LeftArm, LocoBone.LeftForeArm, LocoBone.LeftHand,
                LocoBone.LeftHandMiddle, LocoBone.LeftHandIndex, LocoBone.LeftHandPinky);
            Arm(PartId.UpperArmR, PartId.ForearmR, PartId.HandR, LocoBone.RightArm, LocoBone.RightForeArm, LocoBone.RightHand,
                LocoBone.RightHandMiddle, LocoBone.RightHandIndex, LocoBone.RightHandPinky);
            RestNow();
            if (TwoHanded)
            {
                GripTwoHanded();
            }

            if (CarryL == Carry.Shield)
            {
                // (In the guard and its strikes the shield faces the way the body does, at the enemy, a little out.)
                FaceShieldAt(SmoothRootTurn * Quaternion.Euler(0f, -ShieldOut, 0f) * Vector3.forward, ClipArms * ShieldTurned);
            }

            // The legs.
            Leg(0, PartId.ThighL, PartId.ShinL, PartId.FootL, LocoBone.LeftUpLeg, LocoBone.LeftLeg, LocoBone.LeftFoot, LocoBone.LeftToeEnd);
            Leg(1, PartId.ThighR, PartId.ShinR, PartId.FootR, LocoBone.RightUpLeg, LocoBone.RightLeg, LocoBone.RightFoot, LocoBone.RightToeEnd);
        }

        // ---- The walk's parts, for a pose built elsewhere (the fight's pose pipeline) -----------------

        /// <summary>The feet's locks for this step: where each ankle goes (<see cref="FootLock.Target"/>).</summary>
        public void UpdateFeet(float dt)
        {
            clock += dt;
            shiftAsked = false;
            for (int f = 0; f < 2; f++)
            {
                HoldFoot(f, dt);
            }

            if (hopAsked)
            {
                // (Both feet have been asked. Neither left the ground, each already in a step of its own: no hop.)
                hopAsked = false;
                if (!Feet[0].Hopping && !Feet[1].Hopping)
                {
                    hopOver = 0f;
                    loco.NoLead = false;
                    Hops--;
                }
            }

            if (!shiftAsked)
            {
                shiftsFor = -1;
            }
        }

        // ---- The weight over the feet (the plan "Grounded legs", step 3) ---------------------------------------

        /// <summary>
        /// The weight shifts before a foot lifts (the scene's Combat Feel, "Weight Shifts"). Standing in its stance, the
        /// hips go toward the foot that carries the body while the other steps, and a step taken from standing waits
        /// a moment for them to. The stance is wide: with the hips kept in its middle a foot lifted while all the
        /// body's weight was half a stance away from the one left standing.
        /// </summary>
        public static bool WeightShifts = true;

        /// <summary>
        /// How far the hips go toward the carrying foot, as a share of the way from where the stance has them to over
        /// that foot (<see cref="ShiftShare"/>, standing or going slowly), and how much of that is left while the body
        /// is going at a step's pace (<see cref="ShiftGoing"/>: there each step is a fifth of a second and the body is
        /// carried by its going, toward the foot it has lifted).
        /// </summary>
        public static float ShiftShare = DefaultShiftShare;

        public const float DefaultShiftShare = 0.4f, ShiftGoing = 0.3f;

        /// <summary>How long the hips take over it (seconds, the half of the way), and how long a step from standing waits for them before its foot lifts.</summary>
        public static float ShiftTime = DefaultShiftTime, ShiftWaits = DefaultShiftWaits;

        public const float DefaultShiftTime = 0.08f, DefaultShiftWaits = 0.14f;

        /// <summary>How far the hips are shifted now from where the clip has them (world, flat, metres).</summary>
        public Vector3 HipShift => hipShift;

        /// <summary>Steps from standing that waited for the weight to shift (for the reports).</summary>
        public int ShiftedSteps { get; private set; }

        /// <summary>
        /// Standing on both feet with the body's weight off what they hold: which way, and how far, it has to go to be
        /// over them (world, flat, metres; zero when it is; set each step by whoever reads the body's weight). The hips
        /// are placed over the support: they go that way until the weight is on it (<see cref="BalanceGain"/> of what
        /// is left a second, <see cref="MostBalance"/> leg lengths at most), and ease back when it would stay on it.
        /// </summary>
        public Vector3 WeightOff;

        public const float BalanceGain = 6f, MostBalance = 0.14f, BalanceEases = 4f;

        private Vector3 balance;

        private Vector3 hipShift, hipShiftSpeed;
        private int shiftsFor = -1;
        private float shiftWaited;
        private bool shiftAsked;

        /// <summary>Stepping in its stance, standing: where the weight is shifted.</summary>
        private bool ShiftsNow => WeightShifts && Locomotor.StepsInStanceOn && loco.QuickSteps && loco.Current == Locomotor.Mode.Idle;

        private Vector3 ShiftHips(Vector3 pelvisAt, Quaternion pelvisTurn, float dt)
        {
            Vector3 want = Vector3.zero;
            if (ShiftsNow)
            {
                // (The foot that carries: the one down while the other steps, or the one a waiting step will leave down.)
                bool left = Feet[0].Locked, right = Feet[1].Locked;
                int carries = left != right ? (left ? 0 : 1) : left && shiftsFor >= 0 ? 1 - shiftsFor : -1;
                if (carries >= 0)
                {
                    float pace = loco.Going.magnitude / Mathf.Sqrt(loco.Leg / 0.85f);
                    float going = Mathf.Clamp01((pace - 0.2f) / 0.5f);
                    float share = ShiftShare * Mathf.Lerp(1f, ShiftGoing, going * going * (3f - 2f * going));
                    Vector3 to = Feet[carries].Target - pelvisAt;
                    to.y = 0f;
                    want = Vector3.ClampMagnitude(to * share, 0.35f * loco.Leg);
                }
            }

            // (Both feet down: the hips over what they hold.)
            if (ShiftsNow && Feet[0].Locked && Feet[1].Locked && shiftsFor < 0)
            {
                Vector3 off = WeightOff;
                off.y = 0f;
                balance = off.sqrMagnitude > 1e-8f
                    ? Vector3.ClampMagnitude(balance + off * Mathf.Min(1f, BalanceGain * dt), MostBalance * loco.Leg)
                    : balance * Mathf.Exp(-dt / BalanceEases);
            }
            else
            {
                balance *= Mathf.Exp(-dt / (ShiftsNow ? 0.6f : 0.2f));
            }

            want += balance;
            Springs.Track(ref hipShift, ref hipShiftSpeed, want, Mathf.Max(0.02f, ShiftTime), dt);
            hipShift.y = 0f;
            // (No further than both legs reach, the hips let down as far as they are for a leg: a leg past its reach
            // is what the engine cannot take.)
            for (int tries = 0; tries < 4 && hipShift.sqrMagnitude > 1e-8f; tries++)
            {
                bool reaches = true;
                for (int f = 0; f < 2 && reaches; f++)
                {
                    Vector3 to = Feet[f].Target - HipPoint(f, pelvisAt + hipShift, pelvisTurn);
                    float deep = Mathf.Max(0f, -to.y - 0.1f * legReach);
                    reaches = new Vector2(to.x, to.z).sqrMagnitude + deep * deep <= 0.96f * legReach * legReach;
                }

                if (reaches)
                {
                    break;
                }

                hipShift *= tries == 3 ? 0f : 0.6f;
                hipShiftSpeed = Vector3.zero;
            }

            return hipShift;
        }

        /// <summary>Lets go of both feet at once (the walk hands the body over, or is handed it).</summary>
        /// <summary>Lets go of one foot (a leg swept by a blow: it steps again from where it is thrown).</summary>
        public void ReleaseFoot(int f)
        {
            var foot = Feet[f];
            foot.Locked = false;
            foot.OnToe = false;
            foot.Stepping = false;
        }

        public void ReleaseFeet()
        {
            foreach (var foot in Feet)
            {
                foot.Locked = false;
                foot.OnToe = false;
                foot.Stepping = false;
                foot.Fade = foot.FadeFrom = foot.FadeSpeed = Vector3.zero;
                foot.FadeTime = foot.FadeOver = 0f;
                foot.YawHold = foot.YawFrom = 0f;
            }

            PelvisDrop = 0f;
            dropSpeed = 0f;
            hipShift = hipShiftSpeed = balance = Vector3.zero;
            shiftsFor = -1;
        }

        /// <summary>The walk's pelvis (world): its place (before any drop for the legs) and its turn, from the hips' line and the lower spine.</summary>
        public void Pelvis(out Vector3 position, out Quaternion rotation)
        {
            Vector3 hipsMid = 0.5f * (World(LocoBone.LeftUpLeg) + World(LocoBone.RightUpLeg));
            Vector3 hipsRight = World(LocoBone.RightUpLeg) - World(LocoBone.LeftUpLeg);
            rotation = Frame(World(LocoBone.Spine1) - World(LocoBone.Hips), hipsRight);
            position = hipsMid + rotation * new Vector3(0f, pelvisAboveHips, 0f);
        }

        private Vector3 HipPoint(int f, Vector3 pelvisAt, Quaternion pelvisTurn)
        {
            float halfHips = 0.5f * (World(LocoBone.RightUpLeg) - World(LocoBone.LeftUpLeg)).magnitude;
            return pelvisAt - pelvisTurn * new Vector3(0f, pelvisAboveHips, 0f) + pelvisTurn * new Vector3(f == 0 ? -halfHips : halfHips, 0f, 0f);
        }

        /// <summary>The walk's spine, chest and head (world turns).</summary>
        public Quaternion SpineRotation
        {
            get
            {
                Vector3 hipsRight = World(LocoBone.RightUpLeg) - World(LocoBone.LeftUpLeg);
                Vector3 shoulderRight = World(LocoBone.RightArm) - World(LocoBone.LeftArm);
                return Frame(World(LocoBone.Spine2) - World(LocoBone.Spine), hipsRight.normalized + shoulderRight.normalized);
            }
        }

        public Quaternion ChestRotation => Frame(World(LocoBone.Neck) - World(LocoBone.Spine2), World(LocoBone.RightArm) - World(LocoBone.LeftArm));

        public Quaternion HeadRotation => Frame(World(LocoBone.HeadTop) - World(LocoBone.Head), World(LocoBone.RightEye) - World(LocoBone.LeftEye));

        /// <summary>The walk's foot turn (world): along the clip's foot, flat at rest.</summary>
        public Quaternion FootRotation(int f) => Quaternion.Euler(0f, Feet[f].YawHold, 0f) * ClipFootRotation(f);

        /// <summary>The clip's foot turn on our foot (world), before a planted foot's own turn is kept.</summary>
        private Quaternion ClipFootRotation(int f)
        {
            Vector3 dir = Quaternion.Inverse(RootTurn) * (World(f == 0 ? LocoBone.LeftToeEnd : LocoBone.RightToeEnd) - World(f == 0 ? LocoBone.LeftFoot : LocoBone.RightFoot));
            return RootTurn * Quaternion.FromToRotation(restFootDir, dir.normalized);
        }

        /// <summary>A planted foot this far from the clip's (leg lengths), standing, steps over to it; after it has been down this long (seconds), and not within this long of the last such step.</summary>
        public float SettleRadius = 0.12f, SettleAfter = 0.1f, SettleGap = 0.3f;

        /// <summary>Stepping in the stance (<see cref="Locomotor.StepsInStance"/>), a foot steps this long after the other set off (seconds): as it lands.</summary>
        public float StepGap => StepTime + 0.02f;

        /// <summary>How long a step to its place takes, lifting to landing (seconds; the scene's Combat Feel, "Step Takes").</summary>
        public static float StepTime = DefaultStepTime;

        public const float DefaultStepTime = 0.22f;

        /// <summary>
        /// A step to its place: the shares of its time the foot only lifts before it goes, and only comes down after
        /// it is over where it lands; and how high it goes at its middle (metres at a Medium's size; the scene's Combat
        /// Feel, "Step Height").
        /// </summary>
        public const float StepLifts = 0.1f, StepLands = 0.2f;

        public static float StepHeight = DefaultStepHeight;
        /// <summary>A step come down over where it lands waits this long (seconds) for the ground to take it up; a clip that began under it and has the foot in the air doesn't, and the foot goes back onto the clip's path.</summary>
        public const float StepWaitsToLand = 0.05f;

        public const float DefaultStepHeight = 0.06f;

        private float lastSettle = -10f, clock;

        private float Time() => clock;

        /// <summary>Standing still: the standing clip, or a guard or strike clip playing (they plant their own feet).</summary>
        private bool Standing => loco.Current == Locomotor.Mode.Idle || (loco.Current == Locomotor.Mode.OneShot && loco.Shot != null && loco.Shot.IsGuard && loco.Shot.Role != "turn");

        /// <summary>A planted foot keeps its turn while the body turns over it, up to this (degrees); past it the foot pivots.</summary>
        public const float MostYawHold = 35f;

        /// <summary>Which way the knee goes (world, from the hip-to-ankle line): the clip's bend carried onto the leg from <paramref name="hip"/> to its target.</summary>
        public Vector3 KneePole(int f, Vector3 hip)
        {
            var hipBone = f == 0 ? LocoBone.LeftUpLeg : LocoBone.RightUpLeg;
            var knee = f == 0 ? LocoBone.LeftLeg : LocoBone.RightLeg;
            var ankle = f == 0 ? LocoBone.LeftFoot : LocoBone.RightFoot;
            Vector3 clipAxis = World(ankle) - World(hipBone);
            Vector3 ourAxis = Feet[f].Target - hip;
            Vector3 bend = Vector3.ProjectOnPlane(World(knee) - World(hipBone), clipAxis.sqrMagnitude > 1e-8f ? clipAxis.normalized : Vector3.down);
            Vector3 pole = Quaternion.FromToRotation(clipAxis, ourAxis) * bend;
            // (A nearly straight clip leg says little about which way its knee goes: its bend counts in
            // full only from a tenth of a leg off the line, so a straight leg's knee goes forward, not
            // whichever way a few millimetres point.)
            float says = Mathf.Clamp01(pole.magnitude / (0.1f * loco.Leg));
            return (pole.sqrMagnitude > 1e-8f ? pole.normalized * says : Vector3.zero) + 0.3f * (SmoothRootTurn * Vector3.forward);
        }

        /// <summary>A frame (rest: up +y, right +x) turned so its up lies along <paramref name="up"/> and its right toward <paramref name="right"/>.</summary>
        private static Quaternion Frame(Vector3 up, Vector3 right)
        {
            Vector3 forward = Vector3.Cross(right, up);
            if (forward.sqrMagnitude < 1e-8f || up.sqrMagnitude < 1e-8f)
            {
                return Quaternion.identity;
            }

            return Quaternion.LookRotation(forward.normalized, up.normalized);
        }

        // ---- What the hands carry -----------------------------------------------------------------------

        /// <summary>What a hand carries while it walks: nothing (the clip's arm as it is), a weapon, a shield.</summary>
        public enum Carry
        {
            Free,
            Weapon,
            Shield,
        }

        /// <summary>What the right and the left hand carry.</summary>
        public Carry CarryR = Carry.Free, CarryL = Carry.Free;

        /// <summary>A carried weapon's line in its hand's own frame (the slot's forward, as gripped).</summary>
        public Vector3 BladeR = Vector3.forward, BladeL = Vector3.forward;

        /// <summary>
        /// A carrying arm at rest: the way its upper arm and its forearm point, in the body's frame (x out
        /// to its side, y up, z forward). The weapon arm hangs a little out and back, the forearm forward
        /// and down, so with the wrist straight the weapon's point is up and forward; the shield arm's
        /// elbow is a little out, the forearm forward and well out, so the shield faces that way, clear
        /// of the body.
        /// </summary>
        public Vector3 WeaponUpper = new Vector3(0.12f, -0.97f, -0.18f), WeaponFore = new Vector3(0.1f, -0.45f, 0.89f);
        public Vector3 ShieldUpper = new Vector3(0.12f, -0.97f, -0.1f), ShieldFore = new Vector3(0.4f, -0.15f, 0.9f);

        /// <summary>Which way a carried shield faces at rest (the body's frame, x out): forward and well out, the wrist tipping it past its forearm.</summary>
        public Vector3 ShieldFaces = new Vector3(0.72f, -0.05f, 0.7f);

        /// <summary>How much of the turn the arm's swing gives a carried thing the wrist takes back out (1: none is left).</summary>
        public float StabiliseWrist = 0.8f;

        /// <summary>How much of the clip's arm swing a carrying arm keeps (the rhythm stays; a heavy or held thing swings less).</summary>
        public float WeaponSwing = 0.75f, ShieldSwing = 0.4f;


        private float poseDt = 0.02f;
        private readonly Vector3[] swingMean = new Vector3[2];
        private readonly bool[] swingKnown = new bool[2];

        // Each elbow's way across its shoulder-to-hand line, against the body's turn: it turns at most
        // MostElbowTurn a second. A hand brought down past its shoulder sweeps that line through the
        // clip's elbow's way, and the elbow would be flung round to the other side at once.
        private readonly Vector3[] elbowWay = new Vector3[2];
        private readonly bool[] elbowKnown = new bool[2];

        // Each arm's upper arm and forearm as its solve last left them (against their parents): what it is solved from.
        private readonly Quaternion[] solvedUpper = new Quaternion[2], solvedFore = new Quaternion[2];
        private readonly bool[] armSolved = new bool[2];

        /// <summary>The fastest an elbow's way turns (degrees a second).</summary>
        public float MostElbowTurn = 540f;

        /// <summary>The root turned at once (a combo's link): the directions kept against it turned back, so the body doesn't turn with it.</summary>
        private void OnRootTurned(float degrees)
        {
            rootTurnLeft += degrees;
            var backTurn = Quaternion.Euler(0f, -degrees, 0f);
            for (int i = 0; i < heldApplied.Length; i++)
            {
                heldApplied[i] = backTurn * heldApplied[i];
                armRelease[i] = backTurn * armRelease[i];
            }

            var back = Quaternion.Euler(0f, -degrees, 0f);
            var there = Quaternion.Euler(0f, degrees, 0f);
            for (int a = 0; a < 2; a++)
            {
                elbowWay[a] = back * elbowWay[a];
                swingMean[a] = back * swingMean[a];
                lastReach[a] = back * lastReach[a];
                reachFade[a] = back * reachFade[a] * there;
            }
        }

        private Vector3 SteadyElbow(int a, Vector3 pole, Vector3 shoulder, Vector3 target)
        {
            // (Which way the elbow points is what the pole says across the shoulder-to-hand line; it is
            // that, kept against the body's turn, which turns at most MostElbowTurn a second.)
            var turn = RootTurn;
            Vector3 axis = target - shoulder;
            if (axis.sqrMagnitude < 1e-8f)
            {
                return pole;
            }

            axis.Normalize();
            Vector3 across = Vector3.ProjectOnPlane(pole, axis);
            if (!elbowKnown[a])
            {
                if (across.sqrMagnitude > 1e-8f)
                {
                    elbowWay[a] = Quaternion.Inverse(turn) * across.normalized;
                    elbowKnown[a] = true;
                }

                return pole;
            }

            Vector3 was = Vector3.ProjectOnPlane(turn * elbowWay[a], axis);
            if (was.sqrMagnitude < 1e-6f)
            {
                was = across;
            }

            Vector3 now = across.sqrMagnitude > 1e-8f && pole.magnitude > 1e-4f && across.magnitude > 0.2f * pole.magnitude
                ? Vector3.RotateTowards(was.normalized, across.normalized, MostElbowTurn * Mathf.Deg2Rad * poseDt, 0f)
                : was.normalized;
            elbowWay[a] = Quaternion.Inverse(turn) * now;
            return now;
        }

        /// <summary>
        /// How much the arms are the clip's own even where a hand carries something (0: the carry's;
        /// 1: the clip's): a guard or strike clip's hands hold the weapons themselves, so while one plays
        /// the arms go over to it, and come back to the carry after.
        /// </summary>
        public float ClipArms { get; private set; }

        /// <summary>How long the arms take to go over to a guard clip's, or back (seconds).</summary>
        public const float ClipArmsBlend = 0.3f;

        private void UpdateClipArms()
        {
            // (A guard or strike clip playing, or the guard's own cycles: the clips' hands hold the weapons.)
            bool guardClip = loco.Current == Locomotor.Mode.OneShot && loco.Shot != null && loco.Shot.IsGuard;
            float wanted = guardClip ? 1f : loco.Guard != null ? loco.GuardWeight : 0f;
            ClipArms = Mathf.MoveTowards(ClipArms, wanted, poseDt / ClipArmsBlend);
        }

        private void Arm(PartId upperId, PartId foreId, PartId handId, LocoBone shoulder, LocoBone elbow, LocoBone wrist,
            LocoBone middle, LocoBone index, LocoBone pinky)
        {
            var upper = anim.Part(upperId);
            var fore = anim.Part(foreId);
            var hand = anim.Part(handId);
            bool right = handId == PartId.HandR;
            if (right)
            {
                UpdateClipArms();
            }

            int a = right ? 0 : 1;
            var carry = right ? CarryR : CarryL;
            float clipShare = carry == Carry.Free ? 1f : ClipArms;
            // (The clip's arm from the current motion's own pose, its switch faded as a turn of the arm
            // about the shoulder and a change in its reach (FadeReach): faded point by point, a hand
            // brought down from overhead would pass by its shoulder, and the arm's way swing round.)
            Vector3 s = RawWorld(shoulder), e = RawWorld(elbow), w = RawWorld(wrist);
            float clipArm = (e - s).magnitude + (w - e).magnitude;
            float ourArm = anim.RestLocalPosition(foreId).magnitude + anim.RestLocalPosition(handId).magnitude;
            Vector3 reach = (w - s) * (ourArm / Mathf.Max(1e-3f, clipArm));
            Vector3 clipPole = e - 0.5f * (s + w);
            var reachTurn = FadeReach(a, ref reach);
            clipPole = reachTurn * clipPole;

            // The clip's arm: the hand where the clip's is against the shoulder, the elbow toward the
            // clip's, the hand turned as the clip's (wrist to the middle knuckle, its thumb side the clip's).
            Vector3 clipTarget = upper.position + reach;
            var clipHand = hand.rotation;
            bool clipHandKnown = false;
            {
                // (The hand's turn from the current motion's own fingers, not the faded ones: fading the
                // fingers' places between two very different grips can turn the hand over. The turn is
                // faded on its own below.)
                Vector3 rw = RawWorld(wrist);
                Vector3 along = RawWorld(middle) - rw;
                Vector3 side = RawWorld(index) - RawWorld(pinky);
                Vector3 restAlong = anim.RestLocalPosition(handId).normalized;   // (the hand carries on the forearm's line at rest)
                var slot = right ? anim.SlotWeaponR : anim.SlotWeaponL;
                if (slot != null && slot.localPosition.sqrMagnitude > 1e-6f)
                {
                    restAlong = slot.localPosition.normalized;
                }

                if (along.sqrMagnitude > 1e-8f && side.sqrMagnitude > 1e-8f)
                {
                    var want = Quaternion.LookRotation(along.normalized, Vector3.Cross(along, side).normalized);
                    var rest = Quaternion.LookRotation(restAlong, Vector3.Cross(restAlong, Vector3.forward).normalized);
                    clipHand = want * Quaternion.Inverse(rest);
                    clipHandKnown = true;
                }
            }

            var turn = RootTurn;
            Vector3 swing = Vector3.zero;
            Vector3 restFore = Vector3.zero;   // (the forearm's way at rest, world)
            Vector3 target = clipTarget, pole = clipPole;
            if (clipShare < 1f)
            {
                // The carrying arm: the clip's swing kept, smaller, about where the carried thing is held:
                // its swing is the hand's way off its own mean over the last second or so (the clip's
                // rhythm, whatever the cycle).
                Vector3 rel = Quaternion.Inverse(turn) * reach / ourArm;
                if (!swingKnown[a])
                {
                    swingMean[a] = rel;
                    swingKnown[a] = true;
                }

                swingMean[a] = Vector3.Lerp(swingMean[a], rel, 1f - Mathf.Exp(-poseDt / 0.8f));
                swing = rel - swingMean[a];
                Vector3 up = (carry == Carry.Weapon ? WeaponUpper : ShieldUpper).normalized;
                Vector3 down = (carry == Carry.Weapon ? WeaponFore : ShieldFore).normalized;
                if (!right)
                {
                    up.x = -up.x;
                    down.x = -down.x;
                }

                float upperLength = anim.RestLocalPosition(foreId).magnitude, foreLength = anim.RestLocalPosition(handId).magnitude;
                Vector3 elbowAt = upper.position + turn * up * upperLength;
                Vector3 restHand = elbowAt + turn * down * foreLength;
                restFore = turn * down;
                Vector3 carryTarget = restHand + turn * swing * ((carry == Carry.Weapon ? WeaponSwing : ShieldSwing) * ourArm);
                // (The elbow where the rest pose has it: the forearm keeps its way as the arm swings.)
                Vector3 carryPole = elbowAt - 0.5f * (upper.position + carryTarget);
                target = Vector3.Lerp(carryTarget, clipTarget, clipShare);
                pole = Vector3.Lerp(carryPole.normalized, clipPole.normalized, clipShare);
            }

            // (An aimed strike: the weapon hand pulled toward the line through its target, most at contact.)
            if ((aimClip != null || pullLeftWeight > 0f) && (right || TwoHanded))
            {
                target += aimPull * AimWeight() + pullLeft * pullLeftWeight;
            }

            // (A knock on the weapon: the hand thrown, the elbow giving. Bound to a parrying blade: held off its clip
            // so its weapon stays on that blade.)
            if (right || TwoHanded)
            {
                target += handKnock + bindOffset + restOffset;
            }

            pole = SteadyElbow(a, pole, upper.position, target);
            // (Solved from where this solve last left the arm, not from where what came after it moved it. Which
            // way a limb near straight is bent is read from a few millimetres of elbow; a hand pushed a little
            // further by a weapon at rest on a shield, or a blow's bend, moves those, and the solve, bending the
            // arm that way and twisting its elbow back to its pole, rolled a straight arm about its own length
            // by up to 78 degrees a step, on and on while the weapon lay there.)
            if (armSolved[a])
            {
                upper.localRotation = solvedUpper[a];
                fore.localRotation = solvedFore[a];
            }

            TwoBoneIK.Solve(upper, fore, hand, target, pole);
            solvedUpper[a] = upper.localRotation;
            solvedFore[a] = fore.localRotation;
            armSolved[a] = true;
            if (clipShare >= 1f)
            {
                if (clipHandKnown)
                {
                    hand.rotation = FadeHand(a, clipHand);
                }

                if (right)
                {
                    SteerBlade(hand);
                    LimitBlade(hand);
                }

                return;
            }

            // The carrying hand: the wrist straight, as a hand holding something is (a weapon's angle is
            // the grip's on the forearm's; a shield faces the way the forearm points), but for two things:
            // it takes out most of the turn the arm's swing gives the forearm (StabiliseWrist: the weapon
            // and the shield keep facing much the same way against the body as it walks, not quite, or it
            // would look mounted on a rig), and a shield is tipped a little further out than its forearm
            // (ShieldFaces). A weapon's twist about the forearm points it the way the body goes, a little out.
            var held = fore.rotation;
            Vector3 foreNow = (hand.position - fore.position).normalized;
            if (restFore.sqrMagnitude > 1e-6f)
            {
                held = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(foreNow, restFore), StabiliseWrist) * held;
            }

            Vector3 handAlong = held * anim.RestLocalPosition(handId).normalized;
            if (carry == Carry.Shield)
            {
                Vector3 faces = ShieldFaces.normalized;
                faces.x *= right ? 1f : -1f;
                held = Quaternion.FromToRotation(restFore, turn * faces) * held;
            }
            else
            {
                Vector3 blade = Vector3.ProjectOnPlane(held * (right ? BladeR : BladeL), handAlong);
                Vector3 wantWay = Vector3.ProjectOnPlane(turn * Quaternion.Euler(0f, right ? 8f : -8f, 0f) * Vector3.forward, handAlong);
                if (blade.sqrMagnitude > 1e-6f && wantWay.sqrMagnitude > 1e-6f)
                {
                    float twist = Mathf.Clamp(Vector3.SignedAngle(blade, wantWay, handAlong), -MostWristTwist, MostWristTwist);
                    held = Quaternion.AngleAxis(twist, handAlong) * held;
                }
            }

            hand.rotation = FadeHand(a, clipHandKnown && clipShare > 0f ? Quaternion.Slerp(held, clipHand, clipShare) : held);
            if (right)
            {
                SteerBlade(hand);
                LimitBlade(hand);
            }
        }

        /// <summary>
        /// An aimed strike's wrist: about contact the weapon hand turns the blade toward the target, as far as
        /// a wrist would for a cut (<see cref="MostBladeSteer"/>); a thrust is pointed at it all the way
        /// (<see cref="MostThrustSteer"/>: the clips' thrusting hands don't say where their blade points).
        /// </summary>
        private void SteerBlade(Transform hand)
        {
            float w = SteerWeight();
            if (w <= 0f)
            {
                return;
            }

            var slot = anim.SlotWeaponR;
            Vector3 grip = slot != null ? slot.position : hand.position;
            Vector3 blade = hand.rotation * BladeR;
            Vector3 want = AimTarget - grip;
            if (want.sqrMagnitude < 1e-4f || blade.sqrMagnitude < 1e-6f)
            {
                return;
            }

            float most = IsThrust(aimClip) ? MostThrustSteer : MostBladeSteer;
            Vector3 axis = Vector3.Cross(blade, want);
            if (axis.sqrMagnitude < 1e-10f)
            {
                return;
            }

            float angle = Mathf.Min(Vector3.Angle(blade, want), most) * w;
            hand.rotation = Quaternion.AngleAxis(angle, axis.normalized) * hand.rotation;
        }

        // A strike's blade as the pose last left it, and how far the pose wanted its point to go for each second of
        // its clip over the last step.
        private Vector3 bladeTipWas;
        private LocoClip bladeTipClip;
        private float bladeTipTime, bladePerClipSecond;

        /// <summary>
        /// The share of the blade's speed limit a strike's clip is paced for: under 1, the clip is slowed a little more
        /// than the limit asks, so a blade the limit has left behind its pose has speed to spare to catch it up with.
        /// </summary>
        public static float BladePaceShare = 0.8f;

        /// <summary>How far behind the pose its blade's point was left by its speed limit this step (metres; 0: on it).</summary>
        public float BladeBehind { get; private set; }

        /// <summary>
        /// A strike's blade goes no faster than <see cref="CombatFeel.MostBladeSpeed"/> (the clips whip it through their
        /// fastest frames at two or three times what an arm can swing a sword at). Where the pose would swing its point
        /// further than that in this step, the weapon hand is turned back: the point goes as far along its way as the
        /// limit lets it. What the pose wanted is noted, and the clip is slowed by it from the next step
        /// (<see cref="BladePace"/>), so the body waits for its blade and the blade is back on its pose within a step or two.
        /// </summary>
        private void LimitBlade(Transform hand)
        {
            float most = CombatFeel.MostBladeSpeed;
            var clip = loco.Current == Locomotor.Mode.OneShot ? loco.Shot : null;
            var slot = anim.SlotWeaponR;
            BladeBehind = 0f;
            if (most <= 0f || clip == null || clip.ContactFrame < 0 || slot == null)
            {
                bladeTipClip = null;
                bladePerClipSecond = 0f;
                return;
            }

            Vector3 grip = slot.position, way = slot.rotation * Vector3.forward;
            Vector3 tip = grip + way * WeaponLength;
            if (bladeTipClip != clip)
            {
                bladePerClipSecond = 0f;
            }
            else
            {
                float far = (tip - bladeTipWas).magnitude, went = loco.ShotTime - bladeTipTime;
                if (went > 1e-4f)
                {
                    bladePerClipSecond = far / went;
                }

                float reach = most * poseDt;
                if (far > reach && far > 1e-5f)
                {
                    Vector3 to = bladeTipWas + (tip - bladeTipWas) * (reach / far) - grip;
                    if (to.sqrMagnitude > 1e-8f)
                    {
                        hand.rotation = Quaternion.FromToRotation(way, to.normalized) * hand.rotation;
                        BladeBehind = far - reach;
                        tip = slot.position + slot.rotation * Vector3.forward * WeaponLength;
                    }
                }
            }

            bladeTipWas = tip;
            bladeTipTime = loco.ShotTime;
            bladeTipClip = clip;
        }

        /// <summary>
        /// How fast a strike's clip plays (0..1 of its own pace) for its blade's point to go no faster than
        /// <see cref="CombatFeel.MostBladeSpeed"/>: by what the pose did with it over the last step (how far it wanted
        /// the point to go, the way it had fallen behind as well). 1 with no limit set, and for what isn't a strike.
        /// </summary>
        public float BladePace(LocoClip clip, float time)
        {
            float most = CombatFeel.MostBladeSpeed;
            most *= BladePaceShare;
            return most > 0f && bladeTipClip == clip && bladePerClipSecond > most ? most / bladePerClipSecond : 1f;
        }

        private static bool IsThrust(LocoClip clip) => clip != null && clip.Kind != null && clip.Kind.StartsWith("thrust");

        /// <summary>How much of the wrist's steer is on now (worked out once a pose: <see cref="UpdateAimWeights"/>).</summary>
        private float SteerWeight() => steerWeight;

        private float aimWeight, steerWeight, aimFrom;

        /// <summary>How much of the aim's pull is on now (for the logs).</summary>
        public float AimWeightNow => aimWeight;

        // (A combo's strike taken up while the last one's pull is still on: that pull is let go over AimLetGo.)
        private Vector3 pullLeft;
        private float pullLeftWeight;

        /// <summary>How long an aimed strike's pull and steer take to let go when it ends early (cut short).</summary>
        public const float AimLetGo = 0.25f;

        /// <summary>
        /// The aim's pull: rising through the wind-up, all of it at contact, gone by the end; the wrist's
        /// steer: a thrust's from halfway through its wind-up, a cut's just before contact, let go after. A
        /// strike that ends early lets both go over <see cref="AimLetGo"/>, not at once.
        /// </summary>
        private void UpdateAimWeights(float dt)
        {
            pullLeftWeight = Mathf.MoveTowards(pullLeftWeight, 0f, dt / AimLetGo);
            if (aimClip == null)
            {
                aimWeight = steerWeight = 0f;
                return;
            }

            if (loco.Shot != aimClip)
            {
                aimWeight = Mathf.MoveTowards(aimWeight, 0f, dt / AimLetGo);
                steerWeight = Mathf.MoveTowards(steerWeight, 0f, dt / AimLetGo);
                if (aimWeight <= 0f && steerWeight <= 0f)
                {
                    aimClip = null;
                }

                return;
            }

            float t = loco.ShotTime, c = aimClip.ContactFrame / aimClip.Rate, end = aimClip.Duration;
            float u = t <= c ? Mathf.Clamp01((t - aimFrom) / Mathf.Max(0.05f, c - aimFrom)) : 1f - Mathf.Clamp01((t - c) / Mathf.Max(0.05f, end - c));
            aimWeight = u * u * (3f - 2f * u);
            float from = Mathf.Max(aimFrom, IsThrust(aimClip) ? 0.4f * c : Mathf.Max(0f, c - 0.15f));
            float v = t <= c ? Mathf.Clamp01((t - from) / Mathf.Max(0.05f, c - from)) : 1f - Mathf.Clamp01((t - c) / 0.25f);
            steerWeight = v * v * (3f - 2f * v);
        }

        // Each arm's reach (shoulder to hand, against the body's turn) faded at each switch: the turn
        // and the length the last pose had and the new one hasn't, let go together.
        private readonly Quaternion[] reachFade = { Quaternion.identity, Quaternion.identity };
        private readonly float[] reachFadeLength = new float[2];
        private readonly Vector3[] lastReach = new Vector3[2];
        private readonly int[] reachSwitches = new int[2];
        private readonly bool[] reachKnown = new bool[2];

        /// <returns>The fade's turn now (world), to turn the elbow's way by as well.</returns>
        private Quaternion FadeReach(int a, ref Vector3 reach)
        {
            var turn = RootTurn;
            Vector3 local = Quaternion.Inverse(turn) * reach;
            if (reachKnown[a] && reachSwitches[a] != loco.Switches && local.sqrMagnitude > 1e-8f && lastReach[a].sqrMagnitude > 1e-8f)
            {
                reachFade[a] = Quaternion.FromToRotation(local, lastReach[a]);
                reachFadeLength[a] = lastReach[a].magnitude - local.magnitude;
            }

            reachSwitches[a] = loco.Switches;
            float keep = Mathf.Pow(0.5f, poseDt / Mathf.Max(0.02f, loco.SwitchHalfLifeNow));
            reachFade[a] = Quaternion.Slerp(Quaternion.identity, reachFade[a], keep);
            reachFadeLength[a] *= keep;
            float length = Mathf.Max(0f, local.magnitude + reachFadeLength[a]);
            local = reachFade[a] * (local.sqrMagnitude > 1e-8f ? local.normalized : Vector3.down) * length;
            lastReach[a] = local;
            reachKnown[a] = true;
            reach = turn * local;
            return turn * reachFade[a] * Quaternion.Inverse(turn);
        }

        // Each hand's turn faded at each switch between motions, as the walk fades the places: what the
        // last pose had and the new one hasn't is kept, then let go.
        private readonly Quaternion[] lastHand = { Quaternion.identity, Quaternion.identity };
        private readonly Quaternion[] handFade = { Quaternion.identity, Quaternion.identity };
        private readonly int[] handSwitches = new int[2];
        private readonly bool[] handKnown = new bool[2];
        private readonly float[] lastHandU = new float[2];

        /// <summary>A strike's hand has let go of the last motion's turn by this long before contact (seconds of its clip).</summary>
        public float HandFadeGoneBefore = 0.08f;

        private Quaternion FadeHand(int a, Quaternion want)
        {
            if (handKnown[a] && handSwitches[a] != loco.Switches)
            {
                handFade[a] = lastHand[a] * Quaternion.Inverse(want);
            }

            handSwitches[a] = loco.Switches;
            handFade[a] = Quaternion.Slerp(handFade[a], Quaternion.identity, 1f - Mathf.Pow(0.5f, poseDt / Mathf.Max(0.02f, loco.SwitchHalfLifeNow)));
            // (A strike taken up late in its wind-up (a combo's next blow): the turn the last pose had is let go by just
            // before contact, whatever is left of it, so the weapon meets its target the way the strike swings it. Left
            // to the fade alone, a long club still pointed a metre off when the blow landed.)
            if (aimClip != null && loco.Shot == aimClip && aimClip.ContactFrame >= 0)
            {
                float c = aimClip.ContactFrame / aimClip.Rate - HandFadeGoneBefore;
                float u = Mathf.Clamp01((loco.ShotTime - aimFrom) / Mathf.Max(0.05f, c - aimFrom));
                handFade[a] = Quaternion.Slerp(Quaternion.identity, handFade[a], (1f - u) / Mathf.Max(1e-3f, 1f - lastHandU[a]));
                lastHandU[a] = u < 1f ? u : 0f;
            }
            else
            {
                lastHandU[a] = 0f;
            }

            var shown = handFade[a] * want;
            lastHand[a] = shown;
            handKnown[a] = true;
            return shown;
        }

        // ---- Aimed strikes (the combat plan's step 3) ---------------------------------------------------

        /// <summary>The weapon's length (metres) and where along it a strike meets its target (its share of the length).</summary>
        public float WeaponLength = 0.9f, SweetSpot = 0.7f;

        /// <summary>How far an aimed cut's wrist turns its blade toward the target (degrees), and a thrust's.</summary>
        public float MostBladeSteer = 30f, MostThrustSteer = 110f;

        /// <summary>A target for a strike played at nothing in particular: ahead at chest height, where the strikes' reach is measured.</summary>
        public Vector3 NominalTarget() => new Vector3(loco.Position.x, GroundY, loco.Position.z) + Quaternion.Euler(0f, loco.Facing, 0f) * new Vector3(0f, NominalHeight * loco.Leg, NominalReach * loco.Leg);

        /// <summary>The nominal target's height and distance (leg lengths: about 1.3 m up and 1.5 m ahead of a Medium).</summary>
        public const float NominalHeight = 1.54f, NominalReach = 1.78f;

        /// <summary>Which body this is ("Small", "Medium", "Large"): its strikes' measured reach is looked up by it.</summary>
        public string BodyKey = "Medium";

        /// <summary>How much a held shield is turned in the guard to face the way the body does (not all of it: it moves with the arm), and how far out to the side of straight ahead (degrees).</summary>
        public float ShieldTurned = 0.85f, ShieldOut = 12f;

        private Quaternion shieldGrip;
        private bool shieldGripKnown;

        /// <summary>The shield's slot as it is gripped, unturned (against the hand); null before it was first turned.</summary>
        public Quaternion? ShieldGripRest => shieldGripKnown ? shieldGrip : (Quaternion?)null;

        /// <summary>
        /// The shield turned in the hand to face <paramref name="way"/> (by <paramref name="share"/>: 0 holds it as
        /// gripped): its face (the slot's forward) toward it. A round shield held by its middle turns about the fist.
        /// </summary>
        public void FaceShieldAt(Vector3 way, float share)
        {
            var slot = anim.SlotWeaponL;
            if (slot == null)
            {
                return;
            }

            if (!shieldGripKnown)
            {
                shieldGrip = slot.localRotation;
                shieldGripKnown = true;
            }

            slot.localRotation = shieldGrip;
            // (Fixed in the fist (the scene's Combat Feel): the board stays as the hand grips it, and looks where the hand has it.)
            if (CombatFeel.ShieldFixed || share <= 0f || way.sqrMagnitude < 1e-6f)
            {
                return;
            }

            Vector3 face = slot.rotation * Vector3.forward;
            slot.rotation = Quaternion.Slerp(Quaternion.identity, Quaternion.FromToRotation(face, way.normalized), Mathf.Clamp01(share)) * slot.rotation;
        }

        /// <summary>How far below the weapon hand the other hand holds a two-handed weapon (metres at a Medium's size: the clips' hands are 0.10 to 0.15 m apart).</summary>
        public float SecondGrip = 0.12f;

        /// <summary>
        /// A two-handed weapon: in the guard and its strikes (the clips' own hands, <see cref="ClipArms"/>),
        /// the left hand is put on the handle below the right, turned to hold it as the right does. The
        /// clips' own left hands are there, near enough: on our weapon they would float beside it.
        /// </summary>
        private void GripTwoHanded()
        {
            float share = CarryL == Carry.Free ? ClipArms : 0f;
            var slotR = anim.SlotWeaponR;
            var slotL = anim.SlotWeaponL;
            if (share <= 0f || slotR == null)
            {
                return;
            }

            var upper = anim.Part(PartId.UpperArmL);
            var fore = anim.Part(PartId.ForearmL);
            var hand = anim.Part(PartId.HandL);
            Vector3 blade = (slotR.rotation * Vector3.forward).normalized;
            float size = loco.Leg / 0.845f;
            Vector3 gripAt = slotR.position - blade * (SecondGrip * size);
            // (The left hand turned so its grip runs along the handle the way the right's does.)
            Vector3 leftWay = hand.rotation * BladeL;
            var turnTo = Quaternion.FromToRotation(leftWay, blade);
            var wantHand = Quaternion.Slerp(hand.rotation, turnTo * hand.rotation, share);
            Vector3 palm = slotL != null ? slotL.localPosition : Vector3.zero;
            Vector3 wrist = gripAt - wantHand * palm;
            Vector3 pole = fore.position - 0.5f * (upper.position + hand.position);
            TwoBoneIK.Solve(upper, fore, hand, Vector3.Lerp(hand.position, wrist, share), pole);
            hand.rotation = wantHand;
        }

        /// <summary>Both hands hold the weapon (the Large's two-handed grip): both are pulled to aim it.</summary>
        public bool TwoHanded;

        /// <summary>The most a strike's hand is pulled to aim it (metres), and the most the body turns toward its target (degrees); its lunge is stretched or shortened within these.</summary>
        public float MostAimPull = 0.4f, MostAimTurn = 60f, LeastLunge = 0.4f, MostLunge = 1.4f;

        /// <summary>The last aimed strike's plan: its turn (degrees), its lunge's stretch, and how far its weapon would have missed with no pull (metres).</summary>
        public float AimTurn, AimLunge, AimMissBeforePull;

        public Vector3 AimTarget { get; private set; }

        /// <summary>Where the plan put the weapon's sweet spot at contact before the hand's pull (world).</summary>
        public Vector3 AimPredicted { get; private set; }

        /// <summary>Where the strike will land, with the arm's pull (world): the target, or as near it as the pull goes.</summary>
        public Vector3 AimLands { get; private set; }

        private LocoClip aimClip;
        private Vector3 aimPull;
        private readonly Vector3[] clipPose = new Vector3[LocoClip.BoneCount];

        /// <summary>
        /// Starts a strike clip aimed at a point (world): the body turns toward it over the wind-up, the
        /// lunge is stretched or shortened so contact is at its distance, and the weapon hand is pulled the
        /// rest of the way, most at contact. The feet step as the clip's do.
        /// </summary>
        /// <param name="plan">False: played as it is, no turn, stretch or pull (the wrist still steers toward the target).</param>
        /// <param name="startFrom">Taken up from this far into the clip (seconds): a combo's next strike, its wind-up met part way.</param>
        /// <param name="stepIn">A combo's longer step in on top of the strike's travel (world, metres; <see cref="Locomotor.PlayAimed"/>): planned from where it ends.</param>
        public void PlayStrike(LocoClip clip, Vector3 target, bool plan = true, float startFrom = 0f, Vector3 stepIn = default)
        {
            if (clip == null || clip.ContactFrame < 0)
            {
                return;
            }

            startFrom = Mathf.Clamp(startFrom, 0f, Mathf.Max(0f, clip.ContactFrame / clip.Rate - 0.05f));
            if (aimClip != null && aimWeight > 0f)
            {
                pullLeft = aimPull * aimWeight + pullLeft * pullLeftWeight;
                pullLeftWeight = 1f;
            }

            if (!plan)
            {
                aimPull = Vector3.zero;
                AimTurn = 0f;
                AimLunge = 1f;
                AimMissBeforePull = 0f;
                AimTarget = target;
                aimClip = clip;
                aimFrom = startFrom;
                loco.PlayAimed(clip, 1f, 0f, clip.ContactFrame / clip.Rate, startFrom);
                return;
            }

            float contact = clip.ContactFrame / clip.Rate;
            stepIn.y = 0f;
            Vector3 rootAt = loco.Position + stepIn;
            Plan(clip, target, startFrom, rootAt, loco.Facing, out float k, out float by, out Vector3 predicted, out Vector3 want);
            var turn0 = Quaternion.Euler(0f, loco.Facing, 0f);
            Vector3 miss = want - predicted;
            AimMissBeforePull = miss.magnitude;
            aimPull = turn0 * Quaternion.Euler(0f, by, 0f) * Vector3.ClampMagnitude(Quaternion.Inverse(Quaternion.Euler(0f, by, 0f)) * miss, MostAimPull);
            AimTurn = by;
            AimLunge = k;
            planGripWorld = new Vector3(rootAt.x, GroundY, rootAt.z) + turn0 * planGrip;
            KeepHandOff();
            AimPredicted = new Vector3(rootAt.x, GroundY, rootAt.z) + turn0 * predicted;
            AimTarget = target;
            AimLands = AimPredicted + aimPull;
            aimClip = clip;
            aimFrom = startFrom;
            loco.PlayAimed(clip, k, by, contact, startFrom, stepIn);
        }

        /// <summary>
        /// A strike's plan at a target from where the body is now: the turn and the lunge's stretch, and where the
        /// weapon would be at contact without the arm's pull, against the root now (<paramref name="predicted"/>),
        /// with where the target is against it (<paramref name="want"/>).
        /// </summary>
        private void Plan(LocoClip clip, Vector3 target, float startFrom, out float k, out float by, out Vector3 predicted, out Vector3 want)
            => Plan(clip, target, startFrom, loco.Position, loco.Facing, out k, out by, out predicted, out want);

        private void Plan(LocoClip clip, Vector3 target, float startFrom, Vector3 rootAt, float facing, out float k, out float by, out Vector3 predicted, out Vector3 want)
        {
            float contact = clip.ContactFrame / clip.Rate;
            clip.RootMotion(0f, contact, out var moved, out float turned);
            Vector3 travel = new Vector3(moved.x, 0f, moved.y) * loco.Leg;
            // (Where the weapon is at contact against the start, played as it is on this body: measured once
            // by the headless runner, StrikeReach.json; without it, worked out from the clip's hand.)
            bool gripKnown = StrikeReach.TryGet(BodyKey, clip.Name, out var grip, out var bladeWay);
            Vector3 point = gripKnown
                ? grip + bladeWay * (WeaponLength * SweetSpot) - travel
                : Quaternion.Euler(0f, turned, 0f) * ClipWeaponPoint(clip, contact);
            Vector3 gripAt = gripKnown ? grip - travel : point;
            if (startFrom > 0f)
            {
                // (Taken up part way: the travel and the reach against where the root is then.)
                clip.RootMotion(0f, startFrom, out var before, out float turnedBefore);
                Vector3 rootThen = new Vector3(before.x, 0f, before.y) * loco.Leg;
                var back = Quaternion.Euler(0f, -turnedBefore, 0f);
                Vector3 atContact = travel + point, gripAtContact = travel + gripAt;
                travel = back * (travel - rootThen);
                point = back * (atContact - rootThen) - travel;
                gripAt = back * (gripAtContact - rootThen) - travel;
            }
            var turn0 = Quaternion.Euler(0f, facing, 0f);
            want = Quaternion.Inverse(turn0) * (target - new Vector3(rootAt.x, GroundY, rootAt.z));
            Vector3 wantFlat = new Vector3(want.x, 0f, want.z);
            k = 1f;
            by = 0f;
            for (int i = 0; i < 3; i++)
            {
                Vector3 q = k * travel + point;
                Vector3 qFlat = new Vector3(q.x, 0f, q.z);
                by = Mathf.Clamp(Vector3.SignedAngle(qFlat, wantFlat, Vector3.up), -MostAimTurn, MostAimTurn);
                // (The lunge takes up the difference in distance, as far as it goes that way.)
                Vector3 way = qFlat.sqrMagnitude > 1e-6f ? qFlat.normalized : Vector3.forward;
                float along = Vector3.Dot(travel, way);
                if (along > 0.05f)
                {
                    k = Mathf.Clamp(k + (wantFlat.magnitude - qFlat.magnitude) / along, LeastLunge, MostLunge);
                }
            }

            // (Never into the other body: the lunge is cut short where its root would come nearer the body kept
            // from than KeepDistance. The arm's pull and the blade's reach take up what it can.)
            if (KeepFrom.HasValue && KeepDistance > 0f)
            {
                Vector3 keep = Quaternion.Inverse(turn0) * (KeepFrom.Value - rootAt);
                keep.y = 0f;
                var byTurn = Quaternion.Euler(0f, by, 0f);
                for (int i = 0; i < 20 && k > ShortestLunge; i++)
                {
                    Vector3 rootThere = byTurn * (k * travel);
                    if ((keep - rootThere).magnitude >= KeepDistance)
                    {
                        break;
                    }

                    k = Mathf.Max(ShortestLunge, k - 0.06f);
                }
            }

            predicted = Quaternion.Euler(0f, by, 0f) * (k * travel + point);
            planGrip = Quaternion.Euler(0f, by, 0f) * (k * travel + gripAt);
            planGripKnown = gripKnown;
        }

        // (The plan's grip at contact, against the root as planned from, and in the world; whether it is known: StrikeReach has it.)
        private Vector3 planGrip, planGripWorld;
        private bool planGripKnown;

        /// <summary>How near the other body's middle the weapon hand may be pulled at contact (metres at a Medium's size, across the ground): the torso and the hand, clear.</summary>
        public const float HandKeep = 0.32f;

        /// <summary>
        /// The aim's pull never takes the weapon hand into the other body (<see cref="KeepFrom"/>): a blow that would
        /// need it to lands short instead, as a cut from too close does.
        /// </summary>
        private void KeepHandOff()
        {
            if (!KeepFrom.HasValue || !planGripKnown)
            {
                return;
            }

            Vector3 off = planGripWorld + aimPull - KeepFrom.Value;
            off.y = 0f;
            float near = HandKeep * (loco.Leg / 0.845f);
            if (off.magnitude >= near)
            {
                return;
            }

            Vector3 way = off.sqrMagnitude > 1e-6f ? off.normalized : -(Quaternion.Euler(0f, loco.Facing, 0f) * Vector3.forward);
            aimPull = Vector3.ClampMagnitude(aimPull + way * (near - off.magnitude), MostAimPull);
        }

        /// <summary>The other body's place (world; null: none): an aimed strike's lunge stops short of coming nearer it than <see cref="KeepDistance"/> (metres, root to root).</summary>
        public Vector3? KeepFrom;
        public float KeepDistance;

        /// <summary>The shortest a lunge may be cut to, to keep that distance.</summary>
        public const float ShortestLunge = 0.15f;

        // ---- A jumping strike stopped in the air: the weapon arm held off its follow-through ------------------------

        private static readonly LocoBone[] RightArmBones = { LocoBone.RightForeArm, LocoBone.RightHand, LocoBone.RightHandIndex, LocoBone.RightHandMiddle, LocoBone.RightHandPinky };
        private static readonly LocoBone[] LeftArmBones = { LocoBone.LeftForeArm, LocoBone.LeftHand, LocoBone.LeftHandIndex, LocoBone.LeftHandMiddle, LocoBone.LeftHandPinky };
        private readonly Vector3[] heldArm = new Vector3[LocoClip.BoneCount], endArm = new Vector3[LocoClip.BoneCount];
        private LocoClip armHeldIn;

        /// <summary>How long a held weapon arm takes to come back to the clip's guard (seconds of the clip).</summary>
        public const float ArmBackOver = 0.4f;
        private float armHeldFrom;

        /// <summary>
        /// The playing strike was stopped but plays on (a jump met in the air comes down): the weapon arm is held
        /// where it was stopped, against its shoulder, and comes back up to where the clip's ends (its guard) over
        /// <see cref="ArmBackOver"/>, never through the follow-through the blow no longer has.
        /// </summary>
        public void HoldWeaponArm()
        {
            if (loco.Current != Locomotor.Mode.OneShot || loco.Shot == null)
            {
                return;
            }

            var raw = loco.RawPoints;
            armHeldIn = loco.Shot;
            armHeldFrom = loco.ShotTime;
            armHeldIn.Pose(armHeldIn.Duration, endArm, loco.Leg);
            foreach (var b in RightArmBones)
            {
                heldArm[(int)b] = raw[(int)b] - raw[(int)LocoBone.RightArm];
                endArm[(int)b] -= endArm[(int)LocoBone.RightArm];
            }

            foreach (var b in LeftArmBones)
            {
                heldArm[(int)b] = raw[(int)b] - raw[(int)LocoBone.LeftArm];
                endArm[(int)b] -= endArm[(int)LocoBone.LeftArm];
            }
        }

        private void HeldArms()
        {
            if (armHeldIn == null)
            {
                ReleasedArms();
                return;
            }

            if (loco.Current != Locomotor.Mode.OneShot || loco.Shot != armHeldIn)
            {
                // (Let go into the next motion (a combo's next blow): where the held arm was, faded out as a switch is,
                // not dropped in a frame.)
                var now = loco.RawPoints;
                foreach (var b in RightArmBones)
                {
                    armRelease[(int)b] = heldApplied[(int)b] - (now[(int)b] - now[(int)LocoBone.RightArm]);
                }

                foreach (var b in LeftArmBones)
                {
                    armRelease[(int)b] = TwoHanded ? heldApplied[(int)b] - (now[(int)b] - now[(int)LocoBone.LeftArm]) : Vector3.zero;
                }

                armReleaseWeight = 1f;
                armHeldIn = null;
                ReleasedArms();
                return;
            }

            float u = Mathf.Clamp01((loco.ShotTime - armHeldFrom) / Mathf.Clamp(armHeldIn.Duration - armHeldFrom, 0.1f, ArmBackOver));
            float w = u * u * (3f - 2f * u);
            var raw = loco.RawPoints;
            foreach (var b in RightArmBones)
            {
                heldApplied[(int)b] = Vector3.Lerp(heldArm[(int)b], endArm[(int)b], w);
                raw[(int)b] = raw[(int)LocoBone.RightArm] + heldApplied[(int)b];
            }

            if (TwoHanded)
            {
                foreach (var b in LeftArmBones)
                {
                    heldApplied[(int)b] = Vector3.Lerp(heldArm[(int)b], endArm[(int)b], w);
                    raw[(int)b] = raw[(int)LocoBone.LeftArm] + heldApplied[(int)b];
                }
            }
        }

        private readonly Vector3[] heldApplied = new Vector3[LocoClip.BoneCount], armRelease = new Vector3[LocoClip.BoneCount];
        private float armReleaseWeight;

        /// <summary>A held arm let go: what it was off the new motion's, faded out over the switch's half-life.</summary>
        private void ReleasedArms()
        {
            if (armReleaseWeight <= 1e-3f)
            {
                armReleaseWeight = 0f;
                return;
            }

            var raw = loco.RawPoints;
            foreach (var b in RightArmBones)
            {
                raw[(int)b] += armRelease[(int)b] * armReleaseWeight;
            }

            foreach (var b in LeftArmBones)
            {
                raw[(int)b] += armRelease[(int)b] * armReleaseWeight;
            }

            armReleaseWeight *= Mathf.Pow(0.5f, poseDt / Mathf.Max(0.03f, loco.SwitchHalfLifeNow));
        }

        // ---- A blow's knock on the weapon arm (a parry or a block met it): the hand thrown back, and back again -----

        private Vector3 handKnock, handKnockV;

        /// <summary>How quickly a knocked weapon hand comes back (Hz, critically damped), and the most it is thrown (m at a Medium's size).</summary>
        public const float HandKnockFrequency = 3f, MostHandKnock = 0.18f;

        /// <summary>The weapon hand thrown by a meeting of blades or a shield (m/s at the hand): the arm takes it, the elbow giving, the shoulder going back with it.</summary>
        public void KnockHand(Vector3 velocity)
        {
            handKnockV += velocity;
        }

        /// <summary>
        /// The weapon held on a blade it is bound to (the plan "Parries that divert"): the weapon hand moved this far
        /// off where its clip puts it (world) for as long as the bind lasts; let go, it eases back to the clip.
        /// </summary>
        public void Bind(Vector3 offset)
        {
            bindOffset = offset;
            bound = true;
        }

        /// <summary>The bind over: the hand eases back to its clip (most of the way in <see cref="BindLetGo"/> times three).</summary>
        public void LetGo() => bound = false;

        /// <summary>
        /// Its weapon resting against what it touches outside a blow (the plan "Shield and blade contact", step 6): the
        /// weapon hand held off its clip by <paramref name="offset"/> (world, the way out and the depth the weapon would go
        /// in), eased into and out of so it doesn't snap.
        /// </summary>
        public void RestAgainst(Vector3 offset) => restWant = offset;

        /// <summary>The weapon hand held off its clip by its weapon resting against a body or a shield (world).</summary>
        public Vector3 RestOffset => restOffset;

        /// <summary>How quickly a weapon comes to rest against what it touches, and lets go of it (seconds, the e-folding).</summary>
        public const float RestIn = 0.01f, RestOut = 0.08f;

        private Vector3 restOffset, restWant;

        /// <summary>Asked once the weapon arm is posed: how far its weapon is into what it touches (world, the way out and the depth; its striker's, outside its blows).</summary>
        public System.Func<Vector3> RestProbe;

        /// <summary>The weapon arm put back out of what its weapon would go into, in the same step: the hand moved out, its turn kept.</summary>
        private void RestNow()
        {
            if (RestProbe == null || TwoHanded)
            {
                return;
            }

            Vector3 push = RestProbe();
            if (push.sqrMagnitude < 1e-8f)
            {
                return;
            }

            Transform upper = anim.Part(PartId.UpperArmR), fore = anim.Part(PartId.ForearmR), hand = anim.Part(PartId.HandR);
            var turn = hand.rotation;
            Vector3 target = hand.position + push;
            // (The elbow kept on the side of the arm it is on. Taken as the elbow's place against the line to where
            // the hand is put, a straight arm's elbow, a few millimetres off its line, was on the other side of the
            // new one: the arm was rolled about its own length by up to half a turn, and back by the next step's pose.)
            Vector3 pole = TwoBoneIK.BendWay(upper, fore, hand);
            TwoBoneIK.Solve(upper, fore, hand, target, pole.sqrMagnitude > 1e-8f ? pole : RootTurn * Vector3.right);
            hand.rotation = turn;
            restOffset += push;
        }

        private void StepRest(float dt)
        {
            float t = restWant.sqrMagnitude >= restOffset.sqrMagnitude ? RestIn : RestOut;
            restOffset = Vector3.Lerp(restOffset, restWant, 1f - Mathf.Exp(-dt / t));
        }

        /// <summary>How fast a let-go hand eases back to its clip (seconds, the e-folding).</summary>
        public const float BindLetGo = 0.08f;

        private Vector3 bindOffset;
        private bool bound;

        private void StepBind(float dt)
        {
            if (!bound)
            {
                bindOffset *= Mathf.Exp(-dt / BindLetGo);
            }
        }

        private void StepHandKnock(float dt)
        {
            float w = 2f * Mathf.PI * HandKnockFrequency;
            float decay = Mathf.Exp(-w * dt);
            Vector3 c = handKnockV + w * handKnock;
            Vector3 next = (handKnock + c * dt) * decay;
            handKnockV = (handKnockV - c * (w * dt)) * decay;
            handKnock = Vector3.ClampMagnitude(next, MostHandKnock * (loco.Leg / 0.845f));
        }

        /// <summary>How far an aimed strike would miss its target even with all its pull (metres; 0: it lands).</summary>
        public float MissAfterPull(LocoClip clip, Vector3 target, float startFrom = 0f) => MissAfterPull(clip, target, startFrom, loco.Position, loco.Facing);

        /// <summary>How far a strike from a root there would miss before the arm's pull takes any of it up (metres): a combo's step in plans on this, the pull only for what it can't reach.</summary>
        public float MissBeforePull(LocoClip clip, Vector3 target, float startFrom, Vector3 rootAt, float facing)
        {
            if (clip == null || clip.ContactFrame < 0)
            {
                return float.MaxValue;
            }

            startFrom = Mathf.Clamp(startFrom, 0f, Mathf.Max(0f, clip.ContactFrame / clip.Rate - 0.05f));
            Plan(clip, target, startFrom, rootAt, facing, out _, out _, out Vector3 predicted, out Vector3 want);
            return (want - predicted).magnitude;
        }

        /// <summary>
        /// A strike's plan from a root there (a combo's next blow, read coming): the turn onto its target (degrees), the
        /// lunge's stretch, and the arm's pull onto it as far as the pull goes (world), as <see cref="PlayStrike"/> takes them;
        /// and how far the swing would miss it without the pull (<paramref name="miss"/>, world: from where it lands to the target).
        /// </summary>
        public void PlanFrom(LocoClip clip, Vector3 target, float startFrom, Vector3 rootAt, float facing, out float by, out float k, out Vector3 pull, out Vector3 miss)
        {
            by = 0f;
            k = 1f;
            pull = miss = Vector3.zero;
            if (clip == null || clip.ContactFrame < 0)
            {
                return;
            }

            startFrom = Mathf.Clamp(startFrom, 0f, Mathf.Max(0f, clip.ContactFrame / clip.Rate - 0.05f));
            Plan(clip, target, startFrom, rootAt, facing, out k, out by, out Vector3 predicted, out Vector3 want);
            miss = Quaternion.Euler(0f, facing, 0f) * (want - predicted);
            pull = Vector3.ClampMagnitude(miss, MostAimPull);
        }

        /// <summary>
        /// Where a strike planned from a root will hold its blade at contact (world): the grip, the arm's
        /// <paramref name="pull"/> added, and the blade's way, as the wrist will have steered it at the target. What a
        /// fighter reads of a combo's next blow coming (the plan "Shield and blade contact", step 4). False when the
        /// strike's reach on this body isn't known (StrikeReach).
        /// </summary>
        public bool PlannedBlade(LocoClip clip, Vector3 target, float startFrom, Vector3 rootAt, float facing, Vector3 pull, out Vector3 grip, out Vector3 way)
        {
            grip = way = Vector3.zero;
            if (clip == null || clip.ContactFrame < 0)
            {
                return false;
            }

            startFrom = Mathf.Clamp(startFrom, 0f, Mathf.Max(0f, clip.ContactFrame / clip.Rate - 0.05f));
            // (Planning it leaves the plan of the strike playing as it was.)
            Vector3 keepGrip = planGrip;
            bool keepKnown = planGripKnown;
            Plan(clip, target, startFrom, rootAt, facing, out _, out _, out Vector3 predicted, out _);
            Vector3 gripAt = planGrip;
            bool known = planGripKnown;
            planGrip = keepGrip;
            planGripKnown = keepKnown;
            if (!known)
            {
                return false;
            }

            var ground = new Vector3(rootAt.x, GroundY, rootAt.z);
            var turn = Quaternion.Euler(0f, facing, 0f);
            grip = ground + turn * gripAt + pull;
            way = turn * (predicted - gripAt);
            if (way.sqrMagnitude < 1e-8f)
            {
                return false;
            }

            way.Normalize();
            Vector3 want = target - grip;
            Vector3 axis = Vector3.Cross(way, want);
            if (want.sqrMagnitude > 1e-4f && axis.sqrMagnitude > 1e-10f)
            {
                way = Quaternion.AngleAxis(Mathf.Min(Vector3.Angle(way, want), IsThrust(clip) ? MostThrustSteer : MostBladeSteer), axis.normalized) * way;
            }

            return true;
        }

        /// <summary>The same, from a root somewhere else (a combo's link: the root turned and moved there).</summary>
        public float MissAfterPull(LocoClip clip, Vector3 target, float startFrom, Vector3 rootAt, float facing)
        {
            if (clip == null || clip.ContactFrame < 0)
            {
                return float.MaxValue;
            }

            startFrom = Mathf.Clamp(startFrom, 0f, Mathf.Max(0f, clip.ContactFrame / clip.Rate - 0.05f));
            Plan(clip, target, startFrom, rootAt, facing, out _, out _, out Vector3 predicted, out Vector3 want);
            return Mathf.Max(0f, (want - predicted).magnitude - MostAimPull);
        }

        /// <summary>The body was snapped by <paramref name="move"/> (world) toward where its strike lands on its target: the weapon's way there goes with it, and the arm's pull needs that much less.</summary>
        public void SnappedBy(Vector3 move)
        {
            AimPredicted += move;
            planGripWorld += move;
            aimPull = Vector3.ClampMagnitude(aimPull - move, MostAimPull);
            AimLands = AimPredicted + aimPull;
        }

        public void Retarget(Vector3 target)
        {
            if (aimClip == null || loco.Current != Locomotor.Mode.OneShot || loco.Shot != aimClip || loco.ShotTime > aimClip.ContactFrame / aimClip.Rate)
            {
                return;
            }

            aimPull = Vector3.ClampMagnitude(aimPull + (target - AimTarget), MostAimPull);
            KeepHandOff();
            AimTarget = target;
            AimLands = AimPredicted + aimPull;
        }

        /// <summary>How far a strike's plan would turn the body to bring it onto its target from a root (degrees; 0: it lands as it is played).</summary>
        public float AimTurnFor(LocoClip clip, Vector3 target, float startFrom, Vector3 rootAt, float facing)
        {
            if (clip == null || clip.ContactFrame < 0)
            {
                return 0f;
            }

            startFrom = Mathf.Clamp(startFrom, 0f, Mathf.Max(0f, clip.ContactFrame / clip.Rate - 0.05f));
            Plan(clip, target, startFrom, rootAt, facing, out _, out float by, out _, out _);
            return by;
        }

        /// <summary>How much of the aim's pull is on now (worked out once a pose: <see cref="UpdateAimWeights"/>).</summary>
        private float AimWeight() => aimWeight;

        /// <summary>
        /// A blade's way (world, from <paramref name="grip"/>) as the aimed strike's wrist will have steered it at a
        /// later time of its clip (<see cref="SteerBlade"/>: toward the target about contact, a thrust's all the way).
        /// </summary>
        public Vector3 SteerAhead(float clipTime, Vector3 grip, Vector3 blade)
        {
            if (aimClip == null || loco.Shot != aimClip || blade.sqrMagnitude < 1e-8f)
            {
                return blade;
            }

            float c = aimClip.ContactFrame / aimClip.Rate;
            float from = Mathf.Max(aimFrom, IsThrust(aimClip) ? 0.4f * c : Mathf.Max(0f, c - 0.15f));
            float v = clipTime <= c ? Mathf.Clamp01((clipTime - from) / Mathf.Max(0.05f, c - from)) : 1f - Mathf.Clamp01((clipTime - c) / 0.25f);
            float w = v * v * (3f - 2f * v);
            Vector3 want = AimTarget - grip;
            Vector3 axis = Vector3.Cross(blade, want);
            if (w <= 0f || want.sqrMagnitude < 1e-4f || axis.sqrMagnitude < 1e-10f)
            {
                return blade;
            }

            float most = IsThrust(aimClip) ? MostThrustSteer : MostBladeSteer;
            return Quaternion.AngleAxis(Mathf.Min(Vector3.Angle(blade, want), most) * w, axis.normalized) * blade;
        }

        /// <summary>
        /// The weapon hand's pull (world, metres) as it will be at a later time of the playing strike's clip, the
        /// real seconds until then given: the aim's, rising to contact, and a last strike's being let go.
        /// </summary>
        public Vector3 PullAhead(float clipTime, float realSeconds)
        {
            Vector3 left = pullLeft * Mathf.Max(0f, pullLeftWeight - realSeconds / AimLetGo);
            if (aimClip == null || loco.Shot != aimClip)
            {
                return left + aimPull * Mathf.Max(0f, aimWeight - realSeconds / AimLetGo);
            }

            float c = aimClip.ContactFrame / aimClip.Rate, end = aimClip.Duration;
            float u = clipTime <= c ? Mathf.Clamp01((clipTime - aimFrom) / Mathf.Max(0.05f, c - aimFrom)) : 1f - Mathf.Clamp01((clipTime - c) / Mathf.Max(0.05f, end - c));
            return left + aimPull * (u * u * (3f - 2f * u));
        }

        /// <summary>
        /// Where a clip's weapon would be at a time (its sweet spot), against the clip's root then (metres):
        /// the hand as the arm reaches it on our body, turned as the clip's hand, holding our weapon in our grip.
        /// </summary>
        public Vector3 ClipWeaponPoint(LocoClip clip, float time)
        {
            clip.Pose(time, clipPose, loco.Leg);
            Vector3 P(LocoBone b) => clipPose[(int)b] + Vector3.up * 0f;
            Vector3 sh = P(LocoBone.RightArm), el = P(LocoBone.RightForeArm), wr = P(LocoBone.RightHand);
            float clipArm = (el - sh).magnitude + (wr - el).magnitude;
            float ourArm = anim.RestLocalPosition(PartId.ForearmR).magnitude + anim.RestLocalPosition(PartId.HandR).magnitude;
            Vector3 hand = sh + (wr - sh) * (ourArm / Mathf.Max(1e-3f, clipArm));
            Vector3 along = P(LocoBone.RightHandMiddle) - wr;
            Vector3 side = P(LocoBone.RightHandIndex) - P(LocoBone.RightHandPinky);
            var slot = anim.SlotWeaponR;
            Vector3 restAlong = slot != null && slot.localPosition.sqrMagnitude > 1e-6f ? slot.localPosition.normalized : anim.RestLocalPosition(PartId.HandR).normalized;
            var rot = Quaternion.LookRotation(along.normalized, Vector3.Cross(along, side).normalized) * Quaternion.Inverse(Quaternion.LookRotation(restAlong, Vector3.Cross(restAlong, Vector3.forward).normalized));
            Vector3 grip = hand + rot * (slot != null ? slot.localPosition : Vector3.zero);
            return grip + rot * BladeR * (WeaponLength * SweetSpot);
        }

        /// <summary>How far a carrying hand twists about its forearm to point its weapon (degrees: a forearm turns about this far).</summary>
        public const float MostWristTwist = 90f;

        private void Leg(int f, PartId thighId, PartId shinId, PartId footId, LocoBone hip, LocoBone knee, LocoBone ankle, LocoBone toeEnd)
        {
            // (The knee the clip's way, carried from the clip's leg line onto ours, and always somewhat
            // forward: a held foot far from the clip's can leave the clip's knee on the wrong side.)
            Vector3 pole = KneePole(f, anim.Part(thighId).position);
            TwoBoneIK.Solve(anim.Part(thighId), anim.Part(shinId), anim.Part(footId), Feet[f].Target, pole);
            anim.Part(footId).rotation = FootRotation(f);
        }

        /// <summary>An ankle this high above the ground (leg lengths) is in the air, whatever the motion says.</summary>
        public float AloftFrom = 0.3f;

        /// <summary>A foot's lock: on as it lands, onto the toe as the heel lifts, faded off as it lifts.</summary>
        /// <summary>A foot was planted (any body's): where, how fast the body was going (m/s), and how long since either foot was last down (s; long after a leap).</summary>
        public static event System.Action<LocoBody, int, Vector3, float, float> FootPlanted;

        private float poseClock;
        private readonly float[] liftedAt = { 0f, 0f };

        // ---- A hop: both feet off the ground together, and down together ----

        /// <summary>
        /// A hop's feet: how high they go (metres at a Medium's size), the shortest and the longest they are in the
        /// air (seconds), how far the hips rise in the air and sink as the feet take the weight again (metres at a
        /// Medium's size), and how long that sinking and coming up takes (seconds).
        /// </summary>
        public static float HopHeight = 0.07f, HopLeast = 0.14f, HopMost = 0.3f, HopRise = 0.02f, HopDip = 0.045f, HopDipTime = 0.24f;

        /// <summary>Hops taken (for the reports).</summary>
        public int Hops { get; private set; }

        /// <summary>In a hop now: from its feet leaving the ground until its hips have come up again after landing.</summary>
        public bool InHop => hopOver > 0f;

        private float hopOver, hopFor, hopLandedFor = -1f;
        private bool hopAsked;

        /// <summary>
        /// The body is about to be moved further and faster than its feet can step after one at a time (ground given to
        /// a blow at the last moment: a third of a metre in a seventh of a second). It hops: both feet leave the ground
        /// now, each goes in one arc to where the stance has it under the body as it arrives, they come down together
        /// after <paramref name="seconds"/>, and the hips sink a little as the legs take the weight. Only standing in
        /// its stance; false: it is not, and its feet are let go as the body leaves them, as before.
        /// </summary>
        public bool Hop(float seconds)
        {
            if (!loco.StepsInStance || !Standing || InHop)
            {
                return false;
            }

            hopOver = Mathf.Clamp(seconds, HopLeast, HopMost);
            hopFor = 0f;
            hopLandedFor = -1f;
            hopAsked = true;
            loco.NoLead = true;
            Hops++;
            return true;
        }

        /// <summary>The hips in a hop: up a little while the feet are off the ground, down and up again as they land.</summary>
        private float HopHips(float dt)
        {
            if (hopOver <= 0f)
            {
                return 0f;
            }

            float scale = loco.Leg / 0.85f;
            if (hopLandedFor < 0f)
            {
                hopFor += dt;
                // (Landed: a foot of the hop is on the ground again. Or it never took off, or never came down: over.)
                bool inAir = Feet[0].Hopping || Feet[1].Hopping;
                if (!inAir || hopFor > hopOver + 0.15f)
                {
                    Feet[0].Hopping = Feet[1].Hopping = false;
                    hopLandedFor = 0f;
                    loco.NoLead = false;
                    // (Landed, it stands a moment on both feet before either steps again.)
                    lastSettle = Time();
                    return 0f;
                }

                return HopRise * scale * Mathf.Sin(Mathf.PI * Mathf.Clamp01(hopFor / hopOver));
            }

            hopLandedFor += dt;
            float v = hopLandedFor / HopDipTime;
            if (v >= 1f)
            {
                hopOver = 0f;
                return 0f;
            }

            // (Down quickly, up slowly: the legs take the weight and push up again.)
            float w = v < 0.3f ? v / 0.3f : 1f - (v - 0.3f) / 0.7f;
            return -HopDip * scale * w * w * (3f - 2f * w);
        }

        private void HoldFoot(int f, float dt)
        {
            poseClock += f == 0 ? dt : 0f;
            if (Feet[f].Locked)
            {
                liftedAt[f] = poseClock;
            }

            var foot = Feet[f];
            var ankleBone = f == 0 ? LocoBone.LeftFoot : LocoBone.RightFoot;
            Vector3 posed = World(ankleBone);
            // (The toe where our own foot will put it, turned as the clip's foot is: the clip's toe is
            // where a foot of the clip's proportions has it, and holding that would slide ours.)
            var clipTurn = ClipFootRotation(f);
            float clipYaw = clipTurn.eulerAngles.y;
            foot.Posed = posed;
            bool down = loco.Down[f] >= 0.5f;
            bool heel = loco.Heel[f] >= 0.5f;
            // (A foot the motion says is down but which is still well up in the air (a leap cut short, a
            // switch from a jump into a stand) is not planted where it is: it comes down with the fade first.)
            bool aloft = posed.y - GroundY > AloftFrom * loco.Leg;
            bool landing = down && !aloft && !foot.Locked && (!foot.Stepping || foot.FadeTime >= foot.FadeOver);
            if (landing)
            {
                foot.LockYaw = clipYaw + foot.YawHold;
            }

            if (foot.Locked || landing)
            {
                // (Planted, it keeps the turn it landed with while the body turns over it, as far as a
                // foot will; past that it pivots with the body.)
                float hold = Mathf.DeltaAngle(clipYaw, foot.LockYaw);
                if (Mathf.Abs(hold) > MostYawHold)
                {
                    hold = Mathf.Sign(hold) * MostYawHold;
                    foot.LockYaw = clipYaw + hold;
                }

                foot.YawHold = hold;
            }

            Vector3 posedToe = posed + Quaternion.Euler(0f, foot.YawHold, 0f) * clipTurn * restToe;

            if (landing)
            {
                // (A foot down: heard, a step, or a landing after a while with both feet off the ground.)
                FootPlanted?.Invoke(this, f, posed, loco.Velocity.magnitude, Mathf.Min(poseClock - liftedAt[1 - f], poseClock - liftedAt[f]));
                foot.Locked = true;
                foot.LockedFor = 0f;
                foot.Stepping = false;
                // (Where it is now, faded in: a release still fading carries on into the lock. A foot
                // that lands toe first, walking backward, is held at its toe until the heel comes down.)
                foot.Ankle = posed + foot.Fade;
                foot.Hopping = false;
                if (foot.StepLanded)
                {
                    // (A step that came down onto its place lands there, not a step's drift on.)
                    foot.Ankle = new Vector3(foot.StepLand.x, foot.Ankle.y, foot.StepLand.z);
                    foot.Fade = foot.Ankle - posed;
                    foot.StepLanded = false;
                }

                foot.LandedOff = foot.Fade.magnitude;
                foot.Landings++;
                foot.OnToe = !heel && loco.Toe[f] >= 0.5f;
                foot.ToePoint = posedToe + foot.Fade;
                foot.Fade = foot.FadeSpeed = foot.FadeFrom = Vector3.zero;
            }

            if (foot.Locked)
            {
                foot.LockedFor += dt;
                if (!heel && !foot.OnToe && loco.Toe[f] >= 0.5f)
                {
                    // The heel lifts: the toe holds now.
                    foot.OnToe = true;
                    foot.ToePoint = foot.Ankle + (posedToe - posed);
                }
                else if (heel && foot.OnToe)
                {
                    // The heel comes down onto a foot that landed on its toe: the heel holds now.
                    foot.OnToe = false;
                    foot.Ankle = foot.ToePoint - (posedToe - posed);
                }

                Vector3 held = foot.OnToe ? foot.ToePoint - (posedToe - posed) : foot.Ankle;
                held.y = posed.y;   // (its height is the clip's: the heel lifts, the foot rolls)
                Vector3 pull = held - posed;
                pull.y = 0f;
                Vector3 hipLow = World(f == 0 ? LocoBone.LeftUpLeg : LocoBone.RightUpLeg) + Vector3.down * (0.12f * legReach);
                bool stretched = pull.magnitude > UnlockRadius * loco.Leg || (held - hipLow).magnitude > legReach;

                // Standing (or in a guard or strike clip) with a planted foot well away from where the
                // clip stands it (going from the relaxed stance into the guard's wider one, say): it takes
                // a short step there, one foot at a time, rather than the legs stretching to it.
                // (Stepping in the stance, the body going: the next foot goes as the last lands, and of two that both
                // want to, the one ahead the way it goes steps first, as a fencer's leading foot does.)
                bool going = loco.StepsInStance && loco.Going.sqrMagnitude > 0.02f;
                var other = Feet[1 - f];
                bool settling = Standing && foot.LockedFor > (going ? 0.03f : SettleAfter) && pull.magnitude > SettleRadius * loco.Leg
                                && other.Locked && !other.Stepping && Time() - lastSettle > (going ? StepGap : SettleGap);
                if (settling && going)
                {
                    Vector3 otherPull = other.Ankle - other.Posed;
                    otherPull.y = 0f;
                    if (otherPull.magnitude > SettleRadius * loco.Leg && otherPull.magnitude > pull.magnitude - 0.03f * loco.Leg
                        && Vector3.Dot(other.Posed - posed, loco.Going) > 0f)
                    {
                        settling = false;
                    }
                }

                if (settling && !going && ShiftsNow && ShiftWaits > 0f)
                {
                    // (A step from standing: the weight goes over to the other foot first, and then this one lifts.
                    // One foot at a time: while the other is waiting to, this one stands.)
                    if (shiftsFor >= 0 && shiftsFor != f)
                    {
                        settling = false;
                    }
                    else
                    {
                        if (shiftsFor != f)
                        {
                            shiftsFor = f;
                            shiftWaited = 0f;
                        }

                        shiftAsked = true;
                        shiftWaited += dt;
                        if (shiftWaited < ShiftWaits)
                        {
                            settling = false;
                        }
                        else
                        {
                            ShiftedSteps++;
                        }
                    }
                }

                if (settling)
                {
                    stretched = true;
                    lastSettle = Time();
                }

                // (A hop: this foot leaves the ground now, with the other.)
                bool hops = hopAsked && down && Locomotor.StepsInStanceOn && loco.QuickSteps && loco.Current == Locomotor.Mode.Idle;
                if (hops)
                {
                    stretched = true;
                    lastSettle = Time();
                }

                if (!down || stretched)
                {
                    foot.Locked = false;
                    foot.Released = pull.magnitude;
                    foot.Releases++;
                    foot.ForcedReleases += stretched ? 1 : 0;
                    foot.Fade = foot.FadeFrom = held - posed;
                    foot.FadeSpeed = Vector3.zero;
                    foot.FadeTime = 0f;
                    foot.YawFrom = foot.YawHold;
                    // (A foot that lifts swings from where it stood onto the clip's path by the time it
                    // lands; one let go while it should still be down is drawn back quickly.)
                    foot.Stepping = stretched && down;
                    foot.StepLanded = false;
                    foot.StepArc = foot.Stepping && Locomotor.StepsInStanceOn && loco.QuickSteps && loco.Current == Locomotor.Mode.Idle;
                    foot.FadeOver = foot.Stepping ? (foot.StepArc ? StepTime : 0.22f) : Mathf.Clamp(loco.SwingLeft(f), 0.12f, 0.8f);
                    foot.Hopping = hops && foot.StepArc;
                    foot.HopFrom = 0f;
                    if (foot.Hopping)
                    {
                        foot.FadeOver = hopOver;
                    }

                    Trace?.Invoke($"{(f == 0 ? "L" : "R")} released after {foot.LockedFor:0.00} s{(stretched ? " (stretched)" : "")}: pull {pull.magnitude * 100f:0.0} cm, speed {loco.Velocity.magnitude:0.00}, stride x{loco.StrideScale:0.00}, turn {loco.FacingSpeed:0} deg/s, {loco.Current}, on toe {foot.OnToe}");
                }
                else
                {
                    foot.Target = AboveGround(f, held);
                    LiftMoving(f, foot, dt);
                    return;
                }
            }

            if (hopAsked && !foot.Hopping && foot.Stepping && down && Locomotor.StepsInStanceOn && loco.QuickSteps && loco.Current == Locomotor.Mode.Idle)
            {
                // (A foot already in a step of its own as the hop begins goes with it: from where it is, as high as it
                // is, over to its place and down with the other.)
                foot.Hopping = foot.StepArc = true;
                foot.StepLanded = false;
                foot.HopFrom = Mathf.Max(0f, foot.Fade.y);
                foot.FadeFrom = new Vector3(foot.Fade.x, 0f, foot.Fade.z);
                foot.YawFrom = foot.YawHold;
                foot.FadeTime = 0f;
                foot.FadeOver = hopOver;
            }

            foot.FadeTime += dt;
            float u = Mathf.Clamp01(foot.FadeTime / Mathf.Max(0.05f, foot.FadeOver));
            // (Standing only: in a strike or a reaction the root goes as its clip throws it, and a foot let go there
            // is kept off the ground as it was until it can land; the clips' feet are the plan's step 5.)
            bool arc = foot.Stepping && foot.StepArc;
            if (arc && foot.StepLanded && (foot.FadeTime > foot.FadeOver + StepWaitsToLand
                    || (new Vector3(foot.StepLand.x, posed.y, foot.StepLand.z) - World(f == 0 ? LocoBone.LeftUpLeg : LocoBone.RightUpLeg)).magnitude > 0.97f * legReach))
            {
                // (Come down, and not taken up by the ground: a clip began under it that has this foot in the air, or
                // the body has gone a leg's reach from it. It is no longer held over where it was to land, the body
                // going on without it: it goes back onto the clip's path as a foot let go does. Held there, a walk
                // that began under a step left the foot behind until its leg was past its reach, and the engine threw
                // every body on the field.)
                foot.StepArc = foot.StepLanded = foot.Hopping = false;
                arc = false;
                foot.FadeFrom = new Vector3(foot.StepLand.x - posed.x, 0f, foot.StepLand.z - posed.z);
                foot.FadeTime = 0f;
                foot.FadeOver = 0.15f;
                u = 0f;
            }

            if (arc)
            {
                // (A step to its place is one arc (the plan "Grounded legs", step 2): the foot lifts, goes over to
                // where it lands, and comes down onto it; it does not go over the ground while it is on it. Where it
                // lands is fixed as it gets there: the body goes on, the foot comes straight down.)
                float over = Mathf.Clamp01((u - StepLifts) / (1f - StepLifts - StepLands));
                Vector3 flat;
                if (over < 1f)
                {
                    flat = foot.FadeFrom * (1f - over * over * (3f - 2f * over));
                    foot.StepLanded = false;
                }
                else
                {
                    if (!foot.StepLanded)
                    {
                        foot.StepLanded = true;
                        foot.StepLand = posed;
                    }

                    flat = foot.StepLand - posed;
                }

                foot.Fade = new Vector3(flat.x, 4f * u * (1f - u) * (foot.Hopping ? HopHeight : StepHeight) * loco.Leg / 0.85f + (foot.Hopping ? foot.HopFrom * (1f - u) : 0f), flat.z);
            }
            else
            {
                foot.Fade = foot.FadeFrom * (1f - u * u * (3f - 2f * u));
                if (foot.Stepping)
                {
                    // (Lifted as it goes, as a step is: it is not dragged.)
                    foot.Fade.y = 4f * u * (1f - u) * 0.06f * loco.Leg / 0.85f;
                }
            }

            foot.YawHold = foot.YawFrom * (1f - u * u * (3f - 2f * u));
            foot.Target = AboveGround(f, posed + foot.Fade);
            if (arc)
            {
                // (Its lift is the arc's: lifted for moving as well, it came down over the tenth of a second after it
                // had landed, standing in the air.)
                lift[f] = raised[f] = 0f;
                liftWas[f] = foot.Target;
                liftKnown[f] = true;
                return;
            }

            LiftMoving(f, foot, dt);
        }

        /// <summary>
        /// A foot moving over the ground is in the air (the plan "A different approach", step 4): a foot is either held
        /// where it stands or stepping, never slid. Where its place moves faster than <see cref="LiftFrom"/> (m/s at a
        /// Medium's size) with its sole still down (the root snapped away under a combo's link dragged a foot 55 cm
        /// along the ground in a sixth of a second), its ankle is raised by <see cref="LiftPerSpeed"/> metres for each
        /// m/s over that, <see cref="MostLift"/> at most, up quickly and down again as it slows. A walk's own swing is
        /// already higher than this and is left as it is. The most is the Combat Feel's (<see cref="CombatFeel.MovingFootLift"/>,
        /// centimetres; 0: off).
        /// </summary>
        public static float LiftFrom = 0.4f, LiftPerSpeed = 0.03f;

        /// <summary>How far a foot is lifted now for moving over the ground with its sole down (metres; for the reports).</summary>
        public float Lifted(int f) => raised[f];

        private readonly float[] raised = new float[2];

        private readonly Vector3[] liftWas = new Vector3[2];
        private readonly float[] lift = new float[2];
        private readonly bool[] liftKnown = new bool[2], liftStandKnown = new bool[2];
        private readonly float[] liftStand = new float[2];

        private void LiftMoving(int f, FootLock foot, float dt)
        {
            float scale = loco.Leg / 0.85f;
            Vector3 at = foot.Target;
            float want = 0f;
            float most = 0.01f * CombatFeel.MovingFootLift;
            if (liftKnown[f] && dt > 1e-5f && most > 0f)
            {
                Vector3 moved = at - liftWas[f];
                moved.y = 0f;
                want = Mathf.Clamp((moved.magnitude / dt - LiftFrom * scale) * LiftPerSpeed, 0f, most * scale);
            }

            lift[f] = Mathf.MoveTowards(lift[f], want, (want > lift[f] ? 1.2f : 0.6f) * scale * dt);
            liftWas[f] = at;
            liftKnown[f] = true;
            // (Its standing height: where its ankle is while it is held flat on the ground.)
            if (foot.Locked && !foot.OnToe && lift[f] <= 0f)
            {
                liftStand[f] = liftStandKnown[f] ? Mathf.Lerp(liftStand[f], at.y, 0.2f) : at.y;
                liftStandKnown[f] = true;
            }

            raised[f] = 0f;
            if (lift[f] > 0f && liftStandKnown[f])
            {
                float least = liftStand[f] + lift[f];
                if (at.y < least)
                {
                    raised[f] = least - at.y;
                    foot.Target = new Vector3(at.x, least, at.z);
                }
            }
        }

        /// <summary>
        /// An ankle raised as far as its turned sole would go into the ground (the heel as the foot
        /// lands, the toe as it pushes off: our foot's length is not the clip's).
        /// </summary>
        private Vector3 AboveGround(int f, Vector3 ankle)
        {
            var turn = FootRotation(f);
            float lowest = Mathf.Min((turn * soleToe).y, (turn * restHeel).y);
            float under = GroundY - (ankle.y + lowest);
            if (under > 0f)
            {
                ankle.y += under;
            }

            return ankle;
        }
    }
}
