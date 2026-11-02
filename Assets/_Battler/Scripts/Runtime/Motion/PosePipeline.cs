using PhysicsBattler.Combat;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using UnityEngine;
using PhysicsBattler.Diagnostics;

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
    public sealed partial class PosePipeline
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
        /// <summary>
        /// Milestone 10: a combo's last link, still playing on through its follow-through, and how much
        /// of the pose is still its (1 at the hand-over, easing to 0); the lunge the root took over
        /// from it (world, horizontal).
        /// </summary>
        public MoveState LinkFrom { get; set; }

        public float LinkWeight { get; set; }

        /// <summary>
        /// The arm rebuild: a combo's link with no blow to hand over from (after a stopped one) eases
        /// the weapon arm out of where it was held when the link started (by <see cref="LinkWeight"/>).
        /// True takes the arm as it is now; false lets it go.
        /// </summary>
        public void HoldLinkArm(bool hold)
        {
            LinkArmHeld = hold;
            linkCapture = true;
            linkHandOffset = Vector3.zero;
            linkWristOffset = Quaternion.identity;
            linkSwivelOffset = 0f;
            // (As the arm stands now, the last step's pose done: the hand from the shoulder in the
            // chest's frame, so it rides on the chest as the hand-over eases the body.)
            var chest = anim.Part(PartId.Chest);
            linkHand = Quaternion.Inverse(chest.rotation) * (anim.Part(PartId.HandR).position - anim.Part(PartId.UpperArmR).position);
            linkWrist = anim.Part(PartId.HandR).localRotation;
            linkSwivel = LastWeaponArm.swivel;
        }

        public bool LinkArmHeld { get; private set; }

        private Vector3 linkHand;
        private Quaternion linkWrist = Quaternion.identity;
        private float linkSwivel;

        // (A hand-over from a link: what the last link's keys missed of where its arm was, kept for the blend.)
        private bool linkCapture;

        /// <summary>The hand-over's offset on the last link's hand (for the reports).</summary>
        public Vector3 LinkHandOffset => linkHandOffset;
        private Vector3 linkHandOffset;
        private Quaternion linkWristOffset = Quaternion.identity;
        private float linkSwivelOffset;

        public Vector3 LinkBank { get; set; }

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
        /// <remarks>
        /// <paramref name="armPlays"/>, with <paramref name="dry"/>: the weapon arm carries on from its
        /// last step all the same (the standing pose a get-up blends into, composed each step: a fresh
        /// solve every step would let the arm's shape flick about).
        /// </remarks>
        public void Compose(float time, float dt, bool dry = false, bool armPlays = false)
        {
            // (Timed for the playtest log's performance table.)
            using (Perf.Begin(dry ? PerfSection.ComposeDry : PerfSection.Compose))
            {
                ComposeCore(time, dt, dry, armPlays);
            }
        }

        private void ComposeCore(float time, float dt, bool dry, bool armPlays)
        {
            composingDry = dry;
            composingArmPlays = dry && armPlays;
            composeDt = dt;
            if (!dry || armPlays)
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
                    float hipTurnNow = move.HipTurn * moveBlend, chestTurnNow = move.ChestTurn * moveBlend;
                    lean = move.Lean * moveBlend;
                    float crouchNow = move.Crouch * moveBlend;
                    // (Milestone 10: from a combo's last link into this one, its turns carry over.)
                    var from = LinkFrom;
                    float lw = LinkWeight;
                    if (from != null && lw > 0f && from.Move.shape != MoveShape.Path)
                    {
                        float fb = from.Blend;
                        hipTurnNow = Mathf.Lerp(hipTurnNow, from.HipTurn * fb, lw);
                        chestTurnNow = Mathf.Lerp(chestTurnNow, from.ChestTurn * fb, lw);
                        lean = Mathf.Lerp(lean, from.Lean * fb, lw);
                        crouchNow = Mathf.Lerp(crouchNow, from.Crouch * fb, lw);
                    }

                    pelvis.rotation = Quaternion.AngleAxis(hipTurnNow, Vector3.up) * pelvis.rotation;
                    chest.rotation = Quaternion.AngleAxis(chestTurnNow, Vector3.up) * chest.rotation;
                    pelvis.position += Vector3.down * (crouchNow * leg);
                }

                var spine = anim.Part(PartId.Spine);
                spine.localRotation = Quaternion.Euler(0.5f * lean, 0f, 0f) * spine.localRotation;
                chest.localRotation = Quaternion.Euler(0.5f * lean, 0f, 0f) * chest.localRotation;
                // Blows at something low are met by sinking: the further below the chest, the deeper.
                float below = chest.position.y - move.Aim.y - 0.35f * body.SizeFactor;
                MoveCrouch = Mathf.Clamp(0.5f * below, 0f, 0.22f * leg) * Mathf.Clamp01(moveBody) * moveBlend;
                if (LinkFrom != null && LinkWeight > 0f)
                {
                    // (Milestone 10: the last link's sink eases out.)
                    float belowFrom = chest.position.y - LinkFrom.Aim.y - 0.35f * body.SizeFactor;
                    float crouchFrom = Mathf.Clamp(0.5f * belowFrom, 0f, 0.22f * leg) * Mathf.Clamp01(LinkFrom.Body) * LinkFrom.Blend;
                    MoveCrouch = Mathf.Lerp(MoveCrouch, crouchFrom, LinkWeight);
                }

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
                float offsetBlend = moveBlend;
                // (Milestone 10: the last link's lunge, banked into the root, eases into this one's.)
                if (LinkFrom != null && LinkWeight > 0f)
                {
                    var from = LinkFrom;
                    Vector3 fromOffset = from.Lunge * Mathf.Max(0f, from.Body) + (from.Frame * Vector3.forward) * from.Drive - LinkBank;
                    fromOffset.y = 0f;
                    offset = Vector3.Lerp(offset, fromOffset, LinkWeight);
                    offsetBlend = Mathf.Lerp(moveBlend, from.Blend, LinkWeight);
                }

                pelvis.position += (offset - carried) * offsetBlend;
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
                if (move.Move.hand == MoveHand.Off)
                {
                    // (Milestone 10: an off-hand blow: the weapon hand keeps the rhythm's guard; the off
                    // hand lets go of it.)
                    rhythm.main = r.main;
                    rhythm.mainTurn = r.mainTurn;
                    rhythm.off = r.off * hand;
                    rhythm.offTurn = r.offTurn * hand;
                }
                else
                {
                    rhythm.main = r.main * hand;
                    rhythm.mainTurn = r.mainTurn * hand;
                }
            }

            {
                // The tempo's stance on top, at full weight but for the weapon hand during a blow.
                var m = Mood;
                if (move != null && move != Feint)
                {
                    float hand = Mathf.Clamp01(1f - 2f * moveBlend);
                    if (move.Move.hand == MoveHand.Off)
                    {
                        m.off *= hand * hand;
                        m.offTurn *= hand * hand;
                    }
                    else
                    {
                        m.main *= hand * hand;
                        m.mainTurn *= hand * hand;
                    }
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
                // (Milestone 10: an off-hand blow leaves the weapon arm in its guard.)
                bool offBlow = move != null && move.Move.hand == MoveHand.Off;
                PoseWeaponArm(loadout, offBlow ? null : move, offBlow ? 0f : moveBlend, facing);
            }

            // (The performance pass: a dry pose of a weapon blow, the planner's, poses no off arm:
            // nothing it measures looks at it, and the shield arm's solves were a fifth of its work.)
            bool offArmUnseen = composingDry && !composingArmPlays && move != null && move.Move.hand == MoveHand.Main && SkipOffArmWhenDry;
            if (offArmUnseen)
            {
            }
            else if (loadout != null && loadout.HasShield)
            {
                PoseShieldArm(loadout, facing);
                PoseBash(loadout, facing);
            }
            else if (loadout == null || loadout.Off == null)
            {
                PoseBareOffHand(facing);
            }

            // Milestone 9c: what the hands hold kept out of the ground.
            // (Playing, the lift carries on from last step's, changing a little a step; a tried pose
            // lifts afresh.)
            GroundClamp.Memory mainLift = null, offLift = null;
            if (!composingDry || composingArmPlays)
            {
                if (liftComposed != composeCount - 1)
                {
                    mainClampMemory.known = offClampMemory.known = false;
                }

                liftComposed = composeCount;
                mainLift = mainClampMemory;
                offLift = offClampMemory;
            }

            // (The weapon arm keeps its weapon out of the ground on its target, before its solve.)
            WeaponGroundGap = KeepWeaponsOffGround(anim, Loadout, body.SizeFactor, stance.GroundY, 1f, false, mainLift, offLift, keepMain: false);
            if (Loadout?.Main != null)
            {
                WeaponGroundGap = Mathf.Min(WeaponGroundGap, WeaponLowest(Loadout.Main, Loadout.Scale, anim.SlotWeaponR.position, anim.SlotWeaponR.rotation) - stance.GroundY);
            }
            CenterOfMass = CenterOfMassNow();
        }

        /// <summary>The lowest point of what the hands hold over the ground in this pose (metres; negative: in it).</summary>
        public float WeaponGroundGap { get; private set; } = float.MaxValue;

        private readonly GroundClamp.Memory mainClampMemory = new GroundClamp.Memory(), offClampMemory = new GroundClamp.Memory();
        private int liftComposed = -10;

        /// <summary>Keeps what both hands hold out of the ground (<see cref="GroundClamp"/>); returns the lowest gap left.</summary>
        public static float KeepWeaponsOffGround(AnimRig anim, Loadout loadout, float size, float groundY, float weight = 1f, bool free = false,
            GroundClamp.Memory mainMemory = null, GroundClamp.Memory offMemory = null, bool keepMain = true)
        {
            if (loadout == null)
            {
                return float.MaxValue;
            }

            float gap = float.MaxValue;
            if (loadout.Main != null && keepMain)
            {
                gap = Mathf.Min(gap, GroundClamp.Keep(anim, true, loadout.Main, loadout.Scale, size, groundY, weight, free, mainMemory));
            }

            if (loadout.Off != null)
            {
                gap = Mathf.Min(gap, GroundClamp.Keep(anim, false, loadout.Off, loadout.Scale, size, groundY, weight, free, offMemory));
            }

            return gap;
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

        private void PoseWeaponArm(Loadout loadout, MoveState move, float moveBlend, Quaternion facing)
        {
            var upper = anim.Part(PartId.UpperArmR);
            var fore = anim.Part(PartId.ForearmR);
            var hand = anim.Part(PartId.HandR);
            var slot = anim.SlotWeaponR;
            float scale = loadout.Scale;
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

            // ---- The arm rebuild: the arm's three controls (ArmRig) -------------------------------
            // Everything below sets the controls: where the weapon goes, how it is turned, and the
            // elbow null. One solve at the end makes the arm; nothing changes the arm after it.
            float guardSwivel = Style != null ? Style.guardElbowSwivel : 20f;
            if (move != null && moveBlend > 0f)
            {
                Vector3 strikeLocal = (move.Weapon != null ? move.Weapon.StrikePoint(move.Move.strikeWith) : Vector3.forward * 0.6f) * scale;
                LeanForContact(move, moveBlend, strikeLocal, hand, slot);
            }

            // The guard: the weapon held as the guard has it (its target: the weapon's guard, the
            // rhythm's, a hit's knock, out of the chest and off the ground), the wrist taking as much
            // of its turn as it can, the elbow at the guard's null. Measured, then solved below the
            // same way as a blow, from where the hand is.
            var torso = Rig != null ? new BodyClearance.Posed(Rig, anim, scale) : null;
            GuardTarget(loadout, facing, scale, torso, out var guardGrip, out var guardRotation);
            GuardGrip = guardGrip;
            GuardTurn = guardRotation;
            // (The elbow kept to the nearest best place to its null, so it never jumps to another way of
            // holding the guard as the guard moves.)
            var guardGoal = WeaponGoal(hand, slot, guardGrip, guardRotation, Vector3.zero, guardSwivel, loadout.Main);
            guardGoal.nearNull = true;
            var guardSolve = ArmRig.Solve(anim, true, guardGoal, torso, apply: false);
            Vector3 guardHand = guardSolve.wrist;
            Quaternion guardWrist = guardSolve.wristLocal;
            guardSwivel = guardSolve.swivel;

            // Hand-first: the hand is the arm's effector. A blow moves it through its keys (the wind-up,
            // the contact place solved when it was planned, the follow-through) on arcs round the
            // shoulder, with the wrist and the elbow null keyed at the same moments; the weapon follows
            // the hand, and lands on the aim at the striking moment.
            Vector3 shoulderAt = upper.position;
            Vector3 handAt = guardHand;
            Quaternion wrist = guardWrist;
            float elbowSwivel = guardSwivel;
            if (move != null && moveBlend > 0f)
            {
                // The hand planned for contact lands the striking part on the aim in a dry pose; played,
                // the body is a little elsewhere. So the contact key itself is moved by what it would
                // miss by now (the hand at the key, the contact's wrist and elbow, found again onto the
                // aim): the hand's way through its keys carries the change, all of it at the striking
                // moment and none at the wind-up or the follow-through, and since the body's difference
                // grows and shrinks smoothly, so does the change.
                Vector3 contactShift = Vector3.zero;
                if (move.HasContactShift && move.T > move.ContactTime)
                {
                    // (Past the striking moment: as it landed.)
                    contactShift = move.ContactShift;
                }
                else if (move != Feint && move.Generated && move.HasContactHand && !DebugNoCorrection)
                {
                    Vector3 strikeLocal = (move.Weapon != null ? move.Weapon.StrikePoint(move.Move.strikeWith) : Vector3.forward * 0.6f) * scale;
                    Vector3 inHand = Quaternion.Inverse(hand.rotation) * (slot.position - hand.position) + slot.localRotation * strikeLocal;
                    Vector3 contactHand = move.Aim + move.Frame * move.ContactHand;
                    var atContact = new ArmGoal
                    {
                        point = contactHand,
                        wristGiven = true,
                        wristLocal = ArmRig.Wrist(anim, true, move.Move.wristContact),
                        elbowSwivel = move.Move.elbowContact,
                    };
                    Vector3 found = HandForStrike(atContact, inHand, move.Aim, contactHand, 3, move.Scale, null);
                    contactShift = found - contactHand;
                    if (!composingDry && move.T >= move.ContactTime)
                    {
                        move.ContactShift = contactShift;
                        move.HasContactShift = true;
                    }

                    CorrectionWeight = 1f;
                    CorrectionSize = contactShift.magnitude;
                    CorrectionMiss = Vector3.Distance(hand.position + hand.rotation * inHand, move.Aim);
                }

                handAt = AroundShoulder(shoulderAt, guardHand, KeyedHand(move, shoulderAt, guardHand, contactShift) + KnockR, moveBlend);
                wrist = Quaternion.Slerp(guardWrist, KeyedWrist(move, guardWrist), moveBlend);
                elbowSwivel = Mathf.Lerp(guardSwivel, move.Move.ElbowSwivel(move.T, guardSwivel), moveBlend);

                // Milestone 10: from a combo's last link (still carrying on through its
                // follow-through), the hand, the wrist and the elbow ease into this link's.
                if (LinkFrom != null && LinkWeight > 0f)
                {
                    float fb = LinkFrom.Blend;
                    Vector3 fromHand = AroundShoulder(shoulderAt, guardHand, KeyedHand(LinkFrom, shoulderAt, guardHand, LinkFrom.HasContactShift ? LinkFrom.ContactShift : Vector3.zero) + KnockR, fb);
                    Quaternion fromWrist = Quaternion.Slerp(guardWrist, KeyedWrist(LinkFrom, guardWrist), fb);
                    float fromSwivel = Mathf.Lerp(guardSwivel, LinkFrom.Move.ElbowSwivel(LinkFrom.T, guardSwivel), fb);
                    if (linkCapture && !composingDry)
                    {
                        // (The hand-over's first step: what the last link's keys miss of where its arm
                        // really was (its contact found again, a hit's rebound), kept as an offset on
                        // them, so the arm carries on from where it was.)
                        linkHandOffset = shoulderAt + anim.Part(PartId.Chest).rotation * linkHand - fromHand;
                        linkWristOffset = linkWrist * Quaternion.Inverse(fromWrist);
                        linkSwivelOffset = Mathf.DeltaAngle(fromSwivel, linkSwivel);
                    }

                    if (!composingDry)
                    {
                        linkCapture = false;   // (a dry pose taken as the link starts, its path laid, doesn't take the hand-over's first step)
                    }

                    fromHand += linkHandOffset;
                    fromWrist = linkWristOffset * fromWrist;
                    fromSwivel += linkSwivelOffset;
                    handAt = AroundShoulder(shoulderAt, handAt, fromHand, LinkWeight);
                    wrist = Quaternion.Slerp(wrist, fromWrist, LinkWeight);
                    elbowSwivel = elbowSwivel + Mathf.DeltaAngle(elbowSwivel, fromSwivel) * LinkWeight;
                }
                else if (LinkArmHeld && LinkWeight > 0f)
                {
                    // A link after a stopped one: from where the arm was held when it started.
                    handAt = AroundShoulder(shoulderAt, handAt, shoulderAt + anim.Part(PartId.Chest).rotation * linkHand, LinkWeight);
                    wrist = Quaternion.Slerp(wrist, linkWrist, LinkWeight);
                    elbowSwivel = elbowSwivel + Mathf.DeltaAngle(elbowSwivel, linkSwivel) * LinkWeight;
                }
            }

            // (Never folded tight: near its full bend the elbow's circle turns fast round a short arm,
            // and a hand that moves a little swings the elbow a long way.)
            handAt = Unfolded(shoulderAt, handAt);
            var goal = new ArmGoal
            {
                point = handAt,
                wristGiven = true,
                wristLocal = wrist,
                elbowSwivel = elbowSwivel,
                clearance = ArmClearance * body.SizeFactor,
            };
            LastWeaponArm = ArmRig.Solve(anim, true, goal, torso);
            HandKeyed = goal.point;
            HandCleared = Vector3.zero;
            if (torso != null && move != null && moveBlend > 0f)
            {
                // A blow's keyed hand's way is the author's; where it (or the weapon it carries) would
                // pass through the body, the hand is moved out until the arm and the weapon clear it.
                // Last, so the arm never goes through its own body: a blow that would land only through
                // it misses in its dry run, and the planner turns it down. (Blows only, eased in with the
                // blow: the guard keeps its room by its own measure, and a push that steps from one
                // nearest piece to another would make a held guard jump.)
                Vector3 before = goal.point;
                Vector3 cleared = ClearOfTorso(goal, torso, loadout.Main != null ? loadout.Main.length * 0.98f : 0.8f, scale, ArmClearance * body.SizeFactor);
                goal.point = Vector3.Lerp(before, cleared, moveBlend);
                if (moveBlend < 1f)
                {
                    LastWeaponArm = ArmRig.Solve(anim, true, goal, torso);
                }

                HandCleared = goal.point - before;
            }

            float under = WeaponLowest(loadout.Main, scale, slot.position, slot.rotation) - (stance.GroundY + GroundClamp.Margin * body.SizeFactor);
            if (under < -0.005f)
            {
                // (A weapon the arm and wrist still bring down: the hand's point raised by what is left,
                // and solved once more.)
                goal.point.y -= under;
                LastWeaponArm = ArmRig.Solve(anim, true, goal, torso);
            }

            WeaponArmSwivel = elbowSwivel;

            WristBend = Vector3.Angle(fore.rotation * anim.RestLocalPosition(PartId.HandR), hand.rotation * anim.RestLocalPosition(PartId.HandR));
            if (move != null)
            {
                Vector3 local = move.Weapon != null ? move.Weapon.StrikePoint(move.Move.strikeWith) : Vector3.forward * 0.6f;
                StrikePoint = slot.position + slot.rotation * (local * scale);
                AimError = Vector3.Distance(StrikePoint, move.Aim);
            }
        }

        /// <summary>
        /// The weapon's guard target: the weapon's guard, the rhythm's, a hit's knock, the grip moved out
        /// of the chest (a guard tucked into it leaves the wrist no room), and kept off the ground.
        /// </summary>
        private void GuardTarget(Loadout loadout, Quaternion facing, float scale, BodyClearance.Posed torso, out Vector3 grip, out Quaternion rotation)
        {
            GuardPose(loadout.Main, true, facing, scale, out grip, out rotation);
            grip += FlailOffset(facing, 1f);
            RhythmGuard(facing, rhythm.main, rhythm.mainTurn, ref grip, ref rotation);
            grip += KnockR;
            rotation = TurnOf(KnockTurnR) * rotation;
            if (torso != null)
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
                    grip += away * -room;
                }
            }

            KeepTargetOffGround(loadout.Main, scale, ref grip, ref rotation);
        }

        /// <summary>
        /// A blow whose striking moment the arm can't reach (with the wrist the move keys there) leans
        /// the spine toward it: the bend the contact point needs, eased in toward the striking moment
        /// and out after it (a function of the blow's time, so it never jumps), up to 50 degrees.
        /// </summary>
        private void LeanForContact(MoveState move, float moveBlend, Vector3 strikeLocal, Transform hand, Transform slot)
        {
            if (move == Feint || !move.Generated)
            {
                return;
            }

            float tc = move.ContactTime, t = move.T;
            float w = t <= tc ? Smooth01((t - (tc - LeanAhead)) / LeanAhead) : 1f - Smooth01((t - tc) / LeanAfter);
            w *= moveBlend;
            if (LinkFrom != null)
            {
                w *= 1f - LinkWeight;
            }

            if (w <= 0f)
            {
                return;
            }

            var spine = anim.Part(PartId.Spine);
            var upper = anim.Part(PartId.UpperArmR);
            Vector3 inHand = Quaternion.Inverse(hand.rotation) * (slot.position - hand.position) + slot.localRotation * strikeLocal;
            float longest = ArmRig.MaxReach(anim, true, ArmRig.Wrist(anim, true, move.Move.wristContact), inHand);
            Vector3 point = move.Aim;
            Quaternion before = spine.rotation;
            float bent = 0f;
            for (int i = 0; i < 6; i++)
            {
                Vector3 shoulder = upper.position;
                float excess = Vector3.Distance(point, shoulder) - 0.97f * longest;
                if (excess <= 0f || bent >= 50f)
                {
                    break;
                }

                Vector3 a = shoulder - spine.position;
                Vector3 c = point - spine.position;
                Vector3 axis = Vector3.Cross(a, c);
                if (axis.sqrMagnitude < 1e-8f)
                {
                    break;
                }

                float angle = Mathf.Min(Vector3.Angle(a, c), Mathf.Min(12f, 50f - bent), Mathf.Rad2Deg * excess / Mathf.Max(0.1f, a.magnitude) + 1f);
                spine.rotation = Quaternion.AngleAxis(angle, axis.normalized) * spine.rotation;
                bent += angle;
            }

            // (The whole bend the striking moment needs, weighted by how near it is.)
            Quaternion full = spine.rotation * Quaternion.Inverse(before);
            spine.rotation = Quaternion.Slerp(Quaternion.identity, full, w) * before;
            SpineBend += bent * w;
        }

        private static float Smooth01(float x)
        {
            x = Mathf.Clamp01(x);
            return x * x * (3f - 2f * x);
        }

        /// <summary>How long before the striking moment the lean for it starts, and how long after it eases out (normalized time).</summary>
        public const float LeanAhead = 0.25f, LeanAfter = 0.3f;

        /// <summary>A blow's hand at its time now: between its keys (the guard's hand at its start and end).</summary>
        private Vector3 KeyedHand(MoveState state, Vector3 shoulder, Vector3 guardHand, Vector3 contactShift)
        {
            Vector3 Key(int k)
            {
                switch (k)
                {
                    case 1: return shoulder + state.Frame * (state.Move.handWindUp * state.Scale);
                    case 2: return state.HasContactHand ? state.Aim + state.Frame * state.ContactHand + contactShift : shoulder + state.Frame * (new Vector3(0.1f, 0f, 0.5f) * state.Scale);
                    case 3: return shoulder + state.Frame * (state.Move.handFollow * state.Scale);
                    default: return guardHand;
                }
            }

            // (A thrust drives its hand in a line; a swing carries it round the shoulder.)
            state.Move.HandPhase(state.T, out int from, out int to, out float share);
            if (state.Move.shape == MoveShape.Thrust)
            {
                return Vector3.Lerp(Key(from), Key(to), share);
            }

            // Through the striking moment the hand goes on without a corner: the arc into the contact
            // and the arc out of it (each round the shoulder, in its own plane) are bent near the
            // contact to leave it the same way, half way between the two.
            if (from == 1 && to == 2 || from == 2 && to == 3)
            {
                Vector3 wind = Key(1), contact = Key(2), follow = Key(3);
                Vector3 inward = ArcTangent(shoulder, wind, contact, 1f), outward = ArcTangent(shoulder, contact, follow, 0f);
                if (inward.sqrMagnitude > 1e-8f && outward.sqrMagnitude > 1e-8f)
                {
                    // (Each arc's pace at the contact per unit of progress, its knots at its keys'; the
                    // hand leaves the contact the way it came and at the same pace: half way between the
                    // two arcs, at their mean pace.)
                    state.Move.KeyProgress(out float p1, out float p2, out float p3);
                    Vector3 vIn = inward / (p2 - p1), vOut = outward / (p3 - p2);
                    Vector3 through = (inward.normalized + outward.normalized).normalized * (0.5f * (vIn.magnitude + vOut.magnitude));
                    Vector3 a = from == 1 ? wind : contact, b = from == 1 ? contact : follow;
                    Vector3 own = from == 1 ? inward : outward;
                    float span = from == 1 ? p2 - p1 : p3 - p2;
                    // (Hermite's basis: the contact end's slope changed, the other end and both places kept.)
                    float h = from == 1 ? share * share * share - share * share : share * share * share - 2f * share * share + share;
                    return AroundShoulder(shoulder, a, b, share) + h * (through * span - own);
                }
            }

            return AroundShoulder(shoulder, Key(from), Key(to), share);
        }

        /// <summary>The way a hand going round the shoulder from one place to another moves at a share of the way (per share).</summary>
        private static Vector3 ArcTangent(Vector3 shoulder, Vector3 a, Vector3 b, float share)
        {
            const float e = 1e-3f;
            float s0 = Mathf.Clamp01(share - e), s1 = Mathf.Clamp01(share + e);
            return (AroundShoulder(shoulder, a, b, s1) - AroundShoulder(shoulder, a, b, s0)) / (s1 - s0);
        }

        /// <summary>The hand moved out from the shoulder, if it is nearer, to where the elbow is bent <see cref="MostFold"/>.</summary>
        private Vector3 Unfolded(Vector3 shoulder, Vector3 hand)
        {
            float l1 = anim.RestLocalPosition(PartId.ForearmR).magnitude, l2 = anim.RestLocalPosition(PartId.HandR).magnitude;
            float least = Mathf.Sqrt(l1 * l1 + l2 * l2 + 2f * l1 * l2 * Mathf.Cos(MostFold * Mathf.Deg2Rad));
            Vector3 d = hand - shoulder;
            float at = d.magnitude;
            return at >= least || at < 1e-5f ? hand : shoulder + d * (least / at);
        }

        /// <summary>How far the weapon arm's elbow bends at most in a pose (degrees; its limit is <see cref="ArmRig.ElbowBendLimit"/>).</summary>
        public const float MostFold = 125f;

        /// <summary>Between two hand places on an arc round the shoulder (the way, then the reach), as a swinging hand goes.</summary>
        private static Vector3 AroundShoulder(Vector3 shoulder, Vector3 a, Vector3 b, float share)
        {
            Vector3 da = a - shoulder, db = b - shoulder;
            float la = da.magnitude, lb = db.magnitude;
            if (la < 1e-5f || lb < 1e-5f)
            {
                return Vector3.Lerp(a, b, share);
            }

            // (Two ways nearly opposite have no one arc between them, and the arc would swing from one
            // side of the shoulder to the other as they turn: the farther apart they are past 120°, the
            // more the hand goes straight across instead, so it never jumps.)
            Vector3 round = shoulder + Vector3.Slerp(da / la, db / lb, share) * Mathf.Lerp(la, lb, share);
            float apart = Vector3.Angle(da, db);
            return apart <= 120f ? round : Vector3.Lerp(round, Vector3.Lerp(a, b, share), Smooth01((apart - 120f) / 40f));
        }

        /// <summary>
        /// The arm rebuild: where the hand must be at a blow's striking moment, the body posed there,
        /// for its striking part to land on the aim with the contact's wrist and elbow keys (a few
        /// Newton steps on the hand's place). Kept in the blow's aim frame, from the aim.
        /// </summary>
        public void PlanContactHand(MoveState state, float scale)
        {
            var hand = anim.Part(PartId.HandR);
            var slot = anim.SlotWeaponR;
            var upper = anim.Part(PartId.UpperArmR);
            Vector3 strikeLocal = (state.Weapon != null ? state.Weapon.StrikePoint(state.Move.strikeWith) : Vector3.forward * 0.6f) * scale;
            Vector3 inHand = Quaternion.Inverse(hand.rotation) * (slot.position - hand.position) + slot.localRotation * strikeLocal;
            var goal = new ArmGoal { wristGiven = true, wristLocal = ArmRig.Wrist(anim, true, state.Move.wristContact), elbowSwivel = state.Move.elbowContact };
            Vector3 shoulder = upper.position;
            Vector3 handAt = HandForStrike(goal, inHand, state.Aim, shoulder + (state.Aim - shoulder) * 0.6f, 20, state.Scale, null);
            // (Kept from the aim, not the shoulder: where the hand must be in the world for the blow to
            // land, however the body plays its way there.)
            state.ContactHand = Quaternion.Inverse(state.Frame) * (handAt - state.Aim);
            state.HasContactHand = true;
            goal.point = handAt;
            ArmRig.Solve(anim, true, goal);
            state.ContactHandMiss = Vector3.Distance(hand.position + hand.rotation * inHand, state.Aim);
            state.ContactShoulder = shoulder;
        }

        /// <summary>
        /// The hand's place moved, as little as it takes, so the arm and the weapon (grip to tip) clear
        /// the torso by <paramref name="clear"/>, as <see cref="BodyClearance.Measure"/> counts it: a
        /// few Newton steps on the clearance's slope (measured a centimetre each way). The hand's place
        /// unchanged when already clear. Leaves the arm solved there.
        /// </summary>
        private Vector3 ClearOfTorso(ArmGoal goal, BodyClearance.Posed torso, float bladeLength, float scale, float clear)
        {
            float Clearance(Vector3 at)
            {
                goal.point = at;
                LastWeaponArm = ArmRig.Solve(anim, true, goal, torso);
                var slot = anim.SlotWeaponR;
                Vector3 tip = slot.position + slot.rotation * (Vector3.forward * bladeLength * scale);
                // (Soft: its slope turns smoothly as the nearest piece changes, so the push doesn't step.)
                return torso.Soft(anim.Part(PartId.UpperArmR).position, anim.Part(PartId.ForearmR).position, anim.Part(PartId.HandR).position, slot.position, tip, ClearSoftness * body.SizeFactor);
            }

            // (Every step a smooth function of where the hand is: how far to go eases in from nothing
            // as the clearance runs short (a soft plus over a centimetre), never stepping in at a
            // threshold; and the same few steps every time, not as many as it takes.)
            Vector3 p = goal.point;
            float soft = ClearEase * body.SizeFactor;
            float c = Clearance(p);
            if (c > clear + 6f * soft)
            {
                return p;   // (well clear: nothing to ease, the push is nil to a hair)
            }

            for (int i = 0; i < ClearSteps; i++)
            {
                const float h = 0.01f;
                var slope = new Vector3(Clearance(p + Vector3.right * h) - c, Clearance(p + Vector3.up * h) - c, Clearance(p + Vector3.forward * h) - c) / h;
                if (slope.sqrMagnitude < 1e-4f)
                {
                    break;
                }

                float x = (clear - c) / soft;
                float short_ = soft * (x > 20f ? x : Mathf.Log(1f + Mathf.Exp(x)));
                Vector3 step = slope * (short_ / slope.sqrMagnitude);
                p += Vector3.ClampMagnitude(step, ClearStepMost * body.SizeFactor);
                c = Clearance(p);
            }

            return p;
        }

        /// <summary>How softly <see cref="ClearOfTorso"/> blends the nearest pieces of the arm and blade (metres, a medium body's).</summary>
        private const float ClearSoftness = 0.008f;

        /// <summary>Over how much shortfall <see cref="ClearOfTorso"/>'s push eases in (metres, a medium body's).</summary>
        private const float ClearEase = 0.006f;

        /// <summary>How many steps <see cref="ClearOfTorso"/> takes, and how far one may move the hand (metres, a medium body's).</summary>
        private const int ClearSteps = 3;
        private const float ClearStepMost = 0.08f;

        /// <summary>
        /// Where the hand must be for a point of it (<paramref name="inHand"/>, the striking part) to land
        /// on <paramref name="target"/>, the arm solved as <paramref name="goal"/> has it otherwise: a few
        /// Newton steps on the hand's place, from <paramref name="start"/>. Leaves the arm solved there.
        /// </summary>
        private Vector3 HandForStrike(ArmGoal goal, Vector3 inHand, Vector3 target, Vector3 start, int steps, float size, BodyClearance.Posed torso)
        {
            var hand = anim.Part(PartId.HandR);
            Vector3 Strike(Vector3 at)
            {
                goal.point = at;
                ArmRig.Solve(anim, true, goal, torso);
                return hand.position + hand.rotation * inHand;
            }

            // (The hand kept within the arm's reach: past it the arm stops straight and a hand moved
            // further moves nothing, so the steps would run off.)
            Vector3 shoulder = anim.Part(PartId.UpperArmR).position;
            // (A place asked a little past the eased reach still moves the arm: two of the easing's
            // lengths past it, the arm is at 99.6% of its length.)
            float reach = (2f - ArmRig.SoftReach) * (anim.RestLocalPosition(PartId.ForearmR).magnitude + anim.RestLocalPosition(PartId.HandR).magnitude);
            Vector3 handAt = shoulder + Vector3.ClampMagnitude(start - shoulder, reach);
            float most = 0.15f * size, delta = 0.005f * size;
            for (int i = 0; i < steps; i++)
            {
                handAt = shoulder + Vector3.ClampMagnitude(handAt - shoulder, reach);
                Vector3 now = Strike(handAt);
                Vector3 miss = target - now;
                if (miss.magnitude < 0.002f)
                {
                    break;
                }

                // (The striking part's move for a small move of the hand each way: the Jacobian, its
                // columns; solved by Cramer's rule.)
                Vector3 jx = (Strike(handAt + Vector3.right * delta) - now) / delta;
                Vector3 jy = (Strike(handAt + Vector3.up * delta) - now) / delta;
                Vector3 jz = (Strike(handAt + Vector3.forward * delta) - now) / delta;
                float det = Vector3.Dot(jx, Vector3.Cross(jy, jz));
                if (Mathf.Abs(det) < 1e-6f)
                {
                    handAt += miss * 0.5f;
                    continue;
                }

                Vector3 step = new Vector3(Vector3.Dot(miss, Vector3.Cross(jy, jz)), Vector3.Dot(jx, Vector3.Cross(miss, jz)), Vector3.Dot(jx, Vector3.Cross(jy, miss))) / det;
                handAt += Vector3.ClampMagnitude(step, most);
            }

            return shoulder + Vector3.ClampMagnitude(handAt - shoulder, reach);
        }

        /// <summary>A blow's keyed wrist at its time now (the guard's wrist at its start and end).</summary>
        private Quaternion KeyedWrist(MoveState state, Quaternion guardWrist)
        {
            // (Timed as the hand is: the wrist still turning through the striking moment.)
            state.Move.HandPhase(state.T, out int from, out int to, out float share);
            if (from >= 1 && to >= 1)
            {
                // (Keyed as turn, flexion and deviation: splined through the striking moment.)
                return ArmRig.Wrist(anim, true, state.Move.Through(from, to, share, state.Move.WristKey));
            }

            Quaternion Key(int k) => k == 0 ? guardWrist : ArmRig.Wrist(anim, true, state.Move.WristKey(k));
            return Quaternion.Slerp(Key(from), Key(to), share);
        }

        /// <summary>The arm rebuild: where the blow's keys put the hand this step, before the contact's correction and the torso's push (for the reports).</summary>
        public Vector3 HandKeyed { get; private set; }

        /// <summary>How far the torso's clearance moved the hand this step (for the reports).</summary>
        public Vector3 HandCleared { get; private set; }

        /// <summary>The guard's target this step: the grip and the weapon's turn (for the reports).</summary>
        public Vector3 GuardGrip { get; private set; }

        public Quaternion GuardTurn { get; private set; }

        /// <summary>The contact's correction last made (for the reports): its weight, how far it moves the hand, and how far the striking part would miss with the hand at the contact key found again.</summary>
        public float CorrectionWeight { get; private set; }

        public float CorrectionSize { get; private set; }

        public float CorrectionMiss { get; private set; }

        /// <summary>The reports only: the contact's correction left out, to see the keys alone.</summary>
        public static bool DebugNoCorrection;

        /// <summary>The arm rebuild: what the weapon arm's solve last made (for the gizmo, the log and the tests).</summary>
        public ArmSolve LastWeaponArm { get; private set; }

        /// <summary>The arm rebuild: the weapon arm's elbow null last asked (degrees round the arm).</summary>
        public float WeaponArmSwivel { get; private set; }

        /// <summary>The weapon arm's goal: the hand put so the weapon's anchor lands on its target, turned as the weapon is, the elbow toward its null.</summary>
        private ArmGoal WeaponGoal(Transform hand, Transform slot, Vector3 grip, Quaternion rotation, Vector3 anchorLocal, float elbowSwivel, WeaponDefinition weapon)
        {
            Quaternion handTurn = rotation * Quaternion.Inverse(slot.localRotation);
            // (The anchor in the hand's frame: the slot's place on the hand, then the anchor along the weapon.)
            Vector3 inHand = Quaternion.Inverse(hand.rotation) * (slot.position - hand.position) + slot.localRotation * anchorLocal;
            return new ArmGoal
            {
                point = grip + rotation * anchorLocal,
                pointInHand = inHand,
                handTurn = handTurn,
                rollAxisInHand = slot.localRotation * Vector3.forward,
                rollAllowance = weapon != null ? weapon.gripRoll : 0f,
                elbowSwivel = elbowSwivel,
                clearance = ArmClearance * body.SizeFactor,
                wristEase = GuardWristEase,
            };
        }

        /// <summary>How much of the wrist's bend a guard uses (a share of its limits): held near straight, the weapon's angle giving instead.</summary>
        public const float GuardWristEase = EasyBend / ArmRig.FlexLimit;

        /// <summary>How far the arm keeps from its own torso (metres, medium size).</summary>
        public const float ArmClearance = 0.02f;

        /// <summary>The lowest of a weapon's grip, striking part and tip, held at a grip and turned so (world height).</summary>
        private static float WeaponLowest(WeaponDefinition weapon, float scale, Vector3 grip, Quaternion rotation)
        {
            float low = grip.y;
            low = Mathf.Min(low, (grip + rotation * (weapon.tip * scale)).y);
            low = Mathf.Min(low, (grip + rotation * (weapon.head * scale)).y);
            return low;
        }

        /// <summary>
        /// Keeps a weapon's target out of the ground: tipped up about its grip until its lowest point
        /// clears, and the grip raised if that isn't enough. A function of the target alone, so a
        /// smoothly moving target stays smooth (no memory, no rate limit).
        /// </summary>
        private void KeepTargetOffGround(WeaponDefinition weapon, float scale, ref Vector3 grip, ref Quaternion rotation)
        {
            if (weapon == null || weapon.IsShield)
            {
                return;
            }

            float floor = stance.GroundY + (GroundClamp.Margin + GroundClamp.Soft) * body.SizeFactor;
            Vector3 tip = rotation * (weapon.tip * scale);
            float length = tip.magnitude;
            if (length > 1e-4f && grip.y + tip.y < floor)
            {
                // (In the vertical plane through the weapon: from its slope now to the least that clears.)
                float now = Mathf.Asin(Mathf.Clamp(tip.y / length, -1f, 1f));
                float need = Mathf.Asin(Mathf.Clamp((floor - grip.y) / length, -1f, 1f));
                Vector3 axis = Vector3.Cross(tip, Vector3.up);
                if (axis.sqrMagnitude > 1e-8f && need > now)
                {
                    rotation = Quaternion.AngleAxis((need - now) * Mathf.Rad2Deg, axis.normalized) * rotation;
                }
            }

            float low = WeaponLowest(weapon, scale, grip, rotation);
            if (low < floor)
            {
                grip.y += floor - low;
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
            // Milestone 10 (the arm rebuild's step 5): on the arm rig, as the weapon arm is: the elbow a
            // hinge, the wrist inside the limits PhysX holds, the elbow moved round its circle (from
            // that null, never jumping) where the wrist can't take the shield's turn, the shield rolled
            // about its face last.
            Vector3 pole = facing * new Vector3(-0.35f, -1f, -0.35f);
            PlaceShieldArm(grip, rotation, slot, pole);

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
                    PlaceShieldArm(grip, rotation, slot, pole);
                }

                ShieldCenter = grip + rotation * (shield.faceCenter * scale);
                ShieldClearance = Mathf.Min(ShieldClearance, ShieldGap(ShieldCenter, rotation, radius, ShieldArm) + ArmShieldAllowance * body.SizeFactor);
            }
        }

        /// <summary>The shield arm on the arm rig: the shield's grip to <paramref name="grip"/>, turned to <paramref name="rotation"/>, the elbow's null toward <paramref name="pole"/>.</summary>
        private void PlaceShieldArm(Vector3 grip, Quaternion rotation, Transform slot, Vector3 pole)
        {
            var hand = anim.Part(PartId.HandL);
            Vector3 wrist = Reachable(PartId.UpperArmL, PartId.ForearmL, PartId.HandL, HandFor(grip, rotation, slot));
            LastShieldArm = ArmRig.Solve(anim, false, new ArmGoal
            {
                point = grip + (wrist - HandFor(grip, rotation, slot)),
                pointInHand = Quaternion.Inverse(hand.rotation) * (slot.position - hand.position),
                handTurn = rotation * Quaternion.Inverse(slot.localRotation),
                rollAxisInHand = slot.localRotation * Vector3.forward,
                rollAllowance = ShieldRoll,
                elbowSwivel = ArmRig.SwivelToward(anim, false, wrist, pole),
                turnWeight = 1f,
                nearNull = true,
            });
        }

        /// <summary>What the arm rig made of the shield arm this step.</summary>
        public ArmSolve LastShieldArm { get; private set; }

        /// <summary>How far a shield may roll about its face to suit the wrist (degrees each way): a round shield looks the same.</summary>
        public const float ShieldRoll = 30f;

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

        /// <summary>The weapon wrist's easy bend in a guard (degrees off the forearm's line): held near straight, the weapon's angle giving instead.</summary>
        public const float EasyBend = 22f;

        /// <summary>How far a weapon's blade leans from square across the fist toward the forearm's line (degrees; the diagonal grip).</summary>
        public const float GripLean = 20f;

        /// <summary>How far the weapon arm and its weapon keep from its own torso and head when it can (metres; the measure counts the shoulder's allowance).</summary>
        public const float ArmBodyMargin = 0.045f;

        /// <summary>How far a guard is held out at most to keep its weapon off its own head and body (metres at medium size), and how quickly it goes out and back (Hz, critically damped).</summary>
        public const float GuardOutMost = 0.5f, GuardOutRate = 1.5f;
        private float guardOut, guardOutSpeed;

        /// <summary>Which way a guard is moved to keep its weapon off its own head (the facing's frame): forward and down.</summary>
        public static readonly Vector3 GuardOutWay = new Vector3(0f, -0.6f, 0.8f);
        private int composeCount;

        /// <summary>How much further a thrust's grip leans the blade toward the forearm's line (degrees).</summary>
        public const float ThrustLean = 35f;

        // ---- Hinges --------------------------------------------------------------------------------

        private bool composingDry, composingArmPlays;
        private float composeDt;

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
            PoseBareGuard(facing);
            PoseJab(facing);
        }

        /// <summary>
        /// Milestone 10: the jab. The knuckles go from the guard (the fist where the guard holds it
        /// now, turning with the body) a little back, then straight to the aim point and a little
        /// through it, and straight back, through the move's keys at its pace; the fist turns over
        /// palm down on the way out, knuckles leading. The arm on the rig, the elbow down behind the
        /// fist.
        /// </summary>
        private void PoseJab(Quaternion facing)
        {
            var move = Move;
            if (move == null || move.Move.hand != MoveHand.Off || move.Weapon == null || move.Weapon.kind != WeaponKind.Fist)
            {
                return;
            }

            PoseOffHandBlow(move, facing, OffHandPath.KnuckleLocal(anim, body.SizeFactor), JabElbowWay);
        }

        /// <summary>
        /// Milestone 10: the shield bash, as the jab: the shield's face from its guard a little back,
        /// then straight to the aim point and through, and back; the face turned to meet the way it
        /// goes. The shield's own place for the block and the eyes follows it.
        /// </summary>
        private void PoseBash(Loadout loadout, Quaternion facing)
        {
            var move = Move;
            if (move == null || move.Move.hand != MoveHand.Off || move.Weapon == null || !move.Weapon.IsShield || loadout?.Off == null)
            {
                return;
            }

            var shield = loadout.Off;
            if (PoseOffHandBlow(move, facing, OffHandPath.StrikeLocal(anim, shield, loadout.Scale, body.SizeFactor), BashElbowWay))
            {
                var slot = anim.SlotWeaponL;
                ShieldCenter = slot.position + slot.rotation * (shield.faceCenter * loadout.Scale);
                ShieldFace = slot.rotation * Vector3.forward;
            }
        }

        /// <summary>Which way a shield bash's elbow points (the chest's frame): down and out, behind the shield.</summary>
        public static readonly Vector3 BashElbowWay = new Vector3(-0.35f, -1f, -0.35f);

        /// <summary>
        /// An off-hand blow's arm (milestone 10): the striking part (<paramref name="strikeLocal"/>, in
        /// the hand's frame) from where the guard holds it a little back, then straight to the aim
        /// point and a little through it, and straight back, through the move's keys at its pace; the
        /// hand turned to lead with it (a fist palm down, a shield face first). On the arm rig. False
        /// if there is no blow to pose.
        /// </summary>
        private bool PoseOffHandBlow(MoveState move, Quaternion facing, Vector3 strikeLocal, Vector3 elbowWay)
        {

            float blend = move.Blend;
            if (blend <= 0f)
            {
                return false;
            }

            float size = body.SizeFactor;
            var hand = anim.Part(PartId.HandL);
            var wristFrame = ArmRig.WristFrame(anim, false);
            Vector3 alongL = wristFrame * Vector3.right, thumbL = wristFrame * Vector3.up;
            Vector3 knuckleLocal = strikeLocal;
            Vector3 guardKnuckles = hand.position + hand.rotation * knuckleLocal;
            Quaternion guardTurn = hand.rotation;
            Vector3 punch = move.Aim - guardKnuckles;
            punch = punch.sqrMagnitude > 1e-6f ? punch.normalized : facing * Vector3.forward;
            var def = move.Move;
            Vector3 Key(int k) => k == 0 ? guardKnuckles
                : k == 1 ? guardKnuckles - punch * (def.drawBack * size)
                : k == 2 ? move.Aim
                : move.Aim + punch * (def.through * size);
            def.HandPhase(move.T, out int from, out int to, out float share);
            // (A stopped jab's fist springs back along the push it met: the off arm's knock.)
            Vector3 at = Vector3.Lerp(guardKnuckles, def.Through(from, to, share, Key), blend) + KnockL;
            float out_ = Mathf.Clamp01(def.Through(from, to, share, k => k == 0 ? Vector3.zero : Vector3.one).x) * blend;

            // Palm down, knuckles leading: the forearm's line along the punch, the thumb in across the body.
            Vector3 thumbWant = Vector3.Cross(Vector3.up, punch);
            if (thumbWant.sqrMagnitude < 1e-6f)
            {
                thumbWant = facing * Vector3.right;
            }

            Quaternion jabTurn;
            if (move.Weapon.IsShield && anim.SlotWeaponL != null)
            {
                // (A shield: its face turned from the guard's to meet the way it goes.)
                Vector3 faceNow = guardTurn * (anim.SlotWeaponL.localRotation * Vector3.forward);
                jabTurn = Quaternion.FromToRotation(faceNow, punch) * guardTurn;
            }
            else
            {
                jabTurn = Quaternion.LookRotation(punch, thumbWant) * Quaternion.Inverse(Quaternion.LookRotation(alongL, thumbL));
            }
            var chestPart = anim.Part(PartId.Chest);
            Vector3 chestForward = Vector3.ProjectOnPlane(chestPart.rotation * Vector3.forward, Vector3.up);
            var chestTurn = Quaternion.LookRotation(chestForward.sqrMagnitude > 1e-6f ? chestForward : facing * Vector3.forward, Vector3.up);
            LastOffArm = ArmRig.Solve(anim, false, new ArmGoal
            {
                point = at,
                pointInHand = knuckleLocal,
                handTurn = Quaternion.Slerp(guardTurn, jabTurn, out_),
                turnWeight = 1f,
                elbowSwivel = ArmRig.SwivelToward(anim, false, at, chestTurn * elbowWay),
                nearNull = true,
            });
            StrikePoint = hand.position + hand.rotation * knuckleLocal;
            AimError = Vector3.Distance(StrikePoint, move.Aim);
            return true;
        }

        /// <summary>Whether a dry pose of a weapon blow leaves the off arm unposed (the planner's tries; the reports' poses set it false to see the whole body).</summary>
        public static bool SkipOffArmWhenDry = true;

        /// <summary>Which way a jab's elbow points (the chest's frame): down behind the fist, a little out.</summary>
        public static readonly Vector3 JabElbowWay = new Vector3(-0.3f, -1f, 0f);

        private void PoseBareGuard(Quaternion facing)
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
            // (The elbow down under the fist, a little out: tucked in front of the ribs.) A fist on a
            // straight wrist, in line with the forearm (as the plain guard holds it). Milestone 10 (the
            // arm rebuild's step 5): on the arm rig, inside the joints' limits by construction (a
            // boxer's guard beside the head took the elbow up to 28 degrees past them).
            LastOffArm = ArmRig.Solve(anim, false, new ArmGoal
            {
                point = target,
                wristGiven = true,
                wristLocal = anim.RestLocalRotation(PartId.HandL) * Quaternion.Euler(-10f, 0f, 0f),
                elbowSwivel = ArmRig.SwivelToward(anim, false, target, chestTurn * new Vector3(-0.35f, -1f, 0f)),
                nearNull = true,
            });
        }

        /// <summary>What the arm rig made of the bare off arm this step.</summary>
        public ArmSolve LastOffArm { get; private set; }

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
