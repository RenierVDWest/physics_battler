using PhysicsBattler.Rig;
using PhysicsBattler.Units;
using UnityEngine;

namespace PhysicsBattler.Muscles
{
    /// <summary>
    /// Drives every joint toward the ghost's pose with an acceleration-based slerp spring, so the
    /// same settings work for any unit size. What gets animated is strength, not position:
    /// spring = (2πf)² × group strength × action boost × hit dip × subtree boost.
    ///
    /// The subtree boost is what lets an arm hold a heavy club. An acceleration drive only accounts
    /// for the joint's own child body, so a shoulder carrying a forearm, a hand and a 6 kg club would
    /// sag. Each joint's spring is multiplied by (inertia of everything below it) / (inertia of the
    /// child alone), measured about the joint.
    /// </summary>
    public sealed class MuscleController
    {
        private const int GroupCount = 5;

        private readonly UnitRig rig;
        private readonly GhostRig ghost;
        private readonly UnitProfile profile;
        private readonly float[] groupStrength = new float[GroupCount];
        private readonly float[] groupBoost = new float[GroupCount];
        private readonly float[] subtreeBoost = new float[PartInfo.Count];
        private readonly float[] dipTimer = new float[PartInfo.Count];
        private readonly float[] dipDepth = new float[PartInfo.Count];

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
                if (!PartInfo.HasParent(id))
                {
                    subtreeBoost[(int)id] = 1f;
                    continue;
                }

                Vector3 pivot = rig.Part(id).transform.position;
                float own = InertiaAbout(rig.Part(id).body, pivot);
                float total = 0f;
                foreach (var other in PartInfo.All)
                {
                    if (IsInSubtree(other, id))
                    {
                        total += InertiaAbout(rig.Part(other).body, pivot);
                    }
                }

                subtreeBoost[(int)id] = Mathf.Clamp(own > 1e-6f ? total / own : 1f, 1f, profile.maxSubtreeBoost);
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

        private static float InertiaAbout(Rigidbody body, Vector3 pivot)
        {
            Vector3 tensor = body.inertiaTensor;
            float own = (tensor.x + tensor.y + tensor.z) / 3f;
            return own + body.mass * (body.worldCenterOfMass - pivot).sqrMagnitude;
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

        public void Step(float dt)
        {
            foreach (var id in PartInfo.All)
            {
                int i = (int)id;
                if (dipTimer[i] > 0f)
                {
                    dipTimer[i] -= dt;
                }

                var part = rig.Part(id);
                if (part.joint == null)
                {
                    continue;
                }

                var group = PartInfo.Group(id);
                float strength = groupStrength[(int)group] * groupBoost[(int)group] * DipFactor(id);
                float omega = 2f * Mathf.PI * Frequency(group);
                float spring = Mathf.Max(0f, strength) * omega * omega * subtreeBoost[i];
                float damper = 2f * profile.dampingRatio * Mathf.Sqrt(spring);

                part.joint.slerpDrive = new JointDrive
                {
                    positionSpring = spring,
                    positionDamper = damper,
                    maximumForce = float.MaxValue,
                    useAcceleration = true,
                };
                part.joint.targetRotation = JointMath.TargetRotation(ghost.Part(id).localRotation, part.restLocalRotation);
            }
        }
    }
}
