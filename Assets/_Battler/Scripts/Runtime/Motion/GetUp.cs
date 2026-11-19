using PhysicsBattler.Combat;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// A whole pose held on its own: the pelvis's world position and rotation, every other part's
    /// rotation against its parent (the bones keep their lengths), and where each limb's middle and
    /// end joints are (world) and how its hand or foot is turned. The get-up blends between these:
    /// the ragdoll as it lay, the get-up's key poses, the standing pose. Limbs are blended by where
    /// their joints are and solved again (two quite different arm poses blended by their rotations
    /// can swing an elbow round the wrong way).
    /// </summary>
    public sealed class PoseBuffer
    {
        public Vector3 PelvisPosition;
        public readonly Quaternion[] Rotations = new Quaternion[PartInfo.Count];
        /// <summary>How each limb's middle joint stands off the line from its root to its end (world): the way it bends, and how much.</summary>
        public readonly Vector3[] LimbBend = new Vector3[4];
        public readonly Vector3[] LimbEnd = new Vector3[4];
        public readonly Quaternion[] LimbEndRotation = new Quaternion[4];

        /// <summary>The four limbs: root, middle and end part.</summary>
        public static readonly PartId[,] Limbs =
        {
            { PartId.UpperArmL, PartId.ForearmL, PartId.HandL },
            { PartId.UpperArmR, PartId.ForearmR, PartId.HandR },
            { PartId.ThighL, PartId.ShinL, PartId.FootL },
            { PartId.ThighR, PartId.ShinR, PartId.FootR },
        };

        /// <summary>The animation rig's pose now.</summary>
        public void Capture(AnimRig anim)
        {
            PelvisPosition = anim.Part(PartId.Pelvis).position;
            foreach (var id in PartInfo.All)
            {
                var t = anim.Part(id);
                Rotations[(int)id] = id == PartId.Pelvis ? t.rotation : t.localRotation;
            }

            for (int l = 0; l < 4; l++)
            {
                LimbEnd[l] = anim.Part(Limbs[l, 2]).position;
                LimbBend[l] = Bend(anim.Part(Limbs[l, 0]).position, anim.Part(Limbs[l, 1]).position, LimbEnd[l]);
                LimbEndRotation[l] = anim.Part(Limbs[l, 2]).rotation;
            }
        }

        /// <summary>The physical body as it lies (each part against the physical part it hangs from).</summary>
        public void Capture(UnitRig rig)
        {
            PelvisPosition = rig.Part(PartId.Pelvis).body.position;
            foreach (var id in PartInfo.All)
            {
                var rotation = rig.Part(id).body.rotation;
                Rotations[(int)id] = id == PartId.Pelvis ? rotation
                    : Quaternion.Inverse(rig.Part(PartInfo.Parent(id)).body.rotation) * rotation;
            }

            for (int l = 0; l < 4; l++)
            {
                LimbEnd[l] = rig.Part(Limbs[l, 2]).body.position;
                LimbBend[l] = Bend(rig.Part(Limbs[l, 0]).body.position, rig.Part(Limbs[l, 1]).body.position, LimbEnd[l]);
                LimbEndRotation[l] = rig.Part(Limbs[l, 2]).body.rotation;
            }
        }

        /// <summary>
        /// Puts the pose on the animation rig (parents first, as <see cref="PartInfo.All"/> is
        /// ordered), then (<paramref name="solveLimbs"/>) solves each limb onto its joints.
        /// </summary>
        public void Apply(AnimRig anim, bool solveLimbs = true)
        {
            foreach (var id in PartInfo.All)
            {
                var t = anim.Part(id);
                if (id == PartId.Pelvis)
                {
                    t.SetPositionAndRotation(PelvisPosition, Rotations[0]);
                }
                else
                {
                    t.localRotation = Rotations[(int)id];
                }
            }

            for (int l = 0; l < 4 && solveLimbs; l++)
            {
                var root = anim.Part(Limbs[l, 0]);
                var middle = anim.Part(Limbs[l, 1]);
                Vector3 pole = LimbBend[l].sqrMagnitude > 1e-6f ? LimbBend[l] : Bend(root.position, middle.position, anim.Part(Limbs[l, 2]).position);
                TwoBoneIK.Solve(root, middle, anim.Part(Limbs[l, 2]), LimbEnd[l], pole);

                anim.Part(Limbs[l, 2]).rotation = LimbEndRotation[l];
            }
        }

        private static Vector3 Bend(Vector3 root, Vector3 middle, Vector3 end)
        {
            Vector3 axis = end - root;
            Vector3 off = middle - root;
            return axis.sqrMagnitude > 1e-8f ? off - axis * (Vector3.Dot(off, axis) / axis.sqrMagnitude) : off;
        }

        /// <summary>This pose moved a share of the way to another, each part by its own share (pelvis position too).</summary>
        public void MixToward(PoseBuffer to, System.Func<PartId, float> share)
        {
            PelvisPosition = Vector3.Lerp(PelvisPosition, to.PelvisPosition, share(PartId.Pelvis));
            foreach (var id in PartInfo.All)
            {
                Rotations[(int)id] = Quaternion.Slerp(Rotations[(int)id], to.Rotations[(int)id], share(id));
            }

            for (int l = 0; l < 4; l++)
            {
                float w = share(Limbs[l, 2]);
                // Turned round the limb, not through straight; a straight limb bends the way the other does.
                Vector3 a = LimbBend[l], b = to.LimbBend[l];
                if (a.sqrMagnitude < 1e-4f && b.sqrMagnitude > 1e-8f)
                {
                    a = b.normalized * Mathf.Max(a.magnitude, 1e-3f);
                }
                else if (b.sqrMagnitude < 1e-4f && a.sqrMagnitude > 1e-8f)
                {
                    b = a.normalized * Mathf.Max(b.magnitude, 1e-3f);
                }

                LimbBend[l] = Vector3.Slerp(a, b, w);
                LimbEnd[l] = Vector3.Lerp(LimbEnd[l], to.LimbEnd[l], w);
                LimbEndRotation[l] = Quaternion.Slerp(LimbEndRotation[l], to.LimbEndRotation[l], w);
            }
        }

        public void CopyFrom(PoseBuffer other)
        {
            PelvisPosition = other.PelvisPosition;
            System.Array.Copy(other.Rotations, Rotations, Rotations.Length);
            System.Array.Copy(other.LimbBend, LimbBend, 4);
            System.Array.Copy(other.LimbEnd, LimbEnd, 4);
            System.Array.Copy(other.LimbEndRotation, LimbEndRotation, 4);
        }
    }

    /// <summary>
    /// Getting up off the ground, as the spec has it: the root goes under the pelvis, facing along
    /// the body; each part blends from the pose it lay in to the get-up's first key over a moment,
    /// legs first; then the key poses play, and the last of them blends into the standing pose.
    ///
    /// The key poses are made here rather than drawn: hips, spine and head set by angle, feet,
    /// knees, hands and elbows by IK onto points on the ground or in the air, each in proportion to
    /// the body (so one set fits every size). Face down: push up, hands and knees, kneel on one
    /// knee, rise. Face up: sit up, hands behind, push into a crouch, rise. They end with the feet
    /// on the stance's own spots, so the standing pose takes over without a step.
    /// </summary>
    public sealed class GetUp
    {
        public enum Kind
        {
            FaceDown,
            FaceUp,
        }

        private struct Key
        {
            public float time;
            public Vector3 pelvis;          // unit space
            public float pitch, spine, chest, head;   // degrees, + bends forward
            public float roll;              // degrees about the body's length, + onto the sword side
            public float lean;              // degrees to its right, about the way the root faces
            public Vector3 ankleL, ankleR;  // unit space
            public Vector3 kneeL, kneeR;    // pole directions, unit space
            public float footL, footR;      // foot pitch, + toes down
            public Vector3 handL, handR;    // unit space (the wrist)
            public Vector3 elbowL, elbowR;  // pole directions, unit space
        }

        private readonly Transform root;
        private readonly AnimRig anim;
        private readonly BodyMeasure body;
        private readonly Stance stance;
        private readonly Key[] keys = new Key[16];
        private int count = 5;
        private readonly PoseBuffer lying = new PoseBuffer();
        private readonly PoseBuffer keyed = new PoseBuffer();
        private readonly PoseBuffer mixed = new PoseBuffer();
        private float pace = 1f;

        public Kind Facing { get; private set; }

        /// <summary>Seconds from the ragdoll's pose to the first key (the legs take a little less).</summary>
        public float BlendIn => (Natural ? NaturalBlend : 0.3f) * pace;

        /// <summary>When the standing pose starts taking over (seconds into the get-up), and the whole get-up.</summary>
        public float StandFrom => KeysFrom + keys[count - 2].time;
        public float Duration => KeysFrom + keys[count - 1].time;

        /// <summary>When the keys begin to play: after the blend from how it lay, or (natural) with it.</summary>
        private float KeysFrom => Natural ? 0f : BlendIn;

        /// <summary>
        /// The get-up of the plan "Getting up" (14 November): from the pose the body is really in, with no pause in a
        /// tidy lying pose (the keys play from the first moment while each part leaves how it lay on its own clock,
        /// the head first); lopsided, one hand on the ground and the other arm free, up through one knee as a fighter
        /// is taught to stand up in base; three routes, from the back, the front and a side. Off: as it was. Set
        /// before <see cref="Begin"/>.
        /// </summary>
        public bool Natural { get; set; }

        /// <summary>Natural: how long each part takes to leave how it lay (seconds at a Medium's size; the head and the arms take a share of it).</summary>
        public const float NaturalBlend = 0.6f;

        /// <summary>Natural: it lies on a side (+1 its right side down, -1 its left; 0 on its back or its front). Set before <see cref="Begin"/>.</summary>
        public float Side { get; set; }

        /// <summary>
        /// Natural: the get-up begins at this key, not at lying (0: the whole get-up). 4 is the half kneel: a body
        /// that caught its fall on a knee and a hand rises from there. Set before <see cref="Begin"/>.
        /// </summary>
        public int StartKey { get; set; }

        /// <summary>
        /// Natural: the hand that goes to the ground (<see cref="Stance.Left"/> or <see cref="Stance.Right"/>; -1: the
        /// hand of the back foot's side). A swordsman with no shield posts with its free hand and keeps its sword
        /// before it. Set before <see cref="Begin"/>.
        /// </summary>
        public int PostWith { get; set; } = -1;

        /// <summary>Natural: how far a hand is turned on its wrist to lay what it holds clear of the body (degrees).</summary>
        public const float WristTurns = 55f;

        /// <summary>The key a natural get-up reaches the half kneel at.</summary>
        public const int HalfKneelKey = 4;

        /// <summary>The key a natural get-up from the back is sitting on its hand at.</summary>
        public const int SittingKey = 2;

        /// <summary>Natural: this get-up's own pace, about 1 (so fighters felled together do not rise in step). Set before <see cref="Begin"/>.</summary>
        public float Vary { get; set; } = 1f;

        /// <summary>Natural: how far a get-up turns toward its threat while still on the ground, at most (degrees).</summary>
        public const float TurnsToThreat = 35f;

        /// <summary>
        /// For the reports: the fastest any part moved in the get-up's first moments (m/s; a snap shows as a high
        /// speed), the furthest a hand or foot on the ground was from where its key wanted it (metres; a pose the
        /// body cannot reach), and how far hands and feet on the ground moved while their keys held them still (metres).
        /// </summary>
        public float FastestEarly { get; private set; }
        public float ReachMiss { get; private set; }
        public float Slid { get; private set; }

        /// <summary>The length of what the right hand holds, from its grip (metres; 0: nothing held, or not known). Set before <see cref="Begin"/>.</summary>
        public float BladeLength { get; set; }

        /// <summary>
        /// For the reports: how deep an arm (the forearm and hand of either) and what the right hand holds went into
        /// its own trunk, head or legs through the get-up at the deepest (metres), and for how long each was more
        /// than 2 cm in (seconds).
        /// </summary>
        public float ArmDeepest { get; private set; }

        /// <summary>Which arm that was, and when.</summary>
        public string ArmDeepestAt { get; private set; } = "";

        /// <summary>
        /// For the reports: the fastest the pose turned a thigh on the hips through the get-up (rad/s), and the
        /// furthest it had one past its hip's range (degrees), with which and when.
        /// </summary>
        public float ThighFastest { get; private set; }
        public float ThighPast { get; private set; }
        public string ThighFastestAt { get; private set; } = "";
        public string ThighPastAt { get; private set; } = "";

        /// <summary>For the reports: the nearest an ankle came to its own hip through the get-up, as a share of the leg's length (a leg folds to 0.30), with which and when.</summary>
        public float AnkleNearest { get; private set; } = 1f;
        public string AnkleNearestAt { get; private set; } = "";

        private readonly Quaternion[] thighWas = new Quaternion[2];

        public float BladeDeepest { get; private set; }
        public float ArmInFor { get; private set; }
        public float BladeInFor { get; private set; }

        private static readonly PartId[] TrunkParts = { PartId.Pelvis, PartId.Spine, PartId.Chest, PartId.Head };
        private static readonly PartId[] BladeClearOf = { PartId.Pelvis, PartId.Spine, PartId.Chest, PartId.Head, PartId.ThighL, PartId.ThighR, PartId.ShinL, PartId.ShinR };

        /// <summary>How deep an arm's forearm and hand are in its own trunk or head (metres; 0: clear).</summary>
        private float ArmIn(bool right)
        {
            if (Rig == null)
            {
                return 0f;
            }

            Vector3 elbow = anim.Part(right ? PartId.ForearmR : PartId.ForearmL).position;
            Vector3 wrist = anim.Part(right ? PartId.HandR : PartId.HandL).position;
            Vector3 knuckles = wrist + (wrist - elbow).normalized * (0.08f * body.SizeFactor);
            float deepest = 0f;
            foreach (var id in TrunkParts)
            {
                float allowance = id == PartId.Chest ? BodyClearance.ArmChestAllowance * body.SizeFactor : 0f;
                float d = Mathf.Min(BodyClearance.Distance(Rig, anim, id, Vector3.Lerp(elbow, wrist, 0.25f), wrist), BodyClearance.Distance(Rig, anim, id, wrist, knuckles)) + allowance;
                deepest = Mathf.Max(deepest, -d);
            }

            return deepest;
        }

        /// <summary>How deep what the right hand holds is in its own trunk, head or legs (metres; 0: clear, or nothing held).</summary>
        private float BladeIn()
        {
            var slot = anim.SlotWeaponR;
            if (Rig == null || slot == null || !HoldsMain || BladeLength <= 0f)
            {
                return 0f;
            }

            // (From a hand's breadth up the blade: the grip is in the hand, by the body.)
            Vector3 along = slot.rotation * Vector3.forward;
            Vector3 from = slot.position + along * (0.12f * body.SizeFactor), to = slot.position + along * BladeLength;
            float deepest = 0f;
            foreach (var id in BladeClearOf)
            {
                deepest = Mathf.Max(deepest, -BodyClearance.Distance(Rig, anim, id, from, to));
            }

            return deepest;
        }

        /// <summary>For the reports: which part moved fastest and when, and which hand or foot was furthest from its key and when (seconds into the get-up).</summary>
        public string Fastest { get; private set; } = "";
        public string Missed { get; private set; } = "";
        private float keysAt;

        private readonly Vector3[] partWas = new Vector3[PartInfo.Count];
        private readonly Vector3[] endWas = new Vector3[4], endAsked = new Vector3[4];
        private readonly bool[] endDown = new bool[4];
        private float composedAt = -1f, shieldFrom, shieldBy;

        /// <summary>Holding a weapon in a hand, or a shield (they're laid out of the way while the hands push on the ground).</summary>
        public bool HoldsMain { get; set; }
        public bool HoldsOff { get; set; }

        /// <summary>
        /// Milestone 9c: what the hands hold (their shapes, for the ground clamp and the shield), and
        /// where the threat it minds is (world; none if it knows of none). Set before each pose.
        /// </summary>
        public Loadout Loadout { get; set; }
        public Vector3? Threat { get; set; }

        /// <summary>The unit's physical rig (its colliders' shapes, for keeping the shield off its own body).</summary>
        public UnitRig Rig { get; set; }

        /// <summary>The lowest point of what the hands hold over the ground in this pose (metres; negative: in it).</summary>
        public float WeaponGroundGap { get; private set; } = float.MaxValue;

        /// <summary>Which way the shield faces in this pose (world; zero without one).</summary>
        public Vector3 ShieldFace { get; private set; }

        /// <summary>Getting up behind a shield: its own get-ups (the shield up toward the threat, the sword hand pushing).</summary>
        public bool Shielded => HoldsOff && Loadout != null && Loadout.HasShield;

        /// <summary>How far a shield-bearer getting up face up rolls onto its sword side (degrees).</summary>
        public const float ShieldRoll = 18f;
        private float rollSign = 1f;

        public GetUp(Transform root, AnimRig anim, BodyMeasure body, Stance stance)
        {
            this.root = root;
            this.anim = anim;
            this.body = body;
            this.stance = stance;
        }

        /// <summary>
        /// Where the root goes for a body lying with its pelvis at <paramref name="pelvis"/> and its
        /// head toward <paramref name="headward"/> (world, horizontal), and which way it faces: along
        /// the body toward the head (face down) or the feet (face up), back from the pelvis by as
        /// far as the hips travel over the get-up, so it stands up where its feet are.
        /// </summary>
        public static void Place(Kind kind, Vector3 pelvis, Vector3 headward, float size, out Vector3 position, out float yaw)
        {
            Place(kind, pelvis, headward, size, false, null, out position, out yaw, out _);
        }

        /// <summary>
        /// As above; <paramref name="natural"/>: a body on its back stands up nearer where it sat (it gets up over
        /// its hand and its drawn-in foot, not over where its feet lay), and it is turned toward
        /// <paramref name="threat"/> (world; none: not turned) about its hips by up to <see cref="TurnsToThreat"/>
        /// (<paramref name="turned"/>, degrees): a body sitting up pivots where it sits to face what it fights.
        /// </summary>
        /// <param name="sitsBack">From its back, natural: how far toward its feet it stands up (metres; <see cref="SitsBack"/>). Negative: <see cref="BackSits"/> at its size.</param>
        /// <param name="standsAhead">How far along that way from where its hips lie it stands up (metres; behind them if negative), when the get-up says so itself (<see cref="StandsAhead"/>); not a number: as above.</param>
        public static void Place(Kind kind, Vector3 pelvis, Vector3 headward, float size, bool natural, Vector3? threat, out Vector3 position, out float yaw, out float turned, float sitsBack = -1f, float standsAhead = float.NaN)
        {
            Vector3 along = kind == Kind.FaceDown ? headward : -headward;
            along.y = 0f;
            along = along.sqrMagnitude > 1e-6f ? along.normalized : Vector3.forward;
            yaw = Mathf.Atan2(along.x, along.z) * Mathf.Rad2Deg;
            turned = 0f;
            if (natural && threat.HasValue)
            {
                Vector3 to = threat.Value - pelvis;
                to.y = 0f;
                if (to.sqrMagnitude > 0.04f * size * size)
                {
                    float toward = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
                    turned = Mathf.Clamp(Mathf.DeltaAngle(yaw, toward), -TurnsToThreat, TurnsToThreat);
                    yaw += turned;
                    along = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
                }
            }

            position = pelvis + along * (!float.IsNaN(standsAhead) ? standsAhead : kind == Kind.FaceDown ? 0.25f * size : natural ? (sitsBack >= 0f ? sitsBack : BackSits * size) : 0.45f * size);
        }

        /// <summary>
        /// For <see cref="Place"/>: a get-up made of a recording stands up where the recorded man did, against where
        /// his hips lay (metres along the way it will face; not a number: the get-up is not one of those).
        /// </summary>
        public float StandsAhead(Kind kind)
        {
            if (!Natural || !FromRecordings || StartKey != 0)
            {
                return float.NaN;
            }

            float[][] table = kind == Kind.FaceDown ? RecordedFront : RecordedBack;
            return -table[0][3] * body.LegLength * HipsTravel(table);
        }

        /// <summary>
        /// The share of the recorded hips' way along the ground that this body's hips go: all of it, or as much as
        /// leaves its feet within its legs' reach where it lies (the recorded man lies with his legs straight and
        /// stands up where his feet are; a body whose legs are a little shorter against that way would have its feet
        /// asked for past the ends of its legs, and they would drag after its hips).
        /// </summary>
        private float HipsTravel(float[][] table)
        {
            float leg = body.LegLength, reach = 0.93f * leg, hips = table[0][3] * leg, share = 1f;
            if (Mathf.Abs(hips) < 0.05f * leg)
            {
                return 1f;
            }

            for (int f = 0; f < 2; f++)
            {
                float ankle = table[0][12 + 3 * f] * leg;
                if (Mathf.Abs(ankle - hips) <= reach)
                {
                    continue;
                }

                float a = (ankle - reach) / hips, b = (ankle + reach) / hips, best = 0.3f;
                if (a > best && a <= 1f)
                {
                    best = a;
                }

                if (b > best && b <= 1f)
                {
                    best = b;
                }

                share = Mathf.Min(share, best);
            }

            return share;
        }

        /// <summary>Natural: a body on its back stands up this far toward its feet from where its hips lay (metres at a Medium's size).</summary>
        public const float BackSits = 0.22f;

        /// <summary>Starts a get-up: <paramref name="from"/> is how the body lay (the root already placed).</summary>
        public void Begin(Kind kind, PoseBuffer from)
        {
            BeginSwordHand();
            Facing = kind;
            lying.CopyFrom(from);
            shieldRoll = -1;
            shieldRollAngle = float.NaN;
            // (What the hands held as it lay brought out of the floor once, so the get-up starts from
            // a body holding it above the ground.)
            if (Loadout != null)
            {
                lying.Apply(anim, false);
                PosePipeline.KeepWeaponsOffGround(anim, Loadout, body.SizeFactor, stance.GroundY, 1f, free: true);
                lying.Capture(anim);
            }

            float s = body.SizeFactor;
            pace = Mathf.Sqrt(Mathf.Max(0.3f, s)) * (Natural ? Mathf.Clamp(Vary, 0.8f, 1.25f) : 1f);
            rollSign = RollSign();
            FastestEarly = ReachMiss = Slid = 0f;
            ArmDeepest = BladeDeepest = ArmInFor = BladeInFor = 0f;
            ArmDeepestAt = "";
            ThighFastest = ThighPast = 0f;
            ThighFastestAt = ThighPastAt = "";
            AnkleNearest = 1f;
            AnkleNearestAt = "";
            Fastest = Missed = "";
            composedAt = -1f;
            for (int i = 0; i < 4; i++)
            {
                endDown[i] = false;
            }

            if (Natural && FromRecordings && StartKey == 0)
            {
                // (Made of the recordings; a fall caught part of the way down still rises by the made keys.)
                if (kind == Kind.FaceDown)
                {
                    MakeRecorded(RecordedFront, RecordedFrontPost, RecordedFrontSeconds, RecordedFrontStandPitch, RecordedFrontPlantedL, RecordedFrontPlantedR);
                }
                else
                {
                    MakeRecorded(RecordedBack, RecordedBackPost, RecordedBackSeconds, RecordedBackStandPitch, RecordedBackPlantedL, RecordedBackPlantedR);
                }
            }
            else if (Natural)
            {
                count = 7;
                if (kind == Kind.FaceDown)
                {
                    MakeFromFront();
                }
                else
                {
                    MakeFromBack();
                }
            }
            else
            {
                count = 5;
                if (kind == Kind.FaceDown)
                {
                    MakeFaceDown();
                }
                else
                {
                    MakeFaceUp();
                }

                shieldFrom = keys[1].time;
                shieldBy = keys[2].time;
            }

            if (Natural && StartKey > 0 && StartKey < count - 2)
            {
                // (From a key part of the way up: the keys before it are dropped, and the rest begin at once.)
                float begins = keys[StartKey].time;
                for (int i = StartKey; i < count; i++)
                {
                    keys[i - StartKey] = keys[i];
                    keys[i - StartKey].time -= begins;
                }

                count -= StartKey;
                shieldFrom = shieldBy = 0f;
            }

            for (int i = 0; i < count; i++)
            {
                keys[i].time *= pace;
            }

            shieldFrom *= pace;
            shieldBy *= pace;
        }

        /// <summary>
        /// Poses the animation rig <paramref name="t"/> seconds into the get-up. From
        /// <see cref="StandFrom"/> on, <paramref name="stand"/> (the standing pose, as the pose
        /// pipeline makes it) takes over.
        /// </summary>
        public void Compose(float t, PoseBuffer stand)
        {
            pinDt = pinT >= 0f && t > pinT ? Mathf.Min(t - pinT, 0.05f) : 1f / 60f;
            pinT = t;
            fitFrame++;
            PoseKeys(Mathf.Max(0f, t - KeysFrom));
            bool blendingIn = t < BlendIn * 1.25f;
            bool standing = stand != null && t > StandFrom;
            if (!blendingIn && !standing)
            {
                Finish(t, stand);
                Composed(t);
                return;
            }

            keyed.Capture(anim);
            if (blendingIn)
            {
                // From the ragdoll's pose to the first key, each part on its own clock: legs first.
                if (Natural)
                {
                    // (From how it lay toward keys that are already moving: the head first, then the arms, the legs, the
                    // trunk. Each step takes the pose it had a share of the way that is left to the keys' pose now: mixed
                    // afresh from how it lay each step, a part half a turn from its key took the other way round the
                    // moment the moving key crossed over, and an arm jumped across the body in one step.)
                    if (composedAt < 0f)
                    {
                        mixed.CopyFrom(lying);
                    }

                    float before = Mathf.Max(0f, composedAt);
                    mixed.MixToward(keyed, id =>
                    {
                        float over = BlendIn * NaturalShare(id);
                        float was = Smooth(before / over), now = Smooth(t / over);
                        return was >= 1f - 1e-4f ? 1f : Mathf.Clamp01((now - was) / (1f - was));
                    });
                }
                else
                {
                    mixed.CopyFrom(lying);
                    mixed.MixToward(keyed, id => Smooth(t / (BlendIn * Share(id))));
                }

                keyed.CopyFrom(mixed);
            }

            if (standing)
            {
                float w = Smooth((t - StandFrom) / Mathf.Max(1e-3f, Duration - StandFrom));
                keyed.MixToward(stand, _ => w);
            }

            // Into the first key, every part turns from how it lay (the key holds still meanwhile);
            // into the standing pose, the limbs go by where their joints are (both poses move).
            keyed.Apply(anim, standing);
            Finish(t, stand);
            Composed(t);
        }

        /// <summary>For the reports: how fast the parts are moving in the get-up's first moments.</summary>
        private void Composed(float t)
        {
            if (composedAt >= 0f && t > composedAt)
            {
                float armL = ArmIn(false), armR = ArmIn(true);
                float arm = Mathf.Max(armL, armR), blade = BladeIn();
                if (arm > ArmDeepest)
                {
                    ArmDeepestAt = $"its {(armL > armR ? "shield" : "sword")} arm at {t:0.00} s";
                }

                ArmDeepest = Mathf.Max(ArmDeepest, arm);
                BladeDeepest = Mathf.Max(BladeDeepest, blade);
                ArmInFor += arm > 0.02f ? t - composedAt : 0f;
                BladeInFor += blade > 0.02f ? t - composedAt : 0f;
            }

            // (The legs: how fast a thigh turns on the hips, and how far past the hip's range the pose has it. A knee
            // that swings round through the trunk shows as both.)
            for (int f = 0; f < 2; f++)
            {
                var id = f == 0 ? PartId.ThighL : PartId.ThighR;
                Quaternion local = Quaternion.Inverse(anim.RestLocalRotation(id)) * anim.Part(id).localRotation;
                if (composedAt >= 0f && t > composedAt + 1e-4f)
                {
                    float turns = Quaternion.Angle(thighWas[f], local) * Mathf.Deg2Rad / (t - composedAt);
                    if (turns > ThighFastest)
                    {
                        ThighFastest = turns;
                        ThighFastestAt = $"its {id} at {t:0.00} s";
                    }
                }

                thighWas[f] = local;
                var shinId = f == 0 ? PartId.ShinL : PartId.ShinR;
                var footId = f == 0 ? PartId.FootL : PartId.FootR;
                float folded = Vector3.Distance(anim.Part(id).position, anim.Part(footId).position) / Mathf.Max(1e-4f, anim.RestLocalPosition(shinId).magnitude + anim.RestLocalPosition(footId).magnitude);
                if (folded < AnkleNearest)
                {
                    AnkleNearest = folded;
                    AnkleNearestAt = $"its {footId} at {t:0.00} s";
                }

                float past = Muscles.JointMath.PastLimits(local, Muscles.JointLimitTable.For(id), out _, out _, out _);
                if (past > ThighPast)
                {
                    ThighPast = past;
                    ThighPastAt = $"its {id} at {t:0.00} s";
                }
            }

            bool early = t < 0.7f * pace;
            foreach (var id in PartInfo.All)
            {
                Vector3 at = anim.Part(id).position;
                if (early && composedAt >= 0f && t > composedAt + 1e-4f)
                {
                    float speed = Vector3.Distance(at, partWas[(int)id]) / (t - composedAt);
                    if (speed > FastestEarly)
                    {
                        FastestEarly = speed;
                        Fastest = $"its {id} at {t:0.00} s";
                    }
                }

                partWas[(int)id] = at;
            }

            composedAt = t;
        }

        /// <summary>Natural: the share of the blend from how it lay each part takes: the head leads, the arms follow, then the legs and the trunk.</summary>
        private static float NaturalShare(PartId id)
        {
            switch (id)
            {
                case PartId.Head:
                    return 0.55f;
                case PartId.ThighL:
                case PartId.ThighR:
                case PartId.ShinL:
                case PartId.ShinR:
                case PartId.FootL:
                case PartId.FootR:
                    return 0.95f;
                case PartId.Pelvis:
                case PartId.Spine:
                case PartId.Chest:
                    return 1f;
                default:
                    return 0.9f;
            }
        }

        /// <summary>
        /// Milestone 9c: after the keys, the shield is held up toward the threat (getting up face up all
        /// along; face down once off the hands), giving way to the standing pose's shield as it takes
        /// over; then what the hands hold is kept out of the ground.
        /// </summary>
        public static bool TraceHeld;
        private float dbgKeyGap, dbgWristY;

        private void Finish(float t, PoseBuffer stand)
        {
            // (One step of the pose; a new get-up begins with no turn remembered.)
            bladeStep++;
            if (t < bladeLastT)
            {
                bladeMore = bladeMoreWas = 0f;
            }

            bladeLastT = t;
            float standing = stand != null && t > StandFrom ? Smooth((t - StandFrom) / Mathf.Max(1e-3f, Duration - StandFrom)) : 0f;
            ShieldFace = Vector3.zero;
            if (Shielded)
            {
                float up;
                if (Facing == Kind.FaceUp)
                {
                    up = Smooth(t / BlendIn);
                }
                else
                {
                    float k = t - KeysFrom;
                    up = Smooth((k - shieldFrom) / Mathf.Max(1e-3f, shieldBy - shieldFrom));
                }

                float w = up * (1f - standing);
                if (w > 1e-4f)
                {
                    HoldShieldUp(w);
                }

                var slot = anim.SlotWeaponL;
                ShieldFace = slot != null ? slot.rotation * Vector3.forward : Vector3.zero;
            }

            // (The clamp's floor is soft, so the arm is eased up as what it holds comes down. It comes in
            // over the blend from the body as it lay, which was brought out of the floor once, at the
            // start: a ragdoll's pose is anyhow.)
            // (18 November: with its whole weight from the start. Coming in over the blend, a shield lay up to 17 cm under
            // the ground a quarter of a second into a get-up from the front, in a fight of six.)
            if (TraceHeld && Loadout != null && Loadout.Main != null && anim.SlotWeaponR != null)
            {
                float before = GroundClamp.Gap(anim.SlotWeaponR, Loadout.Main, Loadout.Scale, stance.GroundY);
                Vector3 dirWas = Quaternion.Inverse(root.rotation) * (anim.SlotWeaponR.rotation * Vector3.forward);
                var mem = new GroundClamp.Memory();
                GroundClamp.Keep(anim, true, Loadout.Main, Loadout.Scale, body.SizeFactor, stance.GroundY, 1f, false, mem);
                Vector3 dir = Quaternion.Inverse(root.rotation) * (anim.SlotWeaponR.rotation * Vector3.forward);
                Vector3 hp = Quaternion.Inverse(root.rotation) * (anim.Part(PartId.HandR).position - root.position);
                Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "GETUPHELD {0:0.000} wristY {1:0.00} keygap {2:0.00} before {3:0.00} turn {4:0} raise {5:0} dirWas ({6:0.00},{7:0.00},{8:0.00}) dir ({9:0.00},{10:0.00},{11:0.00}) hand ({12:0.00},{13:0.00},{14:0.00}) stand {15:0.00} nat {16} sh {17}",
                    t, dbgWristY, dbgKeyGap, before, mem.turn, mem.raise, dirWas.x, dirWas.y, dirWas.z, dir.x, dir.y, dir.z, hp.x, hp.y, hp.z, standing, Natural, Shielded));
            }

            if (pinned)
            {
                PinSwordHand(t);
            }

            // (19 November, tried and taken out: the clamp given a memory of its wrist turn and arm raise from step to
            // step. It changed nothing in what the board or the blade's tip does in a get-up.)
            WeaponGroundGap = PosePipeline.KeepWeaponsOffGround(anim, Loadout, body.SizeFactor, stance.GroundY, HeldClearFromTheStart ? 1f : Smooth(t / BlendIn));
            if (pinned)
            {
                SettleSwordHand();
            }

            if (TraceHeld && Loadout != null && Loadout.Main != null && anim.SlotWeaponR != null)
            {
                // (The sword as the get-up leaves it this step, after everything: what the trace above shows is before the settle.)
                Vector3 dirNow = Quaternion.Inverse(root.rotation) * (anim.SlotWeaponR.rotation * Vector3.forward);
                Vector3 handNow = Quaternion.Inverse(root.rotation) * (anim.Part(PartId.HandR).position - root.position);
                Debug.Log(string.Format(System.Globalization.CultureInfo.InvariantCulture, "GETUPFINAL {0:0.000} sh {7} dir ({1:0.00},{2:0.00},{3:0.00}) hand ({4:0.00},{5:0.00},{6:0.00}) pinned {8:0.00} s",
                    t, dirNow.x, dirNow.y, dirNow.z, handNow.x, handNow.y, handNow.z, Shielded, SwordHandPinnedFor));
            }

            if (Shielded && anim.SlotWeaponL != null)
            {
                ShieldFace = anim.SlotWeaponL.rotation * Vector3.forward;
            }
        }

        /// <summary>
        /// The shield arm raised toward the threat (the plan's Fallen state), by <paramref name="w"/>
        /// of the way from where the keys put it: the shield's middle most of an arm out from the
        /// shoulder, facing the threat, never down into the ground.
        /// </summary>
        private void HoldShieldUp(float w)
        {
            var shield = Loadout.Off;
            float scale = Loadout.Scale;
            var upper = anim.Part(PartId.UpperArmL);
            var fore = anim.Part(PartId.ForearmL);
            var hand = anim.Part(PartId.HandL);
            var slot = anim.SlotWeaponL;
            Quaternion upperWas = upper.localRotation, foreWas = fore.localRotation, handWas = hand.localRotation;
            Vector3 chest = anim.Part(PartId.Chest).position;
            Vector3 way = Threat.HasValue ? Threat.Value - chest : Vector3.zero;
            if (way.sqrMagnitude < 1e-4f)
            {
                way = root.rotation * new Vector3(0f, 0.7f, 0.7f);
            }

            way.Normalize();
            way.y = Mathf.Max(way.y, -0.1f);
            way.Normalize();
            // (The arm stays on its own side: a person reaches the shield out to the left and in front,
            // never across the chest and head to a threat on the sword side. The shield turns toward
            // the threat from there, as far as the grip lets it.)
            Vector3 faceWay = way;
            way = ShieldSide(way);
            faceWay = Vector3.RotateTowards(way, faceWay, ShieldFaceOffArm * Mathf.Deg2Rad, 0f);
            Vector3 upWay = Vector3.ProjectOnPlane(Vector3.up, faceWay);
            Quaternion rotation = Quaternion.LookRotation(faceWay, upWay.sqrMagnitude > 1e-4f ? upWay : root.rotation * Vector3.forward);
            Quaternion handFrom = hand.rotation;
            Quaternion handTurn = rotation * Quaternion.Inverse(slot.localRotation);
            Quaternion turn = Quaternion.Slerp(handFrom, handTurn, w);
            // (Held as a guard holds it: the elbow down and out by the ribs, the forearm pointing at the
            // threat, so the shield faces it with the wrist straight. It comes from where the keys had
            // it, by w.)
            float upperLength = anim.RestLocalPosition(PartId.ForearmL).magnitude, foreLength = anim.RestLocalPosition(PartId.HandL).magnitude;
            Vector3 right = ChestFrame(out Vector3 chestUp, out _);
            Vector3 elbowWay = (0.45f * way + 0.8f * Vector3.down - 0.35f * right).normalized;
            Vector3 forearmWay = (faceWay + 0.15f * chestUp).normalized;
            Vector3 guardHand = upper.position + elbowWay * upperLength + forearmWay * (0.95f * foreLength);
            Vector3 target = Vector3.Lerp(hand.position, guardHand, w) + Vector3.up * (0.03f * body.SizeFactor * w);
            // (The elbow turns from where the keys had it to down by the ribs, as the shield comes up.)
            Vector3 elbowWas = fore.position - 0.5f * (upper.position + hand.position);
            Vector3 elbowTo = (Vector3.down - 0.6f * right).normalized;
            Vector3 pole = elbowWas.sqrMagnitude > 1e-8f ? Vector3.Slerp(elbowWas.normalized, elbowTo, w) : elbowTo;
            PlaceShieldArm(target, pole, turn);

            // (Which way up the shield is held, about its face, is the wrist's to choose: the turn the
            // forearm as placed takes with the straightest wrist, and the arm placed again for it.)
            // (It keeps the roll it had unless another is clearly easier: the shield doesn't flick round.)
            float bestCost = float.MaxValue, keptCost = float.MaxValue;
            int best = 0;
            for (int k = 0; k < 24; k++)
            {
                Quaternion rolled = Quaternion.AngleAxis(15f * k, faceWay) * rotation;
                hand.rotation = rolled * Quaternion.Inverse(slot.localRotation);
                float cost = WristCost();
                if (cost < bestCost - 1e-3f)
                {
                    bestCost = cost;
                    best = k;
                }

                if (k == shieldRoll)
                {
                    keptCost = cost;
                }
            }

            if (shieldRoll < 0 || bestCost < keptCost - 8f)
            {
                shieldRoll = best;
            }

            // (Eased round, at most ShieldRollRate a second.)
            float aim = 15f * shieldRoll;
            shieldRollAngle = float.IsNaN(shieldRollAngle) ? aim : shieldRollAngle + Mathf.Clamp(Mathf.DeltaAngle(shieldRollAngle, aim), -ShieldRollRate / 60f, ShieldRollRate / 60f);
            Quaternion bestRotation = Quaternion.AngleAxis(shieldRollAngle, faceWay) * rotation;

            {
                handTurn = bestRotation * Quaternion.Inverse(slot.localRotation);
                turn = Quaternion.Slerp(handFrom, handTurn, w);
            }

            PlaceShieldArm(target, pole, turn);

            // (Its rim into its own head or body: the shield goes further out along its face.)
            if (Rig != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    float deep = ShieldInBody(slot, shield, scale);
                    if (deep <= 0f)
                    {
                        break;
                    }

                    Vector3 face = slot.rotation * Vector3.forward;
                    target += face * (deep + 0.02f * body.SizeFactor);
                    PlaceShieldArm(target, pole, turn);
                }
            }

            // (The arm itself out of its own head and body: the hand goes further out to the side.)
            if (Rig != null)
            {
                Vector3 outward = -ChestFrame(out _, out _);
                for (int i = 0; i < 3; i++)
                {
                    float deep = ArmInBody();
                    if (deep <= 0f)
                    {
                        break;
                    }

                    target += outward * (deep + 0.03f * body.SizeFactor);
                    PlaceShieldArm(target, pole, turn);
                }
            }

            // (Turning up off the floor, its rim would dig in: the hand lifts it as it turns.)
            float gap = GroundClamp.Gap(slot, shield, scale, stance.GroundY) - GroundClamp.Margin * body.SizeFactor;
            if (gap < 0f)
            {
                PlaceShieldArm(target + Vector3.up * -gap, pole, turn);
            }

            // (Coming in and giving way, the arm's whole shape eases from the one it had, so the elbow
            // never changes sides in a step.)
            float ease = Smooth(w / 0.35f);
            if (ease < 1f)
            {
                upper.localRotation = Quaternion.Slerp(upperWas, upper.localRotation, ease);
                fore.localRotation = Quaternion.Slerp(foreWas, fore.localRotation, ease);
                hand.localRotation = Quaternion.Slerp(handWas, hand.localRotation, ease);
            }
        }

        // Milestone 9c: the shield arm fitted inside its joints' limits, carrying on from its last shape
        // (in the keys, and held up toward the threat).
        private int fitFrame, shieldRoll = -1;
        private float shieldRollAngle = float.NaN;

        /// <summary>How fast the shield is turned about its face to suit the wrist (degrees a second).</summary>
        public const float ShieldRollRate = 120f;

        /// <summary>The shield arm's hand to <paramref name="target"/>, turned to <paramref name="turn"/>, the elbow toward <paramref name="pole"/>, fitted inside its joints' limits.</summary>
        private void PlaceShieldArm(Vector3 target, Vector3 pole, Quaternion turn)
        {
            // Milestone 10 (the arm rebuild's step 5): on the arm rig, inside the joints' limits by
            // construction, the elbow moved off its null only where the wrist can't take the turn.
            Vector3 at = Within(PartId.UpperArmL, PartId.ForearmL, PartId.HandL, target, 0.96f);
            ArmRig.Solve(anim, false, new ArmGoal
            {
                point = at,
                handTurn = turn,
                elbowSwivel = ArmRig.SwivelToward(anim, false, at, pole),
                turnWeight = 1f,
                nearNull = true,
            });
        }

        /// <summary>How hard the shield wrist works as posed now: past its limits counts most, then its bend (degrees).</summary>
        private float WristCost()
        {
            // (The arm rig's wrist: the one limit model.)
            Quaternion local = Quaternion.Inverse(anim.RestLocalRotation(PartId.HandL)) * anim.Part(PartId.HandL).localRotation;
            float past = ArmRig.WristAngles(local, ArmRig.WristFrame(anim, false), out float twist, out float flexion, out float deviation);
            return 10f * past + Mathf.Abs(twist) + Mathf.Abs(flexion) + Mathf.Abs(deviation);
        }

        /// <summary>How far the shield's face may turn from the line of the forearm toward the threat (degrees): the centre grip and the wrist.</summary>
        public const float ShieldFaceOffArm = 60f;

        /// <summary>How far across the chest the shield arm may reach (the sine of the angle past straight ahead), and how far behind.</summary>
        public const float ShieldAcross = 0.6f, ShieldBehind = 0.2f;

        /// <summary>The body's own frame at the chest: its right (returned: along the shoulders), up (along the spine) and forward.</summary>
        public Vector3 ChestFrame(out Vector3 up, out Vector3 forward)
        {
            Vector3 right = anim.Part(PartId.UpperArmR).position - anim.Part(PartId.UpperArmL).position;
            up = anim.Part(PartId.Chest).position - anim.Part(PartId.Pelvis).position;
            right = right.sqrMagnitude > 1e-8f ? right.normalized : root.rotation * Vector3.right;
            up = Vector3.ProjectOnPlane(up, right);
            up = up.sqrMagnitude > 1e-8f ? up.normalized : Vector3.up;
            forward = Vector3.Cross(right, up);
            return right;
        }

        /// <summary>A way for the shield arm to reach, kept on its own (left) side of the body and not far behind it.</summary>
        public Vector3 ShieldSide(Vector3 way)
        {
            Vector3 right = ChestFrame(out Vector3 up, out Vector3 forward);
            float x = Vector3.Dot(way, right), y = Vector3.Dot(way, up), z = Vector3.Dot(way, forward);
            x = Mathf.Min(x, ShieldAcross);
            z = Mathf.Max(z, -ShieldBehind);
            Vector3 kept = right * x + up * y + forward * z;
            return kept.sqrMagnitude > 1e-6f ? kept.normalized : -right;
        }

        /// <summary>How deep the shield arm (the far half of the upper arm, the forearm and hand) goes into its own torso or head (metres; 0 clear).</summary>
        public float ArmInBody()
        {
            if (Rig == null)
            {
                return 0f;
            }

            Vector3 shoulder = anim.Part(PartId.UpperArmL).position;
            Vector3 elbow = anim.Part(PartId.ForearmL).position;
            Vector3 wrist = anim.Part(PartId.HandL).position;
            Vector3 knuckles = wrist + (wrist - elbow).normalized * (0.08f * body.SizeFactor);
            float deepest = 0f;
            foreach (var id in ShieldClearOf)
            {
                float allowance = id == PartId.Chest ? BodyClearance.ArmChestAllowance * body.SizeFactor : 0f;
                float d = Mathf.Min(BodyClearance.Distance(Rig, anim, id, Vector3.Lerp(shoulder, elbow, 0.5f), elbow),
                    Mathf.Min(BodyClearance.Distance(Rig, anim, id, elbow, wrist), BodyClearance.Distance(Rig, anim, id, wrist, knuckles))) + allowance;
                deepest = Mathf.Max(deepest, -d);
            }

            return deepest;
        }

        /// <summary>How deep the shield's rim (and middle) go into its wielder's pelvis, spine, chest or head (metres; 0 clear).</summary>
        private float ShieldInBody(Transform slot, WeaponDefinition shield, float scale)
        {
            float deepest = 0f;
            Vector3 centre = slot.position + slot.rotation * (shield.faceCenter * scale);
            for (int i = 0; i <= 12; i++)
            {
                Vector3 p = i == 12 ? centre : centre + slot.rotation * (Quaternion.Euler(0f, 0f, 30f * i) * Vector3.up) * (shield.radius * scale);
                foreach (var id in ShieldClearOf)
                {
                    deepest = Mathf.Max(deepest, -BodyClearance.PointDistance(Rig, anim, id, p));
                }
            }

            return deepest;
        }

        private static readonly PartId[] ShieldClearOf = { PartId.Pelvis, PartId.Spine, PartId.Chest, PartId.Head };

        /// <summary>Which way a positive roll turns the body so its sword (right) side goes down, lying face up.</summary>
        private float RollSign()
        {
            anim.ResetToRest();
            var k = new Key { pelvis = new Vector3(0f, Lie, 0f), pitch = -88f, roll = 20f };
            PoseTorso(k, 1f);
            bool rightDown = anim.Part(PartId.UpperArmR).position.y < anim.Part(PartId.UpperArmL).position.y;
            return rightDown ? 1f : -1f;
        }

        /// <summary>Share of the blend-in each part takes: the legs come first, the arms last.</summary>
        private static float Share(PartId id)
        {
            switch (id)
            {
                case PartId.ThighL:
                case PartId.ThighR:
                case PartId.ShinL:
                case PartId.ShinR:
                case PartId.FootL:
                case PartId.FootR:
                    return 0.7f;
                case PartId.Pelvis:
                    return 0.85f;
                case PartId.Spine:
                case PartId.Chest:
                case PartId.Head:
                    return 1f;
                default:
                    return 1.25f;
            }
        }

        private static float Smooth(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3f - 2f * x);
        }

        // ---- The key poses ----------------------------------------------------------------------

        private float Lie => 0.13f * Mathf.Max(body.SizeFactor, body.WidthFactor);   // pelvis above the ground, lying
        private float HandHeight => 0.06f * body.SizeFactor;                          // wrist above the ground, hand flat on it

        private Vector3 Home(int foot)
        {
            Vector3 p = stance.HomeLocal(foot);
            p.y = body.AnkleHeight;
            return p;
        }

        private void MakeFaceDown()
        {
            float s = body.SizeFactor, w = body.WidthFactor, ph = body.PelvisHeight, leg = body.LegLength;
            int lead = stance.Lead, rear = 1 - lead;

            // 0. Lying on the belly, hands by the chest ready to push, toes tucked.
            var k = Torso(0f, new Vector3(0f, Lie, -0.25f * s), 88f, 0f, 0f, -40f, out var hips, out var shoulders);
            LegsAlongBody(ref k, hips, 88f, 80f, Vector3.down);
            // (Behind a shield: the shield hand out wider, the shield lying flat beside the chest.)
            float wide = Shielded && Loadout.Off != null ? -0.75f * Loadout.Off.radius * Loadout.Scale : 0f;
            k.handL = new Vector3(shoulders[0].x * 1.3f + wide, HandHeight, shoulders[0].z - 0.03f * s);
            k.handR = new Vector3(shoulders[1].x * 1.3f, HandHeight, shoulders[1].z - 0.03f * s);
            k.elbowL = new Vector3(-1f, 0.6f, -0.4f);
            k.elbowR = new Vector3(1f, 0.6f, -0.4f);
            keys[0] = k;

            // 1. Hands and knees: the hips back and up over the knees, hands under the shoulders.
            k = Torso(0.5f, new Vector3(0f, 0.55f * leg, -0.33f * s), 76f, 3f, 3f, -35f, out hips, out shoulders);
            k.ankleL = new Vector3(hips[0].x, 0.08f * s, hips[0].z - 0.46f * leg);
            k.ankleR = new Vector3(hips[1].x, 0.08f * s, hips[1].z - 0.46f * leg);
            k.kneeL = k.kneeR = new Vector3(0f, -1f, 0.15f);
            k.footL = k.footR = 95f;
            k.handL = new Vector3(shoulders[0].x * 1.05f + wide, HandHeight, shoulders[0].z);
            k.handR = new Vector3(shoulders[1].x * 1.05f, HandHeight, shoulders[1].z);
            k.elbowL = new Vector3(-0.5f, 0f, -1f);
            k.elbowR = new Vector3(0.5f, 0f, -1f);
            keys[1] = k;

            // 2. Up on one knee: the lead foot planted on its spot, the rear knee down under the hips.
            k = Torso(1.0f, new Vector3(0f, 0.6f * leg, -0.2f * s), 20f, 4f, 4f, -12f, out hips, out _);
            SetFoot(ref k, lead, Home(lead), new Vector3(0f, 0.3f, 1f), 0f);
            SetFoot(ref k, rear, new Vector3(Home(rear).x, 0.08f * s, hips[rear].z - 0.48f * leg), new Vector3(0f, -1f, 0.25f), 75f);
            k.handL = new Vector3(-0.24f * w, 0.45f * s, 0.25f * s);
            k.handR = new Vector3(0.24f * w, 0.45f * s, 0.25f * s);
            k.elbowL = new Vector3(-0.5f, -1f, -0.4f);
            k.elbowR = new Vector3(0.5f, -1f, -0.4f);
            keys[2] = k;

            Rise(3, 1.35f, ph, s, w, -0.04f, 22f);
            Stand(4, 1.7f, ph, s, w);
        }

        private void MakeFaceUp()
        {
            float s = body.SizeFactor, w = body.WidthFactor, ph = body.PelvisHeight;
            int lead = stance.Lead, rear = 1 - lead;

            // 0. Lying on the back, chin tucked, hands on the ground by the hips. (Behind a shield: rolled
            // a little onto the sword side, the shield arm across the chest, the shield up.)
            float roll = Shielded ? ShieldRoll : 0f;
            var k = Torso(0f, new Vector3(0f, Lie, -0.45f * s), -88f, 0f, 0f, 30f, out var hips, out _, roll);
            LegsAlongBody(ref k, hips, -88f, -75f, Vector3.up);
            k.handL = new Vector3(-0.34f * w, HandHeight, -0.5f * s);
            k.handR = new Vector3(0.34f * w, HandHeight, -0.5f * s);
            k.elbowL = new Vector3(-1f, 0.2f, -0.3f);
            k.elbowR = new Vector3(1f, 0.2f, -0.3f);
            keys[0] = k;

            // 1. Sitting up, knees up and feet drawn in, leaning on the hands behind.
            k = Torso(0.55f, new Vector3(0f, Lie + 0.02f * s, -0.42f * s), 12f, 8f, 8f, 10f, out _, out _, 0.8f * roll);
            k.ankleL = new Vector3(-0.12f * w, body.AnkleHeight, 0.05f * s);
            k.ankleR = new Vector3(0.12f * w, body.AnkleHeight, 0.05f * s);
            k.kneeL = new Vector3(-0.1f, 1f, 0.4f);
            k.kneeR = new Vector3(0.1f, 1f, 0.4f);
            k.handL = new Vector3(-0.27f * w, HandHeight, -0.6f * s);
            k.handR = new Vector3(0.27f * w, HandHeight, -0.6f * s);
            k.elbowL = new Vector3(-1f, 0f, -0.5f);
            k.elbowR = new Vector3(1f, 0f, -0.5f);
            keys[1] = k;

            // 2. Pushing up into a crouch over the feet: one hand on the ground behind, the other forward.
            k = Torso(1.0f, new Vector3(0f, 0.42f * s, -0.16f * s), 38f, 8f, 6f, -15f, out _, out _, 0.5f * roll);
            SetFoot(ref k, lead, Home(lead), new Vector3(0f, 0.5f, 1f), 0f);
            SetFoot(ref k, rear, Home(rear), new Vector3(0f, 0.5f, 1f), 0f);
            k.handR = new Vector3(0.34f * w, HandHeight, -0.4f * s);
            k.elbowR = new Vector3(1f, 0f, -0.5f);
            k.handL = new Vector3(-0.22f * w, 0.42f * s, 0.4f * s);
            k.elbowL = new Vector3(-0.5f, -1f, -0.2f);
            keys[2] = k;

            Rise(3, 1.35f, ph, s, w, -0.04f, 24f);
            Stand(4, 1.7f, ph, s, w);
        }

        // ---- The natural get-ups (the plan "Getting up") ------------------------------------------

        private static void SetHand(ref Key k, int side, Vector3 wrist, Vector3 elbow)
        {
            if (side == Stance.Left)
            {
                k.handL = wrist;
                k.elbowL = elbow;
            }
            else
            {
                k.handR = wrist;
                k.elbowR = elbow;
            }
        }

        /// <summary>
        /// From its back, as a fighter stands up in base: it sits up onto the hand of its back foot's side (for sword
        /// and shield the sword hand, the shield arm left free and toward the enemy), draws in and plants its front
        /// foot, lifts its hips on that hand and that foot while the back leg folds under, comes onto that knee, and
        /// rises off the front foot, the back foot landing on its place in the stance. Never both feet together,
        /// never without two supports.
        /// </summary>
        private void MakeFromBack()
        {
            if (!LegsClearTheHips)
            {
                MakeFromBackLow();
                return;
            }

            float s = body.SizeFactor, w = body.WidthFactor, ph = body.PelvisHeight, leg = body.LegLength;
            // (The foot it plants is its stance's front foot, so it comes up over it into its stance, and the other
            // leg goes back to where it belongs. The hand it posts on is the one across from that foot, unless it is
            // told which (a swordsman's free hand, which is on the planted foot's side).)
            int lead = stance.Lead, rear = 1 - lead;
            int post = PostWith >= 0 ? PostWith : rear, free = 1 - post;
            // (The posting hand's side, the free hand's, and the side of the leg that goes back: +1 its right.)
            float sx = post == Stance.Left ? -1f : 1f, lx = -sx, rx = rear == Stance.Left ? -1f : 1f;
            // (It sits behind the foot it plants by as far as a sitting leg has its foot before it. Sat nearer, the
            // foot was under the hip: a leg folded to nothing, its knee wherever the pose's pole had it, which was
            // up through the trunk and then round behind.)
            float pz = -SitsBack;
            Vector3 plant = Home(lead);
            Vector3 posted = new Vector3(sx * 0.44f * w, HandHeight, pz - 0.1f * s);
            Vector3 postedElbow = new Vector3(sx, 0f, -0.6f);

            // 0. Lying on its back (only where the keys set out from: the body leaves how it really lay).
            var k = Torso(0f, new Vector3(0f, Lie, pz), -88f, 0f, 0f, 30f, out var hips, out _);
            LegsAlongBody(ref k, hips, -88f, -75f, Vector3.up);
            k.handL = new Vector3(-0.34f * w, HandHeight, pz - 0.05f * s);
            k.handR = new Vector3(0.34f * w, HandHeight, pz - 0.05f * s);
            k.elbowL = new Vector3(-1f, 0.2f, -0.3f);
            k.elbowR = new Vector3(1f, 0.2f, -0.3f);
            keys[0] = k;

            // 1. Head up, rolled a little toward the posting hand, which goes down beside the hip; the planted foot drawn in.
            k = Torso(0.35f, new Vector3(sx * 0.03f * s, Lie + 0.02f * s, pz), -50f, 12f, 12f, 28f, out _, out _, 16f * sx);
            SetHand(ref k, post, posted, postedElbow);
            SetHand(ref k, free, new Vector3(lx * 0.2f * w, 0.3f * s, pz + 0.1f * s), new Vector3(lx * 0.6f, -0.6f, -0.3f));
            SetFoot(ref k, lead, new Vector3(plant.x, body.AnkleHeight, plant.z + 0.06f * leg), new Vector3(-rx * 0.1f, 1f, 0.3f), 0f);
            SetFoot(ref k, rear, new Vector3(rx * 0.16f * w, 0.08f * s, pz + 0.85f * leg), new Vector3(rx * 0.6f, 1f, 0.3f), -40f);
            keys[1] = k;

            // 2. Sitting on the hand, leaning toward it and a little back, the back rounded; the foot planted on its
            // place; the other leg out before it, hardly bent; the free arm up before it. (Sat upright with the knees
            // drawn up, the thighs were against the chest: 40 degrees more than a hip bends.)
            k = Torso(0.7f, new Vector3(sx * 0.04f * s, Lie + 0.03f * s, pz + 0.02f * s), -26f, 16f, 14f, 6f, out _, out _, 10f * sx);
            k.lean = 12f * sx;
            SetHand(ref k, post, posted, postedElbow);
            SetHand(ref k, free, new Vector3(lx * 0.2f * w, 0.62f * s, pz + 0.36f * s), new Vector3(lx * 0.5f, -1f, -0.3f));
            SetFoot(ref k, lead, plant, new Vector3(-rx * 0.1f, 1f, 0.5f), 0f);
            SetFoot(ref k, rear, new Vector3(rx * 0.18f * w, 0.08f * s, pz + 0.72f * leg), new Vector3(rx * 0.6f, 1f, 0.3f), -25f);
            keys[2] = k;

            // 3. The bridge: on the hand and the planted foot, the hips lifted high and the trunk well over the hand;
            // the other leg still out before it, its foot light on the ground. (The hips go up before the leg comes
            // in: under low hips the foot went through the hip and the knee swung round, and drawn in as the hips
            // rose, the knee was against the chest.)
            k = Torso(1.0f, new Vector3(sx * 0.1f * s, BridgeHigh * leg, pz + 0.14f * s), 4f, 6f, 4f, -8f, out _, out _, 12f * sx);
            k.lean = 56f * sx;
            SetHand(ref k, post, posted, postedElbow);
            SetHand(ref k, free, new Vector3(lx * 0.22f * w, 0.72f * s, pz + 0.5f * s), new Vector3(lx * 0.5f, -1f, -0.3f));
            SetFoot(ref k, lead, plant, new Vector3(-rx * 0.15f, 0.5f, 1f), 0f);
            SetFoot(ref k, rear, new Vector3(rx * 0.16f * w, 0.08f * s, pz + 0.62f * leg), new Vector3(rx * 0.4f, 1f, 0.4f), -10f);
            keys[3] = k;

            // 4. The leg swung back under the hips, its foot set down behind on its ball; the hand leaving the ground.
            k = Torso(1.34f, new Vector3(sx * 0.03f * s, (BridgeHigh + 0.09f) * leg, pz + 0.2f * s), 24f, 5f, 4f, -12f, out _, out _);
            k.lean = 20f * sx;
            hips = HipsOf(k);
            SetHand(ref k, post, new Vector3(sx * 0.3f * w, 0.4f * s, pz + 0.3f * s), new Vector3(sx * 0.5f, -1f, -0.4f));
            SetHand(ref k, free, new Vector3(lx * 0.22f * w, 0.8f * s, pz + 0.55f * s), new Vector3(lx * 0.5f, -1f, -0.3f));
            SetFoot(ref k, lead, plant, new Vector3(-rx * 0.1f, 0.3f, 1f), 0f);
            // (On its ball the ankle stands this high, and a foot that ends this high is not lifted more on its way.)
            Vector3 behind = new Vector3(Home(rear).x, 0.155f * s, Mathf.Min(Home(rear).z, hips[rear].z - 0.44f * leg));
            SetFoot(ref k, rear, behind, new Vector3(rx * 0.15f, -0.5f, 1f), 55f);
            keys[4] = k;

            // 5, 6. Rising off both feet, and standing.
            Rise(5, 1.68f, ph, s, w, -0.04f, 22f);
            Stand(6, 2.03f, ph, s, w);
            shieldFrom = 0f;
            shieldBy = keys[1].time;
        }

        /// <summary>
        /// A get-up from its back keeps its legs clear of its own hips: it sits behind the foot it plants, lifts its
        /// hips high on that foot and its hand, and only then takes the other leg back under them (a technical
        /// stand-up). Off (as it was): the planted foot under the hip and the other foot swept back through it, the
        /// knees swinging round through the trunk as the folded legs came out of it.
        /// </summary>
        public static bool LegsClearTheHips = true;

        /// <summary>How far before its hips a sitting body has the foot it plants, as a share of its leg's length.</summary>
        public const float SitsBehindFoot = 0.6f;

        /// <summary>How high the hips are lifted on the hand and the planted foot, as a share of the leg's length: high enough for the other leg to go back under them.</summary>
        public const float BridgeHigh = 0.54f;

        /// <summary>
        /// Natural, from its back: how far toward its feet from where its hips lie the body stands up (metres), for
        /// <see cref="Place"/>: as far as puts the foot it plants (its stance's front foot) on its place.
        /// </summary>
        public float SitsBack
        {
            get
            {
                if (!LegsClearTheHips)
                {
                    return BackSits * body.SizeFactor;
                }

                return Mathf.Max(0.1f * body.SizeFactor, SitsBehindFoot * body.LegLength - stance.HomeLocal(stance.Lead).z);
            }
        }

        /// <summary>The get-up from its back as it first was (<see cref="LegsClearTheHips"/> off).</summary>
        private void MakeFromBackLow()
        {
            float s = body.SizeFactor, w = body.WidthFactor, ph = body.PelvisHeight, leg = body.LegLength;
            int rear = PostWith >= 0 ? PostWith : 1 - stance.Lead, lead = 1 - rear;
            // (The posting hand's side, and the front foot's: +1 its right.)
            float sx = rear == Stance.Left ? -1f : 1f, lx = -sx;
            float pz = -BackSits * s;
            Vector3 posted = new Vector3(sx * 0.37f * w, HandHeight, pz - 0.14f * s);
            Vector3 postedElbow = new Vector3(sx, 0f, -0.6f);

            // 0. Lying on its back (only where the keys set out from: the body leaves how it really lay).
            var k = Torso(0f, new Vector3(0f, Lie, pz), -88f, 0f, 0f, 30f, out var hips, out _);
            LegsAlongBody(ref k, hips, -88f, -75f, Vector3.up);
            k.handL = new Vector3(-0.34f * w, HandHeight, pz - 0.05f * s);
            k.handR = new Vector3(0.34f * w, HandHeight, pz - 0.05f * s);
            k.elbowL = new Vector3(-1f, 0.2f, -0.3f);
            k.elbowR = new Vector3(1f, 0.2f, -0.3f);
            keys[0] = k;

            // 1. Head up, rolled a little toward the posting hand, which goes down beside the hip; the front foot drawn in.
            k = Torso(0.35f, new Vector3(sx * 0.03f * s, Lie + 0.02f * s, pz), -50f, 12f, 12f, 28f, out _, out _, 16f * sx);
            SetHand(ref k, rear, posted, postedElbow);
            SetHand(ref k, lead, new Vector3(lx * 0.2f * w, 0.3f * s, pz + 0.1f * s), new Vector3(lx * 0.6f, -0.6f, -0.3f));
            SetFoot(ref k, lead, new Vector3(Home(lead).x, body.AnkleHeight, Home(lead).z - 0.05f * s), new Vector3(lx * 0.1f, 1f, 0.3f), 0f);
            SetFoot(ref k, rear, new Vector3(sx * 0.16f * w, 0.08f * s, pz + 0.75f * leg), new Vector3(sx * 0.8f, 0.5f, 0.3f), -40f);
            keys[1] = k;

            // 2. Sitting on the hand, leaning toward it; the front foot planted on its place; the free arm up before it.
            k = Torso(0.7f, new Vector3(sx * 0.04f * s, Lie + 0.03f * s, pz + 0.02f * s), -8f, 10f, 8f, 5f, out _, out _, 10f * sx);
            k.lean = 12f * sx;
            SetHand(ref k, rear, posted, postedElbow);
            SetHand(ref k, lead, new Vector3(lx * 0.2f * w, 0.62f * s, pz + 0.36f * s), new Vector3(lx * 0.5f, -1f, -0.3f));
            SetFoot(ref k, lead, Home(lead), new Vector3(lx * 0.1f, 1f, 0.5f), 0f);
            SetFoot(ref k, rear, new Vector3(sx * 0.18f * w, 0.08f * s, pz + 0.55f * leg), new Vector3(sx, 0.4f, 0.4f), -20f);
            keys[2] = k;

            // 3. On the hand and the front foot: the hips lifted, the trunk leaning to the hand, the back leg folding under.
            k = Torso(1.0f, new Vector3(sx * 0.06f * s, 0.32f * leg, pz + 0.05f * s), 2f, 8f, 6f, -5f, out _, out _, 14f * sx);
            k.lean = 36f * sx;
            SetHand(ref k, rear, posted, postedElbow);
            SetHand(ref k, lead, new Vector3(lx * 0.22f * w, 0.7f * s, pz + 0.45f * s), new Vector3(lx * 0.5f, -1f, -0.3f));
            SetFoot(ref k, lead, Home(lead), new Vector3(lx * 0.15f, 0.6f, 1f), 0f);
            SetFoot(ref k, rear, new Vector3(sx * 0.12f * w, 0.1f * s, pz - 0.05f * leg), new Vector3(sx * 0.5f, -0.6f, 0.6f), 60f);
            keys[3] = k;

            // 4. A half kneel: the back knee down under the hips, upright, the hand off the ground.
            HalfKneel(4, 1.3f, s, w, leg, lead, rear, sx, lx, pz + 0.1f * s);
            Rise(5, 1.65f, ph, s, w, -0.04f, 22f);
            Stand(6, 2.0f, ph, s, w);
            shieldFrom = 0f;
            shieldBy = keys[1].time;
        }

        /// <summary>
        /// Get-ups are made from the recordings (17 November; the pack "Getting Up"; Combat Feel "Get Ups Are From The
        /// Recordings"). A recorded get-up is not played: Tools/Mocap/Combat/getup_keys.py reads it off, at a dozen
        /// moments, into the same parts the keys here are made of, against where the man ends up standing and with his
        /// standing pose taken off, and <see cref="MakeRecorded"/> makes this body's keys of that table: the hips'
        /// place, pitch, roll and lean, the bends of the spine, chest and head, the ankles, wrists, knees' and elbows'
        /// ways, the feet's pitch. Everything else is as for the keys made here: it sets out from the body as it
        /// really lay, what the hands hold is kept off the ground, the shield comes up, the feet end on the stance's
        /// own spots and the standing pose takes over. A fall caught on a knee or sitting still rises by the made
        /// keys. Off: the made keys, as before.
        /// </summary>
        public static bool FromRecordings = true;

        /// <summary>What the hands hold is kept out of the ground with the clamp's whole weight from a get-up's first step (the body as it lay was brought out of the floor already, as the get-up began). Off: the clamp comes in over the blend from how it lay, as it was first made.</summary>
        public static bool HeldClearFromTheStart = true;

        /// <summary>
        /// Getting up, a blade that his forearm would point into the ground is turned clear of it at the wrist, past
        /// what a wrist turns if it must (Combat Feel "A Rising Fighter's Blade Is Turned Clear Of The Ground"). Off:
        /// as before, his whole arm is thrown up to clear it, and back.
        /// </summary>
        public static bool BladeTurnsClear = true;

        /// <summary>The furthest past a wrist's turn a blade was turned to clear the ground in this get-up (degrees; for the reports).</summary>
        public float BladeTurnedClear;

        /// <summary>How much that turn may change in a step (degrees).</summary>
        public const float BladeTurnChanges = 30f;

        private float bladeMore, bladeMoreWas, bladeLastT = float.MaxValue;
        private int bladeStep, bladeStepSeen = -1;

        /// <summary>How long a recorded get-up is given, against the recording's own time (from his back 3.7 s, from his front 4.5 s as recorded).</summary>
        public static float RecordedTakes = 0.5f;

        // <getup-takes> (made by Tools/Mocap/Combat/getup_keys.py from the pack "Getting Up"; not written by hand)
        // Each row a key: time | pelvis x y z | pitch spine chest head roll lean | ankleL x y z | ankleR x y z | kneeL | kneeR | footL footR | handL | handR | elbowL | elbowR.
        // Positions in leg lengths against where he ends up standing (x to his right, z the way he faces), his standing
        // pose taken off: an ankle reads 0 0 0 on its standing place, the hips' height is a share of his standing height.
        // The pitch and lean are of the line from the hips to between the shoulders; the roll is about that line.
        /// <summary>From lying on his back: sits up, rocks forward over his feet onto them, stands (getup_from_back, 2.6 to 6.3 s of it). The hand on the ground last is his right; the foot that ends in front his left.</summary>
        private const int RecordedBackPost = 1, RecordedBackFront = 0;
        private const float RecordedBackSeconds = 3.70f, RecordedBackPlantedL = 0.64f, RecordedBackPlantedR = 0.64f, RecordedBackStandPitch = 4.7f;
        private static readonly float[][] RecordedBack =
        {
            new[] { 0.000f, 0.102f, 0.126f, -0.829f, -89.151f, -2.899f, -0.576f, 49.328f, 38.772f, -30.482f, 0.126f, 0.011f, 0.157f, -0.094f, -0.019f, -0.101f, 0.120f, 0.992f, 0.036f, 0.977f, 0.211f, 0.010f, -40.000f, -40.000f, -0.179f, 0.186f, -0.855f, 0.674f, 0.062f, -0.924f, -0.747f, -0.663f, -0.051f, 0.128f, -0.977f, -0.172f },
            new[] { 0.091f, 0.113f, 0.123f, -0.820f, -90.086f, 5.832f, 1.875f, 48.957f, 14.358f, -6.896f, 0.126f, 0.013f, 0.157f, -0.095f, -0.010f, -0.094f, 0.102f, 0.994f, 0.028f, 0.948f, 0.317f, 0.024f, -40.000f, -40.000f, -0.246f, 0.101f, -0.809f, 0.617f, 0.076f, -0.900f, -0.588f, -0.730f, -0.349f, 0.246f, -0.903f, -0.352f },
            new[] { 0.182f, 0.050f, 0.129f, -0.793f, -74.146f, 13.504f, 2.410f, 59.252f, 11.532f, -5.481f, 0.141f, 0.021f, 0.169f, -0.107f, 0.015f, -0.111f, 0.185f, 0.983f, 0.008f, 0.876f, 0.478f, -0.063f, -40.000f, -35.630f, -0.306f, 0.081f, -0.824f, 0.335f, 0.126f, -0.894f, -0.349f, -0.720f, -0.600f, 0.526f, -0.694f, -0.491f },
            new[] { 0.273f, 0.038f, 0.150f, -0.734f, -54.527f, 16.477f, 2.733f, 62.672f, 11.425f, -3.349f, 0.127f, 0.055f, 0.182f, -0.127f, 0.029f, -0.180f, 0.169f, 0.985f, -0.028f, 0.647f, 0.759f, -0.075f, -36.238f, -19.676f, -0.335f, 0.105f, -0.790f, 0.307f, 0.107f, -0.823f, -0.780f, 0.020f, -0.625f, 0.331f, -0.592f, -0.735f },
            new[] { 0.364f, 0.073f, 0.161f, -0.706f, -41.515f, 17.711f, 1.723f, 56.751f, 9.881f, -2.554f, 0.095f, 0.026f, 0.161f, -0.113f, 0.020f, -0.219f, 0.123f, 0.992f, 0.005f, 0.517f, 0.855f, -0.037f, -23.049f, -7.728f, -0.293f, 0.102f, -0.833f, 0.338f, 0.085f, -0.792f, -0.875f, 0.344f, -0.341f, 0.441f, -0.363f, -0.821f },
            new[] { 0.455f, 0.134f, 0.194f, -0.669f, -25.167f, 18.016f, 0.103f, 33.112f, 7.791f, 0.819f, 0.074f, 0.014f, 0.134f, -0.071f, 0.013f, -0.190f, 0.162f, 0.984f, 0.069f, 0.411f, 0.911f, 0.044f, -15.552f, -5.679f, -0.199f, 0.118f, -0.857f, 0.408f, 0.082f, -0.765f, -0.881f, 0.367f, -0.299f, 0.556f, -0.096f, -0.826f },
            new[] { 0.545f, 0.193f, 0.279f, -0.465f, -16.696f, 20.342f, 1.092f, 20.617f, 3.185f, 8.352f, 0.059f, 0.002f, 0.135f, -0.054f, 0.004f, -0.146f, 0.306f, 0.891f, 0.336f, 0.460f, 0.811f, 0.362f, -0.749f, 0.374f, -0.101f, 0.161f, -0.771f, 0.502f, 0.103f, -0.714f, -0.704f, 0.469f, -0.534f, 0.616f, 0.224f, -0.755f },
            new[] { 0.636f, 0.185f, 0.426f, -0.285f, 20.814f, 26.445f, 0.433f, -5.782f, 13.115f, 21.563f, 0.018f, 0.006f, 0.050f, -0.017f, 0.001f, -0.057f, 0.219f, 0.696f, 0.684f, 0.114f, 0.666f, 0.737f, -4.386f, 0.401f, -0.238f, 0.446f, -0.207f, 0.679f, 0.209f, -0.440f, -0.686f, 0.638f, 0.348f, 0.000f, -1.000f, -0.300f },
            new[] { 0.727f, 0.112f, 0.744f, -0.225f, 36.292f, 19.876f, 0.053f, -24.826f, 7.338f, 8.375f, 0.002f, 0.004f, -0.034f, 0.001f, 0.000f, 0.031f, -0.067f, 0.446f, 0.892f, -0.140f, 0.396f, 0.908f, -0.543f, 0.328f, -0.312f, 0.922f, 0.418f, 0.422f, 0.575f, 0.345f, -0.797f, 0.225f, -0.561f, 0.837f, -0.193f, -0.513f },
            new[] { 0.818f, 0.030f, 0.939f, -0.124f, 15.775f, 7.166f, 0.379f, -17.009f, 1.332f, 0.563f, -0.002f, 0.001f, -0.036f, 0.000f, 0.000f, 0.033f, -0.208f, 0.257f, 0.944f, -0.017f, 0.205f, 0.979f, 0.092f, 0.289f, -0.342f, 1.506f, 0.329f, 0.039f, 1.529f, 0.424f, -0.788f, -0.419f, -0.451f, 0.628f, -0.650f, 0.428f },
            new[] { 0.909f, -0.004f, 0.989f, -0.039f, 4.424f, 2.171f, -0.258f, -10.252f, -0.575f, -1.254f, -0.002f, 0.000f, -0.015f, 0.000f, 0.000f, 0.014f, -0.292f, 0.176f, 0.940f, 0.064f, 0.085f, 0.994f, 0.392f, 0.072f, -0.368f, 1.587f, 0.244f, 0.024f, 1.672f, 0.315f, -0.621f, -0.744f, -0.246f, 0.452f, -0.681f, 0.575f },
            new[] { 1.000f, 0.031f, 0.985f, -0.013f, 2.470f, 1.847f, 0.084f, -3.272f, 0.156f, -0.461f, -0.000f, 0.002f, 0.001f, 0.000f, 0.000f, 0.001f, -0.243f, 0.162f, 0.956f, 0.145f, 0.049f, 0.988f, 0.840f, 0.021f, -0.307f, 1.562f, 0.300f, 0.029f, 1.594f, 0.338f, -0.557f, -0.812f, -0.175f, 0.418f, -0.792f, 0.445f },
        };

        /// <summary>From lying on his front: pushes up, hips up, a foot stepped through, stands (getup_from_front, 1.6 to 6.1 s of it). The hand on the ground last is his right; the foot that ends in front his left.</summary>
        private const int RecordedFrontPost = 1, RecordedFrontFront = 0;
        private const float RecordedFrontSeconds = 4.50f, RecordedFrontPlantedL = 0.91f, RecordedFrontPlantedR = 0.82f, RecordedFrontStandPitch = 3.6f;
        private static readonly float[][] RecordedFront =
        {
            new[] { 0.000f, 0.028f, 0.118f, 0.330f, 76.119f, -12.337f, -3.843f, 0.710f, 10.192f, -5.904f, 0.208f, 0.041f, -0.893f, -0.114f, 0.026f, -0.646f, -0.057f, -0.998f, -0.020f, 0.212f, -0.977f, 0.012f, 133.072f, 131.082f, -0.382f, 0.229f, 0.253f, 0.487f, 0.196f, 0.266f, 0.200f, 0.980f, -0.017f, -0.259f, 0.946f, -0.195f },
            new[] { 0.091f, 0.017f, 0.120f, 0.329f, 64.252f, -16.466f, -4.973f, 8.969f, 8.482f, -6.053f, 0.199f, 0.116f, -0.880f, -0.125f, 0.066f, -0.640f, 0.060f, -0.991f, -0.119f, 0.289f, -0.957f, -0.015f, 117.301f, 125.325f, -0.252f, 0.187f, 0.447f, 0.395f, 0.164f, 0.434f, -0.047f, 0.933f, -0.358f, 0.152f, 0.919f, -0.365f },
            new[] { 0.182f, -0.018f, 0.262f, 0.316f, 61.196f, -11.672f, -3.604f, 23.186f, 13.214f, -6.109f, 0.210f, 0.106f, -0.888f, -0.101f, 0.100f, -0.620f, 0.118f, -0.993f, 0.017f, 0.534f, -0.833f, 0.145f, 104.463f, 104.423f, -0.290f, 0.201f, 0.428f, 0.356f, 0.168f, 0.469f, -0.376f, 0.768f, -0.519f, 0.445f, 0.624f, -0.642f },
            new[] { 0.273f, -0.065f, 0.473f, 0.248f, 64.252f, -3.060f, -0.861f, 24.437f, 21.099f, -2.973f, 0.211f, 0.099f, -0.913f, -0.091f, 0.073f, -0.513f, 0.250f, -0.943f, 0.221f, 0.750f, -0.507f, 0.424f, 101.978f, 81.040f, -0.332f, 0.208f, 0.410f, 0.318f, 0.171f, 0.514f, -0.503f, 0.534f, -0.679f, 0.579f, 0.272f, -0.769f },
            new[] { 0.364f, -0.115f, 0.700f, 0.149f, 77.363f, 4.698f, 1.570f, -0.751f, 22.392f, -1.052f, 0.319f, 0.114f, -0.939f, -0.096f, 0.072f, -0.311f, 0.332f, -0.818f, 0.470f, 0.746f, -0.163f, 0.645f, 93.290f, 49.781f, -0.386f, 0.238f, 0.386f, 0.266f, 0.201f, 0.570f, -0.661f, 0.388f, -0.642f, 0.659f, 0.067f, -0.749f },
            new[] { 0.455f, -0.056f, 0.743f, 0.018f, 82.921f, 9.578f, 2.996f, -10.655f, 16.977f, -0.820f, 0.341f, 0.101f, -0.955f, -0.143f, 0.029f, -0.200f, 0.331f, -0.765f, 0.553f, 0.663f, -0.077f, 0.744f, 91.975f, 31.654f, -0.390f, 0.223f, 0.342f, 0.310f, 0.200f, 0.539f, -0.777f, 0.365f, -0.513f, 0.807f, 0.041f, -0.590f },
            new[] { 0.545f, 0.056f, 0.705f, -0.188f, 70.418f, 10.305f, 3.273f, -14.335f, 10.536f, -1.021f, 0.418f, 0.079f, -0.978f, -0.105f, 0.029f, -0.166f, 0.263f, -0.719f, 0.643f, 0.506f, 0.112f, 0.855f, 87.389f, 27.218f, -0.302f, 0.318f, 0.025f, 0.336f, 0.222f, 0.389f, -0.635f, 0.462f, -0.619f, 0.000f, -1.000f, -0.300f },
            new[] { 0.636f, 0.069f, 0.796f, -0.182f, 57.669f, 11.904f, 3.603f, -9.216f, 5.316f, -11.281f, 0.352f, 0.134f, -0.596f, -0.077f, 0.020f, -0.132f, 0.102f, -0.354f, 0.930f, 0.411f, 0.123f, 0.903f, 76.880f, 17.954f, -0.290f, 0.520f, -0.100f, 0.277f, 0.439f, 0.360f, -0.551f, 0.522f, -0.651f, 0.886f, -0.146f, -0.440f },
            new[] { 0.727f, -0.063f, 0.893f, 0.041f, 44.081f, 13.609f, 4.113f, -3.819f, 2.313f, -8.733f, 0.043f, 0.022f, 0.079f, -0.043f, 0.011f, -0.078f, -0.039f, 0.353f, 0.935f, 0.277f, -0.090f, 0.957f, 5.477f, 5.561f, -0.449f, 0.646f, 0.371f, 0.004f, 0.788f, 0.756f, -0.846f, 0.296f, -0.444f, 0.904f, -0.408f, -0.123f },
            new[] { 0.818f, -0.142f, 0.892f, 0.068f, 28.597f, 11.520f, 3.626f, -12.369f, 2.956f, -7.653f, 0.032f, 0.005f, 0.062f, -0.018f, 0.026f, -0.031f, -0.139f, 0.293f, 0.946f, 0.243f, -0.067f, 0.968f, 0.720f, 14.641f, -0.580f, 0.808f, 0.503f, -0.031f, 1.038f, 0.837f, -0.781f, 0.127f, -0.611f, 0.898f, -0.437f, -0.054f },
            new[] { 0.909f, 0.006f, 0.979f, 0.023f, 11.863f, 5.352f, 1.692f, -15.958f, 3.515f, -1.553f, 0.027f, 0.002f, 0.049f, -0.021f, 0.005f, -0.047f, -0.083f, 0.290f, 0.953f, 0.354f, -0.042f, 0.934f, -0.962f, 3.484f, -0.327f, 1.282f, 0.469f, -0.044f, 1.443f, 0.530f, -0.779f, -0.284f, -0.559f, 0.536f, -0.742f, 0.404f },
            new[] { 1.000f, 0.013f, 0.991f, 0.023f, 3.756f, 2.492f, 0.839f, -4.733f, 0.946f, 0.241f, 0.005f, 0.000f, 0.011f, -0.007f, 0.000f, -0.013f, -0.129f, 0.257f, 0.958f, 0.389f, 0.005f, 0.921f, -0.393f, 0.626f, -0.279f, 1.548f, 0.290f, 0.059f, 1.569f, 0.353f, -0.696f, -0.629f, -0.348f, 0.192f, -0.821f, 0.538f },
        };

        // </getup-takes>

        /// <summary>The keys made of a recording's table (<see cref="FromRecordings"/>).</summary>
        private void MakeRecorded(float[][] table, int takePost, float seconds, float standPitch, float plantedL, float plantedR)
        {
            float s = body.SizeFactor, leg = body.LegLength, standHigh = body.PelvisHeight - 0.07f * leg;
            // (The hand it posts on is the one the body is told to, or the one across from its stance's front foot: a
            // recording that posts on the other is taken mirrored.)
            int rear = PostWith >= 0 ? PostWith : 1 - stance.Lead;
            bool mirrored = rear != takePost;
            float side = mirrored ? -1f : 1f;
            // (The recording's last moments are the man standing: there the standing pose made here takes over instead.)
            int n = 0;
            while (n < table.Length && n < keys.Length - 1 && table[n][0] <= RecordedUntil)
            {
                n++;
            }

            float total = seconds * RecordedTakes;
            // (The recording's pitch is of the line from the hips to between the shoulders, which is what the hands
            // hang from: the hips here are pitched so this body's own line lies so, whatever its back bends. Standing,
            // that is the standing key's own.)
            float standLine = 4f + TrunkLine(2f, 2f);
            float travel = HipsTravel(table);
            for (int i = 0; i < n; i++)
            {
                float[] r = table[i];
                Vector3 V(int at) => new Vector3(side * r[at], r[at + 1], r[at + 2]);
                float spine = r[5] + 2f * r[0], chest = r[6] + 2f * r[0];
                var k = new Key
                {
                    time = r[0] * total,
                    pelvis = new Vector3(side * r[1] * leg, Mathf.Max(Lie, r[2] * standHigh), r[3] * leg * travel),
                    pitch = r[4] + standPitch * (1f - r[0]) + standLine * r[0] - TrunkLine(spine, chest), spine = spine, chest = chest, head = r[7] - 4f * r[0],
                    roll = side * r[8] / (Mathf.Abs(rollSign) > 0.5f ? rollSign : 1f), lean = side * r[9],
                };
                bool downL = false, downR = false;
                for (int f = 0; f < 2; f++)
                {
                    // (The body's own foot f is the recording's, or with the recording mirrored its other one.)
                    int from = mirrored ? 1 - f : f;
                    // (The recorded man stands with his feet nearly level; this body's stance has one before the other.
                    // Until a foot is on its way to its standing place it is measured from beside the other, as his
                    // is: measured from its own spot from the start, a foot lay a stance's length further off than a
                    // leg reaches.)
                    float planted = from == 0 ? plantedL : plantedR;
                    Vector3 home = Home(f);
                    home.z *= Smooth(Mathf.InverseLerp(planted - 0.35f, planted - 0.1f, r[0]));
                    Vector3 ankle = home + new Vector3(side * r[10 + 3 * from] * leg, r[11 + 3 * from] * leg, r[12 + 3 * from] * leg);
                    ankle.y = Mathf.Max(ankle.y, 0.08f * s);
                    Vector3 knee = V(16 + 3 * from);
                    SetFoot(ref k, f, ankle, knee, r[22 + from]);
                    Vector3 hand = V(24 + 3 * from);
                    // (A wrist the recording has near the ground is a hand on it: flat on the ground here.)
                    bool down = hand.y < HandDownBelow;
                    hand = new Vector3(hand.x * leg, down ? HandHeight : hand.y * leg, hand.z * leg);
                    SetHand(ref k, f, hand, V(30 + 3 * from));
                    downL |= down && f == 0;
                    downR |= down && f == 1;
                }

                HandsReach(ref k, downL, downR);
                k.ankleL = ClearOfHips(k.pelvis, k.ankleL, 0);
                k.ankleR = ClearOfHips(k.pelvis, k.ankleR, 1);
                keys[i] = k;
            }

            if (Side != 0f && Facing == Kind.FaceDown)
            {
                // (On a side: rolling onto its front as it pushes up, as the made keys do.)
                keys[0].roll += -80f * Side;
                keys[1].roll += -35f * Side;
            }

            Stand(n, total, body.PelvisHeight, s, body.WidthFactor);
            count = n + 1;
            // (The shield comes up as with the made keys: from its back at once, from its front once the chest is off the ground.)
            shieldFrom = Facing == Kind.FaceDown ? 0.35f * total : 0f;
            shieldBy = Facing == Kind.FaceDown ? 0.65f * total : 0.2f * total;
        }

        /// <summary>How far forward of straight up the line from the hips to between the shoulders is (degrees), the hips unpitched and the back bent so.</summary>
        private float TrunkLine(float spine, float chest)
        {
            anim.ResetToRest();
            PoseTorso(new Key { pelvis = Vector3.up, spine = spine, chest = chest }, 1f);
            Vector3 line = 0.5f * (Local(anim.Part(PartId.UpperArmL).position) + Local(anim.Part(PartId.UpperArmR).position)) - Vector3.up;
            return Mathf.Atan2(line.z, line.y) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// A recorded key's trunk brought to where the hands it has on the ground reach it. This body is not the
        /// recorded man: its shoulders stand further from its hips, and its wrist lies lower on a flat hand, so with
        /// his hips and his trunk's line its hands hang short of the ground (seen headless: by 20 cm, piked on its
        /// hands). A trunk nearer level than upright is tipped further the way it already tips, by up to
        /// <see cref="TipsToReach"/> degrees; then the hips come down a little (<see cref="HipsLowerToReach"/>); and
        /// what is still left, the hand is off the ground by. A hand's spot further out than the arm reaches is brought in.
        /// </summary>
        private void HandsReach(ref Key k, bool downL, bool downR)
        {
            if (!downL && !downR)
            {
                return;
            }

            float tipped = 0f;
            for (int pass = 0; pass < 12; pass++)
            {
                anim.ResetToRest();
                PoseTorso(k);
                Vector3 line = 0.5f * (Local(anim.Part(PartId.UpperArmL).position) + Local(anim.Part(PartId.UpperArmR).position)) - k.pelvis;
                float over = 0f;
                for (int f = 0; f < 2; f++)
                {
                    if (!(f == 0 ? downL : downR))
                    {
                        continue;
                    }

                    Vector3 shoulder = Local(anim.Part(f == 0 ? PartId.UpperArmL : PartId.UpperArmR).position);
                    Vector3 hand = f == 0 ? k.handL : k.handR;
                    float reach = 0.94f * (anim.RestLocalPosition(f == 0 ? PartId.ForearmL : PartId.ForearmR).magnitude + anim.RestLocalPosition(f == 0 ? PartId.HandL : PartId.HandR).magnitude);
                    Vector3 out2 = new Vector3(hand.x - shoulder.x, 0f, hand.z - shoulder.z);
                    if (out2.magnitude > 0.9f * reach)
                    {
                        out2 = out2.normalized * (0.9f * reach);
                        hand = new Vector3(shoulder.x + out2.x, hand.y, shoulder.z + out2.z);
                        if (f == 0)
                        {
                            k.handL = hand;
                        }
                        else
                        {
                            k.handR = hand;
                        }
                    }

                    over = Mathf.Max(over, shoulder.y - hand.y - Mathf.Sqrt(Mathf.Max(0f, reach * reach - out2.sqrMagnitude)));
                }

                if (over <= 0.005f * body.SizeFactor)
                {
                    return;
                }

                float tips = Mathf.Atan2(line.z, line.y) * Mathf.Rad2Deg;
                if (Mathf.Abs(tips) > 45f && tipped < TipsToReach - 0.5f)
                {
                    k.pitch += Mathf.Sign(tips) * 3f;
                    tipped += 3f;
                    continue;
                }

                // (The hips come down by no more than a little: a trunk that is nearly upright is not sat down for a
                // hand that only just touched. What is left, the hand is off the ground by.)
                float lowered = Mathf.Min(over, HipsLowerToReach * body.LegLength, Mathf.Max(0f, k.pelvis.y - Lie));
                k.pelvis.y -= lowered;
                if (over - lowered > 0.005f * body.SizeFactor)
                {
                    if (downL)
                    {
                        k.handL.y += over - lowered;
                    }

                    if (downR)
                    {
                        k.handR.y += over - lowered;
                    }
                }

                return;
            }
        }

        /// <summary>The most a recorded key's hips come down for its hands to reach the ground, as a share of the leg's length.</summary>
        private const float HipsLowerToReach = 0.1f;

        /// <summary>
        /// A recorded key's foot kept clear of its own hips. The recorded man, sitting up from his back, tucks one
        /// foot back under his other thigh, across his middle, and rises over it. This body sits with its hips on the
        /// ground and that foot went straight through them: under its seat and out at the other side (seen in Unity,
        /// 18 November; headless the shin passed 16 cm from the hips' middle, where the get-ups made by hand come no
        /// nearer than 30). While the hips are low a foot that is near them, fore and aft, stays out at its own side
        /// by <see cref="FootClearsHips"/> of a leg at least: it comes back beside the hip, not under it. Once the
        /// hips are up (<see cref="HipsUpBy"/> of their standing height) the feet are where the recording has them.
        /// </summary>
        private Vector3 ClearOfHips(Vector3 pelvis, Vector3 ankle, int foot)
        {
            if (!FeetClearHips)
            {
                return ankle;
            }

            float leg = body.LegLength, standHigh = body.PelvisHeight - 0.07f * leg, own = foot == 0 ? -1f : 1f;
            float low = 1f - Smooth(Mathf.InverseLerp(HipsUpFrom, HipsUpBy, pelvis.y / standHigh));
            float near = 1f - Smooth(Mathf.InverseLerp(0.45f, 0.75f, Mathf.Abs(ankle.z - pelvis.z) / leg));
            float least = low * near * FootClearsHips * leg;
            float aside = own * (ankle.x - pelvis.x);
            // (The knee is left the way the recording has it. Pointed forward and up for a foot kept out, it bent a
            // thigh 115 degrees past the hip's range in the get-up from the front, where the feet lie behind him.)
            return aside >= least ? ankle : new Vector3(pelvis.x + own * least, ankle.y, ankle.z);
        }

        /// <summary>A recorded get-up's feet are kept clear of its own hips (<see cref="ClearOfHips"/>). Off: where the recording has them, as first made.</summary>
        public static bool FeetClearHips = true;

        /// <summary>How far out at its own side of the hips' middle a foot near low hips stays, as a share of the leg's length.</summary>
        public static float FootClearsHips = 0.3f;

        /// <summary>
        /// The feet come back to where the recording has them as the hips rise from the first of these shares of their
        /// standing height to the second: over the whole rise, not at one height (let go between 0.45 and 0.7, the
        /// knee swung in from the side in a tenth of a second as he stood up, the thigh at 21 rad/s).
        /// </summary>
        public const float HipsUpFrom = 0.3f, HipsUpBy = 0.9f;

        /// <summary>The most a recorded key's trunk is tipped further for its hands to reach the ground (degrees).</summary>
        private const float TipsToReach = 24f;

        /// <summary>A recording is followed up to this share of its time; from there the keys head for the standing pose.</summary>
        private const float RecordedUntil = 0.86f;

        /// <summary>A recorded wrist lower than this (in leg lengths) is a hand on the ground.</summary>
        private const float HandDownBelow = 0.26f;

        /// <summary>
        /// From its front (and from a side, rolling onto its front as it pushes up): the hands planted by the chest
        /// and the chest off the ground, the hips back onto hands and knees, the front foot stepped through to its
        /// place while the hand of the back foot's side stays down, a half kneel, and up.
        /// </summary>
        private void MakeFromFront()
        {
            float s = body.SizeFactor, w = body.WidthFactor, ph = body.PelvisHeight, leg = body.LegLength;
            int rear = PostWith >= 0 ? PostWith : 1 - stance.Lead, lead = 1 - rear;
            float sx = rear == Stance.Left ? -1f : 1f, lx = -sx;
            float pz = -0.25f * s;
            // (Face down, a turn about its length the other way puts its right side down.)
            float onSide = -Side;

            // 0. Lying on its front, or on a side (only where the keys set out from).
            var k = Torso(0f, new Vector3(0f, Lie, pz), 88f, 0f, 0f, -40f, out var hips, out var shoulders, 80f * onSide);
            LegsAlongBody(ref k, hips, 88f, 80f, Vector3.down);
            k.handL = new Vector3(shoulders[0].x * 1.3f, HandHeight, shoulders[0].z - 0.03f * s);
            k.handR = new Vector3(shoulders[1].x * 1.3f, HandHeight, shoulders[1].z - 0.03f * s);
            k.elbowL = new Vector3(-1f, 0.6f, -0.4f);
            k.elbowR = new Vector3(1f, 0.6f, -0.4f);
            keys[0] = k;

            // 1. Head up, the hands planted by the chest, the chest coming off the ground (from a side: rolling onto its front).
            k = Torso(0.35f, new Vector3(0f, Lie + 0.03f * s, pz), 74f, -6f, -8f, -48f, out hips, out shoulders, 35f * onSide);
            LegsAlongBody(ref k, hips, 88f, 80f, Vector3.down);
            Vector3 handL = new Vector3(-Mathf.Abs(shoulders[0].x) * 1.25f, HandHeight, pz + 0.42f * s);
            Vector3 handR = new Vector3(Mathf.Abs(shoulders[1].x) * 1.25f, HandHeight, pz + 0.42f * s);
            k.handL = handL;
            k.handR = handR;
            k.elbowL = new Vector3(-1f, 0.5f, -0.5f);
            k.elbowR = new Vector3(1f, 0.5f, -0.5f);
            keys[1] = k;

            // 2. Hands and knees: the hips back and up over the knees, the weight toward the posting hand; the hands where they were.
            k = Torso(0.7f, new Vector3(sx * 0.03f * s, 0.55f * leg, pz - 0.08f * s), 72f, 3f, 3f, -40f, out hips, out _);
            k.lean = 6f * sx;
            k.ankleL = new Vector3(hips[0].x, 0.08f * s, hips[0].z - 0.46f * leg);
            k.ankleR = new Vector3(hips[1].x, 0.08f * s, hips[1].z - 0.46f * leg);
            k.kneeL = k.kneeR = new Vector3(0f, -1f, 0.15f);
            k.footL = k.footR = 95f;
            k.handL = handL;
            k.handR = handR;
            k.elbowL = new Vector3(-0.5f, 0f, -1f);
            k.elbowR = new Vector3(0.5f, 0f, -1f);
            keys[2] = k;

            // 3. The front foot stepped through to its place; the posting hand still down, the free hand lifting.
            // (The hips higher and less tipped forward for it, the back rounded instead: with the trunk level and
            // the hips low the thigh was folded flat along the belly, 70 degrees more than a hip bends.)
            k = LegsClearTheHips
                ? Torso(1.0f, new Vector3(sx * 0.03f * s, 0.56f * leg, pz + 0.03f * s), 46f, 14f, 12f, -34f, out hips, out _)
                : Torso(1.0f, new Vector3(sx * 0.03f * s, 0.48f * leg, pz + 0.03f * s), 66f, 4f, 4f, -34f, out hips, out _);
            k.lean = 10f * sx;
            SetFoot(ref k, lead, Home(lead), new Vector3(lx * 0.2f, 0.3f, 1f), 0f);
            SetFoot(ref k, rear, new Vector3(hips[rear].x, 0.08f * s, hips[rear].z - 0.46f * leg), new Vector3(0f, -1f, 0.15f), 85f);
            SetHand(ref k, rear, rear == Stance.Left ? handL : handR, new Vector3(sx * 0.5f, 0f, -1f));
            SetHand(ref k, lead, new Vector3(lx * 0.22f * w, 0.45f * s, 0.35f * s), new Vector3(lx * 0.5f, -1f, -0.3f));
            keys[3] = k;

            HalfKneel(4, 1.3f, s, w, leg, lead, rear, sx, lx, -0.2f * s);
            Rise(5, 1.65f, ph, s, w, -0.04f, 22f);
            Stand(6, 2.0f, ph, s, w);
            shieldFrom = keys[2].time;
            shieldBy = keys[4].time;
        }

        /// <summary>A half kneel: the front foot on its place, the back knee down under the hips, upright; the posting hand at the thigh, the free arm up before it.</summary>
        private void HalfKneel(int at, float time, float s, float w, float leg, int lead, int rear, float sx, float lx, float z)
        {
            var k = Torso(time, new Vector3(0f, 0.58f * leg, z), 18f, 5f, 4f, -10f, out var hips, out _);
            SetFoot(ref k, lead, Home(lead), new Vector3(lx * 0.1f, 0.3f, 1f), 0f);
            SetFoot(ref k, rear, new Vector3(Home(rear).x, 0.08f * s, hips[rear].z - 0.48f * leg), new Vector3(0f, -1f, 0.25f), 75f);
            SetHand(ref k, rear, new Vector3(sx * 0.26f * w, 0.42f * s, z + 0.22f * s), new Vector3(sx * 0.5f, -1f, -0.4f));
            SetHand(ref k, lead, new Vector3(lx * 0.22f * w, 0.8f * s, 0.3f * s), new Vector3(lx * 0.5f, -1f, -0.3f));
            keys[at] = k;
        }

        /// <summary>Rising off bent legs, feet on their spots, leaning forward.</summary>
        private void Rise(int at, float time, float ph, float s, float w, float back, float pitch)
        {
            var k = Torso(time, new Vector3(0f, 0.8f * ph, back * s), pitch, 6f, 4f, -10f, out _, out _);
            FeetHome(ref k);
            k.handL = new Vector3(-0.25f * w, 0.68f * s, 0.4f * s);
            k.handR = new Vector3(0.25f * w, 0.68f * s, 0.4f * s);
            k.elbowL = new Vector3(-0.4f, -1f, -0.5f);
            k.elbowR = new Vector3(0.4f, -1f, -0.5f);
            keys[at] = k;
        }

        /// <summary>Standing (the standing pose takes over from here; this is only where the keys head).</summary>
        private void Stand(int at, float time, float ph, float s, float w)
        {
            var k = Torso(time, new Vector3(0f, ph - 0.07f * body.LegLength, 0f), 4f, 2f, 2f, -4f, out _, out _);
            FeetHome(ref k);
            k.handL = new Vector3(-0.2f * w, 1.05f * s, 0.32f * s);
            k.handR = new Vector3(0.2f * w, 1.05f * s, 0.32f * s);
            k.elbowL = new Vector3(-0.6f, -1f, -0.2f);
            k.elbowR = new Vector3(0.6f, -1f, -0.2f);
            keys[at] = k;
        }

        private void FeetHome(ref Key k)
        {
            for (int f = 0; f < 2; f++)
            {
                SetFoot(ref k, f, Home(f), new Vector3(f == Stance.Left ? -0.15f : 0.15f, 0.2f, 1f), 0f);
            }
        }

        private static void SetFoot(ref Key k, int foot, Vector3 ankle, Vector3 knee, float pitch)
        {
            if (foot == Stance.Left)
            {
                k.ankleL = ankle;
                k.kneeL = knee;
                k.footL = pitch;
            }
            else
            {
                k.ankleR = ankle;
                k.kneeR = knee;
                k.footR = pitch;
            }
        }

        /// <summary>Legs lying straight along the body (the knees toward <paramref name="knee"/>, unit space).</summary>
        private void LegsAlongBody(ref Key k, Vector3[] hips, float pitch, float footPitch, Vector3 knee)
        {
            Vector3 down = Quaternion.Euler(pitch, 0f, 0f) * Vector3.down;
            float reach = 0.99f * body.LegLength;
            for (int f = 0; f < 2; f++)
            {
                Vector3 ankle = hips[f] + down * reach;
                ankle.y = Mathf.Max(ankle.y, 0.08f * body.SizeFactor);
                SetFoot(ref k, f, ankle, knee, footPitch);
            }
        }

        /// <summary>A key's hips, spine and head; the hips and shoulders it puts where (unit space), for the limbs.</summary>
        private Key Torso(float time, Vector3 pelvis, float pitch, float spine, float chest, float head, out Vector3[] hips, out Vector3[] shoulders, float roll = 0f)
        {
            var k = new Key { time = time, pelvis = pelvis, pitch = pitch, spine = spine, chest = chest, head = head, roll = roll };
            anim.ResetToRest();
            PoseTorso(k);
            hips = new[] { Local(anim.Part(PartId.ThighL).position), Local(anim.Part(PartId.ThighR).position) };
            shoulders = new[] { Local(anim.Part(PartId.UpperArmL).position), Local(anim.Part(PartId.UpperArmR).position) };
            return k;
        }

        /// <summary>Where a key puts the hips (unit space), its lean and all.</summary>
        private Vector3[] HipsOf(Key k)
        {
            anim.ResetToRest();
            PoseTorso(k);
            return new[] { Local(anim.Part(PartId.ThighL).position), Local(anim.Part(PartId.ThighR).position) };
        }

        private Vector3 Local(Vector3 world) => Quaternion.Inverse(root.rotation) * (world - root.position);

        private Vector3 World(Vector3 local) => root.position + root.rotation * local;

        // ---- Playing them ------------------------------------------------------------------------

        /// <summary>
        /// Poses the animation rig as the keys alone have it <paramref name="t"/> seconds in, with nothing of how it lay
        /// in it and no standing pose: where the body is asked to come to. For weighing one start against another
        /// before a get-up begins (<see cref="Begin"/> again afterwards).
        /// </summary>
        public void PoseAt(float t)
        {
            PoseKeys(Mathf.Max(0f, t - KeysFrom));
        }

        private void PoseKeys(float t)
        {
            int i = 0;
            while (i < count - 2 && t >= keys[i + 1].time)
            {
                i++;
            }

            keysAt = t;
            float span = Mathf.Max(1e-3f, keys[i + 1].time - keys[i].time);
            float u = Mathf.Clamp01((t - keys[i].time) / span);
            u = Mathf.Lerp(u, Smooth(u), 0.5f);   // eased a little, without stopping at each key
            Key a = keys[Mathf.Max(0, i - 1)], b = keys[i], c = keys[i + 1], d = keys[Mathf.Min(count - 1, i + 2)];
            var k = new Key
            {
                pelvis = Spline(a.pelvis, b.pelvis, c.pelvis, d.pelvis, u),
                pitch = Spline(a.pitch, b.pitch, c.pitch, d.pitch, u),
                spine = Spline(a.spine, b.spine, c.spine, d.spine, u),
                chest = Spline(a.chest, b.chest, c.chest, d.chest, u),
                head = Spline(a.head, b.head, c.head, d.head, u),
                roll = Spline(a.roll, b.roll, c.roll, d.roll, u),
                lean = Spline(a.lean, b.lean, c.lean, d.lean, u),
                ankleL = Spline(a.ankleL, b.ankleL, c.ankleL, d.ankleL, u) + Vector3.up * Lift(b.ankleL, c.ankleL, u),
                ankleR = Spline(a.ankleR, b.ankleR, c.ankleR, d.ankleR, u) + Vector3.up * Lift(b.ankleR, c.ankleR, u),
                kneeL = Vector3.Lerp(b.kneeL, c.kneeL, u),
                kneeR = Vector3.Lerp(b.kneeR, c.kneeR, u),
                footL = Mathf.Lerp(b.footL, c.footL, u),
                footR = Mathf.Lerp(b.footR, c.footR, u),
                handL = Spline(a.handL, b.handL, c.handL, d.handL, u) + Vector3.up * Lift(b.handL, c.handL, u),
                handR = Spline(a.handR, b.handR, c.handR, d.handR, u) + Vector3.up * Lift(b.handR, c.handR, u),
                elbowL = Vector3.Lerp(b.elbowL, c.elbowL, u),
                elbowR = Vector3.Lerp(b.elbowR, c.elbowR, u),
            };

            // Nothing below the ground.
            k.pelvis.y = Mathf.Max(k.pelvis.y, 0.8f * Lie);
            k.ankleL.y = Mathf.Max(k.ankleL.y, 0.07f * body.SizeFactor);
            k.ankleR.y = Mathf.Max(k.ankleR.y, 0.07f * body.SizeFactor);
            k.handL.y = Mathf.Max(k.handL.y, 0.9f * HandHeight);
            k.handR.y = Mathf.Max(k.handR.y, 0.9f * HandHeight);
            Pose(k);
        }

        /// <summary>A foot or hand moving from one spot on the ground to another is lifted on the way.</summary>
        private float Lift(Vector3 from, Vector3 to, float u)
        {
            Vector3 d = to - from;
            d.y = 0f;
            float s = body.SizeFactor;
            bool low = from.y < 0.15f * s && to.y < 0.15f * s;
            return low ? 0.1f * s * Mathf.Sin(Mathf.PI * u) * Mathf.Clamp01(d.magnitude / (0.25f * s)) : 0f;
        }

        private static float Spline(float a, float b, float c, float d, float u)
        {
            float u2 = u * u, u3 = u2 * u;
            return 0.5f * (2f * b + (c - a) * u + (2f * a - 5f * b + 4f * c - d) * u2 + (3f * b - a - 3f * c + d) * u3);
        }

        private static Vector3 Spline(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float u) =>
            new Vector3(Spline(a.x, b.x, c.x, d.x, u), Spline(a.y, b.y, c.y, d.y, u), Spline(a.z, b.z, c.z, d.z, u));

        private void PoseTorso(Key k) => PoseTorso(k, rollSign);

        private void PoseTorso(Key k, float sign)
        {
            var pelvis = anim.Part(PartId.Pelvis);
            pelvis.SetPositionAndRotation(World(k.pelvis), root.rotation * Quaternion.Euler(0f, 0f, -k.lean) * Quaternion.Euler(k.pitch, 0f, 0f) * Quaternion.Euler(0f, sign * k.roll, 0f));
            Bend(PartId.Spine, k.spine);
            Bend(PartId.Chest, k.chest);
            Bend(PartId.Head, k.head);
        }

        private void Bend(PartId id, float degrees)
        {
            anim.Part(id).localRotation = Quaternion.Euler(degrees, 0f, 0f) * anim.RestLocalRotation(id);
        }

        private void Pose(Key k)
        {
            anim.ResetToRest();
            PoseTorso(k);
            var facing = root.rotation;
            Leg(PartId.ThighL, PartId.ShinL, PartId.FootL, k.ankleL, k.kneeL, k.footL, facing);
            Leg(PartId.ThighR, PartId.ShinR, PartId.FootR, k.ankleR, k.kneeR, k.footR, facing);
            Arm(PartId.UpperArmL, PartId.ForearmL, PartId.HandL, anim.SlotWeaponL, k.handL, k.elbowL, false, facing);
            Arm(PartId.UpperArmR, PartId.ForearmR, PartId.HandR, anim.SlotWeaponR, k.handR, k.elbowR, true, facing);
            Planted(0, k.ankleL, PartId.FootL, 1.15f * Mathf.Max(body.AnkleHeight, 0.08f * body.SizeFactor));
            Planted(1, k.ankleR, PartId.FootR, 1.15f * Mathf.Max(body.AnkleHeight, 0.08f * body.SizeFactor));
            Planted(2, k.handL, PartId.HandL, 1.15f * HandHeight);
            Planted(3, k.handR, PartId.HandR, 1.15f * HandHeight);
        }

        /// <summary>For the reports: a hand or foot the key has on the ground, against where the body has it.</summary>
        private void Planted(int which, Vector3 asked, PartId end, float downWithin)
        {
            bool down = asked.y <= downWithin;
            Vector3 at = anim.Part(end).position;
            if (down)
            {
                float miss = Vector3.Distance(World(asked), at);
                if (miss > ReachMiss)
                {
                    ReachMiss = miss;
                    Missed = $"its {end} at {keysAt:0.00} s";
                }
                if (endDown[which] && Vector3.Distance(asked, endAsked[which]) < 0.002f * body.SizeFactor)
                {
                    Vector3 moved = at - endWas[which];
                    moved.y = 0f;
                    Slid += moved.magnitude;
                }
            }

            endDown[which] = down;
            endAsked[which] = asked;
            endWas[which] = at;
        }

        private void Leg(PartId thigh, PartId shin, PartId foot, Vector3 ankle, Vector3 knee, float pitch, Quaternion facing)
        {
            Vector3 at = Within(thigh, shin, foot, World(ankle), 0.985f);
            if (Natural && LegsClearTheHips)
            {
                // (No nearer its hip than a leg folds: nearer, the knee is wherever the pole says, and swings round as
                // the foot goes by.)
                Vector3 from = anim.Part(thigh).position, to = at - from;
                float least = FoldsTo * (anim.RestLocalPosition(shin).magnitude + anim.RestLocalPosition(foot).magnitude);
                if (to.magnitude < least)
                {
                    at = from + (to.sqrMagnitude > 1e-6f ? to.normalized : Vector3.down) * least;
                }
            }

            TwoBoneIK.Solve(anim.Part(thigh), anim.Part(shin), anim.Part(foot), at, facing * knee);
            anim.Part(foot).rotation = facing * Quaternion.Euler(pitch, 0f, 0f);
        }

        /// <summary>The nearest a leg's ankle comes to its hip, as a share of the leg's length (a knee bent as far as it goes, 145 degrees, is 0.30).</summary>
        public const float FoldsTo = 0.31f;

        /// <summary>
        /// A limb's target brought in to a share of its full length: a limb that locks straight
        /// swings its middle joint across in a step as the reach runs out, so the hand or foot gives
        /// a little instead.
        /// </summary>
        private Vector3 Within(PartId upper, PartId middle, PartId end, Vector3 target, float share)
        {
            float length = anim.RestLocalPosition(middle).magnitude + anim.RestLocalPosition(end).magnitude;
            Vector3 from = anim.Part(upper).position;
            Vector3 to = target - from;
            float most = share * length;
            return to.magnitude > most ? from + to.normalized * most : target;
        }

        private void Arm(PartId upper, PartId fore, PartId hand, Transform slot, Vector3 wrist, Vector3 elbow, bool right, Quaternion facing)
        {
            var keyed = KeyArm(upper, fore, hand, slot, wrist, elbow, right, facing, null);

            // Milestone 9c: what it holds rests on the ground rather than in it: the hand stands as
            // high as that needs (the arm solved again the same way).
            var held = Loadout != null ? (right ? Loadout.Main : Loadout.Off) : null;
            if (held != null && slot != null && (right ? HoldsMain : HoldsOff))
            {
                float gap = GroundClamp.Gap(slot, held, Loadout.Scale, stance.GroundY) - GroundClamp.Margin * body.SizeFactor;
                if (right) { dbgKeyGap = gap; dbgWristY = wrist.y; }
                if (gap < 0f)
                {
                    Vector3 raised = wrist + Vector3.up * -gap;
                    // (The elbow kept where the key put it: raised, the hand's line may run along the key's elbow way.)
                    KeyArm(upper, fore, hand, slot, raised, elbow, right, facing, keyed);
                }
            }

            if (right && pinned)
            {
                // (Where the get-up's own keys have the sword hand: what the pin lets it go to, in the end.)
                pinKeyY = anim.Part(hand).position.y;
                pinPole = facing * elbow;
            }
        }

        // ---- The sword hand stays where it lies (19 November, Renier: "pinned to the ground in one position") --------

        /// <summary>
        /// Getting up, his sword hand stays where it lay when the get-up began, on the ground, turned as it lay, until
        /// his arm can no longer reach it or the get-up has that hand well off the ground; then it goes to where the
        /// get-up has it. Pinned it may turn only <see cref="SwordHandMayTurn"/> degrees, and it never turns faster than
        /// <see cref="SwordHandTurns"/> degrees a second (Combat Feel "A Rising Fighter's Sword Hand Stays Where It
        /// Lies"). Off: as before, the hand goes where each key of the get-up puts it, and the sword turns about with it.
        /// </summary>
        public static bool SwordHandStays = true;

        /// <summary>How far the pinned hand may turn from how it lay (degrees), and the fastest it ever turns in a get-up (degrees a second).</summary>
        public static float SwordHandMayTurn = DefaultSwordHandMayTurn, SwordHandTurns = DefaultSwordHandTurns;

        public const float DefaultSwordHandMayTurn = 5f, DefaultSwordHandTurns = 180f;

        /// <summary>The get-up's own place for the hand this high over the ground (metres at a Medium's size) lets it go; and how long it takes to go from where it lay to where the get-up has it (seconds).</summary>
        public const float SwordHandLeavesAt = 0.45f, SwordHandLetsGoOver = 0.4f;

        /// <summary>For the reports: how long the hand stayed pinned in this get-up (seconds), and the fastest it turned (degrees a second).</summary>
        public float SwordHandPinnedFor, SwordHandTurnedAt;

        private bool pinned, pinTurnKnown;
        private Vector3 pinAt;
        private Quaternion pinRot, pinTurn;
        private float pinFree, pinT = -1f, pinDt = 1f / 60f;

        private Vector3 pinLay, pinPole = Vector3.back;
        private float pinKeyY;

        private void BeginSwordHand()
        {
            pinned = SwordHandStays && anim != null && Loadout != null && Loadout.Main != null && !Loadout.Main.IsShield;
            pinFree = 0f;
            pinT = -1f;
            pinTurnKnown = false;
            SwordHandPinnedFor = SwordHandTurnedAt = 0f;
            if (!pinned)
            {
                return;
            }

            var hand = anim.Part(PartId.HandR);
            pinLay = pinAt = hand.position;
            pinRot = hand.rotation;
            pinKeyY = stance.GroundY;
            // (On the ground: a hand that lay on his own body, or was still in the air as he came to rest, comes down
            // to the ground under it once, and stays there.)
            float low = stance.GroundY + HandHeight;
            pinAt.y = Mathf.Clamp(pinAt.y, low, low + 0.1f * body.SizeFactor);
        }

        /// <summary>How long a hand that did not lie on the ground takes to come down to it as the get-up begins (seconds).</summary>
        public const float SwordHandComesDownOver = 0.3f;

        /// <summary>
        /// The last word on the sword hand, after the get-up's keys, its blend in from how he lay and its blend out to
        /// his stand have all had theirs: so nothing moves it or turns it but this.
        /// </summary>
        private void PinSwordHand(float t)
        {
            const PartId upper = PartId.UpperArmR, fore = PartId.ForearmR, hand = PartId.HandR;
            var h = anim.Part(hand);
            Vector3 keyedAt = h.position;
            Quaternion keyedRot = h.rotation;
            Vector3 shoulder = anim.Part(upper).position, elbowAt = anim.Part(fore).position;
            if (pinFree <= 0f)
            {
                float arm = (elbowAt - shoulder).magnitude + (keyedAt - elbowAt).magnitude;
                if ((pinAt - shoulder).magnitude > 0.97f * arm || pinKeyY > stance.GroundY + SwordHandLeavesAt * body.SizeFactor)
                {
                    pinFree = 1e-4f;
                }
                else
                {
                    SwordHandPinnedFor += pinDt;
                }
            }

            if (pinFree > 0f)
            {
                pinFree = Mathf.Min(1f, pinFree + pinDt / SwordHandLetsGoOver);
            }

            float down = Mathf.Clamp01(t / SwordHandComesDownOver);
            Vector3 lies = Vector3.Lerp(pinLay, pinAt, down * down * (3f - 2f * down));
            float free = pinFree * pinFree * (3f - 2f * pinFree);
            Vector3 at = Vector3.Lerp(lies, keyedAt, free);
            Quaternion want = pinFree > 0f ? keyedRot : Quaternion.RotateTowards(pinRot, keyedRot, SwordHandMayTurn);
            Vector3 mid = 0.5f * (shoulder + keyedAt);
            pinPoleNow = (elbowAt - mid).sqrMagnitude > 1e-4f ? (elbowAt - mid).normalized : pinPole;
            TwoBoneIK.Solve(anim.Part(upper), anim.Part(fore), h, Within(upper, fore, hand, at, 0.96f), pinPoleNow);
            h.rotation = want;
        }

        /// <summary>The fastest the sword hand travels in a get-up (m/s at a Medium's size).</summary>
        public const float SwordHandMoves = 3f;

        private Vector3 pinPoleNow = Vector3.back, pinWas;

        /// <summary>
        /// After the rule that keeps what he holds out of the ground has had its say too: the sword hand turns no faster
        /// than <see cref="SwordHandTurns"/> and travels no faster than <see cref="SwordHandMoves"/> from where it was
        /// the step before. A blade that would have been flicked clear of the ground in one step comes clear over several.
        /// </summary>
        private void SettleSwordHand()
        {
            const PartId upper = PartId.UpperArmR, fore = PartId.ForearmR, hand = PartId.HandR;
            var h = anim.Part(hand);
            Quaternion want = h.rotation;
            Vector3 wantAt = h.position;
            if (!pinTurnKnown)
            {
                pinTurn = want;
                pinWas = wantAt;
                pinTurnKnown = true;
                return;
            }

            // (Let go and coming round to where the get-up has it, it may turn twice as fast: his stand is taking over.)
            Quaternion turned = Quaternion.RotateTowards(pinTurn, want, SwordHandTurns * (pinFree >= 1f ? 2f : 1f) * pinDt);
            Vector3 moved = Vector3.MoveTowards(pinWas, wantAt, SwordHandMoves * body.SizeFactor * (pinFree >= 1f ? 2f : 1f) * pinDt);
            if (pinDt > 1e-5f)
            {
                SwordHandTurnedAt = Mathf.Max(SwordHandTurnedAt, Quaternion.Angle(pinTurn, turned) / pinDt);
            }

            pinTurn = turned;
            pinWas = moved;
            // (To the end of the get-up: the limits hold after the hand is let go too.)
            if ((moved - wantAt).sqrMagnitude > 1e-8f)
            {
                TwoBoneIK.Solve(anim.Part(upper), anim.Part(fore), h, Within(upper, fore, hand, moved, 0.96f), pinPoleNow);
                pinWas = h.position;
            }

            h.rotation = turned;
        }

        /// <summary>
        /// Milestone 10 (the arm rebuild's step 5): an arm in a key on the arm rig: the wrist to its
        /// place, the elbow's null toward the key's elbow way, the hand turned as it holds what it
        /// holds (a shield's turn kept, the elbow moving for it; a weapon's as far as the wrist goes;
        /// a bare hand straight on the wrist), inside the joints' limits by construction.
        /// </summary>
        private ArmSolve KeyArm(PartId upper, PartId fore, PartId hand, Transform slot, Vector3 wrist, Vector3 elbow, bool right, Quaternion facing, ArmSolve? from)
        {
            Vector3 at = Within(upper, fore, hand, World(wrist), 0.96f);
            if (right)
            {
                // The weapon arm as before, for now: its weapon is laid out by the hand exactly, which
                // asks more of the wrist than it has (up to 100 degrees past); on the rig, either the
                // weapon points into the ground or the elbow flips to make room, and the hand pops.
                // (Its keys want reworking for the rig: the weapon laid out by the arm, not the wrist.)
                TwoBoneIK.Solve(anim.Part(upper), anim.Part(fore), anim.Part(hand), at, facing * elbow);
                HoldIn(upper, fore, hand, slot, wrist, elbow, right, facing);
                return default;
            }

            var goal = new ArmGoal { point = at, elbowSwivel = ArmRig.SwivelToward(anim, right, at, facing * elbow), nearNull = true };
            bool holds = right ? HoldsMain : HoldsOff;
            if (from.HasValue)
            {
                // Raised off the ground: the same arm, moved up (the elbow and the wrist as the key's solve
                // left them; the key's elbow way may run along the raised hand's line).
                goal.elbowSwivel = from.Value.swivel;
                goal.wristGiven = true;
                goal.wristLocal = from.Value.wristLocal;
            }
            else if (holds && slot != null)
            {
                // (A shield's turn is kept, the elbow moving for it. A weapon laid out by the hand asks
                // more of the wrist than it has: the elbow keeps to the key (moved for the turn it would
                // flip between two places from one step to the next), and the hand rolls about the grip
                // instead, which keeps the blade's line.)
                HoldIn(upper, fore, hand, slot, wrist, elbow, right, facing);
                goal.handTurn = anim.Part(hand).rotation;
                var held = Loadout != null ? (right ? Loadout.Main : Loadout.Off) : null;
                if (held != null && held.IsShield)
                {
                    goal.turnWeight = 1f;
                }
                else
                {
                    goal.rollAxisInHand = slot.localRotation * Vector3.forward;
                    goal.rollAllowance = KeyGripRoll;
                }
            }
            else
            {
                goal.wristGiven = true;
                goal.wristLocal = anim.RestLocalRotation(hand);
            }

            return ArmRig.Solve(anim, right, goal);
        }

        /// <summary>How far a weapon laid out in a get-up may roll about its grip to suit the wrist (degrees each way).</summary>
        public const float KeyGripRoll = 90f;

        /// <summary>How the hand holds what it holds in a key.</summary>
        private void HoldIn(PartId upper, PartId fore, PartId hand, Transform slot, Vector3 wrist, Vector3 elbow, bool right, Quaternion facing)
        {
            bool holds = right ? HoldsMain : HoldsOff;
            if (holds && slot != null && !right && Shielded)
            {
                // Milestone 9c: the shield flat on the floor under the forearm, face down, as the hands
                // push (face down; face up it is held up after the keys, see HoldShieldUp).
                anim.Part(hand).rotation = Quaternion.LookRotation(Vector3.down, facing * Vector3.forward) * Quaternion.Inverse(slot.localRotation);
            }
            else if (holds && slot != null)
            {
                // What it holds is laid out to the side, a little raised, clear of the body and the ground;
                // with the hand on the ground (milestone 9c) the blade lies flat along it, out to the side
                // and back, away from the head and body.
                float side = right ? 1f : -1f;
                // (Natural: a weapon whose hand is off the ground is held up before it, toward what it fights.)
                Vector3 raised = facing * (Natural ? new Vector3(0.3f * side, 0.5f, 0.8f) : new Vector3(0.75f * side, 0.25f, 0.6f));
                Vector3 flat = facing * new Vector3(0.85f * side, 0f, -0.5f);
                float low = 1f - Mathf.Clamp01((wrist.y - HandHeight) / (0.25f * body.SizeFactor));
                Vector3 dir = Vector3.Slerp(raised.normalized, flat.normalized, low);
                Vector3 up = Vector3.Slerp(facing * Vector3.up, Vector3.Cross(Vector3.up, flat.normalized), low);
                var asked = Quaternion.LookRotation(dir, up) * Quaternion.Inverse(slot.localRotation);
                // (Natural: as far as a wrist turns. Laid out by the hand exactly, the weapon asked the wrist for up to
                // 100 degrees more than it has; the physical hand could not follow and the weapon swung about.)
                Quaternion wristRest = anim.Part(hand).parent.rotation * anim.RestLocalRotation(hand);
                anim.Part(hand).rotation = Natural ? Quaternion.RotateTowards(wristRest, asked, WristTurns) : asked;
                var held = Loadout != null ? (right ? Loadout.Main : Loadout.Off) : null;
                if (Natural && BladeTurnsClear && held != null && !held.IsShield)
                {
                    // (19 November, the sword that shook as he got up. Turned only as far as a wrist turns, the blade goes
                    // where his forearm points, and for a tenth of a second as the arm comes round that is down into the
                    // ground, up to half a metre. The arm was then thrown up by as much to clear it, which swung the blade
                    // up over his head, and back the next moment: the hand 45 cm up and down in five steps. Instead the
                    // wrist is turned on toward where the blade was asked to lie, only as far as lifts it clear.)
                    float margin = GroundClamp.Margin * body.SizeFactor;
                    float full = Quaternion.Angle(wristRest, asked);
                    float more = 0f;
                    if (full > WristTurns && GroundClamp.Gap(slot, held, Loadout.Scale, stance.GroundY) < margin)
                    {
                        float lo = WristTurns, hi = full;
                        for (int k = 0; k < 8; k++)
                        {
                            float mid = 0.5f * (lo + hi);
                            anim.Part(hand).rotation = Quaternion.RotateTowards(wristRest, asked, mid);
                            if (GroundClamp.Gap(slot, held, Loadout.Scale, stance.GroundY) < margin)
                            {
                                lo = mid;
                            }
                            else
                            {
                                hi = mid;
                            }
                        }

                        more = hi - WristTurns;
                    }

                    // (No faster than a wrist turns: what is still short for a step, the arm's own raise takes up.)
                    if (bladeStepSeen != bladeStep)
                    {
                        bladeStepSeen = bladeStep;
                        bladeMoreWas = bladeMore;
                    }

                    bladeMore = Mathf.Clamp(more, bladeMoreWas - BladeTurnChanges, bladeMoreWas + BladeTurnChanges);
                    anim.Part(hand).rotation = Quaternion.RotateTowards(wristRest, asked, Mathf.Min(full, WristTurns + bladeMore));
                    BladeTurnedClear = Mathf.Max(BladeTurnedClear, bladeMore);
                }
            }
            else
            {
                anim.Part(hand).localRotation = anim.RestLocalRotation(hand);
            }
        }
    }
}
