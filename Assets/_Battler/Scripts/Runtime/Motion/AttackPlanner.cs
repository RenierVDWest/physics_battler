using System;
using PhysicsBattler.Diagnostics;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Motion
{
    /// <summary>
    /// Runs one swing through its phases (wind-up, load, strike, follow-through, recover) and
    /// produces the <see cref="ActionPose"/> for each step. The swing is planned fresh for the
    /// target's distance and height, tracks the target until the end of the load (the commit point),
    /// then is frozen in world space: a target that steps away is missed, and anything that steps
    /// into the arc is hit.
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

        public Phase Current { get; private set; } = Phase.Idle;
        public bool Active => Current != Phase.Idle;
        public bool Committed { get; private set; }
        public float Theta { get; private set; }
        public SwingPlan Plan { get; private set; }
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

            // Start from the ghost's carry pose (where the weapon is meant to be), not from wherever the
            // physical club happens to be: every swing then starts the same way.
            var ghostHand = self.Ghost.Part(PartId.HandR);
            var slot = self.Ghost.SlotWeaponR;
            var root = self.Root;
            carryGripLocal = root.InverseTransformPoint(slot != null ? slot.position : ghostHand.position);
            carryHandLocal = Quaternion.Inverse(root.rotation) * ghostHand.rotation;
            hasCarry = true;
            entryTheta = SwingSolver.AngleOf(Plan, ghostHand.rotation * Vector3.forward);
            if (PlaytestTrace.Enabled)
            {
                var plan = Plan;
                PlaytestTrace.Note(self, "attack",
                    $"swing at {target.name}: target {plan.distance:0.00} m from the shoulder, contact reach {plan.contactReach:0.00} m, "
                    + $"plane roll {plan.rollDegrees:0}°, extra lean {plan.extraLean:0.00} m, crowded ×{plan.crowdedFactor:0.00}, "
                    + $"club starts at θ {entryTheta:0}° (wind-up {(entryTheta > 0f ? "over the top" : "down and back")} to {definition.windUpAngle:0}°)");
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

        /// <summary>The weapon landed a solid hit: stop driving through and move on to the follow-through.</summary>
        public void OnSolidHit()
        {
            if (Current == Phase.Strike && !landedHit)
            {
                landedHit = true;
                SetPhase(Phase.Follow);
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
            AdvancePhases();
            if (!Active)
            {
                return ActionPose.None;
            }

            if (!Committed && Target != null)
            {
                Replan(PredictTarget());
            }

            return BuildPose();
        }

        private void AdvancePhases()
        {
            var d = definition;
            while (Active && phaseTime >= Duration(Current, d))
            {
                phaseTime -= Duration(Current, d);
                if (Current == Phase.Load)
                {
                    Committed = true;
                    if (PlaytestTrace.Enabled && Target != null)
                    {
                        PlaytestTrace.Note(self, "attack",
                            $"committed: arc frozen, aiming at {Fmt(Plan.target)} (target's chest now {Fmt(Target.AimPoint)}, "
                            + $"{Vector3.Distance(Plan.target, Target.AimPoint):0.00} m apart), contact reach {Plan.contactReach:0.00} m");
                    }
                }

                SetPhase(Current == Phase.Recover ? Phase.Idle : Current + 1, keepTime: true);
            }
        }

        private static float Duration(Phase phase, AttackDefinition d)
        {
            switch (phase)
            {
                case Phase.WindUp: return Mathf.Max(0.01f, d.windUpTime);
                case Phase.Load: return Mathf.Max(0.01f, d.loadTime);
                case Phase.Strike: return Mathf.Max(0.01f, d.strikeTime);
                case Phase.Follow: return Mathf.Max(0.01f, d.followTime);
                case Phase.Recover: return Mathf.Max(0.01f, d.recoverTime);
                default: return float.MaxValue;
            }
        }

        private void SetPhase(Phase phase, bool keepTime = false)
        {
            if (!keepTime)
            {
                phaseTime = 0f;
            }

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
            var d = definition;
            float toContact = Duration(Phase.Strike, d) * ContactFraction(d);
            if (Current == Phase.WindUp)
            {
                toContact += Duration(Phase.WindUp, d) - phaseTime + Duration(Phase.Load, d);
            }
            else if (Current == Phase.Load)
            {
                toContact += Duration(Phase.Load, d) - phaseTime;
            }

            return Target.AimPoint + Target.AimVelocity * Mathf.Min(toContact, 1f);
        }

        /// <summary>Share of the strike's time at which θ passes 0.</summary>
        private static float ContactFraction(AttackDefinition d)
        {
            float span = d.followAngle - d.loadAngle;
            float share = span > 0f ? -d.loadAngle / span : 1f;
            return Mathf.Pow(Mathf.Clamp01(share), 1f / Mathf.Max(1f, d.strikePower));
        }

        private void Replan(Vector3 target)
        {
            var d = definition;
            Vector3 shoulder = self.RestShoulderWorld(d.side);
            var reach = Reach;
            float roll = SwingSolver.RollForClearance(shoulder, target, reach, d.planeRoll, d.side,
                Mathf.Min(d.windUpAngle, d.loadAngle), self.GroundY, d.groundClearance * self.Scale);
            Plan = SwingSolver.Plan(shoulder, target, reach, roll, d.side);
        }

        private float CurrentTheta()
        {
            var d = definition;
            float u = phaseTime / Duration(Current, d);
            switch (Current)
            {
                case Phase.WindUp:
                {
                    // From the carry (club up on the shoulder, a positive angle) the wind-up goes on over
                    // the top and drops behind, never down through the front where the target stands.
                    float windUp = entryTheta > 0f ? d.windUpAngle + 360f : d.windUpAngle;
                    return WrapAngle(Mathf.Lerp(entryTheta, windUp, Ease.InOutCubic(u)));
                }

                case Phase.Load: return Mathf.Lerp(d.windUpAngle, d.loadAngle, Ease.OutCubic(u));
                case Phase.Strike: return Mathf.Lerp(d.loadAngle, d.followAngle, Ease.InPower(u, d.strikePower));
                case Phase.Follow: return d.followAngle + 10f * Ease.OutCubic(u);
                case Phase.Recover:
                {
                    // Carry on up the plane (out to the side, clear of the head) toward the carry's angle.
                    float end = entryTheta > d.followAngle + 10f ? entryTheta : d.followAngle + 10f;
                    return Mathf.Lerp(d.followAngle + 10f, end, Ease.InOutCubic(u));
                }

                default: return d.followAngle + 10f;
            }
        }

        /// <summary>An angle in degrees, wrapped into (-180, 180].</summary>
        private static float WrapAngle(float degrees)
        {
            float wrapped = Mathf.Repeat(degrees + 180f, 360f) - 180f;
            return wrapped <= -180f ? wrapped + 360f : wrapped;
        }

        /// <summary>How much the body (twist, lean, crouch) takes part: eases in and out.</summary>
        private float BodyWeight()
        {
            float u = phaseTime / Duration(Current, definition);
            switch (Current)
            {
                case Phase.WindUp: return Ease.OutCubic(Mathf.Clamp01(u / 0.35f));
                case Phase.Recover: return 1f - Ease.InOutCubic(u);
                default: return 1f;
            }
        }

        /// <summary>
        /// How far the weapon's target is along the arc rather than at the carry (0 = carry, 1 = arc).
        /// The blend is in the weapon's own space (grip position and hand rotation), so the club moves
        /// on a predictable path. (Blending joint angles instead, as before, swung the club through
        /// unpredictable places: in the second playtest a wind-up clubbed the target on the head.)
        /// </summary>
        private float ArcBlend()
        {
            float u = phaseTime / Duration(Current, definition);
            switch (Current)
            {
                case Phase.WindUp: return Ease.InOutCubic(Mathf.Clamp01(u / 0.45f));
                case Phase.Recover: return 1f - Ease.InOutCubic(Mathf.Clamp01((u - 0.35f) / 0.65f));
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

            float u = phaseTime / Duration(Current, definition);
            return 1f - Ease.InOutCubic(Mathf.Clamp01((u - 0.85f) / 0.15f));
        }

        private ActionPose BuildPose()
        {
            var d = definition;
            float scale = self.Scale;
            Theta = CurrentTheta();
            float weight = BodyWeight();
            float armWeight = ArmWeight();
            float arc = hasCarry ? ArcBlend() : 1f;
            var plan = Plan;

            // Coil: 1 fully wound back, 0 at contact, -0.7 unwound past it. In the wind-up it follows the
            // phase's progress instead of the angle, which may start anywhere (the carry is over the shoulder).
            float coil;
            if (Current == Phase.WindUp)
            {
                float u = Mathf.Clamp01(phaseTime / Duration(Phase.WindUp, d));
                coil = Ease.InOutCubic(u) * Mathf.Clamp01(-d.windUpAngle / Mathf.Max(1f, -d.loadAngle));
            }
            else
            {
                coil = Theta < 0f
                    ? Mathf.Clamp01(-Theta / Mathf.Max(1f, -d.loadAngle))
                    : -0.7f * Mathf.Clamp01(Theta / Mathf.Max(1f, d.followAngle));
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

            // The grip follows the arc about the current shoulder.
            Vector3 shoulder = self.Ghost.Part(PartId.UpperArmR).position;
            Vector3 direction = SwingSolver.Direction(plan, Theta);
            var reach = Reach;
            float arm = Current == Phase.WindUp || Current == Phase.Load
                ? Mathf.Lerp(plan.contactReach, (reach.minArm + reach.maxArm) * 0.5f, Mathf.Clamp01(coil))
                : plan.contactReach;
            float groundLimit = SwingSolver.GroundLimitedReach(shoulder, direction, reach.tip, self.GroundY,
                d.groundClearance * scale);
            arm = Mathf.Max(reach.minArm, Mathf.Min(arm, groundLimit));
            Vector3 grip = shoulder + direction * arm;

            Vector3 pole = (-0.6f * self.Root.up + 0.5f * d.side * self.Root.right - 0.4f * self.Root.forward).normalized;

            // The hand's forward is the weapon's direction. Its up (toward the elbow) stays in the swing
            // plane, square to the weapon, so the hand rolls with the arc instead of with the world and
            // the wrist never twists through the swing. The side is the one the elbow is on at contact.
            Vector3 tangent = Vector3.Cross(direction, plan.axis);
            Vector3 contactTangent = Vector3.Cross(plan.contactDirection, plan.axis);
            if (Vector3.Dot(contactTangent, pole) < 0f)
            {
                tangent = -tangent;
            }

            if (tangent.sqrMagnitude < 1e-6f)
            {
                tangent = self.Root.up;
            }

            Quaternion handRotation = Quaternion.LookRotation(direction, tangent.normalized);
            if (arc < 1f)
            {
                var root = self.Root;
                grip = Vector3.Lerp(root.TransformPoint(carryGripLocal), grip, arc);
                handRotation = Quaternion.Slerp(root.rotation * carryHandLocal, handRotation, arc);
            }

            pose.gripTarget = grip;
            pose.handRotation = handRotation;
            pose.elbowPole = pole;
            return pose;
        }
    }
}
