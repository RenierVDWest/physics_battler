using System;
using System.Collections.Generic;
using PhysicsBattler.Authority;
using PhysicsBattler.Combat;
using PhysicsBattler.Diagnostics;
using PhysicsBattler.Motion;
using PhysicsBattler.Muscles;
using PhysicsBattler.Rig;
using UnityEngine;

namespace PhysicsBattler.Units
{
    /// <summary>What a blow did when it reached its contact time: how close it came, and what was there.</summary>
    public struct ContactReport
    {
        /// <summary>Striking part to aim point on the animated pose, at contact (the stretch swept that step), metres.</summary>
        public float AnimError;

        /// <summary>Unused at contact (-1): the physical weapon's closest pass is <see cref="MoveState.PhysicalClosest"/>, known when the blow ends.</summary>
        public float PhysicalError;

        /// <summary>The body part nearest the striking part at contact (any unit), and how far from it.</summary>
        public UnitController StruckUnit;
        public PartId StruckPart;
        public float StruckDistance;

        /// <summary>How far the aimed-at null had moved since the aim was locked (the target's sway, dodge or step).</summary>
        public float TargetMoved;
    }

    /// <summary>
    /// One unit. Its transform is the gameplay root: where it stands on the board (on the ground)
    /// and which way it faces. Every physics step it:
    ///
    ///  1. turns toward what it faces (gameplay; during a blow, toward the locked aim point),
    ///  2. advances its blow, if any: the lunge's planned steps, the contact moment,
    ///  3. poses its animation rig (<see cref="PosePipeline"/>: stance, idle, guard, the blow, weight, legs, arms),
    ///  4. drives the physical parts toward that pose (<see cref="AuthorityController"/>): legs and
    ///     pelvis kinematic, so they always carry the body; torso, head and arms pinned, so they
    ///     reach the pose on time but can still be knocked about.
    ///
    /// Physics never decides whether the unit is standing; the pose is valid without it. The same
    /// pose code also runs in edit mode for the scene's lineup preview (<see cref="InitializePreview"/>).
    /// </summary>
    [DefaultExecutionOrder(-100)]
    [DisallowMultipleComponent]
    public partial class UnitController : MonoBehaviour
    {
        [SerializeField] private UnitProfile profile;
        [SerializeField] private int team;

        private static readonly List<UnitController> all = new List<UnitController>();
        private static Transform bodiesContainer;
        private static int spawnCount;   // seeds each unit's idle and shuffles differently

        /// <summary>The next units made get the same seeds as the first ones ever made (tests that must not depend on what ran before them).</summary>
        public static void ResetSeeds(int count = 0) => spawnCount = count;
        private GameObject detachedBody;
        private float facingYaw;
        private Vector3? facePoint;
        private bool initialized;
        private bool physical;

        /// <summary>Whether its body is simulated (a playing unit; not a preview).</summary>
        public bool IsPhysical => physical;

        /// <summary>The shield's mass put on the shield hand's body (kilograms; taken off again when it is unequipped).</summary>
        private float shieldMassOnHand;

        public static event Action<UnitController> Initialized;
        public static event Action<UnitController, MoveState> MoveStarted;
        public static event Action<UnitController, MoveState, ContactReport> MoveContact;
        public static event Action<UnitController, MoveState> MoveEnded;

        /// <summary>Every unit in play.</summary>
        public static IReadOnlyList<UnitController> All => all;

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
        public Loadout Loadout { get; private set; }
        public Weapon MainWeapon { get; private set; }
        public Weapon OffWeapon { get; private set; }

        /// <summary>The blow under way, or null.</summary>
        public MoveState CurrentMove { get; private set; }
        public bool IsAttacking => CurrentMove != null;

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

            Initialize(true);
        }

        /// <summary>
        /// Edit mode: builds the pose machinery without touching physics or the hierarchy, so the
        /// unit can be posed (<see cref="PreviewPose"/>) as a preview in the scene.
        /// </summary>
        public bool InitializePreview()
        {
            return initialized || Initialize(false);
        }

        private bool Initialize(bool asPhysical)
        {
            physical = asPhysical;
            Rig = GetComponent<UnitRig>();
            if (Rig == null || !Rig.IsComplete())
            {
                Debug.LogError($"[Unit] {name} has an incomplete rig. Run Physics Battler > Setup > Rebuild Prefabs.", this);
                enabled = false;
                return false;
            }

            if (profile == null)
            {
                profile = UnitProfile.CreateDefault();
            }

            Style = profile.style != null ? profile.style : CombatStyle.CreateDefault();
            foreach (var part in Rig.Parts)
            {
                part.Owner = this;
            }

            if (physical)
            {
                JointConventions.EnsureProbed();
                var bodyMaterial = PhysicsMaterials.Get("Body", profile.bodyFriction);
                var footMaterial = PhysicsMaterials.Get("Foot", profile.footFriction);
                foreach (var part in Rig.Parts)
                {
                    UnitBuilder.ConfigureBody(part.body, profile);
                    part.limits = JointLimitTable.Physical(part.id);   // (the table as it is now, not as the prefab was baked)
                    if (part.joint != null)
                    {
                        // Milestone 10: each joint in its own frame (the arm rig's: the elbow a hinge
                        // about its axis, the wrist twisting about the forearm), set up here in the
                        // rest pose the body is still in.
                        part.jointFrame = JointFrames.For(Rig, part.id);
                        JointFrames.Apply(part.joint, part.jointFrame);
                        JointMath.ApplyLimits(part.joint, part.limits);
                    }

                    if (part.partCollider != null)
                    {
                        part.partCollider.sharedMaterial = PartInfo.IsFoot(part.id) ? footMaterial : bodyMaterial;
                    }
                }
            }

            // The animation rig is cloned while the body is still in its rest pose under this object.
            Anim = new AnimRig(transform, Rig);
            if (!physical)
            {
                Anim.Root.gameObject.hideFlags = HideFlags.HideAndDontSave;
            }

            Body = BodyMeasure.Of(transform, Anim, Rig);
            Loadout = new Loadout { Scale = Body.SizeFactor };

            if (physical)
            {
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
                InitializeGround();
            }

            int seed = 7919 * team + 104729 * ++spawnCount;
            unitNumber = spawnCount;
            InitializeCombat(seed);
            Stance = new Stance(seed);
            Stance.Configure(Style, Body);
            Idle = new CombatIdle(seed + 1);
            Pose = new PosePipeline(transform, Anim, Body, Stance, Idle, Style) { Loadout = Loadout, IsSevered = Rig.IsSevered, Rig = Rig };
            Authority = new AuthorityController(Rig, Anim);
            Authority.Set(BodyRegion.Legs, AuthorityLevel.Kinematic, 1f);
            Authority.Set(BodyRegion.Torso, AuthorityLevel.Pinned, profile.torsoPin);
            Authority.Set(BodyRegion.Head, AuthorityLevel.Pinned, profile.torsoPin);
            Authority.Set(BodyRegion.ArmL, AuthorityLevel.Pinned, profile.armPin);
            Authority.Set(BodyRegion.ArmR, AuthorityController.ArmMuscles ? AuthorityLevel.Muscled : AuthorityLevel.Pinned, profile.armPin, AuthorityController.ArmMuscles);
            InitializeBalance();
            InitializeRhythm(seed);
            InitializeTempo(seed);
            InitializePerception(seed);
            InitializeGait();

            facingYaw = transform.eulerAngles.y;
            Stance.GroundY = transform.position.y;
            Stance.Plant(transform.position, facingYaw);

            // Start in the pose, so nothing snaps on the first step.
            Pose.Compose(0f, 0f);
            SnapBodyToPose();

            initialized = true;
            if (physical)
            {
                all.Add(this);
                Initialized?.Invoke(this);
            }

            return true;
        }

        private void OnDestroy()
        {
            all.Remove(this);
            ForgetGround();
            DestroySeveredLimbs();
            if (detachedBody != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(detachedBody);
                }
                else
                {
                    DestroyImmediate(detachedBody);
                }
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

        /// <summary>Moves every physical part straight onto the pose (start, and edit-mode previews).</summary>
        private void SnapBodyToPose()
        {
            foreach (var id in PartInfo.All)
            {
                if (Rig.IsSevered(id))
                {
                    continue;
                }

                var part = Rig.Part(id);
                part.transform.SetPositionAndRotation(Anim.Part(id).position, Anim.Part(id).rotation);
                if (physical)
                {
                    part.body.position = Anim.Part(id).position;
                    part.body.rotation = Anim.Part(id).rotation;
                }
            }

            if (physical)
            {
                Physics.SyncTransforms();
            }
        }

        // ---- Equipment -------------------------------------------------------------------------

        /// <summary>Puts weapons in the hands: a main weapon (right hand) and an off-hand item (left), either may be null.</summary>
        public void Equip(WeaponDefinition main, WeaponDefinition off)
        {
            if (MainWeapon != null)
            {
                DestroyWeapon(MainWeapon);
            }

            if (OffWeapon != null)
            {
                DestroyWeapon(OffWeapon);
            }

            // (Not into a hand that has been cut off.)
            if (Rig.IsSevered(PartId.HandR))
            {
                main = null;
            }

            if (Rig.IsSevered(PartId.HandL))
            {
                off = null;
            }

            Loadout.Main = main;
            Loadout.Off = off;

            // Milestone 9b: a shield is held by its centre grip, the bar across the fist and the forearm
            // pointing at the boss (its face follows the forearm); a weapon in a hammer grip, as before.
            var grip = off != null && off.IsShield ? ShieldGrip : Quaternion.identity;
            if (Rig.SlotWeaponL != null)
            {
                Rig.SlotWeaponL.localRotation = grip;
            }

            if (Anim.SlotWeaponL != null)
            {
                Anim.SlotWeaponL.localRotation = grip;
            }

            // Milestone 9c: a handle crosses the fist diagonally, so with the wrist straight the blade
            // leans toward the forearm's line (toward the fingers).
            var mainGrip = main != null && !main.IsShield ? MainGrip : Quaternion.identity;
            if (Rig.SlotWeaponR != null)
            {
                Rig.SlotWeaponR.localRotation = mainGrip;
            }

            if (Anim.SlotWeaponR != null)
            {
                Anim.SlotWeaponR.localRotation = mainGrip;
            }

            MainWeapon = main != null ? Weapon.Equip(main, this, Rig.SlotWeaponR, true, Loadout.Scale) : null;
            OffWeapon = off != null ? Weapon.Equip(off, this, Rig.SlotWeaponL, false, Loadout.Scale) : null;

            // Milestone 9c: the hand carries the shield's weight (a blow on it moves a heavy thing).
            var shieldHand = Rig.Part(PartId.HandL).body;
            if (shieldHand != null)
            {
                shieldHand.mass = Mathf.Max(0.05f, shieldHand.mass - shieldMassOnHand);
                shieldMassOnHand = physical && off != null && off.IsShield ? off.massKg * Loadout.Scale * Loadout.Scale * Loadout.Scale : 0f;
                shieldHand.mass += shieldMassOnHand;
            }
            if (!physical)
            {
                foreach (var w in new[] { MainWeapon, OffWeapon })
                {
                    if (w != null)
                    {
                        foreach (var t in w.GetComponentsInChildren<Transform>(true))
                        {
                            t.gameObject.hideFlags = HideFlags.DontSave;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The shield's turn in the hand (centre grip): its face (forward) along the forearm, away from
        /// the elbow; its bar (its up) through the fist along the knuckles, as a sword's grip is. With
        /// the forearm raised the thumb is on top and the wrist straight (the bar stands upright).
        /// </summary>
        public static readonly Quaternion ShieldGrip = Quaternion.LookRotation(Vector3.down, Vector3.forward);

        /// <summary>A weapon's turn in the hand: the hammer grip, its blade leaning <see cref="PosePipeline.GripLean"/> toward the fingers.</summary>
        public static readonly Quaternion MainGrip = Quaternion.Euler(PosePipeline.GripLean, 0f, 0f);

        private static void DestroyWeapon(Weapon weapon)
        {
            weapon.SetLying(false);
            foreach (var collider in weapon.GetComponentsInChildren<Collider>(true))
            {
                GroundContacts.SetShield(collider, false);
            }

            if (Application.isPlaying)
            {
                Destroy(weapon.gameObject);
            }
            else
            {
                DestroyImmediate(weapon.gameObject);
            }
        }

        // ---- Gameplay ----------------------------------------------------------------------------

        /// <summary>Face a point on the board (the unit turns at its turn speed, stepping as it goes).</summary>
        public void FacePoint(Vector3 point) => facePoint = point;

        /// <summary>Place the unit (gameplay): the feet re-plant under it.</summary>
        public void Place(Vector3 position, float yaw)
        {
            transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            facingYaw = yaw;
            Stance.GroundY = position.y;
            Stance.Plant(position, yaw);
            hasLastRoot = false;
        }

        /// <summary>Moves the unit somewhere else at once, body and all (the sandbox's scenarios).</summary>
        public void Teleport(Vector3 position, float yaw)
        {
            if (CurrentMove != null)
            {
                CancelMove(CurrentMove, "moved by the sandbox");
            }

            DropCutShort();
            StopRhythm();
            StopGait();
            ResetBalance();
            Place(position, yaw);
            Knockback = Vector3.zero;
            threats.Clear();
            incoming = null;
            incomingAttacker = null;
            shieldPlaced = false;
            facePoint = null;
            Look.Reset();
            Pose.Compose(LocalTime, 0f, true);
            SnapBodyToPose();
            foreach (var part in Rig.Parts)
            {
                if (part.body != null && !part.body.isKinematic && !Rig.IsSevered(part.id))
                {
                    part.body.linearVelocity = Vector3.zero;
                    part.body.angularVelocity = Vector3.zero;
                }
            }
        }

        /// <summary>How far the unit still has to turn to face a point (degrees).</summary>
        public float AngleTo(Vector3 point)
        {
            Vector3 to = point - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude < 1e-6f)
            {
                return 0f;
            }

            return Mathf.Abs(Mathf.DeltaAngle(facingYaw, Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg));
        }

        /// <summary>Starts a blow prepared by <see cref="MovePlanner"/>. False if one is already under way.</summary>
        public bool StartMove(MoveState state) => StartMove(state, 0f);

        /// <summary>Starts a blow at <paramref name="startAt"/> of its way (normalized time; milestone 10: a combo's link starts part way into its wind-up).</summary>
        public bool StartMove(MoveState state, float startAt)
        {
            if (CurrentMove != null || state == null || !CanAct)
            {
                return false;   // (off its feet, it can't strike)
            }

            // (The arm rebuild: the path made the blow's real way before it starts, for the shield and the sweeps.)
            SwingPath.PathFromPose(this, state);
            state.Time = Mathf.Clamp01(startAt) * state.Duration;
            state.ContactPassed = state.SteppedOut = state.LeadOut = state.SteppedBack = state.RearMoved = state.RearOut = state.RearBack = false;
            state.HasLastStrike = state.HasLastPhysical = state.HasLastRadial = state.HasContactShift = false;
            state.ContactSpeed = state.PeakSpeed = state.Depth = state.Arc = state.LastProgress = 0f;
            state.ContactWay = Vector3.zero;
            state.PhysicalClosest = -1f;
            CurrentMove = state;
            Pose.Move = state;
            Rhythm?.NoteBlow();
            TempoBlowStarted();
            BeginSweep();
            Stance.Hold = true;
            MoveStarted?.Invoke(this, state);
            if (state.Target is UnitController target && target != this)
            {
                target.OnIncoming(this, state);
            }

            return true;
        }

        /// <summary>
        /// Poses the unit at a blow's moment of contact, as if it had started now (the planned lunge
        /// step landed), measures how far the striking part ends from the aim point, and puts
        /// everything back. The planner keeps only blows that land.
        /// </summary>
        public float TryOut(MoveState state) => TryOut(state, false);

        /// <param name="keep">Leave the unit in the contact pose (debugging, the move preview).</param>
        public float TryOut(MoveState state, bool keep)
        {
            using (Perf.Begin(PerfSection.TryOut))
            {
                return PoseDry(state, state.ContactTime, keep) ? Pose.AimError : float.MaxValue;
            }
        }

        /// <summary>
        /// Tries a blow from start to finish (in <paramref name="samples"/> poses, everything put back
        /// after) and returns how clear of its own body the weapon and weapon arm stay (metres;
        /// negative: they pass through it), and where it comes closest.
        /// </summary>
        public float Clearance(MoveState state, int samples, out PartId closest, out float when, out bool weapon)
        {
            using (Perf.Begin(PerfSection.Clearance))
            {
                return ClearanceCore(state, samples, out closest, out when, out weapon);
            }
        }

        private float ClearanceCore(MoveState state, int samples, out PartId closest, out float when, out bool weapon)
        {
            float best = float.MaxValue;
            closest = PartId.Chest;
            when = 0f;
            weapon = false;
            float length = state.Weapon != null ? state.Weapon.length * 0.98f : 0.8f;
            for (int i = 0; i < samples; i++)
            {
                float t = Mathf.Lerp(0.04f, 0.96f, i / (float)Mathf.Max(1, samples - 1));
                var saved = Stance.Save();
                var anim = SaveAnim();
                var rotation = transform.rotation;
                try
                {
                    PoseDry(state, t, true);
                    float c = BodyClearance.Measure(Rig, Anim, length, Loadout.Scale, out var part, out bool blade);
                    if (c < best)
                    {
                        best = c;
                        closest = part;
                        when = t;
                        weapon = blade;
                    }
                }
                finally
                {
                    transform.rotation = rotation;
                    Stance.Restore(saved);
                    RestoreAnim(anim);
                }
            }

            return best;
        }

        /// <summary>
        /// Where the weapon is at a moment of a planned blow, posed dry and put back: its grip and
        /// the striking point (world).
        /// </summary>
        public void WeaponAt(MoveState state, float normalizedTime, out Vector3 grip, out Vector3 strike)
        {
            var saved = Stance.Save();
            var anim = SaveAnim();
            var rotation = transform.rotation;
            try
            {
                PoseDry(state, normalizedTime, true);
                grip = Anim.SlotWeaponR.position;
                strike = Pose.StrikePoint;
            }
            finally
            {
                transform.rotation = rotation;
                Stance.Restore(saved);
                RestoreAnim(anim);
            }
        }

        /// <summary>
        /// Poses the unit (dry: the feet don't step) at a moment of a blow, as if it had started now:
        /// facing along the aim frame, the lunge's feet planted once the lunge is under way.
        /// </summary>
        /// <summary>
        /// The arm rebuild: solves where the hand must be at a blow's striking moment (the body posed
        /// there) so its striking part lands on the aim, with the contact's wrist and elbow keys.
        /// </summary>
        public void PlanContactHand(MoveState state)
        {
            if (state == null || !state.Generated || Loadout?.Main == null)
            {
                return;
            }

            var saved = Stance.Save();
            var anim = SaveAnim();
            var rotation = transform.rotation;
            try
            {
                state.HasContactHand = false;
                PoseDry(state, state.ContactTime, true);
                Pose.PlanContactHand(state, Loadout.Scale);
            }
            finally
            {
                transform.rotation = rotation;
                Stance.Restore(saved);
                RestoreAnim(anim);
            }
        }

        /// <summary>Poses a planned blow dry at a moment and leaves the animation rig there (the reports' dumps).</summary>
        public bool PoseDryAt(MoveState state, float normalizedTime) => PoseDry(state, normalizedTime, true);

        private bool PoseDry(MoveState state, float normalizedTime, bool keep)
        {
            var saved = Stance.Save();
            var anim = SaveAnim();
            var previousMove = Pose.Move;
            var rotation = transform.rotation;
            (Vector3, Vector3, Vector3, Vector3, Vector3, Vector3, Vector3)? knocks = null;
            try
            {
                float yaw = Quaternion.LookRotation(state.Frame * Vector3.forward).eulerAngles.y;
                transform.rotation = Quaternion.Euler(0f, yaw, 0f);
                var at = state.CopyAt(normalizedTime);
                if (NeedsLungeStep(state) && at.Body > 0.5f)
                {
                    LungeFeet(state, yaw, out int first, out var firstTo, out int second, out var secondTo, out bool secondMoves);
                    Stance.SetFoot(first, firstTo, yaw + Stance.HomeYawLocal(first));
                    if (secondMoves)
                    {
                        Stance.SetFoot(second, secondTo, yaw + Stance.HomeYawLocal(second));
                    }
                }

                // (Tried as it will play: the knocks and recoils of the moment fade before contact.)
                knocks = (Pose.ReactShift, Pose.ReactLean, Pose.ReactHead, Pose.KnockR, Pose.KnockTurnR, Pose.KnockL, Pose.KnockTurnL);
                Pose.ReactShift = Pose.ReactLean = Pose.ReactHead = Pose.KnockR = Pose.KnockTurnR = Pose.KnockL = Pose.KnockTurnL = Vector3.zero;
                Pose.Move = at;
                Pose.Compose(LocalTime, 0f, true);
                return true;
            }
            finally
            {
                if (knocks.HasValue)
                {
                    (Pose.ReactShift, Pose.ReactLean, Pose.ReactHead, Pose.KnockR, Pose.KnockTurnR, Pose.KnockL, Pose.KnockTurnL) = knocks.Value;
                }

                Pose.Move = previousMove;
                if (!keep)
                {
                    transform.rotation = rotation;
                    Stance.Restore(saved);
                    RestoreAnim(anim);
                }
            }
        }

        private bool NeedsLungeStep(MoveState state) => state.Lunge.magnitude > 0.06f * Body.SizeFactor || state.Move.passingStep;

        /// <summary>How much longer than its stance a lunge may stretch the feet (fraction of leg length).</summary>
        private const float LungeStretch = 0.3f;

        /// <summary>
        /// Where a lunge puts the feet at contact: the lead foot out by the whole lunge, and, for a
        /// long one, the rear foot following so the stance stretches by at most
        /// <see cref="LungeStretch"/> of the leg (an advance-lunge). The hips then stay high enough
        /// for the arm to reach up.
        /// </summary>
        /// <remarks>
        /// A passing step (big swings, thrusts) goes the other way round: the rear foot steps through
        /// to where a lead foot on its side would stand, out by the lunge, and the old lead foot
        /// follows to the rear only if the lunge leaves it too far behind. The feet step back home
        /// in the recovery either way.
        /// </remarks>
        private void LungeFeet(MoveState state, float yaw, out int first, out Vector3 firstTo, out int second, out Vector3 secondTo, out bool secondMoves)
        {
            int lead = Stance.Lead;
            Vector3 root = transform.position;
            var facing = Quaternion.Euler(0f, yaw, 0f);
            if (state.Move.passingStep)
            {
                first = 1 - lead;
                second = lead;
                Vector3 leadHome = Stance.HomeLocal(lead), rearHome = Stance.HomeLocal(1 - lead);
                firstTo = root + state.Lunge + facing * new Vector3(-leadHome.x, 0f, leadHome.z);
                firstTo.y = Stance.GroundY;
                secondTo = root + state.Lunge + facing * new Vector3(-rearHome.x, 0f, rearHome.z);
                secondTo.y = Stance.GroundY;
                secondMoves = Vector3.Distance(Stance.Position(lead), secondTo) > 0.25f * Body.LegLength;
                return;
            }

            first = lead;
            second = 1 - lead;
            firstTo = Stance.HomeWorld(lead, root + state.Lunge, yaw);
            float follow = state.Lunge.magnitude - LungeStretch * Body.LegLength;
            secondMoves = follow > 0.05f * Body.LegLength;
            Vector3 rearRoot = root + (secondMoves ? state.Lunge.normalized * follow : Vector3.zero);
            secondTo = Stance.HomeWorld(1 - lead, rearRoot, yaw);
        }

        private (Vector3, Quaternion)[] SaveAnim()
        {
            var saved = new (Vector3, Quaternion)[PartInfo.Count];
            foreach (var id in PartInfo.All)
            {
                var t = Anim.Part(id);
                saved[(int)id] = (t.localPosition, t.localRotation);
            }

            return saved;
        }

        private void RestoreAnim((Vector3, Quaternion)[] saved)
        {
            foreach (var id in PartInfo.All)
            {
                var t = Anim.Part(id);
                t.localPosition = saved[(int)id].Item1;
                t.localRotation = saved[(int)id].Item2;
            }
        }

        // ---- The step ----------------------------------------------------------------------------

        private void FixedUpdate()
        {
            if (!initialized || !physical)
            {
                return;
            }

            using (Perf.Begin(PerfSection.UnitStep))
            {
                Step(Time.fixedDeltaTime, true);
            }
        }

        /// <summary>Edit mode: advance the pose by a time step and put the body (meshes) on it.</summary>
        public void PreviewPose(float dt)
        {
            if (!initialized)
            {
                return;
            }

            Step(dt, false);
            SnapBodyToPose();
        }

        private void Step(float dt, bool drive)
        {
            UpdateSeveredCollisions(dt);
            UpdateGround(dt);
            if (Combo != null && !CanAct)
            {
                EndCombo(IsAlive ? "off its feet" : "killed");
            }

            if (!IsAlive)
            {
                // Dead: a ragdoll, physics has every part.
                if (drive)
                {
                    SetAuthority(null);
                    Authority.Apply(dt);
                }

                return;
            }

            UpdateBleeding(dt);
            if (!IsAlive)
            {
                return;   // (bled out this step)
            }

            // Milestone 9: what it perceives (from its head, as the last step left it).
            UpdatePerception();

            // Milestone 4: on the ground (a ragdoll), or getting up (the get-up's own pose).
            if (State == UnitState.Down)
            {
                DropLook();
                StepDown(dt, drive);
                return;
            }

            if (State == UnitState.GettingUp)
            {
                DropLook();
                StepGetUp(dt, drive);
                return;
            }

            // Hit stop: the pose and the blow stand still for a moment.
            float poseDt = CombatTime(dt);
            LocalTime += poseDt;
            var move = CurrentMove;
            if (move != null)
            {
                facePoint = move.Aim;
            }
            else if (turnToThreat.HasValue)
            {
                facePoint = turnToThreat.Value;   // (a seen blow the shield can't reach yet: it turns to face it)
            }

            StateTime += poseDt;
            if (facePoint.HasValue && State == UnitState.Standing)
            {
                Vector3 to = facePoint.Value - transform.position;
                to.y = 0f;
                if (to.sqrMagnitude > 1e-4f)
                {
                    float wanted = Mathf.Atan2(to.x, to.z) * Mathf.Rad2Deg;
                    facingYaw = Mathf.MoveTowardsAngle(facingYaw, wanted, profile.turnSpeed * poseDt);
                    transform.rotation = Quaternion.Euler(0f, facingYaw, 0f);
                }
            }

            CombatBeforePose(dt);
            UpdateGait(poseDt);
            UpdateTempo(poseDt);
            UpdateRhythm(poseDt);
            UpdateShield(dt);
            UpdateLook(poseDt);

            if (move != null)
            {
                // What the weapon struck since last step (as PhysX moved it); a stopped swing ends the blow.
                if (physical)
                {
                    using (Perf.Begin(PerfSection.Sweep))
                    {
                        Sweep(move, dt);
                    }
                }

                if (CurrentMove == move)
                {
                    TrackPhysicalWeapon(move);
                    AdvanceMove(move, poseDt * move.TimeScale);
                    if (drive && MainWeapon != null)
                    {
                        MainWeapon.ShowTrail(move.Move.hand == MoveHand.Main && move.Active && move.T <= move.Move.SwingFollowEnd + 0.02f);
                    }
                    move.TimeScale = Mathf.MoveTowards(move.TimeScale, 1f, 2f * poseDt);   // the swing picks up again after a cleave
                }
                else
                {
                    move = null;
                }
            }

            FadeCutShort(poseDt);
            BlendLink(poseDt);
            Pose.Compose(LocalTime, poseDt);
            SmoothHandOver(poseDt);

            if (move != null)
            {
                // The striking part moves several centimetres a step, so contact is judged on the
                // stretch it swept since the last step, as a hit would be.
                MeasureSwing(move, poseDt);
                if (!move.ContactPassed && move.T >= move.ContactTime)
                {
                    move.ContactPassed = true;
                    MoveContact?.Invoke(this, move, Report(move));
                }

                move.LastStrike = Pose.StrikePoint;
                move.LastProgress = move.Progress;
                move.LastDeviation = move.Generated ? Vector3.Distance(Pose.StrikePoint, move.PathPoint(move.Progress)) : 0f;
                move.HasLastStrike = true;
            }

            if (drive)
            {
                // A blow fading out after being cut short is still posed onto the weapon arm.
                SetAuthority(move ?? cutShort);
                using (Perf.Begin(PerfSection.Authority))
                {
                    Authority.Apply(dt);
                }
            }

            if (drive && MainWeapon != null && CurrentMove == null)
            {
                MainWeapon.ShowTrail(false);
            }

            if (move != null && move.Done && CurrentMove == move)
            {
                CurrentMove = null;
                Pose.Move = null;
                Stance.Hold = false;
                Tempo?.NoteBlowEnded();
                MoveEnded?.Invoke(this, move);
            }

            // Milestone 10: a combo's link hands on to the next.
            StepCombo();
        }

        /// <summary>The posed swing's speed, how far it carried on past the aim point and its arc, for the log.</summary>
        private void MeasureSwing(MoveState move, float dt)
        {
            if (!move.HasLastStrike || dt <= 0f)
            {
                return;
            }

            float speed = Vector3.Distance(Pose.StrikePoint, move.LastStrike) / dt;
            if (!move.ContactPassed && move.T >= move.ContactTime)
            {
                move.ContactSpeed = speed;
                move.ContactWay = speed > 1e-3f ? (Pose.StrikePoint - move.LastStrike).normalized : Vector3.zero;
            }

            if (move.T <= move.Move.SwingFollowEnd)
            {
                move.PeakSpeed = Mathf.Max(move.PeakSpeed, speed);
            }

            if (!move.Generated)
            {
                return;
            }

            float released = (move.Move.SwingTravel(0) + move.Move.SwingTravel(1)) / move.Move.SwingTotal;
            Vector3 pivot = move.PivotAt(move.Move.ContactProgress);
            if (move.T >= move.ContactTime && move.T <= move.Move.SwingFollowEnd)
            {
                // (The arm rebuild: a hand-keyed blow carries on the way it was going through the aim,
                // across the target for a cut, into it for a thrust; a path-made one, away from its pivot.)
                Vector3 way = move.HasContactHand && move.ContactWay != Vector3.zero ? move.ContactWay : (move.Aim - pivot).normalized;
                move.Depth = Mathf.Max(move.Depth, Vector3.Dot(Pose.StrikePoint - move.Aim, way));
            }

            Vector3 radial = Pose.StrikePoint - move.PivotAt(move.Progress);
            if (move.HasLastRadial && move.Progress >= released && move.T <= move.Move.SwingFollowEnd)
            {
                move.Arc += Vector3.Angle(move.LastRadial, radial);
            }

            move.LastRadial = radial;
            move.HasLastRadial = true;
        }

        /// <summary>The physical weapon as PhysX left it: its sweep since the last step, against the aim point.</summary>
        private void TrackPhysicalWeapon(MoveState move)
        {
            if (!StrikingPart(move, out _, out _, out Vector3 now))
            {
                return;
            }

            if (move.HasLastPhysical && move.Active)
            {
                float d = SegmentDistance(move.Aim, move.LastPhysical, now);
                move.PhysicalClosest = move.PhysicalClosest < 0f ? d : Mathf.Min(move.PhysicalClosest, d);
            }

            move.LastPhysical = now;
            move.HasLastPhysical = true;
        }

        /// <summary>
        /// The lunge's planned steps: the lead foot goes out, and for a long lunge the rear foot
        /// follows, both landing just before contact; they come back in the recovery, lead first.
        /// </summary>
        private void AdvanceMove(MoveState move, float dt)
        {
            move.Time += dt;
            lastAdvance = dt;
            if (!NeedsLungeStep(move))
            {
                return;
            }

            float tc = move.ContactTime;
            int lead = Stance.Lead;
            int rear = 1 - lead;
            float yaw = Quaternion.LookRotation(move.Frame * Vector3.forward).eulerAngles.y;
            LungeFeet(move, yaw, out int first, out var leadTo, out int second, out var rearTo, out bool rearMoves);
            // The lunge's steps go with the drive forward, after the wind-up's rock-back: the lead foot
            // out first and, on a long lunge, the rear foot after it (an advance-lunge), both down
            // just before contact. A short lunge moves only the lead foot.
            float drive = Mathf.Clamp(move.DriveStart - 0.04f, 0.02f, 0.8f * tc);
            float land = 0.97f * tc;
            if (!move.SteppedOut && move.T >= drive)
            {
                move.SteppedOut = true;
                move.RearMoved = rearMoves;
                move.LeadOut = true;
                move.LeadOutAt = drive;
                Stance.StepTo(first, leadTo, yaw + Stance.HomeYawLocal(first), (land - drive) * move.Duration);
            }

            float rearFrom = drive + 0.45f * (land - drive);
            if (move.RearMoved && !move.RearOut && move.T >= rearFrom)
            {
                move.RearOut = true;
                Stance.StepTo(second, rearTo, yaw + Stance.HomeYawLocal(second), (land - rearFrom) * move.Duration);
            }

            // Back home once the blow is spent: swings at the end of their follow-through.
            float back = move.Move.shape == MoveShape.Path ? tc + 0.3f * (1f - tc) : move.Move.SwingFollowEnd - 0.05f;
            float recovery = (1f - (move.Move.shape == MoveShape.Path ? tc : back)) * move.Duration;
            bool passing = move.Move.passingStep;
            if (!move.SteppedBack && move.T >= back)
            {
                move.SteppedBack = true;
                Stance.StepTo(lead, Stance.HomeWorld(lead, transform.position, facingYaw), facingYaw + Stance.HomeYawLocal(lead), 0.32f * recovery);
            }

            if ((move.RearMoved || passing) && !move.RearBack && move.T >= back + (passing ? 0.12f : 0.33f) * (1f - back))
            {
                move.RearBack = true;
                Stance.StepTo(rear, Stance.HomeWorld(rear, transform.position, facingYaw), facingYaw + Stance.HomeYawLocal(rear), 0.3f * recovery);
            }
        }

        private ContactReport Report(MoveState move)
        {
            Vector3 from = move.HasLastStrike ? move.LastStrike : Pose.StrikePoint;
            var report = new ContactReport
            {
                AnimError = SweptDistance(move, from, Pose.StrikePoint), PhysicalError = -1f, StruckDistance = float.MaxValue,
            };
            // What is there: the body part nearest the posed striking part (the physical weapon is a
            // physics step behind; its closest pass is reported when the blow ends).
            Vector3 strike = Pose.StrikePoint;

            foreach (var unit in all)
            {
                if (unit == this || !unit.initialized)
                {
                    continue;
                }

                foreach (var part in unit.Rig.Parts)
                {
                    if (part.partCollider == null)
                    {
                        continue;
                    }

                    float d = Vector3.Distance(part.partCollider.ClosestPoint(strike), strike);
                    if (d < report.StruckDistance)
                    {
                        report.StruckDistance = d;
                        report.StruckUnit = unit;
                        report.StruckPart = part.id;
                    }
                }
            }

            if (move.Target is UnitController target && target.initialized)
            {
                report.TargetMoved = Vector3.Distance(AimPartInfo.PhysicalPoint(target.Rig, move.Part), move.Aim);
            }

            return report;
        }

        /// <summary>
        /// How close the striking part passed the aim point between two steps: along the swing's path
        /// for a swing (a hand-first blow's: its posed way, fitted to the two steps), along the
        /// straight line otherwise.
        /// </summary>
        private static float SweptDistance(MoveState move, Vector3 from, Vector3 to)
        {
            if (!move.Generated || move.Move.shape != MoveShape.Swing)
            {
                return SegmentDistance(move.Aim, from, to);
            }

            if (move.HasContactHand)
            {
                // The arm rebuild: a hand-first blow's posed striking part between the two steps, along
                // the curve of its path (its posed way, laid when it started) moved onto the two posed
                // places: exact at both steps, and following the blow's real curve between them.
                Vector3 off0 = from - move.PathPoint(move.LastProgress), off1 = to - move.PathPoint(move.Progress);
                float nearest = float.MaxValue;
                Vector3 prev = from;
                for (int i = 1; i <= 12; i++)
                {
                    float s = i / 12f;
                    Vector3 p = move.PathPoint(Mathf.Lerp(move.LastProgress, move.Progress, s)) + Vector3.Lerp(off0, off1, s);
                    nearest = Mathf.Min(nearest, SegmentDistance(move.Aim, prev, p));
                    prev = p;
                }

                return nearest;
            }

            // The planned path between the two steps' progress (it curves), plus however far the posed
            // weapon is off the path (an arm that can't reach).
            float best = float.MaxValue, at = 0f;
            Vector3 last = move.PathPoint(move.LastProgress);
            for (int i = 1; i <= 12; i++)
            {
                Vector3 p = move.PathPoint(Mathf.Lerp(move.LastProgress, move.Progress, i / 12f));
                float d = SegmentDistance(move.Aim, last, p);
                if (d < best)
                {
                    best = d;
                    at = (i - 0.5f) / 12f;
                }

                last = p;
            }

            // (The posed weapon's distance off the path, at the moment of the closest pass: between the two steps'.)
            return best + Mathf.Lerp(move.LastDeviation, Vector3.Distance(to, move.PathPoint(move.Progress)), at);
        }

        /// <summary>The distance from a point to the segment a to b.</summary>
        public static float SegmentDistance(Vector3 point, Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            float t = ab.sqrMagnitude > 1e-10f ? Mathf.Clamp01(Vector3.Dot(point - a, ab) / ab.sqrMagnitude) : 0f;
            return Vector3.Distance(point, a + ab * t);
        }

        /// <summary>A part touched something (hands report every step). Hits come in milestone 3.</summary>
        public void OnPartCollision(BodyPart part, Collision collision, bool entered)
        {
            if (entered)
            {
                ImpactFromCollision(part, collision);
            }

            if (entered && PlaytestTrace.Enabled && collision.collider != null && collision.relativeVelocity.magnitude > 1f)
            {
                PlaytestTrace.Note(this, "contact", $"{part.id} touched {collision.collider.name} at {collision.relativeVelocity.magnitude:0.0} m/s");
            }
        }
    }
}
