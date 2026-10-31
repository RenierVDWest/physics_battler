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
    /// Kinematic parts are moved to the pose. Pinned parts get the velocity that brings them onto
    /// the pose this step, blended with what they had by the pin weight (PuppetMaster-style
    /// pinning): 1 arrives exactly, lower lags and overshoots, which reads as mass. Loose parts are
    /// barely held, ragdoll parts not at all.
    /// </summary>
    public sealed class AuthorityController
    {
        public const float LoosePin = 0.04f;

        private readonly UnitRig rig;
        private readonly AnimRig anim;
        private readonly AuthorityLevel[] levels = new AuthorityLevel[5];
        private readonly float[] pins = new float[5];
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

        public void Set(BodyRegion region, AuthorityLevel level, float pin)
        {
            levels[(int)region] = level;
            pins[(int)region] = Mathf.Clamp01(pin);
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

            foreach (var id in PartInfo.All)
            {
                var part = rig.Part(id);
                var body = part.body;
                var target = anim.Part(id);
                var region = RegionOf(id);
                var level = levels[(int)region];
                bool kinematic = level == AuthorityLevel.Kinematic;
                if (body.isKinematic != kinematic)
                {
                    body.isKinematic = kinematic;
                }

                if (kinematic)
                {
                    body.MovePosition(target.position);
                    body.MoveRotation(target.rotation);
                    continue;
                }

                float pin = level == AuthorityLevel.Pinned ? pins[(int)region]
                    : level == AuthorityLevel.Loose ? LoosePin : 0f;
                if (pin <= 0f)
                {
                    continue;
                }

                Vector3 gravity = body.useGravity ? UnityEngine.Physics.gravity : Vector3.zero;
                body.linearVelocity = PinVelocity(body.position, target.position, body.linearVelocity, pin, dt, gravity);
                body.angularVelocity = PinAngularVelocity(body.rotation, target.rotation, body.angularVelocity, pin, dt);
            }
        }

        /// <summary>
        /// The velocity that lands on the target this step, blended with the current one by the pin.
        /// Gravity is taken out in advance (the solver adds it during the step).
        /// </summary>
        public static Vector3 PinVelocity(Vector3 position, Vector3 target, Vector3 velocity, float pin, float dt, Vector3 gravity = default)
        {
            Vector3 needed = (target - position) / dt - gravity * dt;
            return Vector3.Lerp(velocity, needed, Mathf.Clamp01(pin));
        }

        /// <summary>The angular velocity that turns onto the target this step, blended by the pin.</summary>
        public static Vector3 PinAngularVelocity(Quaternion rotation, Quaternion target, Vector3 angularVelocity, float pin, float dt)
        {
            Vector3 needed = JointMath.RotationError(rotation, target) / dt;
            return Vector3.Lerp(angularVelocity, needed, Mathf.Clamp01(pin));
        }

        /// <summary>Distance and angle between a part and its pose (for the log).</summary>
        public void Error(PartId id, out float distance, out float angle)
        {
            var body = rig.Part(id).body;
            var target = anim.Part(id);
            distance = Vector3.Distance(body.position, target.position);
            angle = Quaternion.Angle(body.rotation, target.rotation);
        }
    }
}
