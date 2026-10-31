using PhysicsBattler.Combat;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// Builds the unit's pose on its animation rig, once per physics step, in a fixed order:
    ///
    ///  1. Stance: the feet, planted or stepping.
    ///  2. Idle: the hips drift and bob, the chest and head weave (CombatIdle).
    ///  3. Upper body: hip turn, lean, chest counter-turn, head held toward the front, arms in guard.
    ///  4. Weight: the hips move so the centre of mass sits over the support point between the feet
    ///     (more over the lead foot as the style asks). Leaning forward pushes the hips back.
    ///  5. Legs: the hips are lowered if a foot can't be reached, then each leg is solved (two-bone
    ///     IK) so the ankle sits over its planted foot, and the foot lies flat, turned with its step.
    ///  6. Arms: a weapon or shield is held in its guard; during a move (<see cref="Move"/>) the
    ///     weapon arm is solved so the weapon's striking part lies on the move's path, the spine
    ///     bending toward it if the arm alone can't reach; the head looks at the aim point.
    ///
    /// During a move, the body layer (3) adds the move's twist, lean, crouch and lunge, and the
    /// weight shift (4) gives way to it.
    ///
    /// The legs always end on the feet, so they always carry the body: nothing holds it up.
    /// </summary>
    public sealed class PosePipeline
    {
        private readonly Transform root;
        private readonly AnimRig anim;
        private readonly BodyMeasure body;
        private readonly Stance stance;
        private readonly CombatIdle idle;

        public CombatStyle Style { get; set; }

        /// <summary>What the unit holds (null: bare hands in guard).</summary>
        public Loadout Loadout { get; set; }

        /// <summary>The blow under way, or null.</summary>
        public MoveState Move { get; set; }

        /// <summary>Where the weapon's striking part is on the animated pose (for the move under way), and how far from the aim.</summary>
        public Vector3 StrikePoint { get; private set; }
        public float AimError { get; private set; }
        public float SpineBend { get; private set; }

        /// <summary>A shield raised across an incoming blow: how far (0 to 1), where its face goes and which way it looks.</summary>
        public float BlockBlend { get; set; }
        public Vector3 BlockCenter { get; set; }
        public Vector3 BlockNormal { get; set; } = Vector3.forward;
        public float MoveCrouch { get; private set; }

        // What the last step produced, for the log.
        public Vector3 CenterOfMass { get; private set; }
        public Vector3 SupportPoint { get; private set; }
        public float PelvisDrop { get; private set; }
        public float WeightShift { get; private set; }
        public IdleSample LastIdle { get; private set; }

        /// <summary>How far the hips rock back when a move's body curve dips below 0 (fraction of leg length, at -1).</summary>
        public const float RockBack = 0.25f;

        /// <summary>How far the hips may move to put the weight over the support point (fraction of leg length).</summary>
        public const float MaxWeightShift = 0.14f;

        public PosePipeline(Transform root, AnimRig anim, BodyMeasure body, Stance stance, CombatIdle idle, CombatStyle style)
        {
            this.root = root;
            this.anim = anim;
            this.body = body;
            this.stance = stance;
            this.idle = idle;
            Style = style;
        }

        /// <summary>
        /// Builds this step's pose. With <paramref name="dry"/> the feet stay where they are (the move
        /// planner tries poses this way and puts everything back).
        /// </summary>
        public void Compose(float time, float dt, bool dry = false)
        {
            var style = Style;
            float leg = body.LegLength;
            float yaw = root.eulerAngles.y;
            var facing = Quaternion.Euler(0f, yaw, 0f);
            float leadSign = stance.LeadLeft ? 1f : -1f;

            // 1. Feet
            stance.GroundY = root.position.y;
            if (!dry)
            {
                stance.Update(root.position, yaw, dt);
            }

            var move = Move;
            float moveBlend = move != null ? move.Blend : 0f;
            float moveBody = move != null ? move.Body : 0f;

            // 2. Idle
            var s = idle.Sample(time, style, leg);
            LastIdle = s;

            anim.ResetToRest();

            // 3. Upper body. Hips over the weighted point between the feet, sunk by the crouch.
            int lead = stance.LeadLeft ? Stance.Left : Stance.Right;
            int rear = 1 - lead;
            Vector3 support = Vector3.Lerp(stance.Position(rear), stance.Position(lead), style.leadWeight);
            support.y = root.position.y;
            SupportPoint = support;

            var pelvis = anim.Part(PartId.Pelvis);
            pelvis.rotation = facing * Quaternion.Euler(s.hips.x, leadSign * style.hipTurn + s.hips.y, s.hips.z);
            Vector3 hips = support + facing * s.hipShift;
            hips.y = root.position.y + body.PelvisHeight - style.crouch * leg + s.bob;
            pelvis.position = hips;

            anim.Part(PartId.Spine).localRotation = Quaternion.Euler(style.lean * 0.5f, 0f, 0f);
            anim.Part(PartId.Chest).localRotation = Quaternion.Euler(style.lean * 0.5f + s.chest.x,
                leadSign * style.chestCounterTurn + s.chest.y, s.chest.z);
            // The head keeps looking ahead: it undoes most of the turn below it.
            float turnBelow = leadSign * (style.hipTurn + style.chestCounterTurn) + s.hips.y + s.chest.y;
            anim.Part(PartId.Head).localRotation = Quaternion.Euler(-style.lean + s.head.x, -0.9f * turnBelow + s.head.y, s.head.z);
            Guard(PartId.UpperArmL, PartId.ForearmL, PartId.HandL, +1f, stance.LeadLeft, style);
            Guard(PartId.UpperArmR, PartId.ForearmR, PartId.HandR, -1f, !stance.LeadLeft, style);

            // 3b. The move's body: turn, lean and crouch toward the blow.
            MoveCrouch = 0f;
            if (move != null)
            {
                float twist = move.Move.TwistAt(move.Progress) * moveBlend;
                pelvis.rotation = Quaternion.AngleAxis(0.4f * twist, Vector3.up) * pelvis.rotation;
                var chest = anim.Part(PartId.Chest);
                chest.rotation = Quaternion.AngleAxis(0.6f * twist, Vector3.up) * chest.rotation;
                float lean = move.Move.lean * moveBody * moveBlend;
                var spine = anim.Part(PartId.Spine);
                spine.localRotation = Quaternion.Euler(0.5f * lean, 0f, 0f) * spine.localRotation;
                chest.localRotation = Quaternion.Euler(0.5f * lean, 0f, 0f) * chest.localRotation;
                // Blows at something low are met by sinking: the further below the chest, the deeper.
                float below = chest.position.y - move.Aim.y - 0.35f * body.SizeFactor;
                MoveCrouch = Mathf.Clamp(0.5f * below, 0f, 0.22f * leg) * Mathf.Clamp01(moveBody) * moveBlend;
                pelvis.position += Vector3.down * MoveCrouch;
            }

            // 4. Weight over the support point (plus the sway, which is meant to move it). A move's
            //    lunge takes over from it.
            Vector3 wanted = support + facing * s.hipShift;
            Vector3 com = CenterOfMassNow();
            Vector3 shift = wanted - com;
            shift.y = 0f;
            shift = Vector3.ClampMagnitude(shift * 0.85f, MaxWeightShift * leg) * (1f - 0.85f * moveBlend);
            WeightShift = shift.magnitude;
            pelvis.position += shift;
            if (move != null)
            {
                // The hips follow the lunge curve from where they stand at home: the feet that have
                // already stepped out moved the support point, and that part is taken back out.
                Vector3 home = Vector3.Lerp(stance.HomeWorld(rear, root.position, yaw), stance.HomeWorld(lead, root.position, yaw), style.leadWeight);
                Vector3 carried = support - home;
                carried.y = 0f;
                // Forward, the hips go the lunge's length; the rock-back before it is a fixed share
                // of the leg, whatever the lunge.
                Vector3 toward = move.Frame * Vector3.forward;
                toward.y = 0f;
                toward = toward.sqrMagnitude > 1e-6f ? toward.normalized : facing * Vector3.forward;
                Vector3 offset = moveBody >= 0f ? move.Lunge * moveBody : toward * (moveBody * RockBack * leg);
                pelvis.position += (offset - carried) * moveBlend;
            }

            // 5. Legs: during a blow, a planted foot that the hips have got ahead of holds them back
            //    (the hips stay over the feet rather than sinking); a foot in the air doesn't pull
            //    them down. Then lower the hips if a foot is still out of reach, and solve each leg.
            if (move != null && moveBlend > 0f)
            {
                for (int f = 0; f < 2; f++)
                {
                    if (stance.Stepping(f))
                    {
                        continue;
                    }

                    var thigh = anim.Part(f == Stance.Left ? PartId.ThighL : PartId.ThighR);
                    Vector3 ankle = AnkleTarget(f);
                    Vector3 hip = thigh.position;
                    float reach = 0.985f * leg;
                    float height = hip.y - ankle.y - 0.03f * leg;    // a little sink is fine
                    float across = Mathf.Sqrt(Mathf.Max(0f, reach * reach - height * height));
                    Vector3 flat = hip - ankle;
                    flat.y = 0f;
                    float over = flat.magnitude - across;
                    if (over > 0f)
                    {
                        pelvis.position -= flat.normalized * over;
                    }
                }
            }

            PelvisDrop = 0f;
            for (int f = 0; f < 2; f++)
            {
                if (move != null && moveBlend > 0f && stance.Stepping(f))
                {
                    continue;   // in the air: the leg reaches as far as it can
                }

                var thigh = anim.Part(f == Stance.Left ? PartId.ThighL : PartId.ThighR);
                Vector3 ankle = AnkleTarget(f);
                Vector3 hip = thigh.position;
                Vector3 flat = ankle - hip;
                flat.y = 0f;
                float reach = 0.985f * leg;
                float vertical = Mathf.Sqrt(Mathf.Max(0f, reach * reach - flat.sqrMagnitude));
                float excess = (hip.y - ankle.y) - vertical;
                PelvisDrop = Mathf.Max(PelvisDrop, excess);
            }

            if (PelvisDrop > 0f)
            {
                pelvis.position += Vector3.down * PelvisDrop;
            }

            for (int f = 0; f < 2; f++)
            {
                SolveLeg(f);
            }

            // 6. Arms and head.
            SpineBend = 0f;
            StrikePoint = Vector3.zero;
            AimError = 0f;
            var loadout = Loadout;
            if (loadout?.Main != null)
            {
                PoseWeaponArm(loadout, move, moveBlend, facing);
            }

            if (loadout != null && loadout.HasShield)
            {
                PoseShieldArm(loadout, facing);
            }

            if (move != null && moveBlend > 0f)
            {
                var head = anim.Part(PartId.Head);
                Vector3 look = move.Aim - head.position;
                if (look.sqrMagnitude > 1e-4f)
                {
                    head.rotation = Quaternion.Slerp(head.rotation, Quaternion.LookRotation(look, Vector3.up), 0.8f * moveBlend);
                }
            }

            CenterOfMass = CenterOfMassNow();
        }

        // ---- Arms -------------------------------------------------------------------------------

        /// <summary>The weapon's pose (grip point and rotation, world) in guard.</summary>
        public void GuardPose(WeaponDefinition weapon, bool rightHand, Quaternion facing, float scale, out Vector3 grip, out Quaternion rotation)
        {
            Vector3 chest = anim.Part(PartId.Chest).position;
            // Held on the hand's own side: guardGrip.x is for the hand it is set up for, mirrored for the other.
            Vector3 g = weapon.guardGrip;
            g.x = rightHand ? Mathf.Abs(g.x) : -Mathf.Abs(g.x);
            g = new Vector3(g.x * body.WidthFactor, g.y * body.SizeFactor, g.z * body.SizeFactor);
            Vector3 point = chest + facing * g;
            // Points (or faces) forward, raised, and turned in across the body.
            float yawIn = rightHand ? -weapon.guardAngles.y : weapon.guardAngles.y;
            Vector3 dir = facing * (Quaternion.Euler(-weapon.guardAngles.x, yawIn, 0f) * Vector3.forward);
            rotation = Quaternion.LookRotation(dir, facing * Vector3.up);
            grip = weapon.IsShield ? point - rotation * (weapon.faceCenter * scale) : point;
        }

        /// <summary>The weapon's pose (grip point and rotation, world) on a move's path at its progress now.</summary>
        public void MovePose(MoveState state, float scale, out Vector3 grip, out Quaternion rotation, out Vector3 strike)
        {
            var move = state.Move;
            float p = state.Progress;
            strike = state.PathPoint(p);
            Vector3 pivot = anim.Part(PartId.Chest).position + Vector3.up * (move.pivotRaise * state.Scale);
            // A thrust lines the blade up with the shoulder (as a fencer's arm and blade), a cut fans
            // out from the chest.
            Vector3 dir = move.pointForward ? state.Aim - anim.Part(PartId.UpperArmR).position : strike - pivot;
            if (dir.sqrMagnitude < 1e-6f)
            {
                dir = state.Frame * Vector3.forward;
            }

            dir.Normalize();
            // The edge leads: the blade's -Y faces the way the striking part is moving.
            Vector3 lead = state.PathPoint(Mathf.Min(1f, p + 0.01f)) - state.PathPoint(Mathf.Max(0f, p - 0.01f));
            lead -= dir * Vector3.Dot(lead, dir);
            Vector3 up = lead.sqrMagnitude > 1e-8f ? -lead.normalized : Vector3.up;
            if (Mathf.Abs(Vector3.Dot(up, dir)) > 0.99f)
            {
                up = Vector3.Cross(dir, state.Frame * Vector3.right);
            }

            rotation = Quaternion.LookRotation(dir, up);
            Vector3 local = state.Weapon != null ? state.Weapon.StrikePoint(move.strikeWith) : Vector3.forward * 0.6f;
            grip = strike - rotation * (local * scale);
        }

        private void PoseWeaponArm(Loadout loadout, MoveState move, float moveBlend, Quaternion facing)
        {
            var upper = anim.Part(PartId.UpperArmR);
            var fore = anim.Part(PartId.ForearmR);
            var hand = anim.Part(PartId.HandR);
            var slot = anim.SlotWeaponR;
            float scale = loadout.Scale;
            Vector3 pole = facing * new Vector3(0.6f, -1f, -0.3f);
            float armLength = anim.RestLocalPosition(PartId.ForearmR).magnitude + anim.RestLocalPosition(PartId.HandR).magnitude;

            GuardPose(loadout.Main, true, facing, scale, out var guardGrip, out var guardRotation);
            Vector3 grip = guardGrip;
            Quaternion rotation = guardRotation;
            if (move != null && moveBlend > 0f)
            {
                // If the arm can't reach, the spine bends toward the hand (a few small turns, up to 50 degrees).
                var spine = anim.Part(PartId.Spine);
                Vector3 moveGrip = grip;
                Quaternion moveRotation = rotation;
                for (int i = 0; i < 6; i++)
                {
                    MovePose(move, scale, out moveGrip, out moveRotation, out _);
                    Vector3 handAt = HandFor(moveGrip, moveRotation, slot);
                    Vector3 shoulder = upper.position;
                    float excess = Vector3.Distance(handAt, shoulder) - 0.97f * armLength;
                    if (excess <= 0f || SpineBend >= 50f)
                    {
                        break;
                    }

                    Vector3 a = shoulder - spine.position;
                    Vector3 c = handAt - spine.position;
                    float angle = Mathf.Min(Vector3.Angle(a, c), Mathf.Min(12f, 50f - SpineBend),
                        Mathf.Rad2Deg * excess / Mathf.Max(0.1f, a.magnitude) + 1f);
                    Vector3 axis = Vector3.Cross(a, c);
                    if (axis.sqrMagnitude < 1e-8f)
                    {
                        break;
                    }

                    spine.rotation = Quaternion.AngleAxis(angle * moveBlend, axis.normalized) * spine.rotation;
                    SpineBend += angle * moveBlend;
                }

                // The guard is posed again after the bend (it hangs off the chest).
                GuardPose(loadout.Main, true, facing, scale, out guardGrip, out guardRotation);
                grip = Vector3.Lerp(guardGrip, moveGrip, moveBlend);
                rotation = Quaternion.Slerp(guardRotation, moveRotation, moveBlend);
            }

            TwoBoneIK.Solve(upper, fore, hand, HandFor(grip, rotation, slot), pole);
            hand.rotation = rotation * Quaternion.Inverse(slot.localRotation);
            if (move != null)
            {
                Vector3 local = move.Weapon != null ? move.Weapon.StrikePoint(move.Move.strikeWith) : Vector3.forward * 0.6f;
                StrikePoint = slot.position + slot.rotation * (local * scale);
                AimError = Vector3.Distance(StrikePoint, move.Aim);
            }
        }

        private void PoseShieldArm(Loadout loadout, Quaternion facing)
        {
            var slot = anim.SlotWeaponL;
            GuardPose(loadout.Off, false, facing, loadout.Scale, out var grip, out var rotation);
            if (BlockBlend > 0f && BlockNormal.sqrMagnitude > 1e-6f)
            {
                // Raised across the blow: the face's middle on the strike line, looking back along it.
                Quaternion block = Quaternion.LookRotation(BlockNormal.normalized, Vector3.up);
                Vector3 blockGrip = BlockCenter - block * (loadout.Off.faceCenter * loadout.Scale);
                float b = BlockBlend * BlockBlend * (3f - 2f * BlockBlend);
                grip = Vector3.Lerp(grip, blockGrip, b);
                rotation = Quaternion.Slerp(rotation, block, b);
            }

            Vector3 pole = facing * new Vector3(-0.8f, -1f, -0.2f);
            TwoBoneIK.Solve(anim.Part(PartId.UpperArmL), anim.Part(PartId.ForearmL), anim.Part(PartId.HandL), HandFor(grip, rotation, slot), pole);
            anim.Part(PartId.HandL).rotation = rotation * Quaternion.Inverse(slot.localRotation);
        }

        /// <summary>Where the hand goes so its weapon slot lands on the grip, turned as the weapon is.</summary>
        private static Vector3 HandFor(Vector3 grip, Quaternion weaponRotation, Transform slot)
        {
            Quaternion handRotation = weaponRotation * Quaternion.Inverse(slot.localRotation);
            return grip - handRotation * slot.localPosition;
        }

        private void Guard(PartId upper, PartId fore, PartId hand, float inSign, bool lead, CombatStyle style)
        {
            float raise = style.guardRaise + (lead ? style.leadArmReach : 0f);
            anim.Part(upper).localRotation = Quaternion.Euler(raise, 0f, inSign * style.guardIn);
            anim.Part(fore).localRotation = Quaternion.Euler(style.guardElbow - (lead ? 0.5f * style.leadArmReach : 0f), 0f, 0f);
            anim.Part(hand).localRotation = Quaternion.Euler(-10f, 0f, 0f);
        }

        public Vector3 AnkleTarget(int foot)
        {
            Vector3 ground = stance.Position(foot);
            return ground + Vector3.up * (body.AnkleHeight + stance.Lift(foot));
        }

        private void SolveLeg(int foot)
        {
            bool left = foot == Stance.Left;
            var thigh = anim.Part(left ? PartId.ThighL : PartId.ThighR);
            var shin = anim.Part(left ? PartId.ShinL : PartId.ShinR);
            var ankle = anim.Part(left ? PartId.FootL : PartId.FootR);
            var footYaw = Quaternion.Euler(0f, stance.Yaw(foot), 0f);
            // Knees point over the toes, a little outward.
            Vector3 pole = footYaw * new Vector3(left ? -0.15f : 0.15f, 0f, 1f);
            TwoBoneIK.Solve(thigh, shin, ankle, AnkleTarget(foot), pole);
            // Flat on the ground, toes down a little as it lifts.
            float t = stance.StepProgress(foot);
            float pitch = stance.Stepping(foot) ? 18f * Mathf.Sin(Mathf.PI * t) * (t < 0.5f ? 1f : -0.6f) : 0f;
            ankle.rotation = footYaw * Quaternion.Euler(pitch, 0f, 0f);
        }

        /// <summary>
        /// Mass-weighted centre of the posed parts. Each limb part's mass sits at the middle of its
        /// bone (between its pivot and its child's); hands, feet and the head at their own pivots.
        /// </summary>
        public Vector3 CenterOfMassNow()
        {
            Vector3 sum = Vector3.zero;
            float mass = 0f;
            foreach (var id in PartInfo.All)
            {
                Vector3 p = anim.Part(id).position;
                var child = BoneEnd(id);
                if (child.HasValue)
                {
                    p = 0.5f * (p + anim.Part(child.Value).position);
                }

                float m = body.PartMass[(int)id];
                sum += p * m;
                mass += m;
            }

            return mass > 0f ? sum / mass : root.position;
        }

        private static PartId? BoneEnd(PartId id)
        {
            switch (id)
            {
                case PartId.Pelvis: return PartId.Spine;
                case PartId.Spine: return PartId.Chest;
                case PartId.Chest: return PartId.Head;
                case PartId.UpperArmL: return PartId.ForearmL;
                case PartId.ForearmL: return PartId.HandL;
                case PartId.UpperArmR: return PartId.ForearmR;
                case PartId.ForearmR: return PartId.HandR;
                case PartId.ThighL: return PartId.ShinL;
                case PartId.ShinL: return PartId.FootL;
                case PartId.ThighR: return PartId.ShinR;
                case PartId.ShinR: return PartId.FootR;
                default: return null;
            }
        }
    }
}
