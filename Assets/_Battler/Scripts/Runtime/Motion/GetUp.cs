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
        private readonly Key[] keys = new Key[5];
        private readonly PoseBuffer lying = new PoseBuffer();
        private readonly PoseBuffer keyed = new PoseBuffer();
        private readonly PoseBuffer mixed = new PoseBuffer();
        private float pace = 1f;

        public Kind Facing { get; private set; }

        /// <summary>Seconds from the ragdoll's pose to the first key (the legs take a little less).</summary>
        public float BlendIn => 0.3f * pace;

        /// <summary>When the standing pose starts taking over (seconds into the get-up), and the whole get-up.</summary>
        public float StandFrom => BlendIn + keys[3].time;
        public float Duration => BlendIn + keys[4].time;

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
            Vector3 along = kind == Kind.FaceDown ? headward : -headward;
            along.y = 0f;
            along = along.sqrMagnitude > 1e-6f ? along.normalized : Vector3.forward;
            yaw = Mathf.Atan2(along.x, along.z) * Mathf.Rad2Deg;
            position = pelvis + along * ((kind == Kind.FaceDown ? 0.25f : 0.45f) * size);
        }

        /// <summary>Starts a get-up: <paramref name="from"/> is how the body lay (the root already placed).</summary>
        public void Begin(Kind kind, PoseBuffer from)
        {
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
            pace = Mathf.Sqrt(Mathf.Max(0.3f, s));
            rollSign = RollSign();
            if (kind == Kind.FaceDown)
            {
                MakeFaceDown();
            }
            else
            {
                MakeFaceUp();
            }

            for (int i = 0; i < keys.Length; i++)
            {
                keys[i].time *= pace;
            }
        }

        /// <summary>
        /// Poses the animation rig <paramref name="t"/> seconds into the get-up. From
        /// <see cref="StandFrom"/> on, <paramref name="stand"/> (the standing pose, as the pose
        /// pipeline makes it) takes over.
        /// </summary>
        public void Compose(float t, PoseBuffer stand)
        {
            fitFrame++;
            PoseKeys(Mathf.Max(0f, t - BlendIn));
            bool blendingIn = t < BlendIn * 1.25f;
            bool standing = stand != null && t > StandFrom;
            if (!blendingIn && !standing)
            {
                Finish(t, stand);
                return;
            }

            keyed.Capture(anim);
            if (blendingIn)
            {
                // From the ragdoll's pose to the first key, each part on its own clock: legs first.
                mixed.CopyFrom(lying);
                mixed.MixToward(keyed, id => Smooth(t / (BlendIn * Share(id))));
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
        }

        /// <summary>
        /// Milestone 9c: after the keys, the shield is held up toward the threat (getting up face up all
        /// along; face down once off the hands), giving way to the standing pose's shield as it takes
        /// over; then what the hands hold is kept out of the ground.
        /// </summary>
        private void Finish(float t, PoseBuffer stand)
        {
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
                    float k = t - BlendIn;
                    up = Smooth((k - keys[1].time) / Mathf.Max(1e-3f, keys[2].time - keys[1].time));
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
            WeaponGroundGap = PosePipeline.KeepWeaponsOffGround(anim, Loadout, body.SizeFactor, stance.GroundY, Smooth(t / BlendIn));
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

            Rise(1.35f, ph, s, w, -0.04f, 22f);
            Stand(1.7f, ph, s, w);
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

            Rise(1.35f, ph, s, w, -0.04f, 24f);
            Stand(1.7f, ph, s, w);
        }

        /// <summary>Rising off bent legs, feet on their spots, leaning forward.</summary>
        private void Rise(float time, float ph, float s, float w, float back, float pitch)
        {
            var k = Torso(time, new Vector3(0f, 0.8f * ph, back * s), pitch, 6f, 4f, -10f, out _, out _);
            FeetHome(ref k);
            k.handL = new Vector3(-0.25f * w, 0.68f * s, 0.4f * s);
            k.handR = new Vector3(0.25f * w, 0.68f * s, 0.4f * s);
            k.elbowL = new Vector3(-0.4f, -1f, -0.5f);
            k.elbowR = new Vector3(0.4f, -1f, -0.5f);
            keys[3] = k;
        }

        /// <summary>Standing (the standing pose takes over from here; this is only where the keys head).</summary>
        private void Stand(float time, float ph, float s, float w)
        {
            var k = Torso(time, new Vector3(0f, ph - 0.07f * body.LegLength, 0f), 4f, 2f, 2f, -4f, out _, out _);
            FeetHome(ref k);
            k.handL = new Vector3(-0.2f * w, 1.05f * s, 0.32f * s);
            k.handR = new Vector3(0.2f * w, 1.05f * s, 0.32f * s);
            k.elbowL = new Vector3(-0.6f, -1f, -0.2f);
            k.elbowR = new Vector3(0.6f, -1f, -0.2f);
            keys[4] = k;
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

        private Vector3 Local(Vector3 world) => Quaternion.Inverse(root.rotation) * (world - root.position);

        private Vector3 World(Vector3 local) => root.position + root.rotation * local;

        // ---- Playing them ------------------------------------------------------------------------

        private void PoseKeys(float t)
        {
            int i = 0;
            while (i < keys.Length - 2 && t >= keys[i + 1].time)
            {
                i++;
            }

            float span = Mathf.Max(1e-3f, keys[i + 1].time - keys[i].time);
            float u = Mathf.Clamp01((t - keys[i].time) / span);
            u = Mathf.Lerp(u, Smooth(u), 0.5f);   // eased a little, without stopping at each key
            Key a = keys[Mathf.Max(0, i - 1)], b = keys[i], c = keys[i + 1], d = keys[Mathf.Min(keys.Length - 1, i + 2)];
            var k = new Key
            {
                pelvis = Spline(a.pelvis, b.pelvis, c.pelvis, d.pelvis, u),
                pitch = Spline(a.pitch, b.pitch, c.pitch, d.pitch, u),
                spine = Spline(a.spine, b.spine, c.spine, d.spine, u),
                chest = Spline(a.chest, b.chest, c.chest, d.chest, u),
                head = Spline(a.head, b.head, c.head, d.head, u),
                roll = Spline(a.roll, b.roll, c.roll, d.roll, u),
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
            pelvis.SetPositionAndRotation(World(k.pelvis), root.rotation * Quaternion.Euler(k.pitch, 0f, 0f) * Quaternion.Euler(0f, sign * k.roll, 0f));
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
        }

        private void Leg(PartId thigh, PartId shin, PartId foot, Vector3 ankle, Vector3 knee, float pitch, Quaternion facing)
        {
            TwoBoneIK.Solve(anim.Part(thigh), anim.Part(shin), anim.Part(foot), Within(thigh, shin, foot, World(ankle), 0.985f), facing * knee);
            anim.Part(foot).rotation = facing * Quaternion.Euler(pitch, 0f, 0f);
        }

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
                if (gap < 0f)
                {
                    Vector3 raised = wrist + Vector3.up * -gap;
                    // (The elbow kept where the key put it: raised, the hand's line may run along the key's elbow way.)
                    KeyArm(upper, fore, hand, slot, raised, elbow, right, facing, keyed);
                }
            }
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
                Vector3 raised = facing * new Vector3(0.75f * side, 0.25f, 0.6f);
                Vector3 flat = facing * new Vector3(0.85f * side, 0f, -0.5f);
                float low = 1f - Mathf.Clamp01((wrist.y - HandHeight) / (0.25f * body.SizeFactor));
                Vector3 dir = Vector3.Slerp(raised.normalized, flat.normalized, low);
                Vector3 up = Vector3.Slerp(facing * Vector3.up, Vector3.Cross(Vector3.up, flat.normalized), low);
                anim.Part(hand).rotation = Quaternion.LookRotation(dir, up) * Quaternion.Inverse(slot.localRotation);
            }
            else
            {
                anim.Part(hand).localRotation = anim.RestLocalRotation(hand);
            }
        }
    }
}
