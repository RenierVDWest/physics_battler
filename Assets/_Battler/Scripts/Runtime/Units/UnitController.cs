using System;
using System.Collections.Generic;
using PhysicsBattler.Combat;
using PhysicsBattler.Diagnostics;
using PhysicsBattler.Motion;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>
    /// Runs one unit, in one fixed order every physics step:
    /// sense the body → update stability and state → advance the action → re-anchor and catch
    /// steps → pose the ghost → drive the muscles → apply the balance assist. PhysX then steps, and
    /// collisions come back here to be turned into hits for the next step.
    ///
    /// This object is the unit's logical root on the ground. At start it builds the ghost rig under
    /// itself and detaches the physical body into the scene, because moving a parent of dynamic
    /// rigidbodies would teleport them.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(UnitRig))]
    public class UnitController : MonoBehaviour
    {
        [SerializeField] private UnitProfile profile;
        [Tooltip("Spawned and equipped in the right hand at start.")]
        [SerializeField] private Weapon startingWeapon;
        [SerializeField] private int team;
        [Tooltip("Draw the ghost (cyan) over the physical body (white) in the Scene and Game views (Gizmos on).")]
        [SerializeField] private bool showGhost = true;

        private readonly Dictionary<UnitController, float> lastHitTimes = new Dictionary<UnitController, float>();
        private Vector3 restShoulderLocalR;
        private Vector3 restShoulderLocalL;
        private float restPelvisHeight = 1f;
        private float lastGroundedTime = -10f;
        private float releaseArmTimer;
        private GameObject detachedBody;
        private int groundMask;
        private int boostRecomputeIn;
        private bool initialized;

        private static Transform bodiesContainer;

        public UnitProfile Profile
        {
            get => profile;
            set => profile = value;
        }

        public Weapon StartingWeapon
        {
            get => startingWeapon;
            set => startingWeapon = value;
        }

        public int Team
        {
            get => team;
            set => team = value;
        }

        public bool ShowGhost
        {
            get => showGhost;
            set => showGhost = value;
        }

        public UnitRig Rig { get; private set; }
        public GhostRig Ghost { get; private set; }
        public UnitState State { get; private set; }
        public MuscleController Muscles { get; private set; }
        public BalanceController Balance { get; private set; }
        public AttackPlanner Planner { get; private set; }
        public Stance Stance { get; private set; }
        public PoseComposer Pose { get; private set; }
        public Weapon Weapon { get; private set; }
        public Transform Root => transform;
        public float GroundY { get; private set; }
        public float Scale => Rig != null ? Rig.Scale : 1f;
        public bool Grounded => Time.time - lastGroundedTime < 0.2f;
        public bool IsInitialized => initialized;

        /// <summary>Where attackers aim: the physical chest's aim marker.</summary>
        public Vector3 AimPoint => Rig.AimChest != null
            ? Rig.Part(PartId.Chest).PhysicalPoint(Rig.AimChest.position)
            : Rig.Part(PartId.Chest).body.position;

        public Vector3 AimVelocity => Rig.Part(PartId.Chest).body.linearVelocity;

        public event Action<HitEvent> Hit;

        /// <summary>A part (or the held weapon, through the hand) started touching something outside the unit.</summary>
        public event Action<BodyPart, Collision> ContactEntered;

        /// <summary>Any unit finished setting itself up (the playtest recorder starts following it).</summary>
        public static event Action<UnitController> Initialized;

        public float RestPelvisHeight => restPelvisHeight;

        /// <summary>What the body was doing at the start of the last step.</summary>
        public BodySense LastSense { get; private set; }

        /// <summary>The weight was over the feet at the start of the last step (stagger can end).</summary>
        public bool CaughtBalance { get; private set; }

        private void Awake()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Rig = GetComponent<UnitRig>();
            if (Rig == null || !Rig.IsComplete())
            {
                Debug.LogError($"[Unit] {name} has an incomplete rig. Rebuild it with Physics Battler > Setup > Rebuild Prefabs.", this);
                enabled = false;
                return;
            }

            if (profile == null)
            {
                profile = UnitProfile.CreateDefault();
            }

            JointConventions.EnsureProbed();
            groundMask = LayerMask.GetMask(Layers.Ground);

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

            // The ghost is cloned while the body is still in its rest pose under this object.
            Stance = new Stance();
            Ghost = new GhostRig(transform, Rig);
            Stance.RestLocalL = transform.InverseTransformPoint(Ghost.Part(PartId.FootL).position);
            Stance.RestLocalR = transform.InverseTransformPoint(Ghost.Part(PartId.FootR).position);
            restShoulderLocalR = transform.InverseTransformPoint(Ghost.Part(PartId.UpperArmR).position);
            restShoulderLocalL = transform.InverseTransformPoint(Ghost.Part(PartId.UpperArmL).position);
            restPelvisHeight = Ghost.Part(PartId.Pelvis).position.y - transform.position.y;

            var pelvis = Rig.Part(PartId.Pelvis).transform;
            pelvis.SetParent(BodiesContainer, true);
            detachedBody = pelvis.gameObject;
            detachedBody.name = name + " (body)";
            IgnoreSelfCollisions();

            State = new UnitState(profile);
            State.Changed += OnStateChanged;
            Muscles = new MuscleController(Rig, Ghost, profile);
            Balance = new BalanceController(Rig, Ghost, profile, Stance);
            Planner = new AttackPlanner(this);
            Pose = new PoseComposer(Ghost, Rig, Stance);

            GroundY = ProbeGround(transform.position.y);
            Stance.Plant(transform, GroundY);

            if (startingWeapon != null)
            {
                Equip(Instantiate(startingWeapon));
            }

            initialized = true;
            Initialized?.Invoke(this);
        }

        private void Start()
        {
            if (initialized)
            {
                // Inertia tensors are certain to be computed by now.
                Muscles.RecomputeSubtreeBoost();
            }
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

        private void IgnoreSelfCollisions()
        {
            var colliders = new List<Collider>();
            foreach (var part in Rig.Parts)
            {
                if (part.partCollider != null)
                {
                    colliders.Add(part.partCollider);
                }
            }

            for (int i = 0; i < colliders.Count; i++)
            {
                for (int j = i + 1; j < colliders.Count; j++)
                {
                    Physics.IgnoreCollision(colliders[i], colliders[j], true);
                }
            }
        }

        /// <summary>Puts a weapon in the right hand: its colliders join the hand's rigidbody.</summary>
        public void Equip(Weapon weapon)
        {
            if (weapon == null || Rig.SlotWeaponR == null)
            {
                return;
            }

            var hand = Rig.Part(PartId.HandR);
            var forearm = Rig.Part(PartId.ForearmR);

            // Put the arm in the carry pose (weapon up over the shoulder) before the weapon is in it, so a
            // long weapon never appears inside whoever stands in front. The body is a transform tree, so
            // turning the forearm carries the hand; then the rigidbodies are moved to match.
            forearm.transform.localRotation = forearm.restLocalRotation * PoseComposer.CarryForearm;
            hand.transform.localRotation = hand.restLocalRotation * PoseComposer.CarryHand;
            foreach (var moved in new[] { forearm, hand })
            {
                moved.body.position = moved.transform.position;
                moved.body.rotation = moved.transform.rotation;
                moved.body.linearVelocity = Vector3.zero;
                moved.body.angularVelocity = Vector3.zero;
            }
            weapon.AttachTo(this, hand, Rig.SlotWeaponR, Layers.Index(Layers.Weapon));
            foreach (var weaponCollider in weapon.colliders)
            {
                foreach (var part in Rig.Parts)
                {
                    if (weaponCollider != null && part.partCollider != null)
                    {
                        Physics.IgnoreCollision(weaponCollider, part.partCollider, true);
                    }
                }
            }

            hand.body.mass += weapon.Definition.physicsMassKg;
            hand.body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            hand.body.ResetCenterOfMass();
            hand.body.ResetInertiaTensor();
            Weapon = weapon;
            Muscles?.RecomputeSubtreeBoost();
            boostRecomputeIn = 2;   // the new colliders' inertia is only in after a physics step
        }

        public Vector3 RestShoulderWorld(int side)
        {
            return transform.TransformPoint(side >= 0 ? restShoulderLocalR : restShoulderLocalL);
        }

        public bool TryAttack(UnitController target, AttackDefinition attack, out string reason)
        {
            if (!initialized || !State.IsStanding)
            {
                reason = initialized ? $"can't attack while {State.Kind}" : "not ready";
                return false;
            }

            return Planner.TryBegin(target, attack, out reason);
        }

        public void Kill()
        {
            State?.Kill();
        }

        private void FixedUpdate()
        {
            if (!initialized)
            {
                return;
            }

            float dt = Time.fixedDeltaTime;
            foreach (var part in Rig.Parts)
            {
                part.CacheState();
            }

            GroundY = ProbeGround(GroundY);
            Balance.Sense(GroundY);

            var pelvis = Rig.Part(PartId.Pelvis).body;
            var sense = new BodySense
            {
                tiltDegrees = Vector3.Angle(Rig.Part(PartId.Chest).body.rotation * Vector3.up, Vector3.up),
                pelvisHeight = pelvis.position.y - GroundY,
                restPelvisHeight = restPelvisHeight,
                pelvisSpeed = pelvis.linearVelocity.magnitude,
                grounded = Grounded,
            };
            bool caught = !Stance.Stepping && Balance.BalanceError < profile.stepThreshold * Scale * 1.5f;
            LastSense = sense;
            CaughtBalance = caught;
            State.Step(dt, sense, caught);

            if (Planner.Active && !State.IsStanding)
            {
                Planner.Abandon($"unit is in {State.Kind}");
                releaseArmTimer = 0.6f;
            }

            var action = Planner.Step(dt);

            var kind = State.Kind;
            if (kind == UnitStateKind.Steady || kind == UnitStateKind.Flinch || kind == UnitStateKind.Stagger)
            {
                float radius = (kind == UnitStateKind.Stagger ? 0.04f : profile.reanchorRadius) * Scale;
                Balance.Reanchor(transform, GroundY, radius);

                // Catch steps in every standing state. In the first playtest a steady orc never stepped:
                // its own swing threw its weight 0.5–0.7 m past its feet and it toppled where it stood.
                float factor = kind == UnitStateKind.Steady ? profile.steadyStepFactor : 1f;
                if (!Balance.UpdateCatchSteps(GroundY, Scale, factor))
                {
                    State.ForceKnockdown(Balance.LastVerdict);
                }
                else if (kind == UnitStateKind.Steady && !Stance.Stepping)
                {
                    SettleFeet();
                }
            }

            Stance.Update(dt);
            if (releaseArmTimer > 0f)
            {
                releaseArmTimer -= dt;
            }

            Pose.Compose(new PoseRequest
            {
                root = transform,
                state = State.Kind,
                getUpProgress = State.GetUpProgress,
                action = action,
                time = Time.time,
                scale = Scale,
                releaseWeaponArm = releaseArmTimer > 0f,
                holdingWeapon = Weapon != null,
            });

            Muscles.SetTone(State.MuscleTone);
            Muscles.ClearBoosts();
            if (Planner.Active)
            {
                var attack = Planner.Definition;
                if (Planner.Current == AttackPlanner.Phase.Strike || Planner.Current == AttackPlanner.Phase.Follow)
                {
                    Muscles.SetBoost(MuscleGroup.ArmR, attack.strikeArmBoost);
                }
                else if (Planner.Current == AttackPlanner.Phase.Recover)
                {
                    Muscles.SetBoost(MuscleGroup.ArmR, attack.recoverArmStrength);
                }
            }

            if (releaseArmTimer > 0f)
            {
                Muscles.SetBoost(MuscleGroup.ArmR, 0.15f);
            }

            if (boostRecomputeIn > 0 && --boostRecomputeIn == 0)
            {
                Muscles.RecomputeSubtreeBoost();
            }

            Muscles.StanceLeft = IsPlanted(PartId.FootL);
            Muscles.StanceRight = IsPlanted(PartId.FootR);
            Muscles.Step(dt);
            // Getting up, the body may lie on anything but its feet: the support still lifts it.
            Balance.ApplyForces(State.Balance, Grounded || kind == UnitStateKind.GettingUp);
        }

        /// <summary>A foot touching the ground (or anything) and not mid-step: its leg is carrying weight.</summary>
        private bool IsPlanted(PartId foot)
        {
            if (Stance.Stepping && Stance.SteppingFoot == foot)
            {
                return false;
            }

            var part = Rig.Part(foot);
            float last = Mathf.Max(part.LastGroundContactTime, part.LastOtherContactTime);
            return Time.time - last < 0.1f;
        }

        /// <summary>
        /// While steady, steps a foot that has drifted far from its rest spot under the root back
        /// into place (one at a time), so the stance tidies itself after a stagger or a shove.
        /// </summary>
        private void SettleFeet()
        {
            float limit = 0.22f * Scale;
            foreach (var foot in new[] { PartId.FootL, PartId.FootR })
            {
                Vector3 rest = transform.TransformPoint(foot == PartId.FootL ? Stance.RestLocalL : Stance.RestLocalR);
                Vector3 planted = Stance.Planted(foot);
                Vector3 offset = rest - planted;
                offset.y = 0f;
                if (offset.magnitude > limit)
                {
                    if (PlaytestTrace.Enabled)
                    {
                        PlaytestTrace.Note(this, "step", $"settle step {foot}: {offset.magnitude:0.00} m from its rest spot (limit {limit:0.00} m)");
                    }

                    rest.y = planted.y;
                    Stance.BeginStep(foot, rest, profile.stepDuration * 1.4f, profile.stepHeight * Scale);
                    Stance.StepsTaken = 0;
                    return;
                }
            }
        }

        private float ProbeGround(float fallback)
        {
            Vector3 from = Rig.Part(PartId.Pelvis).body.position + Vector3.up * 0.5f;
            int mask = groundMask != 0 ? groundMask : ~0;
            if (Physics.Raycast(from, Vector3.down, out var hit, 6f, mask, QueryTriggerInteraction.Ignore)
                && !Rig.Owns(hit.collider))
            {
                return hit.point.y;
            }

            return fallback;
        }

        private void OnStateChanged(UnitStateKind previous, UnitStateKind next)
        {
            switch (next)
            {
                case UnitStateKind.Knockdown:
                case UnitStateKind.Dead:
                    if (Planner.Active)
                    {
                        Planner.Abandon($"unit went into {next}");
                        releaseArmTimer = 0.6f;
                    }

                    break;

                case UnitStateKind.GettingUp:
                    StandUpWhereTheBodyLies();
                    break;

                case UnitStateKind.Steady:
                    Stance.StepsTaken = 0;
                    break;
            }
        }

        /// <summary>Moves the root under the fallen body and turns it the way the body lies.</summary>
        private void StandUpWhereTheBodyLies()
        {
            Vector3 pelvis = Rig.Part(PartId.Pelvis).body.position;
            Vector3 head = Rig.Part(PartId.Head).body.position;
            Vector3 facing = Vector3.ProjectOnPlane(Rig.Part(PartId.Chest).body.rotation * Vector3.forward, Vector3.up);
            Vector3 along = Vector3.ProjectOnPlane(head - pelvis, Vector3.up);
            bool headWay = along.sqrMagnitude > facing.sqrMagnitude;
            if (headWay)
            {
                facing = along;   // lying down: rise facing the way the head points
            }

            if (facing.sqrMagnitude > 1e-4f)
            {
                transform.rotation = Quaternion.LookRotation(facing.normalized, Vector3.up);
            }

            transform.position = new Vector3(pelvis.x, GroundY, pelvis.z);
            Stance.Plant(transform, GroundY);

            if (PlaytestTrace.Enabled)
            {
                var chest = Rig.Part(PartId.Chest).body;
                PlaytestTrace.Note(this, "getup",
                    $"getting up: {DescribeLying(chest.rotation)}, pelvis {pelvis.y - GroundY:0.00} m up (rest {restPelvisHeight:0.00} m), "
                    + $"head {head.y - GroundY:0.00} m, chest tilt {Vector3.Angle(chest.rotation * Vector3.up, Vector3.up):0}°; "
                    + $"root moved under the pelvis, facing {(headWay ? "where the head points" : "the chest's way")}");
            }
        }

        /// <summary>How a body lies, from its chest: "lying on its back", "lying face down", "on its side", "upright".</summary>
        public static string DescribeLying(Quaternion chestRotation)
        {
            float tilt = Vector3.Angle(chestRotation * Vector3.up, Vector3.up);
            if (tilt < 40f)
            {
                return "upright";
            }

            if (tilt < 65f)
            {
                return "bent over or sitting (chest tilted " + Mathf.RoundToInt(tilt) + "°)";
            }

            float frontUp = Vector3.Dot(chestRotation * Vector3.forward, Vector3.up);
            return frontUp > 0.5f ? "lying on its back" : frontUp < -0.5f ? "lying face down" : "lying on its side";
        }

        /// <summary>Called by a body part when it collides with something.</summary>
        public void OnPartCollision(BodyPart part, Collision collision, bool entered)
        {
            if (!initialized)
            {
                return;
            }

            var other = collision.collider;
            if (other == null || Rig.Owns(other) || (Weapon != null && Weapon.Owns(other)))
            {
                return;
            }

            if (PartInfo.IsFoot(part.id))
            {
                lastGroundedTime = Time.time;
            }

            if (entered)
            {
                ContactEntered?.Invoke(part, collision);
            }

            // Weapon on weapon is a clash, not a hit on the hand holding the other one: the physics
            // deflects both and nobody takes damage.
            var otherWeapon = other.GetComponentInParent<Weapon>();
            var otherPart = otherWeapon == null ? other.GetComponentInParent<BodyPart>() : null;
            if (Weapon != null && part == Weapon.Hand && otherPart != null && otherPart.Owner != null && otherPart.Owner != this)
            {
                for (int i = 0; i < collision.contactCount; i++)
                {
                    var contact = collision.GetContact(i);
                    if (Weapon.Owns(contact.thisCollider))
                    {
                        ImpactSystem.ResolveWeaponContact(this, Weapon, otherPart, contact, entered);
                        return;
                    }
                }
            }

            // Shoves: other units' bodies and props. A weapon striking us is the impact system's job.
            if (entered && collision.rigidbody != null && otherWeapon == null)
            {
                ImpactSystem.ResolveShove(this, part, collision);
            }
        }

        public bool CanHit(UnitController victim, float cooldown)
        {
            return !lastHitTimes.TryGetValue(victim, out float last) || Time.time - last >= cooldown;
        }

        public void MarkHit(UnitController victim)
        {
            lastHitTimes[victim] = Time.time;
        }

        /// <summary>This unit was hit: lose health and stability, and soften where it landed.</summary>
        public void ReceiveHit(in HitEvent hit)
        {
            State.TakeDamage(hit.damage);
            State.LoseStability(hit.stabilityLoss);
            Muscles.Dip(hit.part);
            Hit?.Invoke(hit);
        }

        /// <summary>This unit's weapon landed a hit.</summary>
        public void OnWeaponLanded(in HitEvent hit, UnitController victim)
        {
            Planner.OnSolidHit();
            if (victim.Rig.TotalMass() > Rig.TotalMass() * 1.3f)
            {
                // Hitting something much heavier: the arm bounces off.
                Muscles.Dip(PartId.ForearmR, 0.6f);
            }
        }

        /// <summary>A debug shove (the sandbox's push tool).</summary>
        public void Push(BodyPart part, Vector3 impulse, Vector3 point)
        {
            part.body.AddForceAtPosition(impulse, point, ForceMode.Impulse);
            State.LoseStability(impulse.magnitude * 0.12f);
            Muscles.Dip(part.id, 0.6f);
        }

        private void OnDrawGizmos()
        {
            if (!showGhost || !initialized)
            {
                return;
            }

            foreach (var id in PartInfo.All)
            {
                if (!PartInfo.HasParent(id))
                {
                    continue;
                }

                var parent = PartInfo.Parent(id);
                Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.9f);
                Gizmos.DrawLine(Ghost.Part(parent).position, Ghost.Part(id).position);
                Gizmos.color = new Color(1f, 1f, 1f, 0.6f);
                Gizmos.DrawLine(Rig.Part(parent).transform.position, Rig.Part(id).transform.position);
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Balance.CapturePoint, 0.05f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(Balance.SupportCenter, 0.05f);

            if (Planner.Active)
            {
                var plan = Planner.Plan;
                var reach = Planner.Reach;
                Gizmos.color = Planner.Committed ? new Color(1f, 0.3f, 0.3f) : new Color(1f, 0.4f, 1f);
                Vector3 previous = Vector3.zero;
                for (float theta = Planner.Definition.loadAngle; theta <= Planner.Definition.followAngle; theta += 10f)
                {
                    Vector3 point = plan.shoulder + SwingSolver.Direction(plan, theta) * (plan.contactReach + reach.sweetSpot);
                    if (theta > Planner.Definition.loadAngle)
                    {
                        Gizmos.DrawLine(previous, point);
                    }

                    previous = point;
                }

                Gizmos.DrawWireSphere(plan.target, 0.08f);
            }
        }
    }
}
