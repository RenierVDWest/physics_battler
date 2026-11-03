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

        private Vector3 World(LocoBone bone) => loco.Position + RootTurn * loco.Points[(int)bone] + Vector3.up * GroundY;

        /// <summary>Poses the rig for this step (after the locomotor's step).</summary>
        public void Pose(float dt)
        {
            poseDt = dt;
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

            // The legs.
            Leg(0, PartId.ThighL, PartId.ShinL, PartId.FootL, LocoBone.LeftUpLeg, LocoBone.LeftLeg, LocoBone.LeftFoot, LocoBone.LeftToeEnd);
            Leg(1, PartId.ThighR, PartId.ShinR, PartId.FootR, LocoBone.RightUpLeg, LocoBone.RightLeg, LocoBone.RightFoot, LocoBone.RightToeEnd);
        }

        // ---- The walk's parts, for a pose built elsewhere (the fight's pose pipeline) -----------------

        /// <summary>The feet's locks for this step: where each ankle goes (<see cref="FootLock.Target"/>).</summary>
        public void UpdateFeet(float dt)
        {
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
            return (pole.sqrMagnitude > 1e-8f ? pole.normalized : Vector3.zero) + 0.3f * (RootTurn * Vector3.forward);
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
        public Vector3 ShieldUpper = new Vector3(0.3f, -0.92f, -0.15f), ShieldFore = new Vector3(0.68f, -0.12f, 0.72f);

        /// <summary>How much of the clip's arm swing a carrying arm keeps (the rhythm stays; a heavy or held thing swings less).</summary>
        public float WeaponSwing = 0.75f, ShieldSwing = 0.4f;


        private float poseDt = 0.02f;
        private readonly Vector3[] swingMean = new Vector3[2];
        private readonly bool[] swingKnown = new bool[2];

        private void Arm(PartId upperId, PartId foreId, PartId handId, LocoBone shoulder, LocoBone elbow, LocoBone wrist,
            LocoBone middle, LocoBone index, LocoBone pinky)
        {
            var upper = anim.Part(upperId);
            var fore = anim.Part(foreId);
            var hand = anim.Part(handId);
            bool right = handId == PartId.HandR;
            int a = right ? 0 : 1;
            var carry = right ? CarryR : CarryL;
            Vector3 s = World(shoulder), e = World(elbow), w = World(wrist);
            float clipArm = (e - s).magnitude + (w - e).magnitude;
            float ourArm = anim.RestLocalPosition(foreId).magnitude + anim.RestLocalPosition(handId).magnitude;
            Vector3 reach = (w - s) * (ourArm / Mathf.Max(1e-3f, clipArm));
            Vector3 target = upper.position + reach;
            Vector3 pole = e - 0.5f * (s + w);
            var turn = RootTurn;
            Vector3 swing = Vector3.zero;
            if (carry != Carry.Free)
            {
                // The clip's swing kept, smaller, about where the carried thing is held: its swing is the
                // hand's way off its own mean over the last second or so (the clip's rhythm, whatever
                // the cycle).
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
                target = restHand + turn * swing * ((carry == Carry.Weapon ? WeaponSwing : ShieldSwing) * ourArm);
                // (The elbow where the rest pose has it: the forearm keeps its way as the arm swings.)
                pole = elbowAt - 0.5f * (upper.position + target);
            }

            TwoBoneIK.Solve(upper, fore, hand, target, pole);

            if (carry != Carry.Free)
            {
                // The wrist straight, as a hand holding something is: the shield faces the way the
                // forearm points; a weapon's angle is the grip's on the forearm's. Only its twist about
                // the forearm is chosen: a weapon's point the way the body goes (a little out), a
                // shield's grip bar across the fist the way it hangs.
                hand.rotation = fore.rotation;
                if (carry == Carry.Weapon)
                {
                    Vector3 axis = (hand.position - fore.position).normalized;
                    Vector3 blade = Vector3.ProjectOnPlane(hand.rotation * (right ? BladeR : BladeL), axis);
                    Vector3 wantWay = Vector3.ProjectOnPlane(turn * Quaternion.Euler(0f, right ? 8f : -8f, 0f) * Vector3.forward, axis);
                    if (blade.sqrMagnitude > 1e-6f && wantWay.sqrMagnitude > 1e-6f)
                    {
                        float twist = Mathf.Clamp(Vector3.SignedAngle(blade, wantWay, axis), -MostWristTwist, MostWristTwist);
                        hand.rotation = Quaternion.AngleAxis(twist, axis) * hand.rotation;
                    }
                }

                return;
            }

            // The hand: along the clip's hand (wrist to the middle knuckle), its thumb side the clip's.
            Vector3 along = World(middle) - w;
            Vector3 side = World(index) - World(pinky);
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
                hand.rotation = want * Quaternion.Inverse(rest);
            }

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
            bool landing = down && !foot.Locked && (!foot.Stepping || foot.FadeTime >= foot.FadeOver);
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
