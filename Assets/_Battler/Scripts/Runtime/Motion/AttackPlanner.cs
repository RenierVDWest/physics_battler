using System;
using PhysicsBattler.Diagnostics;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// Runs one swing and produces the <see cref="ActionPose"/> for each step. The swing is planned for
    /// the target's distance and height, and moves at the speed the wielder's strength can move the
    /// weapon's real weight (<see cref="SwingDynamics"/>):
    ///
    /// 1. Wind-up: the weapon goes from the carry to the wound-up position out at the side, accelerating
    ///    and then braking so it arrives at rest.
    /// 2. Load: hold there until the real weapon has arrived and stopped swinging (or a time limit),
    ///    then commit: the arc is frozen in world space.
    /// 3. Strike: drive it at the target, never faster than the follow-through can stop.
    /// 4. Follow: brake it to a stop (a solid hit takes some of its speed), and wait for the real
    ///    weapon to slow down.
    /// 5. Recover: bring it back to the carry.
    ///
    /// A target that steps away after the commit is missed, and anything that steps into the arc is hit.
    /// </summary>
    public sealed class AttackPlanner
    {
        public enum Phase
        {
            Idle,
            WindUp,
            Load,
            Strike,
            Follow,
            Recover,
        }

        private readonly UnitController self;
        private AttackDefinition definition;
        private float phaseTime;
        private float entryTheta;
        private bool landedHit;
        private Vector3 carryGripLocal;
        private Quaternion carryHandLocal = Quaternion.identity;
        private bool hasCarry;

        // The swing's state: path progress for the wind-up and recover, angle and speed for the strike.
        private float pathDuration;
        private float omega;
        private float settledFor;
        private float lastClubTheta;
        private bool hasClubTheta;
        private Vector3 followEndGripLocal;
        private Quaternion followEndHandLocal = Quaternion.identity;
        private Quaternion lastHandRotation = Quaternion.identity;
        private Vector3 lastGrip;

        public Phase Current { get; private set; } = Phase.Idle;
        public bool Active => Current != Phase.Idle;
        public bool Committed { get; private set; }
        public float Theta { get; private set; }

        /// <summary>The ghost swing's angular speed (rad/s) in the strike and follow-through.</summary>
        public float Omega => omega;

        public SwingPlan Plan { get; private set; }
        public SwingLoad Load { get; private set; }
        public UnitController Target { get; private set; }
        public AttackDefinition Definition => definition;
        public float PhaseTime => phaseTime;
        public float EntryTheta => entryTheta;
        public bool LandedHit => landedHit;

        /// <summary>True for a short while after an attack is abandoned (its weapon still flying).</summary>
        public float AbandonedTimer { get; private set; }

        public event Action<Phase> PhaseChanged;

        public AttackPlanner(UnitController self)
        {
            this.self = self;
        }

        private bool TwoHanded => self.Weapon != null && self.Weapon.Definition.twoHanded;

        /// <summary>The wielder's swing strength (N·m at the shoulder, body behind it), from its profile; more with both hands.</summary>
        public float Strength
        {
            get
            {
                float strength = self.Profile != null ? self.Profile.strength : 200f;
                return TwoHanded && self.Profile != null ? strength * self.Profile.twoHandStrength : strength;
            }
        }

        /// <summary>
        /// The centre the swing turns about: the weapon-side shoulder for one hand; between the shoulders,
        /// toward the weapon side, for two (both hands must reach the grip all the way round).
        /// </summary>
        private Vector3 SwingCentre(Vector3 weaponShoulder, Vector3 otherShoulder)
        {
            return TwoHanded ? Vector3.Lerp(otherShoulder, weaponShoulder, TwoHandCentre) : weaponShoulder;
        }

        private const float TwoHandCentre = 0.7f;

        /// <summary>Hands moving this far (m) count as a radian of turn when pacing a move.</summary>
        private const float HandTravelRadius = 0.5f;

        /// <summary>How much an impact now counts as a deliberate blow (scales damage).</summary>
        public float Intent
        {
            get
            {
                var d = definition;
                if (d == null)
                {
                    return 0.3f;
                }

                if (AbandonedTimer > 0f)
                {
                    return d.abandonedIntent;
                }

                switch (Current)
                {
                    case Phase.Strike:
                    case Phase.Follow:
                        return d.strikeIntent;
                    case Phase.WindUp:
                    case Phase.Load:
                        return d.windUpIntent;
                    default:
                        return d.recoverIntent;
                }
            }
        }

        public SwingReach Reach
        {
            get
            {
                float scale = self.Scale;
                var weapon = self.Weapon;
                return new SwingReach
                {
                    sweetSpot = weapon != null ? weapon.SweetSpotDistance : 1f,
                    tip = weapon != null ? weapon.TipDistance : 1.2f,
                    minArm = definition.minArm * scale,
                    maxArm = definition.maxArm * scale,
                    maxLean = definition.maxLean * scale,
                };
            }
        }

        /// <summary>
        /// The arm and weapon as a load on the shoulder, swung out at <paramref name="reach"/> (shoulder
        /// to grip), measured from the rigidbodies.
        /// </summary>
        public SwingLoad MeasureLoad(float reach)
        {
            var rig = self.Rig;
            var weapon = self.Weapon;
            var hand = rig.Part(PartId.HandR).body;
            var forearm = rig.Part(PartId.ForearmR).body;
            var upper = rig.Part(PartId.UpperArmR).body;
            float centre = 0f;
            if (weapon != null && weapon.PhysicalPose(out Vector3 grip, out Vector3 direction, out _))
            {
                centre = Mathf.Max(0f, Vector3.Dot(hand.worldCenterOfMass - grip, direction));
            }

            float handDistance = reach + centre;
            float load = Mean(hand) + hand.mass * handDistance * handDistance
                         + Mean(forearm) + forearm.mass * Square(0.6f * reach)
                         + Mean(upper) + upper.mass * Square(0.25f * reach);
            float moment = hand.mass * handDistance + forearm.mass * 0.6f * reach + upper.mass * 0.25f * reach;
            return new SwingLoad
            {
                inertia = load,
                massMoment = moment,
                weaponMass = weapon != null ? weapon.Definition.physicsMassKg : 0f,
                weaponCentre = centre,
            };
        }

        private static float Square(float x) => x * x;

        private static float Mean(Rigidbody body)
        {
            Vector3 tensor = body.inertiaTensor;
            return (tensor.x + tensor.y + tensor.z) / 3f;
        }

        /// <summary>Strength over the torque it takes to hold the weapon out: below 1 it can't be swung at all.</summary>
        public float LiftRatio(in SwingLoad load) => Strength / Mathf.Max(1f, load.HoldTorque);

        public bool TryBegin(UnitController target, AttackDefinition attack, out string reason)
        {
            reason = null;
            if (attack == null)
            {
                reason = "no attack definition";
                return false;
            }

            if (self.Weapon == null)
            {
                reason = "no weapon";
                return false;
            }

            if (target == null || target == self)
            {
                reason = "no target";
                return false;
            }

            if (Active)
            {
                reason = "already attacking";
                return false;
            }

            definition = attack;
            Target = target;
            Committed = false;
            landedHit = false;
            AbandonedTimer = 0f;
            Replan(target.AimPoint);
            if (!Plan.inRange)
            {
                reason = $"out of range ({Plan.distance:0.00} m from the shoulder)";
                Target = null;
                return false;
            }

            Load = MeasureLoad(Plan.contactReach);
            float lift = LiftRatio(Load);
            float minimum = self.Profile != null ? self.Profile.minimumLiftRatio : 1.2f;
            if (lift < minimum)
            {
                reason = $"too heavy: holding the weapon out takes {Load.HoldTorque:0} N·m, strength is {Strength:0} N·m";
                Target = null;
                return false;
            }

            // Start from the ghost's carry pose (where the weapon is meant to be), not from wherever the
            // physical club happens to be: every swing then starts the same way.
            var ghostHand = self.Ghost.Part(PartId.HandR);
            var slot = self.Ghost.SlotWeaponR;
            var root = self.Root;
            carryGripLocal = root.InverseTransformPoint(slot != null ? slot.position : ghostHand.position);
            carryHandLocal = Quaternion.Inverse(root.rotation) * ghostHand.rotation;
            hasCarry = true;
            entryTheta = SwingSolver.AngleOf(Plan, ghostHand.rotation * Vector3.forward);

            // The wind-up: rest to rest from the carry to the wound-up pose. How far that is counts both
            // the turn of the weapon and how far the hands travel (lifting it from the hip to the
            // shoulder is a move in its own right).
            Vector3 windUpDirection = SwingSolver.Direction(Plan, definition.windUpAngle);
            Vector3 windUpGrip = SwingCentre(self.RestShoulderWorld(definition.side), self.RestShoulderWorld(-definition.side))
                                 + windUpDirection * Reach.minArm;
            float turn = Vector3.Angle(ghostHand.rotation * Vector3.forward, windUpDirection) * Mathf.Deg2Rad;
            float travel = Vector3.Distance(root.TransformPoint(carryGripLocal), windUpGrip) / HandTravelRadius;
            float pathAngle = Mathf.Max(turn, travel);
            pathDuration = SwingDynamics.RestToRestTime(pathAngle, SwingDynamics.MoveAcceleration(Strength, definition.windUpEffort, Load));
            Theta = definition.windUpAngle;
            omega = 0f;
            hasClubTheta = false;

            if (PlaytestTrace.Enabled)
            {
                var plan = Plan;
                SwingDynamics.Predict(plan, Load, Strength, definition, out float strikeTime, out float contactOmega, out float stopAngle);
                float tipRadius = plan.contactReach + Reach.tip;
                PlaytestTrace.Note(self, "attack",
                    $"swing at {target.name}: target {plan.distance:0.00} m from the shoulder, contact reach {plan.contactReach:0.00} m, "
                    + $"plane roll {plan.rollDegrees:0}°, crowded ×{plan.crowdedFactor:0.00}. "
                    + $"Load: {Load.inertia:0.0} kg·m² about the shoulder, {Load.HoldTorque:0} N·m to hold out, strength {Strength:0} N·m ({lift:0.0}×). "
                    + $"Expect: wind-up {pathDuration:0.00} s over {pathAngle * Mathf.Rad2Deg:0}°, strike {strikeTime:0.00} s, "
                    + $"contact at {contactOmega:0.0} rad/s ({contactOmega * tipRadius:0.0} m/s at the tip), stopping at θ {stopAngle:0}°");
            }

            SetPhase(Phase.WindUp);
            return true;
        }

        /// <summary>Stops steering the weapon. Its momentum carries on; it still hits, at reduced damage.</summary>
        public void Abandon(string reason)
        {
            if (!Active)
            {
                return;
            }

            if (PlaytestTrace.Enabled)
            {
                PlaytestTrace.Note(self, "attack", $"swing abandoned in {Current} at θ {Theta:0}°: {reason}");
            }

            AbandonedTimer = 0.8f;
            Target = null;
            SetPhase(Phase.Idle);
        }

        /// <summary>The weapon landed a solid hit: the target takes some of its speed; brake from there.</summary>
        public void OnSolidHit()
        {
            if ((Current == Phase.Strike || Current == Phase.Follow) && !landedHit)
            {
                landedHit = true;
                omega *= 1f - Mathf.Clamp01(definition.hitAbsorb);
                if (Current == Phase.Strike)
                {
                    BeginFollow();
                }
            }
        }

        public ActionPose Step(float dt)
        {
            if (AbandonedTimer > 0f)
            {
                AbandonedTimer -= dt;
            }

            if (!Active)
            {
                return ActionPose.None;
            }

            phaseTime += dt;
            if (!Committed && Target != null)
            {
                Replan(PredictTarget());
            }

            Advance(dt);
            if (!Active)
            {
                return ActionPose.None;
            }

            return BuildPose();
        }

        private void Advance(float dt)
        {
            var d = definition;
            var load = Load;
            switch (Current)
            {
                case Phase.WindUp:
                    TrackClub(dt, out _);
                    if (phaseTime >= pathDuration)
                    {
                        settledFor = 0f;
                        SetPhase(Phase.Load);
                    }

                    break;

                case Phase.Load:
                {
                    // The turning point: as soon as the real weapon's pull-back has been stopped (it is no
                    // longer moving back, and it is near the wound-up position) the strike starts. No
                    // waiting for every wobble to die down.
                    float rate = TrackClub(dt, out float error);
                    bool stopped = error <= d.settleAngle && rate >= -d.stopRate;
                    settledFor = stopped ? settledFor + dt : 0f;
                    bool ready = settledFor >= d.settleTime;
                    if (ready || phaseTime >= d.maxSettleTime)
                    {
                        if (PlaytestTrace.Enabled)
                        {
                            float tipSpeed = self.Weapon != null ? self.Weapon.TipSpeed() : 0f;
                            PlaytestTrace.Note(self, "attack", ready
                                ? $"turning point: the pull-back stopped {phaseTime:0.00} s after the wind-up ended (club {error:0}° from the wound-up position, moving {rate:0}°/s, tip {tipSpeed:0.0} m/s)"
                                : $"turning point not reached in {d.maxSettleTime:0.0} s (club {error:0}° off, moving {rate:0}°/s, tip {tipSpeed:0.0} m/s); striking anyway");
                        }

                        Committed = true;
                        if (PlaytestTrace.Enabled && Target != null)
                        {
                            PlaytestTrace.Note(self, "attack",
                                $"committed: arc frozen, aiming at {Fmt(Plan.target)} (target's chest now {Fmt(Target.AimPoint)}, "
                                + $"{Vector3.Distance(Plan.target, Target.AimPoint):0.00} m apart), contact reach {Plan.contactReach:0.00} m");
                        }

                        Theta = d.windUpAngle;
                        omega = 0f;
                        SetPhase(Phase.Strike);
                    }

                    break;
                }

                case Phase.Strike:
                {
                    float brakeAcceleration = Strength * d.brakeEffort / Mathf.Max(0.05f, load.inertia);
                    float maxOmega = SwingDynamics.StoppableSpeed(brakeAcceleration, d.followAngle * Mathf.Deg2Rad);
                    float theta = Theta;
                    SwingDynamics.StepStrike(ref theta, ref omega, Strength * d.strikeEffort, maxOmega, Plan, load, dt);
                    Theta = theta;
                    if (Theta >= 0f || phaseTime > 3f)
                    {
                        BeginFollow();
                    }

                    break;
                }

                case Phase.Follow:
                {
                    float theta = Theta;
                    bool stopped = SwingDynamics.StepBrake(ref theta, ref omega, Strength * d.brakeEffort, Plan, load, dt);
                    Theta = theta;
                    float tipSpeed = self.Weapon != null ? self.Weapon.TipSpeed() : 0f;
                    if ((stopped && tipSpeed < d.settleSpeed * 2f) || phaseTime > 2.5f)
                    {
                        BeginRecover();
                    }

                    break;
                }

                case Phase.Recover:
                    if (phaseTime >= pathDuration)
                    {
                        SetPhase(Phase.Idle);
                    }

                    break;
            }
        }

        private void BeginFollow()
        {
            SetPhase(Phase.Follow);
        }

        private void BeginRecover()
        {
            // Back to the carry from wherever the follow-through stopped, rest to rest.
            var root = self.Root;
            followEndGripLocal = root.InverseTransformPoint(lastGrip);
            followEndHandLocal = Quaternion.Inverse(root.rotation) * lastHandRotation;
            float pathAngle = Mathf.Max(Quaternion.Angle(lastHandRotation, root.rotation * carryHandLocal) * Mathf.Deg2Rad,
                Vector3.Distance(lastGrip, root.TransformPoint(carryGripLocal)) / HandTravelRadius);
            pathDuration = SwingDynamics.RestToRestTime(pathAngle, SwingDynamics.MoveAcceleration(Strength, definition.recoverEffort, Load));
            SetPhase(Phase.Recover);
        }

        /// <summary>
        /// The real weapon's angle on the swing plane: returns how fast it is moving along the swing
        /// (degrees/s, negative = further back), and how far it is from the wound-up direction.
        /// </summary>
        private float TrackClub(float dt, out float error)
        {
            error = 180f;
            var weapon = self.Weapon;
            if (weapon == null || !weapon.PhysicalPose(out _, out Vector3 direction, out _))
            {
                error = 0f;
                return 0f;
            }

            error = Vector3.Angle(direction, SwingSolver.Direction(Plan, definition.windUpAngle));
            float theta = SwingSolver.AngleOf(Plan, direction);
            float rate = hasClubTheta && dt > 0f ? Mathf.DeltaAngle(lastClubTheta, theta) / dt : 0f;
            lastClubTheta = theta;
            hasClubTheta = true;
            return rate;
        }

        private void SetPhase(Phase phase)
        {
            phaseTime = 0f;
            Current = phase;
            if (phase == Phase.Idle)
            {
                Target = null;
                Committed = false;
            }

            PhaseChanged?.Invoke(phase);
        }

        private static string Fmt(Vector3 v) => $"({v.x:0.00}, {v.y:0.00}, {v.z:0.00})";

        /// <summary>Where the target's aim point will be when the swing arrives.</summary>
        private Vector3 PredictTarget()
        {
            float toContact = 0.6f;
            if (Current == Phase.WindUp)
            {
                toContact += Mathf.Max(0f, pathDuration - phaseTime) + definition.settleTime;
            }

            return Target.AimPoint + Target.AimVelocity * Mathf.Min(toContact, 1f);
        }

        private void Replan(Vector3 target)
        {
            var d = definition;
            Vector3 shoulder = SwingCentre(self.RestShoulderWorld(d.side), self.RestShoulderWorld(-d.side));
            var reach = Reach;
            float roll = SwingSolver.RollForClearance(shoulder, target, reach, d.planeRoll, d.side,
                d.windUpAngle, self.GroundY, d.groundClearance * self.Scale);
            Plan = SwingSolver.Plan(shoulder, target, reach, roll, d.side);
        }

        /// <summary>Progress of the wind-up or recover path (0..1), accelerating then braking.</summary>
        private float PathProgress() => SwingDynamics.RestToRest(phaseTime, pathDuration);

        /// <summary>How much the body (twist, lean, crouch) takes part: eases in and out.</summary>
        private float BodyWeight()
        {
            switch (Current)
            {
                case Phase.WindUp: return Ease.OutCubic(Mathf.Clamp01(PathProgress() / 0.35f));
                case Phase.Recover: return 1f - PathProgress();
                default: return 1f;
            }
        }

        /// <summary>The arm follows its IK target all through, handing over to the plain carry pose only at the very end.</summary>
        private float ArmWeight()
        {
            if (Current != Phase.Recover)
            {
                return 1f;
            }

            return 1f - Ease.InOutCubic(Mathf.Clamp01((PathProgress() - 0.85f) / 0.15f));
        }

        private ActionPose BuildPose()
        {
            var d = definition;
            float scale = self.Scale;
            float weight = BodyWeight();
            float armWeight = ArmWeight();
            var plan = Plan;
            var root = self.Root;

            // Coil: 1 fully wound back, 0 at contact, -0.7 unwound past it.
            float coil;
            switch (Current)
            {
                case Phase.WindUp: coil = PathProgress(); break;
                case Phase.Load: coil = 1f; break;
                case Phase.Recover: coil = -0.7f * (1f - PathProgress()); break;
                default:
                    coil = Theta < 0f
                        ? Mathf.Clamp01(-Theta / Mathf.Max(1f, -d.windUpAngle))
                        : -0.7f * Mathf.Clamp01(Theta / Mathf.Max(1f, d.followAngle));
                    break;
            }

            float twist = coil * d.twistDegrees * d.side * weight;
            float strikeLean = Current == Phase.Strike || Current == Phase.Follow ? plan.extraLean : 0f;
            float lean = coil > 0f ? -d.leanBack * coil * scale : d.leanForward * (-coil / 0.7f) * scale;

            var pose = new ActionPose
            {
                active = true,
                pelvisOffset = new Vector3(0f, -d.crouch * scale * weight, (lean + strikeLean) * weight),
                pelvisYaw = twist * 0.25f,
                spineYaw = twist * 0.35f,
                chestYaw = twist * 0.4f,
                spinePitch = 6f * weight * Mathf.Max(0f, -coil),
                weaponArmWeight = armWeight,
                freeArmPitch = -25f * coil * weight,
                freeArmOut = 15f * weight,
                hasLookTarget = true,
                lookTarget = plan.target,
            };

            // The arc: the grip on a circle about the swing's centre (the shoulder, or between the
            // shoulders for two hands), the weapon pointing along it.
            Vector3 shoulder = SwingCentre(self.Ghost.Part(PartId.UpperArmR).position, self.Ghost.Part(PartId.UpperArmL).position);
            Vector3 direction = SwingSolver.Direction(plan, Theta);
            var reach = Reach;
            float arm = Current == Phase.WindUp || Current == Phase.Load
                ? Mathf.Lerp(plan.contactReach, (reach.minArm + reach.maxArm) * 0.5f, Mathf.Clamp01(coil))
                : plan.contactReach;
            float groundLimit = SwingSolver.GroundLimitedReach(shoulder, direction, reach.tip, self.GroundY,
                d.groundClearance * scale);
            arm = Mathf.Max(reach.minArm, Mathf.Min(arm, groundLimit));
            Vector3 grip = shoulder + direction * arm;

            Vector3 pole = (-0.6f * root.up + 0.5f * d.side * root.right - 0.4f * root.forward).normalized;

            // The hand's up (toward the elbow) stays in the swing plane, square to the weapon, so the hand
            // rolls with the arc and the wrist never twists through the swing.
            Vector3 tangent = Vector3.Cross(direction, plan.axis);
            Vector3 contactTangent = Vector3.Cross(plan.contactDirection, plan.axis);
            if (Vector3.Dot(contactTangent, pole) < 0f)
            {
                tangent = -tangent;
            }

            if (tangent.sqrMagnitude < 1e-6f)
            {
                tangent = root.up;
            }

            Quaternion handRotation = Quaternion.LookRotation(direction, tangent.normalized);

            // The wind-up and recover go straight between poses (in the weapon's own space), not along the
            // arc: from the carry to the wound-up position out at the side, and from wherever the
            // follow-through stopped back to the carry.
            if (hasCarry && Current == Phase.WindUp)
            {
                float s = PathProgress();
                grip = Vector3.Lerp(root.TransformPoint(carryGripLocal), grip, s);
                handRotation = Quaternion.Slerp(root.rotation * carryHandLocal, handRotation, s);
            }
            else if (hasCarry && Current == Phase.Recover)
            {
                float s = PathProgress();
                grip = Vector3.Lerp(root.TransformPoint(followEndGripLocal), root.TransformPoint(carryGripLocal), s);
                handRotation = Quaternion.Slerp(root.rotation * followEndHandLocal, root.rotation * carryHandLocal, s);
            }

            grip = KeepClearOfTorso(grip, scale);
            lastGrip = grip;
            lastHandRotation = handRotation;

            pose.gripTarget = grip;
            pose.handRotation = handRotation;
            pose.elbowPole = pole;

            if (TwoHanded)
            {
                // The left hand holds on further along: its own wrist turned toward its own shoulder.
                Vector3 clubDirection = handRotation * Vector3.forward;
                Vector3 second = WeaponGrip.SecondGrip(grip, handRotation, self.Weapon.Definition.secondGrip);
                pose.twoHanded = true;
                pose.secondGripTarget = second;
                pose.secondHandRotation = WeaponGrip.HandRotation(clubDirection, second, self.Ghost.Part(PartId.UpperArmL).position);
                pose.secondElbowPole = WeaponGrip.ElbowPole(root, -d.side);
                pose.freeArmPitch = 0f;
                pose.freeArmOut = 0f;
            }

            return pose;
        }

        /// <summary>
        /// Pushes the grip out of the torso (a vertical cylinder around the chest), so the hand is never
        /// asked to be inside the body: with self-collision on, it would jam there instead.
        /// </summary>
        private Vector3 KeepClearOfTorso(Vector3 grip, float scale)
        {
            Vector3 chest = self.Ghost.Part(PartId.Chest).position;
            float radius = TorsoClearance * scale;
            Vector3 offset = grip - chest;
            offset.y = 0f;
            if (grip.y < self.Ghost.Part(PartId.Pelvis).position.y || offset.magnitude >= radius)
            {
                return grip;
            }

            Vector3 outward = offset.sqrMagnitude > 1e-6f ? offset.normalized : self.Root.right * definition.side;
            return grip + outward * (radius - offset.magnitude);
        }

        private const float TorsoClearance = 0.34f;
    }
}
