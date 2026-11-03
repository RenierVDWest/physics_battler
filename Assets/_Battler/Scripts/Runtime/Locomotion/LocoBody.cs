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
        }

        private Quaternion RootTurn => Quaternion.Euler(0f, loco.Facing, 0f);

        private Vector3 World(LocoBone bone) => loco.Position + RootTurn * loco.Points[(int)bone] + Vector3.up * GroundY;

        /// <summary>Poses the rig for this step (after the locomotor's step).</summary>
        public void Pose(float dt)
        {
            var turn = RootTurn;
            Vector3 P(LocoBone b) => World(b);

            // The pelvis: its frame from the hips' line and the lower spine.
            Vector3 hipsMid = 0.5f * (P(LocoBone.LeftUpLeg) + P(LocoBone.RightUpLeg));
            Vector3 hipsRight = P(LocoBone.RightUpLeg) - P(LocoBone.LeftUpLeg);
            var pelvisTurn = Frame(P(LocoBone.Spine1) - P(LocoBone.Hips), hipsRight);

            // The feet first (the hips drop for them).
            for (int f = 0; f < 2; f++)
            {
                HoldFoot(f, dt);
            }

            float drop = 0f;
            for (int f = 0; f < 2; f++)
            {
                Vector3 hip = hipsMid + pelvisTurn * new Vector3(f == 0 ? -1f : 1f, 0f, 0f) * (0.5f * hipsRight.magnitude);
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

            var pelvis = anim.Part(PartId.Pelvis);
            pelvis.SetPositionAndRotation(hipsMid + pelvisTurn * new Vector3(0f, pelvisAboveHips, 0f) + Vector3.down * PelvisDrop, pelvisTurn);

            // The spine, the chest and the head.
            Vector3 shoulderRight = P(LocoBone.RightArm) - P(LocoBone.LeftArm);
            anim.Part(PartId.Spine).rotation = Frame(P(LocoBone.Spine2) - P(LocoBone.Spine), hipsRight.normalized + shoulderRight.normalized);
            anim.Part(PartId.Chest).rotation = Frame(P(LocoBone.Neck) - P(LocoBone.Spine2), shoulderRight);
            anim.Part(PartId.Head).rotation = Frame(P(LocoBone.HeadTop) - P(LocoBone.Head), P(LocoBone.RightEye) - P(LocoBone.LeftEye));

            // The arms: the hands where the clip's are against the shoulders, the elbows toward the clip's.
            Arm(PartId.UpperArmL, PartId.ForearmL, PartId.HandL, LocoBone.LeftArm, LocoBone.LeftForeArm, LocoBone.LeftHand,
                LocoBone.LeftHandMiddle, LocoBone.LeftHandIndex, LocoBone.LeftHandPinky);
            Arm(PartId.UpperArmR, PartId.ForearmR, PartId.HandR, LocoBone.RightArm, LocoBone.RightForeArm, LocoBone.RightHand,
                LocoBone.RightHandMiddle, LocoBone.RightHandIndex, LocoBone.RightHandPinky);

            // The legs.
            Leg(0, PartId.ThighL, PartId.ShinL, PartId.FootL, LocoBone.LeftUpLeg, LocoBone.LeftLeg, LocoBone.LeftFoot, LocoBone.LeftToeEnd);
            Leg(1, PartId.ThighR, PartId.ShinR, PartId.FootR, LocoBone.RightUpLeg, LocoBone.RightLeg, LocoBone.RightFoot, LocoBone.RightToeEnd);
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

        private void Arm(PartId upperId, PartId foreId, PartId handId, LocoBone shoulder, LocoBone elbow, LocoBone wrist,
            LocoBone middle, LocoBone index, LocoBone pinky)
        {
            var upper = anim.Part(upperId);
            var fore = anim.Part(foreId);
            var hand = anim.Part(handId);
            Vector3 s = World(shoulder), e = World(elbow), w = World(wrist);
            float clipArm = (e - s).magnitude + (w - e).magnitude;
            float ourArm = anim.RestLocalPosition(foreId).magnitude + anim.RestLocalPosition(handId).magnitude;
            Vector3 target = upper.position + (w - s) * (ourArm / Mathf.Max(1e-3f, clipArm));
            Vector3 pole = e - 0.5f * (s + w);
            TwoBoneIK.Solve(upper, fore, hand, target, pole);

            // The hand: along the clip's hand (wrist to the middle knuckle), its thumb side the clip's.
            Vector3 along = World(middle) - w;
            Vector3 side = World(index) - World(pinky);
            Vector3 restAlong = anim.RestLocalPosition(handId).normalized;   // (the hand carries on the forearm's line at rest)
            var slot = handId == PartId.HandR ? anim.SlotWeaponR : anim.SlotWeaponL;
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

        private void Leg(int f, PartId thighId, PartId shinId, PartId footId, LocoBone hip, LocoBone knee, LocoBone ankle, LocoBone toeEnd)
        {
            // (The knee the clip's way, and always somewhat forward: a held foot far from the clip's
            // can leave the clip's knee on the wrong side, and the knee would flip.)
            Vector3 dir = Quaternion.Inverse(RootTurn) * (World(toeEnd) - World(ankle));
            // The clip's knee against its own hip-to-ankle line, carried onto ours (turned from the
            // clip's line to ours): our foot is held where it was planted, the clip's has gone on.
            Vector3 clipAxis = World(ankle) - World(hip);
            Vector3 ourAxis = Feet[f].Target - anim.Part(thighId).position;
            Vector3 bend = Vector3.ProjectOnPlane(World(knee) - World(hip), clipAxis.sqrMagnitude > 1e-8f ? clipAxis.normalized : Vector3.down);
            Vector3 pole = Quaternion.FromToRotation(clipAxis, ourAxis) * bend;
            pole = (pole.sqrMagnitude > 1e-8f ? pole.normalized : Vector3.zero) + 0.3f * (RootTurn * Vector3.forward);

            TwoBoneIK.Solve(anim.Part(thighId), anim.Part(shinId), anim.Part(footId), Feet[f].Target, pole);
            anim.Part(footId).rotation = RootTurn * Quaternion.FromToRotation(restFootDir, dir.normalized);
        }

        /// <summary>A foot's lock: on as it lands, onto the toe as the heel lifts, faded off as it lifts.</summary>
        private void HoldFoot(int f, float dt)
        {
            var foot = Feet[f];
            var ankleBone = f == 0 ? LocoBone.LeftFoot : LocoBone.RightFoot;
            Vector3 posed = World(ankleBone);
            // (The toe where our own foot will put it, turned as the clip's foot is: the clip's toe is
            // where a foot of the clip's proportions has it, and holding that would slide ours.)
            Vector3 dir = Quaternion.Inverse(RootTurn) * (World(f == 0 ? LocoBone.LeftToeEnd : LocoBone.RightToeEnd) - posed);
            Vector3 posedToe = posed + RootTurn * Quaternion.FromToRotation(restFootDir, dir.normalized) * restToe;
            foot.Posed = posed;
            bool down = loco.Down[f] >= 0.5f;
            bool heel = loco.Heel[f] >= 0.5f;

            if (down && !foot.Locked && (!foot.Stepping || foot.FadeTime >= foot.FadeOver))
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
                    // (A foot that lifts swings from where it stood onto the clip's path by the time it
                    // lands; one let go while it should still be down is drawn back quickly.)
                    foot.Stepping = stretched && down;
                    foot.FadeOver = foot.Stepping ? 0.22f : Mathf.Clamp(loco.SwingLeft(f), 0.12f, 0.8f);
                    Trace?.Invoke($"{(f == 0 ? "L" : "R")} released after {foot.LockedFor:0.00} s{(stretched ? " (stretched)" : "")}: pull {pull.magnitude * 100f:0.0} cm, speed {loco.Velocity.magnitude:0.00}, stride x{loco.StrideScale:0.00}, turn {loco.FacingSpeed:0} deg/s, {loco.Current}, on toe {foot.OnToe}");
                }
                else
                {
                    foot.Target = held;
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
            foot.Target = posed + foot.Fade;
        }
    }
}
