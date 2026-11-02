using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Authority
{
    /// <summary>How much say physics has over a part.</summary>
    public enum AuthorityLevel
    {
        /// <summary>Moved exactly to the pose each step; pushes others, isn't pushed.</summary>
        Kinematic,
        /// <summary>Dynamic; its velocity is set toward the pose each step, by the pin weight.</summary>
        Pinned,
        /// <summary>Dynamic, barely held: swings with what hits it, drifts back.</summary>
        Loose,
        /// <summary>Dynamic, not held at all.</summary>
        Ragdoll,
    }

    /// <summary>Parts that share an authority level and pin weight.</summary>
    public enum BodyRegion
    {
        Legs,      // pelvis, thighs, shins, feet
        Torso,     // spine, chest
        Head,
        ArmL,
        ArmR,
    }

    /// <summary>
    /// Makes the physical body follow the animation rig as far as each region's authority allows.
    /// Kinematic parts are moved to the pose. Pinned parts are held by a servo (milestone 9c): each
    /// step the part moves with its pose and a spring and damper close what is left between them,
    /// critically damped, so a part pushed off its pose comes back in one smooth return and never
    /// rings. The pin weight sets the servo's stiffness (<see cref="HoldFrequency"/>): 1 arrives
    /// exactly, lower is softer and slower. Loose parts are barely held, ragdoll parts not at all.
    /// </summary>
    public sealed class AuthorityController
    {
        public const float LoosePin = 0.04f;

        private readonly UnitRig rig;
        private readonly AnimRig anim;
        private readonly AuthorityLevel[] levels = new AuthorityLevel[5];
        private readonly float[] pins = new float[5];
        private readonly bool[] keepLimits = new bool[5];
        // Milestone 9b: each joint's limits: free while the pose holds it, the anatomical limits for a
        // weakly held body, the ragdoll's on the ground (eased in from where the joint is).
        private enum LimitMode { Free, Anatomical, Ragdoll }
        private readonly LimitMode?[] limitMode = new LimitMode?[PartInfo.Count];
        private readonly JointLimitSpec[] easeFrom = new JointLimitSpec[PartInfo.Count];
        private readonly float[] easeSince = new float[PartInfo.Count];
        private readonly bool[] hingeLocked = new bool[PartInfo.Count];
        // Milestone 9c: where each part's pose was last step (its pose's own motion is followed, so
        // the servo only has to close what is left), and whether that is known.
        private readonly Vector3[] lastTarget = new Vector3[PartInfo.Count];
        private readonly Quaternion[] lastTargetRotation = new Quaternion[PartInfo.Count];
        private readonly bool[] followed = new bool[PartInfo.Count];
        private readonly bool[] kinematicKnown = new bool[PartInfo.Count];
        private float clock;

        /// <summary>How long a ragdoll's joints take to close from where they are to their limits (seconds).</summary>
        public const float LimitEase = 0.3f;

        /// <summary>A hinge's side limit is locked once it has eased this close (degrees); until then it stays at least this wide (PhysX can't hold narrow side limits).</summary>
        public const float LockWithin = 8f;

        /// <summary>A body on the ground keeps a little muscle tone: each joint is damped (no spring), the damper per kilogram of the part.</summary>
        public float GroundTone { get; set; } = 0f;

        public AuthorityController(UnitRig rig, AnimRig anim)
        {
            this.rig = rig;
            this.anim = anim;
        }

        public static BodyRegion RegionOf(PartId id)
        {
            switch (id)
            {
                case PartId.Spine:
                case PartId.Chest:
                    return BodyRegion.Torso;
                case PartId.Head:
                    return BodyRegion.Head;
                case PartId.UpperArmL:
                case PartId.ForearmL:
                case PartId.HandL:
                    return BodyRegion.ArmL;
                case PartId.UpperArmR:
                case PartId.ForearmR:
                case PartId.HandR:
                    return BodyRegion.ArmR;
                default:
                    return BodyRegion.Legs;
            }
        }

        /// <param name="limits">Keep the joints' anatomical limits on even while pinned (a weakly held body getting up).</param>
        public void Set(BodyRegion region, AuthorityLevel level, float pin, bool limits = false)
        {
            levels[(int)region] = level;
            pins[(int)region] = Mathf.Clamp01(pin);
            keepLimits[(int)region] = limits;
        }

        public AuthorityLevel Level(BodyRegion region) => levels[(int)region];

        public float Pin(BodyRegion region) => pins[(int)region];

        /// <summary>Drives every part toward its pose for the coming physics step.</summary>
        public void Apply(float dt)
        {
            if (dt <= 0f)
            {
                return;
            }

            clock += dt;
            foreach (var id in PartInfo.All)
            {
                if (rig.IsSevered(id))
                {
                    PhysicsBattler.Units.GroundContacts.SetKinematic(rig.Part(id).partCollider, false);
                    continue;   // (cut off: physics alone has it now)
                }

                var part = rig.Part(id);
                var body = part.body;
                var target = anim.Part(id);
                var region = RegionOf(id);
                var level = levels[(int)region];
                bool kinematic = level == AuthorityLevel.Kinematic;
                SetJointLimits(part, level == AuthorityLevel.Loose || level == AuthorityLevel.Ragdoll ? LimitMode.Ragdoll
                    : level == AuthorityLevel.Pinned && keepLimits[(int)region] ? LimitMode.Anatomical : LimitMode.Free);
                if (body.isKinematic != kinematic)
                {
                    body.isKinematic = kinematic;
                }

                // (Kept in step every step: other code frees a body now and then.)
                PhysicsBattler.Units.GroundContacts.SetKinematic(part.partCollider, kinematic);

                int i = (int)id;
                float pin = level == AuthorityLevel.Pinned ? pins[(int)region]
                    : level == AuthorityLevel.Loose ? LoosePin : 0f;
                if (kinematic || pin <= 0f)
                {
                    if (kinematic)
                    {
                        // Milestone 10: a kinematic part goes where its pose is, but no faster than a body
                        // part moves (KinematicMostSpeed): to PhysX a kinematic part is infinitely heavy,
                        // and a pose that jumps would slam whatever it meets at the jump's speed (the
                        // playtest: a hand moved 1.3 m in a step and launched a body at 100 m/s). A pose
                        // that moved further than a teleport is placed at once.
                        Vector3 to = target.position;
                        Quaternion turn = target.rotation;
                        if (kinematicKnown[i])
                        {
                            Vector3 move = to - body.position;
                            float most = KinematicMostSpeed * dt;
                            if (move.sqrMagnitude > most * most && move.sqrMagnitude < KinematicTeleport * KinematicTeleport)
                            {
                                to = body.position + move.normalized * most;
                            }

                            turn = Quaternion.RotateTowards(body.rotation, turn, KinematicMostTurn * dt);
                        }

                        body.MovePosition(to);
                        body.MoveRotation(turn);
                    }

                    kinematicKnown[i] = kinematic;
                    followed[i] = false;
                    continue;
                }

                kinematicKnown[i] = false;
                // The pose's own motion over the last step (none known after a jump, or when the
                // part wasn't held last step).
                Vector3 poseVelocity = Vector3.zero, poseSpin = Vector3.zero;
                if (followed[i])
                {
                    Vector3 moved = target.position - lastTarget[i];
                    if (moved.sqrMagnitude < PoseJump * PoseJump && Quaternion.Angle(lastTargetRotation[i], target.rotation) < PoseTurnJump)
                    {
                        poseVelocity = moved / dt;
                        poseSpin = JointMath.RotationError(lastTargetRotation[i], target.rotation) / dt;
                    }
                }

                lastTarget[i] = target.position;
                lastTargetRotation[i] = target.rotation;
                followed[i] = true;
                Vector3 gravity = body.useGravity ? UnityEngine.Physics.gravity : Vector3.zero;
                // (Milestone 10: never faster than a body part moves, whatever the pose did.)
                body.linearVelocity = Vector3.ClampMagnitude(PinVelocity(body.position, target.position, body.linearVelocity, pin, dt, gravity, poseVelocity), KinematicMostSpeed);
                body.angularVelocity = PinAngularVelocity(body.rotation, target.rotation, body.angularVelocity, pin, dt, poseSpin);
            }
        }

        /// <summary>
        /// Milestone 10: the fastest a kinematic part moves (m/s) and turns (degrees a second) toward its
        /// pose; a pose further than <see cref="KinematicTeleport"/> metres off is a placing, not a move.
        /// (A fast swing's hand moves about 8 m/s.)
        /// </summary>
        public const float KinematicMostSpeed = 15f, KinematicMostTurn = 1800f, KinematicTeleport = 2.5f;

        /// <summary>A pose that moves further than this in one step has jumped (placed): its motion isn't followed (metres), nor a turn of more than <see cref="PoseTurnJump"/> degrees.</summary>
        public const float PoseJump = 0.5f, PoseTurnJump = 120f;

        /// <summary>The most turn the servo closes as if it were this far (radians, 120 degrees).</summary>
        public const float MaxTurnError = 2.1f;

        /// <summary>The servo's damping: critical (1) never overshoots.</summary>
        public const float HoldDamping = 1f;

        /// <summary>
        /// The servo's stiffness for a pin weight, as the frequency it would swing at undamped (Hz):
        /// 0.7 (the arms) about 7 Hz, 0.3 about 2.3 Hz, a loose part's 0.04 about 0.2 Hz.
        /// </summary>
        public static float HoldFrequency(float pin) => Mathf.Min(12f, 11f * Mathf.Pow(Mathf.Clamp01(pin), 1.3f));

        /// <summary>
        /// The velocity for the coming step (milestone 9c). A pin of 1 lands on the target exactly; 0
        /// leaves the velocity alone. In between, the part moves with its pose (the pose's velocity
        /// over the last step) and a critically damped spring closes what is left, solved implicitly
        /// so it is stable at any stiffness and never rings. Gravity is taken out in advance (the
        /// solver adds it during the step).
        /// </summary>
        public static Vector3 PinVelocity(Vector3 position, Vector3 target, Vector3 velocity, float pin, float dt, Vector3 gravity = default, Vector3 poseVelocity = default)
        {
            pin = Mathf.Clamp01(pin);
            if (pin >= 0.999f)
            {
                return (target - position) / dt - gravity * dt;
            }

            if (pin <= 0f)
            {
                return velocity;
            }

            // (What is left once the pose's own motion this step is taken out.)
            Vector3 error = target - poseVelocity * dt - position;
            return poseVelocity + Servo(velocity - poseVelocity, error, HoldFrequency(pin), dt) - gravity * dt;
        }

        /// <summary>The angular velocity for the coming step: as <see cref="PinVelocity"/>, for the part's turn.</summary>
        public static Vector3 PinAngularVelocity(Quaternion rotation, Quaternion target, Vector3 angularVelocity, float pin, float dt, Vector3 poseSpin = default)
        {
            pin = Mathf.Clamp01(pin);
            if (pin >= 0.999f)
            {
                return JointMath.RotationError(rotation, target) / dt;
            }

            if (pin <= 0f)
            {
                return angularVelocity;
            }

            float spun = poseSpin.magnitude * dt;
            Quaternion before = spun > 1e-6f ? Quaternion.AngleAxis(-spun * Mathf.Rad2Deg, poseSpin / poseSpin.magnitude) * target : target;
            // (Far off, it turns back no faster than from 120 degrees: never a flip.)
            Vector3 error = Vector3.ClampMagnitude(JointMath.RotationError(rotation, before), MaxTurnError);
            return poseSpin + Servo(angularVelocity - poseSpin, error, HoldFrequency(pin), dt);
        }

        /// <summary>
        /// One step of a critically damped spring (backward Euler): the relative velocity after the
        /// step, for a relative velocity and what is left to close. It never overshoots and is
        /// stable at any stiffness.
        /// </summary>
        public static Vector3 Servo(Vector3 relative, Vector3 error, float hz, float dt)
        {
            float w = 2f * Mathf.PI * Mathf.Max(0f, hz);
            float hw = dt * w;
            return (relative + error * (dt * w * w)) / (1f + 2f * HoldDamping * hw + hw * hw);
        }

        /// <summary>
        /// A joint's limits only hold while physics owns the part (loose, ragdoll) or a weakly held
        /// body is getting up. While the pose owns it (kinematic, pinned), the joint swings free: a
        /// limit would only fight the pose and leave the visible limb behind it.
        ///
        /// Milestone 9b: on the ground the limits don't snap on. Each starts where the joint is (if it
        /// is outside) and closes to the ragdoll's limits over <see cref="LimitEase"/>, so PhysX brings
        /// the limb back smoothly. The hinges (knees; elbows sideways) end locked: PhysX can't hold a
        /// narrow side limit. And the joints are damped a little, a body's own muscle tone.
        /// </summary>
        private void SetJointLimits(BodyPart part, LimitMode mode)
        {
            var joint = part.joint;
            int i = (int)part.id;
            if (joint == null)
            {
                return;
            }

            if (limitMode[i] != mode)
            {
                limitMode[i] = mode;
                hingeLocked[i] = false;
                switch (mode)
                {
                    case LimitMode.Free:
                        SetMotion(joint, ConfigurableJointMotion.Free, ConfigurableJointMotion.Free, ConfigurableJointMotion.Free);
                        SetTone(joint, part, 0f);
                        break;
                    case LimitMode.Anatomical:
                        JointMath.ApplyLimits(joint, Margined(JointLimitTable.For(part.id), AnatomicalMargin));
                        SetMotion(joint, ConfigurableJointMotion.Limited, ConfigurableJointMotion.Limited, ConfigurableJointMotion.Limited);
                        SetTone(joint, part, 0f);
                        break;
                    default:
                        // Wide enough to take the joint where it is, then closing in.
                        easeFrom[i] = Widened(JointLimitTable.ForRagdoll(part.id), CurrentAngles(part));
                        easeSince[i] = clock;
                        JointMath.ApplyLimits(joint, easeFrom[i]);
                        SetMotion(joint, ConfigurableJointMotion.Limited, ConfigurableJointMotion.Limited, ConfigurableJointMotion.Limited);
                        SetTone(joint, part, GroundTone);
                        break;
                }
            }

            if (mode != LimitMode.Ragdoll || hingeLocked[i])
            {
                return;
            }

            var goal = JointLimitTable.ForRagdoll(part.id);
            float t = Mathf.Clamp01((clock - easeSince[i]) / LimitEase);
            t = t * t * (3f - 2f * t);
            var now = new JointLimitSpec(
                Mathf.Lerp(easeFrom[i].pitchMin, goal.pitchMin, t), Mathf.Lerp(easeFrom[i].pitchMax, goal.pitchMax, t),
                Mathf.Max(Mathf.Lerp(easeFrom[i].twist, goal.twist, t), goal.twist > 0f ? goal.twist : LockWithin),
                Mathf.Max(Mathf.Lerp(easeFrom[i].swing, goal.swing, t), goal.swing > 0f ? goal.swing : LockWithin));
            if (t < 1f || goal.twist <= 0f || goal.swing <= 0f)
            {
                JointMath.ApplyLimits(joint, now);
            }

            // Eased in: a hinge's side axes lock once the joint is close to straight on them.
            if (t >= 1f && (goal.twist <= 0f || goal.swing <= 0f))
            {
                var a = CurrentAngles(part);
                bool yOk = goal.twist > 0f || Mathf.Abs(a.y) <= LockWithin;
                bool zOk = goal.swing > 0f || Mathf.Abs(a.z) <= LockWithin;
                if (yOk && zOk)
                {
                    SetMotion(joint, ConfigurableJointMotion.Limited,
                        goal.twist > 0f ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Locked,
                        goal.swing > 0f ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Locked);
                    hingeLocked[i] = true;
                }
            }
            else if (t >= 1f)
            {
                hingeLocked[i] = true;   // (nothing left to ease)
            }
        }

        private static void SetMotion(ConfigurableJoint joint, ConfigurableJointMotion x, ConfigurableJointMotion y, ConfigurableJointMotion z)
        {
            joint.angularXMotion = x;
            joint.angularYMotion = y;
            joint.angularZMotion = z;
        }

        /// <summary>
        /// A damper on each joint (no spring): limbs fold and settle rather than flop. Per axis, not
        /// slerp (PhysX drops a slerp drive once an axis is locked). Off: the drives as built (slerp, empty).
        /// </summary>
        private static void SetTone(ConfigurableJoint joint, BodyPart part, float perKg)
        {
            if (perKg <= 0f)
            {
                joint.rotationDriveMode = RotationDriveMode.Slerp;
                joint.angularXDrive = new JointDrive { maximumForce = float.MaxValue, useAcceleration = true };
                joint.angularYZDrive = new JointDrive { maximumForce = float.MaxValue, useAcceleration = true };
                return;
            }

            // (Acceleration drives: the damper is a rate, the same feel whatever the part weighs; heavier
            // parts damped a little more, as the larger muscles round them.)
            float damper = perKg * Mathf.Sqrt(Mathf.Max(0.1f, part.body.mass));
            var drive = new JointDrive { positionSpring = 0f, positionDamper = damper, maximumForce = float.MaxValue, useAcceleration = true };
            joint.rotationDriveMode = RotationDriveMode.XYAndZ;
            joint.angularXDrive = drive;
            joint.angularYZDrive = drive;
            joint.targetAngularVelocity = Vector3.zero;
        }

        /// <summary>The joint's turn now (physical), as PhysX splits it: x the bend, y and z the swing.</summary>
        private Vector3 CurrentAngles(BodyPart part)
        {
            var parent = rig.Part(PartInfo.Parent(part.id));
            Quaternion local = Quaternion.Inverse(part.restLocalRotation) * (Quaternion.Inverse(parent.transform.rotation) * part.transform.rotation);
            JointMath.PastLimits(local, part.limits, out float pitch, out float y, out float z);
            return new Vector3(pitch, y, z);
        }

        /// <summary>A held joint's physical limits are this much wider than the pose's own (degrees), so they never fight the pose.</summary>
        public const float AnatomicalMargin = 5f;

        /// <summary>Limits widened by a margin on every side (at most 177 degrees about Y and Z).</summary>
        public static JointLimitSpec Margined(JointLimitSpec spec, float margin) => new JointLimitSpec(
            spec.pitchMin - margin, spec.pitchMax + margin, Mathf.Min(177f, spec.twist + margin), Mathf.Min(177f, spec.swing + margin));

        /// <summary>Limits wide enough to hold a joint where it is now (a little room past it).</summary>
        public static JointLimitSpec Widened(JointLimitSpec spec, Vector3 angles)
        {
            const float room = 2f;
            return new JointLimitSpec(
                Mathf.Min(spec.pitchMin, angles.x - room), Mathf.Max(spec.pitchMax, angles.x + room),
                Mathf.Min(177f, Mathf.Max(Mathf.Max(spec.twist, LockWithin), Mathf.Abs(angles.y) + room)),
                Mathf.Min(177f, Mathf.Max(Mathf.Max(spec.swing, LockWithin), Mathf.Abs(angles.z) + room)));
        }

        /// <summary>Distance and angle between a part and its pose (for the log).</summary>
        public void Error(PartId id, out float distance, out float angle)
        {
            if (rig.IsSevered(id))
            {
                distance = angle = 0f;
                return;
            }

            var body = rig.Part(id).body;
            var target = anim.Part(id);
            distance = Vector3.Distance(body.position, target.position);
            angle = Quaternion.Angle(body.rotation, target.rotation);
        }
    }
}
