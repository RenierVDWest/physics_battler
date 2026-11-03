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

        /// <summary>Seconds a released lock takes to fade into the clip's foot.</summary>
        public const float ReleaseHalfLife = 0.06f;

        public LocoBody(AnimRig anim, Locomotor loco)
        {
            loco.RootTurned += OnRootTurned;
            this.anim = anim;
            this.loco = loco;
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
            UpdateAimWeights(dt);
            // The feet first (the hips drop for them).
            UpdateFeet(dt);
            Pelvis(out Vector3 pelvisAt, out var pelvisTurn);
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
            if (TwoHanded)
            {
                GripTwoHanded();
            }

            if (CarryL == Carry.Shield)
            {
                // (In the guard and its strikes the shield faces the way the body does, at the enemy, a little out.)
                FaceShieldAt(RootTurn * Quaternion.Euler(0f, -ShieldOut, 0f) * Vector3.forward, ClipArms * ShieldTurned);
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
            for (int f = 0; f < 2; f++)
            {
                HoldFoot(f, dt);
            }
        }

        /// <summary>Lets go of both feet at once (the walk hands the body over, or is handed it).</summary>
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
            return (pole.sqrMagnitude > 1e-8f ? pole.normalized * says : Vector3.zero) + 0.3f * (RootTurn * Vector3.forward);
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

        /// <summary>The fastest an elbow's way turns (degrees a second).</summary>
        public float MostElbowTurn = 540f;

        /// <summary>The root turned at once (a combo's link): the directions kept against it turned back, so the body doesn't turn with it.</summary>
        private void OnRootTurned(float degrees)
        {
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

            pole = SteadyElbow(a, pole, upper.position, target);
            TwoBoneIK.Solve(upper, fore, hand, target, pole);

            if (clipShare >= 1f)
            {
                if (clipHandKnown)
                {
                    hand.rotation = FadeHand(a, clipHand);
                }

                if (right)
                {
                    SteerBlade(hand);
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

        private static bool IsThrust(LocoClip clip) => clip != null && clip.Kind != null && clip.Kind.StartsWith("thrust");

        /// <summary>How much of the wrist's steer is on now (worked out once a pose: <see cref="UpdateAimWeights"/>).</summary>
        private float SteerWeight() => steerWeight;

        private float aimWeight, steerWeight, aimFrom;

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

        private Quaternion FadeHand(int a, Quaternion want)
        {
            if (handKnown[a] && handSwitches[a] != loco.Switches)
            {
                handFade[a] = lastHand[a] * Quaternion.Inverse(want);
            }

            handSwitches[a] = loco.Switches;
            handFade[a] = Quaternion.Slerp(handFade[a], Quaternion.identity, 1f - Mathf.Pow(0.5f, poseDt / Mathf.Max(0.02f, loco.SwitchHalfLifeNow)));
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
            if (share <= 0f || way.sqrMagnitude < 1e-6f)
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
        public void PlayStrike(LocoClip clip, Vector3 target, bool plan = true, float startFrom = 0f)
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
            Plan(clip, target, startFrom, out float k, out float by, out Vector3 predicted, out Vector3 want);
            var turn0 = Quaternion.Euler(0f, loco.Facing, 0f);
            Vector3 miss = want - predicted;
            AimMissBeforePull = miss.magnitude;
            aimPull = turn0 * Quaternion.Euler(0f, by, 0f) * Vector3.ClampMagnitude(Quaternion.Inverse(Quaternion.Euler(0f, by, 0f)) * miss, MostAimPull);
            AimTurn = by;
            AimLunge = k;
            AimPredicted = new Vector3(loco.Position.x, GroundY, loco.Position.z) + turn0 * predicted;
            AimTarget = target;
            aimClip = clip;
            aimFrom = startFrom;
            loco.PlayAimed(clip, k, by, contact, startFrom);
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
            Vector3 point = StrikeReach.TryGet(BodyKey, clip.Name, out var grip, out var bladeWay)
                ? grip + bladeWay * (WeaponLength * SweetSpot) - travel
                : Quaternion.Euler(0f, turned, 0f) * ClipWeaponPoint(clip, contact);
            if (startFrom > 0f)
            {
                // (Taken up part way: the travel and the reach against where the root is then.)
                clip.RootMotion(0f, startFrom, out var before, out float turnedBefore);
                Vector3 rootThen = new Vector3(before.x, 0f, before.y) * loco.Leg;
                var back = Quaternion.Euler(0f, -turnedBefore, 0f);
                Vector3 atContact = travel + point;
                travel = back * (travel - rootThen);
                point = back * (atContact - rootThen) - travel;
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

            predicted = Quaternion.Euler(0f, by, 0f) * (k * travel + point);
        }

        /// <summary>How far an aimed strike would miss its target even with all its pull (metres; 0: it lands).</summary>
        public float MissAfterPull(LocoClip clip, Vector3 target, float startFrom = 0f) => MissAfterPull(clip, target, startFrom, loco.Position, loco.Facing);

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

        /// <summary>How much of the aim's pull is on now (worked out once a pose: <see cref="UpdateAimWeights"/>).</summary>
        private float AimWeight() => aimWeight;

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
        private void HoldFoot(int f, float dt)
        {
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
                foot.Locked = true;
                foot.LockedFor = 0f;
                foot.Stepping = false;
                // (Where it is now, faded in: a release still fading carries on into the lock. A foot
                // that lands toe first, walking backward, is held at its toe until the heel comes down.)
                foot.Ankle = posed + foot.Fade;
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
                bool settling = Standing && foot.LockedFor > SettleAfter && pull.magnitude > SettleRadius * loco.Leg
                                && Feet[1 - f].Locked && !Feet[1 - f].Stepping && Time() - lastSettle > SettleGap;
                if (settling)
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
                    foot.FadeOver = foot.Stepping ? 0.22f : Mathf.Clamp(loco.SwingLeft(f), 0.12f, 0.8f);
                    Trace?.Invoke($"{(f == 0 ? "L" : "R")} released after {foot.LockedFor:0.00} s{(stretched ? " (stretched)" : "")}: pull {pull.magnitude * 100f:0.0} cm, speed {loco.Velocity.magnitude:0.00}, stride x{loco.StrideScale:0.00}, turn {loco.FacingSpeed:0} deg/s, {loco.Current}, on toe {foot.OnToe}");
                }
                else
                {
                    foot.Target = AboveGround(f, held);
                    return;
                }
            }

            foot.FadeTime += dt;
            float u = Mathf.Clamp01(foot.FadeTime / Mathf.Max(0.05f, foot.FadeOver));
            foot.Fade = foot.FadeFrom * (1f - u * u * (3f - 2f * u));
            if (foot.Stepping)
            {
                // (Lifted as it goes, as a step is: it is not dragged.)
                foot.Fade.y = 4f * u * (1f - u) * 0.06f * loco.Leg / 0.85f;
            }
            foot.YawHold = foot.YawFrom * (1f - u * u * (3f - 2f * u));
            foot.Target = AboveGround(f, posed + foot.Fade);
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
