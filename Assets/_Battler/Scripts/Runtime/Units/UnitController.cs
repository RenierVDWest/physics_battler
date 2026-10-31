using System;
using PhysicsBattler.Authority;
using PhysicsBattler.Diagnostics;
using PhysicsBattler.Motion;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>
    /// One unit. Its transform is the gameplay root: where it stands on the board (on the ground)
    /// and which way it faces. Every physics step it:
    ///
    ///  1. turns toward what it faces (gameplay),
    ///  2. poses its animation rig (<see cref="PosePipeline"/>: stance, idle, guard, weight, legs),
    ///  3. drives the physical parts toward that pose (<see cref="AuthorityController"/>): legs and
    ///     pelvis kinematic, so they always carry the body; torso, head and arms pinned, so they
    ///     reach the pose on time but can still be knocked about.
    ///
    /// Physics never decides whether the unit is standing; the pose is valid without it.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public class UnitController : MonoBehaviour
    {
        [SerializeField] private UnitProfile profile;
        [SerializeField] private int team;

        private static Transform bodiesContainer;
        private static int spawnCount;   // seeds each unit's idle and shuffles differently
        private GameObject detachedBody;
        private float facingYaw;
        private Vector3? facePoint;
        private bool initialized;

        public static event Action<UnitController> Initialized;

        public UnitProfile Profile
        {
            get => profile;
            set => profile = value;
        }

        public int Team
        {
            get => team;
            set => team = value;
        }

        public CombatStyle Style { get; private set; }
        public UnitRig Rig { get; private set; }
        public AnimRig Anim { get; private set; }
        public BodyMeasure Body { get; private set; }
        public Stance Stance { get; private set; }
        public CombatIdle Idle { get; private set; }
        public PosePipeline Pose { get; private set; }
        public AuthorityController Authority { get; private set; }

        public bool IsInitialized => initialized;
        public float Scale => Rig != null ? Rig.Scale : 1f;
        public Transform Root => transform;
        public float RestPelvisHeight => Body != null ? Body.PelvisHeight : 1f;
        public float FacingYaw => facingYaw;

        /// <summary>The time the pose pipeline runs on (game time since the unit appeared).</summary>
        public float LocalTime { get; private set; }

        private void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Rig = GetComponent<UnitRig>();
            if (Rig == null || !Rig.IsComplete())
            {
                Debug.LogError($"[Unit] {name} has an incomplete rig. Run Physics Battler > Setup > Rebuild Prefabs.", this);
                enabled = false;
                return;
            }

            if (profile == null)
            {
                profile = UnitProfile.CreateDefault();
            }

            Style = profile.style != null ? profile.style : CombatStyle.CreateDefault();
            JointConventions.EnsureProbed();

            var bodyMaterial = PhysicsMaterials.Get("Body", profile.bodyFriction);
            var footMaterial = PhysicsMaterials.Get("Foot", profile.footFriction);
            foreach (var part in Rig.Parts)
            {
                part.Owner = this;
                UnitBuilder.ConfigureBody(part.body, profile);
                if (part.joint != null)
                {
                    JointMath.ApplyLimits(part.joint, part.limits);
                }

                if (part.partCollider != null)
                {
                    part.partCollider.sharedMaterial = PartInfo.IsFoot(part.id) ? footMaterial : bodyMaterial;
                }
            }

            // The animation rig is cloned while the body is still in its rest pose under this object.
            Anim = new AnimRig(transform, Rig);
            Body = BodyMeasure.Of(transform, Anim, Rig);

            // The physical parts leave the unit's hierarchy: each is its own rigidbody, side by side,
            // so moving the root never drags them. Only the authority layer moves them.
            var container = new GameObject(name + " (body)").transform;
            container.SetParent(BodiesContainer, false);
            detachedBody = container.gameObject;
            foreach (var id in PartInfo.All)
            {
                Rig.Part(id).transform.SetParent(container, true);
            }

            Physics.SyncTransforms();
            IgnoreSelfCollisions();

            int seed = 7919 * team + 104729 * ++spawnCount;
            Stance = new Stance(seed);
            Stance.Configure(Style, Body);
            Idle = new CombatIdle(seed + 1);
            Pose = new PosePipeline(transform, Anim, Body, Stance, Idle, Style);
            Authority = new AuthorityController(Rig, Anim);
            Authority.Set(BodyRegion.Legs, AuthorityLevel.Kinematic, 1f);
            Authority.Set(BodyRegion.Torso, AuthorityLevel.Pinned, profile.torsoPin);
            Authority.Set(BodyRegion.Head, AuthorityLevel.Pinned, profile.torsoPin);
            Authority.Set(BodyRegion.ArmL, AuthorityLevel.Pinned, profile.armPin);
            Authority.Set(BodyRegion.ArmR, AuthorityLevel.Pinned, profile.armPin);

            facingYaw = transform.eulerAngles.y;
            Stance.GroundY = transform.position.y;
            Stance.Plant(transform.position, facingYaw);

            // Start in the pose, so nothing snaps on the first step.
            Pose.Compose(0f, 0f);
            foreach (var id in PartInfo.All)
            {
                var part = Rig.Part(id);
                part.transform.SetPositionAndRotation(Anim.Part(id).position, Anim.Part(id).rotation);
                part.body.position = Anim.Part(id).position;
                part.body.rotation = Anim.Part(id).rotation;
            }

            Physics.SyncTransforms();
            initialized = true;
            Initialized?.Invoke(this);
        }

        private void OnDestroy()
        {
            if (detachedBody != null)
            {
                Destroy(detachedBody);
            }
        }

        private static Transform BodiesContainer
        {
            get
            {
                if (bodiesContainer == null)
                {
                    var existing = GameObject.Find("Unit Bodies");
                    bodiesContainer = existing != null ? existing.transform : new GameObject("Unit Bodies").transform;
                }

                return bodiesContainer;
            }
        }

        /// <summary>
        /// For now a unit's parts don't collide with each other: the pose keeps them apart, and
        /// pinned arms grinding against a kinematic torso would only jitter. (Weapons and hits come
        /// with their own rules in later milestones.)
        /// </summary>
        private void IgnoreSelfCollisions()
        {
            var parts = Rig.Parts;
            for (int i = 0; i < parts.Length; i++)
            {
                for (int j = i + 1; j < parts.Length; j++)
                {
                    if (parts[i].partCollider != null && parts[j].partCollider != null)
                    {
                        Physics.IgnoreCollision(parts[i].partCollider, parts[j].partCollider, true);
                    }
                }
            }
        }

        /// <summary>Face a point on the board (the unit turns at its turn speed, stepping as it goes).</summary>
        public void FacePoint(Vector3 point) => facePoint = point;

        /// <summary>Place the unit (gameplay): the feet re-plant under it.</summary>
        public void Place(Vector3 position, float yaw)
        {
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            facingYaw = yaw;
            Stance.GroundY = position.y;
            Stance.Plant(position, yaw);
        }

        private void FixedUpdate()
        {
            if (!initialized)
            {
                return;
            }

            float dt = Time.fixedDeltaTime;
            LocalTime += dt;
            if (facePoint.HasValue)
            {
                Vector3 to = facePoint.Value - transform.position;
                to.y = 0f;
                if (to.sqrMagnitude > 1e-4f)
                {
                    float wanted = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
                    facingYaw = Mathf.MoveTowardsAngle(facingYaw, wanted, profile.turnSpeed * dt);
                    transform.rotation = Quaternion.Euler(0f, facingYaw, 0f);
                }
            }

            Pose.Compose(LocalTime, dt);
            Authority.Apply(dt);
        }

        /// <summary>A part touched something (hands report every step). Hits come in milestone 3.</summary>
        public void OnPartCollision(BodyPart part, Collision collision, bool entered)
        {
            if (entered && PlaytestTrace.Enabled && collision.collider != null && collision.relativeVelocity.magnitude > 1f)
            {
                PlaytestTrace.Note(this, "contact", $"{part.id} touched {collision.collider.name} at {collision.relativeVelocity.magnitude:0.0} m/s");
            }
        }
    }
}
