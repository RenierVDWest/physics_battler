using System.Text;
using PhysicsBattler.Diagnostics;
using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Muscles
{
    /// <summary>
    /// Drives every joint toward the ghost's pose. Three parts:
    ///
    /// 1. A slerp spring-damper on each joint, acceleration-based, at the group's frequency:
    ///    spring = (2πf)² × strength × load boost, damper = 2ζ(2πf)√strength × load boost.
    ///    PhysX scales an acceleration drive by the joint's effective inertia, which comes from the two
    ///    bodies it joins, about their own centres: 1 / (1/I_child + 1/I_parent). That is tiny next to
    ///    what the joint really moves (a light forearm swinging a hand and a 6 kg club), so the load
    ///    boost is (inertia of everything below the joint, about the joint) / (that effective inertia).
    ///    The damper carries the same boost, so the load is damped as asked.
    /// 2. Gravity compensation: each joint also gets the torque that holds up everything hanging below
    ///    it, so the springs no longer sag under weight (the drooping head and the club falling to the
    ///    ground in the first playtest). Legs standing on the ground are left to the balance assist.
    /// 3. Velocity feed-forward: the joints are given the ghost's angular velocity as their target
    ///    speed, so a fast swing is followed rather than chased.
    ///
    /// What gets animated is strength, not position: strength = state tone × action boost × hit dip.
    /// </summary>
    public sealed class MuscleController
    {
        private const int GroupCount = 5;
        private const float MaxFeedForwardSpeed = 25f;     // rad/s
        private const float MaxGhostStepDegrees = 40f;      // a bigger jump in one step is a pose cut, not motion

        private readonly UnitRig rig;
        private readonly GhostRig ghost;
        private readonly UnitProfile profile;
        private readonly float[] groupStrength = new float[GroupCount];
        private readonly float[] groupBoost = new float[GroupCount];
        private readonly float[] subtreeBoost = new float[PartInfo.Count];
        private readonly float[] rawBoost = new float[PartInfo.Count];
        private readonly float[] dipTimer = new float[PartInfo.Count];
        private readonly float[] dipDepth = new float[PartInfo.Count];
        private readonly float[] lastStrength = new float[PartInfo.Count];
        private readonly float[] lastGravityTorque = new float[PartInfo.Count];
        private readonly Quaternion[] previousGhost = new Quaternion[PartInfo.Count];
        private readonly bool[] hasPrevious = new bool[PartInfo.Count];
        private readonly PartId[][] subtrees = new PartId[PartInfo.Count][];
        private readonly Vector3[] centres = new Vector3[PartInfo.Count];
        private readonly float[] masses = new float[PartInfo.Count];

        /// <summary>The drive strength last applied to a part's joint (tone × boost × dip; 1 = the profile's).</summary>
        public float Strength(PartId id) => lastStrength[(int)id];

        /// <summary>The gravity-compensation torque (N·m) last applied at a part's joint.</summary>
        public float GravityTorque(PartId id) => lastGravityTorque[(int)id];

        /// <summary>Feet standing on the ground: their legs carry the body and get no gravity compensation.</summary>
        public bool StanceLeft { get; set; } = true;
        public bool StanceRight { get; set; } = true;

        public MuscleController(UnitRig rig, GhostRig ghost, UnitProfile profile)
        {
            this.rig = rig;
            this.ghost = ghost;
            this.profile = profile;
            for (int g = 0; g < GroupCount; g++)
            {
                groupStrength[g] = 1f;
                groupBoost[g] = 1f;
            }

            foreach (var id in PartInfo.All)
            {
                var members = new System.Collections.Generic.List<PartId>();
                foreach (var other in PartInfo.All)
                {
                    if (IsInSubtree(other, id))
                    {
                        members.Add(other);
                    }
                }

                subtrees[(int)id] = members.ToArray();
            }

            RecomputeSubtreeBoost();
        }

        /// <summary>Overall strength for every group (from the unit's state).</summary>
        public void SetTone(float tone)
        {
            for (int g = 0; g < GroupCount; g++)
            {
                groupStrength[g] = tone;
            }
        }

        public void SetGroupStrength(MuscleGroup group, float strength) => groupStrength[(int)group] = strength;

        /// <summary>A multiplier from the current action (the weapon arm pushing hard during a strike).</summary>
        public void SetBoost(MuscleGroup group, float boost) => groupBoost[(int)group] = boost;

        public void ClearBoosts()
        {
            for (int g = 0; g < GroupCount; g++)
            {
                groupBoost[g] = 1f;
            }
        }

        /// <summary>Softens the struck part a lot and its neighbours a little, briefly.</summary>
        public void Dip(PartId struck, float scale = 1f)
        {
            float duration = profile.dipHold + profile.dipRecover;
            foreach (var id in PartInfo.SelfAndNeighbours(struck))
            {
                float depth = id == struck ? profile.dipStrength : Mathf.Lerp(profile.dipStrength, 1f, 0.45f);
                depth = Mathf.Lerp(1f, depth, Mathf.Clamp01(scale));
                int i = (int)id;
                dipDepth[i] = dipTimer[i] > 0f ? Mathf.Min(dipDepth[i], depth) : depth;
                dipTimer[i] = duration;
            }
        }

        public float DipFactor(PartId id)
        {
            int i = (int)id;
            if (dipTimer[i] <= 0f)
            {
                return 1f;
            }

            if (dipTimer[i] > profile.dipRecover)
            {
                return dipDepth[i];
            }

            float recovered = 1f - dipTimer[i] / Mathf.Max(0.01f, profile.dipRecover);
            return Mathf.Lerp(dipDepth[i], 1f, recovered * recovered);
        }

        /// <summary>Call after anything changes the parts' masses (equipping a weapon).</summary>
        public void RecomputeSubtreeBoost()
        {
            foreach (var id in PartInfo.All)
            {
                int i = (int)id;
                if (!PartInfo.HasParent(id))
                {
                    subtreeBoost[i] = rawBoost[i] = 1f;
                    continue;
                }

                var body = rig.Part(id).body;
                var parent = rig.Part(PartInfo.Parent(id)).body;
                Vector3 pivot = body.position;
                float load = 0f;
                foreach (var other in subtrees[i])
                {
                    load += InertiaAbout(rig.Part(other).body, pivot);
                }

                float child = MeanInertia(body);
                float parentInertia = MeanInertia(parent);
                float effective = child > 1e-6f && parentInertia > 1e-6f ? 1f / (1f / child + 1f / parentInertia) : child;
                rawBoost[i] = effective > 1e-6f ? load / effective : 1f;
                subtreeBoost[i] = Mathf.Clamp(rawBoost[i], 1f, profile.maxSubtreeBoost);
            }

            var owner = rig.GetComponent<UnitController>();
            if (PlaytestTrace.Enabled && owner != null)
            {
                var text = new StringBuilder("load boosts (needed→used): ");
                foreach (var id in PartInfo.All)
                {
                    if (PartInfo.HasParent(id))
                    {
                        int i = (int)id;
                        text.Append($"{id} {rawBoost[i]:0}{(rawBoost[i] > subtreeBoost[i] + 0.5f ? $"→{subtreeBoost[i]:0} (capped)" : "")}, ");
                    }
                }

                PlaytestTrace.Note(owner, "muscles", text.ToString().TrimEnd(',', ' '));
            }
        }

        public float SubtreeBoost(PartId id) => subtreeBoost[(int)id];

        private static bool IsInSubtree(PartId part, PartId root)
        {
            var current = part;
            while (true)
            {
                if (current == root)
                {
                    return true;
                }

                if (!PartInfo.HasParent(current))
                {
                    return false;
                }

                current = PartInfo.Parent(current);
            }
        }

        private static float MeanInertia(Rigidbody body)
        {
            Vector3 tensor = body.inertiaTensor;
            return (tensor.x + tensor.y + tensor.z) / 3f;
        }

        private static float InertiaAbout(Rigidbody body, Vector3 pivot)
        {
            return MeanInertia(body) + body.mass * (body.worldCenterOfMass - pivot).sqrMagnitude;
        }

        private float Frequency(MuscleGroup group)
        {
            switch (group)
            {
                case MuscleGroup.Legs: return profile.legsHz;
                case MuscleGroup.Core: return profile.coreHz;
                case MuscleGroup.Neck: return profile.neckHz;
                default: return profile.armHz;
            }
        }

        private bool IsStanceLeg(PartId id)
        {
            switch (id)
            {
                case PartId.ThighL:
                case PartId.ShinL:
                case PartId.FootL:
                    return StanceLeft;
                case PartId.ThighR:
                case PartId.ShinR:
                case PartId.FootR:
                    return StanceRight;
                default:
                    return false;
            }
        }

        public void Step(float dt)
        {
            foreach (var id in PartInfo.All)
            {
                int i = (int)id;
                var body = rig.Part(id).body;
                masses[i] = body.mass;
                centres[i] = body.worldCenterOfMass;
            }

            Vector3 gravity = Physics.gravity;
            foreach (var id in PartInfo.All)
            {
                int i = (int)id;
                if (dipTimer[i] > 0f)
                {
                    dipTimer[i] -= dt;
                }

                var part = rig.Part(id);
                lastGravityTorque[i] = 0f;
                if (part.joint == null)
                {
                    continue;
                }

                var group = PartInfo.Group(id);
                float strength = groupStrength[(int)group] * groupBoost[(int)group] * DipFactor(id);
                lastStrength[i] = strength;
                JointMath.DriveGains(Frequency(group), profile.dampingRatio, strength, subtreeBoost[i], out float spring, out float damper);

                part.joint.slerpDrive = new JointDrive
                {
                    positionSpring = spring,
                    positionDamper = damper,
                    maximumForce = float.MaxValue,
                    useAcceleration = true,
                };

                Quaternion ghostLocal = ghost.Part(id).localRotation;
                part.joint.targetRotation = JointMath.TargetRotation(ghostLocal, part.restLocalRotation);
                part.joint.targetAngularVelocity = FeedForward(i, ghostLocal, part.restLocalRotation, dt);

                // Hold up everything below this joint against gravity: the torque on the child, and the
                // same torque the other way on the parent.
                float compensation = profile.gravityCompensation * Mathf.Clamp01(strength);
                if (compensation > 0f && !IsStanceLeg(id))
                {
                    Vector3 pivot = part.body.position;
                    Vector3 torque = Vector3.zero;
                    foreach (var member in subtrees[i])
                    {
                        int m = (int)member;
                        torque += Vector3.Cross(centres[m] - pivot, gravity * masses[m]);
                    }

                    torque *= -compensation;   // cancel gravity's torque about the joint
                    part.body.AddTorque(torque, ForceMode.Force);
                    rig.Part(PartInfo.Parent(id)).body.AddTorque(-torque, ForceMode.Force);
                    lastGravityTorque[i] = torque.magnitude;
                }
            }
        }

        /// <summary>The ghost's angular velocity in joint space, as the drive's target speed.</summary>
        private Vector3 FeedForward(int i, Quaternion ghostLocal, Quaternion restLocal, float dt)
        {
            Quaternion relative = Quaternion.Inverse(restLocal) * ghostLocal;
            Quaternion previous = previousGhost[i];
            bool had = hasPrevious[i];
            previousGhost[i] = relative;
            hasPrevious[i] = true;
            if (!had || profile.velocityFeedForward <= 0f || !JointConventions.VelocityProbed)
            {
                return Vector3.zero;
            }

            if (Quaternion.Angle(previous, relative) > MaxGhostStepDegrees)
            {
                return Vector3.zero;   // the pose was cut (a state change), not moved
            }

            Vector3 omega = JointMath.BodyAngularVelocity(previous, relative, dt);
            omega = Vector3.ClampMagnitude(omega, MaxFeedForwardSpeed) * profile.velocityFeedForward;
            return Vector3.Scale(omega, JointConventions.VelocitySign);
        }
    }
}
