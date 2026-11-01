using PhysicsBattler.Combat;
using PhysicsBattler.Muscles;
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

        /// <summary>A shield raised across an incoming blow: how far (0 to 1). Where it goes is in <see cref="Shield"/>.</summary>
        public float BlockBlend { get; set; }
        public float MoveCrouch { get; private set; }

        /// <summary>The unit's own body (its colliders keep the shield out of it). Null: no check.</summary>
        public UnitRig Rig { get; set; }

        /// <summary>
        /// Milestone 9: where the head looks, from the unit's controller (<see cref="HeadLook"/>):
        /// degrees right of the unit's facing and up, and how much of it applies (0 off its feet).
        /// The chest takes some of the turn, the head the rest, within a head's limits.
        /// </summary>
        public float LookYaw { get; set; }
        public float LookPitch { get; set; }
        public float LookWeight { get; set; }

        /// <summary>What the look came to on the pose: the head's turn against the chest (degrees, + right) and its pitch against the chest (+ up).</summary>
        public float HeadTurn { get; private set; }
        public float HeadPitch { get; private set; }

        /// <summary>
        /// Milestone 9: the shield under control (<see cref="ShieldAim"/>), from the unit's shield
        /// controller. With no weight, the plain guard of before.
        /// </summary>
        public ShieldAim Shield { get; set; }

        /// <summary>The shield as posed: its face's middle, which way it faces, and how far its rim stays from its own body (metres; negative: inside).</summary>
        public Vector3 ShieldCenter { get; private set; }
        public Vector3 ShieldFace { get; private set; } = Vector3.forward;
        public float ShieldClearance { get; private set; } = float.MaxValue;

        /// <summary>Where the chest was posed before the arms (a weapon arm that can't reach bends the spine after): the swing planner's pivot.</summary>
        public Vector3 ChestBeforeArms { get; private set; }

        /// <summary>
        /// A hit's reaction on the pose (milestone 3), from the unit's recoil springs: the hips pushed
        /// (world metres; a negative y is the knees giving), the upper body turned about the hips and
        /// the head about the neck (world rotation vectors, radians). The physical body follows.
        /// </summary>
        public Vector3 ReactShift { get; set; }
        public Vector3 ReactLean { get; set; }
        public Vector3 ReactHead { get; set; }

        /// <summary>Milestone 9c: a hit's knock on each arm's pose: the hand moved (world, metres) and turned (a rotation vector, radians).</summary>
        public Vector3 KnockR { get; set; }
        public Vector3 KnockTurnR { get; set; }
        public Vector3 KnockL { get; set; }
        public Vector3 KnockTurnL { get; set; }

        private static Quaternion TurnOf(Vector3 turn) => turn.sqrMagnitude > 1e-10f
            ? Quaternion.AngleAxis(turn.magnitude * Mathf.Rad2Deg, turn.normalized) : Quaternion.identity;

        /// <summary>
        /// A stumble (milestone 4): the upper body leaning with the fall (world rotation vector,
        /// radians), the hips carried ahead of the feet (world metres), and the arms thrown out for
        /// balance (0 to 1), against the way it is falling (<see cref="FlailDirection"/>).
        /// </summary>
        public Vector3 Topple { get; set; }
        public Vector3 ToppleShift { get; set; }
        public float Flail { get; set; }
        public Vector3 FlailDirection { get; set; } = Vector3.back;

        /// <summary>
        /// The Fight Rhythm's base loop and posture (unit space, metres; the unit scales it down
        /// while a blow plays): hips, bounce and crouch, lean, roll, turn, head, and both hands moved
        /// and turned from their guards.
        /// </summary>
        public RhythmSample Rhythm { get; set; }

        /// <summary>
        /// Milestone 8: the tempo's held stance (gathering, pressing, shelled up), already scaled to
        /// the body: hips, crouch, lean, head and both hands. Unlike the rhythm, it stays at full
        /// weight through a blow (a pressing unit leans into its blows), but the weapon hand's part
        /// gives way to the blow as the rhythm's does.
        /// </summary>
        public RhythmSample Mood { get; set; }

        /// <summary>The rhythm's weapon feint, when it is the move posed: the rhythm plays on with it (a real blow takes over from the rhythm as it blends in).</summary>
        public MoveState Feint { get; set; }

        /// <summary>How much the unit is walking (0 to 1): the hips go with the root, not the feet.</summary>
        public float GaitWeight { get; set; }

        /// <summary>Milestone 5: a part cut off the unit (it no longer counts in the weight).</summary>
        public System.Func<PartId, bool> IsSevered { get; set; }

        // What the last step produced, for the log.
        public Vector3 CenterOfMass { get; private set; }
        public Vector3 SupportPoint { get; private set; }
        /// <summary>Where the weight is meant to be: the support point moved by the Fight Rhythm's deliberate shifts (a boxer's rock onto the lead leg).</summary>
        public Vector3 WeightTarget { get; private set; }
        public float PelvisDrop { get; private set; }
        public float WeightShift { get; private set; }
        public IdleSample LastIdle { get; private set; }

        /// <summary>How far the hips rock back when a move's body curve dips below 0 (fraction of leg length, at -1).</summary>
        public const float RockBack = 0.25f;

        /// <summary>How far round a side-on stance's hips and chest are while a blow plays (the default stance's).</summary>
        public const float SquaredHipTurn = 22f, SquaredCounterTurn = -10f;

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
            composingDry = dry;
            composeDt = dt;
            if (!dry)
            {
                composeCount++;
            }
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
            if (move != null && move == Feint)
            {
                moveBody = Mathf.Min(0f, moveBody);   // a feint rocks back but never lunges (no lunge step is taken)
            }

            // 2. Idle
            var s = idle.Sample(time, style, leg);
            LastIdle = s;

            anim.ResetToRest();

            // 3. Upper body. Hips over the weighted point between the feet, sunk by the crouch.
            int lead = stance.LeadLeft ? Stance.Left : Stance.Right;
            int rear = 1 - lead;
            Vector3 support = Vector3.Lerp(stance.Position(rear), stance.Position(lead), style.leadWeight);
            // Walking, the weight goes along with the root (the feet step round it), not in jerks
            // with each foot that lands.
            support = Vector3.Lerp(support, root.position, GaitWeight);
            support.y = root.position.y;
            SupportPoint = support;

            // A side-on stance (a fencer's, more than about 22 degrees) squares up into a blow: the
            // hips turn into it, so the swing doesn't go through the body.
            float squared = move != null ? moveBlend : 0f;
            float hipTurn = Mathf.Lerp(style.hipTurn, Mathf.Min(style.hipTurn, SquaredHipTurn), squared);
            float counterTurn = Mathf.Lerp(style.chestCounterTurn, Mathf.Min(style.chestCounterTurn, SquaredCounterTurn), squared * Mathf.Clamp01((style.hipTurn - SquaredHipTurn) / 10f));
            var pelvis = anim.Part(PartId.Pelvis);
            pelvis.rotation = facing * Quaternion.Euler(s.hips.x, leadSign * hipTurn + s.hips.y, s.hips.z);
            Vector3 hips = support + facing * s.hipShift;
            hips.y = root.position.y + body.PelvisHeight - style.crouch * leg + s.bob;
            pelvis.position = hips;

            anim.Part(PartId.Spine).localRotation = Quaternion.Euler(style.lean * 0.5f, 0f, 0f);
            anim.Part(PartId.Chest).localRotation = Quaternion.Euler(style.lean * 0.5f + s.chest.x,
                leadSign * counterTurn + s.chest.y, s.chest.z);
            // The head keeps looking ahead: it undoes most of the turn below it.
            float turnBelow = leadSign * (hipTurn + counterTurn) + s.hips.y + s.chest.y;
            anim.Part(PartId.Head).localRotation = Quaternion.Euler(-style.lean + s.head.x, -0.9f * turnBelow + s.head.y, s.head.z);
            Guard(PartId.UpperArmL, PartId.ForearmL, PartId.HandL, +1f, stance.LeadLeft, style);
            Guard(PartId.UpperArmR, PartId.ForearmR, PartId.HandR, -1f, !stance.LeadLeft, style);

            // 3b. The move's body: turn, lean and crouch toward the blow.
            MoveCrouch = 0f;
            if (move != null)
            {
                var chest = anim.Part(PartId.Chest);
                float lean;
                if (move.Move.shape == MoveShape.Path)
                {
                    float twist = move.Move.TwistAt(move.Progress) * moveBlend;
                    pelvis.rotation = Quaternion.AngleAxis(0.4f * twist, Vector3.up) * pelvis.rotation;
                    chest.rotation = Quaternion.AngleAxis(0.6f * twist, Vector3.up) * chest.rotation;
                    lean = move.Move.lean * moveBody * moveBlend;
                }
                else
                {
                    // A swing: the hips turn ahead of the weapon and the chest between (the chest's
                    // share on top of the hips'); the lean and the crouch follow the phases.
                    pelvis.rotation = Quaternion.AngleAxis(move.HipTurn * moveBlend, Vector3.up) * pelvis.rotation;
                    chest.rotation = Quaternion.AngleAxis(move.ChestTurn * moveBlend, Vector3.up) * chest.rotation;
                    lean = move.Lean * moveBlend;
                    pelvis.position += Vector3.down * (move.Crouch * leg * moveBlend);
                }

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
                offset += toward * move.Drive;   // a thrust's body driving on past contact
                pelvis.position += (offset - carried) * moveBlend;
            }

            // 4b. A hit's recoil: the hips pushed and the upper body thrown back from the blow (the
            //     spine takes some of the turn, the chest the rest).
            if (ReactShift.sqrMagnitude > 1e-8f || ReactLean.sqrMagnitude > 1e-8f)
            {
                pelvis.position += ReactShift;
                float angle = ReactLean.magnitude * Mathf.Rad2Deg;
                if (angle > 0.01f)
                {
                    Vector3 axis = ReactLean.normalized;
                    var spine = anim.Part(PartId.Spine);
                    var chest = anim.Part(PartId.Chest);
                    pelvis.rotation = Quaternion.AngleAxis(0.15f * angle, axis) * pelvis.rotation;
                    spine.rotation = Quaternion.AngleAxis(0.4f * angle, axis) * spine.rotation;
                    chest.rotation = Quaternion.AngleAxis(0.45f * angle, axis) * chest.rotation;
                }
            }

            // 4b'. The Fight Rhythm: the base's bounce and sway, the posture's hips, lean and turn
            //      (after the weight shift: a posture may put the weight over one foot on purpose).
            //      A real blow takes over from it as it blends in.
            rhythm = Weighted(Rhythm, move != null && move != Feint ? 1f - moveBlend : 1f);
            if (move != null && move != Feint)
            {
                // The weapon hand lets go of the rhythm's guard twice as fast: its loop and turns gone
                // before the blow's path takes the weapon round the body.
                float hand = Mathf.Clamp01(1f - 2f * moveBlend);
                hand *= hand;
                var r = Rhythm;
                rhythm.main = r.main * hand;
                rhythm.mainTurn = r.mainTurn * hand;
            }

            {
                // The tempo's stance on top, at full weight but for the weapon hand during a blow.
                var m = Mood;
                if (move != null && move != Feint)
                {
                    float hand = Mathf.Clamp01(1f - 2f * moveBlend);
                    m.main *= hand * hand;
                    m.mainTurn *= hand * hand;
                }

                rhythm += m;
            }
            {
                var r = rhythm;
                pelvis.position += facing * r.hips + Vector3.down * r.drop;
                WeightTarget = SupportPoint + Vector3.ProjectOnPlane(facing * r.hips, Vector3.up);
                var spine = anim.Part(PartId.Spine);
                var chest = anim.Part(PartId.Chest);
                if (Mathf.Abs(r.turn) > 1e-3f || Mathf.Abs(r.hipTurn) > 1e-3f)
                {
                    // (The chest hangs off the pelvis: it ends turned by `turn` whatever the pelvis does.)
                    float hipsTurn = 0.45f * r.turn + r.hipTurn;
                    pelvis.rotation = Quaternion.AngleAxis(hipsTurn, Vector3.up) * pelvis.rotation;
                    chest.rotation = Quaternion.AngleAxis(r.turn - hipsTurn, Vector3.up) * chest.rotation;
                }

                if (Mathf.Abs(r.lean) > 1e-3f || Mathf.Abs(r.roll) > 1e-3f)
                {
                    spine.localRotation = Quaternion.Euler(0.5f * r.lean, 0f, -0.5f * r.roll) * spine.localRotation;
                    chest.localRotation = Quaternion.Euler(0.5f * r.lean, 0f, -0.5f * r.roll) * chest.localRotation;
                }

                if (r.head.sqrMagnitude > 1e-6f)
                {
                    var head = anim.Part(PartId.Head);
                    head.localRotation = Quaternion.Euler(r.head.x, r.head.y, 0f) * head.localRotation;
                }
            }

            // 4c. A stumble: the hips ahead of the feet, the upper body leaning with the fall.
            if (ToppleShift.sqrMagnitude > 1e-8f || Topple.sqrMagnitude > 1e-8f)
            {
                pelvis.position += ToppleShift;
                float angle = Topple.magnitude * Mathf.Rad2Deg;
                if (angle > 0.01f)
                {
                    Vector3 axis = Topple.normalized;
                    var spine = anim.Part(PartId.Spine);
                    var chest = anim.Part(PartId.Chest);
                    pelvis.rotation = Quaternion.AngleAxis(0.2f * angle, axis) * pelvis.rotation;
                    spine.rotation = Quaternion.AngleAxis(0.35f * angle, axis) * spine.rotation;
                    chest.rotation = Quaternion.AngleAxis(0.45f * angle, axis) * chest.rotation;
                }
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

            // (Never more than a quarter of the leg: a foot further out than that is left short of
            // its spot for a moment rather than the body sinking to the ground after it.)
            PelvisDrop = Mathf.Min(PelvisDrop, 0.25f * leg);
            if (PelvisDrop > 0f)
            {
                pelvis.position += Vector3.down * PelvisDrop;
            }

            for (int f = 0; f < 2; f++)
            {
                SolveLeg(f);
            }

            // 5b. The look (milestone 9): the chest takes some of the turn (a blow owns the chest), the
            //     head the rest, within a head's limits.
            ApplyLook(yaw, move != null && move != Feint ? moveBlend : 0f);

            ChestBeforeArms = anim.Part(PartId.Chest).position;

            // 6. Head, then arms (milestone 9c: the arms keep clear of the head where it ends up).
            if (move != null && moveBlend > 0f)
            {
                var head = anim.Part(PartId.Head);
                Vector3 look = move.Aim - head.position;
                if (look.sqrMagnitude > 1e-4f)
                {
                    head.rotation = Quaternion.Slerp(head.rotation, Quaternion.LookRotation(look, Vector3.up), 0.8f * moveBlend);
                }
            }

            if (ReactHead.sqrMagnitude > 1e-8f)
            {
                var head = anim.Part(PartId.Head);
                head.rotation = Quaternion.AngleAxis(ReactHead.magnitude * Mathf.Rad2Deg, ReactHead.normalized) * head.rotation;
            }

            LimitHead(yaw);
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
            else if (loadout == null || loadout.Off == null)
            {
                PoseBareOffHand(facing);
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
            Vector3 dir;
            if (state.Generated)
            {
                // A thrust points along its line; a swing's weapon points out from the pivot through
                // the striking part, trailing the hand as it accelerates and whipping through at contact.
                dir = move.shape == MoveShape.Thrust ? state.ThrustLine
                    : Quaternion.AngleAxis(-state.Lag, state.PlaneNormal) * (strike - state.PivotAt(p));
            }
            else
            {
                Vector3 pivot = anim.Part(PartId.Chest).position + Vector3.up * (move.pivotRaise * state.Scale);
                // A thrust lines the blade up with the shoulder (as a fencer's arm and blade), a cut fans
                // out from the chest.
                dir = move.pointForward ? state.Aim - anim.Part(PartId.UpperArmR).position : strike - pivot;
            }

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

            // (Milestone 9c: a thrust grips the handle more along the palm, the blade leaning further
            // toward the forearm's line, as a thrusting hand turns the pommel into its heel.)
            if (!loadout.Main.IsShield)
            {
                // (Turned as the thrust drives forward, not while the point is drawn back.)
                float drive = move != null && move.Move.shape == MoveShape.Thrust ? Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((move.Progress - 0.25f) / 0.25f)) : 0f;
                float lean = GripLean + ThrustLean * moveBlend * drive;
                slot.localRotation = Quaternion.Euler(lean, 0f, 0f);
                if (!composingDry && Rig != null && Rig.SlotWeaponR != null)
                {
                    Rig.SlotWeaponR.localRotation = slot.localRotation;
                }
            }

            GuardPose(loadout.Main, true, facing, scale, out var guardGrip, out var guardRotation);
            guardGrip += FlailOffset(facing, 1f);
            RhythmGuard(facing, rhythm.main, rhythm.mainTurn, ref guardGrip, ref guardRotation);
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
                guardGrip += FlailOffset(facing, 1f);
                RhythmGuard(facing, rhythm.main, rhythm.mainTurn, ref guardGrip, ref guardRotation);
                grip = Vector3.Lerp(guardGrip, moveGrip, moveBlend);
                rotation = Quaternion.Slerp(guardRotation, moveRotation, moveBlend);
            }

            // (A hit's knock on the arm: the hand back along the blow and turned with it.)
            grip += KnockR;
            rotation = TurnOf(KnockTurnR) * rotation;
            // Milestone 9c: the arm points the weapon. The hand reaches the grip (the elbow toward its
            // pole, swung round to where the wrist is straightest); the wrist keeps within a gripping
            // wrist's easy range, and what it can't take turns the weapon instead. The anchor (the
            // striking part in a blow, the grip in the guard) is then put back where it was asked to
            // be by moving the hand, a few times over: the blow still lands exactly, and the arm, not
            // the wrist, points the weapon.
            float length = (loadout.Main != null ? loadout.Main.length : 0.9f) * scale;
            var torso = Rig != null ? new BodyClearance.Posed(Rig, anim, scale) : null;
            // (The guard's hand keeps a forearm's room from the torso: a guard tucked into the chest is
            // moved out, so the arm can hold the weapon with the wrist straight.)
            float guardShare = move != null ? 1f - moveBlend : 1f;
            if (torso != null && guardShare > 0f)
            {
                Vector3 chestAt = anim.Part(PartId.Chest).position;
                for (int i = 0; i < 3; i++)
                {
                    float room = torso.Blade(grip, grip) - GripRoom * body.SizeFactor;
                    if (room >= 0f)
                    {
                        break;
                    }

                    Vector3 away = grip - chestAt;
                    away.y = 0f;
                    away = away.sqrMagnitude > 1e-6f ? away.normalized : facing * Vector3.forward;
                    grip += away * (-room * guardShare);
                }
            }

            Vector3 anchorLocal = Vector3.zero;
            if (move != null && moveBlend > 0f)
            {
                Vector3 strikeLocal = (move.Weapon != null ? move.Weapon.StrikePoint(move.Move.strikeWith) : Vector3.forward * 0.6f) * scale;
                // (A feint aims at nothing: the hand holds to its path.)
                anchorLocal = move == Feint ? Vector3.zero : strikeLocal * Mathf.Clamp01(2f * moveBlend);
            }

            grip += facing * GuardOutWay * (guardOut * guardShare);
            Vector3 anchor = grip + rotation * anchorLocal;
            var easy = Easy(move != null ? moveBlend : 0f);
            // (In a blow the arm's shape changes as fast as the blow does.)
            weaponArmRate = Mathf.Lerp(GuardArmRate, BlowArmRate, move != null && move != Feint ? moveBlend : 0f);
            armInBlow = move != null && move != Feint && moveBlend > 0.01f;
            // (Back to holding its weapon after a while posed otherwise, getting up say: it takes its
            // shape at once rather than easing from a stale one.)
            if (!composingDry)
            {
                if (weaponArmComposed != composeCount - 1)
                {
                    armWasKnown = false;
                    guardOut = guardOutSpeed = 0f;
                    weaponArmRate = 1e6f;
                }

                weaponArmComposed = composeCount;
            }
            float swingRoom = Mathf.Lerp(ElbowSwing, BlowElbowSwing, move != null ? moveBlend : 0f);
            float clearTurnStart = mainClearTurn;
            void SolveArm()
            {
                TwoBoneIK.Solve(upper, fore, hand, HandFor(grip, rotation, slot), pole);
                WeaponElbow(rotation, slot, easy, swingRoom, torso, length);
                Vector3 along = (hand.position - upper.position).normalized;
                upper.rotation = Quaternion.AngleAxis(weaponSwing, along) * upper.rotation;
                AlignHinge(PartId.UpperArmR, PartId.ForearmR);
                // The forearm, hand and weapon now one rigid piece from the elbow: the elbow bends and the
                // arm turns so the anchor lands where it was asked to (the weapon points where that puts it).
                // Where the arm can't reach it so, the wrist gives a little more (never past its limits).
                Quaternion upperWas = upper.rotation, foreWas = fore.rotation;
                Quaternion bestUpper = upperWas, bestFore = foreWas, bestHand = hand.rotation;
                float bestMiss = float.MaxValue, clearTurnWas = mainClearTurn, bestClearTurn = mainClearTurn;
                for (int tryWrist = 0; tryWrist < 4; tryWrist++)
                {
                    upper.rotation = upperWas;
                    fore.rotation = foreWas;
                    mainClearTurn = clearTurnWas;
                    hand.rotation = rotation * Quaternion.Inverse(slot.localRotation);
                    EasyWrist(new Vector2(easy.x * (1f + tryWrist), easy.y));
                    ClampJoint(PartId.HandR, JointLimitTable.For(PartId.HandR));
                    RigidAim(slot, anchorLocal, anchor, pole, torso, length);
                    // (How far off, and how far into its own body: either makes the wrist give more; landing
                    // where it is asked to comes first.)
                    float off = (slot.position + slot.rotation * anchorLocal - anchor).magnitude;
                    float miss = off > 0.004f ? 10f * off : 0f;
                    if (torso != null)
                    {
                        float clear = Mathf.Min(torso.Arm(fore.position, hand.position, slot.position),
                            torso.Blade(slot.position, slot.position + slot.rotation * (Vector3.forward * length)));
                        miss += Mathf.Max(0f, ArmBodyMargin - clear);
                    }

                    if (miss < bestMiss - 1e-3f)
                    {
                        bestMiss = miss;
                        bestUpper = upper.rotation;
                        bestFore = fore.rotation;
                        bestHand = hand.rotation;
                        bestClearTurn = mainClearTurn;
                    }

                    if (miss <= 0f)
                    {
                        break;
                    }
                }

                upper.rotation = bestUpper;
                fore.rotation = bestFore;
                hand.rotation = bestHand;
                mainClearTurn = bestClearTurn;
            }

            SolveArm();

            // (In the guard a weapon that would lie against its own head or body is held further out in
            // front and lower, eased out and back in as needed.)
            if (torso != null && !composingDry && composeDt > 0f)
            {
                float clear = torso.Blade(slot.position, slot.position + slot.rotation * (Vector3.forward * length));
                float needed = Mathf.Clamp(guardOut + (ArmBodyMargin - clear), 0f, GuardOutMost * body.SizeFactor);
                // (On a critically damped spring: it starts and stops smoothly.)
                float target = guardShare > 0.5f ? needed : 0f;
                float w = 2f * Mathf.PI * GuardOutRate, e = guardOut - target, c = guardOutSpeed + w * e, decay = Mathf.Exp(-w * composeDt);
                guardOutSpeed = (guardOutSpeed - c * w * composeDt) * decay;
                guardOut = Mathf.Max(0f, target + (e + c * composeDt) * decay);
            }

            // (Out of a blow, the arm eases between poses rather than jumping: a guard that changes
            // quickly, or the wrist coming off its easy range, moves it smoothly.)
            SmoothWeaponArm(move != null && move != Feint ? 1f : 0f);
            WristBend = Vector3.Angle(fore.rotation * anim.RestLocalPosition(PartId.HandR), hand.rotation * anim.RestLocalPosition(PartId.HandR));
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
            var shield = loadout.Off;
            float scale = loadout.Scale;
            GuardPose(shield, false, facing, scale, out var grip, out var rotation);
            var aim = Shield;
            float w = Mathf.Clamp01(aim.weight);
            if (w > 0f)
            {
                // Milestone 9: under control. The guard (and the rhythm's on it) turned round the
                // chest toward the threat, inside the cone a shield arm can cover, its face set
                // against a blow it has seen.
                ControlledShield(shield, scale, aim, out var held, out var heldRotation);
                grip = Vector3.Lerp(grip, held, w);
                rotation = Quaternion.Slerp(rotation, heldRotation, w);
                grip += FlailOffset(facing, -1f);
            }
            else
            {
                grip += FlailOffset(facing, -1f);
                RhythmGuard(facing, rhythm.off, rhythm.offTurn, ref grip, ref rotation);
            }

            // Milestone 9c: the shield turns at most so fast (the torso turning under it in a swing
            // included); it turns about its grip.
            if (w > 0f && shieldTurnKnown && composeDt > 0f)
            {
                float most = (BlockBlend > 0f ? ShieldBlockTurnRate : ShieldTurnRate) * composeDt;
                rotation = Quaternion.RotateTowards(lastShieldTurn, rotation, most);
            }

            if (!composingDry)
            {
                lastShieldTurn = rotation;
                shieldTurnKnown = w > 0f;
            }

            // (A hit's knock on the shield: back along the blow and turned with it.)
            grip += KnockL;
            rotation = TurnOf(KnockTurnL) * rotation;

            // Held in the hand: as far out as the arm reaches, the elbow a little bent.
            Vector3 handAt = HandFor(grip, rotation, slot);
            grip += Reachable(PartId.UpperArmL, PartId.ForearmL, PartId.HandL, handAt, ShieldReach) - handAt;
            KeepShieldClear(shield, scale, slot, w > 0f, ref grip, ref rotation);
            ShieldCenter = grip + rotation * (shield.faceCenter * scale);
            ShieldFace = rotation * Vector3.forward;

            // (The elbow down by the ribs, a little out: the forearm then points at the boss.)
            Vector3 pole = facing * new Vector3(-0.35f, -1f, -0.35f);
            TwoBoneIK.Solve(anim.Part(PartId.UpperArmL), anim.Part(PartId.ForearmL), anim.Part(PartId.HandL),
                Reachable(PartId.UpperArmL, PartId.ForearmL, PartId.HandL, HandFor(grip, rotation, slot)), pole);
            anim.Part(PartId.HandL).rotation = rotation * Quaternion.Inverse(slot.localRotation);
            NaturalArm(PartId.UpperArmL, PartId.ForearmL, PartId.HandL, anim.Part(PartId.HandL).rotation, ref offElbow, 1f);

            // Its own arm: where the rim would cut the shield arm, the shield goes out along its face.
            if (w > 0f && Rig != null)
            {
                float radius = shield.radius * scale;
                for (int i = 0; i < 2; i++)
                {
                    Vector3 center = grip + rotation * (shield.faceCenter * scale);
                    float gap = ShieldGap(center, rotation, radius, ShieldArm);
                    if (gap >= 0.01f * body.SizeFactor)
                    {
                        break;
                    }

                    grip += (rotation * Vector3.forward) * (0.01f * body.SizeFactor - gap);
                    TwoBoneIK.Solve(anim.Part(PartId.UpperArmL), anim.Part(PartId.ForearmL), anim.Part(PartId.HandL),
                        Reachable(PartId.UpperArmL, PartId.ForearmL, PartId.HandL, HandFor(grip, rotation, slot)), pole);
                    anim.Part(PartId.HandL).rotation = rotation * Quaternion.Inverse(slot.localRotation);
                    NaturalArm(PartId.UpperArmL, PartId.ForearmL, PartId.HandL, anim.Part(PartId.HandL).rotation, ref offElbow, 1f);
                }

                ShieldCenter = grip + rotation * (shield.faceCenter * scale);
                ShieldClearance = Mathf.Min(ShieldClearance, ShieldGap(ShieldCenter, rotation, radius, ShieldArm) + ArmShieldAllowance * body.SizeFactor);
            }
        }

        private float lastShieldYaw;
        private bool shieldYawKnown;
        private Quaternion lastShieldTurn;
        private bool shieldTurnKnown;

        /// <summary>The fastest the held shield turns in the world (degrees per second), and in a block; it never flips.</summary>
        public const float ShieldTurnRate = 250f, ShieldBlockTurnRate = 720f;
        private static readonly PartId[] ShieldArm = { PartId.UpperArmL, PartId.ForearmL };

        /// <summary>How close the shield arm's own forearm may come to the back of the shield it holds (metres at medium size).</summary>
        public const float ArmShieldAllowance = 0f;

        // ---- The arm points the weapon (milestone 9c) ---------------------------------------------

        /// <summary>The weapon wrist's bend as posed this step (degrees, the roll left out).</summary>
        public float WristBend { get; private set; }

        /// <summary>How far the guard's hand keeps from the torso (metres at medium size): room for the forearm.</summary>
        public const float GripRoom = 0.12f;


        /// <summary>
        /// The weapon wrist's easy range, measured about the forearm's own line (the rig's rest frame
        /// is turned off it by the arm's tilt): how far the hand may bend off that line, and roll about
        /// it (degrees); in the guard, and through a blow.
        /// </summary>
        public const float EasyBend = 22f, EasyBendBlow = 35f, EasyRoll = 75f;

        private static Vector2 Easy(float blow) => new Vector2(Mathf.Lerp(EasyBend, EasyBendBlow, blow), EasyRoll);

        /// <summary>Holds the weapon wrist inside its easy range: its bend off the forearm's line and its roll about it.</summary>
        private void EasyWrist(Vector2 easy)
        {
            var fore = anim.Part(PartId.ForearmR);
            var hand = anim.Part(PartId.HandR);
            Vector3 line = anim.RestLocalPosition(PartId.HandR).normalized;
            Quaternion rel = Quaternion.Inverse(fore.rotation) * hand.rotation;
            if (rel.w < 0f)
            {
                rel = new Quaternion(-rel.x, -rel.y, -rel.z, -rel.w);
            }

            float along = rel.x * line.x + rel.y * line.y + rel.z * line.z;
            var twist = new Quaternion(line.x * along, line.y * along, line.z * along, rel.w);
            float m = Mathf.Sqrt(twist.x * twist.x + twist.y * twist.y + twist.z * twist.z + twist.w * twist.w);
            twist = m > 1e-6f ? new Quaternion(twist.x / m, twist.y / m, twist.z / m, twist.w / m) : Quaternion.identity;
            Quaternion swing = rel * Quaternion.Inverse(twist);
            twist.ToAngleAxis(out float roll, out Vector3 rollAxis);
            roll = Mathf.DeltaAngle(0f, roll);
            if (Vector3.Dot(rollAxis, line) < 0f)
            {
                roll = -roll;
            }

            swing.ToAngleAxis(out float bend, out Vector3 bendAxis);
            bend = Mathf.DeltaAngle(0f, bend);
            bool changed = false;
            if (Mathf.Abs(roll) > easy.y)
            {
                twist = Quaternion.AngleAxis(Mathf.Clamp(roll, -easy.y, easy.y), line);
                changed = true;
            }

            if (Mathf.Abs(bend) > easy.x && !float.IsNaN(bendAxis.x))
            {
                swing = Quaternion.AngleAxis(Mathf.Clamp(bend, -easy.x, easy.x), bendAxis);
                changed = true;
            }

            if (changed)
            {
                hand.rotation = fore.rotation * (swing * twist);
            }
        }

        /// <summary>How far the weapon arm's elbow may swing round from its pole in a blow (degrees; the guard's is <see cref="ElbowSwing"/>).</summary>
        public const float BlowElbowSwing = 60f;

        /// <summary>How far a weapon's blade leans from square across the fist toward the forearm's line (degrees; the diagonal grip).</summary>
        public const float GripLean = 20f;

        /// <summary>How far the weapon arm and its weapon keep from its own torso and head when it can choose (metres; the measure counts the chest's allowance).</summary>
        public const float ArmBodyMargin = 0.045f;

        /// <summary>How fast the weapon arm's elbow swings and turns clear of the body in a blow (degrees per second; out of one, <see cref="ElbowRate"/>).</summary>
        public const float BlowArmRate = 6000f, GuardArmRate = 900f;

        /// <summary>How far a guard is held out at most to keep its weapon off its own head and body (metres at medium size), and how quickly it goes out and back (Hz, critically damped).</summary>
        public const float GuardOutMost = 0.5f, GuardOutRate = 1.5f;
        private float guardOut, guardOutSpeed;

        /// <summary>Which way a guard is moved to keep its weapon off its own head (the facing's frame): forward and down.</summary>
        public static readonly Vector3 GuardOutWay = new Vector3(0f, -0.6f, 0.8f);
        private float weaponArmRate = ElbowRate;
        private int composeCount, weaponArmComposed = -10;
        private float mainElbowSpeed, mainClearTurnSpeed;
        private bool armInBlow;

        /// <summary>How quickly the weapon arm's shape eases to a better one out of a blow (Hz; critically damped, so it starts and stops smoothly).</summary>
        public const float ArmShapeEase = 5f;

        /// <summary>
        /// Toward a better shape: out of a blow on a critically damped spring at <see cref="ArmShapeEase"/>
        /// (never faster than <see cref="weaponArmRate"/>); in a blow (or after a gap) at once.
        /// </summary>
        private float EaseAngle(float x, ref float v, float target, float dt)
        {
            if (armInBlow || weaponArmRate >= BlowArmRate - 1f)
            {
                v = 0f;
                return target;
            }

            float w = 2f * Mathf.PI * ArmShapeEase, e = x - target, c = v + w * e, decay = Mathf.Exp(-w * dt);
            v = Mathf.Clamp((v - c * w * dt) * decay, -weaponArmRate, weaponArmRate);
            return target + (e + c * dt) * decay;
        }

        /// <summary>How much further a thrust's grip leans the blade toward the forearm's line (degrees).</summary>
        public const float ThrustLean = 35f;

        /// <summary>
        /// The weapon arm's elbow: swung round the shoulder-to-hand line to where the weapon, held as
        /// asked, needs the least of the wrist and keeps the arm and weapon out of its own body (a
        /// little against swinging far, and from where it was), eased there at <see cref="ElbowRate"/>.
        /// </summary>
        private void WeaponElbow(Quaternion weapon, Transform slot, Vector2 easy, float room, BodyClearance.Posed torso, float length)
        {
            var upper = anim.Part(PartId.UpperArmR);
            var hand = anim.Part(PartId.HandR);
            var fore = anim.Part(PartId.ForearmR);
            Vector3 axis = hand.position - upper.position;
            if (axis.sqrMagnitude < 1e-6f)
            {
                return;
            }

            axis.Normalize();
            Quaternion start = upper.rotation;
            Quaternion handRotation = weapon * Quaternion.Inverse(slot.localRotation);
            Vector3 line = anim.RestLocalPosition(PartId.HandR).normalized;
            float best = mainElbow, bestCost = float.MaxValue;
            for (float a = -room; a <= room + 1e-3f; a += ElbowStep)
            {
                upper.rotation = Quaternion.AngleAxis(a, axis) * start;
                AlignHinge(PartId.UpperArmR, PartId.ForearmR);
                hand.rotation = handRotation;
                // (How far the hand, held as asked, bends off the forearm's line.)
                float bend = Vector3.Angle(fore.rotation * line, hand.rotation * line);
                float cost = Mathf.Max(0f, bend - easy.x) + 0.04f * Mathf.Abs(a) + 0.02f * Mathf.Abs(a - mainElbow);
                if (torso != null)
                {
                    // (The weapon as the easy wrist would hold it, and the forearm, kept out of its own body.)
                    EasyWrist(easy);
                    float clear = Mathf.Min(torso.Blade(slot.position, slot.position + slot.rotation * (Vector3.forward * length)),
                        torso.Arm(fore.position, hand.position, slot.position));
                    cost += 1000f * Mathf.Max(0f, ArmBodyMargin - clear);
                }

                if (cost < bestCost)
                {
                    bestCost = cost;
                    best = a;
                }
            }

            upper.rotation = start;
            if (!composingDry)
            {
                mainElbow = composeDt > 0f ? EaseAngle(mainElbow, ref mainElbowSpeed, best, composeDt) : mainElbow;
                weaponSwing = mainElbow;
            }
            else
            {
                // (A tried pose takes the best at once: the blow's own pose, not the guard's it came from.)
                weaponSwing = best;
            }
        }

        /// <summary>The weapon arm's elbow swing for this pose (eased while playing, the best at once in a tried pose).</summary>
        private float weaponSwing;

        /// <summary>Holds a joint inside the given limits (rather than the table's).</summary>
        private void ClampJoint(PartId id, JointLimitSpec spec)
        {
            var local = LocalOf(id);
            float past = JointMath.PastLimits(local, spec, out float pitch, out float y, out float z);
            if (past <= 0f)
            {
                return;
            }

            pitch = Mathf.Clamp(pitch, spec.pitchMin, spec.pitchMax);
            y = Mathf.Clamp(y, -spec.twist, spec.twist);
            z = Mathf.Clamp(z, -spec.swing, spec.swing);
            float sy = Mathf.Sin(0.5f * y * Mathf.Deg2Rad), sz = Mathf.Sin(0.5f * z * Mathf.Deg2Rad);
            var swing = new Quaternion(0f, sy, sz, Mathf.Sqrt(Mathf.Max(0f, 1f - sy * sy - sz * sz)));
            var parent = anim.Part(PartInfo.Parent(id));
            anim.Part(id).rotation = parent.rotation * anim.RestLocalRotation(id) * (swing * Quaternion.AngleAxis(pitch, Vector3.right));
        }

        /// <summary>
        /// Puts a point held rigidly by the forearm (through the hand and weapon: the anchor) where it
        /// is asked to be: the elbow bends about its hinge until the anchor is as far from the shoulder
        /// as its target, the arm turns about the shoulder to bring it there, and turns about the
        /// shoulder-to-target line so the elbow keeps its place round the pole. Out of reach, it reaches
        /// as far as it can.
        /// </summary>
        private void RigidAim(Transform slot, Vector3 anchorLocal, Vector3 target, Vector3 pole, BodyClearance.Posed torso, float length)
        {
            var upper = anim.Part(PartId.UpperArmR);
            var fore = anim.Part(PartId.ForearmR);
            Vector3 shoulder = upper.position;
            Vector3 held = Quaternion.Inverse(fore.rotation) * (slot.position + slot.rotation * anchorLocal - fore.position);
            float want = (target - shoulder).magnitude;
            Quaternion bent = fore.localRotation;
            float pitch;
            JointMath.PastLimits(LocalOf(PartId.ForearmR), JointLimitTable.For(PartId.ForearmR), out pitch, out _, out _);
            var elbow = JointLimitTable.For(PartId.ForearmR);
            float Reach(float extra)
            {
                fore.localRotation = bent * Quaternion.AngleAxis(extra, Vector3.right);
                return (fore.position + fore.rotation * held - shoulder).magnitude;
            }

            // (Bending the elbow brings the anchor nearer the shoulder: the bend that matches, between
            // straight and fully bent.)
            float lo = elbow.pitchMin - pitch, hi = elbow.pitchMax - pitch;
            float atLo = Reach(lo), atHi = Reach(hi);
            float extraBend;
            if ((atLo - want) * (atHi - want) > 0f)
            {
                extraBend = Mathf.Abs(atLo - want) < Mathf.Abs(atHi - want) ? lo : hi;
            }
            else
            {
                bool loNear = atLo < atHi;
                for (int i = 0; i < 18; i++)
                {
                    float mid = 0.5f * (lo + hi);
                    if ((Reach(mid) < want) == loNear)
                    {
                        lo = mid;
                    }
                    else
                    {
                        hi = mid;
                    }
                }

                extraBend = 0.5f * (lo + hi);
            }

            fore.localRotation = bent * Quaternion.AngleAxis(extraBend, Vector3.right);

            // Turn the arm about the shoulder onto the target, then round that line to its elbow's place.
            Vector3 now = fore.position + fore.rotation * held - shoulder;
            Vector3 to = target - shoulder;
            if (now.sqrMagnitude > 1e-8f && to.sqrMagnitude > 1e-8f)
            {
                upper.rotation = Quaternion.FromToRotation(now, to) * upper.rotation;
                Vector3 axis = to.normalized;
                Vector3 middle = Vector3.ProjectOnPlane(fore.position - shoulder, axis);
                Vector3 wanted = Quaternion.AngleAxis(weaponSwing, axis) * Vector3.ProjectOnPlane(pole, axis);
                if (middle.sqrMagnitude > 1e-8f && wanted.sqrMagnitude > 1e-8f)
                {
                    upper.rotation = Quaternion.AngleAxis(Vector3.SignedAngle(middle, wanted, axis), axis) * upper.rotation;
                }

                // Round that line (the anchor stays put), the arm and weapon keep out of its own body:
                // the turn nearest its elbow's place that is clear, eased to from last step's.
                if (torso != null)
                {
                    Quaternion placed = upper.rotation;
                    float best = 0f, bestCost = float.MaxValue;
                    for (float a = -ClearTurn; a <= ClearTurn + 1e-3f; a += ElbowStep)
                    {
                        upper.rotation = Quaternion.AngleAxis(a, axis) * placed;
                        float clear = Mathf.Min(torso.Arm(fore.position, anim.Part(PartId.HandR).position, slot.position),
                            torso.Blade(slot.position, slot.position + slot.rotation * (Vector3.forward * length)));
                        float cost = 1000f * Mathf.Max(0f, ArmBodyMargin - clear) + 0.04f * Mathf.Abs(a) + 0.02f * Mathf.Abs(a - mainClearTurn);
                        if (cost < bestCost)
                        {
                            bestCost = cost;
                            best = a;
                        }
                    }

                    if (!composingDry)
                    {
                        best = composeDt > 0f ? EaseAngle(mainClearTurn, ref mainClearTurnSpeed, best, composeDt) : mainClearTurn;
                        mainClearTurn = best;
                    }

                    // (A tried pose takes the best turn at once.)

                    upper.rotation = Quaternion.AngleAxis(best, axis) * placed;
                }
            }
        }

        /// <summary>How far the weapon arm turns round the shoulder-to-anchor line to keep out of its own body (degrees), and the turn last step.</summary>
        public const float ClearTurn = 90f;
        private float mainClearTurn;

        private readonly Quaternion[] armWas = new Quaternion[3];
        private bool armWasKnown;

        /// <summary>How quickly the weapon arm eases to its new pose out of a blow (Hz).</summary>
        public const float ArmEase = 12f;

        private static readonly PartId[] WeaponArmParts = { PartId.UpperArmR, PartId.ForearmR, PartId.HandR };

        private void SmoothWeaponArm(float blow)
        {
            if (composingDry)
            {
                return;
            }

            float k = composeDt > 0f && armWasKnown ? Mathf.Lerp(1f - Mathf.Exp(-2f * Mathf.PI * ArmEase * composeDt), 1f, blow) : 1f;
            for (int i = 0; i < 3; i++)
            {
                var part = anim.Part(WeaponArmParts[i]);
                if (k < 1f)
                {
                    part.localRotation = Quaternion.Slerp(armWas[i], part.localRotation, k);
                }

                armWas[i] = part.localRotation;
            }

            armWasKnown = true;
        }

        // ---- Natural arms (milestone 9b) --------------------------------------------------------

        /// <summary>Each arm's elbow turn about the shoulder-to-hand line last step (degrees; kept so the elbow doesn't flip between steps).</summary>
        private float mainElbow, offElbow;
        private bool composingDry;
        private float composeDt;

        /// <summary>How fast the elbow swings round to a better place (degrees per second): it eases there, never jumps.</summary>
        public const float ElbowRate = 300f;

        /// <summary>How far the elbow may swing round from where the arm's pole puts it (degrees), and in what steps.</summary>
        public const float ElbowSwing = 90f, ElbowStep = 15f;

        /// <summary>
        /// A natural arm for a hand that must hold its weapon just so (the hand's rotation is given):
        /// the elbow swings round the shoulder-to-hand line to where the wrist is straightest (a
        /// little against moving far from the pole, and from where it was), the forearm rolls to take
        /// the hand's twist, and what the wrist still can't take is held at its limits (the weapon
        /// turns a little less than asked: the planner's tries see the real pose).
        /// </summary>
        private void NaturalArm(PartId upperId, PartId foreId, PartId handId, Quaternion handRotation, ref float lastSwing, float clamp)
        {
            var upper = anim.Part(upperId);
            var hand = anim.Part(handId);
            Vector3 axis = hand.position - upper.position;
            if (axis.sqrMagnitude < 1e-6f)
            {
                return;
            }

            axis.Normalize();
            Quaternion start = upper.rotation;
            var reach = JointLimitTable.For(handId);
            float best = 0f, bestCost = float.MaxValue;
            // (In a blow the weapon arm's elbow stays where the blow puts it.)
            float room = clamp >= 1f ? ElbowSwing : 0f;
            for (float a = -room; a <= room + 1e-3f; a += ElbowStep)
            {
                upper.rotation = Quaternion.AngleAxis(a, axis) * start;
                hand.rotation = handRotation;
                float cost = JointMath.PastLimits(LocalOf(handId), reach, out _, out _, out _)
                             + 0.04f * Mathf.Abs(a) + 0.02f * Mathf.Abs(a - lastSwing);
                if (cost < bestCost)
                {
                    bestCost = cost;
                    best = a;
                }
            }

            if (!composingDry)
            {
                best = composeDt > 0f ? Mathf.MoveTowards(lastSwing, best, ElbowRate * composeDt) : lastSwing;
                lastSwing = best;
            }

            upper.rotation = Quaternion.AngleAxis(best, axis) * start;

            AlignHinge(upperId, foreId);

            // (The forearm's roll shows at the wrist: the hand turns about the forearm's line, as a fist
            // does when the forearm turns over. PhysX can't keep a roll at a bent elbow.)
            hand.rotation = handRotation;

            if (clamp > 0f)
            {
                ClampJoint(handId, clamp);
            }
        }

        /// <summary>
        /// The two-bone solve bends the middle joint about whatever axis the chain happens to lie in;
        /// an elbow or knee is a hinge. The upper bone rolls about its own length (the middle joint
        /// stays where it is, and the lower bone and everything below keep their turn in the world)
        /// so its hinge axis is square to the bend, bending the way the joint does.
        /// </summary>
        private void AlignHinge(PartId upperId, PartId lowerId)
        {
            var upper = anim.Part(upperId);
            var lower = anim.Part(lowerId);
            Vector3 along = lower.position - upper.position;
            Vector3 below = lower.rotation * (anim.RestLocalPosition(LowerEnd(lowerId)).normalized);
            Vector3 bend = Vector3.Cross(along, below);
            if (along.sqrMagnitude < 1e-6f || bend.sqrMagnitude < 1e-6f)
            {
                return;   // (straight: no plane to square to)
            }

            along.Normalize();
            bend.Normalize();
            Vector3 hinge = Vector3.ProjectOnPlane(upper.rotation * Vector3.right, along);
            if (hinge.sqrMagnitude < 1e-6f)
            {
                return;
            }

            // Either way along the bend's normal; the one that leaves the joint bending as it should.
            var lowerTurn = lower.rotation;
            var restPitch = JointLimitTable.For(lowerId);
            float bendsNegative = restPitch.pitchMin + restPitch.pitchMax < 0f ? 1f : -1f;   // (an elbow bends to negative pitch, a knee to positive)
            Quaternion start = upper.rotation;
            float bestPast = float.MaxValue;
            Quaternion best = start;
            foreach (float sign in new[] { 1f, -1f })
            {
                float roll = Vector3.SignedAngle(hinge, sign * bend, along);
                upper.rotation = Quaternion.AngleAxis(roll, along) * start;
                lower.rotation = lowerTurn;
                float past = JointMath.PastLimits(LocalOf(lowerId), restPitch, out float pitch, out _, out _);
                past += pitch * bendsNegative > 0f ? 90f : 0f;   // (the wrong way round)
                if (past < bestPast)
                {
                    bestPast = past;
                    best = upper.rotation;
                }
            }

            upper.rotation = best;
            lower.rotation = lowerTurn;
        }

        private static PartId LowerEnd(PartId lower)
        {
            switch (lower)
            {
                case PartId.ForearmL: return PartId.HandL;
                case PartId.ForearmR: return PartId.HandR;
                case PartId.ShinL: return PartId.FootL;
                default: return PartId.FootR;
            }
        }

        /// <summary>A part's rotation against the one it hangs from, its rest taken out.</summary>
        private Quaternion LocalOf(PartId id)
        {
            var part = anim.Part(id);
            var parent = anim.Part(PartInfo.Parent(id));
            return Quaternion.Inverse(anim.RestLocalRotation(id)) * (Quaternion.Inverse(parent.rotation) * part.rotation);
        }

        /// <summary>How far a rotation turns about an axis (degrees): its twist, the swing taken out.</summary>
        private static float TwistAbout(Quaternion q, Vector3 axis)
        {
            float along = q.x * axis.x + q.y * axis.y + q.z * axis.z;
            float angle = 2f * Mathf.Atan2(along, q.w) * Mathf.Rad2Deg;
            return Mathf.DeltaAngle(0f, angle);
        }

        /// <summary>Holds a joint of the pose inside its limits (the nearest turn that is), as PhysX splits a joint's turn.</summary>
        private void ClampJoint(PartId id, float weight = 1f)
        {
            var spec = JointLimitTable.For(id);
            var local = LocalOf(id);
            float past = JointMath.PastLimits(local, spec, out float pitch, out float y, out float z);
            if (past <= 0f)
            {
                return;
            }

            pitch = Mathf.Clamp(pitch, spec.pitchMin, spec.pitchMax);
            y = Mathf.Clamp(y, -spec.twist, spec.twist);
            z = Mathf.Clamp(z, -spec.swing, spec.swing);
            float sy = Mathf.Sin(0.5f * y * Mathf.Deg2Rad), sz = Mathf.Sin(0.5f * z * Mathf.Deg2Rad);
            var swing = new Quaternion(0f, sy, sz, Mathf.Sqrt(Mathf.Max(0f, 1f - sy * sy - sz * sz)));
            var held = swing * Quaternion.AngleAxis(pitch, Vector3.right);
            var parent = anim.Part(PartInfo.Parent(id));
            var part = anim.Part(id);
            part.rotation = Quaternion.Slerp(part.rotation, parent.rotation * anim.RestLocalRotation(id) * held, Mathf.Clamp01(weight));
        }


        /// <summary>How far out the shield arm reaches at most (share of the arm): the elbow stays a little bent.</summary>
        public const float ShieldReach = 0.92f;

        /// <summary>
        /// Milestone 9c: the shield held close, built from the arm. The upper arm hangs from the
        /// shoulder a little forward and out (the elbow by the ribs), in the chest's frame; the forearm
        /// points at the threat, turned this far in across the body (degrees), and the shield faces
        /// where the forearm points (its centre grip). Raised (high), the elbow comes up and forward; in
        /// a block it opens toward the blow, by at most <see cref="ShieldBlockOpen"/> of the way.
        /// </summary>
        public static readonly Vector3 ShieldUpperArm = new Vector3(-0.1f, -1f, -0.15f), ShieldUpperArmHigh = new Vector3(-0.3f, -0.6f, 0.5f);
        public const float ShieldForearmIn = 15f, ShieldBlockOpen = 0.45f;

        /// <summary>How much of the Fight Rhythm's sway the shield arm takes (it holds its guard steadily).</summary>
        public const float ShieldSway = 0.3f;

        /// <summary>The most a shield's face may turn from its way out from the chest (degrees): any more and it would face its wielder.</summary>
        public const float MaxShieldFaceOff = 60f;

        /// <summary>
        /// The shield under control: the guard as tuned (in the chest's frame), turned round the chest
        /// toward the threat (clamped into the cone), its face turned with it, or set against a blow.
        /// </summary>
        private void ControlledShield(WeaponDefinition shield, float scale, ShieldAim aim, out Vector3 grip, out Quaternion rotation)
        {
            var chestPart = anim.Part(PartId.Chest);
            Vector3 chest = chestPart.position;
            float chestYaw = ShieldCone.FacingOf(chestPart.rotation, root.eulerAngles.y);
            var frame = Quaternion.Euler(0f, chestYaw, 0f);

            // Toward the threat, inside the cone a shield arm can cover.
            Vector3 direction = aim.direction.sqrMagnitude > 1e-6f ? aim.direction.normalized : frame * Vector3.forward;
            ShieldCone.Angles(direction, chestYaw, out float yaw, out float pitch);
            // (A threat behind the chest, as the chest turns away in a swing: the shield stays on the
            // side it was on rather than flipping across the back to the other edge of the cone.)
            if (Mathf.Abs(yaw) > 135f && shieldYawKnown)
            {
                // (Nearer the outward edge last step: out it stays; else across.)
                yaw = Mathf.Abs(lastShieldYaw + ShieldCone.Out) < Mathf.Abs(lastShieldYaw - ShieldCone.Across) ? -179f : 179f;
            }

            ShieldCone.Clamp(ref yaw, ref pitch, true);
            if (!composingDry)
            {
                lastShieldYaw = yaw;
                shieldYawKnown = true;
            }

            var turn = frame * Quaternion.Euler(-pitch, yaw, 0f);

            // Milestone 9c: held close, built from the arm: the elbow by the ribs (raised for a high
            // guard, opening toward a block), the forearm toward the threat a little in across the body,
            // the fist and the shield's face in front of it; the Fight Rhythm sways it a little (in the
            // chest's frame, so it turns round with it).
            Vector3 shoulder = anim.Part(PartId.UpperArmL).position;
            float upperLength = anim.RestLocalPosition(PartId.ForearmL).magnitude;
            float foreLength = anim.RestLocalPosition(PartId.HandL).magnitude + anim.SlotWeaponL.localPosition.magnitude;
            // (The forearm points where the face does: the centre grip, the wrist straight.)
            Vector3 forearm = turn * (Quaternion.Euler(-shield.guardAngles.x, ShieldForearmIn, 0f) * Vector3.forward);
            float high = Mathf.Clamp01(pitch / 40f);
            float open = Mathf.Clamp01(((aim.reach > 0f ? aim.reach : 1f) - 1f) / 0.1f) * ShieldBlockOpen;
            Vector3 upper = frame * Vector3.Lerp(ShieldUpperArm.normalized, ShieldUpperArmHigh.normalized, high).normalized;
            upper = Vector3.Slerp(upper, forearm, open);
            Vector3 center = shoulder + upper * upperLength + forearm * foreLength + turn * (Vector3.forward * (shield.faceCenter.z * scale))
                             + turn * ((rhythm.off - rhythm.offGuard) * ShieldSway);

            // (Never behind the chest: turned right out to the side.)
            Vector3 front = frame * Vector3.forward;
            float ahead = Vector3.Dot(center - chest, front);
            if (ahead < 0.02f * body.SizeFactor)
            {
                center += front * (0.02f * body.SizeFactor - ahead);
            }

            // Its face along the arm, angled a little up and in across the body (blows glance off it).
            Vector3 face = turn * (Quaternion.Euler((rhythm.offTurn - rhythm.offGuardTurn) * ShieldSway) * Quaternion.Euler(-shield.guardAngles.x, ShieldForearmIn, 0f) * Vector3.forward);
            if (aim.faceBlend > 0f && aim.face.sqrMagnitude > 1e-6f)
            {
                face = Vector3.Slerp(face, aim.face.normalized, Mathf.Clamp01(aim.faceBlend));
            }

            Vector3 radial = center - chest;
            if (radial.sqrMagnitude > 1e-6f && Vector3.Angle(face, radial) > MaxShieldFaceOff)
            {
                face = Vector3.RotateTowards(radial.normalized, face.normalized, MaxShieldFaceOff * Mathf.Deg2Rad, 0f);
            }

            rotation = Quaternion.LookRotation(face, Vector3.up);
            grip = center - rotation * (shield.faceCenter * scale);
        }

        /// <summary>How far a held shield's face turns in across the body (degrees).</summary>
        public const float ShieldInward = 10f;

        private static readonly PartId[] ShieldKeepsClearOf = { PartId.Pelvis, PartId.Spine, PartId.Chest, PartId.Head, PartId.ThighL, PartId.ThighR };

        /// <summary>
        /// Keeps the shield's disc out of its wielder: where its rim (or face) would be inside the
        /// torso, head or thighs, it goes further out from the chest; if the arm can't reach that far,
        /// its face turns out instead (edge-on to the body gives the most room). Measures what is left.
        /// </summary>
        private void KeepShieldClear(WeaponDefinition shield, float scale, Transform slot, bool fix, ref Vector3 grip, ref Quaternion rotation)
        {
            if (Rig == null || shield.radius <= 0f)
            {
                ShieldClearance = float.MaxValue;
                return;
            }

            float radius = shield.radius * scale;
            float margin = 0.01f * body.SizeFactor;
            Vector3 chest = anim.Part(PartId.Chest).position;
            Vector3 offset = shield.faceCenter * scale;
            for (int i = 0; i < 8; i++)
            {
                Vector3 center = grip + rotation * offset;
                float gap = ShieldGap(center, rotation, radius);
                ShieldClearance = gap;
                if (!fix || gap >= margin)
                {
                    return;
                }

                // Out from the chest (the arm keeps hold of it: it slides round the shoulder)...
                Vector3 outward = center - chest;
                outward.y *= 0.5f;
                outward = outward.sqrMagnitude > 1e-6f ? outward.normalized : rotation * Vector3.forward;
                Vector3 movedGrip = center + outward * (margin - gap + 0.005f) - rotation * offset;
                Vector3 hand = HandFor(movedGrip, rotation, slot);
                movedGrip += Reachable(PartId.UpperArmL, PartId.ForearmL, PartId.HandL, hand, ShieldReach) - hand;
                float movedGap = ShieldGap(movedGrip + rotation * offset, rotation, radius);
                if (movedGap > gap + 1e-3f)
                {
                    grip = movedGrip;
                    if (movedGap >= margin)
                    {
                        continue;
                    }
                }

                // ...and where that isn't enough, its face turned out (edge-on to the body leaves the most room).
                center = grip + rotation * offset;
                Vector3 face = Vector3.RotateTowards(rotation * Vector3.forward, outward, 12f * Mathf.Deg2Rad, 0f);
                rotation = Quaternion.LookRotation(face, Vector3.up);
                grip = center - rotation * offset;
            }

            ShieldClearance = ShieldGap(grip + rotation * offset, rotation, radius);
        }

        /// <summary>The smallest distance from the shield's disc (its rim, half way in, and its middle) to its own body (negative: inside).</summary>
        private float ShieldGap(Vector3 center, Quaternion rotation, float radius) => ShieldGap(center, rotation, radius, ShieldKeepsClearOf);

        private float ShieldGap(Vector3 center, Quaternion rotation, float radius, PartId[] against)
        {
            Vector3 u = rotation * Vector3.right, v = rotation * Vector3.up;
            float best = float.MaxValue;
            for (int k = 0; k < 13; k++)
            {
                Vector3 p;
                if (k < 8)
                {
                    float a = k * Mathf.PI / 4f;
                    p = center + radius * (Mathf.Cos(a) * u + Mathf.Sin(a) * v);
                }
                else if (k < 12)
                {
                    float a = (k - 8) * Mathf.PI / 2f + Mathf.PI / 4f;
                    p = center + 0.5f * radius * (Mathf.Cos(a) * u + Mathf.Sin(a) * v);
                }
                else
                {
                    p = center;
                }

                foreach (var id in against)
                {
                    best = Mathf.Min(best, BodyClearance.PointDistance(Rig, anim, id, p));
                }
            }

            return best;
        }

        // ---- The look -----------------------------------------------------------------------------

        /// <summary>
        /// The look on the pose: the chest turns by its share (not while a blow owns it), then the head
        /// by the rest, on top of its own weave and nods, less what the eyes take; then held inside a
        /// head's limits against the chest.
        /// </summary>
        private void ApplyLook(float rootYaw, float blowBlend)
        {
            float w = Mathf.Clamp01(LookWeight);
            if (w <= 0f)
            {
                return;
            }

            float yaw = LookYaw * w, pitch = LookPitch * w;
            var chest = anim.Part(PartId.Chest);
            var head = anim.Part(PartId.Head);
            float chestTurn = HeadLook.ChestShare(yaw, Mathf.DeltaAngle(rootYaw, ShieldCone.FacingOf(chest.rotation, rootYaw)), blowBlend);
            if (Mathf.Abs(chestTurn) > 1e-3f)
            {
                chest.rotation = Quaternion.AngleAxis(chestTurn, Vector3.up) * chest.rotation;
            }

            // (The head hangs off the chest: it has turned with it already.)
            float turn = yaw - HeadLook.EyesAcross(yaw) - chestTurn;
            if (Mathf.Abs(turn) > 1e-3f)
            {
                head.rotation = Quaternion.AngleAxis(turn, Vector3.up) * head.rotation;
            }

            float lift = pitch - HeadLook.EyesUpDown(pitch);
            Vector3 across = Vector3.Cross(Vector3.up, head.rotation * Vector3.forward);
            if (Mathf.Abs(lift) > 1e-3f && across.sqrMagnitude > 1e-6f)
            {
                head.rotation = Quaternion.AngleAxis(-lift, across.normalized) * head.rotation;
            }

            LimitHead(rootYaw);
        }

        /// <summary>Holds the head inside its limits against the chest (turn, down and up), and notes where it ended.</summary>
        private void LimitHead(float rootYaw)
        {
            var chest = anim.Part(PartId.Chest);
            var head = anim.Part(PartId.Head);
            float chestYaw = ShieldCone.FacingOf(chest.rotation, rootYaw);
            float rel = Mathf.DeltaAngle(chestYaw, ShieldCone.FacingOf(head.rotation, rootYaw));
            float held = Mathf.Clamp(rel, -HeadLook.Turn, HeadLook.Turn);
            if (Mathf.Abs(held - rel) > 1e-3f)
            {
                head.rotation = Quaternion.AngleAxis(held - rel, Vector3.up) * head.rotation;
            }

            float chestPitch = PitchOf(chest.rotation * Vector3.forward);
            float relPitch = PitchOf(head.rotation * Vector3.forward) - chestPitch;
            float heldPitch = Mathf.Clamp(relPitch, -HeadLook.Down, HeadLook.Up);
            Vector3 across = Vector3.Cross(Vector3.up, head.rotation * Vector3.forward);
            if (Mathf.Abs(heldPitch - relPitch) > 1e-3f && across.sqrMagnitude > 1e-6f)
            {
                head.rotation = Quaternion.AngleAxis(-(heldPitch - relPitch), across.normalized) * head.rotation;
            }

            HeadTurn = held;
            HeadPitch = heldPitch;
        }

        private static float PitchOf(Vector3 forward) => Mathf.Asin(Mathf.Clamp(forward.normalized.y, -1f, 1f)) * Mathf.Rad2Deg;

        /// <summary>
        /// A hand target brought within 96% of the arm's reach: an arm snapping straight swings its
        /// elbow across in a step, so the hand falls a little short instead.
        /// </summary>
        private Vector3 Reachable(PartId upper, PartId fore, PartId hand, Vector3 target, float share = 0.96f)
        {
            float length = anim.RestLocalPosition(fore).magnitude + anim.RestLocalPosition(hand).magnitude;
            Vector3 from = anim.Part(upper).position;
            Vector3 to = target - from;
            float most = share * length;
            return to.magnitude > most ? from + to.normalized * most : target;
        }

        /// <summary>The rhythm as posed this step (weighted down while a real blow plays).</summary>
        private RhythmSample rhythm;

        private static RhythmSample Weighted(RhythmSample s, float w)
        {
            if (w >= 1f)
            {
                return s;
            }

            w = Mathf.Max(0f, w);
            s.hips *= w;
            s.drop *= w;
            s.lean *= w;
            s.roll *= w;
            s.turn *= w;
            s.hipTurn *= w;
            s.head *= w;
            s.main *= w;
            s.mainTurn *= w;
            s.off *= w;
            s.offTurn *= w;
            return s;
        }

        /// <summary>A Fight Rhythm's offset on a hand's guard: the grip moved and the weapon turned (unit space).</summary>
        private static void RhythmGuard(Quaternion facing, Vector3 offset, Vector3 turn, ref Vector3 grip, ref Quaternion rotation)
        {
            grip += facing * offset;
            if (turn.sqrMagnitude > 1e-6f)
            {
                rotation = facing * Quaternion.Euler(turn) * Quaternion.Inverse(facing) * rotation;
            }
        }

        /// <summary>
        /// The empty off hand (a boxer's lead hand at the cheek, a stick fighter's alive hand out in
        /// front): the Fight Rhythm places it against the chest (unit space, metres), blending in
        /// from the plain guard by the rhythm's weight.
        /// </summary>
        private void PoseBareOffHand(Quaternion facing)
        {
            if (rhythm.off.sqrMagnitude < 1e-8f && Flail <= 0f)
            {
                return;
            }

            // (In the chest's frame: the hand goes round with the body as it turns, and the elbow's
            // pole with it, so a wind-up's turn never swings the pole along the arm.)
            var hand = anim.Part(PartId.HandL);
            var chestPart = anim.Part(PartId.Chest);
            var chestTurn = Quaternion.LookRotation(Vector3.ProjectOnPlane(chestPart.rotation * Vector3.forward, Vector3.up).sqrMagnitude > 1e-6f
                ? Vector3.ProjectOnPlane(chestPart.rotation * Vector3.forward, Vector3.up) : facing * Vector3.forward, Vector3.up);
            Vector3 placed = chestPart.position + chestTurn * rhythm.off;
            float w = Mathf.Clamp01(rhythm.off.magnitude / Mathf.Max(1e-4f, Rhythm.off.magnitude));
            Vector3 target = Vector3.Lerp(hand.position, placed, Rhythm.off.sqrMagnitude > 1e-8f ? w : 0f) + FlailOffset(facing, -1f);
            target = Reachable(PartId.UpperArmL, PartId.ForearmL, PartId.HandL, target);
            // (The elbow down under the fist, a little out: tucked in front of the ribs.)
            TwoBoneIK.Solve(anim.Part(PartId.UpperArmL), anim.Part(PartId.ForearmL), hand, target, chestTurn * new Vector3(-0.35f, -1f, 0f));
            // A fist on a straight wrist, in line with the forearm (as the plain guard holds it).
            hand.localRotation = Quaternion.Euler(-10f, 0f, 0f);
        }

        /// <summary>Stumbling, a hand goes out to its side, up, and forward against the fall.</summary>
        private Vector3 FlailOffset(Quaternion facing, float side)
        {
            if (Flail <= 0f)
            {
                return Vector3.zero;
            }

            float s = body.SizeFactor;
            Vector3 against = -FlailDirection;
            against.y = 0f;
            return Flail * (facing * new Vector3(side * 0.24f * body.WidthFactor, 0.14f * s, 0f) + against * (0.18f * s));
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

        /// <summary>How far a knee may point from the hips' facing (degrees): a hip's own turn, a little inside a person's.</summary>
        public const float HipTurnOut = 40f;

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
            // Knees point over the toes, a little outward; but a hip turns only so far (milestone 9b):
            // a foot turned further from the hips than that has the knee point part way.
            float hipsYaw = ShieldCone.FacingOf(anim.Part(PartId.Pelvis).rotation, stance.Yaw(foot));
            float kneeYaw = hipsYaw + Mathf.Clamp(Mathf.DeltaAngle(hipsYaw, stance.Yaw(foot)), -HipTurnOut, HipTurnOut);
            Vector3 pole = Quaternion.Euler(0f, kneeYaw, 0f) * new Vector3(left ? -0.15f : 0.15f, 0f, 1f);
            TwoBoneIK.Solve(thigh, shin, ankle, AnkleTarget(foot), pole);
            AlignHinge(left ? PartId.ThighL : PartId.ThighR, left ? PartId.ShinL : PartId.ShinR);
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
                if (IsSevered != null && IsSevered(id))
                {
                    continue;
                }

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
